using System;
using System.Data;
using Microsoft.Data.SqlClient;
using WebApplication2.MainClasses;

namespace WebApplication2.cls
{
    /// <summary>
    /// Maintains tbl_StockBalance, a per item/store snapshot of on-hand quantity and
    /// average cost. The authoritative source of truth remains tbl_InvoiceDetails; this
    /// snapshot is a performance cache that can be rebuilt at any time and read cheaply.
    /// </summary>
    public class clsStockBalance
    {
        // Recomputes and upserts the snapshot row for a single item/store.
        public void Refresh(string itemGuid, int storeId, int branchId, int companyId, SqlTransaction trn = null)
        {
            clsSQL clsSQL = new clsSQL();
            SqlParameter[] prm =
            {
                new SqlParameter("@ItemGuid", SqlDbType.UniqueIdentifier) { Value = Simulate.Guid(itemGuid) },
                new SqlParameter("@StoreID", SqlDbType.Int) { Value = storeId },
                new SqlParameter("@BranchID", SqlDbType.Int) { Value = branchId },
                new SqlParameter("@CompanyID", SqlDbType.Int) { Value = companyId },
            };

            string sql = @"
DECLARE @OnHand DECIMAL(18,2) = (
    SELECT ISNULL(SUM(d.TotalQTY * jvt.QTYFactor), 0)
    FROM tbl_InvoiceDetails d
    LEFT JOIN tbl_JournalVoucherTypes jvt ON jvt.id = d.InvoiceTypeID
    WHERE d.IsCounted = 1 AND d.ItemGuid = @ItemGuid
      AND (d.StoreID = @StoreID OR @StoreID = 0)
      AND (d.CompanyID = @CompanyID OR @CompanyID = 0));

DECLARE @AvgCost DECIMAL(18,2) = (
    SELECT ISNULL(AVGCostPerUnit, 0) FROM tbl_Items
    WHERE Guid = @ItemGuid AND (CompanyID = @CompanyID OR @CompanyID = 0));

IF EXISTS (SELECT 1 FROM tbl_StockBalance WHERE ItemGuid = @ItemGuid AND StoreID = @StoreID AND CompanyID = @CompanyID)
    UPDATE tbl_StockBalance
       SET OnHandQty = @OnHand, AvgCost = @AvgCost, BranchID = @BranchID, LastUpdated = GETDATE()
     WHERE ItemGuid = @ItemGuid AND StoreID = @StoreID AND CompanyID = @CompanyID;
ELSE
    INSERT INTO tbl_StockBalance (ItemGuid, StoreID, BranchID, CompanyID, OnHandQty, AvgCost, LastUpdated)
    VALUES (@ItemGuid, @StoreID, @BranchID, @CompanyID, @OnHand, @AvgCost, GETDATE());";

            clsSQL.ExecuteNonQueryStatement(sql, clsSQL.CreateDataBaseConnectionString(companyId), prm, trn);
        }

        // Rebuilds the entire snapshot for a company from transaction history.
        public int RebuildAll(int companyId)
        {
            clsSQL clsSQL = new clsSQL();
            SqlParameter[] prm =
            {
                new SqlParameter("@CompanyID", SqlDbType.Int) { Value = companyId },
            };

            string sql = @"
;WITH bal AS (
    SELECT d.ItemGuid, d.StoreID,
           MIN(d.BranchID) AS BranchID,
           SUM(d.TotalQTY * jvt.QTYFactor) AS OnHand
    FROM tbl_InvoiceDetails d
    LEFT JOIN tbl_JournalVoucherTypes jvt ON jvt.id = d.InvoiceTypeID
    WHERE d.IsCounted = 1 AND (d.CompanyID = @CompanyID OR @CompanyID = 0)
    GROUP BY d.ItemGuid, d.StoreID
)
MERGE tbl_StockBalance AS tgt
USING (
    SELECT b.ItemGuid, b.StoreID, b.BranchID, b.OnHand,
           ISNULL(i.AVGCostPerUnit, 0) AS AvgCost
    FROM bal b
    LEFT JOIN tbl_Items i ON i.Guid = b.ItemGuid
) AS src
ON tgt.ItemGuid = src.ItemGuid AND tgt.StoreID = src.StoreID AND tgt.CompanyID = @CompanyID
WHEN MATCHED THEN
    UPDATE SET OnHandQty = src.OnHand, AvgCost = src.AvgCost, BranchID = src.BranchID, LastUpdated = GETDATE()
WHEN NOT MATCHED THEN
    INSERT (ItemGuid, StoreID, BranchID, CompanyID, OnHandQty, AvgCost, LastUpdated)
    VALUES (src.ItemGuid, src.StoreID, src.BranchID, @CompanyID, src.OnHand, src.AvgCost, GETDATE());";

            return clsSQL.ExecuteNonQueryStatement(sql, clsSQL.CreateDataBaseConnectionString(companyId), prm);
        }

        // Fast on-hand read from the snapshot (falls back to 0 when not yet built).
        public decimal GetOnHand(string itemGuid, int storeId, int companyId)
        {
            clsSQL clsSQL = new clsSQL();
            SqlParameter[] prm =
            {
                new SqlParameter("@ItemGuid", SqlDbType.UniqueIdentifier) { Value = Simulate.Guid(itemGuid) },
                new SqlParameter("@StoreID", SqlDbType.Int) { Value = storeId },
                new SqlParameter("@CompanyID", SqlDbType.Int) { Value = companyId },
            };
            DataTable dt = clsSQL.ExecuteQueryStatement(
                @"SELECT ISNULL(SUM(OnHandQty),0) AS OnHand FROM tbl_StockBalance
                  WHERE ItemGuid = @ItemGuid AND (StoreID = @StoreID OR @StoreID = 0) AND (CompanyID = @CompanyID OR @CompanyID = 0)",
                clsSQL.CreateDataBaseConnectionString(companyId), prm);
            if (dt != null && dt.Rows.Count > 0)
                return Simulate.decimal_(dt.Rows[0]["OnHand"]);
            return 0;
        }
    }
}
