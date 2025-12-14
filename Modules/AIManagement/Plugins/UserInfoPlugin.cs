// File: Modules/AIManagement/Plugins/UserInfoPlugin.cs
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

// Project services & entities (adjust namespaces if different)
using FootballField.API.Modules.UserManagement.Services;
using FootballField.API.Modules.BookingManagement.Services;
using FootballField.API.Shared.Storage;
using FootballField.API.Modules.BookingManagement;
using FootballField.API.Modules.BookingManagement.Entities;

namespace FootballField.API.Modules.AIManagement.Plugins
{
    /// <summary>
    /// Plugin cung cấp các hàm an toàn để Kernel/Agent có thể gọi.
    /// MỌI HÀM CHỈ TRUY VẤN THEO userId lấy từ HttpContext (không cho phép LLM đọc user khác).
    /// </summary>
    public class UserInfoPlugin
    {
        private readonly IUserService _userService;
        private readonly IBookingService _bookingService;
        private readonly IStorageService? _storage;
        private readonly ILogger<UserInfoPlugin>? _logger;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public UserInfoPlugin(
            IUserService userService,
            IBookingService bookingService,
            IStorageService? storage,
            ILogger<UserInfoPlugin>? logger,
            IHttpContextAccessor httpContextAccessor)
        {
            _userService = userService ?? throw new ArgumentNullException(nameof(userService));
            _bookingService = bookingService ?? throw new ArgumentNullException(nameof(bookingService));
            _storage = storage;
            _logger = logger;
            _httpContextAccessor = httpContextAccessor ?? throw new ArgumentNullException(nameof(httpContextAccessor));
        }


        /// <summary>
        /// Lấy userId hiện tại từ HttpContext claims.
        /// Hỗ trợ các claim phổ biến: ClaimTypes.NameIdentifier, "sub", "userId".
        /// Trả null nếu không có context hoặc chưa đăng nhập.
        /// </summary>
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
                              ?? user.FindFirst("userId");

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


      

    
        /// <summary>
        /// Lấy presigned avatar URL của người dùng đang đăng nhập.
        /// Trả về JSON: { "url": "...", "expiresAt": "2025-12-09T..." } hoặc message lỗi.
        /// </summary>
        [KernelFunction]
        [Description("Lấy avatar (presigned URL) của người dùng hiện tại. Trả về JSON { url, expiresAt } hoặc thông báo lỗi nếu không có avatar hoặc chưa đăng nhập.")]
        public async Task<string> GetCurrentUserAvatarAsync()
        {
            try
            {
                var userId = GetCurrentUserId();
                if (!userId.HasValue)
                    return SerializeObject(new { message = "Người dùng chưa đăng nhập." });

                var user = await _userService.GetUserByIdAsync(userId.Value);
                if (user == null)
                    return SerializeObject(new { message = "Không tìm thấy thông tin tài khoản." });

                if (string.IsNullOrWhiteSpace(user.AvatarUrl) || _storage == null)
                {
                    // Trả về avatar mặc định hoặc thông báo
                    // Bạn có thể thay bằng URL static của app: "/images/default-avatar.png"
                    return SerializeObject(new { message = "Không có avatar. Sử dụng avatar mặc định." });
                }

                var expiresIn = TimeSpan.FromMinutes(60);
                var presigned = await _storage.GetPresignedUrlAsync(user.AvatarUrl, expiresIn, CancellationToken.None);
                var expiresAt = DateTime.UtcNow.Add(expiresIn);

                return SerializeObject(new { url = presigned, expiresAt = expiresAt.ToString("o") });
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "GetCurrentUserAvatarAsync error");
                return SerializeObject(new { message = "Đã xảy ra lỗi khi lấy avatar." });
            }
        }

