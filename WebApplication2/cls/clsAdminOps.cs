using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Data;
using Newtonsoft.Json;
using WebApplication2.MainClasses;

namespace WebApplication2.cls
{
    public class clsAdminOps
    {
        public object RunDatabaseMigration(int companyId)
        {
            if (companyId <= 0)
            {
                return new { ok = false, message = "CompanyID is required." };
            }

            try
            {
                var version = new clsDataBaseVersion();
                DataTable dt = version.SelectDataBaseVersion(0, companyId);
                decimal before = 0;
                if (dt != null && dt.Rows.Count > 0)
                {
                    before = Simulate.decimal_(dt.Rows[0]["VersionNumber"]);
                }

                version.checkDatabaseUpdates(before, companyId);

                DataTable afterDt = version.SelectDataBaseVersion(0, companyId);
                decimal after = before;
                if (afterDt != null && afterDt.Rows.Count > 0)
                {
                    after = Simulate.decimal_(afterDt.Rows[0]["VersionNumber"]);
                }

                new clsAuditAdmin().RefreshCompanySnapshot(companyId, force: true);
                return new
                {
                    ok = true,
                    message = "Database migration completed.",
                    companyId,
                    versionBefore = before,
                    versionAfter = after,
                };
            }
            catch (Exception ex)
            {
                return new { ok = false, message = ex.Message, companyId };
            }
        }

        public object RunDatabaseMigrationAll(int maxCompanies = 100)
        {
            var sql = new clsSQL();
            DataTable companies = sql.ExecuteQueryStatement(
                "SELECT ID FROM tbl_Company ORDER BY ID",
                sql.MainDataBaseconString,
                null);

            int processed = 0;
            int succeeded = 0;
            var failures = new List<object>();

            if (companies != null)
            {
                foreach (DataRow row in companies.Rows)
                {
                    if (processed >= maxCompanies) break;
                    int companyId = Simulate.Integer32(row["ID"]);
                    processed++;
                    var result = RunDatabaseMigration(companyId);
                    if (result is IDictionary<string, object> map &&
                        map.TryGetValue("ok", out var okObj) &&
                        okObj is bool ok && ok)
                    {
                        succeeded++;
                    }
                    else
                    {
                        failures.Add(new { companyId, result });
                    }
                }
            }

            return new
            {
                ok = failures.Count == 0,
                processed,
                succeeded,
                failed = failures.Count,
                failures,
            };
        }

        public object RefreshAllCompanySnapshots(int maxCompanies = 200)
        {
            var sql = new clsSQL();
            DataTable companies = sql.ExecuteQueryStatement(
                "SELECT TOP (@TopN) ID FROM tbl_Company ORDER BY ID",
                sql.MainDataBaseconString,
                new[] { new SqlParameter("@TopN", SqlDbType.Int) { Value = maxCompanies } });

            int count = 0;
            var audit = new clsAuditAdmin();
            if (companies != null)
            {
                foreach (DataRow row in companies.Rows)
                {
                    int companyId = Simulate.Integer32(row["ID"]);
                    audit.RefreshCompanySnapshot(companyId, force: true);
                    count++;
                }
            }

            return new { ok = true, refreshed = count };
        }

