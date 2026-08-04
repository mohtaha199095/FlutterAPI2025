using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Microsoft.Data.SqlClient;
using WebApplication2.MainClasses;

namespace WebApplication2.cls
{
    public class clsAiChat
    {
        private sealed class ChatIntent
        {
            public string Id { get; init; } = "";
            public string[] Keywords { get; init; } = Array.Empty<string>();
            public string Sql { get; init; } = "";
            public string ResponseTemplateEn { get; init; } = "";
            public string ResponseTemplateAr { get; init; } = "";
            public bool IsList { get; init; }
        }

        private static readonly ChatIntent[] Intents = BuildIntents();

        private static ChatIntent[] BuildIntents() => new[]
        {
            new ChatIntent
            {
                Id = "customers_count",
                Keywords = new[] { "customer", "customers", "client", "clients", "business partner", "عملاء", "عميل", "زبائن", "زبون", "cliente", "clientes", "kunde", "kunden" },
                Sql = clsAiChatSql.CountBusinessPartners(activeOnly: true, type: 1),
                ResponseTemplateEn = "You have {0} active customers.",
                ResponseTemplateAr = "لديك {0} عميل نشط.",
            },
            new ChatIntent
            {
                Id = "vendors_count",
                Keywords = new[] { "vendor", "vendors", "supplier", "suppliers", "مورد", "موردين", "موردون" },
                Sql = clsAiChatSql.CountBusinessPartners(activeOnly: true, type: 2),
                ResponseTemplateEn = "You have {0} active vendors.",
                ResponseTemplateAr = "لديك {0} مورد نشط.",
            },
            new ChatIntent
            {
                Id = "employees_count",
                Keywords = new[] { "employee", "employees", "staff", "موظف", "موظفين", "الموظفين" },
                Sql = clsAiChatSql.CountTable("Employee"),
                ResponseTemplateEn = "You have {0} active employees.",
                ResponseTemplateAr = "لديك {0} موظف نشط.",
            },
            new ChatIntent
            {
                Id = "products_count",
                Keywords = new[] { "product", "products", "item", "items", "inventory", "منتج", "منتجات", "أصناف", "اصناف", "مخزون" },
                Sql = clsAiChatSql.CountTable("Items"),
                ResponseTemplateEn = "You have {0} active products/items.",
                ResponseTemplateAr = "لديك {0} صنف/منتج نشط.",
            },
            new ChatIntent
            {
                Id = "branches_count",
                Keywords = new[] { "branch", "branches", "فرع", "فروع" },
                Sql = clsAiChatSql.CountTable("Branch"),
                ResponseTemplateEn = "You have {0} branches.",
                ResponseTemplateAr = "لديك {0} فرع.",
            },
            new ChatIntent
            {
                Id = "accounts_count",
                Keywords = new[] { "account", "accounts", "chart of account", "حساب", "حسابات", "دليل الحسابات" },
                Sql = clsAiChatSql.CountTable("Accounts"),
                ResponseTemplateEn = "You have {0} accounts.",
                ResponseTemplateAr = "لديك {0} حساب.",
            },
            new ChatIntent
            {
                Id = "total_revenue",
                Keywords = new[] { "revenue", "total sales", "sales total", "total revenue", "ايرادات", "إيرادات", "مبيعات", "اجمالي المبيعات", "إجمالي المبيعات", "ventes", "venta", "ventas", "umsatz", "chiffre" },
                Sql = clsAiChatSql.SumSalesTotal(),
                ResponseTemplateEn = "Total sales revenue is {0}.",
                ResponseTemplateAr = "إجمالي الإيرادات من المبيعات هو {0}.",
            },
            new ChatIntent
            {
                Id = "month_revenue",
                Keywords = new[] { "this month sales", "month sales", "monthly sales", "sales this month", "revenue this month", "total sales this month", "مبيعات الشهر", "مبيعات هذا الشهر", "ايرادات الشهر", "إيرادات الشهر" },
                Sql = clsAiChatSql.SumSalesTotal(clsAiChatSql.FilterThisMonth),
                ResponseTemplateEn = "Sales this month are {0}.",
                ResponseTemplateAr = "مبيعات هذا الشهر هي {0}.",
            },
            new ChatIntent
            {
                Id = "today_sales",
                Keywords = new[] { "today sales", "sales today", "مبيعات اليوم", "مبيعات اليوم" },
                Sql = clsAiChatSql.SumSalesTotal(clsAiChatSql.FilterToday),
                ResponseTemplateEn = "Today's sales are {0}.",
                ResponseTemplateAr = "مبيعات اليوم هي {0}.",
            },
            new ChatIntent
            {
                Id = "invoice_count",
                Keywords = new[] { "invoice", "invoices", "فاتورة", "فواتير", "عدد الفواتير" },
                Sql = clsAiChatSql.CountInvoices(),
                ResponseTemplateEn = "You have {0} invoices in total.",
                ResponseTemplateAr = "لديك {0} فاتورة إجمالاً.",
            },
            new ChatIntent
            {
                Id = "pending_invoices",
                Keywords = new[] { "pending invoice", "pending invoices", "unpaid invoice", "فواتير معلقة", "فواتير غير مدفوعة" },
                Sql = clsAiChatSql.CountInvoices(clsAiChatSql.FilterPendingInvoices),
                ResponseTemplateEn = "You have {0} pending invoices.",
                ResponseTemplateAr = "لديك {0} فاتورة معلقة.",
            },
            new ChatIntent
            {
                Id = "jv_count",
                Keywords = new[] { "journal voucher", "journal vouchers", "jv", "قيد", "قيود", "سند قيد" },
                Sql = clsAiChatSql.CountJournalVouchers(),
                ResponseTemplateEn = "You have {0} journal vouchers.",
                ResponseTemplateAr = "لديك {0} سند قيد.",
            },
            new ChatIntent
            {
                Id = "cash_vouchers",
                Keywords = new[] { "cash voucher", "cash vouchers", "سند صرف", "سند قبض", "سندات نقدية" },
                Sql = clsAiChatSql.CountCashVouchers(),
                ResponseTemplateEn = "You have {0} cash vouchers.",
                ResponseTemplateAr = "لديك {0} سند نقدي.",
            },
            new ChatIntent
            {
                Id = "top_customers",
                Keywords = new[] { "top customer", "top customers", "best customer", "best customers", "أفضل العملاء", "افضل العملاء", "أكبر العملاء" },
                Sql = clsAiChatSql.TopCustomersBySales(),
                ResponseTemplateEn = "Top customers by sales:",
                ResponseTemplateAr = "أفضل العملاء حسب المبيعات:",
                IsList = true,
            },
            new ChatIntent
            {
                Id = "recent_invoices",
                Keywords = new[] { "recent invoice", "recent invoices", "latest invoice", "latest invoices", "آخر الفواتير", "اخر الفواتير", "أحدث الفواتير" },
                Sql = clsAiChatSql.RecentInvoices(),
                ResponseTemplateEn = "Recent invoices:",
                ResponseTemplateAr = "آخر الفواتير:",
                IsList = true,
            },
            new ChatIntent
            {
                Id = "monthly_sales_trend",
                Keywords = new[] { "monthly trend", "sales trend", "monthly sales trend", "اتجاه المبيعات", "مبيعات شهرية" },
                Sql = clsAiChatSql.MonthlySalesTrend(),
                ResponseTemplateEn = "Monthly sales trend:",
                ResponseTemplateAr = "اتجاه المبيعات الشهري:",
                IsList = true,
            },
            new ChatIntent
            {
                Id = "total_debit",
                Keywords = new[] { "total debit", "debit total", "اجمالي المدين", "إجمالي المدين" },
                Sql = clsAiChatSql.TotalDebit(),
                ResponseTemplateEn = "Total debit amount is {0}.",
                ResponseTemplateAr = "إجمالي المدين هو {0}.",
            },
            new ChatIntent
            {
                Id = "total_credit",
                Keywords = new[] { "total credit", "credit total", "اجمالي الدائن", "إجمالي الدائن" },
                Sql = clsAiChatSql.TotalCredit(),
                ResponseTemplateEn = "Total credit amount is {0}.",
                ResponseTemplateAr = "إجمالي الدائن هو {0}.",
            },
            new ChatIntent
            {
                Id = "purchase_total",
                Keywords = new[] { "purchase total", "total purchase", "purchases", "buying", "مشتريات", "اجمالي المشتريات", "إجمالي المشتريات", "شراء" },
                Sql = clsAiChatSql.SumPurchaseTotal(),
                ResponseTemplateEn = "Total purchases are {0}.",
                ResponseTemplateAr = "إجمالي المشتريات هو {0}.",
            },
            new ChatIntent
            {
                Id = "month_purchases",
                Keywords = new[] { "purchase this month", "purchases this month", "مشتريات الشهر", "مشتريات هذا الشهر" },
                Sql = clsAiChatSql.SumPurchaseTotal(clsAiChatSql.FilterThisMonth),
                ResponseTemplateEn = "Purchases this month are {0}.",
                ResponseTemplateAr = "مشتريات هذا الشهر هي {0}.",
            },
            new ChatIntent
            {
                Id = "sales_vs_purchase",
                Keywords = new[] { "sales vs purchase", "sales versus purchase", "compare sales", "مقارنة مبيعات", "مبيعات مقابل مشتريات" },
                Sql = clsAiChatSql.SalesVsPurchases(),
                ResponseTemplateEn = "Sales: {0} | Purchases: {1}",
                ResponseTemplateAr = "المبيعات: {0} | المشتريات: {1}",
                IsList = false,
            },
            new ChatIntent
            {
                Id = "inactive_customers",
                Keywords = new[] { "inactive customer", "inactive customers", "عملاء غير نشطين" },
                Sql = clsAiChatSql.CountInactiveCustomers(1),
                ResponseTemplateEn = "You have {0} inactive customers.",
                ResponseTemplateAr = "لديك {0} عميل غير نشط.",
            },
            new ChatIntent
            {
                Id = "departments_count",
                Keywords = new[] { "department", "departments", "قسم", "اقسام", "أقسام", "الاقسام" },
                Sql = clsAiChatSql.CountTable("Department"),
                ResponseTemplateEn = "You have {0} departments.",
                ResponseTemplateAr = "لديك {0} قسم.",
            },
            new ChatIntent
            {
                Id = "cost_centers_count",
                Keywords = new[] { "cost center", "cost centers", "مركز تكلفة", "مراكز التكلفة" },
                Sql = clsAiChatSql.CountTable("CostCenter"),
                ResponseTemplateEn = "You have {0} cost centers.",
                ResponseTemplateAr = "لديك {0} مركز تكلفة.",
            },
            new ChatIntent
            {
                Id = "avg_invoice",
                Keywords = new[] { "average invoice", "avg invoice", "average sale", "متوسط الفاتورة", "متوسط المبيعات" },
                Sql = clsAiChatSql.AvgInvoiceAmount(),
                ResponseTemplateEn = "Average invoice amount is {0}.",
                ResponseTemplateAr = "متوسط قيمة الفاتورة هو {0}.",
            },
            new ChatIntent
            {
                Id = "top_vendors",
                Keywords = new[] { "top vendor", "top vendors", "best supplier", "أفضل الموردين", "اكبر الموردين" },
                Sql = clsAiChatSql.TopVendorsByPurchases(),
                ResponseTemplateEn = "Top vendors by purchases:",
                ResponseTemplateAr = "أكبر الموردين حسب المشتريات:",
                IsList = true,
            },
            new ChatIntent
            {
                Id = "top_items",
                Keywords = new[] { "top item", "top items", "best selling", "most sold", "أكثر الأصناف", "أفضل الأصناف", "الأصناف الأكثر" },
                Sql = clsAiChatSql.TopItemsByQty(),
                ResponseTemplateEn = "Top selling items by quantity:",
                ResponseTemplateAr = "الأصناف الأكثر مبيعاً:",
                IsList = true,
            },
            new ChatIntent
            {
                Id = "financing_count",
                Keywords = new[] { "financing", "loan", "loans", "cash loan", "installment", "تمويل", "قرض", "قروض", "تقسيط" },
                Sql = clsAiChatSql.CountFinancingDocuments(),
                ResponseTemplateEn = "You have {0} financing/loan documents.",
                ResponseTemplateAr = "لديك {0} مستند تمويل/قرض.",
            },
            new ChatIntent
            {
                Id = "financing_total",
                Keywords = new[] { "total financing", "total loans", "loan total", "اجمالي التمويل", "إجمالي القروض" },
                Sql = clsAiChatSql.SumFinancingTotal(),
                ResponseTemplateEn = "Total financing amount is {0}.",
                ResponseTemplateAr = "إجمالي التمويل هو {0}.",
            },
            new ChatIntent
            {
                Id = "recent_financing",
                Keywords = new[] { "recent loan", "recent financing", "latest loan", "آخر التمويل", "آخر القروض" },
                Sql = clsAiChatSql.RecentFinancing(),
                ResponseTemplateEn = "Recent financing documents:",
                ResponseTemplateAr = "آخر مستندات التمويل:",
                IsList = true,
            },
            new ChatIntent
            {
                Id = "installment_count",
                Keywords = new[] { "installment", "installments", "schedule", "اقساط", "أقساط", "جدول اقساط" },
                Sql = clsAiChatSql.CountFinancingInstallments(),
                ResponseTemplateEn = "You have {0} installment lines.",
                ResponseTemplateAr = "لديك {0} سطر أقساط.",
            },
            new ChatIntent
            {
                Id = "reconciled_total",
                Keywords = new[] { "reconciliation", "reconciled", "settled", "settlement", "تسوية", "مسدد", "المسدد" },
                Sql = clsAiChatSql.TotalReconciledAmount(),
                ResponseTemplateEn = "Total reconciled/settled amount is {0}.",
                ResponseTemplateAr = "إجمالي المبالغ المسددة/المسوّاة هو {0}.",
            },
            new ChatIntent
            {
                Id = "open_receivables",
                Keywords = new[] { "open receivable", "outstanding receivable", "accounts receivable", "ar balance", "ذمم مدينة", "مديونية", "مستحقات" },
                Sql = clsAiChatSql.OpenReceivableBalance(),
                ResponseTemplateEn = "Total open receivables (unsettled debit) is {0}.",
                ResponseTemplateAr = "إجمالي الذمم المدينة المفتوحة (غير المسددة) هو {0}.",
            },
            new ChatIntent
            {
                Id = "cash_balance",
                Keywords = new[] { "cash balance", "cash on hand", "صندوق", "رصيد الصندوق", "رصيد نقدي" },
                Sql = clsAiChatSql.RoleAccountBalance(clsAiChatSchema.AccountRefs.Cash),
                ResponseTemplateEn = "Cash account balance (from journal) is {0}.",
                ResponseTemplateAr = "رصيد حساب الصندوق (من القيود) هو {0}.",
            },
            new ChatIntent
            {
                Id = "bank_balance",
                Keywords = new[] { "bank balance", "banks balance", "رصيد البنك", "رصيد البنوك" },
                Sql = clsAiChatSql.RoleAccountBalance(clsAiChatSchema.AccountRefs.Banks),
                ResponseTemplateEn = "Bank account balance (from journal) is {0}.",
                ResponseTemplateAr = "رصيد حساب البنوك (من القيود) هو {0}.",
            },
        };

