using FastReport;
using FastReport.Export.PdfSimple;
using Microsoft.Data.SqlClient;
using System;
using System.Data;
using System.IO;
using System.IO.Compression;
using System.Text;
using WebApplication2.cls.Reports;

namespace WebApplication2.cls
{
    /// <summary>
    /// HR operational and compliance reports (payroll register, SSC, tax, bank file, payslip).
    /// </summary>
    public class clsHrReports
    {
        public DataTable SelectPayrollRegister(int payrollPeriodId, int departmentId, int companyId)
        {
            clsPayrollEngine engine = new clsPayrollEngine();
            return engine.PreviewPayrollAll(payrollPeriodId, departmentId, companyId);
        }

        public DataTable SelectSscSummary(int payrollPeriodId, int companyId)
        {
            clsSQL sql = new clsSQL();
            clsPayrollPeriod pr = new clsPayrollPeriod();
            DateTime startDate, endDate;
            pr.GetPeriodDates(payrollPeriodId, out startDate, out endDate, companyId);

            SqlParameter[] prm =
            {
                new SqlParameter("@CompanyID", SqlDbType.Int) { Value = companyId },
            };
            DataTable employees = sql.ExecuteQueryStatement(@"
SELECT ID AS EmployeeID, AName AS EmployeeName, EmployeeCode,
       ISNULL(SocialSecurityNumber,'') AS SocialSecurityNumber
FROM tbl_employee WHERE CompanyID = @CompanyID AND ISNULL(IsActive,1)=1",
                sql.CreateDataBaseConnectionString(companyId), prm);

            DataTable result = new DataTable();
            result.Columns.Add("EmployeeID", typeof(int));
            result.Columns.Add("EmployeeName");
            result.Columns.Add("EmployeeCode");
            result.Columns.Add("SocialSecurityNumber");
            result.Columns.Add("SSSubjectWage", typeof(decimal));
            result.Columns.Add("SS_EE", typeof(decimal));
            result.Columns.Add("SS_ER", typeof(decimal));
            result.Columns.Add("PeriodStart", typeof(DateTime));
            result.Columns.Add("PeriodEnd", typeof(DateTime));

            clsPayrollEngine engine = new clsPayrollEngine();
            foreach (DataRow emp in employees.Rows)
            {
                int empId = Simulate.Integer32(emp["EmployeeID"]);
                var preview = engine.PreviewPayroll(empId, payrollPeriodId, companyId);
                decimal ssEe = 0, ssEr = 0, subject = 0;
                if (preview.SalaryElements != null)
                {
                    foreach (var d in preview.SalaryElements)
                    {
                        if (string.Equals(d.BasicSalaryCode, "SS_EE", StringComparison.OrdinalIgnoreCase))
                            ssEe = Math.Abs(d.Amount);
                        else if (string.Equals(d.BasicSalaryCode, "SS_ER", StringComparison.OrdinalIgnoreCase))
                            ssEr = Math.Abs(d.Amount);
                        else if (d.IsAffectSocialSecurity)
                            subject += Math.Abs(d.Amount);
                    }
                }

                result.Rows.Add(
                    empId,
                    Simulate.String(emp["EmployeeName"]),
                    Simulate.String(emp["EmployeeCode"]),
                    Simulate.String(emp["SocialSecurityNumber"]),
                    subject,
                    ssEe,
                    ssEr,
                    startDate,
                    endDate);
            }

            return result;
        }

