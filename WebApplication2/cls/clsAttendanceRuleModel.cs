using System;
using System.Data;
using Microsoft.Data.SqlClient;

namespace WebApplication2.cls
{
    public class clsAttendanceRules
    {
        // ============================================================
        // SELECT
        // ============================================================
        public DataTable SelectAttendanceRules(int ID, string RuleName, int RuleTypeID, int CompanyID)
        {
            try
            {
                SqlParameter[] prm =
                {
                    new SqlParameter("@ID", SqlDbType.Int) { Value = ID },
                    new SqlParameter("@RuleName", SqlDbType.NVarChar, -1) { Value = RuleName },
                    new SqlParameter("@RuleTypeID", SqlDbType.Int) { Value = RuleTypeID },
                    new SqlParameter("@CompanyID", SqlDbType.Int) { Value = CompanyID },
                };

                clsSQL clsSQL = new clsSQL();

                DataTable dt = clsSQL.ExecuteQueryStatement(@"
                    SELECT * FROM tbl_AttendanceRules
                    WHERE (ID = @ID OR @ID = 0)
                    AND (RuleName = @RuleName OR @RuleName = '')
                    AND (RuleTypeID = @RuleTypeID OR @RuleTypeID = 0)
                    AND (CompanyID = @CompanyID OR @CompanyID = 0)
                ", clsSQL.CreateDataBaseConnectionString(CompanyID), prm);

                return dt;
            }
            catch (Exception)
            {
                throw;
            }
        }

        // ============================================================
        // DELETE
        // ============================================================
        public bool DeleteAttendanceRuleByID(int ID, int CompanyID)
        {
            try
            {
                clsSQL clsSQL = new clsSQL();

                SqlParameter[] prm =
                {
                    new SqlParameter("@ID", SqlDbType.Int) { Value = ID }
                };

                int A = clsSQL.ExecuteNonQueryStatement(@"
                    DELETE FROM tbl_AttendanceRules 
                    WHERE ID = @ID
                ", clsSQL.CreateDataBaseConnectionString(CompanyID), prm);

                return true;
            }
            catch (Exception)
            {
                throw;
            }
        }

        // ============================================================
        // INSERT
        // ============================================================
        public int InsertAttendanceRule(
            string RuleName,
            int RuleGroupID,
            int RuleTypeID,
            int CalculationTypeID,
            decimal Value,
            string FormulaText,
            int SalaryElementID,
            decimal MinAmount,
            decimal MaxAmount,
            int RoundTypeID,
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
                    new SqlParameter("@RuleName", SqlDbType.NVarChar, -1) { Value = RuleName },
                    new SqlParameter("@RuleGroupID", SqlDbType.Int) { Value = RuleGroupID },
                    new SqlParameter("@RuleTypeID", SqlDbType.Int) { Value = RuleTypeID },
                    new SqlParameter("@CalculationTypeID", SqlDbType.Int) { Value = CalculationTypeID },
                    new SqlParameter("@Value", SqlDbType.Decimal) { Value = Value },
                    new SqlParameter("@FormulaText", SqlDbType.NVarChar, -1) { Value = (FormulaText ?? "") },
                    new SqlParameter("@SalaryElementID", SqlDbType.Int) { Value = SalaryElementID },
                    new SqlParameter("@MinAmount", SqlDbType.Decimal) { Value = MinAmount },
                    new SqlParameter("@MaxAmount", SqlDbType.Decimal) { Value = MaxAmount },
                    new SqlParameter("@RoundTypeID", SqlDbType.Int) { Value = RoundTypeID },
                    new SqlParameter("@IsActive", SqlDbType.Bit) { Value = IsActive },
                    new SqlParameter("@CompanyID", SqlDbType.Int) { Value = CompanyID },
                    new SqlParameter("@CreationUserID", SqlDbType.Int) { Value = CreationUserID },
                    new SqlParameter("@CreationDate", SqlDbType.DateTime) { Value = DateTime.Now }
                };

                string sql = @"
                    INSERT INTO tbl_AttendanceRules
                    (RuleName, RuleGroupID, RuleTypeID, CalculationTypeID, Value,
                     FormulaText, SalaryElementID, MinAmount, MaxAmount,
                     RoundTypeID, IsActive, CompanyID, CreationUserID, CreationDate)
                    OUTPUT INSERTED.ID
                    VALUES
                    (@RuleName, @RuleGroupID, @RuleTypeID, @CalculationTypeID, @Value,
                     @FormulaText, @SalaryElementID, @MinAmount, @MaxAmount,
                     @RoundTypeID, @IsActive, @CompanyID, @CreationUserID, @CreationDate)
                ";

                clsSQL clsSQL = new clsSQL();

                if (trn == null)
                {
                    return Simulate.Integer32(clsSQL.ExecuteScalar(sql, prm, clsSQL.CreateDataBaseConnectionString(CompanyID)));
                }
                else
                {
                    return Simulate.Integer32(clsSQL.ExecuteScalar(sql, prm, clsSQL.CreateDataBaseConnectionString(CompanyID), trn));
                }
            }
            catch (Exception)
            {
                throw;
            }
        }

