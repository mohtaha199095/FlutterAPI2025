using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System;
using System.Data;
using WebApplication2.cls;

namespace WebApplication2.Controllers
{
    [Route("api/ctlLeave")]
    public class ctlLeave : Controller
    {
        // ---------- Leave Types ----------
        [HttpGet]
        [Route("SelectLeaveTypes")]
        public string SelectLeaveTypes(int ID = 0, string Code = "", int CompanyID = 0, int ActiveOnly = 0)
        {
            DataTable dt = new clsLeave().SelectLeaveTypes(ID, Simulate.String(Code), CompanyID, ActiveOnly);
            return dt == null ? "[]" : JsonConvert.SerializeObject(dt);
        }

        [HttpPost]
        [Route("InsertLeaveType")]
        public int InsertLeaveType(string Code, string AName, string EName, bool IsPaid, bool IsActive,
            int AccrualRuleID, int CompanyID, int CreationUserID)
        {
            return new clsLeave().InsertLeaveType(
                Simulate.String(Code), Simulate.String(AName), Simulate.String(EName),
                IsPaid, IsActive, AccrualRuleID, CompanyID, CreationUserID);
        }

        [HttpPost]
        [Route("UpdateLeaveType")]
        public int UpdateLeaveType(int ID, string Code, string AName, string EName, bool IsPaid, bool IsActive,
            int AccrualRuleID, int CompanyID, int ModificationUserID)
        {
            return new clsLeave().UpdateLeaveType(
                ID, Simulate.String(Code), Simulate.String(AName), Simulate.String(EName),
                IsPaid, IsActive, AccrualRuleID, CompanyID, ModificationUserID);
        }

        [HttpPost]
        [Route("DeleteLeaveType")]
        public bool DeleteLeaveType(int ID, int CompanyID)
        {
            return new clsLeave().DeleteLeaveType(ID, CompanyID);
        }

        // ---------- Holiday Calendar ----------
        [HttpGet]
        [Route("SelectHolidayCalendars")]
        public string SelectHolidayCalendars(int ID = 0, int Year = 0, int CompanyID = 0)
        {
            DataTable dt = new clsLeave().SelectHolidayCalendars(ID, Year, CompanyID);
            return dt == null ? "[]" : JsonConvert.SerializeObject(dt);
        }

        [HttpPost]
        [Route("InsertHolidayCalendar")]
        public int InsertHolidayCalendar(string AName, string EName, int Year, int CompanyID, int CreationUserID)
        {
            return new clsLeave().InsertHolidayCalendar(
                Simulate.String(AName), Simulate.String(EName), Year, CompanyID, CreationUserID);
        }

        [HttpPost]
        [Route("UpdateHolidayCalendar")]
        public int UpdateHolidayCalendar(int ID, string AName, string EName, int Year, int CompanyID, int ModificationUserID)
        {
            return new clsLeave().UpdateHolidayCalendar(
                ID, Simulate.String(AName), Simulate.String(EName), Year, CompanyID, ModificationUserID);
        }

        [HttpPost]
        [Route("DeleteHolidayCalendar")]
        public bool DeleteHolidayCalendar(int ID, int CompanyID)
        {
            return new clsLeave().DeleteHolidayCalendar(ID, CompanyID);
        }

        // ---------- Holidays ----------
        [HttpGet]
        [Route("SelectHolidays")]
        public string SelectHolidays(int ID = 0, int CalendarID = 0, int CompanyID = 0)
        {
            DataTable dt = new clsLeave().SelectHolidays(ID, CalendarID, CompanyID);
            return dt == null ? "[]" : JsonConvert.SerializeObject(dt);
        }

        [HttpPost]
        [Route("InsertHoliday")]
        public int InsertHoliday(int CalendarID, DateTime HolidayDate, string AName, string EName,
            bool IsPaid, int CompanyID, int CreationUserID)
        {
            return new clsLeave().InsertHoliday(
                CalendarID, HolidayDate, Simulate.String(AName), Simulate.String(EName),
                IsPaid, CompanyID, CreationUserID);
        }

        [HttpPost]
        [Route("UpdateHoliday")]
        public int UpdateHoliday(int ID, int CalendarID, DateTime HolidayDate, string AName, string EName,
            bool IsPaid, int CompanyID, int ModificationUserID)
        {
            return new clsLeave().UpdateHoliday(
                ID, CalendarID, HolidayDate, Simulate.String(AName), Simulate.String(EName),
                IsPaid, CompanyID, ModificationUserID);
        }

        [HttpPost]
        [Route("DeleteHoliday")]
        public bool DeleteHoliday(int ID, int CompanyID)
        {
            return new clsLeave().DeleteHoliday(ID, CompanyID);
        }

        // ---------- Balances ----------
        [HttpGet]
        [Route("SelectLeaveBalances")]
        public string SelectLeaveBalances(int ID = 0, int EmployeeID = 0, int LeaveTypeID = 0, int Year = 0, int CompanyID = 0)
        {
            DataTable dt = new clsLeave().SelectLeaveBalances(ID, EmployeeID, LeaveTypeID, Year, CompanyID);
            return dt == null ? "[]" : JsonConvert.SerializeObject(dt);
        }

