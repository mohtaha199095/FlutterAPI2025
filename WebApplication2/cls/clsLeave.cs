using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using WebApplication2.MainClasses;
using static WebApplication2.MainClasses.clsEnum;

namespace WebApplication2.cls
{
    /// <summary>
    /// Leave types, holiday calendars, balances, and leave requests.
    /// </summary>
    public class clsLeave
    {
        // ==========================================================
        // LEAVE TYPES
        // ==========================================================
        public DataTable SelectLeaveTypes(int ID, string Code, int CompanyID, int ActiveOnly = 0)
        {
            SqlParameter[] prm =
            {
                new SqlParameter("@ID", SqlDbType.Int) { Value = ID },
                new SqlParameter("@Code", SqlDbType.NVarChar, 50) { Value = Code ?? "" },
                new SqlParameter("@CompanyID", SqlDbType.Int) { Value = CompanyID },
                new SqlParameter("@ActiveOnly", SqlDbType.Int) { Value = ActiveOnly },
            };
            clsSQL sql = new clsSQL();
            return sql.ExecuteQueryStatement(@"
SELECT * FROM tbl_LeaveType
WHERE (ID = @ID OR @ID = 0)
  AND (Code = @Code OR @Code = '')
  AND (CompanyID = @CompanyID OR @CompanyID = 0)
  AND (@ActiveOnly = 0 OR ISNULL(IsActive,1) = 1)
ORDER BY Code",
                sql.CreateDataBaseConnectionString(CompanyID), prm);
        }

        public int InsertLeaveType(string Code, string AName, string EName, bool IsPaid, bool IsActive,
            int AccrualRuleID, int CompanyID, int CreationUserID, SqlTransaction trn = null)
        {
            SqlParameter[] prm =
            {
                new SqlParameter("@Code", SqlDbType.NVarChar, 50) { Value = Code ?? "" },
                new SqlParameter("@AName", SqlDbType.NVarChar, -1) { Value = AName ?? "" },
                new SqlParameter("@EName", SqlDbType.NVarChar, -1) { Value = EName ?? "" },
                new SqlParameter("@IsPaid", SqlDbType.Bit) { Value = IsPaid },
                new SqlParameter("@IsActive", SqlDbType.Bit) { Value = IsActive },
                new SqlParameter("@AccrualRuleID", SqlDbType.Int) { Value = AccrualRuleID },
                new SqlParameter("@CompanyID", SqlDbType.Int) { Value = CompanyID },
                new SqlParameter("@CreationUserID", SqlDbType.Int) { Value = CreationUserID },
                new SqlParameter("@CreationDate", SqlDbType.DateTime) { Value = DateTime.Now },
            };
            string q = @"
INSERT INTO tbl_LeaveType (Code, AName, EName, IsPaid, IsActive, AccrualRuleID, CompanyID, CreationUserID, CreationDate)
OUTPUT INSERTED.ID
VALUES (@Code, @AName, @EName, @IsPaid, @IsActive, @AccrualRuleID, @CompanyID, @CreationUserID, @CreationDate)";
            clsSQL sql = new clsSQL();
            return Simulate.Integer32(sql.ExecuteScalar(q, prm, sql.CreateDataBaseConnectionString(CompanyID), trn));
        }

        public int UpdateLeaveType(int ID, string Code, string AName, string EName, bool IsPaid, bool IsActive,
            int AccrualRuleID, int CompanyID, int ModificationUserID)
        {
            SqlParameter[] prm =
            {
                new SqlParameter("@ID", SqlDbType.Int) { Value = ID },
                new SqlParameter("@Code", SqlDbType.NVarChar, 50) { Value = Code ?? "" },
                new SqlParameter("@AName", SqlDbType.NVarChar, -1) { Value = AName ?? "" },
                new SqlParameter("@EName", SqlDbType.NVarChar, -1) { Value = EName ?? "" },
                new SqlParameter("@IsPaid", SqlDbType.Bit) { Value = IsPaid },
                new SqlParameter("@IsActive", SqlDbType.Bit) { Value = IsActive },
                new SqlParameter("@AccrualRuleID", SqlDbType.Int) { Value = AccrualRuleID },
                new SqlParameter("@CompanyID", SqlDbType.Int) { Value = CompanyID },
                new SqlParameter("@ModificationUserID", SqlDbType.Int) { Value = ModificationUserID },
                new SqlParameter("@ModificationDate", SqlDbType.DateTime) { Value = DateTime.Now },
            };
            clsSQL sql = new clsSQL();
            return sql.ExecuteNonQueryStatement(@"
UPDATE tbl_LeaveType SET
  Code=@Code, AName=@AName, EName=@EName, IsPaid=@IsPaid, IsActive=@IsActive,
  AccrualRuleID=@AccrualRuleID, ModificationUserID=@ModificationUserID, ModificationDate=@ModificationDate
WHERE ID=@ID AND CompanyID=@CompanyID",
                sql.CreateDataBaseConnectionString(CompanyID), prm);
        }

        public bool DeleteLeaveType(int ID, int CompanyID)
        {
            SqlParameter[] prm =
            {
                new SqlParameter("@ID", SqlDbType.Int) { Value = ID },
                new SqlParameter("@CompanyID", SqlDbType.Int) { Value = CompanyID },
            };
            clsSQL sql = new clsSQL();
            sql.ExecuteNonQueryStatement(
                "DELETE FROM tbl_LeaveType WHERE ID=@ID AND CompanyID=@CompanyID",
                sql.CreateDataBaseConnectionString(CompanyID), prm);
            return true;
        }

        // ==========================================================
        // HOLIDAY CALENDAR + HOLIDAYS
        // ==========================================================
        public DataTable SelectHolidayCalendars(int ID, int Year, int CompanyID)
        {
            SqlParameter[] prm =
            {
                new SqlParameter("@ID", SqlDbType.Int) { Value = ID },
                new SqlParameter("@Year", SqlDbType.Int) { Value = Year },
                new SqlParameter("@CompanyID", SqlDbType.Int) { Value = CompanyID },
            };
            clsSQL sql = new clsSQL();
            return sql.ExecuteQueryStatement(@"
SELECT * FROM tbl_HolidayCalendar
WHERE (ID=@ID OR @ID=0)
  AND (Year=@Year OR @Year=0)
  AND (CompanyID=@CompanyID OR @CompanyID=0)
ORDER BY Year DESC, AName",
                sql.CreateDataBaseConnectionString(CompanyID), prm);
        }

        public int InsertHolidayCalendar(string AName, string EName, int Year, int CompanyID, int CreationUserID)
        {
            SqlParameter[] prm =
            {
                new SqlParameter("@AName", SqlDbType.NVarChar, -1) { Value = AName ?? "" },
                new SqlParameter("@EName", SqlDbType.NVarChar, -1) { Value = EName ?? "" },
                new SqlParameter("@Year", SqlDbType.Int) { Value = Year },
                new SqlParameter("@CompanyID", SqlDbType.Int) { Value = CompanyID },
                new SqlParameter("@CreationUserID", SqlDbType.Int) { Value = CreationUserID },
                new SqlParameter("@CreationDate", SqlDbType.DateTime) { Value = DateTime.Now },
            };
            clsSQL sql = new clsSQL();
            return Simulate.Integer32(sql.ExecuteScalar(@"
INSERT INTO tbl_HolidayCalendar (AName, EName, Year, CompanyID, CreationUserID, CreationDate)
OUTPUT INSERTED.ID VALUES (@AName, @EName, @Year, @CompanyID, @CreationUserID, @CreationDate)",
                prm, sql.CreateDataBaseConnectionString(CompanyID)));
        }

