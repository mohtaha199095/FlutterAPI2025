using FastReport;
using FastReport.Web;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Data;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using WebApplication2.cls;
using WebApplication2.cls.Reports;

namespace WebApplication2.Controllers
{
    /// <summary>
    /// FastReport Online Designer (browser) + FRX upload for tbl_TransactionReport.
    /// Copy WebReportDesigner from FastReport into wwwroot/WebReportDesigner.
    /// </summary>
    public class FastReportDesignerController : Controller
    {
        private static bool DesignerAssetsPresent =>
            System.IO.File.Exists(Path.Combine(
                Directory.GetCurrentDirectory(),
                "wwwroot",
                "WebReportDesigner",
                "index.html"));

        [HttpGet]
        public IActionResult Designer(int TransactionReportID, int CompanyID, int UserId)
        {
            try
            {
                if (!DesignerAssetsPresent)
                {
                    return Content(
                        BuildDesignerMissingHtml(TransactionReportID, CompanyID),
                        "text/html");
                }

                clsTransactionReportPrint printer = new clsTransactionReportPrint();
                Report report = printer.BuildDesignReport(
                    TransactionReportID, CompanyID, UserId);

                FastReport.Utils.Config.WebMode = true;
                WebReport webReport = new WebReport
                {
                    Width = "100%",
                    Height = "100vh",
                    Report = report,
                };

                webReport.Mode = WebReportMode.Designer;
                webReport.DesignScriptCode = false;
                webReport.Debug = false;
                webReport.DesignerPath = "/WebReportDesigner/index.html";
                webReport.DesignerSaveCallBack =
                    $"/FastReportDesigner/SaveDesignedReport?TransactionReportID={TransactionReportID}" +
                    $"&CompanyID={CompanyID}&UserId={UserId}";

                ViewBag.TransactionReportID = TransactionReportID;
                ViewBag.CompanyID = CompanyID;
                ViewBag.UserId = UserId;
                return View("Designer", webReport);
            }
            catch (Exception ex)
            {
                return Content(
                    "<html><body><h3>Designer error</h3><pre>" +
                    System.Net.WebUtility.HtmlEncode(ex.Message) +
                    "</pre></body></html>",
                    "text/html");
            }
        }

        [HttpPost]
        public IActionResult SaveDesignedReport(
            int TransactionReportID,
            int CompanyID,
            int UserId,
            string reportID = "",
            string reportUUID = "")
        {
            try
            {
                int companyId = CompanyID;
                int transactionReportId = TransactionReportID;

                if (transactionReportId <= 0 && clsTransactionReportPrint.TryParseDesignerReportId(
                        reportID, out companyId, out transactionReportId))
                {
                    // fallback when designer sends reportID only
                }

                if (transactionReportId <= 0 || companyId <= 0)
                    return BadRequest("Unknown report.");

                using var ms = new MemoryStream();
                Request.Body.CopyTo(ms);
                ms.Position = 0;
                string frxXml;
                using (var sr = new StreamReader(
                    ms, Encoding.UTF8, detectEncodingFromByteOrderMarks: true))
                {
                    frxXml = sr.ReadToEnd();
                }
                frxXml = clsTransactionReportPrint.StripFrxBom(frxXml);
                if (string.IsNullOrWhiteSpace(frxXml))
                    return BadRequest("Empty report content.");

                clsTransactionReport tr = new clsTransactionReport();
                int rows = tr.UpdateReportFrxXml(
                    transactionReportId,
                    frxXml,
                    UserId > 0 ? UserId : 0,
                    companyId);

                ViewBag.Message = rows > 0
                    ? "Layout saved successfully. You can close this tab and preview from the ERP."
                    : "Layout was not saved.";
                return View("SaveResult");
            }
            catch (Exception ex)
            {
                ViewBag.Message = "Save error: " + ex.Message;
                return View("SaveResult");
            }
        }

        private static string BuildDesignerMissingHtml(int transactionReportId, int companyId)
        {
            return @"<!DOCTYPE html><html><head><meta charset=""utf-8""/>
<title>FastReport Designer</title></head><body style=""font-family:Segoe UI,sans-serif;padding:24px"">
<h2>FastReport Online Designer files are not installed on the server</h2>
<p>Copy the <strong>WebReportDesigner</strong> folder from your FastReport distribution into:</p>
<pre>wwwroot/WebReportDesigner</pre>
<p>Until then, use <strong>Upload .frx</strong> from the ERP app (Settings → Transaction Reports → Customize).</p>
<p>TransactionReportID: " + transactionReportId + @" · CompanyID: " + companyId + @"</p>
</body></html>";
        }
    }