        public DataTable SelectCrossTenantActivity(int topN = 100)
        {
            int take = topN <= 0 ? 100 : Math.Min(topN, 500);
            var sql = new clsSQL();
            var results = new DataTable();
            results.Columns.Add("CompanyID", typeof(int));
            results.Columns.Add("CompanyName", typeof(string));
            results.Columns.Add("UserName", typeof(string));
            results.Columns.Add("LoginTime", typeof(DateTime));
            results.Columns.Add("LogoutTime", typeof(DateTime));
            results.Columns.Add("IsActive", typeof(bool));
            results.Columns.Add("Source", typeof(string));

            DataTable companies = sql.ExecuteQueryStatement(
                "SELECT ID, ISNULL(NULLIF(EName,''), AName) AS CompanyName FROM tbl_Company WHERE ISNULL(IsSuspended,0)=0 ORDER BY ID",
                sql.MainDataBaseconString,
                null);

            if (companies == null) return results;

            foreach (DataRow company in companies.Rows)
            {
                if (results.Rows.Count >= take) break;
                int companyId = Simulate.Integer32(company["ID"]);
                string companyName = Simulate.String(company["CompanyName"]);
                string con = sql.CreateDataBaseConnectionString(companyId);
                if (string.IsNullOrWhiteSpace(con)) continue;

                try
                {
                    DataTable sessions = sql.ExecuteQueryStatement(@"
SELECT TOP 20 UserName, LoginTime, LogoutTime, IsActive
FROM tbl_AuditUserSession
WHERE CompanyID = @CompanyID
ORDER BY LoginTime DESC",
                        con,
                        new[] { new SqlParameter("@CompanyID", SqlDbType.Int) { Value = companyId } });

                    if (sessions == null) continue;
                    foreach (DataRow s in sessions.Rows)
                    {
                        if (results.Rows.Count >= take) break;
                        results.Rows.Add(
                            companyId,
                            companyName,
                            Simulate.String(s["UserName"]),
                            s["LoginTime"] == DBNull.Value ? (object)DBNull.Value : s["LoginTime"],
                            s["LogoutTime"] == DBNull.Value ? (object)DBNull.Value : s["LogoutTime"],
                            s["IsActive"] == DBNull.Value ? false : Simulate.Bool(s["IsActive"]),
                            "Session");
                    }
                }
                catch
                {
                    // Company DB may not have audit tables yet.
                }
            }

            return results;
        }

        public string BuildImpersonationLoginJson(int companyId, IConfiguration configuration, string clientIp)
        {
            if (companyId <= 0) return JsonConvert.SerializeObject(new { ok = false, message = "CompanyID is required." });

            var employee = new clsEmployee();
            DataTable dt = clsAdminLogin.ResolveEmployeeForCompany(employee, companyId, configuration);
            if (dt == null || dt.Rows.Count == 0)
            {
                return JsonConvert.SerializeObject(new
                {
                    ok = false,
                    message = "No system user found for this company. Add a system employee first.",
                });
            }

            int userId = Simulate.Integer32(dt.Rows[0]["ID"]);
            string userName = Simulate.String(dt.Rows[0]["UserName"]);
            var ctx = new AuditContext
            {
                DeviceInfo = "AdminPortal",
                AppVersion = "Admin",
                Platform = "AdminImpersonate",
                IPAddress = clientIp ?? "",
            };
            var session = clsAuditService.StartSession(ctx, userId, userName, companyId, "AdminImpersonate");

            if (!dt.Columns.Contains("SessionGuid"))
                dt.Columns.Add("SessionGuid", typeof(string));
            foreach (DataRow row in dt.Rows)
                row["SessionGuid"] = session.SessionGuid.ToString();

            return JsonConvert.SerializeObject(new { ok = true, rows = dt });
        }

        public object SendSubscriptionExpiryAlerts(IConfiguration configuration, int daysAhead = 7)
        {
            var sql = new clsSQL();
            SqlParameter[] prm =
            {
                new SqlParameter("@Days", SqlDbType.Int) { Value = daysAhead },
            };

            DataTable dt = sql.ExecuteQueryStatement(@"
SELECT ID, ISNULL(NULLIF(EName,''), AName) AS CompanyName, SubscriptionExpiry
FROM tbl_Company
WHERE SubscriptionExpiry IS NOT NULL
  AND SubscriptionExpiry <= DATEADD(day, @Days, GETDATE())
ORDER BY SubscriptionExpiry",
                sql.MainDataBaseconString,
                prm);

            if (dt == null || dt.Rows.Count == 0)
            {
                return new { ok = true, sent = false, message = "No subscriptions expiring soon.", count = 0 };
            }

            string adminEmail = (configuration["AdminLogin:Email"] ?? "").Trim();
            if (string.IsNullOrWhiteSpace(adminEmail))
            {
                return new { ok = false, message = "AdminLogin:Email is not configured." };
            }

