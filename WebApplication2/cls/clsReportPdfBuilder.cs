////using System;
////using System.Collections.Generic;
////using System.IO;
////using System.Linq;
////using System.Text.Json;
////using System.Text.Json.Serialization;
////using iText.Commons.Bouncycastle;
////using iText.Bouncycastleconnector;
////using iText.IO.Image;
////using iText.Kernel.Colors;
////using iText.Kernel.Geom;
////using iText.Kernel.Pdf;
////using iText.Layout;
////using iText.Layout.Element;
////using iText.Layout.Properties;
////using iText.Kernel.Font;
////using iText.IO.Font;
////using iText.Layout.Properties;

////namespace WebApplication2.cls
////{

////        public class clsReportPdfBuilder
////        {
////            // Flutter uses px @ 96dpi. PDF uses points @ 72dpi.
////            private const float PxToPt = 72f / 96f; // 0.75

////        //  public static byte[] Build(string templateJson, PrintData data)
////        //  {
////        //  var _ = BouncyCastleFactoryCreator.GetFactory();
////        //// BouncyCastleFactoryCreator.SetFactory(new BouncyCastleFactory());
////        //  if (string.IsNullOrWhiteSpace(templateJson))
////        //          throw new ArgumentException("templateJson is empty");

////        //      var tpl = JsonSerializer.Deserialize<TemplateModel>(
////        //          templateJson,
////        //          new JsonSerializerOptions
////        //          {
////        //              PropertyNameCaseInsensitive = true
////        //          });

////        //      if (tpl == null) throw new Exception("Template deserialize failed.");

////        //      using var ms = new MemoryStream();

////        //      using var writer = new PdfWriter(ms);
////        //      using var pdf = new PdfDocument(writer);

////        //      // Page size (simple)
////        //      var pageSize = ResolvePageSize(tpl.Page);
////        //      pdf.SetDefaultPageSize(pageSize);

////        //      using var doc = new Document(pdf, pageSize);
////        //      doc.SetMargins(20, 20, 20, 20);

////        //      // Split by band
////        //      var elements = tpl.Elements ?? new List<ElementDef>();
////        //      var pageHeader = elements.Where(e => e.Band == "pageHeader").ToList();
////        //      var detail = elements.Where(e => e.Band == "detail").ToList();
////        //      var pageFooter = elements.Where(e => e.Band == "pageFooter").ToList();
////        //      var reportFooter = elements.Where(e => e.Band == "reportFooter").ToList();

////        //      // NOTE: For now we render header once at top, footer at end.
////        //      // Later we can add proper per-page event handler.
////        //      RenderElements(doc, pageHeader, data, pageSize);
////        //      RenderElements(doc, detail, data, pageSize);
////        //      RenderElements(doc, reportFooter, data, pageSize);
////        //      RenderElements(doc, pageFooter, data, pageSize);

////        //      doc.Close();
////        //      return ms.ToArray();
////        //  }
////        public static byte[] Build(string templateJson, PrintData data)
////        {
////            // Will throw only if adapter not installed correctly
////            var bc = BouncyCastleFactoryCreator.GetFactory();

////            if (string.IsNullOrWhiteSpace(templateJson))
////                throw new ArgumentException("templateJson is empty");

////            var tpl = JsonSerializer.Deserialize<TemplateModel>(
////                templateJson,
////                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

////            if (tpl == null) throw new Exception("Template deserialize failed.");

////            using var ms = new MemoryStream();
////            using var writer = new PdfWriter(ms);
////            using var pdf = new PdfDocument(writer);

////            var pageSize = ResolvePageSize(tpl.Page);
////            pdf.SetDefaultPageSize(pageSize);

////            using var doc = new Document(pdf, pageSize);
////            doc.SetMargins(20, 20, 20, 20);


////            var fontPath = System.IO.Path.Combine(
////    AppContext.BaseDirectory,
////    "Fonts",
////    "Noto_Naskh_Arabic",
////    "static",
////    "NotoNaskhArabic-Regular.ttf"
////);
////        //    var unicodeFont = PdfFontFactory.CreateFont(fontPath, PdfEncodings.IDENTITY_H);
////         //   doc.SetFont(unicodeFont);


////            var elements = tpl.Elements ?? new List<ElementDef>();
////            RenderElements(doc, elements.Where(e => e.Band == "pageHeader").ToList(), data, pageSize);
////            RenderElements(doc, elements.Where(e => e.Band == "detail").ToList(), data, pageSize);
////            RenderElements(doc, elements.Where(e => e.Band == "reportFooter").ToList(), data, pageSize);
////            RenderElements(doc, elements.Where(e => e.Band == "pageFooter").ToList(), data, pageSize);

////            doc.Close();
////            return ms.ToArray();
////        }
////        private static void RenderElements(Document doc, List<ElementDef> elements, PrintData data, PageSize pageSize)
////            {
////                foreach (var el in elements.OrderBy(x => x.Y))
////                {
////                    var type = (el.Type ?? "").Trim();

////                    switch (type)
////                    {
////                        case "text":
////                            DrawText(doc, el, el.Text ?? "", pageSize);
////                            break;

////                        case "field":
////                            DrawText(doc, el, ResolveField(data, el.FieldName), pageSize);
////                            break;

////                        case "image":
////                            DrawImage(doc, el, ResolveImage(data, el.Source), pageSize);
////                            break;

////                        case "line":
////                            DrawLine(doc, el, pageSize);
////                            break;

////                        case "table":
////                            DrawTable(doc, el, data, pageSize);
////                            break;
////                    }
////                }
////            }

////            private static string ResolveField(PrintData data, string key)
////            {
////                if (string.IsNullOrWhiteSpace(key)) return "";

////                if (data.Header != null && data.Header.TryGetValue(key, out var v1)) return v1?.ToString() ?? "";
////                if (data.Footer != null && data.Footer.TryGetValue(key, out var v2)) return v2?.ToString() ?? "";
////                if (data.Company != null && data.Company.TryGetValue(key, out var v3)) return v3?.ToString() ?? "";

////                return "";
////            }

////            private static string ResolveImage(PrintData data, string source)
////            {
////                if (string.IsNullOrWhiteSpace(source)) return "";

////                // Example mapping:
////                if (source == "company_logo" && data.Company != null && data.Company.TryGetValue("LogoBase64", out var v))
////                    return v?.ToString() ?? "";

////                // Add more sources as needed (signature, stamp, etc.)
////                return "";
////            }

////            private static void DrawText(Document doc, ElementDef el, string text, PageSize pageSize)
////            {
////                // Convert Flutter coords (top-left) to PDF coords (bottom-left)
////                var xPt = (float)(el.X * PxToPt);
////                var yPt = ToPdfY(pageSize, el.Y, el.Height);

////                var p = new Paragraph(text ?? "")
////                    .SetFontSize(el.FontSize > 0 ? el.FontSize : 12)
////                  //  .SetBold(el.Bold)
////                    .SetFontColor(ParseColor(el.ColorHex));

////                var align = (el.TextAlign ?? "left").ToLowerInvariant();
////                p.SetTextAlignment(
////                    align == "center" ? TextAlignment.CENTER :
////                    align == "right" ? TextAlignment.RIGHT :
////                    TextAlignment.LEFT
////                );

////                // Absolute position box
////                var wPt = (float)(el.Width * PxToPt);
////                var hPt = (float)(el.Height * PxToPt);

////            // Use a fixed-position Div to respect width
////            var div = new Div()
////                .SetFixedPosition(xPt, yPt, wPt)
////                .SetHeight(hPt);

////            div.Add(p);
////            doc.Add(div);


////    //        var div = new Div()
////    //.SetFixedPosition(xPt, yPt, wPt);

////    //        // IMPORTANT: don't force height (prevents clipping)
////    //        div.Add(p.SetMargin(0).SetMultipliedLeading(1.0f));
////    //        doc.Add(div);
////        }

////            private static void DrawImage(Document doc, ElementDef el, string base64, PageSize pageSize)
////            {
////                if (string.IsNullOrWhiteSpace(base64)) return;

////                // support data:image/png;base64,....
////                var b64 = base64;
////                var comma = b64.IndexOf(',');
////                if (comma > 0 && b64.Substring(0, comma).Contains("base64"))
////                    //知道
////                                    b64 = b64.Substring(comma + 1);

////                byte[] bytes;
////                try { bytes = Convert.FromBase64String(b64); }
////                catch { return; }

////                var xPt = (float)(el.X * PxToPt);
////                var yPt = ToPdfY(pageSize, el.Y, el.Height);

////                var wPt = (float)(el.Width * PxToPt);
////                var hPt = (float)(el.Height * PxToPt);

////                var img = new Image(ImageDataFactory.Create(bytes))
////                    .SetFixedPosition(xPt, yPt)
////                    .ScaleToFit(wPt, hPt);

////                doc.Add(img);
////            }

////            private static void DrawLine(Document doc, ElementDef el, PageSize pageSize)
////            {
////                // Simple line as a thin rectangle
////                var xPt = (float)(el.X * PxToPt);
////                var yPt = ToPdfY(pageSize, el.Y, el.Height);

////                var wPt = (float)(el.Width * PxToPt);
////                var hPt = Math.Max(1f, (float)(el.Thickness * PxToPt));

////                var div = new Div()
////                    .SetFixedPosition(xPt, yPt, wPt)
////                    .SetHeight(hPt)
////                    .SetBackgroundColor(ParseColor(el.ColorHex));

////                doc.Add(div);
////            }

////            private static void DrawTable(Document doc, ElementDef el, PrintData data, PageSize pageSize)
////            {
////                if (el.Table == null || el.Table.Columns == null || el.Table.Columns.Count == 0)
////                    return;

////                var cols = el.Table.Columns;

////                var xPt = (float)(el.X * PxToPt);
////                var yPt = ToPdfY(pageSize, el.Y, el.Height);
////                var wPt = (float)(el.Width * PxToPt);

