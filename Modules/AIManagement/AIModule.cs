using System;
using System.IO;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;
// Dùng cho Google connector v1.0+
// Đảm bảo bạn đã cài đặt NuGet package Microsoft.SemanticKernel.Connectors.Google
using Microsoft.SemanticKernel.Connectors.Google; 
using FootballField.API.Modules.AIManagement.Plugins;
using FootballField.API.Modules.AIManagement.Services;

// Adjust names for your IUserService/IBookingService/IStorageService namespaces
using FootballField.API.Modules.UserManagement.Services;
using FootballField.API.Modules.BookingManagement.Services;
using FootballField.API.Shared.Storage;

namespace FootballField.API.Modules.AIManagement
{
    /// <summary>
    /// Extension method để đăng ký các dịch vụ liên quan đến AI (Semantic Kernel).
    /// KHÔNG resolve scoped services từ trong singleton factory.
    /// </summary>
    public static class AIModule
    {
        public static IServiceCollection AddAIModule(this IServiceCollection services, IConfiguration configuration)
        {
            // 1) ChatHistoryService - scoped hoặc singleton tùy bạn (dùng scoped nếu phụ thuộc user)
            var promptPath = configuration["AI:SystemPromptFilePath"] 
                             ?? Path.Combine("Modules", "AIManagement", "Config", "ai.prompt.txt");

            // ChatHistoryService giữ lịch sử theo user - nếu nó lưu theo user/request -> scoped
            services.AddScoped(sp => new ChatHistoryService(
    maxMessagesPerUser: configuration.GetValue<int?>("AI:MaxHistoryPerUser") ?? 50
));

            // 2) Kernel registration (Semantic Kernel) - singleton OK (heavy object)
            services.AddSingleton<Kernel>(sp =>
            {
                var logger = sp.GetRequiredService<ILogger<Kernel>>();
                var apiKey = configuration["GeminiSettings:ApiKey"];
                var modelId = configuration["GeminiSettings:ModelId"] ?? "gemini-2.5-flash";

                if (string.IsNullOrWhiteSpace(apiKey))
                {
                    logger.LogWarning("Gemini API Key not set (GeminiSettings:ApiKey)");
                    throw new InvalidOperationException("Gemini API Key not configured.");
                }

                var builder = Kernel.CreateBuilder();

                // Add Gemini connector (SK extension)
                builder.AddGoogleAIGeminiChatCompletion(modelId, apiKey);

                var kernel = builder.Build();

                logger.LogInformation("Semantic Kernel initialized with model {ModelId}", modelId);

                // IMPORTANT: Do NOT try to import plugins which depend on scoped services here.
                // Plugins that require scoped services (IUserService, IBookingService, ...) will be created per-scope when needed.

                return kernel;
            });

            // 3) Register plugin types as scoped/transient (do NOT import them into kernel at startup)
            // UserInfoPlugin needs IUserService/IBookingService which are scoped - register plugin as scoped
            services.AddScoped<UserInfoPlugin>();
services.AddScoped<BookingPlugin>();
services.AddScoped<ComplexPlugin>();

            // 4) AI service (scoped) - implementation should create scope when invoking plugins that require scoped services
            services.AddScoped<IAIService, AIService>();

            // 5) If you have an invoker that must remain singleton but needs scoped services occasionally,
            // inject IServiceScopeFactory into that invoker and CreateScope() when required.

            return services;
        }
    }
}
