//using System;
//using System.Collections.Generic;
//using System.IO;
//using iText.Kernel.Pdf;
//using iText.Layout;
//using iText.Layout.Element;
//using iText.Kernel.Colors;
//using iText.Layout.Properties;
//using iText.IO.Font;
//using iText.Kernel.Font;

//using System.Drawing;
////using DocumentFormat.OpenXml.Office2010.PowerPoint;
//using iText.StyledXmlParser.Jsoup.Nodes;
//using System.Data;
//using iText.Layout.Borders;
////using DocumentFormat.OpenXml.Wordprocessing;
//using Paragraph = iText.Layout.Element.Paragraph;
//using TextAlignment = iText.Layout.Properties.TextAlignment;
////using DocumentFormat.OpenXml.Office2010.PowerPoint;
//using Border = iText.Layout.Borders.Border;
//using WebApplication2.cls;
//using System.Linq;
//using iText.IO.Image;
//using FastReport;
//using iText.Kernel.Pdf.Canvas;
////using DocumentFormat.OpenXml.Wordprocessing;
////using DocumentFormat.OpenXml.Wordprocessing;
//public class clsReportsByPDFCreator
//{
//    DataTable dtCompany;
//    public byte[] createNewReport(int CompanyID, int UserID,
//        //     iTextSharp.text.Rectangle PageSize,
//        int leftMargin,
//        int rightMargin,
//        int topMargin,
//        int bottomMargin,
//   List<DataTable> dt, DataTable dtReportData = null, DataTable dtHeaderData = null)
//    {
//        try
//        {
             
//            iText.Kernel.Geom.PageSize pageSize = iText.Kernel.Geom.PageSize.A4;
//            clsEmployee clsEmployee = new clsEmployee();
//            DataTable dataTable = clsEmployee.SelectEmployee(UserID, "", "", "", "", "", "", CompanyID, 1);
//            string PrintUser = "";
//            if (dataTable != null && dataTable.Rows.Count > 0)
//            {
//                PrintUser = Simulate.String(dataTable.Rows[0]["AName"]);
//            }
//            bottomMargin = bottomMargin + 50;
//            clsCompany clsCompany = new clsCompany();
//            dtCompany = clsCompany.SelectCompany(CompanyID, "", "", "", CompanyID, "");
//            using (MemoryStream ms = new MemoryStream())
//            {
//                PdfWriter writer = new iText.Kernel.Pdf.PdfWriter(ms);

//                PdfDocument pdfDoc = new PdfDocument(writer);
               
//                iText.Layout.Document document = new iText.Layout.Document(pdfDoc, pageSize);
//                document.SetHorizontalAlignment(HorizontalAlignment.RIGHT);
//                document.SetTextAlignment(TextAlignment.RIGHT);
//                document.SetBaseDirection(BaseDirection.RIGHT_TO_LEFT);
               
//                // iTextSharp.text.Document pdfDoc = new iTextSharp.text.Document(PageSize, leftMargin, rightMargin, topMargin, bottomMargin);
//                // PdfWriter writer = PdfWriter.GetInstance(pdfDoc, ms);

//                //FooterPageEvent footerEvent = new FooterPageEvent
//                //{
//                //    UserName = PrintUser,               // Replace with dynamic user information if needed
//                //    PrintTime = DateTime.Now.ToString()       // Or a formatted version, e.g., DateTime.Now.ToString("g")
//                //};
//                //writer.PageEvent = footerEvent;
//                //  pdfDoc.AddAuthor("YourAppName");
//                //  pdfDoc.AddTitle("Credit Note Report");

//                // Open the document to begin writing text
//                // pdfDoc.Open();
//                // Initialize the footer event and assign it to the writer
         
          
                   
//                if (dt != null && dt.Count > 0)
//                {
//                    for (global::System.Int32 i = 0; i < dt.Count; i++)
//                    {
//                        DataTable dtcurrent = dt[i];
//                        var groups = dtcurrent.AsEnumerable()
//.GroupBy(row => row.Field<int>("rowindex"));

//                        foreach (var group in groups)
//                        {
//                            DataTable newTable = group.CopyToDataTable();
//                            if (Simulate.String(newTable.Rows[i]["Type"]) == "DatatableDetails")
//                            {
//                                for (global::System.Int32 j = 0; j < dtReportData.Rows.Count; j++)
//                                {
//                                    Table tablea = getTopHeader(pageSize,newTable, dtReportData, j);
//                                    document.Add(tablea);
//                                }
//                            }
//                            else
//                            {
//                                Table table = getTopHeader(pageSize, newTable, dtHeaderData, 0);
//                                document.Add(table);

