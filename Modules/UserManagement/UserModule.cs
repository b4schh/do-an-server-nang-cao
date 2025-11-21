using Microsoft.Extensions.DependencyInjection;
using FootballField.API.Modules.UserManagement.Repositories;
using FootballField.API.Modules.UserManagement.Services;

namespace FootballField.API.Modules.UserManagement;

public static class UserModule
{
    public static IServiceCollection AddUserModule(this IServiceCollection services)
    {
        // Register User Repositories
        services.AddScoped<IUserRepository, UserRepository>();
        
        // Register User Services
        services.AddScoped<IUserService, UserService>();
        
        return services;
    }
}
