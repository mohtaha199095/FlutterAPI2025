using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text.RegularExpressions;
using Microsoft.Data.SqlClient;
using WebApplication2.MainClasses;

namespace WebApplication2.cls
{
    public sealed class NameSearchHit
    {
        public string Kind { get; init; } = "";
        public int Id { get; init; }
        public string EName { get; init; } = "";
        public string AName { get; init; } = "";
        public string Extra { get; init; } = "";
        public int SubType { get; init; }
        public double Score { get; set; }

        public string Token => $"{Kind}:{Id}";

        public string DisplayLabel() =>
            string.IsNullOrWhiteSpace(AName) || AName == EName ? EName : $"{EName} / {AName}";

        public string TypeLabel(string lang)
        {
            bool ar = clsAiChatLanguage.IsArabic(lang);
            return Kind switch
            {
                "account" => ar ? "حساب" : "Account",
                "bank" => ar ? "بنك" : "Bank",
                "cash" => ar ? "صندوق" : "Cash",
                "partner" => SubType == 2 ? (ar ? "مورد" : "Vendor") : (ar ? "عميل" : "Customer"),
                _ => Kind
            };
        }

        public string DisplayLabelWithType(string lang) => $"[{TypeLabel(lang)}] {DisplayLabel()}";
    }

    public static class clsAiChatNameSearch
    {
        private const int MaxPick = 3;
        private const int MaxShow = 3;
        private const int MaxCandidates = 350;

        public static string SearchPartners(string term, int companyId, string lang, AiChatSession session, bool forLlmTool = false) =>
            BuildResponse(term, LoadAndScorePartners(term, companyId), companyId, lang, session, "partner", forLlmTool);

        public static string SearchAccounts(string term, int companyId, string lang, AiChatSession session, bool forLlmTool = false) =>
            BuildResponse(term, LoadAndScoreAccounts(term, companyId), companyId, lang, session, "account", forLlmTool);

        /// <summary>Search chart of accounts, customers, vendors, banks, and cash drawers — pick best match.</summary>
        public static string SearchEverywhere(string term, int companyId, string lang, AiChatSession session, bool forLlmTool = false)
        {
            var hits = new List<NameSearchHit>();
            hits.AddRange(LoadAndScoreAccounts(term, companyId));
            hits.AddRange(LoadAndScorePartners(term, companyId));
            hits.AddRange(LoadAndScoreBanks(term, companyId));
            hits.AddRange(LoadAndScoreCashDrawers(term, companyId));
            return BuildUnifiedResponse(term, hits, companyId, lang, session, forLlmTool);
        }

        public static string GetEntityDetails(string kind, int id, int companyId, string lang) =>
            kind switch
            {
                "account" => GetAccountDetails(id, companyId, lang),
                "partner" => GetPartnerDetails(id, companyId, lang),
                "bank" => GetBankDetails(id, companyId, lang),
                "cash" => GetCashDetails(id, companyId, lang),
                _ => clsAiChatLanguage.IsArabic(lang) ? "لم أجد هذه النتيجة." : "Result not found."
            };

        /// <summary>Balance/name query with a searchable name extracted.</summary>
        public static bool TryExtractNamedBalanceQuery(string normalized, string rawMessage, out string name)
        {
            name = ExtractNameFromBalanceQuery(normalized, rawMessage);
            return !string.IsNullOrWhiteSpace(name) && name.Length >= 2 && !IsGenericBalanceTarget(name);
        }

        private static bool IsEntityPending(string action) =>
            action is "confirm_partner" or "pick_partner" or "confirm_account" or "pick_account"
                or "confirm_entity" or "pick_entity";

        /// <summary>Conversational message when waiting for user yes/no or pick.</summary>
        public static string FormatPendingUserMessage(AiChatSession session, int companyId, string lang)
        {
            if (string.IsNullOrWhiteSpace(session.PendingAction))
                return "";

            bool unified = session.PendingAction is "confirm_entity" or "pick_entity";
            var hits = LoadHitsFromTokens(session.PendingOptions, companyId, session.PendingTopic)
                .Take(MaxShow)
                .ToList();

            if (hits.Count == 0)
                return Arabic(lang)
                    ? "لم أتأكد بعد من الاسم المطلوب. اكتب الاسم مرة أخرى أو جزءاً أوضح منه."
                    : "I'm not sure which name you mean yet. Please type the name again or be more specific.";

            bool ar = Arabic(lang);
            string query = string.IsNullOrWhiteSpace(session.PendingTopic) ? "" : $" «{session.PendingTopic}»";

            if (session.PendingAction is "confirm_partner" or "confirm_account" or "confirm_entity")
            {
                var top = hits[0];
                int pct = (int)Math.Round(top.Score * 100);
                string label = unified || session.PendingAction == "confirm_entity"
                    ? top.DisplayLabelWithType(lang)
                    : top.DisplayLabel();
                return ar
                    ? $"بخصوص{query}، هل تقصد **{label}**؟ (تطابق تقريبي {pct}%)\n\nرد **نعم** للتأكيد، أو **لا** لعرض خيارات أخرى."
                    : $"For{query}, do you mean **{label}**? (~{pct}% match)\n\nReply **yes** to confirm, or **no** to see other options.";
            }

            var lines = new List<string>
            {
                ar
                    ? $"بحثت في **دليل الحسابات** و**العملاء/الموردين** و**البنوك** و**الصناديق**{query}. أي نتيجة تقصد؟"
                    : $"I searched **chart of accounts**, **customers/vendors**, **banks**, and **cash drawers**{query}. Which one do you mean?"
            };

            for (int i = 0; i < hits.Count; i++)
                lines.Add($"{i + 1}. **{hits[i].DisplayLabelWithType(lang)}**");

            lines.Add(ar
                ? "\nاكتب رقم الخيار، أو **نعم** إذا كان الأول صحيحاً."
                : "\nReply with the number, or **yes** if the first one is correct.");

            return string.Join("\n", lines);
        }

        public static string GetPartnerDetails(int id, int companyId, string lang)
        {
            DataRow row = LoadPartnerById(id, companyId);
            return row == null
                ? (Arabic(lang) ? "لم أجد هذا العميل/المورد." : "Customer/vendor not found.")
                : FormatPartnerRow(row, lang, companyId);
        }

        public static string GetAccountDetails(int id, int companyId, string lang)
        {
            DataRow row = LoadAccountById(id, companyId);
            return row == null
                ? (Arabic(lang) ? "لم أجد هذا الحساب." : "Account not found.")
                : FormatAccountRow(row, lang, companyId);
        }