        public int UpdateHolidayCalendar(int ID, string AName, string EName, int Year, int CompanyID, int ModificationUserID)
        {
            SqlParameter[] prm =
            {
                new SqlParameter("@ID", SqlDbType.Int) { Value = ID },
                new SqlParameter("@AName", SqlDbType.NVarChar, -1) { Value = AName ?? "" },
                new SqlParameter("@EName", SqlDbType.NVarChar, -1) { Value = EName ?? "" },
                new SqlParameter("@Year", SqlDbType.Int) { Value = Year },
                new SqlParameter("@CompanyID", SqlDbType.Int) { Value = CompanyID },
                new SqlParameter("@ModificationUserID", SqlDbType.Int) { Value = ModificationUserID },
                new SqlParameter("@ModificationDate", SqlDbType.DateTime) { Value = DateTime.Now },
            };
            clsSQL sql = new clsSQL();
            return sql.ExecuteNonQueryStatement(@"
UPDATE tbl_HolidayCalendar SET AName=@AName, EName=@EName, Year=@Year,
  ModificationUserID=@ModificationUserID, ModificationDate=@ModificationDate
WHERE ID=@ID AND CompanyID=@CompanyID",
                sql.CreateDataBaseConnectionString(CompanyID), prm);
        }

        public bool DeleteHolidayCalendar(int ID, int CompanyID)
        {
            SqlParameter[] prm =
            {
                new SqlParameter("@ID", SqlDbType.Int) { Value = ID },
                new SqlParameter("@CompanyID", SqlDbType.Int) { Value = CompanyID },
            };
            clsSQL sql = new clsSQL();
            sql.ExecuteNonQueryStatement(
                "DELETE FROM tbl_Holiday WHERE CalendarID=@ID AND CompanyID=@CompanyID",
                sql.CreateDataBaseConnectionString(CompanyID), prm);
            sql.ExecuteNonQueryStatement(
                "DELETE FROM tbl_HolidayCalendar WHERE ID=@ID AND CompanyID=@CompanyID",
                sql.CreateDataBaseConnectionString(CompanyID), prm);
            return true;
        }

        public DataTable SelectHolidays(int ID, int CalendarID, int CompanyID)
        {
            SqlParameter[] prm =
            {
                new SqlParameter("@ID", SqlDbType.Int) { Value = ID },
                new SqlParameter("@CalendarID", SqlDbType.Int) { Value = CalendarID },
                new SqlParameter("@CompanyID", SqlDbType.Int) { Value = CompanyID },
            };
            clsSQL sql = new clsSQL();
            return sql.ExecuteQueryStatement(@"
SELECT * FROM tbl_Holiday
WHERE (ID=@ID OR @ID=0)
  AND (CalendarID=@CalendarID OR @CalendarID=0)
  AND (CompanyID=@CompanyID OR @CompanyID=0)
ORDER BY HolidayDate",
                sql.CreateDataBaseConnectionString(CompanyID), prm);
        }

        public int InsertHoliday(int CalendarID, DateTime HolidayDate, string AName, string EName,
            bool IsPaid, int CompanyID, int CreationUserID)
        {
            SqlParameter[] prm =
            {
                new SqlParameter("@CalendarID", SqlDbType.Int) { Value = CalendarID },
                new SqlParameter("@HolidayDate", SqlDbType.DateTime) { Value = HolidayDate.Date },
                new SqlParameter("@AName", SqlDbType.NVarChar, -1) { Value = AName ?? "" },
                new SqlParameter("@EName", SqlDbType.NVarChar, -1) { Value = EName ?? "" },
                new SqlParameter("@IsPaid", SqlDbType.Bit) { Value = IsPaid },
                new SqlParameter("@CompanyID", SqlDbType.Int) { Value = CompanyID },
                new SqlParameter("@CreationUserID", SqlDbType.Int) { Value = CreationUserID },
                new SqlParameter("@CreationDate", SqlDbType.DateTime) { Value = DateTime.Now },
            };
            clsSQL sql = new clsSQL();
            return Simulate.Integer32(sql.ExecuteScalar(@"
INSERT INTO tbl_Holiday (CalendarID, HolidayDate, AName, EName, IsPaid, CompanyID, CreationUserID, CreationDate)
OUTPUT INSERTED.ID
VALUES (@CalendarID, @HolidayDate, @AName, @EName, @IsPaid, @CompanyID, @CreationUserID, @CreationDate)",
                prm, sql.CreateDataBaseConnectionString(CompanyID)));
        }

        public int UpdateHoliday(int ID, int CalendarID, DateTime HolidayDate, string AName, string EName,
            bool IsPaid, int CompanyID, int ModificationUserID)
        {
            SqlParameter[] prm =
            {
                new SqlParameter("@ID", SqlDbType.Int) { Value = ID },
                new SqlParameter("@CalendarID", SqlDbType.Int) { Value = CalendarID },
                new SqlParameter("@HolidayDate", SqlDbType.DateTime) { Value = HolidayDate.Date },
                new SqlParameter("@AName", SqlDbType.NVarChar, -1) { Value = AName ?? "" },
                new SqlParameter("@EName", SqlDbType.NVarChar, -1) { Value = EName ?? "" },
                new SqlParameter("@IsPaid", SqlDbType.Bit) { Value = IsPaid },
                new SqlParameter("@CompanyID", SqlDbType.Int) { Value = CompanyID },
                new SqlParameter("@ModificationUserID", SqlDbType.Int) { Value = ModificationUserID },
                new SqlParameter("@ModificationDate", SqlDbType.DateTime) { Value = DateTime.Now },
            };
            clsSQL sql = new clsSQL();
            return sql.ExecuteNonQueryStatement(@"
UPDATE tbl_Holiday SET CalendarID=@CalendarID, HolidayDate=@HolidayDate, AName=@AName, EName=@EName,
  IsPaid=@IsPaid, ModificationUserID=@ModificationUserID, ModificationDate=@ModificationDate
WHERE ID=@ID AND CompanyID=@CompanyID",
                sql.CreateDataBaseConnectionString(CompanyID), prm);
        }

        public bool DeleteHoliday(int ID, int CompanyID)
        {
            SqlParameter[] prm =
            {
                new SqlParameter("@ID", SqlDbType.Int) { Value = ID },
                new SqlParameter("@CompanyID", SqlDbType.Int) { Value = CompanyID },
            };
            clsSQL sql = new clsSQL();
            sql.ExecuteNonQueryStatement(
                "DELETE FROM tbl_Holiday WHERE ID=@ID AND CompanyID=@CompanyID",
                sql.CreateDataBaseConnectionString(CompanyID), prm);
            return true;
        }

        // ==========================================================
        // LEAVE BALANCES
        // ==========================================================
        public DataTable SelectLeaveBalances(int ID, int EmployeeID, int LeaveTypeID, int Year, int CompanyID)
        {
            SqlParameter[] prm =
            {
                new SqlParameter("@ID", SqlDbType.Int) { Value = ID },
                new SqlParameter("@EmployeeID", SqlDbType.Int) { Value = EmployeeID },
                new SqlParameter("@LeaveTypeID", SqlDbType.Int) { Value = LeaveTypeID },
                new SqlParameter("@Year", SqlDbType.Int) { Value = Year },
                new SqlParameter("@CompanyID", SqlDbType.Int) { Value = CompanyID },
            };
            clsSQL sql = new clsSQL();
            return sql.ExecuteQueryStatement(@"
SELECT b.*, t.Code AS LeaveTypeCode, t.AName AS LeaveTypeAName, t.EName AS LeaveTypeEName,
       ISNULL(b.EntitledDays,0) - ISNULL(b.UsedDays,0) - ISNULL(b.PendingDays,0) AS RemainingDays
FROM tbl_LeaveBalance b
LEFT JOIN tbl_LeaveType t ON t.ID = b.LeaveTypeID
WHERE (b.ID=@ID OR @ID=0)
  AND (b.EmployeeID=@EmployeeID OR @EmployeeID=0)
  AND (b.LeaveTypeID=@LeaveTypeID OR @LeaveTypeID=0)
  AND (b.Year=@Year OR @Year=0)
  AND (b.CompanyID=@CompanyID OR @CompanyID=0)
ORDER BY b.Year DESC, t.Code",
                sql.CreateDataBaseConnectionString(CompanyID), prm);
        }

