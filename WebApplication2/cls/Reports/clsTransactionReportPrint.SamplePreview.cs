using FastReport;
using System;
using System.Data;
using WebApplication2.DataSet;
using WebApplication2.MainClasses;

namespace WebApplication2.cls.Reports
{
    /// <summary>
    /// Builds layout preview PDFs using synthetic sample rows so Settings → Preview
    /// works even when the company has no real transactions yet.
    /// </summary>
    public partial class clsTransactionReportPrint
    {
        public byte[] BuildSamplePreviewPdf(
            string pageName,
            int userId,
            int companyId,
            int transactionReportId = 0)
        {
            TryEnsureTransactionReportSchema(companyId);
            EnsureAllDefaultTransactionReports(companyId, userId);

            ResolvedTransactionReport config = ResolveForPrint(
                pageName, "", companyId, userId, transactionReportId);

            if (string.Equals(config.ReportEngine, EngineJsonTemplate, StringComparison.OrdinalIgnoreCase))
                throw new InvalidOperationException(
                    "JSON template preview needs a saved layout. Open Customize for JSON reports.");

            Report report = new Report();
            RegisterSampleData(report, config.PageName, config.FastReportFileName);
            LoadFastReportTemplate(report, config, companyId);
            EnableRegisteredDataSources(report);
            ApplySampleParameters(report, config.PageName, userId, companyId);
            return ExportReportToPdf(report);
        }

        private static void EnableRegisteredDataSources(Report report)
        {
            try
            {
                for (int i = 0; i < report.Dictionary.DataSources.Count; i++)
                    report.Dictionary.DataSources[i].Enabled = true;
            }
            catch
            {
                // Best-effort; Prepare will still run.
            }
        }

