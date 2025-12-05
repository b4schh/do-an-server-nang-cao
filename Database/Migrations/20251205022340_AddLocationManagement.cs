using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FootballField.API.Database.Migrations
{
    /// <inheritdoc />
    public partial class AddLocationManagement : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "location");

            migrationBuilder.CreateTable(
                name: "Provinces",
                schema: "location",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    code = table.Column<int>(type: "int", nullable: false),
                    name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    codename = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    division_type = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Provinces", x => x.id);
                    table.UniqueConstraint("AK_Provinces_code", x => x.code);
                });

            migrationBuilder.CreateTable(
                name: "Wards",
                schema: "location",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    code = table.Column<int>(type: "int", nullable: false),
                    name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    codename = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    division_type = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    province_code = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Wards", x => x.id);
                    table.ForeignKey(
                        name: "FK_Wards_Provinces_province_code",
                        column: x => x.province_code,
                        principalSchema: "location",
                        principalTable: "Provinces",
                        principalColumn: "code",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Province_Code",
                schema: "location",
                table: "Provinces",
                column: "code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Provinces_name",
                schema: "location",
                table: "Provinces",
                column: "name");

            migrationBuilder.CreateIndex(
                name: "IX_Ward_Code",
                schema: "location",
                table: "Wards",
                column: "code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Ward_ProvinceCode",
                schema: "location",
                table: "Wards",
                column: "province_code");

            migrationBuilder.CreateIndex(
                name: "IX_Wards_name",
                schema: "location",
                table: "Wards",
                column: "name");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Wards",
                schema: "location");

            migrationBuilder.DropTable(
                name: "Provinces",
                schema: "location");
        }
    }
}
