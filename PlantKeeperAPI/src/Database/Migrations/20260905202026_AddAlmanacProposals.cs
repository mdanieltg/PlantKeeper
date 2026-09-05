using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PlantKeeperAPI.Database.Migrations
{
    /// <inheritdoc />
    public partial class AddAlmanacProposals : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Version",
                table: "WateringMethods",
                type: "integer",
                nullable: false,
                defaultValue: 1);

            migrationBuilder.AddColumn<int>(
                name: "Version",
                table: "Treatments",
                type: "integer",
                nullable: false,
                defaultValue: 1);

            migrationBuilder.AddColumn<int>(
                name: "Version",
                table: "SpeciesTreatmentRecommendations",
                type: "integer",
                nullable: false,
                defaultValue: 1);

            migrationBuilder.AddColumn<int>(
                name: "Version",
                table: "SpeciesToxicityProfiles",
                type: "integer",
                nullable: false,
                defaultValue: 1);

            migrationBuilder.AddColumn<int>(
                name: "Version",
                table: "SpeciesPropagationMethods",
                type: "integer",
                nullable: false,
                defaultValue: 1);

            migrationBuilder.AddColumn<int>(
                name: "Version",
                table: "SpeciesFloweringProfiles",
                type: "integer",
                nullable: false,
                defaultValue: 1);

            migrationBuilder.AddColumn<int>(
                name: "Version",
                table: "SpeciesFertilizerRecommendations",
                type: "integer",
                nullable: false,
                defaultValue: 1);

            migrationBuilder.AddColumn<int>(
                name: "Version",
                table: "SpeciesCareProfiles",
                type: "integer",
                nullable: false,
                defaultValue: 1);

            migrationBuilder.AddColumn<int>(
                name: "Version",
                table: "PropagationMethods",
                type: "integer",
                nullable: false,
                defaultValue: 1);

            migrationBuilder.AddColumn<int>(
                name: "Version",
                table: "PottingMixes",
                type: "integer",
                nullable: false,
                defaultValue: 1);

            migrationBuilder.AddColumn<int>(
                name: "Version",
                table: "PlantSpecies",
                type: "integer",
                nullable: false,
                defaultValue: 1);

            migrationBuilder.AddColumn<int>(
                name: "Version",
                table: "Pests",
                type: "integer",
                nullable: false,
                defaultValue: 1);

            migrationBuilder.AddColumn<int>(
                name: "Version",
                table: "Fertilizers",
                type: "integer",
                nullable: false,
                defaultValue: 1);

            migrationBuilder.AddColumn<int>(
                name: "Version",
                table: "Climates",
                type: "integer",
                nullable: false,
                defaultValue: 1);

            migrationBuilder.AddColumn<int>(
                name: "Version",
                table: "BeneficialOrganisms",
                type: "integer",
                nullable: false,
                defaultValue: 1);

            migrationBuilder.CreateTable(
                name: "AlmanacChangeProposals",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TargetType = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    TargetId = table.Column<Guid>(type: "uuid", nullable: true),
                    Operation = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    ProposedState = table.Column<string>(type: "jsonb", nullable: true),
                    TargetVersion = table.Column<int>(type: "integer", nullable: true),
                    Status = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    AutoApproved = table.Column<bool>(type: "boolean", nullable: false),
                    ProposedById = table.Column<Guid>(type: "uuid", nullable: false),
                    ProposedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    ReviewedById = table.Column<Guid>(type: "uuid", nullable: true),
                    ReviewedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    ReviewNote = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AlmanacChangeProposals", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AlmanacChangeProposals_AspNetUsers_ProposedById",
                        column: x => x.ProposedById,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_AlmanacChangeProposals_AspNetUsers_ReviewedById",
                        column: x => x.ReviewedById,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AlmanacChangeProposals_ProposedById",
                table: "AlmanacChangeProposals",
                column: "ProposedById");

            migrationBuilder.CreateIndex(
                name: "IX_AlmanacChangeProposals_ReviewedById",
                table: "AlmanacChangeProposals",
                column: "ReviewedById");

            migrationBuilder.CreateIndex(
                name: "IX_AlmanacChangeProposals_Status_ProposedAt",
                table: "AlmanacChangeProposals",
                columns: new[] { "Status", "ProposedAt" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AlmanacChangeProposals");

            migrationBuilder.DropColumn(
                name: "Version",
                table: "WateringMethods");

            migrationBuilder.DropColumn(
                name: "Version",
                table: "Treatments");

            migrationBuilder.DropColumn(
                name: "Version",
                table: "SpeciesTreatmentRecommendations");

            migrationBuilder.DropColumn(
                name: "Version",
                table: "SpeciesToxicityProfiles");

            migrationBuilder.DropColumn(
                name: "Version",
                table: "SpeciesPropagationMethods");

            migrationBuilder.DropColumn(
                name: "Version",
                table: "SpeciesFloweringProfiles");

            migrationBuilder.DropColumn(
                name: "Version",
                table: "SpeciesFertilizerRecommendations");

            migrationBuilder.DropColumn(
                name: "Version",
                table: "SpeciesCareProfiles");

            migrationBuilder.DropColumn(
                name: "Version",
                table: "PropagationMethods");

            migrationBuilder.DropColumn(
                name: "Version",
                table: "PottingMixes");

            migrationBuilder.DropColumn(
                name: "Version",
                table: "PlantSpecies");

            migrationBuilder.DropColumn(
                name: "Version",
                table: "Pests");

            migrationBuilder.DropColumn(
                name: "Version",
                table: "Fertilizers");

            migrationBuilder.DropColumn(
                name: "Version",
                table: "Climates");

            migrationBuilder.DropColumn(
                name: "Version",
                table: "BeneficialOrganisms");
        }
    }
}
