using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Data;

namespace WebApplication2.cls
{
    /// <summary>
    /// Dashboard widget SQL and default business KPI catalog (templates: UserId = -1).
    /// Each company uses its own database — account-setting subqueries do not need @CompanyId.
    /// </summary>
    public static class clsDashboardWidgets
    {
        private const string RevenueAccounts = @"
(
    SELECT TOP 1 AccountID FROM tbl_AccountSetting
    WHERE AccountRefID = 2 AND AccountID > 0 AND Active = 1
    ORDER BY CreationDate DESC
    UNION ALL
    SELECT TOP 1 AccountID FROM tbl_AccountSetting
    WHERE AccountRefID = 3 AND AccountID > 0 AND Active = 1
    ORDER BY CreationDate DESC
)";

        private const string CogsAccount = @"
(
    SELECT TOP 1 AccountID FROM tbl_AccountSetting
    WHERE AccountRefID = 19 AND AccountID > 0 AND Active = 1
    ORDER BY CreationDate DESC
)";

        /// <summary>Operating expenses only — excludes COGS (ref 19) to avoid double-counting in net profit.</summary>
        private const string OperatingExpenseAccounts = @"
(
    SELECT AccountID FROM tbl_AccountSetting
    WHERE AccountRefID IN (1) AND AccountID > 0 AND Active = 1
)";

        private const string SalesInvoiceTypeFilter = "InvoiceTypeID IN (3, 10)";
        private const string PurchaseInvoiceTypeFilter = "InvoiceTypeID IN (2, 22)";
        private const string InvoiceCountedFilter = "ISNULL(IsCounted, 1) = 1";

        private const string CashAndBankAccounts = @"
(
    SELECT AccountID FROM tbl_AccountSetting
    WHERE AccountRefID IN (5, 15) AND AccountID > 0 AND Active = 1
)";

        private const string ReceivableAccount = @"
(
    SELECT TOP 1 AccountID FROM tbl_AccountSetting
    WHERE AccountRefID = 7 AND AccountID > 0 AND Active = 1
    ORDER BY CreationDate DESC
)";

        private const string PayableAccount = @"
(
    SELECT TOP 1 AccountID FROM tbl_AccountSetting
    WHERE AccountRefID = 6 AND AccountID > 0 AND Active = 1
    ORDER BY CreationDate DESC
)";

        private const string InventoryAccount = @"
(
    SELECT TOP 1 AccountID FROM tbl_AccountSetting
    WHERE AccountRefID = 8 AND AccountID > 0 AND Active = 1
    ORDER BY CreationDate DESC
)";

        public sealed class DefaultDashboardWidgetDefinition
        {
            public string Title { get; init; } = "";
            public string WidgetType { get; init; } = "KPI";
            public string GroupName { get; init; } = "Finance";
            public string Sql { get; init; } = "";
            public string Icon { get; init; } = "0xf155";
            public string Color { get; init; } = "#3498db";
            public string SectionName { get; init; } = "leftWidgets";
            public int SectionIndex { get; init; } = 100;
            public bool IsActive { get; init; } = true;
        }

        /// <summary>
        /// Prefer the latest template SQL (UserId = -1) so KPI trend fixes reach user copies.
        /// </summary>
        public static string ResolveWidgetSql(clsSQL sql, string conn, string title, string fallbackSql)
        {
            if (string.IsNullOrWhiteSpace(title))
                return fallbackSql;

            try
            {
                DataTable template = sql.ExecuteQueryStatement(
                    @"SELECT TOP 1 SQLQuery
                      FROM tbl_DashboardWidgets
                      WHERE UserId = -1 AND RTRIM(Title) = RTRIM(@Title)
                      ORDER BY ModificationDate DESC",
                    conn,
                    new[]
                    {
                        new SqlParameter("@Title", SqlDbType.NVarChar, 200) { Value = title.Trim() },
                    });

                if (template != null &&
                    template.Rows.Count > 0 &&
                    template.Rows[0]["SQLQuery"] != DBNull.Value)
                {
                    string templateSql = template.Rows[0]["SQLQuery"].ToString();
                    if (!string.IsNullOrWhiteSpace(templateSql))
                        return templateSql;
                }
            }
            catch
            {
                // Fall back to the user/widget copy SQL.
            }

            string catalogSql = TryResolveCatalogSql(title);
            if (!string.IsNullOrWhiteSpace(catalogSql))
                return catalogSql;

            return fallbackSql;
        }

        public static string TryResolveCatalogSql(string title)
        {
            if (string.IsNullOrWhiteSpace(title))
                return null;

            string trimmed = title.Trim();
            foreach (DefaultDashboardWidgetDefinition widget in GetBusinessDefaultCatalog())
            {
                if (string.Equals(widget.Title.Trim(), trimmed, StringComparison.OrdinalIgnoreCase))
                    return widget.Sql;
            }

            foreach ((string Title, string Sql) fix in GetSqlFixes())
            {
                if (string.Equals(fix.Title.Trim(), trimmed, StringComparison.OrdinalIgnoreCase))
                    return fix.Sql;
            }

            return null;
        }

        public static double? ExtractTrendValue(DataTable widgetData)
        {
            if (widgetData == null || widgetData.Rows.Count != 1)
                return null;

            DataRow row = widgetData.Rows[0];
            if (!widgetData.Columns.Contains("PercentageChange") || row["PercentageChange"] == DBNull.Value)
                return null;

            return Convert.ToDouble(row["PercentageChange"]);
        }

        public static List<Dictionary<string, object>> ToRowDictionaries(DataTable table)
        {
            var rows = new List<Dictionary<string, object>>();
            if (table == null)
                return rows;

            foreach (DataRow dataRow in table.Rows)
            {
                var dict = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);
                foreach (DataColumn column in table.Columns)
                {
                    dict[column.ColumnName] = dataRow[column] == DBNull.Value ? null : dataRow[column];
                }
                rows.Add(dict);
            }

            return rows;
        }

        /// <summary>
        /// Ensures KPI rows expose a numeric PercentageChange for the Flutter trend badge.
        /// </summary>
        public static void EnsureKpiTrendColumn(DataTable widgetData)
        {
            if (widgetData == null || widgetData.Rows.Count != 1 || !widgetData.Columns.Contains("Total"))
                return;

            DataRow dataRow = widgetData.Rows[0];

            if (widgetData.Columns.Contains("PercentageChange") &&
                dataRow["PercentageChange"] != DBNull.Value)
            {
                return;
            }

            foreach (string previousColumn in new[] { "PreviousTotal", "PreviousMonthTotal", "PreviousValue" })
            {
                if (!widgetData.Columns.Contains(previousColumn) || dataRow[previousColumn] == DBNull.Value)
                    continue;

                double previous = Convert.ToDouble(dataRow[previousColumn]);
                double current = Convert.ToDouble(dataRow["Total"]);
                SetPercentageChange(widgetData, dataRow, ComputePercentChange(current, previous));
                return;
            }

            if (widgetData.Columns.Contains("PercentageChange"))
            {
                dataRow["PercentageChange"] = 0m;
            }
        }

        private static void SetPercentageChange(DataTable widgetData, DataRow dataRow, decimal value)
        {
            if (!widgetData.Columns.Contains("PercentageChange"))
                widgetData.Columns.Add("PercentageChange", typeof(decimal));

            dataRow["PercentageChange"] = value;
        }

        private static decimal ComputePercentChange(double current, double previous)
        {
            if (previous == 0)
                return current == 0 ? 0 : 100;

            return (decimal)((current - previous) * 100.0 / previous);
        }

        /// <summary>Updates SQL on all template/user rows that share a title.</summary>
        public static void ApplyDashboardWidgetSqlFixes(int companyId)
        {
            clsSQL sql = new clsSQL();
            string conn = sql.CreateDataBaseConnectionString(companyId);

            foreach (var fix in GetSqlFixes())
            {
                sql.ExecuteNonQueryStatement(
                    "UPDATE tbl_DashboardWidgets SET SQLQuery = @Sql, ModificationDate = GETDATE() WHERE RTRIM(Title) = RTRIM(@Title)",
                    conn,
                    new SqlParameter[]
                    {
                        new SqlParameter("@Title", SqlDbType.NVarChar, 200) { Value = fix.Title },
                        new SqlParameter("@Sql", SqlDbType.NVarChar, -1) { Value = fix.Sql },
                    });
            }
        }

        public static void ApplyDashboardWidgetTypeFixes(int companyId)
        {
            clsSQL sql = new clsSQL();
            string conn = sql.CreateDataBaseConnectionString(companyId);

            foreach (var fix in GetTypeFixes())
            {
                sql.ExecuteNonQueryStatement(
                    "UPDATE tbl_DashboardWidgets SET WidgetType = @WidgetType, ModificationDate = GETDATE() WHERE Title = @Title",
                    conn,
                    new SqlParameter[]
                    {
                        new SqlParameter("@Title", SqlDbType.NVarChar, 200) { Value = fix.Title },
                        new SqlParameter("@WidgetType", SqlDbType.NVarChar, 50) { Value = fix.WidgetType },
                    });
            }
        }

        /// <summary>
        /// Seeds / refreshes business-focused dashboard templates and deactivates technical-only widgets.
        /// </summary>
        public static void ApplyBusinessDashboardDefaults(int companyId)
        {
            ApplyDashboardWidgetSqlFixes(companyId);
            ApplyDashboardWidgetTypeFixes(companyId);

            clsSQL sql = new clsSQL();
            string conn = sql.CreateDataBaseConnectionString(companyId);

            foreach (var widget in GetBusinessDefaultCatalog())
            {
                UpsertTemplateWidget(sql, conn, companyId, widget);
            }

            foreach (var title in GetTechnicalOnlyTitles())
            {
                sql.ExecuteNonQueryStatement(
                    "UPDATE tbl_DashboardWidgets SET IsActive = 0, ModificationDate = GETDATE() WHERE UserId = -1 AND CompanyID = @CompanyID AND Title = @Title",
                    conn,
                    new SqlParameter[]
                    {
                        new SqlParameter("@CompanyID", SqlDbType.Int) { Value = companyId },
                        new SqlParameter("@Title", SqlDbType.NVarChar, 200) { Value = title },
                    });
            }
        }

        /// <summary>Seeds installment sales and cash loan dashboard widgets.</summary>
        public static void ApplyFinancingDashboardDefaults(int companyId)
        {
            clsSQL sql = new clsSQL();
            string conn = sql.CreateDataBaseConnectionString(companyId);

            foreach (var widget in GetFinancingDashboardCatalog())
            {
                UpsertTemplateWidget(sql, conn, companyId, widget);
            }

            foreach (var fix in GetFinancingDashboardCatalog())
            {
                sql.ExecuteNonQueryStatement(
                    "UPDATE tbl_DashboardWidgets SET SQLQuery = @Sql, ModificationDate = GETDATE() WHERE Title = @Title",
                    conn,
                    new SqlParameter[]
                    {
                        new SqlParameter("@Title", SqlDbType.NVarChar, 200) { Value = fix.Title },
                        new SqlParameter("@Sql", SqlDbType.NVarChar, -1) { Value = fix.Sql },
                    });
            }

            foreach (var fix in GetFinancingTypeFixes())
            {
                sql.ExecuteNonQueryStatement(
                    "UPDATE tbl_DashboardWidgets SET WidgetType = @WidgetType, ModificationDate = GETDATE() WHERE Title = @Title",
                    conn,
                    new SqlParameter[]
                    {
                        new SqlParameter("@Title", SqlDbType.NVarChar, 200) { Value = fix.Title },
                        new SqlParameter("@WidgetType", SqlDbType.NVarChar, 50) { Value = fix.WidgetType },
                    });
            }

            DeactivateDashboardTemplates(sql, conn, companyId, GetFinancingDashboardCatalog());
        }

        public static void ApplyDashboardChartTypeUpgrades(int companyId)
        {
            ApplyDashboardWidgetTypeFixes(companyId);
            ApplyBusinessDashboardDefaults(companyId);
        }

        /// <summary>
        /// Applies leadership KPI SQL fixes, title renames, and refreshes all dashboard catalogs.
        /// </summary>
        public static void ApplyDashboardLeadershipKpiDefaults(int companyId)
        {
            ApplyBusinessDashboardDefaults(companyId);
            ApplyFinancingDashboardDefaults(companyId);
            ApplyAdvancedAnalyticsDashboardDefaults(companyId);
            ApplyDashboardWidgetSqlFixes(companyId);
            ApplyDashboardWidgetTypeFixes(companyId);
            ApplyDashboardTitleRenames(companyId);
        }

