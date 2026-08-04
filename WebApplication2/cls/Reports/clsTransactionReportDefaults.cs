using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using WebApplication2.cls;

namespace WebApplication2.cls.Reports
{
    /// <summary>
    /// Catalog of built-in FastReport (.frx) templates shipped with the API.
    /// Each entry is seeded per company in tbl_TransactionReport so users can customize layouts.
    /// </summary>
    public static class clsTransactionReportDefaults
    {
        public const string PageJournalVoucherAdd = "JournalVoucherAdd";
        public const string PageInvoicePageAdd = "InvoicePageAdd";
        public const string PageCashVoucherAdd = "CashVoucherAdd";
        public const string PageCreditNotePageAdd = "CreditNotePageAdd";
        public const string PageDebitNotePageAdd = "DebitNotePageAdd";
        public const string PagePaymentVoucherAdd = "PaymentVoucherAdd";
        public const string PageReceiptVoucherAdd = "ReceiptVoucherAdd";
        public const string PageSalesInvoicePageAdd = "SalesInvoicePageAdd";
        public const string PagePurchaseInvoicePageAdd = "PurchaseInvoicePageAdd";
        public const string PageEmployeeContractAdd = "EmployeeContractAdd";
        public const string PageTrialBalance = "TrialBalanceReport";
        public const string PageBalanceSheet = "BalanceSheetReport";
        public const string PageIncomeStatement = "IncomeStatementReport";
        public const string PageAccountStatement = "AccountStatementReport";
        public const string PageCashReport = "CashReportReport";
        public const string PageAging = "AgingReport";
        public const string PageBusinessPartnerBalances = "BusinessPartnerBalancesReport";
        public const string PageCheques = "ChequesReport";
        public const string PageCustomerLoans = "CustomerLoansReport";
        public const string PageFinancingReport = "FinancingReport";
        public const string PageFinancingDocument = "FinancingDocumentReport";
        public const string PageFinancingGuarantee = "FinancingGuaranteeReport";
        public const string PageFinancingSalesInvoice = "FinancingSalesInvoiceReport";
        public const string PageCashLoan = "CashLoanReport";
        public const string PageGift = "GiftReport";
        public const string PageInvoicesByFilter = "InvoicesByFilterReport";
        public const string PageItemTransactions = "ItemTransactionsReport";
        public const string PageInventory = "InventoryReport";
        public const string PageEmployeeLoans = "EmployeeLoansReport";
        public const string PageCashVoucherCheque = "CashVoucherChequeReport";
        public const string PageFinancingHeader = "FinancingHeaderAdd";

        public sealed class TransactionReportPageInfo
        {
            public string PageName { get; init; } = "";
            public string TitleEn { get; init; } = "";
            public string TitleAr { get; init; } = "";
            public string SubtitleEn { get; init; } = "";
        }

        public sealed class DefaultTransactionReportDefinition
        {
            public string PageName { get; init; } = "";
            public string ReportName { get; init; } = "";
            public string AName { get; init; } = "";
            public string EName { get; init; } = "";
            public string FrxFileName { get; init; } = "";
            public bool IsDefault { get; init; } = true;
            public int SortOrder { get; init; } = 1;
        }

