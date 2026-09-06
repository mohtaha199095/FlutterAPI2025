using DocumentFormat.OpenXml.Office.Word;
using DocumentFormat.OpenXml.Office2019.Presentation;
using Microsoft.Data.SqlClient;
using Newtonsoft.Json.Linq;
using System;
using System.Data;
using static WebApplication2.MainClasses.clsEnum;

namespace WebApplication2.cls
{
    public class clsItems
    {
        public void SaveItemTabs(string itemGuid, int companyId, int userId, string tabsJson, SqlTransaction trn = null)
        {
            if (string.IsNullOrWhiteSpace(itemGuid)) return;
            if (string.IsNullOrWhiteSpace(tabsJson)) return;

            tabsJson = NormalizeJson(tabsJson);
            JObject root = JObject.Parse(tabsJson);

            // If caller didn't pass a transaction, we open our own and commit/rollback.
            if (trn == null)
            {
                clsSQL sql = new clsSQL();
                using SqlConnection con = new SqlConnection(sql.CreateDataBaseConnectionString(companyId));
                con.Open();

                using SqlTransaction t = con.BeginTransaction();
                try
                {
                    SaveItemTabs_Internal(itemGuid, companyId, userId, root, t);
                    t.Commit();
                }
                catch
                {
                    t.Rollback();
                    throw;
                }
            }
            else
            {
                SaveItemTabs_Internal(itemGuid, companyId, userId, root, trn);
            }
        }

        private void SaveItemTabs_Internal(string itemGuid, int companyId, int userId, JObject root, SqlTransaction trn)
        {
            // Vendors
            new clsItemVendor().ReplaceForItem(
                itemGuid,
                root["vendors"] as JArray,
                companyId,
                userId,
                trn
            );

            // UOMs
            new clsItemUOM().ReplaceForItem(
                itemGuid,
                root["uoms"] as JArray,
                companyId,
                userId,
                trn
            );

            // Additional Barcodes
            new clsItemAdditionalBarcode().ReplaceForItem(
                itemGuid,
                root["additionalBarcodes"] as JArray,
                companyId,
                userId,
                trn
            );

            // Images
            new clsItemImage().ReplaceForItem(
                itemGuid,
                root["images"] as JArray,
                companyId,
                userId,
                trn
            );

            // Reorder Policies
            new clsItemReorder().ReplaceForItem(
                itemGuid,
                root["reorderPolicies"] as JArray,
                companyId,
                userId,
                trn
            );
        }
       
        private string NormalizeJson(string json)
        {
            json = (json ?? "").Trim();

            // When body comes as quoted JSON string: "\"{...}\""
            if (json.StartsWith("\"") && json.EndsWith("\""))
            {
                json = json.Substring(1, json.Length - 2);
                json = json.Replace("\\\"", "\"");
                json = json.Replace("\\\\", "\\");
            }

            return json;
        }
    