        private static void ApplyDashboardTitleRenames(int companyId)
        {
            clsSQL sql = new clsSQL();
            string conn = sql.CreateDataBaseConnectionString(companyId);

            var renames = new (string OldTitle, string NewTitle, bool? Deactivate)[]
            {
                ("Month To Day Revenue ", "MTD Revenue Recognized", null),
                ("Month To Day Revenue", "MTD Revenue Recognized", null),
                ("MTD Sales Revenue", "Sales Invoiced MTD", true),
                ("YTD Sales Revenue", "Sales Invoiced YTD", true),
                ("Monthly Sales Trend", "Monthly Revenue Trend", null),
                ("Monthly Sales vs Purchases", "Sales vs Purchases Trend", null),
                ("Outstanding Receivables", "Open AR Items", null),
                ("Pending Invoices", "Open Sales Invoices (Unsettled)", true),
                ("Top 5 Customers by Invoice Amount", "Top 5 Customers by Revenue", null),
            };

            foreach (var (oldTitle, newTitle, deactivate) in renames)
            {
                var parameters = new List<SqlParameter>
                {
                    new SqlParameter("@OldTitle", SqlDbType.NVarChar, 200) { Value = oldTitle },
                    new SqlParameter("@NewTitle", SqlDbType.NVarChar, 200) { Value = newTitle },
                    new SqlParameter("@CompanyID", SqlDbType.Int) { Value = companyId },
                };

                string setActive = deactivate == true ? ", IsActive = 0" : string.Empty;
                sql.ExecuteNonQueryStatement(
                    $@"UPDATE tbl_DashboardWidgets
                       SET Title = @NewTitle, ModificationDate = GETDATE(){setActive}
                       WHERE CompanyID = @CompanyID AND RTRIM(Title) = RTRIM(@OldTitle)",
                    conn,
                    parameters.ToArray());
            }
        }

        /// <summary>
        /// Seeds partner analytics: returning customers, sales/purchases by partner, open vs settled.
        /// </summary>
        public static void ApplyAdvancedAnalyticsDashboardDefaults(int companyId)
        {
            clsSQL sql = new clsSQL();
            string conn = sql.CreateDataBaseConnectionString(companyId);

            foreach (var widget in GetAdvancedAnalyticsCatalog())
            {
                UpsertTemplateWidget(sql, conn, companyId, widget);
            }

            foreach (var fix in GetAdvancedAnalyticsCatalog())
            {
                sql.ExecuteNonQueryStatement(
                    "UPDATE tbl_DashboardWidgets SET SQLQuery = @Sql, ModificationDate = GETDATE() WHERE Title = @Title",
                    conn,
                    new SqlParameter[]
                    {
                        new SqlParameter("@Title", SqlDbType.NVarChar, 200) { Value = fix.Title },
                        new SqlParameter("@Sql", SqlDbType.NVarChar, -1) { Value = fix.Sql },
                    });
            }

            foreach (var fix in GetAdvancedAnalyticsTypeFixes())
            {
                sql.ExecuteNonQueryStatement(
                    "UPDATE tbl_DashboardWidgets SET WidgetType = @WidgetType, ModificationDate = GETDATE() WHERE Title = @Title",
                    conn,
                    new SqlParameter[]
                    {
                        new SqlParameter("@Title", SqlDbType.NVarChar, 200) { Value = fix.Title },
                        new SqlParameter("@WidgetType", SqlDbType.NVarChar, 50) { Value = fix.WidgetType },
                    });
            }

            DeactivateDashboardTemplates(sql, conn, companyId, GetAdvancedAnalyticsCatalog());
        }

        private static void DeactivateDashboardTemplates(
            clsSQL sql,
            string conn,
            int companyId,
            IReadOnlyList<DefaultDashboardWidgetDefinition> widgets)
        {
            foreach (var widget in widgets)
            {
                sql.ExecuteNonQueryStatement(
                    @"UPDATE tbl_DashboardWidgets SET IsActive = 0, ModificationDate = GETDATE()
                      WHERE UserId = -1 AND CompanyID = @CompanyID AND Title = @Title",
                    conn,
                    new SqlParameter[]
                    {
                        new SqlParameter("@CompanyID", SqlDbType.Int) { Value = companyId },
                        new SqlParameter("@Title", SqlDbType.NVarChar, 200) { Value = widget.Title },
                    });
            }
        }

        public static IReadOnlyList<DefaultDashboardWidgetDefinition> GetAdvancedAnalyticsCatalog()
        {
            return new[]
            {
                // ── Customer & partner KPIs ─────────────────────────────────────
                new DefaultDashboardWidgetDefinition
                {
                    Title = "Returning Customer Rate",
                    GroupName = "Customers",
                    Sql = ReturningCustomerRateSql(),
                    Icon = "0xf234",
                    Color = "#3498db",
                    SectionName = "leftWidgets",
                    SectionIndex = 220,
                },
                new DefaultDashboardWidgetDefinition
                {
                    Title = "Repeat Financing Customer Rate",
                    GroupName = "Financing",
                    Sql = RepeatFinancingCustomerRateSql(),
                    Icon = "0xf234",
                    Color = "#9b59b6",
                    SectionName = "leftWidgets",
                    SectionIndex = 230,
                },
                new DefaultDashboardWidgetDefinition
                {
                    Title = "Installment Collection Rate",
                    GroupName = "Installment Sales",
                    Sql = InstallmentCollectionRateSql(),
                    Icon = "0xf201",
                    Color = "#27ae60",
                    SectionName = "rightWidgets",
                    SectionIndex = 250,
                },
                new DefaultDashboardWidgetDefinition
                {
                    Title = "Open Installment Balance",
                    GroupName = "Installment Sales",
                    Sql = OpenInstallmentBalanceSql(),
                    Icon = "0xe8a1",
                    Color = "#e67e22",
                    SectionName = "rightWidgets",
                    SectionIndex = 260,
                },
                new DefaultDashboardWidgetDefinition
                {
                    Title = "Open Financing Contracts",
                    GroupName = "Financing",
                    Sql = OpenFinancingContractsSql(),
                    Icon = "0xf071",
                    Color = "#e74c3c",
                    SectionName = "rightWidgets",
                    SectionIndex = 270,
                },
                new DefaultDashboardWidgetDefinition
                {
                    Title = "Settled Financing Contracts",
                    GroupName = "Financing",
                    Sql = SettledFinancingContractsSql(),
                    Icon = "0xf058",
                    Color = "#2ecc71",
                    SectionName = "rightWidgets",
                    SectionIndex = 280,
                },
                new DefaultDashboardWidgetDefinition
                {
                    Title = "Settled Sales Invoices YTD",
                    GroupName = "Sales",
                    Sql = SettledSalesInvoicesSql(),
                    Icon = "0xf058",
                    Color = "#16a085",
                    SectionName = "rightWidgets",
                    SectionIndex = 290,
                },

                // ── Partner bar charts ────────────────────────────────────────────
                new DefaultDashboardWidgetDefinition
                {
                    Title = "Top Customers by Sales",
                    WidgetType = "HorizontalBarChart",
                    GroupName = "Sales",
                    Sql = TopCustomersBySalesSql(),
                    Icon = "0xf007",
                    Color = "#2980b9",
                    SectionName = "midWidgets",
                    SectionIndex = 910,
                },
                new DefaultDashboardWidgetDefinition
                {
                    Title = "Top Vendors by Purchases",
                    WidgetType = "HorizontalBarChart",
                    GroupName = "Purchasing",
                    Sql = TopVendorsByPurchasesSql(),
                    Icon = "0xf007",
                    Color = "#d35400",
                    SectionName = "midWidgets",
                    SectionIndex = 920,
                },
                new DefaultDashboardWidgetDefinition
                {
                    Title = "Top 10 Customers by Revenue",
                    WidgetType = "HorizontalBarChart",
                    GroupName = "Customers",
                    Sql = TopTenCustomersByRevenueSql(),
                    Icon = "0xf155",
                    Color = "#2ecc71",
                    SectionName = "midWidgets",
                    SectionIndex = 930,
                },
                new DefaultDashboardWidgetDefinition
                {
                    Title = "Sales vs Purchases by Customer",
                    WidgetType = "StackedBarChart",
                    GroupName = "Customers",
                    Sql = SalesVsPurchasesByCustomerSql(),
                    Icon = "0xf080",
                    Color = "#8e44ad",
                    SectionName = "midWidgets",
                    SectionIndex = 940,
                },

                // ── Open vs settled breakdowns ────────────────────────────────────
                new DefaultDashboardWidgetDefinition
                {
                    Title = "Open vs Settled Financing",
                    WidgetType = "DonutChart",
                    GroupName = "Financing",
                    Sql = OpenVsSettledFinancingSql(),
                    Icon = "0xf080",
                    Color = "#6c3483",
                    SectionName = "midWidgets",
                    SectionIndex = 950,
                },
                new DefaultDashboardWidgetDefinition
                {
                    Title = "Open vs Settled Sales Invoices",
                    WidgetType = "DonutChart",
                    GroupName = "Sales",
                    Sql = OpenVsSettledSalesInvoicesSql(),
                    Icon = "0xf080",
                    Color = "#3498db",
                    SectionName = "midWidgets",
                    SectionIndex = 960,
                },
                new DefaultDashboardWidgetDefinition
                {
                    Title = "Open vs Settled Installment Contracts",
                    WidgetType = "DonutChart",
                    GroupName = "Installment Sales",
                    Sql = OpenVsSettledInstallmentContractsSql(),
                    Icon = "0xf080",
                    Color = "#8e44ad",
                    SectionName = "midWidgets",
                    SectionIndex = 970,
                },
            };
        }

        private static IEnumerable<(string Title, string WidgetType)> GetAdvancedAnalyticsTypeFixes()
        {
            yield return ("Top Customers by Sales", "HorizontalBarChart");
            yield return ("Top Vendors by Purchases", "HorizontalBarChart");
            yield return ("Top 10 Customers by Revenue", "HorizontalBarChart");
            yield return ("Sales vs Purchases by Customer", "StackedBarChart");
            yield return ("Open vs Settled Financing", "DonutChart");
            yield return ("Open vs Settled Sales Invoices", "DonutChart");
            yield return ("Open vs Settled Installment Contracts", "DonutChart");
        }

        public static IReadOnlyList<DefaultDashboardWidgetDefinition> GetFinancingDashboardCatalog()
        {
            return new[]
            {
                // ── Installment sales KPIs ───────────────────────────────────────
                new DefaultDashboardWidgetDefinition
                {
                    Title = "MTD Installment Sales",
                    GroupName = "Installment Sales",
                    Sql = MtdInstallmentSalesSql(),
                    Icon = "0xf155",
                    Color = "#8e44ad",
                    SectionName = "leftWidgets",
                    SectionIndex = 190,
                },
                new DefaultDashboardWidgetDefinition
                {
                    Title = "YTD Installment Sales",
                    GroupName = "Installment Sales",
                    Sql = YtdInstallmentSalesSql(),
                    Icon = "0xe263",
                    Color = "#9b59b6",
                    SectionName = "leftWidgets",
                    SectionIndex = 200,
                },
                new DefaultDashboardWidgetDefinition
                {
                    Title = "Active Installment Contracts",
                    GroupName = "Installment Sales",
                    Sql = ActiveInstallmentContractsSql(),
                    Icon = "0xf080",
                    Color = "#6c3483",
                    SectionName = "leftWidgets",
                    SectionIndex = 210,
                },
                new DefaultDashboardWidgetDefinition
                {
                    Title = "Installments Due This Month",
                    GroupName = "Installment Sales",
                    Sql = InstallmentsDueThisMonthSql(),
                    Icon = "0xf071",
                    Color = "#e67e22",
                    SectionName = "rightWidgets",
                    SectionIndex = 190,
                },
                new DefaultDashboardWidgetDefinition
                {
                    Title = "Overdue Installments",
                    GroupName = "Installment Sales",
                    Sql = OverdueInstallmentsSql(),
                    Icon = "0xf071",
                    Color = "#e74c3c",
                    SectionName = "rightWidgets",
                    SectionIndex = 200,
                },

                // ── Cash loan KPIs ───────────────────────────────────────────────
                new DefaultDashboardWidgetDefinition
                {
                    Title = "MTD Cash Loans Disbursed",
                    GroupName = "Cash Loans",
                    Sql = MtdCashLoansDisbursedSql(),
                    Icon = "0xf0d6",
                    Color = "#d35400",
                    SectionName = "rightWidgets",
                    SectionIndex = 210,
                },
                new DefaultDashboardWidgetDefinition
                {
                    Title = "YTD Cash Loans Disbursed",
                    GroupName = "Cash Loans",
                    Sql = YtdCashLoansDisbursedSql(),
                    Icon = "0xf0d6",
                    Color = "#ca6f1e",
                    SectionName = "rightWidgets",
                    SectionIndex = 220,
                },
                new DefaultDashboardWidgetDefinition
                {
                    Title = "Active Cash Loans",
                    GroupName = "Cash Loans",
                    Sql = ActiveCashLoansSql(),
                    Icon = "0xe53f",
                    Color = "#ba4a00",
                    SectionName = "rightWidgets",
                    SectionIndex = 230,
                },
                new DefaultDashboardWidgetDefinition
                {
                    Title = "Outstanding Cash Loan Balance",
                    GroupName = "Cash Loans",
                    Sql = OutstandingCashLoanBalanceSql(),
                    Icon = "0xe8a1",
                    Color = "#a04000",
                    SectionName = "rightWidgets",
                    SectionIndex = 240,
                },

                // ── Financing charts & tables ────────────────────────────────────
                new DefaultDashboardWidgetDefinition
                {
                    Title = "Monthly Installment Sales Trend",
                    WidgetType = "AreaChart",
                    GroupName = "Installment Sales",
                    Sql = MonthlyInstallmentSalesTrendSql(),
                    Icon = "0xf080",
                    Color = "#8e44ad",
                    SectionName = "midWidgets",
                    SectionIndex = 850,
                },
                new DefaultDashboardWidgetDefinition
                {
                    Title = "Monthly Cash Loans Trend",
                    WidgetType = "AreaChart",
                    GroupName = "Cash Loans",
                    Sql = MonthlyCashLoansTrendSql(),
                    Icon = "0xf0d6",
                    Color = "#d35400",
                    SectionName = "midWidgets",
                    SectionIndex = 860,
                },
                new DefaultDashboardWidgetDefinition
                {
                    Title = "Installment vs Cash Loans",
                    WidgetType = "ComboChart",
                    GroupName = "Financing",
                    Sql = InstallmentVsCashLoansTrendSql(),
                    Icon = "0xf080",
                    Color = "#6c3483",
                    SectionName = "midWidgets",
                    SectionIndex = 870,
                },
                new DefaultDashboardWidgetDefinition
                {
                    Title = "Financing by Loan Type",
                    WidgetType = "DonutChart",
                    GroupName = "Financing",
                    Sql = FinancingByLoanTypeSql(),
                    Icon = "0xf007",
                    Color = "#9b59b6",
                    SectionName = "midWidgets",
                    SectionIndex = 880,
                },
                new DefaultDashboardWidgetDefinition
                {
                    Title = "Top Customers by Financing",
                    WidgetType = "HorizontalBarChart",
                    GroupName = "Financing",
                    Sql = TopCustomersByFinancingSql(),
                    Icon = "0xf007",
                    Color = "#e67e22",
                    SectionName = "midWidgets",
                    SectionIndex = 890,
                },
                new DefaultDashboardWidgetDefinition
                {
                    Title = "Recent Financing Contracts",
                    WidgetType = "Table",
                    GroupName = "Financing",
                    Sql = RecentFinancingContractsSql(),
                    Icon = "0xf07a",
                    Color = "#16a085",
                    SectionName = "midWidgets",
                    SectionIndex = 900,
                },
            };
        }

