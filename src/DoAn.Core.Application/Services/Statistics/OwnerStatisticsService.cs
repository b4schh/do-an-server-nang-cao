using DoAn.Core.Domain.Entities;
using DoAn.Core.Application.DTOs;
using DoAn.Core.Application.DTOs.Statistic;
using Microsoft.Extensions.Logging;
using DoAn.Core.Application.Interfaces.Statistics;
using DoAn.Core.Application.Interfaces.Booking;
using DoAn.Core.Application.Interfaces.Field;
using DoAn.Core.Application.Common.Utils;

namespace DoAn.Core.Application.Services.Statistics
{
    public class OwnerStatisticsService : IStatisticsService
    {
        private readonly IBookingRepository _bookingRepository;
        private readonly IFieldRepository _fieldRepository;
        private readonly ILogger<OwnerStatisticsService> _logger;

        public OwnerStatisticsService(
            IBookingRepository bookingRepository,
            IFieldRepository fieldRepository,
            ILogger<OwnerStatisticsService> logger)
        {
            _bookingRepository = bookingRepository;
            _fieldRepository = fieldRepository;
            _logger = logger;
        }

        public async Task<OwnerDashboardStatsDto> GetOwnerDashboardStatsAsync(int ownerId)
        {
            var vietnamToday = TimeZoneHelper.VietnamNow.Date;
            
            // Get all bookings for owner
            var allBookings = await _bookingRepository.GetBookingsForOwnerAsync(ownerId);
            
            // Get complexes count
            var fields = await _fieldRepository.GetFieldsByOwnerIdAsync(ownerId);
            var complexIds = fields.Select(f => f.ComplexId).Distinct().ToList();
            var totalComplexes = complexIds.Count;
            var totalFields = fields.Count();
            var activeFields = fields.Count(f => f.IsActive);

            // Today bookings
            var todayBookings = allBookings.Where(b => b.BookingDate.Date == vietnamToday).ToList();
            
            // Statistics by status
            var pendingBookings = allBookings.Count(b => b.BookingStatus == BookingStatus.WaitingForApproval);
            var completedBookings = allBookings.Count(b => b.BookingStatus == BookingStatus.Completed);
            var cancelledBookings = allBookings.Count(b => 
                b.BookingStatus == BookingStatus.Cancelled || 
                b.BookingStatus == BookingStatus.Rejected ||
                b.BookingStatus == BookingStatus.NoShow);

            // Revenue calculations
            // Theo nghiệp vụ: 
            // - Confirmed: tiền cọc (deposit_amount)
            // - Completed: toàn bộ tiền (total_amount)
            // - NoShow: tiền cọc (deposit_amount)
            var totalRevenue = allBookings
                .Where(b => 
                    b.BookingStatus == BookingStatus.Completed ||
                    b.BookingStatus == BookingStatus.Confirmed ||
                    b.BookingStatus == BookingStatus.NoShow)
                .Sum(b => b.BookingStatus == BookingStatus.Completed 
                    ? b.TotalAmount 
                    : b.DepositAmount);
            
            var todayRevenue = todayBookings
                .Where(b => 
                    b.BookingStatus == BookingStatus.Completed ||
                    b.BookingStatus == BookingStatus.Confirmed ||
                    b.BookingStatus == BookingStatus.NoShow)
                .Sum(b => b.BookingStatus == BookingStatus.Completed 
                    ? b.TotalAmount 
                    : b.DepositAmount);
            
            var pendingRevenue = allBookings
                .Where(b => b.BookingStatus == BookingStatus.WaitingForApproval)
                .Sum(b => b.DepositAmount);

            // Average booking value
            var avgBookingValue = completedBookings > 0 
                ? totalRevenue / completedBookings 
                : 0;

            // Occupancy rate calculation (simplified - based on today's bookings)
            var totalSlotsToday = totalFields * 10; // Assume 10 slots per field per day
            var bookedSlotsToday = todayBookings.Count(b => 
                b.BookingStatus != BookingStatus.Cancelled && 
                b.BookingStatus != BookingStatus.Rejected);
            var occupancyRate = totalSlotsToday > 0 
                ? (decimal)bookedSlotsToday / totalSlotsToday * 100 
                : 0;

            return new OwnerDashboardStatsDto
            {
                TotalRevenue = totalRevenue,
                TodayRevenue = todayRevenue,
                PendingRevenue = pendingRevenue,
                TotalBookings = allBookings.Count(),
                TodayBookings = todayBookings.Count,
                PendingBookings = pendingBookings,
                CompletedBookings = completedBookings,
                CancelledBookings = cancelledBookings,
                TotalComplexes = totalComplexes,
                TotalFields = totalFields,
                ActiveFields = activeFields,
                OccupancyRate = Math.Round(occupancyRate, 2),
                AvgBookingValue = Math.Round(avgBookingValue, 0)
            };
        }

