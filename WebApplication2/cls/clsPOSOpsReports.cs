using System;
using System.Data;
using Microsoft.Data.SqlClient;

namespace WebApplication2.cls
{
    /// <summary>
    /// POS operational reports: X/Z day-end, sales by cashier/hour/category,
    /// and refunds/discounts/voids audit.
    /// </summary>
    public class clsPOSOpsReports
    {
        private static readonly string PosSalesFilter =
            @" AND h.IsCounted = 1
               AND h.InvoiceTypeID IN (10, 11)
               AND (h.CompanyID = @CompanyID OR @CompanyID = 0)
               AND (h.BranchID = @BranchID OR @BranchID = 0)
               AND (h.CashID = @CashID OR @CashID = 0)
               AND (h.CreationUserID = @FilterUserID OR @FilterUserID = 0)
               AND (
                     (@POSDayGuid = '00000000-0000-0000-0000-000000000000')
                     OR (h.POSDayGuid = @POSDayGuid)
                   )
               AND (
                     (@POSSessionGuid = '00000000-0000-0000-0000-000000000000')
                     OR (h.POSSessionGuid = @POSSessionGuid)
                   )
               AND (
                     @UseDateFilter = 0
                     OR cast(h.InvoiceDate as date) BETWEEN cast(@Date1 as date) AND cast(@Date2 as date)
                   )";

        private SqlParameter[] BuildFilterParams(
            DateTime date1, DateTime date2, bool useDateFilter,
            int branchId, int cashId, int filterUserId,
            string posDayGuid, string posSessionGuid, int companyId)
        {
            return new[]
            {
                new SqlParameter("@Date1", SqlDbType.Date) { Value = date1.Date },
                new SqlParameter("@Date2", SqlDbType.Date) { Value = date2.Date },
                new SqlParameter("@UseDateFilter", SqlDbType.Bit) { Value = useDateFilter },
                new SqlParameter("@BranchID", SqlDbType.Int) { Value = branchId },
                new SqlParameter("@CashID", SqlDbType.Int) { Value = cashId },
                new SqlParameter("@FilterUserID", SqlDbType.Int) { Value = filterUserId },
                new SqlParameter("@POSDayGuid", SqlDbType.UniqueIdentifier) { Value = Simulate.Guid(posDayGuid) },
                new SqlParameter("@POSSessionGuid", SqlDbType.UniqueIdentifier) { Value = Simulate.Guid(posSessionGuid) },
                new SqlParameter("@CompanyID", SqlDbType.Int) { Value = companyId },
            };
        }

