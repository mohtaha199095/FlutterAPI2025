using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Newtonsoft.Json;
using System.Collections.Generic;
using System;
using WebApplication2.cls;
using System.Data;
using WebApplication2.MainClasses;
using WebApplication2.DataSet;
using WebApplication2.cls.Reports;
 
//using FastReport.Export.PdfSimple.PdfObjects;
using System.IO;

 
using FastReport;
using DocumentFormat.OpenXml.Drawing;
 
using System.Threading.Tasks;

namespace WebApplication2.Controllers
{
    [Route("api/CreditNote")]
    public class CreditNoteController : Controller
    {
        [HttpGet]
        [Route("SelectCreditNoteHeaderByGuid")]
        public string SelectCreditNoteHeaderByGuid(string Guid, int BranchID, int VoucherTypeID, int CompanyID, DateTime Date1, DateTime Date2 )
        {
            try
            {
                clsCreditNoteHeader clsCreditNoteHeader = new clsCreditNoteHeader();
                DataTable dt = clsCreditNoteHeader.SelectCreditNoteHeaderByGuid(Simulate.String(Guid), Date1, Date2, VoucherTypeID, BranchID, CompanyID);
                if (dt != null)
                {

                    string JSONString = string.Empty;
                    JSONString = JsonConvert.SerializeObject(dt);
                    return JSONString;
                }
                else
                {

                    return "";
                }
            }
            catch (Exception)
            {

                throw;
            }
        }
        [HttpGet]
        [Route("SelectCreditNoteDetailsByHeaderGuid")]
        public string SelectCreditNoteDetailsByHeaderGuid(string HeaderGuid,  int CompanyID )
        {
            try
            {
                clsCreditNoteDetails clsCreditNoteDetails = new clsCreditNoteDetails();
                DataTable dt = clsCreditNoteDetails.SelectCreditNoteDetailsByHeaderGuid(Simulate.String(HeaderGuid),  CompanyID);
                if (dt != null)
                {

                    string JSONString = string.Empty;
                    JSONString = JsonConvert.SerializeObject(dt);
                    return JSONString;
                }
                else
                {

                    return "";
                }
            }
            catch (Exception)
            {

                throw;
            }
        }
        [HttpGet]
        [Route("DeleteCreditNoteHeaderByGuid")]
        public bool DeleteCreditNoteHeaderByGuid(string Guid, int CompanyID)
        {
            try
            {
                if (Simulate.Guid(Guid) == Simulate.Guid( "00000000-0000-0000-0000-000000000000")) {
                    return false;
                }
                clsCreditNoteDetails clsCreditNoteDetails = new clsCreditNoteDetails();
                clsCreditNoteHeader clsCreditNoteHeader = new clsCreditNoteHeader();
                clsJournalVoucherHeader clsJournalVoucherHeader = new clsJournalVoucherHeader();
                clsJournalVoucherDetails clsJournalVoucherDetails = new clsJournalVoucherDetails();
                SqlTransaction trn; clsSQL clsSQL = new clsSQL();
                SqlConnection con = new SqlConnection(clsSQL.CreateDataBaseConnectionString(CompanyID));
                con.Open();
                trn = con.BeginTransaction(); int A = 0;
                bool IsSaved = true;
                try
                {
                    DataTable dt = clsCreditNoteHeader.SelectCreditNoteHeaderByGuid(Guid, Simulate.StringToDate("1900-01-01"), Simulate.StringToDate("2300-01-01"), 0, 0, 0,  trn);
                    IsSaved = clsCreditNoteHeader.DeleteCreditNoteHeaderByGuid(Guid, CompanyID, trn);
                    bool a = clsCreditNoteDetails.DeleteCreditNoteDetailsByHeaderGuid(Guid, CompanyID, trn);
                    if (dt != null && dt.Rows.Count > 0)
                    {
                        string JVGuid = Simulate.String(dt.Rows[0]["JVGuid"]);
                        bool aa = clsJournalVoucherHeader.DeleteJournalVoucherHeaderByID(JVGuid, CompanyID, trn);
                        bool aaa = clsJournalVoucherDetails.DeleteJournalVoucherDetailsByParentId(JVGuid, CompanyID, trn);
                    }
                    if (!a)
                        IsSaved = false;


                    if (IsSaved)
                        trn.Commit();
                    else
                        trn.Rollback();
                }
                catch (Exception)
                {
                    trn.Rollback();

                }
                finally { con.Close(); }


                return IsSaved;
            }
            catch (Exception)
            {

                throw;
            }

        }
        [HttpPost]
        [Route("InsertCreditNoteHeader")]

