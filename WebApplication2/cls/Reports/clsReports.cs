using FastReport;
using Microsoft.CodeAnalysis.Operations;
using System;
using System.Data;
using System.Data.SqlClient;

namespace WebApplication2.cls.Reports
{
    public class clsReports
    {
        public DataTable SelectTrialBalance(DateTime Date1, DateTime Date2, int BranchID, int CostCenterID, int CompanyID,int level)
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

                string a = "";
                if (level == 4) {

                    a = @"

 
declare @bankAccountID int  = 0;
declare @CashAccountID int  = 0;
declare @ARAccountID int  = 0;
declare @APAccountID int  = 0;
set @bankaccountid = (select AccountID from tbl_AccountSetting where AccountRefID=15 and  Active=1 and CompanyID =@companyid)
set @CashAccountID = (select AccountID from tbl_AccountSetting where AccountRefID= 5 and  Active=1 and CompanyID =@companyid)
set @ARAccountID = (select AccountID from tbl_AccountSetting where AccountRefID= 7 and  Active=1 and CompanyID =@companyid)
set @APAccountID = (select AccountID from tbl_AccountSetting where AccountRefID= 8 and  Active=1 and CompanyID =@companyid)
select * from (select ID,AccountNumber,AName,EName,ChildCount
,
isnull(
(select sum(total) from tbl_JournalVoucherDetails inner join tbl_JournalVoucherHeader 
                             on tbl_JournalVoucherHeader.guid = tbl_JournalVoucherDetails.Parentguid 
                            where (AccountID in (select *from dbo.GetAccountInParent( q.ID))) 
					             and cast( tbl_JournalVoucherHeader.VoucherDate as date )< @date1  
								  and (tbl_JournalVoucherDetails.BranchID= @BranchID or  @BranchID=0)
								  and (tbl_JournalVoucherDetails.SubAccountID= q.SubAccountID or  q.SubAccountID=0)
								    and (tbl_JournalVoucherDetails.CostCenterID= @CostCenterID or  @CostCenterID=0)
								 and (tbl_JournalVoucherDetails.CompanyID= @companyId or  @companyId=0) ) ,0.000)              as OpeningBalance ,

								isnull( (select sum(debit) from tbl_JournalVoucherDetails inner join tbl_JournalVoucherHeader 
                             on tbl_JournalVoucherHeader.guid = tbl_JournalVoucherDetails.Parentguid 
                            where (AccountID in (select *from dbo.GetAccountInParent( q.ID))) 
					             and cast( tbl_JournalVoucherHeader.VoucherDate as date )between @date1 and @date2  
								 and (tbl_JournalVoucherDetails.BranchID= @BranchID or  @BranchID=0)
								 and (tbl_JournalVoucherDetails.SubAccountID= q.SubAccountID or  q.SubAccountID=0)
								    and (tbl_JournalVoucherDetails.CostCenterID= @CostCenterID or  @CostCenterID=0)
								 and (tbl_JournalVoucherDetails.CompanyID= @companyId or  @companyId=0) )   ,0.000)               as Debit ,

								 isnull(		 (select sum(Credit) from tbl_JournalVoucherDetails inner join tbl_JournalVoucherHeader 
                             on tbl_JournalVoucherHeader.guid = tbl_JournalVoucherDetails.Parentguid 
                            where (AccountID in (select *from dbo.GetAccountInParent( q.ID))) 
					             and cast( tbl_JournalVoucherHeader.VoucherDate as date )between @date1 and @date2 
								 and (tbl_JournalVoucherDetails.BranchID= @BranchID or  @BranchID=0)
								 and (tbl_JournalVoucherDetails.SubAccountID= q.SubAccountID or  q.SubAccountID=0)
								    and (tbl_JournalVoucherDetails.CostCenterID= @CostCenterID or  @CostCenterID=0) 
								 and (tbl_JournalVoucherDetails.CompanyID= @companyId or  @companyId=0) )    ,0.000)              as Credit ,

                           isnull( (select sum(total) from tbl_JournalVoucherDetails inner join tbl_JournalVoucherHeader 
                             on tbl_JournalVoucherHeader.guid = tbl_JournalVoucherDetails.Parentguid 
                            where (AccountID in (select *from dbo.GetAccountInParent( q.ID))) 
                            and cast( tbl_JournalVoucherHeader.VoucherDate as date )<= @date2 
							and (tbl_JournalVoucherDetails.BranchID= @BranchID or  @BranchID=0)
							and (tbl_JournalVoucherDetails.SubAccountID= q.SubAccountID or  q.SubAccountID=0)
								    and (tbl_JournalVoucherDetails.CostCenterID= @CostCenterID or  @CostCenterID=0)
					              and (tbl_JournalVoucherDetails.CompanyID= @companyId or  @companyId=0) )        ,0.000)          as EndingBalance 


 from(
select id,AccountNumber,AName,EName ,0 as SubAccountID ,case when (id=@APAccountID    ) then 999 

when (id=@ArAccountID    )  then 
999
when (id=@BankAccountID    )  then 
999
when (id=@CashAccountID    )  then 
999
else 
(select count(ss.id) from tbl_Accounts ss where ParentID = tbl_Accounts.id) 
end
as ChildCount from tbl_Accounts  where   CompanyID =@companyid

union all 
select @CashAccountID,(select accountnumber from tbl_Accounts where id=@CashAccountID)+'-'+FORMAT(id, '000000;-00000') as AccountNumber,AName,EName ,id as SubAccountID , 0 as ChildCount from tbl_CashDrawer  where CompanyID =@companyid 
union all 
select @bankAccountID,(select accountnumber from tbl_Accounts where id=@bankAccountID)+'-'+FORMAT(id, '000000;-00000') as AccountNumber,AName,EName ,id as SubAccountID , 0 as ChildCount from tbl_Banks  where CompanyID =@companyid 
union all 
select @ARAccountID,(select accountnumber from tbl_Accounts where id=@ARAccountID)+'-'+FORMAT(id, '000000;-00000') as AccountNumber,AName,EName ,id as SubAccountID , 0 as ChildCount from tbl_BusinessPartner where CompanyID =@companyid and id in (select subaccountid from tbl_JournalVoucherDetails where AccountID=@ARAccountID)
union all 
select @ApAccountID,(select accountnumber from tbl_Accounts where id=@ApAccountID)+'-'+FORMAT(id, '000000;-00000') as AccountNumber,AName,EName ,id as SubAccountID , 0 as ChildCount from tbl_BusinessPartner where CompanyID =@companyid and id in (select subaccountid from tbl_JournalVoucherDetails where AccountID=@ApAccountID)
) as q  
) as qq where qq.OpeningBalance <>0 or qq.EndingBalance <>0 or qq.Debit<>0 or qq.Credit <>0
order by qq.AccountNumber asc";


                }
                else {

                    a = @"
declare @bankAccountID int  = 0;
declare @CashAccountID int  = 0;
declare @ARAccountID int  = 0;
declare @APAccountID int  = 0;
set @bankaccountid = (select AccountID from tbl_AccountSetting where AccountRefID=15 and  Active=1 and CompanyID =@companyid)
set @CashAccountID = (select AccountID from tbl_AccountSetting where AccountRefID= 5 and  Active=1 and CompanyID =@companyid)
set @ARAccountID = (select AccountID from tbl_AccountSetting where AccountRefID= 7 and  Active=1 and CompanyID =@companyid)
set @APAccountID = (select AccountID from tbl_AccountSetting where AccountRefID= 8 and  Active=1 and CompanyID =@companyid)




select  tbl_Accounts.ID,tbl_Accounts.AccountNumber,tbl_Accounts.AName,tbl_Accounts.EName,

 
(select count(ss.id) from tbl_Accounts ss where ParentID = tbl_Accounts.id) 
 
as ChildCount,


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
     and cast( tbl_JournalVoucherHeader.VoucherDate as date )<= @date2 
							and (tbl_JournalVoucherDetails.BranchID= @BranchID or  @BranchID=0)
								    and (tbl_JournalVoucherDetails.CostCenterID= @CostCenterID or  @CostCenterID=0)
					              and (tbl_JournalVoucherDetails.CompanyID= @companyId or  @companyId=0) )        ,0.000)          as EndingBalance 
from tbl_Accounts 
where (CompanyID= @companyId or  @companyId=0)
order by AccountNumber asc";

}

                clsSQL clsSQL = new clsSQL();

                DataTable dt = clsSQL.ExecuteQueryStatement(a, clsSQL.CreateDataBaseConnectionString(CompanyID), prm);

                return dt;
            }
            catch (Exception)
            {

                throw;
            }


        }

     
       
 
