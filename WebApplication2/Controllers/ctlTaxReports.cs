using ClosedXML.Excel;
using DocumentFormat.OpenXml.Presentation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using WebApplication2.cls;

namespace WebApplication2.Controllers
{
    [Route("api/ctlTaxReports")]
    public class ctlTaxReports : Controller
    {
        string SalesScript = @"
   select sum(PriceBeforeTax) PriceBeforeTax, 
               taxpercentage Taxpercentage, 
               sum(PriceBeforeTax)* (1+taxpercentage) TotalAfterTax,
               sum(PriceBeforeTax*taxpercentage ) Taxamount,
               AName TaxAName
        from (
            select sum(tbl_InvoiceDetails.PriceBeforeTax) as PriceBeforeTax,
                   taxpercentage,
                   sum(taxamount) taxamount,
                   tbl_Tax.AName
            from tbl_InvoiceDetails
            left join tbl_Tax on tbl_Tax.ID = tbl_InvoiceDetails.TaxID
            where InvoiceTypeID in (3,10)
              and          (  tbl_Tax.IsSalesTax=1 or isnull(tbl_InvoiceDetails.TaxID,0)=0 )  
              and tbl_InvoiceDetails.InvoiceDate between @date1 and @date2
            group by taxpercentage, tbl_Tax.AName
            union all
  select
sum(( tbl_FinancingDetails.TotalAmountWithInterest + tbl_FinancingDetails.DownPayment) 
/ (1+(isnull( tbl_Tax.Value,0)))) PriceBeforeTax, 
tbl_Tax.Value taxpercentage,
sum(( tbl_FinancingDetails.TotalAmountWithInterest + tbl_FinancingDetails.DownPayment) 
-( tbl_FinancingDetails.TotalAmountWithInterest / (1+(isnull( tbl_Tax.Value,0))))) Taxamount,
                   tbl_Tax.AName


            from tbl_FinancingDetails
            left join tbl_Tax on tbl_Tax.ID = tbl_FinancingDetails.TaxID
            left join tbl_FinancingHeader on tbl_FinancingHeader.Guid = tbl_FinancingDetails.HeaderGuid
            where          (  tbl_Tax.IsSalesTax=1 or isnull(tbl_FinancingDetails.TaxID,0)=0 )  
              and tbl_FinancingHeader.VoucherDate between @date1 and @date2
            group by tbl_Tax.Value, tbl_Tax.AName
        ) as q
        group by taxpercentage, AName
        order by taxpercentage";

        string SalesRefundScript = @"
        select sum(tbl_InvoiceDetails.PriceBeforeTax) as PriceBeforeTax,
               Taxpercentage,
                        sum(PriceBeforeTax*(1+ taxpercentage)) TotalAfterTax,
               sum(taxamount) Taxamount,
               tbl_Tax.AName TaxAName
        from tbl_InvoiceDetails
        left join tbl_Tax on tbl_Tax.ID = tbl_InvoiceDetails.TaxID
        where InvoiceTypeID in (4,11) and          (  tbl_Tax.IsSalesTax=1 or isnull(tbl_InvoiceDetails.TaxID,0)=0 )  
          and tbl_InvoiceDetails.InvoiceDate between @date1 and @date2
        group by taxpercentage, tbl_Tax.AName
        order by taxpercentage";

        string PurchaseScript = @"
        select sum(tbl_InvoiceDetails.PriceBeforeTax) as PriceBeforeTax,
               Taxpercentage,
                       sum(PriceBeforeTax*(1+ taxpercentage)) TotalAfterTax,
               sum(taxamount) Taxamount,
               tbl_Tax.AName TaxAName
        from tbl_InvoiceDetails
        left join tbl_Tax on tbl_Tax.ID = tbl_InvoiceDetails.TaxID
        where InvoiceTypeID in (2,22) and         (  tbl_Tax.IsPurchaseTax=1 or isnull(tbl_InvoiceDetails.TaxID,0)=0 )  
          and tbl_InvoiceDetails.InvoiceDate between @date1 and @date2
        group by taxpercentage, tbl_Tax.AName
        order by taxpercentage";

