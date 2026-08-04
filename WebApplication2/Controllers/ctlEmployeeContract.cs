using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Newtonsoft.Json;
using System;
using System.Data;
using System.Text.Json;
using WebApplication2.cls;
using static WebApplication2.MainClasses.clsEnum;

namespace WebApplication2.Controllers
{
    [Route("api/ctlEmployeeContract")]
    public class ctlEmployeeContract : Controller
    {
        // ====================================================
        // GET: list contracts (optionally filtered by employee
        // or showing only the active row)
        // ====================================================
        [HttpGet]
        [Route("SelectEmployeeContractByID")]
        public string SelectEmployeeContractByID(int ID, int EmployeeID, bool ActiveOnly, int CompanyID)
        {
            try
            {
                clsEmployeeContract cls = new clsEmployeeContract();
                DataTable dt = cls.SelectEmployeeContractByID(ID, EmployeeID, ActiveOnly, CompanyID);
                if (dt != null)
                {
                    return JsonConvert.SerializeObject(dt);
                }
                return "";
            }
            catch (Exception)
            {
                throw;
            }
        }

        // ====================================================
        // POST: delete
        // ====================================================
        [HttpPost]
        [Route("DeleteEmployeeContractByID")]
        public bool DeleteEmployeeContractByID(int ID, int CompanyID)
        {
            try
            {
                clsEmployeeContract cls = new clsEmployeeContract();
                return cls.DeleteEmployeeContractByID(ID, CompanyID);
            }
            catch (Exception)
            {
                throw;
            }
        }

        // ====================================================
        // POST: insert (used for adding/renewing a contract on
        // an EXISTING employee). Hiring a brand-new employee
        // should call HireEmployee instead.
        // ====================================================
        [HttpPost]
        [Route("InsertEmployeeContract")]
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
            int CreationUserID)
        {
            try
            {
                if (EmployeeID <= 0) throw new ArgumentException("EmployeeID is required");
                if (CompanyID <= 0) throw new ArgumentException("CompanyID is required");

                clsSQL clsSQL = new clsSQL();
                using (SqlConnection con = new SqlConnection(clsSQL.CreateDataBaseConnectionString(CompanyID)))
                {
                    con.Open();
                    using (SqlTransaction trn = con.BeginTransaction())
                    {
                        try
                        {
                            clsEmployeeContract cls = new clsEmployeeContract();
                            clsApprovalEngine approvalEngine = new clsApprovalEngine();
                            int documentStatus = approvalEngine.ResolveInitialDocumentStatus(
                                CompanyID,
                                clsHcmApprovalDocuments.TypeEmployeeContract,
                                BranchID,
                                BasicSalary);
                            bool posted = documentStatus == (int)DocumentStatus.Posted;

                            if (posted && IsActive)
                            {
                                cls.DeactivateActiveContracts(EmployeeID, CompanyID, CreationUserID, trn);
                            }

                            int newId = cls.InsertEmployeeContract(
                                EmployeeID, ContractTypeID, JobTitleID, DepartmentID, BranchID,
                                Simulate.String(ContractNumber), StartDate, EndDate, IsOpenEnded,
                                ProbationMonths, WorkingHoursPerWeek, BasicSalary,
                                AnnualLeaveDaysPerYear, AnnualLeaveDaysAfter5Years,
                                SickLeaveFullPayDaysPerYear, SickLeaveExtendedDaysPerYear,
                                Simulate.String(Notes), IsActive,
                                CompanyID, CreationUserID, trn, documentStatus);

                            if (posted && IsActive)
                            {
                                string contractGuid = clsHcmApprovalDocuments.SelectGuidById(
                                    clsHcmApprovalDocuments.TypeEmployeeContract, newId, CompanyID, trn);
                                if (!string.IsNullOrWhiteSpace(contractGuid))
                                    clsHcmApprovalDocuments.PostDocument(
                                        clsHcmApprovalDocuments.TypeEmployeeContract,
                                        contractGuid, CreationUserID, CompanyID, trn);
                            }

                            trn.Commit();
                            return newId;
                        }
                        catch
                        {
                            trn.Rollback();
                            throw;
                        }
                    }
                }
            }
            catch (Exception)
            {
                throw;
            }
        }

