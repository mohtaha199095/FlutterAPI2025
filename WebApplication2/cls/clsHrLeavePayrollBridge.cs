using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using WebApplication2.DataBaseTable;

namespace WebApplication2.cls
{
    /// <summary>Applies approved leave (unpaid / sick extended / tiered sick) to payroll preview.</summary>
    public class clsHrLeavePayrollBridge
    {
        public void ApplyLeaveAdjustments(
            int employeeId,
            int payrollPeriodId,
            int companyId,
            List<PayrollDetailModel> details,
            Dictionary<string, decimal> variables,
            Dictionary<int, SalariesElementModel> elementMap)
        {
            clsPayrollPeriod periodSvc = new clsPayrollPeriod();
            periodSvc.GetPeriodDates(payrollPeriodId, out DateTime startDate, out DateTime endDate, companyId);

            decimal basic = variables != null && variables.ContainsKey("BASIC")
                ? variables["BASIC"]
                : 0m;
            if (basic <= 0) return;

            decimal dailyRate = basic / 30m;

            decimal tieredSickDeduction = CalculateTieredSickDeduction(
                employeeId, startDate, endDate, dailyRate, companyId);
            if (tieredSickDeduction > 0)
                AddDeduction(details, variables, elementMap, companyId,
                    "SICK_TIER", "Sick Leave Tier Deduction / خصم إجازة مرضية",
                    tieredSickDeduction);

            decimal unpaidDays = CountUnpaidLeaveDays(employeeId, startDate, endDate, companyId);
            if (unpaidDays > 0)
            {
                decimal deduction = Math.Round(dailyRate * unpaidDays, 3);
                AddDeduction(details, variables, elementMap, companyId,
                    "LEAVE_UNPAID", "Leave Deduction / خصم إجازة", deduction);
            }
        }

        static void AddDeduction(
            List<PayrollDetailModel> details,
            Dictionary<string, decimal> variables,
            Dictionary<int, SalariesElementModel> elementMap,
            int companyId,
            string defaultCode,
            string defaultName,
            decimal deduction)
        {
            if (deduction <= 0) return;

            int elementId = ResolveElementId(defaultCode, companyId, elementMap);
            string elementName = defaultName;
            string code = defaultCode;

            if (elementId > 0 && elementMap != null && elementMap.ContainsKey(elementId))
            {
                elementName = elementMap[elementId].AName;
                code = elementMap[elementId].Code;
            }

            details.Add(new PayrollDetailModel
            {
                SalaryElementID = elementId,
                ElementName = elementName,
                Amount = -Math.Abs(deduction),
                ElementTypeID = clsPayrollEngine.ElementTypeDeduction,
                BasicSalaryCode = code,
                IsAffectSocialSecurity = false,
                IsTaxable = false
            });

            if (variables.ContainsKey(code))
                variables[code] += -Math.Abs(deduction);
            else
                variables[code] = -Math.Abs(deduction);
        }

        /// <summary>
        /// Jordan sick tiers: full-pay days from contract, then half-pay extended tier, then unpaid.
        /// Applies to approved SICK leave days in the payroll period.
        /// </summary>
        static decimal CalculateTieredSickDeduction(
            int employeeId, DateTime startDate, DateTime endDate, decimal dailyRate, int companyId)
        {
            int fullPayAllowance = 14;
            int halfPayAllowance = 14;
            LoadSickAllowances(employeeId, companyId, out fullPayAllowance, out halfPayAllowance);

            int year = startDate.Year;
            decimal usedSickBeforePeriod = CountApprovedSickDaysYtd(
                employeeId, year, companyId, endDate: startDate.AddDays(-1));

            decimal sickInPeriod = CountApprovedSickDaysInRange(
                employeeId, startDate, endDate, companyId, sickCodeOnly: true);
            if (sickInPeriod <= 0) return 0;

            decimal remainingFull = Math.Max(0, fullPayAllowance - usedSickBeforePeriod);
            decimal fullPayDays = Math.Min(sickInPeriod, remainingFull);
            decimal afterFull = sickInPeriod - fullPayDays;

            decimal usedInHalfTier = Math.Max(0, usedSickBeforePeriod - fullPayAllowance);
            decimal remainingHalf = Math.Max(0, halfPayAllowance - usedInHalfTier);
            decimal halfPayDays = Math.Min(afterFull, remainingHalf);
            decimal unpaidSickDays = afterFull - halfPayDays;

            decimal deduction = halfPayDays * dailyRate * 0.5m + unpaidSickDays * dailyRate;
            return Math.Round(deduction, 3);
        }

        static void LoadSickAllowances(int employeeId, int companyId, out int fullPayDays, out int halfPayDays)
        {
            fullPayDays = 14;
            halfPayDays = 14;
            clsSQL sql = new clsSQL();
            SqlParameter[] prm =
            {
                new SqlParameter("@EmployeeID", SqlDbType.Int) { Value = employeeId },
                new SqlParameter("@CompanyID", SqlDbType.Int) { Value = companyId },
            };
            DataTable dt = sql.ExecuteQueryStatement(@"
SELECT TOP 1
  ISNULL(SickLeaveFullPayDaysPerYear, 14) AS SickLeaveFullPayDaysPerYear,
  ISNULL(SickLeaveExtendedDaysPerYear, 14) AS SickLeaveExtendedDaysPerYear
FROM tbl_EmployeeContract
WHERE EmployeeID=@EmployeeID AND CompanyID=@CompanyID AND ISNULL(IsActive,0)=1
ORDER BY ID DESC",
                sql.CreateDataBaseConnectionString(companyId), prm);
            if (dt == null || dt.Rows.Count == 0) return;

            fullPayDays = Simulate.Integer32(dt.Rows[0]["SickLeaveFullPayDaysPerYear"]);
            halfPayDays = Simulate.Integer32(dt.Rows[0]["SickLeaveExtendedDaysPerYear"]);
            if (fullPayDays <= 0) fullPayDays = 14;
            if (halfPayDays <= 0) halfPayDays = 14;
        }

