using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using System;
using System.Linq;

namespace WebApplication2.cls
{


    //var sender = new UBLInvoiceSenderFull("your_user_code", "your_secret_key");

    //string response = await sender.SendInvoiceAsync(
    //    dbInvoiceHeader,
    //    detailsList,
    //    supplierName: "MT Softs",
    //    supplierVAT: "123456789",
    //    customerName: "Al Hekma Pharmacy",
    //    customerVAT: "987654321"
    //);

    //Console.WriteLine(response);

    public class clsJOInvoice
    {
        private readonly string _userCode;
        private readonly string _secretKey;

        public clsJOInvoice(string userCode, string secretKey)
        {
            _userCode = userCode;
            _secretKey = secretKey;
        }

        public async Task<string> SendInvoiceAsync(DBInvoiceHeader header, List<DBInvoiceDetails> details, string supplierName, string supplierVAT, string customerName = "", string customerVAT = "")
        {
            XDocument ublXml = GenerateUBLInvoice(header, details, supplierName, supplierVAT, customerName, customerVAT);
            string xmlString = ublXml.ToString(SaveOptions.DisableFormatting);
            string base64EncodedXml = Convert.ToBase64String(Encoding.UTF8.GetBytes(xmlString));

            using (var client = new HttpClient())
            {
                client.DefaultRequestHeaders.Add("UserCode", _userCode);
                client.DefaultRequestHeaders.Add("UserSecret", _secretKey);

                var content = new StringContent($"\"{base64EncodedXml}\"", Encoding.UTF8, "application/json");
                var response = await client.PostAsync("https://backend.jofotara.gov.jo/core/invoices/", content);
                return await response.Content.ReadAsStringAsync();
            }
        }

