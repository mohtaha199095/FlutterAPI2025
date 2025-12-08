using System;
using System.Collections.Generic;
using WebApplication2.DataBaseTable;

namespace WebApplication2.DataBaseTable
{
    public class TblShift
    {
        public int ID { get; set; }

        public string AName { get; set; }
        public string EName { get; set; }

        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }

        public int BreakMinutes { get; set; }

        public int GraceLateMinutes { get; set; }
        public int GraceEarlyMinutes { get; set; }

        public int ShiftType { get; set; }
        // 1 = Normal, 2 = Ramadan, 3 = Flexible

        public bool IsOvernight => EndTime < StartTime;
    }
    public class TblEmployeeShift
    {
        public int ID { get; set; }
        public int EmployeeID { get; set; }
        public int ShiftID { get; set; }

        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }

        public int CompanyID { get; set; }
    }
    public class TblAttendanceRawPunch
    {
        public int ID { get; set; }
        public int EmployeeID { get; set; }
        public DateTime PunchTime { get; set; }
        public int PunchType { get; set; } // 0=IN, 1=OUT
        public int MachineID { get; set; }

        public int CompanyID { get; set; }
    }
    public class TblAttendanceDay
    {
        public int ID { get; set; }
        public int EmployeeID { get; set; }
        public DateTime WorkDate { get; set; }

        public DateTime? FirstIn { get; set; }
        public DateTime? LastOut { get; set; }

        public int WorkedMinutes { get; set; }
        public int LateMinutes { get; set; }
        public int EarlyLeaveMinutes { get; set; }
        public int OvertimeMinutes { get; set; }

        public int StatusID { get; set; } // 1=Present,2=Absent,3=Leave,4=Offday

        public int CompanyID { get; set; }
    }
    public class TblAttendanceToPayroll
    {
        public int ID { get; set; }
        public int AttendanceDayID { get; set; }
        public int SalaryElementID { get; set; }
        public decimal Amount { get; set; }

        public int CompanyID { get; set; }
    }
    //public class AttendanceCalculationResult
    //{
    //    public int EmployeeID { get; set; }
    //    public DateTime WorkDate { get; set; }

    //    public DateTime? FirstIn { get; set; }
    //    public DateTime? LastOut { get; set; }

    //    public int WorkedMinutes { get; set; }
    //    public int LateMinutes { get; set; }
    //    public int EarlyLeaveMinutes { get; set; }
    //    public int OvertimeMinutes { get; set; }

    //    public int StatusID { get; set; }

    //    public List<PayrollImpactItem> PayrollItems { get; set; } = new();
    //    public Dictionary<string, decimal> ToVariableDictionary()
    //    {
    //        return new Dictionary<string, decimal>(StringComparer.OrdinalIgnoreCase)
    //{
    //    { "WorkedMinutes", WorkedMinutes },
    //    { "LateMinutes", LateMinutes },
    //    { "EarlyLeaveMinutes", EarlyLeaveMinutes },
    //    { "OvertimeMinutes", OvertimeMinutes }
    //};
    //    }
    //}

    //public class PayrollImpactItem
    //{
    //    public int SalaryElementID { get; set; }
    //    public decimal Amount { get; set; }
    //}
  
}
public static class AttendanceVariableExtensions
{
    public static Dictionary<string, decimal> ToVariables(this AttendanceCalculationResult r)
    {
        return new Dictionary<string, decimal>(StringComparer.OrdinalIgnoreCase)
        {
            { "WorkedMinutes", r.WorkedMinutes },
            { "LateMinutes", r.LateMinutes },
            { "EarlyLeaveMinutes", r.EarlyLeaveMinutes },
            { "OvertimeMinutes", r.OvertimeMinutes }
        };
    }
}