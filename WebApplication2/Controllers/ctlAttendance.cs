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
        [HttpPost]
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
        [HttpPost]
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

        // ==========================================================
        // GET MONTH (Timesheet view: 1 row per day of the month)
        // Composes attendance day rows with raw punches + shift to
        // emit the shape Flutter's TimesheetDayModel expects.
        // ==========================================================
        [HttpGet]
        [Route("GetMonth")]
        public string GetMonth(int EmployeeID, int Year, int Month, int CompanyID)
        {
            try
            {
                if (EmployeeID <= 0 || Year <= 0 || Month <= 0 || Month > 12)
                    return "[]";

                DateTime first = new DateTime(Year, Month, 1);
                DateTime last = first.AddMonths(1).AddDays(-1);

                string dateFrom = first.ToString("yyyy-MM-dd");
                string dateTo = last.ToString("yyyy-MM-dd");

                clsAttendanceDay att = new clsAttendanceDay();
                DataTable dtDays = att.SelectAttendanceDays(EmployeeID, dateFrom, dateTo, CompanyID);

                // Index calculated days by date for O(1) lookup
                var byDate = new System.Collections.Generic.Dictionary<int, DataRow>();
                if (dtDays != null)
                {
                    foreach (DataRow row in dtDays.Rows)
                    {
                        DateTime workDate = Simulate.StringToDate(row["WorkDate"]);
                        if (workDate.Year == Year && workDate.Month == Month)
                            byDate[workDate.Day] = row;
                    }
                }

                // Build one row per day of month
                DataTable result = new DataTable();
                result.Columns.Add("Day", typeof(int));
                result.Columns.Add("ShiftId", typeof(int));
                result.Columns.Add("Status", typeof(string));
                result.Columns.Add("WorkedHours", typeof(decimal));
                result.Columns.Add("OvertimeHours", typeof(decimal));
                result.Columns.Add("LateMinutes", typeof(decimal));
                result.Columns.Add("EarlyLeaveMinutes", typeof(decimal));
                result.Columns.Add("ExpectedHours", typeof(decimal));

                int daysInMonth = DateTime.DaysInMonth(Year, Month);
                for (int d = 1; d <= daysInMonth; d++)
                {
                    DataRow r = result.NewRow();
                    r["Day"] = d;

                    if (byDate.TryGetValue(d, out DataRow src))
                    {
                        int statusID = Simulate.Integer32(src["StatusID"]);
                        int workedMin = Simulate.Integer32(src["WorkedMinutes"]);
                        int overtimeMin = Simulate.Integer32(src["OvertimeMinutes"]);
                        int lateMin = Simulate.Integer32(src["LateMinutes"]);
                        int earlyMin = Simulate.Integer32(src["EarlyLeaveMinutes"]);

                        r["ShiftId"] = Simulate.Integer32(src["ShiftID"]);
                        r["Status"] = MapStatus(statusID);
                        r["WorkedHours"] = workedMin / 60.0m;
                        r["OvertimeHours"] = overtimeMin / 60.0m;
                        r["LateMinutes"] = (decimal)lateMin;
                        r["EarlyLeaveMinutes"] = (decimal)earlyMin;
                        r["ExpectedHours"] = 0m;
                    }
                    else
                    {
                        r["ShiftId"] = 0;
                        r["Status"] = "";
                        r["WorkedHours"] = 0m;
                        r["OvertimeHours"] = 0m;
                        r["LateMinutes"] = 0m;
                        r["EarlyLeaveMinutes"] = 0m;
                        r["ExpectedHours"] = 0m;
                    }

                    result.Rows.Add(r);
                }

                return JsonConvert.SerializeObject(result);
            }
            catch
            {
                throw;
            }
        }

        private static string MapStatus(int statusID)
        {
            // tbl_AttendanceDay.StatusID: 1=Present, 2=Absent, 3=Leave, 4=Offday
            switch (statusID)
            {
                case 1: return "Present";
                case 2: return "Absent";
                case 3: return "Leave";
                case 4: return "Offday";
                default: return "";
            }
        }
    }
}
