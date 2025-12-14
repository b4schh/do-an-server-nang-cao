using System;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using System.Security.Claims;
using System.ComponentModel;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Http;
using Microsoft.SemanticKernel;
using FootballField.API.Modules.BookingManagement.Services;
using FootballField.API.Modules.BookingManagement.Entities;
using System.Collections.Generic;
using FootballField.API.Shared.Dtos.BookingManagement;
using System.Globalization;
using System.Reflection;


namespace FootballField.API.Modules.AIManagement.Plugins
{
    /// <summary>
    /// BookingPlugin: cung cấp các hàm liên quan đến booking cho Kernel/Agent.
    /// - Tất cả truy vấn chỉ dùng userId lấy từ HttpContext (không cho phép truyền id).
    /// - Trả về text hoặc JSON (camelCase).
    /// - Tương thích với BookingDto (ưu tiên) hoặc entity Booking (fallback).
    /// </summary>
    public class BookingPlugin
    {
        private readonly IBookingService _bookingService;
        private readonly ILogger<BookingPlugin>? _logger;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public BookingPlugin(
            IBookingService bookingService,
            ILogger<BookingPlugin>? logger = null,
            IHttpContextAccessor httpContextAccessor = null!)
        {
            _bookingService = bookingService ?? throw new ArgumentNullException(nameof(bookingService));
            _logger = logger;
            _httpContextAccessor = httpContextAccessor ?? throw new ArgumentNullException(nameof(httpContextAccessor));
        }


        #region Helpers

        private int? GetCurrentUserId()
        {
            try
            {
                var ctx = _httpContextAccessor.HttpContext;
                if (ctx == null) return null;
                var user = ctx.User;
                if (user?.Identity == null || !user.Identity.IsAuthenticated) return null;

                var idClaim = user.FindFirst(ClaimTypes.NameIdentifier)
                              ?? user.FindFirst("sub")
                              ?? user.FindFirst("userId")
                              ?? user.FindFirst("id");

                if (idClaim == null) return null;
                if (int.TryParse(idClaim.Value, out var id)) return id;
                return null;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "GetCurrentUserId error");
                return null;
            }
        }

        private string SerializeObject(object obj)
            => JsonSerializer.Serialize(obj, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });

        // Normalize a booking (BookingDto or entity) into a common anonymous shape
        private object NormalizeBooking(object b)
        {
            // If b is BookingDto
            if (b is BookingDto dto)
            {
                var startAt = CombineDateAndTime(dto.BookingDate, dto.StartTime);
                var endAt = CombineDateAndTime(dto.BookingDate, dto.EndTime);
                return new
                {
                    id = dto.Id,
                    code = dto.Id > 0 ? $"BK-{dto.Id}" : null,
                    fieldId = dto.FieldId,
                    fieldName = dto.FieldName,
                    complexId = dto.ComplexId,
                    complexName = dto.ComplexName,
                    customerId = dto.CustomerId,
                    customerName = dto.CustomerName,
                    customerPhone = dto.CustomerPhone,
                    ownerId = dto.OwnerId,
                    ownerName = dto.OwnerName,
                    timeSlotId = dto.TimeSlotId,
                    startAt,
                    endAt,
                    bookingDate = dto.BookingDate,
                    holdExpiresAt = dto.HoldExpiresAt,
                    totalAmount = dto.TotalAmount,
                    depositAmount = dto.DepositAmount,
                    paymentProofUrl = dto.PaymentProofUrl,
                    note = dto.Note,
                    status = dto.BookingStatus.ToString(),
                    statusText = dto.BookingStatusText,
                    approvedBy = dto.ApprovedBy,
                    approvedByName = dto.ApprovedByName,
                    approvedAt = dto.ApprovedAt,
                    cancelledBy = dto.CancelledBy,
                    cancelledByName = dto.CancelledByName,
                    cancelledAt = dto.CancelledAt,
                    createdAt = dto.CreatedAt,
                    updatedAt = dto.UpdatedAt
                };
            }

            // If b is entity Booking (try to map by reflection)
            var t = b.GetType();
            object GetProp(string name)
            {
                var pi = t.GetProperty(name);
                if (pi == null) return null!;
                return pi.GetValue(b)!;
            }

            // Try to find expected fields; fallback gracefully
            var idVal = TryGet<int?>(b, "Id") ?? TryGet<int?>(b, "id");
            var bookingDateVal = TryGet<DateTime?>(b, "BookingDate") ?? TryGet<DateTime?>(b, "bookingDate") ?? DateTime.MinValue;
            var startTimeVal = TryGet<TimeSpan?>(b, "StartTime") ?? TryGet<TimeSpan?>(b, "startAt") ?? null;
            var endTimeVal = TryGet<TimeSpan?>(b, "EndTime") ?? TryGet<TimeSpan?>(b, "endAt") ?? null;
            DateTime? startAtDt = null;
            DateTime? endAtDt = null;
            if (bookingDateVal != DateTime.MinValue)
            {
                startAtDt = CombineDateAndTime(bookingDateVal, startTimeVal);
                endAtDt = CombineDateAndTime(bookingDateVal, endTimeVal);
            }

            return new
            {
                id = idVal,
                code = idVal.HasValue ? $"BK-{idVal.Value}" : null,
                fieldId = TryGet<int?>(b, "FieldId") ?? TryGet<int?>(b, "fieldId"),
                fieldName = TryGet<string>(b, "FieldName") ?? TryGet<string>(b, "fieldName"),
                complexId = TryGet<int?>(b, "ComplexId") ?? TryGet<int?>(b, "complexId"),
                complexName = TryGet<string>(b, "ComplexName") ?? TryGet<string>(b, "complexName"),
                customerId = TryGet<int?>(b, "CustomerId") ?? TryGet<int?>(b, "customerId"),
                customerName = TryGet<string>(b, "CustomerName") ?? TryGet<string>(b, "customerName"),
                customerPhone = TryGet<string>(b, "CustomerPhone") ?? TryGet<string>(b, "customerPhone"),
                ownerId = TryGet<int?>(b, "OwnerId") ?? TryGet<int?>(b, "ownerId"),
                ownerName = TryGet<string>(b, "OwnerName") ?? TryGet<string>(b, "ownerName"),
                timeSlotId = TryGet<int?>(b, "TimeSlotId") ?? TryGet<int?>(b, "timeSlotId"),
                startAt = startAtDt,
                endAt = endAtDt,                holdExpiresAt = TryGet<DateTime?>(b, "HoldExpiresAt"),
                totalAmount = TryGet<decimal?>(b, "TotalAmount") ?? TryGet<decimal?>(b, "Price"),
                depositAmount = TryGet<decimal?>(b, "DepositAmount"),
                paymentProofUrl = TryGet<string>(b, "PaymentProofUrl") ?? TryGet<string>(b, "PaymentProof"),
                note = TryGet<string>(b, "Note"),
                status = TryGet<object>(b, "BookingStatus")?.ToString() ?? TryGet<object>(b, "Status")?.ToString(),
                statusText = TryGet<string>(b, "BookingStatusText"),
                approvedBy = TryGet<int?>(b, "ApprovedBy"),
                approvedByName = TryGet<string>(b, "ApprovedByName"),
                approvedAt = TryGet<DateTime?>(b, "ApprovedAt"),
                cancelledBy = TryGet<int?>(b, "CancelledBy"),
                cancelledByName = TryGet<string>(b, "CancelledByName"),
                cancelledAt = TryGet<DateTime?>(b, "CancelledAt"),
                createdAt = TryGet<DateTime?>(b, "CreatedAt"),
                updatedAt = TryGet<DateTime?>(b, "UpdatedAt")
            };
        }

        private static T? TryGet<T>(object obj, string propName)
        {
            try
            {
                var pi = obj.GetType().GetProperty(propName);
                if (pi == null) return default;
                var v = pi.GetValue(obj);
                if (v == null) return default;
                return (T?)Convert.ChangeType(v, typeof(T));
            }
            catch
            {
                return default;
            }
        }

        private static DateTime? CombineDateAndTime(DateTime bookingDate, TimeSpan? time)
        {
            if (bookingDate == DateTime.MinValue) return null;
            if (!time.HasValue) return bookingDate;
            try
            {
                var dt = bookingDate.Date + time.Value;
                return DateTime.SpecifyKind(dt, DateTimeKind.Unspecified);
            }
            catch
            {
                return bookingDate;
            }
        }  // Parse a time string like "18:00" or a range "18:00-20:00"
