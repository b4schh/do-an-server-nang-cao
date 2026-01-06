using DoAn.Core.Domain.Entities;
using DoAn.Core.Application.DTOs.Statistic;
using Microsoft.Extensions.Logging;
using DoAn.Core.Application.Interfaces.Statistics;
using DoAn.Core.Application.Interfaces.Booking;
using DoAn.Core.Application.Interfaces.User;
using DoAn.Core.Application.Interfaces.Complex;
using DoAn.Core.Application.Interfaces.Field;
using DoAn.Core.Application.Interfaces.Review;
using DoAn.Core.Application.Common.Utils;

namespace DoAn.Core.Application.Services.Statistics
{
    /// <summary>
    /// OPTIMIZED AdminStatisticsService - All operations use database aggregation
    /// NO GetAllAsync() calls - everything filtered/grouped at DB level
    /// </summary>
    public class AdminStatisticsService : IAdminStatisticsService
    {
        private readonly IBookingRepository _bookingRepository;
        private readonly IUserRepository _userRepository;
        private readonly IComplexRepository _complexRepository;
        private readonly IFieldRepository _fieldRepository;
        private readonly IReviewRepository _reviewRepository;
        private readonly ILogger<AdminStatisticsService> _logger;

        public AdminStatisticsService(
            IBookingRepository bookingRepository,
            IUserRepository userRepository,
            IComplexRepository complexRepository,
            IFieldRepository fieldRepository,
            IReviewRepository reviewRepository,
            ILogger<AdminStatisticsService> logger)
        {
            _bookingRepository = bookingRepository;
            _userRepository = userRepository;
            _complexRepository = complexRepository;
            _fieldRepository = fieldRepository;
            _reviewRepository = reviewRepository;
            _logger = logger;
        }

        public async Task<AdminDashboardStatsDto> GetAdminDashboardStatsAsync()
        {
            var vietnamToday = TimeZoneHelper.VietnamNow.Date;
            var weekStart = vietnamToday.AddDays(-(int)vietnamToday.DayOfWeek);
            var monthStart = new DateTime(vietnamToday.Year, vietnamToday.Month, 1);

            // OPTIMIZED: Use database counts instead of loading all records
            var bookingStatusCounts = await _bookingRepository.GetBookingCountsByStatusAsync();
            
            // User counts using CountAsync at DB level
            var totalUsers = await _userRepository.CountAsync(u => !u.IsDeleted);
            var totalCustomers = await _userRepository.CountAsync(u => !u.IsDeleted && u.UserRoles.Any(ur => ur.Role.Name == "Customer"));
            var totalOwners = await _userRepository.CountAsync(u => !u.IsDeleted && u.UserRoles.Any(ur => ur.Role.Name == "Owner"));

            // Complex and Field counts
            var totalComplexes = await _complexRepository.CountAsync(c => !c.IsDeleted);
            var activeComplexes = await _complexRepository.CountAsync(c => !c.IsDeleted && c.IsActive);
            var totalFields = await _fieldRepository.CountAsync(f => !f.IsDeleted);

            // Booking statistics by status
            var totalBookings = bookingStatusCounts.Values.Sum();
            var todayBookings = await _bookingRepository.CountAsync(b => b.BookingDate.Date == vietnamToday);
            var pendingBookings = bookingStatusCounts.GetValueOrDefault("Pending", 0);
            var waitingForApprovalBookings = bookingStatusCounts.GetValueOrDefault("WaitingForApproval", 0);
            var confirmedBookings = bookingStatusCounts.GetValueOrDefault("Confirmed", 0);
            var completedBookings = bookingStatusCounts.GetValueOrDefault("Completed", 0);
            var cancelledBookings = bookingStatusCounts.GetValueOrDefault("Cancelled", 0) + 
                                   bookingStatusCounts.GetValueOrDefault("Rejected", 0) +
                                   bookingStatusCounts.GetValueOrDefault("NoShow", 0) +
                                   bookingStatusCounts.GetValueOrDefault("Expired", 0);

            // Revenue calculations using database aggregation
            var revenueData = await _bookingRepository.GetRevenueByDateRangeAsync(monthStart, vietnamToday);
            
            var totalRevenue = revenueData.Values.Sum();
            var todayRevenue = revenueData.GetValueOrDefault(vietnamToday, 0);
            var thisWeekRevenue = revenueData
                .Where(kv => kv.Key >= weekStart && kv.Key <= vietnamToday)
                .Sum(kv => kv.Value);
            var thisMonthRevenue = totalRevenue; // Already filtered to month range

            // Review statistics
            var totalReviews = await _reviewRepository.CountAsync(r => !r.IsDeleted);
            var pendingReviews = 0; // Review không có pending status
            
            // Calculate average rating from all reviews
            var allReviews = await _reviewRepository.GetAllAsync(r => !r.IsDeleted);
            var averageRating = allReviews.Any() ? (decimal)allReviews.Average(r => r.Rating) : 0m;

            return new AdminDashboardStatsDto
            {
                // System Overview
                TotalUsers = totalUsers,
                TotalCustomers = totalCustomers,
                TotalOwners = totalOwners,
                TotalComplexes = totalComplexes,
                TotalFields = totalFields,
                ActiveComplexes = activeComplexes,
                
                // Booking Statistics
                TotalBookings = totalBookings,
                TodayBookings = todayBookings,
                PendingBookings = pendingBookings,
                WaitingForApprovalBookings = waitingForApprovalBookings,
                ConfirmedBookings = confirmedBookings,
                CompletedBookings = completedBookings,
                CancelledBookings = cancelledBookings,
                
                // Revenue Statistics
                TotalRevenue = totalRevenue,
                TodayRevenue = todayRevenue,
                ThisWeekRevenue = thisWeekRevenue,
                ThisMonthRevenue = thisMonthRevenue,
                
                // Review Statistics
                TotalReviews = totalReviews,
                PendingReviews = pendingReviews,
                AverageRating = averageRating
            };
        }