////                // Build table with relative widths from px widths
////                var widths = cols.Select(c => Math.Max(10f, (float)(c.Width * PxToPt))).ToArray();
////                var table = new Table(UnitValue.CreatePointArray(widths))
////                    .SetFixedPosition(xPt, yPt, wPt);

////                // Header
////                if (el.Table.ShowHeader)
////                {
////                    foreach (var c in cols)
////                    {
////                        table.AddHeaderCell(new Cell().Add(new Paragraph(c.Title ?? ""))
////                          //  .SetBold()
////                            .SetFontSize(el.Table.HeaderStyle?.FontSize ?? 11));
////                    }
////                }

////                // Rows
////                var lines = data.Lines ?? new List<Dictionary<string, object>>();
////                foreach (var row in lines)
////                {
////                    foreach (var c in cols)
////                    {
////                        row.TryGetValue(c.Field ?? "", out var v);
////                        table.AddCell(new Cell().Add(new Paragraph(v?.ToString() ?? ""))
////                            .SetFontSize(el.Table.RowStyle?.FontSize ?? 10));
////                    }
////                }

////                doc.Add(table);
////            }

////            private static float ToPdfY(PageSize pageSize, double flutterY, double flutterH)
////            {
////                // Flutter Y is from top. PDF uses bottom.
////                var yTopPt = (float)(flutterY * PxToPt);
////                var hPt = (float)(flutterH * PxToPt);
////                return pageSize.GetHeight() - yTopPt - hPt;
////            }

////            private static DeviceRgb ParseColor(string hex)
////            {
////                // expects #RRGGBB
////                if (string.IsNullOrWhiteSpace(hex)) return new DeviceRgb(0, 0, 0);
////                var h = hex.Trim();
////                if (h.StartsWith("#")) h = h.Substring(1);
////                if (h.Length != 6) return new DeviceRgb(0, 0, 0);

////                try
////                {
////                    var r = Convert.ToByte(h.Substring(0, 2), 16);
////                    var g = Convert.ToByte(h.Substring(2, 2), 16);
////                    var b = Convert.ToByte(h.Substring(4, 2), 16);
////                    return new DeviceRgb(r, g, b);
////                }
////                catch
////                {
////                    return new DeviceRgb(0, 0, 0);
////                }
////            }

////            private static PageSize ResolvePageSize(PageDef page)
////            {
////                // simple default (A4)
////                // you can later map your preset + orientation from Flutter
////                return PageSize.A4;
////            }
////        }

////        // =========================
////        // DTOs (match your Flutter JSON)
////        // =========================

////        public class TemplateModel
////        {
////            [JsonPropertyName("page")]
////            public PageDef Page { get; set; }

////            [JsonPropertyName("elements")]
////            public List<ElementDef> Elements { get; set; }
////        }

////        public class PageDef
////        {
////            [JsonPropertyName("preset")]
////            public string Preset { get; set; }

////            [JsonPropertyName("orientation")]
////            public string Orientation { get; set; }

////            [JsonPropertyName("dpi")]
////            public double Dpi { get; set; }
////        }

////        public class ElementDef
////        {
////            [JsonPropertyName("id")]
////            public string Id { get; set; }

////            [JsonPropertyName("type")]
////            public string Type { get; set; }          // text / field / image / line / table

////            [JsonPropertyName("band")]
////            public string Band { get; set; }          // pageHeader / detail / pageFooter / reportFooter

////            [JsonPropertyName("x")]
////            public double X { get; set; }

////            [JsonPropertyName("y")]
////            public double Y { get; set; }

////            [JsonPropertyName("width")]
////            public double Width { get; set; }

////            [JsonPropertyName("height")]
////            public double Height { get; set; }

////            [JsonPropertyName("fontSize")]
////            public int FontSize { get; set; }

////            [JsonPropertyName("bold")]
////            public bool Bold { get; set; }

////            [JsonPropertyName("colorHex")]
////            public string ColorHex { get; set; }

////            [JsonPropertyName("textAlign")]
////            public string TextAlign { get; set; }    // left/center/right

////            [JsonPropertyName("text")]
////            public string Text { get; set; }

////            [JsonPropertyName("fieldName")]
////            public string FieldName { get; set; }

////            [JsonPropertyName("source")]
////            public string Source { get; set; }

////            [JsonPropertyName("thickness")]
////            public int Thickness { get; set; }

////            [JsonPropertyName("table")]
////            public TableDef Table { get; set; }
////        }

////        public class TableDef
////        {
////            [JsonPropertyName("showHeader")]
////            public bool ShowHeader { get; set; } = true;

////            [JsonPropertyName("headerStyle")]
////            public TableHeaderStyleDef HeaderStyle { get; set; }

////            [JsonPropertyName("rowStyle")]
////            public TableRowStyleDef RowStyle { get; set; }

////            [JsonPropertyName("columns")]
////            public List<TableColumnDef> Columns { get; set; } = new List<TableColumnDef>();
////        }

////        public class TableHeaderStyleDef
////        {
////            [JsonPropertyName("fontSize")]
////            public int FontSize { get; set; } = 11;
////        }

////        public class TableRowStyleDef
////        {
////            [JsonPropertyName("fontSize")]
////            public int FontSize { get; set; } = 10;
////        }

////        public class TableColumnDef
////        {
////            [JsonPropertyName("title")]
////            public string Title { get; set; }

////            [JsonPropertyName("field")]
////            public string Field { get; set; }

////            [JsonPropertyName("width")]
////            public double Width { get; set; } = 120;
////        }

////        public class PrintData
////        {
////            public Dictionary<string, object> Company { get; set; } = new Dictionary<string, object>();
////            public Dictionary<string, object> Header { get; set; } = new Dictionary<string, object>();
////            public List<Dictionary<string, object>> Lines { get; set; } = new List<Dictionary<string, object>>();
////            public Dictionary<string, object> Footer { get; set; } = new Dictionary<string, object>();
////        }
////    }
//using System;
//using System.Collections.Generic;
//using System.Globalization;
//using System.IO;
//using System.Linq;
//using System.Text;
//using System.Text.Json;
//using System.Text.Json.Serialization;
//using Microsoft.Playwright;

//namespace WebApplication2.cls
//{
//    public class clsReportPdfBuilder
//    {
//        // Flutter px @ 96dpi. We'll keep px in HTML; Chromium handles print scaling well.
//        // If your template assumes A4 at 96dpi, it will be close enough. You can tune later.

//        public static byte[] Build(string templateJson, PrintData data)
//            => BuildAsync(templateJson, data).GetAwaiter().GetResult();

//        private static async System.Threading.Tasks.Task<byte[]> BuildAsync(string templateJson, PrintData data)
//        {
//            if (string.IsNullOrWhiteSpace(templateJson))
//                throw new ArgumentException("templateJson is empty");

//            var tpl = JsonSerializer.Deserialize<TemplateModel>(
//                templateJson,
//                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

//            if (tpl == null) throw new Exception("Template deserialize failed.");

//            // 1) Build HTML from your template + data
//            string html = BuildHtml(tpl, data);

//            // 2) Render HTML -> PDF using Chromium (Playwright)
//            using var playwright = await Playwright.CreateAsync();
//            await using var browser = await playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions
//            {
//                Headless = true
//            });

//            var page = await browser.NewPageAsync(new BrowserNewPageOptions
//            {
//                ViewportSize = new ViewportSize { Width = 1280, Height = 720 }
//            });

//            await page.SetContentAsync(html, new PageSetContentOptions
//            {
//                WaitUntil = WaitUntilState.NetworkIdle
//            });

//            // Page size: default A4; you can map preset/orientation later
//            var pdfBytes = await page.PdfAsync(new PagePdfOptions
//            {
//                Format = "A4",
//                PrintBackground = true,
//                PreferCSSPageSize = true
//            });

//            return pdfBytes;
//        }

//        private static string BuildHtml(TemplateModel tpl, PrintData data)
//        {
//            // Load font (local file) — same font you already have
//            // We'll embed via @font-face so Arabic shapes correctly.
//            var fontPath = Path.Combine(AppContext.BaseDirectory, "Fonts", "Noto_Naskh_Arabic", "static", "NotoNaskhArabic-Regular.ttf");

//            // Convert to file:/// URL for Chromium
//            var fontUrl = new Uri(fontPath).AbsoluteUri;

//            // Collect elements by band (same logic you used)
//            var elements = tpl.Elements ?? new List<ElementDef>();

//            // Your template uses (x,y) from top-left in px => perfect for absolute HTML
//            // We'll create a single-page canvas.
//            var sb = new StringBuilder();

//            sb.AppendLine("<!doctype html>");
//            sb.AppendLine("<html>");
//            sb.AppendLine("<head>");
//            sb.AppendLine("<meta charset='utf-8'/>");

//            sb.AppendLine("<style>");
//            sb.AppendLine("@page { size: A4; margin: 20px; }");

//            sb.AppendLine($"@font-face {{ font-family: 'NotoNaskh'; src: url('{fontUrl}') format('truetype'); }}");

//            sb.AppendLine("html, body { margin:0; padding:0; }");
//            sb.AppendLine("body { font-family: NotoNaskh, Arial, sans-serif; }");

//            // Canvas container: keep relative, everything absolute inside
//            sb.AppendLine(".canvas { position: relative; width: 794px; height: 1123px; } /* A4 @ ~96dpi */");

//            sb.AppendLine(".el { position:absolute; box-sizing:border-box; }");
//            sb.AppendLine(".line { background:#000; }");

//            sb.AppendLine("table { border-collapse: collapse; width: 100%; }");
//            sb.AppendLine("th, td { border: 1px solid #333; padding: 6px; font-size: 12px; }");
//            sb.AppendLine("</style>");
//            sb.AppendLine("</head>");

