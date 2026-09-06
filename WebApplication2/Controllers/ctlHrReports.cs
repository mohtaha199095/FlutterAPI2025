using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System;
using System.Data;
using System.Linq;
using System.Text;
using WebApplication2.cls;

namespace WebApplication2.Controllers
{
    [Route("api/ctlHrReports")]
    public class ctlHrReports : Controller
    {
        readonly clsHrReports _reports = new clsHrReports();

        [HttpGet]
        [Route("GetPayrollRegister")]
        public string GetPayrollRegister(int PayrollPeriodID, int DepartmentID, int CompanyID)
        {
            DataTable dt = _reports.SelectPayrollRegister(PayrollPeriodID, DepartmentID, CompanyID);
            return dt == null ? "[]" : JsonConvert.SerializeObject(dt);
        }

        [HttpGet]
        [Route("GetSscSummary")]
        public string GetSscSummary(int PayrollPeriodID, int CompanyID)
        {
            DataTable dt = _reports.SelectSscSummary(PayrollPeriodID, CompanyID);
            return dt == null ? "[]" : JsonConvert.SerializeObject(dt);
        }

        [HttpGet]
        [Route("GetTaxWithholdingSummary")]
        public string GetTaxWithholdingSummary(int PayrollPeriodID, int CompanyID)
        {
            DataTable dt = _reports.SelectTaxWithholdingSummary(PayrollPeriodID, CompanyID);
            return dt == null ? "[]" : JsonConvert.SerializeObject(dt);
        }

        [HttpGet]
        [Route("GetAttendanceSummary")]
        public string GetAttendanceSummary(DateTime DateFrom, DateTime DateTo, int DepartmentID, int CompanyID)
        {
            DataTable dt = _reports.SelectAttendanceSummary(DateFrom, DateTo, DepartmentID, CompanyID);
            return dt == null ? "[]" : JsonConvert.SerializeObject(dt);
        }

        [HttpGet]
        [Route("GetContractExpiryAlerts")]
        public string GetContractExpiryAlerts(int DaysAhead, int CompanyID)
        {
            DataTable dt = _reports.SelectContractExpiryAlerts(DaysAhead, CompanyID);
            return dt == null ? "[]" : JsonConvert.SerializeObject(dt);
        }

