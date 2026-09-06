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

            System.Data.DataSet preferred = BuildSampleDataSet(
                config.PageName, config.FastReportFileName);

            Report report = new Report();

            // Always load first so we can read every TableDataSource schema and fill
            // dummy rows for preview — including layouts without a hand-written sample.
            LoadFastReportTemplate(report, config, companyId);

            // Disable designer DB/XML connections without removing their child tables
            // (removing them breaks DataBand references).
            DisableDesignerConnections(report);

            System.Data.DataSet sampleData = MergePreferredWithSchemaDummies(report, preferred);
            ForceBindTables(report, sampleData);
            EnableRegisteredDataSources(report);
            ApplySampleParameters(report, config.PageName, userId, companyId);
            FillUnsetParametersWithSamples(report);

            try
            {
                return ExportReportToPdf(report);
            }
            catch (Exception firstEx)
            {
                try
                {
                    DisableDesignerConnections(report);
                    ForceBindTables(report, sampleData);
                    EnableRegisteredDataSources(report);
                    ApplySampleParameters(report, config.PageName, userId, companyId);
                    FillUnsetParametersWithSamples(report);
                    return ExportReportToPdf(report);
                }
                catch (Exception)
                {
                    throw firstEx;
                }
            }
        }

        /// <summary>
        /// Ensures every TableDataSource declared in the loaded .frx has at least one
        /// dummy table. Curated samples win when present; otherwise rows are generated
        /// from the datasource column schema.
        /// </summary>
        private static System.Data.DataSet MergePreferredWithSchemaDummies(
            Report report, System.Data.DataSet preferred)
        {
            var result = new System.Data.DataSet();

            if (preferred != null)
            {
                foreach (DataTable table in preferred.Tables)
                {
                    if (table == null || string.IsNullOrWhiteSpace(table.TableName))
                        continue;
                    if (result.Tables.Contains(table.TableName))
                        continue;
                    result.Tables.Add(table.Copy());
                }
            }

            try
            {
                for (int i = 0; i < report.Dictionary.DataSources.Count; i++)
                {
                    object src = report.Dictionary.DataSources[i];
                    if (src == null)
                        continue;

                    string name = Convert.ToString(
                        src.GetType().GetProperty("Name")?.GetValue(src) ?? "")?.Trim() ?? "";
                    if (string.IsNullOrWhiteSpace(name))
                        continue;
                    if (result.Tables.Contains(name))
                        continue;

                    DataTable dummy = BuildDummyTableFromDataSource(src, name);
                    if (dummy != null && !result.Tables.Contains(dummy.TableName))
                        result.Tables.Add(dummy);
                }
            }
            catch
            {
                // Schema reflection is best-effort.
            }

            if (result.Tables.Count == 0 && preferred != null)
                return preferred;

            return result;
        }

        private static DataTable BuildDummyTableFromDataSource(object src, string tableName)
        {
            var table = new DataTable(tableName);

            try
            {
                object columnsObj = src.GetType().GetProperty("Columns")?.GetValue(src);
                if (columnsObj is System.Collections.IEnumerable columns)
                {
                    foreach (object col in columns)
                    {
                        if (col == null)
                            continue;
                        string colName = Convert.ToString(
                            col.GetType().GetProperty("Name")?.GetValue(col) ?? "")?.Trim() ?? "";
                        if (string.IsNullOrWhiteSpace(colName) || table.Columns.Contains(colName))
                            continue;

                        Type colType = col.GetType().GetProperty("DataType")?.GetValue(col) as Type
                                       ?? typeof(string);
                        if (colType == typeof(object) || colType == typeof(byte[]))
                            colType = typeof(string);

                        table.Columns.Add(colName, Nullable.GetUnderlyingType(colType) ?? colType);
                    }
                }
            }
            catch
            {
                // Fall through to default columns.
            }

            if (table.Columns.Count == 0)
            {
                table.Columns.Add("RowIndex", typeof(string));
                table.Columns.Add("Description", typeof(string));
                table.Columns.Add("Amount", typeof(decimal));
                table.Columns.Add("Total", typeof(decimal));
            }

            for (int r = 1; r <= 2; r++)
            {
                DataRow row = table.NewRow();
                foreach (DataColumn c in table.Columns)
                    row[c.ColumnName] = DummyValueFor(c.DataType, c.ColumnName, r);
                table.Rows.Add(row);
            }

            return table;
        }

        private static object DummyValueFor(Type type, string columnName, int rowIndex)
        {
            string col = (columnName ?? "").ToLowerInvariant();
            type = Nullable.GetUnderlyingType(type) ?? type;

            if (type == typeof(string) || type == typeof(char))
            {
                if (col.Contains("guid"))
                    return Guid.NewGuid().ToString();
                if (col.Contains("date") || col.Contains("time"))
                    return DateTime.Now.AddDays(1 - rowIndex).ToString("yyyy-MM-dd");
                if (col.Contains("name") || col.Contains("customer") || col.Contains("partner"))
                    return rowIndex == 1 ? "Sample Customer" : "Sample Item";
                if (col.Contains("branch"))
                    return "Sample Branch";
                if (col.Contains("cashier") || col.Contains("user") || col.Contains("employee"))
                    return "Sample Cashier";
                if (col.Contains("account"))
                    return "1000 - Cash";
                if (col.Contains("note") || col.Contains("desc") || col.Contains("detail"))
                    return "Sample preview row " + rowIndex;
                if (col.Contains("payment"))
                    return rowIndex == 1 ? "Cash" : "Card";
                if (col.Contains("type") || col.Contains("status") || col.Contains("event"))
                    return "Sample";
                if (col.Contains("code") || col.Contains("number") || col.Contains("ref"))
                    return "S-" + (1000 + rowIndex);
                if (col.Contains("label") || col.Contains("hour"))
                    return rowIndex == 1 ? "10:00" : "14:00";
                return "Sample " + rowIndex;
            }

            if (type == typeof(bool))
                return rowIndex == 1;

            if (type == typeof(DateTime))
                return DateTime.Now.AddDays(1 - rowIndex);

            if (type == typeof(int) || type == typeof(short) || type == typeof(long) ||
                type == typeof(byte))
            {
                if (col.Contains("count") || col.Contains("qty") || col.Contains("hour"))
                    return 10 * rowIndex;
                if (col.Contains("id") || col.Contains("index") || col.Contains("row"))
                    return rowIndex;
                if (col.Contains("status"))
                    return 1;
                return rowIndex;
            }

            if (type == typeof(decimal) || type == typeof(double) || type == typeof(float))
            {
                if (col.Contains("tax"))
                    return 16m * rowIndex;
                if (col.Contains("discount"))
                    return 5m * rowIndex;
                if (col.Contains("qty") || col.Contains("quantity"))
                    return (decimal)rowIndex;
                if (col.Contains("price") || col.Contains("rate"))
                    return 50m * rowIndex;
                if (col.Contains("credit"))
                    return rowIndex == 2 ? 100m : 0m;
                if (col.Contains("debit"))
                    return rowIndex == 1 ? 100m : 0m;
                return 100m * rowIndex;
            }

            if (type == typeof(Guid))
                return Guid.NewGuid();

            try
            {
                return Convert.ChangeType(rowIndex, type);
            }
            catch
            {
                return DBNull.Value;
            }
        }

        /// <summary>
        /// Fills any unset report dictionary parameters with preview placeholders.
        /// </summary>
        private static void FillUnsetParametersWithSamples(Report report)
        {
            try
            {
                FillParameterNode(report, report.Dictionary.Parameters, "");
            }
            catch
            {
                // Best-effort.
            }
        }

        private static void FillParameterNode(
            Report report, System.Collections.IEnumerable parameters, string prefix)
        {
            if (parameters == null)
                return;

            foreach (object p in parameters)
            {
                if (p == null)
                    continue;

                Type pt = p.GetType();
                string name = Convert.ToString(pt.GetProperty("Name")?.GetValue(p) ?? "")?.Trim() ?? "";
                if (string.IsNullOrWhiteSpace(name))
                    continue;

                string fullName = string.IsNullOrWhiteSpace(prefix) ? name : prefix + "." + name;

                object nested = pt.GetProperty("Parameters")?.GetValue(p);
                if (nested is System.Collections.IEnumerable nestedList)
                {
                    bool hasChild = false;
                    foreach (object _ in nestedList)
                    {
                        hasChild = true;
                        break;
                    }
                    if (hasChild)
                    {
                        FillParameterNode(report, nestedList, fullName);
                        continue;
                    }
                }

                object current = null;
                try
                {
                    current = report.GetParameterValue(fullName);
                }
                catch
                {
                    current = pt.GetProperty("Value")?.GetValue(p);
                }

                if (current != null &&
                    !(current is string s && string.IsNullOrWhiteSpace(s)) &&
                    current != DBNull.Value)
                {
                    continue;
                }

                Type dataType = pt.GetProperty("DataType")?.GetValue(p) as Type ?? typeof(string);
                TrySetParameter(report, fullName, DummyValueFor(dataType, name, 1));
            }
        }

        private static bool IsPosStyleFrx(string frxFileName)
        {
            string frx = (frxFileName ?? "").Trim();
            return string.Equals(frx, "rptCashReportPOS", StringComparison.OrdinalIgnoreCase)
                || frx.StartsWith("rptPOS", StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Binds sample/live tables onto the TableDataSource names already present in the .frx.
        /// POS templates ship with bare TableDataSource nodes (no connection / ReferenceName),
        /// so RegisterData alone is not enough — set Table + ReferenceName on the existing source.
        /// </summary>
        private static void ForceBindTables(Report report, System.Data.DataSet ds)
        {
            if (ds == null)
                return;

            // Register first so FastReport knows the business objects.
            var anon = new System.Data.DataSet();
            foreach (DataTable table in ds.Tables)
            {
                if (table == null || string.IsNullOrWhiteSpace(table.TableName))
                    continue;
                if (anon.Tables.Contains(table.TableName))
                    continue;
                DataTable copy = table.Copy();
                copy.TableName = table.TableName;
                anon.Tables.Add(copy);
            }

            try
            {
                report.RegisterData(anon);
            }
            catch
            {
                // Ignore duplicate registration.
            }

            foreach (DataTable table in anon.Tables)
            {
                try
                {
                    report.RegisterData(table, table.TableName);
                }
                catch { }

                object src = null;
                try
                {
                    src = report.GetDataSource(table.TableName);
                }
                catch { }

                if (src == null)
                    continue;

                try
                {
                    var enabledProp = src.GetType().GetProperty("Enabled");
                    enabledProp?.SetValue(src, true);

                    var refProp = src.GetType().GetProperty("ReferenceName");
                    if (refProp != null && refProp.CanWrite)
                        refProp.SetValue(src, table.TableName);

                    var tableProp = src.GetType().GetProperty("Table");
                    if (tableProp != null && tableProp.CanWrite)
                        tableProp.SetValue(src, table);

                    var aliasProp = src.GetType().GetProperty("Alias");
                    if (aliasProp != null && aliasProp.CanWrite &&
                        string.IsNullOrWhiteSpace(Convert.ToString(aliasProp.GetValue(src))))
                        aliasProp.SetValue(src, table.TableName);
                }
                catch
                {
                    // Best-effort binding for FastReport version differences.
                }
            }
        }

        private static bool NeedsConnectionClear(string frxFileName)
        {
            // Only receipt invoice keeps a designer XmlDataConnection that breaks Prepare.
            // Bare TableDataSource POS layouts must keep Register-before-Load (no post-clear).
            string frx = (frxFileName ?? "").Trim();
            return string.Equals(frx, "rptInvoicePOS", StringComparison.OrdinalIgnoreCase);
        }

        private static void ClearDesignerConnections(Report report)
        {
            try
            {
                for (int i = report.Dictionary.Connections.Count - 1; i >= 0; i--)
                    report.Dictionary.Connections.RemoveAt(i);
            }
            catch
            {
                // Older FastReport builds may not expose Connections the same way.
            }
        }

        /// <summary>
        /// Preview-only: keep TableDataSource nodes, but stop FastReport from opening the
        /// designer Xml/MsSql connection (which causes ConnectionString / NRE failures).
        /// </summary>
        private static void DisableDesignerConnections(Report report)
        {
            try
            {
                for (int i = 0; i < report.Dictionary.Connections.Count; i++)
                {
                    object conn = report.Dictionary.Connections[i];
                    if (conn == null)
                        continue;
                    try
                    {
                        conn.GetType().GetProperty("Enabled")?.SetValue(conn, false);
                    }
                    catch { }
                    try
                    {
                        var cs = conn.GetType().GetProperty("ConnectionString");
                        if (cs != null && cs.CanWrite)
                            cs.SetValue(conn, "");
                    }
                    catch { }
                }
            }
            catch
            {
                // Best-effort.
            }
        }

        /// <summary>
        /// Registers sample tables the same way live POS print does:
        /// put copies into an anonymous DataSet and RegisterData(ds).
        /// </summary>
        private static void RegisterDataSetTables(Report report, System.Data.DataSet ds)
        {
            if (ds == null)
                return;

            var anon = new System.Data.DataSet();
            foreach (DataTable table in ds.Tables)
            {
                if (table == null || string.IsNullOrWhiteSpace(table.TableName))
                    continue;
                if (anon.Tables.Contains(table.TableName))
                    continue;
                DataTable copy = table.Copy();
                copy.TableName = table.TableName;
                anon.Tables.Add(copy);
            }

            if (anon.Tables.Count == 0)
                return;

            try
            {
                report.RegisterData(anon);
            }
            catch
            {
                // Ignore duplicate registration.
            }

            foreach (DataTable table in anon.Tables)
            {
                try
                {
                    report.RegisterData(table, table.TableName);
                }
                catch
                {
                    // Ignore duplicate registration.
                }

                try
                {
                    var src = report.GetDataSource(table.TableName);
                    if (src != null)
                        src.Enabled = true;
                }
                catch
                {
                    // Source appears after Load for some templates.
                }
            }
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

        private System.Data.DataSet BuildSampleDataSet(string pageName, string frxFileName)
        {
            string printPage = clsTransactionReportDefaults.ResolvePrintPageName(pageName);
            string frx = (frxFileName ?? "").Trim();

            if (printPage == PageJournalVoucherAdd ||
                string.Equals(frx, "rptJV", StringComparison.OrdinalIgnoreCase))
                return BuildSampleJvDetails();

            if (printPage == PageInvoicePageAdd ||
                string.Equals(frx, "rptInvoice", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(frx, "rptInvoicePOS", StringComparison.OrdinalIgnoreCase))
                return BuildSampleInvoiceDetails();

            if (printPage == PageCashVoucherAdd ||
                printPage == PageCreditNotePageAdd ||
                string.Equals(frx, "rptCashVoucher", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(frx, "rptCheques", StringComparison.OrdinalIgnoreCase))
                return BuildSampleCashVoucher();

            if (string.Equals(frx, "rptTrialBalance", StringComparison.OrdinalIgnoreCase) ||
                pageName == clsTransactionReportDefaults.PageTrialBalance)
                return BuildSampleTrialBalance();

            if (string.Equals(frx, "rptBalanceSheet", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(frx, "rptIncomeStatement", StringComparison.OrdinalIgnoreCase) ||
                pageName == clsTransactionReportDefaults.PageBalanceSheet ||
                pageName == clsTransactionReportDefaults.PageIncomeStatement)
                return BuildSampleIncomeStatement();

            if (string.Equals(frx, "rptAging", StringComparison.OrdinalIgnoreCase) ||
                pageName == clsTransactionReportDefaults.PageAging)
                return BuildSampleAging();

            if (string.Equals(frx, "rptBusinessPartnerReports", StringComparison.OrdinalIgnoreCase) ||
                pageName == clsTransactionReportDefaults.PageBusinessPartnerBalances)
                return BuildSampleBusinessPartnerBalances();

            if (string.Equals(frx, "rptCashReport", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(frx, "rptCashReportPOS", StringComparison.OrdinalIgnoreCase) ||
                pageName == clsTransactionReportDefaults.PageCashReport)
                return BuildSampleCashReport();

            if (string.Equals(frx, "rptFinancingReport", StringComparison.OrdinalIgnoreCase) ||
                pageName == clsTransactionReportDefaults.PageFinancingReport)
                return BuildSampleFinancingReport();

            if (string.Equals(frx, "rptCutomerLoansReport", StringComparison.OrdinalIgnoreCase) ||
                pageName == clsTransactionReportDefaults.PageCustomerLoans ||
                pageName == clsTransactionReportDefaults.PageEmployeeLoans)
                return BuildSampleEmployeeLoans();

            if (string.Equals(frx, "rptPaymentInstallmentTree", StringComparison.OrdinalIgnoreCase) ||
                pageName == clsTransactionReportDefaults.PagePaymentInstallmentTree)
                return BuildSamplePaymentInstallmentTree();

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
                // Financing layouts use multiple datasets; combine into one for registration.
                var combined = new System.Data.DataSet();
                void merge(System.Data.DataSet src)
                {
                    foreach (DataTable t in src.Tables)
                        if (combined.Tables.Contains(t.TableName) == false)
                            combined.Tables.Add(t.Copy());
                }
                merge(BuildSampleFinancing());
                merge(BuildSampleBusinessPartner());
                merge(BuildSampleJvDetails());
                return combined;
            }

            if (string.Equals(frx, "rptEmployeeContract", StringComparison.OrdinalIgnoreCase) ||
                pageName == clsTransactionReportDefaults.PageEmployeeContractAdd)
                return BuildSampleEmployeeContract();

            if (string.Equals(frx, "rptPOSXZ", StringComparison.OrdinalIgnoreCase) ||
                pageName == clsTransactionReportDefaults.PagePOSXZ)
                return BuildSamplePosXZ();

            if (string.Equals(frx, "rptPOSSalesByCashier", StringComparison.OrdinalIgnoreCase) ||
                pageName == clsTransactionReportDefaults.PagePOSSalesByCashier)
                return BuildSamplePosSalesByCashier();

            if (string.Equals(frx, "rptPOSSalesByHour", StringComparison.OrdinalIgnoreCase) ||
                pageName == clsTransactionReportDefaults.PagePOSSalesByHour)
                return BuildSamplePosSalesByHour();

            if (string.Equals(frx, "rptPOSSalesByCategory", StringComparison.OrdinalIgnoreCase) ||
                pageName == clsTransactionReportDefaults.PagePOSSalesByCategory)
                return BuildSamplePosSalesByCategory();

            if (string.Equals(frx, "rptPOSAudit", StringComparison.OrdinalIgnoreCase) ||
                pageName == clsTransactionReportDefaults.PagePOSAudit)
                return BuildSamplePosAudit();

            return BuildSampleAccountStatement();
        }

        // Kept for callers that still expect the old name; routes to BuildSampleDataSet registration.
        private void RegisterSampleData(Report report, string pageName, string frxFileName)
        {
            RegisterDataSetTables(report, BuildSampleDataSet(pageName, frxFileName));
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
            TrySetParameter(report, "report.Cashier", "Sample Cashier");
            TrySetParameter(report, "report.ReportType", "X");
            TrySetParameter(report, "report.Scope", "Day");
            TrySetParameter(report, "report.Title", "POS Operations Report (Sample)");
            TrySetParameter(report, "report.ContractNumber", "CTR-001");
            TrySetParameter(report, "report.ContractID", "1");
            TrySetParameter(report, "report.InvoiceDate", today);
            TrySetParameter(report, "report.InvoiceNumber", "POS-1001");
            TrySetParameter(report, "report.InvoiceNumberRef", "REF-1001");
            TrySetParameter(report, "report.QRText", "SAMPLE-QR");
            TrySetParameter(report, "report.JournalVoucherTypes", "POS Sales");
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
            SetCol(r, "EMPCode", "E001");
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
            SetCol(r, "CreationUser", "Sample Cashier");
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

        private static System.Data.DataSet BuildSamplePaymentInstallmentTree()
        {
            var ds = new System.Data.DataSet();
            var t = new System.Data.DataTable("DataTable1");
            t.Columns.Add("Index", typeof(string));
            t.Columns.Add("CustomerName", typeof(string));
            t.Columns.Add("EmpCode", typeof(string));
            t.Columns.Add("PaymentVoucherDate", typeof(string));
            t.Columns.Add("PaymentJVTypeName", typeof(string));
            t.Columns.Add("PaymentJVNumber", typeof(string));
            t.Columns.Add("PaymentAmount", typeof(decimal));
            t.Columns.Add("InstallmentDueDate", typeof(string));
            t.Columns.Add("InstallmentNote", typeof(string));
            t.Columns.Add("InstallmentLineTotal", typeof(decimal));
            t.Columns.Add("ReconciledAmount", typeof(decimal));
            t.Columns.Add("ReconciliationVoucherNumber", typeof(string));
            t.Columns.Add("FinancingVoucherNumber", typeof(string));
            t.Columns.Add("LoanTypeName", typeof(string));
            t.Columns.Add("Status", typeof(string));
            t.Rows.Add(
                "1",
                "Sample Customer",
                "C100",
                DateTime.Now.ToString("yyyy-MM-dd"),
                "Receivable",
                "1205",
                250m,
                DateTime.Now.ToString("yyyy-MM-dd"),
                "Installment 1",
                500m,
                250m,
                "88",
                "1169",
                "Installment Sales",
                "Linked");
            ds.Tables.Add(t);
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

        private static System.Data.DataSet BuildSamplePosXZ()
        {
            var ds = new System.Data.DataSet();
            var summary = new DataTable("Summary");
            summary.Columns.Add("SalesCount", typeof(int));
            summary.Columns.Add("RefundCount", typeof(int));
            summary.Columns.Add("SalesTotal", typeof(decimal));
            summary.Columns.Add("RefundTotal", typeof(decimal));
            summary.Columns.Add("NetSales", typeof(decimal));
            summary.Columns.Add("TotalTax", typeof(decimal));
            summary.Columns.Add("TotalDiscount", typeof(decimal));
            summary.Columns.Add("HeaderDiscount", typeof(decimal));
            summary.Columns.Add("CashNet", typeof(decimal));
            summary.Columns.Add("BankNet", typeof(decimal));
            summary.Columns.Add("DebitNet", typeof(decimal));
            summary.Columns.Add("OtherNet", typeof(decimal));
            summary.Columns.Add("ReportType", typeof(string));
            summary.Columns.Add("Scope", typeof(string));
            summary.Columns.Add("OpeningFloat", typeof(decimal));
            summary.Columns.Add("CountedCash", typeof(decimal));
            summary.Columns.Add("ExpectedCash", typeof(decimal));
            summary.Columns.Add("ExpectedCashSaved", typeof(decimal));
            summary.Columns.Add("Variance", typeof(decimal));
            summary.Columns.Add("VarianceSaved", typeof(decimal));
            summary.Columns.Add("ClosingNote", typeof(string));
            summary.Columns.Add("Status", typeof(int));
            summary.Rows.Add(
                25, 2, 1250m, -80m, 1170m, 160m, 45m, 10m,
                700m, 300m, 170m, 0m, "X", "Day",
                100m, 790m, 800m, 800m, -10m, -10m, "Sample preview", 1);
            ds.Tables.Add(summary);

            var payments = new DataTable("Payments");
            payments.Columns.Add("PaymentMethodID", typeof(int));
            payments.Columns.Add("PaymentMethod", typeof(string));
            payments.Columns.Add("IsCash", typeof(bool));
            payments.Columns.Add("IsBank", typeof(bool));
            payments.Columns.Add("IsDebit", typeof(bool));
            payments.Columns.Add("InvoiceCount", typeof(int));
            payments.Columns.Add("NetTotal", typeof(decimal));
            payments.Rows.Add(1, "Cash", true, false, false, 15, 700m);
            payments.Rows.Add(2, "Card", false, true, false, 10, 470m);
            ds.Tables.Add(payments);
            return ds;
        }

        private static System.Data.DataSet BuildSamplePosSalesByCashier()
        {
            var ds = new System.Data.DataSet();
            var table = new DataTable("SalesByCashier");
            table.Columns.Add("CashierID", typeof(int));
            table.Columns.Add("CashierName", typeof(string));
            table.Columns.Add("SalesCount", typeof(int));
            table.Columns.Add("RefundCount", typeof(int));
            table.Columns.Add("SalesTotal", typeof(decimal));
            table.Columns.Add("RefundTotal", typeof(decimal));
            table.Columns.Add("NetSales", typeof(decimal));
            table.Columns.Add("TotalDiscount", typeof(decimal));
            table.Columns.Add("TotalTax", typeof(decimal));
            table.Rows.Add(1, "Sample Cashier", 20, 1, 1000m, -40m, 960m, 20m, 130m);
            table.Rows.Add(2, "Second Cashier", 10, 0, 500m, 0m, 500m, 5m, 70m);
            ds.Tables.Add(table);
            return ds;
        }

        private static System.Data.DataSet BuildSamplePosSalesByHour()
        {
            var ds = new System.Data.DataSet();
            var table = new DataTable("SalesByHour");
            table.Columns.Add("SaleHour", typeof(int));
            table.Columns.Add("HourLabel", typeof(string));
            table.Columns.Add("SalesCount", typeof(int));
            table.Columns.Add("RefundCount", typeof(int));
            table.Columns.Add("NetSales", typeof(decimal));
            table.Columns.Add("TotalDiscount", typeof(decimal));
            table.Rows.Add(10, "10:00", 8, 0, 320m, 5m);
            table.Rows.Add(14, "14:00", 12, 1, 480m, 10m);
            ds.Tables.Add(table);
            return ds;
        }

        private static System.Data.DataSet BuildSamplePosSalesByCategory()
        {
            var ds = new System.Data.DataSet();
            var table = new DataTable("SalesByCategory");
            table.Columns.Add("CategoryID", typeof(int));
            table.Columns.Add("CategoryName", typeof(string));
            table.Columns.Add("InvoiceCount", typeof(int));
            table.Columns.Add("QtySold", typeof(decimal));
            table.Columns.Add("NetSales", typeof(decimal));
            table.Columns.Add("TotalDiscount", typeof(decimal));
            table.Rows.Add(1, "Grocery", 15, 40m, 600m, 15m);
            table.Rows.Add(2, "Drinks", 8, 20m, 250m, 5m);
            ds.Tables.Add(table);
            return ds;
        }

        private static System.Data.DataSet BuildSamplePosAudit()
        {
            var ds = new System.Data.DataSet();
            var table = new DataTable("AuditReport");
            table.Columns.Add("EventType", typeof(string));
            table.Columns.Add("EventDate", typeof(DateTime));
            table.Columns.Add("Reference", typeof(string));
            table.Columns.Add("CashierName", typeof(string));
            table.Columns.Add("CashierID", typeof(int));
            table.Columns.Add("PaymentMethod", typeof(string));
            table.Columns.Add("Amount", typeof(decimal));
            table.Columns.Add("DiscountAmount", typeof(decimal));
            table.Columns.Add("Details", typeof(string));
            table.Rows.Add("Refund", DateTime.Now.AddHours(-2), "INV-100", "Sample Cashier", 1, "Cash", -40m, 0m, "Sample refund");
            table.Rows.Add("Discount", DateTime.Now.AddHours(-1), "INV-101", "Sample Cashier", 1, "Card", 80m, 10m, "Sample discount");
            table.Rows.Add("VoidCart", DateTime.Now.AddMinutes(-20), "", "Sample Cashier", 1, "", 0m, 0m, "Cleared cart");
            ds.Tables.Add(table);
            return ds;
        }
    }
}
