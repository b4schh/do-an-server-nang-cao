using FootballField.API.Modules.BookingManagement.BackgroundJobs;
using FootballField.API.Modules.BookingManagement.Repositories;
using FootballField.API.Modules.BookingManagement.Services;

namespace FootballField.API.Modules.BookingManagement;

public static class BookingModule
{
    public static IServiceCollection AddBookingModule(this IServiceCollection services)
    {
        // Register Booking Repositories
        services.AddScoped<IBookingRepository, BookingRepository>();
        
        // Register Booking Services
        services.AddScoped<IBookingService, BookingService>();
        
        // Register Background Services
        services.AddHostedService<BookingExpirationBackgroundService>();
        
        return services;
    }
}
