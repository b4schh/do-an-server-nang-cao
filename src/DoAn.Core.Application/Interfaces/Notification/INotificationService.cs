using DoAn.Core.Application.DTOs.Notification;

namespace DoAn.Core.Application.Interfaces.Notification;

public interface INotificationService
{
    /// <summary>
    /// Creates a notification and pushes it to user via SSE
    /// </summary>
    Task<NotificationEntity> CreateAndPushAsync(NotificationEntity notification);

    /// <summary>
    /// Gets notifications for a user with pagination
    /// </summary>
    Task<NotificationListDto> GetNotificationsAsync(int userId, int? sinceId = null, int limit = 50);

    /// <summary>
    /// Marks a single notification as read
    /// </summary>
    Task<bool> MarkAsReadAsync(int userId, int notificationId);

    /// <summary>
    /// Marks multiple notifications as read
    /// </summary>
    Task<int> MarkMultipleAsReadAsync(int userId, List<int> notificationIds);

    /// <summary>
    /// Marks all notifications as read for a user
    /// </summary>
    Task<int> MarkAllAsReadAsync(int userId);

    /// <summary>
    /// Gets unread notification count for a user
    /// </summary>
    Task<int> GetUnreadCountAsync(int userId);

    /// <summary>
    /// Deletes a notification (soft delete by marking as read)
    /// </summary>
    Task<bool> DeleteNotificationAsync(int userId, int notificationId);
}
