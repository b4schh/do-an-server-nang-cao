using System.Text.Json;
using FootballField.API.Database;
using Microsoft.EntityFrameworkCore;
using FootballField.API.Shared.Utils;
using FootballField.API.Modules.NotificationManagement.Repositories;
using FootballField.API.Modules.NotificationManagement.Entities;

namespace FootballField.API.Modules.NotificationManagement.Services
{
    public class NotificationService : INotificationService
    {
        private readonly ApplicationDbContext _db;
        private readonly ISseRepository _sseRepo;

        public NotificationService(ApplicationDbContext db, ISseRepository sseRepo)
        {
            _db = db;
            _sseRepo = sseRepo;
        }

        public async Task<Notification> CreateAndPushAsync(Notification notification)
        {
            // Use Vietnam time or UTC consistent with your DB; here using Vietnam time helper if present
            try
            {
                notification.CreatedAt = TimeZoneHelper.VietnamNow;
            }
            catch
            {
                notification.CreatedAt = DateTime.UtcNow;
            }

            _db.Notifications.Add(notification);
            await _db.SaveChangesAsync();

            var payloadObj = new
            {
                id = notification.Id,
                userId = notification.UserId,
                senderId = notification.SenderId,
                title = notification.Title,
                message = notification.Message,
                type = (byte)notification.Type,
                relatedTable = notification.RelatedTable,
                relatedId = notification.RelatedId,
                createdAt = notification.CreatedAt
            };

            var json = JsonSerializer.Serialize(payloadObj, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });

            // push best-effort
            _sseRepo.PushToUser(notification.UserId, json);

            return notification;
        }

        public async Task<List<Notification>> GetNotificationsAsync(int userId, int? sinceId = null, int limit = 50)
        {
            var q = _db.Notifications.AsNoTracking().Where(n => n.UserId == userId);
            if (sinceId.HasValue) q = q.Where(n => n.Id > sinceId.Value);
            return await q.OrderByDescending(n => n.Id).Take(limit).ToListAsync();
        }

        public async Task MarkAsReadAsync(int userId, int notificationId)
        {
            var notif = await _db.Notifications.FirstOrDefaultAsync(n => n.Id == notificationId && n.UserId == userId);
            if (notif == null) return;
            if (!notif.IsRead)
            {
                notif.IsRead = true;
                notif.ReadAt = TimeZoneHelper.VietnamNow;
                await _db.SaveChangesAsync();
            }
        }
    }
}
