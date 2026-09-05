using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PlantKeeperAPI.Database.Migrations
{
    /// <inheritdoc />
    public partial class RestrictAlmanacDeletes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_FertilizationLogs_Fertilizers_FertilizerId",
                table: "FertilizationLogs");

            migrationBuilder.DropForeignKey(
                name: "FK_Plants_PlantSpecies_SpeciesId",
                table: "Plants");

            migrationBuilder.DropForeignKey(
                name: "FK_PlantSpecies_Climates_ClimateId",
                table: "PlantSpecies");

            migrationBuilder.DropForeignKey(
                name: "FK_PlantSpecies_PottingMixes_PottingMixId",
                table: "PlantSpecies");

            migrationBuilder.DropForeignKey(
                name: "FK_PropagationBatches_PlantSpecies_SpeciesId",
                table: "PropagationBatches");

            migrationBuilder.DropForeignKey(
                name: "FK_PropagationBatches_Plants_SourcePlantId",
                table: "PropagationBatches");

            migrationBuilder.DropForeignKey(
                name: "FK_PropagationBatches_PropagationMethods_PropagationMethodId",
                table: "PropagationBatches");

            migrationBuilder.DropForeignKey(
                name: "FK_SpeciesPropagationMethods_PropagationMethods_PropagationMet~",
                table: "SpeciesPropagationMethods");

            migrationBuilder.DropForeignKey(
                name: "FK_SpeciesTreatmentRecommendations_Treatments_TreatmentId",
                table: "SpeciesTreatmentRecommendations");

            migrationBuilder.DropForeignKey(
                name: "FK_TreatmentLogs_Treatments_TreatmentId",
                table: "TreatmentLogs");

            migrationBuilder.DropForeignKey(
                name: "FK_WateringLogs_WateringMethods_MethodId",
                table: "WateringLogs");

            migrationBuilder.AddForeignKey(
                name: "FK_FertilizationLogs_Fertilizers_FertilizerId",
                table: "FertilizationLogs",
                column: "FertilizerId",
                principalTable: "Fertilizers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Plants_PlantSpecies_SpeciesId",
                table: "Plants",
                column: "SpeciesId",
                principalTable: "PlantSpecies",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_PlantSpecies_Climates_ClimateId",
                table: "PlantSpecies",
                column: "ClimateId",
                principalTable: "Climates",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_PlantSpecies_PottingMixes_PottingMixId",
                table: "PlantSpecies",
                column: "PottingMixId",
                principalTable: "PottingMixes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_PropagationBatches_PlantSpecies_SpeciesId",
                table: "PropagationBatches",
                column: "SpeciesId",
                principalTable: "PlantSpecies",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_PropagationBatches_Plants_SourcePlantId",
                table: "PropagationBatches",
                column: "SourcePlantId",
                principalTable: "Plants",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_PropagationBatches_PropagationMethods_PropagationMethodId",
                table: "PropagationBatches",
                column: "PropagationMethodId",
                principalTable: "PropagationMethods",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_SpeciesPropagationMethods_PropagationMethods_PropagationMet~",
                table: "SpeciesPropagationMethods",
                column: "PropagationMethodId",
                principalTable: "PropagationMethods",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_SpeciesTreatmentRecommendations_Treatments_TreatmentId",
                table: "SpeciesTreatmentRecommendations",
                column: "TreatmentId",
                principalTable: "Treatments",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_TreatmentLogs_Treatments_TreatmentId",
                table: "TreatmentLogs",
                column: "TreatmentId",
                principalTable: "Treatments",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_WateringLogs_WateringMethods_MethodId",
                table: "WateringLogs",
                column: "MethodId",
                principalTable: "WateringMethods",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_FertilizationLogs_Fertilizers_FertilizerId",
                table: "FertilizationLogs");

            migrationBuilder.DropForeignKey(
                name: "FK_Plants_PlantSpecies_SpeciesId",
                table: "Plants");

            migrationBuilder.DropForeignKey(
                name: "FK_PlantSpecies_Climates_ClimateId",
                table: "PlantSpecies");

            migrationBuilder.DropForeignKey(
                name: "FK_PlantSpecies_PottingMixes_PottingMixId",
                table: "PlantSpecies");

            migrationBuilder.DropForeignKey(
                name: "FK_PropagationBatches_PlantSpecies_SpeciesId",
                table: "PropagationBatches");

            migrationBuilder.DropForeignKey(
                name: "FK_PropagationBatches_Plants_SourcePlantId",
                table: "PropagationBatches");

            migrationBuilder.DropForeignKey(
                name: "FK_PropagationBatches_PropagationMethods_PropagationMethodId",
                table: "PropagationBatches");

            migrationBuilder.DropForeignKey(
                name: "FK_SpeciesPropagationMethods_PropagationMethods_PropagationMet~",
                table: "SpeciesPropagationMethods");

            migrationBuilder.DropForeignKey(
                name: "FK_SpeciesTreatmentRecommendations_Treatments_TreatmentId",
                table: "SpeciesTreatmentRecommendations");

            migrationBuilder.DropForeignKey(
                name: "FK_TreatmentLogs_Treatments_TreatmentId",
                table: "TreatmentLogs");

            migrationBuilder.DropForeignKey(
                name: "FK_WateringLogs_WateringMethods_MethodId",
                table: "WateringLogs");

            migrationBuilder.AddForeignKey(
                name: "FK_FertilizationLogs_Fertilizers_FertilizerId",
                table: "FertilizationLogs",
                column: "FertilizerId",
                principalTable: "Fertilizers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Plants_PlantSpecies_SpeciesId",
                table: "Plants",
                column: "SpeciesId",
                principalTable: "PlantSpecies",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_PlantSpecies_Climates_ClimateId",
                table: "PlantSpecies",
                column: "ClimateId",
                principalTable: "Climates",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_PlantSpecies_PottingMixes_PottingMixId",
                table: "PlantSpecies",
                column: "PottingMixId",
                principalTable: "PottingMixes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_PropagationBatches_PlantSpecies_SpeciesId",
                table: "PropagationBatches",
                column: "SpeciesId",
                principalTable: "PlantSpecies",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_PropagationBatches_Plants_SourcePlantId",
                table: "PropagationBatches",
                column: "SourcePlantId",
                principalTable: "Plants",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_PropagationBatches_PropagationMethods_PropagationMethodId",
                table: "PropagationBatches",
                column: "PropagationMethodId",
                principalTable: "PropagationMethods",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_SpeciesPropagationMethods_PropagationMethods_PropagationMet~",
                table: "SpeciesPropagationMethods",
                column: "PropagationMethodId",
                principalTable: "PropagationMethods",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_SpeciesTreatmentRecommendations_Treatments_TreatmentId",
                table: "SpeciesTreatmentRecommendations",
                column: "TreatmentId",
                principalTable: "Treatments",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_TreatmentLogs_Treatments_TreatmentId",
                table: "TreatmentLogs",
                column: "TreatmentId",
                principalTable: "Treatments",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_WateringLogs_WateringMethods_MethodId",
                table: "WateringLogs",
                column: "MethodId",
                principalTable: "WateringMethods",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
