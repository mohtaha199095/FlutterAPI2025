using Microsoft.Data.SqlClient;
using System;
using System.Collections.Concurrent;
using System.Data;
using System.Security.Cryptography;
using WebApplication2.MainClasses;

namespace WebApplication2.cls
{
    /// <summary>
    /// Admin OTP challenges (in-memory) and authenticated sessions (DB-backed).
    /// </summary>
    public static class clsAdminSession
    {
        private sealed class OtpChallenge
        {
            public string UserName { get; set; } = "";
            public string Email { get; set; } = "";
            public string Otp { get; set; } = "";
            public DateTime ExpiresAt { get; set; }
            public DateTime CreatedAt { get; set; }
            public int Attempts { get; set; }
            public bool Consumed { get; set; }
        }

        private sealed class AdminSession
        {
            public string UserName { get; set; } = "";
            public string Email { get; set; } = "";
            public DateTime ExpiresAt { get; set; }
        }

        private static readonly ConcurrentDictionary<string, OtpChallenge> _challenges =
            new ConcurrentDictionary<string, OtpChallenge>(StringComparer.OrdinalIgnoreCase);

        private static readonly ConcurrentDictionary<string, AdminSession> _sessionCache =
            new ConcurrentDictionary<string, AdminSession>(StringComparer.OrdinalIgnoreCase);

        public const int OtpExpiryMinutes = 10;
        public const int SessionExpiryMinutes = 480;
        public const int MaxOtpAttempts = 5;
        public const int ResendCooldownSeconds = 60;

        public static string GenerateOtp()
        {
            int value = RandomNumberGenerator.GetInt32(0, 1_000_000);
            return value.ToString("D6");
        }

        public static string MaskEmail(string email)
        {
            if (string.IsNullOrWhiteSpace(email) || !email.Contains("@"))
            {
                return "your admin email";
            }

            var parts = email.Split('@');
            var local = parts[0];
            var domain = parts[1];
            if (local.Length <= 2)
            {
                return $"{local[0]}***@{domain}";
            }

            return $"{local.Substring(0, 2)}***@{domain}";
        }

        public static (bool ok, string challengeId, string reason) CreateOtpChallenge(
            string userName,
            string email,
            out string otpCode)
        {
            otpCode = GenerateOtp();
            CleanupExpired();

            foreach (var pair in _challenges)
            {
                var challenge = pair.Value;
                if (challenge.Consumed) continue;
                if (!string.Equals(challenge.Email, email, StringComparison.OrdinalIgnoreCase)) continue;
                if ((DateTime.UtcNow - challenge.CreatedAt).TotalSeconds < ResendCooldownSeconds)
                {
                    return (false, "", "rate_limited");
                }
            }

            string challengeId = Guid.NewGuid().ToString("N");
            _challenges[challengeId] = new OtpChallenge
            {
                UserName = userName ?? "",
                Email = email ?? "",
                Otp = otpCode,
                CreatedAt = DateTime.UtcNow,
                ExpiresAt = DateTime.UtcNow.AddMinutes(OtpExpiryMinutes),
            };

            return (true, challengeId, "");
        }

        public static bool VerifyOtp(string challengeId, string otp, out string userName, out string email)
        {
            userName = "";
            email = "";
            CleanupExpired();

            if (string.IsNullOrWhiteSpace(challengeId) ||
                !_challenges.TryGetValue(challengeId, out var challenge) ||
                challenge == null ||
                challenge.Consumed)
            {
                return false;
            }

            if (DateTime.UtcNow > challenge.ExpiresAt)
            {
                _challenges.TryRemove(challengeId, out _);
                return false;
            }

            challenge.Attempts++;
            if (challenge.Attempts > MaxOtpAttempts)
            {
                challenge.Consumed = true;
                return false;
            }

            string normalizedOtp = (otp ?? "").Trim();
            if (normalizedOtp.Length != 6 || !normalizedOtp.Equals(challenge.Otp, StringComparison.Ordinal))
            {
                return false;
            }

            challenge.Consumed = true;
            userName = challenge.UserName;
            email = challenge.Email;
            return true;
        }

        public static string CreateSession(string userName, string email, string clientIp = "")
        {
            CleanupExpired();
            string token = Guid.NewGuid().ToString("N") + Guid.NewGuid().ToString("N");
            var expiresAt = DateTime.UtcNow.AddMinutes(SessionExpiryMinutes);

            _sessionCache[token] = new AdminSession
            {
                UserName = userName ?? "",
                Email = email ?? "",
                ExpiresAt = expiresAt,
            };

            TryInsertSession(token, userName, email, expiresAt, clientIp);
            return token;
        }

