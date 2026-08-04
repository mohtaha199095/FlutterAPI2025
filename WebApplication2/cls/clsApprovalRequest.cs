using Microsoft.Data.SqlClient;
using System;
using System.Data;
using WebApplication2.MainClasses;

namespace WebApplication2.cls
{
    public class clsApprovalRequest
    {
        public DataTable SelectPendingForUser(int userId, int companyId)
        {
            new clsDataBaseVersion().EnsureApprovalWorkflowSchema(companyId);
            clsSQL sql = new clsSQL();
            SqlParameter[] prm =
            {
                new SqlParameter("@UserID", SqlDbType.Int) { Value = userId },
                new SqlParameter("@CompanyID", SqlDbType.Int) { Value = companyId },
            };

            return sql.ExecuteQueryStatement(@"
SELECT r.Guid AS RequestGuid,
       r.DocumentTypeID,
       r.DocumentGuid,
       r.DocumentNumber,
       r.CurrentLevel,
       (SELECT COUNT(*)
        FROM tbl_ApprovalPolicyLevel pl2
        WHERE pl2.PolicyID = r.PolicyID
          AND pl2.CompanyID = r.CompanyID
          AND r.TotalAmount >= ISNULL(pl2.MinAmount, 0)
          AND (ISNULL(pl2.MaxAmount, 0) = 0 OR r.TotalAmount <= pl2.MaxAmount)) AS TotalLevels,
       ISNULL(pl.LevelName, N'') AS LevelName,
       ISNULL(pl.RequireAllApprovers, 0) AS RequireAllApprovers,
       r.TotalAmount,
       r.SubmittedDate,
       r.SubmittedByUserId,
       ISNULL(su.AName, su.EName) AS SubmittedByUserName,
       t.AName AS DocumentTypeAName,
       t.EName AS DocumentTypeEName,
       ISNULL(r.Comments, N'') AS Notes
FROM tbl_ApprovalRequest r
INNER JOIN tbl_ApprovalPolicyLevel pl
    ON pl.PolicyID = r.PolicyID
   AND pl.LevelNo = r.CurrentLevel
   AND pl.CompanyID = r.CompanyID
INNER JOIN tbl_ApprovalPolicyLevelMember m
    ON m.PolicyLevelID = pl.ID
   AND m.CompanyID = pl.CompanyID
   AND m.ApproverUserID = @UserID
LEFT JOIN tbl_JournalVoucherTypes t ON t.id = r.DocumentTypeID
LEFT JOIN tbl_employee su ON su.ID = r.SubmittedByUserId
WHERE r.CompanyID = @CompanyID
  AND r.Status = 0
  AND (
        (ISNULL(pl.RequireAllApprovers, 0) = 0 AND NOT EXISTS (
            SELECT 1 FROM tbl_ApprovalAction a
            WHERE a.RequestGuid = r.Guid
              AND a.CompanyID = r.CompanyID
              AND a.LevelNo = r.CurrentLevel
              AND a.ActionType = 1))
        OR
        (ISNULL(pl.RequireAllApprovers, 0) = 1 AND NOT EXISTS (
            SELECT 1 FROM tbl_ApprovalAction a
            WHERE a.RequestGuid = r.Guid
              AND a.CompanyID = r.CompanyID
              AND a.LevelNo = r.CurrentLevel
              AND a.ActionType = 1
              AND a.ActionByUserId = @UserID))
      )
ORDER BY r.SubmittedDate DESC", sql.CreateDataBaseConnectionString(companyId), prm);
        }

        public int CountPendingForUser(int userId, int companyId)
        {
            new clsDataBaseVersion().EnsureApprovalWorkflowSchema(companyId);
            clsSQL sql = new clsSQL();
            SqlParameter[] prm =
            {
                new SqlParameter("@UserID", SqlDbType.Int) { Value = userId },
                new SqlParameter("@CompanyID", SqlDbType.Int) { Value = companyId },
            };

            return Simulate.Integer32(sql.ExecuteScalarText(@"
SELECT COUNT(*)
FROM tbl_ApprovalRequest r
INNER JOIN tbl_ApprovalPolicyLevel pl
    ON pl.PolicyID = r.PolicyID
   AND pl.LevelNo = r.CurrentLevel
   AND pl.CompanyID = r.CompanyID
INNER JOIN tbl_ApprovalPolicyLevelMember m
    ON m.PolicyLevelID = pl.ID
   AND m.CompanyID = pl.CompanyID
   AND m.ApproverUserID = @UserID
WHERE r.CompanyID = @CompanyID
  AND r.Status = 0
  AND (
        (ISNULL(pl.RequireAllApprovers, 0) = 0 AND NOT EXISTS (
            SELECT 1 FROM tbl_ApprovalAction a
            WHERE a.RequestGuid = r.Guid
              AND a.CompanyID = r.CompanyID
              AND a.LevelNo = r.CurrentLevel
              AND a.ActionType = 1))
        OR
        (ISNULL(pl.RequireAllApprovers, 0) = 1 AND NOT EXISTS (
            SELECT 1 FROM tbl_ApprovalAction a
            WHERE a.RequestGuid = r.Guid
              AND a.CompanyID = r.CompanyID
              AND a.LevelNo = r.CurrentLevel
              AND a.ActionType = 1
              AND a.ActionByUserId = @UserID))
      )", prm, sql.CreateDataBaseConnectionString(companyId)));
        }

        public DataTable SelectByDocumentGuid(string documentGuid, int companyId)
        {
            clsSQL sql = new clsSQL();
            SqlParameter[] prm =
            {
                new SqlParameter("@DocumentGuid", SqlDbType.UniqueIdentifier) { Value = Simulate.Guid(documentGuid) },
                new SqlParameter("@CompanyID", SqlDbType.Int) { Value = companyId },
            };

            return sql.ExecuteQueryStatement(@"
SELECT TOP 1 *
FROM tbl_ApprovalRequest
WHERE DocumentGuid = @DocumentGuid AND CompanyID = @CompanyID
ORDER BY CreationDate DESC", sql.CreateDataBaseConnectionString(companyId), prm);
        }

        public DataTable SelectMySubmissions(int userId, int companyId)
        {
            clsSQL sql = new clsSQL();
            SqlParameter[] prm =
            {
                new SqlParameter("@UserID", SqlDbType.Int) { Value = userId },
                new SqlParameter("@CompanyID", SqlDbType.Int) { Value = companyId },
            };

            return sql.ExecuteQueryStatement(@"
SELECT r.Guid AS RequestGuid,
       r.DocumentTypeID,
       r.DocumentGuid,
       r.DocumentNumber,
       r.CurrentLevel,
       r.Status,
       r.TotalAmount,
       r.SubmittedDate,
       r.FinalApprovedDate,
       ISNULL(t.AName, N'') AS DocumentTypeAName,
       ISNULL(t.EName, N'') AS DocumentTypeEName
FROM tbl_ApprovalRequest r
LEFT JOIN tbl_JournalVoucherTypes t ON t.id = r.DocumentTypeID
WHERE r.CompanyID = @CompanyID AND r.SubmittedByUserId = @UserID
ORDER BY r.SubmittedDate DESC", sql.CreateDataBaseConnectionString(companyId), prm);
        }

        public DataTable SelectActionHistoryByUser(int userId, int companyId, int topN = 200)
        {
            new clsDataBaseVersion().EnsureApprovalWorkflowSchema(companyId);
            clsSQL sql = new clsSQL();
            SqlParameter[] prm =
            {
                new SqlParameter("@UserID", SqlDbType.Int) { Value = userId },
                new SqlParameter("@CompanyID", SqlDbType.Int) { Value = companyId },
                new SqlParameter("@TopN", SqlDbType.Int) { Value = topN },
            };

            return sql.ExecuteQueryStatement(@"
SELECT TOP (@TopN)
       a.RequestGuid,
       a.DocumentGuid,
       a.LevelNo,
       ISNULL(a.LevelName, N'') AS LevelName,
       a.ActionType,
       CASE a.ActionType
            WHEN 1 THEN N'Approve'
            WHEN 2 THEN N'Reject'
            ELSE N'Action'
       END AS ActionName,
       a.ActionDate,
       ISNULL(a.Comments, N'') AS Comments,
       r.DocumentNumber,
       r.DocumentTypeID,
       r.TotalAmount,
       r.Status AS RequestStatus,
       ISNULL(t.AName, N'') AS DocumentTypeAName,
       ISNULL(t.EName, N'') AS DocumentTypeEName,
       ISNULL(su.AName, su.EName) AS SubmittedByUserName
FROM tbl_ApprovalAction a
INNER JOIN tbl_ApprovalRequest r
    ON r.Guid = a.RequestGuid AND r.CompanyID = a.CompanyID
LEFT JOIN tbl_JournalVoucherTypes t ON t.id = r.DocumentTypeID
LEFT JOIN tbl_employee su ON su.ID = r.SubmittedByUserId
WHERE a.CompanyID = @CompanyID
  AND a.ActionByUserId = @UserID
  AND a.ActionType IN (1, 2)
ORDER BY a.ActionDate DESC", sql.CreateDataBaseConnectionString(companyId), prm);
        }

        public DataTable SelectAssignmentHistoryForUser(int userId, int companyId, int topN = 200)
        {
            new clsDataBaseVersion().EnsureApprovalWorkflowSchema(companyId);
            clsSQL sql = new clsSQL();
            SqlParameter[] prm =
            {
                new SqlParameter("@UserID", SqlDbType.Int) { Value = userId },
                new SqlParameter("@CompanyID", SqlDbType.Int) { Value = companyId },
                new SqlParameter("@TopN", SqlDbType.Int) { Value = topN },
            };

            return sql.ExecuteQueryStatement(@"
SELECT TOP (@TopN)
       r.Guid AS RequestGuid,
       r.DocumentTypeID,
       r.DocumentGuid,
       r.DocumentNumber,
       r.CurrentLevel,
       r.Status,
       r.TotalAmount,
       r.SubmittedDate,
       r.FinalApprovedDate,
       ISNULL(t.AName, N'') AS DocumentTypeAName,
       ISNULL(t.EName, N'') AS DocumentTypeEName,
       ISNULL(su.AName, su.EName) AS SubmittedByUserName
FROM tbl_ApprovalRequest r
INNER JOIN tbl_ApprovalPolicyLevel pl
    ON pl.PolicyID = r.PolicyID AND pl.CompanyID = r.CompanyID
INNER JOIN tbl_ApprovalPolicyLevelMember m
    ON m.PolicyLevelID = pl.ID AND m.CompanyID = pl.CompanyID
LEFT JOIN tbl_JournalVoucherTypes t ON t.id = r.DocumentTypeID
LEFT JOIN tbl_employee su ON su.ID = r.SubmittedByUserId
WHERE r.CompanyID = @CompanyID
  AND m.ApproverUserID = @UserID
  AND r.Status <> 0
ORDER BY COALESCE(r.FinalApprovedDate, r.SubmittedDate) DESC", sql.CreateDataBaseConnectionString(companyId), prm);
        }

        public DataTable SelectActions(string documentGuid, int companyId)
        {
            new clsDataBaseVersion().EnsureApprovalWorkflowSchema(companyId);
            clsSQL sql = new clsSQL();
            SqlParameter[] prm =
            {
                new SqlParameter("@DocumentGuid", SqlDbType.UniqueIdentifier) { Value = Simulate.Guid(documentGuid) },
                new SqlParameter("@CompanyID", SqlDbType.Int) { Value = companyId },
            };

            return sql.ExecuteQueryStatement(@"
SELECT a.LevelNo,
       ISNULL(a.LevelName, N'') AS LevelName,
       a.ActionType,
       CASE a.ActionType
            WHEN 0 THEN N'Submit'
            WHEN 1 THEN N'Approve'
            WHEN 2 THEN N'Reject'
            WHEN 3 THEN N'Cancel'
            ELSE N'Action'
       END AS ActionName,
       a.ActionByUserId,
       ISNULL(u.AName, u.EName) AS ActionByUserName,
       a.ActionDate,
       ISNULL(a.Comments, N'') AS Comments
FROM tbl_ApprovalAction a
LEFT JOIN tbl_employee u ON u.ID = a.ActionByUserId
WHERE a.DocumentGuid = @DocumentGuid AND a.CompanyID = @CompanyID
ORDER BY a.ActionDate, a.LevelNo", sql.CreateDataBaseConnectionString(companyId), prm);
        }

        public DataTable SelectByGuid(string requestGuid, int companyId)
        {
            clsSQL sql = new clsSQL();
            SqlParameter[] prm =
            {
                new SqlParameter("@Guid", SqlDbType.UniqueIdentifier) { Value = Simulate.Guid(requestGuid) },
                new SqlParameter("@CompanyID", SqlDbType.Int) { Value = companyId },
            };

            return sql.ExecuteQueryStatement(
                "SELECT * FROM tbl_ApprovalRequest WHERE Guid = @Guid AND CompanyID = @CompanyID",
                sql.CreateDataBaseConnectionString(companyId), prm);
        }

        public string InsertRequest(
            int policyId,
            int documentTypeId,
            string documentGuid,
            string documentNumber,
            decimal totalAmount,
            int branchId,
            int submittedByUserId,
            string comments,
            int companyId,
            int initialLevel,
            SqlTransaction trn)
        {
            clsSQL sql = new clsSQL();
            SqlParameter[] prm =
            {
                new SqlParameter("@PolicyID", SqlDbType.Int) { Value = policyId },
                new SqlParameter("@DocumentTypeID", SqlDbType.Int) { Value = documentTypeId },
                new SqlParameter("@DocumentGuid", SqlDbType.UniqueIdentifier) { Value = Simulate.Guid(documentGuid) },
                new SqlParameter("@DocumentNumber", SqlDbType.NVarChar, 100) { Value = documentNumber ?? "" },
                new SqlParameter("@CurrentLevel", SqlDbType.Int) { Value = initialLevel },
                new SqlParameter("@TotalAmount", SqlDbType.Decimal) { Value = totalAmount },
                new SqlParameter("@BranchID", SqlDbType.Int) { Value = branchId },
                new SqlParameter("@SubmittedByUserId", SqlDbType.Int) { Value = submittedByUserId },
                new SqlParameter("@Comments", SqlDbType.NVarChar, 500) { Value = comments ?? "" },
                new SqlParameter("@CompanyID", SqlDbType.Int) { Value = companyId },
            };

            return Simulate.String(sql.ExecuteScalar(@"
INSERT INTO tbl_ApprovalRequest
    (Guid, PolicyID, DocumentTypeID, DocumentGuid, DocumentNumber, CurrentLevel, Status,
     TotalAmount, BranchID, SubmittedByUserId, SubmittedDate, Comments, CompanyID, CreationDate)
OUTPUT INSERTED.Guid
VALUES
    (NEWID(), @PolicyID, @DocumentTypeID, @DocumentGuid, @DocumentNumber, @CurrentLevel, 0,
     @TotalAmount, @BranchID, @SubmittedByUserId, GETDATE(), @Comments, @CompanyID, GETDATE())",
                prm, sql.CreateDataBaseConnectionString(companyId), trn));
        }

        public void InsertAction(
            string requestGuid,
            string documentGuid,
            int levelNo,
            string levelName,
            int actionType,
            int actionByUserId,
            string comments,
            int companyId,
            SqlTransaction trn)
        {
            clsSQL sql = new clsSQL();
            SqlParameter[] prm =
            {
                new SqlParameter("@RequestGuid", SqlDbType.UniqueIdentifier) { Value = Simulate.Guid(requestGuid) },
                new SqlParameter("@DocumentGuid", SqlDbType.UniqueIdentifier) { Value = Simulate.Guid(documentGuid) },
                new SqlParameter("@LevelNo", SqlDbType.Int) { Value = levelNo },
                new SqlParameter("@LevelName", SqlDbType.NVarChar, 200) { Value = levelName ?? "" },
                new SqlParameter("@ActionType", SqlDbType.Int) { Value = actionType },
                new SqlParameter("@ActionByUserId", SqlDbType.Int) { Value = actionByUserId },
                new SqlParameter("@Comments", SqlDbType.NVarChar, 500) { Value = comments ?? "" },
                new SqlParameter("@CompanyID", SqlDbType.Int) { Value = companyId },
            };

            sql.ExecuteNonQueryStatement(@"
INSERT INTO tbl_ApprovalAction
    (RequestGuid, DocumentGuid, LevelNo, LevelName, ActionType, ActionByUserId, ActionDate, Comments, CompanyID)
VALUES
    (@RequestGuid, @DocumentGuid, @LevelNo, @LevelName, @ActionType, @ActionByUserId, GETDATE(), @Comments, @CompanyID)",
                sql.CreateDataBaseConnectionString(companyId), prm, trn);
        }

        public void UpdateRequestProgress(
            string requestGuid,
            int currentLevel,
            int status,
            int finalApprovedByUserId,
            int companyId,
            SqlTransaction trn)
        {
            clsSQL sql = new clsSQL();
            SqlParameter[] prm =
            {
                new SqlParameter("@Guid", SqlDbType.UniqueIdentifier) { Value = Simulate.Guid(requestGuid) },
                new SqlParameter("@CurrentLevel", SqlDbType.Int) { Value = currentLevel },
                new SqlParameter("@Status", SqlDbType.Int) { Value = status },
                new SqlParameter("@FinalApprovedByUserId", SqlDbType.Int) { Value = finalApprovedByUserId },
                new SqlParameter("@CompanyID", SqlDbType.Int) { Value = companyId },
            };

            sql.ExecuteNonQueryStatement(@"
UPDATE tbl_ApprovalRequest SET
    CurrentLevel = @CurrentLevel,
    Status = @Status,
    FinalApprovedByUserId = CASE WHEN @Status = 1 THEN @FinalApprovedByUserId ELSE FinalApprovedByUserId END,
    FinalApprovedDate = CASE WHEN @Status = 1 THEN GETDATE() ELSE FinalApprovedDate END
WHERE Guid = @Guid AND CompanyID = @CompanyID",
                sql.CreateDataBaseConnectionString(companyId), prm, trn);
        }

        public int CountDistinctApprovalsAtLevel(string requestGuid, int levelNo, int companyId, SqlTransaction trn)
        {
            clsSQL sql = new clsSQL();
            SqlParameter[] prm =
            {
                new SqlParameter("@RequestGuid", SqlDbType.UniqueIdentifier) { Value = Simulate.Guid(requestGuid) },
                new SqlParameter("@LevelNo", SqlDbType.Int) { Value = levelNo },
                new SqlParameter("@CompanyID", SqlDbType.Int) { Value = companyId },
            };

            return Simulate.Integer32(sql.ExecuteScalar(@"
SELECT COUNT(DISTINCT a.ActionByUserId)
FROM tbl_ApprovalAction a
WHERE a.RequestGuid = @RequestGuid
  AND a.CompanyID = @CompanyID
  AND a.LevelNo = @LevelNo
  AND a.ActionType = 1", prm, sql.CreateDataBaseConnectionString(companyId), trn));
        }

        public bool UserAlreadyApprovedAtLevel(string requestGuid, int levelNo, int userId, int companyId, SqlTransaction trn)
        {
            clsSQL sql = new clsSQL();
            SqlParameter[] prm =
            {
                new SqlParameter("@RequestGuid", SqlDbType.UniqueIdentifier) { Value = Simulate.Guid(requestGuid) },
                new SqlParameter("@LevelNo", SqlDbType.Int) { Value = levelNo },
                new SqlParameter("@UserID", SqlDbType.Int) { Value = userId },
                new SqlParameter("@CompanyID", SqlDbType.Int) { Value = companyId },
            };

            return Simulate.Integer32(sql.ExecuteScalar(@"
SELECT COUNT(*)
FROM tbl_ApprovalAction a
WHERE a.RequestGuid = @RequestGuid
  AND a.CompanyID = @CompanyID
  AND a.LevelNo = @LevelNo
  AND a.ActionType = 1
  AND a.ActionByUserId = @UserID", prm, sql.CreateDataBaseConnectionString(companyId), trn)) > 0;
        }
    }
}
