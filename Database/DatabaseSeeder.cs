

using FootballField.API.Modules.ComplexManagement.Entities;
using FootballField.API.Modules.FieldManagement.Entities;
using FootballField.API.Modules.UserManagement.Entities;

namespace FootballField.API.Database;

public static class DatabaseSeeder
{
    public static void SeedData(this ApplicationDbContext context)
    {
        // Seed RBAC data first (Roles, Permissions, RolePermissions)
        SeedRBACData(context);
        
        if (context.Users.Any()) return; // Không seed nếu đã có dữ liệu

        // 1. SEED USERS (1 Admin + 3 Owners + 3 Customers)
        var admin = new User
        {
            LastName = "Admin",
            FirstName = "System",
            Email = "admin@gmail.com",
            Phone = "0900000000",
            Password = BCrypt.Net.BCrypt.HashPassword("Admin@123"),
            Status = UserStatus.Active
        };

        var admin2 = new User
        {
            LastName = "Admin",
            FirstName = "System 2",
            Email = "admin",
            Phone = "0900000001",
            Password = BCrypt.Net.BCrypt.HashPassword("123123"),
            Status = UserStatus.Active
        };

        var owner = new User
        {
            LastName = "Nguyễn",
            FirstName = "I Vân",
            Email = "owner",
            Phone = "0901111112",
            Password = BCrypt.Net.BCrypt.HashPassword("123123"),
            Status = UserStatus.Active
        };

        var owner1 = new User
        {
            LastName = "Nguyễn",
            FirstName = "Văn A",
            Email = "owner1@gmail.com",
            Phone = "0901111111",
            Password = BCrypt.Net.BCrypt.HashPassword("Owner@123"),
            Status = UserStatus.Active
        };

        var owner2 = new User
        {
            LastName = "Trần",
            FirstName = "Thị B",
            Email = "owner2@gmail.com",
            Phone = "0902222222",
            Password = BCrypt.Net.BCrypt.HashPassword("Owner@123"),
            Status = UserStatus.Active
        };

        var owner3 = new User
        {
            LastName = "Lê",
            FirstName = "Văn C",
            Email = "owner3@gmail.com",
            Phone = "0903333333",
            Password = BCrypt.Net.BCrypt.HashPassword("Owner@123"),
            Status = UserStatus.Active
        };

        var customer1 = new User
        {
            LastName = "Phạm",
            FirstName = "Văn D",
            Email = "customer1@gmail.com",
            Phone = "0904444444",
            Password = BCrypt.Net.BCrypt.HashPassword("Customer@123"),
            Status = UserStatus.Active
        };

        var customer2 = new User
        {
            LastName = "Hoàng",
            FirstName = "Thị E",
            Email = "customer2@gmail.com",
            Phone = "0905555555",
            Password = BCrypt.Net.BCrypt.HashPassword("Customer@123"),
            Status = UserStatus.Active
        };

        var customer3 = new User
        {
            LastName = "Vũ",
            FirstName = "Văn F",
            Email = "customer3@gmail.com",
            Phone = "0906666666",
            Password = BCrypt.Net.BCrypt.HashPassword("Customer@123"),
            Status = UserStatus.Active
        };

        context.Users.AddRange(admin, admin2, owner, owner1, owner2, owner3, customer1, customer2, customer3);
        context.SaveChanges();

        // Assign roles to users via USER_ROLE table
        var roles = context.Roles.ToList();
        var adminRoleId = roles.First(r => r.Name == "Admin").Id;
        var ownerRoleId = roles.First(r => r.Name == "Owner").Id;
        var customerRoleId = roles.First(r => r.Name == "Customer").Id;

        context.UserRoles.AddRange(
            new UserRole { UserId = admin.Id, RoleId = adminRoleId },
            new UserRole { UserId = admin2.Id, RoleId = adminRoleId },
            new UserRole { UserId = owner.Id, RoleId = ownerRoleId },
            new UserRole { UserId = owner1.Id, RoleId = ownerRoleId },
            new UserRole { UserId = owner2.Id, RoleId = ownerRoleId },
            new UserRole { UserId = owner3.Id, RoleId = ownerRoleId },
            new UserRole { UserId = customer1.Id, RoleId = customerRoleId },
            new UserRole { UserId = customer2.Id, RoleId = customerRoleId },
            new UserRole { UserId = customer3.Id, RoleId = customerRoleId }
        );
        context.SaveChanges();

        // 2. SEED COMPLEXES (3 cụm sân)
        var complex1 = new Complex
        {
            OwnerId = 2,
            Name = "Sân Bóng Thủ Đô",
            Street = "123 Kim Mã",
            Ward = "Ba Đình ",
            Province = "Hà Nội",
            Phone = "0281234567",
            OpeningTime = new TimeSpan(6, 0, 0),
            ClosingTime = new TimeSpan(22, 0, 0),
            Description = "Sân bóng chất lượng cao tại trung tâm thành phố",
            Status = ComplexStatus.Approved,
            IsActive = true,
        };

        var complex2 = new Complex
        {
            OwnerId = 3,
            Name = "Sân Bóng BKX",
            Street = "456 Tạ Quang Bửu",
            Ward = "Bạch Mai",
            Province = "Hà Nội",
            Phone = "0282345678",
            OpeningTime = new TimeSpan(5, 30, 0),
            ClosingTime = new TimeSpan(23, 0, 0),
            Description = "Sân bóng hiện đại, đầy đủ tiện nghi",
            Status = ComplexStatus.Approved,
            IsActive = true,
        };

        var complex3 = new Complex
        {
            OwnerId = 4,
            Name = "Sân Bóng Đầm Hồng",
            Street = "789 Trường Chinh",
            Ward = "Khương Đình",
            Province = "Hà Nội",
            Phone = "0283456789",
            OpeningTime = new TimeSpan(6, 0, 0),
            ClosingTime = new TimeSpan(22, 30, 0),
            Description = "Sân bóng rộng rãi, thoáng mát",
            Status = ComplexStatus.Approved,
            IsActive = true,
        };

        context.Complexes.AddRange(complex1, complex2, complex3);
        context.SaveChanges();

        // 3. SEED FIELDS (3-5 sân mỗi cụm)
        var fields = new List<Field>();

        // Complex 1 - 4 sân
        for (int i = 1; i <= 4; i++)
        {
            fields.Add(new Field
            {
                ComplexId = 1,
                Name = $"Sân {i}",
                SurfaceType = i % 2 == 0 ? "Cỏ nhân tạo" : "Cỏ tự nhiên",
                FieldSize = i <= 2 ? "Sân 5" : "Sân 7",
                IsActive = true,
            });
        }

        // Complex 2 - 5 sân
        for (int i = 1; i <= 5; i++)
        {
            fields.Add(new Field
            {
                ComplexId = 2,
                Name = $"Sân {i}",
                SurfaceType = "Cỏ nhân tạo",
                FieldSize = i <= 2 ? "Sân 5" : (i <= 4 ? "Sân 7" : "Sân 11"),
                IsActive = true,
            });
        }

        // Complex 3 - 3 sân
        for (int i = 1; i <= 3; i++)
        {
            fields.Add(new Field
            {
                ComplexId = 3,
                Name = $"Sân {i}",
                SurfaceType = "Cỏ nhân tạo",
                FieldSize = i == 1 ? "Sân 5" : "Sân 7",
                IsActive = true,
            });
        }

        context.Fields.AddRange(fields);
        context.SaveChanges();

        // 4. SEED TIME SLOTS (chia khung 1h30 từ 6:00 đến 22:00)
        var timeSlots = new List<TimeSlot>();

        foreach (var field in fields)
        {
            var basePrice = field.FieldSize == "Sân 5"
                ? 300000m
                : (field.FieldSize == "Sân 7" ? 500000m : 800000m);

            var startOfDay = new TimeSpan(6, 0, 0);
            var endOfDay = new TimeSpan(23, 59, 0); // end of day
            var slotDuration = new TimeSpan(1, 30, 0); // 1h30

            for (var start = startOfDay; start + slotDuration <= endOfDay; start += slotDuration)
            {
                var end = start + slotDuration;

                decimal priceMultiplier;

                if (start.Hours < 8)
                    priceMultiplier = 0.8m;
                else if (start.Hours < 17)
                    priceMultiplier = 1.0m;
                else if (start.Hours < 20)
                    priceMultiplier = 1.3m;
                else
                    priceMultiplier = 1.2m;

                timeSlots.Add(new TimeSlot
                {
                    FieldId = field.Id,
                    StartTime = start,
                    EndTime = end,
                    Price = basePrice * priceMultiplier,
                    IsActive = true,
                });
            }
        }

        context.TimeSlots.AddRange(timeSlots);
        context.SaveChanges();

    }

