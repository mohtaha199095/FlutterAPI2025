
using ClosedXML.Excel;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Net.NetworkInformation;
using WebApplication2.MainClasses;
using static WebApplication2.MainClasses.clsEnum;

namespace WebApplication2.cls
{
    public class clsInvoiceHeader
    {
        public int SelectMaxInvoiceNumber( int InvoiceTypeID, int BranchID, int CompanyID, SqlTransaction trn = null)
        {
            try
            {
                clsSQL clsSQL = new clsSQL();

                SqlParameter[] prm =
                 {
                 
   new SqlParameter("@InvoiceTypeID", SqlDbType.Int) { Value = InvoiceTypeID },
        new SqlParameter("@CompanyID", SqlDbType.Int) { Value = CompanyID },
          new SqlParameter("@BranchID", SqlDbType.Int) { Value = BranchID },
                };
                DataTable dt = clsSQL.ExecuteQueryStatement(@"select max(InvoiceNo) from tbl_InvoiceHeader where 
(CompanyID=@CompanyID or @CompanyID=0 )
and (BranchID=@BranchID or @BranchID=0 )
and (InvoiceTypeID=@InvoiceTypeID or @InvoiceTypeID=0 ) 
                     ", clsSQL.CreateDataBaseConnectionString(CompanyID), prm, trn);


                int maxNumber = 0;

                if (dt != null && dt.Rows.Count > 0) {

                    maxNumber = Simulate.Integer32(dt.Rows[0][0]);
                }
                maxNumber = maxNumber + 1;

                return maxNumber;
            }
            catch (Exception ex)
            {

                throw;
            }


        }
        public DataTable SelectInvoiceHeaderByGuid(string guid, DateTime date1, DateTime date2, int InvoiceTypeID, int BranchID,  int TableID, int CompanyID, SqlTransaction trn = null)
        {
            try
            {
                clsSQL clsSQL = new clsSQL();

                SqlParameter[] prm =
                 {
                    new SqlParameter("@Guid", SqlDbType.UniqueIdentifier) { Value = Simulate.Guid( guid) },
 new SqlParameter("@date1", SqlDbType.DateTime) { Value =Simulate.StringToDate(  date1 )},
  new SqlParameter("@date2", SqlDbType.DateTime) { Value = Simulate.StringToDate(  date2 )},
   new SqlParameter("@InvoiceTypeID", SqlDbType.Int) { Value = InvoiceTypeID },
        new SqlParameter("@CompanyID", SqlDbType.Int) { Value = CompanyID },
          new SqlParameter("@BranchID", SqlDbType.Int) { Value = BranchID },
              new SqlParameter("@TableID", SqlDbType.Int) { Value = TableID },


          
                };
                DataTable dt = clsSQL.ExecuteQueryStatement(@"select * from tbl_InvoiceHeader where 
(Guid=@Guid or @Guid='00000000-0000-0000-0000-000000000000' )  
and (CompanyID=@CompanyID or @CompanyID=0 )
and (BranchID=@BranchID or @BranchID=0 ) and (TableID=@TableID or @TableID=0 )
and (InvoiceTypeID=@InvoiceTypeID or @InvoiceTypeID=0 )
and cast( InvoiceDate as date) between  cast(@date1 as date) and  cast(@date2 as date) 
                     ", clsSQL.CreateDataBaseConnectionString(CompanyID), prm, trn);

                return dt;
            }
            catch (Exception ex)
            {

                throw;
            }


        }

        public bool DeleteInvoiceHeaderByGuid(string guid,int CompanyID, SqlTransaction trn)
        {
            try
            {
                clsSQL clsSQL = new clsSQL();

                SqlParameter[] prm =
                 { new SqlParameter("@guid", SqlDbType.UniqueIdentifier) { Value = Simulate.Guid( guid) },

                };
                int A = clsSQL.ExecuteNonQueryStatement(@"delete from tbl_InvoiceHeader where (guid=@guid  )", clsSQL.CreateDataBaseConnectionString(CompanyID), prm , trn);

                return true;
            }
            catch (Exception ex)
            {

                return false;
            }


        }
        public string InsertInvoiceHeader(DBInvoiceHeader DbInvoiceHeader,  SqlTransaction trn)
        {
            try
            {
                SqlParameter[] prm =
                   { new SqlParameter("@InvoiceNo", SqlDbType.Int) { Value = DbInvoiceHeader.InvoiceNo },
                    new SqlParameter("@InvoiceDate", SqlDbType.DateTime) { Value = DbInvoiceHeader.InvoiceDate },
                    new SqlParameter("@PaymentMethodID", SqlDbType.Int) { Value = DbInvoiceHeader.PaymentMethodID },
                    new SqlParameter("@BranchID", SqlDbType.Int) { Value = DbInvoiceHeader.BranchID },
                    new SqlParameter("@Note", SqlDbType.NVarChar,-1) { Value = DbInvoiceHeader.Note },
                    new SqlParameter("@BusinessPartnerID", SqlDbType.Int) { Value = DbInvoiceHeader.BusinessPartnerID },
                    new SqlParameter("@StoreID", SqlDbType.Int) { Value = DbInvoiceHeader.StoreID },
                    new SqlParameter("@InvoiceTypeID", SqlDbType.Int) { Value = DbInvoiceHeader.InvoiceTypeID },
                    new SqlParameter("@IsCounted", SqlDbType.Bit) { Value = DbInvoiceHeader.IsCounted },
                    new SqlParameter("@JVGuid", SqlDbType.UniqueIdentifier) { Value = DbInvoiceHeader.JVGuid },
                    new SqlParameter("@TotalTax", SqlDbType.Decimal) { Value = DbInvoiceHeader.TotalTax },
                    new SqlParameter("@HeaderDiscount", SqlDbType.Decimal) { Value = DbInvoiceHeader.HeaderDiscount },
                    new SqlParameter("@TotalDiscount", SqlDbType.Decimal) { Value = DbInvoiceHeader.TotalDiscount },
                    new SqlParameter("@TotalInvoice", SqlDbType.Decimal) { Value = DbInvoiceHeader.TotalInvoice },
                    new SqlParameter("@RefNo", SqlDbType.NVarChar,-1) { Value = DbInvoiceHeader.RefNo },
                    new SqlParameter("@RelatedInvoiceGuid", SqlDbType.UniqueIdentifier) { Value = DbInvoiceHeader.RelatedInvoiceGuid },
                    new SqlParameter("@CashID", SqlDbType.Int) { Value = DbInvoiceHeader.CashID },
                                  new SqlParameter("@BankID", SqlDbType.Int) { Value = DbInvoiceHeader.BankID },
                    new SqlParameter("@POSDayGuid", SqlDbType.UniqueIdentifier) { Value = DbInvoiceHeader.POSDayGuid },
                    new SqlParameter("@POSSessionGuid", SqlDbType.UniqueIdentifier  ) { Value = DbInvoiceHeader.POSSessionGuid },
                    new SqlParameter("@CompanyID", SqlDbType.Int) { Value = DbInvoiceHeader.CompanyID },
                      new SqlParameter("@AccountID", SqlDbType.Int) { Value = DbInvoiceHeader.AccountID },

                    new SqlParameter("@CreationUserId", SqlDbType.Int) { Value = DbInvoiceHeader.CreationUserID },
                    new SqlParameter("@CreationDate", SqlDbType.DateTime) { Value = DateTime.Now },
                        new SqlParameter("@status", SqlDbType.Int) { Value = DbInvoiceHeader.status },
                                new SqlParameter("@tableID", SqlDbType.Int) { Value = DbInvoiceHeader.tableID },
 new SqlParameter("@CurrencyID", SqlDbType.Int) { Value = DbInvoiceHeader.CurrencyID },
     new SqlParameter("@CurrencyRate", SqlDbType.Decimal) { Value = DbInvoiceHeader.CurrencyRate },
       new SqlParameter("@CurrencyBaseAmount", SqlDbType.Decimal) { Value = DbInvoiceHeader.CurrencyBaseAmount },



         



 











            };

                string a = @"insert into tbl_invoiceHeader (InvoiceNo,InvoiceDate,PaymentMethodID,BranchID,Note,BusinessPartnerID,StoreID
,InvoiceTypeID,IsCounted,JVGuid,TotalTax,HeaderDiscount,TotalDiscount,TotalInvoice,RefNo,RelatedInvoiceGuid
,CashID,BankID,POSDayGuid,POSSessionGuid,AccountID,CompanyID,CreationUserID,CreationDate,tableID,
status,CurrencyID,CurrencyRate,CurrencyBaseAmount)  
OUTPUT INSERTED.Guid  
values (@InvoiceNo,@InvoiceDate,@PaymentMethodID,@BranchID,@Note,@BusinessPartnerID,@StoreID
,@InvoiceTypeID,@IsCounted,@JVGuid,@TotalTax,@HeaderDiscount,@TotalDiscount,@TotalInvoice,@RefNo,@RelatedInvoiceGuid
,@CashID,@BankID,@POSDayGuid,@POSSessionGuid,@AccountID,@CompanyID,@CreationUserID,@CreationDate,@tableID,@status
,@CurrencyID,@CurrencyRate,@CurrencyBaseAmount)";
                clsSQL clsSQL = new clsSQL();
                string myGuid = Simulate.String(clsSQL.ExecuteScalar(a, prm, clsSQL.CreateDataBaseConnectionString(DbInvoiceHeader.CompanyID), trn));
                return myGuid;

            }
            catch (Exception ex)
            {

                return "";
            }
        }
       

        public string UpdateInvoiceHeader(DBInvoiceHeader DbInvoiceHeader,int CompanyID, SqlTransaction trn)
        {
            try
            {
                clsSQL clsSQL = new clsSQL();

                SqlParameter[] prm =
                   {new SqlParameter("@Guid", SqlDbType.UniqueIdentifier) { Value = DbInvoiceHeader.Guid },
                    new SqlParameter("@InvoiceNo", SqlDbType.Int) { Value = DbInvoiceHeader.InvoiceNo },
                    new SqlParameter("@InvoiceDate", SqlDbType.DateTime) { Value = DbInvoiceHeader.InvoiceDate },
                    new SqlParameter("@PaymentMethodID", SqlDbType.Int) { Value = DbInvoiceHeader.PaymentMethodID },
                    new SqlParameter("@BranchID", SqlDbType.Int) { Value = DbInvoiceHeader.BranchID },
                    new SqlParameter("@Note", SqlDbType.NVarChar,-1) { Value =  DbInvoiceHeader.Note },
                    new SqlParameter("@BusinessPartnerID", SqlDbType.Int) { Value = DbInvoiceHeader.BusinessPartnerID },
                    new SqlParameter("@StoreID", SqlDbType.Int) { Value = DbInvoiceHeader.StoreID },
                    new SqlParameter("@InvoiceTypeID", SqlDbType.Int) { Value = DbInvoiceHeader.InvoiceTypeID },
                    new SqlParameter("@IsCounted", SqlDbType.Bit) { Value = DbInvoiceHeader.IsCounted },

                    new SqlParameter("@TotalTax", SqlDbType.Decimal) { Value = DbInvoiceHeader.TotalTax },
                    new SqlParameter("@HeaderDiscount", SqlDbType.Decimal) { Value = DbInvoiceHeader.HeaderDiscount },
                    new SqlParameter("@TotalDiscount", SqlDbType.Decimal) { Value = DbInvoiceHeader.TotalDiscount },
                    new SqlParameter("@TotalInvoice", SqlDbType.Decimal) { Value = DbInvoiceHeader.TotalInvoice },
                    new SqlParameter("@RefNo", SqlDbType.NVarChar,-1) { Value = DbInvoiceHeader.RefNo },
                    new SqlParameter("@RelatedInvoiceGuid", SqlDbType.UniqueIdentifier) { Value = DbInvoiceHeader.RelatedInvoiceGuid },
                    new SqlParameter("@CashID", SqlDbType.Int) { Value = DbInvoiceHeader.CashID },

                     new SqlParameter("@BankID", SqlDbType.Int) { Value = DbInvoiceHeader.BankID },
                    
                    new SqlParameter("@POSDayGuid", SqlDbType.UniqueIdentifier) { Value = DbInvoiceHeader.POSDayGuid },
                    new SqlParameter("@POSSessionGuid", SqlDbType.UniqueIdentifier  ) { Value = DbInvoiceHeader.POSSessionGuid },

                    new SqlParameter("@AccountID", SqlDbType.Int) { Value = DbInvoiceHeader.AccountID },

                     new SqlParameter("@ModificationUserID", SqlDbType.Int) { Value = DbInvoiceHeader.ModificationUserID },
                    new SqlParameter("@ModificationDate", SqlDbType.DateTime) { Value = DateTime.Now },


                          new SqlParameter("@status", SqlDbType.Int) { Value = DbInvoiceHeader.status },
                                new SqlParameter("@tableID", SqlDbType.Int) { Value = DbInvoiceHeader.tableID },

   new SqlParameter("@CurrencyID", SqlDbType.Int) { Value = DbInvoiceHeader.CurrencyID },
     new SqlParameter("@CurrencyRate", SqlDbType.Decimal) { Value = DbInvoiceHeader.CurrencyRate },
       new SqlParameter("@CurrencyBaseAmount", SqlDbType.Decimal) { Value = DbInvoiceHeader.CurrencyBaseAmount },

 

    };
                string a = @"update tbl_invoiceHeader set  

 InvoiceNo =@InvoiceNo,InvoiceDate=@InvoiceDate,PaymentMethodID=@PaymentMethodID,BranchID=@BranchID,Note=@Note,BusinessPartnerID=@BusinessPartnerID,StoreID=@StoreID
,InvoiceTypeID=@InvoiceTypeID,IsCounted=@IsCounted, TotalTax=@TotalTax,HeaderDiscount=@HeaderDiscount,TotalDiscount=@TotalDiscount
,TotalInvoice=@TotalInvoice,RefNo=@RefNo,RelatedInvoiceGuid=@RelatedInvoiceGuid
,CashID=@CashID,  BankID=@BankID, tableID=@tableID, status=@status,CurrencyID=@CurrencyID , CurrencyRate=@CurrencyRate,
CurrencyBaseAmount=@CurrencyBaseAmount,


POSDayGuid=@POSDayGuid,POSSessionGuid=@POSSessionGuid,AccountID=@AccountID,ModificationUserID=@ModificationUserID,ModificationDate=@ModificationDate   
 where Guid=@guid";

                string A = Simulate.String(clsSQL.ExecuteNonQueryStatement(a, clsSQL.CreateDataBaseConnectionString(CompanyID), prm, trn));
                return A;


            }
            catch (Exception ex)
            {

                return "";
            }
        }

        public string UpdateInvoiceHeaderJVGuid(string Guid, string JVGuid,int CompanyID, SqlTransaction trn)
        {
            try
            {
                clsSQL clsSQL = new clsSQL();

                SqlParameter[] prm =
                   {new SqlParameter("@Guid", SqlDbType.UniqueIdentifier) { Value =Simulate.Guid( Guid) },
                    new SqlParameter("@JVGuid", SqlDbType.UniqueIdentifier) { Value = Simulate.Guid(JVGuid) },

                };
                string a = @"update tbl_invoiceHeader set  

  
 JVGuid=@JVGuid  
 where Guid=@guid";

                string A = Simulate.String(clsSQL.ExecuteNonQueryStatement(a, clsSQL.CreateDataBaseConnectionString(CompanyID), prm, trn));
                return A;


            }
            catch (Exception ex)
            {

                return "";
            }
        }
        public bool InsertInvoiceJournalVoucher(List<DBInvoiceDetails> invoiceDetails, int AccountID, int PaymentMethodID, int cashID,int bankID, int businessPartnerID, decimal HeaderDiscount, int BranchID, string Notes, int CompanyID, DateTime VoucherDate, int CreationUserId, int InvoiceType, string InvoiceGuid,int CurrencyID ,decimal CurrencyRate,  SqlTransaction trn)
        {
            try
            {
                clsPaymentMethod clsPaymentMethod = new clsPaymentMethod();
                DataTable dtPaymentMethod = clsPaymentMethod.SelectPaymentMethodByID(PaymentMethodID,"","",CompanyID);
                int PaymentMethodTypeID = 0;
                if (PaymentMethodID>0 &&dtPaymentMethod != null && dtPaymentMethod.Rows.Count > 0)
                {//   Cash = 1,       Debit = 2,          Bank = 3, 
                    if (Simulate.Bool(dtPaymentMethod.Rows[0]["IsCash"])) {
                        PaymentMethodTypeID = 1;
                    }else if (Simulate.Bool(dtPaymentMethod.Rows[0]["IsBank"]))
                    {
                        PaymentMethodTypeID = 3;
                    }
                    else if (Simulate.Bool(dtPaymentMethod.Rows[0]["IsDebit"]))
                    {
                        PaymentMethodTypeID = 2;
                    }


                }

                if (InvoiceType == (int)clsEnum.VoucherType.SalesInvoice ||
                    InvoiceType == (int)clsEnum.VoucherType.SalesRefund ||
                    InvoiceType == (int)clsEnum.VoucherType.PurchaseInvoice ||
                    InvoiceType == (int)clsEnum.VoucherType.PurchaseRefund ||
                    InvoiceType == (int)clsEnum.VoucherType.GoodIssue ||
                    InvoiceType == (int)clsEnum.VoucherType.GoodRecipt ||
                    InvoiceType == (int)clsEnum.VoucherType.POSSalesInvoice ||
                    InvoiceType == (int)clsEnum.VoucherType.POSSalesInvoicereturn)
                {
                    clsInvoiceHeader clsInvoiceHeader = new clsInvoiceHeader();
                    clsJournalVoucherHeader clsJournalVoucherHeader = new clsJournalVoucherHeader();
                    DataTable dtInvoiceHeader = clsInvoiceHeader.SelectInvoiceHeaderByGuid(InvoiceGuid, Simulate.StringToDate("1900-01-01"), Simulate.StringToDate("2300-01-01"), 0, 0, 0, 0,trn);
                    string JVGuid = "";
                    if (dtInvoiceHeader != null && dtInvoiceHeader.Rows.Count > 0)
                    {

                        JVGuid = Simulate.String(dtInvoiceHeader.Rows[0]["JVGuid"]);

                    }

                    DataTable dtMaxJVNumber = clsJournalVoucherHeader.SelectMaxJVNo(JVGuid, InvoiceType, CompanyID, trn);
                    int MaxJVNumber = 0;
                    if (dtMaxJVNumber != null && dtMaxJVNumber.Rows.Count > 0)
                    {

                        MaxJVNumber = Simulate.Integer32(dtMaxJVNumber.Rows[0][0]);
                    }
                    if (JVGuid == "" || JVGuid == Simulate.Guid("").ToString())
                    {

                        MaxJVNumber = MaxJVNumber + 1;

                        JVGuid = clsJournalVoucherHeader.InsertJournalVoucherHeader(BranchID, 0, Notes, Simulate.String(MaxJVNumber), InvoiceType, CompanyID, VoucherDate, CreationUserId, "", 0, trn);
                    }
                    else
                    {
                        clsJournalVoucherHeader.UpdateJournalVoucherHeader(BranchID, 0, Notes, Simulate.String(MaxJVNumber),
                            InvoiceType, VoucherDate, JVGuid, CreationUserId, "", 0,CompanyID, trn);


                    }

                    clsInvoiceHeader.UpdateInvoiceHeaderJVGuid(InvoiceGuid, JVGuid,CompanyID, trn);
                    cls_AccountSetting cls_AccountSetting = new cls_AccountSetting();
                    DataTable dtAccountSetting = cls_AccountSetting.SelectAccountSetting(0, 0, CompanyID, trn);
                    int SalesInvoiceAcc = 0;
                    int PurchaseInvoiceAcc = 0;
                    int PurchaseRefundAcc = 0;
                    int SalesRefundAcc = 0;
                    int CashAccount = 0; 
                    int BankAccount = 0;
                    int InventoryAcc = 0;
                    int PurchaseDiscountAcc = 0;
                    int SalesDiscountAcc = 0;
                    int CustomerAccount = 0;
                    int VendorAccount = 0;
                    int PurchaseTaxAccount = 0;
                    int SalesTaxAccount = 0;
                    int SpecialPurchaseTaxAccount = 0;
                    int SpecialSalesTaxAccount = 0;
                    int COGSAccount = 0;
                    if (dtAccountSetting != null && dtAccountSetting.Rows.Count > 0)
                    {

                        PurchaseDiscountAcc = GetValueFromDT(dtAccountSetting, "AccountRefID", Simulate.String((int)clsEnum.AccountMainSetting.PurchaseDiscount), 2);
                        PurchaseInvoiceAcc = GetValueFromDT(dtAccountSetting, "AccountRefID", Simulate.String((int)clsEnum.AccountMainSetting.PurchaseAccount), 2);
                        PurchaseRefundAcc = GetValueFromDT(dtAccountSetting, "AccountRefID", Simulate.String((int)clsEnum.AccountMainSetting.PurchaseReturnAccount), 2);
                        PurchaseTaxAccount = GetValueFromDT(dtAccountSetting, "AccountRefID", Simulate.String((int)clsEnum.AccountMainSetting.PurchaseTaxAccount), 2);
                        SpecialPurchaseTaxAccount = GetValueFromDT(dtAccountSetting, "AccountRefID", Simulate.String((int)clsEnum.AccountMainSetting.SpecialPurchaseTaxAccount), 2);
                        SalesInvoiceAcc = GetValueFromDT(dtAccountSetting, "AccountRefID", Simulate.String((int)clsEnum.AccountMainSetting.SalesAccount), 2);
                        SalesRefundAcc = GetValueFromDT(dtAccountSetting, "AccountRefID", Simulate.String((int)clsEnum.AccountMainSetting.SalesReturnAccount), 2);
                        SalesDiscountAcc = GetValueFromDT(dtAccountSetting, "AccountRefID", Simulate.String((int)clsEnum.AccountMainSetting.SalesDiscount), 2);
                        SalesTaxAccount = GetValueFromDT(dtAccountSetting, "AccountRefID", Simulate.String((int)clsEnum.AccountMainSetting.SalesTaxAccount), 2);
                        SpecialSalesTaxAccount = GetValueFromDT(dtAccountSetting, "AccountRefID", Simulate.String((int)clsEnum.AccountMainSetting.SpecialSalesTaxAccount), 2);
                        CashAccount = GetValueFromDT(dtAccountSetting, "AccountRefID", Simulate.String((int)clsEnum.AccountMainSetting.CashAccount), 2);
                        BankAccount = GetValueFromDT(dtAccountSetting, "AccountRefID", Simulate.String((int)clsEnum.AccountMainSetting.Banks), 2);
                        COGSAccount = GetValueFromDT(dtAccountSetting, "AccountRefID", Simulate.String((int)clsEnum.AccountMainSetting.COGS), 2);

                        InventoryAcc = GetValueFromDT(dtAccountSetting, "AccountRefID", Simulate.String((int)clsEnum.AccountMainSetting.Inventory), 2);
                        CustomerAccount = GetValueFromDT(dtAccountSetting, "AccountRefID", Simulate.String((int)clsEnum.AccountMainSetting.CustomerAccount), 2);
                        VendorAccount = GetValueFromDT(dtAccountSetting, "AccountRefID", Simulate.String((int)clsEnum.AccountMainSetting.VendorAccount), 2);
                        clsJournalVoucherDetails clsJournalVoucherDetails = new clsJournalVoucherDetails();
                        clsJournalVoucherDetails.DeleteJournalVoucherDetailsByParentId(JVGuid, CompanyID, trn);



                        decimal TotalSales = 0;
                        decimal TotalSalesTax = 0;
                        decimal TotalSalesSpecialTax = 0;
                        decimal TotalDiscount = 0;
                        decimal TotalCosts = 0;
                        for (int i = 0; i < invoiceDetails.Count; i++)
                        {if (InvoiceType == (int)clsEnum.VoucherType.POSSalesInvoice || InvoiceType == (int)clsEnum.VoucherType.POSSalesInvoicereturn) {
                                TotalDiscount = TotalDiscount + (invoiceDetails[i].DiscountAfterTaxAmountAll);
                            } else {
                                TotalDiscount = TotalDiscount + (invoiceDetails[i].DiscountBeforeTaxAmountAll);
                            }

                            TotalCosts = TotalCosts + (invoiceDetails[i].AVGCostPerUnit * invoiceDetails[i].TotalQTY);
                                TotalSales = TotalSales + (invoiceDetails[i].PriceBeforeTax * invoiceDetails[i].Qty);
                            TotalSalesTax = TotalSalesTax + (invoiceDetails[i].TaxAmount);

                            TotalSalesSpecialTax = TotalSalesSpecialTax + (invoiceDetails[i].SpecialTaxAmount);

                        }
                        TotalDiscount = TotalDiscount + HeaderDiscount;
                        int businessPartnerAccount = CustomerAccount;
                        clsBusinessPartner clsBusinessPartner = new clsBusinessPartner();
                        DataTable dtbusinesspartnerType = clsBusinessPartner.SelectBusinessPartner(businessPartnerID, 0, "", "",-1, CompanyID);

                        if (dtbusinesspartnerType != null && dtbusinesspartnerType.Rows.Count > 0)
                        {
                            if (Simulate.Integer32(dtbusinesspartnerType.Rows[0]["type"]) == (int)clsEnum.BusinessPartner.Vendor)
                            {

                                businessPartnerAccount = VendorAccount;
                            }

                        }
                        if ( InvoiceType == (int)clsEnum.VoucherType.SalesInvoice)
                        {//COGS
                            if (TotalCosts>0) {

                                clsJournalVoucherDetails.InsertJournalVoucherDetails(JVGuid, 0, COGSAccount, 0,
                             TotalCosts * CurrencyRate,//                             Debit,
                            0,//                    Credit
                              TotalCosts * CurrencyRate,//              Total,
                              CurrencyID, CurrencyRate, TotalCosts,
                              BranchID, 0, VoucherDate, "", CompanyID, CreationUserId, "", trn);

                                clsJournalVoucherDetails.InsertJournalVoucherDetails(JVGuid, 0, InventoryAcc, 0,
                                                0,//                             Debit,
                                                    TotalCosts * CurrencyRate,//                    Credit
                                                  0-(  TotalCosts * CurrencyRate),//              Total,
                                                    CurrencyID, CurrencyRate,0- TotalCosts,
                                                    BranchID, 0, VoucherDate, "", CompanyID, CreationUserId, "", trn);

                            }
                            //Credit Sales without Tax
                            if (TotalSales > 0)
                            {
                                clsJournalVoucherDetails.InsertJournalVoucherDetails(JVGuid, 0, SalesInvoiceAcc, 0,
                              0,//                             Debit,
                              TotalSales * CurrencyRate,//                    Credit
                              (0 - TotalSales) * CurrencyRate,//              Total,
                              CurrencyID,CurrencyRate, (0 - TotalSales),
                              BranchID, 0, VoucherDate, "", CompanyID, CreationUserId,"", trn);
                            }
                            //===========================================

                            // Credit Sales Tax 
                            if (TotalSalesTax > 0)
                            {
                                clsJournalVoucherDetails.InsertJournalVoucherDetails(JVGuid, 0, SalesTaxAccount, 0,
                              0,//                             Debit,
                              TotalSalesTax * CurrencyRate,//                    Credit
                              (0 - TotalSalesTax) * CurrencyRate,//              Total,
                              CurrencyID, CurrencyRate, (0 - TotalSalesTax),
                              BranchID, 0, VoucherDate, "", CompanyID, CreationUserId,"", trn);
                            }
                            //===========================================

                            // Credit  Special Sales Tax 
                            if (TotalSalesSpecialTax > 0)
                            {
                                clsJournalVoucherDetails.InsertJournalVoucherDetails(JVGuid, 0, SpecialSalesTaxAccount, 0,
                              0,//                             Debit,
                              TotalSalesSpecialTax * CurrencyRate,//                    Credit
                              (0 - TotalSalesSpecialTax) * CurrencyRate,//              Total,
                               CurrencyID, CurrencyRate, (0 - TotalSalesSpecialTax),
                              BranchID, 0, VoucherDate, "", CompanyID, CreationUserId, "", trn);
                            }
                            //===========================================



                            // Debit  Discount ID 
                            if (TotalDiscount > 0)
                            {
                                clsJournalVoucherDetails.InsertJournalVoucherDetails(JVGuid, 0, SalesDiscountAcc, 0,
                     TotalDiscount * CurrencyRate,//                             Debit,
                             0,//                    Credit
                           TotalDiscount * CurrencyRate,//              Total,
                            CurrencyID, CurrencyRate, TotalDiscount,
                              BranchID, 0, VoucherDate, "", CompanyID, CreationUserId,"", trn);
                            }
                            //===========================================


                            // Debit Customer ID 
                            if ((TotalSales + TotalSalesTax + TotalSalesSpecialTax - TotalDiscount) > 0)
                            {
                                clsJournalVoucherDetails.InsertJournalVoucherDetails(JVGuid, 0, businessPartnerAccount, businessPartnerID,
                    ( TotalSales + TotalSalesTax + TotalSalesSpecialTax - TotalDiscount) * CurrencyRate,//                             Debit,
                             0,//                    Credit
                          (  TotalSales + TotalSalesTax + TotalSalesSpecialTax - TotalDiscount) * CurrencyRate,//              Total,
                             CurrencyID, CurrencyRate,( TotalSales + TotalSalesTax + TotalSalesSpecialTax - TotalDiscount),
                              BranchID, 0, VoucherDate, "", CompanyID, CreationUserId, "",trn);
                            }
                            //===========================================
                            if (PaymentMethodTypeID == (int)clsEnum.PaymentMethod.Cash)
                            {    // Credit  Cash ID 
                                if ((TotalSales + TotalSalesTax + TotalSalesSpecialTax - TotalDiscount) > 0)
                                {
                                    clsJournalVoucherDetails.InsertJournalVoucherDetails(JVGuid, 0, businessPartnerAccount, businessPartnerID,
                         0,//                             Debit,
                               ( TotalSales + TotalSalesTax + TotalSalesSpecialTax - TotalDiscount) * CurrencyRate,//                    Credit
                           (  0 - (TotalSales + TotalSalesTax + TotalSalesSpecialTax - TotalDiscount)) * CurrencyRate,//              Total,
                               CurrencyID, CurrencyRate,( 0 - (TotalSales + TotalSalesTax + TotalSalesSpecialTax - TotalDiscount)),
                                  BranchID, 0, VoucherDate, "", CompanyID, CreationUserId,"", trn);
                                }
                                // Debit  Cash ID 
                                if ((TotalSales + TotalSalesTax + TotalSalesSpecialTax - TotalDiscount) > 0)
                                {
                                    clsJournalVoucherDetails.InsertJournalVoucherDetails(JVGuid, 0, CashAccount, cashID,
                        ( TotalSales + TotalSalesTax + TotalSalesSpecialTax - TotalDiscount) * CurrencyRate,//                             Debit,
                                 0,//                    Credit
                               (TotalSales + TotalSalesTax + TotalSalesSpecialTax - TotalDiscount) * CurrencyRate,//              Total,
                                  CurrencyID, CurrencyRate,( TotalSales + TotalSalesTax + TotalSalesSpecialTax - TotalDiscount),
                                 BranchID, 0, VoucherDate, "", CompanyID, CreationUserId,"", trn);
                                }

                            }
                            else if (PaymentMethodTypeID == (int)clsEnum.PaymentMethod.Bank)
                            {    // Credit  Cash ID 
                                if ((TotalSales + TotalSalesTax + TotalSalesSpecialTax - TotalDiscount) > 0)
                                {
                                    clsJournalVoucherDetails.InsertJournalVoucherDetails(JVGuid, 0, businessPartnerAccount, businessPartnerID,
                         0,//                             Debit,
                               ( TotalSales + TotalSalesTax + TotalSalesSpecialTax - TotalDiscount) * CurrencyRate,//                    Credit
                             (0 - (TotalSales + TotalSalesTax + TotalSalesSpecialTax - TotalDiscount)) * CurrencyRate,//              Total,
                             CurrencyID, CurrencyRate,( 0 - (TotalSales + TotalSalesTax + TotalSalesSpecialTax - TotalDiscount)),
                             BranchID, 0, VoucherDate, "", CompanyID, CreationUserId, "", trn);
                                }
                                // Debit  Cash ID 
                                if ((TotalSales + TotalSalesTax + TotalSalesSpecialTax - TotalDiscount) > 0)
                                {
                                    clsJournalVoucherDetails.InsertJournalVoucherDetails(JVGuid, 0, BankAccount, bankID,
                        ( TotalSales + TotalSalesTax + TotalSalesSpecialTax - TotalDiscount) * CurrencyRate,//                             Debit,
                                 0,//                    Credit
                              ( TotalSales + TotalSalesTax + TotalSalesSpecialTax - TotalDiscount) * CurrencyRate,//              Total,
                                 CurrencyID, CurrencyRate, (TotalSales + TotalSalesTax + TotalSalesSpecialTax - TotalDiscount),
                                BranchID, 0, VoucherDate, "", CompanyID, CreationUserId, "", trn);
                                }

                            }
                        }
                        else if ( InvoiceType == (int)clsEnum.VoucherType.POSSalesInvoice)
                        {// COGS
                            if (TotalCosts > 0)
                            {

                                clsJournalVoucherDetails.InsertJournalVoucherDetails(JVGuid, 0, COGSAccount, 0,
                             TotalCosts * CurrencyRate,//                             Debit,
                            0,//                    Credit
                              TotalCosts * CurrencyRate,//              Total,
                              CurrencyID, CurrencyRate, TotalCosts,
                              BranchID, 0, VoucherDate, "", CompanyID, CreationUserId, "", trn);

                                clsJournalVoucherDetails.InsertJournalVoucherDetails(JVGuid, 0, InventoryAcc, 0,
                                                0,//                             Debit,
                                                    TotalCosts * CurrencyRate,//                    Credit
                                                  0 - (TotalCosts * CurrencyRate),//              Total,
                                                    CurrencyID, CurrencyRate, 0 - TotalCosts,
                                                    BranchID, 0, VoucherDate, "", CompanyID, CreationUserId, "", trn);

                            }
                            //Credit Sales without Tax
                            if (TotalSales > 0)
                            {
                                clsJournalVoucherDetails.InsertJournalVoucherDetails(JVGuid, 0, SalesInvoiceAcc, 0,
                              0,//                             Debit,
                              TotalSales  ,//                    Credit
                              (0 - TotalSales) ,//              Total,
                              CurrencyID, CurrencyRate, (0 - TotalSales) /CurrencyRate ,
                              BranchID, 0, VoucherDate, "", CompanyID, CreationUserId, "", trn);
                            }
                            //===========================================

                            // Credit Sales Tax 
                            if (TotalSalesTax > 0)
                            {
                                clsJournalVoucherDetails.InsertJournalVoucherDetails(JVGuid, 0, SalesTaxAccount, 0,
                              0,//                             Debit,
                              TotalSalesTax ,//                    Credit
                              (0 - TotalSalesTax) ,//              Total,
                              CurrencyID, CurrencyRate, (0 - TotalSalesTax) / CurrencyRate,
                              BranchID, 0, VoucherDate, "", CompanyID, CreationUserId, "", trn);
                            }
                            //===========================================

                            // Credit  Special Sales Tax 
                            if (TotalSalesSpecialTax > 0)
                            {
                                clsJournalVoucherDetails.InsertJournalVoucherDetails(JVGuid, 0, SpecialSalesTaxAccount, 0,
                              0,//                             Debit,
                              TotalSalesSpecialTax ,//                    Credit
                              (0 - TotalSalesSpecialTax) ,//              Total,
                               CurrencyID, CurrencyRate, (0 - TotalSalesSpecialTax) / CurrencyRate,
                              BranchID, 0, VoucherDate, "", CompanyID, CreationUserId, "", trn);
                            }
                            //===========================================



                            // Debit  Discount ID 
                            if (TotalDiscount > 0)
                            {
                                clsJournalVoucherDetails.InsertJournalVoucherDetails(JVGuid, 0, SalesDiscountAcc, 0,
                     TotalDiscount ,//                             Debit,
                             0,//                    Credit
                           TotalDiscount ,//              Total,
                            CurrencyID, CurrencyRate, TotalDiscount / CurrencyRate,
                              BranchID, 0, VoucherDate, "", CompanyID, CreationUserId, "", trn);
                            }
                            //===========================================


                            // Debit Customer ID 
                            if ((TotalSales + TotalSalesTax + TotalSalesSpecialTax - TotalDiscount) > 0)
                            {
                                clsJournalVoucherDetails.InsertJournalVoucherDetails(JVGuid, 0, businessPartnerAccount, businessPartnerID,
                    ( TotalSales + TotalSalesTax + TotalSalesSpecialTax - TotalDiscount) ,//                             Debit,
                             0,//                    Credit
                           ( TotalSales + TotalSalesTax + TotalSalesSpecialTax - TotalDiscount) ,//              Total,
                             CurrencyID, CurrencyRate, (TotalSales + TotalSalesTax + TotalSalesSpecialTax - TotalDiscount) / CurrencyRate,
                              BranchID, 0, VoucherDate, "", CompanyID, CreationUserId, "", trn);
                            }
                            //===========================================
                            if (PaymentMethodTypeID == (int)clsEnum.PaymentMethod.Cash)
                            {    // Credit  Cash ID 
                                if ((TotalSales + TotalSalesTax + TotalSalesSpecialTax - TotalDiscount) > 0)
                                {
                                    clsJournalVoucherDetails.InsertJournalVoucherDetails(JVGuid, 0, businessPartnerAccount, businessPartnerID,
                         0,//                             Debit,
                              (  TotalSales + TotalSalesTax + TotalSalesSpecialTax - TotalDiscount) ,//                    Credit
                            ( 0 - (TotalSales + TotalSalesTax + TotalSalesSpecialTax - TotalDiscount)) ,//              Total,
                               CurrencyID, CurrencyRate, (0 - (TotalSales + TotalSalesTax + TotalSalesSpecialTax - TotalDiscount)) / CurrencyRate,
                                  BranchID, 0, VoucherDate, "", CompanyID, CreationUserId, "", trn);
                                }
                                // Debit  Cash ID 
                                if ((TotalSales + TotalSalesTax + TotalSalesSpecialTax - TotalDiscount) > 0)
                                {
                                    clsJournalVoucherDetails.InsertJournalVoucherDetails(JVGuid, 0, CashAccount, cashID,
                        ( TotalSales + TotalSalesTax + TotalSalesSpecialTax - TotalDiscount) ,//                             Debit,
                                 0,//                    Credit
                             (  TotalSales + TotalSalesTax + TotalSalesSpecialTax - TotalDiscount) ,//              Total,
                                  CurrencyID, CurrencyRate, (TotalSales + TotalSalesTax + TotalSalesSpecialTax - TotalDiscount) / CurrencyRate,
                                 BranchID, 0, VoucherDate, "", CompanyID, CreationUserId, "", trn);
                                }

                            }
                            else if (PaymentMethodTypeID == (int)clsEnum.PaymentMethod.Bank)
                            {    // Credit  Cash ID 
                                if ((TotalSales + TotalSalesTax + TotalSalesSpecialTax - TotalDiscount) > 0)
                                {
                                    clsJournalVoucherDetails.InsertJournalVoucherDetails(JVGuid, 0, businessPartnerAccount, businessPartnerID,
                         0,//                             Debit,
                               ( TotalSales + TotalSalesTax + TotalSalesSpecialTax - TotalDiscount) ,//                    Credit
                            ( 0 - (TotalSales + TotalSalesTax + TotalSalesSpecialTax - TotalDiscount)) ,//              Total,
                             CurrencyID, CurrencyRate, (0 - (TotalSales + TotalSalesTax + TotalSalesSpecialTax - TotalDiscount)) / CurrencyRate,
                             BranchID, 0, VoucherDate, "", CompanyID, CreationUserId, "", trn);
                                }
                                // Debit  Cash ID 
                                if ((TotalSales + TotalSalesTax + TotalSalesSpecialTax - TotalDiscount) > 0)
                                {
                                    clsJournalVoucherDetails.InsertJournalVoucherDetails(JVGuid, 0, BankAccount, bankID,
                         (TotalSales + TotalSalesTax + TotalSalesSpecialTax - TotalDiscount),//                             Debit,
                                 0,//                    Credit
                              ( TotalSales + TotalSalesTax + TotalSalesSpecialTax - TotalDiscount) ,//              Total,
                                 CurrencyID, CurrencyRate, (TotalSales + TotalSalesTax + TotalSalesSpecialTax - TotalDiscount) / CurrencyRate,
                                BranchID, 0, VoucherDate, "", CompanyID, CreationUserId, "", trn);
                                }

                            }
                        }
                        else if (InvoiceType == (int)clsEnum.VoucherType.SalesRefund )
                        {
                            //COGS
                            if (TotalCosts > 0)
                            {

                                clsJournalVoucherDetails.InsertJournalVoucherDetails(JVGuid, 0, InventoryAcc , 0,
                             TotalCosts * CurrencyRate,//                             Debit,
                            0,//                    Credit
                              TotalCosts * CurrencyRate,//              Total,
                              CurrencyID, CurrencyRate, TotalCosts,
                              BranchID, 0, VoucherDate, "", CompanyID, CreationUserId, "", trn);

                                clsJournalVoucherDetails.InsertJournalVoucherDetails(JVGuid, 0, COGSAccount, 0,
                                                0,//                             Debit,
                                                    TotalCosts * CurrencyRate,//                    Credit
                                                  0 - (TotalCosts * CurrencyRate),//              Total,
                                                    CurrencyID, CurrencyRate, 0 - TotalCosts,
                                                    BranchID, 0, VoucherDate, "", CompanyID, CreationUserId, "", trn);

                            }

                            //Credit Sales without Tax
                            if (TotalSales > 0)
                            {
                                clsJournalVoucherDetails.InsertJournalVoucherDetails(JVGuid, 0, SalesRefundAcc, 0,
                             TotalSales * CurrencyRate,//                                 Debit,
                                         0,//      Credit
                               TotalSales * CurrencyRate,//              Total,
                               CurrencyID, CurrencyRate, TotalSales ,
                             BranchID, 0, VoucherDate, "", CompanyID, CreationUserId, "", trn);
                            }
                            //===========================================

                            // Credit Sales Tax 
                            if (TotalSalesTax > 0)
                            {
                                clsJournalVoucherDetails.InsertJournalVoucherDetails(JVGuid, 0, SalesTaxAccount, 0,
                             TotalSalesTax * CurrencyRate,//                    Debit
                                    0,//                           Credit  ,

                                TotalSalesTax * CurrencyRate,//              Total,
                             CurrencyID, CurrencyRate, TotalSalesTax , BranchID, 0, VoucherDate, "", CompanyID, CreationUserId, "", trn);
                            }
                            //===========================================

                            // Credit  Special Sales Tax 
                            if (TotalSalesSpecialTax > 0)
                            {
                                clsJournalVoucherDetails.InsertJournalVoucherDetails(JVGuid, 0, SpecialSalesTaxAccount, 0,
                                       TotalSalesSpecialTax * CurrencyRate,//           Debit         
                              0,//                          Credit   ,

                               TotalSalesSpecialTax * CurrencyRate,//              Total,
                               CurrencyID, CurrencyRate, TotalSalesSpecialTax ,
                             BranchID, 0, VoucherDate, "", CompanyID, CreationUserId, "", trn);
                            }
                            //===========================================



                            // Debit  Discount ID 
                            if (TotalDiscount > 0)
                            {
                                clsJournalVoucherDetails.InsertJournalVoucherDetails(JVGuid, 0, SalesDiscountAcc, 0,

                             0,//               Debit     
                              TotalDiscount * CurrencyRate,//           Credit                  ,
                         (0 - TotalDiscount) * CurrencyRate,//              Total,
                           CurrencyID, CurrencyRate, (0 - TotalDiscount) ,
                          BranchID, 0, VoucherDate, "", CompanyID, CreationUserId, "", trn);
                            }
                            //===========================================


                            // Debit Customer ID 
                            if ((TotalSales + TotalSalesTax + TotalSalesSpecialTax - TotalDiscount) > 0)
                            {
                                clsJournalVoucherDetails.InsertJournalVoucherDetails(JVGuid, 0, businessPartnerAccount, businessPartnerID,

                             0,//                  Debit  
                                  (   TotalSales + TotalSalesTax + TotalSalesSpecialTax - TotalDiscount) * CurrencyRate,//    Credit                         ,
                        (0 - (TotalSales + TotalSalesTax + TotalSalesSpecialTax - TotalDiscount)) * CurrencyRate,//              Total,
                          CurrencyID, CurrencyRate, (0 - (TotalSales + TotalSalesTax + TotalSalesSpecialTax - TotalDiscount))
                          ,
                         BranchID, 0, VoucherDate, "", CompanyID, CreationUserId, "", trn);
                            }
                            //===========================================
                            if (PaymentMethodTypeID == (int)clsEnum.PaymentMethod.Cash)
                            {    // Credit  Cash ID 
                                if ((TotalSales + TotalSalesTax + TotalSalesSpecialTax - TotalDiscount) > 0)
                                {
                                    clsJournalVoucherDetails.InsertJournalVoucherDetails(JVGuid, 0, businessPartnerAccount, businessPartnerID,
                         (TotalSales + TotalSalesTax + TotalSalesSpecialTax - TotalDiscount) * CurrencyRate,//  Debit                 
                                        0,//                         Credit    ,

                              (TotalSales + TotalSalesTax + TotalSalesSpecialTax - TotalDiscount) * CurrencyRate,//              Total,
                                CurrencyID, CurrencyRate, (TotalSales + TotalSalesTax + TotalSalesSpecialTax - TotalDiscount)
                                 ,
                               BranchID, 0, VoucherDate, "", CompanyID, CreationUserId, "", trn);
                                }
                                // Debit  Cash ID 
                                if ((TotalSales + TotalSalesTax + TotalSalesSpecialTax - TotalDiscount) > 0)
                                {
                                    clsJournalVoucherDetails.InsertJournalVoucherDetails(JVGuid, 0, CashAccount, cashID,
                        0,//                 Debit   
                                       ( TotalSales + TotalSalesTax + TotalSalesSpecialTax - TotalDiscount) * CurrencyRate,//              Credit               ,

                         (0 - (TotalSales + TotalSalesTax + TotalSalesSpecialTax - TotalDiscount)) * CurrencyRate,//              Total,
                                 CurrencyID, CurrencyRate,
                                 (0 - (TotalSales + TotalSalesTax + TotalSalesSpecialTax - TotalDiscount)) ,
                                 BranchID, 0, VoucherDate, "", CompanyID, CreationUserId, "", trn);
                                }

                            }
                            else if (PaymentMethodTypeID == (int)clsEnum.PaymentMethod.Bank)
                            {    // Credit  Cash ID 
                                if ((TotalSales + TotalSalesTax + TotalSalesSpecialTax - TotalDiscount) > 0)
                                {
                                    clsJournalVoucherDetails.InsertJournalVoucherDetails(JVGuid, 0, businessPartnerAccount, businessPartnerID,
                        ( TotalSales + TotalSalesTax + TotalSalesSpecialTax - TotalDiscount) * CurrencyRate,//  Debit                 
                                        0,//                         Credit    ,

                              (TotalSales + TotalSalesTax + TotalSalesSpecialTax - TotalDiscount) * CurrencyRate,//              Total,
                               CurrencyID, CurrencyRate, (TotalSales + TotalSalesTax + TotalSalesSpecialTax - TotalDiscount)
                                ,
                               BranchID, 0, VoucherDate, "", CompanyID, CreationUserId, "", trn);
                                }
                                // Debit  Cash ID 
                                if ((TotalSales + TotalSalesTax + TotalSalesSpecialTax - TotalDiscount) > 0)
                                {
                                    clsJournalVoucherDetails.InsertJournalVoucherDetails(JVGuid, 0, BankAccount, bankID,
                        0,//                 Debit   
                                      (  TotalSales + TotalSalesTax + TotalSalesSpecialTax - TotalDiscount) * CurrencyRate,//              Credit               ,

                         (0 - (TotalSales + TotalSalesTax + TotalSalesSpecialTax - TotalDiscount))* CurrencyRate,//              Total,
                                 CurrencyID, CurrencyRate,
                                 (0 - (TotalSales + TotalSalesTax + TotalSalesSpecialTax - TotalDiscount))  
                                 , BranchID, 0, VoucherDate, "", CompanyID, CreationUserId, "", trn);
                                }

                            }
                        }
                        else if ( InvoiceType == (int)clsEnum.VoucherType.POSSalesInvoicereturn)
                        {//COGS
                            if (TotalCosts > 0)
                            {

                                clsJournalVoucherDetails.InsertJournalVoucherDetails(JVGuid, 0, InventoryAcc , 0,
                             TotalCosts * CurrencyRate,//                             Debit,
                            0,//                    Credit
                              TotalCosts * CurrencyRate,//              Total,
                              CurrencyID, CurrencyRate, TotalCosts,
                              BranchID, 0, VoucherDate, "", CompanyID, CreationUserId, "", trn);

                                clsJournalVoucherDetails.InsertJournalVoucherDetails(JVGuid, 0, COGSAccount, 0,
                                                0,//                             Debit,
                                                    TotalCosts * CurrencyRate,//                    Credit
                                                  0 - (TotalCosts * CurrencyRate),//              Total,
                                                    CurrencyID, CurrencyRate, 0 - TotalCosts,
                                                    BranchID, 0, VoucherDate, "", CompanyID, CreationUserId, "", trn);

                            }

                            //Credit Sales without Tax
                            if (TotalSales > 0)
                            {
                                clsJournalVoucherDetails.InsertJournalVoucherDetails(JVGuid, 0, SalesRefundAcc, 0,
                             TotalSales,//                                 Debit,
                                         0,//      Credit
                               TotalSales,//              Total,
                               CurrencyID, CurrencyRate, TotalSales/CurrencyRate,
                             BranchID, 0, VoucherDate, "", CompanyID, CreationUserId, "", trn);
                            }
                            //===========================================

                            // Credit Sales Tax 
                            if (TotalSalesTax > 0)
                            {
                                clsJournalVoucherDetails.InsertJournalVoucherDetails(JVGuid, 0, SalesTaxAccount, 0,
                             TotalSalesTax,//                    Debit
                                    0,//                           Credit  ,

                                TotalSalesTax,//              Total,
                             CurrencyID, CurrencyRate, TotalSalesTax/CurrencyRate, BranchID, 0, VoucherDate, "", CompanyID, CreationUserId, "", trn);
                            }
                            //===========================================

                            // Credit  Special Sales Tax 
                            if (TotalSalesSpecialTax > 0)
                            {
                                clsJournalVoucherDetails.InsertJournalVoucherDetails(JVGuid, 0, SpecialSalesTaxAccount, 0,
                                       TotalSalesSpecialTax,//           Debit         
                              0,//                          Credit   ,

                               TotalSalesSpecialTax,//              Total,
                               CurrencyID, CurrencyRate, TotalSalesSpecialTax/CurrencyRate,
                             BranchID, 0, VoucherDate, "", CompanyID, CreationUserId, "",trn);
                            }
                            //===========================================



                            // Debit  Discount ID 
                            if (TotalDiscount > 0)
                            {
                                clsJournalVoucherDetails.InsertJournalVoucherDetails(JVGuid, 0, SalesDiscountAcc, 0,

                             0,//               Debit     
                              TotalDiscount,//           Credit                  ,
                         (0 - TotalDiscount),//              Total,
                           CurrencyID, CurrencyRate, (0 - TotalDiscount)/CurrencyRate,
                          BranchID, 0, VoucherDate, "", CompanyID, CreationUserId,"", trn);
                            }
                            //===========================================


                            // Debit Customer ID 
                            if ((TotalSales + TotalSalesTax + TotalSalesSpecialTax - TotalDiscount) > 0)
                            {
                                clsJournalVoucherDetails.InsertJournalVoucherDetails(JVGuid, 0, businessPartnerAccount, businessPartnerID,

                             0,//                  Debit  
                                     TotalSales + TotalSalesTax + TotalSalesSpecialTax - TotalDiscount,//    Credit                         ,
                        (0 - (TotalSales + TotalSalesTax + TotalSalesSpecialTax - TotalDiscount)),//              Total,
                          CurrencyID, CurrencyRate, (0 - (TotalSales + TotalSalesTax + TotalSalesSpecialTax - TotalDiscount))/CurrencyRate,
                         BranchID, 0, VoucherDate, "", CompanyID, CreationUserId,"", trn);
                            }
                            //===========================================
                            if (PaymentMethodTypeID == (int)clsEnum.PaymentMethod.Cash)
                            {    // Credit  Cash ID 
                                if ((TotalSales + TotalSalesTax + TotalSalesSpecialTax - TotalDiscount) > 0)
                                {
                                    clsJournalVoucherDetails.InsertJournalVoucherDetails(JVGuid, 0, businessPartnerAccount, businessPartnerID,
                         TotalSales + TotalSalesTax + TotalSalesSpecialTax - TotalDiscount,//  Debit                 
                                        0,//                         Credit    ,

                              (TotalSales + TotalSalesTax + TotalSalesSpecialTax - TotalDiscount),//              Total,
                                CurrencyID, CurrencyRate, (TotalSales + TotalSalesTax + TotalSalesSpecialTax - TotalDiscount)/CurrencyRate,
                               BranchID, 0, VoucherDate, "", CompanyID, CreationUserId,"", trn);
                                }
                                // Debit  Cash ID 
                                if ((TotalSales + TotalSalesTax + TotalSalesSpecialTax - TotalDiscount) > 0)
                                {
                                    clsJournalVoucherDetails.InsertJournalVoucherDetails(JVGuid, 0, CashAccount, cashID,
                        0,//                 Debit   
                                        TotalSales + TotalSalesTax + TotalSalesSpecialTax - TotalDiscount,//              Credit               ,

                         (0 - (TotalSales + TotalSalesTax + TotalSalesSpecialTax - TotalDiscount)),//              Total,
                                 CurrencyID, CurrencyRate, (0 - (TotalSales + TotalSalesTax + TotalSalesSpecialTax - TotalDiscount))/CurrencyRate,
                                 BranchID, 0, VoucherDate, "", CompanyID, CreationUserId,"", trn);
                                }

                            }
                            else if (PaymentMethodTypeID == (int)clsEnum.PaymentMethod.Bank)
                            {    // Credit  Cash ID 
                                if ((TotalSales + TotalSalesTax + TotalSalesSpecialTax - TotalDiscount) > 0)
                                {
                                    clsJournalVoucherDetails.InsertJournalVoucherDetails(JVGuid, 0, businessPartnerAccount, businessPartnerID,
                         TotalSales + TotalSalesTax + TotalSalesSpecialTax - TotalDiscount,//  Debit                 
                                        0,//                         Credit    ,

                              (TotalSales + TotalSalesTax + TotalSalesSpecialTax - TotalDiscount),//              Total,
                               CurrencyID, CurrencyRate, (TotalSales + TotalSalesTax + TotalSalesSpecialTax - TotalDiscount)/CurrencyRate,
                               BranchID, 0, VoucherDate, "", CompanyID, CreationUserId, "", trn);
                                }
                                // Debit  Cash ID 
                                if ((TotalSales + TotalSalesTax + TotalSalesSpecialTax - TotalDiscount) > 0)
                                {
                                    clsJournalVoucherDetails.InsertJournalVoucherDetails(JVGuid, 0, BankAccount, bankID,
                        0,//                 Debit   
                                        TotalSales + TotalSalesTax + TotalSalesSpecialTax - TotalDiscount,//              Credit               ,

                         (0 - (TotalSales + TotalSalesTax + TotalSalesSpecialTax - TotalDiscount)),//              Total,
                                 CurrencyID, CurrencyRate, (0 - (TotalSales + TotalSalesTax + TotalSalesSpecialTax - TotalDiscount))/CurrencyRate
                                 , BranchID, 0, VoucherDate, "", CompanyID, CreationUserId, "", trn);
                                }

                            }
                        }
                        else if (InvoiceType == (int)clsEnum.VoucherType.PurchaseInvoice)
                        {

                            //Credit Sales without Tax
                            if (TotalSales > 0)
                            {
                                clsJournalVoucherDetails.InsertJournalVoucherDetails(JVGuid, 0, PurchaseInvoiceAcc, 0,
                             TotalSales * CurrencyRate,//                                 Debit,
                                         0,//      Credit
                               TotalSales*  CurrencyRate,//              Total,
                          CurrencyID, CurrencyRate, TotalSales,
                         BranchID, 0, VoucherDate, "", CompanyID, CreationUserId,"", trn);
                            }
                            //===========================================

                            // Credit Sales Tax 
                            if (TotalSalesTax > 0)
                            {
                                clsJournalVoucherDetails.InsertJournalVoucherDetails(JVGuid, 0, PurchaseTaxAccount, 0,
                             TotalSalesTax * CurrencyRate,//                    Debit
                                    0,//                           Credit  ,

                                TotalSalesTax * CurrencyRate,//              Total,
                            CurrencyID, CurrencyRate, TotalSalesTax,
                           BranchID, 0, VoucherDate, "", CompanyID, CreationUserId,"", trn);
                            }
                            //===========================================

                            // Credit  Special Sales Tax 
                            if (TotalSalesSpecialTax > 0)
                            {
                                clsJournalVoucherDetails.InsertJournalVoucherDetails(JVGuid, 0, SpecialPurchaseTaxAccount, 0,
                                       TotalSalesSpecialTax * CurrencyRate,//           Debit         
                              0,//                          Credit   ,

                               TotalSalesSpecialTax * CurrencyRate,//              Total,
                         CurrencyID, CurrencyRate, TotalSalesSpecialTax, BranchID, 0, VoucherDate, "", CompanyID, CreationUserId,"", trn);
                            }
                            //===========================================



                            // Debit  Discount ID 
                            if (TotalDiscount > 0)
                            {
                                clsJournalVoucherDetails.InsertJournalVoucherDetails(JVGuid, 0, PurchaseDiscountAcc, 0,

                             0,//               Debit     
                              TotalDiscount * CurrencyRate,//           Credit                  ,
                         (0 - TotalDiscount) * CurrencyRate,//              Total,
                           CurrencyID, CurrencyRate, (0 - TotalDiscount), BranchID, 0, VoucherDate, "", CompanyID, CreationUserId, "", trn);
                            }
                            //===========================================


                            // Debit Customer ID 
                            if ((TotalSales + TotalSalesTax + TotalSalesSpecialTax - TotalDiscount) > 0)
                            {
                                clsJournalVoucherDetails.InsertJournalVoucherDetails(JVGuid, 0, businessPartnerAccount,
                                    businessPartnerID,

                             0,//                  Debit  
                                   (  TotalSales + TotalSalesTax + TotalSalesSpecialTax - TotalDiscount) * CurrencyRate,//    Credit                         ,
                        (0 - (TotalSales + TotalSalesTax + TotalSalesSpecialTax - TotalDiscount)) * CurrencyRate,//              Total,
                           CurrencyID, CurrencyRate, (0 - (TotalSales + TotalSalesTax + TotalSalesSpecialTax - TotalDiscount)),
                          BranchID, 0, VoucherDate, "", CompanyID, CreationUserId,"", trn);
                            }
                            //===========================================



                            if (PaymentMethodTypeID == (int)clsEnum.PaymentMethod.Cash)
                            {    // Credit  Cash ID 
                                if ((TotalSales + TotalSalesTax + TotalSalesSpecialTax - TotalDiscount) > 0)
                                {
                                    clsJournalVoucherDetails.InsertJournalVoucherDetails(JVGuid, 0, businessPartnerAccount, businessPartnerID,
                         (TotalSales + TotalSalesTax + TotalSalesSpecialTax - TotalDiscount) * CurrencyRate,//  Debit                 
                                        0,//                         Credit    ,

                              (TotalSales + TotalSalesTax + TotalSalesSpecialTax - TotalDiscount) * CurrencyRate,//              Total,
                               CurrencyID, CurrencyRate, (TotalSales + TotalSalesTax + TotalSalesSpecialTax - TotalDiscount), BranchID, 0, VoucherDate, "", CompanyID, CreationUserId,"", trn);
                                }
                                // Debit  Cash ID 
                                if ((TotalSales + TotalSalesTax + TotalSalesSpecialTax - TotalDiscount) > 0)
                                {
                                    clsJournalVoucherDetails.InsertJournalVoucherDetails(JVGuid, 0, CashAccount, cashID,
                        0,//                 Debit   
                                       ( TotalSales + TotalSalesTax + TotalSalesSpecialTax - TotalDiscount) * CurrencyRate,//              Credit               ,

                         (0 - (TotalSales + TotalSalesTax + TotalSalesSpecialTax - TotalDiscount)) * CurrencyRate,//              Total,
                          CurrencyID, CurrencyRate, (0 - (TotalSales + TotalSalesTax + TotalSalesSpecialTax - TotalDiscount)),
                         BranchID, 0, VoucherDate, "", CompanyID, CreationUserId,"", trn);
                                }

                            }
                            else if (PaymentMethodTypeID == (int)clsEnum.PaymentMethod.Bank)
                            {    // Credit  Cash ID 
                                if ((TotalSales + TotalSalesTax + TotalSalesSpecialTax - TotalDiscount) > 0)
                                {
                                    clsJournalVoucherDetails.InsertJournalVoucherDetails(JVGuid, 0, businessPartnerAccount, businessPartnerID,
                        ( TotalSales + TotalSalesTax + TotalSalesSpecialTax - TotalDiscount) * CurrencyRate,//  Debit                 
                                        0,//                         Credit    ,

                              (TotalSales + TotalSalesTax + TotalSalesSpecialTax - TotalDiscount) * CurrencyRate,//              Total,
                                 CurrencyID, CurrencyRate, (TotalSales + TotalSalesTax + TotalSalesSpecialTax - TotalDiscount)
                                 , BranchID, 0, VoucherDate, "", CompanyID, CreationUserId, "", trn);
                                }
                                // Debit  Cash ID 
                                if ((TotalSales + TotalSalesTax + TotalSalesSpecialTax - TotalDiscount) > 0)
                                {
                                    clsJournalVoucherDetails.InsertJournalVoucherDetails(JVGuid, 0, BankAccount, bankID,
                        0,//                 Debit   
                                      (  TotalSales + TotalSalesTax + TotalSalesSpecialTax - TotalDiscount )* CurrencyRate,//              Credit               ,

                         (0 - (TotalSales + TotalSalesTax + TotalSalesSpecialTax - TotalDiscount)) * CurrencyRate,//              Total,
                                CurrencyID, CurrencyRate, (0 - (TotalSales + TotalSalesTax + TotalSalesSpecialTax - TotalDiscount))
                                , BranchID, 0, VoucherDate, "", CompanyID, CreationUserId, "", trn);
                                }

                            }

                        }
                        else if (InvoiceType == (int)clsEnum.VoucherType.PurchaseRefund)
                        {

                            //Credit Sales without Tax
                            if (TotalSales > 0)
                            {
                                clsJournalVoucherDetails.InsertJournalVoucherDetails(JVGuid, 0, PurchaseRefundAcc, 0,
                                       0, //                                 Debit,
                                       TotalSales * CurrencyRate,//      Credit
                           (0 - TotalSales) *   CurrencyRate,//              Total,
                              CurrencyID, CurrencyRate, (0 - TotalSales)
                              , BranchID, 0, VoucherDate, "", CompanyID, CreationUserId, "", trn);
                            }
                            //===========================================

                            // Credit Sales Tax 
                            if (TotalSalesTax > 0)
                            {
                                clsJournalVoucherDetails.InsertJournalVoucherDetails(JVGuid, 0, PurchaseTaxAccount, 0,
                               0, //                    Debit
                               TotalSalesTax * CurrencyRate,//                           Credit  ,

                               (0 - TotalSalesTax) * CurrencyRate,//              Total,
                         CurrencyID, CurrencyRate, (0 - TotalSalesTax),
                         BranchID, 0, VoucherDate, "", CompanyID, CreationUserId,"", trn);
                            }
                            //===========================================

                            // Credit  Special Sales Tax 
                            if (TotalSalesSpecialTax > 0)
                            {
                                clsJournalVoucherDetails.InsertJournalVoucherDetails(JVGuid, 0, SpecialPurchaseTaxAccount, 0,
                                    0,//           Debit         
                         TotalSalesSpecialTax * CurrencyRate,   //                          Credit   ,

                             (0 - TotalSalesSpecialTax) * CurrencyRate,//              Total,
                               CurrencyID, CurrencyRate, (0 - TotalSalesSpecialTax),
                             BranchID, 0, VoucherDate, "", CompanyID, CreationUserId,"", trn);
                            }
                            //===========================================



                            // Debit  Discount ID 
                            if (TotalDiscount > 0)
                            {
                                clsJournalVoucherDetails.InsertJournalVoucherDetails(JVGuid, 0, PurchaseDiscountAcc, 0,

                            TotalDiscount * CurrencyRate,//               Debit     
                            0,  //           Credit                  ,
                          TotalDiscount * CurrencyRate,//              Total,
                            CurrencyID, CurrencyRate, TotalDiscount, BranchID, 0, VoucherDate, "", CompanyID, CreationUserId,"", trn);
                            }
                            //===========================================


                            // Debit Customer ID 
                            if ((TotalSales + TotalSalesTax + TotalSalesSpecialTax - TotalDiscount) > 0)
                            {
                                clsJournalVoucherDetails.InsertJournalVoucherDetails(JVGuid, 0, businessPartnerAccount, businessPartnerID,

                         (  TotalSales + TotalSalesTax + TotalSalesSpecialTax - TotalDiscount) * CurrencyRate,//                  Debit  
                               0,     //    Credit                         ,
                        (TotalSales + TotalSalesTax + TotalSalesSpecialTax - TotalDiscount) * CurrencyRate,//              Total,
                       CurrencyID, CurrencyRate, (TotalSales + TotalSalesTax + TotalSalesSpecialTax - TotalDiscount),
                       BranchID, 0, VoucherDate, "", CompanyID, CreationUserId,"", trn);
                            }
                            //===========================================
                            if (PaymentMethodTypeID == (int)clsEnum.PaymentMethod.Cash)
                            {    // Credit  Cash ID 
                                if ((TotalSales + TotalSalesTax + TotalSalesSpecialTax - TotalDiscount) > 0)
                                {
                                    clsJournalVoucherDetails.InsertJournalVoucherDetails(JVGuid, 0, businessPartnerAccount, businessPartnerID,
                              0,  //  Debit                 
                              ( TotalSales + TotalSalesTax + TotalSalesSpecialTax - TotalDiscount) * CurrencyRate,//                         Credit    ,

                     (0 - (TotalSales + TotalSalesTax + TotalSalesSpecialTax - TotalDiscount)) * CurrencyRate,//              Total,
                    CurrencyID, CurrencyRate, (0 - (TotalSales + TotalSalesTax + TotalSalesSpecialTax - TotalDiscount)),
                  BranchID, 0, VoucherDate, "", CompanyID, CreationUserId, "", trn);
                                }
                                // Debit  Cash ID 
                                if ((TotalSales + TotalSalesTax + TotalSalesSpecialTax - TotalDiscount) > 0)
                                {
                                    clsJournalVoucherDetails.InsertJournalVoucherDetails(JVGuid, 0, CashAccount, cashID,
                                 ( TotalSales + TotalSalesTax + TotalSalesSpecialTax - TotalDiscount) * CurrencyRate,  //                 Debit   
                                   0,//              Credit               ,

                           (TotalSales + TotalSalesTax + TotalSalesSpecialTax - TotalDiscount) * CurrencyRate,//              Total,
                                CurrencyID, CurrencyRate, (TotalSales + TotalSalesTax + TotalSalesSpecialTax - TotalDiscount)
                                , BranchID, 0, VoucherDate, "", CompanyID, CreationUserId,"", trn);
                                }

                            }
                            else if (PaymentMethodTypeID == (int)clsEnum.PaymentMethod.Bank)
                            {    // Credit  Cash ID 
                                if ((TotalSales + TotalSalesTax + TotalSalesSpecialTax - TotalDiscount) > 0)
                                {
                                    clsJournalVoucherDetails.InsertJournalVoucherDetails(JVGuid, 0, businessPartnerAccount, businessPartnerID,
                              0,  //  Debit                 
                              ( TotalSales + TotalSalesTax + TotalSalesSpecialTax - TotalDiscount) * CurrencyRate,//                         Credit    ,

                     (0 - (TotalSales + TotalSalesTax + TotalSalesSpecialTax - TotalDiscount)) * CurrencyRate,//              Total,
                               CurrencyID, CurrencyRate, (0 - (TotalSales + TotalSalesTax + TotalSalesSpecialTax - TotalDiscount)),
                               BranchID, 0, VoucherDate, "", CompanyID, CreationUserId, "", trn);
                                }
                                // Debit  Cash ID 
                                if ((TotalSales + TotalSalesTax + TotalSalesSpecialTax - TotalDiscount) > 0)
                                {
                                    clsJournalVoucherDetails.InsertJournalVoucherDetails(JVGuid, 0, BankAccount, bankID,
                                 ( TotalSales + TotalSalesTax + TotalSalesSpecialTax - TotalDiscount) * CurrencyRate,  //                 Debit   
                                   0,//              Credit               ,

                           (TotalSales + TotalSalesTax + TotalSalesSpecialTax - TotalDiscount),//              Total,
                                  CurrencyID, CurrencyRate, (TotalSales + TotalSalesTax + TotalSalesSpecialTax - TotalDiscount),
                                 BranchID, 0, VoucherDate, "", CompanyID, CreationUserId, "", trn);
                                }

                            }

                        }
                        else if (InvoiceType == (int)clsEnum.VoucherType.GoodIssue)
                        {
                            //Credit store without Tax
                            if (TotalSales > 0)
                            {
                                clsJournalVoucherDetails.InsertJournalVoucherDetails(JVGuid, 0, InventoryAcc, 0,
                              0,//                             Debit,
                              TotalSales * CurrencyRate,//                    Credit
                              (0 - TotalSales) * CurrencyRate,//              Total,
                                CurrencyID, CurrencyRate, (0 - TotalSales),
                              BranchID, 0, VoucherDate, "", CompanyID, CreationUserId, "", trn);
                            }
                            //===========================================

                            // Credit Sales Tax 
                            if (TotalSalesTax > 0)
                            {
                                clsJournalVoucherDetails.InsertJournalVoucherDetails(JVGuid, 0, SalesTaxAccount, 0,
                              0,//                             Debit,
                              TotalSalesTax * CurrencyRate,//                    Credit
                              (0 - TotalSalesTax) * CurrencyRate,//              Total,
                           CurrencyID, CurrencyRate, (0 - TotalSalesTax),
                          BranchID, 0, VoucherDate, "", CompanyID, CreationUserId, "",trn);
                            }
                            //===========================================

                            // Credit  Special Sales Tax 
                            if (TotalSalesSpecialTax > 0)
                            {
                                clsJournalVoucherDetails.InsertJournalVoucherDetails(JVGuid, 0, SpecialSalesTaxAccount, 0,
                              0,//                             Debit,
                              TotalSalesSpecialTax * CurrencyRate,//                    Credit
                              (0 - TotalSalesSpecialTax) * CurrencyRate,//              Total,
                           CurrencyID, CurrencyRate, (0 - TotalSalesSpecialTax),
                           BranchID, 0, VoucherDate, "", CompanyID, CreationUserId,"", trn);
                            }
                            //===========================================



                            // Debit  Discount ID 
                            if (TotalDiscount > 0)
                            {
                                clsJournalVoucherDetails.InsertJournalVoucherDetails(JVGuid, 0, SalesDiscountAcc, 0,
                     TotalDiscount * CurrencyRate,//                             Debit,
                             0,//                    Credit
                           TotalDiscount * CurrencyRate,//              Total,
                            CurrencyID, CurrencyRate, TotalDiscount, BranchID, 
                            0, VoucherDate, "", CompanyID, CreationUserId, "", trn);
                            }
                            //===========================================


                            // Debit Account ID 
                            if ((TotalSales + TotalSalesTax + TotalSalesSpecialTax - TotalDiscount) > 0)
                            {
                                clsJournalVoucherDetails.InsertJournalVoucherDetails(JVGuid, 0, AccountID, 0,
                    ( TotalSales + TotalSalesTax + TotalSalesSpecialTax - TotalDiscount) * CurrencyRate,//                             Debit,
                             0,//                    Credit
                           ( TotalSales + TotalSalesTax + TotalSalesSpecialTax - TotalDiscount) * CurrencyRate,//              Total,
                               CurrencyID, CurrencyRate,( TotalSales + TotalSalesTax + TotalSalesSpecialTax - TotalDiscount), BranchID, 0, VoucherDate, "", CompanyID, CreationUserId, "",trn);
                            }

                        }
                        else if (InvoiceType == (int)clsEnum.VoucherType.GoodRecipt)
                        {
                            //Credit store without Tax
                            if (TotalSales > 0)
                            {
                                clsJournalVoucherDetails.InsertJournalVoucherDetails(JVGuid, 0, InventoryAcc, 0,
                               TotalSales * CurrencyRate, //                             Debit,
                               0,//                    Credit
                                TotalSales * CurrencyRate,//              Total,
                        CurrencyID, CurrencyRate, TotalSales,
                       BranchID, 0, VoucherDate, "", CompanyID, CreationUserId,"", trn);
                            }
                            //===========================================

                            // Credit Sales Tax 
                            if (TotalSalesTax > 0)
                            {
                                clsJournalVoucherDetails.InsertJournalVoucherDetails(JVGuid, 0, SalesTaxAccount, 0,
                               TotalSalesTax * CurrencyRate,//                             Debit,
                          0,  //                    Credit
                                TotalSalesTax * CurrencyRate,//              Total,
                             CurrencyID, CurrencyRate, TotalSalesTax,
                            BranchID, 0, VoucherDate, "", CompanyID, CreationUserId,"", trn);
                            }
                            //===========================================

                            // Credit  Special Sales Tax 
                            if (TotalSalesSpecialTax > 0)
                            {
                                clsJournalVoucherDetails.InsertJournalVoucherDetails(JVGuid, 0, SpecialSalesTaxAccount, 0,
                             TotalSalesSpecialTax * CurrencyRate,//                             Debit,
                             0,  //                    Credit
                              TotalSalesSpecialTax * CurrencyRate,//              Total,
                            CurrencyID, CurrencyRate, TotalSalesSpecialTax, BranchID, 0, VoucherDate, "", CompanyID, CreationUserId, "", trn);
                            }
                            //===========================================



                            // Debit  Discount ID 
                            if (TotalDiscount > 0)
                            {
                                clsJournalVoucherDetails.InsertJournalVoucherDetails(JVGuid, 0, SalesDiscountAcc, 0,
                        0, //                             Debit,
                        TotalDiscount * CurrencyRate,//                    Credit
                          (0 - TotalDiscount) * CurrencyRate,//              Total,
                            CurrencyID, CurrencyRate, (0 - TotalDiscount),
                          BranchID, 0, VoucherDate, "", CompanyID, CreationUserId, "",trn);
                            }
                            //===========================================


                            // Debit Account ID 
                            if ((TotalSales + TotalSalesTax + TotalSalesSpecialTax - TotalDiscount) > 0)
                            {
                                clsJournalVoucherDetails.InsertJournalVoucherDetails(JVGuid, 0, AccountID, 0,
                        0, //                             Debit,
                       ( TotalSales + TotalSalesTax + TotalSalesSpecialTax - TotalDiscount) * CurrencyRate,//                    Credit
                        (0 - (TotalSales + TotalSalesTax + TotalSalesSpecialTax - TotalDiscount)) * CurrencyRate,//              Total,
                            CurrencyID, CurrencyRate, (0 - (TotalSales + TotalSalesTax + TotalSalesSpecialTax - TotalDiscount)),
                            BranchID, 0, VoucherDate, "", CompanyID, CreationUserId, "",trn);
                            }

                        }

                        if (clsJournalVoucherHeader.CheckJVMatch(JVGuid,CompanyID, trn)) { return true; }
                        else
                        {
                            return false;

                        }


                    }
                    else
                    {
                        return false;
                    }


                }
                else { return true; }







            }
            catch (Exception)
            {
                return false;

            }
        }
        public int GetValueFromDT(DataTable dt, string Col, string Value, int NeededColIndex)
        {
            try
            {
                string searchExpression = Col + "  =  " + Value;
                DataRow[] foundRows = dt.Select(searchExpression);
                if (foundRows.Length > 0)
                {

                    return Simulate.Integer32(foundRows[0].ItemArray[2]);
                }
                else { return 0; }
            }
            catch (Exception)
            {

                return 0;
            }

        }


    }
    public class DBInvoiceHeader
    {


