using Microsoft.Data.SqlClient;
using System;
using System.Data;
using System.Reflection;
using WebApplication2.MainClasses;

namespace WebApplication2.cls
{
    public static class clsTechnicalInfo
    {
        public static object GetTechnicalInfo(int companyId, int userId)
        {
            if (companyId <= 0)
            {
                return new { ok = false, message = "Company ID is required." };
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
                        message = "Company database not found or DataBaseName is missing.",
                        companyId,
                    };
                }

                string databaseName = "";
                string companyAName = "";
                string companyEName = "";
                string companyCreationDate = "";

                DataTable dtCompany = sql.ExecuteQueryStatement(
                    "SELECT TOP 1 ID, AName, EName, DataBaseName, CreationDate FROM tbl_Company WHERE ID = "
                        + Simulate.String(companyId),
                    sql.MainDataBaseconString);

                if (dtCompany != null && dtCompany.Rows.Count > 0)
                {
                    databaseName = Simulate.String(dtCompany.Rows[0]["DataBaseName"]);
                    companyAName = Simulate.String(dtCompany.Rows[0]["AName"]);
                    companyEName = Simulate.String(dtCompany.Rows[0]["EName"]);
                    if (dtCompany.Rows[0]["CreationDate"] != DBNull.Value)
                    {
                        companyCreationDate = Convert.ToDateTime(dtCompany.Rows[0]["CreationDate"])
                            .ToString("yyyy-MM-dd");
                    }
                }

                string currentUserName = "";
                string currentUserDisplayName = "";
                string currentUserEmail = "";

                if (userId > 0)
                {
                    SqlParameter[] prmUser =
                    {
                        new SqlParameter("@UserId", SqlDbType.Int) { Value = userId },
                    };

                    DataTable dtUser = sql.ExecuteQueryStatement(
                        "SELECT TOP 1 UserName, AName, EName, Email FROM tbl_Employee WHERE ID = @UserId",
                        companyConn,
                        prmUser);

                    if (dtUser != null && dtUser.Rows.Count > 0)
                    {
                        currentUserName = Simulate.String(dtUser.Rows[0]["UserName"]);
                        currentUserDisplayName = Simulate.String(dtUser.Rows[0]["EName"]);
                        if (string.IsNullOrWhiteSpace(currentUserDisplayName))
                        {
                            currentUserDisplayName = Simulate.String(dtUser.Rows[0]["AName"]);
                        }

                        currentUserEmail = Simulate.String(dtUser.Rows[0]["Email"]);
                    }
                }

                decimal dbVersion = 0;
                var dbVersionCls = new clsDataBaseVersion();
                DataTable dtVersion = dbVersionCls.SelectDataBaseVersion(0, companyId);
                if (dtVersion != null && dtVersion.Rows.Count > 0)
                {
                    dbVersion = Simulate.decimal_(dtVersion.Rows[0]["VersionNumber"]);
                }

                decimal databaseSizeMb = 0;
                try
                {
                    DataTable dtSize = sql.ExecuteQueryStatement(
                        @"SELECT CAST(SUM(CAST(size AS BIGINT)) * 8.0 / 1024.0 AS DECIMAL(18,2)) AS SizeMB
                          FROM sys.database_files",
                        companyConn);

                    if (dtSize != null && dtSize.Rows.Count > 0)
                    {
                        databaseSizeMb = Simulate.decimal_(dtSize.Rows[0]["SizeMB"]);
                    }
                }
                catch
                {
                    // Size query is best-effort only.
                }

                int usersCount = 0;
                int systemUsersCount = 0;
                int branchesCount = 0;
                int itemsCount = 0;
                int customersCount = 0;
                int vendorsCount = 0;

                try
                {
                    DataTable dtCounts = sql.ExecuteQueryStatement(
                        @"SELECT
                            (SELECT COUNT(*) FROM tbl_Employee) AS UsersCount,
                            (SELECT COUNT(*) FROM tbl_Employee WHERE ISNULL(IsSystemUser, 0) = 1) AS SystemUsersCount,
                            (SELECT COUNT(*) FROM tbl_Branch) AS BranchesCount,
                            (SELECT COUNT(*) FROM tbl_Items WHERE ISNULL(IsActive, 1) = 1) AS ItemsCount,
                            (SELECT COUNT(*) FROM tbl_BusinessPartner WHERE Active = 1 AND [Type] = 1) AS CustomersCount,
                            (SELECT COUNT(*) FROM tbl_BusinessPartner WHERE Active = 1 AND [Type] = 2) AS VendorsCount",
                        companyConn);

                    if (dtCounts != null && dtCounts.Rows.Count > 0)
                    {
                        usersCount = Simulate.Integer32(dtCounts.Rows[0]["UsersCount"]);
                        systemUsersCount = Simulate.Integer32(dtCounts.Rows[0]["SystemUsersCount"]);
                        branchesCount = Simulate.Integer32(dtCounts.Rows[0]["BranchesCount"]);
                        itemsCount = Simulate.Integer32(dtCounts.Rows[0]["ItemsCount"]);
                        customersCount = Simulate.Integer32(dtCounts.Rows[0]["CustomersCount"]);
                        vendorsCount = Simulate.Integer32(dtCounts.Rows[0]["VendorsCount"]);
                    }
                }
                catch
                {
                    // Counts are best-effort only.
                }

                string mainDatabaseName = "";
                try
                {
                    var builder = new SqlConnectionStringBuilder(sql.MainDataBaseconString);
                    mainDatabaseName = builder.InitialCatalog ?? "";
                }
                catch
                {
                    // Ignore connection-string parse errors.
                }

                string sqlServerVersion = "";
                try
                {
                    DataTable dtServer = sql.ExecuteQueryStatement(
                        "SELECT CAST(SERVERPROPERTY('ProductVersion') AS NVARCHAR(128)) AS ProductVersion",
                        companyConn);

                    if (dtServer != null && dtServer.Rows.Count > 0)
                    {
                        sqlServerVersion = Simulate.String(dtServer.Rows[0]["ProductVersion"]);
                    }
                }
                catch
                {
                    // Ignore server version lookup errors.
                }

                string apiVersion = Assembly.GetExecutingAssembly().GetName().Version?.ToString() ?? "";

                return new
                {
                    ok = true,
                    companyId,
                    companyAName,
                    companyEName,
                    databaseName,
                    mainDatabaseName,
                    databaseVersion = dbVersion,
                    databaseSizeMb,
                    usersCount,
                    systemUsersCount,
                    branchesCount,
                    itemsCount,
                    customersCount,
                    vendorsCount,
                    currentUserId = userId,
                    currentUserName,
                    currentUserDisplayName,
                    currentUserEmail,
                    companyCreationDate,
                    sqlServerVersion,
                    serverTimeUtc = DateTime.UtcNow.ToString("o"),
                    apiVersion,
                };
            }
            catch (Exception ex)
            {
                return new
                {
                    ok = false,
                    message = "Failed to load technical info.",
                    detail = ex.Message,
                    companyId,
                };
            }
        }
    }
}
