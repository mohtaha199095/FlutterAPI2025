using Microsoft.Data.SqlClient;
using System;
using System.Data;

namespace WebApplication2.cls
{
    public class clsEmployeeContract
    {
        // =====================================================
        // SELECT
        // =====================================================
        public DataTable SelectEmployeeContractByID(int Id, int EmployeeID, bool ActiveOnly, int CompanyID)
        {
            try
            {
                SqlParameter[] prm =
                {
                    new SqlParameter("@Id", SqlDbType.Int) { Value = Id },
                    new SqlParameter("@EmployeeID", SqlDbType.Int) { Value = EmployeeID },
                    new SqlParameter("@ActiveOnly", SqlDbType.Bit) { Value = ActiveOnly },
                    new SqlParameter("@CompanyID", SqlDbType.Int) { Value = CompanyID },
                };

                string sql = @"
                    SELECT c.*, 
                           ISNULL(e.AName,'') AS EmployeeAName,
                           ISNULL(e.EName,'') AS EmployeeEName,
                           ISNULL(e.EmployeeCode,'') AS EmployeeCode,
                           ISNULL(e.NationalNumber,'') AS EmployeeNationalNumber,
                           ISNULL(e.Email,'') AS EmployeeEmail,
                           ISNULL(e.Tel1,'') AS EmployeeTel1,
                           ISNULL(ct.AName,'') AS ContractTypeAName,
                           ISNULL(ct.EName,'') AS ContractTypeEName,
                           ISNULL(jt.AName,'') AS JobTitleAName,
                           ISNULL(jt.EName,'') AS JobTitleEName,
                           ISNULL(d.AName,'')  AS DepartmentAName,
                           ISNULL(d.EName,'')  AS DepartmentEName,
                           ISNULL(b.AName,'') AS BranchAName,
                           ISNULL(b.EName,'') AS BranchEName
                    FROM tbl_EmployeeContract c
                    LEFT JOIN tbl_employee        e  ON c.EmployeeID     = e.ID
                    LEFT JOIN tbl_HRContractType  ct ON c.ContractTypeID = ct.ID
                    LEFT JOIN tbl_JobTitle        jt ON c.JobTitleID     = jt.ID
                    LEFT JOIN tbl_Department      d  ON c.DepartmentID   = d.ID
                    LEFT JOIN tbl_Branch          b  ON c.BranchID       = b.ID
                    WHERE (c.ID = @Id OR @Id = 0)
                      AND (c.EmployeeID = @EmployeeID OR @EmployeeID = 0)
                      AND (c.IsActive = 1 OR @ActiveOnly = 0)
                      AND (c.CompanyID = @CompanyID OR @CompanyID = 0)
                    ORDER BY c.IsActive DESC, c.StartDate DESC, c.ID DESC";

                clsSQL clsSQL = new clsSQL();
                DataTable dt = clsSQL.ExecuteQueryStatement(sql, clsSQL.CreateDataBaseConnectionString(CompanyID), prm);
                return dt;
            }
            catch (Exception)
            {
                throw;
            }
        }

        // =====================================================
        // DELETE
        // =====================================================
        public bool DeleteEmployeeContractByID(int Id, int CompanyID)
        {
            try
            {
                clsSQL clsSQL = new clsSQL();
                SqlParameter[] prm =
                {
                    new SqlParameter("@Id", SqlDbType.Int) { Value = Id },
                    new SqlParameter("@CompanyID", SqlDbType.Int) { Value = CompanyID },
                };
                clsSQL.ExecuteNonQueryStatement(
                    @"delete from tbl_EmployeeContract where ID = @Id AND CompanyID = @CompanyID",
                    clsSQL.CreateDataBaseConnectionString(CompanyID), prm);
                return true;
            }
            catch (Exception)
            {
                throw;
            }
        }

