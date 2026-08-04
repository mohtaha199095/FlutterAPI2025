using Microsoft.Extensions.Configuration;
using System;
using System.Data;
using WebApplication2.MainClasses;

namespace WebApplication2.cls
{
    public static class clsAdminDiagnostics
    {
        public static bool IsAvailable(IConfiguration configuration) =>
            clsAdminLogin.IsEnabled(configuration);

        public static object Ping(string apiHost)
        {
            return new
            {
                ok = true,
                message = "API is responding",
                apiHost = apiHost ?? "",
                serverTimeUtc = DateTime.UtcNow.ToString("o"),
            };
        }

        public static object CheckMainDatabase(IConfiguration configuration)
        {
            if (!IsAvailable(configuration))
            {
                return new { ok = false, message = "Admin diagnostics are disabled." };
            }

            try
            {
                var sql = new clsSQL();
                DataTable dt = sql.ExecuteQueryStatement(
                    "SELECT 1 AS TestOk",
                    sql.MainDataBaseconString);

                if (dt != null && dt.Rows.Count > 0)
                {
                    return new { ok = true, message = "Main database connection is OK." };
                }

                return new { ok = false, message = "Main database returned no rows." };
            }
            catch (Exception ex)
            {
                return new { ok = false, message = "Main database failed.", detail = ex.Message };
            }
        }

        public static object CheckCompanyDatabase(int companyId, IConfiguration configuration)
        {
            if (!IsAvailable(configuration))
            {
                return new { ok = false, message = "Admin diagnostics are disabled." };
            }

            if (companyId <= 0)
            {
                return new { ok = false, message = "Select a company first (company id is 0)." };
            }

            try
            {
                var sql = new clsSQL();
                string companyConn = sql.CreateDataBaseConnectionString(companyId);

                if (string.IsNullOrWhiteSpace(companyConn))
                {
                    return new
                    {
                        ok = false,
                        message = "Company not found in main database or DataBaseName is missing.",
                        companyId,
                    };
                }

                DataTable dt = sql.ExecuteQueryStatement("SELECT 1 AS TestOk", companyConn);

                if (dt != null && dt.Rows.Count > 0)
                {
                    return new
                    {
                        ok = true,
                        message = "Company database connection is OK.",
                        companyId,
                    };
                }

                return new { ok = false, message = "Company database returned no rows.", companyId };
            }
            catch (Exception ex)
            {
                return new
                {
                    ok = false,
                    message = "Company database failed.",
                    companyId,
                    detail = ex.Message,
                };
            }
        }
    }
}