        private void RegisterSampleData(Report report, string pageName, string frxFileName)
        {
            string printPage = clsTransactionReportDefaults.ResolvePrintPageName(pageName);
            string frx = (frxFileName ?? "").Trim();

            if (printPage == PageJournalVoucherAdd ||
                string.Equals(frx, "rptJV", StringComparison.OrdinalIgnoreCase))
            {
                report.RegisterData(BuildSampleJvDetails());
                return;
            }

            if (printPage == PageInvoicePageAdd ||
                string.Equals(frx, "rptInvoice", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(frx, "rptInvoicePOS", StringComparison.OrdinalIgnoreCase))
            {
                report.RegisterData(BuildSampleInvoiceDetails());
                return;
            }

            if (printPage == PageCashVoucherAdd ||
                printPage == PageCreditNotePageAdd ||
                string.Equals(frx, "rptCashVoucher", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(frx, "rptCheques", StringComparison.OrdinalIgnoreCase))
            {
                report.RegisterData(BuildSampleCashVoucher());
                return;
            }

            if (string.Equals(frx, "rptTrialBalance", StringComparison.OrdinalIgnoreCase) ||
                pageName == clsTransactionReportDefaults.PageTrialBalance)
            {
                report.RegisterData(BuildSampleTrialBalance());
                return;
            }

            if (string.Equals(frx, "rptBalanceSheet", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(frx, "rptIncomeStatement", StringComparison.OrdinalIgnoreCase) ||
                pageName == clsTransactionReportDefaults.PageBalanceSheet ||
                pageName == clsTransactionReportDefaults.PageIncomeStatement)
            {
                report.RegisterData(BuildSampleIncomeStatement());
                return;
            }

            if (string.Equals(frx, "rptAging", StringComparison.OrdinalIgnoreCase) ||
                pageName == clsTransactionReportDefaults.PageAging)
            {
                report.RegisterData(BuildSampleAging());
                return;
            }

            if (string.Equals(frx, "rptBusinessPartnerReports", StringComparison.OrdinalIgnoreCase) ||
                pageName == clsTransactionReportDefaults.PageBusinessPartnerBalances)
            {
                report.RegisterData(BuildSampleBusinessPartnerBalances());
                return;
            }

            if (string.Equals(frx, "rptCashReport", StringComparison.OrdinalIgnoreCase) ||
                pageName == clsTransactionReportDefaults.PageCashReport)
            {
                report.RegisterData(BuildSampleCashReport());
                return;
            }

            if (string.Equals(frx, "rptFinancingReport", StringComparison.OrdinalIgnoreCase) ||
                pageName == clsTransactionReportDefaults.PageFinancingReport)
            {
                report.RegisterData(BuildSampleFinancingReport());
                return;
            }

            if (string.Equals(frx, "rptCutomerLoansReport", StringComparison.OrdinalIgnoreCase) ||
                pageName == clsTransactionReportDefaults.PageCustomerLoans ||
                pageName == clsTransactionReportDefaults.PageEmployeeLoans)
            {
                report.RegisterData(BuildSampleEmployeeLoans());
                return;
            }

            if (string.Equals(frx, "rptFinancing", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(frx, "rptFinancingGuarantee", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(frx, "rptFinancingSalesInvoice", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(frx, "rptCashLoan", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(frx, "rptGift", StringComparison.OrdinalIgnoreCase) ||
                pageName == clsTransactionReportDefaults.PageFinancingDocument ||
                pageName == clsTransactionReportDefaults.PageFinancingHeader ||
                pageName == clsTransactionReportDefaults.PageFinancingGuarantee ||
                pageName == clsTransactionReportDefaults.PageFinancingSalesInvoice ||
                pageName == clsTransactionReportDefaults.PageCashLoan ||
                pageName == clsTransactionReportDefaults.PageGift)
            {
                report.RegisterData(BuildSampleFinancing());
                report.RegisterData(BuildSampleBusinessPartner());
                report.RegisterData(BuildSampleJvDetails());
                return;
            }

            if (string.Equals(frx, "rptEmployeeContract", StringComparison.OrdinalIgnoreCase) ||
                pageName == clsTransactionReportDefaults.PageEmployeeContractAdd)
            {
                report.RegisterData(BuildSampleEmployeeContract());
                return;
            }

            // Account statement + inventory-style reports that reuse that dataset
            report.RegisterData(BuildSampleAccountStatement());
        }

        private void ApplySampleParameters(Report report, string pageName, int userId, int companyId)
        {
            string today = DateTime.Now.ToString("yyyy-MM-dd");
            string from = DateTime.Now.AddMonths(-1).ToString("yyyy-MM-dd");

            TrySetParameter(report, "report.Branch", "Sample Branch");
            TrySetParameter(report, "report.CostCenter", "Sample Cost Center");
            TrySetParameter(report, "report.FromDate", from);
            TrySetParameter(report, "report.ToDate", today);
            TrySetParameter(report, "report.Date", today);
            TrySetParameter(report, "report.AccountName", "Sample Account");
            TrySetParameter(report, "report.AccountNumber", "1000");
            TrySetParameter(report, "report.JVNo", "JV-SAMPLE-001");
            TrySetParameter(report, "report.CreationUser", "Sample User");
            TrySetParameter(report, "report.JournalVoucherTypes", "Sample Voucher Type");
            TrySetParameter(report, "report.BusinessPartner", "Sample Customer");
            TrySetParameter(report, "report.CashDrawer", "Main Cash");
            TrySetParameter(report, "report.PaymentMethod", "Cash");
            TrySetParameter(report, "report.ContractNumber", "CTR-001");
            TrySetParameter(report, "report.ContractID", "1");
            TrySetParameter(report, "VoucherDate", today);
            TrySetParameter(report, "Name", "Sample Payee");
            TrySetParameter(report, "Amount", "1,000.000");
            TrySetParameter(report, "amountfils", "000");
            TrySetParameter(report, "AmountTafkeet", "One Thousand Only");
            TrySetParameter(report, "Notes", "Sample preview layout");
            TrySetParameter(report, "Factor", "1");
            TrySetParameter(report, "Date1", "0-30");
            TrySetParameter(report, "Date2", "31-60");
            TrySetParameter(report, "Date3", "61-90");
            TrySetParameter(report, "Date4", "91-120");
            TrySetParameter(report, "Date5", "121-180");
            TrySetParameter(report, "Date6", "180+");
            TrySetParameter(report, "IsPosDate", "0");
            TrySetParameter(report, "CashDrawer", "Main Cash");
            TrySetParameter(report, "JournalVoucherTypes", "Sales");
            TrySetParameter(report, "Branch", "Sample Branch");
            TrySetParameter(report, "TotalDue", "1000");
            TrySetParameter(report, "Amount", "1000");
            TrySetParameter(report, "DueDate", today);

            _reportsHelper.FastreportStanderdParameters(report, userId, companyId);
        }

        private static void TrySetParameter(Report report, string name, object value)
        {
            try
            {
                report.SetParameterValue(name, value);
            }
            catch
            {
                // Parameter may not exist on this template.
            }
        }

        private static void SetCol(DataRow row, string column, object value)
        {
            if (row?.Table == null || !row.Table.Columns.Contains(column))
                return;
            row[column] = value ?? DBNull.Value;
        }

        private static dsJVDetails BuildSampleJvDetails()
        {
            var ds = new dsJVDetails();
            DataRow r1 = ds.JVDetails.NewRow();
            SetCol(r1, "Guid", Guid.NewGuid().ToString());
            SetCol(r1, "ParentGuid", Guid.NewGuid().ToString());
            SetCol(r1, "RowIndex", "1");
            SetCol(r1, "AccountID", "1");
            SetCol(r1, "AccountName", "Cash");
            SetCol(r1, "SubAccountID", "0");
            SetCol(r1, "SubAccountName", "");
            SetCol(r1, "Debit", 1000m);
            SetCol(r1, "Credit", 0m);
            SetCol(r1, "Total", 1000m);
            SetCol(r1, "BranchName", "Sample Branch");
            SetCol(r1, "CostCenterName", "Admin");
            SetCol(r1, "Note", "Sample debit line");
            ds.JVDetails.Rows.Add(r1);

            DataRow r2 = ds.JVDetails.NewRow();
            SetCol(r2, "Guid", Guid.NewGuid().ToString());
            SetCol(r2, "ParentGuid", r1["ParentGuid"]);
            SetCol(r2, "RowIndex", "2");
            SetCol(r2, "AccountID", "2");
            SetCol(r2, "AccountName", "Revenue");
            SetCol(r2, "Debit", 0m);
            SetCol(r2, "Credit", 1000m);
            SetCol(r2, "Total", 1000m);
            SetCol(r2, "BranchName", "Sample Branch");
            SetCol(r2, "CostCenterName", "Admin");
            SetCol(r2, "Note", "Sample credit line");
            ds.JVDetails.Rows.Add(r2);
            return ds;
        }

        private static dsInvoiceDetails BuildSampleInvoiceDetails()
        {
            var ds = new dsInvoiceDetails();
            string headerGuid = Guid.NewGuid().ToString();
            DataRow r = ds.InvoiceDetails.NewRow();
            SetCol(r, "Guid", Guid.NewGuid().ToString());
            SetCol(r, "HeaderGuid", headerGuid);
            SetCol(r, "RowIndex", "1");
            SetCol(r, "ItemGuid", Guid.NewGuid().ToString());
            SetCol(r, "ItemName", "Sample Item");
            SetCol(r, "Qty", 2m);
            SetCol(r, "PriceBeforeTax", 50m);
            SetCol(r, "DiscountBeforeTaxAmount", 0m);
            SetCol(r, "TaxID", "1");
            SetCol(r, "TaxPercentage", "16");
            SetCol(r, "TaxAmount", 16m);
            SetCol(r, "TotalLine", 116m);
            SetCol(r, "InvoiceDate", DateTime.Now.ToString("yyyy-MM-dd"));
            SetCol(r, "BusinessPartnerID", "1");
            ds.InvoiceDetails.Rows.Add(r);

            DataRow r2 = ds.InvoiceDetails.NewRow();
            SetCol(r2, "Guid", Guid.NewGuid().ToString());
            SetCol(r2, "HeaderGuid", headerGuid);
            SetCol(r2, "RowIndex", "2");
            SetCol(r2, "ItemName", "Sample Item 2");
            SetCol(r2, "Qty", 1m);
            SetCol(r2, "PriceBeforeTax", 100m);
            SetCol(r2, "TaxAmount", 16m);
            SetCol(r2, "TotalLine", 116m);
            ds.InvoiceDetails.Rows.Add(r2);
            return ds;
        }

        private static dsCashVoucher BuildSampleCashVoucher()
        {
            var ds = new dsCashVoucher();
            string headerGuid = Guid.NewGuid().ToString();
            DataRow h = ds.Header.NewRow();
            SetCol(h, "Guid", headerGuid);
            SetCol(h, "VoucherDate", DateTime.Now.ToString("yyyy-MM-dd"));
            SetCol(h, "VoucherNo", "CV-001");
            SetCol(h, "Amount", 500m);
            SetCol(h, "BranchAName", "Sample Branch");
            SetCol(h, "CashDrawerAName", "Main Cash");
            SetCol(h, "JournalVoucherTypesAname", "Receipt");
            ds.Header.Rows.Add(h);

            DataRow d = ds.Details.NewRow();
            SetCol(d, "Guid", Guid.NewGuid().ToString());
            SetCol(d, "HeaderGuid", headerGuid);
            SetCol(d, "RowIndex", "1");
            SetCol(d, "AccountAName", "Customer Receipts");
            SetCol(d, "Debit", 500m);
            SetCol(d, "Credit", 0m);
            SetCol(d, "Total", 500m);
            SetCol(d, "Note", "Sample cash line");
            ds.Details.Rows.Add(d);
            return ds;
        }

        private static dsAccountStatment BuildSampleAccountStatement()
        {
            var ds = new dsAccountStatment();
            DataRow r = ds.AccountStatment.NewRow();
            SetCol(r, "ID", 1);
            SetCol(r, "AccountID", "1");
            SetCol(r, "SubAccountID", "0");
            SetCol(r, "Debit", 250m);
            SetCol(r, "Credit", 0m);
            SetCol(r, "total", 250m);
            SetCol(r, "NetTotal", 250m);
            SetCol(r, "BranchName", "Sample Branch");
            SetCol(r, "CostCenterName", "Admin");
            SetCol(r, "VoucherDate", DateTime.Now.ToString("yyyy-MM-dd"));
            SetCol(r, "VoucherType", "Journal Voucher");
            SetCol(r, "JVNumber", "JV-100");
            SetCol(r, "AccountEname", "Sample Account");
            SetCol(r, "AccountNumber", "1000");
            SetCol(r, "Note", "Sample statement line");
            ds.AccountStatment.Rows.Add(r);

            DataRow r2 = ds.AccountStatment.NewRow();
            SetCol(r2, "ID", 2);
            SetCol(r2, "Debit", 0m);
            SetCol(r2, "Credit", 100m);
            SetCol(r2, "total", -100m);
            SetCol(r2, "NetTotal", 150m);
            SetCol(r2, "BranchName", "Sample Branch");
            SetCol(r2, "VoucherDate", DateTime.Now.AddDays(-2).ToString("yyyy-MM-dd"));
            SetCol(r2, "VoucherType", "Payment");
            SetCol(r2, "JVNumber", "PV-50");
            SetCol(r2, "AccountEname", "Sample Account");
            SetCol(r2, "AccountNumber", "1000");
            SetCol(r2, "Note", "Sample credit");
            ds.AccountStatment.Rows.Add(r2);
            return ds;
        }

        private static dsTrialBalance BuildSampleTrialBalance()
        {
            var ds = new dsTrialBalance();
            DataRow r = ds.TrialBalance.NewRow();
            SetCol(r, "ID", 1);
            SetCol(r, "AccountNumber", "1000");
            SetCol(r, "AName", "Cash");
            SetCol(r, "EName", "Cash");
            SetCol(r, "OpeningBalance", 500m);
            SetCol(r, "Debit", 200m);
            SetCol(r, "Credit", 50m);
            SetCol(r, "EndingBalance", 650m);
            SetCol(r, "ChildCount", 0);
            ds.TrialBalance.Rows.Add(r);

            DataRow r2 = ds.TrialBalance.NewRow();
            SetCol(r2, "ID", 2);
            SetCol(r2, "AccountNumber", "4000");
            SetCol(r2, "AName", "Sales");
            SetCol(r2, "EName", "Sales");
            SetCol(r2, "OpeningBalance", 0m);
            SetCol(r2, "Debit", 0m);
            SetCol(r2, "Credit", 200m);
            SetCol(r2, "EndingBalance", -200m);
            ds.TrialBalance.Rows.Add(r2);
            return ds;
        }

        private static dsIncomeStatement BuildSampleIncomeStatement()
        {
            var ds = new dsIncomeStatement();
            DataRow r = ds.IncomeStatement.NewRow();
            SetCol(r, "ID", 1);
            SetCol(r, "AccountNumber", "4000");
            SetCol(r, "AName", "Revenue");
            SetCol(r, "EName", "Revenue");
            SetCol(r, "parentid", 0);
            SetCol(r, "isparent", 0);
            SetCol(r, "balance", 5000m);
            ds.IncomeStatement.Rows.Add(r);

            DataRow r2 = ds.IncomeStatement.NewRow();
            SetCol(r2, "ID", 2);
            SetCol(r2, "AccountNumber", "5000");
            SetCol(r2, "AName", "Expenses");
            SetCol(r2, "EName", "Expenses");
            SetCol(r2, "balance", 1200m);
            ds.IncomeStatement.Rows.Add(r2);
            return ds;
        }

        private static dsAgingReports BuildSampleAging()
        {
            var ds = new dsAgingReports();
            DataRow r = ds.AgingReports.NewRow();
            SetCol(r, "Index", 1);
            SetCol(r, "ID", 1);
            SetCol(r, "BBAName", "Sample Customer");
            SetCol(r, "Date1", 100m);
            SetCol(r, "Date2", 50m);
            SetCol(r, "Date3", 25m);
            SetCol(r, "Date4", 10m);
            SetCol(r, "Date5", 5m);
            SetCol(r, "Date6", 0m);
            SetCol(r, "Date7", 190m);
            ds.AgingReports.Rows.Add(r);
            return ds;
        }

        private static dsBusinessPartnerReports BuildSampleBusinessPartnerBalances()
        {
            var ds = new dsBusinessPartnerReports();
            DataRow r = ds.BusinessPartnerReports.NewRow();
            SetCol(r, "Index", 1);
            SetCol(r, "ID", 1);
            SetCol(r, "BBAName", "Sample Partner");
            SetCol(r, "AccountAName", "Receivables");
            SetCol(r, "Total", 1500m);
            SetCol(r, "Due", 300m);
            SetCol(r, "EMPCode", "E001");
            ds.BusinessPartnerReports.Rows.Add(r);
            return ds;
        }

        private static dsCashReport BuildSampleCashReport()
        {
            var ds = new dsCashReport();
            DataRow r = ds.CashReport.NewRow();
            SetCol(r, "InvoiceDate", DateTime.Now.ToString("yyyy-MM-dd"));
            SetCol(r, "PaymentMethod", "Cash");
            SetCol(r, "BusinessPartner", "Walk-in");
            SetCol(r, "InvoiceCount", 3);
            SetCol(r, "TotalTax", 48m);
            SetCol(r, "HeaderDiscount", 0m);
            SetCol(r, "TotalDiscount", 0m);
            SetCol(r, "TotalInvoice", 348m);
            ds.CashReport.Rows.Add(r);
            return ds;
        }

        private static dsFinancingReport BuildSampleFinancingReport()
        {
            var ds = new dsFinancingReport();
            DataRow h = ds.DataTableH.NewRow();
            SetCol(h, "Date1", DateTime.Now.AddMonths(-1).ToString("yyyy-MM-dd"));
            SetCol(h, "Date2", DateTime.Now.ToString("yyyy-MM-dd"));
            SetCol(h, "BranchName", "Sample Branch");
            SetCol(h, "CompanyName", "Sample Company");
            ds.DataTableH.Rows.Add(h);

            DataRow d = ds.DataTableD.NewRow();
            SetCol(d, "Index", 1);
            SetCol(d, "Customer", "Sample Customer");
            SetCol(d, "Descrption", "Sample financing line");
            SetCol(d, "QTY", 1m);
            SetCol(d, "Price", 1000m);
            SetCol(d, "Total", 1000m);
            ds.DataTableD.Rows.Add(d);
            return ds;
        }

        private static dsEmployeeLoans BuildSampleEmployeeLoans()
        {
            var ds = new dsEmployeeLoans();
            DataRow r = ds.DataTable1.NewRow();
            SetCol(r, "Index", 1);
            SetCol(r, "VoucherNumber", "LN-001");
            SetCol(r, "BusinessPartnerAName", "Sample Employee");
            SetCol(r, "EmpCode", "E100");
            SetCol(r, "VoucherDate", DateTime.Now.ToString("yyyy-MM-dd"));
            SetCol(r, "TotalAmount", 2000m);
            SetCol(r, "InstallmentAmount", 200m);
            SetCol(r, "Paid", 400m);
            SetCol(r, "RemainingAmount", 1600m);
            SetCol(r, "DueAmount", 200m);
            ds.DataTable1.Rows.Add(r);
            return ds;
        }

        private static dsFinancing BuildSampleFinancing()
        {
            var ds = new dsFinancing();
            string headerGuid = Guid.NewGuid().ToString();
            DataRow h = ds.Header.NewRow();
            SetCol(h, "Guid", headerGuid);
            SetCol(h, "VoucherDate", DateTime.Now.ToString("yyyy-MM-dd"));
            SetCol(h, "VoucherNumber", "FIN-001");
            SetCol(h, "BusinessPartnerName", "Sample Customer");
            SetCol(h, "TotalAmount", 10000m);
            SetCol(h, "DownPayment", 1000m);
            SetCol(h, "NetAmount", 9000m);
            SetCol(h, "LoanTypeAName", "Installment");
            SetCol(h, "GrantorName", "Sample Grantor");
            ds.Header.Rows.Add(h);

            DataRow d = ds.Details.NewRow();
            SetCol(d, "HeaderGuid", headerGuid);
            SetCol(d, "RowIndex", "1");
            SetCol(d, "Description", "Sample product");
            SetCol(d, "TotalAmount", 10000m);
            SetCol(d, "FinancingAmount", 9000m);
            SetCol(d, "InstallmentAmount", 750m);
            SetCol(d, "PeriodInMonths", 12);
            SetCol(d, "InterestRate", 5m);
            ds.Details.Rows.Add(d);
            return ds;
        }

        private static dsBusinessPartner BuildSampleBusinessPartner()
        {
            var ds = new dsBusinessPartner();

            DataRow bp = ds.BusinessPartner.NewRow();
            SetCol(bp, "ID", "1");
            SetCol(bp, "AName", "عميل تجريبي");
            SetCol(bp, "EName", "Sample Customer");
            SetCol(bp, "CommercialName", "Sample Customer Co.");
            SetCol(bp, "Address", "Sample Address");
            SetCol(bp, "Tel", "0790000000");
            SetCol(bp, "Email", "sample@example.com");
            SetCol(bp, "EmpCode", "E001");
            SetCol(bp, "StreetName", "Main St");
            SetCol(bp, "HouseNumber", "10");
            SetCol(bp, "NationalNumber", "1234567890");
            SetCol(bp, "Nationality", "JO");
            SetCol(bp, "IDNumber", "ID-001");
            ds.BusinessPartner.Rows.Add(bp);

            DataRow gr = ds.BusinessGrantor.NewRow();
            SetCol(gr, "ID", "2");
            SetCol(gr, "AName", "كفيل تجريبي");
            SetCol(gr, "EName", "Sample Grantor");
            SetCol(gr, "CommercialName", "Sample Grantor Co.");
            SetCol(gr, "Address", "Grantor Address");
            SetCol(gr, "Tel", "0791111111");
            SetCol(gr, "Email", "grantor@example.com");
            SetCol(gr, "EmpCode", "G001");
            SetCol(gr, "StreetName", "Second St");
            SetCol(gr, "HouseNumber", "20");
            SetCol(gr, "NationalNumber", "0987654321");
            SetCol(gr, "Nationality", "JO");
            SetCol(gr, "IDNumber", "ID-002");
            ds.BusinessGrantor.Rows.Add(gr);

            return ds;
        }

        private static System.Data.DataSet BuildSampleEmployeeContract()
        {
            var ds = new System.Data.DataSet("EmployeeContractData");
            var table = new DataTable("EmployeeContract");
            table.Columns.Add("ID", typeof(int));
            table.Columns.Add("ContractNumber", typeof(string));
            table.Columns.Add("EmployeeAName", typeof(string));
            table.Columns.Add("EmployeeEName", typeof(string));
            table.Columns.Add("EmployeeCode", typeof(string));
            table.Columns.Add("EmployeeNationalNumber", typeof(string));
            table.Columns.Add("EmployeeEmail", typeof(string));
            table.Columns.Add("EmployeeTel1", typeof(string));
            table.Columns.Add("ContractTypeAName", typeof(string));
            table.Columns.Add("ContractTypeEName", typeof(string));
            table.Columns.Add("JobTitleAName", typeof(string));
            table.Columns.Add("JobTitleEName", typeof(string));
            table.Columns.Add("DepartmentAName", typeof(string));
            table.Columns.Add("DepartmentEName", typeof(string));
            table.Columns.Add("BranchAName", typeof(string));
            table.Columns.Add("BranchEName", typeof(string));
            table.Columns.Add("StartDateDisplay", typeof(string));
            table.Columns.Add("EndDateDisplay", typeof(string));
            table.Columns.Add("IsOpenEndedDisplay", typeof(string));
            table.Columns.Add("ProbationMonthsDisplay", typeof(string));
            table.Columns.Add("WorkingHoursDisplay", typeof(string));
            table.Columns.Add("BasicSalaryDisplay", typeof(string));
            table.Columns.Add("Notes", typeof(string));
            table.Columns.Add("IsActiveDisplay", typeof(string));
            table.Columns.Add("AnnualLeaveDisplay", typeof(string));
            table.Columns.Add("SickLeaveDisplay", typeof(string));
            table.Columns.Add("SalaryElementsAgreementDisplay", typeof(string));
            table.Rows.Add(
                1,
                "CTR-001",
                "موظف تجريبي",
                "Sample Employee",
                "E100",
                "1234567890",
                "sample@example.com",
                "0790000000",
                "عقد دائم",
                "Permanent",
                "محاسب",
                "Accountant",
                "المالية",
                "Finance",
                "الفرع الرئيسي",
                "Main Branch",
                DateTime.Now.AddYears(-1).ToString("yyyy-MM-dd"),
                DateTime.Now.AddYears(1).ToString("yyyy-MM-dd"),
                "No / لا",
                "3",
                "40",
                "800.000",
                "Sample preview contract",
                "Active / ساري",
                "14 days / 14 يوماً",
                "14 days / 14 يوماً",
                "As agreed / حسب الاتفاق");
            ds.Tables.Add(table);
            return ds;
        }
    }
}
