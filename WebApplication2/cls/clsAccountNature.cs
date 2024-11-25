using System;
using System.Data;
using System.Data.SqlClient;

namespace WebApplication2.cls
{
    public class clsAccountNature
    {
        public DataTable SelectAccountNatureByID(int Id, int CompanyID)
        {
            try
            {
                SqlParameter[] prm =
                 { new SqlParameter("@Id", SqlDbType.Int) { Value = Id },

     new SqlParameter("@CompanyID", SqlDbType.Int) { Value = CompanyID },

                }; clsSQL clsSQL = new clsSQL();
                DataTable dt = clsSQL.ExecuteQueryStatement(@"select * from tbl_AccountNature where (id=@Id or @Id=0 )   and (CompanyID=@CompanyID or @CompanyID=0 )   ", clsSQL.CreateDataBaseConnectionString(CompanyID), prm);

                return dt;
            }
            catch (Exception)
            {

                throw;
            }


        }
    }
}
