using System;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using System.ComponentModel;
using Microsoft.SemanticKernel;
using Microsoft.Extensions.Logging;
using FootballField.API.Modules.ComplexManagement.Services;
using FootballField.API.Modules.FieldManagement.Services;

using System.Security.Claims;
using FootballField.API.Modules.BookingManagement.Services;


using FootballField.API.Modules.ComplexManagement.Dtos;


namespace FootballField.API.Modules.AIManagement.Plugins
{
    /// <summary>
    /// ComplexPlugin: AI làm việc với CỤM SÂN (Complex)
    /// - Người dùng KHÔNG truyền ID
    /// - Tất cả dựa trên tên / quận / keyword
    /// - Field (sân con) chỉ là dữ liệu phụ bên trong
    /// </summary>
    public class ComplexPlugin
    {
        private readonly IComplexService _complexService;
        private readonly ILogger<ComplexPlugin>? _logger;

        private readonly ITimeSlotService _timeSlotService;

        private  readonly IFieldService _fieldService;

        private readonly IBookingService _bookingService;

        private readonly IHttpContextAccessor _httpContextAccessor;


        public ComplexPlugin(
            IComplexService complexService,
            ILogger<ComplexPlugin>? logger,
             IHttpContextAccessor httpContextAccessor)
        {
            _complexService = complexService;
            _logger = logger;
               _httpContextAccessor = httpContextAccessor;
        }

        private string Json(object obj)
            => JsonSerializer.Serialize(obj, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            });

        // ======================================================
        // A. THÔNG TIN CỤM SÂN
        // ======================================================

        [KernelFunction]
        [Description("Lấy thông tin chi tiết cụm sân theo tên (địa chỉ, mô tả, tiện ích, danh sách sân con).")]
        public async Task<string> GetComplexDetailAsync(string complexName)
        {
            try
            {
                var complex = await _complexService.GetByNameAsync(complexName);
                if (complex == null)
                    return Json(new { message = "Không tìm thấy cụm sân." });

                return Json(complex);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "GetComplexDetailAsync error");
                return Json(new { message = "Lỗi khi lấy thông tin cụm sân." });
            }
        }

        [KernelFunction]
        [Description("Lấy hình ảnh của cụm sân theo tên.")]
        public async Task<string> GetComplexImagesAsync(string complexName)
        {
            try
            {
                var complex = await _complexService.GetByNameAsync(complexName);
                if (complex == null)
                    return Json(new { message = "Không tìm thấy cụm sân." });

                var images = await _complexService.GetImagesAsync(complex.Id);
                return Json(images);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "GetComplexImagesAsync error");
                return Json(new { message = "Lỗi khi lấy hình ảnh cụm sân." });
            }
        }

       [KernelFunction]
[Description("Lấy giá sân trong cụm sân theo giờ. Nếu không truyền giờ sẽ trả bảng giá tất cả khung giờ.")]
public async Task<string> GetComplexPricingAsync(
    string complexName,
    string? time = null)
{
    try
    {
        var complex = await _complexService.GetByNameAsync(complexName);
        if (complex == null)
            return Json(new { message = "Không tìm thấy cụm sân." });

        var fields = await _fieldService.GetFieByComplexIdAsync(complex.Id);
        if (!fields.Any())
            return Json(new { message = "Cụm sân chưa có sân con." });

        var spanTime = TryParseTime(time);

        var result = new List<object>();

        foreach (var field in fields)
        {
            // Không nhập giờ → trả tất cả timeslot
            if (spanTime == null)
            {
                var slots = await _timeSlotService.GetTimeSlotsByFieldIdAsync(field.Id);
                result.Add(new
                {
                    fieldName = field.Name,
                    pricing = slots.Select(s => new
                    {
                        startTime = s.StartTime,
                        endTime = s.EndTime,
                        price = s.Price
                    })
                });
            }
            else
            {
                var slot = await _timeSlotService.GetPricingAsync(field.Id, spanTime.Value);
                if (slot != null)
                {
                    result.Add(new
                    {
                        fieldName = field.Name,
                        startTime = slot.StartTime,
                        endTime = slot.EndTime,
                        price = slot.Price
                    });
                }
            }
        }

        if (!result.Any())
            return Json(new { message = "Không có khung giờ phù hợp." });

        return Json(result);
    }
    catch (Exception ex)
    {
        _logger?.LogError(ex, "GetComplexPricingAsync error");
        return Json(new { message = "Lỗi khi lấy bảng giá cụm sân." });
    }
}

        [KernelFunction]
        [Description("Xem địa chỉ cụ thể của cụm sân.")]
        public async Task<string> GetComplexLocationAsync(string complexName)
        {
            try
            {
                var complex = await _complexService.GetByNameAsync(complexName);
                if (complex == null)
                    return Json(new { message = "Không tìm thấy cụm sân." });

                return Json(new
                {
                complex.Name,
                 complex.Street,
                complex.Ward,
               complex.Province,
                complex.Phone,
              complex.OpeningTime,
               complex.ClosingTime,
                complex.Description
                });
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "GetComplexLocationAsync error");
                return Json(new { message = "Lỗi khi lấy địa chỉ cụm sân." });
            }
        }

        // ======================================================
        // B. TÌM CỤM SÂN THEO NHU CẦU
        // ======================================================

        [KernelFunction]
        [Description("Tìm các cụm sân theo Tỉnh/Thành Phố (ví dụ: Tỉnh Hà Nội,Thành Phố Hồ Chí Minh).")]
        public async Task<string> SearchComplexByProvinceAsync(string province)
        {
            try
            {
                var complexes = await _complexService.SearchByDistrictAsync(province);
                return Json(complexes);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "SearchComplexByDistrictAsync error");
                return Json(new { message = "Lỗi khi tìm cụm sân theo quận." });
            }
        }


        [KernelFunction]
