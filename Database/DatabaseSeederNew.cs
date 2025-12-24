using FootballField.API.Modules.ComplexManagement.Entities;
using FootballField.API.Modules.FieldManagement.Entities;
using FootballField.API.Modules.UserManagement.Entities;
using FootballField.API.Modules.OwnerSettingsManagement.Entities;
using FootballField.API.Modules.SystemConfigManagement.Entities;
using FootballField.API.Shared.Utils;

namespace FootballField.API.Database;

/// <summary>
/// Database Seeder với dữ liệu đầy đủ từ SeedData_ALL_IN_ONE.sql
/// Seed data: 3 Roles, 51 Permissions, 20 Users, 15 Complexes, 60 Fields, 660 TimeSlots, 8 OwnerSettings
/// </summary>
public static class DatabaseSeederNew
{
    public static void SeedFullData(this ApplicationDbContext context)
    {
        Console.WriteLine("============================================");
        Console.WriteLine("Starting Full Database Seed Process");
        Console.WriteLine($"Date: {DateTime.Now}");
        Console.WriteLine("============================================");

        // Seed RBAC data first
        SeedRBACData(context);

        // Seed SystemConfig
        SeedSystemConfig(context);

        if (context.Users.Any())
        {
            Console.WriteLine("Users already exist. Skipping user seed.");
            return;
        }

        // Seed Users and UserRoles
        SeedUsers(context);

        // Seed Complexes, Fields, TimeSlots, OwnerSettings
        SeedComplexesAndFields(context);

        Console.WriteLine("");
        Console.WriteLine("============================================");
        Console.WriteLine("DATABASE SEED COMPLETED SUCCESSFULLY!");
        Console.WriteLine("============================================");
        Console.WriteLine($"  - Roles: {context.Roles.Count()}");
        Console.WriteLine($"  - Permissions: {context.Permissions.Count()}");
        Console.WriteLine($"  - Users: {context.Users.Count()}");
        Console.WriteLine($"  - Complexes: {context.Complexes.Count()}");
        Console.WriteLine($"  - Fields: {context.Fields.Count()}");
        Console.WriteLine($"  - Time Slots: {context.TimeSlots.Count()}");
        Console.WriteLine($"  - Owner Settings: {context.OwnerSettings.Count()}");
        Console.WriteLine($"  - System Configs: {context.SystemConfigs.Count()}");
        Console.WriteLine("");
        Console.WriteLine("Default Login Credentials:");
        Console.WriteLine("  Admin: admin / 123123");
        Console.WriteLine("  Owner: owner / 123123");
        Console.WriteLine("  Customer: customer1@gmail.com / Customer@123");
        Console.WriteLine("============================================");
    }

