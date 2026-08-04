using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System;
using System.Data;
using WebApplication2.cls;

namespace WebApplication2.Controllers
{
    [Route("api/ctlApproval")]
    public class ctlApproval : Controller
    {
        [HttpGet]
        [Route("SelectPending")]
        public string SelectPending(int CompanyID, int UserID)
        {
            clsApprovalRequest cls = new clsApprovalRequest();
            DataTable dt = cls.SelectPendingForUser(UserID, CompanyID);
            return dt != null && dt.Rows.Count > 0 ? JsonConvert.SerializeObject(dt) : "[]";
        }

        [HttpGet]
        [Route("CountPending")]
        public int CountPending(int CompanyID, int UserID)
        {
            clsApprovalRequest cls = new clsApprovalRequest();
            return cls.CountPendingForUser(UserID, CompanyID);
        }

        [HttpGet]
        [Route("SelectMySubmissions")]
        public string SelectMySubmissions(int CompanyID, int UserID)
        {
            clsApprovalRequest cls = new clsApprovalRequest();
            DataTable dt = cls.SelectMySubmissions(UserID, CompanyID);
            return dt != null && dt.Rows.Count > 0 ? JsonConvert.SerializeObject(dt) : "[]";
        }

        [HttpGet]
        [Route("SelectActionHistoryByUser")]
        public string SelectActionHistoryByUser(int CompanyID, int UserID, int TopN = 200)
        {
            clsApprovalRequest cls = new clsApprovalRequest();
            DataTable dt = cls.SelectActionHistoryByUser(UserID, CompanyID, TopN);
            return dt != null && dt.Rows.Count > 0 ? JsonConvert.SerializeObject(dt) : "[]";
        }

        [HttpGet]
        [Route("SelectAssignmentHistoryForUser")]
        public string SelectAssignmentHistoryForUser(int CompanyID, int UserID, int TopN = 200)
        {
            clsApprovalRequest cls = new clsApprovalRequest();
            DataTable dt = cls.SelectAssignmentHistoryForUser(UserID, CompanyID, TopN);
            return dt != null && dt.Rows.Count > 0 ? JsonConvert.SerializeObject(dt) : "[]";
        }

        [HttpGet]
        [Route("SelectHistory")]
        public string SelectHistory(int CompanyID, string DocumentGuid)
        {
            clsApprovalRequest cls = new clsApprovalRequest();
            DataTable dt = cls.SelectActions(DocumentGuid, CompanyID);
            return dt != null && dt.Rows.Count > 0 ? JsonConvert.SerializeObject(dt) : "[]";
        }

        [HttpGet]
        [Route("SelectDocumentProgress")]
        public string SelectDocumentProgress(int CompanyID, string DocumentGuid, int DocumentTypeID = 0)
        {
            try
            {
                clsApprovalEngine engine = new clsApprovalEngine();
                ApprovalDocumentProgress progress = engine.GetDocumentProgress(DocumentGuid, CompanyID, DocumentTypeID);
                return JsonConvert.SerializeObject(progress);
            }
            catch (Exception ex)
            {
                return JsonConvert.SerializeObject(new ApprovalDocumentProgress
                {
                    DocumentGuid = DocumentGuid ?? "",
                    InfoMessage = ex.Message,
                });
            }
        }

        [HttpGet]
        [Route("CountUnpostedDocuments")]
        public int CountUnpostedDocuments(int CompanyID, int DocumentTypeID)
        {
            clsApprovalPolicy cls = new clsApprovalPolicy();
            return cls.CountUnpostedDocuments(CompanyID, DocumentTypeID);
        }

        [HttpGet]
        [Route("SelectPolicies")]
        public string SelectPolicies(int CompanyID, int DocumentTypeID = 0)
        {
            new clsDataBaseVersion().EnsureApprovalWorkflowSchema(CompanyID);
            clsApprovalPolicy cls = new clsApprovalPolicy();
            DataTable dt = cls.SelectPolicies(CompanyID, DocumentTypeID);
            return dt != null && dt.Rows.Count > 0 ? JsonConvert.SerializeObject(dt) : "[]";
        }

