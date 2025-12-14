using FootballField.API.Modules.AIManagement.Dtos;
using System.Threading.Tasks;

namespace FootballField.API.Modules.AIManagement.Services
{
  public interface IAIService
{
  Task<ChatResponse> SendMessageAsync(string userMessage, CancellationToken cancellationToken = default);
    Task ClearChatHistoryAsync();

}
}
