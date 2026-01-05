using Microsoft.Extensions.DependencyInjection;
using System.Reflection;
using DoAn.Core.Application.Interfaces.Auth;
using DoAn.Core.Application.Interfaces.User;
using DoAn.Core.Application.Interfaces.Complex;
using DoAn.Core.Application.Interfaces.Field;
using DoAn.Core.Application.Interfaces.Booking;
using DoAn.Core.Application.Interfaces.Review;
using DoAn.Core.Application.Interfaces.Notification;
using DoAn.Core.Application.Interfaces.OwnerSetting;
using DoAn.Core.Application.Interfaces.SystemConfig;
using DoAn.Core.Application.Interfaces.Location;
using DoAn.Core.Application.Interfaces.Statistics;
using DoAn.Core.Application.Services.Auth;
using DoAn.Core.Application.Services.User;
using DoAn.Core.Application.Services.Complex;
using DoAn.Core.Application.Services.Field;
using DoAn.Core.Application.Services.Booking;
using DoAn.Core.Application.Services.Review;
using DoAn.Core.Application.Services.Notification;
using DoAn.Core.Application.Services.Notification.Helpers;
using DoAn.Core.Application.Services.OwnerSetting;
using DoAn.Core.Application.Services.SystemConfig;
using DoAn.Core.Application.Services.Location;
using DoAn.Core.Application.Services.Statistics;

namespace DoAn.Core.Application;

public static class ApplicationServiceRegistration
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        // ========== AUTOMAPPER ==========
        services.AddAutoMapper(Assembly.GetExecutingAssembly());

        // ========== AUTH SERVICES ==========
        services.AddScoped<IAuthService, AuthService>();

        // ========== USER SERVICES ==========
        services.AddScoped<IUserService, UserService>();
        services.AddScoped<IRoleService, RoleService>();
        services.AddScoped<IPermissionService, PermissionService>();
        services.AddScoped<IPermissionManagementService, PermissionManagementService>();

        // ========== COMPLEX SERVICES ==========
        services.AddScoped<IComplexService, ComplexService>();
        services.AddScoped<IComplexImageService, ComplexImageService>();
        services.AddScoped<IFavoriteComplexService, FavoriteComplexService>();

        // ========== FIELD SERVICES ==========
        services.AddScoped<IFieldService, FieldService>();
        services.AddScoped<ITimeSlotService, TimeSlotService>();

        // ========== BOOKING SERVICES ==========
        services.AddScoped<IBookingService, BookingService>();

        // ========== REVIEW SERVICES ==========
        services.AddScoped<IReviewService, ReviewService>();

        // ========== NOTIFICATION SERVICES ==========
        services.AddScoped<INotificationService, NotificationService>();

        // ========== OWNER SETTING SERVICES ==========
        services.AddScoped<IOwnerSettingService, OwnerSettingService>();

        // ========== SYSTEM CONFIG SERVICES ==========
        services.AddScoped<ISystemConfigService, SystemConfigService>();

        // ========== LOCATION SERVICES ==========
        services.AddScoped<IProvinceService, ProvinceService>();
        services.AddScoped<IWardService, WardService>();

        // ========== STATISTICS SERVICES ==========
        services.AddScoped<IStatisticsService, OwnerStatisticsService>();

        // Register Helpers
        services.AddScoped<NotificationHelper>();

        return services;
    }
}
