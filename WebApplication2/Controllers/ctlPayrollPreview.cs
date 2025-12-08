using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System;
using System.Data;
using WebApplication2.cls;

namespace WebApplication2.Controllers
{
    [Route("api/ctlPayrollPreview")]
    public class ctlPayrollPreview : Controller
    {
        // ==========================================================
        // PREVIEW PAYROLL (NO SAVE)
        // ==========================================================
        [HttpGet]
        [Route("PreviewPayroll")]
        public IActionResult PreviewPayroll(int EmployeeID, int PayrollPeriodID, int CompanyID)
        {
            try
            {
                clsPayrollEngine engine = new clsPayrollEngine();
                PayrollPreviewResult result = engine.PreviewPayroll(EmployeeID, PayrollPeriodID, CompanyID);

                if (result == null)
                    return Json(new { });

                return Json(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }
        [HttpGet]
        [Route("PreviewAll")]
        public string PreviewAll(int PayrollPeriodID, int DepartmentID, int CompanyID)
        {
            try
            {
                clsPayrollEngine eng = new clsPayrollEngine();
                DataTable dt = eng.PreviewPayrollAll(PayrollPeriodID, DepartmentID, CompanyID);

                return JsonConvert.SerializeObject(dt);
            }
            catch
            {
                throw;
            }
        }
    }
}
