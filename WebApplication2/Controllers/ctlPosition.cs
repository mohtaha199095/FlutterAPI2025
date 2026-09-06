using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System;
using System.Data;
using WebApplication2.cls;

namespace WebApplication2.Controllers
{
    [Route("api/ctlPosition")]
    public class ctlPosition : Controller
    {
        [HttpGet]
        [Route("SelectPositionByID")]
        public string SelectPositionByID(int ID, int CompanyID)
        {
            clsPosition cls = new clsPosition();
            DataTable dt = cls.SelectPositionByID(ID, "", "", CompanyID);
            return dt == null ? "" : JsonConvert.SerializeObject(dt);
        }

        [HttpPost]
        [Route("DeletePositionByID")]
        public bool DeletePositionByID(int ID, int CompanyID)
        {
            return new clsPosition().DeletePositionByID(ID, CompanyID);
        }

        [HttpPost]
        [Route("InsertPosition")]
        public int InsertPosition(string AName, string EName, int CompanyID, int CreationUserId)
        {
            return new clsPosition().InsertPosition(Simulate.String(AName), Simulate.String(EName), CompanyID, CreationUserId);
        }

        [HttpPost]
        [Route("UpdatePosition")]
        public int UpdatePosition(int ID, string AName, string EName, int CompanyID, int ModificationUserId)
        {
            return new clsPosition().UpdatePosition(ID, Simulate.String(AName), Simulate.String(EName), ModificationUserId, CompanyID);
        }
    }
}