        [HttpGet]
        [Route("SelectPolicyLevels")]
        public string SelectPolicyLevels(int CompanyID, int PolicyID)
        {
            clsApprovalPolicy cls = new clsApprovalPolicy();
            var levels = cls.GetLevels(PolicyID, CompanyID);
            return levels != null && levels.Count > 0 ? JsonConvert.SerializeObject(levels) : "[]";
        }

        [HttpGet]
        [Route("SelectNotifications")]
        public string SelectNotifications(int CompanyID, int UserID, bool UnreadOnly = false, int TopN = 100)
        {
            clsApprovalNotification cls = new clsApprovalNotification();
            DataTable dt = cls.SelectForUser(UserID, CompanyID, UnreadOnly, TopN);
            return dt != null && dt.Rows.Count > 0 ? JsonConvert.SerializeObject(dt) : "[]";
        }

        [HttpGet]
        [Route("CountUnreadNotifications")]
        public int CountUnreadNotifications(int CompanyID, int UserID)
        {
            clsApprovalNotification cls = new clsApprovalNotification();
            return cls.CountUnread(UserID, CompanyID);
        }

        [HttpPost]
        [Route("MarkNotificationRead")]
        public bool MarkNotificationRead(int CompanyID, int UserID, int NotificationID)
        {
            clsApprovalNotification cls = new clsApprovalNotification();
            cls.MarkRead(NotificationID, UserID, CompanyID);
            return true;
        }

        [HttpPost]
        [Route("Submit")]
        public string Submit(int CompanyID, int UserID, int DocumentTypeID, string DocumentGuid, string Comments = "")
        {
            clsApprovalEngine engine = new clsApprovalEngine();
            ApprovalResult result = engine.Submit(new ApprovalSubmitRequest
            {
                CompanyID = CompanyID,
                UserID = UserID,
                DocumentTypeID = DocumentTypeID,
                DocumentGuid = DocumentGuid,
                Comments = Comments ?? "",
            });
            return JsonConvert.SerializeObject(result);
        }

        [HttpPost]
        [Route("Approve")]
        public string Approve(int CompanyID, int UserID, string RequestGuid, string Comments = "")
        {
            clsApprovalEngine engine = new clsApprovalEngine();
            ApprovalResult result = engine.Approve(new ApprovalDecisionRequest
            {
                CompanyID = CompanyID,
                UserID = UserID,
                RequestGuid = RequestGuid,
                Comments = Comments ?? "",
            });
            return JsonConvert.SerializeObject(result);
        }

        [HttpPost]
        [Route("Reject")]
        public string Reject(int CompanyID, int UserID, string RequestGuid, string Comments = "")
        {
            clsApprovalEngine engine = new clsApprovalEngine();
            ApprovalResult result = engine.Reject(new ApprovalDecisionRequest
            {
                CompanyID = CompanyID,
                UserID = UserID,
                RequestGuid = RequestGuid,
                Comments = Comments ?? "",
            });
            return JsonConvert.SerializeObject(result);
        }

        [HttpPost]
        [Route("SavePolicy")]
        public string SavePolicy(int CompanyID, int UserID, [FromBody] ApprovalPolicySaveRequest req)
        {
            try
            {
                if (req == null)
                {
                    return JsonConvert.SerializeObject(new
                    {
                        Success = false,
                        Message = "Invalid request body.",
                    });
                }

                req.CompanyID = CompanyID;
                req.UserID = UserID;
                clsApprovalPolicy cls = new clsApprovalPolicy();
                int policyId = cls.SavePolicy(req);
                return JsonConvert.SerializeObject(new
                {
                    Success = policyId > 0,
                    PolicyID = policyId,
                    Message = policyId > 0 ? "" : "Policy was not saved.",
                });
            }
            catch (Exception ex)
            {
                return JsonConvert.SerializeObject(new
                {
                    Success = false,
                    Message = ex.Message,
                });
            }
        }

        [HttpGet]
        [Route("ResolveInitialStatus")]
        public int ResolveInitialStatus(int CompanyID, int DocumentTypeID, int BranchID, decimal Amount)
        {
            clsApprovalEngine engine = new clsApprovalEngine();
            return engine.ResolveInitialDocumentStatus(CompanyID, DocumentTypeID, BranchID, Amount);
        }
    }
}