        public static string NormalizePublic(string message) => NormalizeMessage(message);

        public bool TryQueryData(string message, int companyId, string lang, out string result, AiChatSession session = null)
        {
            result = QueryDataOnly(message, companyId, lang, session);
            return !string.IsNullOrWhiteSpace(result);
        }

        public string SearchBusinessPartnersPublic(string term, int companyId, string lang, AiChatSession session, bool forLlmTool = false) =>
            clsAiChatNameSearch.SearchPartners(term, companyId, lang, session, forLlmTool);

        public string SearchAccountsPublic(string term, int companyId, string lang, AiChatSession session, bool forLlmTool = false) =>
            clsAiChatNameSearch.SearchAccounts(term, companyId, lang, session, forLlmTool);

        public string SearchEverywherePublic(string term, int companyId, string lang, AiChatSession session, bool forLlmTool = false) =>
            clsAiChatNameSearch.SearchEverywhere(term, companyId, lang, session, forLlmTool);

        public string QueryDataOnly(string message, int companyId, string lang, AiChatSession session = null)
        {
            if (companyId <= 0)
                return "";

            string normalized = NormalizeMessage(message);
            if (string.IsNullOrWhiteSpace(normalized)) return "";

            normalized = clsAiChatKnowledge.RewriteWithSynonyms(normalized);

            AiChatSession effectiveSession = session ?? new AiChatSession();

            // "رصيد [name]" → search everywhere (accounts, partners, banks, cash)
            if (clsAiChatNameSearch.TryExtractNamedBalanceQuery(normalized, message, out string balanceName))
                return clsAiChatNameSearch.SearchEverywhere(balanceName, companyId, lang, effectiveSession);

            if (TryExtractNameSearch(normalized, out string searchTerm, out bool isAccount))
            {
                if (isAccount)
                    return clsAiChatNameSearch.SearchAccounts(searchTerm, companyId, lang, effectiveSession);
                if (LooksLikeExplicitPartnerSearch(normalized))
                    return clsAiChatNameSearch.SearchPartners(searchTerm, companyId, lang, effectiveSession);
                return clsAiChatNameSearch.SearchEverywhere(searchTerm, companyId, lang, effectiveSession);
            }

            ChatIntent intent = MatchIntent(normalized);
            if (intent != null)
                return ExecuteIntent(intent, companyId, lang);

            ChatIntent widgetIntent = MatchDashboardWidget(normalized, companyId);
            if (widgetIntent != null)
                return ExecuteIntent(widgetIntent, companyId, lang);

            return "";
        }