public DataTable SelectCustomerBalanceBeforeTransaction(string FinancingGuid, DateTime Date2, int Accountid, int subAccountid, int CompanyID)
        {
            try
            {
                clsSQL clsSQL = new clsSQL();

                SqlParameter[] prm =
                 {
                      new SqlParameter("@Date2", SqlDbType.Date) { Value = Date2 },
                                           new SqlParameter("@FinancingGuid", SqlDbType.UniqueIdentifier) { Value = Simulate.Guid(FinancingGuid) },

                                          new SqlParameter("@Accountid", SqlDbType.Int) { Value = Accountid },
                                                               new SqlParameter("@subAccountid", SqlDbType.Int) { Value = subAccountid },

 
                     new SqlParameter("@CompanyID", SqlDbType.Int) { Value = CompanyID },


                };

                string a = @" select sum(Total) from tbl_JournalVoucherDetails  
inner join tbl_JournalVoucherHeader on tbl_JournalVoucherHeader.Guid = tbl_JournalVoucherDetails.ParentGuid
where AccountID = @Accountid and SubAccountID = @subAccountid and VoucherDate<=@Date2 and tbl_JournalVoucherHeader.CompanyID = @CompanyID 
and ParentGuid not in
 (select jvguid from tbl_FinancingHeader 
 where tbl_FinancingHeader.Guid=@FinancingGuid
 union 
all select jvguid from tbl_FinancingDetails
where ParentGuid =@FinancingGuid )";
                DataTable dt = clsSQL.ExecuteQueryStatement(a, clsSQL.CreateDataBaseConnectionString(CompanyID), prm);
               
                return dt;
            }
            catch (Exception)
            {

                throw;
            }


        }
        public DataTable SelectAccountStatement(DateTime Date1, DateTime Date2, int BranchID, int CostCenterID, int Accountid, int subAccountid, int CompanyID,bool IsDue,string JVTypeIDList)
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

                                                               new SqlParameter("@IsDue", SqlDbType.Bit) { Value = IsDue },

                     new SqlParameter("@CompanyID", SqlDbType.Int) { Value = CompanyID },


                };

                string a = @"select * from ( select NEWID() as guid
 ,NEWID() as parentguid,0 as rowindex

 ,0 as accountid,
 0 as subaccountid,
 0 as debit,
 0 as credit,
 isnull(sum(total) ,0) AS total ,
 0 as branchid,
 0 as costcenterid,
 @date1 as duedate,
 'Opening balance' as note ,
 0 as companyid
