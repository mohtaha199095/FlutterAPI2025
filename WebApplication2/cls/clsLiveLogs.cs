using System;
using System.Data;
using Microsoft.Data.SqlClient;

namespace WebApplication2.cls
{
    public class clsLiveLogs
    {
        /// <summary>
        /// Return latest live logs for a company.
        /// You can control how many rows using TopN (default 200 from controller).
        /// Optional DateFrom / DateTo (as yyyy-MM-dd).
        /// </summary>
        public DataTable SelectLiveLogs(
            int companyId,
            int topN,
            string dateFrom,
            string dateTo
        )
        {
            try
            {
                // if you want to filter by date, handle nulls here
                SqlParameter[] prm =
                {
                    new SqlParameter("@CompanyID", companyId),
                    new SqlParameter("@TopN", topN),
                    new SqlParameter("@DateFrom",
                        string.IsNullOrWhiteSpace(dateFrom) ? (object)DBNull.Value : dateFrom),
                    new SqlParameter("@DateTo",
                        string.IsNullOrWhiteSpace(dateTo) ? (object)DBNull.Value : dateTo),
                };

                string sql = @"
                    SELECT TOP (@TopN)
                        L.ID,
                        L.EmployeeID,
                        ISNULL(E.AName, E.EName) AS EmployeeName,
                        L.MachineID,
                        M.MachineName,
                        L.PunchTime,
                        L.PunchType,
                        L.CompanyID
                    FROM tbl_LiveLogs L
                    LEFT JOIN tbl_Employees E
                        ON L.EmployeeID = E.ID AND L.CompanyID = E.CompanyID
                    LEFT JOIN tbl_AttendanceMachines M
                        ON L.MachineID = M.ID AND L.CompanyID = M.CompanyID
                    WHERE L.CompanyID = @CompanyID
                      AND (@DateFrom IS NULL OR CONVERT(date, L.PunchTime) >= @DateFrom)
                      AND (@DateTo   IS NULL OR CONVERT(date, L.PunchTime) <= @DateTo)
                    ORDER BY L.PunchTime DESC
                ";

                clsSQL cls = new clsSQL();
                // Use the same method you use everywhere to get a DataTable:
                // adjust method name if yours is different (GetDataTable / SelectData / ExecuteDataTable...)
                DataTable dt = cls.ExecuteQueryStatement(sql, cls.CreateDataBaseConnectionString(companyId),prm);

                return dt;
            }
            catch (Exception)
            {
                throw;
            }
        }
    }
}
