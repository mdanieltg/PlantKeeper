using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PlantKeeperAPI.Database.Migrations
{
    /// <inheritdoc />
    public partial class RepottingFertilizationAndObservationLog : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Care",
                table: "Plants",
                newName: "Comments");

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "WateringMethods",
                type: "varchar(30)",
                maxLength: 30,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "varchar(20)",
                oldMaxLength: 20)
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "Plants",
                type: "varchar(50)",
                maxLength: 50,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "varchar(30)",
                oldMaxLength: 30)
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "HumidityConditions",
                table: "Plants",
                type: "varchar(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "")
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<Guid>(
                name: "LightingTypeId",
                table: "Plants",
                type: "char(36)",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                collation: "ascii_general_ci");

            migrationBuilder.AddColumn<string>(
                name: "NameInEnglish",
                table: "Plants",
                type: "varchar(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "")
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<Guid>(
                name: "SoilTypeId",
                table: "Plants",
                type: "char(36)",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                collation: "ascii_general_ci");

            migrationBuilder.CreateTable(
                name: "FertilizationMethods",
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
                    table.PrimaryKey("PK_FertilizationMethods", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Lighting",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    Type = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Description = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Lighting", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "ObservationLogs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    PlantId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    ObservationDate = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    Notes = table.Column<string>(type: "varchar(300)", maxLength: 300, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4")
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
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "RepottingLogs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    PlantId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    Date = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    Comments = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4")
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
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Soil",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    Type = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Description = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Soil", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "FertilizationLogs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    PlantId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    FertilizationMethodId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    Date = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    Comments = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FertilizationLogs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FertilizationLogs_FertilizationMethods_FertilizationMethodId",
                        column: x => x.FertilizationMethodId,
                        principalTable: "FertilizationMethods",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_FertilizationLogs_Plants_PlantId",
                        column: x => x.PlantId,
                        principalTable: "Plants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_Plants_LightingTypeId",
                table: "Plants",
                column: "LightingTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_Plants_SoilTypeId",
                table: "Plants",
                column: "SoilTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_FertilizationLogs_FertilizationMethodId",
                table: "FertilizationLogs",
                column: "FertilizationMethodId");

            migrationBuilder.CreateIndex(
                name: "IX_FertilizationLogs_PlantId",
                table: "FertilizationLogs",
                column: "PlantId");

            migrationBuilder.CreateIndex(
                name: "IX_ObservationLogs_PlantId",
                table: "ObservationLogs",
                column: "PlantId");

            migrationBuilder.CreateIndex(
                name: "IX_RepottingLogs_PlantId",
                table: "RepottingLogs",
                column: "PlantId");

            migrationBuilder.AddForeignKey(
                name: "FK_Plants_Lighting_LightingTypeId",
                table: "Plants",
                column: "LightingTypeId",
                principalTable: "Lighting",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Plants_Soil_SoilTypeId",
                table: "Plants",
                column: "SoilTypeId",
                principalTable: "Soil",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Plants_Lighting_LightingTypeId",
                table: "Plants");

            migrationBuilder.DropForeignKey(
                name: "FK_Plants_Soil_SoilTypeId",
                table: "Plants");

            migrationBuilder.DropTable(
                name: "FertilizationLogs");

            migrationBuilder.DropTable(
                name: "Lighting");

            migrationBuilder.DropTable(
                name: "ObservationLogs");

            migrationBuilder.DropTable(
                name: "RepottingLogs");

            migrationBuilder.DropTable(
                name: "Soil");

            migrationBuilder.DropTable(
                name: "FertilizationMethods");

            migrationBuilder.DropIndex(
                name: "IX_Plants_LightingTypeId",
                table: "Plants");

            migrationBuilder.DropIndex(
                name: "IX_Plants_SoilTypeId",
                table: "Plants");

            migrationBuilder.DropColumn(
                name: "HumidityConditions",
                table: "Plants");

            migrationBuilder.DropColumn(
                name: "LightingTypeId",
                table: "Plants");

            migrationBuilder.DropColumn(
                name: "NameInEnglish",
                table: "Plants");

            migrationBuilder.DropColumn(
                name: "SoilTypeId",
                table: "Plants");

            migrationBuilder.RenameColumn(
                name: "Comments",
                table: "Plants",
                newName: "Care");

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "WateringMethods",
                type: "varchar(20)",
                maxLength: 20,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "varchar(30)",
                oldMaxLength: 30)
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "Plants",
                type: "varchar(30)",
                maxLength: 30,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "varchar(50)",
                oldMaxLength: 50)
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");
        }
    }
}
