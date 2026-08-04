using Microsoft.Data.SqlClient;
using System;
using System.Data;

namespace WebApplication2.cls
{
    public class clsAuditSession
    {
        public int InsertSession(
            Guid sessionGuid,
            int userId,
            string userName,
            AuditContext ctx,
            int companyId)
        {
            var now = DateTime.Now;
            SqlParameter[] prm =
            {
                new SqlParameter("@SessionGuid", SqlDbType.UniqueIdentifier) { Value = sessionGuid },
                new SqlParameter("@UserID", SqlDbType.Int) { Value = userId },
                new SqlParameter("@UserName", SqlDbType.NVarChar, -1) { Value = userName ?? "" },
                new SqlParameter("@LoginTime", SqlDbType.DateTime) { Value = now },
                new SqlParameter("@LastActivityTime", SqlDbType.DateTime) { Value = now },
                new SqlParameter("@IPAddress", SqlDbType.NVarChar, 100) { Value = ctx?.IPAddress ?? "" },
                new SqlParameter("@DeviceInfo", SqlDbType.NVarChar, -1) { Value = ctx?.DeviceInfo ?? "" },
                new SqlParameter("@AppVersion", SqlDbType.NVarChar, 50) { Value = ctx?.AppVersion ?? "" },
                new SqlParameter("@Platform", SqlDbType.NVarChar, 30) { Value = ctx?.Platform ?? "" },
                new SqlParameter("@IsActive", SqlDbType.Bit) { Value = true },
                new SqlParameter("@CompanyID", SqlDbType.Int) { Value = companyId },
                new SqlParameter("@CreationUserID", SqlDbType.Int) { Value = userId },
                new SqlParameter("@CreationDate", SqlDbType.DateTime) { Value = now },
            };

            string sql = @"
INSERT INTO tbl_AuditUserSession
(SessionGuid, UserID, UserName, LoginTime, LastActivityTime, IPAddress, DeviceInfo, AppVersion, Platform, IsActive, CompanyID, CreationUserID, CreationDate)
OUTPUT INSERTED.ID
VALUES (@SessionGuid, @UserID, @UserName, @LoginTime, @LastActivityTime, @IPAddress, @DeviceInfo, @AppVersion, @Platform, @IsActive, @CompanyID, @CreationUserID, @CreationDate)";

            clsSQL cls = new clsSQL();
            return Simulate.Integer32(cls.ExecuteScalar(sql, prm, cls.CreateDataBaseConnectionString(companyId)));
        }

        public bool EndSession(Guid sessionGuid, int companyId, string logoutReason)
        {
            SqlParameter[] prm =
            {
                new SqlParameter("@SessionGuid", SqlDbType.UniqueIdentifier) { Value = sessionGuid },
                new SqlParameter("@LogoutTime", SqlDbType.DateTime) { Value = DateTime.Now },
                new SqlParameter("@LogoutReason", SqlDbType.NVarChar, 50) { Value = logoutReason ?? "Manual" },
                new SqlParameter("@CompanyID", SqlDbType.Int) { Value = companyId },
            };

            string sql = @"
UPDATE tbl_AuditUserSession SET
    LogoutTime = @LogoutTime,
    LogoutReason = @LogoutReason,
    IsActive = 0,
    DurationMinutes = DATEDIFF(MINUTE, LoginTime, @LogoutTime),
    LastActivityTime = @LogoutTime
WHERE SessionGuid = @SessionGuid AND CompanyID = @CompanyID AND IsActive = 1";

            clsSQL cls = new clsSQL();
            return cls.ExecuteNonQueryStatement(sql, cls.CreateDataBaseConnectionString(companyId), prm) > 0;
        }

        public bool TouchSession(Guid sessionGuid, int companyId)
        {
            SqlParameter[] prm =
            {
                new SqlParameter("@SessionGuid", SqlDbType.UniqueIdentifier) { Value = sessionGuid },
                new SqlParameter("@LastActivityTime", SqlDbType.DateTime) { Value = DateTime.Now },
                new SqlParameter("@CompanyID", SqlDbType.Int) { Value = companyId },
            };

            string sql = @"
UPDATE tbl_AuditUserSession SET LastActivityTime = @LastActivityTime
WHERE SessionGuid = @SessionGuid AND CompanyID = @CompanyID AND IsActive = 1";

            clsSQL cls = new clsSQL();
            return cls.ExecuteNonQueryStatement(sql, cls.CreateDataBaseConnectionString(companyId), prm) > 0;
        }

        public DataTable SelectSessionByGuid(Guid sessionGuid, int companyId)
        {
            SqlParameter[] prm =
            {
                new SqlParameter("@SessionGuid", SqlDbType.UniqueIdentifier) { Value = sessionGuid },
                new SqlParameter("@CompanyID", SqlDbType.Int) { Value = companyId },
            };

            clsSQL cls = new clsSQL();
            return cls.ExecuteQueryStatement(
                "SELECT TOP 1 * FROM tbl_AuditUserSession WHERE SessionGuid = @SessionGuid AND CompanyID = @CompanyID ORDER BY ID DESC",
                cls.CreateDataBaseConnectionString(companyId), prm);
        }

        public DataTable SelectSessions(
            int companyId,
            int userId,
            DateTime dateFrom,
            DateTime dateTo,
            bool activeOnly,
            int topN)
        {
            SqlParameter[] prm =
            {
                new SqlParameter("@CompanyID", SqlDbType.Int) { Value = companyId },
                new SqlParameter("@UserID", SqlDbType.Int) { Value = userId },
                new SqlParameter("@DateFrom", SqlDbType.DateTime) { Value = dateFrom == DateTime.MinValue ? (object)DBNull.Value : dateFrom.Date },
                new SqlParameter("@DateTo", SqlDbType.DateTime) { Value = dateTo == DateTime.MinValue ? (object)DBNull.Value : dateTo.Date.AddDays(1).AddSeconds(-1) },
                new SqlParameter("@ActiveOnly", SqlDbType.Bit) { Value = activeOnly },
                new SqlParameter("@TopN", SqlDbType.Int) { Value = topN <= 0 ? 500 : topN },
            };

            string sql = @"
SELECT TOP (@TopN)
    S.ID, S.SessionGuid, S.UserID, S.UserName,
    ISNULL(E.AName, E.EName) AS EmployeeName,
    S.LoginTime, S.LogoutTime, S.LogoutReason,
    S.LastActivityTime, S.DurationMinutes,
    S.IPAddress, S.DeviceInfo, S.AppVersion, S.Platform, S.IsActive, S.CompanyID
FROM tbl_AuditUserSession S
LEFT JOIN tbl_employee E ON E.ID = S.UserID AND E.CompanyID = S.CompanyID
WHERE S.CompanyID = @CompanyID
  AND (@UserID = 0 OR S.UserID = @UserID)
  AND (@ActiveOnly = 0 OR S.IsActive = 1)
  AND (@DateFrom IS NULL OR S.LoginTime >= @DateFrom)
  AND (@DateTo IS NULL OR S.LoginTime <= @DateTo)
ORDER BY S.LoginTime DESC";

            clsSQL cls = new clsSQL();
            return cls.ExecuteQueryStatement(sql, cls.CreateDataBaseConnectionString(companyId), prm);
        }
    }
}
