using Microsoft.Data.SqlClient;
using System;
using System.Data;

namespace WebApplication2.cls
{
    public class clsInventoryAnalytics
    {
        public DataTable SelectSerialTrackingReport(Guid ItemGuid, int InvoiceType, string SerialNumber,
            DateTime date1, DateTime date2, int CompanyID)
        {
            SqlParameter[] prm =
            {
                new SqlParameter("@ItemGuid", SqlDbType.UniqueIdentifier) { Value = ItemGuid },
                new SqlParameter("@InvoiceType", SqlDbType.Int) { Value = InvoiceType },
                new SqlParameter("@SerialNumber", SqlDbType.NVarChar, 200) { Value = SerialNumber ?? "" },
                new SqlParameter("@date1", SqlDbType.DateTime) { Value = date1 },
                new SqlParameter("@date2", SqlDbType.DateTime) { Value = date2 },
                new SqlParameter("@CompanyID", SqlDbType.Int) { Value = CompanyID },
            };
            clsSQL clsSQL = new clsSQL();
            return clsSQL.ExecuteQueryStatement(
                @"SELECT
                    s.SerialNumber,
                    i.AName AS ItemsAName,
                    i.EName AS ItemsEName,
                    lt.LotNumber,
                    lt.ExpiryDate,
                    h.InvoiceNo,
                    CAST(h.InvoiceDate AS DATE) AS InvoiceDate,
                    jvt.AName AS JournalVoucherTypesAName,
                    b.AName AS BranchAName,
                    st.AName AS StoreAName,
                    bp.AName AS BusinessPartnerAName,
                    CASE WHEN s.Status = 1 THEN N'Active' ELSE N'Inactive' END AS SerialStatus
                  FROM tbl_InvoiceDetailsLotsSerialNumber s
                  INNER JOIN tbl_InvoiceDetailsLotsTracking lt ON lt.Guid = s.LotGuid
                  INNER JOIN tbl_InvoiceHeader h ON h.Guid = s.InvoiceGuid
                  INNER JOIN tbl_InvoiceDetails d ON d.Guid = s.InvoiceDetailsGuid
                  INNER JOIN tbl_Items i ON i.Guid = s.ItemGuid
                  LEFT JOIN tbl_JournalVoucherTypes jvt ON jvt.id = h.InvoiceTypeID
                  LEFT JOIN tbl_Branch b ON b.ID = h.BranchID
                  LEFT JOIN tbl_Store st ON st.ID = h.StoreID
                  LEFT JOIN tbl_BusinessPartner bp ON bp.ID = h.BusinessPartnerID
                  WHERE (s.CompanyID = @CompanyID OR @CompanyID = 0)
                    AND (s.ItemGuid = @ItemGuid OR @ItemGuid = '00000000-0000-0000-0000-000000000000')
                    AND (h.InvoiceTypeID = @InvoiceType OR @InvoiceType = 0)
                    AND (@SerialNumber = '' OR s.SerialNumber LIKE '%' + @SerialNumber + '%')
                    AND h.InvoiceDate BETWEEN @date1 AND @date2
                  ORDER BY h.InvoiceDate DESC, s.SerialNumber",
                clsSQL.CreateDataBaseConnectionString(CompanyID), prm);
        }

        public DataTable SelectExpiryLotsReport(Guid ItemGuid, int InvoiceType, string LotNumber,
            int DaysAhead, DateTime date1, DateTime date2, int CompanyID)
        {
            if (DaysAhead <= 0) DaysAhead = 90;
            SqlParameter[] prm =
            {
                new SqlParameter("@ItemGuid", SqlDbType.UniqueIdentifier) { Value = ItemGuid },
                new SqlParameter("@InvoiceType", SqlDbType.Int) { Value = InvoiceType },
                new SqlParameter("@LotNumber", SqlDbType.NVarChar, 200) { Value = LotNumber ?? "" },
                new SqlParameter("@DaysAhead", SqlDbType.Int) { Value = DaysAhead },
                new SqlParameter("@date1", SqlDbType.DateTime) { Value = date1 },
                new SqlParameter("@date2", SqlDbType.DateTime) { Value = date2 },
                new SqlParameter("@CompanyID", SqlDbType.Int) { Value = CompanyID },
            };
            clsSQL clsSQL = new clsSQL();
            return clsSQL.ExecuteQueryStatement(
                @"SELECT
                    i.AName AS ItemsAName,
                    i.EName AS ItemsEName,
                    lt.LotNumber,
                    lt.ExpiryDate,
                    DATEDIFF(DAY, CAST(GETDATE() AS DATE), CAST(lt.ExpiryDate AS DATE)) AS DaysToExpiry,
                    lt.QTY * jvt.QTYFactor AS QTY,
                    st.AName AS StoreAName,
                    b.AName AS BranchAName,
                    h.InvoiceNo,
                    CAST(h.InvoiceDate AS DATE) AS InvoiceDate,
                    jvt.AName AS JournalVoucherTypesAName,
                    bp.AName AS BusinessPartnerAName
                  FROM tbl_InvoiceDetailsLotsTracking lt
                  INNER JOIN tbl_InvoiceHeader h ON h.Guid = lt.InvoiceGuid
                  INNER JOIN tbl_InvoiceDetails d ON d.Guid = lt.InvoiceDetailsGuid
                  INNER JOIN tbl_Items i ON i.Guid = lt.ItemGuid
                  LEFT JOIN tbl_JournalVoucherTypes jvt ON jvt.id = h.InvoiceTypeID
                  LEFT JOIN tbl_Branch b ON b.ID = h.BranchID
                  LEFT JOIN tbl_Store st ON st.ID = h.StoreID
                  LEFT JOIN tbl_BusinessPartner bp ON bp.ID = h.BusinessPartnerID
                  WHERE (lt.CompanyID = @CompanyID OR @CompanyID = 0)
                    AND lt.ExpiryDate IS NOT NULL
                    AND CAST(lt.ExpiryDate AS DATE) > '1900-01-02'
                    AND (lt.ItemGuid = @ItemGuid OR @ItemGuid = '00000000-0000-0000-0000-000000000000')
                    AND (h.InvoiceTypeID = @InvoiceType OR @InvoiceType = 0)
                    AND (@LotNumber = '' OR lt.LotNumber LIKE '%' + @LotNumber + '%')
                    AND h.InvoiceDate BETWEEN @date1 AND @date2
                    AND DATEDIFF(DAY, CAST(GETDATE() AS DATE), CAST(lt.ExpiryDate AS DATE)) <= @DaysAhead
                  ORDER BY lt.ExpiryDate ASC, lt.LotNumber",
                clsSQL.CreateDataBaseConnectionString(CompanyID), prm);
        }

