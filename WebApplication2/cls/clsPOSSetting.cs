using System;
using System.Data;
using Microsoft.Data.SqlClient;
namespace WebApplication2.cls
{
    public class clsPOSSetting
    {

        public DataTable SelectPOSSettingByID(int Id, int CashDrawerID, int POSSettingID, int CompanyID)
        {
            try
            {
                clsSQL clsSQL = new clsSQL();

                SqlParameter[] prm =
                 { new SqlParameter("@Id", SqlDbType.Int) { Value = Id },
                 new SqlParameter("@CashDrawerID", SqlDbType.Int) { Value = CashDrawerID },
    new SqlParameter("@POSSettingID", SqlDbType.Int) { Value = POSSettingID },
        new SqlParameter("@CompanyID", SqlDbType.Int) { Value = CompanyID },

                };
                DataTable dt = clsSQL.ExecuteQueryStatement(@"select * from tbl_POSSetting where (id=@Id or @Id=0 ) and  
                     (CashDrawerID=@CashDrawerID or @CashDrawerID=0 ) and (POSSettingID=@POSSettingID or @POSSettingID=0 )   and (CompanyID=@CompanyID or @CompanyID=0 )
                  order by creationdate desc   ", clsSQL.CreateDataBaseConnectionString(CompanyID), prm);

                return dt;
            }
            catch (Exception)
            {

                throw;
            }


        }

        public bool DeletePOSSettingByID(int Id, int CompanyID)
        {
            try
            {
                clsSQL clsSQL = new clsSQL();

                SqlParameter[] prm =
                 { new SqlParameter("@Id", SqlDbType.Int) { Value = Id },
                 new SqlParameter("@CompanyID", SqlDbType.Int) { Value = CompanyID },

                };
                int A = clsSQL.ExecuteNonQueryStatement(@"delete from tbl_POSSetting where (id=@Id or @id=0 ) and (CompanyID=@CompanyID or @CompanyID=0 )", clsSQL.CreateDataBaseConnectionString(CompanyID), prm);

                return true;
            }
            catch (Exception)
            {

                throw;
            }


        }
        public int InsertPOSSetting(int CashDrawerID, int POSSettingID, string Value, int CompanyID, int CreationUserId)
        {
            try
            {
                SqlParameter[] prm =
                 {
                    new SqlParameter("@CashDrawerID", SqlDbType.Int) { Value = CashDrawerID },
                  new SqlParameter("@POSSettingID", SqlDbType.Int) { Value = POSSettingID },
                    new SqlParameter("@Value", SqlDbType.NVarChar,-1) { Value = Value },


                  new SqlParameter("@CompanyID", SqlDbType.Int) { Value = CompanyID },
                   new SqlParameter("@CreationUserId", SqlDbType.Int) { Value = CreationUserId },
                     new SqlParameter("@CreationDate", SqlDbType.DateTime) { Value = DateTime.Now },


                   
                };

                string a = @"insert into tbl_POSSetting(CashDrawerID,POSSettingID,Value,CompanyID,CreationUserId,CreationDate)
                    OUTPUT INSERTED.ID values(@CashDrawerID,@POSSettingID,@Value,@CompanyID,@CreationUserId,@CreationDate)";
                clsSQL clsSQL = new clsSQL();
                return Simulate.Integer32(clsSQL.ExecuteScalar(a, prm, clsSQL.CreateDataBaseConnectionString(CompanyID)));

            }
            catch (Exception)
            {

                throw;
            }


        }
        public int UpdatePOSSetting(int ID, int CashDrawerID, int POSSettingID, string Value, int ModificationUserId,int CompanyID
            )
        {
            try
            {
                clsSQL clsSQL = new clsSQL();

                SqlParameter[] prm =
                 {
                     new SqlParameter("@ID", SqlDbType.Int) { Value = ID },
              new SqlParameter("@CashDrawerID", SqlDbType.Int) { Value = CashDrawerID },
                  new SqlParameter("@POSSettingID", SqlDbType.Int) { Value = POSSettingID },
                    new SqlParameter("@Value", SqlDbType.NVarChar,-1) { Value = Value },


                         new SqlParameter("@ModificationUserId", SqlDbType.Int) { Value = ModificationUserId },
                     new SqlParameter("@ModificationDate", SqlDbType.DateTime) { Value = DateTime.Now },


      

                };
                int A = clsSQL.ExecuteNonQueryStatement(@"update tbl_POSSetting set 
                       CashDrawerID=@CashDrawerID,
                       POSSettingID=@POSSettingID,
Value=@Value,


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