private bool TryParseTimeRange(string timeRange, out TimeSpan start, out TimeSpan end)
{
    start = default;
    end = default;
    if (string.IsNullOrWhiteSpace(timeRange)) return false;
    timeRange = timeRange.Trim();
    if (timeRange.Contains("-"))
    {
        var parts = timeRange.Split('-', StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length != 2) return false;
        if (TimeSpan.TryParse(parts[0].Trim(), out start) && TimeSpan.TryParse(parts[1].Trim(), out end))
            return true;
        return false;
    }
    // single time -> assume 1 hour slot
    if (TimeSpan.TryParse(timeRange, out start))
    {
        end = start.Add(TimeSpan.FromHours(1));
        return true;
    }
    return false;
}



        #endregion

        #region Kernel functions (Booking queries)

        [KernelFunction]
        [Description("Lấy toàn bộ lịch sử booking của người dùng hiện tại (mới nhất trước). Trả về JSON array với các trường chính: id, code, fieldName, startAt, endAt, status, totalAmount, createdAt.")]
        public async Task<string> GetBookingHistoryAsync()
        {
            try
            {
                var userId = GetCurrentUserId();
                if (!userId.HasValue) return SerializeObject(new { message = "Người dùng chưa đăng nhập." });

                // Try to call service method that returns BookingDto list if available
                var mi = _bookingService.GetType().GetMethod("GetBookingsForCustomerAsync", new[] { typeof(int) });
                IEnumerable<object>? bookings = null;

                if (mi != null)
                {
                    var taskObj = mi.Invoke(_bookingService, new object[] { userId.Value });
                    if (taskObj is Task t)
                    {
                        await t.ConfigureAwait(false);
                        var resultProp = t.GetType().GetProperty("Result");
                        bookings = resultProp?.GetValue(t) as IEnumerable<object>;
                    }
                }

                // If null, try other overloads
                if (bookings == null)
                {
                    // try GetBookingsForCustomerAsync(int userId, BookingStatus status)
                    var mi2 = _bookingService.GetType().GetMethod("GetBookingsForCustomerAsync", new[] { typeof(int), typeof(BookingStatus) });
                    if (mi2 != null)
                    {
                        var taskObj = mi2.Invoke(_bookingService, new object[] { userId.Value, BookingStatus.Pending });
                        if (taskObj is Task t2)
                        {
                            await t2.ConfigureAwait(false);
                            var resultProp = t2.GetType().GetProperty("Result");
                            bookings = resultProp?.GetValue(t2) as IEnumerable<object>;
                        }
                    }
                }

                // Final fallback: call method without reflection (common)
                if (bookings == null)
                {
                    var direct = await _bookingService.GetBookingsForCustomerAsync(userId.Value);
                    bookings = direct?.Cast<object>();
                }

                if (bookings == null) return SerializeObject(Array.Empty<object>());

                var items = bookings
                    .Select(b => NormalizeBooking(b))
                    .OrderByDescending(x => ((dynamic)x).createdAt ?? DateTime.MinValue)
                    .Select(x => new
                    {
                        id = ((dynamic)x).id,
                        code = ((dynamic)x).code,
                        fieldName = ((dynamic)x).fieldName,
                        startAt = ((dynamic)x).startAt,
                        endAt = ((dynamic)x).endAt,
                        status = ((dynamic)x).status,
                        totalAmount = ((dynamic)x).totalAmount,
                        createdAt = ((dynamic)x).createdAt
                    })
                    .ToArray();

                return SerializeObject(items);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "GetBookingHistoryAsync error");
                return SerializeObject(new { message = "Đã xảy ra lỗi khi lấy lịch sử booking." });
            }
        }

        [KernelFunction]
        [Description("Lấy booking mới nhất của người dùng hiện tại. Trả về JSON object tóm tắt (id, code, fieldName, startAt, endAt, status, totalAmount).")]
        public async Task<string> GetLatestBookingAsync()
        {
            try
            {
                var userId = GetCurrentUserId();
                if (!userId.HasValue) return SerializeObject(new { message = "Người dùng chưa đăng nhập." });

                // Prefer dedicated service method if exists
                var mi = _bookingService.GetType().GetMethod("GetLatestBookingAsync", new[] { typeof(int) });
                object? latestObj = null;

                if (mi != null)
                {
                    var taskObj = mi.Invoke(_bookingService, new object[] { userId.Value });
                    if (taskObj is Task t)
                    {
                        await t.ConfigureAwait(false);
                        var resultProp = t.GetType().GetProperty("Result");
                        latestObj = resultProp?.GetValue(t);
                    }
                }

                if (latestObj == null)
                {
                    var all = await _bookingService.GetBookingsForCustomerAsync(userId.Value);
                    latestObj = all?.OrderByDescending(b => {
                        // try CreatedAt or BookingDate
                        var created = GetDateTimeSafe(b, "CreatedAt") ?? GetDateTimeSafe(b, "BookingDate") ?? DateTime.MinValue;
                        return created;
                    }).FirstOrDefault();
                }

                if (latestObj == null) return SerializeObject(new { message = "Không tìm thấy booking nào." });

                var nb = NormalizeBooking(latestObj);
                var outObj = new
                {
                    id = ((dynamic)nb).id,
                    code = ((dynamic)nb).code,
                    fieldName = ((dynamic)nb).fieldName,
                    startAt = ((dynamic)nb).startAt,
                    endAt = ((dynamic)nb).endAt,
                    status = ((dynamic)nb).status,
                    totalAmount = ((dynamic)nb).totalAmount
                };

                return SerializeObject(outObj);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "GetLatestBookingAsync error");
                return SerializeObject(new { message = "Đã xảy ra lỗi khi lấy booking mới nhất." });
            }
        }

        [KernelFunction]
        [Description("Kiểm tra xem người dùng có booking đang hoạt động (current / ongoing) không. Trả về 'Có' hoặc 'Không'.")]
        public async Task<string> HasActiveBookingsAsync()
        {
            try
            {
                var userId = GetCurrentUserId();
                if (!userId.HasValue) return "Người dùng chưa đăng nhập.";

                // Prefer dedicated service method if exists
                var mi = _bookingService.GetType().GetMethod("GetUpcomingBookingAsync", new[] { typeof(int) });
                object? upcoming = null;
                if (mi != null)
                {
                    var taskObj = mi.Invoke(_bookingService, new object[] { userId.Value });
                    if (taskObj is Task t)
                    {
                        await t.ConfigureAwait(false);
                        var resultProp = t.GetType().GetProperty("Result");
                        upcoming = resultProp?.GetValue(t);
                    }
                    return (upcoming != null) ? "Có" : "Không";
                }

                var bookings = await _bookingService.GetBookingsForCustomerAsync(userId.Value);
                if (bookings == null) return "Không";

                var now = DateTime.UtcNow;
                bool active = bookings.Any(b =>
                {
                    var start = GetDateTimeSafe(b, "StartAt") ?? CombineDateAndTime(GetDateTimeSafe(b, "BookingDate") ?? DateTime.MinValue,
                                                                                TryGet<TimeSpan?>(b, "StartTime") ?? TryGet<TimeSpan?>(b, "startTime"));
                    var end = GetDateTimeSafe(b, "EndAt") ?? CombineDateAndTime(GetDateTimeSafe(b, "BookingDate") ?? DateTime.MinValue,
                                                                                TryGet<TimeSpan?>(b, "EndTime") ?? TryGet<TimeSpan?>(b, "endTime"));
                    if (start == null || end == null) return false;
                    return start <= now && end >= now;
                });

                return active ? "Có" : "Không";
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "HasActiveBookingsAsync error");
                return "Đã xảy ra lỗi khi kiểm tra booking đang hoạt động.";
            }
        }

        [KernelFunction]
        [Description("Lấy các booking đang chờ xác nhận (Pending) của người dùng. Trả về JSON array tóm tắt (id, code, fieldName, startAt, endAt, totalAmount, createdAt).")]
        public async Task<string> GetPendingBookingsAsync()
        {
            try
            {
                var userId = GetCurrentUserId();
                if (!userId.HasValue) return SerializeObject(new { message = "Người dùng chưa đăng nhập." });

                // Try service overload that accepts status
                var mi = _bookingService.GetType().GetMethod("GetBookingsForCustomerAsync", new[] { typeof(int), typeof(BookingStatus) });
                IEnumerable<object>? bookings = null;

                if (mi != null)
                {
                    var taskObj = mi.Invoke(_bookingService, new object[] { userId.Value, BookingStatus.Pending });
                    if (taskObj is Task t)
                    {
                        await t.ConfigureAwait(false);
                        var resultProp = t.GetType().GetProperty("Result");
                        bookings = resultProp?.GetValue(t) as IEnumerable<object>;
                    }
                }

                if (bookings == null)
                {
                    var all = await _bookingService.GetBookingsForCustomerAsync(userId.Value);
                    bookings = all?.Where(b => {
                        var st = TryGet<object>(b, "BookingStatus")?.ToString() ?? TryGet<object>(b, "Status")?.ToString();
                        return string.Equals(st, BookingStatus.Pending.ToString(), StringComparison.OrdinalIgnoreCase);
                    }).Cast<object>();
                }

                if (bookings == null || !bookings.Any()) return SerializeObject(new object[0]);

                var items = bookings.Select(b => NormalizeBooking(b))
                    .Select(x => new
                    {
                        id = ((dynamic)x).id,
                        code = ((dynamic)x).code,
                        fieldName = ((dynamic)x).fieldName,
                        startAt = ((dynamic)x).startAt,
                        endAt = ((dynamic)x).endAt,
                        totalAmount = ((dynamic)x).totalAmount,
                        createdAt = ((dynamic)x).createdAt
                    }).ToArray();

                return SerializeObject(items);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "GetPendingBookingsAsync error");
                return SerializeObject(new { message = "Đã xảy ra lỗi khi lấy booking chờ xác nhận." });
            }
        }

        [KernelFunction]
        [Description("Kiểm tra xem người dùng có booking vào ngày cụ thể (yyyy-MM-dd) không. Truyền ngày dưới dạng chuỗi. Trả về 'Có' hoặc 'Không' hoặc thông báo lỗi.")]
        public async Task<string> HasBookingOnDateAsync(string dateIso)
        {
            try
            {
                var userId = GetCurrentUserId();
                if (!userId.HasValue) return "Người dùng chưa đăng nhập.";
                if (!DateTime.TryParse(dateIso, out var date)) return "Ngày truyền vào không hợp lệ. Dùng định dạng yyyy-MM-dd hoặc ISO.";

                var dayStart = date.Date;
                var dayEnd = dayStart.AddDays(1).AddTicks(-1);

                var bookings = await _bookingService.GetBookingsForCustomerAsync(userId.Value);
                if (bookings == null) return "Không tìm thấy booking.";

                var exists = bookings.Any(b =>
                {
                    var start = GetDateTimeSafe(b, "StartAt") ?? CombineDateAndTime(GetDateTimeSafe(b, "BookingDate") ?? DateTime.MinValue,
                                                                                     TryGet<TimeSpan?>(b, "StartTime") ?? TryGet<TimeSpan?>(b, "startTime"));
                    var end = GetDateTimeSafe(b, "EndAt") ?? CombineDateAndTime(GetDateTimeSafe(b, "BookingDate") ?? DateTime.MinValue,
                                                                                   TryGet<TimeSpan?>(b, "EndTime") ?? TryGet<TimeSpan?>(b, "endTime"));
                    if (start == null || end == null) return false;
                    return start <= dayEnd && end >= dayStart;
                });

                return exists ? "Có" : "Không";
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "HasBookingOnDateAsync error");
                return "Đã xảy ra lỗi khi kiểm tra booking theo ngày.";
            }
        }

        [KernelFunction]
        [Description("Tổng số tiền người dùng đã chi cho booking trong tháng (month: 1-12, year: yyyy). Trả về JSON { total } hoặc message nếu chưa có chức năng server.")]
        public async Task<string> GetTotalPaidByMonthAsync(int month, int year)
        {
            try
            {
                var userId = GetCurrentUserId();
                if (!userId.HasValue) return SerializeObject(new { message = "Người dùng chưa đăng nhập." });

                // Prefer dedicated service method if exists
                var mi = _bookingService.GetType().GetMethod("GetTotalPaidByMonthAsync", new[] { typeof(int), typeof(int), typeof(int) });
                if (mi != null)
                {
                    var taskObj = mi.Invoke(_bookingService, new object[] { userId.Value, month, year });
                    if (taskObj is Task t)
                    {
                        await t.ConfigureAwait(false);
                        var resultProp = t.GetType().GetProperty("Result");
                        var total = resultProp?.GetValue(t);
                        return SerializeObject(new { total = total ?? 0m });
                    }
                }

                // Fallback compute from bookings
                var bookings = await _bookingService.GetBookingsForCustomerAsync(userId.Value);
                if (bookings == null) return SerializeObject(new { total = 0m });

                decimal totalPaid = 0m;
                foreach (var b in bookings)
                {
                    var bookingDate = GetDateTimeSafe(b, "BookingDate") ?? GetDateTimeSafe(b, "CreatedAt") ?? DateTime.MinValue;
                    if (bookingDate == DateTime.MinValue) continue;
                    if (bookingDate.Year == year && bookingDate.Month == month)
                    {
                        var statusStr = TryGet<object>(b, "BookingStatus")?.ToString() ?? TryGet<object>(b, "Status")?.ToString();
                        if (string.Equals(statusStr, BookingStatus.Completed.ToString(), StringComparison.OrdinalIgnoreCase) ||
                            string.Equals(statusStr, BookingStatus.Completed.ToString(), StringComparison.OrdinalIgnoreCase))
                        {
                            totalPaid += TryGet<decimal?>(b, "TotalAmount") ?? TryGet<decimal?>(b, "Price") ?? 0m;
                        }
                    }
                }

                return SerializeObject(new { total = totalPaid });
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "GetTotalPaidByMonthAsync error");
                return SerializeObject(new { message = "Đã xảy ra lỗi khi tính tổng chi tiêu." });
            }
        }


        /// <summary>
