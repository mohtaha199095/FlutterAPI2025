using Microsoft.Data.SqlClient;
using System;
using System.Data;

namespace WebApplication2.cls
{
    public class clsCRMOpportunity
    {
        public DataTable SelectCRMOpportunityByID(int ID, int PipelineID, int StageID, int CompanyID)
        {
            SqlParameter[] prm =
            {
                new SqlParameter("@ID", SqlDbType.Int) { Value = ID },
                new SqlParameter("@PipelineID", SqlDbType.Int) { Value = PipelineID },
                new SqlParameter("@StageID", SqlDbType.Int) { Value = StageID },
                new SqlParameter("@CompanyID", SqlDbType.Int) { Value = CompanyID },
            };
            clsSQL clsSQL = new clsSQL();
            return clsSQL.ExecuteQueryStatement(
                @"SELECT O.*,
                         S.AName AS StageAName, S.EName AS StageEName, S.Color AS StageColor,
                         S.IsWon AS StageIsWon, S.IsLost AS StageIsLost,
                         E.AName AS AssignedUserAName, E.EName AS AssignedUserEName
                  FROM tbl_CRMOpportunity O
                  LEFT JOIN tbl_CRMJourneyStage S ON S.ID = O.StageID
                  LEFT JOIN tbl_employee E ON E.ID = O.AssignedUserID
                  WHERE (O.ID = @ID OR @ID = 0)
                    AND (O.PipelineID = @PipelineID OR @PipelineID = 0)
                    AND (O.StageID = @StageID OR @StageID = 0)
                    AND O.CompanyID = @CompanyID
                    AND O.IsActive = 1
                  ORDER BY O.ModificationDate DESC, O.ID DESC",
                clsSQL.CreateDataBaseConnectionString(CompanyID), prm);
        }

        public DataTable SelectCRMOpportunityByStage(int PipelineID, int CompanyID)
        {
            SqlParameter[] prm =
            {
                new SqlParameter("@PipelineID", SqlDbType.Int) { Value = PipelineID },
                new SqlParameter("@CompanyID", SqlDbType.Int) { Value = CompanyID },
            };
            clsSQL clsSQL = new clsSQL();
            return clsSQL.ExecuteQueryStatement(
                @"SELECT O.*,
                         S.AName AS StageAName, S.EName AS StageEName, S.Color AS StageColor,
                         S.StageOrder, S.IsWon AS StageIsWon, S.IsLost AS StageIsLost
                  FROM tbl_CRMOpportunity O
                  INNER JOIN tbl_CRMJourneyStage S ON S.ID = O.StageID AND S.IsActive = 1
                  WHERE O.PipelineID = @PipelineID
                    AND O.CompanyID = @CompanyID
                    AND O.IsActive = 1
                  ORDER BY S.StageOrder, O.ModificationDate DESC",
                clsSQL.CreateDataBaseConnectionString(CompanyID), prm);
        }

        public int GetDefaultStageID(int PipelineID, int CompanyID, SqlTransaction trn = null)
        {
            SqlParameter[] prm =
            {
                new SqlParameter("@PipelineID", SqlDbType.Int) { Value = PipelineID },
                new SqlParameter("@CompanyID", SqlDbType.Int) { Value = CompanyID },
            };
            clsSQL clsSQL = new clsSQL();
            string sql = @"SELECT TOP 1 ID FROM tbl_CRMJourneyStage
                           WHERE PipelineID = @PipelineID AND CompanyID = @CompanyID AND IsDefault = 1 AND IsActive = 1
                           ORDER BY StageOrder";
            DataTable dt;
            if (trn == null)
                dt = clsSQL.ExecuteQueryStatement(sql, clsSQL.CreateDataBaseConnectionString(CompanyID), prm);
            else
                dt = clsSQL.ExecuteQueryStatement(sql, clsSQL.CreateDataBaseConnectionString(CompanyID), prm, trn);

            if (dt != null && dt.Rows.Count > 0)
                return Simulate.Integer32(dt.Rows[0]["ID"]);

            sql = @"SELECT TOP 1 ID FROM tbl_CRMJourneyStage
                    WHERE PipelineID = @PipelineID AND CompanyID = @CompanyID AND IsActive = 1
                    ORDER BY StageOrder";
            if (trn == null)
                dt = clsSQL.ExecuteQueryStatement(sql, clsSQL.CreateDataBaseConnectionString(CompanyID), prm);
            else
                dt = clsSQL.ExecuteQueryStatement(sql, clsSQL.CreateDataBaseConnectionString(CompanyID), prm, trn);

            return dt != null && dt.Rows.Count > 0 ? Simulate.Integer32(dt.Rows[0]["ID"]) : 0;
        }

