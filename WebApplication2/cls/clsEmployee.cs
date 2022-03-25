using System;
using System.Data;
using System.Data.SqlClient;

namespace WebApplication2.cls
{
    public class clsEmployee
    {
        public DataTable SelectEmployee(int Id, string AName, string EName, string UserName, string Password, int CompanyId)
        {
            try
            {
                SqlParameter[] prm =
                 { new SqlParameter("@Id", SqlDbType.Int) { Value = Id },
      new SqlParameter("@AName", SqlDbType.NVarChar,-1) { Value = AName },
       new SqlParameter("@EName", SqlDbType.NVarChar,-1) { Value = EName },
           new SqlParameter("@UserName", SqlDbType.NVarChar,-1) { Value = UserName },
                new SqlParameter("@Password", SqlDbType.NVarChar,-1) { Value = Password },new SqlParameter("@CompanyId", SqlDbType.Int) { Value = CompanyId },
                }; clsSQL clsSQL = new clsSQL();
                DataTable dt = clsSQL.ExecuteQueryStatement(@"select * from tbl_employee where (id=@Id or @Id=0 ) and  
                     (AName=@AName or @AName='' ) and (EName=@EName or @EName='' ) and (UserName=@UserName or @UserName='' ) 
                      and  (Password=@Password or @Password='' ) and (CompanyId=@CompanyId or @CompanyId=0 ) ", prm);

                return dt;
            }
            catch (Exception)
            {

                throw;
            }


        }

        public bool DeleteEmployeeByID(int Id)
        {
            try
            {
                clsSQL clsSQL = new clsSQL();

                SqlParameter[] prm =
                 { new SqlParameter("@Id", SqlDbType.Int) { Value = Id },

                };
                int A = clsSQL.ExecuteNonQueryStatement(@"delete from tbl_employee where (id=@Id  )", prm);

                return true;
            }
            catch (Exception)
            {

                throw;
            }


        }
        public int InsertEmployee(string AName, string EName, string UserName, string Password, int CompanyID, int CreationUserId)
        {
            try
            {
                SqlParameter[] prm =
                 { new SqlParameter("@AName", SqlDbType.NVarChar,-1) { Value = AName },
                  new SqlParameter("@EName", SqlDbType.NVarChar,-1) { Value = EName },
                    new SqlParameter("@UserName", SqlDbType.NVarChar,-1) { Value = UserName },
                    new SqlParameter("@Password", SqlDbType.NVarChar,-1) { Value = Password },
                     new SqlParameter("@CompanyID", SqlDbType.Int) { Value = CompanyID },
                       new SqlParameter("@CreationUserId", SqlDbType.Int) { Value = CreationUserId },
                     new SqlParameter("@CreationDate", SqlDbType.DateTime) { Value = DateTime.Now },
                };

                string a = @"insert into tbl_employee(AName,EName,UserName,Password,CompanyID,CreationUserId,CreationDate)  OUTPUT INSERTED.ID values(@AName,@EName,@UserName,@Password,@CompanyID,@CreationUserId,@CreationDate)";
                clsSQL clsSQL = new clsSQL();
                return Simulate.Integer32(clsSQL.ExecuteScalar(a, prm));

            }
            catch (Exception)
            {

                throw;
            }


        }
        public int UpdateEmployee(string AName, string EName, string UserName, string Password, int ID, int ModificationUserId)
        {
            try
            {
                clsSQL clsSQL = new clsSQL();

                SqlParameter[] prm =
                 { new SqlParameter("@AName", SqlDbType.NVarChar,-1) { Value = AName },
                  new SqlParameter("@EName", SqlDbType.NVarChar,-1) { Value = EName },
                    new SqlParameter("@UserName", SqlDbType.NVarChar,-1) { Value = UserName },
                    new SqlParameter("@Password", SqlDbType.NVarChar,-1) { Value = Password },
                     new SqlParameter("@ID", SqlDbType.Int) { Value = ID },
                           new SqlParameter("@ModificationUserId", SqlDbType.Int) { Value = ModificationUserId },
                     new SqlParameter("@ModificationDate", SqlDbType.DateTime) { Value = DateTime.Now },
                };
                int A = clsSQL.ExecuteNonQueryStatement(@"update tbl_employee set AName=@AName,EName=@EName,UserName=@UserName,Password=@Password,ModificationDate=@ModificationDate,ModificationUserId=@ModificationUserId where id =@id", prm);

                return A;
            }
            catch (Exception)
            {

                throw;
            }


        }
    }
}
