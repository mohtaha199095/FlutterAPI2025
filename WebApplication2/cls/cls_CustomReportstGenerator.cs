using DocumentFormat.OpenXml.Drawing;
using DocumentFormat.OpenXml.ExtendedProperties;
using DocumentFormat.OpenXml.Office2016.Drawing.ChartDrawing;
using DocumentFormat.OpenXml.Spreadsheet;
using DocumentFormat.OpenXml.Wordprocessing;
using FastReport.DevComponents.AdvTree;

using PuppeteerSharp;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using WebApplication2.cls;

class InvoiceGenerator
{
    public static async Task Main(string[] args)
    {
        // Download Chromium if not already downloaded
        await new BrowserFetcher().DownloadAsync();//BrowserFetcher.DefaultRevision

   
    }

    
    public static string GenerateFooterHtml(string printUser, string printTime)
    {
        return $@"
    <div class='footer'>
        <span style='float: left;'>Printed by: {printUser}</span>
        <span style='margin: auto;'>Page <span class='pageNumber'></span> of <span class='totalPages'></span></span>
        <span style='float: right;'>Print Time: {printTime}</span>
    </div>";
    }

 

        // Generate the HTML content for the invoice
       public static async Task<byte[]> GenerateInvoicePdf(int CompanyID, int UserID, int leftMargin, int rightMargin, int topMargin, int bottomMargin,
    List<DataTable> dt, DataTable dtReportData = null, DataTable dtHeaderData = null)
    {
        try
        {
            var browser = await Puppeteer.LaunchAsync(new LaunchOptions
        {  Headless = true,
  ExecutablePath = @"C:\inetpub\wwwroot\Chrome\Application\chrome.exe",
    Args = new[]
    {
        "--no-sandbox",
        "--disable-gpu",
        "--disable-dev-shm-usage",
        "--remote-debugging-port=9222"
    },
    Timeout = 60000,
    DumpIO = true,
    UserDataDir = @"C:\Temp\Puppeteer"
            });

            //var browser = await Puppeteer.LaunchAsync(new LaunchOptions
            //{
            //    Headless = true,//C:\inetpub\wwwroot\Chrome\Application
            //   // ExecutablePath = @"C:\Program Files\Google\Chrome\Application\chrome.exe"
            //      ExecutablePath = @"C:\inetpub\wwwroot\Chrome\Application\chrome.exe"
            //});

            var page = await browser.NewPageAsync();

        // Generate the HTML content for the invoice
        string htmlContent = GenerateHtmlInvoiceContent(CompanyID, UserID, leftMargin, rightMargin, topMargin, bottomMargin, dt, dtReportData, dtHeaderData);

        // Fetch Print User from database
        clsEmployee clsEmployee = new clsEmployee();
        DataTable dataTable = clsEmployee.SelectEmployee(UserID, "", "", "", "", "", "", CompanyID, 1);
        string printUser = (dataTable != null && dataTable.Rows.Count > 0) ? Simulate.String(dataTable.Rows[0]["AName"]) : "Unknown User";

        string printTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

        // Footer for PDF (Page Number + User Info)
        string footerTemplate = $@"
    <div style='width: 100%; font-size: 12px; text-align: center; color: black; display: flex; justify-content: space-between; padding: 5px 20px;'>
        <span style='text-align: left;'>Printed by: {printUser}</span>
        <span style='text-align: center;'>Page <span class='pageNumber'></span> of <span class='totalPages'></span></span>
        <span style='text-align: right;'>Print Time: {printTime}</span>
    </div>";

        // Complete HTML document
        string fullHtml = $@"
<!DOCTYPE html>
<html lang='ar'>
<head>
    <meta charset='UTF-8'>
    <meta name='viewport' content='width=device-width, initial-scale=1.0'>
    <title>Invoice</title>
    <style>
        body {{ margin: 0; padding: 0;  }}
     
       th, td {{margin: 0;
            padding: 0;
             
        }}
 
 
    </style>
</head>
<body>
    {htmlContent}
</body>
</html>";


        // Set the content in Puppeteer
        await page.SetContentAsync(fullHtml);

        // Generate PDF with proper footer
        byte[] pdf = await page.PdfDataAsync(new PdfOptions
        {
            Format = PuppeteerSharp.Media.PaperFormat.A4,
            DisplayHeaderFooter = true,  // Required for page numbers
            PrintBackground = true,      // Ensures styles and colors are preserved
            MarginOptions = new PuppeteerSharp.Media.MarginOptions
            {
                Bottom = "50px",
                Top = "20px"
            },
            FooterTemplate = footerTemplate
        });

        await browser.CloseAsync();
        return pdf;
        }
        catch (Exception ex)
        {

            throw;
        }
    }


