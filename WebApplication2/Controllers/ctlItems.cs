using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System;
using System.Data;
using WebApplication2.cls;

namespace WebApplication2.Controllers
{
    [Route("api/ctlItems")]
    public class ctlItems : Controller
    {
        // ==========================================================
        // Helpers
        // ==========================================================
        private byte[] DecodeBase64Image(string base64)
        {
            try
            {
                if (string.IsNullOrEmpty(base64))
                    return new byte[0];

                base64 = base64.Trim();

                if (base64.StartsWith("\"") && base64.EndsWith("\""))
                    base64 = base64.Substring(1, base64.Length - 2);

                if (base64.StartsWith("data:image", StringComparison.OrdinalIgnoreCase))
                {
                    int idx = base64.IndexOf("base64,", StringComparison.OrdinalIgnoreCase);
                    if (idx >= 0)
                        base64 = base64.Substring(idx + "base64,".Length);
                }

                if (string.IsNullOrEmpty(base64))
                    return new byte[0];

                return Convert.FromBase64String(base64);
            }
            catch
            {
                return new byte[0];
            }
        }

        // ==========================================================
        // ✅ SELECT ITEM FULL (Header + 5 Tabs)
        // Flutter: GET /api/ctlItems/SelectItemFullByGuid?Guid=...&CompanyId=...
        // ==========================================================
        [HttpGet]
        [Route("SelectItemFullByGuid")]
        public string SelectItemFullByGuid(string Guid, int CompanyId)
        {
            try
            {
                // -------------------------------
                // 1) Item Header
                // -------------------------------
                clsItems obj = new clsItems();
                DataTable dtItem = obj.SelectItemsByGuid(
                    Simulate.String(Guid),
                    "", "", "", 0, -1,
                    CompanyId
                );

                // if not found => return empty
                if (dtItem == null || dtItem.Rows.Count == 0)
                {
                    var empty = new
                    {
                        item = new object[] { },
                        vendors = new object[] { },
                        uoms = new object[] { },
                        additionalBarcodes = new object[] { },
                        images = new object[] { },
                        reorderPolicies = new object[] { }
                    };
                    return JsonConvert.SerializeObject(empty);
                }

                // -------------------------------
                // 2) Tabs Data (best-effort)
                // -------------------------------
                DataTable dtVendors = null;
                DataTable dtUoms = null;
                DataTable dtBarcodes = null;
                DataTable dtImages = null;
                DataTable dtReorder = null;

                // ✅ Vendors
                try
                {
                    // If you already have: clsItemVendor in your project
                    clsItemVendor v = new clsItemVendor();
                    dtVendors = v.SelectItemVendorByGuid("",Simulate.String( Simulate.Guid(Guid)),0, 1,CompanyId);
                }
                catch { dtVendors = new DataTable(); }

                // ✅ UOM
                try
                {
                    clsItemUOM u = new clsItemUOM();
                    dtUoms = u.SelectItemUOMByGuid("", Simulate.String(Simulate.Guid(Guid)), CompanyId);
                }
                catch { dtUoms = new DataTable(); }

                // ✅ Additional Barcodes
                try
                {
                    clsItemAdditionalBarcode b = new clsItemAdditionalBarcode();
                    dtBarcodes = b.SelectByItemGuid(Simulate.String(Simulate.Guid(Guid)), CompanyId);
                }
                catch { dtBarcodes = new DataTable(); }

                // ✅ Images (optional)
                try
                {
                    clsItemImage img = new clsItemImage();
                    dtImages = img.SelectByItemGuid(Simulate.String(Simulate.Guid(Guid)), CompanyId);
                }
                catch { dtImages = new DataTable(); }

                // ✅ Reorder Policies (optional)
                try
                {
                    clsItemReorder r = new clsItemReorder();
                    dtReorder = r.SelectByItemGuid(Simulate.String(Simulate.Guid(Guid)), CompanyId);
                }
                catch { dtReorder = new DataTable(); }

                // -------------------------------
                // 3) Return Combined JSON
                // -------------------------------
                var payload = new
                {
                    item = dtItem,
                    vendors = dtVendors,
                    uoms = dtUoms,
                    additionalBarcodes = dtBarcodes,
                    images = dtImages,
                    reorderPolicies = dtReorder
                };

                return JsonConvert.SerializeObject(payload);
            }
            catch
            {
                throw;
            }
        }