        public int UpsertLeaveBalance(int EmployeeID, int LeaveTypeID, int Year,
            decimal EntitledDays, decimal UsedDays, decimal PendingDays,
            int CompanyID, int UserID, SqlTransaction trn = null)
        {
            SqlParameter[] prm =
            {
                new SqlParameter("@EmployeeID", SqlDbType.Int) { Value = EmployeeID },
                new SqlParameter("@LeaveTypeID", SqlDbType.Int) { Value = LeaveTypeID },
                new SqlParameter("@Year", SqlDbType.Int) { Value = Year },
                new SqlParameter("@EntitledDays", SqlDbType.Decimal) { Value = EntitledDays },
                new SqlParameter("@UsedDays", SqlDbType.Decimal) { Value = UsedDays },
                new SqlParameter("@PendingDays", SqlDbType.Decimal) { Value = PendingDays },
                new SqlParameter("@CompanyID", SqlDbType.Int) { Value = CompanyID },
                new SqlParameter("@UserID", SqlDbType.Int) { Value = UserID },
                new SqlParameter("@Now", SqlDbType.DateTime) { Value = DateTime.Now },
            };
            clsSQL sql = new clsSQL();
            string q = @"
DECLARE @ExistingID INT = (SELECT TOP 1 ID FROM tbl_LeaveBalance
  WHERE EmployeeID=@EmployeeID AND LeaveTypeID=@LeaveTypeID AND Year=@Year AND CompanyID=@CompanyID);
IF @ExistingID IS NULL
BEGIN
  INSERT INTO tbl_LeaveBalance (EmployeeID, LeaveTypeID, Year, EntitledDays, UsedDays, PendingDays, CompanyID, CreationUserID, CreationDate)
  OUTPUT INSERTED.ID
  VALUES (@EmployeeID, @LeaveTypeID, @Year, @EntitledDays, @UsedDays, @PendingDays, @CompanyID, @UserID, @Now);
END
ELSE
BEGIN
  UPDATE tbl_LeaveBalance SET
    EntitledDays=@EntitledDays, UsedDays=@UsedDays, PendingDays=@PendingDays,
    ModificationUserID=@UserID, ModificationDate=@Now
  WHERE ID=@ExistingID;
  SELECT @ExistingID;
END";
            return Simulate.Integer32(sql.ExecuteScalar(q, prm, sql.CreateDataBaseConnectionString(CompanyID), trn));
        }

        /// <summary>
        /// Seeds annual / sick balances from contract leave entitlement columns for the given year (default current).
        /// </summary>
        public void SeedBalancesFromContract(int employeeId, int annualLeaveDays, int sickLeaveDays,
            int companyId, int userId = 1, int year = 0, SqlTransaction trn = null,
            int sickExtendedDays = 0)
        {
            if (year <= 0) year = DateTime.Now.Year;
            EnsureJordanLeaveTypes(companyId, userId, trn);

            int annualTypeId = ResolveLeaveTypeIdByCode("ANNUAL", companyId, trn);
            int sickTypeId = ResolveLeaveTypeIdByCode("SICK", companyId, trn);
            int sickExtTypeId = ResolveLeaveTypeIdByCode("SICK_EXT", companyId, trn);

            if (annualTypeId > 0 && annualLeaveDays > 0)
                UpsertLeaveBalance(employeeId, annualTypeId, year, annualLeaveDays, 0, 0, companyId, userId, trn);

            if (sickTypeId > 0 && sickLeaveDays > 0)
                UpsertLeaveBalance(employeeId, sickTypeId, year, sickLeaveDays, 0, 0, companyId, userId, trn);

            if (sickExtTypeId > 0 && sickExtendedDays > 0)
                UpsertLeaveBalance(employeeId, sickExtTypeId, year, sickExtendedDays, 0, 0, companyId, userId, trn);

            int maternityTypeId = ResolveLeaveTypeIdByCode("MATERNITY", companyId, trn);
            int paternityTypeId = ResolveLeaveTypeIdByCode("PATERNITY", companyId, trn);
            if (maternityTypeId > 0)
                UpsertLeaveBalance(employeeId, maternityTypeId, year, 70, 0, 0, companyId, userId, trn);
            if (paternityTypeId > 0)
                UpsertLeaveBalance(employeeId, paternityTypeId, year, 3, 0, 0, companyId, userId, trn);
        }

        public void SeedBalancesFromContract(int employeeId, int companyId, int userId = 1, SqlTransaction trn = null)
        {
            clsSQL sql = new clsSQL();
            SqlParameter[] prm =
            {
                new SqlParameter("@EmployeeID", SqlDbType.Int) { Value = employeeId },
                new SqlParameter("@CompanyID", SqlDbType.Int) { Value = companyId },
            };
            DataTable dt = sql.ExecuteQueryStatement(@"
SELECT TOP 1
  ISNULL(StartDate, CreationDate) AS StartDate,
  ISNULL(AnnualLeaveDaysPerYear, 14) AS AnnualLeaveDaysPerYear,
  ISNULL(AnnualLeaveDaysAfter5Years, 21) AS AnnualLeaveDaysAfter5Years,
  ISNULL(SickLeaveFullPayDaysPerYear, 14) AS SickLeaveFullPayDaysPerYear,
  ISNULL(SickLeaveExtendedDaysPerYear, 14) AS SickLeaveExtendedDaysPerYear
FROM tbl_EmployeeContract
WHERE EmployeeID = @EmployeeID AND CompanyID = @CompanyID AND ISNULL(IsActive,0) = 1
ORDER BY ID DESC",
                sql.CreateDataBaseConnectionString(companyId), prm, trn);

            if (dt == null || dt.Rows.Count == 0) return;

            DataRow row = dt.Rows[0];
            DateTime startDate = Convert.ToDateTime(row["StartDate"]).Date;
            int years = ResolveYearsOfService(startDate, DateTime.Now);
            int annualDays = years >= 5
                ? Simulate.Integer32(row["AnnualLeaveDaysAfter5Years"])
                : Simulate.Integer32(row["AnnualLeaveDaysPerYear"]);
            if (annualDays <= 0) annualDays = years >= 5 ? 21 : 14;

            SeedBalancesFromContract(
                employeeId,
                annualDays,
                Simulate.Integer32(row["SickLeaveFullPayDaysPerYear"]),
                companyId,
                userId,
                DateTime.Now.Year,
                trn,
                Simulate.Integer32(row["SickLeaveExtendedDaysPerYear"]));
        }

        public static int ResolveYearsOfService(DateTime startDate, DateTime asOf)
        {
            if (startDate.Year <= 1900) return 0;
            int years = asOf.Year - startDate.Year;
            if (asOf.Month < startDate.Month ||
                (asOf.Month == startDate.Month && asOf.Day < startDate.Day))
                years--;
            return Math.Max(0, years);
        }

        /// <summary>Ensure Jordan-specific leave types exist (sick extended, maternity, etc.).</summary>
        public void EnsureJordanLeaveTypes(int companyId, int userId = 1, SqlTransaction trn = null)
        {
            EnsureLeaveType("SICK_EXT", "إجازة مرضية (فترة ثانية)", "Sick Leave (Extended)", false, companyId, userId, trn);
            EnsureLeaveType("MATERNITY", "إجازة أمومة", "Maternity Leave", true, companyId, userId, trn);
            EnsureLeaveType("PATERNITY", "إجازة أبوة", "Paternity Leave", true, companyId, userId, trn);
            EnsureLeaveType("HAJJ", "إجازة حج", "Hajj Leave", true, companyId, userId, trn);
            EnsureLeaveType("BEREAVEMENT", "إجازة وفاة", "Bereavement Leave", true, companyId, userId, trn);
        }

        void EnsureLeaveType(string code, string aName, string eName, bool isPaid,
            int companyId, int userId, SqlTransaction trn)
        {
            if (ResolveLeaveTypeIdByCode(code, companyId, trn) > 0) return;
            InsertLeaveType(code, aName, eName, isPaid, true, 0, companyId, userId, trn);
        }

