using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System;
using System.Data;
using WebApplication2.cls;

namespace WebApplication2.Controllers
{
    [Route("api/ctlCRMActivity")]
    public class ctlCRMActivity : Controller
    {
        [HttpGet]
        [Route("SelectCRMActivityByID")]
        public string SelectCRMActivityByID(int ID, int OpportunityID, int CompanyID)
        {
            clsCRMActivity cls = new clsCRMActivity();
            DataTable dt = cls.SelectCRMActivityByID(ID, OpportunityID, CompanyID);
            return dt != null ? JsonConvert.SerializeObject(dt) : "";
        }

        [HttpPost]
        [Route("InsertCRMActivity")]
        public int InsertCRMActivity(int OpportunityID, string ActivityType, string Subject,
            DateTime DueDate, bool IsDone, string Notes, int AssignedUserID,
            int CompanyID, int CreationUserID)
        {
            clsCRMActivity cls = new clsCRMActivity();
            return cls.InsertCRMActivity(OpportunityID, Simulate.String(ActivityType),
                Simulate.String(Subject), DueDate, IsDone, Simulate.String(Notes),
                AssignedUserID, CompanyID, CreationUserID);
        }

        [HttpPost]
        [Route("UpdateCRMActivity")]
        public int UpdateCRMActivity(int ID, int OpportunityID, string ActivityType, string Subject,
            DateTime DueDate, bool IsDone, string Notes, int AssignedUserID,
            int ModificationUserID, int CompanyID)
        {
            clsCRMActivity cls = new clsCRMActivity();
            return cls.UpdateCRMActivity(ID, OpportunityID, Simulate.String(ActivityType),
                Simulate.String(Subject), DueDate, IsDone, Simulate.String(Notes),
                AssignedUserID, ModificationUserID, CompanyID);
        }

        [HttpPost]
        [Route("DeleteCRMActivityByID")]
        public bool DeleteCRMActivityByID(int ID, int CompanyID)
        {
            clsCRMActivity cls = new clsCRMActivity();
            return cls.DeleteCRMActivityByID(ID, CompanyID);
        }
    }
}