    [Route("api/ctlFastReportDesigner")]
    public class ctlFastReportDesigner : Controller
    {
        [HttpGet]
        [Route("GetTransactionReportFrx")]
        public IActionResult GetTransactionReportFrx(
            int TransactionReportID, int CompanyID, int UserId)
        {
            try
            {
                clsTransactionReportPrint printer = new clsTransactionReportPrint();
                string xml = printer.GetFrxXmlForTransactionReport(
                    TransactionReportID, CompanyID, UserId);
                return Content(xml, "application/xml", Encoding.UTF8);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet]
        [Route("GetTransactionReportLayoutSource")]
        public IActionResult GetTransactionReportLayoutSource(
            int TransactionReportID, int CompanyID, int UserId)
        {
            try
            {
                return Ok(new clsTransactionReportPrint()
                    .DescribeLayoutSource(TransactionReportID, CompanyID));
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost]
        [Route("SaveTransactionReportFrx")]
        public async Task<IActionResult> SaveTransactionReportFrx(
            int TransactionReportID,
            int CompanyID,
            int UserId)
        {
            try
            {
                using var reader = new StreamReader(
                    Request.Body, Encoding.UTF8, detectEncodingFromByteOrderMarks: true);
                string frxXml = await reader.ReadToEndAsync();
                frxXml = clsTransactionReportPrint.StripFrxBom(frxXml);
                if (string.IsNullOrWhiteSpace(frxXml))
                    return BadRequest("FRX content is empty.");

                clsTransactionReport tr = new clsTransactionReport();
                int rows = tr.UpdateReportFrxXml(
                    TransactionReportID, frxXml, UserId, CompanyID);
                return Ok(new { saved = rows > 0 });
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost]
        [Route("UploadTransactionReportFrx")]
        public async Task<IActionResult> UploadTransactionReportFrx(
            int TransactionReportID,
            int CompanyID,
            int UserId,
            IFormFile file)
        {
            try
            {
                if (file == null || file.Length == 0)
                    return BadRequest("No file uploaded.");

                using var ms = new MemoryStream();
                await file.CopyToAsync(ms);
                ms.Position = 0;
                string frxXml;
                using (var sr = new StreamReader(
                    ms, Encoding.UTF8, detectEncodingFromByteOrderMarks: true))
                {
                    frxXml = sr.ReadToEnd();
                }
                frxXml = clsTransactionReportPrint.StripFrxBom(frxXml);
                if (string.IsNullOrWhiteSpace(frxXml))
                    return BadRequest("File is empty.");

                clsTransactionReport tr = new clsTransactionReport();
                int rows = tr.UpdateReportFrxXml(
                    TransactionReportID, frxXml, UserId, CompanyID);
                return Ok(new { saved = rows > 0, fileName = file.FileName });
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost]
        [Route("ResetTransactionReportFrxFromFile")]
        public IActionResult ResetTransactionReportFrxFromFile(
            int TransactionReportID,
            int CompanyID,
            int UserId)
        {
            try
            {
                DataTable dt = new clsTransactionReport().SelectTransactionReportByID(
                    TransactionReportID, CompanyID);
                if (dt == null || dt.Rows.Count == 0)
                    return BadRequest("Report not found.");

                string frxName = Simulate.String(dt.Rows[0]["FastReportFileName"]);
                if (string.IsNullOrWhiteSpace(frxName))
                    frxName = clsTransactionReportPrint.DefaultJvFrxFileName;

                // Clear DB stored XML → print falls to company file → standard file.
                int rows = new clsTransactionReport().ClearReportFrxXml(
                    TransactionReportID, UserId, CompanyID);

                return Ok(new { saved = rows > 0, cleared = true });
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet]
        [Route("DesignerUrl")]
        public IActionResult DesignerUrl(
            int TransactionReportID,
            int CompanyID,
            int UserId)
        {
            string baseUrl = $"{Request.Scheme}://{Request.Host}";
            string url =
                $"{baseUrl}/FastReportDesigner/Designer?TransactionReportID={TransactionReportID}" +
                $"&CompanyID={CompanyID}&UserId={UserId}";
            bool installed = System.IO.File.Exists(Path.Combine(
                Directory.GetCurrentDirectory(),
                "wwwroot",
                "WebReportDesigner",
                "index.html"));
            return Ok(new { url, designerInstalled = installed });
        }
    }
}