/// Kiểm tra slot trống. Tham số 'place' chấp nhận tên sân hoặc tên complex — plugin sẽ cố resolve.
/// Trả về JSON { available: true/false, fieldName, message } hoặc message nếu cần service bổ sung.
/// </summary>
[KernelFunction]
[Description("Kiểm tra xem sân (theo tên sân hoặc tên cụm) có trống vào ngày + khung giờ đã cho hay không. Params: place (tên sân hoặc tên cụm), dateIso (yyyy-MM-dd), timeRange (e.g. '18:00' or '18:00-19:00').")]
public async Task<string> CheckSlotAvailableAsync(string place, string dateIso, string timeRange)
{
    try
    {
        var userId = GetCurrentUserId();
        if (!userId.HasValue) return SerializeObject(new { message = "Người dùng chưa đăng nhập." });

        if (!DateTime.TryParse(dateIso, out var date)) return SerializeObject(new { message = "Ngày không hợp lệ. Dùng yyyy-MM-dd." });
        if (!TryParseTimeRange(timeRange, out var startTime, out var endTime)) return SerializeObject(new { message = "Khung giờ không hợp lệ. Ví dụ '18:00' hoặc '18:00-20:00'." });

        // 1) Nếu service có CheckSlotAvailable(complexId/fieldId, DateTime, TimeSpan start, TimeSpan end) -> gọi
        var svcType = _bookingService.GetType();

        // Try by field name resolution: if IBookingService has method CheckSlotAvailable(fieldId, date, start, end)
        var miFieldCheck = svcType.GetMethod("CheckSlotAvailable", new[] { typeof(int), typeof(DateTime), typeof(TimeSpan), typeof(TimeSpan) });
        if (miFieldCheck != null)
        {
            // Need resolve fieldId from place -> try to call IFieldService if available
            var fieldId = await ResolveFieldIdByNameAsync(place);
            if (!fieldId.HasValue) return SerializeObject(new { message = $"Không tìm thấy sân/complex tên '{place}'. Cần IFieldService hoặc IComplexService để resolve tên->id." });

            var taskObj = miFieldCheck.Invoke(_bookingService, new object[] { fieldId.Value, date.Date, startTime, endTime });
            if (taskObj is Task t)
            {
                await t.ConfigureAwait(false);
                var res = t.GetType().GetProperty("Result")?.GetValue(t);
                return SerializeObject(new { available = res, field = place });
            }
        }

        // Try signature: CheckSlotAvailable(complexId, date, start, end)
        var miComplexCheck = svcType.GetMethod("CheckSlotAvailable", new[] { typeof(string), typeof(DateTime), typeof(TimeSpan), typeof(TimeSpan) });
        if (miComplexCheck != null)
        {
            // maybe service accepts complex name directly
            var taskObj = miComplexCheck.Invoke(_bookingService, new object[] { place, date.Date, startTime, endTime });
            if (taskObj is Task t2)
            {
                await t2.ConfigureAwait(false);
                var res = t2.GetType().GetProperty("Result")?.GetValue(t2);
                return SerializeObject(new { available = res, place });
            }
        }

        // Try general method SearchAvailableFields(date, district, timeRange) if user asked by district
        // If place looks like district (we can't know) -> skip

        // If none exists, tell developer which methods to add
        return SerializeObject(new
        {
            message = "Chức năng kiểm tra slot chưa được implement trên server.",
            requiredServiceMethods = new[]
            {
                new { name = "CheckSlotAvailable", signature = "Task<bool> CheckSlotAvailable(int fieldId, DateTime date, TimeSpan start, TimeSpan end)", note = "Trả về true nếu slot trống." },
                new { name = "ResolveFieldByName", signature = "Task<int?> GetFieldIdByName(string fieldName) OR IFieldService.GetFieldByName", note = "Dùng để map tên sân sang id; nếu bạn có IFieldService hãy đăng ký và plugin sẽ dùng." }
            }
        });
    }
    catch (Exception ex)
    {
        _logger?.LogError(ex, "CheckSlotAvailableAsync error");
        return SerializeObject(new { message = "Đã xảy ra lỗi khi kiểm tra slot." });
    }
}

