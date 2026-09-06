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

        private bool TryAuthorizeAdmin(out IActionResult errorResult, out string adminUser)
        {
            adminUser = "";
            var configuration = HttpContext.RequestServices.GetService(typeof(IConfiguration)) as IConfiguration;
            if (!clsAdminAuthHelper.TryAuthorizeAdmin(
                    configuration,
                    Request,
                    out errorResult,
                    out adminUser,
                    out _))
            {
                return false;
            }

            return true;
        }

        private void LogAdminAction(string action, string adminUser, string details, bool success = true)
        {
            clsAdminAuditLog.Write(
                action,
                adminUser,
                details,
                clsAdminAuthHelper.ReadClientIp(Request),
                success);
        }

        [HttpGet]
        [Route("SelectAllCompaniesOverview")]
        public IActionResult SelectAllCompaniesOverview(
            string Search = "",
            bool? ActiveOnly = null,
            bool? SuspendedOnly = null,
            bool? ExpiringOnly = null,
            int TopN = 100,
            int Offset = 0)
        {
            if (!TryAuthorizeAdmin(out IActionResult errorResult, out _)) return errorResult;

            clsAuditAdmin cls = new clsAuditAdmin();
            DataTable dt = cls.SelectAllCompaniesOverview(Search, ActiveOnly, SuspendedOnly, ExpiringOnly, TopN, Offset);
            return Content(dt != null && dt.Rows.Count > 0 ? JsonConvert.SerializeObject(dt) : "[]", "application/json");
        }

        [HttpGet]
        [Route("SelectCompanyAuditSummary")]
        public IActionResult SelectCompanyAuditSummary(int CompanyID, int TopN = 100)
        {
            if (!TryAuthorizeAdmin(out IActionResult errorResult, out _)) return errorResult;
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
            if (!TryAuthorizeAdmin(out IActionResult errorResult, out string adminUser)) return errorResult;
            if (CompanyID <= 0) return BadRequest(new { ok = false, message = "CompanyID is required." });

            clsAuditAdmin cls = new clsAuditAdmin();
            bool ok = cls.UpdateCompanyStatus(CompanyID, IsActive, IsSuspended, AdminNotes, SubscriptionExpiry);
            cls.RefreshCompanySnapshot(CompanyID, force: true);
            LogAdminAction(
                "CompanyStatusUpdate",
                adminUser,
                $"CompanyID={CompanyID}; IsActive={IsActive}; IsSuspended={IsSuspended}",
                ok);
            return Ok(new { ok });
        }

        [HttpPost]
        [Route("RefreshCompanySnapshot")]
        public IActionResult RefreshCompanySnapshot(int CompanyID)
        {
            if (!TryAuthorizeAdmin(out IActionResult errorResult, out string adminUser)) return errorResult;
            if (CompanyID <= 0) return BadRequest(new { ok = false, message = "CompanyID is required." });

            clsAuditAdmin cls = new clsAuditAdmin();
            cls.RefreshCompanySnapshot(CompanyID, force: true);
            LogAdminAction("RefreshCompanySnapshot", adminUser, $"CompanyID={CompanyID}");
            return Ok(new { ok = true });
        }

        [HttpGet]
        [Route("SelectDashboardSummary")]
        public IActionResult SelectDashboardSummary()
        {
            if (!TryAuthorizeAdmin(out IActionResult errorResult, out _)) return errorResult;

            try
            {
                var dashboard = new clsAdminDashboard();
                return Ok(dashboard.SelectSummary());
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { ok = false, message = "Dashboard failed to load: " + ex.Message });
            }
        }

        [HttpGet]
        [Route("SelectAdminAuditLog")]
        public IActionResult SelectAdminAuditLog(int TopN = 100)
        {
            if (!TryAuthorizeAdmin(out IActionResult errorResult, out _)) return errorResult;

            DataTable dt = clsAdminAuditLog.SelectRecent(TopN);
            return Content(dt != null && dt.Rows.Count > 0 ? JsonConvert.SerializeObject(dt) : "[]", "application/json");
        }

        [HttpPost]
        [Route("RunDatabaseMigration")]
        public IActionResult RunDatabaseMigration(int CompanyID)
        {
            if (!TryAuthorizeAdmin(out IActionResult errorResult, out string adminUser)) return errorResult;
            var result = new clsAdminOps().RunDatabaseMigration(CompanyID);
            LogAdminAction("RunDatabaseMigration", adminUser, $"CompanyID={CompanyID}");
            return Ok(result);
        }

        [HttpPost]
        [Route("RunDatabaseMigrationAll")]
        public IActionResult RunDatabaseMigrationAll(int MaxCompanies = 100)
        {
            if (!TryAuthorizeAdmin(out IActionResult errorResult, out string adminUser)) return errorResult;
            var result = new clsAdminOps().RunDatabaseMigrationAll(MaxCompanies);
            LogAdminAction("RunDatabaseMigrationAll", adminUser, $"Max={MaxCompanies}");
            return Ok(result);
        }

        [HttpPost]
        [Route("RefreshAllCompanySnapshots")]
        public IActionResult RefreshAllCompanySnapshots(int MaxCompanies = 200)
        {
            if (!TryAuthorizeAdmin(out IActionResult errorResult, out string adminUser)) return errorResult;
            var result = new clsAdminOps().RefreshAllCompanySnapshots(MaxCompanies);
            LogAdminAction("RefreshAllSnapshots", adminUser, $"Max={MaxCompanies}");
            return Ok(result);
        }

        [HttpGet]
        [Route("SelectCrossTenantActivity")]
        public IActionResult SelectCrossTenantActivity(int TopN = 100)
        {
            if (!TryAuthorizeAdmin(out IActionResult errorResult, out _)) return errorResult;
            DataTable dt = new clsAdminOps().SelectCrossTenantActivity(TopN);
            return Content(dt != null && dt.Rows.Count > 0 ? JsonConvert.SerializeObject(dt) : "[]", "application/json");
        }

        [HttpGet]
        [Route("AdminImpersonateCompany")]
        public IActionResult AdminImpersonateCompany(int CompanyID)
        {
            if (!TryAuthorizeAdmin(out IActionResult errorResult, out string adminUser)) return errorResult;
            var configuration = HttpContext.RequestServices.GetService(typeof(IConfiguration)) as IConfiguration;
            string json = new clsAdminOps().BuildImpersonationLoginJson(
                CompanyID,
                configuration,
                clsAdminAuthHelper.ReadClientIp(Request));
            LogAdminAction("ImpersonateCompany", adminUser, $"CompanyID={CompanyID}");
            return Content(json, "application/json");
        }

        [HttpPost]
        [Route("SendSubscriptionExpiryAlerts")]
        public IActionResult SendSubscriptionExpiryAlerts(int DaysAhead = 7)
        {
            if (!TryAuthorizeAdmin(out IActionResult errorResult, out string adminUser)) return errorResult;
            var configuration = HttpContext.RequestServices.GetService(typeof(IConfiguration)) as IConfiguration;
            var result = new clsAdminOps().SendSubscriptionExpiryAlerts(configuration, DaysAhead);
            LogAdminAction("SubscriptionExpiryAlert", adminUser, $"DaysAhead={DaysAhead}");
            return Ok(result);
        }

        [HttpGet]
        [Route("SelectDatabaseMigrationDrift")]
        public IActionResult SelectDatabaseMigrationDrift(bool BehindOnly = true, int TopN = 200)
        {
            if (!TryAuthorizeAdmin(out IActionResult errorResult, out _)) return errorResult;

            var ops = new clsAdminOps();
            DataTable dt = ops.SelectDatabaseMigrationDrift(BehindOnly, TopN);
            decimal targetVersion = ops.ResolveTargetSchemaVersion();

            return Ok(new
            {
                targetVersion,
                behindCount = dt?.Rows.Count ?? 0,
                rows = dt != null && dt.Rows.Count > 0
                    ? JsonConvert.DeserializeObject(JsonConvert.SerializeObject(dt))
                    : new object[0],
            });
        }
    }
}