//            // If you want RTL globally, use dir="rtl".
//            // If template is mixed, we set per element based on textAlign.
//            sb.AppendLine("<body>");
//            sb.AppendLine("<div class='canvas'>");

//            foreach (var el in elements.OrderBy(e => e.Y))
//            {
//                var type = (el.Type ?? "").Trim().ToLowerInvariant();
//                switch (type)
//                {
//                    case "text":
//                        sb.AppendLine(RenderText(el, el.Text ?? "", data, isField: false));
//                        break;

//                    case "field":
//                        sb.AppendLine(RenderText(el, ResolveField(data, el.FieldName), data, isField: true));
//                        break;

//                    case "image":
//                        sb.AppendLine(RenderImage(el, ResolveImage(data, el.Source)));
//                        break;

//                    case "line":
//                        sb.AppendLine(RenderLine(el));
//                        break;

//                    case "table":
//                        sb.AppendLine(RenderTable(el, data));
//                        break;
//                }
//            }

//            sb.AppendLine("</div>");
//            sb.AppendLine("</body>");
//            sb.AppendLine("</html>");

//            return sb.ToString();
//        }

//        private static string RenderText(ElementDef el, string text, PrintData data, bool isField)
//        {
//            var x = ToCss(el.X);
//            var y = ToCss(el.Y);
//            var w = ToCss(el.Width);
//            var h = ToCss(el.Height);

//            var fontSize = el.FontSize > 0 ? el.FontSize : 12;
//            var color = CssColor(el.ColorHex);

//            var align = (el.TextAlign ?? "left").ToLowerInvariant();
//            var cssAlign = align == "center" ? "center" : align == "right" ? "right" : "left";

//            // Arabic shaping works automatically in Chromium, but direction matters.
//            // If align=right we assume RTL, else LTR. You can refine later per field.
//            var dir = (cssAlign == "right") ? "rtl" : "ltr";

//            // IMPORTANT: don’t force height clipping. Use min-height instead.
//            // If your element must clip, you can add overflow:hidden.
//            var safe = HtmlEscape(text ?? "");

//            return $@"
//<div class='el'
//     style='left:{x}; top:{y}; width:{w}; min-height:{h};
//            color:{color}; font-size:{fontSize}px;
//            text-align:{cssAlign}; direction:{dir};
//            white-space:pre-wrap;'>
//  {safe}
//</div>";
//        }

//        private static string RenderImage(ElementDef el, string base64)
//        {
//            if (string.IsNullOrWhiteSpace(base64)) return "";

//            var b64 = base64;
//            var comma = b64.IndexOf(',');
//            if (comma > 0 && b64.Substring(0, comma).Contains("base64", StringComparison.OrdinalIgnoreCase))
//                b64 = b64[(comma + 1)..];

//            // Use data URI
//            var x = ToCss(el.X);
//            var y = ToCss(el.Y);
//            var w = ToCss(el.Width);
//            var h = ToCss(el.Height);

//            return $@"
//<img class='el'
//     style='left:{x}; top:{y}; width:{w}; height:{h}; object-fit:contain;'
//     src='data:image/png;base64,{b64}' />";
//        }

//        private static string RenderLine(ElementDef el)
//        {
//            var x = ToCss(el.X);
//            var y = ToCss(el.Y);
//            var w = ToCss(el.Width);
//            var h = ToCss(Math.Max(1, el.Thickness));

//            var color = CssColor(el.ColorHex);
//            return $@"<div class='el line' style='left:{x}; top:{y}; width:{w}; height:{h}; background:{color};'></div>";
//        }

//        private static string RenderTable(ElementDef el, PrintData data)
//        {
//            if (el.Table?.Columns == null || el.Table.Columns.Count == 0) return "";

//            var x = ToCss(el.X);
//            var y = ToCss(el.Y);
//            var w = ToCss(el.Width);
//            // Height optional — let it grow (no clipping)
//            var fontSize = el.Table.RowStyle?.FontSize ?? 12;

//            var cols = el.Table.Columns;

//            // Build colgroup widths from template
//            var colGroup = new StringBuilder();
//            colGroup.Append("<colgroup>");
//            foreach (var c in cols)
//            {
//                var cw = Math.Max(20, c.Width);
//                colGroup.Append($"<col style='width:{cw}px'>");
//            }
//            colGroup.Append("</colgroup>");

//            var sb = new StringBuilder();
//            sb.AppendLine($@"<div class='el' style='left:{x}; top:{y}; width:{w};'>");
//            sb.AppendLine($@"<table style='font-size:{fontSize}px;'>");
//            sb.AppendLine(colGroup.ToString());

//            if (el.Table.ShowHeader)
//            {
//                sb.AppendLine("<thead><tr>");
//                foreach (var c in cols)
//                    sb.AppendLine($"<th>{HtmlEscape(c.Title ?? "")}</th>");
//                sb.AppendLine("</tr></thead>");
//            }

//            sb.AppendLine("<tbody>");
//            var lines = data.Lines ?? new List<Dictionary<string, object>>();

//            foreach (var row in lines)
//            {
//                sb.AppendLine("<tr>");
//                foreach (var c in cols)
//                {
//                    row.TryGetValue(c.Field ?? "", out var v);
//                    sb.AppendLine($"<td>{HtmlEscape(v?.ToString() ?? "")}</td>");
//                }
//                sb.AppendLine("</tr>");
//            }

//            sb.AppendLine("</tbody>");
//            sb.AppendLine("</table>");
//            sb.AppendLine("</div>");

//            return sb.ToString();
//        }

//        private static string ResolveField(PrintData data, string key)
//        {
//            if (string.IsNullOrWhiteSpace(key)) return "";

//            if (data.Header != null && data.Header.TryGetValue(key, out var v1)) return v1?.ToString() ?? "";
//            if (data.Footer != null && data.Footer.TryGetValue(key, out var v2)) return v2?.ToString() ?? "";
//            if (data.Company != null && data.Company.TryGetValue(key, out var v3)) return v3?.ToString() ?? "";

//            return "";
//        }

//        private static string ResolveImage(PrintData data, string source)
//        {
//            if (string.IsNullOrWhiteSpace(source)) return "";
//            if (source == "company_logo" && data.Company != null && data.Company.TryGetValue("LogoBase64", out var v))
//                return v?.ToString() ?? "";
//            return "";
//        }

//        private static string ToCss(double v) => $"{v.ToString("0.###", CultureInfo.InvariantCulture)}px";

//        private static string CssColor(string hex)
//        {
//            if (string.IsNullOrWhiteSpace(hex)) return "#000000";
//            var h = hex.Trim();
//            if (!h.StartsWith("#")) h = "#" + h;
//            return h.Length == 7 ? h : "#000000";
//        }

//        private static string HtmlEscape(string s)
//        {
//            if (string.IsNullOrEmpty(s)) return "";
//            return s.Replace("&", "&amp;")
//                    .Replace("<", "&lt;")
//                    .Replace(">", "&gt;")
//                    .Replace("\"", "&quot;")
//                    .Replace("'", "&#39;");
//        }

//        // =========================
//        // DTOs (same as yours)
//        // =========================

//        public class TemplateModel
//        {
//            [JsonPropertyName("page")]
//            public PageDef Page { get; set; }

//            [JsonPropertyName("elements")]
//            public List<ElementDef> Elements { get; set; }
//        }

//        public class PageDef
//        {
//            [JsonPropertyName("preset")]
//            public string Preset { get; set; }

//            [JsonPropertyName("orientation")]
//            public string Orientation { get; set; }

//            [JsonPropertyName("dpi")]
//            public double Dpi { get; set; }
//        }

//        public class ElementDef
//        {
//            [JsonPropertyName("id")]
//            public string Id { get; set; }

//            [JsonPropertyName("type")]
//            public string Type { get; set; }

//            [JsonPropertyName("band")]
//            public string Band { get; set; }

//            [JsonPropertyName("x")]
//            public double X { get; set; }

//            [JsonPropertyName("y")]
//            public double Y { get; set; }

//            [JsonPropertyName("width")]
//            public double Width { get; set; }

//            [JsonPropertyName("height")]
//            public double Height { get; set; }

//            [JsonPropertyName("fontSize")]
//            public int FontSize { get; set; }

//            [JsonPropertyName("bold")]
//            public bool Bold { get; set; }

//            [JsonPropertyName("colorHex")]
//            public string ColorHex { get; set; }

//            [JsonPropertyName("textAlign")]
//            public string TextAlign { get; set; }

//            [JsonPropertyName("text")]
//            public string Text { get; set; }

//            [JsonPropertyName("fieldName")]
//            public string FieldName { get; set; }

//            [JsonPropertyName("source")]
//            public string Source { get; set; }

//            [JsonPropertyName("thickness")]
//            public int Thickness { get; set; }

//            [JsonPropertyName("table")]
//            public TableDef Table { get; set; }
//        }

//        public class TableDef
//        {
//            [JsonPropertyName("showHeader")]
//            public bool ShowHeader { get; set; } = true;

//            [JsonPropertyName("headerStyle")]
//            public TableHeaderStyleDef HeaderStyle { get; set; }

//            [JsonPropertyName("rowStyle")]
//            public TableRowStyleDef RowStyle { get; set; }

//            [JsonPropertyName("columns")]
//            public List<TableColumnDef> Columns { get; set; } = new();
//        }

//        public class TableHeaderStyleDef
//        {
//            [JsonPropertyName("fontSize")]
//            public int FontSize { get; set; } = 11;
//        }

//        public class TableRowStyleDef
//        {
//            [JsonPropertyName("fontSize")]
//            public int FontSize { get; set; } = 10;
//        }

//        public class TableColumnDef
//        {
//            [JsonPropertyName("title")]
//            public string Title { get; set; }

//            [JsonPropertyName("field")]
//            public string Field { get; set; }

