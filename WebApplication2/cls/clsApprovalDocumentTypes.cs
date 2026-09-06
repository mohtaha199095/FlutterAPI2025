using System;
using WebApplication2.MainClasses;
using static WebApplication2.MainClasses.clsEnum;

namespace WebApplication2.cls
{
    /// <summary>
    /// Document types that participate in the multi-level approval workflow.
    /// </summary>
    public static class clsApprovalDocumentTypes
    {
        public static readonly int[] InvoiceHeaderTypeIds =
        {
            (int)VoucherType.PurchaseInvoice,
            (int)VoucherType.SalesInvoice,
            (int)VoucherType.SalesRefund,
            (int)VoucherType.SalesOffer,
            (int)VoucherType.PurchaseOffer,
            (int)VoucherType.PurchaseRefund,
            (int)VoucherType.GoodRecipt,
            (int)VoucherType.GoodIssue,
            (int)VoucherType.POSSalesInvoice,
            (int)VoucherType.POSSalesInvoicereturn,
            (int)VoucherType.PurchaseInvoiceFromFinancing,
        };

        public static readonly int[] HcmTypeIds =
        {
            clsHcmApprovalDocuments.TypePayroll,
            clsHcmApprovalDocuments.TypeEmployeeContract,
            clsHcmApprovalDocuments.TypeEmployeeSalaryElement,
            clsHcmApprovalDocuments.TypePayrollPeriod,
            clsHcmApprovalDocuments.TypeEmployeeShiftAssignment,
            clsHcmApprovalDocuments.TypeLeaveRequest,
        };

        public static bool IsHcmType(int documentTypeId) =>
            clsHcmApprovalDocuments.IsHcmType(documentTypeId);

        public static bool IsInvoiceHeaderType(int documentTypeId) =>
            Array.IndexOf(InvoiceHeaderTypeIds, documentTypeId) >= 0;

        public static bool IsBudgetType(int documentTypeId) =>
            documentTypeId == clsBudget.TypeBudget ||
            documentTypeId == (int)VoucherType.Budget;

        public static bool IsSupported(int documentTypeId) =>
            documentTypeId == (int)VoucherType.ManualJV ||
            documentTypeId == (int)VoucherType.CashPayment ||
            documentTypeId == (int)VoucherType.Cashrecivable ||
            documentTypeId == (int)VoucherType.creditNote ||
            documentTypeId == (int)VoucherType.debitNote ||
            IsInvoiceHeaderType(documentTypeId) ||
            IsHcmType(documentTypeId) ||
            IsBudgetType(documentTypeId);
    }
}
