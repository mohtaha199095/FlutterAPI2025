////using Newtonsoft.Json;
////using System;
////using System.Collections.Generic;
////using System.Data;
////namespace WebApplication2.cls
////{
////    public class clsReportBuilder
////    {

////        // ==========================================================
////        // 1) CATALOG BUILDER
////        // ==========================================================
////        public List<object> BuildCatalog(int CompanyID)
////        {
////            // You can build from DB tables:
////            // TblReportModules, TblReportFields, TblReportJoins
////            // OR build hardcoded list for now

////            List<object> modules = new List<object>();

////            modules.Add(new
////            {
////                Id = "sales_invoices",
////                Name = "Sales Invoices",
////                PrimaryTable = "tbl_InvoiceHeader",
////                Icon = "receipt",
////                Color = "#2563EB",
////                Fields = new object[]
////                {
////                    new { Id="inv_no", Label="Invoice No", Table="tbl_InvoiceHeader", Column="InvoiceNumber", Type="text", Icon="hash" },
////                    new { Id="inv_date", Label="Invoice Date", Table="tbl_InvoiceHeader", Column="InvoiceDate", Type="date", Icon="calendar" },
////                    new { Id="cust_name", Label="Customer", Table="tbl_Customer", Column="AName", Type="text", Icon="user" },
////                    new { Id="net_total", Label="Net Total", Table="tbl_InvoiceHeader", Column="NetTotal", Type="currency", Icon="dollar" },
////                },
////                Joins = new object[]
////                {
////                    new { FromTable="tbl_InvoiceHeader", FromColumn="CustomerID", ToTable="tbl_Customer", ToColumn="ID" }
////                }
////            });
////            modules.Add(new
////            {
////                Id = "finance_journal_vouchers",
////                Name = "Journal Vouchers",
////                PrimaryTable = "tbl_JournalVoucherHeader",
////                Icon = "file-text",
////                Color = "#2563EB",

////                Fields = new object[]
////    {
////        // ===== Header fields =====
////        new { Id="hdr_guid",      Label="Header Guid",   Table="tbl_JournalVoucherHeader", Column="Guid",        Type="guid",     Icon="fingerprint" },
////        new { Id="jv_no",         Label="JV Number",     Table="tbl_JournalVoucherHeader", Column="JVNumber",    Type="text",     Icon="hash" },
////        new { Id="voucher_date",  Label="Voucher Date",  Table="tbl_JournalVoucherHeader", Column="VoucherDate", Type="date",     Icon="calendar" },

////        new { Id="branch_name",   Label="Branch",        Table="tbl_Branch",               Column="AName",       Type="text",     Icon="building" },
////        new { Id="cc_name",       Label="Cost Center",   Table="tbl_CostCenter",           Column="AName",       Type="text",     Icon="target" },

////        new { Id="notes",         Label="Notes",         Table="tbl_JournalVoucherHeader", Column="Notes",       Type="text",     Icon="message-square" },
////        new { Id="created_at",    Label="Created At",    Table="tbl_JournalVoucherHeader", Column="CreationDate",Type="datetime", Icon="clock" },

////        // ===== Details fields =====
////        new { Id="dtl_guid",      Label="Detail Guid",   Table="tbl_JournalVoucherDetails", Column="Guid",       Type="guid",     Icon="fingerprint" },
////        new { Id="dtl_parent",    Label="Parent Guid",   Table="tbl_JournalVoucherDetails", Column="ParentGuid", Type="guid",     Icon="link" },
////        new { Id="row_index",     Label="Row",           Table="tbl_JournalVoucherDetails", Column="RowIndex",   Type="number",   Icon="hash" },

////        new { Id="debit",         Label="Debit",         Table="tbl_JournalVoucherDetails", Column="Debit",      Type="currency", Icon="plus-circle" },
////        new { Id="credit",        Label="Credit",        Table="tbl_JournalVoucherDetails", Column="Credit",     Type="currency", Icon="minus-circle" },
////        new { Id="total",         Label="Total",         Table="tbl_JournalVoucherDetails", Column="Total",      Type="currency", Icon="calculator" },

////        new { Id="due_date",      Label="Due Date",      Table="tbl_JournalVoucherDetails", Column="DueDate",    Type="date",     Icon="calendar" },
////        new { Id="detail_note",   Label="Detail Note",   Table="tbl_JournalVoucherDetails", Column="Note",       Type="text",     Icon="message-square" },
////    },

////                Joins = new object[]
////    {
////        // Header -> Branch / Cost Center
////        new { FromTable="tbl_JournalVoucherHeader", FromColumn="BranchID",     ToTable="tbl_Branch",     ToColumn="ID" },
////        new { FromTable="tbl_JournalVoucherHeader", FromColumn="CostCenterID", ToTable="tbl_CostCenter", ToColumn="ID" },

////        // Header -> Details (Guid -> ParentGuid)
////        new { FromTable="tbl_JournalVoucherHeader", FromColumn="Guid",         ToTable="tbl_JournalVoucherDetails", ToColumn="ParentGuid" }
////    }
////            });
////            return modules;
////        }

////        // ==========================================================
////        // 2) RUN REPORT (returns DataTable + totalRows)
////        // ==========================================================
////        public void RunReport(
////            int CompanyID,
////            string ModuleId,
////            List<string> FieldIds,
////            List<WebApplication2.Controllers.ctlReportBuilder.RunReportFilter> Filters,
////            string SortByFieldId,
////            string SortDir,
////            string GroupByFieldId,
////            int Page,
////            int PageSize,
////            ref DataTable dtRows,
////            ref int totalRows
////        )
////        {
////            // TODO:
////            // - load module catalog
////            // - map fieldId => (table,column,join)
////            // - build SELECT + JOIN + WHERE + GROUP + ORDER + paging
////            // - execute SQL (clsSQL)
////            // - fill dtRows + totalRows

////            dtRows = new DataTable();
////            totalRows = 0;

////            // placeholder columns so Flutter can render (remove later)
////            dtRows.Columns.Add("inv_no");
////            dtRows.Columns.Add("inv_date");
////            dtRows.Columns.Add("cust_name");
////            dtRows.Columns.Add("net_total");

////            // sample row
////            DataRow r = dtRows.NewRow();
////            r["inv_no"] = "INV-0001";
////            r["inv_date"] = DateTime.Now.ToString("yyyy-MM-dd");
////            r["cust_name"] = "Demo Customer";
////            r["net_total"] = "100.00";
////            dtRows.Rows.Add(r);

////            totalRows = 1;
////        }
////    }
////}
////using DocumentFormat.OpenXml.Office.CustomUI;
//using Microsoft.Data.SqlClient;
//using System;
//using System.Collections.Generic;
//using System.Data;

//using System.Linq;
//using System.Text;
//using System.Text.Json;

//namespace WebApplication2.Controllers
//{
//    public class ReportBuilderService
//    {
//        public List<object> BuildCatalog1(int CompanyID)
//        {
//            // You can build from DB tables:
//            // TblReportModules, TblReportFields, TblReportJoins
//            // OR build hardcoded list for now

//            List<object> modules = new List<object>();


//            modules.Add(new
//            {
//                Id = "finance_journal_vouchers",
//                Name = "Journal Vouchers",
//                PrimaryTable = "tbl_JournalVoucherHeader",
//                Icon = "file-text",
//                Color = "#2563EB",

//                Fields = new object[]
//    {
//                // ===== Header fields =====
//            //    new { Id="hdr_guid",      Label="Header Guid",   Table="tbl_JournalVoucherHeader", Column="Guid",        Type="guid",     Icon="fingerprint" },
//                new { Id="jv_no",         Label="JV Number",     Table="tbl_JournalVoucherHeader", Column="JVNumber",    Type="text",     Icon="hash" },
//                new { Id="voucher_date",  Label="Voucher Date",  Table="tbl_JournalVoucherHeader", Column="VoucherDate", Type="date",     Icon="calendar" },
//                new { Id="Account_name",       Label="Account",   Table="tbl_Accounts",           Column="AName",       Type="text",     Icon="target" },

//                new { Id="branch_name",   Label="Branch",        Table="tbl_Branch",               Column="AName",       Type="text",     Icon="building" },
//                new { Id="cc_name",       Label="Cost Center",   Table="tbl_CostCenter",           Column="AName",       Type="text",     Icon="target" },

//                new { Id="notes",         Label="Notes",         Table="tbl_JournalVoucherHeader", Column="Notes",       Type="text",     Icon="message-square" },
//                new { Id="created_at",    Label="Created At",    Table="tbl_JournalVoucherHeader", Column="CreationDate",Type="datetime", Icon="clock" },
//                new { Id="modified_at",    Label="Modified At",    Table="tbl_JournalVoucherHeader", Column="ModificationDate",Type="datetime", Icon="clock" },

//                // ===== Details fields =====
//           //     new { Id="dtl_guid",      Label="Detail Guid",   Table="tbl_JournalVoucherDetails", Column="Guid",       Type="guid",     Icon="fingerprint" },
//         //       new { Id="dtl_parent",    Label="Parent Guid",   Table="tbl_JournalVoucherDetails", Column="ParentGuid", Type="guid",     Icon="link" },
//                new { Id="row_index",     Label="Row",           Table="tbl_JournalVoucherDetails", Column="RowIndex",   Type="number",   Icon="hash" },

//                new { Id="debit",         Label="Debit",         Table="tbl_JournalVoucherDetails", Column="Debit",      Type="currency", Icon="plus-circle" },
//                new { Id="credit",        Label="Credit",        Table="tbl_JournalVoucherDetails", Column="Credit",     Type="currency", Icon="minus-circle" },
//                new { Id="total",         Label="Total",         Table="tbl_JournalVoucherDetails", Column="Total",      Type="currency", Icon="calculator" },
//                new { Id="dtl_branch_name",   Label="Branch",        Table="tbl_Branch",               Column="AName",       Type="text",     Icon="building" },
//                new { Id="dtl_cc_name",       Label="Cost Center",   Table="tbl_CostCenter",           Column="AName",       Type="text",     Icon="target" },

//                new { Id="due_date",      Label="Due Date",      Table="tbl_JournalVoucherDetails", Column="DueDate",    Type="date",     Icon="calendar" },
//                new { Id="detail_note",   Label="Detail Note",   Table="tbl_JournalVoucherDetails", Column="Note",       Type="text",     Icon="message-square" },
//    },

//                Joins = new object[]
//    {
//                // Header -> Branch / Cost Center
//                new { FromTable="tbl_JournalVoucherHeader", FromColumn="BranchID",     ToTable="tbl_Branch",     ToColumn="ID" },
//                new { FromTable="tbl_JournalVoucherHeader", FromColumn="CostCenterID", ToTable="tbl_CostCenter", ToColumn="ID" },
//                new { FromTable="tbl_JournalVoucherDetails", FromColumn="BranchID",     ToTable="tbl_Branch",     ToColumn="ID" },
//                new { FromTable="tbl_JournalVoucherDetails", FromColumn="CostCenterID", ToTable="tbl_CostCenter", ToColumn="ID" },
//                new { FromTable="tbl_JournalVoucherDetails", FromColumn="AccountID", ToTable="tbl_Accounts", ToColumn="ID" },

