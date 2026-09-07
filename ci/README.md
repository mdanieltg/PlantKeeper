# ci

Container build and orchestration for PlantKeeper, following the layout used in Wallet.

| File | Purpose |
|---|---|
| `backend.dockerfile` | ASP.NET Core API — `sdk:10.0` build, `aspnet:10.0` runtime |
| `frontend.dockerfile` | Angular app — `node:24-alpine` build, served by `nginx:alpine` |
| `nginx.conf` | SPA fallback routing, and `/api` proxied to the backend |
| `docker-compose.yml` | Development — builds from source, includes PostgreSQL |
| `docker-compose.prod.yml` | Production — prebuilt registry images, external database |
| `.env.example` | Template for `.env`, which is gitignored |
| `create-service-user.sql` | Least-privilege PostgreSQL role for the API, granted per table |

Both dockerfiles are built with the **repository root** as context, so run compose with
the `-f ci/...` paths shown below rather than from inside this directory.

## Development

```bash
cp ci/.env.example ci/.env      # then edit the password
docker compose -f ci/docker-compose.yml up -d --build
```

The app is served at <http://localhost:3000>. The API is not published; the browser
calls `/api/...` on the same origin and nginx proxies it to `backend:8080`. That is also
why no CORS configuration is involved.

PostgreSQL **is** published, but to loopback only: `127.0.0.1:5432:5432`, so Rider or a
`psql` client on this machine can reach it while nothing outside can. Keep the
`127.0.0.1` prefix — a bare `5432:5432` binds every interface, and Docker publishes
past the host firewall. Inside the compose network it is `db:5432`.

### Applying migrations

Nothing applies migrations automatically, so a freshly created database has no schema
and the API will fail on its first query. Run this once after the first `up`, and again
whenever a migration is added:

```bash
docker run --rm -v "$PWD/PlantKeeperAPI:/api:ro" \
  --network plantkeeper_plantkeeper-net \
  -e ASPNETCORE_ENVIRONMENT=Development \
  -e "ConnectionStrings__Dev=Host=db;Port=5432;Database=plants;Username=postgres;Password=YOUR_PASSWORD" \
  mcr.microsoft.com/dotnet/sdk:10.0 sh -c '
    cp -r /api /work && rm -rf /work/src/obj /work/src/bin &&
    dotnet tool install -g dotnet-ef >/dev/null 2>&1 &&
    export PATH="$PATH:/root/.dotnet/tools" &&
    dotnet restore /work/src/PlantKeeperAPI.csproj >/dev/null &&
    dotnet ef database update --project /work/src/PlantKeeperAPI.csproj'
```

Three details in there are not obvious, and each one breaks the command if dropped:

- **It runs inside the compose network**, because `db` is not published to the host.
  `Host=db` only resolves there.
- **The project is copied to `/work` and `obj/` deleted** before restoring. The mount is
  read-only and the host's `obj/project.assets.json` records host NuGet paths that do not
  exist in the container, so restoring against the mount fails and restoring *into* it
  would corrupt your local build.
- **The connection string is passed as `ConnectionStrings__Dev`.** EF resolves the
  DbContext through the application's own service provider, and `AddDatabase` picks the
  configuration key from the environment name — so `ASPNETCORE_ENVIRONMENT=Development`
  and `ConnectionStrings__Dev` have to agree. This is the combination that is actually
  verified; without the environment variable the connection string is null and the
  command fails on connect.

### A least-privilege service account

`create-service-user.sql` creates a `plantkeeper` PostgreSQL role granted table by table
across all 34 application tables, at the narrowest rights each one actually needs. It holds
no DDL rights, and only `SELECT` on `__EFMigrationsHistory`, so the running API can read
what has been applied but cannot change the schema or misreport it. Every table name in
that file is double-quoted on purpose: Postgres folds unquoted identifiers to lower case,
and EF creates them in PascalCase.

```bash
# edit CHANGE_ME first, then:
docker compose -f ci/docker-compose.yml exec -T db psql -U postgres -d plants < ci/create-service-user.sql
```

Then point `CONNECTION_STRING` in `.env` at it (`Username=plantkeeper`) and recreate the
backend. Migrations must keep using an administrative account — the command above
deliberately fails as `plantkeeper`.

Notes on the grants:

- The three payload-free join tables (`PestTreatments`, `SpeciesBeneficialOrganisms`,
  `BeneficialOrganismPests`) get `SELECT, INSERT, DELETE` but **not** `UPDATE`. The link
  endpoints replace a whole set, which EF performs as deletes plus inserts; there is no
  column to update.
- **Identity is nearly read-only.** `AspNetUsers` gets `UPDATE` for lockout counters, the
  sign-out stamp rotation and the bootstrap hash, and `INSERT` on it and `AspNetUserRoles`
  for the first-keeper seed at startup. Nothing else, and no `DELETE` anywhere — removing a
  keeper or revoking a role is an administrative act.
- **`AlmanacChangeProposals` gets no `DELETE`.** The queue is the almanac's history too.
- The verification query is in the header of the SQL file itself. Expect **35 rows** — 34
  tables plus history — split 23 / 3 / 2 / 1 / 6 by privilege set. A `__EFMigrationsHistory`
  row carrying anything beyond `SELECT` means the delimitation has been lost.

> Do not `source` or `set -a; . ci/.env` in a shell. The connection string contains
> semicolons, which the shell reads as command separators — it silently truncates to
> `Server=db` and, because shell variables take precedence over the file, compose then
> starts the backend with no credentials. Let compose read the file itself.

