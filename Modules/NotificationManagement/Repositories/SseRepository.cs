using System.Collections.Concurrent;
using System.Threading.Channels;

namespace FootballField.API.Modules.NotificationManagement.Repositories
{
    /// <summary>
    /// Simple in-memory SSE repository. Stores Channel<string> per connection.
    /// </summary>
    public class SseRepository : ISseRepository
    {
        private readonly ConcurrentDictionary<int, List<Channel<string>>> _map = new();

        public void AddConnection(int userId, Channel<string> channel)
        {
            var list = _map.GetOrAdd(userId, _ => new List<Channel<string>>());
            lock (list)
            {
                list.Add(channel);
            }
        }

        public void RemoveConnection(int userId, Channel<string> channel)
        {
            if (_map.TryGetValue(userId, out var list))
            {
                lock (list)
                {
                    list.Remove(channel);
                    if (list.Count == 0)
                    {
                        _map.TryRemove(userId, out _);
                    }
                }
            }
        }

        public void PushToUser(int userId, string jsonPayload)
        {
            if (!_map.TryGetValue(userId, out var list)) return;

            List<Channel<string>> snapshot;
            lock (list)
            {
                snapshot = list.ToList();
            }

            foreach (var ch in snapshot)
            {
                try
                {
                    // best-effort non-blocking
                    ch.Writer.TryWrite(jsonPayload);
                }
                catch
                {
                    // ignore
                }
            }
        }
    }
}
