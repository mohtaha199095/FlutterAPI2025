using Microsoft.Data.SqlClient;
using System;
using System.Data;

namespace WebApplication2.cls
{
    public class clsCRMPipeline
    {
        public DataTable SelectCRMPipeline(int ID, int CompanyID)
        {
            SqlParameter[] prm =
            {
                new SqlParameter("@ID", SqlDbType.Int) { Value = ID },
                new SqlParameter("@CompanyID", SqlDbType.Int) { Value = CompanyID },
            };
            clsSQL clsSQL = new clsSQL();
            return clsSQL.ExecuteQueryStatement(
                @"SELECT * FROM tbl_CRMPipeline
                  WHERE (ID = @ID OR @ID = 0)
                    AND CompanyID = @CompanyID
                    AND IsActive = 1
                  ORDER BY IsDefault DESC, ID",
                clsSQL.CreateDataBaseConnectionString(CompanyID), prm);
        }

        public int EnsureDefaultPipeline(int CompanyID, int CreationUserID)
        {
            clsSQL clsSQL = new clsSQL();
            string conn = clsSQL.CreateDataBaseConnectionString(CompanyID);
            DataTable existing = clsSQL.ExecuteQueryStatement(
                @"SELECT TOP 1 ID FROM tbl_CRMPipeline WHERE CompanyID = @CompanyID AND IsDefault = 1",
                conn,
                new[] { new SqlParameter("@CompanyID", SqlDbType.Int) { Value = CompanyID } });

            int pipelineId;
            if (existing != null && existing.Rows.Count > 0)
            {
                pipelineId = Simulate.Integer32(existing.Rows[0]["ID"]);
            }
            else
            {
                SqlParameter[] prm =
                {
                    new SqlParameter("@CompanyID", SqlDbType.Int) { Value = CompanyID },
                    new SqlParameter("@AName", SqlDbType.NVarChar, -1) { Value = "مسار المبيعات" },
                    new SqlParameter("@EName", SqlDbType.NVarChar, -1) { Value = "Sales Pipeline" },
                    new SqlParameter("@IsDefault", SqlDbType.Bit) { Value = true },
                    new SqlParameter("@IsActive", SqlDbType.Bit) { Value = true },
                    new SqlParameter("@CreationUserID", SqlDbType.Int) { Value = CreationUserID },
                    new SqlParameter("@CreationDate", SqlDbType.DateTime) { Value = DateTime.Now },
                };
                pipelineId = Simulate.Integer32(clsSQL.ExecuteScalar(
                    @"INSERT INTO tbl_CRMPipeline (CompanyID, AName, EName, IsDefault, IsActive, CreationUserID, CreationDate)
                      OUTPUT INSERTED.ID
                      VALUES (@CompanyID, @AName, @EName, @IsDefault, @IsActive, @CreationUserID, @CreationDate)",
                    prm, conn));

                clsCRMJourneyStage stages = new clsCRMJourneyStage();
                stages.SeedDefaultStages(pipelineId, CompanyID, CreationUserID);

                MigrateLegacyLeads(pipelineId, CompanyID, CreationUserID);
            }

            return pipelineId;
        }

