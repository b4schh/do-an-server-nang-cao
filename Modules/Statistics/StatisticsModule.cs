using FootballField.API.Modules.Statistics.Services;

namespace FootballField.API.Modules.Statistics;

public static class StatisticsModule
{
    public static IServiceCollection AddStatisticsModule(this IServiceCollection services)
    {
        // Register Statistics Services
        services.AddScoped<IStatisticsService, OwnerStatisticsService>();
        
        return services;
    }
}
