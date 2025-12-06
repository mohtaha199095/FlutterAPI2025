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
