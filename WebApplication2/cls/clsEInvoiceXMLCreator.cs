using Newtonsoft.Json.Linq;
using RestSharp;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using System.Xml;
using System;

namespace WebApplication2.cls
{
    public class clsEInvoiceXMLCreator
    {

        // Define the namespaces used in the invoice
        private const string CBC_NAMESPACE = "urn:oasis:names:specification:ubl:schema:xsd:CommonBasicComponents-2";
        private const string CAC_NAMESPACE = "urn:oasis:names:specification:ubl:schema:xsd:CommonAggregateComponents-2";
        private const string UBL_NAMESPACE = "urn:oasis:names:specification:ubl:schema:xsd:Invoice-2";
        public static string postInvoice(string ClientId, string SecretKey,
           string InvoiceNumber, string InvoiceGuid, DateTime InvoiceDate,
             string CurrencyCode, string CountryCode, string SupplierTaxID, string SupplierName
             , string CustomerID, BuyerIdentificationType buyerIdentificationType
             , string CustomerPostalZone, JordanGovernorate jordanGovernorate, string CustomerName,
             string CustomerTel, string OurCompanyIncomeCode, Decimal DiscountAmount, Decimal TotalBeforeTax
             , Decimal TotalAfterTax, Decimal TaxAmount, Decimal NetTotal, List<InvoiceLine> details, InvoiceTypeCode invoiceTypeCode,
            string originalInvoiceId, string originalInvoiceUuid, decimal originalInvoiceTotal)
        {
            var client = new RestClient("https://backend.jofotara.gov.jo/core/invoices/");
            // client.Timeout = -1;
            var request = new RestRequest("", Method.Post);
            request.AddHeader("Client-Id", ClientId);
            request.AddHeader("Secret-Key", SecretKey);
            request.AddHeader("Content-Type", "application/json");
            request.AddHeader("Cookie", "stickounet=4fdb7136e666916d0e373058e9e5c44e|7480c8b0e4ce7933ee164081a50488f1");


            var body = @"{
    ""invoice"": """ + buildXMLbase64(
                          InvoiceNumber, InvoiceGuid, InvoiceDate,
              CurrencyCode, CountryCode, SupplierTaxID, SupplierName
            , CustomerID, buyerIdentificationType
            , CustomerPostalZone, jordanGovernorate, CustomerName,
              CustomerTel, OurCompanyIncomeCode, DiscountAmount, TotalBeforeTax
            , TotalAfterTax, TaxAmount, NetTotal, details, invoiceTypeCode,
              originalInvoiceId,   originalInvoiceUuid,    originalInvoiceTotal

    ) + @"""
}";

            request.AddParameter("application/json", body, ParameterType.RequestBody);
            var response = client.Execute(request);
            // Console.WriteLine(response.Content);
            //  richTextBox3.Text = response.Content;
            // Parse the JSON response


            var jsonResponse = JObject.Parse(response.Content);
            // Extract the EINV_QR field
         //   var errorMessage = jsonResponse["EINV_RESULTS"]?["ERRORS"]?[0]?["EINV_CODE"]?.ToString();

