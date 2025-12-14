using FootballField.API.Modules.OwnerSettingsManagement.Repositories;
using FootballField.API.Modules.OwnerSettingsManagement.Services;

namespace FootballField.API.Modules.OwnerSettingsManagement;

public static class OwnerSettingsModule
{
    public static IServiceCollection AddOwnerSettingsModule(this IServiceCollection services)
    {
        // Register OwnerSettings Repositories
        services.AddScoped<IOwnerSettingRepository, OwnerSettingRepository>();
        
        // Register OwnerSettings Services
        services.AddScoped<IOwnerSettingService, OwnerSettingService>();
        
        return services;
    }
}