        public DataTable SelectTaxWithholdingSummary(int payrollPeriodId, int companyId)
        {
            clsSQL sql = new clsSQL();
            SqlParameter[] prm =
            {
                new SqlParameter("@CompanyID", SqlDbType.Int) { Value = companyId },
            };
            DataTable employees = sql.ExecuteQueryStatement(@"
SELECT ID AS EmployeeID, AName AS EmployeeName, EmployeeCode,
       ISNULL(NationalNumber,'') AS NationalNumber
FROM tbl_employee WHERE CompanyID = @CompanyID AND ISNULL(IsActive,1)=1",
                sql.CreateDataBaseConnectionString(companyId), prm);

            DataTable result = new DataTable();
            result.Columns.Add("EmployeeID", typeof(int));
            result.Columns.Add("EmployeeName");
            result.Columns.Add("EmployeeCode");
            result.Columns.Add("NationalNumber");
            result.Columns.Add("TaxableIncome", typeof(decimal));
            result.Columns.Add("IncomeTax", typeof(decimal));
            result.Columns.Add("NetSalary", typeof(decimal));

            clsPayrollEngine engine = new clsPayrollEngine();
            foreach (DataRow emp in employees.Rows)
            {
                int empId = Simulate.Integer32(emp["EmployeeID"]);
                var preview = engine.PreviewPayroll(empId, payrollPeriodId, companyId);
                decimal tax = 0, taxable = 0;
                if (preview.SalaryElements != null)
                {
                    foreach (var d in preview.SalaryElements)
                    {
                        if (string.Equals(d.BasicSalaryCode, "TAX_EE", StringComparison.OrdinalIgnoreCase))
                            tax = Math.Abs(d.Amount);
                        else if (d.IsTaxable)
                            taxable += Math.Abs(d.Amount);
                    }
                }

                result.Rows.Add(
                    empId,
                    Simulate.String(emp["EmployeeName"]),
                    Simulate.String(emp["EmployeeCode"]),
                    Simulate.String(emp["NationalNumber"]),
                    taxable,
                    tax,
                    preview.NetSalary);
            }

            return result;
        }

        public string BuildBankSalaryCsv(int payrollPeriodId, int companyId)
        {
            clsSQL sql = new clsSQL();
            SqlParameter[] prm =
            {
                new SqlParameter("@CompanyID", SqlDbType.Int) { Value = companyId },
            };
            DataTable employees = sql.ExecuteQueryStatement(@"
SELECT ID AS EmployeeID, AName AS EmployeeName, EmployeeCode,
       ISNULL(IBAN,'') AS IBAN, ISNULL(BankAccountNumber,'') AS BankAccountNumber,
       ISNULL(NationalNumber,'') AS NationalNumber
FROM tbl_employee WHERE CompanyID = @CompanyID AND ISNULL(IsActive,1)=1",
                sql.CreateDataBaseConnectionString(companyId), prm);

            var sb = new StringBuilder();
            sb.AppendLine("EmployeeCode,EmployeeName,NationalNumber,IBAN,BankAccount,NetSalary");

            clsPayrollEngine engine = new clsPayrollEngine();
            foreach (DataRow emp in employees.Rows)
            {
                int empId = Simulate.Integer32(emp["EmployeeID"]);
                var preview = engine.PreviewPayroll(empId, payrollPeriodId, companyId);
                if (preview.NetSalary <= 0) continue;

                sb.Append(EscapeCsv(Simulate.String(emp["EmployeeCode"]))).Append(',');
                sb.Append(EscapeCsv(Simulate.String(emp["EmployeeName"]))).Append(',');
                sb.Append(EscapeCsv(Simulate.String(emp["NationalNumber"]))).Append(',');
                sb.Append(EscapeCsv(Simulate.String(emp["IBAN"]))).Append(',');
                sb.Append(EscapeCsv(Simulate.String(emp["BankAccountNumber"]))).Append(',');
                sb.Append(preview.NetSalary.ToString("0.000")).AppendLine();
            }

            return sb.ToString();
        }

        public DataTable SelectAttendanceSummary(DateTime dateFrom, DateTime dateTo, int departmentId, int companyId)
        {
            clsSQL sql = new clsSQL();
            SqlParameter[] prm =
            {
                new SqlParameter("@DateFrom", SqlDbType.DateTime) { Value = dateFrom.Date },
                new SqlParameter("@DateTo", SqlDbType.DateTime) { Value = dateTo.Date },
                new SqlParameter("@DepartmentID", SqlDbType.Int) { Value = departmentId },
                new SqlParameter("@CompanyID", SqlDbType.Int) { Value = companyId },
            };
            return sql.ExecuteQueryStatement(@"
SELECT e.ID AS EmployeeID, e.AName AS EmployeeName, e.EmployeeCode,
       COUNT(d.ID) AS DaysRecorded,
       SUM(ISNULL(d.WorkedMinutes,0)) AS TotalWorkedMinutes,
       SUM(ISNULL(d.LateMinutes,0)) AS TotalLateMinutes,
       SUM(ISNULL(d.OvertimeMinutes,0)) AS TotalOvertimeMinutes,
       SUM(CASE WHEN ISNULL(d.StatusID,0)=2 THEN 1 ELSE 0 END) AS AbsentDays,
       SUM(CASE WHEN ISNULL(d.StatusID,0)=3 THEN 1 ELSE 0 END) AS LeaveDays
FROM tbl_employee e
LEFT JOIN tbl_AttendanceDay d ON d.EmployeeID = e.ID AND d.CompanyID = e.CompanyID
  AND CAST(d.WorkDate AS DATE) BETWEEN @DateFrom AND @DateTo
WHERE e.CompanyID = @CompanyID
  AND (e.DepartmentID = @DepartmentID OR @DepartmentID = 0)
GROUP BY e.ID, e.AName, e.EmployeeCode
ORDER BY e.AName",
                sql.CreateDataBaseConnectionString(companyId), prm);
        }

