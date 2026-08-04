using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WebApplication2.cls
{
    public static class clsAiChatGuides
    {
        public sealed class ErpGuide
        {
            public string Id { get; init; } = "";
            public string TitleEn { get; init; } = "";
            public string TitleAr { get; init; } = "";
            public string MenuPathEn { get; init; } = "";
            public string MenuPathAr { get; init; } = "";
            public string[] Keywords { get; init; } = Array.Empty<string>();
            public string[] StepsEn { get; init; } = Array.Empty<string>();
            public string[] StepsAr { get; init; } = Array.Empty<string>();
        }

        private static readonly ErpGuide[] Guides =
        {
            new ErpGuide
            {
                Id = "sales_invoice",
                TitleEn = "Create a Sales Invoice",
                TitleAr = "إنشاء فاتورة مبيعات",
                MenuPathEn = "Menu → Inventory → Sales Invoice",
                MenuPathAr = "القائمة → المخزون → فاتورة مبيعات",
                Keywords = new[] { "sales invoice", "sell invoice", "create invoice", "new invoice", "فاتورة مبيعات", "فاتورة بيع", "انشاء فاتورة", "إنشاء فاتورة" },
                StepsEn = new[]
                {
                    "Open the main menu and go to Inventory.",
                    "Select Sales Invoice.",
                    "Click Create / New to open a blank invoice.",
                    "Set invoice date, customer, branch, and currency.",
                    "Add items, quantities, and prices in the grid.",
                    "Review totals and click Save."
                },
                StepsAr = new[]
                {
                    "افتح القائمة الرئيسية ثم ادخل إلى المخزون.",
                    "اختر فاتورة مبيعات.",
                    "اضغط إنشاء / جديد لفتح فاتورة فارغة.",
                    "حدد التاريخ والعميل والفرع والعملة.",
                    "أضف الأصناف والكميات والأسعار في الجدول.",
                    "راجع الإجماليات ثم احفظ."
                }
            },
            new ErpGuide
            {
                Id = "purchase_invoice",
                TitleEn = "Create a Purchase Invoice",
                TitleAr = "إنشاء فاتورة مشتريات",
                MenuPathEn = "Menu → Inventory → Purchase Invoice",
                MenuPathAr = "القائمة → المخزون → فاتورة مشتريات",
                Keywords = new[] { "purchase invoice", "buy invoice", "supplier invoice", "فاتورة مشتريات", "فاتورة شراء", "مورد" },
                StepsEn = new[]
                {
                    "Open the main menu and go to Inventory.",
                    "Select Purchase Invoice.",
                    "Click Create / New.",
                    "Set invoice date, supplier, branch, and currency.",
                    "Add purchased items and costs.",
                    "Save the invoice."
                },
                StepsAr = new[]
                {
                    "افتح القائمة الرئيسية ثم ادخل إلى المخزون.",
                    "اختر فاتورة مشتريات.",
                    "اضغط إنشاء / جديد.",
                    "حدد التاريخ والمورد والفرع والعملة.",
                    "أضف الأصناف المشتراة والتكاليف.",
                    "احفظ الفاتورة."
                }
            },
            new ErpGuide
            {
                Id = "journal_voucher",
                TitleEn = "Create a Journal Voucher",
                TitleAr = "إنشاء سند قيد",
                MenuPathEn = "Menu → Accounting → Journal Voucher",
                MenuPathAr = "القائمة → المحاسبة → سند قيد",
                Keywords = new[] { "journal voucher", "jv", "journal entry", "قيد", "سند قيد", "قيد يومية" },
                StepsEn = new[]
                {
                    "Open the main menu and go to Accounting.",
                    "Select Journal Voucher.",
                    "Click Create / New voucher.",
                    "Enter voucher date, branch, cost center, and notes.",
                    "Add debit and credit lines until balanced.",
                    "Save the journal voucher."
                },
                StepsAr = new[]
                {
                    "افتح القائمة الرئيسية ثم ادخل إلى المحاسبة.",
                    "اختر سند قيد.",
                    "اضغط إنشاء سند جديد.",
                    "أدخل التاريخ والفرع ومركز التكلفة والملاحظات.",
                    "أضف بنود مدين ودائن حتى يتوازن السند.",
                    "احفظ سند القيد."
                }
            },
            new ErpGuide
            {
                Id = "payment_voucher",
                TitleEn = "Create a Payment Voucher",
                TitleAr = "إنشاء سند صرف",
                MenuPathEn = "Menu → Accounting → Payment Voucher",
                MenuPathAr = "القائمة → المحاسبة → سند صرف",
                Keywords = new[] { "payment voucher", "pay voucher", "cash payment", "سند صرف", "صرف", "دفع" },
                StepsEn = new[]
                {
                    "Open Accounting from the main menu.",
                    "Select Payment Voucher.",
                    "Create a new voucher.",
                    "Choose cash/bank account, date, branch, and cost center.",
                    "Fill the grid with accounts and amounts.",
                    "Save the voucher."
                },
                StepsAr = new[]
                {
                    "افتح المحاسبة من القائمة الرئيسية.",
                    "اختر سند صرف.",
                    "أنشئ سنداً جديداً.",
                    "اختر حساب الصندوق/البنك والتاريخ والفرع ومركز التكلفة.",
                    "عبئ الجدول بالحسابات والمبالغ.",
                    "احفظ السند."
                }
            },
            new ErpGuide
            {
                Id = "receivable_voucher",
                TitleEn = "Create a Receivable Voucher",
                TitleAr = "إنشاء سند قبض",
                MenuPathEn = "Menu → Accounting → Receivable Voucher",
                MenuPathAr = "القائمة → المحاسبة → سند قبض",
                Keywords = new[] { "receivable voucher", "receipt voucher", "cash receipt", "سند قبض", "قبض" },
                StepsEn = new[]
                {
                    "Open Accounting from the main menu.",
                    "Select Receivable Voucher.",
                    "Create a new voucher.",
                    "Choose cash/bank account, date, branch, and cost center.",
                    "Fill the grid with customer/account lines.",
                    "Save the voucher."
                },
                StepsAr = new[]
                {
                    "افتح المحاسبة من القائمة الرئيسية.",
                    "اختر سند قبض.",
                    "أنشئ سنداً جديداً.",
                    "اختر حساب الصندوق/البنك والتاريخ والفرع ومركز التكلفة.",
                    "عبئ الجدول ببنود العملاء/الحسابات.",
                    "احفظ السند."
                }
            },
            new ErpGuide
            {
                Id = "credit_note",
                TitleEn = "Create a Credit Note",
                TitleAr = "إنشاء إشعار دائن",
                MenuPathEn = "Menu → Accounting → Credit Note",
                MenuPathAr = "القائمة → المحاسبة → إشعار دائن",
                Keywords = new[] { "credit note", "اشعار دائن", "إشعار دائن" },
                StepsEn = new[]
                {
                    "Open Accounting from the main menu.",
                    "Select Credit Note.",
                    "Create a new note.",
                    "Set date, branch, cost center, and account.",
                    "Enter amount and supporting lines.",
                    "Save the credit note."
                },
                StepsAr = new[]
                {
                    "افتح المحاسبة من القائمة الرئيسية.",
                    "اختر إشعار دائن.",
                    "أنشئ إشعاراً جديداً.",
                    "حدد التاريخ والفرع ومركز التكلفة والحساب.",
                    "أدخل المبلغ والبنود.",
                    "احفظ الإشعار."
                }
            },
            new ErpGuide
            {
                Id = "reconciliation",
                TitleEn = "Run Account Reconciliation",
                TitleAr = "تسوية الحسابات",
                MenuPathEn = "Menu → Accounting → Reconciliations",
                MenuPathAr = "القائمة → المحاسبة → التسويات",
                Keywords = new[] { "reconciliation", "reconcile", "match", "تسوية", "مطابقة" },
                StepsEn = new[]
                {
                    "Open Accounting from the main menu.",
                    "Select Reconciliations.",
                    "Create a new reconciliation or open manual reconciliation.",
                    "Select the account and unmatched amounts.",
                    "Match debit/credit entries together.",
                    "Save / run reconciliation."
                },
                StepsAr = new[]
                {
                    "افتح المحاسبة من القائمة الرئيسية.",
                    "اختر التسويات.",
                    "أنشئ تسوية جديدة أو افتح التسوية اليدوية.",
                    "اختر الحساب والمبالغ غير المطابقة.",
                    "طابق القيود المدينة والدائنة.",
                    "احفظ / نفّذ التسوية."
                }
            },
            new ErpGuide
            {
                Id = "payroll",
                TitleEn = "Process Payroll",
                TitleAr = "معالجة الرواتب",
                MenuPathEn = "Menu → HR → Payroll",
                MenuPathAr = "القائمة → الموارد البشرية → الرواتب",
                Keywords = new[] { "payroll", "salary", "salaries", "wages", "run payroll", "process payroll", "راتب", "رواتب", "الرواتب", "تشغيل الرواتب" },
                StepsEn = new[]
                {
                    "Open HR from the main menu.",
                    "Go to Payroll and select the payroll period.",
                    "Review employee salary elements and attendance if needed.",
                    "Run payroll calculation / preview.",
                    "Post or save payroll when totals are correct."
                },
                StepsAr = new[]
                {
                    "افتح الموارد البشرية من القائمة الرئيسية.",
                    "ادخل إلى الرواتب واختر فترة الرواتب.",
                    "راجع عناصر الراتب والحضور إن لزم.",
                    "نفّذ حساب / معاينة الرواتب.",
                    "رحّل أو احفظ الرواتب بعد التأكد من الإجماليات."
                }
            },
            new ErpGuide
            {
                Id = "attendance",
                TitleEn = "Manage Attendance",
                TitleAr = "إدارة الحضور",
                MenuPathEn = "Menu → HR → Attendance",
                MenuPathAr = "القائمة → الموارد البشرية → الحضور",
                Keywords = new[] { "attendance", "punch", "time clock", "حضور", "انصراف", "بصمة" },
                StepsEn = new[]
                {
                    "Open HR from the main menu.",
                    "Go to Attendance or Attendance Machines.",
                    "Import/sync punches if using biometric devices.",
                    "Review daily attendance and exceptions.",
                    "Recalculate attendance for the selected period if needed."
                },
                StepsAr = new[]
                {
                    "افتح الموارد البشرية من القائمة الرئيسية.",
                    "ادخل إلى الحضور أو أجهزة البصمة.",
                    "استورد/زامن البصمات إن كنت تستخدم أجهزة.",
                    "راجع الحضور اليومي والاستثناءات.",
                    "أعد حساب الحضور للفترة المطلوبة إن لزم."
                }
            },
            new ErpGuide
            {
                Id = "reports",
                TitleEn = "Open Financial Reports",
                TitleAr = "فتح التقارير المالية",
                MenuPathEn = "Menu → Accounting → Reports",
                MenuPathAr = "القائمة → المحاسبة → التقارير",
                Keywords = new[] { "report", "reports", "trial balance", "balance sheet", "statement", "تقرير", "تقارير", "ميزان", "كشف حساب" },
                StepsEn = new[]
                {
                    "Open Accounting from the main menu.",
                    "Select Reports.",
                    "Choose the report type (Trial Balance, Account Statement, etc.).",
                    "Set date range, accounts, and filters.",
                    "Preview or export the report."
                },
                StepsAr = new[]
                {
                    "افتح المحاسبة من القائمة الرئيسية.",
                    "اختر التقارير.",
                    "حدد نوع التقرير (ميزان مراجعة، كشف حساب، إلخ).",
                    "حدد الفترة والحسابات والفلاتر.",
                    "اعرض أو صدّر التقرير."
                }
            },
            new ErpGuide
            {
                Id = "trial_balance",
                TitleEn = "Run Trial Balance Report",
                TitleAr = "تشغيل ميزان المراجعة",
                MenuPathEn = "Menu → Accounting → Trial Balance",
                MenuPathAr = "القائمة → المحاسبة → ميزان المراجعة",
                Keywords = new[] { "trial balance", "tb report", "run trial balance", "ميزان مراجعة", "ميزان" },
                StepsEn = new[] { "Open Accounting.", "Select Trial Balance.", "Set date range and branch filters.", "Run the report.", "Export or print if needed." },
                StepsAr = new[] { "افتح المحاسبة.", "اختر ميزان المراجعة.", "حدد الفترة والفرع.", "شغّل التقرير.", "صدّر أو اطبع إن لزم." }
            },
            new ErpGuide
            {
                Id = "good_issue",
                TitleEn = "Create Good Issue",
                TitleAr = "صرف بضاعة",
                MenuPathEn = "Menu → Inventory → Good Issue",
                MenuPathAr = "القائمة → المخزون → صرف بضاعة",
                Keywords = new[] { "good issue", "issue stock", "stock issue", "صرف بضاعة", "صرف مخزون" },
                StepsEn = new[] { "Open Inventory.", "Select Good Issue.", "Create new document.", "Select warehouse/branch and items.", "Enter quantities and save." },
                StepsAr = new[] { "افتح المخزون.", "اختر صرف بضاعة.", "أنشئ مستنداً جديداً.", "اختر المستودع/الفرع والأصناف.", "أدخل الكميات واحفظ." }
            },
            new ErpGuide
            {
                Id = "pos_sales",
                TitleEn = "Use POS",
                TitleAr = "استخدام نقطة البيع",
                MenuPathEn = "Main menu → POS",
                MenuPathAr = "القائمة → POS",
                Keywords = new[] { "pos", "point of sale", "cashier", "retail sale", "نقطة بيع", "كاشير", "pos sale" },
                StepsEn = new[] { "Open POS from main menu.", "Select customer (optional) and branch.", "Scan or pick items.", "Choose payment method.", "Complete sale and print receipt." },
                StepsAr = new[] { "افتح POS من القائمة.", "اختر العميل (اختياري) والفرع.", "امسح أو اختر الأصناف.", "اختر طريقة الدفع.", "أتمم البيع واطبع الإيصال." }
            },
            new ErpGuide
            {
                Id = "employee_contract",
                TitleEn = "Create Employee Contract",
                TitleAr = "إنشاء عقد موظف",
                MenuPathEn = "Menu → Human Resources → Make Contract",
                MenuPathAr = "القائمة → الموارد البشرية → عقد موظف",
                Keywords = new[] { "employee contract", "hire employee", "contract", "عقد", "عقد موظف", "توظيف" },
                StepsEn = new[] { "Open Human Resources.", "Select Make Contract.", "Choose employee or create new.", "Set contract type, dates, salary elements.", "Save and activate contract." },
                StepsAr = new[] { "افتح الموارد البشرية.", "اختر عقد موظف.", "اختر الموظف أو أنشئ جديد.", "حدد نوع العقد والتواريخ وعناصر الراتب.", "احفظ وفعّل العقد." }
            },
            new ErpGuide
            {
                Id = "financing_loan",
                TitleEn = "Create Financing / Loan",
                TitleAr = "إنشاء تمويل / قرض",
                MenuPathEn = "Main menu → Financing",
                MenuPathAr = "القائمة → التمويل",
                Keywords = new[] { "financing", "loan", "installment", "credit sale", "تمويل", "قرض", "تقسيط" },
                StepsEn = new[] { "Open Financing from main menu.", "Create new financing/loan contract.", "Select customer and loan type.", "Enter amount, installments, and schedule.", "Save and generate installments." },
                StepsAr = new[] { "افتح التمويل من القائمة.", "أنشئ عقد تمويل/قرض.", "اختر العميل ونوع القرض.", "أدخل المبلغ والأقساط والجدول.", "احفظ وأنشئ جدول الأقساط." }
            },
        };

        public static ErpGuide GetById(string id)
        {
            if (string.IsNullOrWhiteSpace(id)) return null;
            return Guides.FirstOrDefault(g =>
                g.Id.Equals(id, StringComparison.OrdinalIgnoreCase));
        }

        public static ErpGuide MatchGuide(string normalizedMessage)
        {
            if (string.IsNullOrWhiteSpace(normalizedMessage)) return null;

            ErpGuide best = null;
            int bestScore = 0;

            foreach (ErpGuide guide in Guides)
            {
                int score = 0;
                foreach (string keyword in guide.Keywords)
                {
                    if (normalizedMessage.Contains(keyword, StringComparison.Ordinal))
                        score += keyword.Contains(' ') ? 4 : 2;
                }

                if (score > bestScore)
                {
                    bestScore = score;
                    best = guide;
                }
            }

            return bestScore >= 2 ? best : null;
        }

        public static IReadOnlyList<ErpGuide> SearchGuides(string normalizedMessage, int max = 5)
        {
            var scored = new List<(ErpGuide guide, int score)>();

            foreach (ErpGuide guide in Guides)
            {
                int score = 0;
                foreach (string keyword in guide.Keywords)
                {
                    if (normalizedMessage.Contains(keyword, StringComparison.Ordinal))
                        score += keyword.Contains(' ') ? 4 : 2;
                }

                foreach (string word in normalizedMessage.Split(' ', StringSplitOptions.RemoveEmptyEntries))
                {
                    if (word.Length < 3) continue;
                    if (guide.TitleEn.Contains(word, StringComparison.OrdinalIgnoreCase) ||
                        guide.TitleAr.Contains(word, StringComparison.Ordinal))
                        score++;
                }

                if (score > 0)
                    scored.Add((guide, score));
            }

            return scored
                .OrderByDescending(x => x.score)
                .Take(max)
                .Select(x => x.guide)
                .ToList();
        }

        public static string FormatGuide(ErpGuide guide, string lang)
        {
            bool ar = IsArabic(lang);
            string title = ar ? guide.TitleAr : guide.TitleEn;
            string path = ar ? guide.MenuPathAr : guide.MenuPathEn;
            string[] steps = ar ? guide.StepsAr : guide.StepsEn;

            var sb = new StringBuilder();
            sb.AppendLine(ar ? $"📋 {title}" : $"📋 {title}");
            sb.AppendLine(ar ? $"📍 المسار: {path}" : $"📍 Path: {path}");
            sb.AppendLine(ar ? "الخطوات:" : "Steps:");
            for (int i = 0; i < steps.Length; i++)
                sb.AppendLine($"{i + 1}. {steps[i]}");

            sb.AppendLine();
            sb.AppendLine(ar
                ? "💡 يمكنك أيضاً استخدام زر المساعدة (?) في الشاشة لتفعيل الدليل التفاعلي."
                : "💡 You can also use the help button (?) on screen for interactive guided tours.");

            return sb.ToString().TrimEnd();
        }

        public static string ListGuideTopics(string lang)
        {
            bool ar = IsArabic(lang);
            var lines = new List<string>
            {
                ar ? "يمكنني إرشادك في:" : "I can guide you with:"
            };

            foreach (ErpGuide guide in Guides)
                lines.Add("• " + (ar ? guide.TitleAr : guide.TitleEn));

            return string.Join("\n", lines);
        }

        private static bool IsArabic(string lang) =>
            !string.IsNullOrWhiteSpace(lang) &&
            lang.StartsWith("Ar", StringComparison.OrdinalIgnoreCase);
    }
}
