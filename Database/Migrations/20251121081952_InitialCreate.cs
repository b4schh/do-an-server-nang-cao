using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FootballField.API.Database.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "SYSTEM_CONFIG",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    config_key = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    config_value = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    data_type = table.Column<string>(type: "varchar(20)", unicode: false, maxLength: 20, nullable: false, defaultValue: "string"),
                    description = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    updated_at = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "DATEADD(HOUR, 7, GETUTCDATE())")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SYSTEM_CONFIG", x => x.id);
                    table.CheckConstraint("CK_SystemConfig_DataType", "data_type IN ('string','int','decimal','boolean','json')");
                });

            migrationBuilder.CreateTable(
                name: "SYSTEM_LOG",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    log_level = table.Column<string>(type: "varchar(20)", unicode: false, maxLength: 20, nullable: true),
                    source = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    message = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "DATEADD(HOUR, 7, GETUTCDATE())")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SYSTEM_LOG", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "USER",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    last_name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    first_name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    email = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    phone = table.Column<string>(type: "nvarchar(15)", maxLength: 15, nullable: true),
                    password = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    role = table.Column<byte>(type: "tinyint", nullable: false),
                    avatar_url = table.Column<string>(type: "varchar(max)", unicode: false, nullable: true),
                    status = table.Column<byte>(type: "tinyint", nullable: false),
                    is_deleted = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    deleted_by = table.Column<int>(type: "int", nullable: true),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "DATEADD(HOUR, 7, GETUTCDATE())"),
                    updated_at = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "DATEADD(HOUR, 7, GETUTCDATE())"),
                    deleted_at = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_USER", x => x.id);
                    table.ForeignKey(
                        name: "FK_USER_USER_deleted_by",
                        column: x => x.deleted_by,
                        principalTable: "USER",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "COMPLEX",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    owner_id = table.Column<int>(type: "int", nullable: false),
                    name = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    street = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    ward = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    province = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    phone = table.Column<string>(type: "nvarchar(15)", maxLength: 15, nullable: true),
                    opening_time = table.Column<TimeSpan>(type: "time", nullable: true),
                    closing_time = table.Column<TimeSpan>(type: "time", nullable: true),
                    description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    status = table.Column<byte>(type: "tinyint", nullable: false),
                    is_active = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    is_deleted = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "DATEADD(HOUR, 7, GETUTCDATE())"),
                    updated_at = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "DATEADD(HOUR, 7, GETUTCDATE())"),
                    deleted_at = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_COMPLEX", x => x.id);
                    table.ForeignKey(
                        name: "FK_COMPLEX_USER_owner_id",
                        column: x => x.owner_id,
                        principalTable: "USER",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "NOTIFICATION",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    user_id = table.Column<int>(type: "int", nullable: false),
                    sender_id = table.Column<int>(type: "int", nullable: true),
                    title = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    message = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    type = table.Column<byte>(type: "tinyint", nullable: false),
                    related_table = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    related_id = table.Column<int>(type: "int", nullable: true),
                    is_read = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "DATEADD(HOUR, 7, GETUTCDATE())"),
                    read_at = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NOTIFICATION", x => x.id);
                    table.ForeignKey(
                        name: "FK_NOTIFICATION_USER_sender_id",
                        column: x => x.sender_id,
                        principalTable: "USER",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "FK_NOTIFICATION_USER_user_id",
                        column: x => x.user_id,
                        principalTable: "USER",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "OWNER_SETTING",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    owner_id = table.Column<int>(type: "int", nullable: false),
                    deposit_rate = table.Column<decimal>(type: "decimal(5,2)", nullable: true),
                    min_booking_notice = table.Column<int>(type: "int", nullable: true),
                    allow_review = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "DATEADD(HOUR, 7, GETUTCDATE())"),
                    updated_at = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "DATEADD(HOUR, 7, GETUTCDATE())")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OWNER_SETTING", x => x.id);
                    table.CheckConstraint("CK_OwnerSetting_DepositRate", "deposit_rate BETWEEN 0 AND 1");
                    table.CheckConstraint("CK_OwnerSetting_MinBookingNotice", "min_booking_notice >= 0");
                    table.ForeignKey(
                        name: "FK_OWNER_SETTING_USER_owner_id",
                        column: x => x.owner_id,
                        principalTable: "USER",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "USER_ACTIVITY_LOG",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    user_id = table.Column<int>(type: "int", nullable: true),
                    action = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    target_table = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    target_id = table.Column<int>(type: "int", nullable: true),
                    description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "DATEADD(HOUR, 7, GETUTCDATE())")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_USER_ACTIVITY_LOG", x => x.id);
                    table.ForeignKey(
                        name: "FK_USER_ACTIVITY_LOG_USER_user_id",
                        column: x => x.user_id,
                        principalTable: "USER",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "COMPLEX_IMAGE",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    complex_id = table.Column<int>(type: "int", nullable: false),
                    image_url = table.Column<string>(type: "varchar(max)", unicode: false, nullable: false),
                    is_main = table.Column<bool>(type: "bit", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_COMPLEX_IMAGE", x => x.id);
                    table.ForeignKey(
                        name: "FK_COMPLEX_IMAGE_COMPLEX_complex_id",
                        column: x => x.complex_id,
                        principalTable: "COMPLEX",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "FIELD",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    complex_id = table.Column<int>(type: "int", nullable: false),
                    name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    surface_type = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    field_size = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    is_active = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    is_deleted = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "DATEADD(HOUR, 7, GETUTCDATE())"),
                    updated_at = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "DATEADD(HOUR, 7, GETUTCDATE())"),
                    deleted_at = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FIELD", x => x.id);
                    table.ForeignKey(
                        name: "FK_FIELD_COMPLEX_complex_id",
                        column: x => x.complex_id,
                        principalTable: "COMPLEX",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "FAVORITE_COMPLEX",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    user_id = table.Column<int>(type: "int", nullable: false),
                    complex_id = table.Column<int>(type: "int", nullable: false),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "DATEADD(HOUR, 7, GETUTCDATE())")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FAVORITE_COMPLEX", x => x.id);
                    table.ForeignKey(
                        name: "FK_FAVORITE_COMPLEX_FIELD_complex_id",
                        column: x => x.complex_id,
                        principalTable: "FIELD",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_FAVORITE_COMPLEX_USER_user_id",
                        column: x => x.user_id,
                        principalTable: "USER",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TIME_SLOT",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    field_id = table.Column<int>(type: "int", nullable: false),
                    start_time = table.Column<TimeSpan>(type: "time", nullable: false),
                    end_time = table.Column<TimeSpan>(type: "time", nullable: false),
                    price = table.Column<decimal>(type: "decimal(10,2)", nullable: false),
                    is_active = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "DATEADD(HOUR, 7, GETUTCDATE())"),
                    updated_at = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "DATEADD(HOUR, 7, GETUTCDATE())")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TIME_SLOT", x => x.id);
                    table.CheckConstraint("CK_TimeSlot_TimeRange", "start_time < end_time");
                    table.ForeignKey(
                        name: "FK_TIME_SLOT_FIELD_field_id",
                        column: x => x.field_id,
                        principalTable: "FIELD",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "BOOKING",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    field_id = table.Column<int>(type: "int", nullable: false),
                    customer_id = table.Column<int>(type: "int", nullable: false),
                    owner_id = table.Column<int>(type: "int", nullable: false),
                    time_slot_id = table.Column<int>(type: "int", nullable: false),
                    booking_date = table.Column<DateTime>(type: "datetime2", nullable: false),
                    hold_expires_at = table.Column<DateTime>(type: "datetime2", nullable: false),
                    total_amount = table.Column<decimal>(type: "decimal(10,2)", nullable: false),
                    deposit_amount = table.Column<decimal>(type: "decimal(10,2)", nullable: false),
                    payment_proof_url = table.Column<string>(type: "varchar(max)", unicode: false, nullable: true),
                    note = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    booking_status = table.Column<byte>(type: "tinyint", nullable: false),
                    approved_by = table.Column<int>(type: "int", nullable: true),
                    approved_at = table.Column<DateTime>(type: "datetime2", nullable: true),
                    cancelled_by = table.Column<int>(type: "int", nullable: true),
                    cancelled_at = table.Column<DateTime>(type: "datetime2", nullable: true),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "DATEADD(HOUR, 7, GETUTCDATE())"),
                    updated_at = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "DATEADD(HOUR, 7, GETUTCDATE())")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BOOKING", x => x.id);
                    table.CheckConstraint("CK_Booking_DepositAmount", "deposit_amount >= 0 AND deposit_amount <= total_amount");
                    table.CheckConstraint("CK_Booking_Status", "booking_status BETWEEN 0 AND 7");
                    table.CheckConstraint("CK_Booking_TotalAmount", "total_amount > 0");
                    table.ForeignKey(
                        name: "FK_BOOKING_FIELD_field_id",
                        column: x => x.field_id,
                        principalTable: "FIELD",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "FK_BOOKING_TIME_SLOT_time_slot_id",
                        column: x => x.time_slot_id,
                        principalTable: "TIME_SLOT",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "FK_BOOKING_USER_approved_by",
                        column: x => x.approved_by,
                        principalTable: "USER",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "FK_BOOKING_USER_cancelled_by",
                        column: x => x.cancelled_by,
                        principalTable: "USER",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "FK_BOOKING_USER_customer_id",
                        column: x => x.customer_id,
                        principalTable: "USER",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "FK_BOOKING_USER_owner_id",
                        column: x => x.owner_id,
                        principalTable: "USER",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "REVIEW",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    booking_id = table.Column<int>(type: "int", nullable: false),
                    rating = table.Column<byte>(type: "tinyint", nullable: false),
                    comment = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    is_visible = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    is_deleted = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "DATEADD(HOUR, 7, GETUTCDATE())"),
                    updated_at = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "DATEADD(HOUR, 7, GETUTCDATE())"),
                    deleted_at = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_REVIEW", x => x.id);
                    table.CheckConstraint("CK_Review_Rating", "rating BETWEEN 1 AND 5");
                    table.ForeignKey(
                        name: "FK_REVIEW_BOOKING_booking_id",
                        column: x => x.booking_id,
                        principalTable: "BOOKING",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_BOOKING_approved_by",
                table: "BOOKING",
                column: "approved_by");

            migrationBuilder.CreateIndex(
                name: "IX_Booking_BookingDate_Status",
                table: "BOOKING",
                columns: new[] { "booking_date", "booking_status" });

            migrationBuilder.CreateIndex(
                name: "IX_BOOKING_cancelled_by",
                table: "BOOKING",
                column: "cancelled_by");

            migrationBuilder.CreateIndex(
                name: "IX_Booking_CustomerId",
                table: "BOOKING",
                column: "customer_id");

            migrationBuilder.CreateIndex(
                name: "IX_Booking_OwnerId",
                table: "BOOKING",
                column: "owner_id");

            migrationBuilder.CreateIndex(
                name: "IX_BOOKING_time_slot_id",
                table: "BOOKING",
                column: "time_slot_id");

            migrationBuilder.CreateIndex(
                name: "IX_Booking_UniqueActiveSlot",
                table: "BOOKING",
                columns: new[] { "field_id", "booking_date", "time_slot_id" },
                unique: true,
                filter: "[booking_status] IN (0, 1, 2)");

            migrationBuilder.CreateIndex(
                name: "IX_Complex_OwnerId",
                table: "COMPLEX",
                column: "owner_id");

            migrationBuilder.CreateIndex(
                name: "IX_COMPLEX_IMAGE_complex_id",
                table: "COMPLEX_IMAGE",
                column: "complex_id");

            migrationBuilder.CreateIndex(
                name: "IX_FAVORITE_COMPLEX_complex_id",
                table: "FAVORITE_COMPLEX",
                column: "complex_id");

            migrationBuilder.CreateIndex(
                name: "IX_FAVORITE_COMPLEX_user_id_complex_id",
                table: "FAVORITE_COMPLEX",
                columns: new[] { "user_id", "complex_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Field_ComplexId",
                table: "FIELD",
                column: "complex_id");

            migrationBuilder.CreateIndex(
                name: "IX_NOTIFICATION_sender_id",
                table: "NOTIFICATION",
                column: "sender_id");

            migrationBuilder.CreateIndex(
                name: "IX_Notification_UserId_IsRead",
                table: "NOTIFICATION",
                columns: new[] { "user_id", "is_read" });

            migrationBuilder.CreateIndex(
                name: "IX_OWNER_SETTING_owner_id",
                table: "OWNER_SETTING",
                column: "owner_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_REVIEW_booking_id",
                table: "REVIEW",
                column: "booking_id");

            migrationBuilder.CreateIndex(
                name: "IX_SYSTEM_CONFIG_config_key",
                table: "SYSTEM_CONFIG",
                column: "config_key",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TIME_SLOT_field_id_start_time_end_time",
                table: "TIME_SLOT",
                columns: new[] { "field_id", "start_time", "end_time" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TimeSlot_FieldId",
                table: "TIME_SLOT",
                column: "field_id");

            migrationBuilder.CreateIndex(
                name: "IX_USER_deleted_by",
                table: "USER",
                column: "deleted_by");

            migrationBuilder.CreateIndex(
                name: "IX_USER_email",
                table: "USER",
                column: "email",
                unique: true,
                filter: "[email] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_USER_ACTIVITY_LOG_user_id",
                table: "USER_ACTIVITY_LOG",
                column: "user_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "COMPLEX_IMAGE");

            migrationBuilder.DropTable(
                name: "FAVORITE_COMPLEX");

            migrationBuilder.DropTable(
                name: "NOTIFICATION");

            migrationBuilder.DropTable(
                name: "OWNER_SETTING");

            migrationBuilder.DropTable(
                name: "REVIEW");

            migrationBuilder.DropTable(
                name: "SYSTEM_CONFIG");

            migrationBuilder.DropTable(
                name: "SYSTEM_LOG");

            migrationBuilder.DropTable(
                name: "USER_ACTIVITY_LOG");

            migrationBuilder.DropTable(
                name: "BOOKING");

            migrationBuilder.DropTable(
                name: "TIME_SLOT");

            migrationBuilder.DropTable(
                name: "FIELD");

            migrationBuilder.DropTable(
                name: "COMPLEX");

            migrationBuilder.DropTable(
                name: "USER");
        }
    }
}
