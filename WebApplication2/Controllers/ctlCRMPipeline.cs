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
    }
}
