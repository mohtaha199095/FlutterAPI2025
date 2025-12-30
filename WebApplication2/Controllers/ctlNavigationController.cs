using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;

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

                string where = BuildWhereClause(config, branchId.HasValue);

                string sql = $@"
            SELECT CASE WHEN EXISTS(
                SELECT 1 FROM {config.TableName}
                WHERE {where} AND {config.IdColumn} = @Id
            ) THEN 1 ELSE 0 END AS Found
        ";

                var p = new List<SqlParameter>();

                if (config.IdType == EntityIdType.Integer)
                    p.Add(new SqlParameter("@Id", SqlDbType.Int) { Value = int.Parse(id) });
                else
                    p.Add(new SqlParameter("@Id", SqlDbType.UniqueIdentifier) { Value = Guid.Parse(id) });

                if (config.RequiresCompanyFilter)
                    p.Add(new SqlParameter("@CompanyId", SqlDbType.Int) { Value = companyId });

                if (config.RequiresBranchFilter && branchId.HasValue)
                    p.Add(new SqlParameter("@BranchId", SqlDbType.Int) { Value = branchId.Value });

                clsSQL cls = new clsSQL();
                var conn = cls.CreateDataBaseConnectionString(companyId);
                DataTable dt = cls.ExecuteQueryStatement(sql, conn, p.ToArray());

                bool found = (dt != null && dt.Rows.Count > 0 && Convert.ToInt32(dt.Rows[0]["Found"]) == 1);
                return JsonConvert.SerializeObject(new { found });
            }
            catch (Exception ex)
            {
                return JsonConvert.SerializeObject(new { found = false, error = ex.Message });
            }
        }
        /// <summary>
        /// Check navigation availability (has next/prev)
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

                    return JsonConvert.SerializeObject(new { hasNext, hasPrevious });
                }
                else
                {
                    return JsonConvert.SerializeObject(new { hasNext = false, hasPrevious = false });
                }
            }
            catch (Exception ex)
            {
                return JsonConvert.SerializeObject(new { hasNext = false, hasPrevious = false, error = ex.Message });
            }
        }

        #endregion

        #region Entity Configuration

        /// <summary>
        /// ⭐⭐⭐ ADD YOUR ENTITIES HERE ⭐⭐⭐
        /// Map entity keys to your database tables
        /// </summary>
        private EntityConfig GetEntityConfig(string entityKey)
        {
            return entityKey?.ToLower() switch
            {
                // =============================================================================
                // INTEGER ID ENTITIES
                // =============================================================================

                "cash-drawers" => new EntityConfig
                {
                    TableName = "tbl_cashdrawer",
                    IdColumn = "ID",
                    IdType = EntityIdType.Integer,
                    OrderColumn = "ID",
                    RequiresCompanyFilter = true,
                    RequiresBranchFilter = false,
                    
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

                "items" => new EntityConfig
                {
                    TableName = "tbl_Items",
                    IdColumn = "ItemID",
                    IdType = EntityIdType.Integer,
                    OrderColumn = "ItemID",
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
                    TableName = "TblJournalVoucherHeader",
                    IdColumn = "Guid",
                    IdType = EntityIdType.Guid,
                    OrderColumn = "JVNumber", // ⚠️ Use document number, NOT Guid!
                    RequiresCompanyFilter = true,
                    RequiresBranchFilter = false,
                   
                },

                "sales-invoices" => new EntityConfig
                {
                    TableName = "TblSalesInvoiceHeader",
                    IdColumn = "Guid",
                    IdType = EntityIdType.Guid,
                    OrderColumn = "InvoiceNumber",
                    RequiresCompanyFilter = true,
                    RequiresBranchFilter = true,
                    
                },

                "purchase-invoices" => new EntityConfig
                {
                    TableName = "TblPurchaseInvoiceHeader",
                    IdColumn = "Guid",
                    IdType = EntityIdType.Guid,
                    OrderColumn = "InvoiceNumber",
                    RequiresCompanyFilter = true,
                    RequiresBranchFilter = true,
                  
                },

                "receipts" => new EntityConfig
                {
                    TableName = "TblReceiptHeader",
                    IdColumn = "Guid",
                    IdType = EntityIdType.Guid,
                    OrderColumn = "ReceiptNumber",
                    RequiresCompanyFilter = true,
                    RequiresBranchFilter = false,
                 
                },

                "payments" => new EntityConfig
                {
                    TableName = "TblPaymentHeader",
                    IdColumn = "Guid",
                    IdType = EntityIdType.Guid,
                    OrderColumn = "PaymentNumber",
                    RequiresCompanyFilter = true,
                    RequiresBranchFilter = false,
                    
                },

                // =============================================================================
                // ⭐ ADD MORE ENTITIES HERE ⭐
                // =============================================================================

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
            string whereClause = BuildWhereClause(config, includeBranchFilter);

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
            string whereClause = BuildWhereClause(config, includeBranchFilter);

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

        /// <summary>
        /// Build SQL query to check availability
        /// </summary>
        private string BuildAvailabilityQuery(EntityConfig config, bool includeBranchFilter)
        {
            string whereClause = BuildWhereClause(config, includeBranchFilter);

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
                        ) THEN 1 ELSE 0 END AS HasNext
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
                        ) THEN 1 ELSE 0 END AS HasNext
                ";
            }
        }

        /// <summary>
        /// Build WHERE clause based on entity configuration
        /// </summary>
        private string BuildWhereClause(EntityConfig config, bool includeBranchFilter)
        {
            var clauses = new List<string>();

            if (config.RequiresCompanyFilter)
                clauses.Add("CompanyID = @CompanyId");

            if (config.RequiresBranchFilter && includeBranchFilter)
                clauses.Add("BranchID = @BranchId");

         

            return string.Join(" AND ", clauses);
        }

        /// <summary>
        /// Build SQL parameters
        /// </summary>
        private SqlParameter[] BuildParameters(EntityConfig config, string currentId, int companyId, int? branchId)
        {
            var parameters = new List<SqlParameter>();

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