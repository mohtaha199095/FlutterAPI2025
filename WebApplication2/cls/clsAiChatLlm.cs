using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace WebApplication2.cls
{
    public sealed class clsAiChatLlm
    {
        private readonly AiChatSettings _settings;
        private static readonly HttpClient Http = new() { Timeout = TimeSpan.FromSeconds(90) };

        public clsAiChatLlm(IConfiguration configuration)
        {
            _settings = configuration.GetSection("AiChat").Get<AiChatSettings>() ?? new AiChatSettings();

            // Allow API key from environment: AiChat__ApiKey
            string envKey = configuration["AiChat:ApiKey"] ?? configuration["AI_CHAT_API_KEY"];
            if (!string.IsNullOrWhiteSpace(envKey))
                _settings.ApiKey = envKey;
        }

        public AiChatSettings Settings => _settings;

        public bool IsConfigured
        {
            get
            {
                if (!_settings.Enabled || string.IsNullOrWhiteSpace(_settings.BaseUrl))
                    return false;

                // Ollama and local models don't need a real API key
                if (_settings.Provider.Equals("Ollama", StringComparison.OrdinalIgnoreCase))
                    return true;

                return !string.IsNullOrWhiteSpace(_settings.ApiKey);
            }
        }

        public async Task<AiChatLlmResult> ChatAsync(
            AiChatSession session,
            string userMessage,
            int companyId,
            string lang,
            Func<string, string, int, string, string> runDataQuery,
            Func<string, int, string, string> searchCustomer,
            Func<string, int, string, string> searchAccount,
            Func<string, string, string> formatGuide)
        {
            var messages = BuildMessages(session, userMessage, lang, companyId);
            var tools = BuildTools();
            var toolsUsed = new List<string>();

            for (int round = 0; round < 8; round++)
            {
                JObject response = await PostChatAsync(messages, tools);
                JArray choices = response["choices"] as JArray;
                if (choices == null || choices.Count == 0)
                    return AiChatLlmResult.Fail(FallbackFromResponse(response, lang));

                JObject message = choices[0]["message"] as JObject;
                if (message == null)
                    return AiChatLlmResult.Fail(ErrorText(lang));

                JArray toolCalls = message["tool_calls"] as JArray;
                if (toolCalls == null || toolCalls.Count == 0)
                {
                    string content = message["content"]?.ToString()?.Trim();
                    if (string.IsNullOrWhiteSpace(content))
                        return AiChatLlmResult.Fail(ErrorText(lang));

                    return AiChatLlmResult.Ok(content, toolsUsed);
                }

                messages.Add(message);

                foreach (JToken call in toolCalls)
                {
                    string toolId = call["id"]?.ToString() ?? Guid.NewGuid().ToString("N");
                    string fnName = call["function"]?["name"]?.ToString() ?? "";
                    string argsJson = call["function"]?["arguments"]?.ToString() ?? "{}";

                    if (!string.IsNullOrWhiteSpace(fnName))
                        toolsUsed.Add(fnName);

                    string toolResult = ExecuteTool(
                        fnName, argsJson, companyId, lang, session,
                        runDataQuery, searchCustomer, searchAccount, formatGuide);

                    messages.Add(new JObject
                    {
                        ["role"] = "tool",
                        ["tool_call_id"] = toolId,
                        ["content"] = toolResult
                    });
                }

                // Name/account search needs user confirmation — ask directly, don't dump lists
                if (!string.IsNullOrWhiteSpace(session.PendingAction))
                {
                    return AiChatLlmResult.Ok(
                        clsAiChatNameSearch.FormatPendingUserMessage(session, companyId, lang),
                        toolsUsed);
                }
            }

            return AiChatLlmResult.Ok(
                clsAiChatLanguage.IsArabic(lang)
                    ? "أحتاج توضيحاً أكثر. ماذا تريد بالضبط؟"
                    : "I need a bit more detail. What would you like me to do?",
                toolsUsed);
        }

        private JArray BuildTools()
        {
            return new JArray
            {
                Tool("query_erp_data",
                    "Query live ERP data: counts, sales/purchases (invoice tables + InvoiceTypeID), GL balances (journal voucher header/details), cash/bank balances, financing/loans, reconciliation/settlements, lists and trends. Never guess numbers.",
                    new
                    {
                        type = "object",
                        properties = new
                        {
                            question = new { type = "string", description = "The data question rewritten in English or Arabic (translate from the user's language). Examples: 'how many customers', 'total sales this month', 'top 5 products'" }
                        },
                        required = new[] { "question" }
                    }),
                Tool("search_by_name",
                    "Search by name across ALL master data: chart of accounts (tbl_Accounts), customers/vendors (tbl_BusinessPartner), banks (tbl_Banks), cash drawers (tbl_CashDrawer). Default for «رصيد [name]» balance questions. Returns journal balance when confirmed.",
                    new
                    {
                        type = "object",
                        properties = new
                        {
                            name = new { type = "string", description = "Name, number, or partial name to search (any language)" }
                        },
                        required = new[] { "name" }
                    }),
                Tool("search_customer",
                    "Search customers or vendors in tbl_BusinessPartner ONLY when user explicitly asks about عميل/مورد/customer/vendor.",
                    new
                    {
                        type = "object",
                        properties = new
                        {
                            name = new { type = "string", description = "Name, phone, or partial name to search (any language)" }
                        },
                        required = new[] { "name" }
                    }),
                Tool("search_account",
                    "Search GL chart-of-accounts (tbl_Accounts) ONLY when user explicitly asks about دليل الحسابات/chart of accounts. For general name/balance lookups use search_by_name instead.",
                    new
                    {
                        type = "object",
                        properties = new
                        {
                            name = new { type = "string", description = "Account name or number (partial OK)" }
                        },
                        required = new[] { "name" }
                    }),
                Tool("get_erp_guide",
                    "Get step-by-step instructions for doing a task in the ERP (create invoice, payroll, journal voucher, etc.).",
                    new
                    {
                        type = "object",
                        properties = new
                        {
                            topic = new { type = "string", description = "Task topic e.g. sales invoice, payroll, reconciliation" }
                        },
                        required = new[] { "topic" }
                    }),
                Tool("get_system_knowledge",
                    "Get where a screen/report is in the menu, or explain an ERP concept (trial balance, reconciliation, cost center).",
                    new
                    {
                        type = "object",
                        properties = new
                        {
                            topic = new { type = "string", description = "Screen name or concept" }
                        },
                        required = new[] { "topic" }
                    }),
                Tool("list_erp_guides",
                    "List all available how-to guides in the ERP.",
                    new { type = "object", properties = new { } }),
                Tool("get_erp_schema",
                    "Get ERP table/column dictionary, join patterns, document types, and balance rules. Use when you need to understand how data is stored before answering complex questions.",
                    new
                    {
                        type = "object",
                        properties = new
                        {
                            section = new { type = "string", description = "Optional: summary, tables, joins, documentTypes, subAccounts, or full" }
                        }
                    })
            };
        }

        private static JObject Tool(string name, string description, object parameters) =>
            new()
            {
                ["type"] = "function",
                ["function"] = new JObject
                {
                    ["name"] = name,
                    ["description"] = description,
                    ["parameters"] = JObject.FromObject(parameters)
                }
            };

        private JArray BuildMessages(AiChatSession session, string userMessage, string lang, int companyId)
        {
            var messages = new JArray
            {
                new JObject
                {
                    ["role"] = "system",
                    ["content"] = BuildSystemPrompt(userMessage, lang, companyId)
                }
            };

            foreach (AiChatMessage item in session.History.TakeLast(24))
            {
                if (item.Role is "user" or "assistant")
                {
                    messages.Add(new JObject
                    {
                        ["role"] = item.Role,
                        ["content"] = item.Content
                    });
                }
            }

            return messages;
        }

        private string BuildSystemPrompt(string userMessage, string lang, int companyId)
        {
            string overview = clsAiChatKnowledge.GetSystemOverview("En");
            string loginNote = companyId <= 0
                ? "User is NOT logged into a company (CompanyId=0). For data tools, politely ask them to log in first. You can still explain ERP concepts and navigation."
                : $"User company database: CompanyId={companyId}. Use tools to fetch real data.";

            string schemaSummary = clsAiChatSchema.GetPromptSummary(lang);
            string langHint = clsAiChatLanguage.DescribeForPrompt(userMessage, lang);

            return $@"You are **MT SOFTS ERP Assistant** — a friendly, intelligent AI assistant like ChatGPT, built into an ERP system.
You can answer **anything** about the ERP: data, balances, navigation, how-to, and concepts.

## Language (CRITICAL)
- **Always reply in the SAME language the user writes in** (Arabic, English, French, Spanish, etc.).
- Detected language of latest message: **{langHint}**.
- Be natural and conversational — not robotic.

## ERP data model (CRITICAL — read before answering financial questions)
{schemaSummary}

### Key rules
1. **Balances source of truth** = tbl_JournalVoucherHeader + tbl_JournalVoucherDetails (ParentGuid → Guid). Never invent balances.
2. **SubAccountID** on JV details filters sub-ledgers: customer/vendor (via tbl_AccountSetting AR/AP), bank (tbl_Banks), cash (tbl_CashDrawer).
3. **Invoices** = tbl_InvoiceHeader + tbl_InvoiceDetails; type via InvoiceTypeID → tbl_JournalVoucherTypes (sales=3,10; purchases=2,22; etc.).
4. **Financing/loans** = tbl_FinancingHeader + tbl_FinancingDetails (installment sales & cash loans).
5. **Settlements** = tbl_Reconciliation links JVDetailsGuid to settled Amount (open debit = debit minus reconciled).
6. **Master data** (customers, accounts, items) can be read fully; use search tools for name lookups.

## Your capabilities
{overview}

## How to work
1. **Understand** what the user wants. If unclear, ask **one** short question.
2. **Name / balance lookups (default):**
   - Arabic **«رصيد [اسم]»** (e.g. رصيد محمد طه) → `search_by_name` across accounts, customers, vendors, banks, cash drawers.
   - Use `search_customer` **only** when user says عميل/مورد/زبون/customer/vendor.
   - Use `search_account` **only** when user explicitly asks about دليل الحسابات/chart of accounts.
3. **Counts, totals, trends, aggregate balances** (cash, bank, totals) → `query_erp_data`. Never invent numbers.
4. **How to do something in the system** → `get_erp_guide`. **Where is a screen** → `get_system_knowledge`.
5. **Complex schema questions** → `get_erp_schema`.
6. When search needs confirmation, wait for yes/no — max 3 options, conversational tone.
7. **Guide the user** through the system when they ask how to navigate or complete a task.
8. Summarize tool results naturally — don't dump raw JSON.

## Tools
- `query_erp_data` → live data (sales, purchases, JV totals, cash/bank, financing, reconciliation, counts, lists)
- `search_by_name` → unified lookup in accounts + customers/vendors + banks + cash (returns journal balance when confirmed)
- `search_customer` → customer/vendor lookup only
- `search_account` → chart-of-accounts lookup only
- `get_erp_guide` → step-by-step tasks
- `get_system_knowledge` → navigation & concepts
- `get_erp_schema` → table/column dictionary & join rules
- `list_erp_guides` → all guides

## Context
{loginNote}

Be concise but helpful. Use bullet points or numbered steps when guiding.";
        }

        private async Task<JObject> PostChatAsync(JArray messages, JArray tools)
        {
            string url = _settings.BaseUrl.TrimEnd('/') + "/chat/completions";
            var payload = new JObject
            {
                ["model"] = _settings.Model,
                ["messages"] = messages,
                ["tools"] = tools,
                ["tool_choice"] = "auto",
                ["temperature"] = 0.4
            };

            using var request = new HttpRequestMessage(HttpMethod.Post, url);

            if (!string.IsNullOrWhiteSpace(_settings.ApiKey))
                request.Headers.Add("Authorization", "Bearer " + _settings.ApiKey);
            else if (!_settings.Provider.Equals("Ollama", StringComparison.OrdinalIgnoreCase))
                request.Headers.Add("Authorization", "Bearer " + (_settings.ApiKey ?? ""));

            request.Content = new StringContent(payload.ToString(Formatting.None), Encoding.UTF8, "application/json");

            using HttpResponseMessage response = await Http.SendAsync(request);
            string body = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
                throw new InvalidOperationException($"LLM HTTP {(int)response.StatusCode}: {body}");

            return JObject.Parse(body);
        }

        private static string ExecuteTool(
            string fnName,
            string argsJson,
            int companyId,
            string lang,
            AiChatSession session,
            Func<string, string, int, string, string> runDataQuery,
            Func<string, int, string, string> searchCustomer,
            Func<string, int, string, string> searchAccount,
            Func<string, string, string> formatGuide)
        {
            if (companyId <= 0 && fnName is "query_erp_data" or "search_customer" or "search_account" or "search_by_name")
            {
                return clsAiChatLanguage.IsArabic(lang)
                    ? "ERROR: المستخدم غير مسجل أو لم يختر شركة. اطلب منه تسجيل الدخول."
                    : "ERROR: User not logged in or no company selected. Ask them to log in.";
            }

            JObject args = string.IsNullOrWhiteSpace(argsJson) ? new JObject() : JObject.Parse(argsJson);

            switch (fnName)
            {
                case "query_erp_data":
                {
                    string q = args["question"]?.ToString() ?? "";
                    string result = runDataQuery(q, "", companyId, lang);
                    return string.IsNullOrWhiteSpace(result)
                        ? (clsAiChatLanguage.IsArabic(lang) ? "NO_DATA: لا توجد بيانات مطابقة." : "NO_DATA: No matching data found.")
                        : result;
                }
                case "search_customer":
                    return searchCustomer(args["name"]?.ToString() ?? "", companyId, lang);
                case "search_by_name":
                    return searchAccount(args["name"]?.ToString() ?? "", companyId, lang);
                case "search_account":
                    return searchAccount(args["name"]?.ToString() ?? "", companyId, lang);
                case "get_erp_guide":
                {
                    string topic = args["topic"]?.ToString() ?? "";
                    var guide = clsAiChatGuides.GetById(topic)
                        ?? clsAiChatGuides.MatchGuide(clsAiChatKnowledge.RewriteWithSynonyms(clsAiChat.NormalizePublic(topic)));
                    if (guide == null)
                    {
                        var list = clsAiChatGuides.SearchGuides(clsAiChat.NormalizePublic(topic), 1);
                        guide = list.Count > 0 ? list[0] : null;
                    }
                    return guide == null
                        ? clsAiChatGuides.ListGuideTopics(lang)
                        : formatGuide(guide.Id, lang);
                }
                case "list_erp_guides":
                    return clsAiChatGuides.ListGuideTopics(lang);
                case "get_erp_schema":
                {
                    string section = args["section"]?.ToString()?.Trim().ToLowerInvariant() ?? "summary";
                    return section switch
                    {
                        "full" => clsAiChatSchema.GetSchemaJson(),
                        "tables" => JsonConvert.SerializeObject(clsAiChatSchema.Tables.Values, Formatting.Indented),
                        "joins" => JsonConvert.SerializeObject(clsAiChatSchema.Joins, Formatting.Indented),
                        "documenttypes" or "document_types" => JsonConvert.SerializeObject(new
                        {
                            table = clsAiChatSchema.DocTypeTable,
                            sales = clsAiChatSchema.DocTypes.Sales,
                            purchases = clsAiChatSchema.DocTypes.Purchases,
                            financing = clsAiChatSchema.DocTypes.Financing
                        }, Formatting.Indented),
                        "subaccounts" or "sub_accounts" => JsonConvert.SerializeObject(clsAiChatSchema.SubAccountRules, Formatting.Indented),
                        _ => clsAiChatSchema.GetPromptSummary(lang)
                    };
                }
                case "get_system_knowledge":
                {
                    string topic = args["topic"]?.ToString() ?? "";
                    string norm = clsAiChatKnowledge.RewriteWithSynonyms(clsAiChat.NormalizePublic(topic));
                    var entry = clsAiChatKnowledge.SearchBest(norm);
                    return entry != null
                        ? clsAiChatKnowledge.FormatEntry(entry, lang)
                        : clsAiChatKnowledge.GetSystemOverview(lang);
                }
                default:
                    return "Unknown tool.";
            }
        }

        private static string FallbackFromResponse(JObject response, string lang)
        {
            string err = response["error"]?["message"]?.ToString();
            if (!string.IsNullOrWhiteSpace(err))
                return (clsAiChatLanguage.IsArabic(lang) ? "خطأ في الذكاء الاصطناعي: " : "AI error: ") + err;
            return ErrorText(lang);
        }

        private static string ErrorText(string lang) =>
            clsAiChatLanguage.IsArabic(lang)
                ? "تعذر الحصول على رد من المساعد الذكي."
                : "Could not get a response from the AI assistant.";
    }

    public sealed class AiChatLlmResult
    {
        public bool Success { get; init; }
        public string Content { get; init; } = "";
        public List<string> ToolsUsed { get; init; } = new();

        public static AiChatLlmResult Ok(string content, List<string> tools) =>
            new() { Success = true, Content = content, ToolsUsed = tools };

        public static AiChatLlmResult Fail(string message) =>
            new() { Success = false, Content = message };
    }

    public sealed class AiChatSettings
    {
        public bool Enabled { get; set; }
        public string Provider { get; set; } = "OpenAI";
        public string ApiKey { get; set; } = "";
        public string Model { get; set; } = "gpt-4o-mini";
        public string BaseUrl { get; set; } = "https://api.openai.com/v1";
    }
}
