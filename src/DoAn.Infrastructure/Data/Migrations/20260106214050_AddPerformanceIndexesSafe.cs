using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DoAn.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddPerformanceIndexesSafe : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Use raw SQL with IF NOT EXISTS to avoid errors if indexes already exist

            // 1. COMPLEX: is_active + status + owner_id
            migrationBuilder.Sql(@"
                IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_COMPLEX_Active_Status_Owner' AND object_id = OBJECT_ID('COMPLEX'))
                BEGIN
                    CREATE NONCLUSTERED INDEX IX_COMPLEX_Active_Status_Owner 
                    ON COMPLEX (is_active, status, owner_id);
                END
            ");

            // 2. COMPLEX: province + ward
            migrationBuilder.Sql(@"
                IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_COMPLEX_Province_Ward' AND object_id = OBJECT_ID('COMPLEX'))
                BEGIN
                    CREATE NONCLUSTERED INDEX IX_COMPLEX_Province_Ward 
                    ON COMPLEX (province, ward);
                END
            ");

            // 3. FIELD: complex_id + is_active
            migrationBuilder.Sql(@"
                IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_FIELD_Complex_Active' AND object_id = OBJECT_ID('FIELD'))
                BEGIN
                    CREATE NONCLUSTERED INDEX IX_FIELD_Complex_Active 
                    ON FIELD (complex_id, is_active);
                END
            ");

            // 4. BOOKING: field_id + booking_status
            migrationBuilder.Sql(@"
                IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_BOOKING_Field_Status' AND object_id = OBJECT_ID('BOOKING'))
                BEGIN
                    CREATE NONCLUSTERED INDEX IX_BOOKING_Field_Status 
                    ON BOOKING (field_id, booking_status);
                END
            ");

            // 5. BOOKING: booking_date + booking_status
            migrationBuilder.Sql(@"
                IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_BOOKING_BookingDate_Status' AND object_id = OBJECT_ID('BOOKING'))
                BEGIN
                    CREATE NONCLUSTERED INDEX IX_BOOKING_BookingDate_Status 
                    ON BOOKING (booking_date, booking_status);
                END
            ");

            // 6. BOOKING: customer_id + booking_status + booking_date
            migrationBuilder.Sql(@"
                IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_BOOKING_Customer_Status_Date' AND object_id = OBJECT_ID('BOOKING'))
                BEGIN
                    CREATE NONCLUSTERED INDEX IX_BOOKING_Customer_Status_Date 
                    ON BOOKING (customer_id, booking_status, booking_date);
                END
            ");

            // 7. BOOKING: owner_id + booking_status
            migrationBuilder.Sql(@"
                IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_BOOKING_Owner_Status' AND object_id = OBJECT_ID('BOOKING'))
                BEGIN
                    CREATE NONCLUSTERED INDEX IX_BOOKING_Owner_Status 
                    ON BOOKING (owner_id, booking_status);
                END
            ");

            // 8. OWNER_SETTING: filtered index for bank info check
            migrationBuilder.Sql(@"
                IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_OWNER_SETTING_BankInfo' AND object_id = OBJECT_ID('OWNER_SETTING'))
                BEGIN
                    CREATE NONCLUSTERED INDEX IX_OWNER_SETTING_BankInfo 
                    ON OWNER_SETTING (owner_id, bank_account_number)
                    WHERE bank_account_number IS NOT NULL;
                END
            ");

            // 9. REVIEW: booking_id + rating
            migrationBuilder.Sql(@"
                IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_REVIEW_Booking_Rating' AND object_id = OBJECT_ID('REVIEW'))
                BEGIN
                    CREATE NONCLUSTERED INDEX IX_REVIEW_Booking_Rating 
                    ON REVIEW (booking_id, rating);
                END
            ");

            // 10. TIME_SLOT: field_id + start_time
            migrationBuilder.Sql(@"
                IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_TIME_SLOT_Field_Time' AND object_id = OBJECT_ID('TIME_SLOT'))
                BEGIN
                    CREATE NONCLUSTERED INDEX IX_TIME_SLOT_Field_Time 
                    ON TIME_SLOT (field_id, start_time);
                END
            ");

            // 11. USER: status + is_deleted
            migrationBuilder.Sql(@"
                IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_USER_Status_IsDeleted' AND object_id = OBJECT_ID('USER'))
                BEGIN
                    CREATE NONCLUSTERED INDEX IX_USER_Status_IsDeleted 
                    ON [USER] (status, is_deleted);
                END
            ");

            // Note: NOTIFICATION already has IX_Notification_UserId_IsRead from NotificationConfiguration
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Drop indexes in reverse order (only if they exist)
            migrationBuilder.Sql("DROP INDEX IF EXISTS IX_USER_Status_IsDeleted ON [USER];");
            migrationBuilder.Sql("DROP INDEX IF EXISTS IX_TIME_SLOT_Field_Time ON TIME_SLOT;");
            migrationBuilder.Sql("DROP INDEX IF EXISTS IX_REVIEW_Booking_Rating ON REVIEW;");
            migrationBuilder.Sql("DROP INDEX IF EXISTS IX_OWNER_SETTING_BankInfo ON OWNER_SETTING;");
            migrationBuilder.Sql("DROP INDEX IF EXISTS IX_BOOKING_Owner_Status ON BOOKING;");
            migrationBuilder.Sql("DROP INDEX IF EXISTS IX_BOOKING_Customer_Status_Date ON BOOKING;");
            migrationBuilder.Sql("DROP INDEX IF EXISTS IX_BOOKING_BookingDate_Status ON BOOKING;");
            migrationBuilder.Sql("DROP INDEX IF EXISTS IX_BOOKING_Field_Status ON BOOKING;");
            migrationBuilder.Sql("DROP INDEX IF EXISTS IX_FIELD_Complex_Active ON FIELD;");
            migrationBuilder.Sql("DROP INDEX IF EXISTS IX_COMPLEX_Province_Ward ON COMPLEX;");
            migrationBuilder.Sql("DROP INDEX IF EXISTS IX_COMPLEX_Active_Status_Owner ON COMPLEX;");
        }
    }
}
