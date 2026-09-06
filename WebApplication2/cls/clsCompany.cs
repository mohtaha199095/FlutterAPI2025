using DocumentFormat.OpenXml.Wordprocessing;
using System;
using System.Collections.Generic;
using System.Data;
using Microsoft.Data.SqlClient;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;

namespace WebApplication2.cls
{
    public class clsCompany
    {
        private static string NormalizePhoneDigits(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return string.Empty;
            }

            StringBuilder digits = new StringBuilder();
            foreach (char character in value)
            {
                if (char.IsDigit(character))
                {
                    int digit = (int)char.GetNumericValue(character);
                    if (digit >= 0 && digit <= 9)
                    {
                        digits.Append(digit);
                    }
                }
            }

            return digits.ToString();
        }

        private static string NormalizeCompanyName(string value)
        {
            string normalized = (value ?? string.Empty)
                .Trim()
                .Replace('أ', 'ا')
                .Replace('إ', 'ا')
                .Replace('آ', 'ا')
                .Replace('ى', 'ي');

            return Regex.Replace(normalized, @"\s+", " ");
        }

        private static string CanonicalPhoneSearch(string phoneDigits)
        {
            string canonical = phoneDigits ?? string.Empty;
            if (canonical.StartsWith("00962", StringComparison.Ordinal))
            {
                canonical = canonical.Substring(5);
            }
            else if (canonical.StartsWith("962", StringComparison.Ordinal))
            {
                canonical = canonical.Substring(3);
            }

            if (canonical.StartsWith("0", StringComparison.Ordinal))
            {
                canonical = canonical.Substring(1);
            }

            return canonical.Length > 9
                ? canonical.Substring(canonical.Length - 9)
                : canonical;
        }

