using FastReport;
using FastReport.Export.PdfSimple;
using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Text;
using WebApplication2.cls;
using WebApplication2.DataSet;
using static WebApplication2.cls.clsReportPdfBuilder;

namespace WebApplication2.cls.Reports
{
    /// <summary>
    /// Resolves tbl_TransactionReport defaults and prepares FastReport documents.
    /// </summary>
    public partial class clsTransactionReportPrint
    {
        public const string PageJournalVoucherAdd = "JournalVoucherAdd";
        public const string PageInvoicePageAdd = "InvoicePageAdd";
        public const string PageCashVoucherAdd = "CashVoucherAdd";
        public const string PageCreditNotePageAdd = "CreditNotePageAdd";

        public const string DefaultJvReportName = "DefaultJV";
        public const string DefaultJvFrxFileName = "rptJV";
        public const string DefaultInvoiceReportName = "DefaultInvoice";
        public const string DefaultInvoiceFrxFileName = "rptInvoice";
        public const string DefaultInvoicePosReportName = "DefaultInvoicePOS";
        public const string DefaultInvoicePosFrxFileName = "rptInvoicePOS";
        public const string DefaultCashVoucherReportName = "DefaultCashVoucher";
        public const string DefaultCashVoucherFrxFileName = "rptCashVoucher";
        public const string DefaultCreditNoteReportName = "DefaultCreditNote";
        public const string DefaultCreditNoteFrxFileName = "rptCashVoucher";
        public const string EngineFastReport = "FastReport";
        public const string EngineJsonTemplate = "JsonTemplate";

        private readonly clsReports _reportsHelper = new clsReports();
        private readonly clsTransactionReport _transactionReport = new clsTransactionReport();

        public class ResolvedTransactionReport
        {
            public int Id { get; set; }
            public string PageName { get; set; } = "";
            public string ReportName { get; set; } = "";
            public string ReportEngine { get; set; } = EngineFastReport;
            public string FastReportFileName { get; set; } = DefaultJvFrxFileName;
            public string ReportFrxXml { get; set; } = "";
            public int ReportTemplateId { get; set; }
            public bool IsDefault { get; set; }
        }

        public ResolvedTransactionReport EnsureAndResolveJournalVoucher(
            int companyId, int userId, int transactionReportId = 0)
        {
            TryEnsureTransactionReportSchema(companyId);

            if (TransactionReportTableExists(companyId))
            {
                try
                {
                    EnsureDefaultJournalVoucherReport(companyId, userId);
                }
                catch
                {
                    // Keep print working if seed fails
                }
            }

            return Resolve(PageJournalVoucherAdd, companyId, transactionReportId);
        }

        /// <summary>
        /// Runs pending DB migrations when tbl_TransactionReport is missing (e.g. user did not re-login).
        /// </summary>
        public static void TryEnsureTransactionReportSchema(int companyId)
        {
            if (TransactionReportTableExists(companyId))
                return;

            try
            {
                clsDataBaseVersion dbVersion = new clsDataBaseVersion();
                DataTable dt = dbVersion.SelectDataBaseVersion(0, companyId);
                decimal versionNumber = 0;
                if (dt != null && dt.Rows.Count > 0)
                    versionNumber = Simulate.decimal_(dt.Rows[0]["VersionNumber"]);

                dbVersion.checkDatabaseUpdates(versionNumber, companyId);
            }
            catch
            {
                // Print still works via file fallback in Resolve()
            }
        }

        public ResolvedTransactionReport Resolve(
            string pageName, int companyId, int transactionReportId = 0)
        {
            if (!TransactionReportTableExists(companyId))
            {
                if (pageName == PageJournalVoucherAdd)
                    return BuildFallbackJournalVoucherDefault();

                return new ResolvedTransactionReport { PageName = pageName };
            }

            try
            {
                DataTable dt = transactionReportId > 0
                    ? _transactionReport.SelectTransactionReportByID(transactionReportId, companyId)
                    : _transactionReport.SelectDefaultTransactionReport(pageName, companyId);

                if (dt == null || dt.Rows.Count == 0)
                {
                    if (pageName == PageJournalVoucherAdd)
                        return BuildFallbackJournalVoucherDefault();

                    return new ResolvedTransactionReport { PageName = pageName };
                }

                return MapRow(dt.Rows[0]);
            }
            catch (SqlException)
            {
                if (pageName == PageJournalVoucherAdd)
                    return BuildFallbackJournalVoucherDefault();

                throw;
            }
        }

