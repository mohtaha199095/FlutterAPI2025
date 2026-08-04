using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System;
using System.Data;
using WebApplication2.cls;

namespace WebApplication2.Controllers
{
    [Route("api/ctlAuditLog")]
    public class ctlAuditLog : Controller
    {
        [HttpGet]
        [Route("SelectSessions")]
        public string SelectSessions(
            int CompanyID,
            int UserID = 0,
            DateTime DateFrom = default,
            DateTime DateTo = default,
            bool ActiveOnly = false,
            int TopN = 500)
        {
            clsAuditSession cls = new clsAuditSession();
            DataTable dt = cls.SelectSessions(CompanyID, UserID, DateFrom, DateTo, ActiveOnly, TopN);
            return dt != null && dt.Rows.Count > 0 ? JsonConvert.SerializeObject(dt) : "[]";
        }

        [HttpGet]
        [Route("SelectEvents")]
        public string SelectEvents(
            int CompanyID,
            int UserID = 0,
            string ActionTypeCode = "",
            string ModuleName = "",
            DateTime DateFrom = default,
            DateTime DateTo = default,
            int TopN = 500)
        {
            clsAuditEvent cls = new clsAuditEvent();
            DataTable dt = cls.SelectEvents(CompanyID, UserID, ActionTypeCode, ModuleName, DateFrom, DateTo, TopN);
            return dt != null && dt.Rows.Count > 0 ? JsonConvert.SerializeObject(dt) : "[]";
        }

        [HttpGet]
        [Route("SelectActionTypes")]
        public string SelectActionTypes(int CompanyID)
        {
            clsAuditEvent cls = new clsAuditEvent();
            DataTable dt = cls.SelectActionTypes(CompanyID);
            return dt != null && dt.Rows.Count > 0 ? JsonConvert.SerializeObject(dt) : "[]";
        }

        [HttpGet]
        [Route("SelectLoginSummaryReport")]
        public string SelectLoginSummaryReport(int CompanyID, DateTime DateFrom, DateTime DateTo)
        {
            clsAuditReport cls = new clsAuditReport();
            DataTable dt = cls.SelectLoginSummaryReport(CompanyID, DateFrom, DateTo);
            return dt != null && dt.Rows.Count > 0 ? JsonConvert.SerializeObject(dt) : "[]";
        }

        [HttpGet]
        [Route("SelectRecordMetadata")]
        public string SelectRecordMetadata(int CompanyID, string TableName, string RecordKey)
        {
            clsRecordAudit cls = new clsRecordAudit();
            return cls.SelectRecordMetadata(TableName, RecordKey, CompanyID);
        }

        [HttpPost]
        [Route("LogEvent")]
        public int LogEvent(
            int CompanyID,
            int UserID,
            string SessionGuid,
            string ActionTypeCode,
            string ModuleName = "",
            string EntityTable = "",
            int RecordID = 0,
            string RecordReference = "",
            string Description = "",
            int FormID = 0,
            string IPAddress = "",
            string DeviceInfo = "",
            string AppVersion = "",
            string Platform = "")
        {
            var ctx = BuildContext(SessionGuid, IPAddress, DeviceInfo, AppVersion, Platform);
            return clsAuditService.LogEvent(new AuditEventRequest
            {
                Context = ctx,
                UserId = UserID,
                CompanyId = CompanyID,
                ActionTypeCode = ActionTypeCode,
                ModuleName = ModuleName,
                EntityTable = EntityTable,
                RecordId = RecordID,
                RecordReference = RecordReference,
                Description = Description,
                FormId = FormID,
            });
        }

        [HttpPost]
        [Route("EndSession")]
        public IActionResult EndSession(
            int CompanyID,
            int UserID,
            string SessionGuid,
            string LogoutReason = "Manual",
            string IPAddress = "",
            string DeviceInfo = "",
            string AppVersion = "",
            string Platform = "")
        {
            var ctx = BuildContext(SessionGuid, IPAddress, DeviceInfo, AppVersion, Platform);
            Guid guid = ParseGuid(SessionGuid);
            clsAuditService.EndSession(guid, CompanyID, UserID, LogoutReason, ctx);
            return Ok(new { ok = true });
        }

        [HttpPost]
        [Route("TouchSession")]
        public IActionResult TouchSession(int CompanyID, string SessionGuid)
        {
            clsAuditService.TouchSession(ParseGuid(SessionGuid), CompanyID);
            return Ok(new { ok = true });
        }

        private static AuditContext BuildContext(string sessionGuid, string ip, string device, string appVersion, string platform)
        {
            return new AuditContext
            {
                SessionGuid = ParseGuid(sessionGuid),
                IPAddress = ip ?? "",
                DeviceInfo = device ?? "",
                AppVersion = appVersion ?? "",
                Platform = platform ?? "",
            };
        }

        private static Guid ParseGuid(string value)
        {
            return Guid.TryParse(value, out var g) ? g : Guid.Empty;
        }
    }
}