        // =====================================================
        // DEACTIVATE existing active contracts for the employee
        // (so the new one is the only IsActive=1 record)
        // =====================================================
        public int DeactivateActiveContracts(int EmployeeID, int CompanyID, int ModificationUserID, SqlTransaction trn = null)
        {
            try
            {
                SqlParameter[] prm =
                {
                    new SqlParameter("@EmployeeID", SqlDbType.Int) { Value = EmployeeID },
                    new SqlParameter("@CompanyID", SqlDbType.Int) { Value = CompanyID },
                    new SqlParameter("@ModificationUserID", SqlDbType.Int) { Value = ModificationUserID },
                    new SqlParameter("@ModificationDate", SqlDbType.DateTime) { Value = DateTime.Now },
                };

                string sql = @"
                    UPDATE tbl_EmployeeContract
                       SET IsActive = 0,
                           ModificationUserID = @ModificationUserID,
                           ModificationDate   = @ModificationDate
                     WHERE EmployeeID = @EmployeeID
                       AND CompanyID  = @CompanyID
                       AND IsActive   = 1";

                clsSQL clsSQL = new clsSQL();
                return clsSQL.ExecuteNonQueryStatement(sql, clsSQL.CreateDataBaseConnectionString(CompanyID), prm, trn);
            }
            catch (Exception)
            {
                throw;
            }
        }