        public void EnsureDefaultJournalVoucherReport(int companyId, int creationUserId)
        {
            if (!TransactionReportTableExists(companyId))
                return;

            DataTable dt = _transactionReport.SelectDefaultTransactionReport(
                PageJournalVoucherAdd, companyId);

            if (dt != null && dt.Rows.Count > 0)
                return;

            _transactionReport.InsertTransactionReport(
                PageJournalVoucherAdd,
                DefaultJvReportName,
                "قيد يومية - افتراضي",
                "Journal Voucher - Default",
                EngineFastReport,
                DefaultJvFrxFileName,
                null,
                true,
                true,
                1,
                companyId,
                creationUserId);
        }

        public byte[] BuildTransactionReportPdf(
            string headerGuid,
            string pageName,
            int userId,
            int companyId,
            int transactionReportId = 0)
        {
            TryEnsureTransactionReportSchema(companyId);
            EnsureAllDefaultTransactionReports(companyId, userId);

            ResolvedTransactionReport config = ResolveForPrint(
                pageName, headerGuid, companyId, userId, transactionReportId);

            if (string.Equals(config.ReportEngine, EngineJsonTemplate, StringComparison.OrdinalIgnoreCase))
                return BuildJsonTemplatePdf(headerGuid, pageName, config.ReportTemplateId, userId, companyId);

            string printPage = clsTransactionReportDefaults.ResolvePrintPageName(pageName);

            Report report = printPage switch
            {
                _ when printPage == PageJournalVoucherAdd => PrepareJournalVoucherReport(
                    headerGuid, userId, companyId, config.Id > 0 ? config.Id : transactionReportId),
                _ when printPage == PageInvoicePageAdd => PrepareInvoiceReport(
                    headerGuid, userId, companyId, config),
                _ when printPage == PageCashVoucherAdd => PrepareCashVoucherReport(
                    headerGuid, userId, companyId, config),
                _ when printPage == PageCreditNotePageAdd => PrepareCreditNoteReport(
                    headerGuid, userId, companyId, config),
                _ => throw new InvalidOperationException(
                    $"FastReport print for '{pageName}' is not configured yet."),
            };

            return ExportReportToPdf(report);
        }

        private static byte[] ExportReportToPdf(Report report)
        {
            FastReport.Utils.Config.WebMode = true;
            report.Prepare();
            using MemoryStream ms = new MemoryStream();
            report.Export(new PDFSimpleExport(), ms);
            return ms.ToArray();
        }

        public Report PrepareJournalVoucherReport(
            string guid,
            int userId,
            int companyId,
            int transactionReportId = 0)
        {
            var config = EnsureAndResolveJournalVoucher(companyId, userId, transactionReportId);

            if (!string.Equals(config.ReportEngine, EngineFastReport, StringComparison.OrdinalIgnoreCase))
                throw new InvalidOperationException(
                    $"Report engine '{config.ReportEngine}' is not supported for journal voucher FastReport print.");

            var clsJournalVoucherHeader = new clsJournalVoucherHeader();
            var clsJournalVoucherDetails = new clsJournalVoucherDetails();

            DataTable dtHeader = clsJournalVoucherHeader.SelectJournalVoucherHeaderForPrint(
                guid, 0, 0, "", "", 0, companyId,
                DateTime.Now.AddYears(-100), DateTime.Now.AddYears(100));

            if (dtHeader == null || dtHeader.Rows.Count == 0)
                throw new InvalidOperationException("Journal voucher not found.");

            dsJVDetails ds = clsJournalVoucherDetails.SelectJournalVoucherDetailsByParentIdForPrint(
                companyId, guid, 0, 0);

            Report report = new Report();
            report.RegisterData(ds);
            LoadFastReportTemplate(report, config, companyId);
            ApplyJournalVoucherParameters(report, dtHeader, userId, companyId);
            // Prepare runs once in Main.FastreporttoPDF
            return report;
        }

