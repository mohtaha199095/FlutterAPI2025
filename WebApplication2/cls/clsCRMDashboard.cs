using Microsoft.Data.SqlClient;
using System;
using System.Data;

namespace WebApplication2.cls
{
    public class clsCRMDashboard
    {
        public DataTable SelectCRMDashboardSummary(int PipelineID, int CompanyID)
        {
            SqlParameter[] prm =
            {
                new SqlParameter("@PipelineID", SqlDbType.Int) { Value = PipelineID },
                new SqlParameter("@CompanyID", SqlDbType.Int) { Value = CompanyID },
            };
            clsSQL clsSQL = new clsSQL();
            return clsSQL.ExecuteQueryStatement(
                @"SELECT
                    SUM(CASE WHEN S.IsWon = 0 AND S.IsLost = 0 THEN 1 ELSE 0 END) AS OpenDeals,
                    SUM(CASE WHEN S.IsWon = 1 THEN 1 ELSE 0 END) AS WonDeals,
                    SUM(CASE WHEN S.IsLost = 1 THEN 1 ELSE 0 END) AS LostDeals,
                    SUM(CASE WHEN S.IsWon = 0 AND S.IsLost = 0 THEN ISNULL(O.ExpectedValue, 0) ELSE 0 END) AS PipelineValue,
                    SUM(CASE WHEN S.IsWon = 1 THEN ISNULL(O.ExpectedValue, 0) ELSE 0 END) AS WonValue,
                    SUM(CASE WHEN S.IsLost = 1 THEN ISNULL(O.ExpectedValue, 0) ELSE 0 END) AS LostValue,
                    SUM(CASE WHEN O.BusinessPartnerID IS NOT NULL AND O.BusinessPartnerID > 0 THEN 1 ELSE 0 END) AS ConvertedCustomers,
                    AVG(CASE WHEN S.IsWon = 0 AND S.IsLost = 0 AND O.Probability > 0 THEN O.Probability ELSE NULL END) AS AvgProbability
                  FROM tbl_CRMOpportunity O
                  INNER JOIN tbl_CRMJourneyStage S ON S.ID = O.StageID AND S.IsActive = 1
                  WHERE O.CompanyID = @CompanyID
                    AND O.PipelineID = @PipelineID
                    AND O.IsActive = 1",
                clsSQL.CreateDataBaseConnectionString(CompanyID), prm);
        }

        public DataTable SelectCRMPendingActivities(int CompanyID)
        {
            SqlParameter[] prm =
            {
                new SqlParameter("@CompanyID", SqlDbType.Int) { Value = CompanyID },
            };
            clsSQL clsSQL = new clsSQL();
            return clsSQL.ExecuteQueryStatement(
                @"SELECT
                    COUNT(*) AS PendingActivities,
                    SUM(CASE WHEN A.DueDate IS NOT NULL AND A.DueDate < GETDATE() THEN 1 ELSE 0 END) AS OverdueActivities
                  FROM tbl_CRMActivity A
                  WHERE A.CompanyID = @CompanyID
                    AND A.IsDone = 0",
                clsSQL.CreateDataBaseConnectionString(CompanyID), prm);
        }

        public DataTable SelectCRMDealsByStage(int PipelineID, int CompanyID)
        {
            SqlParameter[] prm =
            {
                new SqlParameter("@PipelineID", SqlDbType.Int) { Value = PipelineID },
                new SqlParameter("@CompanyID", SqlDbType.Int) { Value = CompanyID },
            };
            clsSQL clsSQL = new clsSQL();
            return clsSQL.ExecuteQueryStatement(
                @"SELECT S.EName AS Name, S.AName, S.Color,
                         COUNT(O.ID) AS Total,
                         ISNULL(SUM(O.ExpectedValue), 0) AS Value
                  FROM tbl_CRMJourneyStage S
                  LEFT JOIN tbl_CRMOpportunity O ON O.StageID = S.ID
                    AND O.IsActive = 1 AND O.CompanyID = @CompanyID
                  WHERE S.CompanyID = @CompanyID
                    AND S.PipelineID = @PipelineID
                    AND S.IsActive = 1
                  GROUP BY S.ID, S.EName, S.AName, S.Color, S.StageOrder
                  ORDER BY S.StageOrder",
                clsSQL.CreateDataBaseConnectionString(CompanyID), prm);
        }

