using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System;
using System.Data;
using WebApplication2.cls;

namespace WebApplication2.Controllers
{
    [Route("api/ctlEmployeeShiftAssignment")]
    public class ctlEmployeeShiftAssignment : Controller
    {
        // ==========================================================
        // SELECT ALL BY EMPLOYEE
        // ==========================================================
        [HttpGet]
        [Route("SelectAll")]
        public string SelectAll(int EmployeeID, int CompanyID)
        {
            try
            {
                clsEmployeeShiftAssignment obj = new clsEmployeeShiftAssignment();

                DataTable dt = obj.SelectAll(EmployeeID, CompanyID);

                return dt != null ? JsonConvert.SerializeObject(dt) : "";
            }
            catch
            {
                throw;
            }
        }

        // ==========================================================
        // SELECT BY ID
        // ==========================================================
        [HttpGet]
        [Route("SelectByID")]
        public string SelectByID(int ID,int employeeID, int CompanyID)
        {
            try
            {
                clsEmployeeShiftAssignment obj = new clsEmployeeShiftAssignment();

                DataTable dt = obj.SelectByID(ID, employeeID, CompanyID);

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
        [Route("Insert")]
        public int Insert(
            int EmployeeID,
            int ShiftID,
            int WeekDay,
            string StartDate,
            string EndDate,
            bool IsActive,
            int CompanyID,
            int CreationUserID
        )
        {
            try
            {
                clsEmployeeShiftAssignment obj = new clsEmployeeShiftAssignment();

                int newID = obj.Insert(
                    EmployeeID,
                    ShiftID,
                    WeekDay,
                    Simulate.String(StartDate),
                    Simulate.String(EndDate),
                    IsActive,
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
        [Route("Update")]
        public int Update(
            int ID,
            int EmployeeID,
            int ShiftID,
            int WeekDay,
            string StartDate,
            string EndDate,
            bool IsActive,
            int CompanyID,
            int ModificationUserID
        )
        {
            try
            {
                clsEmployeeShiftAssignment obj = new clsEmployeeShiftAssignment();

                int A = obj.Update(
                    ID,
                    EmployeeID,
                    ShiftID,
                    WeekDay,
                    Simulate.String(StartDate),
                    Simulate.String(EndDate),
                    IsActive,
                    CompanyID,
                    ModificationUserID
                );

                return A;
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
        [Route("Delete")]
        public int Delete(int ID, int CompanyID)
        {
            try
            {
                clsEmployeeShiftAssignment obj = new clsEmployeeShiftAssignment();

                int A = obj.Delete(ID, CompanyID);

                return A;
            }
            catch
            {
                throw;
            }
        }
    }
}
