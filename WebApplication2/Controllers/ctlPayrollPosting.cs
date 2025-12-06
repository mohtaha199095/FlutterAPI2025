using Microsoft.AspNetCore.Mvc;
using WebApplication2.cls;
using System;
using System.Collections.Generic;

namespace WebApplication2.Controllers
{
    [Route("api/Payroll")]
    [ApiController]
    public class PayrollController : ControllerBase
    {
        // =============================
        // POST: api/Payroll/Post
        // =============================
        [HttpPost("Post")]
        public IActionResult PostPayroll([FromBody] PayrollPostingRequest req)
        {
            if (req == null)
                return BadRequest(new { success = false, message = "Invalid request" });

            try
            {
                clsPayrollPosting posting = new clsPayrollPosting();
                var result = posting.PostSelectedEmployees(req);

                return Ok(new
                {
                    success = true,
                    message = "Payroll posted successfully",
                    journalNumber = result.JournalNumber,
                    postedCount = result.PostedCount
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new
                {
                    success = false,
                    message = ex.Message
                });
            }
        }
    }

    // =============================
    // REQUEST MODEL
    // =============================
    public class PayrollPostingRequest
    {
        public int PeriodID { get; set; }
        public int CompanyID { get; set; }
        public int UserID { get; set; }
        public List<int> EmployeeIDs { get; set; }
    }
}