        public DataTable SelectCRMDealsBySource(int PipelineID, int CompanyID)
        {
            SqlParameter[] prm =
            {
                new SqlParameter("@PipelineID", SqlDbType.Int) { Value = PipelineID },
                new SqlParameter("@CompanyID", SqlDbType.Int) { Value = CompanyID },
            };
            clsSQL clsSQL = new clsSQL();
            return clsSQL.ExecuteQueryStatement(
                @"SELECT
                    CASE WHEN ISNULL(O.Source, '') = '' THEN N'Unknown' ELSE O.Source END AS Name,
                    COUNT(*) AS Total,
                    ISNULL(SUM(O.ExpectedValue), 0) AS Value
                  FROM tbl_CRMOpportunity O
                  WHERE O.CompanyID = @CompanyID
                    AND O.PipelineID = @PipelineID
                    AND O.IsActive = 1
                  GROUP BY CASE WHEN ISNULL(O.Source, '') = '' THEN N'Unknown' ELSE O.Source END
                  ORDER BY Total DESC",
                clsSQL.CreateDataBaseConnectionString(CompanyID), prm);
        }

        public DataTable SelectCRMMonthlyTrend(int PipelineID, int CompanyID, int MonthsBack)
        {
            if (MonthsBack <= 0) MonthsBack = 6;
            SqlParameter[] prm =
            {
                new SqlParameter("@PipelineID", SqlDbType.Int) { Value = PipelineID },
                new SqlParameter("@CompanyID", SqlDbType.Int) { Value = CompanyID },
                new SqlParameter("@MonthsBack", SqlDbType.Int) { Value = MonthsBack },
            };
            clsSQL clsSQL = new clsSQL();
            return clsSQL.ExecuteQueryStatement(
                @"SELECT
                    FORMAT(O.CreationDate, 'yyyy-MM') AS Month,
                    COUNT(*) AS NewDeals,
                    ISNULL(SUM(O.ExpectedValue), 0) AS NewValue
                  FROM tbl_CRMOpportunity O
                  WHERE O.CompanyID = @CompanyID
                    AND O.PipelineID = @PipelineID
                    AND O.IsActive = 1
                    AND O.CreationDate >= DATEADD(MONTH, -@MonthsBack, GETDATE())
                  GROUP BY FORMAT(O.CreationDate, 'yyyy-MM')
                  ORDER BY Month",
                clsSQL.CreateDataBaseConnectionString(CompanyID), prm);
        }

        public DataTable SelectCRMRecentWonDeals(int PipelineID, int CompanyID, int TopN)
        {
            if (TopN <= 0) TopN = 5;
            SqlParameter[] prm =
            {
                new SqlParameter("@PipelineID", SqlDbType.Int) { Value = PipelineID },
                new SqlParameter("@CompanyID", SqlDbType.Int) { Value = CompanyID },
                new SqlParameter("@TopN", SqlDbType.Int) { Value = TopN },
            };
            clsSQL clsSQL = new clsSQL();
            return clsSQL.ExecuteQueryStatement(
                @"SELECT TOP (@TopN)
                    O.ID, O.Title, O.AName, O.EName, O.ExpectedValue, O.ModificationDate,
                    O.BusinessPartnerID, S.EName AS StageEName, S.AName AS StageAName
                  FROM tbl_CRMOpportunity O
                  INNER JOIN tbl_CRMJourneyStage S ON S.ID = O.StageID AND S.IsWon = 1
                  WHERE O.CompanyID = @CompanyID
                    AND O.PipelineID = @PipelineID
                    AND O.IsActive = 1
                  ORDER BY O.ModificationDate DESC",
                clsSQL.CreateDataBaseConnectionString(CompanyID), prm);
        }
    }
}