//                            }
//                        }
//                    }
//               }
               

//                // Close the document
               
//                pdfDoc.Close();
//                byte[] pdfBytes = ms.ToArray();
//                return pdfBytes;
//            }
//        }
//        catch (System.Exception ex)
//        {

//            throw;
//        }
//    }
//    //    dtCompany = clsCompany.SelectCompany(CompanyID, "", "", "", CompanyID, "");
//    //    using (MemoryStream ms = new MemoryStream())
//    //    {
//    //        // Create PdfWriter instance
//    //        PdfWriter writer = new PdfWriter(ms);

//        //        // Create PdfDocument instance
//        //        PdfDocument pdfDoc = new PdfDocument(writer);

//        //        // Set default page size to A4
//        //        iText.Layout.Document document = new iText.Layout.Document(pdfDoc, iText.Kernel.Geom.PageSize.A4);

//        //        // Create Document instance
//        //     //   Document document = new Document(pdfDoc);
//        //     //  pdfDoc.SetDefaultPageSize(new Rectangle(0,0,595 , 841 )); // A4
//        //        // Set margins for the document
//        //        document.SetMargins(50, 50, 50, 50);  // top, right, bottom, left

//        //        // Create Table (3 columns in this case)
//        //        Table table = new Table(3);

//        //        // Add Header to the table
//        //        AddCellToHeader(table, "Header 1");
//        //        AddCellToHeader(table, "Header 2");
//        //        AddCellToHeader(table, "Header 3");

//        //        // Add Content to the table
//        //        table.AddCell("Content 1");
//        //        table.AddCell("Content 2");
//        //        table.AddCell("Content 3");

//        //        // Add the table to the document
//        //        document.Add(table);

//        //        // Close the document to finalize the PDF creation
//        //        document.Close();

//        //        // Return the byte array of the PDF
//        //        return ms.ToArray();
//        //    }
//        //}

//    private void AddCellToHeader(Table table, string cellText)
//    {
//        // Create the font using PdfFontFactory
//       // var font = PdfFontFactory.CreateFont(iText.IO.Font.Constants.StandardFonts.HELVETICA_BOLD);
//        PdfFont font= PdfFontFactory.CreateFont("C:/windows/fonts/arialuni.ttf",
//            PdfEncodings.IDENTITY_H);
//        // Create the cell with the font
//        var cell = new Cell().Add(new Paragraph(cellText).SetFont(font))
//                              .SetBackgroundColor(ColorConstants.DARK_GRAY)
//                              .SetTextAlignment(TextAlignment.CENTER)
//                              .SetFontColor(ColorConstants.WHITE);
//        table.AddCell("Content 1");
//        // Add the cell to the table
//        table.AddCell(cell);
//    }


//iText.Layout.Element.Table getTopHeader(iText.Kernel.Geom.PageSize pageSize,
//    DataTable dtTopHeader, DataTable dtData = null, int dtrowindex = 0)
//    {
 

//        DataView dv = dtTopHeader.DefaultView;
//    dv.Sort = "ID ASC"; // or DESC
//    dtTopHeader = dv.ToTable();
//    float[] columnWidths = new float[dtTopHeader.Rows.Count];
//        iText.Layout.Element.Table headerTable = new iText.Layout.Element.Table(dtTopHeader.Rows.Count);
  
//        headerTable.SetWidth(100); // Equivalent to WidthPercentage in iTextSharp
//        headerTable.SetBorder(Border.NO_BORDER);

//        PdfFont font = PdfFontFactory.CreateFont("C:/windows/fonts/arialuni.ttf",
//        PdfEncodings.IDENTITY_H);
//        var cell = new Cell().Add(new Paragraph( ).SetFont(font))
//                        .SetBackgroundColor(ColorConstants.DARK_GRAY)
//                        .SetTextAlignment(TextAlignment.CENTER)
//                        .SetFontColor(ColorConstants.WHITE);

