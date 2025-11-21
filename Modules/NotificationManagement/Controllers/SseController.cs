using System.Security.Claims;
using System.Text.Json;
using System.Threading.Channels;
using FootballField.API.Modules.NotificationManagement.Repositories;
using FootballField.API.Modules.NotificationManagement.Services;
using FootballField.API.Shared.Utils;
using Microsoft.AspNetCore.Authorization; // Cần có using này
using Microsoft.AspNetCore.Mvc;

namespace FootballField.API.Modules.NotificationManagement.Controllers
{
    [ApiController]
    [Route("sse")]
    
    public class SseController : ControllerBase
    {
        private readonly ISseRepository _sseRepo;
        private readonly INotificationService _notificationService;

        // Lưu ý: Bỏ JwtHelper khỏi constructor
        public SseController(ISseRepository sseRepo, INotificationService notificationService)
        {
            _sseRepo = sseRepo;
            _notificationService = notificationService;
        }

        [Authorize] // 🔥 Bắt buộc phải có token hợp lệ trong Header
        [HttpGet("stream")]
        public async Task Stream(CancellationToken cancellationToken)
        {
            // Nếu code chạy đến đây, HttpContext.User đã được JWT Middleware xác thực thành công.
            var principal = HttpContext.User;

            var idClaim = principal.FindFirst(ClaimTypes.NameIdentifier) ?? principal.FindFirst("sub");
            if (idClaim == null || !int.TryParse(idClaim.Value, out var userId))
            {
                // Trường hợp hiếm gặp: Token hợp lệ nhưng thiếu Claim ID
                Response.StatusCode = 401;
                await Response.WriteAsync("Unauthorized - invalid id in token");
                return;
            }

            Response.Headers.Add("Content-Type", "text/event-stream");
            Response.Headers.Add("Cache-Control", "no-cache");
            Response.Headers.Add("Connection", "keep-alive");

            var channel = Channel.CreateUnbounded<string>(new UnboundedChannelOptions
            {
                SingleReader = true,
                SingleWriter = false
            });

            _sseRepo.AddConnection(userId, channel);

            // Replay if Last-Event-ID exists
            var lastEventIdHeader = Request.Headers["Last-Event-ID"].FirstOrDefault();
            if (!string.IsNullOrEmpty(lastEventIdHeader) && int.TryParse(lastEventIdHeader, out var lastEventId))
            {
                var missed = await _notificationService.GetNotificationsAsync(userId, sinceId: lastEventId, limit: 200);
                foreach (var n in missed.OrderBy(n => n.Id))
                {
                    var payloadObj = new
                    {
                        id = n.Id,
                        userId = n.UserId,
                        senderId = n.SenderId,
                        title = n.Title,
                        message = n.Message,
                        type = (byte)n.Type,
                        relatedTable = n.RelatedTable,
                        relatedId = n.RelatedId,
                        createdAt = n.CreatedAt
                    };
                    var json = JsonSerializer.Serialize(payloadObj, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });
                    await WriteSseAsync(Response, json, cancellationToken);
                }
            }

            // send connected ping
            var connPayload = JsonSerializer.Serialize(new { msg = "connected", at = TimeZoneHelper.VietnamNow });
            await WriteSseAsync(Response, connPayload, cancellationToken);

            try
            {
                await foreach (var message in channel.Reader.ReadAllAsync(cancellationToken))
                {
                    await WriteSseAsync(Response, message, cancellationToken);
                }
            }
            catch (OperationCanceledException)
            {
                // client disconnected
            }
            finally
            {
                _sseRepo.RemoveConnection(userId, channel);
                try { channel.Writer.TryComplete(); } catch { }
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
    }
}