using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System;
using System.ComponentModel;
using static WebApplication2.MainClasses.clsEnum;
using DocumentFormat.OpenXml.Spreadsheet;
using System.IO;
using ClosedXML.Excel;
using Microsoft.AspNetCore.Http;
using System.Data;
using WebApplication2.cls;
using Microsoft.Data.SqlClient;
using System.Drawing;
using System.Linq;
using DocumentFormat.OpenXml.Office2019.Presentation;

namespace WebApplication2.Controllers
{
    [Route("api/MasterDataImport")]
    [ApiController]
    public class MasterDataImport : Controller
    {
        [HttpPost("UploadExcelWithParams")]
        public IActionResult UploadExcelWithParams(
           [FromForm] IFormFile excelFile,
           [FromForm] string tableName,
           [FromForm] int CompanyId,
           [FromForm] int CreationUserID)
        {
            // 1. Validate input
            if (excelFile == null || excelFile.Length == 0)
            {
                return BadRequest("No file uploaded or file is empty.");
            }

            if (string.IsNullOrWhiteSpace(tableName))
            {
                return BadRequest("tableName parameter is required.");
            }

            using var ms = new MemoryStream();
            excelFile.CopyTo(ms);
            DataTable dt = ConvertExcelToDataTable(ms);
            // Attempt to retrieve the "SystemMetadata" sheet
            using var workbook = new XLWorkbook(ms);
            var metaSheet = workbook.Worksheets.FirstOrDefault(ws => ws.Name == "SystemMetadata");
            if (metaSheet == null)
            {
                // The file is missing our metadata sheet, possibly tampered
               // throw new Exception("Invalid Excel file. Missing system metadata.");
                return BadRequest("Invalid Excel file. Missing system metadata.");
            }

            // If sheet is protected, you might optionally .Unprotect("MetadataPassword") 
            // to read or verify the cell, though typically read operations are allowed.

            string storedSignature = metaSheet.Cell("A1").GetValue<string>();
            if (string.IsNullOrEmpty(storedSignature))
            {
                // Signature is missing or invalid
              //  throw new Exception("File signature is invalid or missing.");
                return BadRequest("File signature is invalid or missing.");
            }



            // 2. Example: Convert excel to DataTable (same logic from previous snippet)
         
            SqlTransaction trn;
            clsSQL clsSQL = new clsSQL();
            SqlConnection con = new SqlConnection(clsSQL.CreateDataBaseConnectionString(CompanyId));
            con.Open();
            trn = con.BeginTransaction();
            try
            {
               
          

            if (dt != null && dt.Rows.Count > 0) {

                   
                   
                    int A = 0;
                    bool IsSaved = true;

                    if (tableName == "tbl_Banks")
                    {
                        string ttt = metaSheet.Cell("A2").GetValue<string>();
                        if (ttt != tableName)
                        {

                            return BadRequest("This file is not for Banks");
                        }

                        clsBanks clsBanks = new clsBanks();

                        for (int i = 0; i < dt.Rows.Count; i++)
                        {

                            if (Simulate.Integer32(dt.Rows[i]["id"]) == 0)
                            {
                                A = clsBanks.InsertBanks(Simulate.String(dt.Rows[i]["AName"]), Simulate.String(dt.Rows[i]["EName"])
                                   , Simulate.String(dt.Rows[i]["AccountNumber"]), CompanyId, CreationUserID, trn);
                                if (A == 0)
                                {
                                    IsSaved = false;
                                }


                            }
                            else
                            {
                                A = clsBanks.UpdateBanks(Simulate.Integer32(dt.Rows[i]["ID"]), Simulate.String(dt.Rows[i]["AName"]), Simulate.String(dt.Rows[i]["EName"])
                                   , Simulate.String(dt.Rows[i]["AccountNumber"]), CreationUserID, CompanyId, trn);
                                if (A == 0)
                                {
                                    IsSaved = false;
                                }

                            }

                        }

                    }
                    else if (tableName == "tbl_ItemsCategory")
                    {
                        string ttt = metaSheet.Cell("A2").GetValue<string>();
                        if (ttt != tableName)
                        {

                            return BadRequest("This file is not for Items Category");
                        }

                        clsItemsCategory clsItemsCategory = new clsItemsCategory();

                        for (int i = 0; i < dt.Rows.Count; i++)
                        {

                            if (Simulate.Integer32(dt.Rows[i]["id"]) == 0)
                            {
                                A = clsItemsCategory.InsertItemsCategory(Simulate.String(dt.Rows[i]["AName"]), Simulate.String(dt.Rows[i]["EName"])
                                   , CompanyId, CreationUserID, trn);
                                if (A == 0)
                                {
                                    IsSaved = false;
                                }


                            }
                            else
                            {
                                A = clsItemsCategory.UpdateItemsCategory(Simulate.Integer32(dt.Rows[i]["ID"]), Simulate.String(dt.Rows[i]["AName"]), Simulate.String(dt.Rows[i]["EName"])
                                   , CreationUserID, CompanyId, trn);
                                if (A == 0)
                                {
                                    IsSaved = false;
                                }

                            }

                        }



                    }
                    else if (tableName == "tbl_Items")
                    {
                        string ttt = metaSheet.Cell("A2").GetValue<string>();
                        if (ttt != tableName)
                        {

                            return BadRequest("This file is not for Items Category");
                        }

                        clsItems clsItems = new clsItems();
                        clsTax clsTax = new clsTax();
                        DataTable dtTax = clsTax.SelectTaxByID(0, "", "", CompanyId, -1, -1, -1, -1);
                        clsItemsCategory clsItemsCategory = new clsItemsCategory();
                        DataTable dtItemCategory = clsItemsCategory.SelectItemsCategory(0, "", "", CompanyId);
                        clsItemsBoxType clsItemsBoxType = new clsItemsBoxType();
                        DataTable dtItemBoxType = clsItemsBoxType.SelectItemsBoxTypeByID(0, "", "", CompanyId);
                        clsCountries clsCountries = new clsCountries();
                        DataTable dtCountries = clsCountries.SelectCountriesByID(0, "", "", CompanyId);
                        clsItemReadType clsItemReadType = new clsItemReadType();
                        DataTable dtItemReadType = clsItemReadType.SelectItemReadTypeByID(0, "", "", CompanyId);
                        for (int i = 0; i < dt.Rows.Count; i++)
                        {



                            int SalesTaxid = 0;
                            int SpecialSalesTaxid = 0;
                            int PurchaseTaxid = 0;
                            int SpecialPurchaseTaxid = 0;
                            int Categoryid = 0;
                            //                                SpecialSales.AName as SpecialSalesTax,
                            //PurchaseTax.AName as PurchaseTax,
                            //SpecialPurchaseTax.AName as SpecialPurchaseTax,
                            //                    string AName, string EName, string Description, decimal SalesPriceBeforeTax, decimal SalesPriceAfterTax, int CategoryID, int SalesTaxID
                            //, int SpecialSalesTaxID, int PurchaseTaxID, int SpecialPurchaseTaxID, string Barcode, int ReadType, int OriginID, decimal MinimumLimit, byte[] Picture
                            //, bool IsActive, bool IsPOS, int BoxTypeID, bool IsStockItem, int POSOrder, int CompanyID, int CreationUserId
                            ///////////////////////////Sales Tax
                            decimal rowSalesTaxValue = Simulate.decimal_(dt.Rows[i]["SalesTax"]);
                            var filteredSalesTaxRows = dtTax.AsEnumerable()
                            .Where(row => Simulate.decimal_(row.Field<decimal>("Value")) == Simulate.decimal_(rowSalesTaxValue)
                            && row.Field<bool>("IsSalesTax") == true);
                            if (filteredSalesTaxRows.Any())
                            {
                                SalesTaxid = filteredSalesTaxRows.First().Field<int>("ID");
                                // Now you can use idnValue as needed
                            }
                            else
                            {

                                SalesTaxid = clsTax.InsertTax(Simulate.String(rowSalesTaxValue), Simulate.String(rowSalesTaxValue), rowSalesTaxValue, true, true, true, true, CompanyId, CreationUserID, trn);
                                dtTax = clsTax.SelectTaxByID(0, "", "", CompanyId, -1, -1, -1, -1, trn);
                            }
                            /////////////////////////// SpecialSalesTax
                            decimal rowSpecialSalesTaxValue = Simulate.decimal_(dt.Rows[i]["SpecialSalesTax"]);
                            var filteredSpecialSalesTaxRows = dtTax.AsEnumerable()
                            .Where(row => Simulate.decimal_(row.Field<decimal>("Value")) == Simulate.decimal_(rowSpecialSalesTaxValue)
                            && row.Field<bool>("IsSalesSpecialTax") == true);
                            if (filteredSpecialSalesTaxRows.Any())
                            {
                                SpecialSalesTaxid = filteredSpecialSalesTaxRows.First().Field<int>("ID");
                                // Now you can use idnValue as needed
                            }
                            else
                            {

                                SpecialSalesTaxid = clsTax.InsertTax(Simulate.String(rowSpecialSalesTaxValue), Simulate.String(rowSpecialSalesTaxValue), rowSpecialSalesTaxValue, true, true, true, true, CompanyId, CreationUserID, trn);
                                dtTax = clsTax.SelectTaxByID(0, "", "", CompanyId, -1, -1, -1, -1, trn);
                            }

                            /////////////////////////// PurchaseTax
                            decimal rowPurchaseTaxValue = Simulate.decimal_(dt.Rows[i]["PurchaseTax"]);
                            var filteredPurchaseTaxRows = dtTax.AsEnumerable()
                            .Where(row => Simulate.decimal_(row.Field<decimal>("Value")) == Simulate.decimal_(rowPurchaseTaxValue)
                            && row.Field<bool>("IsPurchaseTax") == true);
                            if (filteredPurchaseTaxRows.Any())
                            {
                                PurchaseTaxid = filteredPurchaseTaxRows.First().Field<int>("ID");
                                // Now you can use idnValue as needed
                            }
                            else
                            {

                                PurchaseTaxid = clsTax.InsertTax(Simulate.String(rowPurchaseTaxValue), Simulate.String(rowPurchaseTaxValue), rowPurchaseTaxValue, true, true, true, true, CompanyId, CreationUserID, trn);
                                dtTax = clsTax.SelectTaxByID(0, "", "", CompanyId, -1, -1, -1, -1, trn);
                            }  ///////////////////////////  SpecialPurchaseTax
                            decimal rowSpecialPurchaseTaxValue = Simulate.decimal_(dt.Rows[i]["SpecialPurchaseTax"]);
                            var filteredSpecialPurchaseTaxRows = dtTax.AsEnumerable()
                            .Where(row => Simulate.decimal_(row.Field<decimal>("Value")) == Simulate.decimal_(rowSpecialPurchaseTaxValue)
                            && row.Field<bool>("IsSpecialPurchaseTax") == true);
                            if (filteredSpecialPurchaseTaxRows.Any())
                            {
                                SpecialPurchaseTaxid = filteredSpecialPurchaseTaxRows.First().Field<int>("ID");
                                // Now you can use idnValue as needed
                            }
                            else
                            {

                                SpecialPurchaseTaxid = clsTax.InsertTax(Simulate.String(rowSpecialPurchaseTaxValue), Simulate.String(rowSpecialPurchaseTaxValue), rowSpecialPurchaseTaxValue, true, true, true, true, CompanyId, CreationUserID, trn);
                                dtTax = clsTax.SelectTaxByID(0, "", "", CompanyId, -1, -1, -1, -1, trn);
                            }///////////////////////////  Categoryid
                            string ItemsCategoryAName = Simulate.String(dt.Rows[i]["ItemsCategoryAName"]);
                            var filteredCategoryidRows = dtItemCategory.AsEnumerable()
                            .Where(row => row.Field<string>("AName") == ItemsCategoryAName
                             );
                            if (filteredCategoryidRows.Any())
                            {
                                Categoryid = filteredCategoryidRows.First().Field<int>("ID");
                                // Now you can use idnValue as needed
                            }
                            else if (ItemsCategoryAName.Length > 0)
                            {

                                Categoryid = clsItemsCategory.InsertItemsCategory(ItemsCategoryAName, ItemsCategoryAName, CompanyId, CreationUserID, trn);


                            }
                            ///////////////////////////  ItemsReadType
                            int ItemsReadTypeID = 0;
                            string ItemsReadTypeAName = Simulate.String(dt.Rows[i]["ItemReadType"]);
                            var filteredItemsReadTypeRows = dtItemReadType.AsEnumerable()
                            .Where(row => row.Field<string>("AName") == ItemsReadTypeAName
                             );
                            if (filteredItemsReadTypeRows.Any())
                            {
                                ItemsReadTypeID = filteredItemsReadTypeRows.First().Field<int>("ID");
                                // Now you can use idnValue as needed
                            }
                            else if (ItemsReadTypeAName.Length > 0)
                            {
                                ItemsReadTypeID = clsItemReadType.InsertItemReadType(ItemsReadTypeAName, ItemsReadTypeAName, CompanyId, CreationUserID, trn);


                            }
                            ///////////////////////////  dtCountries
                            int CountriesID = 0;
                            string CountrieAName = Simulate.String(dt.Rows[i]["Origin"]);
                            var filteredCountriesRows = dtCountries.AsEnumerable()
                            .Where(row => row.Field<string>("AName") == CountrieAName
                             );
                            if (filteredCountriesRows.Any())
                            {
                                CountriesID = filteredCountriesRows.First().Field<int>("ID");
                                // Now you can use idnValue as needed
                            }
                            else if (CountrieAName.Length > 0)
                            {

                                CountriesID = clsCountries.InsertCountries(CountrieAName, CountrieAName, CompanyId, CreationUserID, trn);
                            }

                            ///////////////////////////  dtCountries
                            int BoxTypeID = 0;
                            string BoxTypeAName = Simulate.String(dt.Rows[i]["ItemsBoxType"]);
                            var filteredItemBoxTypeRows = dtItemBoxType.AsEnumerable()
                            .Where(row => row.Field<string>("AName") == BoxTypeAName
                             );
                            if (filteredItemBoxTypeRows.Any())
                            {
                                BoxTypeID = filteredItemBoxTypeRows.First().Field<int>("ID");
                                // Now you can use idnValue as needed
                            }
                            else if (BoxTypeAName.Length > 0)
                            {

                                BoxTypeID = clsItemsBoxType.InsertItemsBoxType(BoxTypeAName, BoxTypeAName, 1, CompanyId, CreationUserID, trn);
                            }
                            if (Simulate.String(dt.Rows[i]["Guid"]) == "")
                            {
                                string guid = clsItems.InsertItems(Simulate.String(dt.Rows[i]["AName"]),
                                    Simulate.String(dt.Rows[i]["EName"]),
                                    Simulate.String(dt.Rows[i]["Description"]),
                                    Simulate.decimal_(dt.Rows[i]["SalesPriceAfterTax"]) / (1 + rowSpecialSalesTaxValue)
                                     / (1 + rowSalesTaxValue),

                                    Simulate.decimal_(dt.Rows[i]["SalesPriceAfterTax"]), Categoryid,
                                    Simulate.Integer32(SalesTaxid),
                                        Simulate.Integer32(SpecialSalesTaxid),
                                          Simulate.Integer32(PurchaseTaxid),
                                         Simulate.Integer32(SpecialPurchaseTaxid),
                                     Simulate.String(dt.Rows[i]["Barcode"]),
                                     ItemsReadTypeID, CountriesID,


                                    Simulate.decimal_(dt.Rows[i]["MinimumLimit"])
                                    , new byte[0], true, true,
                                  BoxTypeID, true, i,
                                  Simulate.Bool(dt.Rows[i]["TrackLot"]), 
                                  Simulate.Bool(dt.Rows[i]["TrackSerial"]), 
                                  Simulate.Bool(dt.Rows[i]["TrackExpiryDate"])
                                  , CompanyId, CreationUserID, trn);
                                if (guid == "")
                                {
                                    IsSaved = false;
                                }


                            }
                            else
                            {
                                DataTable dtsub = clsItems.SelectItemsByGuid(Simulate.String(dt.Rows[i]["Guid"]), "", "", "", 0, -1, CompanyId, trn);

                                A = clsItems.UpdateItems(Simulate.String(dt.Rows[i]["Guid"]), Simulate.String(dt.Rows[i]["AName"]),
                                     Simulate.String(dt.Rows[i]["EName"]), Simulate.String(dt.Rows[i]["Description"]),
                                       Simulate.decimal_(dt.Rows[i]["SalesPriceAfterTax"]) / (1 + rowSpecialSalesTaxValue)
                                     / (1 + rowSalesTaxValue), Simulate.decimal_(dt.Rows[i]["SalesPriceAfterTax"]), Categoryid,
                                    Simulate.Integer32(SalesTaxid), Simulate.Integer32(SpecialSalesTaxid),
                                          Simulate.Integer32(PurchaseTaxid),
                                         Simulate.Integer32(SpecialPurchaseTaxid), Simulate.String(dt.Rows[i]["Barcode"]),
                                     ItemsReadTypeID, CountriesID,


                                    Simulate.decimal_(dt.Rows[i]["MinimumLimit"]), dtsub.Rows[0]["Picture"] as byte[],
                                    Simulate.Bool(dtsub.Rows[0]["IsActive"]), Simulate.Bool(dtsub.Rows[0]["IsPOS"])
                                    , BoxTypeID, Simulate.Bool(dtsub.Rows[0]["IsStockItem"]), Simulate.Integer32(dtsub.Rows[0]["POSOrder"]),
                                    Simulate.Bool(dt.Rows[i]["TrackLot"]) , Simulate.Bool(dt.Rows[i]["TrackSerial"]) , Simulate.Bool(dt.Rows[i]["TrackExpiryDate"]) , CreationUserID, CompanyId, trn);










                                if (A == 0)
                                {
                                    IsSaved = false;
                                }

                            }

                        }



                    }
                    else if (tableName == "tbl_Customers" || tableName == "tbl_Vendors")
                    {
                        int BpType = 0;
                        if (tableName == "tbl_Customers") {

                            BpType = 1;
                        } else {

                            BpType = 2;
                        }

                            clsBusinessPartner clsBusinessPartner = new clsBusinessPartner();
                        string ttt = metaSheet.Cell("A2").GetValue<string>();
                        if (ttt != tableName)
                        {

                            return BadRequest("This file is not for Customers");
                        }

                        for (int i = 0; i < dt.Rows.Count; i++)
                        {

                        
                        if (Simulate.Integer32(dt.Rows[i]["id"]) == 0)
                        {
                          A=  clsBusinessPartner.InsertBusinessPartner(
                                Simulate.String(dt.Rows[i]["AName"]),
                                 Simulate.String(dt.Rows[i]["EName"]),
                                  Simulate.String(dt.Rows[i]["CommercialName"]),
                                  Simulate.String(dt.Rows[i]["Address"]),
                                    Simulate.String(dt.Rows[i]["Tel"]),
                                     Simulate.Bool(dt.Rows[i]["Active"]), 
                                     Simulate.Val(dt.Rows[i]["Limit"]),
                                     Simulate.String(dt.Rows[i]["Email"]), BpType, CompanyId,CreationUserID
                                     , Simulate.String(dt.Rows[i]["EmpCode"]),
                                     Simulate.String(dt.Rows[i]["StreetName"]),
                                        Simulate.String(dt.Rows[i]["HouseNumber"]),
                                           Simulate.String(dt.Rows[i]["NationalNumber"]), 
                                            Simulate.String(dt.Rows[i]["PassportNumber"]),0,
                                       Simulate.String(dt.Rows[i]["IDNumber"]),
                                        Simulate.String(dt.Rows[i]["TaxNumber"]),
                                         Simulate.String(dt.Rows[i]["Job"]),trn

                                ); if (A == 0)
                                {
                                    IsSaved = false;
                                }
                            }
                        else
                            {
                                A = clsBusinessPartner.UpdateBusinessPartner(Simulate.Integer32(dt.Rows[i]["id"]),
                                Simulate.String(dt.Rows[i]["AName"]),
                                 Simulate.String(dt.Rows[i]["EName"]),
                                  Simulate.String(dt.Rows[i]["CommercialName"]),
                                  Simulate.String(dt.Rows[i]["Address"]),
                                    Simulate.String(dt.Rows[i]["Tel"]),
                                     Simulate.Bool(dt.Rows[i]["Active"]),
                                     Simulate.Val(dt.Rows[i]["Limit"]),
                                     Simulate.String(dt.Rows[i]["Email"]),
                                     BpType, CreationUserID
                                     , Simulate.String(dt.Rows[i]["EmpCode"]),
                                     Simulate.String(dt.Rows[i]["StreetName"]),
                                        Simulate.String(dt.Rows[i]["HouseNumber"]),
                                           Simulate.String(dt.Rows[i]["NationalNumber"]),
                                            Simulate.String(dt.Rows[i]["PassportNumber"]), 0,
                                       Simulate.String(dt.Rows[i]["IDNumber"]),
                                        Simulate.String(dt.Rows[i]["TaxNumber"]),
                                         Simulate.String(dt.Rows[i]["Job"]),CompanyId,
                                         trn                                ); if (A == 0)
                                {
                                    IsSaved = false;
                                }

                            }
                        }
                    }
                  




                    //if (A == 0)
                    //IsSaved = false;
                    if (IsSaved)
                        trn.Commit();
                    else
                        trn.Rollback();
                
                con.Close();

            }


            }

            catch (Exception ex)
            {  trn.Rollback(); con.Close();
                return BadRequest($"Error reading Excel file: {ex.Message}");
            }

            // 3. Use the parameters (tableName, userId) as needed
            //    Insert dt rows into your DB, referencing tableName or userId if relevant

            // 4. Return success or other details
            return Ok(new
            {
                Message = $"Excel file uploaded successfully for table '{tableName}' by user {CreationUserID}.",
                RowCount = dt.Rows.Count
            });
        }

