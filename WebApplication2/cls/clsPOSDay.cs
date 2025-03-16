
using System;
using System.Data;
using Microsoft.Data.SqlClient;

namespace WebApplication2.cls
{
    public class clsPOSDay
    {
        public DataTable SelectPOSDayByGuid(string Guid, int Status, int CompanyID)
        {
            try
            {
                clsSQL clsSQL = new clsSQL();

                SqlParameter[] prm =
                 {
      new SqlParameter("@Guid", SqlDbType.UniqueIdentifier) { Value =Simulate.Guid( Guid) },
        new SqlParameter("@Status", SqlDbType.Int) { Value = Status },
     new SqlParameter("@CompanyID", SqlDbType.Int) { Value = CompanyID },

                }; 
                DataTable dt = clsSQL.ExecuteQueryStatement(@"select * from tbl_POSDay where (Guid=@Guid or @Guid='00000000-0000-0000-0000-000000000000') and  
                      (CompanyID=@CompanyID or @CompanyID=0 ) and  
                      (Status=@Status or @Status=-1 )  order by CreationDate desc
                     ", clsSQL.CreateDataBaseConnectionString(CompanyID), prm);

                return dt;
            }
            catch (Exception)
            {

                throw;
            }


        }

        public bool DeletePOSDayByGuid(string Guid, int CompanyID)
        {
            try
            {
                clsSQL clsSQL = new clsSQL();

                SqlParameter[] prm =
                 { new SqlParameter("@Guid", SqlDbType.UniqueIdentifier) { Value = Simulate.Guid( Guid) },

                };
                int A = clsSQL.ExecuteNonQueryStatement(@"delete from tbl_POSDay where (Guid=@Guid  )", clsSQL.CreateDataBaseConnectionString(CompanyID), prm);

                return true;
            }
            catch (Exception)
            {

                throw;
            }


        }
        public string InsertPOSDay(DateTime StartDate, DateTime EndDate, DateTime POSDate, int Status, int CompanyID,int CashDrawerID, int CreationUserId, SqlTransaction trn = null)
        {
            try
            {
                SqlParameter[] prm =
                 {

                    new SqlParameter("@StartDate", SqlDbType.DateTime) { Value = StartDate },
                    new SqlParameter("@EndDate", SqlDbType.DateTime) { Value = EndDate },
                    new SqlParameter("@POSDate", SqlDbType.DateTime) { Value = POSDate },
                        new SqlParameter("@Status", SqlDbType.Int) { Value = Status },        
                    new SqlParameter("@CompanyID", SqlDbType.Int) { Value = CompanyID },
                     new SqlParameter("@CashDrawerID", SqlDbType.Int) { Value = CashDrawerID },
                   new SqlParameter("@CreationUserId", SqlDbType.Int) { Value = CreationUserId },
                     new SqlParameter("@CreationDate", SqlDbType.DateTime) { Value = DateTime.Now },
                };

                string a = @"insert into tbl_POSDay(StartDate,EndDate,POSDate,Status,CompanyID,CashDrawerID, CreationUserId,CreationDate)
                        OUTPUT INSERTED.Guid values(@StartDate,@EndDate,@POSDate,@Status,@CompanyID,@CashDrawerID,@CreationUserId,@CreationDate)";
                clsSQL clsSQL = new clsSQL();

                return Simulate.String(clsSQL.ExecuteScalar(a, prm, clsSQL.CreateDataBaseConnectionString(CompanyID)));

            }
            catch (Exception)
            {

                throw;
            }


        }
        public int UpdatePOSDay(string Guid, DateTime StartDate, DateTime EndDate, DateTime POSDate, int Status, int ModificationUserId,int CompanyID)
        {
            try
            {
                clsSQL clsSQL = new clsSQL();

                SqlParameter[] prm =
                 { new SqlParameter("@Guid", SqlDbType.UniqueIdentifier) { Value =Simulate.Guid( Guid) },
                                      new SqlParameter("@StartDate", SqlDbType.DateTime) { Value = StartDate },
                    new SqlParameter("@EndDate", SqlDbType.DateTime) { Value = EndDate },
                    new SqlParameter("@POSDate", SqlDbType.DateTime) { Value = POSDate },
                        new SqlParameter("@Status", SqlDbType.Int) { Value = Status },



                         new SqlParameter("@ModificationUserId", SqlDbType.Int) { Value = ModificationUserId },
                     new SqlParameter("@ModificationDate", SqlDbType.DateTime) { Value = DateTime.Now },
                };
                int A = clsSQL.ExecuteNonQueryStatement(@"update tbl_POSDay set 
                       StartDate=@StartDate,
                       EndDate=@EndDate,
                       POSDate=@POSDate,
                       Status=@Status,
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
        public int ClosePOSDay(string Guid, DateTime EndDate, int ModificationUserId,int CashDrawerID,int CompanyID, SqlTransaction trn)
        {
            try
            {
                clsSQL clsSQL = new clsSQL();

                SqlParameter[] prm =
                 { new SqlParameter("@Guid", SqlDbType.UniqueIdentifier) { Value =Simulate.Guid( Guid) },

                    new SqlParameter("@EndDate", SqlDbType.DateTime) { Value = EndDate },

                        new SqlParameter("@Status", SqlDbType.Int) { Value = 0 },

                         new SqlParameter("@CashDrawerID", SqlDbType.Int) { Value = CashDrawerID },

                         new SqlParameter("@ModificationUserId", SqlDbType.Int) { Value = ModificationUserId },
                     new SqlParameter("@ModificationDate", SqlDbType.DateTime) { Value = DateTime.Now },
                };
                int A = clsSQL.ExecuteNonQueryStatement(@"update tbl_POSDay set 
                     
                       EndDate=@EndDate,
                     
                       Status=@Status,
                       ModificationDate=@ModificationDate,
                       ModificationUserId=@ModificationUserId
                   where  CashDrawerID=@CashDrawerID and Guid =@Guid", clsSQL.CreateDataBaseConnectionString(CompanyID), prm, trn);

                return A;
            }
            catch (Exception)
            {

                throw;
            }


        }
    }
}