        public DataTable SelectInvoiceTaxSummaryReport(int InvoiceType, int BranchID, int BusinessPartnerID,
            DateTime date1, DateTime date2, int CompanyID)
        {
            SqlParameter[] prm =
            {
                new SqlParameter("@InvoiceType", SqlDbType.Int) { Value = InvoiceType },
                new SqlParameter("@BranchID", SqlDbType.Int) { Value = BranchID },
                new SqlParameter("@BusinessPartnerID", SqlDbType.Int) { Value = BusinessPartnerID },
                new SqlParameter("@date1", SqlDbType.DateTime) { Value = date1 },
                new SqlParameter("@date2", SqlDbType.DateTime) { Value = date2 },
                new SqlParameter("@CompanyID", SqlDbType.Int) { Value = CompanyID },
            };
            clsSQL clsSQL = new clsSQL();
            return clsSQL.ExecuteQueryStatement(
                @"SELECT
                    h.InvoiceNo,
                    CAST(h.InvoiceDate AS DATE) AS InvoiceDate,
                    jvt.AName AS JournalVoucherTypesAName,
                    bp.AName AS BusinessPartnerAName,
                    b.AName AS BranchAName,
                    SUM(d.TaxAmount) AS TaxAmount,
                    SUM(d.SpecialTaxAmount) AS SpecialTaxAmount,
                    SUM(d.TaxAmount + d.SpecialTaxAmount) AS TotalLineTax,
                    MAX(h.HeaderDiscount) AS HeaderDiscount,
                    MAX(h.TotalDiscount) AS TotalDiscount,
                    MAX(h.TotalTax) AS HeaderTotalTax,
                    MAX(h.TotalInvoice) AS TotalInvoice
                  FROM tbl_InvoiceHeader h
                  INNER JOIN tbl_InvoiceDetails d ON d.HeaderGuid = h.Guid
                  LEFT JOIN tbl_JournalVoucherTypes jvt ON jvt.id = h.InvoiceTypeID
                  LEFT JOIN tbl_BusinessPartner bp ON bp.ID = h.BusinessPartnerID
                  LEFT JOIN tbl_Branch b ON b.ID = h.BranchID
                  WHERE (h.CompanyID = @CompanyID OR @CompanyID = 0)
                    AND h.IsCounted = 1
                    AND (h.InvoiceTypeID = @InvoiceType OR @InvoiceType = 0)
                    AND (h.BranchID = @BranchID OR @BranchID = 0)
                    AND (h.BusinessPartnerID = @BusinessPartnerID OR @BusinessPartnerID = 0)
                    AND h.InvoiceDate BETWEEN @date1 AND @date2
                  GROUP BY h.Guid, h.InvoiceNo, h.InvoiceDate, jvt.AName, bp.AName, b.AName
                  ORDER BY h.InvoiceDate DESC, h.InvoiceNo",
                clsSQL.CreateDataBaseConnectionString(CompanyID), prm);
        }

