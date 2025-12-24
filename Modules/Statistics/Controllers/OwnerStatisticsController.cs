using FootballField.API.Modules.Statistics.Dtos;
using FootballField.API.Modules.Statistics.Services;
using FootballField.API.Shared.Dtos;
using FootballField.API.Shared.Middlewares;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace FootballField.API.Modules.Statistics.Controllers
{
    [ApiController]
    [Route("api/statistics/owner")]
    [Authorize]
    [HasPermission("booking.view_own_complex")]
    public class OwnerStatisticsController : ControllerBase
    {
        private readonly IStatisticsService _statisticsService;

        public OwnerStatisticsController(IStatisticsService statisticsService)
        {
            _statisticsService = statisticsService;
        }

        private int GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
            {
                throw new UnauthorizedAccessException("Không thể xác định user");
            }
            return userId;
        }

        /// <summary>
        /// Get dashboard statistics for owner
        /// </summary>
        [HttpGet("dashboard")]
        public async Task<IActionResult> GetDashboardStats()
        {
            var ownerId = GetCurrentUserId();
            var stats = await _statisticsService.GetOwnerDashboardStatsAsync(ownerId);
            return Ok(ApiResponse<OwnerDashboardStatsDto>.Ok(stats, "Lấy thống kê dashboard thành công"));
        }

        /// <summary>
        /// Get revenue chart data for a date range
        /// </summary>
        [HttpGet("revenue")]
        public async Task<IActionResult> GetRevenueChart(
            [FromQuery] DateTime? startDate = null,
            [FromQuery] DateTime? endDate = null)
        {
            var ownerId = GetCurrentUserId();
            
            // Default to last 7 days
            var start = startDate ?? DateTime.Today.AddDays(-6);
            var end = endDate ?? DateTime.Today;

            if (end < start)
                return BadRequest(ApiResponse<string>.Fail("endDate phải sau startDate", 400));

            if ((end - start).TotalDays > 90)
                return BadRequest(ApiResponse<string>.Fail("Chỉ cho phép lấy tối đa 90 ngày", 400));

            var data = await _statisticsService.GetOwnerRevenueChartAsync(ownerId, start, end);
            return Ok(ApiResponse<IEnumerable<RevenueChartDto>>.Ok(data, "Lấy dữ liệu doanh thu thành công"));
        }

        /// <summary>
        /// Get top fields by booking count
        /// </summary>
        [HttpGet("top-fields")]
        public async Task<IActionResult> GetTopFields([FromQuery] int limit = 5)
        {
            var ownerId = GetCurrentUserId();
            
            if (limit < 1 || limit > 20)
                return BadRequest(ApiResponse<string>.Fail("Limit phải từ 1 đến 20", 400));

            var data = await _statisticsService.GetTopFieldsAsync(ownerId, limit);
            return Ok(ApiResponse<IEnumerable<TopFieldDto>>.Ok(data, "Lấy top sân thành công"));
        }

        /// <summary>
        /// Get peak hours analysis
        /// </summary>
        [HttpGet("peak-hours")]
        public async Task<IActionResult> GetPeakHours()
        {
            var ownerId = GetCurrentUserId();
            var data = await _statisticsService.GetPeakHoursAsync(ownerId);
            return Ok(ApiResponse<IEnumerable<PeakHourDto>>.Ok(data, "Lấy giờ cao điểm thành công"));
        }

        /// <summary>
        /// Get upcoming bookings (next few hours)
        /// </summary>
        [HttpGet("upcoming")]
        public async Task<IActionResult> GetUpcomingBookings([FromQuery] int hoursAhead = 3)
        {
            var ownerId = GetCurrentUserId();
            
            if (hoursAhead < 1 || hoursAhead > 24)
                return BadRequest(ApiResponse<string>.Fail("hoursAhead phải từ 1 đến 24", 400));

            var data = await _statisticsService.GetUpcomingBookingsAsync(ownerId, hoursAhead);
            return Ok(ApiResponse<IEnumerable<UpcomingBookingDto>>.Ok(data, "Lấy booking sắp tới thành công"));
        }

        /// <summary>
        /// Get revenue summary by period type
        /// </summary>
        /// <param name="periodType">0=Daily, 1=Weekly, 2=Monthly, 3=Quarterly, 4=Yearly</param>
        [HttpGet("revenue-summary")]
        public async Task<IActionResult> GetRevenueSummary(
            [FromQuery] RevenuePeriodType periodType = RevenuePeriodType.Daily,
            [FromQuery] DateTime? startDate = null,
            [FromQuery] DateTime? endDate = null)
        {
            var ownerId = GetCurrentUserId();
            
            // Default date ranges based on period type
            var end = endDate ?? DateTime.Today;
            var start = startDate ?? periodType switch
            {
                RevenuePeriodType.Daily => end.AddDays(-30),
                RevenuePeriodType.Weekly => end.AddDays(-90),
                RevenuePeriodType.Monthly => end.AddMonths(-12),
                RevenuePeriodType.Quarterly => end.AddYears(-2),
                RevenuePeriodType.Yearly => end.AddYears(-5),
                _ => end.AddDays(-30)
            };

            var data = await _statisticsService.GetRevenueSummaryAsync(ownerId, periodType, start, end);
            return Ok(ApiResponse<IEnumerable<RevenueSummaryDto>>.Ok(data, "Lấy tổng hợp doanh thu thành công"));
        }

        /// <summary>
        /// Compare revenue between current and previous period
        /// </summary>
        [HttpGet("revenue-comparison")]
        public async Task<IActionResult> GetRevenueComparison(
            [FromQuery] RevenuePeriodType periodType = RevenuePeriodType.Monthly,
            [FromQuery] DateTime? startDate = null,
            [FromQuery] DateTime? endDate = null)
        {
            var ownerId = GetCurrentUserId();
            
            var end = endDate ?? DateTime.Today;
            var start = startDate ?? periodType switch
            {
                RevenuePeriodType.Daily => end,
                RevenuePeriodType.Weekly => end.AddDays(-6),
                RevenuePeriodType.Monthly => new DateTime(end.Year, end.Month, 1),
                RevenuePeriodType.Quarterly => new DateTime(end.Year, ((end.Month - 1) / 3) * 3 + 1, 1),
                RevenuePeriodType.Yearly => new DateTime(end.Year, 1, 1),
                _ => end.AddDays(-30)
            };

            var comparison = await _statisticsService.GetRevenueComparisonAsync(ownerId, periodType, start, end);
            return Ok(ApiResponse<RevenueComparisonDto>.Ok(comparison, "So sánh doanh thu thành công"));
        }
    }
}
