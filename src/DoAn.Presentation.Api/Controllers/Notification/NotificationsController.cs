using System.Security.Claims;
using DoAn.Core.Application.DTOs.Base;
using DoAn.Core.Application.DTOs.Notification;
using DoAn.Core.Application.Interfaces.Notification;
using DoAn.Core.Domain.Entities;
using DoAn.Presentation.Api.Middlewares;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FootballField.API.Modules.NotificationManagement.Controllers
{
    [ApiController]
    [Route("api/notification")]
    [Authorize]
    public class NotificationsController : ControllerBase
    {
        private readonly INotificationService _notificationService;

        public NotificationsController(INotificationService notificationService)
        {
            _notificationService = notificationService;
        }

        /// <summary>
        /// Get notifications for the current user
        /// </summary>
        /// <param name="sinceId">Get notifications after this ID (for cursor pagination)</param>
        /// <param name="limit">Number of notifications to retrieve (max 200, default 50)</param>
        /// <returns>List of notifications with pagination info</returns>
        [HttpGet]
        [ProducesResponseType(typeof(ApiResponse<NotificationListDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> Get([FromQuery] int? sinceId = null, [FromQuery] int limit = 50)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out var userId))
                throw new UnauthorizedAccessException("Invalid user ID in token");

            var result = await _notificationService.GetNotificationsAsync(userId, sinceId, limit);
            return Ok(ApiResponse<NotificationListDto>.Ok(result, "Lấy danh sách thông báo thành công"));
        }

        /// <summary>
        /// Get unread notification count
        /// </summary>
        [HttpGet("unread-count")]
        [ProducesResponseType(typeof(ApiResponse<UnreadCountDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GetUnreadCount()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out var userId))
                throw new UnauthorizedAccessException("Invalid user ID in token");

            var count = await _notificationService.GetUnreadCountAsync(userId);
            return Ok(ApiResponse<UnreadCountDto>.Ok(
                new UnreadCountDto { Count = count }, 
                "Lấy số lượng thông báo chưa đọc thành công"));
        }

        /// <summary>
        /// Mark a single notification as read
        /// </summary>
        [HttpPost("mark-read")]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> MarkRead([FromBody] MarkReadDto dto)
        {
            if (dto.NotificationId <= 0)
                throw new ArgumentException("Invalid notification ID");

            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out var userId))
                throw new UnauthorizedAccessException("Invalid user ID in token");

            var success = await _notificationService.MarkAsReadAsync(userId, dto.NotificationId);
            if (!success)
                throw new KeyNotFoundException("Notification not found");

            return Ok(ApiResponse<string>.Ok(null, "Đánh dấu đã đọc thành công"));
        }

        /// <summary>
        /// Mark multiple notifications as read
        /// </summary>
        [HttpPost("mark-multiple-read")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> MarkMultipleRead([FromBody] MarkMultipleReadDto dto)
        {
            if (dto.NotificationIds == null || dto.NotificationIds.Count == 0)
                throw new ArgumentException("Notification IDs list cannot be empty");

            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out var userId))
                throw new UnauthorizedAccessException("Invalid user ID in token");

            var count = await _notificationService.MarkMultipleAsReadAsync(userId, dto.NotificationIds);
            return Ok(ApiResponse<object>.Ok(
                new { markedCount = count }, 
                $"Đã đánh dấu {count} thông báo là đã đọc"));
        }

        /// <summary>
        /// Mark all notifications as read
        /// </summary>
        [HttpPost("mark-all-read")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        public async Task<IActionResult> MarkAllRead()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out var userId))
                throw new UnauthorizedAccessException("Invalid user ID in token");

            var count = await _notificationService.MarkAllAsReadAsync(userId);
            return Ok(ApiResponse<object>.Ok(
                new { markedCount = count }, 
                $"Đã đánh dấu tất cả {count} thông báo là đã đọc"));
        }

        /// <summary>
        /// Delete a notification
        /// </summary>
        [HttpDelete("{id}")]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Delete(int id)
        {
            if (id <= 0)
                throw new ArgumentException("Invalid notification ID");

            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out var userId))
                throw new UnauthorizedAccessException("Invalid user ID in token");

            var success = await _notificationService.DeleteNotificationAsync(userId, id);
            if (!success)
                throw new KeyNotFoundException("Notification not found");

            return Ok(ApiResponse<string>.Ok(null, "Xóa thông báo thành công"));
        }

#if DEBUG
        /// <summary>
        /// [TEST ONLY] Send a test notification to any user
        /// </summary>
        [HttpPost("test/send")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        public async Task<IActionResult> SendTestNotification([FromBody] TestNotificationDto dto)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (!int.TryParse(userIdClaim?.Value, out var currentUserId))
                throw new UnauthorizedAccessException("Invalid token");

            var notification = new Notification
            {
                UserId = dto.TargetUserId,
                SenderId = currentUserId,
                Title = dto.Title ?? "Test Notification",
                Message = dto.Message ?? "This is a test notification from API",
                Type = dto.Type,
                RelatedTable = dto.RelatedTable,
                RelatedId = dto.RelatedId
            };

            var result = await _notificationService.CreateAndPushAsync(notification);
            
            return Ok(ApiResponse<object>.Ok(
                new { 
                    notificationId = result.Id,
                    targetUserId = result.UserId,
                    title = result.Title,
                    message = result.Message,
                    type = result.Type.ToString()
                }, 
                "Test notification sent successfully"));
        }

        /// <summary>
        /// [TEST ONLY] Send a test notification to yourself
        /// </summary>
        [HttpPost("test/send-to-me")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        public async Task<IActionResult> SendTestNotificationToSelf([FromBody] SimpleTestDto? dto = null)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (!int.TryParse(userIdClaim?.Value, out var currentUserId))
                throw new UnauthorizedAccessException("Invalid token");

            var notification = new Notification
            {
                UserId = currentUserId,
                SenderId = currentUserId, // Tự gửi cho chính mình (để test sender name)
                Title = dto?.Title ?? "Thông Báo Test",
                Message = dto?.Message ?? $"Đây là thông báo test lúc {DateTime.Now:HH:mm:ss}",
                Type = NotificationType.System,
                RelatedTable = null,
                RelatedId = null
            };

            var result = await _notificationService.CreateAndPushAsync(notification);
            
            return Ok(ApiResponse<object>.Ok(
                new { 
                    notificationId = result.Id,
                    userId = result.UserId,
                    title = result.Title,
                    message = result.Message
                }, 
                "Test notification sent to yourself"));
        }

        public class TestNotificationDto
        {
            public int TargetUserId { get; set; }
            public string? Title { get; set; }
            public string? Message { get; set; }
            public NotificationType Type { get; set; } = NotificationType.System;
            public string? RelatedTable { get; set; }
            public int? RelatedId { get; set; }
        }

        public class SimpleTestDto
        {
            public string? Title { get; set; }
            public string? Message { get; set; }
        }
#endif
    }
}
