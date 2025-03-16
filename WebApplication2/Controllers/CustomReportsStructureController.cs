 using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System;
using WebApplication2.cls;
using DocumentFormat.OpenXml.Office2010.Excel;
using Newtonsoft.Json;
using System.Data;
using System.Threading.Tasks;
using WebApplication2.cls.Reports;

namespace WebApplication2.Controllers
{
 
    [Route("api/CustomReportsStructureController")]
    public class CustomReportsStructureController : Controller
    {
        [HttpGet]
        [Route("PrintByHeaderGuid")]
        public async Task<IActionResult> PrintByHeaderGuid(string HeaderGuid, string PageName, string ReportName, int UserId, int CompanyID)
        {
            try
            {
                DataTable dtHeader = new DataTable();
                DataTable dtDetails = new DataTable();
                // 1. Fetch your data
                if (PageName == "InvoicePageAdd")
                {
                    clsInvoiceDetails clsInvoiceDetails = new clsInvoiceDetails();
                    clsInvoiceHeader clsInvoiceHeader = new clsInvoiceHeader();
               
                    dtHeader = clsInvoiceHeader.SelectInvoiceHeaderByGuid(
                HeaderGuid,
                DateTime.Now.AddYears(-100),
                DateTime.Now.AddYears(100),
               0,
                0, 0,
                CompanyID
            );

                    dtDetails = clsInvoiceDetails.SelectInvoiceDetailsByHeaderGuid(HeaderGuid, "", CompanyID);

                }
                else if (PageName == "CreditNotePageAdd")
                {
                    clsCreditNoteHeader clsCreditNoteHeader = new clsCreditNoteHeader();
                    clsCreditNoteDetails clsCreditNoteDetails = new clsCreditNoteDetails();
                    dtHeader = clsCreditNoteHeader.SelectCreditNoteHeaderByGuid(
                  HeaderGuid,
                  DateTime.Now.AddYears(-100),
                  DateTime.Now.AddYears(100),
                  0,
                  0,
                  CompanyID
              );

                    dtDetails = clsCreditNoteDetails.SelectCreditNoteDetailsByHeaderGuid(HeaderGuid, CompanyID);
                }
                else if (PageName == "CashVoucherAdd")
                {
                    clsCashVoucherHeader clsCashVoucherHeader = new clsCashVoucherHeader();
                    clsCashVoucherDetails clsCashVoucherDetails = new clsCashVoucherDetails();
                    dtHeader = clsCashVoucherHeader.SelectCashVoucherHeaderByGuid(
                  HeaderGuid,
                  DateTime.Now.AddYears(-100),
                  DateTime.Now.AddYears(100),
                  0,
                  0,
                  CompanyID,""
              );

                    dtDetails = clsCashVoucherDetails.SelectCashVoucherDetailsByHeaderGuid(HeaderGuid, CompanyID);
                }
                else if (PageName == "JournalVoucherAdd")
                {
                    clsJournalVoucherHeader clsJournalVoucherHeader = new clsJournalVoucherHeader();
                    clsJournalVoucherDetails clsJournalVoucherDetails = new clsJournalVoucherDetails();
                    dtHeader = clsJournalVoucherHeader.SelectJournalVoucherHeader(
                  HeaderGuid,0,0,"","",0, CompanyID,
                  DateTime.Now.AddYears(-100),
                  DateTime.Now.AddYears(100)
                
                  
              );

                    dtDetails = clsJournalVoucherDetails.SelectJournalVoucherDetailsByParentIdForPrint(CompanyID,
                        HeaderGuid,0,0,null).Tables[0];
                }






                List<System.Data.DataTable> listOfTables = new List<System.Data.DataTable>();

                cls_CustomReportsStrucuture cls_CustomReportsStrucuture = new cls_CustomReportsStrucuture();

                DataTable dt = cls_CustomReportsStrucuture.SelectCustomReportsStrucuture(PageName, ReportName, CompanyID);
                listOfTables.Add(dt);
                // clsReportsByPDFCreator clsReportsByPDFCreator = new clsReportsByPDFCreator();
                //var a = clsReportsByPDFCreator.createNewReport(CompanyID, UserId,
                // iTextSharp.text.PageSize.A4 ,


                //   10, 10,1,1, listOfTables,dtDetails,dtHeader);
                var a = await InvoiceGenerator.GenerateInvoicePdf(CompanyID, UserId,
                    // iTextSharp.text.PageSize.A4 ,


                    10, 10, 1, 1, listOfTables, dtDetails, dtHeader);
                // 8. Return file to browser
                return File(a, "application/pdf", PageName.Replace("PageAdd", "") + "Report.pdf");

            }
            catch (Exception ex)
            {
                return Json(ex);
            }
        }
        [HttpGet("Select")]
        public string SelectCustomReportsStructure(string PageName, string ReportName, int CompanyID)
        {
            cls_CustomReportsStrucuture cls_CustomReportsStrucuture = new cls_CustomReportsStrucuture();
            DataTable dt = cls_CustomReportsStrucuture.SelectCustomReportsStrucuture( PageName,  ReportName,  CompanyID);
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
        [HttpGet("SelectAvailableReport")]
        public string SelectAvailableReport(string PageName,  int CompanyID)
        {
            cls_CustomReportsStrucuture cls_CustomReportsStrucuture = new cls_CustomReportsStrucuture();
            DataTable dt = cls_CustomReportsStrucuture.SelectAvailableReport(PageName, CompanyID);
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
        [HttpGet("DeleteReportByName")]
        public string DeleteReportByName(string PageName,string ReportName, int CompanyID)
        {
            cls_CustomReportsStrucuture cls_CustomReportsStrucuture = new cls_CustomReportsStrucuture();
            DataTable dt = cls_CustomReportsStrucuture.DeleteReportByName(PageName, ReportName, CompanyID);
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
        [HttpPost("Save")]
        public int SaveCustomReportsStructure(int CompanyID,[FromBody] List<CustomReportStructureDto> configs)
        {
            if (configs == null || configs.Count == 0)
            {
                return 0;
            }

            try
            {
                bool isdeleted = false;
                // Create an instance of your ADO.NET helper class.
                cls_CustomReportsStrucuture repo = new cls_CustomReportsStrucuture();
                List<int> insertedIds = new List<int>();

                // In a real-world scenario you might also use a transaction here.
                foreach (var config in configs)
                {
                    if (!isdeleted) {
                        repo.DeleteCustomReportsStrucuture(config.pageName, config.reportName,Simulate.Integer32( config.companyID));
                        isdeleted = true;
                    }

                    // Call the InsertCustomReportStructure method.
                    // For PageName and ReportName, we use empty strings if not provided.
                    // Adjust CompanyID and CreationUserID as needed.
                    int id = repo.InsertCustomReportStructure(
                        pageName: config.pageName,
                        reportName: config.reportName,
                        rowIndex: Simulate.Integer32(config.row),
                        type: config.type,
                        parameter: config.id,
                        otherValue: config.text,
                        font: config.font,
                        fontSize: Simulate.Integer32( config.fontSize),
                        fontWeight: config.fontWeight,
                        withBorder: Simulate.Bool(config.withBorder),
                        horizontalAlignment: config.horizontalAlignment,
                        verticalAlignment: config.verticalAlignment,
                        fontColor: config.fontColor,
                        backColor: config.backColor,
                        height: Simulate.Integer32(config.height),
                        width:Simulate.decimal_( config.widthWeight),
                        companyID: Simulate.Integer32( config.companyID),
                        creationUserID: Simulate.Integer32(config.creationUserID),
                        widgetIndex: Simulate.Integer32(config.widgetIndex)
                    );
                    insertedIds.Add(id);
                }

                return 1;
            }
            catch (Exception ex)
            {
                // Optionally log the exception.
                return 0;
            }
        }
        public class CustomReportStructureDto
        {
            public string row { get; set; }
            public string id { get; set; }              // Will be inserted into the Parameter column
            public string type { get; set; }
            public string text { get; set; }            // Will be inserted into the OtherValue column
            public string font { get; set; }
            public string fontSize { get; set; }
            public string fontWeight { get; set; }
            public string withBorder { get; set; }
            public string horizontalAlignment { get; set; }
            public string verticalAlignment { get; set; }
            public string fontColor { get; set; }
            public string backColor { get; set; }
            public string height { get; set; }

            public string pageName { get; set; }
            public string reportName { get; set; }
            public string widthWeight { get; set; }
            public string companyID { get; set; }
            public string creationUserID { get; set; }
            public string widgetIndex { get; set; }
            



        }
    }
}
