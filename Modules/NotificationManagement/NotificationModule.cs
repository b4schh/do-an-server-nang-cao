using FootballField.API.Modules.NotificationManagement.Repositories;
using FootballField.API.Modules.NotificationManagement.Services;
using FootballField.API.Modules.NotificationManagement.Helpers;

namespace FootballField.API.Modules.NotificationManagement;

public static class NotificationModule
{
    public static IServiceCollection AddNotificationModule(this IServiceCollection services)
    {
        // Register Notification Repositories
        services.AddScoped<INotificationRepository, NotificationRepository>();
        
        // ⚠️ WARNING: SseRepository uses in-memory storage - only suitable for single-instance deployment
        // For production with multiple instances, replace with Redis-based implementation
        services.AddSingleton<ISseRepository, SseRepository>();
        
        // Register Notification Services
        services.AddScoped<INotificationService, NotificationService>();
        
        // Register Helpers
        services.AddScoped<NotificationHelper>();
        
        return services;
    }
}
