using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System;
using System.Data;
using WebApplication2.cls;

namespace WebApplication2.Controllers
{
    [Route("api/ctlCRMReport")]
    public class ctlCRMReport : Controller
    {
        private int ResolvePipeline(int pipelineID, int companyID)
        {
            if (pipelineID > 0) return pipelineID;
            clsCRMPipeline pipeline = new clsCRMPipeline();
            return pipeline.EnsureDefaultPipeline(companyID, 1);
        }

        [HttpGet]
        [Route("SelectPipelineSummaryReport")]
        public string SelectPipelineSummaryReport(int PipelineID, int CompanyID,
            DateTime DateFrom, DateTime DateTo)
        {
            PipelineID = ResolvePipeline(PipelineID, CompanyID);
            clsCRMReport cls = new clsCRMReport();
            DataTable dt = cls.SelectPipelineSummaryReport(PipelineID, CompanyID, DateFrom, DateTo);
            return dt != null ? JsonConvert.SerializeObject(dt) : "";
        }

        [HttpGet]
        [Route("SelectWonLostReport")]
        public string SelectWonLostReport(int PipelineID, int CompanyID,
            DateTime DateFrom, DateTime DateTo)
        {
            PipelineID = ResolvePipeline(PipelineID, CompanyID);
            clsCRMReport cls = new clsCRMReport();
            DataTable dt = cls.SelectWonLostReport(PipelineID, CompanyID, DateFrom, DateTo);
            return dt != null ? JsonConvert.SerializeObject(dt) : "";
        }

        [HttpGet]
        [Route("SelectActivityReport")]
        public string SelectActivityReport(int CompanyID, int OpportunityID,
            DateTime DateFrom, DateTime DateTo)
        {
            clsCRMReport cls = new clsCRMReport();
            DataTable dt = cls.SelectActivityReport(CompanyID, OpportunityID, DateFrom, DateTo);
            return dt != null ? JsonConvert.SerializeObject(dt) : "";
        }

        [HttpGet]
        [Route("SelectSourceAnalysisReport")]
        public string SelectSourceAnalysisReport(int PipelineID, int CompanyID,
            DateTime DateFrom, DateTime DateTo)
        {
            PipelineID = ResolvePipeline(PipelineID, CompanyID);
            clsCRMReport cls = new clsCRMReport();
            DataTable dt = cls.SelectSourceAnalysisReport(PipelineID, CompanyID, DateFrom, DateTo);
            return dt != null ? JsonConvert.SerializeObject(dt) : "";
        }

        [HttpGet]
        [Route("SelectStageConversionReport")]
        public string SelectStageConversionReport(int PipelineID, int CompanyID,
            DateTime DateFrom, DateTime DateTo)
        {
            PipelineID = ResolvePipeline(PipelineID, CompanyID);
            clsCRMReport cls = new clsCRMReport();
            DataTable dt = cls.SelectStageConversionReport(PipelineID, CompanyID, DateFrom, DateTo);
            return dt != null ? JsonConvert.SerializeObject(dt) : "";
        }
    }
}