        public static string GetBankDetails(int id, int companyId, string lang)
        {
            DataRow row = LoadBankById(id, companyId);
            return row == null
                ? (Arabic(lang) ? "لم أجد هذا البنك." : "Bank not found.")
                : FormatBankRow(row, lang, companyId);
        }

        public static string GetCashDetails(int id, int companyId, string lang)
        {
            DataRow row = LoadCashById(id, companyId);
            return row == null
                ? (Arabic(lang) ? "لم أجد هذا الصندوق." : "Cash drawer not found.")
                : FormatCashRow(row, lang, companyId);
        }

        /// <summary>User rejected a customer/vendor pick and wants GL / chart-of-accounts instead.</summary>
        public static bool TryAbandonPendingForNewRequest(AiChatSession session, string normalized, string rawMessage)
        {
            if (string.IsNullOrWhiteSpace(session.PendingAction))
                return false;

            if (!IsEntityPending(session.PendingAction))
                return false;

            if (!TryExtractNamedBalanceQuery(normalized, rawMessage, out _))
                return false;

            session.ClearPending();
            return true;
        }

        public static bool LooksLikeChartOfAccountsRequest(string normalized, string rawMessage)
        {
            string n = normalized ?? "";
            if (string.IsNullOrWhiteSpace(n))
                return false;

            bool rejectsPartner = ContainsAny(n,
                "not customer", "not client", "not vendor", "not partner", "not a customer",
                "not in customers", "not in customer", "not the customer", "not customers",
                "instead of customer", "instead of vendor", "wrong customer",
                "chart of account", "chart of accounts", "gl account", "ledger account", "general ledger",
                "coa", "دليل الحسابات", "دليل حسابات", "حساب محاسبي", "حسابات عامة",
                "مو عميل", "مو مورد", "ليس عميل", "ليس مورد", "مش عميل", "مش مورد", "ليس عميلا");

            bool wantsBalance = ContainsAny(n,
                "balance", "account balance", "رصيد", "رصيد حساب", "الرصيد", "كم رصيد");

            bool wantsGlAccount = ContainsAny(n,
                "chart of account", "chart of accounts", "gl account", "ledger", "coa",
                "دليل الحسابات", "دليل حسابات", "حساب في", "حساب من");

            bool mentionsAccount = ContainsAny(n, "account", "accounts", "حساب", "حسابات");

            if (rejectsPartner)
                return true;

            if (wantsBalance && (mentionsAccount || wantsGlAccount))
                return true;

            if (wantsGlAccount)
                return true;

            // Default: "رصيد [name]" without customer/vendor keywords → chart of accounts
            if (wantsBalance && !LooksLikePartnerBalanceRequest(n, rawMessage))
            {
                string name = ExtractNameFromBalanceQuery(n, rawMessage);
                if (name.Length >= 2 && !IsGenericBalanceTarget(name))
                    return true;
            }

            // Long correction that is not a yes/no/number pick — only switch to GL if explicitly about accounts
            if (n.Length > 18 && !IsAffirmative(n) && !IsPlainNegativeOnly(n) && !IsPickNumber(n))
            {
                if (wantsBalance && (mentionsAccount || wantsGlAccount))
                    return true;
                if (mentionsAccount && ContainsAny(n, "need", "want", "looking", "ابغى", "اريد", "أريد", "بدي", "محتاج"))
                    return true;
            }

            return false;
        }

        public static string ExtractGlAccountSearchTerm(string rawMessage)
        {
            string n = clsAiChat.NormalizePublic(rawMessage ?? "").ToLowerInvariant();
            string[] noise =
            {
                "no", "not", "wrong", "instead", "customer", "customers", "client", "clients", "vendor", "vendors",
                "partner", "i need", "i want", "account balance", "balance for", "balance of", "the balance",
                "chart of accounts", "chart of account", "in the chart of accounts", "from chart of accounts",
                "gl account", "ledger account", "general ledger", "for account", "for the account",
                "لا", "مو", "عميل", "عملاء", "مورد", "موردين", "رصيد", "رصيد حساب", "دليل الحسابات",
                "حساب في", "حساب من", "اريد", "أريد", "ابغى", "محتاج", "بدي"
            };

            foreach (string phrase in noise.OrderByDescending(p => p.Length))
                n = n.Replace(phrase, " ");

            return Regex.Replace(n, @"\s+", " ").Trim();
        }

        /// <summary>Extract name from "رصيد محمد طه" / "balance for X".</summary>
        public static string ExtractNameFromBalanceQuery(string normalized, string rawMessage)
        {
            string n = normalized ?? "";
            if (!ContainsAny(n, "رصيد", "balance", "كم رصيد", "what is the balance", "what is balance"))
                return "";

            string[] leadingPhrases =
            {
                "ممكن اعرف رصيد", "ممكن أعرف رصيد", "ممكن اعرف", "اعرف رصيد", "أعرف رصيد",
                "اريد رصيد", "أريد رصيد", "ابغى رصيد", "بدي رصيد", "محتاج رصيد", "اعطيني رصيد",
                "كم رصيد", "ما رصيد", "what is the balance of", "what is balance of",
                "what is the balance for", "balance of", "balance for", "show balance for"
            };

            foreach (string phrase in leadingPhrases.OrderByDescending(p => p.Length))
            {
                int idx = n.IndexOf(phrase, StringComparison.Ordinal);
                if (idx < 0) continue;
                string rest = TrimBalanceNameNoise(n[(idx + phrase.Length)..].Trim());
                if (rest.Length >= 2) return rest;
            }

            foreach (string marker in new[] { "رصيد", "balance" })
            {
                int idx = n.IndexOf(marker, StringComparison.Ordinal);
                if (idx < 0) continue;
                string rest = TrimBalanceNameNoise(n[(idx + marker.Length)..].Trim());
                if (rest.Length >= 2) return rest;
            }

            return "";
        }

        /// <summary>User explicitly asked for customer/vendor balance (tbl_BusinessPartner).</summary>
        public static bool LooksLikePartnerBalanceRequest(string normalized, string rawMessage)
        {
            string n = normalized ?? "";
            return ContainsAny(n,
                "customer", "customers", "client", "clients", "vendor", "vendors", "supplier", "suppliers",
                "business partner", "partner balance", "sub ledger", "sub-ledger",
                "عميل", "العميل", "عملاء", "العملاء", "مورد", "المورد", "موردين", "زبون", "الزبون", "زبائن",
                "ذمم مدينة", "ذمم دائنة", "مديونية العميل", "مديونية المورد");
        }

