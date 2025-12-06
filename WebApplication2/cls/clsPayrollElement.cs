 
using Microsoft.Data.SqlClient;
using Microsoft.Identity.Client;
using System;
using System.Data;

namespace WebApplication2.cls
{
    public class clsPayrollElement
    {
        public DataTable SelectElementsWithAccounts(
    int employeeId,
    int periodId,
    int companyId,
    SqlConnection conn,
    SqlTransaction trn)
        {
            try
            {
                SqlParameter[] prm =
                   {
                    new SqlParameter("@EmployeeID", SqlDbType.Int) { Value = employeeId },
      new SqlParameter("@CompanyID", SqlDbType.Int) { Value = companyId },
       new SqlParameter("@PeriodID", SqlDbType.Int) { Value = periodId },


                };

       
                clsSQL cls = new clsSQL();
                string a = @"
     	 select tbl_EmployeeSalaryElements.SalaryElementID  
			 , tbl_SalariesElements.AName as ElementName
			 ,tbl_EmployeeSalaryElements.AssignedValue as Amount
			 ,tbl_SalariesElements.CompanyDebitAccountID as AccountID
			 ,0 AS BranchID
             ,0 AS CostCenterID
			 from tbl_EmployeeSalaryElements
			 left join  tbl_SalariesElements on tbl_SalariesElements.ID = tbl_EmployeeSalaryElements.SalaryElementID
			       WHERE tbl_EmployeeSalaryElements.EmployeeID = @EmployeeID
  
              AND tbl_EmployeeSalaryElements.CompanyID  = @CompanyID
            ";
                DataTable dd = cls.ExecuteQueryStatement(a, cls.CreateDataBaseConnectionString(companyId), prm, trn);

              
                return dd;
                 
            }
            catch (Exception ex)
            {
                throw new Exception("Error loading payroll elements for posting", ex);
            }
        }

    }
}
