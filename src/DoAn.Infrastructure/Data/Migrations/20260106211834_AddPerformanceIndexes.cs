using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DoAn.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddPerformanceIndexes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // 1. COMPLEX: is_active + status + owner_id (for GetPagedAsync)
            migrationBuilder.CreateIndex(
                name: "IX_COMPLEX_Active_Status_Owner",
                table: "COMPLEX",
                columns: new[] { "is_active", "status", "owner_id" });

            // 2. COMPLEX: province + ward (for location filtering)
            migrationBuilder.CreateIndex(
                name: "IX_COMPLEX_Province_Ward",
                table: "COMPLEX",
                columns: new[] { "province", "ward" });

            // 3. FIELD: complex_id + is_active (for GetAllActiveFieldsWithDetailsAsync)
            migrationBuilder.CreateIndex(
                name: "IX_FIELD_Complex_Active",
                table: "FIELD",
                columns: new[] { "complex_id", "is_active" });

            // 4. BOOKING: field_id + booking_status (for field booking queries)
            migrationBuilder.CreateIndex(
                name: "IX_BOOKING_Field_Status",
                table: "BOOKING",
                columns: new[] { "field_id", "booking_status" });

            // 5. BOOKING: booking_date + booking_status (for date range queries)
            migrationBuilder.CreateIndex(
                name: "IX_BOOKING_BookingDate_Status",
                table: "BOOKING",
                columns: new[] { "booking_date", "booking_status" });

            // 6. BOOKING: customer_id + booking_status + booking_date (for customer history)
            migrationBuilder.CreateIndex(
                name: "IX_BOOKING_Customer_Status_Date",
                table: "BOOKING",
                columns: new[] { "customer_id", "booking_status", "booking_date" });

            // 7. BOOKING: owner_id + booking_status (for owner revenue queries)
            migrationBuilder.CreateIndex(
                name: "IX_BOOKING_Owner_Status",
                table: "BOOKING",
                columns: new[] { "owner_id", "booking_status" });

            // 8. OWNER_SETTING: bank_account_number (filtered index for bank check)
            migrationBuilder.Sql(@"
                CREATE NONCLUSTERED INDEX IX_OWNER_SETTING_BankInfo
                ON OWNER_SETTING (owner_id, bank_account_number)
                WHERE bank_account_number IS NOT NULL;
            ");

            // 9. REVIEW: booking_id + rating (for review statistics)
            migrationBuilder.CreateIndex(
                name: "IX_REVIEW_Booking_Rating",
                table: "REVIEW",
                columns: new[] { "booking_id", "rating" });

            // 10. TIME_SLOT: field_id + start_time (for time slot queries)
            migrationBuilder.CreateIndex(
                name: "IX_TIME_SLOT_Field_Time",
                table: "TIME_SLOT",
                columns: new[] { "field_id", "start_time" });

            // 11. USER: status + is_deleted (for active user queries)
            migrationBuilder.CreateIndex(
                name: "IX_USER_Status_IsDeleted",
                table: "USER",
                columns: new[] { "status", "is_deleted" });

            // 12. NOTIFICATION: user_id + is_read (already exists as IX_Notification_UserId_IsRead, skip)
            // migrationBuilder.CreateIndex(
            //     name: "IX_NOTIFICATION_User_IsRead",
            //     table: "NOTIFICATION",
            //     columns: new[] { "user_id", "is_read" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Drop all indexes in reverse order
            // migrationBuilder.DropIndex(
            //     name: "IX_NOTIFICATION_User_IsRead",
            //     table: "NOTIFICATION");

            migrationBuilder.DropIndex(
                name: "IX_USER_Status_IsDeleted",
                table: "USER");

            migrationBuilder.DropIndex(
                name: "IX_TIME_SLOT_Field_Time",
                table: "TIME_SLOT");

            migrationBuilder.DropIndex(
                name: "IX_REVIEW_Booking_Rating",
                table: "REVIEW");

            migrationBuilder.Sql(@"
                DROP INDEX IF EXISTS IX_OWNER_SETTING_BankInfo ON OWNER_SETTING;
            ");

            migrationBuilder.DropIndex(
                name: "IX_BOOKING_Owner_Status",
                table: "BOOKING");

            migrationBuilder.DropIndex(
                name: "IX_BOOKING_Customer_Status_Date",
                table: "BOOKING");

            migrationBuilder.DropIndex(
                name: "IX_BOOKING_BookingDate_Status",
                table: "BOOKING");

            migrationBuilder.DropIndex(
                name: "IX_BOOKING_Field_Status",
                table: "BOOKING");

            migrationBuilder.DropIndex(
                name: "IX_FIELD_Complex_Active",
                table: "FIELD");

            migrationBuilder.DropIndex(
                name: "IX_COMPLEX_Province_Ward",
                table: "COMPLEX");

            migrationBuilder.DropIndex(
                name: "IX_COMPLEX_Active_Status_Owner",
                table: "COMPLEX");
        }
    }
}
