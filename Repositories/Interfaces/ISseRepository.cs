using System.Threading.Channels;

namespace FootballField.API.Repositories.Interfaces
{
    /// <summary>
    /// Repository kiểu in-memory để quản lý kết nối SSE per user.
    /// (Nếu bạn dùng multi-instance, sẽ chuyển sang Redis pub/sub.)
    /// </summary>
    public interface ISseRepository
    {
        void AddConnection(int userId, Channel<string> channel);
        void RemoveConnection(int userId, Channel<string> channel);
        void PushToUser(int userId, string jsonPayload);
    }
}
