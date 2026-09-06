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
    /// Covers: Journal Vouchers · Sales/Purchase/POS Invoices · Inventory · Cash ·
    /// Items · Partners · POS Sessions · Employees · Contracts · Leave · Payroll ·
    /// Financing · Manufacturing · BOM · Work Centers.
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
            {
                BuildModule_JournalVouchers(),
                BuildModule_SalesInvoices(),
                BuildModule_PurchaseInvoices(),
                BuildModule_PurchaseOffers(),
                BuildModule_InventoryMovements(),
                BuildModule_POSSales(),
                BuildModule_CashVouchers(),
                BuildModule_Items(),
                BuildModule_BusinessPartners(),
                BuildModule_POSSessions(),
                BuildModule_Employees(),
                BuildModule_EmployeeContracts(),
                BuildModule_LeaveRequests(),
                BuildModule_PayrollRuns(),
                BuildModule_FinancingLoans(),
                BuildModule_ManufacturingOrders(),
                BuildModule_BOM(),
                BuildModule_WorkCenters(),
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
        };         private static ModuleDef BuildModule_SalesInvoices() => new ModuleDef
        {
            Id = "sales_invoices",
            Label = "Sales Invoices",
            Icon = "receipt",
            Color = "#2563EB",
            PrimaryTable = "tbl_InvoiceHeader",
            PrimaryAlias = "h",
            PrimaryKey = "Guid",
            FixedWhere = "h.InvoiceTypeID IN (3, 5)",
            Fields = new List<FieldDef>
            {
                // ── Invoice header ────────────────────────────────────────────
                new FieldDef { Id = "inv_no",           Label = "Invoice No",        Table = "tbl_InvoiceHeader",    Alias = "h",   Column = "InvoiceNo",       Type = "number"   },
                new FieldDef { Id = "inv_date",         Label = "Invoice Date",      Table = "tbl_InvoiceHeader",    Alias = "h",   Column = "InvoiceDate",     Type = "date"     },
                new FieldDef { Id = "inv_type_id",      Label = "Doc Type Id",       Table = "tbl_InvoiceHeader",    Alias = "h",   Column = "InvoiceTypeID",   Type = "number"   },
                new FieldDef { Id = "inv_type",         Label = "Doc Type",          Table = "tbl_JournalVoucherTypes", Alias = "jvt", Column = "AName",       Type = "text"     },
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
                new JoinDef { FromAlias = "h",   FromColumn = "BusinessPartnerID", ToTable = "tbl_BusinessPartner", ToAlias = "bp",  ToColumn = "ID",         JoinType = "LEFT" },
                new JoinDef { FromAlias = "h",   FromColumn = "BranchID",          ToTable = "tbl_Branch",          ToAlias = "br",  ToColumn = "ID",         JoinType = "LEFT" },
                new JoinDef { FromAlias = "h",   FromColumn = "StoreID",           ToTable = "tbl_Store",           ToAlias = "st",  ToColumn = "ID",         JoinType = "LEFT" },
                new JoinDef { FromAlias = "h",   FromColumn = "CurrencyID",        ToTable = "tbl_Currency",        ToAlias = "cur", ToColumn = "ID",         JoinType = "LEFT" },
                new JoinDef { FromAlias = "h",   FromColumn = "InvoiceTypeID",     ToTable = "tbl_JournalVoucherTypes", ToAlias = "jvt", ToColumn = "ID",    JoinType = "LEFT" },
                new JoinDef { FromAlias = "h",   FromColumn = "Guid",              ToTable = "tbl_InvoiceDetails",  ToAlias = "d",   ToColumn = "HeaderGuid", JoinType = "LEFT" },
                new JoinDef { FromAlias = "d",   FromColumn = "ItemGuid",          ToTable = "tbl_Items",           ToAlias = "itm", ToColumn = "Guid",       JoinType = "LEFT" },
                new JoinDef { FromAlias = "itm", FromColumn = "CategoryID",        ToTable = "tbl_ItemsCategory",   ToAlias = "cat", ToColumn = "ID",         JoinType = "LEFT" },
            }
        };


        // =====================================================================
        // MODULE — PURCHASE / AP INVOICES
        // =====================================================================
        private static ModuleDef BuildModule_PurchaseInvoices() => new ModuleDef
        {
            Id = "purchase_invoices",
            Label = "Purchase Invoices",
            Icon = "shopping-cart",
            Color = "#B45309",
            PrimaryTable = "tbl_InvoiceHeader",
            PrimaryAlias = "h",
            PrimaryKey = "Guid",
            FixedWhere = "h.InvoiceTypeID IN (2, 7, 22)",
            Fields = new List<FieldDef>
            {
                new FieldDef { Id = "inv_no",           Label = "Invoice No",        Table = "tbl_InvoiceHeader", Alias = "h",   Column = "InvoiceNo",      Type = "number"   },
                new FieldDef { Id = "inv_date",         Label = "Invoice Date",      Table = "tbl_InvoiceHeader", Alias = "h",   Column = "InvoiceDate",    Type = "date"     },
                new FieldDef { Id = "inv_type_id",      Label = "Doc Type Id",       Table = "tbl_InvoiceHeader", Alias = "h",   Column = "InvoiceTypeID",  Type = "number"   },
                new FieldDef { Id = "inv_type",         Label = "Doc Type",          Table = "tbl_JournalVoucherTypes", Alias = "jvt", Column = "AName",    Type = "text"     },
                new FieldDef { Id = "inv_ref",          Label = "Reference No",      Table = "tbl_InvoiceHeader", Alias = "h",   Column = "RefNo",          Type = "text"     },
                new FieldDef { Id = "inv_note",         Label = "Note",              Table = "tbl_InvoiceHeader", Alias = "h",   Column = "Note",           Type = "text"     },
                new FieldDef { Id = "inv_total_tax",    Label = "Total Tax",         Table = "tbl_InvoiceHeader", Alias = "h",   Column = "TotalTax",       Type = "currency" },
                new FieldDef { Id = "inv_hdr_discount", Label = "Header Discount",   Table = "tbl_InvoiceHeader", Alias = "h",   Column = "HeaderDiscount", Type = "currency" },
                new FieldDef { Id = "inv_total_disc",   Label = "Total Discount",    Table = "tbl_InvoiceHeader", Alias = "h",   Column = "TotalDiscount",  Type = "currency" },
                new FieldDef { Id = "inv_total",        Label = "Invoice Total",     Table = "tbl_InvoiceHeader", Alias = "h",   Column = "TotalInvoice",   Type = "currency" },
                new FieldDef { Id = "inv_currency_rate",Label = "Currency Rate",     Table = "tbl_InvoiceHeader", Alias = "h",   Column = "CurrencyRate",   Type = "number"   },
                new FieldDef { Id = "inv_base_amount",  Label = "Base Amount",       Table = "tbl_InvoiceHeader", Alias = "h",   Column = "CurrencyBaseAmount", Type = "currency" },
                new FieldDef { Id = "inv_is_posted",    Label = "Is Posted",         Table = "tbl_InvoiceHeader", Alias = "h",   Column = "IsPosted",       Type = "bool"     },
                new FieldDef { Id = "inv_doc_status",   Label = "Document Status",   Table = "tbl_InvoiceHeader", Alias = "h",   Column = "DocumentStatus", Type = "number"   },
                new FieldDef { Id = "bp_name",          Label = "Supplier",          Table = "tbl_BusinessPartner", Alias = "bp", Column = "AName",         Type = "text"     },
                new FieldDef { Id = "bp_commercial",    Label = "Commercial Name",   Table = "tbl_BusinessPartner", Alias = "bp", Column = "CommercialName", Type = "text"    },
                new FieldDef { Id = "bp_tel",           Label = "Partner Tel",       Table = "tbl_BusinessPartner", Alias = "bp", Column = "Tel",            Type = "text"     },
                new FieldDef { Id = "pay_method",       Label = "Payment Method",    Table = "tbl_PaymentMethod", Alias = "pm",  Column = "AName",          Type = "text"     },
                new FieldDef { Id = "branch_name",      Label = "Branch",            Table = "tbl_Branch",        Alias = "br",  Column = "AName",          Type = "text"     },
                new FieldDef { Id = "store_name",       Label = "Store",             Table = "tbl_Store",         Alias = "st",  Column = "AName",          Type = "text"     },
                new FieldDef { Id = "currency_name",    Label = "Currency",          Table = "tbl_Currency",      Alias = "cur", Column = "EName",          Type = "text"     },
                new FieldDef { Id = "line_item_name",   Label = "Item Name",         Table = "tbl_InvoiceDetails", Alias = "d",  Column = "ItemName",       Type = "text"     },
                new FieldDef { Id = "line_qty",         Label = "Quantity",          Table = "tbl_InvoiceDetails", Alias = "d",  Column = "Qty",            Type = "number"   },
                new FieldDef { Id = "line_free_qty",    Label = "Free Qty",          Table = "tbl_InvoiceDetails", Alias = "d",  Column = "FreeQty",        Type = "number"   },
                new FieldDef { Id = "line_total_qty",   Label = "Total Qty",         Table = "tbl_InvoiceDetails", Alias = "d",  Column = "TotalQTY",       Type = "number"   },
                new FieldDef { Id = "line_price_bt",    Label = "Price (Before Tax)",Table = "tbl_InvoiceDetails", Alias = "d",  Column = "PriceBeforeTax", Type = "currency" },
                new FieldDef { Id = "line_tax_pct",     Label = "Tax %",             Table = "tbl_InvoiceDetails", Alias = "d",  Column = "TaxPercentage",  Type = "number"   },
                new FieldDef { Id = "line_tax_amt",     Label = "Tax Amount",        Table = "tbl_InvoiceDetails", Alias = "d",  Column = "TaxAmount",      Type = "currency" },
                new FieldDef { Id = "line_total",       Label = "Line Total",        Table = "tbl_InvoiceDetails", Alias = "d",  Column = "TotalLine",      Type = "currency" },
                new FieldDef { Id = "line_avg_cost",    Label = "Avg Cost / Unit",   Table = "tbl_InvoiceDetails", Alias = "d",  Column = "AVGCostPerUnit", Type = "currency" },
                new FieldDef { Id = "item_barcode",     Label = "Barcode",           Table = "tbl_Items",         Alias = "itm", Column = "Barcode",        Type = "text"     },
                new FieldDef { Id = "item_category",    Label = "Category",          Table = "tbl_ItemsCategory", Alias = "cat", Column = "AName",          Type = "text"     },
            },
            Joins = new List<JoinDef>
            {
                new JoinDef { FromAlias = "h",   FromColumn = "BusinessPartnerID", ToTable = "tbl_BusinessPartner", ToAlias = "bp",  ToColumn = "ID",         JoinType = "LEFT" },
                new JoinDef { FromAlias = "h",   FromColumn = "BranchID",          ToTable = "tbl_Branch",          ToAlias = "br",  ToColumn = "ID",         JoinType = "LEFT" },
                new JoinDef { FromAlias = "h",   FromColumn = "StoreID",           ToTable = "tbl_Store",           ToAlias = "st",  ToColumn = "ID",         JoinType = "LEFT" },
                new JoinDef { FromAlias = "h",   FromColumn = "CurrencyID",        ToTable = "tbl_Currency",        ToAlias = "cur", ToColumn = "ID",         JoinType = "LEFT" },
                new JoinDef { FromAlias = "h",   FromColumn = "PaymentMethodID",   ToTable = "tbl_PaymentMethod",   ToAlias = "pm",  ToColumn = "ID",         JoinType = "LEFT" },
                new JoinDef { FromAlias = "h",   FromColumn = "InvoiceTypeID",     ToTable = "tbl_JournalVoucherTypes", ToAlias = "jvt", ToColumn = "ID",    JoinType = "LEFT" },
                new JoinDef { FromAlias = "h",   FromColumn = "Guid",              ToTable = "tbl_InvoiceDetails",  ToAlias = "d",   ToColumn = "HeaderGuid", JoinType = "LEFT" },
                new JoinDef { FromAlias = "d",   FromColumn = "ItemGuid",          ToTable = "tbl_Items",           ToAlias = "itm", ToColumn = "Guid",       JoinType = "LEFT" },
                new JoinDef { FromAlias = "itm", FromColumn = "CategoryID",        ToTable = "tbl_ItemsCategory",   ToAlias = "cat", ToColumn = "ID",         JoinType = "LEFT" },
            }
        };


        // =====================================================================
        // MODULE — PURCHASE OFFERS / ORDERS
        // =====================================================================
        private static ModuleDef BuildModule_PurchaseOffers() => new ModuleDef
        {
            Id = "purchase_offers",
            Label = "Purchase Offers / Orders",
            Icon = "file-plus",
            Color = "#C2410C",
            PrimaryTable = "tbl_InvoiceHeader",
            PrimaryAlias = "h",
            PrimaryKey = "Guid",
            FixedWhere = "h.InvoiceTypeID = 6",
            Fields = new List<FieldDef>
            {
                new FieldDef { Id = "inv_no",         Label = "Offer No",         Table = "tbl_InvoiceHeader", Alias = "h",   Column = "InvoiceNo",    Type = "number"   },
                new FieldDef { Id = "inv_date",       Label = "Offer Date",       Table = "tbl_InvoiceHeader", Alias = "h",   Column = "InvoiceDate",  Type = "date"     },
                new FieldDef { Id = "inv_ref",        Label = "Reference No",     Table = "tbl_InvoiceHeader", Alias = "h",   Column = "RefNo",        Type = "text"     },
                new FieldDef { Id = "inv_note",       Label = "Note",             Table = "tbl_InvoiceHeader", Alias = "h",   Column = "Note",         Type = "text"     },
                new FieldDef { Id = "inv_total",      Label = "Offer Total",      Table = "tbl_InvoiceHeader", Alias = "h",   Column = "TotalInvoice", Type = "currency" },
                new FieldDef { Id = "inv_total_tax",  Label = "Total Tax",        Table = "tbl_InvoiceHeader", Alias = "h",   Column = "TotalTax",     Type = "currency" },
                new FieldDef { Id = "inv_is_posted",  Label = "Is Posted",        Table = "tbl_InvoiceHeader", Alias = "h",   Column = "IsPosted",     Type = "bool"     },
                new FieldDef { Id = "bp_name",        Label = "Supplier",         Table = "tbl_BusinessPartner", Alias = "bp", Column = "AName",       Type = "text"     },
                new FieldDef { Id = "branch_name",    Label = "Branch",           Table = "tbl_Branch",        Alias = "br",  Column = "AName",        Type = "text"     },
                new FieldDef { Id = "store_name",     Label = "Store",            Table = "tbl_Store",         Alias = "st",  Column = "AName",        Type = "text"     },
                new FieldDef { Id = "line_item_name", Label = "Item Name",        Table = "tbl_InvoiceDetails", Alias = "d",  Column = "ItemName",     Type = "text"     },
                new FieldDef { Id = "line_qty",       Label = "Quantity",         Table = "tbl_InvoiceDetails", Alias = "d",  Column = "Qty",          Type = "number"   },
                new FieldDef { Id = "line_price_bt",  Label = "Price (Before Tax)", Table = "tbl_InvoiceDetails", Alias = "d", Column = "PriceBeforeTax", Type = "currency" },
                new FieldDef { Id = "line_total",     Label = "Line Total",       Table = "tbl_InvoiceDetails", Alias = "d",  Column = "TotalLine",    Type = "currency" },
                new FieldDef { Id = "item_barcode",   Label = "Barcode",          Table = "tbl_Items",         Alias = "itm", Column = "Barcode",      Type = "text"     },
                new FieldDef { Id = "item_category",  Label = "Category",         Table = "tbl_ItemsCategory", Alias = "cat", Column = "AName",        Type = "text"     },
            },
            Joins = new List<JoinDef>
            {
                new JoinDef { FromAlias = "h",   FromColumn = "BusinessPartnerID", ToTable = "tbl_BusinessPartner", ToAlias = "bp",  ToColumn = "ID",         JoinType = "LEFT" },
                new JoinDef { FromAlias = "h",   FromColumn = "BranchID",          ToTable = "tbl_Branch",          ToAlias = "br",  ToColumn = "ID",         JoinType = "LEFT" },
                new JoinDef { FromAlias = "h",   FromColumn = "StoreID",           ToTable = "tbl_Store",           ToAlias = "st",  ToColumn = "ID",         JoinType = "LEFT" },
                new JoinDef { FromAlias = "h",   FromColumn = "Guid",              ToTable = "tbl_InvoiceDetails",  ToAlias = "d",   ToColumn = "HeaderGuid", JoinType = "LEFT" },
                new JoinDef { FromAlias = "d",   FromColumn = "ItemGuid",          ToTable = "tbl_Items",           ToAlias = "itm", ToColumn = "Guid",       JoinType = "LEFT" },
                new JoinDef { FromAlias = "itm", FromColumn = "CategoryID",        ToTable = "tbl_ItemsCategory",   ToAlias = "cat", ToColumn = "ID",         JoinType = "LEFT" },
            }
        };


        // =====================================================================
        // MODULE — INVENTORY MOVEMENTS
        // =====================================================================
        private static ModuleDef BuildModule_InventoryMovements() => new ModuleDef
        {
            Id = "inventory_movements",
            Label = "Inventory Movements",
            Icon = "truck",
            Color = "#0F766E",
            PrimaryTable = "tbl_InvoiceHeader",
            PrimaryAlias = "h",
            PrimaryKey = "Guid",
            FixedWhere = "h.InvoiceTypeID IN (8, 9)",
            Fields = new List<FieldDef>
            {
                new FieldDef { Id = "inv_no",         Label = "Doc No",            Table = "tbl_InvoiceHeader", Alias = "h",   Column = "InvoiceNo",     Type = "number"   },
                new FieldDef { Id = "inv_date",       Label = "Movement Date",     Table = "tbl_InvoiceHeader", Alias = "h",   Column = "InvoiceDate",   Type = "date"     },
                new FieldDef { Id = "inv_type_id",    Label = "Movement Type Id",  Table = "tbl_InvoiceHeader", Alias = "h",   Column = "InvoiceTypeID", Type = "number"   },
                new FieldDef { Id = "inv_type",       Label = "Movement Type",     Table = "tbl_JournalVoucherTypes", Alias = "jvt", Column = "AName",   Type = "text"     },
                new FieldDef { Id = "inv_ref",        Label = "Reference",         Table = "tbl_InvoiceHeader", Alias = "h",   Column = "RefNo",         Type = "text"     },
                new FieldDef { Id = "inv_note",       Label = "Note",              Table = "tbl_InvoiceHeader", Alias = "h",   Column = "Note",          Type = "text"     },
                new FieldDef { Id = "inv_is_counted", Label = "Is Counted",        Table = "tbl_InvoiceHeader", Alias = "h",   Column = "IsCounted",     Type = "bool"     },
                new FieldDef { Id = "inv_is_posted",  Label = "Is Posted",         Table = "tbl_InvoiceHeader", Alias = "h",   Column = "IsPosted",      Type = "bool"     },
                new FieldDef { Id = "branch_name",    Label = "Branch",            Table = "tbl_Branch",        Alias = "br",  Column = "AName",         Type = "text"     },
                new FieldDef { Id = "store_name",     Label = "Store",             Table = "tbl_Store",         Alias = "st",  Column = "AName",         Type = "text"     },
                new FieldDef { Id = "line_item_name", Label = "Item Name",         Table = "tbl_InvoiceDetails", Alias = "d",  Column = "ItemName",      Type = "text"     },
                new FieldDef { Id = "line_qty",       Label = "Quantity",          Table = "tbl_InvoiceDetails", Alias = "d",  Column = "Qty",           Type = "number"   },
                new FieldDef { Id = "line_total_qty", Label = "Total Qty",         Table = "tbl_InvoiceDetails", Alias = "d",  Column = "TotalQTY",      Type = "number"   },
                new FieldDef { Id = "line_avg_cost",  Label = "Avg Cost / Unit",   Table = "tbl_InvoiceDetails", Alias = "d",  Column = "AVGCostPerUnit", Type = "currency" },
                new FieldDef { Id = "line_price_bt",  Label = "Unit Cost",         Table = "tbl_InvoiceDetails", Alias = "d",  Column = "PriceBeforeTax", Type = "currency" },
                new FieldDef { Id = "line_total",     Label = "Line Total",        Table = "tbl_InvoiceDetails", Alias = "d",  Column = "TotalLine",     Type = "currency" },
                new FieldDef { Id = "item_barcode",   Label = "Barcode",           Table = "tbl_Items",         Alias = "itm", Column = "Barcode",       Type = "text"     },
                new FieldDef { Id = "item_name_ar",   Label = "Item Name (AR)",    Table = "tbl_Items",         Alias = "itm", Column = "AName",         Type = "text"     },
                new FieldDef { Id = "item_category",  Label = "Category",          Table = "tbl_ItemsCategory", Alias = "cat", Column = "AName",         Type = "text"     },
            },
            Joins = new List<JoinDef>
            {
                new JoinDef { FromAlias = "h",   FromColumn = "BranchID",      ToTable = "tbl_Branch",               ToAlias = "br",  ToColumn = "ID",         JoinType = "LEFT" },
                new JoinDef { FromAlias = "h",   FromColumn = "StoreID",       ToTable = "tbl_Store",                ToAlias = "st",  ToColumn = "ID",         JoinType = "LEFT" },
                new JoinDef { FromAlias = "h",   FromColumn = "InvoiceTypeID", ToTable = "tbl_JournalVoucherTypes",  ToAlias = "jvt", ToColumn = "ID",         JoinType = "LEFT" },
                new JoinDef { FromAlias = "h",   FromColumn = "Guid",          ToTable = "tbl_InvoiceDetails",       ToAlias = "d",   ToColumn = "HeaderGuid", JoinType = "LEFT" },
                new JoinDef { FromAlias = "d",   FromColumn = "ItemGuid",      ToTable = "tbl_Items",                ToAlias = "itm", ToColumn = "Guid",       JoinType = "LEFT" },
                new JoinDef { FromAlias = "itm", FromColumn = "CategoryID",    ToTable = "tbl_ItemsCategory",        ToAlias = "cat", ToColumn = "ID",         JoinType = "LEFT" },
            }
        };


        // =====================================================================
        // MODULE — POS SALES
        // =====================================================================
        private static ModuleDef BuildModule_POSSales() => new ModuleDef
        {
            Id = "pos_sales",
            Label = "POS Sales",
            Icon = "shopping-bag",
            Color = "#7C3AED",
            PrimaryTable = "tbl_InvoiceHeader",
            PrimaryAlias = "h",
            PrimaryKey = "Guid",
            FixedWhere = "h.InvoiceTypeID IN (10, 11)",
            Fields = new List<FieldDef>
            {
                new FieldDef { Id = "inv_no",         Label = "Invoice No",      Table = "tbl_InvoiceHeader", Alias = "h",   Column = "InvoiceNo",     Type = "number"   },
                new FieldDef { Id = "inv_date",       Label = "Invoice Date",    Table = "tbl_InvoiceHeader", Alias = "h",   Column = "InvoiceDate",   Type = "date"     },
                new FieldDef { Id = "inv_type_id",    Label = "Doc Type Id",     Table = "tbl_InvoiceHeader", Alias = "h",   Column = "InvoiceTypeID", Type = "number"   },
                new FieldDef { Id = "inv_type",       Label = "Doc Type",        Table = "tbl_JournalVoucherTypes", Alias = "jvt", Column = "AName",   Type = "text"     },
                new FieldDef { Id = "inv_total",      Label = "Invoice Total",   Table = "tbl_InvoiceHeader", Alias = "h",   Column = "TotalInvoice",  Type = "currency" },
                new FieldDef { Id = "inv_total_tax",  Label = "Total Tax",       Table = "tbl_InvoiceHeader", Alias = "h",   Column = "TotalTax",      Type = "currency" },
                new FieldDef { Id = "inv_total_disc", Label = "Total Discount",  Table = "tbl_InvoiceHeader", Alias = "h",   Column = "TotalDiscount", Type = "currency" },
                new FieldDef { Id = "inv_is_posted",  Label = "Is Posted",       Table = "tbl_InvoiceHeader", Alias = "h",   Column = "IsPosted",      Type = "bool"     },
                new FieldDef { Id = "bp_name",        Label = "Customer",        Table = "tbl_BusinessPartner", Alias = "bp", Column = "AName",       Type = "text"     },
                new FieldDef { Id = "branch_name",    Label = "Branch",          Table = "tbl_Branch",        Alias = "br",  Column = "AName",         Type = "text"     },
                new FieldDef { Id = "store_name",     Label = "Store",           Table = "tbl_Store",         Alias = "st",  Column = "AName",         Type = "text"     },
                new FieldDef { Id = "cash_drawer",    Label = "Cash Drawer",     Table = "tbl_CashDrawer",    Alias = "cd",  Column = "AName",         Type = "text"     },
                new FieldDef { Id = "pay_method",     Label = "Payment Method",  Table = "tbl_PaymentMethod", Alias = "pm",  Column = "AName",         Type = "text"     },
                new FieldDef { Id = "session_start",  Label = "Session Start",   Table = "tbl_POSSessions",   Alias = "s",   Column = "StartDate",     Type = "datetime" },
                new FieldDef { Id = "pos_date",       Label = "POS Date",        Table = "Tbl_POSDay",        Alias = "pd",  Column = "POSDate",       Type = "date"     },
                new FieldDef { Id = "cashier_name",   Label = "Cashier",         Table = "tbl_employee",      Alias = "e",   Column = "AName",         Type = "text"     },
                new FieldDef { Id = "line_item_name", Label = "Item Name",       Table = "tbl_InvoiceDetails", Alias = "d",  Column = "ItemName",      Type = "text"     },
                new FieldDef { Id = "line_qty",       Label = "Quantity",        Table = "tbl_InvoiceDetails", Alias = "d",  Column = "Qty",           Type = "number"   },
                new FieldDef { Id = "line_price_at",  Label = "Price (After Tax)", Table = "tbl_InvoiceDetails", Alias = "d", Column = "PriceAfterTaxPcs", Type = "currency" },
                new FieldDef { Id = "line_total",     Label = "Line Total",      Table = "tbl_InvoiceDetails", Alias = "d",  Column = "TotalLine",     Type = "currency" },
                new FieldDef { Id = "item_barcode",   Label = "Barcode",         Table = "tbl_Items",         Alias = "itm", Column = "Barcode",       Type = "text"     },
                new FieldDef { Id = "item_category",  Label = "Category",        Table = "tbl_ItemsCategory", Alias = "cat", Column = "AName",         Type = "text"     },
            },
            Joins = new List<JoinDef>
            {
                new JoinDef { FromAlias = "h",   FromColumn = "BusinessPartnerID", ToTable = "tbl_BusinessPartner", ToAlias = "bp",  ToColumn = "ID",         JoinType = "LEFT" },
                new JoinDef { FromAlias = "h",   FromColumn = "BranchID",          ToTable = "tbl_Branch",          ToAlias = "br",  ToColumn = "ID",         JoinType = "LEFT" },
                new JoinDef { FromAlias = "h",   FromColumn = "StoreID",           ToTable = "tbl_Store",           ToAlias = "st",  ToColumn = "ID",         JoinType = "LEFT" },
                new JoinDef { FromAlias = "h",   FromColumn = "CashID",            ToTable = "tbl_CashDrawer",      ToAlias = "cd",  ToColumn = "ID",         JoinType = "LEFT" },
                new JoinDef { FromAlias = "h",   FromColumn = "PaymentMethodID",   ToTable = "tbl_PaymentMethod",   ToAlias = "pm",  ToColumn = "ID",         JoinType = "LEFT" },
                new JoinDef { FromAlias = "h",   FromColumn = "InvoiceTypeID",     ToTable = "tbl_JournalVoucherTypes", ToAlias = "jvt", ToColumn = "ID",    JoinType = "LEFT" },
                new JoinDef { FromAlias = "h",   FromColumn = "POSSessionGuid",    ToTable = "tbl_POSSessions",     ToAlias = "s",   ToColumn = "Guid",       JoinType = "LEFT" },
                new JoinDef { FromAlias = "s",   FromColumn = "POSDayGuid",        ToTable = "Tbl_POSDay",          ToAlias = "pd",  ToColumn = "Guid",       JoinType = "LEFT" },
                new JoinDef { FromAlias = "h",   FromColumn = "CreationUserID",    ToTable = "tbl_employee",        ToAlias = "e",   ToColumn = "ID",         JoinType = "LEFT" },
                new JoinDef { FromAlias = "h",   FromColumn = "Guid",              ToTable = "tbl_InvoiceDetails",  ToAlias = "d",   ToColumn = "HeaderGuid", JoinType = "LEFT" },
                new JoinDef { FromAlias = "d",   FromColumn = "ItemGuid",          ToTable = "tbl_Items",           ToAlias = "itm", ToColumn = "Guid",       JoinType = "LEFT" },
                new JoinDef { FromAlias = "itm", FromColumn = "CategoryID",        ToTable = "tbl_ItemsCategory",   ToAlias = "cat", ToColumn = "ID",         JoinType = "LEFT" },
            }
        };


        // =====================================================================
        // MODULE — CASH VOUCHERS
        // =====================================================================
        private static ModuleDef BuildModule_CashVouchers() => new ModuleDef
        {
            Id = "cash_vouchers",
            Label = "Cash Payments / Receipts",
            Icon = "wallet",
            Color = "#059669",
            PrimaryTable = "tbl_CashVoucherHeader",
            PrimaryAlias = "h",
            PrimaryKey = "Guid",
            Fields = new List<FieldDef>
            {
                new FieldDef { Id = "voucher_no",      Label = "Voucher No",       Table = "tbl_CashVoucherHeader", Alias = "h",   Column = "VoucherNo",   Type = "number"   },
                new FieldDef { Id = "manual_no",       Label = "Manual No",        Table = "tbl_CashVoucherHeader", Alias = "h",   Column = "ManualNo",    Type = "text"     },
                new FieldDef { Id = "voucher_date",    Label = "Voucher Date",     Table = "tbl_CashVoucherHeader", Alias = "h",   Column = "VoucherDate", Type = "date"     },
                new FieldDef { Id = "due_date",        Label = "Due Date",         Table = "tbl_CashVoucherHeader", Alias = "h",   Column = "DueDate",     Type = "date"     },
                new FieldDef { Id = "amount",          Label = "Amount",           Table = "tbl_CashVoucherHeader", Alias = "h",   Column = "Amount",      Type = "currency" },
                new FieldDef { Id = "note",            Label = "Note",             Table = "tbl_CashVoucherHeader", Alias = "h",   Column = "Note",        Type = "text"     },
                new FieldDef { Id = "cheque_name",     Label = "Cheque Name",      Table = "tbl_CashVoucherHeader", Alias = "h",   Column = "ChequeName",  Type = "text"     },
                new FieldDef { Id = "voucher_type_id", Label = "Voucher Type Id",  Table = "tbl_CashVoucherHeader", Alias = "h",   Column = "VoucherType", Type = "number"   },
                new FieldDef { Id = "voucher_type",    Label = "Voucher Type",     Table = "tbl_JournalVoucherTypes", Alias = "jvt", Column = "AName",     Type = "text"     },
                new FieldDef { Id = "pay_method",      Label = "Payment Method",   Table = "tbl_PaymentMethod",     Alias = "pm",  Column = "AName",       Type = "text"     },
                new FieldDef { Id = "branch_name",     Label = "Branch",           Table = "tbl_Branch",            Alias = "br",  Column = "AName",       Type = "text"     },
                new FieldDef { Id = "cc_name",         Label = "Cost Center",      Table = "tbl_CostCenter",        Alias = "cc",  Column = "AName",       Type = "text"     },
                new FieldDef { Id = "cash_drawer",     Label = "Cash Drawer",      Table = "tbl_CashDrawer",        Alias = "cd",  Column = "AName",       Type = "text"     },
                new FieldDef { Id = "hdr_account",     Label = "Header Account",   Table = "tbl_Accounts",          Alias = "acc", Column = "AName",       Type = "text"     },
                new FieldDef { Id = "doc_status",      Label = "Document Status",  Table = "tbl_CashVoucherHeader", Alias = "h",   Column = "DocumentStatus", Type = "number" },
                new FieldDef { Id = "row_index",       Label = "Row",              Table = "tbl_CashVoucherDetails", Alias = "d",  Column = "RowIndex",    Type = "number"   },
                new FieldDef { Id = "debit",           Label = "Debit",            Table = "tbl_CashVoucherDetails", Alias = "d",  Column = "Debit",       Type = "currency" },
                new FieldDef { Id = "credit",          Label = "Credit",           Table = "tbl_CashVoucherDetails", Alias = "d",  Column = "Credit",      Type = "currency" },
                new FieldDef { Id = "total",           Label = "Line Total",       Table = "tbl_CashVoucherDetails", Alias = "d",  Column = "Total",       Type = "currency" },
                new FieldDef { Id = "detail_note",     Label = "Detail Note",      Table = "tbl_CashVoucherDetails", Alias = "d",  Column = "Note",        Type = "text"     },
                new FieldDef { Id = "dtl_account",     Label = "Detail Account",   Table = "tbl_Accounts",          Alias = "dacc", Column = "AName",      Type = "text"     },
                new FieldDef { Id = "dtl_account_no",  Label = "Account Number",   Table = "tbl_Accounts",          Alias = "dacc", Column = "AccountNumber", Type = "text"  },
            },
            Joins = new List<JoinDef>
            {
                new JoinDef { FromAlias = "h", FromColumn = "BranchID",            ToTable = "tbl_Branch",               ToAlias = "br",   ToColumn = "ID",         JoinType = "LEFT" },
                new JoinDef { FromAlias = "h", FromColumn = "CostCenterID",        ToTable = "tbl_CostCenter",           ToAlias = "cc",   ToColumn = "ID",         JoinType = "LEFT" },
                new JoinDef { FromAlias = "h", FromColumn = "CashID",              ToTable = "tbl_CashDrawer",           ToAlias = "cd",   ToColumn = "ID",         JoinType = "LEFT" },
                new JoinDef { FromAlias = "h", FromColumn = "AccountID",           ToTable = "tbl_Accounts",             ToAlias = "acc",  ToColumn = "ID",         JoinType = "LEFT" },
                new JoinDef { FromAlias = "h", FromColumn = "VoucherType",         ToTable = "tbl_JournalVoucherTypes",  ToAlias = "jvt",  ToColumn = "ID",         JoinType = "LEFT" },
                new JoinDef { FromAlias = "h", FromColumn = "PaymentMethodTypeID", ToTable = "tbl_PaymentMethod",        ToAlias = "pm",   ToColumn = "ID",         JoinType = "LEFT" },
                new JoinDef { FromAlias = "h", FromColumn = "Guid",                ToTable = "tbl_CashVoucherDetails",   ToAlias = "d",    ToColumn = "HeaderGuid", JoinType = "LEFT" },
                new JoinDef { FromAlias = "d", FromColumn = "AccountID",           ToTable = "tbl_Accounts",             ToAlias = "dacc", ToColumn = "ID",         JoinType = "LEFT" },
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
        // MODULE — EMPLOYEES
        // =====================================================================
        private static ModuleDef BuildModule_Employees() => new ModuleDef
        {
            Id = "employees",
            Label = "Employees",
            Icon = "users",
            Color = "#4F46E5",
            PrimaryTable = "tbl_employee",
            PrimaryAlias = "e",
            PrimaryKey = "ID",
            Fields = new List<FieldDef>
            {
                new FieldDef { Id = "emp_code",       Label = "Employee Code",     Table = "tbl_employee", Alias = "e",   Column = "EmployeeCode",        Type = "text"     },
                new FieldDef { Id = "emp_name_ar",    Label = "Name (AR)",         Table = "tbl_employee", Alias = "e",   Column = "AName",               Type = "text"     },
                new FieldDef { Id = "emp_name_en",    Label = "Name (EN)",         Table = "tbl_employee", Alias = "e",   Column = "EName",               Type = "text"     },
                new FieldDef { Id = "emp_email",      Label = "Email",             Table = "tbl_employee", Alias = "e",   Column = "Email",               Type = "text"     },
                new FieldDef { Id = "emp_tel1",       Label = "Tel 1",             Table = "tbl_employee", Alias = "e",   Column = "Tel1",                Type = "text"     },
                new FieldDef { Id = "emp_tel2",       Label = "Tel 2",             Table = "tbl_employee", Alias = "e",   Column = "Tel2",                Type = "text"     },
                new FieldDef { Id = "emp_address",    Label = "Address",           Table = "tbl_employee", Alias = "e",   Column = "Address",             Type = "text"     },
                new FieldDef { Id = "emp_national_no",Label = "National No",       Table = "tbl_employee", Alias = "e",   Column = "NationalNumber",      Type = "text"     },
                new FieldDef { Id = "emp_id_number",  Label = "ID Number",         Table = "tbl_employee", Alias = "e",   Column = "IDNumber",            Type = "text"     },
                new FieldDef { Id = "emp_id_issue",   Label = "ID Issue Date",     Table = "tbl_employee", Alias = "e",   Column = "IDIssueDate",         Type = "date"     },
                new FieldDef { Id = "emp_id_expire",  Label = "ID Expire Date",    Table = "tbl_employee", Alias = "e",   Column = "IDExpireDate",        Type = "date"     },
                new FieldDef { Id = "emp_passport",   Label = "Passport No",       Table = "tbl_employee", Alias = "e",   Column = "PassportNumber",      Type = "text"     },
                new FieldDef { Id = "emp_hire_date",  Label = "Hire Date",         Table = "tbl_employee", Alias = "e",   Column = "HireDate",            Type = "date"     },
                new FieldDef { Id = "emp_bank_name",  Label = "Bank Name",         Table = "tbl_employee", Alias = "e",   Column = "BankName",            Type = "text"     },
                new FieldDef { Id = "emp_iban",       Label = "IBAN",              Table = "tbl_employee", Alias = "e",   Column = "IBAN",                Type = "text"     },
                new FieldDef { Id = "emp_bank_acct",  Label = "Bank Account",      Table = "tbl_employee", Alias = "e",   Column = "BankAccountNumber",   Type = "text"     },
                new FieldDef { Id = "emp_ssn",        Label = "Social Security No",Table = "tbl_employee", Alias = "e",   Column = "SocialSecurityNumber", Type = "text"    },
                new FieldDef { Id = "emp_active",     Label = "Active",            Table = "tbl_employee", Alias = "e",   Column = "IsActive",            Type = "bool"     },
                new FieldDef { Id = "emp_admin",      Label = "Is Admin",          Table = "tbl_employee", Alias = "e",   Column = "IsAdmin",             Type = "bool"     },
                new FieldDef { Id = "emp_pos_only",   Label = "POS Only",          Table = "tbl_employee", Alias = "e",   Column = "IsPOSOnly",           Type = "bool"     },
                new FieldDef { Id = "dept_name",      Label = "Department",        Table = "tbl_Department", Alias = "dep", Column = "AName",             Type = "text"     },
            },
            Joins = new List<JoinDef>
            {
                new JoinDef { FromAlias = "e", FromColumn = "DepartmentID", ToTable = "tbl_Department", ToAlias = "dep", ToColumn = "ID", JoinType = "LEFT" },
            }
        };


        // =====================================================================
        // MODULE — EMPLOYEE CONTRACTS
        // =====================================================================
        private static ModuleDef BuildModule_EmployeeContracts() => new ModuleDef
        {
            Id = "employee_contracts",
            Label = "Employee Contracts",
            Icon = "file-text",
            Color = "#6366F1",
            PrimaryTable = "tbl_EmployeeContract",
            PrimaryAlias = "c",
            PrimaryKey = "ID",
            Fields = new List<FieldDef>
            {
                new FieldDef { Id = "contract_no",    Label = "Contract Number",   Table = "tbl_EmployeeContract", Alias = "c",  Column = "ContractNumber",     Type = "text"     },
                new FieldDef { Id = "start_date",     Label = "Start Date",        Table = "tbl_EmployeeContract", Alias = "c",  Column = "StartDate",          Type = "date"     },
                new FieldDef { Id = "end_date",       Label = "End Date",          Table = "tbl_EmployeeContract", Alias = "c",  Column = "EndDate",            Type = "date"     },
                new FieldDef { Id = "open_ended",     Label = "Open Ended",        Table = "tbl_EmployeeContract", Alias = "c",  Column = "IsOpenEnded",        Type = "bool"     },
                new FieldDef { Id = "basic_salary",   Label = "Basic Salary",      Table = "tbl_EmployeeContract", Alias = "c",  Column = "BasicSalary",        Type = "currency" },
                new FieldDef { Id = "hours_week",     Label = "Hours / Week",      Table = "tbl_EmployeeContract", Alias = "c",  Column = "WorkingHoursPerWeek", Type = "number"  },
                new FieldDef { Id = "is_active",      Label = "Active",            Table = "tbl_EmployeeContract", Alias = "c",  Column = "IsActive",           Type = "bool"     },
                new FieldDef { Id = "notes",          Label = "Notes",             Table = "tbl_EmployeeContract", Alias = "c",  Column = "Notes",              Type = "text"     },
                new FieldDef { Id = "emp_name",       Label = "Employee",          Table = "tbl_employee",         Alias = "e",  Column = "AName",              Type = "text"     },
                new FieldDef { Id = "emp_code",       Label = "Employee Code",     Table = "tbl_employee",         Alias = "e",  Column = "EmployeeCode",       Type = "text"     },
                new FieldDef { Id = "job_title",      Label = "Job Title",         Table = "tbl_JobTitle",         Alias = "jt", Column = "AName",              Type = "text"     },
                new FieldDef { Id = "dept_name",      Label = "Department",        Table = "tbl_Department",       Alias = "dep", Column = "AName",             Type = "text"     },
                new FieldDef { Id = "contract_type",  Label = "Contract Type",     Table = "tbl_HRContractType",   Alias = "ct", Column = "AName",              Type = "text"     },
                new FieldDef { Id = "branch_name",    Label = "Branch",            Table = "tbl_Branch",           Alias = "br", Column = "AName",              Type = "text"     },
            },
            Joins = new List<JoinDef>
            {
                new JoinDef { FromAlias = "c", FromColumn = "EmployeeID",     ToTable = "tbl_employee",       ToAlias = "e",   ToColumn = "ID", JoinType = "LEFT" },
                new JoinDef { FromAlias = "c", FromColumn = "JobTitleID",     ToTable = "tbl_JobTitle",       ToAlias = "jt",  ToColumn = "ID", JoinType = "LEFT" },
                new JoinDef { FromAlias = "c", FromColumn = "DepartmentID",   ToTable = "tbl_Department",     ToAlias = "dep", ToColumn = "ID", JoinType = "LEFT" },
                new JoinDef { FromAlias = "c", FromColumn = "ContractTypeID", ToTable = "tbl_HRContractType", ToAlias = "ct",  ToColumn = "ID", JoinType = "LEFT" },
                new JoinDef { FromAlias = "c", FromColumn = "BranchID",       ToTable = "tbl_Branch",         ToAlias = "br",  ToColumn = "ID", JoinType = "LEFT" },
            }
        };


        // =====================================================================
        // MODULE — LEAVE REQUESTS
        // =====================================================================
        private static ModuleDef BuildModule_LeaveRequests() => new ModuleDef
        {
            Id = "leave_requests",
            Label = "Leave Requests",
            Icon = "calendar",
            Color = "#DB2777",
            PrimaryTable = "tbl_LeaveRequest",
            PrimaryAlias = "r",
            PrimaryKey = "ID",
            Fields = new List<FieldDef>
            {
                new FieldDef { Id = "from_date",    Label = "From Date",        Table = "tbl_LeaveRequest", Alias = "r",  Column = "FromDate",      Type = "date"     },
                new FieldDef { Id = "to_date",      Label = "To Date",          Table = "tbl_LeaveRequest", Alias = "r",  Column = "ToDate",        Type = "date"     },
                new FieldDef { Id = "days",         Label = "Days",             Table = "tbl_LeaveRequest", Alias = "r",  Column = "Days",          Type = "number"   },
                new FieldDef { Id = "reason",       Label = "Reason",           Table = "tbl_LeaveRequest", Alias = "r",  Column = "Reason",        Type = "text"     },
                new FieldDef { Id = "doc_status",   Label = "Document Status",  Table = "tbl_LeaveRequest", Alias = "r",  Column = "DocumentStatus", Type = "number"  },
                new FieldDef { Id = "submitted_at", Label = "Submitted At",     Table = "tbl_LeaveRequest", Alias = "r",  Column = "SubmittedDate", Type = "datetime" },
                new FieldDef { Id = "posted_at",    Label = "Posted At",        Table = "tbl_LeaveRequest", Alias = "r",  Column = "PostedDate",    Type = "datetime" },
                new FieldDef { Id = "created_at",   Label = "Created At",       Table = "tbl_LeaveRequest", Alias = "r",  Column = "CreationDate",  Type = "datetime" },
                new FieldDef { Id = "emp_name",     Label = "Employee",         Table = "tbl_employee",     Alias = "e",  Column = "AName",         Type = "text"     },
                new FieldDef { Id = "emp_code",     Label = "Employee Code",    Table = "tbl_employee",     Alias = "e",  Column = "EmployeeCode",  Type = "text"     },
                new FieldDef { Id = "leave_code",   Label = "Leave Type Code",  Table = "tbl_LeaveType",    Alias = "t",  Column = "Code",          Type = "text"     },
                new FieldDef { Id = "leave_name",   Label = "Leave Type",       Table = "tbl_LeaveType",    Alias = "t",  Column = "AName",         Type = "text"     },
                new FieldDef { Id = "leave_paid",   Label = "Is Paid Leave",    Table = "tbl_LeaveType",    Alias = "t",  Column = "IsPaid",        Type = "bool"     },
                new FieldDef { Id = "branch_name",  Label = "Branch",           Table = "tbl_Branch",       Alias = "br", Column = "AName",         Type = "text"     },
            },
            Joins = new List<JoinDef>
            {
                new JoinDef { FromAlias = "r", FromColumn = "LeaveTypeID", ToTable = "tbl_LeaveType", ToAlias = "t",  ToColumn = "ID", JoinType = "LEFT" },
                new JoinDef { FromAlias = "r", FromColumn = "EmployeeID",  ToTable = "tbl_employee",  ToAlias = "e",  ToColumn = "ID", JoinType = "LEFT" },
                new JoinDef { FromAlias = "r", FromColumn = "BranchID",    ToTable = "tbl_Branch",    ToAlias = "br", ToColumn = "ID", JoinType = "LEFT" },
            }
        };


        // =====================================================================
        // MODULE — PAYROLL RUNS
        // =====================================================================
        private static ModuleDef BuildModule_PayrollRuns() => new ModuleDef
        {
            Id = "payroll_runs",
            Label = "Payroll Runs",
            Icon = "banknote",
            Color = "#7C3AED",
            PrimaryTable = "tbl_PayrollHeader",
            PrimaryAlias = "h",
            PrimaryKey = "ID",
            Fields = new List<FieldDef>
            {
                new FieldDef { Id = "basic_salary",     Label = "Basic Salary",      Table = "tbl_PayrollHeader", Alias = "h",  Column = "BasicSalary",      Type = "currency" },
                new FieldDef { Id = "total_earnings",   Label = "Total Earnings",    Table = "tbl_PayrollHeader", Alias = "h",  Column = "TotalEarnings",    Type = "currency" },
                new FieldDef { Id = "total_deductions", Label = "Total Deductions",  Table = "tbl_PayrollHeader", Alias = "h",  Column = "TotalDeductions",  Type = "currency" },
                new FieldDef { Id = "net_salary",       Label = "Net Salary",        Table = "tbl_PayrollHeader", Alias = "h",  Column = "NetSalary",        Type = "currency" },
                new FieldDef { Id = "status",           Label = "Status",            Table = "tbl_PayrollHeader", Alias = "h",  Column = "Status",           Type = "number"   },
                new FieldDef { Id = "is_posted",        Label = "Is Posted",         Table = "tbl_PayrollHeader", Alias = "h",  Column = "IsPosted",         Type = "bool"     },
                new FieldDef { Id = "doc_status",       Label = "Document Status",   Table = "tbl_PayrollHeader", Alias = "h",  Column = "DocumentStatus",   Type = "number"   },
                new FieldDef { Id = "created_at",       Label = "Created At",        Table = "tbl_PayrollHeader", Alias = "h",  Column = "CreationDate",     Type = "datetime" },
                new FieldDef { Id = "period_name_ar",   Label = "Period (AR)",       Table = "tbl_PayrollPeriod", Alias = "p",  Column = "PeriodAName",      Type = "text"     },
                new FieldDef { Id = "period_name_en",   Label = "Period (EN)",       Table = "tbl_PayrollPeriod", Alias = "p",  Column = "PeriodEName",      Type = "text"     },
                new FieldDef { Id = "period_start",     Label = "Period Start",      Table = "tbl_PayrollPeriod", Alias = "p",  Column = "StartDate",        Type = "date"     },
                new FieldDef { Id = "period_end",       Label = "Period End",        Table = "tbl_PayrollPeriod", Alias = "p",  Column = "EndDate",          Type = "date"     },
                new FieldDef { Id = "period_closed",    Label = "Period Closed",     Table = "tbl_PayrollPeriod", Alias = "p",  Column = "IsClosed",         Type = "bool"     },
                new FieldDef { Id = "emp_name",         Label = "Employee",          Table = "tbl_employee",      Alias = "e",  Column = "AName",            Type = "text"     },
                new FieldDef { Id = "emp_code",         Label = "Employee Code",     Table = "tbl_employee",      Alias = "e",  Column = "EmployeeCode",     Type = "text"     },
                new FieldDef { Id = "elem_code",        Label = "Element Code",      Table = "tbl_SalariesElements", Alias = "se", Column = "Code",          Type = "text"     },
                new FieldDef { Id = "elem_name",        Label = "Element Name",      Table = "tbl_SalariesElements", Alias = "se", Column = "AName",         Type = "text"     },
                new FieldDef { Id = "elem_type_id",     Label = "Element Type Id",   Table = "tbl_PayrollDetails", Alias = "d", Column = "ElementTypeID",    Type = "number"   },
                new FieldDef { Id = "calc_type_id",     Label = "Calc Type Id",      Table = "tbl_PayrollDetails", Alias = "d", Column = "CalcTypeID",       Type = "number"   },
                new FieldDef { Id = "assigned_value",   Label = "Assigned Value",    Table = "tbl_PayrollDetails", Alias = "d", Column = "AssignedValue",    Type = "number"   },
                new FieldDef { Id = "calculated_amt",   Label = "Calculated Amount", Table = "tbl_PayrollDetails", Alias = "d", Column = "CalculatedAmount", Type = "currency" },
            },
            Joins = new List<JoinDef>
            {
                new JoinDef { FromAlias = "h", FromColumn = "PayrollPeriodID", ToTable = "tbl_PayrollPeriod",    ToAlias = "p",  ToColumn = "ID",              JoinType = "LEFT" },
                new JoinDef { FromAlias = "h", FromColumn = "EmployeeID",      ToTable = "tbl_employee",         ToAlias = "e",  ToColumn = "ID",              JoinType = "LEFT" },
                new JoinDef { FromAlias = "h", FromColumn = "ID",              ToTable = "tbl_PayrollDetails",   ToAlias = "d",  ToColumn = "PayrollHeaderID", JoinType = "LEFT" },
                new JoinDef { FromAlias = "d", FromColumn = "SalaryElementID", ToTable = "tbl_SalariesElements", ToAlias = "se", ToColumn = "ID",              JoinType = "LEFT" },
            }
        };


        // =====================================================================
        // MODULE — FINANCING / LOANS
        // =====================================================================
        private static ModuleDef BuildModule_FinancingLoans() => new ModuleDef
        {
            Id = "financing_loans",
            Label = "Financing / Loans",
            Icon = "landmark",
            Color = "#C2410C",
            PrimaryTable = "tbl_FinancingHeader",
            PrimaryAlias = "h",
            PrimaryKey = "Guid",
            Fields = new List<FieldDef>
            {
                new FieldDef { Id = "voucher_no",     Label = "Voucher Number",        Table = "tbl_FinancingHeader", Alias = "h",   Column = "VoucherNumber",            Type = "text"     },
                new FieldDef { Id = "voucher_date",   Label = "Voucher Date",          Table = "tbl_FinancingHeader", Alias = "h",   Column = "VoucherDate",              Type = "date"     },
                new FieldDef { Id = "total_amount",   Label = "Total Amount",          Table = "tbl_FinancingHeader", Alias = "h",   Column = "TotalAmount",              Type = "currency" },
                new FieldDef { Id = "down_payment",   Label = "Down Payment",          Table = "tbl_FinancingHeader", Alias = "h",   Column = "DownPayment",              Type = "currency" },
                new FieldDef { Id = "net_amount",     Label = "Net Amount",            Table = "tbl_FinancingHeader", Alias = "h",   Column = "NetAmount",                Type = "currency" },
                new FieldDef { Id = "interest_rate",  Label = "Interest Rate",         Table = "tbl_FinancingHeader", Alias = "h",   Column = "IntrestRate",              Type = "number"   },
                new FieldDef { Id = "months_count",   Label = "Months Count",          Table = "tbl_FinancingHeader", Alias = "h",   Column = "MonthsCount",              Type = "number"   },
                new FieldDef { Id = "note",           Label = "Note",                  Table = "tbl_FinancingHeader", Alias = "h",   Column = "Note",                     Type = "text"     },
                new FieldDef { Id = "grantor",        Label = "Grantor",               Table = "tbl_FinancingHeader", Alias = "h",   Column = "Grantor",                  Type = "text"     },
                new FieldDef { Id = "purch_ref",      Label = "Purchase Invoice Ref",  Table = "tbl_FinancingHeader", Alias = "h",   Column = "PurchaseInvoiceRefNumber", Type = "text"     },
                new FieldDef { Id = "is_returned",    Label = "Amount Returned",       Table = "tbl_FinancingHeader", Alias = "h",   Column = "isAmountReturned",         Type = "bool"     },
                new FieldDef { Id = "show_monthly",   Label = "Show In Monthly Reports", Table = "tbl_FinancingHeader", Alias = "h", Column = "IsShowInMonthlyReports",  Type = "bool"     },
                new FieldDef { Id = "is_posted",      Label = "Is Posted",             Table = "tbl_FinancingHeader", Alias = "h",   Column = "IsPosted",                 Type = "bool"     },
                new FieldDef { Id = "bp_name",        Label = "Customer",              Table = "tbl_BusinessPartner", Alias = "bp",  Column = "AName",                    Type = "text"     },
                new FieldDef { Id = "bp_emp_code",    Label = "Partner Emp Code",      Table = "tbl_BusinessPartner", Alias = "bp",  Column = "EmpCode",                  Type = "text"     },
                new FieldDef { Id = "vendor_name",    Label = "Vendor",                Table = "tbl_BusinessPartner", Alias = "vend", Column = "AName",                   Type = "text"     },
                new FieldDef { Id = "loan_type",      Label = "Loan Type",             Table = "tbl_LoanTypes",       Alias = "lt",  Column = "AName",                    Type = "text"     },
                new FieldDef { Id = "loan_code",      Label = "Loan Type Code",        Table = "tbl_LoanTypes",       Alias = "lt",  Column = "Code",                     Type = "text"     },
                new FieldDef { Id = "branch_name",    Label = "Branch",                Table = "tbl_Branch",          Alias = "br",  Column = "AName",                    Type = "text"     },
                new FieldDef { Id = "cc_name",        Label = "Cost Center",           Table = "tbl_CostCenter",      Alias = "cc",  Column = "AName",                    Type = "text"     },
                new FieldDef { Id = "dtl_desc",       Label = "Line Description",      Table = "tbl_FinancingDetails", Alias = "d",  Column = "Description",              Type = "text"     },
                new FieldDef { Id = "dtl_total",      Label = "Line Total",            Table = "tbl_FinancingDetails", Alias = "d",  Column = "TotalAmount",              Type = "currency" },
                new FieldDef { Id = "dtl_down",       Label = "Line Down Payment",     Table = "tbl_FinancingDetails", Alias = "d",  Column = "DownPayment",              Type = "currency" },
                new FieldDef { Id = "dtl_fin_amt",    Label = "Financing Amount",      Table = "tbl_FinancingDetails", Alias = "d",  Column = "FinancingAmount",          Type = "currency" },
                new FieldDef { Id = "dtl_period",     Label = "Period (Months)",       Table = "tbl_FinancingDetails", Alias = "d",  Column = "PeriodInMonths",           Type = "number"   },
                new FieldDef { Id = "dtl_rate",       Label = "Line Interest Rate",    Table = "tbl_FinancingDetails", Alias = "d",  Column = "InterestRate",             Type = "number"   },
                new FieldDef { Id = "dtl_interest",   Label = "Interest Amount",       Table = "tbl_FinancingDetails", Alias = "d",  Column = "InterestAmount",           Type = "currency" },
                new FieldDef { Id = "dtl_with_int",   Label = "Total With Interest",   Table = "tbl_FinancingDetails", Alias = "d",  Column = "TotalAmountWithInterest",  Type = "currency" },
                new FieldDef { Id = "dtl_first_inst", Label = "First Installment Date", Table = "tbl_FinancingDetails", Alias = "d", Column = "FirstInstallmentDate",     Type = "date"     },
                new FieldDef { Id = "dtl_inst_amt",   Label = "Installment Amount",    Table = "tbl_FinancingDetails", Alias = "d",  Column = "InstallmentAmount",        Type = "currency" },
                new FieldDef { Id = "dtl_serial",     Label = "Serial Number",         Table = "tbl_FinancingDetails", Alias = "d",  Column = "SerialNumber",             Type = "text"     },
            },
            Joins = new List<JoinDef>
            {
                new JoinDef { FromAlias = "h", FromColumn = "BusinessPartnerID", ToTable = "tbl_BusinessPartner", ToAlias = "bp",   ToColumn = "ID",         JoinType = "LEFT" },
                new JoinDef { FromAlias = "h", FromColumn = "VendorID",          ToTable = "tbl_BusinessPartner", ToAlias = "vend", ToColumn = "ID",         JoinType = "LEFT" },
                new JoinDef { FromAlias = "h", FromColumn = "LoanType",          ToTable = "tbl_LoanTypes",       ToAlias = "lt",   ToColumn = "ID",         JoinType = "LEFT" },
                new JoinDef { FromAlias = "h", FromColumn = "BranchID",          ToTable = "tbl_Branch",          ToAlias = "br",   ToColumn = "ID",         JoinType = "LEFT" },
                new JoinDef { FromAlias = "h", FromColumn = "CostCenterID",      ToTable = "tbl_CostCenter",      ToAlias = "cc",   ToColumn = "ID",         JoinType = "LEFT" },
                new JoinDef { FromAlias = "h", FromColumn = "Guid",              ToTable = "tbl_FinancingDetails", ToAlias = "d",   ToColumn = "HeaderGuid", JoinType = "LEFT" },
            }
        };


        // =====================================================================
        // MODULE — MANUFACTURING ORDERS
        // =====================================================================
        private static ModuleDef BuildModule_ManufacturingOrders() => new ModuleDef
        {
            Id = "manufacturing_orders",
            Label = "Manufacturing Orders",
            Icon = "factory",
            Color = "#1D4ED8",
            PrimaryTable = "tbl_MOHeader",
            PrimaryAlias = "h",
            PrimaryKey = "Guid",
            Fields = new List<FieldDef>
            {
                new FieldDef { Id = "mo_code",         Label = "MO Code",               Table = "tbl_MOHeader",  Alias = "h",   Column = "MOCode",           Type = "text"     },
                new FieldDef { Id = "mo_name",         Label = "MO Name",               Table = "tbl_MOHeader",  Alias = "h",   Column = "MOName",           Type = "text"     },
                new FieldDef { Id = "mo_date",         Label = "MO Date",               Table = "tbl_MOHeader",  Alias = "h",   Column = "MODate",           Type = "date"     },
                new FieldDef { Id = "planned_start",   Label = "Planned Start",         Table = "tbl_MOHeader",  Alias = "h",   Column = "PlannedStartDate", Type = "date"     },
                new FieldDef { Id = "planned_end",     Label = "Planned End",           Table = "tbl_MOHeader",  Alias = "h",   Column = "PlannedEndDate",   Type = "date"     },
                new FieldDef { Id = "planned_qty",     Label = "Planned Qty",           Table = "tbl_MOHeader",  Alias = "h",   Column = "PlannedQty",       Type = "number"   },
                new FieldDef { Id = "batch_qty",       Label = "Batch Qty",             Table = "tbl_MOHeader",  Alias = "h",   Column = "BatchQty",         Type = "number"   },
                new FieldDef { Id = "status_id",       Label = "Status Id",             Table = "tbl_MOHeader",  Alias = "h",   Column = "StatusID",         Type = "number"   },
                new FieldDef { Id = "notes",           Label = "Notes",                 Table = "tbl_MOHeader",  Alias = "h",   Column = "Notes",            Type = "text"     },
                new FieldDef { Id = "is_active",       Label = "Active",                Table = "tbl_MOHeader",  Alias = "h",   Column = "IsActive",         Type = "bool"     },
                new FieldDef { Id = "bom_code",        Label = "BOM Code",              Table = "tbl_BOMHeader", Alias = "bom", Column = "BOMCode",          Type = "text"     },
                new FieldDef { Id = "bom_name",        Label = "BOM Name",              Table = "tbl_BOMHeader", Alias = "bom", Column = "BOMName",          Type = "text"     },
                new FieldDef { Id = "branch_name",     Label = "Branch",                Table = "tbl_Branch",    Alias = "br",  Column = "AName",            Type = "text"     },
                new FieldDef { Id = "store_name",      Label = "Store",                 Table = "tbl_Store",     Alias = "st",  Column = "AName",            Type = "text"     },
                new FieldDef { Id = "line_type_id",    Label = "Line Type Id",          Table = "tbl_MODetails", Alias = "d",   Column = "LineTypeID",       Type = "number"   },
                new FieldDef { Id = "line_item_name",  Label = "Component/Output Name", Table = "tbl_MODetails", Alias = "d",   Column = "ItemName",         Type = "text"     },
                new FieldDef { Id = "line_planned_qty",Label = "Line Planned Qty",      Table = "tbl_MODetails", Alias = "d",   Column = "PlannedQty",       Type = "number"   },
                new FieldDef { Id = "line_scrap_pct",  Label = "Scrap %",               Table = "tbl_MODetails", Alias = "d",   Column = "ScrapPercent",     Type = "number"   },
                new FieldDef { Id = "line_cost_share", Label = "Cost Share %",          Table = "tbl_MODetails", Alias = "d",   Column = "CostSharePercent", Type = "number"   },
                new FieldDef { Id = "line_notes",      Label = "Line Notes",            Table = "tbl_MODetails", Alias = "d",   Column = "Notes",            Type = "text"     },
                new FieldDef { Id = "item_barcode",    Label = "Barcode",               Table = "tbl_Items",     Alias = "itm", Column = "Barcode",          Type = "text"     },
                new FieldDef { Id = "uom_name",        Label = "UOM",                   Table = "tbl_UOM",       Alias = "uom", Column = "AName",            Type = "text"     },
            },
            Joins = new List<JoinDef>
            {
                new JoinDef { FromAlias = "h", FromColumn = "BOMID",    ToTable = "tbl_BOMHeader", ToAlias = "bom", ToColumn = "ID",         JoinType = "LEFT" },
                new JoinDef { FromAlias = "h", FromColumn = "BranchID", ToTable = "tbl_Branch",    ToAlias = "br",  ToColumn = "ID",         JoinType = "LEFT" },
                new JoinDef { FromAlias = "h", FromColumn = "StoreID",  ToTable = "tbl_Store",     ToAlias = "st",  ToColumn = "ID",         JoinType = "LEFT" },
                new JoinDef { FromAlias = "h", FromColumn = "Guid",     ToTable = "tbl_MODetails", ToAlias = "d",   ToColumn = "HeaderGuid", JoinType = "LEFT" },
                new JoinDef { FromAlias = "d", FromColumn = "ItemGuid", ToTable = "tbl_Items",     ToAlias = "itm", ToColumn = "Guid",       JoinType = "LEFT" },
                new JoinDef { FromAlias = "d", FromColumn = "UOMID",    ToTable = "tbl_UOM",       ToAlias = "uom", ToColumn = "ID",         JoinType = "LEFT" },
            }
        };


        // =====================================================================
        // MODULE — BILL OF MATERIALS
        // =====================================================================
        private static ModuleDef BuildModule_BOM() => new ModuleDef
        {
            Id = "bom",
            Label = "Bill of Materials",
            Icon = "layers",
            Color = "#2563EB",
            PrimaryTable = "tbl_BOMHeader",
            PrimaryAlias = "h",
            PrimaryKey = "ID",
            Fields = new List<FieldDef>
            {
                new FieldDef { Id = "bom_code",       Label = "BOM Code",         Table = "tbl_BOMHeader", Alias = "h",   Column = "BOMCode",       Type = "text"     },
                new FieldDef { Id = "bom_name",       Label = "BOM Name",         Table = "tbl_BOMHeader", Alias = "h",   Column = "BOMName",       Type = "text"     },
                new FieldDef { Id = "batch_qty",      Label = "Batch Qty",        Table = "tbl_BOMHeader", Alias = "h",   Column = "BatchQty",      Type = "number"   },
                new FieldDef { Id = "version_no",     Label = "Version",          Table = "tbl_BOMHeader", Alias = "h",   Column = "VersionNo",     Type = "number"   },
                new FieldDef { Id = "is_default",     Label = "Is Default",       Table = "tbl_BOMHeader", Alias = "h",   Column = "IsDefault",     Type = "bool"     },
                new FieldDef { Id = "is_active",      Label = "Active",           Table = "tbl_BOMHeader", Alias = "h",   Column = "IsActive",      Type = "bool"     },
                new FieldDef { Id = "effective_from", Label = "Effective From",   Table = "tbl_BOMHeader", Alias = "h",   Column = "EffectiveFrom", Type = "date"     },
                new FieldDef { Id = "effective_to",   Label = "Effective To",     Table = "tbl_BOMHeader", Alias = "h",   Column = "EffectiveTo",   Type = "date"     },
                new FieldDef { Id = "notes",          Label = "Notes",            Table = "tbl_BOMHeader", Alias = "h",   Column = "Notes",         Type = "text"     },
                new FieldDef { Id = "comp_qty",       Label = "Component Qty",    Table = "tbl_BOMInput",  Alias = "i",   Column = "Qty",           Type = "number"   },
                new FieldDef { Id = "comp_scrap",     Label = "Component Scrap %",Table = "tbl_BOMInput",  Alias = "i",   Column = "ScrapPercent",  Type = "number"   },
                new FieldDef { Id = "comp_phantom",   Label = "Is Phantom",       Table = "tbl_BOMInput",  Alias = "i",   Column = "IsPhantom",     Type = "bool"     },
                new FieldDef { Id = "comp_notes",     Label = "Component Notes",  Table = "tbl_BOMInput",  Alias = "i",   Column = "Notes",         Type = "text"     },
                new FieldDef { Id = "comp_name",      Label = "Component Item",   Table = "tbl_Items",     Alias = "itm", Column = "AName",         Type = "text"     },
                new FieldDef { Id = "comp_barcode",   Label = "Component Barcode",Table = "tbl_Items",     Alias = "itm", Column = "Barcode",       Type = "text"     },
                new FieldDef { Id = "uom_name",       Label = "UOM",              Table = "tbl_UOM",       Alias = "uom", Column = "AName",         Type = "text"     },
            },
            Joins = new List<JoinDef>
            {
                new JoinDef { FromAlias = "h", FromColumn = "ID",                 ToTable = "tbl_BOMInput", ToAlias = "i",   ToColumn = "BOMID", JoinType = "LEFT" },
                new JoinDef { FromAlias = "i", FromColumn = "ComponentItemGuid",  ToTable = "tbl_Items",    ToAlias = "itm", ToColumn = "Guid",  JoinType = "LEFT" },
                new JoinDef { FromAlias = "i", FromColumn = "UOMID",              ToTable = "tbl_UOM",      ToAlias = "uom", ToColumn = "ID",    JoinType = "LEFT" },
            }
        };


        // =====================================================================
        // MODULE — WORK CENTERS
        // =====================================================================
        private static ModuleDef BuildModule_WorkCenters() => new ModuleDef
        {
            Id = "work_centers",
            Label = "Work Centers",
            Icon = "settings",
            Color = "#64748B",
            PrimaryTable = "tbl_WorkCenter",
            PrimaryAlias = "wc",
            PrimaryKey = "ID",
            Fields = new List<FieldDef>
            {
                new FieldDef { Id = "wc_code",         Label = "Work Center Code", Table = "tbl_WorkCenter", Alias = "wc", Column = "WorkCenterCode", Type = "text"     },
                new FieldDef { Id = "wc_name_ar",      Label = "Name (AR)",        Table = "tbl_WorkCenter", Alias = "wc", Column = "AName",          Type = "text"     },
                new FieldDef { Id = "wc_name_en",      Label = "Name (EN)",        Table = "tbl_WorkCenter", Alias = "wc", Column = "EName",          Type = "text"     },
                new FieldDef { Id = "capacity_day",    Label = "Capacity / Day",   Table = "tbl_WorkCenter", Alias = "wc", Column = "CapacityPerDay", Type = "number"   },
                new FieldDef { Id = "hourly_rate",     Label = "Hourly Rate",      Table = "tbl_WorkCenter", Alias = "wc", Column = "HourlyRate",     Type = "currency" },
                new FieldDef { Id = "is_active",       Label = "Active",           Table = "tbl_WorkCenter", Alias = "wc", Column = "IsActive",       Type = "bool"     },
                new FieldDef { Id = "notes",           Label = "Notes",            Table = "tbl_WorkCenter", Alias = "wc", Column = "Notes",          Type = "text"     },
                new FieldDef { Id = "branch_name",     Label = "Branch",           Table = "tbl_Branch",     Alias = "br", Column = "AName",          Type = "text"     },
            },
            Joins = new List<JoinDef>
            {
                new JoinDef { FromAlias = "wc", FromColumn = "BranchID", ToTable = "tbl_Branch", ToAlias = "br", ToColumn = "ID", JoinType = "LEFT" },
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
            /// <summary>Optional extra SQL AND clauses (no leading AND). Use primary alias only.</summary>
            public string FixedWhere { get; set; } = "";
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

            if (!string.IsNullOrWhiteSpace(module.FixedWhere))
                sbWhere.AppendLine($"AND ({module.FixedWhere})");

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
                        // Prefer client Alias (e.g. sum_debit) so grouped preview columns match.
                        string alias = string.IsNullOrWhiteSpace(m.Alias) ? m.FieldId! : m.Alias!;
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
        // SAVED REPORT LAYOUTS (persistent)
        // =====================================================================
        private void EnsureSavedTable(int companyId)
        {
            const string sql = @"
IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'tbl_ReportBuilderSaved')
BEGIN
    CREATE TABLE dbo.tbl_ReportBuilderSaved (
        ID INT IDENTITY(1,1) PRIMARY KEY,
        CreationDate DATETIME NULL CONSTRAINT DF_ReportBuilderSaved_CreationDate DEFAULT (GETDATE()),
        ReportName NVARCHAR(MAX) NULL,
        ModuleId NVARCHAR(100) NULL,
        ConfigJson NVARCHAR(MAX) NULL,
        UserID INT NULL,
        CompanyID INT NULL,
        IsActive BIT NULL CONSTRAINT DF_ReportBuilderSaved_IsActive DEFAULT (1),
        CreationUserID INT NULL,
        ModificationUserID INT NULL,
        ModificationDate DATETIME NULL
    );
END";
            ExecuteNonQuery(sql, new List<SqlParameter>(), companyId);
        }

        public List<object> ListSavedReports(int companyId, int userId)
        {
            EnsureSavedTable(companyId);
            const string sql = @"
SELECT ID, ReportName, ModuleId, ConfigJson, CreationDate, ModificationDate, UserID
FROM tbl_ReportBuilderSaved
WHERE CompanyID = @CompanyID
  AND IsActive = 1
  AND (UserID = @UserID OR UserID = 0 OR UserID IS NULL)
ORDER BY ISNULL(ModificationDate, CreationDate) DESC, ID DESC";
            var prms = new List<SqlParameter>
            {
                new SqlParameter("@CompanyID", SqlDbType.Int) { Value = companyId },
                new SqlParameter("@UserID", SqlDbType.Int) { Value = userId },
            };
            var dt = ExecuteDataTable(sql, prms, companyId);
            var list = new List<object>();
            foreach (DataRow row in dt.Rows)
            {
                list.Add(new
                {
                    id = Convert.ToInt32(row["ID"]),
                    name = Simulate.String(row["ReportName"]),
                    moduleId = Simulate.String(row["ModuleId"]),
                    configJson = Simulate.String(row["ConfigJson"]),
                    createdAt = row["CreationDate"] == DBNull.Value ? (DateTime?)null : Convert.ToDateTime(row["CreationDate"]),
                    modifiedAt = row["ModificationDate"] == DBNull.Value ? (DateTime?)null : Convert.ToDateTime(row["ModificationDate"]),
                    userId = row["UserID"] == DBNull.Value ? 0 : Convert.ToInt32(row["UserID"]),
                });
            }
            return list;
        }

        public int SaveReportLayout(
            int? id,
            string reportName,
            string moduleId,
            string configJson,
            int companyId,
            int userId)
        {
            EnsureSavedTable(companyId);
            if (string.IsNullOrWhiteSpace(reportName))
                throw new ArgumentException("Report name is required.");
            if (string.IsNullOrWhiteSpace(moduleId))
                throw new ArgumentException("Module is required.");
            if (string.IsNullOrWhiteSpace(configJson))
                throw new ArgumentException("Config is required.");

            if (id.HasValue && id.Value > 0)
            {
                const string upd = @"
UPDATE tbl_ReportBuilderSaved
SET ReportName = @ReportName,
    ModuleId = @ModuleId,
    ConfigJson = @ConfigJson,
    ModificationUserID = @UserID,
    ModificationDate = GETDATE(),
    IsActive = 1
WHERE ID = @ID AND CompanyID = @CompanyID AND IsActive = 1";
                var updPrms = new List<SqlParameter>
                {
                    new SqlParameter("@ID", SqlDbType.Int) { Value = id.Value },
                    new SqlParameter("@ReportName", SqlDbType.NVarChar, -1) { Value = reportName.Trim() },
                    new SqlParameter("@ModuleId", SqlDbType.NVarChar, 100) { Value = moduleId.Trim() },
                    new SqlParameter("@ConfigJson", SqlDbType.NVarChar, -1) { Value = configJson },
                    new SqlParameter("@UserID", SqlDbType.Int) { Value = userId },
                    new SqlParameter("@CompanyID", SqlDbType.Int) { Value = companyId },
                };
                int updated = ExecuteNonQuery(upd, updPrms, companyId);
                if (updated > 0) return id.Value;
                // Fall through to insert if the row was missing/soft-deleted.
            }

            const string ins = @"
INSERT INTO tbl_ReportBuilderSaved
    (ReportName, ModuleId, ConfigJson, UserID, CompanyID, IsActive, CreationUserID, CreationDate)
OUTPUT INSERTED.ID
VALUES
    (@ReportName, @ModuleId, @ConfigJson, @UserID, @CompanyID, 1, @UserID, GETDATE())";
            var insPrms = new List<SqlParameter>
            {
                new SqlParameter("@ReportName", SqlDbType.NVarChar, -1) { Value = reportName.Trim() },
                new SqlParameter("@ModuleId", SqlDbType.NVarChar, 100) { Value = moduleId.Trim() },
                new SqlParameter("@ConfigJson", SqlDbType.NVarChar, -1) { Value = configJson },
                new SqlParameter("@UserID", SqlDbType.Int) { Value = userId },
                new SqlParameter("@CompanyID", SqlDbType.Int) { Value = companyId },
            };
            object? newId = ExecuteScalar(ins, insPrms, companyId);
            return Convert.ToInt32(newId);
        }

        public bool DeleteSavedReport(int id, int companyId, int userId)
        {
            EnsureSavedTable(companyId);
            const string sql = @"
UPDATE tbl_ReportBuilderSaved
SET IsActive = 0,
    ModificationUserID = @UserID,
    ModificationDate = GETDATE()
WHERE ID = @ID AND CompanyID = @CompanyID AND IsActive = 1";
            var prms = new List<SqlParameter>
            {
                new SqlParameter("@ID", SqlDbType.Int) { Value = id },
                new SqlParameter("@CompanyID", SqlDbType.Int) { Value = companyId },
                new SqlParameter("@UserID", SqlDbType.Int) { Value = userId },
            };
            return ExecuteNonQuery(sql, prms, companyId) > 0;
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
            string conString = clsSQL.CreateDataBaseConnectionString(CompanyID);
            if (string.IsNullOrWhiteSpace(conString))
                throw new InvalidOperationException($"Database connection not found for CompanyID={CompanyID}.");
            using var cn = new SqlConnection(conString);
            using var cmd = new SqlCommand(sql, cn);
            if (prms != null && prms.Count > 0)
                cmd.Parameters.AddRange(prms.ToArray());
            using var da = new SqlDataAdapter(cmd);
            var dt = new DataTable();
            da.Fill(dt);
            return dt;
        }

        private object? ExecuteScalar(string sql, List<SqlParameter> prms, int CompanyID)
        {
            clsSQL clsSQL = new clsSQL();
            string conString = clsSQL.CreateDataBaseConnectionString(CompanyID);
            if (string.IsNullOrWhiteSpace(conString))
                throw new InvalidOperationException($"Database connection not found for CompanyID={CompanyID}.");
            using var cn = new SqlConnection(conString);
            using var cmd = new SqlCommand(sql, cn);
            if (prms != null && prms.Count > 0)
                cmd.Parameters.AddRange(prms.ToArray());
            cn.Open();
            return cmd.ExecuteScalar();
        }

        private int ExecuteNonQuery(string sql, List<SqlParameter> prms, int CompanyID)
        {
            clsSQL clsSQL = new clsSQL();
            string conString = clsSQL.CreateDataBaseConnectionString(CompanyID);
            if (string.IsNullOrWhiteSpace(conString))
                throw new InvalidOperationException($"Database connection not found for CompanyID={CompanyID}.");
            using var cn = new SqlConnection(conString);
            using var cmd = new SqlCommand(sql, cn);
            if (prms != null && prms.Count > 0)
                cmd.Parameters.AddRange(prms.ToArray());
            cn.Open();
            return cmd.ExecuteNonQuery();
        }
    }
}