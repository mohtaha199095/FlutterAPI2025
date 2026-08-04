using System;

namespace WebApplication2.cls
{
    public class AuditContext
    {
        public string IPAddress { get; set; } = "";
        public string DeviceInfo { get; set; } = "";
        public string AppVersion { get; set; } = "";
        public string Platform { get; set; } = "";
        public Guid SessionGuid { get; set; } = Guid.Empty;
    }

    public class AuditEventRequest
    {
        public AuditContext Context { get; set; }
        public int UserId { get; set; }
        public int CompanyId { get; set; }
        public int SessionId { get; set; }
        public string ActionTypeCode { get; set; } = "";
        public string ModuleName { get; set; } = "";
        public string EntityTable { get; set; } = "";
        public int RecordId { get; set; }
        public string RecordReference { get; set; } = "";
        public string Description { get; set; } = "";
        public int FormId { get; set; }
        public string OldValuesJson { get; set; } = "";
        public string NewValuesJson { get; set; } = "";
    }

    public class AuditSessionResult
    {
        public Guid SessionGuid { get; set; }
        public int SessionId { get; set; }
    }
}
