namespace DoAn.Core.Application.DTOs.Statistic;

/// <summary>
/// DTO for Owner Dashboard Statistics
/// </summary>
public class OwnerDashboardStatsDto
{
    public decimal TotalRevenue { get; set; }
    public decimal TodayRevenue { get; set; }
    public decimal PendingRevenue { get; set; }
    public int TotalBookings { get; set; }
    public int TodayBookings { get; set; }
    public int PendingBookings { get; set; }
    public int CompletedBookings { get; set; }
    public int CancelledBookings { get; set; }
    public int TotalComplexes { get; set; }
    public int TotalFields { get; set; }
    public int ActiveFields { get; set; }
    public decimal OccupancyRate { get; set; }
    public decimal AvgBookingValue { get; set; }
}

/// <summary>
/// DTO for Revenue Chart Data
/// </summary>
public class RevenueChartDto
{
    public DateTime Date { get; set; }
    public decimal Revenue { get; set; }
    public int BookingCount { get; set; }
}

/// <summary>
/// Revenue Period Type Enum
/// </summary>
public enum RevenuePeriodType
{
    Daily,      // Theo ngày
    Weekly,     // Theo tuần
    Monthly,    // Theo tháng
    Quarterly,  // Theo quý
    Yearly      // Theo năm
}

/// <summary>
/// DTO for Revenue Summary by Period
/// </summary>
public class RevenueSummaryDto
{
    public string Period { get; set; } = string.Empty; // e.g., "2024-01", "2024-Q1", "2024-W1"
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public decimal TotalRevenue { get; set; }
    public decimal ConfirmedRevenue { get; set; } // Từ Confirmed (deposit)
    public decimal CompletedRevenue { get; set; } // Từ Completed (total)
    public decimal NoShowRevenue { get; set; }    // Từ NoShow (deposit)
    public int TotalBookings { get; set; }
    public int CompletedBookings { get; set; }
    public int ConfirmedBookings { get; set; }
    public int NoShowBookings { get; set; }
}

/// <summary>
/// DTO for Revenue Comparison
/// </summary>
public class RevenueComparisonDto
{
    public string CurrentPeriod { get; set; } = string.Empty;
    public string PreviousPeriod { get; set; } = string.Empty;
    public decimal CurrentRevenue { get; set; }
    public decimal PreviousRevenue { get; set; }
    public decimal ChangeAmount { get; set; }
    public decimal ChangePercentage { get; set; }
    public int CurrentBookings { get; set; }
    public int PreviousBookings { get; set; }
}

/// <summary>
/// DTO for Top Field Statistics
/// </summary>
public class TopFieldDto
{
    public int FieldId { get; set; }
    public string FieldName { get; set; } = string.Empty;
    public int BookingCount { get; set; }
    public decimal Revenue { get; set; }
}

/// <summary>
/// DTO for Peak Hours Analysis
/// </summary>
public class PeakHourDto
{
    public string Hour { get; set; } = string.Empty;
    public int BookingCount { get; set; }
}

/// <summary>
/// DTO for Upcoming Bookings
/// </summary>
public class UpcomingBookingDto
{
    public int Id { get; set; }
    public string FieldName { get; set; } = string.Empty;
    public string ComplexName { get; set; } = string.Empty;
    public string CustomerName { get; set; } = string.Empty;
    public DateTime BookingDate { get; set; }
    public TimeSpan StartTime { get; set; }
    public TimeSpan EndTime { get; set; }
    public decimal TotalAmount { get; set; }
}

/// <summary>
/// DTO for Admin Dashboard Statistics
/// </summary>
public class AdminDashboardStatsDto
{
    // System Overview
    public int TotalUsers { get; set; }
    public int TotalCustomers { get; set; }
    public int TotalOwners { get; set; }
    public int TotalComplexes { get; set; }
    public int TotalFields { get; set; }
    public int ActiveComplexes { get; set; }
    
    // Booking Statistics
    public int TotalBookings { get; set; }
    public int TodayBookings { get; set; }
    public int PendingBookings { get; set; }
    public int ConfirmedBookings { get; set; }
    public int CompletedBookings { get; set; }
    public int CancelledBookings { get; set; }
    public int WaitingForApprovalBookings { get; set; }
    
    // Revenue Statistics
    public decimal TotalRevenue { get; set; }
    public decimal TodayRevenue { get; set; }
    public decimal ThisWeekRevenue { get; set; }
    public decimal ThisMonthRevenue { get; set; }
    
    // Review Statistics
    public int TotalReviews { get; set; }
    public int PendingReviews { get; set; }
    public decimal AverageRating { get; set; }
}

/// <summary>
/// DTO for System Growth Statistics
/// </summary>
public class SystemGrowthDto
{
    public DateTime Date { get; set; }
    public int NewUsers { get; set; }
    public int NewBookings { get; set; }
    public decimal Revenue { get; set; }
}

/// <summary>
/// DTO for Top Complex by Bookings/Revenue
/// </summary>
public class TopComplexDto
{
    public int ComplexId { get; set; }
    public string ComplexName { get; set; } = string.Empty;
    public string OwnerName { get; set; } = string.Empty;
    public int BookingCount { get; set; }
    public decimal Revenue { get; set; }
    public decimal AverageRating { get; set; }
    public int ReviewCount { get; set; }
}

/// <summary>
/// DTO for Top Customer Statistics
/// </summary>
public class TopCustomerDto
{
    public int CustomerId { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public int BookingCount { get; set; }
    public decimal TotalSpent { get; set; }
}

/// <summary>
/// DTO for Booking Status Distribution
/// </summary>
public class BookingStatusDistributionDto
{
    public string Status { get; set; } = string.Empty;
    public int Count { get; set; }
    public decimal Percentage { get; set; }
}