        public string ProcessMessage(string message, int companyId, string lang)
        {
            string normalized = NormalizeMessage(message);
            if (string.IsNullOrWhiteSpace(normalized))
            {
                return IsArabic(lang)
                    ? "اكتب سؤالك وسأجيبك من بيانات نظام ERP."
                    : "Type your question and I will answer from your ERP data.";
            }

            if (IsGreeting(normalized))
            {
                return IsArabic(lang)
                    ? "مرحباً! أنا مساعد MT SOFTS. اسألني عن العملاء، المبيعات، الفواتير، الموظفين، المنتجات، والمزيد."
                    : "Hello! I am the MT SOFTS assistant. Ask me about customers, sales, invoices, employees, products, and more.";
            }

            if (IsHelpRequest(normalized))
            {
                return BuildHelpMessage(lang);
            }

            if (companyId <= 0)
            {
                return IsArabic(lang)
                    ? "يرجى تسجيل الدخول واختيار شركة أولاً."
                    : "Please log in and select a company first.";
            }

            string dataResult = QueryDataOnly(message, companyId, lang);
            if (!string.IsNullOrWhiteSpace(dataResult))
                return dataResult;

            return IsArabic(lang)
                ? "لم أفهم سؤالك بعد. جرّب: كم عدد العملاء؟ أو ما هي مبيعات هذا الشهر؟ أو اكتب help للمساعدة."
                : "I could not match your question yet. Try: how many customers? what are this month sales? or type help.";
        }

