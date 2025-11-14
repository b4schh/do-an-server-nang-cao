using System;

namespace FootballField.API.Utils;

/// <summary>
/// Helper class để làm việc với múi giờ Việt Nam (Asia/Ho_Chi_Minh / SE Asia Standard Time)
/// </summary>
public static class TimeZoneHelper
{
    private static readonly TimeZoneInfo _vietnamTimeZone;

    static TimeZoneHelper()
    {
        // Windows sử dụng "SE Asia Standard Time"
        // Linux/Mac sử dụng "Asia/Ho_Chi_Minh"
        string timeZoneId = OperatingSystem.IsWindows()
            ? "SE Asia Standard Time"
            : "Asia/Ho_Chi_Minh";

        _vietnamTimeZone = TimeZoneInfo.FindSystemTimeZoneById(timeZoneId);
    }

    /// <summary>
    /// Lấy thời gian hiện tại theo múi giờ Việt Nam
    /// </summary>
    public static DateTime VietnamNow => TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, _vietnamTimeZone);

    /// <summary>
    /// Chuyển đổi UTC time sang Vietnam time
    /// </summary>
    public static DateTime ConvertFromUtc(DateTime utcDateTime)
    {
        if (utcDateTime.Kind != DateTimeKind.Utc)
            utcDateTime = DateTime.SpecifyKind(utcDateTime, DateTimeKind.Utc);

        return TimeZoneInfo.ConvertTimeFromUtc(utcDateTime, _vietnamTimeZone);
    }

    /// <summary>
    /// Chuyển đổi Vietnam time sang UTC time
    /// </summary>
    public static DateTime ConvertToUtc(DateTime vietnamDateTime)
    {
        return TimeZoneInfo.ConvertTimeToUtc(vietnamDateTime, _vietnamTimeZone);
    }

    /// <summary>
    /// Lấy ngày hôm nay theo múi giờ Việt Nam (không có giờ)
    /// </summary>
    public static DateTime VietnamToday => VietnamNow.Date;

    /// <summary>
    /// Lấy TimeZoneInfo của Việt Nam
    /// </summary>
    public static TimeZoneInfo VietnamTimeZone => _vietnamTimeZone;
}
