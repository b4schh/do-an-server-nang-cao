using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FootballField.API.Migrations
{
    /// <inheritdoc />
    public partial class AddSystemConfigAndBankInfo : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "bank_account_name",
                table: "OWNER_SETTING",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "bank_account_number",
                table: "OWNER_SETTING",
                type: "varchar(50)",
                unicode: false,
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "bank_name",
                table: "OWNER_SETTING",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "bank_qr_code_url",
                table: "OWNER_SETTING",
                type: "varchar(max)",
                unicode: false,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "bank_account_name",
                table: "OWNER_SETTING");

            migrationBuilder.DropColumn(
                name: "bank_account_number",
                table: "OWNER_SETTING");

            migrationBuilder.DropColumn(
                name: "bank_name",
                table: "OWNER_SETTING");

            migrationBuilder.DropColumn(
                name: "bank_qr_code_url",
                table: "OWNER_SETTING");
        }
    }
}
