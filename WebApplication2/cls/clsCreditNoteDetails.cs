using Microsoft.Data.SqlClient;
using System;
using System.Data;

namespace WebApplication2.cls
{
    public class clsCreditNoteDetails
    {
        public DataTable SelectCreditNoteDetailsByHeaderGuid(string HeaderGuid, int CompanyID)
        {
            try
            {
                clsSQL clsSQL = new clsSQL();

                SqlParameter[] prm =
                 { new SqlParameter("@HeaderGuid", SqlDbType.UniqueIdentifier) { Value = Simulate.Guid( HeaderGuid) },

        new SqlParameter("@CompanyID", SqlDbType.Int) { Value = CompanyID },

                };
                var a = @"select tbl_CreditNotedetails.*,
tbl_Branch.AName as BranchAName,
tbl_Accounts.AName as AccountsAName,
tbl_CostCenter.AName as CostCenterAName,
case when (AccountID = (select top 1 AccountID from tbl_AccountSetting where AccountRefID in (6,7) and CompanyID =tbl_CreditNotedetails.CompanyID order by id desc))then 
tbl_BusinessPartner.AName   when   (AccountID = (select top 1 AccountID from tbl_AccountSetting where AccountRefID in (15) and CompanyID = tbl_CreditNotedetails.CompanyID order by id desc))then 
  tbl_Banks.AName when   (AccountID = (select top 1 AccountID from tbl_AccountSetting where AccountRefID in (5) and CompanyID = tbl_CreditNotedetails.CompanyID order by id desc))then 
tbl_CashDrawer.AName else '' end as SubAccountAName 
from tbl_CreditNotedetails
 left join tbl_Branch on tbl_Branch.ID =tbl_CreditNotedetails.BranchID
  left join tbl_Accounts on tbl_Accounts.ID =tbl_CreditNotedetails.AccountID
    left join tbl_CostCenter on tbl_CostCenter.ID =tbl_CreditNotedetails.CostCenterID
	 left join tbl_Banks on tbl_Banks.ID =tbl_CreditNotedetails.SubAccountID
	  left join tbl_BusinessPartner on tbl_BusinessPartner.ID =tbl_CreditNotedetails.SubAccountID
	   left join tbl_CashDrawer on tbl_CashDrawer.ID =tbl_CreditNotedetails.SubAccountID
 where   (tbl_CreditNotedetails.HeaderGuid=@HeaderGuid or @HeaderGuid='00000000-0000-0000-0000-000000000000' )  
and (tbl_CreditNotedetails.CompanyID=@CompanyID or @CompanyID=0  )  order by tbl_CreditNotedetails.rowindex asc
                     ";
                DataTable dt = clsSQL.ExecuteQueryStatement(a, clsSQL.CreateDataBaseConnectionString(CompanyID), prm);

                return dt;
            }
            catch (Exception ex)
            {

                throw;
            }


        }








        // Method to delete all Credit Note Details for a given Header Guid
        public bool DeleteCreditNoteDetailsByHeaderGuid(string HeaderGuid, int CompanyID, SqlTransaction trn)
        {
            try
            {
                clsSQL clsSQL = new clsSQL();

                SqlParameter[] prm = new SqlParameter[]
                {
            new SqlParameter("@HeaderGuid", SqlDbType.UniqueIdentifier)
            {
                Value = Simulate.Guid(HeaderGuid)
            },
                };

                int A = clsSQL.ExecuteNonQueryStatement(
                    @"DELETE FROM tbl_CreditNoteDetails 
              WHERE (HeaderGuid = @HeaderGuid)",
                    clsSQL.CreateDataBaseConnectionString(CompanyID),
                    prm,
                    trn
                );

                return true;
            }
            catch (Exception ex)
            {
                // Optionally log the exception: ex.Message
                return false;
            }
        }

