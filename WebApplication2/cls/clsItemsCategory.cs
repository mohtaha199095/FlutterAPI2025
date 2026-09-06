using System;
using System.Data;
using Microsoft.Data.SqlClient;

namespace WebApplication2.cls
{
    public class clsItemsCategory
    {
        public DataTable SelectItemsCategory(int Id, string AName, string EName, int CompanyId)
        {
            try
            {
                clsSQL clsSQL = new clsSQL();

                SqlParameter[] prm =
                 { new SqlParameter("@Id", SqlDbType.Int) { Value = Id },
      new SqlParameter("@AName", SqlDbType.NVarChar,-1) { Value = AName },
       new SqlParameter("@EName", SqlDbType.NVarChar,-1) { Value = EName },
       new SqlParameter("@CompanyId", SqlDbType.Int) { Value = CompanyId },

                };
                DataTable dt = clsSQL.ExecuteQueryStatement(@"select * from tbl_ItemsCategory where (id=@Id or @Id=0 ) and  
                     (AName=@AName or @AName='' ) and (EName=@EName or @EName='' )and (CompanyId=@CompanyId or @CompanyId=0  )  
                     order by ISNULL(POSOrder, 2147483647), AName
                     ", clsSQL.CreateDataBaseConnectionString( CompanyId), prm);

                return dt;
            }
            catch (Exception)
            {

                throw;
            }


        }

        public bool ReorderItemsCategories(string orderedIds, int CompanyID, int ModificationUserID)
        {
            if (string.IsNullOrWhiteSpace(orderedIds)) return false;
            string[] ids = orderedIds.Split(',', StringSplitOptions.RemoveEmptyEntries);
            clsSQL clsSQL = new clsSQL();
            string conn = clsSQL.CreateDataBaseConnectionString(CompanyID);
            for (int i = 0; i < ids.Length; i++)
            {
                int categoryId = Simulate.Integer32(ids[i].Trim());
                if (categoryId <= 0) continue;
                SqlParameter[] prm =
                {
                    new SqlParameter("@ID", SqlDbType.Int) { Value = categoryId },
                    new SqlParameter("@POSOrder", SqlDbType.Int) { Value = i + 1 },
                    new SqlParameter("@ModificationUserId", SqlDbType.Int) { Value = ModificationUserID },
                    new SqlParameter("@CompanyID", SqlDbType.Int) { Value = CompanyID },
                };
                clsSQL.ExecuteNonQueryStatement(
                    @"UPDATE tbl_ItemsCategory SET POSOrder=@POSOrder, ModificationUserId=@ModificationUserId, ModificationDate=GETDATE()
                      WHERE ID=@ID AND CompanyID=@CompanyID",
                    conn, prm);
            }
            return true;
        }

        public bool DeleteItemsCategoryByID(int Id,int CompanyID)
        {
            try
            {
                clsSQL clsSQL = new clsSQL();

                SqlParameter[] prm =
                 { new SqlParameter("@Id", SqlDbType.Int) { Value = Id },

                };
                int A = clsSQL.ExecuteNonQueryStatement(@"delete from tbl_ItemsCategory where (id=@Id  )", clsSQL.CreateDataBaseConnectionString(CompanyID), prm);

                return true;
            }
            catch (Exception)
            {

                throw;
            }


        }
        public int InsertItemsCategory(string AName, string EName, int CompanyID, int CreationUserId,SqlTransaction trn=null)
        {
            try
            {
                SqlParameter[] prm =
                 { new SqlParameter("@AName", SqlDbType.NVarChar,-1) { Value = AName },
                  new SqlParameter("@EName", SqlDbType.NVarChar,-1) { Value = EName },

                    new SqlParameter("@CompanyID", SqlDbType.Int) { Value = CompanyID },
                   new SqlParameter("@CreationUserId", SqlDbType.Int) { Value = CreationUserId },
                     new SqlParameter("@CreationDate", SqlDbType.DateTime) { Value = DateTime.Now },
                };

                string a = @"insert into tbl_ItemsCategory(AName,EName,CompanyID,CreationUserId,CreationDate)
                        OUTPUT INSERTED.ID values(@AName,@EName,@CompanyID,@CreationUserId,@CreationDate)";
                clsSQL clsSQL = new clsSQL();
                if (trn == null) {
                    return Simulate.Integer32(clsSQL.ExecuteScalar(a, prm, clsSQL.CreateDataBaseConnectionString(CompanyID)));


                }
                else
                { 
                return Simulate.Integer32(clsSQL.ExecuteScalar(a, prm, clsSQL.CreateDataBaseConnectionString(CompanyID),trn));
                
                }

            }
            catch (Exception)
            {

                throw;
            }


        }
        public int UpdateItemsCategory(int ID, string AName, string EName, int ModificationUserId, int CompanyID,SqlTransaction trn=null)
        {
            try
            {
                clsSQL clsSQL = new clsSQL();

                SqlParameter[] prm =
                 {
                     new SqlParameter("@ID", SqlDbType.Int) { Value = ID },
                  new SqlParameter("@AName", SqlDbType.NVarChar,-1) { Value = AName },
                  new SqlParameter("@EName", SqlDbType.NVarChar,-1) { Value = EName },

                         new SqlParameter("@ModificationUserId", SqlDbType.Int) { Value = ModificationUserId },
                     new SqlParameter("@ModificationDate", SqlDbType.DateTime) { Value = DateTime.Now },
                };
                if (trn == null) {
                    int A = clsSQL.ExecuteNonQueryStatement(@"update tbl_ItemsCategory set 
                       AName=@AName,
                       EName=@EName,
                       ModificationDate=@ModificationDate,
                       ModificationUserId=@ModificationUserId
                   where id =@id", clsSQL.CreateDataBaseConnectionString(CompanyID), prm);

                    return A;

                } else {
                    int A = clsSQL.ExecuteNonQueryStatement(@"update tbl_ItemsCategory set 
                       AName=@AName,
                       EName=@EName,
                       ModificationDate=@ModificationDate,
                       ModificationUserId=@ModificationUserId
                   where id =@id", clsSQL.CreateDataBaseConnectionString(CompanyID), prm,trn);

                    return A;

                }
               
            }
            catch (Exception)
            {

                throw;
            }


        }
    }
}
