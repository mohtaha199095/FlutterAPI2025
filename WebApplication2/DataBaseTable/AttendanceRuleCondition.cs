namespace WebApplication2.DataBaseTable
{
    public class AttendanceRuleCondition
    {
        public int RuleID { get; set; }

        public string LeftOperand { get; set; }
        // Example: "LateMinutes", "WorkedMinutes", "OvertimeMinutes", "ShiftGraceLate"

        public string Operator { get; set; }
        // >, >=, <, <=, ==, !=

        public string RightOperand { get; set; }
        // Number or variable name

        public int ValueType { get; set; }
        // 1 = Numeric  
        // 2 = Variable (LateMinutes, EarlyLeaveMinutes…)
    }
}
