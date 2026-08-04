using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Microsoft.Data.SqlClient;
using Newtonsoft.Json;
using WebApplication2.MainClasses;

namespace WebApplication2.cls
{
    public class InventoryDemoSeedResult
    {
        public bool Success { get; set; }
        public string Message { get; set; } = "";
        public Dictionary<string, int> Counts { get; set; } = new Dictionary<string, int>();
        public List<string> Warnings { get; set; } = new List<string>();
    }

    internal class DemoItemRow
    {
        public Guid Guid { get; set; }
        public string ItemName { get; set; }
        public decimal Cost { get; set; }
        public decimal SalesPrice { get; set; }
    }

    /// <summary>
    /// Seeds realistic demo inventory documents (5–10 per transaction type, 3–5 lines each)
    /// using the production posting engine so every inventory list/dashboard screen has data.
    /// Documents are tagged with Note starting with "DEMO-" for easy identification.
    /// </summary>
    public class clsInventoryDemoData
    {
        const string DemoTag = "DEMO-";
        const int Posted = (int)clsEnum.DocumentStatus.Posted;
        const int MovementDocMin = 2;
        const int MovementDocMax = 4;
        const bool DemoBypassApproval = true;

        /// <summary>
        /// Seeds warehouse transfers, stock counts, manual stock adjustments (GI/GR),
        /// and sales returns. Always additive — safe to run multiple times.
        /// </summary>
        public InventoryDemoSeedResult SeedMovementTransactions(int companyId, int userId)
        {
            var result = new InventoryDemoSeedResult();
            var rng = new Random();

            DemoContext ctx = LoadContext(companyId);
            if (ctx.Items.Count < 2)
            {
                result.Success = false;
                result.Message = "At least 2 active items (without lot/serial tracking) are required.";
                return result;
            }
            if (ctx.BranchId <= 0 || ctx.StoreIds.Count == 0)
            {
                result.Success = false;
                result.Message = "At least one branch and store are required.";
                return result;
            }

            EnsureDemoStores(ctx, userId);
            clsInvoiceHeader header = new clsInvoiceHeader();
            TopUpAllStores(result, rng, header, ctx, userId);

            SeedStockAdjustments(result, rng, header, ctx, userId);
            SeedSalesReturns(result, rng, header, ctx, userId);
            SeedTransfers(result, rng, ctx, userId);
            SeedStockCounts(result, rng, ctx, userId);

            result.Success = true;
            int total = result.Counts.Values.Sum();
            result.Message = $"Seeded {total} movement demo document(s).";
            return result;
        }

        public InventoryDemoSeedResult Seed(int companyId, int userId, bool force = false)
        {
            var result = new InventoryDemoSeedResult();
            var rng = new Random();

            if (!force && ExistingDemoCount(companyId) > 0)
            {
                result.Success = false;
                result.Message =
                    "Demo inventory data already exists (notes starting with DEMO-). Pass force=true to add more.";
                return result;
            }

            DemoContext ctx = LoadContext(companyId);
            if (ctx.Items.Count < 2)
            {
                result.Success = false;
                result.Message = "At least 2 active items (without lot/serial tracking) are required.";
                return result;
            }
            if (ctx.BranchId <= 0 || ctx.StoreIds.Count == 0)
            {
                result.Success = false;
                result.Message = "At least one branch and store are required.";
                return result;
            }

            clsInvoiceHeader header = new clsInvoiceHeader();

            // Build stock first so sales, issues and transfers can post.
            SeedInvoiceType(result, rng, header, ctx, userId,
                (int)clsEnum.VoucherType.PurchaseInvoice, true, ctx.VendorId, "");
            SeedInvoiceType(result, rng, header, ctx, userId,
                (int)clsEnum.VoucherType.GoodRecipt, true, 0, "");

            SeedInvoiceType(result, rng, header, ctx, userId,
                (int)clsEnum.VoucherType.SalesInvoice, true, ctx.CustomerId, "");
            SeedSalesReturns(result, rng, header, ctx, userId);
            SeedInvoiceType(result, rng, header, ctx, userId,
                (int)clsEnum.VoucherType.SalesOffer, false, ctx.CustomerId, "");
            SeedInvoiceType(result, rng, header, ctx, userId,
                (int)clsEnum.VoucherType.POSSalesInvoice, true, ctx.CustomerId, "");
            SeedInvoiceType(result, rng, header, ctx, userId,
                (int)clsEnum.VoucherType.POSSalesInvoicereturn, true, ctx.CustomerId, "");

            SeedInvoiceType(result, rng, header, ctx, userId,
                (int)clsEnum.VoucherType.PurchaseRefund, true, ctx.VendorId, "");
            SeedInvoiceType(result, rng, header, ctx, userId,
                (int)clsEnum.VoucherType.PurchaseOffer, false, ctx.VendorId, "");

            EnsureDemoStores(ctx, userId);
            TopUpAllStores(result, rng, header, ctx, userId);
            SeedStockAdjustments(result, rng, header, ctx, userId);

            SeedInvoiceType(result, rng, header, ctx, userId,
                (int)clsEnum.VoucherType.GoodIssue, true, 0, "");

            SeedTransfers(result, rng, ctx, userId);
            SeedStockCounts(result, rng, ctx, userId);

            result.Success = true;
            int total = result.Counts.Values.Sum();
            result.Message = $"Seeded {total} inventory demo document(s).";
            return result;
        }

