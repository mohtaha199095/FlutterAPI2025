using System.Collections.Generic;
using Microsoft.Data.SqlClient;
using System.Data;
using System;
using WebApplication2.MainClasses;
 
 
using static WebApplication2.MainClasses.clsEnum;
using DocumentFormat.OpenXml.Wordprocessing;
using FastReport.Barcode;
using Microsoft.CodeAnalysis.Operations;
using Microsoft.Identity.Client;
//using DocumentFormat.OpenXml.Spreadsheet;
using WebApplication2.Controllers;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using WebApplication2.DataSet;
using System.Threading.Tasks;

namespace WebApplication2.cls
{
    public class clsFinancingHeader
    {
        public DataTable SelectEmployeesLoans(DateTime Date1, DateTime Date2, int accountid, int BusinessPartnerID, int CompanyID,bool HideZeroBalances)
        {
            try
            {
                SqlParameter[] prm = {
                    new SqlParameter("@Date1", SqlDbType.Date) { Value = Date1 },
                     new SqlParameter("@Date2", SqlDbType.Date) { Value = Date2 },
                         new SqlParameter("@BusinessPartnerID", SqlDbType.Int) { Value = BusinessPartnerID },
                  new SqlParameter("@GLAccount", SqlDbType.Int) { Value = accountid },
                           new SqlParameter("@CompanyID", SqlDbType.Int) { Value = CompanyID },
                             new SqlParameter("@HideZeroBalances", SqlDbType.Bit) { Value = HideZeroBalances },
                           

                };
                string a = @"   with dt as (
 
select 
tbl_FinancingDetails.JVGuid as JVGuid,
tbl_FinancingHeader.LoanType as LoanType ,
tbl_FinancingHeader.Guid as HeaderGuid ,
tbl_FinancingHeader.VoucherNumber ,
tbl_BusinessPartner.ID as BusinessPartnerID ,
tbl_BusinessPartner.AName as BusinessPartnerAName,
tbl_BusinessPartner.EmpCode,
tbl_LoanTypes.Code,
tbl_FinancingHeader.VoucherDate,
 tbl_FinancingDetails.Description,
tbl_FinancingDetails.TotalAmountWithInterest as TotalAmount,
tbl_FinancingDetails.InstallmentAmount,


 
 (

 select sum(tbl_Reconciliation.Amount) from tbl_Reconciliation 
 left join tbl_JournalVoucherDetails on tbl_JournalVoucherDetails.Guid = tbl_Reconciliation.JVDetailsGuid
 where tbl_JournalVoucherDetails.ParentGuid =tbl_FinancingDetails.jvGuid
 
 
and  accountid = @GLAccount
) as Paid ,
tbl_FinancingDetails.PeriodInMonths,
Format(tbl_FinancingDetails.FirstInstallmentDate,'yyyy-MM') as FirstInstallmentDate,
Format(DATEADD( MONTH,tbl_FinancingDetails.PeriodInMonths-1,tbl_FinancingDetails.FirstInstallmentDate),'yyyy-MM') as LastInstallmentDate

, 
(select  sum(hjvd.Total) -
isnull((select sum(tbl_Reconciliation.Amount) from tbl_Reconciliation
left join tbl_JournalVoucherDetails sdd on sdd.Guid = tbl_Reconciliation.JVDetailsGuid

  where  sdd.ParentGuid = hjvd.ParentGuid
and  accountid =  @GLAccount  
    and DueDate   <= (SELECT DATEADD(month, ((YEAR(GETDATE() ) - 1900) * 12) + MONTH(GETDATE() ), -1))
  ),0) as tt
 from tbl_JournalVoucherDetails as hjvd
   
 where hjvd.accountid = @GLAccount and
  hjvd.ParentGuid	=  tbl_FinancingDetails.JVGuid
   and DueDate   <= (SELECT DATEADD(month, ((YEAR(GETDATE() ) - 1900) * 12) + MONTH(GETDATE() ), -1))
  group by hjvd.ParentGuid
) 
 as DueAmount
,
 (
  select sum( re.Amount)  
  from tbl_FinancingDetails  FD 
  left join tbl_JournalVoucherDetails Jd on Jd.ParentGuid =  FD.JVGuid
  left join tbl_Reconciliation re on Jd.Guid = re.JVDetailsGuid
   where FD.HeaderGuid =tbl_FinancingHeader.Guid
   and   (
  select top 1 SH.JVTypeID from tbl_journalvoucherdetails  SD
  left join tbl_JournalVoucherHeader SH on SH.Guid = SD.ParentGuid
   where SH.JVTypeID=15  and SD.guid in 
  (select SD.JVDetailsGuid from tbl_Reconciliation SD where SD.VoucherNumber = re.VoucherNumber)
  
  
  ) >0
) as  scheduled
 from tbl_FinancingHeader 
left join tbl_LoanTypes on tbl_LoanTypes.id =tbl_FinancingHeader.LoanType
left join tbl_FinancingDetails on tbl_FinancingDetails.HeaderGuid = tbl_FinancingHeader.Guid
left join tbl_BusinessPartner on tbl_FinancingHeader.BusinessPartnerID = tbl_BusinessPartner.ID
where LoanType	 = 1 
and ( tbl_FinancingHeader.BusinessPartnerID =@BusinessPartnerID or @BusinessPartnerID = 0)
and (tbl_FinancingHeader.VoucherDate between @Date1 and @Date2)
and (tbl_FinancingHeader.CompanyID = @CompanyID or @CompanyID = 0)
union all 
select 
 tbl_FinancingHeader.JVGuid as JVGuid,
tbl_FinancingHeader.LoanType as LoanType ,
tbl_FinancingHeader.Guid as HeaderGuid ,

tbl_FinancingHeader.VoucherNumber ,
tbl_BusinessPartner.ID as BusinessPartnerID ,
tbl_BusinessPartner.AName as BusinessPartnerAName,
tbl_BusinessPartner.EmpCode,
tbl_LoanTypes.Code,
tbl_FinancingHeader.VoucherDate,
 tbl_FinancingHeader.Note,
tbl_FinancingHeader.TotalAmount,
(select top 1 debit from tbl_JournalVoucherDetails where debit > 0 and  ParentGuid = tbl_FinancingHeader.JVGuid) as InstallmentAmount,


 
 (

 select sum(tbl_Reconciliation.Amount) from tbl_Reconciliation 
 left join tbl_JournalVoucherDetails on tbl_JournalVoucherDetails.Guid = tbl_Reconciliation.JVDetailsGuid
 where tbl_JournalVoucherDetails.ParentGuid =tbl_FinancingHeader.jvGuid
 
 
and  accountid = @GLAccount
) as Paid ,
tbl_FinancingHeader.MonthsCount,
Format((select top 1 DueDate from tbl_JournalVoucherDetails where debit > 0 and ParentGuid = tbl_FinancingHeader.JVGuid order by duedate asc),'yyyy-MM') as FirstInstallmentDate,
Format((select top 1 DueDate from tbl_JournalVoucherDetails where debit > 0 and  ParentGuid = tbl_FinancingHeader.JVGuid order by duedate desc),'yyyy-MM') as LastInstallmentDate
, 
(select  sum(hjvd.Total) -
isnull((select sum(tbl_Reconciliation.Amount) from tbl_Reconciliation
left join tbl_JournalVoucherDetails sdd on sdd.Guid = tbl_Reconciliation.JVDetailsGuid

  where  sdd.ParentGuid = hjvd.ParentGuid
and  accountid =  @GLAccount  
    and DueDate   <= (SELECT DATEADD(month, ((YEAR(GETDATE() ) - 1900) * 12) + MONTH(GETDATE() ), -1))

  ),0) as tt
 from tbl_JournalVoucherDetails as hjvd
   
 where hjvd.accountid = @GLAccount and
  hjvd.ParentGuid	=  tbl_FinancingHeader.JVGuid
   and DueDate   <= (SELECT DATEADD(month, ((YEAR(GETDATE() ) - 1900) * 12) + MONTH(GETDATE() ), -1))
  group by hjvd.ParentGuid
) 
 as DueAmount
--,
--(
-- select sum(amount*-1) from tbl_Reconciliation  ss
-- left join tbl_JournalVoucherDetails on tbl_JournalVoucherDetails.Guid = ss.JVDetailsGuid
--left join tbl_JournalVoucherHeader on tbl_JournalVoucherDetails.ParentGuid = tbl_JournalVoucherHeader.Guid
-- where VoucherNumber in (
--select tbl_Reconciliation.VoucherNumber from tbl_Reconciliation 
--
--where   
-- JVDetailsGuid in (
-- select guid   from tbl_JournalVoucherDetails where ParentGuid in(select jvguid from tbl_FinancingDetails where HeaderGuid =tbl_FinancingHeader.guid union all  select jvguid from tbl_FinancingHeader as ss where ss.guid = tbl_FinancingHeader.guid )
-- 
--  ))and tbl_JournalVoucherHeader.JVTypeID=15 ) 
,
--(select sum(total) from tbl_JournalVoucherDetails dd where dd.Guid in ( select JVDetailsGuid from tbl_Reconciliation rr where rr.VoucherNumber in (
-- select VoucherNumber from tbl_Reconciliation  ss
-- left join tbl_JournalVoucherDetails on tbl_JournalVoucherDetails.Guid = ss.JVDetailsGuid
--left join tbl_JournalVoucherHeader on tbl_JournalVoucherDetails.ParentGuid = tbl_JournalVoucherHeader.Guid
-- where VoucherNumber in (
--select tbl_Reconciliation.VoucherNumber from tbl_Reconciliation 
--
--where   
-- JVDetailsGuid in (
-- select guid   from tbl_JournalVoucherDetails where ParentGuid in(select jvguid from tbl_FinancingDetails where HeaderGuid =tbl_FinancingHeader.guid
--  union all  select jvguid from tbl_FinancingHeader as ss where ss.guid =  tbl_FinancingHeader.guid )
-- 
--  )) )) and ParentGuid in  (select jvguid from tbl_FinancingDetails  fff where fff.HeaderGuid =tbl_FinancingHeader.guid
--  union all  select jvguid from tbl_FinancingHeader as ss where ss.guid =  tbl_FinancingHeader.guid )
--) 
-- as  scheduled
(
  select sum( re.Amount)  
  from tbl_FinancingHeader  FH 
  left join tbl_JournalVoucherDetails Jd on Jd.ParentGuid =  FH.JVGuid
  left join tbl_Reconciliation re on Jd.Guid = re.JVDetailsGuid
   where FH.Guid =tbl_FinancingHeader.Guid
   and   (
  select top 1 SH.JVTypeID from tbl_journalvoucherdetails  SD
  left join tbl_JournalVoucherHeader SH on SH.Guid = SD.ParentGuid
   where SH.JVTypeID=15  and SD.guid in 
  (select SD.JVDetailsGuid from tbl_Reconciliation SD where SD.VoucherNumber = re.VoucherNumber)
  
  
  ) >0
) as  scheduled

 from tbl_FinancingHeader 
left join tbl_LoanTypes on tbl_LoanTypes.id =tbl_FinancingHeader.LoanType
 left join tbl_BusinessPartner on tbl_FinancingHeader.BusinessPartnerID = tbl_BusinessPartner.ID
where LoanType	 <> 1
and ( tbl_FinancingHeader.BusinessPartnerID =@BusinessPartnerID or @BusinessPartnerID = 0)
and (tbl_FinancingHeader.VoucherDate between @Date1 and @Date2)
and (tbl_FinancingHeader.CompanyID = @CompanyID or @CompanyID = 0)
union all 
select 
tbl_JournalVoucherHeader.guid as JVGuid,
RelatedLoanTypeID as LoanType ,
tbl_JournalVoucherHeader.RelatedFinancingHeaderGuid as HeaderGuid ,
tbl_JournalVoucherHeader.JVNumber  as VoucherNumber ,
 (select top 1 SubAccountID from tbl_JournalVoucherDetails where debit > 0 and ParentGuid = tbl_JournalVoucherHeader.Guid order by duedate asc)   as BusinessPartnerID ,
 (select top 1 tbl_BusinessPartner.AName from tbl_JournalVoucherDetails left join tbl_BusinessPartner on tbl_BusinessPartner.ID = tbl_JournalVoucherDetails.SubAccountID where debit > 0 and ParentGuid = tbl_JournalVoucherHeader.Guid )   as BusinessPartnerAName,
 (select top 1 tbl_BusinessPartner.EmpCode from tbl_JournalVoucherDetails left join tbl_BusinessPartner on tbl_BusinessPartner.ID = tbl_JournalVoucherDetails.SubAccountID where debit > 0 and ParentGuid = tbl_JournalVoucherHeader.Guid )  as EmpCode,
N'S_'+ (select code from tbl_LoanTypes where id = tbl_JournalVoucherHeader.RelatedLoanTypeID)   as tbl_LoanTypesCode,
tbl_JournalVoucherHeader.VoucherDate,
 tbl_JournalVoucherHeader.Notes,
(select sum( Debit) from tbl_JournalVoucherDetails where debit > 0 and ParentGuid = tbl_JournalVoucherHeader.Guid  ) as TotalAmount,
  (select top 1 Debit from tbl_JournalVoucherDetails where debit > 0 and ParentGuid = tbl_JournalVoucherHeader.Guid  order by duedate asc )  as InstallmentAmount ,

 (

 select sum(tbl_Reconciliation.Amount) from tbl_Reconciliation 
 left join tbl_JournalVoucherDetails on tbl_JournalVoucherDetails.Guid = tbl_Reconciliation.JVDetailsGuid
 where tbl_JournalVoucherDetails.ParentGuid =tbl_JournalVoucherHeader.Guid
 
 
and  accountid = @GLAccount and debit >0
) as Paid ,
 
(select count( Debit) from tbl_JournalVoucherDetails where debit > 0 and ParentGuid = tbl_JournalVoucherHeader.Guid  )  as MonthsCount,
Format((select top 1 DueDate from tbl_JournalVoucherDetails where debit > 0 and ParentGuid = tbl_JournalVoucherHeader.Guid  order by duedate asc),'yyyy-MM') as FirstInstallmentDate,
Format((select top 1 DueDate from tbl_JournalVoucherDetails where debit > 0 and  ParentGuid =tbl_JournalVoucherHeader.Guid order by duedate desc ),'yyyy-MM') as LastInstallmentDate

, (
--select sum (tbl_JournalVoucherDetails.Total) -  isnull( sum (tbl_Reconciliation.Amount) ,0)
-- from tbl_JournalVoucherDetails
-- left join tbl_Reconciliation on tbl_JournalVoucherDetails.Guid = tbl_Reconciliation.JVDetailsGuid
-- where accountid = @GLAccount and
--  ParentGuid	= tbl_JournalVoucherHeader.Guid
-- 
-- and DueDate   <= (SELECT DATEADD(month, ((YEAR(GETDATE() ) - 1900) * 12) + MONTH(GETDATE() ), -1))

 




--select sum(a)DueAmount from (
--select sum (tbl_JournalVoucherDetails.Total) a 
-- from tbl_JournalVoucherDetails
-- 
-- where accountid = @GLAccount and
--  ParentGuid	= tbl_JournalVoucherHeader.Guid
-- 
-- and DueDate   <= (SELECT DATEADD(month, ((YEAR(GETDATE() ) - 1900) * 12) + MONTH(GETDATE() ), -1))
-- union all 
-- select isnull( sum (tbl_Reconciliation.Amount) ,0) a from 
-- tbl_Reconciliation 
-- left join tbl_JournalVoucherDetails ss on ss.Guid = tbl_Reconciliation.JVDetailsGuid
--where  tbl_Reconciliation.JVDetailsGuid 
-- in (select Guid from tbl_JournalVoucherDetails where ParentGuid = tbl_JournalVoucherHeader.Guid)
-- and ss.DueDate<=
--(SELECT DATEADD(month, ((YEAR(GETDATE() ) - 1900) * 12) + MONTH(GETDATE() ), -1))
-- ) as q


select total from dbo.GetSumDueUnReconciledAmountByFinanceGuid(@glaccount,(SELECT DATEADD(month, ((YEAR(GETDATE() ) - 1900) * 12) + MONTH(GETDATE() ), -1))
,@BusinessPartnerID,@CompanyID, tbl_JournalVoucherHeader.guid,0)
) 
 as DueAmount,
(
 select sum(amount) from tbl_Reconciliation  ss
 left join tbl_JournalVoucherDetails on tbl_JournalVoucherDetails.Guid = ss.JVDetailsGuid
left join tbl_JournalVoucherHeader as sss  on tbl_JournalVoucherDetails.ParentGuid = sss.Guid
 where VoucherNumber in (
select tbl_Reconciliation.VoucherNumber from tbl_Reconciliation 

where  
 JVDetailsGuid in (
 select guid   from tbl_JournalVoucherDetails where Debit > 0 
 
  ))and sss.JVTypeID=15 and  Debit > 0 and   ParentGuid =  tbl_JournalVoucherHeader.guid ) as  scheduled
from tbl_JournalVoucherHeader

 where JVTypeID = 15
 and (  (select top 1 SubAccountID from tbl_JournalVoucherDetails where debit > 0 and ParentGuid = tbl_JournalVoucherHeader.Guid order by duedate asc)  =@BusinessPartnerID or @BusinessPartnerID = 0)
and (tbl_JournalVoucherHeader.VoucherDate between @Date1 and @Date2)
and (tbl_JournalVoucherHeader.CompanyID = @CompanyID or @CompanyID = 0)) select * from dt 

where (@HideZeroBalances = 0 or (dt.totalamount-isnull( dt.paid ,0)<>0)  )
order by VoucherNumber desc
";

                clsSQL cls = new clsSQL();
                DataTable dt = cls.ExecuteQueryStatement(a, cls.CreateDataBaseConnectionString(CompanyID), prm);

                return dt;
            }
            catch (Exception ex)
            {

                throw;
            }


        }