        [HttpGet]
        [Route("GetBankSalaryFile")]
        public IActionResult GetBankSalaryFile(int PayrollPeriodID, int CompanyID)
        {
            try
            {
                string csv = _reports.BuildBankSalaryCsv(PayrollPeriodID, CompanyID);
                byte[] bytes = Encoding.UTF8.GetBytes(csv);
                return File(bytes, "text/csv", $"salary-transfer-{PayrollPeriodID}.csv");
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        [HttpGet]
        [Route("GetPayslipPDF")]
        public IActionResult GetPayslipPDF(int EmployeeID, int PayrollPeriodID, int CompanyID, int UserID = 1)
        {
            try
            {
                byte[] pdf = _reports.BuildPayslipPdf(EmployeeID, PayrollPeriodID, CompanyID, UserID);
                return File(pdf, "application/pdf", $"payslip-{EmployeeID}-{PayrollPeriodID}.pdf");
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        [HttpGet]
        [Route("GetPayrollRegisterPDF")]
        public IActionResult GetPayrollRegisterPDF(int PayrollPeriodID, int DepartmentID, int CompanyID, int UserID = 1)
        {
            try
            {
                DataTable dt = _reports.SelectPayrollRegister(PayrollPeriodID, DepartmentID, CompanyID);
                byte[] pdf = _reports.BuildGenericTablePdf(dt, "Payroll Register / سجل الرواتب", CompanyID, UserID);
                return File(pdf, "application/pdf", $"payroll-register-{PayrollPeriodID}.pdf");
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        [HttpGet]
        [Route("GetSscSummaryPDF")]
        public IActionResult GetSscSummaryPDF(int PayrollPeriodID, int CompanyID, int UserID = 1)
        {
            try
            {
                DataTable dt = _reports.SelectSscSummary(PayrollPeriodID, CompanyID);
                byte[] pdf = _reports.BuildGenericTablePdf(dt, "SSC Summary / ملخص الضمان الاجتماعي", CompanyID, UserID);
                return File(pdf, "application/pdf", $"ssc-summary-{PayrollPeriodID}.pdf");
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        [HttpGet]
        [Route("GetTaxWithholdingPDF")]
        public IActionResult GetTaxWithholdingPDF(int PayrollPeriodID, int CompanyID, int UserID = 1)
        {
            try
            {
                DataTable dt = _reports.SelectTaxWithholdingSummary(PayrollPeriodID, CompanyID);
                byte[] pdf = _reports.BuildGenericTablePdf(dt, "Tax Withholding / ضريبة الدخل", CompanyID, UserID);
                return File(pdf, "application/pdf", $"tax-withholding-{PayrollPeriodID}.pdf");
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        [HttpGet]
        [Route("CalculateEndOfService")]
        public string CalculateEndOfService(int EmployeeID, DateTime TerminationDate, int CompanyID)
        {
            DataTable dt = new clsLeave().CalculateEndOfService(EmployeeID, TerminationDate, CompanyID);
            return dt == null || dt.Rows.Count == 0 ? "{}" : JsonConvert.SerializeObject(dt.Rows[0]);
        }

        [HttpGet]
        [Route("GetDashboardStats")]
        public string GetDashboardStats(int CompanyID)
        {
            DataTable dt = _reports.GetDashboardStats(CompanyID);
            return dt == null || dt.Rows.Count == 0 ? "{}" : JsonConvert.SerializeObject(dt.Rows[0]);
        }

        [HttpGet]
        [Route("GetEmployeeSelfService")]
        public string GetEmployeeSelfService(string UserName, int CompanyID)
        {
            DataTable dt = _reports.SelectEmployeeSelfService(UserName, CompanyID);
            return dt == null || dt.Rows.Count == 0 ? "{}" : JsonConvert.SerializeObject(dt.Rows[0]);
        }

        [HttpGet]
        [Route("GetSscOfficialFile")]
        public IActionResult GetSscOfficialFile(int PayrollPeriodID, int CompanyID)
        {
            try
            {
                string csv = _reports.BuildSscOfficialCsv(PayrollPeriodID, CompanyID);
                byte[] bytes = Encoding.UTF8.GetBytes(csv);
                return File(bytes, "text/csv", $"ssc-official-{PayrollPeriodID}.csv");
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        [HttpGet]
        [Route("GetBankWpsFile")]
        public IActionResult GetBankWpsFile(int PayrollPeriodID, int CompanyID)
        {
            try
            {
                string csv = _reports.BuildBankWpsCsv(PayrollPeriodID, CompanyID);
                byte[] bytes = Encoding.UTF8.GetBytes(csv);
                return File(bytes, "text/csv", $"bank-wps-{PayrollPeriodID}.csv");
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        [HttpPost]
        [Route("PostEndOfServiceSettlement")]
        public IActionResult PostEndOfServiceSettlement(int EmployeeID, DateTime TerminationDate,
            int BranchID, int CompanyID, int UserID = 1)
        {
            try
            {
                string jvGuid = _reports.PostEndOfServiceSettlement(
                    EmployeeID, TerminationDate, BranchID, CompanyID, UserID);
                return Ok(new { success = true, jvGuid });
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        [HttpPost]
        [Route("UpdateEmployeeReportsTo")]
        public IActionResult UpdateEmployeeReportsTo(int EmployeeID, int ReportsToEmployeeID, int CompanyID, int UserID = 1)
        {
            try
            {
                int rows = new clsEmployee().UpdateEmployeeReportsTo(
                    EmployeeID, ReportsToEmployeeID, CompanyID, UserID);
                return Ok(new { success = rows > 0 });
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        [HttpGet]
        [Route("BulkPayslipZip")]
        public IActionResult BulkPayslipZip(int PayrollPeriodID, int DepartmentID = 0, int CompanyID = 0, int UserID = 1)
        {
            try
            {
                byte[] zip = new clsHrReports().BuildBulkPayslipZip(
                    PayrollPeriodID, DepartmentID, CompanyID, UserID);
                if (zip == null || zip.Length == 0)
                    return NotFound(new { error = "No payslips generated." });
                return File(zip, "application/zip", $"payslips-{PayrollPeriodID}.zip");
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        /// <summary>
        /// Full HR QA: payroll golden fixtures, Jordan validators, sick tier, schema, automatic journal QA.
        /// </summary>
        [HttpGet]
        [Route("RunHrQa")]
        public IActionResult RunHrQa(int CompanyID = 0, bool ScanDatabase = true)
        {
            try
            {
                var report = clsHrQaHarness.Run(CompanyID, ScanDatabase);
                return Ok(report);
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        /// <summary>Automatic journal QA only (pattern fixtures + optional DB scan).</summary>
        [HttpGet]
        [Route("RunJournalQa")]
        public IActionResult RunJournalQa(int CompanyID = 0)
        {
            try
            {
                var results = clsHrJournalQa.RunFixtureChecks();
                if (CompanyID > 0)
                    results.AddRange(clsHrJournalQa.RunDatabaseScan(CompanyID));

                return Ok(new
                {
                    allPassed = results.All(r => r.Passed),
                    totalChecks = results.Count,
                    passedChecks = results.Count(r => r.Passed),
                    failedChecks = results.Count(r => !r.Passed),
                    results
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }
    }
}