        string PurchaseRefundScript = @"
        select sum(tbl_InvoiceDetails.PriceBeforeTax) as PriceBeforeTax,
               Taxpercentage,
               sum(PriceBeforeTax)* taxpercentage TotalAfterTax,
                       sum(PriceBeforeTax*(1+ taxpercentage)) TotalAfterTax,
               tbl_Tax.AName TaxAName
        from tbl_InvoiceDetails
        left join tbl_Tax on tbl_Tax.ID = tbl_InvoiceDetails.TaxID
        where InvoiceTypeID in (7) and        (  tbl_Tax.IsPurchaseTax=1 or isnull(tbl_InvoiceDetails.TaxID,0)=0 )  
          and tbl_InvoiceDetails.InvoiceDate between @date1 and @date2
        group by taxpercentage, tbl_Tax.AName
        order by taxpercentage";
        [HttpGet]
        [Route("GetTaxReport")]
        public IActionResult GetTaxReport(int CompanyID,DateTime date1,DateTime date2)
        {
            try
            {
                SqlParameter[] prm1 =
            {
                    new SqlParameter("@date1", SqlDbType.Date) { Value = Simulate.DateString( date1 ) },
                      new SqlParameter("@date2", SqlDbType.Date) { Value = Simulate.DateString( date2 ) },
              };
                SqlParameter[] prm2 =
          {
                    new SqlParameter("@date1", SqlDbType.Date) { Value = Simulate.DateString( date1 ) },
                      new SqlParameter("@date2", SqlDbType.Date) { Value = Simulate.DateString( date2 ) },
              };
                SqlParameter[] prm3 =
          {
                    new SqlParameter("@date1", SqlDbType.Date) { Value = Simulate.DateString( date1 ) },
                      new SqlParameter("@date2", SqlDbType.Date) { Value = Simulate.DateString( date2 ) },
              };
                SqlParameter[] prm4 =
          {
                    new SqlParameter("@date1", SqlDbType.Date) { Value = Simulate.DateString( date1 ) },
                      new SqlParameter("@date2", SqlDbType.Date) { Value = Simulate.DateString( date2 ) },
              };
                clsSQL clsSQL= new clsSQL();
                
                DataTable dtSalesTaxDetails = clsSQL.ExecuteQueryStatement(SalesScript, clsSQL.CreateDataBaseConnectionString(CompanyID),prm1);
                // ========================================
                 
                DataTable dtSalesRefundTaxDetails = clsSQL.ExecuteQueryStatement(SalesRefundScript, clsSQL.CreateDataBaseConnectionString(CompanyID), prm2);
                // ========================================
                
                DataTable dtPurchaseTaxDetails = clsSQL.ExecuteQueryStatement(PurchaseScript, clsSQL.CreateDataBaseConnectionString(CompanyID), prm3);

                // ========================================
              
                DataTable dtPurchaseRefundTaxDetails = clsSQL.ExecuteQueryStatement(PurchaseRefundScript, clsSQL.CreateDataBaseConnectionString(CompanyID), prm4);



                var result = new
                {
                    sales = DataTableToList(dtSalesTaxDetails),
                    salesRefund = DataTableToList(dtSalesRefundTaxDetails),
                    purchase = DataTableToList(dtPurchaseTaxDetails),
                    purchaseRefund = DataTableToList(dtPurchaseRefundTaxDetails),
                };

                // Option A (recommended): return proper JSON response
                return Ok(result);
            }
            catch
            {
                throw;
            }
        }
      
