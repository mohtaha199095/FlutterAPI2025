using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System;
using System.Data;
using WebApplication2.cls;

namespace WebApplication2.Controllers
{
    [Route("api/ctlPayrollPeriod")]
    public class ctlPayrollPeriod : Controller
    {
        // ==========================================================
        // SELECT
        // ==========================================================
        [HttpGet]
        [Route("SelectPayrollPeriod")]
        public string SelectPayrollPeriod(int ID, string AName, int IsClosed, int CompanyID)
        {
            try
            {
                clsPayrollPeriod obj = new clsPayrollPeriod();

                DataTable dt = obj.SelectPayrollPeriod(
                    ID,  Simulate.String( AName),   IsClosed,
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
        [Route("InsertPayrollPeriod")]
        public int InsertPayrollPeriod(
            string AName,
            string EName,
            DateTime StartDate,
            DateTime EndDate, bool IsClosed,
            int CompanyID,
            int CreationUserID
        )
        {
            try
            {
                clsPayrollPeriod obj = new clsPayrollPeriod();

                int newID = obj.InsertPayrollPeriod(
                    Simulate.String(AName),
                    Simulate.String(EName),
                  
                    StartDate,
                    EndDate, IsClosed,
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
        [Route("UpdatePayrollPeriod")]
        public int UpdatePayrollPeriod(
            int ID,
            string AName,
            string EName,
            DateTime StartDate,
            DateTime EndDate, bool IsClosed,
            int ModificationUserID,
            int CompanyID
        )
        {
            try
            {
                clsPayrollPeriod obj = new clsPayrollPeriod();

                int A = obj.UpdatePayrollPeriod(
                    ID,
                    Simulate.String(AName),
                    Simulate.String(EName),
                    StartDate,
                    EndDate, IsClosed,
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
