using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using WebApplication2.MainClasses;
using static WebApplication2.MainClasses.clsEnum;

namespace WebApplication2.cls
{
    public class clsApprovalEngine
    {
        private readonly clsApprovalPolicy _policy = new clsApprovalPolicy();
        private readonly clsApprovalRequest _requests = new clsApprovalRequest();
        private readonly clsApprovalNotification _notifications = new clsApprovalNotification();
        private readonly clsDocumentPostingService _posting = new clsDocumentPostingService();
        private readonly clsJournalVoucherHeader _jvHeader = new clsJournalVoucherHeader();
        private readonly clsCashVoucherHeader _cashHeader = new clsCashVoucherHeader();

        public int ResolveInitialDocumentStatus(int companyId, int documentTypeId, int branchId, decimal amount)
        {
            if (!clsDocumentPostingService.IsMvpApprovalType(documentTypeId))
                return (int)DocumentStatus.Posted;

            if (!IsApprovalRequired(companyId, documentTypeId, branchId, amount))
                return (int)DocumentStatus.Posted;

            return (int)DocumentStatus.Draft;
        }

        public bool IsApprovalRequired(int companyId, int documentTypeId, int branchId, decimal amount)
        {
            if (!clsDocumentPostingService.IsMvpApprovalType(documentTypeId))
                return false;

            try
            {
                // Do NOT run EnsureApprovalWorkflowSchema on every document save.
                // Schema migration (and full-table UPDATEs) caused POS invoice timeouts.
                // Admin/submit approval endpoints call Ensure explicitly when needed.
                var policy = _policy.ResolvePolicy(companyId, documentTypeId, branchId, amount);
                if (policy == null || !policy.IsEnabled)
                    return false;

                var levels = _policy.GetApplicableLevels(policy.ID, companyId, amount);
                return levels != null && levels.Count > 0;
            }
            catch
            {
                // Missing approval tables / schema → treat as no approval required.
                return false;
            }
        }

        /// <summary>
        /// Posted documents are read-only only when approval workflow is active.
        /// Pending approval is always blocked.
        /// </summary>
        public bool DocumentStatusBlocksEdit(int companyId, int documentTypeId, int branchId, decimal amount, int documentStatus)
        {
            if (documentStatus == (int)DocumentStatus.PendingApproval)
                return true;

            if (documentStatus == (int)DocumentStatus.Posted)
                return IsApprovalRequired(companyId, documentTypeId, branchId, amount);

            return false;
        }

