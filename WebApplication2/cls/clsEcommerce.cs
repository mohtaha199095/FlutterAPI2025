using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Text;

namespace WebApplication2.cls
{
    public class clsEcommerce
    {
        public class ShopInfo
        {
            public int CompanyID { get; set; }
            public string TradeName { get; set; } = "";
            public string AName { get; set; } = "";
            public string EName { get; set; } = "";
            public string WebSlug { get; set; } = "";
            public string Email { get; set; } = "";
            public string Address { get; set; } = "";
            public string Tel1 { get; set; } = "";
            public string Tel2 { get; set; } = "";
            public string ContactPerson { get; set; } = "";
            public byte[] Logo { get; set; }
        }

        public class OrderLineInput
        {
            public string ItemGuid { get; set; } = "";
            public decimal Qty { get; set; }
            public string Size { get; set; } = "";
            public string Color { get; set; } = "";
            public string LineNote { get; set; } = "";
        }

        static string NormalizeSlug(string slug)
        {
            if (string.IsNullOrWhiteSpace(slug)) return "";
            string s = slug.Trim().ToLowerInvariant();
            var sb = new StringBuilder(s.Length);
            foreach (char c in s)
            {
                if ((c >= 'a' && c <= 'z') || (c >= '0' && c <= '9') || c == '-')
                    sb.Append(c);
            }
            return sb.ToString();
        }

        /// <summary>Resolve enabled shop from main DB by WebSlug, then enrich from tenant.</summary>
        public ShopInfo GetShopBySlug(string slug)
        {
            string normalized = NormalizeSlug(slug);
            if (string.IsNullOrEmpty(normalized)) return null;

            clsSQL sql = new clsSQL();
            SqlParameter[] prm =
            {
                new SqlParameter("@Slug", SqlDbType.NVarChar, 80) { Value = normalized },
            };
            DataTable dt = sql.ExecuteQueryStatement(@"
SELECT TOP 1 ID, AName, EName, TradeName, Email, Address, Tel1, Tel2, ContactPerson, Logo, WebSlug,
       ISNULL(EnableEcommerce, 0) AS EnableEcommerce
FROM tbl_Company
WHERE LOWER(LTRIM(RTRIM(ISNULL(WebSlug, '')))) = @Slug
  AND ISNULL(EnableEcommerce, 0) = 1",
                sql.MainDataBaseconString, prm);

            if (dt == null || dt.Rows.Count == 0) return null;

            DataRow r = dt.Rows[0];
            var shop = new ShopInfo
            {
                CompanyID = Simulate.Integer32(r["ID"]),
                AName = Simulate.String(r["AName"]),
                EName = Simulate.String(r["EName"]),
                TradeName = Simulate.String(r["TradeName"]),
                Email = Simulate.String(r["Email"]),
                Address = Simulate.String(r["Address"]),
                Tel1 = Simulate.String(r["Tel1"]),
                Tel2 = Simulate.String(r["Tel2"]),
                ContactPerson = Simulate.String(r["ContactPerson"]),
                WebSlug = Simulate.String(r["WebSlug"]),
                Logo = r["Logo"] == DBNull.Value ? null : (byte[])r["Logo"],
            };

            // Prefer richer branding from the tenant company row (logo often lives there).
            try
            {
                if (shop.CompanyID > 0)
                {
                    DataTable tenant = sql.ExecuteQueryStatement(@"
SELECT TOP 1 AName, EName, TradeName, Email, Address, Tel1, Tel2, ContactPerson, Logo
FROM tbl_Company
ORDER BY CASE WHEN ID = @ID THEN 0 ELSE 1 END, ID",
                        sql.CreateDataBaseConnectionString(shop.CompanyID),
                        new[] { new SqlParameter("@ID", SqlDbType.Int) { Value = shop.CompanyID } });

                    if (tenant != null && tenant.Rows.Count > 0)
                    {
                        DataRow t = tenant.Rows[0];
                        string trade = Simulate.String(t["TradeName"]);
                        if (!string.IsNullOrWhiteSpace(trade)) shop.TradeName = trade;
                        string aName = Simulate.String(t["AName"]);
                        if (!string.IsNullOrWhiteSpace(aName)) shop.AName = aName;
                        string eName = Simulate.String(t["EName"]);
                        if (!string.IsNullOrWhiteSpace(eName)) shop.EName = eName;
                        string email = Simulate.String(t["Email"]);
                        if (!string.IsNullOrWhiteSpace(email)) shop.Email = email;
                        string address = Simulate.String(t["Address"]);
                        if (!string.IsNullOrWhiteSpace(address)) shop.Address = address;
                        string tel1 = Simulate.String(t["Tel1"]);
                        if (!string.IsNullOrWhiteSpace(tel1)) shop.Tel1 = tel1;
                        string tel2 = Simulate.String(t["Tel2"]);
                        if (!string.IsNullOrWhiteSpace(tel2)) shop.Tel2 = tel2;
                        string contact = Simulate.String(t["ContactPerson"]);
                        if (!string.IsNullOrWhiteSpace(contact)) shop.ContactPerson = contact;
                        if (t["Logo"] != DBNull.Value)
                        {
                            byte[] logo = (byte[])t["Logo"];
                            if (logo != null && logo.Length > 0) shop.Logo = logo;
                        }
                    }
                }
            }
            catch { /* best-effort enrichment */ }

            return shop;
        }

