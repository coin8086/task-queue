# Using PostgreSQL Container in Test

1. Select and pull a docker image from https://hub.docker.com/_/postgres/, like

   ```
   docker image pull postgres:17.2
   ```

1. Run PostgreSQL in container like

   ```
   docker run --name postgres -e POSTGRES_PASSWORD=*** -p 127.0.0.1:5432:5432 -d postgres:17.2
   ```

1. Set `PgConnectionString` and `PgVersion` in TestSettings.json.

For more details and usage of the PostgreSQL container, see https://hub.docker.com/_/postgres/.
