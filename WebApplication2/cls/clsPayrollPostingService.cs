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
    public class clsPayrollPostingService
    {
        private readonly Dictionary<int, DataRow> _elementMasterCache = new();

        // ---------------------------------------------------------
        // LOAD EMPLOYEES + CALCULATED LINES (matches payroll preview)
        // ---------------------------------------------------------
        public List<PayrollEmployeePostingRow> LoadEmployeesForPosting(int periodId, int companyId)
        {
            var result = new List<PayrollEmployeePostingRow>();
            clsPayrollEngine engine = new clsPayrollEngine();
            clsSQL cls = new clsSQL();

            SqlParameter[] prm =
            {
                new SqlParameter("@CompanyID", SqlDbType.Int) { Value = companyId }
            };

            string sqlEmp = @"
                SELECT bp.ID AS EmployeeID, bp.AName AS EmployeeName
                FROM tbl_employee bp
                WHERE bp.CompanyID = @CompanyID";

            DataTable dtEmployees = cls.ExecuteQueryStatement(
                sqlEmp,
                cls.CreateDataBaseConnectionString(companyId),
                prm);

            foreach (DataRow empRow in dtEmployees.Rows)
            {
                int empId = Simulate.Integer32(empRow["EmployeeID"]);
                var preview = engine.PreviewPayroll(empId, periodId, companyId);

                var emp = new PayrollEmployeePostingRow
                {
                    EmployeeID = empId,
                    EmployeeName = Simulate.String(empRow["EmployeeName"]),
                    BasicSalary = preview.BasicSalary,
                    TotalEarnings = preview.TotalEarnings,
                    TotalDeductions = preview.TotalDeductions,
                    NetSalary = preview.NetSalary,
                    IsPosted = preview.IsPosted
                };

                try
                {
                    emp.Elements = BuildPostingLines(empId, periodId, companyId, preview);
                }
                catch (Exception ex)
                {
                    emp.Elements = new List<PayrollElementRow>();
                    emp.ValidationError = ex.Message;
                }

                result.Add(emp);
            }

            return result;
        }

        // ---------------------------------------------------------
        // POST PAYROLL — creates balanced JV per employee
        // ---------------------------------------------------------
        public clsPayrollPostingResult PostPayrollBatch(clsPayrollPostingRequest req)
        {
            clsPayrollPostingResult result = new clsPayrollPostingResult();
            clsSQL clsSQL = new clsSQL();
            _elementMasterCache.Clear();

            clsPayrollPeriod period = new clsPayrollPeriod();
            DataTable dtPeriod = period.SelectPayrollPeriod(req.PeriodID, "", -1, req.CompanyID);
            if (dtPeriod == null || dtPeriod.Rows.Count == 0)
            {
                result.Success = false;
                result.Messages.Add("Payroll period not found.");
                return result;
            }
            if (Simulate.Bool(dtPeriod.Rows[0]["IsClosed"]))
            {
                result.Success = false;
                result.Messages.Add("Payroll period is closed and cannot be posted.");
                return result;
            }

            clsPayrollEngine engine = new clsPayrollEngine();
            int postedCount = 0;

            using (SqlConnection con = new SqlConnection(clsSQL.CreateDataBaseConnectionString(req.CompanyID)))
            {
                con.Open();

                foreach (var empId in req.EmployeeIDs)
                {
                    SqlTransaction trn = con.BeginTransaction();
                    try
                    {
                        PayrollPreviewResult preview =
                            engine.PreviewPayroll(empId, req.PeriodID, req.CompanyID);

                        if (preview.IsPosted)
                        {
                            trn.Rollback();
                            result.Messages.Add($"Employee {empId}: already posted for this period.");
                            continue;
                        }

                        List<PayrollElementRow> elements =
                            BuildPostingLines(empId, req.PeriodID, req.CompanyID, preview);

                        if (elements.Count == 0)
                        {
                            trn.Rollback();
                            result.Messages.Add($"Employee {empId}: no payroll lines to post.");
                            continue;
                        }

                        ValidatePostingLines(elements, empId);

                        clsApprovalEngine approvalEngine = new clsApprovalEngine();
                        int documentStatus = approvalEngine.ResolveInitialDocumentStatus(
                            req.CompanyID,
                            clsHcmApprovalDocuments.TypePayroll,
                            req.BranchID,
                            preview.NetSalary);
                        if (documentStatus != (int)DocumentStatus.Posted)
                        {
                            clsPayrollHeader draftHeader = new clsPayrollHeader();
                            draftHeader.InsertPayrollHeader(
                                req.PeriodID,
                                empId,
                                preview.BasicSalary,
                                preview.TotalEarnings,
                                preview.TotalDeductions,
                                preview.NetSalary,
                                1,
                                req.CompanyID,
                                req.UserID,
                                "",
                                trn,
                                documentStatus);
                            trn.Commit();
                            result.Messages.Add(
                                $"Employee {empId}: payroll saved as draft pending approval.");
                            continue;
                        }

                        string jvGuid = CreatePayrollJvHeader(req, empId, trn);

                        decimal totalEarn = preview.TotalEarnings;
                        decimal totalDed = preview.TotalDeductions;
                        decimal net = preview.NetSalary;

                        clsPayrollHeader clsPayrollHeader = new clsPayrollHeader();
                        clsPayrollDetails clsPayrollDetails = new clsPayrollDetails();
                        int postingGuid = clsPayrollHeader.InsertPayrollHeader(
                            req.PeriodID,
                            empId,
                            preview.BasicSalary,
                            totalEarn,
                            totalDed,
                            net,
                            3,
                            req.CompanyID,
                            req.UserID,
                            jvGuid,
                            trn);

                        foreach (var e in elements)
                        {
                            DataRow master = GetSalaryElementMaster(e.ElementID, req.CompanyID);
                            int calcTypeId = master != null
                                ? Simulate.Integer32(master["CalcTypeID"])
                                : 0;

                            clsPayrollDetails.InsertPayrollDetails(
                                postingGuid,
                                e.ElementID,
                                e.ElementTypeID,
                                calcTypeId,
                                e.Amount,
                                e.Amount,
                                req.CompanyID,
                                req.UserID,
                                trn);

                            PostBalancedElementLine(e, jvGuid, empId, req, trn);
                        }

                        clsJournalVoucherHeader jvCheck = new clsJournalVoucherHeader();
                        if (!jvCheck.CheckJVMatch(jvGuid, req.CompanyID, trn))
                        {
                            trn.Rollback();
                            result.Messages.Add(
                                $"Employee {empId}: journal voucher is not balanced. Check debit/credit accounts on salary elements.");
                            continue;
                        }

                        clsPayrollHeader hdr = new clsPayrollHeader();
                        hdr.MarkAsPosted(empId, req.PeriodID, req.CompanyID, trn);

                        trn.Commit();
                        postedCount++;
                        result.JVGuid = jvGuid;
                        result.Messages.Add(
                            $"Employee {empId}: posted. Net={net:N2}. JV={jvGuid}");
                    }
                    catch (Exception ex)
                    {
                        trn.Rollback();
                        result.Messages.Add($"Employee {empId}: {ex.Message}");
                    }
                }
            }

            result.Success = postedCount > 0;
            if (postedCount == 0 && result.Messages.Count == 0)
                result.Messages.Add("No employees were posted.");

            return result;
        }

        // ---------------------------------------------------------
        // Build lines from payroll engine (salary + attendance rules)
        // ---------------------------------------------------------
        public List<PayrollElementRow> BuildPostingLines(
            int employeeId,
            int periodId,
            int companyId,
            PayrollPreviewResult preview)
        {
            var lines = new List<PayrollElementRow>();

            if (preview?.SalaryElements != null)
            {
                foreach (var d in preview.SalaryElements)
                {
                    AddPostingLine(lines, d.SalaryElementID, d.ElementName, d.Amount, d.ElementTypeID, companyId);
                }
            }

            if (preview?.AttendanceElements != null)
            {
                foreach (var a in preview.AttendanceElements)
                {
                    if (a.SalaryElementID <= 0)
                        continue;

                    string name = !string.IsNullOrWhiteSpace(a.ElementName) ? a.ElementName : a.Code;
                    AddPostingLine(lines, a.SalaryElementID, name, a.Amount, a.ElementTypeID, companyId);
                }
            }

            return lines;
        }

        private void AddPostingLine(
            List<PayrollElementRow> lines,
            int salaryElementId,
            string elementName,
            decimal amount,
            int elementTypeId,
            int companyId)
        {
            if (salaryElementId <= 0 || amount == 0)
                return;

            DataRow master = GetSalaryElementMaster(salaryElementId, companyId);
            if (master == null)
                throw new Exception($"Salary element #{salaryElementId} ({elementName}) not found.");

            ResolveGlAccounts(master, elementTypeId, companyId, out int debitAcc, out int creditAcc);

            lines.Add(new PayrollElementRow
            {
                ElementID = salaryElementId,
                ElementName = elementName,
                Amount = Math.Abs(amount),
                ElementTypeID = elementTypeId,
                IsDeduction = elementTypeId == 2,
                DebitAccountID = debitAcc,
                CreditAccountID = creditAcc
            });
        }

        private DataRow GetSalaryElementMaster(int elementId, int companyId)
        {
            if (_elementMasterCache.TryGetValue(elementId, out DataRow cached))
                return cached;

            clsSalariesElements master = new clsSalariesElements();
            DataTable dt = master.SelectSalariesElements(elementId, "", "", "", companyId);
            if (dt != null && dt.Rows.Count > 0)
            {
                _elementMasterCache[elementId] = dt.Rows[0];
                return dt.Rows[0];
            }

            return null;
        }

        /// <summary>
        /// Maps element GL accounts: company accounts first, then employee-specific accounts.
        /// </summary>
        private static void ResolveGlAccounts(
            DataRow master,
            int elementTypeId,
            int companyId,
            out int debitAccountId,
            out int creditAccountId)
        {
            int compDr = Simulate.Integer32(master["CompanyDebitAccountID"]);
            int compCr = Simulate.Integer32(master["CompanyCreditAccountID"]);
            int empDr = Simulate.Integer32(master["EmployeeDebitAccountID"]);
            int empCr = Simulate.Integer32(master["EmployeeCreditAccountID"]);

            debitAccountId = compDr > 0 ? compDr : empDr;
            creditAccountId = compCr > 0 ? compCr : empCr;

            int defaultPayable = GetDefaultSalariesPayableAccount(companyId);

            if (elementTypeId == 2)
            {
                if (debitAccountId <= 0)
                    debitAccountId = empDr > 0 ? empDr : defaultPayable;
                if (creditAccountId <= 0)
                    creditAccountId = compCr > 0 ? compCr : empCr;
            }
            else
            {
                if (debitAccountId <= 0)
                    debitAccountId = compDr;
                if (creditAccountId <= 0)
                    creditAccountId = defaultPayable > 0 ? defaultPayable : empCr;
            }

            if (debitAccountId <= 0 || creditAccountId <= 0)
            {
                string code = Simulate.String(master["Code"]);
                string name = Simulate.String(master["AName"]);
                throw new Exception(
                    $"GL accounts missing for salary element '{code}' ({name}). Set company debit/credit accounts (and Salaries Payable in Account Settings if needed).");
            }

            if (debitAccountId == creditAccountId)
            {
                string code = Simulate.String(master["Code"]);
                throw new Exception(
                    $"Debit and credit accounts must differ for element '{code}'.");
            }
        }

        private static int GetDefaultSalariesPayableAccount(int companyId)
        {
            try
            {
                cls_AccountSetting settings = new cls_AccountSetting();
                DataTable dt = settings.SelectAccountSetting(
                    0,
                    (int)clsEnum.AccountMainSetting.Employees,
                    companyId);

                if (dt != null && dt.Rows.Count > 0)
                    return Simulate.Integer32(dt.Rows[0]["AccountID"]);
            }
            catch
            {
                // ignore
            }

            return 0;
        }

        private static void ValidatePostingLines(List<PayrollElementRow> elements, int employeeId)
        {
            foreach (var e in elements)
            {
                if (e.DebitAccountID <= 0 || e.CreditAccountID <= 0)
                    throw new Exception(
                        $"Employee {employeeId}, element '{e.ElementName}': invalid GL accounts.");
            }
        }

        private static string CreatePayrollJvHeader(
            clsPayrollPostingRequest req,
            int employeeId,
            SqlTransaction trn)
        {
            clsJournalVoucherHeader jvh = new clsJournalVoucherHeader();
            return jvh.InsertJournalVoucherHeader(
                req.BranchID,
                0,
                $"Payroll Period {req.PeriodID} — Employee {employeeId}",
                "",
                (int)clsEnum.VoucherType.Payroll,
                req.CompanyID,
                DateTime.Now,
                req.UserID,
                "",
                0,
                trn);
        }

        // ---------------------------------------------------------
        // POST Dr / Cr pair (expense or deduction → payable / liability)
        // ---------------------------------------------------------
        private void PostBalancedElementLine(
            PayrollElementRow e,
            string jvGuid,
            int employeeId,
            clsPayrollPostingRequest req,
            SqlTransaction trn)
        {
            if (e.Amount == 0)
                return;

            int debitSub = ResolveEmployeeSubAccount(e.DebitAccountID, employeeId, req.CompanyID);
            int creditSub = ResolveEmployeeSubAccount(e.CreditAccountID, employeeId, req.CompanyID);

            InsertJvDetail(jvGuid, e.DebitAccountID, debitSub, e.Amount, 0, req, trn);
            InsertJvDetail(jvGuid, e.CreditAccountID, creditSub, 0, e.Amount, req, trn);
        }

        private static int ResolveEmployeeSubAccount(int accountId, int employeeId, int companyId)
        {
            int payable = GetDefaultSalariesPayableAccount(companyId);
            if (payable > 0 && accountId == payable)
                return employeeId;
            return 0;
        }

        private static void InsertJvDetail(
            string jvGuid,
            int accountId,
            int subAccountId,
            decimal debit,
            decimal credit,
            clsPayrollPostingRequest req,
            SqlTransaction trn)
        {
            clsJournalVoucherDetails clsJV = new clsJournalVoucherDetails();
            decimal amount = debit > 0 ? debit : credit;

            clsJV.InsertJournalVoucherDetails(
                jvGuid,
                0,
                accountId,
                subAccountId,
                debit,
                credit,
                debit - credit,
                1,
                1,
                amount,
                req.BranchID,
                0,
                DateTime.Now,
                $"Payroll P{req.PeriodID}",
                req.CompanyID,
                req.UserID,
                "",
                trn);
        }

        public bool PostPayrollHeaderByGuid(string payrollHeaderGuid, int userId, int companyId, SqlTransaction trn)
        {
            clsSQL sql = new clsSQL();
            SqlParameter[] prm =
            {
                new SqlParameter("@Guid", SqlDbType.UniqueIdentifier) { Value = Simulate.Guid(payrollHeaderGuid) },
                new SqlParameter("@CompanyID", SqlDbType.Int) { Value = companyId },
            };

            DataTable dt = sql.ExecuteQueryStatement(@"
SELECT ID, PayrollPeriodID, EmployeeID, BasicSalary, TotalEarnings, TotalDeductions, NetSalary,
       ISNULL(JVGuid,'') AS JVGuid, ISNULL(DocumentStatus,2) AS DocumentStatus, ISNULL(IsPosted,0) AS IsPosted
FROM tbl_PayrollHeader
WHERE Guid = @Guid AND CompanyID = @CompanyID",
                sql.CreateDataBaseConnectionString(companyId), prm, trn);

            if (dt == null || dt.Rows.Count == 0) return false;

            DataRow row = dt.Rows[0];
            if (Simulate.Bool(row["IsPosted"]) ||
                Simulate.Integer32(row["DocumentStatus"]) == (int)DocumentStatus.Posted)
                return true;

            int empId = Simulate.Integer32(row["EmployeeID"]);
            int periodId = Simulate.Integer32(row["PayrollPeriodID"]);
            int headerId = Simulate.Integer32(row["ID"]);

            clsPayrollEngine engine = new clsPayrollEngine();
            PayrollPreviewResult preview = engine.PreviewPayroll(empId, periodId, companyId);
            List<PayrollElementRow> elements = BuildPostingLines(empId, periodId, companyId, preview);
            if (elements.Count == 0) return false;

            ValidatePostingLines(elements, empId);

            var req = new clsPayrollPostingRequest
            {
                PeriodID = periodId,
                CompanyID = companyId,
                UserID = userId,
                BranchID = 0,
            };

            string jvGuid = CreatePayrollJvHeader(req, empId, trn);
            clsPayrollDetails clsPayrollDetails = new clsPayrollDetails();

            foreach (var e in elements)
            {
                DataRow master = GetSalaryElementMaster(e.ElementID, companyId);
                int calcTypeId = master != null ? Simulate.Integer32(master["CalcTypeID"]) : 0;

                clsPayrollDetails.InsertPayrollDetails(
                    headerId,
                    e.ElementID,
                    e.ElementTypeID,
                    calcTypeId,
                    e.Amount,
                    e.Amount,
                    companyId,
                    userId,
                    trn);

                PostBalancedElementLine(e, jvGuid, empId, req, trn);
            }

            clsJournalVoucherHeader jvCheck = new clsJournalVoucherHeader();
            if (!jvCheck.CheckJVMatch(jvGuid, companyId, trn)) return false;

            clsPayrollHeader hdr = new clsPayrollHeader();
            hdr.MarkAsPosted(empId, periodId, companyId, trn);

            SqlParameter[] upd =
            {
                new SqlParameter("@Guid", SqlDbType.UniqueIdentifier) { Value = Simulate.Guid(payrollHeaderGuid) },
                new SqlParameter("@CompanyID", SqlDbType.Int) { Value = companyId },
                new SqlParameter("@JVGuid", SqlDbType.NVarChar, -1) { Value = jvGuid },
                new SqlParameter("@UserId", SqlDbType.Int) { Value = userId },
            };

            sql.ExecuteNonQueryStatement(@"
UPDATE tbl_PayrollHeader
SET JVGuid = @JVGuid,
    Status = 3,
    IsPosted = 1,
    PostedDate = GETDATE(),
    DocumentStatus = 2,
    PostedByUserId = @UserId,
    ModificationUserID = @UserId,
    ModificationDate = GETDATE()
WHERE Guid = @Guid AND CompanyID = @CompanyID",
                sql.CreateDataBaseConnectionString(companyId), upd, trn);

            new clsJournalVoucherHeader().UpdateDocumentStatus(
                jvGuid, (int)DocumentStatus.Posted, userId, companyId, trn);

            return true;
        }

        public string CancelPayrollPosting_HardDelete(int periodId, int EmployeeID, int companyId)
        {
            clsSQL clsSQL = new clsSQL();
            using (SqlConnection con = new SqlConnection(clsSQL.CreateDataBaseConnectionString(companyId)))
            using (SqlCommand cmd = new SqlCommand())
            {
                cmd.Connection = con;
                cmd.CommandType = CommandType.Text;

                cmd.CommandText = @"

            DECLARE @JVGuid NVARCHAR(100);

            SELECT @JVGuid = JVGuid
            FROM tbl_PayrollHeader
            WHERE PayrollPeriodID = @PeriodID
              AND EmployeeID = @EmployeeID;

            IF (@JVGuid IS NULL OR @JVGuid = '')
            BEGIN
                SELECT 'NO_POSTING_FOUND' AS Status;
                RETURN;
            END

            DELETE FROM tbl_JournalVoucherDetails
            WHERE ParentGuid = @JVGuid;

            DELETE FROM tbl_JournalVoucherHeader
            WHERE Guid = @JVGuid;

            DELETE FROM tbl_PayrollDetails
            WHERE PayrollHeaderID IN (
                SELECT id FROM tbl_PayrollHeader
                WHERE PayrollPeriodID = @PeriodID
                 AND EmployeeID = @EmployeeID
            );

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
    public decimal BasicSalary { get; set; }
    public decimal TotalEarnings { get; set; }
    public decimal TotalDeductions { get; set; }
    public decimal NetSalary { get; set; }
    public bool IsPosted { get; set; }
    public string ValidationError { get; set; }

    public List<PayrollElementRow> Elements { get; set; } = new();
}

public class PayrollElementRow
{
    public int ElementID { get; set; }
    public string ElementName { get; set; }
    public decimal Amount { get; set; }
    public bool IsDeduction { get; set; }
    public int ElementTypeID { get; set; }

    public int DebitAccountID { get; set; }
    public int CreditAccountID { get; set; }

    [Obsolete("Use DebitAccountID / CreditAccountID")]
    public int AccountID
    {
        get => DebitAccountID;
        set => DebitAccountID = value;
    }

    public int BranchID { get; set; }
    public int CostCenterID { get; set; }
}