        /// <summary>Monthly accrual: entitled = annualDays * monthsWorked / 12.</summary>
        public void AccrueLeaveBalancesForYear(int companyId, int year = 0, int userId = 1, SqlTransaction trn = null)
        {
            if (year <= 0) year = DateTime.Now.Year;
            clsSQL sql = new clsSQL();
            SqlParameter[] prm =
            {
                new SqlParameter("@CompanyID", SqlDbType.Int) { Value = companyId },
            };
            DataTable employees = sql.ExecuteQueryStatement(@"
SELECT ID AS EmployeeID FROM tbl_employee WHERE CompanyID = @CompanyID AND ISNULL(IsActive,1) = 1",
                sql.CreateDataBaseConnectionString(companyId), prm, trn);
            if (employees == null) return;

            foreach (DataRow emp in employees.Rows)
            {
                int employeeId = Simulate.Integer32(emp["EmployeeID"]);
                SeedBalancesFromContract(employeeId, companyId, userId, trn);
            }
        }

        /// <summary>Seed Jordan public holidays for a calendar year (fixed + approximate Islamic dates).</summary>
        public int SeedJordanPublicHolidays(int year, int companyId, int userId = 1, SqlTransaction trn = null)
        {
            int calendarId = ResolveOrCreateHolidayCalendar(year, companyId, userId, trn);
            if (calendarId <= 0) return 0;

            var holidays = new List<(DateTime Date, string AName, string EName)>
            {
                (new DateTime(year, 1, 1), "رأس السنة الميلادية", "New Year's Day"),
                (new DateTime(year, 5, 1), "عيد العمال", "Labour Day"),
                (new DateTime(year, 5, 25), "عيد الاستقلال", "Independence Day"),
                (new DateTime(year, 12, 25), "عيد الميلاد", "Christmas Day"),
            };

            // Approximate Islamic holidays — HR should adjust exact dates each year.
            if (year == 2026)
            {
                holidays.Add((new DateTime(2026, 3, 20), "عيد الفطر", "Eid Al-Fitr"));
                holidays.Add((new DateTime(2026, 3, 21), "عيد الفطر", "Eid Al-Fitr"));
                holidays.Add((new DateTime(2026, 5, 27), "عيد الأضحى", "Eid Al-Adha"));
                holidays.Add((new DateTime(2026, 5, 28), "عيد الأضحى", "Eid Al-Adha"));
                holidays.Add((new DateTime(2026, 9, 4), "المولد النبوي", "Prophet's Birthday"));
            }

            int inserted = 0;
            foreach (var h in holidays)
            {
                if (HolidayExists(calendarId, h.Date, companyId, trn)) continue;
                InsertHoliday(calendarId, h.Date, h.AName, h.EName, true, companyId, userId);
                inserted++;
            }
            return inserted;
        }

        int ResolveOrCreateHolidayCalendar(int year, int companyId, int userId, SqlTransaction trn)
        {
            clsSQL sql = new clsSQL();
            SqlParameter[] prm =
            {
                new SqlParameter("@Year", SqlDbType.Int) { Value = year },
                new SqlParameter("@CompanyID", SqlDbType.Int) { Value = companyId },
            };
            object existing = sql.ExecuteScalar(
                "SELECT TOP 1 ID FROM tbl_HolidayCalendar WHERE Year=@Year AND CompanyID=@CompanyID",
                prm, sql.CreateDataBaseConnectionString(companyId), trn);
            int id = Simulate.Integer32(existing);
            if (id > 0) return id;
            return InsertHolidayCalendar($"Jordan {year}", $"Jordan {year}", year, companyId, userId);
        }

        bool HolidayExists(int calendarId, DateTime date, int companyId, SqlTransaction trn)
        {
            clsSQL sql = new clsSQL();
            SqlParameter[] prm =
            {
                new SqlParameter("@CalendarID", SqlDbType.Int) { Value = calendarId },
                new SqlParameter("@HolidayDate", SqlDbType.DateTime) { Value = date.Date },
                new SqlParameter("@CompanyID", SqlDbType.Int) { Value = companyId },
            };
            object val = sql.ExecuteScalar(@"
SELECT TOP 1 ID FROM tbl_Holiday
WHERE CalendarID=@CalendarID AND CAST(HolidayDate AS DATE)=CAST(@HolidayDate AS DATE) AND CompanyID=@CompanyID",
                prm, sql.CreateDataBaseConnectionString(companyId), trn);
            return Simulate.Integer32(val) > 0;
        }

        /// <summary>
        /// Jordan end-of-service benefit estimate per Labour Law (simplified):
        /// last drawn wage × years of service (fractional years prorated).
        /// </summary>
        public DataTable CalculateEndOfService(int employeeId, DateTime terminationDate, int companyId)
        {
            clsSQL sql = new clsSQL();
            SqlParameter[] prm =
            {
                new SqlParameter("@EmployeeID", SqlDbType.Int) { Value = employeeId },
                new SqlParameter("@CompanyID", SqlDbType.Int) { Value = companyId },
            };

            DataTable dt = sql.ExecuteQueryStatement(@"
SELECT TOP 1
  e.ID AS EmployeeID,
  e.AName AS EmployeeName,
  e.EmployeeCode,
  ISNULL(c.StartDate, e.CreationDate) AS StartDate,
  ISNULL(c.BasicSalary, 0) AS BasicSalary,
  ISNULL(c.IsOpenEnded, 0) AS IsOpenEnded,
  c.EndDate
FROM tbl_employee e
LEFT JOIN tbl_EmployeeContract c ON c.EmployeeID = e.ID AND c.CompanyID = e.CompanyID AND ISNULL(c.IsActive,0)=1
WHERE e.ID = @EmployeeID AND e.CompanyID = @CompanyID
ORDER BY c.ID DESC",
                sql.CreateDataBaseConnectionString(companyId), prm);

            if (dt == null || dt.Rows.Count == 0)
                return dt;

            DataRow row = dt.Rows[0];
            DateTime startDate = Convert.ToDateTime(row["StartDate"]).Date;
            decimal basicSalary = Simulate.Decimal(row["BasicSalary"]);
            decimal years = (decimal)(terminationDate.Date - startDate).TotalDays / 365.25m;
            if (years < 0) years = 0;

            // Simplified: one month salary per year of service (common Jordan reference; verify per contract/policy).
            decimal eosAmount = Math.Round(basicSalary * years, 3);

            dt.Columns.Add("TerminationDate", typeof(DateTime));
            dt.Columns.Add("YearsOfService", typeof(decimal));
            dt.Columns.Add("LastBasicSalary", typeof(decimal));
            dt.Columns.Add("EstimatedEOS", typeof(decimal));
            dt.Columns.Add("Notes", typeof(string));

            row["TerminationDate"] = terminationDate.Date;
            row["YearsOfService"] = Math.Round(years, 2);
            row["LastBasicSalary"] = basicSalary;
            row["EstimatedEOS"] = eosAmount;
            row["Notes"] = "Estimate only — verify against contract, law and SSC rules before settlement.";

            return dt;
        }

        int ResolveLeaveTypeIdByCode(string code, int companyId, SqlTransaction trn = null)
        {
            clsSQL sql = new clsSQL();
            SqlParameter[] prm =
            {
                new SqlParameter("@Code", SqlDbType.NVarChar, 50) { Value = code },
                new SqlParameter("@CompanyID", SqlDbType.Int) { Value = companyId },
            };
            return Simulate.Integer32(sql.ExecuteScalar(
                "SELECT TOP 1 ID FROM tbl_LeaveType WHERE Code=@Code AND CompanyID=@CompanyID",
                prm, sql.CreateDataBaseConnectionString(companyId), trn));
        }

