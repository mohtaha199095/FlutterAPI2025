using Newtonsoft.Json;
using System.Data.SqlClient;
using System.Data;
using System;

namespace WebApplication2.cls
{
    public class clsSubscriptionsStatus
    {
        public DataTable SelectSubscriptionsStatus(int ID, int CompanyID)
        {
            try
            {
                clsSQL clsSQL = new clsSQL();

                SqlParameter[] prm ={
                       new SqlParameter("@id", SqlDbType.Int) { Value = ID },
                       new SqlParameter("@CompanyID", SqlDbType.Int) { Value = CompanyID }
                };
                DataTable dt = clsSQL.ExecuteQueryStatement("select * from tbl_SubscriptionsStatus where (id=@id or @id=0) and (companyid=@companyid or  @companyid in (0,2))", clsSQL.CreateDataBaseConnectionString(CompanyID), prm);
                return dt;
            }
            catch (Exception)
            {

                throw;
            }
        }
    }
}
