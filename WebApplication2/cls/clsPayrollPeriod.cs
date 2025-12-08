using System;
using System.Data;
using Microsoft.Data.SqlClient;

namespace WebApplication2.cls
{
    public class clsPayrollPeriod
    {
        // ===========================================================
        // SELECT
        // ===========================================================
        public void GetPeriodDates(int periodId, out DateTime startDate, out DateTime endDate, int companyId)
        {
            startDate = DateTime.MinValue;
            endDate = DateTime.MinValue;

            DataTable dt = SelectPayrollPeriod(periodId, "", -1, companyId);

            if (dt.Rows.Count == 0)
                throw new Exception("Invalid Payroll Period.");

            startDate = Simulate.StringToDate(dt.Rows[0]["StartDate"]);
            endDate = Simulate.StringToDate(dt.Rows[0]["EndDate"]);
        }
        public DataTable SelectPayrollPeriod(int ID, string PeriodAName, int IsClosed, int CompanyID)
        {
            try
            {
                SqlParameter[] prm =
                {
                    new SqlParameter("@ID", SqlDbType.Int) { Value = ID },
                    new SqlParameter("@PeriodAName", SqlDbType.NVarChar, -1) { Value = PeriodAName },
                    new SqlParameter("@IsClosed", SqlDbType.Int) { Value = IsClosed }, // -1 means ignore
                    new SqlParameter("@CompanyID", SqlDbType.Int) { Value = CompanyID },
                };

                clsSQL cls = new clsSQL();

                DataTable dt = cls.ExecuteQueryStatement(@"
                    SELECT * 
                    FROM tbl_PayrollPeriod 
                    WHERE (ID = @ID OR @ID = 0)
                      AND (PeriodAName = @PeriodAName OR @PeriodAName = '')
                      AND (IsClosed = @IsClosed OR @IsClosed = -1)
                      AND (CompanyID = @CompanyID OR @CompanyID = 0)
                ", cls.CreateDataBaseConnectionString(CompanyID), prm);

                return dt;
            }
            catch
            {
                throw;
            }
        }

        // ===========================================================
        // DELETE
        // ===========================================================
        public bool DeletePayrollPeriod(int ID, int CompanyID)
        {
            try
            {
                SqlParameter[] prm =
                {
                    new SqlParameter("@ID", SqlDbType.Int) { Value = ID }
                };

                clsSQL cls = new clsSQL();

                int a = cls.ExecuteNonQueryStatement(@"
                    DELETE FROM tbl_PayrollPeriod
                    WHERE ID = @ID
                ", cls.CreateDataBaseConnectionString(CompanyID), prm);

                return true;
            }
            catch
            {
                throw;
            }
        }

        // ===========================================================
        // INSERT
        // ===========================================================
        public int InsertPayrollPeriod(
            string PeriodAName, string PeriodEName,
            DateTime StartDate,
            DateTime EndDate,
            bool IsClosed,
            int CompanyID,
            int CreationUserID,
            SqlTransaction trn = null
        )
        {
            try
            {
                SqlParameter[] prm =
                {
                    new SqlParameter("@PeriodAName", SqlDbType.NVarChar, -1) { Value = PeriodAName },
                     new SqlParameter("@PeriodEName", SqlDbType.NVarChar, -1) { Value = PeriodEName },
                    new SqlParameter("@StartDate", SqlDbType.DateTime) { Value = StartDate },
                    new SqlParameter("@EndDate", SqlDbType.DateTime) { Value = EndDate },
                    new SqlParameter("@IsClosed", SqlDbType.Bit) { Value = IsClosed },

                    new SqlParameter("@CompanyID", SqlDbType.Int) { Value = CompanyID },
                    new SqlParameter("@CreationUserID", SqlDbType.Int) { Value = CreationUserID },
                    new SqlParameter("@CreationDate", SqlDbType.DateTime) { Value = DateTime.Now },
                };

                string sql = @"
                    INSERT INTO tbl_PayrollPeriod
                    (PeriodAName,PeriodEName, StartDate, EndDate, IsClosed,
                     CompanyID, CreationUserID, CreationDate)
                    OUTPUT INSERTED.ID
                    VALUES
                    (@PeriodAName,@PeriodEName, @StartDate, @EndDate, @IsClosed,
                     @CompanyID, @CreationUserID, @CreationDate)
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

        // ===========================================================
        // UPDATE
        // ===========================================================
        public int UpdatePayrollPeriod(
            int ID,
            string PeriodAName, string PeriodEName,
            DateTime StartDate,
            DateTime EndDate,
            bool IsClosed,
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
                     new SqlParameter("@PeriodAName", SqlDbType.NVarChar, -1) { Value = PeriodAName },
                     new SqlParameter("@PeriodEName", SqlDbType.NVarChar, -1) { Value = PeriodEName },
                    new SqlParameter("@StartDate", SqlDbType.DateTime) { Value = StartDate },
                    new SqlParameter("@EndDate", SqlDbType.DateTime) { Value = EndDate },
                    new SqlParameter("@IsClosed", SqlDbType.Bit) { Value = IsClosed },

                    new SqlParameter("@ModificationUserID", SqlDbType.Int) { Value = ModificationUserID },
                    new SqlParameter("@ModificationDate", SqlDbType.DateTime) { Value = DateTime.Now },
                };

                string sql = @"
                    UPDATE tbl_PayrollPeriod SET 
                        PeriodAName = @PeriodAName,
                        PeriodEName = @PeriodEName,
                        StartDate = @StartDate,
                        EndDate = @EndDate,
                        IsClosed = @IsClosed,
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
    }
}