        public async Task<IEnumerable<RevenueChartDto>> GetOwnerRevenueChartAsync(int ownerId, DateTime startDate, DateTime endDate)
        {
            var bookings = await _bookingRepository.GetBookingsForOwnerAsync(ownerId);
            
            var revenueData = bookings
                .Where(b => 
                    b.BookingStatus == BookingStatus.Completed ||
                    b.BookingStatus == BookingStatus.Confirmed ||
                    b.BookingStatus == BookingStatus.NoShow)
                .Where(b => b.BookingDate.Date >= startDate.Date && b.BookingDate.Date <= endDate.Date)
                .GroupBy(b => b.BookingDate.Date)
                .Select(g => new RevenueChartDto
                {
                    Date = g.Key,
                    Revenue = g.Sum(b => b.BookingStatus == BookingStatus.Completed 
                        ? b.TotalAmount 
                        : b.DepositAmount),
                    BookingCount = g.Count()
                })
                .OrderBy(r => r.Date)
                .ToList();

            // Fill missing dates with zero revenue
            var allDates = new List<RevenueChartDto>();
            for (var date = startDate.Date; date <= endDate.Date; date = date.AddDays(1))
            {
                var existing = revenueData.FirstOrDefault(r => r.Date == date);
                allDates.Add(existing ?? new RevenueChartDto 
                { 
                    Date = date, 
                    Revenue = 0, 
                    BookingCount = 0 
                });
            }

            return allDates;
        }

        public async Task<IEnumerable<TopFieldDto>> GetTopFieldsAsync(int ownerId, int limit = 5)
        {
            var bookings = await _bookingRepository.GetBookingsForOwnerAsync(ownerId);
            
            var topFields = bookings
                .Where(b => b.BookingStatus == BookingStatus.Completed || b.BookingStatus == BookingStatus.Confirmed)
                .GroupBy(b => new { b.FieldId, b.Field.Name })
                .Select(g => new TopFieldDto
                {
                    FieldId = g.Key.FieldId,
                    FieldName = g.Key.Name,
                    BookingCount = g.Count(),
                    Revenue = g.Sum(b => b.TotalAmount)
                })
                .OrderByDescending(f => f.BookingCount)
                .Take(limit)
                .ToList();

            return topFields;
        }

        public async Task<IEnumerable<PeakHourDto>> GetPeakHoursAsync(int ownerId)
        {
            var bookings = await _bookingRepository.GetBookingsForOwnerAsync(ownerId);
            
            var peakHours = bookings
                .Where(b => b.BookingStatus == BookingStatus.Completed || b.BookingStatus == BookingStatus.Confirmed)
                .Where(b => b.TimeSlot != null)
                .GroupBy(b => b.TimeSlot!.StartTime.Hours)
                .Select(g => new PeakHourDto
                {
                    Hour = $"{g.Key:00}:00",
                    BookingCount = g.Count()
                })
                .OrderByDescending(h => h.BookingCount)
                .ToList();

            return peakHours;
        }

        public async Task<IEnumerable<UpcomingBookingDto>> GetUpcomingBookingsAsync(int ownerId, int hoursAhead = 3)
        {
            var vietnamNow = TimeZoneHelper.VietnamNow;
            var futureTime = vietnamNow.AddHours(hoursAhead);
            
            var bookings = await _bookingRepository.GetBookingsForOwnerAsync(ownerId);
            
            var upcomingBookings = bookings
                .Where(b => b.BookingStatus == BookingStatus.Confirmed)
                .Where(b => 
                {
                    var bookingDateTime = b.BookingDate.Date.Add(b.TimeSlot?.StartTime ?? TimeSpan.Zero);
                    return bookingDateTime >= vietnamNow && bookingDateTime <= futureTime;
                })
                .OrderBy(b => b.BookingDate)
                .ThenBy(b => b.TimeSlot!.StartTime)
                .Take(10)
                .Select(b => new UpcomingBookingDto
                {
                    Id = b.Id,
                    FieldName = b.Field.Name,
                    ComplexName = b.Field.Complex.Name,
                    CustomerName = $"{b.Customer.FirstName} {b.Customer.LastName}",
                    BookingDate = b.BookingDate,
                    StartTime = b.TimeSlot!.StartTime,
                    EndTime = b.TimeSlot!.EndTime,
                    TotalAmount = b.TotalAmount
                })
                .ToList();

            return upcomingBookings;
        }

