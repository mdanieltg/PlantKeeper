-- ---------------------------------------------------------------------------
-- Least-privilege PostgreSQL role for PlantKeeperAPI.
--
-- The service does data work only. Schema changes are applied separately, by
-- `dotnet ef database update` running as an administrative role -- see
-- ci/README.md. That separation is the point of this script: with these grants
-- the running API cannot alter the schema, and cannot touch the migration
-- history table to misreport it.
--
-- Postgres folds unquoted identifiers to lower case, and EF creates tables in
-- PascalCase, so every table name here is double-quoted -- drop the quotes and
-- the grant silently lands on a table that does not exist. Grants take effect
-- immediately; nothing needs reloading afterwards.
--
-- Before running:
--   1. Replace CHANGE_ME with a real password.
--   2. There is no host part to configure. Postgres roles are not host-scoped;
--      reachability is decided by the network and pg_hba.conf, not by the role.
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

-- Identity. The API signs keepers in and rebuilds the principal from the
-- database on every request (SecurityStampValidatorOptions.ValidationInterval is
-- Zero), so it reads all seven of these constantly. The writes are narrower than
-- they look, and each one has exactly one caller.
--
--   AspNetUsers      UPDATE  lockout counters on a failed sign-in, the security
--                            stamp rotation on sign-out, and the hash the
--                            bootstrap endpoint sets. Without it sign-in fails.
--                    INSERT  SeedFirstKeeperAsync creates the first keeper at
--                            boot when the table is empty. A fresh deployment
--                            has no keeper without this.
--   AspNetUserRoles  INSERT  the same seed assigns that keeper its three roles.
--
-- Neither gets DELETE. Removing a keeper or revoking a role is an administrative
-- act, done with an admin connection, not something a request should be able to
-- do -- and there is no endpoint for either.
GRANT SELECT, INSERT, UPDATE ON TABLE public."AspNetUsers"      TO plantkeeper;
GRANT SELECT, INSERT         ON TABLE public."AspNetUserRoles"  TO plantkeeper;

-- Read-only: seeded by the AddIdentity migration and never written at runtime.
-- AspNetRoleClaims is where the permission claims live, so it is read on every
-- request that authorizes anything.
GRANT SELECT ON TABLE public."AspNetRoles"      TO plantkeeper;
GRANT SELECT ON TABLE public."AspNetRoleClaims" TO plantkeeper;
GRANT SELECT ON TABLE public."AspNetUserClaims" TO plantkeeper;

-- Nothing in the API touches these two today -- there are no external login
-- providers and no two-factor -- but Identity's stores are registered against
-- them, so a code path that reaches one should come back empty rather than
-- raise a permission error. Read-only until a feature actually needs to write.
GRANT SELECT ON TABLE public."AspNetUserLogins" TO plantkeeper;
GRANT SELECT ON TABLE public."AspNetUserTokens" TO plantkeeper;

-- The almanac's proposal queue, and its history. UPDATE is the decision being
-- recorded on a pending row. No DELETE: a proposal that was applied, rejected or
-- refused as stale is the record of what happened to the almanac, so nothing
-- removes one.
GRANT SELECT, INSERT, UPDATE ON TABLE public."AlmanacChangeProposals" TO plantkeeper;

-- SELECT, and only SELECT, on the migration history. The API reads it at boot:
-- SeedFirstKeeperAsync calls GetPendingMigrationsAsync() and skips the seed when
-- migrations are outstanding, so with no grant at all the application throws
-- `permission denied for table __EFMigrationsHistory` before it finishes
-- starting. Reading what has been applied is harmless; the point of withholding
-- the rest stands -- the service still cannot rewrite what the database believes
-- it has applied.
GRANT SELECT ON TABLE public."__EFMigrationsHistory" TO plantkeeper;

-- Deliberately NOT granted:
--   INSERT/UPDATE/DELETE on
--   public."__EFMigrationsHistory" -- migrations run as an admin role, and the
--                                     service must not be able to rewrite what the
--                                     database believes it has applied. SELECT is
--                                     granted above, because the app reads it at boot.
--   DELETE on the Identity tables  -- removing a keeper or revoking a role is an
--                                     administrative act; no endpoint does either.
--   DELETE on
--   public."AlmanacChangeProposals" -- the queue doubles as the almanac's history.
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
-- Expect 35 rows, over the 34 application tables plus the history table:
--
--   23  SELECT, INSERT, UPDATE, DELETE   the entity tables
--    3  SELECT, INSERT, DELETE           the payload-free join tables
--    2  SELECT, INSERT, UPDATE           AspNetUsers, AlmanacChangeProposals
--    1  SELECT, INSERT                   AspNetUserRoles
--    6  SELECT                           the five remaining Identity tables and
--                                        __EFMigrationsHistory
--
-- A `__EFMigrationsHistory` row carrying anything beyond SELECT would mean the
-- delimitation has been lost.
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
