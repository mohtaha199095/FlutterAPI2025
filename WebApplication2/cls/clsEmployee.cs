using System;
using System.Data;
using Microsoft.Data.SqlClient;
using System.Security.Cryptography.Xml;

namespace WebApplication2.cls
{
    public class clsEmployee
    {
        public DataTable SelectEmployee(int Id, string AName, string EName, string UserName, string Password,string Email, string Tel1,int CompanyId, int IsSystemUser)
        {
            try
            {
                SqlParameter[] prm =
                 { new SqlParameter("@Id", SqlDbType.Int) { Value = Id },
      new SqlParameter("@AName", SqlDbType.NVarChar,-1) { Value = AName },
       new SqlParameter("@EName", SqlDbType.NVarChar,-1) { Value = EName },
           new SqlParameter("@UserName", SqlDbType.NVarChar,-1) { Value = UserName },
                new SqlParameter("@Password", SqlDbType.NVarChar,-1) { Value = Password },
                    new SqlParameter("@CompanyId", SqlDbType.Int) { Value = CompanyId },
                      new SqlParameter("@IsSystemUser", SqlDbType.Int) { Value = IsSystemUser },
                               new SqlParameter("@Email",SqlDbType.NVarChar,-1) { Value = Email },
                      new SqlParameter("@Tel1", SqlDbType.NVarChar,-1) { Value = Tel1 },

                }; clsSQL clsSQL = new clsSQL();
                DataTable dt = clsSQL.ExecuteQueryStatement(@"select * from tbl_employee where (id=@Id or @Id=0 ) and  
                     (AName=@AName or @AName='' ) and (EName=@EName or @EName='' ) and (UserName=@UserName or @UserName='' ) 
                      and  (Password=@Password or @Password='' )
and (CompanyId=@CompanyId or @CompanyId=0 ) 
   and  (Email=@Email or @Email='' )
   and  (Tel1=@Tel1 or @Tel1='' )
and (IsSystemUser=@IsSystemUser or @IsSystemUser=-1 ) 


", clsSQL.CreateDataBaseConnectionString(CompanyId), prm);

                return dt;
            }
            catch (Exception)
            {

                throw;
            }


        }

        public bool DeleteEmployeeByID(int Id,int CompanyID)
        {
            try
            {
                clsSQL clsSQL = new clsSQL();

                SqlParameter[] prm =
                 { new SqlParameter("@Id", SqlDbType.Int) { Value = Id },

                };
                int A = clsSQL.ExecuteNonQueryStatement(@"delete from tbl_employee where (id=@Id  )", clsSQL.CreateDataBaseConnectionString(CompanyID), prm);

                return true;
            }
            catch (Exception)
            {

                throw;
            }


        }
        public int InsertEmployee(string AName, string EName, string UserName, string Password, int CompanyID, int CreationUserId,
          bool IsSystemUser, string Email, string Tel1, byte[] Signuture)
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
                          new SqlParameter("@IsSystemUser", SqlDbType.Bit) { Value = IsSystemUser },
                               new SqlParameter("@Signuture", SqlDbType.Image) { Value = Signuture },
                                    new SqlParameter("@Email", SqlDbType.NVarChar,-1) { Value = Email },
                                         new SqlParameter("@Tel1", SqlDbType.NVarChar,-1) { Value = Tel1 },
                };

                string a = @"insert into tbl_employee(AName,EName,UserName,Password,CompanyID,CreationUserId,CreationDate,IsSystemUser,Email,Tel1,Signuture) 
OUTPUT INSERTED.ID values(@AName,@EName,@UserName,@Password,@CompanyID,@CreationUserId,@CreationDate,@IsSystemUser,@Email,@Tel1,@Signuture)";
                clsSQL clsSQL = new clsSQL();
                return Simulate.Integer32(clsSQL.ExecuteScalar(a, prm, clsSQL.CreateDataBaseConnectionString(CompanyID)));

            }
            catch (Exception)
            {

                throw;
            }


        }
        public int UpdateEmployee(string AName, string EName, string UserName, string Password, int ID
            , int ModificationUserId,bool IsSystemUser,String Email,String Tel1, byte[] Signuture,int CompanyID)
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
                            new SqlParameter("@IsSystemUser", SqlDbType.Bit) { Value = IsSystemUser },
                               new SqlParameter("@Signuture", SqlDbType.Image) { Value = Signuture },
                                   new SqlParameter("@Email", SqlDbType.NVarChar,-1) { Value = Email },
                                       new SqlParameter("@Tel1", SqlDbType.NVarChar,-1) { Value = Tel1 },
                };
                int A = clsSQL.ExecuteNonQueryStatement(@"update tbl_employee set AName=@AName,EName=@EName,
UserName=@UserName,Password=@Password
,ModificationDate=@ModificationDate
,ModificationUserId=@ModificationUserId 
,Signuture=@Signuture
,IsSystemUser=@IsSystemUser
,Email=@Email 
,Tel1=@Tel1 
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
