using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System;
using System.Linq;
using System.Threading.Tasks;
using WebApplication2.cls;

namespace WebApplication2.Controllers
{
    [ApiController]
    [Route("api/ctlAiChat")]
    public class ctlAiChat : Controller
    {
        private readonly clsAiChatAgent _agent;
        private readonly clsAiChatLlm _llm;

        public ctlAiChat(IConfiguration configuration)
        {
            _agent = new clsAiChatAgent(configuration);
            _llm = new clsAiChatLlm(configuration);
        }

        public sealed class AiChatRequest
        {
            public string SessionId { get; set; } = "";
            public string ChatInput { get; set; } = "";
            public int CompanyId { get; set; }
            public string Lang { get; set; } = "auto";
        }

        public sealed class AiChatSessionRequest
        {
            public string SessionId { get; set; } = "";
            public int CompanyId { get; set; }
            public string Lang { get; set; } = "auto";
        }

        [HttpGet]
        [Route("Status")]
        public string Status()
        {
            var s = _llm.Settings;
            return JsonConvert.SerializeObject(new
            {
                aiEnabled = _llm.IsConfigured,
                provider = s.Provider,
                model = s.Model,
                mode = _llm.IsConfigured ? "ai" : "rules"
            });
        }

        [HttpPost]
        [Route("History")]
        public string History([FromBody] AiChatSessionRequest request)
        {
            var history = _agent.GetHistory(request?.SessionId ?? "");
            return JsonConvert.SerializeObject(new
            {
                sessionId = history.SessionId,
                messages = history.Messages.Select(m => new
                {
                    role = m.Role,
                    content = m.Content,
                    at = m.At
                })
            });
        }

        [HttpPost]
        [Route("Welcome")]
        public string Welcome([FromBody] AiChatSessionRequest request)
        {
            var welcome = _agent.GetWelcome(request?.CompanyId ?? 0, request?.Lang ?? "auto");
            return JsonConvert.SerializeObject(new
            {
                output = welcome.Text,
                mode = welcome.Mode,
                lang = welcome.Lang,
                suggestions = welcome.Suggestions
            });
        }

        [HttpPost]
        [Route("Reset")]
        public string Reset([FromBody] AiChatSessionRequest request)
        {
            AiChatSessionStore.Clear(request?.SessionId ?? "");
            var welcome = _agent.GetWelcome(request?.CompanyId ?? 0, request?.Lang ?? "auto");
            return JsonConvert.SerializeObject(new
            {
                ok = true,
                output = welcome.Text,
                mode = welcome.Mode,
                lang = welcome.Lang,
                suggestions = welcome.Suggestions
            });
        }

        [HttpPost]
        [Route("Chat")]
        public async Task<string> Chat([FromBody] AiChatRequest request)
        {
            try
            {
                AiChatAgentResult result = await _agent.ProcessAsync(
                    request?.SessionId ?? "",
                    request?.ChatInput ?? "",
                    request?.CompanyId ?? 0,
                    request?.Lang ?? "auto");

                return JsonConvert.SerializeObject(new
                {
                    output = result.Output,
                    mode = result.Mode,
                    lang = result.Lang,
                    suggestions = result.Suggestions,
                    toolsUsed = result.ToolsUsed
                });
            }
            catch (Exception ex)
            {
                return JsonConvert.SerializeObject(new
                {
                    output = "Assistant error: " + ex.Message,
                    mode = "error",
                    lang = "En",
                    suggestions = new string[0],
                    toolsUsed = new string[0]
                });
            }
        }
    }
}
