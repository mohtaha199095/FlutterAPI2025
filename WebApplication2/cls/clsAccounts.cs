using System;
using System.Data;
using Microsoft.Data.SqlClient;

namespace WebApplication2.cls
{
    public class clsAccounts
    {
        public DataTable SelectAccountsByID(int Id, int ParentID, string AccountNumber, string AName, string EName, int CompanyID)
        {
            try
            {
                clsSQL clsSQL = new clsSQL();

                SqlParameter[] prm =
                 { new SqlParameter("@Id", SqlDbType.Int) { Value = Id },new SqlParameter("@ParentID", SqlDbType.Int) { Value = ParentID },
                    new SqlParameter("@AccountNumber", SqlDbType.NVarChar,-1) { Value = AccountNumber },
      new SqlParameter("@AName", SqlDbType.NVarChar,-1) { Value = AName },
       new SqlParameter("@EName", SqlDbType.NVarChar,-1) { Value = EName },
     new SqlParameter("@CompanyID", SqlDbType.Int) { Value = CompanyID },

     

                };
                DataTable dt = clsSQL.ExecuteQueryStatement(@"select * from tbl_Accounts where (id=@Id or @Id=0 )and (ParentID=@ParentID or @ParentID=0 ) and  
                     (AName=@AName or @AName='' ) and (EName=@EName or @EName='' ) and (AccountNumber like N'@AccountNumber%' or @AccountNumber='' )  and (CompanyID=@CompanyID or @CompanyID=0 ) order by AccountNumber
                     ", clsSQL.CreateDataBaseConnectionString(CompanyID), prm);

                return dt;
            }
            catch (Exception)
            {

                throw;
            }


        }

        public DataTable SelectTransactionAccounts(int CompanyID)
        {
            try
            {
                clsSQL clsSQL = new clsSQL();

                SqlParameter[] prm =
                 {

     new SqlParameter("@CompanyID", SqlDbType.Int) { Value = CompanyID },

                };

                string a = @"select * from tbl_accounts where id not in (select ParentID from tbl_Accounts) and (CompanyID=@CompanyID or @CompanyID=0) order by AccountNumber";
                DataTable dt = clsSQL.ExecuteQueryStatement(a, clsSQL.CreateDataBaseConnectionString(CompanyID), prm);

                return dt;
            }
            catch (Exception ex)
            {

                throw ex;
            }


        }

        public bool DeleteAccountsByID(int Id, int CompanyID)
        {
            try
            {
                SqlParameter[] prm =
                 { new SqlParameter("@Id", SqlDbType.Int) { Value = Id },

                }; clsSQL clsSQL = new clsSQL();
                int A = clsSQL.ExecuteNonQueryStatement(@"delete from tbl_Accounts where (id=@Id  )", clsSQL.CreateDataBaseConnectionString(CompanyID), prm);

                return true;
            }
            catch (Exception)
            {

                throw;
            }


        }
        public int InsertAccounts(int ParentID, string AccountNumber, string AName, string EName, int ReportingTypeID,int ReportingTypeNodeID, int AccountNatureID, int CompanyID, int CreationUserId, bool IsSubLedger)
        {
            try
            {
                SqlParameter[] prm =
                 { new SqlParameter("@ParentID", SqlDbType.Int) { Value = ParentID },
                   new SqlParameter("@AccountNumber", SqlDbType.NVarChar,-1) { Value = AccountNumber },
                    new SqlParameter("@AName", SqlDbType.NVarChar,-1) { Value = AName },
                  new SqlParameter("@EName", SqlDbType.NVarChar,-1) { Value = EName },
                  new SqlParameter("@ReportingTypeID", SqlDbType.Int) { Value = ReportingTypeID },
                      new SqlParameter("@ReportingTypeNodeID", SqlDbType.Int) { Value = ReportingTypeNodeID },
                  
                   new SqlParameter("@AccountNatureID", SqlDbType.Int) { Value = AccountNatureID },
                     new SqlParameter("@CompanyID", SqlDbType.Int) { Value = CompanyID },
                   new SqlParameter("@CreationUserId", SqlDbType.Int) { Value = CreationUserId },
                     new SqlParameter("@CreationDate", SqlDbType.DateTime) { Value = DateTime.Now },
                           new SqlParameter("@IsSubLedger", SqlDbType.Bit) { Value = IsSubLedger },
                };

                string a = @"insert into tbl_Accounts(ParentID,AccountNumber,AName,EName,ReportingTypeID,ReportingTypeNodeID,AccountNatureID,CompanyID,CreationUserId,CreationDate,IsSubLedger)
                           OUTPUT INSERTED.ID values(@ParentID,@AccountNumber,@AName,@EName,@ReportingTypeID,@ReportingTypeNodeID,@AccountNatureID,@CompanyID,@CreationUserId,@CreationDate,@IsSubLedger)";
                clsSQL clsSQL = new clsSQL();
                return Simulate.Integer32(clsSQL.ExecuteScalar(a, prm, clsSQL.CreateDataBaseConnectionString(CompanyID)));

            }
            catch (Exception)
            {

                throw;
            }


        }
        public int UpdateAccounts(int ID, int ParentID, string AccountNumber, string AName, string EName, int ReportingTypeID,int ReportingTypeNodeID, int AccountNatureID, int ModificationUserId,int CompanyID, bool IsSubLedger)
        {
            try
            {
                SqlParameter[] prm =
                 {
                     new SqlParameter("@ID", SqlDbType.Int) { Value = ID },
               new SqlParameter("@ParentID", SqlDbType.Int) { Value = ParentID },
                   new SqlParameter("@AccountNumber", SqlDbType.NVarChar,-1) { Value = AccountNumber },
                    new SqlParameter("@AName", SqlDbType.NVarChar,-1) { Value = AName },
                  new SqlParameter("@EName", SqlDbType.NVarChar,-1) { Value = EName },
                  new SqlParameter("@ReportingTypeID", SqlDbType.Int) { Value = ReportingTypeID },
                      new SqlParameter("@ReportingTypeNodeID", SqlDbType.Int) { Value = ReportingTypeNodeID },
                  
                   new SqlParameter("@AccountNatureID", SqlDbType.Int) { Value = AccountNatureID },

                         new SqlParameter("@ModificationUserId", SqlDbType.Int) { Value = ModificationUserId },
                     new SqlParameter("@ModificationDate", SqlDbType.DateTime) { Value = DateTime.Now },
                           new SqlParameter("@IsSubLedger", SqlDbType.Bit) { Value = IsSubLedger },
                }; clsSQL clsSQL = new clsSQL();
                int A = clsSQL.ExecuteNonQueryStatement(@"update tbl_Accounts set 
ParentID=@ParentID,
AccountNumber=@AccountNumber,
                       AName=@AName,
                       EName=@EName,
ReportingTypeID=@ReportingTypeID,
ReportingTypeNodeID=@ReportingTypeNodeID,
IsSubLedger=@IsSubLedger,
AccountNatureID=@AccountNatureID,
                       ModificationDate=@ModificationDate,
                       ModificationUserId=@ModificationUserId
                   where id =@id", clsSQL.CreateDataBaseConnectionString(CompanyID), prm);

                return A;
            }
            catch (Exception)
            {

                throw;
            }


        }
    }
}
