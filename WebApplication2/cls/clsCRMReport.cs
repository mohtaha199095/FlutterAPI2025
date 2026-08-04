using Microsoft.Data.SqlClient;
using System;
using System.Data;

namespace WebApplication2.cls
{
    public class clsCRMReport
    {
        public DataTable SelectPipelineSummaryReport(int PipelineID, int CompanyID,
            DateTime DateFrom, DateTime DateTo)
        {
            SqlParameter[] prm =
            {
                new SqlParameter("@PipelineID", SqlDbType.Int) { Value = PipelineID },
                new SqlParameter("@CompanyID", SqlDbType.Int) { Value = CompanyID },
                new SqlParameter("@DateFrom", SqlDbType.DateTime) { Value = DateFrom == DateTime.MinValue ? (object)DBNull.Value : DateFrom.Date },
                new SqlParameter("@DateTo", SqlDbType.DateTime) { Value = DateTo == DateTime.MinValue ? (object)DBNull.Value : DateTo.Date.AddDays(1).AddSeconds(-1) },
            };
            clsSQL clsSQL = new clsSQL();
            return clsSQL.ExecuteQueryStatement(
                @"SELECT
                    S.EName AS StageEName, S.AName AS StageAName, S.Color,
                    S.IsWon, S.IsLost, S.StageOrder,
                    COUNT(O.ID) AS DealCount,
                    ISNULL(SUM(O.ExpectedValue), 0) AS TotalValue,
                    AVG(CASE WHEN O.Probability > 0 THEN O.Probability ELSE NULL END) AS AvgProbability
                  FROM tbl_CRMJourneyStage S
                  LEFT JOIN tbl_CRMOpportunity O ON O.StageID = S.ID
                    AND O.IsActive = 1 AND O.CompanyID = @CompanyID
                    AND (@DateFrom IS NULL OR O.CreationDate >= @DateFrom)
                    AND (@DateTo IS NULL OR O.CreationDate <= @DateTo)
                  WHERE S.CompanyID = @CompanyID
                    AND S.PipelineID = @PipelineID
                    AND S.IsActive = 1
                  GROUP BY S.ID, S.EName, S.AName, S.Color, S.IsWon, S.IsLost, S.StageOrder
                  ORDER BY S.StageOrder",
                clsSQL.CreateDataBaseConnectionString(CompanyID), prm);
        }

        public DataTable SelectWonLostReport(int PipelineID, int CompanyID,
            DateTime DateFrom, DateTime DateTo)
        {
            SqlParameter[] prm =
            {
                new SqlParameter("@PipelineID", SqlDbType.Int) { Value = PipelineID },
                new SqlParameter("@CompanyID", SqlDbType.Int) { Value = CompanyID },
                new SqlParameter("@DateFrom", SqlDbType.DateTime) { Value = DateFrom == DateTime.MinValue ? (object)DBNull.Value : DateFrom.Date },
                new SqlParameter("@DateTo", SqlDbType.DateTime) { Value = DateTo == DateTime.MinValue ? (object)DBNull.Value : DateTo.Date.AddDays(1).AddSeconds(-1) },
            };
            clsSQL clsSQL = new clsSQL();
            return clsSQL.ExecuteQueryStatement(
                @"SELECT
                    O.ID, O.Title, O.AName, O.EName, O.Tel1, O.Email, O.Source,
                    O.ExpectedValue, O.Probability, O.ModificationDate,
                    O.BusinessPartnerID,
                    S.EName AS StageEName, S.AName AS StageAName, S.Color,
                    S.IsWon, S.IsLost
                  FROM tbl_CRMOpportunity O
                  INNER JOIN tbl_CRMJourneyStage S ON S.ID = O.StageID
                  WHERE O.CompanyID = @CompanyID
                    AND O.PipelineID = @PipelineID
                    AND O.IsActive = 1
                    AND (S.IsWon = 1 OR S.IsLost = 1)
                    AND (@DateFrom IS NULL OR O.ModificationDate >= @DateFrom)
                    AND (@DateTo IS NULL OR O.ModificationDate <= @DateTo)
                  ORDER BY O.ModificationDate DESC",
                clsSQL.CreateDataBaseConnectionString(CompanyID), prm);
        }

        public DataTable SelectActivityReport(int CompanyID, int OpportunityID,
            DateTime DateFrom, DateTime DateTo)
        {
            SqlParameter[] prm =
            {
                new SqlParameter("@CompanyID", SqlDbType.Int) { Value = CompanyID },
                new SqlParameter("@OpportunityID", SqlDbType.Int) { Value = OpportunityID },
                new SqlParameter("@DateFrom", SqlDbType.DateTime) { Value = DateFrom == DateTime.MinValue ? (object)DBNull.Value : DateFrom.Date },
                new SqlParameter("@DateTo", SqlDbType.DateTime) { Value = DateTo == DateTime.MinValue ? (object)DBNull.Value : DateTo.Date.AddDays(1).AddSeconds(-1) },
            };
            clsSQL clsSQL = new clsSQL();
            return clsSQL.ExecuteQueryStatement(
                @"SELECT
                    A.ID, A.ActivityType, A.Subject, A.Notes, A.DueDate, A.IsDone,
                    A.CreationDate, O.Title AS OpportunityTitle, O.AName AS OpportunityAName
                  FROM tbl_CRMActivity A
                  LEFT JOIN tbl_CRMOpportunity O ON O.ID = A.OpportunityID
                  WHERE A.CompanyID = @CompanyID
                    AND (@OpportunityID = 0 OR A.OpportunityID = @OpportunityID)
                    AND (@DateFrom IS NULL OR A.CreationDate >= @DateFrom)
                    AND (@DateTo IS NULL OR A.CreationDate <= @DateTo)
                  ORDER BY A.CreationDate DESC",
                clsSQL.CreateDataBaseConnectionString(CompanyID), prm);
        }

