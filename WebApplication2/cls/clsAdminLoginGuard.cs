using System;
using System.Collections.Concurrent;

namespace WebApplication2.cls
{
    /// <summary>
    /// In-memory brute-force protection for platform admin password attempts.
    /// </summary>
    public static class clsAdminLoginGuard
    {
        private sealed class AttemptState
        {
            public int FailedCount { get; set; }
            public DateTime? LockedUntilUtc { get; set; }
            public DateTime LastAttemptUtc { get; set; }
        }

        private static readonly ConcurrentDictionary<string, AttemptState> _states =
            new ConcurrentDictionary<string, AttemptState>(StringComparer.OrdinalIgnoreCase);

        public const int MaxFailedAttempts = 5;
        public const int LockoutMinutes = 15;

        public static string BuildKey(string userName, string clientIp)
        {
            string user = (userName ?? "").Trim().ToLowerInvariant();
            string ip = (clientIp ?? "").Trim();
            if (string.IsNullOrEmpty(user)) user = "unknown";
            if (string.IsNullOrEmpty(ip)) ip = "unknown";
            return $"{user}|{ip}";
        }

        public static bool IsLocked(string key, out int retryAfterSeconds)
        {
            retryAfterSeconds = 0;
            CleanupExpired();

            if (!_states.TryGetValue(key, out var state) || state?.LockedUntilUtc == null)
            {
                return false;
            }

            if (DateTime.UtcNow >= state.LockedUntilUtc.Value)
            {
                state.FailedCount = 0;
                state.LockedUntilUtc = null;
                return false;
            }

            retryAfterSeconds = Math.Max(
                1,
                (int)Math.Ceiling((state.LockedUntilUtc.Value - DateTime.UtcNow).TotalSeconds));
            return true;
        }

        public static void RegisterFailure(string key)
        {
            CleanupExpired();
            var state = _states.GetOrAdd(key, _ => new AttemptState());
            state.FailedCount++;
            state.LastAttemptUtc = DateTime.UtcNow;

            if (state.FailedCount >= MaxFailedAttempts)
            {
                state.LockedUntilUtc = DateTime.UtcNow.AddMinutes(LockoutMinutes);
            }
        }

        public static void RegisterSuccess(string key)
        {
            if (_states.TryGetValue(key, out var state) && state != null)
            {
                state.FailedCount = 0;
                state.LockedUntilUtc = null;
                state.LastAttemptUtc = DateTime.UtcNow;
            }
        }

        private static void CleanupExpired()
        {
            var now = DateTime.UtcNow;
            foreach (var pair in _states)
            {
                var state = pair.Value;
                if (state.LockedUntilUtc != null && state.LockedUntilUtc.Value <= now)
                {
                    state.FailedCount = 0;
                    state.LockedUntilUtc = null;
                }

                if ((now - state.LastAttemptUtc).TotalHours > 24)
                {
                    _states.TryRemove(pair.Key, out _);
                }
            }
        }
    }
}