            string qrCodeData = jsonResponse["EINV_QR"]?.ToString();
            return qrCodeData;
        }
        static string buildXMLbase64(string InvoiceNumber, string InvoiceGuid, DateTime InvoiceDate,
                string CurrencyCode, string CountryCode, string SupplierTaxID, string SupplierName
                , string CustomerID, BuyerIdentificationType buyerIdentificationType
                , string CustomerPostalZone, JordanGovernorate jordanGovernorate, string CustomerName,
                string CustomerTel, string OurCompanyIncomeCode, Decimal DiscountAmount, Decimal TotalBeforeTax
                , Decimal TotalAfterTax, Decimal TaxAmount, Decimal NetTotal, List<InvoiceLine> details, InvoiceTypeCode invoiceTypeCode
            ,
            string originalInvoiceId, string originalInvoiceUuid, decimal originalInvoiceTotal)
        {



            string xmlContent = BuildXML(InvoiceNumber, InvoiceGuid,



              InvoiceDate,
                invoiceTypeCode,
                 InvoiceTypeName.CashSalesInvoice,
                 "",
              CurrencyCode,
             CountryCode, SupplierTaxID, SupplierName
            , CustomerID, buyerIdentificationType
            , CustomerPostalZone, jordanGovernorate, CustomerName,
                CustomerTel, OurCompanyIncomeCode, DiscountAmount, TotalBeforeTax
            , TotalAfterTax, TaxAmount, NetTotal

                 , details,
              originalInvoiceId,   originalInvoiceUuid,    originalInvoiceTotal
                );

            // Convert XML string to byte array using UTF-8 encoding
            byte[] xmlBytes = Encoding.UTF8.GetBytes(xmlContent);

            // Convert byte array to Base64 string
            string base64String = Convert.ToBase64String(xmlBytes);

            // Display the Base64 string in another TextBox or use it as needed

            return base64String;
        }
        public static InvoiceLine createLine(string CurrencyCode, int index, decimal Quantity, decimal PriceBeforeTaxPcs
                , decimal TaxAmount, decimal PriceAfterTax, ItemTaxCategory itemTaxCategory, decimal taxPercentage
                , string ItemName, decimal totalDiscount)
        {

            var a = new InvoiceLine
            {
                Id = index,//1,
                Quantity = Quantity,//1.00m,
                UnitCode = "PCE",
                LineExtensionAmount = (PriceBeforeTaxPcs * Quantity) - totalDiscount,// 1.00m,
                TaxAmount = TaxAmount,//0.16m,
                RoundingAmount = PriceAfterTax,// 1.16m,
                TaxCategoryId = GetItemTaxCategory(itemTaxCategory),//"S",
                TaxPercent = taxPercentage,//16.00m,
                ItemName = ItemName,//"AVC",
                PriceAmount = PriceBeforeTaxPcs,//1.00m,//Unit Price 
                AllowanceCharge = new AllowanceChargeInfo
                {
                    IsCharge = false,
                    Reason = "DISCOUNT",
                    Amount = totalDiscount,// 0.00m,
                    CurrencyCode = CurrencyCode,//,"JO"
                }
            };
            return a;
        }
        static String BuildXML(string InvoiceNumber, string InvoiceGuid, DateTime InvoiceDate,
            InvoiceTypeCode invoiceTypeCode, InvoiceTypeName invoiceTypeName, string Note
            , string CurrencyCode, string CountryCode, string SupplierTaxID, string SupplierName
            , string CustomerID, BuyerIdentificationType buyerIdentificationType
            , string CustomerPostalZone, JordanGovernorate jordanGovernorate, string CustomerName,
            string CustomerTel, string OurCompanyIncomeCode, Decimal DiscountAmount, Decimal TotalBeforeTax
            , Decimal TotalAfterTax, Decimal TaxAmount, Decimal NetTotal, List<InvoiceLine> details
         ,
            string originalInvoiceId, string originalInvoiceUuid, decimal originalInvoiceTotal)
        {




            var header = new InvoiceHeader
            {
                Id = InvoiceNumber,//"EIN0006",
                Uuid = InvoiceGuid,//"0043a15e-740b-4e1b-889d-504afdb1d1d",
                IssueDate = InvoiceDate,// new DateTime(2023, 11, 20),
                InvoiceTypeCode = GetInvoiceTypeCodeString(invoiceTypeCode),//"388",
                InvoiceTypeName = GetInvoiceTypeNameString(invoiceTypeName),//"012",
                Note = Note,//"ملاحظات",
                
                CurrencyCode = CurrencyCode,//"JOD",
                DocumentCurrencyCode = CurrencyCode,//"JOD",
                TaxCurrencyCode = CurrencyCode,//"JOD",
                AdditionalDocRefId = "ICV",
                AdditionalDocRefUuid = InvoiceGuid,// "1",
                SupplierInfo = new SupplierInfo
                {
                    CountryCode = CountryCode,//"JO",
                    TaxId = SupplierTaxID,// "12764205", //12787027  -- 12764205
                    RegistrationName = SupplierName,//"الشركه الفنيه لبع المستلزمات"
                },
                CustomerInfo = new CustomerInfo
                {
                    Id = CustomerID,//"33445544",
                    SchemeId = GetBuyerIdentificationCode(buyerIdentificationType),//"TN",
                    PostalZone = CustomerPostalZone,//"33554",
                    CountrySubentityCode = GetGovernorateCode(jordanGovernorate),//"JO-AZ",
                    CountryCode = CountryCode,//"JO",
                    TaxId = CustomerID,// "12764205",
                    RegistrationName = CustomerName,//"احمد محمود",
                    Telephone = CustomerTel,//"324323434"
                },
                SellerInfo = new SellerInfo
                {
                    Id = OurCompanyIncomeCode,//"12787027" 
                    
                },
                AllowanceCharge = new AllowanceChargeInfo
                {
                    IsCharge = false,
                    Reason = "discount",
                    Amount = DiscountAmount,
                    CurrencyCode = CurrencyCode,//"JOD",
                },
                TaxAmount = TaxAmount,
                TaxExclusiveAmount = TotalBeforeTax,
                TaxInclusiveAmount = TotalAfterTax,
                AllowanceTotalAmount = DiscountAmount,
                PayableAmount = NetTotal 
                
            };




            string xml = GenerateInvoiceXml(header, details,
             originalInvoiceId,  originalInvoiceUuid,   originalInvoiceTotal);
            return xml;
        }
        public static string GenerateInvoiceXml(InvoiceHeader header, List<InvoiceLine> lines,
            string originalInvoiceId , string originalInvoiceUuid,decimal  originalInvoiceTotal)
        {
            // Create settings for the XML writer
            XmlWriterSettings settings = new XmlWriterSettings
            {
                Indent = true,
                IndentChars = "  ",
                Encoding = Encoding.UTF8,
            };

            // Create a string builder to hold the XML
            using (MemoryStream ms = new MemoryStream())
            {
                using (XmlWriter writer = XmlWriter.Create(ms, settings))
                {
                    // Start the document
                    writer.WriteStartDocument();

                    writer.WriteStartElement("", "Invoice", UBL_NAMESPACE);
                    writer.WriteAttributeString("xmlns", "cbc", null, CBC_NAMESPACE);
                    writer.WriteAttributeString("xmlns", "cac", null, CAC_NAMESPACE);
                    writer.WriteAttributeString("xmlns", "ext", null, "urn:oasis:names:specification:ubl:schema:xsd:CommonExtensionComponents-2");

                    // ✅ Insert UBLExtensions
                    writer.WriteStartElement("ext", "UBLExtensions", "urn:oasis:names:specification:ubl:schema:xsd:CommonExtensionComponents-2");

                    writer.WriteStartElement("ext", "UBLExtension", "urn:oasis:names:specification:ubl:schema:xsd:CommonExtensionComponents-2");
                    writer.WriteStartElement("ext", "ExtensionContent", "urn:oasis:names:specification:ubl:schema:xsd:CommonExtensionComponents-2");

                    // Write full structure of ds:Signature (even if dummy)
                    writer.WriteStartElement("ds", "Signature", "http://www.w3.org/2000/09/xmldsig#");

                    writer.WriteStartElement("ds", "SignedInfo", "http://www.w3.org/2000/09/xmldsig#");
                    writer.WriteStartElement("ds", "CanonicalizationMethod", "http://www.w3.org/2000/09/xmldsig#");
                    writer.WriteAttributeString("Algorithm", "http://www.w3.org/TR/2001/REC-xml-c14n-20010315");
                    writer.WriteEndElement(); // CanonicalizationMethod

                    writer.WriteStartElement("ds", "SignatureMethod", "http://www.w3.org/2000/09/xmldsig#");
                    writer.WriteAttributeString("Algorithm", "http://www.w3.org/2000/09/xmldsig#rsa-sha1");
                    writer.WriteEndElement(); // SignatureMethod

                    writer.WriteStartElement("ds", "Reference", "http://www.w3.org/2000/09/xmldsig#");
                    writer.WriteAttributeString("URI", "");

                    writer.WriteStartElement("ds", "Transforms", "http://www.w3.org/2000/09/xmldsig#");
                    writer.WriteStartElement("ds", "Transform", "http://www.w3.org/2000/09/xmldsig#");
                    writer.WriteAttributeString("Algorithm", "http://www.w3.org/2000/09/xmldsig#enveloped-signature");
                    writer.WriteEndElement(); // Transform
                    writer.WriteEndElement(); // Transforms

                    writer.WriteStartElement("ds", "DigestMethod", "http://www.w3.org/2000/09/xmldsig#");
                    writer.WriteAttributeString("Algorithm", "http://www.w3.org/2000/09/xmldsig#sha1");
                    writer.WriteEndElement(); // DigestMethod

                    writer.WriteElementString("ds", "DigestValue", "http://www.w3.org/2000/09/xmldsig#", "SGVsbG9Xb3JsZAo=");

                    writer.WriteEndElement(); // Reference
                    writer.WriteEndElement(); // SignedInfo

                    writer.WriteElementString("ds", "SignatureValue", "http://www.w3.org/2000/09/xmldsig#", "U2lnbmF0dXJlVmFsdWUyMw==");

                    writer.WriteStartElement("ds", "KeyInfo", "http://www.w3.org/2000/09/xmldsig#");
                    writer.WriteStartElement("ds", "X509Data", "http://www.w3.org/2000/09/xmldsig#");



                    writer.WriteElementString("ds", "X509Certificate", "http://www.w3.org/2000/09/xmldsig#", "Q2VydGlmaWNhdGVWYWx1ZQ==");

                    writer.WriteEndElement(); // X509Data
                    writer.WriteEndElement(); // KeyInfo

                    writer.WriteEndElement(); // Signature

                    writer.WriteEndElement(); // ExtensionContent
                    writer.WriteEndElement(); // UBLExtension
                    writer.WriteEndElement(); // UBLExtensions



                    writer.WriteStartElement("UBLVersionID", CBC_NAMESPACE);
                    writer.WriteString("2.1");
                    writer.WriteEndElement(); // UBLVersionID

                    writer.WriteStartElement("CustomizationID", CBC_NAMESPACE);
                    writer.WriteString("urn:oasis:names:specification:ubl:default:Invoice-2"); // Replace if your local tax system requires a custom one
                    writer.WriteEndElement(); // CustomizationID

                    // <cbc:ProfileID>Basic</cbc:ProfileID>
                    writer.WriteStartElement("ProfileID", CBC_NAMESPACE);
                    writer.WriteString("Basic");
                    writer.WriteEndElement(); // ProfileID
                                              // END BLOCK ↑↑↑
                                              /////////////////////////////////

                    // Write header elements
                    writer.WriteStartElement("ID", CBC_NAMESPACE);
                    writer.WriteString(header.Id);
                    writer.WriteEndElement(); // ID

                    writer.WriteStartElement("UUID", CBC_NAMESPACE);
                    writer.WriteString(header.Uuid);
                    writer.WriteEndElement(); // UUID

                    writer.WriteStartElement("IssueDate", CBC_NAMESPACE);
                    writer.WriteString(header.IssueDate.ToString("yyyy-MM-dd"));
                    writer.WriteEndElement(); // IssueDate

                    // Write invoice type code with attribute
                    writer.WriteStartElement("InvoiceTypeCode", CBC_NAMESPACE);
                    writer.WriteAttributeString("name", header.InvoiceTypeName);
                    writer.WriteString(header.InvoiceTypeCode);
                    writer.WriteEndElement(); // InvoiceTypeCode


                 
                    // Write note if provided
                    if (!string.IsNullOrEmpty(header.Note))
                    {
                        writer.WriteStartElement("Note", CBC_NAMESPACE);
                        writer.WriteString(header.Note);
                        writer.WriteEndElement(); // Note
                    }
                   

                    writer.WriteStartElement("DocumentCurrencyCode", CBC_NAMESPACE);
                    writer.WriteString(header.DocumentCurrencyCode);
                    writer.WriteEndElement(); // DocumentCurrencyCode

                    writer.WriteStartElement("TaxCurrencyCode", CBC_NAMESPACE);
                    writer.WriteString(header.TaxCurrencyCode);
                    writer.WriteEndElement(); // TaxCurrencyCode
                    if (header.InvoiceTypeCode == GetInvoiceTypeCodeString(InvoiceTypeCode.NewReturnSalesInvoice))
                    {
                        //// Write invoice type code with attribute
                        //writer.WriteStartElement("InvoiceTypeCode", CBC_NAMESPACE);
                        //writer.WriteAttributeString("name", header.InvoiceTypeName);
                        //writer.WriteString(header.InvoiceTypeCode);
                        //writer.WriteEndElement(); // InvoiceTypeCode
                        // BillingReference
                        writer.WriteStartElement("BillingReference", CAC_NAMESPACE);
                        writer.WriteStartElement("InvoiceDocumentReference", CAC_NAMESPACE);
                        writer.WriteElementString("ID", CBC_NAMESPACE, originalInvoiceId);
                        writer.WriteElementString("UUID", CBC_NAMESPACE, originalInvoiceUuid);
                        writer.WriteElementString("DocumentDescription", CBC_NAMESPACE, originalInvoiceTotal.ToString("0.000", CultureInfo.InvariantCulture));
                        writer.WriteEndElement(); // InvoiceDocumentReference
                        writer.WriteEndElement(); // BillingReference
           
                      
                    }

                    // Write additional document reference
                    writer.WriteStartElement("AdditionalDocumentReference", CAC_NAMESPACE);

                    writer.WriteStartElement("ID", CBC_NAMESPACE);
                    writer.WriteString(header.AdditionalDocRefId);
                    writer.WriteEndElement(); // ID

                    writer.WriteStartElement("UUID", CBC_NAMESPACE);
                    writer.WriteString(header.AdditionalDocRefUuid);
                    writer.WriteEndElement(); // UUID

                    writer.WriteEndElement(); // AdditionalDocumentReference
             
                    // Write supplier party
                  WriteSupplierParty(writer, header.SupplierInfo);

                    // Write customer party
                    WriteCustomerParty(writer, header.CustomerInfo);

                   

                    // Write seller supplier party if specified
                    if (header.SellerInfo != null)
                    {
                        WriteSellerParty(writer, header.SellerInfo);
                    }
                    if (header.InvoiceTypeCode == GetInvoiceTypeCodeString(InvoiceTypeCode.NewReturnSalesInvoice))
                    {
                        // Return Reason 
                        writer.WriteStartElement("PaymentMeans", CAC_NAMESPACE);
                        writer.WriteStartElement("PaymentMeansCode", CBC_NAMESPACE);
                        writer.WriteAttributeString("listID", "UN/ECE 4461");
                        writer.WriteString("10");
                        writer.WriteEndElement(); // PaymentMeansCode
                        writer.WriteElementString("InstructionNote", CBC_NAMESPACE, "Return for testing"); // Or hardcode the reason here
                        writer.WriteEndElement();
                    }
                    // Write allowance charge if specified
                    if (header.AllowanceCharge != null)
                    {
                        WriteAllowanceCharge(writer, header.AllowanceCharge);
                    }

                    // Write tax total
                    WriteTaxTotal(writer, header.TaxAmount, header.CurrencyCode);

                    // Write legal monetary total
                    WriteLegalMonetaryTotal(writer, header);

                    // Write invoice lines
                    foreach (var line in lines)
                    {
                        WriteInvoiceLine(writer, line, header.CurrencyCode);
                    }

                    // End the document
                    writer.WriteEndElement(); // Invoice
                    writer.WriteEndDocument();
                }
                return Encoding.UTF8.GetString(ms.ToArray());
            }
        }

