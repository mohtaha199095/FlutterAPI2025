using System;
using System.Data;
using System.Data.SqlClient;

namespace WebApplication2.cls
{
    public class clsJournalVoucherDetails
    {
        public DataTable SelectJournalVoucherDetailsByParentId(string ParentGuid, int AccountId, int SubAccountid, SqlTransaction trn = null)
        {
            try
            {
                clsSQL clsSQL = new clsSQL();

                SqlParameter[] prm =
                 { new SqlParameter("@ParentGuid", SqlDbType.UniqueIdentifier) { Value = Simulate.Guid( ParentGuid ) },
    new SqlParameter("@accountid", SqlDbType.Int) { Value = AccountId },    new SqlParameter("@SubAccountid", SqlDbType.Int) { Value = SubAccountid },
                };
                DataTable dt = clsSQL.ExecuteQueryStatement(@"select * from tbl_JournalVoucherDetails where (accountid=@accountid or @accountid=0 ) and (SubAccountid=@SubAccountid or @SubAccountid=0 ) and (ParentGuid=@ParentGuid or @ParentGuid='00000000-0000-0000-0000-000000000000' )", prm, trn);

                return dt;
            }
            catch (Exception ex)
            {

                throw;
            }


        }

        public bool DeleteJournalVoucherDetailsByParentId(string ParentGuid, SqlTransaction trn)
        {
            try
            {
                clsSQL clsSQL = new clsSQL();

                SqlParameter[] prm =
                 { new SqlParameter("@ParentGuid", SqlDbType.UniqueIdentifier) { Value =  Simulate.Guid( ParentGuid ) },

                };
                int A = clsSQL.ExecuteNonQueryStatement(@"delete from tbl_JournalVoucherDetails where (ParentGuid=@ParentGuid  )", prm, trn);

                return true;
            }
            catch (Exception)
            {

                throw;
            }


        }
        public string InsertJournalVoucherDetails(string ParentGuid, int AccountID, int SubAccountID,
            decimal Debit,
            decimal Credit,
            decimal Total,
            int BranchID,
            int CostCenterID,
            DateTime DueDate,
            string Note,
            int CompanyID,
                      int CreationUserId, SqlTransaction trn = null)
        {
            try
            {
                SqlParameter[] prm =
                 {
                     new SqlParameter("@ParentGuid", SqlDbType.UniqueIdentifier) { Value =  Simulate.Guid( ParentGuid ) },
                      new SqlParameter("@AccountID", SqlDbType.Int) { Value =  AccountID },
                       new SqlParameter("@SubAccountID", SqlDbType.Int) { Value = SubAccountID },
                      new SqlParameter("@Debit", SqlDbType.Decimal) { Value = Debit },
                       new SqlParameter("@Credit", SqlDbType.Decimal) { Value = Credit },
                        new SqlParameter("@Total", SqlDbType.Decimal) { Value = Total },
                          new SqlParameter("@BranchID", SqlDbType.Int) { Value = BranchID },
                           new SqlParameter("@CostCenterID", SqlDbType.Int) { Value = CostCenterID },
                             new SqlParameter("@DueDate", SqlDbType.DateTime) { Value = DueDate },
                               new SqlParameter("@Note", SqlDbType.NVarChar,-1) { Value = Note },
                                new SqlParameter("@CompanyID", SqlDbType.Int) { Value = CompanyID },
                                 new SqlParameter("@CreationUserId", SqlDbType.Int) { Value = CreationUserId },
                                  new SqlParameter("@CreationDate", SqlDbType.DateTime) { Value = DateTime.Now },
                };
                clsSQL clsSQL = new clsSQL();
                string a = @"insert into tbl_JournalVoucherDetails(ParentGuid,AccountID,SubAccountID,Debit,Credit,Total,BranchID,CostCenterID,DueDate,Note,CompanyID,CreationUserId,CreationDate) 
                                       OUTPUT INSERTED.Guid values(@ParentGuid,@AccountID,@SubAccountID,@Debit,@Credit,@Total,@BranchID,@CostCenterID,@DueDate,@Note,@CompanyID,@CreationUserId,@CreationDate)";
                if (trn == null)
                    return Simulate.String(clsSQL.ExecuteScalar(a, prm));
                else
                    return Simulate.String(clsSQL.ExecuteScalar(a, prm, trn));

            }
            catch (Exception)
            {

                throw;
            }


        }

    }
}
