using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System;
using System.Data;
using WebApplication2.cls;

[Route("api/ctlPayroll")]
public class ctlPayroll : Controller
{
    [HttpGet]
    [Route("RunPayroll")]
    public string RunPayroll(int PayrollPeriodID, int CompanyID, int UserID)
    {
        try
        {
            clsPayrollEngine eng = new clsPayrollEngine();
            DataTable dt = eng.RunPayroll(PayrollPeriodID, CompanyID, UserID);

            return JsonConvert.SerializeObject(dt);
        }
        catch (Exception ex)
        {
            return ex.Message;
        }
    }
}
