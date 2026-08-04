using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using WebApplication2.cls;

namespace WebApplication2.Controllers
{
    [Route("api/ctlPayrollFormula")]
    public class ctlPayrollFormula : Controller
    {
        // ============================================================
        // VALIDATE FORMULA
        // ============================================================
        [HttpGet]
        [Route("Validate")]
        public string ValidateFormula(string Formula, int CompanyID)
        {
            try
            {
                // -----------------------------
                // 1) Basic input validation
                // -----------------------------
                if (string.IsNullOrWhiteSpace(Formula))
                    return JsonConvert.SerializeObject(new
                    {
                        success = false,
                        message = "Formula is empty."
                    });

                // -----------------------------
                // 2) Check illegal characters
                // -----------------------------
                if (!IsValidCharacters(Formula))
                    return JsonConvert.SerializeObject(new
                    {
                        success = false,
                        message = "Formula contains invalid characters."
                    });

                // -----------------------------
                // 3) Check parentheses balance
                // -----------------------------
                if (!IsParenthesesBalanced(Formula))
                    return JsonConvert.SerializeObject(new
                    {
                        success = false,
                        message = "Parentheses are not balanced."
                    });

                // -----------------------------
                // 4) Load variables from Salaries Elements
                // -----------------------------
                clsSalariesElements el = new clsSalariesElements();
                DataTable dt = el.SelectSalariesElements(0, "", "", "", CompanyID);

                Dictionary<string, decimal> testVariables = new Dictionary<string, decimal>();

                foreach (DataRow row in dt.Rows)
                {
                    string code = row["Code"].ToString().ToUpper();
                    if (!testVariables.ContainsKey(code))
                        testVariables.Add(code, 100); // default dummy value
                }

                // Add predefined system variables
                testVariables["BASIC"] = 500;
                testVariables["GROSS"] = 800;
                testVariables["NET"] = 700;
                testVariables["OT_HOURS"] = 5;
                testVariables["WORKING_DAYS"] = 22;

                // -----------------------------
                // 5) Replace variables in formula
                // -----------------------------
                string parsedFormula = FormulaEvaluator.ReplaceVariables(Formula, testVariables);

                // -----------------------------
                // 6) Evaluate safely
                // -----------------------------
                decimal result = FormulaEvaluator.SafeEvaluate(parsedFormula);

                // -----------------------------
                // SUCCESS
                // -----------------------------
                return JsonConvert.SerializeObject(new
                {
                    success = true,
                    message = "Formula is valid.",
                    testResult = result
                });
            }
            catch (Exception ex)
            {
                return JsonConvert.SerializeObject(new
                {
                    success = false,
                    message = ex.Message
                });
            }
        }

        // ============================================================
        // GET FORMULA VARIABLE SCHEMA
        // Publishes the supported variable set so client and server stay
        // in sync. Combines built-in system variables (BASIC, GROSS, NET,
        // OT_HOURS, WORKING_DAYS) plus every active salary element Code
        // and every active attendance rule code for the company.
        // ============================================================
        [HttpGet]
        [Route("GetVariables")]
        public string GetVariables(int CompanyID)
        {
            try
            {
                List<object> vars = new List<object>();

                // 1) Built-in system variables
                vars.Add(new { code = "BASIC", source = "system", description = "Employee basic salary" });
                vars.Add(new { code = "GROSS", source = "system", description = "Sum of earnings before deductions" });
                vars.Add(new { code = "NET", source = "system", description = "Net pay (gross minus deductions)" });
                vars.Add(new { code = "OT_HOURS", source = "system", description = "Total overtime hours" });
                vars.Add(new { code = "WORKING_DAYS", source = "system", description = "Number of working days in the period" });

                // 2) Salary element codes
                clsSalariesElements el = new clsSalariesElements();
                DataTable dtEl = el.SelectSalariesElements(0, "", "", "", CompanyID);
                if (dtEl != null)
                {
                    foreach (DataRow row in dtEl.Rows)
                    {
                        string code = Simulate.String(row["Code"]).ToUpper();
                        if (string.IsNullOrWhiteSpace(code)) continue;

                        string name = "";
                        if (dtEl.Columns.Contains("EName"))
                            name = Simulate.String(row["EName"]);
                        if (string.IsNullOrWhiteSpace(name) && dtEl.Columns.Contains("AName"))
                            name = Simulate.String(row["AName"]);

                        vars.Add(new { code, source = "salary_element", description = name });
                    }
                }

                // 3) Attendance rule codes (if rules table provides Code)
                clsAttendanceRules ar = new clsAttendanceRules();
                DataTable dtAr = ar.SelectAttendanceRules(0, "", 0, CompanyID);
                if (dtAr != null && dtAr.Columns.Contains("RuleName"))
                {
                    foreach (DataRow row in dtAr.Rows)
                    {
                        string code = Simulate.String(row["RuleName"]).ToUpper().Replace(' ', '_');
                        if (string.IsNullOrWhiteSpace(code)) continue;
                        vars.Add(new { code, source = "attendance_rule", description = Simulate.String(row["RuleName"]) });
                    }
                }

                return JsonConvert.SerializeObject(new
                {
                    success = true,
                    variables = vars
                });
            }
            catch (Exception ex)
            {
                return JsonConvert.SerializeObject(new
                {
                    success = false,
                    message = ex.Message
                });
            }
        }

        // ======================================================================
        // VALIDATION HELPERS
        // ======================================================================

        private bool IsValidCharacters(string f)
        {
            foreach (char c in f)
            {
                if (!(char.IsLetterOrDigit(c) ||
                      "+-*/{}()._ ".Contains(c)))
                    return false;
            }
            return true;
        }

        private bool IsParenthesesBalanced(string f)
        {
            int count = 0;
            foreach (char c in f)
            {
                if (c == '(') count++;
                if (c == ')') count--;
                if (count < 0) return false; // ) before (
            }
            return count == 0;
        }
    }
}