        public Guid? Guid { get; set; }
        public int InvoiceNo { get; set; }
        public DateTime InvoiceDate { get; set; }
        public int PaymentMethodID { get; set; }
        public int BranchID { get; set; }
        public string Note { get; set; }
        public int BusinessPartnerID { get; set; }
        public int StoreID { get; set; }
        public int InvoiceTypeID { get; set; }
        public bool IsCounted { get; set; }
        public Guid JVGuid { get; set; }
        public decimal TotalTax { get; set; }
        public decimal HeaderDiscount { get; set; }
        public decimal TotalDiscount { get; set; }
        public decimal TotalInvoice { get; set; }

        public string RefNo { get; set; }
        public Guid RelatedInvoiceGuid { get; set; }
        public int CashID { get; set; }
        public int BankID { get; set; }
        public int tableID { get; set; }
        public int status { get; set; }
      


        public Guid POSDayGuid { get; set; }
        public Guid POSSessionGuid { get; set; }
        public int CompanyID { get; set; }
        public int AccountID { get; set; }
        public int CreationUserID { get; set; }
        public DateTime CreationDate { get; set; }
        public int? ModificationUserID { get; set; }
        public DateTime? ModificationDate { get; set; }
        public int CurrencyID { get; set; }
        public decimal CurrencyRate { get; set; }
        public decimal CurrencyBaseAmount { get; set; }
        
    }
}
