using Microsoft.Data.SqlClient;
using System.Data;
using System;

namespace WebApplication2.cls
{
    public class clsLeads
    {
        public int InsertLead(string AName, string Tel1, string Email, string Country, string Note )
        {
            try
            {
                // Define the SQL parameters
                SqlParameter[] prm =
                {
            new SqlParameter("@AName", SqlDbType.NVarChar, -1) { Value = AName },
            new SqlParameter("@Tel1", SqlDbType.NVarChar, -1) { Value = Tel1 },
            new SqlParameter("@Email", SqlDbType.NVarChar, -1) { Value = Email },
            new SqlParameter("@Country", SqlDbType.NVarChar, -1) { Value = Country },
            new SqlParameter("@Note", SqlDbType.NVarChar, -1) { Value = Note },
            new SqlParameter("@CreationDate", SqlDbType.DateTime) { Value = DateTime.Now }
        };

                // Define the SQL query
                string query = @"INSERT INTO tbl_Leads (AName, Tel1, Email, Country, Note, CreationDate)
                         
                          VALUES (@AName, @Tel1, @Email, @Country, @Note, @CreationDate)";

                // Use a helper class to execute the query
                clsSQL clsSQL = new clsSQL();
                return Simulate.Integer32(clsSQL.ExecuteScalar(query, prm, clsSQL.MainDataBaseconString));
            }
            catch (Exception)
            {
                throw;
            }
        }
    }
}
