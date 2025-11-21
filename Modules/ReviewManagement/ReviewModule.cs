using FootballField.API.Modules.ReviewManagement.Repositories;
using FootballField.API.Modules.ReviewManagement.Services;

namespace FootballField.API.Modules.ReviewManagement;

public static class ReviewModule
{
    public static IServiceCollection AddReviewModule(this IServiceCollection services)
    {
        // Register Review Repositories
        services.AddScoped<IReviewRepository, ReviewRepository>();
        
        // Register Review Services
        services.AddScoped<IReviewService, ReviewService>();
        
        return services;
    }
}
