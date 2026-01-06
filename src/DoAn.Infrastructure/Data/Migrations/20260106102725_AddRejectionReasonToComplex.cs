using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DoAn.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddRejectionReasonToComplex : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "rejection_reason",
                table: "COMPLEX",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "rejection_reason",
                table: "COMPLEX");
        }
    }
}
