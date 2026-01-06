using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DoAn.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class Fix_Unique_Email_For_Soft_Delete : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_USER_email",
                table: "USER");

            migrationBuilder.DropCheckConstraint(
                name: "CK_SystemConfig_DataType",
                table: "SYSTEM_CONFIG");

            migrationBuilder.DropCheckConstraint(
                name: "CK_Review_Rating",
                table: "REVIEW");

            migrationBuilder.DropCheckConstraint(
                name: "CK_OwnerSetting_DepositRate",
                table: "OWNER_SETTING");

            migrationBuilder.DropCheckConstraint(
                name: "CK_Booking_DepositAmount",
                table: "BOOKING");

            migrationBuilder.DropCheckConstraint(
                name: "CK_Booking_Status",
                table: "BOOKING");

            migrationBuilder.DropCheckConstraint(
                name: "CK_Booking_TotalAmount",
                table: "BOOKING");

            migrationBuilder.CreateIndex(
                name: "IX_USER_email",
                table: "USER",
                column: "email",
                unique: true,
                filter: "[is_deleted] = 0");

            migrationBuilder.AddCheckConstraint(
                name: "CK_SystemConfig_DataType",
                table: "SYSTEM_CONFIG",
                sql: "data_type IN ('string', 'int', 'decimal', 'boolean', 'datetime')");

            migrationBuilder.AddCheckConstraint(
                name: "CK_Review_Rating",
                table: "REVIEW",
                sql: "rating >= 1 AND rating <= 5");

            migrationBuilder.AddCheckConstraint(
                name: "CK_OwnerSetting_DepositRate",
                table: "OWNER_SETTING",
                sql: "deposit_rate >= 0 AND deposit_rate <= 100");

            migrationBuilder.AddCheckConstraint(
                name: "CK_Booking_DepositAmount",
                table: "BOOKING",
                sql: "deposit_amount >= 0");

            migrationBuilder.AddCheckConstraint(
                name: "CK_Booking_DepositLessThanTotal",
                table: "BOOKING",
                sql: "deposit_amount <= total_amount");

            migrationBuilder.AddCheckConstraint(
                name: "CK_Booking_TotalAmount",
                table: "BOOKING",
                sql: "total_amount >= 0");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_USER_email",
                table: "USER");

            migrationBuilder.DropCheckConstraint(
                name: "CK_SystemConfig_DataType",
                table: "SYSTEM_CONFIG");

            migrationBuilder.DropCheckConstraint(
                name: "CK_Review_Rating",
                table: "REVIEW");

            migrationBuilder.DropCheckConstraint(
                name: "CK_OwnerSetting_DepositRate",
                table: "OWNER_SETTING");

            migrationBuilder.DropCheckConstraint(
                name: "CK_Booking_DepositAmount",
                table: "BOOKING");

            migrationBuilder.DropCheckConstraint(
                name: "CK_Booking_DepositLessThanTotal",
                table: "BOOKING");

            migrationBuilder.DropCheckConstraint(
                name: "CK_Booking_TotalAmount",
                table: "BOOKING");

            migrationBuilder.CreateIndex(
                name: "IX_USER_email",
                table: "USER",
                column: "email",
                unique: true,
                filter: "[email] IS NOT NULL");

            migrationBuilder.AddCheckConstraint(
                name: "CK_SystemConfig_DataType",
                table: "SYSTEM_CONFIG",
                sql: "data_type IN ('string','int','decimal','boolean','json')");

            migrationBuilder.AddCheckConstraint(
                name: "CK_Review_Rating",
                table: "REVIEW",
                sql: "rating BETWEEN 1 AND 5");

            migrationBuilder.AddCheckConstraint(
                name: "CK_OwnerSetting_DepositRate",
                table: "OWNER_SETTING",
                sql: "deposit_rate BETWEEN 0 AND 1");

            migrationBuilder.AddCheckConstraint(
                name: "CK_Booking_DepositAmount",
                table: "BOOKING",
                sql: "deposit_amount >= 0 AND deposit_amount <= total_amount");

            migrationBuilder.AddCheckConstraint(
                name: "CK_Booking_Status",
                table: "BOOKING",
                sql: "booking_status BETWEEN 0 AND 7");

            migrationBuilder.AddCheckConstraint(
                name: "CK_Booking_TotalAmount",
                table: "BOOKING",
                sql: "total_amount > 0");
        }
    }
}
