using Microsoft.CodeAnalysis.Operations;
using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using WebApplication2.DataBaseTable;
using WebApplication2.MainClasses;

namespace WebApplication2.cls
{
    public class clsPayrollPostingService
    {

        // ---------------------------------------------------------
        // LOAD EMPLOYEES + ELEMENTS FOR POSTING
        // ---------------------------------------------------------
        public List<PayrollEmployeePostingRow> LoadEmployeesForPosting(int periodId, int companyId)
        {
            var result = new List<PayrollEmployeePostingRow>();

            clsPayrollDetails cls = new clsPayrollDetails();

            DataTable dt = cls.SelectPayrollForPosting(periodId, companyId);

          
             

            foreach (DataRow row in dt.Rows)
            {
                var emp = new PayrollEmployeePostingRow
                {
                    EmployeeID = Simulate.Integer32(row["EmployeeID"]),
                    EmployeeName = Simulate.String(row["EmployeeName"])
                };
               
                // Load salary elements
                clsPayrollElement clsEl = new clsPayrollElement();
                var dtElements = clsEl.SelectElementsWithAccounts(emp.EmployeeID, periodId, companyId,null,null);

                foreach (DataRow el in dtElements.Rows)
                {
                    emp.Elements.Add(new PayrollElementRow
                    {
                        ElementID = Simulate.Integer32(el["SalaryElementID"]),
                        ElementName = Simulate.String(el["ElementName"]),
                        Amount = Simulate.Decimal(el["Amount"]),
                        //IsDeduction = Simulate.Bool(el["IsDeduction"]),
                        AccountID = Simulate.Integer32(el["AccountID"]) // ★ USE ELEMENT ACCOUNT
                    });
                }

                result.Add(emp);
            }

            return result;
        }

        // ---------------------------------------------------------
        // POST PAYROLL — MAIN ENGINE
        // ---------------------------------------------------------
        public clsPayrollPostingResult PostPayrollBatch(clsPayrollPostingRequest req)
        {
            clsPayrollPostingResult result = new clsPayrollPostingResult();
            clsSQL clsSQL = new clsSQL();
            using (SqlConnection con = new SqlConnection(clsSQL.CreateDataBaseConnectionString(req.CompanyID)))
            {
                con.Open();
                SqlTransaction trn = con.BeginTransaction();

                try
                {
                   
                    // 1) Create JV Header
                    clsJournalVoucherHeader jvh = new clsJournalVoucherHeader();
                    string JVGuid = jvh.InsertJournalVoucherHeader(
                        req.BranchID, 0,
                        "Payroll Posting Period " + req.PeriodID,
                        "",
                        (int)clsEnum.VoucherType.Payroll,
                        req.CompanyID,
                        DateTime.Now, req.UserID, "", 0, trn);

                    result.JVGuid = JVGuid;

                    // 2) Loop employees
                    foreach (var empId in req.EmployeeIDs)
                    {
                        var emp = LoadEmployeesForPosting(req.PeriodID, req.CompanyID)
                                    .First(x => x.EmployeeID == empId);
                        
                        var elements =  LoadEmployeeElements(emp.EmployeeID, req.PeriodID, req.CompanyID,
                           con, trn);

                        clsPayrollHeader clsPayrollHeader = new clsPayrollHeader();
                        clsPayrollDetails clsPayrollDetails = new clsPayrollDetails();
                        decimal BasicSalary = 0;
                        decimal totalEarn = elements.Where(x => !x.IsDeduction).Sum(x => x.Amount);
                        decimal totalDed = elements.Where(x => x.IsDeduction).Sum(x => x.Amount);
                        decimal net = totalEarn - totalDed;
                        int postingGuid = clsPayrollHeader.InsertPayrollHeader(req.PeriodID,
    empId, BasicSalary, totalEarn, totalDed, net,0,req.CompanyID, req.UserID, JVGuid, trn);
                         
             
            
          
                                // 1=Draft, 2=Approved, 3=Posted
           
                        foreach (var e in elements)
                        {
                            clsPayrollDetails.InsertPayrollDetails(postingGuid, e.ElementID
                                , 0//elementtypeid
                                , 0//CalcTypeID    
                                ,e.Amount, e.Amount, req.CompanyID, req.UserID
                                , trn);

                           
            
     
        
          
                            PostElementLine(e, JVGuid, req, trn);
                        }

                        // Mark payroll row as posted
                        clsPayrollHeader hdr = new clsPayrollHeader();
                        hdr.MarkAsPosted(empId, req.PeriodID, req.CompanyID, trn);
                    }
                

                    
                    // 3) Validate JV balancing
                    clsJournalVoucherHeader jvCheck = new clsJournalVoucherHeader();
                    if (!jvCheck.CheckJVMatch(JVGuid, req.CompanyID, trn))
                    {
                        trn.Rollback();
                        result.Success = false;
                        result.Messages.Add("Journal is not balanced!");
                        return result;
                    }

                    trn.Commit();
                    result.Success = true;
                    result.Messages.Add("Payroll posted successfully.");

                    return result;
                }
                catch (Exception ex)
                {
                    trn.Rollback();
                    result.Success = false;
                    result.Messages.Add(ex.Message);
                    return result;
                }
            }
        }

