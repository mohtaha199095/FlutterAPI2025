using System;
using System.Linq;
using System.Text.RegularExpressions;

namespace WebApplication2.cls
{
    public static class clsAiChatLanguage
    {
        /// <summary>Resolve effective language: auto-detect from message or use app hint.</summary>
        public static string Resolve(string message, string langHint)
        {
            if (!string.IsNullOrWhiteSpace(langHint) &&
                !langHint.Equals("auto", StringComparison.OrdinalIgnoreCase))
            {
                return langHint.StartsWith("Ar", StringComparison.OrdinalIgnoreCase) ? "Ar" : "En";
            }

            return DetectFromText(message);
        }

        public static string DetectFromText(string text)
        {
            if (string.IsNullOrWhiteSpace(text)) return "En";

            int arabic = 0;
            int latin = 0;
            foreach (char c in text)
            {
                if (c >= '\u0600' && c <= '\u06FF') arabic++;
                else if ((c >= 'A' && c <= 'Z') || (c >= 'a' && c <= 'z')) latin++;
            }

            if (arabic > latin) return "Ar";
            if (arabic > 0 && latin > 0) return "Ar"; // mixed → prefer Arabic if significant
            return "En";
        }

        public static bool IsArabic(string lang) =>
            !string.IsNullOrWhiteSpace(lang) &&
            lang.StartsWith("Ar", StringComparison.OrdinalIgnoreCase);

        /// <summary>Human-readable label for the LLM system prompt.</summary>
        public static string DescribeForPrompt(string message, string lang)
        {
            if (IsArabic(lang)) return "Arabic";
            if (string.IsNullOrWhiteSpace(message)) return "English";

            string lower = message.ToLowerInvariant();
            if (Regex.IsMatch(message, @"[\u0600-\u06FF]")) return "Arabic";
            if (Regex.IsMatch(message, @"[\u0400-\u04FF]")) return "Russian/Cyrillic";
            if (Regex.IsMatch(message, @"[\u4e00-\u9fff]")) return "Chinese";

            string[] french = { "combien", "comment", "bonjour", "merci", "facture", "client", "ventes", "employé" };
            if (french.Any(w => lower.Contains(w))) return "French";

            string[] spanish = { "cuántos", "como", "hola", "gracias", "factura", "cliente", "ventas", "empleado" };
            if (spanish.Any(w => lower.Contains(w))) return "Spanish";

            string[] german = { "wie viele", "hallo", "danke", "rechnung", "kunde", "verkauf", "mitarbeiter" };
            if (german.Any(w => lower.Contains(w))) return "German";

            return "English (or match the user's language from their message)";
        }

        public static string NotConfiguredMessage(string lang) =>
            IsArabic(lang)
                ? "🤖 **وضع المساعد الأساسي** (بدون ذكاء اصطناعي)\n\n" +
                  "لتفعيل محادثة ذكية مثل ChatGPT تفهم أي سؤال وترد بلغتك:\n" +
                  "1. عيّن متغير البيئة `AiChat__ApiKey` (مفضل) أو عدّل `appsettings.json`\n" +
                  "2. عيّن `AiChat:Enabled` = true\n" +
                  "3. أو استخدم **Ollama** محلياً: Provider=Ollama, BaseUrl=http://localhost:11434/v1\n\n" +
                  "حالياً يمكنني الإجابة على أسئلة محددة عن البيانات والإرشاد. جرّب: «كم عدد العملاء؟»"
                : "🤖 **Basic assistant mode** (no AI model connected)\n\n" +
                  "For ChatGPT-like conversation that understands any question in your language:\n" +
                  "1. Set environment variable `AiChat__ApiKey` (recommended) OR edit `appsettings.json`\n" +
                  "2. Set `AiChat:Enabled` = true\n" +
                  "3. Or use local **Ollama**: Provider=Ollama, BaseUrl=http://localhost:11434/v1\n\n" +
                  "I can still answer specific data and guide questions. Try: «how many customers?»";
    }
}