        public DataTable SelectContractExpiryAlerts(int daysAhead, int companyId)
        {
            clsSQL sql = new clsSQL();
            SqlParameter[] prm =
            {
                new SqlParameter("@DaysAhead", SqlDbType.Int) { Value = daysAhead },
                new SqlParameter("@CompanyID", SqlDbType.Int) { Value = companyId },
            };
            return sql.ExecuteQueryStatement(@"
SELECT c.ID, c.ContractNumber, c.EmployeeID, e.AName AS EmployeeName,
       c.StartDate, c.EndDate, c.IsOpenEnded,
       e.PassportExpireDate
FROM tbl_EmployeeContract c
INNER JOIN tbl_employee e ON e.ID = c.EmployeeID AND e.CompanyID = c.CompanyID
WHERE c.CompanyID = @CompanyID AND ISNULL(c.IsActive,0)=1
  AND (
    (ISNULL(c.IsOpenEnded,0)=0 AND c.EndDate IS NOT NULL AND c.EndDate <= DATEADD(day, @DaysAhead, GETDATE()))
    OR (e.PassportExpireDate IS NOT NULL AND e.PassportExpireDate <= DATEADD(day, @DaysAhead, GETDATE()))
  )
ORDER BY c.EndDate, e.PassportExpireDate",
                sql.CreateDataBaseConnectionString(companyId), prm);
        }

        public byte[] BuildPayslipPdf(int employeeId, int payrollPeriodId, int companyId, int userId = 1)
        {
            clsPayrollEngine engine = new clsPayrollEngine();
            var preview = engine.PreviewPayroll(employeeId, payrollPeriodId, companyId);

            clsSQL sql = new clsSQL();
            SqlParameter[] prm =
            {
                new SqlParameter("@EmployeeID", SqlDbType.Int) { Value = employeeId },
                new SqlParameter("@CompanyID", SqlDbType.Int) { Value = companyId },
                new SqlParameter("@PeriodID", SqlDbType.Int) { Value = payrollPeriodId },
            };

            DataTable header = sql.ExecuteQueryStatement(@"
SELECT e.AName AS EmployeeName, e.EmployeeCode AS EmpCode, e.NationalNumber, e.SocialSecurityNumber,
       e.IBAN, dep.AName AS DepartmentName,
       pp.AName AS PeriodName, pp.StartDate AS PeriodStart, pp.EndDate AS PeriodEnd,
       CAST('' AS NVARCHAR(200)) AS CompanyName
FROM tbl_employee e
LEFT JOIN tbl_Department dep ON dep.ID = e.DepartmentID
LEFT JOIN tbl_PayrollPeriod pp ON pp.ID = @PeriodID AND pp.CompanyID = @CompanyID
WHERE e.ID = @EmployeeID AND e.CompanyID = @CompanyID",
                sql.CreateDataBaseConnectionString(companyId), prm);

            if (header == null || header.Rows.Count == 0)
                throw new Exception("Employee not found.");

            DataRow h = header.Rows[0];
            h["CompanyName"] = ResolveCompanyName(companyId);
            header.Columns.Add("BasicSalary", typeof(decimal));
            header.Columns.Add("TotalEarnings", typeof(decimal));
            header.Columns.Add("TotalDeductions", typeof(decimal));
            header.Columns.Add("NetSalary", typeof(decimal));
            header.Columns.Add("EmployerContributions", typeof(decimal));
            h["BasicSalary"] = preview.BasicSalary;
            h["TotalEarnings"] = preview.TotalEarnings;
            h["TotalDeductions"] = preview.TotalDeductions;
            h["NetSalary"] = preview.NetSalary;
            h["EmployerContributions"] = preview.EmployerContributions;
            header.TableName = "PayslipHeader";

            DataTable details = new DataTable("PayslipDetails");
            details.Columns.Add("ElementName");
            details.Columns.Add("Amount", typeof(decimal));
            details.Columns.Add("ElementType");
            details.Columns.Add("Code");

            if (preview.SalaryElements != null)
            {
                foreach (var d in preview.SalaryElements)
                {
                    if (d.Amount == 0) continue;
                    string typeLabel = d.ElementTypeID == clsPayrollEngine.ElementTypeEarning ? "Earning"
                        : d.ElementTypeID == clsPayrollEngine.ElementTypeDeduction ? "Deduction"
                        : "Employer";
                    details.Rows.Add(d.ElementName, d.Amount, typeLabel, d.BasicSalaryCode ?? "");
                }
            }

            if (preview.AttendanceElements != null)
            {
                foreach (var a in preview.AttendanceElements)
                {
                    if (a.Amount == 0) continue;
                    details.Rows.Add(a.ElementName, a.Amount, "Attendance", a.Code ?? "");
                }
            }

            System.Data.DataSet ds = new System.Data.DataSet("PayslipData");
            ds.Tables.Add(header.Copy());
            ds.Tables.Add(details.Copy());

            clsReports helper = new clsReports();
            string frxPath = helper.getMyPath("rptPayslip", companyId);
            if (!File.Exists(frxPath))
                throw new FileNotFoundException($"Payslip template not found: {frxPath}");

            FastReport.Utils.Config.WebMode = true;
            Report report = new Report();
            report.RegisterData(ds);
            report.Load(frxPath);
            report.Prepare();

            using MemoryStream ms = new MemoryStream();
            report.Export(new PDFSimpleExport(), ms);
            return ms.ToArray();
        }