    private static void SeedRBACData(ApplicationDbContext context)
    {
        if (context.Roles.Any()) return;

        Console.WriteLine("Seeding RBAC Data...");

        // Create roles
        var customerRole = new Role { Name = "Customer", Description = "Khách hàng đặt sân", IsActive = true };
        var ownerRole = new Role { Name = "Owner", Description = "Chủ sân quản lý cụm sân", IsActive = true };
        var adminRole = new Role { Name = "Admin", Description = "Quản trị viên hệ thống", IsActive = true };

        context.Roles.AddRange(customerRole, ownerRole, adminRole);
        context.SaveChanges();

        // Create 51 permissions (matching SQL)
        var permissions = new List<Permission>
        {
            // Customer permissions (1-10)
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

            // Owner permissions (11-26)
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
            new Permission { PermissionKey = "booking.mark_complete", Description = "Đánh dấu hoàn thành đơn đặt sân", Module = "BookingManagement" },
            new Permission { PermissionKey = "booking.mark_no_show", Description = "Đánh dấu khách không đến", Module = "BookingManagement" },
            new Permission { PermissionKey = "review.reply", Description = "Trả lời đánh giá", Module = "ReviewManagement" },
            new Permission { PermissionKey = "review.view_own", Description = "Xem đánh giá của sân mình", Module = "ReviewManagement" },
            new Permission { PermissionKey = "owner_settings.manage", Description = "Quản lý cấu hình chủ sân", Module = "OwnerSettingsManagement" },

            // Admin permissions (27-51)
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

        // Assign permissions to Customer role (permissions 1-10)
        var customerPermissions = permissions.Take(10).ToList();
        foreach (var perm in customerPermissions)
        {
            context.RolePermissions.Add(new RolePermission
            {
                RoleId = customerRole.Id,
                PermissionId = perm.Id
            });
        }

        // Assign permissions to Owner role (permissions 1-26)
        var ownerPermissions = permissions.Take(26).ToList();
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
        Console.WriteLine("RBAC data seeded successfully!");
    }

    private static void SeedUsers(ApplicationDbContext context)
    {
        Console.WriteLine("Seeding Users...");

        var hashedPassword = BCrypt.Net.BCrypt.HashPassword("123123123");

        var users = new List<User>
        {
            // Admin users (2)
            new User { LastName = "Admin", FirstName = "System", Email = "admin@gmail.com", Phone = "0900000000", Password = hashedPassword, Status = UserStatus.Active },
            new User { LastName = "Admin", FirstName = "System 2", Email = "admin", Phone = "0900000001", Password = hashedPassword, Status = UserStatus.Active },

            // Owner users (8)
            new User { LastName = "Nguyễn", FirstName = "I Vân", Email = "owner", Phone = "0901111112", Password = hashedPassword, Status = UserStatus.Active },
            new User { LastName = "Nguyễn", FirstName = "Văn A", Email = "owner1@gmail.com", Phone = "0901111111", Password = hashedPassword, Status = UserStatus.Active },
            new User { LastName = "Trần", FirstName = "Thị B", Email = "owner2@gmail.com", Phone = "0902222222", Password = hashedPassword, Status = UserStatus.Active },
            new User { LastName = "Lê", FirstName = "Văn C", Email = "owner3@gmail.com", Phone = "0903333333", Password = hashedPassword, Status = UserStatus.Active },
            new User { LastName = "Phạm", FirstName = "Minh D", Email = "owner4@gmail.com", Phone = "0904444441", Password = hashedPassword, Status = UserStatus.Active },
            new User { LastName = "Hoàng", FirstName = "Văn E", Email = "owner5@gmail.com", Phone = "0905555551", Password = hashedPassword, Status = UserStatus.Active },
            new User { LastName = "Vũ", FirstName = "Thị F", Email = "owner6@gmail.com", Phone = "0906666661", Password = hashedPassword, Status = UserStatus.Active },
            new User { LastName = "Đặng", FirstName = "Văn G", Email = "owner7@gmail.com", Phone = "0907777771", Password = hashedPassword, Status = UserStatus.Active },

            // Customer users (10)
            new User { LastName = "Phạm", FirstName = "Văn D", Email = "customer1@gmail.com", Phone = "0904444444", Password = hashedPassword, Status = UserStatus.Active },
            new User { LastName = "Hoàng", FirstName = "Thị E", Email = "customer2@gmail.com", Phone = "0905555555", Password = hashedPassword, Status = UserStatus.Active },
            new User { LastName = "Vũ", FirstName = "Văn F", Email = "customer3@gmail.com", Phone = "0906666666", Password = hashedPassword, Status = UserStatus.Active },
            new User { LastName = "Đỗ", FirstName = "Thị H", Email = "customer4@gmail.com", Phone = "0908888888", Password = hashedPassword, Status = UserStatus.Active },
            new User { LastName = "Bùi", FirstName = "Văn I", Email = "customer5@gmail.com", Phone = "0909999999", Password = hashedPassword, Status = UserStatus.Active },
            new User { LastName = "Ngô", FirstName = "Thị K", Email = "customer6@gmail.com", Phone = "0910000000", Password = hashedPassword, Status = UserStatus.Active },
            new User { LastName = "Dương", FirstName = "Văn L", Email = "customer7@gmail.com", Phone = "0911111110", Password = hashedPassword, Status = UserStatus.Active },
            new User { LastName = "Lý", FirstName = "Thị M", Email = "customer8@gmail.com", Phone = "0912222220", Password = hashedPassword, Status = UserStatus.Active },
            new User { LastName = "Mai", FirstName = "Văn N", Email = "customer9@gmail.com", Phone = "0913333330", Password = hashedPassword, Status = UserStatus.Active },
            new User { LastName = "Tô", FirstName = "Thị O", Email = "customer10@gmail.com", Phone = "0914444440", Password = hashedPassword, Status = UserStatus.Active }
        };

        context.Users.AddRange(users);
        context.SaveChanges();

        // Assign roles to users
        var roles = context.Roles.ToList();
        var adminRoleId = roles.First(r => r.Name == "Admin").Id;
        var ownerRoleId = roles.First(r => r.Name == "Owner").Id;
        var customerRoleId = roles.First(r => r.Name == "Customer").Id;

        // Admin roles (users 1-2)
        for (int i = 0; i < 2; i++)
        {
            context.UserRoles.Add(new UserRole { UserId = users[i].Id, RoleId = adminRoleId });
        }

        // Owner roles (users 3-10)
        for (int i = 2; i < 10; i++)
        {
            context.UserRoles.Add(new UserRole { UserId = users[i].Id, RoleId = ownerRoleId });
        }

        // Customer roles (users 11-20)
        for (int i = 10; i < 20; i++)
        {
            context.UserRoles.Add(new UserRole { UserId = users[i].Id, RoleId = customerRoleId });
        }

        context.SaveChanges();
        Console.WriteLine($"Users seeded successfully! Total: {users.Count} (2 Admins, 8 Owners, 10 Customers)");
    }

    private static void SeedComplexesAndFields(ApplicationDbContext context)
    {
        Console.WriteLine("Seeding Complexes, Fields, TimeSlots, OwnerSettings...");

        // Get owner users (IDs 3-10 = 8 owners)
        var ownerIds = context.Users
            .Where(u => u.UserRoles.Any(ur => ur.Role.Name == "Owner"))
            .OrderBy(u => u.Id)
            .Select(u => u.Id)
            .ToList();

        // Seed 15 complexes (matching SQL)
        var complexes = new List<Complex>
        {
            // Owner 3's complexes (3)
            new Complex { OwnerId = ownerIds[0], Name = "Sân Bóng Thủ Đô", Street = "123 Kim Mã", Ward = "Phường Ba Đình", Province = "Thành phố Hà Nội", Phone = "0281234567", OpeningTime = new TimeSpan(6,0,0), ClosingTime = new TimeSpan(22,0,0), Description = "Sân bóng chất lượng cao tại trung tâm thành phố", Status = ComplexStatus.Approved, IsActive = true },
            new Complex { OwnerId = ownerIds[0], Name = "Sân Bóng Mỹ Đình", Street = "456 Lê Văn Lương", Ward = "Phường Cầu Giấy", Province = "Thành phố Hà Nội", Phone = "0281234568", OpeningTime = new TimeSpan(5,30,0), ClosingTime = new TimeSpan(23,0,0), Description = "Sân bóng hiện đại gần sân vận động Mỹ Đình", Status = ComplexStatus.Approved, IsActive = true },
            new Complex { OwnerId = ownerIds[0], Name = "Sân Bóng Cầu Giấy", Street = "789 Xuân Thủy", Ward = "Phường Cầu Giấy", Province = "Thành phố Hà Nội", Phone = "0281234569", OpeningTime = new TimeSpan(6,0,0), ClosingTime = new TimeSpan(22,30,0), Description = "Sân bóng tiện lợi cho sinh viên", Status = ComplexStatus.Approved, IsActive = true },
            
            // Owner 4's complexes (2)
            new Complex { OwnerId = ownerIds[1], Name = "Sân Bóng BKX", Street = "456 Tạ Quang Bửu", Ward = "Phường Bạch Mai", Province = "Thành phố Hà Nội", Phone = "0282345678", OpeningTime = new TimeSpan(5,30,0), ClosingTime = new TimeSpan(23,0,0), Description = "Sân bóng hiện đại, đầy đủ tiện nghi", Status = ComplexStatus.Approved, IsActive = true },
            new Complex { OwnerId = ownerIds[1], Name = "Sân Bóng Hai Bà Trưng", Street = "321 Trần Đại Nghĩa", Ward = "Phường Bạch Mai", Province = "Thành phố Hà Nội", Phone = "0282345679", OpeningTime = new TimeSpan(6,0,0), ClosingTime = new TimeSpan(22,0,0), Description = "Sân bóng gần trường đại học", Status = ComplexStatus.Approved, IsActive = true },
            
            // Owner 5's complexes (2)
            new Complex { OwnerId = ownerIds[2], Name = "Sân Bóng Đầm Hồng", Street = "789 Trường Chinh", Ward = "Phường Khương Đình", Province = "Thành phố Hà Nội", Phone = "0283456789", OpeningTime = new TimeSpan(6,0,0), ClosingTime = new TimeSpan(22,30,0), Description = "Sân bóng rộng rãi, thoáng mát", Status = ComplexStatus.Approved, IsActive = true },
            new Complex { OwnerId = ownerIds[2], Name = "Sân Bóng Thanh Xuân", Street = "234 Nguyễn Trãi", Ward = "Phường Thanh Xuân", Province = "Thành phố Hà Nội", Phone = "0283456790", OpeningTime = new TimeSpan(5,0,0), ClosingTime = new TimeSpan(23,30,0), Description = "Sân bóng hoạt động sớm nhất Hà Nội", Status = ComplexStatus.Approved, IsActive = true },
            
            // Owner 6's complexes (2)
            new Complex { OwnerId = ownerIds[3], Name = "Sân Bóng Long Biên", Street = "567 Nguyễn Văn Cừ", Ward = "Phường Long Biên", Province = "Thành phố Hà Nội", Phone = "0284567890", OpeningTime = new TimeSpan(6,0,0), ClosingTime = new TimeSpan(22,0,0), Description = "Sân bóng bên bờ sông Hồng", Status = ComplexStatus.Approved, IsActive = true },
            new Complex { OwnerId = ownerIds[3], Name = "Sân Bóng Gia Lâm", Street = "890 Ngô Gia Tự", Ward = "Xã Gia Lâm", Province = "Thành phố Hà Nội", Phone = "0284567891", OpeningTime = new TimeSpan(6,0,0), ClosingTime = new TimeSpan(22,0,0), Description = "Sân bóng rộng rãi, giá rẻ", Status = ComplexStatus.Approved, IsActive = true },
            
            // Owner 7's complexes (2)
            new Complex { OwnerId = ownerIds[4], Name = "Sân Bóng Hà Đông", Street = "123 Quang Trung", Ward = "Phường Hà Đông", Province = "Thành phố Hà Nội", Phone = "0285678901", OpeningTime = new TimeSpan(6,0,0), ClosingTime = new TimeSpan(22,0,0), Description = "Sân bóng khu vực Hà Đông", Status = ComplexStatus.Approved, IsActive = true },
            new Complex { OwnerId = ownerIds[4], Name = "Sân Bóng Hoàng Mai", Street = "456 Giải Phóng", Ward = "Phường Hoàng Mai", Province = "Thành phố Hà Nội", Phone = "0285678902", OpeningTime = new TimeSpan(5,30,0), ClosingTime = new TimeSpan(23,0,0), Description = "Sân bóng tiêu chuẩn FIFA", Status = ComplexStatus.Approved, IsActive = true },
            
            // Owner 8's complexes (2)
            new Complex { OwnerId = ownerIds[5], Name = "Sân Bóng Đống Đa", Street = "789 Láng Hạ", Ward = "Phường Đống Đa", Province = "Thành phố Hà Nội", Phone = "0286789012", OpeningTime = new TimeSpan(6,0,0), ClosingTime = new TimeSpan(22,0,0), Description = "Sân bóng trung tâm quận Đống Đa", Status = ComplexStatus.Approved, IsActive = true },
            new Complex { OwnerId = ownerIds[5], Name = "Sân Bóng Tây Hồ", Street = "321 Lạc Long Quân", Ward = "Phường Tây Hồ", Province = "Thành phố Hà Nội", Phone = "0286789013", OpeningTime = new TimeSpan(6,0,0), ClosingTime = new TimeSpan(22,30,0), Description = "Sân bóng view Hồ Tây đẹp", Status = ComplexStatus.Approved, IsActive = true },
            
            // Owner 9's complex (1)
            new Complex { OwnerId = ownerIds[6], Name = "Sân Bóng Nam Từ Liêm", Street = "654 Phạm Văn Đồng", Ward = "Phường Từ Liêm", Province = "Thành phố Hà Nội", Phone = "0287890123", OpeningTime = new TimeSpan(6,0,0), ClosingTime = new TimeSpan(22,0,0), Description = "Sân bóng hiện đại, đầy đủ dịch vụ", Status = ComplexStatus.Approved, IsActive = true },
            
            // Owner 10's complex (1)
            new Complex { OwnerId = ownerIds[7], Name = "Sân Bóng Bắc Từ Liêm", Street = "987 Đường Lâm", Ward = "Phường Từ Liêm", Province = "Thành phố Hà Nội", Phone = "0288901234", OpeningTime = new TimeSpan(5,30,0), ClosingTime = new TimeSpan(23,0,0), Description = "Sân bóng rộng, phục vụ 24/7", Status = ComplexStatus.Approved, IsActive = true }
        };

        context.Complexes.AddRange(complexes);
        context.SaveChanges();

        // Seed OwnerSettings (8 settings for 8 owners)
        var ownerSettings = new List<OwnerSetting>
        {
            new OwnerSetting { OwnerId = ownerIds[0], DepositRate = 0.30m, MinBookingNotice = 2, AllowReview = true },
            new OwnerSetting { OwnerId = ownerIds[1], DepositRate = 0.50m, MinBookingNotice = 3, AllowReview = true },
            new OwnerSetting { OwnerId = ownerIds[2], DepositRate = 0.30m, MinBookingNotice = 2, AllowReview = true },
            new OwnerSetting { OwnerId = ownerIds[3], DepositRate = 0.40m, MinBookingNotice = 4, AllowReview = true },
            new OwnerSetting { OwnerId = ownerIds[4], DepositRate = 0.30m, MinBookingNotice = 2, AllowReview = true },
            new OwnerSetting { OwnerId = ownerIds[5], DepositRate = 0.50m, MinBookingNotice = 3, AllowReview = true },
            new OwnerSetting { OwnerId = ownerIds[6], DepositRate = 0.30m, MinBookingNotice = 1, AllowReview = true },
            new OwnerSetting { OwnerId = ownerIds[7], DepositRate = 0.40m, MinBookingNotice = 2, AllowReview = true }
        };

        context.OwnerSettings.AddRange(ownerSettings);
        context.SaveChanges();

        // Seed 60 fields (4 per complex)
        var fields = new List<Field>();
        foreach (var complex in complexes)
        {
            fields.AddRange(new[]
            {
                new Field { ComplexId = complex.Id, Name = "Sân 1", SurfaceType = "Cỏ nhân tạo", FieldSize = "Sân 5 người", IsActive = true },
                new Field { ComplexId = complex.Id, Name = "Sân 2", SurfaceType = "Cỏ nhân tạo", FieldSize = "Sân 5 người", IsActive = true },
                new Field { ComplexId = complex.Id, Name = "Sân 3", SurfaceType = "Cỏ tự nhiên", FieldSize = "Sân 7 người", IsActive = true },
                new Field { ComplexId = complex.Id, Name = "Sân 4", SurfaceType = "Cỏ nhân tạo", FieldSize = "Sân 7 người", IsActive = true }
            });
        }

        context.Fields.AddRange(fields);
        context.SaveChanges();

        // Seed 660 time slots (11 slots per field)
        var timeSlots = new List<TimeSlot>();
        foreach (var field in fields)
        {
            var basePrice = field.FieldSize == "Sân 5 người" ? 300000m : 500000m;

            // 11 time slots from 06:00 to 22:30 (1h30 each)
            var slotTimes = new[]
            {
                (6, 0, 7, 30, 0.8m),    // 06:00-07:30
                (7, 30, 9, 0, 0.8m),    // 07:30-09:00
                (9, 0, 10, 30, 1.0m),   // 09:00-10:30
                (10, 30, 12, 0, 1.0m),  // 10:30-12:00
                (12, 0, 13, 30, 1.0m),  // 12:00-13:30
                (13, 30, 15, 0, 1.0m),  // 13:30-15:00
                (15, 0, 16, 30, 1.0m),  // 15:00-16:30
                (16, 30, 18, 0, 1.3m),  // 16:30-18:00 (prime)
                (18, 0, 19, 30, 1.3m),  // 18:00-19:30 (prime)
                (19, 30, 21, 0, 1.2m),  // 19:30-21:00
                (21, 0, 22, 30, 1.2m)   // 21:00-22:30
            };

            foreach (var (startHour, startMin, endHour, endMin, multiplier) in slotTimes)
            {
                timeSlots.Add(new TimeSlot
                {
                    FieldId = field.Id,
                    StartTime = new TimeSpan(startHour, startMin, 0),
                    EndTime = new TimeSpan(endHour, endMin, 0),
                    Price = basePrice * multiplier,
                    IsActive = true
                });
            }
        }

        context.TimeSlots.AddRange(timeSlots);
        context.SaveChanges();

        Console.WriteLine($"Complexes: {complexes.Count}, Fields: {fields.Count}, TimeSlots: {timeSlots.Count}, OwnerSettings: {ownerSettings.Count} seeded successfully!");
    }

    private static void SeedSystemConfig(ApplicationDbContext context)
    {
        if (context.SystemConfigs.Any())
        {
            Console.WriteLine("SystemConfigs already exist. Skipping SystemConfig seed.");
            return;
        }

        var now = DateTime.UtcNow;
        var configs = new List<SystemConfig>
        {
            new SystemConfig
            {
                ConfigKey = "DEFAULT_DEPOSIT_RATE",
                ConfigValue = "0.50",
                DataType = "decimal",
                Description = "Tỷ lệ đặt cọc mặc định (50%)",
                UpdatedAt = now
            },
            new SystemConfig
            {
                ConfigKey = "MIN_BOOKING_NOTICE_MINUTES",
                ConfigValue = "120",
                DataType = "int",
                Description = "Số phút tối thiểu trước khi đặt sân",
                UpdatedAt = now
            },
            new SystemConfig
            {
                ConfigKey = "BOOKING_HOLD_TIME_MINUTES",
                ConfigValue = "5",
                DataType = "int",
                Description = "Thời gian giữ đơn đặt sân (phút)",
                UpdatedAt = now
            },
            new SystemConfig
            {
                ConfigKey = "ALLOW_CANCEL_BEFORE_HOURS",
                ConfigValue = "24",
                DataType = "int",
                Description = "Số giờ trước khi được phép hủy đặt sân",
                UpdatedAt = now
            },
            new SystemConfig
            {
                ConfigKey = "ENABLE_REVIEW_SYSTEM",
                ConfigValue = "true",
                DataType = "boolean",
                Description = "Bật/tắt hệ thống đánh giá",
                UpdatedAt = now
            },
            new SystemConfig
            {
                ConfigKey = "MAINTENANCE_MODE",
                ConfigValue = "false",
                DataType = "boolean",
                Description = "Chế độ bảo trì hệ thống",
                UpdatedAt = now
            }
        };

        context.SystemConfigs.AddRange(configs);
        context.SaveChanges();

        Console.WriteLine($"SystemConfigs: {configs.Count} seeded successfully!");
    }
}