//                // Header -> Details (Guid -> ParentGuid)
//                new { FromTable="tbl_JournalVoucherHeader", FromColumn="Guid",         ToTable="tbl_JournalVoucherDetails", ToColumn="ParentGuid" }
//    }
//            });
//            return modules;
//        }
//        public Dictionary<string, ModuleDef> BuildCatalog()
//        {
//            // Primary: tbl_JournalVoucherHeader h
//            // Joins:
//            //   h.BranchID -> tbl_Branch b
//            //   h.CostCenterID -> tbl_CostCenter cc
//            //   h.Guid -> tbl_JournalVoucherDetails d.ParentGuid
//            var finance = new ModuleDef
//            {
//                Id = "finance_journal_vouchers",
//                PrimaryTable = "tbl_JournalVoucherHeader",
//                PrimaryAlias = "h",
//                Fields = new List<FieldDef>
//                {
//                    // Header
//                    new FieldDef { Id="hdr_guid",     Table="tbl_JournalVoucherHeader", Alias="h", Column="Guid",        Type="guid" },
//                    new FieldDef { Id="jv_no",        Table="tbl_JournalVoucherHeader", Alias="h", Column="JVNumber",    Type="text" },
//                    new FieldDef { Id="voucher_date", Table="tbl_JournalVoucherHeader", Alias="h", Column="VoucherDate", Type="date" },
//                    new FieldDef { Id="notes",        Table="tbl_JournalVoucherHeader", Alias="h", Column="Notes",       Type="text" },
//                    new FieldDef { Id="created_at",   Table="tbl_JournalVoucherHeader", Alias="h", Column="CreationDate",Type="datetime" },

//                    // Branch + CostCenter (from header joins)
//                    new FieldDef { Id="branch_name",  Table="tbl_Branch",    Alias="b",  Column="AName", Type="text" },
//                    new FieldDef { Id="cc_name",      Table="tbl_CostCenter",Alias="cc", Column="AName", Type="text" },
//                        // Accounts  (from header joins)
//                    new FieldDef { Id="Account_name",  Table="tbl_Accounts",    Alias="acc",  Column="AName", Type="text" },
//                    // Details (joined via ParentGuid)
//                    new FieldDef { Id="dtl_guid",     Table="tbl_JournalVoucherDetails", Alias="d", Column="Guid",       Type="guid" },
//                    new FieldDef { Id="dtl_parent",   Table="tbl_JournalVoucherDetails", Alias="d", Column="ParentGuid", Type="guid" },
//                    new FieldDef { Id="row_index",    Table="tbl_JournalVoucherDetails", Alias="d", Column="RowIndex",   Type="number" },
//                    new FieldDef { Id="debit",        Table="tbl_JournalVoucherDetails", Alias="d", Column="Debit",      Type="currency" },
//                    new FieldDef { Id="credit",       Table="tbl_JournalVoucherDetails", Alias="d", Column="Credit",     Type="currency" },
//                    new FieldDef { Id="total",        Table="tbl_JournalVoucherDetails", Alias="d", Column="Total",      Type="currency" },
//                    new FieldDef { Id="due_date",     Table="tbl_JournalVoucherDetails", Alias="d", Column="DueDate",    Type="date" },
//                    new FieldDef { Id="detail_note",  Table="tbl_JournalVoucherDetails", Alias="d", Column="Note",       Type="text" },
//                },
//                Joins = new List<JoinDef>
//                {  
//                    new JoinDef { FromAlias="h", FromColumn="BranchID",     ToTable="tbl_Branch",     ToAlias="b",  ToColumn="ID",   JoinType="LEFT" },
//                    new JoinDef { FromAlias="h", FromColumn="CostCenterID", ToTable="tbl_CostCenter", ToAlias="cc", ToColumn="ID",   JoinType="LEFT" },
//                    new JoinDef { FromAlias="h", FromColumn="Guid",         ToTable="tbl_JournalVoucherDetails", ToAlias="d", ToColumn="ParentGuid", JoinType="LEFT" },
//                    new JoinDef { FromAlias="d", FromColumn="AccountID", ToTable="tbl_Accounts", ToAlias="acc", ToColumn="ID",   JoinType="LEFT" },

//                }
//            };

//            return new Dictionary<string, ModuleDef>(StringComparer.OrdinalIgnoreCase)
//            {
//                { finance.Id, finance }
//            };
//        }

//        // --------------------------------------------------------------------
//        // Catalog models
//        // --------------------------------------------------------------------
//        public class ModuleDef
//        {
//            public string Id { get; set; } = "";
//            public string PrimaryTable { get; set; } = "";
//            public string PrimaryAlias { get; set; } = "";
//            public List<FieldDef> Fields { get; set; } = new();
//            public List<JoinDef> Joins { get; set; } = new();
//        }

//        public class FieldDef
//        {
//            public string Id { get; set; } = "";
//            public string Table { get; set; } = "";
//            public string Alias { get; set; } = "";
//            public string Column { get; set; } = "";
//            public string Type { get; set; } = "text"; // text/date/datetime/number/currency/bool/guid
//        }

//        public class JoinDef
//        {
//            public string FromAlias { get; set; } = "";
//            public string FromColumn { get; set; } = "";
//            public string ToTable { get; set; } = "";
//            public string ToAlias { get; set; } = "";
//            public string ToColumn { get; set; } = "";
//            public string JoinType { get; set; } = "LEFT"; // LEFT/INNER
//        }

//        // --------------------------------------------------------------------
//        // Your filter DTO (assumed). Adjust property names if yours differ.
//        // --------------------------------------------------------------------
//        public class RunReportFilter
//        {
//            public string fieldId { get; set; } = "";
//            public string operatorName { get; set; } = "eq"; // eq, ne, gt, gte, lt, lte, contains, starts, ends, between, in, isnull, notnull
//            public object? Value { get; set; }
//            public object? Value2 { get; set; } // for between
//        }
//        public class MeasureDto
//        {
//            public string FieldId { get; set; } = "";
//            public string Fn { get; set; } = "sum"; // sum,count,countDistinct,avg,min,max
//            public string? Alias { get; set; }
//        }

//        // --------------------------------------------------------------------
//        // PUBLIC: RunReport
//        // --------------------------------------------------------------------
//        public void RunReport(
//            int CompanyID,
//            string ModuleId,
//            List<string> FieldIds,
//            List<RunReportFilter> Filters,
//            string SortByFieldId,
//            string SortDir,
//            string GroupByFieldId,
//            int Page,
//            int PageSize,
//    // NEW
//    List<MeasureDto>? Agg,
//   bool IncludeRowCount,
//            ref DataTable dtRows,
//            ref int totalRows
//        )
//        {
//            dtRows = new DataTable();
//            totalRows = 0;

//            // 1) Load catalog + pick module
//            var catalog = BuildCatalog();
//            if (!catalog.TryGetValue(ModuleId, out var module))
//                throw new Exception($"Unknown ModuleId: {ModuleId}");

//            // 2) Resolve fields
//            var fieldMap = module.Fields.ToDictionary(f => f.Id, f => f, StringComparer.OrdinalIgnoreCase);

//            // if user passes nothing, select a sensible default set
//            if (FieldIds == null || FieldIds.Count == 0)
//            {
//                FieldIds = new List<string> { "jv_no", "voucher_date", "branch_name", "cc_name", "dtl_guid", "row_index", "debit", "credit", "total" };
//            }

//            // remove unknown field IDs silently (or throw if you prefer)
//            FieldIds = FieldIds.Where(id => fieldMap.ContainsKey(id)).Distinct(StringComparer.OrdinalIgnoreCase).ToList();
//            if (FieldIds.Count == 0)
//                throw new Exception("No valid FieldIds were provided.");

//            // 3) Build SQL parts
//            var sbSelect = new StringBuilder();
//            var sbFrom = new StringBuilder();
//            var sbJoin = new StringBuilder();
//            var sbWhere = new StringBuilder();
//            var sbOrder = new StringBuilder();

//            var parameters = new List<SqlParameter>();
//            int pIndex = 0;

//            // FROM
//            sbFrom.AppendLine($"FROM {module.PrimaryTable} {module.PrimaryAlias}");

//            // JOINs (module defined)
//            foreach (var j in module.Joins)
//            {
//                sbJoin.AppendLine($"{j.JoinType} JOIN {j.ToTable} {j.ToAlias} ON {j.FromAlias}.{j.FromColumn} = {j.ToAlias}.{j.ToColumn}");
//            }

//            // WHERE base: CompanyID on header (exists in your tables)
//            // Primary is header alias "h"
//            sbWhere.AppendLine("WHERE 1=1");
//            sbWhere.AppendLine($"AND {module.PrimaryAlias}.CompanyID = @p_company");
//            parameters.Add(new SqlParameter("@p_company", SqlDbType.Int) { Value = CompanyID });

//            // Extra filters
//            if (Filters != null)
//            {
//                foreach (var f in Filters)
//                {
//                    if (f == null) continue;
//                    if (string.IsNullOrWhiteSpace(f.fieldId)) continue;
//                    if (!fieldMap.TryGetValue(f.fieldId, out var fd)) continue;

//                    string expr = $"{fd.Alias}.{fd.Column}";
//                    AppendFilter(sbWhere, parameters, ref pIndex, expr, fd.Type, f);
//                }
//            }

//            // SELECT (normal vs grouping)
//            bool hasGroup = !string.IsNullOrWhiteSpace(GroupByFieldId) && fieldMap.ContainsKey(GroupByFieldId);

//            if (hasGroup)
//            {
//                var gfd = fieldMap[GroupByFieldId];

//                sbSelect.AppendLine("SELECT");
//                sbSelect.AppendLine($"  {gfd.Alias}.{gfd.Column} AS [{gfd.Id}]");

//                // NEW: add row count if requested
//                if (IncludeRowCount)
//                    sbSelect.AppendLine($", COUNT(1) AS [group_count]");

//                // NEW: add measures (SUM/AVG/MIN/MAX/COUNT distinct ...)
//                if (Agg != null)
//                {
//                    foreach (var m in Agg)
//                    {
//                        if (m == null || string.IsNullOrWhiteSpace(m.FieldId)) continue;
//                        if (!fieldMap.TryGetValue(m.FieldId, out var mfd)) continue;

//                        string fn = (m.Fn ?? "sum").Trim().ToLowerInvariant();

//                        // allow only safe functions (prevent SQL injection)
//                        string sqlFn = fn switch
//                        {
//                            "sum" => "SUM",
//                            "avg" => "AVG",
//                            "min" => "MIN",
//                            "max" => "MAX",
//                            "count" => "COUNT",
//                            "countdistinct" => "COUNT",
//                            _ => "SUM"
//                        };

//                        string alias = (m.FieldId)
//                          ;  //       : $"{fn}_{m.FieldId}";

//                        if (fn == "countdistinct")
//                            sbSelect.AppendLine($", COUNT(DISTINCT {mfd.Alias}.{mfd.Column}) AS [{alias}]");
//                        else if (fn == "count")
//                            sbSelect.AppendLine($", COUNT({mfd.Alias}.{mfd.Column}) AS [{alias}]");
//                        else
//                            sbSelect.AppendLine($", {sqlFn}({mfd.Alias}.{mfd.Column}) AS [{alias}]");
//                    }
//                }
//            }
//            else
//            {
//                sbSelect.AppendLine("SELECT");
//                for (int i = 0; i < FieldIds.Count; i++)
//                {
//                    var fd = fieldMap[FieldIds[i]];
//                    string comma = (i == FieldIds.Count - 1) ? "" : ",";
//                    sbSelect.AppendLine($"  {fd.Alias}.{fd.Column} AS [{fd.Id}]{comma}");
//                }
//            }