//            [JsonPropertyName("width")]
//            public double Width { get; set; } = 120;
//        }

//        public class PrintData
//        {
//            public Dictionary<string, object> Company { get; set; } = new();
//            public Dictionary<string, object> Header { get; set; } = new();
//            public List<Dictionary<string, object>> Lines { get; set; } = new();
//            public Dictionary<string, object> Footer { get; set; } = new();
//        }
//    }
////}
//using System;
//using System.Collections.Generic;
//using System.Globalization;
//using System.IO;
//using System.Linq;
//using System.Text;
//using System.Text.Json;
//using System.Text.Json.Serialization;
//using Microsoft.Playwright;

//namespace WebApplication2.cls
//{
//    public class clsReportPdfBuilder
//    {
//        public static byte[] Build(string templateJson, PrintData data)
//            => BuildAsync(templateJson, data).GetAwaiter().GetResult();

//        private static async System.Threading.Tasks.Task<byte[]> BuildAsync(string templateJson, PrintData data)
//        {
//            if (string.IsNullOrWhiteSpace(templateJson))
//                throw new ArgumentException("templateJson is empty");

//            var tpl = JsonSerializer.Deserialize<TemplateModel>(
//                templateJson,
//                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

//            if (tpl == null) throw new Exception("Template deserialize failed.");

//            // Compute page in px based on preset/orientation/dpi
//            var dpi = tpl.Page?.Dpi > 0 ? tpl.Page.Dpi : 96.0;
//            var (pageWpx, pageHpx, pageWmm, pageHmm) = ResolvePage(tpl.Page, dpi);

//            var html = BuildHtml(tpl, data, pageWpx, pageHpx, pageWmm, pageHmm);

//            using var playwright = await Playwright.CreateAsync();
//            await using var browser = await playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions
//            {
//                Headless = true
//            });

//            var page = await browser.NewPageAsync(new BrowserNewPageOptions
//            {
//                ViewportSize = new ViewportSize { Width = (int)Math.Ceiling(pageWpx), Height = (int)Math.Ceiling(pageHpx) }
//            });

//            await page.SetContentAsync(html, new PageSetContentOptions { WaitUntil = WaitUntilState.NetworkIdle });

//            // IMPORTANT: margin 0 so canvas is 1:1 (no shrink/fit weirdness)
//            var pdfBytes = await page.PdfAsync(new PagePdfOptions
//            {
//                Width = $"{pageWmm.ToString("0.###", CultureInfo.InvariantCulture)}mm",
//                Height = $"{pageHmm.ToString("0.###", CultureInfo.InvariantCulture)}mm",
//                PrintBackground = true,
//                PreferCSSPageSize = true,
//                Margin = new Margin
//                {
//                    Top = "0mm",
//                    Right = "0mm",
//                    Bottom = "0mm",
//                    Left = "0mm"
//                },
//                Scale = 1
//            });

//            return pdfBytes;
//        }

//        private static string BuildHtml(TemplateModel tpl, PrintData data, double pageWpx, double pageHpx, double pageWmm, double pageHmm)
//        {
//            var fontPath = Path.Combine(AppContext.BaseDirectory, "Fonts", "Noto_Naskh_Arabic", "static", "NotoNaskhArabic-Regular.ttf");
//            var fontUrl = new Uri(fontPath).AbsoluteUri;

//            var elements = tpl.Elements ?? new List<ElementDef>();

//            var sb = new StringBuilder();
//            sb.AppendLine("<!doctype html>");
//            sb.AppendLine("<html><head><meta charset='utf-8'/>");
//            sb.AppendLine("<style>");

//            // Exact page size + ZERO margins to avoid scaling/shifting
//            sb.AppendLine($"@page {{ size: {pageWmm.ToString("0.###", CultureInfo.InvariantCulture)}mm {pageHmm.ToString("0.###", CultureInfo.InvariantCulture)}mm; margin: 0; }}");

//            sb.AppendLine($"@font-face {{ font-family: 'NotoNaskh'; src: url('{fontUrl}') format('truetype'); }}");
//            sb.AppendLine("html, body { margin:0; padding:0; }");
//            sb.AppendLine("body { font-family: NotoNaskh, Arial, sans-serif; }");

//            // Canvas exactly equals page px
//            sb.AppendLine($".canvas {{ position: relative; width: {pageWpx.ToString("0.###", CultureInfo.InvariantCulture)}px; height: {pageHpx.ToString("0.###", CultureInfo.InvariantCulture)}px; overflow:hidden; }}");
//            sb.AppendLine(".el { position:absolute; box-sizing:border-box; }");

//            // Table defaults (we will override by template styles too)
//            sb.AppendLine("table { border-collapse: collapse; width: 100%; }");
//            sb.AppendLine("th, td { border: 1px solid #333; padding: 4px 6px; }");

//            sb.AppendLine("</style></head><body>");
//            sb.AppendLine("<div class='canvas'>");

//            foreach (var el in elements.OrderBy(e => e.Y))
//            {
//                var type = (el.Type ?? "").Trim().ToLowerInvariant();
//                switch (type)
//                {
//                    case "text":
//                        sb.AppendLine(RenderText(el, el.Text ?? "", isField: false));
//                        break;
//                    case "field":
//                        sb.AppendLine(RenderText(el, ResolveField(data, el.FieldName), isField: true));
//                        break;
//                    case "image":
//                        sb.AppendLine(RenderImage(el, ResolveImage(data, el.Source)));
//                        break;
//                    case "line":
//                        sb.AppendLine(RenderLine(el));
//                        break;
//                    case "table":
//                        sb.AppendLine(RenderTable(el, data));
//                        break;
//                }
//            }

//            sb.AppendLine("</div></body></html>");
//            return sb.ToString();
//        }

//        private static string RenderText(ElementDef el, string text, bool isField)
//        {
//            var x = ToPx(el.X);
//            var y = ToPx(el.Y);
//            var w = ToPx(el.Width);
//            var h = ToPx(el.Height);

//            var fontSize = el.FontSize > 0 ? el.FontSize : 12;
//            var color = CssColor(el.ColorHex);
//            var align = (el.TextAlign ?? "left").ToLowerInvariant();
//            var cssAlign = align == "center" ? "center" : align == "right" ? "right" : "left";

//            // Better RTL: detect Arabic chars
//            var dir = ContainsArabic(text) ? "rtl" : "ltr";

//            // Border support
//            var borderCss = "";
//            if (el.BorderEnabled)
//            {
//                var bc = CssColor(el.BorderColorHex);
//                var bl = Math.Max(0, el.BorderLeft);
//                var bt = Math.Max(0, el.BorderTop);
//                var br = Math.Max(0, el.BorderRight);
//                var bb = Math.Max(0, el.BorderBottom);
//                borderCss =
//                    $"border-left:{bl}px solid {bc};" +
//                    $"border-top:{bt}px solid {bc};" +
//                    $"border-right:{br}px solid {bc};" +
//                    $"border-bottom:{bb}px solid {bc};";
//            }

//            var safe = HtmlEscape(text ?? "");

//            // Keep min-height (avoid clipping). If you want clipping, add overflow:hidden.
//            return $@"
//<div class='el'
//     style='left:{x}; top:{y}; width:{w}; min-height:{h};
//            {borderCss}
//            color:{color}; font-size:{fontSize}px;
//            text-align:{cssAlign}; direction:{dir};
//            white-space:pre-wrap; line-height:1.2;'>
//  {safe}
//</div>";
//        }

//        private static string RenderImage(ElementDef el, string base64)
//        {
//            if (string.IsNullOrWhiteSpace(base64)) return "";

//            var b64 = base64;
//            var comma = b64.IndexOf(',');
//            if (comma > 0 && b64.Substring(0, comma).Contains("base64", StringComparison.OrdinalIgnoreCase))
//                b64 = b64[(comma + 1)..];

//            var x = ToPx(el.X);
//            var y = ToPx(el.Y);
//            var w = ToPx(el.Width);
//            var h = ToPx(el.Height);

//            return $@"
//<img class='el'
//     style='left:{x}; top:{y}; width:{w}; height:{h}; object-fit:contain;'
//     src='data:image/png;base64,{b64}' />";
//        }

//        private static string RenderLine(ElementDef el)
//        {
//            var x = ToPx(el.X);
//            var y = ToPx(el.Y);
//            var w = ToPx(el.Width);
//            var h = ToPx(Math.Max(1, el.Thickness));
//            var color = CssColor(el.ColorHex);
//            return $@"<div class='el' style='left:{x}; top:{y}; width:{w}; height:{h}; background:{color};'></div>";
//        }

//        private static string RenderTable(ElementDef el, PrintData data)
//        {
//            if (el.Table?.Columns == null || el.Table.Columns.Count == 0) return "";

//            var x = ToPx(el.X);
//            var y = ToPx(el.Y);
//            var w = ToPx(el.Width);

//            var cols = el.Table.Columns;

//            var headerFont = el.Table.HeaderStyle?.FontSize ?? 11;
//            var headerBg = CssColor(el.Table.HeaderStyle?.BackgroundColorHex, "#F3F4F6");
//            var headerText = CssColor(el.Table.HeaderStyle?.TextColorHex, "#111827");
//            var headerBold = (el.Table.HeaderStyle?.Bold ?? true) ? "font-weight:700;" : "font-weight:400;";
//            var headerH = el.Table.HeaderStyle?.Height ?? 28;

//            var rowFont = el.Table.RowStyle?.FontSize ?? 10;
//            var rowText = CssColor(el.Table.RowStyle?.TextColorHex, "#6B7280");
//            var rowBg = CssColor(el.Table.RowStyle?.BackgroundColorHex, "#FFFFFF");
//            var rowH = el.Table.RowStyle?.Height ?? 25;

//            // Optional RTL for Arabic tables
//            var tableDir = "rtl";

