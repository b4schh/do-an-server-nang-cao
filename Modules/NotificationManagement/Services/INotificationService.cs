
using FootballField.API.Modules.NotificationManagement.Entities;

namespace FootballField.API.Modules.NotificationManagement.Services
{
    public interface INotificationService
    {
        Task<Notification> CreateAndPushAsync(Notification notification);
        Task<List<Notification>> GetNotificationsAsync(int userId, int? sinceId = null, int limit = 50);
        Task MarkAsReadAsync(int userId, int notificationId);
    }
}