        private static string NormalizeMessage(string message)
        {
            if (string.IsNullOrWhiteSpace(message)) return "";
            string text = message.Trim().ToLowerInvariant();
            text = Regex.Replace(text, @"[^\p{L}\p{N}\s\?\؟]", " ");
            text = Regex.Replace(text, @"\s+", " ").Trim();
            return text;
        }

        private static bool IsArabic(string lang) =>
            !string.IsNullOrWhiteSpace(lang) &&
            lang.StartsWith("Ar", StringComparison.OrdinalIgnoreCase);

        private static bool IsGreeting(string normalized)
        {
            string[] greetings =
            {
                "hi", "hello", "hey", "good morning", "good evening", "good afternoon",
                "مرحبا", "مرحباً", "السلام عليكم", "اهلا", "أهلا", "صباح الخير", "مساء الخير"
            };
            return greetings.Any(g => normalized == g || normalized.StartsWith(g + " "));
        }

        private static bool IsHelpRequest(string normalized)
        {
            string[] helpWords = { "help", "what can you do", "commands", "مساعدة", "ماذا يمكنك", "ساعدني" };
            return helpWords.Any(h => normalized.Contains(h));
        }

        private static string BuildHelpMessage(string lang)
        {
            if (IsArabic(lang))
            {
                return string.Join("\n", new[]
                {
                    "يمكنني مساعدتك في أي شيء متعلق بنظام ERP:",
                    "",
                    "**بيانات حية:**",
                    "• عملاء / موردين / موظفين / أصناف / حسابات",
                    "• مبيعات ومشتريات (فواتير + أنواع المستندات)",
                    "• أرصدة من **سندات القيد** (مدين/دائن/رصيد حساب/رصيد عميل)",
                    "• صندوق وبنوك، تمويل/قروض، تسويات (tbl_Reconciliation)",
                    "",
                    "**إرشاد:**",
                    "• كيف أنشئ فاتورة / قيد / تسوية / كشف حساب",
                    "• أين أجد الشاشات والتقارير في القائمة",
                    "",
                    "**أمثلة:**",
                    "• كم مبيعات هذا الشهر؟",
                    "• رصيد **[اسم حساب]** من دليل الحسابات (مثال: رصيد محمد طه)",
                    "• رصيد **عميل/مورد** فقط إذا ذكرت «عميل» أو «مورد» صراحة",
                    "• ابحث عن عميل [اسم] / ابحث عن حساب [اسم]",
                    "• كيف أنشئ فاتورة مبيعات؟"
                });
            }

            return string.Join("\n", new[]
            {
                "I can help with anything in the ERP:",
                "",
                "**Live data:**",
                "• Customers, vendors, employees, items, chart of accounts",
                "• Sales & purchases (invoices + document types)",
                "• Balances from **journal vouchers** (GL, customer/vendor sub-ledger)",
                "• Cash, banks, financing/loans, reconciliations (tbl_Reconciliation)",
                "",
                "**Guidance:**",
                "• How to create invoices, JVs, reconciliations, account statements",
                "• Where to find screens and reports in the menu",
                "",
                "**Examples:**",
                "• This month sales?",
                "• Cash balance / account balance for [name]",
                "• Find customer [name] / find account [name]",
                "• How do I create a sales invoice?"
            });
        }

