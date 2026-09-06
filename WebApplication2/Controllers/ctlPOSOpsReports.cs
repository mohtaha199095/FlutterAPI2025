using FastReport.Export.PdfSimple;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System;
using System.Data;
using System.IO;
using WebApplication2.cls;
using WebApplication2.cls.Reports;

namespace WebApplication2.Controllers
{
    [Route("api/ctlPOSOpsReports")]
    public class ctlPOSOpsReports : Controller
    {
        [HttpGet]
        [Route("GetXZReport")]
        public string GetXZReport(
            string ReportType,
            DateTime Date1, DateTime Date2, bool UseDateFilter,
            int BranchID, int CashID, int FilterUserID,
            string POSDayGuid, string POSSessionGuid, int CompanyID)
        {
            clsPOSOpsReports reports = new clsPOSOpsReports();
            DataTable summary = reports.SelectXZReport(
                ReportType, Date1, Date2, UseDateFilter,
                BranchID, CashID, FilterUserID,
                POSDayGuid ?? "", POSSessionGuid ?? "", CompanyID);
            DataTable payments = reports.SelectPaymentBreakdown(
                Date1, Date2, UseDateFilter,
                BranchID, CashID, FilterUserID,
                POSDayGuid ?? "", POSSessionGuid ?? "", CompanyID);

            var result = new
            {
                Summary = summary,
                Payments = payments,
            };
            return JsonConvert.SerializeObject(result);
        }

        [HttpGet]
        [Route("GetSalesByCashier")]
        public string GetSalesByCashier(
            DateTime Date1, DateTime Date2, bool UseDateFilter,
            int BranchID, int CashID, int FilterUserID,
            string POSDayGuid, string POSSessionGuid, int CompanyID)
        {
            clsPOSOpsReports reports = new clsPOSOpsReports();
            DataTable dt = reports.SelectSalesByCashier(
                Date1, Date2, UseDateFilter,
                BranchID, CashID, FilterUserID,
                POSDayGuid ?? "", POSSessionGuid ?? "", CompanyID);
            return JsonConvert.SerializeObject(dt);
        }

        [HttpGet]
        [Route("GetSalesByHour")]
        public string GetSalesByHour(
            DateTime Date1, DateTime Date2, bool UseDateFilter,
            int BranchID, int CashID, int FilterUserID,
            string POSDayGuid, string POSSessionGuid, int CompanyID)
        {
            clsPOSOpsReports reports = new clsPOSOpsReports();
            DataTable dt = reports.SelectSalesByHour(
                Date1, Date2, UseDateFilter,
                BranchID, CashID, FilterUserID,
                POSDayGuid ?? "", POSSessionGuid ?? "", CompanyID);
            return JsonConvert.SerializeObject(dt);
        }

        [HttpGet]
        [Route("GetSalesByCategory")]
        public string GetSalesByCategory(
            DateTime Date1, DateTime Date2, bool UseDateFilter,
            int BranchID, int CashID, int FilterUserID,
            string POSDayGuid, string POSSessionGuid, int CompanyID)
        {
            clsPOSOpsReports reports = new clsPOSOpsReports();
            DataTable dt = reports.SelectSalesByCategory(
                Date1, Date2, UseDateFilter,
                BranchID, CashID, FilterUserID,
                POSDayGuid ?? "", POSSessionGuid ?? "", CompanyID);
            return JsonConvert.SerializeObject(dt);
        }

        [HttpGet]
        [Route("GetAuditReport")]
        public string GetAuditReport(
            DateTime Date1, DateTime Date2, bool UseDateFilter,
            int BranchID, int CashID, int FilterUserID,
            string POSDayGuid, string POSSessionGuid, int CompanyID)
        {
            clsPOSOpsReports reports = new clsPOSOpsReports();
            DataTable dt = reports.SelectAuditReport(
                Date1, Date2, UseDateFilter,
                BranchID, CashID, FilterUserID,
                POSDayGuid ?? "", POSSessionGuid ?? "", CompanyID);
            return JsonConvert.SerializeObject(dt);
        }

        [HttpGet]
        [Route("SaveCashCount")]
        public string SaveCashCount(
            string Scope, string Guid,
            decimal OpeningFloat, decimal CountedCash, decimal ExpectedCash,
            string ClosingNote, int ModificationUserId, int CompanyID)
        {
            clsPOSOpsReports reports = new clsPOSOpsReports();
            bool ok = reports.SaveCashCount(
                Scope ?? "Day", Guid ?? "",
                OpeningFloat, CountedCash, ExpectedCash,
                ClosingNote ?? "", ModificationUserId, CompanyID);
            return JsonConvert.SerializeObject(ok);
        }