        public int InsertCRMOpportunity(int PipelineID, int StageID, string Title, string AName, string EName,
            string Tel1, string Email, string Country, string Source, string Notes,
            int BusinessPartnerID, int AssignedUserID, decimal ExpectedValue, int CurrencyID,
            int Probability, DateTime ExpectedCloseDate, int Priority,
            int CompanyID, int CreationUserID, SqlTransaction trn = null)
        {
            if (StageID <= 0)
                StageID = GetDefaultStageID(PipelineID, CompanyID, trn);
            if (PipelineID <= 0)
            {
                clsCRMPipeline pipeline = new clsCRMPipeline();
                PipelineID = pipeline.EnsureDefaultPipeline(CompanyID, CreationUserID);
                if (StageID <= 0)
                    StageID = GetDefaultStageID(PipelineID, CompanyID, trn);
            }

            SqlParameter[] prm =
            {
                new SqlParameter("@PipelineID", SqlDbType.Int) { Value = PipelineID },
                new SqlParameter("@StageID", SqlDbType.Int) { Value = StageID },
                new SqlParameter("@Title", SqlDbType.NVarChar, -1) { Value = Title ?? AName ?? "" },
                new SqlParameter("@AName", SqlDbType.NVarChar, -1) { Value = AName ?? "" },
                new SqlParameter("@EName", SqlDbType.NVarChar, -1) { Value = EName ?? "" },
                new SqlParameter("@Tel1", SqlDbType.NVarChar, -1) { Value = Tel1 ?? "" },
                new SqlParameter("@Email", SqlDbType.NVarChar, -1) { Value = Email ?? "" },
                new SqlParameter("@Country", SqlDbType.NVarChar, -1) { Value = Country ?? "" },
                new SqlParameter("@Source", SqlDbType.NVarChar, -1) { Value = Source ?? "" },
                new SqlParameter("@Notes", SqlDbType.NVarChar, -1) { Value = Notes ?? "" },
                new SqlParameter("@BusinessPartnerID", SqlDbType.Int) { Value = BusinessPartnerID > 0 ? BusinessPartnerID : (object)DBNull.Value },
                new SqlParameter("@AssignedUserID", SqlDbType.Int) { Value = AssignedUserID > 0 ? AssignedUserID : (object)DBNull.Value },
                new SqlParameter("@ExpectedValue", SqlDbType.Decimal) { Value = ExpectedValue },
                new SqlParameter("@CurrencyID", SqlDbType.Int) { Value = CurrencyID > 0 ? CurrencyID : (object)DBNull.Value },
                new SqlParameter("@Probability", SqlDbType.Int) { Value = Probability },
                new SqlParameter("@ExpectedCloseDate", SqlDbType.DateTime) { Value = ExpectedCloseDate == DateTime.MinValue ? (object)DBNull.Value : ExpectedCloseDate },
                new SqlParameter("@Priority", SqlDbType.Int) { Value = Priority },
                new SqlParameter("@IsActive", SqlDbType.Bit) { Value = true },
                new SqlParameter("@CompanyID", SqlDbType.Int) { Value = CompanyID },
                new SqlParameter("@CreationUserID", SqlDbType.Int) { Value = CreationUserID },
                new SqlParameter("@CreationDate", SqlDbType.DateTime) { Value = DateTime.Now },
            };
            string sql = @"INSERT INTO tbl_CRMOpportunity
                (PipelineID, StageID, Title, AName, EName, Tel1, Email, Country, Source, Notes,
                 BusinessPartnerID, AssignedUserID, ExpectedValue, CurrencyID, Probability, ExpectedCloseDate,
                 Priority, IsActive, CompanyID, CreationUserID, CreationDate)
                OUTPUT INSERTED.ID
                VALUES (@PipelineID, @StageID, @Title, @AName, @EName, @Tel1, @Email, @Country, @Source, @Notes,
                 @BusinessPartnerID, @AssignedUserID, @ExpectedValue, @CurrencyID, @Probability, @ExpectedCloseDate,
                 @Priority, @IsActive, @CompanyID, @CreationUserID, @CreationDate)";
            clsSQL clsSQL = new clsSQL();
            if (trn == null)
                return Simulate.Integer32(clsSQL.ExecuteScalar(sql, prm, clsSQL.CreateDataBaseConnectionString(CompanyID)));
            return Simulate.Integer32(clsSQL.ExecuteScalar(sql, prm, clsSQL.CreateDataBaseConnectionString(CompanyID), trn));
        }