    static DataTable dtCompany ;
    // Function to generate the HTML content for the invoice
    public static string GenerateHtmlInvoiceContent(int CompanyID, int UserID,
        //     iTextSharp.text.Rectangle PageSize,
        int leftMargin,
        int rightMargin,
        int topMargin,
        int bottomMargin,
   List<DataTable> dt, DataTable dtReportData = null, DataTable dtHeaderData = null)
    {
        try
        {

      
        var htmlBuilder = new DynamicHtmlBuilder();
        clsEmployee clsEmployee = new clsEmployee();
        DataTable dataTable = clsEmployee.SelectEmployee(UserID, "", "", "", "", "", "", CompanyID, 1);
        string PrintUser = "";
        if (dataTable != null && dataTable.Rows.Count > 0)
        {
            PrintUser = Simulate.String(dataTable.Rows[0]["AName"]);
        }
   
        clsCompany clsCompany = new clsCompany();
          dtCompany = clsCompany.SelectCompany(CompanyID, "", "", "", CompanyID, "");

        if (dt != null && dt.Count > 0)
        {
            for (global::System.Int32 i = 0; i < dt.Count; i++)
            {
                DataTable dtcurrent = dt[i];
                var groups = dtcurrent.AsEnumerable().GroupBy(row => row.Field<int>("rowindex"));
            
                foreach (var group in groups)
                {
                    DataTable newTable = group.CopyToDataTable();
                        if ((newTable.Select().Any(row => Simulate.String(row["Type"]) == "DatatableDetails")))
                        {
                            for (global::System.Int32 j = 0; j < dtReportData.Rows.Count; j++)
                            {
                                List<string> row = getTopHeader(htmlBuilder, newTable, dtHeaderData, dtReportData, j);

                                htmlBuilder.AddRow(row);
                            }
                        } else if (newTable.Select().Any(row => Simulate.String(row["Type"]) == "DatatableTotal")) {

                            List<string> row = getTopHeader(htmlBuilder, newTable, dtHeaderData, dtReportData, 0);

                            htmlBuilder.AddRow(row);

                        }

                        else
                        {
                            List<string> row = getTopHeader(htmlBuilder, newTable, dtHeaderData, dtReportData, 0);

                            htmlBuilder.AddRow(row);
                        }
                }
            }
        }

          
   

           var ss =   htmlBuilder.GetHtmlContent();
             
        return ss;

        }
        catch (Exception ex)
        {

            return ex.ToString();
        }
;
    }
 

