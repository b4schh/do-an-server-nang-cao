using System.Threading.Channels;

namespace DoAn.Core.Application.Interfaces.Notification;

/// <summary>
/// Repository interface for managing SSE connections.
/// 
/// ⚠️ DEPLOYMENT CONSTRAINT:
/// The default in-memory implementation (SseRepository) is designed for single-instance deployments only.
/// 
/// For production environments with:
/// - Load balancers
/// - Multiple instances
/// - Horizontal scaling
/// - Docker replicas
/// 
/// You MUST implement this interface using:
/// - Redis Pub/Sub (recommended for .NET)
/// - SignalR with Redis backplane
/// - Azure SignalR Service
/// - RabbitMQ or other message brokers
/// </summary>
public interface ISseRepository
{
    /// <summary>
    /// Adds a new SSE connection for a user
    /// </summary>
    void AddConnection(int userId, Channel<string> channel);

    /// <summary>
    /// Removes an SSE connection when client disconnects
    /// </summary>
    void RemoveConnection(int userId, Channel<string> channel);

    /// <summary>
    /// Pushes a notification message to all active connections for a user
    /// </summary>
    void PushToUser(int userId, string jsonPayload);

    /// <summary>
    /// Gets connection statistics (for monitoring)
    /// </summary>
    Dictionary<string, int> GetConnectionStats();
}