        public DataTable SelectLoanReportRJ(string Date1, string Date2,int accountid,  int UserId, int CompanyID)
        {
            try
            {
                SqlParameter[] prm = {
                    new SqlParameter("@Date1", SqlDbType.Date) { Value = Date1 },
                     new SqlParameter("@Date2", SqlDbType.Date) { Value = Date2 },
                         new SqlParameter("@CompanyID", SqlDbType.Int) { Value = CompanyID },
                  new SqlParameter("@accountid", SqlDbType.Int) { Value = accountid },

                };
                string a   = @"select * from (
select tbl_BusinessPartner.AName,
tbl_BusinessPartner.EmpCode as employee_number,
 

FORMAT(@date1, 'dd/MM/yyyy')
   as  effective_start_date,
 
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


,(select sum (t) from ((select  Total  t from tbl_JournalVoucherDetails 
inner join tbl_JournalVoucherHeader on tbl_JournalVoucherHeader.guid = tbl_JournalVoucherDetails.ParentGuid
left join tbl_FinancingHeader on tbl_FinancingHeader.JVGuid = tbl_JournalVoucherHeader.Guid
left join tbl_FinancingDetails on tbl_FinancingDetails.JVGuid = tbl_JournalVoucherHeader.Guid

where tbl_JournalVoucherDetails.AccountID = @accountid 
and isnull( tbl_FinancingHeader.IsShowInMonthlyReports,1)<>0
and tbl_JournalVoucherDetails.SubAccountID =tbl_BusinessPartner.ID 
 
and   ( tbl_JournalVoucherHeader.relatedloantypeid
in (select id from tbl_LoanTypes where tbl_LoanTypes.MainTypeID=2 and IsShowInMonthlyReports=1) 
or tbl_FinancingHeader.LoanType in 
(select id from tbl_LoanTypes where tbl_LoanTypes.MainTypeID=2 and IsShowInMonthlyReports=1)) )
union all 
select amount *-1 t from tbl_Reconciliation where JVDetailsGuid in (
select tbl_JournalVoucherDetails.guid

from tbl_JournalVoucherDetails 
inner join tbl_JournalVoucherHeader on tbl_JournalVoucherHeader.guid = tbl_JournalVoucherDetails.ParentGuid
left join tbl_FinancingHeader on tbl_FinancingHeader.JVGuid = tbl_JournalVoucherHeader.Guid
left join tbl_FinancingDetails on tbl_FinancingDetails.JVGuid = tbl_JournalVoucherHeader.Guid

where tbl_JournalVoucherDetails.AccountID = @accountid 
and isnull( tbl_FinancingHeader.IsShowInMonthlyReports,1)<>0
and tbl_JournalVoucherDetails.SubAccountID =tbl_BusinessPartner.ID 
 
and   ( tbl_JournalVoucherHeader.relatedloantypeid
in (select id from tbl_LoanTypes where tbl_LoanTypes.MainTypeID=2 and IsShowInMonthlyReports=1) 
or tbl_FinancingHeader.LoanType in 
(select id from tbl_LoanTypes where tbl_LoanTypes.MainTypeID=2 and IsShowInMonthlyReports=1)) ))as q)as input_value3


, 'Monthly Installment' as input_name4,
 (


 
 select * from  GetSumDueUnReconciledAmountByFinanceGuid (

@AccountID,
(SELECT DATEADD(month, ((YEAR(GETDATE() ) - 1900) * 12) + MONTH(GETDATE() ), -1)) ,
tbl_BusinessPartner.ID ,
@CompanyID,'00000000-0000-0000-0000-000000000000',
-1
)

) as input_value4,


 'Comment' as input_name5
,'loan' as input_value5
, '' as conc
from  
  tbl_BusinessPartner where tbl_BusinessPartner.CompanyID =@CompanyID) as q where q.input_value4>0";

                clsSQL cls = new clsSQL();
                DataTable dt = cls.ExecuteQueryStatement(a, cls.CreateDataBaseConnectionString(CompanyID), prm);

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
                string a = @"select * from (
select tbl_BusinessPartner.AName,
tbl_BusinessPartner.EmpCode as employee_number,
FORMAT(@date1, 'dd/MM/yyyy') as  effective_start_date,
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


,(select sum(a) from (select sum(Total) from tbl_JournalVoucherDetails 
inner join tbl_JournalVoucherHeader on tbl_JournalVoucherHeader.guid = tbl_JournalVoucherDetails.ParentGuid
left join tbl_FinancingHeader on tbl_FinancingHeader.JVGuid = tbl_JournalVoucherHeader.Guid
left join tbl_FinancingDetails on tbl_FinancingDetails.JVGuid = tbl_JournalVoucherHeader.Guid
left join tbl_FinancingHeader ss on ss.Guid = tbl_FinancingDetails.HeaderGuid
where tbl_JournalVoucherDetails.AccountID = @AccountID 
and tbl_JournalVoucherDetails.SubAccountID =tbl_BusinessPartner.ID 
 
and   ( tbl_JournalVoucherHeader.relatedloantypeid = 1 or tbl_FinancingHeader.LoanType = 1 or ss.LoanType = 1) 



union all 
select amount *-1 t from tbl_Reconciliation where JVDetailsGuid in (
select tbl_JournalVoucherDetails.guid
from tbl_JournalVoucherDetails 
inner join tbl_JournalVoucherHeader on tbl_JournalVoucherHeader.guid = tbl_JournalVoucherDetails.ParentGuid
left join tbl_FinancingHeader on tbl_FinancingHeader.JVGuid = tbl_JournalVoucherHeader.Guid
left join tbl_FinancingDetails on tbl_FinancingDetails.JVGuid = tbl_JournalVoucherHeader.Guid
left join tbl_FinancingHeader ss on ss.Guid = tbl_FinancingDetails.HeaderGuid
where tbl_JournalVoucherDetails.AccountID = @accountid 
and isnull( tbl_FinancingHeader.IsShowInMonthlyReports,1)<>0
and tbl_JournalVoucherDetails.SubAccountID = tbl_BusinessPartner.ID 
 and   ( tbl_JournalVoucherHeader.relatedloantypeid=1
or tbl_FinancingHeader.LoanType =1 or ss.LoanType = 1) ))as q



)as input_value3


,'Monthly Installment' as input_name4,

(select sum(Total) from tbl_JournalVoucherDetails 
inner join tbl_JournalVoucherHeader on tbl_JournalVoucherHeader.guid = tbl_JournalVoucherDetails.ParentGuid
left join tbl_FinancingHeader on tbl_FinancingHeader.JVGuid = tbl_JournalVoucherHeader.Guid
left join tbl_FinancingDetails on tbl_FinancingDetails.JVGuid = tbl_JournalVoucherHeader.Guid
left join tbl_FinancingHeader ss on ss.Guid = tbl_FinancingDetails.HeaderGuid
where tbl_JournalVoucherDetails.AccountID = @AccountID 
and tbl_JournalVoucherDetails.SubAccountID =tbl_BusinessPartner.ID 
and tbl_JournalVoucherDetails.DueDate <=@date2
and   ( tbl_JournalVoucherHeader.relatedloantypeid = 1 or tbl_FinancingHeader.LoanType = 1 or ss.LoanType = 1)) as input_value4,


 'Comment' as input_name5
,'Mobile' as input_value5
, '' as conc
from  
  tbl_BusinessPartner where tbl_BusinessPartner.CompanyID =@CompanyID) as q where q.input_value4>0";