        // ==========================================================
        // SELECT ITEMS (existing)
        // ==========================================================
        [HttpGet]
        [Route("SelectItemsByGuid")]
        public string SelectItemsByGuid(
            string Guid,
            string AName,
            string EName,
            string Barcode,
            int CategoryID,
            int IsPOS,
            int CompanyId
        )
        {
            try
            {
                clsItems obj = new clsItems();

                DataTable dt = obj.SelectItemsByGuid(
                    Simulate.String(Guid),
                    Simulate.String(AName),
                    Simulate.String(EName),
                    Simulate.String(Barcode),
                    CategoryID,
                    IsPOS,
                    CompanyId
                );

                return dt != null ? JsonConvert.SerializeObject(dt) : "";
            }
            catch
            {
                throw;
            }
        }

        // ==========================================================
        // DELETE ITEM
        // ==========================================================
        // Deletion is a state-changing operation, so it is exposed over POST (not GET).
        [HttpPost]
        [Route("DeleteItemsByGuid")]
        public string DeleteItemsByGuid(string Guid, int CompanyID)
        {
            try
            {
                clsItems obj = new clsItems();
                bool success = obj.DeleteItemsByGuid(Simulate.String(Guid), CompanyID);

                return success ? JsonConvert.SerializeObject(true) : JsonConvert.SerializeObject(false);
            }
            catch
            {
                throw;
            }
        }

        // ==========================================================
        // INSERT ITEM
        // ==========================================================
        [HttpPost]
        [Route("InsertItems")]
        public string InsertItems(
            string AName, string EName, string Description,
            decimal SalesPriceBeforeTax, decimal SalesPriceAfterTax,
            int CategoryID, int SalesTaxID, int SpecialSalesTaxID,
            int PurchaseTaxID, int SpecialPurchaseTaxID,
            string Barcode, int ReadType, int OriginID,
            decimal MinimumLimit, [FromBody] string Picture,
            bool IsActive, bool IsPOS, int BoxTypeID, bool IsStockItem, int POSOrder,
            bool TrackLot, bool TrackSerial, bool TrackExpiryDate,
            string tabsJson,
            // NEW
            string ItemCode, int ItemTypeID,
            int BrandID, int ManufacturerID, string ModelNo,
            int BaseUOMID, int SalesUOMID, int PurchaseUOMID,
            decimal StandardCost, decimal LastPurchaseCost,
            bool IsWeightedItem, bool IsOpenPrice,
            bool AllowNegativeStock,
            int ShelfLifeDays, int ExpiryWarningDays,
             string ParentGuid,
            decimal BaseFactor,
            bool ShowOnWeb = false, decimal WebPrice = 0,
            bool WebAllowCustomNote = false, bool WebHasSize = false, bool WebHasColor = false,
            string WebSizeOptions = "", string WebColorOptions = "",
            int CompanyID = 0, int CreationUserId = 0
        )
        {
            try
            {
                byte[] myPicture = DecodeBase64Image(Picture);

                clsItems obj = new clsItems();

                string guid = obj.InsertItems(
                    Simulate.String(AName),
                    Simulate.String(EName),
                    Simulate.String(Description),
                    Simulate.decimal_(SalesPriceBeforeTax),
                    Simulate.decimal_(SalesPriceAfterTax),
                    CategoryID,
                    SalesTaxID,
                    SpecialSalesTaxID,
                    PurchaseTaxID,
                    SpecialPurchaseTaxID,
                    Simulate.String(Barcode),
                    ReadType,
                    OriginID,
                    MinimumLimit,
                    myPicture,
                    IsActive,
                    IsPOS,
                    BoxTypeID,
                    IsStockItem,
                    POSOrder,
                    TrackLot,
                    TrackSerial,
                    TrackExpiryDate,

                    // NEW
                    Simulate.String(ItemCode),
                    ItemTypeID,
                    BrandID,
                    ManufacturerID,
                    Simulate.String(ModelNo),
                    BaseUOMID,
                    SalesUOMID,
                    PurchaseUOMID,
                    StandardCost,
                    LastPurchaseCost,
                    IsWeightedItem,
                    IsOpenPrice,
                    AllowNegativeStock,
                    ShelfLifeDays,
                    ExpiryWarningDays,
                       ParentGuid,
  BaseFactor,
                    CompanyID,
                    CreationUserId,
                    null,
                    ShowOnWeb,
                    WebPrice,
                    WebAllowCustomNote,
                    WebHasSize,
                    WebHasColor,
                    Simulate.String(WebSizeOptions),
                    Simulate.String(WebColorOptions)
                );
                if (!string.IsNullOrWhiteSpace(tabsJson))
                {

                    obj.SaveItemTabs(guid, CompanyID, CreationUserId, tabsJson);
                }
                return Simulate.String(guid);
            }
            catch
            {
                throw;
            }
        }

