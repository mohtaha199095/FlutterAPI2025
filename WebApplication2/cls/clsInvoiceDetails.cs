using System;
using System.Collections.Generic;
using System.Data;
using DocumentFormat.OpenXml.Office2019.Presentation;
using Microsoft.Data.SqlClient;
using WebApplication2.MainClasses;

namespace WebApplication2.cls
{
    public class clsInvoiceDetails
    {

        public DataTable SelectInvoiceDetailsByHeaderGuid(string HeaderGuid, string ItemGuid, int CompanyID)
        {
            try
            {
                clsSQL clsSQL = new clsSQL();

                SqlParameter[] prm =
                 { new SqlParameter("@HeaderGuid", SqlDbType.UniqueIdentifier) { Value = Simulate.Guid( HeaderGuid) },
                  new SqlParameter("@ItemGuid", SqlDbType.UniqueIdentifier) { Value = Simulate.Guid( ItemGuid) },
        new SqlParameter("@CompanyID", SqlDbType.Int) { Value = CompanyID },

                };
                DataTable dt = clsSQL.ExecuteQueryStatement(@"select * from tbl_InvoiceDetails where (ItemGuid=@ItemGuid or @ItemGuid='00000000-0000-0000-0000-000000000000' ) and (HeaderGuid=@HeaderGuid or @HeaderGuid='00000000-0000-0000-0000-000000000000' )    and (CompanyID=@CompanyID or @CompanyID=0  )order by rowindex asc
                     ", clsSQL.CreateDataBaseConnectionString(CompanyID), prm);

                return dt;
            }
            catch (Exception ex)
            {

                throw;
            }


        }

        public bool DeleteInvoiceDetailsByHeaderGuid(string HeaderGuid, int CompanyID,SqlTransaction trn)
        {
            try
            {
                clsSQL clsSQL = new clsSQL();

                SqlParameter[] prm =
                 { new SqlParameter("@HeaderGuid", SqlDbType.UniqueIdentifier) { Value = Simulate.Guid( HeaderGuid) },

                };
                int A = clsSQL.ExecuteNonQueryStatement(@"delete from tbl_InvoiceDetails where (HeaderGuid=@HeaderGuid  )", clsSQL.CreateDataBaseConnectionString(CompanyID), prm, trn);

                return true;
            }
            catch (Exception ex)
            {

                return false;
            }


        }
        public string InsertInvoiceDetails(DBInvoiceDetails dBInvoiceDetails, string HeaderGuid, SqlTransaction trn)
        {
            try
            {
                clsSQL clsSQL = new clsSQL();
                if (dBInvoiceDetails.InvoiceTypeID == (int)clsEnum.VoucherType.PurchaseInvoice)
                {
                    dBInvoiceDetails.AVGCostPerUnit = dBInvoiceDetails.PriceBeforeTax - dBInvoiceDetails.DiscountBeforeTaxAmountPcs;

                }
                else if (dBInvoiceDetails.IsCounted)
                {
                    SqlParameter[] aa = { };
                    DataTable dt = clsSQL.ExecuteQueryStatement("select * from tbl_items where guid = '" + dBInvoiceDetails.ItemGuid+"'", clsSQL.CreateDataBaseConnectionString(dBInvoiceDetails.CompanyID), aa, trn);
                
                if(dt != null && dt.Rows.Count>0)
                                 {

                        dBInvoiceDetails.AVGCostPerUnit = Simulate.decimal_(dt.Rows[0]["AVGCostPerUnit"]);
                    }
                }

                SqlParameter[] prm =
                { new SqlParameter("@HeaderGuid", SqlDbType.UniqueIdentifier) { Value = Simulate.Guid(HeaderGuid)},
                    new SqlParameter("@ItemGuid", SqlDbType.UniqueIdentifier) { Value = dBInvoiceDetails.ItemGuid },
                    new SqlParameter("@RowIndex", SqlDbType.Int) { Value = dBInvoiceDetails.RowIndex },

                    new SqlParameter("@ItemName", SqlDbType.NVarChar,-1) { Value = dBInvoiceDetails.ItemName },
                    new SqlParameter("@Qty", SqlDbType.Decimal) { Value = dBInvoiceDetails.Qty },
                    new SqlParameter("@PriceBeforeTax", SqlDbType.Decimal) { Value = dBInvoiceDetails.PriceBeforeTax },
                    new SqlParameter("@DiscountBeforeTaxAmountAll", SqlDbType.Decimal) { Value = dBInvoiceDetails.DiscountBeforeTaxAmountAll },
                    new SqlParameter("@DiscountBeforeTaxAmountPcs", SqlDbType.Decimal) { Value = dBInvoiceDetails.DiscountBeforeTaxAmountPcs },


                    new SqlParameter("@TaxID", SqlDbType.Int) { Value = dBInvoiceDetails.TaxID },
                    new SqlParameter("@TaxPercentage", SqlDbType.Decimal) { Value = dBInvoiceDetails.TaxPercentage },
                    new SqlParameter("@TaxAmount", SqlDbType.Decimal) { Value = dBInvoiceDetails.TaxAmount },
                    new SqlParameter("@SpecialTaxID", SqlDbType.Int) { Value = dBInvoiceDetails.SpecialTaxID },
                    new SqlParameter("@SpecialTaxPercentage", SqlDbType.Decimal) { Value = dBInvoiceDetails.SpecialTaxPercentage },
                    new SqlParameter("@SpecialTaxAmount", SqlDbType.Decimal) { Value = dBInvoiceDetails.SpecialTaxAmount },
                    new SqlParameter("@DiscountAfterTaxAmountAll", SqlDbType.Decimal) { Value = dBInvoiceDetails.DiscountAfterTaxAmountAll },
                    new SqlParameter("@DiscountAfterTaxAmountPcs", SqlDbType.Decimal) { Value = dBInvoiceDetails.DiscountAfterTaxAmountPcs },

                    new SqlParameter("@HeaderDiscountAfterTaxAmount", SqlDbType.Decimal) { Value = dBInvoiceDetails.HeaderDiscountAfterTaxAmount },
                    new SqlParameter("@HeaderDiscountTax", SqlDbType.Decimal) { Value = dBInvoiceDetails.HeaderDiscountTax },



                    
                    new SqlParameter("@PriceAfterTaxPcs", SqlDbType.Decimal) { Value = dBInvoiceDetails.PriceAfterTaxPcs },


                    

                    new SqlParameter("@FreeQty", SqlDbType.Decimal) { Value = dBInvoiceDetails.FreeQty },
                    new SqlParameter("@TotalQTY", SqlDbType.Decimal) { Value = dBInvoiceDetails.TotalQTY },
                    new SqlParameter("@ServiceBeforeTax", SqlDbType.Decimal) { Value = dBInvoiceDetails.ServiceBeforeTax },
                    new SqlParameter("@ServiceTaxAmount", SqlDbType.Decimal) { Value = dBInvoiceDetails.ServiceTaxAmount },
                    new SqlParameter("@ServiceAfterTax", SqlDbType.Decimal) { Value = dBInvoiceDetails.ServiceAfterTax },
                    new SqlParameter("@TotalLine", SqlDbType.Decimal  ) { Value = dBInvoiceDetails.TotalLine },
                    new SqlParameter("@BranchID", SqlDbType.Int) { Value = dBInvoiceDetails.BranchID },
                    new SqlParameter("@StoreID", SqlDbType.Int) { Value = dBInvoiceDetails.StoreID },
                    new SqlParameter("@CompanyID", SqlDbType.Int) { Value =dBInvoiceDetails.CompanyID },
                   new SqlParameter("@InvoiceTypeID", SqlDbType.Int) { Value =dBInvoiceDetails.InvoiceTypeID },
                  new SqlParameter("@IsCounted", SqlDbType.Bit) { Value =dBInvoiceDetails.IsCounted },
                  new SqlParameter("@InvoiceDate", SqlDbType.DateTime) { Value = dBInvoiceDetails.InvoiceDate  },
                  new SqlParameter("@BusinessPartnerID", SqlDbType.Int) { Value =dBInvoiceDetails.BusinessPartnerID },
                  new SqlParameter("@ItemBatchsGuid", SqlDbType.UniqueIdentifier) { Value =dBInvoiceDetails.ItemBatchsGuid },
                         new SqlParameter("@CreationDate", SqlDbType.DateTime) { Value = DateTime.Now },
                            new SqlParameter("@AVGCostPerUnit", SqlDbType.Decimal  ) { Value = dBInvoiceDetails.AVGCostPerUnit },


       new SqlParameter("@trackLot", SqlDbType.Bit) { Value =dBInvoiceDetails.TrackLot },
              new SqlParameter("@trackSerial", SqlDbType.Bit) { Value =dBInvoiceDetails.TrackSerial },
                     new SqlParameter("@trackExpiryDate", SqlDbType.Bit) { Value =dBInvoiceDetails.TrackExpiryDate },

                            new SqlParameter("@LotDetails", SqlDbType.NVarChar,-1) { Value =dBInvoiceDetails.LotDetails },
                     

                };

                string a = @"insert into tbl_InvoiceDetails (HeaderGuid,RowIndex,ItemGuid,ItemName,Qty,PriceBeforeTax,DiscountBeforeTaxAmountPcs,DiscountBeforeTaxAmountAll,TaxID
,TaxPercentage,TaxAmount,SpecialTaxID,SpecialTaxPercentage,SpecialTaxAmount,PriceAfterTaxPcs,DiscountAfterTaxAmountPcs,DiscountAfterTaxAmountAll,HeaderDiscountAfterTaxAmount,HeaderDiscountTax,FreeQty,TotalQTY,
ServiceBeforeTax,ServiceTaxAmount,ServiceAfterTax,TotalLine,BranchID,StoreID,CompanyID,InvoiceTypeID,IsCounted,InvoiceDate,BusinessPartnerID,ItemBatchsGuid,CreationDate
,AVGCostPerUnit,trackLot,trackSerial,trackExpiryDate,LotDetails)  
OUTPUT INSERTED.Guid  
values (@HeaderGuid,@RowIndex,@ItemGuid,@ItemName,@Qty,@PriceBeforeTax,@DiscountBeforeTaxAmountPcs,@DiscountBeforeTaxAmountAll,@TaxID
,@TaxPercentage,@TaxAmount,@SpecialTaxID,@SpecialTaxPercentage,@SpecialTaxAmount,@PriceAfterTaxPcs,@DiscountAfterTaxAmountPcs,@DiscountAfterTaxAmountAll,@HeaderDiscountAfterTaxAmount,@HeaderDiscountTax,@FreeQty,@TotalQTY,
@ServiceBeforeTax,@ServiceTaxAmount,@ServiceAfterTax,@TotalLine,@BranchID,@StoreID,@CompanyID,@InvoiceTypeID,@IsCounted,@InvoiceDate,@BusinessPartnerID,@ItemBatchsGuid,@CreationDate,
@AVGCostPerUnit,@trackLot,@trackSerial,@trackExpiryDate,@LotDetails)";
             
                string myGuid = Simulate.String(clsSQL.ExecuteScalar(a, prm, clsSQL.CreateDataBaseConnectionString(dBInvoiceDetails.CompanyID),trn));
                return myGuid;

            }
            catch (Exception ex)
            {

                return "";
            }
        }


    }
    public class DBInvoiceDetails
    {