        public DataTable SelectSourceAnalysisReport(int PipelineID, int CompanyID,
            DateTime DateFrom, DateTime DateTo)
        {
            SqlParameter[] prm =
            {
                new SqlParameter("@PipelineID", SqlDbType.Int) { Value = PipelineID },
                new SqlParameter("@CompanyID", SqlDbType.Int) { Value = CompanyID },
                new SqlParameter("@DateFrom", SqlDbType.DateTime) { Value = DateFrom == DateTime.MinValue ? (object)DBNull.Value : DateFrom.Date },
                new SqlParameter("@DateTo", SqlDbType.DateTime) { Value = DateTo == DateTime.MinValue ? (object)DBNull.Value : DateTo.Date.AddDays(1).AddSeconds(-1) },
            };
            clsSQL clsSQL = new clsSQL();
            return clsSQL.ExecuteQueryStatement(
                @"SELECT
                    CASE WHEN ISNULL(O.Source, '') = '' THEN N'Unknown' ELSE O.Source END AS SourceName,
                    COUNT(*) AS DealCount,
                    ISNULL(SUM(O.ExpectedValue), 0) AS TotalValue,
                    SUM(CASE WHEN S.IsWon = 1 THEN 1 ELSE 0 END) AS WonCount,
                    SUM(CASE WHEN S.IsLost = 1 THEN 1 ELSE 0 END) AS LostCount,
                    SUM(CASE WHEN S.IsWon = 0 AND S.IsLost = 0 THEN 1 ELSE 0 END) AS OpenCount
                  FROM tbl_CRMOpportunity O
                  INNER JOIN tbl_CRMJourneyStage S ON S.ID = O.StageID
                  WHERE O.CompanyID = @CompanyID
                    AND O.PipelineID = @PipelineID
                    AND O.IsActive = 1
                    AND (@DateFrom IS NULL OR O.CreationDate >= @DateFrom)
                    AND (@DateTo IS NULL OR O.CreationDate <= @DateTo)
                  GROUP BY CASE WHEN ISNULL(O.Source, '') = '' THEN N'Unknown' ELSE O.Source END
                  ORDER BY DealCount DESC",
                clsSQL.CreateDataBaseConnectionString(CompanyID), prm);
        }

        public DataTable SelectStageConversionReport(int PipelineID, int CompanyID,
            DateTime DateFrom, DateTime DateTo)
        {
            SqlParameter[] prm =
            {
                new SqlParameter("@PipelineID", SqlDbType.Int) { Value = PipelineID },
                new SqlParameter("@CompanyID", SqlDbType.Int) { Value = CompanyID },
                new SqlParameter("@DateFrom", SqlDbType.DateTime) { Value = DateFrom == DateTime.MinValue ? (object)DBNull.Value : DateFrom.Date },
                new SqlParameter("@DateTo", SqlDbType.DateTime) { Value = DateTo == DateTime.MinValue ? (object)DBNull.Value : DateTo.Date.AddDays(1).AddSeconds(-1) },
            };
            clsSQL clsSQL = new clsSQL();
            return clsSQL.ExecuteQueryStatement(
                @"SELECT
                    TS.EName AS ToStageEName, TS.AName AS ToStageAName, TS.Color,
                    COUNT(H.ID) AS MoveCount,
                    AVG(DATEDIFF(DAY, H.MovedDate,
                        (SELECT MIN(H2.MovedDate) FROM tbl_CRMStageHistory H2
                         WHERE H2.OpportunityID = H.OpportunityID AND H2.MovedDate > H.MovedDate)
                    )) AS AvgDaysToNextStage
                  FROM tbl_CRMStageHistory H
                  INNER JOIN tbl_CRMOpportunity O ON O.ID = H.OpportunityID AND O.IsActive = 1
                  INNER JOIN tbl_CRMJourneyStage TS ON TS.ID = H.ToStageID
                  WHERE O.CompanyID = @CompanyID
                    AND O.PipelineID = @PipelineID
                    AND (@DateFrom IS NULL OR H.MovedDate >= @DateFrom)
                    AND (@DateTo IS NULL OR H.MovedDate <= @DateTo)
                  GROUP BY TS.ID, TS.EName, TS.AName, TS.Color, TS.StageOrder
                  ORDER BY TS.StageOrder",
                clsSQL.CreateDataBaseConnectionString(CompanyID), prm);
        }
    }
}
