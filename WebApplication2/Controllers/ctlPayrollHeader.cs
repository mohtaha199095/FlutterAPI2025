using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System;
using System.Data;
using WebApplication2.cls;

namespace WebApplication2.Controllers
{
    [Route("api/ctlPayrollHeader")]
    public class ctlPayrollHeader : Controller
    {
        // ==========================================================
        // SELECT
        // ==========================================================
        [HttpGet]
        [Route("SelectPayrollHeader")]
        public string SelectPayrollHeader(
            int ID,
            int PayrollPeriodID,
            int EmployeeID,
            int Status,
            int CompanyID)
        {
            try
            {
                clsPayrollHeader obj = new clsPayrollHeader();

                DataTable dt = obj.SelectPayrollHeader(
                    ID,
                    PayrollPeriodID,
                    EmployeeID,
                    Status,
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
        // DELETE
        // ==========================================================
        [HttpGet]
        [Route("DeletePayrollHeaderByID")]
        public bool DeletePayrollHeaderByID(int ID, int CompanyID)
        {
            try
            {
                clsPayrollHeader obj = new clsPayrollHeader();
                return obj.DeletePayrollHeader(ID, CompanyID);
            }
            catch
            {
                throw;
            }
        }

        // ==========================================================
        // INSERT
        // ==========================================================
        //[HttpGet]
        //[Route("InsertPayrollHeader")]
        //public int InsertPayrollHeader(
        //    int PayrollPeriodID,
        //    int EmployeeID,
        //    decimal BasicSalary,
        //    decimal TotalEarnings,
        //    decimal TotalDeductions,
        //    decimal NetSalary,
        //    int Status,
        //    int CompanyID,
        //    int CreationUserID)
        //{
        //    try
        //    {
        //        clsPayrollHeader obj = new clsPayrollHeader();

        //        int newID = obj.InsertPayrollHeader(
        //            PayrollPeriodID,
        //            EmployeeID,
        //            Simulate.Decimal(BasicSalary),
        //            Simulate.Decimal(TotalEarnings),
        //            Simulate.Decimal(TotalDeductions),
        //            Simulate.Decimal(NetSalary),
        //            Status,
        //            CompanyID,
        //            CreationUserID
        //        );

        //        return newID;
        //    }
        //    catch
        //    {
        //        throw;
        //    }
        //}

        // ==========================================================
        // UPDATE
        // ==========================================================
        [HttpGet]
        [Route("UpdatePayrollHeader")]
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
            int CompanyID)
        {
            try
            {
                clsPayrollHeader obj = new clsPayrollHeader();

                int A = obj.UpdatePayrollHeader(
                    ID,
                    PayrollPeriodID,
                    EmployeeID,
                    Simulate.Decimal(BasicSalary),
                    Simulate.Decimal(TotalEarnings),
                    Simulate.Decimal(TotalDeductions),
                    Simulate.Decimal(NetSalary),
                    Status,
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