        // ==========================================================
        // LEAVE REQUESTS
        // ==========================================================
        public DataTable SelectLeaveRequests(int ID, int EmployeeID, int DocumentStatus, int CompanyID, string Guid = "", int Year = 0)
        {
            SqlParameter[] prm =
            {
                new SqlParameter("@ID", SqlDbType.Int) { Value = ID },
                new SqlParameter("@EmployeeID", SqlDbType.Int) { Value = EmployeeID },
                new SqlParameter("@DocumentStatus", SqlDbType.Int) { Value = DocumentStatus },
                new SqlParameter("@CompanyID", SqlDbType.Int) { Value = CompanyID },
                new SqlParameter("@Guid", SqlDbType.NVarChar, 50) { Value = Guid ?? "" },
                new SqlParameter("@Year", SqlDbType.Int) { Value = Year },
            };
            clsSQL sql = new clsSQL();
            return sql.ExecuteQueryStatement(@"
SELECT r.*, t.Code AS LeaveTypeCode, t.AName AS LeaveTypeAName, t.EName AS LeaveTypeEName,
       t.IsPaid, CAST(r.Guid AS NVARCHAR(50)) AS GuidText,
       ISNULL(e.AName, '') AS EmployeeName,
       ISNULL(b.PendingDays, 0) AS BalancePendingDays
FROM tbl_LeaveRequest r
LEFT JOIN tbl_LeaveType t ON t.ID = r.LeaveTypeID
LEFT JOIN tbl_employee e ON e.ID = r.EmployeeID AND e.CompanyID = r.CompanyID
LEFT JOIN tbl_LeaveBalance b ON b.EmployeeID = r.EmployeeID
  AND b.LeaveTypeID = r.LeaveTypeID
  AND b.Year = YEAR(r.FromDate)
  AND b.CompanyID = r.CompanyID
WHERE (r.ID=@ID OR @ID=0)
  AND (r.EmployeeID=@EmployeeID OR @EmployeeID=0)
  AND (r.DocumentStatus=@DocumentStatus OR @DocumentStatus < 0)
  AND (r.CompanyID=@CompanyID OR @CompanyID=0)
  AND (CAST(r.Guid AS NVARCHAR(50))=@Guid OR @Guid='')
  AND (@Year=0 OR YEAR(r.FromDate)=@Year OR YEAR(r.ToDate)=@Year)
ORDER BY r.FromDate DESC",
                sql.CreateDataBaseConnectionString(CompanyID), prm);
        }

        public string SelectLeaveRequestGuid(int id, int companyId)
        {
            if (id <= 0) return "";
            clsSQL sql = new clsSQL();
            SqlParameter[] prm =
            {
                new SqlParameter("@ID", SqlDbType.Int) { Value = id },
                new SqlParameter("@CompanyID", SqlDbType.Int) { Value = companyId },
            };
            object val = sql.ExecuteScalar(@"
SELECT TOP 1 CAST(Guid AS NVARCHAR(50))
FROM tbl_LeaveRequest
WHERE ID=@ID AND CompanyID=@CompanyID",
                prm, sql.CreateDataBaseConnectionString(companyId), null);
            return Simulate.String(val);
        }

        void ValidateLeaveNoOverlap(int employeeId, DateTime fromDate, DateTime toDate, int companyId, int excludeId = 0)
        {
            clsSQL sql = new clsSQL();
            SqlParameter[] prm =
            {
                new SqlParameter("@EmployeeID", SqlDbType.Int) { Value = employeeId },
                new SqlParameter("@CompanyID", SqlDbType.Int) { Value = companyId },
                new SqlParameter("@FromDate", SqlDbType.DateTime) { Value = fromDate.Date },
                new SqlParameter("@ToDate", SqlDbType.DateTime) { Value = toDate.Date },
                new SqlParameter("@ExcludeID", SqlDbType.Int) { Value = excludeId },
            };
            object val = sql.ExecuteScalar(@"
SELECT COUNT(*)
FROM tbl_LeaveRequest
WHERE EmployeeID=@EmployeeID AND CompanyID=@CompanyID
  AND ID <> @ExcludeID
  AND ISNULL(DocumentStatus,0) IN (0,1,2)
  AND FromDate <= @ToDate AND ToDate >= @FromDate",
                prm, sql.CreateDataBaseConnectionString(companyId), null);
            if (Simulate.Integer32(val) > 0)
                throw new Exception("Leave dates overlap an existing approved or pending request.");
        }

        public int InsertLeaveRequest(int EmployeeID, int LeaveTypeID, DateTime FromDate, DateTime ToDate,
            decimal Days, string Reason, int BranchID, int CompanyID, int CreationUserID,
            int DocumentStatus = 0, SqlTransaction trn = null)
        {
            if (Days <= 0)
                Days = (decimal)(ToDate.Date - FromDate.Date).TotalDays + 1;

            ValidateLeaveNoOverlap(EmployeeID, FromDate, ToDate, CompanyID, 0);

            SqlParameter[] prm =
            {
                new SqlParameter("@EmployeeID", SqlDbType.Int) { Value = EmployeeID },
                new SqlParameter("@LeaveTypeID", SqlDbType.Int) { Value = LeaveTypeID },
                new SqlParameter("@FromDate", SqlDbType.DateTime) { Value = FromDate.Date },
                new SqlParameter("@ToDate", SqlDbType.DateTime) { Value = ToDate.Date },
                new SqlParameter("@Days", SqlDbType.Decimal) { Value = Days },
                new SqlParameter("@Reason", SqlDbType.NVarChar, -1) { Value = Reason ?? "" },
                new SqlParameter("@DocumentStatus", SqlDbType.Int) { Value = DocumentStatus },
                new SqlParameter("@BranchID", SqlDbType.Int) { Value = BranchID },
                new SqlParameter("@CompanyID", SqlDbType.Int) { Value = CompanyID },
                new SqlParameter("@CreationUserID", SqlDbType.Int) { Value = CreationUserID },
                new SqlParameter("@CreationDate", SqlDbType.DateTime) { Value = DateTime.Now },
            };
            clsSQL sql = new clsSQL();
            int id = Simulate.Integer32(sql.ExecuteScalar(@"
INSERT INTO tbl_LeaveRequest
  (Guid, EmployeeID, LeaveTypeID, FromDate, ToDate, Days, Reason, DocumentStatus, BranchID,
   CompanyID, CreationUserID, CreationDate)
OUTPUT INSERTED.ID
VALUES (NEWID(), @EmployeeID, @LeaveTypeID, @FromDate, @ToDate, @Days, @Reason, @DocumentStatus, @BranchID,
        @CompanyID, @CreationUserID, @CreationDate)",
                prm, sql.CreateDataBaseConnectionString(CompanyID), trn));

            if (id > 0 && DocumentStatus != 2)
                ReservePendingDays(EmployeeID, LeaveTypeID, FromDate.Year, Days, CompanyID, CreationUserID, trn);

            return id;
        }

        public int UpdateLeaveRequest(int ID, int EmployeeID, int LeaveTypeID, DateTime FromDate, DateTime ToDate,
            decimal Days, string Reason, int BranchID, int CompanyID, int ModificationUserID)
        {
            if (Days <= 0)
                Days = (decimal)(ToDate.Date - FromDate.Date).TotalDays + 1;

            ValidateLeaveNoOverlap(EmployeeID, FromDate, ToDate, CompanyID, ID);

            SqlParameter[] prm =
            {
                new SqlParameter("@ID", SqlDbType.Int) { Value = ID },
                new SqlParameter("@EmployeeID", SqlDbType.Int) { Value = EmployeeID },
                new SqlParameter("@LeaveTypeID", SqlDbType.Int) { Value = LeaveTypeID },
                new SqlParameter("@FromDate", SqlDbType.DateTime) { Value = FromDate.Date },
                new SqlParameter("@ToDate", SqlDbType.DateTime) { Value = ToDate.Date },
                new SqlParameter("@Days", SqlDbType.Decimal) { Value = Days },
                new SqlParameter("@Reason", SqlDbType.NVarChar, -1) { Value = Reason ?? "" },
                new SqlParameter("@BranchID", SqlDbType.Int) { Value = BranchID },
                new SqlParameter("@CompanyID", SqlDbType.Int) { Value = CompanyID },
                new SqlParameter("@ModificationUserID", SqlDbType.Int) { Value = ModificationUserID },
                new SqlParameter("@ModificationDate", SqlDbType.DateTime) { Value = DateTime.Now },
            };
            clsSQL sql = new clsSQL();
            return sql.ExecuteNonQueryStatement(@"
UPDATE tbl_LeaveRequest SET
  EmployeeID=@EmployeeID, LeaveTypeID=@LeaveTypeID, FromDate=@FromDate, ToDate=@ToDate,
  Days=@Days, Reason=@Reason, BranchID=@BranchID,
  ModificationUserID=@ModificationUserID, ModificationDate=@ModificationDate
WHERE ID=@ID AND CompanyID=@CompanyID AND ISNULL(DocumentStatus,0) <> 2",
                sql.CreateDataBaseConnectionString(CompanyID), prm);
        }

