using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System;
using System.Data;
using WebApplication2.cls;

namespace WebApplication2.Controllers
{
    [Route("api/ctlAuditAdmin")]
    public class ctlAuditAdmin : Controller
    {
        private IActionResult AdminDisabled()
        {
            return StatusCode(403, new { ok = false, message = "Admin tools are disabled." });
        }

        private bool IsAdminAvailable()
        {
            var configuration = HttpContext.RequestServices.GetService(typeof(IConfiguration)) as IConfiguration;
            return clsAdminLogin.IsEnabled(configuration);
        }

        [HttpGet]
        [Route("SelectAllCompaniesOverview")]
        public IActionResult SelectAllCompaniesOverview(
            string Search = "",
            bool? ActiveOnly = null,
            bool? SuspendedOnly = null,
            int TopN = 100,
            int Offset = 0)
        {
            if (!IsAdminAvailable()) return AdminDisabled();

            clsAuditAdmin cls = new clsAuditAdmin();
            DataTable dt = cls.SelectAllCompaniesOverview(Search, ActiveOnly, SuspendedOnly, TopN, Offset);
            return Content(dt != null && dt.Rows.Count > 0 ? JsonConvert.SerializeObject(dt) : "[]", "application/json");
        }

        [HttpGet]
        [Route("SelectCompanyAuditSummary")]
        public IActionResult SelectCompanyAuditSummary(int CompanyID, int TopN = 100)
        {
            if (!IsAdminAvailable()) return AdminDisabled();
            if (CompanyID <= 0) return BadRequest(new { ok = false, message = "CompanyID is required." });

            clsAuditAdmin cls = new clsAuditAdmin();
            cls.RefreshCompanySnapshot(CompanyID, force: true);
            DataTable dt = cls.SelectCompanyAuditSummary(CompanyID, TopN);
            return Content(dt != null ? JsonConvert.SerializeObject(dt) : "[]", "application/json");
        }

        [HttpPost]
        [Route("UpdateCompanyStatus")]
        public IActionResult UpdateCompanyStatus(
            int CompanyID,
            bool? IsActive = null,
            bool? IsSuspended = null,
            string AdminNotes = "",
            DateTime SubscriptionExpiry = default)
        {
            if (!IsAdminAvailable()) return AdminDisabled();
            if (CompanyID <= 0) return BadRequest(new { ok = false, message = "CompanyID is required." });

            clsAuditAdmin cls = new clsAuditAdmin();
            bool ok = cls.UpdateCompanyStatus(CompanyID, IsActive, IsSuspended, AdminNotes, SubscriptionExpiry);
            cls.RefreshCompanySnapshot(CompanyID, force: true);
            return Ok(new { ok });
        }

        [HttpPost]
        [Route("RefreshCompanySnapshot")]
        public IActionResult RefreshCompanySnapshot(int CompanyID)
        {
            if (!IsAdminAvailable()) return AdminDisabled();
            if (CompanyID <= 0) return BadRequest(new { ok = false, message = "CompanyID is required." });

            clsAuditAdmin cls = new clsAuditAdmin();
            cls.RefreshCompanySnapshot(CompanyID, force: true);
            return Ok(new { ok = true });
        }
    }
}
