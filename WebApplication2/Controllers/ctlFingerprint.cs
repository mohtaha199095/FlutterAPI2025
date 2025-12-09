using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System;
using System.Data;
using WebApplication2.cls;

namespace WebApplication2.Controllers
{
    [Route("api/ctlAttendanceRawPunch")]
    public class ctlAttendanceRawPunch : Controller
    {
        // ==========================================================
        // GET RAW PUNCHES
        // ==========================================================
        [HttpGet]
        [Route("SelectAttendanceRawPunch")]
        public string SelectAttendanceRawPunch(
            int EmployeeID,
            string DateFrom,
            string DateTo,
            int CompanyID
        )
        {
            try
            {
                clsAttendanceRawPunch obj = new clsAttendanceRawPunch();

                DataTable dt = obj.SelectAttendanceRawPunch(
                    EmployeeID,
                    Simulate.String(DateFrom),
                    Simulate.String(DateTo),
                    CompanyID
                );

                return dt != null ? JsonConvert.SerializeObject(dt) : "";
            }
            catch
            {
                throw;
            }
        }

        // ==========================================================
        // DELETE PUNCH
        // ==========================================================
        [HttpGet]
        [Route("DeleteAttendanceRawPunch")]
        public string DeleteAttendanceRawPunch(int ID, int CompanyID)
        {
            try
            {
                clsAttendanceRawPunch obj = new clsAttendanceRawPunch();

                bool success = obj.DeleteFromAttendanceRawPunch(ID, CompanyID);

                return success ? JsonConvert.SerializeObject(true)   : JsonConvert.SerializeObject(false)   ;
            }
            catch
            {
                throw;
            }
        }
    }
}
