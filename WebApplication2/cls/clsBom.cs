using System;
using System.Data;
using Microsoft.Data.SqlClient;

namespace WebApplication2.cls
{
    public class clsBOM
    {
        // =========================================================
        // HEADER
        // =========================================================

        public DataTable SelectBOMHeader(int Id, string BOMCode, string BOMName, int CompanyID)
        {
            try
            {
                SqlParameter[] prm =
                {
                    new SqlParameter("@Id", SqlDbType.Int) { Value = Id },
                    new SqlParameter("@BOMCode", SqlDbType.NVarChar, -1) { Value = BOMCode ?? "" },
                    new SqlParameter("@BOMName", SqlDbType.NVarChar, -1) { Value = BOMName ?? "" },
                    new SqlParameter("@CompanyID", SqlDbType.Int) { Value = CompanyID },
                };

                clsSQL clsSQL = new clsSQL();
                DataTable dt = clsSQL.ExecuteQueryStatement(@"
                    select * from tbl_BOMHeader
                    where (id=@Id or @Id=0)
                      and (BOMCode=@BOMCode or @BOMCode='')
                      and (BOMName=@BOMName or @BOMName='')
                      and (CompanyID=@CompanyID or @CompanyID=0)
                    order by id desc
                ", clsSQL.CreateDataBaseConnectionString(CompanyID), prm);

                return dt;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public bool DeleteBOMByID(int Id, int CompanyID, SqlTransaction trn = null)
        {
            try
            {
                clsSQL clsSQL = new clsSQL();

                SqlParameter[] prm =
                {
                    new SqlParameter("@Id", SqlDbType.Int) { Value = Id },
                };

                if (trn == null)
                {
                    // delete children first
                    clsSQL.ExecuteNonQueryStatement(@"delete from tbl_BOMInput where BOMID=@Id", clsSQL.CreateDataBaseConnectionString(CompanyID), prm);
                    clsSQL.ExecuteNonQueryStatement(@"delete from tbl_BOMOutput where BOMID=@Id", clsSQL.CreateDataBaseConnectionString(CompanyID), prm);
                    clsSQL.ExecuteNonQueryStatement(@"delete from tbl_BOMHeader where id=@Id", clsSQL.CreateDataBaseConnectionString(CompanyID), prm);
                }
                else
                {
                    clsSQL.ExecuteNonQueryStatement(@"delete from tbl_BOMInput where BOMID=@Id", clsSQL.CreateDataBaseConnectionString(CompanyID), prm, trn);
                    clsSQL.ExecuteNonQueryStatement(@"delete from tbl_BOMOutput where BOMID=@Id", clsSQL.CreateDataBaseConnectionString(CompanyID), prm, trn);
                    clsSQL.ExecuteNonQueryStatement(@"delete from tbl_BOMHeader where id=@Id", clsSQL.CreateDataBaseConnectionString(CompanyID), prm, trn);
                }

                return true;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public int InsertBOMHeader(
            string BOMCode,
            string BOMName,
            decimal BatchQty,
            int VersionNo,
            bool IsDefault,
            bool IsActive,
            DateTime? EffectiveFrom,
            DateTime? EffectiveTo,
            string Notes,
            int CompanyID,
            int CreationUserId,
            SqlTransaction trn = null)
        {
            try
            {
                SqlParameter[] prm =
                {
                    new SqlParameter("@BOMCode", SqlDbType.NVarChar, -1) { Value = BOMCode ?? "" },
                    new SqlParameter("@BOMName", SqlDbType.NVarChar, -1) { Value = BOMName ?? "" },

                    new SqlParameter("@BatchQty", SqlDbType.Decimal) { Value = BatchQty },
                    new SqlParameter("@VersionNo", SqlDbType.Int) { Value = VersionNo },

                    new SqlParameter("@IsDefault", SqlDbType.Bit) { Value = IsDefault },
                    new SqlParameter("@IsActive", SqlDbType.Bit) { Value = IsActive },

                    new SqlParameter("@EffectiveFrom", SqlDbType.DateTime) { Value = (object?)EffectiveFrom ?? DBNull.Value },
                    new SqlParameter("@EffectiveTo", SqlDbType.DateTime) { Value = (object?)EffectiveTo ?? DBNull.Value },

                    new SqlParameter("@Notes", SqlDbType.NVarChar, -1) { Value = Notes ?? "" },

                    new SqlParameter("@CompanyID", SqlDbType.Int) { Value = CompanyID },
                    new SqlParameter("@CreationUserId", SqlDbType.Int) { Value = CreationUserId },
                    new SqlParameter("@CreationDate", SqlDbType.DateTime) { Value = DateTime.Now },
                };

                string q = @"
                    insert into tbl_BOMHeader
                    (BOMCode,BOMName,BatchQty,VersionNo,IsDefault,IsActive,EffectiveFrom,EffectiveTo,Notes,CompanyID,CreationUserId,CreationDate)
                    output inserted.ID
                    values
                    (@BOMCode,@BOMName,@BatchQty,@VersionNo,@IsDefault,@IsActive,@EffectiveFrom,@EffectiveTo,@Notes,@CompanyID,@CreationUserId,@CreationDate)
                ";

                clsSQL clsSQL = new clsSQL();
                if (trn == null)
                    return Simulate.Integer32(clsSQL.ExecuteScalar(q, prm, clsSQL.CreateDataBaseConnectionString(CompanyID)));
                else
                    return Simulate.Integer32(clsSQL.ExecuteScalar(q, prm, clsSQL.CreateDataBaseConnectionString(CompanyID), trn));
            }
            catch (Exception)
            {
                throw;
            }
        }

        public int UpdateBOMHeader(
            int ID,
            string BOMCode,
            string BOMName,
            decimal BatchQty,
            int VersionNo,
            bool IsDefault,
            bool IsActive,
            DateTime? EffectiveFrom,
            DateTime? EffectiveTo,
            string Notes,
            int ModificationUserId,
            int CompanyID,
            SqlTransaction trn = null)
        {
            try
            {
                clsSQL clsSQL = new clsSQL();

                SqlParameter[] prm =
                {
                    new SqlParameter("@ID", SqlDbType.Int) { Value = ID },
                    new SqlParameter("@BOMCode", SqlDbType.NVarChar, -1) { Value = BOMCode ?? "" },
                    new SqlParameter("@BOMName", SqlDbType.NVarChar, -1) { Value = BOMName ?? "" },

                    new SqlParameter("@BatchQty", SqlDbType.Decimal) { Value = BatchQty },
                    new SqlParameter("@VersionNo", SqlDbType.Int) { Value = VersionNo },

                    new SqlParameter("@IsDefault", SqlDbType.Bit) { Value = IsDefault },
                    new SqlParameter("@IsActive", SqlDbType.Bit) { Value = IsActive },

                    new SqlParameter("@EffectiveFrom", SqlDbType.DateTime) { Value = (object?)EffectiveFrom ?? DBNull.Value },
                    new SqlParameter("@EffectiveTo", SqlDbType.DateTime) { Value = (object?)EffectiveTo ?? DBNull.Value },

                    new SqlParameter("@Notes", SqlDbType.NVarChar, -1) { Value = Notes ?? "" },

                    new SqlParameter("@ModificationUserId", SqlDbType.Int) { Value = ModificationUserId },
                    new SqlParameter("@ModificationDate", SqlDbType.DateTime) { Value = DateTime.Now },
                };

                string q = @"
                    update tbl_BOMHeader set
                        BOMCode=@BOMCode,
                        BOMName=@BOMName,
                        BatchQty=@BatchQty,
                        VersionNo=@VersionNo,
                        IsDefault=@IsDefault,
                        IsActive=@IsActive,
                        EffectiveFrom=@EffectiveFrom,
                        EffectiveTo=@EffectiveTo,
                        Notes=@Notes,
                        ModificationUserId=@ModificationUserId,
                        ModificationDate=@ModificationDate
                    where id=@ID
                ";

                if (trn == null)
                    return clsSQL.ExecuteNonQueryStatement(q, clsSQL.CreateDataBaseConnectionString(CompanyID), prm);
                else
                    return clsSQL.ExecuteNonQueryStatement(q, clsSQL.CreateDataBaseConnectionString(CompanyID), prm, trn);
            }
            catch (Exception)
            {
                throw;
            }
        }

        // =========================================================
        // INPUTS
        // =========================================================

        public DataTable SelectBOMInputsByBOMID(int BOMID, int CompanyID)
        {
            try
            {
                SqlParameter[] prm =
                {
                    new SqlParameter("@BOMID", SqlDbType.Int) { Value = BOMID },
                    new SqlParameter("@CompanyID", SqlDbType.Int) { Value = CompanyID },
                };

                clsSQL clsSQL = new clsSQL();
                return clsSQL.ExecuteQueryStatement(@"
                    select * from tbl_BOMInput
                    where BOMID=@BOMID and (CompanyID=@CompanyID or @CompanyID=0)
                    order by LineOrder, ID
                ", clsSQL.CreateDataBaseConnectionString(CompanyID), prm);
            }
            catch (Exception)
            {
                throw;
            }
        }

        public int DeleteBOMInputsByBOMID(int BOMID, int CompanyID, SqlTransaction trn = null)
        {
            try
            {
                clsSQL clsSQL = new clsSQL();
                SqlParameter[] prm = { new SqlParameter("@BOMID", SqlDbType.Int) { Value = BOMID } };

                if (trn == null)
                    return clsSQL.ExecuteNonQueryStatement(@"delete from tbl_BOMInput where BOMID=@BOMID", clsSQL.CreateDataBaseConnectionString(CompanyID), prm);
                else
                    return clsSQL.ExecuteNonQueryStatement(@"delete from tbl_BOMInput where BOMID=@BOMID", clsSQL.CreateDataBaseConnectionString(CompanyID), prm, trn);
            }
            catch (Exception)
            {
                throw;
            }
        }

        public int InsertBOMInput(
            int BOMID,
            Guid ComponentItemGuid,
            decimal Qty,
            int UOMID,
            int LineNo,
            decimal ScrapPercent,
            string Notes,
            int CompanyID,
            int CreationUserId,
            SqlTransaction trn = null,
            bool IsPhantom = false)
        {
            try
            {
                SqlParameter[] prm =
                {
                    new SqlParameter("@BOMID", SqlDbType.Int) { Value = BOMID },
                    new SqlParameter("@ComponentItemGuid", SqlDbType.UniqueIdentifier) { Value = ComponentItemGuid },
                    new SqlParameter("@Qty", SqlDbType.Decimal) { Value = Qty },
                    new SqlParameter("@UOMID", SqlDbType.Int) { Value = UOMID },
                    new SqlParameter("@LineOrder", SqlDbType.Int) { Value = LineNo },
                    new SqlParameter("@ScrapPercent", SqlDbType.Decimal) { Value = ScrapPercent },
                    new SqlParameter("@Notes", SqlDbType.NVarChar, -1) { Value = Notes ?? "" },
                    new SqlParameter("@IsPhantom", SqlDbType.Bit) { Value = IsPhantom },

                    new SqlParameter("@CompanyID", SqlDbType.Int) { Value = CompanyID },
                    new SqlParameter("@CreationUserId", SqlDbType.Int) { Value = CreationUserId },
                    new SqlParameter("@CreationDate", SqlDbType.DateTime) { Value = DateTime.Now },
                };

                string q = @"
                    insert into tbl_BOMInput
                    (BOMID,ComponentItemGuid,Qty,UOMID,LineOrder,ScrapPercent,Notes,IsPhantom,CompanyID,CreationUserId,CreationDate)
                    output inserted.ID
                    values
                    (@BOMID,@ComponentItemGuid,@Qty,@UOMID,@LineOrder,@ScrapPercent,@Notes,@IsPhantom,@CompanyID,@CreationUserId,@CreationDate)
                ";

                clsSQL clsSQL = new clsSQL();
                if (trn == null)
                    return Simulate.Integer32(clsSQL.ExecuteScalar(q, prm, clsSQL.CreateDataBaseConnectionString(CompanyID)));
                else
                    return Simulate.Integer32(clsSQL.ExecuteScalar(q, prm, clsSQL.CreateDataBaseConnectionString(CompanyID), trn));
            }
            catch (Exception)
            {
                throw;
            }
        }

        public DataTable SelectBOMRoutingByBOMID(int bomId, int companyID)
        {
            try
            {
                SqlParameter[] prm =
                {
                    new SqlParameter("@BOMID", SqlDbType.Int) { Value = bomId },
                    new SqlParameter("@CompanyID", SqlDbType.Int) { Value = companyID },
                };
                clsSQL clsSQL = new clsSQL();
                return clsSQL.ExecuteQueryStatement(@"
                    SELECT r.*, wc.AName AS WorkCenterName
                    FROM tbl_BOMRouting r
                    LEFT JOIN tbl_WorkCenter wc ON wc.ID = r.WorkCenterID AND wc.CompanyID = r.CompanyID
                    WHERE r.BOMID = @BOMID AND r.CompanyID = @CompanyID
                    ORDER BY r.LineNo
                ", clsSQL.CreateDataBaseConnectionString(companyID), prm);
            }
            catch (Exception)
            {
                throw;
            }
        }

        public int DeleteBOMRoutingByBOMID(int bomId, int companyID, SqlTransaction trn = null)
        {
            try
            {
                clsSQL clsSQL = new clsSQL();
                SqlParameter[] prm =
                {
                    new SqlParameter("@BOMID", SqlDbType.Int) { Value = bomId },
                    new SqlParameter("@CompanyID", SqlDbType.Int) { Value = companyID },
                };
                string sql = "DELETE FROM tbl_BOMRouting WHERE BOMID = @BOMID AND CompanyID = @CompanyID";
                if (trn == null)
                    return clsSQL.ExecuteNonQueryStatement(sql, clsSQL.CreateDataBaseConnectionString(companyID), prm);
                return clsSQL.ExecuteNonQueryStatement(sql, clsSQL.CreateDataBaseConnectionString(companyID), prm, trn);
            }
            catch (Exception)
            {
                throw;
            }
        }

        public int InsertBOMRouting(
            int bomId,
            int lineNo,
            int workCenterId,
            string operationName,
            decimal plannedHours,
            string notes,
            int companyID,
            int creationUserId,
            SqlTransaction trn = null)
        {
            try
            {
                SqlParameter[] prm =
                {
                    new SqlParameter("@BOMID", SqlDbType.Int) { Value = bomId },
                    new SqlParameter("@LineNo", SqlDbType.Int) { Value = lineNo },
                    new SqlParameter("@WorkCenterID", SqlDbType.Int) { Value = workCenterId },
                    new SqlParameter("@OperationName", SqlDbType.NVarChar, -1) { Value = operationName ?? "" },
                    new SqlParameter("@PlannedHours", SqlDbType.Decimal) { Value = plannedHours },
                    new SqlParameter("@Notes", SqlDbType.NVarChar, -1) { Value = notes ?? "" },
                    new SqlParameter("@CompanyID", SqlDbType.Int) { Value = companyID },
                    new SqlParameter("@CreationUserId", SqlDbType.Int) { Value = creationUserId },
                    new SqlParameter("@CreationDate", SqlDbType.DateTime) { Value = DateTime.Now },
                };
                string sql = @"
                    INSERT INTO tbl_BOMRouting
                    (BOMID, LineNo, WorkCenterID, OperationName, PlannedHours, Notes, CompanyID, CreationUserId, CreationDate)
                    OUTPUT INSERTED.ID
                    VALUES
                    (@BOMID, @LineNo, @WorkCenterID, @OperationName, @PlannedHours, @Notes, @CompanyID, @CreationUserId, @CreationDate)";
                clsSQL clsSQL = new clsSQL();
                if (trn == null)
                    return Simulate.Integer32(clsSQL.ExecuteScalar(sql, prm, clsSQL.CreateDataBaseConnectionString(companyID)));
                return Simulate.Integer32(clsSQL.ExecuteScalar(sql, prm, clsSQL.CreateDataBaseConnectionString(companyID), trn));
            }
            catch (Exception)
            {
                throw;
            }
        }

        // =========================================================
        // OUTPUTS
        // =========================================================

        public DataTable SelectBOMOutputsByBOMID(int BOMID, int CompanyID)
        {
            try
            {
                SqlParameter[] prm =
                {
                    new SqlParameter("@BOMID", SqlDbType.Int) { Value = BOMID },
                    new SqlParameter("@CompanyID", SqlDbType.Int) { Value = CompanyID },
                };

                clsSQL clsSQL = new clsSQL();
                return clsSQL.ExecuteQueryStatement(@"
                    select * from tbl_BOMOutput
                    where BOMID=@BOMID and (CompanyID=@CompanyID or @CompanyID=0)
                    order by LineOrder, ID
                ", clsSQL.CreateDataBaseConnectionString(CompanyID), prm);
            }
            catch (Exception)
            {
                throw;
            }
        }

        public int DeleteBOMOutputsByBOMID(int BOMID, int CompanyID, SqlTransaction trn = null)
        {
            try
            {
                clsSQL clsSQL = new clsSQL();
                SqlParameter[] prm = { new SqlParameter("@BOMID", SqlDbType.Int) { Value = BOMID } };

                if (trn == null)
                    return clsSQL.ExecuteNonQueryStatement(@"delete from tbl_BOMOutput where BOMID=@BOMID", clsSQL.CreateDataBaseConnectionString(CompanyID), prm);
                else
                    return clsSQL.ExecuteNonQueryStatement(@"delete from tbl_BOMOutput where BOMID=@BOMID", clsSQL.CreateDataBaseConnectionString(CompanyID), prm, trn);
            }
            catch (Exception)
            {
                throw;
            }
        }

        public int InsertBOMOutput(
            int BOMID,
            Guid OutputItemGuid,
            decimal Qty,
            int UOMID,
            decimal CostSharePercent,
            int LineNo,
            string Notes,
            int CompanyID,
            int CreationUserId,
            SqlTransaction trn = null)
        {
            try
            {
                SqlParameter[] prm =
                {
                    new SqlParameter("@BOMID", SqlDbType.Int) { Value = BOMID },
                    new SqlParameter("@OutputItemGuid", SqlDbType.UniqueIdentifier) { Value = OutputItemGuid },

                    new SqlParameter("@Qty", SqlDbType.Decimal) { Value = Qty },
                    new SqlParameter("@UOMID", SqlDbType.Int) { Value = UOMID },
                    new SqlParameter("@CostSharePercent", SqlDbType.Decimal) { Value = CostSharePercent },

                    new SqlParameter("@LineOrder", SqlDbType.Int) { Value = LineNo },
                    new SqlParameter("@Notes", SqlDbType.NVarChar, -1) { Value = Notes ?? "" },

                    new SqlParameter("@CompanyID", SqlDbType.Int) { Value = CompanyID },
                    new SqlParameter("@CreationUserId", SqlDbType.Int) { Value = CreationUserId },
                    new SqlParameter("@CreationDate", SqlDbType.DateTime) { Value = DateTime.Now },
                };

                string q = @"
                    insert into tbl_BOMOutput
                    (BOMID,OutputItemGuid,Qty,UOMID,CostSharePercent,LineOrder,Notes,CompanyID,CreationUserId,CreationDate)
                    output inserted.ID
                    values
                    (@BOMID,@OutputItemGuid,@Qty,@UOMID,@CostSharePercent,@LineOrder,@Notes,@CompanyID,@CreationUserId,@CreationDate)
                ";

                clsSQL clsSQL = new clsSQL();
                if (trn == null)
                    return Simulate.Integer32(clsSQL.ExecuteScalar(q, prm, clsSQL.CreateDataBaseConnectionString(CompanyID)));
                else
                    return Simulate.Integer32(clsSQL.ExecuteScalar(q, prm, clsSQL.CreateDataBaseConnectionString(CompanyID), trn));
            }
            catch (Exception)
            {
                throw;
            }
        }

         
    }
}