        private static List<Dictionary<string, object>> DataTableToList(DataTable dt)
        {
            var list = new List<Dictionary<string, object>>();
            if (dt == null) return list;

            foreach (DataRow row in dt.Rows)
            {
                var dict = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);
                foreach (DataColumn col in dt.Columns)
                {
                    var v = row[col];
                    dict[col.ColumnName] = (v == DBNull.Value) ? null : v;
                }
                list.Add(dict);
            }
            return list;
        }
        [HttpGet]
        [Route("GetTaxReportExcel")]
        public FileContentResult GetTaxReportExcel(int CompanyID, DateTime date1, DateTime date2)
        {
            // ── 1. Run the same 4 queries as GetTaxReport ──────────────────────────────
            SqlParameter[] prm1 =
            {
        new SqlParameter("@date1", SqlDbType.Date) { Value = Simulate.DateString(date1) },
        new SqlParameter("@date2", SqlDbType.Date) { Value = Simulate.DateString(date2) },
    };
            SqlParameter[] prm2 =
            {
        new SqlParameter("@date1", SqlDbType.Date) { Value = Simulate.DateString(date1) },
        new SqlParameter("@date2", SqlDbType.Date) { Value = Simulate.DateString(date2) },
    };
            SqlParameter[] prm3 =
            {
        new SqlParameter("@date1", SqlDbType.Date) { Value = Simulate.DateString(date1) },
        new SqlParameter("@date2", SqlDbType.Date) { Value = Simulate.DateString(date2) },
    };
            SqlParameter[] prm4 =
            {
        new SqlParameter("@date1", SqlDbType.Date) { Value = Simulate.DateString(date1) },
        new SqlParameter("@date2", SqlDbType.Date) { Value = Simulate.DateString(date2) },
    };

            clsSQL clsSQL = new clsSQL();
            string connStr = clsSQL.CreateDataBaseConnectionString(CompanyID);

        

            DataTable dtSales = clsSQL.ExecuteQueryStatement(SalesScript, connStr, prm1);
            DataTable dtSalesRefund = clsSQL.ExecuteQueryStatement(SalesRefundScript, connStr, prm2);
            DataTable dtPurchase = clsSQL.ExecuteQueryStatement(PurchaseScript, connStr, prm3);
            DataTable dtPurchaseRefund = clsSQL.ExecuteQueryStatement(PurchaseRefundScript, connStr, prm4);

            // ── 2. Build Excel matching the attached report layout ─────────────────────
            using (XLWorkbook wb = new XLWorkbook())
            {
                var ws = wb.Worksheets.Add("ضريبة المبيعات");
                ws.RightToLeft = true;

                // ── Shared styles ──────────────────────────────────────────────────────
                XLColor colHeader = XLColor.FromHtml("#D3D3D3");   // column header grey
                XLColor colSection = XLColor.FromHtml("#BDBDBD");   // section title dark grey
                XLColor colTotal = XLColor.FromHtml("#ECECEC");   // totals row light grey
                XLColor colYellow = XLColor.FromHtml("#FFF59D");   // net / exempt rows
                XLColor colWhite = XLColor.White;

                // Column widths (A=البند … E=الإجمالي)
                ws.Column(1).Width = 34;
                ws.Column(2).Width = 18;
                ws.Column(3).Width = 14;
                ws.Column(4).Width = 18;
                ws.Column(5).Width = 18;

                int row = 1;

                // ── Local helpers ──────────────────────────────────────────────────────

                // Merge A:E, write centered bold title
                void MergeTitle(string text, XLColor bg, int fontSize = 12)
                {
                    ws.Range(row, 1, row, 5).Merge();
                    var cell = ws.Cell(row, 1);
                    cell.Value = text;
                    cell.Style.Font.Bold = true;
                    cell.Style.Font.FontSize = fontSize;
                    cell.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                    cell.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                    cell.Style.Fill.BackgroundColor = bg;
                    ws.Row(row).Height = 22;
                    row++;
                }

                // Write the 5-column header row
                void TableHeader()
                {
                    string[] headers = { "البند", "قيمة قبل الضريبة", "نسبة الضريبة", "قيمة الضريبة", "الإجمالي" };
                    for (int c = 0; c < 5; c++)
                    {
                        var cell = ws.Cell(row, c + 1);
                        cell.Value = headers[c];
                        cell.Style.Font.Bold = true;
                        cell.Style.Fill.BackgroundColor = colHeader;
                        cell.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                        cell.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                    }
                    row++;
                }

                // Write a data row (label | before | % | tax | total)
           
                void DataRow(string label, decimal before, decimal perc, decimal tax,
                             XLColor bg = null, bool bold = false)
                {
                    decimal total = before + tax;
                    string percText = (perc <= 1 && perc > 0)
                        ? $"{(perc * 100):0}%"
                        : (perc == 0 ? "0%" : $"{perc:0}%");

                    for (int c = 0; c < 5; c++)
                    {
                        var cell = ws.Cell(row, c + 1);

                        // ── Assign value with explicit type (fixes CS0266) ─────────────────
                        switch (c)
                        {
                            case 0: cell.Value = (XLCellValue)label; break;
                            case 1: cell.Value = (XLCellValue)(double)before; break;
                            case 2: cell.Value = (XLCellValue)percText; break;
                            case 3: cell.Value = (XLCellValue)(double)tax; break;
                            case 4: cell.Value = (XLCellValue)(double)total; break;
                        }

                        cell.Style.Font.Bold = bold;
                        if (bg != null) cell.Style.Fill.BackgroundColor = bg;
                        cell.Style.Alignment.Horizontal = c == 0
                            ? XLAlignmentHorizontalValues.Right
                            : XLAlignmentHorizontalValues.Center;
                        cell.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;

                        if (c == 1 || c == 3 || c == 4)
                            cell.Style.NumberFormat.Format = "#,##0.000";
                    }
                    row++;
                }

                // Write a totals row (merges nothing, just styled differently)
                void TotalRow(string label, decimal before, decimal tax,
                              XLColor bg = null)
                {
                    DataRow(label, before, 0, tax, bg ?? colTotal, bold: true);
                    // blank out the % cell
                    ws.Cell(row - 1, 3).Value = "";
                }

                // Write all lines from a DataTable
                void WriteTaxLines(DataTable dt, string defaultLabel, bool negative = false)
                {
                    if (dt.Rows.Count == 0)
                    {
                        DataRow(defaultLabel, 0, 0, 0);
                        return;
                    }
                    foreach (DataRow dr in dt.Rows)
                    {
                        decimal sign = negative ? -1m : 1m;
                        decimal before = Simulate.decimal_(dr["PriceBeforeTax"]) * sign;
                        decimal tax = Simulate.decimal_(dr["Taxamount"]) * sign;
                        decimal perc = Simulate.decimal_(dr["Taxpercentage"]);
                        string label = Simulate.String(dr["TaxAName"]);
                        if (string.IsNullOrWhiteSpace(label)) label = defaultLabel;
                        DataRow(label, before, perc, tax);
                    }
                }

                decimal SumBefore(DataTable dt) =>
                    dt.Rows.Cast<DataRow>().Sum(r => Simulate.decimal_(r["PriceBeforeTax"]));
                decimal SumTax(DataTable dt) =>
                    dt.Rows.Cast<DataRow>().Sum(r => Simulate.decimal_(r["Taxamount"]));

                // ── 3. Report content ──────────────────────────────────────────────────

                // Company name
                clsCompany clsCompany = new clsCompany();
                DataTable dt = clsCompany.SelectCompany(CompanyID, "", "", "", CompanyID, "", false);
                if (dt != null && dt.Rows.Count > 0) {
                    MergeTitle(Simulate.String( dt.Rows[0]["AName"]), colWhite, 13);
                
                }

                // Report period title
                MergeTitle(
                    $"ضريبة المبيعات  {Simulate.DateString(date1)} + {Simulate.DateString(date2)}",
                    colHeader, 12);

                row++; // spacer

                // ── PURCHASES ─────────────────────────────────────────────────────────
                MergeTitle("المشتريات", colSection);
                TableHeader();

                WriteTaxLines(dtPurchase, "مشتريات اخرى");

                // Exempt purchases (taxpercentage = 0)
                decimal exemptPurchBefore = dtPurchase.Rows.Cast<DataRow>()
                    .Where(r => Simulate.decimal_(r["Taxpercentage"]) == 0)
                    .Sum(r => Simulate.decimal_(r["PriceBeforeTax"]));
                //DataRow("مشتريات معفاة", exemptPurchBefore, 0, 0);
                //DataRow("مشتريات خاضعة بنسبة 0%", 0, 0, 0);
                //DataRow("مشتريات خاضعة بنسبة 10%", 0, 0.1m, 0);
                //DataRow("مصاريف خاضعة", 0, 0.16m, 0);

                decimal purchBefore = SumBefore(dtPurchase) - SumBefore(dtPurchaseRefund);
                decimal purchTax = SumTax(dtPurchase) - SumTax(dtPurchaseRefund);
                TotalRow("مجموع ضريبة المشتريات", purchBefore, purchTax);

                row++; // spacer

                // ── SALES ─────────────────────────────────────────────────────────────
                MergeTitle("المبيعات", colSection);
                TableHeader();

                WriteTaxLines(dtSales, "مبيعات اخرى");
                //DataRow("", 0, 0.1m, 0);   // placeholder 10% row
                //DataRow("", 0, 0.16m, 0);  // placeholder 16% row

                decimal salesBefore = SumBefore(dtSales) - SumBefore(dtSalesRefund);
                decimal salesTax = SumTax(dtSales) - SumTax(dtSalesRefund);
                TotalRow("مجموع ضريبة المبيعات", salesBefore, salesTax);

                row++; // spacer

                // ── SALES REFUNDS ─────────────────────────────────────────────────────
                MergeTitle("مردودات مبيعات", colSection);
                TableHeader();

                WriteTaxLines(dtSalesRefund, "مردود مبيعات", negative: true);
                TotalRow("مجموع ضريبة مردود مبيعات", -SumBefore(dtSalesRefund), -SumTax(dtSalesRefund));

                row++; // spacer

                // ── EXEMPT / ZERO-RATE SALES (yellow) ────────────────────────────────
                decimal exemptSalesBefore = dtSales.Rows.Cast<DataRow>()
                    .Where(r => Simulate.decimal_(r["Taxpercentage"]) == 0)
                    .Sum(r => Simulate.decimal_(r["PriceBeforeTax"]));

                var rExempt = ws.Row(row);
                ws.Cell(row, 1).Value = "مبيعات معفاة";
                ws.Cell(row, 2).Value = exemptSalesBefore;
                ws.Cell(row, 3).Value = "0%";
                ws.Cell(row, 5).Value = exemptSalesBefore;
                foreach (var c in ws.Range(row, 1, row, 5).Cells())
                {
                    c.Style.Fill.BackgroundColor = colYellow;
                    c.Style.Font.Bold = true;
                    c.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                    c.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                    if (c.Address.ColumnNumber == 2 || c.Address.ColumnNumber == 5)
                        c.Style.NumberFormat.Format = "#,##0.000";
                }
                row++;

                ws.Cell(row, 1).Value = "مبيعات خاضعة للنسبة الصفر";
                ws.Cell(row, 2).Value = 0m;
                ws.Cell(row, 3).Value = "0%";
                ws.Cell(row, 5).Value = 0m;
                foreach (var c in ws.Range(row, 1, row, 5).Cells())
                {
                    c.Style.Fill.BackgroundColor = colYellow;
                    c.Style.Font.Bold = true;
                    c.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                    c.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                }
                row++;

                // Sub-total row for exempt
                TotalRow("", exemptSalesBefore, 0, colWhite);

                row++; // spacer

                // ── NET ROW ───────────────────────────────────────────────────────────
                decimal net = salesTax - purchTax;

                ws.Range(row, 1, row, 4).Merge();
               // ws.Cell(row, 1).Value = "";
                ws.Cell(row, 1).Value = "ضريبة غير قابلة للخصم";

                ws.Cell(row, 1).Style.Font.Bold = true;
                ws.Cell(row, 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
                ws.Cell(row, 5).Value = 0;// purchBefore + SumBefore(dtPurchase);
                ws.Cell(row, 5).Style.NumberFormat.Format = "#,##0.000";
                ws.Cell(row, 5).Style.Font.Bold = true;
                foreach (var c in ws.Range(row, 1, row, 5).Cells())
                    c.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                row++;

                // رصيد امانات ضريبة المبيعات
                ws.Range(row, 1, row, 2).Merge();
                ws.Cell(row, 1).Value = $"رصيد امانات ضريبة المبيعات {Simulate.DateString(date2)}";
                ws.Cell(row, 1).Style.Font.Bold = true;
                ws.Cell(row, 3).Value = "دائن";
                ws.Cell(row, 3).Style.Font.Bold = true;
                ws.Cell(row, 3).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                ws.Cell(row, 4).Value = net;
                ws.Cell(row, 4).Style.NumberFormat.Format = "#,##0.000";
                ws.Cell(row, 4).Style.Font.Bold = true;
                foreach (var c in ws.Range(row, 1, row, 5).Cells())
                    c.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;

                // ── 4. Stream back to client ──────────────────────────────────────────
                using (MemoryStream stream = new MemoryStream())
                {
                    wb.SaveAs(stream);
                    return File(
                        stream.ToArray(),
                        "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                        $"TaxReport_{Simulate.DateString(date1)}_{Simulate.DateString(date2)}.xlsx"
                    );
                }
            }
        }
    }
}
