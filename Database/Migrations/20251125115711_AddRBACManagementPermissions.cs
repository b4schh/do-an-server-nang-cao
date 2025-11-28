using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FootballField.API.Database.Migrations
{
    /// <inheritdoc />
    public partial class AddRBACManagementPermissions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Insert 8 RBAC Management permissions
            migrationBuilder.Sql(@"
                INSERT INTO PERMISSION (permission_key, description, module, created_at, updated_at)
                VALUES 
                    ('role.view_all', N'Xem tất cả roles', N'RBACManagement', DATEADD(HOUR, 7, GETUTCDATE()), DATEADD(HOUR, 7, GETUTCDATE())),
                    ('role.create', N'Tạo role mới', N'RBACManagement', DATEADD(HOUR, 7, GETUTCDATE()), DATEADD(HOUR, 7, GETUTCDATE())),
                    ('role.edit', N'Sửa role', N'RBACManagement', DATEADD(HOUR, 7, GETUTCDATE()), DATEADD(HOUR, 7, GETUTCDATE())),
                    ('role.delete', N'Xóa role', N'RBACManagement', DATEADD(HOUR, 7, GETUTCDATE()), DATEADD(HOUR, 7, GETUTCDATE())),
                    ('permission.view_all', N'Xem tất cả permissions', N'RBACManagement', DATEADD(HOUR, 7, GETUTCDATE()), DATEADD(HOUR, 7, GETUTCDATE())),
                    ('permission.create', N'Tạo permission mới', N'RBACManagement', DATEADD(HOUR, 7, GETUTCDATE()), DATEADD(HOUR, 7, GETUTCDATE())),
                    ('permission.edit', N'Sửa permission', N'RBACManagement', DATEADD(HOUR, 7, GETUTCDATE()), DATEADD(HOUR, 7, GETUTCDATE())),
                    ('permission.delete', N'Xóa permission', N'RBACManagement', DATEADD(HOUR, 7, GETUTCDATE()), DATEADD(HOUR, 7, GETUTCDATE()));
            ");

            // Assign all 8 RBAC permissions to Admin role (role_id = 3)
            migrationBuilder.Sql(@"
                INSERT INTO ROLE_PERMISSION (role_id, permission_id)
                SELECT 3, id FROM PERMISSION 
                WHERE permission_key IN (
                    'role.view_all', 'role.create', 'role.edit', 'role.delete',
                    'permission.view_all', 'permission.create', 'permission.edit', 'permission.delete'
                );
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Remove RBAC permissions from roles
            migrationBuilder.Sql(@"
                DELETE FROM ROLE_PERMISSION 
                WHERE permission_id IN (
                    SELECT id FROM PERMISSION WHERE permission_key IN (
                        'role.view_all', 'role.create', 'role.edit', 'role.delete',
                        'permission.view_all', 'permission.create', 'permission.edit', 'permission.delete'
                    )
                );
            ");
            
            // Remove RBAC permissions
            migrationBuilder.Sql(@"
                DELETE FROM PERMISSION 
                WHERE permission_key IN (
                    'role.view_all', 'role.create', 'role.edit', 'role.delete',
                    'permission.view_all', 'permission.create', 'permission.edit', 'permission.delete'
                );
            ");
        }
    }
}
