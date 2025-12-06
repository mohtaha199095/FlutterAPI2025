using System.Collections.Generic;

namespace WebApplication2.cls
{
    public class clsPayrollPostingRequest
    {
         
            public int PeriodID { get; set; }
            public int CompanyID { get; set; }
            public int BranchID { get; set; }
            public int UserID { get; set; }
            public List<int> EmployeeIDs { get; set; }
       
    }
}