        public string GetFrxXmlForTransactionReport(
            int transactionReportId, int companyId, int userId)
        {
            TryEnsureTransactionReportSchema(companyId);

            DataTable dt = _transactionReport.SelectTransactionReportByID(
                transactionReportId, companyId);
            if (dt == null || dt.Rows.Count == 0)
                throw new InvalidOperationException("Transaction report not found.");

            ResolvedTransactionReport config = MapRow(dt.Rows[0]);

            string frxName = string.IsNullOrWhiteSpace(config.FastReportFileName)
                ? DefaultJvFrxFileName
                : config.FastReportFileName;

            // 1) DB stored XML
            string storedXml = StripFrxBom(config.ReportFrxXml);
            if (IsValidFrxXml(storedXml))
                return storedXml;

            // 2) Company file → 3) standard file
            string path = _reportsHelper.getMyPath(frxName, companyId);
            if (!File.Exists(path))
                throw new FileNotFoundException($"FastReport template not found: {path}");

            return File.ReadAllText(path, Encoding.UTF8);
        }

        public Report BuildDesignReport(
            int transactionReportId,
            int companyId,
            int userId)
        {
            TryEnsureTransactionReportSchema(companyId);

            DataTable dt = _transactionReport.SelectTransactionReportByID(
                transactionReportId, companyId);
            if (dt == null || dt.Rows.Count == 0)
                throw new InvalidOperationException("Transaction report not found.");

            ResolvedTransactionReport config = MapRow(dt.Rows[0]);
            if (!string.Equals(config.ReportEngine, EngineFastReport, StringComparison.OrdinalIgnoreCase))
                throw new InvalidOperationException("Only FastReport layouts can be opened in the designer.");

            Report report = new Report();
            RegisterDesignData(report, config.PageName, companyId, userId);
            LoadFastReportTemplate(report, config, companyId);

            string sampleGuid = _transactionReport.SelectLatestHeaderGuidForPage(
                config.PageName, companyId);

            string printPage = clsTransactionReportDefaults.ResolvePrintPageName(config.PageName);

            if (printPage == PageJournalVoucherAdd && !string.IsNullOrWhiteSpace(sampleGuid))
            {
                clsJournalVoucherHeader jvHeader = new clsJournalVoucherHeader();
                DataTable dtHeader = jvHeader.SelectJournalVoucherHeaderForPrint(
                    sampleGuid, 0, 0, "", "", 0, companyId,
                    DateTime.Now.AddYears(-100), DateTime.Now.AddYears(100));
                ApplyJournalVoucherParameters(report, dtHeader, userId, companyId);
            }
            else if (printPage == PageInvoicePageAdd && !string.IsNullOrWhiteSpace(sampleGuid))
            {
                clsInvoiceHeader invoiceHeader = new clsInvoiceHeader();
                DataTable dtHeader = invoiceHeader.SelectInvoiceHeaderByGuid(
                    sampleGuid, DateTime.Now.AddYears(-100), DateTime.Now.AddYears(100),
                    0, 0, 0, companyId);
                ApplyInvoiceParameters(report, dtHeader, userId, companyId);
            }
            else if (printPage == PageCashVoucherAdd && !string.IsNullOrWhiteSpace(sampleGuid))
            {
                clsCashVoucherHeader cashHeader = new clsCashVoucherHeader();
                DataTable dtHeader = cashHeader.SelectCashVoucherHeaderByGuid(
                    sampleGuid, DateTime.Now.AddYears(-100), DateTime.Now.AddYears(100),
                    0, 0, companyId, "");
                ApplyCashVoucherAmountParameters(report, dtHeader);
                _reportsHelper.FastreportStanderdParameters(report, userId, companyId);
            }
            else if (printPage == PageCreditNotePageAdd && !string.IsNullOrWhiteSpace(sampleGuid))
            {
                clsCreditNoteHeader cnHeader = new clsCreditNoteHeader();
                DataTable dtHeader = cnHeader.SelectCreditNoteHeaderByGuid(
                    sampleGuid, DateTime.Now.AddYears(-100), DateTime.Now.AddYears(100),
                    0, 0, companyId);
                ApplyCashVoucherAmountParameters(report, dtHeader);
                _reportsHelper.FastreportStanderdParameters(report, userId, companyId);
            }

            return report;
        }