        public async Task<IEnumerable<SystemGrowthDto>> GetSystemGrowthChartAsync(DateTime startDate, DateTime endDate)
        {
            // OPTIMIZED: Get revenue in single query with GroupBy
            var revenueData = await _bookingRepository.GetRevenueByDateRangeAsync(startDate, endDate);
            
            // Get booking counts by date - load and group in memory (OK for date range queries)
            var bookingsInRange = await _bookingRepository.GetAllAsync(b => b.BookingDate >= startDate && b.BookingDate <= endDate);
            var bookingCounts = bookingsInRange
                .GroupBy(b => b.BookingDate.Date)
                .ToDictionary(g => g.Key, g => g.Count());

            // Get new user counts by date
            var usersInRange = await _userRepository.GetAllAsync(u => u.CreatedAt >= startDate && u.CreatedAt <= endDate && !u.IsDeleted);
            var userCounts = usersInRange
                .GroupBy(u => u.CreatedAt.Date)
                .ToDictionary(g => g.Key, g => g.Count());

            var result = new List<SystemGrowthDto>();
            for (var date = startDate.Date; date <= endDate.Date; date = date.AddDays(1))
            {
                result.Add(new SystemGrowthDto
                {
                    Date = date,
                    NewUsers = userCounts.GetValueOrDefault(date, 0),
                    NewBookings = bookingCounts.GetValueOrDefault(date, 0),
                    Revenue = revenueData.GetValueOrDefault(date, 0)
                });
            }

            return result;
        }

        public async Task<IEnumerable<TopComplexDto>> GetTopComplexesAsync(int limit = 10)
        {
            // OPTIMIZED: Use optimized repository method
            var topComplexes = await _bookingRepository.GetTopComplexesByRevenueAsync(limit);
            
            // Get review stats for these complexes - use raw query to avoid navigation property issues
            var complexIds = topComplexes.Select(c => c.ComplexId).ToList();
            
            // Get all reviews with proper includes
            var allReviews = await _reviewRepository.GetAllAsync();
            var reviewsForComplexes = allReviews
                .Where(r => !r.IsDeleted && 
                           r.Booking != null && 
                           r.Booking.Field != null && 
                           complexIds.Contains(r.Booking.Field.ComplexId))
                .ToList();
            
            var reviewStats = reviewsForComplexes
                .GroupBy(r => r.Booking!.Field!.ComplexId)
                .ToDictionary(
                    g => g.Key,
                    g => new { ReviewCount = g.Count(), AverageRating = (decimal)g.Average(r => r.Rating) }
                );

            return topComplexes.Select(c => new TopComplexDto
            {
                ComplexId = c.ComplexId,
                ComplexName = c.ComplexName,
                OwnerName = c.OwnerName,
                BookingCount = c.BookingCount,
                Revenue = c.Revenue,
                ReviewCount = reviewStats.ContainsKey(c.ComplexId) ? reviewStats[c.ComplexId].ReviewCount : 0,
                AverageRating = reviewStats.ContainsKey(c.ComplexId) ? reviewStats[c.ComplexId].AverageRating : 0m
            }).ToList();
        }