                a = @"select * from (
select tbl_BusinessPartner.AName,
tbl_BusinessPartner.EmpCode as employee_number,
FORMAT(@date1, 'dd/MM/yyyy') as  effective_start_date,
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
inner join tbl_JournalVoucherHeader on tbl_JournalVoucherHeader.guid = tbl_JournalVoucherDetails.ParentGuid
left join tbl_FinancingHeader on tbl_FinancingHeader.JVGuid = tbl_JournalVoucherHeader.Guid
left join tbl_FinancingDetails on tbl_FinancingDetails.JVGuid = tbl_JournalVoucherHeader.Guid
left join tbl_FinancingHeader ss on ss.Guid = tbl_FinancingDetails.HeaderGuid
where tbl_JournalVoucherDetails.AccountID = @AccountID 
and tbl_JournalVoucherDetails.SubAccountID =tbl_BusinessPartner.ID 
 
and   ( tbl_JournalVoucherHeader.relatedloantypeid = 1 or tbl_FinancingHeader.LoanType = 1 or ss.LoanType = 1) )as input_value3


,'Monthly Installment' as input_name4,

(select sum(Total) from tbl_JournalVoucherDetails 
inner join tbl_JournalVoucherHeader on tbl_JournalVoucherHeader.guid = tbl_JournalVoucherDetails.ParentGuid
left join tbl_FinancingHeader on tbl_FinancingHeader.JVGuid = tbl_JournalVoucherHeader.Guid
left join tbl_FinancingDetails on tbl_FinancingDetails.JVGuid = tbl_JournalVoucherHeader.Guid
left join tbl_FinancingHeader ss on ss.Guid = tbl_FinancingDetails.HeaderGuid
where tbl_JournalVoucherDetails.AccountID = @AccountID 
and tbl_JournalVoucherDetails.SubAccountID =tbl_BusinessPartner.ID 
and tbl_JournalVoucherDetails.DueDate <=@date2
and   ( tbl_JournalVoucherHeader.relatedloantypeid = 1 or tbl_FinancingHeader.LoanType = 1 or ss.LoanType = 1)) as input_value4,


 'Comment' as input_name5