        /// <summary>All known defaults mapped to their ERP page / print context.</summary>
        public static readonly IReadOnlyList<DefaultTransactionReportDefinition> BuiltIn = new[]
        {
            new DefaultTransactionReportDefinition
            {
                PageName = PageJournalVoucherAdd,
                ReportName = "DefaultJV",
                AName = "قيد يومية - افتراضي",
                EName = "Journal Voucher - Default",
                FrxFileName = "rptJV",
            },
            new DefaultTransactionReportDefinition
            {
                PageName = PageInvoicePageAdd,
                ReportName = "DefaultInvoice",
                AName = "فاتورة - افتراضي",
                EName = "Invoice - Default (Sales / Purchase)",
                FrxFileName = "rptInvoice",
            },
            new DefaultTransactionReportDefinition
            {
                PageName = PageInvoicePageAdd,
                ReportName = "DefaultInvoicePOS",
                AName = "فاتورة نقطة بيع",
                EName = "POS Invoice - Default",
                FrxFileName = "rptInvoicePOS",
                IsDefault = false,
                SortOrder = 2,
            },
            new DefaultTransactionReportDefinition
            {
                PageName = PageCashVoucherAdd,
                ReportName = "DefaultCashVoucher",
                AName = "سند صندوق - افتراضي",
                EName = "Cash Voucher - Default",
                FrxFileName = "rptCashVoucher",
            },
            new DefaultTransactionReportDefinition
            {
                PageName = PageCreditNotePageAdd,
                ReportName = "DefaultCreditNote",
                AName = "إشعار دائن - افتراضي",
                EName = "Credit Note - Default",
                FrxFileName = "rptCashVoucher",
            },
            new DefaultTransactionReportDefinition
            {
                PageName = PageDebitNotePageAdd,
                ReportName = "DefaultDebitNote",
                AName = "إشعار مدين - افتراضي",
                EName = "Debit Note - Default",
                FrxFileName = "rptCashVoucher",
            },
            new DefaultTransactionReportDefinition
            {
                PageName = PagePaymentVoucherAdd,
                ReportName = "DefaultPaymentVoucher",
                AName = "سند صرف - افتراضي",
                EName = "Payment Voucher - Default",
                FrxFileName = "rptCashVoucher",
            },
            new DefaultTransactionReportDefinition
            {
                PageName = PageReceiptVoucherAdd,
                ReportName = "DefaultReceiptVoucher",
                AName = "سند قبض - افتراضي",
                EName = "Receipt Voucher - Default",
                FrxFileName = "rptCashVoucher",
            },
            new DefaultTransactionReportDefinition
            {
                PageName = PageSalesInvoicePageAdd,
                ReportName = "DefaultSalesInvoice",
                AName = "فاتورة مبيعات - افتراضي",
                EName = "Sales Invoice - Default",
                FrxFileName = "rptInvoice",
            },
            new DefaultTransactionReportDefinition
            {
                PageName = PagePurchaseInvoicePageAdd,
                ReportName = "DefaultPurchaseInvoice",
                AName = "فاتورة مشتريات - افتراضي",
                EName = "Purchase Invoice - Default",
                FrxFileName = "rptInvoice",
            },
            new DefaultTransactionReportDefinition
            {
                PageName = PageEmployeeContractAdd,
                ReportName = "DefaultEmployeeContract",
                AName = "عقد عمل - افتراضي",
                EName = "Employee Contract - Default",
                FrxFileName = "rptEmployeeContract",
            },
            new DefaultTransactionReportDefinition
            {
                PageName = PageTrialBalance,
                ReportName = "DefaultTrialBalance",
                AName = "ميزان مراجعة - افتراضي",
                EName = "Trial Balance - Default",
                FrxFileName = "rptTrialBalance",
            },
            new DefaultTransactionReportDefinition
            {
                PageName = PageBalanceSheet,
                ReportName = "DefaultBalanceSheet",
                AName = "ميزانية - افتراضي",
                EName = "Balance Sheet - Default",
                FrxFileName = "rptBalanceSheet",
            },
            new DefaultTransactionReportDefinition
            {
                PageName = PageIncomeStatement,
                ReportName = "DefaultIncomeStatement",
                AName = "قائمة دخل - افتراضي",
                EName = "Income Statement - Default",
                FrxFileName = "rptIncomeStatement",
            },
            new DefaultTransactionReportDefinition
            {
                PageName = PageAccountStatement,
                ReportName = "DefaultAccountStatement",
                AName = "كشف حساب - افتراضي",
                EName = "Account Statement - Default",
                FrxFileName = "rptAccountStatement",
            },
            new DefaultTransactionReportDefinition
            {
                PageName = PageCashReport,
                ReportName = "DefaultCashReport",
                AName = "تقرير الصندوق - افتراضي",
                EName = "Cash Report - Default",
                FrxFileName = "rptCashReport",
            },
            new DefaultTransactionReportDefinition
            {
                PageName = PageAging,
                ReportName = "DefaultAging",
                AName = "أعمار الذمم - افتراضي",
                EName = "Aging Report - Default",
                FrxFileName = "rptAging",
            },
            new DefaultTransactionReportDefinition
            {
                PageName = PageBusinessPartnerBalances,
                ReportName = "DefaultBusinessPartnerBalances",
                AName = "أرصدة الشركاء - افتراضي",
                EName = "Business Partner Balances - Default",
                FrxFileName = "rptBusinessPartnerReports",
            },
            new DefaultTransactionReportDefinition
            {
                PageName = PageCheques,
                ReportName = "DefaultCheques",
                AName = "الشيكات - افتراضي",
                EName = "Cheques - Default",
                FrxFileName = "rptCheques",
            },
            new DefaultTransactionReportDefinition
            {
                PageName = PageCustomerLoans,
                ReportName = "DefaultCustomerLoans",
                AName = "قروض العملاء - افتراضي",
                EName = "Customer Loans - Default",
                FrxFileName = "rptCutomerLoansReport",
            },
            new DefaultTransactionReportDefinition
            {
                PageName = PageFinancingReport,
                ReportName = "DefaultFinancingReport",
                AName = "تقرير التمويل - افتراضي",
                EName = "Financing Report - Default",
                FrxFileName = "rptFinancingReport",
            },
            new DefaultTransactionReportDefinition
            {
                PageName = PageFinancingDocument,
                ReportName = "DefaultFinancing",
                AName = "مستند تمويل - افتراضي",
                EName = "Financing Document - Default",
                FrxFileName = "rptFinancing",
            },
            new DefaultTransactionReportDefinition
            {
                PageName = PageFinancingGuarantee,
                ReportName = "DefaultFinancingGuarantee",
                AName = "ضمان تمويل - افتراضي",
                EName = "Financing Guarantee - Default",
                FrxFileName = "rptFinancingGuarantee",
            },
            new DefaultTransactionReportDefinition
            {
                PageName = PageFinancingSalesInvoice,
                ReportName = "DefaultFinancingSalesInvoice",
                AName = "فاتورة تمويل - افتراضي",
                EName = "Financing Sales Invoice - Default",
                FrxFileName = "rptFinancingSalesInvoice",
            },
            new DefaultTransactionReportDefinition
            {
                PageName = PageCashLoan,
                ReportName = "DefaultCashLoan",
                AName = "قرض نقدي - افتراضي",
                EName = "Cash Loan - Default",
                FrxFileName = "rptCashLoan",
            },
            new DefaultTransactionReportDefinition
            {
                PageName = PageGift,
                ReportName = "DefaultGift",
                AName = "هدية - افتراضي",
                EName = "Gift Voucher - Default",
                FrxFileName = "rptGift",
            },
            new DefaultTransactionReportDefinition
            {
                PageName = PageInvoicesByFilter,
                ReportName = "DefaultInvoicesByFilter",
                AName = "تقرير الفواتير - افتراضي",
                EName = "Invoices By Filter - Default",
                FrxFileName = "rptAccountStatement",
            },
            new DefaultTransactionReportDefinition
            {
                PageName = PageItemTransactions,
                ReportName = "DefaultItemTransactions",
                AName = "حركة الأصناف - افتراضي",
                EName = "Item Transactions - Default",
                FrxFileName = "rptAccountStatement",
            },
            new DefaultTransactionReportDefinition
            {
                PageName = PageInventory,
                ReportName = "DefaultInventoryReport",
                AName = "تقرير المخزون - افتراضي",
                EName = "Inventory Report - Default",
                FrxFileName = "rptAccountStatement",
            },
            new DefaultTransactionReportDefinition
            {
                PageName = PageEmployeeLoans,
                ReportName = "DefaultEmployeeLoans",
                AName = "قروض الموظفين - افتراضي",
                EName = "Employee Loans - Default",
                FrxFileName = "rptCutomerLoansReport",
            },
            new DefaultTransactionReportDefinition
            {
                PageName = PageCashVoucherCheque,
                ReportName = "DefaultCashVoucherCheque",
                AName = "شيك سند صندوق - افتراضي",
                EName = "Cash Voucher Cheque - Default",
                FrxFileName = "rptCheques",
            },
            new DefaultTransactionReportDefinition
            {
                PageName = PageFinancingHeader,
                ReportName = "DefaultFinancingHeader",
                AName = "مستند تمويل (رأس) - افتراضي",
                EName = "Financing Header - Default",
                FrxFileName = "rptFinancing",
            },
        };

