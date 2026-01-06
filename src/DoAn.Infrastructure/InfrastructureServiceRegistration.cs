using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Minio;
using DoAn.Infrastructure.Data;
using DoAn.Infrastructure.Repositories.Base;
using DoAn.Infrastructure.Repositories.User;
using DoAn.Infrastructure.Repositories.Complex;
using DoAn.Infrastructure.Repositories.Field;
using DoAn.Infrastructure.Repositories.Booking;
using DoAn.Infrastructure.Repositories.Review;
using DoAn.Infrastructure.Repositories.Notification;
using DoAn.Infrastructure.Repositories.OwnerSetting;
using DoAn.Infrastructure.Repositories.SystemConfig;
using DoAn.Infrastructure.Repositories.Location;
using DoAn.Infrastructure.Authentication;
using DoAn.Core.Application.Interfaces.Base;
using DoAn.Core.Application.Interfaces.User;
using DoAn.Core.Application.Interfaces.Complex;
using DoAn.Core.Application.Interfaces.Field;
using DoAn.Core.Application.Interfaces.Booking;
using DoAn.Core.Application.Interfaces.Review;
using DoAn.Core.Application.Interfaces.Notification;
using DoAn.Core.Application.Interfaces.OwnerSetting;
using DoAn.Core.Application.Interfaces.SystemConfig;
using DoAn.Core.Application.Interfaces.Location;
using DoAn.Core.Application.Interfaces.Storage;
using DoAn.Core.Application.Interfaces.Auth;
using DoAn.Infrastructure.Storage;
using DoAn.Infrastructure.BackgroundJobs;

namespace DoAn.Infrastructure;

public static class InfrastructureServiceRegistration
{
    public static IServiceCollection AddInfrastructureServices(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // ========== DATABASE CONTEXT ==========
        var connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseSqlServer(connectionString));

        // ========== GENERIC REPOSITORY ==========
        services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));

        // ========== USER & AUTH REPOSITORIES ==========
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IRoleRepository, RoleRepository>();
        services.AddScoped<IPermissionRepository, PermissionRepository>();

        // ========== AUTH SERVICES ==========
        services.AddScoped<ITokenService, JwtTokenService>();

        // ========== COMPLEX REPOSITORIES ==========
        services.AddScoped<IComplexRepository, ComplexRepository>();
        services.AddScoped<IComplexImageRepository, ComplexImageRepository>();
        services.AddScoped<IFavoriteComplexRepository, FavoriteComplexRepository>();

        // ========== FIELD REPOSITORIES ==========
        services.AddScoped<IFieldRepository, FieldRepository>();
        services.AddScoped<ITimeSlotRepository, TimeSlotRepository>();

        // ========== BOOKING REPOSITORIES ==========
        services.AddScoped<IBookingRepository, BookingRepository>();

        // ========== REVIEW REPOSITORIES ==========
        services.AddScoped<IReviewRepository, ReviewRepository>();
        services.AddScoped<IReviewHelpfulVoteRepository, ReviewHelpfulVoteRepository>();

        // ========== NOTIFICATION REPOSITORIES ==========
        services.AddScoped<INotificationRepository, NotificationRepository>();
        services.AddScoped<ISseRepository, SseRepository>();

        // ========== OWNER SETTING REPOSITORIES ==========
        services.AddScoped<IOwnerSettingRepository, OwnerSettingRepository>();

        // ========== SYSTEM CONFIG REPOSITORIES ==========
        services.AddScoped<ISystemConfigRepository, SystemConfigRepository>();

        // ========== LOCATION REPOSITORIES ==========
        services.AddScoped<IProvinceRepository, ProvinceRepository>();
        services.AddScoped<IWardRepository, WardRepository>();

        // ========== MINIO STORAGE SERVICE ==========
        services.Configure<MinioSettings>(configuration.GetSection("Minio"));

        services.AddSingleton<IMinioClient>(sp =>
        {
            // Lấy setting đã được bind tự động thông qua DI
            var settings = sp.GetRequiredService<IOptions<MinioSettings>>().Value;

            var endpoint = settings.Endpoint;
            var accessKey = settings.AccessKey;
            var secretKey = settings.SecretKey;

            // Kiểm tra an toàn (Optional)
            if (string.IsNullOrWhiteSpace(endpoint))
                throw new ArgumentNullException(nameof(endpoint), "Minio Endpoint is not configured.");

            var client = new MinioClient()
                .WithEndpoint(endpoint)
                .WithCredentials(accessKey, secretKey);

            if (settings.WithSSL)
                client = client.WithSSL();

            return client.Build();
        });

        services.AddSingleton<IStorageService, MinioStorageService>();

         // Register Background Services
        services.AddHostedService<BookingExpirationBackgroundService>();

        services.AddScoped<ITokenService, JwtTokenService>();
        return services;
    }
}
