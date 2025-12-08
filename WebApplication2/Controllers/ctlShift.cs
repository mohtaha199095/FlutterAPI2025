using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System;
using System.Data;
using WebApplication2.cls;

namespace WebApplication2.Controllers
{
    [Route("api/ctlShift")]
    public class ctlShift : Controller
    {
        // ==========================================================
        // SELECT SHIFT
        // ==========================================================
        [HttpGet]
        [Route("SelectShiftByID")]
        public string SelectShiftByID(int ID, int CompanyID)
        {
            try
            {
                clsShift obj = new clsShift();

                DataTable dt = obj.SelectShiftByID(ID, CompanyID);

                return dt != null ? JsonConvert.SerializeObject(dt) : "";
            }
            catch
            {
                throw;
            }
        }

        // ==========================================================
        // SELECT SHIFT DETAILS
        // ==========================================================
        [HttpGet]
        [Route("SelectShiftDetails")]
        public string SelectShiftDetails(int ShiftID, int CompanyID)
        {
            try
            {
                clsShift obj = new clsShift();

                DataTable dt = obj.SelectShiftDetails(ShiftID, CompanyID);

                return dt != null ? JsonConvert.SerializeObject(dt) : "";
            }
            catch
            {
                throw;
            }
        }

        // ==========================================================
        // INSERT SHIFT (WITH DETAILS IN BODY)
        // ==========================================================
        [HttpPost]
        [Route("InsertShift")]
        public int InsertShift(
            string AName,
            string EName,
            string StartTime,
            string EndTime,
            int GraceEarly,
            int GraceLate,
            int BreakMinutes,
            bool IsOvernight,
            bool IsUseDetails,
            int CreationUserID,
            int CompanyID,
            [FromBody] dynamic body
        )
        {
            try
            {
                clsShift obj = new clsShift();

                string json = body.ToString(); // Full JSON string
                var jsonObj = Newtonsoft.Json.Linq.JObject.Parse(json);
                string jsonDetails = jsonObj["Details"]!.ToString();

                return obj.InsertShift(
                    Simulate.String(AName),
                    Simulate.String(EName),
                    Simulate.String(StartTime),
                    Simulate.String(EndTime),
                    GraceEarly,
                    GraceLate,
                    BreakMinutes,
                    IsOvernight,
                    IsUseDetails,
                    jsonDetails,
                    CompanyID,
                    CreationUserID
                );
            }
            catch
            {
                throw;
            }
        }

        // ==========================================================
        // UPDATE SHIFT
        // ==========================================================
        [HttpPost]
        [Route("UpdateShift")]
        public int UpdateShift(
            int ID,
            string AName,
            string EName,
            string StartTime,
            string EndTime,
            int GraceEarly,
            int GraceLate,
            int BreakMinutes,
            bool IsOvernight,
            bool IsUseDetails,
            int ModificationUserID,
            int CompanyID,
            [FromBody] dynamic body
        )
        {
            try
            {
                clsShift obj = new clsShift();
                string json = body.ToString(); // Full JSON string
                var jsonObj = Newtonsoft.Json.Linq.JObject.Parse(json);
                string jsonDetails = jsonObj["Details"]!.ToString();

                return obj.UpdateShift(
                    ID,
                    Simulate.String(AName),
                    Simulate.String(EName),
                    Simulate.String(StartTime),
                    Simulate.String(EndTime),
                    GraceEarly,
                    GraceLate,
                    BreakMinutes,
                    IsOvernight,
                    IsUseDetails,
                    jsonDetails,
                    ModificationUserID,
                    CompanyID
                );
            }
            catch
            {
                throw;
            }
        }

        // ==========================================================
        // DELETE SHIFT
        // ==========================================================
        [HttpGet]
        [Route("DeleteShift")]
        public int DeleteShift(int ID, int CompanyID)
        {
            try
            {
                clsShift obj = new clsShift();
                return obj.DeleteShift(ID, CompanyID);
            }
            catch
            {
                throw;
            }
        }
    }
}