        private static void WriteSupplierParty(XmlWriter writer, SupplierInfo supplierInfo)
        {
            writer.WriteStartElement("AccountingSupplierParty", CAC_NAMESPACE);
            writer.WriteStartElement("Party", CAC_NAMESPACE);

            // Postal address
            writer.WriteStartElement("PostalAddress", CAC_NAMESPACE);
            writer.WriteStartElement("Country", CAC_NAMESPACE);

            writer.WriteStartElement("IdentificationCode", CBC_NAMESPACE);
            writer.WriteString(supplierInfo.CountryCode);
            writer.WriteEndElement(); // IdentificationCode

            writer.WriteEndElement(); // Country
            writer.WriteEndElement(); // PostalAddress

            // Party tax scheme
            writer.WriteStartElement("PartyTaxScheme", CAC_NAMESPACE);

            writer.WriteStartElement("CompanyID", CBC_NAMESPACE);
            writer.WriteString(supplierInfo.TaxId);
            writer.WriteEndElement(); // CompanyID

            writer.WriteStartElement("TaxScheme", CAC_NAMESPACE);

            writer.WriteStartElement("ID", CBC_NAMESPACE);
            writer.WriteString("VAT");
            writer.WriteEndElement(); // ID

            writer.WriteEndElement(); // TaxScheme
            writer.WriteEndElement(); // PartyTaxScheme

            // Party legal entity
            writer.WriteStartElement("PartyLegalEntity", CAC_NAMESPACE);

            writer.WriteStartElement("RegistrationName", CBC_NAMESPACE);
            writer.WriteString(supplierInfo.RegistrationName);
            writer.WriteEndElement(); // RegistrationName

            writer.WriteEndElement(); // PartyLegalEntity

            writer.WriteEndElement(); // Party
            writer.WriteEndElement(); // AccountingSupplierParty
        }