        public ApprovalResult Submit(ApprovalSubmitRequest req)
        {
            var result = new ApprovalResult();
            clsSQL sql = new clsSQL();
            SqlConnection con = new SqlConnection(sql.CreateDataBaseConnectionString(req.CompanyID));
            con.Open();
            SqlTransaction trn = con.BeginTransaction();

            try
            {
                if (!TryGetDocumentMeta(req.DocumentTypeID, req.DocumentGuid, req.CompanyID, trn,
                        out int branchId, out decimal amount, out string documentNumber, out int currentStatus, out int submittedBy))
                {
                    result.Message = "Document not found.";
                    trn.Rollback();
                    return result;
                }

                if (currentStatus != (int)DocumentStatus.Draft && currentStatus != (int)DocumentStatus.Rejected)
                {
                    result.Message = "Only draft or rejected documents can be submitted.";
                    trn.Rollback();
                    return result;
                }

                var policy = _policy.ResolvePolicy(req.CompanyID, req.DocumentTypeID, branchId, amount);
                if (policy == null)
                {
                    result.Message = "No approval policy configured for this document type.";
                    trn.Rollback();
                    return result;
                }

                var levels = _policy.GetApplicableLevels(policy.ID, req.CompanyID, amount);
                if (levels.Count == 0)
                {
                    result.Message = "No approval levels apply for this document amount.";
                    trn.Rollback();
                    return result;
                }

                if (!policy.AllowSelfApproval && submittedBy == req.UserID &&
                    levels.Exists(l => l.MemberUserIds != null && l.MemberUserIds.Contains(req.UserID)))
                {
                    // submitter is also approver - still allowed at submit; blocked at approve step
                }

                var firstLevel = levels[0];
                string requestGuid = _requests.InsertRequest(
                    policy.ID, req.DocumentTypeID, req.DocumentGuid, documentNumber, amount, branchId,
                    req.UserID, req.Comments, req.CompanyID, firstLevel.LevelNo, trn);

                _requests.InsertAction(requestGuid, req.DocumentGuid, 0, "Submit", (int)ApprovalActionType.Submit,
                    req.UserID, req.Comments, req.CompanyID, trn);

                SetDocumentStatus(req.DocumentTypeID, req.DocumentGuid, (int)DocumentStatus.PendingApproval,
                    req.UserID, req.CompanyID, trn);

                NotifyLevelGroup(firstLevel, req.CompanyID, req.DocumentTypeID, req.DocumentGuid,
                    documentNumber, trn);

                clsAuditService.LogUpdate(req.UserID, req.CompanyID, "Approval", "tbl_ApprovalRequest", 0,
                    documentNumber, "Submitted for approval");

                trn.Commit();
                result.Success = true;
                result.Message = "Submitted for approval.";
                result.DocumentStatus = (int)DocumentStatus.PendingApproval;
                result.RequestGuid = requestGuid;
                return result;
            }
            catch (Exception ex)
            {
                trn.Rollback();
                result.Message = ex.Message;
                return result;
            }
            finally
            {
                con.Close();
            }
        }

        public ApprovalResult Approve(ApprovalDecisionRequest req)
        {
            return ProcessDecision(req, approve: true);
        }

        public ApprovalResult Reject(ApprovalDecisionRequest req)
        {
            return ProcessDecision(req, approve: false);
        }