//            var colGroup = new StringBuilder();
//            colGroup.Append("<colgroup>");
//            foreach (var c in cols)
//            {
//                var cw = Math.Max(20, c.Width);
//                colGroup.Append($"<col style='width:{cw.ToString("0.###", CultureInfo.InvariantCulture)}px'>");
//            }
//            colGroup.Append("</colgroup>");

//            var sb = new StringBuilder();
//            sb.AppendLine($@"<div class='el' style='left:{x}; top:{y}; width:{w};'>");
//            sb.AppendLine($@"<table dir='{tableDir}' style='width:100%;'>");
//            sb.AppendLine(colGroup.ToString());

//            if (el.Table.ShowHeader)
//            {
//                sb.AppendLine("<thead><tr>");
//                foreach (var c in cols)
//                {
//                    sb.AppendLine($@"
//<th style='background:{headerBg}; color:{headerText}; {headerBold}
//           font-size:{headerFont}px; height:{headerH}px;'>
//  {HtmlEscape(c.Title ?? "")}
//</th>");
//                }
//                sb.AppendLine("</tr></thead>");
//            }

//            sb.AppendLine("<tbody>");
//            var lines = data.Lines ?? new List<Dictionary<string, object>>();

//            for (int i = 0; i < lines.Count; i++)
//            {
//                var bg = rowBg;

//                if (el.Table.Zebra && !string.IsNullOrWhiteSpace(el.Table.RowStyle?.ZebraColorHex) && (i % 2 == 1))
//                    bg = CssColor(el.Table.RowStyle.ZebraColorHex, rowBg);

//                sb.AppendLine("<tr>");
//                foreach (var c in cols)
//                {
//                    lines[i].TryGetValue(c.Field ?? "", out var v);

//                    var cellAlign = (c.Align ?? "left").ToLowerInvariant();
//                    var cssAlign = cellAlign == "center" ? "center" : cellAlign == "right" ? "right" : "left";
//                    var bold = c.Bold ? "font-weight:700;" : "font-weight:400;";

//                    sb.AppendLine($@"
//<td style='background:{bg}; color:{rowText}; font-size:{rowFont}px; height:{rowH}px;
//          text-align:{cssAlign}; {bold}'>
//  {HtmlEscape(v?.ToString() ?? "")}
//</td>");
//                }
//                sb.AppendLine("</tr>");
//            }

//            sb.AppendLine("</tbody></table></div>");
//            return sb.ToString();
//        }

//        private static string ResolveField(PrintData data, string key)
//        {
//            if (string.IsNullOrWhiteSpace(key)) return "";

//            if (data.Header != null && data.Header.TryGetValue(key, out var v1)) return v1?.ToString() ?? "";
//            if (data.Footer != null && data.Footer.TryGetValue(key, out var v2)) return v2?.ToString() ?? "";
//            if (data.Company != null && data.Company.TryGetValue(key, out var v3)) return v3?.ToString() ?? "";

//            return "";
//        }

//        private static string ResolveImage(PrintData data, string source)
//        {
//            if (string.IsNullOrWhiteSpace(source)) return "";
//            if (source == "company_logo" && data.Company != null && data.Company.TryGetValue("LogoBase64", out var v))
//                return v?.ToString() ?? "";
//            return "";
//        }

//        private static (double pageWpx, double pageHpx, double pageWmm, double pageHmm) ResolvePage(PageDef page, double dpi)
//        {
//            // Defaults
//            var preset = (page?.Preset ?? "a4").ToLowerInvariant();
//            var orientation = (page?.Orientation ?? "portrait").ToLowerInvariant();

//            // mm sizes
//            double wmm, hmm;

//            switch (preset)
//            {
//                case "a3":
//                    wmm = 297; hmm = 420; break;
//                case "thermal80":
//                case "thermal_80":
//                    wmm = 80; hmm = 300; break; // example
//                case "thermal58":
//                case "thermal_58":
//                    wmm = 58; hmm = 300; break; // example
//                case "a4":
//                default:
//                    wmm = 210; hmm = 297; break;
//            }

//            if (orientation == "landscape")
//                (wmm, hmm) = (hmm, wmm);

//            // px from mm
//            var wpx = (wmm / 25.4) * dpi;
//            var hpx = (hmm / 25.4) * dpi;

//            return (wpx, hpx, wmm, hmm);
//        }

//        private static string ToPx(double v) => $"{v.ToString("0.###", CultureInfo.InvariantCulture)}px";

//        private static string CssColor(string hex, string fallback = "#000000")
//        {
//            if (string.IsNullOrWhiteSpace(hex)) return fallback;
//            var h = hex.Trim();
//            if (!h.StartsWith("#")) h = "#" + h;
//            return h.Length == 7 ? h : fallback;
//        }

//        private static string HtmlEscape(string s)
//        {
//            if (string.IsNullOrEmpty(s)) return "";
//            return s.Replace("&", "&amp;")
//                    .Replace("<", "&lt;")
//                    .Replace(">", "&gt;")
//                    .Replace("\"", "&quot;")
//                    .Replace("'", "&#39;");
//        }

//        private static bool ContainsArabic(string s)
//        {
//            if (string.IsNullOrEmpty(s)) return false;
//            foreach (var ch in s)
//            {
//                // Arabic blocks
//                if ((ch >= '\u0600' && ch <= '\u06FF') ||
//                    (ch >= '\u0750' && ch <= '\u077F') ||
//                    (ch >= '\u08A0' && ch <= '\u08FF') ||
//                    (ch >= '\uFB50' && ch <= '\uFDFF') ||
//                    (ch >= '\uFE70' && ch <= '\uFEFF'))
//                    return true;
//            }
//            return false;
//        }

//        // =========================
//        // DTOs (extended to match your JSON)
//        // =========================

//        public class TemplateModel
//        {
//            [JsonPropertyName("page")]
//            public PageDef Page { get; set; }

//            [JsonPropertyName("elements")]
//            public List<ElementDef> Elements { get; set; }
//        }

//        public class PageDef
//        {
//            [JsonPropertyName("preset")]
//            public string Preset { get; set; }

//            [JsonPropertyName("orientation")]
//            public string Orientation { get; set; }

//            [JsonPropertyName("dpi")]
//            public double Dpi { get; set; }
//        }

//        public class ElementDef
//        {
//            [JsonPropertyName("id")]
//            public string Id { get; set; }

//            [JsonPropertyName("type")]
//            public string Type { get; set; }

//            [JsonPropertyName("band")]
//            public string Band { get; set; }

//            [JsonPropertyName("x")]
//            public double X { get; set; }

//            [JsonPropertyName("y")]
//            public double Y { get; set; }

//            [JsonPropertyName("width")]
//            public double Width { get; set; }

//            [JsonPropertyName("height")]
//            public double Height { get; set; }

//            [JsonPropertyName("fontSize")]
//            public int FontSize { get; set; }

//            [JsonPropertyName("bold")]
//            public bool Bold { get; set; }

//            [JsonPropertyName("colorHex")]
//            public string ColorHex { get; set; }

//            [JsonPropertyName("textAlign")]
//            public string TextAlign { get; set; }

//            [JsonPropertyName("text")]
//            public string Text { get; set; }

//            [JsonPropertyName("fieldName")]
//            public string FieldName { get; set; }

//            [JsonPropertyName("source")]
//            public string Source { get; set; }

//            [JsonPropertyName("thickness")]
//            public int Thickness { get; set; }

//            [JsonPropertyName("table")]
//            public TableDef Table { get; set; }

//            // ✅ Border fields (in your JSON)
//            [JsonPropertyName("borderEnabled")]
//            public bool BorderEnabled { get; set; }

//            [JsonPropertyName("borderColorHex")]
//            public string BorderColorHex { get; set; }

//            [JsonPropertyName("borderLeft")]
//            public double BorderLeft { get; set; }

//            [JsonPropertyName("borderTop")]
//            public double BorderTop { get; set; }

//            [JsonPropertyName("borderRight")]
//            public double BorderRight { get; set; }

//            [JsonPropertyName("borderBottom")]
//            public double BorderBottom { get; set; }
//        }

//        public class TableDef
//        {
//            [JsonPropertyName("showHeader")]
//            public bool ShowHeader { get; set; } = true;

//            [JsonPropertyName("repeatHeader")]
//            public bool RepeatHeader { get; set; } = true;

//            [JsonPropertyName("showGrid")]
//            public bool ShowGrid { get; set; } = true;

//            [JsonPropertyName("zebra")]
//            public bool Zebra { get; set; } = false;

//            [JsonPropertyName("headerStyle")]
//            public TableHeaderStyleDef HeaderStyle { get; set; }

//            [JsonPropertyName("rowStyle")]
//            public TableRowStyleDef RowStyle { get; set; }

//            [JsonPropertyName("columns")]
//            public List<TableColumnDef> Columns { get; set; } = new();
//        }

//        public class TableHeaderStyleDef
//        {
//            [JsonPropertyName("height")]
//            public double Height { get; set; } = 28;

//            [JsonPropertyName("fontSize")]
//            public int FontSize { get; set; } = 11;

//            [JsonPropertyName("bold")]
//            public bool Bold { get; set; } = true;

//            [JsonPropertyName("textColorHex")]
//            public string TextColorHex { get; set; } = "#111827";

//            [JsonPropertyName("backgroundColorHex")]
//            public string BackgroundColorHex { get; set; } = "#F3F4F6";
//        }

//        public class TableRowStyleDef
//        {
//            [JsonPropertyName("height")]
//            public double Height { get; set; } = 25;

//            [JsonPropertyName("fontSize")]
//            public int FontSize { get; set; } = 10;

//            [JsonPropertyName("textColorHex")]
//            public string TextColorHex { get; set; } = "#6B7280";

//            [JsonPropertyName("backgroundColorHex")]
//            public string BackgroundColorHex { get; set; } = "#FFFFFF";

