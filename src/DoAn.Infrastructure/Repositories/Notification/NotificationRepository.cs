using DoAn.Core.Application.Interfaces.Notification;
using DoAn.Core.Application.Common.Utils;
using DoAn.Infrastructure.Data;
using DoAn.Infrastructure.Repositories.Base;
using Microsoft.EntityFrameworkCore;

namespace DoAn.Infrastructure.Repositories.Notification;

public class NotificationRepository : GenericRepository<NotificationEntity>, INotificationRepository
{
    public NotificationRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<NotificationEntity> CreateAsync(NotificationEntity notification)
    {
        notification.CreatedAt = TimeZoneHelper.VietnamNow;
        return await AddAsync(notification);
    }

    public async Task<List<NotificationEntity>> GetByUserIdAsync(int userId, int? sinceId = null, int limit = 50)
    {
        var query = _dbSet
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

    public async Task<NotificationEntity?> GetByIdAsync(int id, int userId)
    {
        return await _dbSet
            .FirstOrDefaultAsync(n => n.Id == id && n.UserId == userId);
    }

    public async Task<int> GetUnreadCountAsync(int userId)
    {
        return await _dbSet
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
            await UpdateAsync(notification);
        }

        return true;
    }

    public async Task<int> MarkMultipleAsReadAsync(int userId, List<int> notificationIds)
    {
        if (notificationIds == null || notificationIds.Count == 0)
            return 0;

        // OPTIMIZED: Use ExecuteUpdateAsync instead of loading and updating
        var now = TimeZoneHelper.VietnamNow;
        var updatedCount = await _dbSet
            .Where(n => n.UserId == userId && notificationIds.Contains(n.Id) && !n.IsRead)
            .ExecuteUpdateAsync(setters => setters
                .SetProperty(n => n.IsRead, true)
                .SetProperty(n => n.ReadAt, now));

        return updatedCount;
    }

    public async Task<int> MarkAllAsReadAsync(int userId)
    {
        // OPTIMIZED: Use ExecuteUpdateAsync for bulk update
        var now = TimeZoneHelper.VietnamNow;
        var updatedCount = await _dbSet
            .Where(n => n.UserId == userId && !n.IsRead)
            .ExecuteUpdateAsync(setters => setters
                .SetProperty(n => n.IsRead, true)
                .SetProperty(n => n.ReadAt, now));

        return updatedCount;
    }

    public async Task<bool> DeleteAsync(int userId, int notificationId)
    {
        var notification = await GetByIdAsync(notificationId, userId);
        if (notification == null)
            return false;

        await DeleteAsync(notification);
        return true;
    }
}