        private static void WriteCustomerParty(XmlWriter writer, CustomerInfo customerInfo)
        {
            writer.WriteStartElement("AccountingCustomerParty", CAC_NAMESPACE);
            writer.WriteStartElement("Party", CAC_NAMESPACE);

            // Party identification
            writer.WriteStartElement("PartyIdentification", CAC_NAMESPACE);

            writer.WriteStartElement("ID", CBC_NAMESPACE);
            writer.WriteAttributeString("schemeID", customerInfo.SchemeId);
            writer.WriteString(customerInfo.Id);
            writer.WriteEndElement(); // ID

            writer.WriteEndElement(); // PartyIdentification

            // Postal address
            writer.WriteStartElement("PostalAddress", CAC_NAMESPACE);

            writer.WriteStartElement("PostalZone", CBC_NAMESPACE);
            writer.WriteString(customerInfo.PostalZone);
            writer.WriteEndElement(); // PostalZone

            writer.WriteStartElement("CountrySubentityCode", CBC_NAMESPACE);
            writer.WriteString(customerInfo.CountrySubentityCode);
            writer.WriteEndElement(); // CountrySubentityCode

            writer.WriteStartElement("Country", CAC_NAMESPACE);

            writer.WriteStartElement("IdentificationCode", CBC_NAMESPACE);
            writer.WriteString(customerInfo.CountryCode);
            writer.WriteEndElement(); // IdentificationCode

            writer.WriteEndElement(); // Country
            writer.WriteEndElement(); // PostalAddress

            // Party tax scheme
            writer.WriteStartElement("PartyTaxScheme", CAC_NAMESPACE);

            writer.WriteStartElement("CompanyID", CBC_NAMESPACE);
            writer.WriteString(customerInfo.TaxId);
            writer.WriteEndElement(); // CompanyID

            writer.WriteStartElement("TaxScheme", CAC_NAMESPACE);

            writer.WriteStartElement("ID", CBC_NAMESPACE);
            writer.WriteString("VAT");
            writer.WriteEndElement(); // ID

            writer.WriteEndElement(); // TaxScheme
            writer.WriteEndElement(); // PartyTaxScheme

            // Party legal entity
            writer.WriteStartElement("PartyLegalEntity", CAC_NAMESPACE);

            writer.WriteStartElement("RegistrationName", CBC_NAMESPACE);
            writer.WriteString(customerInfo.RegistrationName);
            writer.WriteEndElement(); // RegistrationName

            writer.WriteEndElement(); // PartyLegalEntity

            writer.WriteEndElement(); // Party

            // Contact info
            if (!string.IsNullOrEmpty(customerInfo.Telephone))
            {
                writer.WriteStartElement("AccountingContact", CAC_NAMESPACE);

                writer.WriteStartElement("Telephone", CBC_NAMESPACE);
                writer.WriteString(customerInfo.Telephone);
                writer.WriteEndElement(); // Telephone

                writer.WriteEndElement(); // AccountingContact
            }

            writer.WriteEndElement(); // AccountingCustomerParty
        }

