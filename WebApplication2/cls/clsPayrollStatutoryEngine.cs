using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace WebApplication2.cls
{
    /// <summary>
    /// Applies country-pack statutory deductions (SSC + income tax) to payroll preview lines.
    /// </summary>
    public class clsPayrollStatutoryEngine
    {
        public void ApplyStatutory(
            int employeeId,
            int payrollPeriodId,
            int companyId,
            List<PayrollDetailModel> details,
            Dictionary<string, decimal> variables,
            Dictionary<int, SalariesElementModel> elementMap)
        {
            if (details == null) return;

            string countryPack = ResolveCountryPack(companyId);
            if (string.IsNullOrWhiteSpace(countryPack))
                countryPack = "JO";

            DateTime periodStart;
            DateTime periodEnd;
            new clsPayrollPeriod().GetPeriodDates(payrollPeriodId, out periodStart, out periodEnd, companyId);
            // Prefer calendar-month fraction when period looks like a full month
            decimal periodMonths;
            if (periodStart.Day == 1 && periodEnd.Day >= 28 && periodStart.Month == periodEnd.Month)
                periodMonths = 1m;
            else
                periodMonths = Math.Max(0.1m, Math.Round(((periodEnd.Date - periodStart.Date).Days + 1m) / 30m, 4));

            DataRow rate = LoadActiveStatutoryRate(countryPack, periodEnd, companyId);
            if (rate != null)
            {
                ApplySocialSecurity(details, variables, elementMap, rate, companyId);
            }

            // Tax: JO (and packs with brackets). GCC SA/AE/Generic: skip tax when no brackets.
            DataTable brackets = LoadTaxBrackets(countryPack, periodEnd, companyId);
            if (brackets != null && brackets.Rows.Count > 0)
            {
                ApplyIncomeTax(details, variables, elementMap, brackets, periodMonths, companyId);
            }
        }

        string ResolveCountryPack(int companyId)
        {
            try
            {
                clsSQL sql = new clsSQL();
                SqlParameter[] prm = { new SqlParameter("@ID", SqlDbType.Int) { Value = companyId } };
                object val = sql.ExecuteScalar(
                    "SELECT TOP 1 ISNULL(PayrollCountryPack, N'JO') FROM tbl_Company WHERE ID = @ID",
                    prm,
                    sql.MainDataBaseconString,
                    null);
                string pack = Simulate.String(val);
                return string.IsNullOrWhiteSpace(pack) ? "JO" : pack.Trim().ToUpperInvariant();
            }
            catch
            {
                return "JO";
            }
        }

        DataRow LoadActiveStatutoryRate(string countryPack, DateTime asOf, int companyId)
        {
            clsSQL sql = new clsSQL();
            SqlParameter[] prm =
            {
                new SqlParameter("@CountryPack", SqlDbType.NVarChar, 10) { Value = countryPack },
                new SqlParameter("@AsOf", SqlDbType.DateTime) { Value = asOf.Date },
                new SqlParameter("@CompanyID", SqlDbType.Int) { Value = companyId },
            };
            DataTable dt = sql.ExecuteQueryStatement(@"
SELECT TOP 1 *
FROM tbl_StatutoryRate
WHERE CountryPack = @CountryPack
  AND CompanyID = @CompanyID
  AND ISNULL(IsActive,1) = 1
  AND EffectiveFrom <= @AsOf
ORDER BY EffectiveFrom DESC",
                sql.CreateDataBaseConnectionString(companyId), prm);
            return (dt != null && dt.Rows.Count > 0) ? dt.Rows[0] : null;
        }

        DataTable LoadTaxBrackets(string countryPack, DateTime asOf, int companyId)
        {
            clsSQL sql = new clsSQL();
            SqlParameter[] prm =
            {
                new SqlParameter("@CountryPack", SqlDbType.NVarChar, 10) { Value = countryPack },
                new SqlParameter("@AsOf", SqlDbType.DateTime) { Value = asOf.Date },
                new SqlParameter("@CompanyID", SqlDbType.Int) { Value = companyId },
            };
            // Use latest EffectiveFrom set for this pack
            return sql.ExecuteQueryStatement(@"
DECLARE @Eff DATETIME = (
  SELECT MAX(EffectiveFrom) FROM tbl_IncomeTaxBracket
  WHERE CountryPack=@CountryPack AND CompanyID=@CompanyID AND EffectiveFrom <= @AsOf);
SELECT * FROM tbl_IncomeTaxBracket
WHERE CountryPack=@CountryPack AND CompanyID=@CompanyID AND EffectiveFrom=@Eff
ORDER BY FromAmount",
                sql.CreateDataBaseConnectionString(companyId), prm);
        }

        void ApplySocialSecurity(
            List<PayrollDetailModel> details,
            Dictionary<string, decimal> variables,
            Dictionary<int, SalariesElementModel> elementMap,
            DataRow rate,
            int companyId)
        {
            decimal eePct = Simulate.Decimal(rate["EmployeePercent"]);
            decimal erPct = Simulate.Decimal(rate["EmployerPercent"]);
            decimal ceiling = Simulate.Decimal(rate["WageCeiling"]);
            decimal minWage = Simulate.Decimal(rate["MinSubjectWage"]);

            decimal subject = 0;
            foreach (var d in details)
            {
                if (d.IsAffectSocialSecurity)
                    subject += Math.Abs(d.Amount);
                else if (clsPayrollEngine.IsBasicCode(d.BasicSalaryCode) &&
                         elementMap != null &&
                         elementMap.TryGetValue(d.SalaryElementID, out var el) &&
                         el.IsAffectSocialSecurity)
                    subject += Math.Abs(d.Amount);
            }

            // Also include BASIC from variables if flagged on any BASIC-coded detail
            if (subject <= 0 && variables != null && variables.ContainsKey("BASIC"))
            {
                // Fallback: many installs flag BASIC as SS-subject on the master element
                bool basicAffects = details.Any(d =>
                    clsPayrollEngine.IsBasicCode(d.BasicSalaryCode) && d.IsAffectSocialSecurity);
                if (basicAffects)
                    subject = Math.Abs(variables["BASIC"]);
            }

            if (subject < minWage) subject = 0;
            if (ceiling > 0 && subject > ceiling) subject = ceiling;
            if (subject <= 0) return;

            decimal eeAmount = Math.Round(subject * eePct / 100m, 3);
            decimal erAmount = Math.Round(subject * erPct / 100m, 3);

            AddOrReplaceSystemLine(details, variables, companyId, "SS_EE", eeAmount, clsPayrollEngine.ElementTypeDeduction, "STATUTORY");
            AddOrReplaceSystemLine(details, variables, companyId, "SS_ER", erAmount, clsPayrollEngine.ElementTypeEmployerContribution, "STATUTORY");
        }

        void ApplyIncomeTax(
            List<PayrollDetailModel> details,
            Dictionary<string, decimal> variables,
            Dictionary<int, SalariesElementModel> elementMap,
            DataTable brackets,
            decimal periodMonths,
            int companyId)
        {
            decimal taxablePeriod = 0;
            foreach (var d in details)
            {
                if (d.IsTaxable || (clsPayrollEngine.IsBasicCode(d.BasicSalaryCode) && d.IsTaxable))
                    taxablePeriod += Math.Abs(d.Amount);
            }

            if (taxablePeriod <= 0) return;

            // Annualize then apply brackets; prorate tax back to period.
            decimal months = periodMonths <= 0 ? 1m : periodMonths;
            decimal annualTaxable = taxablePeriod * (12m / months);

            decimal personalExemption = Simulate.Decimal(brackets.Rows[0]["PersonalExemption"]);
            decimal remaining = Math.Max(0, annualTaxable - personalExemption);
            decimal annualTax = 0;

            foreach (DataRow b in brackets.Rows)
            {
                if (remaining <= 0) break;
                decimal fromAmt = Simulate.Decimal(b["FromAmount"]);
                decimal toAmt = Simulate.Decimal(b["ToAmount"]);
                decimal ratePct = Simulate.Decimal(b["RatePercent"]);
                if (ratePct <= 0) continue;

                // Bracket width (inclusive tiers stored as 5001–10000 etc.)
                decimal bandStart = fromAmt <= 0 ? 0 : fromAmt;
                decimal bandEnd = toAmt <= 0 ? decimal.MaxValue : toAmt;
                decimal bandWidth = bandEnd - (bandStart > 0 ? bandStart - 1 : 0);
                if (bandWidth <= 0) continue;

                // Amount of taxable income that falls in this band after exemption already subtracted from remaining
                // remaining is post-exemption annual income; map onto absolute brackets by walking sequentially.
                decimal slice = Math.Min(remaining, bandWidth);
                annualTax += Math.Round(slice * ratePct / 100m, 3);
                remaining -= slice;
            }

            decimal periodTax = Math.Round(annualTax * (months / 12m), 3);
            if (periodTax <= 0) return;

            AddOrReplaceSystemLine(details, variables, companyId, "TAX_EE", periodTax, clsPayrollEngine.ElementTypeDeduction, "STATUTORY");
        }

        void AddOrReplaceSystemLine(
            List<PayrollDetailModel> details,
            Dictionary<string, decimal> variables,
            int companyId,
            string code,
            decimal amount,
            int elementTypeId,
            string systemSource)
        {
            if (amount <= 0) return;

            int elementId = ResolveElementIdByCode(code, companyId);
            if (elementId <= 0) return;

            details.RemoveAll(d =>
                d.IsSystemGenerated &&
                string.Equals(d.BasicSalaryCode, code, StringComparison.OrdinalIgnoreCase));

            string name = code;
            DataTable el = new clsSalariesElements().SelectSalariesElements(elementId, "", "", "", companyId);
            if (el != null && el.Rows.Count > 0)
                name = Simulate.String(el.Rows[0]["AName"]);

            details.Add(new PayrollDetailModel
            {
                SalaryElementID = elementId,
                ElementName = name,
                Amount = amount,
                ElementTypeID = elementTypeId,
                BasicSalaryCode = code,
                IsAffectSocialSecurity = false,
                IsTaxable = false,
                IsSystemGenerated = true,
                SystemSource = systemSource
            });

            if (variables != null)
                variables[code] = amount;
        }

        int ResolveElementIdByCode(string code, int companyId)
        {
            DataTable dt = new clsSalariesElements().SelectSalariesElements(0, code, "", "", companyId);
            if (dt == null || dt.Rows.Count == 0) return 0;
            return Simulate.Integer32(dt.Rows[0]["ID"]);
        }
    }
}