        public byte[] BuildGenericTablePdf(DataTable table, string title, int companyId, int userId)
        {
            table.TableName = "ReportData";
            System.Data.DataSet ds = new System.Data.DataSet("HrReport");
            ds.Tables.Add(table.Copy());

            FastReport.Utils.Config.WebMode = true;
            Report report = new Report();
            report.RegisterData(ds);

            report.ReportInfo.Name = title;
            report.LoadFromString(BuildSimpleTableFrx(title));
            report.Prepare();

            using MemoryStream ms = new MemoryStream();
            report.Export(new PDFSimpleExport(), ms);
            return ms.ToArray();
        }

        static string BuildSimpleTableFrx(string title)
        {
            return $@"<?xml version=""1.0"" encoding=""utf-8""?>
<Report ScriptLanguage=""CSharp"">
  <Dictionary/>
  <ReportPage Name=""Page1"" PaperWidth=""210"" PaperHeight=""297"">
    <ReportTitleBand Name=""Title1"" Width=""718.2"" Height=""47.25"">
      <TextObject Name=""TxtTitle"" Width=""718.2"" Height=""37.8"" Text=""{title}"" HorzAlign=""Center"" Font=""Arial, 14pt, style=Bold""/>
    </ReportTitleBand>
    <DataBand Name=""Data1"" Top=""51.03"" Width=""718.2"" Height=""18.9"" DataSource=""ReportData"">
      <TextObject Name=""TxtRow"" Width=""718.2"" Height=""18.9"" Text=""[ReportData]"" Font=""Arial, 9pt""/>
    </DataBand>
  </ReportPage>
</Report>";
        }

        static string EscapeCsv(string value)
        {
            if (string.IsNullOrEmpty(value)) return "";
            if (value.Contains(',') || value.Contains('"') || value.Contains('\n'))
                return "\"" + value.Replace("\"", "\"\"") + "\"";
            return value;
        }

        public static string ResolveCompanyName(int companyId)
        {
            try
            {
                clsSQL sql = new clsSQL();
                SqlParameter[] prm =
                {
                    new SqlParameter("@ID", SqlDbType.Int) { Value = companyId },
                };
                object val = sql.ExecuteScalar(
                    "SELECT TOP 1 ISNULL(AName, EName) FROM tbl_Company WHERE ID=@ID",
                    prm, sql.MainDataBaseconString, null);
                return Simulate.String(val);
            }
            catch
            {
                return "";
            }
        }

