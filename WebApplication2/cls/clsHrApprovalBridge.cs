using Microsoft.Data.SqlClient;
using System;
using System.Data;
using WebApplication2.MainClasses;

namespace WebApplication2.cls
{
    /// <summary>HR-specific approval helpers (line manager routing for leave).</summary>
    public static class clsHrApprovalBridge
    {
        public const int TypeLeaveRequest = 31;

        /// <summary>
        /// Resolves the line manager's user/employee id for a leave request
        /// via tbl_employee.ReportsToEmployeeID.
        /// </summary>
        public static int ResolveLeaveRequestManagerUserId(string documentGuid, int companyId, SqlTransaction trn = null)
        {
            if (string.IsNullOrWhiteSpace(documentGuid) || companyId <= 0) return 0;

            clsSQL sql = new clsSQL();
            SqlParameter[] prm =
            {
                new SqlParameter("@Guid", SqlDbType.UniqueIdentifier) { Value = Simulate.Guid(documentGuid) },
                new SqlParameter("@CompanyID", SqlDbType.Int) { Value = companyId },
            };

            object val = sql.ExecuteScalar(@"
SELECT TOP 1 ISNULL(e.ReportsToEmployeeID, 0)
FROM tbl_LeaveRequest lr
INNER JOIN tbl_employee e ON e.ID = lr.EmployeeID AND e.CompanyID = lr.CompanyID
WHERE lr.Guid = @Guid AND lr.CompanyID = @CompanyID",
                prm, sql.CreateDataBaseConnectionString(companyId), trn);

            return Simulate.Integer32(val);
        }

        public static bool CanUserActOnLeaveLevel(ApprovalPolicyLevelRow level, int userId,
            string documentGuid, int companyId, SqlTransaction trn)
        {
            if (level != null && level.MemberUserIds != null && level.MemberUserIds.Contains(userId))
                return true;

            int managerId = ResolveLeaveRequestManagerUserId(documentGuid, companyId, trn);
            return managerId > 0 && managerId == userId;
        }
    }
}
