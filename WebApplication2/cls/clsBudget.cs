using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using WebApplication2.MainClasses;
using static WebApplication2.MainClasses.clsEnum;

namespace WebApplication2.cls
{
    public class clsBudget
    {
        public const int TypeBudget = 34;

        public DataTable SelectHeaders(int id, int companyId, int fiscalYear, int documentStatus, string guid)
        {
            clsSQL sql = new clsSQL();
            SqlParameter[] prm =
            {
                new SqlParameter("@ID", SqlDbType.Int) { Value = id },
                new SqlParameter("@CompanyID", SqlDbType.Int) { Value = companyId },
                new SqlParameter("@FiscalYear", SqlDbType.Int) { Value = fiscalYear },
                new SqlParameter("@DocumentStatus", SqlDbType.Int) { Value = documentStatus },
                new SqlParameter("@Guid", SqlDbType.UniqueIdentifier)
                {
                    Value = string.IsNullOrWhiteSpace(guid) || guid == "00000000-0000-0000-0000-000000000000"
                        ? Guid.Empty
                        : Simulate.Guid(guid)
                },
            };

            return sql.ExecuteQueryStatement(@"
SELECT h.*,
       ISNULL((SELECT SUM(l.Amount) FROM tbl_BudgetLine l WHERE l.BudgetHeaderID = h.ID), 0) AS TotalAmount,
       ISNULL((SELECT COUNT(*) FROM tbl_BudgetLine l WHERE l.BudgetHeaderID = h.ID), 0) AS LineCount
FROM tbl_BudgetHeader h
WHERE h.CompanyID = @CompanyID
  AND (@ID = 0 OR h.ID = @ID)
  AND (@FiscalYear = 0 OR h.FiscalYear = @FiscalYear)
  AND (@DocumentStatus < 0 OR h.DocumentStatus = @DocumentStatus)
  AND (@Guid = '00000000-0000-0000-0000-000000000000' OR h.Guid = @Guid)
ORDER BY h.FiscalYear DESC, h.ID DESC",
                sql.CreateDataBaseConnectionString(companyId), prm);
        }

        public DataTable SelectLines(int budgetHeaderId, int companyId)
        {
            clsSQL sql = new clsSQL();
            SqlParameter[] prm =
            {
                new SqlParameter("@BudgetHeaderID", SqlDbType.Int) { Value = budgetHeaderId },
                new SqlParameter("@CompanyID", SqlDbType.Int) { Value = companyId },
            };

            return sql.ExecuteQueryStatement(@"
SELECT l.*,
       ISNULL(a.AccountNumber, '') AS AccountNumber,
       ISNULL(a.AName, '') AS AccountAName,
       ISNULL(a.EName, '') AS AccountEName,
       ISNULL(cc.AName, '') AS CostCenterAName,
       ISNULL(cc.EName, '') AS CostCenterEName,
       ISNULL(b.AName, '') AS BranchAName,
       ISNULL(b.EName, '') AS BranchEName
FROM tbl_BudgetLine l
LEFT JOIN tbl_Accounts a ON a.ID = l.AccountID
LEFT JOIN tbl_CostCenter cc ON cc.ID = l.CostCenterID
LEFT JOIN tbl_Branch b ON b.ID = l.BranchID
WHERE l.BudgetHeaderID = @BudgetHeaderID AND l.CompanyID = @CompanyID
ORDER BY l.AccountID, l.CostCenterID, l.BranchID, l.Month",
                sql.CreateDataBaseConnectionString(companyId), prm);
        }