,'Mobile' as input_value5
, '' as conc
from  
  tbl_BusinessPartner where tbl_BusinessPartner.CompanyID =@CompanyID) as q where q.input_value4>0";




                a = @"select * from (
select tbl_BusinessPartner.AName,
tbl_BusinessPartner.EmpCode as employee_number,
FORMAT(@date1, 'dd/MM/yyyy') as  effective_start_date,
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


,

(

select sum(t)from (
select sum(Total) t from tbl_JournalVoucherDetails 
inner join tbl_JournalVoucherHeader on tbl_JournalVoucherHeader.guid = tbl_JournalVoucherDetails.ParentGuid
left join tbl_FinancingHeader on tbl_FinancingHeader.JVGuid = tbl_JournalVoucherHeader.Guid
left join tbl_FinancingDetails on tbl_FinancingDetails.JVGuid = tbl_JournalVoucherHeader.Guid
left join tbl_FinancingHeader ss on ss.Guid = tbl_FinancingDetails.HeaderGuid
where tbl_JournalVoucherDetails.AccountID = @AccountID
and tbl_FinancingHeader.IsShowInMonthlyReports=1
and tbl_JournalVoucherDetails.SubAccountID =tbl_BusinessPartner.ID 
 
and   ( tbl_JournalVoucherHeader.relatedloantypeid = 1 or tbl_FinancingHeader.LoanType = 1 or ss.LoanType = 1)  
union all 
select amount *-1 t from tbl_Reconciliation where JVDetailsGuid in (
select tbl_JournalVoucherDetails.guid

from tbl_JournalVoucherDetails 
inner join tbl_JournalVoucherHeader on tbl_JournalVoucherHeader.guid = tbl_JournalVoucherDetails.ParentGuid
left join tbl_FinancingHeader on tbl_FinancingHeader.JVGuid = tbl_JournalVoucherHeader.Guid
left join tbl_FinancingDetails on tbl_FinancingDetails.JVGuid = tbl_JournalVoucherHeader.Guid
left join tbl_FinancingHeader ss on ss.Guid = tbl_FinancingDetails.HeaderGuid
where tbl_JournalVoucherDetails.AccountID = @AccountID
and tbl_FinancingHeader.IsShowInMonthlyReports=1
and tbl_JournalVoucherDetails.SubAccountID =tbl_BusinessPartner.ID 
 
and   ( tbl_JournalVoucherHeader.relatedloantypeid = 1 or tbl_FinancingHeader.LoanType = 1 or ss.LoanType = 1)  ))as q  )as input_value3


,'Monthly Installment' as input_name4,

(


select  sum(Amount) as Total  from( select -1 as ss, -1 as a,tbl_JournalVoucherDetails.total as Amount,tbl_JournalVoucherDetails.* from  tbl_JournalVoucherDetails 
inner join tbl_JournalVoucherHeader on tbl_JournalVoucherHeader.guid = tbl_JournalVoucherDetails.ParentGuid
left join tbl_FinancingHeader on tbl_FinancingHeader.JVGuid = tbl_JournalVoucherHeader.Guid
left join tbl_FinancingDetails on tbl_FinancingDetails.JVGuid = tbl_JournalVoucherHeader.Guid
left join tbl_FinancingHeader ss on ss.Guid = tbl_FinancingDetails.HeaderGuid
where tbl_JournalVoucherDetails.AccountID = @AccountID 
and tbl_FinancingHeader.IsShowInMonthlyReports=1
and tbl_JournalVoucherDetails.SubAccountID =tbl_BusinessPartner.ID 
and tbl_JournalVoucherDetails.DueDate <= @date2
and   ( tbl_JournalVoucherHeader.relatedloantypeid = 1 or tbl_FinancingHeader.LoanType = 1 or ss.LoanType = 1)
union all
select   tbl_JournalVoucherHeader.JVTypeID, tbl_JournalVoucherHeader.JVTypeID as a, -1*tbl_Reconciliation.Amount as Amount,tbl_JournalVoucherDetails.* from tbl_JournalVoucherDetails 
inner join tbl_Reconciliation on tbl_Reconciliation.JVDetailsGuid = tbl_JournalVoucherDetails.Guid
inner join tbl_JournalVoucherHeader on tbl_JournalVoucherHeader.Guid = tbl_Reconciliation.TransactionGuid
 where tbl_JournalVoucherDetails.AccountID = @AccountID and isnull( tbl_JournalVoucherHeader.RelatedLoanTypeID,0)= 0 
and tbl_JournalVoucherHeader.JVTypeID  <>14
and tbl_JournalVoucherDetails.SubAccountID =tbl_BusinessPartner.ID 
and tbl_JournalVoucherDetails.DueDate <= @date2
and tbl_JournalVoucherDetails.ParentGuid in (

select  tbl_JournalVoucherDetails.ParentGuid   from  tbl_JournalVoucherDetails 
inner join tbl_JournalVoucherHeader on tbl_JournalVoucherHeader.guid = tbl_JournalVoucherDetails.ParentGuid
left join tbl_FinancingHeader on tbl_FinancingHeader.JVGuid = tbl_JournalVoucherHeader.Guid
left join tbl_FinancingDetails on tbl_FinancingDetails.JVGuid = tbl_JournalVoucherHeader.Guid
left join tbl_FinancingHeader ss on ss.Guid = tbl_FinancingDetails.HeaderGuid
where tbl_JournalVoucherDetails.AccountID = @AccountID 
and tbl_FinancingHeader.IsShowInMonthlyReports=1
and tbl_JournalVoucherDetails.SubAccountID =tbl_BusinessPartner.ID 
and tbl_JournalVoucherDetails.DueDate <= @date2
and   ( tbl_JournalVoucherHeader.relatedloantypeid = 1 or tbl_FinancingHeader.LoanType = 1 or ss.LoanType = 1)



)
) as q


) as input_value4,


 'Comment' as input_name5
,'Mobile' as input_value5
, '' as conc
from  
  tbl_BusinessPartner where tbl_BusinessPartner.CompanyID =@CompanyID) as q where q.input_value4>0";


                a = @"select * from (
select tbl_BusinessPartner.AName,
tbl_BusinessPartner.EmpCode as employee_number,
FORMAT(@date1, 'dd/MM/yyyy') as  effective_start_date,
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