        public async Task<IEnumerable<RevenueSummaryDto>> GetRevenueSummaryAsync(
            int ownerId, 
            RevenuePeriodType periodType, 
            DateTime startDate, 
            DateTime endDate)
        {
            var bookings = await _bookingRepository.GetBookingsForOwnerAsync(ownerId);
            
            var revenueBookings = bookings
                .Where(b => 
                    b.BookingStatus == BookingStatus.Completed ||
                    b.BookingStatus == BookingStatus.Confirmed ||
                    b.BookingStatus == BookingStatus.NoShow)
                .Where(b => b.BookingDate.Date >= startDate.Date && b.BookingDate.Date <= endDate.Date)
                .ToList();

            var groupedData = periodType switch
            {
                RevenuePeriodType.Daily => GroupByDaily(revenueBookings, startDate, endDate),
                RevenuePeriodType.Weekly => GroupByWeekly(revenueBookings, startDate, endDate),
                RevenuePeriodType.Monthly => GroupByMonthly(revenueBookings, startDate, endDate),
                RevenuePeriodType.Quarterly => GroupByQuarterly(revenueBookings, startDate, endDate),
                RevenuePeriodType.Yearly => GroupByYearly(revenueBookings, startDate, endDate),
                _ => new List<RevenueSummaryDto>()
            };

            return groupedData;
        }

        public async Task<RevenueComparisonDto> GetRevenueComparisonAsync(
            int ownerId,
            RevenuePeriodType periodType,
            DateTime currentStartDate,
            DateTime currentEndDate)
        {
            // Calculate previous period dates
            var periodLength = (currentEndDate - currentStartDate).Days + 1;
            var previousEndDate = currentStartDate.AddDays(-1);
            var previousStartDate = previousEndDate.AddDays(-periodLength + 1);

            // Get data for both periods
            var currentData = await GetRevenueSummaryAsync(ownerId, periodType, currentStartDate, currentEndDate);
            var previousData = await GetRevenueSummaryAsync(ownerId, periodType, previousStartDate, previousEndDate);

            var currentRevenue = currentData.Sum(d => d.TotalRevenue);
            var previousRevenue = previousData.Sum(d => d.TotalRevenue);
            var currentBookings = currentData.Sum(d => d.TotalBookings);
            var previousBookings = previousData.Sum(d => d.TotalBookings);

            var changeAmount = currentRevenue - previousRevenue;
            var changePercentage = previousRevenue > 0 
                ? (changeAmount / previousRevenue) * 100 
                : 0;

            return new RevenueComparisonDto
            {
                CurrentPeriod = $"{currentStartDate:yyyy-MM-dd} - {currentEndDate:yyyy-MM-dd}",
                PreviousPeriod = $"{previousStartDate:yyyy-MM-dd} - {previousEndDate:yyyy-MM-dd}",
                CurrentRevenue = currentRevenue,
                PreviousRevenue = previousRevenue,
                ChangeAmount = changeAmount,
                ChangePercentage = Math.Round(changePercentage, 2),
                CurrentBookings = currentBookings,
                PreviousBookings = previousBookings
            };
        }

        #region Private Helper Methods

        private List<RevenueSummaryDto> GroupByDaily(List<DoAn.Core.Domain.Entities.Booking> bookings, DateTime startDate, DateTime endDate)
        {
            var grouped = bookings
                .GroupBy(b => b.BookingDate.Date)
                .ToDictionary(g => g.Key, g => g.ToList());
            
            var result = new List<RevenueSummaryDto>();
            
            // Fill tất cả các ngày từ startDate đến endDate (7 ngày từ T2 đến CN)
            for (var date = startDate.Date; date <= endDate.Date; date = date.AddDays(1))
            {
                var dayBookings = grouped.ContainsKey(date) ? grouped[date] : new List<BookingEntity>();
                
                result.Add(new RevenueSummaryDto
                {
                    Period = date.ToString("yyyy-MM-dd"),
                    StartDate = date,
                    EndDate = date,
                    TotalRevenue = dayBookings.Sum(b => b.BookingStatus == BookingStatus.Completed ? b.TotalAmount : b.DepositAmount),
                    CompletedRevenue = dayBookings.Where(b => b.BookingStatus == BookingStatus.Completed).Sum(b => b.TotalAmount),
                    ConfirmedRevenue = dayBookings.Where(b => b.BookingStatus == BookingStatus.Confirmed).Sum(b => b.DepositAmount),
                    NoShowRevenue = dayBookings.Where(b => b.BookingStatus == BookingStatus.NoShow).Sum(b => b.DepositAmount),
                    TotalBookings = dayBookings.Count,
                    CompletedBookings = dayBookings.Count(b => b.BookingStatus == BookingStatus.Completed),
                    ConfirmedBookings = dayBookings.Count(b => b.BookingStatus == BookingStatus.Confirmed),
                    NoShowBookings = dayBookings.Count(b => b.BookingStatus == BookingStatus.NoShow)
                });
            }
            
            return result;
        }