        /// <summary>Default: "رصيد [name]" → search everywhere.</summary>
        public static bool TryExtractAccountBalanceQuery(string normalized, string rawMessage, out string accountName) =>
            TryExtractNamedBalanceQuery(normalized, rawMessage, out accountName);

        public static bool TryExtractPartnerBalanceQuery(string normalized, string rawMessage, out string partnerName) =>
            TryExtractNamedBalanceQuery(normalized, rawMessage, out partnerName);

        [Obsolete("Use ExtractNameFromBalanceQuery")]
        public static string ExtractPartnerNameFromBalanceQuery(string normalized, string rawMessage) =>
            ExtractNameFromBalanceQuery(normalized, rawMessage);

        private static string TrimBalanceNameNoise(string text)
        {
            if (string.IsNullOrWhiteSpace(text)) return "";

            string[] suffix = { " لو سمحت", " من فضلك", " please", "؟", "?" };
            foreach (string s in suffix)
            {
                if (text.EndsWith(s, StringComparison.Ordinal))
                    text = text[..^s.Length].Trim();
            }

            string[] leading =
            {
                "العميل", "عميل", "المورد", "مورد", "الزبون", "زبون",
                "for customer", "for vendor", "for client", "customer", "vendor"
            };
            foreach (string l in leading.OrderByDescending(x => x.Length))
            {
                if (text.StartsWith(l + " ", StringComparison.Ordinal))
                    text = text[(l.Length + 1)..].Trim();
            }

            return text;
        }

        private static bool IsGenericBalanceTarget(string name)
        {
            string n = name.Trim();
            string[] generic =
            {
                "الصندوق", "صندوق", "البنك", "بنك", "bank", "cash", "النقد", "نقد",
                "الذمم", "ذمم مدينة", "ذمم", "open receivable", "receivables"
            };
            return generic.Any(g => n.Equals(g, StringComparison.OrdinalIgnoreCase));
        }

        public static NameSearchHit ParseToken(string token)
        {
            if (string.IsNullOrWhiteSpace(token)) return new NameSearchHit();
            string[] parts = token.Split(':', 2);
            if (parts.Length != 2 || !int.TryParse(parts[1], out int id))
                return new NameSearchHit();
            return new NameSearchHit { Kind = parts[0], Id = id };
        }

        public static string ResolvePendingConfirmation(
            AiChatSession session, string normalized, int companyId, string lang, string kind)
        {
            if (IsAffirmative(normalized) && session.PendingOptions.Count > 0)
            {
                var hit = ParseToken(session.PendingOptions[0]);
                session.ClearPending();
                return hit.Kind == "account"
                    ? GetAccountDetails(hit.Id, companyId, lang)
                    : GetPartnerDetails(hit.Id, companyId, lang);
            }

            if (IsNegative(normalized))
            {
                if (kind == "partner" && IsPlainNegativeOnly(normalized))
                {
                    session.ClearPending();
                    return Arabic(lang)
                        ? "حسناً. ما الذي تقصده؟\n\nمثلاً: **رصيد حساب** من **دليل الحسابات**، أو اسم **عميل/مورد** آخر."
                        : "OK. What did you mean instead?\n\nFor example: a **chart-of-accounts balance**, or a different **customer/vendor** name.";
                }

                session.PendingAction = kind == "account" ? "pick_account" : "pick_partner";
                return FormatPendingUserMessage(session, companyId, lang);
            }

            return FormatPendingUserMessage(session, companyId, lang);
        }

        public static string ResolvePendingPick(
            AiChatSession session, string normalized, int companyId, string lang, string kind, string rawMessage = "")
        {
            if (kind == "partner" && LooksLikeChartOfAccountsRequest(normalized, string.IsNullOrWhiteSpace(rawMessage) ? normalized : rawMessage))
            {
                session.ClearPending();
                string term = ExtractGlAccountSearchTerm(rawMessage);
                if (term.Length >= 2)
                    return SearchEverywhere(term, companyId, lang, session);

                return Arabic(lang)
                    ? "فهمت — تريد حساباً من دليل الحسابات وليس عميلاً/مورداً. اكتب اسم الحساب أو رقمه."
                    : "Got it — you want a **chart-of-accounts** entry, not a customer/vendor. Type the account name or number.";
            }

            var options = LoadHitsFromTokens(session.PendingOptions, companyId, session.PendingTopic);

            if (IsAffirmative(normalized) && options.Count > 0)
            {
                var pick = options[0];
                session.ClearPending();
                return pick.Kind == "account"
                    ? GetAccountDetails(pick.Id, companyId, lang)
                    : GetPartnerDetails(pick.Id, companyId, lang);
            }

            if (int.TryParse(normalized, out int idx) && idx >= 1 && idx <= options.Count)
            {
                var pick = options[idx - 1];
                session.ClearPending();
                return pick.Kind == "account"
                    ? GetAccountDetails(pick.Id, companyId, lang)
                    : GetPartnerDetails(pick.Id, companyId, lang);
            }

            foreach (var opt in options)
            {
                double score = clsAiChatFuzzyMatch.ScoreMatch(normalized, opt.EName, opt.AName, opt.Extra);
                if (score >= clsAiChatFuzzyMatch.ConfirmThreshold)
                {
                    session.ClearPending();
                    return opt.Kind == "account"
                        ? GetAccountDetails(opt.Id, companyId, lang)
                        : GetPartnerDetails(opt.Id, companyId, lang);
                }
            }

            return FormatPendingUserMessage(session, companyId, lang);
        }

        public static string ResolvePendingEntityConfirmation(
            AiChatSession session, string normalized, int companyId, string lang)
        {
            if (IsAffirmative(normalized) && session.PendingOptions.Count > 0)
            {
                var hit = ParseToken(session.PendingOptions[0]);
                session.ClearPending();
                return GetEntityDetails(hit.Kind, hit.Id, companyId, lang);
            }

            if (IsNegative(normalized))
            {
                if (IsPlainNegativeOnly(normalized))
                {
                    session.ClearPending();
                    return Arabic(lang)
                        ? "حسناً. اكتب الاسم أو جزءاً منه وسأبحث في الحسابات والعملاء والموردين والبنوك والصناديق."
                        : "OK. Type the name and I'll search accounts, customers, vendors, banks, and cash drawers.";
                }

                session.PendingAction = "pick_entity";
                return FormatPendingUserMessage(session, companyId, lang);
            }

            return FormatPendingUserMessage(session, companyId, lang);
        }

