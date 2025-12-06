using Microsoft.CodeAnalysis.Operations;
using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using WebApplication2.Controllers;


namespace WebApplication2.cls
{
    public class clsPayrollPosting
    {
        // ------------------------------------------------------------
        // MAIN ENTRY FUNCTION — CALLED BY API
        // ------------------------------------------------------------
        public PayrollPostingResult PostSelectedEmployees(PayrollPostingRequest req)
        {
            clsSQL clsSQL = new clsSQL();
            using (SqlConnection con = new SqlConnection(clsSQL.CreateDataBaseConnectionString(req.CompanyID)))
            {
                con.Open();
                SqlTransaction trn = con.BeginTransaction();

                try
                {
                    int postedCount = 0;
                    string journalNumber = GenerateJournalNumber(con, trn);

                    foreach (int empId in req.EmployeeIDs)
                    {
                        // 1) Load employee + salary elements
                        var employee = LoadEmployeeForPosting(empId, req.PeriodID, req.CompanyID, con, trn);
                        if (employee == null)
                            throw new Exception($"Employee {empId} not found.");

                        List<PayrollEmployeePostingRow> elements =
                            LoadEmployeeElements(empId, req.PeriodID, req.CompanyID, con, trn);

                        // 2) Insert journal voucher rows
                        InsertJournalRows(journalNumber, req, employee, elements, con, trn);

                        // 3) Mark payroll header as posted
                        MarkAsPosted(empId, req.PeriodID, req.CompanyID, con, trn);

                        postedCount++;
                    }

                    trn.Commit();

                    return new PayrollPostingResult
                    {
                        PostedCount = postedCount,
                        JournalNumber = journalNumber
                    };
                }
                catch (Exception ex)
                {
                    trn.Rollback();
                    throw new Exception($"Posting failed: {ex.Message}");
                }
            }
        }

        // ------------------------------------------------------------
        // GENERATE JOURNAL NUMBER
        // ------------------------------------------------------------
        private string GenerateJournalNumber(SqlConnection con, SqlTransaction trn)
        {
            return "JV-" + DateTime.Now.ToString("yyyyMMdd-HHmmss");
        }

        // ------------------------------------------------------------
        // LOAD EMPLOYEE HEADER DATA
        // ------------------------------------------------------------
        private PayrollEmployeeForPosting LoadEmployeeForPosting(
            int empId, int periodId, int companyId,
            SqlConnection con, SqlTransaction trn)
        {
            string sql = @"
                SELECT e.EmployeeID, e.AName AS EmployeeName, d.AName AS DepartmentName
                FROM tbl_Employee e
                LEFT JOIN tbl_Department d ON d.ID = e.DepartmentID
                WHERE e.EmployeeID = @EmpID AND e.CompanyID = @CompanyID";

            using (SqlCommand cmd = new SqlCommand(sql, con, trn))
            {
                cmd.Parameters.AddWithValue("@EmpID", empId);
                cmd.Parameters.AddWithValue("@CompanyID", companyId);

                using (SqlDataReader rd = cmd.ExecuteReader())
                {
                    if (!rd.Read()) return null;

                    return new PayrollEmployeeForPosting
                    {
                        EmployeeID = empId,
                        EmployeeName = rd["EmployeeName"].ToString(),
                        DepartmentName = rd["DepartmentName"].ToString()
                    };
                }
            }
        }

        // ------------------------------------------------------------
        // LOAD PAYROLL ELEMENTS WITH THEIR ACCOUNTS
        // ------------------------------------------------------------
        private List<PayrollEmployeePostingRow> LoadEmployeeElements(
            int empId, int periodId, int companyId,
            SqlConnection con, SqlTransaction trn)
        {
            string sql = @"
                SELECT 
                    e.ElementID,
                    e.ElementType,   -- Earning or Deduction
                    e.Amount,
                    el.AName AS ElementName,
                    el.AccountID     -- <-- ACCOUNT COMES FROM ELEMENT MASTER
                FROM tbl_PayrollEmployeeElements e
                INNER JOIN tbl_PayrollElements el ON el.ID = e.ElementID
                WHERE e.EmployeeID = @EmpID
                  AND e.PeriodID = @PeriodID
                  AND e.CompanyID = @CompanyID";

            List<PayrollEmployeePostingRow> list = new List<PayrollEmployeePostingRow>();

            using (SqlCommand cmd = new SqlCommand(sql, con, trn))
            {
                cmd.Parameters.AddWithValue("@EmpID", empId);
                cmd.Parameters.AddWithValue("@PeriodID", periodId);
                cmd.Parameters.AddWithValue("@CompanyID", companyId);

                using (SqlDataReader rd = cmd.ExecuteReader())
                {
                    while (rd.Read())
                    {
                        list.Add(new PayrollEmployeePostingRow
                        {
                            ElementID = Convert.ToInt32(rd["ElementID"]),
                            ElementName = rd["ElementName"].ToString(),
                            ElementType = rd["ElementType"].ToString(),  // Earning/Deduction
                            Amount = Convert.ToDecimal(rd["Amount"]),
                            AccountID = Convert.ToInt32(rd["AccountID"]) // << IMPORTANT
                        });
                    }
                }
            }

            return list;
        }