        /// <summary>Returns all .frx files in Reports\ (top level only).</summary>
        public static IReadOnlyList<string> DiscoverGlobalFrxFileNames()
        {
            try
            {
                string reportsDir = Path.Combine(Environment.CurrentDirectory, "Reports");
                if (!Directory.Exists(reportsDir))
                    return Array.Empty<string>();

                return Directory
                    .GetFiles(reportsDir, "*.frx", SearchOption.TopDirectoryOnly)
                    .Select(Path.GetFileNameWithoutExtension)
                    .Where(n => !string.IsNullOrWhiteSpace(n))
                    .OrderBy(n => n, StringComparer.OrdinalIgnoreCase)
                    .ToList();
            }
            catch
            {
                return Array.Empty<string>();
            }
        }

        /// <summary>
        /// Built-in catalog plus any extra .frx files found on disk (auto-registered under Report_{name} pages).
        /// </summary>
        public static IReadOnlyList<DefaultTransactionReportDefinition> GetAllDefinitions()
        {
            var list = new List<DefaultTransactionReportDefinition>(BuiltIn);
            var knownFrx = new HashSet<string>(
                BuiltIn.Select(d => d.FrxFileName),
                StringComparer.OrdinalIgnoreCase);

            foreach (string frx in DiscoverGlobalFrxFileNames())
            {
                if (knownFrx.Contains(frx))
                    continue;

                list.Add(new DefaultTransactionReportDefinition
                {
                    PageName = $"Report_{frx}",
                    ReportName = $"Default_{frx}",
                    AName = $"{frx} - افتراضي",
                    EName = $"{frx} - Default",
                    FrxFileName = frx,
                });
                knownFrx.Add(frx);
            }

            return list;
        }