        private static void WriteSellerParty(XmlWriter writer, SellerInfo sellerInfo)
        {
            writer.WriteStartElement("SellerSupplierParty", CAC_NAMESPACE);
            writer.WriteStartElement("Party", CAC_NAMESPACE);
            writer.WriteStartElement("PartyIdentification", CAC_NAMESPACE);

            writer.WriteStartElement("ID", CBC_NAMESPACE);
            writer.WriteString(sellerInfo.Id);
            writer.WriteEndElement(); // ID

            writer.WriteEndElement(); // PartyIdentification
            writer.WriteEndElement(); // Party
            writer.WriteEndElement(); // SellerSupplierParty
        }

        private static void WriteAllowanceCharge(XmlWriter writer, AllowanceChargeInfo allowanceCharge)
        {
            writer.WriteStartElement("AllowanceCharge", CAC_NAMESPACE);

            writer.WriteStartElement("ChargeIndicator", CBC_NAMESPACE);
            writer.WriteString(allowanceCharge.IsCharge ? "true" : "false");
            writer.WriteEndElement(); // ChargeIndicator

            writer.WriteStartElement("AllowanceChargeReason", CBC_NAMESPACE);
            writer.WriteString(allowanceCharge.Reason);
            writer.WriteEndElement(); // AllowanceChargeReason

            writer.WriteStartElement("Amount", CBC_NAMESPACE);
            writer.WriteAttributeString("currencyID", allowanceCharge.CurrencyCode);
            writer.WriteString(allowanceCharge.Amount.ToString("0.000", CultureInfo.InvariantCulture));
            writer.WriteEndElement(); // Amount

            writer.WriteEndElement(); // AllowanceCharge
        }

