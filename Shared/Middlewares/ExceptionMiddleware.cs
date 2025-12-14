using System.Net;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
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
            var (statusCode, message, shouldLog) = ex switch
            {
                // 400 - Bad Request (specific exceptions first)
                ArgumentNullException argNullEx => (
                    (int)HttpStatusCode.BadRequest,
                    argNullEx.Message,
                    false
                ),
                ArgumentException argEx => (
                    (int)HttpStatusCode.BadRequest,
                    argEx.Message,
                    false // Không cần log, là lỗi từ user input
                ),

                // 401 - Unauthorized
                UnauthorizedAccessException unauthorizedEx => (
                    (int)HttpStatusCode.Forbidden, // 403 cho UnauthorizedAccessException
                    unauthorizedEx.Message,
                    true // Log vì có thể là tấn công
                ),

                // 404 - Not Found
                KeyNotFoundException notFoundEx => (
                    (int)HttpStatusCode.NotFound,
                    notFoundEx.Message,
                    false
                ),
                FileNotFoundException fileNotFoundEx => (
                    (int)HttpStatusCode.NotFound,
                    fileNotFoundEx.Message,
                    false
                ),

                // 409 - Conflict
                InvalidDataException invalidDataEx => (
                    (int)HttpStatusCode.Conflict,
                    invalidDataEx.Message,
                    false
                ),

                // 500 - Internal Server Error
                DbUpdateException dbEx => (
                    (int)HttpStatusCode.InternalServerError,
                    "Lỗi khi thao tác với cơ sở dữ liệu",
                    true // Luôn log database errors
                ),
                TimeoutException timeoutEx => (
                    (int)HttpStatusCode.RequestTimeout,
                    "Yêu cầu đã quá thời gian chờ",
                    true
                ),
                TaskCanceledException => (
                    (int)HttpStatusCode.RequestTimeout,
                    "Yêu cầu đã bị hủy",
                    false
                ),

                // Default - 500
                _ => (
                    (int)HttpStatusCode.InternalServerError,
                    "Lỗi không xác định xảy ra trán máy chủ",
                    true // Luôn log unknown errors
                )
            };

            // Log error nếu cần
            if (shouldLog)
            {
                _logger.LogError(ex, 
                    "❌ Exception caught by middleware | Type: {ExceptionType} | StatusCode: {StatusCode} | Path: {Path}",
                    ex.GetType().Name,
                    statusCode,
                    context.Request.Path);
            }
            else
            {
                _logger.LogWarning(
                    "⚠️ Validation/Business error | Type: {ExceptionType} | Message: {Message} | Path: {Path}",
                    ex.GetType().Name,
                    ex.Message,
                    context.Request.Path);
            }

            // Tạo response
            var response = ApiResponse<string>.Fail(
                message: message,
                statusCode: statusCode,
                errors: _env.IsDevelopment() && shouldLog
                    ? new[] { ex.Message, ex.StackTrace ?? "" }
                    : null // Ẩn stacktrace trong production
            );

            context.Response.ContentType = "application/json";
            context.Response.StatusCode = statusCode;

            var options = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                WriteIndented = _env.IsDevelopment()
            };

            await context.Response.WriteAsync(JsonSerializer.Serialize(response, options));
        }
    }
}