//        for (int i = 0; i < dtTopHeader.Rows.Count; i++)
//    {
//        columnWidths[i] = (float)Simulate.Val(dtTopHeader.Rows[i]["Width"]);
//            iText.Layout.Properties.HorizontalAlignment horizontalAlignment = iText.Layout.Properties.HorizontalAlignment.CENTER;
//        if (Simulate.String(dtTopHeader.Rows[i]["HorizontalAlignment"]) != "")
//        {
//            switch (Simulate.String(dtTopHeader.Rows[i]["HorizontalAlignment"]))
//            {
//                case "Right":
//                        horizontalAlignment = iText.Layout.Properties.HorizontalAlignment.RIGHT;
//                        break;
//                    case "Left":
//                        horizontalAlignment = iText.Layout.Properties.HorizontalAlignment.LEFT;
//                        break;
//                case "Center":
//                        horizontalAlignment = iText.Layout.Properties.HorizontalAlignment.CENTER;
//                        break;
//                default:
//                    break;

//            }
//        }
//            iText.Layout.Properties.VerticalAlignment verticalAlignment = iText.Layout.Properties.VerticalAlignment.MIDDLE;
//            if (Simulate.String(dtTopHeader.Rows[i]["VerticalAlignment"]) != "")
//            {
//                switch (Simulate.String(dtTopHeader.Rows[i]["VerticalAlignment"]))
//                {
//                    case "Top":
//                        verticalAlignment = VerticalAlignment.TOP;
//                        break;
//                    case "Bottom":
//                        verticalAlignment = VerticalAlignment.BOTTOM;
//                        break;
//                    case "Center":
//                        verticalAlignment = VerticalAlignment.MIDDLE;
//                        break;
//                    default:
//                        break;

//                }
//            }
//            var width = (float)(Simulate.Val(dtTopHeader.Rows[i]["Width"]) * pageSize.GetWidth())*3;
//            width = width;
//            //    int fontWeight = FontWeight.NORMAL; // Default to normal weight
//            //    if (Simulate.String(dtTopHeader.Rows[i]["FontWeight"]) != "")
//            //{
//            //    switch (Simulate.String(dtTopHeader.Rows[i]["FontWeight"]))
//            //    {
//            //        case "Bold":
//            //            fontWeight = Font.BOLD;
//            //            break;
//            //        case "Italic":

//            //                  fontWeight = iText.Layout.Font.BOLD;
//            //                break;
//            //        case "Underline":
//            //                fontWeight = iText.Layout.Font.Font.UNDERLINE;
//            //                break;
//            //        default:
//            //            break;

//            //    }
//            //}

//            float fixedHeight = 50;
//        if (Simulate.Val(dtTopHeader.Rows[i]["Height"]) > 0)
//        {
//            fixedHeight = (float)Simulate.Val(dtTopHeader.Rows[i]["Height"]);
//        }

          
//            if (Simulate.String(dtTopHeader.Rows[i]["Font"]) != "")
//            {
//                try
//                {
//                    font = PdfFontFactory.CreateFont(Simulate.String(dtTopHeader.Rows[i]["Font"]), PdfEncodings.IDENTITY_H);

//                }
//                catch (Exception ex)
//                {

                    
//                }


//            }
//            int fontsize = 12;
//        if (Simulate.Integer32(dtTopHeader.Rows[i]["FontSize"]) > 0)
//        {
//            fontsize = Simulate.Integer32(dtTopHeader.Rows[i]["FontSize"]);
//        }
//            iText.Kernel.Colors.Color baseColor = ColorConstants.BLACK;
//            if (Simulate.String(dtTopHeader.Rows[i]["FontColor"]) != "")
//        {

//            long colorValue = long.Parse(Simulate.String(dtTopHeader.Rows[i]["FontColor"]));
//            int r = (int)((colorValue >> 16) & 0xFF);
//            int g = (int)((colorValue >> 8) & 0xFF);
//            int b = (int)(colorValue & 0xFF);
//            baseColor = new DeviceRgb(r, g, b);
//            }

//        iText.Kernel.Colors.Color backgroundColor = ColorConstants.WHITE;
//            if (Simulate.String(dtTopHeader.Rows[i]["BackColor"]) != "")
//        {
//            long colorValue = long.Parse(Simulate.String(dtTopHeader.Rows[i]["BackColor"]));
//            int r = (int)((colorValue >> 16) & 0xFF);
//            int g = (int)((colorValue >> 8) & 0xFF);
//            int b = (int)(colorValue & 0xFF);
//            backgroundColor = new DeviceRgb(r, g, b);
//            }

