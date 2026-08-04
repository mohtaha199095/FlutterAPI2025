using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Data;
using WebApplication2.cls;

namespace WebApplication2.Controllers
{
    [Route("api/ctlPayroll")]
    public class ctlPayroll : Controller
    {
    //[HttpGet]
    //[Route("RunPayroll")]
    //public string RunPayroll(int PayrollPeriodID, int CompanyID, int UserID )
    //{
    //    try
    //    {
    //        clsPayrollEngine eng = new clsPayrollEngine();
    //        DataTable dt = eng.RunPayroll(PayrollPeriodID, CompanyID, UserID );

    //        return JsonConvert.SerializeObject(dt);
    //    }
    //    catch (Exception ex)
    //    {
    //        return ex.Message;
    //    }
    //}
    [HttpGet]
    [Route("GetPayrollPostingData")]
    public IActionResult GetPayrollPostingData(int periodId, int companyId)
    {
        try
        {
            var svc = new clsPayrollPostingService();
            var result = svc.LoadEmployeesForPosting(periodId, companyId);

            return Ok(result);
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }
    [HttpPost]
    [Route("PostPayroll")]
    public IActionResult PostPayroll(
        [FromQuery] int PeriodID,
        [FromQuery] int CompanyID,
        [FromQuery] int BranchID,
        [FromQuery] int UserID,
        [FromBody] List<int> EmployeeIDs)
    {
        try
        {
            if (EmployeeIDs == null || EmployeeIDs.Count == 0)
                return BadRequest("No employees in body.");
            clsPayrollPostingRequest req = new clsPayrollPostingRequest();
            req.PeriodID = PeriodID;
            req.BranchID = BranchID;
            req.UserID = UserID;
            req.CompanyID = CompanyID;
            req.EmployeeIDs = EmployeeIDs;
            var svc = new clsPayrollPostingService();
            var result = svc.PostPayrollBatch(req);

            return Ok(result);
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }
    [HttpPost]
    [Route("CancelPosting")]
    public IActionResult CancelPosting(int periodId, int EmployeeID, int companyId)
    {
        try
        {
            clsPayrollPostingService svc = new clsPayrollPostingService();
            string status = svc.CancelPayrollPosting_HardDelete(periodId, EmployeeID, companyId);
            return Ok(new { status });
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }

    }
}