        public DataTable GetDashboardStats(int companyId)
        {
            clsSQL sql = new clsSQL();
            SqlParameter[] prm =
            {
                new SqlParameter("@CompanyID", SqlDbType.Int) { Value = companyId },
            };

            DataTable dt = new DataTable();
            dt.Columns.Add("ActiveEmployees", typeof(int));
            dt.Columns.Add("PendingLeaveRequests", typeof(int));
            dt.Columns.Add("PendingApprovals", typeof(int));
            dt.Columns.Add("ContractsExpiring90Days", typeof(int));
            dt.Columns.Add("CompanyName");

            object active = sql.ExecuteScalar(@"
SELECT COUNT(*) FROM tbl_employee
WHERE CompanyID=@CompanyID AND ISNULL(IsActive,1)=1",
                prm, sql.CreateDataBaseConnectionString(companyId), null);

            object pendingLeave = sql.ExecuteScalar(@"
SELECT COUNT(*) FROM tbl_LeaveRequest
WHERE CompanyID=@CompanyID AND ISNULL(DocumentStatus,0) IN (0,1)",
                prm, sql.CreateDataBaseConnectionString(companyId), null);

            object pendingApproval = sql.ExecuteScalar(@"
SELECT COUNT(*) FROM tbl_ApprovalRequest
WHERE CompanyID=@CompanyID AND ISNULL(Status,0)=0",
                prm, sql.CreateDataBaseConnectionString(companyId), null);

            object expiring = sql.ExecuteScalar(@"
SELECT COUNT(*) FROM tbl_EmployeeContract c
WHERE c.CompanyID=@CompanyID AND ISNULL(c.IsActive,0)=1
  AND ISNULL(c.IsOpenEnded,0)=0 AND c.EndDate IS NOT NULL
  AND c.EndDate <= DATEADD(day, 90, GETDATE())",
                prm, sql.CreateDataBaseConnectionString(companyId), null);

            dt.Rows.Add(
                Simulate.Integer32(active),
                Simulate.Integer32(pendingLeave),
                Simulate.Integer32(pendingApproval),
                Simulate.Integer32(expiring),
                ResolveCompanyName(companyId));
            return dt;
        }

        public string BuildSscOfficialCsv(int payrollPeriodId, int companyId)
        {
            DataTable summary = SelectSscSummary(payrollPeriodId, companyId);
            string companyName = ResolveCompanyName(companyId);
            clsPayrollPeriod pr = new clsPayrollPeriod();
            pr.GetPeriodDates(payrollPeriodId, out DateTime startDate, out DateTime endDate, companyId);

            var sb = new StringBuilder();
            sb.AppendLine("CompanyName,PeriodStart,PeriodEnd,EmployeeCode,EmployeeName,SocialSecurityNumber,SSSubjectWage,EmployeeContribution,EmployerContribution");
            foreach (DataRow row in summary.Rows)
            {
                sb.Append(EscapeCsv(companyName)).Append(',');
                sb.Append(startDate.ToString("yyyy-MM-dd")).Append(',');
                sb.Append(endDate.ToString("yyyy-MM-dd")).Append(',');
                sb.Append(EscapeCsv(Simulate.String(row["EmployeeCode"]))).Append(',');
                sb.Append(EscapeCsv(Simulate.String(row["EmployeeName"]))).Append(',');
                sb.Append(EscapeCsv(Simulate.String(row["SocialSecurityNumber"]))).Append(',');
                sb.Append(Simulate.Decimal(row["SSSubjectWage"]).ToString("0.000")).Append(',');
                sb.Append(Simulate.Decimal(row["SS_EE"]).ToString("0.000")).Append(',');
                sb.Append(Simulate.Decimal(row["SS_ER"]).ToString("0.000")).AppendLine();
            }
            return sb.ToString();
        }