        public int InsertHeader(int fiscalYear, string aName, string eName, int branchId, string notes,
            int companyId, int creationUserId, int documentStatus = 0)
        {
            clsSQL sql = new clsSQL();
            Guid g = Guid.NewGuid();
            SqlParameter[] prm =
            {
                new SqlParameter("@Guid", SqlDbType.UniqueIdentifier) { Value = g },
                new SqlParameter("@FiscalYear", SqlDbType.Int) { Value = fiscalYear },
                new SqlParameter("@AName", SqlDbType.NVarChar) { Value = Simulate.String(aName) },
                new SqlParameter("@EName", SqlDbType.NVarChar) { Value = Simulate.String(eName) },
                new SqlParameter("@BranchID", SqlDbType.Int) { Value = branchId },
                new SqlParameter("@DocumentStatus", SqlDbType.Int) { Value = documentStatus },
                new SqlParameter("@Notes", SqlDbType.NVarChar) { Value = Simulate.String(notes) },
                new SqlParameter("@CompanyID", SqlDbType.Int) { Value = companyId },
                new SqlParameter("@CreationUserID", SqlDbType.Int) { Value = creationUserId },
            };

            object id = sql.ExecuteScalar(@"
INSERT INTO tbl_BudgetHeader
 (Guid, FiscalYear, AName, EName, BranchID, DocumentStatus, Notes, CompanyID, CreationUserID, CreationDate)
VALUES
 (@Guid, @FiscalYear, @AName, @EName, @BranchID, @DocumentStatus, @Notes, @CompanyID, @CreationUserID, GETDATE());
SELECT CAST(SCOPE_IDENTITY() AS INT);",
                prm, sql.CreateDataBaseConnectionString(companyId), null);
            return Simulate.Integer32(id);
        }

        public bool UpdateHeader(int id, int fiscalYear, string aName, string eName, int branchId, string notes,
            int companyId, int modificationUserId)
        {
            EnsureEditable(id, companyId);
            clsSQL sql = new clsSQL();
            SqlParameter[] prm =
            {
                new SqlParameter("@ID", SqlDbType.Int) { Value = id },
                new SqlParameter("@FiscalYear", SqlDbType.Int) { Value = fiscalYear },
                new SqlParameter("@AName", SqlDbType.NVarChar) { Value = Simulate.String(aName) },
                new SqlParameter("@EName", SqlDbType.NVarChar) { Value = Simulate.String(eName) },
                new SqlParameter("@BranchID", SqlDbType.Int) { Value = branchId },
                new SqlParameter("@Notes", SqlDbType.NVarChar) { Value = Simulate.String(notes) },
                new SqlParameter("@CompanyID", SqlDbType.Int) { Value = companyId },
                new SqlParameter("@UserID", SqlDbType.Int) { Value = modificationUserId },
            };

            int n = sql.ExecuteNonQueryStatement(@"
UPDATE tbl_BudgetHeader
SET FiscalYear = @FiscalYear, AName = @AName, EName = @EName, BranchID = @BranchID, Notes = @Notes,
    ModificationUserID = @UserID, ModificationDate = GETDATE()
WHERE ID = @ID AND CompanyID = @CompanyID
  AND DocumentStatus IN (0, 3)",
                sql.CreateDataBaseConnectionString(companyId), prm);
            return n > 0;
        }

        public bool DeleteHeader(int id, int companyId)
        {
            EnsureEditable(id, companyId);
            clsSQL sql = new clsSQL();
            SqlParameter[] prm =
            {
                new SqlParameter("@ID", SqlDbType.Int) { Value = id },
                new SqlParameter("@CompanyID", SqlDbType.Int) { Value = companyId },
            };
            string con = sql.CreateDataBaseConnectionString(companyId);
            sql.ExecuteNonQueryStatement(
                "DELETE FROM tbl_BudgetLine WHERE BudgetHeaderID = @ID AND CompanyID = @CompanyID", con, prm);
            int n = sql.ExecuteNonQueryStatement(
                "DELETE FROM tbl_BudgetHeader WHERE ID = @ID AND CompanyID = @CompanyID AND DocumentStatus IN (0, 3)",
                con, prm);
            return n > 0;
        }

