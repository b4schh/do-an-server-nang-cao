using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FootballField.API.Database.Migrations
{
    /// <inheritdoc />
    public partial class FixRBACDataUnicode : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Fix ROLE descriptions
            migrationBuilder.Sql(@"
                UPDATE ROLE SET description = N'Khách hàng đặt sân' WHERE name = 'Customer';
                UPDATE ROLE SET description = N'Chủ sân quản lý cụm sân' WHERE name = 'Owner';
                UPDATE ROLE SET description = N'Quản trị viên hệ thống' WHERE name = 'Admin';
            ");

            // Fix PERMISSION descriptions (chỉ update những cái bị lỗi)
            migrationBuilder.Sql(@"
                UPDATE PERMISSION SET description = N'Tạo đơn đặt sân' WHERE permission_key = 'booking.create';
                UPDATE PERMISSION SET description = N'Xem đơn đặt sân của mình' WHERE permission_key = 'booking.view_own';
                UPDATE PERMISSION SET description = N'Hủy đơn đặt sân của mình' WHERE permission_key = 'booking.cancel_own';
                UPDATE PERMISSION SET description = N'Tạo đánh giá sân' WHERE permission_key = 'review.create';
                UPDATE PERMISSION SET description = N'Sửa đánh giá của mình' WHERE permission_key = 'review.edit_own';
                UPDATE PERMISSION SET description = N'Xóa đánh giá của mình' WHERE permission_key = 'review.delete_own';
                UPDATE PERMISSION SET description = N'Đánh dấu sân yêu thích' WHERE permission_key = 'complex.favorite';
                UPDATE PERMISSION SET description = N'Xem hồ sơ cá nhân' WHERE permission_key = 'user.view_own_profile';
                UPDATE PERMISSION SET description = N'Cập nhật hồ sơ cá nhân' WHERE permission_key = 'user.update_own_profile';
                UPDATE PERMISSION SET description = N'Tạo cụm sân' WHERE permission_key = 'complex.create';
                UPDATE PERMISSION SET description = N'Sửa cụm sân của mình' WHERE permission_key = 'complex.edit_own';
                UPDATE PERMISSION SET description = N'Xóa cụm sân của mình' WHERE permission_key = 'complex.delete_own';
                UPDATE PERMISSION SET description = N'Tạo sân con' WHERE permission_key = 'field.create';
                UPDATE PERMISSION SET description = N'Sửa sân con của mình' WHERE permission_key = 'field.edit_own';
                UPDATE PERMISSION SET description = N'Xóa sân con của mình' WHERE permission_key = 'field.delete_own';
                UPDATE PERMISSION SET description = N'Tạo khung giờ' WHERE permission_key = 'timeslot.create';
                UPDATE PERMISSION SET description = N'Sửa khung giờ của mình' WHERE permission_key = 'timeslot.edit_own';
                UPDATE PERMISSION SET description = N'Xóa khung giờ của mình' WHERE permission_key = 'timeslot.delete_own';
                UPDATE PERMISSION SET description = N'Duyệt bill đặt sân' WHERE permission_key = 'booking.approve';
                UPDATE PERMISSION SET description = N'Từ chối bill đặt sân' WHERE permission_key = 'booking.reject';
                UPDATE PERMISSION SET description = N'Xem booking của cụm sân mình quản lý' WHERE permission_key = 'booking.view_own_complex';
                UPDATE PERMISSION SET description = N'Đánh dấu khách không đến' WHERE permission_key = 'booking.mark_no_show';
                UPDATE PERMISSION SET description = N'Trả lời đánh giá' WHERE permission_key = 'review.reply';
                UPDATE PERMISSION SET description = N'Quản lý cấu hình chủ sân' WHERE permission_key = 'owner_settings.manage';
                UPDATE PERMISSION SET description = N'Duyệt cụm sân' WHERE permission_key = 'complex.approve';
                UPDATE PERMISSION SET description = N'Từ chối cụm sân' WHERE permission_key = 'complex.reject';
                UPDATE PERMISSION SET description = N'Xem tất cả cụm sân' WHERE permission_key = 'complex.view_all';
                UPDATE PERMISSION SET description = N'Xóa bất kỳ cụm sân nào' WHERE permission_key = 'complex.delete_any';
                UPDATE PERMISSION SET description = N'Xem tất cả người dùng' WHERE permission_key = 'user.view_all';
                UPDATE PERMISSION SET description = N'Tạo người dùng' WHERE permission_key = 'user.create';
                UPDATE PERMISSION SET description = N'Cập nhật bất kỳ người dùng nào' WHERE permission_key = 'user.update_any';
                UPDATE PERMISSION SET description = N'Xóa bất kỳ người dùng nào' WHERE permission_key = 'user.delete_any';
                UPDATE PERMISSION SET description = N'Thay đổi role người dùng' WHERE permission_key = 'user.change_role';
                UPDATE PERMISSION SET description = N'Cấm người dùng' WHERE permission_key = 'user.ban';
                UPDATE PERMISSION SET description = N'Xem tất cả booking' WHERE permission_key = 'booking.view_all';
                UPDATE PERMISSION SET description = N'Xóa bất kỳ đánh giá nào' WHERE permission_key = 'review.delete_any';
                UPDATE PERMISSION SET description = N'Kiểm duyệt đánh giá' WHERE permission_key = 'review.moderate';
                UPDATE PERMISSION SET description = N'Xem tất cả cấu hình chủ sân' WHERE permission_key = 'owner_settings.view_all';
                UPDATE PERMISSION SET description = N'Xem tất cả roles' WHERE permission_key = 'role.view_all';
                UPDATE PERMISSION SET description = N'Tạo role mới' WHERE permission_key = 'role.create';
                UPDATE PERMISSION SET description = N'Sửa role' WHERE permission_key = 'role.edit';
                UPDATE PERMISSION SET description = N'Xóa role' WHERE permission_key = 'role.delete';
                UPDATE PERMISSION SET description = N'Xem tất cả permissions' WHERE permission_key = 'permission.view_all';
                UPDATE PERMISSION SET description = N'Tạo permission mới' WHERE permission_key = 'permission.create';
                UPDATE PERMISSION SET description = N'Sửa permission' WHERE permission_key = 'permission.edit';
                UPDATE PERMISSION SET description = N'Xóa permission' WHERE permission_key = 'permission.delete';
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // No need to revert - data fix is one-way
        }
    }
}