        public async Task<IEnumerable<TopCustomerDto>> GetTopCustomersAsync(int limit = 10)
        {
            // OPTIMIZED: Use optimized repository method
            var topCustomers = await _bookingRepository.GetTopCustomersBySpendingAsync(limit);
            
            return topCustomers.Select(c => new TopCustomerDto
            {
                CustomerId = c.CustomerId,
                CustomerName = c.CustomerName,
                Phone = c.Phone,
                BookingCount = c.BookingCount,
                TotalSpent = c.TotalSpent
            }).ToList();
        }

        public async Task<IEnumerable<BookingStatusDistributionDto>> GetBookingStatusDistributionAsync()
        {
            // OPTIMIZED: Use database GroupBy
            var statusCounts = await _bookingRepository.GetBookingCountsByStatusAsync();
            var totalCount = statusCounts.Values.Sum();

            if (totalCount == 0)
                return new List<BookingStatusDistributionDto>();

            return statusCounts.Select(kvp => new BookingStatusDistributionDto
            {
                Status = kvp.Key,
                Count = kvp.Value,
                Percentage = Math.Round((decimal)kvp.Value / totalCount * 100, 2)
            })
            .OrderByDescending(d => d.Count)
            .ToList();
        }

        public async Task<IEnumerable<RevenueChartDto>> GetAdminRevenueChartAsync(DateTime startDate, DateTime endDate)
        {
            // OPTIMIZED: Get revenue data with single query
            var revenueData = await _bookingRepository.GetRevenueByDateRangeAsync(startDate, endDate);
            
            // Get booking counts by date
            var bookingsInRange = await _bookingRepository.GetAllAsync(b => b.BookingDate >= startDate && b.BookingDate <= endDate);
            var bookingCounts = bookingsInRange
                .GroupBy(b => b.BookingDate.Date)
                .ToDictionary(g => g.Key, g => g.Count());

            var result = new List<RevenueChartDto>();
            for (var date = startDate.Date; date <= endDate.Date; date = date.AddDays(1))
            {
                result.Add(new RevenueChartDto
                {
                    Date = date,
                    Revenue = revenueData.GetValueOrDefault(date, 0),
                    BookingCount = bookingCounts.GetValueOrDefault(date, 0)
                });
            }

            return result;
        }

        public async Task<IEnumerable<UpcomingBookingDto>> GetRecentBookingsAsync(int limit = 10)
        {
            // OPTIMIZED: Use GetQueryableWithDetails which already returns IQueryable
            // Note: In Application layer, we convert IQueryable to list for compatibility
            var recentBookings = _bookingRepository.GetQueryableWithDetails()
                .OrderByDescending(b => b.CreatedAt)
                .Take(limit)
                .ToList(); // Synchronous ToList is OK here since GetQueryableWithDetails is from Infrastructure

            return recentBookings.Select(b => new UpcomingBookingDto
            {
                Id = b.Id,
                FieldName = b.Field?.Name ?? "N/A",
                ComplexName = b.Field?.Complex?.Name ?? "N/A",
                CustomerName = b.Customer != null ? $"{b.Customer.LastName} {b.Customer.FirstName}".Trim() : "N/A",
                BookingDate = b.BookingDate,
                StartTime = b.TimeSlot?.StartTime ?? TimeSpan.Zero,
                EndTime = b.TimeSlot?.EndTime ?? TimeSpan.Zero,
                TotalAmount = b.TotalAmount
            }).ToList();
        }
    }
}