        public bool ReplaceLines(int budgetHeaderId, int companyId, int userId, List<BudgetLineDto> lines)
        {
            EnsureEditable(budgetHeaderId, companyId);
            clsSQL sql = new clsSQL();
            string con = sql.CreateDataBaseConnectionString(companyId);
            SqlConnection connection = new SqlConnection(con);
            connection.Open();
            SqlTransaction trn = connection.BeginTransaction();
            try
            {
                SqlParameter[] del =
                {
                    new SqlParameter("@BudgetHeaderID", SqlDbType.Int) { Value = budgetHeaderId },
                    new SqlParameter("@CompanyID", SqlDbType.Int) { Value = companyId },
                };
                sql.ExecuteNonQueryStatement(
                    "DELETE FROM tbl_BudgetLine WHERE BudgetHeaderID = @BudgetHeaderID AND CompanyID = @CompanyID",
                    con, del, trn);

                if (lines != null)
                {
                    foreach (var line in lines)
                    {
                        if (line.AccountID <= 0 || line.Month < 1 || line.Month > 12) continue;
                        SqlParameter[] ins =
                        {
                            new SqlParameter("@BudgetHeaderID", SqlDbType.Int) { Value = budgetHeaderId },
                            new SqlParameter("@AccountID", SqlDbType.Int) { Value = line.AccountID },
                            new SqlParameter("@CostCenterID", SqlDbType.Int) { Value = line.CostCenterID },
                            new SqlParameter("@BranchID", SqlDbType.Int) { Value = line.BranchID },
                            new SqlParameter("@Month", SqlDbType.Int) { Value = line.Month },
                            new SqlParameter("@Amount", SqlDbType.Decimal) { Value = line.Amount },
                            new SqlParameter("@CompanyID", SqlDbType.Int) { Value = companyId },
                            new SqlParameter("@CreationUserID", SqlDbType.Int) { Value = userId },
                        };
                        sql.ExecuteNonQueryStatement(@"
INSERT INTO tbl_BudgetLine
 (BudgetHeaderID, AccountID, CostCenterID, BranchID, Month, Amount, CompanyID, CreationUserID, CreationDate)
VALUES
 (@BudgetHeaderID, @AccountID, @CostCenterID, @BranchID, @Month, @Amount, @CompanyID, @CreationUserID, GETDATE())",
                            con, ins, trn);
                    }
                }

                trn.Commit();
                return true;
            }
            catch
            {
                trn.Rollback();
                throw;
            }
            finally
            {
                connection.Close();
            }
        }

        public string GetGuidById(int id, int companyId)
        {
            clsSQL sql = new clsSQL();
            object o = sql.ExecuteScalar(
                "SELECT CAST(Guid AS NVARCHAR(50)) FROM tbl_BudgetHeader WHERE ID = @ID AND CompanyID = @CompanyID",
                new SqlParameter[]
                {
                    new SqlParameter("@ID", SqlDbType.Int) { Value = id },
                    new SqlParameter("@CompanyID", SqlDbType.Int) { Value = companyId },
                },
                sql.CreateDataBaseConnectionString(companyId), null);
            return Simulate.String(o);
        }

        public int GetStatusByGuid(string guid, int companyId, SqlTransaction trn = null)
        {
            clsSQL sql = new clsSQL();
            object o = sql.ExecuteScalar(
                "SELECT ISNULL(DocumentStatus,0) FROM tbl_BudgetHeader WHERE Guid = @Guid AND CompanyID = @CompanyID",
                new SqlParameter[]
                {
                    new SqlParameter("@Guid", SqlDbType.UniqueIdentifier) { Value = Simulate.Guid(guid) },
                    new SqlParameter("@CompanyID", SqlDbType.Int) { Value = companyId },
                },
                sql.CreateDataBaseConnectionString(companyId), trn);
            return Simulate.Integer32(o);
        }

        public static bool TryGetDocumentMeta(
            string documentGuid, int companyId, SqlTransaction trn,
            out int branchId, out decimal amount, out string documentNumber, out int currentStatus, out int submittedBy)
        {
            branchId = 0;
            amount = 0;
            documentNumber = "";
            currentStatus = (int)DocumentStatus.Draft;
            submittedBy = 0;

            if (string.IsNullOrWhiteSpace(documentGuid)) return false;

            clsSQL sql = new clsSQL();
            DataTable dt = sql.ExecuteQueryStatement(@"
SELECT ISNULL(BranchID,0) AS BranchID,
       ISNULL((SELECT SUM(Amount) FROM tbl_BudgetLine WHERE BudgetHeaderID = h.ID),0) AS Amount,
       ISNULL(CAST(FiscalYear AS NVARCHAR(20)),'') AS DocumentNumber,
       ISNULL(DocumentStatus,0) AS DocumentStatus,
       ISNULL(CreationUserID,0) AS CreationUserID
FROM tbl_BudgetHeader h
WHERE Guid = @Guid AND CompanyID = @CompanyID",
                sql.CreateDataBaseConnectionString(companyId),
                new SqlParameter[]
                {
                    new SqlParameter("@Guid", SqlDbType.UniqueIdentifier) { Value = Simulate.Guid(documentGuid) },
                    new SqlParameter("@CompanyID", SqlDbType.Int) { Value = companyId },
                },
                trn);

            if (dt == null || dt.Rows.Count == 0) return false;
            DataRow row = dt.Rows[0];
            branchId = Simulate.Integer32(row["BranchID"]);
            amount = Simulate.Decimal(row["Amount"]);
            documentNumber = Simulate.String(row["DocumentNumber"]);
            currentStatus = Simulate.Integer32(row["DocumentStatus"]);
            submittedBy = Simulate.Integer32(row["CreationUserID"]);
            return true;
        }

