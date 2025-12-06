using Microsoft.Data.SqlClient;
using System;
using System.Data;

namespace WebApplication2.cls
{
    public class clsSalariesElements
    {
        // =============================================================
        // SELECT BY FILTER
        // =============================================================
        public DataTable SelectSalariesElements(int ID, string Code, string AName, string EName, int CompanyID)
        {
            try
            {
                SqlParameter[] prm =
                {
                    new SqlParameter("@ID", SqlDbType.Int) { Value = ID },
                    new SqlParameter("@Code", SqlDbType.VarChar,-1) { Value = Code },
                    new SqlParameter("@AName", SqlDbType.NVarChar,-1) { Value = AName },
                    new SqlParameter("@EName", SqlDbType.NVarChar,-1) { Value = EName },
                    new SqlParameter("@CompanyID", SqlDbType.Int) { Value = CompanyID }
                };

                clsSQL clsSQL = new clsSQL();

                string query = @"
                SELECT * 
                FROM tbl_SalariesElements 
                WHERE (ID=@ID OR @ID=0)
                  AND (Code=@Code OR @Code='')
                  AND (AName=@AName OR @AName='')
                  AND (EName=@EName OR @EName='')
                  AND (CompanyID=@CompanyID OR @CompanyID=0)
                ";

                return clsSQL.ExecuteQueryStatement(query, clsSQL.CreateDataBaseConnectionString(CompanyID), prm);
            }
            catch (Exception)
            {
                throw;
            }
        }

        // =============================================================
        // DELETE
        // =============================================================
        public bool DeleteSalariesElementByID(int ID, int CompanyID)
        {
            try
            {
                clsSQL clsSQL = new clsSQL();

                SqlParameter[] prm =
                {
                    new SqlParameter("@ID", SqlDbType.Int) { Value = ID }
                };

                clsSQL.ExecuteNonQueryStatement(
                    @"DELETE FROM tbl_SalariesElements WHERE ID=@ID",
                    clsSQL.CreateDataBaseConnectionString(CompanyID),
                    prm
                );

                return true;
            }
            catch (Exception)
            {
                throw;
            }
        }