        public string BuildBankWpsCsv(int payrollPeriodId, int companyId)
        {
            clsSQL sql = new clsSQL();
            string companyName = ResolveCompanyName(companyId);
            clsPayrollPeriod pr = new clsPayrollPeriod();
            pr.GetPeriodDates(payrollPeriodId, out DateTime startDate, out DateTime endDate, companyId);

            SqlParameter[] prm =
            {
                new SqlParameter("@CompanyID", SqlDbType.Int) { Value = companyId },
            };
            DataTable employees = sql.ExecuteQueryStatement(@"
SELECT ID AS EmployeeID, AName AS EmployeeName, EmployeeCode,
       ISNULL(IBAN,'') AS IBAN, ISNULL(BankAccountNumber,'') AS BankAccountNumber,
       ISNULL(NationalNumber,'') AS NationalNumber, ISNULL(BankName,'') AS BankName
FROM tbl_employee WHERE CompanyID = @CompanyID AND ISNULL(IsActive,1)=1",
                sql.CreateDataBaseConnectionString(companyId), prm);

            var sb = new StringBuilder();
            sb.AppendLine("RecordType,CompanyName,PayPeriodStart,PayPeriodEnd,EmployeeCode,EmployeeName,NationalID,IBAN,BankAccount,BankName,NetSalary,Currency");
            clsPayrollEngine engine = new clsPayrollEngine();
            foreach (DataRow emp in employees.Rows)
            {
                int empId = Simulate.Integer32(emp["EmployeeID"]);
                var preview = engine.PreviewPayroll(empId, payrollPeriodId, companyId);
                if (preview.NetSalary <= 0) continue;

                sb.Append("SAL,");
                sb.Append(EscapeCsv(companyName)).Append(',');
                sb.Append(startDate.ToString("yyyy-MM-dd")).Append(',');
                sb.Append(endDate.ToString("yyyy-MM-dd")).Append(',');
                sb.Append(EscapeCsv(Simulate.String(emp["EmployeeCode"]))).Append(',');
                sb.Append(EscapeCsv(Simulate.String(emp["EmployeeName"]))).Append(',');
                sb.Append(EscapeCsv(Simulate.String(emp["NationalNumber"]))).Append(',');
                sb.Append(EscapeCsv(Simulate.String(emp["IBAN"]))).Append(',');
                sb.Append(EscapeCsv(Simulate.String(emp["BankAccountNumber"]))).Append(',');
                sb.Append(EscapeCsv(Simulate.String(emp["BankName"]))).Append(',');
                sb.Append(preview.NetSalary.ToString("0.000")).Append(',');
                sb.Append("JOD").AppendLine();
            }
            return sb.ToString();
        }

        public DataTable SelectEmployeeSelfService(string userName, int companyId)
        {
            clsSQL sql = new clsSQL();
            SqlParameter[] prm =
            {
                new SqlParameter("@UserName", SqlDbType.NVarChar, -1) { Value = userName ?? "" },
                new SqlParameter("@CompanyID", SqlDbType.Int) { Value = companyId },
            };
            return sql.ExecuteQueryStatement(@"
SELECT TOP 1 e.ID AS EmployeeID, e.AName AS EmployeeName, e.EmployeeCode,
       ISNULL(e.ReportsToEmployeeID,0) AS ReportsToEmployeeID,
       mgr.AName AS ManagerName
FROM tbl_employee e
LEFT JOIN tbl_employee mgr ON mgr.ID = e.ReportsToEmployeeID AND mgr.CompanyID = e.CompanyID
WHERE e.CompanyID = @CompanyID AND e.UserName = @UserName AND ISNULL(e.IsActive,1)=1",
                sql.CreateDataBaseConnectionString(companyId), prm);
        }