        public DataTable GetCatalog(int companyId)
        {
            clsSQL sql = new clsSQL();
            SqlParameter[] prm =
            {
                new SqlParameter("@CompanyID", SqlDbType.Int) { Value = companyId },
            };
            return sql.ExecuteQueryStatement(@"
SELECT
    i.Guid,
    i.AName,
    i.EName,
    i.Description,
    i.Picture,
    ISNULL(i.CategoryID, 0) AS CategoryID,
    ISNULL(c.AName, N'') AS CategoryAName,
    ISNULL(c.EName, N'') AS CategoryEName,
    CASE
        WHEN ISNULL(i.WebPrice, 0) > 0 THEN i.WebPrice
        ELSE ISNULL(i.SalesPriceAfterTax, 0)
    END AS WebPrice,
    ISNULL(i.WebAllowCustomNote, 0) AS WebAllowCustomNote,
    ISNULL(i.WebHasSize, 0) AS WebHasSize,
    ISNULL(i.WebHasColor, 0) AS WebHasColor,
    ISNULL(i.WebSizeOptions, N'') AS WebSizeOptions,
    ISNULL(i.WebColorOptions, N'') AS WebColorOptions
FROM tbl_Items i
LEFT JOIN tbl_ItemsCategory c
  ON c.ID = i.CategoryID
 AND (c.CompanyID = i.CompanyID OR c.CompanyID = 0 OR @CompanyID = 0)
WHERE ISNULL(i.ShowOnWeb, 0) = 1
  AND ISNULL(i.IsActive, 1) = 1
  AND (i.CompanyID = @CompanyID OR @CompanyID = 0)
ORDER BY i.AName, i.EName",
                sql.CreateDataBaseConnectionString(companyId), prm);
        }

        public DataTable SelectOrders(int companyId, string statusFilter)
        {
            clsSQL sql = new clsSQL();
            SqlParameter[] prm =
            {
                new SqlParameter("@CompanyID", SqlDbType.Int) { Value = companyId },
                new SqlParameter("@Status", SqlDbType.NVarChar, 20) { Value = statusFilter ?? "" },
            };
            return sql.ExecuteQueryStatement(@"
SELECT ID, Guid, OrderNo, CustomerName, Phone, Address, Notes, Status, Total, CreatedAt, CompanyID
FROM tbl_EcommerceOrder
WHERE (CompanyID = @CompanyID OR @CompanyID = 0)
  AND (
        @Status = N''
     OR Status = @Status
     OR (@Status = N'Due' AND Status IN (N'New', N'Seen'))
  )
ORDER BY CreatedAt DESC",
                sql.CreateDataBaseConnectionString(companyId), prm);
        }

        public DataTable SelectOrderById(int companyId, int orderId)
        {
            clsSQL sql = new clsSQL();
            SqlParameter[] prm =
            {
                new SqlParameter("@CompanyID", SqlDbType.Int) { Value = companyId },
                new SqlParameter("@ID", SqlDbType.Int) { Value = orderId },
            };
            return sql.ExecuteQueryStatement(@"
SELECT ID, Guid, OrderNo, CustomerName, Phone, Address, Notes, Status, Total, CreatedAt, CompanyID
FROM tbl_EcommerceOrder
WHERE ID = @ID AND (CompanyID = @CompanyID OR @CompanyID = 0)",
                sql.CreateDataBaseConnectionString(companyId), prm);
        }

