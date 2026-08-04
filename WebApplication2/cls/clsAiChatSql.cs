using System;
using System.Text;

namespace WebApplication2.cls
{
    /// <summary>
    /// SQL query templates built from clsAiChatSchema.
    /// All AI chat financial queries should go through here.
    /// </summary>
    public static class clsAiChatSql
    {
        private static string T(string key) => clsAiChatSchema.T(key);
        private static string C(string table, string col) => clsAiChatSchema.C(table, col);
        private static string Q(string table, string col, string alias) => clsAiChatSchema.Qualify(table, col, alias);

        // ── Master data counts ──────────────────────────────────────────────

        public static string CountInactiveCustomers(int type = 1) =>
            $"SELECT COUNT(*) AS Total FROM {T("BusinessPartner")} WHERE {C("BusinessPartner", "Active")} = 0 AND {C("BusinessPartner", "Type")} = {type}";

        public static string CountBusinessPartners(bool activeOnly = true, int? type = null)
        {
            var sb = new StringBuilder($"SELECT COUNT(*) AS Total FROM {T("BusinessPartner")} WHERE 1=1");
            if (activeOnly)
                sb.Append($" AND {C("BusinessPartner", "Active")} = 1");
            if (type.HasValue)
                sb.Append($" AND {C("BusinessPartner", "Type")} = {type.Value}");
            return sb.ToString();
        }

        public static string CountTable(string tableKey) =>
            $"SELECT COUNT(*) AS Total FROM {T(tableKey)}";

        // ── Invoice / sales ─────────────────────────────────────────────────

        public static string SumInvoiceTotal(string extraWhere = null)
        {
            string col = C("InvoiceHeader", "TotalInvoice");
            var sb = new StringBuilder($"SELECT ISNULL(SUM({col}), 0) AS Total FROM {T("InvoiceHeader")}");
            if (!string.IsNullOrWhiteSpace(extraWhere))
                sb.Append(" WHERE ").Append(extraWhere);
            return sb.ToString();
        }

        public static string SumSalesTotal(string extraWhere = null) =>
            SumInvoiceTotalByTypes(clsAiChatSchema.DocTypes.Sales, extraWhere);

        public static string SumPurchaseTotal(string extraWhere = null) =>
            SumInvoiceTotalByTypes(clsAiChatSchema.DocTypes.Purchases, extraWhere);

        public static string SumInvoiceTotalByTypes(int[] typeIds, string extraWhere = null)
        {
            const string h = "H";
            string typeCol = Q("InvoiceHeader", "TypeId", h);
            string totalCol = Q("InvoiceHeader", "TotalInvoice", h);
            var sb = new StringBuilder(
                $"SELECT ISNULL(SUM({totalCol}), 0) AS Total FROM {T("InvoiceHeader")} {h} WHERE {typeCol} IN ({clsAiChatSchema.DocTypes.InClause(typeIds)})");
            if (!string.IsNullOrWhiteSpace(extraWhere))
                sb.Append(" AND ").Append(extraWhere.Replace("InvoiceDate", Q("InvoiceHeader", "Date", h)));
            return sb.ToString();
        }

        public static string CountInvoices(string extraWhere = null)
        {
            var sb = new StringBuilder($"SELECT COUNT(*) AS Total FROM {T("InvoiceHeader")}");
            if (!string.IsNullOrWhiteSpace(extraWhere))
                sb.Append(" WHERE ").Append(extraWhere);
            return sb.ToString();
        }

        public static string TopCustomersBySales(int top = 5) =>
            $@"SELECT TOP {top} BP.{C("BusinessPartner", "NameEn")} AS Name,
                      ISNULL(SUM(I.{C("InvoiceHeader", "TotalInvoice")}), 0) AS Total
               FROM {T("InvoiceHeader")} I
               INNER JOIN {T("BusinessPartner")} BP ON BP.{C("BusinessPartner", "Pk")} = I.{C("InvoiceHeader", "PartnerId")}
               WHERE I.{C("InvoiceHeader", "TypeId")} IN ({clsAiChatSchema.DocTypes.InClause(clsAiChatSchema.DocTypes.Sales)})
               GROUP BY BP.{C("BusinessPartner", "NameEn")}
               ORDER BY Total DESC";

        public static string TopVendorsByPurchases(int top = 5) =>
            $@"SELECT TOP {top} BP.{C("BusinessPartner", "NameEn")} AS Name,
                      ISNULL(SUM(I.{C("InvoiceHeader", "TotalInvoice")}), 0) AS Total
               FROM {T("InvoiceHeader")} I
               INNER JOIN {T("BusinessPartner")} BP ON BP.{C("BusinessPartner", "Pk")} = I.{C("InvoiceHeader", "PartnerId")}
               WHERE I.{C("InvoiceHeader", "TypeId")} IN ({clsAiChatSchema.DocTypes.InClause(clsAiChatSchema.DocTypes.Purchases)})
               GROUP BY BP.{C("BusinessPartner", "NameEn")}
               ORDER BY Total DESC";

