using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System;
using System.Data;
using WebApplication2.cls;

namespace WebApplication2.Controllers
{
    [Route("api/ctlDesktopUpdate")]
    public class ctlDesktopUpdate : Controller
    {
        private IActionResult AdminDisabled() =>
            StatusCode(403, new { ok = false, message = "Admin tools are disabled." });

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

        private void LogAdminAction(string action, string adminUser, string details)
        {
            clsAdminAuditLog.Write(
                action,
                adminUser,
                details,
                clsAdminAuthHelper.ReadClientIp(Request),
                true);
        }

        /// <summary>Device poll — no admin gate. Returns download/apply/rollback command.</summary>
        [HttpPost]
        [Route("Heartbeat")]
        public IActionResult Heartbeat(
            string DeviceGuid,
            int CompanyID = 0,
            string AppVersion = "",
            int BuildNumber = 0,
            string MachineName = "",
            string DeviceLabel = "",
            string LocalStatus = "Idle")
        {
            if (!Guid.TryParse(DeviceGuid, out var guid) || guid == Guid.Empty)
                return BadRequest(new { ok = false, message = "DeviceGuid is required." });

            var cls = new clsDesktopUpdate();
            var dt = cls.Heartbeat(guid, CompanyID, AppVersion, BuildNumber, MachineName, DeviceLabel, LocalStatus);
            if (dt == null || dt.Rows.Count == 0)
                return Ok(new { ok = true, command = "None", pollSeconds = 300 });

            var row = dt.Rows[0];
            var status = Convert.ToString(row["UpdateStatus"]) ?? clsDesktopUpdate.StatusIdle;
            var response = new
            {
                ok = true,
                pollSeconds = 300,
                command = ResolveCommand(status, row),
                release = BuildReleasePayload(row, status),
                rollback = status == clsDesktopUpdate.StatusRollbackPending
                    ? new
                    {
                        appVersion = Convert.ToString(row["PreviousVersion"]),
                        buildNumber = row["PreviousBuild"] == DBNull.Value ? 0 : Convert.ToInt32(row["PreviousBuild"]),
                    }
                    : null,
            };
            return Ok(response);
        }

        [HttpPost]
        [Route("ReportError")]
        public IActionResult ReportError(string DeviceGuid, string Error = "")
        {
            if (!Guid.TryParse(DeviceGuid, out var guid) || guid == Guid.Empty)
                return BadRequest(new { ok = false, message = "DeviceGuid is required." });

            new clsDesktopUpdate().ReportDeviceError(guid, Error);
            return Ok(new { ok = true });
        }

        [HttpGet]
        [Route("SelectDevices")]
        public IActionResult SelectDevices(int CompanyID = 0, string Search = "", int TopN = 200, int Offset = 0, bool StaleOnly = false)
        {
            if (!TryAuthorizeAdmin(out IActionResult errorResult, out _)) return errorResult;

            var dt = new clsDesktopUpdate().SelectDevices(CompanyID, Search, TopN, Offset, StaleOnly);
            return Content(dt != null && dt.Rows.Count > 0 ? JsonConvert.SerializeObject(dt) : "[]", "application/json");
        }

        [HttpGet]
        [Route("SelectReleases")]
        public IActionResult SelectReleases(int TopN = 50)
        {
            if (!TryAuthorizeAdmin(out IActionResult errorResult, out _)) return errorResult;

            var dt = new clsDesktopUpdate().SelectReleases(TopN);
            return Content(dt != null && dt.Rows.Count > 0 ? JsonConvert.SerializeObject(dt) : "[]", "application/json");
        }

        [HttpPost]
        [Route("RegisterRelease")]
        public IActionResult RegisterRelease(
            string AppVersion,
            int BuildNumber,
            string FolderName = "",
            string ZipFileName = "",
            string DownloadUrl = "",
            string Sha256 = "",
            long FileSizeBytes = 0,
            string Notes = "")
        {
            if (!TryAuthorizeAdmin(out IActionResult errorResult, out string adminUser)) return errorResult;
            if (string.IsNullOrWhiteSpace(AppVersion) || BuildNumber <= 0 || string.IsNullOrWhiteSpace(Sha256))
                return BadRequest(new { ok = false, message = "AppVersion, BuildNumber, and Sha256 are required." });

            if (string.IsNullOrWhiteSpace(FolderName))
                FolderName = $"{AppVersion}_{BuildNumber}";

            var id = new clsDesktopUpdate().RegisterRelease(
                AppVersion, BuildNumber, FolderName, ZipFileName, DownloadUrl, Sha256, FileSizeBytes, Notes);
            if (id > 0)
            {
                LogAdminAction("DesktopRegisterRelease", adminUser, $"{AppVersion}+{BuildNumber} (ID {id})");
            }
            return Ok(new { ok = id > 0, releaseId = id });
        }