        public DataTable SelectCompany(int Id, string AName, string EName, string Tel1,int CompanyID,string PartOfTheName,bool fromMainDB)
        {
            try
            {
                clsSQL clsSQL = new clsSQL();

                string phoneDigits = NormalizePhoneDigits(Tel1);
                string phoneSearch = CanonicalPhoneSearch(phoneDigits);
                string normalizedName = NormalizeCompanyName(PartOfTheName);

                List<SqlParameter> parameters = new List<SqlParameter>
                {
                    new SqlParameter("@Id", SqlDbType.Int) { Value = Id },
                    new SqlParameter("@AName", SqlDbType.NVarChar, -1) { Value = AName ?? string.Empty },
                    new SqlParameter("@EName", SqlDbType.NVarChar, -1) { Value = EName ?? string.Empty },
                    new SqlParameter("@PhoneDigits", SqlDbType.NVarChar, 64) { Value = phoneDigits },
                    new SqlParameter("@PhoneSearch", SqlDbType.NVarChar, 9) { Value = phoneSearch }
                };

                const string searchableNames = @"
REPLACE(REPLACE(REPLACE(REPLACE(
    CONCAT(ISNULL(AName, ''), N' ', ISNULL(EName, ''), N' ', ISNULL(TradeName, '')),
    N'أ', N'ا'), N'إ', N'ا'), N'آ', N'ا'), N'ى', N'ي')";
                const string normalizedTel1 = "REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(ISNULL(Tel1, ''), '+', ''), ' ', ''), '-', ''), '(', ''), ')', ''), '.', '')";
                const string normalizedTel2 = "REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(ISNULL(Tel2, ''), '+', ''), ' ', ''), '-', ''), '(', ''), ')', ''), '.', '')";
                const string normalizedContact = "REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(ISNULL(ContactNumber, ''), '+', ''), ' ', ''), '-', ''), '(', ''), ')', ''), '.', '')";

                StringBuilder query = new StringBuilder(@"
SELECT *
FROM tbl_Company
WHERE (ID = @Id OR @Id = 0)
  AND (AName = @AName OR @AName = '')
  AND (EName = @EName OR @EName = '')");

                string[] nameParts = normalizedName.Split(
                    ' ',
                    StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
                for (int index = 0; index < nameParts.Length; index++)
                {
                    string parameterName = $"@NamePart{index}";
                    query.Append($"\n  AND {searchableNames} LIKE N'%' + {parameterName} + N'%'");
                    parameters.Add(new SqlParameter(parameterName, SqlDbType.NVarChar, -1)
                    {
                        Value = nameParts[index]
                    });
                }

                query.Append($@"
  AND (
      @PhoneDigits = ''
      OR (
          LEN(@PhoneSearch) >= 8
          AND (
              RIGHT({normalizedTel1}, 9) LIKE '%' + @PhoneSearch + '%'
              OR RIGHT({normalizedTel2}, 9) LIKE '%' + @PhoneSearch + '%'
              OR RIGHT({normalizedContact}, 9) LIKE '%' + @PhoneSearch + '%'
          )
      )
  )");

                string con = clsSQL.CreateDataBaseConnectionString(CompanyID);

                if (fromMainDB) {
                    con = clsSQL.MainDataBaseconString;


                }

                DataTable dt = clsSQL.ExecuteQueryStatement(
                    query.ToString(),
                    con,
                    parameters.ToArray());

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
            bool EnableTouchScreenPosLogin = false,
            bool EnableEcommerce = false,
            string WebSlug = "")
        {
            try
            {
                string normalizedSlug = NormalizeWebSlug(WebSlug);
                if (EnableEcommerce)
                {
                    if (string.IsNullOrEmpty(normalizedSlug))
                        throw new InvalidOperationException("Web shop slug is required when e-commerce is enabled.");
                    string conflict = FindWebSlugConflict(ID, normalizedSlug);
                    if (!string.IsNullOrEmpty(conflict))
                        throw new InvalidOperationException("Web shop slug is already used by another company.");
                }
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
                     new SqlParameter("@EnableEcommerce", SqlDbType.Bit) { Value = EnableEcommerce },
                     new SqlParameter("@WebSlug", SqlDbType.NVarChar, 80) { Value = (object)normalizedSlug ?? DBNull.Value },
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
                       EnableTouchScreenPosLogin=@EnableTouchScreenPosLogin,
                       EnableEcommerce=@EnableEcommerce,
                       WebSlug=@WebSlug
                   where ID = @ID
                      OR (
                           NOT EXISTS (SELECT 1 FROM tbl_Company WHERE ID = @ID)
                           AND ID = (SELECT TOP 1 ID FROM tbl_Company ORDER BY ID)
                         )", clsSQL.CreateDataBaseConnectionString(CompanyID), prm);

                // Keep main DB in sync so public /shop/{slug} can resolve the tenant.
                try
                {
                    SyncEcommerceFlagsToMainDb(ID, EnableEcommerce, normalizedSlug);
                }
                catch { /* non-blocking */ }

                return A;
            }
            catch (Exception)
            {

                throw;
            }


        }

        static string NormalizeWebSlug(string slug)
        {
            if (string.IsNullOrWhiteSpace(slug)) return "";
            string s = slug.Trim().ToLowerInvariant();
            var sb = new System.Text.StringBuilder(s.Length);
            foreach (char c in s)
            {
                if ((c >= 'a' && c <= 'z') || (c >= '0' && c <= '9') || c == '-')
                    sb.Append(c);
            }
            return sb.ToString();
        }

        string FindWebSlugConflict(int companyId, string webSlug)
        {
            try
            {
                clsSQL sql = new clsSQL();
                SqlParameter[] prm =
                {
                    new SqlParameter("@ID", SqlDbType.Int) { Value = companyId },
                    new SqlParameter("@Slug", SqlDbType.NVarChar, 80) { Value = webSlug },
                };
                // Any other company that already reserved this slug (enabled or not).
                object val = sql.ExecuteScalar(@"
SELECT TOP 1 CAST(ID AS NVARCHAR(20))
FROM tbl_Company
WHERE LOWER(LTRIM(RTRIM(ISNULL(WebSlug, '')))) = @Slug
  AND ID <> @ID
  AND LTRIM(RTRIM(ISNULL(WebSlug, ''))) <> ''",
                    prm, sql.MainDataBaseconString);
                return Simulate.String(val);
            }
            catch
            {
                return "";
            }
        }

        /// <summary>True when slug is free for this company on the main database.</summary>
        public bool IsWebSlugAvailable(int companyId, string webSlug)
        {
            string normalized = NormalizeWebSlug(webSlug);
            if (string.IsNullOrEmpty(normalized)) return false;
            return string.IsNullOrEmpty(FindWebSlugConflict(companyId, normalized));
        }

        void SyncEcommerceFlagsToMainDb(int companyId, bool enableEcommerce, string webSlug)
        {
            if (companyId <= 0) return;
            clsSQL sql = new clsSQL();
            SqlParameter[] prm =
            {
                new SqlParameter("@ID", SqlDbType.Int) { Value = companyId },
                new SqlParameter("@EnableEcommerce", SqlDbType.Bit) { Value = enableEcommerce },
                new SqlParameter("@WebSlug", SqlDbType.NVarChar, 80) { Value = (object)(webSlug ?? "") ?? DBNull.Value },
            };
            sql.ExecuteNonQueryStatement(@"
UPDATE tbl_Company
SET EnableEcommerce = @EnableEcommerce,
    WebSlug = @WebSlug
WHERE ID = @ID", sql.MainDataBaseconString, prm);
        }

        /// <summary>
        /// Returns whether e-commerce is enabled for the company (tenant DB).
        /// </summary>
        public bool IsEcommerceEnabled(int companyId)
        {
            try
            {
                if (companyId <= 0) return false;
                clsSQL clsSQL = new clsSQL();
                string con = clsSQL.CreateDataBaseConnectionString(companyId);
                if (string.IsNullOrWhiteSpace(con)) return false;

                object val = clsSQL.ExecuteScalar(
                    @"SELECT TOP 1 ISNULL(EnableEcommerce, 0)
                      FROM tbl_Company
                      ORDER BY CASE WHEN ID = @ID THEN 0 ELSE 1 END, ID",
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
                      FROM tbl_Company
                      ORDER BY CASE WHEN ID = @ID THEN 0 ELSE 1 END, ID",
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
  AND ISNULL(IsActive, 1) = 1
  AND ISNULL(ShowOnTouchLogin, 1) = 1
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