        public static void SetDocumentStatus(string documentGuid, int status, int userId, int companyId, SqlTransaction trn)
        {
            clsSQL sql = new clsSQL();
            sql.ExecuteNonQueryStatement(@"
UPDATE tbl_BudgetHeader
SET DocumentStatus = @DocumentStatus,
    PostedDate = CASE WHEN @DocumentStatus = 2 THEN GETDATE() ELSE PostedDate END,
    PostedByUserId = CASE WHEN @DocumentStatus = 2 THEN @UserId ELSE PostedByUserId END,
    SubmittedByUserId = CASE WHEN @DocumentStatus = 1 THEN @UserId ELSE SubmittedByUserId END,
    SubmittedDate = CASE WHEN @DocumentStatus = 1 THEN GETDATE() ELSE SubmittedDate END,
    ModificationUserID = @UserId,
    ModificationDate = GETDATE()
WHERE Guid = @Guid AND CompanyID = @CompanyID",
                sql.CreateDataBaseConnectionString(companyId),
                new SqlParameter[]
                {
                    new SqlParameter("@Guid", SqlDbType.UniqueIdentifier) { Value = Simulate.Guid(documentGuid) },
                    new SqlParameter("@DocumentStatus", SqlDbType.Int) { Value = status },
                    new SqlParameter("@UserId", SqlDbType.Int) { Value = userId },
                    new SqlParameter("@CompanyID", SqlDbType.Int) { Value = companyId },
                },
                trn);
        }

        /// <summary>Activate budget on final approval: cancel other active budgets for same year.</summary>
        public static bool PostDocument(string documentGuid, int userId, int companyId, SqlTransaction trn)
        {
            clsSQL sql = new clsSQL();
            string con = sql.CreateDataBaseConnectionString(companyId);
            DataTable dt = sql.ExecuteQueryStatement(@"
SELECT ID, FiscalYear FROM tbl_BudgetHeader WHERE Guid = @Guid AND CompanyID = @CompanyID",
                con,
                new SqlParameter[]
                {
                    new SqlParameter("@Guid", SqlDbType.UniqueIdentifier) { Value = Simulate.Guid(documentGuid) },
                    new SqlParameter("@CompanyID", SqlDbType.Int) { Value = companyId },
                },
                trn);
            if (dt == null || dt.Rows.Count == 0) return false;

            int fiscalYear = Simulate.Integer32(dt.Rows[0]["FiscalYear"]);
            sql.ExecuteNonQueryStatement(@"
UPDATE tbl_BudgetHeader
SET DocumentStatus = 4,
    ModificationUserID = @UserId,
    ModificationDate = GETDATE()
WHERE CompanyID = @CompanyID AND FiscalYear = @FiscalYear AND DocumentStatus = 2
  AND Guid <> @Guid",
                con,
                new SqlParameter[]
                {
                    new SqlParameter("@Guid", SqlDbType.UniqueIdentifier) { Value = Simulate.Guid(documentGuid) },
                    new SqlParameter("@CompanyID", SqlDbType.Int) { Value = companyId },
                    new SqlParameter("@FiscalYear", SqlDbType.Int) { Value = fiscalYear },
                    new SqlParameter("@UserId", SqlDbType.Int) { Value = userId },
                },
                trn);

            SetDocumentStatus(documentGuid, (int)DocumentStatus.Posted, userId, companyId, trn);
            return true;
        }

