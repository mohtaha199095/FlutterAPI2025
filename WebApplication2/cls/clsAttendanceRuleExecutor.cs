using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using WebApplication2.cls.Reports;
using WebApplication2.DataBaseTable;

namespace WebApplication2.cls
{
    /// <summary>
    /// Evaluates attendance rules against daily attendance and produces payroll impact lines.
    /// Uses the current schema (tbl_AttendanceRules + mapping/conditions), not the legacy header table.
    /// </summary>
    public class clsAttendanceRuleExecutor
    {
        public List<PayrollImpactItem> ExecuteRules(
            List<AttendanceRuleModel> rules,
            AttendanceCalculationResult day,
            TblShift shift)
        {
            List<PayrollImpactItem> impact = new List<PayrollImpactItem>();
            clsAttendanceRuleEvaluator evaluator = new clsAttendanceRuleEvaluator();

            foreach (var rule in rules)
            {
                bool conditionsMet = true;
                if (rule.Conditions != null && rule.Conditions.Count > 0)
                {
                    foreach (var cond in rule.Conditions)
                    {
                        if (!evaluator.EvaluateCondition(cond, day, shift))
                        {
                            conditionsMet = false;
                            break;
                        }
                    }
                }
                else
                {
                    // Default gate by rule type when no explicit conditions exist
                    conditionsMet = DefaultRuleApplies(rule, day);
                }

                if (!conditionsMet) continue;

                decimal amount = CalculateAmount(rule, day);
                if (amount == 0) continue;

                if (rule.MinAmount > 0 && amount < rule.MinAmount)
                    amount = rule.MinAmount;
                if (rule.MaxAmount > 0 && amount > rule.MaxAmount)
                    amount = rule.MaxAmount;

                impact.Add(new PayrollImpactItem
                {
                    SalaryElementID = rule.SalaryElementID,
                    ElementName = string.IsNullOrWhiteSpace(rule.AName) ? rule.RuleName : rule.AName,
                    Amount = amount,
                    ElementTypeID = rule.ElementTypeID,
                    Code = rule.ElementCode
                });
            }

            return impact;
        }

        public List<PayrollImpactItem> ExecuteRulesForEmployee(int employeeId, int payrollPeriodId, int companyId)
        {
            var aggregated = new Dictionary<int, PayrollImpactItem>();

            DateTime startDate, endDate;
            clsPayrollPeriod pr = new clsPayrollPeriod();
            pr.GetPeriodDates(payrollPeriodId, out startDate, out endDate, companyId);

            int departmentId = GetEmployeeDepartmentId(employeeId, companyId);
            clsShiftResolverService shiftSvc = new clsShiftResolverService();
            clsAttendanceRuleService ruleSvc = new clsAttendanceRuleService();

            for (DateTime day = startDate.Date; day <= endDate.Date; day = day.AddDays(1))
            {
                int shiftId = shiftSvc.ResolveShiftForDay(employeeId, day, companyId);
                if (shiftId <= 0) continue;

                AttendanceCalculationResult calc =
                    shiftSvc.BuildAttendanceDay(employeeId, day, shiftId, companyId);
                if (calc == null) continue;

                ApplyJordanOvertimeCap(calc, companyId);

                TblShift shift = shiftSvc.LoadShift(shiftId, companyId);
                if (shift == null) continue;

                List<AttendanceRuleModel> rules =
                    ruleSvc.LoadRulesForEmployee(employeeId, departmentId, shiftId, companyId);

                EnrichRulesWithSalaryElementMeta(rules, companyId);

                List<PayrollImpactItem> dayImpact = ExecuteRules(rules, calc, shift);
                foreach (var item in dayImpact)
                {
                    if (item.SalaryElementID <= 0) continue;
                    if (aggregated.TryGetValue(item.SalaryElementID, out var existing))
                    {
                        existing.Amount += item.Amount;
                    }
                    else
                    {
                        aggregated[item.SalaryElementID] = new PayrollImpactItem
                        {
                            SalaryElementID = item.SalaryElementID,
                            ElementName = item.ElementName,
                            Amount = item.Amount,
                            ElementTypeID = item.ElementTypeID,
                            Code = item.Code
                        };
                    }
                }
            }

            return aggregated.Values.ToList();
        }

        /// <summary>
        /// Legacy path removed — always load from tbl_AttendanceRules via service.
        /// Kept for callers that still expect a DataTable of active rules.
        /// </summary>
        private DataTable LoadAllRules(int companyId)
        {
            clsSQL cls = new clsSQL();
            string q = @"
                SELECT R.*,
                       SE.ElementTypeID,
                       SE.Code AS ElementCode,
                       SE.AName AS ElementAName
                FROM tbl_AttendanceRules R
                LEFT JOIN tbl_SalariesElements SE ON SE.ID = R.SalaryElementID AND SE.CompanyID = R.CompanyID
                WHERE R.CompanyID = @CID AND R.IsActive = 1";
            SqlParameter[] prm = { new SqlParameter("@CID", companyId) };
            return cls.ExecuteQueryStatement(q, cls.CreateDataBaseConnectionString(companyId), prm);
        }

