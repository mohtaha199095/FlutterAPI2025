using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using WebApplication2.DataBaseTable;

namespace WebApplication2.cls
{
    public class clsPayrollEngine
    {
        // ElementTypeID: 1=Earning, 2=Deduction, 3=Employer Contribution
        public const int ElementTypeEarning = 1;
        public const int ElementTypeDeduction = 2;
        public const int ElementTypeEmployerContribution = 3;

        public DataTable PreviewPayrollAll(int PayrollPeriodID, int DepartmentID, int CompanyID)
        {
            try
            {
                clsSQL cls = new clsSQL();

                clsPayrollPeriod per = new clsPayrollPeriod();
                DataTable dtPeriod = per.SelectPayrollPeriod(PayrollPeriodID, "", -1, CompanyID);

                if (dtPeriod.Rows.Count == 0)
                    throw new Exception("Invalid Payroll Period");

                SqlParameter[] prm =
                {
                    new SqlParameter("@DepartmentID", DepartmentID),
                    new SqlParameter("@CompanyID", CompanyID)
                };

                string sqlEmp = @"
                    SELECT bp.ID AS EmployeeID,
                           bp.AName AS EmployeeName,
                           dep.AName AS DepartmentName
                    FROM tbl_employee bp
                    LEFT JOIN tbl_Department dep ON dep.ID = bp.DepartmentID
                    WHERE (bp.DepartmentID = @DepartmentID OR @DepartmentID = 0)
                      AND bp.CompanyID = @CompanyID";

                DataTable dtEmployees = cls.ExecuteQueryStatement(
                    sqlEmp,
                    cls.CreateDataBaseConnectionString(CompanyID), prm
                );

                DataTable result = new DataTable();
                result.Columns.Add("EmployeeID", typeof(int));
                result.Columns.Add("EmployeeName");
                result.Columns.Add("DepartmentName");
                result.Columns.Add("BasicSalary", typeof(decimal));
                result.Columns.Add("TotalEarnings", typeof(decimal));
                result.Columns.Add("TotalDeductions", typeof(decimal));
                result.Columns.Add("NetSalary", typeof(decimal));
                result.Columns.Add("IsPosted", typeof(bool));

                decimal totalBasic = 0, totalEarn = 0, totalDed = 0, totalNet = 0;

                foreach (DataRow empRow in dtEmployees.Rows)
                {
                    int empID = Convert.ToInt32(empRow["EmployeeID"]);
                    string empName = empRow["EmployeeName"].ToString();
                    string depName = empRow["DepartmentName"].ToString();

                    PayrollPreviewResult preview =
                        PreviewPayroll(empID, PayrollPeriodID, CompanyID);

                    result.Rows.Add(
                        empID, empName, depName,
                        preview.BasicSalary,
                        preview.TotalEarnings,
                        preview.TotalDeductions,
                        preview.NetSalary,
                        preview.IsPosted
                    );

                    totalBasic += preview.BasicSalary;
                    totalEarn += preview.TotalEarnings;
                    totalDed += preview.TotalDeductions;
                    totalNet += preview.NetSalary;
                }

                result.Rows.Add(
                    -1, "TOTAL", "ALL DEPARTMENTS",
                    totalBasic, totalEarn, totalDed, totalNet, false
                );

                return result;
            }
            catch
            {
                throw;
            }
        }

