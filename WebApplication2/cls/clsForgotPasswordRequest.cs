using System.Data.SqlClient;
using System.Data;
using System;
using static WebApplication2.MainClasses.clsEnum;

namespace WebApplication2.cls
{
    public class clsForgotPasswordRequest
    {
        public DataTable SelectForgotPasswordRequest(string Email, string GeneratedPassword,int CompanyId)
        {
            try
            {
                SqlParameter[] prm =
                 { 
                    new SqlParameter("@Email", SqlDbType.NVarChar,-1) { Value = Email },
    new SqlParameter("@GeneratedPassword", SqlDbType.NVarChar,-1) { Value = GeneratedPassword },
     new SqlParameter("@creationdate", SqlDbType.DateTime) { Value = DateTime.Now.AddMinutes(-15) },
                }; 
                clsSQL clsSQL = new clsSQL();
                DataTable dt = clsSQL.ExecuteQueryStatement(@"select * from tbl_ForgotPasswordRequest
where ( Email=@Email or @Email= '')  
and ( GeneratedPassword=@GeneratedPassword or @GeneratedPassword= '') 
and (creationdate> @creationdate)


", clsSQL.CreateDataBaseConnectionString(CompanyId), prm);

                return dt;
            }
            catch (Exception)
            {

                throw;
            }


        }

        public int InsertForgotPasswordRequest(int CompanyId, string Email, string Tel1,string GeneratedPassword, int EmployeeID)
        {
            try
            {
                SqlParameter[] prm =
                 {
                  
                new SqlParameter("@CompanyId", SqlDbType.Int) { Value = CompanyId },
                  new SqlParameter("@Email", SqlDbType.NVarChar,-1) { Value = Email },
                                    new SqlParameter("@Tel1", SqlDbType.NVarChar,-1) { Value = Tel1 }, 
                    new SqlParameter("@GeneratedPassword", SqlDbType.NVarChar,-1) { Value = GeneratedPassword },
 new SqlParameter("@EmployeeID", SqlDbType.Int) { Value = EmployeeID },
                };

                string a = @"insert into tbl_ForgotPasswordRequest(CompanyId,Email,Tel1,GeneratedPassword,EmployeeID)
                         values(@CompanyId,@Email,@Tel1,@GeneratedPassword,@EmployeeID)";
                clsSQL clsSQL = new clsSQL();
                return Simulate.Integer32(clsSQL.ExecuteScalar(a, prm, clsSQL.CreateDataBaseConnectionString(CompanyId)));

            }
            catch (Exception)
            {

                throw;
            }


        }
    }
}