/// <summary>
/// Tìm các sân còn trống theo ngày + quận + khung giờ.
/// Trả về JSON array các sân tóm tắt: fieldId/fieldName/complexName/availableRanges...
/// </summary>
[KernelFunction]
[Description("Tìm các sân còn trống theo ngày, quận (district), và khung giờ. Params: dateIso (yyyy-MM-dd), district, timeRange (optional).")]
public async Task<string> SearchAvailableFieldsAsync(string dateIso, string district, string timeRange = null)
{
    try
    {
        var userId = GetCurrentUserId();
        if (!userId.HasValue) return SerializeObject(new { message = "Người dùng chưa đăng nhập." });

        if (!DateTime.TryParse(dateIso, out var date)) return SerializeObject(new { message = "Ngày không hợp lệ. Dùng yyyy-MM-dd." });

        TimeSpan? startTime = null;
        TimeSpan? endTime = null;
        if (!string.IsNullOrWhiteSpace(timeRange))
        {
            if (!TryParseTimeRange(timeRange, out var s, out var e))
                return SerializeObject(new { message = "Khung giờ không hợp lệ." });
            startTime = s; endTime = e;
        }

        var svcType = _bookingService.GetType();
        // prefer SearchAvailableFields(date, district, timeRange)
        var miSearch = svcType.GetMethod("SearchAvailableFields", new[] { typeof(DateTime), typeof(string), typeof(TimeSpan), typeof(TimeSpan) })
                       ?? svcType.GetMethod("SearchAvailableFields", new[] { typeof(DateTime), typeof(string), typeof(string) }); // timeRange string

        if (miSearch != null)
        {
            object[] args;
            if (miSearch.GetParameters().Length == 4)
                args = new object[] { date.Date, district, startTime ?? TimeSpan.Zero, endTime ?? TimeSpan.Zero };
            else
                args = new object[] { date.Date, district, timeRange ?? "" };

            var taskObj = miSearch.Invoke(_bookingService, args);
            if (taskObj is Task t)
            {
                await t.ConfigureAwait(false);
                var result = t.GetType().GetProperty("Result")?.GetValue(t) as IEnumerable<object>;
                return SerializeObject(result ?? Array.Empty<object>());
            }
        }

        // fallback: if we have GetAvailableTimeRanges(fieldId, date) we can iterate all fields in district (requires IFieldService)
        var fieldService = TryResolveFieldService();
        if (fieldService == null)
        {
            return SerializeObject(new
            {
                message = "Chức năng tìm sân trống chưa có. Cần một trong các method sau trên server:",
                requiredServiceMethods = new[]
                {
                    new { name="SearchAvailableFields", signature="Task<IEnumerable<FieldSummary>> SearchAvailableFields(DateTime date, string district, TimeSpan? start, TimeSpan? end)", note="Trả về danh sách sân rảnh" },
                    new { name="GetAvailableTimeRanges", signature="Task<IEnumerable<TimeRangeDto>> GetAvailableTimeRanges(int fieldId, DateTime date)", note="Nếu không có Search, cần IFieldService + GetAvailableTimeRanges" }
                }
            });
        }

        // If we have fieldService, ask it for fields in district then call booking service GetAvailableTimeRanges
        var fieldsInDistrict = await fieldService.GetFieldsByDistrictAsync(district); // assume exists
        if (fieldsInDistrict == null) return SerializeObject(Array.Empty<object>());

        var results = new List<object>();
        foreach (var f in fieldsInDistrict)
        {
            int fid = f.Id;
            // try booking method GetAvailableTimeRanges(fieldId, date)
            var miRanges = _bookingService.GetType().GetMethod("GetAvailableTimeRanges", new[] { typeof(int), typeof(DateTime) });
            if (miRanges == null) continue;
            var taskObj = miRanges.Invoke(_bookingService, new object[] { fid, date.Date });
            if (taskObj is Task t2)
            {
                await t2.ConfigureAwait(false);
                var ranges = t2.GetType().GetProperty("Result")?.GetValue(t2);
                results.Add(new { fieldId = fid, fieldName = f.Name, availableRanges = ranges });
            }
        }

        return SerializeObject(results);
    }
    catch (Exception ex)
    {
        _logger?.LogError(ex, "SearchAvailableFieldsAsync error");
        return SerializeObject(new { message = "Đã xảy ra lỗi khi tìm sân trống." });
    }
}