        // ====================================================
        // POST: update
        // ====================================================
        [HttpPost]
        [Route("UpdateEmployeeContract")]
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
            int ModificationUserID)
        {
            try
            {
                clsEmployeeContract cls = new clsEmployeeContract();
                string existingGuid = clsHcmApprovalDocuments.SelectGuidById(
                    clsHcmApprovalDocuments.TypeEmployeeContract, ID, CompanyID);
                if (!string.IsNullOrWhiteSpace(existingGuid))
                {
                    int existingStatus = clsHcmApprovalDocuments.GetDocumentStatusByGuid(
                        clsHcmApprovalDocuments.TypeEmployeeContract, existingGuid, CompanyID);
                    if (existingStatus == (int)DocumentStatus.PendingApproval ||
                        existingStatus == (int)DocumentStatus.Posted)
                    {
                        throw new InvalidOperationException(
                            "This contract cannot be edited while pending approval or after posting.");
                    }
                }

                return cls.UpdateEmployeeContract(
                    ID, EmployeeID, ContractTypeID, JobTitleID, DepartmentID, BranchID,
                    Simulate.String(ContractNumber), StartDate, EndDate, IsOpenEnded,
                    ProbationMonths, WorkingHoursPerWeek, BasicSalary,
                    AnnualLeaveDaysPerYear, AnnualLeaveDaysAfter5Years,
                    SickLeaveFullPayDaysPerYear, SickLeaveExtendedDaysPerYear,
                    Simulate.String(Notes), IsActive, CompanyID, ModificationUserID);
            }
            catch (Exception)
            {
                throw;
            }
        }

        // ====================================================
        // POST: composite Hire Employee
        // Atomic: inserts the employee, the active contract and
        // (optionally) the BASIC salary element in one transaction.
        // ====================================================
        [HttpPost]
        [Route("HireEmployee")]
        public int HireEmployee(
            [FromBody] System.Text.Json.JsonElement data,
            // Employee personal
            string AName, string EName,
            string UserName, string Password, bool IsSystemUser,
            string Email, string Tel1, string Tel2,
            string EmployeeCode,
            string Address,
            int CountryID, int CityID, int NationalityID,
            string NationalNumber, string IDNumber,
            DateTime IDIssueDate, DateTime IDExpireDate,
            string PassportNumber, DateTime PassportIssueDate, DateTime PassportExpireDate,
            int EducationalLevelID,
            string BankName, string IBAN, string SWIFTCode, string BankAccountNumber,
            string SocialSecurityNumber, int SocialSecurityProgramID,
            string MedicalInsuranceNumber, int MedicalInsuranceProgramID,
            // Position / Contract
            int ContractTypeID, int JobTitleID, int DepartmentID, int BranchID,
            string ContractNumber,
            DateTime StartDate, DateTime EndDate, bool IsOpenEnded,
            int ProbationMonths,
            decimal WorkingHoursPerWeek,
            decimal BasicSalary,
            string Notes,
            // Leave (Jordan Labour Law statutory defaults if omitted: 14 / 21 / 14 / 14)
            int AnnualLeaveDaysPerYear,
            int AnnualLeaveDaysAfter5Years,
            int SickLeaveFullPayDaysPerYear,
            int SickLeaveExtendedDaysPerYear,
            // Options
            bool SeedBasicSalaryElement,
            // Audit
            int CompanyID, int CreationUserID
        )
        {
            try
            {
                if (CompanyID <= 0) throw new ArgumentException("CompanyID is required");

                // optional signature byte[]
                byte[] signature = Array.Empty<byte>();
                try
                {
                    if (data.ValueKind == JsonValueKind.Object &&
                        data.TryGetProperty("Signuture", out var sig) &&
                        sig.ValueKind == JsonValueKind.String)
                    {
                        string b64 = sig.GetString();
                        if (!string.IsNullOrEmpty(b64))
                        {
                            signature = Convert.FromBase64String(b64);
                        }
                    }
                }
                catch { /* tolerate malformed signature */ }

                clsSQL clsSQL = new clsSQL();
                using (SqlConnection con = new SqlConnection(clsSQL.CreateDataBaseConnectionString(CompanyID)))
                {
                    con.Open();
                    using (SqlTransaction trn = con.BeginTransaction())
                    {
                        try
                        {
                            // 1) Insert the employee
                            clsEmployee clsEmp = new clsEmployee();
                            int newEmployeeID = clsEmp.InsertEmployee(
                                Simulate.String(AName), Simulate.String(EName),
                                Simulate.String(UserName), Simulate.String(Password),
                                CompanyID, CreationUserID,
                                IsSystemUser, Simulate.String(Email), Simulate.String(Tel1),
                                Simulate.String(EmployeeCode), Simulate.String(Tel2),
                                Simulate.String(Address),
                                CountryID, CityID, NationalityID,
                                Simulate.String(NationalNumber), Simulate.String(IDNumber),
                                IDIssueDate, IDExpireDate,
                                Simulate.String(PassportNumber), PassportIssueDate, PassportExpireDate,
                                EducationalLevelID,
                                StartDate, // HireDate = contract start date
                                Simulate.String(BankName), Simulate.String(IBAN), Simulate.String(SWIFTCode),
                                Simulate.String(BankAccountNumber),
                                Simulate.String(SocialSecurityNumber), SocialSecurityProgramID,
                                Simulate.String(MedicalInsuranceNumber), MedicalInsuranceProgramID,
                                DepartmentID,
                                false,
                                signature,
                                trn);

                            if (newEmployeeID <= 0)
                            {
                                trn.Rollback();
                                throw new Exception("Failed to insert employee record");
                            }

                            // 2) Insert the active contract
                            clsEmployeeContract clsContract = new clsEmployeeContract();
                            int newContractID = clsContract.InsertEmployeeContract(
                                newEmployeeID, ContractTypeID, JobTitleID, DepartmentID, BranchID,
                                Simulate.String(ContractNumber), StartDate, EndDate, IsOpenEnded,
                                ProbationMonths, WorkingHoursPerWeek, BasicSalary,
                                AnnualLeaveDaysPerYear <= 0 ? 14 : AnnualLeaveDaysPerYear,
                                AnnualLeaveDaysAfter5Years <= 0 ? 21 : AnnualLeaveDaysAfter5Years,
                                SickLeaveFullPayDaysPerYear <= 0 ? 14 : SickLeaveFullPayDaysPerYear,
                                SickLeaveExtendedDaysPerYear <= 0 ? 14 : SickLeaveExtendedDaysPerYear,
                                Simulate.String(Notes),
                                true, // first contract for a hire is always active
                                CompanyID, CreationUserID, trn);

                            if (newContractID <= 0)
                            {
                                trn.Rollback();
                                throw new Exception("Failed to insert contract record");
                            }

                            // 3) Optionally seed the BASIC salary element
                            if (SeedBasicSalaryElement && BasicSalary > 0)
                            {
                                int basicElementID = clsContract.GetBasicSalaryElementID(CompanyID, trn);
                                if (basicElementID > 0)
                                {
                                    clsEmployeeSalaryElements clsEse = new clsEmployeeSalaryElements();

                                    DateTime ese_end = IsOpenEnded
                                        ? StartDate.AddYears(50)
                                        : EndDate;

                                    clsEse.InsertEmployeeSalaryElement(
                                        newEmployeeID,
                                        basicElementID,
                                        1, // Fixed Amount
                                        BasicSalary,
                                        false,
                                        StartDate,
                                        ese_end,
                                        true,
                                        CompanyID,
                                        CreationUserID,
                                        newContractID,
                                        true,
                                        trn);
                                }
                                // else: no BASIC element is configured for this company; silently skip
                            }

                            trn.Commit();
                            return newEmployeeID;
                        }
                        catch
                        {
                            try { trn.Rollback(); } catch { /* ignore */ }
                            throw;
                        }
                    }
                }
            }
            catch (Exception)
            {
                throw;
            }
        }
    }
}
