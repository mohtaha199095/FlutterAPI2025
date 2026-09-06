using Microsoft.Data.SqlClient;
using System;
using System.Data;

namespace WebApplication2.cls
{
    public class clsLeads
    {
        /// <summary>0 = Open, 1 = Rejected. Converted leads have CRMOpportunityID set.</summary>
        public const int StatusOpen = 0;
        public const int StatusRejected = 1;

        public int InsertLead(string AName, string Tel1, string Email, string Country, string Note, int CompanyID, int CreationUserID = 1)
        {
            try
            {
                SqlParameter[] prm =
                {
                    new SqlParameter("@AName", SqlDbType.NVarChar, -1) { Value = AName },
                    new SqlParameter("@Tel1", SqlDbType.NVarChar, -1) { Value = Tel1 },
                    new SqlParameter("@Email", SqlDbType.NVarChar, -1) { Value = Email },
                    new SqlParameter("@Country", SqlDbType.NVarChar, -1) { Value = Country },
                    new SqlParameter("@Note", SqlDbType.NVarChar, -1) { Value = Note },
                    new SqlParameter("@CompanyID", SqlDbType.Int) { Value = CompanyID },
                    new SqlParameter("@Status", SqlDbType.Int) { Value = StatusOpen },
                    new SqlParameter("@CreationDate", SqlDbType.DateTime) { Value = DateTime.Now },
                };

                string query = @"INSERT INTO tbl_Leads (AName, Tel1, Email, Country, Note, CompanyID, Status, CreationDate)
                                 OUTPUT INSERTED.ID
                                 VALUES (@AName, @Tel1, @Email, @Country, @Note, @CompanyID, @Status, @CreationDate)";

                clsSQL clsSQL = new clsSQL();
                int leadId = Simulate.Integer32(clsSQL.ExecuteScalar(query, prm, clsSQL.CreateDataBaseConnectionString(CompanyID)));

                if (leadId > 0 && CompanyID > 0)
                {
                    clsCRMOpportunity crm = new clsCRMOpportunity();
                    crm.CreateFromLead(AName, Tel1, Email, Country, Note, CompanyID, CreationUserID, leadId);
                }

                return leadId;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public DataTable SelectLeads(int ID, int CompanyID, int StatusFilter = -1)
        {
            SqlParameter[] prm =
            {
                new SqlParameter("@ID", SqlDbType.Int) { Value = ID },
                new SqlParameter("@CompanyID", SqlDbType.Int) { Value = CompanyID },
                new SqlParameter("@StatusFilter", SqlDbType.Int) { Value = StatusFilter },
            };
            clsSQL clsSQL = new clsSQL();
            return clsSQL.ExecuteQueryStatement(
                @"SELECT L.*,
                         ISNULL(L.Status, 0) AS Status,
                         O.Title AS OpportunityTitle,
                         O.StageID AS OpportunityStageID,
                         S.EName AS StageEName,
                         S.AName AS StageAName
                  FROM tbl_Leads L
                  LEFT JOIN tbl_CRMOpportunity O ON O.ID = L.CRMOpportunityID AND O.CompanyID = L.CompanyID
                  LEFT JOIN tbl_CRMJourneyStage S ON S.ID = O.StageID
                  WHERE (L.ID = @ID OR @ID = 0)
                    AND L.CompanyID = @CompanyID
                    AND (@StatusFilter < 0 OR ISNULL(L.Status, 0) = @StatusFilter)
                  ORDER BY L.CreationDate DESC, L.ID DESC",
                clsSQL.CreateDataBaseConnectionString(CompanyID), prm);
        }

        public bool UpdateLeadStatus(int ID, int Status, int CompanyID)
        {
            SqlParameter[] prm =
            {
                new SqlParameter("@ID", SqlDbType.Int) { Value = ID },
                new SqlParameter("@Status", SqlDbType.Int) { Value = Status },
                new SqlParameter("@CompanyID", SqlDbType.Int) { Value = CompanyID },
            };
            clsSQL clsSQL = new clsSQL();
            return clsSQL.ExecuteNonQueryStatement(
                @"UPDATE tbl_Leads SET Status = @Status WHERE ID = @ID AND CompanyID = @CompanyID",
                clsSQL.CreateDataBaseConnectionString(CompanyID), prm) > 0;
        }
    }
}
