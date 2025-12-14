using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using System.Data;
using System.Security.Claims;
using Microsoft.OpenApi.Models;
using Microsoft.OpenApi.Any;
using Microsoft.AspNetCore.Http.Features;
using System.Globalization;
using Minio;
using Serilog;
using Serilog.Sinks.MSSqlServer;
using Serilog.Events;
using Serilog.Context;
using FootballField.API.Modules.AIManagement;

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
using FootballField.API.Modules.LocationManagement;
using FootballField.API.Modules.LocationManagement.Services;

var builder = WebApplication.CreateBuilder(args);

// ========== CẤU HÌNH SERILOG ==========
// Configure Serilog with connection string from configuration
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Override("Microsoft.EntityFrameworkCore", LogEventLevel.Warning)
    .MinimumLevel.Override("Microsoft", LogEventLevel.Information)
    .MinimumLevel.Information()
    .Enrich.FromLogContext()
    .Enrich.WithMachineName()
    .Enrich.WithProcessId()
    .Enrich.WithThreadId()
    .WriteTo.Console(
        outputTemplate: "[{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz}] [{Level:u3}] [{SourceContext}] {Message:lj}{NewLine}{Exception}")
    .WriteTo.File(
        path: "logs/system-.log",
        rollingInterval: RollingInterval.Day,
        outputTemplate: "[{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz}] [{Level:u3}] [{SourceContext}] {Message:lj}{NewLine}{Exception}",
        retainedFileCountLimit: 30)
    .WriteTo.MSSqlServer(
        connectionString: connectionString,
        restrictedToMinimumLevel: LogEventLevel.Warning, // Chỉ lưu Warning, Error, Fatal vào DB
        sinkOptions: new MSSqlServerSinkOptions
        {
            TableName = "SYSTEM_LOG",
            AutoCreateSqlTable = false,
            SchemaName = "dbo"
        },
        columnOptions: GetSqlColumnOptions())
    .CreateLogger();

static ColumnOptions GetSqlColumnOptions()
{
    var columnOptions = new ColumnOptions();

    // Clear default columns
    columnOptions.Store.Clear();

    // Only add the columns that exist in SYSTEM_LOG table
    columnOptions.Store.Add(StandardColumn.Level);
    columnOptions.Store.Add(StandardColumn.Message);
    columnOptions.Store.Add(StandardColumn.TimeStamp);

    // Map to your table columns (lowercase with underscore)
    columnOptions.Level.ColumnName = "log_level";
    columnOptions.Level.StoreAsEnum = false;

    columnOptions.Message.ColumnName = "message";

    columnOptions.TimeStamp.ColumnName = "created_at";
    columnOptions.TimeStamp.ConvertToUtc = false;

    // Add Source as additional column with custom value from property
    columnOptions.AdditionalColumns = new System.Collections.ObjectModel.Collection<SqlColumn>
    {
        new SqlColumn
        {
            ColumnName = "source",
            PropertyName = "SourceContext",
            DataType = System.Data.SqlDbType.NVarChar,
            DataLength = 100,
            AllowNull = true
        }
    };

    columnOptions.DisableTriggers = true;

    return columnOptions;
}

// Use Serilog for all logging
builder.Host.UseSerilog();
builder.Logging.ClearProviders();

// ========== CẤU HÌNH TIMEZONE ==========
// Set timezone cho toàn bộ ứng dụng, safe fallback
TimeZoneInfo vietnamTimeZone;
try
{
    string timeZoneId = OperatingSystem.IsWindows() ? "SE Asia Standard Time" : "Asia/Ho_Chi_Minh";
    vietnamTimeZone = TimeZoneInfo.FindSystemTimeZoneById(timeZoneId);
}
catch (Exception ex)
{
    // fallback to UTC if not found on platform
    Log.Warning(ex, "Timezone id not found, fallback to UTC");
    vietnamTimeZone = TimeZoneInfo.Utc;
}
Environment.SetEnvironmentVariable("TZ", vietnamTimeZone.Id);

// Đặt culture mặc định cho ứng dụng
CultureInfo.DefaultThreadCurrentCulture = new CultureInfo("vi-VN");
CultureInfo.DefaultThreadCurrentUICulture = new CultureInfo("vi-VN");

// Đọc Connection String từ appsettings.json (already read above for Serilog)
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
builder.Services.AddLocationManagementModule();

// NOTE: AddAIModule should NOT resolve scoped services from root. Register AI module AFTER other modules.
builder.Services.AddAIModule(builder.Configuration);

