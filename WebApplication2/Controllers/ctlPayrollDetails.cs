using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System;
using System.Data;
using WebApplication2.cls;

namespace WebApplication2.Controllers
{
    [Route("api/ctlPayrollDetails")]
    public class ctlPayrollDetails : Controller
    {
        // ==========================================================
        // SELECT
        // ==========================================================
        [HttpGet]
        [Route("SelectPayrollDetails")]
        public string SelectPayrollDetails(int ID, int PayrollHeaderID, int SalaryElementID, int CompanyID)
        {
            try
            {
                clsPayrollDetails obj = new clsPayrollDetails();

                DataTable dt = obj.SelectPayrollDetails(
                    ID,
                    PayrollHeaderID,
                    SalaryElementID,
                    CompanyID
                );

                return dt != null ? JsonConvert.SerializeObject(dt) : "";
            }
            catch
            {
                throw;
            }
        }

        // ==========================================================
        // INSERT
        // ==========================================================
        [HttpGet]
        [Route("InsertPayrollDetails")]
        public int InsertPayrollDetails(
            int PayrollHeaderID,
            int SalaryElementID,
            int ElementTypeID,
            int CalcTypeID,
            decimal AssignedValue,
            decimal CalculatedAmount,
            int CompanyID,
            int CreationUserID
        )
        {
            try
            {
                clsPayrollDetails obj = new clsPayrollDetails();

                int newID = obj.InsertPayrollDetails(
                    PayrollHeaderID,
                    SalaryElementID,
                    ElementTypeID,
                    CalcTypeID,
                    Simulate.Decimal(AssignedValue),
                    Simulate.Decimal(CalculatedAmount),
                    CompanyID,
                    CreationUserID
                );

                return newID;
            }
            catch
            {
                throw;
            }
        }

        // ==========================================================
        // UPDATE
        // ==========================================================
        [HttpGet]
        [Route("UpdatePayrollDetails")]
        public int UpdatePayrollDetails(
            int ID,
            int PayrollHeaderID,
            int SalaryElementID,
            int ElementTypeID,
            int CalcTypeID,
            decimal AssignedValue,
            decimal CalculatedAmount,
            int ModificationUserID,
            int CompanyID
        )
        {
            try
            {
                clsPayrollDetails obj = new clsPayrollDetails();

                int A = obj.UpdatePayrollDetails(
                    ID,
                    PayrollHeaderID,
                    SalaryElementID,
                    ElementTypeID,
                    CalcTypeID,
                    Simulate.Decimal(AssignedValue),
                    Simulate.Decimal(CalculatedAmount),
                    ModificationUserID,
                    CompanyID
                );

                return A;
            }
            catch
            {
                throw;
            }
        }
    }
}
