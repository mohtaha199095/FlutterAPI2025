using System;
using System.Data;
using System.Data.SqlClient;
namespace WebApplication2.cls
{

    public class clsItemsBoxType
    {




        public DataTable SelectItemsBoxTypeByID(int Id, string AName, string EName, int CompanyID)
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
                DataTable dt = clsSQL.ExecuteQueryStatement(@"select * from tbl_ItemsBoxType where (id=@Id or @Id=0 ) and  
                     (AName=@AName or @AName='' ) and (EName=@EName or @EName='' )   and (CompanyID=@CompanyID or @CompanyID=0 )
                     ", prm);

                return dt;
            }
            catch (Exception)
            {

                throw;
            }


        }

        public bool DeleteItemsBoxTypeByID(int Id)
        {
            try
            {
                clsSQL clsSQL = new clsSQL();

                SqlParameter[] prm =
                 { new SqlParameter("@Id", SqlDbType.Int) { Value = Id },

                };
                int A = clsSQL.ExecuteNonQueryStatement(@"delete from tbl_ItemsBoxType where (id=@Id  )", prm);

                return true;
            }
            catch (Exception)
            {

                throw;
            }


        }
        public int InsertItemsBoxType(string AName, string EName, decimal Qty, int CompanyID, int CreationUserId)
        {
            try
            {
                SqlParameter[] prm =
                 { new SqlParameter("@AName", SqlDbType.NVarChar,-1) { Value = AName },
                  new SqlParameter("@EName", SqlDbType.NVarChar,-1) { Value = EName },
                    new SqlParameter("@Qty", SqlDbType.Decimal) { Value = Qty },


                  new SqlParameter("@CompanyID", SqlDbType.Int) { Value = CompanyID },
                   new SqlParameter("@CreationUserId", SqlDbType.Int) { Value = CreationUserId },
                     new SqlParameter("@CreationDate", SqlDbType.DateTime) { Value = DateTime.Now },
                };

                string a = @"insert into tbl_ItemsBoxType(AName,EName,Qty,CompanyID,CreationUserId,CreationDate)
                    OUTPUT INSERTED.ID values(@AName,@EName,@Qty ,@CompanyID,@CreationUserId,@CreationDate)";
                clsSQL clsSQL = new clsSQL();


                return Simulate.Integer32(clsSQL.ExecuteScalar(a, prm));

            }
            catch (Exception)
            {

                throw;
            }


        }
        public int UpdateItemsBoxType(int ID, string AName, string EName, decimal Qty, int ModificationUserId)
        {
            try
            {
                clsSQL clsSQL = new clsSQL();

                SqlParameter[] prm =
                 {
                     new SqlParameter("@ID", SqlDbType.Int) { Value = ID },
                new SqlParameter("@AName", SqlDbType.NVarChar,-1) { Value = AName },
                  new SqlParameter("@EName", SqlDbType.NVarChar,-1) { Value = EName },
                    new SqlParameter("@Qty", SqlDbType.Decimal) { Value = Qty },



                         new SqlParameter("@ModificationUserId", SqlDbType.Int) { Value = ModificationUserId },
                     new SqlParameter("@ModificationDate", SqlDbType.DateTime) { Value = DateTime.Now },
                };
                int A = clsSQL.ExecuteNonQueryStatement(@"update tbl_ItemsBoxType set 
                       AName=@AName,
                       EName=@EName,
Qty=@Qty,
 

                       ModificationDate=@ModificationDate,
                       ModificationUserId=@ModificationUserId
                   where id =@id", prm);

                return A;
            }
            catch (Exception)
            {

                throw;
            }


        }
    }
}

