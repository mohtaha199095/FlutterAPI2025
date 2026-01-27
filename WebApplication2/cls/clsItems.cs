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
    
        public DataTable UpdateItemCost(string Itemguid,decimal addedQTY, decimal newcostPerUnit,int CompanyId,SqlTransaction trn)
        {
            try
            {



          



                clsSQL clsSQL = new clsSQL();

                SqlParameter[] prm =
                 { new SqlParameter("@Itemguid", SqlDbType.UniqueIdentifier) { Value =Simulate.Guid( Itemguid )},
      new SqlParameter("@newcost", SqlDbType.Decimal) { Value = newcostPerUnit },
      
           new SqlParameter("@CompanyId", SqlDbType.Int) { Value = CompanyId },
                };
                DataTable dt = clsSQL.ExecuteQueryStatement(@"select sum( QTYFactor* TotalQTY ) as TotalQTY ,tbl_Items.AVGCostPerUnit from tbl_InvoiceDetails 
left join tbl_JournalVoucherTypes on tbl_InvoiceDetails.InvoiceTypeID = tbl_JournalVoucherTypes.id
left join tbl_Items on tbl_Items.Guid = tbl_InvoiceDetails.ItemGuid
where IsCounted = 1 and ItemGuid = @Itemguid   and (tbl_InvoiceDetails.CompanyId=@CompanyId or @CompanyId=0  )  
                    group by tbl_Items.AVGCostPerUnit ", clsSQL.CreateDataBaseConnectionString(CompanyId), prm, trn);
                if (dt != null && dt.Rows.Count > 0 )
                {
                    decimal rowqty = 0;
                    if ( Simulate.decimal_(dt.Rows[0]["TotalQTY"]) > 0) {
                        rowqty = Simulate.decimal_(dt.Rows[0]["TotalQTY"])- addedQTY;


                    }

                    decimal newCostAfteraddition = ((rowqty * Simulate.decimal_(dt.Rows[0]["AVGCostPerUnit"])) + (newcostPerUnit* addedQTY))/ (addedQTY+ rowqty);



                    SqlParameter[] prm1 =
                     { new SqlParameter("@Itemguid", SqlDbType.UniqueIdentifier) { Value =Simulate.Guid( Itemguid )},
      new SqlParameter("@newcost", SqlDbType.Decimal) { Value = newCostAfteraddition },

           new SqlParameter("@CompanyId", SqlDbType.Int) { Value = CompanyId },
                };
                    clsSQL.ExecuteNonQueryStatement("update tbl_Items set AVGCostPerUnit =@newcost where guid =@Itemguid  and (CompanyId=@CompanyId or @CompanyId=0  ) ", clsSQL.CreateDataBaseConnectionString(CompanyId), prm1,trn);
                }
                return dt;
            }
            catch (Exception ex)
            {

                throw;
            }


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
                     ", clsSQL.CreateDataBaseConnectionString(CompanyId), prm, trn);

                return dt;
            }
            catch (Exception)
            {

                throw;
            }


        }

        public bool DeleteItemsByGuid(string Guid,int CompanyID)
        {
            try
            {
                clsSQL clsSQL = new clsSQL();
               
                SqlParameter[] prm =
                 { new SqlParameter("@Guid", SqlDbType.UniqueIdentifier) { Value =Simulate.Guid( Guid) },

                };
                int A = clsSQL.ExecuteNonQueryStatement(@"delete from tbl_Items where (Guid=@Guid  )", clsSQL.CreateDataBaseConnectionString(CompanyID), prm);

                return true;
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
            int CompanyID, int CreationUserId,SqlTransaction trn=null)
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
                };

                string a = @"insert into tbl_Items(AName,EName,Description,SalesPriceBeforeTax,SalesPriceAfterTax,CategoryID,SalesTaxID,SpecialSalesTaxID,PurchaseTaxID
 ,SpecialPurchaseTaxID ,Barcode,ReadType ,OriginID,MinimumLimit ,Picture,IsActive ,IsPOS,BoxTypeID,IsStockItem,
POSOrder,CompanyID,CreationUserId,CreationDate,TrackLot,TrackSerial,TrackExpiryDate,
ItemCode,ItemTypeID,
    BrandID,ManufacturerID,ModelNo,
    BaseUOMID,SalesUOMID,PurchaseUOMID,
    StandardCost,LastPurchaseCost,
    IsWeightedItem,IsOpenPrice,AllowNegativeStock,
    ShelfLifeDays,ExpiryWarningDays ,ParentGuid, BaseFactor

)
                        OUTPUT INSERTED.guid values(@AName,@EName,@Description,@SalesPriceBeforeTax,@SalesPriceAfterTax,@CategoryID,@SalesTaxID,@SpecialSalesTaxID,@PurchaseTaxID
, @SpecialPurchaseTaxID ,@Barcode,@ReadType,@OriginID,@MinimumLimit,@Picture,@IsActive,@IsPOS,@BoxTypeID,
@IsStockItem,@POSOrder,@CompanyID,@CreationUserId,@CreationDate,@TrackLot,@TrackSerial,@TrackExpiryDate,
    @ItemCode,@ItemTypeID,
    @BrandID,@ManufacturerID,@ModelNo,
    @BaseUOMID,@SalesUOMID,@PurchaseUOMID,
    @StandardCost,@LastPurchaseCost,
    @IsWeightedItem,@IsOpenPrice,@AllowNegativeStock,
    @ShelfLifeDays,@ExpiryWarningDays,@ParentGuid, @BaseFactor

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

            int ModificationUserId,int CompanyID,SqlTransaction trn=null)
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
