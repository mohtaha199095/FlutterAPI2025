using System;

namespace WebApplication2.cls
{
    /// <summary>
    /// Unified audit entry point for sessions and transaction/action logging.
    /// </summary>
    public static class clsAuditService
    {
        private static readonly clsAuditSession _sessions = new clsAuditSession();
        private static readonly clsAuditEvent _events = new clsAuditEvent();
        private static readonly clsAuditAdmin _admin = new clsAuditAdmin();

        public static AuditSessionResult StartSession(AuditContext ctx, int userId, string userName, int companyId, string authMethod = "Password")
        {
            var sessionGuid = Guid.NewGuid();
            int sessionId = _sessions.InsertSession(sessionGuid, userId, userName, ctx, companyId);

            string method = string.IsNullOrWhiteSpace(authMethod) ? "Password" : authMethod.Trim();
            LogEvent(new AuditEventRequest
            {
                Context = ctx,
                UserId = userId,
                CompanyId = companyId,
                SessionId = sessionId,
                ActionTypeCode = "Login",
                ModuleName = "Authentication",
                Description = "User logged in via " + method,
                RecordReference = method,
            });

            if (ctx != null) ctx.SessionGuid = sessionGuid;

            _admin.RefreshCompanySnapshot(companyId);

            return new AuditSessionResult { SessionGuid = sessionGuid, SessionId = sessionId };
        }

        public static void EndSession(Guid sessionGuid, int companyId, int userId, string reason, AuditContext ctx = null)
        {
            if (sessionGuid == Guid.Empty) return;

            var dt = _sessions.SelectSessionByGuid(sessionGuid, companyId);
            int sessionId = 0;
            if (dt != null && dt.Rows.Count > 0)
                sessionId = Simulate.Integer32(dt.Rows[0]["ID"]);

            _sessions.EndSession(sessionGuid, companyId, reason);

            LogEvent(new AuditEventRequest
            {
                Context = ctx,
                UserId = userId,
                CompanyId = companyId,
                SessionId = sessionId,
                ActionTypeCode = "Logout",
                ModuleName = "Authentication",
                Description = "User logged out: " + (reason ?? "Manual"),
            });

            _admin.RefreshCompanySnapshot(companyId);
        }

        public static void TouchSession(Guid sessionGuid, int companyId)
        {
            if (sessionGuid == Guid.Empty) return;
            _sessions.TouchSession(sessionGuid, companyId);
        }

        public static int LogEvent(AuditEventRequest req)
        {
            if (req == null || req.CompanyId <= 0) return 0;

            if (req.SessionId <= 0 && req.Context != null && req.Context.SessionGuid != Guid.Empty)
            {
                var dt = _sessions.SelectSessionByGuid(req.Context.SessionGuid, req.CompanyId);
                if (dt != null && dt.Rows.Count > 0)
                    req.SessionId = Simulate.Integer32(dt.Rows[0]["ID"]);
            }

            int eventId = _events.InsertEvent(req);

            if (req.Context != null && req.Context.SessionGuid != Guid.Empty)
                _sessions.TouchSession(req.Context.SessionGuid, req.CompanyId);

            _admin.RefreshCompanySnapshot(req.CompanyId);
            return eventId;
        }

        public static int LogLoginFailed(AuditContext ctx, int companyId, string userName)
        {
            return LogEvent(new AuditEventRequest
            {
                Context = ctx,
                UserId = 0,
                CompanyId = companyId,
                ActionTypeCode = "LoginFailed",
                ModuleName = "Authentication",
                RecordReference = userName ?? "",
                Description = "Failed login attempt for: " + (userName ?? ""),
            });
        }

        public static int LogInsert(int userId, int companyId, string moduleName, string entityTable,
            int recordId, string recordReference, string description = "")
        {
            return LogEvent(new AuditEventRequest
            {
                UserId = userId,
                CompanyId = companyId,
                ActionTypeCode = "Insert",
                ModuleName = moduleName,
                EntityTable = entityTable,
                RecordId = recordId,
                RecordReference = recordReference,
                Description = string.IsNullOrEmpty(description) ? "Inserted " + moduleName + " " + recordReference : description,
            });
        }

        public static int LogUpdate(int userId, int companyId, string moduleName, string entityTable,
            int recordId, string recordReference, string description = "")
        {
            return LogEvent(new AuditEventRequest
            {
                UserId = userId,
                CompanyId = companyId,
                ActionTypeCode = "Update",
                ModuleName = moduleName,
                EntityTable = entityTable,
                RecordId = recordId,
                RecordReference = recordReference,
                Description = string.IsNullOrEmpty(description) ? "Updated " + moduleName + " " + recordReference : description,
            });
        }

        public static int LogDelete(int userId, int companyId, string moduleName, string entityTable,
            int recordId, string recordReference, string description = "")
        {
            return LogEvent(new AuditEventRequest
            {
                UserId = userId,
                CompanyId = companyId,
                ActionTypeCode = "Delete",
                ModuleName = moduleName,
                EntityTable = entityTable,
                RecordId = recordId,
                RecordReference = recordReference,
                Description = string.IsNullOrEmpty(description) ? "Deleted " + moduleName + " " + recordReference : description,
            });
        }

        public static int LogPrint(AuditContext ctx, int userId, int companyId, string moduleName, string reference, int recordId = 0)
        {
            return LogEvent(new AuditEventRequest
            {
                Context = ctx,
                UserId = userId,
                CompanyId = companyId,
                ActionTypeCode = "Print",
                ModuleName = moduleName,
                RecordReference = reference,
                RecordId = recordId,
                Description = "Printed " + moduleName + " " + reference,
            });
        }

        public static int LogReport(AuditContext ctx, int userId, int companyId, string reportName, string description = "")
        {
            return LogEvent(new AuditEventRequest
            {
                Context = ctx,
                UserId = userId,
                CompanyId = companyId,
                ActionTypeCode = "Report",
                ModuleName = "Reports",
                RecordReference = reportName,
                Description = string.IsNullOrEmpty(description) ? "Ran report: " + reportName : description,
            });
        }

        public static int LogExport(AuditContext ctx, int userId, int companyId, string moduleName, string format)
        {
            return LogEvent(new AuditEventRequest
            {
                Context = ctx,
                UserId = userId,
                CompanyId = companyId,
                ActionTypeCode = "Export",
                ModuleName = moduleName,
                RecordReference = format,
                Description = "Exported " + moduleName + " as " + format,
            });
        }

        public static int LogView(AuditContext ctx, int userId, int companyId, string formName, int recordId = 0)
        {
            return LogEvent(new AuditEventRequest
            {
                Context = ctx,
                UserId = userId,
                CompanyId = companyId,
                ActionTypeCode = "View",
                ModuleName = formName,
                RecordId = recordId,
                Description = "Viewed " + formName,
            });
        }
    }
}
