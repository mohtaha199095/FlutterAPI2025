using Microsoft.Data.SqlClient;
using System;
using System.Data;

namespace WebApplication2.cls
{
    public class clsAuditEvent
    {
        public int InsertEvent(AuditEventRequest req)
        {
            var now = DateTime.Now;
            string ip = req.Context?.IPAddress ?? "";
            string device = req.Context?.DeviceInfo ?? "";

            SqlParameter[] prm =
            {
                new SqlParameter("@SessionID", SqlDbType.Int) { Value = req.SessionId > 0 ? req.SessionId : (object)DBNull.Value },
                new SqlParameter("@UserID", SqlDbType.Int) { Value = req.UserId },
                new SqlParameter("@ActionTypeCode", SqlDbType.NVarChar, 50) { Value = req.ActionTypeCode ?? "" },
                new SqlParameter("@ModuleName", SqlDbType.NVarChar, -1) { Value = req.ModuleName ?? "" },
                new SqlParameter("@EntityTable", SqlDbType.NVarChar, -1) { Value = req.EntityTable ?? "" },
                new SqlParameter("@RecordID", SqlDbType.Int) { Value = req.RecordId > 0 ? req.RecordId : (object)DBNull.Value },
                new SqlParameter("@RecordReference", SqlDbType.NVarChar, -1) { Value = req.RecordReference ?? "" },
                new SqlParameter("@Description", SqlDbType.NVarChar, -1) { Value = req.Description ?? "" },
                new SqlParameter("@FormID", SqlDbType.Int) { Value = req.FormId > 0 ? req.FormId : (object)DBNull.Value },
                new SqlParameter("@IPAddress", SqlDbType.NVarChar, 100) { Value = ip },
                new SqlParameter("@DeviceInfo", SqlDbType.NVarChar, -1) { Value = device },
                new SqlParameter("@OldValuesJson", SqlDbType.NVarChar, -1) { Value = string.IsNullOrEmpty(req.OldValuesJson) ? (object)DBNull.Value : req.OldValuesJson },
                new SqlParameter("@NewValuesJson", SqlDbType.NVarChar, -1) { Value = string.IsNullOrEmpty(req.NewValuesJson) ? (object)DBNull.Value : req.NewValuesJson },
                new SqlParameter("@EventDateTime", SqlDbType.DateTime) { Value = now },
                new SqlParameter("@CompanyID", SqlDbType.Int) { Value = req.CompanyId },
                new SqlParameter("@CreationUserID", SqlDbType.Int) { Value = req.UserId },
                new SqlParameter("@CreationDate", SqlDbType.DateTime) { Value = now },
            };

            string sql = @"
INSERT INTO tbl_AuditEvent
(SessionID, UserID, ActionTypeCode, ModuleName, EntityTable, RecordID, RecordReference, Description, FormID,
 IPAddress, DeviceInfo, OldValuesJson, NewValuesJson, EventDateTime, CompanyID, CreationUserID, CreationDate)
OUTPUT INSERTED.ID
VALUES
(@SessionID, @UserID, @ActionTypeCode, @ModuleName, @EntityTable, @RecordID, @RecordReference, @Description, @FormID,
 @IPAddress, @DeviceInfo, @OldValuesJson, @NewValuesJson, @EventDateTime, @CompanyID, @CreationUserID, @CreationDate)";

            clsSQL cls = new clsSQL();
            return Simulate.Integer32(cls.ExecuteScalar(sql, prm, cls.CreateDataBaseConnectionString(req.CompanyId)));
        }

        public DataTable SelectEvents(
            int companyId,
            int userId,
            string actionTypeCode,
            string moduleName,
            DateTime dateFrom,
            DateTime dateTo,
            int topN)
        {
            SqlParameter[] prm =
            {
                new SqlParameter("@CompanyID", SqlDbType.Int) { Value = companyId },
                new SqlParameter("@UserID", SqlDbType.Int) { Value = userId },
                new SqlParameter("@ActionTypeCode", SqlDbType.NVarChar, 50) { Value = actionTypeCode ?? "" },
                new SqlParameter("@ModuleName", SqlDbType.NVarChar, -1) { Value = moduleName ?? "" },
                new SqlParameter("@DateFrom", SqlDbType.DateTime) { Value = dateFrom == DateTime.MinValue ? (object)DBNull.Value : dateFrom.Date },
                new SqlParameter("@DateTo", SqlDbType.DateTime) { Value = dateTo == DateTime.MinValue ? (object)DBNull.Value : dateTo.Date.AddDays(1).AddSeconds(-1) },
                new SqlParameter("@TopN", SqlDbType.Int) { Value = topN <= 0 ? 500 : topN },
            };

            string sql = @"
SELECT TOP (@TopN)
    E.ID, E.SessionID, E.UserID,
    ISNULL(EMP.AName, EMP.EName) AS EmployeeName,
    E.ActionTypeCode, AT.AName AS ActionAName, AT.EName AS ActionEName,
    E.ModuleName, E.EntityTable, E.RecordID, E.RecordReference, E.Description,
    E.FormID, E.IPAddress, E.DeviceInfo, E.EventDateTime, E.CompanyID
FROM tbl_AuditEvent E
LEFT JOIN tbl_employee EMP ON EMP.ID = E.UserID AND EMP.CompanyID = E.CompanyID
LEFT JOIN tbl_AuditActionType AT ON AT.Code = E.ActionTypeCode AND AT.CompanyID = E.CompanyID
WHERE E.CompanyID = @CompanyID
  AND (@UserID = 0 OR E.UserID = @UserID)
  AND (@ActionTypeCode = '' OR E.ActionTypeCode = @ActionTypeCode)
  AND (@ModuleName = '' OR E.ModuleName LIKE '%' + @ModuleName + '%')
  AND (@DateFrom IS NULL OR E.EventDateTime >= @DateFrom)
  AND (@DateTo IS NULL OR E.EventDateTime <= @DateTo)
ORDER BY E.EventDateTime DESC";

            clsSQL cls = new clsSQL();
            return cls.ExecuteQueryStatement(sql, cls.CreateDataBaseConnectionString(companyId), prm);
        }

        public DataTable SelectActionTypes(int companyId)
        {
            SqlParameter[] prm =
            {
                new SqlParameter("@CompanyID", SqlDbType.Int) { Value = companyId },
            };

            clsSQL cls = new clsSQL();
            return cls.ExecuteQueryStatement(
                "SELECT * FROM tbl_AuditActionType WHERE CompanyID = @CompanyID OR @CompanyID = 0 ORDER BY Category, Code",
                cls.CreateDataBaseConnectionString(companyId), prm);
        }
    }
}
