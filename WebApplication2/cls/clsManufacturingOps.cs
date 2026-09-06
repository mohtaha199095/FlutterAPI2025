using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using WebApplication2.MainClasses;

namespace WebApplication2.cls
{
    /// <summary>
    /// BOM explosion, MRP, material ATP, FG cost allocation, and MO close/variance.
    /// </summary>
    public class clsManufacturingOps
    {
        const int InputTypeId = 25;
        const int OutputTypeId = 26;
        const int MaxExplodeDepth = 8;

        public DataTable ExplodeBOM(int bomId, int companyId)
        {
            DataTable result = NewExplodeTable();
            if (bomId <= 0) return result;

            var merged = new Dictionary<Guid, ExplodeLine>(16);
            ExplodeRecursive(bomId, 1m, companyId, 0, merged, new HashSet<int>());

            int line = 1;
            foreach (var kv in merged)
            {
                DataRow row = result.NewRow();
                row["ItemGuid"] = kv.Key;
                row["ItemName"] = kv.Value.ItemName ?? "";
                row["QtyBeforeScrap"] = kv.Value.QtyBeforeScrap;
                row["ScrapPercent"] = kv.Value.ScrapPercent;
                row["UOMID"] = kv.Value.UomId;
                row["IsPhantom"] = false;
                row["LineNo"] = line++;
                row["Notes"] = kv.Value.Notes ?? "";
                result.Rows.Add(row);
            }
            return result;
        }

        void ExplodeRecursive(
            int bomId,
            decimal parentBatches,
            int companyId,
            int depth,
            Dictionary<Guid, ExplodeLine> merged,
            HashSet<int> visiting)
        {
            if (bomId <= 0 || parentBatches == 0 || depth > MaxExplodeDepth) return;
            if (!visiting.Add(bomId)) return;

            clsBOM bom = new clsBOM();
            DataTable header = bom.SelectBOMHeader(bomId, "", "", companyId);
            if (header == null || header.Rows.Count == 0)
            {
                visiting.Remove(bomId);
                return;
            }

            decimal batchQty = Simulate.decimal_(header.Rows[0]["BatchQty"]);
            if (batchQty <= 0) batchQty = 1;

            DataTable inputs = bom.SelectBOMInputsByBOMID(bomId, companyId);
            if (inputs != null)
            {
                foreach (DataRow input in inputs.Rows)
                {
                    Guid itemGuid = Simulate.Guid(Simulate.String(input["ComponentItemGuid"]));
                    if (itemGuid == Guid.Empty) continue;

                    decimal lineQty = Simulate.decimal_(input["Qty"]);
                    decimal scrap = Simulate.decimal_(input["ScrapPercent"]);
                    int uomId = Simulate.Integer32(input["UOMID"]);
                    bool isPhantom = false;
                    if (input.Table.Columns.Contains("IsPhantom"))
                        isPhantom = Simulate.Bool(input["IsPhantom"]);
                    string notes = Simulate.String(input["Notes"]);

                    decimal qtyForBatches = lineQty / batchQty * parentBatches;
                    decimal qtyWithScrap = qtyForBatches * (1m + scrap / 100m);

                    if (isPhantom)
                    {
                        ChildBom child = FindBomForOutputItem(itemGuid, companyId);
                        if (child.BomId > 0 && child.OutputQty > 0)
                        {
                            decimal childBatches = qtyWithScrap / child.OutputQty * child.BatchQty;
                            ExplodeRecursive(child.BomId, childBatches, companyId, depth + 1, merged, visiting);
                            continue;
                        }
                    }

                    if (!merged.TryGetValue(itemGuid, out ExplodeLine existing))
                    {
                        existing = new ExplodeLine
                        {
                            ItemName = LookupItemName(itemGuid, companyId),
                            UomId = uomId,
                            Notes = notes
                        };
                        merged[itemGuid] = existing;
                    }
                    existing.QtyBeforeScrap += qtyForBatches;
                    if (scrap > existing.ScrapPercent) existing.ScrapPercent = scrap;
                    if (existing.UomId == 0) existing.UomId = uomId;
                }
            }

            visiting.Remove(bomId);
        }

