using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Data;

namespace WebApplication2.cls
{
    public class clsPayrollEngine
    {
        public DataTable PreviewPayrollAll(int PayrollPeriodID, int CompanyID)
        {
            try
            {
                clsSQL cls = new clsSQL();

                // 1) Load payroll period
                clsPayrollPeriod per = new clsPayrollPeriod();
                DataTable dtPeriod = per.SelectPayrollPeriod(PayrollPeriodID, "", -1, CompanyID);
                if (dtPeriod.Rows.Count == 0)
                    throw new Exception("Invalid Payroll Period.");

                DateTime startDate = Simulate.StringToDate(dtPeriod.Rows[0]["StartDate"]);
                DateTime endDate = Simulate.StringToDate(dtPeriod.Rows[0]["EndDate"]);

                // 2) Load ALL employees + department
                string sqlEmp = @"
            SELECT bp.ID AS EmployeeID,
                   bp.AName AS EmployeeName,
                   dep.AName AS DepartmentName
            FROM tbl_employee bp
            LEFT JOIN tbl_Department dep ON dep.ID = bp.DepartmentID
            WHERE   bp.CompanyID =  " + CompanyID;

                DataTable dtEmployees = cls.ExecuteQueryStatement(
                    sqlEmp,
                    cls.CreateDataBaseConnectionString(CompanyID),
                   null
                );

                // 3) Result table
                DataTable result = new DataTable();
                result.Columns.Add("EmployeeID", typeof(int));
                result.Columns.Add("EmployeeName");
                result.Columns.Add("DepartmentName");

                result.Columns.Add("BasicSalary", typeof(decimal));
                result.Columns.Add("TotalEarnings", typeof(decimal));
                result.Columns.Add("TotalDeductions", typeof(decimal));
                result.Columns.Add("NetSalary", typeof(decimal));

                // Totals
                decimal totalBasic = 0, totalEarn = 0, totalDed = 0, totalNet = 0;

                clsEmployeeSalaryElements empSal = new clsEmployeeSalaryElements();
                clsSalariesElements master = new clsSalariesElements();

                foreach (DataRow empRow in dtEmployees.Rows)
                {
                    int empID = Convert.ToInt32(empRow["EmployeeID"]);
                    string empName = empRow["EmployeeName"].ToString();
                    string depName = empRow["DepartmentName"].ToString();

                    // Preview per employee (single employee calculation)
                    PayrollPreviewResult preview = PreviewPayroll(empID, PayrollPeriodID, CompanyID);

                    result.Rows.Add(empID, empName, depName,
                        preview.BasicSalary,
                        preview.TotalEarnings,
                        preview.TotalDeductions,
                        preview.NetSalary);

                    // accumulate totals
                    totalBasic += preview.BasicSalary;
                    totalEarn += preview.TotalEarnings;
                    totalDed += preview.TotalDeductions;
                    totalNet += preview.NetSalary;
                }

                // Add totals row
                result.Rows.Add(
                    -1, "TOTAL", "ALL DEPARTMENTS",
                    totalBasic, totalEarn, totalDed, totalNet
                );

                return result;
            }
            catch
            {
                throw;
            }
        }