        // =============================================================
        // INSERT
        // =============================================================
        public int InsertSalariesElement(
            string Code,
            string AName,
            string EName,
            int ElementTypeID,
            int SalariesElementCategoryID,
            int CalcTypeID,
            decimal DefaultValue,
            int PercentageOfElementID,
            string FormulaText,
            bool IsTaxable,
            bool IsAffectSocialSecurity,
            bool IsRecurring,
            bool IsSystemElement,
            bool IsEditable,
            DateTime StartDate,
            DateTime EndDate,
            int EmployeeDebitAccountID,
            int EmployeeCreditAccountID,
            int CompanyDebitAccountID,
            int CompanyCreditAccountID,
            int CompanyID,
            int CreationUserId,
            int SortIndex,
            SqlTransaction trn = null
        )
        {
            try
            {
                SqlParameter[] prm =
                {
                    new SqlParameter("@Code", SqlDbType.VarChar) { Value = Code },
                    new SqlParameter("@AName", SqlDbType.NVarChar,-1) { Value = AName },
                    new SqlParameter("@EName", SqlDbType.NVarChar,-1) { Value = EName },

                    new SqlParameter("@ElementTypeID", SqlDbType.Int) { Value = ElementTypeID },
                    new SqlParameter("@SalariesElementCategoryID", SqlDbType.Int) { Value = SalariesElementCategoryID },
                    new SqlParameter("@CalcTypeID", SqlDbType.Int) { Value = CalcTypeID },
                    new SqlParameter("@DefaultValue", SqlDbType.Decimal) { Value = DefaultValue },
                    new SqlParameter("@PercentageOfElementID", SqlDbType.Int) { Value = PercentageOfElementID },
                    new SqlParameter("@FormulaText", SqlDbType.NVarChar,-1) { Value = FormulaText },

                    new SqlParameter("@IsTaxable", SqlDbType.Bit) { Value = IsTaxable },
                    new SqlParameter("@IsAffectSocialSecurity", SqlDbType.Bit) { Value = IsAffectSocialSecurity },
                    new SqlParameter("@IsRecurring", SqlDbType.Bit) { Value = IsRecurring },
                    new SqlParameter("@IsSystemElement", SqlDbType.Bit) { Value = IsSystemElement },
                    new SqlParameter("@IsEditable", SqlDbType.Bit) { Value = IsEditable },

                    new SqlParameter("@StartDate", SqlDbType.DateTime) { Value = StartDate },
                    new SqlParameter("@EndDate", SqlDbType.DateTime) { Value = EndDate },

                    new SqlParameter("@EmployeeDebitAccountID", SqlDbType.Int) { Value = EmployeeDebitAccountID },
                    new SqlParameter("@EmployeeCreditAccountID", SqlDbType.Int) { Value = EmployeeCreditAccountID },
                    new SqlParameter("@CompanyDebitAccountID", SqlDbType.Int) { Value = CompanyDebitAccountID },
                    new SqlParameter("@CompanyCreditAccountID", SqlDbType.Int) { Value = CompanyCreditAccountID },

                    new SqlParameter("@CompanyID", SqlDbType.Int) { Value = CompanyID },
                    new SqlParameter("@CreationUserId", SqlDbType.Int) { Value = CreationUserId },
                    new SqlParameter("@CreationDate", SqlDbType.DateTime) { Value = DateTime.Now },

                     new SqlParameter("@SortIndex", SqlDbType.Int) { Value = SortIndex },
                    
                };

                string sql = @"
                INSERT INTO tbl_SalariesElements
                (
                    Code, AName, EName,
                    ElementTypeID, SalariesElementCategoryID, CalcTypeID,
                    DefaultValue, PercentageOfElementID, FormulaText,
                    IsTaxable, IsAffectSocialSecurity, IsRecurring, IsSystemElement, IsEditable,
                    StartDate, EndDate,
                    EmployeeDebitAccountID, EmployeeCreditAccountID,
                    CompanyDebitAccountID, CompanyCreditAccountID,
                    CompanyID, CreationUserId, CreationDate,SortIndex
                )
                OUTPUT INSERTED.ID
                VALUES
                (
                    @Code, @AName, @EName,
                    @ElementTypeID, @SalariesElementCategoryID, @CalcTypeID,
                    @DefaultValue, @PercentageOfElementID, @FormulaText,
                    @IsTaxable, @IsAffectSocialSecurity, @IsRecurring, @IsSystemElement, @IsEditable,
                    @StartDate, @EndDate,
                    @EmployeeDebitAccountID, @EmployeeCreditAccountID,
                    @CompanyDebitAccountID, @CompanyCreditAccountID,
                    @CompanyID, @CreationUserId, @CreationDate,@SortIndex
                )";

                clsSQL clsSQL = new clsSQL();

                if (trn == null)
                    return Simulate.Integer32(clsSQL.ExecuteScalar(sql, prm, clsSQL.CreateDataBaseConnectionString(CompanyID)));
                else
                    return Simulate.Integer32(clsSQL.ExecuteScalar(sql, prm, clsSQL.CreateDataBaseConnectionString(CompanyID), trn));
            }
            catch (Exception)
            {
                throw;
            }
        }