/// <summary>
/// Gợi ý các sân rảnh tối nay (ví dụ trong 3 giờ tới). Trả về JSON danh sách gợi ý.
/// </summary>
[KernelFunction]
[Description("Gợi ý sân đang rảnh tối nay (gồm fieldName, nextAvailableRange). Param: windowHours (option) = số giờ tiếp theo để kiểm tra).")]
public async Task<string> SuggestAvailableFieldsTonightAsync(int windowHours = 6)
{
    try
    {
        var userId = GetCurrentUserId();
        if (!userId.HasValue) return SerializeObject(new { message = "Người dùng chưa đăng nhập." });

        var now = DateTime.UtcNow;
        var until = now.AddHours(windowHours);

        // Prefer SearchAvailableFields if implemented (date=now.Date)
        var svcType = _bookingService.GetType();
        var miSearch = svcType.GetMethod("SearchAvailableFields", new[] { typeof(DateTime), typeof(string), typeof(TimeSpan), typeof(TimeSpan) });
        if (miSearch != null)
        {
            // district empty -> search globally
            var taskObj = miSearch.Invoke(_bookingService, new object[] { now.Date, "", now.TimeOfDay, until.TimeOfDay });
            if (taskObj is Task t)
            {
                await t.ConfigureAwait(false);
                var result = t.GetType().GetProperty("Result")?.GetValue(t) as IEnumerable<object>;
                return SerializeObject(result ?? Array.Empty<object>());
            }
        }

        return SerializeObject(new { message = "Chức năng gợi ý sân tối nay chưa được implement trên server. Hãy thêm SearchAvailableFields hoặc GetAvailableTimeRanges." });
    }
    catch (Exception ex)
    {
        _logger?.LogError(ex, "SuggestAvailableFieldsTonightAsync error");
        return SerializeObject(new { message = "Đã xảy ra lỗi khi gợi ý sân tối nay." });
    }
}