    static List<string> getTopHeader(DynamicHtmlBuilder htmlBuilder,
    DataTable dtTopHeader, DataTable dtHeader = null, DataTable dtDataDetails = null, int dtrowindex = 0)
    {
        try
        {

     
        List<string> a = [];


        DataView dv = dtTopHeader.DefaultView;
        dv.Sort = "ID ASC"; // or DESC
        dtTopHeader = dv.ToTable();
    

        for (int i = 0; i < dtTopHeader.Rows.Count; i++)
        {
          
          //  columnWidths[i] = (float)Simulate.Val(dtTopHeader.Rows[i]["Width"]);
            string horizontalAlignment = "CENTER";
            if (Simulate.String(dtTopHeader.Rows[i]["HorizontalAlignment"]) != "")
            {
                horizontalAlignment = Simulate.String(dtTopHeader.Rows[i]["HorizontalAlignment"]);
 
            }
            string verticalAlignment = "MIDDLE";
            if (Simulate.String(dtTopHeader.Rows[i]["VerticalAlignment"]) != "")
            {
                verticalAlignment = Simulate.String(dtTopHeader.Rows[i]["VerticalAlignment"]);
         
            }
            string width = Simulate.String( ((float)(Simulate.Val(dtTopHeader.Rows[i]["Width"]) ))*800)+ "px";
        

            int fixedHeight = 50;
            if (Simulate.Val(dtTopHeader.Rows[i]["Height"]) > 0)
            {
                fixedHeight = Simulate.Integer32( (float)Simulate.Val(dtTopHeader.Rows[i]["Height"]));
            }

            string font = "Arial";
            if (Simulate.String(dtTopHeader.Rows[i]["Font"]) != "")
            {
                try
                {
                    font = Simulate.String(dtTopHeader.Rows[i]["Font"]);

                }
                catch (Exception ex)
                {


                }


            }
            int fontsize = 12;
            if (Simulate.Integer32(dtTopHeader.Rows[i]["FontSize"]) > 0)
            {
                fontsize = Simulate.Integer32(dtTopHeader.Rows[i]["FontSize"]);
            }
            string baseColor = "";
            if (Simulate.String(dtTopHeader.Rows[i]["FontColor"]) != "")
            {

                
                long colorValue = long.Parse(Simulate.String(dtTopHeader.Rows[i]["FontColor"]));

                // Extract RGB values from the color value
                int r = (int)((colorValue >> 16) & 0xFF); // Red component
                int g = (int)((colorValue >> 8) & 0xFF);  // Green component
                int b = (int)(colorValue & 0xFF);         // Blue component

                // Format the background color for use in HTML (CSS rgb format)
                baseColor = $"rgb({r}, {g}, {b})";
            }

            string backgroundColor = "";
            if (Simulate.String(dtTopHeader.Rows[i]["BackColor"]) != "")
            {
                long colorValue = long.Parse(Simulate.String(dtTopHeader.Rows[i]["BackColor"]));

                // Extract RGB values from the color value
                int r = (int)((colorValue >> 16) & 0xFF); // Red component
                int g = (int)((colorValue >> 8) & 0xFF);  // Green component
                int b = (int)(colorValue & 0xFF);         // Blue component

                // Format the background color for use in HTML (CSS rgb format)
                backgroundColor = $"rgb({r}, {g}, {b})";
            }
 

 
            switch (Simulate.String(dtTopHeader.Rows[i]["Type"]))
            {
                case "@@EmptyValue":
                    {


                        var c2 = htmlBuilder.createCell("", font, fontsize, baseColor, backgroundColor
                                      , (Simulate.String(dtTopHeader.Rows[i]["FontWeight"])) == "Bold",
                                      (Simulate.String(dtTopHeader.Rows[i]["FontWeight"])) == "Italic",
                                        (Simulate.Bool(dtTopHeader.Rows[i]["WithBoarder"])) ? "1px solid black" : "0px solid black",
                                        (Simulate.String(dtTopHeader.Rows[i]["HorizontalAlignment"])),
                                          (Simulate.String(dtTopHeader.Rows[i]["VerticalAlignment"]))
                                          , width, fixedHeight);
                           

                            a.Add(c2);
                        break;
                    }
                case "@@CompanyLogo":
                    {
                     

                  
                        var ss = htmlBuilder.createImageCell(  (byte[])dtCompany.Rows[0]["Logo"],
                            "", width, fixedHeight, backgroundColor,
                             (Simulate.Bool(dtTopHeader.Rows[i]["WithBoarder"])) ? ".5px solid black" : "0px solid black",
                            (Simulate.String(dtTopHeader.Rows[i]["HorizontalAlignment"])),
                            (Simulate.String(dtTopHeader.Rows[i]["VerticalAlignment"])), "0", "0");


                          


                            a.Add(ss);
                       
                        break;
                    }
                case "@@Address":
                    {

                        string Address = dtCompany.Rows[0]["Address"].ToString();

            


                        var c2 = htmlBuilder.createCell(Address, font, fontsize, baseColor, backgroundColor
                             , (Simulate.String(dtTopHeader.Rows[i]["FontWeight"])) == "Bold",
                             (Simulate.String(dtTopHeader.Rows[i]["FontWeight"])) == "Italic",
                               (Simulate.Bool(dtTopHeader.Rows[i]["WithBoarder"])) ? ".5px solid black" : "0px solid black",
                               (Simulate.String(dtTopHeader.Rows[i]["HorizontalAlignment"])),
                                 (Simulate.String(dtTopHeader.Rows[i]["VerticalAlignment"]))
                                 , width, fixedHeight);


                        a.Add(c2);
                        break;


                    }
                case "@@CompanyName":
                    {
                        string companyName = dtCompany.Rows[0]["AName"].ToString();

                        var c2 = htmlBuilder.createCell(companyName, font, fontsize, baseColor, backgroundColor
                                , (Simulate.String(dtTopHeader.Rows[i]["FontWeight"])) == "Bold",
                                (Simulate.String(dtTopHeader.Rows[i]["FontWeight"])) == "Italic",
                                  (Simulate.Bool(dtTopHeader.Rows[i]["WithBoarder"])) ? ".5px solid black" : "0px solid black",
                                  (Simulate.String(dtTopHeader.Rows[i]["HorizontalAlignment"])),
                                    (Simulate.String(dtTopHeader.Rows[i]["VerticalAlignment"]))
                                    , width, fixedHeight);


                        a.Add(c2);
                        break;

                    }
                default:
                    string text = Simulate.String(dtTopHeader.Rows[i]["OtherValue"]);
                    if (Simulate.String(dtTopHeader.Rows[i]["Type"]) == "Datatable")
                    {
                        text = Simulate.String(dtTopHeader.Rows[i]["OtherValue"]);

                    }
                    else if (Simulate.String(dtTopHeader.Rows[i]["Type"]) == "DatatableDetails")
                    {
                        text = Simulate.String(dtTopHeader.Rows[i]["OtherValue"]);
                        text = Simulate.String(dtDataDetails.Rows[dtrowindex][text]);

                    } else if (Simulate.String(dtTopHeader.Rows[i]["Type"]) == "DatatableTotal") {


                        string col = Simulate.String(dtTopHeader.Rows[i]["OtherValue"]);
                        decimal total = 0;
                        for (global::System.Int32 j = 0; j < dtDataDetails.Rows.Count; j++)
                        {
                            total = total + Simulate.decimal_(dtDataDetails.Rows[j][col]);


                        }
                        text = Simulate.Currency_format(total);

                    }
                    else if (Simulate.String(dtTopHeader.Rows[i]["Type"]) == "HeaderData")
                    {
                        text = Simulate.String(dtTopHeader.Rows[i]["OtherValue"]);
                        text = Simulate.String(dtHeader.Rows[dtrowindex][text.Replace("##", "")]);

                    }

                    
                    var c = htmlBuilder.createCell(text, font, fontsize, baseColor, backgroundColor
                        , (Simulate.String(dtTopHeader.Rows[i]["FontWeight"])) == "Bold",
                        (Simulate.String(dtTopHeader.Rows[i]["FontWeight"])) == "Italic",
                          (Simulate.Bool(dtTopHeader.Rows[i]["WithBoarder"]))  ? ".5px solid black" : "0px solid black",
                          (Simulate.String(dtTopHeader.Rows[i]["HorizontalAlignment"])),
                            (Simulate.String(dtTopHeader.Rows[i]["VerticalAlignment"]))
                            , width, fixedHeight);

                   
                    a.Add(c);
                    break;
            }
        }

       // headerTable.SetWidth(pageSize.GetWidth());
 


        return a;
        }
        catch (Exception ex)
        {

            throw;
        }
    }

}
public class DynamicHtmlBuilder
{
    private StringBuilder htmlContent;

