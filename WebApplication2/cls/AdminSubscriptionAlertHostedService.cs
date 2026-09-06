using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;
using WebApplication2.cls;

namespace WebApplication2
{
    /// <summary>
    /// Sends subscription expiry alert emails once per day (configurable).
    /// </summary>
    public class AdminSubscriptionAlertHostedService : BackgroundService
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<AdminSubscriptionAlertHostedService> _logger;
        private DateTime _lastRunDate = DateTime.MinValue;

        public AdminSubscriptionAlertHostedService(
            IConfiguration configuration,
            ILogger<AdminSubscriptionAlertHostedService> logger)
        {
            _configuration = configuration;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    if (ShouldRunNow())
                    {
                        RunAlertJob();
                        _lastRunDate = DateTime.UtcNow.Date;
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Admin subscription alert job failed.");
                }

                await Task.Delay(TimeSpan.FromMinutes(30), stoppingToken);
            }
        }

        bool ShouldRunNow()
        {
            if (_configuration.GetValue("AdminAlerts:SubscriptionExpiryEnabled", true) != true)
            {
                return false;
            }

            if (!clsAdminLogin.IsEnabled(_configuration))
            {
                return false;
            }

            int hourUtc = _configuration.GetValue("AdminAlerts:SubscriptionExpiryHourUtc", 6);
            var now = DateTime.UtcNow;
            if (now.Hour != hourUtc)
            {
                return false;
            }

            if (_lastRunDate == now.Date)
            {
                return false;
            }

            if (WasAlreadySentToday())
            {
                _lastRunDate = now.Date;
                return false;
            }

            return true;
        }

        bool WasAlreadySentToday()
        {
            try
            {
                var sql = new clsSQL();
                var dt = sql.ExecuteQueryStatement(@"
SELECT TOP 1 1 AS X
FROM tbl_AdminAuditLog
WHERE Action = 'SubscriptionExpiryAlertScheduled'
  AND Success = 1
  AND CreatedAt >= CAST(GETDATE() AS DATE)",
                    sql.MainDataBaseconString,
                    null);
                return dt != null && dt.Rows.Count > 0;
            }
            catch
            {
                return false;
            }
        }

        void RunAlertJob()
        {
            int daysAhead = _configuration.GetValue("AdminAlerts:SubscriptionExpiryDaysAhead", 7);
            var result = new clsAdminOps().SendSubscriptionExpiryAlerts(_configuration, daysAhead);

            clsAdminAuditLog.Write(
                "SubscriptionExpiryAlertScheduled",
                "System",
                $"DaysAhead={daysAhead}; Result={result}",
                "Scheduler",
                true);

            _logger.LogInformation("Admin subscription expiry alert job completed: {Result}", result);
        }
    }
}