        public DataTable GetSettings(int companyId)
        {
            clsSQL sql = new clsSQL();
            return sql.ExecuteQueryStatement(@"
SELECT * FROM tbl_BudgetSettings WHERE CompanyID = @CompanyID",
                sql.CreateDataBaseConnectionString(companyId),
                new SqlParameter[] { new SqlParameter("@CompanyID", SqlDbType.Int) { Value = companyId } });
        }

        public bool IsControlEnabled(int companyId)
        {
            clsSQL sql = new clsSQL();
            object o = sql.ExecuteScalar(
                "SELECT ISNULL(IsEnabled,0) FROM tbl_BudgetSettings WHERE CompanyID = @CompanyID",
                new SqlParameter[] { new SqlParameter("@CompanyID", SqlDbType.Int) { Value = companyId } },
                sql.CreateDataBaseConnectionString(companyId), null);
            // Default enabled when settings row missing (schema seed creates it)
            if (o == null || o == DBNull.Value) return true;
            return Simulate.Bool(o);
        }

        public bool SaveSettings(int companyId, bool isEnabled, int userId)
        {
            clsSQL sql = new clsSQL();
            int n = sql.ExecuteNonQueryStatement(@"
IF EXISTS (SELECT 1 FROM tbl_BudgetSettings WHERE CompanyID = @CompanyID)
    UPDATE tbl_BudgetSettings SET IsEnabled = @IsEnabled, ModificationUserID = @UserID, ModificationDate = GETDATE()
    WHERE CompanyID = @CompanyID;
ELSE
    INSERT INTO tbl_BudgetSettings (CompanyID, IsEnabled, ModificationUserID, ModificationDate)
    VALUES (@CompanyID, @IsEnabled, @UserID, GETDATE());",
                sql.CreateDataBaseConnectionString(companyId),
                new SqlParameter[]
                {
                    new SqlParameter("@CompanyID", SqlDbType.Int) { Value = companyId },
                    new SqlParameter("@IsEnabled", SqlDbType.Bit) { Value = isEnabled },
                    new SqlParameter("@UserID", SqlDbType.Int) { Value = userId },
                });
            return n > 0;
        }

        public DataTable SelectOverrideLog(int companyId, int year, int documentTypeId)
        {
            clsSQL sql = new clsSQL();
            return sql.ExecuteQueryStatement(@"
SELECT o.*,
       ISNULL(a.AccountNumber,'') AS AccountNumber,
       ISNULL(a.AName,'') AS AccountAName,
       ISNULL(cc.AName,'') AS CostCenterAName,
       ISNULL(b.AName,'') AS BranchAName
FROM tbl_BudgetOverrideLog o
LEFT JOIN tbl_Accounts a ON a.ID = o.AccountID
LEFT JOIN tbl_CostCenter cc ON cc.ID = o.CostCenterID
LEFT JOIN tbl_Branch b ON b.ID = o.BranchID
WHERE o.CompanyID = @CompanyID
  AND (@Year = 0 OR o.Year = @Year)
  AND (@DocumentTypeId = 0 OR o.DocumentTypeId = @DocumentTypeId)
ORDER BY o.RequestedAt DESC",
                sql.CreateDataBaseConnectionString(companyId),
                new SqlParameter[]
                {
                    new SqlParameter("@CompanyID", SqlDbType.Int) { Value = companyId },
                    new SqlParameter("@Year", SqlDbType.Int) { Value = year },
                    new SqlParameter("@DocumentTypeId", SqlDbType.Int) { Value = documentTypeId },
                });
        }

        public void SetBudgetOverride(string tableName, string documentGuid, int companyId, bool flag, string reason, SqlTransaction trn = null)
        {
            if (string.IsNullOrWhiteSpace(tableName) || string.IsNullOrWhiteSpace(documentGuid)) return;
            clsSQL sql = new clsSQL();
            sql.ExecuteNonQueryStatement($@"
UPDATE {tableName}
SET BudgetOverrideFlag = @Flag, BudgetOverrideReason = @Reason
WHERE Guid = @Guid AND CompanyID = @CompanyID",
                sql.CreateDataBaseConnectionString(companyId),
                new SqlParameter[]
                {
                    new SqlParameter("@Flag", SqlDbType.Bit) { Value = flag },
                    new SqlParameter("@Reason", SqlDbType.NVarChar) { Value = Simulate.String(reason) },
                    new SqlParameter("@Guid", SqlDbType.UniqueIdentifier) { Value = Simulate.Guid(documentGuid) },
                    new SqlParameter("@CompanyID", SqlDbType.Int) { Value = companyId },
                },
                trn);
        }