//            // ORDER BY
//            string sortDir = (string.Equals(SortDir, "desc", StringComparison.OrdinalIgnoreCase)) ? "DESC" : "ASC";

//            if (hasGroup)
//            {
//                // order by group field by default
//                var gfd = fieldMap[GroupByFieldId];
//                sbOrder.AppendLine($"ORDER BY {gfd.Alias}.{gfd.Column} {sortDir}");
//            }
//            else if (!string.IsNullOrWhiteSpace(SortByFieldId) && fieldMap.TryGetValue(SortByFieldId, out var sfd) && FieldIds.Contains(sfd.Id, StringComparer.OrdinalIgnoreCase))
//            {
//                sbOrder.AppendLine($"ORDER BY {sfd.Alias}.{sfd.Column} {sortDir}");
//            }
//            else
//            {
//                // Default: header Guid then detail RowIndex (stable)
//                sbOrder.AppendLine($"ORDER BY {module.PrimaryAlias}.Guid DESC, d.RowIndex ASC");
//            }

//            // Paging (SQL Server OFFSET/FETCH)
//            if (Page <= 0) Page = 1;
//            if (PageSize <= 0) PageSize = 50;

//            int offset = (Page - 1) * PageSize;

//            // 4) Final SQL
//            // Data query
//            var sqlData = new StringBuilder();
//            sqlData.Append(sbSelect);
//            sqlData.Append(sbFrom);
//            sqlData.Append(sbJoin);
//            sqlData.Append(sbWhere);

//            if (hasGroup)
//            {
//                var gfd = fieldMap[GroupByFieldId];
//                sqlData.AppendLine($"GROUP BY {gfd.Alias}.{gfd.Column}");
//            }

//            sqlData.Append(sbOrder);
//            sqlData.AppendLine("OFFSET @p_offset ROWS FETCH NEXT @p_fetch ROWS ONLY;");

//            parameters.Add(new SqlParameter("@p_offset", SqlDbType.Int) { Value = offset });
//            parameters.Add(new SqlParameter("@p_fetch", SqlDbType.Int) { Value = PageSize });

//            // Count query
//            var sqlCount = new StringBuilder();
//            if (hasGroup)
//            {
//                // Count groups
//                var gfd = fieldMap[GroupByFieldId];
//             sqlCount.AppendLine("SELECT COUNT(1) FROM (");
//                sqlCount.AppendLine($"  SELECT {gfd.Alias}.{gfd.Column}");
//                sqlCount.Append(sbFrom);
//                sqlCount.Append(sbJoin);
//                sqlCount.Append(sbWhere);
//                sqlCount.AppendLine($"  GROUP BY {gfd.Alias}.{gfd.Column}");
//                sqlCount.AppendLine(") x;");
//            }
//            else
//            {
//                // Count rows (details rows)
//                sqlCount.AppendLine("SELECT COUNT(1)");
//                sqlCount.Append(sbFrom);
//                sqlCount.Append(sbJoin);
//                sqlCount.Append(sbWhere);
//            }

//            // 5) Execute (replace with your clsSQL)
//            dtRows = ExecuteDataTable(sqlData.ToString(), parameters, CompanyID);
//            var countParams = parameters
//    .Where(p => p.ParameterName != "@p_offset" && p.ParameterName != "@p_fetch")
//    .Select(p => new SqlParameter(p.ParameterName, p.SqlDbType)
//    {
//        Value = p.Value ?? DBNull.Value,
//        Size = p.Size,
//        Precision = p.Precision,
//        Scale = p.Scale
//    })
//    .ToList();

//            object? c = ExecuteScalar(sqlCount.ToString(), countParams, CompanyID);
//            totalRows = (c == null || c == DBNull.Value) ? 0 : Convert.ToInt32(c);
//            //  object? c = ExecuteScalar(sqlCount.ToString(), parameters.Where(p => p.ParameterName != "@p_offset" && p.ParameterName != "@p_fetch").ToList(), CompanyID);
//            // totalRows = (c == null || c == DBNull.Value) ? 0 : Convert.ToInt32(c);

//            //        var countParams = parameters
//            //.Where(p => p.ParameterName != "@p_offset" && p.ParameterName != "@p_fetch")
//            //.ToArray();
//            //        clsSQL clsSQL = new clsSQL();
//            //        object? c = clsSQL.ExecuteScalarText(
//            //            sqlCount.ToString(),
//            //            countParams,
//            //            clsSQL.CreateDataBaseConnectionString(CompanyID)
//            //        );

//            //        totalRows = (c == null || c == DBNull.Value) ? 0 : Convert.ToInt32(c);
//        }

//        // --------------------------------------------------------------------
//        // Module catalog (Finance single module)
//        // --------------------------------------------------------------------

//        // --------------------------------------------------------------------
//        // Filter builder (parameterized)
//        // --------------------------------------------------------------------
//        //private static void AppendFilter(
//        //    StringBuilder sbWhere,
//        //    List<SqlParameter> parameters,
//        //    ref int pIndex,
//        //    string expr,
//        //    string type,
//        //    RunReportFilter f
//        //)
//        //{
//        //    string op = (f.Op ?? "eq").Trim().ToLowerInvariant();

//        //    // Helpers for parameter creation
//        //    string pn = "@p_" + pIndex;
//        //    pIndex++;

//        //    // null ops
//        //    if (op == "isnull")
//        //    {
//        //        sbWhere.AppendLine($"AND {expr} IS NULL");
//        //        return;
//        //    }
//        //    if (op == "notnull")
//        //    {
//        //        sbWhere.AppendLine($"AND {expr} IS NOT NULL");
//        //        return;
//        //    }

//        //    // IN (expects Value = IEnumerable or comma string)
//        //    if (op == "in")
//        //    {
//        //        var values = new List<object?>();
//        //        if (f.Value is IEnumerable<object> ieObj) values.AddRange(ieObj);
//        //        else if (f.Value is string s && s.Contains(",")) values.AddRange(s.Split(',').Select(x => (object?)x.Trim()));
//        //        else values.Add(f.Value);

//        //        values = values.Where(v => v != null).ToList();
//        //        if (values.Count == 0) return;

//        //        var inParams = new List<string>();
//        //        foreach (var v in values)
//        //        {
//        //            string pn = NextParamName();
//        //            inParams.Add(pn);
//        //            parameters.Add(MakeParam(pn, type, v));
//        //        }
//        //        sbWhere.AppendLine($"AND {expr} IN ({string.Join(",", inParams)})");
//        //        return;
//        //    }

//        //    // BETWEEN
//        //    if (op == "between")
//        //    {
//        //        if (f.Value == null || f.Value2 == null) return;
//        //        string p1 = NextParamName();
//        //        string p2 = NextParamName();
//        //        parameters.Add(MakeParam(p1, type, f.Value));
//        //        parameters.Add(MakeParam(p2, type, f.Value2));
//        //        sbWhere.AppendLine($"AND {expr} BETWEEN {p1} AND {p2}");
//        //        return;
//        //    }

//        //    // LIKE ops
//        //    if (op == "contains" || op == "starts" || op == "ends")
//        //    {
//        //        if (f.Value == null) return;
//        //        string pn = NextParamName();

//        //        string raw = Convert.ToString(f.Value) ?? "";
//        //        string like = op switch
//        //        {
//        //            "starts" => raw + "%",
//        //            "ends" => "%" + raw,
//        //            _ => "%" + raw + "%"
//        //        };

//        //        parameters.Add(new SqlParameter(pn, SqlDbType.NVarChar) { Value = like });
//        //        sbWhere.AppendLine($"AND {expr} LIKE {pn}");
//        //        return;
//        //    }

//        //    // Comparison ops
//        //    string sqlOp = op switch
//        //    {
//        //        "eq" => "=",
//        //        "ne" => "<>",
//        //        "gt" => ">",
//        //        "gte" => ">=",
//        //        "lt" => "<",
//        //        "lte" => "<=",
//        //        _ => "="
//        //    };

//        //    if (f.Value == null)
//        //    {
//        //        // Treat eq null as IS NULL, ne null as IS NOT NULL
//        //        if (sqlOp == "=") sbWhere.AppendLine($"AND {expr} IS NULL");
//        //        else if (sqlOp == "<>") sbWhere.AppendLine($"AND {expr} IS NOT NULL");
//        //        return;
//        //    }

//        //    string p = NextParamName();
//        //    parameters.Add(MakeParam(p, type, f.Value));
//        //    sbWhere.AppendLine($"AND {expr} {sqlOp} {p}");
//        //}
//        private static void AppendFilter(
//            StringBuilder sbWhere,
//            List<SqlParameter> parameters,
//            ref int pIndex,
//            string expr,
//            string type,
//            RunReportFilter f
//        )
//        {
//           // string op = (f.Op ?? "eq").Trim().ToLowerInvariant();
//            string op = NormalizeOp(f.operatorName);
//            // NULL operators
//            if (op == "isnull")
//            {
//                sbWhere.AppendLine($"AND {expr} IS NULL");
//                return;
//            }

//            if (op == "notnull")
//            {
//                sbWhere.AppendLine($"AND {expr} IS NOT NULL");
//                return;
//            }

//            // IN operator
//            //if (op == "in")
//            //{
//            //    if (f.Value == null) return;

//            //    var values = f.Value.ToString()?.Split(',') ?? Array.Empty<string>();
//            //    if (values.Length == 0) return;

//            //    var inParams = new List<string>();

//            //    foreach (var v in values)
//            //    {
//            //        string pn = "@p_" + pIndex;
//            //        pIndex++;

//            //        inParams.Add(pn);
//            //        parameters.Add(MakeParam(pn, type, v.Trim()));
//            //    }

//            //    sbWhere.AppendLine($"AND {expr} IN ({string.Join(",", inParams)})");
//            //    return;
//            //}
//            if (op == "in")
//            {
//                if (f.Value == null) return;

//                var v = NormalizeJsonValue(f.Value);

//                List<string> items = new();

//                if (v is string s)
//                {
//                    s = s.Trim();

//                    // JSON array string
//                    if (s.StartsWith("[") && s.EndsWith("]"))
//                    {
//                        try
//                        {
//                            using var doc = JsonDocument.Parse(s);
//                            if (doc.RootElement.ValueKind == JsonValueKind.Array)
//                            {
//                                foreach (var el in doc.RootElement.EnumerateArray())
//                                    items.Add(el.ToString());
//                            }
//                        }
//                        catch
//                        {
//                            // fallback to comma split
//                            items.AddRange(s.Split(',').Select(x => x.Trim()).Where(x => x.Length > 0));
//                        }
//                    }
//                    else
//                    {
//                        items.AddRange(s.Split(',').Select(x => x.Trim()).Where(x => x.Length > 0));
//                    }
//                }
//                else
//                {
//                    items.Add(v.ToString() ?? "");
//                }

//                if (items.Count == 0) return;

