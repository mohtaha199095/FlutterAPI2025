using Microsoft.Data.SqlClient;
using System;
using System.Data;

namespace WebApplication2.cls
{
    public class clsPosition
    {
        public DataTable SelectPositionByID(int Id, string AName, string EName, int CompanyID)
        {
            SqlParameter[] prm =
            {
                new SqlParameter("@Id", SqlDbType.Int) { Value = Id },
                new SqlParameter("@AName", SqlDbType.NVarChar, -1) { Value = AName ?? "" },
                new SqlParameter("@EName", SqlDbType.NVarChar, -1) { Value = EName ?? "" },
                new SqlParameter("@CompanyID", SqlDbType.Int) { Value = CompanyID },
            };
            clsSQL clsSQL = new clsSQL();
            return clsSQL.ExecuteQueryStatement(@"
                SELECT * FROM tbl_Position
                WHERE (ID=@Id OR @Id=0)
                  AND (AName=@AName OR @AName='')
                  AND (EName=@EName OR @EName='')
                  AND (CompanyID=@CompanyID OR @CompanyID=0)",
                clsSQL.CreateDataBaseConnectionString(CompanyID), prm);
        }

        public bool DeletePositionByID(int Id, int CompanyID)
        {
            clsSQL clsSQL = new clsSQL();
            SqlParameter[] prm =
            {
                new SqlParameter("@Id", SqlDbType.Int) { Value = Id },
                new SqlParameter("@CompanyID", SqlDbType.Int) { Value = CompanyID },
            };
            clsSQL.ExecuteNonQueryStatement(
                "DELETE FROM tbl_Position WHERE ID=@Id AND CompanyID=@CompanyID",
                clsSQL.CreateDataBaseConnectionString(CompanyID), prm);
            return true;
        }

        public int InsertPosition(string AName, string EName, int CompanyID, int CreationUserId, SqlTransaction trn = null)
        {
            SqlParameter[] prm =
            {
                new SqlParameter("@AName", SqlDbType.NVarChar, -1) { Value = AName },
                new SqlParameter("@EName", SqlDbType.NVarChar, -1) { Value = EName },
                new SqlParameter("@CompanyID", SqlDbType.Int) { Value = CompanyID },
                new SqlParameter("@CreationUserId", SqlDbType.Int) { Value = CreationUserId },
                new SqlParameter("@CreationDate", SqlDbType.DateTime) { Value = DateTime.Now },
            };
            string sql = @"INSERT INTO tbl_Position(AName,EName,CompanyID,CreationUserId,CreationDate)
                           OUTPUT INSERTED.ID VALUES(@AName,@EName,@CompanyID,@CreationUserId,@CreationDate)";
            clsSQL clsSQL = new clsSQL();
            if (trn == null)
                return Simulate.Integer32(clsSQL.ExecuteScalar(sql, prm, clsSQL.CreateDataBaseConnectionString(CompanyID)));
            return Simulate.Integer32(clsSQL.ExecuteScalar(sql, prm, clsSQL.CreateDataBaseConnectionString(CompanyID), trn));
        }

        public int UpdatePosition(int ID, string AName, string EName, int ModificationUserId, int CompanyID)
        {
            clsSQL clsSQL = new clsSQL();
            SqlParameter[] prm =
            {
                new SqlParameter("@ID", SqlDbType.Int) { Value = ID },
                new SqlParameter("@AName", SqlDbType.NVarChar, -1) { Value = AName },
                new SqlParameter("@EName", SqlDbType.NVarChar, -1) { Value = EName },
                new SqlParameter("@ModificationUserId", SqlDbType.Int) { Value = ModificationUserId },
                new SqlParameter("@ModificationDate", SqlDbType.DateTime) { Value = DateTime.Now },
            };
            return clsSQL.ExecuteNonQueryStatement(@"
                UPDATE tbl_Position SET
                    AName=@AName, EName=@EName,
                    ModificationDate=@ModificationDate, ModificationUserId=@ModificationUserId
                WHERE ID=@ID AND CompanyID=" + CompanyID,
                clsSQL.CreateDataBaseConnectionString(CompanyID), prm);
        }
    }
}
