using Microsoft.Data.SqlClient;
using System.Data;
using System;

namespace WebApplication2.cls
{
    public class clsBranchFloors
    {
        public DataTable SelectBranchFloors(int Id, string AName, string EName, int BranchID, int CompanyID)
        {
            try
            {
                SqlParameter[] prm =
                {
            new SqlParameter("@Id", SqlDbType.Int) { Value = Id },
            new SqlParameter("@AName", SqlDbType.NVarChar, -1) { Value = AName },
            new SqlParameter("@EName", SqlDbType.NVarChar, -1) { Value = EName },
            new SqlParameter("@BranchID", SqlDbType.Int) { Value = BranchID },
            new SqlParameter("@CompanyID", SqlDbType.Int) { Value = CompanyID }
        };

                clsSQL clsSQL = new clsSQL();
                DataTable dt = clsSQL.ExecuteQueryStatement(@"select * from tbl_BranchFloors where (id=@Id or @Id=0) and 
            (AName=@AName or @AName='') and (EName=@EName or @EName='') and (BranchID=@BranchID or @BranchID=0) and (CompanyID=@CompanyID or @CompanyID=0)",
                    clsSQL.CreateDataBaseConnectionString(CompanyID), prm);

                return dt;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public bool DeleteBranchFloorByID(int Id, int CompanyID)
        {
            try
            {
                clsSQL clsSQL = new clsSQL();

                SqlParameter[] prm =
                {
            new SqlParameter("@Id", SqlDbType.Int) { Value = Id }
        };

                clsSQL.ExecuteNonQueryStatement("delete from tbl_BranchFloors where id=@Id", clsSQL.CreateDataBaseConnectionString(CompanyID), prm);
                return true;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public int InsertBranchFloor(string AName, string EName, int BranchID, int CompanyID, int CreationUserID)
        {
            try
            {
                SqlParameter[] prm =
                {
            new SqlParameter("@AName", SqlDbType.NVarChar, -1) { Value = AName },
            new SqlParameter("@EName", SqlDbType.NVarChar, -1) { Value = EName },
            new SqlParameter("@BranchID", SqlDbType.Int) { Value = BranchID },
            new SqlParameter("@CompanyID", SqlDbType.Int) { Value = CompanyID },
            new SqlParameter("@CreationUserID", SqlDbType.Int) { Value = CreationUserID },
            new SqlParameter("@CreationDate", SqlDbType.DateTime) { Value = DateTime.Now }
        };

                string query = @"insert into tbl_BranchFloors (AName, EName, BranchID, CompanyID, CreationUserID, CreationDate)
            OUTPUT INSERTED.ID values (@AName, @EName, @BranchID, @CompanyID, @CreationUserID, @CreationDate)";

                clsSQL clsSQL = new clsSQL();
                return Simulate.Integer32(clsSQL.ExecuteScalar(query, prm, clsSQL.CreateDataBaseConnectionString(CompanyID)));
            }
            catch (Exception)
            {
                throw;
            }
        }

        public int UpdateBranchFloor(int ID, string AName, string EName, int BranchID, int ModificationUserID, int CompanyID)
        {
            try
            {
                SqlParameter[] prm =
                {
            new SqlParameter("@ID", SqlDbType.Int) { Value = ID },
            new SqlParameter("@AName", SqlDbType.NVarChar, -1) { Value = AName },
            new SqlParameter("@EName", SqlDbType.NVarChar, -1) { Value = EName },
            new SqlParameter("@BranchID", SqlDbType.Int) { Value = BranchID },
            new SqlParameter("@ModificationUserID", SqlDbType.Int) { Value = ModificationUserID },
            new SqlParameter("@ModificationDate", SqlDbType.DateTime) { Value = DateTime.Now }
        };

                string query = @"update tbl_BranchFloors set 
            AName=@AName,
            EName=@EName,
            BranchID=@BranchID,
            ModificationUserID=@ModificationUserID,
            ModificationDate=@ModificationDate
            where id=@ID";

                clsSQL clsSQL = new clsSQL();
                return clsSQL.ExecuteNonQueryStatement(query, clsSQL.CreateDataBaseConnectionString(CompanyID), prm);
            }
            catch (Exception)
            {
                throw;
            }
        }

    }
}