        public string InsertCreditNoteHeader(DateTime voucherDate, int branchID, int costCenterID,
            int AccountID, int SubAccountID
            , decimal amount, string note, string VoucherNo
            ,   int voucherType, string relatedInvoiceGuid, int companyID, int creationUserID
           , DateTime DueDate,
           
            [FromBody] List<DBCreditNoteDetails> DetailsList, string BudgetOverrideReason = "")

        {
            try
            {
                DBCreditNoteHeader DBCreditNoteHeader = new DBCreditNoteHeader
                {
                    VoucherDate = voucherDate,
                    BranchID = branchID,
                    CostCenterID = costCenterID,
                    SubAccountID = SubAccountID,
                    AccountID = AccountID,
                    VoucherNo = VoucherNo,
                    Amount = amount,
                    JVGuid = Simulate.Guid(""),
                    Note = Simulate.String(note),
                    
                    VoucherType = voucherType,
                    
                    CompanyID = companyID,
                    CreationUserID = creationUserID,
                    CreationDate = DateTime.Now,
                 
                    DueDate = DueDate,
                 
                };
                List<DBCreditNoteDetails> details = DetailsList;

                //    List<DBCreditNoteDetails> details = JsonConvert.DeserializeObject<List<DBCreditNoteDetails>>(DetailsList);
                clsCreditNoteHeader clsCreditNoteHeader = new clsCreditNoteHeader();
                clsCreditNoteDetails clsCreditNoteDetails = new clsCreditNoteDetails();
                SqlTransaction trn; clsSQL clsSQL = new clsSQL();
                SqlConnection con = new SqlConnection(clsSQL.CreateDataBaseConnectionString(companyID));
                con.Open();
                trn = con.BeginTransaction(); string A = "";
                try
                {
                    bool IsSaved = true;

                    DataTable dt = clsSQL.ExecuteQueryStatement("select isnull( MAX( CAST (VoucherNo as integer )) ,0)+1 as Max from tbl_CreditNoteHeader  where  VoucherType =" + Simulate.String(voucherType) + " and companyid=" + companyID.ToString(), clsSQL.CreateDataBaseConnectionString(companyID), trn);
                    if (dt != null && dt.Rows.Count > 0)
                    {

                        DBCreditNoteHeader.VoucherNo = Simulate.String(dt.Rows[0][0]);
                    }
                    else
                    {

                        DBCreditNoteHeader.VoucherNo = "1";
                    }

                    clsApprovalEngine approvalEngine = new clsApprovalEngine();
                    int documentStatus = approvalEngine.ResolveInitialDocumentStatus(
                        companyID, voucherType, branchID, amount);

                    bool forceBudgetApproval = false;
                    BudgetCheckResult budgetCheck = null;
                    if (voucherType == (int)clsEnum.VoucherType.debitNote)
                    {
                        var spend = clsBudgetControl.FromCreditNoteDetails(details, branchID, costCenterID);
                        string blocked = new clsBudgetControl().ApplyGate(
                            companyID, voucherType, voucherDate, branchID, costCenterID, spend,
                            BudgetOverrideReason, out forceBudgetApproval, out budgetCheck);
                        if (blocked != null)
                        {
                            trn.Rollback();
                            return blocked;
                        }
                        if (forceBudgetApproval)
                            documentStatus = (int)clsEnum.DocumentStatus.Draft;
                    }

                    A = clsCreditNoteHeader.InsertCreditNoteHeader(DBCreditNoteHeader, trn, documentStatus);
                    if (A == "")
                    { IsSaved = false; }
                    else
                    {
                        for (int i = 0; i < details.Count; i++)
                        {
                            string c = clsCreditNoteDetails.InsertCreditNoteDetails(details[i], A, trn);
                            if (c == "")
                                IsSaved = false;
                        }

                    }


                    if (IsSaved && documentStatus == (int)clsEnum.DocumentStatus.Posted)
                        IsSaved = clsCreditNoteHeader.InsertCreditNoteJournalVoucher(A, AccountID, SubAccountID,
                            branchID, costCenterID, amount, Simulate.String(note), voucherDate, details, "",
                            voucherType, companyID, creationUserID, trn);
                    if (IsSaved)
                    { trn.Commit(); }
                    else
                    { trn.Rollback(); return ""; }

                    if (forceBudgetApproval && !string.IsNullOrEmpty(A))
                    {
                        string ovErr = new clsBudget().CompleteBudgetOverride(
                            "tbl_CreditNoteHeader", companyID, creationUserID, voucherType, A,
                            Simulate.String(DBCreditNoteHeader.VoucherNo), BudgetOverrideReason,
                            budgetCheck?.Breaches);
                        if (ovErr != null) return ovErr;
                    }
                    return A;

                }
                catch (Exception)
                {

                    trn.Rollback();
                    return "";
                }
                finally { con.Close(); }

            }
            catch (Exception ex)
            {

                return "";
            }

        }
        [Route("UpdateCreditNoteHeader")]
        public string UpdateCreditNoteHeader(DateTime voucherDate, int branchID,
            int costCenterID, int AccountID 
            , decimal amount, string jVGuid, string note
            , int voucherType,  
            int companyID,
             int modificationUserID, string guid,
             int SubAccountID, string VoucherNo,
              DateTime DueDate,
             
             [FromBody] List<DBCreditNoteDetails> details)
        {





            try
            {

                DBCreditNoteHeader dbCreditNoteHeader = new DBCreditNoteHeader
                {
                    VoucherDate = voucherDate,
                    BranchID = branchID,
                    CostCenterID = costCenterID,
                    AccountID = AccountID,
                    //CashID = cashID,
                    Amount = amount,
                    JVGuid = Simulate.Guid(jVGuid),
                    SubAccountID= SubAccountID,
                    VoucherNo= VoucherNo,
                    Note = Simulate.String(note),

                   // ManualNo = Simulate.String(manualNo),
                    VoucherType = voucherType,
                   // RelatedInvoiceGuid = Simulate.Guid(relatedInvoiceGuid),
                    CompanyID = companyID,
                    ModificationUserID = modificationUserID,
                    ModificationDate = DateTime.Now,
                    Guid = Simulate.Guid(guid),
                  //  ChequeName = Simulate.String(ChequeName),
                    DueDate = DueDate,
                  //  ChequeNote = Simulate.String(ChequeNote),
                  //  PaymentMethodTypeID = Simulate.Integer32(PaymentMethodTypeID),
                };

              //  List<DBCreditNoteDetails> details = JsonConvert.DeserializeObject<List<DBCreditNoteDetails>>(detailsList);
                clsCreditNoteHeader clsCreditNoteHeader = new clsCreditNoteHeader();
                clsCreditNoteDetails clsCreditNoteDetails = new clsCreditNoteDetails();
                SqlTransaction trn; clsSQL clsSQL = new clsSQL();
                SqlConnection con = new SqlConnection(clsSQL.CreateDataBaseConnectionString(companyID));
                con.Open();
                trn = con.BeginTransaction();
                string A = "";
                try
                {
                    bool IsSaved = true;

                    DataTable dtExisting = clsCreditNoteHeader.SelectCreditNoteHeaderByGuid(
                        guid,
                        Simulate.StringToDate("1900-01-01"),
                        Simulate.StringToDate("2300-01-01"),
                        0, 0, companyID,
                        trn);
                    int documentStatus = (int)clsEnum.DocumentStatus.Posted;
                    if (dtExisting != null && dtExisting.Rows.Count > 0)
                    {
                        var row = dtExisting.Rows[0];
                        documentStatus = Simulate.Integer32(row["DocumentStatus"]);
                        int branchId = Simulate.Integer32(row["BranchID"]);
                        int voucherTypeId = Simulate.Integer32(row["VoucherType"]);
                        decimal voucherAmount = Simulate.Decimal(row["Amount"]);

                        var approvalEngine = new clsApprovalEngine();
                        if (approvalEngine.DocumentStatusBlocksEdit(
                                companyID, voucherTypeId, branchId, voucherAmount, documentStatus))
                        {
                            trn.Rollback();
                            return "";
                        }
                    }

                    A = clsCreditNoteHeader.UpdateCreditNoteHeader(dbCreditNoteHeader, companyID, trn);
                    clsCreditNoteDetails.DeleteCreditNoteDetailsByHeaderGuid(guid, companyID, trn);
                    for (int i = 0; i < details.Count; i++)
                    {

                        string c = clsCreditNoteDetails.InsertCreditNoteDetails(details[i], guid, trn);
                        if (c == "")
                            IsSaved = false;
                    }
                    if (IsSaved && documentStatus == (int)clsEnum.DocumentStatus.Posted)
                        IsSaved = clsCreditNoteHeader.InsertCreditNoteJournalVoucher(guid, AccountID, SubAccountID,
                            branchID, costCenterID, amount, Simulate.String(note), voucherDate, details,
                            Simulate.String(jVGuid), voucherType, companyID, modificationUserID, trn);
                    if (IsSaved)
                    { trn.Commit(); return A; }
                    else
                    { trn.Rollback(); return ""; }
                }
                catch (Exception)
                {
                    A = "";
                    trn.Rollback();
                }
                finally { con.Close(); }
                return A;
            }
            catch (Exception)
            {

                throw;
            }

        }
        [HttpGet]
        [Route("PrintCreditNoteByHeaderGuid1")]
        public IActionResult PrintCreditNoteByHeaderGuid1(
            string HeaderGuid, int UserId, int CompanyID, int TransactionReportID = 0)
        {
            try
            {
                clsTransactionReportPrint printer = new clsTransactionReportPrint();
                byte[] pdfBytes = printer.BuildTransactionReportPdf(
                    HeaderGuid,
                    clsTransactionReportPrint.PageCreditNotePageAdd,
                    UserId,
                    CompanyID,
                    TransactionReportID);
                return File(pdfBytes, "application/pdf", "CreditNote.pdf");
            }
            catch (Exception ex)
            {
                return BadRequest("Print error: " + ex.Message);
            }
        }

// ...