//        iText.Layout.Borders.Border boarder = Border.NO_BORDER ;
//        if (Simulate.Bool(dtTopHeader.Rows[i]["WithBoarder"]))
//        {
                 
//                boarder = iText.Layout.Borders.SolidBorder.NO_BORDER;
//            }
       
//       // BaseFont bf = BaseFont.CreateFont("C:/windows/fonts/arialuni.ttf", BaseFont.IDENTITY_H, BaseFont.EMBEDDED);
//       // Font arabicFont = new Font(bf, fontsize, fontWeight, baseColor);
//        switch (Simulate.String(dtTopHeader.Rows[i]["Type"]))
//        {
//            case "@@EmptyValue":
//                {
                   

//                        var textCell = new Cell().Add(new Paragraph("").SetFont(font))
//                           .SetBackgroundColor(backgroundColor).SetWidth(width)
//                    .SetTextAlignment(TextAlignment.CENTER)
//                           .SetFontColor(baseColor).SetBorder(boarder);
//                        headerTable.AddCell(textCell);
//                        break;
//                }
//            case "@@CompanyLogo":
//                {
//                        byte[] logoBytes = (byte[])dtCompany.Rows[0]["Logo"];

//                        // Create the image from the byte array
//                        iText.Layout.Element.Image logo = new iText.Layout.Element.Image(ImageDataFactory.Create(logoBytes));

//                        // Scale image to fit within a fixed height, for example:
//                       // float fixedHeight = 50f; // example height
//                        logo.SetAutoScale(true); // Auto-scale image to maintain aspect ratio

//                        // Set up a cell for the logo
//                        Cell logoCell = new Cell()
//                            .Add(logo)
//                            .SetBorder(boarder).SetWidth(width)
//                            .SetHeight(fixedHeight)  // match the height as required
//                            .SetVerticalAlignment(verticalAlignment)
//                            .SetHorizontalAlignment(horizontalAlignment)
//                            .SetBackgroundColor(backgroundColor);

//                        // Add logo cell to the table
//                        headerTable.AddCell(logoCell);

//                        break;
//                }
//            case "@@Address":
//                {

//                    string Address = dtCompany.Rows[0]["Address"].ToString();

//                        var textCell = new Cell().Add(new Paragraph(Address).SetFont(font))
//                           .SetBackgroundColor(backgroundColor)
//                     .SetVerticalAlignment(verticalAlignment)
//                            .SetHorizontalAlignment(horizontalAlignment).SetWidth(width)
//                           .SetFontColor(baseColor);
//                        headerTable.AddCell(textCell).SetBorder(boarder); ;
//                        break;


//                }
//            case "@@CompanyName":
//                {
//                    string companyName = dtCompany.Rows[0]["AName"].ToString();

//                        var textCell = new Cell().Add(new Paragraph(companyName).SetFont(font))
//                         .SetBackgroundColor(backgroundColor)
//                    .SetVerticalAlignment(verticalAlignment)
//                            .SetHorizontalAlignment(horizontalAlignment).SetWidth(width)
//                         .SetFontColor(baseColor);
//                        headerTable.AddCell(textCell).SetBorder(boarder); ;
//                        break;

//                }
//            default:
//                string text = Simulate.String(dtTopHeader.Rows[i]["OtherValue"]);
//                if (Simulate.String(dtTopHeader.Rows[i]["Type"]) == "Datatable")
//                {
//                    text = Simulate.String(dtTopHeader.Rows[i]["OtherValue"]);

//                }
//                else if (Simulate.String(dtTopHeader.Rows[i]["Type"]) == "DatatableDetails")
//                {
//                    text = Simulate.String(dtTopHeader.Rows[i]["OtherValue"]);
//                    text = Simulate.String(dtData.Rows[dtrowindex][text]);

//                }
//                else if (Simulate.String(dtTopHeader.Rows[i]["Type"]) == "HeaderData")
//                {
//                    text = Simulate.String(dtTopHeader.Rows[i]["OtherValue"]);
//                    text = Simulate.String(dtData.Rows[dtrowindex][text.Replace("##", "")]);

//                }

//                    //Phrase phrase = new Phrase(text, arabicFont);
//                    //PdfPCell textCell = new PdfPCell(phrase);
//                    //textCell.RunDirection = PdfWriter.RUN_DIRECTION_RTL;
//                    //    textCell.BackgroundColor = backgroundColor;




