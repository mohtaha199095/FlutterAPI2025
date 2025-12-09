using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System;
using System.Data;
using WebApplication2.cls;

namespace WebApplication2.Controllers
{
    [Route("api/ctlAttendance")]
    public class ctlAttendance : Controller
    {
        // ==========================================================
        // SELECT ATTENDANCE DAYS (Summary Results)
        // ==========================================================
        [HttpGet]
        [Route("GetAttendanceDays")]
        public string GetAttendanceDays(
            int EmployeeID,
            string DateFrom,
            string DateTo,
            int CompanyID
        )
        {
            try
            {
                clsAttendanceDay obj = new clsAttendanceDay();

                DataTable dt = obj.SelectAttendanceDays(
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
        // RECALCULATE ATTENDANCE FOR RANGE (Optional Feature)
        // ==========================================================
        [HttpGet]
        [Route("RecalculateAttendance")]
        public string RecalculateAttendance(
            int EmployeeID,
            string DateFrom,
            string DateTo,
            int CompanyID,
            int UserID
        )
        {
            try
            {
                clsAttendanceDay obj = new clsAttendanceDay();
                bool result = obj.RecalculateAttendance(
                    EmployeeID,
                    Simulate.String(DateFrom),
                    Simulate.String(DateTo),
                    CompanyID,
                    UserID
                );

                return result ? "1" : "0";
            }
            catch
            {
                throw;
            }
        }

        // ==========================================================
        // DELETE DAY RECORD (Optional Admin Feature)
        // ==========================================================
        [HttpGet]
        [Route("DeleteAttendanceDay")]
        public string DeleteAttendanceDay(
            int ID,
            int CompanyID
        )
        {
            try
            {
                clsAttendanceDay obj = new clsAttendanceDay();
                bool result = obj.DeleteAttendanceDay(ID, CompanyID);

                return result ? "1" : "0";
            }
            catch
            {
                throw;
            }
        }
    }
}
