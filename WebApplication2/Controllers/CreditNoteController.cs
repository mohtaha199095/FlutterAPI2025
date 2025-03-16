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
           
            [FromBody] List<DBCreditNoteDetails> DetailsList)

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

                    DataTable dt = clsSQL.ExecuteQueryStatement("select isnull( max(VoucherNo ),0)+1 as Max from tbl_CreditNoteHeader  where  VoucherType =" + Simulate.String(voucherType) + " and companyid=" + companyID.ToString(), clsSQL.CreateDataBaseConnectionString(companyID), trn);
                    if (dt != null && dt.Rows.Count > 0)
                    {

                        DBCreditNoteHeader.VoucherNo = Simulate.String(dt.Rows[0][0]);
                    }
                    else
                    {

                        DBCreditNoteHeader.VoucherNo = "1";
                    }

                    A = clsCreditNoteHeader.InsertCreditNoteHeader(DBCreditNoteHeader, trn);
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


                     if (IsSaved)
                        IsSaved = InsertCreditNoteVoucherJournalVoucher(A, AccountID, SubAccountID, branchID, costCenterID,  amount, Simulate.String(note), voucherDate, details, "", voucherType, companyID, creationUserID, trn);
                    if (IsSaved)
                    { trn.Commit(); return A; }
                    else
                    { trn.Rollback(); return ""; }

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

                    A = clsCreditNoteHeader.UpdateCreditNoteHeader(dbCreditNoteHeader, companyID, trn);
                    clsCreditNoteDetails.DeleteCreditNoteDetailsByHeaderGuid(guid, companyID, trn);
                    for (int i = 0; i < details.Count; i++)
                    {

                        string c = clsCreditNoteDetails.InsertCreditNoteDetails(details[i], guid, trn);
                        if (c == "")
                            IsSaved = false;
                    }
                     if (IsSaved)
                         IsSaved = InsertCreditNoteVoucherJournalVoucher(guid, AccountID, SubAccountID, branchID, costCenterID,  amount, Simulate.String(note), voucherDate, details, Simulate.String(jVGuid), voucherType, companyID, modificationUserID, trn);
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
        public bool InsertCreditNoteVoucherJournalVoucher(string CreditNoteGuid, int CashAccount,int SubAccountID, int BranchID, int CostCenterID,  decimal Amount, string Note, DateTime VoucherDate, List<DBCreditNoteDetails> dbCashVoucherDetails, string JVGuid, int JVTypeID, int CompanyID, int CreationUserID, SqlTransaction trn)
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
                if (JVGuid == "" || JVGuid == "00000000-0000-0000-0000-000000000000")
                {

                    JVGuid = clsJournalVoucherHeader.InsertJournalVoucherHeader(BranchID, CostCenterID, Note, Simulate.String(MaxJVNumber), JVTypeID, CompanyID, VoucherDate, CreationUserID, "", 0, trn);
                }
                else
                {
                    clsJournalVoucherHeader.UpdateJournalVoucherHeader(BranchID, CostCenterID, Note, Simulate.String(MaxJVNumber), JVTypeID, VoucherDate, JVGuid, CreationUserID, "", 0, CompanyID, trn);

                    clsJournalVoucherDetails.DeleteJournalVoucherDetailsByParentId(JVGuid, CompanyID, trn);
                }
                if (JVGuid == "")
                {

                    IsSaved = false;
                }
                clsCreditNoteHeader clsCreditNoteHeader = new clsCreditNoteHeader();
                clsCreditNoteHeader.UpdateCreditNoteHeaderJVGuid(CreditNoteGuid, JVGuid, CompanyID, trn);
                cls_AccountSetting cls_AccountSetting = new cls_AccountSetting(); clsInvoiceHeader clsInvoiceHeader = new clsInvoiceHeader();

                DataTable dtAccountSetting = cls_AccountSetting.SelectAccountSetting(0, 0, CompanyID, trn);
                //  int CashAccount = 0;
                //  CashAccount = clsInvoiceHeader.GetValueFromDT(dtAccountSetting, "AccountRefID", Simulate.String((int)clsEnum.AccountMainSetting.CashAccount), 2);

                if (JVTypeID == (int)clsEnum.VoucherType.creditNote  )
                {
                    string a = clsJournalVoucherDetails.InsertJournalVoucherDetails(JVGuid, 0, CashAccount
                                   , SubAccountID, 0, Amount, -1 * Amount, 1, 1, -1 * Amount
                                   , BranchID, CostCenterID, DateTime.Now, Simulate.String(Note), CompanyID
                                   , CreationUserID, "", trn);
                    if (a == "")
                    {
                        IsSaved = false;
                    }
                }
                else
                {
                    string a = clsJournalVoucherDetails.InsertJournalVoucherDetails(JVGuid, 0, CashAccount
                                   , SubAccountID, Amount, 0, Amount, 1, 1, Amount
                                   , BranchID, CostCenterID, DateTime.Now, Simulate.String(Note), CompanyID
                                   , CreationUserID, "", trn);
                    if (a == "")
                    {
                        IsSaved = false;
                    }

                }
                for (int i = 0; i < dbCashVoucherDetails.Count; i++)
                {
                    string a = clsJournalVoucherDetails.InsertJournalVoucherDetails(JVGuid, i + 1, dbCashVoucherDetails[i].AccountID
                            , dbCashVoucherDetails[i].SubAccountID, dbCashVoucherDetails[i].Debit, dbCashVoucherDetails[i].Credit, dbCashVoucherDetails[i].Debit - dbCashVoucherDetails[i].Credit, 1, 1, dbCashVoucherDetails[i].Debit - dbCashVoucherDetails[i].Credit
                            , dbCashVoucherDetails[i].BranchID, dbCashVoucherDetails[i].CostCenterID, DateTime.Now, Simulate.String(dbCashVoucherDetails[i].Note), dbCashVoucherDetails[i].CompanyID
                            , CreationUserID, "", trn);
                    if (a == "")
                    {
                        IsSaved = false;
                    }
                }
                bool test = clsJournalVoucherHeader.CheckJVMatch(JVGuid, CompanyID, trn);
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
        [HttpGet]
        [Route("PrintCreditNoteByHeaderGuid1")]
        public IActionResult PrintCreditNoteByHeaderGuid1(string HeaderGuid, int UserId, int CompanyID)
        {
            try
            {
                 
                FastReport.Utils.Config.WebMode = true;
                clsCreditNoteHeader clsCreditNoteHeader = new clsCreditNoteHeader();
                clsCreditNoteDetails clsCreditNoteDetails = new clsCreditNoteDetails();

                DataTable dtHeader = clsCreditNoteHeader.SelectCreditNoteHeaderByGuid(HeaderGuid, DateTime.Now.AddYears(-100), DateTime.Now.AddYears(100), 0, 0, CompanyID);
                DataTable dtDetails = clsCreditNoteDetails.SelectCreditNoteDetailsByHeaderGuid(HeaderGuid, CompanyID);

                dsCashVoucher ds = new dsCashVoucher();

                if (dtDetails != null && dtDetails.Rows.Count > 0)
                {
                    for (int i = 0; i < dtDetails.Rows.Count; i++)
                    {
                        ds.Details.Rows.Add();

                        ds.Details.Rows[i]["Guid"] = Simulate.String(dtDetails.Rows[i]["Guid"]);
                        ds.Details.Rows[i]["HeaderGuid"] = Simulate.String(dtDetails.Rows[i]["HeaderGuid"]);
                        ds.Details.Rows[i]["RowIndex"] = Simulate.String(Simulate.Integer32(dtDetails.Rows[i]["RowIndex"]) + 1);
                  //      ds.Details.Rows[i]["IsUpper"] = Simulate.Bool(dtDetails.Rows[i]["IsUpper"]);
                        ds.Details.Rows[i]["AccountID"] = Simulate.Integer32(dtDetails.Rows[i]["AccountID"]);
                        ds.Details.Rows[i]["SubAccountID"] = Simulate.Integer32(dtDetails.Rows[i]["SubAccountID"]);
                        ds.Details.Rows[i]["BranchID"] = Simulate.Integer32(dtDetails.Rows[i]["BranchID"]);
                        ds.Details.Rows[i]["CostCenterID"] = Simulate.Integer32(dtDetails.Rows[i]["CostCenterID"]);
                        ds.Details.Rows[i]["Debit"] = Simulate.decimal_(dtDetails.Rows[i]["Debit"]);
                        ds.Details.Rows[i]["Credit"] = Simulate.decimal_(dtDetails.Rows[i]["Credit"]);
                        ds.Details.Rows[i]["Total"] = Simulate.decimal_(dtDetails.Rows[i]["Total"]);
                        ds.Details.Rows[i]["Note"] = Simulate.String(dtDetails.Rows[i]["Note"]);
                        ds.Details.Rows[i]["VoucherType"] = Simulate.Integer32(dtDetails.Rows[i]["VoucherType"]);
                        ds.Details.Rows[i]["CompanyID"] = Simulate.Integer32(dtDetails.Rows[i]["CompanyID"]);

                        ds.Details.Rows[i]["BranchAName"] = Simulate.String(dtDetails.Rows[i]["BranchAName"]);
                        ds.Details.Rows[i]["AccountAName"] = Simulate.String(dtDetails.Rows[i]["AccountsAName"]);
                        ds.Details.Rows[i]["CostCenterAName"] = Simulate.String(dtDetails.Rows[i]["CostCenterAName"]);
                        ds.Details.Rows[i]["SubAccountAName"] = Simulate.String(dtDetails.Rows[i]["SubAccountAName"]);


                    }
                }

                if (dtHeader != null && dtHeader.Rows.Count > 0)
                {
                    for (int i = 0; i < dtHeader.Rows.Count; i++)
                    {
                        ds.Header.Rows.Add();

                        ds.Header.Rows[i]["Guid"] = Simulate.String(dtHeader.Rows[i]["Guid"]);
                        ds.Header.Rows[i]["VoucherDate"] = Simulate.StringToDate(dtHeader.Rows[i]["VoucherDate"]).ToString("yyyy-MM-dd");
                        ds.Header.Rows[i]["BranchID"] = Simulate.Integer32(dtHeader.Rows[i]["BranchID"]);
                        ds.Header.Rows[i]["CostCenterID"] = Simulate.Integer32(dtHeader.Rows[i]["CostCenterID"]);
                        //ds.Header.Rows[i]["CashID"] = Simulate.Integer32(dtHeader.Rows[i]["CashID"]);
                        ds.Header.Rows[i]["Amount"] = Simulate.Currency_format(dtHeader.Rows[i]["Amount"]);

                        ds.Header.Rows[i]["JVGuid"] = Simulate.String(dtHeader.Rows[i]["JVGuid"]);
                        ds.Header.Rows[i]["Note"] = Simulate.String(dtHeader.Rows[i]["Note"]);
                        ds.Header.Rows[i]["VoucherNo"] = Simulate.Integer32(dtHeader.Rows[i]["VoucherNo"]);
                //        ds.Header.Rows[i]["ManualNo"] = Simulate.String(dtHeader.Rows[i]["ManualNo"]);

                        ds.Header.Rows[i]["VoucherType"] = Simulate.Integer32(dtHeader.Rows[i]["VoucherType"]);
//                        ds.Header.Rows[i]["RelatedInvoiceGuid"] = Simulate.String(dtHeader.Rows[i]["RelatedInvoiceGuid"]);
                        ds.Header.Rows[i]["BranchAName"] = Simulate.String(dtHeader.Rows[i]["BranchAName"]); 
                        ds.Header.Rows[i]["CostCenterAName"] = Simulate.String(dtHeader.Rows[i]["CostCenterAName"]);
                        //ds.Header.Rows[i]["CashDrawerAName"] = Simulate.String(dtHeader.Rows[i]["CashDrawerAName"]);
                       // ds.Header.Rows[i]["JournalVoucherTypesAname"] = Simulate.String(dtHeader.Rows[i]["JournalVoucherTypesAname"]);



                        ds.Header.Rows[i]["CreationUserID"] = Simulate.Integer32(dtHeader.Rows[i]["CreationUserID"]);
                        ds.Header.Rows[i]["CreationDate"] = Simulate.StringToDate(dtHeader.Rows[i]["CreationDate"]);
                        ds.Header.Rows[i]["ModificationUserID"] = Simulate.Integer32(dtHeader.Rows[i]["ModificationUserID"]);
                        ds.Header.Rows[i]["ModificationDate"] = Simulate.StringToDate(dtHeader.Rows[i]["ModificationDate"]);
                        ds.Header.Rows[i]["CompanyID"] = Simulate.Integer32(dtHeader.Rows[i]["CompanyID"]);

                        //ds.Header.Rows[i]["PaymentMethodAName"] = Simulate.String(dtHeader.Rows[i]["PaymentMethodAName"]);


                    }
                }

                string AmountWithOutDecimal = "";
                string AmountDecimal = "";
                string AmountToWord = "";
                AmountToWord = clsConvertNumberToString.NoToTxt(Simulate.Val(dtHeader.Rows[0]["Amount"]));
                AmountWithOutDecimal = Simulate.String(Simulate.Integer32(dtHeader.Rows[0]["Amount"]));
                AmountDecimal = Simulate.String(Simulate.Integer32((Simulate.Val(dtHeader.Rows[0]["Amount"]) - Simulate.Val(dtHeader.Rows[0]["Amount"])) * 1000));

                FastReport.Report report = new FastReport.Report();


                clsReports clsReports = new clsReports();
                string MyPath =clsReports.getMyPath("rptCashVoucher", CompanyID);
                report.Load(MyPath);
                report.RegisterData(ds);


                report.SetParameterValue("report.AmountWithOutDecimal", Simulate.String(AmountWithOutDecimal));
                report.SetParameterValue("report.AmountDecimal", Simulate.String(AmountDecimal));
                report.SetParameterValue("report.AmountToWord", Simulate.String(AmountToWord));



               clsReports.FastreportStanderdParameters(report, UserId, CompanyID);


                report.Prepare();

                return clsReports.FastreporttoPDF(report);



            }
            catch (Exception ex)
            {

                return Json(ex);
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