        public bool DeleteLeaveRequest(int ID, int CompanyID)
        {
            clsSQL sql = new clsSQL();
            SqlParameter[] sel =
            {
                new SqlParameter("@ID", SqlDbType.Int) { Value = ID },
                new SqlParameter("@CompanyID", SqlDbType.Int) { Value = CompanyID },
            };
            DataTable dt = sql.ExecuteQueryStatement(@"
SELECT EmployeeID, LeaveTypeID, FromDate, ISNULL(Days,0) AS Days, ISNULL(DocumentStatus,0) AS DocumentStatus
FROM tbl_LeaveRequest
WHERE ID=@ID AND CompanyID=@CompanyID",
                sql.CreateDataBaseConnectionString(CompanyID), sel);

            if (dt != null && dt.Rows.Count > 0)
            {
                DataRow row = dt.Rows[0];
                int status = Simulate.Integer32(row["DocumentStatus"]);
                if (status == (int)DocumentStatus.Posted)
                    throw new Exception("Posted leave requests cannot be deleted.");

                if (status != (int)DocumentStatus.Posted)
                {
                    ReleasePendingDays(
                        Simulate.Integer32(row["EmployeeID"]),
                        Simulate.Integer32(row["LeaveTypeID"]),
                        Convert.ToDateTime(row["FromDate"]).Year,
                        Simulate.Decimal(row["Days"]),
                        CompanyID,
                        0,
                        null);
                }
            }

            SqlParameter[] prm =
            {
                new SqlParameter("@ID", SqlDbType.Int) { Value = ID },
                new SqlParameter("@CompanyID", SqlDbType.Int) { Value = CompanyID },
            };
            sql.ExecuteNonQueryStatement(@"
DELETE FROM tbl_LeaveRequest
WHERE ID=@ID AND CompanyID=@CompanyID AND ISNULL(DocumentStatus,0) NOT IN (2)",
                sql.CreateDataBaseConnectionString(CompanyID), prm);
            return true;
        }

        /// <summary>Rejects a pending leave request and releases reserved pending days.</summary>
        public bool RejectLeaveRequest(string documentGuid, int userId, int companyId, SqlTransaction trn = null)
        {
            clsSQL sql = new clsSQL();
            SqlParameter[] sel =
            {
                new SqlParameter("@Guid", SqlDbType.UniqueIdentifier) { Value = Simulate.Guid(documentGuid) },
                new SqlParameter("@CompanyID", SqlDbType.Int) { Value = companyId },
            };

            DataTable dt = sql.ExecuteQueryStatement(@"
SELECT EmployeeID, LeaveTypeID, FromDate, ISNULL(Days,0) AS Days, ISNULL(DocumentStatus,0) AS DocumentStatus
FROM tbl_LeaveRequest
WHERE Guid = @Guid AND CompanyID = @CompanyID",
                sql.CreateDataBaseConnectionString(companyId), sel, trn);

            if (dt == null || dt.Rows.Count == 0) return false;

            DataRow row = dt.Rows[0];
            int status = Simulate.Integer32(row["DocumentStatus"]);
            if (status == (int)DocumentStatus.Posted)
                return true;
            if (status == (int)DocumentStatus.Rejected)
                return true;

            ReleasePendingDays(
                Simulate.Integer32(row["EmployeeID"]),
                Simulate.Integer32(row["LeaveTypeID"]),
                Convert.ToDateTime(row["FromDate"]).Year,
                Simulate.Decimal(row["Days"]),
                companyId,
                userId,
                trn);

            SqlParameter[] upd =
            {
                new SqlParameter("@Guid", SqlDbType.UniqueIdentifier) { Value = Simulate.Guid(documentGuid) },
                new SqlParameter("@CompanyID", SqlDbType.Int) { Value = companyId },
                new SqlParameter("@UserId", SqlDbType.Int) { Value = userId },
            };
            sql.ExecuteNonQueryStatement(@"
UPDATE tbl_LeaveRequest SET
  DocumentStatus = 3,
  ModificationUserID = @UserId,
  ModificationDate = GETDATE()
WHERE Guid = @Guid AND CompanyID = @CompanyID",
                sql.CreateDataBaseConnectionString(companyId), upd, trn);
            return true;
        }

        /// <summary>Encashes unused leave balance and optionally posts a payroll JV.</summary>
        public LeaveEncashmentResult ProcessLeaveEncashment(int employeeId, int leaveTypeId, decimal days, int year,
            int companyId, int userId = 1, int branchId = 1, bool postJournal = true, SqlTransaction trn = null)
        {
            if (days <= 0) throw new Exception("Encashment days must be greater than zero.");
            if (year <= 0) year = DateTime.Now.Year;

            decimal remaining = GetRemainingLeaveDays(employeeId, leaveTypeId, year, companyId, trn);
            if (days > remaining)
                throw new Exception($"Insufficient leave balance. Remaining: {remaining:N2} days.");

            SqlParameter[] bal =
            {
                new SqlParameter("@EmployeeID", SqlDbType.Int) { Value = employeeId },
                new SqlParameter("@LeaveTypeID", SqlDbType.Int) { Value = leaveTypeId },
                new SqlParameter("@Year", SqlDbType.Int) { Value = year },
                new SqlParameter("@Days", SqlDbType.Decimal) { Value = days },
                new SqlParameter("@CompanyID", SqlDbType.Int) { Value = companyId },
                new SqlParameter("@UserID", SqlDbType.Int) { Value = userId },
            };
            clsSQL sql = new clsSQL();
            sql.ExecuteNonQueryStatement(@"
UPDATE tbl_LeaveBalance SET
  UsedDays = ISNULL(UsedDays,0) + @Days,
  ModificationUserID = @UserID,
  ModificationDate = GETDATE()
WHERE EmployeeID=@EmployeeID AND LeaveTypeID=@LeaveTypeID AND Year=@Year AND CompanyID=@CompanyID",
                sql.CreateDataBaseConnectionString(companyId), bal, trn);

            decimal dailyRate = ResolveEmployeeDailyRate(employeeId, companyId, trn);
            decimal amount = Math.Round(dailyRate * days, 3);

            string jvGuid = "";
            if (postJournal && amount > 0)
                jvGuid = PostLeaveEncashmentJournal(employeeId, amount, branchId, companyId, userId, trn);

            return new LeaveEncashmentResult { Amount = amount, JvGuid = jvGuid };
        }

