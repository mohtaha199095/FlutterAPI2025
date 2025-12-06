using System;
using System.Data;
using Microsoft.Data.SqlClient;

namespace WebApplication2.cls
{
    public class clsEmployeeSalaryElements
    {
        public DataTable SelectElementsWithAccounts(int employeeID, int payrollPeriodID, int companyID, SqlTransaction trn = null)
        {
            try
            {
                string sql = @"
            SELECT 
                d.ElementID,
                se.AName AS ElementName,
                se.ElementTypeID,
                se.AccountID,
                d.Amount,
                ISNULL(se.SortIndex, 0) AS SortIndex
            FROM tbl_PayrollDetails d
            INNER JOIN tbl_SalaryElements se ON d.ElementID = se.ID
            WHERE 
                d.EmployeeID = @EmployeeID
                AND d.PayrollPeriodID = @PayrollPeriodID
                AND d.CompanyID = @CompanyID
            ORDER BY se.SortIndex, se.AName
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

                cmd.Parameters.AddWithValue("@EmployeeID", employeeID);
                cmd.Parameters.AddWithValue("@PayrollPeriodID", payrollPeriodID);
                cmd.Parameters.AddWithValue("@CompanyID", companyID);

                SqlDataAdapter da = new SqlDataAdapter(cmd);
                DataTable dt = new DataTable();
                da.Fill(dt);

                return dt;
            }
            catch (Exception ex)
            {
                throw new Exception("Error in SelectElementsWithAccounts: " + ex.Message, ex);
            }
        }

