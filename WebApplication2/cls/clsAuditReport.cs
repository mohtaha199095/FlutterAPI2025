using Microsoft.Data.SqlClient;
using System;
using System.Data;

namespace WebApplication2.cls
{
    public class clsAuditReport
    {
        public DataTable SelectLoginSummaryReport(int companyId, DateTime dateFrom, DateTime dateTo)
        {
            SqlParameter[] prm =
            {
                new SqlParameter("@CompanyID", SqlDbType.Int) { Value = companyId },
                new SqlParameter("@DateFrom", SqlDbType.DateTime) { Value = dateFrom == DateTime.MinValue ? (object)DBNull.Value : dateFrom.Date },
                new SqlParameter("@DateTo", SqlDbType.DateTime) { Value = dateTo == DateTime.MinValue ? (object)DBNull.Value : dateTo.Date.AddDays(1).AddSeconds(-1) },
            };

            string sql = @"
SELECT
    S.UserID,
    ISNULL(E.AName, E.EName) AS EmployeeName,
    COUNT(*) AS LoginCount,
    MAX(S.LoginTime) AS LastLogin,
    SUM(ISNULL(S.DurationMinutes, DATEDIFF(MINUTE, S.LoginTime, ISNULL(S.LogoutTime, GETDATE())))) AS TotalSessionMinutes
FROM tbl_AuditUserSession S
LEFT JOIN tbl_employee E ON E.ID = S.UserID AND E.CompanyID = S.CompanyID
WHERE S.CompanyID = @CompanyID
  AND (@DateFrom IS NULL OR S.LoginTime >= @DateFrom)
  AND (@DateTo IS NULL OR S.LoginTime <= @DateTo)
GROUP BY S.UserID, E.AName, E.EName
ORDER BY LastLogin DESC";

            clsSQL cls = new clsSQL();
            return cls.ExecuteQueryStatement(sql, cls.CreateDataBaseConnectionString(companyId), prm);
        }
    }
}