        public DataTable RunPayroll(int PayrollPeriodID, int CompanyID, int UserID)
        {
            clsSQL cls = new clsSQL();
            SqlConnection con = new SqlConnection(cls.CreateDataBaseConnectionString(CompanyID));
            con.Open();

            SqlTransaction trn = con.BeginTransaction();

            try
            {
                // =====================================================================
                // STEP 1 — Get Payroll Period
                // =====================================================================
                clsPayrollPeriod per = new clsPayrollPeriod();
                DataTable dtPeriod = per.SelectPayrollPeriod(PayrollPeriodID, "", -1, CompanyID);

                if (dtPeriod.Rows.Count == 0)
                    throw new Exception("Invalid Payroll Period.");

                bool isClosed = Simulate.Bool(dtPeriod.Rows[0]["IsClosed"]);
                if (isClosed)
                    throw new Exception("This payroll period is closed.");

                DateTime startDate = Simulate.StringToDate(dtPeriod.Rows[0]["StartDate"]);
                DateTime endDate = Simulate.StringToDate(dtPeriod.Rows[0]["EndDate"]);

                // =====================================================================
                // STEP 2 — Get active employees
                // =====================================================================
                clsBusinessPartner emp = new clsBusinessPartner();
                DataTable dtEmployees = emp.SelectBusinessPartner(
                    0,  // ID
                    1,  // Type = Employee
                    "", "", "", "", -1,
                    CompanyID, trn
                );

                if (dtEmployees.Rows.Count == 0)
                    throw new Exception("No employees found.");

                // =====================================================================
                // STEP 3 — Prepare result table
                // =====================================================================
                DataTable result = new DataTable();
                result.Columns.Add("EmployeeID");
                result.Columns.Add("EmployeeName");
                result.Columns.Add("NetSalary");

                clsPayrollHeader hdr = new clsPayrollHeader();
                clsPayrollDetails dtl = new clsPayrollDetails();
                clsEmployeeSalaryElements empSal = new clsEmployeeSalaryElements();
                clsSalariesElements salEl = new clsSalariesElements();

                // =====================================================================
                // MAIN LOOP — For Each Employee
                // =====================================================================
                foreach (DataRow empRow in dtEmployees.Rows)
                {
                    int EmployeeID = Simulate.Integer32(empRow["ID"]);
                    string EmpName = empRow["AName"].ToString();

                    // --------------------------------------------
                    // Get employee salary elements list
                    // --------------------------------------------
                    DataTable dtAssign = empSal.SelectEmployeeSalaryElements(
                        0, EmployeeID, 0, 1, CompanyID, trn
                    );

                    decimal basicSalary = 0;
                    decimal totalEarnings = 0;
                    decimal totalDeductions = 0;

                    // =================================================================
                    // STEP 3A — Calculate salary elements
                    // =================================================================
                    foreach (DataRow row in dtAssign.Rows)
                    {
                        int SalaryElementID = Simulate.Integer32(row["SalaryElementID"]);
                        decimal AssignedValue = Simulate.Decimal(row["AssignedValue"]);
                        int CalcTypeID = Simulate.Integer32(row["CalcTypeID"]);
                        string FormulaText = Simulate.String(row["FormulaText"]);
                        int ElementTypeID = Simulate.Integer32(row["ElementTypeID"]); // 1=Earn 2=Ded

                        decimal lineValue = 0;

                        // ------------------------
                        // Calc Type 1 — Fixed
                        // ------------------------
                        if (CalcTypeID == 1)
                        {
                            lineValue = AssignedValue;
                        }

                        // ------------------------
                        // Calc Type 2 — Percentage
                        // ------------------------
                        if (CalcTypeID == 2)
                        {
                            lineValue = basicSalary * (AssignedValue / 100m);
                        }

                        // ------------------------
                        // Calc Type 3 — Formula
                        // ------------------------
                        if (CalcTypeID == 3 && FormulaText != "")
                        {
                            // Example: {BASIC} * 0.10
                            string f = FormulaText.Replace("{BASIC}", basicSalary.ToString());
                            lineValue = EvaluateFormula(f);
                        }

                        // ------------------------
                        // Accumulate based on type
                        // ------------------------
                        if (ElementTypeID == 1)  // Earnings
                            totalEarnings += lineValue;

                        if (ElementTypeID == 2)  // Deductions
                            totalDeductions += lineValue;

                        // Add to payroll details
                        dtl.InsertPayrollDetails(
                            0, // header ID will be inserted later
                            SalaryElementID,
                            0,//Eliment type id
                            CalcTypeID,
                            AssignedValue,
                            lineValue,
                            CompanyID,
                            UserID,
                            trn
                        );
                    }

                    // =================================================================
                    // STEP 4 — Calculate Net Salary
                    // =================================================================
                    decimal netSalary = (basicSalary + totalEarnings) - totalDeductions;

                    // =================================================================
                    // STEP 5 — Insert Header
                    // =================================================================
                    int headerID = hdr.InsertPayrollHeader(
                        PayrollPeriodID,
                        EmployeeID,
                        basicSalary,
                        totalEarnings,
                        totalDeductions,
                        netSalary,
                        1, // Draft
                        CompanyID,
                        UserID,
                        trn
                    );

                    // =================================================================
                    // STEP 6 — Update inserted details with headerID
                    // =================================================================
                    dtl.UpdateHeaderIDForDraft(EmployeeID, PayrollPeriodID, headerID, CompanyID, trn);

                    // Add to result
                    result.Rows.Add(EmployeeID, EmpName, netSalary);
                }

                trn.Commit();
                return result;
            }
            catch
            {
                trn.Rollback();
                throw;
            }
        }

