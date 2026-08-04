using FastReport;
using System;
using System.Data;
using System.IO;
using WebApplication2.cls;
using WebApplication2.DataSet;
using WebApplication2.MainClasses;

namespace WebApplication2.cls.Reports
{
    public partial class clsTransactionReportPrint
    {
        public void EnsureAllDefaultTransactionReports(int companyId, int creationUserId)
        {
            clsTransactionReportDefaults.ApplyDefaultSeeds(companyId, creationUserId);
            // Drop leftover seeded copies of the standard .frx from ReportFrxXml so print
            // can fall through to company file → standard file unless the user uploaded.
            clsTransactionReportDefaults.ClearSeededFrxMatchingGlobalStandard(
                companyId, creationUserId);
        }

        private void EnsureDefaultReportRow(
            int companyId,
            int creationUserId,
            string pageName,
            string reportName,
            string aName,
            string eName,
            string frxFileName,
            bool isDefault,
            int sortOrder)
        {
            DataTable dt = _transactionReport.SelectTransactionReportByPageAndName(
                pageName, reportName, companyId);
            if (dt != null && dt.Rows.Count > 0)
                return;

            _transactionReport.InsertTransactionReport(
                pageName,
                reportName,
                aName,
                eName,
                EngineFastReport,
                frxFileName,
                null,
                isDefault,
                true,
                sortOrder,
                companyId,
                creationUserId);
        }

        public ResolvedTransactionReport ResolveForPrint(
            string pageName,
            string headerGuid,
            int companyId,
            int userId,
            int transactionReportId = 0)
        {
            if (transactionReportId > 0)
            {
                DataTable dt = _transactionReport.SelectTransactionReportByID(
                    transactionReportId, companyId);
                if (dt != null && dt.Rows.Count > 0)
                    return MapRow(dt.Rows[0]);
            }

            if (pageName == PageJournalVoucherAdd)
                return EnsureAndResolveJournalVoucher(companyId, userId, 0);

            if (pageName == PageInvoicePageAdd
                || pageName == clsTransactionReportDefaults.PageSalesInvoicePageAdd
                || pageName == clsTransactionReportDefaults.PagePurchaseInvoicePageAdd)
                return ResolveInvoiceReport(headerGuid, companyId, pageName);

            DataTable def = _transactionReport.SelectDefaultTransactionReport(pageName, companyId);
            if (def != null && def.Rows.Count > 0)
                return MapRow(def.Rows[0]);

            return BuildFallbackForPage(pageName);
        }

        public sealed class LayoutSourceInfo
        {
            public int TransactionReportId { get; set; }
            public string PageName { get; set; } = "";
            public string ReportName { get; set; } = "";
            public string ReportEngine { get; set; } = "";
            public string FastReportFileName { get; set; } = "";
            public int UploadedLayoutLength { get; set; }
            public bool UploadedLayoutValid { get; set; }
            public bool UploadedLayoutMatchesStandard { get; set; }
            public bool CompanyFileExists { get; set; }
            public string CompanyFilePath { get; set; } = "";
            public bool StandardFileExists { get; set; }
            public string StandardFilePath { get; set; } = "";
            public string EffectiveSource { get; set; } = "";
            public string EffectivePath { get; set; } = "";
        }

        /// <summary>
        /// Reports which of the three layout sources (uploaded copy in the database, the
        /// Reports\{CompanyID} folder, the shared standard .frx) print will really use.
        /// </summary>
        public LayoutSourceInfo DescribeLayoutSource(int transactionReportId, int companyId)
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

            string standardPath = _reportsHelper.getStandardGlobalPath(frxName);
            string companyPath = Path.Combine(
                Environment.CurrentDirectory, "Reports", companyId.ToString(), frxName + ".frx");
            string storedXml = StripFrxBom(config.ReportFrxXml);

            // Same priority as LoadFastReportTemplate / print:
            // 1) DB XML  2) company file  3) shared standard
            LayoutSourceInfo info = new LayoutSourceInfo
            {
                TransactionReportId = config.Id,
                PageName = config.PageName,
                ReportName = config.ReportName,
                ReportEngine = config.ReportEngine,
                FastReportFileName = frxName,
                UploadedLayoutLength = storedXml == null ? 0 : storedXml.Length,
                UploadedLayoutValid = IsValidFrxXml(storedXml),
                UploadedLayoutMatchesStandard = IsSeededCopyOfGlobalStandard(storedXml, standardPath),
                CompanyFilePath = companyPath,
                CompanyFileExists = File.Exists(companyPath),
                StandardFilePath = standardPath,
                StandardFileExists = File.Exists(standardPath),
            };