//                var inParams = new List<string>();
//                foreach (var item in items)
//                {
//                    string pn = "@p_" + pIndex++;
//                    inParams.Add(pn);
//                    parameters.Add(MakeParam(pn, type, item));
//                }

//                sbWhere.AppendLine($"AND {expr} IN ({string.Join(",", inParams)})");
//                return;
//            }
//            // BETWEEN
//            if (op == "between")
//            {
//                if (f.Value == null || f.Value2 == null) return;

//                string p1 = "@p_" + pIndex;
//                pIndex++;

//                string p2 = "@p_" + pIndex;
//                pIndex++;

//                parameters.Add(MakeParam(p1, type, f.Value));
//                parameters.Add(MakeParam(p2, type, f.Value2));

//                sbWhere.AppendLine($"AND {expr} BETWEEN {p1} AND {p2}");
//                return;
//            }

//            // LIKE operators
//            if (op == "contains" || op == "starts" || op == "ends")
//            {
//                if (f.Value == null) return;

//                string pn = "@p_" + pIndex;
//                pIndex++;

//                string raw = f.Value.ToString() ?? "";
//                string likeValue = op switch
//                {
//                    "starts" => raw + "%",
//                    "ends" => "%" + raw,
//                    _ => "%" + raw + "%"
//                };

//                parameters.Add(new SqlParameter(pn, SqlDbType.NVarChar) { Value = likeValue });
//                sbWhere.AppendLine($"AND {expr} LIKE {pn}");
//                return;
//            }

//            // Comparison operators
//            string sqlOp = op switch
//            {
//                "eq" => "=",
//                "ne" => "<>",
//                "gt" => ">",
//                "gte" => ">=",
//                "lt" => "<",
//                "lte" => "<=",
//                _ => "="
//            };

//            if (f.Value == null)
//            {
//                if (sqlOp == "=")
//                    sbWhere.AppendLine($"AND {expr} IS NULL");
//                else if (sqlOp == "<>")
//                    sbWhere.AppendLine($"AND {expr} IS NOT NULL");
//                return;
//            }

//            string paramName = "@p_" + pIndex;
//            pIndex++;

//            parameters.Add(MakeParam(paramName, type, f.Value));
//            sbWhere.AppendLine($"AND {expr} {sqlOp} {paramName}");
//        }
//        //private static SqlParameter MakeParam(string name, string type, object? value)
//        //{
//        //    // Keep it simple; SQL Server will convert most things.
//        //    // You can tighten types if you want.
//        //    switch ((type ?? "").ToLowerInvariant())
//        //    {
//        //        case "number":
//        //            return new SqlParameter(name, SqlDbType.Decimal) { Value = Convert.ToDecimal(value) };
//        //        case "currency":
//        //            return new SqlParameter(name, SqlDbType.Decimal) { Value = Convert.ToDecimal(value) };
//        //        case "bool":
//        //            return new SqlParameter(name, SqlDbType.Bit) { Value = Convert.ToBoolean(value) };
//        //        case "date":
//        //        case "datetime":
//        //            return new SqlParameter(name, SqlDbType.DateTime) { Value = Convert.ToDateTime(value) };
//        //        case "guid":
//        //            return new SqlParameter(name, SqlDbType.UniqueIdentifier) { Value = (value is Guid g) ? g : Guid.Parse(value.ToString()!) };
//        //        default:
//        //            return new SqlParameter(name, SqlDbType.NVarChar) { Value = Convert.ToString(value) ?? "" };
//        //    }
//        //}
//        private static SqlParameter MakeParam(string name, string type, object? value)
//        {
//            object? v = NormalizeJsonValue(value);

//            if (v == null)
//                return new SqlParameter(name, SqlDbType.NVarChar) { Value = DBNull.Value };

//            switch ((type ?? "").ToLowerInvariant())
//            {
//                case "number":
//                case "currency":
//                    {
//                        // Accept numeric types or numeric strings
//                        if (v is decimal d) return new SqlParameter(name, SqlDbType.Decimal) { Value = d };
//                        if (v is double db) return new SqlParameter(name, SqlDbType.Decimal) { Value = Convert.ToDecimal(db) };
//                        if (v is float fl) return new SqlParameter(name, SqlDbType.Decimal) { Value = Convert.ToDecimal(fl) };
//                        if (v is long l) return new SqlParameter(name, SqlDbType.Decimal) { Value = Convert.ToDecimal(l) };
//                        if (v is int i) return new SqlParameter(name, SqlDbType.Decimal) { Value = Convert.ToDecimal(i) };

//                        // string
//                        if (decimal.TryParse(v.ToString(), out var parsed))
//                            return new SqlParameter(name, SqlDbType.Decimal) { Value = parsed };

//                        // fallback
//                        return new SqlParameter(name, SqlDbType.Decimal) { Value = 0m };
//                    }

//                case "bool":
//                    {
//                        if (v is bool b) return new SqlParameter(name, SqlDbType.Bit) { Value = b };

//                        if (bool.TryParse(v.ToString(), out var parsed))
//                            return new SqlParameter(name, SqlDbType.Bit) { Value = parsed };

//                        // accept 0/1
//                        if (int.TryParse(v.ToString(), out var bi))
//                            return new SqlParameter(name, SqlDbType.Bit) { Value = bi != 0 };

//                        return new SqlParameter(name, SqlDbType.Bit) { Value = false };
//                    }

//                case "date":
//                case "datetime":
//                    {
//                        if (v is DateTime dt)
//                            return new SqlParameter(name, SqlDbType.DateTime) { Value = dt };

//                        // If Flutter sends ISO string
//                        if (DateTime.TryParse(v.ToString(), out var parsed))
//                            return new SqlParameter(name, SqlDbType.DateTime) { Value = parsed };

//                        return new SqlParameter(name, SqlDbType.DateTime) { Value = DBNull.Value };
//                    }

//                case "guid":
//                    {
//                        if (v is Guid g)
//                            return new SqlParameter(name, SqlDbType.UniqueIdentifier) { Value = g };

//                        if (Guid.TryParse(v.ToString(), out var parsed))
//                            return new SqlParameter(name, SqlDbType.UniqueIdentifier) { Value = parsed };

//                        return new SqlParameter(name, SqlDbType.UniqueIdentifier) { Value = DBNull.Value };
//                    }

//                default:
//                    return new SqlParameter(name, SqlDbType.NVarChar) { Value = v.ToString() ?? "" };
//            }
//        }
//        private static object? NormalizeJsonValue(object? value)
//        {
//            if (value == null) return null;

//            if (value is JsonElement je)
//            {
//                switch (je.ValueKind)
//                {
//                    case JsonValueKind.Null:
//                    case JsonValueKind.Undefined:
//                        return null;

//                    case JsonValueKind.String:
//                        return je.GetString();

//                    case JsonValueKind.Number:
//                        // prefer decimal for money
//                        if (je.TryGetDecimal(out var dec)) return dec;
//                        if (je.TryGetInt64(out var i64)) return i64;
//                        return je.GetDouble();

//                    case JsonValueKind.True:
//                    case JsonValueKind.False:
//                        return je.GetBoolean();

//                    // For arrays/objects, return raw json string (useful for logging or "in" parsing if you want)
//                    case JsonValueKind.Array:
//                    case JsonValueKind.Object:
//                        return je.GetRawText();

//                    default:
//                        return je.ToString();
//                }
//            }

//            return value;
//        }
//        private static string NormalizeOp(string? op)
//        {
//            op = (op ?? "eq").Trim();

//            // Flutter sends enum name like: equals, notEquals, greaterThan, lessThan, between, inList
//            return op.ToLowerInvariant() switch
//            {
//                "equals" => "eq",
//                "notequals" => "ne",
//                "contains" => "contains",
//                "greaterthan" => "gt",
//                "greaterthanorequal" => "gte",
//                "lessthan" => "lt",
//                "lessthanorequal" => "lte",
//                "between" => "between",
//                "inlist" => "in",

//                // if backend already sends eq/gt/... keep it
//                "eq" => "eq",
//                "ne" => "ne",
//                "gt" => "gt",
//                "gte" => "gte",
//                "lt" => "lt",
//                "lte" => "lte",
//                "in" => "in",
//                "starts" => "starts",
//                "ends" => "ends",
//                "isnull" => "isnull",
//                "notnull" => "notnull",

//                _ => "eq"
//            };
//        }
//        // --------------------------------------------------------------------
//        // Replace these with your clsSQL calls
//        // --------------------------------------------------------------------
//        private DataTable ExecuteDataTable(string sql, List<SqlParameter> prms,int CompanyID)
//        {
//            // Example ADO.NET implementation. Replace with clsSQL if you want.
//            // var cls = new clsSQL();
//            // return cls.GetDataTable(sql, prms);
//            clsSQL clsSQL = new clsSQL();
//            using var cn = new SqlConnection(clsSQL.CreateDataBaseConnectionString(CompanyID));
//            using var cmd = new SqlCommand(sql, cn);
//            cmd.Parameters.AddRange(prms.ToArray());
//            using var da = new SqlDataAdapter(cmd);
//            var dt = new DataTable();
//            da.Fill(dt);
//            return dt;
//        }

//        private object? ExecuteScalar(string sql, List<SqlParameter> prms, int CompanyID)
//        {
//            clsSQL clsSQL = new clsSQL();
//            // Example ADO.NET implementation. Replace with clsSQL if you want.
//            using var cn = new SqlConnection(clsSQL.CreateDataBaseConnectionString(CompanyID));
//            using var cmd = new SqlCommand(sql, cn);
//            cmd.Parameters.AddRange(prms.ToArray());
//            cn.Open();
//            return cmd.ExecuteScalar();
//        }
//    }
//}
using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Text.Json;

namespace WebApplication2.Controllers
{
    /// <summary>
    /// Full dynamic report catalog.
    /// Covers: Sales Invoices · Items · Business Partners · POS Sessions
    /// All fields are user-facing only — no IDs, GUIDs, or audit columns.
    /// </summary>
    public partial class ReportBuilderService
    {
        public List<object> GetCatalogList(int CompanyID)
        {
            return BuildCatalog()
                .Values
                .Select(m => (object)new
                {
                    id = m.Id,
                    name = m.Label,        // ← was "label", Flutter reads "name"
                    label = m.Label,        // keep both just in case
                    icon = m.Icon,
                    color = m.Color,
                    primaryTable = m.PrimaryTable,
                    fields = m.Fields.Select(f => new
                    {
                        id = f.Id,
                        name = f.Label,          // ← "name" for display
                        label = f.Label,          // keep both
                        type = f.Type,
                        icon = FieldIcon(f.Type),
                        table = f.Table,
                        column = f.Column
                    }).ToList(),
                    joins = m.Joins.Select(j => new
                    {
                        fromAlias = j.FromAlias,
                        fromColumn = j.FromColumn,
                        toTable = j.ToTable,
                        toAlias = j.ToAlias,
                        toColumn = j.ToColumn,
                        joinType = j.JoinType
                    }).ToList()
                })
                .ToList();
        }

        private static string FieldIcon(string type) => type switch
        {
            "date" or "datetime" => "calendar",
            "currency" => "dollar-sign",
            "number" => "hash",
            "bool" => "toggle-left",
            _ => "type"
        };





