using System.Data.SqlClient;
using System.Data;
using System;
using DocumentFormat.OpenXml.Presentation;

namespace WebApplication2.cls
{
    public class clsReconciliation
    {
        public DataTable SelectReconciliationByJVDetailsGuid(int VoucherNumber,string JVDetailsGuid, int CompanyID)
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
                     ", prm);

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
                DataTable dt = clsSQL.ExecuteQueryStatement(@"select  tbl_JournalVoucherDetails.Guid,
tbl_JournalVoucherTypes.AName,duedate,note,Total 
,(select count(JVDetailsGuid)from tbl_Reconciliation where JVDetailsGuid=tbl_JournalVoucherDetails.Guid ) as IsReconciled
from tbl_JournalVoucherDetails
inner join tbl_JournalVoucherHeader on tbl_JournalVoucherDetails.ParentGuid=tbl_JournalVoucherHeader.Guid
inner join tbl_JournalVoucherTypes on tbl_JournalVoucherTypes.ID=tbl_JournalVoucherHeader.JVTypeID
where( AccountID = @AccountID or @AccountID=0)
and( tbl_JournalVoucherHeader.CompanyID =  @companyID )
and( tbl_JournalVoucherDetails.SubAccountID =  @SubAccountID or @SubAccountID=0 )
and ((select count(JVDetailsGuid)from tbl_Reconciliation where JVDetailsGuid=tbl_JournalVoucherDetails.Guid )=0
or(select count(JVDetailsGuid)from tbl_Reconciliation where JVDetailsGuid=tbl_JournalVoucherDetails.Guid and tbl_Reconciliation.TransactionGuid=@TransactionGuid)>0
)
                     ", prm);

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