//                    //    textCell.Border = boarder;

//                    //textCell.FixedHeight = fixedHeight;
//                    //textCell.VerticalAlignment = verticalAlignment;
//                    //textCell.HorizontalAlignment = horizontalAlignment;
//                    //    headerTable.AddCell(textCell);
                 
//                    font = PdfFontFactory.CreateFont("C:/windows/fonts/arialuni.ttf", PdfEncodings.IDENTITY_H);

//                    // Create the Text object
//                    Text asd = new Text(text)
//                        .SetFont(font)
//                        .SetTextAlignment(TextAlignment.RIGHT)   // Right-align the text (for Arabic)
//                        .SetBaseDirection(BaseDirection.RIGHT_TO_LEFT);  // Ensure Right-To-Left base direction for Arabic
                   
                     
//                    // Create the cell
//                    //var textCellw = new Cell()
//                    //    .Add(new Paragraph(asd))
//                    //    .SetFont(font);
//                    //    .SetBackgroundColor(backgroundColor)
//                    //    .SetTextAlignment(TextAlignment.RIGHT)
//                    //.SetVerticalAlignment(verticalAlignment)
//                    //     .SetHorizontalAlignment(horizontalAlignment)
//                    //     .SetWidth(width)
//                    //    .SetFontColor(baseColor).SetBorder(boarder);

//                    headerTable.AddCell(textCellw);
                
//                    break;
//        }
//    }

//        headerTable.SetWidth(pageSize.GetWidth());
//        //headerTable.SetWidths(columnWidths);
//        // headerTable.SetWidths(columnWidths);


//        return headerTable;
//}

//}
////-----------------------------------
////using Azure.Core;

////using System;
////using System.Collections.Generic;
////using System.Data;
////using System.IO;

////using System.Linq;
////namespace WebApplication2.cls.Reports
////{
////    public class clsReportsByPDFCreator
////    {
////        DataTable dtCompany;
////        public byte[] createNewReport(int CompanyID, int UserID,
////            //     iTextSharp.text.Rectangle PageSize,
////            int leftMargin,
////            int rightMargin,
////            int topMargin,
////            int bottomMargin,
////       List<DataTable> dt, DataTable dtReportData = null, DataTable dtHeaderData = null)
////        {
////            try
////            {


////                clsEmployee clsEmployee = new clsEmployee();
////                DataTable dataTable = clsEmployee.SelectEmployee(UserID, "", "", "", "", "", "", CompanyID, 1);
////                string PrintUser = "";
////                if (dataTable != null && dataTable.Rows.Count > 0)
////                {
////                    PrintUser = Simulate.String(dataTable.Rows[0]["AName"]);
////                }



////                bottomMargin = bottomMargin + 50;
////                clsCompany clsCompany = new clsCompany();
////                dtCompany = clsCompany.SelectCompany(CompanyID, "", "", "", CompanyID, "");
////                using (MemoryStream ms = new MemoryStream())
////                {

////                    iTextSharp.text.Document pdfDoc = new iTextSharp.text.Document(PageSize, leftMargin, rightMargin, topMargin, bottomMargin);
////                    PdfWriter writer = PdfWriter.GetInstance(pdfDoc, ms);

////                    FooterPageEvent footerEvent = new FooterPageEvent
////                    {
////                        UserName = PrintUser,               // Replace with dynamic user information if needed
////                        PrintTime = DateTime.Now.ToString()       // Or a formatted version, e.g., DateTime.Now.ToString("g")
////                    };
////                    writer.PageEvent = footerEvent;
////                    pdfDoc.AddAuthor("YourAppName");
////                    pdfDoc.AddTitle("Credit Note Report");

////                    // Open the document to begin writing text
////                    pdfDoc.Open();
////                    // Initialize the footer event and assign it to the writer

////                    if (dt != null && dt.Count > 0)
////                    {

////                        for (global::System.Int32 i = 0; i < dt.Count; i++)
////                        {
////                            DataTable dtcurrent = dt[i];



////                            var groups = dtcurrent.AsEnumerable()
////    .GroupBy(row => row.Field<int>("rowindex"));

////                            foreach (var group in groups)
////                            {

////                                DataTable newTable = group.CopyToDataTable();