        public int UpdateCRMOpportunity(int ID, int PipelineID, int StageID, string Title, string AName, string EName,
            string Tel1, string Email, string Country, string Source, string Notes,
            int BusinessPartnerID, int AssignedUserID, decimal ExpectedValue, int CurrencyID,
            int Probability, DateTime ExpectedCloseDate, int Priority, bool IsActive,
            int ModificationUserID, int CompanyID)
        {
            SqlParameter[] prm =
            {
                new SqlParameter("@ID", SqlDbType.Int) { Value = ID },
                new SqlParameter("@PipelineID", SqlDbType.Int) { Value = PipelineID },
                new SqlParameter("@StageID", SqlDbType.Int) { Value = StageID },
                new SqlParameter("@Title", SqlDbType.NVarChar, -1) { Value = Title ?? "" },
                new SqlParameter("@AName", SqlDbType.NVarChar, -1) { Value = AName ?? "" },
                new SqlParameter("@EName", SqlDbType.NVarChar, -1) { Value = EName ?? "" },
                new SqlParameter("@Tel1", SqlDbType.NVarChar, -1) { Value = Tel1 ?? "" },
                new SqlParameter("@Email", SqlDbType.NVarChar, -1) { Value = Email ?? "" },
                new SqlParameter("@Country", SqlDbType.NVarChar, -1) { Value = Country ?? "" },
                new SqlParameter("@Source", SqlDbType.NVarChar, -1) { Value = Source ?? "" },
                new SqlParameter("@Notes", SqlDbType.NVarChar, -1) { Value = Notes ?? "" },
                new SqlParameter("@BusinessPartnerID", SqlDbType.Int) { Value = BusinessPartnerID > 0 ? BusinessPartnerID : (object)DBNull.Value },
                new SqlParameter("@AssignedUserID", SqlDbType.Int) { Value = AssignedUserID > 0 ? AssignedUserID : (object)DBNull.Value },
                new SqlParameter("@ExpectedValue", SqlDbType.Decimal) { Value = ExpectedValue },
                new SqlParameter("@CurrencyID", SqlDbType.Int) { Value = CurrencyID > 0 ? CurrencyID : (object)DBNull.Value },
                new SqlParameter("@Probability", SqlDbType.Int) { Value = Probability },
                new SqlParameter("@ExpectedCloseDate", SqlDbType.DateTime) { Value = ExpectedCloseDate == DateTime.MinValue ? (object)DBNull.Value : ExpectedCloseDate },
                new SqlParameter("@Priority", SqlDbType.Int) { Value = Priority },
                new SqlParameter("@IsActive", SqlDbType.Bit) { Value = IsActive },
                new SqlParameter("@ModificationUserID", SqlDbType.Int) { Value = ModificationUserID },
                new SqlParameter("@ModificationDate", SqlDbType.DateTime) { Value = DateTime.Now },
                new SqlParameter("@CompanyID", SqlDbType.Int) { Value = CompanyID },
            };
            clsSQL clsSQL = new clsSQL();
            return clsSQL.ExecuteNonQueryStatement(
                @"UPDATE tbl_CRMOpportunity SET
                    PipelineID=@PipelineID, StageID=@StageID, Title=@Title, AName=@AName, EName=@EName,
                    Tel1=@Tel1, Email=@Email, Country=@Country, Source=@Source, Notes=@Notes,
                    BusinessPartnerID=@BusinessPartnerID, AssignedUserID=@AssignedUserID,
                    ExpectedValue=@ExpectedValue, CurrencyID=@CurrencyID, Probability=@Probability,
                    ExpectedCloseDate=@ExpectedCloseDate, Priority=@Priority, IsActive=@IsActive,
                    ModificationUserID=@ModificationUserID, ModificationDate=@ModificationDate
                  WHERE ID=@ID AND CompanyID=@CompanyID",
                clsSQL.CreateDataBaseConnectionString(CompanyID), prm);
        }

