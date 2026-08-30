-- ---------------------------------------------------------------------------
-- Least-privilege MySQL account for PlantKeeperAPI.
--
-- The service does data work only. Schema changes are applied separately, by
-- `dotnet ef database update` running as an administrative account -- see
-- ci/README.md. That separation is the point of this script: with these grants
-- the running API cannot alter the schema, and cannot touch the migration
-- history table to misreport it.
--
-- Before running:
--   1. Replace CHANGE_ME with a real password.
--   2. Check the host part. '%' is what the Docker Compose stack needs, since
--      the API connects from another container; the database is not published
--      to the host, so '%' stays confined to the compose network. For a MySQL
--      running directly on your machine, use the 'localhost' variant at the
--      bottom instead.
--
-- Run as root:
--   docker compose -f ci/docker-compose.yml exec -T db \
--     mysql -uroot -p < ci/create-service-user.sql
-- ---------------------------------------------------------------------------

CREATE USER IF NOT EXISTS 'plantkeeper'@'%' IDENTIFIED BY 'CHANGE_ME';

-- Reference data -- lookups the rest of the schema hangs off.
GRANT SELECT, INSERT, UPDATE, DELETE ON plants.Climates           TO 'plantkeeper'@'%';
GRANT SELECT, INSERT, UPDATE, DELETE ON plants.PottingMixes       TO 'plantkeeper'@'%';
GRANT SELECT, INSERT, UPDATE, DELETE ON plants.WateringMethods    TO 'plantkeeper'@'%';
GRANT SELECT, INSERT, UPDATE, DELETE ON plants.Fertilizers        TO 'plantkeeper'@'%';
GRANT SELECT, INSERT, UPDATE, DELETE ON plants.Treatments         TO 'plantkeeper'@'%';
GRANT SELECT, INSERT, UPDATE, DELETE ON plants.PropagationMethods TO 'plantkeeper'@'%';

-- Species and its 1:1 profiles. The profiles are written as part of the species
-- aggregate, so they need the same rights as the parent.
GRANT SELECT, INSERT, UPDATE, DELETE ON plants.PlantSpecies             TO 'plantkeeper'@'%';
GRANT SELECT, INSERT, UPDATE, DELETE ON plants.SpeciesCareProfiles      TO 'plantkeeper'@'%';
GRANT SELECT, INSERT, UPDATE, DELETE ON plants.SpeciesToxicityProfiles  TO 'plantkeeper'@'%';
GRANT SELECT, INSERT, UPDATE, DELETE ON plants.SpeciesFloweringProfiles TO 'plantkeeper'@'%';

-- Recommendation matrices -- explicit join entities carrying a rating and notes.
GRANT SELECT, INSERT, UPDATE, DELETE ON plants.SpeciesFertilizerRecommendations TO 'plantkeeper'@'%';
GRANT SELECT, INSERT, UPDATE, DELETE ON plants.SpeciesTreatmentRecommendations  TO 'plantkeeper'@'%';
GRANT SELECT, INSERT, UPDATE, DELETE ON plants.SpeciesPropagationMethods        TO 'plantkeeper'@'%';

-- Plant instances and their logs.
GRANT SELECT, INSERT, UPDATE, DELETE ON plants.Plants            TO 'plantkeeper'@'%';
GRANT SELECT, INSERT, UPDATE, DELETE ON plants.WateringLogs      TO 'plantkeeper'@'%';
GRANT SELECT, INSERT, UPDATE, DELETE ON plants.FertilizationLogs TO 'plantkeeper'@'%';
GRANT SELECT, INSERT, UPDATE, DELETE ON plants.TreatmentLogs     TO 'plantkeeper'@'%';
GRANT SELECT, INSERT, UPDATE, DELETE ON plants.RepottingLogs     TO 'plantkeeper'@'%';
GRANT SELECT, INSERT, UPDATE, DELETE ON plants.ObservationLogs   TO 'plantkeeper'@'%';
GRANT SELECT, INSERT, UPDATE, DELETE ON plants.GrowthLogs        TO 'plantkeeper'@'%';

-- Ecosystem tables.
GRANT SELECT, INSERT, UPDATE, DELETE ON plants.Pests              TO 'plantkeeper'@'%';
GRANT SELECT, INSERT, UPDATE, DELETE ON plants.BeneficialOrganisms TO 'plantkeeper'@'%';
GRANT SELECT, INSERT, UPDATE, DELETE ON plants.PropagationBatches  TO 'plantkeeper'@'%';

-- Payload-free join tables. No UPDATE: these rows are only ever inserted or
-- removed. The link endpoints replace a whole set, which EF performs as a
-- delete followed by inserts -- there is no column to update.
GRANT SELECT, INSERT, DELETE ON plants.PestTreatments            TO 'plantkeeper'@'%';
GRANT SELECT, INSERT, DELETE ON plants.SpeciesBeneficialOrganisms TO 'plantkeeper'@'%';
GRANT SELECT, INSERT, DELETE ON plants.BeneficialOrganismPests    TO 'plantkeeper'@'%';

-- Deliberately NOT granted:
--   plants.__EFMigrationsHistory  -- migrations run as an admin account, and the
--                                    service must not be able to rewrite what the
--                                    database believes it has applied.
--   CREATE / ALTER / DROP / INDEX -- no DDL at runtime.
--   Anything outside the `plants` schema.

FLUSH PRIVILEGES;

-- Verify:
--   SHOW GRANTS FOR 'plantkeeper'@'%';
--
-- Expect 26 table-level grants and no schema-level (plants.*) grant. A
-- `GRANT ... ON plants.*` line would mean the delimitation has been lost.

-- ---------------------------------------------------------------------------
-- Variant for a MySQL running directly on the host rather than in Compose.
-- Same grants, host part 'localhost'. Uncomment and run instead of the above.
-- ---------------------------------------------------------------------------
-- CREATE USER IF NOT EXISTS 'plantkeeper'@'localhost' IDENTIFIED BY 'CHANGE_ME';
-- ... repeat each GRANT with @'localhost' ...

-- ---------------------------------------------------------------------------
-- To start over:
--   DROP USER IF EXISTS 'plantkeeper'@'%';
-- ---------------------------------------------------------------------------
