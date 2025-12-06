namespace WebApplication2.cls
{
    public class clsPayrollElementRow
    {
        public int ElementID { get; set; }
        public string ElementName { get; set; }
        public decimal Amount { get; set; }
        public bool IsDeduction { get; set; }
        public int AccountID { get; set; }
    }
}