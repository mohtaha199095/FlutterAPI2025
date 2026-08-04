using Microsoft.Data.SqlClient;
using System;
using System.Data;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using static WebApplication2.MainClasses.clsEnum;

namespace WebApplication2.cls
{
    public class clsForgotPasswordRequest
    {
        public const int OtpExpiryMinutes = 15;
        public const int MinSecondsBetweenRequests = 60;
        public const int MaxRequestsPerHour = 5;
        public const int MaxVerifyAttemptsPerOtp = 5;

        public static string NormalizeEmail(string email)
        {
            return (email ?? string.Empty).Trim().ToLowerInvariant();
        }

        public static string NormalizePhoneDigits(string phone)
        {
            if (string.IsNullOrWhiteSpace(phone)) return string.Empty;
            var sb = new StringBuilder();
            foreach (char c in phone)
            {
                if (char.IsDigit(c)) sb.Append(c);
            }
            return sb.ToString();
        }

        /// <summary>Last 9 digits after stripping Jordan country code (962) or leading 0.</summary>
        public static string CanonicalMobileSuffix(string phoneDigits)
        {
            string digits = NormalizePhoneDigits(phoneDigits);
            if (digits.Length == 0) return string.Empty;
            if (digits.StartsWith("962", StringComparison.Ordinal) && digits.Length > 3)
            {
                digits = digits.Substring(3);
            }
            if (digits.StartsWith("0", StringComparison.Ordinal) && digits.Length > 1)
            {
                digits = digits.Substring(1);
            }
            if (digits.Length > 9)
            {
                digits = digits.Substring(digits.Length - 9);
            }
            return digits;
        }

        public DataTable FindEmployeesByEmailOrLogin(int companyId, string email)
        {
            try
            {
                string normalizedEmail = NormalizeEmail(email);
                SqlParameter[] prm =
                {
                    new SqlParameter("@Email", SqlDbType.NVarChar, 320) { Value = normalizedEmail },
                    new SqlParameter("@CompanyId", SqlDbType.Int) { Value = companyId },
                };
                clsSQL clsSQL = new clsSQL();
                string conn = clsSQL.CreateDataBaseConnectionString(companyId);
                if (string.IsNullOrWhiteSpace(conn))
                {
                    return null;
                }

                return clsSQL.ExecuteQueryStatement(
                    @"select * from tbl_employee
where (CompanyID = @CompanyId OR CompanyId = @CompanyId)
and (
    LOWER(LTRIM(RTRIM(ISNULL(Email, '')))) = @Email
    OR LOWER(LTRIM(RTRIM(ISNULL(UserName, '')))) = @Email
)",
                    conn,
                    prm);
            }
            catch (Exception)
            {
                throw;
            }
        }

        public static System.Collections.Generic.List<DataRow> FilterByPhone(DataTable employees, string phoneNumber)
        {
            if (employees == null || employees.Rows.Count == 0)
            {
                return new System.Collections.Generic.List<DataRow>();
            }

            return employees.AsEnumerable()
                .Where(r => EmployeeMatchesPhone(r, phoneNumber))
                .ToList();
        }

        public static string GenerateSecureOtp()
        {
            Span<byte> bytes = stackalloc byte[4];
            RandomNumberGenerator.Fill(bytes);
            int value = BitConverter.ToInt32(bytes) & int.MaxValue;
            return (value % 900000 + 100000).ToString();
        }

        public static int GetEmployeeIdFromRow(DataRow row)
        {
            if (row == null) return 0;
            int id = Simulate.Integer32(row["EmployeeID"]);
            if (id > 0) return id;
            return Simulate.Integer32(Simulate.String(row["EmployeeID"]));
        }