        // =====================================================================
        // CATALOG
        // =====================================================================
        public Dictionary<string, ModuleDef> BuildCatalog()
        {
            var modules = new List<ModuleDef>
            {  BuildModule_JournalVouchers(),    // ← NEW
                BuildModule_SalesInvoices(),
                BuildModule_Items(),
                BuildModule_BusinessPartners(),
                BuildModule_POSSessions()
            };

            return modules.ToDictionary(
                m => m.Id,
                m => m,
                StringComparer.OrdinalIgnoreCase
            );
        }

        // =====================================================================
        // MODULE 1 — SALES INVOICES
        // Primary  : tbl_InvoiceHeader  (alias: h)
        // Joined   : tbl_BusinessPartner (bp) · tbl_Branch (br) · tbl_Store (st)
        //            tbl_Currency (cur)  · tbl_InvoiceDetails (d)
        //            tbl_Items (itm)     · tbl_ItemsCategory (cat)
        //            tbl_Tax (tax)
        // =====================================================================
        private static ModuleDef BuildModule_JournalVouchers() => new ModuleDef
        {
            Id = "finance_journal_vouchers",
            Label = "Journal Vouchers",
            Icon = "file-text",
            Color = "#2563EB",
            PrimaryTable = "tbl_JournalVoucherHeader",
            PrimaryAlias = "h",
            PrimaryKey = "Guid",

            Fields = new List<FieldDef>
    {
        // ── Header ────────────────────────────────────────────────────
        new FieldDef { Id = "jv_no",          Label = "JV Number",        Table = "tbl_JournalVoucherHeader",  Alias = "h",   Column = "JVNumber",          Type = "text"     },
        new FieldDef { Id = "voucher_date",   Label = "Voucher Date",     Table = "tbl_JournalVoucherHeader",  Alias = "h",   Column = "VoucherDate",       Type = "date"     },
        new FieldDef { Id = "notes",          Label = "Notes",            Table = "tbl_JournalVoucherHeader",  Alias = "h",   Column = "Notes",             Type = "text"     },
        new FieldDef { Id = "created_at",     Label = "Created At",       Table = "tbl_JournalVoucherHeader",  Alias = "h",   Column = "CreationDate",      Type = "datetime" },
        new FieldDef { Id = "modified_at",    Label = "Modified At",      Table = "tbl_JournalVoucherHeader",  Alias = "h",   Column = "ModificationDate",  Type = "datetime" },

        // ── Header → Branch & Cost Center ────────────────────────────
        new FieldDef { Id = "branch_name",    Label = "Branch",           Table = "tbl_Branch",                Alias = "br",  Column = "AName",             Type = "text"     },
        new FieldDef { Id = "cc_name",        Label = "Cost Center",      Table = "tbl_CostCenter",            Alias = "cc",  Column = "AName",             Type = "text"     },

        // ── Details ───────────────────────────────────────────────────
        new FieldDef { Id = "row_index",      Label = "Row",              Table = "tbl_JournalVoucherDetails", Alias = "d",   Column = "RowIndex",          Type = "number"   },
        new FieldDef { Id = "debit",          Label = "Debit",            Table = "tbl_JournalVoucherDetails", Alias = "d",   Column = "Debit",             Type = "currency" },
        new FieldDef { Id = "credit",         Label = "Credit",           Table = "tbl_JournalVoucherDetails", Alias = "d",   Column = "Credit",            Type = "currency" },
        new FieldDef { Id = "total",          Label = "Total",            Table = "tbl_JournalVoucherDetails", Alias = "d",   Column = "Total",             Type = "currency" },
        new FieldDef { Id = "due_date",       Label = "Due Date",         Table = "tbl_JournalVoucherDetails", Alias = "d",   Column = "DueDate",           Type = "date"     },
        new FieldDef { Id = "detail_note",    Label = "Detail Note",      Table = "tbl_JournalVoucherDetails", Alias = "d",   Column = "Note",              Type = "text"     },

        // ── Detail → Account ──────────────────────────────────────────
        new FieldDef { Id = "account_name",   Label = "Account",          Table = "tbl_Accounts",              Alias = "acc", Column = "AName",             Type = "text"     },
        new FieldDef { Id = "account_number", Label = "Account Number",   Table = "tbl_Accounts",              Alias = "acc", Column = "AccountNumber",     Type = "text"     },

        // ── Detail → Branch & Cost Center ────────────────────────────
        new FieldDef { Id = "dtl_branch_name",Label = "Detail Branch",    Table = "tbl_Branch",                Alias = "dbr", Column = "AName",             Type = "text"     },
        new FieldDef { Id = "dtl_cc_name",    Label = "Detail Cost Center",Table = "tbl_CostCenter",           Alias = "dcc", Column = "AName",             Type = "text"     },
    },

            Joins = new List<JoinDef>
    {
        // header → details  (one-to-many; drives row granularity)
        new JoinDef { FromAlias = "h",   FromColumn = "Guid",         ToTable = "tbl_JournalVoucherDetails", ToAlias = "d",   ToColumn = "ParentGuid",  JoinType = "LEFT" },

        // header → branch (header level)
        new JoinDef { FromAlias = "h",   FromColumn = "BranchID",     ToTable = "tbl_Branch",                ToAlias = "br",  ToColumn = "ID",          JoinType = "LEFT" },

        // header → cost center (header level)
        new JoinDef { FromAlias = "h",   FromColumn = "CostCenterID", ToTable = "tbl_CostCenter",            ToAlias = "cc",  ToColumn = "ID",          JoinType = "LEFT" },

        // detail → account
        new JoinDef { FromAlias = "d",   FromColumn = "AccountID",    ToTable = "tbl_Accounts",              ToAlias = "acc", ToColumn = "ID",          JoinType = "LEFT" },

        // detail → branch (detail level — separate alias to avoid conflict)
        new JoinDef { FromAlias = "d",   FromColumn = "BranchID",     ToTable = "tbl_Branch",                ToAlias = "dbr", ToColumn = "ID",          JoinType = "LEFT" },

        // detail → cost center (detail level — separate alias)
        new JoinDef { FromAlias = "d",   FromColumn = "CostCenterID", ToTable = "tbl_CostCenter",            ToAlias = "dcc", ToColumn = "ID",          JoinType = "LEFT" },
    }
        }; private static ModuleDef BuildModule_SalesInvoices() => new ModuleDef
        {
            Id = "sales_invoices",
            Label = "Sales Invoices",
            Icon = "receipt",
            Color = "#2563EB",
            PrimaryTable = "tbl_InvoiceHeader",
            PrimaryAlias = "h",
            PrimaryKey = "Guid",     // ← ADD THIS
            Fields = new List<FieldDef>
            {
                // ── Invoice header ────────────────────────────────────────────
                new FieldDef { Id = "inv_no",           Label = "Invoice No",        Table = "tbl_InvoiceHeader",    Alias = "h",   Column = "InvoiceNo",       Type = "number"   },
                new FieldDef { Id = "inv_date",         Label = "Invoice Date",      Table = "tbl_InvoiceHeader",    Alias = "h",   Column = "InvoiceDate",     Type = "date"     },
                new FieldDef { Id = "inv_ref",          Label = "Reference No",      Table = "tbl_InvoiceHeader",    Alias = "h",   Column = "RefNo",           Type = "text"     },
                new FieldDef { Id = "inv_note",         Label = "Note",              Table = "tbl_InvoiceHeader",    Alias = "h",   Column = "Note",            Type = "text"     },
                new FieldDef { Id = "inv_total_tax",    Label = "Total Tax",         Table = "tbl_InvoiceHeader",    Alias = "h",   Column = "TotalTax",        Type = "currency" },
                new FieldDef { Id = "inv_hdr_discount", Label = "Header Discount",   Table = "tbl_InvoiceHeader",    Alias = "h",   Column = "HeaderDiscount",  Type = "currency" },
                new FieldDef { Id = "inv_total_disc",   Label = "Total Discount",    Table = "tbl_InvoiceHeader",    Alias = "h",   Column = "TotalDiscount",   Type = "currency" },
                new FieldDef { Id = "inv_total",        Label = "Invoice Total",     Table = "tbl_InvoiceHeader",    Alias = "h",   Column = "TotalInvoice",    Type = "currency" },
                new FieldDef { Id = "inv_currency_rate",Label = "Currency Rate",     Table = "tbl_InvoiceHeader",    Alias = "h",   Column = "CurrencyRate",    Type = "number"   },
                new FieldDef { Id = "inv_base_amount",  Label = "Base Amount",       Table = "tbl_InvoiceHeader",    Alias = "h",   Column = "CurrencyBaseAmount", Type = "currency" },
                new FieldDef { Id = "inv_is_posted",    Label = "Is Posted",         Table = "tbl_InvoiceHeader",    Alias = "h",   Column = "IsPosted",        Type = "bool"     },

                // ── Business partner ──────────────────────────────────────────
                new FieldDef { Id = "bp_name",          Label = "Customer / Supplier", Table = "tbl_BusinessPartner", Alias = "bp",  Column = "AName",          Type = "text"     },
                new FieldDef { Id = "bp_commercial",    Label = "Commercial Name",    Table = "tbl_BusinessPartner", Alias = "bp",  Column = "CommercialName",  Type = "text"     },
                new FieldDef { Id = "bp_tel",           Label = "Partner Tel",        Table = "tbl_BusinessPartner", Alias = "bp",  Column = "Tel",             Type = "text"     },
                new FieldDef { Id = "bp_email",         Label = "Partner Email",      Table = "tbl_BusinessPartner", Alias = "bp",  Column = "Email",           Type = "text"     },

                // ── Branch & Store ────────────────────────────────────────────
                new FieldDef { Id = "branch_name",      Label = "Branch",            Table = "tbl_Branch",           Alias = "br",  Column = "AName",           Type = "text"     },
                new FieldDef { Id = "store_name",       Label = "Store",             Table = "tbl_Store",            Alias = "st",  Column = "AName",           Type = "text"     },

                // ── Currency ──────────────────────────────────────────────────
                new FieldDef { Id = "currency_name",    Label = "Currency",          Table = "tbl_Currency",         Alias = "cur", Column = "EName",           Type = "text"     },
                new FieldDef { Id = "currency_symbol",  Label = "Currency Symbol",   Table = "tbl_Currency",         Alias = "cur", Column = "Symbol",          Type = "text"     },

                // ── Invoice line details ───────────────────────────────────────
                new FieldDef { Id = "line_item_name",   Label = "Item Name",         Table = "tbl_InvoiceDetails",   Alias = "d",   Column = "ItemName",        Type = "text"     },
                new FieldDef { Id = "line_qty",         Label = "Quantity",          Table = "tbl_InvoiceDetails",   Alias = "d",   Column = "Qty",             Type = "number"   },
                new FieldDef { Id = "line_free_qty",    Label = "Free Qty",          Table = "tbl_InvoiceDetails",   Alias = "d",   Column = "FreeQty",         Type = "number"   },
                new FieldDef { Id = "line_total_qty",   Label = "Total Qty",         Table = "tbl_InvoiceDetails",   Alias = "d",   Column = "TotalQTY",        Type = "number"   },
                new FieldDef { Id = "line_price_bt",    Label = "Price (Before Tax)",Table = "tbl_InvoiceDetails",   Alias = "d",   Column = "PriceBeforeTax",  Type = "currency" },
                new FieldDef { Id = "line_price_at",    Label = "Price (After Tax)", Table = "tbl_InvoiceDetails",   Alias = "d",   Column = "PriceAfterTaxPcs",Type = "currency" },
                new FieldDef { Id = "line_tax_pct",     Label = "Tax %",             Table = "tbl_InvoiceDetails",   Alias = "d",   Column = "TaxPercentage",   Type = "number"   },
                new FieldDef { Id = "line_tax_amt",     Label = "Tax Amount",        Table = "tbl_InvoiceDetails",   Alias = "d",   Column = "TaxAmount",       Type = "currency" },
                new FieldDef { Id = "line_disc_bt",     Label = "Discount (Before Tax)", Table = "tbl_InvoiceDetails", Alias = "d", Column = "DiscountBeforeTaxAmountPcs", Type = "currency" },
                new FieldDef { Id = "line_disc_at",     Label = "Discount (After Tax)",  Table = "tbl_InvoiceDetails", Alias = "d", Column = "DiscountAfterTaxAmountPcs",  Type = "currency" },
                new FieldDef { Id = "line_service_bt",  Label = "Service (Before Tax)",  Table = "tbl_InvoiceDetails", Alias = "d", Column = "ServiceBeforeTax",           Type = "currency" },
                new FieldDef { Id = "line_service_tax", Label = "Service Tax",           Table = "tbl_InvoiceDetails", Alias = "d", Column = "ServiceTaxAmount",           Type = "currency" },
                new FieldDef { Id = "line_service_at",  Label = "Service (After Tax)",   Table = "tbl_InvoiceDetails", Alias = "d", Column = "ServiceAfterTax",            Type = "currency" },
                new FieldDef { Id = "line_total",       Label = "Line Total",        Table = "tbl_InvoiceDetails",   Alias = "d",   Column = "TotalLine",       Type = "currency" },
                new FieldDef { Id = "line_avg_cost",    Label = "Avg Cost / Unit",   Table = "tbl_InvoiceDetails",   Alias = "d",   Column = "AVGCostPerUnit",  Type = "currency" },

                // ── Item master ───────────────────────────────────────────────
                new FieldDef { Id = "item_barcode",     Label = "Barcode",           Table = "tbl_Items",            Alias = "itm", Column = "Barcode",         Type = "text"     },

                // ── Item category ─────────────────────────────────────────────
                new FieldDef { Id = "item_category",    Label = "Category",          Table = "tbl_ItemsCategory",    Alias = "cat", Column = "AName",           Type = "text"     },
            },

            Joins = new List<JoinDef>
            {
                // header → business partner
                new JoinDef { FromAlias = "h",   FromColumn = "BusinessPartnerID", ToTable = "tbl_BusinessPartner", ToAlias = "bp",  ToColumn = "ID",         JoinType = "LEFT" },
                // header → branch
                new JoinDef { FromAlias = "h",   FromColumn = "BranchID",          ToTable = "tbl_Branch",          ToAlias = "br",  ToColumn = "ID",         JoinType = "LEFT" },
                // header → store
                new JoinDef { FromAlias = "h",   FromColumn = "StoreID",           ToTable = "tbl_Store",           ToAlias = "st",  ToColumn = "ID",         JoinType = "LEFT" },
                // header → currency
                new JoinDef { FromAlias = "h",   FromColumn = "CurrencyID",        ToTable = "tbl_Currency",        ToAlias = "cur", ToColumn = "ID",         JoinType = "LEFT" },
                // header → details  (one-to-many; drives the row granularity)
                new JoinDef { FromAlias = "h",   FromColumn = "Guid",              ToTable = "tbl_InvoiceDetails",  ToAlias = "d",   ToColumn = "HeaderGuid", JoinType = "LEFT" },
                // details → item master
                new JoinDef { FromAlias = "d",   FromColumn = "ItemGuid",          ToTable = "tbl_Items",           ToAlias = "itm", ToColumn = "Guid",       JoinType = "LEFT" },
                // item master → category
                new JoinDef { FromAlias = "itm", FromColumn = "CategoryID",        ToTable = "tbl_ItemsCategory",   ToAlias = "cat", ToColumn = "ID",         JoinType = "LEFT" },
            }
        };


