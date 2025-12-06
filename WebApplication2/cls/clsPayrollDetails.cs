using System;
using System.Data;
using Microsoft.Data.SqlClient;

namespace WebApplication2.cls
{
    public class clsPayrollDetails
    {
        public DataTable SelectPayrollForPosting(int payrollPeriodID, int companyID, SqlTransaction trn = null)
        {
            try
            {
                string sql = @"
                      SELECT 
    tbl_EmployeeSalaryElements.EmployeeID,
    tbl_employee.AName AS EmployeeName,
   tbl_EmployeeSalaryElements.SalaryElementID AS ElementID,
   tbl_SalariesElements.AName AS ElementName,
 tbl_EmployeeSalaryElements.AssignedValue AS Amount,
   CASE WHEN tbl_SalariesElements.ElementTypeID = 2 THEN 1 ELSE 0 END AS IsDeduction,
    ISNULL(tbl_SalariesElements.CompanyDebitAccountID,0) AS AccountID
 FROM tbl_EmployeeSalaryElements
left join tbl_employee on tbl_EmployeeSalaryElements.EmployeeID= tbl_employee.ID
left join tbl_SalariesElements on tbl_EmployeeSalaryElements.SalaryElementID= tbl_SalariesElements.ID
WHERE --tbl_EmployeeSalaryElements.PayrollPeriodID = @PayrollPeriodID AND
  tbl_EmployeeSalaryElements.CompanyID = @CompanyID
ORDER BY tbl_employee.AName, tbl_SalariesElements.SortIndex
        ";

                clsSQL cls = new clsSQL();
                SqlConnection con;

                if (trn == null)
                {
                    con = new SqlConnection(cls.CreateDataBaseConnectionString(companyID));
                    con.Open();
                }
                else
                {
                    con = trn.Connection;
                }

                SqlCommand cmd = new SqlCommand(sql, con);
                if (trn != null)
                    cmd.Transaction = trn;

                cmd.Parameters.AddWithValue("@PayrollPeriodID", payrollPeriodID);
                cmd.Parameters.AddWithValue("@CompanyID", companyID);

                SqlDataAdapter da = new SqlDataAdapter(cmd);
                DataTable dt = new DataTable();
                da.Fill(dt);

                return dt;
            }
            catch (Exception ex)
            {
                throw new Exception("Error in SelectPayrollForPosting: " + ex.Message, ex);
            }
        }