        private static IEnumerable<(string Title, string WidgetType)> GetFinancingTypeFixes()
        {
            yield return ("Monthly Installment Sales Trend", "AreaChart");
            yield return ("Monthly Cash Loans Trend", "AreaChart");
            yield return ("Installment vs Cash Loans", "ComboChart");
            yield return ("Financing by Loan Type", "DonutChart");
            yield return ("Top Customers by Financing", "HorizontalBarChart");
            yield return ("Top 10 Customers by Revenue", "HorizontalBarChart");
        }

        public static IReadOnlyList<DefaultDashboardWidgetDefinition> GetBusinessDefaultCatalog()
        {
            return new[]
            {
                // ── Finance KPIs (left column) ───────────────────────────────────
                new DefaultDashboardWidgetDefinition
                {
                    Title = "Sales Invoiced MTD",
                    GroupName = "Sales",
                    Sql = MtdSalesInvoicedSql(),
                    Icon = "0xf155",
                    Color = "#2ecc71",
                    SectionName = "leftWidgets",
                    SectionIndex = 100,
                    IsActive = false,
                },
                new DefaultDashboardWidgetDefinition
                {
                    Title = "MTD Revenue Recognized",
                    GroupName = "Finance",
                    Sql = MtdRevenueRecognizedSql(),
                    Icon = "0xf155",
                    Color = "#27ae60",
                    SectionName = "leftWidgets",
                    SectionIndex = 110,
                },
                new DefaultDashboardWidgetDefinition
                {
                    Title = "YTD Revenue Recognized",
                    GroupName = "Finance",
                    Sql = GlYtdRevenueSql(),
                    Icon = "0xe263",
                    Color = "#16a085",
                    SectionName = "leftWidgets",
                    SectionIndex = 120,
                },
                new DefaultDashboardWidgetDefinition
                {
                    Title = "Month Gross Profit Margin",
                    GroupName = "Finance",
                    Sql = MonthGrossProfitMarginSql(),
                    Icon = "0xf0e7",
                    Color = "#8e44ad",
                    SectionName = "leftWidgets",
                    SectionIndex = 130,
                },
                new DefaultDashboardWidgetDefinition
                {
                    Title = "Net Profit MTD",
                    GroupName = "Finance",
                    Sql = NetProfitMtdSql(),
                    Icon = "0xf0e7",
                    Color = "#9b59b6",
                    SectionName = "leftWidgets",
                    SectionIndex = 140,
                },
                new DefaultDashboardWidgetDefinition
                {
                    Title = "Accounts Receivable",
                    GroupName = "Receivables",
                    Sql = AccountsReceivableSql(),
                    Icon = "0xe8a1",
                    Color = "#2980b9",
                    SectionName = "leftWidgets",
                    SectionIndex = 150,
                },
                new DefaultDashboardWidgetDefinition
                {
                    Title = "Open AR Items",
                    GroupName = "Receivables",
                    Sql = OutstandingReceivablesTotalSql(),
                    Icon = "0xe8a1",
                    Color = "#e67e22",
                    SectionName = "leftWidgets",
                    SectionIndex = 160,
                },
                new DefaultDashboardWidgetDefinition
                {
                    Title = "Accounts Payable",
                    GroupName = "Payables",
                    Sql = AccountsPayableSql(),
                    Icon = "0xe8a1",
                    Color = "#c0392b",
                    SectionName = "leftWidgets",
                    SectionIndex = 170,
                },
                new DefaultDashboardWidgetDefinition
                {
                    Title = "Cash & Bank Balance",
                    GroupName = "Finance",
                    Sql = CashAndBankBalanceSql(),
                    Icon = "0xe53f",
                    Color = "#1abc9c",
                    SectionName = "leftWidgets",
                    SectionIndex = 180,
                },

                // ── Operations KPIs (right column) ─────────────────────────────
                new DefaultDashboardWidgetDefinition
                {
                    Title = "MTD Expenses",
                    GroupName = "Finance",
                    Sql = MtdExpensesSql(),
                    Icon = "0xf0d6",
                    Color = "#c0392b",
                    SectionName = "rightWidgets",
                    SectionIndex = 100,
                },
                new DefaultDashboardWidgetDefinition
                {
                    Title = "Active Customers",
                    GroupName = "Sales",
                    Sql = ActiveCustomersSql(),
                    Icon = "0xf007",
                    Color = "#e67e22",
                    SectionName = "rightWidgets",
                    SectionIndex = 110,
                },
                new DefaultDashboardWidgetDefinition
                {
                    Title = "Active Vendors",
                    GroupName = "Purchasing",
                    Sql = ActiveVendorsSql(),
                    Icon = "0xf007",
                    Color = "#d35400",
                    SectionName = "rightWidgets",
                    SectionIndex = 120,
                    IsActive = false,
                },
                new DefaultDashboardWidgetDefinition
                {
                    Title = "New Customers MTD",
                    GroupName = "Sales",
                    Sql = NewCustomersMtdSql(),
                    Icon = "0xf234",
                    Color = "#3498db",
                    SectionName = "rightWidgets",
                    SectionIndex = 130,
                },
                new DefaultDashboardWidgetDefinition
                {
                    Title = "Open Sales Invoices (Unsettled)",
                    GroupName = "Sales",
                    Sql = PendingSalesInvoicesSql(),
                    Icon = "0xf071",
                    Color = "#e74c3c",
                    SectionName = "rightWidgets",
                    SectionIndex = 140,
                    IsActive = false,
                },
                new DefaultDashboardWidgetDefinition
                {
                    Title = "Inventory Value",
                    GroupName = "Inventory",
                    Sql = InventoryValueSql(),
                    Icon = "0xe8cc",
                    Color = "#1abc9c",
                    SectionName = "rightWidgets",
                    SectionIndex = 150,
                },
                new DefaultDashboardWidgetDefinition
                {
                    Title = "Total Products",
                    GroupName = "Inventory",
                    Sql = TotalProductsSql(),
                    Icon = "0xe8cc",
                    Color = "#16a085",
                    SectionName = "rightWidgets",
                    SectionIndex = 160,
                    IsActive = false,
                },
                new DefaultDashboardWidgetDefinition
                {
                    Title = "Total Employees",
                    GroupName = "HR",
                    Sql = TotalEmployeesSql(),
                    Icon = "0xe7ef",
                    Color = "#34495e",
                    SectionName = "rightWidgets",
                    SectionIndex = 170,
                },
                new DefaultDashboardWidgetDefinition
                {
                    Title = "Avg Sales per Customer",
                    GroupName = "Sales",
                    Sql = AvgSalesPerCustomerSql(),
                    Icon = "0xf080",
                    Color = "#2980b9",
                    SectionName = "rightWidgets",
                    SectionIndex = 180,
                    IsActive = false,
                },

                new DefaultDashboardWidgetDefinition
                {
                    Title = "Sales Invoiced YTD",
                    GroupName = "Sales",
                    Sql = YtdSalesInvoicedSql(),
                    Icon = "0xe263",
                    Color = "#16a085",
                    SectionName = "leftWidgets",
                    SectionIndex = 125,
                    IsActive = false,
                },

                // ── Charts (middle column) ───────────────────────────────────────
                new DefaultDashboardWidgetDefinition
                {
                    Title = "Monthly Revenue Trend",
                    WidgetType = "AreaChart",
                    GroupName = "Finance",
                    Sql = GlMonthlyRevenueTrendSql(),
                    Icon = "0xf080",
                    Color = "#2980b9",
                    SectionName = "midWidgets",
                    SectionIndex = 100,
                },
                new DefaultDashboardWidgetDefinition
                {
                    Title = "Sales vs Purchases Trend",
                    WidgetType = "ComboChart",
                    GroupName = "Sales",
                    Sql = MonthlySalesVsPurchasesSql(),
                    Icon = "0xf080",
                    Color = "#2ecc71",
                    SectionName = "midWidgets",
                    SectionIndex = 200,
                },
                new DefaultDashboardWidgetDefinition
                {
                    Title = "Top 5 Customers by Revenue",
                    WidgetType = "HorizontalBarChart",
                    GroupName = "Sales",
                    Sql = TopCustomersSql(),
                    Icon = "0xf007",
                    Color = "#e67e22",
                    SectionName = "midWidgets",
                    SectionIndex = 300,
                },
                new DefaultDashboardWidgetDefinition
                {
                    Title = "Outstanding Invoices by Age (0-30, 30-60, 60+ Days).",
                    WidgetType = "DonutChart",
                    GroupName = "Receivables",
                    Sql = OutstandingInvoicesSql(),
                    Icon = "0xf071",
                    Color = "#e74c3c",
                    SectionName = "midWidgets",
                    SectionIndex = 400,
                },
                new DefaultDashboardWidgetDefinition
                {
                    Title = "Branch-wise Sales",
                    WidgetType = "DonutChart",
                    GroupName = "Sales",
                    Sql = BranchWiseSalesSql(),
                    Icon = "0xf080",
                    Color = "#3498db",
                    SectionName = "midWidgets",
                    SectionIndex = 500,
                },
                new DefaultDashboardWidgetDefinition
                {
                    Title = "Monthly Expense Trend",
                    WidgetType = "AreaChart",
                    GroupName = "Finance",
                    Sql = MonthlyExpenseTrendSql(),
                    Icon = "0xf0d6",
                    Color = "#c0392b",
                    SectionName = "midWidgets",
                    SectionIndex = 600,
                },
                new DefaultDashboardWidgetDefinition
                {
                    Title = "Top Products by Sales",
                    WidgetType = "HorizontalBarChart",
                    GroupName = "Sales",
                    Sql = TopProductsBySalesSql(),
                    Icon = "0xf128",
                    Color = "#1abc9c",
                    SectionName = "midWidgets",
                    SectionIndex = 700,
                    IsActive = false,
                },
                new DefaultDashboardWidgetDefinition
                {
                    Title = "Recent Sales Invoices",
                    WidgetType = "Table",
                    GroupName = "Sales",
                    Sql = RecentSalesInvoicesSql(),
                    Icon = "0xf07a",
                    Color = "#16a085",
                    SectionName = "midWidgets",
                    SectionIndex = 800,
                },

                new DefaultDashboardWidgetDefinition
                {
                    Title = "Revenue vs Expense Trend",
                    WidgetType = "ComboChart",
                    GroupName = "Finance",
                    Sql = RevenueVsExpenseTrendSql(),
                    Icon = "0xf080",
                    Color = "#8e44ad",
                    SectionName = "midWidgets",
                    SectionIndex = 750,
                    IsActive = false,
                },
                new DefaultDashboardWidgetDefinition
                {
                    Title = "Sales vs Purchases (Stacked)",
                    WidgetType = "StackedBarChart",
                    GroupName = "Sales",
                    Sql = MonthlySalesVsPurchasesSql(),
                    Icon = "0xf080",
                    Color = "#16a085",
                    SectionName = "midWidgets",
                    SectionIndex = 760,
                    IsActive = false,
                },

                // ── Optional / advanced (available but off by default) ─────────
                new DefaultDashboardWidgetDefinition
                {
                    Title = "Monthly Debit vs Credit",
                    WidgetType = "LineChart",
                    GroupName = "Finance",
                    Sql = MonthlyDebitVsCreditSql(),
                    Icon = "0xe85d",
                    Color = "#f39c12",
                    SectionName = "midWidgets",
                    SectionIndex = 900,
                    IsActive = false,
                },
                new DefaultDashboardWidgetDefinition
                {
                    Title = "Inventory Turnover Rate/YTD",
                    WidgetType = "KPI",
                    GroupName = "Inventory",
                    Sql = InventoryTurnoverSql(),
                    Icon = "0xe8cc",
                    Color = "#1abc9c",
                    SectionName = "rightWidgets",
                    SectionIndex = 200,
                    IsActive = false,
                },
            };
        }