        public static string ResolvePendingEntityPick(
            AiChatSession session, string normalized, int companyId, string lang, string rawMessage = "")
        {
            if (TryExtractNamedBalanceQuery(normalized, rawMessage, out string newTerm) && newTerm != session.PendingTopic)
            {
                session.ClearPending();
                return SearchEverywhere(newTerm, companyId, lang, session);
            }

            var options = LoadHitsFromTokens(session.PendingOptions, companyId, session.PendingTopic);

            if (IsAffirmative(normalized) && options.Count > 0)
            {
                var pick = options[0];
                session.ClearPending();
                return GetEntityDetails(pick.Kind, pick.Id, companyId, lang);
            }

            if (int.TryParse(normalized, out int idx) && idx >= 1 && idx <= options.Count)
            {
                var pick = options[idx - 1];
                session.ClearPending();
                return GetEntityDetails(pick.Kind, pick.Id, companyId, lang);
            }

            foreach (var opt in options)
            {
                double score = clsAiChatFuzzyMatch.ScoreMatch(normalized, opt.EName, opt.AName, opt.Extra);
                if (score >= clsAiChatFuzzyMatch.ConfirmThreshold)
                {
                    session.ClearPending();
                    return GetEntityDetails(opt.Kind, opt.Id, companyId, lang);
                }
            }

            return FormatPendingUserMessage(session, companyId, lang);
        }

        private static string BuildUnifiedResponse(
            string term,
            List<NameSearchHit> hits,
            int companyId,
            string lang,
            AiChatSession session,
            bool forLlmTool)
        {
            bool ar = Arabic(lang);
            var ranked = hits
                .Where(h => h.Score >= clsAiChatFuzzyMatch.MinMatchThreshold)
                .OrderByDescending(h => h.Score)
                .Take(MaxPick)
                .ToList();

            if (ranked.Count == 0)
            {
                var weak = hits.OrderByDescending(h => h.Score).Take(MaxShow).ToList();
                if (weak.Count > 0 && weak[0].Score >= 0.30)
                {
                    SetUnifiedPickPending(session, term, weak);
                    return forLlmTool
                        ? ToolJson("pick", term, weak)
                        : FormatPendingUserMessage(session, companyId, lang);
                }

                return ar
                    ? $"لم أجد \"{term}\" في الحسابات أو العملاء أو الموردين أو البنوك أو الصناديق. جرّب اسماً أو رقماً أوضح."
                    : $"No match for \"{term}\" in accounts, customers, vendors, banks, or cash drawers. Try a clearer name or number.";
            }

            var top = ranked[0];

            if (ranked.Count == 1 && top.Score >= 0.98)
            {
                session.ClearPending();
                string details = GetEntityDetails(top.Kind, top.Id, companyId, lang);
                return forLlmTool ? ToolJson("found", term, new[] { top }, details) : details;
            }

            double scoreGap = ranked.Count > 1 ? top.Score - ranked[1].Score : 1.0;
            if (ranked.Count == 1 || (top.Score >= clsAiChatFuzzyMatch.ConfirmThreshold && scoreGap >= 0.15))
            {
                session.PendingAction = "confirm_entity";
                session.PendingTopic = term;
                session.PendingOptions = ranked.Take(MaxShow).Select(h => h.Token).ToList();
                return forLlmTool
                    ? ToolJson("confirm", term, LoadHitsFromTokens(session.PendingOptions, companyId, term))
                    : FormatPendingUserMessage(session, companyId, lang);
            }

            SetUnifiedPickPending(session, term, ranked.Take(MaxShow).ToList());
            return forLlmTool
                ? ToolJson("pick", term, ranked)
                : FormatPendingUserMessage(session, companyId, lang);
        }

        private static string BuildResponse(
            string term,
            List<NameSearchHit> hits,
            int companyId,
            string lang,
            AiChatSession session,
            string kind,
            bool forLlmTool)
        {
            bool ar = Arabic(lang);
            var ranked = hits
                .Where(h => h.Score >= clsAiChatFuzzyMatch.MinMatchThreshold)
                .OrderByDescending(h => h.Score)
                .Take(MaxPick)
                .ToList();

            if (ranked.Count == 0)
            {
                var weak = hits.OrderByDescending(h => h.Score).Take(MaxShow).ToList();
                if (weak.Count > 0 && weak[0].Score >= 0.30)
                {
                    SetPickPending(session, term, weak);
                    return forLlmTool
                        ? ToolJson("pick", term, weak)
                        : FormatPendingUserMessage(session, companyId, lang);
                }

                return ar
                    ? $"لم أجد نتيجة قريبة من \"{term}\". ما الاسم أو الرقم الذي تبحث عنه بالضبط؟"
                    : $"I couldn't find a close match for \"{term}\". What exact name or number are you looking for?";
            }

            var top = ranked[0];

            if (ranked.Count == 1 && top.Score >= 0.98)
            {
                session.ClearPending();
                string details = kind == "account"
                    ? GetAccountDetails(top.Id, companyId, lang)
                    : GetPartnerDetails(top.Id, companyId, lang);
                return forLlmTool ? ToolJson("found", term, new[] { top }, details) : details;
            }

            double scoreGap = ranked.Count > 1 ? top.Score - ranked[1].Score : 1.0;
            if (ranked.Count == 1 || (top.Score >= clsAiChatFuzzyMatch.ConfirmThreshold && scoreGap >= 0.15))
            {
                session.PendingAction = kind == "account" ? "confirm_account" : "confirm_partner";
                session.PendingTopic = term;
                session.PendingOptions = ranked.Take(MaxShow).Select(h => h.Token).ToList();
                return forLlmTool
                    ? ToolJson("confirm", term, LoadHitsFromTokens(session.PendingOptions, companyId, term))
                    : FormatPendingUserMessage(session, companyId, lang);
            }

            SetPickPending(session, term, ranked.Take(MaxShow).ToList());
            return forLlmTool
                ? ToolJson("pick", term, ranked)
                : FormatPendingUserMessage(session, companyId, lang);
        }

        private static void SetUnifiedPickPending(AiChatSession session, string term, List<NameSearchHit> hits)
        {
            session.PendingAction = "pick_entity";
            session.PendingTopic = term;
            session.PendingOptions = hits.Select(h => h.Token).ToList();
        }

        private static string ToolJson(string status, string query, IReadOnlyList<NameSearchHit> hits, string details = null)
        {
            return Newtonsoft.Json.JsonConvert.SerializeObject(new
            {
                status,
                query,
                awaiting_user = true,
                candidates = hits.Select((h, i) => new
                {
                    index = i + 1,
                    label = h.DisplayLabel(),
                    score = (int)Math.Round(h.Score * 100)
                }),
                details
            });
        }

