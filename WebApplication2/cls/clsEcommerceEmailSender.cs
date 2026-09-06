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
    public static class clsEcommerceEmailSender
    {
        static SecureSocketOptions GetSocketOptions(int port, bool useSsl)
        {
            if (port == 465) return SecureSocketOptions.SslOnConnect;
            return useSsl ? SecureSocketOptions.StartTls : SecureSocketOptions.None;
        }

        /// <summary>
        /// Notify company of a new online order. Reuses PasswordResetEmail SMTP settings.
        /// </summary>
        public static bool TrySendOrderNotification(
            IConfiguration configuration,
            IHostEnvironment environment,
            ILogger logger,
            string recipientEmail,
            string shopName,
            string orderNo,
            string customerName,
            string phone,
            string address,
            decimal total)
        {
            if (string.IsNullOrWhiteSpace(recipientEmail) || string.IsNullOrWhiteSpace(orderNo))
                return false;

            IConfigurationSection section = configuration?.GetSection("PasswordResetEmail");
            bool enabled = section?.GetValue<bool>("Enabled") ?? false;

            string subject = $"New online order {orderNo} — {shopName}";
            string body =
                $"A new online order was placed for {shopName}.\n\n" +
                $"Order: {orderNo}\n" +
                $"Customer: {customerName}\n" +
                $"Phone: {phone}\n" +
                $"Address: {address}\n" +
                $"Total: {total:0.###}\n\n" +
                "Open the E-commerce module → Online Orders in MT Softs ERP to review.";

            if (!enabled)
            {
                string line = $"[EcommerceOrder] {subject} | {customerName} {phone}";
                Console.WriteLine(line);
                logger?.LogWarning("{Line}", line);
                return false;
            }

            string smtpHost = section["SmtpHost"] ?? "smtp.zoho.com";
            int smtpPort = section.GetValue<int>("SmtpPort", 587);
            bool useSsl = section.GetValue<bool>("UseSsl", true);
            string userName = section["UserName"] ?? string.Empty;
            string password = section["Password"] ?? string.Empty;
            string fromAddress = section["FromAddress"] ?? userName;
            string fromName = section["FromDisplayName"] ?? "MT Softs";

            var message = new MimeMessage();
            message.From.Add(new MailboxAddress(fromName, fromAddress));
            message.To.Add(MailboxAddress.Parse(recipientEmail.Trim()));
            message.Subject = subject;
            message.Body = new TextPart("plain") { Text = body };

            var socketOptions = GetSocketOptions(smtpPort, useSsl);
            string[] hostsToTry = new[] { smtpHost, "smtppro.zoho.com", "smtp.zoho.com" }
                .Where(h => !string.IsNullOrWhiteSpace(h))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToArray();

            foreach (string host in hostsToTry)
            {
                try
                {
                    using var client = new SmtpClient();
                    client.Connect(host, smtpPort, socketOptions);
                    if (!string.IsNullOrEmpty(userName))
                        client.Authenticate(userName, password);
                    client.Send(message);
                    client.Disconnect(true);
                    logger?.LogInformation("Ecommerce order email sent to {Email} via {Host}", recipientEmail, host);
                    return true;
                }
                catch (Exception ex)
                {
                    logger?.LogWarning(ex, "Ecommerce email failed via {Host}", host);
                }
            }

            return false;
        }
    }
}