        public DataTable SelectInventoryOperationsSummary(int CompanyID)
        {
            SqlParameter[] prm =
            {
                new SqlParameter("@CompanyID", SqlDbType.Int) { Value = CompanyID },
            };
            clsSQL clsSQL = new clsSQL();
            return clsSQL.ExecuteQueryStatement(
                @"SELECT
                    (SELECT COUNT(*) FROM tbl_Items WHERE CompanyID = @CompanyID AND IsActive = 1) AS ActiveItems,
                    (SELECT COUNT(*) FROM tbl_InvoiceDetailsLotsSerialNumber WHERE CompanyID = @CompanyID) AS TrackedSerials,
                    (SELECT COUNT(*) FROM tbl_InvoiceDetailsLotsTracking lt
                       WHERE lt.CompanyID = @CompanyID
                         AND lt.ExpiryDate IS NOT NULL
                         AND CAST(lt.ExpiryDate AS DATE) > '1900-01-02'
                         AND DATEDIFF(DAY, CAST(GETDATE() AS DATE), CAST(lt.ExpiryDate AS DATE)) BETWEEN 0 AND 30) AS ExpiringLots30,
                    (SELECT COUNT(*) FROM tbl_InvoiceDetailsLotsTracking lt
                       WHERE lt.CompanyID = @CompanyID
                         AND lt.ExpiryDate IS NOT NULL
                         AND CAST(lt.ExpiryDate AS DATE) > '1900-01-02'
                         AND DATEDIFF(DAY, CAST(GETDATE() AS DATE), CAST(lt.ExpiryDate AS DATE)) < 0) AS ExpiredLots,
                    ISNULL((SELECT SUM(d.TotalQTY * jvt.QTYFactor)
                       FROM tbl_InvoiceDetails d
                       INNER JOIN tbl_JournalVoucherTypes jvt ON jvt.id = d.InvoiceTypeID
                       WHERE d.CompanyID = @CompanyID AND d.IsCounted = 1), 0) AS NetStockQty,
                    (SELECT COUNT(*) FROM tbl_InvoiceDetailsLotsTracking WHERE CompanyID = @CompanyID) AS TotalLotRecords",
                clsSQL.CreateDataBaseConnectionString(CompanyID), prm);
        }

        public DataTable SelectInventoryOperationsPeriodSummary(int CompanyID, DateTime date1, DateTime date2)
        {
            SqlParameter[] prm =
            {
                new SqlParameter("@CompanyID", SqlDbType.Int) { Value = CompanyID },
                new SqlParameter("@date1", SqlDbType.DateTime) { Value = date1 },
                new SqlParameter("@date2", SqlDbType.DateTime) { Value = date2 },
            };
            clsSQL clsSQL = new clsSQL();
            return clsSQL.ExecuteQueryStatement(
                @"SELECT
                    ISNULL(SUM(CASE WHEN jvt.QTYFactor > 0 THEN d.TotalQTY * jvt.QTYFactor ELSE 0 END), 0) AS InQty,
                    ISNULL(SUM(CASE WHEN jvt.QTYFactor < 0 THEN ABS(d.TotalQTY * jvt.QTYFactor) ELSE 0 END), 0) AS OutQty,
                    ISNULL(SUM(d.TotalQTY * jvt.QTYFactor), 0) AS NetMovement,
                    COUNT(DISTINCT h.Guid) AS InvoiceCount
                  FROM tbl_InvoiceDetails d
                  INNER JOIN tbl_InvoiceHeader h ON h.Guid = d.HeaderGuid
                  INNER JOIN tbl_JournalVoucherTypes jvt ON jvt.id = d.InvoiceTypeID
                  WHERE d.CompanyID = @CompanyID
                    AND d.IsCounted = 1
                    AND h.InvoiceDate BETWEEN @date1 AND @date2",
                clsSQL.CreateDataBaseConnectionString(CompanyID), prm);
        }

        public DataTable SelectInventoryMovementTrend(int CompanyID, DateTime date1, DateTime date2)
        {
            SqlParameter[] prm =
            {
                new SqlParameter("@CompanyID", SqlDbType.Int) { Value = CompanyID },
                new SqlParameter("@date1", SqlDbType.DateTime) { Value = date1 },
                new SqlParameter("@date2", SqlDbType.DateTime) { Value = date2 },
            };
            clsSQL clsSQL = new clsSQL();
            return clsSQL.ExecuteQueryStatement(
                @"SELECT
                    CASE
                      WHEN DATEDIFF(DAY, @date1, @date2) <= 31 THEN FORMAT(h.InvoiceDate, 'yyyy-MM-dd')
                      ELSE FORMAT(h.InvoiceDate, 'yyyy-MM')
                    END AS Month,
                    SUM(CASE WHEN jvt.QTYFactor > 0 THEN d.TotalQTY * jvt.QTYFactor ELSE 0 END) AS InQty,
                    SUM(CASE WHEN jvt.QTYFactor < 0 THEN ABS(d.TotalQTY * jvt.QTYFactor) ELSE 0 END) AS OutQty
                  FROM tbl_InvoiceDetails d
                  INNER JOIN tbl_InvoiceHeader h ON h.Guid = d.HeaderGuid
                  INNER JOIN tbl_JournalVoucherTypes jvt ON jvt.id = d.InvoiceTypeID
                  WHERE d.CompanyID = @CompanyID
                    AND d.IsCounted = 1
                    AND h.InvoiceDate BETWEEN @date1 AND @date2
                  GROUP BY CASE
                      WHEN DATEDIFF(DAY, @date1, @date2) <= 31 THEN FORMAT(h.InvoiceDate, 'yyyy-MM-dd')
                      ELSE FORMAT(h.InvoiceDate, 'yyyy-MM')
                    END
                  ORDER BY Month",
                clsSQL.CreateDataBaseConnectionString(CompanyID), prm);
        }

