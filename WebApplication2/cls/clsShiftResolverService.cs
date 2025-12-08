using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using WebApplication2.DataBaseTable;

namespace WebApplication2.cls
{
    public class clsShiftResolverService
    {
        private readonly clsSQL _clsSQL = new clsSQL();

        // ---------------------------------------------------------
        // 1️⃣ Resolve Shift for Specific Day
        // ---------------------------------------------------------
        public int ResolveShiftForDay(int employeeId, DateTime date, int companyId)
        {
            int weekday = (int)date.DayOfWeek;

            // ------------ Query 1: EmployeeShiftAssignment ------------
            SqlParameter[] prm1 =
            {
                new SqlParameter("@Emp", SqlDbType.Int) { Value = employeeId },
                new SqlParameter("@CID", SqlDbType.Int) { Value = companyId },
                new SqlParameter("@Day", SqlDbType.Int) { Value = weekday },
                new SqlParameter("@Dt", SqlDbType.DateTime) { Value = date }
            };

            string q1 = @"
                SELECT TOP 1 ShiftID
                FROM tbl_EmployeeShiftAssignment
                WHERE EmployeeID = @Emp
                  AND CompanyID = @CID
                  AND (@Day = WeekDay OR WeekDay = 0)
                  AND @Dt BETWEEN StartDate AND EndDate
                ORDER BY WeekDay DESC, StartDate DESC";

            object s1 = _clsSQL.ExecuteScalar(q1, prm1, _clsSQL.CreateDataBaseConnectionString(companyId));
            if (s1 != null) return Convert.ToInt32(s1);

            // ------------ Query 2: EmployeeShift Range ------------
            SqlParameter[] prm2 =
            {
                new SqlParameter("@Emp", SqlDbType.Int) { Value = employeeId },
                new SqlParameter("@CID", SqlDbType.Int) { Value = companyId },
                new SqlParameter("@Dt", SqlDbType.DateTime) { Value = date }
            };

            string q2 = @"
                SELECT TOP 1 ShiftID
                FROM tbl_EmployeeShift
                WHERE EmployeeID = @Emp
                  AND CompanyID = @CID
                  AND @Dt BETWEEN StartDate AND EndDate
                ORDER BY StartDate DESC";

            object s2 = _clsSQL.ExecuteScalar(q2, prm2, _clsSQL.CreateDataBaseConnectionString(companyId));
            if (s2 != null) return Convert.ToInt32(s2);

            // ------------ Query 3: Default Shift (ShiftType=1) ------------
            SqlParameter[] prm3 =
            {
                new SqlParameter("@CID", SqlDbType.Int) { Value = companyId }
            };

            string q3 = @"
                SELECT TOP 1 ID 
                FROM tbl_Shifts 
                WHERE CompanyID = @CID AND ShiftType = 1 
                ORDER BY ID";

            object s3 = _clsSQL.ExecuteScalar(q3, prm3, _clsSQL.CreateDataBaseConnectionString(companyId));
            if (s3 != null) return Convert.ToInt32(s3);

            return 0;
        }

        // ---------------------------------------------------------
        // 2️⃣ Load Punches for a Day
        // ---------------------------------------------------------
        public (DateTime? FirstIn, DateTime? LastOut) GetPunches(int employeeId, DateTime date, int companyId)
        {
            SqlParameter[] prm =
            {
                new SqlParameter("@Emp", SqlDbType.Int) { Value = employeeId },
                new SqlParameter("@CID", SqlDbType.Int) { Value = companyId },
                new SqlParameter("@Dt", SqlDbType.Date) { Value = date.Date }
            };

            string q = @"
                SELECT PunchTime, PunchType
                FROM tbl_AttendanceRawPunch
                WHERE EmployeeID = @Emp
                  AND CompanyID = @CID
                  AND CAST(PunchTime AS DATE) = @Dt
                ORDER BY PunchTime";

            DateTime? firstIn = null;
            DateTime? lastOut = null;

            DataTable dt = _clsSQL.ExecuteQueryStatement(q, _clsSQL.CreateDataBaseConnectionString(companyId), prm);
            foreach (DataRow r in dt.Rows)
            {
                DateTime punchTime = Convert.ToDateTime(r["PunchTime"]);
                int punchType = Convert.ToInt32(r["PunchType"]);

                if (punchType == 0 && firstIn == null)
                    firstIn = punchTime;

                if (punchType == 1)
                    lastOut = punchTime;
            }

            return (firstIn, lastOut);
        }