        public void InsertOverrideLogRows(int companyId, int documentTypeId, string documentGuid, string documentNumber,
            string overrideReason, int userId, int? approvalRequestId, List<BudgetBreach> breaches, SqlTransaction trn = null)
        {
            if (breaches == null || breaches.Count == 0) return;
            clsSQL sql = new clsSQL();
            string con = sql.CreateDataBaseConnectionString(companyId);
            foreach (var b in breaches)
            {
                sql.ExecuteNonQueryStatement(@"
INSERT INTO tbl_BudgetOverrideLog
 (CompanyID, DocumentTypeId, DocumentGuid, DocumentNumber, AccountID, CostCenterID, BranchID, Year, Month,
  BudgetAmount, ActualBefore, RequestedAmount, OverAmount, OverrideReason, RequestedByUserID, RequestedAt, ApprovalRequestID)
VALUES
 (@CompanyID, @DocumentTypeId, @DocumentGuid, @DocumentNumber, @AccountID, @CostCenterID, @BranchID, @Year, @Month,
  @BudgetAmount, @ActualBefore, @RequestedAmount, @OverAmount, @OverrideReason, @UserID, GETDATE(), @ApprovalRequestID)",
                    con,
                    new SqlParameter[]
                    {
                        new SqlParameter("@CompanyID", SqlDbType.Int) { Value = companyId },
                        new SqlParameter("@DocumentTypeId", SqlDbType.Int) { Value = documentTypeId },
                        new SqlParameter("@DocumentGuid", SqlDbType.UniqueIdentifier) { Value = Simulate.Guid(documentGuid) },
                        new SqlParameter("@DocumentNumber", SqlDbType.NVarChar) { Value = Simulate.String(documentNumber) },
                        new SqlParameter("@AccountID", SqlDbType.Int) { Value = b.AccountID },
                        new SqlParameter("@CostCenterID", SqlDbType.Int) { Value = b.CostCenterID },
                        new SqlParameter("@BranchID", SqlDbType.Int) { Value = b.BranchID },
                        new SqlParameter("@Year", SqlDbType.Int) { Value = b.Year },
                        new SqlParameter("@Month", SqlDbType.Int) { Value = b.Month },
                        new SqlParameter("@BudgetAmount", SqlDbType.Decimal) { Value = b.BudgetAmount },
                        new SqlParameter("@ActualBefore", SqlDbType.Decimal) { Value = b.ActualBefore },
                        new SqlParameter("@RequestedAmount", SqlDbType.Decimal) { Value = b.RequestedAmount },
                        new SqlParameter("@OverAmount", SqlDbType.Decimal) { Value = b.OverAmount },
                        new SqlParameter("@OverrideReason", SqlDbType.NVarChar) { Value = Simulate.String(overrideReason) },
                        new SqlParameter("@UserID", SqlDbType.Int) { Value = userId },
                        new SqlParameter("@ApprovalRequestID", SqlDbType.Int)
                        {
                            Value = approvalRequestId.HasValue ? (object)approvalRequestId.Value : DBNull.Value
                        },
                    },
                    trn);
            }
        }

        public void FinalizeOverrideLog(string documentGuid, int companyId, int finalDecision, int decidedBy, SqlTransaction trn = null)
        {
            clsSQL sql = new clsSQL();
            sql.ExecuteNonQueryStatement(@"
UPDATE tbl_BudgetOverrideLog
SET FinalDecision = @FinalDecision, DecidedBy = @DecidedBy, DecidedAt = GETDATE()
WHERE DocumentGuid = @DocumentGuid AND CompanyID = @CompanyID AND FinalDecision IS NULL",
                sql.CreateDataBaseConnectionString(companyId),
                new SqlParameter[]
                {
                    new SqlParameter("@DocumentGuid", SqlDbType.UniqueIdentifier) { Value = Simulate.Guid(documentGuid) },
                    new SqlParameter("@CompanyID", SqlDbType.Int) { Value = companyId },
                    new SqlParameter("@FinalDecision", SqlDbType.Int) { Value = finalDecision },
                    new SqlParameter("@DecidedBy", SqlDbType.Int) { Value = decidedBy },
                },
                trn);
        }