//            [JsonPropertyName("zebraColorHex")]
//            public string ZebraColorHex { get; set; } = "#FAFAFA";
//        }

//        public class TableColumnDef
//        {
//            [JsonPropertyName("title")]
//            public string Title { get; set; }

//            [JsonPropertyName("field")]
//            public string Field { get; set; }

//            [JsonPropertyName("width")]
//            public double Width { get; set; } = 120;

//            // ✅ In your JSON
//            [JsonPropertyName("align")]
//            public string Align { get; set; } = "left";

//            [JsonPropertyName("bold")]
//            public bool Bold { get; set; } = false;
//        }

//        public class PrintData
//        {
//            public Dictionary<string, object> Company { get; set; } = new();
//            public Dictionary<string, object> Header { get; set; } = new();
//            public List<Dictionary<string, object>> Lines { get; set; } = new();
//            public Dictionary<string, object> Footer { get; set; } = new();
//        }
//    }
//}
////////////////// here 2 
///
// ============================================================================
// clsReportPdfBuilder.cs  (UPDATED)
// ✅ Adds: Table totals row (sum/count/avg/min/max) per column via column.totalCalc
// ✅ Adds: totalsStyle (background/text/bold/font/height)
// ✅ Adds: headerOverride + detailOverride per column (font/bold/text/background)
// ✅ Respects: showGrid, zebra, repeatHeader (best-effort with thead), showHeader
// ✅ Safer numeric parsing for totals + optional format per column
// ============================================================================

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Playwright;

namespace WebApplication2.cls
{
    public class clsReportPdfBuilder
    {
        public static byte[] Build(string templateJson, PrintData data)
            => BuildAsync(templateJson, data).GetAwaiter().GetResult();

        private static async System.Threading.Tasks.Task<byte[]> BuildAsync(string templateJson, PrintData data)
        {
            if (string.IsNullOrWhiteSpace(templateJson))
                throw new ArgumentException("templateJson is empty");

            var tpl = JsonSerializer.Deserialize<TemplateModel>(
                templateJson,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            if (tpl == null) throw new Exception("Template deserialize failed.");

            var dpi = tpl.Page?.Dpi > 0 ? tpl.Page.Dpi : 96.0;
            var (pageWpx, pageHpx, pageWmm, pageHmm) = ResolvePage(tpl.Page, dpi);

            var html = BuildHtml(tpl, data, pageWpx, pageHpx, pageWmm, pageHmm);

            using var playwright = await Playwright.CreateAsync();
            await using var browser = await playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions
            {
                Headless = true
            });

            var page = await browser.NewPageAsync(new BrowserNewPageOptions
            {
                ViewportSize = new ViewportSize
                {
                    Width = (int)Math.Ceiling(pageWpx),
                    Height = (int)Math.Ceiling(pageHpx)
                }
            });

            await page.SetContentAsync(html, new PageSetContentOptions { WaitUntil = WaitUntilState.NetworkIdle });

            var pdfBytes = await page.PdfAsync(new PagePdfOptions
            {
                Width = $"{pageWmm.ToString("0.###", CultureInfo.InvariantCulture)}mm",
                Height = $"{pageHmm.ToString("0.###", CultureInfo.InvariantCulture)}mm",
                PrintBackground = true,
                PreferCSSPageSize = true,
                Margin = new Margin
                {
                    Top = "0mm",
                    Right = "0mm",
                    Bottom = "0mm",
                    Left = "0mm"
                },
                Scale = 1
            });

            return pdfBytes;
        }