        public DataTable SelectForgotPasswordRequestById(int requestId, int companyId)
        {
            try
            {
                SqlParameter[] prm =
                {
                    new SqlParameter("@Id", SqlDbType.Int) { Value = requestId },
                    new SqlParameter("@CompanyId", SqlDbType.Int) { Value = companyId },
                    new SqlParameter("@creationdate", SqlDbType.DateTime) { Value = DateTime.Now.AddMinutes(-OtpExpiryMinutes) },
                };
                clsSQL clsSQL = new clsSQL();
                return clsSQL.ExecuteQueryStatement(
                    @"select top 1 *
from tbl_ForgotPasswordRequest
where ID = @Id
and (CompanyId = @CompanyId or CompanyID = @CompanyId)
and CreationDate > @creationdate",
                    clsSQL.CreateDataBaseConnectionString(companyId),
                    prm);
            }
            catch (Exception)
            {
                throw;
            }
        }

        public DataTable SelectForgotPasswordRequest(string email, string otp, int companyId)
        {
            try
            {
                string normalizedEmail = NormalizeEmail(email);
                SqlParameter[] prm =
                 {
                    new SqlParameter("@Email", SqlDbType.NVarChar, 320) { Value = normalizedEmail },
                    new SqlParameter("@GeneratedPassword", SqlDbType.NVarChar, 32) { Value = otp ?? string.Empty },
                    new SqlParameter("@CompanyId", SqlDbType.Int) { Value = companyId },
                    new SqlParameter("@creationdate", SqlDbType.DateTime) { Value = DateTime.Now.AddMinutes(-OtpExpiryMinutes) },
                };
                clsSQL clsSQL = new clsSQL();
                DataTable dt = clsSQL.ExecuteQueryStatement(@"select top 1 *
from tbl_ForgotPasswordRequest
where (CompanyId = @CompanyId or CompanyID = @CompanyId)
and LOWER(LTRIM(RTRIM(Email))) = @Email
and GeneratedPassword = @GeneratedPassword
and CreationDate > @creationdate
order by CreationDate desc", clsSQL.CreateDataBaseConnectionString(companyId), prm);

                return dt;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public int CountRecentRequests(string email, int companyId, int withinMinutes)
        {
            try
            {
                string normalizedEmail = NormalizeEmail(email);
                SqlParameter[] prm =
                {
                    new SqlParameter("@Email", SqlDbType.NVarChar, 320) { Value = normalizedEmail },
                    new SqlParameter("@CompanyId", SqlDbType.Int) { Value = companyId },
                    new SqlParameter("@Since", SqlDbType.DateTime) { Value = DateTime.Now.AddMinutes(-withinMinutes) },
                };
                clsSQL clsSQL = new clsSQL();
                object scalar = clsSQL.ExecuteScalar(
                    @"select count(1) from tbl_ForgotPasswordRequest
where CompanyId = @CompanyId
and LOWER(LTRIM(RTRIM(Email))) = @Email
and CreationDate > @Since",
                    prm,
                    clsSQL.CreateDataBaseConnectionString(companyId));
                return Simulate.Integer32(scalar);
            }
            catch (Exception)
            {
                throw;
            }
        }

        public DateTime? GetLastRequestTime(string email, int companyId)
        {
            try
            {
                string normalizedEmail = NormalizeEmail(email);
                SqlParameter[] prm =
                {
                    new SqlParameter("@Email", SqlDbType.NVarChar, 320) { Value = normalizedEmail },
                    new SqlParameter("@CompanyId", SqlDbType.Int) { Value = companyId },
                };
                clsSQL clsSQL = new clsSQL();
                object scalar = clsSQL.ExecuteScalar(
                    @"select max(CreationDate) from tbl_ForgotPasswordRequest
where CompanyId = @CompanyId
and LOWER(LTRIM(RTRIM(Email))) = @Email",
                    prm,
                    clsSQL.CreateDataBaseConnectionString(companyId));
                if (scalar == null || scalar == DBNull.Value) return null;
                return Convert.ToDateTime(scalar);
            }
            catch (Exception)
            {
                throw;
            }
        }

        public void InvalidatePendingForEmployee(int companyId, int employeeId)
        {
            try
            {
                SqlParameter[] prm =
                {
                    new SqlParameter("@CompanyId", SqlDbType.Int) { Value = companyId },
                    new SqlParameter("@EmployeeID", SqlDbType.Int) { Value = employeeId },
                    new SqlParameter("@Since", SqlDbType.DateTime) { Value = DateTime.Now.AddMinutes(-OtpExpiryMinutes) },
                };
                clsSQL clsSQL = new clsSQL();
                clsSQL.ExecuteScalar(
                    @"delete from tbl_ForgotPasswordRequest
where CompanyId = @CompanyId
and EmployeeID = @EmployeeID
and CreationDate > @Since",
                    prm,
                    clsSQL.CreateDataBaseConnectionString(companyId));
            }
            catch (Exception)
            {
                throw;
            }
        }

        public int InsertForgotPasswordRequest(int companyId, string email, string tel1, string generatedPassword, int employeeId)
        {
            try
            {
                string normalizedEmail = NormalizeEmail(email);
                SqlParameter[] prm =
                {
                    new SqlParameter("@CompanyId", SqlDbType.Int) { Value = companyId },
                    new SqlParameter("@Email", SqlDbType.NVarChar, 320) { Value = normalizedEmail },
                    new SqlParameter("@Tel1", SqlDbType.NVarChar, 64) { Value = tel1 ?? string.Empty },
                    new SqlParameter("@GeneratedPassword", SqlDbType.NVarChar, 32) { Value = generatedPassword },
                    new SqlParameter("@EmployeeID", SqlDbType.Int) { Value = employeeId },
                };

                string a = @"insert into tbl_ForgotPasswordRequest(CompanyId,Email,Tel1,GeneratedPassword,EmployeeID)
                         values(@CompanyId,@Email,@Tel1,@GeneratedPassword,@EmployeeID)";
                clsSQL clsSQL = new clsSQL();
                return Simulate.Integer32(clsSQL.ExecuteScalar(a, prm, clsSQL.CreateDataBaseConnectionString(companyId)));
            }
            catch (Exception)
            {
                throw;
            }
        }

        public void ConsumeForgotPasswordRequest(int requestId, int companyId)
        {
            try
            {
                SqlParameter[] prm =
                {
                    new SqlParameter("@Id", SqlDbType.Int) { Value = requestId },
                    new SqlParameter("@CompanyId", SqlDbType.Int) { Value = companyId },
                };
                clsSQL clsSQL = new clsSQL();
                clsSQL.ExecuteScalar(
                    @"delete from tbl_ForgotPasswordRequest where ID = @Id and CompanyId = @CompanyId",
                    prm,
                    clsSQL.CreateDataBaseConnectionString(companyId));
            }
            catch (Exception)
            {
                throw;
            }
        }

        public static bool EmployeeMatchesPhone(DataRow row, string phoneNumber)
        {
            string storedRaw = Simulate.String(row["Tel1"]);
            string providedRaw = phoneNumber ?? string.Empty;

            string stored = CanonicalMobileSuffix(storedRaw);
            string provided = CanonicalMobileSuffix(providedRaw);

            if (stored.Length == 0)
            {
                return NormalizePhoneDigits(providedRaw).Length > 0;
            }
            if (provided.Length == 0) return false;

            if (string.Equals(stored, provided, StringComparison.Ordinal))
            {
                return true;
            }

            string storedFull = NormalizePhoneDigits(storedRaw);
            string providedFull = NormalizePhoneDigits(providedRaw);
            if (storedFull.Length > 0 && string.Equals(storedFull, providedFull, StringComparison.Ordinal))
            {
                return true;
            }

            return stored.EndsWith(provided, StringComparison.Ordinal)
                || provided.EndsWith(stored, StringComparison.Ordinal);
        }

        public static string PhoneHintSuffix(DataRow row)
        {
            string suffix = CanonicalMobileSuffix(Simulate.String(row["Tel1"]));
            if (suffix.Length < 4) return string.Empty;
            return suffix.Substring(suffix.Length - 4);
        }
    }
}
