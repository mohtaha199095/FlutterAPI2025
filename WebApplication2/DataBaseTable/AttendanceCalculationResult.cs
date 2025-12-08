using System;
using System.Collections.Generic;
using System.Data;

namespace WebApplication2.DataBaseTable
 {
    public class AttendanceCalculationResult
    {
        public int EmployeeID { get; set; }
        public DateTime WorkDate { get; set; }

        public DateTime? FirstIn { get; set; }
        public DateTime? LastOut { get; set; }

        public int WorkedMinutes { get; set; }
        public int LateMinutes { get; set; }
        public int EarlyLeaveMinutes { get; set; }
        public int OvertimeMinutes { get; set; }

        public int StatusID { get; set; }

        public static AttendanceCalculationResult FromDataRow(DataRow row)
        {
            return new AttendanceCalculationResult
            {
                EmployeeID = Simulate.Integer32(row["EmployeeID"]),
                WorkDate = Simulate.StringToDate(row["WorkDate"]),
                FirstIn = row["FirstIn"] == DBNull.Value ? null : Simulate.StringToDate(row["FirstIn"]),
                LastOut = row["LastOut"] == DBNull.Value ? null : Simulate.StringToDate(row["LastOut"]),
                WorkedMinutes = Simulate.Integer32(row["WorkedMinutes"]),
                LateMinutes = Simulate.Integer32(row["LateMinutes"]),
                EarlyLeaveMinutes = Simulate.Integer32(row["EarlyLeaveMinutes"]),
                OvertimeMinutes = Simulate.Integer32(row["OvertimeMinutes"]),
                StatusID = Simulate.Integer32(row["StatusID"])
            };
        }
    }

}
