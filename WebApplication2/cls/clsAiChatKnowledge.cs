using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace WebApplication2.cls
{
    /// <summary>
    /// ERP system knowledge: modules, screens, features, and query synonyms.
    /// </summary>
    public static class clsAiChatKnowledge
    {
        public sealed class KnowledgeEntry
        {
            public string Id { get; init; } = "";
            public string Category { get; init; } = "";
            public string TitleEn { get; init; } = "";
            public string TitleAr { get; init; } = "";
            public string AnswerEn { get; init; } = "";
            public string AnswerAr { get; init; } = "";
            public string[] Keywords { get; init; } = Array.Empty<string>();
        }

        private static readonly (string from, string to)[] Synonyms =
        {
            ("revenue", "total sales"), ("income", "total sales"), ("turnover", "total sales"),
            ("clients", "customers"), ("buyers", "customers"), ("suppliers", "vendors"),
            ("stock", "inventory"), ("products", "items"), ("sku", "items"),
            ("staff", "employees"), ("workers", "employees"), ("headcount", "employees"),
            ("bills", "invoices"), ("receipts", "invoices"),
            ("profit", "profit margin"), ("loss", "expenses"),
            ("jv", "journal voucher"), ("gl", "journal voucher"),
            ("ar", "receivable"), ("ap", "payable"),
            ("p&l", "income statement"), ("pnl", "income statement"),
            ("balance sheet", "balances sheet"),
            ("loans", "financing"), ("installments", "financing"),
            ("pos", "pos sales"), ("point of sale", "pos sales"),
            ("إيرادات", "مبيعات"), ("ايرادات", "مبيعات"), ("دخل", "مبيعات"),
            ("زبائن", "عملاء"), ("زبون", "عملاء"), ("موردين", "مورد"),
            ("مخزون", "منتجات"), ("أصناف", "منتجات"), ("اصناف", "منتجات"),
            ("موظفين", "موظف"), ("فواتير", "فاتورة"), ("قيود", "قيد"),
            ("ميزان", "ميزان مراجعة"), ("كشف", "كشف حساب"),
            ("تقسيط", "تمويل"), ("قروض", "تمويل"), ("سلف", "تمويل"),
        };

        private static readonly KnowledgeEntry[] Entries =
        {
            // ── Modules ──
            Entry("mod_accounting", "module", "Accounting Module", "وحدة المحاسبة",
                "Handles journal vouchers, payment/receivable vouchers, debit/credit notes, reconciliations, and financial reports (trial balance, account statement, P&L, balance sheet, aging).",
                "تدير سندات القيد، سندات الصرف والقبض، الإشعارات المدينة والدائنة، التسويات، والتقارير المالية (ميزان المراجعة، كشف الحساب، الأرباح والخسائر، الميزانية، أعمار الذمم).",
                "accounting", "finance", "general ledger", "gl", "محاسبة", "مالية"),

            Entry("mod_inventory", "module", "Inventory Module", "وحدة المخزون",
                "Sales/purchase invoices, refunds, offers, good issue/receipt, POS invoices, and inventory reports.",
                "فواتير المبيعات والمشتريات، المرتجعات، العروض، صرف واستلام البضاعة، فواتير POS، وتقارير المخزون.",
                "inventory", "warehouse", "stock", "purchasing", "sales", "مخزون", "مشتريات", "مبيعات"),

            Entry("mod_hr", "module", "Human Resources Module", "وحدة الموارد البشرية",
                "Employees, contracts, job titles, salary elements, payroll periods, payroll preview, attendance machines, punch review, attendance rules, shifts.",
                "الموظفين، العقود، المسميات الوظيفية، عناصر الراتب، فترات الرواتب، معاينة الرواتب، أجهزة البصمة، مراجعة الحضور، قواعد الحضور، الورديات.",
                "hr", "human resources", "payroll", "attendance", "employees", "موارد بشرية", "رواتب", "حضور"),

            Entry("mod_financing", "module", "Financing Module", "وحدة التمويل",
                "Installment sales, cash loans, loan scheduling, customer loans, financing reports.",
                "مبيعات التقسيط، القروض النقدية، جدولة الأقساط، قروض العملاء، تقارير التمويل.",
                "financing", "loans", "installments", "credit sales", "تمويل", "قروض", "تقسيط"),

            Entry("mod_pos", "module", "POS Module", "نقطة البيع",
                "Point-of-sale screen for fast retail/restaurant sales with payment methods and hold invoices.",
                "شاشة نقطة البيع للمبيعات السريعة (تجزئة/مطاعم) مع طرق الدفع وحفظ الفواتير.",
                "pos", "point of sale", "retail", "restaurant", "نقطة بيع", "كاشير"),

            Entry("mod_manufacturing", "module", "Manufacturing Module", "وحدة التصنيع",
                "Manufacturing orders, BOM (bill of materials), production planning.",
                "أوامر التصنيع، قائمة المواد BOM، تخطيط الإنتاج.",
                "manufacturing", "production", "bom", "mo", "تصنيع", "انتاج", "مواد"),

            Entry("mod_settings", "module", "Settings Module", "الإعدادات",
                "Company setup, chart of accounts, items, branches, cost centers, users, authorization, dashboard widgets, report templates.",
                "إعداد الشركة، دليل الحسابات، الأصناف، الفروع، مراكز التكلفة، المستخدمين، الصلاحيات، لوحة التحكم، قوالب التقارير.",
                "settings", "setup", "configuration", "admin", "إعدادات", "ضبط"),

            // ── Navigation: Accounting ──
            Nav("nav_jv", "Journal Voucher", "سند قيد", "Main menu → Accounting → Journal Voucher",
                "القائمة → المحاسبة → سند قيد", "Create manual journal entries with debit/credit lines.",
                "journal voucher", "jv", "manual jv", "قيد", "سند قيد", "قيد يومية"),

            Nav("nav_payment", "Payment Voucher", "سند صرف", "Main menu → Accounting → Payment Voucher",
                "القائمة → المحاسبة → سند صرف", "Pay suppliers, expenses, or any account from cash/bank.",
                "payment voucher", "cash payment", "pay", "سند صرف", "صرف"),

            Nav("nav_receivable", "Receivable Voucher", "سند قبض", "Main menu → Accounting → Receivable Voucher",
                "القائمة → المحاسبة → سند قبض", "Receive money from customers into cash/bank.",
                "receivable voucher", "cash receipt", "receipt voucher", "سند قبض", "قبض"),

            Nav("nav_trial_balance", "Trial Balance", "ميزان المراجعة", "Main menu → Accounting → Trial Balance",
                "القائمة → المحاسبة → ميزان المراجعة", "Shows all account balances for a date range.",
                "trial balance", "tb", "ميزان", "ميزان مراجعة"),

            Nav("nav_account_statement", "Account Statement", "كشف حساب", "Main menu → Accounting → Account Statement",
                "القائمة → المحاسبة → كشف حساب", "Detailed transactions and running balance for an account.",
                "account statement", "statement of account", "كشف حساب", "كشف"),

            Nav("nav_aging", "Aging Report", "تقرير أعمار الذمم", "Main menu → Accounting → Aging Report",
                "القائمة → المحاسبة → Aging Report", "Shows overdue receivables/payables by aging buckets.",
                "aging", "aging report", "overdue", "أعمار", "ذمم"),

            Nav("nav_balance_sheet", "Balance Sheet", "الميزانية العمومية", "Main menu → Accounting → Balance Sheet Report",
                "القائمة → المحاسبة → تقرير الميزانية", "Assets, liabilities, and equity at a point in time.",
                "balance sheet", "financial position", "ميزانية", "مركز مالي"),

            Nav("nav_income_statement", "Income Statement (P&L)", "قائمة الدخل", "Main menu → Accounting → Income Statement Report",
                "القائمة → المحاسبة → قائمة الدخل", "Revenue, expenses, and profit for a period.",
                "income statement", "p&l", "profit and loss", "pnl", "أرباح", "خسائر", "دخل"),

            Nav("nav_reconciliation", "Reconciliations", "التسويات", "Main menu → Accounting → Reconciliations",
                "القائمة → المحاسبة → التسويات", "Match open debit/credit entries for customers, vendors, or accounts.",
                "reconciliation", "reconcile", "match entries", "تسوية", "مطابقة"),

            // ── Concepts: data model ──
            Entry("concept_jv_balance", "concept", "Journal Voucher Balances", "أرصدة سندات القيد",
                "All GL balances come from tbl_JournalVoucherDetails (debit/credit lines) linked to tbl_JournalVoucherHeader via ParentGuid→Guid. This is the source of truth for account balances.",
                "كل الأرصدة المحاسبية تأتي من tbl_JournalVoucherDetails (مدين/دائن) المرتبطة بـ tbl_JournalVoucherHeader عبر ParentGuid→Guid. هذا مصدر الحقيقة للأرصدة.",
                "journal balance", "gl balance", "source of truth", "ledger balance", "رصيد قيد", "رصيد محاسبي"),

            Entry("concept_subaccount", "concept", "SubAccountID (Sub-ledger)", "SubAccountID (الدفتر الفرعي)",
                "SubAccountID on JV detail lines identifies the sub-ledger row: customer/vendor (tbl_BusinessPartner), bank (tbl_Banks), or cash drawer (tbl_CashDrawer), depending on the GL AccountID from tbl_AccountSetting.",
                "SubAccountID في تفاصيل القيد يحدد الدفتر الفرعي: عميل/مورد (tbl_BusinessPartner)، بنك (tbl_Banks)، أو صندوق (tbl_CashDrawer) حسب AccountID من tbl_AccountSetting.",
                "subaccount", "sub account", "sub ledger", "subledger", "دفتر فرعي", "subaccountid"),

            Entry("concept_reconciliation", "concept", "Reconciliation / Settlements", "التسوية والمسدد",
                "tbl_Reconciliation stores settled amounts linked to JV detail lines (JVDetailsGuid). Open receivable = debit minus reconciled amount. Use this for collections and payment matching.",
                "tbl_Reconciliation يخزن المبالغ المسددة المرتبطة بتفاصيل القيد (JVDetailsGuid). الذمم المفتوحة = مدين ناقص المسدد. تُستخدم لمتابعة التحصيل ومطابقة الدفعات.",
                "reconciliation table", "settled", "settlement", "open balance", "مسدد", "تسوية", "reconciled"),

            Entry("concept_invoice_types", "concept", "Invoice & Document Types", "أنواع الفواتير والمستندات",
                "InvoiceTypeID and JVTypeID both reference tbl_JournalVoucherTypes. Sales invoices=3,10; POS sales=10; purchases=2,22; sales returns=4,11; financing=14,15.",
                "InvoiceTypeID و JVTypeID يشيران إلى tbl_JournalVoucherTypes. مبيعات=3,10؛ POS=10؛ مشتريات=2,22؛ مرتجعات=4,11؛ تمويل=14,15.",
                "invoice type", "document type", "voucher type", "نوع فاتورة", "نوع مستند"),

            Entry("concept_financing", "concept", "Financing & Installment Sales", "التمويل والبيع بالتقسيط",
                "Cash loans and installment sales are stored in tbl_FinancingHeader with schedule lines in tbl_FinancingDetails. Each may link to a journal voucher via JVGuid.",
                "القروض النقدية والبيع بالتقسيط في tbl_FinancingHeader مع جدول الأقساط في tbl_FinancingDetails. كل مستند قد يرتبط بقيد محاسبي عبر JVGuid.",
                "financing header", "installment sales", "cash loan", "loan scheduling", "تمويل", "تقسيط", "قرض"),

            Nav("nav_debit_note", "Debit Note", "إشعار مدين", "Main menu → Accounting → Debit Note",
                "القائمة → المحاسبة → إشعار مدين", "Increase what a customer owes or adjust receivables.",
                "debit note", "اشعار مدين", "إشعار مدين"),

            Nav("nav_credit_note", "Credit Note", "إشعار دائن", "Main menu → Accounting → Credit Note",
                "القائمة → المحاسبة → إشعار دائن", "Reduce customer balance or grant credit.",
                "credit note", "اشعار دائن", "إشعار دائن"),

            // ── Navigation: Inventory ──
            Nav("nav_sales_inv", "Sales Invoice", "فاتورة مبيعات", "Main menu → Inventory → Sales Invoice",
                "القائمة → المخزون → فاتورة مبيعات", "Invoice to a customer; reduces stock and posts revenue.",
                "sales invoice", "sell", "فاتورة مبيعات", "بيع"),

            Nav("nav_purchase_inv", "Purchase Invoice", "فاتورة مشتريات", "Main menu → Inventory → Purchase Invoice",
                "القائمة → المخزون → فاتورة مشتريات", "Invoice from supplier; increases stock and payables.",
                "purchase invoice", "buy", "فاتورة مشتريات", "شراء"),

            Nav("nav_good_issue", "Good Issue", "صرف بضاعة", "Main menu → Inventory → Good Issue",
                "القائمة → المخزون → صرف بضاعة", "Issue stock without a sales invoice (consumption, transfer).",
                "good issue", "issue stock", "صرف", "صرف بضاعة"),

            Nav("nav_good_receipt", "Good Receipt", "استلام بضاعة", "Main menu → Inventory → Good Receipt",
                "القائمة → المخزون → استلام بضاعة", "Receive stock without a purchase invoice.",
                "good receipt", "receive stock", "استلام", "استلام بضاعة"),

            Nav("nav_inv_report", "Inventory Report", "تقرير المخزون", "Main menu → Inventory → Inventory Report",
                "القائمة → المخزون → تقرير المخزون", "Current stock quantities and values by item/warehouse.",
                "inventory report", "stock report", "stock on hand", "تقرير مخزون", "رصيد مخزون"),

            Nav("nav_item_trans", "Item Transactions Report", "حركة الأصناف", "Main menu → Inventory → Item Transactions Report",
                "القائمة → المخزون → حركة الأصناف", "All movements (in/out) per item for a period.",
                "item transactions", "item movement", "stock movement", "حركة اصناف", "حركة الأصناف"),

            // ── Navigation: HR ──
            Nav("nav_employees", "Employees", "الموظفين", "Main menu → Human Resources → Employees",
                "القائمة → الموارد البشرية → الموظفين", "Employee master data, users, and profiles.",
                "employees", "staff list", "users", "موظفين", "الموظفين"),

            Nav("nav_payroll_period", "Payroll Period", "فترة الرواتب", "Main menu → Human Resources → Payroll Period",
                "القائمة → الموارد البشرية → فترة الرواتب", "Define open/closed payroll months.",
                "payroll period", "salary period", "فترة رواتب", "فترة الرواتب"),

            Nav("nav_payroll_preview", "Payroll Preview", "معاينة الرواتب", "Main menu → Human Resources → Payroll Preview",
                "القائمة → الموارد البشرية → معاينة الرواتب", "Calculate and review salaries before posting.",
                "payroll preview", "run payroll", "salary calculation", "معاينة رواتب", "احتساب رواتب"),

            Nav("nav_attendance", "Attendance Calculation", "احتساب الحضور", "Main menu → Human Resources → Attendance Calculation",
                "القائمة → الموارد البشرية → احتساب الحضور", "Review worked hours, late, overtime, absence.",
                "attendance calculation", "attendance review", "timesheet", "حضور", "احتساب حضور"),

            Nav("nav_att_machines", "Attendance Machines", "أجهزة البصمة", "Main menu → Human Resources → Attendance Machines",
                "القائمة → الموارد البشرية → أجهزة البصمة", "Register devices and sync biometric punches.",
                "attendance machine", "fingerprint", "biometric", "بصمة", "جهاز حضور"),

            Nav("nav_shifts", "Shift Management", "إدارة الورديات", "Main menu → Human Resources → Shift Management",
                "القائمة → الموارد البشرية → إدارة الورديات", "Define work shifts and assign to employees.",
                "shift", "shifts", "work schedule", "وردية", "ورديات", "شفت"),

            // ── Navigation: Other ──
            Nav("nav_financing", "Financing", "التمويل", "Main menu → Financing",
                "القائمة → التمويل", "Installment sales, loan contracts, scheduling, and loan reports.",
                "financing", "loans", "installment", "تمويل", "قروض"),

            Nav("nav_dashboard", "Dashboard", "لوحة التحكم", "Main menu → Dashboard",
                "القائمة → Dashboard", "Custom KPI widgets, charts, and business metrics.",
                "dashboard", "kpi", "widgets", "لوحة", "دashboard"),

            Nav("nav_pos", "POS", "نقطة البيع", "Main menu → POS",
                "القائمة → POS", "Open the point-of-sale screen for retail/restaurant.",
                "pos", "point of sale", "cashier", "كاشير"),

            Nav("nav_settings", "Settings", "الإعدادات", "Main menu → Settings",
                "القائمة → الإعدادات", "Master data, accounts, items, branches, users, permissions.",
                "settings", "setup", "configuration", "إعدادات"),

            // ── FAQs ──
            Faq("faq_trial_balance", "What is Trial Balance?", "ما هو ميزان المراجعة؟",
                "A Trial Balance lists every account's debit and credit totals for a period. It helps verify that books are balanced before preparing financial statements. Open it from Accounting → Trial Balance.",
                "ميزان المراجعة يعرض مجموع المدين والدائن لكل حساب في فترة معينة. يساعد على التأكد من توازن الدفاتر قبل القوائم المالية. افتحه من المحاسبة → ميزان المراجعة.",
                "what is trial balance", "explain trial balance", "ما هو ميزان", "اشرح ميزان"),

            Faq("faq_reconciliation", "What is Reconciliation?", "ما هي التسوية؟",
                "Reconciliation links related debit and credit entries (e.g. invoice payment against invoice) so open balances are cleared. Use Accounting → Reconciliations.",
                "التسوية تربط القيود المدينة والدائنة المرتبطة (مثل دفعة فاتورة مع الفاتورة) لتصفية الأرصدة المفتوحة. استخدم المحاسبة → التسويات.",
                "what is reconciliation", "explain reconciliation", "ما هي التسوية", "شرح التسوية"),

            Faq("faq_jv", "What is a Journal Voucher?", "ما هو سند القيد؟",
                "A Journal Voucher (JV) is a manual accounting entry with debit and credit lines. Used for adjustments, accruals, and non-invoice transactions. Path: Accounting → Journal Voucher.",
                "سند القيد هو قيد محاسبي يدوي ببنود مدين ودائن. يُستخدم للتسويات والقيود غير المرتبطة بفاتورة. المسار: المحاسبة → سند قيد.",
                "what is journal voucher", "what is jv", "ما هو القيد", "ما هو سند القيد"),

            Faq("faq_cost_center", "What is a Cost Center?", "ما هو مركز التكلفة؟",
                "A Cost Center lets you track expenses/revenue by department, project, or branch. Select it on vouchers and invoices for segmented reporting.",
                "مركز التكلفة يتيح تتبع المصروفات والإيرادات حسب قسم أو مشروع أو فرع. يُختار في السندات والفواتير للتقارير التفصيلية.",
                "cost center", "what is cost center", "مركز تكلفة", "مراكز التكلفة"),

            Faq("faq_business_partner", "What is a Business Partner?", "ما هو شريك الأعمال؟",
                "Business Partners are customers and vendors in one master table. They link to sales/purchase invoices and AR/AP accounts.",
                "شركاء الأعمال هم العملاء والموردون في جدول رئيسي واحد. يرتبطون بفواتير المبيعات/المشتريات وحسابات الذمم.",
                "business partner", "customer vendor", "شريك اعمال", "شريك الأعمال"),
        };

        public static string RewriteWithSynonyms(string normalized)
        {
            if (string.IsNullOrWhiteSpace(normalized)) return normalized;
            string text = " " + normalized + " ";
            foreach ((string from, string to) in Synonyms)
            {
                text = Regex.Replace(text, $@"\b{Regex.Escape(from)}\b", " " + to + " ", RegexOptions.IgnoreCase);
            }
            return Regex.Replace(text, @"\s+", " ").Trim();
        }

        public static KnowledgeEntry SearchBest(string normalizedMessage)
        {
            if (string.IsNullOrWhiteSpace(normalizedMessage)) return null;

            KnowledgeEntry best = null;
            int bestScore = 0;

            foreach (KnowledgeEntry entry in Entries)
            {
                int score = ScoreEntry(entry, normalizedMessage);
                if (score > bestScore)
                {
                    bestScore = score;
                    best = entry;
                }
            }

            return bestScore >= 3 ? best : null;
        }

        public static IReadOnlyList<KnowledgeEntry> SearchAll(string normalizedMessage, int max = 5)
        {
            return Entries
                .Select(e => (entry: e, score: ScoreEntry(e, normalizedMessage)))
                .Where(x => x.score > 0)
                .OrderByDescending(x => x.score)
                .Take(max)
                .Select(x => x.entry)
                .ToList();
        }

        public static string FormatEntry(KnowledgeEntry entry, string lang)
        {
            bool ar = IsArabic(lang);
            string title = ar ? entry.TitleAr : entry.TitleEn;
            string answer = ar ? entry.AnswerAr : entry.AnswerEn;

            if (entry.Category == "navigation")
            {
                return ar
                    ? $"📍 **{title}**\n{answer}\n\n💡 للمساعدة التفاعلية اضغط زر ? في الشاشة."
                    : $"📍 **{title}**\n{answer}\n\n💡 Use the ? help button on screen for interactive tours.";
            }

            return ar ? $"ℹ️ **{title}**\n{answer}" : $"ℹ️ **{title}**\n{answer}";
        }

        public static string GetSystemOverview(string lang)
        {
            if (IsArabic(lang))
            {
                return string.Join("\n", new[]
                {
                    "🏢 **MT SOFTS ERP** — الوحدات الرئيسية:",
                    "",
                    "📒 **المحاسبة**: سندات، قيود، تسويات، تقارير مالية",
                    "📦 **المخزون**: فواتير مبيعات/مشتريات، صرف/استلام، تقارير",
                    "👥 **الموارد البشرية**: موظفين، رواتب، حضور، ورديات",
                    "💰 **التمويل**: تقسيط، قروض، جدولة أقساط",
                    "🛒 **POS**: نقطة بيع سريعة",
                    "🏭 **التصنيع**: أوامر تصنيع و BOM",
                    "⚙️ **الإعدادات**: حسابات، أصناف، فروع، صلاحيات",
                    "",
                    "اسألني: «أين ميزان المراجعة؟» أو «كم مبيعات هذا الشهر؟» أو «كيف أنشئ فاتورة؟»"
                });
            }

            return string.Join("\n", new[]
            {
                "🏢 **MT SOFTS ERP** — main modules:",
                "",
                "📒 **Accounting**: vouchers, JVs, reconciliation, financial reports",
                "📦 **Inventory**: sales/purchase invoices, stock moves, reports",
                "👥 **HR**: employees, payroll, attendance, shifts",
                "💰 **Financing**: installments, loans, scheduling",
                "🛒 **POS**: fast point-of-sale",
                "🏭 **Manufacturing**: MO, BOM, planning",
                "⚙️ **Settings**: accounts, items, branches, permissions",
                "",
                "Ask me: «where is trial balance?» or «this month sales?» or «how to create invoice?»"
            });
        }

        public static bool LooksLikeNavigationQuestion(string normalized) =>
            ContainsAny(normalized, "where is", "where are", "how to open", "how do i find", "navigate to", "go to", "open",
                "location", "menu", "screen", "page",
                "أين", "اين", "وين", "كيف افتح", "كيف أفتح", "من القائمة", "من وين", "مسار");

        public static bool LooksLikeConceptQuestion(string normalized) =>
            ContainsAny(normalized, "what is", "what are", "explain", "tell me about", "define", "meaning of",
                "ما هو", "ما هي", "اشرح", "اشرح لي", "وضح", "تعريف", "معنى");

        public static bool LooksLikeModuleQuestion(string normalized) =>
            ContainsAny(normalized, "modules", "features", "system", "what do you have", "capabilities", "what can erp",
                "وحدات", "موديول", "مميزات", "النظام", "ماذا يحتوي", "إمكانيات");

        private static int ScoreEntry(KnowledgeEntry entry, string normalized)
        {
            int score = 0;
            foreach (string kw in entry.Keywords)
            {
                if (normalized.Contains(kw, StringComparison.Ordinal))
                    score += kw.Contains(' ') ? 5 : 2;
            }

            string titleNorm = entry.TitleEn.ToLowerInvariant();
            foreach (string word in normalized.Split(' ', StringSplitOptions.RemoveEmptyEntries))
            {
                if (word.Length >= 4 && titleNorm.Contains(word, StringComparison.Ordinal))
                    score++;
            }

            return score;
        }

        private static KnowledgeEntry Entry(string id, string cat, string titleEn, string titleAr,
            string ansEn, string ansAr, params string[] keywords) => new()
        {
            Id = id, Category = cat, TitleEn = titleEn, TitleAr = titleAr,
            AnswerEn = ansEn, AnswerAr = ansAr, Keywords = keywords
        };

        private static KnowledgeEntry Nav(string id, string titleEn, string titleAr, string pathEn, string pathAr,
            string descEn, params string[] keywords) => new()
        {
            Id = id, Category = "navigation", TitleEn = titleEn, TitleAr = titleAr,
            AnswerEn = $"Path: {pathEn}\n{descEn}",
            AnswerAr = $"المسار: {pathAr}\n{descEn}",
            Keywords = keywords
        };

        private static KnowledgeEntry Faq(string id, string titleEn, string titleAr,
            string ansEn, string ansAr, params string[] keywords) => new()
        {
            Id = id, Category = "faq", TitleEn = titleEn, TitleAr = titleAr,
            AnswerEn = ansEn, AnswerAr = ansAr, Keywords = keywords
        };

        private static bool ContainsAny(string text, params string[] parts) =>
            parts.Any(p => text.Contains(p, StringComparison.Ordinal));

        private static bool IsArabic(string lang) =>
            !string.IsNullOrWhiteSpace(lang) &&
            lang.StartsWith("Ar", StringComparison.OrdinalIgnoreCase);
    }
}
