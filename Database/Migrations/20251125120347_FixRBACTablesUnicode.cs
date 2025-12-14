using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FootballField.API.Database.Migrations
{
    /// <inheritdoc />
    public partial class FixRBACTablesUnicode : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Alter ROLE.name to nvarchar(50)
            migrationBuilder.AlterColumn<string>(
                name: "name",
                table: "ROLE",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "varchar(50)",
                oldMaxLength: 50);

            // Alter PERMISSION.module to nvarchar(50)
            migrationBuilder.AlterColumn<string>(
                name: "module",
                table: "PERMISSION",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "varchar(50)",
                oldMaxLength: 50,
                oldNullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Revert ROLE.name to varchar(50)
            migrationBuilder.AlterColumn<string>(
                name: "name",
                table: "ROLE",
                type: "varchar(50)",
                maxLength: 50,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(50)",
                oldMaxLength: 50);

            // Revert PERMISSION.module to varchar(50)
            migrationBuilder.AlterColumn<string>(
                name: "module",
                table: "PERMISSION",
                type: "varchar(50)",
                maxLength: 50,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(50)",
                oldMaxLength: 50,
                oldNullable: true);
        }
    }
}
