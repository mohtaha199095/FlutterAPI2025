using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using WebApplication2.DataBaseTable;
using WebApplication2.MainClasses;
using static WebApplication2.MainClasses.clsEnum;

namespace WebApplication2.cls
{
    public class clsBudgetControl
    {
        readonly clsBudget _budget = new clsBudget();

        public BudgetCheckResult Evaluate(
            int companyId,
            DateTime voucherDate,
            int headerBranchId,
            int headerCostCenterId,
            IEnumerable<BudgetSpendLine> spendLines,
            string excludeDocumentGuid = null,
            string excludeJvGuid = null)
        {
            var result = new BudgetCheckResult { ControlEnabled = _budget.IsControlEnabled(companyId) };
            if (!result.ControlEnabled)
            {
                result.Message = "Budget control disabled";
                return result;
            }

            if (spendLines == null)
            {
                result.Message = "No lines";
                return result;
            }

            int year = voucherDate.Year;
            int month = voucherDate.Month;

            DataTable active = _budget.SelectHeaders(0, companyId, year, (int)DocumentStatus.Posted, "");
            if (active == null || active.Rows.Count == 0)
            {
                result.Message = "No active budget for year";
                return result;
            }

            int headerId = Simulate.Integer32(active.Rows[0]["ID"]);
            DataTable lines = _budget.SelectLines(headerId, companyId);
            if (lines == null || lines.Rows.Count == 0)
            {
                result.Message = "Active budget has no lines";
                return result;
            }

            var budgetMap = new Dictionary<string, DataRow>();
            foreach (DataRow row in lines.Rows)
            {
                if (Simulate.Integer32(row["Month"]) != month) continue;
                string key = Key(
                    Simulate.Integer32(row["AccountID"]),
                    Simulate.Integer32(row["CostCenterID"]),
                    Simulate.Integer32(row["BranchID"]));
                budgetMap[key] = row;
            }

            var requested = new Dictionary<string, decimal>();
            foreach (var line in spendLines)
            {
                if (line == null || line.AccountID <= 0 || line.Amount <= 0) continue;
                int cc = line.CostCenterID > 0 ? line.CostCenterID : headerCostCenterId;
                int br = line.BranchID > 0 ? line.BranchID : headerBranchId;
                string key = Key(line.AccountID, cc, br);
                if (!budgetMap.ContainsKey(key)) continue; // only budgeted keys
                if (!requested.ContainsKey(key)) requested[key] = 0;
                requested[key] += line.Amount;
            }

            if (requested.Count == 0)
            {
                result.Message = "No budgeted accounts on document";
                return result;
            }

            foreach (var kv in requested)
            {
                DataRow brow = budgetMap[kv.Key];
                int accountId = Simulate.Integer32(brow["AccountID"]);
                int costCenterId = Simulate.Integer32(brow["CostCenterID"]);
                int branchId = Simulate.Integer32(brow["BranchID"]);
                decimal budgetAmount = Simulate.Decimal(brow["Amount"]);
                decimal actual = GetPostedActual(companyId, accountId, costCenterId, branchId, year, month,
                    excludeDocumentGuid, excludeJvGuid);
                decimal over = (actual + kv.Value) - budgetAmount;
                if (over > 0.0001m)
                {
                    result.Breaches.Add(new BudgetBreach
                    {
                        AccountID = accountId,
                        CostCenterID = costCenterId,
                        BranchID = branchId,
                        Year = year,
                        Month = month,
                        BudgetAmount = budgetAmount,
                        ActualBefore = actual,
                        RequestedAmount = kv.Value,
                        OverAmount = over,
                        AccountName = Simulate.String(brow["AccountAName"]),
                    });
                }
            }

            result.IsOver = result.Breaches.Count > 0;
            result.RequiresOverride = result.IsOver;
            result.Message = result.IsOver
                ? "Document exceeds approved budget"
                : "Within budget";
            return result;
        }