////                                if (Simulate.String(newTable.Rows[i]["Type"]) == "DatatableDetails")
////                                {
////                                    for (global::System.Int32 j = 0; j < dtReportData.Rows.Count; j++)
////                                    {
////                                        PdfPTable tablea = getTopHeader(newTable, dtReportData, j);
////                                        pdfDoc.Add(tablea);

////                                    }
////                                }
////                                else
////                                {

////                                    PdfPTable table = getTopHeader(newTable, dtHeaderData, 0);
////                                    pdfDoc.Add(table);

////                                }
////                            }
////                        }





////                    }








////                    pdfDoc.Close();
////                    byte[] pdfBytes = ms.ToArray();
////                    return pdfBytes;
////                }
////            }
////            catch (System.Exception ex)
////            {

////                throw;
////            }

////        }

////        PdfPTable getTopHeader(DataTable dtTopHeader, DataTable dtData = null, int dtrowindex = 0)
////        {
////            DataView dv = dtTopHeader.DefaultView;
////            dv.Sort = "ID ASC"; // or DESC
////            dtTopHeader = dv.ToTable();
////            float[] columnWidths = new float[dtTopHeader.Rows.Count];
////            PdfPTable headerTable = new PdfPTable(dtTopHeader.Rows.Count);
////            headerTable.DefaultCell.Border = Rectangle.NO_BORDER;
////            headerTable.DefaultCell.FixedHeight = 10;
////            headerTable.WidthPercentage = 100;
////            for (int i = 0; i < dtTopHeader.Rows.Count; i++)
////            {
////                columnWidths[i] = (float)Simulate.Val(dtTopHeader.Rows[i]["Width"]);
////                int horizontalAlignment = Element.ALIGN_MIDDLE;
////                if (Simulate.String(dtTopHeader.Rows[i]["HorizontalAlignment"]) != "")
////                {
////                    switch (Simulate.String(dtTopHeader.Rows[i]["HorizontalAlignment"]))
////                    {
////                        case "Right":
////                            horizontalAlignment = Element.ALIGN_LEFT;
////                            break;
////                        case "Left":
////                            horizontalAlignment = Element.ALIGN_RIGHT;
////                            break;
////                        case "Center":
////                            horizontalAlignment = Element.ALIGN_CENTER;
////                            break;
////                        default:
////                            break;

////                    }
////                }
////                int verticalAlignment = Element.ALIGN_MIDDLE;
////                if (Simulate.String(dtTopHeader.Rows[i]["VerticalAlignment"]) != "")
////                {
////                    switch (Simulate.String(dtTopHeader.Rows[i]["VerticalAlignment"]))
////                    {
////                        case "Top":
////                            verticalAlignment = Element.ALIGN_TOP;
////                            break;
////                        case "Bottom":
////                            verticalAlignment = Element.ALIGN_BOTTOM;
////                            break;
////                        case "Center":
////                            verticalAlignment = Element.ALIGN_MIDDLE;
////                            break;
////                        default:
////                            break;

////                    }
////                }

////                int fontWeight = Font.NORMAL;
////                if (Simulate.String(dtTopHeader.Rows[i]["FontWeight"]) != "")
////                {
////                    switch (Simulate.String(dtTopHeader.Rows[i]["FontWeight"]))
////                    {
////                        case "Bold":
////                            fontWeight = Font.BOLD;
////                            break;
////                        case "Italic":

////                            fontWeight = Font.ITALIC;
////                            break;
////                        case "Underline":
////                            fontWeight = Font.UNDERLINE;
////                            break;
////                        default:
////                            break;

////                    }
////                }

////                float fixedHeight = 50;
////                if (Simulate.Val(dtTopHeader.Rows[i]["Height"]) > 0)
////                {
////                    fixedHeight = (float)Simulate.Val(dtTopHeader.Rows[i]["Height"]);
////                }

////                string font = FontFactory.HELVETICA_BOLD;
////                if (Simulate.String(dtTopHeader.Rows[i]["Font"]) != "")
////                {
////                    font = Simulate.String(dtTopHeader.Rows[i]["Font"]);//"Arial"

////                }
////                int fontsize = 12;
////                if (Simulate.Integer32(dtTopHeader.Rows[i]["FontSize"]) > 0)
////                {
////                    fontsize = Simulate.Integer32(dtTopHeader.Rows[i]["FontSize"]);
////                }
////                BaseColor baseColor = BaseColor.BLACK;
////                if (Simulate.String(dtTopHeader.Rows[i]["FontColor"]) != "")
////                {