/// <summary>
/// Tạo booking (yêu cầu: fieldName hoặc complexName + date + timeRange + optional note). 
/// Plugin sẽ cố resolve fieldId và gọi service method CreateBookingAsync(userId, fieldId, date, start, end, deposit/payment info).
/// Nếu service chưa có, plugin sẽ trả về signature cần thêm.
/// </summary>
[KernelFunction]
[Description("Tạo booking: params: fieldOrComplexName, dateIso (yyyy-MM-dd), timeRange (e.g. '18:00-19:00'), note (optional).")]
public async Task<string> CreateBookingAsync(string fieldOrComplexName, string dateIso, string timeRange, string note = "")
{
    try
    {
        var userId = GetCurrentUserId();
        if (!userId.HasValue) return SerializeObject(new { message = "Người dùng chưa đăng nhập." });
        if (!DateTime.TryParse(dateIso, out var date)) return SerializeObject(new { message = "Ngày không hợp lệ." });
        if (!TryParseTimeRange(timeRange, out var startTime, out var endTime)) return SerializeObject(new { message = "Khung giờ không hợp lệ." });

        // Resolve fieldId
        var fieldId = await ResolveFieldIdByNameAsync(fieldOrComplexName);
        if (!fieldId.HasValue) return SerializeObject(new { message = $"Không tìm thấy sân '{fieldOrComplexName}'. Cần IFieldService để resolve tên->id." });

        // Try to call CreateBookingAsync(userId, fieldId, date, start, end, note)
        var miCreate = _bookingService.GetType().GetMethod("CreateBookingAsync", new[] { typeof(int), typeof(int), typeof(DateTime), typeof(TimeSpan), typeof(TimeSpan), typeof(string) });
        if (miCreate != null)
        {
            var taskObj = miCreate.Invoke(_bookingService, new object[] { userId.Value, fieldId.Value, date.Date, startTime, endTime, note });
            if (taskObj is Task t)
            {
                await t.ConfigureAwait(false);
                var result = t.GetType().GetProperty("Result")?.GetValue(t);
                return SerializeObject(new { success = true, booking = result });
            }
        }

        // If not found
        return SerializeObject(new
        {
            message = "Chức năng tạo booking chưa được implement. Cần method:",
            requiredServiceMethod = new { name = "CreateBookingAsync", signature = "Task<BookingDto> CreateBookingAsync(int userId, int fieldId, DateTime bookingDate, TimeSpan start, TimeSpan end, string note)", note = "Nên validate slot trống, create record, trả về BookingDto" }
        });
    }
    catch (Exception ex)
    {
        _logger?.LogError(ex, "CreateBookingAsync error");
        return SerializeObject(new { message = "Đã xảy ra lỗi khi tạo booking." });
    }
}

#endregion

// --- helper để resolve field service nếu có ---
private dynamic? TryResolveFieldService()
{
    try
    {
        // attempt to resolve IFieldService from DI via IHttpContextAccessor.HttpContext.RequestServices
        var sp = _httpContextAccessor.HttpContext?.RequestServices;
        if (sp == null) return null;

        // try common interface types
        var typesToTry = new[]
        {
            "FootballField.API.Modules.FieldManagement.Services.IFieldService",
            "FootballField.API.Modules.FieldManagement.IFieldService",
            "IFieldService"
        };

        foreach (var tn in typesToTry)
        {
            var t = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(a => GetTypesSafe(a))          // <-- sửa ở đây: truyền 'a'
                .FirstOrDefault(tt => tt.FullName == tn || tt.Name == tn);

            if (t == null) continue;

            var svc = _httpContextAccessor.HttpContext?.RequestServices.GetService(t);
            if (svc != null) return svc;
        }

        return null;
    }
    catch (Exception ex)
    {
        _logger?.LogError(ex, "TryResolveFieldService error");
        return null;
    }
}