        int ExistingDemoCount(int companyId)
        {
            clsSQL sql = new clsSQL();
            object scalar = sql.ExecuteScalar(
                "SELECT COUNT(*) FROM tbl_InvoiceHeader WHERE CompanyID=@CompanyID AND Note LIKE 'DEMO-%'",
                CompanyPrm(companyId), sql.CreateDataBaseConnectionString(companyId), null);
            return Simulate.Integer32(scalar);
        }

        static SqlParameter[] CompanyPrm(int companyId) =>
            new[] { new SqlParameter("@CompanyID", SqlDbType.Int) { Value = companyId } };

        static void RunInCompanyTransaction(int companyId, Action<SqlTransaction> action)
        {
            clsSQL sql = new clsSQL();
            using SqlConnection con = new SqlConnection(sql.CreateDataBaseConnectionString(companyId));
            con.Open();
            using SqlTransaction trn = con.BeginTransaction();
            try
            {
                action(trn);
                trn.Commit();
            }
            catch
            {
                trn.Rollback();
                throw;
            }
        }

        DemoContext LoadContext(int companyId)
        {
            clsSQL sql = new clsSQL();
            string conn = sql.CreateDataBaseConnectionString(companyId);

            var ctx = new DemoContext { CompanyId = companyId };

            DataTable dtItems = sql.ExecuteQueryStatement(@"
SELECT TOP 30 Guid, AName,
       ISNULL(AVGCostPerUnit, 0) AS Cost,
       ISNULL(NULLIF(SalesPriceBeforeTax, 0), AVGCostPerUnit) AS SalesPrice
FROM tbl_Items
WHERE CompanyID = @CompanyID AND IsActive = 1
  AND ISNULL(TrackLot, 0) = 0 AND ISNULL(TrackSerial, 0) = 0
ORDER BY AName", conn, CompanyPrm(companyId), null);
            foreach (DataRow row in dtItems.Rows)
            {
                ctx.Items.Add(new DemoItemRow
                {
                    Guid = Simulate.Guid(Simulate.String(row["Guid"])),
                    ItemName = Simulate.String(row["AName"]),
                    Cost = Simulate.decimal_(row["Cost"]),
                    SalesPrice = Simulate.decimal_(row["SalesPrice"]),
                });
            }

            DataTable dtBranch = sql.ExecuteQueryStatement(
                "SELECT TOP 1 ID FROM tbl_Branch WHERE CompanyID=@CompanyID ORDER BY ID",
                conn, CompanyPrm(companyId), null);
            if (dtBranch.Rows.Count > 0)
                ctx.BranchId = Simulate.Integer32(dtBranch.Rows[0]["ID"]);

            DataTable dtStores = sql.ExecuteQueryStatement(
                "SELECT ID FROM tbl_Store WHERE CompanyID=@CompanyID ORDER BY ID",
                conn, CompanyPrm(companyId), null);
            foreach (DataRow row in dtStores.Rows)
                ctx.StoreIds.Add(Simulate.Integer32(row["ID"]));

            DataTable dtCust = sql.ExecuteQueryStatement(
                "SELECT TOP 1 ID FROM tbl_BusinessPartner WHERE CompanyID=@CompanyID AND Type=1 AND Active=1 ORDER BY ID",
                conn, CompanyPrm(companyId), null);
            if (dtCust.Rows.Count > 0)
                ctx.CustomerId = Simulate.Integer32(dtCust.Rows[0]["ID"]);

            DataTable dtVend = sql.ExecuteQueryStatement(
                "SELECT TOP 1 ID FROM tbl_BusinessPartner WHERE CompanyID=@CompanyID AND Type=2 AND Active=1 ORDER BY ID",
                conn, CompanyPrm(companyId), null);
            if (dtVend.Rows.Count > 0)
                ctx.VendorId = Simulate.Integer32(dtVend.Rows[0]["ID"]);

            DataTable dtPm = sql.ExecuteQueryStatement(
                "SELECT TOP 1 ID FROM tbl_PaymentMethod WHERE CompanyID=@CompanyID ORDER BY ID",
                conn, CompanyPrm(companyId), null);
            if (dtPm.Rows.Count > 0)
                ctx.PaymentMethodId = Simulate.Integer32(dtPm.Rows[0]["ID"]);

            cls_AccountSetting accountSetting = new cls_AccountSetting();
            DataTable dtAcc = accountSetting.SelectAccountSetting(0, 0, companyId, null);
            if (dtAcc != null && dtAcc.Rows.Count > 0)
            {
                clsInvoiceHeader header = new clsInvoiceHeader();
                ctx.InventoryAccountId = header.GetValueFromDT(
                    dtAcc, "AccountRefID",
                    Simulate.String((int)clsEnum.AccountMainSetting.Inventory), 2);
            }

            return ctx;
        }

        void SeedInvoiceType(
            InventoryDemoSeedResult result, Random rng, clsInvoiceHeader header,
            DemoContext ctx, int userId,
            int invoiceTypeId, bool isCounted, int businessPartnerId, string refNo,
            int minDocs = 5, int maxDocs = 10, string noteLabel = null)
        {
            string key = noteLabel ?? ((clsEnum.VoucherType)invoiceTypeId).ToString();
            int target = rng.Next(minDocs, maxDocs + 1);
            int created = 0;

            for (int d = 0; d < target; d++)
            {
                int storeId = ctx.StoreIds[rng.Next(ctx.StoreIds.Count)];
                int bpId = businessPartnerId;
                if (bpId <= 0 && NeedsBusinessPartner(invoiceTypeId))
                    bpId = IsPurchaseFamily(invoiceTypeId) ? ctx.VendorId : ctx.CustomerId;

                List<DBInvoiceDetails> lines = BuildLines(
                    rng, ctx, invoiceTypeId, isCounted, ctx.BranchId, storeId, ctx.CompanyId);
                if (lines.Count == 0) continue;

                decimal total = lines.Sum(l => l.TotalLine);
                DateTime docDate = DateTime.Today.AddDays(-rng.Next(1, 90));
                string note = $"{DemoTag}{key} #{d + 1}";
                int accountId = 0;
                if (invoiceTypeId == (int)clsEnum.VoucherType.GoodRecipt
                    || invoiceTypeId == (int)clsEnum.VoucherType.GoodIssue)
                    accountId = ctx.InventoryAccountId;

                ApiResponse<string> resp = null;
                try
                {
                    RunInCompanyTransaction(ctx.CompanyId, trn =>
                    {
                        resp = header.InsertInvoiceHeaderWithDetails(
                            ctx.BranchId, 0, storeId, bpId,
                            0, 0, refNo, 0, 0,
                            invoiceTypeId, isCounted, note, ctx.CompanyId,
                            0, "", "",
                            0, ctx.PaymentMethodId,
                            "", total,
                            docDate, userId, accountId, 0, Posted,
                            0, 0, 1,
                            JsonConvert.SerializeObject(lines), trn, DemoBypassApproval);
                    });
                }
                catch (Exception ex)
                {
                    result.Warnings.Add($"{key} #{d + 1}: {ex.Message}");
                    continue;
                }

                if (resp != null && resp.Success)
                    created++;
                else
                    result.Warnings.Add($"{key} #{d + 1}: {resp?.Message ?? "unknown error"}");
            }

            result.Counts[key] = created;
        }

        List<DBInvoiceDetails> BuildLines(
            Random rng, DemoContext ctx, int invoiceTypeId, bool isCounted,
            int branchId, int storeId, int companyId)
        {
            int lineCount = rng.Next(2, 4);
            var lines = new List<DBInvoiceDetails>();
            var used = new HashSet<Guid>();
            clsItems items = invoiceTypeId == (int)clsEnum.VoucherType.GoodIssue
                ? new clsItems() : null;

            for (int i = 0; i < lineCount; i++)
            {
                DemoItemRow item = ctx.Items[rng.Next(ctx.Items.Count)];
                if (!used.Add(item.Guid) && ctx.Items.Count >= lineCount)
                {
                    i--;
                    continue;
                }

                decimal qty = rng.Next(3, 11);
                if (items != null)
                {
                    decimal onHand = items.GetOnHandQty(item.Guid.ToString(), storeId, companyId, null);
                    if (onHand <= 0) { i--; continue; }
                    qty = Math.Min(qty, onHand);
                }
                bool inbound = IsInbound(invoiceTypeId);
                decimal unitPrice = inbound
                    ? (item.Cost > 0 ? item.Cost : rng.Next(5, 40))
                    : (item.SalesPrice > 0 ? item.SalesPrice : (item.Cost > 0 ? item.Cost * 1.25m : rng.Next(10, 80)));

                if (!inbound && !IsOffer(invoiceTypeId))
                    unitPrice = item.Cost > 0 ? item.Cost : unitPrice;

                decimal lineTotal = qty * unitPrice;
                lines.Add(new DBInvoiceDetails
                {
                    Guid = Guid.Empty,
                    HeaderGuid = Guid.Empty,
                    RowIndex = lines.Count + 1,
                    ItemGuid = item.Guid,
                    ItemName = item.ItemName ?? "",
                    Qty = qty,
                    TotalQTY = qty,
                    UOMQTY = qty,
                    UOMID = 0,
                    UOMFactor = 1,
                    PriceBeforeTax = unitPrice,
                    AVGCostPerUnit = item.Cost > 0 ? item.Cost : unitPrice,
                    TotalLine = lineTotal,
                    BranchID = branchId,
                    StoreID = storeId,
                    CompanyID = companyId,
                    InvoiceTypeID = invoiceTypeId,
                    IsCounted = isCounted,
                    InvoiceDate = DateTime.Today,
                    LotDetails = "",
                });
            }

            return lines;
        }

        void EnsureDemoStores(DemoContext ctx, int userId)
        {
            if (ctx.StoreIds.Count >= 2) return;
            clsStore store = new clsStore();
            int id = store.InsertStore(
                $"{DemoTag}Warehouse 2", $"{DemoTag}Warehouse 2",
                ctx.BranchId, ctx.CompanyId, userId);
            if (id > 0)
                ctx.StoreIds.Add(id);
        }

        void TopUpAllStores(
            InventoryDemoSeedResult result, Random rng, clsInvoiceHeader header,
            DemoContext ctx, int userId)
        {
            foreach (int storeId in ctx.StoreIds)
                TopUpStoreStock(result, rng, header, ctx, userId, storeId);
        }

        void TopUpStoreStock(
            InventoryDemoSeedResult result, Random rng, clsInvoiceHeader header,
            DemoContext ctx, int userId, int storeId)
        {
            clsItems items = new clsItems();
            const decimal minQty = 40m;
            var lines = new List<DBInvoiceDetails>();
            int added = 0;

            foreach (DemoItemRow item in ctx.Items.OrderBy(_ => rng.Next()).Take(12))
            {
                if (added >= 8) break;

                decimal onHand = items.GetOnHandQty(item.Guid.ToString(), storeId, ctx.CompanyId, null);
                if (onHand >= minQty) continue;

                decimal qty = minQty - onHand + rng.Next(5, 15);
                decimal unitPrice = item.Cost > 0 ? item.Cost : rng.Next(5, 40);
                lines.Add(new DBInvoiceDetails
                {
                    Guid = Guid.Empty,
                    HeaderGuid = Guid.Empty,
                    RowIndex = lines.Count + 1,
                    ItemGuid = item.Guid,
                    ItemName = item.ItemName ?? "",
                    Qty = qty,
                    TotalQTY = qty,
                    UOMQTY = qty,
                    UOMID = 0,
                    UOMFactor = 1,
                    PriceBeforeTax = unitPrice,
                    AVGCostPerUnit = unitPrice,
                    TotalLine = qty * unitPrice,
                    BranchID = ctx.BranchId,
                    StoreID = storeId,
                    CompanyID = ctx.CompanyId,
                    InvoiceTypeID = (int)clsEnum.VoucherType.GoodRecipt,
                    IsCounted = true,
                    InvoiceDate = DateTime.Today,
                });
                added++;
            }

            if (lines.Count == 0) return;

            decimal total = lines.Sum(l => l.TotalLine);
            ApiResponse<string> resp = null;
            try
            {
                RunInCompanyTransaction(ctx.CompanyId, trn =>
                {
                    resp = header.InsertInvoiceHeaderWithDetails(
                        ctx.BranchId, 0, storeId, 0,
                        0, 0, "", 0, 0,
                        (int)clsEnum.VoucherType.GoodRecipt, true,
                        $"{DemoTag}Stock top-up store {storeId}", ctx.CompanyId,
                        0, "", "",
                        0, 0,
                        "", total,
                        DateTime.Today.AddDays(-rng.Next(1, 30)), userId, ctx.InventoryAccountId, 0, Posted,
                        0, 0, 1,
                        JsonConvert.SerializeObject(lines), trn, DemoBypassApproval);
                });
            }
            catch (Exception ex)
            {
                result.Warnings.Add($"Stock top-up store {storeId}: {ex.Message}");
                return;
            }

            if (resp == null || !resp.Success)
                result.Warnings.Add($"Stock top-up store {storeId}: {resp?.Message ?? "unknown error"}");
        }

        void SeedSalesReturns(
            InventoryDemoSeedResult result, Random rng, clsInvoiceHeader header,
            DemoContext ctx, int userId)
        {
            if (ctx.CustomerId <= 0)
            {
                result.Warnings.Add("SalesRefund: no active customer found.");
                result.Counts["SalesRefund"] = 0;
                return;
            }

            SeedInvoiceType(result, rng, header, ctx, userId,
                (int)clsEnum.VoucherType.SalesRefund, true, ctx.CustomerId, "",
                MovementDocMin, MovementDocMax);
        }

        void SeedStockAdjustments(
            InventoryDemoSeedResult result, Random rng, clsInvoiceHeader header,
            DemoContext ctx, int userId)
        {
            SeedInvoiceType(result, rng, header, ctx, userId,
                (int)clsEnum.VoucherType.GoodRecipt, true, 0, "",
                MovementDocMin, MovementDocMax, "StockAdjustment-Receipt");
            SeedInvoiceType(result, rng, header, ctx, userId,
                (int)clsEnum.VoucherType.GoodIssue, true, 0, "",
                MovementDocMin, MovementDocMax, "StockAdjustment-Issue");
        }

        void SeedTransfers(InventoryDemoSeedResult result, Random rng, DemoContext ctx, int userId)
        {
            if (ctx.StoreIds.Count < 2)
            {
                result.Warnings.Add("WarehouseTransfer: need at least 2 stores.");
                result.Counts["WarehouseTransfer"] = 0;
                return;
            }

            clsWarehouseTransfer transfer = new clsWarehouseTransfer();
            clsItems items = new clsItems();
            int target = rng.Next(MovementDocMin, MovementDocMax + 1);
            int created = 0;

            for (int t = 0; t < target; t++)
            {
                int src = ctx.StoreIds[rng.Next(ctx.StoreIds.Count)];
                int dest = ctx.StoreIds[rng.Next(ctx.StoreIds.Count)];
                if (src == dest) { t--; continue; }

                var lines = new List<WarehouseTransferLine>();
                int lineCount = rng.Next(3, 6);
                for (int i = 0; i < lineCount; i++)
                {
                    DemoItemRow item = ctx.Items[rng.Next(ctx.Items.Count)];
                    decimal onHand = items.GetOnHandQty(item.Guid.ToString(), src, ctx.CompanyId, null);
                    if (onHand <= 0) continue;

                    decimal qty = Math.Min(rng.Next(1, 8), onHand);
                    if (qty <= 0) continue;

                    lines.Add(new WarehouseTransferLine
                    {
                        ItemGuid = item.Guid.ToString(),
                        Qty = qty,
                    });
                }

                if (lines.Count == 0)
                {
                    result.Warnings.Add($"WarehouseTransfer #{t + 1}: no stock in source store.");
                    continue;
                }

                WarehouseTransferResult tr = null;
                try
                {
                    RunInCompanyTransaction(ctx.CompanyId, trn =>
                    {
                        tr = transfer.PostTransfer(
                            ctx.BranchId, src, dest, lines,
                            $"{DemoTag}WarehouseTransfer #{t + 1}",
                            ctx.CompanyId, userId,
                            DateTime.Today.AddDays(-rng.Next(1, 60)), trn, DemoBypassApproval);
                    });
                }
                catch (Exception ex)
                {
                    result.Warnings.Add($"WarehouseTransfer #{t + 1}: {ex.Message}");
                    continue;
                }

                if (tr != null && tr.Success) created++;
                else result.Warnings.Add($"WarehouseTransfer #{t + 1}: {tr?.Message ?? "unknown error"}");
            }

            result.Counts["WarehouseTransfer"] = created;
        }

        void SeedStockCounts(InventoryDemoSeedResult result, Random rng, DemoContext ctx, int userId)
        {
            clsStockCount stockCount = new clsStockCount();
            clsItems items = new clsItems();
            int target = rng.Next(MovementDocMin, MovementDocMax + 1);
            int created = 0;

            for (int c = 0; c < target; c++)
            {
                int storeId = ctx.StoreIds[rng.Next(ctx.StoreIds.Count)];
                var lines = new List<StockCountLine>();
                int lineCount = rng.Next(3, 6);

                for (int i = 0; i < lineCount; i++)
                {
                    DemoItemRow item = ctx.Items[rng.Next(ctx.Items.Count)];
                    decimal onHand = items.GetOnHandQty(item.Guid.ToString(), storeId, ctx.CompanyId, null);
                    decimal counted;
                    if (onHand <= 0)
                        counted = rng.Next(5, 15);
                    else if (rng.Next(2) == 0)
                        counted = onHand + rng.Next(1, 8);
                    else
                        counted = Math.Max(0, onHand - rng.Next(1, Math.Min(6, (int)onHand + 1)));

                    if (counted == onHand)
                        counted = onHand + rng.Next(1, 5);

                    lines.Add(new StockCountLine
                    {
                        ItemGuid = item.Guid.ToString(),
                        CountedQty = counted,
                    });
                }

                StockCountResult sc = null;
                try
                {
                    RunInCompanyTransaction(ctx.CompanyId, trn =>
                    {
                        sc = stockCount.PostStockCount(
                            ctx.BranchId, storeId, lines, 0,
                            $"{DemoTag}StockCount #{c + 1}",
                            ctx.CompanyId, userId,
                            DateTime.Today.AddDays(-rng.Next(1, 45)), trn, DemoBypassApproval);
                    });
                }
                catch (Exception ex)
                {
                    result.Warnings.Add($"StockCount #{c + 1}: {ex.Message}");
                    continue;
                }

                if (sc != null && sc.Success &&
                    (!string.IsNullOrEmpty(sc.IncreaseGuid) || !string.IsNullOrEmpty(sc.DecreaseGuid)))
                    created++;
                else if (sc != null && !sc.Success)
                    result.Warnings.Add($"StockCount #{c + 1}: {sc.Message}");
                else
                    result.Warnings.Add($"StockCount #{c + 1}: count matched system; no adjustment posted.");
            }

            result.Counts["StockCount"] = created;
        }

        static bool NeedsBusinessPartner(int typeId) =>
            typeId == (int)clsEnum.VoucherType.SalesInvoice
            || typeId == (int)clsEnum.VoucherType.SalesRefund
            || typeId == (int)clsEnum.VoucherType.SalesOffer
            || typeId == (int)clsEnum.VoucherType.POSSalesInvoice
            || typeId == (int)clsEnum.VoucherType.POSSalesInvoicereturn
            || typeId == (int)clsEnum.VoucherType.PurchaseInvoice
            || typeId == (int)clsEnum.VoucherType.PurchaseRefund
            || typeId == (int)clsEnum.VoucherType.PurchaseOffer;

        static bool IsPurchaseFamily(int typeId) =>
            typeId == (int)clsEnum.VoucherType.PurchaseInvoice
            || typeId == (int)clsEnum.VoucherType.PurchaseRefund
            || typeId == (int)clsEnum.VoucherType.PurchaseOffer;

        static bool IsInbound(int typeId) =>
            typeId == (int)clsEnum.VoucherType.PurchaseInvoice
            || typeId == (int)clsEnum.VoucherType.GoodRecipt
            || typeId == (int)clsEnum.VoucherType.SalesRefund;

        static bool IsOffer(int typeId) =>
            typeId == (int)clsEnum.VoucherType.SalesOffer
            || typeId == (int)clsEnum.VoucherType.PurchaseOffer;

        class DemoContext
        {
            public int CompanyId { get; set; }
            public int BranchId { get; set; }
            public List<int> StoreIds { get; } = new List<int>();
            public List<DemoItemRow> Items { get; } = new List<DemoItemRow>();
            public int CustomerId { get; set; }
            public int VendorId { get; set; }
            public int PaymentMethodId { get; set; }
            public int InventoryAccountId { get; set; }
        }
    }
}
