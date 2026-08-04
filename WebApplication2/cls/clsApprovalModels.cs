using System;
using System.Collections.Generic;

namespace WebApplication2.cls
{
    public class ApprovalPolicyRow
    {
        public int ID { get; set; }
        public int CompanyID { get; set; }
        public int DocumentTypeID { get; set; }
        public int BranchID { get; set; }
        public bool IsEnabled { get; set; }
        public decimal MinAmount { get; set; }
        public decimal MaxAmount { get; set; }
        public bool AllowSelfApproval { get; set; }
        public string DocumentTypeAName { get; set; }
        public string DocumentTypeEName { get; set; }
    }

    public class ApprovalPolicyLevelRow
    {
        public int ID { get; set; }
        public int PolicyID { get; set; }
        public int LevelNo { get; set; }
        public string LevelName { get; set; }
        public decimal MinAmount { get; set; }
        public decimal MaxAmount { get; set; }
        public bool RequireAllApprovers { get; set; }
        public int ApproverUserID { get; set; }
        public string ApproverUserName { get; set; }
        public int MinApproversRequired { get; set; }
        public List<int> MemberUserIds { get; set; } = new List<int>();
        public List<string> MemberUserNames { get; set; } = new List<string>();
    }

    public class ApprovalPendingRow
    {
        public string RequestGuid { get; set; }
        public int DocumentTypeID { get; set; }
        public string DocumentGuid { get; set; }
        public string DocumentNumber { get; set; }
        public int CurrentLevel { get; set; }
        public int TotalLevels { get; set; }
        public string LevelName { get; set; }
        public decimal TotalAmount { get; set; }
        public DateTime SubmittedDate { get; set; }
        public int SubmittedByUserId { get; set; }
        public string SubmittedByUserName { get; set; }
        public string DocumentTypeAName { get; set; }
        public string DocumentTypeEName { get; set; }
        public string Notes { get; set; }
    }

    public class ApprovalActionRow
    {
        public int LevelNo { get; set; }
        public string LevelName { get; set; }
        public int ActionType { get; set; }
        public string ActionName { get; set; }
        public int ActionByUserId { get; set; }
        public string ActionByUserName { get; set; }
        public DateTime ActionDate { get; set; }
        public string Comments { get; set; }
    }

    public class ApprovalNotificationRow
    {
        public int ID { get; set; }
        public string Title { get; set; }
        public string Body { get; set; }
        public string EntityType { get; set; }
        public string EntityGuid { get; set; }
        public bool IsRead { get; set; }
        public DateTime CreatedDate { get; set; }
    }

    public class ApprovalSubmitRequest
    {
        public int CompanyID { get; set; }
        public int UserID { get; set; }
        public int DocumentTypeID { get; set; }
        public string DocumentGuid { get; set; }
        public string Comments { get; set; }
    }

    public class ApprovalDecisionRequest
    {
        public int CompanyID { get; set; }
        public int UserID { get; set; }
        public string RequestGuid { get; set; }
        public string Comments { get; set; }
    }

    public class ApprovalPolicySaveRequest
    {
        public int CompanyID { get; set; }
        public int UserID { get; set; }
        public int ID { get; set; }
        public int DocumentTypeID { get; set; }
        public int BranchID { get; set; }
        public bool IsEnabled { get; set; }
        public decimal MinAmount { get; set; }
        public decimal MaxAmount { get; set; }
        public bool AllowSelfApproval { get; set; }
        public List<ApprovalPolicyLevelRow> Levels { get; set; }
    }

    public class ApprovalResult
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public int DocumentStatus { get; set; }
        public string RequestGuid { get; set; }
    }

    public class ApprovalLevelProgressRow
    {
        public int LevelNo { get; set; }
        public string LevelName { get; set; }
        public decimal MinAmount { get; set; }
        public decimal MaxAmount { get; set; }
        public bool RequireAllApprovers { get; set; }
        /// <summary>Approved, Current, NotStarted, Rejected, NotApplicable, NotConfigured</summary>
        public string State { get; set; }
        public bool IsApplicableForAmount { get; set; }
        public int ApprovedCount { get; set; }
        public int RequiredCount { get; set; }
        public List<string> ApproverNames { get; set; } = new List<string>();
        public string LastActionByUserName { get; set; }
        public DateTime? LastActionDate { get; set; }
    }

    public class ApprovalDocumentProgress
    {
        public string DocumentGuid { get; set; }
        public string RequestGuid { get; set; }
        public int DocumentStatus { get; set; }
        public int RequestStatus { get; set; }
        public int CurrentLevel { get; set; }
        public bool PolicyEnabled { get; set; }
        public decimal DocumentAmount { get; set; }
        public string InfoMessage { get; set; }
        public List<ApprovalLevelProgressRow> Levels { get; set; } = new List<ApprovalLevelProgressRow>();
    }
}