// Resolve field id by name using fieldService or fallback (null)
private async Task<int?> ResolveFieldIdByNameAsync(string name)
{
    try
    {
        if (string.IsNullOrWhiteSpace(name)) return null;
        var fieldService = TryResolveFieldService();
        if (fieldService == null) return null;

        // try method GetFieldByNameAsync or GetByName
        var mi = fieldService.GetType().GetMethod("GetFieldByNameAsync", new[] { typeof(string) })
                 ?? fieldService.GetType().GetMethod("GetByNameAsync", new[] { typeof(string) })
                 ?? fieldService.GetType().GetMethod("GetFieldByName", new[] { typeof(string) });

        if (mi == null) return null;
        var taskObj = mi.Invoke(fieldService, new object[] { name });
        if (taskObj is Task t)
        {
            await t.ConfigureAwait(false);
            var res = t.GetType().GetProperty("Result")?.GetValue(t);
            if (res == null) return null;
            // try to extract Id property
            var pi = res.GetType().GetProperty("Id") ?? res.GetType().GetProperty("id");
            if (pi == null) return null;
            var idVal = pi.GetValue(res);
            if (idVal == null) return null;
            return Convert.ToInt32(idVal);
        }
        return null;
    }
    catch
    {
        return null;
    }
}

    [KernelFunction]
[Description("Lấy booking chi tiết theo mã booking (ví dụ 'ABC123'). Chỉ trả về booking nếu thuộc về người đang đăng nhập.")]
public async Task<string> GetBookingByCodeAsync(string code)
{
    try
    {
        var userId = GetCurrentUserId();
        if (!userId.HasValue) return SerializeObject(new { message = "Người dùng chưa đăng nhập." });
        if (string.IsNullOrWhiteSpace(code)) return SerializeObject(new { message = "Vui lòng cung cấp mã booking." });

        // prefer service method
        var mi = _bookingService.GetType().GetMethod("GetBookingByCodeAsync", new[] { typeof(int), typeof(string) });
        object? bookingObj = null;
        if (mi != null)
        {
            var taskObj = mi.Invoke(_bookingService, new object[] { userId.Value, code });
            if (taskObj is Task t)
            {
                await t.ConfigureAwait(false);
                bookingObj = t.GetType().GetProperty("Result")?.GetValue(t);
            }
        }
        else
        {
            // fallback: fetch all and match code
            var all = await _bookingService.GetBookingsForCustomerAsync(userId.Value);
            bookingObj = all?.FirstOrDefault(b =>
            {
                var c = TryGet<string>(b, "Code") ?? TryGet<string>(b, "code");
                return string.Equals(c, code, StringComparison.OrdinalIgnoreCase);
            });
        }

        if (bookingObj == null) return SerializeObject(new { message = "Không tìm thấy booking với mã đã cho (hoặc booking không thuộc về bạn)." });

        var normalized = NormalizeBooking(bookingObj);
        return SerializeObject(normalized);
    }
    catch (Exception ex)
    {
        _logger?.LogError(ex, "GetBookingByCodeAsync error");
        return SerializeObject(new { message = "Đã xảy ra lỗi khi lấy booking theo mã." });
    }
}

// Kernel function: hủy booking theo mã (user không cần biết id)
[KernelFunction]
[Description("Hủy booking theo mã (ví dụ 'ABC123'). Plugin sẽ kiểm tra quyền (chỉ owner) và trạng thái trước khi hủy. Param: code, reason (optional).")]
public async Task<string> CancelBookingByCodeAsync(string code, string reason = "")
{
    try
    {
        var userId = GetCurrentUserId();
        if (!userId.HasValue) return SerializeObject(new { success = false, message = "Người dùng chưa đăng nhập." });
        if (string.IsNullOrWhiteSpace(code)) return SerializeObject(new { success = false, message = "Vui lòng cung cấp mã booking." });

        // 1) resolve booking by code
        var miGet = _bookingService.GetType().GetMethod("GetBookingByCodeAsync", new[] { typeof(int), typeof(string) });
        object? bookingObj = null;
        if (miGet != null)
        {
            var taskObj = miGet.Invoke(_bookingService, new object[] { userId.Value, code });
            if (taskObj is Task t)
            {
                await t.ConfigureAwait(false);
                bookingObj = t.GetType().GetProperty("Result")?.GetValue(t);
            }
        }
        else
        {
            var all = await _bookingService.GetBookingsForCustomerAsync(userId.Value);
            bookingObj = all?.FirstOrDefault(b =>
            {
                var c = TryGet<string>(b, "Code") ?? TryGet<string>(b, "code");
                return string.Equals(c, code, StringComparison.OrdinalIgnoreCase);
            });
        }

        if (bookingObj == null) return SerializeObject(new { success = false, message = "Không tìm thấy booking với mã đã cho (hoặc booking không thuộc về bạn)." });

        // 2) validate status (cannot cancel already cancelled/completed)
        var statusStr = TryGet<object>(bookingObj, "BookingStatus")?.ToString() ?? TryGet<object>(bookingObj, "Status")?.ToString();
        if (string.IsNullOrWhiteSpace(statusStr)) statusStr = "Unknown";

        if (string.Equals(statusStr, BookingStatus.Cancelled.ToString(), StringComparison.OrdinalIgnoreCase) ||
            string.Equals(statusStr, BookingStatus.Completed.ToString(), StringComparison.OrdinalIgnoreCase))
        {
            return SerializeObject(new { success = false, message = $"Không thể hủy booking có trạng thái '{statusStr}'." });
        }

        // 3) extract bookingId
        var id = TryGet<int?>(bookingObj, "Id") ?? TryGet<int?>(bookingObj, "id");
        if (!id.HasValue) return SerializeObject(new { success = false, message = "Booking không có id hợp lệ. Cần service hỗ trợ GetBookingByCode trả id." });

        // 4) call CancelBooking service
        var miCancel = _bookingService.GetType().GetMethod("CancelBookingAsync", new[] { typeof(int), typeof(int), typeof(string) })
                      ?? _bookingService.GetType().GetMethod("CancelBookingAsync", new[] { typeof(int), typeof(int) })
                      ?? _bookingService.GetType().GetMethod("CancelBooking", new[] { typeof(int) });

        if (miCancel != null)
        {
            object[] args;
            if (miCancel.GetParameters().Length == 3)
                args = new object[] { id.Value, userId.Value, reason ?? "" };
            else if (miCancel.GetParameters().Length == 2)
                args = new object[] { id.Value, userId.Value };
            else
                args = new object[] { id.Value };

            var taskObj = miCancel.Invoke(_bookingService, args);
            if (taskObj is Task t2)
            {
                await t2.ConfigureAwait(false);
                var result = t2.GetType().GetProperty("Result")?.GetValue(t2);
                // result could be bool or BookingDto
                if (result is bool bres)
                {
                    return SerializeObject(new { success = bres, message = bres ? "Hủy booking thành công." : "Hủy booking thất bại." });
                }
                else if (result != null)
                {
                    return SerializeObject(new { success = true, booking = result });
                }
                else
                {
                    return SerializeObject(new { success = true, message = "Đã gửi yêu cầu hủy (server trả null)." });
                }
            }
            else
            {
                // non-async cancel
                var res = miCancel.Invoke(_bookingService, args);
                return SerializeObject(new { success = true, message = "Hủy booking (sync) đã thực hiện." });
            }
        }

        // If service lacks cancel method, return required signature
        return SerializeObject(new
        {
            success = false,
            message = "Server chưa hỗ trợ hủy booking programmatically. Cần thêm method CancelBookingAsync.",
            requiredServiceMethods = new[]
            {
                new { name = "GetBookingByCodeAsync", signature = "Task<BookingDto?> GetBookingByCodeAsync(int userId, string code)", note = "Trả về BookingDto có Id, status, ..." },
                new { name = "CancelBookingAsync", signature = "Task<bool> CancelBookingAsync(int bookingId, int cancelledByUserId, string reason = null)", note = "Cần validate quyền & trạng thái trước khi cancel." }
            }
        });
    }
    catch (Exception ex)
    {
        _logger?.LogError(ex, "CancelBookingByCodeAsync error");
        return SerializeObject(new { success = false, message = "Đã xảy ra lỗi khi hủy booking." });
    }
}

