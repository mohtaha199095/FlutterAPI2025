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
                };

                string a = @"insert into tbl_FinancingHeader (VoucherDate,BranchID,VoucherNumber,BusinessPartnerID,TotalAmount,DownPayment,NetAmount,
                                                                Note,Grantor, LoanType,
                                                               IntrestRate,isAmountReturned,MonthsCount,
                                                               CompanyID,CreationUserID,CreationDate)  
OUTPUT INSERTED.Guid  
values (@VoucherDate,@BranchID,@VoucherNumber,@BusinessPartnerID,@TotalAmount,@DownPayment,@NetAmount,
                                                               @Note,@Grantor ,@LoanType,
                                                               @IntrestRate,@isAmountReturned,@MonthsCount,
                                                               @CompanyID,@CreationUserID,@CreationDate)  ";
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
MonthsCount=@MonthsCount
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

    }
}
 
