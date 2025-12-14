using FootballField.API.Modules.FieldManagement.Services;
using FootballField.API.Modules.FieldManagement.Repositories;

namespace FootballField.API.Modules.FieldManagement;

public static class FieldManagementModule
{
    public static IServiceCollection AddFieldManagementModule(this IServiceCollection services)
    {
        // Register Field Repositories
        services.AddScoped<IFieldRepository, FieldRepository>();
        services.AddScoped<ITimeSlotRepository, TimeSlotRepository>();
        
        // Register Field Services
        services.AddScoped<IFieldService, FieldService>();
        services.AddScoped<ITimeSlotService, TimeSlotService>();
        
        return services;
    }
}