        // =====================================================
        // SELECT (Filter by ID, EmployeeID, SalaryElementID...)
        // =====================================================
        public DataTable SelectEmployeeSalaryElements(
            int ID,
            int EmployeeID,
            int SalaryElementID,
            int IsActive,
            int CompanyID,
            SqlTransaction trn = null)
        {
            try
            {
                SqlParameter[] prm =
                {
                    new SqlParameter("@ID", SqlDbType.Int) { Value = ID },
                    new SqlParameter("@EmployeeID", SqlDbType.Int) { Value = EmployeeID },
                    new SqlParameter("@SalaryElementID", SqlDbType.Int) { Value = SalaryElementID },
                    new SqlParameter("@IsActive", SqlDbType.Int) { Value = IsActive },
                    new SqlParameter("@CompanyID", SqlDbType.Int) { Value = CompanyID }
                };

                clsSQL clsSQL = new clsSQL();

                DataTable dt = clsSQL.ExecuteQueryStatement(@"
                    SELECT * 
                    FROM tbl_EmployeeSalaryElements 
                    WHERE (ID = @ID OR @ID = 0)
                      AND (EmployeeID = @EmployeeID OR @EmployeeID = 0)
                      AND (SalaryElementID = @SalaryElementID OR @SalaryElementID = 0)
                      AND (IsActive = @IsActive OR @IsActive = -1)
                      AND (CompanyID = @CompanyID OR @CompanyID = 0)
                ",
                clsSQL.CreateDataBaseConnectionString(CompanyID), prm, trn);

                return dt;
            }
            catch (Exception)
            {
                throw;
            }
        }
        public DataTable SelectEmployeeSalaryElementsForCalculation(
          int EmployeeID,
          DateTime PayrollDate,
          int CompanyID,
          SqlTransaction trn = null)
        {
            try
            {
                clsSQL cls = new clsSQL();

                SqlParameter[] prm =
                {
                    new SqlParameter("@EmployeeID", SqlDbType.Int) { Value = EmployeeID },
                    new SqlParameter("@PayrollDate", SqlDbType.DateTime) { Value = PayrollDate },
                    new SqlParameter("@CompanyID", SqlDbType.Int) { Value = CompanyID }
                };

                string sql = @"
                    SELECT 
                        EE.ID AS EmployeeSalaryElementID,
                        EE.EmployeeID,
                        EE.SalaryElementID,
                        EE.CalcTypeID,
                        EE.AssignedValue,
                        EE.IsCalculated,
                        EE.StartDate,
                        EE.EndDate,
                       (SELECT isposted FROM tbl_PayrollHeader WHERE tbl_PayrollHeader.EmployeeID =  EE.EmployeeID ) AS IsPosted,

                        -- From Salary Element Master Table
                        SE.Code,
                        SE.AName,
                        SE.EName,
                        SE.ElementTypeID,
                        SE.SalariesElementCategoryID,
                        SE.CalcTypeID AS MasterCalcTypeID,
                        SE.DefaultValue,
                        SE.PercentageOfElementID,
                        SE.FormulaText,
                        SE.IsTaxable,
                        SE.IsAffectSocialSecurity,
                        SE.IsRecurring,
                        SE.IsSystemElement,
                        SE.IsEditable

                    FROM tbl_EmployeeSalaryElements EE
                    INNER JOIN tbl_SalariesElements SE
                        ON EE.SalaryElementID = SE.ID
                       AND SE.CompanyID = @CompanyID

                    WHERE EE.EmployeeID = @EmployeeID
                      AND EE.CompanyID = @CompanyID
                      AND EE.IsActive = 1

                      -- Element must be valid for the payroll period date
                      AND @PayrollDate BETWEEN EE.StartDate AND EE.EndDate
                ";

                DataTable dt;

                if (trn == null)
                {
                    dt = cls.ExecuteQueryStatement(
                        sql,
                        cls.CreateDataBaseConnectionString(CompanyID),
                        prm
                    );
                }
                else
                {
                    dt = cls.ExecuteQueryStatement(
                        sql,
                        cls.CreateDataBaseConnectionString(CompanyID),
                        prm,
                        trn
                    );
                }

                return dt;
            }
            catch
            {
                throw;
            }
        }
        // =====================================================
        // DELETE
        // =====================================================
        public bool DeleteEmployeeSalaryElementByID(int ID, int CompanyID)
        {
            try
            {
                clsSQL clsSQL = new clsSQL();

                SqlParameter[] prm =
                {
                    new SqlParameter("@ID", SqlDbType.Int) { Value = ID }
                };

                int A = clsSQL.ExecuteNonQueryStatement(
                    @"DELETE FROM tbl_EmployeeSalaryElements WHERE ID = @ID",
                    clsSQL.CreateDataBaseConnectionString(CompanyID), prm);

                return true;
            }
            catch (Exception)
            {
                throw;
            }
        }

