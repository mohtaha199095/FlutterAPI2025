using Microsoft.Data.SqlClient;
using System;
using System.Data;

namespace WebApplication2.cls
{
    public class clsCRMStageHistory
    {
        public int InsertCRMStageHistory(int OpportunityID, int FromStageID, int ToStageID,
            int MovedByUserID, int CompanyID, SqlTransaction trn = null)
        {
            SqlParameter[] prm =
            {
                new SqlParameter("@OpportunityID", SqlDbType.Int) { Value = OpportunityID },
                new SqlParameter("@FromStageID", SqlDbType.Int) { Value = FromStageID },
                new SqlParameter("@ToStageID", SqlDbType.Int) { Value = ToStageID },
                new SqlParameter("@MovedByUserID", SqlDbType.Int) { Value = MovedByUserID },
                new SqlParameter("@MovedDate", SqlDbType.DateTime) { Value = DateTime.Now },
                new SqlParameter("@CompanyID", SqlDbType.Int) { Value = CompanyID },
            };
            string sql = @"INSERT INTO tbl_CRMStageHistory
                (OpportunityID, FromStageID, ToStageID, MovedByUserID, MovedDate, CompanyID)
                OUTPUT INSERTED.ID
                VALUES (@OpportunityID, @FromStageID, @ToStageID, @MovedByUserID, @MovedDate, @CompanyID)";
            clsSQL clsSQL = new clsSQL();
            if (trn == null)
                return Simulate.Integer32(clsSQL.ExecuteScalar(sql, prm, clsSQL.CreateDataBaseConnectionString(CompanyID)));
            return Simulate.Integer32(clsSQL.ExecuteScalar(sql, prm, clsSQL.CreateDataBaseConnectionString(CompanyID), trn));
        }

        public DataTable SelectCRMStageHistoryByOpportunity(int OpportunityID, int CompanyID)
        {
            SqlParameter[] prm =
            {
                new SqlParameter("@OpportunityID", SqlDbType.Int) { Value = OpportunityID },
                new SqlParameter("@CompanyID", SqlDbType.Int) { Value = CompanyID },
            };
            clsSQL clsSQL = new clsSQL();
            return clsSQL.ExecuteQueryStatement(
                @"SELECT H.*,
                         FS.AName AS FromStageAName, FS.EName AS FromStageEName,
                         TS.AName AS ToStageAName, TS.EName AS ToStageEName
                  FROM tbl_CRMStageHistory H
                  LEFT JOIN tbl_CRMJourneyStage FS ON FS.ID = H.FromStageID
                  LEFT JOIN tbl_CRMJourneyStage TS ON TS.ID = H.ToStageID
                  WHERE H.OpportunityID = @OpportunityID AND H.CompanyID = @CompanyID
                  ORDER BY H.MovedDate DESC",
                clsSQL.CreateDataBaseConnectionString(CompanyID), prm);
        }
    }
}
