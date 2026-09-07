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
  `Server=db` only resolves there.
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
contains **no database service** — production is expected to point at a PostgreSQL that
already exists, via `CONNECTION_STRING` in `.env`.

```bash
docker compose -f ci/docker-compose.prod.yml up -d
```

The frontend publishes to `127.0.0.1:5004`, intended to sit behind a reverse proxy on
the host. Adjust the port if it collides with something already deployed.
