using System;
using System.Data;
using Microsoft.Data.SqlClient;

namespace WebApplication2.cls
{
    public class clsJournalVoucherTypes
    {
        public DataTable SelectJournalVoucherTypes(int ID, int CompanyID)
        {
            try
            {
                clsSQL clsSQL = new clsSQL();

                SqlParameter[] prm =
                 {

     new SqlParameter("@ID", SqlDbType.Int) { Value =  ID },

                };

                string a = @"select * from tbl_JournalVoucherTypes where   (id=@id or @ID=0) ";
                DataTable dt = clsSQL.ExecuteQueryStatement(a, clsSQL.CreateDataBaseConnectionString(CompanyID), prm);

                return dt;
            }
            catch (Exception ex)
            {

                throw ex;
            }


        }
        public string Inserttbl_JournalVoucherTypes(int ID,String AName, String EName,int QTYFactor, int CompanyID)
        {
            try
            {
                SqlParameter[] prm =
                   { 
  new SqlParameter("@ID", SqlDbType.Int) { Value = ID },
                    new SqlParameter("@AName", SqlDbType.NVarChar,-1 ) {Value = AName },
                     new SqlParameter("@EName", SqlDbType.NVarChar,-1) { Value = EName },
                      new SqlParameter("@QTYFactor", SqlDbType.Int) { Value = QTYFactor },
                };
                
                string a = @" SET IDENTITY_INSERT tbl_JournalVoucherTypes ON;
insert into tbl_JournalVoucherTypes 
(ID,AName,EName,QTYFactor )  
  
values (@ID,@AName,@EName,@QTYFactor );   SET IDENTITY_INSERT tbl_JournalVoucherTypes OFF; ";
                clsSQL clsSQL = new clsSQL();
                string MyID = Simulate.String(clsSQL.ExecuteScalar(a, prm, clsSQL.CreateDataBaseConnectionString( CompanyID)));
                return MyID;

            }
            catch (Exception ex)
            {

                return "";
            }
        }
    }
}