        // ============================================================
        // UPDATE
        // ============================================================
        public int UpdateAttendanceRule(
            int ID,
            string RuleName,
            int RuleGroupID,
            int RuleTypeID,
            int CalculationTypeID,
            decimal Value,
            string FormulaText,
            int SalaryElementID,
            decimal MinAmount,
            decimal MaxAmount,
            int RoundTypeID,
            bool IsActive,
            int ModificationUserID,
            int CompanyID,
            SqlTransaction trn = null
        )
        {
            try
            {
                clsSQL clsSQL = new clsSQL();

                SqlParameter[] prm =
                {
                    new SqlParameter("@ID", SqlDbType.Int) { Value = ID },
                    new SqlParameter("@RuleName", SqlDbType.NVarChar, -1) { Value = RuleName },
                    new SqlParameter("@RuleGroupID", SqlDbType.Int) { Value = RuleGroupID },
                    new SqlParameter("@RuleTypeID", SqlDbType.Int) { Value = RuleTypeID },
                    new SqlParameter("@CalculationTypeID", SqlDbType.Int) { Value = CalculationTypeID },
                    new SqlParameter("@Value", SqlDbType.Decimal) { Value = Value },
                    new SqlParameter("@FormulaText", SqlDbType.NVarChar, -1) { Value = (FormulaText ?? "") },
                    new SqlParameter("@SalaryElementID", SqlDbType.Int) { Value = SalaryElementID },
                    new SqlParameter("@MinAmount", SqlDbType.Decimal) { Value = MinAmount },
                    new SqlParameter("@MaxAmount", SqlDbType.Decimal) { Value = MaxAmount },
                    new SqlParameter("@RoundTypeID", SqlDbType.Int) { Value = RoundTypeID },
                    new SqlParameter("@IsActive", SqlDbType.Bit) { Value = IsActive },
                    new SqlParameter("@ModificationUserID", SqlDbType.Int) { Value = ModificationUserID },
                    new SqlParameter("@ModificationDate", SqlDbType.DateTime) { Value = DateTime.Now }
                };

                string sql = @"
                    UPDATE tbl_AttendanceRules SET
                        RuleName = @RuleName,
                        RuleGroupID = @RuleGroupID,
                        RuleTypeID = @RuleTypeID,
                        CalculationTypeID = @CalculationTypeID,
                        Value = @Value,
                        FormulaText = @FormulaText,
                        SalaryElementID = @SalaryElementID,
                        MinAmount = @MinAmount,
                        MaxAmount = @MaxAmount,
                        RoundTypeID = @RoundTypeID,
                        IsActive = @IsActive,
                        ModificationUserID = @ModificationUserID,
                        ModificationDate = @ModificationDate
                    WHERE ID = @ID
                ";

                if (trn == null)
                {
                    return clsSQL.ExecuteNonQueryStatement(sql, clsSQL.CreateDataBaseConnectionString(CompanyID), prm);
                }
                else
                {
                    return clsSQL.ExecuteNonQueryStatement(sql, clsSQL.CreateDataBaseConnectionString(CompanyID), prm, trn);
                }
            }
            catch (Exception)
            {
                throw;
            }
        }
    }
}