        // =====================================================
        // INSERT
        // =====================================================
        public int InsertEmployeeContract(
            int EmployeeID,
            int ContractTypeID,
            int JobTitleID,
            int DepartmentID,
            int BranchID,
            string ContractNumber,
            DateTime StartDate,
            DateTime EndDate,
            bool IsOpenEnded,
            int ProbationMonths,
            decimal WorkingHoursPerWeek,
            decimal BasicSalary,
            int AnnualLeaveDaysPerYear,
            int AnnualLeaveDaysAfter5Years,
            int SickLeaveFullPayDaysPerYear,
            int SickLeaveExtendedDaysPerYear,
            string Notes,
            bool IsActive,
            int CompanyID,
            int CreationUserID,
            SqlTransaction trn = null,
            int documentStatus = 2)
        {
            try
            {
                SqlParameter[] prm =
                {
                    new SqlParameter("@EmployeeID",           SqlDbType.Int)        { Value = EmployeeID },
                    new SqlParameter("@ContractTypeID",       SqlDbType.Int)        { Value = ContractTypeID },
                    new SqlParameter("@JobTitleID",           SqlDbType.Int)        { Value = JobTitleID },
                    new SqlParameter("@DepartmentID",         SqlDbType.Int)        { Value = DepartmentID },
                    new SqlParameter("@BranchID",             SqlDbType.Int)        { Value = BranchID },
                    new SqlParameter("@ContractNumber",       SqlDbType.NVarChar,-1){ Value = ContractNumber ?? "" },
                    new SqlParameter("@StartDate",            SqlDbType.DateTime)   { Value = StartDate },
                    new SqlParameter("@EndDate",              SqlDbType.DateTime)   { Value = EndDate },
                    new SqlParameter("@IsOpenEnded",          SqlDbType.Bit)        { Value = IsOpenEnded },
                    new SqlParameter("@ProbationMonths",      SqlDbType.Int)        { Value = ProbationMonths },
                    new SqlParameter("@WorkingHoursPerWeek",  SqlDbType.Decimal)    { Value = WorkingHoursPerWeek },
                    new SqlParameter("@BasicSalary",          SqlDbType.Decimal)    { Value = BasicSalary },
                    new SqlParameter("@AnnualLeaveDaysPerYear", SqlDbType.Int)        { Value = AnnualLeaveDaysPerYear },
                    new SqlParameter("@AnnualLeaveDaysAfter5Years", SqlDbType.Int)   { Value = AnnualLeaveDaysAfter5Years },
                    new SqlParameter("@SickLeaveFullPayDaysPerYear", SqlDbType.Int)  { Value = SickLeaveFullPayDaysPerYear },
                    new SqlParameter("@SickLeaveExtendedDaysPerYear", SqlDbType.Int) { Value = SickLeaveExtendedDaysPerYear },
                    new SqlParameter("@Notes",                SqlDbType.NVarChar,-1){ Value = Notes ?? "" },
                    new SqlParameter("@IsActive",             SqlDbType.Bit)        { Value = IsActive },
                    new SqlParameter("@CompanyID",            SqlDbType.Int)        { Value = CompanyID },
                    new SqlParameter("@CreationUserID",       SqlDbType.Int)        { Value = CreationUserID },
                    new SqlParameter("@CreationDate",         SqlDbType.DateTime)   { Value = DateTime.Now },
                    new SqlParameter("@DocumentStatus",       SqlDbType.Int)        { Value = documentStatus },
                };

                string sql = @"
                    INSERT INTO tbl_EmployeeContract
                    (
                        EmployeeID, ContractTypeID, JobTitleID, DepartmentID, BranchID,
                        ContractNumber, StartDate, EndDate, IsOpenEnded, ProbationMonths,
                        WorkingHoursPerWeek, BasicSalary,
                        AnnualLeaveDaysPerYear, AnnualLeaveDaysAfter5Years,
                        SickLeaveFullPayDaysPerYear, SickLeaveExtendedDaysPerYear,
                        Notes, IsActive,
                        CompanyID, CreationUserID, CreationDate,
                        Guid, DocumentStatus
                    )
                    OUTPUT INSERTED.ID
                    VALUES
                    (
                        @EmployeeID, @ContractTypeID, @JobTitleID, @DepartmentID, @BranchID,
                        @ContractNumber, @StartDate, @EndDate, @IsOpenEnded, @ProbationMonths,
                        @WorkingHoursPerWeek, @BasicSalary,
                        @AnnualLeaveDaysPerYear, @AnnualLeaveDaysAfter5Years,
                        @SickLeaveFullPayDaysPerYear, @SickLeaveExtendedDaysPerYear,
                        @Notes, @IsActive,
                        @CompanyID, @CreationUserID, @CreationDate,
                        NEWID(), @DocumentStatus
                    )";

                clsSQL clsSQL = new clsSQL();
                if (trn == null)
                {
                    return Simulate.Integer32(
                        clsSQL.ExecuteScalar(sql, prm, clsSQL.CreateDataBaseConnectionString(CompanyID)));
                }
                else
                {
                    return Simulate.Integer32(
                        clsSQL.ExecuteScalar(sql, prm, clsSQL.CreateDataBaseConnectionString(CompanyID), trn));
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
        public int UpdateEmployeeContract(
            int ID,
            int EmployeeID,
            int ContractTypeID,
            int JobTitleID,
            int DepartmentID,
            int BranchID,
            string ContractNumber,
            DateTime StartDate,
            DateTime EndDate,
            bool IsOpenEnded,
            int ProbationMonths,
            decimal WorkingHoursPerWeek,
            decimal BasicSalary,
            int AnnualLeaveDaysPerYear,
            int AnnualLeaveDaysAfter5Years,
            int SickLeaveFullPayDaysPerYear,
            int SickLeaveExtendedDaysPerYear,
            string Notes,
            bool IsActive,
            int CompanyID,
            int ModificationUserID,
            SqlTransaction trn = null)
        {
            try
            {
                SqlParameter[] prm =
                {
                    new SqlParameter("@ID",                   SqlDbType.Int)        { Value = ID },
                    new SqlParameter("@EmployeeID",           SqlDbType.Int)        { Value = EmployeeID },
                    new SqlParameter("@ContractTypeID",       SqlDbType.Int)        { Value = ContractTypeID },
                    new SqlParameter("@JobTitleID",           SqlDbType.Int)        { Value = JobTitleID },
                    new SqlParameter("@DepartmentID",         SqlDbType.Int)        { Value = DepartmentID },
                    new SqlParameter("@BranchID",             SqlDbType.Int)        { Value = BranchID },
                    new SqlParameter("@ContractNumber",       SqlDbType.NVarChar,-1){ Value = ContractNumber ?? "" },
                    new SqlParameter("@StartDate",            SqlDbType.DateTime)   { Value = StartDate },
                    new SqlParameter("@EndDate",              SqlDbType.DateTime)   { Value = EndDate },
                    new SqlParameter("@IsOpenEnded",          SqlDbType.Bit)        { Value = IsOpenEnded },
                    new SqlParameter("@ProbationMonths",      SqlDbType.Int)        { Value = ProbationMonths },
                    new SqlParameter("@WorkingHoursPerWeek",  SqlDbType.Decimal)    { Value = WorkingHoursPerWeek },
                    new SqlParameter("@BasicSalary",          SqlDbType.Decimal)    { Value = BasicSalary },
                    new SqlParameter("@AnnualLeaveDaysPerYear", SqlDbType.Int)        { Value = AnnualLeaveDaysPerYear },
                    new SqlParameter("@AnnualLeaveDaysAfter5Years", SqlDbType.Int)   { Value = AnnualLeaveDaysAfter5Years },
                    new SqlParameter("@SickLeaveFullPayDaysPerYear", SqlDbType.Int)  { Value = SickLeaveFullPayDaysPerYear },
                    new SqlParameter("@SickLeaveExtendedDaysPerYear", SqlDbType.Int) { Value = SickLeaveExtendedDaysPerYear },
                    new SqlParameter("@Notes",                SqlDbType.NVarChar,-1){ Value = Notes ?? "" },
                    new SqlParameter("@IsActive",             SqlDbType.Bit)        { Value = IsActive },
                    new SqlParameter("@ModificationUserID",   SqlDbType.Int)        { Value = ModificationUserID },
                    new SqlParameter("@ModificationDate",     SqlDbType.DateTime)   { Value = DateTime.Now },
                    new SqlParameter("@CompanyID",            SqlDbType.Int)        { Value = CompanyID },
                };

                string sql = @"
                    UPDATE tbl_EmployeeContract
                       SET EmployeeID          = @EmployeeID,
                           ContractTypeID      = @ContractTypeID,
                           JobTitleID          = @JobTitleID,
                           DepartmentID        = @DepartmentID,
                           BranchID            = @BranchID,
                           ContractNumber      = @ContractNumber,
                           StartDate           = @StartDate,
                           EndDate             = @EndDate,
                           IsOpenEnded         = @IsOpenEnded,
                           ProbationMonths     = @ProbationMonths,
                           WorkingHoursPerWeek = @WorkingHoursPerWeek,
                           BasicSalary         = @BasicSalary,
                           AnnualLeaveDaysPerYear = @AnnualLeaveDaysPerYear,
                           AnnualLeaveDaysAfter5Years = @AnnualLeaveDaysAfter5Years,
                           SickLeaveFullPayDaysPerYear = @SickLeaveFullPayDaysPerYear,
                           SickLeaveExtendedDaysPerYear = @SickLeaveExtendedDaysPerYear,
                           Notes               = @Notes,
                           IsActive            = @IsActive,
                           ModificationUserID  = @ModificationUserID,
                           ModificationDate    = @ModificationDate
                     WHERE ID = @ID AND CompanyID = @CompanyID";

                clsSQL clsSQL = new clsSQL();
                return clsSQL.ExecuteNonQueryStatement(sql, clsSQL.CreateDataBaseConnectionString(CompanyID), prm, trn);
            }
            catch (Exception)
            {
                throw;
            }
        }

        // =====================================================
        // Helper: find the BASIC salary element ID for a company,
        // so a new hire can be wired into payroll immediately.
        // Returns 0 if none configured.
        // =====================================================
        public int GetBasicSalaryElementID(int CompanyID, SqlTransaction trn = null)
        {
            try
            {
                SqlParameter[] prm =
                {
                    new SqlParameter("@CompanyID", SqlDbType.Int) { Value = CompanyID },
                };

                string sql = @"
                    SELECT TOP 1 ID
                      FROM tbl_SalariesElements
                     WHERE CompanyID = @CompanyID
                       AND Code = 'BASIC'
                     ORDER BY ID ASC";

                clsSQL clsSQL = new clsSQL();
                object o;
                if (trn == null)
                {
                    o = clsSQL.ExecuteScalar(sql, prm, clsSQL.CreateDataBaseConnectionString(CompanyID));
                }
                else
                {
                    o = clsSQL.ExecuteScalar(sql, prm, clsSQL.CreateDataBaseConnectionString(CompanyID), trn);
                }
                return Simulate.Integer32(o);
            }
            catch (Exception)
            {
                return 0;
            }
        }
    }
}
