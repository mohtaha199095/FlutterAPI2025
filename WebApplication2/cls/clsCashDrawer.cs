using System;
using System.Data;
using Microsoft.Data.SqlClient;
namespace WebApplication2.cls
{
    public class clsCashDrawer
    {
        public DataTable SelectCashDrawerByID(int Id, string AName, string EName, int CompanyID)
        {
            try
            {
                clsSQL clsSQL = new clsSQL();

                SqlParameter[] prm =
                 { new SqlParameter("@Id", SqlDbType.Int) { Value = Id },
      new SqlParameter("@AName", SqlDbType.NVarChar,-1) { Value = AName },
       new SqlParameter("@EName", SqlDbType.NVarChar,-1) { Value = EName },
        new SqlParameter("@CompanyID", SqlDbType.Int) { Value = CompanyID },

                };
                DataTable dt = clsSQL.ExecuteQueryStatement(@"select * from tbl_CashDrawer where (id=@Id or @Id=0 ) and  
                     (AName=@AName or @AName='' ) and (EName=@EName or @EName='' )   and (CompanyID=@CompanyID or @CompanyID=0 )
                     ", clsSQL.CreateDataBaseConnectionString(CompanyID), prm);

                return dt;
            }
            catch (Exception)
            {

                throw;
            }


        }

        public bool DeleteCashDrawerByID(int Id, int CompanyID)
        {
            try
            {
                clsSQL clsSQL = new clsSQL();

                SqlParameter[] prm =
                 { new SqlParameter("@Id", SqlDbType.Int) { Value = Id },

                };
                int A = clsSQL.ExecuteNonQueryStatement(@"delete from tbl_CashDrawer where (id=@Id  )", clsSQL.CreateDataBaseConnectionString(CompanyID), prm);

                return true;
            }
            catch (Exception)
            {

                throw;
            }


        }
        public int InsertCashDrawer(string AName, string EName, int BranchID, int CompanyID, int CreationUserId)
        {
            try
            {
                SqlParameter[] prm =
                 { new SqlParameter("@AName", SqlDbType.NVarChar,-1) { Value = AName },
                  new SqlParameter("@EName", SqlDbType.NVarChar,-1) { Value = EName },


                                    new SqlParameter("@BranchID", SqlDbType.Int) { Value = BranchID },
                              

                  new SqlParameter("@CompanyID", SqlDbType.Int) { Value = CompanyID },
                   new SqlParameter("@CreationUserId", SqlDbType.Int) { Value = CreationUserId },
                     new SqlParameter("@CreationDate", SqlDbType.DateTime) { Value = DateTime.Now },
                };

                string a = @"insert into tbl_CashDrawer(AName,EName,BranchID,CompanyID,CreationUserId,CreationDate)
                           OUTPUT INSERTED.ID values(@AName,@EName,@BranchID,@CompanyID,@CreationUserId,@CreationDate)";
                clsSQL clsSQL = new clsSQL();
                return Simulate.Integer32(clsSQL.ExecuteScalar(a, prm,clsSQL.CreateDataBaseConnectionString(CompanyID)));

            }
            catch (Exception)
            {

                throw;
            }


        }
        public int UpdateCashDrawer(int ID, string AName, string EName, int BranchID, int ModificationUserId,int CompanyID)
        {
            try
            {
                clsSQL clsSQL = new clsSQL();

                SqlParameter[] prm =
                 {
                     new SqlParameter("@ID", SqlDbType.Int) { Value = ID },
                new SqlParameter("@AName", SqlDbType.NVarChar,-1) { Value = AName },
                  new SqlParameter("@EName", SqlDbType.NVarChar,-1) { Value = EName },



                                    new SqlParameter("@BranchID", SqlDbType.Int) { Value = BranchID },


                         new SqlParameter("@ModificationUserId", SqlDbType.Int) { Value = ModificationUserId },

                        

                         
                     new SqlParameter("@ModificationDate", SqlDbType.DateTime) { Value = DateTime.Now },
                };
                int A = clsSQL.ExecuteNonQueryStatement(@"update tbl_CashDrawer set 
                       AName=@AName,
                       EName=@EName,
                       BranchID=@BranchID,
                       ModificationDate=@ModificationDate,
                       ModificationUserId=@ModificationUserId
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

