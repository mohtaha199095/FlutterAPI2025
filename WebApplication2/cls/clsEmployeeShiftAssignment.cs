using System;
using System.Data;
using Microsoft.Data.SqlClient;

namespace WebApplication2.cls
{
    public class clsEmployeeShiftAssignment
    {
        // ==========================================================
        // SELECT ALL BY EMPLOYEE
        // ==========================================================
        public DataTable SelectAll(int EmployeeID, int CompanyID)
        {
            try
            {
                SqlParameter[] prm =
                {
                    new SqlParameter("@EmployeeID", SqlDbType.Int) { Value = EmployeeID },
                    new SqlParameter("@CompanyID", SqlDbType.Int) { Value = CompanyID },
                };

                clsSQL cls = new clsSQL();

                string sql = @"
                    SELECT *
                    FROM tbl_EmployeeShiftAssignment
                    WHERE (EmployeeID = @EmployeeID OR @EmployeeID = 0)
                      AND (CompanyID = @CompanyID OR @CompanyID = 0)
                    ORDER BY EmployeeID, WeekDay, StartDate
                ";

                return cls.ExecuteQueryStatement(sql, cls.CreateDataBaseConnectionString(CompanyID), prm);
            }
            catch
            {
                throw;
            }
        }

        // ==========================================================
        // SELECT BY ID
        // ==========================================================
        public DataTable SelectByID(int ID,int employeeID, int CompanyID)
        {
            try
            {
                SqlParameter[] prm =
                {
                    new SqlParameter("@ID", SqlDbType.Int) { Value = ID },
                    new SqlParameter("@employeeID", SqlDbType.Int) { Value = employeeID },
                };
                  
                clsSQL cls = new clsSQL();

                string sql = @"
                    SELECT *
                    FROM tbl_EmployeeShiftAssignment
                    WHERE (ID = @ID or  @ID= 0 )and (employeeID=@employeeID or @employeeID=0)
                ";

                return cls.ExecuteQueryStatement(sql, cls.CreateDataBaseConnectionString(CompanyID), prm);
            }
            catch
            {
                throw;
            }
        }

        // ==========================================================
        // INSERT
        // ==========================================================
        public int Insert(
            int EmployeeID,
            int ShiftID,
            int WeekDay,
            string StartDate,
            string EndDate,
            bool IsActive,
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
                    new SqlParameter("@ShiftID", SqlDbType.Int) { Value = ShiftID },
                    new SqlParameter("@WeekDay", SqlDbType.Int) { Value = WeekDay },
                    new SqlParameter("@StartDate", SqlDbType.DateTime) { Value = Simulate.StringToDate(StartDate) },
                    new SqlParameter("@EndDate", SqlDbType.DateTime) { Value = Simulate.StringToDate(EndDate) },
                    new SqlParameter("@IsActive", SqlDbType.Bit) { Value = IsActive },
                    new SqlParameter("@CompanyID", SqlDbType.Int) { Value = CompanyID },
                    new SqlParameter("@CreationUserID", SqlDbType.Int) { Value = CreationUserID },
                    new SqlParameter("@CreationDate", SqlDbType.DateTime) { Value = DateTime.Now },
                };

                string sql = @"
                    INSERT INTO tbl_EmployeeShiftAssignment
                    (EmployeeID, ShiftID, WeekDay, StartDate, EndDate,
                     IsActive, CompanyID, CreationUserID, CreationDate)
                    OUTPUT INSERTED.ID
                    VALUES
                    (@EmployeeID, @ShiftID, @WeekDay, @StartDate, @EndDate,
                     @IsActive, @CompanyID, @CreationUserID, @CreationDate)
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
        // UPDATE
        // ==========================================================
        public int Update(
            int ID,
            int EmployeeID,
            int ShiftID,
            int WeekDay,
            string StartDate,
            string EndDate,
            bool IsActive,
            int CompanyID,
            int ModificationUserID,
            SqlTransaction trn = null
        )
        {
            try
            {
                SqlParameter[] prm =
                {
                    new SqlParameter("@ID", SqlDbType.Int) { Value = ID },
                    new SqlParameter("@EmployeeID", SqlDbType.Int) { Value = EmployeeID },
                    new SqlParameter("@ShiftID", SqlDbType.Int) { Value = ShiftID },
                    new SqlParameter("@WeekDay", SqlDbType.Int) { Value = WeekDay },
                    new SqlParameter("@StartDate", SqlDbType.DateTime) { Value = Simulate.StringToDate(StartDate) },
                    new SqlParameter("@EndDate", SqlDbType.DateTime) { Value = Simulate.StringToDate(EndDate) },
                    new SqlParameter("@IsActive", SqlDbType.Bit) { Value = IsActive },
                    new SqlParameter("@ModificationUserID", SqlDbType.Int) { Value = ModificationUserID },
                    new SqlParameter("@ModificationDate", SqlDbType.DateTime) { Value = DateTime.Now },
                };

                string sql = @"
                    UPDATE tbl_EmployeeShiftAssignment SET
                        EmployeeID = @EmployeeID,
                        ShiftID = @ShiftID,
                        WeekDay = @WeekDay,
                        StartDate = @StartDate,
                        EndDate = @EndDate,
                        IsActive = @IsActive,
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
        // DELETE
        // ==========================================================
        public int Delete(int ID, int CompanyID)
        {
            try
            {
                SqlParameter[] prm =
                {
                    new SqlParameter("@ID", SqlDbType.Int) { Value = ID },
                };

                clsSQL cls = new clsSQL();

                string sql = @"
                    DELETE FROM tbl_EmployeeShiftAssignment
                    WHERE ID = @ID
                ";

                return cls.ExecuteNonQueryStatement(sql, cls.CreateDataBaseConnectionString(CompanyID), prm);
            }
            catch
            {
                throw;
            }
        }
    }
}
