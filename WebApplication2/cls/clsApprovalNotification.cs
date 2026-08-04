using Microsoft.Data.SqlClient;
using System;
using System.Data;

namespace WebApplication2.cls
{
    public class clsApprovalNotification
    {
        public DataTable SelectForUser(int userId, int companyId, bool unreadOnly, int topN = 100)
        {
            clsSQL sql = new clsSQL();
            SqlParameter[] prm =
            {
                new SqlParameter("@UserID", SqlDbType.Int) { Value = userId },
                new SqlParameter("@CompanyID", SqlDbType.Int) { Value = companyId },
                new SqlParameter("@UnreadOnly", SqlDbType.Bit) { Value = unreadOnly },
                new SqlParameter("@TopN", SqlDbType.Int) { Value = topN },
            };

            return sql.ExecuteQueryStatement(@"
SELECT TOP (@TopN)
       ID, Title, Body, EntityType, EntityGuid, IsRead, CreatedDate
FROM tbl_ApprovalNotification
WHERE CompanyID = @CompanyID
  AND UserID = @UserID
  AND (@UnreadOnly = 0 OR IsRead = 0)
ORDER BY CreatedDate DESC", sql.CreateDataBaseConnectionString(companyId), prm);
        }

        public int InsertNotification(
            int userId,
            int companyId,
            string title,
            string body,
            string entityType,
            string entityGuid,
            SqlTransaction trn = null)
        {
            clsSQL sql = new clsSQL();
            SqlParameter[] prm =
            {
                new SqlParameter("@UserID", SqlDbType.Int) { Value = userId },
                new SqlParameter("@CompanyID", SqlDbType.Int) { Value = companyId },
                new SqlParameter("@Title", SqlDbType.NVarChar, 200) { Value = title ?? "" },
                new SqlParameter("@Body", SqlDbType.NVarChar, 1000) { Value = body ?? "" },
                new SqlParameter("@EntityType", SqlDbType.NVarChar, 50) { Value = entityType ?? "" },
                new SqlParameter("@EntityGuid", SqlDbType.UniqueIdentifier) { Value = Simulate.Guid(entityGuid) },
            };

            return Simulate.Integer32(sql.ExecuteScalar(@"
INSERT INTO tbl_ApprovalNotification
    (UserID, CompanyID, Title, Body, EntityType, EntityGuid, IsRead, CreatedDate)
OUTPUT INSERTED.ID
VALUES
    (@UserID, @CompanyID, @Title, @Body, @EntityType, @EntityGuid, 0, GETDATE())",
                prm, sql.CreateDataBaseConnectionString(companyId), trn));
        }

        public void MarkRead(int notificationId, int userId, int companyId)
        {
            clsSQL sql = new clsSQL();
            SqlParameter[] prm =
            {
                new SqlParameter("@ID", SqlDbType.Int) { Value = notificationId },
                new SqlParameter("@UserID", SqlDbType.Int) { Value = userId },
                new SqlParameter("@CompanyID", SqlDbType.Int) { Value = companyId },
            };

            sql.ExecuteNonQueryStatement(@"
UPDATE tbl_ApprovalNotification SET IsRead = 1
WHERE ID = @ID AND UserID = @UserID AND CompanyID = @CompanyID",
                sql.CreateDataBaseConnectionString(companyId), prm);
        }

        public int CountUnread(int userId, int companyId)
        {
            clsSQL sql = new clsSQL();
            SqlParameter[] prm =
            {
                new SqlParameter("@UserID", SqlDbType.Int) { Value = userId },
                new SqlParameter("@CompanyID", SqlDbType.Int) { Value = companyId },
            };

            return Simulate.Integer32(sql.ExecuteScalar(@"
SELECT COUNT(*) FROM tbl_ApprovalNotification
WHERE UserID = @UserID AND CompanyID = @CompanyID AND IsRead = 0",
                prm, sql.CreateDataBaseConnectionString(companyId)));
        }
    }
}