        private ApprovalResult ProcessDecision(ApprovalDecisionRequest req, bool approve)
        {
            var result = new ApprovalResult();
            clsSQL sql = new clsSQL();
            SqlConnection con = new SqlConnection(sql.CreateDataBaseConnectionString(req.CompanyID));
            con.Open();
            SqlTransaction trn = con.BeginTransaction();

            try
            {
                DataTable dtReq = _requests.SelectByGuid(req.RequestGuid, req.CompanyID);
                if (dtReq == null || dtReq.Rows.Count == 0)
                {
                    result.Message = "Approval request not found.";
                    trn.Rollback();
                    return result;
                }

                DataRow row = dtReq.Rows[0];
                if (Simulate.Integer32(row["Status"]) != (int)ApprovalRequestStatus.Pending)
                {
                    result.Message = "Approval request is not pending.";
                    trn.Rollback();
                    return result;
                }

                int policyId = Simulate.Integer32(row["PolicyID"]);
                int currentLevel = Simulate.Integer32(row["CurrentLevel"]);
                int documentTypeId = Simulate.Integer32(row["DocumentTypeID"]);
                string documentGuid = Simulate.String(row["DocumentGuid"]);
                string documentNumber = Simulate.String(row["DocumentNumber"]);
                int submittedBy = Simulate.Integer32(row["SubmittedByUserId"]);

                decimal totalAmount = Simulate.Decimal(row["TotalAmount"]);
                var levels = _policy.GetApplicableLevels(policyId, req.CompanyID, totalAmount);
                var level = levels.Find(l => l.LevelNo == currentLevel);
                if (level == null)
                {
                    result.Message = "Approval level not found for this document amount.";
                    trn.Rollback();
                    return result;
                }

                if (!IsUserInLevelGroup(level, req.UserID))
                {
                    result.Message = "You are not authorized to act on this approval level.";
                    trn.Rollback();
                    return result;
                }

                if (_requests.UserAlreadyApprovedAtLevel(req.RequestGuid, currentLevel, req.UserID, req.CompanyID, trn))
                {
                    result.Message = "You have already approved at this level.";
                    trn.Rollback();
                    return result;
                }

                var policy = _policy.ResolvePolicy(req.CompanyID, documentTypeId, Simulate.Integer32(row["BranchID"]),
                    Simulate.Decimal(row["TotalAmount"]));
                if (policy != null && !policy.AllowSelfApproval && submittedBy == req.UserID)
                {
                    result.Message = "Submitter cannot approve their own document.";
                    trn.Rollback();
                    return result;
                }

                if (!approve)
                {
                    _requests.InsertAction(req.RequestGuid, documentGuid, currentLevel, level.LevelName,
                        (int)ApprovalActionType.Reject, req.UserID, req.Comments, req.CompanyID, trn);
                    _requests.UpdateRequestProgress(req.RequestGuid, currentLevel, (int)ApprovalRequestStatus.Rejected,
                        0, req.CompanyID, trn);
                    SetDocumentStatus(documentTypeId, documentGuid, (int)DocumentStatus.Rejected, req.UserID,
                        req.CompanyID, trn);
                    _notifications.InsertNotification(submittedBy, req.CompanyID,
                        "Document rejected", "Document " + documentNumber + " was rejected.", "Approval",
                        documentGuid, trn);

                    trn.Commit();
                    result.Success = true;
                    result.Message = "Document rejected.";
                    result.DocumentStatus = (int)DocumentStatus.Rejected;
                    return result;
                }

                _requests.InsertAction(req.RequestGuid, documentGuid, currentLevel, level.LevelName,
                    (int)ApprovalActionType.Approve, req.UserID, req.Comments, req.CompanyID, trn);

                if (level.RequireAllApprovers)
                {
                    int approvedCount = _requests.CountDistinctApprovalsAtLevel(
                        req.RequestGuid, currentLevel, req.CompanyID, trn);
                    int requiredCount = level.MemberUserIds != null ? level.MemberUserIds.Count : 1;
                    if (approvedCount < requiredCount)
                    {
                        trn.Commit();
                        result.Success = true;
                        result.Message = "Approval recorded. Waiting for other approvers (" +
                                         approvedCount + "/" + requiredCount + ").";
                        result.DocumentStatus = (int)DocumentStatus.PendingApproval;
                        return result;
                    }
                }

                int idx = levels.FindIndex(l => l.LevelNo == currentLevel);
                if (idx >= 0 && idx < levels.Count - 1)
                {
                    var nextLevel = levels[idx + 1];
                    _requests.UpdateRequestProgress(req.RequestGuid, nextLevel.LevelNo, (int)ApprovalRequestStatus.Pending,
                        0, req.CompanyID, trn);
                    NotifyLevelGroup(nextLevel, req.CompanyID, documentTypeId, documentGuid,
                        documentNumber, trn);
                    _notifications.InsertNotification(submittedBy, req.CompanyID,
                        "Approval in progress",
                        "Document " + documentNumber + " advanced to " + nextLevel.LevelName + ".",
                        "Approval", documentGuid, trn);

                    trn.Commit();
                    result.Success = true;
                    result.Message = "Approved and forwarded to next level.";
                    result.DocumentStatus = (int)DocumentStatus.PendingApproval;
                    return result;
                }

                if (!_posting.PostDocument(documentTypeId, documentGuid, req.UserID, req.CompanyID, trn))
                {
                    result.Message = "Final approval succeeded but posting failed.";
                    trn.Rollback();
                    return result;
                }

                _requests.UpdateRequestProgress(req.RequestGuid, currentLevel, (int)ApprovalRequestStatus.Approved,
                    req.UserID, req.CompanyID, trn);
                _notifications.InsertNotification(submittedBy, req.CompanyID,
                    "Document approved",
                    "Document " + documentNumber + " was approved and posted.",
                    "Approval", documentGuid, trn);

                clsAuditService.LogUpdate(req.UserID, req.CompanyID, "Approval", "tbl_ApprovalRequest", 0,
                    documentNumber, "Final approval and post");

                trn.Commit();
                result.Success = true;
                result.Message = "Document approved and posted.";
                result.DocumentStatus = (int)DocumentStatus.Posted;
                return result;
            }
            catch (Exception ex)
            {
                trn.Rollback();
                result.Message = ex.Message;
                return result;
            }
            finally
            {
                con.Close();
            }
        }

