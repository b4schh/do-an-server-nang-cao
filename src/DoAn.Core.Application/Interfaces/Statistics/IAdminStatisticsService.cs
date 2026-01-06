using DoAn.Core.Application.DTOs.Statistic;

namespace DoAn.Core.Application.Interfaces.Statistics;

public interface IAdminStatisticsService
{
    /// <summary>
    /// Get comprehensive dashboard statistics for admin
    /// </summary>
    Task<AdminDashboardStatsDto> GetAdminDashboardStatsAsync();

    /// <summary>
    /// Get system growth chart data for specified date range
    /// </summary>
    Task<IEnumerable<SystemGrowthDto>> GetSystemGrowthChartAsync(DateTime startDate, DateTime endDate);

    /// <summary>
    /// Get top complexes by bookings and revenue
    /// </summary>
    Task<IEnumerable<TopComplexDto>> GetTopComplexesAsync(int limit = 10);

    /// <summary>
    /// Get top customers by bookings and spending
    /// </summary>
    Task<IEnumerable<TopCustomerDto>> GetTopCustomersAsync(int limit = 10);

    /// <summary>
    /// Get booking status distribution
    /// </summary>
    Task<IEnumerable<BookingStatusDistributionDto>> GetBookingStatusDistributionAsync();

    /// <summary>
    /// Get revenue chart data for admin (all system)
    /// </summary>
    Task<IEnumerable<RevenueChartDto>> GetAdminRevenueChartAsync(DateTime startDate, DateTime endDate);

    /// <summary>
    /// Get recent bookings across all system
    /// </summary>
    Task<IEnumerable<UpcomingBookingDto>> GetRecentBookingsAsync(int limit = 10);
}
