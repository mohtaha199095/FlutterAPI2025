using System;
using Microsoft.Data.SqlClient;
using System.Data;

namespace WebApplication2.cls
{
    public class clsUserAuthorizationModels
    {
        public DataTable SelectUserAuthorizationModels(int UserId, int TypeID,int ModelID, int CompanyID)
        {
            try
            {
                SqlParameter[] prm =
                 { new SqlParameter("@UserId", SqlDbType.Int) { Value = UserId },
     new SqlParameter("@TypeID", SqlDbType.Int) { Value = TypeID },
       new SqlParameter("@ModelID", SqlDbType.Int) { Value = ModelID },  new SqlParameter("@CompanyID", SqlDbType.Int) { Value = CompanyID },

                }; clsSQL clsSQL = new clsSQL();
                DataTable dt = clsSQL.ExecuteQueryStatement(@"select * from tbl_UserAuthorizationModels where (UserId=@UserId or @UserId=0 ) and  
                   (ModelID=@ModelID or @ModelID=0 )    and    (TypeID=@TypeID or @TypeID=0 )    and (CompanyID=@CompanyID or @CompanyID=0 )
                     ", clsSQL.CreateDataBaseConnectionString(CompanyID), prm);

                return dt;
            }
            catch (Exception)
            {

                throw;
            }


        }
        public bool DeleteUserAuthorizationModelsByUserID(int UserId,int CompanyID)
        {
            try
            {
                clsSQL clsSQL = new clsSQL();

                SqlParameter[] prm =
                 { new SqlParameter("@UserId", SqlDbType.Int) { Value = UserId },

                };
                int A = clsSQL.ExecuteNonQueryStatement(@"delete from tbl_UserAuthorizationModels where (UserId=@UserId  )", clsSQL.CreateDataBaseConnectionString(CompanyID), prm);

                return true;
            }
            catch (Exception)
            {

                throw;
            }


        }
        public string InsertUserAuthorizationModels(DBUserAuthrizationModels DBUserAuthrizationModels, SqlTransaction trn = null)
        {
            try
            {
                SqlParameter[] prm =
                 { new SqlParameter("@TypeID", SqlDbType.Int) { Value = DBUserAuthrizationModels.TypeID },
                 new SqlParameter("@ModelID", SqlDbType.Int) { Value =DBUserAuthrizationModels. ModelID },
                       new SqlParameter("@UserID", SqlDbType.Int) { Value = DBUserAuthrizationModels.UserID },
                        new SqlParameter("@IsAccess", SqlDbType.Bit) { Value =DBUserAuthrizationModels. IsAccess },
                         new SqlParameter("@IsDefault", SqlDbType.Bit) { Value = DBUserAuthrizationModels.IsDefault },
                          
                  new SqlParameter("@CompanyID", SqlDbType.Int) { Value = DBUserAuthrizationModels.CompanyID },
                   new SqlParameter("@CreationUserId", SqlDbType.Int) { Value = DBUserAuthrizationModels.CreationUserID },
                     new SqlParameter("@CreationDate", SqlDbType.DateTime) { Value = DateTime.Now },
                };

                string a = @"insert into tbl_UserAuthorizationModels(TypeID,ModelID,UserID,IsAccess,IsDefault,CompanyID,CreationUserId,CreationDate)
                        OUTPUT INSERTED.Guid                 values(@TypeID,@ModelID,@UserID,@IsAccess,@IsDefault,@CompanyID,@CreationUserId,@CreationDate)";
                clsSQL clsSQL = new clsSQL();
                return Simulate.String(clsSQL.ExecuteScalar(a, prm, clsSQL.CreateDataBaseConnectionString(DBUserAuthrizationModels.CompanyID), trn));

            }
            catch (Exception)
            {

                throw;
            }


        }
    }
}
public class DBUserAuthrizationModels
{
    public string Guid { get; set; }
    public int TypeID { get; set; }
    public int ModelID { get; set; }
    public int UserID { get; set; }
    public bool IsAccess { get; set; }
    public bool IsDefault { get; set; }
 
    public int CompanyID { get; set; }
    public int CreationUserID { get; set; }
    public DateTime CreationDate { get; set; }
}