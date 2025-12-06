using System.Collections.Generic;

namespace WebApplication2.cls
{
    public class clsPayrollPostingResult
    {
        public bool Success { get; set; }
        public string JVGuid { get; set; }
        public string JVNumber { get; set; }
        public List<string> Messages { get; set; } = new();
    }
}