        private static void SetPickPending(AiChatSession session, string term, List<NameSearchHit> hits)
        {
            string kind = hits.FirstOrDefault()?.Kind ?? "partner";
            session.PendingAction = kind == "account" ? "pick_account" : "pick_partner";
            session.PendingTopic = term;
            session.PendingOptions = hits.Select(h => h.Token).ToList();
        }

        private static string BuildConfirmPrompt(NameSearchHit hit, string term, string lang, string kind)
        {
            bool ar = Arabic(lang);
            int pct = (int)Math.Round(hit.Score * 100);
            string entity = kind == "account"
                ? (ar ? "الحساب" : "account")
                : (ar ? "العميل/المورد" : "customer/vendor");

            if (ar)
            {
                return $"هل تقصد **{hit.DisplayLabel()}**؟ (تطابق تقريبي {pct}% لـ \"{term}\")\n\n" +
                       $"اكتب **نعم** للتأكيد، **لا** لعرض قائمة أخرى، أو اكتب رقم من القائمة إذا ظهرت.";
            }

            return $"Did you mean **{hit.DisplayLabel()}**? (~{pct}% match for \"{term}\")\n\n" +
                   $"Reply **yes** to confirm, **no** to see other matches, or type a list number.";
        }

        private static string BuildPickList(List<string> tokens, string term, int companyId, string lang, string kind)
        {
            bool ar = Arabic(lang);
            var hits = LoadHitsFromTokens(tokens, companyId, term);
            if (hits.Count == 0)
                return ar ? "لا توجد خيارات. أعد البحث باسم أوضح." : "No options left. Try searching again with a clearer name.";

            return (ar ? "اختر رقم النتيجة:\n" : "Pick a result number:\n")
                   + FormatNumberedList(hits, lang, kind);
        }

        private static List<NameSearchHit> LoadHitsFromTokens(List<string> tokens, int companyId, string term)
        {
            var hits = new List<NameSearchHit>();
            foreach (string token in tokens)
            {
                var parsed = ParseToken(token);
                if (parsed.Id <= 0) continue;

                if (parsed.Kind == "account")
                {
                    DataRow row = LoadAccountById(parsed.Id, companyId);
                    if (row == null) continue;
                    string en = Simulate.String(row["EName"]);
                    string arName = Simulate.String(row["AName"]);
                    string num = Simulate.String(row["AccountNumber"]);
                    hits.Add(new NameSearchHit
                    {
                        Kind = "account",
                        Id = parsed.Id,
                        EName = en,
                        AName = arName,
                        Extra = num,
                        Score = clsAiChatFuzzyMatch.ScoreMatch(term, en, arName, num)
                    });
                }
                else if (parsed.Kind == "bank")
                {
                    DataRow row = LoadBankById(parsed.Id, companyId);
                    if (row == null) continue;
                    string en = Simulate.String(row["EName"]);
                    string arName = Simulate.String(row["AName"]);
                    string num = Simulate.String(row["AccountNumber"]);
                    hits.Add(new NameSearchHit
                    {
                        Kind = "bank",
                        Id = parsed.Id,
                        EName = en,
                        AName = arName,
                        Extra = num,
                        Score = clsAiChatFuzzyMatch.ScoreMatch(term, en, arName, num)
                    });
                }
                else if (parsed.Kind == "cash")
                {
                    DataRow row = LoadCashById(parsed.Id, companyId);
                    if (row == null) continue;
                    string en = Simulate.String(row["EName"]);
                    string arName = Simulate.String(row["AName"]);
                    hits.Add(new NameSearchHit
                    {
                        Kind = "cash",
                        Id = parsed.Id,
                        EName = en,
                        AName = arName,
                        Score = clsAiChatFuzzyMatch.ScoreMatch(term, en, arName, "")
                    });
                }
                else
                {
                    DataRow row = LoadPartnerById(parsed.Id, companyId);
                    if (row == null) continue;
                    string en = Simulate.String(row["EName"]);
                    string ar = Simulate.String(row["AName"]);
                    string tel = Simulate.String(row["Tel"]);
                    hits.Add(new NameSearchHit
                    {
                        Kind = "partner",
                        Id = parsed.Id,
                        EName = en,
                        AName = ar,
                        Extra = tel,
                        SubType = Simulate.Integer32(row["Type"]),
                        Score = clsAiChatFuzzyMatch.ScoreMatch(term, en, ar, tel)
                    });
                }
            }
            return hits;
        }

        private static string FormatNumberedList(List<NameSearchHit> hits, string lang, string kind)
        {
            bool ar = Arabic(lang);
            var lines = new List<string>();
            for (int i = 0; i < hits.Count; i++)
            {
                var h = hits[i];
                int pct = (int)Math.Round(h.Score * 100);
                string typeLabel = "";
                if (kind == "partner")
                    typeLabel = h.SubType == 2 ? (ar ? "مورد" : "Vendor") : (ar ? "عميل" : "Customer");
                else if (!string.IsNullOrWhiteSpace(h.Extra))
                    typeLabel = h.Extra;

                lines.Add($"{i + 1}. {h.DisplayLabel()}" +
                          (string.IsNullOrWhiteSpace(typeLabel) ? "" : $" ({typeLabel})") +
                          $" — {pct}%");
            }
            return string.Join("\n", lines);
        }

        private static List<NameSearchHit> LoadAndScorePartners(string term, int companyId)
        {
            DataTable dt = QueryPartners(term, companyId);
            var hits = new List<NameSearchHit>();

            foreach (DataRow row in dt.Rows)
            {
                string en = Simulate.String(row["EName"]);
                string ar = Simulate.String(row["AName"]);
                string commercial = Simulate.String(row["CommercialName"]);
                string tel = Simulate.String(row["Tel"]);

                var hit = new NameSearchHit
                {
                    Kind = "partner",
                    Id = Simulate.Integer32(row["ID"]),
                    EName = en,
                    AName = ar,
                    Extra = tel,
                    SubType = Simulate.Integer32(row["Type"]),
                    Score = clsAiChatFuzzyMatch.ScoreMatch(term, en, ar, commercial, tel)
                };
                if (hit.Score > 0) hits.Add(hit);
            }

            if (hits.Count >= 5) return hits;

            // Broader fallback — score more records in memory
            DataTable broad = QueryPartnersBroad(companyId);
            var seen = hits.Select(h => h.Id).ToHashSet();
            foreach (DataRow row in broad.Rows)
            {
                int id = Simulate.Integer32(row["ID"]);
                if (seen.Contains(id)) continue;

                string en = Simulate.String(row["EName"]);
                string ar = Simulate.String(row["AName"]);
                string commercial = Simulate.String(row["CommercialName"]);
                string tel = Simulate.String(row["Tel"]);
                double score = clsAiChatFuzzyMatch.ScoreMatch(term, en, ar, commercial, tel);
                if (score < 0.28) continue;

                hits.Add(new NameSearchHit
                {
                    Kind = "partner",
                    Id = id,
                    EName = en,
                    AName = ar,
                    Extra = tel,
                    SubType = Simulate.Integer32(row["Type"]),
                    Score = score
                });
            }

            return hits;
        }