        /// <summary>
        /// Reads the first worksheet in the given Excel stream 
        /// and converts it into a DataTable.
        /// </summary>
        private DataTable ConvertExcelToDataTable(Stream excelStream)
        {
            // 1) Create an in-memory workbook using the uploaded file
            using var workbook = new XLWorkbook(excelStream);

            // 2) Get the first worksheet (or specify by name)
            var worksheet = workbook.Worksheet(1);

            // 3) Identify the used range (cells with data)
            var range = worksheet.RangeUsed();
            if (range == null)
                throw new Exception("The Excel file is empty or corrupt.");

            int rowCount = range.RowCount();
            int colCount = range.ColumnCount();

            var dataTable = new DataTable();

            // 4) The first row (row 1) is assumed to be headers
            //    Create DataTable columns from these headers
            for (int col = 1; col <= colCount; col++)
            {
                string columnName = range.Cell(1, col).GetValue<string>();

                // If a header cell is empty, assign a generic "ColumnX" name
                if (string.IsNullOrWhiteSpace(columnName))
                {
                    columnName = $"Column{col}";
                }

                dataTable.Columns.Add(columnName);
            }

            // 5) Read the remaining rows and add to DataTable
            for (int row = 2; row <= rowCount; row++)
            {
                var dataRow = dataTable.NewRow();

                for (int col = 1; col <= colCount; col++)
                {
                    dataRow[col - 1] = range.Cell(row, col).GetValue<string>();
                }

                dataTable.Rows.Add(dataRow);
            }

            return dataTable;
        }
     