        [HttpGet]
        [Route("LogAuditEvent")]
        public string LogAuditEvent(
            string EventType, string InvoiceGuid, string InvoiceNo,
            int CashDrawerID, string POSDayGuid, string POSSessionGuid,
            decimal Amount, string Details, int CreationUserID, int CompanyID)
        {
            clsPOSOpsReports reports = new clsPOSOpsReports();
            bool ok = reports.LogAuditEvent(
                EventType ?? "", InvoiceGuid ?? "", InvoiceNo ?? "",
                CashDrawerID, POSDayGuid ?? "", POSSessionGuid ?? "",
                Amount, Details ?? "", CreationUserID, CompanyID);
            return JsonConvert.SerializeObject(ok);
        }

        #region PDF

        [HttpGet]
        [Route("GetXZReportPDF")]
        public IActionResult GetXZReportPDF(
            string ReportType,
            DateTime Date1, DateTime Date2, bool UseDateFilter,
            int BranchID, int CashID, int FilterUserID,
            string POSDayGuid, string POSSessionGuid,
            int UserId, int CompanyID)
        {
            try
            {
                FastReport.Utils.Config.WebMode = true;
                clsPOSOpsReports dal = new clsPOSOpsReports();
                DataTable summary = dal.SelectXZReport(
                    ReportType, Date1, Date2, UseDateFilter,
                    BranchID, CashID, FilterUserID,
                    POSDayGuid ?? "", POSSessionGuid ?? "", CompanyID);
                DataTable payments = dal.SelectPaymentBreakdown(
                    Date1, Date2, UseDateFilter,
                    BranchID, CashID, FilterUserID,
                    POSDayGuid ?? "", POSSessionGuid ?? "", CompanyID);

                summary.TableName = "Summary";
                payments.TableName = "Payments";
                System.Data.DataSet ds = new System.Data.DataSet();
                ds.Tables.Add(summary.Copy());
                ds.Tables.Add(payments.Copy());

                string reportType = string.IsNullOrWhiteSpace(ReportType)
                    ? "X"
                    : ReportType.Trim().ToUpperInvariant();
                string title = reportType == "Z"
                    ? "Z Report (Day-End) / تقرير Z"
                    : "X Report (Live) / تقرير X";

                return BuildPosOpsPdf(
                    ds,
                    clsTransactionReportDefaults.PagePOSXZ,
                    "rptPOSXZ",
                    title,
                    Date1, Date2, UseDateFilter,
                    BranchID, CashID, FilterUserID,
                    reportType,
                    UserId, CompanyID);
            }
            catch (Exception ex)
            {
                return BadRequest("Print error: " + ex.Message);
            }
        }

        [HttpGet]
        [Route("GetSalesByCashierPDF")]
        public IActionResult GetSalesByCashierPDF(
            DateTime Date1, DateTime Date2, bool UseDateFilter,
            int BranchID, int CashID, int FilterUserID,
            string POSDayGuid, string POSSessionGuid,
            int UserId, int CompanyID)
        {
            try
            {
                FastReport.Utils.Config.WebMode = true;
                clsPOSOpsReports dal = new clsPOSOpsReports();
                DataTable dt = dal.SelectSalesByCashier(
                    Date1, Date2, UseDateFilter,
                    BranchID, CashID, FilterUserID,
                    POSDayGuid ?? "", POSSessionGuid ?? "", CompanyID);
                dt.TableName = "SalesByCashier";
                System.Data.DataSet ds = new System.Data.DataSet();
                ds.Tables.Add(dt.Copy());

                return BuildPosOpsPdf(
                    ds,
                    clsTransactionReportDefaults.PagePOSSalesByCashier,
                    "rptPOSSalesByCashier",
                    "Sales by Cashier / المبيعات حسب الكاشير",
                    Date1, Date2, UseDateFilter,
                    BranchID, CashID, FilterUserID,
                    "",
                    UserId, CompanyID);
            }
            catch (Exception ex)
            {
                return BadRequest("Print error: " + ex.Message);
            }
        }

