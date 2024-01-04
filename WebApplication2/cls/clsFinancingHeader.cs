using System.Collections.Generic;
using System.Data.SqlClient;
using System.Data;
using System;
using WebApplication2.MainClasses;
using static FastReport.Barcode.Iban;

namespace WebApplication2.cls
{
    public class clsFinancingHeader
    {
        
             public DataTable SelectLoanReportRJ(string Date1, string Date2, int UserId, int CompanyID)
        {
            try
            {
                SqlParameter[] prm = {
                    new SqlParameter("@Date1", SqlDbType.Date) { Value = Date1 },
                     new SqlParameter("@Date2", SqlDbType.Date) { Value = Date2 },
                         new SqlParameter("@CompanyID", SqlDbType.Int) { Value = CompanyID },
                };
                string a = @"  select
tbl_BusinessPartner.EmpCode as employee_number,
(select top 1  

 

FORMAT(DueDate, 'dd-MM-yy')

from tbl_JournalVoucherDetails
 where ParentGuid = tbl_FinancingHeader.JVGuid  and debit > 0
 order by DueDate asc
 ) as  effective_start_date,
 'TPT Deductions' as element_name
,'1' as cost_segment1
,'D010' as cost_segment2
,'116003' as cost_segment3
, '0' as cost_segment4
, 'Actual Source Of Deduction' as input_name1
,'Jordan Islamic Bank' as input_value1
,'Source' as input_name2
,'Jordan Islamic Bank/Khrebet Alsouq-Ajwaa Alordon Ass. (Loans)' as input_value2
, 'Source Amount In JOD' as input_name3
, tbl_FinancingHeader.TotalAmount as input_value3
,'Monthly Installment' as input_name4
,(select sum(Debit) from tbl_JournalVoucherDetails
 where ParentGuid = tbl_FinancingHeader.JVGuid  and debit > 0
  and cast( DueDate as date) between  cast( @date1 as date) and cast( @date2 as date) 
 ) as input_value4,
 'Comment' as input_name5
,'loan' as input_value5
, '' as conc
from tbl_FinancingHeader 
left join tbl_BusinessPartner on tbl_BusinessPartner.ID = tbl_FinancingHeader.BusinessPartnerID
where cast( tbl_FinancingHeader.VoucherDate as date) 
between cast( @date1 as date) and cast( @date2 as date) 
and tbl_FinancingHeader.LoanType in (select id from tbl_LoanTypes where tbl_LoanTypes.MainTypeID=2)
and tbl_FinancingHeader.CompanyID =@CompanyID";

                clsSQL cls = new clsSQL();
                DataTable dt = cls.ExecuteQueryStatement(a, prm);

                return dt;
            }
            catch (Exception ex)
            {

                throw;
            }


        }


       
        public DataTable SelectSalesReportRJ(string Date1, string Date2, int UserId, int CompanyID,int ARAccountID)
        {
            try
            {
                SqlParameter[] prm = {
                    new SqlParameter("@Date1", SqlDbType.Date) { Value = Date1 },
                     new SqlParameter("@Date2", SqlDbType.Date) { Value = Date2 },
                         new SqlParameter("@CompanyID", SqlDbType.Int) { Value = CompanyID },
                           new SqlParameter("@AccountID", SqlDbType.Int) { Value = ARAccountID },
                };
                string a = @"  select
tbl_BusinessPartner.EmpCode as employee_number,
FORMAT(@date1, 'dd-MM-yy')  as  effective_start_date,
 

 'TPT Deductions' as element_name
,'1' as cost_segment1
,'D010' as cost_segment2
,'116003' as cost_segment3
, '0' as cost_segment4
, 'Actual Source Of Deduction' as input_name1
,'Jordan Islamic Bank' as input_value1
,'Source' as input_name2
,'Jordan Islamic Bank/Khrebet Alsouq-Ajwaa Alordon Ass. (Sales)' as input_value2
, 'Source Amount In JOD' as input_name3
, tbl_FinancingDetails.TotalAmount as input_value3
,'Monthly Installment' as input_name4
,(select sum(Debit) from tbl_JournalVoucherDetails
 where ParentGuid = tbl_FinancingHeader.JVGuid  and debit > 0
  and cast( DueDate as date) between  cast( @date1 as date) and cast( @date2 as date) 
 ) as input_value4,
 'Comment' as input_name5
,'Mobile' as input_value5
, '' as conc
from tbl_FinancingHeader 
left join tbl_FinancingDetails on tbl_FinancingDetails.HeaderGuid = tbl_FinancingHeader.Guid
left join tbl_BusinessPartner on tbl_BusinessPartner.ID = tbl_FinancingHeader.BusinessPartnerID
where cast( tbl_FinancingHeader.VoucherDate as date) 
between cast( @date1 as date) and cast( @date2 as date) 
and tbl_FinancingHeader.LoanType in (1)
and tbl_FinancingHeader.CompanyID =@CompanyID";
                a = @"select * from (
select tbl_BusinessPartner.AName,
tbl_BusinessPartner.EmpCode as employee_number,
FORMAT(@date1, 'dd-MM-yy') as  effective_start_date,
  'TPT Deductions' as element_name
,'1' as cost_segment1
,'D010' as cost_segment2
,'116003' as cost_segment3
, '0' as cost_segment4
, 'Actual Source Of Deduction' as input_name1
,'Jordan Islamic Bank' as input_value1
,'Source' as input_name2
,'Jordan Islamic Bank/Khrebet Alsouq-Ajwaa Alordon Ass. (Sales)' as input_value2
, 'Source Amount In JOD' as input_name3
--,sum( tbl_FinancingDetails.TotalAmount) as input_value3
,(select sum(Total) from tbl_JournalVoucherDetails 
where AccountID = @AccountID and SubAccountID =tbl_BusinessPartner.ID
and ParentGuid in (select tbl_FinancingDetails.JVGuid from tbl_FinancingDetails inner join 
tbl_FinancingHeader on tbl_FinancingHeader.Guid = tbl_FinancingDetails.HeaderGuid
and LoanType = 1)
) as input_value3
,'Monthly Installment' as input_name4,

(select sum(unreconciled) 
from  dbo.GetDueUnReconciledAmount(@date1,@date2,@companyid,tbl_BusinessPartner.ID)) as input_value4,
 'Comment' as input_name5
,'Mobile' as input_value5
, '' as conc
from  
  tbl_BusinessPartner  
where tbl_BusinessPartner.CompanyID =@CompanyID) as q where q.input_value3>0
 
 ";
                clsSQL cls = new clsSQL();
                DataTable dt = cls.ExecuteQueryStatement(a, prm);

                return dt;
            }
            catch (Exception ex)
            {

                throw;
            }


        }
        public DataTable SelectSubscriptionsReportRJ(string Date1, string Date2, int UserId, int CompanyID
            ,int SubscriptionsTypesID,int SubscriptionsStatusID
            )
        {
            try
            {
                SqlParameter[] prm = {
                    new SqlParameter("@Date1", SqlDbType.Date) { Value = Date1 },
                     new SqlParameter("@Date2", SqlDbType.Date) { Value = Date2 },
                         new SqlParameter("@CompanyID", SqlDbType.Int) { Value = CompanyID },
                             new SqlParameter("@SubscriptionsTypesID", SqlDbType.Int) { Value = SubscriptionsTypesID },
                                 new SqlParameter("@SubscriptionsStatusID", SqlDbType.Int) { Value = SubscriptionsStatusID },
                };
                string a = @"select
tbl_BusinessPartner.EmpCode as employee_number,
FORMAT(tbl_Subscriptions.TransactionDate, 'dd-MM-yy')
  as  effective_start_date,
 'TPT Deductions' as element_name
,'1' as cost_segment1
,'D010' as cost_segment2
,'116003' as cost_segment3
, '0' as cost_segment4
, 'Actual Source Of Deduction' as input_name1
,'Jordan Islamic Bank' as input_value1
,'Source' as input_name2
,'Jordan Islamic Bank/Khrebet Alsouq-Ajwaa Alordon Ass. (Monthly Subscribtion)' as input_value2
, 'Source Amount In JOD' as input_name3
, tbl_Subscriptions.Amount as input_value3
,'Monthly Installment' as input_name4
,'' as input_value4,
 '' as input_name5
,'' as input_value5
 
from tbl_Subscriptions 
left join tbl_BusinessPartner on tbl_BusinessPartner.ID = tbl_Subscriptions.BusinessPartnerID
left join tbl_SubscriptionsTypes on tbl_SubscriptionsTypes.ID = tbl_Subscriptions.SubscriptionTypeID
left join tbl_SubscriptionsStatus on tbl_SubscriptionsStatus.ID = tbl_Subscriptions.TransactionStatusID
where cast( tbl_Subscriptions.TransactionDate as date) 
between cast( @date1 as date) and cast( @date2 as date) 
--and tbl_FinancingHeader.LoanType in (select id from tbl_LoanTypes where tbl_LoanTypes.MainTypeID=2)
and tbl_Subscriptions.CompanyID =@CompanyID 
and (tbl_SubscriptionsTypes.ID = @SubscriptionsTypesID or @SubscriptionsTypesID=0)
and (tbl_SubscriptionsStatus.ID = @SubscriptionsStatusID or @SubscriptionsStatusID=0)
";

                clsSQL cls = new clsSQL();
                DataTable dt = cls.ExecuteQueryStatement(a, prm);

                return dt;
            }
            catch (Exception ex)
            {

                throw;
            }


        }
        public DataTable SelectFinancingHeaderByGuid(string guid, DateTime date1, DateTime date2,   int BranchID,int CreationUserID, int CompanyID,int CurrentUserId,string LoanMainType, SqlTransaction trn = null)
        {
            try
            {
                clsSQL clsSQL = new clsSQL();

                SqlParameter[] prm =
                 {
                    new SqlParameter("@Guid", SqlDbType.UniqueIdentifier) { Value = Simulate.Guid( guid) },
 new SqlParameter("@date1", SqlDbType.DateTime) { Value =Simulate.StringToDate(  date1 )},
  new SqlParameter("@date2", SqlDbType.DateTime) { Value = Simulate.StringToDate(  date2 )},
   
        new SqlParameter("@CompanyID", SqlDbType.Int) { Value = CompanyID },
          new SqlParameter("@BranchID", SqlDbType.Int) { Value = BranchID },
                    new SqlParameter("@CreationUserID", SqlDbType.Int) { Value = CreationUserID },
                                       new SqlParameter("@CurrentUserId", SqlDbType.Int) { Value = CurrentUserId },

                    

                };
                DataTable dt = clsSQL.ExecuteQueryStatement(@"select tbl_FinancingHeader.*,
tbl_Branch.AName as BranchName,
BusinessPartner.AName as BusinessPartnerName,
BusinessPartner.EmpCode as BusinessPartnerEmpCode,
Grantor.AName as GrantorName,
tbl_employee.AName as CreationUserName ,
tbl_LoanTypes.AName AS LoanTypeanAName,
PaymentAccount.aname  as PaymentAccountIDAName,
PaymentSubAccount.aname  as PaymentSubAccountIDAName,
( select top 1 s.FirstInstallmentDate from tbl_FinancingDetails as s 
   where  s.HeaderGuid = tbl_FinancingHeader.Guid)
 as FirstInstallmentDate,
 ( select sum( s.InstallmentAmount) from tbl_FinancingDetails as s 
 where  s.HeaderGuid = tbl_FinancingHeader.Guid)
 as TotalInstallmentAmount,
 ( select sum( s.TotalAmountWithInterest) from tbl_FinancingDetails as s 
   where  s.HeaderGuid = tbl_FinancingHeader.Guid)
 as TotalAmountWithInterest ,
STUFF((SELECT ', ' + ss.Description 
FROM tbl_FinancingDetails ss
WHERE ss.headerguid=tbl_FinancingHeader.Guid
FOR XML PATH('')), 1, 1, '') as DetailsDescription
, tbl_LoanTypes.AName as  LoanTypeAName
,(select sum(Total) from tbl_JournalVoucherDetails
 where ParentGuid =  tbl_FinancingHeader.JVGuid 
 and Total>0
 and tbl_JournalVoucherDetails.Guid  in
 (select tbl_Reconciliation.JVDetailsGuid from tbl_Reconciliation)) as Paid
,tbl_FinancingHeader.JVGuid
from tbl_FinancingHeader
 left join tbl_Branch on tbl_Branch.ID = tbl_FinancingHeader.BranchID
  left join tbl_BusinessPartner as  BusinessPartner on BusinessPartner.ID = tbl_FinancingHeader.BusinessPartnerID
    left join tbl_BusinessPartner as Grantor on Grantor.ID = tbl_FinancingHeader.Grantor
	  left join tbl_employee  on tbl_employee.ID = tbl_FinancingHeader.CreationUserID 
   left join tbl_LoanTypes  on tbl_LoanTypes.ID = tbl_FinancingHeader.LoanType 
    left join tbl_Accounts  as PaymentAccount on PaymentAccount.ID = tbl_FinancingHeader.PaymentAccountID 
	   left join tbl_BusinessPartner as PaymentSubAccount on PaymentSubAccount.ID = tbl_FinancingHeader.PaymentSubAccountID   
where 
(tbl_FinancingHeader.Guid=@Guid or @Guid='00000000-0000-0000-0000-000000000000' )  
and (tbl_FinancingHeader.CompanyID=@CompanyID or @CompanyID=0 )
and (tbl_FinancingHeader.BranchID=@BranchID or @BranchID=0 )
and (tbl_FinancingHeader.CreationUserID=@CreationUserID or @CreationUserID=0 )
and (tbl_LoanTypes.MainTypeID in(" + LoanMainType + @") or -1 in (" + LoanMainType + @"))

and cast( tbl_FinancingHeader.VoucherDate as date) between  cast(@date1 as date) and  cast(@date2 as date) 
and (branchid in (select ModelID from 
    tbl_UserAuthorizationModels where TypeID =1 and UserID = @CurrentUserId and ModelID = BranchID and IsAccess =1) or @CurrentUserId=0 )
                     ", prm, trn);

                return dt;
            }
            catch (Exception ex)
            {

                throw;
            }


        }
        public DataTable SelectMaxFinancingHeader( int BranchID, int CompanyID, SqlTransaction trn = null)
        {
            try
            {
                clsSQL clsSQL = new clsSQL();

                SqlParameter[] prm =
                 {
                 

        new SqlParameter("@CompanyID", SqlDbType.Int) { Value = CompanyID },
          new SqlParameter("@BranchID", SqlDbType.Int) { Value = BranchID },
                };
                DataTable dt = clsSQL.ExecuteQueryStatement(@" select isnull(max(vouchernumber )+1,1) Max from tbl_FinancingHeader where 
 
  (CompanyID=@CompanyID or @CompanyID=0 )
and (BranchID=@BranchID or @BranchID=0 )

 
                     ", prm, trn);

                return dt;
            }
            catch (Exception ex)
            {

                throw;
            }


        }
        public bool DeleteFinancingHeaderByGuid(string guid, SqlTransaction trn)
        {
            try
            {
                clsSQL clsSQL = new clsSQL();

                SqlParameter[] prm =
                 { new SqlParameter("@guid", SqlDbType.UniqueIdentifier) { Value = Simulate.Guid( guid) },

                };
                int A = clsSQL.ExecuteNonQueryStatement(@"delete from tbl_FinancingHeader where (guid=@guid  )", prm, trn);

                return true;
            }
            catch (Exception ex)
            {

                return false;
            }


        }
        public string InsertFinancingHeader(DBFinancingHeader DBFinancingHeader, SqlTransaction trn)
        {
            try
            {
                SqlParameter[] prm =
                   {
                    new SqlParameter("@VoucherDate", SqlDbType.DateTime) { Value = DBFinancingHeader.VoucherDate },
                    new SqlParameter("@BranchID", SqlDbType.Int) { Value = DBFinancingHeader.BranchID },
                    new SqlParameter("@VoucherNumber", SqlDbType.Int) { Value = DBFinancingHeader.VoucherNumber },
                    new SqlParameter("@BusinessPartnerID", SqlDbType.Int) { Value = DBFinancingHeader.BusinessPartnerID },
                    new SqlParameter("@Note", SqlDbType.NVarChar,-1) { Value = DBFinancingHeader.Note },
                    new SqlParameter("@TotalAmount", SqlDbType.Decimal) { Value = DBFinancingHeader.TotalAmount },
                    new SqlParameter("@DownPayment", SqlDbType.Decimal) { Value = DBFinancingHeader.DownPayment },
                    new SqlParameter("@NetAmount", SqlDbType.Decimal) { Value = DBFinancingHeader.NetAmount },
                    new SqlParameter("@Grantor", SqlDbType.Int) { Value = DBFinancingHeader.Grantor },
                    new SqlParameter("@LoanType", SqlDbType.Int) { Value = DBFinancingHeader.LoanType },

                    new SqlParameter("@CompanyID", SqlDbType.Int) { Value = DBFinancingHeader.CompanyID },
                    new SqlParameter("@CreationUserId", SqlDbType.Int) { Value = DBFinancingHeader.CreationUserID },
                    new SqlParameter("@CreationDate", SqlDbType.DateTime) { Value = DateTime.Now },
                     new SqlParameter("@IntrestRate", SqlDbType.Decimal) { Value = DBFinancingHeader.IntrestRate },
                      new SqlParameter("@isAmountReturned", SqlDbType.Bit) { Value = DBFinancingHeader.isAmountReturned },
                       new SqlParameter("@MonthsCount", SqlDbType.Int) { Value = DBFinancingHeader.MonthsCount },
                         new SqlParameter("@PaymentAccountID", SqlDbType.Int) { Value = DBFinancingHeader.PaymentAccountID },
                           new SqlParameter("@PaymentSubAccountID", SqlDbType.Int) { Value = DBFinancingHeader.PaymentSubAccountID },
                                 new SqlParameter("@VendorID", SqlDbType.Int) { Value = DBFinancingHeader.VendorID },

                };

                string a = @"insert into tbl_FinancingHeader (VoucherDate,BranchID,VoucherNumber,BusinessPartnerID,TotalAmount,DownPayment,NetAmount,
                                                                Note,Grantor, LoanType,
                                                               IntrestRate,isAmountReturned,MonthsCount,PaymentAccountID,PaymentSubAccountID,
                                                               CompanyID,CreationUserID,CreationDate,VendorID)  
OUTPUT INSERTED.Guid  
values (@VoucherDate,@BranchID,@VoucherNumber,@BusinessPartnerID,@TotalAmount,@DownPayment,@NetAmount,
                                                               @Note,@Grantor ,@LoanType,
                                                               @IntrestRate,@isAmountReturned,@MonthsCount,@PaymentAccountID,@PaymentSubAccountID,
                                                               @CompanyID,@CreationUserID,@CreationDate,@VendorID)  ";
                clsSQL clsSQL = new clsSQL();
                string myGuid = Simulate.String(clsSQL.ExecuteScalar(a, prm, trn));
                return myGuid;

            }
            catch (Exception ex)
            {

                return "";
            }
        }

        public string UpdateFinancingHeader(DBFinancingHeader DBFinancingHeader, SqlTransaction trn)
        {
            try
            {
                clsSQL clsSQL = new clsSQL();

                SqlParameter[] prm =
                   {
                    new SqlParameter("@Guid", SqlDbType.UniqueIdentifier) { Value = DBFinancingHeader.Guid },
                    new SqlParameter("@VoucherDate", SqlDbType.DateTime) { Value = DBFinancingHeader.VoucherDate },
                    new SqlParameter("@BranchID", SqlDbType.Int) { Value = DBFinancingHeader.BranchID },
                    new SqlParameter("@VoucherNumber", SqlDbType.Int) { Value = DBFinancingHeader.VoucherNumber },
                    new SqlParameter("@BusinessPartnerID", SqlDbType.Int) { Value = DBFinancingHeader.BusinessPartnerID },
                    new SqlParameter("@Note", SqlDbType.NVarChar,-1) { Value = DBFinancingHeader.Note },
                    new SqlParameter("@TotalAmount", SqlDbType.Decimal) { Value = DBFinancingHeader.TotalAmount },
                    new SqlParameter("@DownPayment", SqlDbType.Decimal) { Value = DBFinancingHeader.DownPayment },
                    new SqlParameter("@NetAmount", SqlDbType.Decimal) { Value = DBFinancingHeader.NetAmount },
                    new SqlParameter("@Grantor", SqlDbType.Int) { Value = DBFinancingHeader.Grantor },
                        new SqlParameter("@LoanType", SqlDbType.Int) { Value = DBFinancingHeader.LoanType },
                    new SqlParameter("@ModificationUserID", SqlDbType.Int) { Value = DBFinancingHeader.ModificationUserID },
                    new SqlParameter("@ModificationDate", SqlDbType.DateTime) { Value = DateTime.Now },
                new SqlParameter("@IntrestRate", SqlDbType.Decimal) { Value = DBFinancingHeader.IntrestRate },
                      new SqlParameter("@isAmountReturned", SqlDbType.Bit) { Value = DBFinancingHeader.isAmountReturned },
                       new SqlParameter("@MonthsCount", SqlDbType.Int) { Value = DBFinancingHeader.MonthsCount }, 
                    new SqlParameter("@PaymentAccountID", SqlDbType.Int) { Value = DBFinancingHeader.PaymentAccountID },
                           new SqlParameter("@PaymentSubAccountID", SqlDbType.Int) { Value = DBFinancingHeader.PaymentSubAccountID },
                    new SqlParameter("@VendorID", SqlDbType.Int) { Value = DBFinancingHeader.VendorID },

                };
                string a = @"update tbl_FinancingHeader set  

 VoucherDate=@VoucherDate,
BranchID=@BranchID,
VoucherNumber=@VoucherNumber,
TotalAmount=@TotalAmount,
DownPayment=@DownPayment,
NetAmount=@NetAmount,
BusinessPartnerID=@BusinessPartnerID,
Note=@Note,
 Grantor=@Grantor, 
LoanType=@LoanType,
ModificationUserID=@ModificationUserID,
ModificationDate=@ModificationDate   ,
IntrestRate=@IntrestRate,
isAmountReturned=@isAmountReturned,
MonthsCount=@MonthsCount,
PaymentAccountID=@PaymentAccountID,
PaymentSubAccountID=@PaymentSubAccountID,
VendorID=@VendorID
 where Guid=@guid";

                string A = Simulate.String(clsSQL.ExecuteNonQueryStatement(a, prm, trn));
                return A;


            }
            catch (Exception ex)
            {

                return "";
            }
        }

        public string UpdateFinancingHeaderJVGuid(string Guid,string JVGuid, SqlTransaction trn)
        {
            try
            {
                clsSQL clsSQL = new clsSQL();

                SqlParameter[] prm =
                   {
                     new SqlParameter("@Guid", SqlDbType.UniqueIdentifier) { Value = new Guid( Guid)},
                    new SqlParameter("@JVGuid", SqlDbType.UniqueIdentifier) { Value =new Guid( JVGuid)},
                };
                string a = @"update tbl_FinancingHeader set  

 JVGuid=@JVGuid  
 where Guid=@guid";

                string A = Simulate.String(clsSQL.ExecuteNonQueryStatement(a, prm, trn));
                return A;


            }
            catch (Exception ex)
            {

                return "";
            }
        }
        public DataTable SelectFinancingReport(  DateTime date1, DateTime date2,string users, int BranchID, int CompanyID, SqlTransaction trn = null)
        {
            try
            {
                string UsersFilter = "";
                if (users != "") {
                    UsersFilter = " and tbl_FinancingHeader.creationuserid in ( " + users + ")";
                }
                clsSQL clsSQL = new clsSQL();

                SqlParameter[] prm =
                 {
                    
 new SqlParameter("@date1", SqlDbType.DateTime) { Value =Simulate.StringToDate(  date1 )},
  new SqlParameter("@date2", SqlDbType.DateTime) { Value = Simulate.StringToDate(  date2 )},

        new SqlParameter("@CompanyID", SqlDbType.Int) { Value = CompanyID },
          new SqlParameter("@BranchID", SqlDbType.Int) { Value = BranchID },
                };
                DataTable dt = clsSQL.ExecuteQueryStatement(@"select tbl_FinancingDetails.* ,tbl_Branch.AName as BranchAName
,tbl_BusinessPartner.AName as BusinessPartnerAName 
,tbl_BusinessPartner.EmpCode as BusinessEmpCode 
,tbl_employee.AName as EmployeeAName
from tbl_FinancingDetails 
inner join tbl_FinancingHeader on tbl_FinancingHeader.Guid = tbl_FinancingDetails.HeaderGuid
inner join tbl_Branch on tbl_FinancingHeader.BranchID = tbl_Branch.ID
left join tbl_BusinessPartner on tbl_FinancingHeader.BusinessPartnerID = tbl_BusinessPartner.ID
left join tbl_employee on tbl_FinancingHeader.CreationUserID = tbl_employee.ID
where (BranchID = @branchID or @branchid=0)and
(tbl_FinancingHeader.CompanyID = @CompanyId or @CompanyId=0) " + UsersFilter + @" and
cast( tbl_FinancingHeader.VoucherDate as date) between cast (@date1 as date) and cast (@date2 as date)   ", prm, trn);

                return dt;
            }
            catch (Exception ex)
            {

                throw;
            }


        }
    }
    public class DBFinancingHeader
    {


        public Guid? Guid { get; set; }
        public DateTime VoucherDate { get; set; }
        public int BranchID { get; set; }
        public int VoucherNumber { get; set; }
        public int BusinessPartnerID { get; set; }
        public string Note { get; set; }
        public decimal TotalAmount { get; set; }
        public decimal DownPayment { get; set; }
        public decimal NetAmount { get; set; }
        public int Grantor { get; set; }
        public int LoanType { get; set; }
        
        public int CreationUserID { get; set; }
        public DateTime CreationDate { get; set; }
        public int? ModificationUserID { get; set; }
        public DateTime? ModificationDate { get; set; }
        public int CompanyID { get; set; }
        public Guid? JVGuid { get; set; }
        public decimal IntrestRate { get; set; }
        public bool isAmountReturned { get; set; }
        public int MonthsCount { get; set; }
        public int PaymentAccountID { get; set; }
        public int PaymentSubAccountID { get; set; }
        public int VendorID { get; set; }
    }
}
 