        public DataTable SelectInventoryTopItemsByMovement(int CompanyID, int TopN, DateTime date1, DateTime date2)
        {
            if (TopN <= 0) TopN = 10;
            SqlParameter[] prm =
            {
                new SqlParameter("@CompanyID", SqlDbType.Int) { Value = CompanyID },
                new SqlParameter("@TopN", SqlDbType.Int) { Value = TopN },
                new SqlParameter("@date1", SqlDbType.DateTime) { Value = date1 },
                new SqlParameter("@date2", SqlDbType.DateTime) { Value = date2 },
            };
            clsSQL clsSQL = new clsSQL();
            return clsSQL.ExecuteQueryStatement(
                @"SELECT TOP (@TopN)
                    i.AName AS Name,
                    i.EName,
                    SUM(ABS(d.TotalQTY * jvt.QTYFactor)) AS Total,
                    SUM(d.TotalQTY * jvt.QTYFactor) AS NetQty
                  FROM tbl_InvoiceDetails d
                  INNER JOIN tbl_InvoiceHeader h ON h.Guid = d.HeaderGuid
                  INNER JOIN tbl_JournalVoucherTypes jvt ON jvt.id = d.InvoiceTypeID
                  INNER JOIN tbl_Items i ON i.Guid = d.ItemGuid
                  WHERE d.CompanyID = @CompanyID
                    AND d.IsCounted = 1
                    AND h.InvoiceDate BETWEEN @date1 AND @date2
                  GROUP BY i.Guid, i.AName, i.EName
                  ORDER BY Total DESC",
                clsSQL.CreateDataBaseConnectionString(CompanyID), prm);
        }

        public DataTable SelectInventoryUpcomingExpiry(int CompanyID, int TopN)
        {
            if (TopN <= 0) TopN = 10;
            SqlParameter[] prm =
            {
                new SqlParameter("@CompanyID", SqlDbType.Int) { Value = CompanyID },
                new SqlParameter("@TopN", SqlDbType.Int) { Value = TopN },
            };
            clsSQL clsSQL = new clsSQL();
            return clsSQL.ExecuteQueryStatement(
                @"SELECT TOP (@TopN)
                    i.AName AS Name,
                    i.EName,
                    lt.LotNumber,
                    lt.ExpiryDate,
                    DATEDIFF(DAY, CAST(GETDATE() AS DATE), CAST(lt.ExpiryDate AS DATE)) AS DaysToExpiry,
                    lt.QTY AS Total
                  FROM tbl_InvoiceDetailsLotsTracking lt
                  INNER JOIN tbl_Items i ON i.Guid = lt.ItemGuid
                  WHERE lt.CompanyID = @CompanyID
                    AND lt.ExpiryDate IS NOT NULL
                    AND CAST(lt.ExpiryDate AS DATE) > '1900-01-02'
                    AND DATEDIFF(DAY, CAST(GETDATE() AS DATE), CAST(lt.ExpiryDate AS DATE)) >= 0
                  ORDER BY lt.ExpiryDate ASC",
                clsSQL.CreateDataBaseConnectionString(CompanyID), prm);
        }

        public DataTable SelectInvoiceAnalyticsSummary(int CompanyID, DateTime date1, DateTime date2)
        {
            SqlParameter[] prm =
            {
                new SqlParameter("@CompanyID", SqlDbType.Int) { Value = CompanyID },
                new SqlParameter("@date1", SqlDbType.DateTime) { Value = date1 },
                new SqlParameter("@date2", SqlDbType.DateTime) { Value = date2 },
            };
            clsSQL clsSQL = new clsSQL();
            return clsSQL.ExecuteQueryStatement(
                @"SELECT
                    ISNULL(SUM(CASE WHEN h.InvoiceTypeID IN (3, 10, 19)
                      THEN h.TotalInvoice ELSE 0 END), 0) AS SalesMTD,
                    ISNULL(SUM(CASE WHEN h.InvoiceTypeID IN (2, 22, 8)
                      THEN h.TotalInvoice ELSE 0 END), 0) AS PurchaseMTD,
                    ISNULL(SUM(CASE WHEN h.InvoiceTypeID IN (3, 10, 19)
                      THEN 1 ELSE 0 END), 0) AS SalesCountMTD,
                    ISNULL(SUM(CASE WHEN h.InvoiceTypeID IN (2, 22, 8)
                      THEN 1 ELSE 0 END), 0) AS PurchaseCountMTD,
                    ISNULL(SUM(ISNULL(h.TotalTax, 0)), 0) AS TaxMTD,
                    ISNULL(SUM(ISNULL(h.TotalDiscount, 0)), 0) AS DiscountMTD
                  FROM tbl_InvoiceHeader h
                  WHERE h.CompanyID = @CompanyID
                    AND h.IsCounted = 1
                    AND h.InvoiceDate BETWEEN @date1 AND @date2",
                clsSQL.CreateDataBaseConnectionString(CompanyID), prm);
        }