,0 as CreationUserID,
@date1  as creationdate,
0 as  ModificationUserID,
@date1  as  ModificationDate,
'' as branchName,
'' as costCenterName,
0 as JVTypeID,
@date1  as voucherdate,
'OP' as voucherType
 ,0 as JVNumber,
 '' as AccountNumber,
'' as AccountEname
 , 0 as CurrencyID
 , '' as CurrencyAName
 , 1 as CurrencyRate
 , isnull(sum(total) ,0)  as CurrencyBaseAmount
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
and ((cast ( tbl_journalvoucherheader.voucherdate as date) < cast( @date1 as date) and (@IsDue=0) )
or (cast ( tbl_JournalVoucherDetails.duedate as date) < cast( @date1 as date) and (@IsDue=1) ))
union all
select tbl_JournalVoucherDetails.guid,
tbl_JournalVoucherDetails.parentguid,
tbl_JournalVoucherDetails.RowIndex,
tbl_JournalVoucherDetails.AccountID,
tbl_JournalVoucherDetails.SubAccountID,
tbl_JournalVoucherDetails.Debit,
tbl_JournalVoucherDetails.Credit,
tbl_JournalVoucherDetails.Total,
tbl_JournalVoucherDetails.BranchID,
tbl_JournalVoucherDetails.CostCenterID,
tbl_JournalVoucherDetails.DueDate,
tbl_JournalVoucherDetails.Note,
tbl_JournalVoucherDetails.CompanyID,
tbl_JournalVoucherDetails.CreationUserID,
tbl_JournalVoucherDetails.CreationDate,
tbl_JournalVoucherDetails.ModificationUserID,
tbl_JournalVoucherDetails.ModificationDate
,tbl_branch.AName
,tbl_costCenter.AName
,tbl_JournalVoucherHeader.JVTypeID 
,tbl_JournalVoucherHeader.VoucherDate
 ,tbl_journalvouchertypes.aname as voucherType
 ,tbl_JournalVoucherHeader.JVNumber
 , tbl_accounts.ename as AccountEname
 , tbl_accounts.AccountNumber as AccountNumber
 , tbl_JournalVoucherDetails.CurrencyID
 , tbl_Currency.AName as CurrencyAName
 , tbl_JournalVoucherDetails.CurrencyRate
 , tbl_JournalVoucherDetails.CurrencyBaseAmount
  from tbl_JournalVoucherDetails 
