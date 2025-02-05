using System;
using System.Data;
using System.Data.SqlClient;

namespace WebApplication2.cls
{
    public class clsItems
    {
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

        public DataTable SelectItemsByGuid(string guid, string AName, string EName, String Barcode, int CategoryID, int IsPOS, int CompanyId)
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
                     ", clsSQL.CreateDataBaseConnectionString(CompanyId), prm);

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
            , bool IsActive, bool IsPOS, int BoxTypeID, bool IsStockItem, int POSOrder, int CompanyID, int CreationUserId)
        {
            try
            {
                SqlParameter[] prm =
                 {
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
                  new SqlParameter("@CompanyID", SqlDbType.Int) { Value = CompanyID },
                  new SqlParameter("@CreationUserId", SqlDbType.Int) { Value = CreationUserId },
                  new SqlParameter("@CreationDate", SqlDbType.DateTime) { Value = DateTime.Now },
                };

                string a = @"insert into tbl_Items(AName,EName,Description,SalesPriceBeforeTax,SalesPriceAfterTax,CategoryID,SalesTaxID,SpecialSalesTaxID,PurchaseTaxID
 ,SpecialPurchaseTaxID ,Barcode,ReadType ,OriginID,MinimumLimit ,Picture,IsActive ,IsPOS,BoxTypeID,IsStockItem,POSOrder,CompanyID,CreationUserId,CreationDate)
                        OUTPUT INSERTED.guid values(@AName,@EName,@Description,@SalesPriceBeforeTax,@SalesPriceAfterTax,@CategoryID,@SalesTaxID,@SpecialSalesTaxID,@PurchaseTaxID
, @SpecialPurchaseTaxID ,@Barcode,@ReadType,@OriginID,@MinimumLimit,@Picture,@IsActive,@IsPOS,@BoxTypeID,@IsStockItem,@POSOrder,@CompanyID,@CreationUserId,@CreationDate)";
                clsSQL clsSQL = new clsSQL();

                return Simulate.String(clsSQL.ExecuteScalar(a, prm, clsSQL.CreateDataBaseConnectionString(CompanyID)));

            }
            catch (Exception ex)
            {

                throw;
            }


        }
        public int UpdateItems(string Guid, string AName, string EName, string Description, decimal SalesPriceBeforeTax, decimal SalesPriceAfterTax, int CategoryID, int SalesTaxID
            , int SpecialSalesTaxID, int PurchaseTaxID, int SpecialPurchaseTaxID, string Barcode, int ReadType, int OriginID, decimal MinimumLimit, byte[] Picture
            , bool IsActive, bool IsPOS, int BoxTypeID, bool IsStockItem, int POSOrder, int ModificationUserId,int CompanyID)
        {
            try
            {
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

                         new SqlParameter("@ModificationUserId", SqlDbType.Int) { Value = ModificationUserId },
                     new SqlParameter("@ModificationDate", SqlDbType.DateTime) { Value = DateTime.Now },
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
                     
                      
                       ModificationDate=@ModificationDate,
                       ModificationUserId=@ModificationUserId
                   where Guid =@Guid", clsSQL.CreateDataBaseConnectionString(CompanyID), prm);

                return A;
            }
            catch (Exception)
            {

                throw;
            }


        }
    }
}
