using Microsoft.Data.SqlClient;
using Newtonsoft.Json;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;

namespace WebApplication2.cls
{
    public class clsRecordAudit
    {
        private static readonly ConcurrentDictionary<string, AuditColumnNames> _columnCache =
            new ConcurrentDictionary<string, AuditColumnNames>(StringComparer.OrdinalIgnoreCase);

        private static readonly Dictionary<string, RecordAuditEntityConfig> _entities = BuildEntityRegistry();

        private static Dictionary<string, RecordAuditEntityConfig> BuildEntityRegistry()
        {
            var registry = new Dictionary<string, RecordAuditEntityConfig>(StringComparer.OrdinalIgnoreCase);

            void AddInt(string table, string idColumn = "ID", bool requiresCompanyFilter = true)
            {
                registry[table] = new RecordAuditEntityConfig
                {
                    TableName = table,
                    IdColumn = idColumn,
                    IdType = RecordAuditIdType.Integer,
                    RequiresCompanyFilter = requiresCompanyFilter,
                };
            }

            void AddGuid(string table, string idColumn = "Guid", bool requiresCompanyFilter = true)
            {
                registry[table] = new RecordAuditEntityConfig
                {
                    TableName = table,
                    IdColumn = idColumn,
                    IdType = RecordAuditIdType.Guid,
                    RequiresCompanyFilter = requiresCompanyFilter,
                };
            }

            AddInt("tbl_Department");
            AddInt("tbl_JobTitle");
            AddInt("tbl_Branch");
            AddInt("tbl_BranchFloors");
            AddInt("tbl_BusinessPartner");
            AddInt("tbl_Banks");
            AddInt("tbl_Tax");
            AddInt("tbl_Store");
            AddInt("tbl_LoanTypes");
            AddInt("tbl_CostCenter");
            AddInt("tbl_Accounts");
            AddInt("tbl_CashDrawer");
            AddInt("tbl_Currency");
            AddInt("tbl_Countries");
            AddInt("tbl_City");
            AddInt("tbl_UOM");
            AddInt("tbl_ItemsCategory");
            AddInt("tbl_ItemsBoxType");
            AddInt("tbl_PaymentMethod");
            AddInt("tbl_POSSessionsType");
            AddInt("tbl_POSScaleConfiguration");
            AddInt("tbl_ReportingTypeNodes");
            AddInt("tbl_SalariesElements");
            AddInt("tbl_HRContractType");
            AddInt("tbl_BOMHeader");
            AddInt("tbl_EInvoiceConfigurations");
            AddInt("tbl_PayrollPeriod");
            AddInt("tbl_EmployeeSalaryElements");
            AddInt("tbl_EmployeeShiftAssignment");
            AddInt("tbl_AttendanceRules");
            AddInt("tbl_Shifts");
            AddInt("tbl_Subscriptions");
            AddInt("tbl_CRMOpportunity");
            AddInt("tbl_employee", requiresCompanyFilter: false);
            AddInt("tbl_Company", requiresCompanyFilter: false);

            AddGuid("tbl_JournalVoucherHeader");
            AddGuid("tbl_InvoiceHeader");
            AddGuid("tbl_invoiceHeader");
            AddGuid("tbl_CashVoucherHeader");
            AddGuid("tbl_CreditNoteHeader");
            AddGuid("tbl_FinancingHeader");
            AddGuid("tbl_MOHeader");
            AddInt("tbl_WorkCenter");
            AddGuid("tbl_Items");

            return registry;
        }

        public string SelectRecordMetadata(string tableName, string recordKey, int companyId)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(tableName))
                    return NotFound("Table name is required.");

                if (string.IsNullOrWhiteSpace(recordKey) || recordKey == "0")
                    return NotFound("Record key is required.");

                if (!_entities.TryGetValue(tableName.Trim(), out var config))
                    return NotFound("Table is not registered for record audit.");

                if (!TryParseRecordKey(config, recordKey, out var parsedKey, out var parseError))
                    return NotFound(parseError);

                var conn = new clsSQL().CreateDataBaseConnectionString(companyId);
                var columns = ResolveAuditColumns(conn, config.TableName);
                if (columns == null)
                    return NotFound("Audit columns were not found on this table.");

                string whereClause = $"h.[{config.IdColumn}] = @RecordKey";
                if (config.RequiresCompanyFilter)
                    whereClause += " AND h.CompanyID = @CompanyId";

                string sql = BuildAuditQuery(config, columns, whereClause);

                var parameters = new List<SqlParameter>
                {
                    BuildRecordKeyParameter(config, parsedKey),
                };

                if (config.RequiresCompanyFilter)
                    parameters.Add(new SqlParameter("@CompanyId", SqlDbType.Int) { Value = companyId });

                clsSQL cls = new clsSQL();
                DataTable dt = cls.ExecuteQueryStatement(sql, conn, parameters.ToArray());

                if (dt == null || dt.Rows.Count == 0)
                    return NotFound("Record not found.");

                DataRow row = dt.Rows[0];
                var result = new
                {
                    found = true,
                    creationUserAName = Simulate.String(row["CreationUserAName"]),
                    creationUserEName = Simulate.String(row["CreationUserEName"]),
                    creationDate = FormatDate(row["CreationDate"]),
                    modificationUserAName = Simulate.String(row["ModificationUserAName"]),
                    modificationUserEName = Simulate.String(row["ModificationUserEName"]),
                    modificationDate = FormatDate(row["ModificationDate"]),
                };

