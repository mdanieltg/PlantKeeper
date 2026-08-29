using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PlantKeeperAPI.Database.Migrations
{
    /// <inheritdoc />
    public partial class AddSpeciesCareAndEcosystem : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_FertilizationLogs_FertilizationMethods_MethodId",
                table: "FertilizationLogs");

            migrationBuilder.DropForeignKey(
                name: "FK_TreatmentLogs_TreatmentMethods_MethodId",
                table: "TreatmentLogs");

            migrationBuilder.DropTable(
                name: "FertilizationMethods");

            migrationBuilder.DropTable(
                name: "TreatmentMethods");

            migrationBuilder.RenameColumn(
                name: "MethodId",
                table: "TreatmentLogs",
                newName: "TreatmentId");

            migrationBuilder.RenameIndex(
                name: "IX_TreatmentLogs_MethodId",
                table: "TreatmentLogs",
                newName: "IX_TreatmentLogs_TreatmentId");

            migrationBuilder.DropColumn(
                name: "NameInSpanish",
                table: "PlantSpecies");

            migrationBuilder.AddColumn<string>(
                name: "FertilizationFrequency",
                table: "PlantSpecies",
                type: "varchar(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "")
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.RenameColumn(
                name: "MethodId",
                table: "FertilizationLogs",
                newName: "FertilizerId");

            migrationBuilder.RenameIndex(
                name: "IX_FertilizationLogs_MethodId",
                table: "FertilizationLogs",
                newName: "IX_FertilizationLogs_FertilizerId");

            migrationBuilder.AddColumn<string>(
                name: "FertilizationNotes",
                table: "PlantSpecies",
                type: "varchar(255)",
                maxLength: 255,
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "FloweringHabit",
                table: "PlantSpecies",
                type: "varchar(30)",
                maxLength: 30,
                nullable: false,
                defaultValue: "")
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "NameInEnglish",
                table: "PlantSpecies",
                type: "varchar(50)",
                maxLength: 50,
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "Dose",
                table: "FertilizationLogs",
                type: "varchar(100)",
                maxLength: 100,
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "BeneficialOrganisms",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    Name = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ScientificName = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Role = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    SuggestedPlants = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Notes = table.Column<string>(type: "varchar(300)", maxLength: 300, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BeneficialOrganisms", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Fertilizers",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    Name = table.Column<string>(type: "varchar(30)", maxLength: 30, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Description = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    NpkRatio = table.Column<string>(type: "varchar(15)", maxLength: 15, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Category = table.Column<string>(type: "varchar(30)", maxLength: 30, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Fertilizers", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "GrowthLogs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    PlantId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    Date = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    HeightCm = table.Column<decimal>(type: "decimal(6,1)", precision: 6, scale: 1, nullable: true),
                    HeightToLastNodeCm = table.Column<decimal>(type: "decimal(6,1)", precision: 6, scale: 1, nullable: true),
                    Notes = table.Column<string>(type: "varchar(300)", maxLength: 300, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4")
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
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Pests",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    Name = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Description = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Status = table.Column<string>(type: "varchar(30)", maxLength: 30, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Notes = table.Column<string>(type: "varchar(300)", maxLength: 300, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Pests", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "PropagationMethods",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    Name = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Description = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PropagationMethods", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "SpeciesCareProfiles",
                columns: table => new
                {
                    SpeciesId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    LightMin = table.Column<string>(type: "varchar(30)", maxLength: 30, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    LightMax = table.Column<string>(type: "varchar(30)", maxLength: 30, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    LightNotes = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    MinTemperatureCelsius = table.Column<int>(type: "int", nullable: false),
                    MaxTemperatureCelsius = table.Column<int>(type: "int", nullable: false),
                    WateringRequirement = table.Column<string>(type: "varchar(150)", maxLength: 150, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    SoilPhMin = table.Column<decimal>(type: "decimal(3,1)", precision: 3, scale: 1, nullable: false),
                    SoilPhMax = table.Column<decimal>(type: "decimal(3,1)", precision: 3, scale: 1, nullable: false),
                    SoilPhNotes = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    WindTolerance = table.Column<string>(type: "varchar(30)", maxLength: 30, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    WindToleranceNotes = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4")
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
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "SpeciesFertilizerRecommendations",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    SpeciesId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    Category = table.Column<string>(type: "varchar(30)", maxLength: 30, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Suitability = table.Column<string>(type: "varchar(30)", maxLength: 30, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Notes = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4")
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
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "SpeciesFloweringProfiles",
                columns: table => new
                {
                    SpeciesId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    BloomSeason = table.Column<string>(type: "varchar(150)", maxLength: 150, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    BloomCareNotes = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    SeedViability = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    SeedHarvestTiming = table.Column<string>(type: "varchar(150)", maxLength: 150, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4")
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
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "SpeciesToxicityProfiles",
                columns: table => new
                {
                    SpeciesId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    ToHumans = table.Column<string>(type: "varchar(30)", maxLength: 30, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ToHumansNotes = table.Column<string>(type: "varchar(150)", maxLength: 150, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ToPets = table.Column<string>(type: "varchar(30)", maxLength: 30, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ToPetsNotes = table.Column<string>(type: "varchar(150)", maxLength: 150, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4")
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
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Treatments",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    Name = table.Column<string>(type: "varchar(30)", maxLength: 30, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Description = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Treatments", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "SpeciesBeneficialOrganisms",
                columns: table => new
                {
                    BeneficialOrganismsId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    SupportingSpeciesId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SpeciesBeneficialOrganisms", x => new { x.BeneficialOrganismsId, x.SupportingSpeciesId });
                    table.ForeignKey(
                        name: "FK_SpeciesBeneficialOrganisms_BeneficialOrganisms_BeneficialOrg~",
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
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "BeneficialOrganismPests",
                columns: table => new
                {
                    ControlledById = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    PestsControlledId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci")
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
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "PropagationBatches",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    SpeciesId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    SourcePlantId = table.Column<Guid>(type: "char(36)", nullable: true, collation: "ascii_general_ci"),
                    PropagationMethodId = table.Column<Guid>(type: "char(36)", nullable: true, collation: "ascii_general_ci"),
                    StartDate = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    Count = table.Column<int>(type: "int", nullable: false),
                    Medium = table.Column<string>(type: "varchar(30)", maxLength: 30, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    RootingHormone = table.Column<string>(type: "varchar(30)", maxLength: 30, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Status = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    TargetTransplantWindow = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Notes = table.Column<string>(type: "varchar(300)", maxLength: 300, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4")
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
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "SpeciesPropagationMethods",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    SpeciesId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    PropagationMethodId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    IsPrimary = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    RootingHormone = table.Column<string>(type: "varchar(30)", maxLength: 30, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    BestSeason = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Difficulty = table.Column<string>(type: "varchar(30)", maxLength: 30, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Notes = table.Column<string>(type: "varchar(300)", maxLength: 300, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4")
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
                        name: "FK_SpeciesPropagationMethods_PropagationMethods_PropagationMeth~",
                        column: x => x.PropagationMethodId,
                        principalTable: "PropagationMethods",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "PestTreatments",
                columns: table => new
                {
                    PestsId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    TreatmentsId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci")
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
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "SpeciesTreatmentRecommendations",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    SpeciesId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    TreatmentId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    Safety = table.Column<string>(type: "varchar(30)", maxLength: 30, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Notes = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4")
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
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_BeneficialOrganismPests_PestsControlledId",
                table: "BeneficialOrganismPests",
                column: "PestsControlledId");

            migrationBuilder.CreateIndex(
                name: "IX_GrowthLogs_PlantId",
                table: "GrowthLogs",
                column: "PlantId");

            migrationBuilder.CreateIndex(
                name: "IX_PestTreatments_TreatmentsId",
                table: "PestTreatments",
                column: "TreatmentsId");

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

            migrationBuilder.AddForeignKey(
                name: "FK_FertilizationLogs_Fertilizers_FertilizerId",
                table: "FertilizationLogs",
                column: "FertilizerId",
                principalTable: "Fertilizers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_TreatmentLogs_Treatments_TreatmentId",
                table: "TreatmentLogs",
                column: "TreatmentId",
                principalTable: "Treatments",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_FertilizationLogs_Fertilizers_FertilizerId",
                table: "FertilizationLogs");

            migrationBuilder.DropForeignKey(
                name: "FK_TreatmentLogs_Treatments_TreatmentId",
                table: "TreatmentLogs");

            migrationBuilder.DropTable(
                name: "BeneficialOrganismPests");

            migrationBuilder.DropTable(
                name: "Fertilizers");

            migrationBuilder.DropTable(
                name: "GrowthLogs");

            migrationBuilder.DropTable(
                name: "PestTreatments");

            migrationBuilder.DropTable(
                name: "PropagationBatches");

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
                name: "Pests");

            migrationBuilder.DropTable(
                name: "BeneficialOrganisms");

            migrationBuilder.DropTable(
                name: "PropagationMethods");

            migrationBuilder.DropTable(
                name: "Treatments");

            migrationBuilder.DropColumn(
                name: "FertilizationNotes",
                table: "PlantSpecies");

            migrationBuilder.DropColumn(
                name: "FloweringHabit",
                table: "PlantSpecies");

            migrationBuilder.DropColumn(
                name: "NameInEnglish",
                table: "PlantSpecies");

            migrationBuilder.DropColumn(
                name: "Dose",
                table: "FertilizationLogs");

            migrationBuilder.RenameColumn(
                name: "TreatmentId",
                table: "TreatmentLogs",
                newName: "MethodId");

            migrationBuilder.RenameIndex(
                name: "IX_TreatmentLogs_TreatmentId",
                table: "TreatmentLogs",
                newName: "IX_TreatmentLogs_MethodId");

            migrationBuilder.DropColumn(
                name: "FertilizationFrequency",
                table: "PlantSpecies");

            migrationBuilder.AddColumn<string>(
                name: "NameInSpanish",
                table: "PlantSpecies",
                type: "varchar(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "")
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.RenameColumn(
                name: "FertilizerId",
                table: "FertilizationLogs",
                newName: "MethodId");

            migrationBuilder.RenameIndex(
                name: "IX_FertilizationLogs_FertilizerId",
                table: "FertilizationLogs",
                newName: "IX_FertilizationLogs_MethodId");

            migrationBuilder.CreateTable(
                name: "FertilizationMethods",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    Description = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Name = table.Column<string>(type: "varchar(30)", maxLength: 30, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FertilizationMethods", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "TreatmentMethods",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    Description = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Name = table.Column<string>(type: "varchar(30)", maxLength: 30, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TreatmentMethods", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddForeignKey(
                name: "FK_FertilizationLogs_FertilizationMethods_MethodId",
                table: "FertilizationLogs",
                column: "MethodId",
                principalTable: "FertilizationMethods",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_TreatmentLogs_TreatmentMethods_MethodId",
                table: "TreatmentLogs",
                column: "MethodId",
                principalTable: "TreatmentMethods",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