        public static string RecentInvoices(int top = 5) =>
            $@"SELECT TOP {top} {C("InvoiceHeader", "Number")} AS Name,
                      {C("InvoiceHeader", "TotalInvoice")} AS Total,
                      {C("InvoiceHeader", "Date")} AS DateValue
               FROM {T("InvoiceHeader")}
               ORDER BY {C("InvoiceHeader", "Date")} DESC";

        public static string MonthlySalesTrend(int months = 6) =>
            $@"SELECT TOP {months} FORMAT({C("InvoiceHeader", "Date")}, 'yyyy-MM') AS Name,
                      ISNULL(SUM({C("InvoiceHeader", "TotalInvoice")}), 0) AS Total
               FROM {T("InvoiceHeader")}
               WHERE {C("InvoiceHeader", "TypeId")} IN ({clsAiChatSchema.DocTypes.InClause(clsAiChatSchema.DocTypes.Sales)})
               GROUP BY FORMAT({C("InvoiceHeader", "Date")}, 'yyyy-MM')
               ORDER BY Name DESC";

        public static string SalesVsPurchases() =>
            $@"SELECT
                ISNULL((SELECT SUM({C("InvoiceHeader", "TotalInvoice")}) FROM {T("InvoiceHeader")}
                        WHERE {C("InvoiceHeader", "TypeId")} IN ({clsAiChatSchema.DocTypes.InClause(clsAiChatSchema.DocTypes.Sales)})), 0) AS Sales,
                ISNULL((SELECT SUM({C("InvoiceHeader", "TotalInvoice")}) FROM {T("InvoiceHeader")}
                        WHERE {C("InvoiceHeader", "TypeId")} IN ({clsAiChatSchema.DocTypes.InClause(clsAiChatSchema.DocTypes.Purchases)})), 0) AS Purchases";

        public static string TopItemsByQty(int top = 5) =>
            $@"SELECT TOP {top} I.{C("Items", "NameEn")} AS Name, ISNULL(SUM(D.{C("InvoiceDetail", "Qty")}), 0) AS Total
               FROM {T("InvoiceDetail")} D
               INNER JOIN {T("Items")} I ON I.{C("Items", "Pk")} = D.{C("InvoiceDetail", "ItemGuid")}
               GROUP BY I.{C("Items", "NameEn")}
               ORDER BY Total DESC";

        public static string AvgInvoiceAmount() =>
            $@"SELECT ISNULL(AVG({C("InvoiceHeader", "TotalInvoice")}), 0) AS Total
               FROM {T("InvoiceHeader")}
               WHERE {C("InvoiceHeader", "TotalInvoice")} > 0";

        // ── Journal / GL balances (source of truth) ─────────────────────────

        public static string TotalDebit() =>
            $"SELECT ISNULL(SUM({C("JvDetail", "Debit")}), 0) AS Total FROM {T("JvDetail")}";

        public static string TotalCredit() =>
            $"SELECT ISNULL(SUM({C("JvDetail", "Credit")}), 0) AS Total FROM {T("JvDetail")}";

        public static string CountJournalVouchers() =>
            $"SELECT COUNT(*) AS Total FROM {T("JvHeader")}";

        public static string GlAccountBalanceSimple() =>
            $@"SELECT {clsAiChatSchema.GlBalanceExpression("d")} AS Balance
               FROM {T("JvDetail")} d
               WHERE d.{C("JvDetail", "AccountId")} = @AccountId
                 AND (d.{C("JvDetail", "CompanyId")} = @CompanyId OR @CompanyId = 0)";

        public static string PartnerSubLedgerBalance(int accountRefId)
        {
            const string d = "d";
            return $@"SELECT {clsAiChatSchema.GlBalanceExpression(d)} AS Balance
                      FROM {T("JvDetail")} {d}
                      WHERE {d}.{C("JvDetail", "AccountId")} = (
                          SELECT TOP 1 {C("AccountSetting", "AccountId")}
                          FROM {T("AccountSetting")}
                          WHERE {C("AccountSetting", "AccountRefId")} = {accountRefId}
                            AND ({C("AccountSetting", "CompanyId")} = @CompanyId OR @CompanyId = 0)
                      )
                      AND {d}.{C("JvDetail", "SubAccountId")} = @PartnerId
                      AND ({d}.{C("JvDetail", "CompanyId")} = @CompanyId OR @CompanyId = 0)";
        }

        public static string RoleAccountBalance(int accountRefId)
        {
            const string d = "d";
            return $@"SELECT {clsAiChatSchema.GlBalanceExpression(d)} AS Balance
                      FROM {T("JvDetail")} {d}
                      WHERE {d}.{C("JvDetail", "AccountId")} = (
                          SELECT TOP 1 {C("AccountSetting", "AccountId")}
                          FROM {T("AccountSetting")}
                          WHERE {C("AccountSetting", "AccountRefId")} = {accountRefId}
                            AND ({C("AccountSetting", "CompanyId")} = @CompanyId OR @CompanyId = 0)
                      )
                      AND ({d}.{C("JvDetail", "CompanyId")} = @CompanyId OR @CompanyId = 0)";
        }