        private static List<NameSearchHit> LoadAndScoreAccounts(string term, int companyId)
        {
            DataTable dt = QueryAccounts(term, companyId);
            var hits = new List<NameSearchHit>();

            foreach (DataRow row in dt.Rows)
            {
                string en = Simulate.String(row["EName"]);
                string ar = Simulate.String(row["AName"]);
                string num = Simulate.String(row["AccountNumber"]);

                var hit = new NameSearchHit
                {
                    Kind = "account",
                    Id = Simulate.Integer32(row["ID"]),
                    EName = en,
                    AName = ar,
                    Extra = num,
                    Score = clsAiChatFuzzyMatch.ScoreMatch(term, en, ar, num)
                };
                if (hit.Score > 0) hits.Add(hit);
            }

            if (hits.Count >= 5) return hits;

            DataTable broad = QueryAccountsBroad(companyId);
            var seen = hits.Select(h => h.Id).ToHashSet();
            foreach (DataRow row in broad.Rows)
            {
                int id = Simulate.Integer32(row["ID"]);
                if (seen.Contains(id)) continue;

                string en = Simulate.String(row["EName"]);
                string ar = Simulate.String(row["AName"]);
                string num = Simulate.String(row["AccountNumber"]);
                double score = clsAiChatFuzzyMatch.ScoreMatch(term, en, ar, num);
                if (score < 0.28) continue;

                hits.Add(new NameSearchHit
                {
                    Kind = "account",
                    Id = id,
                    EName = en,
                    AName = ar,
                    Extra = num,
                    Score = score
                });
            }

            return hits;
        }

        private static List<NameSearchHit> LoadAndScoreBanks(string term, int companyId)
        {
            DataTable dt = QueryBanks(term, companyId);
            var hits = new List<NameSearchHit>();

            foreach (DataRow row in dt.Rows)
            {
                string en = Simulate.String(row["EName"]);
                string ar = Simulate.String(row["AName"]);
                string num = Simulate.String(row["AccountNumber"]);

                var hit = new NameSearchHit
                {
                    Kind = "bank",
                    Id = Simulate.Integer32(row["ID"]),
                    EName = en,
                    AName = ar,
                    Extra = num,
                    Score = clsAiChatFuzzyMatch.ScoreMatch(term, en, ar, num)
                };
                if (hit.Score > 0) hits.Add(hit);
            }

            if (hits.Count >= 5) return hits;

            DataTable broad = QueryBanksBroad(companyId);
            var seen = hits.Select(h => h.Id).ToHashSet();
            foreach (DataRow row in broad.Rows)
            {
                int id = Simulate.Integer32(row["ID"]);
                if (seen.Contains(id)) continue;

                string en = Simulate.String(row["EName"]);
                string ar = Simulate.String(row["AName"]);
                string num = Simulate.String(row["AccountNumber"]);
                double score = clsAiChatFuzzyMatch.ScoreMatch(term, en, ar, num);
                if (score < 0.28) continue;

                hits.Add(new NameSearchHit
                {
                    Kind = "bank",
                    Id = id,
                    EName = en,
                    AName = ar,
                    Extra = num,
                    Score = score
                });
            }

            return hits;
        }

        private static List<NameSearchHit> LoadAndScoreCashDrawers(string term, int companyId)
        {
            DataTable dt = QueryCashDrawers(term, companyId);
            var hits = new List<NameSearchHit>();

            foreach (DataRow row in dt.Rows)
            {
                string en = Simulate.String(row["EName"]);
                string ar = Simulate.String(row["AName"]);

                var hit = new NameSearchHit
                {
                    Kind = "cash",
                    Id = Simulate.Integer32(row["ID"]),
                    EName = en,
                    AName = ar,
                    Score = clsAiChatFuzzyMatch.ScoreMatch(term, en, ar, "")
                };
                if (hit.Score > 0) hits.Add(hit);
            }

            if (hits.Count >= 5) return hits;

            DataTable broad = QueryCashDrawersBroad(companyId);
            var seen = hits.Select(h => h.Id).ToHashSet();
            foreach (DataRow row in broad.Rows)
            {
                int id = Simulate.Integer32(row["ID"]);
                if (seen.Contains(id)) continue;

                string en = Simulate.String(row["EName"]);
                string ar = Simulate.String(row["AName"]);
                double score = clsAiChatFuzzyMatch.ScoreMatch(term, en, ar, "");
                if (score < 0.28) continue;

                hits.Add(new NameSearchHit
                {
                    Kind = "cash",
                    Id = id,
                    EName = en,
                    AName = ar,
                    Score = score
                });
            }

            return hits;
        }

        private static DataTable QueryPartners(string term, int companyId)
        {
            var patterns = clsAiChatFuzzyMatch.BuildLikePatterns(term).Take(6).ToList();
            if (patterns.Count == 0) return new DataTable();

            var wheres = new List<string>();
            var parms = new List<SqlParameter>();
            for (int i = 0; i < patterns.Count; i++)
            {
                wheres.Add($"(EName LIKE @P{i} OR AName LIKE @P{i} OR CommercialName LIKE @P{i} OR Tel LIKE @P{i})");
                parms.Add(new SqlParameter($"@P{i}", SqlDbType.NVarChar, 200) { Value = patterns[i] });
            }

            string sql = $@"SELECT TOP {MaxCandidates} ID, EName, AName, Tel, Email, Type, CommercialName
                            FROM tbl_BusinessPartner
                            WHERE Active = 1 AND ({string.Join(" OR ", wheres)})
                            ORDER BY EName";

            return Execute(companyId, sql, parms.ToArray());
        }

        private static DataTable QueryPartnersBroad(int companyId) =>
            Execute(companyId,
                $"SELECT TOP {MaxCandidates} ID, EName, AName, Tel, Email, Type, CommercialName FROM tbl_BusinessPartner WHERE Active = 1 ORDER BY EName",
                null);

