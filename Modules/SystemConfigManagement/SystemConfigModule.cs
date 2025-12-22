using FootballField.API.Modules.SystemConfigManagement.Repositories;
using FootballField.API.Modules.SystemConfigManagement.Services;
using Microsoft.Extensions.DependencyInjection;

namespace FootballField.API.Modules.SystemConfigManagement
{
    public static class SystemConfigModule
    {
        public static IServiceCollection AddSystemConfigModule(this IServiceCollection services)
        {
            services.AddScoped<ISystemConfigRepository, SystemConfigRepository>();
            services.AddScoped<ISystemConfigService, SystemConfigService>();
            
            return services;
        }
    }
}
