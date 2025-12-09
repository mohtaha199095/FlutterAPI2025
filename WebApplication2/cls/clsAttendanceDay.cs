using System;
using System.Data;
using Microsoft.Data.SqlClient;

namespace WebApplication2.cls
{
    public class clsAttendanceDay
    {
        // ==========================================================
        // SELECT ATTENDANCE DAYS
        // ==========================================================
        public DataTable SelectAttendanceDays(
            int EmployeeID,
            string DateFrom,
            string DateTo,
            int CompanyID
        )
        {
            try
            {
                SqlParameter[] prm =
                {
                    new SqlParameter("@EmployeeID", SqlDbType.Int) { Value = EmployeeID },
                    new SqlParameter("@DateFrom", SqlDbType.Date) { Value = DateFrom },
                    new SqlParameter("@DateTo", SqlDbType.Date) { Value = DateTo },
                };

                clsSQL cls = new clsSQL();

                string sql = @"
                    SELECT *
                    FROM tbl_AttendanceDay
                    WHERE (EmployeeID = @EmployeeID OR @EmployeeID = 0)
                      AND WorkDate BETWEEN @DateFrom AND @DateTo
                    ORDER BY WorkDate ASC
                ";

                return cls.ExecuteQueryStatement(
                    sql,
                    cls.CreateDataBaseConnectionString(CompanyID),
                    prm
                );
            }
            catch
            {
                throw;
            }
        }

        // ==========================================================
        // DELETE ATTENDANCE DAY
        // ==========================================================
        public bool DeleteAttendanceDay(int ID, int CompanyID)
        {
            try
            {
                SqlParameter[] prm =
                {
                    new SqlParameter("@ID", SqlDbType.Int) { Value = ID }
                };

                clsSQL cls = new clsSQL();

                string sql = @"
                    DELETE FROM tbl_AttendanceDay
                    WHERE ID = @ID
                ";

                cls.ExecuteNonQueryStatement(
                    sql,
                    cls.CreateDataBaseConnectionString(CompanyID),
                    prm
                );

                return true;
            }
            catch
            {
                throw;
            }
        }

        // ==========================================================
        // INSERT ATTENDANCE DAY
        // Used after calculation engine
        // ==========================================================
        public int InsertAttendanceDay(
            int EmployeeID,
            DateTime WorkDate,
            int ShiftID,
            DateTime? FirstIn,
            DateTime? LastOut,
            int WorkedMinutes,
            int LateMinutes,
            int EarlyLeaveMinutes,
            int OvertimeMinutes,
            int StatusID,
            int CompanyID,
            int CreationUserID,
            SqlTransaction trn = null
        )
        {
            try
            {
                SqlParameter[] prm =
                {
                    new SqlParameter("@EmployeeID", SqlDbType.Int) { Value = EmployeeID },
                    new SqlParameter("@WorkDate", SqlDbType.DateTime) { Value = WorkDate },
                    new SqlParameter("@ShiftID", SqlDbType.Int) { Value = ShiftID },

                    new SqlParameter("@FirstIn", SqlDbType.DateTime) { Value = (object?)FirstIn ?? DBNull.Value },
                    new SqlParameter("@LastOut", SqlDbType.DateTime) { Value = (object?)LastOut ?? DBNull.Value },

                    new SqlParameter("@WorkedMinutes", SqlDbType.Int) { Value = WorkedMinutes },
                    new SqlParameter("@LateMinutes", SqlDbType.Int) { Value = LateMinutes },
                    new SqlParameter("@EarlyLeaveMinutes", SqlDbType.Int) { Value = EarlyLeaveMinutes },
                    new SqlParameter("@OvertimeMinutes", SqlDbType.Int) { Value = OvertimeMinutes },

                    new SqlParameter("@StatusID", SqlDbType.Int) { Value = StatusID },

                    new SqlParameter("@CompanyID", SqlDbType.Int) { Value = CompanyID },
                    new SqlParameter("@CreationUserID", SqlDbType.Int) { Value = CreationUserID },
                    new SqlParameter("@CreationDate", SqlDbType.DateTime) { Value = DateTime.Now },
                };

                string sql = @"
                    INSERT INTO tbl_AttendanceDay
                    (EmployeeID, WorkDate, ShiftID, FirstIn, LastOut,
                     WorkedMinutes, LateMinutes, EarlyLeaveMinutes, OvertimeMinutes,
                     StatusID, CompanyID, CreationUserID, CreationDate)
                    OUTPUT INSERTED.ID
                    VALUES
                    (@EmployeeID, @WorkDate, @ShiftID, @FirstIn, @LastOut,
                     @WorkedMinutes, @LateMinutes, @EarlyLeaveMinutes, @OvertimeMinutes,
                     @StatusID, @CompanyID, @CreationUserID, @CreationDate)
                ";

                clsSQL cls = new clsSQL();

                if (trn == null)
                {
                    return Simulate.Integer32(
                        cls.ExecuteScalar(sql, prm, cls.CreateDataBaseConnectionString(CompanyID))
                    );
                }
                else
                {
                    return Simulate.Integer32(
                        cls.ExecuteScalar(sql, prm, cls.CreateDataBaseConnectionString(CompanyID), trn)
                    );
                }
            }
            catch
            {
                throw;
            }
        }

