using FootballField.API.Services.Interfaces;

namespace FootballField.API.BackgroundJobs
{
    public class BookingExpirationBackgroundService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<BookingExpirationBackgroundService> _logger;
        private readonly TimeSpan _checkInterval = TimeSpan.FromMinutes(1); // Check every 1 minute

        public BookingExpirationBackgroundService(
            IServiceProvider serviceProvider,
            ILogger<BookingExpirationBackgroundService> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Booking Expiration Background Service is starting.");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await ProcessExpiredBookings(stoppingToken);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error occurred while processing expired bookings.");
                }

                await Task.Delay(_checkInterval, stoppingToken);
            }

            _logger.LogInformation("Booking Expiration Background Service is stopping.");
        }

        private async Task ProcessExpiredBookings(CancellationToken stoppingToken)
        {
            using var scope = _serviceProvider.CreateScope();
            var bookingService = scope.ServiceProvider.GetRequiredService<IBookingService>();

            _logger.LogInformation("Checking for expired bookings...");
            await bookingService.ProcessExpiredBookingsAsync();
        }
    }
}
