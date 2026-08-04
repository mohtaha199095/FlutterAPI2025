using Microsoft.Data.SqlClient;
using System;
using System.Data;

namespace WebApplication2.cls
{
    public class clsCRMJourneyStage
    {
        public DataTable SelectCRMJourneyStageByID(int ID, int PipelineID, int CompanyID)
        {
            SqlParameter[] prm =
            {
                new SqlParameter("@ID", SqlDbType.Int) { Value = ID },
                new SqlParameter("@PipelineID", SqlDbType.Int) { Value = PipelineID },
                new SqlParameter("@CompanyID", SqlDbType.Int) { Value = CompanyID },
            };
            clsSQL clsSQL = new clsSQL();
            return clsSQL.ExecuteQueryStatement(
                @"SELECT * FROM tbl_CRMJourneyStage
                  WHERE (ID = @ID OR @ID = 0)
                    AND (PipelineID = @PipelineID OR @PipelineID = 0)
                    AND CompanyID = @CompanyID
                    AND IsActive = 1
                  ORDER BY StageOrder, ID",
                clsSQL.CreateDataBaseConnectionString(CompanyID), prm);
        }

        public void SeedDefaultStages(int PipelineID, int CompanyID, int CreationUserID)
        {
            clsSQL clsSQL = new clsSQL();
            string conn = clsSQL.CreateDataBaseConnectionString(CompanyID);
            DataTable check = clsSQL.ExecuteQueryStatement(
                @"SELECT TOP 1 ID FROM tbl_CRMJourneyStage WHERE PipelineID = @PipelineID AND CompanyID = @CompanyID",
                conn,
                new[]
                {
                    new SqlParameter("@PipelineID", SqlDbType.Int) { Value = PipelineID },
                    new SqlParameter("@CompanyID", SqlDbType.Int) { Value = CompanyID },
                });
            if (check != null && check.Rows.Count > 0) return;

            var defaults = new[]
            {
                new { Order = 1, AName = "عميل محتمل", EName = "Lead", Color = "#3498db", IsDefault = true, IsWon = false, IsLost = false },
                new { Order = 2, AName = "تم التواصل", EName = "Contacted", Color = "#9b59b6", IsDefault = false, IsWon = false, IsLost = false },
                new { Order = 3, AName = "تم التحقق", EName = "Verified", Color = "#f39c12", IsDefault = false, IsWon = false, IsLost = false },
                new { Order = 4, AName = "عرض", EName = "Proposal", Color = "#1abc9c", IsDefault = false, IsWon = false, IsLost = false },
                new { Order = 5, AName = "نجاح", EName = "Success", Color = "#27ae60", IsDefault = false, IsWon = true, IsLost = false },
                new { Order = 6, AName = "خسارة", EName = "Lost", Color = "#e74c3c", IsDefault = false, IsWon = false, IsLost = true },
            };

            foreach (var s in defaults)
            {
                InsertCRMJourneyStage(PipelineID, s.AName, s.EName, s.Order, s.Color, s.IsWon, s.IsLost, s.IsDefault, CompanyID, CreationUserID);
            }
        }

        public int InsertCRMJourneyStage(int PipelineID, string AName, string EName, int StageOrder,
            string Color, bool IsWon, bool IsLost, bool IsDefault, int CompanyID, int CreationUserID,
            SqlTransaction trn = null)
        {
            SqlParameter[] prm =
            {
                new SqlParameter("@PipelineID", SqlDbType.Int) { Value = PipelineID },
                new SqlParameter("@AName", SqlDbType.NVarChar, -1) { Value = AName },
                new SqlParameter("@EName", SqlDbType.NVarChar, -1) { Value = EName },
                new SqlParameter("@StageOrder", SqlDbType.Int) { Value = StageOrder },
                new SqlParameter("@Color", SqlDbType.NVarChar, 20) { Value = Color ?? "#3498db" },
                new SqlParameter("@IsWon", SqlDbType.Bit) { Value = IsWon },
                new SqlParameter("@IsLost", SqlDbType.Bit) { Value = IsLost },
                new SqlParameter("@IsDefault", SqlDbType.Bit) { Value = IsDefault },
                new SqlParameter("@IsActive", SqlDbType.Bit) { Value = true },
                new SqlParameter("@CompanyID", SqlDbType.Int) { Value = CompanyID },
                new SqlParameter("@CreationUserID", SqlDbType.Int) { Value = CreationUserID },
                new SqlParameter("@CreationDate", SqlDbType.DateTime) { Value = DateTime.Now },
            };
            string sql = @"INSERT INTO tbl_CRMJourneyStage
                (PipelineID, AName, EName, StageOrder, Color, IsWon, IsLost, IsDefault, IsActive, CompanyID, CreationUserID, CreationDate)
                OUTPUT INSERTED.ID
                VALUES (@PipelineID, @AName, @EName, @StageOrder, @Color, @IsWon, @IsLost, @IsDefault, @IsActive, @CompanyID, @CreationUserID, @CreationDate)";
            clsSQL clsSQL = new clsSQL();
            if (trn == null)
                return Simulate.Integer32(clsSQL.ExecuteScalar(sql, prm, clsSQL.CreateDataBaseConnectionString(CompanyID)));
            return Simulate.Integer32(clsSQL.ExecuteScalar(sql, prm, clsSQL.CreateDataBaseConnectionString(CompanyID), trn));
        }