        /// <summary>
        /// X/Z summary: sales, refunds, discounts, payment mix, expected cash, saved variance.
        /// ReportType: X = live snapshot, Z = include closed day/session cash counts.
        /// </summary>
        public DataTable SelectXZReport(
            string reportType,
            DateTime date1, DateTime date2, bool useDateFilter,
            int branchId, int cashId, int filterUserId,
            string posDayGuid, string posSessionGuid, int companyId)
        {
            try
            {
                clsSQL clsSQL = new clsSQL();
                var prm = BuildFilterParams(date1, date2, useDateFilter, branchId, cashId,
                    filterUserId, posDayGuid, posSessionGuid, companyId);

                string sql = $@"
SELECT
  ISNULL(SUM(CASE WHEN h.InvoiceTypeID = 10 THEN 1 ELSE 0 END), 0) AS SalesCount,
  ISNULL(SUM(CASE WHEN h.InvoiceTypeID = 11 THEN 1 ELSE 0 END), 0) AS RefundCount,
  ISNULL(SUM(CASE WHEN h.InvoiceTypeID = 10 THEN h.TotalInvoice * -1 * j.QTYFactor ELSE 0 END), 0) AS SalesTotal,
  ISNULL(SUM(CASE WHEN h.InvoiceTypeID = 11 THEN h.TotalInvoice * -1 * j.QTYFactor ELSE 0 END), 0) AS RefundTotal,
  ISNULL(SUM(h.TotalInvoice * -1 * j.QTYFactor), 0) AS NetSales,
  ISNULL(SUM(h.TotalTax * -1 * j.QTYFactor), 0) AS TotalTax,
  ISNULL(SUM(h.TotalDiscount * -1 * j.QTYFactor), 0) AS TotalDiscount,
  ISNULL(SUM(h.HeaderDiscount * -1 * j.QTYFactor), 0) AS HeaderDiscount,
  ISNULL(SUM(CASE WHEN ISNULL(pm.IsCash, 0) = 1 THEN h.TotalInvoice * -1 * j.QTYFactor ELSE 0 END), 0) AS CashNet,
  ISNULL(SUM(CASE WHEN ISNULL(pm.IsBank, 0) = 1 THEN h.TotalInvoice * -1 * j.QTYFactor ELSE 0 END), 0) AS BankNet,
  ISNULL(SUM(CASE WHEN ISNULL(pm.IsDebit, 0) = 1 THEN h.TotalInvoice * -1 * j.QTYFactor ELSE 0 END), 0) AS DebitNet,
  ISNULL(SUM(CASE WHEN ISNULL(pm.IsCash, 0) = 0 AND ISNULL(pm.IsBank, 0) = 0 AND ISNULL(pm.IsDebit, 0) = 0
                   THEN h.TotalInvoice * -1 * j.QTYFactor ELSE 0 END), 0) AS OtherNet
FROM tbl_InvoiceHeader h
LEFT JOIN tbl_JournalVoucherTypes j ON j.ID = h.InvoiceTypeID
LEFT JOIN tbl_PaymentMethod pm ON pm.ID = h.PaymentMethodID
WHERE 1 = 1
{PosSalesFilter}";

                DataTable summary = clsSQL.ExecuteQueryStatement(sql,
                    clsSQL.CreateDataBaseConnectionString(companyId), prm);

                // Attach day/session cash-count fields when scoped
                decimal openingFloat = 0;
                decimal countedCash = 0;
                decimal expectedCashSaved = 0;
                decimal varianceSaved = 0;
                string closingNote = "";
                int status = -1;
                string scopeName = reportType ?? "X";

                if (!string.IsNullOrWhiteSpace(posSessionGuid) &&
                    Simulate.Guid(posSessionGuid) != Guid.Empty)
                {
                    SqlParameter[] sp =
                    {
                        new SqlParameter("@Guid", SqlDbType.UniqueIdentifier) { Value = Simulate.Guid(posSessionGuid) },
                        new SqlParameter("@CompanyID", SqlDbType.Int) { Value = companyId },
                    };
                    DataTable sess = clsSQL.ExecuteQueryStatement(
                        @"SELECT TOP 1 ISNULL(OpeningFloat,0) OpeningFloat, ISNULL(CountedCash,0) CountedCash,
                                 ISNULL(ExpectedCash,0) ExpectedCash, ISNULL(Variance,0) Variance,
                                 ISNULL(ClosingNote,'') ClosingNote, Status
                          FROM tbl_POSSessions WHERE Guid=@Guid AND (CompanyID=@CompanyID OR @CompanyID=0)",
                        clsSQL.CreateDataBaseConnectionString(companyId), sp);
                    if (sess.Rows.Count > 0)
                    {
                        openingFloat = Simulate.decimal_(sess.Rows[0]["OpeningFloat"]);
                        countedCash = Simulate.decimal_(sess.Rows[0]["CountedCash"]);
                        expectedCashSaved = Simulate.decimal_(sess.Rows[0]["ExpectedCash"]);
                        varianceSaved = Simulate.decimal_(sess.Rows[0]["Variance"]);
                        closingNote = Simulate.String(sess.Rows[0]["ClosingNote"]);
                        status = Simulate.Integer32(sess.Rows[0]["Status"]);
                        scopeName = "Session";
                    }
                }
                else if (!string.IsNullOrWhiteSpace(posDayGuid) &&
                         Simulate.Guid(posDayGuid) != Guid.Empty)
                {
                    SqlParameter[] sp =
                    {
                        new SqlParameter("@Guid", SqlDbType.UniqueIdentifier) { Value = Simulate.Guid(posDayGuid) },
                        new SqlParameter("@CompanyID", SqlDbType.Int) { Value = companyId },
                    };
                    DataTable day = clsSQL.ExecuteQueryStatement(
                        @"SELECT TOP 1 ISNULL(OpeningFloat,0) OpeningFloat, ISNULL(CountedCash,0) CountedCash,
                                 ISNULL(ExpectedCash,0) ExpectedCash, ISNULL(Variance,0) Variance,
                                 ISNULL(ClosingNote,'') ClosingNote, Status
                          FROM tbl_POSDay WHERE Guid=@Guid AND (CompanyID=@CompanyID OR @CompanyID=0)",
                        clsSQL.CreateDataBaseConnectionString(companyId), sp);
                    if (day.Rows.Count > 0)
                    {
                        openingFloat = Simulate.decimal_(day.Rows[0]["OpeningFloat"]);
                        countedCash = Simulate.decimal_(day.Rows[0]["CountedCash"]);
                        expectedCashSaved = Simulate.decimal_(day.Rows[0]["ExpectedCash"]);
                        varianceSaved = Simulate.decimal_(day.Rows[0]["Variance"]);
                        closingNote = Simulate.String(day.Rows[0]["ClosingNote"]);
                        status = Simulate.Integer32(day.Rows[0]["Status"]);
                        scopeName = "Day";
                    }
                }

                decimal cashNet = summary.Rows.Count > 0
                    ? Simulate.decimal_(summary.Rows[0]["CashNet"])
                    : 0;
                decimal expectedLive = openingFloat + cashNet;
                decimal varianceLive = countedCash - expectedLive;

                if (summary.Rows.Count == 0)
                {
                    summary.Rows.Add(summary.NewRow());
                }

                EnsureColumn(summary, "ReportType", typeof(string));
                EnsureColumn(summary, "Scope", typeof(string));
                EnsureColumn(summary, "OpeningFloat", typeof(decimal));
                EnsureColumn(summary, "CountedCash", typeof(decimal));
                EnsureColumn(summary, "ExpectedCash", typeof(decimal));
                EnsureColumn(summary, "ExpectedCashSaved", typeof(decimal));
                EnsureColumn(summary, "Variance", typeof(decimal));
                EnsureColumn(summary, "VarianceSaved", typeof(decimal));
                EnsureColumn(summary, "ClosingNote", typeof(string));
                EnsureColumn(summary, "Status", typeof(int));

                summary.Rows[0]["ReportType"] = string.IsNullOrWhiteSpace(reportType) ? "X" : reportType.ToUpperInvariant();
                summary.Rows[0]["Scope"] = scopeName;
                summary.Rows[0]["OpeningFloat"] = openingFloat;
                summary.Rows[0]["CountedCash"] = countedCash;
                summary.Rows[0]["ExpectedCash"] = expectedLive;
                summary.Rows[0]["ExpectedCashSaved"] = expectedCashSaved;
                summary.Rows[0]["Variance"] = varianceLive;
                summary.Rows[0]["VarianceSaved"] = varianceSaved;
                summary.Rows[0]["ClosingNote"] = closingNote;
                summary.Rows[0]["Status"] = status;

                return summary;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public DataTable SelectPaymentBreakdown(
            DateTime date1, DateTime date2, bool useDateFilter,
            int branchId, int cashId, int filterUserId,
            string posDayGuid, string posSessionGuid, int companyId)
        {
            try
            {
                clsSQL clsSQL = new clsSQL();
                var prm = BuildFilterParams(date1, date2, useDateFilter, branchId, cashId,
                    filterUserId, posDayGuid, posSessionGuid, companyId);

                string sql = $@"
SELECT
  ISNULL(pm.ID, 0) AS PaymentMethodID,
  ISNULL(pm.AName, N'Unknown') AS PaymentMethod,
  ISNULL(pm.IsCash, 0) AS IsCash,
  ISNULL(pm.IsBank, 0) AS IsBank,
  ISNULL(pm.IsDebit, 0) AS IsDebit,
  COUNT(h.InvoiceNo) AS InvoiceCount,
  SUM(h.TotalInvoice * -1 * j.QTYFactor) AS NetTotal
FROM tbl_InvoiceHeader h
LEFT JOIN tbl_JournalVoucherTypes j ON j.ID = h.InvoiceTypeID
LEFT JOIN tbl_PaymentMethod pm ON pm.ID = h.PaymentMethodID
WHERE 1 = 1
{PosSalesFilter}
GROUP BY pm.ID, pm.AName, pm.IsCash, pm.IsBank, pm.IsDebit
ORDER BY NetTotal DESC";

                return clsSQL.ExecuteQueryStatement(sql,
                    clsSQL.CreateDataBaseConnectionString(companyId), prm);
            }
            catch (Exception)
            {
                throw;
            }
        }

        public DataTable SelectSalesByCashier(
            DateTime date1, DateTime date2, bool useDateFilter,
            int branchId, int cashId, int filterUserId,
            string posDayGuid, string posSessionGuid, int companyId)
        {
            try
            {
                clsSQL clsSQL = new clsSQL();
                var prm = BuildFilterParams(date1, date2, useDateFilter, branchId, cashId,
                    filterUserId, posDayGuid, posSessionGuid, companyId);

                string sql = $@"
SELECT
  ISNULL(h.CreationUserID, 0) AS CashierID,
  ISNULL(NULLIF(LTRIM(RTRIM(e.AName)), ''), ISNULL(e.EName, N'User ' + CAST(ISNULL(h.CreationUserID,0) AS NVARCHAR(20)))) AS CashierName,
  SUM(CASE WHEN h.InvoiceTypeID = 10 THEN 1 ELSE 0 END) AS SalesCount,
  SUM(CASE WHEN h.InvoiceTypeID = 11 THEN 1 ELSE 0 END) AS RefundCount,
  SUM(CASE WHEN h.InvoiceTypeID = 10 THEN h.TotalInvoice * -1 * j.QTYFactor ELSE 0 END) AS SalesTotal,
  SUM(CASE WHEN h.InvoiceTypeID = 11 THEN h.TotalInvoice * -1 * j.QTYFactor ELSE 0 END) AS RefundTotal,
  SUM(h.TotalInvoice * -1 * j.QTYFactor) AS NetSales,
  SUM(h.TotalDiscount * -1 * j.QTYFactor) AS TotalDiscount,
  SUM(h.TotalTax * -1 * j.QTYFactor) AS TotalTax
FROM tbl_InvoiceHeader h
LEFT JOIN tbl_JournalVoucherTypes j ON j.ID = h.InvoiceTypeID
LEFT JOIN tbl_employee e ON e.ID = h.CreationUserID
WHERE 1 = 1
{PosSalesFilter}
GROUP BY h.CreationUserID, e.AName, e.EName
ORDER BY NetSales DESC";

                return clsSQL.ExecuteQueryStatement(sql,
                    clsSQL.CreateDataBaseConnectionString(companyId), prm);
            }
            catch (Exception)
            {
                throw;
            }
        }

        public DataTable SelectSalesByHour(
            DateTime date1, DateTime date2, bool useDateFilter,
            int branchId, int cashId, int filterUserId,
            string posDayGuid, string posSessionGuid, int companyId)
        {
            try
            {
                clsSQL clsSQL = new clsSQL();
                var prm = BuildFilterParams(date1, date2, useDateFilter, branchId, cashId,
                    filterUserId, posDayGuid, posSessionGuid, companyId);

                string sql = $@"
SELECT
  DATEPART(HOUR, h.InvoiceDate) AS SaleHour,
  RIGHT('0' + CAST(DATEPART(HOUR, h.InvoiceDate) AS VARCHAR(2)), 2) + N':00' AS HourLabel,
  SUM(CASE WHEN h.InvoiceTypeID = 10 THEN 1 ELSE 0 END) AS SalesCount,
  SUM(CASE WHEN h.InvoiceTypeID = 11 THEN 1 ELSE 0 END) AS RefundCount,
  SUM(h.TotalInvoice * -1 * j.QTYFactor) AS NetSales,
  SUM(h.TotalDiscount * -1 * j.QTYFactor) AS TotalDiscount
FROM tbl_InvoiceHeader h
LEFT JOIN tbl_JournalVoucherTypes j ON j.ID = h.InvoiceTypeID
WHERE 1 = 1
{PosSalesFilter}
GROUP BY DATEPART(HOUR, h.InvoiceDate)
ORDER BY SaleHour";

                return clsSQL.ExecuteQueryStatement(sql,
                    clsSQL.CreateDataBaseConnectionString(companyId), prm);
            }
            catch (Exception)
            {
                throw;
            }
        }

        public DataTable SelectSalesByCategory(
            DateTime date1, DateTime date2, bool useDateFilter,
            int branchId, int cashId, int filterUserId,
            string posDayGuid, string posSessionGuid, int companyId)
        {
            try
            {
                clsSQL clsSQL = new clsSQL();
                var prm = BuildFilterParams(date1, date2, useDateFilter, branchId, cashId,
                    filterUserId, posDayGuid, posSessionGuid, companyId);

                string sql = $@"
SELECT
  ISNULL(c.ID, 0) AS CategoryID,
  ISNULL(NULLIF(LTRIM(RTRIM(c.AName)), ''), ISNULL(c.EName, N'Uncategorized')) AS CategoryName,
  COUNT(DISTINCT h.Guid) AS InvoiceCount,
  SUM(d.Qty * -1 * j.QTYFactor) AS QtySold,
  SUM(d.TotalLine * -1 * j.QTYFactor) AS NetSales,
  SUM(ISNULL(d.DiscountAfterTaxAmountAll, 0) * -1 * j.QTYFactor
    + ISNULL(d.DiscountBeforeTaxAmountAll, 0) * -1 * j.QTYFactor) AS TotalDiscount
FROM tbl_InvoiceHeader h
INNER JOIN tbl_InvoiceDetails d ON d.HeaderGuid = h.Guid
LEFT JOIN tbl_Items i ON i.Guid = d.ItemGuid
LEFT JOIN tbl_ItemsCategory c ON c.ID = i.CategoryID
LEFT JOIN tbl_JournalVoucherTypes j ON j.ID = h.InvoiceTypeID
WHERE 1 = 1
{PosSalesFilter}
GROUP BY c.ID, c.AName, c.EName
ORDER BY NetSales DESC";

                return clsSQL.ExecuteQueryStatement(sql,
                    clsSQL.CreateDataBaseConnectionString(companyId), prm);
            }
            catch (Exception)
            {
                throw;
            }
        }

        /// <summary>
        /// Refund invoices, discounted invoices, and POS audit log events (voids/clears).
        /// </summary>
        public DataTable SelectAuditReport(
            DateTime date1, DateTime date2, bool useDateFilter,
            int branchId, int cashId, int filterUserId,
            string posDayGuid, string posSessionGuid, int companyId)
        {
            try
            {
                clsSQL clsSQL = new clsSQL();
                var prm = BuildFilterParams(date1, date2, useDateFilter, branchId, cashId,
                    filterUserId, posDayGuid, posSessionGuid, companyId);

                string sql = $@"
SELECT * FROM (
  SELECT
    N'Refund' AS EventType,
    h.InvoiceDate AS EventDate,
    h.InvoiceNo AS Reference,
    ISNULL(e.AName, ISNULL(e.EName, CAST(h.CreationUserID AS NVARCHAR(20)))) AS CashierName,
    h.CreationUserID AS CashierID,
    ISNULL(pm.AName, N'') AS PaymentMethod,
    h.TotalInvoice * -1 * j.QTYFactor AS Amount,
    h.TotalDiscount * -1 * j.QTYFactor AS DiscountAmount,
    CAST(h.Guid AS NVARCHAR(50)) AS Details
  FROM tbl_InvoiceHeader h
  LEFT JOIN tbl_JournalVoucherTypes j ON j.ID = h.InvoiceTypeID
  LEFT JOIN tbl_PaymentMethod pm ON pm.ID = h.PaymentMethodID
  LEFT JOIN tbl_employee e ON e.ID = h.CreationUserID
  WHERE h.InvoiceTypeID = 11
  {PosSalesFilter}

  UNION ALL

  SELECT
    N'Discount' AS EventType,
    h.InvoiceDate AS EventDate,
    h.InvoiceNo AS Reference,
    ISNULL(e.AName, ISNULL(e.EName, CAST(h.CreationUserID AS NVARCHAR(20)))) AS CashierName,
    h.CreationUserID AS CashierID,
    ISNULL(pm.AName, N'') AS PaymentMethod,
    h.TotalInvoice * -1 * j.QTYFactor AS Amount,
    (ISNULL(h.TotalDiscount,0) + ISNULL(h.HeaderDiscount,0)) * -1 * j.QTYFactor AS DiscountAmount,
    CAST(h.Guid AS NVARCHAR(50)) AS Details
  FROM tbl_InvoiceHeader h
  LEFT JOIN tbl_JournalVoucherTypes j ON j.ID = h.InvoiceTypeID
  LEFT JOIN tbl_PaymentMethod pm ON pm.ID = h.PaymentMethodID
  LEFT JOIN tbl_employee e ON e.ID = h.CreationUserID
  WHERE h.InvoiceTypeID = 10
    AND (ISNULL(h.TotalDiscount,0) + ISNULL(h.HeaderDiscount,0)) > 0
  {PosSalesFilter}

  UNION ALL

  SELECT
    a.EventType,
    a.CreationDate AS EventDate,
    ISNULL(a.InvoiceNo, N'') AS Reference,
    ISNULL(e.AName, ISNULL(e.EName, CAST(a.CreationUserID AS NVARCHAR(20)))) AS CashierName,
    a.CreationUserID AS CashierID,
    N'' AS PaymentMethod,
    ISNULL(a.Amount, 0) AS Amount,
    0 AS DiscountAmount,
    ISNULL(a.Details, N'') AS Details
  FROM tbl_POSAuditLog a
  LEFT JOIN tbl_employee e ON e.ID = a.CreationUserID
  WHERE (a.CompanyID = @CompanyID OR @CompanyID = 0)
    AND (a.CashDrawerID = @CashID OR @CashID = 0)
    AND (a.CreationUserID = @FilterUserID OR @FilterUserID = 0)
    AND (
          (@POSDayGuid = '00000000-0000-0000-0000-000000000000')
          OR (a.POSDayGuid = @POSDayGuid)
        )
    AND (
          (@POSSessionGuid = '00000000-0000-0000-0000-000000000000')
          OR (a.POSSessionGuid = @POSSessionGuid)
        )
    AND (
          @UseDateFilter = 0
          OR cast(a.CreationDate as date) BETWEEN cast(@Date1 as date) AND cast(@Date2 as date)
        )
) x
ORDER BY EventDate DESC";

                return clsSQL.ExecuteQueryStatement(sql,
                    clsSQL.CreateDataBaseConnectionString(companyId), prm);
            }
            catch (Exception)
            {
                throw;
            }
        }

        public bool SaveCashCount(
            string scope, string guid, decimal openingFloat, decimal countedCash,
            decimal expectedCash, string closingNote, int modificationUserId, int companyId)
        {
            try
            {
                clsSQL clsSQL = new clsSQL();
                decimal variance = countedCash - expectedCash;
                SqlParameter[] prm =
                {
                    new SqlParameter("@Guid", SqlDbType.UniqueIdentifier) { Value = Simulate.Guid(guid) },
                    new SqlParameter("@OpeningFloat", SqlDbType.Decimal) { Value = openingFloat },
                    new SqlParameter("@CountedCash", SqlDbType.Decimal) { Value = countedCash },
                    new SqlParameter("@ExpectedCash", SqlDbType.Decimal) { Value = expectedCash },
                    new SqlParameter("@Variance", SqlDbType.Decimal) { Value = variance },
                    new SqlParameter("@ClosingNote", SqlDbType.NVarChar) { Value = closingNote ?? "" },
                    new SqlParameter("@ModificationUserId", SqlDbType.Int) { Value = modificationUserId },
                    new SqlParameter("@ModificationDate", SqlDbType.DateTime) { Value = DateTime.Now },
                };

                string table = string.Equals(scope, "Session", StringComparison.OrdinalIgnoreCase)
                    ? "tbl_POSSessions"
                    : "tbl_POSDay";

                string sql = $@"UPDATE {table} SET
                    OpeningFloat=@OpeningFloat,
                    CountedCash=@CountedCash,
                    ExpectedCash=@ExpectedCash,
                    Variance=@Variance,
                    ClosingNote=@ClosingNote,
                    ModificationUserId=@ModificationUserId,
                    ModificationDate=@ModificationDate
                  WHERE Guid=@Guid";

                clsSQL.ExecuteNonQueryStatement(sql,
                    clsSQL.CreateDataBaseConnectionString(companyId), prm);
                return true;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public bool LogAuditEvent(
            string eventType, string invoiceGuid, string invoiceNo,
            int cashDrawerId, string posDayGuid, string posSessionGuid,
            decimal amount, string details, int creationUserId, int companyId)
        {
            try
            {
                clsSQL clsSQL = new clsSQL();
                SqlParameter[] prm =
                {
                    new SqlParameter("@EventType", SqlDbType.NVarChar, 50) { Value = eventType ?? "" },
                    new SqlParameter("@InvoiceGuid", SqlDbType.UniqueIdentifier)
                    {
                        Value = (string.IsNullOrWhiteSpace(invoiceGuid) ||
                                 Simulate.Guid(invoiceGuid) == Guid.Empty)
                            ? (object)DBNull.Value
                            : Simulate.Guid(invoiceGuid)
                    },
                    new SqlParameter("@InvoiceNo", SqlDbType.NVarChar, 100) { Value = invoiceNo ?? "" },
                    new SqlParameter("@CashDrawerID", SqlDbType.Int) { Value = cashDrawerId },
                    new SqlParameter("@POSDayGuid", SqlDbType.UniqueIdentifier)
                    {
                        Value = (string.IsNullOrWhiteSpace(posDayGuid) ||
                                 Simulate.Guid(posDayGuid) == Guid.Empty)
                            ? (object)DBNull.Value
                            : Simulate.Guid(posDayGuid)
                    },
                    new SqlParameter("@POSSessionGuid", SqlDbType.UniqueIdentifier)
                    {
                        Value = (string.IsNullOrWhiteSpace(posSessionGuid) ||
                                 Simulate.Guid(posSessionGuid) == Guid.Empty)
                            ? (object)DBNull.Value
                            : Simulate.Guid(posSessionGuid)
                    },
                    new SqlParameter("@Amount", SqlDbType.Decimal) { Value = amount },
                    new SqlParameter("@Details", SqlDbType.NVarChar) { Value = details ?? "" },
                    new SqlParameter("@CreationUserID", SqlDbType.Int) { Value = creationUserId },
                    new SqlParameter("@CreationDate", SqlDbType.DateTime) { Value = DateTime.Now },
                    new SqlParameter("@CompanyID", SqlDbType.Int) { Value = companyId },
                };

                string sql = @"INSERT INTO tbl_POSAuditLog
                    (EventType, InvoiceGuid, InvoiceNo, CashDrawerID, POSDayGuid, POSSessionGuid,
                     Amount, Details, CreationUserID, CreationDate, CompanyID)
                    VALUES
                    (@EventType, @InvoiceGuid, @InvoiceNo, @CashDrawerID, @POSDayGuid, @POSSessionGuid,
                     @Amount, @Details, @CreationUserID, @CreationDate, @CompanyID)";

                clsSQL.ExecuteNonQueryStatement(sql,
                    clsSQL.CreateDataBaseConnectionString(companyId), prm);
                return true;
            }
            catch (Exception)
            {
                throw;
            }
        }

        private static void EnsureColumn(DataTable dt, string name, Type type)
        {
            if (!dt.Columns.Contains(name))
                dt.Columns.Add(name, type);
        }
    }
}
