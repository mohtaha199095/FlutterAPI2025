using Microsoft.Extensions.Configuration;
using System;
using System.Data;

namespace WebApplication2.cls
{
    /// <summary>
    /// Config-based administrator login (see AdminLogin in appsettings).
    /// After credentials match, resolves an employee row for the selected company.
    /// </summary>
    public static class clsAdminLogin
    {
        public static bool IsEnabled(IConfiguration configuration) =>
            configuration != null && configuration.GetValue("AdminLogin:Enabled", false);

        public static bool CredentialsMatch(
            IConfiguration configuration,
            string userName,
            string password,
            string email)
        {
            if (!IsEnabled(configuration)) return false;

            var adminEmail = (configuration["AdminLogin:Email"] ?? "").Trim();
            var adminUser = (configuration["AdminLogin:UserName"] ?? adminEmail).Trim();
            var adminPass = configuration["AdminLogin:Password"] ?? "";

            if (string.IsNullOrEmpty(adminPass)) return false;

            var u = (userName ?? "").Trim();
            var e = (email ?? "").Trim();
            var p = password ?? "";

            if (!string.Equals(p, adminPass, StringComparison.Ordinal)) return false;

            return string.Equals(u, adminUser, StringComparison.OrdinalIgnoreCase)
                || string.Equals(u, adminEmail, StringComparison.OrdinalIgnoreCase)
                || string.Equals(e, adminEmail, StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Finds an employee to attach to the admin session for the given company.
        /// </summary>
        public static DataTable ResolveEmployeeForCompany(
            clsEmployee clsEmployee,
            int companyId,
            IConfiguration configuration)
        {
            var adminEmail = (configuration["AdminLogin:Email"] ?? "").Trim();

            if (!string.IsNullOrEmpty(adminEmail))
            {
                var byEmail = clsEmployee.SelectEmployee(
                    0, "", "", "", "", adminEmail, "", companyId, -1);
                if (byEmail != null && byEmail.Rows.Count > 0) return byEmail;
            }

            var systemUsers = clsEmployee.SelectEmployee(
                0, "", "", "", "", "", "", companyId, 1);
            if (systemUsers != null && systemUsers.Rows.Count > 0) return systemUsers;

            return clsEmployee.SelectEmployee(
                0, "", "", "", "", "", "", companyId, -1);
        }
    }
}