        public DataTable SelectInvoiceAnalyticsMonthlyTrend(int CompanyID, DateTime date1, DateTime date2)
        {
            SqlParameter[] prm =
            {
                new SqlParameter("@CompanyID", SqlDbType.Int) { Value = CompanyID },
                new SqlParameter("@date1", SqlDbType.DateTime) { Value = date1 },
                new SqlParameter("@date2", SqlDbType.DateTime) { Value = date2 },
            };
            clsSQL clsSQL = new clsSQL();
            return clsSQL.ExecuteQueryStatement(
                @"SELECT
                    CASE
                      WHEN DATEDIFF(DAY, @date1, @date2) <= 31 THEN FORMAT(h.InvoiceDate, 'yyyy-MM-dd')
                      ELSE FORMAT(h.InvoiceDate, 'yyyy-MM')
                    END AS Month,
                    SUM(CASE WHEN h.InvoiceTypeID IN (3, 10, 19) THEN h.TotalInvoice ELSE 0 END) AS Sales,
                    SUM(CASE WHEN h.InvoiceTypeID IN (2, 22, 8) THEN h.TotalInvoice ELSE 0 END) AS Purchases
                  FROM tbl_InvoiceHeader h
                  WHERE h.CompanyID = @CompanyID
                    AND h.IsCounted = 1
                    AND h.InvoiceDate BETWEEN @date1 AND @date2
                  GROUP BY CASE
                      WHEN DATEDIFF(DAY, @date1, @date2) <= 31 THEN FORMAT(h.InvoiceDate, 'yyyy-MM-dd')
                      ELSE FORMAT(h.InvoiceDate, 'yyyy-MM')
                    END
                  ORDER BY Month",
                clsSQL.CreateDataBaseConnectionString(CompanyID), prm);
        }

        public DataTable SelectInvoiceAnalyticsTaxBreakdown(int CompanyID, DateTime date1, DateTime date2)
        {
            SqlParameter[] prm =
            {
                new SqlParameter("@CompanyID", SqlDbType.Int) { Value = CompanyID },
                new SqlParameter("@date1", SqlDbType.DateTime) { Value = date1 },
                new SqlParameter("@date2", SqlDbType.DateTime) { Value = date2 },
            };
            clsSQL clsSQL = new clsSQL();
            return clsSQL.ExecuteQueryStatement(
                @"SELECT
                    SUM(d.TaxAmount) AS RegularTax,
                    SUM(d.SpecialTaxAmount) AS SpecialTax,
                    SUM(d.TaxAmount + d.SpecialTaxAmount) AS TotalTax
                  FROM tbl_InvoiceDetails d
                  INNER JOIN tbl_InvoiceHeader h ON h.Guid = d.HeaderGuid
                  WHERE d.CompanyID = @CompanyID
                    AND h.IsCounted = 1
                    AND h.InvoiceDate BETWEEN @date1 AND @date2",
                clsSQL.CreateDataBaseConnectionString(CompanyID), prm);
        }

        public DataTable SelectInvoiceAnalyticsPaymentMix(int CompanyID, DateTime date1, DateTime date2)
        {
            SqlParameter[] prm =
            {
                new SqlParameter("@CompanyID", SqlDbType.Int) { Value = CompanyID },
                new SqlParameter("@date1", SqlDbType.DateTime) { Value = date1 },
                new SqlParameter("@date2", SqlDbType.DateTime) { Value = date2 },
            };
            clsSQL clsSQL = new clsSQL();
            return clsSQL.ExecuteQueryStatement(
                @"SELECT
                    CASE WHEN ISNULL(pm.AName, '') = '' THEN N'Unknown' ELSE pm.AName END AS Name,
                    COUNT(*) AS Total,
                    ISNULL(SUM(h.TotalInvoice), 0) AS Value
                  FROM tbl_InvoiceHeader h
                  LEFT JOIN tbl_PaymentMethod pm ON pm.ID = h.PaymentMethodID
                  WHERE h.CompanyID = @CompanyID
                    AND h.IsCounted = 1
                    AND h.InvoiceDate BETWEEN @date1 AND @date2
                  GROUP BY CASE WHEN ISNULL(pm.AName, '') = '' THEN N'Unknown' ELSE pm.AName END
                  ORDER BY Value DESC",
                clsSQL.CreateDataBaseConnectionString(CompanyID), prm);
        }

        public DataTable SelectInvoiceAnalyticsRecentInvoices(int CompanyID, int TopN, DateTime date1, DateTime date2)
        {
            if (TopN <= 0) TopN = 8;
            SqlParameter[] prm =
            {
                new SqlParameter("@CompanyID", SqlDbType.Int) { Value = CompanyID },
                new SqlParameter("@TopN", SqlDbType.Int) { Value = TopN },
                new SqlParameter("@date1", SqlDbType.DateTime) { Value = date1 },
                new SqlParameter("@date2", SqlDbType.DateTime) { Value = date2 },
            };
            clsSQL clsSQL = new clsSQL();
            return clsSQL.ExecuteQueryStatement(
                @"SELECT TOP (@TopN)
                    h.InvoiceNo,
                    CAST(h.InvoiceDate AS DATE) AS InvoiceDate,
                    jvt.AName AS InvoiceTypeName,
                    bp.AName AS BusinessPartnerAName,
                    h.TotalInvoice,
                    h.TotalTax,
                    h.TotalDiscount
                  FROM tbl_InvoiceHeader h
                  LEFT JOIN tbl_JournalVoucherTypes jvt ON jvt.id = h.InvoiceTypeID
                  LEFT JOIN tbl_BusinessPartner bp ON bp.ID = h.BusinessPartnerID
                  WHERE h.CompanyID = @CompanyID
                    AND h.IsCounted = 1
                    AND h.InvoiceDate BETWEEN @date1 AND @date2
                  ORDER BY h.InvoiceDate DESC, h.InvoiceNo DESC",
                clsSQL.CreateDataBaseConnectionString(CompanyID), prm);
        }