        public int InsertCRMPipeline(string AName, string EName, bool IsDefault, int CompanyID, int CreationUserID)
        {
            clsSQL clsSQL = new clsSQL();
            string conn = clsSQL.CreateDataBaseConnectionString(CompanyID);
            if (IsDefault)
            {
                clsSQL.ExecuteNonQueryStatement(
                    @"UPDATE tbl_CRMPipeline SET IsDefault = 0 WHERE CompanyID = @CompanyID",
                    conn,
                    new[] { new SqlParameter("@CompanyID", SqlDbType.Int) { Value = CompanyID } });
            }

            SqlParameter[] prm =
            {
                new SqlParameter("@CompanyID", SqlDbType.Int) { Value = CompanyID },
                new SqlParameter("@AName", SqlDbType.NVarChar, -1) { Value = AName ?? "" },
                new SqlParameter("@EName", SqlDbType.NVarChar, -1) { Value = EName ?? "" },
                new SqlParameter("@IsDefault", SqlDbType.Bit) { Value = IsDefault },
                new SqlParameter("@IsActive", SqlDbType.Bit) { Value = true },
                new SqlParameter("@CreationUserID", SqlDbType.Int) { Value = CreationUserID },
                new SqlParameter("@CreationDate", SqlDbType.DateTime) { Value = DateTime.Now },
            };
            int pipelineId = Simulate.Integer32(clsSQL.ExecuteScalar(
                @"INSERT INTO tbl_CRMPipeline (CompanyID, AName, EName, IsDefault, IsActive, CreationUserID, CreationDate)
                  OUTPUT INSERTED.ID
                  VALUES (@CompanyID, @AName, @EName, @IsDefault, @IsActive, @CreationUserID, @CreationDate)",
                prm, conn));

            if (pipelineId > 0)
            {
                clsCRMJourneyStage stages = new clsCRMJourneyStage();
                stages.SeedDefaultStages(pipelineId, CompanyID, CreationUserID);
            }
            return pipelineId;
        }

        public int UpdateCRMPipeline(int ID, string AName, string EName, bool IsDefault, bool IsActive,
            int ModificationUserID, int CompanyID)
        {
            clsSQL clsSQL = new clsSQL();
            string conn = clsSQL.CreateDataBaseConnectionString(CompanyID);
            if (IsDefault)
            {
                clsSQL.ExecuteNonQueryStatement(
                    @"UPDATE tbl_CRMPipeline SET IsDefault = 0 WHERE CompanyID = @CompanyID AND ID <> @ID",
                    conn,
                    new[]
                    {
                        new SqlParameter("@CompanyID", SqlDbType.Int) { Value = CompanyID },
                        new SqlParameter("@ID", SqlDbType.Int) { Value = ID },
                    });
            }

            SqlParameter[] prm =
            {
                new SqlParameter("@ID", SqlDbType.Int) { Value = ID },
                new SqlParameter("@AName", SqlDbType.NVarChar, -1) { Value = AName ?? "" },
                new SqlParameter("@EName", SqlDbType.NVarChar, -1) { Value = EName ?? "" },
                new SqlParameter("@IsDefault", SqlDbType.Bit) { Value = IsDefault },
                new SqlParameter("@IsActive", SqlDbType.Bit) { Value = IsActive },
                new SqlParameter("@ModificationUserID", SqlDbType.Int) { Value = ModificationUserID },
                new SqlParameter("@ModificationDate", SqlDbType.DateTime) { Value = DateTime.Now },
                new SqlParameter("@CompanyID", SqlDbType.Int) { Value = CompanyID },
            };
            return clsSQL.ExecuteNonQueryStatement(
                @"UPDATE tbl_CRMPipeline SET
                    AName=@AName, EName=@EName, IsDefault=@IsDefault, IsActive=@IsActive,
                    ModificationUserID=@ModificationUserID, ModificationDate=@ModificationDate
                  WHERE ID=@ID AND CompanyID=@CompanyID",
                conn, prm);
        }

