using System;
using System.Collections.Generic;
using System.Linq;
using WebApplication2.DataBaseTable;

namespace WebApplication2.cls
{
    /// <summary>
    /// Deterministic golden fixtures for payroll classification and attendance amount math.
    /// Call <see cref="RunAll"/> from a diagnostic endpoint or unit harness.
    /// </summary>
    public static class clsPayrollGoldenFixtures
    {
        public class FixtureResult
        {
            public string Name { get; set; }
            public bool Passed { get; set; }
            public string Detail { get; set; }
        }

        public static List<FixtureResult> RunAll()
        {
            var results = new List<FixtureResult>
            {
                Fixture_BasicPlusAllowanceMinusDeduction(),
                Fixture_EmployerContributionExcludedFromNet(),
                Fixture_LateRatePerHour(),
                Fixture_OvertimeFixed(),
                Fixture_AbsenceDoesNotApplyWhenPresent(),
                Fixture_StatutoryCodesNotDoubleCountedAsBasic()
            };
            return results;
        }

        public static bool AllPassed() => RunAll().All(r => r.Passed);

        static FixtureResult Fixture_BasicPlusAllowanceMinusDeduction()
        {
            var variables = new Dictionary<string, decimal>(StringComparer.OrdinalIgnoreCase)
            {
                ["BASIC"] = 500m
            };
            var details = new List<PayrollDetailModel>
            {
                new PayrollDetailModel { BasicSalaryCode = "BASIC", ElementTypeID = 1, Amount = 500m, ElementName = "Basic" },
                new PayrollDetailModel { BasicSalaryCode = "HOUSING", ElementTypeID = 1, Amount = 100m, ElementName = "Housing" },
                new PayrollDetailModel { BasicSalaryCode = "PENALTY", ElementTypeID = 2, Amount = 25m, ElementName = "Penalty" }
            };
            var preview = clsPayrollEngine.BuildSummary(details, new List<PayrollImpactItem>(), variables, false);
            bool ok = preview.BasicSalary == 500m
                      && preview.TotalEarnings == 100m
                      && preview.TotalDeductions == 25m
                      && preview.NetSalary == 575m;
            return new FixtureResult
            {
                Name = "Basic+Allowance-Deduction",
                Passed = ok,
                Detail = $"Net={preview.NetSalary} Earn={preview.TotalEarnings} Ded={preview.TotalDeductions}"
            };
        }

        static FixtureResult Fixture_EmployerContributionExcludedFromNet()
        {
            var variables = new Dictionary<string, decimal>(StringComparer.OrdinalIgnoreCase) { ["BASIC"] = 400m };
            var details = new List<PayrollDetailModel>
            {
                new PayrollDetailModel { BasicSalaryCode = "BASIC", ElementTypeID = 1, Amount = 400m },
                new PayrollDetailModel { BasicSalaryCode = "SS_ER", ElementTypeID = 3, Amount = 57m, ElementName = "Employer SS" }
            };
            var preview = clsPayrollEngine.BuildSummary(details, new List<PayrollImpactItem>(), variables, false);
            bool ok = preview.NetSalary == 400m && preview.EmployerContributions == 57m && preview.TotalDeductions == 0m;
            return new FixtureResult
            {
                Name = "EmployerContributionExcludedFromNet",
                Passed = ok,
                Detail = $"Net={preview.NetSalary} ER={preview.EmployerContributions}"
            };
        }

        static FixtureResult Fixture_LateRatePerHour()
        {
            var exec = new clsAttendanceRuleExecutor();
            var rule = new AttendanceRuleModel
            {
                RuleTypeID = 1,
                CalculationTypeID = 3,
                Value = 2m, // 2 per hour late
                SalaryElementID = 10,
                ElementTypeID = 2,
                ElementCode = "LATE",
                RuleName = "Late"
            };
            var day = new AttendanceCalculationResult { LateMinutes = 90, StatusID = 1 };
            decimal amt = exec.CalculateAmount(rule, day);
            bool ok = amt == 3m; // 1.5h * 2
            return new FixtureResult
            {
                Name = "LateRatePerHour",
                Passed = ok,
                Detail = $"Amount={amt}"
            };
        }

        static FixtureResult Fixture_OvertimeFixed()
        {
            var exec = new clsAttendanceRuleExecutor();
            var rule = new AttendanceRuleModel
            {
                RuleTypeID = 3,
                CalculationTypeID = 1,
                Value = 15m,
                SalaryElementID = 11,
                ElementTypeID = 1,
                ElementCode = "OT",
                RuleName = "OT Fixed"
            };
            var day = new AttendanceCalculationResult { OvertimeMinutes = 120, StatusID = 1 };
            var shift = new TblShift { GraceLateMinutes = 0, GraceEarlyMinutes = 0, BreakMinutes = 0 };
            var items = exec.ExecuteRules(new List<AttendanceRuleModel> { rule }, day, shift);
            bool ok = items.Count == 1 && items[0].Amount == 15m && items[0].ElementTypeID == 1;
            return new FixtureResult
            {
                Name = "OvertimeFixedAppliesWhenOT",
                Passed = ok,
                Detail = $"Count={items.Count} Amt={(items.FirstOrDefault()?.Amount ?? 0)}"
            };
        }

        static FixtureResult Fixture_AbsenceDoesNotApplyWhenPresent()
        {
            var exec = new clsAttendanceRuleExecutor();
            var rule = new AttendanceRuleModel
            {
                RuleTypeID = 4,
                CalculationTypeID = 1,
                Value = 50m,
                SalaryElementID = 12,
                ElementTypeID = 2,
                ElementCode = "ABS",
                RuleName = "Absence"
            };
            var day = new AttendanceCalculationResult { StatusID = 1, WorkedMinutes = 480 };
            var shift = new TblShift();
            var items = exec.ExecuteRules(new List<AttendanceRuleModel> { rule }, day, shift);
            bool ok = items.Count == 0;
            return new FixtureResult
            {
                Name = "AbsenceSkippedWhenPresent",
                Passed = ok,
                Detail = $"Count={items.Count}"
            };
        }

        /// <summary>
        /// Golden-path classification: SS/TAX system lines are deductions, not earnings.
        /// </summary>
        static FixtureResult Fixture_StatutoryCodesNotDoubleCountedAsBasic()
        {
            var variables = new Dictionary<string, decimal>(StringComparer.OrdinalIgnoreCase)
            {
                ["BASIC"] = 1000m
            };
            var details = new List<PayrollDetailModel>
            {
                new PayrollDetailModel { BasicSalaryCode = "BASIC", ElementTypeID = 1, Amount = 1000m },
                new PayrollDetailModel { BasicSalaryCode = "SS_EE", ElementTypeID = 2, Amount = 75m, IsSystemGenerated = true },
                new PayrollDetailModel { BasicSalaryCode = "TAX_EE", ElementTypeID = 2, Amount = 40m, IsSystemGenerated = true },
                new PayrollDetailModel { BasicSalaryCode = "SS_ER", ElementTypeID = 3, Amount = 142.5m, IsSystemGenerated = true },
            };
            var preview = clsPayrollEngine.BuildSummary(details, new List<PayrollImpactItem>(), variables, false);
            bool ok = preview.BasicSalary == 1000m
                      && preview.TotalDeductions == 115m
                      && preview.EmployerContributions == 142.5m
                      && preview.NetSalary == 885m;
            return new FixtureResult
            {
                Name = "StatutoryDeductionClassification",
                Passed = ok,
                Detail = $"Net={preview.NetSalary} Ded={preview.TotalDeductions} ER={preview.EmployerContributions}"
            };
        }
    }
}