        public static string BuildDesignerReportId(int companyId, int transactionReportId)
        {
            return $"TR_{companyId}_{transactionReportId}";
        }

        public static bool TryParseDesignerReportId(
            string reportId, out int companyId, out int transactionReportId)
        {
            companyId = 0;
            transactionReportId = 0;
            if (string.IsNullOrWhiteSpace(reportId) || !reportId.StartsWith("TR_"))
                return false;

            string[] parts = reportId.Split('_');
            if (parts.Length < 3)
                return false;

            companyId = Simulate.Integer32(parts[1]);
            transactionReportId = Simulate.Integer32(parts[2]);
            return companyId > 0 && transactionReportId > 0;
        }

        private void RegisterDesignData(Report report, string pageName, int companyId, int userId)
        {
            string printPage = clsTransactionReportDefaults.ResolvePrintPageName(pageName);
            string guid = _transactionReport.SelectLatestHeaderGuidForPage(pageName, companyId);
            if (string.IsNullOrWhiteSpace(guid))
                return;

            if (printPage == PageJournalVoucherAdd)
            {
                clsJournalVoucherDetails jvDetails = new clsJournalVoucherDetails();
                dsJVDetails ds = jvDetails.SelectJournalVoucherDetailsByParentIdForPrint(
                    companyId, guid, 0, 0);
                report.RegisterData(ds);
            }
            else if (printPage == PageInvoicePageAdd)
            {
                clsInvoiceDetails invoiceDetails = new clsInvoiceDetails();
                DataTable dtDetails = invoiceDetails.SelectInvoiceDetailsByHeaderGuid(
                    guid, "", companyId);
                dsInvoiceDetails ds = new dsInvoiceDetails();
                if (dtDetails != null && dtDetails.Rows.Count > 0)
                {
                    for (int i = 0; i < dtDetails.Rows.Count; i++)
                    {
                        ds.InvoiceDetails.Rows.Add();
                        ds.InvoiceDetails.Rows[i]["ItemName"] = Simulate.String(dtDetails.Rows[i]["ItemName"]);
                        ds.InvoiceDetails.Rows[i]["Qty"] = Simulate.decimal_(dtDetails.Rows[i]["Qty"]);
                        ds.InvoiceDetails.Rows[i]["TotalLine"] = Simulate.decimal_(dtDetails.Rows[i]["TotalLine"]);
                    }
                }
                report.RegisterData(ds);
            }
            else if (printPage == PageCashVoucherAdd)
            {
                clsCashVoucherHeader cashHeader = new clsCashVoucherHeader();
                clsCashVoucherDetails cashDetails = new clsCashVoucherDetails();
                DataTable dtHeader = cashHeader.SelectCashVoucherHeaderByGuid(
                    guid, DateTime.Now.AddYears(-100), DateTime.Now.AddYears(100),
                    0, 0, companyId, "");
                DataTable dtDetails = cashDetails.SelectCashVoucherDetailsByHeaderGuid(guid, companyId);
                report.RegisterData(BuildCashVoucherDataSet(dtHeader, dtDetails));
            }
            else if (printPage == PageCreditNotePageAdd)
            {
                clsCreditNoteHeader cnHeader = new clsCreditNoteHeader();
                clsCreditNoteDetails cnDetails = new clsCreditNoteDetails();
                DataTable dtHeader = cnHeader.SelectCreditNoteHeaderByGuid(
                    guid, DateTime.Now.AddYears(-100), DateTime.Now.AddYears(100),
                    0, 0, companyId);
                DataTable dtDetails = cnDetails.SelectCreditNoteDetailsByHeaderGuid(guid, companyId);
                report.RegisterData(BuildCreditNoteDataSet(dtHeader, dtDetails));
            }
        }

