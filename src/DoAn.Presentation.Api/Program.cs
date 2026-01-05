using System.Globalization;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Serilog;
using Serilog.Events;
using Serilog.Sinks.MSSqlServer;
using Serilog.Context;
using DoAn.Core.Application;
using DoAn.Infrastructure;
using DoAn.Infrastructure.Data;
using DoAn.Infrastructure.Data.Seeding;
using DoAn.Infrastructure.Authentication;

using DoAn.Presentation.Api.Middlewares;
using DoAn.Core.Application.Services.Location;
using Microsoft.OpenApi.Any;

var builder = WebApplication.CreateBuilder(args);

// ============================================================================
// 1. CẤU HÌNH LOGGING (SERILOG)
// ============================================================================
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
    .WriteTo.Console(outputTemplate: "[{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz}] [{Level:u3}] [{SourceContext}] {Message:lj}{NewLine}{Exception}")
    .WriteTo.File(
        path: "logs/system-.log",
        rollingInterval: RollingInterval.Day,
        outputTemplate: "[{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz}] [{Level:u3}] [{SourceContext}] {Message:lj}{NewLine}{Exception}",
        retainedFileCountLimit: 30)
    .WriteTo.MSSqlServer(
        connectionString: connectionString,
        restrictedToMinimumLevel: LogEventLevel.Warning,
        sinkOptions: new MSSqlServerSinkOptions
        {
            TableName = "SYSTEM_LOG",
            AutoCreateSqlTable = false,
            SchemaName = "dbo"
        },
        columnOptions: GetSqlColumnOptions())
    .CreateLogger();

builder.Host.UseSerilog();
builder.Logging.ClearProviders();

// ============================================================================
// 2. CẤU HÌNH HỆ THỐNG (Timezone, Culture)
// ============================================================================
string timeZoneId = OperatingSystem.IsWindows() ? "SE Asia Standard Time" : "Asia/Ho_Chi_Minh";
var vietnamTimeZone = TimeZoneInfo.FindSystemTimeZoneById(timeZoneId);
Environment.SetEnvironmentVariable("TZ", timeZoneId);

CultureInfo.DefaultThreadCurrentCulture = new CultureInfo("vi-VN");
CultureInfo.DefaultThreadCurrentUICulture = new CultureInfo("vi-VN");

// ============================================================================
// 3. ĐĂNG KÝ SERVICES (DEPENDENCY INJECTION)
// ============================================================================

// A. Tầng Application (Services, AutoMapper, Helpers)
builder.Services.AddApplicationServices();

// B. Tầng Infrastructure (DbContext, Repositories, Storage, BackgroundJobs)
builder.Services.AddInfrastructureServices(builder.Configuration);

// C. Các dịch vụ Web API & Utilities khác
builder.Services.AddSingleton(vietnamTimeZone);
builder.Services.AddHttpContextAccessor();
builder.Services.AddHttpClient(); // Cần cho LocationSeeder (nếu có dùng gọi API ngoài)

builder.Services.AddScoped<LocationSeeder>();

builder.Services.Configure<FormOptions>(o =>
{
    o.MultipartBodyLengthLimit = 20_000_000; // 20MB
});

// ============================================================================
// 4. CẤU HÌNH AUTHENTICATION & SWAGGER
// ============================================================================
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

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
    });

builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Football Field Booking API",
        Version = "v1",
        Description = "API for managing football field bookings"
    });

    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header. Enter your token below.",
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

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

var app = builder.Build();

// ============================================================================
// 5. CẤU HÌNH PIPELINE (MIDDLEWARE)
// ============================================================================

// A. Serilog Request Logging (Đặt đầu tiên để log toàn bộ request)
app.UseSerilogRequestLogging();

// B. Log UserId context
app.Use(async (context, next) =>
{
    var userId = context.User?.FindFirst("id")?.Value ?? "anonymous";
    LogContext.PushProperty("UserId", userId);
    await next();
});

// C. Seeding Data (Chạy khi khởi động)
await SeedDatabaseAsync(app.Services);

// D. Exception Middleware (Bắt lỗi toàn cục)
app.UseMiddleware<ExceptionMiddleware>();

// E. Swagger (Chỉ Dev)
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

// ============================================================================
// LOCAL FUNCTIONS (Helpers)
// ============================================================================

async Task SeedDatabaseAsync(IServiceProvider services)
{
    using var scope = services.CreateScope();
    // Lấy DbContext từ DI (đã đăng ký trong Infrastructure)
    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

    try
    {
        if (!db.Database.CanConnect())
        {
            await db.Database.MigrateAsync();
        }
    }
    catch (Exception ex)
    {
        Log.Error(ex, "Database migration failed");
    }

    // Seed Data Mẫu
    try
    {
        db.SeedFullData(); 
        Log.Information("Database seeded successfully");
    }
    catch (Exception ex)
    {
        Log.Error(ex, "Database seeding failed");
    }

    // Seed Location (Province/Ward)
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

static ColumnOptions GetSqlColumnOptions()
{
    var columnOptions = new ColumnOptions();
    columnOptions.Store.Clear();
    columnOptions.Store.Add(StandardColumn.Level);
    columnOptions.Store.Add(StandardColumn.Message);
    columnOptions.Store.Add(StandardColumn.TimeStamp);

    columnOptions.Level.ColumnName = "log_level";
    columnOptions.Level.StoreAsEnum = false;
    columnOptions.Message.ColumnName = "message";
    columnOptions.TimeStamp.ColumnName = "created_at";
    columnOptions.TimeStamp.ConvertToUtc = false;

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