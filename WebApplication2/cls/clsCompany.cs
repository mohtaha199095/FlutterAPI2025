using DocumentFormat.OpenXml.Wordprocessing;
using System;
using System.Data;
using Microsoft.Data.SqlClient;
using System.Net;

namespace WebApplication2.cls
{
    public class clsCompany
    {
        public DataTable SelectCompany(int Id, string AName, string EName, string Tel1,int CompanyID,string PartOfTheName,bool fromMainDB)
        {
            try
            {
                clsSQL clsSQL = new clsSQL();

                SqlParameter[] prm =
                 { new SqlParameter("@Id", SqlDbType.Int) { Value = Id },
      new SqlParameter("@AName", SqlDbType.NVarChar,-1) { Value = AName },
       new SqlParameter("@EName", SqlDbType.NVarChar,-1) { Value = EName },
           new SqlParameter("@Tel1", SqlDbType.NVarChar,-1) { Value = Simulate.String(Tel1?.Trim() ?? "") },
           new SqlParameter("@PartOfTheName", SqlDbType.NVarChar,-1) { Value = Simulate.String(PartOfTheName?.Trim() ?? "") },
           
                };

                string con = clsSQL.CreateDataBaseConnectionString(CompanyID);

                if (fromMainDB) {
                    con = clsSQL.MainDataBaseconString;


                } 
                    DataTable dt = clsSQL.ExecuteQueryStatement(@"select * from tbl_Company where (id=@Id or @Id=0 ) and  
                     (AName=@AName or @AName='' ) and (EName=@EName or @EName='' ) 
    AND (
(AName LIKE N'%' + @PartOfTheName + '%' OR @PartOfTheName = '') or 
(tradeName LIKE N'%' + @PartOfTheName + '%' OR @PartOfTheName = '')
        )
AND (Tel1 LIKE N'%' + @Tel1 + '%' OR @Tel1 = '')
                     ", con, prm);

                return dt;
            }
            catch (Exception)
            {

                throw;
            }


        }

        public bool DeleteCompanyByID(int Id, int CompanyID)
        {
            try
            {
                SqlParameter[] prm =
                 { new SqlParameter("@Id", SqlDbType.Int) { Value = Id },

                }; clsSQL clsSQL = new clsSQL();

                int A = clsSQL.ExecuteNonQueryStatement(@"delete from tbl_Company where (id=@Id  )", clsSQL.CreateDataBaseConnectionString(CompanyID), prm);

                return true;
            }
            catch (Exception)
            {

                throw;
            }


        }
        public int InsertCompany(string AName, string EName, string Email
            , string Address, string Tel1, string Tel2, string ContactPerson,
            string ContactNumber, byte[] Logo, string TradeName,string DataBaseName,string conString)
        {
            try
            {
                SqlParameter[] prm =
                 { new SqlParameter("@AName", SqlDbType.NVarChar,-1) { Value = AName },
                  new SqlParameter("@EName", SqlDbType.NVarChar,-1) { Value = EName },
                    new SqlParameter("@Email", SqlDbType.NVarChar,-1) { Value = Email },
                    new SqlParameter("@Address", SqlDbType.NVarChar,-1) { Value = Address },
                      new SqlParameter("@Tel1", SqlDbType.NVarChar,-1) { Value = Tel1 },
                        new SqlParameter("@Tel2", SqlDbType.NVarChar,-1) { Value = Tel2 },
                        new SqlParameter("@ContactPerson", SqlDbType.NVarChar,-1) { Value = ContactPerson },
                        new SqlParameter("@ContactNumber", SqlDbType.NVarChar,-1) { Value = ContactNumber },
                        new SqlParameter("@TradeName", SqlDbType.NVarChar,-1) { Value = TradeName },
                     new SqlParameter("@Logo", SqlDbType.Binary) { Value = Logo!= null ? Logo: DBNull.Value },

                     new SqlParameter("@CreationDate", SqlDbType.DateTime) { Value = DateTime.Now },
                            new SqlParameter("@DataBaseName", SqlDbType.NVarChar,-1) { Value = DataBaseName },

                     
                };

                string a = @"insert into tbl_Company(AName,EName,Email,Address,Tel1,Tel2,ContactPerson,ContactNumber,TradeName,Logo,CreationDate,DataBaseName)
                        OUTPUT INSERTED.ID values(@AName,@EName,@Email,@Address,@Tel1,@Tel2,@ContactPerson,@ContactNumber,@TradeName,@Logo,@CreationDate,@DataBaseName)";
                clsSQL clsSQL = new clsSQL();

                return Simulate.Integer32(clsSQL.ExecuteScalar(a, prm,conString));

            }
            catch (Exception)
            {

                throw;
            }


        }
        public int InsertCompanyWithID(int ID,string AName, string EName, string Email
            , string Address, string Tel1, string Tel2, string ContactPerson,
            string ContactNumber, byte[] Logo, string TradeName, string DataBaseName, string conString)
        {
            try
            {
                SqlParameter[] prm =
                 {  new SqlParameter("@ID", SqlDbType.NVarChar,-1) { Value = ID },
                    new SqlParameter("@AName", SqlDbType.NVarChar,-1) { Value = AName },
                  new SqlParameter("@EName", SqlDbType.NVarChar,-1) { Value = EName },
                    new SqlParameter("@Email", SqlDbType.NVarChar,-1) { Value = Email },
                    new SqlParameter("@Address", SqlDbType.NVarChar,-1) { Value = Address },
                      new SqlParameter("@Tel1", SqlDbType.NVarChar,-1) { Value = Tel1 },
                        new SqlParameter("@Tel2", SqlDbType.NVarChar,-1) { Value = Tel2 },
                        new SqlParameter("@ContactPerson", SqlDbType.NVarChar,-1) { Value = ContactPerson },
                        new SqlParameter("@ContactNumber", SqlDbType.NVarChar,-1) { Value = ContactNumber },
                        new SqlParameter("@TradeName", SqlDbType.NVarChar,-1) { Value = TradeName },
                     new SqlParameter("@Logo", SqlDbType.Binary) { Value = Logo!= null ? Logo: DBNull.Value },

                     new SqlParameter("@CreationDate", SqlDbType.DateTime) { Value = DateTime.Now },
                            new SqlParameter("@DataBaseName", SqlDbType.NVarChar,-1) { Value = DataBaseName },


                };

                string a = @" SET IDENTITY_INSERT tbl_Company ON;

INSERT INTO tbl_Company (
    ID, AName, EName, Email, Address, Tel1, Tel2, ContactPerson, ContactNumber, TradeName, Logo, CreationDate, DataBaseName
)
OUTPUT INSERTED.ID 
VALUES (
    @ID, @AName, @EName, @Email, @Address, @Tel1, @Tel2, @ContactPerson, @ContactNumber, @TradeName, @Logo, @CreationDate, @DataBaseName
);

SET IDENTITY_INSERT tbl_Company OFF;";
                clsSQL clsSQL = new clsSQL();

                return Simulate.Integer32(clsSQL.ExecuteScalar(a, prm, conString));

            }
            catch (Exception)
            {

                throw;
            }


        }
        public int UpdateCompanyDataBaseName(int ID, string DataBaseName) {
            try
            {
                SqlParameter[] prm =
                 {
                     new SqlParameter("@ID", SqlDbType.Int) { Value = ID },
                  new SqlParameter("@DataBaseName", SqlDbType.NVarChar,-1) { Value = DataBaseName }, 
                }; clsSQL clsSQL = new clsSQL();

                int A = clsSQL.ExecuteNonQueryStatement(@"update tbl_Company set 
                       DataBaseName=@DataBaseName 
                   where id =@id", clsSQL.CreateDataBaseConnectionString(ID), prm);

                return A;
            }
            catch (Exception)
            {

                throw;
            }
        }


      
        public int UpdateCompany(int ID, string AName, string EName, string Email
            , string Address, string Tel1, string Tel2, string ContactPerson,
            string ContactNumber, byte[] Logo, string TradeName, int ModificationUserId,int CompanyID,
            bool EnableTouchScreenPosLogin = false)
        {
            try
            {
                SqlParameter[] prm =
                 {
                     new SqlParameter("@ID", SqlDbType.Int) { Value = ID },
                  new SqlParameter("@AName", SqlDbType.NVarChar,-1) { Value = AName },
                  new SqlParameter("@EName", SqlDbType.NVarChar,-1) { Value = EName },
                    new SqlParameter("@Email", SqlDbType.NVarChar,-1) { Value = Email },
                    new SqlParameter("@Address", SqlDbType.NVarChar,-1) { Value = Address },
                      new SqlParameter("@Tel1", SqlDbType.NVarChar,-1) { Value = Tel1 },
                        new SqlParameter("@Tel2", SqlDbType.NVarChar,-1) { Value = Tel2 },
                        new SqlParameter("@ContactPerson", SqlDbType.NVarChar,-1) { Value = ContactPerson },
                        new SqlParameter("@ContactNumber", SqlDbType.NVarChar,-1) { Value = ContactNumber },
                        new SqlParameter("@TradeName", SqlDbType.NVarChar,-1) { Value = TradeName },
                     new SqlParameter("@Logo", SqlDbType.Binary) { Value = Logo!= null ? Logo: DBNull.Value },
                         new SqlParameter("@ModificationUserId", SqlDbType.Int) { Value = ModificationUserId },
                     new SqlParameter("@ModificationDate", SqlDbType.DateTime) { Value = DateTime.Now },
                     new SqlParameter("@EnableTouchScreenPosLogin", SqlDbType.Bit) { Value = EnableTouchScreenPosLogin },
                }; clsSQL clsSQL = new clsSQL();

                int A = clsSQL.ExecuteNonQueryStatement(@"update tbl_Company set 
                       AName=@AName,
                       EName=@EName,
                       Email=@Email,
                       Address=@Address,
                       Tel1=@Tel1,
                       Tel2=@Tel2,
                       ContactPerson=@ContactPerson,
                       ContactNumber=@ContactNumber,
                       TradeName=@TradeName,
                       Logo=@Logo,
                       ModificationDate=@ModificationDate,
                       ModificationUserId=@ModificationUserId,
                       EnableTouchScreenPosLogin=@EnableTouchScreenPosLogin
                   where id =@id", clsSQL.CreateDataBaseConnectionString(CompanyID), prm);

                return A;
            }
            catch (Exception)
            {

                throw;
            }


        }

        /// <summary>
        /// Returns whether touch-screen POS login is enabled for the company (tenant DB).
        /// Missing column / errors default to false.
        /// </summary>
        public bool IsTouchScreenPosLoginEnabled(int companyId)
        {
            try
            {
                if (companyId <= 0) return false;
                clsSQL clsSQL = new clsSQL();
                string con = clsSQL.CreateDataBaseConnectionString(companyId);
                if (string.IsNullOrWhiteSpace(con)) return false;

                object val = clsSQL.ExecuteScalar(
                    @"SELECT TOP 1 ISNULL(EnableTouchScreenPosLogin, 0)
                      FROM tbl_Company WHERE ID = @ID OR @ID = 0",
                    new[] { new SqlParameter("@ID", SqlDbType.Int) { Value = companyId } },
                    con);
                return Simulate.Bool(val);
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// System users for touch POS login — never includes Password.
        /// </summary>
        public DataTable SelectTouchPosUsers(int companyId)
        {
            try
            {
                clsSQL clsSQL = new clsSQL();
                SqlParameter[] prm =
                {
                    new SqlParameter("@CompanyId", SqlDbType.Int) { Value = companyId },
                };
                return clsSQL.ExecuteQueryStatement(@"
SELECT ID, AName, EName, UserName, Email
FROM tbl_employee
WHERE IsSystemUser = 1
  AND (CompanyId = @CompanyId OR @CompanyId = 0)
  AND ISNULL(UserName, '') <> ''
ORDER BY EName, AName, UserName",
                    clsSQL.CreateDataBaseConnectionString(companyId), prm);
            }
            catch
            {
                throw;
            }
        }
    }
}