inner join tbl_accounts on accountid=tbl_accounts.id
inner join tbl_JournalVoucherHeader on tbl_JournalVoucherHeader.guid =tbl_journalvoucherdetails.parentguid and  tbl_JournalVoucherHeader.companyid =tbl_journalvoucherdetails.companyid
left join tbl_Branch on tbl_branch.id=tbl_JournalVoucherDetails.branchid
left join tbl_costCenter on tbl_costCenter.id=tbl_JournalVoucherDetails.CostCenterID
left join tbl_Currency on tbl_Currency.ID = tbl_JournalVoucherDetails.CurrencyID
left join tbl_journalvouchertypes on tbl_journalvouchertypes.id = jvtypeid
where(tbl_JournalVoucherDetails.companyid=@companyID or @companyid=0)
 and (tbl_JournalVoucherDetails.AccountID=@Accountid or @Accountid=0)
 and (tbl_JournalVoucherDetails.BranchID=@BranchID or @BranchID=0)
 and (tbl_JournalVoucherDetails.CostCenterID=@CostCenterID or @CostCenterID=0)
and (tbl_JournalVoucherDetails.SubAccountID=@Subaccountid or @Subaccountid=0)
and ((cast ( tbl_journalvoucherheader.voucherdate as date) between cast( @date1 as date) and cast(@date2 as date)  and (@IsDue=0))
or (cast ( tbl_JournalVoucherDetails.duedate as date) between cast( @date1 as date) and cast(@date2 as date)  and (@IsDue=1))) 

and tbl_journalvoucherheader.JVTypeID not in (14)

union all
select '00000000-0000-0000-0000-000000000000' as Guid,
tbl_JournalVoucherDetails.parentguid,
'1' as RowIndex,
tbl_JournalVoucherDetails.AccountID,
tbl_JournalVoucherDetails.SubAccountID,
sum(tbl_JournalVoucherDetails.Debit) as Debit,
sum(tbl_JournalVoucherDetails.Credit) as Credit,
sum(tbl_JournalVoucherDetails.Total) as Total,
tbl_JournalVoucherDetails.BranchID,
tbl_JournalVoucherDetails.CostCenterID,
tbl_JournalVoucherHeader.VoucherDate,
tbl_JournalVoucherDetails.Note,
tbl_JournalVoucherDetails.CompanyID,
tbl_JournalVoucherDetails.CreationUserID,
cast(tbl_JournalVoucherDetails.CreationDate as date) as CreationDate,
tbl_JournalVoucherDetails.ModificationUserID,
cast(tbl_JournalVoucherDetails.ModificationDate as date) as ModificationDate,
tbl_branch.AName
,tbl_costCenter.AName
,tbl_JournalVoucherHeader.JVTypeID 
,tbl_JournalVoucherHeader.VoucherDate
 ,tbl_journalvouchertypes.aname as voucherType
 ,tbl_JournalVoucherHeader.JVNumber
 , tbl_accounts.ename as AccountEname
 , tbl_accounts.AccountNumber as AccountNumber

 , tbl_JournalVoucherDetails.CurrencyID
 , tbl_Currency.AName as CurrencyAName
 , tbl_JournalVoucherDetails.CurrencyRate
 , tbl_JournalVoucherDetails.CurrencyBaseAmount


  from tbl_JournalVoucherDetails 
inner join tbl_accounts on accountid=tbl_accounts.id
inner join tbl_JournalVoucherHeader on tbl_JournalVoucherHeader.guid =tbl_journalvoucherdetails.parentguid and  tbl_JournalVoucherHeader.companyid =tbl_journalvoucherdetails.companyid
left join tbl_Branch on tbl_branch.id=tbl_JournalVoucherDetails.branchid
left join tbl_Currency on tbl_Currency.ID = tbl_JournalVoucherDetails.CurrencyID
left join tbl_costCenter on tbl_costCenter.id=tbl_JournalVoucherDetails.CostCenterID
left join tbl_journalvouchertypes on tbl_journalvouchertypes.id = jvtypeid
where
(tbl_JournalVoucherDetails.companyid=@companyID or @companyid=0)
 and (tbl_JournalVoucherDetails.AccountID=@Accountid or @Accountid=0)
 and (tbl_JournalVoucherDetails.BranchID=@BranchID or @BranchID=0)
 and (tbl_JournalVoucherDetails.CostCenterID=@CostCenterID or @CostCenterID=0)