        /// <summary>
        /// Priority: 1) DB stored XML  2) Reports\{CompanyID}\  3) Reports\ (standard)
        /// Reset to default = clear DB XML → system falls to company file → standard file.
        /// </summary>
        public void LoadFastReportTemplate(
            Report report, ResolvedTransactionReport config, int companyId)
        {
            string frxName = string.IsNullOrWhiteSpace(config.FastReportFileName)
                ? DefaultJvFrxFileName
                : config.FastReportFileName;

            // 1) DB stored XML
            string storedXml = StripFrxBom(config.ReportFrxXml);
            if (IsValidFrxXml(storedXml))
            {
                try
                {
                    byte[] bytes = Encoding.UTF8.GetBytes(storedXml);
                    using MemoryStream ms = new MemoryStream(bytes);
                    report.Load(ms);
                    return;
                }
                catch { }
            }

            // 2) Company-specific file, then 3) standard file (getMyPath does both)
            string path = _reportsHelper.getMyPath(frxName, companyId);
            if (!File.Exists(path))
                throw new FileNotFoundException($"FastReport template not found: {path}");

            report.Load(path);
        }

        /// <summary>
        /// True when DB XML is only a copy of the shared Reports\ standard (not a real customization).
        /// Those copies block company-specific .frx updates and product standard updates.
        /// </summary>
        public static bool IsSeededCopyOfGlobalStandard(string storedXml, string globalPath)
        {
            if (string.IsNullOrWhiteSpace(storedXml) || string.IsNullOrWhiteSpace(globalPath))
                return false;
            if (!File.Exists(globalPath))
                return false;

            try
            {
                string globalXml = StripFrxBom(File.ReadAllText(globalPath, Encoding.UTF8));
                if (!IsValidFrxXml(globalXml))
                    return false;

                return string.Equals(
                    NormalizeFrxForCompare(storedXml),
                    NormalizeFrxForCompare(globalXml),
                    StringComparison.Ordinal);
            }
            catch
            {
                return false;
            }
        }

        public static string NormalizeFrxForCompare(string xml)
        {
            if (string.IsNullOrWhiteSpace(xml))
                return "";

            return StripFrxBom(xml)
                .Replace("\r\n", "\n")
                .Replace("\r", "\n")
                .Trim();
        }

        /// <summary>
        /// Removes a leading UTF-8 BOM / zero-width characters that .NET's Trim() does not
        /// strip. FastReport Designer often saves .frx files with a BOM, which otherwise
        /// makes the stored layout look invalid and reverts previews to the shipped default.
        /// </summary>
        public static string StripFrxBom(string xml)
        {
            if (string.IsNullOrEmpty(xml))
                return xml;

            return xml.TrimStart('\uFEFF', '\u200B', '\uFFFE');
        }

        public static bool IsValidFrxXml(string xml)
        {
            if (string.IsNullOrWhiteSpace(xml))
                return false;

            string trimmed = StripFrxBom(xml).Trim();
            if (trimmed.Length < 500)
                return false;

            bool hasRoot = trimmed.StartsWith("<?xml", StringComparison.OrdinalIgnoreCase)
                || trimmed.StartsWith("<Report", StringComparison.OrdinalIgnoreCase);
            if (!hasRoot)
                return false;

            return trimmed.Contains("</Report>", StringComparison.OrdinalIgnoreCase);
        }

        public void ApplyJournalVoucherParameters(
            Report report, DataTable dtHeader, int userId, int companyId)
        {
            if (dtHeader == null || dtHeader.Rows.Count == 0)
                return;

            DataRow row = dtHeader.Rows[0];

            if (Simulate.Integer32(row["BranchID"]) == 0)
                report.SetParameterValue("report.Branch", "All Branches");
            else
            {
                clsBranch clsBranch = new clsBranch();
                DataTable dtBranch = clsBranch.SelectBranch(
                    Simulate.Integer32(row["BranchID"]), "", "", companyId);
                if (dtBranch != null && dtBranch.Rows.Count > 0)
                    report.SetParameterValue("report.Branch", Simulate.String(dtBranch.Rows[0]["AName"]));
            }

            report.SetParameterValue("report.JVNo", Simulate.String(row["JVNumber"]));
            report.SetParameterValue("report.CreationUser", Simulate.String(row["EmployeeAName"]));
            report.SetParameterValue(
                "report.Date",
                Simulate.StringToDate(row["VoucherDate"]).ToString("yyyy-MM-dd"));

            int jvTypeId = 0;
            if (dtHeader.Columns.Contains("JVTypeID"))
                jvTypeId = Simulate.Integer32(row["JVTypeID"]);

            if (jvTypeId == 0)
            {
                report.SetParameterValue("report.JournalVoucherTypes", "Manual JV");
            }
            else
            {
                clsJournalVoucherTypes clsJournalVoucherTypes = new clsJournalVoucherTypes();
                DataTable dtType = clsJournalVoucherTypes.SelectJournalVoucherTypes(jvTypeId, companyId);
                if (dtType != null && dtType.Rows.Count > 0)
                    report.SetParameterValue(
                        "report.JournalVoucherTypes",
                        Simulate.String(dtType.Rows[0]["AName"]));
                else
                    report.SetParameterValue("report.JournalVoucherTypes", "");
            }

            _reportsHelper.FastreportStanderdParameters(report, userId, companyId);
        }

