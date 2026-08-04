using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System;
using System.Data;
using WebApplication2.cls;

namespace WebApplication2.Controllers
{
    [Route("api/ctlCRMJourneyStage")]
    public class ctlCRMJourneyStage : Controller
    {
        [HttpGet]
        [Route("SelectCRMJourneyStageByID")]
        public string SelectCRMJourneyStageByID(int ID, int PipelineID, int CompanyID)
        {
            clsCRMJourneyStage cls = new clsCRMJourneyStage();
            DataTable dt = cls.SelectCRMJourneyStageByID(ID, PipelineID, CompanyID);
            return dt != null ? JsonConvert.SerializeObject(dt) : "";
        }

        [HttpPost]
        [Route("InsertCRMJourneyStage")]
        public int InsertCRMJourneyStage(int PipelineID, string AName, string EName, int StageOrder,
            string Color, bool IsWon, bool IsLost, bool IsDefault, int CompanyID, int CreationUserID)
        {
            if (PipelineID <= 0)
            {
                clsCRMPipeline pipeline = new clsCRMPipeline();
                PipelineID = pipeline.EnsureDefaultPipeline(CompanyID, CreationUserID);
            }
            clsCRMJourneyStage cls = new clsCRMJourneyStage();
            return cls.InsertCRMJourneyStage(PipelineID, Simulate.String(AName), Simulate.String(EName),
                StageOrder, Simulate.String(Color), IsWon, IsLost, IsDefault, CompanyID, CreationUserID);
        }

        [HttpPost]
        [Route("UpdateCRMJourneyStage")]
        public int UpdateCRMJourneyStage(int ID, string AName, string EName, int StageOrder, string Color,
            bool IsWon, bool IsLost, bool IsDefault, bool IsActive, int ModificationUserID, int CompanyID)
        {
            clsCRMJourneyStage cls = new clsCRMJourneyStage();
            return cls.UpdateCRMJourneyStage(ID, Simulate.String(AName), Simulate.String(EName),
                StageOrder, Simulate.String(Color), IsWon, IsLost, IsDefault, IsActive,
                ModificationUserID, CompanyID);
        }

        [HttpPost]
        [Route("DeleteCRMJourneyStageByID")]
        public bool DeleteCRMJourneyStageByID(int ID, int CompanyID)
        {
            clsCRMJourneyStage cls = new clsCRMJourneyStage();
            return cls.DeleteCRMJourneyStageByID(ID, CompanyID);
        }

        [HttpPost]
        [Route("ReorderCRMJourneyStages")]
        public bool ReorderCRMJourneyStages(string OrderedStageIds, int CompanyID, int ModificationUserID)
        {
            clsCRMJourneyStage cls = new clsCRMJourneyStage();
            return cls.ReorderCRMJourneyStages(Simulate.String(OrderedStageIds), CompanyID, ModificationUserID);
        }
    }
}
