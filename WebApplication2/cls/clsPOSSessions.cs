

using System;
using System.Data;
using Microsoft.Data.SqlClient;

namespace WebApplication2.cls
{
    public class clsPOSSessions
    {
        public DataTable SelectPOSSessionsByGuid(string Guid, string POSDayGuid, int Status, int CompanyID)
        {
            try
            {
                clsSQL clsSQL = new clsSQL();

                SqlParameter[] prm =
                 {
      new SqlParameter("@Guid", SqlDbType.UniqueIdentifier) { Value =Simulate.Guid( Guid) },
       new SqlParameter("@POSDayGuid", SqlDbType.UniqueIdentifier) { Value =Simulate.Guid( POSDayGuid) },
     new SqlParameter("@CompanyID", SqlDbType.Int) { Value = CompanyID },

      new SqlParameter("@Status", SqlDbType.Int) { Value = Status },


                };
                DataTable dt = clsSQL.ExecuteQueryStatement(@"select * from tbl_POSSessions where (Guid=@Guid or @Guid='00000000-0000-0000-0000-000000000000') and  
                  (POSDayGuid=@POSDayGuid or @POSDayGuid='00000000-0000-0000-0000-000000000000') and    (Status=@Status or @Status=-1 ) and    (CompanyID=@CompanyID or @CompanyID=0 )
                     ", clsSQL.CreateDataBaseConnectionString(CompanyID), prm);

                return dt;
            }
            catch (Exception)
            {

                throw;
            }


        }

        public bool DeletePOSSessionsByGuid(string Guid,int CompanyID)
        {
            try
            {
                clsSQL clsSQL = new clsSQL();

                SqlParameter[] prm =
                 { new SqlParameter("@Guid", SqlDbType.UniqueIdentifier) { Value = Simulate.Guid( Guid) },

                };
                int A = clsSQL.ExecuteNonQueryStatement(@"delete from tbl_POSSessions where (Guid=@Guid  )", clsSQL.CreateDataBaseConnectionString(CompanyID), prm);

                return true;
            }
            catch (Exception)
            {

                throw;
            }


        }
        public string InsertPOSSessions(string POSDayGuid, int SessionTypeID, DateTime StartDate, DateTime EndDate, int CashDrawerID, int Status, int CompanyID, int CreationUserId, SqlTransaction trn = null)
        {
            try
            {
                SqlParameter[] prm =
                 {
   new SqlParameter("@POSDayGuid", SqlDbType.UniqueIdentifier) { Value =Simulate.Guid( POSDayGuid) },

                           new SqlParameter("@SessionTypeID", SqlDbType.Int) { Value = SessionTypeID },

                    new SqlParameter("@StartDate", SqlDbType.DateTime) { Value = StartDate },
                    new SqlParameter("@EndDate", SqlDbType.DateTime) { Value = EndDate },
                        new SqlParameter("@CashDrawerID", SqlDbType.Int) { Value = CashDrawerID },


                        new SqlParameter("@Status", SqlDbType.Int) { Value = Status },
                     new SqlParameter("@CompanyID", SqlDbType.Int) { Value = CompanyID },
                   new SqlParameter("@CreationUserId", SqlDbType.Int) { Value = CreationUserId },
                     new SqlParameter("@CreationDate", SqlDbType.DateTime) { Value = DateTime.Now },
                };

                string a = @"insert into tbl_POSSessions(POSDayGuid,SessionTypeID,StartDate,EndDate,CashDrawerID,Status,CompanyID,CreationUserId,CreationDate)
                            OUTPUT INSERTED.Guid values(@POSDayGuid,@SessionTypeID,@StartDate,@EndDate,@CashDrawerID,@Status,@CompanyID,@CreationUserId,@CreationDate)";
                clsSQL clsSQL = new clsSQL();

                return Simulate.String(clsSQL.ExecuteScalar(a, prm, clsSQL.CreateDataBaseConnectionString(CompanyID), trn));

            }
            catch (Exception)
            {

                throw;
            }


        }
        public int UpdatePOSSessions(string Guid, int SessionTypeID, string POSDayGuid, DateTime StartDate, DateTime EndDate, int CashDrawerID, int Status, int ModificationUserId,int CompanyID)
        {
            try
            {
                clsSQL clsSQL = new clsSQL();

                SqlParameter[] prm =
                 {  new SqlParameter("@Guid", SqlDbType.UniqueIdentifier) { Value =Simulate.Guid( Guid) },
                                     new SqlParameter("@SessionTypeID", SqlDbType.Int) { Value = SessionTypeID },

                    new SqlParameter("@POSDayGuid", SqlDbType.UniqueIdentifier) { Value =Simulate.Guid( POSDayGuid) },


                    new SqlParameter("@StartDate", SqlDbType.DateTime) { Value = StartDate },
                    new SqlParameter("@EndDate", SqlDbType.DateTime) { Value = EndDate },
                        new SqlParameter("@CashDrawerID", SqlDbType.Int) { Value = CashDrawerID },


                        new SqlParameter("@Status", SqlDbType.Int) { Value = Status },


                         new SqlParameter("@ModificationUserId", SqlDbType.Int) { Value = ModificationUserId },
                     new SqlParameter("@ModificationDate", SqlDbType.DateTime) { Value = DateTime.Now },
                };
                int A = clsSQL.ExecuteNonQueryStatement(@"update tbl_POSSessions set 
                       POSDayGuid=@POSDayGuid,
                       StartDate=@StartDate,
                       EndDate=@EndDate,
                       CashDrawerID=@CashDrawerID,
                       Status=@Status
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
        public int ClosePOSSessions(string Guid, DateTime EndDate, int ModificationUserId,int CompanyID, SqlTransaction trn)
        {
            try
            {
                clsSQL clsSQL = new clsSQL();

                SqlParameter[] prm =
                 {  new SqlParameter("@Guid", SqlDbType.UniqueIdentifier) { Value =Simulate.Guid( Guid) },




                    new SqlParameter("@EndDate", SqlDbType.DateTime) { Value = EndDate },



                        new SqlParameter("@Status", SqlDbType.Int) { Value = 0 },


                         new SqlParameter("@ModificationUserId", SqlDbType.Int) { Value = ModificationUserId },
                     new SqlParameter("@ModificationDate", SqlDbType.DateTime) { Value = DateTime.Now },
                };
                int A = clsSQL.ExecuteNonQueryStatement(@"update tbl_POSSessions set 
                      
                       EndDate=@EndDate,
                      
                       Status=@Status,
                       ModificationDate=@ModificationDate,
                       ModificationUserId=@ModificationUserId
                   where Guid =@Guid", clsSQL.CreateDataBaseConnectionString(CompanyID), prm, trn);

                return A;
            }
            catch (Exception)
            {

                throw;
            }


        }
    }
}