    public DynamicHtmlBuilder()
    {
        htmlContent = new StringBuilder();
    }

   

 

    // Add a row with cells (supports dynamic text and styling)
    public void AddRow(List<string> cellValues)
    {    
        string html = "<table style='border-spacing: 0px; padding: 0px;'>";
         htmlContent.Append(html);
        htmlContent.Append("<thead>");
        htmlContent.Append("<tr>");
        foreach (var value in cellValues)
        {
         

            string style = "padding: 0px;"; // Adjust the padding as needed
      //      htmlContent.Append($"<td style='{style}'>{value}</td>");
            htmlContent.Append(value);
        }
        htmlContent.Append("</tr>"); 
        htmlContent.Append("</thead>");
        htmlContent.Append("</table>");
    }
    public string createImageCell(
     byte[] logoBytes,                          // Image in byte array format
    string altText = "Image",                   // Alternative text for the image
    string width = "auto",                      // Width of the image (e.g., "50%" or "100px")
    int height = 30,                            // Height of the image in pixels
    string backgroundColor = "white",           // Background color of the cell
    string border = "0px solid black",          // Border style for the cell
    string horizontalAlign = "center",         // Horizontal alignment of the image in the cell
    string verticalAlign = "middle",           // Vertical alignment of the image in the cell
    string padding = "0",                      // Padding around the image
    string margin = "0"                        // Margin around the image
)
    {
        // Convert the byte array to a Base64 string
        string base64Image = Convert.ToBase64String(logoBytes);

        // Base style for the cell
        string style = $"background-color: {backgroundColor}; " +
                       $"border: {border}; text-align: {horizontalAlign}; vertical-align: {verticalAlign}; " +
                       $"width: {width}; height: {height}px; padding: {padding}; margin: {margin}; ";
    //    +$"display: inline-block; box-sizing: border-box;";

        // Return the HTML for the image cell with the <img> tag containing the Base64 image data
        return $"<td style='{style}'><img src='data:image/jpeg;base64,{base64Image}' alt='{altText}' width='{width}' height='{height}' /></td>";
    }
    public string createCell(
    string value,
    string font = "Arial",
    int fontSize = 12,
    string color = "black",
    string backgroundColor = "white",
    bool isBold = false,
    bool isItalic = false,
    string border = "0px solid black",
    string horizontalAlign = "center",  // Left, center, right
    string verticalAlign = "middle",    // Top, middle, bottom
    string width = "auto",              // Width in percentage or fixed size (e.g., "50%" or "100px")
    int height = 30                      // Height in pixels
)
    {
        if (verticalAlign == "Center") {
            verticalAlign = "middle";
        }
        //backgroundColor = "red";
        // Base style for the cell
        string style = $"font-family: {font} !important; font-size: {fontSize}px !important; color: {color} !important; background-color: {backgroundColor} !important; " +
                    $"border: {border} !important; text-align: {horizontalAlign} !important; vertical-align: {verticalAlign} !important; " +
                    $"width: {width} !important; height: {height}px !important;display: table-cell !important;";

        // Add font styles for bold and italic
        if (isBold)
        {

            style += " font-weight: bold !important;";
        }

        else if (isItalic)
        {
            style += " font-style: italic !important;";

        }
        else {
            style += " font-weight: normal !important;";

        }
        // Return the HTML for the cell
        return $"<th style='vertical-align:top' > <div style='{style}'>{value} </div> </th>";




    }
   
    
   

    // Get the full HTML content
    public string GetHtmlContent()
    {
        htmlContent.Append("</body></html>");
        return htmlContent.ToString();
    }
}