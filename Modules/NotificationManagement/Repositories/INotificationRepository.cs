using FootballField.API.Modules.NotificationManagement.Entities;

namespace FootballField.API.Modules.NotificationManagement.Repositories;

public interface INotificationRepository
{
    Task<Notification> CreateAsync(Notification notification);
    Task<List<Notification>> GetByUserIdAsync(int userId, int? sinceId = null, int limit = 50);
    Task<Notification?> GetByIdAsync(int id, int userId);
    Task<int> GetUnreadCountAsync(int userId);
    Task<bool> MarkAsReadAsync(int userId, int notificationId);
    Task<int> MarkMultipleAsReadAsync(int userId, List<int> notificationIds);
    Task<int> MarkAllAsReadAsync(int userId);
    Task<bool> DeleteAsync(int userId, int notificationId);
}