        // =====================================================================
        // MODULE 2 — ITEMS / PRODUCTS
        // Primary  : tbl_Items  (alias: itm)
        // Joined   : tbl_ItemsCategory (cat)
        //            tbl_Tax (stax — sales tax)
        //            tbl_Tax (ptax — purchase tax)
        // =====================================================================
        private static ModuleDef BuildModule_Items() => new ModuleDef
        {
            Id = "items",
            Label = "Items / Products",
            Icon = "package",
            Color = "#16A34A",
            PrimaryTable = "tbl_Items",
            PrimaryAlias = "itm",
            PrimaryKey = "Guid",     // ← ADD THIS
            Fields = new List<FieldDef>
            {
                // ── Item identity ─────────────────────────────────────────────
                new FieldDef { Id = "item_name_ar",     Label = "Item Name (AR)",    Table = "tbl_Items",          Alias = "itm",  Column = "AName",              Type = "text"     },
                new FieldDef { Id = "item_name_en",     Label = "Item Name (EN)",    Table = "tbl_Items",          Alias = "itm",  Column = "EName",              Type = "text"     },
                new FieldDef { Id = "item_desc",        Label = "Description",       Table = "tbl_Items",          Alias = "itm",  Column = "Description",        Type = "text"     },
                new FieldDef { Id = "item_barcode",     Label = "Barcode",           Table = "tbl_Items",          Alias = "itm",  Column = "Barcode",            Type = "text"     },

                // ── Pricing ───────────────────────────────────────────────────
                new FieldDef { Id = "item_price_bt",    Label = "Sales Price (Before Tax)", Table = "tbl_Items", Alias = "itm", Column = "SalesPriceBeforeTax",  Type = "currency" },
                new FieldDef { Id = "item_price_at",    Label = "Sales Price (After Tax)",  Table = "tbl_Items", Alias = "itm", Column = "SalesPriceAfterTax",   Type = "currency" },
                new FieldDef { Id = "item_avg_cost",    Label = "Avg Cost / Unit",   Table = "tbl_Items",          Alias = "itm",  Column = "AVGCostPerUnit",     Type = "currency" },
                new FieldDef { Id = "item_min_limit",   Label = "Minimum Stock",     Table = "tbl_Items",          Alias = "itm",  Column = "MinimumLimit",       Type = "number"   },

                // ── Flags ─────────────────────────────────────────────────────
                new FieldDef { Id = "item_is_active",   Label = "Active",            Table = "tbl_Items",          Alias = "itm",  Column = "IsActive",           Type = "bool"     },
                new FieldDef { Id = "item_is_pos",      Label = "Show on POS",       Table = "tbl_Items",          Alias = "itm",  Column = "IsPOS",              Type = "bool"     },
                new FieldDef { Id = "item_is_stock",    Label = "Stock Item",        Table = "tbl_Items",          Alias = "itm",  Column = "IsStockItem",        Type = "bool"     },
                new FieldDef { Id = "item_track_lot",   Label = "Track Lot",         Table = "tbl_Items",          Alias = "itm",  Column = "TrackLot",           Type = "bool"     },
                new FieldDef { Id = "item_track_serial",Label = "Track Serial",      Table = "tbl_Items",          Alias = "itm",  Column = "TrackSerial",        Type = "bool"     },
                new FieldDef { Id = "item_track_expiry",Label = "Track Expiry Date", Table = "tbl_Items",          Alias = "itm",  Column = "TrackExpiryDate",    Type = "bool"     },

                // ── Category ──────────────────────────────────────────────────
                new FieldDef { Id = "item_category",    Label = "Category",          Table = "tbl_ItemsCategory",  Alias = "cat",  Column = "AName",              Type = "text"     },

                // ── Tax rates ─────────────────────────────────────────────────
                new FieldDef { Id = "sales_tax_name",   Label = "Sales Tax",         Table = "tbl_Tax",            Alias = "stax", Column = "AName",              Type = "text"     },
                new FieldDef { Id = "sales_tax_value",  Label = "Sales Tax %",       Table = "tbl_Tax",            Alias = "stax", Column = "Value",              Type = "number"   },
                new FieldDef { Id = "purch_tax_name",   Label = "Purchase Tax",      Table = "tbl_Tax",            Alias = "ptax", Column = "AName",              Type = "text"     },
                new FieldDef { Id = "purch_tax_value",  Label = "Purchase Tax %",    Table = "tbl_Tax",            Alias = "ptax", Column = "Value",              Type = "number"   },
            },

            Joins = new List<JoinDef>
            {
                // item → category
                new JoinDef { FromAlias = "itm", FromColumn = "CategoryID",      ToTable = "tbl_ItemsCategory", ToAlias = "cat",  ToColumn = "ID", JoinType = "LEFT" },
                // item → sales tax  (SalesTaxID)
                new JoinDef { FromAlias = "itm", FromColumn = "SalesTaxID",      ToTable = "tbl_Tax",           ToAlias = "stax", ToColumn = "ID", JoinType = "LEFT" },
                // item → purchase tax  (PurchaseTaxID) — separate alias to avoid ambiguity
                new JoinDef { FromAlias = "itm", FromColumn = "PurchaseTaxID",   ToTable = "tbl_Tax",           ToAlias = "ptax", ToColumn = "ID", JoinType = "LEFT" },
            }
        };