        // ==========================================================
        // UPDATE ITEM
        // ==========================================================
        [HttpPost]
        [Route("UpdateItems")]
        public int UpdateItems(
            string Guid,
            string AName, string EName, string Description,
            decimal SalesPriceBeforeTax, decimal SalesPriceAfterTax,
            int CategoryID, int SalesTaxID, int SpecialSalesTaxID,
            int PurchaseTaxID, int SpecialPurchaseTaxID,
            string Barcode, int ReadType, int OriginID,
            decimal MinimumLimit, [FromBody] string Picture,
            bool IsActive, bool IsPOS, int BoxTypeID, bool IsStockItem, int POSOrder,
            bool TrackLot, bool TrackSerial, bool TrackExpiryDate,
            string tabsJson,
            // NEW
            string ItemCode, int ItemTypeID,
            int BrandID, int ManufacturerID, string ModelNo,
            int BaseUOMID, int SalesUOMID, int PurchaseUOMID,
            decimal StandardCost, decimal LastPurchaseCost,
            bool IsWeightedItem, bool IsOpenPrice,
            bool AllowNegativeStock,
            int ShelfLifeDays, int ExpiryWarningDays,
            string ParentGuid,
            decimal BaseFactor,
            bool ShowOnWeb = false, decimal WebPrice = 0,
            bool WebAllowCustomNote = false, bool WebHasSize = false, bool WebHasColor = false,
            string WebSizeOptions = "", string WebColorOptions = "",
            int ModificationUserId = 0, int CompanyID = 0
        )
        {
            try
            {
                byte[] myPicture = DecodeBase64Image(Picture);

                clsItems obj = new clsItems();

                int A = obj.UpdateItems(
                    Simulate.String(Guid),
                    Simulate.String(AName),
                    Simulate.String(EName),
                    Simulate.String(Description),
                    Simulate.decimal_(SalesPriceBeforeTax),
                    Simulate.decimal_(SalesPriceAfterTax),
                    CategoryID,
                    SalesTaxID,
                    SpecialSalesTaxID,
                    PurchaseTaxID,
                    SpecialPurchaseTaxID,
                    Simulate.String(Barcode),
                    ReadType,
                    OriginID,
                    MinimumLimit,
                    myPicture,
                    IsActive,
                    IsPOS,
                    BoxTypeID,
                    IsStockItem,
                    POSOrder,
                    TrackLot,
                    TrackSerial,
                    TrackExpiryDate,

                    // NEW
                    Simulate.String(ItemCode),
                    ItemTypeID,
                    BrandID,
                    ManufacturerID,
                    Simulate.String(ModelNo),
                    BaseUOMID,
                    SalesUOMID,
                    PurchaseUOMID,
                    StandardCost,
                    LastPurchaseCost,
                    IsWeightedItem,
                    IsOpenPrice,
                    AllowNegativeStock,
                    ShelfLifeDays,
                    ExpiryWarningDays,
                       ParentGuid,
  BaseFactor,
                    ModificationUserId,
                    CompanyID,
                    null,
                    ShowOnWeb,
                    WebPrice,
                    WebAllowCustomNote,
                    WebHasSize,
                    WebHasColor,
                    Simulate.String(WebSizeOptions),
                    Simulate.String(WebColorOptions)
                );
                if (!string.IsNullOrWhiteSpace(tabsJson))
                {
                   
                    obj.SaveItemTabs(Guid, CompanyID, ModificationUserId, tabsJson);
                }
                return A;
            }
            catch
            {
                throw;
            }
        }
        public class UploadItemImageBody
        {
            public int sortOrder { get; set; }
            public bool isDefault { get; set; }
            public bool isActive { get; set; }
            public string imageBase64 { get; set; } // or data:image/...;base64,...
        }