        private static bool TryExtractNameSearch(string normalized, out string term, out bool isAccount)
        {
            term = ExtractSearchTerm(normalized, out isAccount);
            return !string.IsNullOrWhiteSpace(term);
        }

        private static string ExtractSearchTerm(string normalized, out bool isAccount)
        {
            isAccount = false;

            string[] accountPrefixes =
            {
                "find account", "search account", "lookup account", "account named", "chart account",
                "find accounts", "search accounts", "ابحث عن حساب", "بحث عن حساب", "اعثر على حساب", "حساب"
            };

            string[] partnerPrefixes =
            {
                "find customer", "search customer", "lookup customer", "customer named",
                "find client", "search client", "find vendor", "search vendor",
                "find partner", "search partner", "business partner",
                "balance for", "balance of",
                "ابحث عن", "بحث عن", "اعثر على", "جد", "ابحث", "عميل", "مورد", "زبون"
            };

            foreach (string prefix in accountPrefixes)
            {
                int idx = normalized.IndexOf(prefix, StringComparison.Ordinal);
                if (idx >= 0)
                {
                    isAccount = true;
                    string t = normalized[(idx + prefix.Length)..].Trim();
                    if (!string.IsNullOrWhiteSpace(t)) return t;
                }
            }

            foreach (string prefix in partnerPrefixes)
            {
                int idx = normalized.IndexOf(prefix, StringComparison.Ordinal);
                if (idx >= 0)
                {
                    string t = normalized[(idx + prefix.Length)..].Trim();
                    if (!string.IsNullOrWhiteSpace(t)) return t;
                }
            }

            return "";
        }

