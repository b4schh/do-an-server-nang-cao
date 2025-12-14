using System;
using System.Diagnostics;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using FootballField.API.Modules.AIManagement.Dtos;
using FootballField.API.Modules.AIManagement.Services;

namespace FootballField.API.Modules.AIManagement.Controllers
{
    [ApiController]
    [Route("api/ai")]
    [Authorize]
    public class AIController : ControllerBase
    {
        private readonly IAIService _aiService;
        private readonly ILogger<AIController> _logger;

        public AIController(IAIService aiService, ILogger<AIController> logger)
        {
            _aiService = aiService;
            _logger = logger;
        }

        [HttpPost("chat")]
        public async Task<IActionResult> Chat([FromBody] ChatRequest request)
        {
            var sw = Stopwatch.StartNew();
            try
            {
                // Basic logging (no sensitive tokens)
                var authHeader = HttpContext.Request.Headers["Authorization"].ToString();
                _logger.LogInformation("AI.Chat called. Auth header present: {HasAuth}, StartsWithBearer: {HasBearer}",
                    !string.IsNullOrEmpty(authHeader),
                    !string.IsNullOrEmpty(authHeader) && authHeader.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase));

                // Ensure authenticated
                if (!(User?.Identity?.IsAuthenticated ?? false))
                {
                    _logger.LogWarning("AI.Chat - request not authenticated");
                    return Unauthorized("Không thể xác thực người dùng");
                }

                // Minimal useful claim logs (avoid printing everything)
                var nameId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                var email = User.FindFirst(ClaimTypes.Email)?.Value;
                _logger.LogInformation("AI.Chat - Authenticated userId: {UserId}, email: {Email}", Truncate(nameId, 60), Truncate(email, 120));

                if (!ModelState.IsValid)
                {
                    _logger.LogWarning("AI.Chat invalid model state: {Errors}", 
                        string.Join("; ", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage)));
                    return BadRequest("Invalid request");
                }

                // NOTE: do NOT pass userId from controller to AIService.
                // The plugin will read userId from HttpContext via IHttpContextAccessor inside plugin.
                var resp = await _aiService.SendMessageAsync(request.UserMessage);
                sw.Stop();
                _logger.LogInformation("AI.SendMessageAsync completed in {ElapsedMs}ms. Response length: {Len}", sw.ElapsedMilliseconds,
                    resp?.ToString()?.Length ?? 0);

                return Ok(resp);
            }
            catch (UnauthorizedAccessException ex)
            {
                sw.Stop();
                _logger.LogWarning(ex, "AI.Chat unauthorized. Elapsed {ElapsedMs}ms", sw.ElapsedMilliseconds);
                return Unauthorized("Không thể xác thực người dùng");
            }
            catch (Exception ex)
            {
                sw.Stop();
                _logger.LogError(ex, "AIController.Chat unexpected error. Elapsed {ElapsedMs}ms", sw.ElapsedMilliseconds);
                return StatusCode(500, "Đã xảy ra lỗi");
            }
        }

        [HttpPost("clear-history")]
        public IActionResult ClearHistory()
        {
            var sw = Stopwatch.StartNew();
            try
            {
                if (!(User?.Identity?.IsAuthenticated ?? false))
                {
                    _logger.LogWarning("AI.ClearHistory - request not authenticated");
                    return Unauthorized("Không thể xác thực người dùng");
                }

                var nameId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                _logger.LogInformation("Clear chat history for userId = {UserId}", Truncate(nameId, 60));

                // Call service without passing explicit userId; AIService/plugin will use HttpContext
                _aiService.ClearChatHistoryAsync();

                sw.Stop();
                _logger.LogInformation("AI.ClearHistory completed in {ElapsedMs}ms", sw.ElapsedMilliseconds);
                return Ok("Đã xóa lịch sử chat");
            }
            catch (UnauthorizedAccessException ex)
            {
                sw.Stop();
                _logger.LogWarning(ex, "AI.ClearHistory unauthorized. Elapsed {ElapsedMs}ms", sw.ElapsedMilliseconds);
                return Unauthorized("Không thể xác thực người dùng");
            }
            catch (Exception ex)
            {
                sw.Stop();
                _logger.LogError(ex, "AIController.ClearHistory unexpected error. Elapsed {ElapsedMs}ms", sw.ElapsedMilliseconds);
                return StatusCode(500, "Đã xảy ra lỗi");
            }
        }

        private static string Truncate(string? value, int maxLen)
        {
            if (string.IsNullOrEmpty(value)) return string.Empty;
            return value.Length <= maxLen ? value : value.Substring(0, maxLen) + "...";
        }
    }
}
