namespace WebApplication2.DataBaseTable
{
    public class AttendanceRuleMappingModel
    {
        public int ID { get; set; }

        public int RuleID { get; set; }
        public int? EmployeeID { get; set; }
        public int? DepartmentID { get; set; }
        public int? ShiftID { get; set; }

        public int Priority { get; set; }

        public bool IsActive { get; set; }
    }
}