and (tbl_JournalVoucherDetails.SubAccountID=@Subaccountid or @Subaccountid=0)
and ((cast ( tbl_journalvoucherheader.voucherdate as date) between cast( @date1 as date) and cast(@date2 as date)  and (@IsDue=0))
or (cast ( tbl_JournalVoucherDetails.duedate as date) between cast( @date1 as date) and cast(@date2 as date)  and (@IsDue=1))) 
and

 tbl_journalvoucherheader.JVTypeID  in   (14)
group by 

tbl_JournalVoucherDetails.parentguid,
 
tbl_JournalVoucherDetails.AccountID,
tbl_JournalVoucherDetails.SubAccountID,
 
tbl_JournalVoucherDetails.BranchID,
tbl_JournalVoucherDetails.CostCenterID,
 
tbl_JournalVoucherDetails.Note,
tbl_JournalVoucherDetails.CompanyID,
tbl_JournalVoucherDetails.CreationUserID,
cast(tbl_JournalVoucherDetails.CreationDate as date),
tbl_JournalVoucherDetails.ModificationUserID,
cast(tbl_JournalVoucherDetails.ModificationDate as date),
tbl_branch.AName
,tbl_costCenter.AName
,tbl_JournalVoucherHeader.JVTypeID 
 
,tbl_JournalVoucherHeader.VoucherDate
 ,tbl_journalvouchertypes.aname  
 ,tbl_JournalVoucherHeader.JVNumber
 , tbl_accounts.ename  
 , tbl_accounts.AccountNumber 
  , tbl_JournalVoucherDetails.CurrencyID
 , tbl_Currency.AName  
 , tbl_JournalVoucherDetails.CurrencyRate
 , tbl_JournalVoucherDetails.CurrencyBaseAmount




 ) as q   where q.JVTypeID in (" + JVTypeIDList + ") order by q.DueDate ";
                DataTable dt = clsSQL.ExecuteQueryStatement(a, clsSQL.CreateDataBaseConnectionString(CompanyID), prm);
                dt.Columns.Add("netTotal");
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    if (i > 0)
                        dt.Rows[i]["nettotal"] = Simulate.Val(dt.Rows[i]["debit"]) + Simulate.Val(dt.Rows[i - 1]["nettotal"]) - Simulate.Val(dt.Rows[i]["credit"]);
                else
                        dt.Rows[i]["nettotal"] = Simulate.Val(dt.Rows[i]["Total"]) ;

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
                DataTable dt = clsSQL.ExecuteQueryStatement(a, clsSQL.CreateDataBaseConnectionString(CompanyID), prm);


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
                DataTable dt = clsSQL.ExecuteQueryStatement(a, clsSQL.CreateDataBaseConnectionString(CompanyID), prm);


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
                DataTable dt = clsSQL.ExecuteQueryStatement(a, clsSQL.CreateDataBaseConnectionString(CompanyID), prm);


                return dt;
            }
            catch (Exception)
            {

                throw;
            }


        }
        public DataTable SelectCashReport(bool IsPosDate, DateTime Date1, DateTime Date2, int BranchID, int CashID, int InvoiceTypeid, int CompanyID)
        {
            try
            {
                SqlParameter[] prm =
                 { new SqlParameter("@IsPosDate", SqlDbType.Bit) { Value = IsPosDate },
                     new SqlParameter("@date1", SqlDbType.Date) { Value = Date1 },
                     new SqlParameter("@Date2", SqlDbType.Date) { Value = Date2 },
                     new SqlParameter("@BranchID", SqlDbType.Int) { Value = BranchID },
                     new SqlParameter("@CashID", SqlDbType.Int) { Value = CashID },
                                          new SqlParameter("@InvoiceTypeid", SqlDbType.Int) { Value = InvoiceTypeid },

                     new SqlParameter("@CompanyID", SqlDbType.Int) { Value = CompanyID },


                };

                string a = @"select cast(InvoiceDate as date) as InvoiceDate ,
PaymentMethodID,
tbl_PaymentMethod.AName  as PaymentMethod,

BusinessPartnerID,
tbl_BusinessPartner.AName as BusinessPartner,
count(InvoiceNo) as InvoiceCount,
sum(TotalTax* -1*tbl_JournalVoucherTypes.QTYFactor)   as TotalTax,
sum(HeaderDiscount*-1* tbl_JournalVoucherTypes.QTYFactor)   as HeaderDiscount,
sum(TotalDiscount*-1* tbl_JournalVoucherTypes.QTYFactor)   as TotalDiscount,
sum(TotalInvoice*-1* tbl_JournalVoucherTypes.QTYFactor)  as TotalInvoice 
from tbl_InvoiceHeader
 left join tbl_PaymentMethod on tbl_PaymentMethod.ID=PaymentMethodID
  left join tbl_BusinessPartner on tbl_BusinessPartner.ID=BusinessPartnerID
  left join tbl_POSSessions on tbl_POSSessions.Guid = tbl_InvoiceHeader.POSSessionGuid
  left join Tbl_POSDay on Tbl_POSDay.Guid = tbl_POSSessions.POSDayGuid
  left join tbl_JournalVoucherTypes on tbl_JournalVoucherTypes.ID = tbl_InvoiceHeader.InvoiceTypeID
 where (tbl_InvoiceHeader.companyid =@companyID or @companyID=0 )
 and (cast(tbl_InvoiceHeader.InvoiceDate as date)  between cast( @date1 as date) and  cast( @date2 as date) or @IsPosDate=1)
  and (cast(Tbl_POSDay.POSDate as date)  between cast( @date1 as date) and  cast( @date2 as date) or @IsPosDate=0)
 and ( tbl_InvoiceHeader.BranchID =@BranchID or @BranchID=0 )
  and ( tbl_InvoiceHeader.CashID =@CashID or @CashID=0 )  
  and ( tbl_InvoiceHeader.InvoiceTypeid =@InvoiceTypeid or @InvoiceTypeid=0 )
  and (tbl_InvoiceHeader.IsCounted=1)
  group by cast(InvoiceDate as date),PaymentMethodID,BusinessPartnerID,tbl_PaymentMethod.AName ,tbl_BusinessPartner.AName
  order by InvoiceDate,PaymentMethodID"; clsSQL clsSQL = new clsSQL();

                DataTable dt = clsSQL.ExecuteQueryStatement(a, clsSQL.CreateDataBaseConnectionString(CompanyID), prm);

                return dt;
            }
            catch (Exception)
            {

                throw;
            }


        }
        public DataTable SelectAgingReports(DateTime date1, DateTime date2,
             DateTime date3, DateTime date4, DateTime date5, DateTime date6,
             string Accounts, int CompanyID)
        {
            try
            {
                clsSQL clsSQL = new clsSQL();
                SqlParameter[] prm =
                {
                        new SqlParameter("@date1", SqlDbType.DateTime) { Value = date1 },
                        new SqlParameter("@date2", SqlDbType.DateTime) { Value = date2 },
                        new SqlParameter("@date3", SqlDbType.DateTime) { Value = date3 },
                        new SqlParameter("@date4", SqlDbType.DateTime) { Value = date4 },
                        new SqlParameter("@date5", SqlDbType.DateTime) { Value = date5 },
                        new SqlParameter("@date6", SqlDbType.DateTime) { Value = date6 },
                        new SqlParameter("@CompanyID", SqlDbType.Int) { Value =CompanyID },
                        new SqlParameter("@Accounts", SqlDbType.NVarChar,-1) { Value =Accounts },
                   };
                string a = @"
select tbl_BusinessPartner.id,tbl_BusinessPartner.AName, 
(
select SUM(total)  as Credit 
from tbl_JournalVoucherDetails  
where 
cast( DueDate as date) <= @date1
and AccountID in (  SELECT * FROM dbo.SplitInts(@Accounts , ',')) 
and SubAccountID = tbl_BusinessPartner.ID	
and CompanyID = @CompanyID
and tbl_JournalVoucherDetails.Guid not in (select JVDetailsGuid  from tbl_Reconciliation)
) as  date1
,(
select SUM(total)  as Credit 
from tbl_JournalVoucherDetails  
where 
cast( DueDate as date) <= @date2 
and cast( DueDate as date)>@date1 
and AccountID in (  SELECT * FROM dbo.SplitInts(@Accounts , ',')) 
and SubAccountID = tbl_BusinessPartner.ID	
and CompanyID = @CompanyID
and tbl_JournalVoucherDetails.Guid not in (select JVDetailsGuid  from tbl_Reconciliation)
) as  date2
,(
select SUM(total)  as Credit 
from tbl_JournalVoucherDetails  
where 
cast( DueDate as date) <= @date3 
and cast( DueDate as date)>@date2
and AccountID in (  SELECT * FROM dbo.SplitInts(@Accounts , ',')) 
and SubAccountID = tbl_BusinessPartner.ID	
and CompanyID = @CompanyID
and tbl_JournalVoucherDetails.Guid not in (select JVDetailsGuid  from tbl_Reconciliation)
) as  date3
,(
select SUM(total)  as Credit 
from tbl_JournalVoucherDetails  
where 
cast( DueDate as date) <= @date4 
and cast( DueDate as date)>@date3
and AccountID in (  SELECT * FROM dbo.SplitInts(@Accounts , ',')) 
and SubAccountID = tbl_BusinessPartner.ID	
and CompanyID = @CompanyID
and tbl_JournalVoucherDetails.Guid not in (select JVDetailsGuid  from tbl_Reconciliation)
) as  date4
,(
select SUM(total)  as Credit 
from tbl_JournalVoucherDetails  
where 
cast( DueDate as date) <= @date5
and cast( DueDate as date)>@date4
and AccountID in (  SELECT * FROM dbo.SplitInts(@Accounts , ',')) 
and SubAccountID = tbl_BusinessPartner.ID	
and CompanyID = @CompanyID
and tbl_JournalVoucherDetails.Guid not in (select JVDetailsGuid  from tbl_Reconciliation)
) as  date5
,(
select SUM(total)  as Credit 
from tbl_JournalVoucherDetails  
where 
cast( DueDate as date) <= @date6
and cast( DueDate as date)>@date5
and AccountID in (  SELECT * FROM dbo.SplitInts(@Accounts , ',')) 
and SubAccountID = tbl_BusinessPartner.ID	
and CompanyID = @CompanyID
and tbl_JournalVoucherDetails.Guid not in (select JVDetailsGuid  from tbl_Reconciliation)
) as  date6
,(
select SUM(total)  as Credit 
from tbl_JournalVoucherDetails  
where 
cast( DueDate as date) <= @date6

and AccountID in (  SELECT * FROM dbo.SplitInts(@Accounts , ',')) 
and SubAccountID = tbl_BusinessPartner.ID	
and CompanyID = @CompanyID
and tbl_JournalVoucherDetails.Guid not in (select JVDetailsGuid  from tbl_Reconciliation)
) as  BalanceTodate
 from tbl_BusinessPartner 
 where companyid = @companyid
";
                DataTable dt = clsSQL.ExecuteQueryStatement(a, clsSQL.CreateDataBaseConnectionString(CompanyID), prm);

                return dt;
            }
            catch (Exception)
            {

                throw;
            }


        }
        public DataTable SelectBusinessPartnerBalances(DateTime Date, string Accounts,
              int CompanyID,bool withZeroAmount)
        {
            try
            {
                clsSQL clsSQL = new clsSQL();
                SqlParameter[] prm =
                {
                        new SqlParameter("@date", SqlDbType.DateTime) { Value = Date },
                       
                        new SqlParameter("@CompanyID", SqlDbType.Int) { Value =CompanyID },
                        new SqlParameter("@Accounts", SqlDbType.NVarChar,-1) { Value =Accounts },
                             new SqlParameter("@withZeroAmount", SqlDbType.Bit) { Value =withZeroAmount },
                   };
                string a = @"select * from (
select tbl_BusinessPartner.ID ,tbl_BusinessPartner.AName,tbl_Accounts.AName as AccountAName,

 tbl_BusinessPartner.EMPCode EMPCode ,
(select sum(a.Total) from  tbl_JournalVoucherDetails  as a inner join tbl_JournalVoucherHeader on tbl_JournalVoucherHeader.Guid = a.ParentGuid
where (a.CompanyID = @companyID or @companyID =0)
and (AccountID in (select * from dbo.SplitInts(@accounts,','))
and a.SubAccountID = tbl_BusinessPartner.ID
and a.AccountID = tbl_Accounts.ID
and tbl_JournalVoucherHeader.VoucherDate <= @date
)
) as Total ,
--==========================
(select sum(a.Total) from  tbl_JournalVoucherDetails  as a 
where (a.CompanyID = @companyID or @companyID =0)
and (AccountID in (select * from dbo.SplitInts(@accounts,','))
and a.SubAccountID = tbl_BusinessPartner.ID
and a.AccountID = tbl_Accounts.ID
and a.DueDate <= @date
)
) as Due 

--==========================
from tbl_JournalVoucherDetails
inner join tbl_BusinessPartner on tbl_BusinessPartner.ID = tbl_JournalVoucherDetails.SubAccountID
inner join tbl_Accounts on tbl_Accounts.ID = tbl_JournalVoucherDetails.AccountID
where 
(tbl_JournalVoucherDetails.CompanyID = @companyID or @companyID =0)
and (AccountID in (select * from dbo.SplitInts(@accounts,',')))
  
group by tbl_BusinessPartner.ID,tbl_Accounts.ID, tbl_BusinessPartner.EMPCode ,tbl_BusinessPartner.AName,tbl_Accounts.AName 
) as q where (@withZeroAmount=1 or q.Total<>0 ) order by q.AName asc
";
                DataTable dt = clsSQL.ExecuteQueryStatement(a, clsSQL.CreateDataBaseConnectionString(CompanyID), prm);

                return dt;
            }
            catch (Exception)
            {

                throw;
            }


        }
        public DataTable SelectBalanceSheet(DateTime Date, string BranchID, string CostCenterID,
              string CompanyID)
        {
            try
            {
                clsSQL clsSQL = new clsSQL();
                SqlParameter[] prm =
                {
                        new SqlParameter("@date", SqlDbType.DateTime) { Value = Date },
                        new SqlParameter("@CostCenterID", SqlDbType.NVarChar,-1) { Value =CostCenterID },
                        new SqlParameter("@BranchID", SqlDbType.NVarChar,-1) { Value =BranchID },
                        new SqlParameter("@CompanyID", SqlDbType.NVarChar,-1) { Value =CompanyID },
                   };
                string a = @"select 
id,tbl_Accounts.AccountNumber,AName,EName,parentid,
(select count (id) from tbl_Accounts p where p.ParentID = tbl_Accounts.ID)isparent,
(select sum(Total ) from tbl_JournalVoucherDetails inner join tbl_JournalVoucherHeader on tbl_JournalVoucherHeader .Guid = tbl_JournalVoucherDetails.ParentGuid
where AccountID in (select id from dbo.GetChildAccounts(tbl_Accounts.ID)) 
and (tbl_JournalVoucherDetails. CostCenterID in (select * FROM dbo.SplitInts(@costcenterid,','))or 0 in (select * FROM dbo.SplitInts(@costcenterid,',')) )
and (tbl_JournalVoucherDetails. BranchID in (select * FROM dbo.SplitInts(@branchID,','))  or 0 in (select * FROM dbo.SplitInts(@branchID,',')))
and cast( tbl_JournalVoucherHeader.VoucherDate as date) <= @date
) as balance
from tbl_Accounts 
where CompanyID in (select * FROM dbo.SplitInts(@CompanyID,',')) and ReportingTypeID=  2 order by AccountNumber asc
";
                DataTable dt = clsSQL.ExecuteQueryStatement(a, clsSQL.CreateDataBaseConnectionString(Simulate.Integer32( CompanyID)), prm);

                return dt;
            }
            catch (Exception)
            {

                throw;
            }


        }
        public DataTable SelectIncomeStatement(DateTime Date1, DateTime Date2,
            string BranchID, string CostCenterID,
              string CompanyID)
        {
            try
            {
                clsSQL clsSQL = new clsSQL();
                SqlParameter[] prm =
                {
                        new SqlParameter("@date1", SqlDbType.DateTime) { Value = Date1 },
                        new SqlParameter("@date2", SqlDbType.DateTime) { Value = Date2 },
                        new SqlParameter("@CostCenterID", SqlDbType.NVarChar,-1) { Value =CostCenterID },
                        new SqlParameter("@BranchID", SqlDbType.NVarChar,-1) { Value =BranchID },
                        new SqlParameter("@CompanyID", SqlDbType.NVarChar,-1) { Value =CompanyID },
                   };
                string a = @" 
select 
id,tbl_Accounts.AccountNumber,AName,EName,parentid,
(select count (id) from tbl_Accounts p where p.ParentID = tbl_Accounts.ID)isparent,

(select sum(Total ) from tbl_JournalVoucherDetails inner join tbl_JournalVoucherHeader on tbl_JournalVoucherHeader .Guid = tbl_JournalVoucherDetails.ParentGuid
where AccountID in (select id from dbo.GetChildAccounts(tbl_Accounts.ID)) 
and (tbl_JournalVoucherDetails. CostCenterID in (select * FROM dbo.SplitInts(@costcenterid,','))or 0 in (select * FROM dbo.SplitInts(@costcenterid,',')) )
and (tbl_JournalVoucherDetails. BranchID in (select * FROM dbo.SplitInts(@branchID,','))  or 0 in (select * FROM dbo.SplitInts(@branchID,',')))
and cast( tbl_JournalVoucherHeader.VoucherDate as date)  between  @date1 and @date2 
) *-1 as balance

from tbl_Accounts 
where CompanyID in (select * FROM dbo.SplitInts(@CompanyID,',')) and ReportingTypeID=  1 order by AccountNumber asc

 
";
                DataTable dt = clsSQL.ExecuteQueryStatement(a, clsSQL.CreateDataBaseConnectionString(Simulate.Integer32( CompanyID)), prm);

                return dt;
            }
            catch (Exception)
            {

                throw;
            }


        }
    }
}
