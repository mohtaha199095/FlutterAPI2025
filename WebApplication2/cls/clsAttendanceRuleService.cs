using System;
using System.Collections.Generic;
using System.Data;
using Microsoft.Data.SqlClient;
using WebApplication2.DataBaseTable;

namespace WebApplication2.cls
{
    public class clsAttendanceRuleService
    {
        private readonly clsSQL _sql = new clsSQL();

        // ---------------------------------------------------------
        // Load rule groups, rules, conditions, mapping
        // ---------------------------------------------------------
        public List<AttendanceRuleModel> LoadRulesForEmployee(
            int employeeId,
            int departmentId,
            int shiftId,
            int companyId)
        {
            List<AttendanceRuleModel> rules = new List<AttendanceRuleModel>();

            SqlParameter[] prm =
            {
                new SqlParameter("@Emp", employeeId),
                new SqlParameter("@Dept", departmentId),
                new SqlParameter("@Shift", shiftId),
                new SqlParameter("@CID", companyId)
            };

            string q = @"
                SELECT R.*, 
                       M.Priority,
                       RG.AName AS GroupAName,
                       RG.EName AS GroupEName
                FROM tbl_AttendanceRules R
                INNER JOIN tbl_AttendanceRuleMapping M 
                    ON M.RuleID = R.ID AND M.IsActive = 1
                LEFT JOIN tbl_AttendanceRuleGroups RG 
                    ON RG.ID = R.RuleGroupID
                WHERE R.CompanyID = @CID
                  AND R.IsActive = 1
                  AND (
                        M.EmployeeID = @Emp 
                     OR M.DepartmentID = @Dept 
                     OR M.ShiftID = @Shift
                     OR (M.EmployeeID IS NULL AND M.DepartmentID IS NULL AND M.ShiftID IS NULL)
                  )
                ORDER BY M.Priority DESC, R.ID";

            DataTable dt = _sql.ExecuteQueryStatement(q, _sql.CreateDataBaseConnectionString(companyId), prm);

            foreach (DataRow row in dt.Rows)
            {
                AttendanceRuleModel r = new AttendanceRuleModel
                {
                    ID = Simulate.Integer32(row["ID"]),
                    RuleName = Simulate.String(row["RuleName"]),
                    RuleTypeID = Simulate.Integer32(row["RuleTypeID"]),
                    CalculationTypeID = Simulate.Integer32(row["CalculationTypeID"]),
                    SalaryElementID = Simulate.Integer32(row["SalaryElementID"]),
                    Value = Simulate.decimal_(row["Value"]),
                    FormulaText = Simulate.String(row["FormulaText"]),
                    MinAmount = Simulate.decimal_(row["MinAmount"]),
                    MaxAmount = Simulate.decimal_(row["MaxAmount"])
                };

                r.Conditions = LoadRuleConditions(r.ID, companyId);
                rules.Add(r);
            }

            return rules;
        }

        // ---------------------------------------------------------
        // Load rule conditions
        // ---------------------------------------------------------
        private List<AttendanceRuleCondition> LoadRuleConditions(int ruleID, int companyId)
        {
            List<AttendanceRuleCondition> list = new List<AttendanceRuleCondition>();

            SqlParameter[] prm =
            {
                new SqlParameter("@ID", ruleID),
                new SqlParameter("@CID", companyId)
            };

            string q = @"
                SELECT *
                FROM tbl_AttendanceRuleConditions
                WHERE RuleID = @ID AND CompanyID = @CID";

            DataTable dt = _sql.ExecuteQueryStatement(q, _sql.CreateDataBaseConnectionString(companyId), prm);

            foreach (DataRow row in dt.Rows)
            {
                list.Add(new AttendanceRuleCondition
                {
                    RuleID = ruleID,
                    LeftOperand = Simulate.String(row["LeftOperand"]),
                    RightOperand = Simulate.String(row["RightOperand"]),
                    Operator = Simulate.String(row["Operator"]),
                    ValueType = Simulate.Integer32(row["ValueType"])
                });
            }

            return list;
        }
        public List<PayrollImpactItem> GetPayrollImpact(int employeeId, int periodId, int companyId)
        {
            List<PayrollImpactItem> items = new List<PayrollImpactItem>();

            DateTime start, end;
            clsPayrollPeriod pr = new clsPayrollPeriod();
            pr.GetPeriodDates(periodId, out start, out end, companyId);

            string q = @"
        SELECT A.*, R.*
        FROM tbl_AttendanceDay A
        INNER JOIN tbl_AttendanceRule R
            ON A.CompanyID = R.CompanyID
        WHERE A.EmployeeID = @Emp
          AND A.CompanyID = @CID
          AND A.WorkDate BETWEEN @S AND @E
          AND R.IsActive = 1";

            SqlParameter[] prm =
            {
        new SqlParameter("@Emp", employeeId),
        new SqlParameter("@CID", companyId),
        new SqlParameter("@S", start),
        new SqlParameter("@E", end)
    };
            clsSQL _clsSQL = new clsSQL();
            DataTable dt = _clsSQL.ExecuteQueryStatement(q, _clsSQL.CreateDataBaseConnectionString(companyId), prm);

            foreach (DataRow rd in dt.Rows)
            {
                AttendanceRuleModel rule = AttendanceRuleModel.FromDataRow(rd);
                AttendanceCalculationResult day = AttendanceCalculationResult.FromDataRow(rd);
                clsAttendanceRuleExecutor ss = new clsAttendanceRuleExecutor();
                decimal amount = ss.CalculateAmount(rule, day);
                if (amount == 0) continue;

                items.Add(new PayrollImpactItem
                {
                    SalaryElementID = rule.SalaryElementID,
                    ElementName = rule.AName,
                    Amount = amount,
                    ElementTypeID = rule.ElementTypeID,
                    Code = rule.ElementCode
                });
            }

            return items;
        }
    }
}