        // ------------------------------------------------------------
        // INSERT JOURNAL VOUCHER LINES
        // ------------------------------------------------------------
        private void InsertJournalRows(
            string journalNumber, PayrollPostingRequest req,
            PayrollEmployeeForPosting emp,
            List<PayrollEmployeePostingRow> elements,
            SqlConnection con, SqlTransaction trn)
        {
            foreach (var row in elements)
            {
                string sql = @"
                    INSERT INTO tbl_JournalVoucher
                    (JournalNumber, CompanyID, PeriodID, EmployeeID, AccountID, Debit, Credit, Description)
                    VALUES
                    (@JV, @CID, @PID, @EmpID, @AccID, @Debit, @Credit, @Desc)";

                decimal debit = 0, credit = 0;

                if (row.ElementType == "EARNING")
                    debit = row.Amount;
                else
                    credit = row.Amount;

                using (SqlCommand cmd = new SqlCommand(sql, con, trn))
                {
                    cmd.Parameters.AddWithValue("@JV", journalNumber);
                    cmd.Parameters.AddWithValue("@CID", req.CompanyID);
                    cmd.Parameters.AddWithValue("@PID", req.PeriodID);
                    cmd.Parameters.AddWithValue("@EmpID", emp.EmployeeID);
                    cmd.Parameters.AddWithValue("@AccID", row.AccountID);
                    cmd.Parameters.AddWithValue("@Debit", debit);
                    cmd.Parameters.AddWithValue("@Credit", credit);
                    cmd.Parameters.AddWithValue("@Desc",
                        $"{row.ElementName} for {emp.EmployeeName}");

                    cmd.ExecuteNonQuery();
                }
            }
        }

        // ------------------------------------------------------------
        // MARK PAYROLL HEADER AS POSTED
        // ------------------------------------------------------------
        private void MarkAsPosted(
            int empId, int periodId, int companyId,
            SqlConnection con, SqlTransaction trn)
        {
            string sql = @"
                UPDATE tbl_PayrollHeader
                SET IsPosted = 1, PostedDate = GETDATE()
                WHERE EmployeeID = @EmpID
                  AND PeriodID = @PID
                  AND CompanyID = @CID";

            using (SqlCommand cmd = new SqlCommand(sql, con, trn))
            {
                cmd.Parameters.AddWithValue("@EmpID", empId);
                cmd.Parameters.AddWithValue("@PID", periodId);
                cmd.Parameters.AddWithValue("@CID", companyId);
                cmd.ExecuteNonQuery();
            }
        }
    }

    // ------------------------------------------------------------
    // RETURN MODEL
    // ------------------------------------------------------------
    public class PayrollPostingResult
    {
        public int PostedCount { get; set; }
        public string JournalNumber { get; set; }
    }
    public class PayrollEmployeePostingRow
    {
        public int EmployeeID { get; set; }
        public string EmployeeName { get; set; }
        
        public bool IsDeduction { get; set; }
        public int ElementID { get; set; }
        public string ElementName { get; set; }
        public string ElementType { get; set; }   // EARNING or DEDUCTION
        public decimal Amount { get; set; }
        public int AccountID { get; set; }
        public int BranchID { get; set; }
        public int CostCenterID { get; set; }
        public List<PayrollElementRow> Elements { get; set; } = new List<PayrollElementRow>();


    }
    public class PayrollEmployeeForPosting
    {
        public int EmployeeID { get; set; }
        public string EmployeeName { get; set; }
        public string DepartmentName { get; set; }
    }
}
