using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;

namespace WebApplication2.cls
{
    public static class clsAdminAuthHelper
    {
        public static string ReadToken(HttpRequest request)
        {
            if (request == null) return "";

            if (request.Headers.TryGetValue("X-Admin-Token", out var headerValues))
            {
                string header = (headerValues.ToString() ?? "").Trim();
                if (!string.IsNullOrEmpty(header)) return header;
            }

            return Simulate.String(request.Query["AdminToken"]);
        }

        public static bool IsSensitiveAdminRequestAvailable(
            IConfiguration configuration,
            HttpRequest request,
            out IActionResult errorResult)
        {
            return TryAuthorizeAdmin(configuration, request, out errorResult, out _, out _);
        }

        public static bool TryAuthorizeAdmin(
            IConfiguration configuration,
            HttpRequest request,
            out IActionResult errorResult,
            out string userName,
            out string email)
        {
            errorResult = null;
            userName = "";
            email = "";

            if (!clsAdminLogin.IsEnabled(configuration))
            {
                errorResult = new ObjectResult(new { ok = false, message = "Admin tools are disabled." })
                {
                    StatusCode = 403
                };
                return false;
            }

            string token = ReadToken(request);
            if (!clsAdminSession.TryValidateToken(token, out userName, out email))
            {
                errorResult = new UnauthorizedObjectResult(new
                {
                    ok = false,
                    message = "Admin session expired or invalid. Sign in again with password and email verification code."
                });
                return false;
            }

            return true;
        }

        public static string ReadClientIp(HttpRequest request)
        {
            if (request == null) return "";

            if (request.Headers.TryGetValue("X-Forwarded-For", out var forwarded))
            {
                string value = (forwarded.ToString() ?? "").Trim();
                if (!string.IsNullOrEmpty(value))
                {
                    int comma = value.IndexOf(',');
                    return comma > 0 ? value.Substring(0, comma).Trim() : value;
                }
            }

            return request.HttpContext?.Connection?.RemoteIpAddress?.ToString() ?? "";
        }
    }
}