        private static bool IsUserInLevelGroup(ApprovalPolicyLevelRow level, int userId)
        {
            return level.MemberUserIds != null && level.MemberUserIds.Contains(userId);
        }

        private void NotifyLevelGroup(ApprovalPolicyLevelRow level, int companyId, int documentTypeId,
            string documentGuid, string documentNumber, SqlTransaction trn)
        {
            if (level.MemberUserIds == null) return;

            foreach (int approverUserId in level.MemberUserIds)
            {
                NotifyApprover(approverUserId, companyId, documentTypeId, documentGuid,
                    documentNumber, level.LevelName, trn);
            }
        }

        private void NotifyApprover(int approverUserId, int companyId, int documentTypeId, string documentGuid,
            string documentNumber, string levelName, SqlTransaction trn)
        {
            if (approverUserId <= 0) return;

            _notifications.InsertNotification(approverUserId, companyId,
                "Approval required",
                "Document " + documentNumber + " awaits your approval (" + levelName + ").",
                "ApprovalPending", documentGuid, trn);
        }

        private bool TryGetDocumentMeta(int documentTypeId, string documentGuid, int companyId, SqlTransaction trn,
            out int branchId, out decimal amount, out string documentNumber, out int currentStatus, out int submittedBy)
        {
            branchId = 0;
            amount = 0;
            documentNumber = "";
            currentStatus = (int)DocumentStatus.Draft;
            submittedBy = 0;

            if (clsDocumentPostingService.IsCashVoucherType(documentTypeId))
            {
                DataTable dt = _cashHeader.SelectCashVoucherHeaderByGuid(documentGuid,
                    Simulate.StringToDate("1900-01-01"), Simulate.StringToDate("2300-01-01"),
                    0, 0, companyId, "00000000-0000-0000-0000-000000000000", trn);
                if (dt == null || dt.Rows.Count == 0) return false;

                DataRow row = dt.Rows[0];
                branchId = Simulate.Integer32(row["BranchID"]);
                amount = Simulate.Decimal(row["Amount"]);
                documentNumber = Simulate.String(row["VoucherNo"]);
                currentStatus = Simulate.Integer32(row["DocumentStatus"]);
                submittedBy = Simulate.Integer32(row["CreationUserID"]);
                return true;
            }

            if (clsDocumentPostingService.IsCreditNoteType(documentTypeId))
            {
                DataTable dt = new clsCreditNoteHeader().SelectCreditNoteHeaderByGuid(
                    documentGuid,
                    Simulate.StringToDate("1900-01-01"),
                    Simulate.StringToDate("2300-01-01"),
                    0, 0, companyId,
                    trn);
                if (dt == null || dt.Rows.Count == 0) return false;

                DataRow row = dt.Rows[0];
                branchId = Simulate.Integer32(row["BranchID"]);
                amount = Simulate.Decimal(row["Amount"]);
                documentNumber = Simulate.String(row["VoucherNo"]);
                currentStatus = Simulate.Integer32(row["DocumentStatus"]);
                submittedBy = Simulate.Integer32(row["CreationUserID"]);
                return true;
            }

            if (clsApprovalDocumentTypes.IsInvoiceHeaderType(documentTypeId))
            {
                DataTable dt = new clsInvoiceHeader().SelectInvoiceHeaderByGuid(
                    documentGuid,
                    Simulate.StringToDate("1900-01-01"),
                    Simulate.StringToDate("2300-01-01"),
                    documentTypeId,
                    0,
                    0,
                    companyId,
                    trn);
                if (dt == null || dt.Rows.Count == 0) return false;

                DataRow row = dt.Rows[0];
                branchId = Simulate.Integer32(row["BranchID"]);
                amount = Simulate.Decimal(row["TotalInvoice"]);
                documentNumber = Simulate.String(row["InvoiceNo"]);
                currentStatus = row.Table.Columns.Contains("DocumentStatus")
                    ? Simulate.Integer32(row["DocumentStatus"])
                    : (int)DocumentStatus.Posted;
                submittedBy = Simulate.Integer32(row["CreationUserId"]);
                return true;
            }

            if (clsApprovalDocumentTypes.IsHcmType(documentTypeId))
                return clsHcmApprovalDocuments.TryGetDocumentMeta(
                    documentTypeId, documentGuid, companyId, trn,
                    out branchId, out amount, out documentNumber, out currentStatus, out submittedBy);

            DataTable jv = _jvHeader.SelectJournalVoucherHeaderByGuid(documentGuid, companyId, trn);
            if (jv == null || jv.Rows.Count == 0) return false;

            DataRow jvRow = jv.Rows[0];
            branchId = Simulate.Integer32(jvRow["BranchID"]);
            amount = _jvHeader.GetJournalVoucherAmount(documentGuid, companyId, trn);
            documentNumber = Simulate.String(jvRow["JVNumber"]);
            currentStatus = Simulate.Integer32(jvRow["DocumentStatus"]);
            submittedBy = Simulate.Integer32(jvRow["CreationUserId"]);
            return true;
        }