        public static bool TryValidateToken(string token, out string userName, out string email)
        {
            userName = "";
            email = "";
            CleanupExpired();

            if (string.IsNullOrWhiteSpace(token))
            {
                return false;
            }

            token = token.Trim();

            if (_sessionCache.TryGetValue(token, out var cached) && cached != null)
            {
                if (DateTime.UtcNow <= cached.ExpiresAt)
                {
                    userName = cached.UserName;
                    email = cached.Email;
                    return true;
                }

                _sessionCache.TryRemove(token, out _);
            }

            if (TryLoadSessionFromDb(token, out userName, out email, out DateTime expiresAt))
            {
                if (DateTime.UtcNow > expiresAt)
                {
                    RevokeToken(token);
                    return false;
                }

                _sessionCache[token] = new AdminSession
                {
                    UserName = userName,
                    Email = email,
                    ExpiresAt = expiresAt,
                };
                return true;
            }

            return false;
        }

        public static void RevokeToken(string token)
        {
            if (string.IsNullOrWhiteSpace(token)) return;

            token = token.Trim();
            _sessionCache.TryRemove(token, out _);

            try
            {
                var sql = new clsSQL();
                sql.ExecuteNonQueryStatement(@"
UPDATE tbl_AdminSession SET IsRevoked = 1 WHERE Token = @Token",
                    sql.MainDataBaseconString,
                    new[] { new SqlParameter("@Token", SqlDbType.VarChar, 128) { Value = token } });
            }
            catch
            {
                // Table may not exist until migration 10.57 runs.
            }
        }

        private static void TryInsertSession(
            string token,
            string userName,
            string email,
            DateTime expiresAtUtc,
            string clientIp)
        {
            try
            {
                var sql = new clsSQL();
                sql.ExecuteNonQueryStatement(@"
INSERT INTO tbl_AdminSession (Token, UserName, Email, ExpiresAt, CreatedAt, ClientIP, IsRevoked)
VALUES (@Token, @UserName, @Email, @ExpiresAt, @CreatedAt, @ClientIP, 0)",
                    sql.MainDataBaseconString,
                    new[]
                    {
                        new SqlParameter("@Token", SqlDbType.VarChar, 128) { Value = token },
                        new SqlParameter("@UserName", SqlDbType.VarChar, 200) { Value = userName ?? "" },
                        new SqlParameter("@Email", SqlDbType.VarChar, 200) { Value = email ?? "" },
                        new SqlParameter("@ExpiresAt", SqlDbType.DateTime) { Value = expiresAtUtc },
                        new SqlParameter("@CreatedAt", SqlDbType.DateTime) { Value = DateTime.UtcNow },
                        new SqlParameter("@ClientIP", SqlDbType.VarChar, 64) { Value = clientIp ?? "" },
                    });
            }
            catch
            {
                // Table may not exist until migration 10.57 runs — cache still works.
            }
        }

        private static bool TryLoadSessionFromDb(
            string token,
            out string userName,
            out string email,
            out DateTime expiresAt)
        {
            userName = "";
            email = "";
            expiresAt = DateTime.MinValue;

            try
            {
                var sql = new clsSQL();
                DataTable dt = sql.ExecuteQueryStatement(@"
SELECT TOP 1 UserName, Email, ExpiresAt
FROM tbl_AdminSession
WHERE Token = @Token AND ISNULL(IsRevoked, 0) = 0",
                    sql.MainDataBaseconString,
                    new[] { new SqlParameter("@Token", SqlDbType.VarChar, 128) { Value = token } });

                if (dt == null || dt.Rows.Count == 0) return false;

                userName = Simulate.String(dt.Rows[0]["UserName"]);
                email = Simulate.String(dt.Rows[0]["Email"]);
                expiresAt = dt.Rows[0]["ExpiresAt"] == DBNull.Value
                    ? DateTime.MinValue
                    : Simulate.StringToDate(dt.Rows[0]["ExpiresAt"]);
                return true;
            }
            catch
            {
                return false;
            }
        }

        private static void CleanupExpired()
        {
            var now = DateTime.UtcNow;
            foreach (var pair in _challenges)
            {
                if (pair.Value.Consumed || pair.Value.ExpiresAt < now)
                {
                    _challenges.TryRemove(pair.Key, out _);
                }
            }

            foreach (var pair in _sessionCache)
            {
                if (pair.Value.ExpiresAt < now)
                {
                    _sessionCache.TryRemove(pair.Key, out _);
                }
            }

            try
            {
                var sql = new clsSQL();
                sql.ExecuteNonQueryStatement(@"
DELETE FROM tbl_AdminSession
WHERE ExpiresAt < @Now OR (ISNULL(IsRevoked, 0) = 1 AND CreatedAt < DATEADD(day, -7, @Now))",
                    sql.MainDataBaseconString,
                    new[] { new SqlParameter("@Now", SqlDbType.DateTime) { Value = now } });
            }
            catch
            {
                // Ignore if table missing.
            }
        }
    }
}