// ========== ĐĂNG KÝ UTILITIES ==========
builder.Services.AddScoped<JwtHelper>();
builder.Services.AddHttpContextAccessor();
builder.Services.AddHttpClient(); // For LocationSeeder

builder.Services.AddSingleton(vietnamTimeZone);
// Register AI plugin as scoped so it can use IHttpContextAccessor and request-scoped services




// ========== CẤU HÌNH JWT AUTHENTICATION ==========
var jwtSettings = builder.Configuration.GetSection("JwtSettings");
var secretKey = jwtSettings["SecretKey"] ?? throw new InvalidOperationException("JWT SecretKey not configured");
var issuer = jwtSettings["Issuer"];
var audience = jwtSettings["Audience"];

// (Optional) show PII for identity model in dev for deeper debugging - comment out in production
// Microsoft.IdentityModel.Logging.IdentityModelEventSource.ShowPII = true;

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.RequireHttpsMetadata = false; // dev ok; set true for prod
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
        Description = "JWT Authorization header. Enter 'Bearer {token}' (without quotes).",
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

// ========== Đăng ký MinioClient qua DI (validate config) ==========
builder.Services.AddSingleton<IMinioClient>(sp =>
{
    var cfg = sp.GetRequiredService<IConfiguration>().GetSection("Minio");
    var endpoint = cfg["Endpoint"];
    var accessKey = cfg["AccessKey"];
    var secretKey = cfg["SecretKey"];
    var withSslRaw = cfg["WithSSL"];

    if (string.IsNullOrWhiteSpace(endpoint) || string.IsNullOrWhiteSpace(accessKey) || string.IsNullOrWhiteSpace(secretKey))
    {
        // Fail fast during startup so we know config is missing
        throw new InvalidOperationException("Minio configuration is missing. Please set Minio:Endpoint, Minio:AccessKey, Minio:SecretKey in configuration.");
    }

    var clientBuilder = new MinioClient()
        .WithEndpoint(endpoint)
        .WithCredentials(accessKey, secretKey);

    if (bool.TryParse(withSslRaw, out var withSSL) && withSSL) clientBuilder = clientBuilder.WithSSL();

    return clientBuilder.Build();
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

// Use Serilog request logging
app.UseSerilogRequestLogging();

// Áp dụng Migration tự động và Seeding (safe, supports sync/async SeedData)
await RunMigrationsAndSeedAsync(app.Services);

static async Task RunMigrationsAndSeedAsync(IServiceProvider services)
{
    using var scope = services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

    try
    {
        // Always attempt migrate (wrapped in try/catch)
        await db.Database.MigrateAsync();
        Log.Information("Database migrated");
    }
    catch (Exception ex)
    {
        Log.Error(ex, "Database migration failed");
    }

    try
    {
        // Try to call SeedData; support async or sync method
        var seedMethod = db.GetType().GetMethod("SeedData");
        if (seedMethod != null)
        {
            var result = seedMethod.Invoke(db, null);
            if (result is System.Threading.Tasks.Task t)
            {
                await t;
            }
            Log.Information("Database seeded successfully (via SeedData)");
        }
        else
        {
            Log.Information("No SeedData method found on ApplicationDbContext");
        }
    }
    catch (Exception ex)
    {
        Log.Error(ex, "Database seeding failed");
    }

    // Seed location data (Province & Ward)
    try
    {
        var locationSeeder = scope.ServiceProvider.GetRequiredService<LocationSeeder>();
        await locationSeeder.SeedLocationsAsync();
        Log.Information("Location data seeded successfully");
    }
    catch (Exception ex)
    {
        Log.Error(ex, "Location seeding failed");
    }
}

// Use exception middleware early to catch downstream errors
app.UseMiddleware<ExceptionMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("AllowAll");

app.UseHttpsRedirection();

app.UseRouting();

// Authentication must run before we push UserId property for logging
app.UseAuthentication();

// Push UserId into Serilog context per-request (after authentication). Use using to pop property automatically.
app.Use(async (context, next) =>
{
    // Resolve user id from claims if possible (cover common claim names)
    string? userId = null;
    try
    {
        userId = context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value
                 ?? context.User?.FindFirst("id")?.Value
                 ?? context.User?.FindFirst("sub")?.Value
                 ?? "anonymous";
    }
    catch
    {
        userId = "anonymous";
    }

    using (LogContext.PushProperty("UserId", userId))
    {
        await next();
    }
});

app.UseAuthorization();

app.MapControllers();

try
{
    Log.Information("Starting Football Field Booking API");

    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Application terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}
