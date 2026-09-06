using System;
using Microsoft.AspNetCore.Mvc;
using WebApplication2.cls;

namespace WebApplication2.Controllers
{
    [Route("api/ctlInventoryQa")]
    public class ctlInventoryQa : Controller
    {
        /// <summary>
        /// Full Warehouse / Inventory QA: validation fixtures, QTYFactor, schema, live integrity.
        /// Pass CompanyID to scan the tenant database.
        /// </summary>
        [HttpGet]
        [Route("RunInventoryQa")]
        public IActionResult RunInventoryQa(int CompanyID = 0, bool ScanDatabase = true)
        {
            try
            {
                var report = clsInventoryQaHarness.Run(CompanyID, ScanDatabase);
                return Ok(report);
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        /// <summary>Validation fixtures only (no DB) — fast smoke.</summary>
        [HttpGet]
        [Route("RunFixturesOnly")]
        public IActionResult RunFixturesOnly()
        {
            try
            {
                var report = clsInventoryQaHarness.Run(0, false);
                return Ok(new
                {
                    report.AllPassed,
                    report.TotalChecks,
                    report.PassedChecks,
                    report.FailedChecks,
                    report.Results
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }
    }
}