        // ==========================================================
        // UPDATE ATTENDANCE DAY
        // ==========================================================
        public int UpdateAttendanceDay(
            int ID,
            DateTime? FirstIn,
            DateTime? LastOut,
            int WorkedMinutes,
            int LateMinutes,
            int EarlyLeaveMinutes,
            int OvertimeMinutes,
            int StatusID,
            int ModificationUserID,
            int CompanyID,
            SqlTransaction trn = null
        )
        {
            try
            {
                SqlParameter[] prm =
                {
                    new SqlParameter("@ID", SqlDbType.Int) { Value = ID },

                    new SqlParameter("@FirstIn", SqlDbType.DateTime) { Value = (object?)FirstIn ?? DBNull.Value },
                    new SqlParameter("@LastOut", SqlDbType.DateTime) { Value = (object?)LastOut ?? DBNull.Value },

                    new SqlParameter("@WorkedMinutes", SqlDbType.Int) { Value = WorkedMinutes },
                    new SqlParameter("@LateMinutes", SqlDbType.Int) { Value = LateMinutes },
                    new SqlParameter("@EarlyLeaveMinutes", SqlDbType.Int) { Value = EarlyLeaveMinutes },
                    new SqlParameter("@OvertimeMinutes", SqlDbType.Int) { Value = OvertimeMinutes },

                    new SqlParameter("@StatusID", SqlDbType.Int) { Value = StatusID },

                    new SqlParameter("@ModificationUserID", SqlDbType.Int) { Value = ModificationUserID },
                    new SqlParameter("@ModificationDate", SqlDbType.DateTime) { Value = DateTime.Now },
                };

                string sql = @"
                    UPDATE tbl_AttendanceDay SET
                        FirstIn = @FirstIn,
                        LastOut = @LastOut,
                        WorkedMinutes = @WorkedMinutes,
                        LateMinutes = @LateMinutes,
                        EarlyLeaveMinutes = @EarlyLeaveMinutes,
                        OvertimeMinutes = @OvertimeMinutes,
                        StatusID = @StatusID,
                        ModificationUserID = @ModificationUserID,
                        ModificationDate = @ModificationDate
                    WHERE ID = @ID
                ";

                clsSQL cls = new clsSQL();
                int A = 0;

                if (trn == null)
                {
                    A = cls.ExecuteNonQueryStatement(sql, cls.CreateDataBaseConnectionString(CompanyID), prm);
                }
                else
                {
                    A = cls.ExecuteNonQueryStatement(sql, cls.CreateDataBaseConnectionString(CompanyID), prm, trn);
                }

                return A;
            }
            catch
            {
                throw;
            }
        }

        // ==========================================================
        // RECALCULATE FULL ATTENDANCE FOR RANGE
        // (Will call shift resolver + punch resolver)
        // ==========================================================
        public bool RecalculateAttendance(
            int EmployeeID,
            string DateFrom,
            string DateTo,
            int CompanyID,
            int UserID
        )
        {
            try
            {
                // Example — This will be replaced when you want full engine:
                // 1) Load raw punches
                // 2) Load shift
                // 3) Compute day results
                // 4) Insert/Update tbl_AttendanceDay

                return true;
            }
            catch
            {
                throw;
            }
        }
    }
}