        // Method to insert a new Credit Note Details record and return its Guid.
        public string InsertCreditNoteDetails(DBCreditNoteDetails dBCreditNoteDetails, string HeaderGuid, SqlTransaction trn)
        {
            try
            {
                SqlParameter[] prm = new SqlParameter[]
                {
            new SqlParameter("@HeaderGuid", SqlDbType.UniqueIdentifier)
            {
                Value = Simulate.Guid(HeaderGuid)
            },
            new SqlParameter("@RowIndex", SqlDbType.Int)
            {
                Value = dBCreditNoteDetails.RowIndex
            },
            new SqlParameter("@AccountID", SqlDbType.Int)
            {
                Value = dBCreditNoteDetails.AccountID
            },
            new SqlParameter("@SubAccountID", SqlDbType.Int)
            {
                Value = dBCreditNoteDetails.SubAccountID
            },
            new SqlParameter("@BranchID", SqlDbType.Int)
            {
                Value = dBCreditNoteDetails.BranchID
            },
            new SqlParameter("@CostCenterID", SqlDbType.Int)
            {
                Value = dBCreditNoteDetails.CostCenterID
            },
            new SqlParameter("@Debit", SqlDbType.Decimal)
            {
                Value = dBCreditNoteDetails.Debit
            },
            new SqlParameter("@Credit", SqlDbType.Decimal)
            {
                Value = dBCreditNoteDetails.Credit
            },
            new SqlParameter("@Total", SqlDbType.Decimal)
            {
                Value = dBCreditNoteDetails.Total
            },
            new SqlParameter("@Note", SqlDbType.NVarChar, -1)
            {
                Value = dBCreditNoteDetails.Note
            },
            new SqlParameter("@VoucherType", SqlDbType.Int)
            {
                Value = dBCreditNoteDetails.VoucherType
            },
            new SqlParameter("@CompanyID", SqlDbType.Int)
            {
                Value = dBCreditNoteDetails.CompanyID
            },
                };

                string sql = @"
INSERT INTO tbl_CreditNoteDetails 
    (HeaderGuid, RowIndex, AccountID, SubAccountID, BranchID,
     CostCenterID, Debit, Credit, Total, Note, VoucherType, CompanyID)  
OUTPUT INSERTED.Guid  
VALUES 
    (@HeaderGuid, @RowIndex, @AccountID, @SubAccountID, @BranchID,
     @CostCenterID, @Debit, @Credit, @Total, @Note, @VoucherType, @CompanyID)";

                // Create a new instance of clsSQL for executing the command.
                clsSQL clsSQL = new clsSQL();

                // Execute the INSERT statement and get the newly generated Guid.
                string myGuid = Simulate.String(clsSQL.ExecuteScalar(
                    sql,
                    prm,
                    clsSQL.CreateDataBaseConnectionString(dBCreditNoteDetails.CompanyID),
                    trn
                ));

                return myGuid;
            }
            catch (Exception ex)
            {
                // Optionally log the exception: ex.Message
                return "";
            }
        }
    }
    public class DBCreditNoteDetails
    {
        /// <summary>
        /// Primary unique identifier (Guid).
        /// </summary>
        public Guid? Guid { get; set; }

        /// <summary>
        /// Row index (line item index).
        /// </summary>
        public int RowIndex { get; set; }

        /// <summary>
        /// Links to the CreditNoteHeader's Guid (foreign key).
        /// </summary>
        public Guid? HeaderGuid { get; set; }

        /// <summary>
        /// Account identifier.
        /// </summary>
        public int AccountID { get; set; }

        /// <summary>
        /// Sub-account identifier.
        /// </summary>
        public int SubAccountID { get; set; }

        /// <summary>
        /// Branch identifier.
        /// </summary>
        public int BranchID { get; set; }

        /// <summary>
        /// Cost center identifier.
        /// </summary>
        public int CostCenterID { get; set; }

        /// <summary>
        /// Debit amount.
        /// </summary>
        public decimal Debit { get; set; }

        /// <summary>
        /// Credit amount.
        /// </summary>
        public decimal Credit { get; set; }

        /// <summary>
        /// Total amount (if you are explicitly storing it).
        /// </summary>
        public decimal Total { get; set; }

        /// <summary>
        /// A note or description for this detail line.
        /// </summary>
        public string Note { get; set; }

        /// <summary>
        /// Voucher type indicator.
        /// </summary>
        public int VoucherType { get; set; }

        /// <summary>
        /// Company identifier.
        /// </summary>
        public int CompanyID { get; set; }



        /// <summary>
        /// The ID of the user who created this record.
        /// </summary>
        public int CreationUserID { get; set; }
        public DateTime? CreationDate { get; set; }

        /// <summary>
        /// The ID of the user who last modified this record.
        /// </summary>
        public int? ModificationUserID { get; set; }

        /// <summary>
        /// The date/time when this record was last modified.
        /// </summary>
        public DateTime? ModificationDate { get; set; }
    }
}