        // ============================================================
        // Inventory Valuation & Health
        // On-hand qty is the source of truth: SUM(TotalQTY * QTYFactor) over
        // counted lines (matching the costing/operations basis). Stock value
        // uses the item's current weighted-average cost (tbl_Items.AVGCostPerUnit).
        // All queries are optionally scoped to a single store (@StoreID = 0 => all).
        // ============================================================

        public DataTable SelectInventoryValuationSummary(int CompanyID, int StoreID)
        {
            SqlParameter[] prm =
            {
                new SqlParameter("@CompanyID", SqlDbType.Int) { Value = CompanyID },
                new SqlParameter("@StoreID", SqlDbType.Int) { Value = StoreID },
            };
            clsSQL clsSQL = new clsSQL();
            return clsSQL.ExecuteQueryStatement(
                @"WITH OnHand AS (
                    SELECT i.Guid,
                           ISNULL(i.AVGCostPerUnit, 0) AS AvgCost,
                           ISNULL(SUM(d.TotalQTY * jvt.QTYFactor), 0) AS OnHandQty,
                           MAX(CASE WHEN jvt.QTYFactor < 0 THEN h.InvoiceDate END) AS LastOutDate
                    FROM tbl_Items i
                    LEFT JOIN tbl_InvoiceDetails d
                           ON d.ItemGuid = i.Guid AND d.IsCounted = 1
                          AND (@StoreID = 0 OR d.StoreID = @StoreID)
                          AND (d.CompanyID = @CompanyID OR @CompanyID = 0)
                    LEFT JOIN tbl_InvoiceHeader h ON h.Guid = d.HeaderGuid
                    LEFT JOIN tbl_JournalVoucherTypes jvt ON jvt.id = d.InvoiceTypeID
                    WHERE (i.CompanyID = @CompanyID OR @CompanyID = 0)
                    GROUP BY i.Guid, i.AVGCostPerUnit
                  )
                  SELECT
                    ISNULL(SUM(CASE WHEN OnHandQty > 0 THEN OnHandQty * AvgCost ELSE 0 END), 0) AS TotalInventoryValue,
                    ISNULL(SUM(OnHandQty), 0) AS TotalOnHandQty,
                    SUM(CASE WHEN OnHandQty > 0 THEN 1 ELSE 0 END) AS ItemsInStock,
                    SUM(CASE WHEN OnHandQty < 0 THEN 1 ELSE 0 END) AS NegativeStockItems,
                    SUM(CASE WHEN OnHandQty = 0 THEN 1 ELSE 0 END) AS ZeroStockItems,
                    SUM(CASE WHEN OnHandQty > 0
                              AND (LastOutDate IS NULL
                                   OR DATEDIFF(DAY, LastOutDate, GETDATE()) >= 90)
                             THEN 1 ELSE 0 END) AS DeadStockItems
                  FROM OnHand",
                clsSQL.CreateDataBaseConnectionString(CompanyID), prm);
        }

        public DataTable SelectInventoryValueByStore(int CompanyID)
        {
            SqlParameter[] prm =
            {
                new SqlParameter("@CompanyID", SqlDbType.Int) { Value = CompanyID },
            };
            clsSQL clsSQL = new clsSQL();
            return clsSQL.ExecuteQueryStatement(
                @"SELECT
                    CASE WHEN ISNULL(st.AName, '') = '' THEN N'Unassigned' ELSE st.AName END AS Name,
                    ISNULL(SUM(d.TotalQTY * jvt.QTYFactor * ISNULL(i.AVGCostPerUnit, 0)), 0) AS Value
                  FROM tbl_InvoiceDetails d
                  INNER JOIN tbl_JournalVoucherTypes jvt ON jvt.id = d.InvoiceTypeID
                  INNER JOIN tbl_Items i ON i.Guid = d.ItemGuid
                  LEFT JOIN tbl_Store st ON st.ID = d.StoreID
                  WHERE (d.CompanyID = @CompanyID OR @CompanyID = 0)
                    AND d.IsCounted = 1
                  GROUP BY CASE WHEN ISNULL(st.AName, '') = '' THEN N'Unassigned' ELSE st.AName END
                  HAVING SUM(d.TotalQTY * jvt.QTYFactor * ISNULL(i.AVGCostPerUnit, 0)) > 0
                  ORDER BY Value DESC",
                clsSQL.CreateDataBaseConnectionString(CompanyID), prm);
        }

        public DataTable SelectTopItemsByValue(int CompanyID, int TopN, int StoreID)
        {
            if (TopN <= 0) TopN = 10;
            SqlParameter[] prm =
            {
                new SqlParameter("@CompanyID", SqlDbType.Int) { Value = CompanyID },
                new SqlParameter("@TopN", SqlDbType.Int) { Value = TopN },
                new SqlParameter("@StoreID", SqlDbType.Int) { Value = StoreID },
            };
            clsSQL clsSQL = new clsSQL();
            return clsSQL.ExecuteQueryStatement(
                @"SELECT TOP (@TopN)
                    i.AName AS Name,
                    i.EName,
                    ISNULL(SUM(d.TotalQTY * jvt.QTYFactor), 0) AS Total,
                    ISNULL(SUM(d.TotalQTY * jvt.QTYFactor), 0) * ISNULL(i.AVGCostPerUnit, 0) AS Value
                  FROM tbl_Items i
                  LEFT JOIN tbl_InvoiceDetails d
                         ON d.ItemGuid = i.Guid AND d.IsCounted = 1
                        AND (@StoreID = 0 OR d.StoreID = @StoreID)
                        AND (d.CompanyID = @CompanyID OR @CompanyID = 0)
                  LEFT JOIN tbl_JournalVoucherTypes jvt ON jvt.id = d.InvoiceTypeID
                  WHERE (i.CompanyID = @CompanyID OR @CompanyID = 0)
                  GROUP BY i.Guid, i.AName, i.EName, i.AVGCostPerUnit
                  HAVING ISNULL(SUM(d.TotalQTY * jvt.QTYFactor), 0) * ISNULL(i.AVGCostPerUnit, 0) > 0
                  ORDER BY Value DESC",
                clsSQL.CreateDataBaseConnectionString(CompanyID), prm);
        }