        public PayrollPreviewResult PreviewPayroll(int EmployeeID, int PayrollPeriodID, int CompanyID)
        {
            try
            {
                clsEmployeeSalaryElements emp = new clsEmployeeSalaryElements();
                DataTable dtAssigned =
                    emp.SelectEmployeeSalaryElementsForCalculation(EmployeeID, PayrollPeriodID, CompanyID);

                clsSalariesElements master = new clsSalariesElements();
                DataTable dtMaster = master.SelectSalariesElements(0, "", "", "", CompanyID);

                Dictionary<int, SalariesElementModel> elementMap = BuildElementMap(dtMaster);

                var variables = new Dictionary<string, decimal>(StringComparer.OrdinalIgnoreCase);

                clsAttendanceRuleExecutor attendanceExec = new clsAttendanceRuleExecutor();
                List<PayrollImpactItem> attendanceItems =
                    attendanceExec.ExecuteRulesForEmployee(EmployeeID, PayrollPeriodID, CompanyID);

                foreach (var att in attendanceItems)
                {
                    if (!string.IsNullOrWhiteSpace(att.Code))
                    {
                        if (variables.ContainsKey(att.Code))
                            variables[att.Code] += att.Amount;
                        else
                            variables[att.Code] = att.Amount;
                    }
                }

                List<PayrollDetailModel> details = new List<PayrollDetailModel>();
                bool IsPosted = false;

                foreach (DataRow row in dtAssigned.Rows)
                {
                    int ElementTypeID = Simulate.Integer32(row["ElementTypeID"]);
                    int salaryElementID = Simulate.Integer32(row["SalaryElementID"]);
                    decimal assignedValue = Simulate.decimal_(row["ProratedAmount"]);
                    int calcType = Simulate.Integer32(row["CalcTypeID"]);
                    string basicCode = Simulate.String(row["Code"]);
                    IsPosted = Simulate.Bool(row["IsPosted"]);

                    if (!elementMap.ContainsKey(salaryElementID))
                        continue;

                    var masterElement = elementMap[salaryElementID];

                    decimal finalAmount = 0;

                    if (calcType == 1)
                        finalAmount = assignedValue;
                    else if (calcType == 2)
                    {
                        int baseElementID = masterElement.PercentageOfElementID;

                        if (baseElementID > 0 &&
                            elementMap.ContainsKey(baseElementID) &&
                            variables.ContainsKey(elementMap[baseElementID].Code))
                        {
                            decimal baseValue = variables[elementMap[baseElementID].Code];
                            finalAmount = baseValue * (assignedValue / 100m);
                        }
                    }
                    else if (calcType == 3)
                    {
                        finalAmount = FormulaEvaluator.SafeEvaluate(
                            masterElement.FormulaText,
                            variables
                        );
                    }

                    variables[masterElement.Code] = finalAmount;

                    details.Add(new PayrollDetailModel
                    {
                        SalaryElementID = salaryElementID,
                        ElementName = masterElement.AName,
                        Amount = finalAmount,
                        ElementTypeID = ElementTypeID,
                        BasicSalaryCode = basicCode,
                        IsAffectSocialSecurity = masterElement.IsAffectSocialSecurity,
                        IsTaxable = masterElement.IsTaxable
                    });
                }

                // Statutory (Jordan / country pack) — applied after base elements
                var statutory = new clsPayrollStatutoryEngine();
                statutory.ApplyStatutory(EmployeeID, PayrollPeriodID, CompanyID, details, variables, elementMap);

                // Loan installments due in period
                var loanBridge = new clsPayrollLoanBridge();
                loanBridge.ApplyDueLoans(EmployeeID, PayrollPeriodID, CompanyID, details, variables);

                // Unpaid / extended sick leave deductions
                new clsHrLeavePayrollBridge().ApplyLeaveAdjustments(
                    EmployeeID, PayrollPeriodID, CompanyID, details, variables, elementMap);

                return BuildSummary(details, attendanceItems, variables, IsPosted);
            }
            catch
            {
                throw;
            }
        }

        public static PayrollPreviewResult BuildSummary(
            List<PayrollDetailModel> details,
            List<PayrollImpactItem> attendanceItems,
            Dictionary<string, decimal> variables,
            bool isPosted)
        {
            decimal basic = 0;
            if (variables != null && variables.ContainsKey("BASIC"))
                basic = variables["BASIC"];

            decimal earnings = 0;
            decimal deductions = 0;
            decimal employerContributions = 0;

            foreach (var d in details)
            {
                ClassifyAmount(d.ElementTypeID, d.BasicSalaryCode, d.Amount,
                    ref basic, ref earnings, ref deductions, ref employerContributions);
            }

            foreach (var a in attendanceItems)
            {
                string code = a.Code ?? "";
                ClassifyAmount(a.ElementTypeID, code, a.Amount,
                    ref basic, ref earnings, ref deductions, ref employerContributions);
            }

            // BASIC already counted separately — do not double-count from details
            // Recompute basic from BASIC-coded earning if present
            foreach (var d in details)
            {
                if (IsBasicCode(d.BasicSalaryCode) && d.ElementTypeID == ElementTypeEarning)
                    basic = d.Amount;
            }

            earnings = 0;
            deductions = 0;
            employerContributions = 0;
            foreach (var d in details)
            {
                if (IsBasicCode(d.BasicSalaryCode)) continue;
                switch (d.ElementTypeID)
                {
                    case ElementTypeEarning:
                        earnings += d.Amount;
                        break;
                    case ElementTypeDeduction:
                        deductions += Math.Abs(d.Amount);
                        break;
                    case ElementTypeEmployerContribution:
                        employerContributions += Math.Abs(d.Amount);
                        break;
                    default:
                        // Unknown types treated as deduction to be safe for net pay
                        deductions += Math.Abs(d.Amount);
                        break;
                }
            }

            foreach (var a in attendanceItems)
            {
                if (IsBasicCode(a.Code)) continue;
                switch (a.ElementTypeID)
                {
                    case ElementTypeEarning:
                        earnings += a.Amount;
                        break;
                    case ElementTypeDeduction:
                        deductions += Math.Abs(a.Amount);
                        break;
                    case ElementTypeEmployerContribution:
                        employerContributions += Math.Abs(a.Amount);
                        break;
                    default:
                        deductions += Math.Abs(a.Amount);
                        break;
                }
            }

            decimal net = basic + earnings - deductions;

            return new PayrollPreviewResult
            {
                BasicSalary = basic,
                TotalEarnings = earnings,
                TotalDeductions = deductions,
                EmployerContributions = employerContributions,
                NetSalary = net,
                IsPosted = isPosted,
                SalaryElements = details,
                AttendanceElements = attendanceItems
            };
        }