,

(
select sum(t) from (select sum(Total)t from tbl_JournalVoucherDetails 
inner join tbl_JournalVoucherHeader on tbl_JournalVoucherHeader.guid = tbl_JournalVoucherDetails.ParentGuid
left join tbl_FinancingHeader on tbl_FinancingHeader.JVGuid = tbl_JournalVoucherHeader.Guid
left join tbl_FinancingDetails on tbl_FinancingDetails.JVGuid = tbl_JournalVoucherHeader.Guid
left join tbl_FinancingHeader ss on ss.Guid = tbl_FinancingDetails.HeaderGuid
where tbl_JournalVoucherDetails.AccountID = @AccountID 
and isnull(tbl_FinancingHeader.IsShowInMonthlyReports,1)<>0
and tbl_JournalVoucherDetails.SubAccountID =tbl_BusinessPartner.ID 
 
and   ( tbl_JournalVoucherHeader.relatedloantypeid = 1 or tbl_FinancingHeader.LoanType = 1 or ss.LoanType = 1) 




union all 
select amount *-1 t from tbl_Reconciliation where JVDetailsGuid in (
select tbl_JournalVoucherDetails.guid
from tbl_JournalVoucherDetails 
inner join tbl_JournalVoucherHeader on tbl_JournalVoucherHeader.guid = tbl_JournalVoucherDetails.ParentGuid
left join tbl_FinancingHeader on tbl_FinancingHeader.JVGuid = tbl_JournalVoucherHeader.Guid
left join tbl_FinancingDetails on tbl_FinancingDetails.JVGuid = tbl_JournalVoucherHeader.Guid
left join tbl_FinancingHeader ss on ss.Guid = tbl_FinancingDetails.HeaderGuid
where tbl_JournalVoucherDetails.AccountID = @accountid 
and isnull( tbl_FinancingHeader.IsShowInMonthlyReports,1)<>0
and tbl_JournalVoucherDetails.SubAccountID = tbl_BusinessPartner.ID 
 and   ( tbl_JournalVoucherHeader.relatedloantypeid=1
or tbl_FinancingHeader.LoanType =1 or ss.LoanType = 1) ))as q



)as input_value3


,'Monthly Installment' as input_name4,

(

 


 
 select * from  GetSumDueUnReconciledAmountByFinanceGuid (

@AccountID,
(SELECT DATEADD(month, ((YEAR(GETDATE() ) - 1900) * 12) + MONTH(GETDATE() ), -1)) , 
tbl_BusinessPartner.ID ,
@CompanyID,'00000000-0000-0000-0000-000000000000',1
)
 


) as input_value4,


 'Comment' as input_name5
,'Mobile' as input_value5
, '' as conc
from  
  tbl_BusinessPartner where tbl_BusinessPartner.CompanyID =@CompanyID) as q where q.input_value4>0";
                clsSQL cls = new clsSQL();
                DataTable dt = cls.ExecuteQueryStatement(a, cls.CreateDataBaseConnectionString(CompanyID), prm);

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
tbl_BusinessPartner.EmpCode as EMPLOYEE_NUMBER,tbl_BusinessPartner.EName,
FORMAT(tbl_Subscriptions.TransactionDate, '01/MM/yyyy')
  as  EFFECTIVE_START_DATE,
 'TPT Deductions' as ELEMENT_NAME,
'1' as COST_SEGMENT1

,'D010' as COST_SEGMENT2

,'116003' as COST_SEGMENT3

, '0' as COST_SEGMENT4

, 'Actual Source Of Deduction' as INPUT_NAME1

,'Jordan Islamic Bank' as INPUT_VALUE1

,'Source' as INPUT_NAME2

,'Jordan Islamic Bank/Khrebet Alsouq-Ajwaa Alordon Ass. (Monthly Subscribtion)' as INPUT_VALUE2

, 'Source Amount In JOD' as INPUT_NAME3

, 0 as INPUT_VALUE3

,'Monthly Installment' as INPUT_NAME4

,tbl_Subscriptions.Amount as INPUT_VALUE4
,
 '' as INPUT_NAME5

