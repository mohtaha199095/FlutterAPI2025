using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using WebApplication2.cls;

[Route("api/ctlPayroll")]
public class ctlPayroll : Controller
{
    [HttpGet]
    [Route("RunPayroll")]
    public string RunPayroll(int PayrollPeriodID, int CompanyID, int UserID )
    {
        try
        {
            clsPayrollEngine eng = new clsPayrollEngine();
            DataTable dt = eng.RunPayroll(PayrollPeriodID, CompanyID, UserID );

            return JsonConvert.SerializeObject(dt);
        }
        catch (Exception ex)
        {
            return ex.Message;
        }
    }
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
    public IActionResult PostPayroll( int PeriodID,  int CompanyID, int BranchID, int UserID, [FromBody] string EmployeeIDs)
    {
        try
        {
            List<int> details = JsonConvert.DeserializeObject<List<int>>(EmployeeIDs);
            clsPayrollPostingRequest req = new clsPayrollPostingRequest();
            req.PeriodID = PeriodID;
            req.BranchID = BranchID;
            req.UserID = UserID;
            req.CompanyID = CompanyID;
             req.EmployeeIDs =details;
            var svc = new clsPayrollPostingService();
            var result = svc.PostPayrollBatch(req);

            return Ok(result);
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }
}
