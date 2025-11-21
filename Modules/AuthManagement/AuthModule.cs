using FootballField.API.Modules.AuthManagement.Services;

namespace FootballField.API.Modules.AuthManagement;

public static class AuthModule
{
    public static IServiceCollection AddAuthModule(this IServiceCollection services)
    {
        // Register Auth Services
        services.AddScoped<IAuthService, AuthService>();
        
        return services;
    }
}
