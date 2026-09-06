using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using WebApplication2.MainClasses;

namespace WebApplication2.cls
{
    public class clsAdminDashboard
    {
        public Dictionary<string, object> SelectSummary()
        {
            var sql = new clsSQL();
            string main = sql.MainDataBaseconString;

            var tenants = SafeReadSingleRow(sql, @"
SELECT
    COUNT(*) AS TotalCompanies,
    SUM(CASE WHEN ISNULL(C.IsSuspended, 0) = 0 AND ISNULL(C.IsActive, 1) = 1 THEN 1 ELSE 0 END) AS ActiveCompanies,
    SUM(CASE WHEN ISNULL(C.IsSuspended, 0) = 1 THEN 1 ELSE 0 END) AS SuspendedCompanies,
    SUM(CASE WHEN ISNULL(C.IsActive, 1) = 0 AND ISNULL(C.IsSuspended, 0) = 0 THEN 1 ELSE 0 END) AS InactiveCompanies,
    SUM(CASE WHEN C.SubscriptionExpiry IS NOT NULL AND C.SubscriptionExpiry < GETDATE() THEN 1 ELSE 0 END) AS ExpiredSubscriptions,
    SUM(CASE WHEN C.SubscriptionExpiry IS NOT NULL AND C.SubscriptionExpiry >= GETDATE()
              AND C.SubscriptionExpiry <= DATEADD(day, 30, GETDATE()) THEN 1 ELSE 0 END) AS ExpiringSoon,
    SUM(ISNULL(S.ActiveSessionsToday, 0)) AS ActiveSessionsToday,
    SUM(CASE WHEN COALESCE(C.LastLoginDate, S.LastLoginDate) >= DATEADD(hour, -24, GETDATE()) THEN 1 ELSE 0 END) AS CompaniesLoggedIn24h
FROM tbl_Company C
LEFT JOIN tbl_AuditCompanySnapshot S ON S.CompanyID = C.ID", main);

            var desktop = SafeReadSingleRow(sql, @"
SELECT
    COUNT(*) AS TotalDevices,
    SUM(CASE WHEN D.LastSeen IS NULL OR D.LastSeen < DATEADD(day, -7, GETDATE()) THEN 1 ELSE 0 END) AS StaleDevices,
    SUM(CASE WHEN D.UpdateStatus IN ('PendingDownload', 'Downloading', 'Ready', 'RollbackPending') THEN 1 ELSE 0 END) AS UpdatesInProgress,
    SUM(CASE WHEN D.UpdateStatus = 'Failed' THEN 1 ELSE 0 END) AS FailedUpdates
FROM tbl_DesktopDevice D", main);

            var latestRelease = SafeReadSingleRow(sql, @"
SELECT TOP 1 AppVersion, BuildNumber
FROM tbl_DesktopRelease
ORDER BY BuildNumber DESC, ID DESC", main);

            Dictionary<string, object> security;
            List<Dictionary<string, object>> recentSecurityEvents;
            try
            {
                security = ReadSingleRow(sql, @"
SELECT
    SUM(CASE WHEN Action = 'LoginSuccess' AND CreatedAt >= DATEADD(hour, -24, GETDATE()) THEN 1 ELSE 0 END) AS Logins24h,
    SUM(CASE WHEN Action IN ('LoginFailed', 'LoginOtpFailed', 'LoginLocked') AND CreatedAt >= DATEADD(hour, -24, GETDATE()) THEN 1 ELSE 0 END) AS FailedAttempts24h,
    SUM(CASE WHEN Success = 0 AND CreatedAt >= DATEADD(hour, -24, GETDATE()) THEN 1 ELSE 0 END) AS SecurityAlerts24h
FROM tbl_AdminAuditLog", main);
                recentSecurityEvents = TableToList(SelectRecentSecurityEvents(sql, main, 6));
            }
            catch
            {
                security = new Dictionary<string, object>();
                recentSecurityEvents = new List<Dictionary<string, object>>();
            }

            List<Dictionary<string, object>> attentionCompanies;
            try { attentionCompanies = TableToList(SelectAttentionCompanies(sql, main, 8)); }
            catch { attentionCompanies = new List<Dictionary<string, object>>(); }

            List<Dictionary<string, object>> topActiveCompanies;
            try { topActiveCompanies = TableToList(SelectTopActiveCompanies(sql, main, 6)); }
            catch { topActiveCompanies = new List<Dictionary<string, object>>(); }

            Dictionary<string, object> migrationDrift;
            try { migrationDrift = ReadMigrationDriftSummary(sql, main); }
            catch { migrationDrift = new Dictionary<string, object>(); }

            return new Dictionary<string, object>
            {
                ["tenants"] = tenants,
                ["desktop"] = desktop,
                ["latestRelease"] = latestRelease,
                ["security"] = security,
                ["attentionCompanies"] = attentionCompanies,
                ["recentSecurityEvents"] = recentSecurityEvents,
                ["topActiveCompanies"] = topActiveCompanies,
                ["migrationDrift"] = migrationDrift,
                ["generatedAt"] = DateTime.Now.ToString("o"),
            };
        }

        static DataTable SelectAttentionCompanies(clsSQL sql, string mainCon, int topN)
        {
            SqlParameter[] prm =
            {
                new SqlParameter("@TopN", SqlDbType.Int) { Value = topN },
            };

            return sql.ExecuteQueryStatement(@"
SELECT TOP (@TopN)
    C.ID AS CompanyID,
    ISNULL(NULLIF(C.EName, ''), C.AName) AS CompanyName,
    ISNULL(C.IsSuspended, 0) AS IsSuspended,
    ISNULL(C.IsActive, 1) AS IsActive,
    C.SubscriptionExpiry,
    COALESCE(C.LastLoginDate, S.LastLoginDate) AS LastLoginDate,
    CASE
        WHEN ISNULL(C.IsSuspended, 0) = 1 THEN 'Suspended'
        WHEN C.SubscriptionExpiry IS NOT NULL AND C.SubscriptionExpiry < GETDATE() THEN 'Subscription expired'
        WHEN C.SubscriptionExpiry IS NOT NULL AND C.SubscriptionExpiry <= DATEADD(day, 30, GETDATE()) THEN 'Expiring soon'
        WHEN COALESCE(C.LastLoginDate, S.LastLoginDate) IS NULL OR COALESCE(C.LastLoginDate, S.LastLoginDate) < DATEADD(day, -30, GETDATE()) THEN 'No recent login'
        ELSE 'Review'
    END AS AttentionReason
FROM tbl_Company C
LEFT JOIN tbl_AuditCompanySnapshot S ON S.CompanyID = C.ID
WHERE ISNULL(C.IsSuspended, 0) = 1
   OR (C.SubscriptionExpiry IS NOT NULL AND C.SubscriptionExpiry <= DATEADD(day, 30, GETDATE()))
   OR COALESCE(C.LastLoginDate, S.LastLoginDate) IS NULL
   OR COALESCE(C.LastLoginDate, S.LastLoginDate) < DATEADD(day, -30, GETDATE())
ORDER BY
    CASE WHEN ISNULL(C.IsSuspended, 0) = 1 THEN 0 ELSE 1 END,
    C.SubscriptionExpiry,
    COALESCE(C.LastLoginDate, S.LastLoginDate)", mainCon, prm);
        }

        static DataTable SelectRecentSecurityEvents(clsSQL sql, string mainCon, int topN)
        {
            SqlParameter[] prm =
            {
                new SqlParameter("@TopN", SqlDbType.Int) { Value = topN },
            };

            return sql.ExecuteQueryStatement(@"
SELECT TOP (@TopN)
    Action,
    AdminUser,
    Details,
    Success,
    CreatedAt
FROM tbl_AdminAuditLog
ORDER BY ID DESC", mainCon, prm);
        }

        static DataTable SelectTopActiveCompanies(clsSQL sql, string mainCon, int topN)
        {
            SqlParameter[] prm =
            {
                new SqlParameter("@TopN", SqlDbType.Int) { Value = topN },
            };

            return sql.ExecuteQueryStatement(@"
SELECT TOP (@TopN)
    C.ID AS CompanyID,
    ISNULL(NULLIF(C.EName, ''), C.AName) AS CompanyName,
    ISNULL(S.ActiveSessionsToday, 0) AS ActiveSessionsToday,
    COALESCE(C.LastLoginDate, S.LastLoginDate) AS LastLoginDate,
    ISNULL(S.TotalUsers, C.ActiveUserCount) AS TotalUsers
FROM tbl_Company C
LEFT JOIN tbl_AuditCompanySnapshot S ON S.CompanyID = C.ID
WHERE ISNULL(C.IsSuspended, 0) = 0 AND ISNULL(C.IsActive, 1) = 1
ORDER BY ISNULL(S.ActiveSessionsToday, 0) DESC, COALESCE(C.LastLoginDate, S.LastLoginDate) DESC", mainCon, prm);
        }

        static Dictionary<string, object> ReadMigrationDriftSummary(clsSQL sql, string mainCon)
        {
            decimal target = new clsAdminOps().ResolveTargetSchemaVersion();
            SqlParameter[] prm =
            {
                new SqlParameter("@TargetVersion", SqlDbType.Decimal) { Value = target },
            };

            return ReadSingleRow(sql, @"
SELECT
    @TargetVersion AS TargetVersion,
    SUM(CASE WHEN ISNULL(C.DBVersionCache, 0) < @TargetVersion THEN 1 ELSE 0 END) AS BehindCount,
    MAX(@TargetVersion - ISNULL(C.DBVersionCache, 0)) AS MaxGap
FROM tbl_Company C", mainCon, prm);
        }

        static Dictionary<string, object> SafeReadSingleRow(clsSQL sql, string query, string connection, SqlParameter[] parameters = null)
        {
            try
            {
                return ReadSingleRow(sql, query, connection, parameters);
            }
            catch
            {
                return new Dictionary<string, object>();
            }
        }

        static Dictionary<string, object> ReadSingleRow(clsSQL sql, string query, string connection, SqlParameter[] parameters)
        {
            // clsSQL.ExecuteQueryStatement(..., SqlParameter[]) does AddRange(parameters) —
            // null parameters must use the no-array overload or it throws ArgumentNullException.
            DataTable dt = parameters == null || parameters.Length == 0
                ? sql.ExecuteQueryStatement(query, connection)
                : sql.ExecuteQueryStatement(query, connection, parameters);
            if (dt == null || dt.Rows.Count == 0)
            {
                return new Dictionary<string, object>();
            }

            var row = dt.Rows[0];
            var map = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);
            foreach (DataColumn col in dt.Columns)
            {
                object value = row[col];
                map[col.ColumnName] = value == DBNull.Value ? null : value;
            }

            return map;
        }

        static Dictionary<string, object> ReadSingleRow(clsSQL sql, string query, string connection)
        {
            return ReadSingleRow(sql, query, connection, null);
        }

        static List<Dictionary<string, object>> TableToList(DataTable dt)
        {
            var list = new List<Dictionary<string, object>>();
            if (dt == null || dt.Rows.Count == 0) return list;

            foreach (DataRow row in dt.Rows)
            {
                var map = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);
                foreach (DataColumn col in dt.Columns)
                {
                    object value = row[col];
                    map[col.ColumnName] = value == DBNull.Value ? null : value;
                }

                list.Add(map);
            }

            return list;
        }
    }
}