        [HttpPost("GetExcelTemplate")]
        public IActionResult GetExcelTemplate(string CompanyID,string tableName )
        {
            var hiddenCols = new List<string> { };
          DataTable dt=new DataTable();
            if (tableName == "tbl_Banks")
            {
                  hiddenCols = new List<string> { "ID", "CompanyID", "CreationUserId", "CreationDate", "ModificationUserId", "ModificationDate", };

                clsBanks clsbanks = new clsBanks();
                dt = clsbanks.SelectBanks(0, "", "", Simulate.Integer32(CompanyID));
            }
            else if (tableName == "tbl_ItemsCategory")
            {
                  hiddenCols = new List<string> { "ID", "CompanyID", "CreationUserId", "CreationDate", "ModificationUserID", "ModificationDate", };

                clsItemsCategory clsItemsCategory = new clsItemsCategory();
                dt = clsItemsCategory.SelectItemsCategory(0, "", "", Simulate.Integer32(CompanyID));

            }
           
            else if (tableName == "tbl_Items")
            {
                
                hiddenCols = new List<string> { "Guid"  };
                string a = @"select Guid, tbl_items.AName,tbl_items.EName,[Description],SalesPriceAfterTax, 
tbl_ItemsCategory.AName ItemsCategoryAName,
SalesTax.AName as SalesTax,
SpecialSales.AName as SpecialSalesTax,
PurchaseTax.AName as PurchaseTax,
SpecialPurchaseTax.AName as SpecialPurchaseTax,
Barcode,
ItemReadType.AName ItemReadType,
tbl_Countries.AName Origin ,
MinimumLimit,
ItemsBoxType.AName as  ItemsBoxType,
TrackLot,
 TrackSerial,
TrackExpiryDate
from tbl_items
left join tbl_ItemsCategory on tbl_ItemsCategory.id = tbl_Items.CategoryID
left join tbl_Countries on tbl_Countries.id = tbl_Items.OriginID
left join tbl_Tax SalesTax on SalesTax.ID= tbl_Items.SalesTaxID
left join tbl_Tax SpecialSales on SpecialSales.ID= tbl_Items.SpecialSalesTaxID
left join tbl_Tax PurchaseTax on PurchaseTax.ID= tbl_Items.PurchaseTaxID
left join tbl_Tax  SpecialPurchaseTax on SpecialPurchaseTax.ID= tbl_Items.SpecialPurchaseTaxID
left join tbl_ItemReadType ItemReadType on ItemReadType.ID= tbl_Items.ReadType
left join tbl_ItemsBoxType ItemsBoxType on ItemsBoxType.ID= tbl_Items.BoxTypeID
where tbl_items.CompanyID =" + CompanyID.ToString();
                clsSQL cls = new clsSQL();
                dt = cls.ExecuteQueryStatement(a,cls.CreateDataBaseConnectionString(Simulate.Integer32( CompanyID)));

            }
            else if (tableName == "tbl_Customers")
            {
                hiddenCols = new List<string> { "ID", "CompanyID", "CreationUserID", "Nationality", "Type", "CreationDate", "ModificationUserID", "ModificationDate", };

                clsBusinessPartner clsBusinessPartner = new clsBusinessPartner();
                dt = clsBusinessPartner.SelectBusinessPartner(0,1, "", "",-1, Simulate.Integer32(CompanyID));

            }
            else if (tableName == "tbl_Vendors")
            {
                hiddenCols = new List<string> { "ID", "CompanyID", "CreationUserID",  "Nationality" ,"Type", "CreationDate", "ModificationUserID", "ModificationDate", };
                clsBusinessPartner clsBusinessPartner = new clsBusinessPartner();
                dt = clsBusinessPartner.SelectBusinessPartner(0, 2, "", "", -1, Simulate.Integer32(CompanyID));

            }
             



            var fileBytes = GenerateExcelFromDataTable( tableName, dt,
    hiddenCols);
            var fileName = $"{tableName}_Template.xlsx";
            return File(fileBytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
        }
        //private List<ColumnDefinition> GetColumnDefinitions(string tableName)
        //{
        //    var columns = new List<ColumnDefinition>();

        //    //switch (tableName )
        //    //{
        //    //    case "tbl_Banks":
        //    //        columns.Add(new ColumnDefinition("AName", SQLColumnDataType.VarChar));
        //    //        columns.Add(new ColumnDefinition("EName", SQLColumnDataType.VarChar));
        //    //        columns.Add(new ColumnDefinition("AccountNumber", SQLColumnDataType.VarChar));
               




        //    //        // Add more columns as needed
        //    //        break;

        //    //    // case "OTHER_TABLE":
        //    //    //     columns.Add(new ColumnDefinition("Column1", typeof(int)));
        //    //    //     columns.Add(new ColumnDefinition("Column2", typeof(string)));
        //    //    //     break;

        //    //    default:
        //    //        // Handle unknown table (throw exception or return empty list)
        //    //        throw new ArgumentException($"No column definitions found for table: {tableName}");
        //    //}

        //    return columns;








        //}
        [HttpGet("GenerateExcelTemplate/{tableName}")]
        public static byte[] GenerateExcelFromDataTable(
        string tableName,
        DataTable data,
        List<string> columnsToHide = null)
        {
            // Ensure we have something to export
            if (data == null || data.Columns.Count == 0)
                throw new ArgumentException("DataTable is null or has no columns.");

            // Create an in-memory ClosedXML workbook
            using var workbook = new XLWorkbook();
            var worksheet = workbook.Worksheets.Add(tableName + " Template");

            // 1) Write the header row in row #1
            var headerRow = worksheet.Row(1);
            for (int colIndex = 0; colIndex < data.Columns.Count; colIndex++)
            {
                string colName = data.Columns[colIndex].ColumnName;
                var cell = headerRow.Cell(colIndex + 1);
                cell.Value = colName;
            }

            // Make the header row bold
            headerRow.Style.Font.Bold = true;
            headerRow.Style.Font.FontSize = 16;
            headerRow.Style.Fill.PatternType = XLFillPatternValues.Solid;
            headerRow.Style.Fill.BackgroundColor = XLColor.LightGray;
            // 2) Freeze the top row
            worksheet.SheetView.FreezeRows(1);

            // 3) Write the data rows (start from row #2)
            int currentExcelRow = 2;
            foreach (DataRow row in data.Rows)
            {
                for (int colIndex = 0; colIndex < data.Columns.Count; colIndex++)
                {
                    worksheet.Cell(currentExcelRow, colIndex + 1)
                             .Value = row[colIndex]?.ToString();
                }
                currentExcelRow++;
            }
       
            // 4) Hide columns if specified
            if (columnsToHide != null && columnsToHide.Count > 0)
            {
                // For each DataTable column, check if it's in columnsToHide
                // If yes, hide that column in the Excel sheet
                for (int colIndex = 0; colIndex < data.Columns.Count; colIndex++)
                {
                    string colName = data.Columns[colIndex].ColumnName;
                    if (columnsToHide.Contains(colName))
                    {
                        // Hide the entire column (1-based index in Excel)
                        worksheet.Column(colIndex + 1).Hide();
                    }
                }
            }

            // 5) Auto-fit columns (the visible ones)
            // Safely adjust columns:
            var usedCols = worksheet.ColumnsUsed().ToList();

            foreach (var col in usedCols)
            {
                try
                {
                    col.AdjustToContents();
                }
                catch (Exception ex)
                {
                 //   Console.WriteLine($"Error on column {col.ColumnNumber()}: {ex.Message}");
                }
            }

            //////////////////////
            worksheet.Range(1, 1, 1, data.Columns.Count)
           .Style
           .Protection
           .SetLocked(true);
            int lastUsedRow = worksheet.LastRowUsed().RowNumber();
            worksheet.Range(2, 1, 1000000, data.Columns.Count)
         .Style
         .Protection
         .SetLocked(false);
            // (C) Protect the worksheet
            var protection = worksheet.Protect("OptionalPassword");
            // (D) Configure what a user can/cannot select



            // Add hidden sheet 
            // Add hidden metadata sheet
            var metaSheet = workbook.Worksheets.Add("SystemMetadata");
            metaSheet.Visibility = XLWorksheetVisibility.Hidden;

            // Create a unique signature
            var signature = Guid.NewGuid().ToString(); // Or a hashed string
            metaSheet.Cell("A1").Value = signature;
            metaSheet.Cell("A2").Value = tableName;
            // Optionally protect the metadata sheet
            metaSheet.Protect("MetadataPassword");








            // 6) Return the workbook as a byte array
            using var ms = new MemoryStream();
            workbook.SaveAs(ms);
            return ms.ToArray();
        }
    }

    public class ColumnDefinition
        {
            public string ColumnName { get; set; }
            public SQLColumnDataType DataType { get; set; }

            public ColumnDefinition(string columnName, SQLColumnDataType dataType)
            {
                ColumnName = columnName;
                DataType = dataType;
            }
        }


}
 
