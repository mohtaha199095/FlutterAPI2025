using System;
using System.Data;
using Microsoft.Data.SqlClient;

namespace WebApplication2.cls
{
    public class clsItems1
    {
   
        // ==========================================================
        // Update Item AVG Cost Per Unit (Weighted Avg) داخل ترانزاكشن
        // ==========================================================
        public DataTable UpdateItemCost(string Itemguid, decimal addedQTY, decimal newcostPerUnit, int CompanyId, SqlTransaction trn)
        {
            try
            {
                clsSQL clsSQL = new clsSQL();

                SqlParameter[] prm =
                {
                    new SqlParameter("@Itemguid", SqlDbType.UniqueIdentifier) { Value = Simulate.Guid(Itemguid) },
                    new SqlParameter("@CompanyId", SqlDbType.Int) { Value = CompanyId },
                };

                DataTable dt = clsSQL.ExecuteQueryStatement(@"
select 
    sum(QTYFactor * TotalQTY) as TotalQTY,
    tbl_Items.AVGCostPerUnit
from tbl_InvoiceDetails
left join tbl_JournalVoucherTypes on tbl_InvoiceDetails.InvoiceTypeID = tbl_JournalVoucherTypes.id
left join tbl_Items on tbl_Items.Guid = tbl_InvoiceDetails.ItemGuid
where IsCounted = 1 
  and ItemGuid = @Itemguid
  and (tbl_InvoiceDetails.CompanyId = @CompanyId or @CompanyId = 0)
group by tbl_Items.AVGCostPerUnit
", clsSQL.CreateDataBaseConnectionString(CompanyId), prm, trn);

                if (dt != null && dt.Rows.Count > 0)
                {
                    decimal rowqty = 0;
                    decimal totalQty = Simulate.decimal_(dt.Rows[0]["TotalQTY"]);

                    if (totalQty > 0)
                        rowqty = totalQty - addedQTY;

                    decimal oldAvg = Simulate.decimal_(dt.Rows[0]["AVGCostPerUnit"]);

                    // Weighted average after adding new stock
                    decimal newCostAfteraddition = 0;
                    if ((addedQTY + rowqty) > 0)
                        newCostAfteraddition = ((rowqty * oldAvg) + (newcostPerUnit * addedQTY)) / (addedQTY + rowqty);

                    SqlParameter[] prm1 =
                    {
                        new SqlParameter("@Itemguid", SqlDbType.UniqueIdentifier) { Value = Simulate.Guid(Itemguid) },
                        new SqlParameter("@newcost", SqlDbType.Decimal) { Value = newCostAfteraddition },
                        new SqlParameter("@CompanyId", SqlDbType.Int) { Value = CompanyId },
                    };

                    clsSQL.ExecuteNonQueryStatement(
                        @"update tbl_Items 
                          set AVGCostPerUnit = @newcost 
                          where guid = @Itemguid and (CompanyId = @CompanyId or @CompanyId = 0)",
                        clsSQL.CreateDataBaseConnectionString(CompanyId),
                        prm1,
                        trn
                    );
                }

                return dt;
            }
            catch
            {
                throw;
            }
        }

        // ==========================================================
        // Select Items
        // ==========================================================
        public DataTable SelectItemsByGuid(string guid, string AName, string EName, string Barcode, int CategoryID, int IsPOS, int CompanyId, SqlTransaction trn = null)
        {
            try
            {
                clsSQL clsSQL = new clsSQL();

                SqlParameter[] prm =
                {
                    new SqlParameter("@guid", SqlDbType.UniqueIdentifier) { Value = Simulate.Guid(guid) },
                    new SqlParameter("@AName", SqlDbType.NVarChar, -1) { Value = AName ?? "" },
                    new SqlParameter("@EName", SqlDbType.NVarChar, -1) { Value = EName ?? "" },
                    new SqlParameter("@Barcode", SqlDbType.NVarChar, -1) { Value = Barcode ?? "" },
                    new SqlParameter("@CategoryID", SqlDbType.Int) { Value = CategoryID },
                    new SqlParameter("@IsPOS", SqlDbType.Int) { Value = IsPOS },
                    new SqlParameter("@CompanyId", SqlDbType.Int) { Value = CompanyId },
                };

                DataTable dt = clsSQL.ExecuteQueryStatement(@"
select * 
from tbl_Items 
where (guid = @guid or @guid = '00000000-0000-0000-0000-000000000000')
  and (AName = @AName or @AName = '')
  and (EName = @EName or @EName = '')
  and (CategoryID = @CategoryID or @CategoryID = 0)
  and (IsPOS = @IsPOS or @IsPOS = -1)
  and (Barcode = @Barcode or @Barcode = '')
  and (CompanyId = @CompanyId or @CompanyId = 0)
", clsSQL.CreateDataBaseConnectionString(CompanyId), prm, trn);

                return dt;
            }
            catch
            {
                throw;
            }
        }

        // ==========================================================
        // Delete Item
        // ==========================================================
        public bool DeleteItemsByGuid(string Guid, int CompanyID)
        {
            try
            {
                clsSQL clsSQL = new clsSQL();

                SqlParameter[] prm =
                {
                    new SqlParameter("@Guid", SqlDbType.UniqueIdentifier) { Value = Simulate.Guid(Guid) },
                };

                clsSQL.ExecuteNonQueryStatement(
                    @"delete from tbl_Items where (Guid = @Guid)",
                    clsSQL.CreateDataBaseConnectionString(CompanyID),
                    prm
                );

                return true;
            }
            catch
            {
                throw;
            }
        }

        // ==========================================================
        // Insert Item (with NEW columns)
        // ==========================================================
        public string InsertItems(
            string AName, string EName, string Description,
            decimal SalesPriceBeforeTax, decimal SalesPriceAfterTax,
            int CategoryID, int SalesTaxID, int SpecialSalesTaxID,
            int PurchaseTaxID, int SpecialPurchaseTaxID,
            string Barcode, int ReadType, int OriginID,
            decimal MinimumLimit, byte[] Picture,
            bool IsActive, bool IsPOS, int BoxTypeID, bool IsStockItem, int POSOrder,
            bool TrackLot, bool TrackSerial, bool TrackExpiryDate,

            // NEW
            string ItemCode, int ItemTypeID,
            int BrandID, int ManufacturerID, string ModelNo,
            int BaseUOMID, int SalesUOMID, int PurchaseUOMID,
            decimal StandardCost, decimal LastPurchaseCost,
            bool IsWeightedItem, bool IsOpenPrice,
            bool AllowNegativeStock,
            int ShelfLifeDays, int ExpiryWarningDays,

            int CompanyID, int CreationUserId,
            SqlTransaction trn = null
        )
        {
            try
            {
                SqlParameter picPrm = new SqlParameter("@Picture", SqlDbType.Image);
                if (Picture == null || Picture.Length == 0)
                    picPrm.Value = DBNull.Value;
                else
                    picPrm.Value = Picture;

                SqlParameter[] prm =
                {
                    new SqlParameter("@AName", SqlDbType.NVarChar, -1) { Value = AName ?? "" },
                    new SqlParameter("@EName", SqlDbType.NVarChar, -1) { Value = EName ?? "" },
                    new SqlParameter("@Description", SqlDbType.NVarChar, -1) { Value = Description ?? "" },
                    new SqlParameter("@SalesPriceBeforeTax", SqlDbType.Decimal) { Value = SalesPriceBeforeTax },
                    new SqlParameter("@SalesPriceAfterTax", SqlDbType.Decimal) { Value = SalesPriceAfterTax },
                    new SqlParameter("@CategoryID", SqlDbType.Int) { Value = CategoryID },
                    new SqlParameter("@SalesTaxID", SqlDbType.Int) { Value = SalesTaxID },
                    new SqlParameter("@SpecialSalesTaxID", SqlDbType.Int) { Value = SpecialSalesTaxID },
                    new SqlParameter("@PurchaseTaxID", SqlDbType.Int) { Value = PurchaseTaxID },
                    new SqlParameter("@SpecialPurchaseTaxID", SqlDbType.Int) { Value = SpecialPurchaseTaxID },
                    new SqlParameter("@Barcode", SqlDbType.NVarChar, -1) { Value = Barcode ?? "" },
                    new SqlParameter("@ReadType", SqlDbType.Int) { Value = ReadType },
                    new SqlParameter("@OriginID", SqlDbType.Int) { Value = OriginID },
                    new SqlParameter("@MinimumLimit", SqlDbType.Decimal) { Value = MinimumLimit },

                    picPrm,

                    new SqlParameter("@IsActive", SqlDbType.Bit) { Value = IsActive },
                    new SqlParameter("@IsPOS", SqlDbType.Bit) { Value = IsPOS },
                    new SqlParameter("@BoxTypeID", SqlDbType.Int) { Value = BoxTypeID },
                    new SqlParameter("@IsStockItem", SqlDbType.Bit) { Value = IsStockItem },
                    new SqlParameter("@POSOrder", SqlDbType.Int) { Value = POSOrder },

                    // ✅ FIXED: flags MUST be BIT
                    new SqlParameter("@TrackLot", SqlDbType.Bit) { Value = TrackLot },
                    new SqlParameter("@TrackSerial", SqlDbType.Bit) { Value = TrackSerial },
                    new SqlParameter("@TrackExpiryDate", SqlDbType.Bit) { Value = TrackExpiryDate },

                    // NEW columns
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
                };

                string q = @"
insert into tbl_Items
(
    AName,EName,Description,SalesPriceBeforeTax,SalesPriceAfterTax,CategoryID,SalesTaxID,SpecialSalesTaxID,PurchaseTaxID,
    SpecialPurchaseTaxID,Barcode,ReadType,OriginID,MinimumLimit,Picture,IsActive,IsPOS,BoxTypeID,IsStockItem,
    POSOrder,CompanyID,CreationUserId,CreationDate,TrackLot,TrackSerial,TrackExpiryDate,

    ItemCode,ItemTypeID,
    BrandID,ManufacturerID,ModelNo,
    BaseUOMID,SalesUOMID,PurchaseUOMID,
    StandardCost,LastPurchaseCost,
    IsWeightedItem,IsOpenPrice,AllowNegativeStock,
    ShelfLifeDays,ExpiryWarningDays
)
OUTPUT INSERTED.Guid
values
(
    @AName,@EName,@Description,@SalesPriceBeforeTax,@SalesPriceAfterTax,@CategoryID,@SalesTaxID,@SpecialSalesTaxID,@PurchaseTaxID,
    @SpecialPurchaseTaxID,@Barcode,@ReadType,@OriginID,@MinimumLimit,@Picture,@IsActive,@IsPOS,@BoxTypeID,@IsStockItem,
    @POSOrder,@CompanyID,@CreationUserId,@CreationDate,@TrackLot,@TrackSerial,@TrackExpiryDate,

    @ItemCode,@ItemTypeID,
    @BrandID,@ManufacturerID,@ModelNo,
    @BaseUOMID,@SalesUOMID,@PurchaseUOMID,
    @StandardCost,@LastPurchaseCost,
    @IsWeightedItem,@IsOpenPrice,@AllowNegativeStock,
    @ShelfLifeDays,@ExpiryWarningDays
)
";

                clsSQL clsSQL = new clsSQL();

                if (trn == null)
                    return Simulate.String(clsSQL.ExecuteScalar(q, prm, clsSQL.CreateDataBaseConnectionString(CompanyID)));
                else
                    return Simulate.String(clsSQL.ExecuteScalar(q, prm, clsSQL.CreateDataBaseConnectionString(CompanyID), trn));
            }
            catch
            {
                throw;
            }
        }

        // ==========================================================
        // Update Item (with NEW columns)
        // ==========================================================
        public int UpdateItems(
            string Guid,
            string AName, string EName, string Description,
            decimal SalesPriceBeforeTax, decimal SalesPriceAfterTax,
            int CategoryID, int SalesTaxID, int SpecialSalesTaxID,
            int PurchaseTaxID, int SpecialPurchaseTaxID,
            string Barcode, int ReadType, int OriginID,
            decimal MinimumLimit, byte[] Picture,
            bool IsActive, bool IsPOS, int BoxTypeID, bool IsStockItem, int POSOrder,
            bool TrackLot, bool TrackSerial, bool TrackExpiryDate,

            // NEW
            string ItemCode, int ItemTypeID,
            int BrandID, int ManufacturerID, string ModelNo,
            int BaseUOMID, int SalesUOMID, int PurchaseUOMID,
            decimal StandardCost, decimal LastPurchaseCost,
            bool IsWeightedItem, bool IsOpenPrice,
            bool AllowNegativeStock,
            int ShelfLifeDays, int ExpiryWarningDays,

            int ModificationUserId, int CompanyID,
            SqlTransaction trn = null
        )
        {
            try
            {
                clsSQL clsSQL = new clsSQL();

                SqlParameter picPrm = new SqlParameter("@Picture", SqlDbType.Image);
                if (Picture == null || Picture.Length == 0)
                    picPrm.Value = DBNull.Value;
                else
                    picPrm.Value = Picture;

                SqlParameter[] prm =
                {
                    new SqlParameter("@Guid", SqlDbType.UniqueIdentifier) { Value = Simulate.Guid(Guid) },

                    new SqlParameter("@AName", SqlDbType.NVarChar, -1) { Value = AName ?? "" },
                    new SqlParameter("@EName", SqlDbType.NVarChar, -1) { Value = EName ?? "" },
                    new SqlParameter("@Description", SqlDbType.NVarChar, -1) { Value = Description ?? "" },
                    new SqlParameter("@SalesPriceBeforeTax", SqlDbType.Decimal) { Value = SalesPriceBeforeTax },
                    new SqlParameter("@SalesPriceAfterTax", SqlDbType.Decimal) { Value = SalesPriceAfterTax },
                    new SqlParameter("@CategoryID", SqlDbType.Int) { Value = CategoryID },
                    new SqlParameter("@SalesTaxID", SqlDbType.Int) { Value = SalesTaxID },
                    new SqlParameter("@SpecialSalesTaxID", SqlDbType.Int) { Value = SpecialSalesTaxID },
                    new SqlParameter("@PurchaseTaxID", SqlDbType.Int) { Value = PurchaseTaxID },
                    new SqlParameter("@SpecialPurchaseTaxID", SqlDbType.Int) { Value = SpecialPurchaseTaxID },
                    new SqlParameter("@Barcode", SqlDbType.NVarChar, -1) { Value = Barcode ?? "" },
                    new SqlParameter("@ReadType", SqlDbType.Int) { Value = ReadType },
                    new SqlParameter("@OriginID", SqlDbType.Int) { Value = OriginID },
                    new SqlParameter("@MinimumLimit", SqlDbType.Decimal) { Value = MinimumLimit },

                    picPrm,

                    new SqlParameter("@IsActive", SqlDbType.Bit) { Value = IsActive },
                    new SqlParameter("@IsPOS", SqlDbType.Bit) { Value = IsPOS },
                    new SqlParameter("@BoxTypeID", SqlDbType.Int) { Value = BoxTypeID },
                    new SqlParameter("@IsStockItem", SqlDbType.Bit) { Value = IsStockItem },
                    new SqlParameter("@POSOrder", SqlDbType.Int) { Value = POSOrder },

                    // ✅ flags MUST be BIT
                    new SqlParameter("@TrackLot", SqlDbType.Bit) { Value = TrackLot },
                    new SqlParameter("@TrackSerial", SqlDbType.Bit) { Value = TrackSerial },
                    new SqlParameter("@TrackExpiryDate", SqlDbType.Bit) { Value = TrackExpiryDate },

                    // NEW columns
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
                };

                int A = clsSQL.ExecuteNonQueryStatement(@"
update tbl_Items set 
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

    ModificationDate=@ModificationDate,
    ModificationUserId=@ModificationUserId
where Guid=@Guid
", clsSQL.CreateDataBaseConnectionString(CompanyID), prm, trn);

                return A;
            }
            catch
            {
                throw;
            }
        }
    }
}
