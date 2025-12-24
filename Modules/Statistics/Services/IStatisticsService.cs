using FootballField.API.Modules.Statistics.Dtos;

namespace FootballField.API.Modules.Statistics.Services
{
    public interface IStatisticsService
    {
        /// <summary>
        /// Get comprehensive dashboard statistics for an owner
        /// </summary>
        Task<OwnerDashboardStatsDto> GetOwnerDashboardStatsAsync(int ownerId);

        /// <summary>
        /// Get revenue chart data for specified date range
        /// </summary>
        Task<IEnumerable<RevenueChartDto>> GetOwnerRevenueChartAsync(int ownerId, DateTime startDate, DateTime endDate);

        /// <summary>
        /// Get top performing fields by booking count
        /// </summary>
        Task<IEnumerable<TopFieldDto>> GetTopFieldsAsync(int ownerId, int limit = 5);

        /// <summary>
        /// Get peak booking hours analysis
        /// </summary>
        Task<IEnumerable<PeakHourDto>> GetPeakHoursAsync(int ownerId);

        /// <summary>
        /// Get upcoming bookings within specified hours
        /// </summary>
        Task<IEnumerable<UpcomingBookingDto>> GetUpcomingBookingsAsync(int ownerId, int hoursAhead = 3);

        /// <summary>
        /// Get revenue summary grouped by period type
        /// </summary>
        Task<IEnumerable<RevenueSummaryDto>> GetRevenueSummaryAsync(
            int ownerId, 
            RevenuePeriodType periodType, 
            DateTime startDate, 
            DateTime endDate);

        /// <summary>
        /// Compare revenue between current and previous period
        /// </summary>
        Task<RevenueComparisonDto> GetRevenueComparisonAsync(
            int ownerId, 
            RevenuePeriodType periodType, 
            DateTime currentStartDate, 
            DateTime currentEndDate);
    }
}
