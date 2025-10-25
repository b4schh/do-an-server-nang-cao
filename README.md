# Football Field Booking Backend API

## Mô tả dự án

API Backend cho hệ thống đặt sân bóng đá, được xây dựng bằng ASP.NET Core 8.0. Hệ thống hỗ trợ quản lý sân bóng, đặt lịch, thanh toán và đánh giá.

## Công nghệ sử dụng

- **Framework**: ASP.NET Core 8.0
- **Database**: SQL Server
- **ORM**: Entity Framework Core 8.0
- **Authentication**: JWT Bearer Token
- **Mapping**: AutoMapper
- **Password Hashing**: BCrypt.Net
- **Storage**: MinIO (Object Storage)
- **API Documentation**: Swagger/OpenAPI

## Yêu cầu hệ thống

- .NET 8.0 SDK
- SQL Server 2019 trở lên (hoặc SQL Server Express)
- Visual Studio 2022 / Visual Studio Code / JetBrains Rider
- Git

## Cài đặt

### 1. Clone dự án

```bash
git clone <repository-url>
cd do-an-server-nang-cao
```

### 2. Cài đặt .NET 8.0 SDK

Tải và cài đặt từ: https://dotnet.microsoft.com/download/dotnet/8.0

Kiểm tra cài đặt:
```bash
dotnet --version
```

### 3. Cài đặt SQL Server

- Tải SQL Server Express: https://www.microsoft.com/en-us/sql-server/sql-server-downloads
- Hoặc sử dụng SQL Server đã có sẵn

### 4. Cấu hình Database

Mở file `appsettings.json` và cập nhật connection string phù hợp với môi trường của bạn:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost\\SQLEXPRESS;Database=FootballFieldManagement;Integrated Security=True;Encrypt=True;Trust Server Certificate=True"
  }
}
```

**Lưu ý**: 
- Nếu dùng SQL Server thông thường: `Server=localhost;...`
- Nếu dùng SQL Server Express: `Server=localhost\\SQLEXPRESS;...`
- Nếu dùng SQL Authentication: thêm `User Id=sa;Password=yourpassword;`

### 5. Restore packages

```bash
dotnet restore
```

### 6. Tạo và áp dụng Migration

```bash
# Tạo database và apply migrations
dotnet ef database update

# Nếu cần tạo migration mới
dotnet ef migrations add MigrationName
```

### 7. Chạy ứng dụng

```bash
dotnet run
```

Hoặc sử dụng watch mode để tự động reload khi có thay đổi:
```bash
dotnet watch run
```

### 8. Truy cập ứng dụng

- **API**: http://localhost:5000 hoặc https://localhost:5001
- **Swagger UI**: https://localhost:5001/swagger

## Dữ liệu mẫu (Seeding)

Dự án có sẵn dữ liệu mẫu được tự động seed khi chạy lần đầu, bao gồm:

### Tài khoản mẫu:

**Admin:**
- Email: `admin@footballfield.com`
- Password: `Admin@123`

**Chủ sân (Owner):**
- Email: `owner1@footballfield.com` | Password: `Owner@123`
- Email: `owner2@footballfield.com` | Password: `Owner@123`
- Email: `owner3@footballfield.com` | Password: `Owner@123`

**Khách hàng (Customer):**
- Email: `customer1@footballfield.com` | Password: `Customer@123`
- Email: `customer2@footballfield.com` | Password: `Customer@123`
- Email: `customer3@footballfield.com` | Password: `Customer@123`

### Dữ liệu khác:
- 3 cụm sân bóng
- 12 sân bóng (4-5 sân mỗi cụm)
- 60 time slots (5 slots/sân)
- 4 bookings mẫu
- 3 reviews