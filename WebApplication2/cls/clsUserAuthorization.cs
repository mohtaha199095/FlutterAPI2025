using System.Data.SqlClient;
using System.Data;
using System;

namespace WebApplication2.cls
{
    public class clsUserAuthorization
    {

        public DataTable SelectUserAuthorization(int UserId, int PageID,  int CompanyID)
        {
            try
            {
                SqlParameter[] prm =
                 { new SqlParameter("@UserId", SqlDbType.Int) { Value = UserId },
     new SqlParameter("@PageID", SqlDbType.Int) { Value = PageID },
        new SqlParameter("@CompanyID", SqlDbType.Int) { Value = CompanyID },

                }; clsSQL clsSQL = new clsSQL();
                DataTable dt = clsSQL.ExecuteQueryStatement(@"select * from tbl_UserAuthorization where (UserId=@UserId or @UserId=0 ) and  
                     (PageID=@PageID or @PageID=0 )    and (CompanyID=@CompanyID or @CompanyID=0 )
                   order by pageid asc  ", clsSQL.CreateDataBaseConnectionString(CompanyID), prm);

                return dt;
            }
            catch (Exception)
            {

                throw;
            }


        }

        public bool DeleteUserAuthorizationByUserID(int UserId, int CompanyID)
        {
            try
            {
                clsSQL clsSQL = new clsSQL();

                SqlParameter[] prm =
                 { new SqlParameter("@UserId", SqlDbType.Int) { Value = UserId },

                };
                int A = clsSQL.ExecuteNonQueryStatement(@"delete from tbl_UserAuthorization where (UserId=@UserId  )", clsSQL.CreateDataBaseConnectionString(CompanyID), prm);

                return true;
            }
            catch (Exception)
            {

                throw;
            }


        }
        public string InsertUserAuthorization(DBUserAuthrization DBUserAuthrization, SqlTransaction trn=null)
        {
            try
            {
                SqlParameter[] prm =
                 { new SqlParameter("@PageID", SqlDbType.Int) { Value = DBUserAuthrization.PageID },
                 new SqlParameter("@UserID", SqlDbType.Int) { Value =DBUserAuthrization. UserID },
                       new SqlParameter("@IsAccess", SqlDbType.Bit) { Value = DBUserAuthrization.IsAccess },
                        new SqlParameter("@IsSearch", SqlDbType.Bit) { Value =DBUserAuthrization. IsSearch },
                         new SqlParameter("@IsAdd", SqlDbType.Bit) { Value = DBUserAuthrization.IsAdd },
                          new SqlParameter("@IsEdit", SqlDbType.Bit) { Value = DBUserAuthrization.IsEdit },
                           new SqlParameter("@IsDelete", SqlDbType.Bit) { Value =DBUserAuthrization. IsDelete },
                            new SqlParameter("@IsPrint", SqlDbType.Bit) { Value = DBUserAuthrization.IsPrint },
                  new SqlParameter("@CompanyID", SqlDbType.Int) { Value = DBUserAuthrization.CompanyID },
                   new SqlParameter("@CreationUserId", SqlDbType.Int) { Value = DBUserAuthrization.CreationUserID },
                     new SqlParameter("@CreationDate", SqlDbType.DateTime) { Value = DateTime.Now },
                };

                string a = @"insert into tbl_UserAuthorization(PageID,UserID,IsAccess,IsSearch,IsAdd,IsEdit,IsDelete,IsPrint,CompanyID,CreationUserId,CreationDate)
                        OUTPUT INSERTED.Guid values(@PageID,@UserID,@IsAccess,@IsSearch,@IsAdd,@IsEdit,@IsDelete,@IsPrint,@CompanyID,@CreationUserId,@CreationDate)";
                clsSQL clsSQL = new clsSQL();
                return Simulate.String(clsSQL.ExecuteScalar(a, prm, clsSQL.CreateDataBaseConnectionString(DBUserAuthrization.CompanyID), trn));

            }
            catch (Exception)
            {

                throw;
            }


        }
    }
}
public class DBUserAuthrization
{
    public string Guid { get; set; }
    public int PageID { get; set; }
    public int UserID { get; set; }
    public bool IsAccess { get; set; }
    public bool IsSearch { get; set; }
    public bool IsAdd { get; set; }
    public bool IsEdit { get; set; }
    public bool IsDelete { get; set; }
    public bool IsPrint { get; set; }
    public int CompanyID { get; set; }
    public int CreationUserID { get; set; }
    public DateTime CreationDate { get; set; }
 }