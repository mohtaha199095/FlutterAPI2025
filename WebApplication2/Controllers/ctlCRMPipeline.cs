using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Data;
using WebApplication2.cls;

namespace WebApplication2.Controllers
{
    [Route("api/ctlCRMPipeline")]
    public class ctlCRMPipeline : Controller
    {
        [HttpGet]
        [Route("SelectCRMPipeline")]
        public string SelectCRMPipeline(int ID, int CompanyID)
        {
            clsCRMPipeline cls = new clsCRMPipeline();
            cls.EnsureDefaultPipeline(CompanyID, 1);
            DataTable dt = cls.SelectCRMPipeline(ID, CompanyID);
            return dt != null ? JsonConvert.SerializeObject(dt) : "";
        }

        [HttpPost]
        [Route("InsertCRMPipeline")]
        public int InsertCRMPipeline(string AName, string EName, bool IsDefault, int CompanyID, int CreationUserID)
        {
            clsCRMPipeline cls = new clsCRMPipeline();
            return cls.InsertCRMPipeline(Simulate.String(AName), Simulate.String(EName), IsDefault, CompanyID, CreationUserID);
        }

        [HttpPost]
        [Route("UpdateCRMPipeline")]
        public int UpdateCRMPipeline(int ID, string AName, string EName, bool IsDefault, bool IsActive,
            int ModificationUserID, int CompanyID)
        {
            clsCRMPipeline cls = new clsCRMPipeline();
            return cls.UpdateCRMPipeline(ID, Simulate.String(AName), Simulate.String(EName), IsDefault, IsActive,
                ModificationUserID, CompanyID);
        }

        [HttpPost]
        [Route("DeleteCRMPipelineByID")]
        public bool DeleteCRMPipelineByID(int ID, int CompanyID)
        {
            clsCRMPipeline cls = new clsCRMPipeline();
            return cls.DeleteCRMPipelineByID(ID, CompanyID);
        }
    }
}
