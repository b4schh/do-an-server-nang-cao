using System.Security.Claims;
using FootballField.API.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FootballField.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class NotificationsController : ControllerBase
    {
        private readonly INotificationService _notificationService;
        public NotificationsController(INotificationService notificationService)
        {
            _notificationService = notificationService;
        }

        [HttpGet]
        public async Task<IActionResult> Get([FromQuery] int? sinceId = null, [FromQuery] int limit = 50)
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);
            var list = await _notificationService.GetNotificationsAsync(userId, sinceId, limit);
            return Ok(list);
        }

        [HttpPost("mark-read")]
        public async Task<IActionResult> MarkRead([FromBody] MarkReadDto dto)
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);
            await _notificationService.MarkAsReadAsync(userId, dto.NotificationId);
            return NoContent();
        }

        public class MarkReadDto
        {
            public int NotificationId { get; set; }
        }
    }
}