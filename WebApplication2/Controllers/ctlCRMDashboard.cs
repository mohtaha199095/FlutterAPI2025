using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Data;
using WebApplication2.cls;

namespace WebApplication2.Controllers
{
    [Route("api/ctlCRMDashboard")]
    public class ctlCRMDashboard : Controller
    {
        [HttpGet]
        [Route("GetCRMDashboard")]
        public string GetCRMDashboard(int PipelineID, int CompanyID, int MonthsBack = 6)
        {
            if (PipelineID <= 0)
            {
                clsCRMPipeline pipeline = new clsCRMPipeline();
                PipelineID = pipeline.EnsureDefaultPipeline(CompanyID, 1);
            }

            clsCRMDashboard dash = new clsCRMDashboard();
            DataTable summary = dash.SelectCRMDashboardSummary(PipelineID, CompanyID);
            DataTable activities = dash.SelectCRMPendingActivities(CompanyID);
            DataTable byStage = dash.SelectCRMDealsByStage(PipelineID, CompanyID);
            DataTable bySource = dash.SelectCRMDealsBySource(PipelineID, CompanyID);
            DataTable monthly = dash.SelectCRMMonthlyTrend(PipelineID, CompanyID, MonthsBack);
            DataTable recentWon = dash.SelectCRMRecentWonDeals(PipelineID, CompanyID, 5);

            var result = new
            {
                PipelineID,
                Summary = summary,
                Activities = activities,
                ByStage = byStage,
                BySource = bySource,
                MonthlyTrend = monthly,
                RecentWon = recentWon,
            };
            return JsonConvert.SerializeObject(result);
        }
    }
}
