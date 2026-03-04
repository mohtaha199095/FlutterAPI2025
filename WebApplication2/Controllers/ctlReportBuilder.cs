using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using WebApplication2.cls;
using static WebApplication2.Controllers.ReportBuilderService;

namespace WebApplication2.Controllers
{
    [Route("api/ctlReportBuilder")]
    public class ctlReportBuilder : Controller
    {
   
            // ==========================================================
            // DTOs (Body)
            // ==========================================================
            public class RunReportBody
            {
                public List<string> FieldIds { get; set; }
                public List<RunReportFilter> Filters { get; set; }
            // NEW: fieldId -> agg function ("sum","count","avg","min","max","countDistinct")
            public List<MeasureDto>? Agg { get; set; }   // list of aggregations
            // NEW: include COUNT(1)
            public bool IncludeRowCount { get; set; } = true;
        }

            //public class RunReportFilter
            //{
            //    public string FieldId { get; set; }
            //    public string Operator { get; set; } // equals/contains/between/inList...
            //    public string Value { get; set; }
            //    public string Value2 { get; set; }
            //}

            // ==========================================================
            // 1) GET CATALOG
            // Flutter: GET /api/ctlReports/GetCatalog?CompanyID=...
            // Returns: List of modules
            // each module has Fields[] + Joins[]
            // ==========================================================
            [HttpGet]
            [Route("GetCatalog")]
            public string GetCatalog(int CompanyID)
            {
                try
                {
                ReportBuilderService r = new ReportBuilderService();

                    // You decide where catalog lives:
                    // - Could be JSON stored in DB
                    // - Could be static in code
                    // Here we return a fully built object list (no DataTables needed)

                    var modules = r.GetCatalogList(CompanyID);

                    return JsonConvert.SerializeObject(modules);
                }
                catch
                {
                    throw;
                }
            }

            // ==========================================================
            // 2) RUN REPORT
            // Flutter: POST /api/ctlReports/Run?CompanyID=...&ModuleId=...&Page=1&PageSize=50&SortByFieldId=...&SortDir=desc&GroupByFieldId=...
            // Body: { FieldIds:[...], Filters:[...] }
            // Returns: { rows:[{fieldId:value,...}], totalRows: 123 }
            // ==========================================================
            [HttpPost]
            [Route("Run")]
            public string Run(
                int CompanyID,
                string ModuleId,
                int Page,
                int PageSize,
                string SortByFieldId,
                string SortDir,
                string GroupByFieldId,
                [FromBody] RunReportBody body
            )
            {
                try
                {
                    if (body == null)
                    {
                        var empty0 = new { rows = new object[] { }, totalRows = 0 };
                        return JsonConvert.SerializeObject(empty0);
                    }

                    // Safe defaults
                    if (Page <= 0) Page = 1;
                    if (PageSize <= 0) PageSize = 50;
                    if (body.FieldIds == null) body.FieldIds = new List<string>();
                    if (body.Filters == null) body.Filters = new List<RunReportFilter>();
              
                ReportBuilderService r = new ReportBuilderService();

                    // 1) Build SQL + Execute (inside service)
                    //    It should return:
                    //    - DataTable dtRows (data)
                    //    - int totalRows (count without paging)
                    DataTable dtRows = null;
                    int totalRows = 0;

                    r.RunReport(
                        CompanyID: CompanyID,
                        ModuleId: Simulate.String(ModuleId),
                        FieldIds: body.FieldIds,
                        Filters: body.Filters,
                        SortByFieldId: Simulate.String(SortByFieldId),
                        SortDir: Simulate.String(SortDir),
                        GroupByFieldId: Simulate.String(GroupByFieldId),
                        Page: Page,
                        PageSize: PageSize,
                        Agg: body.Agg,
                        IncludeRowCount: body.IncludeRowCount,
                        dtRows: ref dtRows,
                        totalRows: ref totalRows
                    );

                    // If no rows
                    if (dtRows == null)
                    {
                        var empty = new { rows = new object[] { }, totalRows = 0 };
                        return JsonConvert.SerializeObject(empty);
                    }

                    // IMPORTANT:
                    // DataTable JSON becomes List<dynamic> => Flutter reads it easy
                    var payload = new
                    {
                        rows = dtRows,
                        totalRows = totalRows
                    };

                    return JsonConvert.SerializeObject(payload);
                }
                catch
                {
                    throw;
                }
            }
        }
    }