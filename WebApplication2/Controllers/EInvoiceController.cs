using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using WebApplication2.cls;
using WebApplication2.Models;

namespace WebApplication2.Controllers
{
    [Route("api/EInvoice")]
    [ApiController]
    public class EInvoiceController : Controller
    {
        [HttpPost]
        [Route("SubmitEInvoice")]
        public IActionResult SubmitEInvoice(int CompanyID,string InvoiceGuid,string FinancingGuid,string ReturnInvoiceNumber)
        {
            SqlTransaction trn;
            clsSQL clsSQL = new clsSQL();
            SqlConnection con = new SqlConnection(clsSQL.CreateDataBaseConnectionString(CompanyID));
            con.Open();
            trn = con.BeginTransaction();
            try
            {
              
                bool IsSaved = true;

                clsEInvoiceConfigurations eInvoiceConfigurations = new clsEInvoiceConfigurations();
                DataTable dtEinvoiceConfiguration = eInvoiceConfigurations.SelectEInvoiceConfigurations(0, "", "", CompanyID, trn);
                if (Simulate.String(InvoiceGuid) != "") {

                    clsInvoiceHeader invoiceHeader = new clsInvoiceHeader();
                    DataTable dtHeader = invoiceHeader.SelectInvoiceHeaderByGuid(InvoiceGuid, Simulate.StringToDate("1900-01-01"),
                         Simulate.StringToDate("2900-01-01"), 0, 0, 0, CompanyID, trn);
                    DataTable dtDetails = new DataTable();
                    if (dtHeader != null && dtHeader.Rows.Count > 0) {
                        clsInvoiceDetails invoiceDetails = new clsInvoiceDetails();
                        dtDetails = invoiceDetails.SelectInvoiceDetailsByHeaderGuid(InvoiceGuid, "", CompanyID, trn);
                    }

                    clsBusinessPartner clsBusinessPartner = new clsBusinessPartner();
                    clsCurrency clsCurrency = new clsCurrency();

                    DataTable dtCustomer = clsBusinessPartner.SelectBusinessPartner(Simulate.Integer32(dtHeader.Rows[0]["BusinessPartnerID"])
                        , 0, "", "", "", "", -1, CompanyID, trn);
                    DataTable dtCurrency = clsCurrency.SelectCurrency(Simulate.Integer32(dtHeader.Rows[0]["CurrencyID"]), "", "", CompanyID, trn);
                    List<InvoiceLine> details = new List<InvoiceLine>();
                    clsTax clsTax = new clsTax();
                    for (global::System.Int32 i = 0; i < dtDetails.Rows.Count; i++)
                    {
                        DataTable dtTax = clsTax.SelectTaxByID(Simulate.Integer32(dtDetails.Rows[i]["TaxID"]),
                    "", "", CompanyID, -1, -1, -1, -1, trn);
                        details.Add(

                        clsEInvoiceXMLCreator.createLine(Simulate.String(dtCurrency.Rows[0]["Code"]),//"JOD"
                         (i + 1) //index
                        , Simulate.decimal_(dtCurrency.Rows[0]["qty"])//Quantity
                        , Simulate.decimal_(dtCurrency.Rows[0]["PriceBeforeTax"])//Price before Tax
                        , Simulate.decimal_(dtCurrency.Rows[0]["TaxAmount"])//TaxAmount
                        , Simulate.decimal_(dtCurrency.Rows[0]["TotalLine"])//PriceAfterTax
                         , Simulate.decimal_(dtTax.Rows[0]["Value"]) == 0 ? ItemTaxCategory.Zero : ItemTaxCategory.Taxable
                        , (Simulate.decimal_(dtCurrency.Rows[0]["TaxPercentage"]) * 100)//Taxpercenage  
                        , Simulate.String(dtCurrency.Rows[0]["ItemName"]) // itemName
                        , Simulate.decimal_(dtCurrency.Rows[0]["DiscountAfterTaxAmountAll"])//Total Discount
                        ));

                    }
                    InvoiceTypeCode invoiceTypeCode = InvoiceTypeCode.NewSalesInvoice;
                    if ((Simulate.Integer32(dtHeader.Rows[0]["InvoiceTypeID"]) == 10 || Simulate.Integer32(dtHeader.Rows[0]["InvoiceTypeID"]) == 3))
                    {
                        if (Simulate.String(dtHeader.Rows[0]["EInvoiceQRCode"]) != "")
                        {
                            trn.Rollback();
                            con.Close();
                            return Ok(new { ID = "", Message = "Invoice Posted Before" });


                        }
                        invoiceTypeCode = InvoiceTypeCode.NewSalesInvoice;
                    }
                    else if (Simulate.Integer32(dtHeader.Rows[0]["InvoiceTypeID"]) == 11 || Simulate.Integer32(dtHeader.Rows[0]["InvoiceTypeID"]) == 4)
                    {
                        invoiceTypeCode = InvoiceTypeCode.NewReturnSalesInvoice;



                    }
                    else {
                        trn.Rollback();
                        con.Close();
                        return Ok(new { ID = "", Message = "Invoice Type Not Supported." });
                    }

                    var qr = clsEInvoiceXMLCreator.postInvoice(Simulate.String(dtEinvoiceConfiguration.Rows[0]["UserCode"])//clientID"6106d7cc-4406-4412-ae85-c6d489c65ed1"
                       , Simulate.String(dtEinvoiceConfiguration.Rows[0]["SecretKey"])//secretKey,//  "Gj5nS9wyYHRadaVffz5VKB4v4wlVWyPhcJvrTD4NHtNPzhMz/WHtFrcmuPPkid0kQ/fbCxoJCG9UTGUfPfuVuWe6ABxt+2i/pSn2R0iObbf9lEaHWsPQ9xfQApuCRtJC6yFf/9naWNpWFzCGOvvekdRYkTq0eXm+3yeBpJ3RSVFc5uNDMq3D9FHFjEfizY4oHMTqAyq0+7T3W2XXcxR//Bg51hXQIuk8cuptI2lWNCwlfe482Vqg3rVCgJ9tjbm3iY6cYIYxDiPRvdUr2LVY1Q=="
                          , Simulate.String(dtHeader.Rows[0]["invoiceNO"]),//"0043a15e-740b-4e1b-889d-504afdb1d1d",// InvoiceNo
                         InvoiceGuid,//  "0043a15e-740b-4e1b-889d-504afdb1d1d",//InvoiceGuid
                       Simulate.StringToDate(dtHeader.Rows[0]["InvoiceDate"]), //InvoiceDate
                        Simulate.String(dtCurrency.Rows[0]["Code"]),//"JOD",//Currency
                         "JO",//Country
                          Simulate.String(dtEinvoiceConfiguration.Rows[0]["TaxNumber"]),// "12764205",//SupplierTaxID
                         Simulate.String(dtCustomer.Rows[0]["AName"]),// "الشركه الفنيه لبع المستلزمات",//CustomerName
                         Simulate.String(dtHeader.Rows[0]["BusinessPartnerID"]),//CustomerID
                         BuyerIdentificationType.NationalID,//CustomerIDType 
                         "",//CustomerPostalZone
                         JordanGovernorate.Amman, //CustomerCity
                          Simulate.String(dtCustomer.Rows[0]["AName"]),//CustomerName
                          Simulate.String(dtCustomer.Rows[0]["Tel"]),//CustomerPhone
                          Simulate.String(dtEinvoiceConfiguration.Rows[0]["ActivityNumber"])//     "12787027"//OurCompanyIncomeTaxID
                        , Simulate.decimal_(dtHeader.Rows[0]["TotalDiscount"])//discountamount
                        , (Simulate.decimal_(dtHeader.Rows[0]["TotalInvoice"]) +
                          Simulate.decimal_(dtHeader.Rows[0]["TotalDiscount"]) -
                          Simulate.decimal_(dtHeader.Rows[0]["TotalTax"])) //TotalbeforeTaxa
                        , Simulate.decimal_(dtHeader.Rows[0]["TotalInvoice"])// TotalAfterTax
                        , Simulate.decimal_(dtHeader.Rows[0]["TotalTax"])//TaxAmount
                        , Simulate.decimal_(dtHeader.Rows[0]["TotalInvoice"])//NetTotal  
                        , details, invoiceTypeCode,
              Simulate.String(dtHeader.Rows[0]["invoiceNO"]), InvoiceGuid, (Simulate.decimal_(dtHeader.Rows[0]["TotalInvoice"])));

                    updatetblInvoice(qr, InvoiceGuid, trn);


                }
                else if (Simulate.String(FinancingGuid) != "") {
                    clsFinancingHeader clsFinancingHeader = new clsFinancingHeader();
                    DataTable dtHeader = clsFinancingHeader.SelectFinancingHeaderByGuid(FinancingGuid,
                        Simulate.StringToDate("1900-01-01"), Simulate.StringToDate("2900-01-01"), 0, 0, CompanyID, 0, "-1", 0, trn);
                    DataTable dtDetails = new DataTable();
                    if (dtHeader != null && dtHeader.Rows.Count > 0 && Simulate.Integer32(dtHeader.Rows[0]["loantype"]) == 1)
                    {
                        clsFinancingDetails clsFinancingDetails = new clsFinancingDetails();
                        dtDetails = clsFinancingDetails.SelectFinancingDetailsByHeaderGuid(FinancingGuid, 0, CompanyID, trn);
                    }
                    clsBusinessPartner clsBusinessPartner = new clsBusinessPartner();
                    clsCurrency clsCurrency = new clsCurrency();

                    DataTable dtCustomer = clsBusinessPartner.SelectBusinessPartner(Simulate.Integer32(dtHeader.Rows[0]["BusinessPartnerID"])
                        , 0, "", "", "", "", -1, CompanyID, trn);
                    DataTable dtVendor = clsBusinessPartner.SelectBusinessPartner(Simulate.Integer32(dtHeader.Rows[0]["VendorID"])
                  , 0, "", "", "", "", -1, CompanyID, trn);
                    DataTable dtCurrency = clsCurrency.SelectCurrency(1, "", "", CompanyID, trn);


                    List<InvoiceLine> details = new List<InvoiceLine>();
                    decimal TotalAmount = 0;
                    decimal totalTax = 0;
                    decimal totalBeforeTax = 0;
                    clsTax clsTax = new clsTax();

                    //if (dtCurrency != null && dtCurrency.Rows.Count > 0) {
                    //    for (global::System.Int32 i = 0; i < dtDetails.Rows.Count; i++)
                    //    {
                    //        DataTable dtTax = clsTax.SelectTaxByID(Simulate.Integer32(dtDetails.Rows[i]["TaxID"]),
                    //"", "", CompanyID, -1, -1, -1, -1, trn);
                    //        decimal linePriceBeforeTax = Simulate.decimal_(dtDetails.Rows[i]["PriceBeforeTax"]);
                    //        decimal lintTax = linePriceBeforeTax * (Simulate.decimal_(dtTax.Rows[0]["Value"]) * 100);
                    //        lintTax = Simulate.Integer32(lintTax);// / 1000;
                    //        lintTax = lintTax / 100;
                    //        decimal lineTotal = lintTax + linePriceBeforeTax;
                    //        TotalAmount = TotalAmount + lineTotal;
                    //        totalTax = totalTax + lintTax;
                    //        totalBeforeTax = totalBeforeTax + linePriceBeforeTax;
                    //        details.Add(

                    //        clsEInvoiceXMLCreator.createLine(Simulate.String(dtCurrency.Rows[0]["Code"]),//"JOD"
                    //         (i + 1) //index
                    //        , Simulate.decimal_(1)//Quantity
                    //        , Simulate.decimal_(linePriceBeforeTax)//Price before Tax
                    //        , Simulate.decimal_(lintTax)//TaxAmount
                    //        , Simulate.decimal_(lineTotal)//PriceAfterTax
                    //        , ItemTaxCategory.Taxable
                    //        , 16m///(Simulate.decimal_(dtTax.Rows[0]["Value"]) * 100)//Taxpercenage  
                    //        , Simulate.String(dtDetails.Rows[i]["Description"]) // itemName
                    //        , Simulate.decimal_(0)//Total Discount
                    //        ));

                    //    }
                    //}
                    ////////////////Test

                    clsEInvoiceXMLCreator cls = new clsEInvoiceXMLCreator();
                    List<InvoiceLine> details1 = new List<InvoiceLine>();
                
                    for (global::System.Int32 i = 0; i < dtDetails.Rows.Count; i++)
                    {
                        DataTable dtTax = clsTax.SelectTaxByID(Simulate.Integer32(dtDetails.Rows[i]["TaxID"]),
                    "", "", CompanyID, -1, -1, -1, -1, trn);

                        //  decimal linePriceBeforeTax = Simulate.decimal_(Simulate.decimal_(dtDetails.Rows[i]["PriceBeforeTax"]));
                        decimal lineTotalAmountWithInterest = Simulate.decimal_(Simulate.decimal_(dtDetails.Rows[i]["TotalAmountWithInterest"]));
                        decimal lineTotalAmountWithInterestWithOutTax = lineTotalAmountWithInterest/ (1+ (Simulate.decimal_(dtTax.Rows[0]["Value"])));

                        //   decimal lintTax = linePriceBeforeTax * (Simulate.decimal_(dtTax.Rows[0]["Value"]) * 100);
                        //  decimal tax = 
                        decimal lintTax = lineTotalAmountWithInterestWithOutTax * (Simulate.decimal_(dtTax.Rows[0]["Value"]) );
                        //     lintTax = Simulate.Integer32(lintTax);// / 1000;
                        
                       // decimal lineTotal = lineTotalAmountWithInterest;
                        TotalAmount = TotalAmount + lineTotalAmountWithInterest;
                        totalTax = totalTax + lintTax;
                        totalBeforeTax = totalBeforeTax + lineTotalAmountWithInterestWithOutTax;



                        details1.Add(
                       clsEInvoiceXMLCreator.createLine(Simulate.String(dtCurrency.Rows[0]["Code"]),//"JOD"
                             (i + 1) //index
                            , Simulate.decimal_(1)//Quantity
                            , Simulate.decimal_(lineTotalAmountWithInterestWithOutTax)//Price before Tax
                            , Simulate.decimal_(lintTax)//TaxAmount
                            , Simulate.decimal_(lineTotalAmountWithInterest)//PriceAfterTax
                        , Simulate.decimal_(dtTax.Rows[0]["Value"]) ==0 ? ItemTaxCategory.Zero :  ItemTaxCategory.Taxable
                        , (Simulate.decimal_(dtTax.Rows[0]["Value"]) * 100)//Taxpercenage  16//Taxpercenage
                        , Simulate.String(dtDetails.Rows[i]["Description"]) // itemName
                        , 0//Total Discount
                        )); 
                
                }


                    ////////////////
                    string voucherNumber = Simulate.String(dtHeader.Rows[0]["VoucherNumber"]);
                    string InvGuid = FinancingGuid;
                    InvoiceTypeCode invoiceTypeCode = InvoiceTypeCode.NewSalesInvoice;
                    if ((Simulate.String(ReturnInvoiceNumber)==""))
                    {
                        if (Simulate.String(dtHeader.Rows[0]["EInvoiceQRCode"]) != "")
                        {
                            trn.Rollback();
                            con.Close();
                            return Ok(new { ID = "", Message = "Invoice Posted Before" });
                        }
                        invoiceTypeCode = InvoiceTypeCode.NewSalesInvoice;
                         
                    }
                    else
                    {
                        InvGuid = Simulate.String(Guid.NewGuid());
                        voucherNumber = "RInv#"+voucherNumber;
                        invoiceTypeCode = InvoiceTypeCode.NewReturnSalesInvoice;
                    }
                 
                    var qr = "";
                    if (details1.Count > 0) { 
                    qr=clsEInvoiceXMLCreator.postInvoice(
                         Simulate.String(dtEinvoiceConfiguration.Rows[0]["UserCode"])//clientID"6106d7cc-4406-4412-ae85-c6d489c65ed1"
                     , Simulate.String(dtEinvoiceConfiguration.Rows[0]["SecretKey"])//secretKey,//  "Gj5nS9wyYHRadaVffz5VKB4v4wlVWyPhcJvrTD4NHtNPzhMz/WHtFrcmuPPkid0kQ/fbCxoJCG9UTGUfPfuVuWe6ABxt+2i/pSn2R0iObbf9lEaHWsPQ9xfQApuCRtJC6yFf/9naWNpWFzCGOvvekdRYkTq0eXm+3yeBpJ3RSVFc5uNDMq3D9FHFjEfizY4oHMTqAyq0+7T3W2XXcxR//Bg51hXQIuk8cuptI2lWNCwlfe482Vqg3rVCgJ9tjbm3iY6cYIYxDiPRvdUr2LVY1Q=="
                        ,voucherNumber,//"0043a15e-740b-4e1b-889d-504afdb1d1d",// InvoiceNo
                       InvGuid,//  "0043a15e-740b-4e1b-889d-504afdb1d1d",//InvoiceGuid
                     Simulate.StringToDate(dtHeader.Rows[0]["VoucherDate"]), //InvoiceDate
                      Simulate.String(dtCurrency.Rows[0]["Code"]),//"JOD",//Currency
                       "JO",//Country
                       Simulate.String(dtEinvoiceConfiguration.Rows[0]["TaxNumber"]),// "12764205",//SupplierTaxID 12764205
                       Simulate.String(dtVendor.Rows[0]["AName"]),// "الشركه الفنيه لبع المستلزمات",//CustomerName
                       Simulate.String(dtCustomer.Rows[0]["NationalNumber"]), // Simulate.String(dtHeader.Rows[0]["BusinessPartnerID"]),//CustomerID
                       BuyerIdentificationType.NationalID,//CustomerIDType 
                       "",//CustomerPostalZone
                       JordanGovernorate.Amman, //CustomerCity
                        Simulate.String(dtCustomer.Rows[0]["AName"]),//CustomerName
                        Simulate.String(dtCustomer.Rows[0]["Tel"]),//CustomerPhone
                        Simulate.String(dtEinvoiceConfiguration.Rows[0]["ActivityNumber"])//     "12787027"//OurCompanyIncomeTaxID
               //       , 0//discountamount
                         //, totalBeforeTax //TotalbeforeTaxa
                         //, TotalAmount// TotalAfterTax
                         //, totalTax//TaxAmount
                         //, TotalAmount//NetTotal  
                         //, details, 
                         , Simulate.decimal_(0) //discountamount
                , totalBeforeTax//Simulate.decimal_(1.0) //TotalbeforeTaxa
                ,TotalAmount //Simulate.decimal_(1.16) // TotalAfterTax
                , totalTax//Simulate.decimal_(0.16)//TaxAmount
                , TotalAmount//Simulate.decimal_(1.16)//NetTotal

                      , details1,
                       invoiceTypeCode,
                        Simulate.String(dtHeader.Rows[0]["VoucherNumber"]),
                        FinancingGuid
                        , TotalAmount);
                        if ((Simulate.String(ReturnInvoiceNumber) == ""))
                        {
                            updatetblFinancing(qr, FinancingGuid, trn);
                            if (qr != "")
                            {
                                IsSaved = true;
                            }
                        }
                    }

                
                   
                }

                if (IsSaved)
                {
                    trn.Commit();
                    con.Close();
                }
                else
                { trn.Rollback();
                    con.Close(); }

                return Ok(new { ID = "", Message = "Scale inserted successfully." });
            }
            catch (Exception ex)
            {
                trn.Rollback(); con.Close();
                return BadRequest(ex.Message);
            }
        }
        bool updatetblInvoice(string qr, string invoiceGuid,SqlTransaction trn) {

            string a = "update tbl_InvoiceHeader set EInvoiceQRCode= '' where guid = ''";
            clsSQL clsSQL = new clsSQL();
         var A = clsSQL.ExecuteNonQueryStatement(a, clsSQL.CreateDataBaseConnectionString(0),null, trn);
            if (A == 0)
            {
                return false;
            }
            else
            {

                return true;
            }
        }
        bool updatetblFinancing(string qr, string FinancingGuid, SqlTransaction trn)
        {

            string a = "update tbl_FinancingHeader set EInvoiceQRCode= '"+ qr + "' where guid = '"+ FinancingGuid + "'";
            clsSQL clsSQL = new clsSQL();
            var A = clsSQL.ExecuteNonQueryStatement(a, clsSQL.CreateDataBaseConnectionString(0), null, trn);
            if (A == 0)
            {
                return false;
            }
            else
            {

                return true;
            }
        }
    }
}
