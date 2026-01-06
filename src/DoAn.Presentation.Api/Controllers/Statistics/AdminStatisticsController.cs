using DoAn.Presentation.Api.Middlewares;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using DoAn.Core.Application.DTOs.Base;
using DoAn.Core.Application.Interfaces.Statistics;
using DoAn.Core.Application.DTOs.Statistic;

namespace DoAn.Presentation.Api.Controllers.Statistics
{
    [ApiController]
    [Route("api/statistics/admin")]
    [Authorize]
    [HasPermission("system.view_statistics")]
    public class AdminStatisticsController : ControllerBase
    {
        private readonly IAdminStatisticsService _adminStatisticsService;

        public AdminStatisticsController(IAdminStatisticsService adminStatisticsService)
        {
            _adminStatisticsService = adminStatisticsService;
        }

        /// <summary>
        /// Get comprehensive dashboard statistics for admin
        /// GET /api/statistics/admin/dashboard
        /// </summary>
        [HttpGet("dashboard")]
        public async Task<IActionResult> GetDashboardStats()
        {
            var stats = await _adminStatisticsService.GetAdminDashboardStatsAsync();
            return Ok(ApiResponse<AdminDashboardStatsDto>.Ok(stats, "Lấy thống kê dashboard thành công"));
        }

        /// <summary>
        /// Get system growth chart data (users, bookings, revenue over time)
        /// GET /api/statistics/admin/growth?startDate=2026-01-01&endDate=2026-01-31
        /// </summary>
        [HttpGet("growth")]
        public async Task<IActionResult> GetSystemGrowthChart(
            [FromQuery] DateTime? startDate = null,
            [FromQuery] DateTime? endDate = null)
        {
            // Default to last 30 days
            var end = endDate ?? DateTime.Today;
            var start = startDate ?? end.AddDays(-29);

            if (end < start)
                return BadRequest(ApiResponse<string>.Fail("endDate phải sau startDate", 400));

            if ((end - start).TotalDays > 365)
                return BadRequest(ApiResponse<string>.Fail("Chỉ cho phép lấy tối đa 365 ngày", 400));

            var data = await _adminStatisticsService.GetSystemGrowthChartAsync(start, end);
            return Ok(ApiResponse<IEnumerable<SystemGrowthDto>>.Ok(data, "Lấy dữ liệu tăng trưởng hệ thống thành công"));
        }

        /// <summary>
        /// Get top complexes by bookings and revenue
        /// GET /api/statistics/admin/top-complexes?limit=10
        /// </summary>
        [HttpGet("top-complexes")]
        public async Task<IActionResult> GetTopComplexes([FromQuery] int limit = 10)
        {
            if (limit < 1 || limit > 50)
                return BadRequest(ApiResponse<string>.Fail("Limit phải từ 1 đến 50", 400));

            var data = await _adminStatisticsService.GetTopComplexesAsync(limit);
            return Ok(ApiResponse<IEnumerable<TopComplexDto>>.Ok(data, "Lấy top cụm sân thành công"));
        }

        /// <summary>
        /// Get top customers by bookings and spending
        /// GET /api/statistics/admin/top-customers?limit=10
        /// </summary>
        [HttpGet("top-customers")]
        public async Task<IActionResult> GetTopCustomers([FromQuery] int limit = 10)
        {
            if (limit < 1 || limit > 50)
                return BadRequest(ApiResponse<string>.Fail("Limit phải từ 1 đến 50", 400));

            var data = await _adminStatisticsService.GetTopCustomersAsync(limit);
            return Ok(ApiResponse<IEnumerable<TopCustomerDto>>.Ok(data, "Lấy top khách hàng thành công"));
        }

        /// <summary>
        /// Get booking status distribution (pie chart data)
        /// GET /api/statistics/admin/booking-distribution
        /// </summary>
        [HttpGet("booking-distribution")]
        public async Task<IActionResult> GetBookingStatusDistribution()
        {
            var data = await _adminStatisticsService.GetBookingStatusDistributionAsync();
            return Ok(ApiResponse<IEnumerable<BookingStatusDistributionDto>>.Ok(data, "Lấy phân bố trạng thái booking thành công"));
        }

        /// <summary>
        /// Get revenue chart data for admin (all system)
        /// GET /api/statistics/admin/revenue?startDate=2026-01-01&endDate=2026-01-31
        /// </summary>
        [HttpGet("revenue")]
        public async Task<IActionResult> GetRevenueChart(
            [FromQuery] DateTime? startDate = null,
            [FromQuery] DateTime? endDate = null)
        {
            // Default to last 30 days
            var end = endDate ?? DateTime.Today;
            var start = startDate ?? end.AddDays(-29);

            if (end < start)
                return BadRequest(ApiResponse<string>.Fail("endDate phải sau startDate", 400));

            if ((end - start).TotalDays > 365)
                return BadRequest(ApiResponse<string>.Fail("Chỉ cho phép lấy tối đa 365 ngày", 400));

            var data = await _adminStatisticsService.GetAdminRevenueChartAsync(start, end);
            return Ok(ApiResponse<IEnumerable<RevenueChartDto>>.Ok(data, "Lấy dữ liệu doanh thu thành công"));
        }

        /// <summary>
        /// Get recent bookings across all system
        /// GET /api/statistics/admin/recent-bookings?limit=10
        /// </summary>
        [HttpGet("recent-bookings")]
        public async Task<IActionResult> GetRecentBookings([FromQuery] int limit = 10)
        {
            if (limit < 1 || limit > 100)
                return BadRequest(ApiResponse<string>.Fail("Limit phải từ 1 đến 100", 400));

            var data = await _adminStatisticsService.GetRecentBookingsAsync(limit);
            return Ok(ApiResponse<IEnumerable<UpcomingBookingDto>>.Ok(data, "Lấy booking gần đây thành công"));
        }
    }
}