        [HttpPost]
        [Route("UpsertLeaveBalance")]
        public int UpsertLeaveBalance(int EmployeeID, int LeaveTypeID, int Year,
            decimal EntitledDays, decimal UsedDays, decimal PendingDays, int CompanyID, int UserID)
        {
            return new clsLeave().UpsertLeaveBalance(
                EmployeeID, LeaveTypeID, Year, EntitledDays, UsedDays, PendingDays, CompanyID, UserID);
        }

        [HttpPost]
        [Route("SeedBalancesFromContract")]
        public IActionResult SeedBalancesFromContract(int EmployeeID, int CompanyID, int UserID = 1)
        {
            try
            {
                new clsLeave().SeedBalancesFromContract(EmployeeID, CompanyID, UserID);
                return Ok(new { success = true });
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        // ---------- Leave Requests ----------
        [HttpGet]
        [Route("SelectLeaveRequests")]
        public string SelectLeaveRequests(int ID = 0, int EmployeeID = 0, int DocumentStatus = -1, int CompanyID = 0, string Guid = "", int Year = 0)
        {
            DataTable dt = new clsLeave().SelectLeaveRequests(ID, EmployeeID, DocumentStatus, CompanyID, Simulate.String(Guid), Year);
            return dt == null ? "[]" : JsonConvert.SerializeObject(dt);
        }

        [HttpGet]
        [Route("SelectLeaveRequestGuid")]
        public string SelectLeaveRequestGuid(int ID, int CompanyID)
        {
            return new clsLeave().SelectLeaveRequestGuid(ID, CompanyID);
        }

        [HttpPost]
        [Route("InsertLeaveRequest")]
        public int InsertLeaveRequest(int EmployeeID, int LeaveTypeID, DateTime FromDate, DateTime ToDate,
            decimal Days, string Reason, int BranchID, int CompanyID, int CreationUserID, int DocumentStatus = 0)
        {
            return new clsLeave().InsertLeaveRequest(
                EmployeeID, LeaveTypeID, FromDate, ToDate, Days, Simulate.String(Reason),
                BranchID, CompanyID, CreationUserID, DocumentStatus);
        }

        [HttpPost]
        [Route("UpdateLeaveRequest")]
        public int UpdateLeaveRequest(int ID, int EmployeeID, int LeaveTypeID, DateTime FromDate, DateTime ToDate,
            decimal Days, string Reason, int BranchID, int CompanyID, int ModificationUserID)
        {
            return new clsLeave().UpdateLeaveRequest(
                ID, EmployeeID, LeaveTypeID, FromDate, ToDate, Days, Simulate.String(Reason),
                BranchID, CompanyID, ModificationUserID);
        }

        [HttpPost]
        [Route("DeleteLeaveRequest")]
        public bool DeleteLeaveRequest(int ID, int CompanyID)
        {
            return new clsLeave().DeleteLeaveRequest(ID, CompanyID);
        }

        [HttpPost]
        [Route("ApproveLeaveRequest")]
        public IActionResult ApproveLeaveRequest(string Guid, int UserID, int CompanyID)
        {
            try
            {
                bool ok = new clsLeave().ApproveLeaveRequest(Simulate.String(Guid), UserID, CompanyID);
                return Ok(new { success = ok });
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        [HttpPost]
        [Route("SeedJordanHolidays")]
        public IActionResult SeedJordanHolidays(int Year, int CompanyID, int UserID = 1)
        {
            try
            {
                int count = new clsLeave().SeedJordanPublicHolidays(Year, CompanyID, UserID);
                return Ok(new { success = true, inserted = count });
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        [HttpPost]
        [Route("AccrueLeaveBalances")]
        public IActionResult AccrueLeaveBalances(int CompanyID, int Year = 0, int UserID = 1)
        {
            try
            {
                new clsLeave().AccrueLeaveBalancesForYear(CompanyID, Year, UserID);
                return Ok(new { success = true });
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        [HttpPost]
        [Route("RejectLeaveRequest")]
        public IActionResult RejectLeaveRequest(string Guid, int UserID, int CompanyID)
        {
            try
            {
                bool ok = new clsLeave().RejectLeaveRequest(Simulate.String(Guid), UserID, CompanyID);
                return Ok(new { success = ok });
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        [HttpPost]
        [Route("ProcessLeaveEncashment")]
        public IActionResult ProcessLeaveEncashment(int EmployeeID, int LeaveTypeID, decimal Days, int Year,
            int CompanyID, int UserID = 1, int BranchID = 1, bool PostJournal = true)
        {
            try
            {
                var result = new clsLeave().ProcessLeaveEncashment(
                    EmployeeID, LeaveTypeID, Days, Year, CompanyID, UserID, BranchID, PostJournal);
                return Ok(new { success = true, encashmentAmount = result.Amount, jvGuid = result.JvGuid });
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        [HttpGet]
        [Route("SelectLeaveRequestByGuid")]
        public string SelectLeaveRequestByGuid(string Guid, int CompanyID)
        {
            DataTable dt = new clsLeave().SelectLeaveRequests(0, 0, -1, CompanyID, Simulate.String(Guid));
            return dt == null || dt.Rows.Count == 0 ? "{}" : JsonConvert.SerializeObject(dt.Rows[0]);
        }
    }
}