            var lines = new List<string> { "The following tenant subscriptions need attention:", "" };
            foreach (DataRow row in dt.Rows)
            {
                lines.Add($"- {Simulate.String(row["CompanyName"])} (#{Simulate.Integer32(row["ID"])}) expires {Simulate.String(row["SubscriptionExpiry"])}");
            }

            string body = string.Join(Environment.NewLine, lines);
            bool sent = TrySendAdminPlainEmail(configuration, adminEmail,
                "Tenant subscription expiry alert",
                body);

            return new { ok = sent, sent, count = dt.Rows.Count, message = sent ? "Alert email sent." : "SMTP not configured — see company list in admin portal." };
        }

        public decimal ResolveTargetSchemaVersion()
        {
            try
            {
                var version = new clsDataBaseVersion();
                DataTable dt = version.SelectDataBaseVersion(0, 0);
                if (dt != null && dt.Rows.Count > 0)
                {
                    return Simulate.decimal_(dt.Rows[0]["VersionNumber"]);
                }
            }
            catch
            {
                // Fall through.
            }

            return Simulate.decimal_(10.57);
        }

        public DataTable SelectDatabaseMigrationDrift(bool behindOnly = true, int topN = 200)
        {
            decimal targetVersion = ResolveTargetSchemaVersion();
            int take = topN <= 0 ? 200 : Math.Min(topN, 500);

            var sql = new clsSQL();
            SqlParameter[] prm =
            {
                new SqlParameter("@TargetVersion", SqlDbType.Decimal) { Value = targetVersion },
                new SqlParameter("@TopN", SqlDbType.Int) { Value = take },
            };

            string filter = behindOnly
                ? "AND ISNULL(C.DBVersionCache, 0) < @TargetVersion"
                : "";

            return sql.ExecuteQueryStatement($@"
SELECT TOP (@TopN)
    C.ID AS CompanyID,
    ISNULL(NULLIF(C.EName, ''), C.AName) AS CompanyName,
    C.DataBaseName,
    ISNULL(C.IsSuspended, 0) AS IsSuspended,
    ISNULL(C.DBVersionCache, 0) AS TenantVersion,
    @TargetVersion AS TargetVersion,
    (@TargetVersion - ISNULL(C.DBVersionCache, 0)) AS VersionGap,
    S.DBVersion AS SnapshotDBVersion,
    S.UpdatedAt AS SnapshotUpdatedAt
FROM tbl_Company C
LEFT JOIN tbl_AuditCompanySnapshot S ON S.CompanyID = C.ID
WHERE 1 = 1 {filter}
ORDER BY VersionGap DESC, C.ID",
                sql.MainDataBaseconString,
                prm);
        }

        static bool TrySendAdminPlainEmail(IConfiguration configuration, string to, string subject, string body)
        {
            try
            {
                var section = configuration?.GetSection("PasswordResetEmail");
                if (section?.GetValue<bool>("Enabled") != true) return false;
                string smtpHost = section["SmtpHost"] ?? "smtp.zoho.com";
                int smtpPort = section.GetValue<int>("SmtpPort", 587);
                bool useSsl = section.GetValue<bool>("UseSsl", true);
                string userName = section["UserName"] ?? "";
                string password = section["Password"] ?? "";
                string fromAddress = section["FromAddress"] ?? userName;
                string fromName = section["FromDisplayName"] ?? "MT Softs Support";
                if (string.IsNullOrWhiteSpace(userName) || string.IsNullOrWhiteSpace(password)) return false;

                var message = new MimeKit.MimeMessage();
                message.From.Add(new MimeKit.MailboxAddress(fromName, fromAddress));
                message.To.Add(MimeKit.MailboxAddress.Parse(to));
                message.Subject = subject;
                message.Body = new MimeKit.TextPart("plain") { Text = body };

                using var client = new MailKit.Net.Smtp.SmtpClient();
                var socketOptions = smtpPort == 465
                    ? MailKit.Security.SecureSocketOptions.SslOnConnect
                    : (useSsl ? MailKit.Security.SecureSocketOptions.StartTls : MailKit.Security.SecureSocketOptions.None);
                client.Connect(smtpHost, smtpPort, socketOptions);
                client.Authenticate(userName, password);
                client.Send(message);
                client.Disconnect(true);
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}
