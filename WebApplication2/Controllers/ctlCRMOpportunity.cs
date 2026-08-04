using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System;
using System.Data;
using WebApplication2.cls;

namespace WebApplication2.Controllers
{
    [Route("api/ctlCRMOpportunity")]
    public class ctlCRMOpportunity : Controller
    {
        [HttpGet]
        [Route("SelectCRMOpportunityByID")]
        public string SelectCRMOpportunityByID(int ID, int PipelineID, int StageID, int CompanyID)
        {
            clsCRMOpportunity cls = new clsCRMOpportunity();
            DataTable dt = cls.SelectCRMOpportunityByID(ID, PipelineID, StageID, CompanyID);
            return dt != null ? JsonConvert.SerializeObject(dt) : "";
        }

        [HttpGet]
        [Route("SelectCRMOpportunityByStage")]
        public string SelectCRMOpportunityByStage(int PipelineID, int CompanyID)
        {
            if (PipelineID <= 0)
            {
                clsCRMPipeline pipeline = new clsCRMPipeline();
                PipelineID = pipeline.EnsureDefaultPipeline(CompanyID, 1);
            }
            clsCRMOpportunity cls = new clsCRMOpportunity();
            DataTable dt = cls.SelectCRMOpportunityByStage(PipelineID, CompanyID);
            return dt != null ? JsonConvert.SerializeObject(dt) : "";
        }

        [HttpGet]
        [Route("SelectCRMStageHistoryByOpportunity")]
        public string SelectCRMStageHistoryByOpportunity(int OpportunityID, int CompanyID)
        {
            clsCRMStageHistory cls = new clsCRMStageHistory();
            DataTable dt = cls.SelectCRMStageHistoryByOpportunity(OpportunityID, CompanyID);
            return dt != null ? JsonConvert.SerializeObject(dt) : "";
        }

        [HttpPost]
        [Route("InsertCRMOpportunity")]
        public int InsertCRMOpportunity(int PipelineID, int StageID, string Title, string AName, string EName,
            string Tel1, string Email, string Country, string Source, string Notes,
            int BusinessPartnerID, int AssignedUserID, decimal ExpectedValue, int CurrencyID,
            int Probability, DateTime ExpectedCloseDate, int Priority,
            int CompanyID, int CreationUserID)
        {
            clsCRMOpportunity cls = new clsCRMOpportunity();
            return cls.InsertCRMOpportunity(PipelineID, StageID, Simulate.String(Title),
                Simulate.String(AName), Simulate.String(EName), Simulate.String(Tel1),
                Simulate.String(Email), Simulate.String(Country), Simulate.String(Source),
                Simulate.String(Notes), BusinessPartnerID, AssignedUserID, ExpectedValue,
                CurrencyID, Probability, ExpectedCloseDate, Priority, CompanyID, CreationUserID);
        }

        [HttpPost]
        [Route("UpdateCRMOpportunity")]
        public int UpdateCRMOpportunity(int ID, int PipelineID, int StageID, string Title, string AName, string EName,
            string Tel1, string Email, string Country, string Source, string Notes,
            int BusinessPartnerID, int AssignedUserID, decimal ExpectedValue, int CurrencyID,
            int Probability, DateTime ExpectedCloseDate, int Priority, bool IsActive,
            int ModificationUserID, int CompanyID)
        {
            clsCRMOpportunity cls = new clsCRMOpportunity();
            return cls.UpdateCRMOpportunity(ID, PipelineID, StageID, Simulate.String(Title),
                Simulate.String(AName), Simulate.String(EName), Simulate.String(Tel1),
                Simulate.String(Email), Simulate.String(Country), Simulate.String(Source),
                Simulate.String(Notes), BusinessPartnerID, AssignedUserID, ExpectedValue,
                CurrencyID, Probability, ExpectedCloseDate, Priority, IsActive,
                ModificationUserID, CompanyID);
        }

        [HttpPost]
        [Route("DeleteCRMOpportunityByID")]
        public bool DeleteCRMOpportunityByID(int ID, int CompanyID)
        {
            clsCRMOpportunity cls = new clsCRMOpportunity();
            return cls.DeleteCRMOpportunityByID(ID, CompanyID);
        }

        [HttpPost]
        [Route("MoveCRMOpportunityStage")]
        public bool MoveCRMOpportunityStage(int OpportunityID, int ToStageID, int MovedByUserID, int CompanyID)
        {
            clsCRMOpportunity cls = new clsCRMOpportunity();
            return cls.MoveCRMOpportunityStage(OpportunityID, ToStageID, MovedByUserID, CompanyID);
        }

        [HttpPost]
        [Route("ConvertCRMOpportunityToBusinessPartner")]
        public int ConvertCRMOpportunityToBusinessPartner(int OpportunityID, int CompanyID, int CreationUserID)
        {
            clsCRMOpportunity cls = new clsCRMOpportunity();
            return cls.ConvertCRMOpportunityToBusinessPartner(OpportunityID, CompanyID, CreationUserID);
        }
    }
}