        private static DataTable QueryAccounts(string term, int companyId)
        {
            var patterns = clsAiChatFuzzyMatch.BuildLikePatterns(term).Take(6).ToList();
            if (patterns.Count == 0) return new DataTable();

            var wheres = new List<string>();
            var parms = new List<SqlParameter>();
            for (int i = 0; i < patterns.Count; i++)
            {
                wheres.Add($"(EName LIKE @P{i} OR AName LIKE @P{i} OR AccountNumber LIKE @P{i})");
                parms.Add(new SqlParameter($"@P{i}", SqlDbType.NVarChar, 200) { Value = patterns[i] });
            }

            string sql = $@"SELECT TOP {MaxCandidates} ID, AccountNumber, AName, EName
                            FROM tbl_Accounts
                            WHERE ({string.Join(" OR ", wheres)})
                            ORDER BY AccountNumber";

            return Execute(companyId, sql, parms.ToArray());
        }

        private static DataTable QueryAccountsBroad(int companyId) =>
            Execute(companyId,
                $"SELECT TOP {MaxCandidates} ID, AccountNumber, AName, EName FROM tbl_Accounts ORDER BY AccountNumber",
                null);

        private static DataTable QueryBanks(string term, int companyId)
        {
            var patterns = clsAiChatFuzzyMatch.BuildLikePatterns(term).Take(6).ToList();
            if (patterns.Count == 0) return new DataTable();

            var wheres = new List<string>();
            var parms = new List<SqlParameter>();
            for (int i = 0; i < patterns.Count; i++)
            {
                wheres.Add($"(EName LIKE @P{i} OR AName LIKE @P{i} OR AccountNumber LIKE @P{i})");
                parms.Add(new SqlParameter($"@P{i}", SqlDbType.NVarChar, 200) { Value = patterns[i] });
            }

            string sql = $@"SELECT TOP {MaxCandidates} ID, AccountNumber, AName, EName
                            FROM tbl_Banks
                            WHERE CompanyID = @CompanyId AND ({string.Join(" OR ", wheres)})
                            ORDER BY EName";

            parms.Add(new SqlParameter("@CompanyId", SqlDbType.Int) { Value = companyId });
            return Execute(companyId, sql, parms.ToArray());
        }

        private static DataTable QueryBanksBroad(int companyId) =>
            Execute(companyId,
                $"SELECT TOP {MaxCandidates} ID, AccountNumber, AName, EName FROM tbl_Banks WHERE CompanyID = @CompanyId ORDER BY EName",
                new[] { new SqlParameter("@CompanyId", SqlDbType.Int) { Value = companyId } });

        private static DataTable QueryCashDrawers(string term, int companyId)
        {
            var patterns = clsAiChatFuzzyMatch.BuildLikePatterns(term).Take(6).ToList();
            if (patterns.Count == 0) return new DataTable();

            var wheres = new List<string>();
            var parms = new List<SqlParameter>();
            for (int i = 0; i < patterns.Count; i++)
            {
                wheres.Add($"(EName LIKE @P{i} OR AName LIKE @P{i})");
                parms.Add(new SqlParameter($"@P{i}", SqlDbType.NVarChar, 200) { Value = patterns[i] });
            }

            string sql = $@"SELECT TOP {MaxCandidates} ID, AName, EName
                            FROM tbl_CashDrawer
                            WHERE CompanyID = @CompanyId AND ({string.Join(" OR ", wheres)})
                            ORDER BY EName";

            parms.Add(new SqlParameter("@CompanyId", SqlDbType.Int) { Value = companyId });
            return Execute(companyId, sql, parms.ToArray());
        }

        private static DataTable QueryCashDrawersBroad(int companyId) =>
            Execute(companyId,
                $"SELECT TOP {MaxCandidates} ID, AName, EName FROM tbl_CashDrawer WHERE CompanyID = @CompanyId ORDER BY EName",
                new[] { new SqlParameter("@CompanyId", SqlDbType.Int) { Value = companyId } });

        private static DataRow LoadPartnerById(int id, int companyId)
        {
            DataTable dt = Execute(companyId, clsAiChatSql.LoadPartnerById(),
                new[] { new SqlParameter("@Id", SqlDbType.Int) { Value = id } });
            return dt.Rows.Count > 0 ? dt.Rows[0] : null;
        }

        private static DataRow LoadAccountById(int id, int companyId)
        {
            DataTable dt = Execute(companyId, clsAiChatSql.LoadAccountById(),
                new[] { new SqlParameter("@Id", SqlDbType.Int) { Value = id } });
            return dt.Rows.Count > 0 ? dt.Rows[0] : null;
        }

        private static DataRow LoadBankById(int id, int companyId)
        {
            DataTable dt = Execute(companyId,
                "SELECT ID, EName, AName, AccountNumber FROM tbl_Banks WHERE ID = @Id",
                new[] { new SqlParameter("@Id", SqlDbType.Int) { Value = id } });
            return dt.Rows.Count > 0 ? dt.Rows[0] : null;
        }

        private static DataRow LoadCashById(int id, int companyId)
        {
            DataTable dt = Execute(companyId,
                "SELECT ID, EName, AName FROM tbl_CashDrawer WHERE ID = @Id",
                new[] { new SqlParameter("@Id", SqlDbType.Int) { Value = id } });
            return dt.Rows.Count > 0 ? dt.Rows[0] : null;
        }

        private static DataTable Execute(int companyId, string sql, SqlParameter[] parms)
        {
            clsSQL db = new clsSQL();
            string conn = db.CreateDataBaseConnectionString(companyId);
            return db.ExecuteQueryStatement(sql, conn, parms) ?? new DataTable();
        }

        private static string FormatPartnerRow(DataRow row, string lang, int companyId = 0)
        {
            bool ar = Arabic(lang);
            string en = Simulate.String(row["EName"]);
            string an = Simulate.String(row["AName"]);
            string tel = Simulate.String(row["Tel"]);
            string email = Simulate.String(row["Email"]);
            string commercial = Simulate.String(row["CommercialName"]);
            int type = Simulate.Integer32(row["Type"]);
            int id = Simulate.Integer32(row["ID"]);
            string typeLabel = type == 2 ? (ar ? "مورد" : "Vendor") : (ar ? "عميل" : "Customer");

            var lines = new List<string> { $"**{typeLabel}** — {en} / {an}" };
            if (!string.IsNullOrWhiteSpace(commercial))
                lines.Add(ar ? $"الاسم التجاري: {commercial}" : $"Commercial: {commercial}");
            if (!string.IsNullOrWhiteSpace(tel))
                lines.Add(ar ? $"الهاتف: {tel}" : $"Phone: {tel}");
            if (!string.IsNullOrWhiteSpace(email))
                lines.Add(ar ? $"البريد: {email}" : $"Email: {email}");
            if (companyId > 0)
            {
                decimal bal = QueryPartnerBalance(id, type, companyId);
                string balanceLabel = type == 2
                    ? (ar ? "**الرصيد (ذمم دائنة — من القيود):**" : "**Balance (AP sub-ledger — from journal):**")
                    : (ar ? "**الرصيد (ذمم مدينة — من القيود):**" : "**Balance (AR sub-ledger — from journal):**");
                lines.Add($"{balanceLabel} {bal:N2}");
            }
            lines.Add(ar ? $"رقم السجل: {id}" : $"Record ID: {id}");
            return string.Join("\n", lines);
        }

