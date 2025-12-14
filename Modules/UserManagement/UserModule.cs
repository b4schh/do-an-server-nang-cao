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
        services.AddScoped<IPermissionRepository, PermissionRepository>();
        services.AddScoped<IRoleRepository, RoleRepository>();
        
        // Register User Services
        services.AddScoped<IUserService, UserService>();
        services.AddScoped<IPermissionService, PermissionService>();
        services.AddScoped<IRoleService, RoleService>();
        services.AddScoped<IPermissionManagementService, PermissionManagementService>();
        
        return services;
    }
}
