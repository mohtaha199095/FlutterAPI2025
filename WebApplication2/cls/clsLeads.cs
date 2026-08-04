using Microsoft.Data.SqlClient;
using System;
using System.Data;

namespace WebApplication2.cls
{
    public class clsLeads
    {
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
                    new SqlParameter("@CreationDate", SqlDbType.DateTime) { Value = DateTime.Now },
                };

                string query = @"INSERT INTO tbl_Leads (AName, Tel1, Email, Country, Note, CompanyID, CreationDate)
                                 OUTPUT INSERTED.ID
                                 VALUES (@AName, @Tel1, @Email, @Country, @Note, @CompanyID, @CreationDate)";

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
    }
}
