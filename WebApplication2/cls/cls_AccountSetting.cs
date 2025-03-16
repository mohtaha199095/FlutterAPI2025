using System;
using System.Data;
using Microsoft.Data.SqlClient;

namespace WebApplication2.cls
{
    public class cls_AccountSetting
    {
        public DataTable SelectAccountSetting(int Id, int AccountRefID, int CompanyID, SqlTransaction trn = null)
        {
            try
            {
                SqlParameter[] prm =
                 { new SqlParameter("@Id", SqlDbType.Int) { Value = Id },
      new SqlParameter("@AccountRefID", SqlDbType.Int) { Value = AccountRefID },

        new SqlParameter("@CompanyID", SqlDbType.Int) { Value = CompanyID },

                }; clsSQL clsSQL = new clsSQL();

                DataTable dt = clsSQL.ExecuteQueryStatement(@"select * from tbl_AccountSetting where (active=1) and  (id=@Id or @Id=0 ) and (AccountRefID=@AccountRefID or @AccountRefID=0 ) 
                       and (CompanyID=@CompanyID or @CompanyID=0 )
                     ", clsSQL.CreateDataBaseConnectionString(CompanyID), prm, trn);

                return dt;
            }
            catch (Exception)
            {

                throw;
            }


        }

        public bool DeleteAccountSettingByID(int Id, int CompanyID)
        {
            try
            {
                clsSQL clsSQL = new clsSQL();

                SqlParameter[] prm =
                 { new SqlParameter("@Id", SqlDbType.Int) { Value = Id },
                  new SqlParameter("@CompanyID", SqlDbType.Int) { Value = CompanyID },
                };
                int A = clsSQL.ExecuteNonQueryStatement(@"delete from tbl_AccountSetting where (id=@Id  ) and (CompanyID=@CompanyID or @CompanyID=0)", clsSQL.CreateDataBaseConnectionString(CompanyID), prm);

                return true;
            }
            catch (Exception)
            {

                throw;
            }


        }
        public bool DeActivateAccountSettingByID(int Id, int AccountRefID, int CompanyID)
        {
            try
            {
                clsSQL clsSQL = new clsSQL();

                SqlParameter[] prm =
                 { new SqlParameter("@Id", SqlDbType.Int) { Value = Id },   new SqlParameter("@AccountRefID", SqlDbType.Int) { Value = AccountRefID },
                  new SqlParameter("@CompanyID", SqlDbType.Int) { Value = CompanyID },
                };
                int A = clsSQL.ExecuteNonQueryStatement(@"update  tbl_AccountSetting set active=0 where (id=@Id or @Id=0 ) and(AccountRefID=@AccountRefID or @AccountRefID=0 ) and (CompanyID=@CompanyID or @CompanyID=0)", clsSQL.CreateDataBaseConnectionString(CompanyID), prm);

                return true;
            }
            catch (Exception)
            {

                throw;
            }


        }
        public int InsertAccountSetting(int AccountRefID, int AccountID, int CompanyID, int CreationUserId)
        {
            try
            {
                SqlParameter[] prm =
                 { new SqlParameter("@AccountRefID", SqlDbType.Int) { Value = AccountRefID },
                  new SqlParameter("@AccountID", SqlDbType.Int) { Value = AccountID },
                                    new SqlParameter("@Active", SqlDbType.Bit) { Value = true },

                  new SqlParameter("@CompanyID", SqlDbType.Int) { Value = CompanyID },
                   new SqlParameter("@CreationUserId", SqlDbType.Int) { Value = CreationUserId },
                     new SqlParameter("@CreationDate", SqlDbType.DateTime) { Value = DateTime.Now },
                };

                string a = @"insert into tbl_AccountSetting(AccountRefID,AccountID,Active,CompanyID,CreationUserId,CreationDate)
                        OUTPUT INSERTED.ID values(@AccountRefID,@AccountID,@Active,@CompanyID,@CreationUserId,@CreationDate)";
                clsSQL clsSQL = new clsSQL();
                return Simulate.Integer32(clsSQL.ExecuteScalar(a, prm, clsSQL.CreateDataBaseConnectionString(CompanyID)));

            }
            catch (Exception)
            {

                throw;
            }


        }

    }
}
