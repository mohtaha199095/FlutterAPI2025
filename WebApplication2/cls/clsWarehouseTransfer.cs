using System;
using System.Collections.Generic;
using System.Data;
using Microsoft.Data.SqlClient;
using Newtonsoft.Json;
using WebApplication2.MainClasses;

namespace WebApplication2.cls
{
    /// <summary>
    /// A single piece of a warehouse transfer (one item moving between stores).
    /// </summary>
    public class WarehouseTransferLine
    {
        public string ItemGuid { get; set; }
        public decimal Qty { get; set; }
    }

    /// <summary>
    /// Result of a transfer attempt.
    /// </summary>
    public class WarehouseTransferResult
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public string IssueGuid { get; set; }
        public string ReceiptGuid { get; set; }
    }

    /// <summary>
    /// Atomic stock transfer between two stores. Implemented on top of the existing
    /// invoice engine: it posts a Good Issue from the source store and a Good Receipt
    /// into the destination store inside ONE transaction, so a transfer can never be
    /// left half-applied. Both legs use the item's current average cost and the Inventory
    /// account as the contra, so the net GL impact is zero (stock just moves stores).
    /// </summary>
    public class clsWarehouseTransfer
    {
        public WarehouseTransferResult PostTransfer(
            int branchId, int sourceStoreId, int destStoreId,
            List<WarehouseTransferLine> lines, string note,
            int companyId, int userId, DateTime transferDate, SqlTransaction trn,
            bool bypassApprovalCheck = false)
        {
            var result = new WarehouseTransferResult { Success = false };

            if (sourceStoreId <= 0 || destStoreId <= 0)
            { result.Message = "Source and destination stores are required."; return result; }
            if (sourceStoreId == destStoreId)
            { result.Message = "Source and destination stores must be different."; return result; }
            if (lines == null || lines.Count == 0)
            { result.Message = "At least one transfer line is required."; return result; }

            // Resolve the inventory account so both legs net to zero in the GL.
            cls_AccountSetting accountSetting = new cls_AccountSetting();
            DataTable dtAcc = accountSetting.SelectAccountSetting(0, 0, companyId, trn);
            clsInvoiceHeader header = new clsInvoiceHeader();
            int inventoryAcc = header.GetValueFromDT(
                dtAcc, "AccountRefID", Simulate.String((int)clsEnum.AccountMainSetting.Inventory), 2);

            string issueGuid = PostLeg(
                branchId, sourceStoreId, (int)clsEnum.VoucherType.GoodIssue,
                lines, note, companyId, userId, transferDate, inventoryAcc, "", trn, bypassApprovalCheck, out string issueError);
            if (string.IsNullOrEmpty(issueGuid))
            { result.Message = "Transfer (issue leg) failed: " + issueError; return result; }

            string receiptGuid = PostLeg(
                branchId, destStoreId, (int)clsEnum.VoucherType.GoodRecipt,
                lines, note, companyId, userId, transferDate, inventoryAcc, issueGuid, trn, bypassApprovalCheck, out string receiptError);
            if (string.IsNullOrEmpty(receiptGuid))
            { result.Message = "Transfer (receipt leg) failed: " + receiptError; return result; }

            result.Success = true;
            result.IssueGuid = issueGuid;
            result.ReceiptGuid = receiptGuid;
            result.Message = "Transfer posted.";
            return result;
        }

        private string PostLeg(
            int branchId, int storeId, int invoiceTypeId,
            List<WarehouseTransferLine> lines, string note,
            int companyId, int userId, DateTime date, int accountId,
            string relatedInvoiceGuid, SqlTransaction trn, bool bypassApprovalCheck, out string error)
        {
            error = "";
            clsItems items = new clsItems();
            List<DBInvoiceDetails> details = new List<DBInvoiceDetails>();
            for (int i = 0; i < lines.Count; i++)
            {
                if (lines[i] == null || string.IsNullOrWhiteSpace(lines[i].ItemGuid) || lines[i].Qty <= 0)
                    continue;
                decimal cost = items.GetAvgCost(lines[i].ItemGuid, companyId, trn);
                details.Add(new DBInvoiceDetails
                {
                    Guid = Guid.Empty,
                    HeaderGuid = Guid.Empty,
                    RowIndex = i + 1,
                    ItemGuid = Simulate.Guid(lines[i].ItemGuid),
                    Qty = lines[i].Qty,
                    TotalQTY = lines[i].Qty,
                    PriceBeforeTax = cost,
                    AVGCostPerUnit = cost,
                    BranchID = branchId,
                    StoreID = storeId,
                    CompanyID = companyId,
                    InvoiceTypeID = invoiceTypeId,
                    IsCounted = true,
                    InvoiceDate = date,
                });
            }
            if (details.Count == 0) { error = "No valid lines."; return ""; }

            string detailsJson = JsonConvert.SerializeObject(details);
            clsInvoiceHeader header = new clsInvoiceHeader();
            ApiResponse<string> resp = header.InsertInvoiceHeaderWithDetails(
                branchId, 0, storeId, 0,
                0, 0, "WHTRANSFER", 0, 0,
                invoiceTypeId, true, note, companyId,
                0, "", relatedInvoiceGuid,
                0, 0,
                "", 0,
                date, userId, accountId, 0, (int)clsEnum.DocumentStatus.Posted,
                0, 0, 1,
                detailsJson, trn, bypassApprovalCheck);

            if (resp == null || !resp.Success)
            {
                error = resp == null ? "Unknown error." : resp.Message;
                return "";
            }
            return resp.Data;
        }
    }
}
