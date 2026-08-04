using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System;
using System.Data;
using System.IO;
using WebApplication2.cls;
using WebApplication2.cls.Reports;

namespace WebApplication2.Controllers
{
    [Route("api/ctlTransactionReport")]
    public class ctlTransactionReport : Controller
    {
        [HttpGet]
        [Route("SelectTransactionReportByID")]
        public string SelectTransactionReportByID(int ID, int CompanyID)
        {
            try
            {
                clsTransactionReport cls = new clsTransactionReport();
                DataTable dt = cls.SelectTransactionReportByID(ID, CompanyID);
                if (dt != null)
                    return JsonConvert.SerializeObject(dt);
                return "";
            }
            catch (Exception)
            {
                throw;
            }
        }

        [HttpGet]
        [Route("SelectTransactionReportList")]
        public string SelectTransactionReportList(string PageName, int CompanyID)
        {
            try
            {
                clsTransactionReport cls = new clsTransactionReport();
                DataTable dt = cls.SelectTransactionReportList(Simulate.String(PageName), CompanyID);
                if (dt != null)
                    return JsonConvert.SerializeObject(dt);
                return "";
            }
            catch (Exception)
            {
                throw;
            }
        }

        [HttpGet]
        [Route("SelectDefaultTransactionReport")]
        public string SelectDefaultTransactionReport(string PageName, int CompanyID)
        {
            try
            {
                clsTransactionReport cls = new clsTransactionReport();
                DataTable dt = cls.SelectDefaultTransactionReport(Simulate.String(PageName), CompanyID);
                if (dt != null)
                    return JsonConvert.SerializeObject(dt);
                return "";
            }
            catch (Exception)
            {
                throw;
            }
        }

        [HttpPost]
        [Route("InsertTransactionReport")]
        public int InsertTransactionReport(
            string PageName,
            string ReportName,
            string AName,
            string EName,
            string ReportEngine,
            string FastReportFileName,
            int ReportTemplateID,
            bool IsDefault,
            bool IsActive,
            int SortOrder,
            int CompanyID,
            int CreationUserID)
        {
            try
            {
                clsTransactionReport cls = new clsTransactionReport();
                int? templateId = ReportTemplateID > 0 ? ReportTemplateID : null;
                return cls.InsertTransactionReport(
                    Simulate.String(PageName),
                    Simulate.String(ReportName),
                    Simulate.String(AName),
                    Simulate.String(EName),
                    Simulate.String(ReportEngine),
                    Simulate.String(FastReportFileName),
                    templateId,
                    IsDefault,
                    IsActive,
                    SortOrder,
                    CompanyID,
                    CreationUserID);
            }
            catch (Exception)
            {
                throw;
            }
        }

        [HttpPost]
        [Route("UpdateTransactionReport")]
        public int UpdateTransactionReport(
            int ID,
            string PageName,
            string ReportName,
            string AName,
            string EName,
            string ReportEngine,
            string FastReportFileName,
            int ReportTemplateID,
            bool IsDefault,
            bool IsActive,
            int SortOrder,
            int ModificationUserID,
            int CompanyID)
        {
            try
            {
                clsTransactionReport cls = new clsTransactionReport();
                int? templateId = ReportTemplateID > 0 ? ReportTemplateID : null;
                return cls.UpdateTransactionReport(
                    ID,
                    Simulate.String(PageName),
                    Simulate.String(ReportName),
                    Simulate.String(AName),
                    Simulate.String(EName),
                    Simulate.String(ReportEngine),
                    Simulate.String(FastReportFileName),
                    templateId,
                    IsDefault,
                    IsActive,
                    SortOrder,
                    ModificationUserID,
                    CompanyID);
            }
            catch (Exception)
            {
                throw;
            }
        }

        [HttpPost]
        [Route("SyncJournalVoucherDefaultFrxFromFile")]
        public int SyncJournalVoucherDefaultFrxFromFile(int CompanyID, int ModificationUserID)
        {
            try
            {
                clsTransactionReportPrint printer = new clsTransactionReportPrint();
                printer.EnsureDefaultJournalVoucherReport(CompanyID, ModificationUserID);
                var config = printer.Resolve(
                    clsTransactionReportPrint.PageJournalVoucherAdd, CompanyID);

                if (config.Id <= 0)
                    return 0;

                clsReports clsReports = new clsReports();
                string frxName = string.IsNullOrWhiteSpace(config.FastReportFileName)
                    ? clsTransactionReportPrint.DefaultJvFrxFileName
                    : config.FastReportFileName;
                string path = clsReports.getMyPath(frxName, CompanyID);
                if (!System.IO.File.Exists(path))
                    return 0;

                string xml = System.IO.File.ReadAllText(path);
                clsTransactionReport cls = new clsTransactionReport();
                return cls.UpdateReportFrxXml(config.Id, xml, ModificationUserID, CompanyID);
            }
            catch (Exception)
            {
                throw;
            }
        }

