using System;

namespace WebApplication2.DataBaseTable
{
    public class tbl_JournalVoucherDetails
    {
        public string Guid { get; set; }
        public string parentGuid { get; set; }
        public int AccountID { get; set; }
        public int SubAccountID { get; set; }

        public decimal Debit { get; set; }
        public decimal Credit { get; set; }
        public decimal Total { get; set; }
        public int BranchID { get; set; }
        public int CostCenterID { get; set; }
        public DateTime DueDate { get; set; }
        public string Note { get; set; }
        public int CompanyID { get; set; }
        public int CreationUserID { get; set; }
        public DateTime CreationDate { get; set; }
        public int ModificationUserID { get; set; }
        public DateTime ModificationDate { get; set; }

    }
}