        public DataTable GetBudgetVsActual(int companyId, int fiscalYear, int accountId = 0, int costCenterId = 0, int branchId = 0)
        {
            clsSQL sql = new clsSQL();
            return sql.ExecuteQueryStatement(@"
SELECT l.AccountID, l.CostCenterID, l.BranchID, l.Month, l.Amount AS BudgetAmount,
       ISNULL(a.AccountNumber,'') AS AccountNumber,
       ISNULL(a.AName,'') AS AccountAName,
       ISNULL(cc.AName,'') AS CostCenterAName,
       ISNULL(b.AName,'') AS BranchAName,
       ISNULL((
            SELECT SUM(d.Debit)
            FROM tbl_JournalVoucherDetails d
            INNER JOIN tbl_JournalVoucherHeader h ON h.Guid = d.HeaderGuid AND h.CompanyID = d.CompanyID
            WHERE d.CompanyID = @CompanyID
              AND d.AccountID = l.AccountID
              AND ISNULL(d.CostCenterID,0) = l.CostCenterID
              AND ISNULL(d.BranchID,0) = l.BranchID
              AND YEAR(h.VoucherDate) = @FiscalYear
              AND MONTH(h.VoucherDate) = l.Month
              AND ISNULL(h.DocumentStatus,2) = 2
       ), 0) AS ActualAmount
FROM tbl_BudgetLine l
INNER JOIN tbl_BudgetHeader bh ON bh.ID = l.BudgetHeaderID AND bh.CompanyID = l.CompanyID
LEFT JOIN tbl_Accounts a ON a.ID = l.AccountID
LEFT JOIN tbl_CostCenter cc ON cc.ID = l.CostCenterID
LEFT JOIN tbl_Branch b ON b.ID = l.BranchID
WHERE l.CompanyID = @CompanyID
  AND bh.FiscalYear = @FiscalYear
  AND bh.DocumentStatus = 2
  AND (@AccountID = 0 OR l.AccountID = @AccountID)
  AND (@CostCenterID < 0 OR l.CostCenterID = @CostCenterID)
  AND (@BranchID < 0 OR l.BranchID = @BranchID)
ORDER BY l.AccountID, l.CostCenterID, l.BranchID, l.Month",
                sql.CreateDataBaseConnectionString(companyId),
                new SqlParameter[]
                {
                    new SqlParameter("@CompanyID", SqlDbType.Int) { Value = companyId },
                    new SqlParameter("@FiscalYear", SqlDbType.Int) { Value = fiscalYear },
                    new SqlParameter("@AccountID", SqlDbType.Int) { Value = accountId },
                    new SqlParameter("@CostCenterID", SqlDbType.Int) { Value = costCenterId },
                    new SqlParameter("@BranchID", SqlDbType.Int) { Value = branchId },
                });
        }

        public static List<BudgetSpendLine> FromCashDetails(
            IEnumerable<DBCashVoucherDetails> details, int headerBranchId, int headerCostCenterId)
        {
            var list = new List<BudgetSpendLine>();
            if (details == null) return list;
            foreach (var d in details)
            {
                if (d == null || d.AccountID <= 0) continue;
                decimal amt = d.Debit > 0 ? d.Debit : d.Credit;
                if (amt <= 0) continue;
                list.Add(new BudgetSpendLine
                {
                    AccountID = d.AccountID,
                    CostCenterID = d.CostCenterID > 0 ? d.CostCenterID : headerCostCenterId,
                    BranchID = d.BranchID > 0 ? d.BranchID : headerBranchId,
                    Amount = amt,
                });
            }
            return list;
        }

        public static List<BudgetSpendLine> FromJournalDetails(
            IEnumerable<tbl_JournalVoucherDetails> details, int headerBranchId, int headerCostCenterId)
        {
            var list = new List<BudgetSpendLine>();
            if (details == null) return list;
            foreach (var d in details)
            {
                if (d == null || d.AccountID <= 0 || d.Debit <= 0) continue;
                list.Add(new BudgetSpendLine
                {
                    AccountID = d.AccountID,
                    CostCenterID = d.CostCenterID > 0 ? d.CostCenterID : headerCostCenterId,
                    BranchID = d.BranchID > 0 ? d.BranchID : headerBranchId,
                    Amount = d.Debit,
                });
            }
            return list;
        }

        public static List<BudgetSpendLine> FromCreditNoteDetails(
            IEnumerable<DBCreditNoteDetails> details, int headerBranchId, int headerCostCenterId)
        {
            var list = new List<BudgetSpendLine>();
            if (details == null) return list;
            foreach (var d in details)
            {
                if (d == null || d.AccountID <= 0) continue;
                decimal amt = d.Debit > 0 ? d.Debit : d.Credit;
                if (amt <= 0) continue;
                list.Add(new BudgetSpendLine
                {
                    AccountID = d.AccountID,
                    CostCenterID = d.CostCenterID > 0 ? d.CostCenterID : headerCostCenterId,
                    BranchID = d.BranchID > 0 ? d.BranchID : headerBranchId,
                    Amount = amt,
                });
            }
            return list;
        }