        public static string OpenReceivableBalance()
        {
            const string d = "d";
            const string r = "r";
            return $@"SELECT ISNULL(SUM({d}.{C("JvDetail", "Debit")} - ISNULL({r}.{C("Reconciliation", "Amount")}, 0)), 0) AS Total
                      FROM {T("JvDetail")} {d}
                      LEFT JOIN (
                          SELECT {C("Reconciliation", "JvDetailFk")}, SUM({C("Reconciliation", "Amount")}) AS {C("Reconciliation", "Amount")}
                          FROM {T("Reconciliation")}
                          GROUP BY {C("Reconciliation", "JvDetailFk")}
                      ) {r} ON {r}.{C("Reconciliation", "JvDetailFk")} = {d}.{C("JvDetail", "Pk")}
                      WHERE {d}.{C("JvDetail", "AccountId")} = (
                          SELECT TOP 1 {C("AccountSetting", "AccountId")}
                          FROM {T("AccountSetting")}
                          WHERE {C("AccountSetting", "AccountRefId")} = {clsAiChatSchema.AccountRefs.Customer}
                      )
                      AND {d}.{C("JvDetail", "SubAccountId")} > 0
                      AND {d}.{C("JvDetail", "Debit")} > ISNULL({r}.{C("Reconciliation", "Amount")}, 0)";
        }

        public static string TotalReconciledAmount() =>
            $"SELECT ISNULL(SUM({C("Reconciliation", "Amount")}), 0) AS Total FROM {T("Reconciliation")}";

        // ── Financing / loans ───────────────────────────────────────────────

        public static string CountFinancingDocuments() =>
            $"SELECT COUNT(*) AS Total FROM {T("FinancingHeader")}";

        public static string SumFinancingTotal() =>
            $"SELECT ISNULL(SUM({C("FinancingHeader", "TotalAmount")}), 0) AS Total FROM {T("FinancingHeader")}";

        public static string RecentFinancing(int top = 5) =>
            $@"SELECT TOP {top} {C("FinancingHeader", "Number")} AS Name,
                      {C("FinancingHeader", "TotalAmount")} AS Total,
                      {C("FinancingHeader", "Date")} AS DateValue
               FROM {T("FinancingHeader")}
               ORDER BY {C("FinancingHeader", "Date")} DESC";

        public static string CountFinancingInstallments() =>
            $"SELECT COUNT(*) AS Total FROM {T("FinancingDetail")}";

        // ── Cash vouchers ───────────────────────────────────────────────────

        public static string CountCashVouchers() =>
            $"SELECT COUNT(*) AS Total FROM {T("CashVoucherHeader")}";

        // ── Master data search ──────────────────────────────────────────────

        public static string LoadAccountById() =>
            $@"SELECT {C("Accounts", "Pk")} AS ID, {C("Accounts", "Number")} AS AccountNumber,
                      {C("Accounts", "NameAr")} AS AName, {C("Accounts", "NameEn")} AS EName,
                      {C("Accounts", "NatureId")} AS AccountNatureID
               FROM {T("Accounts")} WHERE {C("Accounts", "Pk")} = @Id";

        public static string LoadPartnerById() =>
            $@"SELECT {C("BusinessPartner", "Pk")} AS ID, {C("BusinessPartner", "NameEn")} AS EName,
                      {C("BusinessPartner", "NameAr")} AS AName, {C("BusinessPartner", "Tel")} AS Tel,
                      {C("BusinessPartner", "Email")} AS Email, {C("BusinessPartner", "Type")} AS Type,
                      {C("BusinessPartner", "CommercialName")} AS CommercialName
               FROM {T("BusinessPartner")} WHERE {C("BusinessPartner", "Pk")} = @Id AND {C("BusinessPartner", "Active")} = 1";

        // ── Date filters (helpers) ──────────────────────────────────────────

        public const string FilterThisMonth = "YEAR(InvoiceDate) = YEAR(GETDATE()) AND MONTH(InvoiceDate) = MONTH(GETDATE())";
        public const string FilterToday = "CAST(InvoiceDate AS date) = CAST(GETDATE() AS date)";
        public const string FilterPendingInvoices = "Status = 0";

        public static string FilterThisMonthOn(string dateColumn) =>
            $"YEAR({dateColumn}) = YEAR(GETDATE()) AND MONTH({dateColumn}) = MONTH(GETDATE())";

        public static string FilterTodayOn(string dateColumn) =>
            $"CAST({dateColumn} AS date) = CAST(GETDATE() AS date)";
    }
}
