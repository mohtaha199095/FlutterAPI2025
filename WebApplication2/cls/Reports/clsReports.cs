using System;
using System.Data;
using System.Data.SqlClient;

namespace WebApplication2.cls.Reports
{
    public class clsReports
    {
        public DataTable SelectTrialBalance(DateTime Date1, DateTime Date2, int BranchID, int CostCenterID, int CompanyID)
        {
            try
            {
                SqlParameter[] prm =
                 {
                     new SqlParameter("@date1", SqlDbType.Date) { Value = Date1 },
                     new SqlParameter("@Date2", SqlDbType.Date) { Value = Date2 },
                     new SqlParameter("@BranchID", SqlDbType.Int) { Value = BranchID },
                     new SqlParameter("@CostCenterID", SqlDbType.Int) { Value = CostCenterID },

                     new SqlParameter("@CompanyID", SqlDbType.Int) { Value = CompanyID },


                };

                string a = @"
select  tbl_Accounts.ID,tbl_Accounts.AccountNumber,tbl_Accounts.AName,tbl_Accounts.EName,
isnull(
(select sum(total) from tbl_JournalVoucherDetails inner join tbl_JournalVoucherHeader 
                             on tbl_JournalVoucherHeader.guid = tbl_JournalVoucherDetails.Parentguid 
                            where (AccountID in (select *from dbo.GetAccountInParent( tbl_Accounts.ID))) 
					             and cast( tbl_JournalVoucherHeader.VoucherDate as date )< @date1  
								  and (tbl_JournalVoucherDetails.BranchID= @BranchID or  @BranchID=0)
								    and (tbl_JournalVoucherDetails.CostCenterID= @CostCenterID or  @CostCenterID=0)
								 and (tbl_JournalVoucherDetails.CompanyID= @companyId or  @companyId=0) ) ,0.000)              as OpeningBalance ,

								isnull( (select sum(debit) from tbl_JournalVoucherDetails inner join tbl_JournalVoucherHeader 
                             on tbl_JournalVoucherHeader.guid = tbl_JournalVoucherDetails.Parentguid 
                            where (AccountID in (select *from dbo.GetAccountInParent( tbl_Accounts.ID))) 
					             and cast( tbl_JournalVoucherHeader.VoucherDate as date )between @date1 and @date2  
								 and (tbl_JournalVoucherDetails.BranchID= @BranchID or  @BranchID=0)
								    and (tbl_JournalVoucherDetails.CostCenterID= @CostCenterID or  @CostCenterID=0)
								 and (tbl_JournalVoucherDetails.CompanyID= @companyId or  @companyId=0) )   ,0.000)               as Debit ,

								 isnull(		 (select sum(Credit) from tbl_JournalVoucherDetails inner join tbl_JournalVoucherHeader 
                             on tbl_JournalVoucherHeader.guid = tbl_JournalVoucherDetails.Parentguid 
                            where (AccountID in (select *from dbo.GetAccountInParent( tbl_Accounts.ID))) 
					             and cast( tbl_JournalVoucherHeader.VoucherDate as date )between @date1 and @date2 
								 and (tbl_JournalVoucherDetails.BranchID= @BranchID or  @BranchID=0)
								    and (tbl_JournalVoucherDetails.CostCenterID= @CostCenterID or  @CostCenterID=0) 
								 and (tbl_JournalVoucherDetails.CompanyID= @companyId or  @companyId=0) )    ,0.000)              as Credit ,

                           isnull( (select sum(total) from tbl_JournalVoucherDetails inner join tbl_JournalVoucherHeader 
                             on tbl_JournalVoucherHeader.guid = tbl_JournalVoucherDetails.Parentguid 
                            where (AccountID in (select *from dbo.GetAccountInParent( tbl_Accounts.ID))) 
							and (tbl_JournalVoucherDetails.BranchID= @BranchID or  @BranchID=0)
								    and (tbl_JournalVoucherDetails.CostCenterID= @CostCenterID or  @CostCenterID=0)
					              and (tbl_JournalVoucherDetails.CompanyID= @companyId or  @companyId=0) )        ,0.000)          as EndingBalance 
from tbl_Accounts 
where (CompanyID= @companyId or  @companyId=0)
order by AccountNumber asc"; clsSQL clsSQL = new clsSQL();

                DataTable dt = clsSQL.ExecuteQueryStatement(a, prm);

                return dt;
            }
            catch (Exception)
            {

                throw;
            }


        }
        public DataTable SelectAccountStatement(DateTime Date1, DateTime Date2, int BranchID, int CostCenterID, int Accountid, int subAccountid, int CompanyID)
        {
            try
            {
                clsSQL clsSQL = new clsSQL();

                SqlParameter[] prm =
                 {
                     new SqlParameter("@date1", SqlDbType.Date) { Value = Date1 },
                     new SqlParameter("@Date2", SqlDbType.Date) { Value = Date2 },
                     new SqlParameter("@BranchID", SqlDbType.Int) { Value = BranchID },
                     new SqlParameter("@CostCenterID", SqlDbType.Int) { Value = CostCenterID },
                                          new SqlParameter("@Accountid", SqlDbType.Int) { Value = Accountid },
                                                               new SqlParameter("@subAccountid", SqlDbType.Int) { Value = subAccountid },


                     new SqlParameter("@CompanyID", SqlDbType.Int) { Value = CompanyID },


                };

                string a = @" select NEWID() as guid
 ,NEWID() as parentguid

 ,0 as accountid,
 0 as subaccountid,
 0 as debit,
 0 as credit,
 isnull(sum(total) ,0) AS total ,
 0 as branchid,
 0 as costcenterid,
 '2000-01-01'as duedate,
 'Opening balance' as note ,
 0 as companyid
,0 as CreationUserID,
'2000-01-01' as creationdate,
0 as  ModificationUserID,
'2000-01-01' as  ModificationDate,
'' as branchName,
'' as costCenterName,
0 as JVTypeID,
'2000-01-01'as voucherdate,
'OP' as voucherType
 ,0 as JVNumber,
 '' as AccountNumber,
'' as AccountEname
 from tbl_JournalVoucherDetails 
inner join tbl_accounts on accountid=tbl_accounts.id
inner join tbl_JournalVoucherHeader on tbl_JournalVoucherHeader.guid =tbl_journalvoucherdetails.parentguid and  tbl_JournalVoucherHeader.companyid =tbl_journalvoucherdetails.companyid
left join tbl_Branch on tbl_branch.id=tbl_JournalVoucherDetails.branchid
left join tbl_costCenter on tbl_costCenter.id=tbl_JournalVoucherDetails.CostCenterID
where(tbl_JournalVoucherDetails.companyid=@companyID or @companyid=0)
 and (tbl_JournalVoucherDetails.AccountID=@Accountid or @Accountid=0)
 
 and (tbl_JournalVoucherDetails.BranchID=@BranchID or @BranchID=0)
 and (tbl_JournalVoucherDetails.CostCenterID=@CostCenterID or @CostCenterID=0)
and (tbl_JournalVoucherDetails.SubAccountID=@Subaccountid or @Subaccountid=0)
and cast ( tbl_journalvoucherheader.voucherdate as date) < cast( @date1 as date) 
union all
select tbl_JournalVoucherDetails.*
,tbl_branch.AName
,tbl_costCenter.AName
,tbl_JournalVoucherHeader.JVTypeID 
,tbl_JournalVoucherHeader.VoucherDate
 ,tbl_journalvouchertypes.ename as voucherType
 ,tbl_JournalVoucherHeader.JVNumber
 , tbl_accounts.ename as AccountEname
 , tbl_accounts.AccountNumber as AccountNumber
  from tbl_JournalVoucherDetails 
inner join tbl_accounts on accountid=tbl_accounts.id
inner join tbl_JournalVoucherHeader on tbl_JournalVoucherHeader.guid =tbl_journalvoucherdetails.parentguid and  tbl_JournalVoucherHeader.companyid =tbl_journalvoucherdetails.companyid
left join tbl_Branch on tbl_branch.id=tbl_JournalVoucherDetails.branchid
left join tbl_costCenter on tbl_costCenter.id=tbl_JournalVoucherDetails.CostCenterID
left join tbl_journalvouchertypes on tbl_journalvouchertypes.id = jvtypeid
where(tbl_JournalVoucherDetails.companyid=@companyID or @companyid=0)
 and (tbl_JournalVoucherDetails.AccountID=@Accountid or @Accountid=0)
 and (tbl_JournalVoucherDetails.BranchID=@BranchID or @BranchID=0)
 and (tbl_JournalVoucherDetails.CostCenterID=@CostCenterID or @CostCenterID=0)
and (tbl_JournalVoucherDetails.SubAccountID=@Subaccountid or @Subaccountid=0)
and cast ( tbl_journalvoucherheader.voucherdate as date) between cast( @date1 as date) and cast(@date2 as date)";
                DataTable dt = clsSQL.ExecuteQueryStatement(a, prm);
                dt.Columns.Add("netTotal");
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    if (i > 0)
                        dt.Rows[i]["nettotal"] = Simulate.Val(dt.Rows[i]["debit"]) + Simulate.Val(dt.Rows[i - 1]["nettotal"]) - Simulate.Val(dt.Rows[i]["credit"]);
                }
                return dt;
            }
            catch (Exception)
            {

                throw;
            }


        }
        public DataTable SelectInvoicesByFilter(DateTime Date1, DateTime Date2, bool WithDateFilter, int PaymentMethodID, int BranchID, int BusinessPartnerID, int storeid, int invoiceTypeid, int cashDrawerID, int IsCounted, int CompanyID)
        {
            try
            {
                clsSQL clsSQL = new clsSQL();

                SqlParameter[] prm =
                 {
                     new SqlParameter("@date1", SqlDbType.Date) { Value = Date1 },
                     new SqlParameter("@Date2", SqlDbType.Date) { Value = Date2 },
 new SqlParameter("@WithDateFilter", SqlDbType.Bit) { Value = WithDateFilter },
  new SqlParameter("@cashDrawerID", SqlDbType.Int) { Value = cashDrawerID },
    new SqlParameter("@invoiceTypeid", SqlDbType.Int) { Value = invoiceTypeid },
        new SqlParameter("@storeid", SqlDbType.Int) { Value = storeid },
         new SqlParameter("@BusinessPartnerID", SqlDbType.Int) { Value = BusinessPartnerID },
         new SqlParameter("@PaymentMethodID", SqlDbType.Int) { Value = PaymentMethodID },
                                                   new SqlParameter("@BranchID", SqlDbType.Int) { Value = BranchID },
                    new SqlParameter("@CompanyID", SqlDbType.Int) { Value = CompanyID },
                     new SqlParameter("@IsCounted", SqlDbType.Int) { Value = IsCounted },
                };

                string a = @" 
select tbl_invoiceheader.* 
,tbl_paymentmethod.AName as PaymentmethodName
,tbl_branch.AName as BranchName
,tbl_businesspartner.AName as BusinesspartnerName
,tbl_store.AName as StoreName
,tbl_JournalVoucherTypes.AName as JournalVoucherTypesName
,tbl_cashdrawer.AName as CashdrawerName
 ,tbl_employee.AName as EmployeeName
from tbl_invoiceheader
left join tbl_paymentmethod on tbl_invoiceheader.paymentmethodid= tbl_paymentmethod.id
left join tbl_branch on tbl_invoiceheader.branchid= tbl_branch.id
left join tbl_businesspartner on tbl_invoiceheader. businesspartnerid= tbl_businesspartner.id
left join tbl_store on tbl_invoiceheader. storeid= tbl_store.id
left join tbl_JournalVoucherTypes on tbl_invoiceheader. invoiceTypeid= tbl_JournalVoucherTypes.id
left join tbl_cashdrawer on tbl_invoiceheader. cashID= tbl_cashdrawer.id
left join tbl_employee on tbl_invoiceheader.creationUserID= tbl_employee.id
where 
(tbl_invoiceheader.branchid =@branchID or @branchID=0)
and (tbl_invoiceheader.paymentmethodid  =@paymentmethodID  or @paymentmethodID =0)
and (tbl_invoiceheader.businesspartnerid  =@businesspartnerid  or @businesspartnerid  =0)
and (tbl_invoiceheader.storeid  =@storeid  or @storeid  =0)
and (tbl_invoiceheader.invoiceTypeid  =@invoiceTypeid  or @invoiceTypeid  =0)
and (tbl_invoiceheader.CashID  =@cashDrawerID  or @cashDrawerID  =0)
and (tbl_invoiceheader.CompanyID  =@CompanyID  or @CompanyID  =0)
and (tbl_invoiceheader.iscounted  =@iscounted  or @iscounted  =-1)
and (@WithDateFilter=0 or cast(invoicedate as date)between cast(@date1 as date)and cast(@date2 as date))

order by tbl_invoiceheader.invoicedate







";
                DataTable dt = clsSQL.ExecuteQueryStatement(a, prm);


                return dt;
            }
            catch (Exception)
            {

                throw;
            }


        }
        public DataTable SelectItemTransactionsByFilter(DateTime Date1, DateTime Date2, bool WithDateFilter,
            string Itemguid, int BranchID, int BusinessPartnerID, int storeid,
            int invoiceTypeid, int IsCounted, int CompanyID)
        {
            try
            {
                clsSQL clsSQL = new clsSQL();

                SqlParameter[] prm =
                 {   new SqlParameter("@BranchID", SqlDbType.Int) { Value = BranchID },
                    new SqlParameter("@BusinessPartnerID", SqlDbType.Int) { Value = BusinessPartnerID },
     new SqlParameter("@storeid", SqlDbType.Int) { Value = storeid },
     new SqlParameter("@date1", SqlDbType.Date) { Value = Date1 },
                     new SqlParameter("@Date2", SqlDbType.Date) { Value = Date2 },
 new SqlParameter("@WithDateFilter", SqlDbType.Bit) { Value = WithDateFilter },
          new SqlParameter("@IsCounted", SqlDbType.Int) { Value = IsCounted },
      new SqlParameter("@CompanyID", SqlDbType.Int) { Value = CompanyID },
          new SqlParameter("@invoiceTypeid", SqlDbType.Int) { Value = invoiceTypeid },

  new SqlParameter("@Itemguid", SqlDbType.UniqueIdentifier) { Value =Simulate.Guid( Itemguid) },







                };

                string a = @"   
  
 
 


select @itemguid, '2000-01-01'invoicedate
,''itemAName 
,''  as branchName
,'' as businesspartnerName
,''  as storeName
,'Opening Balance'  as journalVoucherTypesName
,isnull(sum(tbl_InvoiceDetails.totalqty*tbl_JournalVoucherTypes.qtyfactor),0) as totalqty
, 0 as priceBeforeTax
, 0 as totalLine
   from tbl_InvoiceDetails
 inner join tbl_Items on tbl_Items.guid =  itemguid

 
left join tbl_branch on tbl_InvoiceDetails.branchid= tbl_branch.id
left join tbl_businesspartner on tbl_InvoiceDetails. businesspartnerid= tbl_businesspartner.id
left join tbl_store on tbl_InvoiceDetails. storeid= tbl_store.id
left join tbl_JournalVoucherTypes on tbl_InvoiceDetails. invoiceTypeid= tbl_JournalVoucherTypes.id
 


 where
(tbl_InvoiceDetails. companyid=@companyid or @companyid=0)
and ( tbl_InvoiceDetails.BranchID=@BranchID or @BranchID=0)
and (tbl_InvoiceDetails. itemguid=@itemguid or @itemguid='00000000-0000-0000-0000-000000000000')
and (tbl_InvoiceDetails. businesspartnerid=@businesspartnerid or @businesspartnerid =0)
and (tbl_InvoiceDetails. storeid=@storeid or @storeid =0)
and ( tbl_InvoiceDetails. invoiceTypeid=@invoiceTypeid or @invoiceTypeid =0)
and (tbl_InvoiceDetails. iscounted  =@iscounted  or @iscounted  =-1)
and (@WithDateFilter=1 and cast(tbl_InvoiceDetails.invoicedate as date)< cast(@date1 as date) )
 --=====================================================================-
union all
select tbl_InvoiceDetails.ItemGuid,tbl_InvoiceDetails.invoicedate,tbl_Items.aName 
,tbl_branch.AName as branchName
,tbl_businesspartner.AName as businesspartnerName
,tbl_store.AName as storeName
,tbl_JournalVoucherTypes.AName as journalVoucherTypesName
,tbl_InvoiceDetails.totalqty*tbl_JournalVoucherTypes.qtyfactor as totalqty
,tbl_InvoiceDetails.priceBeforeTax as priceBeforeTax
,tbl_InvoiceDetails.TotalLine as totalLine
   from tbl_InvoiceDetails
 inner join tbl_Items on tbl_Items.guid =  itemguid

 
left join tbl_branch on tbl_InvoiceDetails.branchid= tbl_branch.id
left join tbl_businesspartner on tbl_InvoiceDetails. businesspartnerid= tbl_businesspartner.id
left join tbl_store on tbl_InvoiceDetails. storeid= tbl_store.id
left join tbl_JournalVoucherTypes on tbl_InvoiceDetails. invoiceTypeid= tbl_JournalVoucherTypes.id
 


 where
(tbl_InvoiceDetails. companyid=@companyid or @companyid=0)
and ( tbl_InvoiceDetails.BranchID=@BranchID or @BranchID=0)
and (tbl_InvoiceDetails. itemguid=@itemguid or @itemguid='00000000-0000-0000-0000-000000000000')
and (tbl_InvoiceDetails. businesspartnerid=@businesspartnerid or @businesspartnerid =0)
and (tbl_InvoiceDetails. storeid=@storeid or @storeid =0)
and ( tbl_InvoiceDetails. invoiceTypeid=@invoiceTypeid or @invoiceTypeid =0)
and (tbl_InvoiceDetails. iscounted  =@iscounted  or @iscounted  =-1)
and (@WithDateFilter=0 or cast(tbl_InvoiceDetails.invoicedate as date)between cast(@date1 as date)and cast(@date2 as date))
 ORDER by invoicedate
  

";
                DataTable dt = clsSQL.ExecuteQueryStatement(a, prm);


                return dt;
            }
            catch (Exception)
            {

                throw;
            }


        }
        public DataTable SelectInventoryReportByFilter(DateTime Date1, DateTime Date2, bool WithDateFilter,
          string Itemguid, int BranchID, int categoryid, int storeid,
            int CompanyID)
        {
            try
            {
                clsSQL clsSQL = new clsSQL();

                SqlParameter[] prm =
                 { new SqlParameter("@BranchID", SqlDbType.Int) { Value = BranchID },
                   new SqlParameter("@Itemguid", SqlDbType.UniqueIdentifier) { Value =Simulate.Guid( Itemguid) },
                     new SqlParameter("@storeid", SqlDbType.Int) { Value = storeid },
                    new SqlParameter("@categoryid", SqlDbType.Int) { Value = categoryid },
                    new SqlParameter("@CompanyID", SqlDbType.Int) { Value = CompanyID },
                      new SqlParameter("@date1", SqlDbType.Date) { Value = Date1 },
                     new SqlParameter("@Date2", SqlDbType.Date) { Value = Date2 },
                     new SqlParameter("@WithDateFilter", SqlDbType.Bit) { Value = WithDateFilter },

                  };

                string a = @"   
  
select 
tbl_items.guid as itemGuid,tbl_items.barcode as barcode,
tbl_items.aname as itemAName,
tbl_items.SalesPriceAfterTax as salesPriceAfterTax,
(select isnull( sum(qty),0) from tbl_invoicedetails where iscounted=1 and tbl_invoicedetails.itemguid=tbl_items.guid
 and( (cast( tbl_invoicedetails.invoicedate as date) < cast(@date1 as date) ) and @withdatefilter=1)
  and (branchid=@branchid or @branchid=0)
  and  (storeid=@storeid or @storeid=0)
   and   (CompanyID=@CompanyID or @CompanyID=0     )                                                                             )as balanceBefore,
(select isnull( sum(qty),0) from tbl_invoicedetails where iscounted=1 and tbl_invoicedetails.itemguid=tbl_items.guid
 and (cast( tbl_invoicedetails.invoicedate as date) between cast(@date1 as date)and cast(@date2 as date)  or @withdatefilter=0)
  and (branchid=@branchid or @branchid=0)
  and  (storeid=@storeid or @storeid=0)
   and   (CompanyID=@CompanyID or @CompanyID=0     )                                                                             )as qty,
(select isnull( sum(qty),0) from tbl_invoicedetails where iscounted=1 and tbl_invoicedetails.itemguid=tbl_items.guid
 and (cast( tbl_invoicedetails.invoicedate as date) <= cast(@date2 as date)   or @withdatefilter=0)
  and (branchid=@branchid or @branchid=0)
  and  (storeid=@storeid or @storeid=0)
   and   (CompanyID=@CompanyID or @CompanyID=0     )                                                                            )as balanceAfter
 from tbl_items
 where (tbl_items.guid=@itemguid  or @itemguid ='00000000-0000-0000-0000-000000000000')
 and (tbl_items.categoryid =@categoryid or @categoryid=0)
    and   (CompanyID=@CompanyID or @CompanyID=0     ) 

";
                DataTable dt = clsSQL.ExecuteQueryStatement(a, prm);


                return dt;
            }
            catch (Exception)
            {

                throw;
            }


        }
    }
}