        [HttpPost]
        [Route("Deploy")]
        public IActionResult Deploy(int ReleaseID, string DeviceGuids = "", int CompanyID = 0)
        {
            if (!TryAuthorizeAdmin(out IActionResult errorResult, out string adminUser)) return errorResult;
            if (ReleaseID <= 0)
                return BadRequest(new { ok = false, message = "ReleaseID is required." });

            var cls = new clsDesktopUpdate();
            if (!string.IsNullOrWhiteSpace(DeviceGuids))
            {
                cls.DeployToDevices(ReleaseID, DeviceGuids);
                LogAdminAction("DesktopDeploy", adminUser, $"ReleaseID={ReleaseID}; Devices={DeviceGuids}");
                return Ok(new { ok = true, mode = "devices" });
            }
            if (CompanyID > 0)
            {
                var count = cls.DeployToCompany(ReleaseID, CompanyID);
                LogAdminAction("DesktopDeploy", adminUser, $"ReleaseID={ReleaseID}; CompanyID={CompanyID}; Count={count}");
                return Ok(new { ok = true, mode = "company", devicesUpdated = count });
            }
            return BadRequest(new { ok = false, message = "Provide DeviceGuids or CompanyID." });
        }

        [HttpPost]
        [Route("Cancel")]
        public IActionResult Cancel(string DeviceGuids = "")
        {
            if (!TryAuthorizeAdmin(out IActionResult errorResult, out string adminUser)) return errorResult;
            if (string.IsNullOrWhiteSpace(DeviceGuids))
                return BadRequest(new { ok = false, message = "DeviceGuids is required." });

            new clsDesktopUpdate().CancelDevices(DeviceGuids);
            LogAdminAction("DesktopCancel", adminUser, DeviceGuids);
            return Ok(new { ok = true });
        }

        [HttpPost]
        [Route("Rollback")]
        public IActionResult Rollback(string DeviceGuids = "")
        {
            if (!TryAuthorizeAdmin(out IActionResult errorResult, out string adminUser)) return errorResult;
            if (string.IsNullOrWhiteSpace(DeviceGuids))
                return BadRequest(new { ok = false, message = "DeviceGuids is required." });

            new clsDesktopUpdate().RollbackDevices(DeviceGuids);
            LogAdminAction("DesktopRollback", adminUser, DeviceGuids);
            return Ok(new { ok = true });
        }

        private static string ResolveCommand(string status, DataRow row)
        {
            switch (status)
            {
                case clsDesktopUpdate.StatusPendingDownload:
                case clsDesktopUpdate.StatusDownloading:
                    if (row["TargetReleaseID"] != DBNull.Value)
                        return "Download";
                    return "None";
                case clsDesktopUpdate.StatusReady:
                    return "ApplyOnRestart";
                case clsDesktopUpdate.StatusRollbackPending:
                    return "Rollback";
                default:
                    return "None";
            }
        }

        private static object? BuildReleasePayload(DataRow row, string status)
        {
            if (status != clsDesktopUpdate.StatusPendingDownload &&
                status != clsDesktopUpdate.StatusDownloading &&
                status != clsDesktopUpdate.StatusReady &&
                status != clsDesktopUpdate.StatusRollbackPending)
                return null;

            if (row["TargetReleaseID"] == DBNull.Value) return null;

            return new
            {
                appVersion = Convert.ToString(row["TargetAppVersion"]),
                buildNumber = row["TargetBuildNumber"] == DBNull.Value ? 0 : Convert.ToInt32(row["TargetBuildNumber"]),
                folderName = Convert.ToString(row["TargetFolderName"]),
                downloadUrl = Convert.ToString(row["TargetDownloadUrl"]),
                sha256 = Convert.ToString(row["TargetSha256"]),
            };
        }
    }
}