[Description("Gợi ý sân phù hợp cho người dùng dựa trên lịch sử đặt sân gần nhất.")]
public async Task<string> SuggestComplexForUserAsync()
{
    try
    {
        var userIdStr = _httpContextAccessor.HttpContext?
            .User
            .FindFirst(ClaimTypes.NameIdentifier)?
            .Value;

        if (!int.TryParse(userIdStr, out var userId))
            return Json(new { message = "Không xác định được người dùng." });

        // 1️⃣ Lấy 2 booking gần nhất
        var recentBookings = await _bookingService.GetRecentBookingsAsync(userId, 2);

        if (!recentBookings.Any())
            return Json(new { message = "Bạn chưa có lịch sử đặt sân để gợi ý." });

        var suggested = new List<ComplexDto>();
        var usedComplexIds = new HashSet<int>();

        foreach (var booking in recentBookings)
        {   var FieldId= booking.FieldId;
        var Field= await _fieldService.GetFieldByIdAsync(FieldId);

            var complex = await _complexService.GetComplexByIdAsync(Field.ComplexId);
            if (complex == null) continue;

            usedComplexIds.Add(complex.Id);

            // 2️⃣ Gợi ý 3 complex cùng quận
            var nearby = await _complexService.GetSuggestionByDistrictAsync(
                complex.Province,
                usedComplexIds,
                3
            );

            foreach (var c in nearby)
                usedComplexIds.Add(c.Id);

            suggested.AddRange(nearby);
        }

        return Json(new
        {
            message = "Dựa trên lịch sử đặt sân của bạn, chúng tôi gợi ý các cụm sân sau:",
            suggestions = suggested.DistinctBy(c => c.Id)
        });
    }
    catch (Exception ex)
    {
        _logger?.LogError(ex, "SuggestComplexForUserAsync error");
        return Json(new { message = "Lỗi khi gợi ý sân cho bạn." });
    }
}

        [KernelFunction]
        [Description("Tìm cụm sân theo từ khóa (tên, mô tả, địa chỉ).")]
        public async Task<string> SearchComplexByKeywordAsync(string keyword)
        {
            try
            {
                var complexes = await _complexService.SearchByKeywordAsync(keyword);
                return Json(complexes);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "SearchComplexByKeywordAsync error");
                return Json(new { message = "Lỗi khi tìm cụm sân theo từ khóa." });
            }
        }

        #region helpers
        private TimeSpan? TryParseTime(string? input)
{
    if (string.IsNullOrWhiteSpace(input))
        return null;

    input = input.ToLower().Trim();

    // 21h, 9h30
    if (input.Contains("h"))
    {
        input = input.Replace("giờ", "h")
                     .Replace("phút", "")
                     .Replace(" ", "");

        var parts = input.Split('h');
        if (int.TryParse(parts[0], out var hour))
        {
            var minute = 0;
            if (parts.Length > 1 && int.TryParse(parts[1], out var m))
                minute = m;

            return new TimeSpan(hour, minute, 0);
        }
    }

    // 09:30, 21:00
    if (TimeSpan.TryParse(input, out var span))
        return span;

    return null;
}

#endregion

        
    }
}