        private static string ExtractSearchTerm(string normalized) =>
            ExtractSearchTerm(normalized, out _);

        private static bool LooksLikeExplicitPartnerSearch(string normalized)
        {
            string[] markers =
            {
                "customer", "vendor", "client", "partner", "business partner",
                "عميل", "مورد", "زبون", "شريك"
            };
            return markers.Any(m => normalized.Contains(m, StringComparison.Ordinal));
        }

        private static ChatIntent MatchIntent(string normalized)
        {
            ChatIntent best = null;
            int bestScore = 0;

            foreach (ChatIntent intent in Intents)
            {
                int score = 0;
                foreach (string keyword in intent.Keywords)
                {
                    if (normalized.Contains(keyword, StringComparison.Ordinal))
                        score += keyword.Contains(' ') ? 5 : 1;
                }

                if (score > bestScore)
                {
                    bestScore = score;
                    best = intent;
                }
            }

            // Prefer more specific (multi-word) matches
            return bestScore >= 2 ? best : null;
        }

        private ChatIntent MatchDashboardWidget(string normalized, int companyId)
        {
            try
            {
                clsSQL sql = new clsSQL();
                string conn = sql.CreateDataBaseConnectionString(companyId);
                DataTable dt = sql.ExecuteQueryStatement(
                    @"SELECT TOP 20 Title, SQLQuery
                      FROM tbl_DashboardWidgets
                      WHERE IsActive = 1 AND UserId = -1 AND CompanyID = @CompanyID
                      ORDER BY ID",
                    conn,
                    new[]
                    {
                        new SqlParameter("@CompanyID", SqlDbType.Int) { Value = companyId }
                    });

                if (dt == null || dt.Rows.Count == 0) return null;

                ChatIntent best = null;
                int bestScore = 0;

                foreach (DataRow row in dt.Rows)
                {
                    string title = Simulate.String(row["Title"]).Trim();
                    if (string.IsNullOrWhiteSpace(title)) continue;

                    string titleNorm = NormalizeMessage(title);
                    if (string.IsNullOrWhiteSpace(titleNorm)) continue;

                    int score = 0;
                    foreach (string word in titleNorm.Split(' ', StringSplitOptions.RemoveEmptyEntries))
                    {
                        if (word.Length >= 3 && normalized.Contains(word, StringComparison.Ordinal))
                            score++;
                    }

                    if (normalized.Contains(titleNorm, StringComparison.Ordinal))
                        score += 3;

                    if (score > bestScore)
                    {
                        bestScore = score;
                        string widgetSql = Simulate.String(row["SQLQuery"]);
                        best = new ChatIntent
                        {
                            Id = "widget_" + titleNorm.Replace(' ', '_'),
                            Sql = widgetSql,
                            ResponseTemplateEn = title + ":",
                            ResponseTemplateAr = title + ":",
                            IsList = !LooksLikeSingleValueResult(widgetSql),
                        };
                    }
                }

                return bestScore >= 2 ? best : null;
            }
            catch
            {
                return null;
            }
        }