        public DataTable SelectOrderLines(int companyId, int orderId)
        {
            clsSQL sql = new clsSQL();
            SqlParameter[] prm =
            {
                new SqlParameter("@CompanyID", SqlDbType.Int) { Value = companyId },
                new SqlParameter("@OrderID", SqlDbType.Int) { Value = orderId },
            };
            return sql.ExecuteQueryStatement(@"
SELECT ID, OrderID, ItemGuid, ItemName, Qty, UnitPrice, LineTotal,
       ISNULL(Size, N'') AS Size,
       ISNULL(Color, N'') AS Color,
       ISNULL(LineNote, N'') AS LineNote
FROM tbl_EcommerceOrderLine
WHERE OrderID = @OrderID AND (CompanyID = @CompanyID OR @CompanyID = 0)
ORDER BY ID",
                sql.CreateDataBaseConnectionString(companyId), prm);
        }

        public int MarkOrderSeen(int companyId, int orderId)
        {
            clsSQL sql = new clsSQL();
            SqlParameter[] prm =
            {
                new SqlParameter("@CompanyID", SqlDbType.Int) { Value = companyId },
                new SqlParameter("@ID", SqlDbType.Int) { Value = orderId },
            };
            return sql.ExecuteNonQueryStatement(@"
UPDATE tbl_EcommerceOrder SET Status = N'Seen'
WHERE ID = @ID AND (CompanyID = @CompanyID OR @CompanyID = 0)
  AND Status = N'New'",
                sql.CreateDataBaseConnectionString(companyId), prm);
        }

        /// <summary>
        /// Marks order as Progressed (done) or reopens it to Seen when progressed=false.
        /// </summary>
        public int SetOrderProgressed(int companyId, int orderId, bool progressed)
        {
            clsSQL sql = new clsSQL();
            SqlParameter[] prm =
            {
                new SqlParameter("@CompanyID", SqlDbType.Int) { Value = companyId },
                new SqlParameter("@ID", SqlDbType.Int) { Value = orderId },
                new SqlParameter("@Status", SqlDbType.NVarChar, 20)
                {
                    Value = progressed ? "Progressed" : "Seen"
                },
            };
            return sql.ExecuteNonQueryStatement(@"
UPDATE tbl_EcommerceOrder SET Status = @Status
WHERE ID = @ID AND (CompanyID = @CompanyID OR @CompanyID = 0)",
                sql.CreateDataBaseConnectionString(companyId), prm);
        }

