using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System;
using System.Data;
using WebApplication2.cls;

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
            int CompanyID)
        {
            try
            {
                clsEmployeeSalaryElements obj = new clsEmployeeSalaryElements();

                DataTable dt = obj.SelectEmployeeSalaryElements(
                    ID,
                    EmployeeID,
                    SalaryElementID,
                    IsActive,
                    CompanyID
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

        // ==========================================================
        // DELETE
        // ==========================================================
        [HttpGet]
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

        // ==========================================================
        // INSERT
        // ==========================================================
        [HttpGet]
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
            int CreationUserId
        )
        {
            try
            {
                clsEmployeeSalaryElements obj = new clsEmployeeSalaryElements();

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
                    CreationUserId
                );

                return newID;
            }
            catch (Exception)
            {
                throw;
            }
        }

        // ==========================================================
        // UPDATE
        // ==========================================================
        [HttpGet]
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
            int CompanyID
        )
        {
            try
            {
                clsEmployeeSalaryElements obj = new clsEmployeeSalaryElements();

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
                    CompanyID
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