        private static IEnumerable<string> GetTechnicalOnlyTitles()
        {
            yield return "Total Credit Amount";
            yield return "Total Journal Vouchers";
            yield return "Total Accounts";
            yield return "Total Cost Centers";
            yield return "Total Branches";
            yield return "Total Transactions";
            yield return "Lowest Sale";
            yield return "Highest Sale";
            yield return "Account Balances Overview";
            yield return "Debit by Account";
            yield return "Credit by Account";
            yield return "Monthly Debit Trend";
            yield return "Monthly Credit Trend";
            yield return "Best Performing Accounts by Credit";
            yield return "Customer Retention Rate";
            yield return "Debit Distribution by Cost Center";
            yield return "Recent Journal Entries";
            yield return "Recent Transactions";
            yield return "Total Revenue";
            yield return "Total Expenses";
            yield return "Profit Margin";
        }

        private static void UpsertTemplateWidget(
            clsSQL sql,
            string conn,
            int companyId,
            DefaultDashboardWidgetDefinition widget)
        {
            var exists = sql.ExecuteQueryStatement(
                "SELECT ID FROM tbl_DashboardWidgets WHERE UserId = -1 AND CompanyID = @CompanyID AND Title = @Title",
                conn,
                new SqlParameter[]
                {
                    new SqlParameter("@CompanyID", SqlDbType.Int) { Value = companyId },
                    new SqlParameter("@Title", SqlDbType.NVarChar, 200) { Value = widget.Title },
                });

            if (exists != null && exists.Rows.Count > 0)
            {
                sql.ExecuteNonQueryStatement(
                    @"UPDATE tbl_DashboardWidgets SET
                        WidgetType = @WidgetType,
                        GroupName = @GroupName,
                        SQLQuery = @Sql,
                        Icon = @Icon,
                        Color = @Color,
                        SectionName = @SectionName,
                        SectionIndex = @SectionIndex,
                        IsActive = @IsActive,
                        ModificationDate = GETDATE()
                      WHERE UserId = -1 AND CompanyID = @CompanyID AND Title = @Title",
                    conn,
                    BuildWidgetParameters(companyId, widget));
                return;
            }

            sql.ExecuteNonQueryStatement(
                @"INSERT INTO tbl_DashboardWidgets
                    (UserId, WidgetType, GroupName, Title, SQLQuery, ChartConfig, Icon, Color,
                     SectionName, SectionIndex, CreationDate, CompanyID, IsActive)
                  VALUES
                    (-1, @WidgetType, @GroupName, @Title, @Sql, NULL, @Icon, @Color,
                     @SectionName, @SectionIndex, GETDATE(), @CompanyID, @IsActive)",
                conn,
                BuildWidgetParameters(companyId, widget));
        }

        private static SqlParameter[] BuildWidgetParameters(int companyId, DefaultDashboardWidgetDefinition widget)
        {
            return new SqlParameter[]
            {
                new SqlParameter("@CompanyID", SqlDbType.Int) { Value = companyId },
                new SqlParameter("@Title", SqlDbType.NVarChar, 200) { Value = widget.Title },
                new SqlParameter("@WidgetType", SqlDbType.NVarChar, 50) { Value = widget.WidgetType },
                new SqlParameter("@GroupName", SqlDbType.NVarChar, 100) { Value = widget.GroupName },
                new SqlParameter("@Sql", SqlDbType.NVarChar, -1) { Value = widget.Sql },
                new SqlParameter("@Icon", SqlDbType.NVarChar, 100) { Value = widget.Icon },
                new SqlParameter("@Color", SqlDbType.NVarChar, 50) { Value = widget.Color },
                new SqlParameter("@SectionName", SqlDbType.NVarChar, 50) { Value = widget.SectionName },
                new SqlParameter("@SectionIndex", SqlDbType.Int) { Value = widget.SectionIndex },
                new SqlParameter("@IsActive", SqlDbType.Bit) { Value = widget.IsActive },
            };
        }

        private static IEnumerable<(string Title, string WidgetType)> GetTypeFixes()
        {
            yield return ("Debit by Account", "PieChart");
            yield return ("Credit by Account", "PieChart");
            yield return ("Monthly Debit vs Credit", "LineChart");
            yield return ("Monthly Sales vs Purchases", "ComboChart");
            yield return ("Sales vs Purchases Trend", "ComboChart");
            yield return ("Revenue vs Expense Trend", "ComboChart");
            yield return ("Sales vs Purchases (Stacked)", "StackedBarChart");
            yield return ("Monthly Revenue Trend", "AreaChart");
            yield return ("Monthly Expense Trend", "AreaChart");
            yield return ("Outstanding Invoices by Age (0-30, 30-60, 60+ Days).", "DonutChart");
            yield return ("Branch-wise Sales", "DonutChart");
            yield return ("Top 5 Customers by Invoice Amount", "HorizontalBarChart");
            yield return ("Top 5 Customers by Revenue", "HorizontalBarChart");
            yield return ("Top Products by Sales", "HorizontalBarChart");
            foreach (var fix in GetFinancingTypeFixes())
            {
                yield return fix;
            }
            foreach (var fix in GetAdvancedAnalyticsTypeFixes())
            {
                yield return fix;
            }
        }

        private static IEnumerable<(string Title, string Sql)> GetSqlFixes()
        {
            foreach (var w in GetBusinessDefaultCatalog())
            {
                yield return (w.Title, w.Sql);
            }

            foreach (var w in GetFinancingDashboardCatalog())
            {
                yield return (w.Title, w.Sql);
            }

            foreach (var w in GetAdvancedAnalyticsCatalog())
            {
                yield return (w.Title, w.Sql);
            }

            // Legacy titles still present in older databases
            yield return ("Month To Day Revenue ", MtdRevenueRecognizedSql());
            yield return ("Month To Day Revenue", MtdRevenueRecognizedSql());
            yield return ("MTD Revenue Recognized", MtdRevenueRecognizedSql());
            yield return ("MTD Sales Revenue", MtdSalesInvoicedSql());
            yield return ("Sales Invoiced MTD", MtdSalesInvoicedSql());
            yield return ("YTD Sales Revenue", YtdSalesInvoicedSql());
            yield return ("Sales Invoiced YTD", YtdSalesInvoicedSql());
            yield return ("YTD Revenue Recognized", GlYtdRevenueSql());
            yield return ("Monthly Sales Trend", MonthlySalesTrendSql());
            yield return ("Monthly Revenue Trend", GlMonthlyRevenueTrendSql());
            yield return ("Revenue vs Expense Trend", RevenueVsExpenseTrendSql());
            yield return ("Sales vs Purchases (Stacked)", MonthlySalesVsPurchasesSql());
            yield return ("Sales vs Purchases Trend", MonthlySalesVsPurchasesSql());
            yield return ("Monthly Sales vs Purchases", MonthlySalesVsPurchasesSql());
            yield return ("Outstanding Receivables", OutstandingReceivablesTotalSql());
            yield return ("Open AR Items", OutstandingReceivablesTotalSql());
            yield return ("Pending Invoices", PendingSalesInvoicesSql());
            yield return ("Open Sales Invoices (Unsettled)", PendingSalesInvoicesSql());
            yield return ("Top 5 Customers by Invoice Amount", TopCustomersSql());
            yield return ("Top 5 Customers by Revenue", TopCustomersSql());
            yield return ("Total Revenue", GlYtdRevenueSql());
            yield return ("Total Expenses", MtdExpensesSql());
            yield return ("Profit Margin", NetProfitMtdSql());
            yield return ("Customer Retention Rate", ReturningCustomerRateSql());
            yield return ("Recent Transactions", RecentSalesInvoicesSql());
            yield return ("Top 10 Customers by Revenue", TopTenCustomersByRevenueSql());
            yield return ("Debit Distribution by Cost Center", CostCenterDebitSql());
            yield return ("Account Balances Overview", CashAndBankBalanceSql());
            yield return ("Debit by Account", TopDebitAccountsSql());
            yield return ("Credit by Account", TopCreditAccountsSql());
            yield return ("Monthly Debit vs Credit", MonthlyDebitVsCreditSql());
            yield return ("Total Credit Amount", TotalCreditAmountSql());
            yield return ("Total Journal Vouchers", TotalJournalVouchersSql());
            yield return ("Total Accounts", TotalAccountsSql());
            yield return ("Total Branches", TotalBranchesSql());
            yield return ("Total Transactions", TotalTransactionsSql());
            yield return ("Highest Sale", HighestSaleSql());
            yield return ("Lowest Sale", LowestSaleSql());
            yield return ("Avg Sale", AvgSalesPerCustomerSql());
        }

        // ── KPI SQL ───────────────────────────────────────────────────────────

        private static string KpiMomComparisonSql(string currentSubquery, string previousSubquery) => $@"
SELECT
    COALESCE(CQ.Total, 0) AS Total,
    CASE
        WHEN COALESCE(PQ.Total, 0) = 0 AND COALESCE(CQ.Total, 0) = 0 THEN 0
        WHEN COALESCE(PQ.Total, 0) = 0 THEN 100
        ELSE (COALESCE(CQ.Total, 0) - COALESCE(PQ.Total, 0)) * 100.0 / COALESCE(PQ.Total, 1)
    END AS PercentageChange
FROM ({currentSubquery}) CQ, ({previousSubquery}) PQ";

