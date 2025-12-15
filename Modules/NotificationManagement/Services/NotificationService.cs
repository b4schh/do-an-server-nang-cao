using System.Text.Json;
using FootballField.API.Modules.NotificationManagement.Repositories;
using FootballField.API.Modules.NotificationManagement.Entities;
using FootballField.API.Modules.NotificationManagement.Dtos;

namespace FootballField.API.Modules.NotificationManagement.Services
{
    public class NotificationService : INotificationService
    {
        private readonly INotificationRepository _notificationRepository;
        private readonly ISseRepository _sseRepo;
        private readonly ILogger<NotificationService> _logger;

        public NotificationService(
            INotificationRepository notificationRepository,
            ISseRepository sseRepo,
            ILogger<NotificationService> logger)
        {
            _notificationRepository = notificationRepository;
            _sseRepo = sseRepo;
            _logger = logger;
        }

        public async Task<Notification> CreateAndPushAsync(Notification notification)
        {
            try
            {
                var result = await _notificationRepository.CreateAsync(notification);

                var payloadObj = new
                {
                    id = result.Id,
                    userId = result.UserId,
                    senderId = result.SenderId,
                    title = result.Title,
                    message = result.Message,
                    type = (byte)result.Type,
                    typeName = result.Type.ToString(),
                    relatedTable = result.RelatedTable,
                    relatedId = result.RelatedId,
                    createdAt = result.CreatedAt
                };

                var json = JsonSerializer.Serialize(payloadObj, new JsonSerializerOptions 
                { 
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase 
                });

                // Push best-effort to SSE after DB success
                _sseRepo.PushToUser(result.UserId, json);
                
                _logger.LogInformation(
                    "Notification {NotificationId} created and pushed to user {UserId}", 
                    result.Id, 
                    result.UserId);

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error creating notification for user {UserId}", notification.UserId);
                throw;
            }
        }

        public async Task<NotificationListDto> GetNotificationsAsync(int userId, int? sinceId = null, int limit = 50)
        {
            try
            {
                // Validate and cap limit
                if (limit <= 0 || limit > 200)
                    limit = 50;

                var notifications = await _notificationRepository.GetByUserIdAsync(userId, sinceId, limit);

                var hasMore = notifications.Count > limit;
                if (hasMore)
                    notifications = notifications.Take(limit).ToList();

                var unreadCount = await _notificationRepository.GetUnreadCountAsync(userId);

                var notificationDtos = notifications.Select(n => new NotificationDto
                {
                    Id = n.Id,
                    UserId = n.UserId,
                    SenderId = n.SenderId,
                    SenderName = n.Sender != null ? $"{n.Sender.FirstName} {n.Sender.LastName}" : null,
                    Title = n.Title,
                    Message = n.Message,
                    Type = (byte)n.Type,
                    TypeName = n.Type.ToString(),
                    RelatedTable = n.RelatedTable,
                    RelatedId = n.RelatedId,
                    IsRead = n.IsRead,
                    CreatedAt = n.CreatedAt,
                    ReadAt = n.ReadAt
                }).ToList();

                return new NotificationListDto
                {
                    Notifications = notificationDtos,
                    UnreadCount = unreadCount,
                    HasMore = hasMore,
                    LastId = notificationDtos.LastOrDefault()?.Id
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting notifications for user {UserId}", userId);
                throw;
            }
        }

        public async Task<bool> MarkAsReadAsync(int userId, int notificationId)
        {
            try
            {
                var success = await _notificationRepository.MarkAsReadAsync(userId, notificationId);
                
                if (success)
                {
                    _logger.LogInformation(
                        "Notification {NotificationId} marked as read by user {UserId}", 
                        notificationId, 
                        userId);
                }
                else
                {
                    _logger.LogWarning(
                        "Notification {NotificationId} not found for user {UserId}", 
                        notificationId, 
                        userId);
                }

                return success;
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex, 
                    "Error marking notification {NotificationId} as read for user {UserId}", 
                    notificationId, 
                    userId);
                throw;
            }
        }

        public async Task<int> MarkMultipleAsReadAsync(int userId, List<int> notificationIds)
        {
            try
            {
                var count = await _notificationRepository.MarkMultipleAsReadAsync(userId, notificationIds);
                
                _logger.LogInformation(
                    "Marked {Count} notifications as read for user {UserId}", 
                    count, 
                    userId);

                return count;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error marking multiple notifications as read for user {UserId}", userId);
                throw;
            }
        }

        public async Task<int> MarkAllAsReadAsync(int userId)
        {
            try
            {
                var count = await _notificationRepository.MarkAllAsReadAsync(userId);
                
                _logger.LogInformation(
                    "Marked all {Count} notifications as read for user {UserId}", 
                    count, 
                    userId);

                return count;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error marking all notifications as read for user {UserId}", userId);
                throw;
            }
        }

        public async Task<int> GetUnreadCountAsync(int userId)
        {
            try
            {
                return await _notificationRepository.GetUnreadCountAsync(userId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting unread count for user {UserId}", userId);
                throw;
            }
        }

        public async Task<bool> DeleteNotificationAsync(int userId, int notificationId)
        {
            try
            {
                var success = await _notificationRepository.DeleteAsync(userId, notificationId);
                
                if (success)
                {
                    _logger.LogInformation(
                        "Notification {NotificationId} deleted by user {UserId}", 
                        notificationId, 
                        userId);
                }
                else
                {
                    _logger.LogWarning(
                        "Notification {NotificationId} not found for deletion by user {UserId}", 
                        notificationId, 
                        userId);
                }

                return success;
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex, 
                    "Error deleting notification {NotificationId} for user {UserId}", 
                    notificationId, 
                    userId);
                throw;
            }
        }
    }
}