        private string ExecuteIntent(ChatIntent intent, int companyId, string lang)
        {
            if (string.IsNullOrWhiteSpace(intent.Sql))
            {
                return IsArabic(lang)
                    ? "لا يوجد استعلام مرتبط بهذا السؤال."
                    : "No query is linked to this question.";
            }

            if (!IsReadOnlySql(intent.Sql))
            {
                return IsArabic(lang)
                    ? "هذا النوع من الأسئلة غير مدعوم حالياً."
                    : "This type of question is not supported yet.";
            }

            try
            {
                clsSQL sql = new clsSQL();
                string conn = sql.CreateDataBaseConnectionString(companyId);
                SqlParameter[] parms = intent.Sql.Contains("@CompanyId", StringComparison.OrdinalIgnoreCase)
                    ? new[] { new SqlParameter("@CompanyId", SqlDbType.Int) { Value = companyId } }
                    : null;
                DataTable dt = sql.ExecuteQueryStatement(intent.Sql, conn, parms);

                if (dt == null || dt.Rows.Count == 0)
                {
                    return IsArabic(lang) ? "لا توجد بيانات." : "No data found.";
                }

                string header = IsArabic(lang) ? intent.ResponseTemplateAr : intent.ResponseTemplateEn;

                if (intent.IsList)
                {
                    return header + "\n" + FormatList(dt, lang);
                }

                if (dt.Columns.Contains("Sales") && dt.Columns.Contains("Purchases"))
                {
                    return string.Format(CultureInfo.InvariantCulture, header,
                        FormatValue(dt.Rows[0]["Sales"]),
                        FormatValue(dt.Rows[0]["Purchases"]));
                }

                object value;
                if (dt.Columns.Contains("Total"))
                    value = dt.Rows[0]["Total"];
                else if (dt.Columns.Contains("Balance"))
                    value = dt.Rows[0]["Balance"];
                else
                    value = dt.Rows[0][0];

                string formatted = FormatValue(value);
                return string.Format(CultureInfo.InvariantCulture, header, formatted);
            }
            catch (Exception ex)
            {
                return IsArabic(lang)
                    ? "حدث خطأ أثناء قراءة البيانات: " + ex.Message
                    : "Error reading data: " + ex.Message;
            }
        }

