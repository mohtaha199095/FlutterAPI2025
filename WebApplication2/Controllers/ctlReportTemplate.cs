using DocumentFormat.OpenXml.ExtendedProperties;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Data;
using WebApplication2.cls;
 
using WebApplication2.DataBaseTable;
using static WebApplication2.cls.clsReportPdfBuilder;

namespace WebApplication2.Controllers
{
    [Route("api/ctlReportTemplate")]
    public class ctlReportTemplate : Controller
    {
        [HttpGet]
        [Route("SelectReportTemplateByID")]
        public string SelectReportTemplateByID(int ID, int CompanyID)
        {
            try
            {
                clsReportTemplate cls = new clsReportTemplate();
                DataTable dt = cls.SelectReportTemplateByID(ID, CompanyID);
                if (dt != null)
                    return JsonConvert.SerializeObject(dt);
                return "";
            }
            catch (Exception)
            {
                throw;
            }
        }

        [HttpGet]
        [Route("SelectReportTemplateList")]
        public string SelectReportTemplateList(string TemplateType, string EntityName, int CompanyID)
        {
            try
            {
                clsReportTemplate cls = new clsReportTemplate();
                DataTable dt = cls.SelectReportTemplateList(Simulate.String(TemplateType), Simulate.String(EntityName), CompanyID);
                if (dt != null)
                    return JsonConvert.SerializeObject(dt);
                return "";
            }
            catch (Exception)
            {
                throw;
            }
        }

        [HttpGet]
        [Route("SelectLatestActiveTemplate")]
        public string SelectLatestActiveTemplate(string TemplateType, string EntityName, int CompanyID)
        {
            try
            {
                clsReportTemplate cls = new clsReportTemplate();
                DataTable dt = cls.SelectLatestActiveTemplate(Simulate.String(TemplateType), Simulate.String(EntityName), CompanyID);
                if (dt != null)
                    return JsonConvert.SerializeObject(dt);
                return "";
            }
            catch (Exception)
            {
                throw;
            }
        }

        [HttpGet]
        [Route("DeleteReportTemplateByID")]
        public bool DeleteReportTemplateByID(int ID, int CompanyID)
        {
            try
            {
                clsReportTemplate cls = new clsReportTemplate();
                return cls.DeleteReportTemplateByID(ID, CompanyID);
            }
            catch (Exception)
            {
                throw;
            }
        }

        [HttpPost]
        [Route("InsertReportTemplate")]
        public int InsertReportTemplate(string TemplateName, string TemplateType, string EntityName,
            int CompanyID, int CreationUserId,[FromBody] string TemplateJson)
        {
            try
            {
               // string details = JsonConvert.DeserializeObject<string>(TemplateJson);
                clsReportTemplate cls = new clsReportTemplate();
                int A = cls.InsertReportTemplate(
                    Simulate.String(TemplateName),
                    Simulate.String(TemplateType),
                    Simulate.String(EntityName),
                    Simulate.String(TemplateJson),
                    CompanyID,
                    CreationUserId
                );
                return A;
            }
            catch (Exception)
            {
                throw;
            }
        }

        [HttpPost]
        [Route("UpdateReportTemplate")]
        public int UpdateReportTemplate(int ID, string TemplateName, string TemplateType, string EntityName,  
            int ModificationUserId, int CompanyID, [FromBody] string TemplateJson)
        {
            try
            {
      //         string details = JsonConvert.DeserializeObject<string>(TemplateJson);
                clsReportTemplate cls = new clsReportTemplate();
                int A = cls.UpdateReportTemplate(
                    ID,
                    Simulate.String(TemplateName),
                    Simulate.String(TemplateType),
                    Simulate.String(EntityName),
                    Simulate.String(TemplateJson),
                    ModificationUserId,
                    CompanyID
                );
                return A;
            }
            catch (Exception)
            {
                throw;
            }
        }