        /// <summary>
        /// Apply budget gate. Returns null if OK to proceed (possibly with forcedDraft/override).
        /// Returns BUDGET_OVER JSON payload string when blocked.
        /// When override accepted, sets forceDraftForApproval=true and caller must auto-submit.
        /// </summary>
        public string ApplyGate(
            int companyId,
            int documentTypeId,
            DateTime voucherDate,
            int branchId,
            int costCenterId,
            IEnumerable<BudgetSpendLine> spendLines,
            string budgetOverrideReason,
            out bool forceDraftForApproval,
            out BudgetCheckResult check,
            string excludeDocumentGuid = null,
            string excludeJvGuid = null)
        {
            forceDraftForApproval = false;
            check = Evaluate(companyId, voucherDate, branchId, costCenterId, spendLines,
                excludeDocumentGuid, excludeJvGuid);

            if (!check.IsOver) return null;

            if (string.IsNullOrWhiteSpace(budgetOverrideReason))
            {
                return "BUDGET_OVER:" + Newtonsoft.Json.JsonConvert.SerializeObject(check);
            }

            // Override requires an enabled approval policy for this document type
            if (!new clsApprovalEngine().HasEnabledPolicyWithLevels(companyId, documentTypeId))
            {
                check.Message = "Budget override requires an enabled approval policy for this document type.";
                return "BUDGET_OVER:" + Newtonsoft.Json.JsonConvert.SerializeObject(check);
            }

            forceDraftForApproval = true;
            return null;
        }

        decimal GetPostedActual(int companyId, int accountId, int costCenterId, int branchId,
            int year, int month, string excludeDocumentGuid, string excludeJvGuid)
        {
            clsSQL sql = new clsSQL();
            object o = sql.ExecuteScalar(@"
SELECT ISNULL(SUM(d.Debit),0)
FROM tbl_JournalVoucherDetails d
INNER JOIN tbl_JournalVoucherHeader h ON h.Guid = d.HeaderGuid AND h.CompanyID = d.CompanyID
WHERE d.CompanyID = @CompanyID
  AND d.AccountID = @AccountID
  AND ISNULL(d.CostCenterID,0) = @CostCenterID
  AND ISNULL(d.BranchID,0) = @BranchID
  AND YEAR(h.VoucherDate) = @Year
  AND MONTH(h.VoucherDate) = @Month
  AND ISNULL(h.DocumentStatus,2) = 2
  AND (@ExcludeJv = '00000000-0000-0000-0000-000000000000' OR h.Guid <> @ExcludeJv)
  AND (@ExcludeDoc = '00000000-0000-0000-0000-000000000000' OR h.Guid <> @ExcludeDoc)",
                new SqlParameter[]
                {
                    new SqlParameter("@CompanyID", SqlDbType.Int) { Value = companyId },
                    new SqlParameter("@AccountID", SqlDbType.Int) { Value = accountId },
                    new SqlParameter("@CostCenterID", SqlDbType.Int) { Value = costCenterId },
                    new SqlParameter("@BranchID", SqlDbType.Int) { Value = branchId },
                    new SqlParameter("@Year", SqlDbType.Int) { Value = year },
                    new SqlParameter("@Month", SqlDbType.Int) { Value = month },
                    new SqlParameter("@ExcludeJv", SqlDbType.UniqueIdentifier)
                    {
                        Value = string.IsNullOrWhiteSpace(excludeJvGuid)
                            ? Guid.Empty
                            : Simulate.Guid(excludeJvGuid)
                    },
                    new SqlParameter("@ExcludeDoc", SqlDbType.UniqueIdentifier)
                    {
                        Value = string.IsNullOrWhiteSpace(excludeDocumentGuid)
                            ? Guid.Empty
                            : Simulate.Guid(excludeDocumentGuid)
                    },
                },
                sql.CreateDataBaseConnectionString(companyId), null);
            return Simulate.Decimal(o);
        }

        static string Key(int accountId, int costCenterId, int branchId) =>
            accountId + "|" + costCenterId + "|" + branchId;
    }
}