        public (string OrderNo, int OrderId, string CompanyEmail, string ShopName) PlaceOrder(
            string slug,
            string customerName,
            string phone,
            string address,
            string notes,
            List<OrderLineInput> lines)
        {
            ShopInfo shop = GetShopBySlug(slug);
            if (shop == null || shop.CompanyID <= 0)
                throw new InvalidOperationException("Shop not found or e-commerce is disabled.");

            if (string.IsNullOrWhiteSpace(customerName) || string.IsNullOrWhiteSpace(phone) || string.IsNullOrWhiteSpace(address))
                throw new InvalidOperationException("Customer name, phone and address are required.");

            if (lines == null || lines.Count == 0)
                throw new InvalidOperationException("Cart is empty.");

            int companyId = shop.CompanyID;
            clsSQL sql = new clsSQL();
            string con = sql.CreateDataBaseConnectionString(companyId);

            var resolved = new List<(Guid itemGuid, string name, decimal qty, decimal unitPrice, decimal lineTotal, string size, string color, string lineNote)>();
            decimal total = 0;

            foreach (var line in lines)
            {
                if (line == null || line.Qty <= 0) continue;
                Guid itemGuid;
                if (!Guid.TryParse(line.ItemGuid, out itemGuid)) continue;

                SqlParameter[] itemPrm =
                {
                    new SqlParameter("@Guid", SqlDbType.UniqueIdentifier) { Value = itemGuid },
                    new SqlParameter("@CompanyID", SqlDbType.Int) { Value = companyId },
                };
                DataTable itemDt = sql.ExecuteQueryStatement(@"
SELECT TOP 1 Guid, AName, EName,
    CASE WHEN ISNULL(WebPrice, 0) > 0 THEN WebPrice ELSE ISNULL(SalesPriceAfterTax, 0) END AS UnitPrice,
    ISNULL(WebAllowCustomNote, 0) AS WebAllowCustomNote,
    ISNULL(WebHasSize, 0) AS WebHasSize,
    ISNULL(WebHasColor, 0) AS WebHasColor
FROM tbl_Items
WHERE Guid = @Guid
  AND ISNULL(ShowOnWeb, 0) = 1
  AND ISNULL(IsActive, 1) = 1
  AND (CompanyID = @CompanyID OR @CompanyID = 0)", con, itemPrm);

                if (itemDt == null || itemDt.Rows.Count == 0) continue;

                DataRow ir = itemDt.Rows[0];
                decimal unit = Simulate.decimal_(ir["UnitPrice"]);
                decimal lineTotal = Math.Round(unit * line.Qty, 3);
                string name = Simulate.String(ir["AName"]);
                if (string.IsNullOrWhiteSpace(name)) name = Simulate.String(ir["EName"]);
                string size = Simulate.Bool(ir["WebHasSize"]) ? (line.Size ?? "").Trim() : "";
                string color = Simulate.Bool(ir["WebHasColor"]) ? (line.Color ?? "").Trim() : "";
                string lineNote = Simulate.Bool(ir["WebAllowCustomNote"]) ? (line.LineNote ?? "").Trim() : "";
                resolved.Add((itemGuid, name, line.Qty, unit, lineTotal, size, color, lineNote));
                total += lineTotal;
            }

            if (resolved.Count == 0)
                throw new InvalidOperationException("No valid items in cart.");

            string orderGuid = Guid.NewGuid().ToString();
            string orderNo = "WEB-" + DateTime.Now.ToString("yyyyMMddHHmmss");

            SqlParameter[] orderPrm =
            {
                new SqlParameter("@Guid", SqlDbType.UniqueIdentifier) { Value = Guid.Parse(orderGuid) },
                new SqlParameter("@OrderNo", SqlDbType.NVarChar, 40) { Value = orderNo },
                new SqlParameter("@CustomerName", SqlDbType.NVarChar, 200) { Value = customerName.Trim() },
                new SqlParameter("@Phone", SqlDbType.NVarChar, 50) { Value = phone.Trim() },
                new SqlParameter("@Address", SqlDbType.NVarChar, -1) { Value = address ?? "" },
                new SqlParameter("@Notes", SqlDbType.NVarChar, -1) { Value = notes ?? "" },
                new SqlParameter("@Status", SqlDbType.NVarChar, 20) { Value = "New" },
                new SqlParameter("@Total", SqlDbType.Decimal) { Value = total },
                new SqlParameter("@CreatedAt", SqlDbType.DateTime) { Value = DateTime.Now },
                new SqlParameter("@CompanyID", SqlDbType.Int) { Value = companyId },
            };

            object orderIdObj = sql.ExecuteScalar(@"
INSERT INTO tbl_EcommerceOrder (Guid, OrderNo, CustomerName, Phone, Address, Notes, Status, Total, CreatedAt, CompanyID)
OUTPUT INSERTED.ID
VALUES (@Guid, @OrderNo, @CustomerName, @Phone, @Address, @Notes, @Status, @Total, @CreatedAt, @CompanyID)",
                orderPrm, con);

            int orderId = Simulate.Integer32(orderIdObj);
            if (orderId <= 0)
                throw new InvalidOperationException("Failed to create order.");

            foreach (var r in resolved)
            {
                SqlParameter[] linePrm =
                {
                    new SqlParameter("@OrderID", SqlDbType.Int) { Value = orderId },
                    new SqlParameter("@ItemGuid", SqlDbType.UniqueIdentifier) { Value = r.itemGuid },
                    new SqlParameter("@ItemName", SqlDbType.NVarChar, 300) { Value = r.name },
                    new SqlParameter("@Qty", SqlDbType.Decimal) { Value = r.qty },
                    new SqlParameter("@UnitPrice", SqlDbType.Decimal) { Value = r.unitPrice },
                    new SqlParameter("@LineTotal", SqlDbType.Decimal) { Value = r.lineTotal },
                    new SqlParameter("@CompanyID", SqlDbType.Int) { Value = companyId },
                    new SqlParameter("@Size", SqlDbType.NVarChar, 100) { Value = r.size ?? "" },
                    new SqlParameter("@Color", SqlDbType.NVarChar, 100) { Value = r.color ?? "" },
                    new SqlParameter("@LineNote", SqlDbType.NVarChar, -1) { Value = r.lineNote ?? "" },
                };
                sql.ExecuteNonQueryStatement(@"
INSERT INTO tbl_EcommerceOrderLine (OrderID, ItemGuid, ItemName, Qty, UnitPrice, LineTotal, CompanyID, Size, Color, LineNote)
VALUES (@OrderID, @ItemGuid, @ItemName, @Qty, @UnitPrice, @LineTotal, @CompanyID, @Size, @Color, @LineNote)", con, linePrm);
            }

            string shopName = string.IsNullOrWhiteSpace(shop.TradeName) ? shop.AName : shop.TradeName;
            return (orderNo, orderId, shop.Email, shopName);
        }
    }
}
