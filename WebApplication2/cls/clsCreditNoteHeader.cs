using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using WebApplication2.MainClasses;
using static WebApplication2.MainClasses.clsEnum;

namespace WebApplication2.cls
{
    public class clsCreditNoteHeader
    {
        public DataTable SelectCreditNoteHeaderByGuid(string guid, DateTime date1, DateTime date2, int VoucherType, int BranchID, int CompanyID,  SqlTransaction trn = null)
        {
            try
            {
                clsSQL clsSQL = new clsSQL();

                SqlParameter[] prm =
                 {
                    new SqlParameter("@Guid", SqlDbType.UniqueIdentifier) { Value = Simulate.Guid( guid) },
 new SqlParameter("@date1", SqlDbType.DateTime) { Value =Simulate.StringToDate(  date1 )},
  new SqlParameter("@date2", SqlDbType.DateTime) { Value = Simulate.StringToDate(  date2 )},
   new SqlParameter("@VoucherType", SqlDbType.Int) { Value = VoucherType },
        new SqlParameter("@CompanyID", SqlDbType.Int) { Value = CompanyID },
          new SqlParameter("@BranchID", SqlDbType.Int) { Value = BranchID },
 

                };
                DataTable dt = clsSQL.ExecuteQueryStatement(@"select tbl_CreditNoteHeader.*   ,
tbl_Branch.AName as BranchAName,
 tbl_CostCenter.AName as CostCenterAName,
tbl_BusinessPartner.AName as  SubAccountAName ,
tbl_BusinessPartner.EmpCode as EmpCode ,
tbl_JournalVoucherTypes.AName as  JournalVoucherTypesAName  ,
 CONVERT(DATE, tbl_CreditNoteHeader.VoucherDate) AS VoucherDate1 

 from tbl_CreditNoteHeader
 left join tbl_Branch on tbl_Branch.ID =tbl_CreditNoteHeader.BranchID 
 left join tbl_CostCenter on tbl_CostCenter.ID =tbl_CreditNoteHeader.CostCenterID
  left join tbl_BusinessPartner on tbl_BusinessPartner.ID =tbl_CreditNoteHeader.SubAccountID
  left join tbl_JournalVoucherTypes on tbl_JournalVoucherTypes.ID = tbl_CreditNoteHeader.VoucherType

where 
(tbl_CreditNoteHeader.Guid=@Guid or @Guid='00000000-0000-0000-0000-000000000000' )  
 and (tbl_CreditNoteHeader.CompanyID=@CompanyID or @CompanyID=0 )
and (tbl_CreditNoteHeader.BranchID=@BranchID or @BranchID=0 )
and (tbl_CreditNoteHeader.VoucherType=@VoucherType or @VoucherType=0 )
and cast( tbl_CreditNoteHeader.VoucherDate as date) between  cast(@date1 as date) and  cast(@date2 as date) 
order by voucherdate desc  ,voucherno desc
                     ", clsSQL.CreateDataBaseConnectionString(CompanyID), prm, trn);

                return dt;
            }
            catch (Exception ex)
            {

                throw;
            }


        }

        public bool DeleteCreditNoteHeaderByGuid(string guid, int CompanyID, SqlTransaction trn)
        {
            try
            {
                clsSQL clsSQL = new clsSQL();

                SqlParameter[] prm =
                 { new SqlParameter("@guid", SqlDbType.UniqueIdentifier) { Value = Simulate.Guid( guid) },

                };
                int A = clsSQL.ExecuteNonQueryStatement(@"delete from tbl_CreditNoteHeader where (guid=@guid  )", clsSQL.CreateDataBaseConnectionString(CompanyID), prm, trn);

                return true;
            }
            catch (Exception ex)
            {

                return false;
            }


        }
        public string InsertCreditNoteHeader(DBCreditNoteHeader dbCreditNoteHeader, SqlTransaction trn, int documentStatus = 2)
        {
            try
            {
                SqlParameter[] prm =
  {
    new SqlParameter("@VoucherDate", SqlDbType.DateTime)
        { Value = dbCreditNoteHeader.VoucherDate },

    new SqlParameter("@BranchID", SqlDbType.Int)
        { Value = dbCreditNoteHeader.BranchID },

    new SqlParameter("@CostCenterID", SqlDbType.Int)
        { Value = dbCreditNoteHeader.CostCenterID },

    new SqlParameter("@AccountID", SqlDbType.Int)
        { Value = dbCreditNoteHeader.AccountID },

    new SqlParameter("@SubAccountID", SqlDbType.Int)
        { Value = dbCreditNoteHeader.SubAccountID },

    new SqlParameter("@Amount", SqlDbType.Decimal)
        { Value = dbCreditNoteHeader.Amount },

    // Use NVarChar(-1) for unlimited text (like "max") if you want to match your snippet.
    new SqlParameter("@Note", SqlDbType.NVarChar, -1)
        { Value = (object)dbCreditNoteHeader.Note ?? DBNull.Value },

    new SqlParameter("@JVGuid", SqlDbType.UniqueIdentifier)
        { Value = (object)dbCreditNoteHeader.JVGuid ?? DBNull.Value },

    // If the VoucherNo is stored as VarChar in DB, you can use NVarChar here or VarChar.
    // Adjust the length (50, 100, etc.) as appropriate.
    new SqlParameter("@VoucherNo", SqlDbType.NVarChar, 50)
        { Value = (object)dbCreditNoteHeader.VoucherNo ?? DBNull.Value },

    new SqlParameter("@VoucherType", SqlDbType.Int)
        { Value = dbCreditNoteHeader.VoucherType },

    new SqlParameter("@CreationUserID", SqlDbType.Int)
        { Value = dbCreditNoteHeader.CreationUserID },
       new SqlParameter("@CompanyID", SqlDbType.Int)
        { Value = dbCreditNoteHeader.CompanyID },
    
     new SqlParameter("@CreationDate", SqlDbType.DateTime)
        { Value = DateTime.Now },
 

    // Same for DueDate
    new SqlParameter("@DueDate", SqlDbType.DateTime)
        { Value = (object)dbCreditNoteHeader.DueDate ?? DBNull.Value },

    new SqlParameter("@DocumentStatus", SqlDbType.Int)
        { Value = documentStatus },
};

                // The SQL command for inserting and returning the auto-generated Guid.
                string sql = @"
INSERT INTO tbl_CreditNoteHeader
(
    VoucherDate,
    BranchID,
    CostCenterID,
    AccountID,
    SubAccountID,
    Amount,
    Note,
    JVGuid,
    VoucherNo,
    VoucherType,
    CreationUserID,
    CreationDate,
 
    DueDate,
CompanyID,
    DocumentStatus

)
OUTPUT INSERTED.Guid
VALUES
(
    @VoucherDate,
    @BranchID,
    @CostCenterID,
    @AccountID,
    @SubAccountID,
    @Amount,
    @Note,
    @JVGuid,
    @VoucherNo,
    @VoucherType,
    @CreationUserID,
    @CreationDate,
 
    @DueDate,
@CompanyID,
    @DocumentStatus
);";
                clsSQL clsSQL = new clsSQL();
                string myGuid = Simulate.String(clsSQL.ExecuteScalar(sql, prm, clsSQL.CreateDataBaseConnectionString(dbCreditNoteHeader.CompanyID), trn));
                return myGuid;

            }
            catch (Exception ex)
            {

                return "";
            }
        }

        public string UpdateCreditNoteHeader(DBCreditNoteHeader dbCreditNoteHeader, int CompanyID, SqlTransaction trn)
        {
            try
            {
                clsSQL clsSQL = new clsSQL();


                SqlParameter[] prm = new SqlParameter[]
                {
    new SqlParameter("@Guid", SqlDbType.UniqueIdentifier)
        { Value = dbCreditNoteHeader.Guid },

    new SqlParameter("@VoucherDate", SqlDbType.DateTime)
        { Value = dbCreditNoteHeader.VoucherDate },

    new SqlParameter("@BranchID", SqlDbType.Int)
        { Value = dbCreditNoteHeader.BranchID },

    new SqlParameter("@CostCenterID", SqlDbType.Int)
        { Value = dbCreditNoteHeader.CostCenterID },

    new SqlParameter("@AccountID", SqlDbType.Int)
        { Value = dbCreditNoteHeader.AccountID },

    new SqlParameter("@SubAccountID", SqlDbType.Int)
        { Value = dbCreditNoteHeader.SubAccountID },

    new SqlParameter("@Amount", SqlDbType.Decimal)
        { Value = dbCreditNoteHeader.Amount },

    new SqlParameter("@Note", SqlDbType.NVarChar, -1)
        { Value = string.IsNullOrEmpty(dbCreditNoteHeader.Note) ? (object)DBNull.Value : dbCreditNoteHeader.Note },

    new SqlParameter("@JVGuid", SqlDbType.UniqueIdentifier)
        { Value = dbCreditNoteHeader.JVGuid },

    new SqlParameter("@VoucherNo", SqlDbType.NVarChar, -1)
        { Value = string.IsNullOrEmpty(dbCreditNoteHeader.VoucherNo) ? (object)DBNull.Value : dbCreditNoteHeader.VoucherNo },

    new SqlParameter("@VoucherType", SqlDbType.Int)
        { Value = dbCreditNoteHeader.VoucherType },

    new SqlParameter("@CreationUserID", SqlDbType.Int)
        { Value = dbCreditNoteHeader.CreationUserID },

    new SqlParameter("@ModificationUserID", SqlDbType.Int)
        { Value =  (object)dbCreditNoteHeader.ModificationUserID  },

    new SqlParameter("@ModificationDate", SqlDbType.DateTime)
        { Value = (object)dbCreditNoteHeader.ModificationDate },

    new SqlParameter("@DueDate", SqlDbType.DateTime)
        { Value =  (object)dbCreditNoteHeader.DueDate }
                };

                string sql = @"
UPDATE tbl_CreditNoteHeader 
SET  
    VoucherDate = @VoucherDate,
    BranchID = @BranchID,
    CostCenterID = @CostCenterID,
    AccountID = @AccountID,
    SubAccountID = @SubAccountID,
    Amount = @Amount,
    Note = @Note,
    JVGuid = @JVGuid,
    VoucherNo = @VoucherNo,
    VoucherType = @VoucherType,
    CreationUserID = @CreationUserID,
    ModificationUserID = @ModificationUserID,
    ModificationDate = @ModificationDate,
    DueDate = @DueDate
WHERE Guid = @Guid;
";

                string A = Simulate.String(clsSQL.ExecuteNonQueryStatement(sql, clsSQL.CreateDataBaseConnectionString(CompanyID), prm, trn));
                return A;


            }
            catch (Exception ex)
            {

                return "";
            }
        }

        public bool UpdateDocumentStatus(string guid, int documentStatus, int userId, int companyId, SqlTransaction trn = null)
        {
            try
            {
                clsSQL clsSQL = new clsSQL();
                SqlParameter[] prm =
                {
                    new SqlParameter("@Guid", SqlDbType.UniqueIdentifier) { Value = Simulate.Guid(guid) },
                    new SqlParameter("@DocumentStatus", SqlDbType.Int) { Value = documentStatus },
                    new SqlParameter("@UserId", SqlDbType.Int) { Value = userId },
                };

                string sql = @"
UPDATE tbl_CreditNoteHeader SET
    DocumentStatus = @DocumentStatus,
    PostedDate = CASE WHEN @DocumentStatus = 2 THEN GETDATE() ELSE PostedDate END,
    PostedByUserId = CASE WHEN @DocumentStatus = 2 THEN @UserId ELSE PostedByUserId END,
    SubmittedByUserId = CASE WHEN @DocumentStatus = 1 THEN @UserId ELSE SubmittedByUserId END,
    SubmittedDate = CASE WHEN @DocumentStatus = 1 THEN GETDATE() ELSE SubmittedDate END
WHERE Guid = @Guid";

                return clsSQL.ExecuteNonQueryStatement(sql, clsSQL.CreateDataBaseConnectionString(companyId), prm, trn) > 0;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public bool InsertCreditNoteJournalVoucher(string creditNoteGuid, int cashAccount, int subAccountID,
            int branchID, int costCenterID, decimal amount, string note, DateTime voucherDate,
            List<DBCreditNoteDetails> details, string jvGuid, int jvTypeID, int companyID, int userId,
            SqlTransaction trn, int jvDocumentStatus = 2)
        {
            try
            {
                bool isSaved = true;
                clsJournalVoucherHeader clsJournalVoucherHeader = new clsJournalVoucherHeader();
                clsJournalVoucherDetails clsJournalVoucherDetails = new clsJournalVoucherDetails();

                DataTable dtMaxJVNumber = clsJournalVoucherHeader.SelectMaxJVNo(jvGuid, jvTypeID, companyID, trn);
                int maxJvNumber = 0;
                if (dtMaxJVNumber != null && dtMaxJVNumber.Rows.Count > 0)
                    maxJvNumber = Simulate.Integer32(dtMaxJVNumber.Rows[0][0]) + 1;
                else
                    maxJvNumber = 1;

                if (string.IsNullOrWhiteSpace(jvGuid) ||
                    jvGuid == "00000000-0000-0000-0000-000000000000")
                {
                    jvGuid = clsJournalVoucherHeader.InsertJournalVoucherHeader(branchID, costCenterID, note,
                        Simulate.String(maxJvNumber), jvTypeID, companyID, voucherDate, userId, "", 0, trn,
                        jvDocumentStatus);
                }
                else
                {
                    clsJournalVoucherHeader.UpdateJournalVoucherHeader(branchID, costCenterID, note,
                        Simulate.String(maxJvNumber), jvTypeID, voucherDate, jvGuid, userId, "", 0, companyID, trn);
                    clsJournalVoucherDetails.DeleteJournalVoucherDetailsByParentId(jvGuid, companyID, trn);
                }

                if (string.IsNullOrWhiteSpace(jvGuid))
                    return false;

                UpdateCreditNoteHeaderJVGuid(creditNoteGuid, jvGuid, companyID, trn);

                if (jvTypeID == (int)VoucherType.creditNote)
                {
                    string a = clsJournalVoucherDetails.InsertJournalVoucherDetails(jvGuid, 0, cashAccount,
                        subAccountID, 0, amount, -1 * amount, 1, 1, -1 * amount,
                        branchID, costCenterID, DateTime.Now, Simulate.String(note), companyID,
                        userId, "", trn);
                    if (a == "") isSaved = false;
                }
                else
                {
                    string a = clsJournalVoucherDetails.InsertJournalVoucherDetails(jvGuid, 0, cashAccount,
                        subAccountID, amount, 0, amount, 1, 1, amount,
                        branchID, costCenterID, DateTime.Now, Simulate.String(note), companyID,
                        userId, "", trn);
                    if (a == "") isSaved = false;
                }

                if (details != null)
                {
                    for (int i = 0; i < details.Count; i++)
                    {
                        string a = clsJournalVoucherDetails.InsertJournalVoucherDetails(jvGuid, i + 1,
                            details[i].AccountID, details[i].SubAccountID, details[i].Debit, details[i].Credit,
                            details[i].Debit - details[i].Credit, 1, 1, details[i].Debit - details[i].Credit,
                            details[i].BranchID, details[i].CostCenterID, DateTime.Now,
                            Simulate.String(details[i].Note), details[i].CompanyID,
                            userId, "", trn);
                        if (a == "") isSaved = false;
                    }
                }

                if (!clsJournalVoucherHeader.CheckJVMatch(jvGuid, companyID, trn))
                    isSaved = false;

                return isSaved;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public string UpdateCreditNoteHeaderJVGuid(string Guid, string JVGuid, int CompanyID, SqlTransaction trn)
        {
            try
            {
                clsSQL clsSQL = new clsSQL();

                SqlParameter[] prm =
                   {new SqlParameter("@Guid", SqlDbType.UniqueIdentifier) { Value =Simulate.Guid( Guid) },
                    new SqlParameter("@JVGuid", SqlDbType.UniqueIdentifier) { Value = Simulate.Guid(JVGuid) },

                };
                string a = @"update tbl_CreditNoteHeader set  

  
 JVGuid=@JVGuid  
 where Guid=@guid";

                string A = Simulate.String(clsSQL.ExecuteNonQueryStatement(a, clsSQL.CreateDataBaseConnectionString(CompanyID), prm, trn));
                return A;


            }
            catch (Exception ex)
            {

                return "";
            }
        }







    }
    public class DBCreditNoteHeader
    {
        /// <summary>
        /// Primary unique identifier (Guid).
        /// </summary>
        public Guid Guid { get; set; }

        /// <summary>
        /// Voucher date.
        /// </summary>
        public DateTime VoucherDate { get; set; }

        /// <summary>
        /// Branch identifier.
        /// </summary>
        public int BranchID { get; set; }

        /// <summary>
        /// Cost center identifier.
        /// </summary>
        public int CostCenterID { get; set; }

        /// <summary>
        /// Account identifier.
        /// </summary>
        public int AccountID { get; set; }

        /// <summary>
        /// Sub-account identifier.
        /// </summary>
        public int SubAccountID { get; set; }

        /// <summary>
        /// Amount for the credit note.
        /// </summary>
        public decimal Amount { get; set; }

        /// <summary>
        /// A general note or description.
        /// </summary>
        public string Note { get; set; }

        /// <summary>
        /// Related JV (journal voucher) identifier (Guid).
        /// </summary>
        public Guid JVGuid { get; set; }

        /// <summary>
        /// Voucher number (stored as a string/varchar in DB).
        /// </summary>
        public string VoucherNo { get; set; }

        /// <summary>
        /// Voucher type indicator.
        /// </summary>
        public int VoucherType { get; set; }

        /// <summary>
        /// The ID of the user who created this record.
        /// </summary>
        public int CreationUserID { get; set; }
        public DateTime? CreationDate { get; set; }

        /// <summary>
        /// The ID of the user who last modified this record.
        /// </summary>
        public int ModificationUserID { get; set; }
        
            public int CompanyID { get; set; }
        /// <summary>
        /// The date/time when this record was last modified.
        /// </summary>
        public DateTime ModificationDate { get; set; }

        /// <summary>
        /// Due date for the credit note (if applicable).
        /// </summary>
        public DateTime DueDate { get; set; }
    }

}
