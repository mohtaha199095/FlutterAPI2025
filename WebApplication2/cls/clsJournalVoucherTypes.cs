using System;
using System.Data;
using System.Data.SqlClient;

namespace WebApplication2.cls
{
    public class clsJournalVoucherTypes
    {
        public DataTable SelectJournalVoucherTypes(int type)
        {
            try
            {
                clsSQL clsSQL = new clsSQL();

                SqlParameter[] prm =
                 {

     new SqlParameter("@type", SqlDbType.Int) { Value = type },

                };

                string a = @"select * from tbl_JournalVoucherTypes where   (Type=@type or @type=0) ";
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
