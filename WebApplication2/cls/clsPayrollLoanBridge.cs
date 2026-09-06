using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Data;

namespace WebApplication2.cls
{
    /// <summary>
    /// Auto-deducts employee financing loans (LoanType=1) into payroll via LOAN_DED.
    /// Links posted deductions in tbl_PayrollLoanDeduction to prevent double-deduct.
    /// </summary>
    public class clsPayrollLoanBridge
    {
        public void ApplyDueLoans(
            int employeeId,
            int payrollPeriodId,
            int companyId,
            List<PayrollDetailModel> details,
            Dictionary<string, decimal> variables)
        {
            if (details == null) return;

            DateTime periodStart;
            DateTime periodEnd;
            new clsPayrollPeriod().GetPeriodDates(payrollPeriodId, out periodStart, out periodEnd, companyId);

            string empCode = ResolveEmployeeCode(employeeId, companyId);
            if (string.IsNullOrWhiteSpace(empCode)) return;

            int businessPartnerId = ResolveBusinessPartnerId(empCode, companyId);
            if (businessPartnerId <= 0) return;

            DataTable dues = LoadDueLoanInstallments(businessPartnerId, periodStart, periodEnd, companyId);
            if (dues == null || dues.Rows.Count == 0) return;

            decimal totalDue = 0;
            foreach (DataRow row in dues.Rows)
            {
                decimal due = Simulate.Decimal(row["DueAmount"]);
                if (due > 0) totalDue += due;
            }

            if (totalDue <= 0) return;

            int elementId = ResolveElementIdByCode("LOAN_DED", companyId);
            if (elementId <= 0) return;

            details.RemoveAll(d =>
                d.IsSystemGenerated &&
                string.Equals(d.BasicSalaryCode, "LOAN_DED", StringComparison.OrdinalIgnoreCase));

            string name = "Loan Deduction";
            DataTable el = new clsSalariesElements().SelectSalariesElements(elementId, "", "", "", companyId);
            if (el != null && el.Rows.Count > 0)
                name = Simulate.String(el.Rows[0]["AName"]);

            details.Add(new PayrollDetailModel
            {
                SalaryElementID = elementId,
                ElementName = name,
                Amount = Math.Round(totalDue, 3),
                ElementTypeID = clsPayrollEngine.ElementTypeDeduction,
                BasicSalaryCode = "LOAN_DED",
                IsAffectSocialSecurity = false,
                IsTaxable = false,
                IsSystemGenerated = true,
                SystemSource = "LOAN"
            });

            if (variables != null)
                variables["LOAN_DED"] = Math.Round(totalDue, 3);
        }

        /// <summary>
        /// Records link rows for due financing amounts deducted on this payroll header (prevents re-deduct).
        /// </summary>
        public void MarkLoansPaidOnPost(int employeeId, int payrollPeriodId, int companyId, string payrollHeaderGuid, SqlTransaction trn)
        {
            if (string.IsNullOrWhiteSpace(payrollHeaderGuid)) return;

            DateTime periodStart;
            DateTime periodEnd;
            new clsPayrollPeriod().GetPeriodDates(payrollPeriodId, out periodStart, out periodEnd, companyId);

            string empCode = ResolveEmployeeCode(employeeId, companyId);
            if (string.IsNullOrWhiteSpace(empCode)) return;

            int businessPartnerId = ResolveBusinessPartnerId(empCode, companyId);
            if (businessPartnerId <= 0) return;

            DataTable dues = LoadDueLoanInstallments(businessPartnerId, periodStart, periodEnd, companyId, trn);
            if (dues == null || dues.Rows.Count == 0) return;

            clsSQL sql = new clsSQL();
            foreach (DataRow row in dues.Rows)
            {
                decimal amount = Simulate.Decimal(row["DueAmount"]);
                if (amount <= 0) continue;

                string financingGuid = Simulate.String(row["FinancingHeaderGuid"]);
                string jvDetailGuid = Simulate.String(row["JVDetailGuid"]);

                SqlParameter[] prm =
                {
                    new SqlParameter("@PayrollHeaderGuid", SqlDbType.UniqueIdentifier) { Value = Simulate.Guid(payrollHeaderGuid) },
                    new SqlParameter("@FinancingHeaderGuid", SqlDbType.UniqueIdentifier) { Value = Simulate.Guid(financingGuid) },
                    new SqlParameter("@JVDetailGuid", SqlDbType.UniqueIdentifier)
                    {
                        Value = string.IsNullOrWhiteSpace(jvDetailGuid)
                            ? (object)DBNull.Value
                            : Simulate.Guid(jvDetailGuid)
                    },
                    new SqlParameter("@Amount", SqlDbType.Decimal) { Value = amount },
                    new SqlParameter("@CompanyID", SqlDbType.Int) { Value = companyId },
                };

                sql.ExecuteNonQueryStatement(@"
IF NOT EXISTS (
  SELECT 1 FROM tbl_PayrollLoanDeduction
  WHERE PayrollHeaderGuid=@PayrollHeaderGuid
    AND FinancingHeaderGuid=@FinancingHeaderGuid
    AND CompanyID=@CompanyID
    AND (@JVDetailGuid IS NULL OR JVDetailGuid=@JVDetailGuid)
)
INSERT INTO tbl_PayrollLoanDeduction
  (PayrollHeaderGuid, FinancingHeaderGuid, JVDetailGuid, Amount, CompanyID, CreationUserID, CreationDate)
VALUES
  (@PayrollHeaderGuid, @FinancingHeaderGuid, @JVDetailGuid, @Amount, @CompanyID, 0, GETDATE())",
                    sql.CreateDataBaseConnectionString(companyId), prm, trn);
            }
        }