        private static bool LooksLikeSingleValueResult(string sql)
        {
            string upper = sql.ToUpperInvariant();
            return upper.Contains("COUNT(") ||
                   upper.Contains("SUM(") ||
                   upper.Contains("AVG(") ||
                   upper.Contains("MAX(") ||
                   upper.Contains("MIN(");
        }

        private static bool IsReadOnlySql(string sql)
        {
            if (string.IsNullOrWhiteSpace(sql)) return false;

            string upper = Regex.Replace(sql, @"--.*$|/\*.*?\*/", "", RegexOptions.Multiline | RegexOptions.Singleline)
                .Trim()
                .ToUpperInvariant();

            if (!upper.StartsWith("SELECT", StringComparison.Ordinal) &&
                !upper.StartsWith("WITH", StringComparison.Ordinal))
                return false;

            string[] forbidden =
            {
                "INSERT ", "UPDATE ", "DELETE ", "DROP ", "ALTER ", "TRUNCATE ",
                "EXEC ", "EXECUTE ", "MERGE ", "CREATE ", "GRANT ", "REVOKE "
            };

            return !forbidden.Any(f => upper.Contains(f, StringComparison.Ordinal));
        }

        private static string FormatList(DataTable dt, string lang)
        {
            var lines = new List<string>();
            int max = Math.Min(dt.Rows.Count, 5);
            for (int r = 0; r < max; r++)
            {
                DataRow row = dt.Rows[r];
                if (dt.Columns.Count >= 2)
                {
                    string name = Simulate.String(row[0]);
                    string total = FormatValue(row[1]);
                    if (dt.Columns.Count >= 3 && row[2] != DBNull.Value)
                        lines.Add($"• {name}: {total} ({FormatValue(row[2])})");
                    else
                        lines.Add($"• {name}: {total}");
                }
                else
                {
                    lines.Add("• " + FormatValue(row[0]));
                }
            }

            if (dt.Rows.Count > max)
            {
                lines.Add(IsArabic(lang)
                    ? $"... و {dt.Rows.Count - max} أخرى. اسألني إن أردت التفاصيل."
                    : $"... and {dt.Rows.Count - max} more. Ask me if you need details.");
            }

            return string.Join("\n", lines);
        }

        private static string FormatValue(object value)
        {
            if (value == null || value == DBNull.Value) return "0";

            if (value is decimal dec)
                return dec.ToString("N2", CultureInfo.InvariantCulture);
            if (value is double dbl)
                return dbl.ToString("N2", CultureInfo.InvariantCulture);
            if (value is float flt)
                return flt.ToString("N2", CultureInfo.InvariantCulture);
            if (value is int or long or short)
                return Convert.ToString(value, CultureInfo.InvariantCulture);
            if (value is DateTime dt)
                return dt.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);

            return Convert.ToString(value, CultureInfo.InvariantCulture) ?? "";
        }
    }
}
