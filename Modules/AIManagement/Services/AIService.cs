// File: Modules/AIManagement/Services/AIService.cs
using System;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Http;
using FootballField.API.Modules.AIManagement.Dtos;
using FootballField.API.Modules.AIManagement.Plugins;

namespace FootballField.API.Modules.AIManagement.Services
{
    public class AIService : IAIService
    {
        private readonly Kernel _rootKernel;
        private readonly ChatHistoryService _chatHistory;
        private readonly ILogger<AIService> _logger;
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public AIService(
            Kernel rootKernel,
            ChatHistoryService chatHistory,
            ILogger<AIService> logger,
            IServiceScopeFactory scopeFactory,
            IHttpContextAccessor httpContextAccessor)
        {
            _rootKernel = rootKernel;
            _chatHistory = chatHistory;
            _logger = logger;
            _scopeFactory = scopeFactory;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<ChatResponse> SendMessageAsync(
            string userMessage,
            CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(userMessage))
                throw new ArgumentException("userMessage empty", nameof(userMessage));

            var sw = System.Diagnostics.Stopwatch.StartNew();
            int? userId = GetCurrentUserIdFromContext();

            try
            {
                // Clone kernel cho mỗi request
                var kernel = _rootKernel.Clone();

                // Import plugin theo scope
                using var scope = _scopeFactory.CreateScope();

                kernel.ImportPluginFromObject(
                    scope.ServiceProvider.GetRequiredService<UserInfoPlugin>(),
                    "User");

                kernel.ImportPluginFromObject(
                    scope.ServiceProvider.GetRequiredService<BookingPlugin>(),
                    "Booking");

                kernel.ImportPluginFromObject(
                    scope.ServiceProvider.GetRequiredService<ComplexPlugin>(),
                    "Complex");

                var conversationPrompt = userId.HasValue
                    ? _chatHistory.GetConversationPrompt(userId.Value, userMessage, 20)
                    : _chatHistory.GetConversationPrompt(0, userMessage, 20);

                var finalPrompt = $"{BuildSystemPrompt()}\n\n{conversationPrompt}";

                var raw = await kernel.InvokePromptAsync<string>(
                    finalPrompt,
                    cancellationToken: cancellationToken);

                if (string.IsNullOrWhiteSpace(raw))
                    raw = "Hiện tại chưa có dữ liệu phù hợp.";

                var processed = PostProcessModelOutput(raw, userId);
                var cleaned = SanitizeAiResponse(processed);

                if (userId.HasValue)
                {
                    _chatHistory.AddMessage(userId.Value, "User", userMessage);
                    _chatHistory.AddMessage(userId.Value, "Assistant", cleaned);
                }

                sw.Stop();

                return new ChatResponse
                {
                    Text = cleaned,
                    Timestamp = DateTime.UtcNow
                };
            }
            catch (OperationCanceledException)
            {
                return new ChatResponse
                {
                    Text = "Yêu cầu đã bị hủy.",
                    Timestamp = DateTime.UtcNow
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "AIService error for user {UserId}", userId);
                return new ChatResponse
                {
                    Text = "Xin lỗi, đã xảy ra lỗi khi xử lý yêu cầu.",
                    Timestamp = DateTime.UtcNow
                };
            }
        }

        public async Task ClearChatHistoryAsync()
        {
            var userId = GetCurrentUserIdFromContext();
            if (userId.HasValue)
                await Task.Run(() => _chatHistory.ClearHistory(userId.Value));
        }

        private int? GetCurrentUserIdFromContext()
        {
            try
            {
                var user = _httpContextAccessor.HttpContext?.User;
                if (user?.Identity?.IsAuthenticated != true) return null;

                var claim =
                    user.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier) ??
                    user.FindFirst("id") ??
                    user.FindFirst("sub");

                return int.TryParse(claim?.Value, out var id) ? id : null;
            }
            catch
            {
                return null;
            }
        }

        #region Post-process helpers

        private string PostProcessModelOutput(string raw, int? userId)
        {
            if (string.IsNullOrWhiteSpace(raw)) return raw;

            var s = raw;

            s = Regex.Replace(s, @"\*\*\[.*?\]\*\*", "", RegexOptions.IgnoreCase);
            s = Regex.Replace(s, @"```[\s\S]*?```", "", RegexOptions.Multiline);
            s = Regex.Replace(s, @"\n{3,}", "\n\n").Trim();

            var json = ExtractFirstJsonObject(s);
            if (!string.IsNullOrEmpty(json))
            {
                try
                {
                    var bullets = ConvertJsonToBullets(json);
                    if (!string.IsNullOrWhiteSpace(bullets))
                        return "Dưới đây là thông tin bạn cần:\n" + bullets;
                }
                catch { }
            }

            if (!Regex.IsMatch(s, @"(?i)^(chào|xin chào|dưới đây)"))
                s = "Chào bạn, " + s;

            return s.Trim();
        }

        private static string ExtractFirstJsonObject(string text)
        {
            int depth = 0, start = -1;
            for (int i = 0; i < text.Length; i++)
            {
                if (text[i] == '{')
                {
                    if (depth == 0) start = i;
                    depth++;
                }
                else if (text[i] == '}')
                {
                    depth--;
                    if (depth == 0 && start >= 0)
                    {
                        var json = text.Substring(start, i - start + 1);
                        try
                        {
                            JsonDocument.Parse(json);
                            return json;
                        }
                        catch { start = -1; }
                    }
                }
            }
            return string.Empty;
        }

        private string ConvertJsonToBullets(string json)
        {
            using var doc = JsonDocument.Parse(json);
            if (doc.RootElement.ValueKind != JsonValueKind.Object) return "";

            var sb = new StringBuilder();
            foreach (var prop in doc.RootElement.EnumerateObject())
            {
                if (prop.NameEquals("id") || prop.NameEquals("userId")) continue;
                sb.AppendLine($"- {ToVietnameseLabel(prop.Name)}: {prop.Value}");
            }
            return sb.ToString().TrimEnd();
        }

        private static string ToVietnameseLabel(string key)
        {
            return key.ToLower() switch
            {
                "name" or "username" => "Tên",
                "email" => "Email",
                "phone" or "phonenumber" => "Số điện thoại",
                "role" => "Vai trò",
                "status" => "Trạng thái",
                _ => ToTitleCaseWithSpaces(key)
            };
        }

        private static string ToTitleCaseWithSpaces(string input)
        {
            var s = Regex.Replace(input, "([a-z])([A-Z])", "$1 $2");
            return char.ToUpper(s[0]) + s.Substring(1);
        }

        private static string SanitizeAiResponse(string text)
        {
            if (string.IsNullOrWhiteSpace(text)) return text;
            text = Regex.Replace(text, @"\n{2,}", "\n\n").Trim();
            if (!Regex.IsMatch(text, @"[.!?]$")) text += ".";
            return text;
        }

        #endregion

   private string BuildSystemPrompt()
{
    try
    {
        var path = Path.Combine(
            AppContext.BaseDirectory,
            "AI",
            "systemPrompt.txt");

        if (File.Exists(path))
            return File.ReadAllText(path);
    }
    catch
    {
        // ignore
    }

    // Fallback tối thiểu – KHÔNG LOGIC
    return """
Bạn là trợ lý AI của hệ thống SanBong.
Trả lời người dùng bằng dữ liệu từ hệ thống.
""";
}

    }
}
