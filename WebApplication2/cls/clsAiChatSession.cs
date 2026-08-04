using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace WebApplication2.cls
{
    public sealed class AiChatMessage
    {
        public string Role { get; init; } = "";
        public string Content { get; init; } = "";
        public DateTime At { get; init; } = DateTime.UtcNow;
    }

    public sealed class AiChatSession
    {
        public string SessionId { get; init; } = "";
        public List<AiChatMessage> History { get; } = new();
        public string LastDataTopic { get; set; } = "";
        public string PendingAction { get; set; } = "";
        public string PendingTopic { get; set; } = "";
        public List<string> PendingOptions { get; set; } = new();
        public DateTime LastActivity { get; set; } = DateTime.UtcNow;
        public bool AiModeHintShown { get; set; }

        public void AddUser(string content)
        {
            History.Add(new AiChatMessage { Role = "user", Content = content });
            LastActivity = DateTime.UtcNow;
            TrimHistory();
        }

        public void AddAssistant(string content)
        {
            History.Add(new AiChatMessage { Role = "assistant", Content = content });
            LastActivity = DateTime.UtcNow;
            TrimHistory();
        }

        public void ClearPending()
        {
            PendingAction = "";
            PendingTopic = "";
            PendingOptions.Clear();
        }

        private void TrimHistory()
        {
            const int maxMessages = 40;
            if (History.Count > maxMessages)
                History.RemoveRange(0, History.Count - maxMessages);
        }

        public IReadOnlyList<AiChatMessage> GetHistoryForClient() => History.ToList();
    }

    public static class AiChatSessionStore
    {
        private static readonly ConcurrentDictionary<string, AiChatSession> Sessions = new();
        private static readonly TimeSpan Expiry = TimeSpan.FromHours(2);

        public static AiChatSession GetOrCreate(string sessionId)
        {
            string key = string.IsNullOrWhiteSpace(sessionId) ? Guid.NewGuid().ToString("N") : sessionId.Trim();
            CleanupExpired();

            return Sessions.GetOrAdd(key, id => new AiChatSession { SessionId = id });
        }

        public static void Clear(string sessionId)
        {
            if (string.IsNullOrWhiteSpace(sessionId)) return;
            Sessions.TryRemove(sessionId.Trim(), out _);
        }

        private static void CleanupExpired()
        {
            DateTime cutoff = DateTime.UtcNow - Expiry;
            foreach (KeyValuePair<string, AiChatSession> pair in Sessions.ToArray())
            {
                if (pair.Value.LastActivity < cutoff)
                    Sessions.TryRemove(pair.Key, out _);
            }
        }
    }
}