        [HttpGet]
        [Route("SetActive")]
        public int SetActive(int ID, bool IsActive, int ModificationUserId, int CompanyID)
        {
            try
            {
                clsReportTemplate cls = new clsReportTemplate();
                return cls.SetActive(ID, IsActive, ModificationUserId, CompanyID);
            }
            catch (Exception)
            {
                throw;
            }
        }
        [HttpGet]
        [Route("PrintPdf")]
        public IActionResult PrintPdf(string TemplateType, string EntityName, string TransactionId, int CompanyID)
        {
            try
            {
                // 1) Get latest active template for (TemplateType + EntityName)
                clsReportTemplate cls = new clsReportTemplate();
                DataTable dtTpl = cls.SelectLatestActiveTemplate(
                    Simulate.String(TemplateType),
                    Simulate.String(EntityName),
                    CompanyID
                );

                if (dtTpl == null || dtTpl.Rows.Count == 0)
                    return BadRequest("No active template found.");

                // IMPORTANT: adjust column name if your DB column differs
                string templateJson = Simulate.String(dtTpl.Rows[0]["TemplateJson"]);
                string templateName = Simulate.String(dtTpl.Rows[0]["TemplateName"]);

                if (string.IsNullOrWhiteSpace(templateJson))
                    return BadRequest("TemplateJson is empty.");

                // 2) Build PrintData from your transaction
                // TODO: Replace this with your real transaction loader based on EntityName/TransactionId
                PrintData data = BuildTransactionPrintData(EntityName, TransactionId, CompanyID);

                // 3) Build PDF bytes
                byte[] pdfBytes = clsReportPdfBuilder.Build(templateJson, data);

                // 4) Return PDF file
                var fileName = $"{templateName}_{TransactionId}.pdf";
                return File(pdfBytes, "application/pdf", fileName);
            }
            catch (Exception ex)
            {
                return BadRequest("PrintPdf error: " + ex.Message);
            }
        }

        // ==========================
        // ✅ PUT THIS HELPER INSIDE THE SAME CONTROLLER
        // ==========================
        private PrintData BuildTransactionPrintData(string entityName, string trxId, int companyId)
        {
            // IMPORTANT:
            // - Keys must match your template fields exactly:
            //   Example: {InvoiceNo} => Header["InvoiceNo"]
            //   Table field "ItemName" => each Lines row has ["ItemName"]

            var d = new PrintData();

            // -------------------------
            // 1) Company-level
            // -------------------------
            // TODO: Load from DB (Company table) + logo base64 if you want
            d.Company["CompanyName"] = "MT SOFTS";
            d.Company["LogoBase64"] = ""; // optional

            // -------------------------
            // 2) Header-level (example)
            // -------------------------
            // TODO: Load header by trxId from DB
            d.Header["TransactionId"] = trxId;
            d.Header["PrintDate"] = DateTime.Now.ToString("yyyy-MM-dd HH:mm");

            // Example fields for invoice (change to your fields)
            d.Header["InvoiceNo"] = trxId;
            d.Header["InvoiceDate"] = DateTime.Now.ToString("yyyy-MM-dd");
            d.Header["SupplierName"] = "Supplier X";

            // -------------------------
            // 3) Detail lines (example)
            // -------------------------
            // TODO: Load lines from DB by trxId
            d.Lines.Add(new Dictionary<string, object>
            {
                ["ItemName"] = "Item A",
                ["Qty"] = 2,
                ["Price"] = 10,
                ["Total"] = 20
            });

            d.Lines.Add(new Dictionary<string, object>
            {
                ["ItemName"] = "Item B",
                ["Qty"] = 1,
                ["Price"] = 15,
                ["Total"] = 15
            });

            // -------------------------
            // 4) Footer totals (example)
            // -------------------------
            d.Footer["GrandTotal"] = 35;

            return d;
        }
        private static Dictionary<string, object> DataRowToDict(DataRow row)
        {
            var dict = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);
            foreach (DataColumn col in row.Table.Columns)
            {
                var val = row[col];
                dict[col.ColumnName] = val == DBNull.Value ? null : val;
            }
            return dict;
        }

        private static List<Dictionary<string, object>> DataTableToList(DataTable dt)
        {
            var list = new List<Dictionary<string, object>>();
            if (dt == null) return list;

            foreach (DataRow r in dt.Rows)
                list.Add(DataRowToDict(r));

            return list;
        }

        private static Dictionary<string, object> FirstRowOrEmpty(DataTable dt)
        {
            if (dt == null || dt.Rows.Count == 0)
                return new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);