        private static void WriteTaxTotal(XmlWriter writer, decimal taxAmount, string currencyCode)
        {
            writer.WriteStartElement("TaxTotal", CAC_NAMESPACE);

            writer.WriteStartElement("TaxAmount", CBC_NAMESPACE);
            writer.WriteAttributeString("currencyID", currencyCode);
            writer.WriteString(taxAmount.ToString("0.000", CultureInfo.InvariantCulture));
            writer.WriteEndElement(); // TaxAmount

            writer.WriteEndElement(); // TaxTotal
        }

        private static void WriteLegalMonetaryTotal(XmlWriter writer, InvoiceHeader header)
        {
            writer.WriteStartElement("LegalMonetaryTotal", CAC_NAMESPACE);

            writer.WriteStartElement("TaxExclusiveAmount", CBC_NAMESPACE);
            writer.WriteAttributeString("currencyID", header.CurrencyCode);
            writer.WriteString(header.TaxExclusiveAmount.ToString("0.000", CultureInfo.InvariantCulture));
            writer.WriteEndElement(); // TaxExclusiveAmount

            writer.WriteStartElement("TaxInclusiveAmount", CBC_NAMESPACE);
            writer.WriteAttributeString("currencyID", header.CurrencyCode);
            writer.WriteString(header.TaxInclusiveAmount.ToString("0.000", CultureInfo.InvariantCulture));
            writer.WriteEndElement(); // TaxInclusiveAmount

            writer.WriteStartElement("AllowanceTotalAmount", CBC_NAMESPACE);
            writer.WriteAttributeString("currencyID", header.CurrencyCode);
            writer.WriteString(header.AllowanceTotalAmount.ToString("0.000", CultureInfo.InvariantCulture));
            writer.WriteEndElement(); // AllowanceTotalAmount

            writer.WriteStartElement("PayableAmount", CBC_NAMESPACE);
            writer.WriteAttributeString("currencyID", header.CurrencyCode);
            writer.WriteString(header.PayableAmount.ToString("0.000", CultureInfo.InvariantCulture));
            writer.WriteEndElement(); // PayableAmount

            writer.WriteEndElement(); // LegalMonetaryTotal
        }

