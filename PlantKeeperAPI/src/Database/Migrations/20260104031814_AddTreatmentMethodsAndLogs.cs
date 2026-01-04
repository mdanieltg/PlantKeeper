using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PlantKeeperAPI.Database.Migrations
{
    /// <inheritdoc />
    public partial class AddTreatmentMethodsAndLogs : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "TreatmentMethods",
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
                    table.PrimaryKey("PK_TreatmentMethods", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "TreatmentLogs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    PlantId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    TreatmentMethodId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    Date = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    Comments = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4")
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
                        name: "FK_TreatmentLogs_TreatmentMethods_TreatmentMethodId",
                        column: x => x.TreatmentMethodId,
                        principalTable: "TreatmentMethods",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_TreatmentLogs_PlantId",
                table: "TreatmentLogs",
                column: "PlantId");

            migrationBuilder.CreateIndex(
                name: "IX_TreatmentLogs_TreatmentMethodId",
                table: "TreatmentLogs",
                column: "TreatmentMethodId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TreatmentLogs");

            migrationBuilder.DropTable(
                name: "TreatmentMethods");
        }
    }
}
