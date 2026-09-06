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
            string ruleName = Col(row, "RuleName");
            string aName = Col(row, "AName");
            if (string.IsNullOrWhiteSpace(aName))
                aName = Col(row, "ElementAName");
            if (string.IsNullOrWhiteSpace(aName))
                aName = ruleName;

            return new AttendanceRuleModel
            {
                ID = IntCol(row, "ID"),
                RuleName = ruleName,
                AName = aName,
                RuleTypeID = IntCol(row, "RuleTypeID"),
                CalculationTypeID = IntCol(row, "CalculationTypeID"),
                SalaryElementID = IntCol(row, "SalaryElementID"),
                ElementTypeID = IntCol(row, "ElementTypeID"),
                ElementCode = Col(row, "ElementCode"),
                Value = DecCol(row, "Value"),
                FormulaText = Col(row, "FormulaText"),
                MinAmount = DecCol(row, "MinAmount"),
                MaxAmount = DecCol(row, "MaxAmount")
            };
        }

        static bool Has(DataRow row, string col) =>
            row.Table.Columns.Contains(col) && row[col] != System.DBNull.Value;

        static string Col(DataRow row, string col) =>
            Has(row, col) ? Simulate.String(row[col]) : "";

        static int IntCol(DataRow row, string col) =>
            Has(row, col) ? Simulate.Integer32(row[col]) : 0;

        static decimal DecCol(DataRow row, string col) =>
            Has(row, col) ? Simulate.decimal_(row[col]) : 0m;
    }

}