        // =====================================================
        // INSERT
        // =====================================================
        public int InsertEmployeeSalaryElement(
            int EmployeeID,
            int SalaryElementID,
            int CalcTypeID,
            decimal AssignedValue,
            bool IsCalculated,
            DateTime StartDate,
            DateTime EndDate,
            bool IsActive,
            int CompanyID,
            int CreationUserId,
            SqlTransaction trn = null)
        {
            try
            {
                SqlParameter[] prm =
                {
                    new SqlParameter("@EmployeeID", SqlDbType.Int) { Value = EmployeeID },
                    new SqlParameter("@SalaryElementID", SqlDbType.Int) { Value = SalaryElementID },
                    new SqlParameter("@CalcTypeID", SqlDbType.Int) { Value = CalcTypeID },
                    new SqlParameter("@AssignedValue", SqlDbType.Decimal) { Value = AssignedValue },
                    new SqlParameter("@IsCalculated", SqlDbType.Bit) { Value = IsCalculated },
                    new SqlParameter("@StartDate", SqlDbType.DateTime) { Value = StartDate },
                    new SqlParameter("@EndDate", SqlDbType.DateTime) { Value = EndDate },
                    new SqlParameter("@IsActive", SqlDbType.Bit) { Value = IsActive },
                    new SqlParameter("@CompanyID", SqlDbType.Int) { Value = CompanyID },
                    new SqlParameter("@CreationUserId", SqlDbType.Int) { Value = CreationUserId },
                    new SqlParameter("@CreationDate", SqlDbType.DateTime) { Value = DateTime.Now }
                };

                string sql = @"
                    INSERT INTO tbl_EmployeeSalaryElements
                    (
                        EmployeeID,
                        SalaryElementID,
                        CalcTypeID,
                        AssignedValue,
                        IsCalculated,
                        StartDate,
                        EndDate,
                        IsActive,
                        CompanyID,
                        CreationUserId,
                        CreationDate
                    )
                    OUTPUT INSERTED.ID
                    VALUES
                    (
                        @EmployeeID,
                        @SalaryElementID,
                        @CalcTypeID,
                        @AssignedValue,
                        @IsCalculated,
                        @StartDate,
                        @EndDate,
                        @IsActive,
                        @CompanyID,
                        @CreationUserId,
                        @CreationDate
                    )";

                clsSQL clsSQL = new clsSQL();

                if (trn == null)
                {
                    return Simulate.Integer32(
                        clsSQL.ExecuteScalar(sql, prm, clsSQL.CreateDataBaseConnectionString(CompanyID))
                    );
                }
                else
                {
                    return Simulate.Integer32(
                        clsSQL.ExecuteScalar(sql, prm, clsSQL.CreateDataBaseConnectionString(CompanyID), trn)
                    );
                }
            }
            catch (Exception)
            {
                throw;
            }
        }

        // =====================================================
        // UPDATE
        // =====================================================
        public int UpdateEmployeeSalaryElement(
            int ID,
            int EmployeeID,
            int SalaryElementID,
            int CalcTypeID,
            decimal AssignedValue,
            bool IsCalculated,
            DateTime StartDate,
            DateTime EndDate,
            bool IsActive,
            int ModificationUserId,
            int CompanyID,
            SqlTransaction trn = null)
        {
            try
            {
                SqlParameter[] prm =
                {
                    new SqlParameter("@ID", SqlDbType.Int) { Value = ID },
                    new SqlParameter("@EmployeeID", SqlDbType.Int) { Value = EmployeeID },
                    new SqlParameter("@SalaryElementID", SqlDbType.Int) { Value = SalaryElementID },
                    new SqlParameter("@CalcTypeID", SqlDbType.Int) { Value = CalcTypeID },
                    new SqlParameter("@AssignedValue", SqlDbType.Decimal) { Value = AssignedValue },
                    new SqlParameter("@IsCalculated", SqlDbType.Bit) { Value = IsCalculated },
                    new SqlParameter("@StartDate", SqlDbType.DateTime) { Value = StartDate },
                    new SqlParameter("@EndDate", SqlDbType.DateTime) { Value = EndDate },
                    new SqlParameter("@IsActive", SqlDbType.Bit) { Value = IsActive },
                    new SqlParameter("@ModificationUserId", SqlDbType.Int) { Value = ModificationUserId },
                    new SqlParameter("@ModificationDate", SqlDbType.DateTime) { Value = DateTime.Now }
                };

                clsSQL clsSQL = new clsSQL();

                int result = clsSQL.ExecuteNonQueryStatement(@"
                    UPDATE tbl_EmployeeSalaryElements SET
                        EmployeeID = @EmployeeID,
                        SalaryElementID = @SalaryElementID,
                        CalcTypeID = @CalcTypeID,
                        AssignedValue = @AssignedValue,
                        IsCalculated = @IsCalculated,
                        StartDate = @StartDate,
                        EndDate = @EndDate,
                        IsActive = @IsActive,
                        ModificationUserId = @ModificationUserId,
                        ModificationDate = @ModificationDate
                    WHERE ID = @ID
                ",
                clsSQL.CreateDataBaseConnectionString(CompanyID), prm, trn);

                return result;
            }
            catch (Exception)
            {
                throw;
            }
        }
    }
}