        // =============================================
        // Formula Evaluator (simple version)
        // =============================================
        public decimal EvaluateFormula(string formula)
        {
            System.Data.DataTable dt = new System.Data.DataTable();
            var value = dt.Compute(formula, "");
            return Convert.ToDecimal(value);
        }
        public PayrollPreviewResult PreviewPayroll(int EmployeeID, int PayrollPeriodID, int CompanyID)
        {
            try
            {
                // 1) Load assigned salary elements for this employee
                clsEmployeeSalaryElements emp = new clsEmployeeSalaryElements();
                DataTable dtAssigned = emp.SelectEmployeeSalaryElementsForCalculation(EmployeeID, DateTime.Now, CompanyID);

                // 2) Load master salary elements
                clsSalariesElements master = new clsSalariesElements();
                DataTable dtMaster = master.SelectSalariesElements(0, "", "", "", CompanyID);

                // Mapping for quick access
                Dictionary<int, SalariesElementModel> elementMap = BuildElementMap(dtMaster);

                // 3) Build variables dictionary
                var variables = new Dictionary<string, decimal>(StringComparer.OrdinalIgnoreCase);

                // 4) Calculate values for each assigned element
                List<PayrollDetailModel> details = new List<PayrollDetailModel>();

                foreach (DataRow row in dtAssigned.Rows)
                {
                    int salaryElementID = Convert.ToInt32(row["SalaryElementID"]);
                    decimal assignedValue = Convert.ToDecimal(row["AssignedValue"]);
                    int calcType = Convert.ToInt32(row["CalcTypeID"]);

                    if (!elementMap.ContainsKey(salaryElementID))
                        continue;

                    var masterElement = elementMap[salaryElementID];

                    decimal finalAmount = 0;

                    // -----------------------------------------------
                    // FIXED VALUE
                    // -----------------------------------------------
                    if (calcType == 1)
                    {
                        finalAmount = assignedValue;
                    }

                    // -----------------------------------------------
                    // PERCENTAGE
                    // -----------------------------------------------
                    else if (calcType == 2)
                    {
                        int baseElementID = masterElement.PercentageOfElementID;

                        if (baseElementID > 0 && variables.ContainsKey(elementMap[baseElementID].Code))
                        {
                            decimal baseValue = variables[elementMap[baseElementID].Code];
                            finalAmount = baseValue * (assignedValue / 100);
                        }
                    }

                    // -----------------------------------------------
                    // FORMULA
                    // -----------------------------------------------
                    else if (calcType == 3)
                    {
                        string formula = masterElement.FormulaText;

                        finalAmount = FormulaEvaluator.SafeEvaluate(formula, variables);


                    }

                    // Add element to variables for formulas of next elements
                    variables[masterElement.Code] = finalAmount;

                    // Add detail line
                    details.Add(new PayrollDetailModel
                    {
                        SalaryElementID = salaryElementID,
                        ElementName = masterElement.AName,
                        Amount = finalAmount
                    });
                }

                // 5) Build summary
                decimal basic = variables.ContainsKey("BASIC") ? variables["BASIC"] : 0;
                decimal earnings = 0;
                decimal deductions = 0;

                foreach (var d in details)
                {
                    if (d.Amount >= 0)
                        earnings += d.Amount;
                    else
                        deductions += Math.Abs(d.Amount);
                }

                decimal net = earnings - deductions;

                return new PayrollPreviewResult
                {
                    BasicSalary = basic,
                    TotalEarnings = earnings,
                    TotalDeductions = deductions,
                    NetSalary = net,
                    Details = details
                };
            }
            catch
            {
                throw;
            }
        }

        // ===========================================
        // Build master element dictionary
        // ===========================================
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
    // MODEL CLASSES
    // =============================================================
    public class PayrollPreviewResult
    {
        public decimal BasicSalary { get; set; }
        public decimal TotalEarnings { get; set; }
        public decimal TotalDeductions { get; set; }
        public decimal NetSalary { get; set; }

        public List<PayrollDetailModel> Details { get; set; }
    }

    public class PayrollDetailModel
    {
        public int SalaryElementID { get; set; }
        public string ElementName { get; set; }
        public decimal Amount { get; set; }
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