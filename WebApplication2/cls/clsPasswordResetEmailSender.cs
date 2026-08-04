using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using MimeKit;
using System;
using System.Linq;

namespace WebApplication2.cls
{
    public static class clsPasswordResetEmailSender
    {
        public static void LogOtpForDevelopment(
            IConfiguration configuration,
            IHostEnvironment environment,
            ILogger logger,
            string recipientEmail,
            string otpCode,
            int expiryMinutes)
        {
            IConfigurationSection section = configuration?.GetSection("PasswordResetEmail");
            bool logToConsole = section?.GetValue<bool>("LogOtpToConsoleInDevelopment") ?? false;
            if (environment?.IsDevelopment() != true && !logToConsole)
            {
                return;
            }

            string line =
                $"[PasswordReset] OTP for {recipientEmail}: {otpCode} (expires in {expiryMinutes} min)";
            Console.WriteLine(line);
            logger?.LogWarning("{Line}", line);
        }

        public static bool ShouldExposeOtpInApiResponse(IConfiguration configuration, IHostEnvironment environment)
        {
            if (environment?.IsDevelopment() != true) return false;
            return configuration?.GetSection("PasswordResetEmail")
                .GetValue("ExposeOtpInDevelopmentResponse", true) ?? true;
        }

        static SecureSocketOptions GetSocketOptions(int port, bool useSsl)
        {
            if (port == 465)
            {
                return SecureSocketOptions.SslOnConnect;
            }

            return useSsl ? SecureSocketOptions.StartTls : SecureSocketOptions.None;
        }

        public static bool TrySend(
            IConfiguration configuration,
            IHostEnvironment environment,
            ILogger logger,
            string recipientEmail,
            string otpCode,
            int expiryMinutes)
        {
            if (string.IsNullOrWhiteSpace(recipientEmail) || string.IsNullOrWhiteSpace(otpCode))
            {
                return false;
            }

            IConfigurationSection section = configuration?.GetSection("PasswordResetEmail");
            bool enabled = section?.GetValue<bool>("Enabled") ?? false;

            if (!enabled)
            {
                LogOtpForDevelopment(configuration, environment, logger, recipientEmail, otpCode, expiryMinutes);
                return false;
            }

            string smtpHost = section["SmtpHost"] ?? "smtp.zoho.com";
            int smtpPort = section.GetValue<int>("SmtpPort", 587);
            bool useSsl = section.GetValue<bool>("UseSsl", true);
            string userName = section["UserName"] ?? string.Empty;
            string password = section["Password"] ?? string.Empty;
            string fromAddress = section["FromAddress"] ?? userName;
            string fromName = section["FromDisplayName"] ?? "MT Softs Support";

            if (string.IsNullOrWhiteSpace(userName) || string.IsNullOrWhiteSpace(password))
            {
                logger?.LogError(
                    "PasswordResetEmail.Enabled is true but UserName/Password are empty. For Zoho, create an application-specific password for {User}.",
                    userName);
                LogOtpForDevelopment(configuration, environment, logger, recipientEmail, otpCode, expiryMinutes);
                return false;
            }

            var message = new MimeMessage();
            message.From.Add(new MailboxAddress(fromName, fromAddress));
            message.To.Add(MailboxAddress.Parse(recipientEmail.Trim()));
            message.Subject = "Your password reset verification code";
            message.Body = new TextPart("plain")
            {
                Text =
                    $"Your one-time verification code is: {otpCode}{Environment.NewLine}{Environment.NewLine}" +
                    $"This code expires in {expiryMinutes} minutes.{Environment.NewLine}" +
                    "Sign in with your email as the username and this code as the password, then change your password.",
            };

            var socketOptions = GetSocketOptions(smtpPort, useSsl);
            string[] hostsToTry = new[] { smtpHost, "smtppro.zoho.com", "smtp.zoho.com" }
                .Where(h => !string.IsNullOrWhiteSpace(h))
                .Select(h => h.Trim())
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToArray();

            Exception lastError = null;
            foreach (string host in hostsToTry)
            {
                try
                {
                    using var client = new SmtpClient();
                    client.Connect(host, smtpPort, socketOptions);
                    client.Authenticate(userName, password);
                    client.Send(message);
                    client.Disconnect(true);

                    logger?.LogInformation(
                        "Password reset OTP email sent to {Email} via {Host}:{Port}",
                        recipientEmail,
                        host,
                        smtpPort);
                    return true;
                }
                catch (Exception ex)
                {
                    lastError = ex;
                    logger?.LogWarning(ex, "SMTP attempt failed for host {Host}", host);
                }
            }

            logger?.LogError(
                lastError,
                "Failed to send password reset email to {Email}. Tried hosts: {Hosts}. Use a Zoho application-specific password with TFA enabled.",
                recipientEmail,
                string.Join(", ", hostsToTry));
            LogOtpForDevelopment(configuration, environment, logger, recipientEmail, otpCode, expiryMinutes);
            return false;
        }
    }
}
