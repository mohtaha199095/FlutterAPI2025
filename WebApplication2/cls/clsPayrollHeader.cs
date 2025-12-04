using System;
using System.Data;
using Microsoft.Data.SqlClient;

namespace WebApplication2.cls
{
    public class clsPayrollHeader
    {
        // ===========================================================
        // SELECT
        // ===========================================================
        public DataTable SelectPayrollHeader(
            int ID,
            int PayrollPeriodID,
            int EmployeeID,
            int Status,
            int CompanyID,
            SqlTransaction trn = null)
        {
            try
            {
                SqlParameter[] prm =
                {
                    new SqlParameter("@ID", SqlDbType.Int) { Value = ID },
                    new SqlParameter("@PayrollPeriodID", SqlDbType.Int) { Value = PayrollPeriodID },
                    new SqlParameter("@EmployeeID", SqlDbType.Int) { Value = EmployeeID },
                    new SqlParameter("@Status", SqlDbType.Int) { Value = Status },
                    new SqlParameter("@CompanyID", SqlDbType.Int) { Value = CompanyID },
                };

                clsSQL cls = new clsSQL();
                DataTable dt = cls.ExecuteQueryStatement(@"
                    SELECT *
                    FROM tbl_PayrollHeader
                    WHERE (ID = @ID OR @ID = 0)
                      AND (PayrollPeriodID = @PayrollPeriodID OR @PayrollPeriodID = 0)
                      AND (EmployeeID = @EmployeeID OR @EmployeeID = 0)
                      AND (Status = @Status OR @Status = -1)
                      AND (CompanyID = @CompanyID OR @CompanyID = 0)
                ",
                cls.CreateDataBaseConnectionString(CompanyID),
                prm,
                trn);

                return dt;
            }
            catch { throw; }
        }

        // ===========================================================
        // DELETE
        // ===========================================================
        public bool DeletePayrollHeader(int ID, int CompanyID)
        {
            try
            {
                SqlParameter[] prm =
                {
                    new SqlParameter("@ID", SqlDbType.Int) { Value = ID }
                };

                clsSQL cls = new clsSQL();

                cls.ExecuteNonQueryStatement(@"
                    DELETE FROM tbl_PayrollHeader
                    WHERE ID = @ID
                ", cls.CreateDataBaseConnectionString(CompanyID), prm);

                return true;
            }
            catch { throw; }
        }

        // ===========================================================
        // INSERT
        // ===========================================================
        public int InsertPayrollHeader(
            int PayrollPeriodID,
            int EmployeeID,
            decimal BasicSalary,
            decimal TotalEarnings,
            decimal TotalDeductions,
            decimal NetSalary,
            int Status,                     // 1=Draft, 2=Approved, 3=Posted
            int CompanyID,
            int CreationUserID,
            SqlTransaction trn = null)
        {
            try
            {
                SqlParameter[] prm =
                {
                    new SqlParameter("@PayrollPeriodID", SqlDbType.Int) { Value = PayrollPeriodID },
                    new SqlParameter("@EmployeeID", SqlDbType.Int) { Value = EmployeeID },

                    new SqlParameter("@BasicSalary", SqlDbType.Decimal) { Value = BasicSalary },
                    new SqlParameter("@TotalEarnings", SqlDbType.Decimal) { Value = TotalEarnings },
                    new SqlParameter("@TotalDeductions", SqlDbType.Decimal) { Value = TotalDeductions },
                    new SqlParameter("@NetSalary", SqlDbType.Decimal) { Value = NetSalary },

                    new SqlParameter("@Status", SqlDbType.Int) { Value = Status },

                    new SqlParameter("@CompanyID", SqlDbType.Int) { Value = CompanyID },
                    new SqlParameter("@CreationUserID", SqlDbType.Int) { Value = CreationUserID },
                    new SqlParameter("@CreationDate", SqlDbType.DateTime) { Value = DateTime.Now },
                };

                string sql = @"
                    INSERT INTO tbl_PayrollHeader
                    (PayrollPeriodID, EmployeeID,
                     BasicSalary, TotalEarnings, TotalDeductions, NetSalary,
                     Status,
                     CompanyID, CreationUserID, CreationDate)
                    OUTPUT INSERTED.ID
                    VALUES
                    (@PayrollPeriodID, @EmployeeID,
                     @BasicSalary, @TotalEarnings, @TotalDeductions, @NetSalary,
                     @Status,
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
            catch { throw; }
        }


        // ===========================================================
        // UPDATE
        // ===========================================================
        public int UpdatePayrollHeader(
            int ID,
            int PayrollPeriodID,
            int EmployeeID,
            decimal BasicSalary,
            decimal TotalEarnings,
            decimal TotalDeductions,
            decimal NetSalary,
            int Status,
            int ModificationUserID,
            int CompanyID,
            SqlTransaction trn = null)
        {
            try
            {
                SqlParameter[] prm =
                {
                    new SqlParameter("@ID", SqlDbType.Int) { Value = ID },
                    new SqlParameter("@PayrollPeriodID", SqlDbType.Int) { Value = PayrollPeriodID },
                    new SqlParameter("@EmployeeID", SqlDbType.Int) { Value = EmployeeID },

                    new SqlParameter("@BasicSalary", SqlDbType.Decimal) { Value = BasicSalary },
                    new SqlParameter("@TotalEarnings", SqlDbType.Decimal) { Value = TotalEarnings },
                    new SqlParameter("@TotalDeductions", SqlDbType.Decimal) { Value = TotalDeductions },
                    new SqlParameter("@NetSalary", SqlDbType.Decimal) { Value = NetSalary },

                    new SqlParameter("@Status", SqlDbType.Int) { Value = Status },

                    new SqlParameter("@ModificationUserID", SqlDbType.Int) { Value = ModificationUserID },
                    new SqlParameter("@ModificationDate", SqlDbType.DateTime) { Value = DateTime.Now },
                };

                string sql = @"
                    UPDATE tbl_PayrollHeader SET
                        PayrollPeriodID = @PayrollPeriodID,
                        EmployeeID = @EmployeeID,
                        BasicSalary = @BasicSalary,
                        TotalEarnings = @TotalEarnings,
                        TotalDeductions = @TotalDeductions,
                        NetSalary = @NetSalary,
                        Status = @Status,
                        ModificationUserID = @ModificationUserID,
                        ModificationDate = @ModificationDate
                    WHERE ID = @ID
                ";

                clsSQL cls = new clsSQL();

                if (trn == null)
                {
                    return cls.ExecuteNonQueryStatement(sql, cls.CreateDataBaseConnectionString(CompanyID), prm);
                }
                else
                {
                    return cls.ExecuteNonQueryStatement(sql, cls.CreateDataBaseConnectionString(CompanyID), prm, trn);
                }
            }
            catch { throw; }
        }
    }
}
