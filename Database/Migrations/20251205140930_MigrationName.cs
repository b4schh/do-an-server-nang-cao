using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FootballField.API.Database.Migrations
{
    /// <inheritdoc />
    public partial class MigrationName : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Wards_Provinces_province_code",
                schema: "location",
                table: "Wards");

            migrationBuilder.AddUniqueConstraint(
                name: "AK_Provinces_code",
                schema: "location",
                table: "Provinces",
                column: "code");

            migrationBuilder.AddForeignKey(
                name: "FK_Wards_Provinces_province_code",
                schema: "location",
                table: "Wards",
                column: "province_code",
                principalSchema: "location",
                principalTable: "Provinces",
                principalColumn: "code",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Wards_Provinces_province_code",
                schema: "location",
                table: "Wards");

            migrationBuilder.DropUniqueConstraint(
                name: "AK_Provinces_code",
                schema: "location",
                table: "Provinces");

            migrationBuilder.AddForeignKey(
                name: "FK_Wards_Provinces_province_code",
                schema: "location",
                table: "Wards",
                column: "province_code",
                principalSchema: "location",
                principalTable: "Provinces",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