        // ===========================================================
        // SELECT
        // ===========================================================
        public DataTable SelectPayrollDetails(
            int ID,
            int PayrollHeaderID,
            int SalaryElementID,
            int CompanyID,
            SqlTransaction trn = null)
        {
            try
            {
                SqlParameter[] prm =
                {
                    new SqlParameter("@ID", SqlDbType.Int) { Value = ID },
                    new SqlParameter("@PayrollHeaderID", SqlDbType.Int) { Value = PayrollHeaderID },
                    new SqlParameter("@SalaryElementID", SqlDbType.Int) { Value = SalaryElementID },
                    new SqlParameter("@CompanyID", SqlDbType.Int) { Value = CompanyID },
                };

                clsSQL cls = new clsSQL();

                DataTable dt = cls.ExecuteQueryStatement(@"
                    SELECT *
                    FROM tbl_PayrollDetails
                    WHERE (ID = @ID OR @ID = 0)
                      AND (PayrollHeaderID = @PayrollHeaderID OR @PayrollHeaderID = 0)
                      AND (SalaryElementID = @SalaryElementID OR @SalaryElementID = 0)
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
        public bool DeletePayrollDetails(int ID, int CompanyID)
        {
            try
            {
                SqlParameter[] prm =
                {
                    new SqlParameter("@ID", SqlDbType.Int) { Value = ID }
                };

                clsSQL cls = new clsSQL();

                cls.ExecuteNonQueryStatement(@"
                    DELETE FROM tbl_PayrollDetails
                    WHERE ID = @ID
                ",
                cls.CreateDataBaseConnectionString(CompanyID),
                prm);

                return true;
            }
            catch { throw; }
        }

        // ===========================================================
        // INSERT
        // ===========================================================
        public int InsertPayrollDetails(
            int PayrollHeaderID,
            int SalaryElementID,
            int ElementTypeID,
            int CalcTypeID,
            decimal AssignedValue,
            decimal CalculatedAmount,
            int CompanyID,
            int CreationUserID,
            SqlTransaction trn = null)
        {
            try
            {
                SqlParameter[] prm =
                {
                    new SqlParameter("@PayrollHeaderID", SqlDbType.Int) { Value = PayrollHeaderID },
                    new SqlParameter("@SalaryElementID", SqlDbType.Int) { Value = SalaryElementID },

                    new SqlParameter("@ElementTypeID", SqlDbType.Int) { Value = ElementTypeID },
                    new SqlParameter("@CalcTypeID", SqlDbType.Int) { Value = CalcTypeID },

                    new SqlParameter("@AssignedValue", SqlDbType.Decimal) { Value = AssignedValue },
                    new SqlParameter("@CalculatedAmount", SqlDbType.Decimal) { Value = CalculatedAmount },

                    new SqlParameter("@CompanyID", SqlDbType.Int) { Value = CompanyID },
                    new SqlParameter("@CreationUserID", SqlDbType.Int) { Value = CreationUserID },
                    new SqlParameter("@CreationDate", SqlDbType.DateTime) { Value = DateTime.Now }
                };

                string sql = @"
                    INSERT INTO tbl_PayrollDetails
                    (PayrollHeaderID, SalaryElementID, ElementTypeID, CalcTypeID,
                     AssignedValue, CalculatedAmount,
                     CompanyID, CreationUserID, CreationDate)
                    OUTPUT INSERTED.ID
                    VALUES
                    (@PayrollHeaderID, @SalaryElementID, @ElementTypeID, @CalcTypeID,
                     @AssignedValue, @CalculatedAmount,
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
        public int UpdatePayrollDetails(
            int ID,
            int PayrollHeaderID,
            int SalaryElementID,
            int ElementTypeID,
            int CalcTypeID,
            decimal AssignedValue,
            decimal CalculatedAmount,
            int ModificationUserID,
            int CompanyID,
            SqlTransaction trn = null)
        {
            try
            {
                SqlParameter[] prm =
                {
                    new SqlParameter("@ID", SqlDbType.Int) { Value = ID },

                    new SqlParameter("@PayrollHeaderID", SqlDbType.Int) { Value = PayrollHeaderID },
                    new SqlParameter("@SalaryElementID", SqlDbType.Int) { Value = SalaryElementID },

                    new SqlParameter("@ElementTypeID", SqlDbType.Int) { Value = ElementTypeID },
                    new SqlParameter("@CalcTypeID", SqlDbType.Int) { Value = CalcTypeID },

                    new SqlParameter("@AssignedValue", SqlDbType.Decimal) { Value = AssignedValue },
                    new SqlParameter("@CalculatedAmount", SqlDbType.Decimal) { Value = CalculatedAmount },

                    new SqlParameter("@ModificationUserID", SqlDbType.Int) { Value = ModificationUserID },
                    new SqlParameter("@ModificationDate", SqlDbType.DateTime) { Value = DateTime.Now },
                };

                string sql = @"
                    UPDATE tbl_PayrollDetails SET
                        PayrollHeaderID = @PayrollHeaderID,
                        SalaryElementID = @SalaryElementID,
                        ElementTypeID = @ElementTypeID,
                        CalcTypeID = @CalcTypeID,
                        AssignedValue = @AssignedValue,
                        CalculatedAmount = @CalculatedAmount,
                        ModificationUserID = @ModificationUserID,
                        ModificationDate = @ModificationDate
                    WHERE ID = @ID
                ";

                clsSQL cls = new clsSQL();

                if (trn == null)
                {
                    return cls.ExecuteNonQueryStatement(
                        sql,
                        cls.CreateDataBaseConnectionString(CompanyID),
                        prm
                    );
                }
                else
                {
                    return cls.ExecuteNonQueryStatement(
                        sql,
                        cls.CreateDataBaseConnectionString(CompanyID),
                        prm,
                        trn
                    );
                }
            }
            catch { throw; }
        }
        public int UpdateHeaderIDForDraft(
    int EmployeeID,
    int PayrollPeriodID,
    int NewHeaderID,
    int CompanyID,
    SqlTransaction trn = null)
        {
            try
            {
                clsSQL cls = new clsSQL();

                SqlParameter[] prm =
                {
            new SqlParameter("@EmployeeID", SqlDbType.Int) { Value = EmployeeID },
            new SqlParameter("@PayrollPeriodID", SqlDbType.Int) { Value = PayrollPeriodID },
            new SqlParameter("@NewHeaderID", SqlDbType.Int) { Value = NewHeaderID }
        };

                string sql = @"
            UPDATE tbl_PayrollDetails
            SET HeaderID = @NewHeaderID
            WHERE EmployeeID = @EmployeeID
              AND PayrollPeriodID = @PayrollPeriodID
              AND (HeaderID = 0 OR HeaderID IS NULL)
        ";

                if (trn == null)
                {
                    return cls.ExecuteNonQueryStatement(
                        sql,
                        cls.CreateDataBaseConnectionString(CompanyID),
                        prm
                    );
                }
                else
                {
                    return cls.ExecuteNonQueryStatement(
                        sql,
                        cls.CreateDataBaseConnectionString(CompanyID),
                        prm,
                        trn
                    );
                }
            }
            catch
            {
                throw;
            }
        }

    }
}
