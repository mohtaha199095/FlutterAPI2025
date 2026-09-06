using Microsoft.Data.SqlClient;
using System;
using System.Data;
using WebApplication2.MainClasses;

namespace WebApplication2.cls
{
    public static class clsAdminAuditLog
    {
        public static void Write(
            string action,
            string adminUser,
            string details = "",
            string clientIp = "",
            bool success = true)
        {
            try
            {
                var sql = new clsSQL();
                SqlParameter[] prm =
                {
                    new SqlParameter("@Action", SqlDbType.NVarChar, 80) { Value = action ?? "" },
                    new SqlParameter("@AdminUser", SqlDbType.NVarChar, 200) { Value = adminUser ?? "" },
                    new SqlParameter("@Details", SqlDbType.NVarChar, -1) { Value = details ?? "" },
                    new SqlParameter("@ClientIP", SqlDbType.NVarChar, 64) { Value = clientIp ?? "" },
                    new SqlParameter("@Success", SqlDbType.Bit) { Value = success },
                    new SqlParameter("@CreatedAt", SqlDbType.DateTime) { Value = DateTime.Now },
                };

                sql.ExecuteNonQueryStatement(@"
INSERT INTO tbl_AdminAuditLog (Action, AdminUser, Details, ClientIP, Success, CreatedAt)
VALUES (@Action, @AdminUser, @Details, @ClientIP, @Success, @CreatedAt);",
                    sql.MainDataBaseconString,
                    prm);
            }
            catch
            {
                // Audit logging must never break admin operations.
            }
        }

        public static DataTable SelectRecent(int topN = 100)
        {
            int take = topN <= 0 ? 100 : Math.Min(topN, 500);
            var sql = new clsSQL();
            SqlParameter[] prm =
            {
                new SqlParameter("@TopN", SqlDbType.Int) { Value = take },
            };

            return sql.ExecuteQueryStatement(@"
SELECT TOP (@TopN)
    ID,
    Action,
    AdminUser,
    Details,
    ClientIP,
    Success,
    CreatedAt
FROM tbl_AdminAuditLog
ORDER BY ID DESC;",
                sql.MainDataBaseconString,
                prm);
        }
    }
}
