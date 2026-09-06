using System;
using System.Text.RegularExpressions;

namespace WebApplication2.cls
{
    /// <summary>Jordan-specific HR field validation helpers.</summary>
    public static class clsJordanHrValidators
    {
        /// <summary>Jordan national number is typically 10 digits.</summary>
        public static bool IsValidNationalNumber(string value, out string message)
        {
            message = "";
            string digits = Regex.Replace(Simulate.String(value), @"\D", "");
            if (string.IsNullOrWhiteSpace(digits))
                return true;
            if (digits.Length != 10)
            {
                message = "National number must be 10 digits.";
                return false;
            }
            return true;
        }

        /// <summary>SSC subscription number is typically 8–12 digits.</summary>
        public static bool IsValidSocialSecurityNumber(string value, out string message)
        {
            message = "";
            string digits = Regex.Replace(Simulate.String(value), @"\D", "");
            if (string.IsNullOrWhiteSpace(digits))
                return true;
            if (digits.Length < 8 || digits.Length > 12)
            {
                message = "Social security number must be 8–12 digits.";
                return false;
            }
            return true;
        }

        public static decimal ResolveMinimumWage(int companyId)
        {
            try
            {
                clsSQL sql = new clsSQL();
                Microsoft.Data.SqlClient.SqlParameter[] prm =
                {
                    new Microsoft.Data.SqlClient.SqlParameter("@CompanyID", System.Data.SqlDbType.Int) { Value = companyId },
                };
                object val = sql.ExecuteScalar(@"
SELECT TOP 1 ISNULL(MinSubjectWage, 0)
FROM tbl_StatutoryRate
WHERE CompanyID = @CompanyID AND CountryPack = N'JO' AND ISNULL(IsActive,1)=1
ORDER BY EffectiveFrom DESC",
                    prm, sql.CreateDataBaseConnectionString(companyId), null);
                decimal min = Simulate.Decimal(val);
                return min > 0 ? min : 260m;
            }
            catch
            {
                return 260m;
            }
        }

        public static void ValidateContractBasicSalary(decimal basicSalary, int companyId)
        {
            decimal minWage = ResolveMinimumWage(companyId);
            if (basicSalary > 0 && basicSalary < minWage)
                throw new Exception($"Basic salary ({basicSalary:N3}) is below Jordan minimum wage ({minWage:N3}).");
        }
    }
}
