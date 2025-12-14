using FootballField.API.Modules.ComplexManagement.Services;
using FootballField.API.Modules.ComplexManagement.Repositories;

namespace FootballField.API.Modules.ComplexManagement;

public static class ComplexManagementModule
{
    public static IServiceCollection AddComplexManagementModule(this IServiceCollection services)
    {
        // Register Complex Repositories
        services.AddScoped<IComplexRepository, ComplexRepository>();
        services.AddScoped<IComplexImageRepository, ComplexImageRepository>();
        
        // Register Complex Services
        services.AddScoped<IComplexService, ComplexService>();
        services.AddScoped<IComplexImageService, ComplexImageService>();
        
        return services;
    }
}