            return DataRowToDict(dt.Rows[0]);
        }
        private void LoadTransactionTables(string pageName, string headerGuid, int companyId,
    out DataTable dtHeader, out DataTable dtDetails)
        {
            dtHeader = new DataTable();
            dtDetails = new DataTable();

            if (pageName == "InvoicePageAdd")
            {
                var clsInvoiceDetails = new clsInvoiceDetails();
                var clsInvoiceHeader = new clsInvoiceHeader();

                dtHeader = clsInvoiceHeader.SelectInvoiceHeaderByGuid(
                    headerGuid,
                    DateTime.Now.AddYears(-100),
                    DateTime.Now.AddYears(100),
                    0, 0, 0,
                    companyId
                );

                dtDetails = clsInvoiceDetails.SelectInvoiceDetailsByHeaderGuid(headerGuid, "", companyId);
            }
            else if (pageName == "CreditNotePageAdd")
            {
                var clsCreditNoteHeader = new clsCreditNoteHeader();
                var clsCreditNoteDetails = new clsCreditNoteDetails();

                dtHeader = clsCreditNoteHeader.SelectCreditNoteHeaderByGuid(
                    headerGuid,
                    DateTime.Now.AddYears(-100),
                    DateTime.Now.AddYears(100),
                    0, 0,
                    companyId
                );

                dtDetails = clsCreditNoteDetails.SelectCreditNoteDetailsByHeaderGuid(headerGuid, companyId);
            }
            else if (pageName == "CashVoucherAdd")
            {
                var clsCashVoucherHeader = new clsCashVoucherHeader();
                var clsCashVoucherDetails = new clsCashVoucherDetails();

                dtHeader = clsCashVoucherHeader.SelectCashVoucherHeaderByGuid(
                    headerGuid,
                    DateTime.Now.AddYears(-100),
                    DateTime.Now.AddYears(100),
                    0, 0,
                    companyId, ""
                );

                dtDetails = clsCashVoucherDetails.SelectCashVoucherDetailsByHeaderGuid(headerGuid, companyId);
            }
            else if (pageName == "JournalVoucherAdd")
            {
                var clsJournalVoucherHeader = new clsJournalVoucherHeader();
                var clsJournalVoucherDetails = new clsJournalVoucherDetails();

                dtHeader = clsJournalVoucherHeader.SelectJournalVoucherHeader(
                    headerGuid, 0, 0, "", "", 0, companyId,
                    DateTime.Now.AddYears(-100),
                    DateTime.Now.AddYears(100)
                );

                dtDetails = clsJournalVoucherDetails
                    .SelectJournalVoucherDetailsByParentIdForPrint(companyId, headerGuid, 0, 0, null)
                    .Tables[0];
            }
        }
        private PrintData BuildPrintDataFromTables(DataTable dtHeader, DataTable dtDetails, int companyId, int userId)
        {
            var data = new PrintData();
            clsCompany clsCompany = new clsCompany();
        DataTable    dtCompany = clsCompany.SelectCompany(companyId, "", "", "", companyId, "");
 
            if (dtCompany != null && dtCompany.Rows.Count > 0 && dtCompany.Columns.Contains("Logo"))
            {
                var logoObj = dtCompany.Rows[0]["Logo"];
                if (logoObj != DBNull.Value && logoObj is byte[] logoBytes && logoBytes.Length > 0)
                {
                    data.Company["LogoBase64"] = Convert.ToBase64String(logoBytes);
                    data.Company["LogoMime"] = "image/png"; // لو عندك نوع الصورة خليه ديناميك
                }
            }

           










            // Company fields (fill from your company table if you want)
            data.Company["CompanyID"] = companyId;
            data.Company["UserId"] = userId;
            data.Company["PrintDate"] = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
    
            // Header: first row columns become fields
            data.Header = FirstRowOrEmpty(dtHeader);

            // Lines: each dtDetails row becomes a line map
            data.Lines = DataTableToList(dtDetails);

            // Footer: you can compute totals from dtDetails if needed (optional)
            // Example: if dtDetails has column "LineTotal"
            // decimal total = 0;
            // foreach (DataRow r in dtDetails.Rows)
            //    total += r["LineTotal"] == DBNull.Value ? 0 : Convert.ToDecimal(r["LineTotal"]);
            // data.Footer["GrandTotal"] = total;

            return data;
        }
        [HttpGet]
        [Route("PrintPdfByHeaderGuid")]
        public IActionResult PrintPdfByHeaderGuid(string HeaderGuid, string PageName, string TemplateType, string EntityName, int UserId, int CompanyID)
        {
            try
            {
                // 1) Get template JSON
                var cls = new clsReportTemplate();
                var dtTpl = cls.SelectLatestActiveTemplate(
                    Simulate.String(TemplateType),
                    Simulate.String(PageName),
                    CompanyID
                );

                if (dtTpl == null || dtTpl.Rows.Count == 0)
                    return BadRequest("No active template found.");

                string templateJson = Simulate.String(dtTpl.Rows[0]["TemplateJson"]);
                string templateName = Simulate.String(dtTpl.Rows[0]["TemplateName"]);

                // 2) Load transaction tables using your existing logic
                LoadTransactionTables(PageName, HeaderGuid, CompanyID, out var dtHeader, out var dtDetails);

                if (dtHeader == null || dtHeader.Rows.Count == 0)
                    return BadRequest("Transaction header not found.");

                // 3) Convert to PrintData
                var data = BuildPrintDataFromTables(dtHeader, dtDetails, CompanyID, UserId);
                // ✅ Fill company logo
        
                // 4) Build PDF
                byte[] pdfBytes = clsReportPdfBuilder.Build(templateJson, data);

                return File(pdfBytes, "application/pdf", $"{PageName.Replace("PageAdd", "")}_{templateName}.pdf");
            }
            catch (Exception ex)
            {
                return BadRequest("Print error: " + ex.Message);
            }
        }

    }
}
