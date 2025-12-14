using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FootballField.API.Modules.AIManagement.Services
{
    public record ChatMessage(
        int UserId,
        string Role,
        string Content,
        DateTime CreatedAt);

    /// <summary>
    /// In-memory chat history service (scoped).
    /// Không chứa system prompt.
    /// Không lộ thông tin nội bộ cho LLM.
    /// </summary>
    public class ChatHistoryService
    {
        private readonly ConcurrentDictionary<int, ConcurrentQueue<ChatMessage>> _histories = new();
        private readonly int _maxMessagesPerUser;

        public ChatHistoryService(int maxMessagesPerUser = 50)
        {
            _maxMessagesPerUser = Math.Max(10, maxMessagesPerUser);
        }

        public void AddMessage(int userId, string role, string content)
        {
            var queue = _histories.GetOrAdd(
                userId,
                _ => new ConcurrentQueue<ChatMessage>());

            queue.Enqueue(new ChatMessage(
                userId,
                role,
                content,
                DateTime.UtcNow));

            while (queue.Count > _maxMessagesPerUser && queue.TryDequeue(out _)) { }
        }

        public IReadOnlyList<ChatMessage> GetHistory(int userId)
        {
            if (!_histories.TryGetValue(userId, out var q))
                return Array.Empty<ChatMessage>();

            return q.ToArray();
        }

        public void ClearHistory(int userId)
        {
            _histories.TryRemove(userId, out _);
        }

        /// <summary>
        /// Chỉ build conversation context, KHÔNG system prompt.
        /// </summary>
        public string GetConversationPrompt(
            int userId,
            string userMessage,
            int includeLastN = 20)
        {
            var sb = new StringBuilder();

            var history = GetHistory(userId);
            var last = history
                .Skip(Math.Max(0, history.Count - includeLastN))
                .ToList();

            if (last.Any())
            {
                foreach (var m in last)
                {
                    sb.AppendLine($"{m.Role}: {m.Content}");
                }
            }

            sb.AppendLine($"User: {userMessage}");
            sb.AppendLine("Assistant:");

            return sb.ToString();
        }
    }
}
