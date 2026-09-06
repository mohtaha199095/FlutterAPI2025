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
                       SE.ElementTypeID AS SE_ElementTypeID,
                       SE.Code AS ElementCode,
                       SE.AName AS ElementAName
                FROM tbl_AttendanceRules R
                LEFT JOIN tbl_SalariesElements SE
                    ON SE.ID = R.SalaryElementID AND SE.CompanyID = R.CompanyID
                WHERE R.CompanyID = @CID
                  AND R.IsActive = 1
                  AND (
                        NOT EXISTS (
                            SELECT 1 FROM tbl_AttendanceRuleMapping M0
                            WHERE M0.RuleID = R.ID AND M0.CompanyID = R.CompanyID AND M0.IsActive = 1
                        )
                     OR EXISTS (
                            SELECT 1 FROM tbl_AttendanceRuleMapping M
                            WHERE M.RuleID = R.ID AND M.CompanyID = R.CompanyID AND M.IsActive = 1
                              AND (
                                    M.EmployeeID = @Emp
                                 OR M.DepartmentID = @Dept
                                 OR M.ShiftID = @Shift
                                 OR (
                                        (M.EmployeeID IS NULL OR M.EmployeeID = 0)
                                    AND (M.DepartmentID IS NULL OR M.DepartmentID = 0)
                                    AND (M.ShiftID IS NULL OR M.ShiftID = 0)
                                 )
                              )
                        )
                  )
                ORDER BY R.ID";

            DataTable dt = _sql.ExecuteQueryStatement(q, _sql.CreateDataBaseConnectionString(companyId), prm);

            foreach (DataRow row in dt.Rows)
            {
                AttendanceRuleModel r = new AttendanceRuleModel
                {
                    ID = Simulate.Integer32(row["ID"]),
                    RuleName = Simulate.String(row["RuleName"]),
                    AName = Simulate.String(row["ElementAName"]),
                    RuleTypeID = Simulate.Integer32(row["RuleTypeID"]),
                    CalculationTypeID = Simulate.Integer32(row["CalculationTypeID"]),
                    SalaryElementID = Simulate.Integer32(row["SalaryElementID"]),
                    ElementTypeID = Simulate.Integer32(row["SE_ElementTypeID"]),
                    ElementCode = Simulate.String(row["ElementCode"]),
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