        [HttpGet]
        [Route("GetSalesByHourPDF")]
        public IActionResult GetSalesByHourPDF(
            DateTime Date1, DateTime Date2, bool UseDateFilter,
            int BranchID, int CashID, int FilterUserID,
            string POSDayGuid, string POSSessionGuid,
            int UserId, int CompanyID)
        {
            try
            {
                FastReport.Utils.Config.WebMode = true;
                clsPOSOpsReports dal = new clsPOSOpsReports();
                DataTable dt = dal.SelectSalesByHour(
                    Date1, Date2, UseDateFilter,
                    BranchID, CashID, FilterUserID,
                    POSDayGuid ?? "", POSSessionGuid ?? "", CompanyID);
                dt.TableName = "SalesByHour";
                System.Data.DataSet ds = new System.Data.DataSet();
                ds.Tables.Add(dt.Copy());

                return BuildPosOpsPdf(
                    ds,
                    clsTransactionReportDefaults.PagePOSSalesByHour,
                    "rptPOSSalesByHour",
                    "Sales by Hour / المبيعات حسب الساعة",
                    Date1, Date2, UseDateFilter,
                    BranchID, CashID, FilterUserID,
                    "",
                    UserId, CompanyID);
            }
            catch (Exception ex)
            {
                return BadRequest("Print error: " + ex.Message);
            }
        }

        [HttpGet]
        [Route("GetSalesByCategoryPDF")]
        public IActionResult GetSalesByCategoryPDF(
            DateTime Date1, DateTime Date2, bool UseDateFilter,
            int BranchID, int CashID, int FilterUserID,
            string POSDayGuid, string POSSessionGuid,
            int UserId, int CompanyID)
        {
            try
            {
                FastReport.Utils.Config.WebMode = true;
                clsPOSOpsReports dal = new clsPOSOpsReports();
                DataTable dt = dal.SelectSalesByCategory(
                    Date1, Date2, UseDateFilter,
                    BranchID, CashID, FilterUserID,
                    POSDayGuid ?? "", POSSessionGuid ?? "", CompanyID);
                dt.TableName = "SalesByCategory";
                System.Data.DataSet ds = new System.Data.DataSet();
                ds.Tables.Add(dt.Copy());

                return BuildPosOpsPdf(
                    ds,
                    clsTransactionReportDefaults.PagePOSSalesByCategory,
                    "rptPOSSalesByCategory",
                    "Sales by Category / المبيعات حسب التصنيف",
                    Date1, Date2, UseDateFilter,
                    BranchID, CashID, FilterUserID,
                    "",
                    UserId, CompanyID);
            }
            catch (Exception ex)
            {
                return BadRequest("Print error: " + ex.Message);
            }
        }

        [HttpGet]
        [Route("GetAuditReportPDF")]
        public IActionResult GetAuditReportPDF(
            DateTime Date1, DateTime Date2, bool UseDateFilter,
            int BranchID, int CashID, int FilterUserID,
            string POSDayGuid, string POSSessionGuid,
            int UserId, int CompanyID)
        {
            try
            {
                FastReport.Utils.Config.WebMode = true;
                clsPOSOpsReports dal = new clsPOSOpsReports();
                DataTable dt = dal.SelectAuditReport(
                    Date1, Date2, UseDateFilter,
                    BranchID, CashID, FilterUserID,
                    POSDayGuid ?? "", POSSessionGuid ?? "", CompanyID);
                dt.TableName = "AuditReport";
                System.Data.DataSet ds = new System.Data.DataSet();
                ds.Tables.Add(dt.Copy());

                return BuildPosOpsPdf(
                    ds,
                    clsTransactionReportDefaults.PagePOSAudit,
                    "rptPOSAudit",
                    "Voids / Refunds / Discounts Audit / تدقيق الإلغاء والمرتجعات والخصومات",
                    Date1, Date2, UseDateFilter,
                    BranchID, CashID, FilterUserID,
                    "",
                    UserId, CompanyID);
            }
            catch (Exception ex)
            {
                return BadRequest("Print error: " + ex.Message);
            }
        }

