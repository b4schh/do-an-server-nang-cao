using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DoAn.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class Add_Unique_Phone_Index : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_USER_phone",
                table: "USER",
                column: "phone",
                unique: true,
                filter: "[phone] IS NOT NULL AND [is_deleted] = 0");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_USER_phone",
                table: "USER");
        }
    }
}