,'' as INPUT_VALUE5

 
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
                DataTable dt = cls.ExecuteQueryStatement(a, cls.CreateDataBaseConnectionString(CompanyID), prm);

                return dt;
            }
            catch (Exception ex)
            {

                throw;
            }


        }
        public DataTable SelectFinancingHeaderByGuid(string guid, DateTime date1, DateTime date2,   int BranchID,int CreationUserID, int CompanyID,int CurrentUserId,string LoanMainType,int businessPartnerID, SqlTransaction trn = null)
        {
            try
            {
                clsSQL clsSQL = new clsSQL();

                SqlParameter[] prm =
                 {
                    new SqlParameter("@Guid", SqlDbType.UniqueIdentifier) { Value = Simulate.Guid( guid) },
 new SqlParameter("@date1", SqlDbType.DateTime) { Value =Simulate.StringToDate(  date1 )},
  new SqlParameter("@date2", SqlDbType.DateTime) { Value = Simulate.StringToDate(  date2 )},
        new SqlParameter("@businessPartnerID", SqlDbType.Int) { Value = businessPartnerID },
        new SqlParameter("@CompanyID", SqlDbType.Int) { Value = CompanyID },
          new SqlParameter("@BranchID", SqlDbType.Int) { Value = BranchID },
                    new SqlParameter("@CreationUserID", SqlDbType.Int) { Value = CreationUserID },
                                       new SqlParameter("@CurrentUserId", SqlDbType.Int) { Value = CurrentUserId },

                    

                };
                string  a = @" 

declare @monthEnd date = (SELECT DATEADD(month, ((YEAR( GETDATE()) - 1900) * 12) + MONTH( GETDATE()), -1))
declare @ARAccount int = 0
set @ARAccount=(select tbl_AccountSetting.AccountID from tbl_AccountSetting where tbl_AccountSetting.AccountRefID = 7 and Active= 1 and CompanyID =@CompanyID)
select tbl_FinancingHeader.*,
tbl_Branch.AName as BranchName,
BusinessPartner.AName as BusinessPartnerName,
BusinessPartner.EmpCode as BusinessPartnerEmpCode,
Grantor.AName as GrantorName,
tbl_employee.AName as CreationUserName ,
tbl_LoanTypes.AName AS LoanTypeanAName,
PaymentAccount.aname  as PaymentAccountIDAName,
case when (PaymentAccount.ID = (select tbl_AccountSetting.AccountID from tbl_AccountSetting where tbl_AccountSetting.AccountRefID = 5 and Active= 1 and CompanyID =@CompanyID))then 
tbl_CashDrawer.aname  
when  (PaymentAccount.ID = (select tbl_AccountSetting.AccountID from tbl_AccountSetting where tbl_AccountSetting.AccountRefID = 15 and Active= 1 and CompanyID =@CompanyID))
then 
tbl_Banks.aname  
else 
PaymentSubAccount.aname  
end 
  as PaymentSubAccountIDAName,
( select top 1 s.FirstInstallmentDate from tbl_FinancingDetails as s 
   where  s.HeaderGuid = tbl_FinancingHeader.Guid)
 as FirstInstallmentDate,
 


 case when tbl_FinancingHeader.LoanType=1 then ( select sum( s.InstallmentAmount) from tbl_FinancingDetails as s 
 where  s.HeaderGuid = tbl_FinancingHeader.Guid)

 else (
 
 select top 1 debit from tbl_JournalVoucherDetails where ParentGuid = tbl_FinancingHeader.JVGuid and debit > 1 order by duedate asc
 
 ) end 

 as TotalInstallmentAmount,





 ( select sum( s.TotalAmountWithInterest) from tbl_FinancingDetails as s 
   where  s.HeaderGuid = tbl_FinancingHeader.Guid)
 as TotalAmountWithInterest ,
STUFF((SELECT ', ' + ss.Description 
FROM tbl_FinancingDetails ss
WHERE ss.headerguid=tbl_FinancingHeader.Guid
FOR XML PATH('')), 1, 1, '') as DetailsDescription
, tbl_LoanTypes.AName as  LoanTypeAName
,
 
 

--case when (tbl_FinancingHeader.LoanType=1 )then 
--
-- 
-- (
-- select sum( tbl_Reconciliation.Amount) from tbl_Reconciliation 
-- left join tbl_JournalVoucherDetails rr on rr.Guid = tbl_Reconciliation.JVDetailsGuid
--  where   accountid = @ARAccount   and             
--(  
-- 
--  rr.ParentGuid in (select JVGuid from tbl_FinancingDetails dd where dd.HeaderGuid = tbl_FinancingHeader.Guid)
-- )
-- )
--
--
--else 
--
-- (
--
--	select sum( tbl_Reconciliation.Amount )from tbl_Reconciliation 
-- left join tbl_JournalVoucherDetails rr on rr.Guid = tbl_Reconciliation.JVDetailsGuid
-- 
-- where   accountid = @ARAccount   and             
--(  
--   rr.ParentGuid =  tbl_FinancingHeader.JVGuid 
--)
--)  end 

 
0
  as Paid
,tbl_FinancingHeader.JVGuid
,
--(
--select sum (tbl_JournalVoucherDetails.Total) -  isnull( sum (tbl_Reconciliation.Amount) ,0)
-- from tbl_JournalVoucherDetails
-- left join tbl_Reconciliation on tbl_JournalVoucherDetails.Guid = tbl_Reconciliation.JVDetailsGuid
-- where accountid = @ARAccount and
--  ParentGuid	in
--(
--select jvGuid from tbl_FinancingDetails d where d.headerguid =tbl_FinancingHeader.guid 
--union all 
--select jvGuid from tbl_FinancingHeader as s
--where s.Guid=tbl_FinancingHeader.guid
--)
-- and  tbl_JournalVoucherDetails.DueDate   <=  @monthEnd 
--
-- 
--) 
0 as DueAmount,
tblSalesMan.AName as SalesManName 



from tbl_FinancingHeader
 left join tbl_Branch on tbl_Branch.ID = tbl_FinancingHeader.BranchID
 left join tbl_employee tblSalesMan on tblSalesMan.id =SalesManID
  left join tbl_BusinessPartner as  BusinessPartner on BusinessPartner.ID = tbl_FinancingHeader.BusinessPartnerID
    left join tbl_BusinessPartner as Grantor on Grantor.ID = tbl_FinancingHeader.Grantor
	  left join tbl_employee  on tbl_employee.ID = tbl_FinancingHeader.CreationUserID 
   left join tbl_LoanTypes  on tbl_LoanTypes.ID = tbl_FinancingHeader.LoanType 
    left join tbl_Accounts  as PaymentAccount on PaymentAccount.ID = tbl_FinancingHeader.PaymentAccountID 
	   left join tbl_BusinessPartner as PaymentSubAccount on PaymentSubAccount.ID = tbl_FinancingHeader.PaymentSubAccountID   
  left join tbl_CashDrawer   on tbl_CashDrawer.ID = tbl_FinancingHeader.PaymentSubAccountID   
    left join tbl_Banks  on tbl_Banks.ID = tbl_FinancingHeader.PaymentSubAccountID   
 
where 
(tbl_FinancingHeader.Guid=@Guid or @Guid='00000000-0000-0000-0000-000000000000' )  
and (tbl_FinancingHeader.CompanyID=@CompanyID or @CompanyID=0 )
and (tbl_FinancingHeader.BranchID=@BranchID or @BranchID=0 )
and (tbl_FinancingHeader.CreationUserID=@CreationUserID or @CreationUserID=0 )
and (tbl_LoanTypes.MainTypeID in(" + LoanMainType + @") or -1 in (" + LoanMainType + @"))
and (BusinessPartner.ID  = @businessPartnerID or @businessPartnerID=0)
and cast( tbl_FinancingHeader.VoucherDate as date) between  cast(@date1 as date) and  cast(@date2 as date) 
and  ( (select IsAccess from tbl_UserAuthorization where UserID = @CurrentUserId and tbl_UserAuthorization.PageID=71)=0
 or ( tbl_FinancingHeader.branchid =0 and (select count(id) from tbl_Branch where CompanyID = @CompanyID)=(select count (ModelID) from  tbl_UserAuthorizationModels where TypeID =1 and companyid =@CompanyID and UserID = @CurrentUserId and IsAccess =1) )


or tbl_FinancingHeader.branchid in (select ModelID from 
    tbl_UserAuthorizationModels where TypeID =1 and UserID = @CurrentUserId and ModelID = tbl_FinancingHeader.BranchID and IsAccess =1) or @CurrentUserId=0 )


--    union all 
--    select tbl_JournalVoucherHeader.Guid,VoucherDate,branchid,costcenterid   id,0 Bankcostcenterid    ,jvnumber as VoucherNumber,
--   (select top 1 SubAccountID guid from tbl_JournalVoucherDetails where ParentGuid = tbl_JournalVoucherHeader.Guid)as BusinessPartnerID,
--   Notes,
--   (select sum( debit )guid from tbl_JournalVoucherDetails where ParentGuid = tbl_JournalVoucherHeader.Guid)as TotalAmount ,
--   0 as DownPayment,
--   (select sum( debit )guid from tbl_JournalVoucherDetails where ParentGuid = tbl_JournalVoucherHeader.Guid)as NetAmount ,
--   0 as Grantor,
--   tbl_JournalVoucherHeader.CreationUserID ,
--   tbl_JournalVoucherHeader.CreationDate,
--   tbl_JournalVoucherHeader.ModificationUserID,
--   tbl_JournalVoucherHeader.ModificationDate ,
--   tbl_JournalVoucherHeader.CompanyID ,
--   0 as LoanType,
--   Guid as JVGuid ,
--   0 as IntrestRate, 
--   1 as IsAmountReturned ,
--   (select sum( debit )guid from tbl_JournalVoucherDetails where ParentGuid = tbl_JournalVoucherHeader.Guid and debit > 0 )as MonthsCount ,
--   0 as PaymentAccountID,
--   0 as PaymentSubAccountID,
--   0 as VendorID,
--   1 as IsShowInMonthlyReportschange,
--   '' as BranchName ,
--   (select top 1 tbl_BusinessPartner.AName guid from tbl_JournalVoucherDetails left join tbl_BusinessPartner 
--   on tbl_BusinessPartner.ID = tbl_JournalVoucherDetails.SubAccountID where ParentGuid = tbl_JournalVoucherHeader.Guid) 
--     as BusinessPartnerName,
--   (select top 1 tbl_BusinessPartner.EmpCode guid from tbl_JournalVoucherDetails left join tbl_BusinessPartner 
--   on tbl_BusinessPartner.ID = tbl_JournalVoucherDetails.SubAccountID where ParentGuid = tbl_JournalVoucherHeader.Guid) as BusinessPartnerEmpCode,
--   
--   
--   
--   '' as GrantorName,
--     tbl_employee.AName
--       as CreationUserName ,
--   'Scheduling' AS LoanTypeanAName,
--   '' as PaymentAccountIDAName,
--   '' as PaymentSubAccountIDAName ,
--   (select top 1 DueDate from tbl_JournalVoucherDetails where ParentGuid = tbl_JournalVoucherHeader.Guid and debit > 0 order by DueDate asc )as FirstInstallmentDate
--    
--    ,(select top 1( debit )guid from tbl_JournalVoucherDetails where ParentGuid = tbl_JournalVoucherHeader.Guid and debit > 0 ) as TotalInstallmentAmount,
--    (select sum( debit )guid from tbl_JournalVoucherDetails where ParentGuid = tbl_JournalVoucherHeader.Guid and debit > 0 ) as TotalAmountWithInterest  
--   
--    , '' as DetailsDescription
--    , N'جدولة' as LoanTypeAName
--    ,
--   --(
--   --
--   -- 
--   -- select sum(tbl_Reconciliation.Amount) from tbl_Reconciliation 
--   -- left join tbl_JournalVoucherDetails on tbl_JournalVoucherDetails.Guid = tbl_Reconciliation.JVDetailsGuid
--   -- where tbl_JournalVoucherDetails.ParentGuid =  tbl_JournalVoucherHeader.Guid and debit>0
--   --  
--   --
--   --) 
--   0 as Paid
--   , Guid as JVGuid
--   , 
--   --( 
--   --select sum (Total)-sum (tbl_Reconciliation.Amount)  
--   -- from tbl_JournalVoucherDetails fff
--   -- left join tbl_Reconciliation on tbl_Reconciliation.JVDetailsGuid = fff.Guid
--   -- where fff.ParentGuid	=
--   --tbl_JournalVoucherHeader.guid
--   -- 
--   -- and   fff.DueDate  <=@monthEnd
--   --
--   -- 
--   --)
--   0 as DueAmount 
--    from tbl_JournalVoucherHeader  left join tbl_employee on tbl_employee.ID = tbl_JournalVoucherHeader.CreationUserID
--    
--    where JVTypeID = 15
--   and (tbl_JournalVoucherHeader.Guid=@Guid or @Guid='00000000-0000-0000-0000-000000000000' )
--   and (tbl_JournalVoucherHeader.BranchID=@BranchID or @BranchID=0 )
--   and (tbl_JournalVoucherHeader.CreationUserID=@CreationUserID or @CreationUserID=0 )
--   and cast( tbl_JournalVoucherHeader.VoucherDate as date) between  cast(@date1 as date) and  cast(@date2 as date) 
--   and ( ( select top 1 tbl_JournalVoucherDetails.SubAccountID from tbl_JournalVoucherDetails where ParentGuid = tbl_JournalVoucherHeader.Guid)= @businessPartnerID or @businessPartnerID=0)
--   and  ( (select IsAccess from tbl_UserAuthorization where UserID = @CurrentUserId and tbl_UserAuthorization.PageID=71)=0
--    or ( tbl_JournalVoucherHeader.branchid =0 and (select count(id) from tbl_Branch where CompanyID = @CompanyID)=(select count (ModelID) from  tbl_UserAuthorizationModels where TypeID =1 and companyid =@CompanyID and UserID = @CurrentUserId and IsAccess =1) )
--   or tbl_JournalVoucherHeader.branchid in (select ModelID from 
--       tbl_UserAuthorizationModels where TypeID =1 and UserID = @CurrentUserId
--   and ModelID = tbl_JournalVoucherHeader.BranchID and IsAccess =1) or @CurrentUserId=0 )
                ";
                DataTable dt = clsSQL.ExecuteQueryStatement(a, clsSQL.CreateDataBaseConnectionString(CompanyID), prm, trn);
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

 
                     ", clsSQL.CreateDataBaseConnectionString(CompanyID),prm, trn);

                return dt;
            }
            catch (Exception ex)
            {

                throw;
            }


        }
        public bool DeleteFinancingHeaderByGuid(string guid,int CompanyID, SqlTransaction trn)
        {
            try
            {
                clsSQL clsSQL = new clsSQL();

                SqlParameter[] prm =
                 { new SqlParameter("@guid", SqlDbType.UniqueIdentifier) { Value = Simulate.Guid( guid) },

                };
                int A = clsSQL.ExecuteNonQueryStatement(@"delete from tbl_FinancingHeader where (guid=@guid  )", clsSQL.CreateDataBaseConnectionString(CompanyID), prm, trn);

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
                    new SqlParameter("@CostCenterID", SqlDbType.Int) { Value = DBFinancingHeader.CostCenterID },
                       new SqlParameter("@BankCostCenterID", SqlDbType.Int) { Value = DBFinancingHeader.BankCostCenterID },
                    
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
                                       new SqlParameter("@SalesManID", SqlDbType.Int) { Value = DBFinancingHeader.SalesManID },
    new SqlParameter("@IsShowInMonthlyReports", SqlDbType.Bit) { Value = DBFinancingHeader.IsShowInMonthlyReports },
     new SqlParameter("@PurchaseInvoiceRefNumber", SqlDbType.NVarChar,-1) { Value = DBFinancingHeader.PurchaseInvoiceRefNumber },
                };

                string a = @"insert into tbl_FinancingHeader (VoucherDate,BranchID,CostCenterID,BankCostCenterID,VoucherNumber,BusinessPartnerID,
TotalAmount,DownPayment,NetAmount,
                                                                Note,Grantor, LoanType,
                                                               IntrestRate,isAmountReturned,MonthsCount,PaymentAccountID,PaymentSubAccountID,
                                                               CompanyID,CreationUserID,CreationDate,VendorID,
IsShowInMonthlyReports,SalesManID,PurchaseInvoiceRefNumber)  
OUTPUT INSERTED.Guid  
values (@VoucherDate,@BranchID,@CostCenterID,@BankCostCenterID,@VoucherNumber,@BusinessPartnerID,@TotalAmount,@DownPayment,@NetAmount,
                                                               @Note,@Grantor ,@LoanType,
                                                               @IntrestRate,@isAmountReturned,@MonthsCount,@PaymentAccountID,@PaymentSubAccountID,
                                                               @CompanyID,@CreationUserID,@CreationDate,@VendorID,
@IsShowInMonthlyReports,@SalesManID,@PurchaseInvoiceRefNumber)  ";
                clsSQL clsSQL = new clsSQL();
                string myGuid = Simulate.String(clsSQL.ExecuteScalar(a,  prm, clsSQL.CreateDataBaseConnectionString(DBFinancingHeader.CompanyID), trn));
                return myGuid;

            }
            catch (Exception ex)
            {

                return "";
            }
        }


             public string UpdateFinancingHeaderIsShowInMonthlyReports(string Guid, bool IsShowInMonthlyReports,int CompanyID, SqlTransaction trn)
        {
            try
            {
                clsSQL clsSQL = new clsSQL();

                SqlParameter[] prm =
                   {
                     new SqlParameter("@Guid", SqlDbType.UniqueIdentifier) { Value = new Guid( Guid)},
                    new SqlParameter("@IsShowInMonthlyReports", SqlDbType.Bit) { Value = IsShowInMonthlyReports},


                };
                string a = @" update tbl_FinancingHeader set IsShowInMonthlyReports =@IsShowInMonthlyReports 
where Guid = @Guid
";

                string A = Simulate.String(clsSQL.ExecuteNonQueryStatement(a, clsSQL.CreateDataBaseConnectionString(CompanyID), prm, trn));
                return A;


            }
            catch (Exception ex)
            {

                return "";
            }
        }
        public string UpdateFinancingHeader(DBFinancingHeader DBFinancingHeader, int CompanyID, SqlTransaction trn)
        {
            try
            {
                clsSQL clsSQL = new clsSQL();

                SqlParameter[] prm =
                   {
                    new SqlParameter("@Guid", SqlDbType.UniqueIdentifier) { Value = DBFinancingHeader.Guid },
                    new SqlParameter("@VoucherDate", SqlDbType.DateTime) { Value = DBFinancingHeader.VoucherDate },
                    new SqlParameter("@BranchID", SqlDbType.Int) { Value = DBFinancingHeader.BranchID },
                     new SqlParameter("@CostCenterID", SqlDbType.Int) { Value = DBFinancingHeader.CostCenterID },
                     new SqlParameter("@BankCostCenterID", SqlDbType.Int) { Value = DBFinancingHeader.BankCostCenterID },

                     
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
    new SqlParameter("@IsShowInMonthlyReports", SqlDbType.Bit) { Value = DBFinancingHeader.IsShowInMonthlyReports },
       new SqlParameter("@SalesManID", SqlDbType.Int) { Value = DBFinancingHeader.SalesManID },

              new SqlParameter("@PurchaseInvoiceRefNumber", SqlDbType.NVarChar,-1) { Value = DBFinancingHeader.PurchaseInvoiceRefNumber },
       

                };
                string a = @"update tbl_FinancingHeader set  

 VoucherDate=@VoucherDate,
BranchID=@BranchID,

CostCenterID=@CostCenterID,

BankCostCenterID=@BankCostCenterID,
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
VendorID=@VendorID ,
IsShowInMonthlyReports=@IsShowInMonthlyReports,
SalesManID=@SalesManID,
PurchaseInvoiceRefNumber=@PurchaseInvoiceRefNumber
 where Guid=@guid";

                string A = Simulate.String(clsSQL.ExecuteNonQueryStatement(a, clsSQL.CreateDataBaseConnectionString(CompanyID), prm, trn));
                return A;


            }
            catch (Exception ex)
            {

                return "";
            }
        }
        