        public bool DeleteCRMPipelineByID(int ID, int CompanyID)
        {
            clsSQL clsSQL = new clsSQL();
            string conn = clsSQL.CreateDataBaseConnectionString(CompanyID);
            DataTable check = clsSQL.ExecuteQueryStatement(
                @"SELECT IsDefault FROM tbl_CRMPipeline WHERE ID=@ID AND CompanyID=@CompanyID",
                conn,
                new[]
                {
                    new SqlParameter("@ID", SqlDbType.Int) { Value = ID },
                    new SqlParameter("@CompanyID", SqlDbType.Int) { Value = CompanyID },
                });
            if (check == null || check.Rows.Count == 0) return false;
            if (Simulate.Bool(check.Rows[0]["IsDefault"]))
                return false;

            DataTable oppCount = clsSQL.ExecuteQueryStatement(
                @"SELECT COUNT(*) AS C FROM tbl_CRMOpportunity WHERE PipelineID=@ID AND CompanyID=@CompanyID AND IsActive=1",
                conn,
                new[]
                {
                    new SqlParameter("@ID", SqlDbType.Int) { Value = ID },
                    new SqlParameter("@CompanyID", SqlDbType.Int) { Value = CompanyID },
                });
            if (oppCount != null && oppCount.Rows.Count > 0 && Simulate.Integer32(oppCount.Rows[0]["C"]) > 0)
                return false;

            return clsSQL.ExecuteNonQueryStatement(
                @"UPDATE tbl_CRMPipeline SET IsActive = 0 WHERE ID=@ID AND CompanyID=@CompanyID",
                conn,
                new[]
                {
                    new SqlParameter("@ID", SqlDbType.Int) { Value = ID },
                    new SqlParameter("@CompanyID", SqlDbType.Int) { Value = CompanyID },
                }) > 0;
        }

        void MigrateLegacyLeads(int pipelineId, int CompanyID, int CreationUserID)
        {
            clsSQL clsSQL = new clsSQL();
            string conn = clsSQL.CreateDataBaseConnectionString(CompanyID);

            DataTable stageDt = clsSQL.ExecuteQueryStatement(
                @"SELECT TOP 1 ID FROM tbl_CRMJourneyStage
                  WHERE CompanyID = @CompanyID AND PipelineID = @PipelineID AND IsDefault = 1",
                conn,
                new[]
                {
                    new SqlParameter("@CompanyID", SqlDbType.Int) { Value = CompanyID },
                    new SqlParameter("@PipelineID", SqlDbType.Int) { Value = pipelineId },
                });
            if (stageDt == null || stageDt.Rows.Count == 0) return;

            int defaultStageId = Simulate.Integer32(stageDt.Rows[0]["ID"]);

            clsSQL.ExecuteNonQueryStatement(@"
INSERT INTO tbl_CRMOpportunity
    (PipelineID, StageID, Title, AName, EName, Tel1, Email, Country, Source, Notes,
     IsActive, CompanyID, CreationUserID, CreationDate)
SELECT
    @PipelineID, @StageID,
    ISNULL(NULLIF(L.AName, ''), N'Lead'),
    L.AName, L.AName, L.Tel1, L.Email, L.Country, N'Website', L.Note,
    1, @CompanyID, @CreationUserID, ISNULL(L.CreationDate, GETDATE())
FROM tbl_Leads L
WHERE (L.CRMOpportunityID IS NULL OR L.CRMOpportunityID = 0)
  AND NOT EXISTS (
      SELECT 1 FROM tbl_CRMOpportunity O
      WHERE O.CompanyID = @CompanyID AND O.Tel1 = L.Tel1 AND O.Email = L.Email
        AND L.Tel1 IS NOT NULL AND L.Tel1 <> ''
  );",
                conn,
                new[]
                {
                    new SqlParameter("@PipelineID", SqlDbType.Int) { Value = pipelineId },
                    new SqlParameter("@StageID", SqlDbType.Int) { Value = defaultStageId },
                    new SqlParameter("@CompanyID", SqlDbType.Int) { Value = CompanyID },
                    new SqlParameter("@CreationUserID", SqlDbType.Int) { Value = CreationUserID },
                });

            clsSQL.ExecuteNonQueryStatement(@"
UPDATE L SET L.CRMOpportunityID = O.ID
FROM tbl_Leads L
INNER JOIN tbl_CRMOpportunity O ON O.CompanyID = @CompanyID
    AND O.Tel1 = L.Tel1 AND O.Email = L.Email AND O.Source = N'Website'
WHERE L.CRMOpportunityID IS NULL OR L.CRMOpportunityID = 0;",
                conn,
                new[] { new SqlParameter("@CompanyID", SqlDbType.Int) { Value = CompanyID } });
        }
    }
}