        // Kept for backward compatibility with existing callers (purchase / good-receipt
        // posting). Previously this did an incremental moving-average that could divide by
        // zero or produce a negative cost when prior on-hand was zero/negative, and it
        // disagreed with RecalculateItemAverageCost used on delete. It now delegates to the
        // single canonical costing routine so save and delete always produce the same value.
        // The addedQTY / newcostPerUnit arguments are intentionally ignored: the canonical
        // routine recomputes the weighted average from the full inbound history.
        public DataTable UpdateItemCost(string Itemguid, decimal addedQTY, decimal newcostPerUnit, int CompanyId, SqlTransaction trn)
        {
            try
            {
                RecalculateItemAverageCost(Itemguid, CompanyId, trn);
                return null;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public void RecalculateItemAverageCost(string itemGuid, int companyId, SqlTransaction trn)
        {
            try
            {
                clsSQL clsSQL = new clsSQL();
                SqlParameter[] prm =
                {
                    new SqlParameter("@Itemguid", SqlDbType.UniqueIdentifier) { Value = Simulate.Guid(itemGuid) },
                    new SqlParameter("@CompanyId", SqlDbType.Int) { Value = companyId },
                };

                // Take an exclusive lock on the item row for the duration of the transaction so
                // concurrent purchases/receipts of the same item cannot interleave their
                // read-modify-write of AVGCostPerUnit and lose an update.
                clsSQL.ExecuteQueryStatement(
                    "SELECT Guid FROM tbl_Items WITH (UPDLOCK, ROWLOCK) WHERE Guid = @Itemguid AND (CompanyID = @CompanyId OR @CompanyId = 0)",
                    clsSQL.CreateDataBaseConnectionString(companyId), prm, trn);

                // Inbound cost layers: Purchase (2), Good Receipt (8), Financing purchase (22),
                // and Manufacturing FG receipt (26) so produced goods enter the weighted average.
                // Exclude warehouse-transfer and stock-count GRs (RefNo markers) — those move or
                // reconcile existing stock and must not inflate the weighted-average cost base.
                DataTable dt = clsSQL.ExecuteQueryStatement(@"
SELECT
  SUM(d.TotalQTY) AS InboundQty,
  SUM(d.TotalQTY * (d.PriceBeforeTax - d.DiscountBeforeTaxAmountPcs)) AS InboundCost
FROM tbl_InvoiceDetails d
LEFT JOIN tbl_InvoiceHeader h ON h.Guid = d.HeaderGuid
WHERE d.ItemGuid = @Itemguid
  AND (d.CompanyID = @CompanyId OR @CompanyId = 0)
  AND d.InvoiceTypeID IN (2, 8, 22, 26)
  AND ISNULL(h.RefNo, N'') NOT IN (N'WHTRANSFER', N'STOCKCOUNT')",
                    clsSQL.CreateDataBaseConnectionString(companyId), prm, trn);

                decimal newCost = 0;
                if (dt != null && dt.Rows.Count > 0)
                {
                    decimal inboundQty = Simulate.decimal_(dt.Rows[0]["InboundQty"]);
                    decimal inboundCost = Simulate.decimal_(dt.Rows[0]["InboundCost"]);
                    if (inboundQty > 0)
                        newCost = inboundCost / inboundQty;
                }

                SqlParameter[] prmUpdate =
                {
                    new SqlParameter("@Itemguid", SqlDbType.UniqueIdentifier) { Value = Simulate.Guid(itemGuid) },
                    new SqlParameter("@newcost", SqlDbType.Decimal) { Value = newCost },
                    new SqlParameter("@CompanyId", SqlDbType.Int) { Value = companyId },
                };
                clsSQL.ExecuteNonQueryStatement(
                    "UPDATE tbl_Items SET AVGCostPerUnit = @newcost WHERE Guid = @Itemguid AND (CompanyID = @CompanyId OR @CompanyId = 0)",
                    clsSQL.CreateDataBaseConnectionString(companyId), prmUpdate, trn);
            }
            catch (Exception)
            {
                throw;
            }
        }

        // Current weighted-average unit cost for an item.
        public decimal GetAvgCost(string itemGuid, int companyId, SqlTransaction trn)
        {
            clsSQL clsSQL = new clsSQL();
            SqlParameter[] prm =
            {
                new SqlParameter("@Itemguid", SqlDbType.UniqueIdentifier) { Value = Simulate.Guid(itemGuid) },
                new SqlParameter("@CompanyId", SqlDbType.Int) { Value = companyId },
            };
            DataTable dt = clsSQL.ExecuteQueryStatement(
                "SELECT ISNULL(AVGCostPerUnit,0) AS AvgCost FROM tbl_Items WHERE Guid = @Itemguid AND (CompanyID = @CompanyId OR @CompanyId = 0)",
                clsSQL.CreateDataBaseConnectionString(companyId), prm, trn);
            if (dt != null && dt.Rows.Count > 0)
                return Simulate.decimal_(dt.Rows[0]["AvgCost"]);
            return 0;
        }

        // Current on-hand quantity for an item, optionally scoped to a store.
        // Uses TotalQTY * QTYFactor over counted lines, matching the costing basis.
        public decimal GetOnHandQty(string itemGuid, int storeId, int companyId, SqlTransaction trn)
        {
            clsSQL clsSQL = new clsSQL();
            SqlParameter[] prm =
            {
                new SqlParameter("@Itemguid", SqlDbType.UniqueIdentifier) { Value = Simulate.Guid(itemGuid) },
                new SqlParameter("@StoreID", SqlDbType.Int) { Value = storeId },
                new SqlParameter("@CompanyId", SqlDbType.Int) { Value = companyId },
            };
            DataTable dt = clsSQL.ExecuteQueryStatement(@"
SELECT ISNULL(SUM(d.TotalQTY * jvt.QTYFactor), 0) AS OnHand
FROM tbl_InvoiceDetails d
LEFT JOIN tbl_JournalVoucherTypes jvt ON jvt.id = d.InvoiceTypeID
WHERE d.IsCounted = 1
  AND d.ItemGuid = @Itemguid
  AND (d.StoreID = @StoreID OR @StoreID = 0)
  AND (d.CompanyID = @CompanyId OR @CompanyId = 0)",
                clsSQL.CreateDataBaseConnectionString(companyId), prm, trn);

            if (dt != null && dt.Rows.Count > 0)
                return Simulate.decimal_(dt.Rows[0]["OnHand"]);
            return 0;
        }

        // Returns stock-control flags for an item: index 0 = IsStockItem, 1 = AllowNegativeStock,
        // plus the item display name. Returns null when the item cannot be found.
        public DataTable GetItemStockFlags(string itemGuid, int companyId, SqlTransaction trn)
        {
            clsSQL clsSQL = new clsSQL();
            SqlParameter[] prm =
            {
                new SqlParameter("@Itemguid", SqlDbType.UniqueIdentifier) { Value = Simulate.Guid(itemGuid) },
                new SqlParameter("@CompanyId", SqlDbType.Int) { Value = companyId },
            };
            return clsSQL.ExecuteQueryStatement(@"
SELECT ISNULL(IsStockItem,0) AS IsStockItem, ISNULL(AllowNegativeStock,0) AS AllowNegativeStock, AName
FROM tbl_Items
WHERE Guid = @Itemguid AND (CompanyID = @CompanyId OR @CompanyId = 0)",
                clsSQL.CreateDataBaseConnectionString(companyId), prm, trn);
        }

        public DataTable SelectItemsByGuid(string guid, string AName, string EName, String Barcode, int CategoryID, int IsPOS, int CompanyId,SqlTransaction trn= null)
        {
            try
            {
                clsSQL clsSQL = new clsSQL();

                SqlParameter[] prm =
                 { new SqlParameter("@guid", SqlDbType.UniqueIdentifier) { Value =Simulate.Guid( guid )},
      new SqlParameter("@AName", SqlDbType.NVarChar,-1) { Value = AName },
       new SqlParameter("@EName", SqlDbType.NVarChar,-1) { Value = EName },
         new SqlParameter("@Barcode", SqlDbType.NVarChar,-1) { Value = Barcode },
       new SqlParameter("@CategoryID", SqlDbType.Int) { Value = CategoryID },
         new SqlParameter("@IsPOS", SqlDbType.Int) { Value = IsPOS },
           new SqlParameter("@CompanyId", SqlDbType.Int) { Value = CompanyId },
                };
                DataTable dt = clsSQL.ExecuteQueryStatement(@"select * from tbl_Items where (guid=@guid or @guid='00000000-0000-0000-0000-000000000000' ) and  
                     (AName=@AName or @AName='' ) and (EName=@EName or @EName='' ) and (CategoryID=@CategoryID or @CategoryID=0 )and (IsPOS=@IsPOS or @IsPOS=-1 ) and(Barcode=@Barcode or @Barcode='' ) and (CompanyId=@CompanyId or @CompanyId=0  )  
                     order by ISNULL(POSOrder, 2147483647), AName
                     ", clsSQL.CreateDataBaseConnectionString(CompanyId), prm, trn);

                return dt;
            }
            catch (Exception)
            {

                throw;
            }


        }

        public bool ReorderPOSItems(string orderedGuids, int CompanyID, int ModificationUserID)
        {
            if (string.IsNullOrWhiteSpace(orderedGuids)) return false;
            string[] guids = orderedGuids.Split(',', StringSplitOptions.RemoveEmptyEntries);
            clsSQL clsSQL = new clsSQL();
            string conn = clsSQL.CreateDataBaseConnectionString(CompanyID);
            using (SqlConnection con = new SqlConnection(conn))
            {
                con.Open();
                using (SqlTransaction trn = con.BeginTransaction())
                {
                    for (int i = 0; i < guids.Length; i++)
                    {
                        string g = guids[i].Trim();
                        if (string.IsNullOrWhiteSpace(g)) continue;
                        SqlParameter[] prm =
                        {
                            new SqlParameter("@Guid", SqlDbType.UniqueIdentifier) { Value = Simulate.Guid(g) },
                            new SqlParameter("@POSOrder", SqlDbType.Int) { Value = i + 1 },
                            new SqlParameter("@ModificationUserId", SqlDbType.Int) { Value = ModificationUserID },
                            new SqlParameter("@CompanyID", SqlDbType.Int) { Value = CompanyID },
                        };
                        clsSQL.ExecuteNonQueryStatement(
                            @"UPDATE tbl_Items SET POSOrder=@POSOrder, ModificationUserId=@ModificationUserId, ModificationDate=GETDATE()
                              WHERE Guid=@Guid AND CompanyID=@CompanyID",
                            conn, prm, trn);
                    }
                    trn.Commit();
                }
            }
            return true;
        }

        public bool DeleteItemsByGuid(string Guid,int CompanyID)
        {
            try
            {
                clsSQL clsSQL = new clsSQL();

                SqlParameter[] prm =
                 { new SqlParameter("@Guid", SqlDbType.UniqueIdentifier) { Value =Simulate.Guid( Guid) },
                    new SqlParameter("@CompanyID", SqlDbType.Int) { Value = CompanyID },
                };

                // Referential safety: never hard-delete an item that has transaction history,
                // otherwise existing invoice lines / stock movements are orphaned and reports
                // (and average cost) break. Authoritative check on the server side.
                DataTable usage = clsSQL.ExecuteQueryStatement(
                    @"SELECT COUNT(*) AS Cnt FROM tbl_InvoiceDetails WHERE ItemGuid = @Guid AND (CompanyID = @CompanyID OR @CompanyID = 0)",
                    clsSQL.CreateDataBaseConnectionString(CompanyID), prm);
                if (usage != null && usage.Rows.Count > 0 && Simulate.Integer32(usage.Rows[0]["Cnt"]) > 0)
                {
                    throw new InvalidOperationException(
                        "This item cannot be deleted because it is used in one or more transactions. Deactivate it instead.");
                }

                int A = clsSQL.ExecuteNonQueryStatement(@"delete from tbl_Items where (Guid=@Guid  )", clsSQL.CreateDataBaseConnectionString(CompanyID), prm);

                // Reflect whether a row was actually removed instead of always reporting success.
                return A > 0;
            }
            catch (Exception)
            {

                throw;
            }


        }
        public String InsertItems(string AName, string EName, string Description, decimal SalesPriceBeforeTax, decimal SalesPriceAfterTax, int CategoryID, int SalesTaxID
            , int SpecialSalesTaxID, int PurchaseTaxID, int SpecialPurchaseTaxID, string Barcode, int ReadType, int OriginID, decimal MinimumLimit, byte[] Picture
            , bool IsActive, bool IsPOS, int BoxTypeID, bool IsStockItem, int POSOrder, 
            bool TrackLot,bool TrackSerial, bool TrackExpiryDate,
                        /////
            string ItemCode, int ItemTypeID,
            int BrandID, int ManufacturerID, string ModelNo,
            int BaseUOMID, int SalesUOMID, int PurchaseUOMID,
            decimal StandardCost, decimal LastPurchaseCost,
            bool IsWeightedItem, bool IsOpenPrice,
            bool AllowNegativeStock,
            int ShelfLifeDays, int ExpiryWarningDays, string ParentGuid,
decimal BaseFactor,
            ///
            int CompanyID, int CreationUserId,SqlTransaction trn=null,
            bool ShowOnWeb = false, decimal WebPrice = 0,
            bool WebAllowCustomNote = false, bool WebHasSize = false, bool WebHasColor = false,
            string WebSizeOptions = "", string WebColorOptions = "")
        {
            try
            {
             

                SqlParameter[] prm =
                {
                     new SqlParameter("@TrackLot", SqlDbType.Bit) { Value = TrackLot },
new SqlParameter("@TrackSerial", SqlDbType.Bit) { Value = TrackSerial },
new SqlParameter("@TrackExpiryDate", SqlDbType.Bit) { Value = TrackExpiryDate },
                  new SqlParameter("@AName", SqlDbType.NVarChar,-1) { Value = AName },
                  new SqlParameter("@EName", SqlDbType.NVarChar,-1) { Value = EName },
                  new SqlParameter("@Description", SqlDbType.NVarChar,-1) { Value = Description },
                  new SqlParameter("@SalesPriceBeforeTax", SqlDbType.Decimal) { Value = SalesPriceBeforeTax },
                       new SqlParameter("@SalesPriceAfterTax", SqlDbType.Decimal) { Value = SalesPriceAfterTax },
                  new SqlParameter("@CategoryID", SqlDbType.Int) { Value = CategoryID },
                  new SqlParameter("@SalesTaxID", SqlDbType.Int) { Value = SalesTaxID },
                  new SqlParameter("@SpecialSalesTaxID", SqlDbType.Int) { Value = SpecialSalesTaxID },
                  new SqlParameter("@PurchaseTaxID", SqlDbType.Int) { Value = PurchaseTaxID },
                  new SqlParameter("@SpecialPurchaseTaxID", SqlDbType.Int) { Value = SpecialPurchaseTaxID },
                  new SqlParameter("@Barcode", SqlDbType.NVarChar,-1) { Value = Barcode },
                  new SqlParameter("@ReadType", SqlDbType.Int) { Value = ReadType },
                  new SqlParameter("@OriginID", SqlDbType.Int) { Value = OriginID },
                  new SqlParameter("@MinimumLimit", SqlDbType.Decimal) { Value = MinimumLimit },
                  new SqlParameter("@Picture", SqlDbType.Image) { Value = Picture },
                  new SqlParameter("@IsActive", SqlDbType.Bit) { Value = IsActive },
                  new SqlParameter("@IsPOS", SqlDbType.Bit) { Value = IsPOS },
                  new SqlParameter("@BoxTypeID", SqlDbType.Int) { Value = BoxTypeID },
                  new SqlParameter("@IsStockItem", SqlDbType.Bit) { Value = IsStockItem },
                  new SqlParameter("@POSOrder", SqlDbType.Int) { Value = POSOrder },
                     // ---- New columns
                    new SqlParameter("@ItemCode", SqlDbType.NVarChar, -1) { Value = ItemCode ?? "" },
                    new SqlParameter("@ItemTypeID", SqlDbType.Int) { Value = ItemTypeID },

                    new SqlParameter("@BrandID", SqlDbType.Int) { Value = BrandID },
                    new SqlParameter("@ManufacturerID", SqlDbType.Int) { Value = ManufacturerID },
                    new SqlParameter("@ModelNo", SqlDbType.NVarChar, -1) { Value = ModelNo ?? "" },

                    new SqlParameter("@BaseUOMID", SqlDbType.Int) { Value = BaseUOMID },
                    new SqlParameter("@SalesUOMID", SqlDbType.Int) { Value = SalesUOMID },
                    new SqlParameter("@PurchaseUOMID", SqlDbType.Int) { Value = PurchaseUOMID },

                    new SqlParameter("@StandardCost", SqlDbType.Decimal) { Value = StandardCost },
                    new SqlParameter("@LastPurchaseCost", SqlDbType.Decimal) { Value = LastPurchaseCost },

                    new SqlParameter("@IsWeightedItem", SqlDbType.Bit) { Value = IsWeightedItem },
                    new SqlParameter("@IsOpenPrice", SqlDbType.Bit) { Value = IsOpenPrice },
                    new SqlParameter("@AllowNegativeStock", SqlDbType.Bit) { Value = AllowNegativeStock },

                    new SqlParameter("@ShelfLifeDays", SqlDbType.Int) { Value = ShelfLifeDays },
                    new SqlParameter("@ExpiryWarningDays", SqlDbType.Int) { Value = ExpiryWarningDays },





                  new SqlParameter("@CompanyID", SqlDbType.Int) { Value = CompanyID },
                  new SqlParameter("@CreationUserId", SqlDbType.Int) { Value = CreationUserId },
                  new SqlParameter("@CreationDate", SqlDbType.DateTime) { Value = DateTime.Now },
                  new SqlParameter("@ParentGuid", SqlDbType.UniqueIdentifier){ Value = Simulate.Guid(ParentGuid) },
                  new SqlParameter("@BaseFactor", SqlDbType.Decimal){ Value = BaseFactor <= 0 ? 1 : BaseFactor },
                  new SqlParameter("@ShowOnWeb", SqlDbType.Bit) { Value = ShowOnWeb },
                  new SqlParameter("@WebPrice", SqlDbType.Decimal) { Value = WebPrice },
                  new SqlParameter("@WebAllowCustomNote", SqlDbType.Bit) { Value = WebAllowCustomNote },
                  new SqlParameter("@WebHasSize", SqlDbType.Bit) { Value = WebHasSize },
                  new SqlParameter("@WebHasColor", SqlDbType.Bit) { Value = WebHasColor },
                  new SqlParameter("@WebSizeOptions", SqlDbType.NVarChar, 500) { Value = WebSizeOptions ?? "" },
                  new SqlParameter("@WebColorOptions", SqlDbType.NVarChar, 500) { Value = WebColorOptions ?? "" },
                };

                string a = @"insert into tbl_Items(AName,EName,Description,SalesPriceBeforeTax,SalesPriceAfterTax,CategoryID,SalesTaxID,SpecialSalesTaxID,PurchaseTaxID
 ,SpecialPurchaseTaxID ,Barcode,ReadType ,OriginID,MinimumLimit ,Picture,IsActive ,IsPOS,BoxTypeID,IsStockItem,
POSOrder,CompanyID,CreationUserId,CreationDate,TrackLot,TrackSerial,TrackExpiryDate,
ItemCode,ItemTypeID,
    BrandID,ManufacturerID,ModelNo,
    BaseUOMID,SalesUOMID,PurchaseUOMID,
    StandardCost,LastPurchaseCost,
    IsWeightedItem,IsOpenPrice,AllowNegativeStock,
    ShelfLifeDays,ExpiryWarningDays ,ParentGuid, BaseFactor, ShowOnWeb, WebPrice,
    WebAllowCustomNote, WebHasSize, WebHasColor, WebSizeOptions, WebColorOptions

)
                        OUTPUT INSERTED.guid values(@AName,@EName,@Description,@SalesPriceBeforeTax,@SalesPriceAfterTax,@CategoryID,@SalesTaxID,@SpecialSalesTaxID,@PurchaseTaxID
, @SpecialPurchaseTaxID ,@Barcode,@ReadType,@OriginID,@MinimumLimit,@Picture,@IsActive,@IsPOS,@BoxTypeID,
@IsStockItem,@POSOrder,@CompanyID,@CreationUserId,@CreationDate,@TrackLot,@TrackSerial,@TrackExpiryDate,
    @ItemCode,@ItemTypeID,
    @BrandID,@ManufacturerID,@ModelNo,
    @BaseUOMID,@SalesUOMID,@PurchaseUOMID,
    @StandardCost,@LastPurchaseCost,
    @IsWeightedItem,@IsOpenPrice,@AllowNegativeStock,
    @ShelfLifeDays,@ExpiryWarningDays,@ParentGuid, @BaseFactor, @ShowOnWeb, @WebPrice,
    @WebAllowCustomNote, @WebHasSize, @WebHasColor, @WebSizeOptions, @WebColorOptions

)";
                clsSQL clsSQL = new clsSQL();
                if (trn == null) { 
                
                return Simulate.String(clsSQL.ExecuteScalar(a, prm, clsSQL.CreateDataBaseConnectionString(CompanyID)));
                
                } else {

                    return Simulate.String(clsSQL.ExecuteScalar(a, prm, clsSQL.CreateDataBaseConnectionString(CompanyID),trn));

                }

            }
            catch (Exception ex)
            {

                throw;
            }


        }
        public int UpdateItems(string Guid, string AName, string EName, string Description, decimal SalesPriceBeforeTax, decimal SalesPriceAfterTax, int CategoryID, int SalesTaxID
            , int SpecialSalesTaxID, int PurchaseTaxID, int SpecialPurchaseTaxID, string Barcode, int ReadType, int OriginID, decimal MinimumLimit, byte[] Picture
            , bool IsActive, bool IsPOS, int BoxTypeID, bool IsStockItem, int POSOrder, bool TrackLot,bool TrackSerial,
            bool TrackExpiryDate,
                        string ItemCode, int ItemTypeID,
            int BrandID, int ManufacturerID, string ModelNo,
            int BaseUOMID, int SalesUOMID, int PurchaseUOMID,
            decimal StandardCost, decimal LastPurchaseCost,
            bool IsWeightedItem, bool IsOpenPrice,
            bool AllowNegativeStock,
            int ShelfLifeDays, int ExpiryWarningDays, string ParentGuid,
decimal BaseFactor,

            int ModificationUserId,int CompanyID,SqlTransaction trn=null,
            bool ShowOnWeb = false, decimal WebPrice = 0,
            bool WebAllowCustomNote = false, bool WebHasSize = false, bool WebHasColor = false,
            string WebSizeOptions = "", string WebColorOptions = "")
        {
            try
            {
                if ( ParentGuid == Simulate.String(Guid))
                    ParentGuid = null; 
                clsSQL clsSQL = new clsSQL();

                SqlParameter[] prm =
                 {
          
                     new SqlParameter("@Guid", SqlDbType.UniqueIdentifier) { Value =Simulate.Guid( Guid) },
               new SqlParameter("@AName", SqlDbType.NVarChar,-1) { Value = AName },
                  new SqlParameter("@EName", SqlDbType.NVarChar,-1) { Value = EName },
                  new SqlParameter("@Description", SqlDbType.NVarChar,-1) { Value = Description },
                  new SqlParameter("@SalesPriceBeforeTax", SqlDbType.Decimal) { Value = SalesPriceBeforeTax },
                    new SqlParameter("@SalesPriceAfterTax", SqlDbType.Decimal) { Value = SalesPriceAfterTax },
                  new SqlParameter("@CategoryID", SqlDbType.Int) { Value = CategoryID },
                  new SqlParameter("@SalesTaxID", SqlDbType.Int) { Value = SalesTaxID },
                  new SqlParameter("@SpecialSalesTaxID", SqlDbType.Int) { Value = SpecialSalesTaxID },
                  new SqlParameter("@PurchaseTaxID", SqlDbType.Int) { Value = PurchaseTaxID },
                  new SqlParameter("@SpecialPurchaseTaxID", SqlDbType.Int) { Value = SpecialPurchaseTaxID },
                  new SqlParameter("@Barcode", SqlDbType.NVarChar,-1) { Value = Barcode },
                  new SqlParameter("@ReadType", SqlDbType.Int) { Value = ReadType },
                  new SqlParameter("@OriginID", SqlDbType.Int) { Value = OriginID },
                  new SqlParameter("@MinimumLimit", SqlDbType.Decimal) { Value = MinimumLimit },
                  new SqlParameter("@Picture", SqlDbType.Image) { Value = Picture },
                  new SqlParameter("@IsActive", SqlDbType.Bit) { Value = IsActive },
                  new SqlParameter("@IsPOS", SqlDbType.Bit) { Value = IsPOS },
                  new SqlParameter("@BoxTypeID", SqlDbType.Int) { Value = BoxTypeID },
                  new SqlParameter("@IsStockItem", SqlDbType.Bit) { Value = IsStockItem },
                  new SqlParameter("@POSOrder", SqlDbType.Int) { Value = POSOrder },
                    // flags MUST be BIT
                    new SqlParameter("@TrackLot", SqlDbType.Bit) { Value = TrackLot },
                    new SqlParameter("@TrackSerial", SqlDbType.Bit) { Value = TrackSerial },
                    new SqlParameter("@TrackExpiryDate", SqlDbType.Bit) { Value = TrackExpiryDate },

                    // ---- New columns
                    new SqlParameter("@ItemCode", SqlDbType.NVarChar, -1) { Value = ItemCode ?? "" },
                    new SqlParameter("@ItemTypeID", SqlDbType.Int) { Value = ItemTypeID },

                    new SqlParameter("@BrandID", SqlDbType.Int) { Value = BrandID },
                    new SqlParameter("@ManufacturerID", SqlDbType.Int) { Value = ManufacturerID },
                    new SqlParameter("@ModelNo", SqlDbType.NVarChar, -1) { Value = ModelNo ?? "" },

                    new SqlParameter("@BaseUOMID", SqlDbType.Int) { Value = BaseUOMID },
                    new SqlParameter("@SalesUOMID", SqlDbType.Int) { Value = SalesUOMID },
                    new SqlParameter("@PurchaseUOMID", SqlDbType.Int) { Value = PurchaseUOMID },

                    new SqlParameter("@StandardCost", SqlDbType.Decimal) { Value = StandardCost },
                    new SqlParameter("@LastPurchaseCost", SqlDbType.Decimal) { Value = LastPurchaseCost },

                    new SqlParameter("@IsWeightedItem", SqlDbType.Bit) { Value = IsWeightedItem },
                    new SqlParameter("@IsOpenPrice", SqlDbType.Bit) { Value = IsOpenPrice },
                    new SqlParameter("@AllowNegativeStock", SqlDbType.Bit) { Value = AllowNegativeStock },

                    new SqlParameter("@ShelfLifeDays", SqlDbType.Int) { Value = ShelfLifeDays },
                    new SqlParameter("@ExpiryWarningDays", SqlDbType.Int) { Value = ExpiryWarningDays },

                         new SqlParameter("@ModificationUserId", SqlDbType.Int) { Value = ModificationUserId },
                     new SqlParameter("@ModificationDate", SqlDbType.DateTime) { Value = DateTime.Now },
                     new SqlParameter("@ParentGuid", SqlDbType.UniqueIdentifier){ Value =Simulate.Guid(ParentGuid   )},
                     new SqlParameter("@BaseFactor", SqlDbType.Decimal){ Value = BaseFactor <= 0 ? 1 : BaseFactor },
                     new SqlParameter("@ShowOnWeb", SqlDbType.Bit) { Value = ShowOnWeb },
                     new SqlParameter("@WebPrice", SqlDbType.Decimal) { Value = WebPrice },
                     new SqlParameter("@WebAllowCustomNote", SqlDbType.Bit) { Value = WebAllowCustomNote },
                     new SqlParameter("@WebHasSize", SqlDbType.Bit) { Value = WebHasSize },
                     new SqlParameter("@WebHasColor", SqlDbType.Bit) { Value = WebHasColor },
                     new SqlParameter("@WebSizeOptions", SqlDbType.NVarChar, 500) { Value = WebSizeOptions ?? "" },
                     new SqlParameter("@WebColorOptions", SqlDbType.NVarChar, 500) { Value = WebColorOptions ?? "" },
                };
              
                int A = clsSQL.ExecuteNonQueryStatement(@"update tbl_Items set 
                       AName=@AName,
                       EName=@EName, 
                       Description=@Description,
                       SalesPriceBeforeTax=@SalesPriceBeforeTax,
                       SalesPriceAfterTax=@SalesPriceAfterTax,
                       CategoryID=@CategoryID,
                       SalesTaxID=@SalesTaxID,
                       SpecialSalesTaxID=@SpecialSalesTaxID,
                       PurchaseTaxID=@PurchaseTaxID,
                       SpecialPurchaseTaxID=@SpecialPurchaseTaxID,
                       Barcode=@Barcode,
                       ReadType=@ReadType,
                       OriginID=@OriginID,
                       MinimumLimit=@MinimumLimit,
                       Picture=@Picture,
                       IsActive=@IsActive,
                       IsPOS=@IsPOS,
                       BoxTypeID=@BoxTypeID,
                       IsStockItem=@IsStockItem,
                       POSOrder=@POSOrder,
                     TrackExpiryDate=@TrackExpiryDate,
                      TrackSerial=@TrackSerial,
                      TrackLot=@TrackLot,
                      

    ItemCode=@ItemCode,
    ItemTypeID=@ItemTypeID,
    BrandID=@BrandID,
    ManufacturerID=@ManufacturerID,
    ModelNo=@ModelNo,
    BaseUOMID=@BaseUOMID,
    SalesUOMID=@SalesUOMID,
    PurchaseUOMID=@PurchaseUOMID,
    StandardCost=@StandardCost,
    LastPurchaseCost=@LastPurchaseCost,
    IsWeightedItem=@IsWeightedItem,
    IsOpenPrice=@IsOpenPrice,
    AllowNegativeStock=@AllowNegativeStock,
    ShelfLifeDays=@ShelfLifeDays,
    ExpiryWarningDays=@ExpiryWarningDays,
ParentGuid=@ParentGuid,
BaseFactor=@BaseFactor,
ShowOnWeb=@ShowOnWeb,
WebPrice=@WebPrice,
WebAllowCustomNote=@WebAllowCustomNote,
WebHasSize=@WebHasSize,
WebHasColor=@WebHasColor,
WebSizeOptions=@WebSizeOptions,
WebColorOptions=@WebColorOptions,
                       ModificationDate=@ModificationDate,
                       ModificationUserId=@ModificationUserId
                   where Guid =@Guid", clsSQL.CreateDataBaseConnectionString(CompanyID), prm, trn);

                return A;
            }
            catch (Exception)
            {

                throw;
            }


        }
    }
}