        public DataTable SelectInventoryTurnover(int CompanyID, DateTime date1, DateTime date2, int StoreID)
        {
            SqlParameter[] prm =
            {
                new SqlParameter("@CompanyID", SqlDbType.Int) { Value = CompanyID },
                new SqlParameter("@StoreID", SqlDbType.Int) { Value = StoreID },
                new SqlParameter("@date1", SqlDbType.DateTime) { Value = date1 },
                new SqlParameter("@date2", SqlDbType.DateTime) { Value = date2 },
            };
            clsSQL clsSQL = new clsSQL();
            // COGS = cost of stock that left in the period (outbound lines valued at the
            // cost recorded on the line). Current inventory value is used as a proxy for
            // average inventory; the Flutter layer derives turnover ratio and days on hand.
            return clsSQL.ExecuteQueryStatement(
                @"SELECT
                    ISNULL((SELECT SUM(d.TotalQTY * ABS(jvt.QTYFactor) * ISNULL(d.AVGCostPerUnit, 0))
                            FROM tbl_InvoiceDetails d
                            INNER JOIN tbl_InvoiceHeader h ON h.Guid = d.HeaderGuid
                            INNER JOIN tbl_JournalVoucherTypes jvt ON jvt.id = d.InvoiceTypeID
                            WHERE d.IsCounted = 1 AND jvt.QTYFactor < 0
                              AND (d.CompanyID = @CompanyID OR @CompanyID = 0)
                              AND (@StoreID = 0 OR d.StoreID = @StoreID)
                              AND h.InvoiceDate BETWEEN @date1 AND @date2), 0) AS Cogs,
                    ISNULL((SELECT SUM(CASE WHEN q.OnHandQty > 0 THEN q.OnHandQty * q.AvgCost ELSE 0 END)
                            FROM (
                              SELECT ISNULL(i.AVGCostPerUnit, 0) AS AvgCost,
                                     ISNULL(SUM(d.TotalQTY * jvt.QTYFactor), 0) AS OnHandQty
                              FROM tbl_Items i
                              LEFT JOIN tbl_InvoiceDetails d
                                     ON d.ItemGuid = i.Guid AND d.IsCounted = 1
                                    AND (@StoreID = 0 OR d.StoreID = @StoreID)
                                    AND (d.CompanyID = @CompanyID OR @CompanyID = 0)
                              LEFT JOIN tbl_JournalVoucherTypes jvt ON jvt.id = d.InvoiceTypeID
                              WHERE (i.CompanyID = @CompanyID OR @CompanyID = 0)
                              GROUP BY i.Guid, i.AVGCostPerUnit
                            ) q), 0) AS InventoryValue,
                    DATEDIFF(DAY, @date1, @date2) AS PeriodDays",
                clsSQL.CreateDataBaseConnectionString(CompanyID), prm);
        }

        public DataTable SelectStockValuationReport(int CompanyID, int StoreID, Guid ItemGuid)
        {
            SqlParameter[] prm =
            {
                new SqlParameter("@CompanyID", SqlDbType.Int) { Value = CompanyID },
                new SqlParameter("@StoreID", SqlDbType.Int) { Value = StoreID },
                new SqlParameter("@ItemGuid", SqlDbType.UniqueIdentifier) { Value = ItemGuid },
            };
            clsSQL clsSQL = new clsSQL();
            return clsSQL.ExecuteQueryStatement(
                @"SELECT
                    i.AName AS ItemName,
                    i.EName AS ItemNameEn,
                    ISNULL(SUM(d.TotalQTY * jvt.QTYFactor), 0) AS OnHandQty,
                    ISNULL(i.AVGCostPerUnit, 0) AS AvgCost,
                    CAST(ISNULL(SUM(d.TotalQTY * jvt.QTYFactor), 0) * ISNULL(i.AVGCostPerUnit, 0) AS DECIMAL(18,2)) AS StockValue
                  FROM tbl_Items i
                  LEFT JOIN tbl_InvoiceDetails d
                         ON d.ItemGuid = i.Guid AND d.IsCounted = 1
                        AND (@StoreID = 0 OR d.StoreID = @StoreID)
                        AND (d.CompanyID = @CompanyID OR @CompanyID = 0)
                  LEFT JOIN tbl_JournalVoucherTypes jvt ON jvt.id = d.InvoiceTypeID
                  WHERE (i.CompanyID = @CompanyID OR @CompanyID = 0)
                    AND (@ItemGuid = '00000000-0000-0000-0000-000000000000' OR i.Guid = @ItemGuid)
                  GROUP BY i.Guid, i.AName, i.EName, i.AVGCostPerUnit
                  HAVING ISNULL(SUM(d.TotalQTY * jvt.QTYFactor), 0) <> 0
                  ORDER BY StockValue DESC",
                clsSQL.CreateDataBaseConnectionString(CompanyID), prm);
        }