        private static void WriteInvoiceLine(XmlWriter writer, InvoiceLine line, string currencyCode)
        {
            writer.WriteStartElement("InvoiceLine", CAC_NAMESPACE);

            writer.WriteStartElement("ID", CBC_NAMESPACE);
            writer.WriteString(line.Id.ToString());
            writer.WriteEndElement(); // ID

            writer.WriteStartElement("InvoicedQuantity", CBC_NAMESPACE);
            writer.WriteAttributeString("unitCode", line.UnitCode);
            writer.WriteString(line.Quantity.ToString("0.000", CultureInfo.InvariantCulture));
            writer.WriteEndElement(); // InvoicedQuantity

            writer.WriteStartElement("LineExtensionAmount", CBC_NAMESPACE);
            writer.WriteAttributeString("currencyID", currencyCode);
            writer.WriteString(line.LineExtensionAmount.ToString("0.000", CultureInfo.InvariantCulture));
            writer.WriteEndElement(); // LineExtensionAmount

            // Tax total
            writer.WriteStartElement("TaxTotal", CAC_NAMESPACE);

            writer.WriteStartElement("TaxAmount", CBC_NAMESPACE);
            writer.WriteAttributeString("currencyID", currencyCode);
            writer.WriteString(line.TaxAmount.ToString("0.000", CultureInfo.InvariantCulture));
            writer.WriteEndElement(); // TaxAmount

            writer.WriteStartElement("RoundingAmount", CBC_NAMESPACE);
            writer.WriteAttributeString("currencyID", currencyCode);
            writer.WriteString(line.RoundingAmount.ToString("0.000", CultureInfo.InvariantCulture));
            writer.WriteEndElement(); // RoundingAmount

            // Tax subtotal
            writer.WriteStartElement("TaxSubtotal", CAC_NAMESPACE);

            writer.WriteStartElement("TaxAmount", CBC_NAMESPACE);
            writer.WriteAttributeString("currencyID", currencyCode);
            writer.WriteString(line.TaxAmount.ToString("0.000", CultureInfo.InvariantCulture));
            writer.WriteEndElement(); // TaxAmount

            writer.WriteStartElement("TaxCategory", CAC_NAMESPACE);

            writer.WriteStartElement("ID", CBC_NAMESPACE);
            writer.WriteAttributeString("schemeAgencyID", "6");
            writer.WriteAttributeString("schemeID", "UN/ECE 5305");
            writer.WriteString(line.TaxCategoryId);
            writer.WriteEndElement(); // ID

            writer.WriteStartElement("Percent", CBC_NAMESPACE);
            writer.WriteString(Simulate.Integer32( line.TaxPercent).ToString( ));
            writer.WriteEndElement(); // Percent

            writer.WriteStartElement("TaxScheme", CAC_NAMESPACE);

            writer.WriteStartElement("ID", CBC_NAMESPACE);
            writer.WriteAttributeString("schemeAgencyID", "6");
            writer.WriteAttributeString("schemeID", "UN/ECE 5153");
            writer.WriteString("VAT");
            writer.WriteEndElement(); // ID

            writer.WriteEndElement(); // TaxScheme
            writer.WriteEndElement(); // TaxCategory
            writer.WriteEndElement(); // TaxSubtotal
            writer.WriteEndElement(); // TaxTotal

            // Item
            writer.WriteStartElement("Item", CAC_NAMESPACE);

            writer.WriteStartElement("Name", CBC_NAMESPACE);
            writer.WriteString(line.ItemName);
            writer.WriteEndElement(); // Name

            writer.WriteEndElement(); // Item

            // Price
            writer.WriteStartElement("Price", CAC_NAMESPACE);

            writer.WriteStartElement("PriceAmount", CBC_NAMESPACE);
            writer.WriteAttributeString("currencyID", currencyCode);
            writer.WriteString(line.PriceAmount.ToString("0.000", CultureInfo.InvariantCulture));
            writer.WriteEndElement(); // PriceAmount

            // Allowance charge
            if (line.AllowanceCharge != null)
            {
                writer.WriteStartElement("AllowanceCharge", CAC_NAMESPACE);

                writer.WriteStartElement("ChargeIndicator", CBC_NAMESPACE);
                writer.WriteString(line.AllowanceCharge.IsCharge ? "true" : "false");
                writer.WriteEndElement(); // ChargeIndicator

                writer.WriteStartElement("AllowanceChargeReason", CBC_NAMESPACE);
                writer.WriteString(line.AllowanceCharge.Reason);
                writer.WriteEndElement(); // AllowanceChargeReason

                writer.WriteStartElement("Amount", CBC_NAMESPACE);
                writer.WriteAttributeString("currencyID", currencyCode);
                writer.WriteString(line.AllowanceCharge.Amount.ToString("0.000", CultureInfo.InvariantCulture));
                writer.WriteEndElement(); // Amount

                writer.WriteEndElement(); // AllowanceCharge
            }

            writer.WriteEndElement(); // Price

            writer.WriteEndElement(); // InvoiceLine
        }
        #region enums


        // Dictionary to map the enum to its code (e.g., NIN, PN, TN)
        static Dictionary<BuyerIdentificationType,
            string> _buyerIdentificationTypeCodes = new Dictionary<BuyerIdentificationType, string>
    {
    { BuyerIdentificationType.NationalID, "NIN" },
    { BuyerIdentificationType.PersonalNumber, "PN" },
    { BuyerIdentificationType.TaxNumber, "TN" }
    };
        static string GetBuyerIdentificationCode(BuyerIdentificationType type)
        {
            return _buyerIdentificationTypeCodes[type];
        }
        ////////////City 
        ///


        // Dictionary for governorate codes
        private static readonly Dictionary<JordanGovernorate, string> _governorateCodes = new Dictionary<JordanGovernorate, string>
{
    { JordanGovernorate.Balqa, "JO-BA" },
    { JordanGovernorate.Maan, "JO-MN" },
    { JordanGovernorate.Madaba, "JO-MD" },
    { JordanGovernorate.Mafraq, "JO-MA" },
    { JordanGovernorate.Karak, "JO-KA" },
    { JordanGovernorate.Jerash, "JO-JA" },
    { JordanGovernorate.Irbid, "JO-IR" },
    { JordanGovernorate.Zarqa, "JO-AZ" },
    { JordanGovernorate.Tafilah, "JO-AT" },
    { JordanGovernorate.Aqaba, "JO-AQ" },
    { JordanGovernorate.Amman, "JO-AM" },
    { JordanGovernorate.Ajloun, "JO-AJ" }
};

        static string GetGovernorateCode(JordanGovernorate governorate)
        {
            return _governorateCodes[governorate];
        }






        // Dictionary for governorate codes
        private static readonly Dictionary<ItemTaxCategory, string> _ItemTaxCategory = new Dictionary<ItemTaxCategory, string>
{
    { ItemTaxCategory.Exempt, "Z" },
    { ItemTaxCategory.Zero, "O" },
    { ItemTaxCategory.Taxable, "S" },

};