        private List<RevenueSummaryDto> GroupByWeekly(List<BookingEntity> bookings, DateTime startDate, DateTime endDate)
        {
            var grouped = bookings
                .GroupBy(b => new
                {
                    Year = b.BookingDate.Year,
                    Week = System.Globalization.ISOWeek.GetWeekOfYear(b.BookingDate)
                })
                .ToDictionary(g => $"{g.Key.Year}-W{g.Key.Week:00}", g => g.ToList());
            
            var result = new List<RevenueSummaryDto>();
            
            // Fill tất cả các tuần từ startDate đến endDate (8 tuần)
            var currentWeekStart = startDate.Date;
            while (currentWeekStart <= endDate.Date)
            {
                var weekEnd = currentWeekStart.AddDays(6);
                if (weekEnd > endDate.Date)
                    weekEnd = endDate.Date;
                
                var year = System.Globalization.ISOWeek.GetYear(currentWeekStart);
                var weekNum = System.Globalization.ISOWeek.GetWeekOfYear(currentWeekStart);
                var weekKey = $"{year}-W{weekNum:00}";
                
                var weekBookings = grouped.ContainsKey(weekKey) ? grouped[weekKey] : new List<BookingEntity>();
                
                result.Add(new RevenueSummaryDto
                {
                    Period = weekKey,
                    StartDate = currentWeekStart,
                    EndDate = weekEnd,
                    TotalRevenue = weekBookings.Sum(b => b.BookingStatus == BookingStatus.Completed ? b.TotalAmount : b.DepositAmount),
                    CompletedRevenue = weekBookings.Where(b => b.BookingStatus == BookingStatus.Completed).Sum(b => b.TotalAmount),
                    ConfirmedRevenue = weekBookings.Where(b => b.BookingStatus == BookingStatus.Confirmed).Sum(b => b.DepositAmount),
                    NoShowRevenue = weekBookings.Where(b => b.BookingStatus == BookingStatus.NoShow).Sum(b => b.DepositAmount),
                    TotalBookings = weekBookings.Count,
                    CompletedBookings = weekBookings.Count(b => b.BookingStatus == BookingStatus.Completed),
                    ConfirmedBookings = weekBookings.Count(b => b.BookingStatus == BookingStatus.Confirmed),
                    NoShowBookings = weekBookings.Count(b => b.BookingStatus == BookingStatus.NoShow)
                });
                
                currentWeekStart = currentWeekStart.AddDays(7);
            }
            
            return result;
        }

        private List<RevenueSummaryDto> GroupByMonthly(List<BookingEntity> bookings, DateTime startDate, DateTime endDate)
        {
            var grouped = bookings
                .GroupBy(b => new { b.BookingDate.Year, b.BookingDate.Month })
                .ToDictionary(g => $"{g.Key.Year}-{g.Key.Month:00}", g => g.ToList());
            
            var result = new List<RevenueSummaryDto>();
            var year = startDate.Year; // Năm hiện tại
            
            // Fill đầy đủ 12 tháng
            for (int month = 1; month <= 12; month++)
            {
                var firstDay = new DateTime(year, month, 1);
                var lastDay = firstDay.AddMonths(1).AddDays(-1);
                var monthKey = $"{year}-{month:00}";
                
                var monthBookings = grouped.ContainsKey(monthKey) ? grouped[monthKey] : new List<BookingEntity>();
                
                result.Add(new RevenueSummaryDto
                {
                    Period = monthKey,
                    StartDate = firstDay,
                    EndDate = lastDay,
                    TotalRevenue = monthBookings.Sum(b => b.BookingStatus == BookingStatus.Completed ? b.TotalAmount : b.DepositAmount),
                    CompletedRevenue = monthBookings.Where(b => b.BookingStatus == BookingStatus.Completed).Sum(b => b.TotalAmount),
                    ConfirmedRevenue = monthBookings.Where(b => b.BookingStatus == BookingStatus.Confirmed).Sum(b => b.DepositAmount),
                    NoShowRevenue = monthBookings.Where(b => b.BookingStatus == BookingStatus.NoShow).Sum(b => b.DepositAmount),
                    TotalBookings = monthBookings.Count,
                    CompletedBookings = monthBookings.Count(b => b.BookingStatus == BookingStatus.Completed),
                    ConfirmedBookings = monthBookings.Count(b => b.BookingStatus == BookingStatus.Confirmed),
                    NoShowBookings = monthBookings.Count(b => b.BookingStatus == BookingStatus.NoShow)
                });
            }
            
            return result;
        }