// Kernel function: hủy booking sắp tới (nếu user muốn hủy booking upcoming mới nhất)
[KernelFunction]
[Description("Hủy booking sắp tới (upcoming) của người dùng — plugin sẽ tìm booking sắp tới và hủy nếu có. Param: reason (optional).")]
public async Task<string> CancelUpcomingBookingAsync(string reason = "")
{
    try
    {
        var userId = GetCurrentUserId();
        if (!userId.HasValue) return SerializeObject(new { success = false, message = "Người dùng chưa đăng nhập." });

        // prefer service GetUpcomingBookingAsync(userId)
        var miUpcoming = _bookingService.GetType().GetMethod("GetUpcomingBookingAsync", new[] { typeof(int) });
        object? upcoming = null;
        if (miUpcoming != null)
        {
            var t = miUpcoming.Invoke(_bookingService, new object[] { userId.Value }) as Task;
            if (t != null)
            {
                await t.ConfigureAwait(false);
                upcoming = t.GetType().GetProperty("Result")?.GetValue(t);
            }
        }
        else
        {
            // fallback: search bookings and pick next start > now
            var all = await _bookingService.GetBookingsForCustomerAsync(userId.Value);
            var now = DateTime.UtcNow;
            upcoming = all?.OrderBy(b =>
            {
                var start = GetDateTimeSafe(b, "StartAt") ?? CombineDateAndTime(GetDateTimeSafe(b, "BookingDate") ?? DateTime.MinValue,
                                                                           TryGet<TimeSpan?>(b, "StartTime") ?? TryGet<TimeSpan?>(b, "startTime"));
                return start ?? DateTime.MaxValue;
            }).FirstOrDefault(b =>
            {
                var start = GetDateTimeSafe(b, "StartAt") ?? CombineDateAndTime(GetDateTimeSafe(b, "BookingDate") ?? DateTime.MinValue,
                                                                            TryGet<TimeSpan?>(b, "StartTime") ?? TryGet<TimeSpan?>(b, "startTime"));
                return start != null && start > now;
            });
        }

        if (upcoming == null) return SerializeObject(new { success = false, message = "Không tìm thấy booking sắp tới để hủy." });

        var code = TryGet<string>(upcoming, "Code") ?? TryGet<string>(upcoming, "code");
        if (!string.IsNullOrWhiteSpace(code))
        {
            return await CancelBookingByCodeAsync(code, reason);
        }

        var id = TryGet<int?>(upcoming, "Id") ?? TryGet<int?>(upcoming, "id");
        if (!id.HasValue) return SerializeObject(new { success = false, message = "Booking sắp tới không có id hợp lệ." });

        // call CancelBooking by id
        var miCancel = _bookingService.GetType().GetMethod("CancelBookingAsync", new[] { typeof(int), typeof(int), typeof(string) })
                      ?? _bookingService.GetType().GetMethod("CancelBookingAsync", new[] { typeof(int), typeof(int) })
                      ?? _bookingService.GetType().GetMethod("CancelBooking", new[] { typeof(int) });

        if (miCancel != null)
        {
            object[] args;
            if (miCancel.GetParameters().Length == 3) args = new object[] { id.Value, userId.Value, reason ?? "" };
            else if (miCancel.GetParameters().Length == 2) args = new object[] { id.Value, userId.Value };
            else args = new object[] { id.Value };

            var t = miCancel.Invoke(_bookingService, args) as Task;
            if (t != null)
            {
                await t.ConfigureAwait(false);
                var result = t.GetType().GetProperty("Result")?.GetValue(t);
                if (result is bool bres) return SerializeObject(new { success = bres, message = bres ? "Hủy booking thành công." : "Hủy thất bại." });
                return SerializeObject(new { success = true, booking = result });
            }
        }

        return SerializeObject(new { success = false, message = "Server chưa hỗ trợ hủy booking programmatically." });
    }
    catch (Exception ex)
    {
        _logger?.LogError(ex, "CancelUpcomingBookingAsync error");
        return SerializeObject(new { success = false, message = "Đã xảy ra lỗi khi hủy booking sắp tới." });
    }
}

        #region Utility: safe datetime getter

        private static DateTime? GetDateTimeSafe(object obj, string propName)
        {
            try
            {
                var pi = obj.GetType().GetProperty(propName);
                if (pi == null) return null;
                var v = pi.GetValue(obj);
                if (v == null) return null;
                if (v is DateTime dt) return dt;
                if (DateTime.TryParse(v.ToString(), out dt)) return dt;
                return null;
            }
            catch
            {
                return null;
            }
        }

                private static IEnumerable<Type> GetTypesSafe( Assembly a)
        {
            try { return a.GetTypes(); } catch { return Array.Empty<Type>(); }
        }

        #endregion
    

    

    }

}


