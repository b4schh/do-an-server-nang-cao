using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Microsoft.OpenApi.Models;
using Microsoft.OpenApi.Any;
using Microsoft.AspNetCore.Http.Features;
using System.Globalization;
using Minio;

// Shared Components
using FootballField.API.Shared.Utils;
using FootballField.API.Shared.Middlewares;
using FootballField.API.Shared.Storage;

// Database
using FootballField.API.Database;

// Module Registrations
using FootballField.API.Modules.AuthManagement;
using FootballField.API.Modules.UserManagement;
using FootballField.API.Modules.ComplexManagement;
using FootballField.API.Modules.FieldManagement;
using FootballField.API.Modules.BookingManagement;
using FootballField.API.Modules.ReviewManagement;
using FootballField.API.Modules.NotificationManagement;
using FootballField.API.Modules.OwnerSettingsManagement;

var builder = WebApplication.CreateBuilder(args);

// ========== CẤU HÌNH TIMEZONE ==========
// Set timezone cho toàn bộ ứng dụng
string timeZoneId =
    OperatingSystem.IsWindows()
        ? "SE Asia Standard Time"
        : "Asia/Ho_Chi_Minh";

var vietnamTimeZone = TimeZoneInfo.FindSystemTimeZoneById(timeZoneId);
Environment.SetEnvironmentVariable("TZ", timeZoneId);

// Đặt culture mặc định cho ứng dụng
CultureInfo.DefaultThreadCurrentCulture = new CultureInfo("vi-VN");
CultureInfo.DefaultThreadCurrentUICulture = new CultureInfo("vi-VN");

// Đọc Connection String từ appsettings.json
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));

// ========== ĐĂNG KÝ AUTOMAPPER ==========
builder.Services.AddAutoMapper(typeof(MappingProfile));

// ========== ĐĂNG KÝ MODULE DEPENDENCIES ==========
// Register all modules with their services and repositories
builder.Services.AddAuthModule();
builder.Services.AddUserModule();
builder.Services.AddComplexManagementModule();
builder.Services.AddFieldManagementModule();
builder.Services.AddBookingModule();
builder.Services.AddReviewModule();
builder.Services.AddNotificationModule();
builder.Services.AddOwnerSettingsModule();

// ========== ĐĂNG KÝ UTILITIES ==========
builder.Services.AddScoped<JwtHelper>();
builder.Services.AddHttpContextAccessor();

builder.Services.AddSingleton(vietnamTimeZone);

// ========== CẤU HÌNH JWT AUTHENTICATION ==========
var jwtSettings = builder.Configuration.GetSection("JwtSettings");
var secretKey = jwtSettings["SecretKey"] ?? throw new InvalidOperationException("JWT SecretKey not configured");
var issuer = jwtSettings["Issuer"];
var audience = jwtSettings["Audience"];

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = issuer,
        ValidAudience = audience,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey)),
        ClockSkew = TimeSpan.Zero
    };
    
});

builder.Services.AddAuthorization();

// Cấu hình dịch vụ (Swagger, Controller, CORS, Logging…)
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
    });

builder.Services.AddEndpointsApiExplorer();

// Cấu hình Swagger với JWT Authentication
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Football Field Booking API",
        Version = "v1",
        Description = "API for managing football field bookings"
    });

    // Thêm định nghĩa bảo mật JWT
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header. Just enter your token below - no need for 'Bearer' prefix",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });

    c.MapType<TimeSpan>(() => new OpenApiSchema
    {
        Type = "string",
        Format = "time",
        Example = new OpenApiString("HH:mm:ss")
    });
});

// Bind options
builder.Services.Configure<MinioSettings>(builder.Configuration.GetSection("Minio"));

// File upload limit
builder.Services.Configure<FormOptions>(o =>
{
    o.MultipartBodyLengthLimit = 20_000_000;
});

// Đăng ký MinioClient qua DI
builder.Services.AddSingleton<IMinioClient>(sp =>
{
    var cfg = sp.GetRequiredService<IConfiguration>().GetSection("Minio");
    var endpoint = cfg["Endpoint"]!;
    var accessKey = cfg["AccessKey"]!;
    var secretKey = cfg["SecretKey"]!;
    var withSSL = bool.TryParse(cfg["WithSSL"], out var ssl) && ssl;

    var client = new MinioClient()
        .WithEndpoint(endpoint)
        .WithCredentials(accessKey, secretKey);

    if (withSSL) client = client.WithSSL();

    return client.Build();
});

// Đăng ký storage service
builder.Services.AddSingleton<IStorageService, MinioStorageService>();

// Cho phép gọi API từ frontend khác domain
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

// Build app
var app = builder.Build();

// Áp dụng Migration tự động
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    
    try
    {
        if (!db.Database.CanConnect())
        {
            db.Database.Migrate();
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine($"[Warning] Database migration skipped: {ex.Message}");
    }

    // Seed dữ liệu mẫu
    db.SeedData();
}

app.UseMiddleware<ExceptionMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("AllowAll");

app.UseHttpsRedirection();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();


app.MapControllers();

app.Run();