        private List<RevenueSummaryDto> GroupByQuarterly(List<BookingEntity> bookings, DateTime startDate, DateTime endDate)
        {
            var grouped = bookings
                .GroupBy(b => new
                {
                    b.BookingDate.Year,
                    Quarter = (b.BookingDate.Month - 1) / 3 + 1
                })
                .ToDictionary(g => $"{g.Key.Year}-Q{g.Key.Quarter}", g => g.ToList());
            
            var result = new List<RevenueSummaryDto>();
            var year = startDate.Year; // Năm hiện tại
            
            // Fill đầy đủ 4 quý
            for (int quarter = 1; quarter <= 4; quarter++)
            {
                var firstMonth = (quarter - 1) * 3 + 1;
                var firstDay = new DateTime(year, firstMonth, 1);
                var lastDay = firstDay.AddMonths(3).AddDays(-1);
                var quarterKey = $"{year}-Q{quarter}";
                
                var quarterBookings = grouped.ContainsKey(quarterKey) ? grouped[quarterKey] : new List<BookingEntity>();
                
                result.Add(new RevenueSummaryDto
                {
                    Period = quarterKey,
                    StartDate = firstDay,
                    EndDate = lastDay,
                    TotalRevenue = quarterBookings.Sum(b => b.BookingStatus == BookingStatus.Completed ? b.TotalAmount : b.DepositAmount),
                    CompletedRevenue = quarterBookings.Where(b => b.BookingStatus == BookingStatus.Completed).Sum(b => b.TotalAmount),
                    ConfirmedRevenue = quarterBookings.Where(b => b.BookingStatus == BookingStatus.Confirmed).Sum(b => b.DepositAmount),
                    NoShowRevenue = quarterBookings.Where(b => b.BookingStatus == BookingStatus.NoShow).Sum(b => b.DepositAmount),
                    TotalBookings = quarterBookings.Count,
                    CompletedBookings = quarterBookings.Count(b => b.BookingStatus == BookingStatus.Completed),
                    ConfirmedBookings = quarterBookings.Count(b => b.BookingStatus == BookingStatus.Confirmed),
                    NoShowBookings = quarterBookings.Count(b => b.BookingStatus == BookingStatus.NoShow)
                });
            }
            
            return result;
        }

        private List<RevenueSummaryDto> GroupByYearly(List<BookingEntity> bookings, DateTime startDate, DateTime endDate)
        {
            var grouped = bookings
                .GroupBy(b => b.BookingDate.Year)
                .ToDictionary(g => g.Key.ToString(), g => g.ToList());
            
            var result = new List<RevenueSummaryDto>();
            var currentYear = endDate.Year; // Năm hiện tại
            
            // Fill đầy đủ 3 năm gần nhất
            for (int year = currentYear - 2; year <= currentYear; year++)
            {
                var firstDay = new DateTime(year, 1, 1);
                var lastDay = new DateTime(year, 12, 31);
                var yearKey = year.ToString();
                
                var yearBookings = grouped.ContainsKey(yearKey) ? grouped[yearKey] : new List<BookingEntity>();
                
                result.Add(new RevenueSummaryDto
                {
                    Period = yearKey,
                    StartDate = firstDay,
                    EndDate = lastDay,
                    TotalRevenue = yearBookings.Sum(b => b.BookingStatus == BookingStatus.Completed ? b.TotalAmount : b.DepositAmount),
                    CompletedRevenue = yearBookings.Where(b => b.BookingStatus == BookingStatus.Completed).Sum(b => b.TotalAmount),
                    ConfirmedRevenue = yearBookings.Where(b => b.BookingStatus == BookingStatus.Confirmed).Sum(b => b.DepositAmount),
                    NoShowRevenue = yearBookings.Where(b => b.BookingStatus == BookingStatus.NoShow).Sum(b => b.DepositAmount),
                    TotalBookings = yearBookings.Count,
                    CompletedBookings = yearBookings.Count(b => b.BookingStatus == BookingStatus.Completed),
                    ConfirmedBookings = yearBookings.Count(b => b.BookingStatus == BookingStatus.Confirmed),
                    NoShowBookings = yearBookings.Count(b => b.BookingStatus == BookingStatus.NoShow)
                });
            }
            
            return result;
        }

        #endregion
    }
}
