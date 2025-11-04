using FootballField.API.Utils;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Microsoft.OpenApi.Models;
using Microsoft.OpenApi.Any;
using FootballField.API.Middlewares;
using FootballField.API.DbContexts;
using FootballField.API.Mappings;
using FootballField.API.Repositories.Interfaces;
using FootballField.API.Repositories.Implements;
using FootballField.API.Services.Interfaces;
using FootballField.API.Services.Implements;
using Minio;
using FootballField.API.Storage;
using Microsoft.AspNetCore.Http.Features;
using System.Security.Claims;
using FootballField.API.Entities;

var builder = WebApplication.CreateBuilder(args);

// -------------------- Configuration & DbContext --------------------
var configuration = builder.Configuration;
var connectionString = configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));

// -------------------- AutoMapper --------------------
// Keep AddAutoMapper (ensure packages match versions in csproj)
builder.Services.AddAutoMapper(typeof(MappingProfile));

// -------------------- Repositories --------------------
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));
builder.Services.AddScoped<IComplexRepository, ComplexRepository>();

// -------------------- Services --------------------
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IComplexService, ComplexService>();
builder.Services.AddScoped<IComplexImageService, ComplexImageService>();

// Utilities
builder.Services.AddScoped<JwtHelper>();

// -------------------- JWT Authentication --------------------
// Read section "JwtSettings" from appsettings.json
var jwtSettings = configuration.GetSection("JwtSettings");
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
        ClockSkew = TimeSpan.Zero,
        RoleClaimType = ClaimTypes.Role
    };
});

// -------------------- Authorization Policies --------------------
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("RequireAdminRole", policy =>
        policy.RequireRole(UserRole.Admin.ToString()));

    options.AddPolicy("RequireOwnerRole", policy =>
        policy.RequireRole(UserRole.Owner.ToString(), UserRole.Admin.ToString()));

    options.AddPolicy("RequireCustomerRole", policy =>
        policy.RequireRole(UserRole.Customer.ToString(), UserRole.Owner.ToString(), UserRole.Admin.ToString()));
});

// -------------------- Controllers / JSON --------------------
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
    });

builder.Services.AddEndpointsApiExplorer();

// -------------------- Swagger (with JWT) --------------------
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
        Description = "JWT Authorization header — enter Bearer <token>",
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

// -------------------- Options / Minio / Storage --------------------
builder.Services.Configure<MinioSettings>(configuration.GetSection("Minio"));

// File upload limit (e.g. 20MB)
builder.Services.Configure<FormOptions>(o => {
    o.MultipartBodyLengthLimit = 20_000_000;
});

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

builder.Services.AddSingleton<IStorageService, MinioStorageService>();

// -------------------- CORS --------------------
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

// -------------------- Build app --------------------
var app = builder.Build();

// -------------------- Seed database on startup --------------------
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        var context = services.GetRequiredService<ApplicationDbContext>();

        context.Database.Migrate();

        logger.LogInformation("DB Connection: {Conn}", context.Database.GetDbConnection().ConnectionString);

        context.SeedData();

        logger.LogInformation("Database seeding completed.");
    }
    catch (Exception ex)
    {
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "Lỗi khi seed dữ liệu vào database");
        // Không throw để ứng dụng vẫn có thể boot nếu bạn muốn; tuy nhiên giữ throw giúp phát hiện lỗi sớm.
        throw;
    }
}

// -------------------- Middleware pipeline --------------------
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
