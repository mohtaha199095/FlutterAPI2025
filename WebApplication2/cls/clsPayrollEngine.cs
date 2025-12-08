using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using WebApplication2.DataBaseTable;

namespace WebApplication2.cls
{
    public class clsPayrollEngine
    {
        // =============================================================
        // PREVIEW PAYROLL FOR ALL EMPLOYEES
        // =============================================================
        public DataTable PreviewPayrollAll(int PayrollPeriodID, int DepartmentID, int CompanyID)
        {
            try
            {
                clsSQL cls = new clsSQL();

                // Load payroll period
                clsPayrollPeriod per = new clsPayrollPeriod();
                DataTable dtPeriod = per.SelectPayrollPeriod(PayrollPeriodID, "", -1, CompanyID);

                if (dtPeriod.Rows.Count == 0)
                    throw new Exception("Invalid Payroll Period");

                SqlParameter[] prm =
                {
                    new SqlParameter("@DepartmentID", DepartmentID),
                    new SqlParameter("@CompanyID", CompanyID)
                };

                // Load employees
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

                // Result table
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

                // Totals row
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

        // =============================================================
        // SINGLE EMPLOYEE PAYROLL PREVIEW
        // =============================================================
        public PayrollPreviewResult PreviewPayroll(int EmployeeID, int PayrollPeriodID, int CompanyID)
        {
            try
            {
                // ---- 1) Load assigned salary elements
                clsEmployeeSalaryElements emp = new clsEmployeeSalaryElements();
                DataTable dtAssigned =
                    emp.SelectEmployeeSalaryElementsForCalculation(EmployeeID, PayrollPeriodID, CompanyID);

                // ---- 2) Load master salary elements
                clsSalariesElements master = new clsSalariesElements();
                DataTable dtMaster = master.SelectSalariesElements(0, "", "", "", CompanyID);

                Dictionary<int, SalariesElementModel> elementMap = BuildElementMap(dtMaster);

                // ---- 3) Variables dictionary (for formulas)
                var variables = new Dictionary<string, decimal>(StringComparer.OrdinalIgnoreCase);

                // ---- 4) Attendance rule engine
                clsAttendanceRuleExecutor attendanceExec = new clsAttendanceRuleExecutor();
                List<PayrollImpactItem> attendanceItems =
                    attendanceExec.ExecuteRulesForEmployee(EmployeeID, PayrollPeriodID, CompanyID);

                // Put attendance values into variables
                foreach (var att in attendanceItems)
                {
                    variables[att.Code] = att.Amount;
                }

                // ---- 5) Salary element calculation
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

                    // ---- Fixed
                    if (calcType == 1)
                        finalAmount = assignedValue;

                    // ---- Percentage
                    else if (calcType == 2)
                    {
                        int baseElementID = masterElement.PercentageOfElementID;

                        if (baseElementID > 0 &&
                            variables.ContainsKey(elementMap[baseElementID].Code))
                        {
                            decimal baseValue = variables[elementMap[baseElementID].Code];
                            finalAmount = baseValue * (assignedValue / 100m);
                        }
                    }

                    // ---- Formula
                    else if (calcType == 3)
                    {
                        finalAmount = FormulaEvaluator.SafeEvaluate(
                            masterElement.FormulaText,
                            variables
                        );
                    }

                    // Add to variable dictionary
                    variables[masterElement.Code] = finalAmount;

                    // Add detail record
                    details.Add(new PayrollDetailModel
                    {
                        SalaryElementID = salaryElementID,
                        ElementName = masterElement.AName,
                        Amount = finalAmount,
                        ElementTypeID = ElementTypeID,
                        BasicSalaryCode = basicCode
                    });
                }

                // ---- 6) Summary
                decimal basic = variables.ContainsKey("BASIC") ? variables["BASIC"] : 0;
                decimal earnings = 0;
                decimal deductions = 0;

                // Salary elements
                foreach (var d in details)
                {
                    if (d.ElementTypeID == 1 && d.BasicSalaryCode!="BASIC")
                        earnings += d.Amount;
                    else if ( d.BasicSalaryCode != "BASIC")
                        deductions += Math.Abs(d.Amount);
                }

                // Attendance items
                foreach (var a in attendanceItems)
                {
                    if (a.ElementTypeID == 1)
                        earnings += a.Amount;
                    else
                        deductions += Math.Abs(a.Amount);
                }

                decimal net = basic + earnings - deductions;

                return new PayrollPreviewResult
                {
                    BasicSalary = basic,
                    TotalEarnings = earnings,
                    TotalDeductions = deductions,
                    NetSalary = net,
                    IsPosted = IsPosted,
                    SalaryElements = details,
                    AttendanceElements = attendanceItems
                };
            }
            catch
            {
                throw;
            }
        }

        // =============================================================
        // Build dictionary for master salary elements
        // =============================================================
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
                    FormulaText = row["FormulaText"].ToString(),
                    PercentageOfElementID = Convert.ToInt32(row["PercentageOfElementID"])
                };

                map[el.ID] = el;
            }

            return map;
        }
    }

    // =============================================================
    // MODELS
    // =============================================================
    public class PayrollPreviewResult
    {
        public decimal BasicSalary { get; set; }
        public decimal TotalEarnings { get; set; }
        public decimal TotalDeductions { get; set; }
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
    }

    public class SalariesElementModel
    {
        public int ID { get; set; }
        public string Code { get; set; }
        public string AName { get; set; }
        public int CalcTypeID { get; set; }
        public string FormulaText { get; set; }
        public int PercentageOfElementID { get; set; }
    }
}
