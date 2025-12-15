using FootballField.API.Database;
using FootballField.API.Modules.NotificationManagement.Entities;
using FootballField.API.Shared.Utils;
using Microsoft.EntityFrameworkCore;

namespace FootballField.API.Modules.NotificationManagement.Repositories;

public class NotificationRepository : INotificationRepository
{
    private readonly ApplicationDbContext _db;

    public NotificationRepository(ApplicationDbContext db)
    {
        _db = db;
    }

    public async Task<Notification> CreateAsync(Notification notification)
    {
        notification.CreatedAt = TimeZoneHelper.VietnamNow;
        _db.Notifications.Add(notification);
        await _db.SaveChangesAsync();
        return notification;
    }

    public async Task<List<Notification>> GetByUserIdAsync(int userId, int? sinceId = null, int limit = 50)
    {
        var query = _db.Notifications
            .AsNoTracking()
            .Include(n => n.Sender)
            .Where(n => n.UserId == userId);

        if (sinceId.HasValue)
            query = query.Where(n => n.Id > sinceId.Value);

        return await query
            .OrderByDescending(n => n.Id)
            .Take(limit + 1)
            .ToListAsync();
    }

    public async Task<Notification?> GetByIdAsync(int id, int userId)
    {
        return await _db.Notifications
            .FirstOrDefaultAsync(n => n.Id == id && n.UserId == userId);
    }

    public async Task<int> GetUnreadCountAsync(int userId)
    {
        return await _db.Notifications
            .Where(n => n.UserId == userId && !n.IsRead)
            .CountAsync();
    }

    public async Task<bool> MarkAsReadAsync(int userId, int notificationId)
    {
        var notification = await GetByIdAsync(notificationId, userId);
        if (notification == null)
            return false;

        if (!notification.IsRead)
        {
            notification.IsRead = true;
            notification.ReadAt = TimeZoneHelper.VietnamNow;
            await _db.SaveChangesAsync();
        }

        return true;
    }

    public async Task<int> MarkMultipleAsReadAsync(int userId, List<int> notificationIds)
    {
        if (notificationIds == null || notificationIds.Count == 0)
            return 0;

        var notifications = await _db.Notifications
            .Where(n => n.UserId == userId && notificationIds.Contains(n.Id) && !n.IsRead)
            .ToListAsync();

        if (notifications.Count == 0)
            return 0;

        var now = TimeZoneHelper.VietnamNow;
        foreach (var notification in notifications)
        {
            notification.IsRead = true;
            notification.ReadAt = now;
        }

        await _db.SaveChangesAsync();
        return notifications.Count;
    }

    public async Task<int> MarkAllAsReadAsync(int userId)
    {
        var unreadNotifications = await _db.Notifications
            .Where(n => n.UserId == userId && !n.IsRead)
            .ToListAsync();

        if (unreadNotifications.Count == 0)
            return 0;

        var now = TimeZoneHelper.VietnamNow;
        foreach (var notification in unreadNotifications)
        {
            notification.IsRead = true;
            notification.ReadAt = now;
        }

        await _db.SaveChangesAsync();
        return unreadNotifications.Count;
    }

    public async Task<bool> DeleteAsync(int userId, int notificationId)
    {
        var notification = await GetByIdAsync(notificationId, userId);
        if (notification == null)
            return false;

        _db.Notifications.Remove(notification);
        await _db.SaveChangesAsync();
        return true;
    }
}