////                    long colorValue = long.Parse(Simulate.String(dtTopHeader.Rows[i]["FontColor"]));
////                    int r = (int)((colorValue >> 16) & 0xFF);
////                    int g = (int)((colorValue >> 8) & 0xFF);
////                    int b = (int)(colorValue & 0xFF);
////                    baseColor = new BaseColor(r, g, b);
////                }

////                BaseColor backgroundColor = BaseColor.WHITE;
////                if (Simulate.String(dtTopHeader.Rows[i]["BackColor"]) != "")
////                {
////                    long colorValue = long.Parse(Simulate.String(dtTopHeader.Rows[i]["BackColor"]));
////                    int r = (int)((colorValue >> 16) & 0xFF);
////                    int g = (int)((colorValue >> 8) & 0xFF);
////                    int b = (int)(colorValue & 0xFF);
////                    backgroundColor = new BaseColor(r, g, b);
////                }

////                int boarder = PdfPCell.NO_BORDER;
////                if (Simulate.Bool(dtTopHeader.Rows[i]["WithBoarder"]))
////                {
////                    boarder = PdfPCell.TOP_BORDER
////            | PdfPCell.BOTTOM_BORDER
////            | PdfPCell.LEFT_BORDER
////            | PdfPCell.RIGHT_BORDER;
////                }
////                if (Simulate.String(dtTopHeader.Rows[i]["type"]) == "@@CompanyName")
////                {
////                    var a = 5;
////                    a = 6;
////                }
////                BaseFont bf = BaseFont.CreateFont("C:/windows/fonts/arialuni.ttf", BaseFont.IDENTITY_H, BaseFont.EMBEDDED);
////                Font arabicFont = new Font(bf, fontsize, fontWeight, baseColor);
////                switch (Simulate.String(dtTopHeader.Rows[i]["Type"]))
////                {
////                    case "@@EmptyValue":
////                        {
////                            PdfPCell emptyCell = new PdfPCell(new Phrase(""));
////                            emptyCell.Border = boarder;
////                            emptyCell.FixedHeight = fixedHeight;

////                            emptyCell.VerticalAlignment = verticalAlignment;
////                            emptyCell.HorizontalAlignment = horizontalAlignment;
////                            emptyCell.BackgroundColor = backgroundColor;
////                            headerTable.AddCell(emptyCell);
////                            break;
////                        }
////                    case "@@CompanyLogo":
////                        {
////                            byte[] logoBytes = (byte[])dtCompany.Rows[0]["Logo"];
////                            iTextSharp.text.Image logo = iTextSharp.text.Image.GetInstance(logoBytes);

////                            // Scale image to fit within 50f height:
////                            // For example, set a max width/height
////                            //logo.ScaleToFit(50f, 50f);
////                            logo.ScaleToFit(10, fixedHeight);

////                            PdfPCell logoCell = new PdfPCell(logo, fit: true);
////                            logoCell.Border = boarder;
////                            logoCell.FixedHeight = fixedHeight; // match table's default
////                            logoCell.VerticalAlignment = verticalAlignment;
////                            logoCell.HorizontalAlignment = horizontalAlignment;
////                            logoCell.BackgroundColor = backgroundColor;
////                            headerTable.AddCell(logoCell);

////                            break;
////                        }
////                    case "@@Address":
////                        {

////                            string companyName = dtCompany.Rows[0]["Address"].ToString();

////                            Phrase phrase1 = new Phrase(companyName, arabicFont);



////                            PdfPCell titleCell = new PdfPCell(phrase1);
////                            titleCell.RunDirection = PdfWriter.RUN_DIRECTION_RTL;
////                            titleCell.Border = boarder;
////                            titleCell.FixedHeight = fixedHeight;
////                            titleCell.VerticalAlignment = verticalAlignment;
////                            titleCell.HorizontalAlignment = horizontalAlignment;
////                            titleCell.BackgroundColor = backgroundColor;
////                            headerTable.AddCell(titleCell);
////                            break;


////                        }
////                    case "@@CompanyName":
////                        {
////                            string companyName = dtCompany.Rows[0]["AName"].ToString();

////                            Phrase phrase1 = new Phrase(companyName, arabicFont);



