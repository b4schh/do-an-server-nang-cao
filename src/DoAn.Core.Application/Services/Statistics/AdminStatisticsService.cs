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

            // Get all data
            var allBookings = (await _bookingRepository.GetAllAsync()).ToList();
            var allUsers = (await _userRepository.GetAllAsync()).ToList();
            var allComplexes = (await _complexRepository.GetAllAsync()).ToList();
            var allFields = (await _fieldRepository.GetAllAsync()).ToList();
            var allReviews = (await _reviewRepository.GetAllAsync()).ToList();

            // User statistics
            var totalUsers = allUsers.Count;
            var totalCustomers = allUsers.Count(u => u.UserRoles != null && u.UserRoles.Any(ur => ur.Role.Name == "Customer"));
            var totalOwners = allUsers.Count(u => u.UserRoles != null && u.UserRoles.Any(ur => ur.Role.Name == "Owner"));

            // Complex and Field statistics
            var totalComplexes = allComplexes.Count;
            var activeComplexes = allComplexes.Count(c => c.IsActive);
            var totalFields = allFields.Count;

            // Booking statistics by status
            var totalBookings = allBookings.Count;
            var todayBookings = allBookings.Count(b => b.BookingDate.Date == vietnamToday);
            var pendingBookings = allBookings.Count(b => b.BookingStatus == BookingStatus.Pending);
            var waitingForApprovalBookings = allBookings.Count(b => b.BookingStatus == BookingStatus.WaitingForApproval);
            var confirmedBookings = allBookings.Count(b => b.BookingStatus == BookingStatus.Confirmed);
            var completedBookings = allBookings.Count(b => b.BookingStatus == BookingStatus.Completed);
            var cancelledBookings = allBookings.Count(b => 
                b.BookingStatus == BookingStatus.Cancelled || 
                b.BookingStatus == BookingStatus.Rejected ||
                b.BookingStatus == BookingStatus.NoShow ||
                b.BookingStatus == BookingStatus.Expired);

            // Revenue calculations
            var revenueBookings = allBookings
                .Where(b => 
                    b.BookingStatus == BookingStatus.Completed ||
                    b.BookingStatus == BookingStatus.Confirmed ||
                    b.BookingStatus == BookingStatus.NoShow)
                .ToList();

            var totalRevenue = revenueBookings
                .Sum(b => b.BookingStatus == BookingStatus.Completed ? b.TotalAmount : b.DepositAmount);

            var todayRevenue = revenueBookings
                .Where(b => b.BookingDate.Date == vietnamToday)
                .Sum(b => b.BookingStatus == BookingStatus.Completed ? b.TotalAmount : b.DepositAmount);

            var thisWeekRevenue = revenueBookings
                .Where(b => b.BookingDate.Date >= weekStart && b.BookingDate.Date <= vietnamToday)
                .Sum(b => b.BookingStatus == BookingStatus.Completed ? b.TotalAmount : b.DepositAmount);

            var thisMonthRevenue = revenueBookings
                .Where(b => b.BookingDate.Date >= monthStart && b.BookingDate.Date <= vietnamToday)
                .Sum(b => b.BookingStatus == BookingStatus.Completed ? b.TotalAmount : b.DepositAmount);

            // Review statistics
            var totalReviews = allReviews.Count;
            var pendingReviews = 0; // Review không có pending status, chỉ có IsVisible
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
            var allBookings = (await _bookingRepository.GetAllAsync()).ToList();
            var allUsers = (await _userRepository.GetAllAsync()).ToList();

            var result = new List<SystemGrowthDto>();
            for (var date = startDate.Date; date <= endDate.Date; date = date.AddDays(1))
            {
                var dayBookings = allBookings.Where(b => b.BookingDate.Date == date).ToList();
                var newUsers = allUsers.Count(u => u.CreatedAt.Date == date);
                
                var dayRevenue = dayBookings
                    .Where(b => 
                        b.BookingStatus == BookingStatus.Completed ||
                        b.BookingStatus == BookingStatus.Confirmed ||
                        b.BookingStatus == BookingStatus.NoShow)
                    .Sum(b => b.BookingStatus == BookingStatus.Completed ? b.TotalAmount : b.DepositAmount);

                result.Add(new SystemGrowthDto
                {
                    Date = date,
                    NewUsers = newUsers,
                    NewBookings = dayBookings.Count,
                    Revenue = dayRevenue
                });
            }

            return result;
        }

        public async Task<IEnumerable<TopComplexDto>> GetTopComplexesAsync(int limit = 10)
        {
            var allBookings = _bookingRepository.GetQueryableWithDetails().ToList();
            var allReviews = (await _reviewRepository.GetAllAsync()).ToList();

            var complexStats = allBookings
                .Where(b => b.Field?.Complex != null)
                .GroupBy(b => new
                {
                    ComplexId = b.Field.Complex.Id,
                    ComplexName = b.Field.Complex.Name,
                    OwnerName = b.Owner != null ? $"{b.Owner.LastName} {b.Owner.FirstName}".Trim() : "N/A"
                })
                .Select(g => new TopComplexDto
                {
                    ComplexId = g.Key.ComplexId,
                    ComplexName = g.Key.ComplexName,
                    OwnerName = g.Key.OwnerName,
                    BookingCount = g.Count(),
                    Revenue = g
                        .Where(b => 
                            b.BookingStatus == BookingStatus.Completed ||
                            b.BookingStatus == BookingStatus.Confirmed ||
                            b.BookingStatus == BookingStatus.NoShow)
                        .Sum(b => b.BookingStatus == BookingStatus.Completed ? b.TotalAmount : b.DepositAmount),
                    ReviewCount = allReviews.Count(r => r.Booking != null && r.Booking.Field != null && r.Booking.Field.ComplexId == g.Key.ComplexId),
                    AverageRating = allReviews
                        .Where(r => r.Booking != null && r.Booking.Field != null && r.Booking.Field.ComplexId == g.Key.ComplexId)
                        .Any() 
                        ? (decimal)allReviews.Where(r => r.Booking != null && r.Booking.Field != null && r.Booking.Field.ComplexId == g.Key.ComplexId).Average(r => r.Rating) 
                        : 0m
                })
                .OrderByDescending(c => c.Revenue)
                .Take(limit)
                .ToList();

            return complexStats;
        }

        public async Task<IEnumerable<TopCustomerDto>> GetTopCustomersAsync(int limit = 10)
        {
            var allBookings = _bookingRepository.GetQueryableWithDetails().ToList();

            var customerStats = allBookings
                .Where(b => b.Customer != null)
                .GroupBy(b => new
                {
                    CustomerId = b.CustomerId,
                    CustomerName = $"{b.Customer!.LastName} {b.Customer.FirstName}".Trim(),
                    Phone = b.Customer.Phone ?? "N/A"
                })
                .Select(g => new TopCustomerDto
                {
                    CustomerId = g.Key.CustomerId,
                    CustomerName = g.Key.CustomerName,
                    Phone = g.Key.Phone,
                    BookingCount = g.Count(),
                    TotalSpent = g
                        .Where(b => 
                            b.BookingStatus == BookingStatus.Completed ||
                            b.BookingStatus == BookingStatus.Confirmed ||
                            b.BookingStatus == BookingStatus.NoShow)
                        .Sum(b => b.BookingStatus == BookingStatus.Completed ? b.TotalAmount : b.DepositAmount)
                })
                .OrderByDescending(c => c.TotalSpent)
                .Take(limit)
                .ToList();

            return customerStats;
        }

        public async Task<IEnumerable<BookingStatusDistributionDto>> GetBookingStatusDistributionAsync()
        {
            var allBookings = (await _bookingRepository.GetAllAsync()).ToList();
            var totalCount = allBookings.Count;

            if (totalCount == 0)
                return new List<BookingStatusDistributionDto>();

            var distribution = allBookings
                .GroupBy(b => b.BookingStatus)
                .Select(g => new BookingStatusDistributionDto
                {
                    Status = g.Key.ToString(),
                    Count = g.Count(),
                    Percentage = Math.Round((decimal)g.Count() / totalCount * 100, 2)
                })
                .OrderByDescending(d => d.Count)
                .ToList();

            return distribution;
        }

        public async Task<IEnumerable<RevenueChartDto>> GetAdminRevenueChartAsync(DateTime startDate, DateTime endDate)
        {
            var allBookings = (await _bookingRepository.GetAllAsync()).ToList();

            var result = new List<RevenueChartDto>();
            for (var date = startDate.Date; date <= endDate.Date; date = date.AddDays(1))
            {
                var dayBookings = allBookings.Where(b => b.BookingDate.Date == date).ToList();
                
                var dayRevenue = dayBookings
                    .Where(b => 
                        b.BookingStatus == BookingStatus.Completed ||
                        b.BookingStatus == BookingStatus.Confirmed ||
                        b.BookingStatus == BookingStatus.NoShow)
                    .Sum(b => b.BookingStatus == BookingStatus.Completed ? b.TotalAmount : b.DepositAmount);

                result.Add(new RevenueChartDto
                {
                    Date = date,
                    Revenue = dayRevenue,
                    BookingCount = dayBookings.Count
                });
            }

            return result;
        }

        public async Task<IEnumerable<UpcomingBookingDto>> GetRecentBookingsAsync(int limit = 10)
        {
            var recentBookings = _bookingRepository.GetQueryableWithDetails()
                .OrderByDescending(b => b.CreatedAt)
                .Take(limit)
                .ToList();

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