        public static bool TransactionReportTableExists(int companyId)
        {
            try
            {
                clsSQL sql = new clsSQL();
                string con = sql.CreateDataBaseConnectionString(companyId);
                if (string.IsNullOrWhiteSpace(con))
                    return false;

                object scalar = sql.ExecuteScalarCommandText(
                    @"SELECT COUNT(*) FROM INFORMATION_SCHEMA.TABLES
                      WHERE TABLE_NAME = 'tbl_TransactionReport'",
                    con);
                return Simulate.Integer32(scalar) > 0;
            }
            catch
            {
                return false;
            }
        }

        private static ResolvedTransactionReport BuildFallbackJournalVoucherDefault()
        {
            return new ResolvedTransactionReport
            {
                PageName = PageJournalVoucherAdd,
                ReportName = DefaultJvReportName,
                ReportEngine = EngineFastReport,
                FastReportFileName = DefaultJvFrxFileName,
                IsDefault = true,
            };
        }

        public byte[] BuildJsonTemplatePdf(
            string headerGuid,
            string pageName,
            int reportTemplateId,
            int userId,
            int companyId)
        {
            if (reportTemplateId <= 0)
                throw new InvalidOperationException("Report template is not linked.");

            clsReportTemplate tplDal = new clsReportTemplate();
            DataTable dtTpl = tplDal.SelectReportTemplateByID(reportTemplateId, companyId);
            if (dtTpl == null || dtTpl.Rows.Count == 0)
                throw new InvalidOperationException("Template not found.");

            string templateJson = Simulate.String(dtTpl.Rows[0]["TemplateJson"]);
            if (string.IsNullOrWhiteSpace(templateJson))
                throw new InvalidOperationException("Template JSON is empty.");

            LoadTransactionTables(pageName, headerGuid, companyId, out DataTable dtHeader, out DataTable dtDetails);
            if (dtHeader == null || dtHeader.Rows.Count == 0)
                throw new InvalidOperationException("Transaction header not found.");

            PrintData data = BuildPrintDataFromTables(dtHeader, dtDetails, companyId, userId);
            return clsReportPdfBuilder.Build(templateJson, data);
        }

        public void LoadTransactionTables(
            string pageName,
            string headerGuid,
            int companyId,
            out DataTable dtHeader,
            out DataTable dtDetails)
        {
            dtHeader = new DataTable();
            dtDetails = new DataTable();

            string printPage = clsTransactionReportDefaults.ResolvePrintPageName(pageName);

            if (printPage == "InvoicePageAdd")
            {
                var clsInvoiceDetails = new clsInvoiceDetails();
                var clsInvoiceHeader = new clsInvoiceHeader();
                dtHeader = clsInvoiceHeader.SelectInvoiceHeaderByGuid(
                    headerGuid,
                    DateTime.Now.AddYears(-100),
                    DateTime.Now.AddYears(100),
                    0, 0, 0,
                    companyId);
                dtDetails = clsInvoiceDetails.SelectInvoiceDetailsByHeaderGuid(headerGuid, "", companyId);
            }
            else if (printPage == "CreditNotePageAdd")
            {
                var clsCreditNoteHeader = new clsCreditNoteHeader();
                var clsCreditNoteDetails = new clsCreditNoteDetails();
                dtHeader = clsCreditNoteHeader.SelectCreditNoteHeaderByGuid(
                    headerGuid,
                    DateTime.Now.AddYears(-100),
                    DateTime.Now.AddYears(100),
                    0, 0,
                    companyId);
                dtDetails = clsCreditNoteDetails.SelectCreditNoteDetailsByHeaderGuid(headerGuid, companyId);
            }
            else if (printPage == "CashVoucherAdd")
            {
                var clsCashVoucherHeader = new clsCashVoucherHeader();
                var clsCashVoucherDetails = new clsCashVoucherDetails();
                dtHeader = clsCashVoucherHeader.SelectCashVoucherHeaderByGuid(
                    headerGuid,
                    DateTime.Now.AddYears(-100),
                    DateTime.Now.AddYears(100),
                    0, 0,
                    companyId, "");
                dtDetails = clsCashVoucherDetails.SelectCashVoucherDetailsByHeaderGuid(headerGuid, companyId);
            }
            else if (printPage == "JournalVoucherAdd")
            {
                var clsJournalVoucherHeader = new clsJournalVoucherHeader();
                var clsJournalVoucherDetails = new clsJournalVoucherDetails();
                dtHeader = clsJournalVoucherHeader.SelectJournalVoucherHeaderForPrint(
                    headerGuid, 0, 0, "", "", 0, companyId,
                    DateTime.Now.AddYears(-100), DateTime.Now.AddYears(100));
                dtDetails = clsJournalVoucherDetails
                    .SelectJournalVoucherDetailsByParentIdForPrint(companyId, headerGuid, 0, 0, null)
                    .Tables[0];
            }
        }

