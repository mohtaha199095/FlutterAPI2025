using System.Data.SqlClient;
using System.Data;
using System;
using System.Collections.Generic;
using WebApplication2.MainClasses;
using System.ComponentModel.Design;
using DocumentFormat.OpenXml.ExtendedProperties;
using DocumentFormat.OpenXml.Office2010.Excel;
using DocumentFormat.OpenXml.Wordprocessing;

namespace WebApplication2.cls
{
    public class clsFinancingDetails
    {


        public DataTable SelectFinancingDetailsByHeaderGuid(string HeaderGuid,int CreationUserID, int CompanyID)
        {
            try
            {
                clsSQL clsSQL = new clsSQL();

                SqlParameter[] prm =
                 { new SqlParameter("@HeaderGuid", SqlDbType.UniqueIdentifier) { Value = Simulate.Guid( HeaderGuid) },
        new SqlParameter("@CompanyID", SqlDbType.Int) { Value = CompanyID },

        new SqlParameter("@CreationUserID", SqlDbType.Int) { Value = CreationUserID },
        
                };
                DataTable dt = clsSQL.ExecuteQueryStatement(@"select * from tbl_FinancingDetails where   (HeaderGuid=@HeaderGuid or @HeaderGuid='00000000-0000-0000-0000-000000000000' )    and (CreationUserID=@CreationUserID or @CreationUserID=0 ) and (CompanyID=@CompanyID or @CompanyID=0  )  order by rowindex asc
                     ", prm);

                return dt;
            }
            catch (Exception ex)
            {

                throw;
            }


        }

        public bool DeleteFinancingDetailsByHeaderGuid(string HeaderGuid, SqlTransaction trn)
        {
            try
            {
                clsSQL clsSQL = new clsSQL();
                clsJournalVoucherHeader clsJournalVoucherHeader=new clsJournalVoucherHeader();
                clsJournalVoucherDetails clsJournalVoucherDetails = new clsJournalVoucherDetails();

                SqlParameter[] prm =
                 { new SqlParameter("@HeaderGuid", SqlDbType.UniqueIdentifier) { Value = Simulate.Guid( HeaderGuid) },

                };
                
                DataTable dtJVs = clsSQL.ExecuteQueryStatement("select JVGuid from tbl_FinancingDetails where HeaderGuid =@HeaderGuid  ", prm, trn);
                if (dtJVs != null && dtJVs.Rows.Count > 0) {
                    for (int i = 0; i < dtJVs.Rows.Count; i++)
                    {
                        bool IsSaved = clsJournalVoucherHeader.DeleteJournalVoucherHeaderByID(Simulate.String( dtJVs.Rows[i]["JVGuid"]), trn);
                        bool a = clsJournalVoucherDetails.DeleteJournalVoucherDetailsByParentId(Simulate.String(dtJVs.Rows[i]["JVGuid"]), trn);
                    }
                  
                }
                SqlParameter[] prm1 =
                { new SqlParameter("@HeaderGuid", SqlDbType.UniqueIdentifier) { Value = Simulate.Guid( HeaderGuid) },

                };
                int A = clsSQL.ExecuteNonQueryStatement(@"delete from tbl_FinancingDetails where (HeaderGuid=@HeaderGuid  )", prm1, trn);

                return true;
            }
            catch (Exception ex)
            {

                return false;
            }


        }
        public string InsertFinancingDetails(DBFinancingHeader DBFinancingHeader, DBFinancingDetails DBFinancingDetails, string HeaderGuid, SqlTransaction trn)
        {
            try
            {
                string jvGuid = InsertFinancingJV(DBFinancingHeader, DBFinancingDetails, trn);
                if (jvGuid == "")
                {
                    return "";
                }
                else {
                    clsFinancingHeader clsFinancingHeader = new clsFinancingHeader();
                    clsFinancingHeader.UpdateFinancingHeaderJVGuid(HeaderGuid, jvGuid, trn);
                    DBFinancingDetails.JVGuid= jvGuid;  
                }
                SqlParameter[] prm =
                   { new SqlParameter("@HeaderGuid", SqlDbType.UniqueIdentifier) { Value = Simulate.Guid(HeaderGuid)},
                    new SqlParameter("@RowIndex", SqlDbType.Int) { Value = DBFinancingDetails.RowIndex },
                    new SqlParameter("@Description", SqlDbType.NVarChar,-1) { Value = DBFinancingDetails.Description },
                    new SqlParameter("@TotalAmount", SqlDbType.Decimal) { Value = DBFinancingDetails.TotalAmount },
                    new SqlParameter("@DownPayment", SqlDbType.Decimal) { Value = DBFinancingDetails.DownPayment },
                    new SqlParameter("@FinancingAmount", SqlDbType.Decimal) { Value = DBFinancingDetails.FinancingAmount },
                    new SqlParameter("@PeriodInMonths", SqlDbType.Int) { Value = DBFinancingDetails.PeriodInMonths },
                    new SqlParameter("@InterestRate", SqlDbType.Decimal) { Value = DBFinancingDetails.InterestRate },
                    new SqlParameter("@InterestAmount", SqlDbType.Decimal) { Value = DBFinancingDetails.InterestAmount },
                    new SqlParameter("@TotalAmountWithInterest", SqlDbType.Decimal) { Value = DBFinancingDetails.TotalAmountWithInterest },
                    new SqlParameter("@FirstInstallmentDate", SqlDbType.DateTime) { Value = DBFinancingDetails.FirstInstallmentDate },
                    new SqlParameter("@InstallmentAmount", SqlDbType.Decimal) { Value = DBFinancingDetails.InstallmentAmount },
                    new SqlParameter("@JVGuid", SqlDbType.UniqueIdentifier) { Value = Simulate.Guid( DBFinancingDetails.JVGuid)  },
                    new SqlParameter("@CreationUserID", SqlDbType.Int) { Value = DBFinancingDetails.CreationUserID },
                    new SqlParameter("@CreationDate", SqlDbType.DateTime) { Value = DateTime.Now },
                    new SqlParameter("@CompanyID", SqlDbType.Int) { Value = DBFinancingDetails.CompanyID },
             new SqlParameter("@SerialNumber", SqlDbType.NVarChar,-1) { Value = DBFinancingDetails.SerialNumber },

                    
                };
         
                string a = @"insert into tbl_FinancingDetails( 
HeaderGuid,
RowIndex,
Description,
TotalAmount,
DownPayment,
FinancingAmount,
PeriodInMonths,
InterestRate,
InterestAmount,
TotalAmountWithInterest,
FirstInstallmentDate,
InstallmentAmount,
JVGuid,
CreationUserID,
CreationDate,
CompanyID,
SerialNumber
)  
OUTPUT INSERTED.Guid  
values ( 
@HeaderGuid,
@RowIndex,
@Description,
@TotalAmount,
@DownPayment,
@FinancingAmount,
@PeriodInMonths,
@InterestRate,
@InterestAmount,
@TotalAmountWithInterest,
@FirstInstallmentDate,
@InstallmentAmount,
@JVGuid,
@CreationUserID,
@CreationDate,
@CompanyID,
@SerialNumber

)";
                clsSQL clsSQL = new clsSQL();
                string myGuid = Simulate.String(clsSQL.ExecuteScalar(a, prm, trn));
                return myGuid;

            }
            catch (Exception ex)
            {

                return "";
            }
        }
        public string InsertFinancingJV(DBFinancingHeader DBFinancingHeader,DBFinancingDetails DBFinancingDetails,SqlTransaction trn)
        {
            try
            {
                clsJournalVoucherHeader clsJournalVoucherHeader = new clsJournalVoucherHeader();
                clsJournalVoucherDetails clsJournalVoucherDetails = new clsJournalVoucherDetails();
                cls_AccountSetting cls_AccountSetting = new cls_AccountSetting();
                clsInvoiceHeader clsInvoiceHeader = new clsInvoiceHeader();
                DataTable dtMaxJV = clsJournalVoucherHeader.SelectMaxJVNo("", 0, DBFinancingHeader.CompanyID, trn);
                int maxJv = 1;
                if (dtMaxJV != null && dtMaxJV.Rows.Count > 0)
                {
                    maxJv = Simulate.Integer32(dtMaxJV.Rows[0][0]) + 1;
                }
                string jvGuid = clsJournalVoucherHeader.InsertJournalVoucherHeader(DBFinancingHeader.BranchID, 0, DBFinancingHeader.Note, Simulate.String(maxJv), (int)clsEnum.VoucherType.Finance, DBFinancingHeader.CompanyID, DBFinancingHeader.VoucherDate, DBFinancingHeader.CreationUserID, trn);

                if (jvGuid == "") {
                    return "";
                }
                DataTable dtAccountSetting = cls_AccountSetting.SelectAccountSetting(0, 0, DBFinancingHeader.CompanyID, trn);

                int SalesInvoiceAcc = clsInvoiceHeader.GetValueFromDT(dtAccountSetting, "AccountRefID", Simulate.String((int)clsEnum.AccountMainSetting.SalesAccount), 2);
                int CustomerAccount  = clsInvoiceHeader.GetValueFromDT(dtAccountSetting, "AccountRefID", Simulate.String((int)clsEnum.AccountMainSetting.CustomerAccount), 2);
                int VendorAccount = clsInvoiceHeader.GetValueFromDT(dtAccountSetting, "AccountRefID", Simulate.String((int)clsEnum.AccountMainSetting.VendorAccount), 2);

                string detailsGuid =  clsJournalVoucherDetails.InsertJournalVoucherDetails(jvGuid, 1, SalesInvoiceAcc,DBFinancingHeader.BusinessPartnerID,0, DBFinancingDetails.TotalAmountWithInterest,
                    DBFinancingDetails.TotalAmountWithInterest *-1, DBFinancingHeader.BranchID,0, DBFinancingHeader.VoucherDate, DBFinancingDetails.Description,
                    DBFinancingHeader.CompanyID, DBFinancingHeader.CreationUserID,trn
                  );
                if (detailsGuid == "")
                {
                    return "";
                }
                clsBusinessPartner clsBusinessPartner = new clsBusinessPartner();
                DataTable dtBP = clsBusinessPartner.SelectBusinessPartner(DBFinancingHeader.BusinessPartnerID, 0, "","", -1, 0,trn);
                int BPAccount = 0;
                if (Simulate.Integer32(dtBP.Rows[0]["Type"]) == 2)
                {

                    BPAccount = VendorAccount;
                }
                else {
                    BPAccount = CustomerAccount;
                }
                
                decimal total = DBFinancingDetails.TotalAmountWithInterest;
                for (int i = 0; i < DBFinancingDetails.PeriodInMonths; i++)
                {
                    decimal InstallmentAmount = DBFinancingDetails.InstallmentAmount;
                    if (i == DBFinancingDetails.PeriodInMonths - 1 && total > 0)
                    {
                        InstallmentAmount = total;
                    }
                    else if (total < DBFinancingDetails.InstallmentAmount && total > 0) {

                        InstallmentAmount = total;
                    }
                    if (InstallmentAmount > 0) { 
                    string a = clsJournalVoucherDetails.InsertJournalVoucherDetails(jvGuid,i+2, BPAccount
                        , DBFinancingHeader.BusinessPartnerID, InstallmentAmount,0, InstallmentAmount,
                       DBFinancingHeader.BranchID,0, DBFinancingDetails.FirstInstallmentDate.AddMonths(i), DBFinancingDetails.Description,
                       DBFinancingHeader.CompanyID, DBFinancingHeader.CreationUserID,trn);
                        if (a == "")
                        {
                            return "";
                        }
                    }
                    total = total - DBFinancingDetails.InstallmentAmount;
                   
                }
                if ( clsJournalVoucherHeader.CheckJVMatch(jvGuid, trn))
                {
                    return jvGuid;
                }
                else {
                    return "";
                }
               
            }
            catch (Exception ex)
            {
                return "";
                
            } }
        public string UpdateFinancingDetailsJVGuid(string Guid, string JVGuid, SqlTransaction trn)
        {
            try
            {
                clsSQL clsSQL = new clsSQL();

                SqlParameter[] prm =
                   {new SqlParameter("@Guid", SqlDbType.UniqueIdentifier) { Value =Simulate.Guid( Guid) },
                    new SqlParameter("@JVGuid", SqlDbType.UniqueIdentifier) { Value = Simulate.Guid(JVGuid) },

                };
                string a = @"update tbl_FinancingDetails set  

  
 JVGuid=@JVGuid  
 where Guid=@guid";

                string A = Simulate.String(clsSQL.ExecuteNonQueryStatement(a, prm, trn));
                return A;


            }
            catch (Exception ex)
            {

                return "";
            }
        }
        public bool InsertInvoiceJournalVoucher(string CashVoucherGuid, int BranchID, int CostCenterID, int CashID, decimal Amount, string Note, DateTime VoucherDate, List<DBCashVoucherDetails> dbCashVoucherDetails, string JVGuid, int JVTypeID, int CompanyID, int CreationUserID, SqlTransaction trn)
        {
            try
            {
                bool IsSaved = true;

                clsJournalVoucherHeader clsJournalVoucherHeader = new clsJournalVoucherHeader();
                clsJournalVoucherDetails clsJournalVoucherDetails = new clsJournalVoucherDetails();
                DataTable dtMaxJVNumber = clsJournalVoucherHeader.SelectMaxJVNo(JVGuid, JVTypeID, CompanyID, trn);
                int MaxJVNumber = 0;
                if (dtMaxJVNumber != null && dtMaxJVNumber.Rows.Count > 0)
                {

                    MaxJVNumber = Simulate.Integer32(dtMaxJVNumber.Rows[0][0]) + 1;
                }
                else { MaxJVNumber = 1; }
                if (JVGuid == "")
                {

                    JVGuid = clsJournalVoucherHeader.InsertJournalVoucherHeader(BranchID, CostCenterID, Note, Simulate.String(MaxJVNumber), JVTypeID, CompanyID, VoucherDate, CreationUserID, trn);
                }
                else
                {
                    clsJournalVoucherHeader.UpdateJournalVoucherHeader(BranchID, CostCenterID, Note, Simulate.String(MaxJVNumber), JVTypeID, VoucherDate, JVGuid, CreationUserID, trn);

                    clsJournalVoucherDetails.DeleteJournalVoucherDetailsByParentId(JVGuid, trn);
                }
                if (JVGuid == "")
                {

                    IsSaved = false;
                }
                UpdateFinancingDetailsJVGuid(CashVoucherGuid, JVGuid, trn);
                cls_AccountSetting cls_AccountSetting = new cls_AccountSetting(); clsInvoiceHeader clsInvoiceHeader = new clsInvoiceHeader();

                DataTable dtAccountSetting = cls_AccountSetting.SelectAccountSetting(0, 0, CompanyID, trn);
                int CashAccount = 0;
                CashAccount = clsInvoiceHeader.GetValueFromDT(dtAccountSetting, "AccountRefID", Simulate.String((int)clsEnum.AccountMainSetting.CashAccount), 2);

                if (JVTypeID == (int)clsEnum.VoucherType.CashPayment)
                {
                    string a = clsJournalVoucherDetails.InsertJournalVoucherDetails(JVGuid, 0, CashAccount
                                   , CashID, 0, Amount, -1 * Amount
                                   , BranchID, CostCenterID, DateTime.Now, Simulate.String(Note), CompanyID
                                   , CreationUserID, trn);
                    if (a == "")
                    {
                        IsSaved = false;
                    }
                }
                else
                {
                    string a = clsJournalVoucherDetails.InsertJournalVoucherDetails(JVGuid, 0, CashAccount
                                   , CashID, Amount, 0, Amount
                                   , BranchID, CostCenterID, DateTime.Now, Simulate.String(Note), CompanyID
                                   , CreationUserID, trn);
                    if (a == "")
                    {
                        IsSaved = false;
                    }

                }
                for (int i = 0; i < dbCashVoucherDetails.Count; i++)
                {
                    string a = clsJournalVoucherDetails.InsertJournalVoucherDetails(JVGuid, i + 1, dbCashVoucherDetails[i].AccountID
                            , dbCashVoucherDetails[i].SubAccountID, dbCashVoucherDetails[i].Debit, dbCashVoucherDetails[i].Credit, dbCashVoucherDetails[i].Debit - dbCashVoucherDetails[i].Credit
                            , dbCashVoucherDetails[i].BranchID, dbCashVoucherDetails[i].CostCenterID, DateTime.Now, Simulate.String(dbCashVoucherDetails[i].Note), dbCashVoucherDetails[i].CompanyID
                            , CreationUserID, trn);
                    if (a == "")
                    {
                        IsSaved = false;
                    }
                }
                bool test = clsJournalVoucherHeader.CheckJVMatch(JVGuid, trn);
                if (!test)
                {
                    IsSaved = false;
                }
                return IsSaved;
            }
            catch (Exception)
            {

                return false;
            }




        }

    }
    public class DBFinancingDetails
    {


        public string Guid { get; set; }
        public string HeaderGuid { get; set; }
         public int RowIndex { get; set; }
        public string Description  { get; set; }
        public decimal TotalAmount   { get; set; }
        public decimal DownPayment { get; set; }
        public decimal FinancingAmount { get; set; }

        public int PeriodInMonths { get; set; }
        public decimal InterestRate { get; set; }
        public decimal InterestAmount { get; set; }
        public decimal TotalAmountWithInterest { get; set; }
        public DateTime FirstInstallmentDate { get; set; }
        public decimal InstallmentAmount { get; set; }
        public string JVGuid { get; set; }
        public int CreationUserID { get; set; }
        public DateTime CreationDate { get; set; }
        public int ModificationUserID { get; set; }
        public DateTime ModificationDate { get; set; }
     

        
        public int CompanyID { get; set; }
        public string SerialNumber { get; set; }




    }
}