                return JsonConvert.SerializeObject(result);
            }
            catch (Exception ex)
            {
                return JsonConvert.SerializeObject(new { found = false, message = ex.Message });
            }
        }

        private static string BuildAuditQuery(
            RecordAuditEntityConfig config,
            AuditColumnNames columns,
            string whereClause)
        {
            var selectParts = new List<string>();
            var joins = new List<string>();

            if (!string.IsNullOrEmpty(columns.CreationUserColumn))
            {
                selectParts.Add("ce.AName AS CreationUserAName");
                selectParts.Add("ce.EName AS CreationUserEName");
                joins.Add($"LEFT JOIN tbl_employee ce ON ce.ID = h.[{columns.CreationUserColumn}]");
            }
            else
            {
                selectParts.Add("CAST(NULL AS nvarchar(max)) AS CreationUserAName");
                selectParts.Add("CAST(NULL AS nvarchar(max)) AS CreationUserEName");
            }

            if (!string.IsNullOrEmpty(columns.ModificationUserColumn))
            {
                selectParts.Add("me.AName AS ModificationUserAName");
                selectParts.Add("me.EName AS ModificationUserEName");
                joins.Add($"LEFT JOIN tbl_employee me ON me.ID = h.[{columns.ModificationUserColumn}]");
            }
            else
            {
                selectParts.Add("CAST(NULL AS nvarchar(max)) AS ModificationUserAName");
                selectParts.Add("CAST(NULL AS nvarchar(max)) AS ModificationUserEName");
            }

            selectParts.Add($"h.[{columns.CreationDateColumn}] AS CreationDate");

            if (!string.IsNullOrEmpty(columns.ModificationDateColumn))
                selectParts.Add($"h.[{columns.ModificationDateColumn}] AS ModificationDate");
            else
                selectParts.Add("CAST(NULL AS datetime) AS ModificationDate");

            return $@"
SELECT TOP 1
    {string.Join(",\n    ", selectParts)}
FROM [{config.TableName}] h
{string.Join("\n", joins)}
WHERE {whereClause}";
        }

        private static bool TryParseRecordKey(
            RecordAuditEntityConfig config,
            string recordKey,
            out object parsedKey,
            out string error)
        {
            parsedKey = null;
            error = "";

            if (config.IdType == RecordAuditIdType.Integer)
            {
                if (!int.TryParse(recordKey, out int id) || id <= 0)
                {
                    error = "Invalid record key.";
                    return false;
                }

                parsedKey = id;
                return true;
            }

            if (!Guid.TryParse(recordKey, out Guid guid) || guid == Guid.Empty)
            {
                error = "Invalid record key.";
                return false;
            }

            parsedKey = guid;
            return true;
        }

        private static SqlParameter BuildRecordKeyParameter(RecordAuditEntityConfig config, object parsedKey)
        {
            if (config.IdType == RecordAuditIdType.Integer)
            {
                return new SqlParameter("@RecordKey", SqlDbType.Int) { Value = parsedKey };
            }

            return new SqlParameter("@RecordKey", SqlDbType.UniqueIdentifier) { Value = parsedKey };
        }

        private static AuditColumnNames ResolveAuditColumns(string connectionString, string tableName)
        {
            string cacheKey = $"{connectionString}|{tableName}";
            if (_columnCache.TryGetValue(cacheKey, out var cached))
                return cached;

            clsSQL cls = new clsSQL();
            DataTable dt = cls.ExecuteQueryStatement(@"
SELECT COLUMN_NAME
FROM INFORMATION_SCHEMA.COLUMNS
WHERE TABLE_NAME = @TableName",
                connectionString,
                new[] { new SqlParameter("@TableName", SqlDbType.NVarChar, 128) { Value = tableName } });

            if (dt == null || dt.Rows.Count == 0)
                return null;

            var columnNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach (DataRow row in dt.Rows)
                columnNames.Add(Simulate.String(row["COLUMN_NAME"]));

            string creationUser = ResolveColumn(columnNames, "CreationUserID", "CreationUserId");
            string modificationUser = ResolveColumn(columnNames, "ModificationUserID", "ModificationUserId");
            string creationDate = ResolveColumn(columnNames, "CreationDate");
            string modificationDate = ResolveColumn(columnNames, "ModificationDate");

            if (string.IsNullOrEmpty(creationDate) &&
                string.IsNullOrEmpty(creationUser) &&
                string.IsNullOrEmpty(modificationDate) &&
                string.IsNullOrEmpty(modificationUser))
            {
                return null;
            }

            if (string.IsNullOrEmpty(creationDate))
                return null;

            var resolved = new AuditColumnNames
            {
                CreationUserColumn = creationUser,
                ModificationUserColumn = modificationUser,
                CreationDateColumn = creationDate,
                ModificationDateColumn = modificationDate,
            };

            _columnCache[cacheKey] = resolved;
            return resolved;
        }

        private static string ResolveColumn(HashSet<string> columns, params string[] candidates)
        {
            foreach (var candidate in candidates)
            {
                if (columns.Contains(candidate))
                    return candidate;
            }

            return null;
        }

        private static string FormatDate(object value)
        {
            if (value == null || value == DBNull.Value)
                return null;

            if (value is DateTime dt)
                return dt.ToString("yyyy-MM-ddTHH:mm:ss");

            return Simulate.String(value);
        }

        private static string NotFound(string message)
        {
            return JsonConvert.SerializeObject(new { found = false, message });
        }
    }

    public class RecordAuditEntityConfig
    {
        public string TableName { get; set; }
        public string IdColumn { get; set; }
        public RecordAuditIdType IdType { get; set; }
        public bool RequiresCompanyFilter { get; set; } = true;
    }

    public enum RecordAuditIdType
    {
        Integer,
        Guid,
    }

    internal class AuditColumnNames
    {
        public string CreationUserColumn { get; set; }
        public string ModificationUserColumn { get; set; }
        public string CreationDateColumn { get; set; }
        public string ModificationDateColumn { get; set; }
    }
}
