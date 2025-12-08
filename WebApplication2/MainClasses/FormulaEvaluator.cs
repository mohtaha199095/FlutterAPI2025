using System;
using System.Collections.Generic;
using System.Data;
using System.Text.RegularExpressions;

namespace WebApplication2.cls
{
    public static class FormulaEvaluator
    {
        // =============================================================
        // Main Function — Safe Evaluate Formula
        // =============================================================
        public static decimal SafeEvaluate(string formula, Dictionary<string, decimal> variables)
        {
            if (string.IsNullOrWhiteSpace(formula))
                return 0;

            string processed = formula;

            // ---------------------------------------------------------
            // 1) Replace {VARIABLE} format
            // ---------------------------------------------------------
            processed = Regex.Replace(processed, @"\{([A-Za-z0-9_]+)\}", match =>
            {
                string varName = match.Groups[1].Value;

                if (variables.ContainsKey(varName))
                    return variables[varName].ToString();

                return "0";
            });

            // ---------------------------------------------------------
            // 2) Replace VARIABLE format (no braces)
            // ---------------------------------------------------------
            foreach (var kvp in variables)
            {
                processed = Regex.Replace(
                    processed,
                    $@"\b{Regex.Escape(kvp.Key)}\b",
                    kvp.Value.ToString()
                );
    
            }

            // ---------------------------------------------------------
            // 3) Final cleanup
            // ---------------------------------------------------------
            processed = processed.Replace("--", "+"); // prevent errors

            // ---------------------------------------------------------
            // 4) Evaluate safely using DataTable.Compute
            // ---------------------------------------------------------
            try
            {
                DataTable dt = new DataTable();
                var result = dt.Compute(processed, "");

                return Convert.ToDecimal(result);
            }
            catch
            {
                // Any error = return 0 (safe)
                return 0;
            }
        }
        public static string ReplaceVariables(string formula, Dictionary<string, decimal> vars)
        {
            string output = formula;

            foreach (var v in vars)
            {
                output = output.Replace("{" + v.Key + "}", v.Value.ToString());
            }

            return output;
        }
        public static decimal SafeEvaluate(string expression)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(expression))
                    return 0;

                // DataTable.Compute safely handles arithmetic expressions
                DataTable dt = new DataTable();
                dt.Locale = System.Globalization.CultureInfo.InvariantCulture;

                var result = dt.Compute(expression, "");

                return Convert.ToDecimal(result);
            }
            catch (Exception ex)
            {
                throw new Exception("Formula error: " + ex.Message);
            }
        }

    }
}