        string PostLeaveEncashmentJournal(int employeeId, decimal amount, int branchId, int companyId, int userId, SqlTransaction trn)
        {
            cls_AccountSetting settings = new cls_AccountSetting();
            DataTable payableSetting = settings.SelectAccountSetting(
                0, (int)MainClasses.clsEnum.AccountMainSetting.Employees, companyId);
            int payableAccountId = payableSetting != null && payableSetting.Rows.Count > 0
                ? Simulate.Integer32(payableSetting.Rows[0]["AccountID"])
                : 0;

            clsEmployeeContract contractSvc = new clsEmployeeContract();
            int basicElementId = contractSvc.GetBasicSalaryElementID(companyId, trn);
            int expenseAccountId = 0;
            if (basicElementId > 0)
            {
                clsSQL sql = new clsSQL();
                SqlParameter[] prm =
                {
                    new SqlParameter("@ID", SqlDbType.Int) { Value = basicElementId },
                    new SqlParameter("@CompanyID", SqlDbType.Int) { Value = companyId },
                };
                object val = sql.ExecuteScalar(
                    "SELECT TOP 1 ISNULL(CompanyDebitAccountID,0) FROM tbl_SalariesElements WHERE ID=@ID AND CompanyID=@CompanyID",
                    prm, sql.CreateDataBaseConnectionString(companyId), trn);
                expenseAccountId = Simulate.Integer32(val);
            }

            if (payableAccountId <= 0 || expenseAccountId <= 0)
                throw new Exception("Configure employee payable and payroll expense GL accounts before encashment posting.");

            clsJournalVoucherHeader jvh = new clsJournalVoucherHeader();
            string jvGuid = jvh.InsertJournalVoucherHeader(
                branchId, 0,
                $"Leave encashment — Employee {employeeId}",
                "",
                (int)MainClasses.clsEnum.VoucherType.Payroll,
                companyId, DateTime.Now, userId, "", 0, trn);

            clsJournalVoucherDetails jvd = new clsJournalVoucherDetails();
            jvd.InsertJournalVoucherDetails(
                jvGuid, 1, expenseAccountId, 0, amount, 0, amount,
                1, 1, amount, branchId, 0, DateTime.Now,
                "Leave encashment expense", companyId, userId, "", trn);
            jvd.InsertJournalVoucherDetails(
                jvGuid, 2, payableAccountId, employeeId, 0, amount, -amount,
                1, 1, amount, branchId, 0, DateTime.Now,
                "Leave encashment payable", companyId, userId, "", trn);

            clsJournalVoucherHeader check = new clsJournalVoucherHeader();
            if (!check.CheckJVMatch(jvGuid, companyId, trn))
                throw new Exception("Leave encashment journal voucher is not balanced.");

            return jvGuid;
        }

        public class LeaveEncashmentResult
        {
            public decimal Amount { get; set; }
            public string JvGuid { get; set; }
        }

        void ReservePendingDays(int employeeId, int leaveTypeId, int year, decimal days,
            int companyId, int userId, SqlTransaction trn)
        {
            if (days <= 0 || leaveTypeId <= 0) return;
            EnsureLeaveBalanceRow(employeeId, leaveTypeId, year, companyId, userId, trn);

            SqlParameter[] prm =
            {
                new SqlParameter("@EmployeeID", SqlDbType.Int) { Value = employeeId },
                new SqlParameter("@LeaveTypeID", SqlDbType.Int) { Value = leaveTypeId },
                new SqlParameter("@Year", SqlDbType.Int) { Value = year },
                new SqlParameter("@Days", SqlDbType.Decimal) { Value = days },
                new SqlParameter("@CompanyID", SqlDbType.Int) { Value = companyId },
                new SqlParameter("@UserID", SqlDbType.Int) { Value = userId },
            };
            clsSQL sql = new clsSQL();
            sql.ExecuteNonQueryStatement(@"
UPDATE tbl_LeaveBalance SET
  PendingDays = ISNULL(PendingDays,0) + @Days,
  ModificationUserID = @UserID,
  ModificationDate = GETDATE()
WHERE EmployeeID=@EmployeeID AND LeaveTypeID=@LeaveTypeID AND Year=@Year AND CompanyID=@CompanyID",
                sql.CreateDataBaseConnectionString(companyId), prm, trn);
        }

        void ReleasePendingDays(int employeeId, int leaveTypeId, int year, decimal days,
            int companyId, int userId, SqlTransaction trn)
        {
            if (days <= 0 || leaveTypeId <= 0) return;

            SqlParameter[] prm =
            {
                new SqlParameter("@EmployeeID", SqlDbType.Int) { Value = employeeId },
                new SqlParameter("@LeaveTypeID", SqlDbType.Int) { Value = leaveTypeId },
                new SqlParameter("@Year", SqlDbType.Int) { Value = year },
                new SqlParameter("@Days", SqlDbType.Decimal) { Value = days },
                new SqlParameter("@CompanyID", SqlDbType.Int) { Value = companyId },
                new SqlParameter("@UserID", SqlDbType.Int) { Value = userId },
            };
            clsSQL sql = new clsSQL();
            sql.ExecuteNonQueryStatement(@"
UPDATE tbl_LeaveBalance SET
  PendingDays = CASE WHEN ISNULL(PendingDays,0) >= @Days THEN ISNULL(PendingDays,0) - @Days ELSE 0 END,
  ModificationUserID = @UserID,
  ModificationDate = GETDATE()
WHERE EmployeeID=@EmployeeID AND LeaveTypeID=@LeaveTypeID AND Year=@Year AND CompanyID=@CompanyID",
                sql.CreateDataBaseConnectionString(companyId), prm, trn);
        }

        decimal GetRemainingLeaveDays(int employeeId, int leaveTypeId, int year, int companyId, SqlTransaction trn)
        {
            clsSQL sql = new clsSQL();
            SqlParameter[] prm =
            {
                new SqlParameter("@EmployeeID", SqlDbType.Int) { Value = employeeId },
                new SqlParameter("@LeaveTypeID", SqlDbType.Int) { Value = leaveTypeId },
                new SqlParameter("@Year", SqlDbType.Int) { Value = year },
                new SqlParameter("@CompanyID", SqlDbType.Int) { Value = companyId },
            };
            object val = sql.ExecuteScalar(@"
SELECT ISNULL(EntitledDays,0) - ISNULL(UsedDays,0) - ISNULL(PendingDays,0)
FROM tbl_LeaveBalance
WHERE EmployeeID=@EmployeeID AND LeaveTypeID=@LeaveTypeID AND Year=@Year AND CompanyID=@CompanyID",
                prm, sql.CreateDataBaseConnectionString(companyId), trn);
            return Simulate.Decimal(val);
        }

        void EnsureLeaveBalanceRow(int employeeId, int leaveTypeId, int year, int companyId, int userId, SqlTransaction trn)
        {
            clsSQL sql = new clsSQL();
            SqlParameter[] check =
            {
                new SqlParameter("@EmployeeID", SqlDbType.Int) { Value = employeeId },
                new SqlParameter("@LeaveTypeID", SqlDbType.Int) { Value = leaveTypeId },
                new SqlParameter("@Year", SqlDbType.Int) { Value = year },
                new SqlParameter("@CompanyID", SqlDbType.Int) { Value = companyId },
            };
            object exists = sql.ExecuteScalar(@"
SELECT TOP 1 ID FROM tbl_LeaveBalance
WHERE EmployeeID=@EmployeeID AND LeaveTypeID=@LeaveTypeID AND Year=@Year AND CompanyID=@CompanyID",
                check, sql.CreateDataBaseConnectionString(companyId), trn);
            if (Simulate.Integer32(exists) > 0) return;

            UpsertLeaveBalance(employeeId, leaveTypeId, year, 0, 0, 0, companyId, userId, trn);
        }

        decimal ResolveEmployeeDailyRate(int employeeId, int companyId, SqlTransaction trn)
        {
            clsSQL sql = new clsSQL();
            SqlParameter[] prm =
            {
                new SqlParameter("@EmployeeID", SqlDbType.Int) { Value = employeeId },
                new SqlParameter("@CompanyID", SqlDbType.Int) { Value = companyId },
            };
            object val = sql.ExecuteScalar(@"
SELECT TOP 1 ISNULL(BasicSalary,0)
FROM tbl_EmployeeContract
WHERE EmployeeID=@EmployeeID AND CompanyID=@CompanyID AND ISNULL(IsActive,0)=1
ORDER BY ID DESC",
                prm, sql.CreateDataBaseConnectionString(companyId), trn);
            decimal basic = Simulate.Decimal(val);
            return basic > 0 ? basic / 30m : 0m;
        }

