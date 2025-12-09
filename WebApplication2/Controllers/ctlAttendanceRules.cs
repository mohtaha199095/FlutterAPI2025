using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System;
using WebApplication2.cls;

namespace WebApplication2.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class ctlAttendanceRules : ControllerBase
    {
        // ============================================================
        // SELECT RULES
        // ============================================================
        [HttpGet]
        [Route("SelectAttendanceRules")]
        public string SelectAttendanceRules(int ID, string RuleName, int RuleTypeID, int CompanyID)
        {
            try
            {
                clsAttendanceRules cls = new clsAttendanceRules();
                var dt = cls.SelectAttendanceRules(ID,Simulate.String( RuleName), RuleTypeID, CompanyID);

                return dt != null ? JsonConvert.SerializeObject(dt) : "";
            }
            catch (Exception)
            {
                throw;
            }
        }

        // ============================================================
        // DELETE RULE
        // ============================================================
        [HttpPost]
        [Route("DeleteAttendanceRuleByID")]
        public bool DeleteAttendanceRuleByID(int ID, int CompanyID)
        {
            try
            {
                clsAttendanceRules cls = new clsAttendanceRules();
                return cls.DeleteAttendanceRuleByID(ID, CompanyID);
            }
            catch (Exception)
            {
                throw;
            }
        }

        // ============================================================
        // INSERT RULE
        // ============================================================
        [HttpPost]
        [Route("InsertAttendanceRule")]
        public int InsertAttendanceRule(
            string RuleName,
            int RuleGroupID,
            int RuleTypeID,
            int CalculationTypeID,
            decimal Value,
            string FormulaText,
            int SalaryElementID,
            decimal MinAmount,
            decimal MaxAmount,
            int RoundTypeID,
            bool IsActive,
            int CompanyID,
            int CreationUserID
        )
        {
            try
            {
                clsAttendanceRules cls = new clsAttendanceRules();

                return cls.InsertAttendanceRule(
                    RuleName,
                    RuleGroupID,
                    RuleTypeID,
                    CalculationTypeID,
                    Value,
                    FormulaText,
                    SalaryElementID,
                    MinAmount,
                    MaxAmount,
                    RoundTypeID,
                    IsActive,
                    CompanyID,
                    CreationUserID
                );
            }
            catch (Exception)
            {
                throw;
            }
        }

        // ============================================================
        // UPDATE RULE
        // ============================================================
        [HttpPost]
        [Route("UpdateAttendanceRule")]
        public int UpdateAttendanceRule(
            int ID,
            string RuleName,
            int RuleGroupID,
            int RuleTypeID,
            int CalculationTypeID,
            decimal Value,
            string FormulaText,
            int SalaryElementID,
            decimal MinAmount,
            decimal MaxAmount,
            int RoundTypeID,
            bool IsActive,
            int ModificationUserID,
            int CompanyID
        )
        {
            try
            {
                clsAttendanceRules cls = new clsAttendanceRules();

                return cls.UpdateAttendanceRule(
                    ID,
                    RuleName,
                    RuleGroupID,
                    RuleTypeID,
                    CalculationTypeID,
                    Value,
                    FormulaText,
                    SalaryElementID,
                    MinAmount,
                    MaxAmount,
                    RoundTypeID,
                    IsActive,
                    ModificationUserID,
                    CompanyID
                );
            }
            catch (Exception)
            {
                throw;
            }
        }
    }
}
