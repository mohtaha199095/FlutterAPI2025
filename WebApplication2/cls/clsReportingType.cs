using System;
using System.Data;
using System.Data.SqlClient;

namespace WebApplication2.cls
{
    public class clsReportingType
    {
        public DataTable SelectReportingTypeByID(int Id, int CompanyID)
        {
            try
            {
                clsSQL clsSQL = new clsSQL();

                SqlParameter[] prm =
                 { new SqlParameter("@Id", SqlDbType.Int) { Value = Id },

     new SqlParameter("@CompanyID", SqlDbType.Int) { Value = CompanyID },

                };
                DataTable dt = clsSQL.ExecuteQueryStatement(@"select * from tbl_ReportingType where (id=@Id or @Id=0 )   and (CompanyID=@CompanyID or @CompanyID=0 )   ", clsSQL.CreateDataBaseConnectionString(CompanyID), prm);

                return dt;
            }
            catch (Exception)
            {

                throw;
            }


        }
    }
}
