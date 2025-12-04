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
    }
}
