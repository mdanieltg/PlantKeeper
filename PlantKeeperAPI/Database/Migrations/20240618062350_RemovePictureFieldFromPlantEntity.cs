using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PlantKeeperAPI.Database.Migrations
{
    /// <inheritdoc />
    public partial class RemovePictureFieldFromPlantEntity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Picture",
                table: "Plants");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<byte[]>(
                name: "Picture",
                table: "Plants",
                type: "longblob",
                maxLength: 2000000,
                nullable: true);
        }
    }
}