        public Guid Guid { get; set; }
        public Guid HeaderGuid { get; set; }
        public int RowIndex { get; set; }
        public Guid ItemGuid { get; set; }
        public string ItemName { get; set; }
        public decimal Qty { get; set; }
        public decimal PriceBeforeTax { get; set; }
        public decimal DiscountBeforeTaxAmountPcs { get; set; }
        public decimal DiscountBeforeTaxAmountAll { get; set; }
        public int TaxID { get; set; }
        public decimal TaxPercentage { get; set; }
        public decimal TaxAmount { get; set; }
        public int SpecialTaxID { get; set; }
        public decimal SpecialTaxPercentage { get; set; }
        public decimal SpecialTaxAmount { get; set; }
        public decimal PriceAfterTaxPcs { get; set; }
        

        public decimal DiscountAfterTaxAmountPcs { get; set; }
        public decimal DiscountAfterTaxAmountAll { get; set; }
        public decimal HeaderDiscountAfterTaxAmount { get; set; }
        public decimal HeaderDiscountTax { get; set; }
        

        public decimal FreeQty { get; set; }
        public decimal TotalQTY { get; set; }
        public decimal ServiceBeforeTax { get; set; }
        public decimal ServiceTaxAmount { get; set; }
        public decimal ServiceAfterTax { get; set; }
        public decimal TotalLine { get; set; }
        public int BranchID { get; set; }
        public int StoreID { get; set; }
        public int CompanyID { get; set; }
        public int InvoiceTypeID { get; set; }
        public bool IsCounted { get; set; }
        public DateTime InvoiceDate { get; set; }
        public int BusinessPartnerID { get; set; }
        public Guid ItemBatchsGuid { get; set; }
        public decimal AVGCostPerUnit { get; set; }

        public bool TrackLot { get; set; }
        public bool TrackSerial { get; set; }
        public bool TrackExpiryDate { get; set; }
        public string LotDetails { get; set; }
        

    }
    public class LotDetails
    {
        public string lotNumber { get; set; }
        public string expiryDate { get; set; }
        public double quantity { get; set; }
        public List<string> serialNumbers { get; set; }
        public bool status { get; set; }
        public string itemName { get; set; }
    }
}
