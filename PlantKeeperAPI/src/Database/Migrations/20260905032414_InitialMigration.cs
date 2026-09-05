using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PlantKeeperAPI.Database.Migrations
{
    /// <inheritdoc />
    public partial class InitialMigration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "BeneficialOrganisms",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    ScientificName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    Role = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    SuggestedPlants = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    Notes = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BeneficialOrganisms", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Climates",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Temperature = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Precipitation = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Humidity = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Sun = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Wind = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Description = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Climates", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Fertilizers",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    Description = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    NpkRatio = table.Column<string>(type: "character varying(15)", maxLength: 15, nullable: true),
                    Category = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Fertilizers", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Pests",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Description = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    Status = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    Notes = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Pests", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PottingMixes",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    Description = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PottingMixes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PropagationMethods",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Description = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PropagationMethods", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Treatments",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    Description = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Treatments", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "WateringMethods",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    Description = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WateringMethods", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "BeneficialOrganismPests",
                columns: table => new
                {
                    ControlledById = table.Column<Guid>(type: "uuid", nullable: false),
                    PestsControlledId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BeneficialOrganismPests", x => new { x.ControlledById, x.PestsControlledId });
                    table.ForeignKey(
                        name: "FK_BeneficialOrganismPests_BeneficialOrganisms_ControlledById",
                        column: x => x.ControlledById,
                        principalTable: "BeneficialOrganisms",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_BeneficialOrganismPests_Pests_PestsControlledId",
                        column: x => x.PestsControlledId,
                        principalTable: "Pests",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PlantSpecies",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ScientificName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Name = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    NameInEnglish = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    ClimateId = table.Column<Guid>(type: "uuid", nullable: false),
                    PottingMixId = table.Column<Guid>(type: "uuid", nullable: false),
                    FloweringHabit = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    FertilizationFrequency = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    FertilizationNotes = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    Comments = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PlantSpecies", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PlantSpecies_Climates_ClimateId",
                        column: x => x.ClimateId,
                        principalTable: "Climates",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PlantSpecies_PottingMixes_PottingMixId",
                        column: x => x.PottingMixId,
                        principalTable: "PottingMixes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PestTreatments",
                columns: table => new
                {
                    PestsId = table.Column<Guid>(type: "uuid", nullable: false),
                    TreatmentsId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PestTreatments", x => new { x.PestsId, x.TreatmentsId });
                    table.ForeignKey(
                        name: "FK_PestTreatments_Pests_PestsId",
                        column: x => x.PestsId,
                        principalTable: "Pests",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PestTreatments_Treatments_TreatmentsId",
                        column: x => x.TreatmentsId,
                        principalTable: "Treatments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Plants",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Alias = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    SpeciesId = table.Column<Guid>(type: "uuid", nullable: false),
                    Comments = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Plants", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Plants_PlantSpecies_SpeciesId",
                        column: x => x.SpeciesId,
                        principalTable: "PlantSpecies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SpeciesBeneficialOrganisms",
                columns: table => new
                {
                    BeneficialOrganismsId = table.Column<Guid>(type: "uuid", nullable: false),
                    SupportingSpeciesId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SpeciesBeneficialOrganisms", x => new { x.BeneficialOrganismsId, x.SupportingSpeciesId });
                    table.ForeignKey(
                        name: "FK_SpeciesBeneficialOrganisms_BeneficialOrganisms_BeneficialOr~",
                        column: x => x.BeneficialOrganismsId,
                        principalTable: "BeneficialOrganisms",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_SpeciesBeneficialOrganisms_PlantSpecies_SupportingSpeciesId",
                        column: x => x.SupportingSpeciesId,
                        principalTable: "PlantSpecies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SpeciesCareProfiles",
                columns: table => new
                {
                    SpeciesId = table.Column<Guid>(type: "uuid", nullable: false),
                    LightMin = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    LightMax = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    LightNotes = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    MinTemperatureCelsius = table.Column<int>(type: "integer", nullable: false),
                    MaxTemperatureCelsius = table.Column<int>(type: "integer", nullable: false),
                    WateringRequirement = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    SoilPhMin = table.Column<decimal>(type: "numeric(3,1)", precision: 3, scale: 1, nullable: false),
                    SoilPhMax = table.Column<decimal>(type: "numeric(3,1)", precision: 3, scale: 1, nullable: false),
                    SoilPhNotes = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    WindTolerance = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    WindToleranceNotes = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SpeciesCareProfiles", x => x.SpeciesId);
                    table.ForeignKey(
                        name: "FK_SpeciesCareProfiles_PlantSpecies_SpeciesId",
                        column: x => x.SpeciesId,
                        principalTable: "PlantSpecies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SpeciesFertilizerRecommendations",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    SpeciesId = table.Column<Guid>(type: "uuid", nullable: false),
                    Category = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    Suitability = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    Notes = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SpeciesFertilizerRecommendations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SpeciesFertilizerRecommendations_PlantSpecies_SpeciesId",
                        column: x => x.SpeciesId,
                        principalTable: "PlantSpecies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SpeciesFloweringProfiles",
                columns: table => new
                {
                    SpeciesId = table.Column<Guid>(type: "uuid", nullable: false),
                    BloomSeason = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    BloomCareNotes = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    SeedViability = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    SeedHarvestTiming = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SpeciesFloweringProfiles", x => x.SpeciesId);
                    table.ForeignKey(
                        name: "FK_SpeciesFloweringProfiles_PlantSpecies_SpeciesId",
                        column: x => x.SpeciesId,
                        principalTable: "PlantSpecies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SpeciesPropagationMethods",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    SpeciesId = table.Column<Guid>(type: "uuid", nullable: false),
                    PropagationMethodId = table.Column<Guid>(type: "uuid", nullable: false),
                    IsPrimary = table.Column<bool>(type: "boolean", nullable: false),
                    RootingHormone = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    BestSeason = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    Difficulty = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: true),
                    Notes = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SpeciesPropagationMethods", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SpeciesPropagationMethods_PlantSpecies_SpeciesId",
                        column: x => x.SpeciesId,
                        principalTable: "PlantSpecies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_SpeciesPropagationMethods_PropagationMethods_PropagationMet~",
                        column: x => x.PropagationMethodId,
                        principalTable: "PropagationMethods",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SpeciesToxicityProfiles",
                columns: table => new
                {
                    SpeciesId = table.Column<Guid>(type: "uuid", nullable: false),
                    ToHumans = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    ToHumansNotes = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: true),
                    ToPets = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    ToPetsNotes = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SpeciesToxicityProfiles", x => x.SpeciesId);
                    table.ForeignKey(
                        name: "FK_SpeciesToxicityProfiles_PlantSpecies_SpeciesId",
                        column: x => x.SpeciesId,
                        principalTable: "PlantSpecies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SpeciesTreatmentRecommendations",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    SpeciesId = table.Column<Guid>(type: "uuid", nullable: false),
                    TreatmentId = table.Column<Guid>(type: "uuid", nullable: false),
                    Safety = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    Notes = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SpeciesTreatmentRecommendations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SpeciesTreatmentRecommendations_PlantSpecies_SpeciesId",
                        column: x => x.SpeciesId,
                        principalTable: "PlantSpecies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_SpeciesTreatmentRecommendations_Treatments_TreatmentId",
                        column: x => x.TreatmentId,
                        principalTable: "Treatments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "FertilizationLogs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    PlantId = table.Column<Guid>(type: "uuid", nullable: false),
                    FertilizerId = table.Column<Guid>(type: "uuid", nullable: false),
                    Date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Dose = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    Comments = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FertilizationLogs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FertilizationLogs_Fertilizers_FertilizerId",
                        column: x => x.FertilizerId,
                        principalTable: "Fertilizers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_FertilizationLogs_Plants_PlantId",
                        column: x => x.PlantId,
                        principalTable: "Plants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "GrowthLogs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    PlantId = table.Column<Guid>(type: "uuid", nullable: false),
                    Date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    HeightCm = table.Column<decimal>(type: "numeric(6,1)", precision: 6, scale: 1, nullable: true),
                    HeightToLastNodeCm = table.Column<decimal>(type: "numeric(6,1)", precision: 6, scale: 1, nullable: true),
                    Notes = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GrowthLogs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_GrowthLogs_Plants_PlantId",
                        column: x => x.PlantId,
                        principalTable: "Plants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ObservationLogs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    PlantId = table.Column<Guid>(type: "uuid", nullable: false),
                    Date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Notes = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ObservationLogs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ObservationLogs_Plants_PlantId",
                        column: x => x.PlantId,
                        principalTable: "Plants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PropagationBatches",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    SpeciesId = table.Column<Guid>(type: "uuid", nullable: false),
                    SourcePlantId = table.Column<Guid>(type: "uuid", nullable: true),
                    PropagationMethodId = table.Column<Guid>(type: "uuid", nullable: true),
                    StartDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Count = table.Column<int>(type: "integer", nullable: false),
                    Medium = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    RootingHormone = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    Status = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    TargetTransplantWindow = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    Notes = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PropagationBatches", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PropagationBatches_PlantSpecies_SpeciesId",
                        column: x => x.SpeciesId,
                        principalTable: "PlantSpecies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PropagationBatches_Plants_SourcePlantId",
                        column: x => x.SourcePlantId,
                        principalTable: "Plants",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_PropagationBatches_PropagationMethods_PropagationMethodId",
                        column: x => x.PropagationMethodId,
                        principalTable: "PropagationMethods",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "RepottingLogs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    PlantId = table.Column<Guid>(type: "uuid", nullable: false),
                    Date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Dimensions = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Volume = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    Material = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Comments = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RepottingLogs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RepottingLogs_Plants_PlantId",
                        column: x => x.PlantId,
                        principalTable: "Plants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TreatmentLogs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    PlantId = table.Column<Guid>(type: "uuid", nullable: false),
                    TreatmentId = table.Column<Guid>(type: "uuid", nullable: false),
                    Date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Comments = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TreatmentLogs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TreatmentLogs_Plants_PlantId",
                        column: x => x.PlantId,
                        principalTable: "Plants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_TreatmentLogs_Treatments_TreatmentId",
                        column: x => x.TreatmentId,
                        principalTable: "Treatments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "WateringLogs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    PlantId = table.Column<Guid>(type: "uuid", nullable: false),
                    MethodId = table.Column<Guid>(type: "uuid", nullable: false),
                    Date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Comments = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WateringLogs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_WateringLogs_Plants_PlantId",
                        column: x => x.PlantId,
                        principalTable: "Plants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_WateringLogs_WateringMethods_MethodId",
                        column: x => x.MethodId,
                        principalTable: "WateringMethods",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_BeneficialOrganismPests_PestsControlledId",
                table: "BeneficialOrganismPests",
                column: "PestsControlledId");

            migrationBuilder.CreateIndex(
                name: "IX_FertilizationLogs_FertilizerId",
                table: "FertilizationLogs",
                column: "FertilizerId");

            migrationBuilder.CreateIndex(
                name: "IX_FertilizationLogs_PlantId",
                table: "FertilizationLogs",
                column: "PlantId");

            migrationBuilder.CreateIndex(
                name: "IX_GrowthLogs_PlantId",
                table: "GrowthLogs",
                column: "PlantId");

            migrationBuilder.CreateIndex(
                name: "IX_ObservationLogs_PlantId",
                table: "ObservationLogs",
                column: "PlantId");

            migrationBuilder.CreateIndex(
                name: "IX_PestTreatments_TreatmentsId",
                table: "PestTreatments",
                column: "TreatmentsId");

            migrationBuilder.CreateIndex(
                name: "IX_Plants_SpeciesId",
                table: "Plants",
                column: "SpeciesId");

            migrationBuilder.CreateIndex(
                name: "IX_PlantSpecies_ClimateId",
                table: "PlantSpecies",
                column: "ClimateId");

            migrationBuilder.CreateIndex(
                name: "IX_PlantSpecies_PottingMixId",
                table: "PlantSpecies",
                column: "PottingMixId");

            migrationBuilder.CreateIndex(
                name: "IX_PropagationBatches_PropagationMethodId",
                table: "PropagationBatches",
                column: "PropagationMethodId");

            migrationBuilder.CreateIndex(
                name: "IX_PropagationBatches_SourcePlantId",
                table: "PropagationBatches",
                column: "SourcePlantId");

            migrationBuilder.CreateIndex(
                name: "IX_PropagationBatches_SpeciesId",
                table: "PropagationBatches",
                column: "SpeciesId");

            migrationBuilder.CreateIndex(
                name: "IX_RepottingLogs_PlantId",
                table: "RepottingLogs",
                column: "PlantId");

            migrationBuilder.CreateIndex(
                name: "IX_SpeciesBeneficialOrganisms_SupportingSpeciesId",
                table: "SpeciesBeneficialOrganisms",
                column: "SupportingSpeciesId");

            migrationBuilder.CreateIndex(
                name: "IX_SpeciesFertilizerRecommendations_SpeciesId_Category",
                table: "SpeciesFertilizerRecommendations",
                columns: new[] { "SpeciesId", "Category" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SpeciesPropagationMethods_PropagationMethodId",
                table: "SpeciesPropagationMethods",
                column: "PropagationMethodId");

            migrationBuilder.CreateIndex(
                name: "IX_SpeciesPropagationMethods_SpeciesId_PropagationMethodId",
                table: "SpeciesPropagationMethods",
                columns: new[] { "SpeciesId", "PropagationMethodId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SpeciesTreatmentRecommendations_SpeciesId_TreatmentId",
                table: "SpeciesTreatmentRecommendations",
                columns: new[] { "SpeciesId", "TreatmentId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SpeciesTreatmentRecommendations_TreatmentId",
                table: "SpeciesTreatmentRecommendations",
                column: "TreatmentId");

            migrationBuilder.CreateIndex(
                name: "IX_TreatmentLogs_PlantId",
                table: "TreatmentLogs",
                column: "PlantId");

            migrationBuilder.CreateIndex(
                name: "IX_TreatmentLogs_TreatmentId",
                table: "TreatmentLogs",
                column: "TreatmentId");

            migrationBuilder.CreateIndex(
                name: "IX_WateringLogs_MethodId",
                table: "WateringLogs",
                column: "MethodId");

            migrationBuilder.CreateIndex(
                name: "IX_WateringLogs_PlantId",
                table: "WateringLogs",
                column: "PlantId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "BeneficialOrganismPests");

            migrationBuilder.DropTable(
                name: "FertilizationLogs");

            migrationBuilder.DropTable(
                name: "GrowthLogs");

            migrationBuilder.DropTable(
                name: "ObservationLogs");

            migrationBuilder.DropTable(
                name: "PestTreatments");

            migrationBuilder.DropTable(
                name: "PropagationBatches");

            migrationBuilder.DropTable(
                name: "RepottingLogs");

            migrationBuilder.DropTable(
                name: "SpeciesBeneficialOrganisms");

            migrationBuilder.DropTable(
                name: "SpeciesCareProfiles");

            migrationBuilder.DropTable(
                name: "SpeciesFertilizerRecommendations");

            migrationBuilder.DropTable(
                name: "SpeciesFloweringProfiles");

            migrationBuilder.DropTable(
                name: "SpeciesPropagationMethods");

            migrationBuilder.DropTable(
                name: "SpeciesToxicityProfiles");

            migrationBuilder.DropTable(
                name: "SpeciesTreatmentRecommendations");

            migrationBuilder.DropTable(
                name: "TreatmentLogs");

            migrationBuilder.DropTable(
                name: "WateringLogs");

            migrationBuilder.DropTable(
                name: "Fertilizers");

            migrationBuilder.DropTable(
                name: "Pests");

            migrationBuilder.DropTable(
                name: "BeneficialOrganisms");

            migrationBuilder.DropTable(
                name: "PropagationMethods");

            migrationBuilder.DropTable(
                name: "Treatments");

            migrationBuilder.DropTable(
                name: "Plants");

            migrationBuilder.DropTable(
                name: "WateringMethods");

            migrationBuilder.DropTable(
                name: "PlantSpecies");

            migrationBuilder.DropTable(
                name: "Climates");

            migrationBuilder.DropTable(
                name: "PottingMixes");
        }
    }
}
