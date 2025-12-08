using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using WebApplication2.cls.Reports;
using WebApplication2.DataBaseTable;

namespace WebApplication2.cls
{
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

                foreach (var cond in rule.Conditions)
                {
                    if (!evaluator.EvaluateCondition(cond, day, shift))
                    {
                        conditionsMet = false;
                        break;
                    }
                }

                if (!conditionsMet) continue;
       
                decimal amount =  CalculateAmount(rule, day);

                // Apply min/max if needed
                if (rule.MinAmount > 0 && amount < rule.MinAmount)
                    amount = rule.MinAmount;

                if (rule.MaxAmount > 0 && amount > rule.MaxAmount)
                    amount = rule.MaxAmount;

                // Add to payroll list
                impact.Add(new PayrollImpactItem
                {
                     SalaryElementID = rule.SalaryElementID,
                    Amount = amount
                });
            }

            return impact;
        }
        public List<PayrollImpactItem> ExecuteRulesForEmployee(int employeeId, int payrollPeriodId, int companyId)
        {
            List<PayrollImpactItem> results = new List<PayrollImpactItem>();
            clsSQL cls = new clsSQL();

            DateTime startDate, endDate;

            // Load payroll period dates
            clsPayrollPeriod pr = new clsPayrollPeriod();
            pr.GetPeriodDates(payrollPeriodId, out startDate, out endDate, companyId);

            // Load all rule rows from database
            DataTable dt = LoadAllRules(companyId);

            // Resolve attendance day-by-day
            clsShiftResolverService shift = new clsShiftResolverService();

            List<DateTime> allDays = new List<DateTime>();
            for (DateTime d = startDate; d <= endDate; d = d.AddDays(1))
                allDays.Add(d);

            foreach (DateTime day in allDays)
            {
                int shiftId = shift.ResolveShiftForDay(employeeId, day, companyId);

                AttendanceCalculationResult calc = shift.BuildAttendanceDay(employeeId, day, shiftId, companyId);
                if (calc == null) continue;

                // Loop rules
                foreach (DataRow r in dt.Rows)
                {
                    AttendanceRuleModel rule = AttendanceRuleModel.FromDataRow(r);

                    decimal amt = CalculateAmount(rule, calc);
                    if (amt == 0) continue;

                    results.Add(new PayrollImpactItem
                    {
                        SalaryElementID = rule.SalaryElementID,
                        ElementName = rule.RuleName,
                        Amount = amt,
                        ElementTypeID = rule.ElementTypeID,
                        Code = rule.ElementCode
                    });
                }
            }

            return results;
        }
        private DataTable LoadAllRules(int companyId)
        {
            clsSQL cls = new clsSQL();

            string q = @"SELECT * FROM tbl_AttendanceRuleheader WHERE CompanyID = @CID AND IsActive = 1";
            SqlParameter[] prm = { new SqlParameter("@CID", companyId) };

            return cls.ExecuteQueryStatement(q, cls.CreateDataBaseConnectionString(companyId), prm);
        }
        public decimal CalculateAmount(AttendanceRuleModel r, AttendanceCalculationResult day)
        {
            switch (r.CalculationTypeID)
            {
                case 1: // Fixed value
                    return r.Value;

                case 2: // Per Worked Minute
                    return day.WorkedMinutes * (r.Value / 100m);

                case 3: // Per Overtime Hour
                    return (day.OvertimeMinutes / 60m) * r.Value;

                case 4: // Formula
                    return FormulaEvaluator.SafeEvaluate(
                        r.FormulaText,
                        day.ToVariableDictionary()
                    );
            }

            return 0;
        }

    }
}