        // =====================================================================
        // MODULE 3 — BUSINESS PARTNERS  (Customers & Suppliers)
        // Primary  : tbl_BusinessPartner  (alias: bp)
        // No complex joins needed for this module
        // =====================================================================
        private static ModuleDef BuildModule_BusinessPartners() => new ModuleDef
        {
            Id = "business_partners",
            Label = "Business Partners",
            Icon = "users",
            Color = "#D97706",
            PrimaryTable = "tbl_BusinessPartner",
            PrimaryAlias = "bp",
            PrimaryKey = "ID",
            Fields = new List<FieldDef>
            {
                // ── Identity ──────────────────────────────────────────────────
                new FieldDef { Id = "bp_name_ar",       Label = "Name (AR)",         Table = "tbl_BusinessPartner", Alias = "bp", Column = "AName",             Type = "text" },
                new FieldDef { Id = "bp_name_en",       Label = "Name (EN)",         Table = "tbl_BusinessPartner", Alias = "bp", Column = "EName",             Type = "text" },
                new FieldDef { Id = "bp_commercial",    Label = "Commercial Name",   Table = "tbl_BusinessPartner", Alias = "bp", Column = "CommercialName",    Type = "text" },
                new FieldDef { Id = "bp_emp_code",      Label = "Employee Code",     Table = "tbl_BusinessPartner", Alias = "bp", Column = "EmpCode",           Type = "text" },

                // ── Contact ───────────────────────────────────────────────────
                new FieldDef { Id = "bp_address",       Label = "Address",           Table = "tbl_BusinessPartner", Alias = "bp", Column = "Address",           Type = "text" },
                new FieldDef { Id = "bp_street",        Label = "Street",            Table = "tbl_BusinessPartner", Alias = "bp", Column = "StreetName",        Type = "text" },
                new FieldDef { Id = "bp_house_no",      Label = "House No",          Table = "tbl_BusinessPartner", Alias = "bp", Column = "HouseNumber",       Type = "text" },
                new FieldDef { Id = "bp_tel",           Label = "Telephone",         Table = "tbl_BusinessPartner", Alias = "bp", Column = "Tel",               Type = "text" },
                new FieldDef { Id = "bp_email",         Label = "Email",             Table = "tbl_BusinessPartner", Alias = "bp", Column = "Email",             Type = "text" },

                // ── Legal / Financial ─────────────────────────────────────────
                new FieldDef { Id = "bp_national_no",   Label = "National No",       Table = "tbl_BusinessPartner", Alias = "bp", Column = "NationalNumber",    Type = "text" },
                new FieldDef { Id = "bp_passport",      Label = "Passport No",       Table = "tbl_BusinessPartner", Alias = "bp", Column = "PassportNumber",    Type = "text" },
                new FieldDef { Id = "bp_id_number",     Label = "ID Number",         Table = "tbl_BusinessPartner", Alias = "bp", Column = "IDNumber",          Type = "text" },
                new FieldDef { Id = "bp_tax_number",    Label = "Tax Number",        Table = "tbl_BusinessPartner", Alias = "bp", Column = "TaxNumber",         Type = "text" },
                new FieldDef { Id = "bp_job",           Label = "Job",               Table = "tbl_BusinessPartner", Alias = "bp", Column = "Job",               Type = "text" },
                new FieldDef { Id = "bp_credit_limit",  Label = "Credit Limit",      Table = "tbl_BusinessPartner", Alias = "bp", Column = "Limit",             Type = "currency" },
                new FieldDef { Id = "bp_active",        Label = "Active",            Table = "tbl_BusinessPartner", Alias = "bp", Column = "Active",            Type = "bool" },

                // ── Bank details ──────────────────────────────────────────────
                new FieldDef { Id = "bp_bank_name",     Label = "Bank Name",         Table = "tbl_BusinessPartner", Alias = "bp", Column = "BankName",          Type = "text" },
                new FieldDef { Id = "bp_bank_account",  Label = "Bank Account No",   Table = "tbl_BusinessPartner", Alias = "bp", Column = "BankAccountNumber", Type = "text" },
            },

            Joins = new List<JoinDef>()  // standalone — no joins required
        };


        // =====================================================================
        // MODULE 4 — POS SESSIONS
        // Primary  : tbl_POSSessions  (alias: s)
        // Joined   : Tbl_POSDay (pd) · tbl_POSSessionsType (stype)
        //            tbl_CashDrawer (cd) · tbl_Branch (br)
        // =====================================================================
        private static ModuleDef BuildModule_POSSessions() => new ModuleDef
        {
            Id = "pos_sessions",
            Label = "POS Sessions",
            Icon = "monitor",
            Color = "#7C3AED",
            PrimaryTable = "tbl_POSSessions",
            PrimaryAlias = "s",
            PrimaryKey = "Guid",     // ← ADD THIS
            Fields = new List<FieldDef>
            {
                // ── Session ───────────────────────────────────────────────────
                new FieldDef { Id = "session_type",     Label = "Session Type",      Table = "tbl_POSSessionsType",  Alias = "stype", Column = "AName",     Type = "text"     },
                new FieldDef { Id = "session_start",    Label = "Session Start",     Table = "tbl_POSSessions",      Alias = "s",     Column = "StartDate", Type = "datetime" },
                new FieldDef { Id = "session_end",      Label = "Session End",       Table = "tbl_POSSessions",      Alias = "s",     Column = "EndDate",   Type = "datetime" },
                new FieldDef { Id = "session_status",   Label = "Session Status",    Table = "tbl_POSSessions",      Alias = "s",     Column = "Status",    Type = "number"   },

                // ── POS Day ───────────────────────────────────────────────────
                new FieldDef { Id = "pos_date",         Label = "POS Date",          Table = "Tbl_POSDay",           Alias = "pd",    Column = "POSDate",   Type = "date"     },
                new FieldDef { Id = "pos_day_start",    Label = "Day Start",         Table = "Tbl_POSDay",           Alias = "pd",    Column = "StartDate", Type = "datetime" },
                new FieldDef { Id = "pos_day_end",      Label = "Day End",           Table = "Tbl_POSDay",           Alias = "pd",    Column = "EndDate",   Type = "datetime" },
                new FieldDef { Id = "pos_day_status",   Label = "Day Status",        Table = "Tbl_POSDay",           Alias = "pd",    Column = "Status",    Type = "number"   },

                // ── Cash drawer & branch ──────────────────────────────────────
                new FieldDef { Id = "cash_drawer_name", Label = "Cash Drawer",       Table = "tbl_CashDrawer",       Alias = "cd",    Column = "AName",     Type = "text"     },
                new FieldDef { Id = "branch_name",      Label = "Branch",            Table = "tbl_Branch",           Alias = "br",    Column = "AName",     Type = "text"     },
            },

            Joins = new List<JoinDef>
            {
                // session → pos day
                new JoinDef { FromAlias = "s",  FromColumn = "POSDayGuid",      ToTable = "Tbl_POSDay",          ToAlias = "pd",    ToColumn = "Guid", JoinType = "LEFT" },
                // session → session type
                new JoinDef { FromAlias = "s",  FromColumn = "SessionTypeID",   ToTable = "tbl_POSSessionsType", ToAlias = "stype", ToColumn = "ID",   JoinType = "LEFT" },
                // session → cash drawer
                new JoinDef { FromAlias = "s",  FromColumn = "CashDrawerID",    ToTable = "tbl_CashDrawer",      ToAlias = "cd",    ToColumn = "ID",   JoinType = "LEFT" },
                // cash drawer → branch
                new JoinDef { FromAlias = "cd", FromColumn = "BranchID",        ToTable = "tbl_Branch",          ToAlias = "br",    ToColumn = "ID",   JoinType = "LEFT" },
            }
        };


        // =====================================================================
        // MODELS  (shared with RunReport)
        // =====================================================================
        public class ModuleDef
        {
            public string Id { get; set; } = "";
            public string Label { get; set; } = "";
            public string Icon { get; set; } = "";
            public string Color { get; set; } = "";
            public string PrimaryTable { get; set; } = "";
            public string PrimaryAlias { get; set; } = "";
            public string PrimaryKey { get; set; } = "ID";   // ← NEW: "ID" or "Guid"
            public List<FieldDef> Fields { get; set; } = new();
            public List<JoinDef> Joins { get; set; } = new();
        }


        public class FieldDef
        {
            public string Id { get; set; } = "";
            public string Label { get; set; } = "";
            public string Table { get; set; } = "";
            public string Alias { get; set; } = "";
            public string Column { get; set; } = "";
            /// <summary>text | date | datetime | number | currency | bool | guid</summary>
            public string Type { get; set; } = "text";
        }

        public class JoinDef
        {
            public string FromAlias { get; set; } = "";
            public string FromColumn { get; set; } = "";
            public string ToTable { get; set; } = "";
            public string ToAlias { get; set; } = "";
            public string ToColumn { get; set; } = "";
            /// <summary>LEFT | INNER</summary>
            public string JoinType { get; set; } = "LEFT";
        }


        // =====================================================================
        // RUN REPORT  (unchanged engine from your original file)
        // =====================================================================
        public class RunReportFilter
        {
            public string fieldId { get; set; } = "";
            public string operatorName { get; set; } = "eq";
            public object? Value { get; set; }
            public object? Value2 { get; set; }
        }

        public class MeasureDto
        {
            public string FieldId { get; set; } = "";
            public string Fn { get; set; } = "sum"; // sum | count | countDistinct | avg | min | max
            public string? Alias { get; set; }
        }