        public DataTable SelectSlowMovingItems(int CompanyID, int Days, int StoreID)
        {
            if (Days <= 0) Days = 90;
            SqlParameter[] prm =
            {
                new SqlParameter("@CompanyID", SqlDbType.Int) { Value = CompanyID },
                new SqlParameter("@StoreID", SqlDbType.Int) { Value = StoreID },
                new SqlParameter("@Days", SqlDbType.Int) { Value = Days },
            };
            clsSQL clsSQL = new clsSQL();
            return clsSQL.ExecuteQueryStatement(
                @"WITH OnHand AS (
                    SELECT d.ItemGuid, SUM(d.TotalQTY * jvt.QTYFactor) AS OnHandQty
                    FROM tbl_InvoiceDetails d
                    INNER JOIN tbl_JournalVoucherTypes jvt ON jvt.id = d.InvoiceTypeID
                    WHERE d.IsCounted = 1
                      AND (d.CompanyID = @CompanyID OR @CompanyID = 0)
                      AND (@StoreID = 0 OR d.StoreID = @StoreID)
                    GROUP BY d.ItemGuid
                  ),
                  LastOut AS (
                    SELECT d.ItemGuid, MAX(h.InvoiceDate) AS LastOutDate
                    FROM tbl_InvoiceDetails d
                    INNER JOIN tbl_InvoiceHeader h ON h.Guid = d.HeaderGuid
                    INNER JOIN tbl_JournalVoucherTypes jvt ON jvt.id = d.InvoiceTypeID
                    WHERE d.IsCounted = 1 AND jvt.QTYFactor < 0
                      AND (d.CompanyID = @CompanyID OR @CompanyID = 0)
                      AND (@StoreID = 0 OR d.StoreID = @StoreID)
                    GROUP BY d.ItemGuid
                  )
                  SELECT
                    i.AName AS ItemName,
                    i.EName AS ItemNameEn,
                    oh.OnHandQty,
                    ISNULL(i.AVGCostPerUnit, 0) AS AvgCost,
                    CAST(oh.OnHandQty * ISNULL(i.AVGCostPerUnit, 0) AS DECIMAL(18,2)) AS StockValue,
                    CAST(lo.LastOutDate AS DATE) AS LastSaleDate,
                    ISNULL(DATEDIFF(DAY, lo.LastOutDate, GETDATE()), 9999) AS DaysIdle
                  FROM OnHand oh
                  INNER JOIN tbl_Items i ON i.Guid = oh.ItemGuid
                  LEFT JOIN LastOut lo ON lo.ItemGuid = oh.ItemGuid
                  WHERE oh.OnHandQty > 0
                    AND (lo.LastOutDate IS NULL OR DATEDIFF(DAY, lo.LastOutDate, GETDATE()) >= @Days)
                  ORDER BY DaysIdle DESC, StockValue DESC",
                clsSQL.CreateDataBaseConnectionString(CompanyID), prm);
        }

        public DataTable SelectReorderReport(int CompanyID, int StoreID)
        {
            SqlParameter[] prm =
            {
                new SqlParameter("@CompanyID", SqlDbType.Int) { Value = CompanyID },
                new SqlParameter("@StoreID", SqlDbType.Int) { Value = StoreID },
            };
            clsSQL clsSQL = new clsSQL();
            return clsSQL.ExecuteQueryStatement(
                @"SELECT * FROM (
                    SELECT
                      i.AName AS ItemName,
                      i.EName AS ItemNameEn,
                      CASE WHEN ISNULL(st.AName, '') = '' THEN N'All Stores' ELSE st.AName END AS StoreName,
                      r.ReorderPointQty,
                      r.MinQty,
                      r.MaxQty,
                      r.SafetyStockQty,
                      r.ReorderQty AS SuggestedOrderQty,
                      ISNULL((SELECT SUM(d.TotalQTY * jvt.QTYFactor)
                              FROM tbl_InvoiceDetails d
                              INNER JOIN tbl_JournalVoucherTypes jvt ON jvt.id = d.InvoiceTypeID
                              WHERE d.IsCounted = 1 AND d.ItemGuid = i.Guid
                                AND (d.CompanyID = @CompanyID OR @CompanyID = 0)
                                AND (r.WarehouseID = 0 OR d.StoreID = r.WarehouseID)), 0) AS OnHandQty
                    FROM tbl_ItemReorder r
                    INNER JOIN tbl_Items i ON i.Guid = TRY_CONVERT(uniqueidentifier, r.ItemGuid)
                    LEFT JOIN tbl_Store st ON st.ID = r.WarehouseID
                    WHERE r.IsActive = 1
                      AND (r.CompanyID = @CompanyID OR @CompanyID = 0)
                      AND (@StoreID = 0 OR r.WarehouseID = @StoreID OR r.WarehouseID = 0)
                  ) q
                  WHERE q.OnHandQty <= q.ReorderPointQty
                  ORDER BY (q.ReorderPointQty - q.OnHandQty) DESC",
                clsSQL.CreateDataBaseConnectionString(CompanyID), prm);
        }
    }
}
