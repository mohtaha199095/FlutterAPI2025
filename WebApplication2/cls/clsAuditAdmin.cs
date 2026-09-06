using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Data;

namespace WebApplication2.cls
{
    public class clsAuditAdmin
    {
        private static readonly Dictionary<int, DateTime> _snapshotThrottle = new Dictionary<int, DateTime>();
        private static readonly object _snapshotLock = new object();

        public DataTable SelectAllCompaniesOverview(
            string search,
            bool? activeOnly,
            bool? suspendedOnly,
            bool? expiringOnly,
            int topN,
            int offset)
        {
            SqlParameter[] prm =
            {
                new SqlParameter("@Search", SqlDbType.NVarChar, -1) { Value = search ?? "" },
                new SqlParameter("@ActiveOnly", SqlDbType.Bit) { Value = activeOnly ?? (object)DBNull.Value },
                new SqlParameter("@SuspendedOnly", SqlDbType.Bit) { Value = suspendedOnly ?? (object)DBNull.Value },
                new SqlParameter("@ExpiringOnly", SqlDbType.Bit) { Value = expiringOnly ?? (object)DBNull.Value },
                new SqlParameter("@TopN", SqlDbType.Int) { Value = topN <= 0 ? 100 : topN },
                new SqlParameter("@Offset", SqlDbType.Int) { Value = offset < 0 ? 0 : offset },
            };

            string sql = @"
SELECT
    C.ID AS CompanyID,
    C.AName AS CompanyAName,
    C.EName AS CompanyEName,
    C.DataBaseName,
    ISNULL(C.IsActive, 1) AS IsActive,
    ISNULL(C.IsSuspended, 0) AS IsSuspended,
    C.AdminNotes,
    C.SubscriptionExpiry,
    C.LastLoginDate,
    C.LastActivityDate,
    C.DBVersionCache,
    C.ActiveUserCount,
    S.LastLoginDate AS SnapshotLastLogin,
    S.LastActivityDate AS SnapshotLastActivity,
    S.ActiveSessionsToday,
    S.TotalUsers,
    S.DBVersion AS SnapshotDBVersion,
    S.UpdatedAt AS SnapshotUpdatedAt
FROM tbl_Company C
LEFT JOIN tbl_AuditCompanySnapshot S ON S.CompanyID = C.ID
WHERE (@Search = '' OR C.AName LIKE '%' + @Search + '%' OR C.EName LIKE '%' + @Search + '%'
       OR C.Tel1 LIKE '%' + @Search + '%' OR C.DataBaseName LIKE '%' + @Search + '%')
  AND (@ActiveOnly IS NULL OR ISNULL(C.IsActive, 1) = @ActiveOnly)
  AND (@SuspendedOnly IS NULL OR ISNULL(C.IsSuspended, 0) = @SuspendedOnly)
  AND (@ExpiringOnly IS NULL OR (
        C.SubscriptionExpiry IS NOT NULL AND C.SubscriptionExpiry <= DATEADD(day, 30, GETDATE())
      ))
ORDER BY C.ID
OFFSET @Offset ROWS FETCH NEXT @TopN ROWS ONLY";

            clsSQL cls = new clsSQL();
            return cls.ExecuteQueryStatement(sql, cls.MainDataBaseconString, prm);
        }

        public bool UpdateCompanyStatus(
            int companyId,
            bool? isActive,
            bool? isSuspended,
            string adminNotes,
            DateTime subscriptionExpiry)
        {
            SqlParameter[] prm =
            {
                new SqlParameter("@CompanyID", SqlDbType.Int) { Value = companyId },
                new SqlParameter("@IsActive", SqlDbType.Bit) { Value = isActive ?? (object)DBNull.Value },
                new SqlParameter("@IsSuspended", SqlDbType.Bit) { Value = isSuspended ?? (object)DBNull.Value },
                new SqlParameter("@AdminNotes", SqlDbType.NVarChar, -1) { Value = adminNotes ?? "" },
                new SqlParameter("@SubscriptionExpiry", SqlDbType.DateTime) { Value = subscriptionExpiry == DateTime.MinValue ? (object)DBNull.Value : subscriptionExpiry },
                new SqlParameter("@ModificationDate", SqlDbType.DateTime) { Value = DateTime.Now },
            };

            string sql = @"
UPDATE tbl_Company SET
    IsActive = CASE WHEN @IsActive IS NULL THEN IsActive ELSE @IsActive END,
    IsSuspended = CASE WHEN @IsSuspended IS NULL THEN IsSuspended ELSE @IsSuspended END,
    AdminNotes = CASE WHEN @AdminNotes = '' THEN AdminNotes ELSE @AdminNotes END,
    SubscriptionExpiry = CASE WHEN @SubscriptionExpiry IS NULL THEN SubscriptionExpiry ELSE @SubscriptionExpiry END,
    ModificationDate = @ModificationDate
WHERE ID = @CompanyID";

            clsSQL cls = new clsSQL();
            return cls.ExecuteNonQueryStatement(sql, cls.MainDataBaseconString, prm) > 0;
        }