        private static PrintData BuildPrintDataFromTables(
            DataTable dtHeader,
            DataTable dtDetails,
            int companyId,
            int userId)
        {
            var data = new PrintData();
            clsCompany clsCompany = new clsCompany();
            DataTable dtCompany = clsCompany.SelectCompany(companyId, "", "", "", companyId, "", false);

            if (dtCompany != null && dtCompany.Rows.Count > 0 && dtCompany.Columns.Contains("Logo"))
            {
                var logoObj = dtCompany.Rows[0]["Logo"];
                if (logoObj != DBNull.Value && logoObj is byte[] logoBytes && logoBytes.Length > 0)
                {
                    data.Company["LogoBase64"] = Convert.ToBase64String(logoBytes);
                    data.Company["LogoMime"] = "image/png";
                }
            }

            data.Company["CompanyID"] = companyId;
            data.Company["UserId"] = userId;
            data.Company["PrintDate"] = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            data.Header = FirstRowOrEmpty(dtHeader);
            data.Lines = DataTableToList(dtDetails);
            return data;
        }

        private static Dictionary<string, object> FirstRowOrEmpty(DataTable dt)
        {
            var dict = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);
            if (dt == null || dt.Rows.Count == 0)
                return dict;
            foreach (DataColumn col in dt.Columns)
            {
                var val = dt.Rows[0][col];
                dict[col.ColumnName] = val == DBNull.Value ? null : val;
            }
            return dict;
        }

        private static List<Dictionary<string, object>> DataTableToList(DataTable dt)
        {
            var list = new List<Dictionary<string, object>>();
            if (dt == null) return list;
            foreach (DataRow r in dt.Rows)
            {
                var dict = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);
                foreach (DataColumn col in r.Table.Columns)
                {
                    var val = r[col];
                    dict[col.ColumnName] = val == DBNull.Value ? null : val;
                }
                list.Add(dict);
            }
            return list;
        }

        private static ResolvedTransactionReport MapRow(DataRow row)
        {
            string frxXml = "";
            if (row.Table.Columns.Contains("ReportFrxXml"))
                frxXml = Simulate.String(row["ReportFrxXml"]);

            if (string.IsNullOrWhiteSpace(frxXml) && row.Table.Columns.Contains("ReportFRXXML"))
                frxXml = Simulate.String(row["ReportFRXXML"]);

            return new ResolvedTransactionReport
            {
                Id = Simulate.Integer32(row["ID"]),
                PageName = Simulate.String(row["PageName"]),
                ReportName = Simulate.String(row["ReportName"]),
                ReportEngine = Simulate.String(row["ReportEngine"]),
                FastReportFileName = Simulate.String(row["FastReportFileName"]),
                ReportFrxXml = frxXml,
                ReportTemplateId = Simulate.Integer32(row["ReportTemplateID"]),
                IsDefault = Simulate.Bool(row["IsDefault"]),
            };
        }
    }
}
