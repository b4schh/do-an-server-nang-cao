using System.Security.Claims;
using System.Text.Json;
using System.Threading.Channels;
using FootballField.API.Modules.NotificationManagement.Repositories;
using FootballField.API.Modules.NotificationManagement.Services;
using FootballField.API.Shared.Dtos;
using FootballField.API.Shared.Utils;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FootballField.API.Modules.NotificationManagement.Controllers
{
    [ApiController]
    [Route("api/sse")]
    [Authorize]
    public class SseController : ControllerBase
    {
        private readonly ISseRepository _sseRepo;
        private readonly INotificationService _notificationService;
        private readonly ILogger<SseController> _logger;

        public SseController(
            ISseRepository sseRepo, 
            INotificationService notificationService,
            ILogger<SseController> logger)
        {
            _sseRepo = sseRepo;
            _notificationService = notificationService;
            _logger = logger;
        }

        /// <summary>
        /// Establishes an SSE (Server-Sent Events) connection for real-time notifications.
        /// Supports automatic reconnection with Last-Event-ID header for missed notifications.
        /// </summary>
        [HttpGet("stream")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task Stream(CancellationToken cancellationToken)
        {
            // Extract and validate user ID from JWT token
            var principal = HttpContext.User;
            var idClaim = principal.FindFirst(ClaimTypes.NameIdentifier) ?? principal.FindFirst("sub");
            
            if (idClaim == null || !int.TryParse(idClaim.Value, out var userId))
            {
                _logger.LogWarning("SSE connection attempt with invalid user ID in token");
                Response.StatusCode = 401;
                await Response.WriteAsync("Unauthorized - invalid id in token");
                return;
            }

            _logger.LogInformation("SSE stream established for user {UserId}", userId);

            // Set SSE headers
            Response.Headers.Add("Content-Type", "text/event-stream");
            Response.Headers.Add("Cache-Control", "no-cache");
            Response.Headers.Add("Connection", "keep-alive");
            Response.Headers.Add("X-Accel-Buffering", "no"); // Disable nginx buffering

            var channel = Channel.CreateUnbounded<string>(new UnboundedChannelOptions
            {
                SingleReader = true,
                SingleWriter = false
            });

            _sseRepo.AddConnection(userId, channel);

            try
            {
                // Replay missed notifications if Last-Event-ID header exists
                var lastEventIdHeader = Request.Headers["Last-Event-ID"].FirstOrDefault();
                if (!string.IsNullOrEmpty(lastEventIdHeader) && int.TryParse(lastEventIdHeader, out var lastEventId))
                {
                    _logger.LogInformation("Replaying missed notifications for user {UserId} since ID {LastEventId}", userId, lastEventId);
                    
                    var missedResult = await _notificationService.GetNotificationsAsync(userId, sinceId: lastEventId, limit: 200);
                    var missed = missedResult.Notifications.OrderBy(n => n.Id).ToList();
                    
                    foreach (var n in missed)
                    {
                        var payloadObj = new
                        {
                            id = n.Id,
                            userId = n.UserId,
                            senderId = n.SenderId,
                            senderName = n.SenderName,
                            title = n.Title,
                            message = n.Message,
                            type = n.Type,
                            typeName = n.TypeName,
                            relatedTable = n.RelatedTable,
                            relatedId = n.RelatedId,
                            createdAt = n.CreatedAt
                        };
                        var json = JsonSerializer.Serialize(payloadObj, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });
                        await WriteSseAsync(Response, json, cancellationToken);
                    }
                    
                    _logger.LogInformation("Replayed {Count} missed notifications for user {UserId}", missed.Count, userId);
                }

                // Send connected ping
                var connPayload = JsonSerializer.Serialize(new 
                { 
                    msg = "connected", 
                    at = TimeZoneHelper.VietnamNow,
                    userId = userId
                }, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });
                await WriteSseAsync(Response, connPayload, cancellationToken);

                // Stream notifications as they arrive
                await foreach (var message in channel.Reader.ReadAllAsync(cancellationToken))
                {
                    await WriteSseAsync(Response, message, cancellationToken);
                }
            }
            catch (OperationCanceledException)
            {
                _logger.LogInformation("SSE connection cancelled for user {UserId}", userId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in SSE stream for user {UserId}", userId);
            }
            finally
            {
                _sseRepo.RemoveConnection(userId, channel);
                try { channel.Writer.TryComplete(); } catch { }
                _logger.LogInformation("SSE stream closed for user {UserId}", userId);
            }
        }

        private static async Task WriteSseAsync(HttpResponse response, string jsonPayload, CancellationToken ct)
        {
            // try get id from payload
            string? idLine = null;
            try
            {
                using var doc = JsonDocument.Parse(jsonPayload);
                if (doc.RootElement.TryGetProperty("id", out var idProp))
                {
                    if (idProp.ValueKind == JsonValueKind.Number && idProp.TryGetInt32(out var id))
                        idLine = $"id: {id}\n";
                    else if (idProp.ValueKind == JsonValueKind.String)
                        idLine = $"id: {idProp.GetString()}\n";
                }
            }
            catch { }

            var sse = string.Empty;
            if (!string.IsNullOrEmpty(idLine)) sse += idLine;
            sse += "event: notification\n";

            using var reader = new System.IO.StringReader(jsonPayload);
            string? line;
            while ((line = reader.ReadLine()) != null)
            {
                sse += $"data: {line}\n";
            }
            sse += "\n";

            await response.WriteAsync(sse, ct);
            await response.Body.FlushAsync(ct);
        }

        /// <summary>
        /// Get SSE connection statistics (admin/monitoring endpoint)
        /// </summary>
        [HttpGet("stats")]
        [Authorize(Roles = "Admin")] // Only admins can view connection stats
        [ProducesResponseType(typeof(ApiResponse<Dictionary<string, int>>), StatusCodes.Status200OK)]
        public IActionResult GetStats()
        {
            var stats = _sseRepo.GetConnectionStats();
            return Ok(ApiResponse<Dictionary<string, int>>.Ok(stats, "Lấy thống kê kết nối SSE thành công"));
        }
    }
}