        private void SetDocumentStatus(int documentTypeId, string documentGuid, int status, int userId, int companyId,
            SqlTransaction trn)
        {
            if (clsDocumentPostingService.IsCashVoucherType(documentTypeId))
                _cashHeader.UpdateDocumentStatus(documentGuid, status, userId, companyId, trn);
            else if (clsDocumentPostingService.IsCreditNoteType(documentTypeId))
                new clsCreditNoteHeader().UpdateDocumentStatus(documentGuid, status, userId, companyId, trn);
            else if (clsApprovalDocumentTypes.IsInvoiceHeaderType(documentTypeId))
                new clsInvoiceHeader().UpdateDocumentStatus(documentGuid, status, userId, companyId, trn);
            else if (clsApprovalDocumentTypes.IsHcmType(documentTypeId))
                clsHcmApprovalDocuments.SetDocumentStatus(documentTypeId, documentGuid, status, userId, companyId, trn);
            else
                _jvHeader.UpdateDocumentStatus(documentGuid, status, userId, companyId, trn);
        }

        public ApprovalDocumentProgress GetDocumentProgress(string documentGuid, int companyId, int documentTypeId = 0)
        {
            new clsDataBaseVersion().EnsureApprovalWorkflowSchema(companyId);

            var progress = new ApprovalDocumentProgress
            {
                DocumentGuid = documentGuid ?? "",
                DocumentStatus = (int)DocumentStatus.Draft,
                RequestStatus = -1,
            };

            if (string.IsNullOrWhiteSpace(documentGuid))
                return progress;

            int branchId = 0;
            decimal amount = 0;
            int docStatus = (int)DocumentStatus.Draft;

            if (documentTypeId <= 0)
                TryResolveDocumentType(documentGuid, companyId, out documentTypeId, out branchId, out amount, out docStatus);
            else
                TryGetDocumentMeta(documentTypeId, documentGuid, companyId, null,
                    out branchId, out amount, out _, out docStatus, out _);

            progress.DocumentStatus = docStatus;
            progress.DocumentAmount = amount;

            if (!clsDocumentPostingService.IsMvpApprovalType(documentTypeId))
            {
                progress.InfoMessage = "Approval workflow is not configured for this document type.";
                return progress;
            }

            DataTable actions = _requests.SelectActions(documentGuid, companyId);
            DataTable reqDt = _requests.SelectByDocumentGuid(documentGuid, companyId);

            List<ApprovalPolicyLevelRow> levels;
            int currentLevel = 0;
            int requestStatus = -1;
            int rejectLevel = 0;
            int policyId = 0;

            if (reqDt != null && reqDt.Rows.Count > 0)
            {
                DataRow req = reqDt.Rows[0];
                progress.RequestGuid = Simulate.String(req["Guid"]);
                currentLevel = Simulate.Integer32(req["CurrentLevel"]);
                requestStatus = Simulate.Integer32(req["Status"]);
                progress.RequestStatus = requestStatus;
                progress.CurrentLevel = currentLevel;
                documentTypeId = Simulate.Integer32(req["DocumentTypeID"]);
                amount = Simulate.Decimal(req["TotalAmount"]);
                progress.DocumentAmount = amount;
                policyId = Simulate.Integer32(req["PolicyID"]);
                progress.PolicyEnabled = true;
                levels = _policy.GetLevels(policyId, companyId);
            }
            else
            {
                var policy = _policy.ResolveEnabledPolicy(companyId, documentTypeId, branchId);
                if (policy == null)
                    policy = _policy.ResolveEnabledPolicyAnyBranch(companyId, documentTypeId);

                if (policy == null)
                {
                    var disabled = _policy.ResolvePolicy(companyId, documentTypeId, branchId, amount);
                    if (disabled == null)
                        disabled = _policy.ResolvePolicy(companyId, documentTypeId, 0, amount);
                    if (disabled == null)
                        progress.InfoMessage = "No approval policy found for Manual Journal Voucher. Configure it in Approval Policies.";
                    else if (!disabled.IsEnabled)
                        progress.InfoMessage = "Approval policy exists but is disabled. Enable it in Approval Policies.";
                    else
                        progress.InfoMessage = "No enabled approval policy matches this document branch.";
                    return progress;
                }

                progress.PolicyEnabled = true;
                policyId = policy.ID;
                levels = _policy.GetLevels(policyId, companyId);
            }

            if (levels == null || levels.Count == 0)
            {
                progress.InfoMessage = "Approval policy is enabled but has no levels configured.";
                return progress;
            }

            if (actions != null)
            {
                DateTime latestReject = DateTime.MinValue;
                foreach (DataRow actionRow in actions.Rows)
                {
                    if (Simulate.Integer32(actionRow["ActionType"]) != (int)ApprovalActionType.Reject)
                        continue;
                    DateTime dt = Simulate.StringToDate(actionRow["ActionDate"]);
                    if (dt >= latestReject)
                    {
                        latestReject = dt;
                        rejectLevel = Simulate.Integer32(actionRow["LevelNo"]);
                    }
                }
            }

            foreach (var level in levels)
            {
                bool hasMembers = level.MemberUserIds != null && level.MemberUserIds.Count > 0;
                bool applicable = hasMembers && clsApprovalPolicy.IsLevelApplicableForAmount(level, amount);

                int approvedCount = CountDistinctApprovalsAtLevel(actions, level.LevelNo);
                int requiredCount = level.RequireAllApprovers
                    ? Math.Max(1, level.MemberUserIds?.Count ?? 1)
                    : 1;

                string state;
                if (!hasMembers)
                    state = "NotConfigured";
                else if (!applicable && string.IsNullOrEmpty(progress.RequestGuid))
                    state = "NotApplicable";
                else
                    state = ResolveLevelState(
                        docStatus, requestStatus, currentLevel, rejectLevel,
                        level.LevelNo, approvedCount, requiredCount, level.RequireAllApprovers);

                GetLatestLevelAction(actions, level.LevelNo, out string lastBy, out DateTime? lastDate);

                progress.Levels.Add(new ApprovalLevelProgressRow
                {
                    LevelNo = level.LevelNo,
                    LevelName = level.LevelName,
                    MinAmount = level.MinAmount,
                    MaxAmount = level.MaxAmount,
                    RequireAllApprovers = level.RequireAllApprovers,
                    State = state,
                    IsApplicableForAmount = applicable,
                    ApprovedCount = approvedCount,
                    RequiredCount = requiredCount,
                    ApproverNames = level.MemberUserNames ?? new List<string>(),
                    LastActionByUserName = lastBy,
                    LastActionDate = lastDate,
                });
            }

            if (progress.Levels.TrueForAll(l => l.State == "NotApplicable"))
                progress.InfoMessage = "Document amount does not fall within any approval level range.";

            return progress;
        }