        // =============================================================
        // UPDATE
        // =============================================================
        public int UpdateSalariesElement(
            int ID,
            string Code,
            string AName,
            string EName,
            int ElementTypeID,
            int SalariesElementCategoryID,
            int CalcTypeID,
            decimal DefaultValue,
            int PercentageOfElementID,
            string FormulaText,
            bool IsTaxable,
            bool IsAffectSocialSecurity,
            bool IsRecurring,
            bool IsEditable,
            DateTime StartDate,
            DateTime EndDate,
            int EmployeeDebitAccountID,
            int EmployeeCreditAccountID,
            int CompanyDebitAccountID,
            int CompanyCreditAccountID,
            int ModificationUserId,
            int CompanyID,
            int SortIndex
        )
        {
            try
            {
                SqlParameter[] prm =
                {
                    new SqlParameter("@ID", SqlDbType.Int) { Value = ID },

                    new SqlParameter("@Code", SqlDbType.VarChar) { Value = Code },
                    new SqlParameter("@AName", SqlDbType.NVarChar,-1) { Value = AName },
                    new SqlParameter("@EName", SqlDbType.NVarChar,-1) { Value = EName },

                    new SqlParameter("@ElementTypeID", SqlDbType.Int) { Value = ElementTypeID },
                    new SqlParameter("@SalariesElementCategoryID", SqlDbType.Int) { Value = SalariesElementCategoryID },
                    new SqlParameter("@CalcTypeID", SqlDbType.Int) { Value = CalcTypeID },
                    new SqlParameter("@DefaultValue", SqlDbType.Decimal) { Value = DefaultValue },
                    new SqlParameter("@PercentageOfElementID", SqlDbType.Int) { Value = PercentageOfElementID },
                    new SqlParameter("@FormulaText", SqlDbType.NVarChar,-1) { Value = FormulaText },

                    new SqlParameter("@IsTaxable", SqlDbType.Bit) { Value = IsTaxable },
                    new SqlParameter("@IsAffectSocialSecurity", SqlDbType.Bit) { Value = IsAffectSocialSecurity },
                    new SqlParameter("@IsRecurring", SqlDbType.Bit) { Value = IsRecurring },
                    new SqlParameter("@IsEditable", SqlDbType.Bit) { Value = IsEditable },

                    new SqlParameter("@StartDate", SqlDbType.DateTime) { Value = StartDate },
                    new SqlParameter("@EndDate", SqlDbType.DateTime) { Value = EndDate },

                    new SqlParameter("@EmployeeDebitAccountID", SqlDbType.Int) { Value = EmployeeDebitAccountID },
                    new SqlParameter("@EmployeeCreditAccountID", SqlDbType.Int) { Value = EmployeeCreditAccountID },
                    new SqlParameter("@CompanyDebitAccountID", SqlDbType.Int) { Value = CompanyDebitAccountID },
                    new SqlParameter("@CompanyCreditAccountID", SqlDbType.Int) { Value = CompanyCreditAccountID },

                    new SqlParameter("@ModificationUserId", SqlDbType.Int) { Value = ModificationUserId },
                    new SqlParameter("@ModificationDate", SqlDbType.DateTime) { Value = DateTime.Now },

                     new SqlParameter("@SortIndex", SqlDbType.Int) { Value = SortIndex },

                    
                };

                clsSQL clsSQL = new clsSQL();

                string sql = @"
                UPDATE tbl_SalariesElements SET
                    Code=@Code,
                    AName=@AName,
                    EName=@EName,
                    ElementTypeID=@ElementTypeID,
                    SalariesElementCategoryID=@SalariesElementCategoryID,
                    CalcTypeID=@CalcTypeID,
                    DefaultValue=@DefaultValue,
                    PercentageOfElementID=@PercentageOfElementID,
                    FormulaText=@FormulaText,
                    IsTaxable=@IsTaxable,
                    IsAffectSocialSecurity=@IsAffectSocialSecurity,
                    IsRecurring=@IsRecurring,
                    IsEditable=@IsEditable,
                    StartDate=@StartDate,
                    EndDate=@EndDate,
                    EmployeeDebitAccountID=@EmployeeDebitAccountID,
                    EmployeeCreditAccountID=@EmployeeCreditAccountID,
                    CompanyDebitAccountID=@CompanyDebitAccountID,
                    CompanyCreditAccountID=@CompanyCreditAccountID,
                    ModificationDate=@ModificationDate,
                    ModificationUserId=@ModificationUserId,
                    SortIndex=@SortIndex
                WHERE ID=@ID
                ";

                return clsSQL.ExecuteNonQueryStatement(
                    sql,
                    clsSQL.CreateDataBaseConnectionString(CompanyID),
                    prm
                );
            }
            catch (Exception)
            {
                throw;
            }
        }
    }
}