        public bool DeleteCRMOpportunityByID(int ID, int CompanyID)
        {
            clsSQL clsSQL = new clsSQL();
            SqlParameter[] prm =
            {
                new SqlParameter("@ID", SqlDbType.Int) { Value = ID },
                new SqlParameter("@CompanyID", SqlDbType.Int) { Value = CompanyID },
            };
            clsSQL.ExecuteNonQueryStatement(
                @"UPDATE tbl_CRMOpportunity SET IsActive=0, ModificationDate=GETDATE()
                  WHERE ID=@ID AND CompanyID=@CompanyID",
                clsSQL.CreateDataBaseConnectionString(CompanyID), prm);
            return true;
        }

        public bool MoveCRMOpportunityStage(int OpportunityID, int ToStageID, int MovedByUserID, int CompanyID)
        {
            clsSQL clsSQL = new clsSQL();
            using (SqlConnection con = new SqlConnection(clsSQL.CreateDataBaseConnectionString(CompanyID)))
            {
                con.Open();
                using (SqlTransaction trn = con.BeginTransaction())
                {
                    try
                    {
                        clsCRMJourneyStage stageCls = new clsCRMJourneyStage();
                        DataTable targetStage = stageCls.GetStageByID(ToStageID, CompanyID, trn);
                        if (targetStage == null || targetStage.Rows.Count == 0)
                            throw new ArgumentException("Invalid stage");

                        SqlParameter[] oppPrm =
                        {
                            new SqlParameter("@ID", SqlDbType.Int) { Value = OpportunityID },
                            new SqlParameter("@CompanyID", SqlDbType.Int) { Value = CompanyID },
                        };
                        DataTable oppDt = clsSQL.ExecuteQueryStatement(
                            @"SELECT StageID, PipelineID FROM tbl_CRMOpportunity WHERE ID=@ID AND CompanyID=@CompanyID",
                            clsSQL.CreateDataBaseConnectionString(CompanyID), oppPrm, trn);
                        if (oppDt == null || oppDt.Rows.Count == 0)
                            throw new ArgumentException("Opportunity not found");

                        int fromStageId = Simulate.Integer32(oppDt.Rows[0]["StageID"]);
                        int pipelineId = Simulate.Integer32(oppDt.Rows[0]["PipelineID"]);
                        if (Simulate.Integer32(targetStage.Rows[0]["PipelineID"]) != pipelineId)
                            throw new ArgumentException("Stage does not belong to opportunity pipeline");

                        SqlParameter[] updPrm =
                        {
                            new SqlParameter("@ID", SqlDbType.Int) { Value = OpportunityID },
                            new SqlParameter("@StageID", SqlDbType.Int) { Value = ToStageID },
                            new SqlParameter("@ModificationUserID", SqlDbType.Int) { Value = MovedByUserID },
                            new SqlParameter("@CompanyID", SqlDbType.Int) { Value = CompanyID },
                        };
                        clsSQL.ExecuteNonQueryStatement(
                            @"UPDATE tbl_CRMOpportunity SET StageID=@StageID, ModificationUserID=@ModificationUserID, ModificationDate=GETDATE()
                              WHERE ID=@ID AND CompanyID=@CompanyID",
                            clsSQL.CreateDataBaseConnectionString(CompanyID), updPrm, trn);

                        clsCRMStageHistory history = new clsCRMStageHistory();
                        history.InsertCRMStageHistory(OpportunityID, fromStageId, ToStageID, MovedByUserID, CompanyID, trn);

                        trn.Commit();
                        return true;
                    }
                    catch
                    {
                        trn.Rollback();
                        throw;
                    }
                }
            }
        }