        [HttpPost]
        [Route("UploadItemImage")]
        public string UploadItemImage(string ItemGuid, int CompanyId, int UserId, [FromBody] UploadItemImageBody body)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(ItemGuid)) return JsonConvert.SerializeObject(false);
                if (body == null) return JsonConvert.SerializeObject(false);

                byte[] bytes = DecodeBase64Image(body.imageBase64); // you already have this helper in controller
                if (bytes == null || bytes.Length == 0) return JsonConvert.SerializeObject(false);

                int sortOrder = body.sortOrder <= 0 ? 1 : body.sortOrder;

                clsItemImage img = new clsItemImage();

                img.Insert(
                    ItemGuid: Simulate.String(ItemGuid),
                    ImageData: bytes,
                    SortOrder: sortOrder,
                    IsDefault: body.isDefault,
                    IsActive: body.isActive,
                    CompanyID: CompanyId,
                    CreationUserId: UserId
                );

                return JsonConvert.SerializeObject(true);
            }
            catch
            {
                throw;
            }
        }

        public class ReorderKeysBody
        {
            public string OrderedGuids { get; set; }
            public string OrderedIds { get; set; }
        }

        [HttpPost]
        [Route("ReorderPOSItems")]
        public bool ReorderPOSItems(
            [FromQuery] int CompanyID,
            [FromQuery] int ModificationUserID,
            [FromQuery] int CashDrawerID = 0,
            [FromQuery] string OrderedGuids = null,
            [FromBody] ReorderKeysBody body = null)
        {
            try
            {
                string ordered = OrderedGuids;
                if (string.IsNullOrWhiteSpace(ordered) && body != null)
                    ordered = body.OrderedGuids;

                // Cash-level: order follows this drawer on any PC (does not change master POSOrder).
                if (CashDrawerID > 0)
                {
                    try
                    {
                        clsPOSCashMenuOrder cashOrder = new clsPOSCashMenuOrder();
                        return cashOrder.ReplaceOrder(
                            CashDrawerID,
                            clsPOSCashMenuOrder.KindItem,
                            Simulate.String(ordered),
                            CompanyID,
                            ModificationUserID);
                    }
                    catch
                    {
                        // Table may not exist yet on older DBs — let client fall back to company order.
                        return false;
                    }
                }
                clsItems obj = new clsItems();
                return obj.ReorderPOSItems(Simulate.String(ordered), CompanyID, ModificationUserID);
            }
            catch
            {
                throw;
            }
        }

        [HttpGet]
        [Route("GetPOSCashMenuOrder")]
        public string GetPOSCashMenuOrder(int CashDrawerID, int CompanyID)
        {
            try
            {
                clsPOSCashMenuOrder cashOrder = new clsPOSCashMenuOrder();
                DataTable dt = cashOrder.SelectByCashDrawer(CashDrawerID, CompanyID);
                return Newtonsoft.Json.JsonConvert.SerializeObject(dt);
            }
            catch
            {
                // Missing table / older DB — treat as "no cash order yet".
                return "[]";
            }
        }

        [HttpPost]
        [Route("CopyPOSCashMenuOrder")]
        public bool CopyPOSCashMenuOrder(
            int FromCashDrawerID,
            int ToCashDrawerID,
            int CompanyID,
            int ModificationUserID)
        {
            try
            {
                clsPOSCashMenuOrder cashOrder = new clsPOSCashMenuOrder();
                return cashOrder.CopyOrder(
                    FromCashDrawerID,
                    ToCashDrawerID,
                    CompanyID,
                    ModificationUserID);
            }
            catch
            {
                throw;
            }
        }

    }
}