        /// <summary>
        /// Sets override flag, submits for approval, logs breaches.
        /// Returns null on success; error payload on failure (cancels document).
        /// </summary>
        public string CompleteBudgetOverride(
            string tableName,
            int companyId,
            int userId,
            int documentTypeId,
            string documentGuid,
            string documentNumber,
            string overrideReason,
            List<BudgetBreach> breaches)
        {
            if (string.IsNullOrWhiteSpace(documentGuid)) return "BUDGET_OVERRIDE_FAILED:Missing document";

            SetBudgetOverride(tableName, documentGuid, companyId, true, overrideReason);
            var submit = new clsApprovalEngine().Submit(new ApprovalSubmitRequest
            {
                CompanyID = companyId,
                UserID = userId,
                DocumentTypeID = documentTypeId,
                DocumentGuid = documentGuid,
                Comments = "Budget override: " + overrideReason,
            });

            if (submit == null || !submit.Success)
            {
                CancelDocument(tableName, documentGuid, companyId, userId);
                string msg = submit == null ? "Submit failed" : Simulate.String(submit.Message);
                return "BUDGET_OVERRIDE_FAILED:" + msg;
            }

            InsertOverrideLogRows(companyId, documentTypeId, documentGuid, documentNumber,
                overrideReason, userId, null, breaches);
            return null;
        }

        void CancelDocument(string tableName, string documentGuid, int companyId, int userId)
        {
            if (string.IsNullOrWhiteSpace(tableName)) return;
            clsSQL sql = new clsSQL();
            sql.ExecuteNonQueryStatement($@"
UPDATE {tableName}
SET DocumentStatus = 4,
    BudgetOverrideFlag = 0,
    ModificationUserID = @UserID,
    ModificationDate = GETDATE()
WHERE Guid = @Guid AND CompanyID = @CompanyID",
                sql.CreateDataBaseConnectionString(companyId),
                new SqlParameter[]
                {
                    new SqlParameter("@Guid", SqlDbType.UniqueIdentifier) { Value = Simulate.Guid(documentGuid) },
                    new SqlParameter("@CompanyID", SqlDbType.Int) { Value = companyId },
                    new SqlParameter("@UserID", SqlDbType.Int) { Value = userId },
                });
        }

        void EnsureEditable(int id, int companyId)
        {
            clsSQL sql = new clsSQL();
            object o = sql.ExecuteScalar(
                "SELECT ISNULL(DocumentStatus,0) FROM tbl_BudgetHeader WHERE ID = @ID AND CompanyID = @CompanyID",
                new SqlParameter[]
                {
                    new SqlParameter("@ID", SqlDbType.Int) { Value = id },
                    new SqlParameter("@CompanyID", SqlDbType.Int) { Value = companyId },
                },
                sql.CreateDataBaseConnectionString(companyId), null);
            int status = Simulate.Integer32(o);
            if (status != (int)DocumentStatus.Draft && status != (int)DocumentStatus.Rejected)
                throw new InvalidOperationException("Budget can only be edited when Draft or Rejected.");
        }
    }

    public class BudgetLineDto
    {
        public int AccountID { get; set; }
        public int CostCenterID { get; set; }
        public int BranchID { get; set; }
        public int Month { get; set; }
        public decimal Amount { get; set; }
    }

    public class BudgetBreach
    {
        public int AccountID { get; set; }
        public int CostCenterID { get; set; }
        public int BranchID { get; set; }
        public int Year { get; set; }
        public int Month { get; set; }
        public decimal BudgetAmount { get; set; }
        public decimal ActualBefore { get; set; }
        public decimal RequestedAmount { get; set; }
        public decimal OverAmount { get; set; }
        public string AccountName { get; set; }
    }

    public class BudgetCheckResult
    {
        public bool IsOver { get; set; }
        public bool RequiresOverride { get; set; }
        public bool ControlEnabled { get; set; }
        public string Message { get; set; }
        public List<BudgetBreach> Breaches { get; set; } = new List<BudgetBreach>();
    }

    public class BudgetSpendLine
    {
        public int AccountID { get; set; }
        public int CostCenterID { get; set; }
        public int BranchID { get; set; }
        public decimal Amount { get; set; }
    }
}
