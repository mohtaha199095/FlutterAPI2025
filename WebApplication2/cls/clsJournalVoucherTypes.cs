using System;
using System.Data;
using System.Data.SqlClient;

namespace WebApplication2.cls
{
    public class clsJournalVoucherTypes
    {
        public DataTable SelectJournalVoucherTypes(int ID)
        {
            try
            {
                clsSQL clsSQL = new clsSQL();

                SqlParameter[] prm =
                 {

     new SqlParameter("@ID", SqlDbType.Int) { Value =  ID },

                };

                string a = @"select * from tbl_JournalVoucherTypes where   (id=@id or @ID=0) ";
                DataTable dt = clsSQL.ExecuteQueryStatement(a, prm);

                return dt;
            }
            catch (Exception ex)
            {

                throw ex;
            }


        }
    }
}