////                            PdfPCell titleCell = new PdfPCell(phrase1);
////                            titleCell.RunDirection = PdfWriter.RUN_DIRECTION_RTL;
////                            titleCell.Border = boarder;
////                            titleCell.FixedHeight = fixedHeight;
////                            titleCell.VerticalAlignment = verticalAlignment;
////                            titleCell.HorizontalAlignment = horizontalAlignment;
////                            titleCell.BackgroundColor = backgroundColor;
////                            headerTable.AddCell(titleCell);
////                            break;

////                        }
////                    default:
////                        string text = Simulate.String(dtTopHeader.Rows[i]["OtherValue"]);
////                        if (Simulate.String(dtTopHeader.Rows[i]["Type"]) == "Datatable")
////                        {
////                            text = Simulate.String(dtTopHeader.Rows[i]["OtherValue"]);

////                        }
////                        else if (Simulate.String(dtTopHeader.Rows[i]["Type"]) == "DatatableDetails")
////                        {
////                            text = Simulate.String(dtTopHeader.Rows[i]["OtherValue"]);
////                            text = Simulate.String(dtData.Rows[dtrowindex][text]);

////                        }
////                        else if (Simulate.String(dtTopHeader.Rows[i]["Type"]) == "HeaderData")
////                        {
////                            text = Simulate.String(dtTopHeader.Rows[i]["OtherValue"]);
////                            text = Simulate.String(dtData.Rows[dtrowindex][text.Replace("##", "")]);

////                        }

////                        Phrase phrase = new Phrase(text, arabicFont);
////                        PdfPCell textCell = new PdfPCell(phrase);
////                        textCell.RunDirection = PdfWriter.RUN_DIRECTION_RTL;



////                        textCell.BackgroundColor = backgroundColor;
////                        textCell.Border = boarder;

////                        textCell.FixedHeight = fixedHeight;
////                        textCell.VerticalAlignment = verticalAlignment;
////                        textCell.HorizontalAlignment = horizontalAlignment;

////                        headerTable.AddCell(textCell);
////                        break;
////                }
////            }


////            headerTable.SetWidths(columnWidths);



////            return headerTable;
////        }

////        private BaseColor ParseColor(string hexString)
////        {
////            // Remove leading '#' if present
////            hexString = hexString.Replace("#", "");

////            // Expect exactly 6 characters for RRGGBB
////            if (hexString.Length != 6)
////            {
////                // Fallback or throw an exception for an invalid format
////                return BaseColor.BLACK;
////            }

////            // Parse red, green, blue from the substring
////            byte r = Convert.ToByte(hexString.Substring(0, 2), 16);
////            byte g = Convert.ToByte(hexString.Substring(2, 2), 16);
////            byte b = Convert.ToByte(hexString.Substring(4, 2), 16);

////            // Return a new BaseColor
////            return new BaseColor(r, g, b);
////        }
////    }
////}

////public class FooterPageEvent : PdfPageEventHelper
////{
////    // Properties to hold user and print time information
////    public string UserName { get; set; }
////    public string PrintTime { get; set; }

////    public override void OnEndPage(PdfWriter writer, Document document)
////    {
////        PdfContentByte cb = writer.DirectContent;
////        BaseFont bf = BaseFont.CreateFont(BaseFont.HELVETICA, BaseFont.CP1252, BaseFont.NOT_EMBEDDED);
////        cb.BeginText();

////        // Increase font size (e.g., 10 instead of 8) and adjust Y position (e.g., move 10 points up)
////        float fontSize = 10;
////        cb.SetFontAndSize(bf, fontSize);
////        float y = document.BottomMargin / 2;  // Adjust as needed to move it further up

////        // Left: Print Time
////        float leftX = document.LeftMargin;
////        cb.ShowTextAligned(PdfContentByte.ALIGN_LEFT, "Print Time: " + PrintTime, leftX, y, 0);

////        // Center: Page Number
////        float centerX = document.PageSize.Width / 2;
////        cb.ShowTextAligned(PdfContentByte.ALIGN_CENTER, "Page " + writer.PageNumber, centerX, y, 0);

////        // Right: User
////        float rightX = document.PageSize.Width - document.RightMargin;
////        cb.ShowTextAligned(PdfContentByte.ALIGN_RIGHT, "User: " + UserName, rightX, y, 0);

////        cb.EndText();
////    }
////}