-- ---------------------------------------------------------------------------
-- Least-privilege PostgreSQL role for PlantKeeperAPI.
--
-- The service does data work only. Schema changes are applied separately, by
-- `dotnet ef database update` running as an administrative role -- see
-- ci/README.md. That separation is the point of this script: with these grants
-- the running API cannot alter the schema, and cannot touch the migration
-- history table to misreport it.
--
-- Two things differ from the MySQL original this replaced. Postgres folds
-- unquoted identifiers to lower case, and EF creates tables in PascalCase, so
-- every table name here is double-quoted -- drop the quotes and the grant
-- silently lands on a table that does not exist. And there is no
-- FLUSH PRIVILEGES; grants take effect immediately.
--
-- Before running:
--   1. Replace CHANGE_ME with a real password.
--   2. No host part to configure. Postgres roles are not host-scoped the way
--      MySQL accounts are; reachability is decided by the network and
--      pg_hba.conf, not by the role.
--
-- Run as the postgres superuser:
--   docker compose -f ci/docker-compose.yml exec -T db \
--     psql -U postgres -d plants < ci/create-service-user.sql
-- ---------------------------------------------------------------------------

CREATE ROLE plantkeeper LOGIN PASSWORD 'CHANGE_ME';

GRANT CONNECT ON DATABASE plants TO plantkeeper;
GRANT USAGE ON SCHEMA public TO plantkeeper;

-- Postgres 15 and later already withhold CREATE on `public` from PUBLIC. Stated
-- explicitly so the intent survives a move to an older server.
REVOKE CREATE ON SCHEMA public FROM plantkeeper;

-- Reference data -- lookups the rest of the schema hangs off.
GRANT SELECT, INSERT, UPDATE, DELETE ON TABLE public."Climates"           TO plantkeeper;
GRANT SELECT, INSERT, UPDATE, DELETE ON TABLE public."PottingMixes"       TO plantkeeper;
GRANT SELECT, INSERT, UPDATE, DELETE ON TABLE public."WateringMethods"    TO plantkeeper;
GRANT SELECT, INSERT, UPDATE, DELETE ON TABLE public."Fertilizers"        TO plantkeeper;
GRANT SELECT, INSERT, UPDATE, DELETE ON TABLE public."Treatments"         TO plantkeeper;
GRANT SELECT, INSERT, UPDATE, DELETE ON TABLE public."PropagationMethods" TO plantkeeper;

-- Species and its 1:1 profiles. The profiles are written as part of the species
-- aggregate, so they need the same rights as the parent.
GRANT SELECT, INSERT, UPDATE, DELETE ON TABLE public."PlantSpecies"             TO plantkeeper;
GRANT SELECT, INSERT, UPDATE, DELETE ON TABLE public."SpeciesCareProfiles"      TO plantkeeper;
GRANT SELECT, INSERT, UPDATE, DELETE ON TABLE public."SpeciesToxicityProfiles"  TO plantkeeper;
GRANT SELECT, INSERT, UPDATE, DELETE ON TABLE public."SpeciesFloweringProfiles" TO plantkeeper;

-- Recommendation matrices -- explicit join entities carrying a rating and notes.
GRANT SELECT, INSERT, UPDATE, DELETE ON TABLE public."SpeciesFertilizerRecommendations" TO plantkeeper;
GRANT SELECT, INSERT, UPDATE, DELETE ON TABLE public."SpeciesTreatmentRecommendations"  TO plantkeeper;
GRANT SELECT, INSERT, UPDATE, DELETE ON TABLE public."SpeciesPropagationMethods"        TO plantkeeper;

-- Plant instances and their logs.
GRANT SELECT, INSERT, UPDATE, DELETE ON TABLE public."Plants"            TO plantkeeper;
GRANT SELECT, INSERT, UPDATE, DELETE ON TABLE public."WateringLogs"      TO plantkeeper;
GRANT SELECT, INSERT, UPDATE, DELETE ON TABLE public."FertilizationLogs" TO plantkeeper;
GRANT SELECT, INSERT, UPDATE, DELETE ON TABLE public."TreatmentLogs"     TO plantkeeper;
GRANT SELECT, INSERT, UPDATE, DELETE ON TABLE public."RepottingLogs"     TO plantkeeper;
GRANT SELECT, INSERT, UPDATE, DELETE ON TABLE public."ObservationLogs"   TO plantkeeper;
GRANT SELECT, INSERT, UPDATE, DELETE ON TABLE public."GrowthLogs"        TO plantkeeper;

-- Ecosystem tables.
GRANT SELECT, INSERT, UPDATE, DELETE ON TABLE public."Pests"              TO plantkeeper;
GRANT SELECT, INSERT, UPDATE, DELETE ON TABLE public."BeneficialOrganisms" TO plantkeeper;
GRANT SELECT, INSERT, UPDATE, DELETE ON TABLE public."PropagationBatches"  TO plantkeeper;

-- Payload-free join tables. No UPDATE: these rows are only ever inserted or
-- removed. The link endpoints replace a whole set, which EF performs as a
-- delete followed by inserts -- there is no column to update.
GRANT SELECT, INSERT, DELETE ON TABLE public."PestTreatments"             TO plantkeeper;
GRANT SELECT, INSERT, DELETE ON TABLE public."SpeciesBeneficialOrganisms" TO plantkeeper;
GRANT SELECT, INSERT, DELETE ON TABLE public."BeneficialOrganismPests"    TO plantkeeper;

-- Deliberately NOT granted:
--   public."__EFMigrationsHistory" -- migrations run as an admin role, and the
--                                     service must not be able to rewrite what the
--                                     database believes it has applied.
--   CREATE on schema public        -- no DDL at runtime.
--   ALTER DEFAULT PRIVILEGES       -- a future table is not granted by accident;
--                                     adding one means adding a line here.
--   Sequences                      -- every key is a client-generated uuid, so the
--                                     schema has none. If that ever changes, the
--                                     new sequence needs an explicit USAGE grant.

-- ---------------------------------------------------------------------------
-- Verify:
--   SELECT table_name, string_agg(privilege_type, ', ' ORDER BY privilege_type)
--   FROM information_schema.role_table_grants
--   WHERE grantee = 'plantkeeper'
--   GROUP BY table_name ORDER BY table_name;
--
-- Expect 26 rows: 23 with SELECT, INSERT, UPDATE, DELETE and the three join
-- tables with SELECT, INSERT, DELETE. A `__EFMigrationsHistory` row would mean
-- the delimitation has been lost.
--
-- Confirm DDL is refused:
--   \c plants plantkeeper
--   CREATE TABLE should_fail (id int);   -- expect: permission denied for schema public
-- ---------------------------------------------------------------------------

-- ---------------------------------------------------------------------------
-- To start over:
--   REASSIGN OWNED BY plantkeeper TO postgres;   -- no-op unless it owns objects
--   DROP OWNED BY plantkeeper;                   -- drops the grants above
--   DROP ROLE IF EXISTS plantkeeper;
-- ---------------------------------------------------------------------------
