using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WebApplication2.cls
{
    public sealed class clsAiChatAgent
    {
        private readonly clsAiChat _data = new();
        private readonly clsAiChatLlm _llm;

        public clsAiChatAgent(IConfiguration configuration)
        {
            _llm = new clsAiChatLlm(configuration);
        }

        public async Task<AiChatAgentResult> ProcessAsync(string sessionId, string message, int companyId, string lang)
        {
            AiChatSession session = AiChatSessionStore.GetOrCreate(sessionId);
            string input = (message ?? "").Trim();

            if (string.IsNullOrWhiteSpace(input))
            {
                string emptyLang = clsAiChatLanguage.Resolve(input, lang);
                return AiChatAgentResult.Of(EmptyPrompt(emptyLang), "rules", emptyLang);
            }

            string effectiveLang = clsAiChatLanguage.Resolve(input, lang);
            session.AddUser(input);

            string normalized = clsAiChatKnowledge.RewriteWithSynonyms(clsAiChat.NormalizePublic(input));

            // Pending name confirmations must run before AI (yes/no/number picks)
            if (!string.IsNullOrWhiteSpace(session.PendingAction))
            {
                if (clsAiChatNameSearch.TryAbandonPendingForNewRequest(session, normalized, input))
                {
                    if (clsAiChatNameSearch.TryExtractNamedBalanceQuery(normalized, input, out string searchTerm) && companyId > 0)
                    {
                        string searchReply = _data.SearchEverywherePublic(searchTerm, companyId, effectiveLang, session);
                        session.AddAssistant(searchReply);
                        return AiChatAgentResult.Of(
                            searchReply,
                            _llm.IsConfigured ? "ai" : "rules",
                            effectiveLang,
                            clsAiChatSuggestions.GetFollowUp(effectiveLang, _llm.IsConfigured ? "ai" : "rules", input, new List<string> { "search_by_name" }));
                    }
                    // No name yet — fall through to AI/rules with pending cleared.
                }
                else
                {
                    string pendingReply = HandlePendingChoice(session, normalized, input, companyId, effectiveLang);
                    session.AddAssistant(pendingReply);
                    return AiChatAgentResult.Of(
                        pendingReply,
                        _llm.IsConfigured ? "ai" : "rules",
                        effectiveLang,
                        clsAiChatSuggestions.GetFollowUp(effectiveLang, _llm.IsConfigured ? "ai" : "rules", input, null));
                }
            }

            string reply;
            string mode;
            var toolsUsed = new List<string>();

            if (_llm.IsConfigured)
            {
                try
                {
                    var llmResult = await _llm.ChatAsync(
                        session,
                        input,
                        companyId,
                        effectiveLang,
                        RunDataQueryWithContext,
                        (term, cid, lng) => _data.SearchBusinessPartnersPublic(term, cid, lng, session, true),
                        (term, cid, lng) => _data.SearchEverywherePublic(term, cid, lng, session, true),
                        GetGuideById);

                    if (llmResult.Success || !IsLlmHardFailure(llmResult.Content))
                    {
                        reply = llmResult.Content;
                        mode = "ai";
                        toolsUsed = llmResult.ToolsUsed;
                    }
                    else
                    {
                        reply = ProcessConversational(session, input, companyId, effectiveLang);
                        mode = "rules";
                    }
                }
                catch (Exception)
                {
                    reply = ProcessConversational(session, input, companyId, effectiveLang);
                    mode = "rules";
                    reply += clsAiChatLanguage.IsArabic(effectiveLang)
                        ? "\n\n⚠️ الذكاء الاصطناعي غير متاح حالياً — تم استخدام الوضع الأساسي."
                        : "\n\n⚠️ AI is temporarily unavailable — using basic mode.";
                }
            }
            else
            {
                reply = ProcessConversational(session, input, companyId, effectiveLang);
                mode = "rules";

                if (!session.AiModeHintShown)
                {
                    session.AiModeHintShown = true;
                    reply = clsAiChatLanguage.NotConfiguredMessage(effectiveLang) + "\n\n---\n\n" + reply;
                }
            }

            session.AddAssistant(reply);
            var suggestions = clsAiChatSuggestions.GetFollowUp(effectiveLang, mode, input, toolsUsed);
            return AiChatAgentResult.Of(reply, mode, effectiveLang, suggestions, toolsUsed);
        }

        public AiChatWelcomeResult GetWelcome(int companyId, string lang)
        {
            string effectiveLang = clsAiChatLanguage.Resolve("", lang);
            bool ai = _llm.IsConfigured;
            string text = ai
                ? (IsArabic(effectiveLang)
                    ? "مرحباً! 👋 أنا مساعد MT SOFTS الذكي.\n\nتحدث معي بأي لغة — سأفهم طلبك وأنفّذه: بيانات حية، إرشاد خطوة بخطوة، أو مساعدة في التنقل."
                    : "Hello! 👋 I'm your MT SOFTS AI assistant.\n\nTalk in any language — I'll understand, fetch live data, guide you step by step, or help you navigate.")
                : (IsArabic(effectiveLang)
                    ? "مرحباً! 👋 أنا مساعد MT SOFTS.\n\nاسأل عن بيانات شركتك أو اطلب إرشاداً. فعّل AiChat في إعدادات الـ API للمحادثة الذكية."
                    : "Hello! 👋 I'm the MT SOFTS assistant.\n\nAsk about your company data or request guidance. Enable AiChat in API settings for smart conversation.");

            if (companyId <= 0)
            {
                text += IsArabic(effectiveLang)
                    ? "\n\n⚠️ سجّل الدخول واختر شركة لجلب البيانات."
                    : "\n\n⚠️ Log in and select a company to fetch live data.";
            }

            return new AiChatWelcomeResult
            {
                Text = text,
                Mode = ai ? "ai" : "rules",
                Lang = effectiveLang,
                Suggestions = clsAiChatSuggestions.GetWelcome(effectiveLang, ai).ToList()
            };
        }

        private string ProcessConversational(AiChatSession session, string message, int companyId, string lang)
        {
            string normalized = clsAiChatKnowledge.RewriteWithSynonyms(clsAiChat.NormalizePublic(message));
            string contextualQuery = BuildContextualQuery(session, message, lang);

            if (!string.IsNullOrWhiteSpace(session.PendingAction))
                return HandlePendingChoice(session, normalized, message, companyId, lang);

            if (IsGreeting(normalized))
                return ConversationalGreeting(lang);

            if (IsHelpRequest(normalized) || clsAiChatKnowledge.LooksLikeModuleQuestion(normalized))
                return BuildCapabilitiesMessage(lang);

            int guideScore = ScoreGuideIntent(normalized);
            int dataScore = ScoreDataIntent(normalized);

            // How-to questions → guides first (before navigation KB)
            if (LooksLikeGuideRequest(normalized, guideScore, dataScore))
            {
                var guide = clsAiChatGuides.MatchGuide(normalized);
                if (guide != null)
                    return clsAiChatGuides.FormatGuide(guide, lang);

                var suggestions = clsAiChatGuides.SearchGuides(normalized, 3);
                if (suggestions.Count > 0)
                {
                    session.PendingAction = "pick_guide";
                    session.PendingOptions = suggestions.Select(g => g.Id).ToList();
                    return BuildGuidePicker(suggestions, lang);
                }
            }

            // System knowledge: navigation & concepts (not how-to)
            if (clsAiChatKnowledge.LooksLikeNavigationQuestion(normalized) ||
                clsAiChatKnowledge.LooksLikeConceptQuestion(normalized))
            {
                var knowledge = clsAiChatKnowledge.SearchBest(normalized);
                if (knowledge != null)
                    return clsAiChatKnowledge.FormatEntry(knowledge, lang);
            }

            // Try knowledge search for any partial match before giving up
            var knowledgeFallback = clsAiChatKnowledge.SearchBest(normalized);
            if (knowledgeFallback != null && ScoreKnowledgeMatch(normalized) >= 4)
                return clsAiChatKnowledge.FormatEntry(knowledgeFallback, lang);

            if (IsFollowUp(normalized) && !string.IsNullOrWhiteSpace(session.LastDataTopic))
            {
                string expanded = ExpandFollowUp(session.LastDataTopic, normalized, lang);
                string followReply = TryAnswerData(expanded, companyId, lang, session);
                if (followReply != null) return followReply;
            }

            // Use conversation context for short/vague messages
            if (normalized.Split(' ').Length <= 5 && session.History.Count >= 2)
            {
                string dataReply = TryAnswerData(contextualQuery, companyId, lang, session);
                if (dataReply != null) return dataReply;
            }

            if (companyId <= 0)
            {
                return IsArabic(lang)
                    ? "يرجى تسجيل الدخول واختيار شركة. بعد ذلك يمكنني جلب بياناتك أو إرشادك خطوة بخطوة."
                    : "Please log in and select a company. Then I can fetch your data or guide you step by step.";
            }

            if (clsAiChatNameSearch.TryExtractNamedBalanceQuery(normalized, message, out string balanceName))
                return _data.SearchEverywherePublic(balanceName, companyId, lang, session);

            if (clsAiChatNameSearch.LooksLikeChartOfAccountsRequest(normalized, message))
            {
                string term = clsAiChatNameSearch.ExtractGlAccountSearchTerm(message);
                if (term.Length >= 2)
                    return _data.SearchEverywherePublic(term, companyId, lang, session);
            }

            if (guideScore > 0 && dataScore > 0 && Math.Abs(guideScore - dataScore) <= 3)
            {
                string topic = DetectTopicLabel(normalized, lang);
                session.PendingAction = "guide_or_data";
                session.PendingTopic = topic;
                session.PendingOptions = new List<string> { "guide", "data" };
                return AskGuideOrData(topic, lang);
            }

            string directDataReply = TryAnswerData(contextualQuery, companyId, lang, session);
            if (directDataReply != null)
                return directDataReply;

            var fallbackGuide = clsAiChatGuides.MatchGuide(normalized);
            if (fallbackGuide != null)
                return clsAiChatGuides.FormatGuide(fallbackGuide, lang);

            // Suggest related knowledge entries
            var related = clsAiChatKnowledge.SearchAll(normalized, 3);
            if (related.Count > 0)
                return BuildKnowledgeSuggestions(related, normalized, lang);

            return BuildClarifyingQuestion(normalized, lang);
        }

        private static string BuildContextualQuery(AiChatSession session, string currentMessage, string lang)
        {
            string current = clsAiChatKnowledge.RewriteWithSynonyms(clsAiChat.NormalizePublic(currentMessage));
            if (session.History.Count < 2)
                return currentMessage;

            // Last user message before current (History already has current added)
            string lastUser = session.History
                .Where(m => m.Role == "user")
                .Reverse()
                .Skip(1)
                .FirstOrDefault()?.Content ?? "";

            if (string.IsNullOrWhiteSpace(lastUser)) return currentMessage;

            string lastNorm = clsAiChatKnowledge.RewriteWithSynonyms(clsAiChat.NormalizePublic(lastUser));

            // Short follow-up: combine with previous topic
            if (current.Split(' ').Length <= 4)
            {
                if (ContainsAny(current, "this month", "today", "yesterday", "top", "best", "recent", "total", "count",
                    "الشهر", "اليوم", "أمس", "أفضل", "آخر", "كم", "عدد", "اجمالي", "إجمالي"))
                {
                    return lastUser + " " + currentMessage;
                }
            }

            // Pronoun / continuation
            if (ContainsAny(current, "same", "also", "and", "what about", "how about", "more",
                "ايضا", "أيضا", "كذلك", "وماذا", "طيب", "نفس"))
                return lastUser + " — " + currentMessage;

            return currentMessage;
        }

        private static int ScoreKnowledgeMatch(string normalized)
        {
            var results = clsAiChatKnowledge.SearchAll(normalized, 1);
            return results.Count > 0 ? 4 : 0;
        }

        private static string BuildKnowledgeSuggestions(IReadOnlyList<clsAiChatKnowledge.KnowledgeEntry> entries, string normalized, string lang)
        {
            bool ar = IsArabic(lang);
            var lines = new List<string>
            {
                ar ? "ربما تقصد أحد هذه المواضيع:" : "You might be looking for one of these:"
            };

            for (int i = 0; i < entries.Count; i++)
                lines.Add($"{i + 1}. " + (ar ? entries[i].TitleAr : entries[i].TitleEn));

            lines.Add(ar
                ? "اكتب رقم الخيار أو اسأل بشكل أوضح (مثال: «أين ميزان المراجعة؟» أو «كم مبيعات الشهر؟»)."
                : "Reply with the number or ask more specifically (e.g. «where is trial balance?» or «this month sales?»).");

            return string.Join("\n", lines);
        }

        private string HandlePendingChoice(AiChatSession session, string normalized, string rawMessage, int companyId, string lang)
        {
            switch (session.PendingAction)
            {
                case "guide_or_data":
                {
                    bool wantsGuide = ContainsAny(normalized, "guide", "how", "steps", "help me", "show me how", "كيف", "خطوات", "دليل", "1", "first", "option 1");
                    bool wantsData = ContainsAny(normalized, "data", "number", "count", "total", "statistics", "stats", "report", "كم", "عدد", "إجمالي", "2", "second", "option 2");

                    if (wantsGuide && !wantsData)
                    {
                        session.ClearPending();
                        return ProcessGuideForTopic(session.PendingTopic, normalized, lang);
                    }

                    if (wantsData && !wantsGuide)
                    {
                        session.ClearPending();
                        string topic = session.PendingTopic;
                        session.ClearPending();
                        string q = BuildDataQuestionFromTopic(topic, rawMessage, lang);
                        string reply = TryAnswerData(q, companyId, lang, session);
                        return reply ?? BuildClarifyingQuestion(normalized, lang);
                    }

                    return AskGuideOrData(session.PendingTopic, lang);
                }
                case "pick_guide":
                {
                    var guides = session.PendingOptions
                        .Select(clsAiChatGuides.GetById)
                        .Where(g => g != null)
                        .ToList();

                    if (int.TryParse(normalized, out int idx) && idx >= 1 && idx <= guides.Count)
                    {
                        session.ClearPending();
                        return clsAiChatGuides.FormatGuide(guides[idx - 1], lang);
                    }

                    foreach (string id in session.PendingOptions)
                    {
                        var g = clsAiChatGuides.GetById(id);
                        if (g != null && normalized.Contains(g.Id.Replace('_', ' '), StringComparison.Ordinal))
                        {
                            session.ClearPending();
                            return clsAiChatGuides.FormatGuide(g, lang);
                        }
                    }

                    var matched = clsAiChatGuides.MatchGuide(normalized);
                    if (matched != null)
                    {
                        session.ClearPending();
                        return clsAiChatGuides.FormatGuide(matched, lang);
                    }

                    return BuildGuidePicker(
                        session.PendingOptions
                            .Select(clsAiChatGuides.GetById)
                            .Where(g => g != null)
                            .Take(3)
                            .ToList(),
                        lang);
                }
                case "confirm_partner":
                    return clsAiChatNameSearch.ResolvePendingConfirmation(session, normalized, companyId, lang, "partner");
                case "pick_partner":
                    return clsAiChatNameSearch.ResolvePendingPick(session, normalized, companyId, lang, "partner", rawMessage);
                case "confirm_account":
                    return clsAiChatNameSearch.ResolvePendingConfirmation(session, normalized, companyId, lang, "account");
                case "pick_account":
                    return clsAiChatNameSearch.ResolvePendingPick(session, normalized, companyId, lang, "account", rawMessage);
                case "confirm_entity":
                    return clsAiChatNameSearch.ResolvePendingEntityConfirmation(session, normalized, companyId, lang);
                case "pick_entity":
                    return clsAiChatNameSearch.ResolvePendingEntityPick(session, normalized, companyId, lang, rawMessage);
                default:
                    session.ClearPending();
                    return BuildClarifyingQuestion(normalized, lang);
            }
        }

        private string TryAnswerData(string message, int companyId, string lang, AiChatSession session)
        {
            if (_data.TryQueryData(message, companyId, lang, out string result, session))
            {
                session.LastDataTopic = DetectDataTopic(clsAiChat.NormalizePublic(message));
                return WrapDataResponse(result, lang);
            }

            return null;
        }

        private string RunDataQueryWithContext(string question, string _, int companyId, string lang)
        {
            if (_data.TryQueryData(question, companyId, lang, out string result))
                return result;

            return clsAiChatLanguage.IsArabic(lang) ? "لا توجد بيانات مطابقة." : "No matching data found.";
        }

        private static bool IsLlmHardFailure(string content)
        {
            if (string.IsNullOrWhiteSpace(content)) return true;
            string c = content.Trim();
            return c.StartsWith("AI error", StringComparison.OrdinalIgnoreCase) ||
                   c.StartsWith("خطأ في الذكاء", StringComparison.OrdinalIgnoreCase) ||
                   c.StartsWith("Could not get a response", StringComparison.OrdinalIgnoreCase) ||
                   c.StartsWith("تعذر الحصول على رد", StringComparison.OrdinalIgnoreCase) ||
                   c.StartsWith("LLM HTTP", StringComparison.OrdinalIgnoreCase);
        }

        public AiChatHistoryResult GetHistory(string sessionId)
        {
            AiChatSession session = AiChatSessionStore.GetOrCreate(sessionId);
            return new AiChatHistoryResult
            {
                SessionId = session.SessionId,
                Messages = session.GetHistoryForClient()
                    .Select(m => new AiChatHistoryItem { Role = m.Role, Content = m.Content, At = m.At })
                    .ToList()
            };
        }

        private string GetGuideById(string guideId, string lang)
        {
            var guide = clsAiChatGuides.GetById(guideId);
            return guide == null
                ? clsAiChatGuides.ListGuideTopics(lang)
                : clsAiChatGuides.FormatGuide(guide, lang);
        }

        private static string ProcessGuideForTopic(string topic, string normalized, string lang)
        {
            var guide = clsAiChatGuides.MatchGuide(normalized);
            if (guide != null)
                return clsAiChatGuides.FormatGuide(guide, lang);

            var suggestions = clsAiChatGuides.SearchGuides(clsAiChat.NormalizePublic(topic + " " + normalized), 1);
            if (suggestions.Count > 0)
                return clsAiChatGuides.FormatGuide(suggestions[0], lang);

            return clsAiChatGuides.ListGuideTopics(lang);
        }

        private static string WrapDataResponse(string data, string lang)
        {
            if (IsArabic(lang))
                return "إليك ما وجدته في بيانات شركتك:\n\n" + data;
            return "Here's what I found in your company data:\n\n" + data;
        }

        private static string ConversationalGreeting(string lang)
        {
            return IsArabic(lang)
                ? "مرحباً! 👋 أنا مساعد MT SOFTS.\n\nيمكنني:\n• جلب أرقام وتقارير من بياناتك (مبيعات، عملاء، فواتير...)\n• إرشادك خطوة بخطوة داخل النظام\n\nما الذي تحتاجه اليوم؟"
                : "Hello! 👋 I'm the MT SOFTS assistant.\n\nI can:\n• Fetch numbers and reports from your data (sales, customers, invoices...)\n• Guide you step-by-step inside the ERP\n\nWhat do you need today?";
        }

        private static string BuildCapabilitiesMessage(string lang)
        {
            if (IsArabic(lang))
            {
                return string.Join("\n", new[]
                {
                    clsAiChatKnowledge.GetSystemOverview(lang),
                    "",
                    "📊 **بيانات**: كم عدد العملاء؟ / مبيعات هذا الشهر / أفضل الأصناف / ابحث عن عميل [الاسم]",
                    "📋 **إرشاد**: كيف أنشئ فاتورة مبيعات؟ / كيف أعمل سند قيد؟",
                    "📍 **تنقل**: أين ميزان المراجعة؟ / أين الرواتب؟",
                    "",
                    clsAiChatGuides.ListGuideTopics(lang)
                });
            }

            return string.Join("\n", new[]
            {
                clsAiChatKnowledge.GetSystemOverview(lang),
                "",
                "📊 **Data**: how many customers? / this month sales / top items / find customer [name]",
                "📋 **Guidance**: how to create sales invoice? / journal voucher steps?",
                "📍 **Navigation**: where is trial balance? / where is payroll?",
                "",
                clsAiChatGuides.ListGuideTopics(lang)
            });
        }

        private static string AskGuideOrData(string topic, string lang)
        {
            if (IsArabic(lang))
                return $"بخصوص \"{topic}\":\n1️⃣ تريد **خطوات الإجراء** داخل النظام؟\n2️⃣ أم **أرقام/بيانات** من قاعدة البيانات؟\n\nاكتب: دليل / بيانات";
            return $"About \"{topic}\":\n1️⃣ Do you want **step-by-step guidance** in the ERP?\n2️⃣ Or **numbers/data** from the database?\n\nReply: guide / data";
        }

        private static string BuildGuidePicker(IReadOnlyList<clsAiChatGuides.ErpGuide> guides, string lang)
        {
            bool ar = IsArabic(lang);
            var lines = new List<string>
            {
                ar ? "وجدت أكثر من دليل قريب. أي واحد تقصد؟" : "I found a few related guides. Which one do you mean?"
            };

            for (int i = 0; i < guides.Count; i++)
                lines.Add($"{i + 1}. " + (ar ? guides[i].TitleAr : guides[i].TitleEn));

            lines.Add(ar ? "اكتب رقم الخيار أو اسم المهمة." : "Reply with the option number or task name.");
            return string.Join("\n", lines);
        }

        private static string BuildClarifyingQuestion(string normalized, string lang)
        {
            if (IsArabic(lang))
            {
                if (normalized.Contains("فات"))
                    return "هل تقصد:\n• بيانات الفواتير (عدد، مبيعات، آخر فواتير)\n• أو كيفية إنشاء فاتورة؟\n\nوضّح لي وسأساعدك مباشرة.";
                if (normalized.Contains("رات") || normalized.Contains("موظ"))
                    return "هل تريد:\n• بيانات الموظفين/الرواتب\n• أو خطوات تشغيل الرواتب في النظام؟";
                return "لم أفهم تماماً. هل تريد:\n1️⃣ **بيانات** من النظام (مبيعات، عملاء، فواتير...)\n2️⃣ **إرشاد** لكيفية تنفيذ مهمة\n\nاكتب سؤالك بشكل أوضح أو اختر 1 أو 2.";
            }

            if (normalized.Contains("invoice"))
                return "Do you mean:\n• Invoice **data** (count, sales, recent invoices)\n• Or **how to create** an invoice?\n\nTell me which and I'll help right away.";
            if (normalized.Contains("payroll") || normalized.Contains("employee"))
                return "Do you want:\n• Employee/payroll **data**\n• Or **steps** to run payroll in the ERP?";
            return "I'm not fully sure yet. Do you want:\n1️⃣ **Data** from the system (sales, customers, invoices...)\n2️⃣ **Guidance** on how to do something\n\nPlease clarify or reply 1 or 2.";
        }

        private static string ExpandFollowUp(string lastTopic, string normalized, string lang)
        {
            string suffix = normalized;
            if (IsArabic(lang))
            {
                return lastTopic switch
                {
                    "sales" when ContainsAny(normalized, "شهر", "month") => "مبيعات هذا الشهر",
                    "sales" when ContainsAny(normalized, "يوم", "today") => "مبيعات اليوم",
                    "customers" when ContainsAny(normalized, "أفضل", "top") => "أفضل العملاء",
                    _ => lastTopic + " " + suffix
                };
            }

            return lastTopic switch
            {
                "sales" when ContainsAny(normalized, "month") => "this month sales",
                "sales" when ContainsAny(normalized, "today") => "today sales",
                "customers" when ContainsAny(normalized, "top", "best") => "top customers",
                _ => lastTopic + " " + suffix
            };
        }

        private static string BuildDataQuestionFromTopic(string topic, string rawMessage, string lang)
        {
            if (!string.IsNullOrWhiteSpace(rawMessage) && rawMessage.Length > 8)
                return rawMessage;

            if (IsArabic(lang))
            {
                return topic switch
                {
                    "فاتورة" or "invoice" => "كم عدد الفواتير",
                    "مبيعات" or "sales" => "مبيعات هذا الشهر",
                    "عميل" or "customer" => "كم عدد العملاء",
                    _ => "help"
                };
            }

            return topic.ToLowerInvariant() switch
            {
                "invoice" => "how many invoices",
                "sales" => "this month sales",
                "customer" => "how many customers",
                _ => rawMessage
            };
        }

        private static string DetectTopicLabel(string normalized, string lang)
        {
            if (ContainsAny(normalized, "invoice", "فات"))
                return IsArabic(lang) ? "الفواتير" : "invoices";
            if (ContainsAny(normalized, "sales", "revenue", "مبيع", "ايراد"))
                return IsArabic(lang) ? "المبيعات" : "sales";
            if (ContainsAny(normalized, "customer", "client", "عم", "زب"))
                return IsArabic(lang) ? "العملاء" : "customers";
            if (ContainsAny(normalized, "payroll", "salary", "رات", "موظ"))
                return IsArabic(lang) ? "الرواتب" : "payroll";
            if (ContainsAny(normalized, "journal", "voucher", "قيد", "سند"))
                return IsArabic(lang) ? "السندات" : "vouchers";
            return IsArabic(lang) ? "هذا الموضوع" : "this topic";
        }

        private static string DetectDataTopic(string normalized)
        {
            if (ContainsAny(normalized, "sales", "revenue", "مبيع", "ايراد")) return "sales";
            if (ContainsAny(normalized, "customer", "client", "عم", "زب")) return "customers";
            if (ContainsAny(normalized, "invoice", "فات")) return "invoices";
            if (ContainsAny(normalized, "employee", "payroll", "موظ", "رات")) return "employees";
            return normalized.Split(' ').FirstOrDefault() ?? "";
        }

        private static bool LooksLikeGuideRequest(string normalized, int guideScore, int dataScore) =>
            guideScore >= 3 && guideScore > dataScore ||
            ContainsAny(normalized, "how to", "how do i", "steps", "guide", "where is", "create", "make", "open",
                "كيف", "خطوات", "انشاء", "إنشاء", "اين", "أين", "دليل");

        private static int ScoreGuideIntent(string normalized)
        {
            int score = 0;
            string[] words = { "how to", "how do", "steps", "guide", "create", "make", "open", "where",
                "كيف", "خطوات", "انشاء", "إنشاء", "دليل", "أين", "اين" };
            foreach (string w in words)
                if (normalized.Contains(w, StringComparison.Ordinal))
                    score += w.Contains(' ') ? 4 : 2;
            return score;
        }

        private static int ScoreDataIntent(string normalized)
        {
            int score = 0;
            string[] words = { "how many", "count", "total", "sum", "list", "show", "top", "recent", "pending",
                "كم", "عدد", "اجمالي", "إجمالي", "قائمة", "أفضل", "اخر", "آخر", "معلق" };
            foreach (string w in words)
                if (normalized.Contains(w, StringComparison.Ordinal))
                    score += w.Contains(' ') ? 4 : 2;
            return score;
        }

        private static bool IsFollowUp(string normalized)
        {
            string[] followUps = {
                "this month", "today", "yesterday", "top", "best", "recent", "more", "and", "what about",
                "الشهر", "اليوم", "أمس", "أفضل", "آخر", "المزيد", "وماذا", "طيب"
            };
            return normalized.Split(' ').Length <= 4 &&
                   followUps.Any(f => normalized.Contains(f, StringComparison.Ordinal));
        }

        private static bool IsGreeting(string normalized)
        {
            string[] greetings = { "hi", "hello", "hey", "good morning", "good evening",
                "مرحبا", "مرحباً", "السلام عليكم", "اهلا", "أهلا", "صباح الخير", "مساء الخير" };
            return greetings.Any(g => normalized == g || normalized.StartsWith(g + " "));
        }

        private static bool IsHelpRequest(string normalized)
        {
            string[] help = { "help", "what can you do", "commands", "مساعدة", "ماذا يمكنك", "ساعدني" };
            return help.Any(h => normalized.Contains(h, StringComparison.Ordinal));
        }

        private static bool ContainsAny(string text, params string[] parts) =>
            parts.Any(p => text.Contains(p, StringComparison.Ordinal));

        private static string EmptyPrompt(string lang) =>
            IsArabic(lang)
                ? "اكتب ما تحتاجه — بيانات أو إرشاد — وسأفهم وأساعدك."
                : "Tell me what you need — data or guidance — and I'll help.";

        private static bool IsArabic(string lang) => clsAiChatLanguage.IsArabic(lang);
    }

    public sealed class AiChatAgentResult
    {
        public string Output { get; init; } = "";
        public string Mode { get; init; } = "rules";
        public string Lang { get; init; } = "En";
        public List<string> Suggestions { get; init; } = new();
        public List<string> ToolsUsed { get; init; } = new();

        public static AiChatAgentResult Of(
            string output,
            string mode,
            string lang,
            IReadOnlyList<string> suggestions = null,
            IReadOnlyList<string> toolsUsed = null) =>
            new()
            {
                Output = output,
                Mode = mode,
                Lang = lang,
                Suggestions = suggestions?.ToList() ?? new List<string>(),
                ToolsUsed = toolsUsed?.ToList() ?? new List<string>()
            };
    }

    public sealed class AiChatWelcomeResult
    {
        public string Text { get; init; } = "";
        public string Mode { get; init; } = "rules";
        public string Lang { get; init; } = "En";
        public List<string> Suggestions { get; init; } = new();
    }

    public sealed class AiChatHistoryItem
    {
        public string Role { get; init; } = "";
        public string Content { get; init; } = "";
        public DateTime At { get; init; }
    }

    public sealed class AiChatHistoryResult
    {
        public string SessionId { get; init; } = "";
        public List<AiChatHistoryItem> Messages { get; init; } = new();
    }
}