        private IActionResult BuildPosOpsPdf(
            System.Data.DataSet data,
            string pageName,
            string frxFileName,
            string title,
            DateTime date1,
            DateTime date2,
            bool useDateFilter,
            int branchId,
            int cashId,
            int filterUserId,
            string reportType,
            int userId,
            int companyId)
        {
            FastReport.Report report = new FastReport.Report();
            clsReports repHelper = new clsReports();
            repHelper.LoadCompanyFastReport(report, pageName, frxFileName, companyId, userId);

            // POS .frx files contain bare TableDataSource nodes; bind live tables onto them.
            System.Data.DataSet anon = new System.Data.DataSet();
            foreach (System.Data.DataTable table in data.Tables)
            {
                if (table == null || string.IsNullOrWhiteSpace(table.TableName))
                    continue;
                if (anon.Tables.Contains(table.TableName))
                    continue;
                System.Data.DataTable copy = table.Copy();
                copy.TableName = table.TableName;
                anon.Tables.Add(copy);
            }

            try { report.RegisterData(anon); } catch { }

            foreach (System.Data.DataTable table in anon.Tables)
            {
                try { report.RegisterData(table, table.TableName); } catch { }

                object src = null;
                try { src = report.GetDataSource(table.TableName); } catch { }
                if (src == null)
                    continue;

                try
                {
                    src.GetType().GetProperty("Enabled")?.SetValue(src, true);
                    var refProp = src.GetType().GetProperty("ReferenceName");
                    if (refProp != null && refProp.CanWrite)
                        refProp.SetValue(src, table.TableName);
                    var tableProp = src.GetType().GetProperty("Table");
                    if (tableProp != null && tableProp.CanWrite)
                        tableProp.SetValue(src, table);
                }
                catch { }
            }

            for (int i = 0; i < report.Dictionary.DataSources.Count; i++)
                report.Dictionary.DataSources[i].Enabled = true;

            ApplyPosOpsFilterParameters(
                report, title, date1, date2, useDateFilter,
                branchId, cashId, filterUserId, reportType, companyId);

            repHelper.FastreportStanderdParameters(report, userId, companyId);
            report.Prepare();

            using (MemoryStream ms = new MemoryStream())
            {
                PDFSimpleExport pdfExport = new PDFSimpleExport();
                pdfExport.Export(report, ms);
                ms.Flush();
                return File(ms.ToArray(), "application/pdf", frxFileName + ".pdf");
            }
        }

        private static void ApplyPosOpsFilterParameters(
            FastReport.Report report,
            string title,
            DateTime date1,
            DateTime date2,
            bool useDateFilter,
            int branchId,
            int cashId,
            int filterUserId,
            string reportType,
            int companyId)
        {
            TrySetParam(report, "report.Title", title);
            TrySetParam(report, "report.FromDate",
                useDateFilter ? date1.ToString("yyyy-MM-dd") : "All");
            TrySetParam(report, "report.ToDate",
                useDateFilter ? date2.ToString("yyyy-MM-dd") : "All");
            TrySetParam(report, "report.ReportType",
                string.IsNullOrWhiteSpace(reportType) ? "" : reportType);
            TrySetParam(report, "report.Scope", "");

            if (branchId == 0)
            {
                TrySetParam(report, "report.Branch", "All Branches");
            }
            else
            {
                clsBranch clsBranch = new clsBranch();
                DataTable dtBranch = clsBranch.SelectBranch(branchId, "", "", companyId);
                TrySetParam(report, "report.Branch",
                    dtBranch != null && dtBranch.Rows.Count > 0
                        ? Simulate.String(dtBranch.Rows[0]["AName"])
                        : branchId.ToString());
            }

            if (cashId == 0)
            {
                TrySetParam(report, "report.CashDrawer", "All Cash Drawers");
            }
            else
            {
                clsCashDrawer clsCashDrawer = new clsCashDrawer();
                DataTable dtCash = clsCashDrawer.SelectCashDrawerByID(cashId, "", "", companyId);
                TrySetParam(report, "report.CashDrawer",
                    dtCash != null && dtCash.Rows.Count > 0
                        ? Simulate.String(dtCash.Rows[0]["AName"])
                        : cashId.ToString());
            }

            if (filterUserId == 0)
            {
                TrySetParam(report, "report.Cashier", "All Cashiers");
            }
            else
            {
                clsEmployee emp = new clsEmployee();
                DataTable dtEmp = emp.SelectEmployee(
                    filterUserId, "", "", "", "", "", "", companyId, -1);
                TrySetParam(report, "report.Cashier",
                    dtEmp != null && dtEmp.Rows.Count > 0
                        ? (string.IsNullOrWhiteSpace(Simulate.String(dtEmp.Rows[0]["AName"]))
                            ? Simulate.String(dtEmp.Rows[0]["EName"])
                            : Simulate.String(dtEmp.Rows[0]["AName"]))
                        : filterUserId.ToString());
            }
        }

        private static void TrySetParam(FastReport.Report report, string name, object value)
        {
            try
            {
                report.SetParameterValue(name, value);
            }
            catch
            {
                // Parameter may not exist on every template.
            }
        }

        #endregion
    }
}
