using System;
using System.Data;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using WebApplication2.cls;

namespace WebApplication2.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ctlLiveLogsController : ControllerBase
    {
        /// <summary>
        /// API used by Flutter:
        /// ClsLiveLogs().loadLiveLogs()
        /// Example URL:
        /// /api/ctlLiveLogs/SelectLiveLogs?CompanyID=1043&TopN=200
        /// </summary>
        [HttpGet]
        [Route("SelectLiveLogs")]
        public string SelectLiveLogs(
            int CompanyID,
            int TopN = 200,
            string DateFrom = "",
            string DateTo = ""
        )
        {
            try
            {
                clsLiveLogs obj = new clsLiveLogs();

                DataTable dt = obj.SelectLiveLogs(
                    CompanyID,
                    TopN,
                    DateFrom,
                    DateTo
                );

                if (dt == null || dt.Rows.Count == 0)
                    return "[]";

                // Return JSON array that Flutter maps to LiveLogItem
                return JsonConvert.SerializeObject(dt);
            }
            catch (Exception ex)
            {
                // You can also wrap this in your standard error handling
                throw;
            }
        }
    }
}