        public static int ApplyDefaultSeeds(int companyId, int creationUserId)
        {
            if (!clsTransactionReportPrint.TransactionReportTableExists(companyId))
            {
                clsTransactionReportPrint.TryEnsureTransactionReportSchema(companyId);
                if (!clsTransactionReportPrint.TransactionReportTableExists(companyId))
                    return 0;
            }

            clsTransactionReport dal = new clsTransactionReport();
            int inserted = 0;
            var errors = new System.Collections.Generic.List<string>();
            foreach (var def in GetAllDefinitions())
            {
                try
                {
                    // Include inactive rows so we do not hit unique-key insert failures.
                    DataTable existing = dal.SelectTransactionReportByPageAndNameAny(
                        def.PageName, def.ReportName, companyId);

                    int reportId;
                    bool isNew = existing == null || existing.Rows.Count == 0;
                    if (isNew)
                    {
                        reportId = dal.InsertTransactionReport(
                            def.PageName,
                            def.ReportName,
                            def.AName,
                            def.EName,
                            clsTransactionReportPrint.EngineFastReport,
                            def.FrxFileName,
                            null,
                            def.IsDefault,
                            true,
                            def.SortOrder,
                            companyId,
                            creationUserId);
                        if (reportId > 0)
                            inserted++;
                        else
                            errors.Add($"{def.PageName}/{def.ReportName}: insert returned 0");
                    }
                    else
                    {
                        reportId = Simulate.Integer32(existing.Rows[0]["ID"]);
                        // Always refresh metadata and force active so defaults appear in Settings.
                        dal.UpdateTransactionReport(
                            reportId,
                            def.PageName,
                            def.ReportName,
                            def.AName,
                            def.EName,
                            clsTransactionReportPrint.EngineFastReport,
                            def.FrxFileName,
                            null,
                            def.IsDefault,
                            true,
                            def.SortOrder,
                            creationUserId,
                            companyId);
                    }

                    if (reportId <= 0)
                        continue;

                    // Do not copy standard .frx into ReportFrxXml.
                    // Shared Reports\*.frx stays the base; ReportFrxXml is only for uploads.
                }
                catch (Exception ex)
                {
                    errors.Add($"{def.PageName}/{def.ReportName}: {ex.Message}");
                }
            }

            LastSeedErrors = errors;
            return inserted;
        }

        public static System.Collections.Generic.IReadOnlyList<string> LastSeedErrors { get; private set; }
            = System.Array.Empty<string>();

        /// <summary>
        /// Legacy helper: previously copied shipped .frx into ReportFrxXml.
        /// Now clears seeded copies that match the global standard so disk templates are used.
        /// True customer uploads (different from the global file) are preserved.
        /// </summary>
        public static int ApplyStandardFrxFromFiles(int companyId, int userId)
        {
            return ClearSeededFrxMatchingGlobalStandard(companyId, userId);
        }

        /// <summary>
        /// Clears ReportFrxXml when it is only a copy of Reports\{file}.frx (not a real customization).
        /// After clear, print uses Reports\{CompanyID}\ then Reports\ shared base.
        /// </summary>
        public static int ClearSeededFrxMatchingGlobalStandard(int companyId, int userId)
        {
            if (!clsTransactionReportPrint.TransactionReportTableExists(companyId))
                return 0;

            clsTransactionReport dal = new clsTransactionReport();
            clsReports reports = new clsReports();
            DataTable all = dal.SelectTransactionReportList("", companyId);
            if (all == null || all.Rows.Count == 0)
                return 0;

            int cleared = 0;
            foreach (DataRow row in all.Rows)
            {
                int reportId = Simulate.Integer32(row["ID"]);
                // List query masks XML as '1' — load full row.
                DataTable full = dal.SelectTransactionReportByID(reportId, companyId);
                if (full == null || full.Rows.Count == 0)
                    continue;

                string frxName = Simulate.String(full.Rows[0]["FastReportFileName"]);
                string currentXml = full.Columns.Contains("ReportFrxXml")
                    ? Simulate.String(full.Rows[0]["ReportFrxXml"])
                    : "";

                if (string.IsNullOrWhiteSpace(currentXml))
                    continue;

                // Only drop copies of the shipped standard. XML that fails validation is already
                // ignored by LoadFastReportTemplate, so deleting it would silently destroy a
                // customer upload every time Settings is opened.
                string globalPath = reports.getStandardGlobalPath(frxName);
                if (!clsTransactionReportPrint.IsSeededCopyOfGlobalStandard(currentXml, globalPath))
                    continue;

                if (dal.ClearReportFrxXml(reportId, userId, companyId) > 0)
                    cleared++;
            }

            return cleared;
        }

