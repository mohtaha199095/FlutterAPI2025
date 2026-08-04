using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

using WebApplication2.cls;

namespace WebApplication2.Controllers
{
    /// <summary>
    /// Dynamic Navigation Controller - Handles next/prev navigation for all entities
    /// Copy-paste ready - Just add your entity configurations
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class NavigationController : Controller
    {
        #region API Endpoints

        /// <summary>
        /// Get next record ID
        /// Example: /api/Navigation/GetNext?entityKey=departments&currentId=1&companyId=1
        /// </summary>
        [HttpGet]
        [Route("GetNext")]
        public string GetNext(string entityKey, string currentId, int companyId, int? branchId = null)
        {
            try
            {
                var config = GetEntityConfig(entityKey);
                if (config == null)
                {
                    return JsonConvert.SerializeObject(new { found = false, message = "Invalid entity key" });
                }

                string sql = BuildNextQuery(config, branchId.HasValue);
                SqlParameter[] parameters = BuildParameters(config, currentId, companyId, branchId);

                clsSQL cls = new clsSQL();
                DataTable dt = cls.ExecuteQueryStatement(sql, cls.CreateDataBaseConnectionString(companyId), parameters);

                if (dt != null && dt.Rows.Count > 0)
                {
                    var nextId = dt.Rows[0][config.IdColumn].ToString();
                    return JsonConvert.SerializeObject(new { found = true, id = nextId });
                }
                else
                {
                    return JsonConvert.SerializeObject(new { found = false });
                }
            }
            catch (Exception ex)
            {
                return JsonConvert.SerializeObject(new { found = false, error = ex.Message });
            }
        }

        /// <summary>
        /// Get previous record ID
        /// Example: /api/Navigation/GetPrev?entityKey=departments&currentId=5&companyId=1
        /// </summary>
        [HttpGet]
        [Route("GetPrev")]
        public string GetPrev(string entityKey, string currentId, int companyId, int? branchId = null)
        {
            try
            {
                var config = GetEntityConfig(entityKey);
                if (config == null)
                {
                    return JsonConvert.SerializeObject(new { found = false, message = "Invalid entity key" });
                }

                string sql = BuildPrevQuery(config, branchId.HasValue);
                SqlParameter[] parameters = BuildParameters(config, currentId, companyId, branchId);

                clsSQL cls = new clsSQL();
                DataTable dt = cls.ExecuteQueryStatement(sql, cls.CreateDataBaseConnectionString(companyId), parameters);

                if (dt != null && dt.Rows.Count > 0)
                {
                    var prevId = dt.Rows[0][config.IdColumn].ToString();
                    return JsonConvert.SerializeObject(new { found = true, id = prevId });
                }
                else
                {
                    return JsonConvert.SerializeObject(new { found = false });
                }
            }
            catch (Exception ex)
            {
                return JsonConvert.SerializeObject(new { found = false, error = ex.Message });
            }
        }
        [HttpGet]
        [Route("Exists")]
        public string Exists(string entityKey, string id, int companyId, int? branchId = null)
        {
            try
            {
                var config = GetEntityConfig(entityKey);
                if (config == null)
                    return JsonConvert.SerializeObject(new { found = false, message = "Invalid entity key" });

                id = id?.Trim();
                if (string.IsNullOrEmpty(id))
                    return JsonConvert.SerializeObject(new { found = false, message = "Invalid id" });

                if (config.IdType == EntityIdType.Integer)
                {
                    if (!int.TryParse(id, out int intId))
                        return JsonConvert.SerializeObject(new { found = false, message = "Invalid id" });
                    return ExistsByPrimaryKey(config, companyId, branchId, intId.ToString(), EntityIdType.Integer);
                }

                if (Guid.TryParse(id, out Guid guidId))
                    return ExistsByPrimaryKey(config, companyId, branchId, guidId.ToString(), EntityIdType.Guid);

                if (config.GoToByDocumentNumber)
                    return ExistsByDocumentNumber(config, companyId, branchId, id);

                return JsonConvert.SerializeObject(new { found = false, message = "Invalid id" });
            }
            catch (Exception ex)
            {
                return JsonConvert.SerializeObject(new { found = false, error = ex.Message });
            }
        }

        private string ExistsByPrimaryKey(EntityConfig config, int companyId, int? branchId, string idValue, EntityIdType idType)
        {
            var queryFilterParameters = new List<SqlParameter>();
            string where = BuildWhereClause(config, branchId.HasValue, queryFilterParameters);
            var p = BuildScopeParameters(config, companyId, branchId, queryFilterParameters);

            if (idType == EntityIdType.Integer)
                p.Add(new SqlParameter("@Id", SqlDbType.Int) { Value = int.Parse(idValue) });
            else
                p.Add(new SqlParameter("@Id", SqlDbType.UniqueIdentifier) { Value = Guid.Parse(idValue) });

            string sql = $@"
                SELECT CASE WHEN EXISTS(
                    SELECT 1 FROM {config.TableName}
                    WHERE {where} AND {config.IdColumn} = @Id
                ) THEN 1 ELSE 0 END AS Found
            ";

            clsSQL cls = new clsSQL();
            DataTable dt = cls.ExecuteQueryStatement(sql, cls.CreateDataBaseConnectionString(companyId), p.ToArray());

            bool found = dt != null && dt.Rows.Count > 0 && Convert.ToInt32(dt.Rows[0]["Found"]) == 1;
            if (!found)
                return JsonConvert.SerializeObject(new { found = false });

            return JsonConvert.SerializeObject(new { found = true, id = idValue });
        }

        /// <summary>
        /// Resolve a document by its visible number (InvoiceNo, JVNumber, VoucherNo, etc.).
        /// </summary>
        private string ExistsByDocumentNumber(EntityConfig config, int companyId, int? branchId, string documentNumber)
        {
            var queryFilterParameters = new List<SqlParameter>();
            string where = BuildWhereClause(config, branchId.HasValue, queryFilterParameters);
            var p = BuildScopeParameters(config, companyId, branchId, queryFilterParameters);
            p.Add(new SqlParameter("@DocumentNumber", SqlDbType.NVarChar, 100) { Value = documentNumber });

            string sql = $@"
                SELECT TOP 1 {config.IdColumn} AS ResolvedId
                FROM {config.TableName}
                WHERE {where}
                AND CONVERT(NVARCHAR(100), {config.OrderColumn}) = @DocumentNumber
            ";

            clsSQL cls = new clsSQL();
            DataTable dt = cls.ExecuteQueryStatement(sql, cls.CreateDataBaseConnectionString(companyId), p.ToArray());

            if (dt == null || dt.Rows.Count == 0)
                return JsonConvert.SerializeObject(new { found = false });

            var resolvedId = dt.Rows[0]["ResolvedId"]?.ToString();
            if (string.IsNullOrWhiteSpace(resolvedId))
                return JsonConvert.SerializeObject(new { found = false });

            return JsonConvert.SerializeObject(new { found = true, id = resolvedId });
        }

        private List<SqlParameter> BuildScopeParameters(
            EntityConfig config,
            int companyId,
            int? branchId,
            List<SqlParameter> queryFilterParameters)
        {
            var p = new List<SqlParameter>();

            if (config.RequiresCompanyFilter)
                p.Add(new SqlParameter("@CompanyId", SqlDbType.Int) { Value = companyId });

            if (config.RequiresBranchFilter && branchId.HasValue)
                p.Add(new SqlParameter("@BranchId", SqlDbType.Int) { Value = branchId.Value });

            p.AddRange(queryFilterParameters);
            return p;
        }
        /// <summary>
        /// Get first record ID (lowest sort order).
        /// </summary>
        [HttpGet]
        [Route("GetFirst")]
        public string GetFirst(string entityKey, int companyId, int? branchId = null)
        {
            return GetBoundary(entityKey, companyId, branchId, first: true);
        }

        /// <summary>
        /// Get last record ID (highest sort order).
        /// </summary>
        [HttpGet]
        [Route("GetLast")]
        public string GetLast(string entityKey, int companyId, int? branchId = null)
        {
            return GetBoundary(entityKey, companyId, branchId, first: false);
        }

        private string GetBoundary(string entityKey, int companyId, int? branchId, bool first)
        {
            try
            {
                var config = GetEntityConfig(entityKey);
                if (config == null)
                    return JsonConvert.SerializeObject(new { found = false, message = "Invalid entity key" });

                string sql = BuildBoundaryQuery(config, branchId.HasValue, first);
                SqlParameter[] parameters = BuildFilterParameters(config, companyId, branchId);

                clsSQL cls = new clsSQL();
                DataTable dt = cls.ExecuteQueryStatement(sql, cls.CreateDataBaseConnectionString(companyId), parameters);

                if (dt != null && dt.Rows.Count > 0)
                {
                    var id = dt.Rows[0][config.IdColumn].ToString();
                    return JsonConvert.SerializeObject(new { found = true, id });
                }

                return JsonConvert.SerializeObject(new { found = false });
            }
            catch (Exception ex)
            {
                return JsonConvert.SerializeObject(new { found = false, error = ex.Message });
            }
        }

        /// <summary>
        /// Check navigation availability (has next/prev/first/last)
        /// Example: /api/Navigation/GetAvailability?entityKey=departments&currentId=3&companyId=1
        /// </summary>
        [HttpGet]
        [Route("GetAvailability")]
        public string GetAvailability(string entityKey, string currentId, int companyId, int? branchId = null)
        {
            try
            {
                var config = GetEntityConfig(entityKey);
                if (config == null)
                {
                    return JsonConvert.SerializeObject(new { hasNext = false, hasPrevious = false });
                }

                string sql = BuildAvailabilityQuery(config, branchId.HasValue);
                SqlParameter[] parameters = BuildParameters(config, currentId, companyId, branchId);

                clsSQL cls = new clsSQL();
                DataTable dt = cls.ExecuteQueryStatement(sql, cls.CreateDataBaseConnectionString(companyId), parameters);

                if (dt != null && dt.Rows.Count > 0)
                {
                    bool hasNext = Convert.ToBoolean(dt.Rows[0]["HasNext"]);
                    bool hasPrevious = Convert.ToBoolean(dt.Rows[0]["HasPrevious"]);
                    bool canGoFirst = dt.Columns.Contains("CanGoFirst") && Convert.ToBoolean(dt.Rows[0]["CanGoFirst"]);
                    bool canGoLast = dt.Columns.Contains("CanGoLast") && Convert.ToBoolean(dt.Rows[0]["CanGoLast"]);

                    return JsonConvert.SerializeObject(new { hasNext, hasPrevious, canGoFirst, canGoLast });
                }
                else
                {
                    return JsonConvert.SerializeObject(new { hasNext = false, hasPrevious = false, canGoFirst = false, canGoLast = false });
                }
            }
            catch (Exception ex)
            {
                return JsonConvert.SerializeObject(new { hasNext = false, hasPrevious = false, canGoFirst = false, canGoLast = false, error = ex.Message });
            }
        }

        #endregion

        #region Entity Configuration

        /// <summary>
        /// ⭐⭐⭐ ADD YOUR ENTITIES HERE ⭐⭐⭐
        /// Map entity keys to your database tables
        /// </summary>
        private static EntityConfig Int(string table, string order = "ID", bool companyFilter = true, string extraWhere = null)
        {
            return new EntityConfig
            {
                TableName = table,
                IdColumn = "ID",
                IdType = EntityIdType.Integer,
                OrderColumn = order,
                RequiresCompanyFilter = companyFilter,
                AdditionalWhere = extraWhere,
            };
        }

        private EntityConfig GetEntityConfig(string entityKey)
        {
            return entityKey?.ToLower() switch
            {
                // =============================================================================
                // INTEGER ID ENTITIES
                // =============================================================================

                "cash-drawers" => new EntityConfig
                {
                    TableName = "tbl_CashDrawer",
                    IdColumn = "ID",
                    IdType = EntityIdType.Integer,
                    OrderColumn = "ID",
                    RequiresCompanyFilter = true,
                },

                "departments" => new EntityConfig
                {
                    TableName = "tbl_Department",
                    IdColumn = "ID",
                    IdType = EntityIdType.Integer,
                    OrderColumn = "ID",
                    RequiresCompanyFilter = true,
                    RequiresBranchFilter = false,
                    
                },

                "customers" => new EntityConfig
                {
                    TableName = "tbl_Customers",
                    IdColumn = "CustomerID",
                    IdType = EntityIdType.Integer,
                    OrderColumn = "CustomerID",
                    RequiresCompanyFilter = true,
                    RequiresBranchFilter = false,
                     
                },

                "suppliers" => new EntityConfig
                {
                    TableName = "tbl_Suppliers",
                    IdColumn = "SupplierID",
                    IdType = EntityIdType.Integer,
                    OrderColumn = "SupplierID",
                    RequiresCompanyFilter = true,
                    RequiresBranchFilter = false,
                    
                },

                "employees" => new EntityConfig
                {
                    TableName = "tbl_employee",
                    IdColumn = "ID",
                    IdType = EntityIdType.Integer,
                    OrderColumn = "ID",
                    RequiresCompanyFilter = true,
                    RequiresBranchFilter = false,
                    
                },

                "branches" => new EntityConfig
                {
                    TableName = "tbl_Branch",
                    IdColumn = "BranchID",
                    IdType = EntityIdType.Integer,
                    OrderColumn = "BranchID",
                    RequiresCompanyFilter = true,
                    RequiresBranchFilter = false,
                  
                },

                "cost-centers" => new EntityConfig
                {
                    TableName = "tbl_CostCenter",
                    IdColumn = "CostCenterID",
                    IdType = EntityIdType.Integer,
                    OrderColumn = "CostCenterID",
                    RequiresCompanyFilter = true,
                    RequiresBranchFilter = false,
                    
                },

                "accounts" => new EntityConfig
                {
                    TableName = "tbl_Accounts",
                    IdColumn = "AccountID",
                    IdType = EntityIdType.Integer,
                    OrderColumn = "AccountID",
                    RequiresCompanyFilter = true,
                    RequiresBranchFilter = false,
                   
                },

                // =============================================================================
                // GUID ENTITIES
                // =============================================================================

                "journal-vouchers" => new EntityConfig
                {
                    TableName = "tbl_JournalVoucherHeader",
                    IdColumn = "Guid",
                    IdType = EntityIdType.Guid,
                    OrderColumn = "JVNumber",
                    RequiresCompanyFilter = true,
                    GoToByDocumentNumber = true,
                    QueryFilters = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
                    {
                        ["jvTypeId"] = "JVTypeID",
                    },
                },

                "invoices" => new EntityConfig
                {
                    TableName = "tbl_InvoiceHeader",
                    IdColumn = "Guid",
                    IdType = EntityIdType.Guid,
                    OrderColumn = "InvoiceNo",
                    RequiresCompanyFilter = true,
                    RequiresBranchFilter = true,
                    GoToByDocumentNumber = true,
                    QueryFilters = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
                    {
                        ["invoiceTypeId"] = "InvoiceTypeID",
                    },
                },

                "sales-invoices" => new EntityConfig
                {
                    TableName = "tbl_InvoiceHeader",
                    IdColumn = "Guid",
                    IdType = EntityIdType.Guid,
                    OrderColumn = "InvoiceNo",
                    RequiresCompanyFilter = true,
                    RequiresBranchFilter = true,
                    GoToByDocumentNumber = true,
                    QueryFilters = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
                    {
                        ["invoiceTypeId"] = "InvoiceTypeID",
                    },
                },

                "cash-vouchers" => new EntityConfig
                {
                    TableName = "tbl_CashVoucherHeader",
                    IdColumn = "Guid",
                    IdType = EntityIdType.Guid,
                    OrderColumn = "VoucherNo",
                    RequiresCompanyFilter = true,
                    RequiresBranchFilter = true,
                    GoToByDocumentNumber = true,
                    QueryFilters = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
                    {
                        ["voucherType"] = "VoucherType",
                    },
                },

                "credit-notes" => new EntityConfig
                {
                    TableName = "tbl_CreditNoteHeader",
                    IdColumn = "Guid",
                    IdType = EntityIdType.Guid,
                    OrderColumn = "VoucherNo",
                    RequiresCompanyFilter = true,
                    GoToByDocumentNumber = true,
                    QueryFilters = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
                    {
                        ["voucherType"] = "VoucherType",
                    },
                },

                "financing" => new EntityConfig
                {
                    TableName = "tbl_FinancingHeader",
                    IdColumn = "Guid",
                    IdType = EntityIdType.Guid,
                    OrderColumn = "VoucherNumber",
                    RequiresCompanyFilter = true,
                    RequiresBranchFilter = true,
                    GoToByDocumentNumber = true,
                    QueryFilters = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
                    {
                        ["loanType"] = "LoanType",
                    },
                    QueryFilterExpressions = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
                    {
                        ["mainTypeId"] = "LoanType IN (SELECT ID FROM tbl_LoanTypes WHERE MainTypeID = @NavFilter_mainTypeId AND (CompanyID = @CompanyId OR CompanyID = -1))",
                    },
                },

                "manufacturing-orders" => new EntityConfig
                {
                    TableName = "tbl_MOHeader",
                    IdColumn = "Guid",
                    IdType = EntityIdType.Guid,
                    OrderColumn = "MOCode",
                    RequiresCompanyFilter = true,
                    GoToByDocumentNumber = true,
                },

                "work-centers" => Int("tbl_WorkCenter", order: "WorkCenterCode"),

                "job-titles" => Int("tbl_JobTitle"),
                "cities" => Int("tbl_city"),
                "countries" => Int("tbl_Countries"),
                "stores" => Int("tbl_Store"),
                "banks" => Int("tbl_Banks"),
                "taxes" => Int("tbl_Tax"),
                "currencies" => Int("tbl_Currency", order: "Id"),
                "uom" => Int("tbl_UOM", order: "Id"),
                "items-categories" => Int("tbl_ItemsCategory"),
                "items-box-types" => Int("tbl_ItemsBoxType"),
                "payment-methods" => Int("tbl_PaymentMethod"),
                "pos-session-types" => Int("tbl_POSSessionsType"),
                "pos-scale-configurations" => Int("tbl_POSScaleConfiguration"),
                "loan-types" => Int("tbl_LoanTypes"),
                "hr-contract-types" => Int("tbl_HRContractType"),
                "salaries-elements" => Int("tbl_SalariesElements"),
                "reporting-types" => Int("tbl_ReportingType"),
                "business-partners" => Int("tbl_BusinessPartner"),
                "branch-floors" => Int("tbl_BranchFloors"),
                "bom" => Int("tbl_BOMHeader"),
                "payroll-periods" => Int("tbl_PayrollPeriod"),
                "employee-salary-elements" => Int("tbl_EmployeeSalaryElements"),
                "employee-shift-assignments" => Int("tbl_EmployeeShiftAssignment"),
                "crm-opportunities" => Int("tbl_CRMOpportunity", extraWhere: "IsActive = 1"),
                "einvoice-configurations" => Int("tbl_EInvoiceConfigurations", companyFilter: false),
                "companies" => Int("tbl_Company", companyFilter: false),
                "items" => new EntityConfig
                {
                    TableName = "tbl_Items",
                    IdColumn = "Guid",
                    IdType = EntityIdType.Guid,
                    OrderColumn = "ItemCode",
                    RequiresCompanyFilter = true,
                    GoToByDocumentNumber = true,
                },

                "users" => Int("tbl_employee"),

                _ => null // Invalid entity key
            };
        }

        #endregion

        #region SQL Query Builders

        /// <summary>
        /// Build SQL query for next record
        /// </summary>
        private string BuildNextQuery(EntityConfig config, bool includeBranchFilter)
        {
            string whereClause = BuildWhereClause(config, includeBranchFilter, new List<SqlParameter>());

            if (config.IdType == EntityIdType.Integer)
            {
                // For integer IDs, simple comparison
                return $@"
                    SELECT TOP 1 {config.IdColumn}
                    FROM {config.TableName}
                    WHERE {whereClause}
                    AND {config.OrderColumn} > @CurrentId
                    ORDER BY {config.OrderColumn} ASC
                ";
            }
            else // GUID
            {
                // For GUIDs, use subquery to get order value
                return $@"
                    SELECT TOP 1 {config.IdColumn}
                    FROM {config.TableName}
                    WHERE {whereClause}
                    AND {config.OrderColumn} > (
                        SELECT {config.OrderColumn} 
                        FROM {config.TableName} 
                        WHERE {config.IdColumn} = @CurrentId
                    )
                    ORDER BY {config.OrderColumn} ASC
                ";
            }
        }

        /// <summary>
        /// Build SQL query for previous record
        /// </summary>
        private string BuildPrevQuery(EntityConfig config, bool includeBranchFilter)
        {
            string whereClause = BuildWhereClause(config, includeBranchFilter, new List<SqlParameter>());

            if (config.IdType == EntityIdType.Integer)
            {
                return $@"
                    SELECT TOP 1 {config.IdColumn}
                    FROM {config.TableName}
                    WHERE {whereClause}
                    AND {config.OrderColumn} < @CurrentId
                    ORDER BY {config.OrderColumn} DESC
                ";
            }
            else // GUID
            {
                return $@"
                    SELECT TOP 1 {config.IdColumn}
                    FROM {config.TableName}
                    WHERE {whereClause}
                    AND {config.OrderColumn} < (
                        SELECT {config.OrderColumn} 
                        FROM {config.TableName} 
                        WHERE {config.IdColumn} = @CurrentId
                    )
                    ORDER BY {config.OrderColumn} DESC
                ";
            }
        }

        private string BuildBoundaryQuery(EntityConfig config, bool includeBranchFilter, bool first)
        {
            string whereClause = BuildWhereClause(config, includeBranchFilter, new List<SqlParameter>());
            string order = first ? "ASC" : "DESC";
            return $@"
                SELECT TOP 1 {config.IdColumn}
                FROM {config.TableName}
                WHERE {whereClause}
                ORDER BY {config.OrderColumn} {order}
            ";
        }

        /// <summary>
        /// Build SQL query to check availability
        /// </summary>
        private string BuildAvailabilityQuery(EntityConfig config, bool includeBranchFilter)
        {
            string whereClause = BuildWhereClause(config, includeBranchFilter, new List<SqlParameter>());

            if (config.IdType == EntityIdType.Integer)
            {
                return $@"
                    SELECT 
                        CASE WHEN EXISTS(
                            SELECT 1 FROM {config.TableName}
                            WHERE {whereClause}
                            AND {config.OrderColumn} < @CurrentId
                        ) THEN 1 ELSE 0 END AS HasPrevious,
                        CASE WHEN EXISTS(
                            SELECT 1 FROM {config.TableName}
                            WHERE {whereClause}
                            AND {config.OrderColumn} > @CurrentId
                        ) THEN 1 ELSE 0 END AS HasNext,
                        CASE WHEN @CurrentId > (
                            SELECT MIN({config.OrderColumn}) FROM {config.TableName} WHERE {whereClause}
                        ) THEN 1 ELSE 0 END AS CanGoFirst,
                        CASE WHEN @CurrentId < (
                            SELECT MAX({config.OrderColumn}) FROM {config.TableName} WHERE {whereClause}
                        ) THEN 1 ELSE 0 END AS CanGoLast
                ";
            }
            else // GUID
            {
                return $@"
                    SELECT 
                        CASE WHEN EXISTS(
                            SELECT 1 FROM {config.TableName}
                            WHERE {whereClause}
                            AND {config.OrderColumn} < (
                                SELECT {config.OrderColumn} FROM {config.TableName} WHERE {config.IdColumn} = @CurrentId
                            )
                        ) THEN 1 ELSE 0 END AS HasPrevious,
                        CASE WHEN EXISTS(
                            SELECT 1 FROM {config.TableName}
                            WHERE {whereClause}
                            AND {config.OrderColumn} > (
                                SELECT {config.OrderColumn} FROM {config.TableName} WHERE {config.IdColumn} = @CurrentId
                            )
                        ) THEN 1 ELSE 0 END AS HasNext,
                        CASE WHEN (
                            SELECT {config.OrderColumn} FROM {config.TableName} WHERE {config.IdColumn} = @CurrentId
                        ) > (
                            SELECT MIN({config.OrderColumn}) FROM {config.TableName} WHERE {whereClause}
                        ) THEN 1 ELSE 0 END AS CanGoFirst,
                        CASE WHEN (
                            SELECT {config.OrderColumn} FROM {config.TableName} WHERE {config.IdColumn} = @CurrentId
                        ) < (
                            SELECT MAX({config.OrderColumn}) FROM {config.TableName} WHERE {whereClause}
                        ) THEN 1 ELSE 0 END AS CanGoLast
                ";
            }
        }

        /// <summary>
        /// Build WHERE clause based on entity configuration and whitelisted query filters.
        /// </summary>
        private string BuildWhereClause(EntityConfig config, bool includeBranchFilter, List<SqlParameter> queryFilterParameters)
        {
            var clauses = new List<string>();

            if (config.RequiresCompanyFilter)
                clauses.Add("CompanyID = @CompanyId");

            if (config.RequiresBranchFilter && includeBranchFilter)
                clauses.Add("BranchID = @BranchId");

            if (!string.IsNullOrWhiteSpace(config.AdditionalWhere))
                clauses.Add(config.AdditionalWhere.Trim());

            AppendWhitelistedQueryFilters(config, clauses, queryFilterParameters);

            return clauses.Count > 0 ? string.Join(" AND ", clauses) : "1=1";
        }

        /// <summary>
        /// Applies only filters defined on the entity config (query key -> SQL column).
        /// </summary>
        private void AppendWhitelistedQueryFilters(EntityConfig config, List<string> clauses, List<SqlParameter> parameters)
        {
            if (config.QueryFilterExpressions != null)
            {
                foreach (var mapping in config.QueryFilterExpressions)
                {
                    if (!Request.Query.TryGetValue(mapping.Key, out var values))
                        continue;

                    var raw = values.FirstOrDefault();
                    if (string.IsNullOrWhiteSpace(raw) || !int.TryParse(raw, out int filterValue))
                        continue;

                    string paramName = "@NavFilter_" + SanitizeFilterParamName(mapping.Key);
                    clauses.Add(mapping.Value);
                    parameters.Add(new SqlParameter(paramName, SqlDbType.Int) { Value = filterValue });
                }
            }

            if (config.QueryFilters == null || config.QueryFilters.Count == 0)
                return;

            foreach (var mapping in config.QueryFilters)
            {
                if (!Request.Query.TryGetValue(mapping.Key, out var values))
                    continue;

                var raw = values.FirstOrDefault();
                if (string.IsNullOrWhiteSpace(raw) || !int.TryParse(raw, out int filterValue))
                    continue;

                string paramName = "@NavFilter_" + SanitizeFilterParamName(mapping.Key);
                clauses.Add($"{mapping.Value} = {paramName}");
                parameters.Add(new SqlParameter(paramName, SqlDbType.Int) { Value = filterValue });
            }
        }

        private static string SanitizeFilterParamName(string queryKey)
        {
            return queryKey.Replace(".", "", StringComparison.Ordinal)
                .Replace("-", "_", StringComparison.Ordinal);
        }

        /// <summary>
        /// Build SQL parameters
        /// </summary>
        private SqlParameter[] BuildParameters(EntityConfig config, string currentId, int companyId, int? branchId)
        {
            var parameters = new List<SqlParameter>();
            var queryFilterParameters = new List<SqlParameter>();

            // Current ID parameter
            if (config.IdType == EntityIdType.Integer)
            {
                parameters.Add(new SqlParameter("@CurrentId", SqlDbType.Int) { Value = int.Parse(currentId) });
            }
            else // GUID
            {
                parameters.Add(new SqlParameter("@CurrentId", SqlDbType.UniqueIdentifier) { Value = Guid.Parse(currentId) });
            }

            // Company ID
            if (config.RequiresCompanyFilter)
            {
                parameters.Add(new SqlParameter("@CompanyId", SqlDbType.Int) { Value = companyId });
            }

            // Branch ID
            if (config.RequiresBranchFilter && branchId.HasValue)
            {
                parameters.Add(new SqlParameter("@BranchId", SqlDbType.Int) { Value = branchId.Value });
            }

            BuildWhereClause(config, branchId.HasValue, queryFilterParameters);
            parameters.AddRange(queryFilterParameters);

            return parameters.ToArray();
        }

        private SqlParameter[] BuildFilterParameters(EntityConfig config, int companyId, int? branchId)
        {
            var parameters = new List<SqlParameter>();
            var queryFilterParameters = new List<SqlParameter>();

            if (config.RequiresCompanyFilter)
                parameters.Add(new SqlParameter("@CompanyId", SqlDbType.Int) { Value = companyId });

            if (config.RequiresBranchFilter && branchId.HasValue)
                parameters.Add(new SqlParameter("@BranchId", SqlDbType.Int) { Value = branchId.Value });

            BuildWhereClause(config, branchId.HasValue, queryFilterParameters);
            parameters.AddRange(queryFilterParameters);

            return parameters.ToArray();
        }

        #endregion
    }

    #region Entity Configuration Classes

    /// <summary>
    /// Entity configuration for navigation
    /// </summary>
    public class EntityConfig
    {
        public string TableName { get; set; }
        public string IdColumn { get; set; }
        public EntityIdType IdType { get; set; }
        public string OrderColumn { get; set; }
        public bool RequiresCompanyFilter { get; set; }
        public bool RequiresBranchFilter { get; set; }
        /// <summary>Optional extra AND clause (e.g. IsActive = 1).</summary>
        public string AdditionalWhere { get; set; }
        /// <summary>Whitelisted query-string keys mapped to SQL columns (screen-type filters).</summary>
        public Dictionary<string, string> QueryFilters { get; set; }
        /// <summary>Whitelisted query-string keys mapped to custom SQL expressions (use @NavFilter_{key} for the int value).</summary>
        public Dictionary<string, string> QueryFilterExpressions { get; set; }
        /// <summary>Go-to dialog accepts document number ([OrderColumn]) instead of primary key.</summary>
        public bool GoToByDocumentNumber { get; set; }
    }
    /// <summary>
    /// Entity ID type enum
    /// </summary>
    public enum EntityIdType
    {
        Integer,
        Guid
    }

    #endregion
}