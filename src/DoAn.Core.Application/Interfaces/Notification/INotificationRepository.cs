namespace DoAn.Core.Application.Interfaces.Notification;

public interface INotificationRepository
{
    Task<NotificationEntity> CreateAsync(NotificationEntity notification);
    Task<List<NotificationEntity>> GetByUserIdAsync(int userId, int? sinceId = null, int limit = 50);
    Task<NotificationEntity?> GetByIdAsync(int id, int userId);
    Task<int> GetUnreadCountAsync(int userId);
    Task<bool> MarkAsReadAsync(int userId, int notificationId);
    Task<int> MarkMultipleAsReadAsync(int userId, List<int> notificationIds);
    Task<int> MarkAllAsReadAsync(int userId);
    Task<bool> DeleteAsync(int userId, int notificationId);
}