        /// <summary>Reads the shared product-standard .frx (Reports\{name}.frx), never company override.</summary>
        public static string ReadStandardFrxXmlFromDisk(int companyId, string frxFileName)
        {
            if (string.IsNullOrWhiteSpace(frxFileName))
                return "";

            try
            {
                clsReports reports = new clsReports();
                string path = reports.getStandardGlobalPath(frxFileName);
                if (!File.Exists(path))
                    return "";

                return File.ReadAllText(path, Encoding.UTF8);
            }
            catch
            {
                return "";
            }
        }

        /// <summary>
        /// Stores the shipped .frx as ReportFrxXml only when explicitly requested (overwrite).
        /// Prefer leaving ReportFrxXml empty so the shared file remains the base.
        /// </summary>
        public static bool SyncStandardFrxFromFile(
            int reportId,
            int companyId,
            string frxFileName,
            int userId,
            bool overwriteExisting = false)
        {
            if (reportId <= 0 || string.IsNullOrWhiteSpace(frxFileName))
                return false;

            clsTransactionReport dal = new clsTransactionReport();
            DataTable dt = dal.SelectTransactionReportByID(reportId, companyId);
            if (dt == null || dt.Rows.Count == 0)
                return false;

            string currentXml = dt.Columns.Contains("ReportFrxXml")
                ? Simulate.String(dt.Rows[0]["ReportFrxXml"])
                : "";

            if (!overwriteExisting && clsTransactionReportPrint.IsValidFrxXml(currentXml))
                return false;

            // Default behavior: clear DB copy so shared file is the base.
            if (!overwriteExisting)
                return dal.ClearReportFrxXml(reportId, userId, companyId) > 0;

            string standardXml = ReadStandardFrxXmlFromDisk(companyId, frxFileName);
            if (string.IsNullOrWhiteSpace(standardXml) || standardXml.Trim().Length < 100)
                return false;

            if (!clsTransactionReportPrint.IsValidFrxXml(standardXml) &&
                !standardXml.Contains("<Report", StringComparison.OrdinalIgnoreCase))
                return false;

            return dal.UpdateReportFrxXml(reportId, standardXml, userId, companyId) > 0;
        }

        public static string ResolvePageNameForFrx(string frxFileName)
        {
            var match = BuiltIn.FirstOrDefault(d =>
                string.Equals(d.FrxFileName, frxFileName, StringComparison.OrdinalIgnoreCase));
            return match?.PageName ?? $"Report_{frxFileName}";
        }

        /// <summary>Maps UI page names to the print/prepare handler page.</summary>
        public static string ResolvePrintPageName(string pageName)
        {
            if (string.IsNullOrWhiteSpace(pageName))
                return pageName;

            return pageName switch
            {
                PagePaymentVoucherAdd or PageReceiptVoucherAdd => PageCashVoucherAdd,
                PageDebitNotePageAdd => PageCreditNotePageAdd,
                PageSalesInvoicePageAdd or PagePurchaseInvoicePageAdd => PageInvoicePageAdd,
                _ => pageName,
            };
        }

        /// <summary>Distinct pages for settings UI (one row per PageName).</summary>
        public static IReadOnlyList<TransactionReportPageInfo> GetPageCatalog()
        {
            return GetAllDefinitions()
                .GroupBy(d => d.PageName, StringComparer.OrdinalIgnoreCase)
                .Select(g =>
                {
                    var d = g.OrderBy(x => x.SortOrder).First();
                    return new TransactionReportPageInfo
                    {
                        PageName = d.PageName,
                        TitleEn = d.EName,
                        TitleAr = d.AName,
                        SubtitleEn = d.FrxFileName,
                    };
                })
                .OrderBy(p => p.PageName, StringComparer.OrdinalIgnoreCase)
                .ToList();
        }
    }
}