        private static string ResolveLevelState(
            int documentStatus,
            int requestStatus,
            int currentLevel,
            int rejectLevel,
            int levelNo,
            int approvedCount,
            int requiredCount,
            bool requireAllApprovers)
        {
            if (documentStatus == (int)DocumentStatus.Posted)
                return "Approved";

            if (requestStatus == (int)ApprovalRequestStatus.Rejected)
            {
                if (rejectLevel > 0 && levelNo == rejectLevel) return "Rejected";
                if (rejectLevel > 0 && levelNo < rejectLevel) return "Approved";
                return "NotStarted";
            }

            if (requestStatus == (int)ApprovalRequestStatus.Pending)
            {
                if (levelNo < currentLevel) return "Approved";
                if (levelNo > currentLevel) return "NotStarted";
                if (requireAllApprovers && approvedCount >= requiredCount) return "Approved";
                return "Current";
            }

            if (requestStatus == (int)ApprovalRequestStatus.Approved)
                return "Approved";

            return "NotStarted";
        }

        private static int CountDistinctApprovalsAtLevel(DataTable actions, int levelNo)
        {
            if (actions == null) return 0;

            var seen = new HashSet<int>();
            foreach (DataRow row in actions.Rows)
            {
                if (Simulate.Integer32(row["ActionType"]) != (int)ApprovalActionType.Approve) continue;
                if (Simulate.Integer32(row["LevelNo"]) != levelNo) continue;
                seen.Add(Simulate.Integer32(row["ActionByUserId"]));
            }

            return seen.Count;
        }

