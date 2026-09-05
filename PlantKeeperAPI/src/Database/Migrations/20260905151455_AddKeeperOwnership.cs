using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PlantKeeperAPI.Database.Migrations
{
    /// <inheritdoc />
    public partial class AddKeeperOwnership : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "KeeperId",
                table: "WateringLogs",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "KeeperId",
                table: "TreatmentLogs",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "KeeperId",
                table: "RepottingLogs",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "KeeperId",
                table: "PropagationBatches",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "KeeperId",
                table: "Plants",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "KeeperId",
                table: "ObservationLogs",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "KeeperId",
                table: "GrowthLogs",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "KeeperId",
                table: "FertilizationLogs",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateIndex(
                name: "IX_WateringLogs_KeeperId",
                table: "WateringLogs",
                column: "KeeperId");

            migrationBuilder.CreateIndex(
                name: "IX_TreatmentLogs_KeeperId",
                table: "TreatmentLogs",
                column: "KeeperId");

            migrationBuilder.CreateIndex(
                name: "IX_RepottingLogs_KeeperId",
                table: "RepottingLogs",
                column: "KeeperId");

            migrationBuilder.CreateIndex(
                name: "IX_PropagationBatches_KeeperId",
                table: "PropagationBatches",
                column: "KeeperId");

            migrationBuilder.CreateIndex(
                name: "IX_Plants_KeeperId",
                table: "Plants",
                column: "KeeperId");

            migrationBuilder.CreateIndex(
                name: "IX_ObservationLogs_KeeperId",
                table: "ObservationLogs",
                column: "KeeperId");

            migrationBuilder.CreateIndex(
                name: "IX_GrowthLogs_KeeperId",
                table: "GrowthLogs",
                column: "KeeperId");

            migrationBuilder.CreateIndex(
                name: "IX_FertilizationLogs_KeeperId",
                table: "FertilizationLogs",
                column: "KeeperId");

            migrationBuilder.AddForeignKey(
                name: "FK_FertilizationLogs_AspNetUsers_KeeperId",
                table: "FertilizationLogs",
                column: "KeeperId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_GrowthLogs_AspNetUsers_KeeperId",
                table: "GrowthLogs",
                column: "KeeperId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ObservationLogs_AspNetUsers_KeeperId",
                table: "ObservationLogs",
                column: "KeeperId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Plants_AspNetUsers_KeeperId",
                table: "Plants",
                column: "KeeperId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_PropagationBatches_AspNetUsers_KeeperId",
                table: "PropagationBatches",
                column: "KeeperId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_RepottingLogs_AspNetUsers_KeeperId",
                table: "RepottingLogs",
                column: "KeeperId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_TreatmentLogs_AspNetUsers_KeeperId",
                table: "TreatmentLogs",
                column: "KeeperId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_WateringLogs_AspNetUsers_KeeperId",
                table: "WateringLogs",
                column: "KeeperId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_FertilizationLogs_AspNetUsers_KeeperId",
                table: "FertilizationLogs");

            migrationBuilder.DropForeignKey(
                name: "FK_GrowthLogs_AspNetUsers_KeeperId",
                table: "GrowthLogs");

            migrationBuilder.DropForeignKey(
                name: "FK_ObservationLogs_AspNetUsers_KeeperId",
                table: "ObservationLogs");

            migrationBuilder.DropForeignKey(
                name: "FK_Plants_AspNetUsers_KeeperId",
                table: "Plants");

            migrationBuilder.DropForeignKey(
                name: "FK_PropagationBatches_AspNetUsers_KeeperId",
                table: "PropagationBatches");

            migrationBuilder.DropForeignKey(
                name: "FK_RepottingLogs_AspNetUsers_KeeperId",
                table: "RepottingLogs");

            migrationBuilder.DropForeignKey(
                name: "FK_TreatmentLogs_AspNetUsers_KeeperId",
                table: "TreatmentLogs");

            migrationBuilder.DropForeignKey(
                name: "FK_WateringLogs_AspNetUsers_KeeperId",
                table: "WateringLogs");

            migrationBuilder.DropIndex(
                name: "IX_WateringLogs_KeeperId",
                table: "WateringLogs");

            migrationBuilder.DropIndex(
                name: "IX_TreatmentLogs_KeeperId",
                table: "TreatmentLogs");

            migrationBuilder.DropIndex(
                name: "IX_RepottingLogs_KeeperId",
                table: "RepottingLogs");

            migrationBuilder.DropIndex(
                name: "IX_PropagationBatches_KeeperId",
                table: "PropagationBatches");

            migrationBuilder.DropIndex(
                name: "IX_Plants_KeeperId",
                table: "Plants");

            migrationBuilder.DropIndex(
                name: "IX_ObservationLogs_KeeperId",
                table: "ObservationLogs");

            migrationBuilder.DropIndex(
                name: "IX_GrowthLogs_KeeperId",
                table: "GrowthLogs");

            migrationBuilder.DropIndex(
                name: "IX_FertilizationLogs_KeeperId",
                table: "FertilizationLogs");

            migrationBuilder.DropColumn(
                name: "KeeperId",
                table: "WateringLogs");

            migrationBuilder.DropColumn(
                name: "KeeperId",
                table: "TreatmentLogs");

            migrationBuilder.DropColumn(
                name: "KeeperId",
                table: "RepottingLogs");

            migrationBuilder.DropColumn(
                name: "KeeperId",
                table: "PropagationBatches");

            migrationBuilder.DropColumn(
                name: "KeeperId",
                table: "Plants");

            migrationBuilder.DropColumn(
                name: "KeeperId",
                table: "ObservationLogs");

            migrationBuilder.DropColumn(
                name: "KeeperId",
                table: "GrowthLogs");

            migrationBuilder.DropColumn(
                name: "KeeperId",
                table: "FertilizationLogs");
        }
    }
}