        public string PostEndOfServiceSettlement(int employeeId, DateTime terminationDate, int branchId,
            int companyId, int userId)
        {
            DataTable eos = new clsLeave().CalculateEndOfService(employeeId, terminationDate, companyId);
            if (eos == null || eos.Rows.Count == 0)
                throw new Exception("Employee not found.");

            DataRow row = eos.Rows[0];
            decimal amount = Simulate.Decimal(row["EstimatedEOS"]);
            if (amount <= 0)
                throw new Exception("Estimated end-of-service amount is zero.");

            cls_AccountSetting settings = new cls_AccountSetting();
            DataTable payableSetting = settings.SelectAccountSetting(
                0, (int)MainClasses.clsEnum.AccountMainSetting.Employees, companyId);
            int payableAccountId = payableSetting != null && payableSetting.Rows.Count > 0
                ? Simulate.Integer32(payableSetting.Rows[0]["AccountID"])
                : 0;

            clsEmployeeContract contractSvc = new clsEmployeeContract();
            int basicElementId = contractSvc.GetBasicSalaryElementID(companyId);
            int expenseAccountId = 0;
            if (basicElementId > 0)
            {
                clsSQL sql = new clsSQL();
                SqlParameter[] prm =
                {
                    new SqlParameter("@ID", SqlDbType.Int) { Value = basicElementId },
                    new SqlParameter("@CompanyID", SqlDbType.Int) { Value = companyId },
                };
                object val = sql.ExecuteScalar(
                    "SELECT TOP 1 ISNULL(CompanyDebitAccountID,0) FROM tbl_SalariesElements WHERE ID=@ID AND CompanyID=@CompanyID",
                    prm, sql.CreateDataBaseConnectionString(companyId), null);
                expenseAccountId = Simulate.Integer32(val);
            }

            if (payableAccountId <= 0 || expenseAccountId <= 0)
                throw new Exception("Configure employee payable and payroll expense GL accounts before posting EOS.");

            clsJournalVoucherHeader jvh = new clsJournalVoucherHeader();
            string jvGuid = jvh.InsertJournalVoucherHeader(
                branchId,
                0,
                $"EOS settlement — Employee {employeeId}",
                "",
                (int)MainClasses.clsEnum.VoucherType.Payroll,
                companyId,
                terminationDate,
                userId,
                "",
                0);

            clsJournalVoucherDetails jvd = new clsJournalVoucherDetails();
            jvd.InsertJournalVoucherDetails(
                jvGuid, 1, expenseAccountId, 0, amount, 0, amount,
                1, 1, amount, branchId, 0, terminationDate,
                "EOS expense", companyId, userId, "");
            jvd.InsertJournalVoucherDetails(
                jvGuid, 2, payableAccountId, employeeId, 0, amount, -amount,
                1, 1, amount, branchId, 0, terminationDate,
                "EOS payable", companyId, userId, "");

            clsJournalVoucherHeader check = new clsJournalVoucherHeader();
            if (!check.CheckJVMatch(jvGuid, companyId, null))
                throw new Exception("EOS journal voucher is not balanced.");

            return jvGuid;
        }

        public byte[] BuildBulkPayslipZip(int payrollPeriodId, int departmentId, int companyId, int userId = 1)
        {
            clsSQL sql = new clsSQL();
            SqlParameter[] prm =
            {
                new SqlParameter("@CompanyID", SqlDbType.Int) { Value = companyId },
                new SqlParameter("@DepartmentID", SqlDbType.Int) { Value = departmentId },
            };
            DataTable employees = sql.ExecuteQueryStatement(@"
SELECT ID AS EmployeeID FROM tbl_employee
WHERE CompanyID=@CompanyID AND ISNULL(IsActive,1)=1
  AND (@DepartmentID=0 OR DepartmentID=@DepartmentID)
ORDER BY AName",
                sql.CreateDataBaseConnectionString(companyId), prm);

            using MemoryStream zipStream = new MemoryStream();
            using (ZipArchive archive = new ZipArchive(zipStream, ZipArchiveMode.Create, true))
            {
                foreach (DataRow row in employees.Rows)
                {
                    int empId = Simulate.Integer32(row["EmployeeID"]);
                    try
                    {
                        byte[] pdf = BuildPayslipPdf(empId, payrollPeriodId, companyId, userId);
                        if (pdf == null || pdf.Length == 0) continue;
                        ZipArchiveEntry entry = archive.CreateEntry($"payslip-{empId}-{payrollPeriodId}.pdf");
                        using Stream entryStream = entry.Open();
                        entryStream.Write(pdf, 0, pdf.Length);
                    }
                    catch
                    {
                        // skip employees without valid payslip data
                    }
                }
            }
            return zipStream.ToArray();
        }

        public int GetMaxDailyOvertimeMinutes(int companyId)
        {
            clsSQL sql = new clsSQL();
            object val = sql.ExecuteScalar(
                "SELECT TOP 1 ISNULL(MaxDailyOvertimeMinutes, 120) FROM tbl_Company",
                null, sql.CreateDataBaseConnectionString(companyId), null);
            int minutes = Simulate.Integer32(val);
            return minutes > 0 ? minutes : 120;
        }

        public void UpdateMaxDailyOvertimeMinutes(int companyId, int minutes)
        {
            if (minutes <= 0) minutes = 120;
            clsSQL sql = new clsSQL();
            SqlParameter[] prm =
            {
                new SqlParameter("@Minutes", SqlDbType.Int) { Value = minutes },
            };
            sql.ExecuteNonQueryStatement(
                "UPDATE tbl_Company SET MaxDailyOvertimeMinutes=@Minutes",
                sql.CreateDataBaseConnectionString(companyId), prm);
        }
    }
}
