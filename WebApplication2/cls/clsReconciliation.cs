using System.Data.SqlClient;
using System.Data;
using System;
using DocumentFormat.OpenXml.Presentation;

namespace WebApplication2.cls
{
    public class clsReconciliation
    {
        public DataTable SelectReconciliationByJVDetailsGuid(int VoucherNumber,string JVDetailsGuid, int CompanyID,SqlTransaction trn=null)
        {
            try
            {
                clsSQL clsSQL = new clsSQL();
                SqlParameter[] prm =
                 { 
                    new SqlParameter("@JVDetailsGuid", SqlDbType.UniqueIdentifier) { Value = Simulate.Guid( JVDetailsGuid ) },
                    new SqlParameter("@CompanyID", SqlDbType.Int) { Value = CompanyID },
                    new SqlParameter("@VoucherNumber", SqlDbType.Int) { Value = VoucherNumber },
                };
                DataTable dt = clsSQL.ExecuteQueryStatement(@"select * from tbl_Reconciliation 
where (JVDetailsGuid=@JVDetailsGuid or @JVDetailsGuid='00000000-0000-0000-0000-000000000000' ) 
and (CompanyID=@CompanyID or @CompanyID=0 )
and (VoucherNumber=@VoucherNumber or @VoucherNumber=0 )
                     ", prm,trn);

                return dt;
            }
            catch (Exception)
            {

                throw;
            }


        }
        public DataTable SelectReconciliationMaxNumber(   int CompanyID,SqlTransaction trn)
        {
            try
            {
                clsSQL clsSQL = new clsSQL();
                SqlParameter[] prm =
                 {
                 
                    new SqlParameter("@CompanyID", SqlDbType.Int) { Value = CompanyID },
      
                };
                DataTable dt = clsSQL.ExecuteQueryStatement(@"select max(vouchernumber)   from tbl_Reconciliation 
where (CompanyID=@CompanyID or @CompanyID=0 )

                     ", prm, trn);

                return dt;
            }
            catch (Exception)
            {

                throw;
            }


        }
        public DataTable SelectReconciliationDetails( int AccountID,int SubAccountID,string TransactionGuid, int CompanyID)
        {
            try
            {
                clsSQL clsSQL = new clsSQL();
                SqlParameter[] prm =
                 {
                    new SqlParameter("@AccountID", SqlDbType.Int) { Value = AccountID },
                     new SqlParameter("@SubAccountID", SqlDbType.Int) { Value = SubAccountID },
                     new SqlParameter("@CompanyID", SqlDbType.Int) { Value = CompanyID },
                       new SqlParameter("@TransactionGuid", SqlDbType.UniqueIdentifier) { Value =Simulate.Guid( TransactionGuid) },
                };
               string a=@"select  tbl_JournalVoucherDetails.Guid,
tbl_JournalVoucherTypes.AName,duedate,note,Total 
,(select count(JVDetailsGuid)from tbl_Reconciliation where JVDetailsGuid=tbl_JournalVoucherDetails.Guid ) as IsReconciled
, sum(tbl_Reconciliation.Amount) AS ReconciledAmount
from tbl_JournalVoucherDetails
inner join tbl_JournalVoucherHeader on tbl_JournalVoucherDetails.ParentGuid=tbl_JournalVoucherHeader.Guid
inner join tbl_JournalVoucherTypes on tbl_JournalVoucherTypes.ID=tbl_JournalVoucherHeader.JVTypeID
left join tbl_Reconciliation on tbl_Reconciliation.JVDetailsGuid  = tbl_JournalVoucherDetails.Guid
where( AccountID = @AccountID or @AccountID=0)
and( tbl_JournalVoucherHeader.CompanyID =  @companyID )
and( tbl_JournalVoucherDetails.SubAccountID =  @SubAccountID or @SubAccountID=0 )
and ((select count(JVDetailsGuid)from tbl_Reconciliation where JVDetailsGuid=tbl_JournalVoucherDetails.Guid )=0
or(select count(JVDetailsGuid)from tbl_Reconciliation where JVDetailsGuid=tbl_JournalVoucherDetails.Guid 
and tbl_Reconciliation.TransactionGuid=@TransactionGuid)>0

)
group by tbl_JournalVoucherDetails.Guid,
tbl_JournalVoucherTypes.AName,duedate,note,Total 
                     " ;
                  a = @"select  tbl_JournalVoucherDetails.Guid,
tbl_JournalVoucherTypes.AName,duedate,note,Total 
,tbl_JournalVoucherHeader.guid as HeaderGuid
, sum(t.Amount) as ReconciledAmount
,sum(tbl_Reconciliation.Amount) ReconciledAmountInSameVoucher
from tbl_JournalVoucherDetails
inner join tbl_JournalVoucherHeader on tbl_JournalVoucherDetails.ParentGuid=tbl_JournalVoucherHeader.Guid
inner join tbl_JournalVoucherTypes on tbl_JournalVoucherTypes.ID=tbl_JournalVoucherHeader.JVTypeID
left join tbl_Reconciliation t on t.JVDetailsGuid  = tbl_JournalVoucherDetails.Guid 
left join tbl_Reconciliation on tbl_Reconciliation.Guid  = t.Guid 
and (tbl_Reconciliation.TransactionGuid=@TransactionGuid  )

 
where ( AccountID = @AccountID or @AccountID=0)
and( tbl_JournalVoucherHeader.CompanyID =  @companyID )
and( tbl_JournalVoucherDetails.SubAccountID =  @SubAccountID or @SubAccountID=0 )

group by tbl_JournalVoucherDetails.Guid,
tbl_JournalVoucherTypes.AName,duedate,note,Total ,tbl_JournalVoucherHeader.guid
having (Total-isnull(sum(t.Amount),0))<>0 or isnull(sum(tbl_Reconciliation.Amount),0)<>0 
 order by DueDate";
                DataTable dt = clsSQL.ExecuteQueryStatement(a, prm);
                return dt;
            }
            catch (Exception)
            {

                throw;
            }


        }
        public bool DeleteReconciliationByVoucherNumber(string TransactionGuid, int CompanyID,SqlTransaction trn=null)
        {
            try
            {
                clsSQL clsSQL = new clsSQL();

                SqlParameter[] prm =
                 { new SqlParameter("@TransactionGuid", SqlDbType.UniqueIdentifier) { Value =Simulate.Guid( TransactionGuid) },
                  new SqlParameter("@CompanyID", SqlDbType.Int) { Value = CompanyID },
                };
                int A = clsSQL.ExecuteNonQueryStatement(@"delete from tbl_Reconciliation where (TransactionGuid=@TransactionGuid  ) and (CompanyID =@CompanyID)", prm, trn);

                return true;
            }
            catch (Exception)
            {

                throw;
            }


        }
        public bool DeleteReconciliationByVoucherNumber(int VoucherNumber, int CompanyID, SqlTransaction trn = null)
        {
            try
            {
                clsSQL clsSQL = new clsSQL();

                SqlParameter[] prm =
                 { new SqlParameter("@VoucherNumber", SqlDbType.Int) { Value =VoucherNumber },
                  new SqlParameter("@CompanyID", SqlDbType.Int) { Value = CompanyID },
                };
                int A = clsSQL.ExecuteNonQueryStatement(@"delete from tbl_Reconciliation where (VoucherNumber=@VoucherNumber  ) and (CompanyID =@CompanyID)", prm, trn);

                return true;
            }
            catch (Exception)
            {

                throw;
            }


        }
        public String InsertReconciliation(int VoucherNumber, string JVDetailsGuid, decimal Amount, int CompanyID, int CreationUserId,string TransactionGuid,SqlTransaction trn)
        {
            try
            {
                clsSQL clsSQL = new clsSQL();

                SqlParameter[] prm =
                 {         
                   new SqlParameter("@JVDetailsGuid", SqlDbType.UniqueIdentifier) { Value = Simulate.Guid( JVDetailsGuid ) },
                   new SqlParameter("@Amount", SqlDbType.Decimal) { Value = Amount },
                   new SqlParameter("@VoucherNumber", SqlDbType.Int) { Value = VoucherNumber },
                   new SqlParameter("@CompanyID", SqlDbType.Int) { Value = CompanyID },
                   new SqlParameter("@CreationUserId", SqlDbType.Int) { Value = CreationUserId },
                   new SqlParameter("@CreationDate", SqlDbType.DateTime) { Value = DateTime.Now },
                     new SqlParameter("@TransactionGuid", SqlDbType.UniqueIdentifier) { Value =  Simulate.Guid( TransactionGuid ) },
                };

                string a = @"insert into tbl_Reconciliation
(VoucherNumber,JVDetailsGuid,Amount,CompanyID,CreationUserId,CreationDate,TransactionGuid)
                        OUTPUT INSERTED.Guid
values(@VoucherNumber,@JVDetailsGuid,@Amount,@CompanyID,@CreationUserId,@CreationDate,@TransactionGuid)";

                return Simulate.String(clsSQL.ExecuteScalar(a, prm, trn));

            }
            catch (Exception)
            {

                throw;
            }


        }
        //public int UpdateReconciliation(int ID, string AName, string EName, int ModificationUserId)
        //{
        //    try
        //    {
        //        clsSQL clsSQL = new clsSQL();

        //        SqlParameter[] prm =
        //         {
        //             new SqlParameter("@ID", SqlDbType.Int) { Value = ID },
        //          new SqlParameter("@AName", SqlDbType.NVarChar,-1) { Value = AName },
        //          new SqlParameter("@EName", SqlDbType.NVarChar,-1) { Value = EName },

        //                 new SqlParameter("@ModificationUserId", SqlDbType.Int) { Value = ModificationUserId },
        //             new SqlParameter("@ModificationDate", SqlDbType.DateTime) { Value = DateTime.Now },
        //        };
        //        int A = clsSQL.ExecuteNonQueryStatement(@"update tbl_CostCenter set 
        //               AName=@AName,
        //               EName=@EName,
        //               ModificationDate=@ModificationDate,
        //               ModificationUserId=@ModificationUserId
        //           where id =@id", prm);

        //        return A;
        //    }
        //    catch (Exception)
        //    {

        //        throw;
        //    }


        //}
        public DataTable SelectLoanScheduling(int AccountID, int SubAccountID,  int CompanyID)
        {
            try
            {
                clsSQL clsSQL = new clsSQL();
                SqlParameter[] prm =
                 {
                    new SqlParameter("@AccountID", SqlDbType.Int) { Value = AccountID },
                     new SqlParameter("@SubAccountID", SqlDbType.Int) { Value = SubAccountID },
                     new SqlParameter("@CompanyID", SqlDbType.Int) { Value = CompanyID },
                 };
                string a = @"
select 
tbl_JournalVoucherTypes.AName as voucherTypesAName,
 tbl_JournalVoucherDetails.Guid as jVDetailsGuid,
 tbl_JournalVoucherDetails.ParentGuid as parentGuid,
 tbl_JournalVoucherDetails.Note as note,
 tbl_JournalVoucherDetails.DueDate as dueDate,
 tbl_JournalVoucherDetails.Total  as total,
( select sum(Amount) from tbl_Reconciliation where JVDetailsGuid = tbl_JournalVoucherDetails.Guid) as   paid ,

  isnull((select tbl_LoanTypes.AName from tbl_FinancingHeader inner join tbl_LoanTypes
  on tbl_LoanTypes.ID = tbl_FinancingHeader.LoanType
   where JVGuid =tbl_JournalVoucherHeader.Guid ),isnull( (select tbl_LoanTypes.AName from tbl_FinancingHeader inner join tbl_LoanTypes
  on tbl_LoanTypes.ID = tbl_FinancingHeader.LoanType
  inner join tbl_FinancingDetails on tbl_FinancingDetails.HeaderGuid = tbl_FinancingHeader.Guid
   where tbl_FinancingDetails.JVGuid =tbl_JournalVoucherHeader.Guid ),(
   select AName from tbl_JournalVoucherTypes where ID = tbl_JournalVoucherHeader.JVTypeID
   ))) as loanTypeAName,

   isnull(
   (select tbl_FinancingHeader.VoucherNumber from tbl_FinancingHeader inner join tbl_LoanTypes
  on tbl_LoanTypes.ID = tbl_FinancingHeader.LoanType
   where JVGuid =tbl_JournalVoucherHeader.Guid  or  tbl_FinancingHeader.Guid =tbl_JournalVoucherHeader.RelatedFinancingHeaderGuid) ,
    (select top 1 tbl_FinancingHeader.VoucherNumber from tbl_FinancingHeader inner join tbl_LoanTypes
  on tbl_LoanTypes.ID = tbl_FinancingHeader.LoanType
inner join tbl_FinancingDetails on tbl_FinancingHeader.Guid = tbl_FinancingDetails.HeaderGuid
   where tbl_FinancingHeader.JVGuid =tbl_JournalVoucherHeader.Guid  or 
   tbl_FinancingDetails.JVGuid =tbl_JournalVoucherHeader.Guid  

or  tbl_FinancingHeader.Guid =tbl_JournalVoucherHeader.RelatedFinancingHeaderGuid) )as financingHeaderVoucherNumber,


 isnull(  isnull(
   (select tbl_FinancingHeader.Guid from tbl_FinancingHeader inner join tbl_LoanTypes
  on tbl_LoanTypes.ID = tbl_FinancingHeader.LoanType
   where JVGuid =tbl_JournalVoucherHeader.Guid ) ,
    (select tbl_FinancingHeader.Guid from tbl_FinancingHeader inner join tbl_LoanTypes
  on tbl_LoanTypes.ID = tbl_FinancingHeader.LoanType
   where JVGuid =tbl_JournalVoucherDetails.Guid ) ),
   
   (select  RelatedFinancingHeaderGuid from tbl_JournalVoucherHeader ss where ss.Guid = tbl_JournalVoucherHeader.Guid)
   )as financingHeaderGuid
 , 
 isnull(  isnull(
   (select tbl_FinancingHeader.LoanType from tbl_FinancingHeader inner join tbl_LoanTypes
  on tbl_LoanTypes.ID = tbl_FinancingHeader.LoanType
   where JVGuid =tbl_JournalVoucherHeader.Guid ) ,
    (select tbl_FinancingHeader.LoanType from tbl_FinancingHeader inner join tbl_LoanTypes
  on tbl_LoanTypes.ID = tbl_FinancingHeader.LoanType
   where JVGuid =tbl_JournalVoucherDetails.Guid ) ),
   
   (select  RelatedLoanTypeID from tbl_JournalVoucherHeader ss where ss.Guid = tbl_JournalVoucherHeader.Guid)
   )as RelatedLoanTypeID
from tbl_JournalVoucherDetails
inner join tbl_JournalVoucherHeader 
on tbl_JournalVoucherHeader.Guid= tbl_JournalVoucherDetails.ParentGuid
inner join tbl_JournalVoucherTypes on tbl_JournalVoucherTypes.ID = tbl_JournalVoucherHeader.JVTypeID

 where 
 (tbl_JournalVoucherDetails.AccountID = @accountid or @accountid=0)
 and( tbl_JournalVoucherDetails.SubAccountID =@subaccountid or @subaccountid=0)
 and (tbl_JournalVoucherHeader.CompanyID =@Companyid or @Companyid=0)
and tbl_JournalVoucherHeader.JVTypeID in ( 14,15) 
and tbl_JournalVoucherDetails.debit > 0
           order by     tbl_JournalVoucherDetails.DueDate asc       ";
              
                DataTable dt = clsSQL.ExecuteQueryStatement(a, prm);
                return dt;
            }
            catch (Exception)
            {

                throw;
            }


        }

        public DataTable SelectAllReconciliations(int CompanyID, SqlTransaction trn = null)
        {
            try
            {
                clsSQL clsSQL = new clsSQL();
                SqlParameter[] prm =
                 {
                     new SqlParameter("@CompanyID", SqlDbType.Int) { Value = CompanyID },

                };
                DataTable dt = clsSQL.ExecuteQueryStatement(@"select q.VoucherNumber,
(select top 1 CreationDate from tbl_Reconciliation d where  CompanyID = @companyid and d.VoucherNumber=q.VoucherNumber) as CreationDate
,(select sum(Amount) from tbl_Reconciliation dd where CompanyID = @companyid and  dd.VoucherNumber=q.VoucherNumber and Amount > 0) as Amount
,(select AName from tbl_employee where CompanyID = @companyid and id = (select top 1 CreationUserID from tbl_Reconciliation ddd where CompanyID = @companyid and ddd.VoucherNumber=q.VoucherNumber and Amount > 0)) as CreationUser
 from (
select      VoucherNumber  
from tbl_Reconciliation 
where tbl_Reconciliation.CompanyID = @companyid
 group by  VoucherNumber  
 
 ) as q
                     ", prm, trn);

                return dt;
            }
            catch (Exception)
            {

                throw;
            }


        }


        public DataTable SelectAccountsForReconciliation(int CompanyID)
        {
            try
            {
                clsSQL clsSQL = new clsSQL();
                SqlParameter[] prm =
                 {
                    
                     new SqlParameter("@CompanyID", SqlDbType.Int) { Value = CompanyID },
                 };
                string a = @"
select q.id,q.AccountNumber ,q.AName ,q.SubAccountID,q.SubLedgerAccountName from (select 
isnull((select sum(Amount) from tbl_Reconciliation 
where tbl_Reconciliation.JVDetailsGuid= tbl_JournalVoucherDetails.Guid) ,0) as reconciled
,tbl_JournalVoucherDetails.* ,tbl_Accounts.AName  ,tbl_Accounts.ID ,tbl_Accounts.AccountNumber 
, case when (tbl_Accounts.ID in (select AccountID from tbl_AccountSetting where CompanyID = @CompanyID and AccountRefID in (6,7)))
then 
tbl_BusinessPartner.AName
 when (tbl_Accounts.ID in (select AccountID from tbl_AccountSetting where CompanyID = @CompanyID and AccountRefID in (15)))
then tbl_Banks.AName
 when (tbl_Accounts.ID in (select AccountID from tbl_AccountSetting where CompanyID = @CompanyID and AccountRefID in (5)))
then tbl_CashDrawer.AName
end as SubLedgerAccountName 
 from tbl_JournalVoucherDetails 
left join tbl_Accounts on tbl_Accounts.ID = tbl_JournalVoucherDetails.AccountID 
left join tbl_Banks on tbl_Banks.ID = tbl_JournalVoucherDetails.SubAccountID 
left join tbl_BusinessPartner  on tbl_BusinessPartner.ID = tbl_JournalVoucherDetails.SubAccountID 
left join tbl_CashDrawer  on tbl_CashDrawer.ID = tbl_JournalVoucherDetails.SubAccountID 
 ) as q where q.reconciled <>q.Total and q.CompanyID = @CompanyID
 group by q.id,q.AccountNumber, q.AName,q.SubAccountID,q.SubLedgerAccountName   ";

                DataTable dt = clsSQL.ExecuteQueryStatement(a, prm);
                return dt;
            }
            catch (Exception)
            {

                throw;
            }


        }
        public DataTable SelectAccountsForAutoReconciliation(int AccountID, int SubAccountID, int CompanyID,int RelatedLoanTypeID, SqlTransaction trn)
        {
            try
            {
                clsSQL clsSQL = new clsSQL();
                SqlParameter[] prm =
                 {
                    new SqlParameter("@AccountID", SqlDbType.Int) { Value = AccountID },
                    new SqlParameter("@SubAccountID", SqlDbType.Int) { Value = SubAccountID },
                     new SqlParameter("@CompanyID", SqlDbType.Int) { Value = CompanyID },
                        new SqlParameter("@RelatedLoanTypeID", SqlDbType.Int) { Value = RelatedLoanTypeID },
                     
                 };
                string a = @"
select * from (select 
isnull((select sum(Amount) from tbl_Reconciliation 
where tbl_Reconciliation.JVDetailsGuid= tbl_JournalVoucherDetails.Guid) ,0) as reconciled
,tbl_JournalVoucherDetails.* ,tbl_Accounts.AName  ,tbl_Accounts.ID 
, case when (tbl_Accounts.ID in (select AccountID from tbl_AccountSetting where CompanyID = @CompanyID and AccountRefID in (6,7)))
then 
tbl_BusinessPartner.AName
 when (tbl_Accounts.ID in (select AccountID from tbl_AccountSetting where CompanyID = @CompanyID and AccountRefID in (15)))
then tbl_Banks.AName
 when (tbl_Accounts.ID in (select AccountID from tbl_AccountSetting where CompanyID = @CompanyID and AccountRefID in (5)))
then tbl_CashDrawer.AName
end as SubLedgerAccountName  ,
tbl_JournalVoucherHeader.RelatedLoanTypeID 
 from tbl_JournalVoucherDetails 
 left join tbl_JournalVoucherHeader on tbl_JournalVoucherHeader.Guid = tbl_JournalVoucherDetails.ParentGuid
left join tbl_Accounts on tbl_Accounts.ID = tbl_JournalVoucherDetails.AccountID 
left join tbl_Banks on tbl_Banks.ID = tbl_JournalVoucherDetails.SubAccountID 
left join tbl_BusinessPartner  on tbl_BusinessPartner.ID = tbl_JournalVoucherDetails.SubAccountID 
left join tbl_CashDrawer  on tbl_CashDrawer.ID = tbl_JournalVoucherDetails.SubAccountID 
) as q 
where q.reconciled <> q.Total 
and (q.AccountID = @accountid or @accountid = 0)
 and (q.SubAccountID = @Subaccountid or @Subaccountid = 0) 
  and (q.RelatedLoanTypeID = @RelatedLoanTypeID or @RelatedLoanTypeID = 0) ";

                DataTable dt = clsSQL.ExecuteQueryStatement(a, prm, trn);
                return dt;
            }
            catch (Exception)
            {

                throw;
            }


        }
    }
    public class tbl_Reconciliation
    {
        public string Guid { get; set; }
        public int VoucherNumber { get; set; }
        public string JVDetailsGuid { get; set; }
        public decimal Amount    { get; set; }
        public int CompanyID { get; set; }
        public int CreationUserID { get; set; }
        public DateTime CreationDate { get; set; }



        public int ModulleID { get; set; }
        public string TransactionGuid { get; set; } 


    }
}
