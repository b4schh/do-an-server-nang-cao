using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FootballField.API.Database.Migrations
{
    /// <inheritdoc />
    public partial class AddRBACSystem : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<byte>(
                name: "role",
                table: "USER",
                type: "tinyint",
                nullable: true,
                oldClrType: typeof(byte),
                oldType: "tinyint");

            migrationBuilder.CreateTable(
                name: "PERMISSION",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    permission_key = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    description = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    module = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "DATEADD(HOUR, 7, GETUTCDATE())"),
                    updated_at = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "DATEADD(HOUR, 7, GETUTCDATE())")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PERMISSION", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "ROLE",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    name = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    description = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    is_active = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "DATEADD(HOUR, 7, GETUTCDATE())"),
                    updated_at = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "DATEADD(HOUR, 7, GETUTCDATE())")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ROLE", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "ROLE_PERMISSION",
                columns: table => new
                {
                    role_id = table.Column<int>(type: "int", nullable: false),
                    permission_id = table.Column<int>(type: "int", nullable: false),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "DATEADD(HOUR, 7, GETUTCDATE())")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ROLE_PERMISSION", x => new { x.role_id, x.permission_id });
                    table.ForeignKey(
                        name: "FK_ROLE_PERMISSION_PERMISSION_permission_id",
                        column: x => x.permission_id,
                        principalTable: "PERMISSION",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ROLE_PERMISSION_ROLE_role_id",
                        column: x => x.role_id,
                        principalTable: "ROLE",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "USER_ROLE",
                columns: table => new
                {
                    user_id = table.Column<int>(type: "int", nullable: false),
                    role_id = table.Column<int>(type: "int", nullable: false),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "DATEADD(HOUR, 7, GETUTCDATE())")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_USER_ROLE", x => new { x.user_id, x.role_id });
                    table.ForeignKey(
                        name: "FK_USER_ROLE_ROLE_role_id",
                        column: x => x.role_id,
                        principalTable: "ROLE",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_USER_ROLE_USER_user_id",
                        column: x => x.user_id,
                        principalTable: "USER",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PERMISSION_permission_key",
                table: "PERMISSION",
                column: "permission_key",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ROLE_name",
                table: "ROLE",
                column: "name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ROLE_PERMISSION_permission_id",
                table: "ROLE_PERMISSION",
                column: "permission_id");

            migrationBuilder.CreateIndex(
                name: "IX_USER_ROLE_role_id",
                table: "USER_ROLE",
                column: "role_id");

            // ==========================
            // DATA MIGRATION: Seed Roles
            // ==========================
            migrationBuilder.Sql(@"
                INSERT INTO ROLE (name, description, is_active, created_at, updated_at)
                VALUES 
                    ('Customer', 'Khách hàng đặt sân', 1, DATEADD(HOUR, 7, GETUTCDATE()), DATEADD(HOUR, 7, GETUTCDATE())),
                    ('Owner', 'Chủ sân quản lý cụm sân', 1, DATEADD(HOUR, 7, GETUTCDATE()), DATEADD(HOUR, 7, GETUTCDATE())),
                    ('Admin', 'Quản trị viên hệ thống', 1, DATEADD(HOUR, 7, GETUTCDATE()), DATEADD(HOUR, 7, GETUTCDATE()))
            ");

            // ================================
            // DATA MIGRATION: Seed Permissions
            // ================================
            migrationBuilder.Sql(@"
                INSERT INTO PERMISSION (permission_key, description, module, created_at, updated_at)
                VALUES 
                    -- Customer permissions
                    ('booking.create', 'Tạo đơn đặt sân', 'BookingManagement', DATEADD(HOUR, 7, GETUTCDATE()), DATEADD(HOUR, 7, GETUTCDATE())),
                    ('booking.view_own', 'Xem đơn đặt sân của mình', 'BookingManagement', DATEADD(HOUR, 7, GETUTCDATE()), DATEADD(HOUR, 7, GETUTCDATE())),
                    ('booking.cancel_own', 'Hủy đơn đặt sân của mình', 'BookingManagement', DATEADD(HOUR, 7, GETUTCDATE()), DATEADD(HOUR, 7, GETUTCDATE())),
                    ('booking.upload_payment', 'Upload bill thanh toán', 'BookingManagement', DATEADD(HOUR, 7, GETUTCDATE()), DATEADD(HOUR, 7, GETUTCDATE())),
                    ('review.create', 'Tạo đánh giá sân', 'ReviewManagement', DATEADD(HOUR, 7, GETUTCDATE()), DATEADD(HOUR, 7, GETUTCDATE())),
                    ('review.edit_own', 'Sửa đánh giá của mình', 'ReviewManagement', DATEADD(HOUR, 7, GETUTCDATE()), DATEADD(HOUR, 7, GETUTCDATE())),
                    ('review.delete_own', 'Xóa đánh giá của mình', 'ReviewManagement', DATEADD(HOUR, 7, GETUTCDATE()), DATEADD(HOUR, 7, GETUTCDATE())),
                    ('complex.favorite', 'Đánh dấu sân yêu thích', 'ComplexManagement', DATEADD(HOUR, 7, GETUTCDATE()), DATEADD(HOUR, 7, GETUTCDATE())),
                    ('user.view_own_profile', 'Xem hồ sơ cá nhân', 'UserManagement', DATEADD(HOUR, 7, GETUTCDATE()), DATEADD(HOUR, 7, GETUTCDATE())),
                    ('user.update_own_profile', 'Cập nhật hồ sơ cá nhân', 'UserManagement', DATEADD(HOUR, 7, GETUTCDATE()), DATEADD(HOUR, 7, GETUTCDATE())),
                    
                    -- Owner permissions
                    ('complex.create', 'Tạo cụm sân', 'ComplexManagement', DATEADD(HOUR, 7, GETUTCDATE()), DATEADD(HOUR, 7, GETUTCDATE())),
                    ('complex.edit_own', 'Sửa cụm sân của mình', 'ComplexManagement', DATEADD(HOUR, 7, GETUTCDATE()), DATEADD(HOUR, 7, GETUTCDATE())),
                    ('complex.delete_own', 'Xóa cụm sân của mình', 'ComplexManagement', DATEADD(HOUR, 7, GETUTCDATE()), DATEADD(HOUR, 7, GETUTCDATE())),
                    ('complex.upload_images', 'Upload ảnh cụm sân', 'ComplexManagement', DATEADD(HOUR, 7, GETUTCDATE()), DATEADD(HOUR, 7, GETUTCDATE())),
                    ('field.create', 'Tạo sân con', 'FieldManagement', DATEADD(HOUR, 7, GETUTCDATE()), DATEADD(HOUR, 7, GETUTCDATE())),
                    ('field.edit_own', 'Sửa sân con của mình', 'FieldManagement', DATEADD(HOUR, 7, GETUTCDATE()), DATEADD(HOUR, 7, GETUTCDATE())),
                    ('field.delete_own', 'Xóa sân con của mình', 'FieldManagement', DATEADD(HOUR, 7, GETUTCDATE()), DATEADD(HOUR, 7, GETUTCDATE())),
                    ('timeslot.create', 'Tạo khung giờ', 'FieldManagement', DATEADD(HOUR, 7, GETUTCDATE()), DATEADD(HOUR, 7, GETUTCDATE())),
                    ('timeslot.edit_own', 'Sửa khung giờ của mình', 'FieldManagement', DATEADD(HOUR, 7, GETUTCDATE()), DATEADD(HOUR, 7, GETUTCDATE())),
                    ('timeslot.delete_own', 'Xóa khung giờ của mình', 'FieldManagement', DATEADD(HOUR, 7, GETUTCDATE()), DATEADD(HOUR, 7, GETUTCDATE())),
                    ('booking.approve', 'Duyệt bill đặt sân', 'BookingManagement', DATEADD(HOUR, 7, GETUTCDATE()), DATEADD(HOUR, 7, GETUTCDATE())),
                    ('booking.reject', 'Từ chối bill đặt sân', 'BookingManagement', DATEADD(HOUR, 7, GETUTCDATE()), DATEADD(HOUR, 7, GETUTCDATE())),
                    ('booking.view_own_complex', 'Xem booking của cụm sân mình quản lý', 'BookingManagement', DATEADD(HOUR, 7, GETUTCDATE()), DATEADD(HOUR, 7, GETUTCDATE())),
                    ('booking.mark_no_show', 'Đánh dấu khách không đến', 'BookingManagement', DATEADD(HOUR, 7, GETUTCDATE()), DATEADD(HOUR, 7, GETUTCDATE())),
                    ('review.reply', 'Trả lời đánh giá', 'ReviewManagement', DATEADD(HOUR, 7, GETUTCDATE()), DATEADD(HOUR, 7, GETUTCDATE())),
                    ('owner_settings.manage', 'Quản lý cấu hình chủ sân', 'OwnerSettingsManagement', DATEADD(HOUR, 7, GETUTCDATE()), DATEADD(HOUR, 7, GETUTCDATE())),
                    
                    -- Admin permissions
                    ('complex.approve', 'Duyệt cụm sân', 'ComplexManagement', DATEADD(HOUR, 7, GETUTCDATE()), DATEADD(HOUR, 7, GETUTCDATE())),
                    ('complex.reject', 'Từ chối cụm sân', 'ComplexManagement', DATEADD(HOUR, 7, GETUTCDATE()), DATEADD(HOUR, 7, GETUTCDATE())),
                    ('complex.view_all', 'Xem tất cả cụm sân', 'ComplexManagement', DATEADD(HOUR, 7, GETUTCDATE()), DATEADD(HOUR, 7, GETUTCDATE())),
                    ('complex.delete_any', 'Xóa bất kỳ cụm sân nào', 'ComplexManagement', DATEADD(HOUR, 7, GETUTCDATE()), DATEADD(HOUR, 7, GETUTCDATE())),
                    ('user.view_all', 'Xem tất cả người dùng', 'UserManagement', DATEADD(HOUR, 7, GETUTCDATE()), DATEADD(HOUR, 7, GETUTCDATE())),
                    ('user.create', 'Tạo người dùng', 'UserManagement', DATEADD(HOUR, 7, GETUTCDATE()), DATEADD(HOUR, 7, GETUTCDATE())),
                    ('user.update_any', 'Cập nhật bất kỳ người dùng nào', 'UserManagement', DATEADD(HOUR, 7, GETUTCDATE()), DATEADD(HOUR, 7, GETUTCDATE())),
                    ('user.delete_any', 'Xóa bất kỳ người dùng nào', 'UserManagement', DATEADD(HOUR, 7, GETUTCDATE()), DATEADD(HOUR, 7, GETUTCDATE())),
                    ('user.change_role', 'Thay đổi role người dùng', 'UserManagement', DATEADD(HOUR, 7, GETUTCDATE()), DATEADD(HOUR, 7, GETUTCDATE())),
                    ('user.ban', 'Cấm người dùng', 'UserManagement', DATEADD(HOUR, 7, GETUTCDATE()), DATEADD(HOUR, 7, GETUTCDATE())),
                    ('booking.view_all', 'Xem tất cả booking', 'BookingManagement', DATEADD(HOUR, 7, GETUTCDATE()), DATEADD(HOUR, 7, GETUTCDATE())),
                    ('booking.force_complete', 'Ép hoàn thành booking (testing)', 'BookingManagement', DATEADD(HOUR, 7, GETUTCDATE()), DATEADD(HOUR, 7, GETUTCDATE())),
                    ('review.delete_any', 'Xóa bất kỳ đánh giá nào', 'ReviewManagement', DATEADD(HOUR, 7, GETUTCDATE()), DATEADD(HOUR, 7, GETUTCDATE())),
                    ('review.moderate', 'Kiểm duyệt đánh giá', 'ReviewManagement', DATEADD(HOUR, 7, GETUTCDATE()), DATEADD(HOUR, 7, GETUTCDATE())),
                    ('owner_settings.view_all', 'Xem tất cả cấu hình chủ sân', 'OwnerSettingsManagement', DATEADD(HOUR, 7, GETUTCDATE()), DATEADD(HOUR, 7, GETUTCDATE())),
                    ('system.view_logs', 'Xem system logs', 'SystemManagement', DATEADD(HOUR, 7, GETUTCDATE()), DATEADD(HOUR, 7, GETUTCDATE())),
                    ('system.manage_config', 'Quản lý cấu hình hệ thống', 'SystemManagement', DATEADD(HOUR, 7, GETUTCDATE()), DATEADD(HOUR, 7, GETUTCDATE()))
            ");

            // ===========================================
            // DATA MIGRATION: Assign Permissions to Roles
            // ===========================================
            migrationBuilder.Sql(@"
                -- Customer permissions (role_id = 1)
                INSERT INTO ROLE_PERMISSION (role_id, permission_id, created_at)
                SELECT 1, id, DATEADD(HOUR, 7, GETUTCDATE())
                FROM PERMISSION
                WHERE permission_key IN (
                    'booking.create', 'booking.view_own', 'booking.cancel_own', 'booking.upload_payment',
                    'review.create', 'review.edit_own', 'review.delete_own',
                    'complex.favorite',
                    'user.view_own_profile', 'user.update_own_profile'
                );

                -- Owner permissions (role_id = 2) - includes all Customer permissions + Owner-specific
                INSERT INTO ROLE_PERMISSION (role_id, permission_id, created_at)
                SELECT 2, id, DATEADD(HOUR, 7, GETUTCDATE())
                FROM PERMISSION
                WHERE permission_key IN (
                    'booking.create', 'booking.view_own', 'booking.cancel_own', 'booking.upload_payment',
                    'review.create', 'review.edit_own', 'review.delete_own',
                    'complex.favorite',
                    'user.view_own_profile', 'user.update_own_profile',
                    'complex.create', 'complex.edit_own', 'complex.delete_own', 'complex.upload_images',
                    'field.create', 'field.edit_own', 'field.delete_own',
                    'timeslot.create', 'timeslot.edit_own', 'timeslot.delete_own',
                    'booking.approve', 'booking.reject', 'booking.view_own_complex', 'booking.mark_no_show',
                    'review.reply',
                    'owner_settings.manage'
                );

                -- Admin permissions (role_id = 3) - all permissions
                INSERT INTO ROLE_PERMISSION (role_id, permission_id, created_at)
                SELECT 3, id, DATEADD(HOUR, 7, GETUTCDATE())
                FROM PERMISSION;
            ");

            // =====================================================
            // DATA MIGRATION: Migrate existing users to USER_ROLE
            // =====================================================
            migrationBuilder.Sql(@"
                -- Migrate existing users based on their old 'role' column
                INSERT INTO USER_ROLE (user_id, role_id, created_at)
                SELECT id, 
                       CASE 
                           WHEN role = 0 THEN 1  -- Customer
                           WHEN role = 1 THEN 2  -- Owner
                           WHEN role = 2 THEN 3  -- Admin
                       END,
                       DATEADD(HOUR, 7, GETUTCDATE())
                FROM [USER]
                WHERE role IS NOT NULL;
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ROLE_PERMISSION");

            migrationBuilder.DropTable(
                name: "USER_ROLE");

            migrationBuilder.DropTable(
                name: "PERMISSION");

            migrationBuilder.DropTable(
                name: "ROLE");

            migrationBuilder.AlterColumn<byte>(
                name: "role",
                table: "USER",
                type: "tinyint",
                nullable: false,
                defaultValue: (byte)0,
                oldClrType: typeof(byte),
                oldType: "tinyint",
                oldNullable: true);
        }
    }
}