        static decimal CountApprovedSickDaysYtd(int employeeId, int year, int companyId, DateTime? endDate = null)
        {
            clsSQL sql = new clsSQL();
            SqlParameter[] prm =
            {
                new SqlParameter("@EmployeeID", SqlDbType.Int) { Value = employeeId },
                new SqlParameter("@CompanyID", SqlDbType.Int) { Value = companyId },
                new SqlParameter("@Year", SqlDbType.Int) { Value = year },
                new SqlParameter("@EndDate", SqlDbType.DateTime) { Value = endDate.HasValue ? endDate.Value.Date : new DateTime(year, 12, 31) },
            };
            DataTable dt = sql.ExecuteQueryStatement(@"
SELECT ISNULL(SUM(ISNULL(r.Days,0)),0) AS SickDays
FROM tbl_LeaveRequest r
INNER JOIN tbl_LeaveType t ON t.ID = r.LeaveTypeID
WHERE r.EmployeeID=@EmployeeID AND r.CompanyID=@CompanyID
  AND ISNULL(r.DocumentStatus,0)=2
  AND UPPER(ISNULL(t.Code,''))='SICK'
  AND YEAR(r.FromDate)=@Year
  AND r.FromDate <= @EndDate",
                sql.CreateDataBaseConnectionString(companyId), prm);
            if (dt == null || dt.Rows.Count == 0) return 0;
            return Simulate.Decimal(dt.Rows[0]["SickDays"]);
        }

        static decimal CountApprovedSickDaysInRange(
            int employeeId, DateTime startDate, DateTime endDate, int companyId, bool sickCodeOnly)
        {
            clsSQL sql = new clsSQL();
            SqlParameter[] prm =
            {
                new SqlParameter("@EmployeeID", SqlDbType.Int) { Value = employeeId },
                new SqlParameter("@CompanyID", SqlDbType.Int) { Value = companyId },
                new SqlParameter("@DateFrom", SqlDbType.DateTime) { Value = startDate.Date },
                new SqlParameter("@DateTo", SqlDbType.DateTime) { Value = endDate.Date },
            };
            string codeFilter = sickCodeOnly
                ? "AND UPPER(ISNULL(t.Code,''))='SICK'"
                : "AND UPPER(ISNULL(t.Code,'')) IN ('SICK','SICK_EXT')";

            DataTable dt = sql.ExecuteQueryStatement($@"
SELECT ISNULL(SUM(ISNULL(r.Days,0)),0) AS SickDays
FROM tbl_LeaveRequest r
INNER JOIN tbl_LeaveType t ON t.ID = r.LeaveTypeID
WHERE r.EmployeeID=@EmployeeID AND r.CompanyID=@CompanyID
  AND ISNULL(r.DocumentStatus,0)=2
  AND r.FromDate <= @DateTo AND r.ToDate >= @DateFrom
  {codeFilter}",
                sql.CreateDataBaseConnectionString(companyId), prm);
            if (dt == null || dt.Rows.Count == 0) return 0;
            return Simulate.Decimal(dt.Rows[0]["SickDays"]);
        }

        static decimal CountUnpaidLeaveDays(int employeeId, DateTime startDate, DateTime endDate, int companyId)
        {
            clsSQL sql = new clsSQL();
            SqlParameter[] prm =
            {
                new SqlParameter("@EmployeeID", SqlDbType.Int) { Value = employeeId },
                new SqlParameter("@CompanyID", SqlDbType.Int) { Value = companyId },
                new SqlParameter("@DateFrom", SqlDbType.DateTime) { Value = startDate.Date },
                new SqlParameter("@DateTo", SqlDbType.DateTime) { Value = endDate.Date },
            };

            DataTable dt = sql.ExecuteQueryStatement(@"
SELECT ISNULL(SUM(ISNULL(r.Days,0)),0) AS UnpaidDays
FROM tbl_LeaveRequest r
INNER JOIN tbl_LeaveType t ON t.ID = r.LeaveTypeID
WHERE r.EmployeeID = @EmployeeID
  AND r.CompanyID = @CompanyID
  AND ISNULL(r.DocumentStatus,0) = 2
  AND r.FromDate <= @DateTo
  AND r.ToDate >= @DateFrom
  AND (
    ISNULL(t.IsPaid,1) = 0
    OR UPPER(ISNULL(t.Code,'')) IN ('SICK_EXT','UNPAID')
  )",
                sql.CreateDataBaseConnectionString(companyId), prm);

            if (dt == null || dt.Rows.Count == 0) return 0;
            return Simulate.Decimal(dt.Rows[0]["UnpaidDays"]);
        }

        static int ResolveElementId(string code, int companyId, Dictionary<int, SalariesElementModel> elementMap)
        {
            if (elementMap != null)
            {
                foreach (var kv in elementMap)
                {
                    if (string.Equals(kv.Value.Code, code, StringComparison.OrdinalIgnoreCase))
                        return kv.Key;
                }
            }

            clsSQL sql = new clsSQL();
            SqlParameter[] prm =
            {
                new SqlParameter("@Code", SqlDbType.VarChar) { Value = code },
                new SqlParameter("@CompanyID", SqlDbType.Int) { Value = companyId },
            };
            object val = sql.ExecuteScalar(
                "SELECT TOP 1 ID FROM tbl_SalariesElements WHERE Code=@Code AND CompanyID=@CompanyID",
                prm, sql.CreateDataBaseConnectionString(companyId), null);
            return Simulate.Integer32(val);
        }
    }
}