        private static void GetLatestLevelAction(
            DataTable actions, int levelNo, out string userName, out DateTime? actionDate)
        {
            userName = "";
            actionDate = null;
            if (actions == null) return;

            DateTime latest = DateTime.MinValue;
            foreach (DataRow row in actions.Rows)
            {
                if (Simulate.Integer32(row["LevelNo"]) != levelNo) continue;
                int actionType = Simulate.Integer32(row["ActionType"]);
                if (actionType != (int)ApprovalActionType.Approve &&
                    actionType != (int)ApprovalActionType.Reject)
                    continue;

                DateTime dt = Simulate.StringToDate(row["ActionDate"]);
                if (dt >= latest)
                {
                    latest = dt;
                    userName = Simulate.String(row["ActionByUserName"]);
                    actionDate = dt;
                }
            }
        }

        private void TryResolveDocumentType(
            string documentGuid, int companyId,
            out int documentTypeId, out int branchId, out decimal amount, out int documentStatus)
        {
            documentTypeId = 0;
            branchId = 0;
            amount = 0;
            documentStatus = (int)DocumentStatus.Draft;

            foreach (int typeId in new[] { 1, 12, 13, 20, 21, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 22, 23, 27, 28, 29, 30 })
            {
                if (TryGetDocumentMeta(typeId, documentGuid, companyId, null,
                        out branchId, out amount, out _, out documentStatus, out _))
                {
                    documentTypeId = typeId;
                    return;
                }
            }
        }
    }
}
