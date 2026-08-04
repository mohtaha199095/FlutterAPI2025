using Microsoft.Data.SqlClient;
using System;
using System.Data;

namespace WebApplication2.cls
{
    public class clsWorkCenter
    {
        public DataTable SelectWorkCenter(int id, string workCenterCode, string aName, int companyID)
        {
            try
            {
                SqlParameter[] prm =
                {
                    new SqlParameter("@ID", SqlDbType.Int) { Value = id },
                    new SqlParameter("@WorkCenterCode", SqlDbType.NVarChar, -1) { Value = workCenterCode ?? "" },
                    new SqlParameter("@AName", SqlDbType.NVarChar, -1) { Value = aName ?? "" },
                    new SqlParameter("@CompanyID", SqlDbType.Int) { Value = companyID },
                };

                clsSQL clsSQL = new clsSQL();
                return clsSQL.ExecuteQueryStatement(@"
                    SELECT * FROM tbl_WorkCenter
                    WHERE (ID = @ID OR @ID = 0)
                      AND (WorkCenterCode = @WorkCenterCode OR @WorkCenterCode = '')
                      AND (AName = @AName OR @AName = '')
                      AND CompanyID = @CompanyID
                    ORDER BY WorkCenterCode
                ", clsSQL.CreateDataBaseConnectionString(companyID), prm);
            }
            catch (Exception)
            {
                throw;
            }
        }

        public bool DeleteWorkCenterByID(int id, int companyID)
        {
            try
            {
                clsSQL clsSQL = new clsSQL();
                SqlParameter[] prm =
                {
                    new SqlParameter("@ID", SqlDbType.Int) { Value = id },
                    new SqlParameter("@CompanyID", SqlDbType.Int) { Value = companyID },
                };
                clsSQL.ExecuteNonQueryStatement(
                    "DELETE FROM tbl_WorkCenter WHERE ID = @ID AND CompanyID = @CompanyID",
                    clsSQL.CreateDataBaseConnectionString(companyID), prm);
                return true;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public int InsertWorkCenter(
            string workCenterCode,
            string aName,
            string eName,
            int branchID,
            decimal capacityPerDay,
            bool isActive,
            string notes,
            int companyID,
            int creationUserId,
            SqlTransaction trn = null)
        {
            try
            {
                SqlParameter[] prm =
                {
                    new SqlParameter("@WorkCenterCode", SqlDbType.NVarChar, -1) { Value = workCenterCode },
                    new SqlParameter("@AName", SqlDbType.NVarChar, -1) { Value = aName },
                    new SqlParameter("@EName", SqlDbType.NVarChar, -1) { Value = eName },
                    new SqlParameter("@BranchID", SqlDbType.Int) { Value = branchID },
                    new SqlParameter("@CapacityPerDay", SqlDbType.Decimal) { Value = capacityPerDay },
                    new SqlParameter("@IsActive", SqlDbType.Bit) { Value = isActive },
                    new SqlParameter("@Notes", SqlDbType.NVarChar, -1) { Value = notes ?? "" },
                    new SqlParameter("@CompanyID", SqlDbType.Int) { Value = companyID },
                    new SqlParameter("@CreationUserId", SqlDbType.Int) { Value = creationUserId },
                    new SqlParameter("@CreationDate", SqlDbType.DateTime) { Value = DateTime.Now },
                };

                string sql = @"
                    INSERT INTO tbl_WorkCenter
                    (WorkCenterCode, AName, EName, BranchID, CapacityPerDay, IsActive, Notes,
                     CompanyID, CreationUserId, CreationDate)
                    OUTPUT INSERTED.ID
                    VALUES
                    (@WorkCenterCode, @AName, @EName, @BranchID, @CapacityPerDay, @IsActive, @Notes,
                     @CompanyID, @CreationUserId, @CreationDate)";

                clsSQL clsSQL = new clsSQL();
                if (trn == null)
                {
                    return Simulate.Integer32(clsSQL.ExecuteScalar(sql, prm, clsSQL.CreateDataBaseConnectionString(companyID)));
                }

                return Simulate.Integer32(clsSQL.ExecuteScalar(sql, prm, clsSQL.CreateDataBaseConnectionString(companyID), trn));
            }
            catch (Exception)
            {
                throw;
            }
        }

        public int UpdateWorkCenter(
            int id,
            string workCenterCode,
            string aName,
            string eName,
            int branchID,
            decimal capacityPerDay,
            bool isActive,
            string notes,
            int modificationUserId,
            int companyID)
        {
            try
            {
                clsSQL clsSQL = new clsSQL();
                SqlParameter[] prm =
                {
                    new SqlParameter("@ID", SqlDbType.Int) { Value = id },
                    new SqlParameter("@WorkCenterCode", SqlDbType.NVarChar, -1) { Value = workCenterCode },
                    new SqlParameter("@AName", SqlDbType.NVarChar, -1) { Value = aName },
                    new SqlParameter("@EName", SqlDbType.NVarChar, -1) { Value = eName },
                    new SqlParameter("@BranchID", SqlDbType.Int) { Value = branchID },
                    new SqlParameter("@CapacityPerDay", SqlDbType.Decimal) { Value = capacityPerDay },
                    new SqlParameter("@IsActive", SqlDbType.Bit) { Value = isActive },
                    new SqlParameter("@Notes", SqlDbType.NVarChar, -1) { Value = notes ?? "" },
                    new SqlParameter("@ModificationUserId", SqlDbType.Int) { Value = modificationUserId },
                    new SqlParameter("@ModificationDate", SqlDbType.DateTime) { Value = DateTime.Now },
                    new SqlParameter("@CompanyID", SqlDbType.Int) { Value = companyID },
                };

                return clsSQL.ExecuteNonQueryStatement(@"
                    UPDATE tbl_WorkCenter SET
                        WorkCenterCode = @WorkCenterCode,
                        AName = @AName,
                        EName = @EName,
                        BranchID = @BranchID,
                        CapacityPerDay = @CapacityPerDay,
                        IsActive = @IsActive,
                        Notes = @Notes,
                        ModificationUserId = @ModificationUserId,
                        ModificationDate = @ModificationDate
                    WHERE ID = @ID AND CompanyID = @CompanyID
                ", clsSQL.CreateDataBaseConnectionString(companyID), prm);
            }
            catch (Exception)
            {
                throw;
            }
        }
    }
}
