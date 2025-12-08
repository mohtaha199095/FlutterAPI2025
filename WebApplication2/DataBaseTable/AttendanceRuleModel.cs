using System;
using System.Collections.Generic;
using System.Data;

namespace WebApplication2.DataBaseTable
{
    public class AttendanceRuleModel
    {
        public int ID { get; set; }
        public string RuleName { get; set; }
        public string AName { get; set; }   // <-- for payroll detail name

        public int RuleTypeID { get; set; }
        public int CalculationTypeID { get; set; }
        public int SalaryElementID { get; set; }

        public int ElementTypeID { get; set; }   // <-- earning/deduction
        public string ElementCode { get; set; }  // <-- salary element code

        public decimal Value { get; set; }
        public string FormulaText { get; set; }
        public decimal MinAmount { get; set; }
        public decimal MaxAmount { get; set; }
        public List<AttendanceRuleCondition> Conditions { get; set; } = new List<AttendanceRuleCondition >();

        public static AttendanceRuleModel FromDataRow(DataRow row)
        {
            return new AttendanceRuleModel
            {
                ID = Simulate.Integer32(row["ID"]),
                RuleName = Simulate.String(row["RuleName"]),
                AName = Simulate.String(row["AName"]), // if needed use RuleName
                RuleTypeID = Simulate.Integer32(row["RuleTypeID"]),
                CalculationTypeID = Simulate.Integer32(row["CalculationTypeID"]),
                SalaryElementID = Simulate.Integer32(row["SalaryElementID"]),
                ElementTypeID = Simulate.Integer32(row["ElementTypeID"]),
                ElementCode = Simulate.String(row["ElementCode"]),
                Value = Simulate.decimal_(row["Value"]),
                FormulaText = Simulate.String(row["FormulaText"]),
                MinAmount = Simulate.decimal_(row["MinAmount"]),
                MaxAmount = Simulate.decimal_(row["MaxAmount"])
            };
        }
    }

}