        private static void ClassifyAmount(
            int elementTypeId, string code, decimal amount,
            ref decimal basic, ref decimal earnings, ref decimal deductions, ref decimal employer)
        {
            // Used only in intermediate path; final summary recomputes cleanly.
        }

        public static bool IsBasicCode(string code)
        {
            return string.Equals(code, "BASIC", StringComparison.OrdinalIgnoreCase);
        }

        private Dictionary<int, SalariesElementModel> BuildElementMap(DataTable dt)
        {
            var map = new Dictionary<int, SalariesElementModel>();

            foreach (DataRow row in dt.Rows)
            {
                var el = new SalariesElementModel
                {
                    ID = Convert.ToInt32(row["ID"]),
                    Code = row["Code"].ToString(),
                    AName = row["AName"].ToString(),
                    CalcTypeID = Convert.ToInt32(row["CalcTypeID"]),
                    FormulaText = row["FormulaText"] == DBNull.Value ? "" : row["FormulaText"].ToString(),
                    PercentageOfElementID = row.Table.Columns.Contains("PercentageOfElementID")
                        ? Simulate.Integer32(row["PercentageOfElementID"]) : 0,
                    ElementTypeID = row.Table.Columns.Contains("ElementTypeID")
                        ? Simulate.Integer32(row["ElementTypeID"]) : ElementTypeEarning,
                    IsAffectSocialSecurity = row.Table.Columns.Contains("IsAffectSocialSecurity")
                        && Simulate.Bool(row["IsAffectSocialSecurity"]),
                    IsTaxable = row.Table.Columns.Contains("IsTaxable")
                        && Simulate.Bool(row["IsTaxable"]),
                    DebitAccountID = row.Table.Columns.Contains("DebitAccountID")
                        ? Simulate.Integer32(row["DebitAccountID"]) : 0,
                    CreditAccountID = row.Table.Columns.Contains("CreditAccountID")
                        ? Simulate.Integer32(row["CreditAccountID"]) : 0
                };

                map[el.ID] = el;
            }

            return map;
        }
    }

    public class PayrollPreviewResult
    {
        public decimal BasicSalary { get; set; }
        public decimal TotalEarnings { get; set; }
        public decimal TotalDeductions { get; set; }
        public decimal EmployerContributions { get; set; }
        public decimal NetSalary { get; set; }
        public bool IsPosted { get; set; }

        public List<PayrollDetailModel> SalaryElements { get; set; }
        public List<PayrollImpactItem> AttendanceElements { get; set; }
    }

    public class PayrollDetailModel
    {
        public int SalaryElementID { get; set; }
        public string ElementName { get; set; }
        public decimal Amount { get; set; }
        public int ElementTypeID { get; set; }
        public string BasicSalaryCode { get; set; }
        public bool IsAffectSocialSecurity { get; set; }
        public bool IsTaxable { get; set; }
        public bool IsSystemGenerated { get; set; }
        public string SystemSource { get; set; }
    }

    public class SalariesElementModel
    {
        public int ID { get; set; }
        public string Code { get; set; }
        public string AName { get; set; }
        public int CalcTypeID { get; set; }
        public string FormulaText { get; set; }
        public int PercentageOfElementID { get; set; }
        public int ElementTypeID { get; set; }
        public bool IsAffectSocialSecurity { get; set; }
        public bool IsTaxable { get; set; }
        public int DebitAccountID { get; set; }
        public int CreditAccountID { get; set; }
    }
}
