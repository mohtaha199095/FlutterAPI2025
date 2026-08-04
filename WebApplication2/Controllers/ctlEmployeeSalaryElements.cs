using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System;
using System.Data;
using WebApplication2.cls;
using static WebApplication2.MainClasses.clsEnum;

namespace WebApplication2.Controllers
{
    [Route("api/ctlEmployeeSalaryElements")]
    public class ctlEmployeeSalaryElements : Controller
    {
        // ==========================================================
        // SELECT
        // ==========================================================
        [HttpGet]
        [Route("SelectEmployeeSalaryElements")]
        public string SelectEmployeeSalaryElements(
            int ID,
            int EmployeeID,
            int SalaryElementID,
            int IsActive,
            int CompanyID,
            int FilterContractID = 0)
        {
            try
            {
                clsEmployeeSalaryElements obj = new clsEmployeeSalaryElements();

                DataTable dt = obj.SelectEmployeeSalaryElements(
                    ID,
                    EmployeeID,
                    SalaryElementID,
                    IsActive,
                    CompanyID,
                    FilterContractID
                );

                if (dt != null)
                {
                    return JsonConvert.SerializeObject(dt);
                }
                else
                {
                    return "";
                }
            }
            catch (Exception)
            {
                throw;
            }
        }

        /// <summary>Salary lines linked to one employment contract (includes element names).</summary>
        [HttpGet]
        [Route("SelectEmployeeSalaryElementsForContract")]
        public string SelectEmployeeSalaryElementsForContract(
            int EmployeeID,
            int EmployeeContractID,
            int CompanyID,
            bool IncludeInactive = true)
        {
            try
            {
                clsEmployeeSalaryElements obj = new clsEmployeeSalaryElements();
                DataTable dt = obj.SelectEmployeeSalaryElementsForContract(
                    EmployeeID,
                    EmployeeContractID,
                    CompanyID,
                    IncludeInactive);
                return dt != null ? JsonConvert.SerializeObject(dt) : "";
            }
            catch (Exception)
            {
                throw;
            }
        }

        // ==========================================================
        // DELETE
        // ==========================================================
        [HttpPost]
        [Route("DeleteEmployeeSalaryElementByID")]
        public bool DeleteEmployeeSalaryElementByID(int ID, int CompanyID)
        {
            try
            {
                clsEmployeeSalaryElements obj = new clsEmployeeSalaryElements();
                return obj.DeleteEmployeeSalaryElementByID(ID, CompanyID);
            }
            catch (Exception)
            {
                throw;
            }
        }

        /// <summary>End assignment by date and deactivate (row kept for audit).</summary>
        [HttpPost]
        [Route("SoftEndEmployeeSalaryElement")]
        public int SoftEndEmployeeSalaryElement(
            int ID,
            DateTime EndDate,
            int CompanyID,
            int ModificationUserId)
        {
            try
            {
                clsEmployeeSalaryElements obj = new clsEmployeeSalaryElements();
                return obj.SoftEndEmployeeSalaryElement(ID, EndDate, CompanyID, ModificationUserId);
            }
            catch (Exception)
            {
                throw;
            }
        }

        // ==========================================================
        // INSERT
        // ==========================================================
        [HttpPost]
        [Route("InsertEmployeeSalaryElement")]
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
            int EmployeeContractID = 0,
            bool IncludeOnContractPrint = true
        )
        {
            try
            {
                clsEmployeeSalaryElements obj = new clsEmployeeSalaryElements();
                clsApprovalEngine approvalEngine = new clsApprovalEngine();
                int documentStatus = approvalEngine.ResolveInitialDocumentStatus(
                    CompanyID,
                    clsHcmApprovalDocuments.TypeEmployeeSalaryElement,
                    0,
                    AssignedValue);

                int newID = obj.InsertEmployeeSalaryElement(
                    EmployeeID,
                    SalaryElementID,
                    CalcTypeID,
                    Simulate.Decimal(AssignedValue),
                    IsCalculated,
                    StartDate,
                    EndDate,
                    IsActive,
                    CompanyID,
                    CreationUserId,
                    EmployeeContractID,
                    IncludeOnContractPrint,
                    null,
                    documentStatus);

                if (documentStatus == (int)DocumentStatus.Posted && newID > 0 && IsActive)
                {
                    string guid = clsHcmApprovalDocuments.SelectGuidById(
                        clsHcmApprovalDocuments.TypeEmployeeSalaryElement, newID, CompanyID);
                    if (!string.IsNullOrWhiteSpace(guid))
                        clsHcmApprovalDocuments.PostDocument(
                            clsHcmApprovalDocuments.TypeEmployeeSalaryElement,
                            guid, CreationUserId, CompanyID, null);
                }

                return newID;
            }
            catch (Exception)
            {
                throw;
            }
        }

        // ==========================================================
        // UPDATE (EmployeeContractID: omit/null = leave column unchanged; 0 = NULL)
        // ==========================================================
        [HttpPost]
        [Route("UpdateEmployeeSalaryElement")]
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
            int? EmployeeContractID = null,
            bool IncludeOnContractPrint = true
        )
        {
            try
            {
                clsEmployeeSalaryElements obj = new clsEmployeeSalaryElements();

                int contractArg = EmployeeContractID ?? -1;

                int A = obj.UpdateEmployeeSalaryElement(
                    ID,
                    EmployeeID,
                    SalaryElementID,
                    CalcTypeID,
                    Simulate.Decimal(AssignedValue),
                    IsCalculated,
                    StartDate,
                    EndDate,
                    IsActive,
                    ModificationUserId,
                    CompanyID,
                    contractArg,
                    IncludeOnContractPrint
                );

                return A;
            }
            catch (Exception)
            {
                throw;
            }
        }
    }
}