    // Utility methods for building tables more cleanly
    //private void AddCellToHeader(iText.Layout.Element.Table table, string cellText)
    //{

    //        //    var font = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 10, BaseColor.WHITE);
    //        //PdfPCell cell = new PdfPCell(new Phrase(cellText, font))
    //        //{
    //        //    BackgroundColor = BaseColor.DARK_GRAY,
    //        //    HorizontalAlignment = Element.ALIGN_CENTER ,

    //        //};
    //        //table.AddCell(cell);



    //        var font = PdfFontFactory.CreateFont(iText.IO.Font.Constants.StandardFonts.HELVETICA_BOLD);

    //        // Create the cell with the font
    //        var cell = new Cell().Add(new iText.Layout.Element.Paragraph(cellText).SetFont(font))
    //                              .SetBackgroundColor(ColorConstants.DARK_GRAY)
    //                              .SetTextAlignment(TextAlignment.CENTER)
    //                              .SetFontColor(ColorConstants.WHITE);

    //        // Add the cell to the table
    //        table.AddCell(cell);

    //    }

    //private void AddCellToBody(iText.Layout.Element.Table table, string cellText)//PdfPTable
    //    {
    //        //var font = FontFactory.GetFont(FontFactory.HELVETICA, 10, BaseColor.BLACK);
    //        //PdfPCell cell = new PdfPCell(new Phrase(cellText, font))
    //        //{
    //        //    BackgroundColor = BaseColor.WHITE,
    //        //    HorizontalAlignment = Element.ALIGN_LEFT
    //        //};
    //        //table.AddCell(cell);

    //        var font = PdfFontFactory.CreateFont(iText.IO.Font.Constants.StandardFonts.HELVETICA_BOLD);

    //        // Create the cell with the font
    //        var cell = new Cell().Add(new iText.Layout.Element.Paragraph(cellText).SetFont(font))
    //                              .SetBackgroundColor(ColorConstants.DARK_GRAY)
    //                              .SetTextAlignment(TextAlignment.CENTER)
    //                              .SetFontColor(ColorConstants.WHITE);

    //        // Add the cell to the table
    //        table.AddCell(cell);

    //    }

}
}
