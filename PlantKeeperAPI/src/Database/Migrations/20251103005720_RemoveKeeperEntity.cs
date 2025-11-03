using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PlantKeeperAPI.Database.Migrations
{
    /// <inheritdoc />
    public partial class RemoveKeeperEntity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_WateringLogs_Keepers_KeeperId",
                table: "WateringLogs");

            migrationBuilder.DropTable(
                name: "Keepers");

            migrationBuilder.DropIndex(
                name: "IX_WateringLogs_KeeperId",
                table: "WateringLogs");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Keepers",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    Name = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Keepers", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_WateringLogs_KeeperId",
                table: "WateringLogs",
                column: "KeeperId");

            migrationBuilder.AddForeignKey(
                name: "FK_WateringLogs_Keepers_KeeperId",
                table: "WateringLogs",
                column: "KeeperId",
                principalTable: "Keepers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