        public void RefreshCompanySnapshot(int companyId, bool force = false)
        {
            if (!force && !ShouldRefreshSnapshot(companyId)) return;

            clsSQL cls = new clsSQL();
            string companyCon = cls.CreateDataBaseConnectionString(companyId);
            if (string.IsNullOrWhiteSpace(companyCon)) return;

            DataTable dtCompany = cls.ExecuteQueryStatement(
                "SELECT TOP 1 * FROM tbl_Company WHERE ID = " + Simulate.String(companyId),
                cls.MainDataBaseconString, null);
            if (dtCompany == null || dtCompany.Rows.Count == 0) return;

            var row = dtCompany.Rows[0];
            decimal dbVersion = 0;
            DateTime lastLogin = DateTime.MinValue;
            DateTime lastActivity = DateTime.MinValue;
            int activeSessionsToday = 0;
            int totalUsers = 0;

            try
            {
                DataTable dtVersion = cls.ExecuteQueryStatement(
                    "SELECT TOP 1 VersionNumber FROM tbl_DataBaseVersion ORDER BY VersionNumber DESC",
                    companyCon, null);
                if (dtVersion != null && dtVersion.Rows.Count > 0)
                    dbVersion = Simulate.decimal_(dtVersion.Rows[0]["VersionNumber"]);

                DataTable dtUsers = cls.ExecuteQueryStatement(
                    "SELECT COUNT(*) AS Cnt FROM tbl_employee WHERE CompanyID = @CompanyID AND IsSystemUser = 1",
                    companyCon,
                    new[] { new SqlParameter("@CompanyID", SqlDbType.Int) { Value = companyId } });
                if (dtUsers != null && dtUsers.Rows.Count > 0)
                    totalUsers = Simulate.Integer32(dtUsers.Rows[0]["Cnt"]);

                DataTable dtSessions = cls.ExecuteQueryStatement(@"
SELECT
    MAX(LoginTime) AS LastLogin,
    MAX(LastActivityTime) AS LastActivity,
    SUM(CASE WHEN IsActive = 1 AND CONVERT(date, LoginTime) = CONVERT(date, GETDATE()) THEN 1 ELSE 0 END) AS ActiveToday
FROM tbl_AuditUserSession WHERE CompanyID = @CompanyID",
                    companyCon,
                    new[] { new SqlParameter("@CompanyID", SqlDbType.Int) { Value = companyId } });
                if (dtSessions != null && dtSessions.Rows.Count > 0)
                {
                    lastLogin = Simulate.StringToDate(dtSessions.Rows[0]["LastLogin"]);
                    lastActivity = Simulate.StringToDate(dtSessions.Rows[0]["LastActivity"]);
                    activeSessionsToday = Simulate.Integer32(dtSessions.Rows[0]["ActiveToday"]);
                }
            }
            catch
            {
                // Company DB may not have audit tables yet on first run.
            }

            SqlParameter[] prm =
            {
                new SqlParameter("@CompanyID", SqlDbType.Int) { Value = companyId },
                new SqlParameter("@CompanyAName", SqlDbType.NVarChar, -1) { Value = Simulate.String(row["AName"]) },
                new SqlParameter("@CompanyEName", SqlDbType.NVarChar, -1) { Value = Simulate.String(row["EName"]) },
                new SqlParameter("@DataBaseName", SqlDbType.NVarChar, -1) { Value = Simulate.String(row["DataBaseName"]) },
                new SqlParameter("@LastLoginDate", SqlDbType.DateTime) { Value = lastLogin == DateTime.MinValue ? (object)DBNull.Value : lastLogin },
                new SqlParameter("@LastActivityDate", SqlDbType.DateTime) { Value = lastActivity == DateTime.MinValue ? (object)DBNull.Value : lastActivity },
                new SqlParameter("@ActiveSessionsToday", SqlDbType.Int) { Value = activeSessionsToday },
                new SqlParameter("@TotalUsers", SqlDbType.Int) { Value = totalUsers },
                new SqlParameter("@DBVersion", SqlDbType.Decimal) { Value = dbVersion },
                new SqlParameter("@IsActive", SqlDbType.Bit) { Value = row["IsActive"] == DBNull.Value ? true : Simulate.Bool(row["IsActive"]) },
                new SqlParameter("@IsSuspended", SqlDbType.Bit) { Value = row["IsSuspended"] == DBNull.Value ? false : Simulate.Bool(row["IsSuspended"]) },
                new SqlParameter("@UpdatedAt", SqlDbType.DateTime) { Value = DateTime.Now },
            };

            string upsert = @"
IF EXISTS (SELECT 1 FROM tbl_AuditCompanySnapshot WHERE CompanyID = @CompanyID)
    UPDATE tbl_AuditCompanySnapshot SET
        CompanyAName = @CompanyAName, CompanyEName = @CompanyEName, DataBaseName = @DataBaseName,
        LastLoginDate = @LastLoginDate, LastActivityDate = @LastActivityDate,
        ActiveSessionsToday = @ActiveSessionsToday, TotalUsers = @TotalUsers, DBVersion = @DBVersion,
        IsActive = @IsActive, IsSuspended = @IsSuspended, UpdatedAt = @UpdatedAt
    WHERE CompanyID = @CompanyID
ELSE
    INSERT INTO tbl_AuditCompanySnapshot
    (CompanyID, CompanyAName, CompanyEName, DataBaseName, LastLoginDate, LastActivityDate,
     ActiveSessionsToday, TotalUsers, DBVersion, IsActive, IsSuspended, UpdatedAt)
    VALUES
    (@CompanyID, @CompanyAName, @CompanyEName, @DataBaseName, @LastLoginDate, @LastActivityDate,
     @ActiveSessionsToday, @TotalUsers, @DBVersion, @IsActive, @IsSuspended, @UpdatedAt)";

            cls.ExecuteNonQueryStatement(upsert, cls.MainDataBaseconString, prm);

            SqlParameter[] companyUpdatePrm =
            {
                new SqlParameter("@CompanyID", SqlDbType.Int) { Value = companyId },
                new SqlParameter("@LastLoginDate", SqlDbType.DateTime) { Value = lastLogin == DateTime.MinValue ? (object)DBNull.Value : lastLogin },
                new SqlParameter("@LastActivityDate", SqlDbType.DateTime) { Value = lastActivity == DateTime.MinValue ? (object)DBNull.Value : lastActivity },
                new SqlParameter("@DBVersion", SqlDbType.Decimal) { Value = dbVersion },
                new SqlParameter("@TotalUsers", SqlDbType.Int) { Value = totalUsers },
                new SqlParameter("@UpdatedAt", SqlDbType.DateTime) { Value = DateTime.Now },
            };

            cls.ExecuteNonQueryStatement(@"
UPDATE tbl_Company SET
    LastLoginDate = CASE WHEN @LastLoginDate IS NULL THEN LastLoginDate ELSE @LastLoginDate END,
    LastActivityDate = CASE WHEN @LastActivityDate IS NULL THEN LastActivityDate ELSE @LastActivityDate END,
    DBVersionCache = @DBVersion,
    ActiveUserCount = @TotalUsers,
    ModificationDate = @UpdatedAt
WHERE ID = @CompanyID", cls.MainDataBaseconString, companyUpdatePrm);

            MarkSnapshotRefreshed(companyId);
        }

        public DataTable SelectCompanyAuditSummary(int companyId, int topN)
        {
            clsAuditSession sessions = new clsAuditSession();
            clsAuditEvent events = new clsAuditEvent();

            DataTable dtSessions = sessions.SelectSessions(companyId, 0, DateTime.MinValue, DateTime.MinValue, false, topN);
            DataTable dtEvents = events.SelectEvents(companyId, 0, "", "", DateTime.MinValue, DateTime.MinValue, topN);

            DataTable result = new DataTable("CompanyAuditSummary");
            result.Columns.Add("Section", typeof(string));
            result.Columns.Add("Payload", typeof(string));

            result.Rows.Add("Sessions", dtSessions != null ? Newtonsoft.Json.JsonConvert.SerializeObject(dtSessions) : "[]");
            result.Rows.Add("Events", dtEvents != null ? Newtonsoft.Json.JsonConvert.SerializeObject(dtEvents) : "[]");
            return result;
        }

        private static bool ShouldRefreshSnapshot(int companyId)
        {
            lock (_snapshotLock)
            {
                if (!_snapshotThrottle.TryGetValue(companyId, out var last)) return true;
                return (DateTime.UtcNow - last).TotalMinutes >= 5;
            }
        }

        private static void MarkSnapshotRefreshed(int companyId)
        {
            lock (_snapshotLock)
            {
                _snapshotThrottle[companyId] = DateTime.UtcNow;
            }
        }
    }
}
