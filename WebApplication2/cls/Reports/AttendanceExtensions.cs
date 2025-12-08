using System;
using System.Collections.Generic;
using WebApplication2.DataBaseTable;

namespace WebApplication2.cls.Reports
{
    public static  class AttendanceExtensions
    {
        public static Dictionary<string, decimal> ToVariableDictionary(this AttendanceCalculationResult r)
        {
            return new Dictionary<string, decimal>(StringComparer.OrdinalIgnoreCase)
        {
            { "WorkedMinutes", r.WorkedMinutes },
            { "LateMinutes", r.LateMinutes },
            { "EarlyLeaveMinutes", r.EarlyLeaveMinutes },
            { "OvertimeMinutes", r.OvertimeMinutes },
            { "StatusID", r.StatusID }
        };
        }
    }
}
