using FootballField.API.Entities;

namespace FootballField.API.DbContexts;

public static class DatabaseSeeder
{
    public static void SeedData(this ApplicationDbContext context)
    {
        if (context.Users.Any()) return; // Không seed nếu đã có dữ liệu

        var now = DateTime.Now;

        // 1. SEED USERS (1 Admin + 3 Owners + 3 Customers)
        var admin = new User
        {
            LastName = "Admin",
            FirstName = "System",
            Email = "admin@footballfield.com",
            Phone = "0900000000",
            Password = BCrypt.Net.BCrypt.HashPassword("Admin@123"),
            Role = UserRole.Admin,
            Status = UserStatus.Active
        };

        var owner1 = new User
        {
            LastName = "Nguyễn",
            FirstName = "Văn A",
            Email = "owner1@footballfield.com",
            Phone = "0901111111",
            Password = BCrypt.Net.BCrypt.HashPassword("Owner@123"),
            Role = UserRole.Owner,
            Status = UserStatus.Active
        };

        var owner2 = new User
        {
            LastName = "Trần",
            FirstName = "Thị B",
            Email = "owner2@footballfield.com",
            Phone = "0902222222",
            Password = BCrypt.Net.BCrypt.HashPassword("Owner@123"),
            Role = UserRole.Owner,
            Status = UserStatus.Active
        };

        var owner3 = new User
        {
            LastName = "Lê",
            FirstName = "Văn C",
            Email = "owner3@footballfield.com",
            Phone = "0903333333",
            Password = BCrypt.Net.BCrypt.HashPassword("Owner@123"),
            Role = UserRole.Owner,
            Status = UserStatus.Active
        };

        var customer1 = new User
        {
            LastName = "Phạm",
            FirstName = "Văn D",
            Email = "customer1@footballfield.com",
            Phone = "0904444444",
            Password = BCrypt.Net.BCrypt.HashPassword("Customer@123"),
            Role = UserRole.Customer,
            Status = UserStatus.Active
        };

        var customer2 = new User
        {
            LastName = "Hoàng",
            FirstName = "Thị E",
            Email = "customer2@footballfield.com",
            Phone = "0905555555",
            Password = BCrypt.Net.BCrypt.HashPassword("Customer@123"),
            Role = UserRole.Customer,
            Status = UserStatus.Active
        };

        var customer3 = new User
        {
            LastName = "Vũ",
            FirstName = "Văn F",
            Email = "customer3@footballfield.com",
            Phone = "0906666666",
            Password = BCrypt.Net.BCrypt.HashPassword("Customer@123"),
            Role = UserRole.Customer,
            Status = UserStatus.Active
        };

        context.Users.AddRange(admin, owner1, owner2, owner3, customer1, customer2, customer3);
        context.SaveChanges();

        // 2. SEED COMPLEXES (3 cụm sân)
        var complex1 = new Complex
        {
            OwnerId = 2,
            Name = "Sân Bóng Thành Phố",
            Street = "123 Nguyễn Huệ",
            Ward = "Bến Nghé",
            Province = "Hồ Chí Minh",
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
            Name = "Sân Bóng Quận 7",
            Street = "456 Nguyễn Thị Thập",
            Ward = "Tân Phú",
            Province = "Hồ Chí Minh",
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
            Name = "Sân Bóng Bình Thạnh",
            Street = "789 Xô Viết Nghệ Tĩnh",
            Ward = "Phường 25",
            Province = "Hồ Chí Minh",
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

        // 4. SEED TIME SLOTS (5 slots mỗi sân)
        var timeSlots = new List<TimeSlot>();

        foreach (var field in fields)
        {
            var basePrice = field.FieldSize == "Sân 5" ? 300000 : (field.FieldSize == "Sân 7" ? 500000 : 800000);

            // Morning slots (6-12)
            timeSlots.Add(new TimeSlot
            {
                FieldId = field.Id,
                StartTime = new TimeSpan(6, 0, 0),
                EndTime = new TimeSpan(8, 0, 0),
                Price = basePrice * 0.8m,
                IsActive = true,
            });

            timeSlots.Add(new TimeSlot
            {
                FieldId = field.Id,
                StartTime = new TimeSpan(8, 0, 0),
                EndTime = new TimeSpan(10, 0, 0),
                Price = basePrice,
                IsActive = true,
            });

            // Afternoon slots
            timeSlots.Add(new TimeSlot
            {
                FieldId = field.Id,
                StartTime = new TimeSpan(14, 0, 0),
                EndTime = new TimeSpan(16, 0, 0),
                Price = basePrice,
                IsActive = true,
            });

            // Evening slots (prime time - expensive)
            timeSlots.Add(new TimeSlot
            {
                FieldId = field.Id,
                StartTime = new TimeSpan(18, 0, 0),
                EndTime = new TimeSpan(20, 0, 0),
                Price = basePrice * 1.3m,
                IsActive = true,
            });

            timeSlots.Add(new TimeSlot
            {
                FieldId = field.Id,
                StartTime = new TimeSpan(20, 0, 0),
                EndTime = new TimeSpan(22, 0, 0),
                Price = basePrice * 1.2m,
                IsActive = true,
            });
        }

        context.TimeSlots.AddRange(timeSlots);
        context.SaveChanges();

        // 5. SEED BOOKINGS
        var bookings = new[]
        {
            new Booking
            {
                FieldId = 1,
                CustomerId = 5,
                OwnerId = 2,
                BookingDate = now.AddDays(2),
                TimeSlotId = 4,
                DepositAmount = 78000,
                TotalAmount = 260000,
                BookingStatus = BookingStatus.Confirmed,
                PaymentStatus = PaymentStatus.DepositPaid,
                PaymentMethod = "MoMo",
                TransactionId = "MOMO123456",
            },
            new Booking
            {
                FieldId = 6,
                CustomerId = 6,
                OwnerId = 3,
                BookingDate = now.AddDays(3),
                TimeSlotId = 29,
                DepositAmount = 195000,
                TotalAmount = 390000,
                BookingStatus = BookingStatus.Confirmed,
                PaymentStatus = PaymentStatus.FullyPaid,
                PaymentMethod = "VNPay",
                TransactionId = "VNP789012",
            },
            new Booking
            {
                FieldId = 10,
                CustomerId = 7,
                OwnerId = 3,
                BookingDate = now.AddDays(1),
                TimeSlotId = 50,
                DepositAmount = 260000,
                TotalAmount = 650000,
                BookingStatus = BookingStatus.Pending,
                PaymentStatus = PaymentStatus.Unpaid,
            },
            new Booking
            {
                FieldId = 11,
                CustomerId = 5,
                OwnerId = 4,
                BookingDate = now.AddDays(5),
                TimeSlotId = 56,
                DepositAmount = 120000,
                TotalAmount = 400000,
                BookingStatus = BookingStatus.Completed,
                PaymentStatus = PaymentStatus.FullyPaid,
                PaymentMethod = "Cash",
            }
        };
        context.Bookings.AddRange(bookings);
        context.SaveChanges();
    }
}