        static string GetItemTaxCategory(ItemTaxCategory itemTaxCategory)
        {
            return _ItemTaxCategory[itemTaxCategory];
        }
     
        private static readonly Dictionary<InvoiceTypeCode, string> _invoiceTypeCodeStrings = new Dictionary<InvoiceTypeCode, string>
    {
        { InvoiceTypeCode.NewSalesInvoice, "388" },
        { InvoiceTypeCode.NewReturnSalesInvoice, "381" }
    };
        static string GetInvoiceTypeCodeString(InvoiceTypeCode type)
        {
            return _invoiceTypeCodeStrings[type];
        }
        enum InvoiceTypeName
        {
            CashIncomeInvoice,
            DebitIncomeInvoice,
            DebitSalesInvoice,
            CashSalesInvoice,
            CashSpecialSalesInvoice, DebitSpecialSalesInvoice,
        }
        private static readonly Dictionary<InvoiceTypeName, string> _invoiceTypeNameStrings = new Dictionary<InvoiceTypeName, string>
    {
        { InvoiceTypeName.CashIncomeInvoice, "011" },
        { InvoiceTypeName.CashSalesInvoice, "012" },
        { InvoiceTypeName.CashSpecialSalesInvoice, "013" },
        { InvoiceTypeName.DebitIncomeInvoice, "021" },
        { InvoiceTypeName.DebitSalesInvoice, "022" },
        { InvoiceTypeName.DebitSpecialSalesInvoice, "023" },
    };
        static string GetInvoiceTypeNameString(InvoiceTypeName type)
        {
            return _invoiceTypeNameStrings[type];
        }

        #endregion
    }
   
    public class InvoiceHeader
    {
        public string Id { get; set; }
        public string Uuid { get; set; }
        public DateTime IssueDate { get; set; }
        public string InvoiceTypeCode { get; set; }
        public string InvoiceTypeName { get; set; }
        public string Note { get; set; }
        public string DocumentCurrencyCode { get; set; }
        public string CurrencyCode { get; set; }
        public string TaxCurrencyCode { get; set; }
        public string AdditionalDocRefId { get; set; }
        public string AdditionalDocRefUuid { get; set; }
        public SupplierInfo SupplierInfo { get; set; }
        public CustomerInfo CustomerInfo { get; set; }
        public SellerInfo SellerInfo { get; set; }
        public AllowanceChargeInfo AllowanceCharge { get; set; }
        public decimal TaxAmount { get; set; }
        public decimal TaxExclusiveAmount { get; set; }
        public decimal TaxInclusiveAmount { get; set; }
        public decimal AllowanceTotalAmount { get; set; }
        public decimal PayableAmount { get; set; }
    }

    public class SupplierInfo
    {
        public string CountryCode { get; set; }
        public string TaxId { get; set; }
        public string RegistrationName { get; set; }
    }

    public class CustomerInfo
    {
        public string Id { get; set; }
        public string SchemeId { get; set; }
        public string PostalZone { get; set; }
        public string CountrySubentityCode { get; set; }
        public string CountryCode { get; set; }
        public string TaxId { get; set; }
        public string RegistrationName { get; set; }
        public string Telephone { get; set; }
    }

    public class SellerInfo
    {
        public string Id { get; set; }
    }

    public class AllowanceChargeInfo
    {
        public bool IsCharge { get; set; }
        public string Reason { get; set; }
        public decimal Amount { get; set; }
        public string CurrencyCode { get; set; }
    }

    public class InvoiceLine
    {
        public int Id { get; set; }
        public decimal Quantity { get; set; }
        public string UnitCode { get; set; }
        public decimal LineExtensionAmount { get; set; }
        public decimal TaxAmount { get; set; }
        public decimal RoundingAmount { get; set; }
        public string TaxCategoryId { get; set; }
        public decimal TaxPercent { get; set; }
        public string ItemName { get; set; }
        public decimal PriceAmount { get; set; }
        public AllowanceChargeInfo AllowanceCharge { get; set; }


    }
    public enum ItemTaxCategory
    {
        Exempt,     // حالكانت السلعة او الخدمة معفية
        Zero,      // حالكانت السلعة او الخدمة خاضعة لنسبة الصفر
        Taxable,    // ف %  عدا ال            حال كانت السلعة او الخدمة خاضعة لنسبة من النسب ال
    }
    public enum BuyerIdentificationType
    {
        NationalID,      // الرقم الوطني للمشتري (NIN)
        PersonalNumber,  // الرقم الشخصي لغير الأردني (PN)
        TaxNumber         // الرقم الضريبي للمشتري (TN)
    }
    public enum InvoiceTypeCode
    {
        NewSalesInvoice,
        NewReturnSalesInvoice,
    }
    public enum JordanGovernorate
    {
        Balqa,     // البلقاء
        Maan,      // معان
        Madaba,    // مادبا
        Mafraq,    // المفرق
        Karak,     // الكرك
        Jerash,    // جرش
        Irbid,     // إربد
        Zarqa,     // الزرقاء
        Tafilah,   // الطفيلة
        Aqaba,     // العقبة
        Amman,     // عمان
        Ajloun     // عجلون
    }
}
