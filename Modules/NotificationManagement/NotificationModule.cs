using FootballField.API.Modules.NotificationManagement.Repositories;
using FootballField.API.Modules.NotificationManagement.Services;

namespace FootballField.API.Modules.NotificationManagement;

public static class NotificationModule
{
    public static IServiceCollection AddNotificationModule(this IServiceCollection services)
    {
        // Register Notification Repositories
        services.AddSingleton<ISseRepository, SseRepository>();
        
        // Register Notification Services
        services.AddScoped<INotificationService, NotificationService>();
        
        return services;
    }
}
