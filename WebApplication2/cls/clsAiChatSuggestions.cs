using System;
using System.Collections.Generic;
using System.Linq;

namespace WebApplication2.cls
{
    public static class clsAiChatSuggestions
    {
        public static IReadOnlyList<string> GetWelcome(string lang, bool aiEnabled)
        {
            if (clsAiChatLanguage.IsArabic(lang))
            {
                return new[]
                {
                    "كم عدد العملاء؟",
                    "مبيعات هذا الشهر",
                    "كيف أنشئ فاتورة مبيعات؟",
                    "أين ميزان المراجعة؟",
                    "ابحث عن عميل",
                    "أفضل 5 أصناف مبيعاً"
                };
            }

            return new[]
            {
                "How many customers?",
                "Sales this month",
                "How to create sales invoice?",
                "Where is trial balance?",
                "Find a customer",
                "Top 5 selling items"
            };
        }

        public static IReadOnlyList<string> GetFollowUp(string lang, string mode, string userMessage, IReadOnlyList<string> toolsUsed)
        {
            bool ar = clsAiChatLanguage.IsArabic(lang);
            string norm = clsAiChat.NormalizePublic(userMessage ?? "");

            if (toolsUsed?.Contains("query_erp_data") == true)
            {
                return ar
                    ? new[] { "مبيعات هذا الشهر", "أفضل الأصناف", "كم عدد الفواتير؟", "إرشاد تقرير المبيعات" }
                    : new[] { "Sales this month", "Top items", "How many invoices?", "Sales report guide" };
            }

            if (toolsUsed?.Contains("get_erp_guide") == true || norm.Contains("how") || norm.Contains("كيف"))
            {
                return ar
                    ? new[] { "كيف سند قيد؟", "كيف فاتورة مشتريات؟", "أين شاشة الرواتب؟", "كم عدد الموظفين؟" }
                    : new[] { "Journal voucher steps?", "Purchase invoice?", "Where is payroll?", "How many employees?" };
            }

            if (toolsUsed?.Contains("get_system_knowledge") == true || norm.Contains("where") || norm.Contains("أين"))
            {
                return ar
                    ? new[] { "أين قائمة الدخل؟", "أين المخزون؟", "كيف أنشئ فاتورة؟", "كم إيرادات اليوم؟" }
                    : new[] { "Where is income statement?", "Where is inventory?", "How to create invoice?", "Today's revenue?" };
            }

            if (norm.Contains("customer") || norm.Contains("عميل") || norm.Contains("client"))
            {
                return ar
                    ? new[] { "كم عدد العملاء؟", "ابحث عن مورد", "مبيعات هذا الشهر" }
                    : new[] { "How many customers?", "Find a vendor", "Sales this month" };
            }

            return GetWelcome(lang, mode == "ai").Take(4).ToList();
        }
    }
}