        private static string FormatAccountRow(DataRow row, string lang, int companyId = 0)
        {
            bool ar = Arabic(lang);
            string en = Simulate.String(row["EName"]);
            string an = Simulate.String(row["AName"]);
            string num = Simulate.String(row["AccountNumber"]);
            int id = Simulate.Integer32(row["ID"]);
            var lines = new List<string>();

            if (ar)
            {
                lines.Add($"**حساب** — {num}");
                lines.Add($"{en} / {an}");
                if (companyId > 0)
                    lines.Add(ar ? $"**الرصيد (من القيود):** {QueryAccountBalance(id, companyId):N2}" : $"**Balance (from journal):** {QueryAccountBalance(id, companyId):N2}");
                lines.Add($"رقم السجل: {id}");
            }
            else
            {
                lines.Add($"**Account** — {num}");
                lines.Add($"{en} / {an}");
                if (companyId > 0)
                    lines.Add($"**Balance (from journal):** {QueryAccountBalance(id, companyId):N2}");
                lines.Add($"Record ID: {id}");
            }

            return string.Join("\n", lines);
        }

        private static string FormatBankRow(DataRow row, string lang, int companyId = 0)
        {
            bool ar = Arabic(lang);
            string en = Simulate.String(row["EName"]);
            string an = Simulate.String(row["AName"]);
            string num = Simulate.String(row["AccountNumber"]);
            int id = Simulate.Integer32(row["ID"]);
            var lines = new List<string>
            {
                ar ? $"**بنك** — {en} / {an}" : $"**Bank** — {en} / {an}"
            };
            if (!string.IsNullOrWhiteSpace(num))
                lines.Add(ar ? $"رقم الحساب: {num}" : $"Account number: {num}");
            if (companyId > 0)
            {
                decimal bal = QuerySubLedgerBalance(id, clsAiChatSchema.AccountRefs.Banks, companyId);
                lines.Add(ar ? $"**الرصيد (من القيود):** {bal:N2}" : $"**Balance (from journal):** {bal:N2}");
            }
            lines.Add(ar ? $"رقم السجل: {id}" : $"Record ID: {id}");
            return string.Join("\n", lines);
        }

        private static string FormatCashRow(DataRow row, string lang, int companyId = 0)
        {
            bool ar = Arabic(lang);
            string en = Simulate.String(row["EName"]);
            string an = Simulate.String(row["AName"]);
            int id = Simulate.Integer32(row["ID"]);
            var lines = new List<string>
            {
                ar ? $"**صندوق** — {en} / {an}" : $"**Cash drawer** — {en} / {an}"
            };
            if (companyId > 0)
            {
                decimal bal = QuerySubLedgerBalance(id, clsAiChatSchema.AccountRefs.Cash, companyId);
                lines.Add(ar ? $"**الرصيد (من القيود):** {bal:N2}" : $"**Balance (from journal):** {bal:N2}");
            }
            lines.Add(ar ? $"رقم السجل: {id}" : $"Record ID: {id}");
            return string.Join("\n", lines);
        }

        private static decimal QueryAccountBalance(int accountId, int companyId)
        {
            DataTable dt = Execute(companyId, clsAiChatSql.GlAccountBalanceSimple(), new[]
            {
                new SqlParameter("@AccountId", accountId),
                new SqlParameter("@CompanyId", companyId)
            });
            return dt.Rows.Count > 0 ? Simulate.Decimal(dt.Rows[0]["Balance"]) : 0m;
        }

        public static decimal QueryPartnerBalance(int partnerId, int partnerType, int companyId)
        {
            int accountRef = partnerType == 2
                ? clsAiChatSchema.AccountRefs.Vendor
                : clsAiChatSchema.AccountRefs.Customer;
            return QuerySubLedgerBalance(partnerId, accountRef, companyId);
        }

        private static decimal QuerySubLedgerBalance(int subAccountId, int accountRefId, int companyId)
        {
            DataTable dt = Execute(companyId, clsAiChatSql.PartnerSubLedgerBalance(accountRefId), new[]
            {
                new SqlParameter("@PartnerId", subAccountId),
                new SqlParameter("@CompanyId", companyId)
            });
            return dt.Rows.Count > 0 ? Simulate.Decimal(dt.Rows[0]["Balance"]) : 0m;
        }

        private static bool IsValidTerm(string term) =>
            !string.IsNullOrWhiteSpace(term) &&
            Regex.IsMatch(term.Trim(), @"^[\p{L}\p{N}\s\-_.]+$") &&
            term.Trim().Length >= 2;

        private static bool Arabic(string lang) => clsAiChatLanguage.IsArabic(lang);

        private static bool IsAffirmative(string normalized) =>
            new[] { "yes", "y", "yep", "yeah", "ok", "okay", "confirm", "correct", "right", "1", "first",
                "نعم", "اه", "أجل", "تم", "صح", "موافق", "ايوه", "ايه", "أكيد", "الاول", "الأول" }
                .Any(w => normalized == w || normalized.StartsWith(w + " "));

        private static bool IsNegative(string normalized) =>
            new[] { "no", "n", "wrong", "not", "2", "لا", "كلا", "مو", "غلط" }
                .Any(w => normalized == w || normalized.StartsWith(w + " "));

        private static bool IsPlainNegativeOnly(string normalized) =>
            new[] { "no", "n", "wrong", "لا", "كلا", "غلط" }.Contains(normalized);

        private static bool IsPickNumber(string normalized) =>
            int.TryParse(normalized, out int idx) && idx >= 1 && idx <= 9;

        private static bool ContainsAny(string text, params string[] parts) =>
            !string.IsNullOrWhiteSpace(text) &&
            parts.Any(p => text.Contains(p, StringComparison.OrdinalIgnoreCase));
    }
}
