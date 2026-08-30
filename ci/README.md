# ci

Container build and orchestration for PlantKeeper, following the layout used in Wallet.

| File | Purpose |
|---|---|
| `backend.dockerfile` | ASP.NET Core API — `sdk:10.0` build, `aspnet:10.0` runtime |
| `frontend.dockerfile` | Angular app — `node:24-alpine` build, served by `nginx:alpine` |
| `nginx.conf` | SPA fallback routing, and `/api` proxied to the backend |
| `docker-compose.yml` | Development — builds from source, includes MySQL |
| `docker-compose.prod.yml` | Production — prebuilt registry images, external database |
| `.env.example` | Template for `.env`, which is gitignored |
| `create-service-user.sql` | Least-privilege MySQL account for the API, granted per table |

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

MySQL is **not** published to the host either — it is reachable only as `db:3306` inside
the compose network. If you want Rider or a `mysql` client to reach it, add a ports
mapping to the `db` service.

### Applying migrations

Nothing applies migrations automatically, so a freshly created database has no schema
and the API will fail on its first query. Run this once after the first `up`, and again
whenever a migration is added:

```bash
docker run --rm -v "$PWD/PlantKeeperAPI:/api:ro" \
  --network plantkeeper_plantkeeper-net \
  -e ASPNETCORE_ENVIRONMENT=Development \
  -e "ConnectionStrings__Dev=Server=db;Port=3306;Database=plants;Uid=root;Pwd=YOUR_PASSWORD" \
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
- **The connection string is passed as `ConnectionStrings__Dev`, not `--connection`.**
  EF resolves the DbContext through the application's own service provider, and
  `AddDatabase` calls `ServerVersion.AutoDetect` while registering services — before
  `--connection` would ever be applied. Without the environment variable the command
  fails with "Unable to connect to any of the specified MySQL hosts".

### A least-privilege service account

`create-service-user.sql` creates a `plantkeeper` MySQL user granted only
`SELECT, INSERT, UPDATE, DELETE`, table by table, across the 26 application tables. It
holds no DDL rights and no access to `__EFMigrationsHistory`, so the running API cannot
change the schema or misreport what has been migrated.

```bash
# edit CHANGE_ME first, then:
docker compose -f ci/docker-compose.yml exec -T db mysql -uroot -p < ci/create-service-user.sql
```

Then point `CONNECTION_STRING` in `.env` at it (`Uid=plantkeeper`) and recreate the
backend. Migrations must keep using an administrative account — the command above
deliberately fails as `plantkeeper`.

Two notes on the grants:

- The three payload-free join tables (`PestTreatments`, `SpeciesBeneficialOrganisms`,
  `BeneficialOrganismPests`) get `SELECT, INSERT, DELETE` but **not** `UPDATE`. The link
  endpoints replace a whole set, which EF performs as deletes plus inserts; there is no
  column to update.
- `SHOW GRANTS FOR 'plantkeeper'@'%'` should list 26 table grants plus `USAGE ON *.*`.
  A `GRANT ... ON plants.*` line would mean the per-table delimitation has been lost.

> Do not `source` or `set -a; . ci/.env` in a shell. The connection string contains
> semicolons, which the shell reads as command separators — it silently truncates to
> `Server=db` and, because shell variables take precedence over the file, compose then
> starts the backend with no credentials. Let compose read the file itself.

### Tearing down

```bash
docker compose -f ci/docker-compose.yml down     # keep the data
docker compose -f ci/docker-compose.yml down -v  # drop the volume too
```

## Production

`docker-compose.prod.yml` pulls `localhost:5000/plantkeeper-{backend,frontend}:latest`
from a local registry and joins an existing external `app-network`. It deliberately
contains **no database service** — production is expected to point at a MySQL that
already exists, via `CONNECTION_STRING` in `.env`.

```bash
docker compose -f ci/docker-compose.prod.yml up -d
```

The frontend publishes to `127.0.0.1:5004`, intended to sit behind a reverse proxy on
the host. Adjust the port if it collides with something already deployed.
