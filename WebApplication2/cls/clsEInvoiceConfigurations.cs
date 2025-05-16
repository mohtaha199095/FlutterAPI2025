using System;
using System.Data;
using Microsoft.Data.SqlClient;

namespace WebApplication2.cls
{
    public class clsEInvoiceConfigurations
    {
        public DataTable SelectEInvoiceConfigurations(int ID, string Country, string UserCode, int CompanyID,SqlTransaction trn =null)
        {
            try
            {
                SqlParameter[] prm =
                {
                    new SqlParameter("@ID", SqlDbType.Int) { Value = ID },
                    new SqlParameter("@Country", SqlDbType.NVarChar, -1) { Value = Country },
                    new SqlParameter("@UserCode", SqlDbType.NVarChar, -1) { Value = UserCode },
    
                     
                };

                clsSQL clsSQL = new clsSQL();
                DataTable dt = clsSQL.ExecuteQueryStatement(@"
                    SELECT * FROM tbl_EInvoiceConfigurations
                    WHERE (@ID = 0 OR ID = @ID)
                      AND (@Country = '' OR Country = @Country)
                      AND (@UserCode = '' OR UserCode = @UserCode)
                ",clsSQL.CreateDataBaseConnectionString(CompanyID), prm, trn);

                return dt;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public bool DeleteEInvoiceConfigurationsByID(int ID, int CompanyID)
        {
            try
            {
                SqlParameter[] prm =
                {
                    new SqlParameter("@ID", SqlDbType.Int) { Value = ID },
                };

                clsSQL clsSQL = new clsSQL();
                int A = clsSQL.ExecuteNonQueryStatement(@"
                    DELETE FROM tbl_EInvoiceConfigurations WHERE ID = @ID
                ", clsSQL.CreateDataBaseConnectionString(CompanyID), prm);

                return A > 0;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public int InsertEInvoiceConfigurations(string Country, string UserCode, string SecretKey, string ActivityNumber,string TaxNumber, bool Active, int CompanyID, SqlTransaction trn = null)
        {
            try
            {
                SqlParameter[] prm =
                {
                    new SqlParameter("@Country", SqlDbType.NVarChar, -1) { Value = Country },
                    new SqlParameter("@UserCode", SqlDbType.NVarChar, -1) { Value = UserCode },
                    new SqlParameter("@SecretKey", SqlDbType.NVarChar, -1) { Value = SecretKey },
                    new SqlParameter("@Active", SqlDbType.Bit) { Value = Active },
                    new SqlParameter("@CreationDate", SqlDbType.DateTime) { Value = DateTime.Now },
                      new SqlParameter("@ActivityNumber", SqlDbType.NVarChar, -1) { Value = ActivityNumber },

              new SqlParameter("@TaxNumber", SqlDbType.NVarChar, -1) { Value = TaxNumber },
                };

                string sql = @"
                    INSERT INTO tbl_EInvoiceConfigurations(Country, UserCode, SecretKey, Active, CreationDate,ActivityNumber,TaxNumber)
                    OUTPUT INSERTED.ID
                    VALUES(@Country, @UserCode, @SecretKey, @Active, @CreationDate,@ActivityNumber,@TaxNumber)
                ";

                clsSQL clsSQL = new clsSQL();
                if (trn == null)
                {
                    return Simulate.Integer32(clsSQL.ExecuteScalar(sql, prm, clsSQL.CreateDataBaseConnectionString(CompanyID)));
                }
                else
                {
                    return Simulate.Integer32(clsSQL.ExecuteScalar(sql, prm, clsSQL.CreateDataBaseConnectionString(CompanyID), trn));
                }
            }
            catch (Exception)
            {
                throw;
            }
        }

        public int UpdateEInvoiceConfigurations(int ID, string Country, string UserCode, string SecretKey,string ActivityNumber,string TaxNumber, bool Active, int CompanyID, SqlTransaction trn = null)
        {
            try
            {
                SqlParameter[] prm =
                {
                    new SqlParameter("@ID", SqlDbType.Int) { Value = ID },
                    new SqlParameter("@Country", SqlDbType.NVarChar, -1) { Value = Country },
                    new SqlParameter("@UserCode", SqlDbType.NVarChar, -1) { Value = UserCode },
                    new SqlParameter("@SecretKey", SqlDbType.NVarChar, -1) { Value = SecretKey },
                    new SqlParameter("@Active", SqlDbType.Bit) { Value = Active },
                      new SqlParameter("@ActivityNumber", SqlDbType.NVarChar, -1) { Value = ActivityNumber },
   new SqlParameter("@TaxNumber", SqlDbType.NVarChar, -1) { Value = TaxNumber },

                      
                };

                string sql = @"
                    UPDATE tbl_EInvoiceConfigurations SET
                        Country = @Country,
                        UserCode = @UserCode,
                        SecretKey = @SecretKey,
                        Active = @Active ,
                        ActivityNumber=@ActivityNumber,
                        TaxNumber=@TaxNumber    
                    WHERE ID = @ID
                ";

                clsSQL clsSQL = new clsSQL();
                if (trn == null)
                {
                    return clsSQL.ExecuteNonQueryStatement(sql, clsSQL.CreateDataBaseConnectionString(CompanyID), prm);
                }
                else
                {
                    return clsSQL.ExecuteNonQueryStatement(sql, clsSQL.CreateDataBaseConnectionString(CompanyID), prm, trn);
                }
            }
            catch (Exception)
            {
                throw;
            }
        }
    }
}
