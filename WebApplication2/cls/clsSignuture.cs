using System;
using System.Data;
using System.Data.SqlClient;
using System.Security.Cryptography.Xml;
using static WebApplication2.MainClasses.clsEnum;

namespace WebApplication2.cls
{
    public class clsSignuture
    {
        public DataTable SelectSignuture( string Guid, int IsOpen,int CreationUserID, int CompanyID)
        {
            try
            {
                SqlParameter[] prm =
                 { new SqlParameter("@Guid", SqlDbType.UniqueIdentifier) { Value = Simulate.Guid( Guid )},
      new SqlParameter("@IsOpen", SqlDbType.Int) { Value = IsOpen },
       new SqlParameter("@CompanyID", SqlDbType.Int) { Value = CompanyID },
              new SqlParameter("@CreationUserID", SqlDbType.Int) { Value = CreationUserID },
       
                }; clsSQL clsSQL = new clsSQL();
                DataTable dt = clsSQL.ExecuteQueryStatement(@"select * from tbl_Signuture where (Guid=@Guid or @Guid='00000000-0000-0000-0000-000000000000' ) and  
                     (IsOpen=@IsOpen or @IsOpen=-1 )   and (CompanyID=@CompanyID or @CompanyID=0 )  and (CreationUserID=@CreationUserID or @CreationUserID=0 )
                  order by creationdate desc    ", clsSQL.CreateDataBaseConnectionString(CompanyID), prm);

                return dt;
            }
            catch (Exception)
            {

                throw;
            }


        }

        public bool DeleteSignutureByGuid(string Guid,int CompanyID)
        {
            try
            {
                clsSQL clsSQL = new clsSQL();

                SqlParameter[] prm =
                 { new SqlParameter("@Guid", SqlDbType.UniqueIdentifier) { Value = Simulate.Guid( Guid) },

                };
                int A = clsSQL.ExecuteNonQueryStatement(@"delete from tbl_Signuture where (Guid=@Guid  )", clsSQL.CreateDataBaseConnectionString(CompanyID), prm);

                return true;
            }
            catch (Exception)
            {

                throw;
            }


        }
        public string InsertSignuture(byte[] Signuture, string SourceGuid, int VoucherType,bool IsOpen, int CompanyID, int CreationUserId, string Terms)
        {
            try
            {
                SqlParameter[] prm =
                 { 
                    
                    new SqlParameter("@Signuture", SqlDbType.Image) { Value = Signuture },
                  new SqlParameter("@SourceGuid", SqlDbType.UniqueIdentifier) { Value = Simulate.Guid( SourceGuid) },
                  new SqlParameter("@VoucherType", SqlDbType.Int) { Value = VoucherType },
                  new SqlParameter("@IsOpen", SqlDbType.Bit) { Value = IsOpen },

                



                  new SqlParameter("@CompanyID", SqlDbType.Int) { Value = CompanyID },
                   new SqlParameter("@CreationUserId", SqlDbType.Int) { Value = CreationUserId },
                     new SqlParameter("@CreationDate", SqlDbType.DateTime) { Value = DateTime.Now },
                         new SqlParameter("@Terms", SqlDbType.NVarChar,-1) { Value = Terms },

                     
                };

                string a = @"insert into tbl_Signuture(Signuture,SourceGuid,VoucherType,IsOpen,CompanyID,CreationUserId,CreationDate,Terms)
                        OUTPUT INSERTED.Guid values(@Signuture,@SourceGuid,@VoucherType,@IsOpen,@CompanyID,@CreationUserId,@CreationDate,@Terms)";
                clsSQL clsSQL = new clsSQL();
                return Simulate.String(clsSQL.ExecuteScalar(a, prm, clsSQL.CreateDataBaseConnectionString(CompanyID)));

            }
            catch (Exception)
            {

                throw;
            }


        }
        public int UpdateSignuture(  string Guid, bool IsOpen, byte[] Signuture, int ModificationUserId,int CompanyID)
        {
            try
            {
                clsSQL clsSQL = new clsSQL();

                SqlParameter[] prm =
                 {
                     new SqlParameter("@Guid", SqlDbType.UniqueIdentifier) { Value = Simulate.Guid(Guid) },
          new SqlParameter("@IsOpen", SqlDbType.Bit) { Value = IsOpen },

                  new SqlParameter("@Signuture", SqlDbType.Image) { Value = Signuture },
                         new SqlParameter("@ModificationUserId", SqlDbType.Int) { Value = ModificationUserId },
                     new SqlParameter("@ModificationDate", SqlDbType.DateTime) { Value = DateTime.Now },
                };
                int A = clsSQL.ExecuteNonQueryStatement(@"update tbl_Signuture set 
                       IsOpen=@IsOpen,
                       Signuture=@Signuture,
                   
                       ModificationDate=@ModificationDate,
                       ModificationUserId=@ModificationUserId
                   where Guid =@Guid", clsSQL.CreateDataBaseConnectionString(CompanyID), prm);

                return A;
            }
            catch (Exception)
            {

                throw;
            }


        }
    }
}