### Session keys

Both compose files mount a `dataprotection-keys` volume at `/keys` and set
`DataProtection__KeyPath` to match. That is where ASP.NET Core keeps the keys that sign and
encrypt the `plantkeeper.session` cookie. Left in the container's own filesystem they go
with every rebuild, and the next request from anyone who was already signed in fails to
decrypt — a deploy signs them out, silently, and only them. Outside `Development` the API
now refuses to start when the setting is missing rather than let that happen quietly.

Two consequences worth knowing:

- **The keys are written unencrypted.** There is no DPAPI on Linux and no certificate
  configured, which the key manager announces at startup: `No XML encryptor configured`.
  Whoever can read the volume can forge a session cookie, so treat it like the database
  password.
- **Deleting the volume signs everyone out**, which is also the only way to do that
  wholesale. `docker compose down -v` takes it with the database.

### Tearing down

```bash
docker compose -f ci/docker-compose.yml down     # keep the data
docker compose -f ci/docker-compose.yml down -v  # drop the volume too
```

## Production

`docker-compose.prod.yml` pulls `localhost:5000/plantkeeper-{backend,frontend}:latest`
from a local registry and joins an existing external `app-network`. It deliberately
contains **no database service** — it points at the shared cluster below, via
`CONNECTION_STRING` in `.env`.

### The shared PostgreSQL

`docker-compose.postgres.yml` runs one `postgres-production` container on `app-network`,
beside the host's existing `mysql-production`. It is a **separate compose project** on
purpose: more than one stack points at that cluster, and none of them should be able to
take it down by running `down` on its own.

Two details in that file are load-bearing:

- **`container_name` as well as the service name.** The containers that connect are in
  other compose projects, so the name they resolve over `app-network` is the container's.
  A service called `postgres` would collide with the first other stack that declares one.
- **The initdb locale is stated, not inherited** — `libc` at `en_US.utf8`, no ICU. A
  nondeterministic ICU collation makes Postgres reject `LIKE`, and EF translates
  `Contains` and `StartsWith` to `LIKE`, so it would break every search in the app.

Nothing is published to the host. Reach it with
`docker exec -it postgres-production psql -U postgres`.

### Standing up production, in order

The order matters twice, and both are easy to get wrong:

```bash
# 1. the shared cluster (once per host)
docker compose -f ci/docker-compose.postgres.yml up -d

# 2. this application's database (once)
docker exec postgres-production psql -U postgres -c 'CREATE DATABASE plants;'

# 3. the schema, as an administrative role, from inside app-network
docker run --rm -v "$PWD/PlantKeeperAPI:/api:ro" --network app-network \
  -e ASPNETCORE_ENVIRONMENT=Production \
  -e "ConnectionStrings__Production=Host=postgres-production;Port=5432;Database=plants;Username=postgres;Password=YOUR_SUPERUSER_PASSWORD" \
  mcr.microsoft.com/dotnet/sdk:10.0 sh -c '
    cp -r /api /work && rm -rf /work/src/obj /work/src/bin &&
    dotnet tool install -g dotnet-ef >/dev/null 2>&1 &&
    export PATH="$PATH:/root/.dotnet/tools" &&
    dotnet restore /work/src/PlantKeeperAPI.csproj >/dev/null &&
    dotnet ef database update --project /work/src/PlantKeeperAPI.csproj'

# 4. the runtime role - AFTER step 3, never before
sed 's/CHANGE_ME/A_REAL_PASSWORD/' ci/create-service-user.sql \
  | docker exec -i postgres-production psql -U postgres -d plants -v ON_ERROR_STOP=1

# 5. point CONNECTION_STRING in ci/.env at that role, then start the app
docker compose -f ci/docker-compose.prod.yml up -d
```

- **Step 4 must follow step 3.** The grant script names tables one at a time, so against a
  database with no schema it stops at `relation "public.Climates" does not exist`.
- **Step 5 must follow step 3.** `SeedFirstKeeperAsync` skips with a warning while
  migrations are pending, so a backend started against an empty database creates no keeper
  — and the bootstrap endpoint then has no account to set a password on. If you did start
  it early, restarting the backend after migrating is enough.
- **The migration command needs the repository on the host**, even though the deployment
  runs from prebuilt images. Check it out, or run step 3 from any machine that can reach
  the cluster.
- **It does not need `DataProtection__KeyPath`.** The app refuses to *serve* without one,
  but that check deliberately sits past `builder.Build()` so `dotnet ef` — which builds the
  whole host to find the DbContext — is unaffected.

Finally, set the first password through `POST /api/authentication/bootstrap-password` with
`BOOTSTRAP_SECRET`, then blank that secret and recreate the backend.

The frontend publishes to `127.0.0.1:5004`, intended to sit behind a reverse proxy on
the host. Adjust the port if it collides with something already deployed.

### The forwarded scheme

Whatever terminates TLS has to send `X-Forwarded-Proto: https`, and `nginx.conf` passes it
through rather than replacing it with its own `$scheme` — which is always `http`, because
that is the hop from the terminator to this container. Get this wrong and the backend
believes every request was insecure, which is not visible in any response the browser shows.

`app.UseHsts()` covers `/api` responses only, since nginx serves the document itself, and it
is excluded for `localhost` by default. **The site's HSTS header should come from the TLS
terminator**, along with the HTTP→HTTPS redirect.
