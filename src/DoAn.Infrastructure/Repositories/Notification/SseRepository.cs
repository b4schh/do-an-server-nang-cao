using DoAn.Core.Application.Interfaces.Notification;
using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;
using System.Threading.Channels;

namespace DoAn.Infrastructure.Repositories.Notification;

public class SseRepository : ISseRepository, IDisposable
{
    private readonly ConcurrentDictionary<int, List<ConnectionInfo>> _map = new();
    private readonly Timer _cleanupTimer;
    private readonly ILogger<SseRepository> _logger;
    private bool _disposed = false;

    public SseRepository(ILogger<SseRepository> logger)
    {
        _logger = logger;

        // Run cleanup every 5 minutes to remove dead connections
        _cleanupTimer = new Timer(CleanupDeadConnections, null, TimeSpan.FromMinutes(5), TimeSpan.FromMinutes(5));

        _logger.LogInformation("SseRepository initialized with automatic cleanup every 5 minutes");
    }

    public void AddConnection(int userId, Channel<string> channel)
    {
        var connectionInfo = new ConnectionInfo
        {
            Channel = channel,
            ConnectedAt = DateTime.UtcNow,
            LastActivity = DateTime.UtcNow
        };

        var list = _map.GetOrAdd(userId, _ => new List<ConnectionInfo>());
        lock (list)
        {
            list.Add(connectionInfo);
            _logger.LogInformation("SSE connection added for user {UserId}. Total connections: {Count}", userId, list.Count);
        }
    }

    public void RemoveConnection(int userId, Channel<string> channel)
    {
        if (_map.TryGetValue(userId, out var list))
        {
            lock (list)
            {
                var removed = list.RemoveAll(c => c.Channel == channel);
                if (removed > 0)
                {
                    _logger.LogInformation("SSE connection removed for user {UserId}. Remaining: {Count}", userId, list.Count);
                }

                if (list.Count == 0)
                {
                    _map.TryRemove(userId, out _);
                    _logger.LogInformation("All SSE connections closed for user {UserId}", userId);
                }
            }
        }
    }

    public void PushToUser(int userId, string jsonPayload)
    {
        if (!_map.TryGetValue(userId, out var list))
        {
            _logger.LogDebug("No active SSE connections for user {UserId}", userId);
            return;
        }

        List<ConnectionInfo> snapshot;
        lock (list)
        {
            snapshot = list.ToList();
        }

        var successCount = 0;
        var deadConnections = new List<ConnectionInfo>();

        foreach (var connInfo in snapshot)
        {
            try
            {
                if (connInfo.Channel.Writer.TryWrite(jsonPayload))
                {
                    connInfo.LastActivity = DateTime.UtcNow;
                    successCount++;
                }
                else
                {
                    // Channel is full or closed
                    deadConnections.Add(connInfo);
                }
            }
            catch
            {
                deadConnections.Add(connInfo);
            }
        }

        // Remove dead connections
        if (deadConnections.Count > 0)
        {
            lock (list)
            {
                foreach (var dead in deadConnections)
                {
                    list.Remove(dead);
                    try { dead.Channel.Writer.TryComplete(); } catch { }
                }

                if (list.Count == 0)
                {
                    _map.TryRemove(userId, out _);
                }
            }

            _logger.LogWarning("Removed {DeadCount} dead SSE connections for user {UserId}", deadConnections.Count, userId);
        }

        if (successCount > 0)
        {
            _logger.LogDebug("Pushed notification to {SuccessCount} SSE connections for user {UserId}", successCount, userId);
        }
    }

    /// <summary>
    /// Get statistics about active connections (for monitoring/debugging)
    /// </summary>
    public Dictionary<string, int> GetConnectionStats()
    {
        var totalUsers = _map.Count;
        var totalConnections = 0;

        foreach (var kvp in _map)
        {
            lock (kvp.Value)
            {
                totalConnections += kvp.Value.Count;
            }
        }

        return new Dictionary<string, int>
            {
                { "TotalUsers", totalUsers },
                { "TotalConnections", totalConnections }
            };
    }

    private void CleanupDeadConnections(object? state)
    {
        try
        {
            var now = DateTime.UtcNow;
            var staleThreshold = TimeSpan.FromMinutes(30); // Consider connections stale after 30 minutes of inactivity
            var removedUsers = 0;
            var removedConnections = 0;

            foreach (var kvp in _map.ToList())
            {
                var userId = kvp.Key;
                var list = kvp.Value;

                List<ConnectionInfo> staleConnections;
                lock (list)
                {
                    staleConnections = list.Where(c => now - c.LastActivity > staleThreshold).ToList();

                    foreach (var stale in staleConnections)
                    {
                        list.Remove(stale);
                        try { stale.Channel.Writer.TryComplete(); } catch { }
                        removedConnections++;
                    }

                    if (list.Count == 0)
                    {
                        _map.TryRemove(userId, out _);
                        removedUsers++;
                    }
                }
            }

            if (removedConnections > 0)
            {
                _logger.LogInformation("Cleanup: Removed {ConnectionCount} stale connections for {UserCount} users", removedConnections, removedUsers);
            }

            var stats = GetConnectionStats();
            _logger.LogInformation("SSE Stats: {TotalUsers} users, {TotalConnections} active connections", stats["TotalUsers"], stats["TotalConnections"]);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during SSE connection cleanup");
        }
    }

    public void Dispose()
    {
        if (_disposed) return;

        _cleanupTimer?.Dispose();

        // Close all active connections
        foreach (var kvp in _map)
        {
            lock (kvp.Value)
            {
                foreach (var conn in kvp.Value)
                {
                    try { conn.Channel.Writer.TryComplete(); } catch { }
                }
                kvp.Value.Clear();
            }
        }
        _map.Clear();

        _disposed = true;
        _logger.LogInformation("SseRepository disposed");
    }

    private class ConnectionInfo
    {
        public Channel<string> Channel { get; set; } = null!;
        public DateTime ConnectedAt { get; set; }
        public DateTime LastActivity { get; set; }
    }
}