        public decimal CalculateAmount(AttendanceRuleModel r, AttendanceCalculationResult day)
        {
            switch (r.CalculationTypeID)
            {
                case 1: // Fixed value
                    return r.Value;

                case 2: // Percentage of relevant minutes (legacy: Value as % of minutes)
                    return GetRuleMinutes(r, day) * (r.Value / 100m);

                case 3: // Rate per hour of relevant minutes
                    return (GetRuleMinutes(r, day) / 60m) * r.Value;

                case 4: // Formula
                    return FormulaEvaluator.SafeEvaluate(
                        r.FormulaText,
                        day.ToVariableDictionary()
                    );
            }

            return 0;
        }

        private static int GetRuleMinutes(AttendanceRuleModel r, AttendanceCalculationResult day)
        {
            int minutes;
            switch (r.RuleTypeID)
            {
                case 1: minutes = day.LateMinutes; break;
                case 2: minutes = day.EarlyLeaveMinutes; break;
                case 3:
                case 5:
                case 6: minutes = day.OvertimeMinutes; break;
                case 4: minutes = day.StatusID == 2 ? Math.Max(day.WorkedMinutes, 1) : 0; break;
                default: minutes = day.WorkedMinutes; break;
            }
            return minutes;
        }

        /// <summary>Cap daily overtime minutes using company setting (default 120).</summary>
        static void ApplyJordanOvertimeCap(AttendanceCalculationResult day, int companyId)
        {
            int maxMinutes = ResolveMaxDailyOvertimeMinutes(companyId);
            if (maxMinutes <= 0) maxMinutes = 120;
            if (day.OvertimeMinutes > maxMinutes)
                day.OvertimeMinutes = maxMinutes;
        }

        static int ResolveMaxDailyOvertimeMinutes(int companyId)
        {
            try
            {
                clsSQL sql = new clsSQL();
                object val = sql.ExecuteScalar(
                    "SELECT TOP 1 ISNULL(MaxDailyOvertimeMinutes, 120) FROM tbl_Company",
                    null, sql.CreateDataBaseConnectionString(companyId), null);
                int minutes = Simulate.Integer32(val);
                return minutes > 0 ? minutes : 120;
            }
            catch
            {
                return 120;
            }
        }

        private static bool DefaultRuleApplies(AttendanceRuleModel rule, AttendanceCalculationResult day)
        {
            switch (rule.RuleTypeID)
            {
                case 1: return day.LateMinutes > 0;
                case 2: return day.EarlyLeaveMinutes > 0;
                case 3:
                case 5:
                case 6: return day.OvertimeMinutes > 0;
                case 4: return day.StatusID == 2; // Absent
                default: return true;
            }
        }

        private int GetEmployeeDepartmentId(int employeeId, int companyId)
        {
            clsSQL cls = new clsSQL();
            SqlParameter[] prm =
            {
                new SqlParameter("@ID", employeeId),
                new SqlParameter("@CID", companyId)
            };
            DataTable dt = cls.ExecuteQueryStatement(
                "SELECT ISNULL(DepartmentID, 0) AS DepartmentID FROM tbl_employee WHERE ID = @ID AND CompanyID = @CID",
                cls.CreateDataBaseConnectionString(companyId), prm);
            if (dt.Rows.Count == 0) return 0;
            return Simulate.Integer32(dt.Rows[0]["DepartmentID"]);
        }

        private void EnrichRulesWithSalaryElementMeta(List<AttendanceRuleModel> rules, int companyId)
        {
            if (rules == null || rules.Count == 0) return;

            var needIds = rules
                .Where(r => r.ElementTypeID == 0 || string.IsNullOrWhiteSpace(r.ElementCode))
                .Select(r => r.SalaryElementID)
                .Where(id => id > 0)
                .Distinct()
                .ToList();
            if (needIds.Count == 0) return;

            clsSQL cls = new clsSQL();
            string idList = string.Join(",", needIds);
            DataTable dt = cls.ExecuteQueryStatement(
                $"SELECT ID, Code, AName, ElementTypeID FROM tbl_SalariesElements WHERE CompanyID = {companyId} AND ID IN ({idList})",
                cls.CreateDataBaseConnectionString(companyId), null);

            var map = new Dictionary<int, DataRow>();
            foreach (DataRow row in dt.Rows)
                map[Simulate.Integer32(row["ID"])] = row;

            foreach (var rule in rules)
            {
                if (!map.TryGetValue(rule.SalaryElementID, out var row)) continue;
                if (rule.ElementTypeID == 0)
                    rule.ElementTypeID = Simulate.Integer32(row["ElementTypeID"]);
                if (string.IsNullOrWhiteSpace(rule.ElementCode))
                    rule.ElementCode = Simulate.String(row["Code"]);
                if (string.IsNullOrWhiteSpace(rule.AName))
                    rule.AName = Simulate.String(row["AName"]);
            }
        }
    }
}
