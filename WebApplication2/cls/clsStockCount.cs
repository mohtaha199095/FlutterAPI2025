using System;
using System.Collections.Generic;
using System.Data;
using Microsoft.Data.SqlClient;
using Newtonsoft.Json;
using WebApplication2.MainClasses;

namespace WebApplication2.cls
{
    /// <summary>
    /// One counted item from a physical stock count.
    /// </summary>
    public class StockCountLine
    {
        public string ItemGuid { get; set; }
        public decimal CountedQty { get; set; }
    }

    public class StockCountResult
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public string IncreaseGuid { get; set; }
        public string DecreaseGuid { get; set; }
        public int AdjustedItems { get; set; }
    }

    /// <summary>
    /// Physical stock count / reconciliation. Compares the counted quantity per item
    /// against system on-hand (per store) and posts the variance as an adjustment:
    ///  - positive variance  -> Good Receipt (stock found)
    ///  - negative variance  -> Good Issue   (stock missing)
    /// Both adjustments use the item's average cost and the supplied adjustment account as
    /// the P&L contra, and run inside ONE transaction so the count posts atomically.
    /// </summary>
    public class clsStockCount
    {
        public StockCountResult PostStockCount(
            int branchId, int storeId, List<StockCountLine> lines,
            int adjustmentAccountId, string note,
            int companyId, int userId, DateTime countDate, SqlTransaction trn,
            bool bypassApprovalCheck = false)
        {
            var result = new StockCountResult { Success = false };

            if (storeId <= 0)
            { result.Message = "Store is required for a stock count."; return result; }
            if (lines == null || lines.Count == 0)
            { result.Message = "At least one counted line is required."; return result; }

            clsItems items = new clsItems();
            clsInvoiceHeader header = new clsInvoiceHeader();

            // Fall back to the inventory account when no explicit adjustment account is given.
            int contraAccount = adjustmentAccountId;
            if (contraAccount <= 0)
            {
                cls_AccountSetting accountSetting = new cls_AccountSetting();
                DataTable dtAcc = accountSetting.SelectAccountSetting(0, 0, companyId, trn);
                contraAccount = header.GetValueFromDT(
                    dtAcc, "AccountRefID", Simulate.String((int)clsEnum.AccountMainSetting.Inventory), 2);
            }

            List<DBInvoiceDetails> increaseLines = new List<DBInvoiceDetails>();
            List<DBInvoiceDetails> decreaseLines = new List<DBInvoiceDetails>();
            int adjusted = 0;

            foreach (StockCountLine line in lines)
            {
                if (line == null || string.IsNullOrWhiteSpace(line.ItemGuid)) continue;
                if (line.CountedQty < 0)
                {
                    result.Message = "Counted quantity cannot be negative.";
                    return result;
                }
                decimal onHand = items.GetOnHandQty(line.ItemGuid, storeId, companyId, trn);
                decimal variance = line.CountedQty - onHand;
                if (variance == 0) continue;

                decimal cost = items.GetAvgCost(line.ItemGuid, companyId, trn);
                decimal qty = Math.Abs(variance);
                var detail = new DBInvoiceDetails
                {
                    Guid = Guid.Empty,
                    HeaderGuid = Guid.Empty,
                    ItemGuid = Simulate.Guid(line.ItemGuid),
                    Qty = qty,
                    TotalQTY = qty,
                    PriceBeforeTax = cost,
                    AVGCostPerUnit = cost,
                    BranchID = branchId,
                    StoreID = storeId,
                    CompanyID = companyId,
                    IsCounted = true,
                    InvoiceDate = countDate,
                };
                if (variance > 0)
                {
                    detail.InvoiceTypeID = (int)clsEnum.VoucherType.GoodRecipt;
                    detail.RowIndex = increaseLines.Count + 1;
                    increaseLines.Add(detail);
                }
                else
                {
                    detail.InvoiceTypeID = (int)clsEnum.VoucherType.GoodIssue;
                    detail.RowIndex = decreaseLines.Count + 1;
                    decreaseLines.Add(detail);
                }
                adjusted++;
            }

            if (adjusted == 0)
            { result.Success = true; result.Message = "Count matches system stock; no adjustment needed."; return result; }

            if (increaseLines.Count > 0)
            {
                string guid = PostAdjustment(
                    branchId, storeId, (int)clsEnum.VoucherType.GoodRecipt,
                    increaseLines, note, companyId, userId, countDate, contraAccount, trn, bypassApprovalCheck, out string err);
                if (string.IsNullOrEmpty(guid))
                { result.Message = "Stock count increase failed: " + err; return result; }
                result.IncreaseGuid = guid;
            }

            if (decreaseLines.Count > 0)
            {
                string guid = PostAdjustment(
                    branchId, storeId, (int)clsEnum.VoucherType.GoodIssue,
                    decreaseLines, note, companyId, userId, countDate, contraAccount, trn, bypassApprovalCheck, out string err);
                if (string.IsNullOrEmpty(guid))
                { result.Message = "Stock count decrease failed: " + err; return result; }
                result.DecreaseGuid = guid;
            }

            result.Success = true;
            result.AdjustedItems = adjusted;
            result.Message = $"Stock count posted. {adjusted} item(s) adjusted.";
            return result;
        }

        private string PostAdjustment(
            int branchId, int storeId, int invoiceTypeId,
            List<DBInvoiceDetails> details, string note,
            int companyId, int userId, DateTime date, int accountId,
            SqlTransaction trn, bool bypassApprovalCheck, out string error)
        {
            error = "";
            string detailsJson = JsonConvert.SerializeObject(details);
            clsInvoiceHeader header = new clsInvoiceHeader();
            ApiResponse<string> resp = header.InsertInvoiceHeaderWithDetails(
                branchId, 0, storeId, 0,
                0, 0, "STOCKCOUNT", 0, 0,
                invoiceTypeId, true, note, companyId,
                0, "", "",
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