        public int UpdateCRMJourneyStage(int ID, string AName, string EName, int StageOrder, string Color,
            bool IsWon, bool IsLost, bool IsDefault, bool IsActive, int ModificationUserID, int CompanyID)
        {
            SqlParameter[] prm =
            {
                new SqlParameter("@ID", SqlDbType.Int) { Value = ID },
                new SqlParameter("@AName", SqlDbType.NVarChar, -1) { Value = AName },
                new SqlParameter("@EName", SqlDbType.NVarChar, -1) { Value = EName },
                new SqlParameter("@StageOrder", SqlDbType.Int) { Value = StageOrder },
                new SqlParameter("@Color", SqlDbType.NVarChar, 20) { Value = Color ?? "#3498db" },
                new SqlParameter("@IsWon", SqlDbType.Bit) { Value = IsWon },
                new SqlParameter("@IsLost", SqlDbType.Bit) { Value = IsLost },
                new SqlParameter("@IsDefault", SqlDbType.Bit) { Value = IsDefault },
                new SqlParameter("@IsActive", SqlDbType.Bit) { Value = IsActive },
                new SqlParameter("@ModificationUserID", SqlDbType.Int) { Value = ModificationUserID },
                new SqlParameter("@ModificationDate", SqlDbType.DateTime) { Value = DateTime.Now },
                new SqlParameter("@CompanyID", SqlDbType.Int) { Value = CompanyID },
            };
            clsSQL clsSQL = new clsSQL();
            return clsSQL.ExecuteNonQueryStatement(
                @"UPDATE tbl_CRMJourneyStage SET
                    AName=@AName, EName=@EName, StageOrder=@StageOrder, Color=@Color,
                    IsWon=@IsWon, IsLost=@IsLost, IsDefault=@IsDefault, IsActive=@IsActive,
                    ModificationUserID=@ModificationUserID, ModificationDate=@ModificationDate
                  WHERE ID=@ID AND CompanyID=@CompanyID",
                clsSQL.CreateDataBaseConnectionString(CompanyID), prm);
        }

        public bool DeleteCRMJourneyStageByID(int ID, int CompanyID)
        {
            clsSQL clsSQL = new clsSQL();
            SqlParameter[] prm =
            {
                new SqlParameter("@ID", SqlDbType.Int) { Value = ID },
                new SqlParameter("@CompanyID", SqlDbType.Int) { Value = CompanyID },
            };
            clsSQL.ExecuteNonQueryStatement(
                @"UPDATE tbl_CRMJourneyStage SET IsActive=0, ModificationDate=GETDATE()
                  WHERE ID=@ID AND CompanyID=@CompanyID",
                clsSQL.CreateDataBaseConnectionString(CompanyID), prm);
            return true;
        }

        public bool ReorderCRMJourneyStages(string orderedStageIds, int CompanyID, int ModificationUserID)
        {
            if (string.IsNullOrWhiteSpace(orderedStageIds)) return false;
            string[] ids = orderedStageIds.Split(',', StringSplitOptions.RemoveEmptyEntries);
            clsSQL clsSQL = new clsSQL();
            string conn = clsSQL.CreateDataBaseConnectionString(CompanyID);
            for (int i = 0; i < ids.Length; i++)
            {
                int stageId = Simulate.Integer32(ids[i].Trim());
                if (stageId <= 0) continue;
                SqlParameter[] prm =
                {
                    new SqlParameter("@ID", SqlDbType.Int) { Value = stageId },
                    new SqlParameter("@StageOrder", SqlDbType.Int) { Value = i + 1 },
                    new SqlParameter("@ModificationUserID", SqlDbType.Int) { Value = ModificationUserID },
                    new SqlParameter("@CompanyID", SqlDbType.Int) { Value = CompanyID },
                };
                clsSQL.ExecuteNonQueryStatement(
                    @"UPDATE tbl_CRMJourneyStage SET StageOrder=@StageOrder, ModificationUserID=@ModificationUserID, ModificationDate=GETDATE()
                      WHERE ID=@ID AND CompanyID=@CompanyID",
                    conn, prm);
            }
            return true;
        }

        public DataTable GetStageByID(int StageID, int CompanyID, SqlTransaction trn = null)
        {
            SqlParameter[] prm =
            {
                new SqlParameter("@ID", SqlDbType.Int) { Value = StageID },
                new SqlParameter("@CompanyID", SqlDbType.Int) { Value = CompanyID },
            };
            clsSQL clsSQL = new clsSQL();
            string sql = @"SELECT * FROM tbl_CRMJourneyStage WHERE ID=@ID AND CompanyID=@CompanyID";
            if (trn == null)
                return clsSQL.ExecuteQueryStatement(sql, clsSQL.CreateDataBaseConnectionString(CompanyID), prm);
            return clsSQL.ExecuteQueryStatement(sql, clsSQL.CreateDataBaseConnectionString(CompanyID), prm, trn);
        }
    }
}
