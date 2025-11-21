using System.Net;
using System.Text.Json;
using FootballField.API.Shared.Dtos;

namespace FootballField.API.Shared.Middlewares
{
    public class ExceptionMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ExceptionMiddleware> _logger;
        private readonly IWebHostEnvironment _env;

        public ExceptionMiddleware(RequestDelegate next, ILogger<ExceptionMiddleware> logger, IWebHostEnvironment env)
        {
            _next = next;
            _logger = logger;
            _env = env;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context); // tiếp tục middleware pipeline
            }
            catch (Exception ex)
            {
                await HandleExceptionAsync(context, ex);
            }
        }

        private async Task HandleExceptionAsync(HttpContext context, Exception ex)
        {
            // Log lỗi ra console
            _logger.LogError(ex, "❌ Middleware đã bắt được một lỗi chưa được xử lý.");

            var statusCode = (int)HttpStatusCode.InternalServerError;
            var response = ApiResponse<string>.Fail(
                message: "Đã có lỗi xảy ra trên máy chủ",
                statusCode: statusCode,
                errors: _env.IsDevelopment()
                    ? new[] { ex.Message, ex.StackTrace ?? "" }
                    : null // ẩn stacktrace trong môi trường production
            );

            context.Response.ContentType = "application/json";
            context.Response.StatusCode = statusCode;

            var options = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                WriteIndented = false
            };

            await context.Response.WriteAsync(JsonSerializer.Serialize(response, options));
        }
    }
}