        /// <summary>
        /// Lấy thông tin cá nhân (tối giản) của người dùng hiện tại.
        /// </summary>
        [KernelFunction]
        [Description("Lấy thông tin cá nhân của người dùng hiện tại (họ tên, email, phone, role, avatar presigned nếu có).")]
        public async Task<string> GetUserProfileAsync()
        {
            try
            {
                var userId = GetCurrentUserId();
                if (!userId.HasValue)
                    return "Người dùng chưa đăng nhập.";

                var user = await _userService.GetUserByIdAsync(userId.Value);
                if (user == null)
                    return "Không tìm thấy thông tin tài khoản của bạn.";

                var sb = new StringBuilder();
                sb.AppendLine("Thông tin tài khoản của bạn:");
                sb.AppendLine($"- Họ và tên: {user.LastName} {user.FirstName}");
                sb.AppendLine($"- Email: {(!string.IsNullOrWhiteSpace(user.Email) ? user.Email : "Chưa cập nhật")}");
                sb.AppendLine($"- SĐT: {(!string.IsNullOrWhiteSpace(user.Phone) ? user.Phone : "Chưa cập nhật")}");
                sb.AppendLine($"- Trạng thái: {(user.Status != null ? user.Status.ToString() : "Không xác định")}");
                sb.AppendLine($"- Ngày tạo: {user.CreatedAt:dd/MM/yyyy}");

                if (!string.IsNullOrWhiteSpace(user.AvatarUrl) && _storage != null)
                {
                    var url = await _storage.GetPresignedUrlAsync(user.AvatarUrl, TimeSpan.FromMinutes(60), CancellationToken.None);
                    sb.AppendLine($"- Avatar: {url}");
                }

                return sb.ToString();
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "UserInfoPlugin.GetUserProfileAsync error");
                return "Đã xảy ra lỗi khi lấy thông tin tài khoản.";
            }
        }

      

       

         /// <summary>
        /// Đếm tổng số booking của người dùng hiện tại (văn bản).
        /// </summary>
        [KernelFunction]
        [Description("Đếm tổng số booking của người đang đăng nhập.")]
        public async Task<string> CountBookingsAsync()
        {
            try
            {
                var userId = GetCurrentUserId();
                if (!userId.HasValue) return "Người dùng chưa đăng nhập.";

                var bookings = await _bookingService.GetBookingsForCustomerAsync(userId.Value);
                var total = bookings?.Count() ?? 0;
                return $"Bạn đã có {total} lần đặt sân.";
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "UserInfoPlugin.CountBookingsAsync error");
                return "Đã xảy ra lỗi khi thống kê booking của bạn.";
            }
        }

        /// <summary>
        /// Đếm booking theo trạng thái cho người dùng (ví dụ Pending).
        /// </summary>
        [KernelFunction]
        [Description("Đếm booking theo trạng thái (ví dụ Pending) cho người đang đăng nhập.")]
        public async Task<string> CountBookingsByStatusAsync(BookingStatus status)
        {
            try
            {
                var userId = GetCurrentUserId();
                if (!userId.HasValue) return "Người dùng chưa đăng nhập.";

                var bookings = await _bookingService.GetBookingsForCustomerAsync(userId.Value, status);
                var total = bookings?.Count() ?? 0;
                return $"Bạn có {total} booking ở trạng thái '{status}'.";
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "UserInfoPlugin.CountBookingsByStatusAsync error");
                return "Đã xảy ra lỗi khi thống kê booking theo trạng thái.";
            }
        }

        /// <summary>
        /// Lấy tóm tắt N booking gần nhất (trả JSON array ngắn gọn).
        /// </summary>
        [KernelFunction]
        [Description("Lấy tóm tắt N booking gần nhất của người dùng (trả JSON array).")]
        public async Task<string> GetRecentBookingsAsync(int top = 5)
        {
            try
            {
                var userId = GetCurrentUserId();
                if (!userId.HasValue) return SerializeObject(new { message = "Người dùng chưa đăng nhập." });

                var bookings = await _bookingService.GetBookingsForCustomerAsync(userId.Value);
                if (bookings == null) return SerializeObject(Array.Empty<object>());

                var items = bookings
                    .OrderByDescending(b => b.CreatedAt)
                    .Take(Math.Max(1, top))
                    .Select(b => new
                    {

                        b.FieldName,
                        b.ComplexName,
                        b.BookingDate,
                        StartTime = b.StartTime?.ToString(@"hh\:mm"),
                        EndTime = b.EndTime?.ToString(@"hh\:mm"),
                        b.TotalAmount,
                        b.DepositAmount,
                        b.BookingStatus,
                        b.BookingStatusText,
                        b.CreatedAt
                    }).ToArray();

                return SerializeObject(items);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "UserInfoPlugin.GetRecentBookingsAsync error");
                return SerializeObject(new { message = "Đã xảy ra lỗi khi lấy booking." });
            }
        }

  
    }
}
