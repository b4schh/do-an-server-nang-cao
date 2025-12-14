using FootballField.API.Modules.LocationManagement.Repositories;
using FootballField.API.Modules.LocationManagement.Services;

namespace FootballField.API.Modules.LocationManagement;

public static class LocationManagementModule
{
    public static IServiceCollection AddLocationManagementModule(this IServiceCollection services)
    {
        // Register Location Repositories
        services.AddScoped<IProvinceRepository, ProvinceRepository>();
        services.AddScoped<IWardRepository, WardRepository>();
        
        // Register Location Services
        services.AddScoped<IProvinceService, ProvinceService>();
        services.AddScoped<IWardService, WardService>();
        services.AddScoped<LocationSeeder>();
        
        return services;
    }
}
