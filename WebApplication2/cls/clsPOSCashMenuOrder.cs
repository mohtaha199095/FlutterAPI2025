using System;
using System.Data;
using System.Text;
using Microsoft.Data.SqlClient;

namespace WebApplication2.cls
{
    /// <summary>
    /// Per cash-drawer POS category/item display order.
    /// When a drawer has rows here, that order follows the drawer on any PC;
    /// otherwise the client falls back to company-wide POSOrder on items/categories.
    /// </summary>
    public class clsPOSCashMenuOrder
    {
        public const int KindCategory = 1;
        public const int KindItem = 2;

        public DataTable SelectByCashDrawer(int cashDrawerID, int companyID, int kind = 0)
        {
            clsSQL clsSQL = new clsSQL();
            SqlParameter[] prm =
            {
                new SqlParameter("@CashDrawerID", SqlDbType.Int) { Value = cashDrawerID },
                new SqlParameter("@CompanyID", SqlDbType.Int) { Value = companyID },
                new SqlParameter("@Kind", SqlDbType.Int) { Value = kind },
            };
            return clsSQL.ExecuteQueryStatement(
                @"SELECT CashDrawerID, Kind, RefKey, POSOrder
                  FROM tbl_POSCashMenuOrder
                  WHERE CashDrawerID=@CashDrawerID AND CompanyID=@CompanyID
                    AND (Kind=@Kind OR @Kind=0)
                  ORDER BY Kind, POSOrder, RefKey",
                clsSQL.CreateDataBaseConnectionString(companyID), prm);
        }

        public bool ReplaceOrder(
            int cashDrawerID,
            int kind,
            string orderedKeysCsv,
            int companyID,
            int modificationUserID)
        {
            if (cashDrawerID <= 0 || kind <= 0) return false;
            if (string.IsNullOrWhiteSpace(orderedKeysCsv)) return false;

            string[] keys = orderedKeysCsv.Split(',', StringSplitOptions.RemoveEmptyEntries);
            clsSQL clsSQL = new clsSQL();
            string conn = clsSQL.CreateDataBaseConnectionString(companyID);

            using (SqlConnection con = new SqlConnection(conn))
            {
                con.Open();
                using (SqlTransaction trn = con.BeginTransaction())
                {
                    SqlParameter[] clearPrm =
                    {
                        new SqlParameter("@CashDrawerID", SqlDbType.Int) { Value = cashDrawerID },
                        new SqlParameter("@CompanyID", SqlDbType.Int) { Value = companyID },
                        new SqlParameter("@Kind", SqlDbType.Int) { Value = kind },
                    };
                    clsSQL.ExecuteNonQueryStatement(
                        @"DELETE FROM tbl_POSCashMenuOrder
                          WHERE CashDrawerID=@CashDrawerID AND CompanyID=@CompanyID AND Kind=@Kind",
                        conn, clearPrm, trn);

                    // Batch inserts (chunks) — one-row-at-a-time was too slow for large menus.
                    const int chunkSize = 80;
                    for (int start = 0; start < keys.Length; start += chunkSize)
                    {
                        int end = Math.Min(start + chunkSize, keys.Length);
                        var sql = new StringBuilder();
                        sql.Append(@"INSERT INTO tbl_POSCashMenuOrder
                            (CashDrawerID, Kind, RefKey, POSOrder, CompanyID, ModificationUserId, ModificationDate) VALUES ");
                        var prmList = new System.Collections.Generic.List<SqlParameter>
                        {
                            new SqlParameter("@CashDrawerID", SqlDbType.Int) { Value = cashDrawerID },
                            new SqlParameter("@Kind", SqlDbType.Int) { Value = kind },
                            new SqlParameter("@CompanyID", SqlDbType.Int) { Value = companyID },
                            new SqlParameter("@ModificationUserId", SqlDbType.Int) { Value = modificationUserID },
                            new SqlParameter("@ModificationDate", SqlDbType.DateTime) { Value = DateTime.Now },
                        };

                        int valueIndex = 0;
                        for (int i = start; i < end; i++)
                        {
                            string key = keys[i].Trim();
                            if (string.IsNullOrWhiteSpace(key)) continue;
                            if (valueIndex > 0) sql.Append(',');
                            sql.Append($"(@CashDrawerID,@Kind,@RefKey{valueIndex},@POSOrder{valueIndex},@CompanyID,@ModificationUserId,@ModificationDate)");
                            prmList.Add(new SqlParameter($"@RefKey{valueIndex}", SqlDbType.NVarChar, 50) { Value = key });
                            prmList.Add(new SqlParameter($"@POSOrder{valueIndex}", SqlDbType.Int) { Value = i + 1 });
                            valueIndex++;
                        }
                        if (valueIndex == 0) continue;
                        clsSQL.ExecuteNonQueryStatement(sql.ToString(), conn, prmList.ToArray(), trn);
                    }

                    trn.Commit();
                }
            }
            return true;
        }

        /// <summary>
        /// Copies category + item POS order from one cash drawer to another.
        /// Returns false if source has no saved order.
        /// </summary>
        public bool CopyOrder(
            int fromCashDrawerID,
            int toCashDrawerID,
            int companyID,
            int modificationUserID)
        {
            if (fromCashDrawerID <= 0 || toCashDrawerID <= 0) return false;
            if (fromCashDrawerID == toCashDrawerID) return true;

            DataTable source = SelectByCashDrawer(fromCashDrawerID, companyID);
            if (source == null || source.Rows.Count == 0) return false;

            // Rebuild as CSV per kind, then ReplaceOrder (batched).
            var categories = new StringBuilder();
            var items = new StringBuilder();
            DataView view = source.DefaultView;
            view.Sort = "Kind ASC, POSOrder ASC";
            foreach (DataRowView row in view)
            {
                int kind = Simulate.Integer32(row["Kind"]);
                string key = Simulate.String(row["RefKey"]);
                if (string.IsNullOrWhiteSpace(key)) continue;
                if (kind == KindCategory)
                {
                    if (categories.Length > 0) categories.Append(',');
                    categories.Append(key);
                }
                else if (kind == KindItem)
                {
                    if (items.Length > 0) items.Append(',');
                    items.Append(key);
                }
            }

            bool any = false;
            if (categories.Length > 0)
            {
                any = ReplaceOrder(toCashDrawerID, KindCategory, categories.ToString(), companyID, modificationUserID) || any;
            }
            if (items.Length > 0)
            {
                any = ReplaceOrder(toCashDrawerID, KindItem, items.ToString(), companyID, modificationUserID) || any;
            }
            return any;
        }
    }
}
