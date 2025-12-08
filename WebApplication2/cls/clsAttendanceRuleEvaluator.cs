using System;
using WebApplication2.DataBaseTable;

namespace WebApplication2.cls
{
    public class clsAttendanceRuleEvaluator
    {
        public bool EvaluateCondition(
            AttendanceRuleCondition c,
            AttendanceCalculationResult day,
            TblShift shift)
        {
            decimal left = ResolveOperandValue(c.LeftOperand, day, shift);
            decimal right = ResolveOperandValue(c.RightOperand, day, shift);

            switch (c.Operator)
            {
                case ">": return left > right;
                case ">=": return left >= right;
                case "<": return left < right;
                case "<=": return left <= right;
                case "==": return Math.Abs(left - right) < 0.0001m;
                case "!=": return left != right;
            }

            return false;
        }

        private decimal ResolveOperandValue(string op, AttendanceCalculationResult day, TblShift shift)
        {
            switch (op)
            {
                case "LateMinutes": return day.LateMinutes;
                case "EarlyLeaveMinutes": return day.EarlyLeaveMinutes;
                case "WorkedMinutes": return day.WorkedMinutes;
                case "OvertimeMinutes": return day.OvertimeMinutes;
                case "ShiftGraceLate": return shift.GraceLateMinutes;
                case "ShiftGraceEarly": return shift.GraceEarlyMinutes;
                case "ShiftBreak": return shift.BreakMinutes;

                default:
                    return Simulate.decimal_(op);
            }
        }
    }
}
