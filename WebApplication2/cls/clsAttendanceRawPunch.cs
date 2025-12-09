using System;
using System.Data;
using Microsoft.Data.SqlClient;

namespace WebApplication2.cls
{
    public class clsAttendanceRawPunch
    {
        // ==========================================================
        // SELECT RAW PUNCHES
        // ==========================================================
        public DataTable SelectAttendanceRawPunch(
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
                    new SqlParameter("@EmployeeID", EmployeeID),
                    new SqlParameter("@DateFrom", DateFrom),
                    new SqlParameter("@DateTo", DateTo)
                };

                string sql = @"
                    SELECT *
                    FROM tbl_AttendanceRawPunch
                    WHERE (EmployeeID = @EmployeeID OR @EmployeeID = 0)
                      AND CONVERT(date, PunchTime) BETWEEN @DateFrom AND @DateTo
                      AND CompanyID = " + CompanyID + @"
                    ORDER BY PunchTime
                ";

                clsSQL cls = new clsSQL();

                return cls.ExecuteQueryStatement(sql, cls.CreateDataBaseConnectionString(CompanyID), prm);
            }
            catch
            {
                throw;
            }
        }

        // ==========================================================
        // DELETE PUNCH
        // ==========================================================
        public bool DeleteFromAttendanceRawPunch(int ID, int CompanyID)
        {
            try
            {
                SqlParameter[] prm =
                {
                    new SqlParameter("@ID", ID)
                };

                string sql = @"
                    DELETE FROM tbl_AttendanceRawPunch
                    WHERE ID = @ID
                      AND CompanyID = " + CompanyID;

                clsSQL cls = new clsSQL();

                cls.ExecuteNonQueryStatement(sql, cls.CreateDataBaseConnectionString(CompanyID), prm);

                return true;
            }
            catch
            {
                throw;
            }
        }
    }
}