        public void ReverseLoansOnCancel(string payrollHeaderGuid, int companyId, SqlTransaction trn)
        {
            if (string.IsNullOrWhiteSpace(payrollHeaderGuid)) return;

            clsSQL sql = new clsSQL();
            SqlParameter[] prm =
            {
                new SqlParameter("@PayrollHeaderGuid", SqlDbType.UniqueIdentifier) { Value = Simulate.Guid(payrollHeaderGuid) },
                new SqlParameter("@CompanyID", SqlDbType.Int) { Value = companyId },
            };
            sql.ExecuteNonQueryStatement(@"
DELETE FROM tbl_PayrollLoanDeduction
WHERE PayrollHeaderGuid = @PayrollHeaderGuid AND CompanyID = @CompanyID",
                sql.CreateDataBaseConnectionString(companyId), prm, trn);
        }

        string ResolveEmployeeCode(int employeeId, int companyId)
        {
            clsSQL sql = new clsSQL();
            SqlParameter[] prm =
            {
                new SqlParameter("@ID", SqlDbType.Int) { Value = employeeId },
                new SqlParameter("@CompanyID", SqlDbType.Int) { Value = companyId },
            };
            return Simulate.String(sql.ExecuteScalar(
                "SELECT TOP 1 ISNULL(EmployeeCode,'') FROM tbl_employee WHERE ID=@ID AND CompanyID=@CompanyID",
                prm, sql.CreateDataBaseConnectionString(companyId), null));
        }

        int ResolveBusinessPartnerId(string empCode, int companyId)
        {
            clsSQL sql = new clsSQL();
            SqlParameter[] prm =
            {
                new SqlParameter("@EmpCode", SqlDbType.NVarChar, 100) { Value = empCode },
                new SqlParameter("@CompanyID", SqlDbType.Int) { Value = companyId },
            };
            return Simulate.Integer32(sql.ExecuteScalar(@"
SELECT TOP 1 ID FROM tbl_BusinessPartner
WHERE EmpCode = @EmpCode AND (CompanyID=@CompanyID OR @CompanyID=0)",
                prm, sql.CreateDataBaseConnectionString(companyId), null));
        }

        /// <summary>
        /// Due installments in period for employee loans (LoanType=1), excluding amounts already linked via payroll.
        /// </summary>
        DataTable LoadDueLoanInstallments(int businessPartnerId, DateTime periodStart, DateTime periodEnd, int companyId, SqlTransaction trn = null)
        {
            clsSQL sql = new clsSQL();
            SqlParameter[] prm =
            {
                new SqlParameter("@BusinessPartnerID", SqlDbType.Int) { Value = businessPartnerId },
                new SqlParameter("@Date1", SqlDbType.DateTime) { Value = periodStart.Date },
                new SqlParameter("@Date2", SqlDbType.DateTime) { Value = periodEnd.Date },
                new SqlParameter("@CompanyID", SqlDbType.Int) { Value = companyId },
            };

            string query = @"
SELECT
  CAST(fh.Guid AS NVARCHAR(50)) AS FinancingHeaderGuid,
  CAST(jvd.Guid AS NVARCHAR(50)) AS JVDetailGuid,
  CAST(
    ISNULL(jvd.Total, 0)
    - ISNULL((
        SELECT SUM(r.Amount) FROM tbl_Reconciliation r WHERE r.JVDetailsGuid = jvd.Guid
      ), 0)
    - ISNULL((
        SELECT SUM(p.Amount) FROM tbl_PayrollLoanDeduction p
        WHERE p.JVDetailGuid = jvd.Guid AND p.CompanyID = @CompanyID
      ), 0)
  AS DECIMAL(18,3)) AS DueAmount
FROM tbl_FinancingHeader fh
INNER JOIN tbl_FinancingDetails fd ON fd.HeaderGuid = fh.Guid
INNER JOIN tbl_JournalVoucherDetails jvd ON jvd.ParentGuid = fd.JVGuid
WHERE fh.LoanType = 1
  AND fh.BusinessPartnerID = @BusinessPartnerID
  AND (fh.CompanyID = @CompanyID OR @CompanyID = 0)
  AND jvd.DueDate BETWEEN @Date1 AND @Date2
  AND ISNULL(jvd.Debit, 0) > 0
  AND NOT EXISTS (
    SELECT 1 FROM tbl_PayrollLoanDeduction p
    WHERE p.FinancingHeaderGuid = fh.Guid
      AND p.JVDetailGuid = jvd.Guid
      AND p.CompanyID = @CompanyID
  )";

            return sql.ExecuteQueryStatement(query, sql.CreateDataBaseConnectionString(companyId), prm, trn);
        }

        int ResolveElementIdByCode(string code, int companyId)
        {
            DataTable dt = new clsSalariesElements().SelectSalariesElements(0, code, "", "", companyId);
            if (dt == null || dt.Rows.Count == 0) return 0;
            return Simulate.Integer32(dt.Rows[0]["ID"]);
        }
    }
}