public async Task<bool> InsertPurchaseInvoiceHeader( 
   int  branchID , int storeID , int creationUserId ,DateTime invoiceDate,
   int businessPartnerID,string  refNo,
                  string note,  
               int CurrencyID , 
                    string Guid,   int CompanyID,
                    List<DBFinancingDetails> details,
                    SqlTransaction trn)
        {
            try
            {
                clsPaymentMethod clsPaymentMethod = new clsPaymentMethod();
                DataTable dtpaymentmethodid = clsPaymentMethod.SelectPaymentMethodByID(0, "", "", CompanyID, trn);
                int paymentMethodID = 0;
                for (global::System.Int32 i = 0; i < dtpaymentmethodid.Rows.Count; i++)
                {
                    if (Simulate.Bool(dtpaymentmethodid.Rows[i]["IsDebit"])) {


                        paymentMethodID = Simulate.Integer32(dtpaymentmethodid.Rows[i]["ID"]);
                        break;  
                    }
                }
                if (paymentMethodID == 0) {
                    return false;
                }
                int invoiceTypeID = (int)clsEnum.VoucherType.PurchaseInvoiceFromFinancing;

                // clsInvoiceHeader clsInvoiceHeader = new clsInvoiceHeader();
                //DBInvoiceHeader dbInvoiceHeader = new DBInvoiceHeader
                //{
                //    BranchID = Simulate.Integer32(branchID),
                //    StoreID = Simulate.Integer32(storeID),
                //    CompanyID = Simulate.Integer32(CompanyID),
                //    CreationUserID = Simulate.Integer32(creationUserId),
                //    InvoiceDate = Simulate.StringToDate(invoiceDate),
                //    BusinessPartnerID = Simulate.Integer32(businessPartnerID),
                //    CashID = Simulate.Integer32(0),
                //    BankID = Simulate.Integer32(0),
                //    status = Simulate.Integer32(0),
                //    tableID = Simulate.Integer32(0),
                //    CreationDate = DateTime.Now,
                //    RefNo = Simulate.String(refNo),
                //    HeaderDiscount = Simulate.decimal_(headerDiscount),
                //    InvoiceNo = Simulate.Integer32(0),
                //    InvoiceTypeID = Simulate.Integer32(invoiceTypeID),
                //    IsCounted = Simulate.Bool(true),
                //    Note = Simulate.String(note),
                //    TotalTax = Simulate.decimal_(totalTax),
                //    POSDayGuid = Simulate.Guid(""),
                //    RelatedInvoiceGuid = Simulate.Guid(""),
                //    TotalDiscount = Simulate.decimal_(totalDiscount),
                //    PaymentMethodID = Simulate.Integer32(paymentMethodID),
                //    POSSessionGuid = Simulate.Guid(""),
                //    TotalInvoice = Simulate.decimal_(totalInvoice),
                //    AccountID = 0,
                //    Guid = Simulate.Guid(""),
                //    CurrencyID = Simulate.Integer32(CurrencyID),

                //    CurrencyBaseAmount = Simulate.decimal_(CurrencyBaseAmount),
                //    CurrencyRate = Simulate.decimal_(1),

                //};
                // dbInvoiceHeader.InvoiceNo = clsInvoiceHeader.SelectMaxInvoiceNumber(Simulate.Integer32(invoiceTypeID), Simulate.Integer32(branchID), Simulate.Integer32(CompanyID), trn);

                decimal headerDiscount = 0;
                decimal totalDiscount = 0;
                decimal totalInvoice = 0;

                decimal totalTax = 0;
               
                decimal CurrencyBaseAmount = 0;
                for (global::System.Int32 i = 0; i < details.Count; i++)
                {
                    totalTax = totalTax + details[i].TaxAmount;
                    totalInvoice = totalInvoice + details[i].FinancingAmount;
                    CurrencyBaseAmount = CurrencyBaseAmount + details[i].FinancingAmount;
                }

                List<DBInvoiceDetails> detailsList =new  List<DBInvoiceDetails>();
                for (int i = 0; i < details.Count; i++)
                {
                    detailsList.Add(

                          new DBInvoiceDetails
                          {
                              Guid = Simulate.Guid(""),
                              HeaderGuid = Simulate.Guid(""),
                              RowIndex = (1 + i),
                              ItemGuid = Simulate.Guid(""),
                              ItemName = details[i].Description,
                              Qty = 1,
                              PriceBeforeTax = details[i].PriceBeforeTax,
                              DiscountBeforeTaxAmountPcs = 0,
                              DiscountBeforeTaxAmountAll = 0,
                              TaxID = details[i].TaxID,
                              TaxPercentage = details[i].TaxAmount,
                              TaxAmount = details[i].TaxAmount,
                              SpecialTaxID = 0,
                              SpecialTaxPercentage = 0,
                              SpecialTaxAmount =0,
                              PriceAfterTaxPcs = details[i].TotalAmount,
                              DiscountAfterTaxAmountPcs = 0,
                              DiscountAfterTaxAmountAll = 0,
                              HeaderDiscountAfterTaxAmount = 0,
                              HeaderDiscountTax = 0,
                              FreeQty =0,
                              TotalQTY = 1,
                              ServiceBeforeTax = 0,
                              ServiceTaxAmount = 0,
                              LotDetails = "",
                              TrackExpiryDate=false,
                              TrackLot = false,
                              TrackSerial = false,
                              ServiceAfterTax = 0,
                              TotalLine = details[i].TotalAmount,
                              BranchID = branchID,
                              StoreID = storeID,
                              CompanyID = CompanyID,
                              InvoiceTypeID = invoiceTypeID,
                              IsCounted = true,
                              InvoiceDate = invoiceDate,
                              BusinessPartnerID = businessPartnerID,
                              ItemBatchsGuid = Simulate.Guid(""),
                              AVGCostPerUnit = details[i].PriceBeforeTax
                          }
                          
                        );
                }
                string detailsListJson = JsonConvert.SerializeObject(detailsList);

                clsInvoiceHeader clsInvoiceHeader = new clsInvoiceHeader();
            
                var HeaderGuid=await clsInvoiceHeader.InsertInvoiceHeaderWithDetails(branchID, storeID, businessPartnerID
                    , 0, 0, refNo, 0, headerDiscount, invoiceTypeID, true, note
                    , CompanyID, totalTax, "", "", totalDiscount, paymentMethodID
                    , "", totalInvoice, invoiceDate, creationUserId, 0, 0, 0, CurrencyID
                    , CurrencyBaseAmount, 1, detailsListJson,trn);




                if (HeaderGuid != null && HeaderGuid != "")
                {
                    clsSQL clsSQL = new clsSQL();

                    SqlParameter[] prm =
                       {
                     new SqlParameter("@Guid", SqlDbType.UniqueIdentifier) { Value = new Guid( Guid)},
                   new SqlParameter("@InvoiceHeaderGuid", SqlDbType.UniqueIdentifier) { Value = new Guid( HeaderGuid)},


                };
                    string a = @"update tbl_FinancingHeader set  

 InvoiceHeaderGuid=@InvoiceHeaderGuid  
 where Guid=@guid";

                    string A = Simulate.String(clsSQL.ExecuteNonQueryStatement(a, clsSQL.CreateDataBaseConnectionString(CompanyID), prm, trn));


                }

      if (HeaderGuid != "") { 
                    return true;
                }
                else {
                    return false;
                }
            }
            catch (Exception ex)
            {

                return false;
            }
        }

        public string UpdateFinancingHeaderJVGuid(string Guid,string JVGuid, int CompanyID, SqlTransaction trn)
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

                string A = Simulate.String(clsSQL.ExecuteNonQueryStatement(a, clsSQL.CreateDataBaseConnectionString(CompanyID), prm, trn));
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
,tbl_FinancingHeader.purchaseinvoicerefnumber as purchaseinvoicerefnumber
,vendor.AName as VendorAName
,tbl_FinancingHeader.VoucherNumber as VoucherNumber
,tbl_FinancingHeader.VoucherDate as VoucherDate
from tbl_FinancingDetails 
inner join tbl_FinancingHeader on tbl_FinancingHeader.Guid = tbl_FinancingDetails.HeaderGuid
inner join tbl_Branch on tbl_FinancingHeader.BranchID = tbl_Branch.ID
left join tbl_BusinessPartner on tbl_FinancingHeader.BusinessPartnerID = tbl_BusinessPartner.ID
left join tbl_employee on tbl_FinancingHeader.CreationUserID = tbl_employee.ID
left join tbl_BusinessPartner vendor on tbl_FinancingHeader.VendorID = vendor.ID
where (BranchID = @branchID or @branchid=0)and
(tbl_FinancingHeader.CompanyID = @CompanyId or @CompanyId=0) " + UsersFilter + @" and
cast( tbl_FinancingHeader.VoucherDate as date) between cast (@date1 as date) and cast (@date2 as date)   ", clsSQL.CreateDataBaseConnectionString(CompanyID), prm, trn);

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
        public int CostCenterID { get; set; }
        public int BankCostCenterID { get; set; }
        
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
        
        public bool IsShowInMonthlyReports { get; set; }
        public int SalesManID { get; set; }
        public Guid? InvoiceHeaderGuid { get; set; }
        public String PurchaseInvoiceRefNumber { get; set; }
          

    }
}
 