        // ---------------------------------------------------------
        // 3️⃣ Load Shift Details
        // ---------------------------------------------------------
        public TblShift LoadShift(int shiftId, int companyId)
        {
            SqlParameter[] prm =
            {
                new SqlParameter("@ID", SqlDbType.Int) { Value = shiftId }
            };

            string q = @"SELECT * FROM tbl_Shifts WHERE ID = @ID";

            DataTable dt = _clsSQL.ExecuteQueryStatement(q, _clsSQL.CreateDataBaseConnectionString(companyId), prm);

            if (dt.Rows.Count == 0) return null;

            DataRow rd = dt.Rows[0];

            return new TblShift
            {
                ID = shiftId,
                AName = rd["AName"].ToString(),
                EName = rd["EName"].ToString(),
                StartTime = (TimeSpan)rd["StartTime"],
                EndTime = (TimeSpan)rd["EndTime"],
                BreakMinutes = Convert.ToInt32(rd["BreakMinutes"]),
                GraceLateMinutes = Convert.ToInt32(rd["GraceLateMinutes"]),
                GraceEarlyMinutes = Convert.ToInt32(rd["GraceEarlyMinutes"]),
                ShiftType = Convert.ToInt32(rd["ShiftType"])
            };
        }

        // ---------------------------------------------------------
        // 4️⃣ Save Attendance Day
        // ---------------------------------------------------------
        public int SaveAttendanceDay(AttendanceCalculationResult r, int companyId, SqlTransaction trn = null)
        {
            SqlParameter[] prm =
            {
                new SqlParameter("@Emp", r.EmployeeID),
                new SqlParameter("@Dt", r.WorkDate),
                new SqlParameter("@FI", (object?)r.FirstIn ?? DBNull.Value),
                new SqlParameter("@LO", (object?)r.LastOut ?? DBNull.Value),
                new SqlParameter("@WM", r.WorkedMinutes),
                new SqlParameter("@LM", r.LateMinutes),
                new SqlParameter("@EL", r.EarlyLeaveMinutes),
                new SqlParameter("@OT", r.OvertimeMinutes),
                new SqlParameter("@ST", r.StatusID),
                new SqlParameter("@CID", companyId),
                new SqlParameter("@CreationDate", DateTime.Now)
            };

            string q = @"
                INSERT INTO tbl_AttendanceDay (
                    EmployeeID, WorkDate, FirstIn, LastOut,
                    WorkedMinutes, LateMinutes, EarlyLeaveMinutes, OvertimeMinutes,
                    StatusID, CompanyID, CreationDate
                )
                OUTPUT INSERTED.ID
                VALUES (
                    @Emp, @Dt, @FI, @LO,
                    @WM, @LM, @EL, @OT,
                    @ST, @CID, @CreationDate
                )";

            if (trn == null)
                return Convert.ToInt32(_clsSQL.ExecuteScalar(q, prm, _clsSQL.CreateDataBaseConnectionString(companyId)));
            else
                return Convert.ToInt32(_clsSQL.ExecuteScalar(q, prm, _clsSQL.CreateDataBaseConnectionString(companyId), trn));
        }
        public List<PayrollImpactItem> GetPayrollImpact(int employeeId, int periodId, int companyId)
        {
            List<PayrollImpactItem> items = new List<PayrollImpactItem>();

            // Get payroll period dates
            clsPayrollPeriod per = new clsPayrollPeriod();
            DataTable dtPer = per.SelectPayrollPeriod(periodId, "", -1, companyId);

            DateTime start = Simulate.StringToDate(dtPer.Rows[0]["StartDate"]);
            DateTime end = Simulate.StringToDate(dtPer.Rows[0]["EndDate"]);

            string q = @"
        SELECT A.*, R.*
        FROM tbl_AttendanceDay A
        INNER JOIN tbl_AttendanceRule R ON R.CompanyID = A.CompanyID
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

            DataTable dt = _clsSQL.ExecuteQueryStatement(q, _clsSQL.CreateDataBaseConnectionString(companyId), prm);

            foreach (DataRow rd in dt.Rows)
            {
                AttendanceRuleModel rule = AttendanceRuleModel.FromDataRow(rd);
                AttendanceCalculationResult day = AttendanceCalculationResult.FromDataRow(rd);
                clsAttendanceRuleExecutor ss=new clsAttendanceRuleExecutor();
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
        public AttendanceCalculationResult BuildAttendanceDay(int employeeId, DateTime date, int shiftId, int companyId)
        {
            // ---------------------------------------
            // 1) Load punches
            // ---------------------------------------
            var punches = GetPunches(employeeId, date, companyId);
            DateTime? firstIn = punches.FirstIn;
            DateTime? lastOut = punches.LastOut;

            // If no punches → ABSENT
            if (firstIn == null && lastOut == null)
            {
                return new AttendanceCalculationResult
                {
                    EmployeeID = employeeId,
                    WorkDate = date,
                    StatusID = 2, // Absent
                    WorkedMinutes = 0,
                    LateMinutes = 0,
                    EarlyLeaveMinutes = 0,
                    OvertimeMinutes = 0
                };
            }

            // ---------------------------------------
            // 2) Load shift
            // ---------------------------------------
            TblShift shift = LoadShift(shiftId, companyId);
            if (shift == null)
                return null;

            DateTime shiftStart = date.Date + shift.StartTime;
            DateTime shiftEnd = date.Date + shift.EndTime;

            // Overnight shift (e.g., 23:00 → 07:00 next day)
            if (shift.EndTime < shift.StartTime)
                shiftEnd = shiftEnd.AddDays(1);

            // ---------------------------------------
            // 3) Calculate presence
            // ---------------------------------------
            int workedMinutes = 0;
            if (firstIn != null && lastOut != null)
                workedMinutes = (int)(lastOut.Value - firstIn.Value).TotalMinutes;

            // ---------------------------------------
            // 4) Calculate LATE mins
            // ---------------------------------------
            int lateMinutes = 0;
            if (firstIn > shiftStart.AddMinutes(shift.GraceLateMinutes))
                lateMinutes = (int)(firstIn.Value - shiftStart).TotalMinutes;

            // ---------------------------------------
            // 5) EARLY LEAVE mins
            // ---------------------------------------
            int earlyLeaveMinutes = 0;
            if (lastOut < shiftEnd.AddMinutes(-shift.GraceEarlyMinutes))
                earlyLeaveMinutes = (int)(shiftEnd - lastOut.Value).TotalMinutes;

            // ---------------------------------------
            // 6) BREAK deduction
            // ---------------------------------------
            if (workedMinutes > shift.BreakMinutes)
                workedMinutes -= shift.BreakMinutes;

            // ---------------------------------------
            // 7) Calculate OVERTIME
            // ---------------------------------------
            int overtimeMinutes = 0;
            if (lastOut > shiftEnd)
                overtimeMinutes = (int)(lastOut.Value - shiftEnd).TotalMinutes;

            // ---------------------------------------
            // 8) Build result object
            // ---------------------------------------
            return new AttendanceCalculationResult
            {
                EmployeeID = employeeId,
                WorkDate = date,
                FirstIn = firstIn,
                LastOut = lastOut,
                WorkedMinutes = workedMinutes,
                LateMinutes = lateMinutes,
                EarlyLeaveMinutes = earlyLeaveMinutes,
                OvertimeMinutes = overtimeMinutes,
                StatusID = 1 // Present
            };
        }
        // ---------------------------------------------------------
        // 5️⃣ Save Attendance → Payroll Elements
        // ---------------------------------------------------------
        public void SaveAttendanceToPayroll(int attendanceDayID, List<PayrollImpactItem> items, int companyId, SqlTransaction trn = null)
        {
            foreach (var item in items)
            {
                SqlParameter[] prm =
                {
                    new SqlParameter("@ADID", attendanceDayID),
                    new SqlParameter("@SEID", item.SalaryElementID),
                    new SqlParameter("@AMT", item.Amount),
                    new SqlParameter("@CID", companyId),
                    new SqlParameter("@CreationDate", DateTime.Now)
                };

                string q = @"
                    INSERT INTO tbl_AttendanceToPayroll (
                        AttendanceDayID, SalaryElementID, Amount, CompanyID, CreationDate
                    )
                    VALUES (@ADID, @SEID, @AMT, @CID, @CreationDate)";

                if (trn == null)
                    _clsSQL.ExecuteNonQueryStatement(q, _clsSQL.CreateDataBaseConnectionString(companyId), prm);
                else
                    _clsSQL.ExecuteNonQueryStatement(q, _clsSQL.CreateDataBaseConnectionString(companyId), prm, trn);
            }
        }
        public void ApplyAttendanceRulesAndSave(
    AttendanceCalculationResult day,
    int employeeId,
    int departmentId,
    int shiftId,
    int companyId)
        {
            clsAttendanceRuleService loader = new clsAttendanceRuleService();
            clsAttendanceRuleExecutor executor = new clsAttendanceRuleExecutor();

            List<AttendanceRuleModel> rules =
                loader.LoadRulesForEmployee(employeeId, departmentId, shiftId, companyId);

            TblShift shift = LoadShift(shiftId, companyId);

            List<PayrollImpactItem> items =
                executor.ExecuteRules(rules, day, shift);

            int attendanceDayID = SaveAttendanceDay(day, companyId);

            SaveAttendanceToPayroll(attendanceDayID, items, companyId);
        }
    }
}