    private static void SeedRBACData(ApplicationDbContext context)
    {
        // Skip if roles already exist
        if (context.Roles.Any()) return;

        // Create roles
        var customerRole = new Role { Name = "Customer", Description = "Khách hàng đặt sân", IsActive = true };
        var ownerRole = new Role { Name = "Owner", Description = "Chủ sân quản lý cụm sân", IsActive = true };
        var adminRole = new Role { Name = "Admin", Description = "Quản trị viên hệ thống", IsActive = true };

        context.Roles.AddRange(customerRole, ownerRole, adminRole);
        context.SaveChanges();

        // Create permissions
        var permissions = new List<Permission>
        {
            // Customer permissions
            new Permission { PermissionKey = "booking.create", Description = "Tạo đơn đặt sân", Module = "BookingManagement" },
            new Permission { PermissionKey = "booking.view_own", Description = "Xem đơn đặt sân của mình", Module = "BookingManagement" },
            new Permission { PermissionKey = "booking.cancel_own", Description = "Hủy đơn đặt sân của mình", Module = "BookingManagement" },
            new Permission { PermissionKey = "booking.upload_payment", Description = "Upload bill thanh toán", Module = "BookingManagement" },
            new Permission { PermissionKey = "review.create", Description = "Tạo đánh giá sân", Module = "ReviewManagement" },
            new Permission { PermissionKey = "review.edit_own", Description = "Sửa đánh giá của mình", Module = "ReviewManagement" },
            new Permission { PermissionKey = "review.delete_own", Description = "Xóa đánh giá của mình", Module = "ReviewManagement" },
            new Permission { PermissionKey = "complex.favorite", Description = "Đánh dấu sân yêu thích", Module = "ComplexManagement" },
            new Permission { PermissionKey = "user.view_own_profile", Description = "Xem hồ sơ cá nhân", Module = "UserManagement" },
            new Permission { PermissionKey = "user.update_own_profile", Description = "Cập nhật hồ sơ cá nhân", Module = "UserManagement" },

            // Owner permissions
            new Permission { PermissionKey = "complex.create", Description = "Tạo cụm sân", Module = "ComplexManagement" },
            new Permission { PermissionKey = "complex.edit_own", Description = "Sửa cụm sân của mình", Module = "ComplexManagement" },
            new Permission { PermissionKey = "complex.delete_own", Description = "Xóa cụm sân của mình", Module = "ComplexManagement" },
            new Permission { PermissionKey = "complex.upload_images", Description = "Upload ảnh cụm sân", Module = "ComplexManagement" },
            new Permission { PermissionKey = "field.create", Description = "Tạo sân con", Module = "FieldManagement" },
            new Permission { PermissionKey = "field.edit_own", Description = "Sửa sân con của mình", Module = "FieldManagement" },
            new Permission { PermissionKey = "field.delete_own", Description = "Xóa sân con của mình", Module = "FieldManagement" },
            new Permission { PermissionKey = "timeslot.create", Description = "Tạo khung giờ", Module = "FieldManagement" },
            new Permission { PermissionKey = "timeslot.edit_own", Description = "Sửa khung giờ của mình", Module = "FieldManagement" },
            new Permission { PermissionKey = "timeslot.delete_own", Description = "Xóa khung giờ của mình", Module = "FieldManagement" },
            new Permission { PermissionKey = "booking.approve", Description = "Duyệt bill đặt sân", Module = "BookingManagement" },
            new Permission { PermissionKey = "booking.reject", Description = "Từ chối bill đặt sân", Module = "BookingManagement" },
            new Permission { PermissionKey = "booking.view_own_complex", Description = "Xem booking của cụm sân mình quản lý", Module = "BookingManagement" },
            new Permission { PermissionKey = "booking.mark_no_show", Description = "Đánh dấu khách không đến", Module = "BookingManagement" },
            new Permission { PermissionKey = "review.reply", Description = "Trả lời đánh giá", Module = "ReviewManagement" },
            new Permission { PermissionKey = "owner_settings.manage", Description = "Quản lý cấu hình chủ sân", Module = "OwnerSettingsManagement" },

            // Admin permissions
            new Permission { PermissionKey = "complex.approve", Description = "Duyệt cụm sân", Module = "ComplexManagement" },
            new Permission { PermissionKey = "complex.reject", Description = "Từ chối cụm sân", Module = "ComplexManagement" },
            new Permission { PermissionKey = "complex.view_all", Description = "Xem tất cả cụm sân", Module = "ComplexManagement" },
            new Permission { PermissionKey = "complex.delete_any", Description = "Xóa bất kỳ cụm sân nào", Module = "ComplexManagement" },
            new Permission { PermissionKey = "user.view_all", Description = "Xem tất cả người dùng", Module = "UserManagement" },
            new Permission { PermissionKey = "user.create", Description = "Tạo người dùng", Module = "UserManagement" },
            new Permission { PermissionKey = "user.update_any", Description = "Cập nhật bất kỳ người dùng nào", Module = "UserManagement" },
            new Permission { PermissionKey = "user.delete_any", Description = "Xóa bất kỳ người dùng nào", Module = "UserManagement" },
            new Permission { PermissionKey = "user.change_role", Description = "Thay đổi role người dùng", Module = "UserManagement" },
            new Permission { PermissionKey = "user.ban", Description = "Cấm người dùng", Module = "UserManagement" },
            new Permission { PermissionKey = "booking.view_all", Description = "Xem tất cả booking", Module = "BookingManagement" },
            new Permission { PermissionKey = "booking.force_complete", Description = "Ép hoàn thành booking (testing)", Module = "BookingManagement" },
            new Permission { PermissionKey = "review.delete_any", Description = "Xóa bất kỳ đánh giá nào", Module = "ReviewManagement" },
            new Permission { PermissionKey = "review.moderate", Description = "Kiểm duyệt đánh giá", Module = "ReviewManagement" },
            new Permission { PermissionKey = "owner_settings.view_all", Description = "Xem tất cả cấu hình chủ sân", Module = "OwnerSettingsManagement" },
            new Permission { PermissionKey = "system.view_logs", Description = "Xem system logs", Module = "SystemManagement" },
            new Permission { PermissionKey = "system.manage_config", Description = "Quản lý cấu hình hệ thống", Module = "SystemManagement" },
            
            // RBAC Management permissions (Admin only)
            new Permission { PermissionKey = "role.view_all", Description = "Xem tất cả roles", Module = "RBACManagement" },
            new Permission { PermissionKey = "role.create", Description = "Tạo role mới", Module = "RBACManagement" },
            new Permission { PermissionKey = "role.edit", Description = "Sửa role", Module = "RBACManagement" },
            new Permission { PermissionKey = "role.delete", Description = "Xóa role", Module = "RBACManagement" },
            new Permission { PermissionKey = "permission.view_all", Description = "Xem tất cả permissions", Module = "RBACManagement" },
            new Permission { PermissionKey = "permission.create", Description = "Tạo permission mới", Module = "RBACManagement" },
            new Permission { PermissionKey = "permission.edit", Description = "Sửa permission", Module = "RBACManagement" },
            new Permission { PermissionKey = "permission.delete", Description = "Xóa permission", Module = "RBACManagement" }
        };

        context.Permissions.AddRange(permissions);
        context.SaveChanges();

        // Assign permissions to Customer role
        var customerPermissions = permissions.Where(p => new[] {
            "booking.create", "booking.view_own", "booking.cancel_own", "booking.upload_payment",
            "review.create", "review.edit_own", "review.delete_own",
            "complex.favorite",
            "user.view_own_profile", "user.update_own_profile"
        }.Contains(p.PermissionKey)).ToList();

        foreach (var perm in customerPermissions)
        {
            context.RolePermissions.Add(new RolePermission
            {
                RoleId = customerRole.Id,
                PermissionId = perm.Id
            });
        }

        // Assign permissions to Owner role (includes Customer permissions + Owner-specific)
        var ownerPermissions = permissions.Where(p => new[] {
            "booking.create", "booking.view_own", "booking.cancel_own", "booking.upload_payment",
            "review.create", "review.edit_own", "review.delete_own",
            "complex.favorite",
            "user.view_own_profile", "user.update_own_profile",
            "complex.create", "complex.edit_own", "complex.delete_own", "complex.upload_images",
            "field.create", "field.edit_own", "field.delete_own",
            "timeslot.create", "timeslot.edit_own", "timeslot.delete_own",
            "booking.approve", "booking.reject", "booking.view_own_complex", "booking.mark_no_show",
            "review.reply",
            "owner_settings.manage"
        }.Contains(p.PermissionKey)).ToList();

        foreach (var perm in ownerPermissions)
        {
            context.RolePermissions.Add(new RolePermission
            {
                RoleId = ownerRole.Id,
                PermissionId = perm.Id
            });
        }

        // Assign all permissions to Admin role
        foreach (var perm in permissions)
        {
            context.RolePermissions.Add(new RolePermission
            {
                RoleId = adminRole.Id,
                PermissionId = perm.Id
            });
        }

        context.SaveChanges();
    }
}