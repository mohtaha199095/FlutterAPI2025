using Microsoft.Data.SqlClient;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;

namespace WebApplication2.cls
{
    public class clsUserMenuPreferences
    {
        public sealed class UserMenuPreferencesDto
        {
            public List<string> PinnedKeys { get; set; } = new List<string>();
            public List<string> ExpandedGroups { get; set; } = new List<string>();
        }

        public UserMenuPreferencesDto SelectUserMenuPreferences(
            int userId,
            int companyId,
            string moduleNamespace)
        {
            try
            {
                clsSQL clsSQL = new clsSQL();
                SqlParameter[] prm =
                {
                    new SqlParameter("@UserId", SqlDbType.Int) { Value = userId },
                    new SqlParameter("@CompanyID", SqlDbType.Int) { Value = companyId },
                    new SqlParameter("@ModuleNamespace", SqlDbType.NVarChar, 100)
                    {
                        Value = moduleNamespace ?? string.Empty
                    },
                };

                const string query = @"
SELECT PinnedKeys, ExpandedGroups
FROM tbl_UserMenuPreferences
WHERE UserId = @UserId
  AND CompanyID = @CompanyID
  AND ModuleNamespace = @ModuleNamespace";

                DataTable dt = clsSQL.ExecuteQueryStatement(
                    query,
                    clsSQL.CreateDataBaseConnectionString(companyId),
                    prm);

                if (dt == null || dt.Rows.Count == 0)
                {
                    return new UserMenuPreferencesDto();
                }

                DataRow row = dt.Rows[0];
                return new UserMenuPreferencesDto
                {
                    PinnedKeys = DeserializeStringList(row["PinnedKeys"]),
                    ExpandedGroups = DeserializeStringList(row["ExpandedGroups"]),
                };
            }
            catch (Exception)
            {
                throw;
            }
        }

        public bool SaveUserMenuPreferences(
            int userId,
            int companyId,
            string moduleNamespace,
            List<string> pinnedKeys,
            List<string> expandedGroups,
            int modificationUserId)
        {
            try
            {
                clsSQL clsSQL = new clsSQL();
                string pinnedJson = JsonConvert.SerializeObject(pinnedKeys ?? new List<string>());
                string expandedJson = JsonConvert.SerializeObject(expandedGroups ?? new List<string>());

                SqlParameter[] prm =
                {
                    new SqlParameter("@UserId", SqlDbType.Int) { Value = userId },
                    new SqlParameter("@CompanyID", SqlDbType.Int) { Value = companyId },
                    new SqlParameter("@ModuleNamespace", SqlDbType.NVarChar, 100)
                    {
                        Value = moduleNamespace ?? string.Empty
                    },
                    new SqlParameter("@PinnedKeys", SqlDbType.NVarChar, -1) { Value = pinnedJson },
                    new SqlParameter("@ExpandedGroups", SqlDbType.NVarChar, -1) { Value = expandedJson },
                    new SqlParameter("@ModificationUserID", SqlDbType.Int) { Value = modificationUserId },
                };

                const string query = @"
IF EXISTS (
    SELECT 1
    FROM tbl_UserMenuPreferences
    WHERE UserId = @UserId
      AND CompanyID = @CompanyID
      AND ModuleNamespace = @ModuleNamespace
)
BEGIN
    UPDATE tbl_UserMenuPreferences
    SET PinnedKeys = @PinnedKeys,
        ExpandedGroups = @ExpandedGroups,
        ModificationUserID = @ModificationUserID,
        ModificationDate = GETDATE()
    WHERE UserId = @UserId
      AND CompanyID = @CompanyID
      AND ModuleNamespace = @ModuleNamespace;
END
ELSE
BEGIN
    INSERT INTO tbl_UserMenuPreferences
        (UserId, CompanyID, ModuleNamespace, PinnedKeys, ExpandedGroups,
         CreationDate, ModificationUserID, ModificationDate)
    VALUES
        (@UserId, @CompanyID, @ModuleNamespace, @PinnedKeys, @ExpandedGroups,
         GETDATE(), @ModificationUserID, GETDATE());
END";

                clsSQL.ExecuteNonQueryStatement(
                    query,
                    clsSQL.CreateDataBaseConnectionString(companyId),
                    prm);

                return true;
            }
            catch (Exception)
            {
                throw;
            }
        }

        private static List<string> DeserializeStringList(object value)
        {
            if (value == null || value == DBNull.Value)
            {
                return new List<string>();
            }

            string raw = value.ToString();
            if (string.IsNullOrWhiteSpace(raw))
            {
                return new List<string>();
            }

            try
            {
                return JsonConvert.DeserializeObject<List<string>>(raw) ?? new List<string>();
            }
            catch
            {
                return new List<string>();
            }
        }
    }
}