        public void RunReport(
            int CompanyID,
            string ModuleId,
            List<string> FieldIds,
            List<RunReportFilter> Filters,
            string SortByFieldId,
            string SortDir,
            string GroupByFieldId,
            int Page,
            int PageSize,
            List<MeasureDto>? Agg,
            bool IncludeRowCount,
            ref DataTable dtRows,
            ref int totalRows
        )
        {
            dtRows = new DataTable();
            totalRows = 0;

            var catalog = BuildCatalog();
            if (!catalog.TryGetValue(ModuleId, out var module))
                throw new Exception($"Unknown ModuleId: {ModuleId}");

            var fieldMap = module.Fields.ToDictionary(f => f.Id, f => f, StringComparer.OrdinalIgnoreCase);

            if (FieldIds == null || FieldIds.Count == 0)
                FieldIds = module.Fields.Take(8).Select(f => f.Id).ToList();

            FieldIds = FieldIds
                .Where(id => fieldMap.ContainsKey(id))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();

            if (FieldIds.Count == 0)
                throw new Exception("No valid FieldIds provided.");

            var sbSelect = new StringBuilder();
            var sbFrom = new StringBuilder();
            var sbJoin = new StringBuilder();
            var sbWhere = new StringBuilder();
            var sbOrder = new StringBuilder();
            var parameters = new List<SqlParameter>();
            int pIndex = 0;

            sbFrom.AppendLine($"FROM {module.PrimaryTable} {module.PrimaryAlias}");

            foreach (var j in module.Joins)
                sbJoin.AppendLine($"{j.JoinType} JOIN {j.ToTable} {j.ToAlias} ON {j.FromAlias}.{j.FromColumn} = {j.ToAlias}.{j.ToColumn}");

            sbWhere.AppendLine("WHERE 1=1");
            sbWhere.AppendLine($"AND {module.PrimaryAlias}.CompanyID = @p_company");
            parameters.Add(new SqlParameter("@p_company", SqlDbType.Int) { Value = CompanyID });

            if (Filters != null)
                foreach (var f in Filters)
                {
                    if (f == null || string.IsNullOrWhiteSpace(f.fieldId)) continue;
                    if (!fieldMap.TryGetValue(f.fieldId, out var fd)) continue;
                    AppendFilter(sbWhere, parameters, ref pIndex, $"{fd.Alias}.{fd.Column}", fd.Type, f);
                }

            bool hasGroup = !string.IsNullOrWhiteSpace(GroupByFieldId) && fieldMap.ContainsKey(GroupByFieldId);

            if (hasGroup)
            {
                var gfd = fieldMap[GroupByFieldId];
                sbSelect.AppendLine("SELECT");
                sbSelect.AppendLine($"  {gfd.Alias}.{gfd.Column} AS [{gfd.Id}]");

                if (IncludeRowCount)
                    sbSelect.AppendLine(", COUNT(1) AS [group_count]");

                if (Agg != null)
                    foreach (var m in Agg)
                    {
                        if (m == null || !fieldMap.TryGetValue(m.FieldId ?? "", out var mfd)) continue;
                        string fn = (m.Fn ?? "sum").ToLowerInvariant();
                        string sqlFn = fn switch { "avg" => "AVG", "min" => "MIN", "max" => "MAX", "count" => "COUNT", "countdistinct" => "COUNT", _ => "SUM" };
                        string alias = m.FieldId!;
                        if (fn == "countdistinct")
                            sbSelect.AppendLine($", COUNT(DISTINCT {mfd.Alias}.{mfd.Column}) AS [{alias}]");
                        else
                            sbSelect.AppendLine($", {sqlFn}({mfd.Alias}.{mfd.Column}) AS [{alias}]");
                    }
            }
            else
            {
                sbSelect.AppendLine("SELECT");
                for (int i = 0; i < FieldIds.Count; i++)
                {
                    var fd = fieldMap[FieldIds[i]];
                    string cm = i == FieldIds.Count - 1 ? "" : ",";
                    sbSelect.AppendLine($"  {fd.Alias}.{fd.Column} AS [{fd.Id}]{cm}");
                }
            }

            string sortDir = SortDir?.ToLowerInvariant() == "desc" ? "DESC" : "ASC";
            if (hasGroup)
            {
                var gfd = fieldMap[GroupByFieldId];
                sbOrder.AppendLine($"ORDER BY {gfd.Alias}.{gfd.Column} {sortDir}");
            }
            else if (!string.IsNullOrWhiteSpace(SortByFieldId) && fieldMap.TryGetValue(SortByFieldId, out var sfd) && FieldIds.Contains(sfd.Id, StringComparer.OrdinalIgnoreCase))
            {
                sbOrder.AppendLine($"ORDER BY {sfd.Alias}.{sfd.Column} {sortDir}");
            }
            else
            {
             //   sbOrder.AppendLine($"ORDER BY {module.PrimaryAlias}.{(module.PrimaryAlias == "h" || module.PrimaryAlias == "s" ? "Guid" : "ID")} DESC");
                sbOrder.AppendLine($"ORDER BY {module.PrimaryAlias}.{module.PrimaryKey} DESC");





            }

            if (Page <= 0) Page = 1;
            if (PageSize <= 0) PageSize = 50;
            int offset = (Page - 1) * PageSize;

            var sqlData = new StringBuilder();
            sqlData.Append(sbSelect); sqlData.Append(sbFrom); sqlData.Append(sbJoin); sqlData.Append(sbWhere);
            if (hasGroup) { var gfd = fieldMap[GroupByFieldId]; sqlData.AppendLine($"GROUP BY {gfd.Alias}.{gfd.Column}"); }
            sqlData.Append(sbOrder);
            sqlData.AppendLine("OFFSET @p_offset ROWS FETCH NEXT @p_fetch ROWS ONLY;");
            parameters.Add(new SqlParameter("@p_offset", SqlDbType.Int) { Value = offset });
            parameters.Add(new SqlParameter("@p_fetch", SqlDbType.Int) { Value = PageSize });

            var sqlCount = new StringBuilder();
            if (hasGroup)
            {
                var gfd = fieldMap[GroupByFieldId];
                sqlCount.AppendLine("SELECT COUNT(1) FROM (");
                sqlCount.AppendLine($"  SELECT {gfd.Alias}.{gfd.Column}");
                sqlCount.Append(sbFrom); sqlCount.Append(sbJoin); sqlCount.Append(sbWhere);
                sqlCount.AppendLine($"  GROUP BY {gfd.Alias}.{gfd.Column}");
                sqlCount.AppendLine(") _cnt;");
            }
            else
            {
                sqlCount.AppendLine("SELECT COUNT(1)");
                sqlCount.Append(sbFrom); sqlCount.Append(sbJoin); sqlCount.Append(sbWhere);
            }

            dtRows = ExecuteDataTable(sqlData.ToString(), parameters, CompanyID);
            var cntPs = parameters
                .Where(p => p.ParameterName != "@p_offset" && p.ParameterName != "@p_fetch")
                .Select(p => new SqlParameter(p.ParameterName, p.SqlDbType) { Value = p.Value ?? DBNull.Value, Size = p.Size, Precision = p.Precision, Scale = p.Scale })
                .ToList();
            object? c = ExecuteScalar(sqlCount.ToString(), cntPs, CompanyID);
            totalRows = (c == null || c == DBNull.Value) ? 0 : Convert.ToInt32(c);
        }


        // =====================================================================
        // HELPERS  (filter builder, parameter factory, SQL execution)
        // =====================================================================
        private static void AppendFilter(StringBuilder sbWhere, List<SqlParameter> parameters, ref int pIndex, string expr, string type, RunReportFilter f)
        {
            string op = NormalizeOp(f.operatorName);

            if (op == "isnull") { sbWhere.AppendLine($"AND {expr} IS NULL"); return; }
            if (op == "notnull") { sbWhere.AppendLine($"AND {expr} IS NOT NULL"); return; }

            if (op == "in")
            {
                if (f.Value == null) return;
                var v = NormalizeJsonValue(f.Value);
                var items = new List<string>();
                if (v is string s)
                {
                    s = s.Trim();
                    if (s.StartsWith("["))
                        try { using var doc = JsonDocument.Parse(s); foreach (var el in doc.RootElement.EnumerateArray()) items.Add(el.ToString()); } catch { items.AddRange(s.Split(',').Select(x => x.Trim()).Where(x => x.Length > 0)); }
                    else
                        items.AddRange(s.Split(',').Select(x => x.Trim()).Where(x => x.Length > 0));
                }
                else items.Add(v?.ToString() ?? "");
                if (items.Count == 0) return;
                var inPs = new List<string>();
                foreach (var item in items) { string pn = "@p_" + pIndex++; inPs.Add(pn); parameters.Add(MakeParam(pn, type, item)); }
                sbWhere.AppendLine($"AND {expr} IN ({string.Join(",", inPs)})");
                return;
            }

            if (op == "between")
            {
                if (f.Value == null || f.Value2 == null) return;
                string p1 = "@p_" + pIndex++, p2 = "@p_" + pIndex++;
                parameters.Add(MakeParam(p1, type, f.Value));
                parameters.Add(MakeParam(p2, type, f.Value2));
                sbWhere.AppendLine($"AND {expr} BETWEEN {p1} AND {p2}");
                return;
            }

            if (op == "contains" || op == "starts" || op == "ends")
            {
                if (f.Value == null) return;
                string pn = "@p_" + pIndex++, raw = f.Value.ToString() ?? "";
                string like = op switch { "starts" => raw + "%", "ends" => "%" + raw, _ => "%" + raw + "%" };
                parameters.Add(new SqlParameter(pn, SqlDbType.NVarChar) { Value = like });
                sbWhere.AppendLine($"AND {expr} LIKE {pn}");
                return;
            }

            string sqlOp = op switch { "ne" => "<>", "gt" => ">", "gte" => ">=", "lt" => "<", "lte" => "<=", _ => "=" };
            if (f.Value == null)
            {
                sbWhere.AppendLine(sqlOp == "=" ? $"AND {expr} IS NULL" : $"AND {expr} IS NOT NULL");
                return;
            }
            string paramName = "@p_" + pIndex++;
            parameters.Add(MakeParam(paramName, type, f.Value));
            sbWhere.AppendLine($"AND {expr} {sqlOp} {paramName}");
        }

        private static SqlParameter MakeParam(string name, string type, object? value)
        {
            var v = NormalizeJsonValue(value);
            if (v == null) return new SqlParameter(name, SqlDbType.NVarChar) { Value = DBNull.Value };

            switch ((type ?? "").ToLowerInvariant())
            {
                case "number":
                case "currency":
                    if (v is decimal d) return new SqlParameter(name, SqlDbType.Decimal) { Value = d };
                    if (v is double db) return new SqlParameter(name, SqlDbType.Decimal) { Value = Convert.ToDecimal(db) };
                    if (v is long l) return new SqlParameter(name, SqlDbType.Decimal) { Value = Convert.ToDecimal(l) };
                    if (v is int i) return new SqlParameter(name, SqlDbType.Decimal) { Value = Convert.ToDecimal(i) };
                    if (decimal.TryParse(v.ToString(), out var dp)) return new SqlParameter(name, SqlDbType.Decimal) { Value = dp };
                    return new SqlParameter(name, SqlDbType.Decimal) { Value = 0m };

                case "bool":
                    if (v is bool b) return new SqlParameter(name, SqlDbType.Bit) { Value = b };
                    if (bool.TryParse(v.ToString(), out var bp)) return new SqlParameter(name, SqlDbType.Bit) { Value = bp };
                    if (int.TryParse(v.ToString(), out var bi)) return new SqlParameter(name, SqlDbType.Bit) { Value = bi != 0 };
                    return new SqlParameter(name, SqlDbType.Bit) { Value = false };

                case "date":
                case "datetime":
                    if (v is DateTime dt) return new SqlParameter(name, SqlDbType.DateTime) { Value = dt };
                    if (DateTime.TryParse(v.ToString(), out var dtp)) return new SqlParameter(name, SqlDbType.DateTime) { Value = dtp };
                    return new SqlParameter(name, SqlDbType.DateTime) { Value = DBNull.Value };

                case "guid":
                    if (v is Guid g) return new SqlParameter(name, SqlDbType.UniqueIdentifier) { Value = g };
                    if (Guid.TryParse(v.ToString(), out var gp)) return new SqlParameter(name, SqlDbType.UniqueIdentifier) { Value = gp };
                    return new SqlParameter(name, SqlDbType.UniqueIdentifier) { Value = DBNull.Value };

                default:
                    return new SqlParameter(name, SqlDbType.NVarChar) { Value = v.ToString() ?? "" };
            }
        }

        private static object? NormalizeJsonValue(object? value)
        {
            if (value is not JsonElement je) return value;
            return je.ValueKind switch
            {
                JsonValueKind.Null or JsonValueKind.Undefined => null,
                JsonValueKind.String => je.GetString(),
                JsonValueKind.True or JsonValueKind.False => je.GetBoolean(),
                JsonValueKind.Number => je.TryGetDecimal(out var d) ? d : je.TryGetInt64(out var l) ? l : je.GetDouble(),
                JsonValueKind.Array or JsonValueKind.Object => je.GetRawText(),
                _ => je.ToString()
            };
        }

        private static string NormalizeOp(string? op) =>
            (op ?? "eq").Trim().ToLowerInvariant() switch
            {
                "equals" => "eq",
                "notequals" => "ne",
                "contains" => "contains",
                "greaterthan" => "gt",
                "greaterthanorequal" => "gte",
                "lessthan" => "lt",
                "lessthanorequal" => "lte",
                "between" => "between",
                "inlist" => "in",
                "starts" => "starts",
                "ends" => "ends",
                "isnull" => "isnull",
                "notnull" => "notnull",
                var x => x   // pass-through for eq/ne/gt/…
            };

        private DataTable ExecuteDataTable(string sql, List<SqlParameter> prms, int CompanyID)
        {
            clsSQL clsSQL = new clsSQL();
            using var cn = new SqlConnection(clsSQL.CreateDataBaseConnectionString(CompanyID));
            using var cmd = new SqlCommand(sql, cn);
            cmd.Parameters.AddRange(prms.ToArray());
            using var da = new SqlDataAdapter(cmd);
            var dt = new DataTable();
            da.Fill(dt);
            return dt;
        }

        private object? ExecuteScalar(string sql, List<SqlParameter> prms, int CompanyID)
        {
            clsSQL clsSQL = new clsSQL();
            using var cn = new SqlConnection(clsSQL.CreateDataBaseConnectionString(CompanyID));
            using var cmd = new SqlCommand(sql, cn);
            cmd.Parameters.AddRange(prms.ToArray());
            cn.Open();
            return cmd.ExecuteScalar();
        }
    }
}