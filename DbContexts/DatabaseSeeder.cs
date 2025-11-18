using FootballField.API.Entities;

namespace FootballField.API.DbContexts;

public static class DatabaseSeeder
{
    public static void SeedData(this ApplicationDbContext context)
    {
        if (context.Users.Any()) return; // Không seed nếu đã có dữ liệu

        // 1. SEED USERS (1 Admin + 3 Owners + 3 Customers)
        var admin = new User
        {
            LastName = "Admin",
            FirstName = "System",
            Email = "admin@gmail.com",
            Phone = "0900000000",
            Password = BCrypt.Net.BCrypt.HashPassword("Admin@123"),
            Role = UserRole.Admin,
            Status = UserStatus.Active
        };

        var admin2 = new User
        {
            LastName = "Admin",
            FirstName = "System 2",
            Email = "admin",
            Phone = "0900000001",
            Password = BCrypt.Net.BCrypt.HashPassword("123123"),
            Role = UserRole.Admin,
            Status = UserStatus.Active
        };

        var owner = new User
        {
            LastName = "Nguyễn",
            FirstName = "I Vân",
            Email = "owner",
            Phone = "0901111112",
            Password = BCrypt.Net.BCrypt.HashPassword("123123"),
            Role = UserRole.Owner,
            Status = UserStatus.Active
        };
        
        var owner1 = new User
        {
            LastName = "Nguyễn",
            FirstName = "Văn A",
            Email = "owner1@gmail.com",
            Phone = "0901111111",
            Password = BCrypt.Net.BCrypt.HashPassword("Owner@123"),
            Role = UserRole.Owner,
            Status = UserStatus.Active
        };

        var owner2 = new User
        {
            LastName = "Trần",
            FirstName = "Thị B",
            Email = "owner2@gmail.com",
            Phone = "0902222222",
            Password = BCrypt.Net.BCrypt.HashPassword("Owner@123"),
            Role = UserRole.Owner,
            Status = UserStatus.Active
        };

        var owner3 = new User
        {
            LastName = "Lê",
            FirstName = "Văn C",
            Email = "owner3@gmail.com",
            Phone = "0903333333",
            Password = BCrypt.Net.BCrypt.HashPassword("Owner@123"),
            Role = UserRole.Owner,
            Status = UserStatus.Active
        };

        var customer1 = new User
        {
            LastName = "Phạm",
            FirstName = "Văn D",
            Email = "customer1@gmail.com",
            Phone = "0904444444",
            Password = BCrypt.Net.BCrypt.HashPassword("Customer@123"),
            Role = UserRole.Customer,
            Status = UserStatus.Active
        };

        var customer2 = new User
        {
            LastName = "Hoàng",
            FirstName = "Thị E",
            Email = "customer2@gmail.com",
            Phone = "0905555555",
            Password = BCrypt.Net.BCrypt.HashPassword("Customer@123"),
            Role = UserRole.Customer,
            Status = UserStatus.Active
        };

        var customer3 = new User
        {
            LastName = "Vũ",
            FirstName = "Văn F",
            Email = "customer3@gmail.com",
            Phone = "0906666666",
            Password = BCrypt.Net.BCrypt.HashPassword("Customer@123"),
            Role = UserRole.Customer,
            Status = UserStatus.Active
        };

        context.Users.AddRange(admin, admin2, owner, owner1, owner2, owner3, customer1, customer2, customer3);
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
            var endOfDay = new TimeSpan(22, 0, 0);
            var slotDuration = new TimeSpan(1, 30, 0); // 1h30

            for (var start = startOfDay; start < endOfDay; start += slotDuration)
            {
                var end = start + slotDuration;

                // Xác định buổi để tính giá
                decimal priceMultiplier;

                if (start.Hours < 8)
                    priceMultiplier = 0.8m; // Sáng sớm
                else if (start.Hours < 17)
                    priceMultiplier = 1.0m; // Ban ngày
                else if (start.Hours < 20)
                    priceMultiplier = 1.3m; // Tối cao điểm
                else
                    priceMultiplier = 1.2m; // Tối muộn

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
}