        private XDocument GenerateUBLInvoice(DBInvoiceHeader header, List<DBInvoiceDetails> details, string supplierName, string supplierVAT, string customerName, string customerVAT)
        {
            XNamespace invoiceNs = "urn:oasis:names:specification:ubl:schema:xsd:Invoice-2";
            XNamespace cac = "urn:oasis:names:specification:ubl:schema:xsd:CommonAggregateComponents-2";
            XNamespace cbc = "urn:oasis:names:specification:ubl:schema:xsd:CommonBasicComponents-2";

            XElement invoice = new XElement(invoiceNs + "Invoice",
                new XAttribute(XNamespace.Xmlns + "cac", cac),
                new XAttribute(XNamespace.Xmlns + "cbc", cbc),

                new XElement(cbc + "ProfileID", "reporting:1.0"),
                new XElement(cbc + "ID", header.InvoiceNo.ToString()),
                new XElement(cbc + "UUID", header.Guid != Guid.Empty ? header.Guid : Guid.NewGuid()),
                new XElement(cbc + "IssueDate", header.InvoiceDate.ToString("yyyy-MM-dd")),
                new XElement(cbc + "InvoiceTypeCode", new XAttribute("name", "012"), "381"),
                new XElement(cbc + "DocumentCurrencyCode", "JOD"),
                new XElement(cbc + "TaxCurrencyCode", "JOD"),

                // Supplier
                new XElement(cac + "AccountingSupplierParty",
                    new XElement(cac + "Party",
                        new XElement(cac + "PartyIdentification",
                            new XElement(cbc + "ID", new XAttribute("schemeID", "TN"), header.AccountID)
                        ),
                        new XElement(cac + "PostalAddress",
                            new XElement(cac + "Country",
                                new XElement(cbc + "IdentificationCode", "JO")
                            )
                        ),
                        new XElement(cac + "PartyTaxScheme",
                            new XElement(cbc + "CompanyID", supplierVAT),
                            new XElement(cac + "TaxScheme", new XElement(cbc + "ID", "VAT"))
                        ),
                        new XElement(cac + "PartyLegalEntity",
                            new XElement(cbc + "RegistrationName", supplierName)
                        )
                    )
                ),

                // Customer
                new XElement(cac + "AccountingCustomerParty",
                    new XElement(cac + "Party",
                        new XElement(cac + "PostalAddress",
                            new XElement(cac + "Country", new XElement(cbc + "IdentificationCode", "JO"))
                        ),
                        new XElement(cac + "PartyTaxScheme",
                            new XElement(cac + "TaxScheme", new XElement(cbc + "ID", "VAT"))
                        ),
                        new XElement(cac + "PartyLegalEntity",
                            new XElement(cbc + "RegistrationName", string.IsNullOrWhiteSpace(customerName) ? "Customer" : customerName)
                        )
                    )
                ),

                // Payment Info
                new XElement(cac + "PaymentMeans",
                    new XElement(cbc + "PaymentMeansCode", new XAttribute("listID", "UN/ECE 4461"), header.PaymentMethodID.ToString()),
                    new XElement(cbc + "InstructionNote", "In cash") // Update as needed
                ),

                // Allowance (Header Discount)
                new XElement(cac + "AllowanceCharge",
                    new XElement(cbc + "ChargeIndicator", "false"),
                    new XElement(cbc + "AllowanceChargeReason", "Header Discount"),
                    new XElement(cbc + "Amount", new XAttribute("currencyID", "JO"), header.HeaderDiscount)
                ),

                // Tax Total
                new XElement(cac + "TaxTotal",
                    new XElement(cbc + "TaxAmount", new XAttribute("currencyID", "JO"), header.TotalTax),
                    details.Select(line => new XElement(cac + "TaxSubtotal",
                        new XElement(cbc + "TaxableAmount", new XAttribute("currencyID", "JO"), line.PriceBeforeTax * line.Qty),
                        new XElement(cbc + "TaxAmount", new XAttribute("currencyID", "JO"), line.TaxAmount),
                        new XElement(cac + "TaxCategory",
                            new XElement(cbc + "ID", new XAttribute("schemeID", "UN/ECE 5305"), "S"),
                            new XElement(cbc + "Percent", line.TaxPercentage),
                            new XElement(cac + "TaxScheme", new XElement(cbc + "ID", new XAttribute("schemeID", "UN/ECE 5153"), "VAT"))
                        )
                    ))
                ),

                // Totals
                new XElement(cac + "LegalMonetaryTotal",
                    new XElement(cbc + "TaxExclusiveAmount", new XAttribute("currencyID", "JO"), header.TotalInvoice - header.TotalTax),
                    new XElement(cbc + "TaxInclusiveAmount", new XAttribute("currencyID", "JO"), header.TotalInvoice),
                    new XElement(cbc + "AllowanceTotalAmount", new XAttribute("currencyID", "JO"), header.TotalDiscount),
                    new XElement(cbc + "PrepaidAmount", new XAttribute("currencyID", "JO"), 0),
                    new XElement(cbc + "PayableAmount", new XAttribute("currencyID", "JO"), header.TotalInvoice)
                ),

                // Invoice Lines
                details.Select((line, index) => new XElement(cac + "InvoiceLine",
                    new XElement(cbc + "ID", (index + 1).ToString()),
                    new XElement(cbc + "InvoicedQuantity", new XAttribute("unitCode", "PCE"), line.Qty),
                    new XElement(cbc + "LineExtensionAmount", new XAttribute("currencyID", "JO"), line.TotalLine),

                    new XElement(cac + "TaxTotal",
                        new XElement(cbc + "TaxAmount", new XAttribute("currencyID", "JO"), line.TaxAmount),
                        new XElement(cac + "TaxSubtotal",
                            new XElement(cbc + "TaxableAmount", new XAttribute("currencyID", "JO"), line.PriceBeforeTax * line.Qty),
                            new XElement(cbc + "TaxAmount", new XAttribute("currencyID", "JO"), line.TaxAmount),
                            new XElement(cac + "TaxCategory",
                                new XElement(cbc + "ID", "S"),
                                new XElement(cbc + "Percent", line.TaxPercentage),
                                new XElement(cac + "TaxScheme", new XElement(cbc + "ID", "VAT"))
                            )
                        )
                    ),

                    new XElement(cac + "Item", new XElement(cbc + "Name", line.ItemName)),
                    new XElement(cac + "Price",
                        new XElement(cbc + "PriceAmount", new XAttribute("currencyID", "JO"), line.PriceBeforeTax),
                        new XElement(cbc + "BaseQuantity", new XAttribute("unitCode", "C62"), 1)
                    )
                ))
            );

            return new XDocument(new XDeclaration("1.0", "UTF-8", null), invoice);
        }
    }
}