        // ---------------------------------------------------------
        // POST SINGLE ELEMENT LINE INTO JOURNAL
        // ---------------------------------------------------------
        private void PostElementLine(PayrollEmployeePostingRow e, string JVGuid, clsPayrollPostingRequest req, SqlTransaction trn)
        {
            clsJournalVoucherDetails clsJV = new clsJournalVoucherDetails();

            decimal debit = e.IsDeduction ? 0 : e.Amount;
            decimal credit = e.IsDeduction ? e.Amount : 0;

            clsJV.InsertJournalVoucherDetails(
                JVGuid,
                0,
                e.AccountID,   // ★ ELEMENT ACCOUNT
                0,
                debit,
                credit,
                debit - credit,
                1, 1, e.Amount,
                req.BranchID, 0,
                DateTime.Now, "",
                req.CompanyID,
                req.UserID,
                "",
                trn);
            clsJV.InsertJournalVoucherDetails(
               JVGuid,
               0,
               e.AccountID,   // ★ ELEMENT ACCOUNT
               0,
               credit,
            debit     ,
              credit - debit ,
               1, 1, e.Amount,
               req.BranchID, 0,
               DateTime.Now, "",
               req.CompanyID,
               req.UserID,
               "",
               trn);
        }

public List<PayrollEmployeePostingRow> LoadEmployeeElements(
    int employeeId,
    int periodId,
    int companyId,
    SqlConnection conn,
    SqlTransaction trn)
        {
            List<PayrollEmployeePostingRow> list = new List<PayrollEmployeePostingRow>();
             
            clsPayrollElement clsEl = new clsPayrollElement();
            DataTable dt = clsEl.SelectElementsWithAccounts(employeeId, periodId, companyId, conn, trn);

            foreach (DataRow row in dt.Rows)
            {
                var item = new PayrollEmployeePostingRow
                {
                    EmployeeID = employeeId,
                    ElementID = Simulate.Integer32(row["SalaryElementID"]),
                    ElementName = Simulate.String(row["ElementName"]),
                    Amount = Simulate.Decimal(row["Amount"]),
                    AccountID = Simulate.Integer32(row["AccountID"]),     // account attached to element
                    BranchID = Simulate.Integer32(row["BranchID"]),
                    CostCenterID = Simulate.Integer32(row["CostCenterID"])
                };

                list.Add(item);
            }
           
            return list;
        }
        public string CancelPayrollPosting_HardDelete(int periodId, int EmployeeID,int companyId)
        {
            clsSQL clsSQL = new clsSQL();
            using (SqlConnection con = new SqlConnection(clsSQL.CreateDataBaseConnectionString(companyId)))
            using (SqlCommand cmd = new SqlCommand())
            {
                cmd.Connection = con;
                cmd.CommandType = CommandType.Text;
                
                cmd.CommandText = @"

            DECLARE @JVGuid NVARCHAR(100);

            -- 1) Get JV Guid from PayrollHeader
            SELECT @JVGuid = JVGuid
            FROM tbl_PayrollHeader
            WHERE PayrollPeriodID = @PeriodID
              AND EmployeeID = @EmployeeID;

            IF (@JVGuid IS NULL OR @JVGuid = '')
            BEGIN
                SELECT 'NO_POSTING_FOUND' AS Status;
                RETURN;
            END

            -- 2) Delete JV Details
            DELETE FROM tbl_JournalVoucherDetails
            WHERE ParentGuid = @JVGuid
              --AND CompanyID = @CompanyID;

            -- 3) Delete JV Header
            DELETE FROM tbl_JournalVoucherHeader
            WHERE Guid = @JVGuid
              --AND CompanyID = @CompanyID;

            -- 4) Delete Payroll Posting Details
            DELETE FROM tbl_PayrollDetails
            WHERE PayrollHeaderID IN (
                SELECT id FROM tbl_PayrollHeader
                WHERE PayrollPeriodID = @PeriodID
                 AND EmployeeID = @EmployeeID
            )
        

            -- 5) Delete Payroll Posting Header
            DELETE FROM tbl_PayrollHeader
            WHERE PayrollPeriodID = @PeriodID
                AND EmployeeID = @EmployeeID;

            

            SELECT 'CANCELLED' AS Status;
        ";
                
                cmd.Parameters.AddWithValue("@PeriodID", periodId);
                cmd.Parameters.AddWithValue("@CompanyID", companyId);
                cmd.Parameters.AddWithValue("@EmployeeID", EmployeeID);

                con.Open();
                object? result = cmd.ExecuteScalar();

                return result?.ToString() ?? "ERROR";
            }
        }

    }
}
public class PayrollEmployeePostingRow
{
    public int EmployeeID { get; set; }
    public string EmployeeName { get; set; }

    // List of all salary elements for this employee
    public List<PayrollElementRow> Elements { get; set; } = new();
}
public class PayrollElementRow
{
    public int ElementID { get; set; }
    public string ElementName { get; set; }
    public decimal Amount { get; set; }
    public bool IsDeduction { get; set; }

    // ★ IMPORTANT: The accounting entry must use THIS account
    public int AccountID { get; set; }
}