using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System;
using System.Data;
using WebApplication2.cls;

namespace WebApplication2.Controllers
{
    [Route("api/ctlSalariesElements")]
    public class ctlSalariesElements : Controller
    {
        // ==========================================================
        // SELECT
        // ==========================================================
        [HttpGet]
        [Route("SelectSalariesElements")]
        public string SelectSalariesElements(int ID, string Code, string AName, string EName, int CompanyID)
        {
            try
            {
                clsSalariesElements obj = new clsSalariesElements();

                DataTable dt = obj.SelectSalariesElements(
                    ID,
                    Simulate.String(Code),
                    Simulate.String(AName),
                    Simulate.String(EName),
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
        [Route("DeleteSalariesElementByID")]
        public bool DeleteSalariesElementByID(int ID, int CompanyID)
        {
            try
            {
                clsSalariesElements obj = new clsSalariesElements();
                return obj.DeleteSalariesElementByID(ID, CompanyID);
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
        [Route("InsertSalariesElement")]
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
            int CreationUserId
        )
        {
            try
            {
                clsSalariesElements obj = new clsSalariesElements();

                int newID = obj.InsertSalariesElement(
                    Simulate.String(Code),
                    Simulate.String(AName),
                    Simulate.String(EName),
                    ElementTypeID,
                    SalariesElementCategoryID,
                    CalcTypeID,
                    Simulate.Decimal(DefaultValue),
                    PercentageOfElementID,
                    Simulate.String(FormulaText),
                    IsTaxable,
                    IsAffectSocialSecurity,
                    IsRecurring,
                    IsSystemElement,
                    IsEditable,
                    StartDate,
                    EndDate,
                    EmployeeDebitAccountID,
                    EmployeeCreditAccountID,
                    CompanyDebitAccountID,
                    CompanyCreditAccountID,
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
        [Route("UpdateSalariesElement")]
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
            int CompanyID
        )
        {
            try
            {
                clsSalariesElements obj = new clsSalariesElements();

                int A = obj.UpdateSalariesElement(
                    ID,
                    Simulate.String(Code),
                    Simulate.String(AName),
                    Simulate.String(EName),
                    ElementTypeID,
                    SalariesElementCategoryID,
                    CalcTypeID,
                    Simulate.Decimal(DefaultValue),
                    PercentageOfElementID,
                    Simulate.String(FormulaText),
                    IsTaxable,
                    IsAffectSocialSecurity,
                    IsRecurring,
                    IsEditable,
                    StartDate,
                    EndDate,
                    EmployeeDebitAccountID,
                    EmployeeCreditAccountID,
                    CompanyDebitAccountID,
                    CompanyCreditAccountID,
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