        ChildBom FindBomForOutputItem(Guid itemGuid, int companyId)
        {
            clsSQL clsSQL = new clsSQL();
            SqlParameter[] prm =
            {
                new SqlParameter("@ItemGuid", SqlDbType.UniqueIdentifier) { Value = itemGuid },
                new SqlParameter("@CompanyID", SqlDbType.Int) { Value = companyId },
            };
            DataTable dt = clsSQL.ExecuteQueryStatement(@"
                SELECT TOP 1 bh.ID AS BOMID, bh.BatchQty, bo.Qty AS OutputQty
                FROM tbl_BOMHeader bh
                INNER JOIN tbl_BOMOutput bo ON bo.BOMID = bh.ID AND bo.CompanyID = bh.CompanyID
                WHERE bh.CompanyID = @CompanyID AND bh.IsActive = 1 AND bo.OutputItemGuid = @ItemGuid
                ORDER BY bh.IsDefault DESC, bh.VersionNo DESC, bh.ID DESC
            ", clsSQL.CreateDataBaseConnectionString(companyId), prm);

            if (dt == null || dt.Rows.Count == 0) return default;
            return new ChildBom
            {
                BomId = Simulate.Integer32(dt.Rows[0]["BOMID"]),
                BatchQty = Simulate.decimal_(dt.Rows[0]["BatchQty"]) <= 0 ? 1 : Simulate.decimal_(dt.Rows[0]["BatchQty"]),
                OutputQty = Simulate.decimal_(dt.Rows[0]["OutputQty"]) <= 0 ? 1 : Simulate.decimal_(dt.Rows[0]["OutputQty"])
            };
        }

        string LookupItemName(Guid itemGuid, int companyId)
        {
            clsSQL clsSQL = new clsSQL();
            SqlParameter[] prm =
            {
                new SqlParameter("@Guid", SqlDbType.UniqueIdentifier) { Value = itemGuid },
            };
            object o = clsSQL.ExecuteScalar(
                "SELECT TOP 1 AName FROM tbl_Items WHERE Guid = @Guid",
                prm,
                clsSQL.CreateDataBaseConnectionString(companyId));
            return Simulate.String(o);
        }

        static DataTable NewExplodeTable()
        {
            DataTable dt = new DataTable();
            dt.Columns.Add("ItemGuid", typeof(Guid));
            dt.Columns.Add("ItemName", typeof(string));
            dt.Columns.Add("QtyBeforeScrap", typeof(decimal));
            dt.Columns.Add("ScrapPercent", typeof(decimal));
            dt.Columns.Add("UOMID", typeof(int));
            dt.Columns.Add("IsPhantom", typeof(bool));
            dt.Columns.Add("LineNo", typeof(int));
            dt.Columns.Add("Notes", typeof(string));
            return dt;
        }

        public DataTable SelectMaterialAvailability(string moGuid, int companyId, int storeId)
        {
            clsSQL clsSQL = new clsSQL();
            SqlParameter[] prm =
            {
                new SqlParameter("@MOGuid", SqlDbType.UniqueIdentifier) { Value = Simulate.Guid(moGuid) },
                new SqlParameter("@CompanyID", SqlDbType.Int) { Value = companyId },
                new SqlParameter("@StoreID", SqlDbType.Int) { Value = storeId },
            };
            return clsSQL.ExecuteQueryStatement(@"
;WITH Stock AS (
    SELECT d.ItemGuid,
           SUM(CASE WHEN h.InvoiceTypeID IN (8, 26) THEN ISNULL(d.Qty, 0)
                    WHEN h.InvoiceTypeID IN (3, 9, 10, 11, 25) THEN -ISNULL(d.Qty, 0)
                    ELSE 0 END) AS OnHand
    FROM tbl_InvoiceDetails d
    INNER JOIN tbl_InvoiceHeader h ON h.Guid = d.HeaderGuid AND h.CompanyID = d.CompanyID
    WHERE d.CompanyID = @CompanyID AND h.IsPosted = 1
      AND (@StoreID = 0 OR d.StoreID = @StoreID OR h.StoreID = @StoreID)
    GROUP BY d.ItemGuid
)
SELECT
    md.ItemGuid,
    MAX(md.ItemName) AS ItemName,
    CAST(SUM(ISNULL(md.PlannedQty, 0)) AS DECIMAL(18,3)) AS PlannedQty,
    CAST(ISNULL(MAX(s.OnHand), 0) AS DECIMAL(18,3)) AS OnHandQty,
    CAST(ISNULL(MAX(s.OnHand), 0) - SUM(ISNULL(md.PlannedQty, 0)) AS DECIMAL(18,3)) AS ShortageQty
FROM tbl_MODetails md
LEFT JOIN Stock s ON s.ItemGuid = md.ItemGuid
WHERE md.HeaderGuid = @MOGuid AND md.CompanyID = @CompanyID AND md.LineTypeID = 25
GROUP BY md.ItemGuid
HAVING ISNULL(MAX(s.OnHand), 0) < SUM(ISNULL(md.PlannedQty, 0))
ORDER BY MAX(md.ItemName)
            ", clsSQL.CreateDataBaseConnectionString(companyId), prm);
        }

        public DataTable SelectReceiptCostAllocation(string moGuid, int companyId)
        {
            DataTable dt = new DataTable();
            dt.Columns.Add("ItemGuid", typeof(Guid));
            dt.Columns.Add("ItemName", typeof(string));
            dt.Columns.Add("PlannedQty", typeof(decimal));
            dt.Columns.Add("CostSharePercent", typeof(decimal));
            dt.Columns.Add("AllocatedTotal", typeof(decimal));
            dt.Columns.Add("AllocatedUnitCost", typeof(decimal));
            dt.Columns.Add("PoolCost", typeof(decimal));

            decimal pool = GetVoucherCost(moGuid, companyId, InputTypeId) + GetLaborCost(moGuid, companyId);
            clsMO mo = new clsMO();
            DataTable details = mo.SelectMODetailsByMOGuid(moGuid, companyId, OutputTypeId);
            if (details == null || details.Rows.Count == 0) return dt;

            decimal shareSum = 0;
            foreach (DataRow r in details.Rows)
                shareSum += Simulate.decimal_(r["CostSharePercent"]);

            foreach (DataRow r in details.Rows)
            {
                decimal planned = Simulate.decimal_(r["PlannedQty"]);
                decimal share = Simulate.decimal_(r["CostSharePercent"]);
                decimal factor = shareSum > 0 ? share / shareSum : (details.Rows.Count == 0 ? 0 : 1m / details.Rows.Count);
                decimal total = pool * factor;
                DataRow row = dt.NewRow();
                row["ItemGuid"] = r["ItemGuid"];
                row["ItemName"] = r["ItemName"];
                row["PlannedQty"] = planned;
                row["CostSharePercent"] = share;
                row["AllocatedTotal"] = total;
                row["AllocatedUnitCost"] = planned == 0 ? 0 : total / planned;
                row["PoolCost"] = pool;
                dt.Rows.Add(row);
            }
            return dt;
        }

        /// <summary>
        /// FG receipt is only allowed after planned input qty has been fully issued
        /// (prevents zero/undercosted finished goods and backflush-after-receipt variance).
        /// </summary>
        public void AssertInputsFullyIssued(string moGuid, int companyId, SqlTransaction trn = null)
        {
            DataTable progress = new clsMO().SelectMOProgress(moGuid, companyId);
            if (progress == null || progress.Rows.Count == 0)
                throw new Exception("Cannot receive finished goods: MO has no progress lines.");

            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            foreach (DataRow p in progress.Rows)
            {
                if (Simulate.String(p["Type"]) != "INPUT") continue;
                decimal planned = Simulate.decimal_(p["PlannedQty"]);
                decimal actual = Simulate.decimal_(p["ActualQty"]);
                decimal rem = planned - actual;
                if (rem > 0.0001m)
                {
                    sb.Append(Simulate.String(p["ItemName"]))
                      .Append(" remaining ")
                      .Append(rem.ToString("0.###"))
                      .Append("; ");
                }
            }
            if (sb.Length > 0)
                throw new Exception(
                    "Cannot receive finished goods until all materials are issued. Remaining: " + sb);
        }

        public void AssertMoReceiptAllowedIfLinked(string relatedMoGuid, int invoiceTypeId, int companyId, SqlTransaction trn = null)
        {
            if (invoiceTypeId != OutputTypeId) return;
            if (string.IsNullOrWhiteSpace(relatedMoGuid)) return;
            Guid g = Simulate.Guid(relatedMoGuid);
            if (g == Guid.Empty) return;

            clsSQL clsSQL = new clsSQL();
            SqlParameter[] prm =
            {
                new SqlParameter("@Guid", SqlDbType.UniqueIdentifier) { Value = g },
                new SqlParameter("@CompanyID", SqlDbType.Int) { Value = companyId },
            };
            DataTable header = clsSQL.ExecuteQueryStatement(
                "SELECT TOP 1 Guid FROM tbl_MOHeader WHERE Guid = @Guid AND CompanyID = @CompanyID",
                clsSQL.CreateDataBaseConnectionString(companyId), prm, trn);
            if (header == null || header.Rows.Count == 0) return;

            AssertInputsFullyIssued(relatedMoGuid, companyId, trn);
        }

        public void AssertMoCanChangeStatus(string moGuid, int companyId, int newStatusId, SqlTransaction trn)
        {
            if (string.IsNullOrWhiteSpace(moGuid)) return;

            clsSQL clsSQL = new clsSQL();
            SqlParameter[] prm =
            {
                new SqlParameter("@Guid", SqlDbType.UniqueIdentifier) { Value = Simulate.Guid(moGuid) },
                new SqlParameter("@CompanyID", SqlDbType.Int) { Value = companyId },
            };
            DataTable header = clsSQL.ExecuteQueryStatement(
                "SELECT StatusID, VarianceJVGuid FROM tbl_MOHeader WHERE Guid = @Guid AND CompanyID = @CompanyID",
                clsSQL.CreateDataBaseConnectionString(companyId), prm, trn);
            if (header == null || header.Rows.Count == 0) return;

            int currentStatus = Simulate.Integer32(header.Rows[0]["StatusID"]);
            string varianceJv = header.Columns.Contains("VarianceJVGuid")
                ? Simulate.String(header.Rows[0]["VarianceJVGuid"])
                : "";
            bool settled = !string.IsNullOrWhiteSpace(varianceJv) && varianceJv != Guid.Empty.ToString();

            if ((currentStatus == 3 || settled) && newStatusId != 3)
                throw new Exception(
                    "Completed manufacturing orders cannot be reopened. Reverse the variance and vouchers first.");

            if (newStatusId == 4)
            {
                decimal inputCost = GetVoucherCost(moGuid, companyId, InputTypeId, trn);
                decimal outputCost = GetVoucherCost(moGuid, companyId, OutputTypeId, trn);
                if (inputCost > 0.0001m || outputCost > 0.0001m || settled)
                    throw new Exception(
                        "Cannot cancel MO with posted issue/receipt vouchers. Delete or reverse vouchers first.");
            }
        }

        public void AssertMoVoucherDeletable(string moGuid, int companyId, SqlTransaction trn)
        {
            clsSQL clsSQL = new clsSQL();
            SqlParameter[] prm =
            {
                new SqlParameter("@Guid", SqlDbType.UniqueIdentifier) { Value = Simulate.Guid(moGuid) },
                new SqlParameter("@CompanyID", SqlDbType.Int) { Value = companyId },
            };
            DataTable header = clsSQL.ExecuteQueryStatement(
                "SELECT StatusID, VarianceJVGuid FROM tbl_MOHeader WHERE Guid = @Guid AND CompanyID = @CompanyID",
                clsSQL.CreateDataBaseConnectionString(companyId), prm, trn);
            if (header == null || header.Rows.Count == 0) return;

            int status = Simulate.Integer32(header.Rows[0]["StatusID"]);
            string varianceJv = header.Columns.Contains("VarianceJVGuid")
                ? Simulate.String(header.Rows[0]["VarianceJVGuid"])
                : "";
            bool settled = !string.IsNullOrWhiteSpace(varianceJv) && varianceJv != Guid.Empty.ToString();
            if (status == 3 || settled)
                throw new Exception(
                    "Cannot delete manufacturing vouchers after the MO is completed / variance settled.");
        }

        public DataTable SelectMRPSuggestions(int companyId)
        {
            DataTable result = new DataTable();
            result.Columns.Add("SuggestionType", typeof(string));
            result.Columns.Add("BOMID", typeof(int));
            result.Columns.Add("BOMCode", typeof(string));
            result.Columns.Add("ItemGuid", typeof(Guid));
            result.Columns.Add("ItemName", typeof(string));
            result.Columns.Add("CurrentQty", typeof(decimal));
            result.Columns.Add("SalesDemand", typeof(decimal));
            result.Columns.Add("OpenMOQty", typeof(decimal));
            result.Columns.Add("RequiredQty", typeof(decimal));
            result.Columns.Add("SuggestedMOQty", typeof(decimal));
            result.Columns.Add("DependentDemand", typeof(decimal));

            Dictionary<Guid, StockRow> stock = LoadStock(companyId);
            Dictionary<Guid, decimal> openMo = LoadOpenMoRemaining(companyId);
            Dictionary<Guid, decimal> salesDemand = LoadSalesOfferDemand(companyId);

            clsBOM bomCls = new clsBOM();
            DataTable bomHeaders = bomCls.SelectBOMHeader(0, "", "", companyId);
            var independentMo = new List<(int BomId, string BomCode, Guid ItemGuid, string ItemName, decimal OutputPerBatch, decimal BatchQty, decimal Shortage)>();

            if (bomHeaders != null)
            {
                foreach (DataRow bh in bomHeaders.Rows)
                {
                    if (!Simulate.Bool(bh["IsActive"])) continue;
                    int bomId = Simulate.Integer32(bh["ID"]);
                    string bomCode = Simulate.String(bh["BOMCode"]);
                    decimal batchQty = Simulate.decimal_(bh["BatchQty"]);
                    if (batchQty <= 0) batchQty = 1;

                    DataTable outputs = bomCls.SelectBOMOutputsByBOMID(bomId, companyId);
                    if (outputs == null) continue;
                    foreach (DataRow bo in outputs.Rows)
                    {
                        Guid itemGuid = Simulate.Guid(Simulate.String(bo["OutputItemGuid"]));
                        if (itemGuid == Guid.Empty) continue;
                        decimal outQty = Simulate.decimal_(bo["Qty"]);
                        if (outQty <= 0) outQty = 1;
                        decimal outputPerBatch = outQty / batchQty;

                        ItemLimits limits = LoadItemLimits(itemGuid, companyId);
                        decimal onHand = stock.TryGetValue(itemGuid, out StockRow sr) ? sr.Qty : 0;
                        decimal sales = salesDemand.TryGetValue(itemGuid, out decimal sd) ? sd : 0;
                        decimal open = openMo.TryGetValue(itemGuid, out decimal om) ? om : 0;
                        decimal target = limits.MinimumLimit;
                        decimal shortage = target + sales - onHand - open;
                        if (shortage <= 0) continue;

                        independentMo.Add((bomId, bomCode, itemGuid, limits.Name, outputPerBatch, batchQty, shortage));
                    }
                }
            }

            var dependent = new Dictionary<Guid, decimal>();
            foreach (var mo in independentMo)
            {
                decimal batches = mo.Shortage / (mo.OutputPerBatch <= 0 ? 1 : mo.OutputPerBatch);
                DataTable exploded = ExplodeBOM(mo.BomId, companyId);
                foreach (DataRow line in exploded.Rows)
                {
                    Guid cg = (Guid)line["ItemGuid"];
                    decimal qty = Simulate.decimal_(line["QtyBeforeScrap"]) *
                                  (1m + Simulate.decimal_(line["ScrapPercent"]) / 100m) *
                                  batches;
                    if (!dependent.ContainsKey(cg)) dependent[cg] = 0;
                    dependent[cg] += qty;
                }
            }

            var emitted = new HashSet<string>();
            foreach (var mo in independentMo)
            {
                decimal suggested = Math.Ceiling(mo.Shortage / (mo.OutputPerBatch <= 0 ? 1 : mo.OutputPerBatch));
                AddMrpRow(result, "MO", mo.BomId, mo.BomCode, mo.ItemGuid, mo.ItemName,
                    stock, openMo, salesDemand, mo.Shortage, suggested, 0);
                emitted.Add("MO:" + mo.ItemGuid);
            }

            foreach (var kv in dependent)
            {
                Guid itemGuid = kv.Key;
                decimal dep = kv.Value;
                ItemLimits limits = LoadItemLimits(itemGuid, companyId);
                decimal onHand = stock.TryGetValue(itemGuid, out StockRow sr) ? sr.Qty : 0;
                decimal sales = salesDemand.TryGetValue(itemGuid, out decimal sd) ? sd : 0;
                decimal open = openMo.TryGetValue(itemGuid, out decimal om) ? om : 0;
                decimal shortage = limits.MinimumLimit + sales + dep - onHand - open;
                if (shortage <= 0) continue;

                ChildBom child = FindBomForOutputItem(itemGuid, companyId);
                if (child.BomId > 0)
                {
                    string key = "MO:" + itemGuid;
                    if (emitted.Contains(key)) continue;
                    decimal suggested = Math.Ceiling(shortage / (child.OutputQty <= 0 ? 1 : child.OutputQty) * child.BatchQty);
                    clsBOM b = new clsBOM();
                    DataTable hd = b.SelectBOMHeader(child.BomId, "", "", companyId);
                    string code = (hd != null && hd.Rows.Count > 0) ? Simulate.String(hd.Rows[0]["BOMCode"]) : "";
                    AddMrpRow(result, "MO", child.BomId, code, itemGuid, limits.Name,
                        stock, openMo, salesDemand, shortage, suggested, dep);
                    emitted.Add(key);
                }
                else
                {
                    AddMrpRow(result, "Purchase", 0, "", itemGuid, limits.Name,
                        stock, openMo, salesDemand, shortage, shortage, dep);
                }
            }

            return result;
        }

        void AddMrpRow(
            DataTable result,
            string type,
            int bomId,
            string bomCode,
            Guid itemGuid,
            string itemName,
            Dictionary<Guid, StockRow> stock,
            Dictionary<Guid, decimal> openMo,
            Dictionary<Guid, decimal> salesDemand,
            decimal required,
            decimal suggested,
            decimal dependent)
        {
            DataRow row = result.NewRow();
            row["SuggestionType"] = type;
            row["BOMID"] = bomId;
            row["BOMCode"] = bomCode ?? "";
            row["ItemGuid"] = itemGuid;
            row["ItemName"] = itemName ?? "";
            row["CurrentQty"] = stock.TryGetValue(itemGuid, out StockRow sr) ? sr.Qty : 0;
            row["SalesDemand"] = salesDemand.TryGetValue(itemGuid, out decimal sd) ? sd : 0;
            row["OpenMOQty"] = openMo.TryGetValue(itemGuid, out decimal om) ? om : 0;
            row["RequiredQty"] = required;
            row["SuggestedMOQty"] = suggested;
            row["DependentDemand"] = dependent;
            result.Rows.Add(row);
        }

        public string SettleMoVariance(string moGuid, int companyId, int userId, SqlTransaction trn)
        {
            if (string.IsNullOrWhiteSpace(moGuid)) throw new Exception("MO is required.");

            clsSQL clsSQL = new clsSQL();
            SqlParameter[] headerPrm =
            {
                new SqlParameter("@Guid", SqlDbType.UniqueIdentifier) { Value = Simulate.Guid(moGuid) },
                new SqlParameter("@CompanyID", SqlDbType.Int) { Value = companyId },
            };
            DataTable header = clsSQL.ExecuteQueryStatement(
                "SELECT * FROM tbl_MOHeader WHERE Guid = @Guid AND CompanyID = @CompanyID",
                clsSQL.CreateDataBaseConnectionString(companyId), headerPrm, trn);
            if (header == null || header.Rows.Count == 0) throw new Exception("MO not found.");

            if (header.Columns.Contains("VarianceJVGuid"))
            {
                string existing = Simulate.String(header.Rows[0]["VarianceJVGuid"]);
                if (!string.IsNullOrWhiteSpace(existing) && existing != Guid.Empty.ToString())
                    return existing;
            }

            DataTable progress = new clsMO().SelectMOProgress(moGuid, companyId);
            decimal plannedOut = 0;
            decimal actualOut = 0;
            if (progress != null)
            {
                foreach (DataRow p in progress.Rows)
                {
                    if (Simulate.String(p["Type"]) != "OUTPUT") continue;
                    plannedOut += Simulate.decimal_(p["PlannedQty"]);
                    actualOut += Simulate.decimal_(p["ActualQty"]);
                }
            }
            if (actualOut <= 0)
                throw new Exception("Cannot complete MO: no finished goods have been received.");
            if (plannedOut - actualOut > 0.0001m)
                throw new Exception("Cannot complete MO: remaining output quantity must be received first.");

            decimal inputCost = GetVoucherCost(moGuid, companyId, InputTypeId, trn);
            decimal outputCost = GetVoucherCost(moGuid, companyId, OutputTypeId, trn);
            decimal laborCost = GetLaborCost(moGuid, companyId, trn);

            cls_AccountSetting settings = new cls_AccountSetting();
            DataTable dtAcc = settings.SelectAccountSetting(0, 0, companyId, trn);
            int inputAcc = GetAccountId(dtAcc, 20);
            int outputAcc = GetAccountId(dtAcc, 21);
            int varianceAcc = GetAccountId(dtAcc, 22);
            int employeeAcc = GetAccountId(dtAcc, 18);
            if (varianceAcc <= 0) throw new Exception("Manufacturing variance account is not configured.");
            if (inputAcc <= 0) throw new Exception("Manufacturing input account is not configured.");
            if (outputAcc <= 0) throw new Exception("Manufacturing output account is not configured.");

            int branchId = Simulate.Integer32(header.Rows[0]["BranchID"]);
            clsJournalVoucherHeader jvh = new clsJournalVoucherHeader();
            DataTable dtMax = jvh.SelectMaxJVNo("", (int)clsEnum.VoucherType.manufacturingOrder, companyId, trn);
            int maxNo = 1;
            if (dtMax != null && dtMax.Rows.Count > 0)
                maxNo = Simulate.Integer32(dtMax.Rows[0][0]) + 1;

            string jvGuid = jvh.InsertJournalVoucherHeader(
                branchId, 0,
                "MO variance close " + Simulate.String(header.Rows[0]["MOCode"]),
                Simulate.String(maxNo),
                (int)clsEnum.VoucherType.manufacturingOrder,
                companyId, DateTime.Now, userId, "", 0, trn, 2);

            clsJournalVoucherDetails jvd = new clsJournalVoucherDetails();
            DateTime now = DateTime.Now;

            if (laborCost > 0.0001m && employeeAcc > 0)
            {
                jvd.InsertJournalVoucherDetails(jvGuid, 0, inputAcc, 0, laborCost, 0, laborCost, 1, 1, laborCost, branchId, 0, now, "Labor absorption", companyId, userId, "", trn);
                jvd.InsertJournalVoucherDetails(jvGuid, 0, employeeAcc, 0, 0, laborCost, -laborCost, 1, 1, -laborCost, branchId, 0, now, "Labor absorption", companyId, userId, "", trn);
                inputCost += laborCost;
            }

            decimal variance = inputCost - outputCost;
            if (outputCost > 0)
            {
                jvd.InsertJournalVoucherDetails(jvGuid, 0, outputAcc, 0, outputCost, 0, outputCost, 1, 1, outputCost, branchId, 0, now, "Close MO output", companyId, userId, "", trn);
            }
            if (inputCost > 0)
            {
                jvd.InsertJournalVoucherDetails(jvGuid, 0, inputAcc, 0, 0, inputCost, -inputCost, 1, 1, -inputCost, branchId, 0, now, "Close MO input", companyId, userId, "", trn);
            }
            if (variance > 0.0001m)
            {
                jvd.InsertJournalVoucherDetails(jvGuid, 0, varianceAcc, 0, variance, 0, variance, 1, 1, variance, branchId, 0, now, "Production variance", companyId, userId, "", trn);
            }
            else if (variance < -0.0001m)
            {
                decimal v = -variance;
                jvd.InsertJournalVoucherDetails(jvGuid, 0, varianceAcc, 0, 0, v, -v, 1, 1, -v, branchId, 0, now, "Production variance", companyId, userId, "", trn);
            }

            if (!jvh.CheckJVMatch(jvGuid, companyId, trn))
                throw new Exception("MO variance journal is not balanced.");

            if (header.Columns.Contains("VarianceJVGuid"))
            {
                SqlParameter[] upd =
                {
                    new SqlParameter("@Guid", SqlDbType.UniqueIdentifier) { Value = Simulate.Guid(moGuid) },
                    new SqlParameter("@JV", SqlDbType.UniqueIdentifier) { Value = Simulate.Guid(jvGuid) },
                };
                clsSQL.ExecuteNonQueryStatement(
                    "UPDATE tbl_MOHeader SET VarianceJVGuid = @JV WHERE Guid = @Guid",
                    clsSQL.CreateDataBaseConnectionString(companyId), upd, trn);
            }

            return jvGuid;
        }

        public void PromoteMoInProgress(string moGuid, int companyId, SqlTransaction trn = null)
        {
            clsSQL clsSQL = new clsSQL();
            SqlParameter[] prm =
            {
                new SqlParameter("@Guid", SqlDbType.UniqueIdentifier) { Value = Simulate.Guid(moGuid) },
                new SqlParameter("@CompanyID", SqlDbType.Int) { Value = companyId },
            };
            string sql = @"UPDATE tbl_MOHeader SET StatusID = 2
                           WHERE Guid = @Guid AND CompanyID = @CompanyID AND StatusID = 1";
            if (trn == null)
                clsSQL.ExecuteNonQueryStatement(sql, clsSQL.CreateDataBaseConnectionString(companyId), prm);
            else
                clsSQL.ExecuteNonQueryStatement(sql, clsSQL.CreateDataBaseConnectionString(companyId), prm, trn);
        }

        decimal GetVoucherCost(string moGuid, int companyId, int linkTypeId, SqlTransaction trn = null)
        {
            clsSQL clsSQL = new clsSQL();
            SqlParameter[] prm =
            {
                new SqlParameter("@MOGuid", SqlDbType.UniqueIdentifier) { Value = Simulate.Guid(moGuid) },
                new SqlParameter("@CompanyID", SqlDbType.Int) { Value = companyId },
                new SqlParameter("@LinkTypeID", SqlDbType.Int) { Value = linkTypeId },
            };
            object o;
            string sql = @"
                SELECT ISNULL(SUM(ISNULL(det.TotalLine, 0)), 0)
                FROM tbl_MOInvoiceLink l
                INNER JOIN tbl_InvoiceHeader h ON h.Guid = l.InvoiceHeaderGuid AND h.CompanyID = l.CompanyID
                INNER JOIN tbl_InvoiceDetails det ON det.HeaderGuid = h.Guid AND det.CompanyID = h.CompanyID
                WHERE l.MOGuid = @MOGuid AND l.CompanyID = @CompanyID AND l.LinkTypeID = @LinkTypeID AND h.IsPosted = 1";
            if (trn == null)
                o = clsSQL.ExecuteScalar(sql, prm, clsSQL.CreateDataBaseConnectionString(companyId));
            else
                o = clsSQL.ExecuteScalar(sql, prm, clsSQL.CreateDataBaseConnectionString(companyId), trn);
            return Simulate.decimal_(o);
        }

        decimal GetLaborCost(string moGuid, int companyId, SqlTransaction trn = null)
        {
            clsSQL clsSQL = new clsSQL();
            SqlParameter[] prm =
            {
                new SqlParameter("@MOGuid", SqlDbType.UniqueIdentifier) { Value = Simulate.Guid(moGuid) },
                new SqlParameter("@CompanyID", SqlDbType.Int) { Value = companyId },
            };
            string sql = @"
                SELECT ISNULL(SUM(ISNULL(r.ActualHours, 0) * ISNULL(wc.HourlyRate, 0)), 0)
                FROM tbl_MORouting r
                LEFT JOIN tbl_WorkCenter wc ON wc.ID = r.WorkCenterID AND wc.CompanyID = r.CompanyID
                WHERE r.MOGuid = @MOGuid AND r.CompanyID = @CompanyID";
            try
            {
                object o = trn == null
                    ? clsSQL.ExecuteScalar(sql, prm, clsSQL.CreateDataBaseConnectionString(companyId))
                    : clsSQL.ExecuteScalar(sql, prm, clsSQL.CreateDataBaseConnectionString(companyId), trn);
                return Simulate.decimal_(o);
            }
            catch
            {
                return 0;
            }
        }

        static int GetAccountId(DataTable dt, int accountRefId)
        {
            if (dt == null) return 0;
            foreach (DataRow row in dt.Rows)
            {
                if (Simulate.Integer32(row["AccountRefID"]) == accountRefId)
                    return Simulate.Integer32(row["AccountID"]);
            }
            return 0;
        }

        Dictionary<Guid, StockRow> LoadStock(int companyId)
        {
            var map = new Dictionary<Guid, StockRow>();
            clsSQL clsSQL = new clsSQL();
            SqlParameter[] prm = { new SqlParameter("@CompanyID", SqlDbType.Int) { Value = companyId } };
            DataTable dt = clsSQL.ExecuteQueryStatement(@"
                SELECT d.ItemGuid,
                       SUM(CASE WHEN h.InvoiceTypeID IN (8, 26) THEN ISNULL(d.Qty, 0)
                                WHEN h.InvoiceTypeID IN (3, 9, 10, 11, 25) THEN -ISNULL(d.Qty, 0)
                                ELSE 0 END) AS Qty
                FROM tbl_InvoiceDetails d
                INNER JOIN tbl_InvoiceHeader h ON h.Guid = d.HeaderGuid AND h.CompanyID = d.CompanyID
                WHERE d.CompanyID = @CompanyID AND h.IsPosted = 1
                GROUP BY d.ItemGuid
            ", clsSQL.CreateDataBaseConnectionString(companyId), prm);
            if (dt == null) return map;
            foreach (DataRow r in dt.Rows)
            {
                Guid g = Simulate.Guid(Simulate.String(r["ItemGuid"]));
                map[g] = new StockRow { Qty = Simulate.decimal_(r["Qty"]) };
            }
            return map;
        }

        Dictionary<Guid, decimal> LoadOpenMoRemaining(int companyId)
        {
            var map = new Dictionary<Guid, decimal>();
            clsSQL clsSQL = new clsSQL();
            SqlParameter[] prm = { new SqlParameter("@CompanyID", SqlDbType.Int) { Value = companyId } };
            DataTable dt = clsSQL.ExecuteQueryStatement(@"
;WITH Planned AS (
    SELECT d.ItemGuid, SUM(ISNULL(d.PlannedQty, 0)) AS PlannedQty
    FROM tbl_MODetails d
    INNER JOIN tbl_MOHeader h ON h.Guid = d.HeaderGuid AND h.CompanyID = d.CompanyID
    WHERE d.CompanyID = @CompanyID AND d.LineTypeID = 26
      AND h.StatusID IN (0, 1, 2) AND h.IsActive = 1
    GROUP BY d.ItemGuid
),
Received AS (
    SELECT det.ItemGuid, SUM(ISNULL(det.Qty, 0)) AS ReceivedQty
    FROM tbl_MOInvoiceLink l
    INNER JOIN tbl_MOHeader h ON h.Guid = l.MOGuid AND h.CompanyID = l.CompanyID
    INNER JOIN tbl_InvoiceHeader ih ON ih.Guid = l.InvoiceHeaderGuid AND ih.CompanyID = l.CompanyID
    INNER JOIN tbl_InvoiceDetails det ON det.HeaderGuid = ih.Guid AND det.CompanyID = ih.CompanyID
    WHERE l.CompanyID = @CompanyID AND l.LinkTypeID = 26 AND ih.IsPosted = 1
      AND h.StatusID IN (0, 1, 2) AND h.IsActive = 1
    GROUP BY det.ItemGuid
)
SELECT p.ItemGuid,
       CAST(ISNULL(p.PlannedQty, 0) - ISNULL(r.ReceivedQty, 0) AS DECIMAL(18,3)) AS Remaining
FROM Planned p
LEFT JOIN Received r ON r.ItemGuid = p.ItemGuid
WHERE ISNULL(p.PlannedQty, 0) - ISNULL(r.ReceivedQty, 0) > 0
            ", clsSQL.CreateDataBaseConnectionString(companyId), prm);
            if (dt == null) return map;
            foreach (DataRow r in dt.Rows)
            {
                Guid g = Simulate.Guid(Simulate.String(r["ItemGuid"]));
                decimal rem = Simulate.decimal_(r["Remaining"]);
                if (rem < 0) rem = 0;
                map[g] = rem;
            }
            return map;
        }

        Dictionary<Guid, decimal> LoadSalesOfferDemand(int companyId)
        {
            var map = new Dictionary<Guid, decimal>();
            clsSQL clsSQL = new clsSQL();
            SqlParameter[] prm = { new SqlParameter("@CompanyID", SqlDbType.Int) { Value = companyId } };
            DataTable dt = clsSQL.ExecuteQueryStatement(@"
                SELECT d.ItemGuid, SUM(ISNULL(d.Qty, 0)) AS Qty
                FROM tbl_InvoiceDetails d
                INNER JOIN tbl_InvoiceHeader h ON h.Guid = d.HeaderGuid AND h.CompanyID = d.CompanyID
                WHERE d.CompanyID = @CompanyID AND h.InvoiceTypeID = 5
                  AND ISNULL(h.Status, 0) NOT IN (3, 4)
                GROUP BY d.ItemGuid
            ", clsSQL.CreateDataBaseConnectionString(companyId), prm);
            if (dt == null) return map;
            foreach (DataRow r in dt.Rows)
            {
                Guid g = Simulate.Guid(Simulate.String(r["ItemGuid"]));
                map[g] = Simulate.decimal_(r["Qty"]);
            }
            return map;
        }

        ItemLimits LoadItemLimits(Guid itemGuid, int companyId)
        {
            clsSQL clsSQL = new clsSQL();
            SqlParameter[] prm = { new SqlParameter("@Guid", SqlDbType.UniqueIdentifier) { Value = itemGuid } };
            DataTable dt = clsSQL.ExecuteQueryStatement(
                "SELECT TOP 1 AName, ISNULL(MinimumLimit, 0) AS MinimumLimit FROM tbl_Items WHERE Guid = @Guid",
                clsSQL.CreateDataBaseConnectionString(companyId), prm);
            if (dt == null || dt.Rows.Count == 0) return new ItemLimits { Name = "", MinimumLimit = 0 };
            return new ItemLimits
            {
                Name = Simulate.String(dt.Rows[0]["AName"]),
                MinimumLimit = Simulate.decimal_(dt.Rows[0]["MinimumLimit"])
            };
        }

        class ExplodeLine
        {
            public string ItemName;
            public decimal QtyBeforeScrap;
            public decimal ScrapPercent;
            public int UomId;
            public string Notes;
        }

        struct ChildBom
        {
            public int BomId;
            public decimal BatchQty;
            public decimal OutputQty;
        }

        struct StockRow
        {
            public decimal Qty;
        }

        struct ItemLimits
        {
            public string Name;
            public decimal MinimumLimit;
        }
    }
}