        public int ConvertCRMOpportunityToBusinessPartner(int OpportunityID, int CompanyID, int CreationUserID)
        {
            clsSQL clsSQL = new clsSQL();
            using (SqlConnection con = new SqlConnection(clsSQL.CreateDataBaseConnectionString(CompanyID)))
            {
                con.Open();
                using (SqlTransaction trn = con.BeginTransaction())
                {
                    try
                    {
                        SqlParameter[] prm =
                        {
                            new SqlParameter("@ID", SqlDbType.Int) { Value = OpportunityID },
                            new SqlParameter("@CompanyID", SqlDbType.Int) { Value = CompanyID },
                        };
                        DataTable dt = clsSQL.ExecuteQueryStatement(
                            @"SELECT * FROM tbl_CRMOpportunity WHERE ID=@ID AND CompanyID=@CompanyID",
                            clsSQL.CreateDataBaseConnectionString(CompanyID), prm, trn);
                        if (dt == null || dt.Rows.Count == 0)
                            throw new ArgumentException("Opportunity not found");

                        DataRow row = dt.Rows[0];
                        int existingBp = Simulate.Integer32(row["BusinessPartnerID"]);
                        if (existingBp > 0)
                        {
                            trn.Commit();
                            return existingBp;
                        }

                        string aName = Simulate.String(row["AName"]);
                        string eName = Simulate.String(row["EName"]);
                        if (string.IsNullOrWhiteSpace(aName))
                            aName = Simulate.String(row["Title"]);
                        if (string.IsNullOrWhiteSpace(eName))
                            eName = aName;

                        clsBusinessPartner bp = new clsBusinessPartner();
                        int bpId = bp.InsertBusinessPartner(
                            aName, eName, aName, Simulate.String(row["Country"]), Simulate.String(row["Tel1"]),
                            true, 0, Simulate.String(row["Email"]), 1, CompanyID, CreationUserID,
                            "", "", "", "", "", 0, "", "", "", "", "", "", trn);

                        SqlParameter[] updPrm =
                        {
                            new SqlParameter("@ID", SqlDbType.Int) { Value = OpportunityID },
                            new SqlParameter("@BusinessPartnerID", SqlDbType.Int) { Value = bpId },
                            new SqlParameter("@ModificationUserID", SqlDbType.Int) { Value = CreationUserID },
                            new SqlParameter("@CompanyID", SqlDbType.Int) { Value = CompanyID },
                        };
                        clsSQL.ExecuteNonQueryStatement(
                            @"UPDATE tbl_CRMOpportunity SET BusinessPartnerID=@BusinessPartnerID,
                              ModificationUserID=@ModificationUserID, ModificationDate=GETDATE()
                              WHERE ID=@ID AND CompanyID=@CompanyID",
                            clsSQL.CreateDataBaseConnectionString(CompanyID), updPrm, trn);

                        trn.Commit();
                        return bpId;
                    }
                    catch
                    {
                        trn.Rollback();
                        throw;
                    }
                }
            }
        }

        public int CreateFromLead(string AName, string Tel1, string Email, string Country, string Note,
            int CompanyID, int CreationUserID, int LeadID = 0)
        {
            clsCRMPipeline pipeline = new clsCRMPipeline();
            int pipelineId = pipeline.EnsureDefaultPipeline(CompanyID, CreationUserID);
            int stageId = GetDefaultStageID(pipelineId, CompanyID);

            int oppId = InsertCRMOpportunity(pipelineId, stageId,
                string.IsNullOrWhiteSpace(AName) ? "Lead" : AName, AName, AName,
                Tel1, Email, Country, "Website", Note,
                0, 0, 0, 0, 0, DateTime.MinValue, 0,
                CompanyID, CreationUserID);

            if (LeadID > 0 && oppId > 0)
            {
                clsSQL clsSQL = new clsSQL();
                SqlParameter[] prm =
                {
                    new SqlParameter("@LeadID", SqlDbType.Int) { Value = LeadID },
                    new SqlParameter("@CRMOpportunityID", SqlDbType.Int) { Value = oppId },
                };
                clsSQL.ExecuteNonQueryStatement(
                    @"UPDATE tbl_Leads SET CRMOpportunityID=@CRMOpportunityID WHERE ID=@LeadID",
                    clsSQL.CreateDataBaseConnectionString(CompanyID), prm);
            }

            return oppId;
        }
    }
}