        /// <summary>
        /// Posts/approves a leave request: DocumentStatus=Posted, updates UsedDays, writes attendance StatusID=3 (Leave)
        /// for each day in range excluding holidays when a calendar exists for that year.
        /// </summary>
        public bool ApproveLeaveRequest(string documentGuid, int userId, int companyId, SqlTransaction trn = null)
        {
            clsSQL sql = new clsSQL();
            SqlParameter[] sel =
            {
                new SqlParameter("@Guid", SqlDbType.UniqueIdentifier) { Value = Simulate.Guid(documentGuid) },
                new SqlParameter("@CompanyID", SqlDbType.Int) { Value = companyId },
            };

            DataTable dt = sql.ExecuteQueryStatement(@"
SELECT ID, EmployeeID, LeaveTypeID, FromDate, ToDate, ISNULL(Days,0) AS Days, ISNULL(DocumentStatus,0) AS DocumentStatus
FROM tbl_LeaveRequest
WHERE Guid = @Guid AND CompanyID = @CompanyID",
                sql.CreateDataBaseConnectionString(companyId), sel, trn);

            if (dt == null || dt.Rows.Count == 0) return false;

            DataRow row = dt.Rows[0];
            if (Simulate.Integer32(row["DocumentStatus"]) == (int)DocumentStatus.Posted)
                return true;

            int employeeId = Simulate.Integer32(row["EmployeeID"]);
            int leaveTypeId = Simulate.Integer32(row["LeaveTypeID"]);
            DateTime fromDate = Convert.ToDateTime(row["FromDate"]).Date;
            DateTime toDate = Convert.ToDateTime(row["ToDate"]).Date;
            decimal days = Simulate.Decimal(row["Days"]);
            if (days <= 0)
                days = (decimal)(toDate - fromDate).TotalDays + 1;

            HashSet<DateTime> holidayDates = LoadHolidayDates(fromDate.Year, companyId, trn);
            if (toDate.Year != fromDate.Year)
            {
                foreach (var d in LoadHolidayDates(toDate.Year, companyId, trn))
                    holidayDates.Add(d);
            }

            int chargeableDays = 0;
            for (DateTime d = fromDate; d <= toDate; d = d.AddDays(1))
            {
                if (holidayDates.Contains(d)) continue;
                chargeableDays++;
                UpsertAttendanceLeaveDay(employeeId, d, companyId, userId, trn);
            }

            if (chargeableDays > 0)
                days = chargeableDays;

            SqlParameter[] upd =
            {
                new SqlParameter("@Guid", SqlDbType.UniqueIdentifier) { Value = Simulate.Guid(documentGuid) },
                new SqlParameter("@CompanyID", SqlDbType.Int) { Value = companyId },
                new SqlParameter("@UserId", SqlDbType.Int) { Value = userId },
                new SqlParameter("@Days", SqlDbType.Decimal) { Value = days },
            };
            sql.ExecuteNonQueryStatement(@"
UPDATE tbl_LeaveRequest SET
  DocumentStatus = 2,
  Days = @Days,
  PostedDate = GETDATE(),
  PostedByUserId = @UserId,
  ModificationUserID = @UserId,
  ModificationDate = GETDATE()
WHERE Guid = @Guid AND CompanyID = @CompanyID",
                sql.CreateDataBaseConnectionString(companyId), upd, trn);

            // Move pending → used (or just add used)
            SqlParameter[] bal =
            {
                new SqlParameter("@EmployeeID", SqlDbType.Int) { Value = employeeId },
                new SqlParameter("@LeaveTypeID", SqlDbType.Int) { Value = leaveTypeId },
                new SqlParameter("@Year", SqlDbType.Int) { Value = fromDate.Year },
                new SqlParameter("@Days", SqlDbType.Decimal) { Value = days },
                new SqlParameter("@CompanyID", SqlDbType.Int) { Value = companyId },
                new SqlParameter("@UserID", SqlDbType.Int) { Value = userId },
            };
            sql.ExecuteNonQueryStatement(@"
IF EXISTS (SELECT 1 FROM tbl_LeaveBalance
           WHERE EmployeeID=@EmployeeID AND LeaveTypeID=@LeaveTypeID AND Year=@Year AND CompanyID=@CompanyID)
BEGIN
  UPDATE tbl_LeaveBalance SET
    UsedDays = ISNULL(UsedDays,0) + @Days,
    PendingDays = CASE WHEN ISNULL(PendingDays,0) >= @Days THEN ISNULL(PendingDays,0) - @Days ELSE 0 END,
    ModificationUserID = @UserID,
    ModificationDate = GETDATE()
  WHERE EmployeeID=@EmployeeID AND LeaveTypeID=@LeaveTypeID AND Year=@Year AND CompanyID=@CompanyID;
END
ELSE
BEGIN
  INSERT INTO tbl_LeaveBalance (EmployeeID, LeaveTypeID, Year, EntitledDays, UsedDays, PendingDays, CompanyID, CreationUserID, CreationDate)
  VALUES (@EmployeeID, @LeaveTypeID, @Year, 0, @Days, 0, @CompanyID, @UserID, GETDATE());
END",
                sql.CreateDataBaseConnectionString(companyId), bal, trn);

            return true;
        }

        HashSet<DateTime> LoadHolidayDates(int year, int companyId, SqlTransaction trn)
        {
            var set = new HashSet<DateTime>();
            clsSQL sql = new clsSQL();
            SqlParameter[] prm =
            {
                new SqlParameter("@Year", SqlDbType.Int) { Value = year },
                new SqlParameter("@CompanyID", SqlDbType.Int) { Value = companyId },
            };
            DataTable dt = sql.ExecuteQueryStatement(@"
SELECT h.HolidayDate
FROM tbl_Holiday h
INNER JOIN tbl_HolidayCalendar c ON c.ID = h.CalendarID AND c.CompanyID = h.CompanyID
WHERE c.Year = @Year AND h.CompanyID = @CompanyID",
                sql.CreateDataBaseConnectionString(companyId), prm, trn);

            if (dt == null) return set;
            foreach (DataRow r in dt.Rows)
                set.Add(Convert.ToDateTime(r["HolidayDate"]).Date);
            return set;
        }

        void UpsertAttendanceLeaveDay(int employeeId, DateTime workDate, int companyId, int userId, SqlTransaction trn)
        {
            clsSQL sql = new clsSQL();
            SqlParameter[] prm =
            {
                new SqlParameter("@EmployeeID", SqlDbType.Int) { Value = employeeId },
                new SqlParameter("@WorkDate", SqlDbType.DateTime) { Value = workDate.Date },
                new SqlParameter("@CompanyID", SqlDbType.Int) { Value = companyId },
                new SqlParameter("@UserID", SqlDbType.Int) { Value = userId },
            };
            // StatusID 3 = Leave
            sql.ExecuteNonQueryStatement(@"
IF EXISTS (SELECT 1 FROM tbl_AttendanceDay WHERE EmployeeID=@EmployeeID AND CAST(WorkDate AS DATE)=CAST(@WorkDate AS DATE) AND CompanyID=@CompanyID)
BEGIN
  UPDATE tbl_AttendanceDay SET
    StatusID = 3,
    ModificationUserID = @UserID,
    ModificationDate = GETDATE()
  WHERE EmployeeID=@EmployeeID AND CAST(WorkDate AS DATE)=CAST(@WorkDate AS DATE) AND CompanyID=@CompanyID;
END
ELSE
BEGIN
  INSERT INTO tbl_AttendanceDay
    (EmployeeID, WorkDate, ShiftID, WorkedMinutes, LateMinutes, EarlyLeaveMinutes, OvertimeMinutes,
     StatusID, CompanyID, CreationUserID, CreationDate)
  VALUES
    (@EmployeeID, @WorkDate, 0, 0, 0, 0, 0, 3, @CompanyID, @UserID, GETDATE());
END",
                sql.CreateDataBaseConnectionString(companyId), prm, trn);
        }
    }
}