        [HttpGet]
        [Route("GetJournalVoucherDefaultReportView")]
        public string GetJournalVoucherDefaultReportView(int CompanyID, int UserId)
        {
            try
            {
                clsTransactionReportPrint printer = new clsTransactionReportPrint();
                var config = printer.EnsureAndResolveJournalVoucher(CompanyID, UserId);
                var payload = new
                {
                    id = config.Id,
                    pageName = config.PageName,
                    reportName = config.ReportName,
                    reportEngine = config.ReportEngine,
                    fastReportFileName = config.FastReportFileName,
                    hasInlineFrx = !string.IsNullOrWhiteSpace(config.ReportFrxXml),
                    isDefault = config.IsDefault,
                };
                return JsonConvert.SerializeObject(payload);
            }
            catch (Exception)
            {
                throw;
            }
        }

        [HttpGet]
        [Route("GetTransactionReportPageCatalog")]
        public string GetTransactionReportPageCatalog(int CompanyID, int UserId = 0)
        {
            try
            {
                clsTransactionReportDefaults.ApplyDefaultSeeds(CompanyID, UserId);
                var pages = clsTransactionReportDefaults.GetPageCatalog();
                return JsonConvert.SerializeObject(pages);
            }
            catch (Exception)
            {
                throw;
            }
        }

        [HttpPost]
        [Route("EnsureAllDefaultTransactionReports")]
        public IActionResult EnsureAllDefaultTransactionReports(int CompanyID, int UserId)
        {
            try
            {
                clsTransactionReportPrint.TryEnsureTransactionReportSchema(CompanyID);
                int inserted = clsTransactionReportDefaults.ApplyDefaultSeeds(CompanyID, UserId);
                int cleared = clsTransactionReportDefaults.ClearSeededFrxMatchingGlobalStandard(CompanyID, UserId);
                return Ok(new { inserted, synced = cleared, cleared });
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        /// <summary>
        /// Seeds standard FastReport defaults into every company database (main company list).
        /// </summary>
        [HttpPost]
        [Route("EnsureAllDefaultTransactionReportsForAllCompanies")]
        public IActionResult EnsureAllDefaultTransactionReportsForAllCompanies(int UserId = 0)
        {
            try
            {
                clsCompany companies = new clsCompany();
                DataTable dt = companies.SelectCompany(0, "", "", "", 0, "", true);
                int companiesProcessed = 0;
                int insertedTotal = 0;
                var errors = new System.Collections.Generic.List<string>();

                if (dt != null)
                {
                    foreach (DataRow row in dt.Rows)
                    {
                        int companyId = Simulate.Integer32(row["ID"]);
                        if (companyId <= 0)
                            continue;

                        try
                        {
                            clsDataBaseVersion dbVersion = new clsDataBaseVersion();
                            DataTable ver = dbVersion.SelectDataBaseVersion(0, companyId);
                            decimal versionNumber = 0;
                            if (ver != null && ver.Rows.Count > 0)
                                versionNumber = Simulate.decimal_(ver.Rows[0]["VersionNumber"]);
                            dbVersion.checkDatabaseUpdates(versionNumber, companyId);

                            clsTransactionReportPrint.TryEnsureTransactionReportSchema(companyId);
                            insertedTotal += clsTransactionReportDefaults.ApplyDefaultSeeds(companyId, UserId);
                            clsTransactionReportDefaults.ApplyStandardFrxFromFiles(companyId, UserId);
                            companiesProcessed++;
                        }
                        catch (Exception ex)
                        {
                            errors.Add($"Company {companyId}: {ex.Message}");
                        }
                    }
                }

                return Ok(new { companiesProcessed, insertedTotal, errors });
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost]
        [Route("SyncAllDefaultFrxFromFiles")]
        public int SyncAllDefaultFrxFromFiles(int CompanyID, int UserId)
        {
            try
            {
                return clsTransactionReportDefaults.ApplyStandardFrxFromFiles(CompanyID, UserId);
            }
            catch (Exception)
            {
                throw;
            }
        }

        [HttpPost]
        [Route("LinkJsonTemplateReport")]
        public int LinkJsonTemplateReport(
            int TransactionReportID,
            string PageName,
            string ReportName,
            string AName,
            string EName,
            int ReportTemplateID,
            bool SetAsDefault,
            int CompanyID,
            int UserId)
        {
            try
            {
                clsTransactionReport cls = new clsTransactionReport();
                return cls.LinkJsonTemplateReport(
                    TransactionReportID,
                    Simulate.String(PageName),
                    Simulate.String(ReportName),
                    Simulate.String(AName),
                    Simulate.String(EName),
                    ReportTemplateID,
                    SetAsDefault,
                    CompanyID,
                    UserId);
            }
            catch (Exception)
            {
                throw;
            }
        }

        [HttpPost]
        [Route("SetAsDefaultTransactionReport")]
        public int SetAsDefaultTransactionReport(int ID, int ModificationUserID, int CompanyID)
        {
            try
            {
                clsTransactionReport cls = new clsTransactionReport();
                return cls.SetAsDefaultTransactionReport(ID, ModificationUserID, CompanyID);
            }
            catch (Exception)
            {
                throw;
            }
        }

        [HttpGet]
        [Route("DeleteTransactionReportByID")]
        public bool DeleteTransactionReportByID(int ID, int CompanyID)
        {
            try
            {
                clsTransactionReport cls = new clsTransactionReport();
                return cls.DeleteTransactionReportByID(ID, CompanyID);
            }
            catch (Exception)
            {
                throw;
            }
        }
    }
}