            if (info.UploadedLayoutValid)
                info.EffectiveSource = "UploadedLayout";
            else if (info.CompanyFileExists)
            {
                info.EffectiveSource = "CompanyFile";
                info.EffectivePath = companyPath;
            }
            else
            {
                info.EffectiveSource = "SharedStandard";
                info.EffectivePath = standardPath;
            }

            return info;
        }

        /// <summary>
        /// Payment (سند صرف) and receipt (سند قبض) vouchers each own a layout page in Settings.
        /// Those pages are used only when the company actually customized them, so companies that
        /// only ever customized the shared Cash Voucher layout keep printing it unchanged.
        /// </summary>
        public string ResolveCashVoucherLayoutPage(string headerGuid, int companyId)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(headerGuid))
                    return PageCashVoucherAdd;

                DataTable dtHeader = new clsCashVoucherHeader().SelectCashVoucherHeaderByGuid(
                    headerGuid,
                    DateTime.Now.AddYears(-100),
                    DateTime.Now.AddYears(100),
                    0, 0, companyId, "");

                if (dtHeader == null || dtHeader.Rows.Count == 0)
                    return PageCashVoucherAdd;

                int voucherType = Simulate.Integer32(dtHeader.Rows[0]["VoucherType"]);
                string typePage = voucherType switch
                {
                    (int)clsEnum.VoucherType.CashPayment =>
                        clsTransactionReportDefaults.PagePaymentVoucherAdd,
                    (int)clsEnum.VoucherType.Cashrecivable =>
                        clsTransactionReportDefaults.PageReceiptVoucherAdd,
                    _ => null,
                };

                if (string.IsNullOrWhiteSpace(typePage))
                    return PageCashVoucherAdd;

                DataTable dt = _transactionReport.SelectDefaultTransactionReport(typePage, companyId);
                if (dt == null || dt.Rows.Count == 0)
                    return PageCashVoucherAdd;

                return HasCompanyCustomization(MapRow(dt.Rows[0]), companyId)
                    ? typePage
                    : PageCashVoucherAdd;
            }
            catch
            {
                return PageCashVoucherAdd;
            }
        }

        /// <summary>
        /// True when the receipt/payment row has its own DB-stored layout.
        /// If no DB layout, receipt/payment falls back to the shared CashVoucher page.
        /// </summary>
        private bool HasCompanyCustomization(ResolvedTransactionReport config, int companyId)
        {
            if (config == null)
                return false;

            if (string.Equals(config.ReportEngine, EngineJsonTemplate, StringComparison.OrdinalIgnoreCase))
                return config.ReportTemplateId > 0;

            string storedXml = StripFrxBom(config.ReportFrxXml);
            return IsValidFrxXml(storedXml);
        }

        public ResolvedTransactionReport ResolveInvoiceReport(
            string headerGuid, int companyId, string pageName = null)
        {
            pageName = string.IsNullOrWhiteSpace(pageName)
                ? PageInvoicePageAdd
                : pageName;

            if (pageName == clsTransactionReportDefaults.PageSalesInvoicePageAdd)
            {
                return ResolveInvoiceReportByName(
                    headerGuid, companyId, pageName, "DefaultSalesInvoice", DefaultInvoiceFrxFileName);
            }

            if (pageName == clsTransactionReportDefaults.PagePurchaseInvoicePageAdd)
            {
                return ResolveInvoiceReportByName(
                    headerGuid, companyId, pageName, "DefaultPurchaseInvoice", DefaultInvoiceFrxFileName);
            }

            bool usePos = false;
            if (!string.IsNullOrWhiteSpace(headerGuid))
            {
                clsInvoiceHeader invoiceHeader = new clsInvoiceHeader();
                DataTable dtHeader = invoiceHeader.SelectInvoiceHeaderByGuid(
                    headerGuid,
                    DateTime.Now.AddYears(-100),
                    DateTime.Now.AddYears(100),
                    0, 0, 0,
                    companyId);
                if (dtHeader != null && dtHeader.Rows.Count > 0)
                {
                    int typeId = Simulate.Integer32(dtHeader.Rows[0]["InvoiceTypeID"]);
                    usePos = IsPosInvoiceType(typeId);
                }
            }

            string reportName = usePos
                ? DefaultInvoicePosReportName
                : DefaultInvoiceReportName;

            DataTable dt = _transactionReport.SelectTransactionReportByPageAndName(
                pageName: PageInvoicePageAdd,
                reportName: reportName,
                companyID: companyId);

            if (dt != null && dt.Rows.Count > 0)
                return MapRow(dt.Rows[0]);

            return usePos ? BuildFallbackInvoicePos() : BuildFallbackInvoice();
        }

        private ResolvedTransactionReport ResolveInvoiceReportByName(
            string headerGuid,
            int companyId,
            string pageName,
            string reportName,
            string frxFileName)
        {
            DataTable dt = _transactionReport.SelectTransactionReportByPageAndName(
                pageName, reportName, companyId);
            if (dt != null && dt.Rows.Count > 0)
                return MapRow(dt.Rows[0]);

            return new ResolvedTransactionReport
            {
                PageName = pageName,
                ReportName = reportName,
                ReportEngine = EngineFastReport,
                FastReportFileName = frxFileName,
                IsDefault = true,
            };
        }

        private static bool IsPosInvoiceType(int invoiceTypeId)
        {
            return invoiceTypeId == (int)clsEnum.VoucherType.POSSalesInvoice
                || invoiceTypeId == (int)clsEnum.VoucherType.POSSalesInvoicereturn;
        }

        private static ResolvedTransactionReport BuildFallbackForPage(string pageName)
        {
            return pageName switch
            {
                PageInvoicePageAdd => BuildFallbackInvoice(),
                PageCashVoucherAdd => BuildFallbackCashVoucher(),
                PageCreditNotePageAdd => BuildFallbackCreditNote(),
                clsTransactionReportDefaults.PagePaymentVoucherAdd => BuildFallbackNamed(
                    clsTransactionReportDefaults.PagePaymentVoucherAdd,
                    "DefaultPaymentVoucher", DefaultCashVoucherFrxFileName),
                clsTransactionReportDefaults.PageReceiptVoucherAdd => BuildFallbackNamed(
                    clsTransactionReportDefaults.PageReceiptVoucherAdd,
                    "DefaultReceiptVoucher", DefaultCashVoucherFrxFileName),
                clsTransactionReportDefaults.PageDebitNotePageAdd => BuildFallbackNamed(
                    clsTransactionReportDefaults.PageDebitNotePageAdd,
                    "DefaultDebitNote", DefaultCreditNoteFrxFileName),
                clsTransactionReportDefaults.PageSalesInvoicePageAdd => BuildFallbackNamed(
                    clsTransactionReportDefaults.PageSalesInvoicePageAdd,
                    "DefaultSalesInvoice", DefaultInvoiceFrxFileName),
                clsTransactionReportDefaults.PagePurchaseInvoicePageAdd => BuildFallbackNamed(
                    clsTransactionReportDefaults.PagePurchaseInvoicePageAdd,
                    "DefaultPurchaseInvoice", DefaultInvoiceFrxFileName),
                _ => BuildFallbackNamed(pageName, $"Default{pageName}", DefaultJvFrxFileName),
            };
        }

        private static ResolvedTransactionReport BuildFallbackNamed(
            string pageName, string reportName, string frxFileName)
        {
            return new ResolvedTransactionReport
            {
                PageName = pageName,
                ReportName = reportName,
                ReportEngine = EngineFastReport,
                FastReportFileName = frxFileName,
                IsDefault = true,
            };
        }

        private static ResolvedTransactionReport BuildFallbackInvoice()
        {
            return new ResolvedTransactionReport
            {
                PageName = PageInvoicePageAdd,
                ReportName = DefaultInvoiceReportName,
                ReportEngine = EngineFastReport,
                FastReportFileName = DefaultInvoiceFrxFileName,
                IsDefault = true,
            };
        }

        private static ResolvedTransactionReport BuildFallbackInvoicePos()
        {
            return new ResolvedTransactionReport
            {
                PageName = PageInvoicePageAdd,
                ReportName = DefaultInvoicePosReportName,
                ReportEngine = EngineFastReport,
                FastReportFileName = DefaultInvoicePosFrxFileName,
                IsDefault = false,
            };
        }

        private static ResolvedTransactionReport BuildFallbackCashVoucher()
        {
            return new ResolvedTransactionReport
            {
                PageName = PageCashVoucherAdd,
                ReportName = DefaultCashVoucherReportName,
                ReportEngine = EngineFastReport,
                FastReportFileName = DefaultCashVoucherFrxFileName,
                IsDefault = true,
            };
        }

        private static ResolvedTransactionReport BuildFallbackCreditNote()
        {
            return new ResolvedTransactionReport
            {
                PageName = PageCreditNotePageAdd,
                ReportName = DefaultCreditNoteReportName,
                ReportEngine = EngineFastReport,
                FastReportFileName = DefaultCreditNoteFrxFileName,
                IsDefault = true,
            };
        }

        public Report PrepareInvoiceReport(
            string guid,
            int userId,
            int companyId,
            ResolvedTransactionReport config)
        {
            clsInvoiceHeader clsInvoiceHeader = new clsInvoiceHeader();
            clsInvoiceDetails clsInvoiceDetails = new clsInvoiceDetails();

            DataTable dtHeader = clsInvoiceHeader.SelectInvoiceHeaderByGuid(
                guid, DateTime.Now.AddYears(-100), DateTime.Now.AddYears(100), 0, 0, 0, companyId);
            if (dtHeader == null || dtHeader.Rows.Count == 0)
                throw new InvalidOperationException("Invoice not found.");

            DataTable dtDetails = clsInvoiceDetails.SelectInvoiceDetailsByHeaderGuid(guid, "", companyId);
            dsInvoiceDetails ds = new dsInvoiceDetails();

            if (dtDetails != null && dtDetails.Rows.Count > 0)
            {
                for (int i = 0; i < dtDetails.Rows.Count; i++)
                {
                    ds.InvoiceDetails.Rows.Add();
                    ds.InvoiceDetails.Rows[i]["Guid"] = Simulate.String(dtDetails.Rows[i]["Guid"]);
                    ds.InvoiceDetails.Rows[i]["HeaderGuid"] = Simulate.String(dtDetails.Rows[i]["HeaderGuid"]);
                    ds.InvoiceDetails.Rows[i]["RowIndex"] = Simulate.String(dtDetails.Rows[i]["RowIndex"]);
                    ds.InvoiceDetails.Rows[i]["ItemGuid"] = Simulate.String(dtDetails.Rows[i]["ItemGuid"]);
                    ds.InvoiceDetails.Rows[i]["ItemName"] = Simulate.String(dtDetails.Rows[i]["ItemName"]);
                    ds.InvoiceDetails.Rows[i]["Qty"] = Simulate.decimal_(dtDetails.Rows[i]["Qty"]);
                    ds.InvoiceDetails.Rows[i]["PriceBeforeTax"] = Simulate.decimal_(dtDetails.Rows[i]["PriceBeforeTax"]);
                    ds.InvoiceDetails.Rows[i]["DiscountBeforeTaxAmount"] = Simulate.decimal_(dtDetails.Rows[i]["DiscountBeforeTaxAmountAll"]);
                    ds.InvoiceDetails.Rows[i]["TaxID"] = Simulate.String(dtDetails.Rows[i]["TaxID"]);
                    ds.InvoiceDetails.Rows[i]["TaxPercentage"] = Simulate.String(dtDetails.Rows[i]["TaxPercentage"]);
                    ds.InvoiceDetails.Rows[i]["TaxAmount"] = Simulate.decimal_(dtDetails.Rows[i]["TaxAmount"]);
                    ds.InvoiceDetails.Rows[i]["SpecialTaxID"] = Simulate.String(dtDetails.Rows[i]["SpecialTaxID"]);
                    ds.InvoiceDetails.Rows[i]["SpecialTaxPercentage"] = Simulate.String(dtDetails.Rows[i]["SpecialTaxPercentage"]);
                    ds.InvoiceDetails.Rows[i]["SpecialTaxAmount"] = Simulate.decimal_(dtDetails.Rows[i]["SpecialTaxAmount"]);
                    ds.InvoiceDetails.Rows[i]["DiscountAfterTaxAmount"] = Simulate.decimal_(dtDetails.Rows[i]["DiscountAfterTaxAmountAll"]);
                    ds.InvoiceDetails.Rows[i]["HeaderDiscountAfterTaxAmount"] = Simulate.decimal_(dtDetails.Rows[i]["HeaderDiscountAfterTaxAmount"]);
                    ds.InvoiceDetails.Rows[i]["FreeQty"] = Simulate.decimal_(dtDetails.Rows[i]["FreeQty"]);
                    ds.InvoiceDetails.Rows[i]["TotalQTY"] = Simulate.decimal_(dtDetails.Rows[i]["TotalQTY"]);
                    ds.InvoiceDetails.Rows[i]["ServiceBeforeTax"] = Simulate.decimal_(dtDetails.Rows[i]["ServiceBeforeTax"]);
                    ds.InvoiceDetails.Rows[i]["ServiceTaxAmount"] = Simulate.decimal_(dtDetails.Rows[i]["ServiceTaxAmount"]);
                    ds.InvoiceDetails.Rows[i]["ServiceAfterTax"] = Simulate.decimal_(dtDetails.Rows[i]["ServiceAfterTax"]);
                    ds.InvoiceDetails.Rows[i]["TotalLine"] = Simulate.decimal_(dtDetails.Rows[i]["TotalLine"]);
                    ds.InvoiceDetails.Rows[i]["BranchID"] = Simulate.String(dtDetails.Rows[i]["BranchID"]);
                    ds.InvoiceDetails.Rows[i]["StoreID"] = Simulate.String(dtDetails.Rows[i]["StoreID"]);
                    ds.InvoiceDetails.Rows[i]["CompanyID"] = Simulate.String(dtDetails.Rows[i]["CompanyID"]);
                    ds.InvoiceDetails.Rows[i]["InvoiceTypeID"] = Simulate.String(dtDetails.Rows[i]["InvoiceTypeID"]);
                    ds.InvoiceDetails.Rows[i]["IsCounted"] = Simulate.String(dtDetails.Rows[i]["IsCounted"]);
                    ds.InvoiceDetails.Rows[i]["InvoiceDate"] = Simulate.String(dtDetails.Rows[i]["InvoiceDate"]);
                    ds.InvoiceDetails.Rows[i]["BusinessPartnerID"] = Simulate.String(dtDetails.Rows[i]["BusinessPartnerID"]);
                    ds.InvoiceDetails.Rows[i]["ItemBatchsGuid"] = Simulate.String(dtDetails.Rows[i]["ItemBatchsGuid"]);
                }
            }

            Report report = new Report();
            report.RegisterData(ds);
            LoadFastReportTemplate(report, config, companyId);
            ApplyInvoiceParameters(report, dtHeader, userId, companyId);
            return report;
        }

        public void ApplyInvoiceParameters(
            Report report, DataTable dtHeader, int userId, int companyId)
        {
            if (dtHeader == null || dtHeader.Rows.Count == 0)
                return;

            DataRow row = dtHeader.Rows[0];
            report.SetParameterValue("report.QRText", Simulate.String(row["EInvoiceQRCode"]));

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

            if (Simulate.Integer32(row["BusinessPartnerID"]) == 0)
                report.SetParameterValue("report.BusinessPartner", "Un Known");
            else
            {
                clsBusinessPartner clsBusinessPartner = new clsBusinessPartner();
                DataTable dtBusinessPartner = clsBusinessPartner.SelectBusinessPartner(
                    Simulate.Integer32(row["BusinessPartnerID"]), 0, "", "", "", "", -1, companyId);
                if (dtBusinessPartner != null && dtBusinessPartner.Rows.Count > 0)
                    report.SetParameterValue(
                        "report.BusinessPartner",
                        Simulate.String(dtBusinessPartner.Rows[0]["AName"]));
            }

            if (Simulate.Integer32(row["CashID"]) == 0)
                report.SetParameterValue("report.CashDrawer", "All Cash Drawer");
            else
            {
                clsCashDrawer clsCashDrawer = new clsCashDrawer();
                DataTable dtCash = clsCashDrawer.SelectCashDrawerByID(
                    Simulate.Integer32(row["CashID"]), "", "", companyId);
                if (dtCash != null && dtCash.Rows.Count > 0)
                    report.SetParameterValue("report.CashDrawer", Simulate.String(dtCash.Rows[0]["AName"]));
            }

            int invoiceTypeCol = dtHeader.Columns.Contains("InvoiceTypeid")
                ? Simulate.Integer32(row["InvoiceTypeid"])
                : Simulate.Integer32(row["InvoiceTypeID"]);

            if (invoiceTypeCol == 0)
                report.SetParameterValue("report.JournalVoucherTypes", "All Invoices");
            else
            {
                clsJournalVoucherTypes clsJournalVoucherTypes = new clsJournalVoucherTypes();
                DataTable dtJournalVoucherTypes = clsJournalVoucherTypes.SelectJournalVoucherTypes(
                    invoiceTypeCol, companyId);
                if (dtJournalVoucherTypes != null && dtJournalVoucherTypes.Rows.Count > 0)
                    report.SetParameterValue(
                        "report.JournalVoucherTypes",
                        Simulate.String(dtJournalVoucherTypes.Rows[0]["AName"]));
            }

            report.SetParameterValue(
                "report.InvoiceDate",
                Simulate.StringToDate(row["InvoiceDate"]).ToString("yyyy-MM-dd"));
            report.SetParameterValue("report.InvoiceNumber", Simulate.String(row["InvoiceNo"]));
            report.SetParameterValue("report.InvoiceNumberRef", Simulate.String(row["RefNo"]));
            report.SetParameterValue("report.PaymentMethod", Simulate.String(row["PaymentMethodAName"]));

            _reportsHelper.FastreportStanderdParameters(report, userId, companyId);
        }

        public Report PrepareCashVoucherReport(
            string headerGuid,
            int userId,
            int companyId,
            ResolvedTransactionReport config)
        {
            clsCashVoucherHeader clsCashVoucherHeader = new clsCashVoucherHeader();
            clsCashVoucherDetails clsCashVoucherDetails = new clsCashVoucherDetails();

            DataTable dtHeader = clsCashVoucherHeader.SelectCashVoucherHeaderByGuid(
                headerGuid, DateTime.Now.AddYears(-100), DateTime.Now.AddYears(100),
                0, 0, companyId, "");
            DataTable dtDetails = clsCashVoucherDetails.SelectCashVoucherDetailsByHeaderGuid(
                headerGuid, companyId);

            if (dtHeader == null || dtHeader.Rows.Count == 0)
                throw new InvalidOperationException("Cash voucher not found.");

            dsCashVoucher ds = BuildCashVoucherDataSet(dtHeader, dtDetails);

            Report report = new Report();
            LoadFastReportTemplate(report, config, companyId);
            report.RegisterData(ds);
            ApplyCashVoucherAmountParameters(report, dtHeader);
            _reportsHelper.FastreportStanderdParameters(report, userId, companyId);
            return report;
        }

        public Report PrepareCreditNoteReport(
            string headerGuid,
            int userId,
            int companyId,
            ResolvedTransactionReport config)
        {
            clsCreditNoteHeader clsCreditNoteHeader = new clsCreditNoteHeader();
            clsCreditNoteDetails clsCreditNoteDetails = new clsCreditNoteDetails();

            DataTable dtHeader = clsCreditNoteHeader.SelectCreditNoteHeaderByGuid(
                headerGuid, DateTime.Now.AddYears(-100), DateTime.Now.AddYears(100),
                0, 0, companyId);
            DataTable dtDetails = clsCreditNoteDetails.SelectCreditNoteDetailsByHeaderGuid(
                headerGuid, companyId);

            if (dtHeader == null || dtHeader.Rows.Count == 0)
                throw new InvalidOperationException("Credit / debit note not found.");

            dsCashVoucher ds = BuildCreditNoteDataSet(dtHeader, dtDetails);

            Report report = new Report();
            LoadFastReportTemplate(report, config, companyId);
            report.RegisterData(ds);
            ApplyCashVoucherAmountParameters(report, dtHeader);
            _reportsHelper.FastreportStanderdParameters(report, userId, companyId);
            return report;
        }

        private static dsCashVoucher BuildCashVoucherDataSet(DataTable dtHeader, DataTable dtDetails)
        {
            dsCashVoucher ds = new dsCashVoucher();

            if (dtDetails != null && dtDetails.Rows.Count > 0)
            {
                for (int i = 0; i < dtDetails.Rows.Count; i++)
                {
                    ds.Details.Rows.Add();
                    ds.Details.Rows[i]["Guid"] = Simulate.String(dtDetails.Rows[i]["Guid"]);
                    ds.Details.Rows[i]["HeaderGuid"] = Simulate.String(dtDetails.Rows[i]["HeaderGuid"]);
                    ds.Details.Rows[i]["RowIndex"] = Simulate.String(Simulate.Integer32(dtDetails.Rows[i]["RowIndex"]) + 1);
                    ds.Details.Rows[i]["AccountID"] = Simulate.Integer32(dtDetails.Rows[i]["AccountID"]);
                    ds.Details.Rows[i]["SubAccountID"] = Simulate.Integer32(dtDetails.Rows[i]["SubAccountID"]);
                    ds.Details.Rows[i]["BranchID"] = Simulate.Integer32(dtDetails.Rows[i]["BranchID"]);
                    ds.Details.Rows[i]["CostCenterID"] = Simulate.Integer32(dtDetails.Rows[i]["CostCenterID"]);
                    ds.Details.Rows[i]["Debit"] = Simulate.decimal_(dtDetails.Rows[i]["Debit"]);
                    ds.Details.Rows[i]["Credit"] = Simulate.decimal_(dtDetails.Rows[i]["Credit"]);
                    ds.Details.Rows[i]["Total"] = Simulate.decimal_(dtDetails.Rows[i]["Total"]);
                    ds.Details.Rows[i]["Note"] = Simulate.String(dtDetails.Rows[i]["Note"]);
                    ds.Details.Rows[i]["VoucherType"] = Simulate.Integer32(dtDetails.Rows[i]["VoucherType"]);
                    ds.Details.Rows[i]["CompanyID"] = Simulate.Integer32(dtDetails.Rows[i]["CompanyID"]);
                    ds.Details.Rows[i]["BranchAName"] = Simulate.String(dtDetails.Rows[i]["BranchAName"]);
                    ds.Details.Rows[i]["AccountAName"] = Simulate.String(dtDetails.Rows[i]["AccountsAName"]);
                    ds.Details.Rows[i]["CostCenterAName"] = Simulate.String(dtDetails.Rows[i]["CostCenterAName"]);
                    ds.Details.Rows[i]["SubAccountAName"] = Simulate.String(dtDetails.Rows[i]["SubAccountAName"]);
                }
            }

            if (dtHeader != null && dtHeader.Rows.Count > 0)
            {
                for (int i = 0; i < dtHeader.Rows.Count; i++)
                {
                    ds.Header.Rows.Add();
                    ds.Header.Rows[i]["Guid"] = Simulate.String(dtHeader.Rows[i]["Guid"]);
                    ds.Header.Rows[i]["VoucherDate"] = Simulate.StringToDate(dtHeader.Rows[i]["VoucherDate"]).ToString("yyyy-MM-dd");
                    ds.Header.Rows[i]["BranchID"] = Simulate.Integer32(dtHeader.Rows[i]["BranchID"]);
                    ds.Header.Rows[i]["CostCenterID"] = Simulate.Integer32(dtHeader.Rows[i]["CostCenterID"]);
                    ds.Header.Rows[i]["Amount"] = Simulate.Currency_format(dtHeader.Rows[i]["Amount"]);
                    ds.Header.Rows[i]["JVGuid"] = Simulate.String(dtHeader.Rows[i]["JVGuid"]);
                    ds.Header.Rows[i]["Note"] = Simulate.String(dtHeader.Rows[i]["Note"]);
                    ds.Header.Rows[i]["VoucherNo"] = Simulate.Integer32(dtHeader.Rows[i]["VoucherNo"]);
                    ds.Header.Rows[i]["ManualNo"] = Simulate.String(dtHeader.Rows[i]["ManualNo"]);
                    ds.Header.Rows[i]["VoucherType"] = Simulate.Integer32(dtHeader.Rows[i]["VoucherType"]);
                    ds.Header.Rows[i]["RelatedInvoiceGuid"] = Simulate.String(dtHeader.Rows[i]["RelatedInvoiceGuid"]);
                    ds.Header.Rows[i]["BranchAName"] = Simulate.String(dtHeader.Rows[i]["BranchAName"]);
                    ds.Header.Rows[i]["CostCenterAName"] = Simulate.String(dtHeader.Rows[i]["CostCenterAName"]);
                    ds.Header.Rows[i]["CashDrawerAName"] = Simulate.String(dtHeader.Rows[i]["CashDrawerAName"]);
                    ds.Header.Rows[i]["JournalVoucherTypesAname"] = Simulate.String(dtHeader.Rows[i]["JournalVoucherTypesAname"]);
                    ds.Header.Rows[i]["CreationUserID"] = Simulate.Integer32(dtHeader.Rows[i]["CreationUserID"]);
                    ds.Header.Rows[i]["CreationDate"] = Simulate.StringToDate(dtHeader.Rows[i]["CreationDate"]);
                    ds.Header.Rows[i]["ModificationUserID"] = Simulate.Integer32(dtHeader.Rows[i]["ModificationUserID"]);
                    ds.Header.Rows[i]["ModificationDate"] = Simulate.StringToDate(dtHeader.Rows[i]["ModificationDate"]);
                    ds.Header.Rows[i]["CompanyID"] = Simulate.Integer32(dtHeader.Rows[i]["CompanyID"]);
                    ds.Header.Rows[i]["PaymentMethodAName"] = Simulate.String(dtHeader.Rows[i]["PaymentMethodAName"]);
                }
            }

            return ds;
        }

        private static dsCashVoucher BuildCreditNoteDataSet(DataTable dtHeader, DataTable dtDetails)
        {
            dsCashVoucher ds = new dsCashVoucher();

            if (dtDetails != null && dtDetails.Rows.Count > 0)
            {
                for (int i = 0; i < dtDetails.Rows.Count; i++)
                {
                    ds.Details.Rows.Add();
                    ds.Details.Rows[i]["Guid"] = Simulate.String(dtDetails.Rows[i]["Guid"]);
                    ds.Details.Rows[i]["HeaderGuid"] = Simulate.String(dtDetails.Rows[i]["HeaderGuid"]);
                    ds.Details.Rows[i]["RowIndex"] = Simulate.String(Simulate.Integer32(dtDetails.Rows[i]["RowIndex"]) + 1);
                    ds.Details.Rows[i]["AccountID"] = Simulate.Integer32(dtDetails.Rows[i]["AccountID"]);
                    ds.Details.Rows[i]["SubAccountID"] = Simulate.Integer32(dtDetails.Rows[i]["SubAccountID"]);
                    ds.Details.Rows[i]["BranchID"] = Simulate.Integer32(dtDetails.Rows[i]["BranchID"]);
                    ds.Details.Rows[i]["CostCenterID"] = Simulate.Integer32(dtDetails.Rows[i]["CostCenterID"]);
                    ds.Details.Rows[i]["Debit"] = Simulate.decimal_(dtDetails.Rows[i]["Debit"]);
                    ds.Details.Rows[i]["Credit"] = Simulate.decimal_(dtDetails.Rows[i]["Credit"]);
                    ds.Details.Rows[i]["Total"] = Simulate.decimal_(dtDetails.Rows[i]["Total"]);
                    ds.Details.Rows[i]["Note"] = Simulate.String(dtDetails.Rows[i]["Note"]);
                    ds.Details.Rows[i]["VoucherType"] = Simulate.Integer32(dtDetails.Rows[i]["VoucherType"]);
                    ds.Details.Rows[i]["CompanyID"] = Simulate.Integer32(dtDetails.Rows[i]["CompanyID"]);
                    ds.Details.Rows[i]["BranchAName"] = Simulate.String(dtDetails.Rows[i]["BranchAName"]);
                    ds.Details.Rows[i]["AccountAName"] = Simulate.String(dtDetails.Rows[i]["AccountsAName"]);
                    ds.Details.Rows[i]["CostCenterAName"] = Simulate.String(dtDetails.Rows[i]["CostCenterAName"]);
                    ds.Details.Rows[i]["SubAccountAName"] = Simulate.String(dtDetails.Rows[i]["SubAccountAName"]);
                }
            }

            if (dtHeader != null && dtHeader.Rows.Count > 0)
            {
                for (int i = 0; i < dtHeader.Rows.Count; i++)
                {
                    ds.Header.Rows.Add();
                    ds.Header.Rows[i]["Guid"] = Simulate.String(dtHeader.Rows[i]["Guid"]);
                    ds.Header.Rows[i]["VoucherDate"] = Simulate.StringToDate(dtHeader.Rows[i]["VoucherDate"]).ToString("yyyy-MM-dd");
                    ds.Header.Rows[i]["BranchID"] = Simulate.Integer32(dtHeader.Rows[i]["BranchID"]);
                    ds.Header.Rows[i]["CostCenterID"] = Simulate.Integer32(dtHeader.Rows[i]["CostCenterID"]);
                    ds.Header.Rows[i]["Amount"] = Simulate.Currency_format(dtHeader.Rows[i]["Amount"]);
                    ds.Header.Rows[i]["JVGuid"] = Simulate.String(dtHeader.Rows[i]["JVGuid"]);
                    ds.Header.Rows[i]["Note"] = Simulate.String(dtHeader.Rows[i]["Note"]);
                    ds.Header.Rows[i]["VoucherNo"] = Simulate.Integer32(dtHeader.Rows[i]["VoucherNo"]);
                    ds.Header.Rows[i]["VoucherType"] = Simulate.Integer32(dtHeader.Rows[i]["VoucherType"]);
                    ds.Header.Rows[i]["BranchAName"] = Simulate.String(dtHeader.Rows[i]["BranchAName"]);
                    ds.Header.Rows[i]["CostCenterAName"] = Simulate.String(dtHeader.Rows[i]["CostCenterAName"]);
                    ds.Header.Rows[i]["CreationUserID"] = Simulate.Integer32(dtHeader.Rows[i]["CreationUserID"]);
                    ds.Header.Rows[i]["CreationDate"] = Simulate.StringToDate(dtHeader.Rows[i]["CreationDate"]);
                    ds.Header.Rows[i]["ModificationUserID"] = Simulate.Integer32(dtHeader.Rows[i]["ModificationUserID"]);
                    ds.Header.Rows[i]["ModificationDate"] = Simulate.StringToDate(dtHeader.Rows[i]["ModificationDate"]);
                    ds.Header.Rows[i]["CompanyID"] = Simulate.Integer32(dtHeader.Rows[i]["CompanyID"]);
                }
            }

            return ds;
        }

        private static void ApplyCashVoucherAmountParameters(Report report, DataTable dtHeader)
        {
            if (dtHeader == null || dtHeader.Rows.Count == 0)
                return;

            string amountToWord = clsConvertNumberToString.NoToTxt(
                Simulate.Val(dtHeader.Rows[0]["Amount"]));
            string amountWithoutDecimal = Simulate.String(
                Simulate.Integer32(dtHeader.Rows[0]["Amount"]));
            string amountDecimal = Simulate.String(Simulate.Integer32(
                (Simulate.Val(dtHeader.Rows[0]["Amount"]) - Simulate.Val(dtHeader.Rows[0]["Amount"])) * 1000));

            report.SetParameterValue("report.AmountWithOutDecimal", amountWithoutDecimal);
            report.SetParameterValue("report.AmountDecimal", amountDecimal);
            report.SetParameterValue("report.AmountToWord", amountToWord);
        }
    }
}