        private static string BuildHtml(TemplateModel tpl, PrintData data, double pageWpx, double pageHpx, double pageWmm, double pageHmm)
        {
            var fontPath = Path.Combine(AppContext.BaseDirectory, "Fonts", "Noto_Naskh_Arabic", "static", "NotoNaskhArabic-Regular.ttf");
            var fontUrl = new Uri(fontPath).AbsoluteUri;

            var elements = tpl.Elements ?? new List<ElementDef>();

            var sb = new StringBuilder();
            sb.AppendLine("<!doctype html>");
            sb.AppendLine("<html><head><meta charset='utf-8'/>");
            sb.AppendLine("<style>");

            sb.AppendLine($"@page {{ size: {pageWmm.ToString("0.###", CultureInfo.InvariantCulture)}mm {pageHmm.ToString("0.###", CultureInfo.InvariantCulture)}mm; margin: 0; }}");
            sb.AppendLine($"@font-face {{ font-family: 'NotoNaskh'; src: url('{fontUrl}') format('truetype'); }}");
            sb.AppendLine("html, body { margin:0; padding:0; }");
            sb.AppendLine("body { font-family: NotoNaskh, Arial, sans-serif; }");

            sb.AppendLine($".canvas {{ position: relative; width: {pageWpx.ToString("0.###", CultureInfo.InvariantCulture)}px; height: {pageHpx.ToString("0.###", CultureInfo.InvariantCulture)}px; overflow:hidden; }}");
            sb.AppendLine(".el { position:absolute; box-sizing:border-box; }");

            // Table base
            sb.AppendLine("table { border-collapse: collapse; width: 100%; }");
            sb.AppendLine("</style></head><body>");
            sb.AppendLine("<div class='canvas'>");

            foreach (var el in elements.OrderBy(e => e.Y))
            {
                var type = (el.Type ?? "").Trim().ToLowerInvariant();
                switch (type)
                {
                    case "text":
                        sb.AppendLine(RenderText(el, el.Text ?? "", isField: false));
                        break;
                    case "field":
                        sb.AppendLine(RenderText(el, ResolveField(data, el.FieldName), isField: true));
                        break;
                    case "image":
                        sb.AppendLine(RenderImage(el, ResolveImage(data, el.Source)));
                        break;
                    case "line":
                        sb.AppendLine(RenderLine(el));
                        break;
                    case "table":
                        sb.AppendLine(RenderTable(el, data));
                        break;
                }
            }

            sb.AppendLine("</div></body></html>");
            return sb.ToString();
        }

        private static string RenderText(ElementDef el, string text, bool isField)
        {
            var x = ToPx(el.X);
            var y = ToPx(el.Y);
            var w = ToPx(el.Width);
            var h = ToPx(el.Height);

            var fontSize = el.FontSize > 0 ? el.FontSize : 12;
            var color = CssColor(el.ColorHex);
            var align = (el.TextAlign ?? "left").ToLowerInvariant();
            var cssAlign = align == "center" ? "center" : align == "right" ? "right" : "left";

            var dir = ContainsArabic(text) ? "rtl" : "ltr";

            var borderCss = "";
            if (el.BorderEnabled)
            {
                var bc = CssColor(el.BorderColorHex);
                var bl = Math.Max(0, el.BorderLeft);
                var bt = Math.Max(0, el.BorderTop);
                var br = Math.Max(0, el.BorderRight);
                var bb = Math.Max(0, el.BorderBottom);
                borderCss =
                    $"border-left:{bl}px solid {bc};" +
                    $"border-top:{bt}px solid {bc};" +
                    $"border-right:{br}px solid {bc};" +
                    $"border-bottom:{bb}px solid {bc};";
            }

            var safe = HtmlEscape(text ?? "");

            return $@"
<div class='el'
     style='left:{x}; top:{y}; width:{w}; min-height:{h};
            {borderCss}
            color:{color}; font-size:{fontSize}px;
            text-align:{cssAlign}; direction:{dir};
            white-space:pre-wrap; line-height:1.2;'>
  {safe}
</div>";
        }

        private static string RenderImage(ElementDef el, string base64)
        {
            if (string.IsNullOrWhiteSpace(base64)) return "";

            var b64 = base64;
            var comma = b64.IndexOf(',');
            if (comma > 0 && b64.Substring(0, comma).Contains("base64", StringComparison.OrdinalIgnoreCase))
                b64 = b64[(comma + 1)..];

            var x = ToPx(el.X);
            var y = ToPx(el.Y);
            var w = ToPx(el.Width);
            var h = ToPx(el.Height);

            return $@"
<img class='el'
     style='left:{x}; top:{y}; width:{w}; height:{h}; object-fit:contain;'
     src='data:image/png;base64,{b64}' />";
        }

        private static string RenderLine(ElementDef el)
        {
            var x = ToPx(el.X);
            var y = ToPx(el.Y);
            var w = ToPx(el.Width);
            var h = ToPx(Math.Max(1, el.Thickness));
            var color = CssColor(el.ColorHex);
            return $@"<div class='el' style='left:{x}; top:{y}; width:{w}; height:{h}; background:{color};'></div>";
        }

        // =========================================================================
        // TABLE (UPDATED: totals + overrides + showGrid)
        // =========================================================================
        private static string RenderTable(ElementDef el, PrintData data)
        {
            if (el.Table?.Columns == null || el.Table.Columns.Count == 0) return "";

            var x = ToPx(el.X);
            var y = ToPx(el.Y);
            var w = ToPx(el.Width);

            var t = el.Table;
            var cols = t.Columns ?? new List<TableColumnDef>();

            // Table style defaults
            var headerFont = t.HeaderStyle?.FontSize ?? 11;
            var headerBgDefault = CssColor(t.HeaderStyle?.BackgroundColorHex, "#F3F4F6");
            var headerTextDefault = CssColor(t.HeaderStyle?.TextColorHex, "#111827");
            var headerBoldDefault = (t.HeaderStyle?.Bold ?? true);
            var headerH = t.HeaderStyle?.Height ?? 28;

            var rowFont = t.RowStyle?.FontSize ?? 10;
            var rowTextDefault = CssColor(t.RowStyle?.TextColorHex, "#6B7280");
            var rowBgDefault = CssColor(t.RowStyle?.BackgroundColorHex, "#FFFFFF");
            var rowH = t.RowStyle?.Height ?? 25;
            var zebraBg = CssColor(t.RowStyle?.ZebraColorHex, rowBgDefault);

            // Totals style
            var showTotals = t.ShowTotals;
            var totalsH = t.TotalsStyle?.Height ?? 30;
            var totalsFont = t.TotalsStyle?.FontSize ?? 11;
            var totalsBold = (t.TotalsStyle?.Bold ?? true);
            var totalsTextDefault = CssColor(t.TotalsStyle?.TextColorHex, "#111827");
            var totalsBgDefault = CssColor(t.TotalsStyle?.BackgroundColorHex, "#E5E7EB");

            // Grid/borders
            var borderCss = t.ShowGrid ? "1px solid #333" : "0px solid transparent";
            var padCss = "4px 6px";

            // (Optional) RTL (your templates are Arabic, so default rtl)
            var tableDir = "rtl";

            // Col widths
            var colGroup = new StringBuilder();
            colGroup.Append("<colgroup>");
            foreach (var c in cols)
            {
                var cw = Math.Max(20, c.Width);
                colGroup.Append($"<col style='width:{cw.ToString("0.###", CultureInfo.InvariantCulture)}px'>");
            }
            colGroup.Append("</colgroup>");

            // Data lines
            var lines = data.Lines ?? new List<Dictionary<string, object>>();

            // Pre-calc totals (numeric columns only)
            var totals = showTotals ? ComputeTotals(lines, cols) : null;

            var sb = new StringBuilder();
            sb.AppendLine($@"<div class='el' style='left:{x}; top:{y}; width:{w};'>");
            sb.AppendLine($@"<table dir='{tableDir}' style='width:100%;'>");
            sb.AppendLine(colGroup.ToString());

            // Header
            if (t.ShowHeader)
            {
                sb.AppendLine("<thead style='display: table-header-group;'><tr>");
                foreach (var c in cols)
                {
                    var hOv = c.HeaderOverride;

                    var bg = CssColor(hOv?.BackgroundColorHex, headerBgDefault);
                    var tc = CssColor(hOv?.TextColorHex, headerTextDefault);

                    var fz = hOv?.FontSize ?? headerFont;
                    var b = (hOv?.Bold ?? headerBoldDefault) ? "font-weight:700;" : "font-weight:400;";

                    var align = (c.Align ?? "left").ToLowerInvariant();
                    var cssAlign = align == "center" ? "center" : align == "right" ? "right" : "left";

                    sb.AppendLine($@"
<th style='background:{bg}; color:{tc}; {b}
           font-size:{fz}px; height:{headerH}px;
           border:{borderCss}; padding:{padCss};
           text-align:{cssAlign};'>
  {HtmlEscape(c.Title ?? "")}
</th>");
                }
                sb.AppendLine("</tr></thead>");
            }

            // Body
            sb.AppendLine("<tbody>");
            for (int i = 0; i < lines.Count; i++)
            {
                var rowBg = rowBgDefault;
                if (t.Zebra && (i % 2 == 1)) rowBg = zebraBg;

                sb.AppendLine("<tr>");
                foreach (var c in cols)
                {
                    lines[i].TryGetValue(c.Field ?? "", out var v);

                    var dOv = c.DetailOverride;

                    var bg = CssColor(dOv?.BackgroundColorHex, rowBg);
                    var tc = CssColor(dOv?.TextColorHex, rowTextDefault);

                    var fz = dOv?.FontSize ?? rowFont;
                    var b = (dOv?.Bold ?? c.Bold) ? "font-weight:700;" : "font-weight:400;";

                    var align = (c.Align ?? "left").ToLowerInvariant();
                    var cssAlign = align == "center" ? "center" : align == "right" ? "right" : "left";

                    var text = FormatValue(v, c.Format);

                    sb.AppendLine($@"
<td style='background:{bg}; color:{tc}; font-size:{fz}px; height:{rowH}px;
          text-align:{cssAlign}; {b}
          border:{borderCss}; padding:{padCss};
          vertical-align:middle;'>
  {HtmlEscape(text)}
</td>");
                }
                sb.AppendLine("</tr>");
            }

            // Totals row
            if (showTotals && totals != null)
            {
                sb.AppendLine("<tr>");
                foreach (var c in cols)
                {
                    var key = (c.Field ?? "").Trim();
                    totals.TryGetValue(key, out var tv);

                    // Totals cell can also respect detailOverride if you want,
                    // but we’ll prioritize totalsStyle for consistent look.
                    var align = (c.Align ?? "left").ToLowerInvariant();
                    var cssAlign = align == "center" ? "center" : align == "right" ? "right" : "left";
                    var b = totalsBold ? "font-weight:700;" : "font-weight:400;";

                    // If column has no totalCalc, show empty (unless you want labels).
                    var calc = ParseTotalCalc(c.TotalCalc);
                    var text = "";
                    if (calc != TotalCalculationType.None)
                    {
                        // If user provided a format -> use it
                        text = FormatValue(tv, c.Format);
                    }

                    sb.AppendLine($@"
<td style='background:{totalsBgDefault}; color:{totalsTextDefault}; font-size:{totalsFont}px; height:{totalsH}px;
          text-align:{cssAlign}; {b}
          border:{borderCss}; padding:{padCss};
          vertical-align:middle;'>
  {HtmlEscape(text)}
</td>");
                }
                sb.AppendLine("</tr>");
            }

            sb.AppendLine("</tbody></table></div>");
            return sb.ToString();
        }

        // =========================================================================
        // TOTALS HELPERS
        // =========================================================================

        private static Dictionary<string, object> ComputeTotals(List<Dictionary<string, object>> lines, List<TableColumnDef> cols)
        {
            var result = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);

            foreach (var c in cols)
            {
                var field = (c.Field ?? "").Trim();
                if (string.IsNullOrWhiteSpace(field)) continue;

                var calc = ParseTotalCalc(c.TotalCalc);
                if (calc == TotalCalculationType.None) continue;

                // Collect numeric values
                var values = new List<decimal>();
                foreach (var row in lines)
                {
                    if (row == null) continue;
                    row.TryGetValue(field, out var raw);
                    if (TryToDecimal(raw, out var d)) values.Add(d);
                }

                object totalValue = "";

                switch (calc)
                {
                    case TotalCalculationType.Sum:
                        totalValue = values.Sum();
                        break;

                    case TotalCalculationType.Count:
                        // Count rows that have a value (numeric or non-empty)
                        int cnt = 0;
                        foreach (var row in lines)
                        {
                            if (row == null) continue;
                            if (!row.TryGetValue(field, out var raw) || raw == null) continue;

                            if (raw is string s)
                            {
                                if (!string.IsNullOrWhiteSpace(s)) cnt++;
                            }
                            else
                            {
                                cnt++;
                            }
                        }
                        totalValue = cnt;
                        break;

                    case TotalCalculationType.Average:
                        totalValue = values.Count == 0 ? 0m : (values.Sum() / values.Count);
                        break;

                    case TotalCalculationType.Min:
                        totalValue = values.Count == 0 ? 0m : values.Min();
                        break;

                    case TotalCalculationType.Max:
                        totalValue = values.Count == 0 ? 0m : values.Max();
                        break;
                }

                result[field] = totalValue;
            }

            return result;
        }

        private static TotalCalculationType ParseTotalCalc(string totalCalc)
        {
            if (string.IsNullOrWhiteSpace(totalCalc)) return TotalCalculationType.None;

            var t = totalCalc.Trim().ToLowerInvariant();
            return t switch
            {
                "sum" => TotalCalculationType.Sum,
                "count" => TotalCalculationType.Count,
                "average" => TotalCalculationType.Average,
                "avg" => TotalCalculationType.Average,
                "min" => TotalCalculationType.Min,
                "max" => TotalCalculationType.Max,
                _ => TotalCalculationType.None
            };
        }

        private static bool TryToDecimal(object raw, out decimal value)
        {
            value = 0m;
            if (raw == null) return false;

            try
            {
                switch (raw)
                {
                    case decimal d:
                        value = d; return true;
                    case double db:
                        value = Convert.ToDecimal(db, CultureInfo.InvariantCulture); return true;
                    case float f:
                        value = Convert.ToDecimal(f, CultureInfo.InvariantCulture); return true;
                    case long l:
                        value = l; return true;
                    case int i:
                        value = i; return true;
                    case short s:
                        value = s; return true;
                    case string str:
                        {
                            if (string.IsNullOrWhiteSpace(str)) return false;

                            // Remove common separators and currency bits safely
                            var cleaned = str.Trim();

                            // If the string contains Arabic digits or commas etc., you can enhance this later.
                            // For now: try invariant, then current.
                            if (decimal.TryParse(cleaned, NumberStyles.Any, CultureInfo.InvariantCulture, out var v1))
                            { value = v1; return true; }

                            if (decimal.TryParse(cleaned, NumberStyles.Any, CultureInfo.CurrentCulture, out var v2))
                            { value = v2; return true; }

                            // try removing commas
                            cleaned = cleaned.Replace(",", "");
                            if (decimal.TryParse(cleaned, NumberStyles.Any, CultureInfo.InvariantCulture, out var v3))
                            { value = v3; return true; }

                            return false;
                        }
                    default:
                        // attempt convert
                        value = Convert.ToDecimal(raw, CultureInfo.InvariantCulture);
                        return true;
                }
            }
            catch
            {
                return false;
            }
        }

        private static string FormatValue(object v, string format)
        {
            if (v == null) return "";
            if (format == "currency") {
                return Simulate.Currency_format(v);
            }
            // If format is null/empty -> use ToString
            if (string.IsNullOrWhiteSpace(format))
                return v.ToString();

            try
            {
                // If numeric, apply .NET format (e.g. "N2", "0.00", "C2")
                if (TryToDecimal(v, out var dec))
                {
                    // Use invariant for PDFs consistency (you can switch to ar-JO or ar-AE if needed)
                    return dec.ToString(format, CultureInfo.InvariantCulture);
                }

                // If not numeric, just return ToString
                return v.ToString();
            }
            catch
            {
                return v.ToString();
            }
        }

        // =========================================================================
        // DATA RESOLVE
        // =========================================================================

        private static string ResolveField(PrintData data, string key)
        {
            if (string.IsNullOrWhiteSpace(key)) return "";

            if (data.Header != null && data.Header.TryGetValue(key, out var v1)) return v1?.ToString() ?? "";
            if (data.Footer != null && data.Footer.TryGetValue(key, out var v2)) return v2?.ToString() ?? "";
            if (data.Company != null && data.Company.TryGetValue(key, out var v3)) return v3?.ToString() ?? "";

            return "";
        }

        private static string ResolveImage(PrintData data, string source)
        {
            if (string.IsNullOrWhiteSpace(source)) return "";
            if (source == "company_logo" && data.Company != null && data.Company.TryGetValue("LogoBase64", out var v))
                return v?.ToString() ?? "";
            return "";
        }

        private static (double pageWpx, double pageHpx, double pageWmm, double pageHmm) ResolvePage(PageDef page, double dpi)
        {
            var preset = (page?.Preset ?? "a4").ToLowerInvariant();
            var orientation = (page?.Orientation ?? "portrait").ToLowerInvariant();

            double wmm, hmm;

            switch (preset)
            {
                case "a3":
                    wmm = 297; hmm = 420; break;
                case "thermal80":
                case "thermal_80":
                    wmm = 80; hmm = 300; break;
                case "thermal58":
                case "thermal_58":
                    wmm = 58; hmm = 300; break;
                case "a4":
                default:
                    wmm = 210; hmm = 297; break;
            }

            if (orientation == "landscape")
                (wmm, hmm) = (hmm, wmm);

            var wpx = (wmm / 25.4) * dpi;
            var hpx = (hmm / 25.4) * dpi;

            return (wpx, hpx, wmm, hmm);
        }

        private static string ToPx(double v) => $"{v.ToString("0.###", CultureInfo.InvariantCulture)}px";

        private static string CssColor(string hex, string fallback = "#000000")
        {
            if (string.IsNullOrWhiteSpace(hex)) return fallback;
            var h = hex.Trim();
            if (!h.StartsWith("#")) h = "#" + h;
            return h.Length == 7 ? h : fallback;
        }

        private static string HtmlEscape(string s)
        {
            if (string.IsNullOrEmpty(s)) return "";
            return s.Replace("&", "&amp;")
                    .Replace("<", "&lt;")
                    .Replace(">", "&gt;")
                    .Replace("\"", "&quot;")
                    .Replace("'", "&#39;");
        }

        private static bool ContainsArabic(string s)
        {
            if (string.IsNullOrEmpty(s)) return false;
            foreach (var ch in s)
            {
                if ((ch >= '\u0600' && ch <= '\u06FF') ||
                    (ch >= '\u0750' && ch <= '\u077F') ||
                    (ch >= '\u08A0' && ch <= '\u08FF') ||
                    (ch >= '\uFB50' && ch <= '\uFDFF') ||
                    (ch >= '\uFE70' && ch <= '\uFEFF'))
                    return true;
            }
            return false;
        }

        // =========================================================================
        // DTOs (UPDATED to match your JSON)
        // =========================================================================

        public enum TotalCalculationType
        {
            None,
            Sum,
            Count,
            Average,
            Min,
            Max
        }

        public class TemplateModel
        {
            [JsonPropertyName("page")]
            public PageDef Page { get; set; }

            [JsonPropertyName("elements")]
            public List<ElementDef> Elements { get; set; }
        }

        public class PageDef
        {
            [JsonPropertyName("preset")]
            public string Preset { get; set; }

            [JsonPropertyName("orientation")]
            public string Orientation { get; set; }

            [JsonPropertyName("dpi")]
            public double Dpi { get; set; }
        }

        public class ElementDef
        {
            [JsonPropertyName("id")]
            public string Id { get; set; }

            [JsonPropertyName("type")]
            public string Type { get; set; }

            [JsonPropertyName("band")]
            public string Band { get; set; }

            [JsonPropertyName("x")]
            public double X { get; set; }

            [JsonPropertyName("y")]
            public double Y { get; set; }

            [JsonPropertyName("width")]
            public double Width { get; set; }

            [JsonPropertyName("height")]
            public double Height { get; set; }

            [JsonPropertyName("fontSize")]
            public int FontSize { get; set; }

            [JsonPropertyName("bold")]
            public bool Bold { get; set; }

            [JsonPropertyName("colorHex")]
            public string ColorHex { get; set; }

            [JsonPropertyName("textAlign")]
            public string TextAlign { get; set; }

            [JsonPropertyName("text")]
            public string Text { get; set; }

            [JsonPropertyName("fieldName")]
            public string FieldName { get; set; }

            [JsonPropertyName("source")]
            public string Source { get; set; }

            [JsonPropertyName("thickness")]
            public int Thickness { get; set; }

            [JsonPropertyName("table")]
            public TableDef Table { get; set; }

            [JsonPropertyName("borderEnabled")]
            public bool BorderEnabled { get; set; }

            [JsonPropertyName("borderColorHex")]
            public string BorderColorHex { get; set; }

            [JsonPropertyName("borderLeft")]
            public double BorderLeft { get; set; }

            [JsonPropertyName("borderTop")]
            public double BorderTop { get; set; }

            [JsonPropertyName("borderRight")]
            public double BorderRight { get; set; }

            [JsonPropertyName("borderBottom")]
            public double BorderBottom { get; set; }
        }

        public class TableDef
        {
            [JsonPropertyName("showHeader")]
            public bool ShowHeader { get; set; } = true;

            [JsonPropertyName("repeatHeader")]
            public bool RepeatHeader { get; set; } = true; // best-effort

            [JsonPropertyName("showGrid")]
            public bool ShowGrid { get; set; } = true;

            [JsonPropertyName("zebra")]
            public bool Zebra { get; set; } = false;

            [JsonPropertyName("headerStyle")]
            public TableHeaderStyleDef HeaderStyle { get; set; }

            [JsonPropertyName("rowStyle")]
            public TableRowStyleDef RowStyle { get; set; }

            // ✅ totals
            [JsonPropertyName("showTotals")]
            public bool ShowTotals { get; set; } = false;

            [JsonPropertyName("totalsStyle")]
            public TableTotalsStyleDef TotalsStyle { get; set; }

            [JsonPropertyName("columns")]
            public List<TableColumnDef> Columns { get; set; } = new();
        }

        public class TableHeaderStyleDef
        {
            [JsonPropertyName("height")]
            public double Height { get; set; } = 28;

            [JsonPropertyName("fontSize")]
            public int FontSize { get; set; } = 11;

            [JsonPropertyName("bold")]
            public bool Bold { get; set; } = true;

            [JsonPropertyName("textColorHex")]
            public string TextColorHex { get; set; } = "#111827";

            [JsonPropertyName("backgroundColorHex")]
            public string BackgroundColorHex { get; set; } = "#F3F4F6";
        }

        public class TableRowStyleDef
        {
            [JsonPropertyName("height")]
            public double Height { get; set; } = 25;

            [JsonPropertyName("fontSize")]
            public int FontSize { get; set; } = 10;

            [JsonPropertyName("textColorHex")]
            public string TextColorHex { get; set; } = "#6B7280";

            [JsonPropertyName("backgroundColorHex")]
            public string BackgroundColorHex { get; set; } = "#FFFFFF";

            [JsonPropertyName("zebraColorHex")]
            public string ZebraColorHex { get; set; } = "#FAFAFA";
        }

        public class TableTotalsStyleDef
        {
            [JsonPropertyName("height")]
            public double Height { get; set; } = 30;

            [JsonPropertyName("fontSize")]
            public int FontSize { get; set; } = 11;

            [JsonPropertyName("bold")]
            public bool Bold { get; set; } = true;

            [JsonPropertyName("textColorHex")]
            public string TextColorHex { get; set; } = "#111827";

            [JsonPropertyName("backgroundColorHex")]
            public string BackgroundColorHex { get; set; } = "#E5E7EB";
        }

        public class TableCellStyleOverride
        {
            [JsonPropertyName("fontSize")]
            public int? FontSize { get; set; }

            [JsonPropertyName("bold")]
            public bool? Bold { get; set; }

            [JsonPropertyName("textColorHex")]
            public string TextColorHex { get; set; }

            [JsonPropertyName("backgroundColorHex")]
            public string BackgroundColorHex { get; set; }
        }

        public class TableColumnDef
        {
            [JsonPropertyName("title")]
            public string Title { get; set; }

            [JsonPropertyName("field")]
            public string Field { get; set; }

            [JsonPropertyName("width")]
            public double Width { get; set; } = 120;

            [JsonPropertyName("align")]
            public string Align { get; set; } = "left";

            [JsonPropertyName("bold")]
            public bool Bold { get; set; } = false;

            // ✅ Optional .NET numeric format string: "N2", "0.00", "C2", etc.
            [JsonPropertyName("format")]
            public string Format { get; set; }

            // ✅ Overrides
            [JsonPropertyName("headerOverride")]
            public TableCellStyleOverride HeaderOverride { get; set; }

            [JsonPropertyName("detailOverride")]
            public TableCellStyleOverride DetailOverride { get; set; }

            // ✅ Totals per column: "none|sum|count|average|min|max"
            [JsonPropertyName("totalCalc")]
            public string TotalCalc { get; set; } = "none";
        }

        public class PrintData
        {
            public Dictionary<string, object> Company { get; set; } = new();
            public Dictionary<string, object> Header { get; set; } = new();
            public List<Dictionary<string, object>> Lines { get; set; } = new();
            public Dictionary<string, object> Footer { get; set; } = new();
        }
    }
}