        private static string BalanceMomComparisonSql(string accountFilter) => KpiMomComparisonSql(
            $@"SELECT ISNULL(SUM(ISNULL(D.Debit, 0) - ISNULL(D.Credit, 0)), 0) AS Total
FROM tbl_JournalVoucherDetails D
WHERE D.AccountID IN {accountFilter}",
            $@"SELECT ISNULL(SUM(ISNULL(D.Debit, 0) - ISNULL(D.Credit, 0)), 0) AS Total
FROM tbl_JournalVoucherDetails D
JOIN tbl_JournalVoucherHeader H ON D.ParentGuid = H.Guid
WHERE D.AccountID IN {accountFilter}
  AND H.VoucherDate <= EOMONTH(DATEADD(MONTH, -1, GETDATE()))");

        private static string MtdSalesInvoicedSql() => KpiMomComparisonSql(
            $@"SELECT ISNULL(SUM(TotalInvoice), 0) AS Total
FROM tbl_InvoiceHeader
WHERE {SalesInvoiceTypeFilter}
  AND {InvoiceCountedFilter}
  AND YEAR(InvoiceDate) = YEAR(GETDATE())
  AND MONTH(InvoiceDate) = MONTH(GETDATE())",
            $@"SELECT ISNULL(SUM(TotalInvoice), 0) AS Total
FROM tbl_InvoiceHeader
WHERE {SalesInvoiceTypeFilter}
  AND {InvoiceCountedFilter}
  AND YEAR(InvoiceDate) = YEAR(DATEADD(MONTH, -1, GETDATE()))
  AND MONTH(InvoiceDate) = MONTH(DATEADD(MONTH, -1, GETDATE()))");

        private static string YtdSalesInvoicedSql() => KpiMomComparisonSql(
            $@"SELECT ISNULL(SUM(TotalInvoice), 0) AS Total
FROM tbl_InvoiceHeader
WHERE {SalesInvoiceTypeFilter}
  AND {InvoiceCountedFilter}
  AND YEAR(InvoiceDate) = YEAR(GETDATE())",
            $@"SELECT ISNULL(SUM(TotalInvoice), 0) AS Total
FROM tbl_InvoiceHeader
WHERE {SalesInvoiceTypeFilter}
  AND {InvoiceCountedFilter}
  AND YEAR(InvoiceDate) = YEAR(DATEADD(YEAR, -1, GETDATE()))
  AND InvoiceDate <= DATEADD(YEAR, -1, GETDATE())");

        private static string GlYtdRevenueSql() => KpiMomComparisonSql(
            $@"SELECT ISNULL(SUM(Total * -1), 0) AS Total
FROM tbl_JournalVoucherDetails
WHERE AccountID IN {RevenueAccounts}
  AND YEAR(DueDate) = YEAR(GETDATE())",
            $@"SELECT ISNULL(SUM(Total * -1), 0) AS Total
FROM tbl_JournalVoucherDetails
WHERE AccountID IN {RevenueAccounts}
  AND YEAR(DueDate) = YEAR(DATEADD(YEAR, -1, GETDATE()))
  AND DueDate <= DATEADD(YEAR, -1, GETDATE())");

        private static string MtdRevenueRecognizedSql() => $@"
WITH CurrentMonth AS (
    SELECT SUM(Total * -1) AS Total
    FROM tbl_JournalVoucherDetails
    WHERE AccountID IN {RevenueAccounts}
      AND YEAR(DueDate) = YEAR(GETDATE())
      AND MONTH(DueDate) = MONTH(GETDATE())
      AND DAY(DueDate) <= DAY(GETDATE())
),
PreviousMonth AS (
    SELECT SUM(Total * -1) AS Total
    FROM tbl_JournalVoucherDetails
    WHERE AccountID IN {RevenueAccounts}
      AND YEAR(DueDate) = YEAR(DATEADD(MONTH, -1, GETDATE()))
      AND MONTH(DueDate) = MONTH(DATEADD(MONTH, -1, GETDATE()))
      AND DAY(DueDate) <= DAY(GETDATE())
)
SELECT
    COALESCE(CM.Total, 0) AS Total,
    CASE
        WHEN COALESCE(PM.Total, 0) = 0 AND COALESCE(CM.Total, 0) = 0 THEN 0
        WHEN COALESCE(PM.Total, 0) = 0 THEN 100
        ELSE (COALESCE(CM.Total, 0) - COALESCE(PM.Total, 0)) * 100.0 / COALESCE(PM.Total, 1)
    END AS PercentageChange
FROM CurrentMonth CM, PreviousMonth PM";

        private static string MonthGrossProfitMarginSql() => $@"
SELECT
    CASE WHEN q.totalRevenue = 0 THEN 0
         ELSE (q.totalRevenue - q.totalCost) / q.totalRevenue * 100
    END AS Total,
    CASE
        WHEN COALESCE(((q.totalRevenuePastMonth - q.totalCostPastMonth) / NULLIF(q.totalRevenuePastMonth, 0) * 100), 0) = 0 THEN NULL
        ELSE (
            COALESCE((q.totalRevenue - q.totalCost) / NULLIF(q.totalRevenue, 0) * 100, 0)
            - COALESCE((q.totalRevenuePastMonth - q.totalCostPastMonth) / NULLIF(q.totalRevenuePastMonth, 0) * 100, 0)
        ) * 100.0 / COALESCE(
            (q.totalRevenuePastMonth - q.totalCostPastMonth) / NULLIF(q.totalRevenuePastMonth, 0) * 100, 1)
    END AS PercentageChange
FROM (
    SELECT
        (SELECT ISNULL(SUM(Total * -1), 0) FROM tbl_JournalVoucherDetails
         WHERE AccountID IN {RevenueAccounts}
           AND YEAR(DueDate) = YEAR(GETDATE()) AND MONTH(DueDate) = MONTH(GETDATE())) AS totalRevenue,
        (SELECT ISNULL(SUM(Total), 0) FROM tbl_JournalVoucherDetails
         WHERE AccountID IN {CogsAccount}
           AND YEAR(DueDate) = YEAR(GETDATE()) AND MONTH(DueDate) = MONTH(GETDATE())) AS totalCost,
        (SELECT ISNULL(SUM(Total * -1), 0) FROM tbl_JournalVoucherDetails
         WHERE AccountID IN {RevenueAccounts}
           AND YEAR(DueDate) = YEAR(DATEADD(MONTH, -1, GETDATE()))
           AND MONTH(DueDate) = MONTH(DATEADD(MONTH, -1, GETDATE()))) AS totalRevenuePastMonth,
        (SELECT ISNULL(SUM(Total), 0) FROM tbl_JournalVoucherDetails
         WHERE AccountID IN {CogsAccount}
           AND YEAR(DueDate) = YEAR(DATEADD(MONTH, -1, GETDATE()))
           AND MONTH(DueDate) = MONTH(DATEADD(MONTH, -1, GETDATE()))) AS totalCostPastMonth
) q";

        private static string NetProfitMtdSql() => KpiMomComparisonSql(
            $@"SELECT ISNULL(rev.Total, 0) - ISNULL(exp.Total, 0) AS Total
FROM
(
    SELECT SUM(Total * -1) AS Total
    FROM tbl_JournalVoucherDetails
    WHERE AccountID IN {RevenueAccounts}
      AND YEAR(DueDate) = YEAR(GETDATE())
      AND MONTH(DueDate) = MONTH(GETDATE())
) rev,
(
    SELECT SUM(D.Debit) AS Total
    FROM tbl_JournalVoucherDetails D
    JOIN tbl_JournalVoucherHeader H ON D.ParentGuid = H.Guid
    WHERE D.AccountID IN {OperatingExpenseAccounts}
      AND YEAR(H.VoucherDate) = YEAR(GETDATE())
      AND MONTH(H.VoucherDate) = MONTH(GETDATE())
) exp",
            $@"SELECT ISNULL(rev.Total, 0) - ISNULL(exp.Total, 0) AS Total
FROM
(
    SELECT SUM(Total * -1) AS Total
    FROM tbl_JournalVoucherDetails
    WHERE AccountID IN {RevenueAccounts}
      AND YEAR(DueDate) = YEAR(DATEADD(MONTH, -1, GETDATE()))
      AND MONTH(DueDate) = MONTH(DATEADD(MONTH, -1, GETDATE()))
) rev,
(
    SELECT SUM(D.Debit) AS Total
    FROM tbl_JournalVoucherDetails D
    JOIN tbl_JournalVoucherHeader H ON D.ParentGuid = H.Guid
    WHERE D.AccountID IN {OperatingExpenseAccounts}
      AND YEAR(H.VoucherDate) = YEAR(DATEADD(MONTH, -1, GETDATE()))
      AND MONTH(H.VoucherDate) = MONTH(DATEADD(MONTH, -1, GETDATE()))
) exp");

        private static string AccountsReceivableSql() =>
            BalanceMomComparisonSql(ReceivableAccount);

        private static string AccountsPayableSql() => KpiMomComparisonSql(
            $@"SELECT ISNULL(SUM(ISNULL(D.Credit, 0) - ISNULL(D.Debit, 0)), 0) AS Total
FROM tbl_JournalVoucherDetails D
WHERE D.AccountID IN {PayableAccount}",
            $@"SELECT ISNULL(SUM(ISNULL(D.Credit, 0) - ISNULL(D.Debit, 0)), 0) AS Total
FROM tbl_JournalVoucherDetails D
JOIN tbl_JournalVoucherHeader H ON D.ParentGuid = H.Guid
WHERE D.AccountID IN {PayableAccount}
  AND H.VoucherDate <= EOMONTH(DATEADD(MONTH, -1, GETDATE()))");

        private static string CashAndBankBalanceSql() =>
            BalanceMomComparisonSql(CashAndBankAccounts);

        private static string OutstandingReceivablesTotalSql() => KpiMomComparisonSql(
            $@"WITH ReconciledAmounts AS (
    SELECT JVDetailsGuid, SUM(Amount) AS Reconciled
    FROM tbl_Reconciliation
    GROUP BY JVDetailsGuid
),
OpenItems AS (
    SELECT JVD.total - ISNULL(R.Reconciled, 0) AS Amount
    FROM tbl_JournalVoucherDetails JVD
    LEFT JOIN ReconciledAmounts R ON R.JVDetailsGuid = JVD.Guid
    WHERE JVD.DueDate <= GETDATE()
      AND JVD.SubAccountID > 0
      AND JVD.AccountID IN {ReceivableAccount}
)
SELECT ISNULL(SUM(CASE WHEN Amount > 0 THEN Amount ELSE 0 END), 0) AS Total
FROM OpenItems",
            $@"WITH ReconciledAmounts AS (
    SELECT JVDetailsGuid, SUM(Amount) AS Reconciled
    FROM tbl_Reconciliation
    GROUP BY JVDetailsGuid
),
OpenItems AS (
    SELECT JVD.total - ISNULL(R.Reconciled, 0) AS Amount
    FROM tbl_JournalVoucherDetails JVD
    LEFT JOIN ReconciledAmounts R ON R.JVDetailsGuid = JVD.Guid
    WHERE JVD.DueDate <= EOMONTH(DATEADD(MONTH, -1, GETDATE()))
      AND JVD.SubAccountID > 0
      AND JVD.AccountID IN {ReceivableAccount}
)
SELECT ISNULL(SUM(CASE WHEN Amount > 0 THEN Amount ELSE 0 END), 0) AS Total
FROM OpenItems");

        private static string MtdExpensesSql() => KpiMomComparisonSql(
            $@"SELECT ISNULL(SUM(D.Debit), 0) AS Total
FROM tbl_JournalVoucherDetails D
JOIN tbl_JournalVoucherHeader H ON D.ParentGuid = H.Guid
WHERE D.AccountID IN {OperatingExpenseAccounts}
  AND YEAR(H.VoucherDate) = YEAR(GETDATE())
  AND MONTH(H.VoucherDate) = MONTH(GETDATE())",
            $@"SELECT ISNULL(SUM(D.Debit), 0) AS Total
FROM tbl_JournalVoucherDetails D
JOIN tbl_JournalVoucherHeader H ON D.ParentGuid = H.Guid
WHERE D.AccountID IN {OperatingExpenseAccounts}
  AND YEAR(H.VoucherDate) = YEAR(DATEADD(MONTH, -1, GETDATE()))
  AND MONTH(H.VoucherDate) = MONTH(DATEADD(MONTH, -1, GETDATE()))");

        private static string ActiveCustomersSql() => KpiMomComparisonSql(
            @"SELECT COUNT(*) AS Total FROM tbl_BusinessPartner WHERE Active = 1 AND [Type] = 1",
            @"SELECT COUNT(*) AS Total
FROM tbl_BusinessPartner
WHERE Active = 1 AND [Type] = 1
  AND CreationDate <= EOMONTH(DATEADD(MONTH, -1, GETDATE()))");

        private static string ActiveVendorsSql() => KpiMomComparisonSql(
            @"SELECT COUNT(*) AS Total FROM tbl_BusinessPartner WHERE Active = 1 AND [Type] = 2",
            @"SELECT COUNT(*) AS Total
FROM tbl_BusinessPartner
WHERE Active = 1 AND [Type] = 2
  AND CreationDate <= EOMONTH(DATEADD(MONTH, -1, GETDATE()))");

        private static string NewCustomersMtdSql() => @"
SELECT q.Total,
    CASE
        WHEN q.TotalPastMonth = 0 AND q.Total = 0 THEN 0
        WHEN q.TotalPastMonth = 0 THEN 100
        ELSE (CAST(q.Total AS DECIMAL(18, 2)) - CAST(q.TotalPastMonth AS DECIMAL(18, 2)))
             * 100.0 / CAST(q.TotalPastMonth AS DECIMAL(18, 2))
    END AS PercentageChange
FROM (
    SELECT
        (SELECT COUNT(id) FROM tbl_BusinessPartner
         WHERE [Type] = 1 AND Active = 1
           AND YEAR(CreationDate) = YEAR(GETDATE()) AND MONTH(CreationDate) = MONTH(GETDATE())) AS Total,
        (SELECT COUNT(id) FROM tbl_BusinessPartner
         WHERE [Type] = 1 AND Active = 1
           AND YEAR(CreationDate) = YEAR(DATEADD(MONTH, -1, GETDATE()))
           AND MONTH(CreationDate) = MONTH(DATEADD(MONTH, -1, GETDATE()))) AS TotalPastMonth
) q";

        private static string PendingSalesInvoicesSql() => KpiMomComparisonSql(
            $@"SELECT COUNT(*) AS Total
FROM tbl_InvoiceHeader
WHERE {SalesInvoiceTypeFilter}
  AND {InvoiceCountedFilter}
  AND ISNULL(Status, 0) = 0",
            $@"SELECT COUNT(*) AS Total
FROM tbl_InvoiceHeader
WHERE {SalesInvoiceTypeFilter}
  AND {InvoiceCountedFilter}
  AND ISNULL(Status, 0) = 0
  AND InvoiceDate <= EOMONTH(DATEADD(MONTH, -1, GETDATE()))");

        private static string InventoryValueSql() =>
            BalanceMomComparisonSql(InventoryAccount);

        private static string TotalProductsSql() => KpiMomComparisonSql(
            @"SELECT COUNT(*) AS Total FROM tbl_Items WHERE ISNULL(IsActive, 1) = 1",
            @"SELECT COUNT(*) AS Total
FROM tbl_Items
WHERE ISNULL(IsActive, 1) = 1
  AND CreationDate <= EOMONTH(DATEADD(MONTH, -1, GETDATE()))");

        private static string TotalEmployeesSql() => KpiMomComparisonSql(
            @"SELECT COUNT(*) AS Total FROM tbl_employee",
            @"SELECT COUNT(*) AS Total
FROM tbl_employee
WHERE CreationDate <= EOMONTH(DATEADD(MONTH, -1, GETDATE()))");

        private static string TotalCreditAmountSql() => KpiMomComparisonSql(
            @"SELECT ISNULL(SUM(D.Credit), 0) AS Total
FROM tbl_JournalVoucherDetails D
JOIN tbl_JournalVoucherHeader H ON D.ParentGuid = H.Guid
WHERE YEAR(H.VoucherDate) = YEAR(GETDATE()) AND MONTH(H.VoucherDate) = MONTH(GETDATE())",
            @"SELECT ISNULL(SUM(D.Credit), 0) AS Total
FROM tbl_JournalVoucherDetails D
JOIN tbl_JournalVoucherHeader H ON D.ParentGuid = H.Guid
WHERE YEAR(H.VoucherDate) = YEAR(DATEADD(MONTH, -1, GETDATE()))
  AND MONTH(H.VoucherDate) = MONTH(DATEADD(MONTH, -1, GETDATE()))");

        private static string TotalJournalVouchersSql() => KpiMomComparisonSql(
            @"SELECT COUNT(*) AS Total
FROM tbl_JournalVoucherHeader
WHERE YEAR(VoucherDate) = YEAR(GETDATE()) AND MONTH(VoucherDate) = MONTH(GETDATE())",
            @"SELECT COUNT(*) AS Total
FROM tbl_JournalVoucherHeader
WHERE YEAR(VoucherDate) = YEAR(DATEADD(MONTH, -1, GETDATE()))
  AND MONTH(VoucherDate) = MONTH(DATEADD(MONTH, -1, GETDATE()))");

        private static string TotalAccountsSql() => KpiMomComparisonSql(
            @"SELECT COUNT(*) AS Total FROM tbl_Accounts",
            @"SELECT COUNT(*) AS Total
FROM tbl_Accounts
WHERE CreationDate <= EOMONTH(DATEADD(MONTH, -1, GETDATE()))");

        private static string TotalBranchesSql() => KpiMomComparisonSql(
            @"SELECT COUNT(*) AS Total FROM tbl_Branch",
            @"SELECT COUNT(*) AS Total
FROM tbl_Branch
WHERE CreationDate <= EOMONTH(DATEADD(MONTH, -1, GETDATE()))");

        private static string TotalTransactionsSql() => KpiMomComparisonSql(
            @"SELECT COUNT(*) AS Total
FROM tbl_CashVoucherHeader
WHERE YEAR(VoucherDate) = YEAR(GETDATE()) AND MONTH(VoucherDate) = MONTH(GETDATE())",
            @"SELECT COUNT(*) AS Total
FROM tbl_CashVoucherHeader
WHERE YEAR(VoucherDate) = YEAR(DATEADD(MONTH, -1, GETDATE()))
  AND MONTH(VoucherDate) = MONTH(DATEADD(MONTH, -1, GETDATE()))");

        private static string HighestSaleSql() => KpiMomComparisonSql(
            $@"SELECT ISNULL(MAX(TotalInvoice), 0) AS Total
FROM tbl_InvoiceHeader
WHERE {SalesInvoiceTypeFilter}
  AND {InvoiceCountedFilter}
  AND YEAR(InvoiceDate) = YEAR(GETDATE()) AND MONTH(InvoiceDate) = MONTH(GETDATE())",
            $@"SELECT ISNULL(MAX(TotalInvoice), 0) AS Total
FROM tbl_InvoiceHeader
WHERE {SalesInvoiceTypeFilter}
  AND {InvoiceCountedFilter}
  AND YEAR(InvoiceDate) = YEAR(DATEADD(MONTH, -1, GETDATE()))
  AND MONTH(InvoiceDate) = MONTH(DATEADD(MONTH, -1, GETDATE()))");

        private static string LowestSaleSql() => KpiMomComparisonSql(
            $@"SELECT ISNULL(MIN(TotalInvoice), 0) AS Total
FROM tbl_InvoiceHeader
WHERE {SalesInvoiceTypeFilter}
  AND {InvoiceCountedFilter}
  AND TotalInvoice > 0
  AND YEAR(InvoiceDate) = YEAR(GETDATE()) AND MONTH(InvoiceDate) = MONTH(GETDATE())",
            $@"SELECT ISNULL(MIN(TotalInvoice), 0) AS Total
FROM tbl_InvoiceHeader
WHERE {SalesInvoiceTypeFilter}
  AND {InvoiceCountedFilter}
  AND TotalInvoice > 0
  AND YEAR(InvoiceDate) = YEAR(DATEADD(MONTH, -1, GETDATE()))
  AND MONTH(InvoiceDate) = MONTH(DATEADD(MONTH, -1, GETDATE()))");

        private static string AvgSalesPerCustomerSql() => KpiMomComparisonSql(
            $@"SELECT CASE
    WHEN COUNT(DISTINCT BusinessPartnerID) = 0 THEN 0
    ELSE SUM(TotalInvoice) * 1.0 / COUNT(DISTINCT BusinessPartnerID)
END AS Total
FROM tbl_InvoiceHeader
WHERE {SalesInvoiceTypeFilter}
  AND {InvoiceCountedFilter}
  AND BusinessPartnerID > 0
  AND YEAR(InvoiceDate) = YEAR(GETDATE())
  AND MONTH(InvoiceDate) = MONTH(GETDATE())",
            $@"SELECT CASE
    WHEN COUNT(DISTINCT BusinessPartnerID) = 0 THEN 0
    ELSE SUM(TotalInvoice) * 1.0 / COUNT(DISTINCT BusinessPartnerID)
END AS Total
FROM tbl_InvoiceHeader
WHERE {SalesInvoiceTypeFilter}
  AND {InvoiceCountedFilter}
  AND BusinessPartnerID > 0
  AND YEAR(InvoiceDate) = YEAR(DATEADD(MONTH, -1, GETDATE()))
  AND MONTH(InvoiceDate) = MONTH(DATEADD(MONTH, -1, GETDATE()))");

        // ── Chart / table SQL ─────────────────────────────────────────────────

        private static string MonthlySalesTrendSql() => $@"
SELECT FORMAT(H.InvoiceDate, 'yyyy-MM') AS Name, SUM(H.TotalInvoice) AS Total
FROM tbl_InvoiceHeader H
WHERE H.{SalesInvoiceTypeFilter}
  AND H.{InvoiceCountedFilter}
  AND H.InvoiceDate >= DATEADD(MONTH, -11, DATEFROMPARTS(YEAR(GETDATE()), MONTH(GETDATE()), 1))
GROUP BY FORMAT(H.InvoiceDate, 'yyyy-MM')
ORDER BY Name";

        private static string GlMonthlyRevenueTrendSql() => $@"
SELECT FORMAT(DueDate, 'yyyy-MM') AS Name, SUM(Total * -1) AS Total
FROM tbl_JournalVoucherDetails
WHERE AccountID IN {RevenueAccounts}
  AND DueDate >= DATEADD(MONTH, -11, DATEFROMPARTS(YEAR(GETDATE()), MONTH(GETDATE()), 1))
GROUP BY FORMAT(DueDate, 'yyyy-MM')
ORDER BY Name";

        private static string RevenueVsExpenseTrendSql() => $@"
SELECT q.Name,
       ISNULL(q.Revenue, 0) AS Total,
       ISNULL(q.Expense, 0) AS TotalCredit
FROM (
    SELECT FORMAT(COALESCE(R.MonthKey, E.MonthKey), 'yyyy-MM') AS Name,
           R.Revenue,
           E.Expense
    FROM (
        SELECT FORMAT(DueDate, 'yyyy-MM') AS MonthKey,
               SUM(Total * -1) AS Revenue
        FROM tbl_JournalVoucherDetails
        WHERE AccountID IN {RevenueAccounts}
          AND DueDate >= DATEADD(MONTH, -11, DATEFROMPARTS(YEAR(GETDATE()), MONTH(GETDATE()), 1))
        GROUP BY FORMAT(DueDate, 'yyyy-MM')
    ) R
    FULL OUTER JOIN (
        SELECT FORMAT(H.VoucherDate, 'yyyy-MM') AS MonthKey,
               SUM(D.Debit) AS Expense
        FROM tbl_JournalVoucherDetails D
        JOIN tbl_JournalVoucherHeader H ON D.ParentGuid = H.Guid
        WHERE D.AccountID IN {OperatingExpenseAccounts}
          AND H.VoucherDate >= DATEADD(MONTH, -11, DATEFROMPARTS(YEAR(GETDATE()), MONTH(GETDATE()), 1))
        GROUP BY FORMAT(H.VoucherDate, 'yyyy-MM')
    ) E ON E.MonthKey = R.MonthKey
) q
WHERE q.Name IS NOT NULL
ORDER BY q.Name";

        private static string MonthlySalesVsPurchasesSql() => $@"
SELECT FORMAT(H.InvoiceDate, 'yyyy-MM') AS Name,
       SUM(CASE WHEN H.{SalesInvoiceTypeFilter} THEN H.TotalInvoice ELSE 0 END) AS Total,
       SUM(CASE WHEN H.{PurchaseInvoiceTypeFilter} THEN H.TotalInvoice ELSE 0 END) AS TotalCredit
FROM tbl_InvoiceHeader H
WHERE H.{InvoiceCountedFilter}
  AND H.InvoiceDate >= DATEADD(MONTH, -11, DATEFROMPARTS(YEAR(GETDATE()), MONTH(GETDATE()), 1))
GROUP BY FORMAT(H.InvoiceDate, 'yyyy-MM')
ORDER BY Name";

        private static string TopCustomersSql() => $@"
SELECT TOP 5 BP.EName AS Name, SUM(I.TotalInvoice) AS Total
FROM tbl_InvoiceHeader I
JOIN tbl_BusinessPartner BP ON I.BusinessPartnerID = BP.ID
WHERE I.{SalesInvoiceTypeFilter}
  AND I.{InvoiceCountedFilter}
  AND YEAR(I.InvoiceDate) = YEAR(GETDATE())
GROUP BY BP.EName
ORDER BY SUM(I.TotalInvoice) DESC";

        private static string TopTenCustomersByRevenueSql() => $@"
SELECT TOP 10 ISNULL(BP.EName, N'Unknown') AS Name, SUM(q.Total) AS Total
FROM (
    SELECT TotalInvoice AS Total, BusinessPartnerID
    FROM tbl_InvoiceHeader
    WHERE {SalesInvoiceTypeFilter}
      AND {InvoiceCountedFilter}
      AND YEAR(InvoiceDate) = YEAR(GETDATE())
    UNION ALL
    SELECT TotalAmount AS Total, BusinessPartnerID
    FROM tbl_FinancingHeader
    WHERE LoanType <> 1 AND YEAR(VoucherDate) = YEAR(GETDATE())
) q
LEFT JOIN tbl_BusinessPartner BP ON BP.ID = q.BusinessPartnerID
GROUP BY BP.EName
ORDER BY SUM(q.Total) DESC";

        private static string BranchWiseSalesSql() => $@"
SELECT B.EName AS Name, SUM(I.TotalInvoice) AS Total
FROM tbl_InvoiceHeader I
JOIN tbl_Branch B ON I.BranchID = B.ID
WHERE I.{SalesInvoiceTypeFilter}
  AND I.{InvoiceCountedFilter}
  AND YEAR(I.InvoiceDate) = YEAR(GETDATE())
GROUP BY B.EName
ORDER BY SUM(I.TotalInvoice) DESC";

        private static string MonthlyExpenseTrendSql() => $@"
SELECT FORMAT(H.VoucherDate, 'yyyy-MM') AS Name, SUM(D.Debit) AS Total
FROM tbl_JournalVoucherDetails D
JOIN tbl_JournalVoucherHeader H ON D.ParentGuid = H.Guid
WHERE D.AccountID IN {OperatingExpenseAccounts}
  AND H.VoucherDate >= DATEADD(MONTH, -11, DATEFROMPARTS(YEAR(GETDATE()), MONTH(GETDATE()), 1))
GROUP BY FORMAT(H.VoucherDate, 'yyyy-MM')
ORDER BY Name";

        private static string TopProductsBySalesSql() => $@"
SELECT TOP 8 ISNULL(I.EName, N'Unknown') AS Name, SUM(D.TotalLine) AS Total
FROM tbl_InvoiceDetails D
INNER JOIN tbl_InvoiceHeader H ON H.Guid = D.HeaderGuid
LEFT JOIN tbl_Items I ON I.Guid = D.ItemGuid
WHERE H.{SalesInvoiceTypeFilter}
  AND H.{InvoiceCountedFilter}
  AND YEAR(H.InvoiceDate) = YEAR(GETDATE())
GROUP BY I.EName
ORDER BY SUM(D.TotalLine) DESC";

        private static string RecentSalesInvoicesSql() => $@"
SELECT TOP 10
    H.InvoiceNo,
    H.InvoiceDate,
    ISNULL(BP.EName, N'') AS Customer,
    H.TotalInvoice
FROM tbl_InvoiceHeader H
LEFT JOIN tbl_BusinessPartner BP ON BP.ID = H.BusinessPartnerID
WHERE H.{SalesInvoiceTypeFilter}
  AND H.{InvoiceCountedFilter}
ORDER BY H.InvoiceDate DESC";

        private static string MonthlyDebitVsCreditSql() => @"
SELECT FORMAT(H.VoucherDate, 'yyyy-MM') AS Name,
       SUM(D.Debit) AS Total,
       SUM(D.Credit) AS TotalCredit
FROM tbl_JournalVoucherDetails D
JOIN tbl_JournalVoucherHeader H ON D.ParentGuid = H.Guid
WHERE H.VoucherDate >= DATEADD(MONTH, -11, DATEFROMPARTS(YEAR(GETDATE()), MONTH(GETDATE()), 1))
GROUP BY FORMAT(H.VoucherDate, 'yyyy-MM')
ORDER BY Name";

        private static string CostCenterDebitSql() => @"
SELECT TOP 8 ISNULL(CC.EName, N'Unassigned') AS Name, SUM(D.Debit) AS Total
FROM tbl_JournalVoucherDetails D
LEFT JOIN tbl_CostCenter CC ON CC.ID = D.CostCenterID
WHERE D.Debit > 0
GROUP BY ISNULL(CC.EName, N'Unassigned')
ORDER BY SUM(D.Debit) DESC";

        private static string TopDebitAccountsSql() => @"
SELECT TOP 8 A.EName AS Name, SUM(D.Debit) AS Total
FROM tbl_JournalVoucherDetails D
JOIN tbl_Accounts A ON D.AccountID = A.ID
WHERE D.Debit > 0
GROUP BY A.EName
ORDER BY SUM(D.Debit) DESC";

        private static string TopCreditAccountsSql() => @"
SELECT TOP 8 A.EName AS Name, SUM(D.Credit) AS Total
FROM tbl_JournalVoucherDetails D
JOIN tbl_Accounts A ON D.AccountID = A.ID
WHERE D.Credit > 0
GROUP BY A.EName
ORDER BY SUM(D.Credit) DESC";

        private static string OutstandingInvoicesSql() => $@"
WITH ReconciledAmounts AS (
    SELECT JVDetailsGuid, SUM(Amount) AS Reconciled
    FROM tbl_Reconciliation
    GROUP BY JVDetailsGuid
),
AgingBuckets AS (
    SELECT
        CASE
            WHEN DATEDIFF(DAY, DueDate, GETDATE()) BETWEEN 0 AND 30 THEN '0-30 Days'
            WHEN DATEDIFF(DAY, DueDate, GETDATE()) BETWEEN 31 AND 60 THEN '31-60 Days'
            WHEN DATEDIFF(DAY, DueDate, GETDATE()) > 60 THEN '60+ Days'
        END AS Name,
        total - ISNULL(R.Reconciled, 0) AS Amount
    FROM tbl_JournalVoucherDetails JVD
    LEFT JOIN ReconciledAmounts R ON R.JVDetailsGuid = JVD.Guid
    WHERE DueDate <= GETDATE()
      AND SubAccountID > 0
      AND AccountID IN {ReceivableAccount}
)
SELECT Name, SUM(CASE WHEN Amount > 0 THEN Amount ELSE 0 END) AS Total
FROM AgingBuckets
WHERE Name IS NOT NULL
GROUP BY Name";

        private static string InventoryTurnoverSql() => $@"
SELECT
    (q.totalCost * -1) / NULLIF((q.totalStock + q.totalStockPastYear) / 2.0, 0) AS Total,
    CASE
        WHEN COALESCE(
            (q.totalCostPastYear * -1) / NULLIF((q.totalStockPastYear + q.totalStockPastYearOpenning) / 2.0, 0), 0) = 0
        THEN NULL
        ELSE (
            COALESCE(
                (q.totalCost * -1) / NULLIF((q.totalStock + q.totalStockPastYear) / 2.0, 0), 0)
            - COALESCE(
                (q.totalCostPastYear * -1) / NULLIF((q.totalStockPastYear + q.totalStockPastYearOpenning) / 2.0, 0), 0)
        ) * 100.0 / COALESCE(
            (q.totalCostPastYear * -1) / NULLIF((q.totalStockPastYear + q.totalStockPastYearOpenning) / 2.0, 0), 1)
    END AS PercentageChange
FROM (
    SELECT
        (SELECT ISNULL(SUM(Total * -1), 0) FROM tbl_JournalVoucherDetails
         WHERE AccountID IN {CogsAccount} AND YEAR(DueDate) = YEAR(GETDATE())) AS totalCost,
        (SELECT ISNULL(SUM(Total * -1), 0) FROM tbl_JournalVoucherDetails
         WHERE AccountID IN {CogsAccount}
           AND YEAR(DueDate) = YEAR(DATEADD(YEAR, -1, GETDATE()))) AS totalCostPastYear,
        (SELECT ISNULL(SUM(Total * -1), 0) FROM tbl_JournalVoucherDetails
         WHERE AccountID IN {InventoryAccount}) AS totalStock,
        (SELECT ISNULL(SUM(Total * -1), 0) FROM tbl_JournalVoucherDetails
         WHERE AccountID IN {InventoryAccount}
           AND YEAR(DueDate) <= YEAR(DATEADD(YEAR, -1, GETDATE()))) AS totalStockPastYear,
        (SELECT ISNULL(SUM(Total * -1), 0) FROM tbl_JournalVoucherDetails
         WHERE AccountID IN {InventoryAccount}
           AND YEAR(DueDate) <= YEAR(DATEADD(YEAR, -2, GETDATE()))) AS totalStockPastYearOpenning
) q";

        // ── Installment sales & cash loan SQL ─────────────────────────────────
        // Employee loans use FinancingHeader.LoanType = 1 — excluded from customer KPIs.
        // MainTypeID: 1/2 = cash loan module, 3 = given/installment credit (see loan type setup).

        private static string InstallmentSalesFilter => @"
FH.LoanType <> 1
AND (
    LT.MainTypeID = 3
    OR EXISTS (SELECT 1 FROM tbl_FinancingDetails FD WHERE FD.HeaderGuid = FH.Guid)
)";

        private static string CashLoansFilter => @"
FH.LoanType <> 1
AND LT.MainTypeID IN (1, 2)
AND NOT (LT.MainTypeID = 3)";

        private static string CustomerFinancingFilter => @"FH.LoanType <> 1";

        private static string FinancingJvGuidsSubquery => @"
(
    SELECT FD.JVGuid FROM tbl_FinancingDetails FD
    INNER JOIN tbl_FinancingHeader FH ON FH.Guid = FD.HeaderGuid
    WHERE FD.JVGuid IS NOT NULL AND FH.LoanType <> 1
    UNION
    SELECT FH.JVGuid FROM tbl_FinancingHeader FH
    WHERE FH.JVGuid IS NOT NULL AND FH.LoanType <> 1
)";

        private static string MtdInstallmentSalesSql() => $@"
SELECT ISNULL(SUM(
    CASE
        WHEN FD.TotalAmountWithInterest IS NOT NULL AND FD.TotalAmountWithInterest > 0
            THEN FD.TotalAmountWithInterest
        ELSE ISNULL(FD.TotalAmount, 0)
    END), 0) AS Total
FROM tbl_FinancingDetails FD
INNER JOIN tbl_FinancingHeader FH ON FH.Guid = FD.HeaderGuid
INNER JOIN tbl_LoanTypes LT ON LT.ID = FH.LoanType
WHERE {InstallmentSalesFilter}
  AND YEAR(FH.VoucherDate) = YEAR(GETDATE())
  AND MONTH(FH.VoucherDate) = MONTH(GETDATE())";

        private static string YtdInstallmentSalesSql() => $@"
SELECT ISNULL(SUM(
    CASE
        WHEN FD.TotalAmountWithInterest IS NOT NULL AND FD.TotalAmountWithInterest > 0
            THEN FD.TotalAmountWithInterest
        ELSE ISNULL(FD.TotalAmount, 0)
    END), 0) AS Total
FROM tbl_FinancingDetails FD
INNER JOIN tbl_FinancingHeader FH ON FH.Guid = FD.HeaderGuid
INNER JOIN tbl_LoanTypes LT ON LT.ID = FH.LoanType
WHERE {InstallmentSalesFilter}
  AND YEAR(FH.VoucherDate) = YEAR(GETDATE())";

        private static string ActiveInstallmentContractsSql() => $@"
SELECT COUNT(DISTINCT FH.Guid) AS Total
FROM tbl_FinancingHeader FH
INNER JOIN tbl_LoanTypes LT ON LT.ID = FH.LoanType
WHERE {InstallmentSalesFilter}";

        private static string MtdCashLoansDisbursedSql() => $@"
SELECT ISNULL(SUM(FH.TotalAmount), 0) AS Total
FROM tbl_FinancingHeader FH
INNER JOIN tbl_LoanTypes LT ON LT.ID = FH.LoanType
WHERE {CashLoansFilter}
  AND YEAR(FH.VoucherDate) = YEAR(GETDATE())
  AND MONTH(FH.VoucherDate) = MONTH(GETDATE())";

        private static string YtdCashLoansDisbursedSql() => $@"
SELECT ISNULL(SUM(FH.TotalAmount), 0) AS Total
FROM tbl_FinancingHeader FH
INNER JOIN tbl_LoanTypes LT ON LT.ID = FH.LoanType
WHERE {CashLoansFilter}
  AND YEAR(FH.VoucherDate) = YEAR(GETDATE())";

        private static string ActiveCashLoansSql() => $@"
SELECT COUNT(*) AS Total
FROM tbl_FinancingHeader FH
INNER JOIN tbl_LoanTypes LT ON LT.ID = FH.LoanType
WHERE {CashLoansFilter}";

        private static string OutstandingCashLoanBalanceSql() => $@"
WITH ReconciledAmounts AS (
    SELECT JVDetailsGuid, SUM(Amount) AS Reconciled
    FROM tbl_Reconciliation
    GROUP BY JVDetailsGuid
)
SELECT ISNULL(SUM(
    CASE WHEN JVD.Debit - ISNULL(R.Reconciled, 0) > 0
         THEN JVD.Debit - ISNULL(R.Reconciled, 0) ELSE 0 END), 0) AS Total
FROM tbl_JournalVoucherDetails JVD
LEFT JOIN ReconciledAmounts R ON R.JVDetailsGuid = JVD.Guid
INNER JOIN tbl_FinancingHeader FH ON FH.JVGuid = JVD.ParentGuid
INNER JOIN tbl_LoanTypes LT ON LT.ID = FH.LoanType
WHERE JVD.Debit > 0
  AND {CashLoansFilter}";

        private static string InstallmentsDueThisMonthSql() => $@"
SELECT ISNULL(SUM(JVD.Debit), 0) AS Total
FROM tbl_JournalVoucherDetails JVD
WHERE JVD.Debit > 0
  AND YEAR(JVD.DueDate) = YEAR(GETDATE())
  AND MONTH(JVD.DueDate) = MONTH(GETDATE())
  AND JVD.ParentGuid IN {FinancingJvGuidsSubquery}";

        private static string OverdueInstallmentsSql() => $@"
WITH ReconciledAmounts AS (
    SELECT JVDetailsGuid, SUM(Amount) AS Reconciled
    FROM tbl_Reconciliation
    GROUP BY JVDetailsGuid
)
SELECT ISNULL(SUM(
    CASE WHEN JVD.Debit - ISNULL(R.Reconciled, 0) > 0
         THEN JVD.Debit - ISNULL(R.Reconciled, 0) ELSE 0 END), 0) AS Total
FROM tbl_JournalVoucherDetails JVD
LEFT JOIN ReconciledAmounts R ON R.JVDetailsGuid = JVD.Guid
WHERE JVD.Debit > 0
  AND JVD.DueDate < CAST(GETDATE() AS DATE)
  AND JVD.ParentGuid IN {FinancingJvGuidsSubquery}";

        private static string MonthlyInstallmentSalesTrendSql() => $@"
SELECT FORMAT(FH.VoucherDate, 'yyyy-MM') AS Name,
       ISNULL(SUM(
           CASE
               WHEN FD.TotalAmountWithInterest IS NOT NULL AND FD.TotalAmountWithInterest > 0
                   THEN FD.TotalAmountWithInterest
               ELSE ISNULL(FD.TotalAmount, 0)
           END), 0) AS Total
FROM tbl_FinancingDetails FD
INNER JOIN tbl_FinancingHeader FH ON FH.Guid = FD.HeaderGuid
INNER JOIN tbl_LoanTypes LT ON LT.ID = FH.LoanType
WHERE {InstallmentSalesFilter}
  AND FH.VoucherDate >= DATEADD(MONTH, -11, DATEFROMPARTS(YEAR(GETDATE()), MONTH(GETDATE()), 1))
GROUP BY FORMAT(FH.VoucherDate, 'yyyy-MM')
ORDER BY Name";

        private static string MonthlyCashLoansTrendSql() => $@"
SELECT FORMAT(FH.VoucherDate, 'yyyy-MM') AS Name,
       ISNULL(SUM(FH.TotalAmount), 0) AS Total
FROM tbl_FinancingHeader FH
INNER JOIN tbl_LoanTypes LT ON LT.ID = FH.LoanType
WHERE {CashLoansFilter}
  AND FH.VoucherDate >= DATEADD(MONTH, -11, DATEFROMPARTS(YEAR(GETDATE()), MONTH(GETDATE()), 1))
GROUP BY FORMAT(FH.VoucherDate, 'yyyy-MM')
ORDER BY Name";

        private static string InstallmentVsCashLoansTrendSql() => @"
SELECT q.Name,
       ISNULL(SUM(q.InstallmentTotal), 0) AS Total,
       ISNULL(SUM(q.CashTotal), 0) AS TotalCredit
FROM (
    SELECT FORMAT(FH.VoucherDate, 'yyyy-MM') AS Name,
           CASE
               WHEN LT.MainTypeID = 3 OR EXISTS (
                   SELECT 1 FROM tbl_FinancingDetails FD2 WHERE FD2.HeaderGuid = FH.Guid)
               THEN ISNULL(FD.TotalAmountWithInterest, ISNULL(FD.TotalAmount, FH.TotalAmount))
               ELSE 0
           END AS InstallmentTotal,
           CASE
               WHEN LT.MainTypeID IN (1, 2) AND LT.MainTypeID <> 3
               THEN ISNULL(FH.TotalAmount, 0)
               ELSE 0
           END AS CashTotal
    FROM tbl_FinancingHeader FH
    INNER JOIN tbl_LoanTypes LT ON LT.ID = FH.LoanType
    LEFT JOIN tbl_FinancingDetails FD ON FD.HeaderGuid = FH.Guid
    WHERE FH.LoanType <> 1
      AND FH.VoucherDate >= DATEADD(MONTH, -11, DATEFROMPARTS(YEAR(GETDATE()), MONTH(GETDATE()), 1))
) q
GROUP BY q.Name
ORDER BY q.Name";

        private static string FinancingByLoanTypeSql() => $@"
SELECT ISNULL(LT.AName, N'Unknown') AS Name,
       ISNULL(SUM(FH.TotalAmount), 0) AS Total
FROM tbl_FinancingHeader FH
INNER JOIN tbl_LoanTypes LT ON LT.ID = FH.LoanType
WHERE {CustomerFinancingFilter}
  AND YEAR(FH.VoucherDate) = YEAR(GETDATE())
GROUP BY LT.AName
ORDER BY SUM(FH.TotalAmount) DESC";

        private static string TopCustomersByFinancingSql() => $@"
SELECT TOP 8 ISNULL(BP.EName, N'Unknown') AS Name,
       ISNULL(SUM(FH.TotalAmount), 0) AS Total
FROM tbl_FinancingHeader FH
INNER JOIN tbl_LoanTypes LT ON LT.ID = FH.LoanType
LEFT JOIN tbl_BusinessPartner BP ON BP.ID = FH.BusinessPartnerID
WHERE {CustomerFinancingFilter}
  AND YEAR(FH.VoucherDate) = YEAR(GETDATE())
GROUP BY BP.EName
ORDER BY SUM(FH.TotalAmount) DESC";

        private static string RecentFinancingContractsSql() => $@"
SELECT TOP 10
    FH.VoucherNumber,
    FH.VoucherDate,
    ISNULL(BP.EName, N'') AS Customer,
    ISNULL(LT.AName, N'') AS LoanType,
    FH.TotalAmount
FROM tbl_FinancingHeader FH
INNER JOIN tbl_LoanTypes LT ON LT.ID = FH.LoanType
LEFT JOIN tbl_BusinessPartner BP ON BP.ID = FH.BusinessPartnerID
WHERE {CustomerFinancingFilter}
ORDER BY FH.VoucherDate DESC";

        // ── Advanced partner & open/settled analytics ─────────────────────────

        private static string ReconciledAmountsCte => @"
ReconciledAmounts AS (
    SELECT JVDetailsGuid, SUM(Amount) AS Reconciled
    FROM tbl_Reconciliation
    GROUP BY JVDetailsGuid
)";

        private static string FinancingContractOutstandingCte(string headerFilter) => $@"
WITH {ReconciledAmountsCte},
FinancingInstallmentLines AS (
    SELECT FH.Guid AS HeaderGuid, JVD.Debit, ISNULL(R.Reconciled, 0) AS Reconciled
    FROM tbl_FinancingHeader FH
    INNER JOIN tbl_LoanTypes LT ON LT.ID = FH.LoanType
    INNER JOIN tbl_FinancingDetails FD ON FD.HeaderGuid = FH.Guid
    INNER JOIN tbl_JournalVoucherDetails JVD ON JVD.ParentGuid = FD.JVGuid AND JVD.Debit > 0
    LEFT JOIN ReconciledAmounts R ON R.JVDetailsGuid = JVD.Guid
    WHERE {headerFilter}
    UNION ALL
    SELECT FH.Guid, JVD.Debit, ISNULL(R.Reconciled, 0)
    FROM tbl_FinancingHeader FH
    INNER JOIN tbl_LoanTypes LT ON LT.ID = FH.LoanType
    INNER JOIN tbl_JournalVoucherDetails JVD ON JVD.ParentGuid = FH.JVGuid AND JVD.Debit > 0
    LEFT JOIN ReconciledAmounts R ON R.JVDetailsGuid = JVD.Guid
    WHERE {headerFilter}
      AND NOT EXISTS (SELECT 1 FROM tbl_FinancingDetails FD WHERE FD.HeaderGuid = FH.Guid)
),
ContractOutstanding AS (
    SELECT HeaderGuid,
           SUM(CASE WHEN Debit > Reconciled THEN Debit - Reconciled ELSE 0 END) AS Outstanding
    FROM FinancingInstallmentLines
    GROUP BY HeaderGuid
)";

        private static string ReturningCustomerRateSql() => $@"
SELECT CASE
    WHEN q.TotalCustomers = 0 THEN 0
    ELSE CAST(q.ReturningCustomers AS DECIMAL(18, 4)) * 100.0 / q.TotalCustomers
END AS Total
FROM (
    SELECT
        COUNT(*) AS TotalCustomers,
        SUM(CASE WHEN TxCount >= 2 THEN 1 ELSE 0 END) AS ReturningCustomers
    FROM (
        SELECT BusinessPartnerID, SUM(TxCount) AS TxCount
        FROM (
            SELECT BusinessPartnerID, COUNT(*) AS TxCount
            FROM tbl_InvoiceHeader
            WHERE {SalesInvoiceTypeFilter}
              AND {InvoiceCountedFilter}
              AND BusinessPartnerID > 0
              AND YEAR(InvoiceDate) = YEAR(GETDATE())
            GROUP BY BusinessPartnerID
            UNION ALL
            SELECT BusinessPartnerID, COUNT(*) AS TxCount
            FROM tbl_FinancingHeader
            WHERE LoanType <> 1
              AND BusinessPartnerID > 0
              AND YEAR(VoucherDate) = YEAR(GETDATE())
            GROUP BY BusinessPartnerID
        ) u
        GROUP BY BusinessPartnerID
    ) x
) q";

        private static string RepeatFinancingCustomerRateSql() => $@"
SELECT CASE
    WHEN q.TotalCustomers = 0 THEN 0
    ELSE CAST(q.RepeatCustomers AS DECIMAL(18, 4)) * 100.0 / q.TotalCustomers
END AS Total
FROM (
    SELECT
        COUNT(*) AS TotalCustomers,
        SUM(CASE WHEN ContractCount >= 2 THEN 1 ELSE 0 END) AS RepeatCustomers
    FROM (
        SELECT FH.BusinessPartnerID, COUNT(*) AS ContractCount
        FROM tbl_FinancingHeader FH
        WHERE {CustomerFinancingFilter}
          AND FH.BusinessPartnerID > 0
          AND YEAR(FH.VoucherDate) = YEAR(GETDATE())
        GROUP BY FH.BusinessPartnerID
    ) x
) q";

        private static string TopCustomersBySalesSql() => $@"
SELECT TOP 8 ISNULL(BP.EName, N'Unknown') AS Name, SUM(I.TotalInvoice) AS Total
FROM tbl_InvoiceHeader I
LEFT JOIN tbl_BusinessPartner BP ON BP.ID = I.BusinessPartnerID
WHERE I.{SalesInvoiceTypeFilter}
  AND I.{InvoiceCountedFilter}
  AND YEAR(I.InvoiceDate) = YEAR(GETDATE())
GROUP BY BP.EName
ORDER BY SUM(I.TotalInvoice) DESC";

        private static string TopVendorsByPurchasesSql() => $@"
SELECT TOP 8 ISNULL(BP.EName, N'Unknown') AS Name, SUM(I.TotalInvoice) AS Total
FROM tbl_InvoiceHeader I
LEFT JOIN tbl_BusinessPartner BP ON BP.ID = I.BusinessPartnerID
WHERE I.{PurchaseInvoiceTypeFilter}
  AND I.{InvoiceCountedFilter}
  AND YEAR(I.InvoiceDate) = YEAR(GETDATE())
GROUP BY BP.EName
ORDER BY SUM(I.TotalInvoice) DESC";

        private static string SalesVsPurchasesByCustomerSql() => $@"
SELECT TOP 8 ISNULL(BP.EName, N'Unknown') AS Name,
       ISNULL(SUM(CASE WHEN I.{SalesInvoiceTypeFilter} THEN I.TotalInvoice ELSE 0 END), 0) AS Total,
       ISNULL(SUM(CASE WHEN I.{PurchaseInvoiceTypeFilter} THEN I.TotalInvoice ELSE 0 END), 0) AS TotalCredit
FROM tbl_InvoiceHeader I
LEFT JOIN tbl_BusinessPartner BP ON BP.ID = I.BusinessPartnerID
WHERE I.{InvoiceCountedFilter}
  AND I.BusinessPartnerID > 0
  AND (I.{SalesInvoiceTypeFilter} OR I.{PurchaseInvoiceTypeFilter})
  AND YEAR(I.InvoiceDate) = YEAR(GETDATE())
GROUP BY BP.EName
HAVING SUM(CASE WHEN I.{SalesInvoiceTypeFilter} THEN I.TotalInvoice ELSE 0 END) > 0
    OR SUM(CASE WHEN I.{PurchaseInvoiceTypeFilter} THEN I.TotalInvoice ELSE 0 END) > 0
ORDER BY SUM(I.TotalInvoice) DESC";

        private static string OpenFinancingContractsSql() => $@"
{FinancingContractOutstandingCte(CustomerFinancingFilter)}
SELECT COUNT(*) AS Total
FROM ContractOutstanding
WHERE Outstanding > 0.01";

        private static string SettledFinancingContractsSql() => $@"
{FinancingContractOutstandingCte(CustomerFinancingFilter)}
SELECT COUNT(*) AS Total
FROM ContractOutstanding
WHERE Outstanding <= 0.01";

        private static string OpenVsSettledFinancingSql() => $@"
{FinancingContractOutstandingCte(CustomerFinancingFilter)}
SELECT N'Open' AS Name, COUNT(*) AS Total
FROM ContractOutstanding
WHERE Outstanding > 0.01
UNION ALL
SELECT N'Settled' AS Name, COUNT(*) AS Total
FROM ContractOutstanding
WHERE Outstanding <= 0.01";

        private static string OpenVsSettledInstallmentContractsSql() => $@"
{FinancingContractOutstandingCte(InstallmentSalesFilter)}
SELECT N'Open' AS Name, COUNT(*) AS Total
FROM ContractOutstanding
WHERE Outstanding > 0.01
UNION ALL
SELECT N'Settled' AS Name, COUNT(*) AS Total
FROM ContractOutstanding
WHERE Outstanding <= 0.01";

        private static string OpenVsSettledSalesInvoicesSql() => $@"
SELECT N'Open' AS Name, COUNT(*) AS Total
FROM tbl_InvoiceHeader
WHERE {SalesInvoiceTypeFilter}
  AND {InvoiceCountedFilter}
  AND ISNULL(Status, 0) = 0
  AND YEAR(InvoiceDate) = YEAR(GETDATE())
UNION ALL
SELECT N'Settled' AS Name, COUNT(*) AS Total
FROM tbl_InvoiceHeader
WHERE {SalesInvoiceTypeFilter}
  AND {InvoiceCountedFilter}
  AND ISNULL(Status, 0) <> 0
  AND YEAR(InvoiceDate) = YEAR(GETDATE())";

        private static string SettledSalesInvoicesSql() => $@"
SELECT COUNT(*) AS Total
FROM tbl_InvoiceHeader
WHERE {SalesInvoiceTypeFilter}
  AND {InvoiceCountedFilter}
  AND ISNULL(Status, 0) <> 0
  AND YEAR(InvoiceDate) = YEAR(GETDATE())";

        private static string InstallmentCollectionRateSql() => $@"
WITH {ReconciledAmountsCte},
InstallmentLines AS (
    SELECT JVD.Debit, ISNULL(R.Reconciled, 0) AS Collected
    FROM tbl_JournalVoucherDetails JVD
    LEFT JOIN ReconciledAmounts R ON R.JVDetailsGuid = JVD.Guid
    WHERE JVD.Debit > 0
      AND JVD.ParentGuid IN {FinancingJvGuidsSubquery}
      AND YEAR(JVD.DueDate) = YEAR(GETDATE())
)
SELECT CASE
    WHEN SUM(Debit) = 0 THEN 0
    ELSE CAST(SUM(Collected) AS DECIMAL(18, 4)) * 100.0 / SUM(Debit)
END AS Total
FROM InstallmentLines";

        private static string OpenInstallmentBalanceSql() => $@"
WITH {ReconciledAmountsCte}
SELECT ISNULL(SUM(
    CASE WHEN JVD.Debit - ISNULL(R.Reconciled, 0) > 0
         THEN JVD.Debit - ISNULL(R.Reconciled, 0) ELSE 0 END), 0) AS Total
FROM tbl_JournalVoucherDetails JVD
LEFT JOIN ReconciledAmounts R ON R.JVDetailsGuid = JVD.Guid
WHERE JVD.Debit > 0
  AND JVD.ParentGuid IN {FinancingJvGuidsSubquery}";
    }
}
