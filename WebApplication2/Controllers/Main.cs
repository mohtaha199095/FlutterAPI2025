using ClosedXML.Excel;
using FastReport;
using FastReport.Web;
using FastReport.Export;
using FastReport.Export.PdfSimple;
 
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Reflection.PortableExecutable;
using WebApplication2.cls;
using WebApplication2.cls.Reports;
using WebApplication2.DataBaseTable;
using WebApplication2.DataSet;
using WebApplication2.MainClasses;
using static WebApplication2.MainClasses.clsEnum;
using Swashbuckle.AspNetCore.SwaggerGen;
using Microsoft.CodeAnalysis.VisualBasic.Syntax;
using Microsoft.AspNetCore.Http;
using DocumentFormat.OpenXml.Bibliography;
using DocumentFormat.OpenXml.Spreadsheet;
using System.Xml;
using System.Xml.Linq;
using DocumentFormat.OpenXml.EMMA;

namespace WebApplication2.Controllers
{
    // [EnableCors]
    [ApiController]
    [Route("[controller]")]
    public class Main : Controller
    {

        public IActionResult Index()
        {
            string a = "asd";
            return Json(a);
        }
        #region Employee


        [HttpGet]
        [Route("CheckLogin")]
        public string CheckLogin(string UserName, string Password)
        {
            try
            {
                string JSONString = string.Empty;
                if (Simulate.String(UserName) == "")
                {
                    return JSONString;

                }
                if (Simulate.String(Password) == "")
                {
                    return JSONString;

                }
                clsEmployee clsEmployee = new clsEmployee();
                DataTable dt = clsEmployee.SelectEmployee(0, "", "", Simulate.String(UserName), Simulate.String(Password), 0);
                if (dt != null && dt.Rows.Count > 0)
                {


                    JSONString = JsonConvert.SerializeObject(dt);
                    return JSONString;
                }
                else
                {

                    return JSONString;
                }
            }
            catch (Exception ex)
            {

                return ex.Message;
            }




        }
        [HttpGet]
        [Route("SelectEmployeesByID")]
        public string SelectEmployeesByID(int ID, string UserName, string Password, int CompanyId)
        {
            try
            {
                clsEmployee clsEmployee = new clsEmployee();
                DataTable dt = clsEmployee.SelectEmployee(ID, "", "", Simulate.String(UserName), Simulate.String(Password), CompanyId);
                if (dt != null)
                {

                    string JSONString = string.Empty;
                    JSONString = JsonConvert.SerializeObject(dt);
                    return JSONString;
                }
                else
                {

                    return "";
                }
            }
            catch (Exception)
            {

                throw;
            }




        }
        [HttpGet]
        [Route("DeleteEmployeeByID")]
        public bool DeleteEmployeeByID(int ID)
        {
            try
            {
                clsJournalVoucherDetails clsJournalVoucherDetails = new clsJournalVoucherDetails();
                DataTable dt = clsJournalVoucherDetails.SelectJournalVoucherDetailsByParentId("", 0, 0, 0, 0, ID);
                if (dt != null && dt.Rows.Count > 0)
                {

                    return false;
                }
                clsEmployee clsEmployee = new clsEmployee();
                bool A = clsEmployee.DeleteEmployeeByID(ID);
                return A;
            }
            catch (Exception)
            {

                throw;
            }

        }
        [HttpGet]
        [Route("InsertEmployee")]
        public int InsertEmployee(string AName, string EName, string UserName, string Password, int CompanyID, int CreationUserId)
        {
            try
            {
                clsEmployee clsEmployee = new clsEmployee();
                int A = clsEmployee.InsertEmployee(Simulate.String(AName), Simulate.String(EName), Simulate.String(UserName), Simulate.String(Password), Simulate.Integer32(CompanyID), CreationUserId);
                return A;
            }
            catch (Exception ex)
            {

                throw;
            }

        }
        [HttpGet]
        [Route("UpdateEmployee")]
        public int UpdateEmployee(string AName, string EName, string UserName, string Password, int ID, int ModificationUserId)
        {
            try
            {
                clsEmployee clsEmployee = new clsEmployee();
                int A = clsEmployee.UpdateEmployee(Simulate.String(AName), Simulate.String(EName), Simulate.String(UserName), Simulate.String(Password), ID, ModificationUserId);
                return A;
            }
            catch (Exception)
            {

                throw;
            }

        }
        #endregion
        #region Company


        [HttpGet]
        [Route("SelectCompanyByID")]
        public string SelectCompanyByID(int ID)
        {
            try
            {
                clsCompany clsCompany = new clsCompany();
                DataTable dt = clsCompany.SelectCompany(ID, "", "", "");
                if (dt != null)
                {


                    string JSONString = string.Empty;
                    JSONString = JsonConvert.SerializeObject(dt);
                    return JSONString;
                }
                else
                {

                    return "";
                }
            }
            catch (Exception)
            {

                throw;
            }




        }
        [HttpGet]
        [Route("DeleteCompanyByID")]
        public bool DeleteCompanyByID(int ID)
        {
            try
            {
                clsCompany clsCompany = new clsCompany();
                bool A = clsCompany.DeleteCompanyByID(ID);
                return A;
            }
            catch (Exception)
            {

                throw;
            }

        }
        [HttpPost]
        [Route("InsertCompany")]
        public int InsertCompany(string UserName, string Password, string AName, string EName, string Email
            , string Address, string Tel1, string Tel2, string ContactPerson,
            string ContactNumber, [FromBody] string Logo, string TradeName)
        {
            try
            {
                byte[] myLogo = new Byte[64];
                if (Logo != null && Logo.Length > 0)
                {
                    myLogo = Convert.FromBase64String(Logo);
                }
                else
                {

                    myLogo = null;
                }

                clsCompany clsCompany = new clsCompany();





                SqlTransaction trn;
                clsSQL clsSQL = new clsSQL();
                SqlConnection con = new SqlConnection(clsSQL.conString);
                con.Open();
                trn = con.BeginTransaction();
                int A = 0;
                bool IsSaved = true;
                try
                {
                    A = clsCompany.InsertCompany(Simulate.String(AName), Simulate.String(EName), Simulate.String(Email)
            , Simulate.String(Address), Simulate.String(Tel1), Simulate.String(Tel2), Simulate.String(ContactPerson),
              Simulate.String(ContactNumber), myLogo, Simulate.String(TradeName));
                    if (A > 0)
                    {
                        clsEmployee clsEmployee = new clsEmployee();
                        int b = clsEmployee.InsertEmployee(Simulate.String(AName), Simulate.String(EName), Simulate.String(UserName), Simulate.String(Password), A, 0);
                        if (b == 0)
                        {
                            A = 0;
                        }
                    }

                    if (A == 0)
                        IsSaved = false;


                    if (IsSaved)
                        trn.Commit();
                    else
                        trn.Rollback();
                }
                catch (Exception)
                {
                    trn.Rollback();

                }
                finally { con.Close(); }




                return A;
            }
            catch (Exception ex)
            {

                throw;
            }

        }
        [HttpPost]
        [Route("UpdateCompany")]
        public int UpdateCompany(int ID, string AName, string EName, string Email
            , string Address, string Tel1, string Tel2, string ContactPerson,
            string ContactNumber, [FromBody] string Logo, string TradeName, int ModificationUserId)
        {
            try
            {
                byte[] myLogo = new Byte[64];
                if (Logo != null && Logo.Length > 0)
                {
                    myLogo = Convert.FromBase64String(Logo);
                    // myLogo = Encoding.ASCII.GetBytes(Logo);
                }
                else
                {

                    myLogo = null;
                }
                clsCompany clsCompany = new clsCompany();
                int A = clsCompany.UpdateCompany(ID, Simulate.String(AName), Simulate.String(EName), Simulate.String(Email)
            , Simulate.String(Address), Simulate.String(Tel1), Simulate.String(Tel2), Simulate.String(ContactPerson),
             Simulate.String(ContactNumber), myLogo, Simulate.String(TradeName), ModificationUserId);
                return A;
            }
            catch (Exception)
            {

                throw;
            }

        }
        #endregion
        #region Branch


        [HttpGet]
        [Route("SelectBranchByID")]
        public string SelectBranchByID(int ID, int CompanyID)
        {
            try
            {
                clsBranch clsBranch = new clsBranch();
                DataTable dt = clsBranch.SelectBranch(ID, "", "", CompanyID);
                if (dt != null)
                {

                    string JSONString = string.Empty;
                    JSONString = JsonConvert.SerializeObject(dt);
                    return JSONString;
                }
                else
                {

                    return "";
                }
            }
            catch (Exception)
            {

                throw;
            }




        }
        [HttpGet]
        [Route("DeleteBranchByID")]
        public bool DeleteBranchByID(int ID)
        {
            try
            {
                clsJournalVoucherDetails clsJournalVoucherDetails = new clsJournalVoucherDetails();
                DataTable dt = clsJournalVoucherDetails.SelectJournalVoucherDetailsByParentId("", 0,0, ID, 0, 0); 
                if (dt != null && dt.Rows.Count > 0)
                {

                    return false;
                }
                clsBranch clsBranch = new clsBranch();
                bool A = clsBranch.DeleteBranchByID(ID);
                return A;
            }
            catch (Exception)
            {

                throw;
            }

        }
        [HttpGet]
        [Route("InsertBranch")]
        public int InsertBranch(string AName, string EName, int CompanyID, int CreationUserId)
        {
            try
            {
                clsBranch clsBranch = new clsBranch();
                int A = clsBranch.InsertBranch(AName, EName, CompanyID, CreationUserId);
                return A;
            }
            catch (Exception ex)
            {

                throw;
            }

        }
        [HttpGet]
        [Route("UpdateBranch")]
        public int UpdateBranch(int ID, string AName, string EName, int ModificationUserId)
        {
            try
            {
                clsBranch clsBranch = new clsBranch();
                int A = clsBranch.UpdateBranch(ID, AName, EName, ModificationUserId);
                return A;
            }
            catch (Exception)
            {

                throw;
            }

        }
        #endregion
        #region CostCenter


        [HttpGet]
        [Route("SelectCostCentersByID")]
        public string SelectCostCentersByID(int ID, int CompanyID)
        {
            try
            {
                clsCostCenter clsCostCenter = new clsCostCenter();
                DataTable dt = clsCostCenter.SelectCostCentersByID(ID, "", "", CompanyID);
                if (dt != null)
                {

                    string JSONString = string.Empty;
                    JSONString = JsonConvert.SerializeObject(dt);
                    return JSONString;
                }
                else
                {

                    return "";
                }
            }
            catch (Exception)
            {

                throw;
            }




        }
        [HttpGet]
        [Route("DeleteCostCenterByID")]
        public bool DeleteCostCenterByID(int ID)
        {
            try
            {
                clsJournalVoucherDetails clsJournalVoucherDetails = new clsJournalVoucherDetails();
                DataTable dt = clsJournalVoucherDetails.SelectJournalVoucherDetailsByParentId("", 0, 0, 0,  ID, 0);
                if (dt != null && dt.Rows.Count > 0)
                {

                    return false;
                }
                clsCostCenter clsCostCenter = new clsCostCenter();
                bool A = clsCostCenter.DeleteCostCenterByID(ID);
                return A;
            }
            catch (Exception)
            {

                throw;
            }

        }
        [HttpGet]
        [Route("InsertCostCenter")]
        public int InsertCostCenter(string AName, string EName, int CompanyID, int CreationUserId)
        {
            try
            {
                clsCostCenter clsCostCenter = new clsCostCenter();
                int A = clsCostCenter.InsertCostCenter(AName, EName, CompanyID, CreationUserId);
                return A;
            }
            catch (Exception ex)
            {

                throw;
            }

        }
        [HttpGet]
        [Route("UpdateCostCenter")]
        public int UpdateCostCenter(int ID, string AName, string EName, int ModificationUserId)
        {
            try
            {
                clsCostCenter clsCostCenter = new clsCostCenter();
                int A = clsCostCenter.UpdateCostCenter(ID, AName, EName, ModificationUserId);
                return A;
            }
            catch (Exception)
            {

                throw;
            }

        }
        #endregion
        #region Items Category


        [HttpGet]
        [Route("SelectItemsCategoryByID")]
        public string SelectItemsCategoryByID(int ID, int CompanyId)
        {
            try
            {
                clsItemsCategory clsItemsCategory = new clsItemsCategory();
                DataTable dt = clsItemsCategory.SelectItemsCategory(ID, "", "", CompanyId);
                if (dt != null)
                {

                    string JSONString = string.Empty;
                    JSONString = JsonConvert.SerializeObject(dt);
                    return JSONString;
                }
                else
                {

                    return "";
                }
            }
            catch (Exception)
            {

                throw;
            }




        }
        [HttpGet]
        [Route("DeleteItemsCategoryByID")]
        public bool DeleteItemsCategoryByID(int ID)
        {
            try
            {
                clsItemsCategory clsItemsCategory = new clsItemsCategory();
                bool A = clsItemsCategory.DeleteItemsCategoryByID(ID);
                return A;
            }
            catch (Exception)
            {

                throw;
            }

        }
        [HttpGet]
        [Route("InsertItemsCategory")]
        public int InsertItemsCategory(string AName, string EName, int CompanyID, int CreationUserId)
        {
            try
            {
                clsItemsCategory clsItemsCategory = new clsItemsCategory();
                int A = clsItemsCategory.InsertItemsCategory(AName, EName, CompanyID, CreationUserId);
                return A;
            }
            catch (Exception ex)
            {

                throw;
            }

        }
        [HttpGet]
        [Route("UpdateItemsCategory")]
        public int UpdateItemsCategory(int ID, string AName, string EName, int ModificationUserId)
        {
            try
            {
                clsItemsCategory clsItemsCategory = new clsItemsCategory();
                int A = clsItemsCategory.UpdateItemsCategory(ID, AName, EName, ModificationUserId);
                return A;
            }
            catch (Exception)
            {

                throw;
            }

        }
        #endregion
        #region journal Voucher


        [HttpGet]
        [Route("SelectJournalVoucherHeadersByID")]
        public string SelectJournalVoucherHeadersByID(string Guid, int BranchID, int CostCenterID, string Notes, string JVNumber, int JVTypeID, int CompanyID, string Date1, string Date2)
        {
            try
            {
                clsJournalVoucherHeader clsJournalVoucherHeader = new clsJournalVoucherHeader();
                DataTable dt = clsJournalVoucherHeader.SelectJournalVoucherHeader(Simulate.String(Guid), BranchID, CostCenterID, Simulate.String(Notes), Simulate.String(JVNumber), JVTypeID, CompanyID, Simulate.StringToDate(Date1), Simulate.StringToDate(Date2));
                if (dt != null)
                {

                    string JSONString = string.Empty;
                    JSONString = JsonConvert.SerializeObject(dt);
                    return JSONString;
                }
                else
                {

                    return "";
                }
            }
            catch (Exception)
            {

                throw;
            }




        }
        [HttpGet]
        [Route("DeleteJournalVoucherHeadersByID")]
        public bool DeleteJournalVoucherHeadersByID(string Guid)
        {
            try
            {
                clsJournalVoucherDetails clsJournalVoucherDetails = new clsJournalVoucherDetails();
                clsJournalVoucherHeader clsJournalVoucherHeader = new clsJournalVoucherHeader();

                SqlTransaction trn; clsSQL clsSQL = new clsSQL();
                SqlConnection con = new SqlConnection(clsSQL.conString);
                con.Open();
                trn = con.BeginTransaction(); int A = 0;
                bool IsSaved = true;
                try
                {
                    IsSaved = clsJournalVoucherHeader.DeleteJournalVoucherHeaderByID(Guid, trn);
                    bool a = clsJournalVoucherDetails.DeleteJournalVoucherDetailsByParentId(Guid, trn);
                    if (!a)
                        IsSaved = false;


                    if (IsSaved)
                        trn.Commit();
                    else
                        trn.Rollback();
                }
                catch (Exception)
                {
                    trn.Rollback();

                }
                finally { con.Close(); }


                return IsSaved;
            }
            catch (Exception)
            {

                throw;
            }

        }
        [HttpPost]
        [Route("InsertJournalVoucherHeader")]

        public string InsertJournalVoucherHeader(int BranchID, int CostCenterID, string Notes, string JVNumber, int JVTypeID, [FromBody] string DetailsList, int CompanyID, DateTime VoucherDate, int CreationUserId)

        {
            try
            {

                List<tbl_JournalVoucherDetails> details = JsonConvert.DeserializeObject<List<tbl_JournalVoucherDetails>>(DetailsList);
                clsJournalVoucherHeader clsJournalVoucherHeader = new clsJournalVoucherHeader();
                clsJournalVoucherDetails clsDetails = new clsJournalVoucherDetails();
                SqlTransaction trn; clsSQL clsSQL = new clsSQL();
                SqlConnection con = new SqlConnection(clsSQL.conString);
                con.Open();
                trn = con.BeginTransaction(); string A = "";
                try
                {
                    bool IsSaved = true;
                    DataTable dt = clsJournalVoucherHeader.SelectMaxJVNo("", JVTypeID, CompanyID, trn);
                    if (dt != null && dt.Rows.Count > 0)
                    {
                        JVNumber = Simulate.String( Simulate.Integer32(dt.Rows[0][0]) + 1 );
                    }
                    else {
                        JVNumber = "1";
                    }
                     
                        A = clsJournalVoucherHeader.InsertJournalVoucherHeader(BranchID, CostCenterID, Simulate.String(Notes), JVNumber, JVTypeID, CompanyID, VoucherDate, CreationUserId, trn);
                    if (A == "") IsSaved = false;
                    for (int i = 0; i < details.Count; i++)
                    {
                        string c = clsDetails.InsertJournalVoucherDetails(A, i, details[i].AccountID, details[i].SubAccountID, details[i].Debit, details[i].Credit
                              , details[i].Total, details[i].BranchID, details[i].CostCenterID, details[i].DueDate, details[i].Note, details[i].CompanyID
                              , details[i].CreationUserID, trn);
                        if (c == "")
                            IsSaved = false;
                    }

                    if (!clsJournalVoucherHeader.CheckJVMatch(A, trn))
                    {
                        IsSaved = false;
                        A = "";
                    }
                    if (IsSaved)
                        trn.Commit();
                    else
                        trn.Rollback();
                    return A;
                }
                catch (Exception)
                {

                    trn.Rollback();
                    return "";
                }
                finally { con.Close(); }

            }
            catch (Exception ex)
            {

                return "";
            }

        }
        [Route("UpdateJournalVoucherHeader")]
        public string UpdateJournalVoucherHeader(int BranchID, int CostCenterID, string Notes, string JVNumber, int JVTypeID, [FromBody] string DetailsList, int CompanyID, DateTime VoucherDate, string Guid, int ModificationUserId)
        {
            try
            {

                List<tbl_JournalVoucherDetails> details = JsonConvert.DeserializeObject<List<tbl_JournalVoucherDetails>>(DetailsList);
                clsJournalVoucherHeader clsJournalVoucherHeader = new clsJournalVoucherHeader();
                clsJournalVoucherDetails clsDetails = new clsJournalVoucherDetails();
                SqlTransaction trn; clsSQL clsSQL = new clsSQL();
                SqlConnection con = new SqlConnection(clsSQL.conString);
                con.Open();
                trn = con.BeginTransaction();
                string A = "";
                try
                {
                    bool IsSaved = true;

                    A = clsJournalVoucherHeader.UpdateJournalVoucherHeader(BranchID, CostCenterID, Simulate.String(Notes), JVNumber, JVTypeID, VoucherDate, Guid, ModificationUserId, trn);
                    clsDetails.DeleteJournalVoucherDetailsByParentId(Guid, trn);
                    for (int i = 0; i < details.Count; i++)
                    {
                        string c = clsDetails.InsertJournalVoucherDetails(Guid, i, details[i].AccountID, details[i].SubAccountID, details[i].Debit, details[i].Credit
                              , details[i].Total, details[i].BranchID, details[i].CostCenterID, details[i].DueDate, details[i].Note, details[i].CompanyID
                              , details[i].CreationUserID, trn);
                        if (c == "")
                            IsSaved = false;
                    }
                    if (!clsJournalVoucherHeader.CheckJVMatch(Guid, trn))
                    {
                        IsSaved = false;
                        A = "";
                    }
                    if (IsSaved)
                        trn.Commit();
                    else
                    {
                        trn.Rollback();
                        A = "";
                    }
                }
                catch (Exception)
                {
                    A = "";
                    trn.Rollback();
                }
                finally { con.Close(); }
                return A;
            }
            catch (Exception)
            {

                throw;
            }

        }
        #endregion
        #region Voucher Details

        [HttpGet]
        [Route("SelectJournalVoucherDetailsByParentId")]
        public string SelectJournalVoucherDetailsByParentId(string ParentGuid, int AccountID, int SubAccountID)
        {
            try
            {
                clsJournalVoucherDetails clsJournalVoucherDetails = new clsJournalVoucherDetails();
                DataTable dt = clsJournalVoucherDetails.SelectJournalVoucherDetailsByParentId(ParentGuid, AccountID, SubAccountID,0, 0, 0);
                if (dt != null)
                {

                    string JSONString = string.Empty;
                    JSONString = JsonConvert.SerializeObject(dt);
                    return JSONString;
                }
                else
                {

                    return "";
                }
            }
            catch (Exception)
            {

                throw;
            }




        }
        #endregion
        #region Accounts


        [HttpGet]
        [Route("SelectAccountsByID")]
        public string SelectAccountsByID(int Id, int ParentID, string AccountNumber, string AName, string EName, int CompanyID)
        {
            try
            {
                clsAccounts clsAccounts = new clsAccounts();
                DataTable dt = clsAccounts.SelectAccountsByID(Id, ParentID, Simulate.String(AccountNumber), Simulate.String(AName), Simulate.String(EName), CompanyID);
                if (dt != null)
                {

                    string JSONString = string.Empty;
                    JSONString = JsonConvert.SerializeObject(dt);
                    return JSONString;
                }

                else
                {
                    return "";
                }
            }
            catch (Exception)
            {

                throw;
            }
        }
        [HttpGet]
        [Route("SelectTransactionAccounts")]
        public string SelectTransactionAccounts(int CompanyID)
        {
            try
            {
                clsAccounts clsAccounts = new clsAccounts();
                DataTable dt = clsAccounts.SelectTransactionAccounts(CompanyID);
                if (dt != null)
                {

                    string JSONString = string.Empty;
                    JSONString = JsonConvert.SerializeObject(dt);
                    return JSONString;
                }

                else
                {
                    return "";
                }
            }
            catch (Exception ex)
            {

                throw;
            }
        }
        [HttpGet]
        [Route("DeleteAccountsByID")]
        public string DeleteAccountsByID(int ID)
        {
            try
            {
                string msg = "Failed to delete this record";
                bool A = false;
                clsJournalVoucherDetails clsJournalVoucherDetails = new clsJournalVoucherDetails();
                clsAccounts clsAccounts = new clsAccounts();
                SqlTransaction trn; clsSQL clsSQL = new clsSQL();
                SqlConnection con = new SqlConnection(clsSQL.conString);
                con.Open();
                trn = con.BeginTransaction();
                try
                {

                    DataTable dt = clsJournalVoucherDetails.SelectJournalVoucherDetailsByParentId("", ID, 0,0,0,0);
                    if (dt != null && dt.Rows.Count > 0)
                    {
                        msg = "this account is used , cant delete used account";
                    }
                    else
                    {
                        A = clsAccounts.DeleteAccountsByID(ID); msg = "account deleted successfully";

                    }



                    if (A)
                    {

                        trn.Commit();
                    }
                    else
                    {
                        trn.Rollback();
                    }
                }
                catch (Exception)
                {

                    trn.Rollback();
                }
                con.Close();
                return JsonConvert.SerializeObject(msg)  ;

            }
            catch (Exception)
            {

                throw;
            }

        }
        [HttpGet]
        [Route("InsertAccounts")]
        public int InsertAccounts(int ParentID, string AccountNumber, string AName, string EName, int ReportingTypeID,int ReportingTypeNodeID, int AccountNatureID, int CompanyID, int CreationUserId)
        {
            try
            {
                clsAccounts clsAccounts = new clsAccounts();
                int A = clsAccounts.InsertAccounts(ParentID, AccountNumber, AName, EName, ReportingTypeID, ReportingTypeNodeID, AccountNatureID, CompanyID, CreationUserId);
                return A;
            }
            catch (Exception ex)
            {

                throw;
            }

        }
        [HttpGet]
        [Route("UpdateAccounts")]
        public int UpdateAccounts(int ID, int ParentID, string AccountNumber, string AName, string EName, int ReportingTypeID,int ReportingTypeNodeID, int AccountNatureID, int ModificationUserId)
        {
            try
            {
                clsAccounts clsAccounts = new clsAccounts();
                int A = clsAccounts.UpdateAccounts(ID, ParentID, AccountNumber, AName, EName, ReportingTypeID, ReportingTypeNodeID,AccountNatureID, ModificationUserId);
                return A;
            }
            catch (Exception)
            {

                throw;
            }

        }
        #endregion
        #region Account Nature 
        [HttpGet]
        [Route("SelectAccountNatureByID")]
        public string SelectAccountNatureByID(int Id, int CompanyID)
        {
            try
            {
                clsAccountNature clsAccountNature = new clsAccountNature();
                DataTable dt = clsAccountNature.SelectAccountNatureByID(Id, CompanyID);
                if (dt != null && dt.Rows.Count > 0)
                {

                    string JSONString = string.Empty;
                    JSONString = JsonConvert.SerializeObject(dt);
                    return JSONString;
                }
                else
                {

                    return "";
                }
            }
            catch (Exception)
            {

                throw;
            }




        }

        #endregion
        #region ReportingType 
        [HttpGet]
        [Route("SelectReportingTypeByID")]
        public string SelectReportingTypeByID(int Id, int CompanyID)
        {
            try
            {
                clsReportingType clsReportingType = new clsReportingType();
                DataTable dt = clsReportingType.SelectReportingTypeByID(Id, CompanyID);
                if (dt != null && dt.Rows.Count > 0)
                {

                    string JSONString = string.Empty;
                    JSONString = JsonConvert.SerializeObject(dt);
                    return JSONString;
                }
                else
                {

                    return "";
                }
            }
            catch (Exception)
            {

                throw;
            }




        }

        #endregion
        #region Business Partner


        [HttpGet]
        [Route("SelectBusinessPartner")]
        public string SelectBusinessPartner(int ID, int Type, int Active, int CompanyID)
        {
            try
            {
                clsBusinessPartner clsBusinessPartner = new clsBusinessPartner();
                DataTable dt = clsBusinessPartner.SelectBusinessPartner(ID,  Type, "", "", Active, CompanyID);
                if (dt != null)
                {

                    string JSONString = string.Empty;
                    JSONString = JsonConvert.SerializeObject(dt);
                    return JSONString;
                }
                else
                {

                    return "";
                }
            }
            catch (Exception)
            {

                throw;
            }




        }
        [HttpGet]
        [Route("DeleteBusinessPartnerByID")]
        public bool DeleteBusinessPartnerByID(int ID)
        {
            try
            {
                clsBusinessPartner clsBusinessPartner = new clsBusinessPartner();

                clsJournalVoucherDetails clsJournalVoucherDetails = new clsJournalVoucherDetails();
                DataTable dt= clsJournalVoucherDetails.SelectJournalVoucherDetailsByParentId( "" ,0,ID,0,0,0);
                if (dt != null && dt.Rows.Count > 0) {

                    return false;
                }
                bool A = clsBusinessPartner.DeleteBusinessPartnerByID(ID);
                return A;
            }
            catch (Exception)
            {

                throw;
            }

        }
        [HttpGet]
        [Route("InsertBusinessPartner")]
        public int InsertBusinessPartner(string AName, string EName, string CommercialName
            , string Address, string Tel, string Active, string Limit,
            string Email, int Type, int CompanyID, int CreationUserId, string EmpCode,
            string StreetName, string HouseNumber, string NationalNumber, string PassportNumber, int Nationality, string IDNumber,string TaxNumber)
        {
            try
            {
                clsBusinessPartner clsBusinessPartner = new clsBusinessPartner();
                int A = clsBusinessPartner.InsertBusinessPartner(Simulate.String(AName), Simulate.String(EName), Simulate.String(CommercialName)
                    , Simulate.String(Address), Simulate.String(Tel), Simulate.Bool(Active), Simulate.Val(Limit),
             Simulate.String(Email), Type, CompanyID, CreationUserId
             , Simulate.String(EmpCode), Simulate.String(StreetName), Simulate.String(HouseNumber), Simulate.String(NationalNumber),
                Simulate.String(PassportNumber), Simulate.Integer32(Nationality), Simulate.String(IDNumber), Simulate.String(TaxNumber));
                return A;
            }
            catch (Exception ex)
            {

                throw;
            }

        }
        [HttpGet]
        [Route("UpdateBusinessPartner")]
        public int UpdateBusinessPartner(int ID, string AName, string EName, string CommercialName, string Address, 
            string Tel, string Active, string Limit,
            string Email, int Type, int ModificationUserId,
            string EmpCode, string StreetName, string HouseNumber, string NationalNumber, 
            string PassportNumber, int Nationality, string IDNumber,string TaxNumber)
        {
            try
            {
                clsBusinessPartner clsBusinessPartner = new clsBusinessPartner();
                int A = clsBusinessPartner.UpdateBusinessPartner(ID, Simulate.String(AName), Simulate.String(EName), Simulate.String(CommercialName)
                    , Simulate.String(Address), Simulate.String(Tel), Simulate.Bool(Active), Simulate.Val(Limit),
            Simulate.String(Email), Type, ModificationUserId
            , Simulate.String(EmpCode), Simulate.String(StreetName), Simulate.String(HouseNumber), Simulate.String(NationalNumber),
                Simulate.String(PassportNumber), Simulate.Integer32(Nationality), Simulate.String(IDNumber),Simulate.String(TaxNumber));
                return A;
            }
            catch (Exception)
            {

                throw;
            }

        }
        #endregion
        #region Reports
        [HttpGet("{menuId}/menuitems")]
        public IActionResult FastreporttoPDF(FastReport.Report report)
        {


            report.Report.Prepare();

            using (MemoryStream ms = new MemoryStream())
            {
                PDFSimpleExport pdfExport = new PDFSimpleExport();
                pdfExport.Export(report.Report, ms);
                ms.Flush();

                return File(ms.ToArray(), "application/pdf", Path.GetFileNameWithoutExtension("Master-Detail") + ".pdf");
            }





            //FastReport.Export.PdfSimple.PdfObjects.PdfPage pdfExport = new FastReport.Export.PdfSimple.PdfObjects.PdfPage  ;
            //// Set PDF export props  
            ////  FastReport.Export.Pdf.PDFExport pdfExport = new FastReport.Export.Pdf.PDFExport();


            //pdfExport.ShowProgress = false;
            //pdfExport.Subject = "Subject";
            //pdfExport.Title = "Report";
            //pdfExport.Compressed = true;
            //pdfExport.AllowPrint = true;
            //pdfExport.EmbeddingFonts = true;

            //MemoryStream strm = new MemoryStream();
            //report.Report.Export(pdfExport, strm);
            //report.Dispose();
            //pdfExport.Dispose();
            //strm.Position = 0;
            // return pdfExport;


        }
        [HttpGet("{menuId}/menuitems")]
        public ActionResult Fastreporttoxls( DataTable ds)
        {

            //var grdReport = new System.Web.UI.WebControls.GridView();

            //grdReport.DataSource = ds;

            //grdReport.DataBind();

            using (XLWorkbook wb = new XLWorkbook())
            {
                ds.TableName = "s";
                wb.Worksheets.Add(ds);
                using (MemoryStream stream = new MemoryStream())
                {
                    wb.SaveAs(stream);
                    return File(stream.ToArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "Grid.xlsx");
                }
            }
          //  //    report.Report.Prepare();

          //  //  WebReport webReport = new WebReport();

          //  System.Data.DataSet dataSet = new System.Data.DataSet();

          //  dataSet.ReadXml("C://Program Files (x86)//FastReports//FastReport.Net//Demos//Reports//nwind.xml");

          //  WebReport.Report.RegisterData(dataSet, "NorthWind");

          //  WebReport.Report.Load("C://Program Files (x86)//FastReports//FastReport.Net//Demos//Reports//Simple List.frx");
          //  ExportBase a = new ExportBase();  
          ////  Excel2007Export excelExport = new Excel2007Export();
          // // MemoryStream stream = new MemoryStream();
          ////  WebReport.Report.Export(excelExport, stream);
          //  //  report.Report.ExportExcel2007();
          ////  WebReport.Report.Export";
          //  return "";

        }
        [HttpGet("{menuId}/menuitems")]
        public FileContentResult FastreporttoCSV([FromBody] List <DataTable>    ds, [FromQuery] List<String> SheetName, [FromQuery] List<String> ColumnType)
        {

 

            using (XLWorkbook wb = new XLWorkbook())
            {
                //ds.TableName = SheetName;
                //
                for (int iii = 0; iii < ds.Count; iii++)
                {
                    wb.Worksheets.Add(SheetName[iii]);

                
             
                    for (int ii = 0; ii < ds[iii].Columns.Count; ii++)
                {
                    wb.Worksheet(SheetName[iii]).Cell(1, ii + 1).Value = Simulate.String(ds[iii].Columns[ii].ColumnName);






                        //-----------------



                        //------------------
                        if (ColumnType.Count > ii)
                        {
                            if (ColumnType[ii] == "int")
                            {
                                for (int i = 0; i < ds[iii].Rows.Count; i++)
                                {
                                    wb.Worksheet(SheetName[iii]).Cell(i + 2, ii + 1).Value = Simulate.Integer32(ds[iii].Rows[i][ii]);
                                }
                            }
                            else
                            {
                                for (int i = 0; i < ds[iii].Rows.Count; i++)
                                {
                                   wb.Worksheet(SheetName[iii]).Cell(i + 2, ii + 1).Value = Simulate.String(ds[iii].Rows[i][ii]);
                                }
                            }

                        }
                        else
                        {
                            for (int i = 0; i < ds[iii].Rows.Count; i++)
                            {
                                wb.Worksheet(SheetName[iii]).Cell(i + 2, ii + 1).Value = Simulate.String(ds[iii].Rows[i][ii]);
                            }
                        }
                      

                     
                }

                  


                }


                using (MemoryStream stream = new MemoryStream())
                {
                    wb.SaveAs(stream);
                    return File(stream.ToArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "Grid.CSV");
                }
            }
        

        }
        [HttpGet("{menuId}/menuitems")]
        private void FastreportStanderdParameters(FastReport.Report Report, int UserID, int CompantID)
        {
            clsCompany clsCompany = new clsCompany();
            DataTable dt = clsCompany.SelectCompany(CompantID, "", "", "");
            if (dt != null)
            {

                Report.SetParameterValue("Standerd.CompanyName", Simulate.String(dt.Rows[0]["AName"]));
                Report.SetParameterValue("Standerd.Address", Simulate.String(dt.Rows[0]["Address"])); try
                {
                    FastReport.PictureObject Logo = (FastReport.PictureObject)Report.FindObject("CompanyLogo"); 
                    if (Logo!=null&&Simulate.String(dt.Rows[0]["Logo"]) != "")
                    {

                        Logo.Image = Simulate.StringToImg((byte[])dt.Rows[0]["Logo"]);
                        Report.SetParameterValue("Standerd.Logo", Simulate.StringToImg((byte[])dt.Rows[0]["Logo"]));
                    }
                }
                catch (Exception)
                {

                
                }
        
               
            }
            clsEmployee clsEmployee = new clsEmployee();
            DataTable dtemp = clsEmployee.SelectEmployee(UserID, "", "", "", "", CompantID);
            if (dtemp != null && dtemp.Rows.Count > 0)
            {
                Report.SetParameterValue("Standerd.User", Simulate.String(dtemp.Rows[0]["AName"]));
            }

            Report.SetParameterValue("Standerd.PrintDate", DateTime.Now.ToString("yyyy-MM-dd"));
            Report.SetParameterValue("Standerd.PrintTime", Simulate.String(Simulate.TimeString(DateTime.Now)));

        }
        #region Trial Balance
        [HttpGet]
        [Route("SelectTrialBalance")]
        public string SelectTrialBalance(DateTime Date1, DateTime Date2, int BranchID, int CostCenterID, int CompanyID)
        {
            try
            {
                clsReports clsReports = new clsReports();
                DataTable dt = clsReports.SelectTrialBalance(Date1, Date2, BranchID, CostCenterID, CompanyID);
                if (dt != null)
                {
                    string JSONString = string.Empty;
                    JSONString = JsonConvert.SerializeObject(dt);
                    return JSONString;
                }
                else
                {

                    return "";
                }
            }
            catch (Exception)
            {

                throw;
            }




        }

        [HttpGet]
        [Route("SelectTrialBalancePDF")]
        public IActionResult SelectTrialBalancePDF(DateTime Date1, DateTime Date2, int BranchID, int CostCenterID, int UserId, int CompanyID)
        {
            try
            {

                FastReport.Utils.Config.WebMode = true;
                clsReports clsReports = new clsReports();
                DataTable dt = clsReports.SelectTrialBalance(Date1, Date2, BranchID, CostCenterID, CompanyID);

                dsTrialBalance ds = new dsTrialBalance();

                if (dt != null && dt.Rows.Count > 0)
                {
                    for (int i = 0; i < dt.Rows.Count; i++)
                    {
                        ds.TrialBalance.Rows.Add();

                        ds.TrialBalance.Rows[i]["id"] = dt.Rows[i]["id"];
                        ds.TrialBalance.Rows[i]["AccountNumber"] = dt.Rows[i]["AccountNumber"];
                        ds.TrialBalance.Rows[i]["AName"] = dt.Rows[i]["AName"];
                        ds.TrialBalance.Rows[i]["EName"] = dt.Rows[i]["EName"];
                        ds.TrialBalance.Rows[i]["OpeningBalance"] = Simulate.decimal_(dt.Rows[i]["OpeningBalance"]);
                        ds.TrialBalance.Rows[i]["Debit"] = Simulate.decimal_(dt.Rows[i]["Debit"]);
                        ds.TrialBalance.Rows[i]["Credit"] = Simulate.decimal_(dt.Rows[i]["Credit"]);
                        ds.TrialBalance.Rows[i]["EndingBalance"] = Simulate.decimal_(dt.Rows[i]["EndingBalance"]);
                    }
                }





                FastReport.Report report = new FastReport.Report();
                report.RegisterData(ds);


                 string MyPath = getMyPath("rptTrialBalance", CompanyID);
                report.Load(MyPath); if (BranchID == 0)
                {
                    report.SetParameterValue("report.Branch", "All Branches");

                }
                else
                {
                    clsBranch clsBranch = new clsBranch();
                    DataTable dtBranch = clsBranch.SelectBranch(BranchID, "", "", 0);
                    if (dtBranch != null && dtBranch.Rows.Count > 0)
                    {
                        report.SetParameterValue("report.Branch", Simulate.String(dtBranch.Rows[0]["AName"]));

                    }
                }
                if (CostCenterID == 0)
                {
                    report.SetParameterValue("report.CostCenter", "All Cost Center");

                }
                else
                {
                    clsCostCenter clsCostCenter = new clsCostCenter();
                    DataTable dtCostCenter = clsCostCenter.SelectCostCentersByID(CostCenterID, "", "", 0);
                    if (dtCostCenter != null && dtCostCenter.Rows.Count > 0)
                    {
                        report.SetParameterValue("report.CostCenter", Simulate.String(dtCostCenter.Rows[0]["AName"]));

                    }
                }
                report.SetParameterValue("report.FromDate", Date1.ToString("yyyy-MM-dd"));
                report.SetParameterValue("report.ToDate", Date2.ToString("yyyy-MM-dd"));



                FastreportStanderdParameters(report, UserId, CompanyID);
                //    report.Prepare();

                report.Prepare();

                return FastreporttoPDF(report);
                //return Json(PrepareFrxReport(report), JsonRequestBehavior.AllowGet);


            }
            catch (Exception ex)
            {

                return Json(ex);
            }

        }




        #endregion
        #region  Balance Sheet  
        [HttpGet]
        [Route("SelectBalanceSheet")]
        public string SelectBalanceSheet(DateTime Date, string BranchID, string CostCenterID,
              string CompanyID)
        {
            try
            {
                
                clsReports clsReports = new clsReports();
                DataTable dt = clsReports.SelectBalanceSheet(  Date,   BranchID,   CostCenterID,
                   CompanyID);
                if (dt != null)
                {
                    string JSONString = string.Empty;
                    JSONString = JsonConvert.SerializeObject(dt);
                    return JSONString;
                }
                else
                {

                    return "";
                }
            }
            catch (Exception)
            {

                throw;
            }




        }

        [HttpGet]
        [Route("SelectBalanceSheetPDF")]
        public IActionResult SelectBalanceSheetPDF(DateTime Date, int BranchID, int CostCenterID,
              int CompanyID,int UserId)
        {
            try
            {

                FastReport.Utils.Config.WebMode = true;
                clsReports clsReports = new clsReports();
                DataTable dt = clsReports.SelectBalanceSheet(Date,Simulate.String( BranchID), Simulate.String(CostCenterID), Simulate.String(CompanyID) );

                dsIncomeStatement ds = new dsIncomeStatement();

                if (dt != null && dt.Rows.Count > 0)
                {
                    for (int i = 0; i < dt.Rows.Count; i++)
                    {
                        ds.IncomeStatement.Rows.Add();

                        ds.IncomeStatement.Rows[i]["id"] = dt.Rows[i]["id"];
                        ds.IncomeStatement.Rows[i]["AccountNumber"] = dt.Rows[i]["AccountNumber"];
                        ds.IncomeStatement.Rows[i]["AName"] = dt.Rows[i]["AName"];
                        ds.IncomeStatement.Rows[i]["EName"] = dt.Rows[i]["EName"];

                        ds.IncomeStatement.Rows[i]["isparent"] = Simulate.decimal_(dt.Rows[i]["isparent"]);
                        ds.IncomeStatement.Rows[i]["parentID"] = Simulate.decimal_(dt.Rows[i]["parentID"]);
                        ds.IncomeStatement.Rows[i]["Balance"] = Simulate.decimal_(dt.Rows[i]["Balance"]);
                    }
                }





                FastReport.Report report = new FastReport.Report();
                report.RegisterData(ds);


     
                string MyPath = getMyPath("rptBalanceSheet", CompanyID);
                report.Load(MyPath); if (BranchID == 0)
                {
                    report.SetParameterValue("report.Branch", "All Branches");

                }
                else
                {
                    clsBranch clsBranch = new clsBranch();
                    DataTable dtBranch = clsBranch.SelectBranch(BranchID, "", "", 0);
                    if (dtBranch != null && dtBranch.Rows.Count > 0)
                    {
                        report.SetParameterValue("report.Branch", Simulate.String(dtBranch.Rows[0]["AName"]));

                    }
                }
                if (CostCenterID == 0)
                {
                    report.SetParameterValue("report.CostCenter", "All Cost Center");

                }
                else
                {
                    clsCostCenter clsCostCenter = new clsCostCenter();
                    DataTable dtCostCenter = clsCostCenter.SelectCostCentersByID(CostCenterID, "", "", 0);
                    if (dtCostCenter != null && dtCostCenter.Rows.Count > 0)
                    {
                        report.SetParameterValue("report.CostCenter", Simulate.String(dtCostCenter.Rows[0]["AName"]));

                    }
                }
                report.SetParameterValue("report.FromDate", Date.ToString("yyyy-MM-dd"));
                report.SetParameterValue("report.ToDate", Date.ToString("yyyy-MM-dd"));



                FastreportStanderdParameters(report, UserId, CompanyID);
                //    report.Prepare();

                report.Prepare();

                return FastreporttoPDF(report);
                //return Json(PrepareFrxReport(report), JsonRequestBehavior.AllowGet);


            }
            catch (Exception ex)
            {

                return Json(ex);
            }

        }




        #endregion
        #region  Income Statement  
        [HttpGet]
        [Route("SelectIncomeStatement")]
        public string SelectIncomeStatement(DateTime Date1, DateTime Date2, string BranchID, string CostCenterID,
              string CompanyID)
        {
            try
            {

                clsReports clsReports = new clsReports();
                DataTable dt = clsReports.SelectIncomeStatement(Date1, Date2, BranchID, CostCenterID,
                   CompanyID);
                if (dt != null)
                {
                    string JSONString = string.Empty;
                    JSONString = JsonConvert.SerializeObject(dt);
                    return JSONString;
                }
                else
                {

                    return "";
                }
            }
            catch (Exception)
            {

                throw;
            }




        }

        [HttpGet]
        [Route("SelectIncomeStatementPDF")]
        public IActionResult SelectIncomeStatementPDF(DateTime Date1, DateTime Date2, int BranchID, int CostCenterID,
              int CompanyID,int UserId)
        {
            try
            {

                FastReport.Utils.Config.WebMode = true;
                clsReports clsReports = new clsReports();
                DataTable dt = clsReports.SelectIncomeStatement(Date1, Date2, Simulate.String( BranchID), Simulate.String(CostCenterID), Simulate.String(CompanyID));

                dsIncomeStatement ds = new dsIncomeStatement();

                if (dt != null && dt.Rows.Count > 0)
                {
                    for (int i = 0; i < dt.Rows.Count; i++)
                    {
                        ds.IncomeStatement.Rows.Add();

                        ds.IncomeStatement.Rows[i]["id"] = dt.Rows[i]["id"];
                        ds.IncomeStatement.Rows[i]["AccountNumber"] = dt.Rows[i]["AccountNumber"];
                        ds.IncomeStatement.Rows[i]["AName"] = dt.Rows[i]["AName"];
                        ds.IncomeStatement.Rows[i]["EName"] = dt.Rows[i]["EName"];
                       
                        ds.IncomeStatement.Rows[i]["isparent"] = Simulate.decimal_(dt.Rows[i]["isparent"]);
                       ds.IncomeStatement.Rows[i]["parentID"] = Simulate.decimal_(dt.Rows[i]["parentID"]);
                        ds.IncomeStatement.Rows[i]["Balance"] = Simulate.decimal_(dt.Rows[i]["Balance"]);
                    }
                }





                FastReport.Report report = new FastReport.Report();
                report.RegisterData(ds);


                 string MyPath = getMyPath("rptIncomeStatement", CompanyID);

                report.Load(MyPath); if (BranchID == 0)
                {
                    report.SetParameterValue("report.Branch", "All Branches");

                }
                else
                {
                    clsBranch clsBranch = new clsBranch();
                    DataTable dtBranch = clsBranch.SelectBranch(BranchID, "", "", 0);
                    if (dtBranch != null && dtBranch.Rows.Count > 0)
                    {
                        report.SetParameterValue("report.Branch", Simulate.String(dtBranch.Rows[0]["AName"]));

                    }
                }
                if (CostCenterID == 0)
                {
                    report.SetParameterValue("report.CostCenter", "All Cost Center");

                }
                else
                {
                    clsCostCenter clsCostCenter = new clsCostCenter();
                    DataTable dtCostCenter = clsCostCenter.SelectCostCentersByID(CostCenterID, "", "", 0);
                    if (dtCostCenter != null && dtCostCenter.Rows.Count > 0)
                    {
                        report.SetParameterValue("report.CostCenter", Simulate.String(dtCostCenter.Rows[0]["AName"]));

                    }
                }
                report.SetParameterValue("report.FromDate", Date1.ToString("yyyy-MM-dd"));
                report.SetParameterValue("report.ToDate", Date2.ToString("yyyy-MM-dd"));



                FastreportStanderdParameters(report, UserId, CompanyID);
                //    report.Prepare();

                report.Prepare();

                return FastreporttoPDF(report);
                //return Json(PrepareFrxReport(report), JsonRequestBehavior.AllowGet);


            }
            catch (Exception ex)
            {

                return Json(ex);
            }

        }




        #endregion
        #region Account Statment
        [HttpGet]
        [Route("SelectAccountStatement")]
        public string SelectAccountStatement(DateTime Date1, DateTime Date2, int BranchID, int CostCenterID, int Accountid, int subAccountid, int CompanyID,bool isDue)
        {
            try
            {
                clsReports clsReports = new clsReports();
                DataTable dt = clsReports.SelectAccountStatement(Date1, Date2, BranchID, CostCenterID, Accountid, subAccountid, CompanyID, isDue);
                if (dt != null)
                {



                    string JSONString = string.Empty;
                    JSONString = JsonConvert.SerializeObject(dt);
                    return JSONString;
                }
                else
                {

                    return "";
                }
            }
            catch (Exception)
            {

                throw;
            }




        }

        [HttpGet]
        [Route("SelectAccountStatementPDF")]
        public IActionResult SelectAccountStatementPDF(DateTime Date1, DateTime Date2, int BranchID, int CostCenterID, int Accountid, int subAccountid, int UserID, int CompanyID,bool isDue)
        {
            try
            {

                FastReport.Utils.Config.WebMode = true;
                clsReports clsReports = new clsReports();
                DataTable dt = clsReports.SelectAccountStatement(Date1, Date2, BranchID, CostCenterID, Accountid, subAccountid, CompanyID, isDue);

                dsAccountStatment ds = new dsAccountStatment();

                if (dt != null && dt.Rows.Count > 0)
                {
                    for (int i = 0; i < dt.Rows.Count; i++)
                    {
                        ds.AccountStatment.Rows.Add();

                        // ds.AccountStatment.Rows[i]["id"] = dt.Rows[i]["id"];
                        //ds.AccountStatment.Rows[i]["parentID"] = dt.Rows[i]["parentID"];
                        ds.AccountStatment.Rows[i]["accountID"] = dt.Rows[i]["accountID"];
                        ds.AccountStatment.Rows[i]["subaccountID"] = dt.Rows[i]["subaccountID"];

                        ds.AccountStatment.Rows[i]["Debit"] = Simulate.decimal_(dt.Rows[i]["Debit"]);
                        ds.AccountStatment.Rows[i]["Credit"] = Simulate.decimal_(dt.Rows[i]["Credit"]);
                        ds.AccountStatment.Rows[i]["total"] = Simulate.decimal_(dt.Rows[i]["total"]);
                        ds.AccountStatment.Rows[i]["nettotal"] = Simulate.decimal_(dt.Rows[i]["nettotal"]);
                        ds.AccountStatment.Rows[i]["branchID"] = Simulate.String(dt.Rows[i]["branchID"]);
                        ds.AccountStatment.Rows[i]["CostCenterID"] = Simulate.String(dt.Rows[i]["CostCenterID"]);
                        ds.AccountStatment.Rows[i]["DueDate"] = dt.Rows[i]["DueDate"];
                        ds.AccountStatment.Rows[i]["Note"] = dt.Rows[i]["Note"];
                        ds.AccountStatment.Rows[i]["CompanyID"] = dt.Rows[i]["CompanyID"];
                        ds.AccountStatment.Rows[i]["CreationUserID"] = dt.Rows[i]["CreationUserID"];
                        ds.AccountStatment.Rows[i]["CreationDate"] = dt.Rows[i]["CreationDate"];
                        ds.AccountStatment.Rows[i]["ModificationUserID"] = dt.Rows[i]["ModificationUserID"];
                        ds.AccountStatment.Rows[i]["ModificationDate"] = dt.Rows[i]["ModificationDate"];
                        ds.AccountStatment.Rows[i]["BranchName"] = dt.Rows[i]["BranchName"];

                        ds.AccountStatment.Rows[i]["CostCenterName"] = dt.Rows[i]["CostCenterName"];
                        ds.AccountStatment.Rows[i]["VoucherDate"] = Simulate.StringToDate(dt.Rows[i]["VoucherDate"]).ToString("yyyy-MM-dd");
                        ds.AccountStatment.Rows[i]["VoucherType"] = dt.Rows[i]["VoucherType"];
                        ds.AccountStatment.Rows[i]["JVNumber"] = dt.Rows[i]["JVNumber"];
                        ds.AccountStatment.Rows[i]["AccountEname"] = dt.Rows[i]["AccountEname"];
                        ds.AccountStatment.Rows[i]["AccountNumber"] = dt.Rows[i]["AccountNumber"];
                    }
                }





                FastReport.Report report = new FastReport.Report();
                report.RegisterData(ds);


                 string MyPath = getMyPath("rptAccountStatement", CompanyID);
                report.Load(MyPath); if (BranchID == 0)
                {
                    report.SetParameterValue("report.Branch", "All Branches");

                }
                else
                {
                    clsBranch clsBranch = new clsBranch();
                    DataTable dtBranch = clsBranch.SelectBranch(BranchID, "", "", 0);
                    if (dtBranch != null && dtBranch.Rows.Count > 0)
                    {
                        report.SetParameterValue("report.Branch", Simulate.String(dtBranch.Rows[0]["AName"]));

                    }
                }
                if (CostCenterID == 0)
                {
                    report.SetParameterValue("report.CostCenter", "All Cost Center");

                }
                else
                {
                    clsCostCenter clsCostCenter = new clsCostCenter();
                    DataTable dtCostCenter = clsCostCenter.SelectCostCentersByID(CostCenterID, "", "", 0);
                    if (dtCostCenter != null && dtCostCenter.Rows.Count > 0)
                    {
                        report.SetParameterValue("report.CostCenter", Simulate.String(dtCostCenter.Rows[0]["AName"]));

                    }
                }
                report.SetParameterValue("report.FromDate", Date1.ToString("yyyy-MM-dd"));
                report.SetParameterValue("report.ToDate", Date2.ToString("yyyy-MM-dd"));

                clsAccounts clsAccount = new clsAccounts();
                DataTable dtAccount = clsAccount.SelectAccountsByID(Accountid, 0, "", "", "", CompanyID);
                if (dtAccount != null && dtAccount.Rows.Count > 0)
                {
                    string SubAccountName = "";
                    if (subAccountid > 0)
                    {
                        clsBusinessPartner clsBusinessPartner = new clsBusinessPartner();
                        DataTable dtSubAccount = clsBusinessPartner.SelectBusinessPartner(subAccountid, 0, "", "", -1, CompanyID);
                        if (dtSubAccount != null && dtSubAccount.Rows.Count > 0)
                        {
                            SubAccountName = " / " + Simulate.String(dtSubAccount.Rows[0]["AName"]);
                        }

                    }
                    string subAccountIdString = "";
                    if (subAccountid > 0)
                    {
                        subAccountIdString = " / " + subAccountid;
                    }
                    report.SetParameterValue("report.AccountName", Simulate.String(dtAccount.Rows[0]["AName"]) + SubAccountName);
                    report.SetParameterValue("report.AccountNumber", Simulate.String(dtAccount.Rows[0]["AccountNumber"]) + subAccountIdString);

                }

                FastreportStanderdParameters(report, UserID, CompanyID);
                //    report.Prepare();

                report.Prepare();

                return FastreporttoPDF(report);
                //return Json(PrepareFrxReport(report), JsonRequestBehavior.AllowGet);


            }
            catch (Exception ex)
            {

                return RedirectToAction("Index", "Home");
            }

        }

        #endregion
        #region Invoices
        [HttpGet]
        [Route("SelectInvoicesByFilter")]
        public string SelectInvoicesByFilter(DateTime date1, DateTime date2, bool withDateFilter, int paymentMethodID, int branchID, int businessPartnerID, int storeid, int invoiceTypeid, int cashDrawerID, int isCounted, int companyID, int userID)
        {
            try
            {
                clsReports clsReports = new clsReports();
                DataTable dt = clsReports.SelectInvoicesByFilter(date1, date2, withDateFilter, paymentMethodID, branchID, businessPartnerID, storeid, invoiceTypeid, cashDrawerID, isCounted, companyID);

                if (dt != null)
                {



                    string JSONString = string.Empty;
                    JSONString = JsonConvert.SerializeObject(dt);
                    return JSONString;
                }
                else
                {

                    return "";
                }
            }
            catch (Exception)
            {

                throw;
            }




        }

        [HttpGet]
        [Route("SelectInvoicesByFilterPDF")]
        public IActionResult SelectInvoicesByFilterPDFDateTime(DateTime date1, DateTime date2, bool withDateFilter, int paymentMethodID, int branchID, int businessPartnerID, int storeid, int invoiceTypeid, int cashDrawerID, int isCounted, int companyID, int userID)
        {
            try
            {

                FastReport.Utils.Config.WebMode = true;
                clsReports clsReports = new clsReports();
                DataTable dt = clsReports.SelectInvoicesByFilter(date1, date2, withDateFilter, paymentMethodID, branchID, businessPartnerID, storeid, invoiceTypeid, cashDrawerID, isCounted, companyID);

                dsAccountStatment ds = new dsAccountStatment();

                if (dt != null && dt.Rows.Count > 0)
                {
                    for (int i = 0; i < dt.Rows.Count; i++)
                    {
                        ds.AccountStatment.Rows.Add();

                        // ds.AccountStatment.Rows[i]["id"] = dt.Rows[i]["id"];
                        //ds.AccountStatment.Rows[i]["parentID"] = dt.Rows[i]["parentID"];
                        ds.AccountStatment.Rows[i]["accountID"] = dt.Rows[i]["accountID"];
                        ds.AccountStatment.Rows[i]["subaccountID"] = dt.Rows[i]["subaccountID"];

                        ds.AccountStatment.Rows[i]["Debit"] = Simulate.decimal_(dt.Rows[i]["Debit"]);
                        ds.AccountStatment.Rows[i]["Credit"] = Simulate.decimal_(dt.Rows[i]["Credit"]);
                        ds.AccountStatment.Rows[i]["total"] = Simulate.decimal_(dt.Rows[i]["total"]);
                        ds.AccountStatment.Rows[i]["nettotal"] = Simulate.decimal_(dt.Rows[i]["nettotal"]);
                        ds.AccountStatment.Rows[i]["branchID"] = Simulate.String(dt.Rows[i]["branchID"]);
                        ds.AccountStatment.Rows[i]["CostCenterID"] = Simulate.String(dt.Rows[i]["CostCenterID"]);
                        ds.AccountStatment.Rows[i]["DueDate"] = dt.Rows[i]["DueDate"];
                        ds.AccountStatment.Rows[i]["Note"] = dt.Rows[i]["Note"];
                        ds.AccountStatment.Rows[i]["CompanyID"] = dt.Rows[i]["CompanyID"];
                        ds.AccountStatment.Rows[i]["CreationUserID"] = dt.Rows[i]["CreationUserID"];
                        ds.AccountStatment.Rows[i]["CreationDate"] = dt.Rows[i]["CreationDate"];
                        ds.AccountStatment.Rows[i]["ModificationUserID"] = dt.Rows[i]["ModificationUserID"];
                        ds.AccountStatment.Rows[i]["ModificationDate"] = dt.Rows[i]["ModificationDate"];
                        ds.AccountStatment.Rows[i]["BranchName"] = dt.Rows[i]["BranchName"];

                        ds.AccountStatment.Rows[i]["CostCenterName"] = dt.Rows[i]["CostCenterName"];
                        ds.AccountStatment.Rows[i]["VoucherDate"] = Simulate.StringToDate(dt.Rows[i]["VoucherDate"]).ToString("yyyy-MM-dd");
                        ds.AccountStatment.Rows[i]["VoucherType"] = dt.Rows[i]["VoucherType"];
                        ds.AccountStatment.Rows[i]["JVNumber"] = dt.Rows[i]["JVNumber"];
                        ds.AccountStatment.Rows[i]["AccountEname"] = dt.Rows[i]["AccountEname"];
                        ds.AccountStatment.Rows[i]["AccountNumber"] = dt.Rows[i]["AccountNumber"];
                    }
                }





                FastReport.Report report = new FastReport.Report();
                report.RegisterData(ds);


    
                string MyPath = getMyPath("rptAccountStatement", companyID);
                report.Load(MyPath); if (branchID == 0)
                {
                    report.SetParameterValue("report.Branch", "All Branches");

                }
                else
                {
                    clsBranch clsBranch = new clsBranch();
                    DataTable dtBranch = clsBranch.SelectBranch(branchID, "", "", 0);
                    if (dtBranch != null && dtBranch.Rows.Count > 0)
                    {
                        report.SetParameterValue("report.Branch", Simulate.String(dtBranch.Rows[0]["AName"]));

                    }
                }
                //if (CostCenterID == 0)
                //{
                //    report.SetParameterValue("report.CostCenter", "All Cost Center");

                //}
                //else
                //{
                //    clsCostCenter clsCostCenter = new clsCostCenter();
                //    DataTable dtCostCenter = clsCostCenter.SelectCostCentersByID(CostCenterID, "", "", 0);
                //    if (dtCostCenter != null && dtCostCenter.Rows.Count > 0)
                //    {
                //        report.SetParameterValue("report.CostCenter", Simulate.String(dtCostCenter.Rows[0]["AName"]));

                //    }
                //}
                report.SetParameterValue("report.FromDate", date1.ToString("yyyy-MM-dd"));
                report.SetParameterValue("report.ToDate", date2.ToString("yyyy-MM-dd"));

                //clsAccounts clsAccount = new clsAccounts();
                //  DataTable dtAccount = clsAccount.SelectAccountsByID(Accountid, 0, "", "", "", CompanyID);
                //if (dtAccount != null && dtAccount.Rows.Count > 0)
                //{
                //    string SubAccountName = "";
                //    if (subAccountid > 0)
                //    {
                //        clsBusinessPartner clsBusinessPartner = new clsBusinessPartner();
                //        DataTable dtSubAccount = clsBusinessPartner.SelectBusinessPartner(subAccountid, 0, "", "", CompanyID);
                //        if (dtSubAccount != null && dtSubAccount.Rows.Count > 0)
                //        {
                //            SubAccountName = " / " + Simulate.String(dtSubAccount.Rows[0]["EName"]);
                //        }

                //    }
                //    string subAccountIdString = "";
                //    if (subAccountid > 0)
                //    {
                //        subAccountIdString = " / " + subAccountid;
                //    }
                //    report.SetParameterValue("report.AccountName", Simulate.String(dtAccount.Rows[0]["EName"]) + SubAccountName);
                //    report.SetParameterValue("report.AccountNumber", Simulate.String(dtAccount.Rows[0]["AccountNumber"]) + subAccountIdString);

                //}

                FastreportStanderdParameters(report, userID, companyID);
                //    report.Prepare();

                report.Prepare();

                return FastreporttoPDF(report);
                //return Json(PrepareFrxReport(report), JsonRequestBehavior.AllowGet);


            }
            catch (Exception ex)
            {

                return RedirectToAction("Index", "Home");
            }

        }

        #endregion
        #region Item Transactions
        [HttpGet]
        [Route("SelectItemTransactionsByFilter")]
        public string SelectItemTransactionsByFilter(DateTime date1, DateTime date2, bool withDateFilter,
            string itemguid, int branchID, int businessPartnerID, int storeid,
            int invoiceTypeid, int isCounted, int companyID, int userID)
        {
            try
            {
                clsReports clsReports = new clsReports();
                DataTable dt = clsReports.SelectItemTransactionsByFilter(date1, date2, withDateFilter,
                           itemguid, branchID, businessPartnerID, storeid,
                           invoiceTypeid, isCounted, companyID);
                if (dt != null)
                {



                    string JSONString = string.Empty;
                    JSONString = JsonConvert.SerializeObject(dt);
                    return JSONString;
                }
                else
                {

                    return "";
                }
            }
            catch (Exception)
            {

                throw;
            }




        }

        [HttpGet]
        [Route("SelectItemTransactionsByFilterPDF")]
        public IActionResult SelectItemTransactionsByFilterPDF(DateTime date1, DateTime date2, bool withDateFilter,
            string itemguid, int branchID, int businessPartnerID, int storeid,
            int invoiceTypeid, int isCounted, int companyID, int userID)
        {
            try
            {

                FastReport.Utils.Config.WebMode = true;
                clsReports clsReports = new clsReports();
                DataTable dt = clsReports.SelectItemTransactionsByFilter(date1, date2, withDateFilter,
              itemguid, branchID, businessPartnerID, storeid,
              invoiceTypeid, isCounted, companyID);

                dsAccountStatment ds = new dsAccountStatment();

                if (dt != null && dt.Rows.Count > 0)
                {
                    for (int i = 0; i < dt.Rows.Count; i++)
                    {
                        ds.AccountStatment.Rows.Add();

                        // ds.AccountStatment.Rows[i]["id"] = dt.Rows[i]["id"];
                        //ds.AccountStatment.Rows[i]["parentID"] = dt.Rows[i]["parentID"];
                        ds.AccountStatment.Rows[i]["accountID"] = dt.Rows[i]["accountID"];
                        ds.AccountStatment.Rows[i]["subaccountID"] = dt.Rows[i]["subaccountID"];

                        ds.AccountStatment.Rows[i]["Debit"] = Simulate.decimal_(dt.Rows[i]["Debit"]);
                        ds.AccountStatment.Rows[i]["Credit"] = Simulate.decimal_(dt.Rows[i]["Credit"]);
                        ds.AccountStatment.Rows[i]["total"] = Simulate.decimal_(dt.Rows[i]["total"]);
                        ds.AccountStatment.Rows[i]["nettotal"] = Simulate.decimal_(dt.Rows[i]["nettotal"]);
                        ds.AccountStatment.Rows[i]["branchID"] = Simulate.String(dt.Rows[i]["branchID"]);
                        ds.AccountStatment.Rows[i]["CostCenterID"] = Simulate.String(dt.Rows[i]["CostCenterID"]);
                        ds.AccountStatment.Rows[i]["DueDate"] = dt.Rows[i]["DueDate"];
                        ds.AccountStatment.Rows[i]["Note"] = dt.Rows[i]["Note"];
                        ds.AccountStatment.Rows[i]["CompanyID"] = dt.Rows[i]["CompanyID"];
                        ds.AccountStatment.Rows[i]["CreationUserID"] = dt.Rows[i]["CreationUserID"];
                        ds.AccountStatment.Rows[i]["CreationDate"] = dt.Rows[i]["CreationDate"];
                        ds.AccountStatment.Rows[i]["ModificationUserID"] = dt.Rows[i]["ModificationUserID"];
                        ds.AccountStatment.Rows[i]["ModificationDate"] = dt.Rows[i]["ModificationDate"];
                        ds.AccountStatment.Rows[i]["BranchName"] = dt.Rows[i]["BranchName"];

                        ds.AccountStatment.Rows[i]["CostCenterName"] = dt.Rows[i]["CostCenterName"];
                        ds.AccountStatment.Rows[i]["VoucherDate"] = Simulate.StringToDate(dt.Rows[i]["VoucherDate"]).ToString("yyyy-MM-dd");
                        ds.AccountStatment.Rows[i]["VoucherType"] = dt.Rows[i]["VoucherType"];
                        ds.AccountStatment.Rows[i]["JVNumber"] = dt.Rows[i]["JVNumber"];
                        ds.AccountStatment.Rows[i]["AccountEname"] = dt.Rows[i]["AccountEname"];
                        ds.AccountStatment.Rows[i]["AccountNumber"] = dt.Rows[i]["AccountNumber"];
                    }
                }





                FastReport.Report report = new FastReport.Report();
                report.RegisterData(ds);


                 string MyPath = getMyPath("rptAccountStatement", companyID);
                report.Load(MyPath); if (branchID == 0)
                {
                    report.SetParameterValue("report.Branch", "All Branches");

                }
                else
                {
                    clsBranch clsBranch = new clsBranch();
                    DataTable dtBranch = clsBranch.SelectBranch(branchID, "", "", 0);
                    if (dtBranch != null && dtBranch.Rows.Count > 0)
                    {
                        report.SetParameterValue("report.Branch", Simulate.String(dtBranch.Rows[0]["AName"]));

                    }
                }
                //if (CostCenterID == 0)
                //{
                //    report.SetParameterValue("report.CostCenter", "All Cost Center");

                //}
                //else
                //{
                //    clsCostCenter clsCostCenter = new clsCostCenter();
                //    DataTable dtCostCenter = clsCostCenter.SelectCostCentersByID(CostCenterID, "", "", 0);
                //    if (dtCostCenter != null && dtCostCenter.Rows.Count > 0)
                //    {
                //        report.SetParameterValue("report.CostCenter", Simulate.String(dtCostCenter.Rows[0]["AName"]));

                //    }
                //}
                report.SetParameterValue("report.FromDate", date1.ToString("yyyy-MM-dd"));
                report.SetParameterValue("report.ToDate", date2.ToString("yyyy-MM-dd"));

                //clsAccounts clsAccount = new clsAccounts();
                //  DataTable dtAccount = clsAccount.SelectAccountsByID(Accountid, 0, "", "", "", CompanyID);
                //if (dtAccount != null && dtAccount.Rows.Count > 0)
                //{
                //    string SubAccountName = "";
                //    if (subAccountid > 0)
                //    {
                //        clsBusinessPartner clsBusinessPartner = new clsBusinessPartner();
                //        DataTable dtSubAccount = clsBusinessPartner.SelectBusinessPartner(subAccountid, 0, "", "", CompanyID);
                //        if (dtSubAccount != null && dtSubAccount.Rows.Count > 0)
                //        {
                //            SubAccountName = " / " + Simulate.String(dtSubAccount.Rows[0]["EName"]);
                //        }

                //    }
                //    string subAccountIdString = "";
                //    if (subAccountid > 0)
                //    {
                //        subAccountIdString = " / " + subAccountid;
                //    }
                //    report.SetParameterValue("report.AccountName", Simulate.String(dtAccount.Rows[0]["EName"]) + SubAccountName);
                //    report.SetParameterValue("report.AccountNumber", Simulate.String(dtAccount.Rows[0]["AccountNumber"]) + subAccountIdString);

                //}

                FastreportStanderdParameters(report, userID, companyID);
                //    report.Prepare();

                report.Prepare();

                return FastreporttoPDF(report);
                //return Json(PrepareFrxReport(report), JsonRequestBehavior.AllowGet);


            }
            catch (Exception ex)
            {

                return RedirectToAction("Index", "Home");
            }

        }

        #endregion
        #region Item Transactions
        [HttpGet]
        [Route("SelectInventoryReportByFilter")]
        public string SelectInventoryReportByFilter(DateTime date1, DateTime date2, bool withDateFilter,
          string itemguid, int branchID, int categoryid, int storeid,
            int companyID, int userID)
        {
            try
            {
                clsReports clsReports = new clsReports();
                DataTable dt = clsReports.SelectInventoryReportByFilter(date1, date2, withDateFilter,
            itemguid, branchID, categoryid, storeid,
              companyID);
                if (dt != null)
                {



                    string JSONString = string.Empty;
                    JSONString = JsonConvert.SerializeObject(dt);
                    return JSONString;
                }
                else
                {

                    return "";
                }
            }
            catch (Exception)
            {

                throw;
            }




        }

        [HttpGet]
        [Route("SelectInventoryReportByFilterPDF")]
        public IActionResult SelectInventoryReportByFilterPDF(DateTime date1, DateTime date2, bool withDateFilter,
          string itemguid, int branchID, int categoryid, int storeid,
            int companyID, int userID)
        {
            try
            {

                FastReport.Utils.Config.WebMode = true;
                clsReports clsReports = new clsReports();
                DataTable dt = clsReports.SelectInventoryReportByFilter(date1, date2, withDateFilter,
                 itemguid, branchID, categoryid, storeid,
                   companyID);

                dsAccountStatment ds = new dsAccountStatment();

                if (dt != null && dt.Rows.Count > 0)
                {
                    for (int i = 0; i < dt.Rows.Count; i++)
                    {
                        ds.AccountStatment.Rows.Add();

                        // ds.AccountStatment.Rows[i]["id"] = dt.Rows[i]["id"];
                        //ds.AccountStatment.Rows[i]["parentID"] = dt.Rows[i]["parentID"];
                        ds.AccountStatment.Rows[i]["accountID"] = dt.Rows[i]["accountID"];
                        ds.AccountStatment.Rows[i]["subaccountID"] = dt.Rows[i]["subaccountID"];

                        ds.AccountStatment.Rows[i]["Debit"] = Simulate.decimal_(dt.Rows[i]["Debit"]);
                        ds.AccountStatment.Rows[i]["Credit"] = Simulate.decimal_(dt.Rows[i]["Credit"]);
                        ds.AccountStatment.Rows[i]["total"] = Simulate.decimal_(dt.Rows[i]["total"]);
                        ds.AccountStatment.Rows[i]["nettotal"] = Simulate.decimal_(dt.Rows[i]["nettotal"]);
                        ds.AccountStatment.Rows[i]["branchID"] = Simulate.String(dt.Rows[i]["branchID"]);
                        ds.AccountStatment.Rows[i]["CostCenterID"] = Simulate.String(dt.Rows[i]["CostCenterID"]);
                        ds.AccountStatment.Rows[i]["DueDate"] = dt.Rows[i]["DueDate"];
                        ds.AccountStatment.Rows[i]["Note"] = dt.Rows[i]["Note"];
                        ds.AccountStatment.Rows[i]["CompanyID"] = dt.Rows[i]["CompanyID"];
                        ds.AccountStatment.Rows[i]["CreationUserID"] = dt.Rows[i]["CreationUserID"];
                        ds.AccountStatment.Rows[i]["CreationDate"] = dt.Rows[i]["CreationDate"];
                        ds.AccountStatment.Rows[i]["ModificationUserID"] = dt.Rows[i]["ModificationUserID"];
                        ds.AccountStatment.Rows[i]["ModificationDate"] = dt.Rows[i]["ModificationDate"];
                        ds.AccountStatment.Rows[i]["BranchName"] = dt.Rows[i]["BranchName"];

                        ds.AccountStatment.Rows[i]["CostCenterName"] = dt.Rows[i]["CostCenterName"];
                        ds.AccountStatment.Rows[i]["VoucherDate"] = Simulate.StringToDate(dt.Rows[i]["VoucherDate"]).ToString("yyyy-MM-dd");
                        ds.AccountStatment.Rows[i]["VoucherType"] = dt.Rows[i]["VoucherType"];
                        ds.AccountStatment.Rows[i]["JVNumber"] = dt.Rows[i]["JVNumber"];
                        ds.AccountStatment.Rows[i]["AccountEname"] = dt.Rows[i]["AccountEname"];
                        ds.AccountStatment.Rows[i]["AccountNumber"] = dt.Rows[i]["AccountNumber"];
                    }
                }





                FastReport.Report report = new FastReport.Report();
                report.RegisterData(ds);


           
                string MyPath = getMyPath("rptAccountStatement", companyID);
                report.Load(MyPath); if (branchID == 0)
                {
                    report.SetParameterValue("report.Branch", "All Branches");

                }
                else
                {
                    clsBranch clsBranch = new clsBranch();
                    DataTable dtBranch = clsBranch.SelectBranch(branchID, "", "", 0);
                    if (dtBranch != null && dtBranch.Rows.Count > 0)
                    {
                        report.SetParameterValue("report.Branch", Simulate.String(dtBranch.Rows[0]["AName"]));

                    }
                }
                //if (CostCenterID == 0)
                //{
                //    report.SetParameterValue("report.CostCenter", "All Cost Center");

                //}
                //else
                //{
                //    clsCostCenter clsCostCenter = new clsCostCenter();
                //    DataTable dtCostCenter = clsCostCenter.SelectCostCentersByID(CostCenterID, "", "", 0);
                //    if (dtCostCenter != null && dtCostCenter.Rows.Count > 0)
                //    {
                //        report.SetParameterValue("report.CostCenter", Simulate.String(dtCostCenter.Rows[0]["AName"]));

                //    }
                //}
                report.SetParameterValue("report.FromDate", date1.ToString("yyyy-MM-dd"));
                report.SetParameterValue("report.ToDate", date2.ToString("yyyy-MM-dd"));

                //clsAccounts clsAccount = new clsAccounts();
                //  DataTable dtAccount = clsAccount.SelectAccountsByID(Accountid, 0, "", "", "", CompanyID);
                //if (dtAccount != null && dtAccount.Rows.Count > 0)
                //{
                //    string SubAccountName = "";
                //    if (subAccountid > 0)
                //    {
                //        clsBusinessPartner clsBusinessPartner = new clsBusinessPartner();
                //        DataTable dtSubAccount = clsBusinessPartner.SelectBusinessPartner(subAccountid, 0, "", "", CompanyID);
                //        if (dtSubAccount != null && dtSubAccount.Rows.Count > 0)
                //        {
                //            SubAccountName = " / " + Simulate.String(dtSubAccount.Rows[0]["EName"]);
                //        }

                //    }
                //    string subAccountIdString = "";
                //    if (subAccountid > 0)
                //    {
                //        subAccountIdString = " / " + subAccountid;
                //    }
                //    report.SetParameterValue("report.AccountName", Simulate.String(dtAccount.Rows[0]["EName"]) + SubAccountName);
                //    report.SetParameterValue("report.AccountNumber", Simulate.String(dtAccount.Rows[0]["AccountNumber"]) + subAccountIdString);

                //}

                FastreportStanderdParameters(report, userID, companyID);
                //    report.Prepare();

                report.Prepare();

                return FastreporttoPDF(report);
                //return Json(PrepareFrxReport(report), JsonRequestBehavior.AllowGet);


            }
            catch (Exception ex)
            {

                return RedirectToAction("Index", "Home");
            }

        }

        #endregion

        #region Cash Report
        [HttpGet]
        [Route("SelectCashReport")]
        public string SelectCashReport(bool IsPosDate, DateTime Date1, DateTime Date2, int BranchID, int CashID, int InvoiceTypeid, int UserID, int CompanyID)
        {
            try
            {
                clsReports clsReports = new clsReports();
                DataTable dt = clsReports.SelectCashReport(IsPosDate, Date1, Date2, BranchID, CashID, InvoiceTypeid, CompanyID);
                if (dt != null)
                {
                    string JSONString = string.Empty;
                    JSONString = JsonConvert.SerializeObject(dt);
                    return JSONString;
                }
                else
                {

                    return "";
                }
            }
            catch (Exception)
            {

                throw;
            }




        }

        [HttpGet]
        [Route("SelectCashReportPDF")]
        public IActionResult SelectCashReportPDF(bool IsPosDate, DateTime Date1, DateTime Date2, int BranchID, int CashID, int InvoiceTypeid, int UserId, int CompanyID)
        {
            try
            {

                FastReport.Utils.Config.WebMode = true;
                clsReports clsReports = new clsReports();
                DataTable dt = clsReports.SelectCashReport(IsPosDate, Date1, Date2, BranchID, CashID, InvoiceTypeid, CompanyID);

                dsCashReport ds = new dsCashReport();

                if (dt != null && dt.Rows.Count > 0)
                {
                    for (int i = 0; i < dt.Rows.Count; i++)
                    {
                        ds.CashReport.Rows.Add();

                        ds.CashReport.Rows[i]["InvoiceDate"] = dt.Rows[i]["InvoiceDate"];
                        ds.CashReport.Rows[i]["PaymentMethodID"] = dt.Rows[i]["PaymentMethodID"];
                        ds.CashReport.Rows[i]["PaymentMethod"] = dt.Rows[i]["PaymentMethod"];
                        ds.CashReport.Rows[i]["BusinessPartnerID"] = dt.Rows[i]["BusinessPartnerID"];
                        ds.CashReport.Rows[i]["BusinessPartner"] = Simulate.String(dt.Rows[i]["BusinessPartner"]);
                        ds.CashReport.Rows[i]["InvoiceCount"] = Simulate.String(dt.Rows[i]["InvoiceCount"]);
                        ds.CashReport.Rows[i]["TotalTax"] = Simulate.decimal_(dt.Rows[i]["TotalTax"]);
                        ds.CashReport.Rows[i]["HeaderDiscount"] = Simulate.decimal_(dt.Rows[i]["HeaderDiscount"]);
                        ds.CashReport.Rows[i]["TotalDiscount"] = Simulate.decimal_(dt.Rows[i]["TotalDiscount"]);
                        ds.CashReport.Rows[i]["TotalInvoice"] = Simulate.decimal_(dt.Rows[i]["TotalInvoice"]);
                    }
                }





                FastReport.Report report = new FastReport.Report();
                report.RegisterData(ds);

 
                string MyPath = getMyPath("rptCashReport", CompanyID);
                report.Load(MyPath); if (BranchID == 0)
                {
                    report.SetParameterValue("report.Branch", "All Branches");

                }
                else
                {
                    clsBranch clsBranch = new clsBranch();
                    DataTable dtBranch = clsBranch.SelectBranch(BranchID, "", "", 0);
                    if (dtBranch != null && dtBranch.Rows.Count > 0)
                    {
                        report.SetParameterValue("report.Branch", Simulate.String(dtBranch.Rows[0]["AName"]));

                    }
                }
                if (IsPosDate == true)
                {
                    report.SetParameterValue("report.IsPosDate", "By POS Day");

                }
                else
                {
                    report.SetParameterValue("report.IsPosDate", "By Voucher Day");

                }
                if (CashID == 0)
                {
                    report.SetParameterValue("report.CashDrawer", "All Cash Drawer");

                }
                else
                {
                    clsCashDrawer clsCashDrawer = new clsCashDrawer();
                    DataTable dtCash = clsCashDrawer.SelectCashDrawerByID(CashID, "", "", 0);
                    if (dtCash != null && dtCash.Rows.Count > 0)
                    {
                        report.SetParameterValue("report.CashDrawer", Simulate.String(dtCash.Rows[0]["AName"]));

                    }
                }
                if (InvoiceTypeid == 0)
                {
                    report.SetParameterValue("report.JournalVoucherTypes", "All Invoices");

                }
                else
                {
                    clsJournalVoucherTypes clsJournalVoucherTypes = new clsJournalVoucherTypes();
                    DataTable dtJournalVoucherTypes = clsJournalVoucherTypes.SelectJournalVoucherTypes(InvoiceTypeid);
                    if (dtJournalVoucherTypes != null && dtJournalVoucherTypes.Rows.Count > 0)
                    {
                        report.SetParameterValue("report.JournalVoucherTypes", Simulate.String(dtJournalVoucherTypes.Rows[0]["AName"]));

                    }
                }


                report.SetParameterValue("report.FromDate", Date1.ToString("yyyy-MM-dd"));
                report.SetParameterValue("report.ToDate", Date2.ToString("yyyy-MM-dd"));



                FastreportStanderdParameters(report, UserId, CompanyID);
                //    report.Prepare();

                report.Prepare();

                return FastreporttoPDF(report);
                //return Json(PrepareFrxReport(report), JsonRequestBehavior.AllowGet);


            }
            catch (Exception ex)
            {

                return Json(ex);
            }

        }

        #endregion


        #region Print Invoice


        [HttpGet]
        [Route("SelectInvoicePDF")]
        public IActionResult SelectInvoicePDF(string guid, int UserId, int CompanyID)
        {
            try
            {

                FastReport.Utils.Config.WebMode = true;
                clsInvoiceHeader clsInvoiceHeader = new clsInvoiceHeader();
                clsInvoiceDetails clsInvoiceDetails = new clsInvoiceDetails();

                DataTable dtHeader = clsInvoiceHeader.SelectInvoiceHeaderByGuid(guid, DateTime.Now.AddYears(-100), DateTime.Now.AddYears(100), 0, 0, CompanyID);
                DataTable dtDetails = clsInvoiceDetails.SelectInvoiceDetailsByHeaderGuid(guid, "", CompanyID);

                dsInvoiceDetails ds = new dsInvoiceDetails();

                if (dtDetails != null && dtDetails.Rows.Count > 0)
                {
                    for (int i = 0; i < dtDetails.Rows.Count; i++)
                    {
                        ds.InvoiceDetails.Rows.Add();

                        ds.InvoiceDetails.Rows[i]["Guid"] = Simulate.String(dtDetails.Rows[i]["Guid"]);
                        ds.InvoiceDetails.Rows[i]["HeaderGuid"] = Simulate.String(dtDetails.Rows[i]["HeaderGuid"]);
                        ds.InvoiceDetails.Rows[i]["RowIndex"] = Simulate.String(dtDetails.Rows[i]["RowIndex"]);
                        ds.InvoiceDetails.Rows[i]["ItemGuid"] = Simulate.String(dtDetails.Rows[i]["ItemGuid"]);
                        ds.InvoiceDetails.Rows[i]["ItemName"] = Simulate.String(dtDetails.Rows[i]["ItemName"]);
                        ds.InvoiceDetails.Rows[i]["Qty"] = Simulate.decimal_(dtDetails.Rows[i]["Qty"]);
                        ds.InvoiceDetails.Rows[i]["PriceBeforeTax"] = Simulate.decimal_(dtDetails.Rows[i]["PriceBeforeTax"]);
                        ds.InvoiceDetails.Rows[i]["DiscountBeforeTaxAmount"] = Simulate.decimal_(dtDetails.Rows[i]["DiscountBeforeTaxAmount"]);
                        ds.InvoiceDetails.Rows[i]["TaxID"] = Simulate.String(dtDetails.Rows[i]["TaxID"]);
                        ds.InvoiceDetails.Rows[i]["TaxPercentage"] = Simulate.String(dtDetails.Rows[i]["TaxPercentage"]);
                        ds.InvoiceDetails.Rows[i]["TaxAmount"] = Simulate.decimal_(dtDetails.Rows[i]["TaxAmount"]);
                        ds.InvoiceDetails.Rows[i]["SpecialTaxID"] = Simulate.String(dtDetails.Rows[i]["SpecialTaxID"]);
                        ds.InvoiceDetails.Rows[i]["SpecialTaxPercentage"] = Simulate.String(dtDetails.Rows[i]["SpecialTaxPercentage"]);
                        ds.InvoiceDetails.Rows[i]["SpecialTaxAmount"] = Simulate.decimal_(dtDetails.Rows[i]["SpecialTaxAmount"]);
                        ds.InvoiceDetails.Rows[i]["DiscountAfterTaxAmount"] = Simulate.decimal_(dtDetails.Rows[i]["DiscountAfterTaxAmount"]);
                        ds.InvoiceDetails.Rows[i]["HeaderDiscountAfterTaxAmount"] = Simulate.decimal_(dtDetails.Rows[i]["HeaderDiscountAfterTaxAmount"]);
                        ds.InvoiceDetails.Rows[i]["FreeQty"] = Simulate.decimal_(dtDetails.Rows[i]["FreeQty"]);
                        ds.InvoiceDetails.Rows[i]["TotalQTY"] = Simulate.decimal_(dtDetails.Rows[i]["TotalQTY"]);
                        ds.InvoiceDetails.Rows[i]["ServiceBeforeTax"] = Simulate.decimal_(dtDetails.Rows[i]["ServiceBeforeTax"]);
                        ds.InvoiceDetails.Rows[i]["ServiceTaxAmount"] = Simulate.decimal_(dtDetails.Rows[i]["ServiceTaxAmount"]);
                        ds.InvoiceDetails.Rows[i]["ServiceAfterTax"] = Simulate.decimal_(dtDetails.Rows[i]["ServiceAfterTax"]);
                        ds.InvoiceDetails.Rows[i]["TotalLine"] = Simulate.decimal_(dtDetails.Rows[i]["TotalLine"]);
                        ds.InvoiceDetails.Rows[i]["BranchID"] = Simulate.String(dtDetails.Rows[i]["BranchID"]);
                        ds.InvoiceDetails.Rows[i]["StoreID"] = Simulate.String(dtDetails.Rows[i]["StoreID"]);
                        ds.InvoiceDetails.Rows[i]["CompanyID"] = Simulate.String(dtDetails.Rows[i]["CompanyID"]);
                        ds.InvoiceDetails.Rows[i]["InvoiceTypeID"] = Simulate.String(dtDetails.Rows[i]["InvoiceTypeID"]);
                        ds.InvoiceDetails.Rows[i]["IsCounted"] = Simulate.String(dtDetails.Rows[i]["IsCounted"]);
                        ds.InvoiceDetails.Rows[i]["InvoiceDate"] = Simulate.String(dtDetails.Rows[i]["InvoiceDate"]);
                        ds.InvoiceDetails.Rows[i]["BusinessPartnerID"] = Simulate.String(dtDetails.Rows[i]["BusinessPartnerID"]);
                        ds.InvoiceDetails.Rows[i]["ItemBatchsGuid"] = Simulate.String(dtDetails.Rows[i]["ItemBatchsGuid"]);




                    }
                }





                FastReport.Report report = new FastReport.Report();
                report.RegisterData(ds);


             
                string MyPath = getMyPath("rptInvoice", CompanyID);
                report.Load(MyPath); if (Simulate.Integer32(dtHeader.Rows[0]["BranchID"]) == 0)
                {
                    report.SetParameterValue("report.Branch", "All Branches");

                }
                else
                {
                    clsBranch clsBranch = new clsBranch();
                    DataTable dtBranch = clsBranch.SelectBranch(Simulate.Integer32(dtHeader.Rows[0]["BranchID"]), "", "", 0);
                    if (dtBranch != null && dtBranch.Rows.Count > 0)
                    {
                        report.SetParameterValue("report.Branch", Simulate.String(dtBranch.Rows[0]["AName"]));

                    }
                }
                if (Simulate.Integer32(dtHeader.Rows[0]["BusinessPartnerID"]) == 0)
                {
                    report.SetParameterValue("report.BusinessPartner", "Un Known");

                }
                else
                {
                    clsBusinessPartner clsBusinessPartner = new clsBusinessPartner();
                    DataTable dtBusinessPartner = clsBusinessPartner.SelectBusinessPartner(Simulate.Integer32(dtHeader.Rows[0]["BusinessPartnerID"]), 0, "", "", -1, 0);
                    report.SetParameterValue("report.BusinessPartner", Simulate.String(dtBusinessPartner.Rows[0]["AName"]));

                }
                if (Simulate.Integer32(dtHeader.Rows[0]["CashID"]) == 0)
                {
                    report.SetParameterValue("report.CashDrawer", "All Cash Drawer");

                }
                else
                {
                    clsCashDrawer clsCashDrawer = new clsCashDrawer();
                    DataTable dtCash = clsCashDrawer.SelectCashDrawerByID(Simulate.Integer32(dtHeader.Rows[0]["CashID"]), "", "", 0);
                    if (dtCash != null && dtCash.Rows.Count > 0)
                    {
                        report.SetParameterValue("report.CashDrawer", Simulate.String(dtCash.Rows[0]["AName"]));

                    }
                }
                if (Simulate.Integer32(dtHeader.Rows[0]["InvoiceTypeid"]) == 0)
                {
                    report.SetParameterValue("report.JournalVoucherTypes", "All Invoices");

                }
                else
                {
                    clsJournalVoucherTypes clsJournalVoucherTypes = new clsJournalVoucherTypes();
                    DataTable dtJournalVoucherTypes = clsJournalVoucherTypes.SelectJournalVoucherTypes(Simulate.Integer32(dtHeader.Rows[0]["InvoiceTypeid"]));
                    if (dtJournalVoucherTypes != null && dtJournalVoucherTypes.Rows.Count > 0)
                    {
                        report.SetParameterValue("report.JournalVoucherTypes", Simulate.String(dtJournalVoucherTypes.Rows[0]["AName"]));

                    }
                }


                report.SetParameterValue("report.Date", Simulate.StringToDate(dtHeader.Rows[0]["InvoiceDate"]).ToString("yyyy-MM-dd"));




                FastreportStanderdParameters(report, UserId, CompanyID);
                //    report.Prepare();

                report.Prepare();

                return FastreporttoPDF(report);
                //return Json(PrepareFrxReport(report), JsonRequestBehavior.AllowGet);


            }
            catch (Exception ex)
            {

                return Json(ex);
            }

        }

        #endregion

        #region Print Invoice


        [HttpGet]
        [Route("SelectJVPDF")]
        public IActionResult SelectJVPDF(string guid, int UserId, int CompanyID)
        {
            try
            {

                FastReport.Utils.Config.WebMode = true;
                clsJournalVoucherHeader clsJournalVoucherHeader = new clsJournalVoucherHeader();
                clsJournalVoucherDetails clsJournalVoucherDetails = new clsJournalVoucherDetails();

                DataTable dtHeader = clsJournalVoucherHeader.SelectJournalVoucherHeaderForPrint(guid, 0, 0, "", "", 0, CompanyID, DateTime.Now.AddYears(-100), DateTime.Now.AddYears(100));
                dsJVDetails ds = clsJournalVoucherDetails.SelectJournalVoucherDetailsByParentIdForPrint(guid, 0, 0);






                FastReport.Report report = new FastReport.Report();
                report.RegisterData(ds);

 
                string MyPath = getMyPath("rptJV", CompanyID);
                report.Load(MyPath); if (Simulate.Integer32(dtHeader.Rows[0]["BranchID"]) == 0)
                {
                    report.SetParameterValue("report.Branch", "All Branches");

                }
                else
                {
                    clsBranch clsBranch = new clsBranch();
                    DataTable dtBranch = clsBranch.SelectBranch(Simulate.Integer32(dtHeader.Rows[0]["BranchID"]), "", "", 0);
                    if (dtBranch != null && dtBranch.Rows.Count > 0)
                    {
                        report.SetParameterValue("report.Branch", Simulate.String(dtBranch.Rows[0]["AName"]));

                    }
                }
                //if (Simulate.Integer32(dtHeader.Rows[0]["BusinessPartnerID"]) == 0)
                //{
                //    report.SetParameterValue("report.BusinessPartner", "Un Known");

                //}
                //else
                //{
                //    clsBusinessPartner clsBusinessPartner = new clsBusinessPartner();
                //    DataTable dtBusinessPartner = clsBusinessPartner.SelectBusinessPartner(Simulate.Integer32(dtHeader.Rows[0]["BusinessPartnerID"]), 0, "", "", 0);
                //    report.SetParameterValue("report.BusinessPartner", Simulate.String(dtBusinessPartner.Rows[0]["AName"]));

                //}
                //if (Simulate.Integer32(dtHeader.Rows[0]["CashID"]) == 0)
                //{
                //    report.SetParameterValue("report.CashDrawer", "All Cash Drawer");

                //}
                //else
                //{
                //    clsCashDrawer clsCashDrawer = new clsCashDrawer();
                //    DataTable dtCash = clsCashDrawer.SelectCashDrawerByID(Simulate.Integer32(dtHeader.Rows[0]["CashID"]), "", "", 0);
                //    if (dtCash != null && dtCash.Rows.Count > 0)
                //    {
                //        report.SetParameterValue("report.CashDrawer", Simulate.String(dtCash.Rows[0]["AName"]));

                //    }
                //}
                //if (Simulate.Integer32(dtHeader.Rows[0]["InvoiceTypeid"]) == 0)
                //{
                //    report.SetParameterValue("report.JournalVoucherTypes", "All Invoices");

                //}
                //else
                //{
                //    clsJournalVoucherTypes clsJournalVoucherTypes = new clsJournalVoucherTypes();
                //    DataTable dtJournalVoucherTypes = clsJournalVoucherTypes.SelectJournalVoucherTypes(Simulate.Integer32(dtHeader.Rows[0]["InvoiceTypeid"]));
                //    if (dtJournalVoucherTypes != null && dtJournalVoucherTypes.Rows.Count > 0)
                //    {
                //        report.SetParameterValue("report.JournalVoucherTypes", Simulate.String(dtJournalVoucherTypes.Rows[0]["AName"]));

                //    }
                //}


                report.SetParameterValue("report.Date", Simulate.StringToDate(dtHeader.Rows[0]["VoucherDate"]).ToString("yyyy-MM-dd"));




                FastreportStanderdParameters(report, UserId, CompanyID);
                //    report.Prepare();

                report.Prepare();

                return FastreporttoPDF(report);
                //return Json(PrepareFrxReport(report), JsonRequestBehavior.AllowGet);


            }
            catch (Exception ex)
            {

                return Json(ex);
            }

        }

        #endregion
        #endregion
        #region Items


        [HttpGet]
        [Route("SelectItemsByGuid")]
        public string SelectItemsByGuid(string Guid, string AName, string EName, String Barcode, int CategoryID, int IsPOS, int CompanyId)
        {
            try
            {

                clsItems clsItems = new clsItems();
                DataTable dt = clsItems.SelectItemsByGuid(Simulate.String(Guid), Simulate.String(AName), Simulate.String(EName), Simulate.String(Barcode), CategoryID, IsPOS, CompanyId);
                if (dt != null)
                {

                    string JSONString = string.Empty;
                    JSONString = JsonConvert.SerializeObject(dt);
                    return JSONString;
                }
                else
                {

                    return "";
                }
            }
            catch (Exception)
            {

                throw;
            }




        }
        [HttpGet]
        [Route("DeleteItemsByGuid")]
        public bool DeleteItemsByGuid(string Guid)
        {
            try
            {
                clsItems clsItems = new clsItems();
                bool A = clsItems.DeleteItemsByGuid(Guid);
                return A;
            }
            catch (Exception)
            {

                throw;
            }

        }
        [HttpPost]
        [Route("InsertItems")]
        public string InsertItems(string AName, string EName, string Description, decimal SalesPriceBeforeTax, decimal SalesPriceAfterTax, int CategoryID, int SalesTaxID
            , int SpecialSalesTaxID, int PurchaseTaxID, int SpecialPurchaseTaxID, string Barcode, int ReadType, int OriginID, decimal MinimumLimit, [FromBody] string Picture
            , bool IsActive, bool IsPOS, int BoxTypeID, bool IsStockItem, int POSOrder, int CompanyID, int CreationUserId)
        {
            try
            {
                byte[] myPicture = new Byte[64];
                if (myPicture != null && myPicture.Length > 0)
                {
                    myPicture = Convert.FromBase64String(Picture);
                }
                else
                {

                    myPicture = null;
                }

                clsItems clsItems = new clsItems();
                String A = clsItems.InsertItems(Simulate.String(AName), Simulate.String(EName), Simulate.String(Description),
                    Simulate.decimal_(SalesPriceBeforeTax), Simulate.decimal_(SalesPriceAfterTax), CategoryID, SalesTaxID
            , SpecialSalesTaxID, PurchaseTaxID, SpecialPurchaseTaxID, Simulate.String(Barcode), ReadType, OriginID, MinimumLimit, myPicture
            , IsActive, IsPOS, BoxTypeID, IsStockItem, POSOrder, CompanyID, CreationUserId);



                return A;
            }
            catch (Exception ex)
            {

                throw;
            }

        }
        [HttpPost]
        [Route("UpdateItems")]
        public int UpdateItems(string Guid, string AName, string EName, string Description, decimal SalesPriceBeforeTax, decimal SalesPriceAfterTax, int CategoryID, int SalesTaxID
            , int SpecialSalesTaxID, int PurchaseTaxID, int SpecialPurchaseTaxID, string Barcode, int ReadType, int OriginID, decimal MinimumLimit, [FromBody] string Picture
            , bool IsActive, bool IsPOS, int BoxTypeID, bool IsStockItem, int POSOrder, int ModificationUserId)
        {
            try
            {



                byte[] myPicture = new Byte[64];
                if (myPicture != null && myPicture.Length > 0)
                {
                    myPicture = Convert.FromBase64String(Picture);
                }
                else
                {

                    myPicture = null;
                }
                clsItems clsItems = new clsItems();
                int A = clsItems.UpdateItems(Guid, Simulate.String(AName), Simulate.String(EName), Simulate.String(Description),
                    Simulate.decimal_(SalesPriceBeforeTax), Simulate.decimal_(SalesPriceAfterTax), CategoryID, SalesTaxID
            , SpecialSalesTaxID, PurchaseTaxID, SpecialPurchaseTaxID, Simulate.String(Barcode), ReadType, OriginID, MinimumLimit, myPicture
            , IsActive, IsPOS, BoxTypeID, IsStockItem, POSOrder, ModificationUserId);
                return A;
            }
            catch (Exception)
            {

                throw;
            }

        }
        #endregion
        #region Tax


        [HttpGet]
        [Route("SelectTaxByID")]
        public string SelectTaxByID(int ID, int CompanyID)
        {
            try
            {

                clsTax clsTax = new clsTax();
                DataTable dt = clsTax.SelectTaxByID(ID, "", "", CompanyID);
                if (dt != null)
                {

                    string JSONString = string.Empty;
                    JSONString = JsonConvert.SerializeObject(dt);
                    return JSONString;
                }
                else
                {

                    return "";
                }
            }
            catch (Exception)
            {

                throw;
            }




        }
        [HttpGet]
        [Route("DeleteTaxByID")]
        public bool DeleteTaxByID(int ID)
        {
            try
            {
                clsTax clsTax = new clsTax();
                bool A = clsTax.DeleteTaxByID(ID);
                return A;
            }
            catch (Exception)
            {

                throw;
            }

        }
        [HttpGet]
        [Route("InsertTax")]
        public int InsertTax(string AName, string EName, decimal Value, bool IsSalesTax,
            bool IsPurchaseTax, bool IsSalesSpecialTax, bool IsSpecialPurchaseTax, int CompanyID, int CreationUserId)
        {
            try
            {
                clsTax clsTax = new clsTax();
                int A = clsTax.InsertTax(Simulate.String(AName), Simulate.String(EName), Value, IsSalesTax, IsPurchaseTax, IsSalesSpecialTax, IsSpecialPurchaseTax, CompanyID, CreationUserId);
                return A;
            }
            catch (Exception ex)
            {

                throw;
            }

        }
        [HttpGet]
        [Route("UpdateTax")]
        public int UpdateTax(int ID, string AName, string EName, decimal Value, bool IsSalesTax, bool IsPurchaseTax, bool IsSalesSpecialTax, bool IsSpecialPurchaseTax, int ModificationUserId)
        {
            try
            {
                clsTax clsTax = new clsTax();
                int A = clsTax.UpdateTax(ID, Simulate.String(AName), Simulate.String(EName), Value, IsSalesTax, IsPurchaseTax, IsSalesSpecialTax, IsSpecialPurchaseTax, ModificationUserId);
                return A;
            }
            catch (Exception)
            {

                throw;
            }

        }
        #endregion
        #region ReadType


        [HttpGet]
        [Route("SelectItemReadTypeByID")]
        public string SelectItemReadTypeByID(int ID, int CompanyID)
        {
            try
            {

                clsItemReadType clsItemReadType = new clsItemReadType();
                DataTable dt = clsItemReadType.SelectItemReadTypeByID(ID, "", "", CompanyID);
                if (dt != null)
                {

                    string JSONString = string.Empty;
                    JSONString = JsonConvert.SerializeObject(dt);
                    return JSONString;
                }
                else
                {

                    return "";
                }
            }
            catch (Exception)
            {

                throw;
            }




        }
        [HttpGet]
        [Route("DeleteItemReadTypeByID")]
        public bool DeleteItemReadTypeByID(int ID)
        {
            try
            {
                clsItemReadType ItemReadType = new clsItemReadType();
                bool A = ItemReadType.DeleteItemReadTypeByID(ID);
                return A;
            }
            catch (Exception)
            {

                throw;
            }

        }
        [HttpGet]
        [Route("InsertItemReadType")]
        public int InsertItemReadType(string AName, string EName, int CompanyID, int CreationUserId)
        {
            try
            {
                clsItemReadType clsItemReadType = new clsItemReadType();
                int A = clsItemReadType.InsertItemReadType(Simulate.String(AName), Simulate.String(EName), CompanyID, CreationUserId);
                return A;
            }
            catch (Exception ex)
            {

                throw;
            }

        }
        [HttpGet]
        [Route("UpdateItemReadType")]
        public int UpdateItemReadType(int ID, string AName, string EName, int ModificationUserId)
        {
            try
            {
                clsItemReadType clsItemReadType = new clsItemReadType();
                int A = clsItemReadType.UpdateItemReadType(ID, Simulate.String(AName), Simulate.String(EName), ModificationUserId);
                return A;
            }
            catch (Exception)
            {

                throw;
            }

        }
        #endregion
        #region Countries


        [HttpGet]
        [Route("SelectCountriesByID")]
        public string SelectCountriesByID(int ID, int CompanyID)
        {
            try
            {

                clsCountries clsCountries = new clsCountries();
                DataTable dt = clsCountries.SelectCountriesByID(ID, "", "", CompanyID);
                if (dt != null)
                {

                    string JSONString = string.Empty;
                    JSONString = JsonConvert.SerializeObject(dt);
                    return JSONString;
                }
                else
                {

                    return "";
                }
            }
            catch (Exception)
            {

                throw;
            }




        }
        [HttpGet]
        [Route("DeleteCountriesByID")]
        public bool DeleteCountriesByID(int ID)
        {
            try
            {
                clsCountries Countries = new clsCountries();
                bool A = Countries.DeleteCountriesByID(ID);
                return A;
            }
            catch (Exception)
            {

                throw;
            }

        }
        [HttpGet]
        [Route("InsertCountries")]
        public int InsertCountries(string AName, string EName, int CompanyID, int CreationUserId)
        {
            try
            {
                clsCountries clsCountries = new clsCountries();
                int A = clsCountries.InsertCountries(Simulate.String(AName), Simulate.String(EName), CompanyID, CreationUserId);
                return A;
            }
            catch (Exception ex)
            {

                throw;
            }

        }
        [HttpGet]
        [Route("UpdateCountries")]
        public int UpdateCountries(int ID, string AName, string EName, int ModificationUserId)
        {
            try
            {
                clsCountries clsCountries = new clsCountries();
                int A = clsCountries.UpdateCountries(ID, Simulate.String(AName), Simulate.String(EName), ModificationUserId);
                return A;
            }
            catch (Exception)
            {

                throw;
            }

        }
        #endregion
        #region Item Box Type


        [HttpGet]
        [Route("SelectItemsBoxTypeByID")]
        public string SelectItemsBoxTypeByID(int ID, int CompanyID)
        {
            try
            {

                clsItemsBoxType clsItemsBoxType = new clsItemsBoxType();
                DataTable dt = clsItemsBoxType.SelectItemsBoxTypeByID(ID, "", "", CompanyID);
                if (dt != null)
                {

                    string JSONString = string.Empty;
                    JSONString = JsonConvert.SerializeObject(dt);
                    return JSONString;
                }
                else
                {

                    return "";
                }
            }
            catch (Exception)
            {

                throw;
            }




        }
        [HttpGet]
        [Route("DeleteItemsBoxTypeByID")]
        public bool DeleteItemsBoxTypeByID(int ID)
        {
            try
            {
                clsItemsBoxType clsItemsBoxType = new clsItemsBoxType();
                bool A = clsItemsBoxType.DeleteItemsBoxTypeByID(ID);
                return A;
            }
            catch (Exception)
            {

                throw;
            }

        }
        [HttpGet]
        [Route("InsertItemsBoxType")]
        public int InsertItemsBoxType(string AName, string EName, decimal Qty, int CompanyID, int CreationUserId)
        {
            try
            {
                clsItemsBoxType clsItemsBoxType = new clsItemsBoxType();
                int A = clsItemsBoxType.InsertItemsBoxType(Simulate.String(AName), Simulate.String(EName), Simulate.decimal_(Qty), CompanyID, CreationUserId);
                return A;
            }
            catch (Exception ex)
            {

                throw;
            }

        }
        [HttpGet]
        [Route("UpdateItemsBoxType")]
        public int UpdateItemsBoxType(int ID, string AName, string EName, decimal Qty, int ModificationUserId)
        {
            try
            {
                clsItemsBoxType clsItemsBoxType = new clsItemsBoxType();
                int A = clsItemsBoxType.UpdateItemsBoxType(ID, Simulate.String(AName), Simulate.String(EName), Simulate.decimal_(Qty), ModificationUserId);
                return A;
            }
            catch (Exception)
            {

                throw;
            }

        }
        #endregion
        #region Invoices Header

        [HttpGet]
        [Route("SelectInvoiceHeaderByGuid")]
        public string SelectInvoiceHeaderByGuid(string Guid, int BranchID, int InvoiceTypeID, int CompanyID, DateTime Date1, DateTime Date2)
        {
            try
            {
                clsInvoiceHeader clsInvoiceHeader = new clsInvoiceHeader();
                DataTable dt = clsInvoiceHeader.SelectInvoiceHeaderByGuid(Simulate.String(Guid), Date1, Date2, InvoiceTypeID, BranchID, CompanyID);
                if (dt != null)
                {

                    string JSONString = string.Empty;
                    JSONString = JsonConvert.SerializeObject(dt);
                    return JSONString;
                }
                else
                {

                    return "";
                }
            }
            catch (Exception)
            {

                throw;
            }
        }
        [HttpGet]
        [Route("DeleteInvoiceDetailsByHeaderGuid")]
        public bool DeleteInvoiceDetailsByHeaderGuid(string Guid)
        {
            try
            {
                clsInvoiceDetails clsInvoiceDetails = new clsInvoiceDetails();
                clsInvoiceHeader clsInvoiceHeader = new clsInvoiceHeader();

                clsJournalVoucherHeader clsJournalVoucherHeader = new clsJournalVoucherHeader();
                clsJournalVoucherDetails clsJournalVoucherDetails = new clsJournalVoucherDetails();
                SqlTransaction trn; clsSQL clsSQL = new clsSQL();
                SqlConnection con = new SqlConnection(clsSQL.conString);
                con.Open();
                trn = con.BeginTransaction(); int A = 0;
                bool IsSaved = true;
                try
                {
                    DataTable dt = clsInvoiceHeader.SelectInvoiceHeaderByGuid(Guid, Simulate.StringToDate("1900-01-01"), Simulate.StringToDate("2300-01-01"), 0, 0, 0, trn);
                    IsSaved = clsInvoiceHeader.DeleteInvoiceHeaderByGuid(Guid, trn);
                    bool a = clsInvoiceDetails.DeleteInvoiceDetailsByHeaderGuid(Guid, trn);
                    if (dt != null && dt.Rows.Count > 0)
                    {
                        string JVGuid = Simulate.String(dt.Rows[0]["JVGuid"]);
                        bool aa = clsJournalVoucherHeader.DeleteJournalVoucherHeaderByID(JVGuid, trn);
                        bool aaa = clsJournalVoucherDetails.DeleteJournalVoucherDetailsByParentId(JVGuid, trn);
                    }
                    if (!a)
                        IsSaved = false;


                    if (IsSaved)
                        trn.Commit();
                    else
                        trn.Rollback();
                }
                catch (Exception)
                {
                    trn.Rollback();

                }
                finally { con.Close(); }


                return IsSaved;
            }
            catch (Exception)
            {

                throw;
            }

        }
        [HttpPost]
        [Route("InsertInvoiceHeader")]

        public string InsertInvoiceHeader(int branchID, int storeID, int businessPartnerID
            , int cashID, string refNo, int invoiceNo, decimal headerDiscount
            , int invoiceTypeID, bool isCounted, string note, int companyID,
            decimal totalTax, string pOSDayGuid, string relatedInvoiceGuid,
            decimal totalDiscount, int paymentMethodID,
            string pOSSessionGuid, decimal totalInvoice,
            DateTime invoiceDate, int creationUserId, int accountID,
            [FromBody] string DetailsList)

        {
            try
            {
                DBInvoiceHeader dbInvoiceHeader = new DBInvoiceHeader
                {
                    BranchID = Simulate.Integer32(branchID),
                    StoreID = Simulate.Integer32(storeID),
                    CompanyID = Simulate.Integer32(companyID),
                    CreationUserID = Simulate.Integer32(creationUserId),
                    InvoiceDate = Simulate.StringToDate(invoiceDate),
                    BusinessPartnerID = Simulate.Integer32(businessPartnerID),
                    CashID = Simulate.Integer32(cashID),
                    CreationDate = DateTime.Now,
                    RefNo = Simulate.String(refNo),
                    HeaderDiscount = Simulate.decimal_(headerDiscount),
                    InvoiceNo = Simulate.Integer32(invoiceNo),
                    InvoiceTypeID = Simulate.Integer32(invoiceTypeID),
                    IsCounted = Simulate.Bool(isCounted),
                    Note = Simulate.String(note),
                    TotalTax = Simulate.decimal_(totalTax),
                    POSDayGuid = Simulate.Guid(pOSDayGuid),
                    RelatedInvoiceGuid = Simulate.Guid(relatedInvoiceGuid),
                    TotalDiscount = Simulate.decimal_(totalDiscount),
                    PaymentMethodID = Simulate.Integer32(paymentMethodID),
                    POSSessionGuid = Simulate.Guid(pOSSessionGuid),
                    TotalInvoice = Simulate.decimal_(totalInvoice),
                    AccountID = accountID,
                    Guid = Simulate.Guid(""),


                };


                List<DBInvoiceDetails> details = JsonConvert.DeserializeObject<List<DBInvoiceDetails>>(DetailsList);
                clsInvoiceHeader clsInvoiceHeader = new clsInvoiceHeader();
                clsInvoiceDetails clsInvoiceDetails = new clsInvoiceDetails();
                SqlTransaction trn; clsSQL clsSQL = new clsSQL();
                SqlConnection con = new SqlConnection(clsSQL.conString);
                con.Open();
                trn = con.BeginTransaction(); string A = "";
                try
                {
                    bool IsSaved = true;

                    A = clsInvoiceHeader.InsertInvoiceHeader(dbInvoiceHeader, trn);
                    if (A == "")
                    { IsSaved = false; }
                    else
                    {
                        for (int i = 0; i < details.Count; i++)
                        {
                            string c = clsInvoiceDetails.InsertInvoiceDetails(details[i], A, trn);
                            if (c == "")
                                IsSaved = false;
                        }

                    }

                    if (IsSaved)
                        IsSaved = clsInvoiceHeader.InsertInvoiceJournalVoucher(details, accountID, paymentMethodID, cashID, businessPartnerID, headerDiscount, Simulate.Integer32(branchID), Simulate.String(note), Simulate.Integer32(companyID), Simulate.StringToDate(invoiceDate), creationUserId, invoiceTypeID, A, trn);
                    if (IsSaved)
                    { trn.Commit(); return A; }
                    else
                    { trn.Rollback(); return ""; }

                }
                catch (Exception)
                {

                    trn.Rollback();
                    return "";
                }
                finally { con.Close(); }

            }
            catch (Exception ex)
            {

                return "";
            }

        }
        [Route("UpdateInvoiceHeader")]
        public string UpdateInvoiceHeader(int branchID, int storeID, int businessPartnerID
            , int cashID, string refNo, int invoiceNo, decimal headerDiscount
            , int invoiceTypeID, bool isCounted, string note,
            decimal totalTax, string pOSDayGuid, string relatedInvoiceGuid,
            decimal totalDiscount, int paymentMethodID,
            string pOSSessionGuid, decimal totalInvoice,
            DateTime invoiceDate, int modificationUserID, string guid, int accountID,
            [FromBody] string DetailsList)
        {





            try
            {

                DBInvoiceHeader dbInvoiceHeader = new DBInvoiceHeader
                {
                    BranchID = branchID,
                    StoreID = storeID,

                    ModificationUserID = modificationUserID,
                    InvoiceDate = invoiceDate,
                    BusinessPartnerID = businessPartnerID,
                    CashID = cashID,
                    ModificationDate = DateTime.Now,
                    RefNo = Simulate.String(refNo),
                    HeaderDiscount = headerDiscount,
                    InvoiceNo = invoiceNo,
                    InvoiceTypeID = invoiceTypeID,
                    IsCounted = isCounted,
                    Note = Simulate.String(note),
                    TotalTax = totalTax,
                    POSDayGuid = Simulate.Guid(pOSDayGuid),
                    RelatedInvoiceGuid = Simulate.Guid(relatedInvoiceGuid),
                    TotalDiscount = totalDiscount,
                    PaymentMethodID = paymentMethodID,
                    POSSessionGuid = Simulate.Guid(pOSSessionGuid),
                    TotalInvoice = totalInvoice,
                    AccountID = accountID,
                    Guid = Simulate.Guid(guid),
                };

                List<DBInvoiceDetails> details = JsonConvert.DeserializeObject<List<DBInvoiceDetails>>(DetailsList);
                clsInvoiceHeader clsInvoiceHeader = new clsInvoiceHeader();
                clsInvoiceDetails clsInvoiceDetails = new clsInvoiceDetails();
                SqlTransaction trn; clsSQL clsSQL = new clsSQL();
                SqlConnection con = new SqlConnection(clsSQL.conString);
                con.Open();
                trn = con.BeginTransaction();
                string A = "";
                try
                {
                    bool IsSaved = true;

                    A = clsInvoiceHeader.UpdateInvoiceHeader(dbInvoiceHeader, trn);
                    clsInvoiceDetails.DeleteInvoiceDetailsByHeaderGuid(guid, trn);
                    for (int i = 0; i < details.Count; i++)
                    {

                        string c = clsInvoiceDetails.InsertInvoiceDetails(details[i], guid, trn);
                        if (c == "")
                            IsSaved = false;
                    }
                    if (IsSaved)
                        IsSaved = clsInvoiceHeader.InsertInvoiceJournalVoucher(details, accountID, paymentMethodID, cashID, businessPartnerID, headerDiscount, Simulate.Integer32(branchID), Simulate.String(note), 0, Simulate.StringToDate(invoiceDate), modificationUserID, invoiceTypeID, guid, trn);
                    if (IsSaved)
                    { trn.Commit(); return A; }
                    else
                    { trn.Rollback(); return ""; }
                }
                catch (Exception)
                {
                    A = "";
                    trn.Rollback();
                }
                finally { con.Close(); }
                return A;
            }
            catch (Exception)
            {

                throw;
            }

        }

        #endregion
        #region Invoice Details
        [HttpGet]
        [Route("SelectInvoiceDetailsByHeaderGuid")]
        public string SelectInvoiceDetailsByHeaderGuid(string HeaderGuid, string ItemGuid, int CompanyID)
        {
            try
            {
                clsInvoiceDetails clsInvoiceDetails = new clsInvoiceDetails();
                DataTable dt = clsInvoiceDetails.SelectInvoiceDetailsByHeaderGuid(Simulate.String(HeaderGuid), Simulate.String(ItemGuid), CompanyID);
                if (dt != null)
                {

                    string JSONString = string.Empty;
                    JSONString = JsonConvert.SerializeObject(dt);
                    return JSONString;
                }
                else
                {

                    return "";
                }
            }
            catch (Exception)
            {

                throw;
            }
        }
        #endregion
        #region Store


        [HttpGet]
        [Route("SelectStoreByID")]
        public string SelectStoreByID(int ID, int CompanyID)
        {
            try
            {

                clsStore clsStore = new clsStore();
                DataTable dt = clsStore.SelectStoreByID(ID, "", "", CompanyID);
                if (dt != null)
                {

                    string JSONString = string.Empty;
                    JSONString = JsonConvert.SerializeObject(dt);
                    return JSONString;
                }
                else
                {

                    return "";
                }
            }
            catch (Exception)
            {

                throw;
            }




        }
        [HttpGet]
        [Route("DeleteStoreByID")]
        public bool DeleteStoreByID(int ID)
        {
            try
            {
                clsStore clsStore = new clsStore();
                bool A = clsStore.DeleteStoreByID(ID);
                return A;
            }
            catch (Exception)
            {

                throw;
            }

        }
        [HttpGet]
        [Route("InsertStore")]
        public int InsertStore(string AName, string EName, int BranchID, int CompanyID, int CreationUserId)
        {
            try
            {
                clsStore clsStore = new clsStore();

                int A = clsStore.InsertStore(Simulate.String(AName), Simulate.String(EName), BranchID, CompanyID, CreationUserId);
                return A;
            }
            catch (Exception ex)
            {

                throw;
            }

        }
        [HttpGet]
        [Route("UpdateStore")]
        public int UpdateStore(int ID, string AName, string EName, int BranchID, int ModificationUserId)
        {
            try
            {
                clsStore clsStore = new clsStore();
                int A = clsStore.UpdateStore(ID, Simulate.String(AName), Simulate.String(EName), BranchID, ModificationUserId);
                return A;
            }
            catch (Exception)
            {

                throw;
            }

        }
        #endregion
        #region Cash Drawer


        [HttpGet]
        [Route("SelectCashDrawerByID")]
        public string SelectCashDrawerByID(int ID, int CompanyID)
        {
            try
            {

                clsCashDrawer clsCashDrawer = new clsCashDrawer();
                DataTable dt = clsCashDrawer.SelectCashDrawerByID(ID, "", "", CompanyID);
                if (dt != null)
                {

                    string JSONString = string.Empty;
                    JSONString = JsonConvert.SerializeObject(dt);
                    return JSONString;
                }
                else
                {

                    return "";
                }
            }
            catch (Exception)
            {

                throw;
            }




        }
        [HttpGet]
        [Route("DeleteCashDrawerByID")]
        public bool DeleteCashDrawerByID(int ID,int CashAccountID)
        {
            try
            {
                clsJournalVoucherDetails clsJournalVoucherDetails = new clsJournalVoucherDetails();
                DataTable dt = clsJournalVoucherDetails.SelectJournalVoucherDetailsByParentId("", CashAccountID, ID, 0, 0, 0);
                if (dt != null && dt.Rows.Count > 0)
                {

                    return false;
                }
                clsCashDrawer clsCashDrawer = new clsCashDrawer();
                bool A = clsCashDrawer.DeleteCashDrawerByID(ID);
                return A;
            }
            catch (Exception)
            {

                throw;
            }

        }
        [HttpGet]
        [Route("InsertCashDrawer")]
        public int InsertCashDrawer(string AName, string EName, int BranchID, int CompanyID, int CreationUserId)
        {
            try
            {
                clsCashDrawer clsCashDrawer = new clsCashDrawer();
                int A = clsCashDrawer.InsertCashDrawer(Simulate.String(AName), Simulate.String(EName), BranchID, CompanyID, CreationUserId);
                return A;
            }
            catch (Exception ex)
            {

                throw;
            }

        }
        [HttpGet]
        [Route("UpdateCashDrawer")]
        public int UpdateCashDrawer(int ID, string AName, string EName, int BranchID, int ModificationUserId)
        {
            try
            {
                clsCashDrawer clsCashDrawer = new clsCashDrawer();
                int A = clsCashDrawer.UpdateCashDrawer(ID, Simulate.String(AName), Simulate.String(EName), BranchID, ModificationUserId);
                return A;
            }
            catch (Exception)
            {

                throw;
            }

        }
        #endregion
        #region Payment Method


        [HttpGet]
        [Route("SelectPaymentMethodByID")]
        public string SelectPaymentMethodByID(int ID, int CompanyID)
        {
            try
            {
                CompanyID = 0;
                clsPaymentMethod clsPaymentMethod = new clsPaymentMethod();
                DataTable dt = clsPaymentMethod.SelectPaymentMethodByID(ID, "", "", CompanyID);
                if (dt != null)
                {

                    string JSONString = string.Empty;
                    JSONString = JsonConvert.SerializeObject(dt);
                    return JSONString;
                }
                else
                {

                    return "";
                }
            }
            catch (Exception)
            {

                throw;
            }




        }
        [HttpGet]
        [Route("DeletePaymentMethodByID")]
        public bool DeletePaymentMethodByID(int ID)
        {
            try
            {
                clsPaymentMethod clsPaymentMethod = new clsPaymentMethod();
                bool A = clsPaymentMethod.DeletePaymentMethodByID(ID);
                return A;
            }
            catch (Exception)
            {

                throw;
            }

        }
        [HttpGet]
        [Route("InsertPaymentMethod")]
        public int InsertPaymentMethod(string AName, string EName, int BranchID, int CompanyID, int CreationUserId)
        {
            try
            {
                clsPaymentMethod clsPaymentMethod = new clsPaymentMethod();
                int A = clsPaymentMethod.InsertPaymentMethod(Simulate.String(AName), Simulate.String(EName), BranchID, CompanyID, CreationUserId);
                return A;
            }
            catch (Exception ex)
            {

                throw;
            }

        }
        [HttpGet]
        [Route("UpdatePaymentMethod")]
        public int UpdatePaymentMethod(int ID, string AName, string EName, int BranchID, int ModificationUserId)
        {
            try
            {
                clsPaymentMethod clsPaymentMethod = new clsPaymentMethod();
                int A = clsPaymentMethod.UpdatePaymentMethod(ID, Simulate.String(AName), Simulate.String(EName), BranchID, ModificationUserId);
                return A;
            }
            catch (Exception)
            {

                throw;
            }

        }
        #endregion
        #region Account Setting

        [HttpGet]
        [Route("SelectAccountSetting")]
        public string SelectAccountSetting(int ID, int AccountRefID, int CompanyID)
        {
            try
            {

                cls_AccountSetting cls_AccountSetting = new cls_AccountSetting();
                DataTable dt = cls_AccountSetting.SelectAccountSetting(ID, AccountRefID, CompanyID);
                if (dt != null)
                {

                    string JSONString = string.Empty;
                    JSONString = JsonConvert.SerializeObject(dt);
                    return JSONString;
                }
                else
                {

                    return "";
                }
            }
            catch (Exception)
            {

                throw;
            }




        }

        [HttpGet]
        [Route("InsertAccountSetting")]
        public int InsertAccountSetting(int AccountRefID, int AccountID, int CompanyID, int CreationUserId)
        {
            try
            {
                cls_AccountSetting cls_AccountSetting = new cls_AccountSetting();
                cls_AccountSetting.DeActivateAccountSettingByID(0, AccountRefID, CompanyID);
                int A = cls_AccountSetting.InsertAccountSetting(AccountRefID, AccountID, CompanyID, CreationUserId);
                return A;
            }
            catch (Exception ex)
            {

                throw;
            }

        }

        #endregion
        #region POS Setting


        [HttpGet]
        [Route("SelectPOSSettingByID")]
        public string SelectPOSSettingByID(int ID, int CashDrawerID, int POSSettingID, int CompanyID)
        {
            try
            {

                clsPOSSetting clsPOSSetting = new clsPOSSetting();
                DataTable dt = clsPOSSetting.SelectPOSSettingByID(ID, CashDrawerID, POSSettingID, CompanyID);
                if (dt != null)
                {

                    string JSONString = string.Empty;
                    JSONString = JsonConvert.SerializeObject(dt);
                    return JSONString;
                }
                else
                {

                    return "";
                }
            }
            catch (Exception)
            {

                throw;
            }




        }
        [HttpGet]
        [Route("DeletePOSSettingByID")]
        public bool DeletePOSSettingByID(int ID, int CompanyID)
        {
            try
            {

                clsPOSSetting clsPOSSetting = new clsPOSSetting();
                bool A = clsPOSSetting.DeletePOSSettingByID(ID, CompanyID);
                return A;
            }
            catch (Exception)
            {

                throw;
            }

        }
        [HttpGet]
        [Route("InsertPOSSetting")]
        public int InsertPOSSetting(int CashDrawerID, int POSSettingID, string Value, int CompanyID, int CreationUserId)
        {
            try
            {
                clsPOSSetting clsPOSSetting = new clsPOSSetting();
                int A = clsPOSSetting.InsertPOSSetting(CashDrawerID, POSSettingID, Simulate.String(Value), CompanyID, CreationUserId);
                return A;
            }
            catch (Exception ex)
            {

                throw;
            }

        }
        [HttpGet]
        [Route("UpdatePOSSetting")]
        public int UpdatePOSSetting(int ID, int CashDrawerID, int POSSettingID, int Value, int ModificationUserId)
        {
            try
            {
                clsPOSSetting clsPOSSetting = new clsPOSSetting();
                int A = clsPOSSetting.UpdatePOSSetting(ID, CashDrawerID, POSSettingID, Simulate.String(Value), ModificationUserId);
                return A;
            }
            catch (Exception)
            {

                throw;
            }

        }
        #endregion
        #region POS Day


        [HttpGet]
        [Route("SelectPOSDayByGuid")]
        public string SelectPOSDayByGuid(string Guid, int Status, int CompanyID)
        {
            try
            {

                clsPOSDay clsPOSDay = new clsPOSDay();
                DataTable dt = clsPOSDay.SelectPOSDayByGuid(Guid, Status, CompanyID);
                if (dt != null)
                {

                    string JSONString = string.Empty;
                    JSONString = JsonConvert.SerializeObject(dt);
                    return JSONString;
                }
                else
                {

                    return "";
                }
            }
            catch (Exception)
            {

                throw;
            }




        }
        [HttpGet]
        [Route("DeletePOSDayByGuid")]
        public bool DeletePOSDayByGuid(string Guid)
        {
            try
            {

                clsPOSDay clsPOSDay = new clsPOSDay();
                bool A = clsPOSDay.DeletePOSDayByGuid(Guid);
                return A;
            }
            catch (Exception)
            {

                throw;
            }

        }
        [HttpGet]
        [Route("InsertPOSDay")]
        public String InsertPOSDay(DateTime StartDate, DateTime EndDate, DateTime POSDate, int Status, int CompanyID, int CreationUserId)
        {
            try
            {
                clsPOSDay clsPOSDay = new clsPOSDay();
                String A = clsPOSDay.InsertPOSDay(StartDate, EndDate, POSDate, Status, CompanyID, CreationUserId);
                return A;
            }
            catch (Exception ex)
            {

                throw;
            }

        }
        [HttpGet]
        [Route("UpdatePOSDay")]
        public int UpdatePOSDay(string Guid, DateTime StartDate, DateTime EndDate, DateTime POSDate, int Status, int ModificationUserId)
        {
            try
            {
                clsPOSDay clsPOSDay = new clsPOSDay();
                int A = clsPOSDay.UpdatePOSDay(Guid, StartDate, EndDate, POSDate, Status, ModificationUserId);
                return A;
            }
            catch (Exception)
            {

                throw;
            }

        }
        [Route("OpenNewPOSDay")]
        public string OpenNewPOSDay(string Guid, DateTime NewDate, DateTime StartDate, DateTime EndDate, int CompanyID, int CreationUserId)
        {
            try
            {

                clsPOSDay clsPOSDay = new clsPOSDay();


                SqlTransaction trn; clsSQL clsSQL = new clsSQL();
                SqlConnection con = new SqlConnection(clsSQL.conString);
                con.Open();
                trn = con.BeginTransaction();
                try
                {
                    bool IsSaved = true;
                    int A = clsPOSDay.ClosePOSDay(Guid, EndDate, CreationUserId, trn);


                    string NewDay = clsPOSDay.InsertPOSDay(DateTime.Now, DateTime.Now, NewDate, 1, CompanyID, CreationUserId, trn);
                    if (NewDay == "")
                    { IsSaved = false; }

                    if (IsSaved)
                    { trn.Commit(); return NewDay; }
                    else
                    { trn.Rollback(); return ""; }

                }
                catch (Exception)
                {

                    trn.Rollback();
                    return "";
                }
                finally { con.Close(); }
            }
            catch (Exception)
            {

                throw;
            }

        }
        #endregion
        #region POS Sessions


        [HttpGet]
        [Route("SelectPOSSessionsByGuid")]
        public string SelectPOSSessionsByGuid(string Guid, string POSDayGuid, int Status, int CompanyID)
        {
            try
            {

                clsPOSSessions clsPOSSessions = new clsPOSSessions();
                DataTable dt = clsPOSSessions.SelectPOSSessionsByGuid(Guid, POSDayGuid, Status, CompanyID);
                if (dt != null)
                {

                    string JSONString = string.Empty;
                    JSONString = JsonConvert.SerializeObject(dt);
                    return JSONString;
                }
                else
                {

                    return "";
                }
            }
            catch (Exception)
            {

                throw;
            }




        }
        [HttpGet]
        [Route("DeletePOSSessionsByGuid")]
        public bool DeletePOSSessionsByGuid(string Guid)
        {
            try
            {

                clsPOSSessions clsPOSSessions = new clsPOSSessions();
                bool A = clsPOSSessions.DeletePOSSessionsByGuid(Guid);
                return A;
            }
            catch (Exception)
            {

                throw;
            }

        }
        [HttpGet]
        [Route("InsertPOSSessions")]
        public String InsertPOSSessions(string POSDayGuid, int SessionTypeID, DateTime StartDate, DateTime EndDate, int CashDrawerID, int Status, int CompanyID, int CreationUserId)
        {
            try
            {
                clsPOSSessions clsPOSSessions = new clsPOSSessions();
                String A = clsPOSSessions.InsertPOSSessions(POSDayGuid, SessionTypeID, StartDate, EndDate, CashDrawerID, Status, CompanyID, CreationUserId);
                return A;
            }
            catch (Exception ex)
            {

                throw;
            }

        }
        [HttpGet]
        [Route("UpdatePOSSessions")]
        public int UpdatePOSSessions(string Guid, int SessionTypeID, string POSDayGuid, DateTime StartDate, DateTime EndDate, int CashDrawerID, int Status, int ModificationUserId)
        {
            try
            {
                clsPOSSessions clsPOSSessions = new clsPOSSessions();
                int A = clsPOSSessions.UpdatePOSSessions(Guid, SessionTypeID, POSDayGuid, StartDate, EndDate, CashDrawerID, Status, ModificationUserId);
                return A;
            }
            catch (Exception)
            {

                throw;
            }

        }


        [Route("OpenNewPOSSessions")]
        public string OpenNewPOSSessions(string Guid, int SessionTypeID, string POSDayGuid, DateTime NewDate, DateTime StartDate, DateTime EndDate, int CashDrawerID, int CompanyID, int CreationUserId)
        {
            try
            {

                clsPOSSessions clsPOSSessions = new clsPOSSessions();


                SqlTransaction trn; clsSQL clsSQL = new clsSQL();
                SqlConnection con = new SqlConnection(clsSQL.conString);
                con.Open();
                trn = con.BeginTransaction();
                try
                {
                    bool IsSaved = true;
                    int A = clsPOSSessions.ClosePOSSessions(Guid, EndDate, CreationUserId, trn);


                    string NewSession = clsPOSSessions.InsertPOSSessions(POSDayGuid, SessionTypeID, NewDate, DateTime.Now, CashDrawerID, 1, CompanyID, CreationUserId, trn);
                    if (NewSession == "")
                    { IsSaved = false; }

                    if (IsSaved)
                    { trn.Commit(); return NewSession; }
                    else
                    { trn.Rollback(); return ""; }

                }
                catch (Exception)
                {

                    trn.Rollback();
                    return "";
                }
                finally { con.Close(); }
            }
            catch (Exception)
            {

                throw;
            }

        }
        #endregion
        #region Journal Voucher Types


        [HttpGet]
        [Route("SelectJournalVoucherTypes")]
        public string SelectJournalVoucherTypes(int type, int CompanyID)
        {
            try
            {

                clsJournalVoucherTypes clsJournalVoucherTypes = new clsJournalVoucherTypes();
                DataTable dt = clsJournalVoucherTypes.SelectJournalVoucherTypes(type);
                if (dt != null)
                {

                    string JSONString = string.Empty;
                    JSONString = JsonConvert.SerializeObject(dt);
                    return JSONString;
                }
                else
                {

                    return "";
                }
            }
            catch (Exception)
            {

                throw;
            }




        }

        #endregion
        #region Dashboard


        [HttpGet]
        [Route("SelectSalesGroupByVoucherType")]
        public string SelectSalesGroupByVoucherType(DateTime date1, DateTime date2, int CompanyID)
        {
            try
            {
                clsSQL clssql = new clsSQL();
                SqlParameter[] prm =
                 {



                        new SqlParameter("@date1", SqlDbType.DateTime) { Value = date1 },
                    new SqlParameter("@date2", SqlDbType.DateTime) { Value = date2 },



                        new SqlParameter("@CompanyID", SqlDbType.Int) { Value =CompanyID },



                };
                string a = @"select tbl_JournalVoucherTypes.AName,sum(TotalInvoice) as TotalInvoice  from tbl_InvoiceHeader
 inner join tbl_JournalVoucherTypes on tbl_JournalVoucherTypes.ID = tbl_InvoiceHeader.InvoiceTypeID
 where(CompanyID = @companyID or @companyID = 0)
 and cast(invoicedate as date )between cast(@date1 as date) and cast(@date2 as date)
 group by tbl_JournalVoucherTypes.AName";
                DataTable dt = clssql.ExecuteQueryStatement(a, prm);


                if (dt != null)
                {

                    string JSONString = string.Empty;
                    JSONString = JsonConvert.SerializeObject(dt);
                    return JSONString;
                }
                else
                {

                    return "";
                }
            }
            catch (Exception)
            {

                throw;
            }




        }


        [HttpGet]
        [Route("SelectInvoiceByDate")]
        public string SelectInvoiceByDate(DateTime date1, DateTime date2, String InvoiceTypeID, int CompanyID)
        {
            try
            {
                clsSQL clssql = new clsSQL();
                SqlParameter[] prm =
                 {



                        new SqlParameter("@date1", SqlDbType.DateTime) { Value = date1 },
                    new SqlParameter("@date2", SqlDbType.DateTime) { Value = date2 },



                        new SqlParameter("@CompanyID", SqlDbType.Int) { Value =CompanyID },



                };
                string a = @"  
select q.Date,(select isnull( sum(TotalInvoice),0)from tbl_InvoiceHeader
 where (companyid=@companyID or @companyID=0)and (InvoiceTypeID in (" + InvoiceTypeID + @"))
and cast(tbl_InvoiceHeader.InvoiceDate as date)=cast(q.Date as date)  ) as TotalInvoice  from (
SELECT  TOP (DATEDIFF(DAY, @date1, @date2) + 1)
        Date = DATEADD(DAY, ROW_NUMBER() OVER(ORDER BY a.object_id) - 1, @date1) 
FROM    sys.all_objects a
        CROSS JOIN sys.all_objects b) as q";
                DataTable dt = clssql.ExecuteQueryStatement(a, prm);


                if (dt != null)
                {

                    string JSONString = string.Empty;
                    JSONString = JsonConvert.SerializeObject(dt);
                    return JSONString;
                }
                else
                {

                    return "";
                }
            }
            catch (Exception)
            {

                throw;
            }




        }
        #endregion
        #region Cash Voucher

        [HttpGet]
        [Route("SelectCashVoucherHeaderByGuid")]
        public string SelectCashVoucherHeaderByGuid(string Guid, int BranchID, int VoucherTypeID, int CompanyID, DateTime Date1, DateTime Date2)
        {
            try
            {
                clsCashVoucherHeader clsCashVoucherHeader = new clsCashVoucherHeader();
                DataTable dt = clsCashVoucherHeader.SelectCashVoucherHeaderByGuid(Simulate.String(Guid), Date1, Date2, VoucherTypeID, BranchID, CompanyID);
                if (dt != null)
                {

                    string JSONString = string.Empty;
                    JSONString = JsonConvert.SerializeObject(dt);
                    return JSONString;
                }
                else
                {

                    return "";
                }
            }
            catch (Exception)
            {

                throw;
            }
        }
        [HttpGet]
        [Route("DeleteCashVoucherHeaderByGuid")]
        public bool DeleteCashVoucherHeaderByGuid(string Guid)
        {
            try
            {
                clsCashVoucherDetails clsCashVoucherDetails = new clsCashVoucherDetails();
                clsCashVoucherHeader clsCashVoucherHeader = new clsCashVoucherHeader();
                clsJournalVoucherHeader clsJournalVoucherHeader = new clsJournalVoucherHeader();
                clsJournalVoucherDetails clsJournalVoucherDetails = new clsJournalVoucherDetails();
                SqlTransaction trn; clsSQL clsSQL = new clsSQL();
                SqlConnection con = new SqlConnection(clsSQL.conString);
                con.Open();
                trn = con.BeginTransaction(); int A = 0;
                bool IsSaved = true;
                try
                {
                    DataTable dt = clsCashVoucherHeader.SelectCashVoucherHeaderByGuid(Guid, Simulate.StringToDate("1900-01-01"), Simulate.StringToDate("2300-01-01"), 0, 0, 0, trn);
                    IsSaved = clsCashVoucherHeader.DeleteCashVoucherHeaderByGuid(Guid, trn);
                    bool a = clsCashVoucherDetails.DeleteCashVoucherDetailsByHeaderGuid(Guid, trn);
                    if (dt != null && dt.Rows.Count > 0)
                    {
                        string JVGuid = Simulate.String(dt.Rows[0]["JVGuid"]);
                        bool aa = clsJournalVoucherHeader.DeleteJournalVoucherHeaderByID(JVGuid, trn);
                        bool aaa = clsJournalVoucherDetails.DeleteJournalVoucherDetailsByParentId(JVGuid, trn);
                    }
                    if (!a)
                        IsSaved = false;


                    if (IsSaved)
                        trn.Commit();
                    else
                        trn.Rollback();
                }
                catch (Exception)
                {
                    trn.Rollback();

                }
                finally { con.Close(); }


                return IsSaved;
            }
            catch (Exception)
            {

                throw;
            }

        }
        [HttpPost]
        [Route("InsertCashVoucherHeader")]

        public string InsertCashVoucherHeader(DateTime voucherDate, int branchID, int costCenterID,int AccountID, int cashID
            , decimal amount, string note, int voucherNumber
            , string manualNo, int voucherType, string relatedInvoiceGuid, int companyID, int creationUserID,
            [FromBody] string DetailsList)

        {
            try
            {
                DBCashVoucherHeader dbCashVoucherHeader = new DBCashVoucherHeader
                {
                    VoucherDate = voucherDate,
                    BranchID = branchID,
                    CostCenterID = costCenterID,
                    CashID = cashID,
                    AccountID= AccountID,
                    VoucherNo = voucherNumber,
                    Amount = amount,
                    JVGuid = Simulate.Guid(""),
                    Note = Simulate.String(note),
                    ManualNo = Simulate.String(manualNo),
                    VoucherType = voucherType,
                    RelatedInvoiceGuid = Simulate.Guid(relatedInvoiceGuid),
                    CompanyID = companyID,
                    CreationUserID = creationUserID,
                    CreationDate = DateTime.Now,
                };


                List<DBCashVoucherDetails> details = JsonConvert.DeserializeObject<List<DBCashVoucherDetails>>(DetailsList);
                clsCashVoucherHeader clsCashVoucherHeader = new clsCashVoucherHeader();
                clsCashVoucherDetails clsCashVoucherDetails = new clsCashVoucherDetails();
                SqlTransaction trn; clsSQL clsSQL = new clsSQL();
                SqlConnection con = new SqlConnection(clsSQL.conString);
                con.Open();
                trn = con.BeginTransaction(); string A = "";
                try
                {
                    bool IsSaved = true;

                    A = clsCashVoucherHeader.InsertCashVoucherHeader(dbCashVoucherHeader, trn);
                    if (A == "")
                    { IsSaved = false; }
                    else
                    {
                        for (int i = 0; i < details.Count; i++)
                        {
                            string c = clsCashVoucherDetails.InsertCashVoucherDetails(details[i], A, trn);
                            if (c == "")
                                IsSaved = false;
                        }

                    }


                    if (IsSaved)
                        IsSaved = clsCashVoucherHeader.InsertInvoiceJournalVoucher(A, branchID, costCenterID, cashID, amount, Simulate.String(note), voucherDate, details, "", voucherType, companyID, creationUserID, trn);
                    if (IsSaved)
                    { trn.Commit(); return A; }
                    else
                    { trn.Rollback(); return ""; }

                }
                catch (Exception)
                {

                    trn.Rollback();
                    return "";
                }
                finally { con.Close(); }

            }
            catch (Exception ex)
            {

                return "";
            }

        }
        [Route("UpdateCashVoucherHeader")]
        public string UpdateCashVoucherHeader(DateTime voucherDate, int branchID, int costCenterID,int AccountID, int cashID
            , decimal amount, string jVGuid, string note
            , string manualNo, int voucherType, string relatedInvoiceGuid, int companyID,
             int modificationUserID, string guid,
            [FromBody] string detailsList)
        {





            try
            {

                DBCashVoucherHeader dbCashVoucherHeader = new DBCashVoucherHeader
                {
                    VoucherDate = voucherDate,
                    BranchID = branchID,
                    CostCenterID = costCenterID,
                    AccountID= AccountID,
                    CashID = cashID,
                    Amount = amount,
                    JVGuid = Simulate.Guid(jVGuid),

                    Note = Simulate.String(note),

                    ManualNo = Simulate.String(manualNo),
                    VoucherType = voucherType,
                    RelatedInvoiceGuid = Simulate.Guid(relatedInvoiceGuid),
                    CompanyID = companyID,
                    ModificationUserID = modificationUserID,
                    ModificationDate = DateTime.Now,
                    Guid = Simulate.Guid(guid),
                };

                List<DBCashVoucherDetails> details = JsonConvert.DeserializeObject<List<DBCashVoucherDetails>>(detailsList);
                clsCashVoucherHeader clsCashVoucherHeader = new clsCashVoucherHeader();
                clsCashVoucherDetails clsCashVoucherDetails = new clsCashVoucherDetails();
                SqlTransaction trn; clsSQL clsSQL = new clsSQL();
                SqlConnection con = new SqlConnection(clsSQL.conString);
                con.Open();
                trn = con.BeginTransaction();
                string A = "";
                try
                {
                    bool IsSaved = true;

                    A = clsCashVoucherHeader.UpdateCashVoucherHeader(dbCashVoucherHeader, trn);
                    clsCashVoucherDetails.DeleteCashVoucherDetailsByHeaderGuid(guid, trn);
                    for (int i = 0; i < details.Count; i++)
                    {

                        string c = clsCashVoucherDetails.InsertCashVoucherDetails(details[i], guid, trn);
                        if (c == "")
                            IsSaved = false;
                    }
                    if (IsSaved)
                        IsSaved = clsCashVoucherHeader.InsertInvoiceJournalVoucher(guid, branchID, costCenterID, cashID, amount, Simulate.String(note), voucherDate, details, Simulate.String(jVGuid), voucherType, companyID, modificationUserID, trn);
                    if (IsSaved)
                    { trn.Commit(); return A; }
                    else
                    { trn.Rollback(); return ""; }
                }
                catch (Exception)
                {
                    A = "";
                    trn.Rollback();
                }
                finally { con.Close(); }
                return A;
            }
            catch (Exception)
            {

                throw;
            }

        }
        [HttpGet]
        [Route("SelectCashVoucherDetailsByHeaderGuid")]
        public string SelectCashVoucherDetailsByHeaderGuid(string HeaderGuid, int CompanyID)
        {
            try
            {
                clsCashVoucherDetails clsCashVoucherDetails = new clsCashVoucherDetails();
                DataTable dt = clsCashVoucherDetails.SelectCashVoucherDetailsByHeaderGuid(Simulate.String(HeaderGuid), CompanyID);
                if (dt != null)
                {

                    string JSONString = string.Empty;
                    JSONString = JsonConvert.SerializeObject(dt);
                    return JSONString;
                }
                else
                {

                    return "";
                }
            }
            catch (Exception)
            {

                throw;
            }
        }


        [HttpGet]
        [Route("PrintCashVoucherByHeaderGuid")]
        public IActionResult PrintCashVoucherByHeaderGuid(string HeaderGuid, int UserId, int CompanyID)
        {
            try
            {

                FastReport.Utils.Config.WebMode = true;
                clsCashVoucherHeader clsCashVoucherHeader = new clsCashVoucherHeader();
                clsCashVoucherDetails clsCashVoucherDetails = new clsCashVoucherDetails();
           
                DataTable dtHeader = clsCashVoucherHeader.SelectCashVoucherHeaderByGuid(HeaderGuid, DateTime.Now.AddYears(-100), DateTime.Now.AddYears(100), 0, 0, CompanyID);
                DataTable dtDetails = clsCashVoucherDetails.SelectCashVoucherDetailsByHeaderGuid(HeaderGuid, CompanyID);
 
                dsCashVoucher ds = new dsCashVoucher();
                 
                if (dtDetails != null && dtDetails.Rows.Count > 0)
                {
                    for (int i = 0; i < dtDetails.Rows.Count; i++)
                    {
                        ds.Details.Rows.Add();

                        ds.Details.Rows[i]["Guid"] = Simulate.String(dtDetails.Rows[i]["Guid"]);
                        ds.Details.Rows[i]["HeaderGuid"] = Simulate.String(dtDetails.Rows[i]["HeaderGuid"]);
                        ds.Details.Rows[i]["RowIndex"] = Simulate.String(Simulate.Integer32(dtDetails.Rows[i]["RowIndex"]) + 1);
                        ds.Details.Rows[i]["IsUpper"] = Simulate.Bool(dtDetails.Rows[i]["IsUpper"]);
                        ds.Details.Rows[i]["AccountID"] = Simulate.Integer32(dtDetails.Rows[i]["AccountID"]);
                        ds.Details.Rows[i]["SubAccountID"] = Simulate.Integer32(dtDetails.Rows[i]["SubAccountID"]);
                        ds.Details.Rows[i]["BranchID"] = Simulate.Integer32(dtDetails.Rows[i]["BranchID"]);
                        ds.Details.Rows[i]["CostCenterID"] = Simulate.Integer32(dtDetails.Rows[i]["CostCenterID"]);
                        ds.Details.Rows[i]["Debit"] = Simulate.decimal_(dtDetails.Rows[i]["Debit"]);
                        ds.Details.Rows[i]["Credit"] = Simulate.decimal_(dtDetails.Rows[i]["Credit"]);
                        ds.Details.Rows[i]["Total"] = Simulate.decimal_(dtDetails.Rows[i]["Total"]);
                        ds.Details.Rows[i]["Note"] = Simulate.String(dtDetails.Rows[i]["Note"]);
                        ds.Details.Rows[i]["VoucherType"] = Simulate.Integer32(dtDetails.Rows[i]["VoucherType"]);
                        ds.Details.Rows[i]["CompanyID"] = Simulate.Integer32(dtDetails.Rows[i]["CompanyID"]);

                        ds.Details.Rows[i]["BranchAName"] = Simulate.String(dtDetails.Rows[i]["BranchAName"]);
                        ds.Details.Rows[i]["AccountAName"] = Simulate.String(dtDetails.Rows[i]["AccountsAName"]);
                        ds.Details.Rows[i]["CostCenterAName"] = Simulate.String(dtDetails.Rows[i]["CostCenterAName"]);
                        ds.Details.Rows[i]["SubAccountAName"] = Simulate.String(dtDetails.Rows[i]["SubAccountAName"]);


                    }
                }

                if (dtHeader != null && dtHeader.Rows.Count > 0)
                {
                    for (int i = 0; i < dtHeader.Rows.Count; i++)
                    {
                         ds.Header.Rows.Add();

                        ds.Header.Rows[i]["Guid"] = Simulate.String(dtHeader.Rows[i]["Guid"]);
                        ds.Header.Rows[i]["VoucherDate"] = Simulate.StringToDate(dtHeader.Rows[i]["VoucherDate"]).ToString("yyyy-MM-dd");
                        ds.Header.Rows[i]["BranchID"] = Simulate.Integer32(dtHeader.Rows[i]["BranchID"]);
                        ds.Header.Rows[i]["CostCenterID"] = Simulate.Integer32(dtHeader.Rows[i]["CostCenterID"]);
                        ds.Header.Rows[i]["CashID"] = Simulate.Integer32(dtHeader.Rows[i]["CashID"]);
                        ds.Header.Rows[i]["Amount"] = Simulate.Currency_format(dtHeader.Rows[i]["Amount"]);

                        ds.Header.Rows[i]["JVGuid"] = Simulate.String(dtHeader.Rows[i]["JVGuid"]);
                        ds.Header.Rows[i]["Note"] = Simulate.String(dtHeader.Rows[i]["Note"]);
                        ds.Header.Rows[i]["VoucherNo"] = Simulate.Integer32(dtHeader.Rows[i]["VoucherNo"]);
                        ds.Header.Rows[i]["ManualNo"] = Simulate.String(dtHeader.Rows[i]["ManualNo"]);

                        ds.Header.Rows[i]["VoucherType"] = Simulate.Integer32(dtHeader.Rows[i]["VoucherType"]);
                        ds.Header.Rows[i]["RelatedInvoiceGuid"] = Simulate.String(dtHeader.Rows[i]["RelatedInvoiceGuid"]);
                        ds.Header.Rows[i]["BranchAName"] = Simulate.String(dtHeader.Rows[i]["BranchAName"]);
                        ds.Header.Rows[i]["CostCenterAName"] = Simulate.String(dtHeader.Rows[i]["CostCenterAName"]);
                        ds.Header.Rows[i]["CashDrawerAName"] = Simulate.String(dtHeader.Rows[i]["CashDrawerAName"]);
                        ds.Header.Rows[i]["JournalVoucherTypesAname"] = Simulate.String(dtHeader.Rows[i]["JournalVoucherTypesAname"]);



                        ds.Header.Rows[i]["CreationUserID"] = Simulate.Integer32(dtHeader.Rows[i]["CreationUserID"]);
                        ds.Header.Rows[i]["CreationDate"] = Simulate.StringToDate(dtHeader.Rows[i]["CreationDate"]);
                        ds.Header.Rows[i]["ModificationUserID"] = Simulate.Integer32(dtHeader.Rows[i]["ModificationUserID"]);
                        ds.Header.Rows[i]["ModificationDate"] = Simulate.StringToDate(dtHeader.Rows[i]["ModificationDate"]);
                        ds.Header.Rows[i]["CompanyID"] = Simulate.Integer32(dtHeader.Rows[i]["CompanyID"]);
                        


                    }
                }

                string AmountWithOutDecimal = "";
                string AmountDecimal = "";
                string AmountToWord = "";
                AmountToWord = clsConvertNumberToString.NoToTxt(Simulate.Val(dtHeader.Rows[0]["Amount"]));
                AmountWithOutDecimal = Simulate.String(Simulate.Integer32(dtHeader.Rows[0]["Amount"]));
                AmountDecimal = Simulate.String(Simulate.Integer32((Simulate.Val(dtHeader.Rows[0]["Amount"]) - Simulate.Val(dtHeader.Rows[0]["Amount"])) * 1000));

                FastReport.Report report = new FastReport.Report();


               
                string MyPath = getMyPath("rptCashVoucher", CompanyID);
                report.Load(MyPath);
                report.RegisterData(ds);
            
 
                report.SetParameterValue("report.AmountWithOutDecimal", Simulate.String(AmountWithOutDecimal));
                report.SetParameterValue("report.AmountDecimal", Simulate.String(AmountDecimal));
                report.SetParameterValue("report.AmountToWord", Simulate.String(AmountToWord));


 
                 FastreportStanderdParameters(report, UserId, CompanyID);


                report.Prepare();

                return FastreporttoPDF(report);



            }
            catch (Exception ex)
            {

                return Json(ex);
            }

        }
        #endregion
        #region Banks


        [HttpGet]
        [Route("SelectBanks")]
        public string SelectBanks(int ID, int CompanyID)
        {
            try
            {
                clsBanks clsBanks = new clsBanks();
                DataTable dt = clsBanks.SelectBanks(ID, "", "", CompanyID);
                if (dt != null)
                {

                    string JSONString = string.Empty;
                    JSONString = JsonConvert.SerializeObject(dt);
                    return JSONString;
                }
                else
                {

                    return "";
                }
            }
            catch (Exception)
            {

                throw;
            }




        }
        [HttpGet]
        [Route("DeleteBanksByID")]
        public bool DeleteBanksByID(int ID)
        {
            try
            {
                clsJournalVoucherDetails clsJournalVoucherDetails = new clsJournalVoucherDetails();
                DataTable dt = clsJournalVoucherDetails.SelectJournalVoucherDetailsByParentId("", 0, 0,ID, 0, 0);
                if (dt != null && dt.Rows.Count > 0)
                {

                    return false;
                }
                clsBanks clsBanks = new clsBanks();
                bool A = clsBanks.DeleteBanksByID(ID);
                return A;
            }
            catch (Exception)
            {

                throw;
            }

        }
        [HttpGet]
        [Route("InsertBanks")]
        public int InsertBanks(string AName, string EName, string AccountNumber, int CompanyID, int CreationUserId)
        {
            try
            {
                clsBanks clsBanks = new clsBanks();
                int A = clsBanks.InsertBanks(Simulate.String(AName), Simulate.String(EName),Simulate.String(  AccountNumber), CompanyID, CreationUserId);
                return A;
            }
            catch (Exception ex)
            {

                throw;
            }

        }
        [HttpGet]
        [Route("UpdateBanks")]
        public int UpdateBanks(int ID, string AName, string EName, string AccountNumber, int ModificationUserId)
        {
            try
            {
                clsBanks clsBanks = new clsBanks();
                int A = clsBanks.UpdateBanks(ID, Simulate.String(AName), Simulate.String(EName), Simulate.String(AccountNumber), ModificationUserId);
                return A;
            }
            catch (Exception)
            {

                throw;
            }

        }
        #endregion
        #region POSSessionsType


        [HttpGet]
        [Route("SelectPOSSessionsType")]
        public string SelectPOSSessionsType(int ID, int CompanyID)
        {
            try
            {
                clsPosSessionsType clsPosSessionsType = new clsPosSessionsType();
                DataTable dt = clsPosSessionsType.SelectPOSSessionsType(ID, "", "", CompanyID);
                if (dt != null)
                {

                    string JSONString = string.Empty;
                    JSONString = JsonConvert.SerializeObject(dt);
                    return JSONString;
                }
                else
                {

                    return "";
                }
            }
            catch (Exception)
            {

                throw;
            }




        }
        [HttpGet]
        [Route("DeletePOSSessionsTypeByID")]
        public bool DeletePOSSessionsTypeByID(int ID)
        {
            try
            {
                clsPosSessionsType clsPosSessionsType = new clsPosSessionsType();
                bool A = clsPosSessionsType.DeletePOSSessionsTypeByID(ID);
                return A;
            }
            catch (Exception)
            {

                throw;
            }

        }
        [HttpGet]
        [Route("InsertPOSSessionsType")]
        public int InsertPOSSessionsType(string AName, string EName, int CompanyID, int CreationUserId)
        {
            try
            {
                clsPosSessionsType clsPosSessionsType = new clsPosSessionsType();
                int A = clsPosSessionsType.InsertPOSSessionsType(AName, EName, CompanyID, CreationUserId);
                return A;
            }
            catch (Exception ex)
            {

                throw;
            }

        }
        [HttpGet]
        [Route("UpdatePOSSessionsType")]
        public int UpdatePOSSessionsType(int ID, string AName, string EName, int ModificationUserId)
        {
            try
            {
                clsPosSessionsType clsPosSessionsType = new clsPosSessionsType();
                int A = clsPosSessionsType.UpdatePOSSessionsType(ID, AName, EName, ModificationUserId);
                return A;
            }
            catch (Exception)
            {

                throw;
            }

        }
        #endregion
        #region FinancingHeader

        [HttpGet]
        [Route("SelectFinancingHeaderByGuid")]
        public string SelectFinancingHeaderByGuid(string Guid, int BranchID, int CreationUserID,  int CompanyID, DateTime Date1, DateTime Date2,int CurrentUserId,string LoanType)
        {
            try
            {
                clsFinancingHeader clsFinancingHeader = new clsFinancingHeader();
                DataTable dt = clsFinancingHeader.SelectFinancingHeaderByGuid(Simulate.String(Guid), Date1, Date2,  BranchID, CreationUserID, CompanyID, CurrentUserId, LoanType);
                if (dt != null)
                {

                    string JSONString = string.Empty;
                    JSONString = JsonConvert.SerializeObject(dt);
                    return JSONString;
                }
                else
                {

                    return "";
                }
            }
            catch (Exception)
            {

                throw;
            }
        }
        [HttpGet]
        [Route("SelectFinancingReport")]
        public String SelectFinancingReport(int BranchID, int CompanyID,string users, DateTime Date1, DateTime Date2)
        {
            try
            {
             
                clsFinancingHeader clsFinancingHeader = new clsFinancingHeader();
                DataTable dt = clsFinancingHeader.SelectFinancingReport(Date1, Date2,Simulate.String( users), BranchID, CompanyID);





                if (dt != null)
                {
                    string JSONString = string.Empty;
                    JSONString = JsonConvert.SerializeObject(dt);
                    return JSONString;
                }
                else
                {

                    return "";
                } 
                // return Json(PrepareFrxReport(report), JsonRequestBehavior.AllowGet);
            }
            catch (Exception)
            {

                throw;
            }
        }
        [HttpGet]
        [Route("SelectFinancingReportPDF")]
        public IActionResult SelectFinancingReportPDF(int BranchID, int CompanyID, string users,  DateTime Date1, DateTime Date2 )
        {
            try
            {
                clsCompany clsCompany =new clsCompany();
             DataTable   dtCompany = clsCompany.SelectCompany(CompanyID, "", "", "");
                clsBranch clsBranch = new clsBranch();

                DataTable dtBranch = clsBranch.SelectBranch(BranchID, "", "" ,0);

                FastReport.Utils.Config.WebMode = true;
                 clsFinancingHeader clsFinancingHeader = new clsFinancingHeader();
                DataTable dt = clsFinancingHeader.SelectFinancingReport(Date1, Date2,Simulate.String(users), BranchID, CompanyID);

                dsFinancingReport ds = new dsFinancingReport();
                ds.DataTableH.Rows.Add();
                ds.DataTableH.Rows[0]["Date1"] = Date1;
                ds.DataTableH.Rows[0]["Date2"] = Date2;
                if (dtCompany != null && dtCompany.Rows.Count > 0) {

                    ds.DataTableH.Rows[0]["CompanyName"] = dtCompany.Rows[0]["AName"];

                }
                if (dtBranch != null && dtBranch.Rows.Count == 1)
                {

                    ds.DataTableH.Rows[0]["BranchName"] = dtBranch.Rows[0]["AName"];

                }
                else {
                    ds.DataTableH.Rows[0]["BranchName"] = "All";

                }
                if (dt != null && dt.Rows.Count > 0)
                {
                    for (int i = 0; i < dt.Rows.Count; i++)
                    {
                        ds.DataTableD.Rows.Add();

                        ds.DataTableD.Rows[i]["Index"] = i+1;
                        ds.DataTableD.Rows[i]["Customer"] = dt.Rows[i]["businessPartnerAName"];

                        ds.DataTableD.Rows[i]["Total"] = dt.Rows[i]["FinancingAmount"];
                        ds.DataTableD.Rows[i]["Price"] = dt.Rows[i]["FinancingAmount"];
                        ds.DataTableD.Rows[i]["QTY"] = 1;
                        ds.DataTableD.Rows[i]["Descrption"] = Simulate.String(dt.Rows[i]["Description"]);
                      
                    }
                }





                FastReport.Report report = new FastReport.Report();
                 report.RegisterData(ds);


                 
                string MyPath = getMyPath("rptFinancingReport", CompanyID);
                report.Load(MyPath);
                //if (BranchID == 0)
                //{
                //    report.SetParameterValue("report.Branch", "All Branches");

                //}
                //else
                //{
                //    clsBranch clsBranch = new clsBranch();
                //    DataTable dtBranch = clsBranch.SelectBranch(BranchID, "", "", 0);
                //    if (dtBranch != null && dtBranch.Rows.Count > 0)
                //    {
                //        report.SetParameterValue("report.Branch", Simulate.String(dtBranch.Rows[0]["AName"]));

                //    }
                //}
                //if (CostCenterID == 0)
                //{
                //    report.SetParameterValue("report.CostCenter", "All Cost Center");

                //}
                //else
                //{
                //    clsCostCenter clsCostCenter = new clsCostCenter();
                //    DataTable dtCostCenter = clsCostCenter.SelectCostCentersByID(CostCenterID, "", "", 0);
                //    if (dtCostCenter != null && dtCostCenter.Rows.Count > 0)
                //    {
                //        report.SetParameterValue("report.CostCenter", Simulate.String(dtCostCenter.Rows[0]["AName"]));

                //    }
                //}
                //report.SetParameterValue("report.FromDate", Date1.ToString("yyyy-MM-dd"));
                //report.SetParameterValue("report.ToDate", Date2.ToString("yyyy-MM-dd"));


                //report.Export(FastReport.Export.Html.);
                //FastreportStanderdParameters(report, UserId, CompanyID);
                ////    report.Prepare();

                report.Prepare();

               return FastreporttoPDF(report);
                // return Json(PrepareFrxReport(report), JsonRequestBehavior.AllowGet);
            }
            catch (Exception)
            {

                throw;
            }
        }
        [HttpGet]
        [Route("SelectFinancingReportXLS")]
        public ActionResult SelectFinancingReportXLS(int BranchID, int CompanyID,string users, DateTime Date1, DateTime Date2)
        {
            try
            {
                clsCompany clsCompany = new clsCompany();
                DataTable dtCompany = clsCompany.SelectCompany(CompanyID, "", "", "");
                clsBranch clsBranch = new clsBranch();

                DataTable dtBranch = clsBranch.SelectBranch(BranchID, "", "", 0);

                FastReport.Utils.Config.WebMode = true;
                clsFinancingHeader clsFinancingHeader = new clsFinancingHeader();
                DataTable dt = clsFinancingHeader.SelectFinancingReport(Date1, Date2, Simulate.String(users), BranchID, CompanyID);

                dsFinancingReport ds = new dsFinancingReport();
                ds.DataTableH.Rows.Add();
                ds.DataTableH.Rows[0]["Date1"] = Date1;
                ds.DataTableH.Rows[0]["Date2"] = Date2;
                if (dtCompany != null && dtCompany.Rows.Count > 0)
                {

                    ds.DataTableH.Rows[0]["CompanyName"] = dtCompany.Rows[0]["AName"];

                }
                if (dtBranch != null && dtBranch.Rows.Count == 1)
                {

                    ds.DataTableH.Rows[0]["BranchName"] = dtBranch.Rows[0]["AName"];

                }
                else
                {
                    ds.DataTableH.Rows[0]["BranchName"] = "All";

                }
                if (dt != null && dt.Rows.Count > 0)
                {
                    for (int i = 0; i < dt.Rows.Count; i++)
                    {
                        ds.DataTableD.Rows.Add();
                         
                        ds.DataTableD.Rows[i]["Index"] = i + 1;
                        ds.DataTableD.Rows[i]["Customer"] = dt.Rows[i]["businessPartnerAName"];

                        ds.DataTableD.Rows[i]["Total"] = dt.Rows[i]["FinancingAmount"];
                        ds.DataTableD.Rows[i]["Price"] = dt.Rows[i]["FinancingAmount"];
                        ds.DataTableD.Rows[i]["QTY"] = 1;
                        ds.DataTableD.Rows[i]["Descrption"] = Simulate.String(dt.Rows[i]["Description"]);

                    }
                }





                FastReport.Web.WebReport report = new FastReport.Web.WebReport();
                report.Report.RegisterData(ds);


               
                string MyPath = getMyPath("rptFinancingReport", CompanyID);
                report.Report.Load(MyPath);
                

                report.Report.Prepare();

                return Fastreporttoxls(ds.DataTableD);
             }
            catch (Exception)
            {

                throw;
            }
        }
        [HttpGet]
        [Route("DeleteFinancingHeaderByGuid")]
        public bool DeleteFinancingHeaderByGuid(string Guid)
        {
            try
            {
                clsFinancingDetails clsFinancingDetails = new clsFinancingDetails();
                clsFinancingHeader clsFinancingHeader = new clsFinancingHeader();
                clsJournalVoucherHeader clsJournalVoucherHeader = new clsJournalVoucherHeader();
                clsJournalVoucherDetails clsJournalVoucherDetails = new clsJournalVoucherDetails();
                SqlTransaction trn; clsSQL clsSQL = new clsSQL();
                SqlConnection con = new SqlConnection(clsSQL.conString);
                con.Open();
                trn = con.BeginTransaction(); int A = 0;
                bool IsSaved = true;
                try
                {
                    DataTable dt = clsFinancingHeader.SelectFinancingHeaderByGuid(Guid, Simulate.StringToDate("1900-01-01"), Simulate.StringToDate("2300-01-01"), 0, 0,  0, 0, "-1", trn);
                    IsSaved = clsFinancingHeader.DeleteFinancingHeaderByGuid(Guid, trn);
                    bool a = clsFinancingDetails.DeleteFinancingDetailsByHeaderGuid(Guid, trn);
                    if (dt != null && dt.Rows.Count > 0)
                    {
                        string JVGuid = Simulate.String(dt.Rows[0]["JVGuid"]);
                        bool aa = clsJournalVoucherHeader.DeleteJournalVoucherHeaderByID(JVGuid, trn);
                        bool aaa = clsJournalVoucherDetails.DeleteJournalVoucherDetailsByParentId(JVGuid, trn);
                    }
                    if (!a)
                        IsSaved = false;


                    if (IsSaved)
                        trn.Commit();
                    else
                        trn.Rollback();
                }
                catch (Exception)
                {
                    trn.Rollback();

                }
                finally { con.Close(); }


                return IsSaved;
            }
            catch (Exception)
            {

                throw;
            }

        }
        [HttpPost]
        [Route("InsertFinancingHeader")]

        public string InsertFinancingHeader(DateTime voucherDate, int branchID, int voucherNumber, int businessPartnerID
            , string note, decimal totalAmount, decimal downPayment, decimal netAmount
            ,  int grantor,int loanType,  int creationUserID, int companyID, decimal IntrestRate,
            bool isAmountReturned,
            int MonthsCount,int PaymentAccountID,int PaymentSubAccountID, int VendorID,
            [FromBody] string DetailsList)

        {
            try
            {

                
                
                DBFinancingHeader dbFinancingHeader = new DBFinancingHeader
                {
                    VoucherDate = voucherDate,
                    BranchID = branchID,
                    VoucherNumber = voucherNumber,
                    BusinessPartnerID= businessPartnerID,
                    Note = Simulate.String(note),
                    TotalAmount = totalAmount,
                    DownPayment = downPayment,
                    NetAmount = netAmount,
                    Grantor= grantor,
                    LoanType = loanType,
                    CompanyID = companyID,
                    CreationUserID = creationUserID,
                    CreationDate = DateTime.Now, 
                    IntrestRate = IntrestRate,
                    isAmountReturned = isAmountReturned,
                    MonthsCount = MonthsCount,
                     PaymentAccountID = PaymentAccountID,
                    PaymentSubAccountID = PaymentSubAccountID,
                    VendorID= VendorID,
                };


                clsFinancingHeader clsFinancingHeader = new clsFinancingHeader();
                clsFinancingDetails clsFinancingDetails = new clsFinancingDetails();
                SqlTransaction trn; clsSQL clsSQL = new clsSQL();
                SqlConnection con = new SqlConnection(clsSQL.conString);
                con.Open();
                trn = con.BeginTransaction(); 
                
                string A = "";
                try
                {
                    DataTable dtmax = clsFinancingHeader.SelectMaxFinancingHeader(0, dbFinancingHeader.CompanyID, trn);
                    if (dtmax!= null && dtmax.Rows.Count>0) {
                        dbFinancingHeader.VoucherNumber = Simulate.Integer32(dtmax.Rows[0][0]);


                    }
                    bool IsSaved = true;

                    A = clsFinancingHeader.InsertFinancingHeader(dbFinancingHeader, trn);
                    if (A == "")
                    { IsSaved = false; }
                    else
                    {   
                        clsLoanTypes clsLoanTypes= new clsLoanTypes();
                        DataTable DTLoanTypes = clsLoanTypes.SelectLoanTypes(loanType,"0,1,2,3","","","",companyID);
                        if (DTLoanTypes != null && DTLoanTypes.Rows.Count > 0 && Simulate.Integer32(DTLoanTypes.Rows[0]["MainTypeID"]) == 1)
                        {// If Sales 
                            List<DBFinancingDetails> details = JsonConvert.DeserializeObject<List<DBFinancingDetails>>(DetailsList);

                            for (int i = 0; i < details.Count; i++)
                            {
                                string c = clsFinancingDetails.InsertFinancingDetails(dbFinancingHeader, details[i], A, trn);
                                if (c == "")
                                    IsSaved = false;
                            }
                        }
                        else {
                            clsJournalVoucherHeader clsJournalVoucherHeader= new clsJournalVoucherHeader();
                            clsJournalVoucherDetails clsJournalVoucherDetails= new clsJournalVoucherDetails();
                            List<tbl_JournalVoucherDetails> details = JsonConvert.DeserializeObject<List<tbl_JournalVoucherDetails>>(DetailsList);
                            DataTable dtMaxJV = clsJournalVoucherHeader.SelectMaxJVNo("", (int)clsEnum.VoucherType.Finance, companyID, trn);
                            int maxJv = 1;
                            if (dtMaxJV != null && dtMaxJV.Rows.Count > 0)
                            {
                                maxJv = Simulate.Integer32(dtMaxJV.Rows[0][0]) + 1;
                            }
                        string    JVGuid = clsJournalVoucherHeader.InsertJournalVoucherHeader(branchID, 0, Simulate.String(note),Simulate.String( maxJv), (int)clsEnum.VoucherType.Finance, companyID, voucherDate, creationUserID, trn);
                            if (JVGuid == "") IsSaved = false;
                            if (DTLoanTypes!= null && DTLoanTypes.Rows.Count>0 && Simulate.Integer32( DTLoanTypes.Rows[0]["MainTypeID"])==2) {
                                for (int i = 0; i < details.Count; i++)
                                {
                                    int AccountNumber = 0;
                                    if (details[i].AccountID == 0)
                                    { AccountNumber = Simulate.Integer32(DTLoanTypes.Rows[0]["ReceivableAccountID"]); } 
                                    else {
                                        AccountNumber = details[i].AccountID;
                                    }
                                    string c1 = clsJournalVoucherDetails.InsertJournalVoucherDetails(JVGuid, i, AccountNumber, details[i].SubAccountID, details[i].Debit, details[i].Credit
                                          , details[i].Total, branchID, details[i].CostCenterID, details[i].DueDate, details[i].Note, companyID
                                          , creationUserID, trn);
                                    if (Simulate.Integer32(AccountNumber) == 0)
                                    {
                                        IsSaved = false;
                                    }
                                    if (c1 == "")
                                        IsSaved = false;
                                }


                            }
                            else {
                                if (Simulate.Integer32(DTLoanTypes.Rows[0]["ReceivableAccountID"]) == 0) {
                                    IsSaved = false;
                                }

                                string c1 = clsJournalVoucherDetails.InsertJournalVoucherDetails(JVGuid, 1, Simulate.Integer32(DTLoanTypes.Rows[0]["ReceivableAccountID"]), businessPartnerID, netAmount, 0
                                           ,  netAmount, branchID,0, voucherDate,Simulate.String( note), companyID
                                           , creationUserID, trn);
                                if (c1 == "")
                                    IsSaved = false;


                                string insertCredit = clsJournalVoucherDetails.InsertJournalVoucherDetails(JVGuid, details.Count + 1,PaymentAccountID, PaymentSubAccountID, 0, totalAmount
                                        , -1 * totalAmount, branchID, 0, voucherDate, Simulate.String(note), companyID
                                        , creationUserID, trn);
                                if (Simulate.Integer32(DTLoanTypes.Rows[0]["PaymentAccountID"]) == 0)
                                {
                                    //IsSaved = false;
                                }
                                if (insertCredit == "")
                                    IsSaved = false;

                            }


                            if (netAmount != totalAmount)
                            {
                                string insertProfit = clsJournalVoucherDetails.InsertJournalVoucherDetails(JVGuid, details.Count+2, Simulate.Integer32(DTLoanTypes.Rows[0]["ProfitAccount"]), businessPartnerID, 0, netAmount - totalAmount
                            ,-1*( netAmount - totalAmount), branchID, 0, voucherDate, Simulate.String(note), companyID
                            , creationUserID, trn);
                                if (insertProfit == "")
                                    IsSaved = false;

                                if (Simulate.Integer32(DTLoanTypes.Rows[0]["ProfitAccount"]) == 0)
                                {
                                    IsSaved = false;
                                }
                            }
                            clsFinancingHeader.UpdateFinancingHeaderJVGuid(A, JVGuid, trn);     
                                
                            if (!clsJournalVoucherHeader.CheckJVMatch(JVGuid, trn))
                            {
                                IsSaved = false;
                                JVGuid = "";
                            }
                        }
                    }


                    //if (IsSaved)
                    //    IsSaved = clsCashVoucherHeader.InsertInvoiceJournalVoucher(A, branchID, costCenterID, cashID, amount, Simulate.String(note), voucherDate, details, "", voucherType, companyID, creationUserID, trn);
                    if (IsSaved)
                    { trn.Commit(); return A; }
                    else
                    { trn.Rollback(); return ""; }

                }
                catch (Exception ex)
                {

                    trn.Rollback();
                    return "";
                }
                finally { con.Close(); }

            }
            catch (Exception ex)
            {

                return "";
            }

        }
        [Route("UpdateFinancingHeader")]
        public string UpdateFinancingHeader(
            DateTime voucherDate,
            int branchID, 
            int voucherNumber, 
            int businessPartnerID,
            string note,
            decimal totalAmount,
            decimal downPayment,
            decimal netAmount,
            int grantor,
            int loanType, 
            int modificationUserID,
            int companyID, 
            string guid,
            string jVGuid,
            decimal IntrestRate,
            bool isAmountReturned,
            int MonthsCount, int PaymentAccountID, int PaymentSubAccountID,int VendorID,
            [FromBody] string DetailsList
            
           )
        {





            try
            {
                 
                DBFinancingHeader dbFinancingHeader = new DBFinancingHeader
                {
                    VoucherDate = voucherDate,
                    BranchID = branchID,
                    VoucherNumber = voucherNumber,
                    BusinessPartnerID = businessPartnerID,
                    Note = Simulate.String(note),
                    TotalAmount = totalAmount,
                    DownPayment = downPayment,
                    NetAmount = netAmount,
                    Grantor = grantor,
                    LoanType= loanType,
                    Guid =Simulate.Guid( guid),
                    CompanyID = companyID,
                    ModificationUserID = modificationUserID,
                    CreationDate = DateTime.Now,
                    JVGuid = Simulate.Guid(jVGuid),
                    IntrestRate= IntrestRate,
                    isAmountReturned= isAmountReturned,
                    MonthsCount= MonthsCount,
                    PaymentAccountID = PaymentAccountID,
                    PaymentSubAccountID = PaymentSubAccountID,
                    VendorID= VendorID,
                };


                clsFinancingHeader clsFinancingHeader = new clsFinancingHeader();
                clsFinancingDetails clsFinancingDetails = new clsFinancingDetails();
                SqlTransaction trn; clsSQL clsSQL = new clsSQL();
                SqlConnection con = new SqlConnection(clsSQL.conString);
                con.Open();
                trn = con.BeginTransaction();
                string A = "";
                try
                {
                    bool IsSaved = true;

                    A = clsFinancingHeader.UpdateFinancingHeader(dbFinancingHeader, trn);
                    clsFinancingDetails.DeleteFinancingDetailsByHeaderGuid(guid, trn);

                    clsLoanTypes clsLoanTypes = new clsLoanTypes();
                    DataTable DTLoanTypes = clsLoanTypes.SelectLoanTypes(loanType, "0,1,2,3", "", "", "", companyID);
                    if (DTLoanTypes != null && DTLoanTypes.Rows.Count > 0 && Simulate.Integer32(DTLoanTypes.Rows[0]["MainTypeID"]) == 1)
                    {
                        List<DBFinancingDetails> details = JsonConvert.DeserializeObject<List<DBFinancingDetails>>(DetailsList);
                        for (int i = 0; i < details.Count; i++)
                        {

                            string c = clsFinancingDetails.InsertFinancingDetails(dbFinancingHeader, details[i], guid, trn);
                            if (c == "")
                                IsSaved = false;
                        }

                    }
                    else {
                        clsJournalVoucherHeader clsJournalVoucherHeader = new clsJournalVoucherHeader();
                        clsJournalVoucherDetails clsJournalVoucherDetails = new clsJournalVoucherDetails();
                         clsJournalVoucherDetails.DeleteJournalVoucherDetailsByParentId(dbFinancingHeader.JVGuid.ToString(), trn);



                        List<tbl_JournalVoucherDetails> details = JsonConvert.DeserializeObject<List<tbl_JournalVoucherDetails>>(DetailsList);
                        if (DTLoanTypes != null && DTLoanTypes.Rows.Count > 0 && Simulate.Integer32(DTLoanTypes.Rows[0]["MainTypeID"]) == 2)
                        {
                       
                            for (int i = 0; i < details.Count; i++)
                            {
                                int AccountNumber = 0;
                                if (details[i].AccountID == 0)
                                { AccountNumber = Simulate.Integer32(DTLoanTypes.Rows[0]["ReceivableAccountID"]); }
                                else
                                {
                                    AccountNumber = details[i].AccountID;
                                }
                                string c1 = clsJournalVoucherDetails.InsertJournalVoucherDetails(dbFinancingHeader.JVGuid.ToString(), i, Simulate.Integer32(AccountNumber), details[i].SubAccountID, details[i].Debit, details[i].Credit
                                  , details[i].Total, branchID, details[i].CostCenterID, details[i].DueDate, details[i].Note, companyID
                                  , modificationUserID, trn);
                                if (Simulate.Integer32(DTLoanTypes.Rows[0]["ReceivableAccountID"]) == 0)
                                {
                                    IsSaved = false;
                                }
                                if (c1 == "")
                                IsSaved = false;
                        }
                        }
                        else
                        {

                            string c1 = clsJournalVoucherDetails.InsertJournalVoucherDetails(dbFinancingHeader.JVGuid.ToString(), 1, Simulate.Integer32(DTLoanTypes.Rows[0]["ReceivableAccountID"]), businessPartnerID, netAmount ,0
                                       , netAmount, branchID, 0, voucherDate, Simulate.String(note), companyID
                                       , modificationUserID, trn);
                            if (Simulate.Integer32(DTLoanTypes.Rows[0]["ReceivableAccountID"]) == 0)
                            {
                                IsSaved = false;
                            }
                            if (c1 == "")
                                IsSaved = false;
                            string insertCredit = clsJournalVoucherDetails.InsertJournalVoucherDetails(dbFinancingHeader.JVGuid.ToString(), details.Count + 1, PaymentAccountID, PaymentSubAccountID, 0, totalAmount
                                     , -1 * totalAmount, branchID, 0, voucherDate, Simulate.String(note), companyID
                                     , modificationUserID, trn);
                            if (Simulate.Integer32(DTLoanTypes.Rows[0]["PaymentAccountID"]) == 0)
                            {
                                //IsSaved = false;
                            }
                            if (insertCredit == "")
                                IsSaved = false;

                        }
                        //string insertCredit = clsJournalVoucherDetails.InsertJournalVoucherDetails(dbFinancingHeader.JVGuid.ToString(), details.Count, Simulate.Integer32(DTLoanTypes.Rows[0]["PaymentAccountID"]), businessPartnerID, 0, totalAmount
                        //                , -1*totalAmount, branchID, 0, voucherDate, Simulate.String(note), companyID
                        //                , modificationUserID, trn);
                        //if (Simulate.Integer32(DTLoanTypes.Rows[0]["PaymentAccountID"]) == 0)
                        //{
                        //    IsSaved = false;
                        //}
                        //if (insertCredit == "")
                        //    IsSaved = false;
                        if (netAmount!= totalAmount) { 
                        string insertProfit = clsJournalVoucherDetails.InsertJournalVoucherDetails(dbFinancingHeader.JVGuid.ToString(), details.Count, Simulate.Integer32(DTLoanTypes.Rows[0]["ProfitAccount"]), businessPartnerID,  0, netAmount - totalAmount
                        ,-1*( netAmount - totalAmount), branchID, 0, voucherDate, Simulate.String(note), companyID
                        , modificationUserID, trn);

                            if (Simulate.Integer32(DTLoanTypes.Rows[0]["ProfitAccount"]) == 0)
                            {
                                IsSaved = false;
                            }
                            if (insertProfit == "")
                            IsSaved = false;
                        }



                        if (!clsJournalVoucherHeader.CheckJVMatch(dbFinancingHeader.JVGuid.ToString(), trn))
                        {
                            IsSaved = false;
                           // dbFinancingHeader.JVGuid = "";
                        }

                    }










                    //if (IsSaved)
                    //    IsSaved = clsCashVoucherHeader.InsertInvoiceJournalVoucher(guid, branchID, costCenterID, cashID, amount, Simulate.String(note), voucherDate, details, Simulate.String(jVGuid), voucherType, companyID, modificationUserID, trn);
                    if (IsSaved)
                    { trn.Commit(); return A; }
                    else
                    { trn.Rollback(); return ""; }
                }
                catch (Exception)
                {
                    A = "";
                    trn.Rollback();
                }
                finally { con.Close(); }
                return A;
            }
            catch (Exception)
            {

                throw;
            }

        }
        [HttpGet]
        [Route("SelectFinancingDetailsByHeaderGuid")]
        public string SelectFinancingDetailsByHeaderGuid(string HeaderGuid,int CreationUserID, int CompanyID)
        {
            try
            {
                clsFinancingDetails clsFinancingDetails = new clsFinancingDetails();
                DataTable dt = clsFinancingDetails.SelectFinancingDetailsByHeaderGuid(Simulate.String(HeaderGuid), CreationUserID, CompanyID);
                if (dt != null)
                {

                    string JSONString = string.Empty;
                    JSONString = JsonConvert.SerializeObject(dt);
                    return JSONString;
                }
                else
                {

                    return "";
                }
            }
            catch (Exception)
            {

                throw;
            }
        }



        [HttpGet]
        [Route("PrintFinancing")]
        public IActionResult PrintFinancing(string guid, int UserId, int CompanyID)
        {
            try
            {
                decimal AmountWithProfit = 0;
                 
                FastReport.Utils.Config.WebMode = true;
                clsFinancingHeader clsFinancingHeader = new clsFinancingHeader();
                clsFinancingDetails clsFinancingDetails = new clsFinancingDetails();

                DataTable dtHeader = clsFinancingHeader.SelectFinancingHeaderByGuid(guid, DateTime.Now.AddYears(-100), DateTime.Now.AddYears(100), 0,0,  CompanyID,0,"-1");
                DataTable dtDetails = clsFinancingDetails.SelectFinancingDetailsByHeaderGuid(guid ,0, CompanyID);

                clsLoanTypes clsLoanTypes = new clsLoanTypes();
                DataTable dtLoanType = clsLoanTypes.SelectLoanTypes(Simulate.Integer32(dtHeader.Rows[0]["LoanType"]), "-1,0,1,2,3", "","","",0);
                decimal TotalDue = 0;
                dsFinancing ds = new dsFinancing();
                dsBusinessPartner dsBusinessPartner = new dsBusinessPartner();
                if (dtDetails != null && dtDetails.Rows.Count > 0)
                {
                    for (int i = 0; i < dtDetails.Rows.Count; i++)
                    {
                        ds.Details.Rows.Add();

                        ds.Details.Rows[i]["Guid"] = Simulate.String(dtDetails.Rows[i]["Guid"]);
                        ds.Details.Rows[i]["HeaderGuid"] = Simulate.String(dtDetails.Rows[i]["HeaderGuid"]);
                        ds.Details.Rows[i]["RowIndex"] = Simulate.String(Simulate.Integer32(dtDetails.Rows[i]["RowIndex"]) + 1);
                        ds.Details.Rows[i]["Description"] = Simulate.String(dtDetails.Rows[i]["Description"]);
                        ds.Details.Rows[i]["TotalAmount"] = Simulate.Currency_format(dtDetails.Rows[i]["TotalAmount"]);
                        ds.Details.Rows[i]["DownPayment"] = Simulate.Currency_format(dtDetails.Rows[i]["DownPayment"]);
                        ds.Details.Rows[i]["FinancingAmount"] = Simulate.Currency_format(dtDetails.Rows[i]["FinancingAmount"]);
                        ds.Details.Rows[i]["PeriodInMonths"] = Simulate.Integer32(dtDetails.Rows[i]["PeriodInMonths"]);
                        ds.Details.Rows[i]["InterestRate"] = Simulate.decimal_(dtDetails.Rows[i]["InterestRate"]);
                        ds.Details.Rows[i]["InterestAmount"] = Simulate.Currency_format(dtDetails.Rows[i]["InterestAmount"]);
                        ds.Details.Rows[i]["TotalAmountWithInterest"] = Simulate.Currency_format(dtDetails.Rows[i]["TotalAmountWithInterest"]);
                        ds.Details.Rows[i]["FirstInstallmentDate"] = Simulate.StringToDate(dtDetails.Rows[i]["FirstInstallmentDate"]).ToString("yyyy-MM-dd");
                        ds.Details.Rows[i]["InstallmentAmount"] = Simulate.Currency_format(dtDetails.Rows[i]["InstallmentAmount"]);
                        ds.Details.Rows[i]["JVGuid"] = Simulate.String(dtDetails.Rows[i]["JVGuid"]);
                        ds.Details.Rows[i]["CreationUserID"] = Simulate.Integer32(dtDetails.Rows[i]["CreationUserID"]);
                        ds.Details.Rows[i]["CreationDate"] = Simulate.StringToDate(dtDetails.Rows[i]["CreationDate"]);
                        ds.Details.Rows[i]["ModificationUserID"] = Simulate.Integer32(dtDetails.Rows[i]["ModificationUserID"]);
                        ds.Details.Rows[i]["ModificationDate"] = Simulate.StringToDate(dtDetails.Rows[i]["ModificationDate"]);
                        ds.Details.Rows[i]["CompanyID"] = Simulate.Integer32(dtDetails.Rows[i]["CompanyID"]);

                        ds.Details.Rows[i]["serialNumber"] = Simulate.String(dtDetails.Rows[i]["SerialNumber"]);
                        AmountWithProfit = AmountWithProfit + Simulate.decimal_(dtDetails.Rows[i]["TotalAmountWithInterest"]);

                    }
                }
                string AmountWithOutDecimal = "";
                string AmountDecimal = "";
                string AmountToWord = "";
                AmountToWord = clsConvertNumberToString.NoToTxt(Simulate.Val(AmountWithProfit));
                AmountWithOutDecimal = Simulate.String(Simulate.Integer32(AmountWithProfit));
                AmountDecimal = Simulate.String(Simulate.Integer32((AmountWithProfit - AmountWithProfit) * 1000));
  
                if (dtHeader != null && dtHeader.Rows.Count > 0)
                {
                    for (int i = 0; i < dtHeader.Rows.Count; i++)
                    {
                        dsBusinessPartner = FillDsBusnessPartner(Simulate.Integer32(dtHeader.Rows[i]["BusinessPartnerID"]), Simulate.Integer32(dtHeader.Rows[i]["Grantor"]));
                        ds.Header.Rows.Add();

                        ds.Header.Rows[i]["Guid"] = Simulate.String(dtHeader.Rows[i]["Guid"]);
                        ds.Header.Rows[i]["VoucherDate"] = Simulate.StringToDate(dtHeader.Rows[i]["VoucherDate"]).ToString("yyyy-MM-dd");
                        ds.Header.Rows[i]["BranchID"] = Simulate.Integer32(dtHeader.Rows[i]["BranchID"]);
                        ds.Header.Rows[i]["VoucherNumber"] = Simulate.String(dtHeader.Rows[i]["VoucherNumber"]);
                        ds.Header.Rows[i]["BusinessPartnerID"] = Simulate.Integer32(dtHeader.Rows[i]["BusinessPartnerID"]);
                        ds.Header.Rows[i]["Note"] = Simulate.String(dtHeader.Rows[i]["Note"]);
                        ds.Header.Rows[i]["TotalAmount"] = Simulate.Currency_format(dtHeader.Rows[i]["TotalAmount"]);
                        ds.Header.Rows[i]["DownPayment"] = Simulate.Currency_format(dtHeader.Rows[i]["DownPayment"]);
                        ds.Header.Rows[i]["NetAmount"] = Simulate.Currency_format(dtHeader.Rows[i]["NetAmount"]);

                        ds.Header.Rows[i]["Grantor"] = Simulate.Integer32(dtHeader.Rows[i]["Grantor"]);

                        ds.Header.Rows[i]["CreationUserID"] = Simulate.Integer32(dtHeader.Rows[i]["CreationUserID"]);
                        ds.Header.Rows[i]["CreationDate"] = Simulate.StringToDate(dtHeader.Rows[i]["CreationDate"]);
                        ds.Header.Rows[i]["ModificationUserID"] = Simulate.Integer32(dtHeader.Rows[i]["ModificationUserID"]);
                        ds.Header.Rows[i]["ModificationDate"] = Simulate.StringToDate(dtHeader.Rows[i]["ModificationDate"]);
                        ds.Header.Rows[i]["CompanyID"] = Simulate.Integer32(dtHeader.Rows[i]["CompanyID"]);
                        ds.Header.Rows[i]["BranchName"] = Simulate.String(dtHeader.Rows[i]["BranchName"]);
                        ds.Header.Rows[i]["BusinessPartnerName"] = Simulate.String(dtHeader.Rows[i]["BusinessPartnerName"]);

                        ds.Header.Rows[i]["GrantorName"] = Simulate.String(dtHeader.Rows[i]["GrantorName"]);

                        ds.Header.Rows[i]["CreationUserName"] = Simulate.String(dtHeader.Rows[i]["CreationUserName"]);

                        ds.Header.Rows[i]["LoanType"] = Simulate.Integer32(dtHeader.Rows[i]["LoanType"]);
                        ds.Header.Rows[i]["JVGuid"] = Simulate.String(dtHeader.Rows[i]["JVGuid"]);
                        ds.Header.Rows[i]["IntrestRate"] = Simulate.decimal_(dtHeader.Rows[i]["IntrestRate"]);
                        ds.Header.Rows[i]["IsAmountReturned"] = Simulate.String(dtHeader.Rows[i]["IsAmountReturned"]);
                        ds.Header.Rows[i]["MonthsCount"] = Simulate.Integer32(dtHeader.Rows[i]["MonthsCount"]);
                        ds.Header.Rows[i]["PaymentAccountID"] = Simulate.Integer32(dtHeader.Rows[i]["PaymentAccountID"]);
                        ds.Header.Rows[i]["PaymentSubAccountID"] = Simulate.Integer32(dtHeader.Rows[i]["PaymentSubAccountID"]);
                        ds.Header.Rows[i]["LoanTypeAName"] = Simulate.String(dtHeader.Rows[i]["LoanTypeAName"]);
                        ds.Header.Rows[i]["PaymentAccountIDAName"] = Simulate.String(dtHeader.Rows[i]["PaymentAccountIDAName"]);
                        ds.Header.Rows[i]["PaymentSubAccountIDAName"] = Simulate.String(dtHeader.Rows[i]["PaymentSubAccountIDAName"]);




                        clsReports clsReports = new clsReports();

                        cls_AccountSetting cls_AccountSetting = new cls_AccountSetting();
                        DataTable dtAccountSetting = cls_AccountSetting.SelectAccountSetting(0, 0, Simulate.Integer32(dtHeader.Rows[i]["CompanyID"]));
                        clsInvoiceHeader clsInvoiceHeader = new clsInvoiceHeader();
                        int CustomerAccount = clsInvoiceHeader.GetValueFromDT(dtAccountSetting, "AccountRefID", Simulate.String((int)clsEnum.AccountMainSetting.CustomerAccount), 2);


                        DataTable dtStatment = clsReports.SelectAccountStatement(Simulate.StringToDate(dtHeader.Rows[i]["VoucherDate"]), Simulate.StringToDate(dtHeader.Rows[i]["VoucherDate"])
                            , 0, 0, CustomerAccount, Simulate.Integer32(dtHeader.Rows[i]["BusinessPartnerID"]), 0, true);
                        if (dtStatment != null && dtStatment.Rows.Count > 0)
                        {

                            TotalDue = Simulate.decimal_(dtStatment.Rows[dtStatment.Rows.Count - 1]["nettotal"]);
                        }



                    }
                }


                if (Simulate.Integer32(dtHeader.Rows[0]["LoanType"]) == 1) {

              
                    FastReport.Report report = new FastReport.Report();
                   
                    string MyPath = getMyPath("rptFinancing", CompanyID);
                    report.Load(MyPath);
                    report.RegisterData(ds);
                    report.RegisterData(dsBusinessPartner);
               
                    report.SetParameterValue("report.TotalDueToWord", Simulate.String(clsConvertNumberToString.NoToTxt(Simulate.Val(Math.Abs(TotalDue)))));
                    report.SetParameterValue("report.AmountWithOutDecimal", Simulate.String(AmountWithOutDecimal));
                    report.SetParameterValue("report.AmountDecimal", Simulate.String(AmountDecimal));
                    report.SetParameterValue("report.AmountToWord", Simulate.String(AmountToWord));
                    report.SetParameterValue("report.DueDate", (Simulate.StringToDate(dtHeader.Rows[0]["VoucherDate"]).AddMonths(4)).ToString("yyyy-MM-dd"));

                    report.SetParameterValue("report.TotalDue", Simulate.Currency_format(TotalDue));
                    FastreportStanderdParameters(report, UserId, CompanyID);


                    report.Prepare();

                    return FastreporttoPDF(report);

                } else if (Simulate.Integer32(dtLoanType.Rows[0]["MainTypeID"])==2) {//Loan



                    clsJournalVoucherDetails clsJVDetails = new clsJournalVoucherDetails();
                    dsJVDetails dsJVDetails = clsJVDetails.SelectJournalVoucherDetailsByParentIdForPrint(Simulate.String(dtHeader.Rows[0]["JVGuid"]), 0, 0);



                    FastReport.Report report = new FastReport.Report();


                     
                    string MyPath = getMyPath("rptCashLoan", CompanyID);
                    for (int i = 0; i < dsJVDetails.Tables[0].Rows.Count; i++)
                    {
                        dsJVDetails.Tables[0].Rows[i]["Rowindex"] = (i + 1);
                        if (Simulate.Val( dsJVDetails.Tables[0].Rows[i]["Total"]) <0) {
                            dsJVDetails.Tables[0].Rows.RemoveAt(i);
                        }

                    }
                    AmountWithProfit = Simulate.decimal_(dtHeader.Rows[0]["NetAmount"]);
                    AmountToWord = clsConvertNumberToString.NoToTxt(Simulate.Val(AmountWithProfit));
                    AmountWithOutDecimal = Simulate.String(Simulate.Integer32(AmountWithProfit));
                    AmountDecimal = Simulate.String(Simulate.Integer32((AmountWithProfit - AmountWithProfit) * 1000));
                    report.Load(MyPath);
                    report.RegisterData(ds);
                    report.RegisterData(dsBusinessPartner);
                    report.RegisterData(dsJVDetails);
                    report.SetParameterValue("report.TotalDueToWord", Simulate.String(clsConvertNumberToString.NoToTxt(Simulate.Val(Math.Abs(TotalDue)))));

                    report.SetParameterValue("report.AmountWithOutDecimal", Simulate.String(AmountWithOutDecimal));
                    report.SetParameterValue("report.AmountDecimal", Simulate.String(AmountDecimal));
                    report.SetParameterValue("report.AmountToWord", Simulate.String(AmountToWord));
                    report.SetParameterValue("report.DueDate", (Simulate.StringToDate(dtHeader.Rows[0]["VoucherDate"]).AddMonths(4)).ToString("yyyy-MM-dd"));

                    report.SetParameterValue("report.TotalDue", Simulate.Currency_format(TotalDue));
                    FastreportStanderdParameters(report, UserId, CompanyID);


                    report.Prepare();

                    return FastreporttoPDF(report);
                }
                else  //gift
                {



                    clsJournalVoucherDetails clsJVDetails = new clsJournalVoucherDetails();
                    dsJVDetails dsJVDetails = clsJVDetails.SelectJournalVoucherDetailsByParentIdForPrint(Simulate.String(dtHeader.Rows[0]["JVGuid"]), 0, 0);



                    FastReport.Report report = new FastReport.Report();


                    
                    string MyPath = getMyPath("rptGift", CompanyID);
                    for (int i = 0; i < dsJVDetails.Tables[0].Rows.Count; i++)
                    {
                        dsJVDetails.Tables[0].Rows[i]["Rowindex"] = (i + 1);
                        if (Simulate.Val(dsJVDetails.Tables[0].Rows[i]["Total"]) < 0)
                        {
                            dsJVDetails.Tables[0].Rows.RemoveAt(i);
                        }

                    }


                


                

                    AmountWithProfit = Simulate.decimal_(dtHeader.Rows[0]["NetAmount"]);
                    AmountToWord = clsConvertNumberToString.NoToTxt(Simulate.Val(AmountWithProfit));
                    AmountWithOutDecimal = Simulate.String(Simulate.Integer32(AmountWithProfit));
                    AmountDecimal = Simulate.String(Simulate.Integer32((AmountWithProfit - AmountWithProfit) * 1000));
                    report.Load(MyPath);
                    report.RegisterData(ds);
                    report.RegisterData(dsBusinessPartner);
                    report.RegisterData(dsJVDetails);

                    report.SetParameterValue("report.TotalDueToWord", Simulate.String(clsConvertNumberToString.NoToTxt(Simulate.Val(Math.Abs(TotalDue)))));

                    report.SetParameterValue("report.AmountWithOutDecimal", Simulate.String(AmountWithOutDecimal));
                    report.SetParameterValue("report.AmountDecimal", Simulate.String(AmountDecimal));
                    report.SetParameterValue("report.AmountToWord", Simulate.String(AmountToWord));
                    report.SetParameterValue("report.DueDate", (Simulate.StringToDate(dtHeader.Rows[0]["VoucherDate"]).AddMonths(4)).ToString("yyyy-MM-dd"));

                    report.SetParameterValue("report.TotalDue", Simulate.Currency_format(TotalDue));
                    FastreportStanderdParameters(report, UserId, CompanyID);


                    report.Prepare();

                    return FastreporttoPDF(report);


                }

            }
            catch (Exception ex)
            {

                return Json(ex);
            }

        }
     string    getMyPath(string ReportName, int CompanyID) {
            string a = "";
            try
            {
                string MyPath = ($"{Environment.CurrentDirectory}" + @"\Reports\"+ Simulate.String(CompanyID)+@"\"+ ReportName + ".frx");
              
                if (System.IO.File.Exists(MyPath))
                {
                    return MyPath;
                }
                else {
                    return ($"{Environment.CurrentDirectory}" + @"\Reports\" + ReportName + ".frx");
                }
       
            }
            catch (Exception)
            {

                return a;
            }
        }
        [HttpGet]
        [Route("PrintFinancingGuarantee")]
        public IActionResult PrintFinancingGuarantee(string guid, int UserId, int CompanyID)
        {
            try
            {

                FastReport.Utils.Config.WebMode = true;
                clsFinancingHeader clsFinancingHeader = new clsFinancingHeader();
                clsFinancingDetails clsFinancingDetails = new clsFinancingDetails();
                decimal AmountWithProfit = 0;
                DataTable dtHeader = clsFinancingHeader.SelectFinancingHeaderByGuid(guid, DateTime.Now.AddYears(-100), DateTime.Now.AddYears(100), 0,0,CompanyID, 0, "-1");
                DataTable dtDetails = clsFinancingDetails.SelectFinancingDetailsByHeaderGuid(guid,0, CompanyID );
                decimal TotalDue = 0;
                dsFinancing ds = new dsFinancing();
                dsBusinessPartner dsBusinessPartner = new dsBusinessPartner();
                if (dtDetails != null && dtDetails.Rows.Count > 0)
                {
                    for (int i = 0; i < dtDetails.Rows.Count; i++)
                    {
                        ds.Details.Rows.Add();

                        ds.Details.Rows[i]["Guid"] = Simulate.String(dtDetails.Rows[i]["Guid"]);
                        ds.Details.Rows[i]["HeaderGuid"] = Simulate.String(dtDetails.Rows[i]["HeaderGuid"]);
                        ds.Details.Rows[i]["RowIndex"] = Simulate.String(Simulate.Integer32( dtDetails.Rows[i]["RowIndex"])+1);
                        ds.Details.Rows[i]["Description"] = Simulate.String(dtDetails.Rows[i]["Description"]);
                        ds.Details.Rows[i]["TotalAmount"] = Simulate.decimal_(dtDetails.Rows[i]["TotalAmount"]);
                        ds.Details.Rows[i]["DownPayment"] = Simulate.decimal_(dtDetails.Rows[i]["DownPayment"]);
                        ds.Details.Rows[i]["FinancingAmount"] = Simulate.decimal_(dtDetails.Rows[i]["FinancingAmount"]);
                        ds.Details.Rows[i]["PeriodInMonths"] = Simulate.Integer32(dtDetails.Rows[i]["PeriodInMonths"]);
                        ds.Details.Rows[i]["InterestRate"] = Simulate.decimal_(dtDetails.Rows[i]["InterestRate"]);
                        ds.Details.Rows[i]["InterestAmount"] = Simulate.decimal_(dtDetails.Rows[i]["InterestAmount"]);
                        ds.Details.Rows[i]["TotalAmountWithInterest"] = Simulate.decimal_(dtDetails.Rows[i]["TotalAmountWithInterest"]);
                        ds.Details.Rows[i]["FirstInstallmentDate"] = Simulate.StringToDate(dtDetails.Rows[i]["FirstInstallmentDate"]).ToString("yyyy-MM-dd");
                        ds.Details.Rows[i]["InstallmentAmount"] = Simulate.decimal_(dtDetails.Rows[i]["InstallmentAmount"]);
                        ds.Details.Rows[i]["JVGuid"] = Simulate.String(dtDetails.Rows[i]["JVGuid"]);
                        ds.Details.Rows[i]["CreationUserID"] = Simulate.Integer32(dtDetails.Rows[i]["CreationUserID"]);
                        ds.Details.Rows[i]["CreationDate"] = Simulate.StringToDate(dtDetails.Rows[i]["CreationDate"]);
                        ds.Details.Rows[i]["ModificationUserID"] = Simulate.Integer32(dtDetails.Rows[i]["ModificationUserID"]);
                        ds.Details.Rows[i]["ModificationDate"] = Simulate.StringToDate(dtDetails.Rows[i]["ModificationDate"]);
                        ds.Details.Rows[i]["CompanyID"] = Simulate.Integer32(dtDetails.Rows[i]["CompanyID"]);
                        AmountWithProfit = AmountWithProfit + Simulate.decimal_(dtDetails.Rows[i]["TotalAmountWithInterest"]);

                        ;

                    }
                }
             
                if (dtHeader != null && dtHeader.Rows.Count > 0)
                {
                    for (int i = 0; i < dtHeader.Rows.Count; i++)
                    {
                        dsBusinessPartner = FillDsBusnessPartner(Simulate.Integer32(dtHeader.Rows[i]["BusinessPartnerID"]), Simulate.Integer32(dtHeader.Rows[i]["Grantor"]));
                        ds.Header.Rows.Add();

                        ds.Header.Rows[i]["Guid"] = Simulate.String(dtHeader.Rows[i]["Guid"]);
                        ds.Header.Rows[i]["VoucherDate"] = Simulate.StringToDate(dtHeader.Rows[i]["VoucherDate"]).ToString("yyyy-MM-dd");
                        ds.Header.Rows[i]["BranchID"] = Simulate.Integer32(dtHeader.Rows[i]["BranchID"]);
                        ds.Header.Rows[i]["VoucherNumber"] = Simulate.String(dtHeader.Rows[i]["VoucherNumber"]);
                        ds.Header.Rows[i]["BusinessPartnerID"] = Simulate.Integer32(dtHeader.Rows[i]["BusinessPartnerID"]);
                        ds.Header.Rows[i]["Note"] = Simulate.String(dtHeader.Rows[i]["Note"]);
                        ds.Header.Rows[i]["TotalAmount"] = Simulate.Currency_format(dtHeader.Rows[i]["TotalAmount"]);
                        ds.Header.Rows[i]["DownPayment"] = Simulate.Currency_format(dtHeader.Rows[i]["DownPayment"]);
                        ds.Header.Rows[i]["NetAmount"] = Simulate.Currency_format(dtHeader.Rows[i]["NetAmount"]);
                        ds.Header.Rows[i]["Grantor"] = Simulate.Integer32(dtHeader.Rows[i]["Grantor"]);
                   
                        ds.Header.Rows[i]["CreationUserID"] = Simulate.Integer32(dtHeader.Rows[i]["CreationUserID"]);
                        ds.Header.Rows[i]["CreationDate"] = Simulate.StringToDate(dtHeader.Rows[i]["CreationDate"]);
                        ds.Header.Rows[i]["ModificationUserID"] = Simulate.Integer32(dtHeader.Rows[i]["ModificationUserID"]);
                        ds.Header.Rows[i]["ModificationDate"] = Simulate.StringToDate(dtHeader.Rows[i]["ModificationDate"]);
                        ds.Header.Rows[i]["CompanyID"] = Simulate.Integer32(dtHeader.Rows[i]["CompanyID"]);
                        ds.Header.Rows[i]["BranchName"] = Simulate.String(dtHeader.Rows[i]["BranchName"]);
                        ds.Header.Rows[i]["BusinessPartnerName"] = Simulate.String(dtHeader.Rows[i]["BusinessPartnerName"]);

                        ds.Header.Rows[i]["GrantorName"] = Simulate.String(dtHeader.Rows[i]["GrantorName"]);

                        ds.Header.Rows[i]["CreationUserName"] = Simulate.String(dtHeader.Rows[i]["CreationUserName"]);
                        clsReports clsReports = new clsReports(); cls_AccountSetting cls_AccountSetting = new cls_AccountSetting();
                        DataTable dtAccountSetting = cls_AccountSetting.SelectAccountSetting(0, 0, Simulate.Integer32(dtHeader.Rows[i]["CompanyID"]));
                        clsInvoiceHeader clsInvoiceHeader = new clsInvoiceHeader();
                        int CustomerAccount = clsInvoiceHeader.GetValueFromDT(dtAccountSetting, "AccountRefID", Simulate.String((int)clsEnum.AccountMainSetting.CustomerAccount), 2);


                        DataTable dtStatment = clsReports.SelectAccountStatement(Simulate.StringToDate(dtHeader.Rows[i]["VoucherDate"]), Simulate.StringToDate(dtHeader.Rows[i]["VoucherDate"])
                            , 0, 0, CustomerAccount, Simulate.Integer32(dtHeader.Rows[i]["BusinessPartnerID"]), 0, true);
                        if (dtStatment != null && dtStatment.Rows.Count > 0)
                        {

                            TotalDue = Simulate.decimal_(dtStatment.Rows[dtStatment.Rows.Count - 1]["nettotal"]);
                        }



                    }
                }

                string AmountWithOutDecimal = "";
                string AmountDecimal = "";
                string AmountToWord = "";
                AmountToWord = clsConvertNumberToString.NoToTxt(Simulate.Val(AmountWithProfit));
                AmountWithOutDecimal = Simulate.String(Simulate.Integer32(AmountWithProfit));
                AmountDecimal = Simulate.String(Simulate.Integer32((AmountWithProfit - AmountWithProfit) * 1000));

                FastReport.Report report = new FastReport.Report();


                string MyPath = getMyPath("rptFinancingGuarantee",CompanyID);
              

                report.Load(MyPath);
                report.RegisterData(ds);
                report.RegisterData(dsBusinessPartner);
                report.SetParameterValue("report.TotalDueToWord", Simulate.String(clsConvertNumberToString.NoToTxt(Simulate.Val(Math.Abs( TotalDue)))));

                report.SetParameterValue("report.AmountWithOutDecimal", Simulate.String(AmountWithOutDecimal));
                report.SetParameterValue("report.AmountDecimal", Simulate.String(AmountDecimal));
                report.SetParameterValue("report.AmountToWord", Simulate.String(AmountToWord));


                report.SetParameterValue("report.DueDate", (Simulate.StringToDate(dtHeader.Rows[0]["VoucherDate"]).AddMonths(4)).ToString("yyyy-MM-dd"));

                report.SetParameterValue("report.TotalDue", Simulate.Currency_format(TotalDue));
                FastreportStanderdParameters(report, UserId, CompanyID);


                report.Prepare();

                return FastreporttoPDF(report);



            }
            catch (Exception ex)
            {

                return Json(ex);
            }

        }
        dsBusinessPartner FillDsBusnessPartner(int BusnessPartnerID, int GrantoID)
        {
            try
            {
                dsBusinessPartner ds =new dsBusinessPartner();
                clsBusinessPartner clsBusinessPartner = new clsBusinessPartner();
               DataTable dtBusinessPartner= clsBusinessPartner.SelectBusinessPartner(BusnessPartnerID, 0, "", "", -1, 0);
                DataTable dtGrantoID = clsBusinessPartner.SelectBusinessPartner(GrantoID, 0, "", "", -1, 0);
                if (dtBusinessPartner != null && dtBusinessPartner.Rows.Count > 0)
                {
                    for (int i = 0; i < dtBusinessPartner.Rows.Count; i++)
                    {
                        ds.BusinessPartner.Rows.Add();
                        ds.BusinessPartner.Rows[i]["ID"] = Simulate.Integer32(dtBusinessPartner.Rows[i]["ID"]);
                        ds.BusinessPartner.Rows[i]["AName"] = Simulate.String(dtBusinessPartner.Rows[i]["AName"]) ;
                        ds.BusinessPartner.Rows[i]["EName"] = Simulate.String(dtBusinessPartner.Rows[i]["EName"]);
                        ds.BusinessPartner.Rows[i]["CommercialName"] = Simulate.String(dtBusinessPartner.Rows[i]["CommercialName"]);
                        ds.BusinessPartner.Rows[i]["Address"] = Simulate.String(dtBusinessPartner.Rows[i]["Address"]);
                        ds.BusinessPartner.Rows[i]["Tel"] = Simulate.String(dtBusinessPartner.Rows[i]["Tel"]);
                        ds.BusinessPartner.Rows[i]["Active"] = Simulate.String(dtBusinessPartner.Rows[i]["Active"]);
                        ds.BusinessPartner.Rows[i]["Limit"] = Simulate.decimal_(dtBusinessPartner.Rows[i]["Limit"]);
                        ds.BusinessPartner.Rows[i]["Email"] = Simulate.String(dtBusinessPartner.Rows[i]["Email"]);
                        ds.BusinessPartner.Rows[i]["Type"] = Simulate.Integer32(dtBusinessPartner.Rows[i]["Type"]);
                        ds.BusinessPartner.Rows[i]["CompanyID"] = Simulate.Integer32(dtBusinessPartner.Rows[i]["CompanyID"]);
                        ds.BusinessPartner.Rows[i]["CreationUserID"] = Simulate.Integer32(dtBusinessPartner.Rows[i]["CreationUserID"]);
                        ds.BusinessPartner.Rows[i]["CreationDate"] = Simulate.StringToDate(dtBusinessPartner.Rows[i]["CreationDate"]).ToString("yyyy-MM-dd");
                        ds.BusinessPartner.Rows[i]["ModificationUserID"] = Simulate.Integer32(dtBusinessPartner.Rows[i]["ModificationUserID"]);
                        ds.BusinessPartner.Rows[i]["ModificationDate"] = Simulate.StringToDate(dtBusinessPartner.Rows[i]["ModificationDate"]).ToString("yyyy-MM-dd");
                        ds.BusinessPartner.Rows[i]["EmpCode"] = Simulate.String(dtBusinessPartner.Rows[i]["EmpCode"]);
                        ds.BusinessPartner.Rows[i]["StreetName"] = Simulate.String(dtBusinessPartner.Rows[i]["StreetName"]);
                        ds.BusinessPartner.Rows[i]["HouseNumber"] = Simulate.String(dtBusinessPartner.Rows[i]["HouseNumber"]);
                        ds.BusinessPartner.Rows[i]["NationalNumber"] = Simulate.String(dtBusinessPartner.Rows[i]["NationalNumber"]);
                        ds.BusinessPartner.Rows[i]["PassportNumber"] = Simulate.String(dtBusinessPartner.Rows[i]["PassportNumber"]);
                        ds.BusinessPartner.Rows[i]["Nationality"] = Simulate.Integer32(dtBusinessPartner.Rows[i]["Nationality"]);
                        ds.BusinessPartner.Rows[i]["IDNumber"] = Simulate.String(dtBusinessPartner.Rows[i]["IDNumber"]);
                    }
                }


                if (dtGrantoID != null && dtGrantoID.Rows.Count > 0)
                {
                    for (int i = 0; i < dtGrantoID.Rows.Count; i++)
                    {
                        ds.BusinessGrantor.Rows.Add();
                        ds.BusinessGrantor.Rows[i]["ID"] = Simulate.Integer32(dtGrantoID.Rows[i]["ID"]);
                        ds.BusinessGrantor.Rows[i]["AName"] = Simulate.String(dtGrantoID.Rows[i]["AName"]);
                        ds.BusinessGrantor.Rows[i]["EName"] = Simulate.String(dtGrantoID.Rows[i]["EName"]);
                        ds.BusinessGrantor.Rows[i]["CommercialName"] = Simulate.String(dtGrantoID.Rows[i]["CommercialName"]);
                        ds.BusinessGrantor.Rows[i]["Address"] = Simulate.String(dtGrantoID.Rows[i]["Address"]);
                        ds.BusinessGrantor.Rows[i]["Tel"] = Simulate.String(dtGrantoID.Rows[i]["Tel"]);
                        ds.BusinessGrantor.Rows[i]["Active"] = Simulate.String(dtGrantoID.Rows[i]["Active"]);
                        ds.BusinessGrantor.Rows[i]["Limit"] = Simulate.decimal_(dtGrantoID.Rows[i]["Limit"]);
                        ds.BusinessGrantor.Rows[i]["Email"] = Simulate.String(dtGrantoID.Rows[i]["Email"]);
                        ds.BusinessGrantor.Rows[i]["Type"] = Simulate.Integer32(dtGrantoID.Rows[i]["Type"]);
                        ds.BusinessGrantor.Rows[i]["CompanyID"] = Simulate.Integer32(dtGrantoID.Rows[i]["CompanyID"]);
                        ds.BusinessGrantor.Rows[i]["CreationUserID"] = Simulate.Integer32(dtGrantoID.Rows[i]["CreationUserID"]);
                        ds.BusinessGrantor.Rows[i]["CreationDate"] = Simulate.StringToDate(dtGrantoID.Rows[i]["CreationDate"]).ToString("yyyy-MM-dd");
                        ds.BusinessGrantor.Rows[i]["ModificationUserID"] = Simulate.Integer32(dtGrantoID.Rows[i]["ModificationUserID"]);
                        ds.BusinessGrantor.Rows[i]["ModificationDate"] = Simulate.StringToDate(dtGrantoID.Rows[i]["ModificationDate"]).ToString("yyyy-MM-dd");
                        ds.BusinessGrantor.Rows[i]["EmpCode"] = Simulate.String(dtGrantoID.Rows[i]["EmpCode"]);
                        ds.BusinessGrantor.Rows[i]["StreetName"] = Simulate.String(dtGrantoID.Rows[i]["StreetName"]);
                        ds.BusinessGrantor.Rows[i]["HouseNumber"] = Simulate.String(dtGrantoID.Rows[i]["HouseNumber"]);
                        ds.BusinessGrantor.Rows[i]["NationalNumber"] = Simulate.String(dtGrantoID.Rows[i]["NationalNumber"]);
                        ds.BusinessGrantor.Rows[i]["PassportNumber"] = Simulate.String(dtGrantoID.Rows[i]["PassportNumber"]);
                        ds.BusinessGrantor.Rows[i]["Nationality"] = Simulate.Integer32(dtGrantoID.Rows[i]["Nationality"]);
                        ds.BusinessGrantor.Rows[i]["IDNumber"] = Simulate.String(dtGrantoID.Rows[i]["IDNumber"]);
                    }
                }
                return ds;

            }
            catch (Exception)
            {

                throw;
            }
        }



        [HttpGet]
        [Route("SelectFinancingPaymentsByFinancingGuid")]
        public string SelectFinancingPaymentsByFinancingGuid(string Guid,  int CompanyID )
        {
            try
            {
                string a = @"  select * from tbl_Reconciliation where JVDetailsGuid in (
 select [Guid] from tbl_JournalVoucherDetails 
 
 where tbl_JournalVoucherDetails.ParentGuid='"+ Guid + @"'
and tbl_JournalVoucherDetails.CompanyID="+ CompanyID + @" )";

                clsSQL cls = new clsSQL();
                 DataTable dt = cls.ExecuteQueryStatement(a);
                if (dt != null)
                {

                    string JSONString = string.Empty;
                    JSONString = JsonConvert.SerializeObject(dt);
                    return JSONString;
                }
                else
                {

                    return "";
                }
            }
            catch (Exception)
            {

                throw;
            }
        }


        [HttpGet]
        [Route("SelectLoanReportRJ")]
        public string SelectLoanReportRJ(string Date1, string Date2,string Type,  int UserId, int CompanyID,int ARAccountID)
        {
            try
            {clsFinancingHeader cls=new clsFinancingHeader();
                DataTable dt;
                if (Type == "Sales") {
                    dt = cls.SelectSalesReportRJ(Date1, Date2, UserId, CompanyID, ARAccountID);
                } else if (Type == "Subscriptions") {
                    dt = cls.SelectSubscriptionsReportRJ(Date1, Date2, UserId, CompanyID,0,0);
                    
                } else {
                    dt = cls.SelectLoanReportRJ(Date1, Date2, UserId, CompanyID);
                }
          


                if (dt != null)
                {

                    string JSONString = string.Empty;
                    JSONString = JsonConvert.SerializeObject(dt);
                    return JSONString;
                }
                else
                {

                    return "";
                }
            }
            catch (Exception)
            {

                throw;
            }
        }

        [HttpGet]
        [Route("SelectLoanReportRJCSV")]
        public ActionResult SelectLoanReportRJCSV(string Date1, string Date2,string Type, int UserId, int CompanyID, int ARAccountID)
        {
            try
            {
                List<String> ColumnType = new List<String>();
                List<DataTable>  dtlist = new List<DataTable> ();
                List<String> dtName = new List<String>();
                clsFinancingHeader cls = new clsFinancingHeader();
                  
                if (Type == "Sales")
                {
                    DataTable dt = cls.SelectSalesReportRJ(Date1, Date2, UserId, CompanyID, ARAccountID);
                    dt.Columns.RemoveAt(0);
                    ColumnType.Add("int");
                    ColumnType.Add("string");
                    ColumnType.Add("string");
                    ColumnType.Add("int");
                    ColumnType.Add("string");
                    ColumnType.Add("int");
                    ColumnType.Add("int");
                    ColumnType.Add("string");
                    ColumnType.Add("string");
                    ColumnType.Add("string");
                    ColumnType.Add("string");
                    ColumnType.Add("string");
                    ColumnType.Add("int");
                    ColumnType.Add("string");
                    ColumnType.Add("string");
                  
                    dtlist.Add(dt);
                    dtName.Add("ts");
                }
                else if (Type == "Loans")
                {
                    DataTable dt = cls.SelectLoanReportRJ(Date1, Date2, UserId, CompanyID);
                    ColumnType.Add("string");
                    ColumnType.Add("string");
                    ColumnType.Add("string");
                    ColumnType.Add("int");
                    ColumnType.Add("string");
                    ColumnType.Add("int");
                    ColumnType.Add("int");
                    ColumnType.Add("string");
                    ColumnType.Add("string");
                    ColumnType.Add("string");
                    ColumnType.Add("string");
                    ColumnType.Add("string");
                    ColumnType.Add("int"); 
                    ColumnType.Add("string");
                    ColumnType.Add("int");

                   
                    dtlist.Add(dt);
                    dtName.Add("tl");
                } else
                {
                    clsSubscriptions clsSubscriptions=new clsSubscriptions();
                    clsSubscriptionsStatus clsSubscriptionsStatus = new clsSubscriptionsStatus();   
                    clsSubscriptionsTypes   clsSubscriptionsTypes = new clsSubscriptionsTypes();    
                DataTable dttype=    clsSubscriptionsTypes.SelectSubscriptionsTypes(0, CompanyID);
                    DataTable dtStatus = clsSubscriptionsStatus.SelectSubscriptionsStatus(0, CompanyID);

                    for (int i = 0; i < dttype.Rows.Count; i++)
                    {
                        for (int ii = 0; ii < dtStatus.Rows.Count; ii++)
                        {
                            DataTable dt = cls.SelectSubscriptionsReportRJ(Date1, Date2, UserId, CompanyID
                                , Simulate.Integer32(dttype.Rows[i]["ID"]), Simulate.Integer32(dtStatus.Rows[ii]["ID"]));
                            dt.Columns.RemoveAt(0);

                          
                            dtlist.Add(dt);
                            dtName.Add(Simulate.String( dtStatus.Rows[ii]["Code"])+ " "+ Simulate.String(dttype.Rows[i]["Code"]));
                      
                        }

                    }
                   
                    ColumnType.Add("string");
                    ColumnType.Add("string");
                    ColumnType.Add("int");
                    ColumnType.Add("string");
                    ColumnType.Add("int");
                    ColumnType.Add("int");
                    ColumnType.Add("string");
                    ColumnType.Add("string");
                    ColumnType.Add("string");
                    ColumnType.Add("string"); 
                    ColumnType.Add("string");
                    ColumnType.Add("int");





                }
           
               


                return FastreporttoCSV(dtlist, dtName, ColumnType);
            }
            catch (Exception)
            {

                throw;
            }
        }
        
        #endregion
        #region UserAuthorization


        [HttpGet]
        [Route("SelectUserAuthorization")]
        public string SelectUserAuthorization(int UserId, int PageID, int CompanyID)
        {
            try
            {
                clsUserAuthorization clsUserAuthorization = new clsUserAuthorization();
                DataTable dt = clsUserAuthorization.SelectUserAuthorization(UserId, PageID,   CompanyID);
                if (dt != null)
                {

                    string JSONString = string.Empty;
                    JSONString = JsonConvert.SerializeObject(dt);
                    return JSONString;
                }
                else
                {

                    return "";
                }
            }
            catch (Exception)
            {

                throw;
            }




        }
        [HttpGet]
        [Route("DeleteUserAuthorizationByUserID")]
        public bool DeleteUserAuthorizationByUserID(int UserId)
        {
            try
            {
                clsUserAuthorization clsUserAuthorization = new clsUserAuthorization();
               
                
                
                bool A = clsUserAuthorization.DeleteUserAuthorizationByUserID(UserId);
                return A;
            }
            catch (Exception)
            {

                throw;
            }

        }
        [HttpPost]
        [Route("InsertUserAuthorization")]
        public string InsertUserAuthorization(int CompanyID,[FromBody] string DetailsList)
        {
            try
            {
                SqlTransaction trn; clsSQL clsSQL = new clsSQL();
                SqlConnection con = new SqlConnection(clsSQL.conString);
                con.Open();
                trn = con.BeginTransaction();
                List<DBUserAuthrization> details = JsonConvert.DeserializeObject<List<DBUserAuthrization>>(DetailsList);

                DBUserAuthrization DBUserAuthrization;
                clsUserAuthorization clsUserAuthorization = new clsUserAuthorization();
                clsUserAuthorization.DeleteUserAuthorizationByUserID(details[0].UserID);
                bool IsSaved = true;
                for (int i = 0; i < details.Count; i++)
                {
                    string A = clsUserAuthorization.InsertUserAuthorization(details[i],trn);
                    if (A == "") {
                        IsSaved = false;
                    }
                }
                if (IsSaved)
                { trn.Commit(); return "True"; }
                else
                { trn.Rollback(); return "False"; }
            }
            catch (Exception ex)
            {

                throw;
            }

        }

        #endregion
        #region Forms
        [HttpGet]
        [Route("SelectForms")]
        public string SelectForms(int FormID)
        {
            try
            {
                clsSQL clsSQL=new clsSQL();

                DataTable dt = clsSQL.ExecuteQueryStatement("select * from tbl_Forms");
                if (dt != null)
                {

                    string JSONString = string.Empty;
                    JSONString = JsonConvert.SerializeObject(dt);
                    return JSONString;
                }
                else
                {

                    return "";
                }
            }
            catch (Exception)
            {

                throw;
            }




        }
        #endregion
        #region UserAuthorizationModels


        [HttpGet]
        [Route("SelectUserAuthorizationModels")]
        public string SelectUserAuthorizationModels(int UserId, int TypeID, int ModelID, int CompanyID)
        {
            try
            {
                clsUserAuthorizationModels clsUserAuthorizationModels = new clsUserAuthorizationModels();
                DataTable dt = clsUserAuthorizationModels.SelectUserAuthorizationModels(UserId, TypeID, ModelID, CompanyID);
                if (dt != null)
                {

                    string JSONString = string.Empty;
                    JSONString = JsonConvert.SerializeObject(dt);
                    return JSONString;
                }
                else
                {

                    return "";
                }
            }
            catch (Exception)
            {

                throw;
            }




        }
        [HttpGet]
        [Route("DeleteUserAuthorizationModelsByUserID")]
        public bool DeleteUserAuthorizationModelsByUserID(int UserId)
        {
            try
            {
                clsUserAuthorizationModels clsUserAuthorizationModels = new clsUserAuthorizationModels();



                bool A = clsUserAuthorizationModels.DeleteUserAuthorizationModelsByUserID(UserId);
                return A;
            }
            catch (Exception)
            {

                throw;
            }

        }
        [HttpPost]
        [Route("InsertUserAuthorizationModels")]
        public string InsertUserAuthorizationModels(int CompanyID, [FromBody] string DetailsList)
        {
            try
            {
                SqlTransaction trn; clsSQL clsSQL = new clsSQL();
                SqlConnection con = new SqlConnection(clsSQL.conString);
                con.Open();
                trn = con.BeginTransaction();
                List<DBUserAuthrizationModels> details = JsonConvert.DeserializeObject<List<DBUserAuthrizationModels>>(DetailsList);

                DBUserAuthrizationModels DBUserAuthrizationModels;
                clsUserAuthorizationModels clsUserAuthorizationModels = new clsUserAuthorizationModels();
                clsUserAuthorizationModels.DeleteUserAuthorizationModelsByUserID(details[0].UserID);
                bool IsSaved = true;
                for (int i = 0; i < details.Count; i++)
                {
                    string A = clsUserAuthorizationModels.InsertUserAuthorizationModels(details[i], trn);
                    if (A == "")
                    {
                        IsSaved = false;
                    }
                }
                if (IsSaved)
                { trn.Commit(); return "True"; }
                else
                { trn.Rollback(); return "False"; }
            }
            catch (Exception ex)
            {

                throw;
            }

        }

        #endregion
        #region LoanTypes


        [HttpGet]
        [Route("SelectLoanTypes")]
        public string SelectLoanTypes(int ID, string LoanMainType,int CompanyID)
        {
            try
            {
                clsLoanTypes clsLoanTypes = new clsLoanTypes();
                DataTable dt = clsLoanTypes.SelectLoanTypes(ID, LoanMainType, "", "", "", CompanyID);
                if (dt != null)
                {

                    string JSONString = string.Empty;
                    JSONString = JsonConvert.SerializeObject(dt);
                    return JSONString;
                }
                else
                {

                    return "";
                }
            }
            catch (Exception)
            {

                throw;
            }




        }
        [HttpGet]
        [Route("DeleteLoanTypesByID")]
        public bool DeleteLoanTypesByID(int ID)
        {
            try
            {
                //clsJournalVoucherDetails clsJournalVoucherDetails = new clsJournalVoucherDetails();
                //DataTable dt = clsJournalVoucherDetails.SelectJournalVoucherDetailsByParentId("", 0, 0, ID, 0, 0);
                //if (dt != null && dt.Rows.Count > 0)
                //{

                //    return false;
                //}
                clsLoanTypes clsLoanTypes = new clsLoanTypes();
                bool A = clsLoanTypes.DeleteLoanTypesByID(ID);
                return A;
            }
            catch (Exception)
            {

                throw;
            }

        }
        [HttpGet]
        [Route("InsertLoanTypes")]
        public int InsertLoanTypes(
            string AName, 
            string EName, 
            string Code,
            bool IsReturned,
            int PaymentAccountID, 
            int ReceivableAccountID,
            decimal DefaultAmount,
            int DevidedMonths,
            bool IsActive,
            decimal InterestRate,
            int MainTypeID,int ProfitAccount, bool IsStopBP,
            int CompanyID,
            int CreationUserId )
        {
            try
            {
                clsLoanTypes clsLoanTypes = new clsLoanTypes();
                int A = clsLoanTypes.InsertLoanTypes(
                    Simulate.String(AName),
                Simulate.String(EName), Simulate.String(Code),
                   IsReturned,
                   PaymentAccountID,
                   ReceivableAccountID,
                   DefaultAmount,
                   DevidedMonths,
                   IsActive,
                   InterestRate,
                   MainTypeID,
                   ProfitAccount,   IsStopBP,
                    CompanyID, 
                    CreationUserId
                          );
                return A;
            }
            catch (Exception ex)
            {

                throw;
            }

        }
        [HttpGet]
        [Route("UpdateLoanTypes")]
        public int UpdateLoanTypes(int ID,
             string AName,
             string EName,
             string Code,
             bool IsReturned,
            int PaymentAccountID, 
            int ReceivableAccountID, 
            decimal DefaultAmount,
            int DevidedMonths,
            bool IsActive,
            decimal InterestRate,
            int  MainTypeID,int ProfitAccount,bool IsStopBP,
            int ModificationUserId)
        {
            try
            {
                clsLoanTypes clsLoanTypes = new clsLoanTypes();
                int A = clsLoanTypes.UpdateLoanTypes(ID, 
                    Simulate.String(AName),
                Simulate.String(EName), 
                Simulate.String(Code), 
             IsReturned,
             PaymentAccountID, 
             ReceivableAccountID,
             DefaultAmount,  
             DevidedMonths, IsActive,
InterestRate,
MainTypeID, ProfitAccount, IsStopBP,
             ModificationUserId);
                return A;
            }
            catch (Exception)
            {

                throw;
            }

        }
        #endregion
        #region Reconciliation


        [HttpGet]
        [Route("SelectReconciliationByJVDetailsGuid")]
        public string SelectReconciliationByJVDetailsGuid(int VoucherNumber, string JVDetailsGuid, int CompanyID)
        {
            try
            {
                clsReconciliation clsReconciliation = new clsReconciliation();
                DataTable dt = clsReconciliation.SelectReconciliationByJVDetailsGuid(VoucherNumber, JVDetailsGuid, CompanyID);
                if (dt != null)
                {

                    string JSONString = string.Empty;
                    JSONString = JsonConvert.SerializeObject(dt);
                    return JSONString;
                }
                else
                {

                    return "";
                }
            }
            catch (Exception)
            {

                throw;
            }




        }
        [HttpGet]
        [Route("SelectReconciliationDetails")]
        public string SelectReconciliationDetails(int AccountID,int SubAccountID,string TransactionGuid,  int CompanyID)
        {
            try
            {
                clsReconciliation clsReconciliation = new clsReconciliation();
                DataTable dt = clsReconciliation.SelectReconciliationDetails(AccountID, SubAccountID , Simulate.String( TransactionGuid),CompanyID);
                if (dt != null)
                {

                    string JSONString = string.Empty;
                    JSONString = JsonConvert.SerializeObject(dt);
                    return JSONString;
                }
                else
                {

                    return "";
                }
            }
            catch (Exception)
            {

                throw;
            }




        }
        [HttpGet]
        [Route("SelectAccountsForReconciliation")]
        public string SelectAccountsForReconciliation( int CompanyID)
        {
            try
            {
                clsReconciliation clsReconciliation = new clsReconciliation();
                DataTable dt = clsReconciliation.SelectAccountsForReconciliation(  CompanyID);
                if (dt != null)
                {
                    string JSONString = string.Empty;
                    JSONString = JsonConvert.SerializeObject(dt);
                    return JSONString;
                }
                else
                {
                    return "";
                }
            }
            catch (Exception)
            {

                throw;
            }
        }
        [HttpGet]
        [Route("AccountsAutoReconciliation")]

        public string AccountsAutoReconciliation(int AccountID, int SubAccountID, int CompanyID, int CreationUserId)

        {
            try
            {
                int VoucherNumber = 0;

                SqlTransaction trn; clsSQL clsSQL = new clsSQL();
                SqlConnection con = new SqlConnection(clsSQL.conString);
                con.Open();
                trn = con.BeginTransaction();
                try
                {
                    clsReconciliation clsReconciliation=new clsReconciliation ();
                    DataTable maxDT = clsReconciliation.SelectReconciliationMaxNumber(CompanyID, trn);

                    if (maxDT != null && maxDT.Rows.Count > 0)
                    {
                        VoucherNumber = 1 + Simulate.Integer32(maxDT.Rows[0][0]);
                    }
                    else {
                        VoucherNumber = 1;
                    }
                DataTable dt = clsReconciliation.SelectAccountsForAutoReconciliation(AccountID,  SubAccountID,  CompanyID);
                bool isSaved = true;
                if (dt != null && dt.Rows.Count > 0)
                {
                    double TotalDebit = 0;

                    double TotalCredit = 0;
                    for (int i = 0; i < dt.Rows.Count; i++)
                    {
                        TotalDebit = TotalDebit + Simulate.Val(dt.Rows[i]["Debit"]);
                        TotalCredit = TotalCredit + Simulate.Val(dt.Rows[i]["Credit"]);
                    }
                 
                        for (int i = 0; i < dt.Rows.Count; i++)
                        {
                            if (TotalDebit > TotalCredit && Simulate.Val(dt.Rows[i]["Credit"]) >0) {
                              var a =  clsReconciliation.InsertReconciliation(VoucherNumber,Simulate.String( dt.Rows[i]["Guid"]),Simulate.decimal_( dt.Rows[i]["Total"]),CompanyID,CreationUserId,Simulate.String( dt.Rows[i]["Guid"]),trn);

                            }
                           else if (TotalCredit>TotalDebit && Simulate.Val(dt.Rows[i]["Debit"]) > 0)
                            {
                                var a = clsReconciliation.InsertReconciliation(VoucherNumber, Simulate.String(dt.Rows[i]["Guid"]), Simulate.decimal_(dt.Rows[i]["Total"]), CompanyID, CreationUserId, Simulate.String(dt.Rows[i]["Guid"]), trn);

                            }

                        }
                        if (TotalCredit > TotalDebit) {
                            double RemainingAmount = TotalDebit;
                            for (int i = 0; i < dt.Rows.Count; i++)
                            {
                                RemainingAmount = RemainingAmount - Simulate.Val(dt.Rows[i]["Credit"]);
                                if (RemainingAmount <= 0)
                                {
                                    var a = clsReconciliation.InsertReconciliation(VoucherNumber, Simulate.String(dt.Rows[i]["Guid"]),Simulate.decimal_( RemainingAmount+ Simulate.Val(dt.Rows[i]["Credit"]))*-1, CompanyID, CreationUserId, Simulate.String(dt.Rows[i]["Guid"]), trn);


                                    break;
                                }
                                else {
                                    var a = clsReconciliation.InsertReconciliation(VoucherNumber, Simulate.String(dt.Rows[i]["Guid"]), Simulate.decimal_(dt.Rows[i]["Total"]), CompanyID, CreationUserId, Simulate.String(dt.Rows[i]["Guid"]), trn);


                                }

                            }
                        
                        }else if (TotalDebit > TotalCredit)
                        {
                            double RemainingAmount = TotalCredit;
                            for (int i = 0; i < dt.Rows.Count; i++)
                            {
                                RemainingAmount = RemainingAmount - Simulate.Val(dt.Rows[i]["Debit"]);
                                if (RemainingAmount <= 0)
                                {
                                    var a = clsReconciliation.InsertReconciliation(VoucherNumber, Simulate.String(dt.Rows[i]["Guid"]), Simulate.decimal_(RemainingAmount + Simulate.Val(dt.Rows[i]["Debit"])), CompanyID, CreationUserId, Simulate.String(dt.Rows[i]["Guid"]), trn);


                                    break;
                                }
                                else
                                {
                                    var a = clsReconciliation.InsertReconciliation(VoucherNumber, Simulate.String(dt.Rows[i]["Guid"]), Simulate.decimal_(dt.Rows[i]["Total"]), CompanyID, CreationUserId, Simulate.String(dt.Rows[i]["Guid"]), trn);


                                }

                            }



                        }
                     

                }
               DataTable dt1=     clsReconciliation.SelectReconciliationByJVDetailsGuid(VoucherNumber, "", 0,trn);
                    string sum = dt1.Compute("Sum(Amount)", "").ToString();

                    //  InsertReconciliation("", 0, JsonConvert.SerializeObject(tbl_Reconciliations), CompanyID, CreationUserId);
                    if (isSaved && Simulate.Val( sum)==0)
                        trn.Commit();
                    else {
                        VoucherNumber = 0;
                        trn.Rollback();
                    }
                    return Simulate.String(VoucherNumber) ;
                }
                catch (Exception)
                {
                    trn.Rollback();
                    return "";
                }
                finally { con.Close(); }

            }
            catch (Exception ex)
            {

                return "";
            }

        }
        [HttpGet]
        [Route("SelectLoanScheduling")]
        public string SelectLoanScheduling(int AccountID, int SubAccountID,   int CompanyID)
        {
            try
            {
                clsReconciliation clsReconciliation = new clsReconciliation();
                DataTable dt = clsReconciliation.SelectLoanScheduling(AccountID, SubAccountID,   CompanyID);
                if (dt != null)
                {

                    string JSONString = string.Empty;
                    JSONString = JsonConvert.SerializeObject(dt);
                    return JSONString;
                }
                else
                {

                    return "";
                }
            }
            catch (Exception)
            {

                throw;
            }




        }
        [HttpPost]
        [Route("InsertLoanScheduling")]

        public string InsertLoanScheduling(int BranchID, int CostCenterID, string Notes, string JVNumber, int JVTypeID, [FromBody] string DetailsList, int CompanyID, DateTime VoucherDate, int CreationUserId)

        {
            try
            {
            var JVGuid=    InsertJournalVoucherHeader(BranchID,   CostCenterID,   Notes,   JVNumber,   JVTypeID,   DetailsList,  CompanyID,  VoucherDate,  CreationUserId);
                 List<tbl_JournalVoucherDetails> details = JsonConvert.DeserializeObject<List<tbl_JournalVoucherDetails>>(DetailsList);
                clsJournalVoucherDetails clsJournalVoucherDetails = new clsJournalVoucherDetails();
               DataTable dt = clsJournalVoucherDetails.SelectJournalVoucherDetailsByParentId(JVGuid,0,0,0,0,0);
                SqlTransaction trn; clsSQL clsSQL = new clsSQL();
                SqlConnection con = new SqlConnection(clsSQL.conString);
                con.Open();
                trn = con.BeginTransaction();  
                try
                { 
                    bool IsSaved = true;
                      clsReconciliation clsReconciliation =new clsReconciliation();

                    List< tbl_Reconciliation> tbl_Reconciliations=new List<tbl_Reconciliation> ();
                    for (int i = 0; i < dt.Rows.Count; i++)
                    {
                        if (Simulate.decimal_(dt.Rows[i]["Total"]) < 0) { 
                        tbl_Reconciliation a = new tbl_Reconciliation();
                        a.CreationDate =Simulate.StringToDate(dt.Rows[i]["CreationDate"]);                        ;
                        a.Amount= Simulate.decimal_(dt.Rows[i]["Total"]);  
                        a.JVDetailsGuid= Simulate.String(dt.Rows[i]["Guid"]);  
                        a.TransactionGuid = JVGuid;
                        a.VoucherNumber = 0;
                        a.CompanyID =  CompanyID;
                        a.CreationUserID =  CreationUserId;   
                       
                        tbl_Reconciliations.Add(a);
                        }
                    }
                    for (int i = 0; i < details.Count; i++)
                    {
                        if (Simulate.String(details[i].Guid) !="")
                        {
                            tbl_Reconciliation a = new tbl_Reconciliation();
                            a.CreationDate = details[i].CreationDate;
                            a.Amount = details[i].Total;
                            a.JVDetailsGuid = details[i].Guid;
                            a.TransactionGuid = JVGuid;
                            a.VoucherNumber = 0;
                            a.CompanyID = CompanyID;
                            a.CreationUserID = CreationUserId;

                            tbl_Reconciliations.Add(a);
                        }
                    }
                    InsertReconciliation("",0, JsonConvert.SerializeObject( tbl_Reconciliations), CompanyID, CreationUserId);
                    if (IsSaved)
                        trn.Commit();
                    else
                        trn.Rollback();
                    return JVGuid;
                }
                catch (Exception)
                {

                    trn.Rollback();
                    return "";
                }
                finally { con.Close(); }

            }
            catch (Exception ex)
            {

                return "";
            }

        }
        [HttpGet]
        [Route("DeleteReconciliationByVoucherNumber")]
        public bool DeleteReconciliationByVoucherNumber(int   VoucherNumber, int CompanyID)
        {
            try
            {
                //clsJournalVoucherDetails clsJournalVoucherDetails = new clsJournalVoucherDetails();
                //DataTable dt = clsJournalVoucherDetails.SelectJournalVoucherDetailsByParentId("", 0, 0, ID, 0, 0);
                //if (dt != null && dt.Rows.Count > 0)
                //{

                //    return false;
                //}
                clsReconciliation clsReconciliation = new clsReconciliation();
                //bool A = clsReconciliation.DeleteReconciliationByVoucherNumber(VoucherNumber, CompanyID);
                return false;
            }
            catch (Exception)
            {

                throw;
            }

        }
        [HttpGet]
        [Route("UpdateBusinessPartnersStatus")]
        public bool UpdateBusinessPartnersStatus(int ID, bool Status,int CompanyID)
        {
            try
            {
                string A = @"update tbl_BusinessPartner set Active = @Status  where ID =@ID and companyID=@companyID";
                clsSQL clssql = new clsSQL();
                SqlParameter[] prm =
                 {
                        new SqlParameter("@ID", SqlDbType.Int) { Value = ID },
                    new SqlParameter("@Status", SqlDbType.Bit) { Value = Status },
                        new SqlParameter("@CompanyID", SqlDbType.Int) { Value =CompanyID },
                };
                clsSQL cls =new clsSQL();
                cls.ExecuteNonQueryStatement(A, prm);
                //clsJournalVoucherDetails clsJournalVoucherDetails = new clsJournalVoucherDetails();
                //DataTable dt = clsJournalVoucherDetails.SelectJournalVoucherDetailsByParentId("", 0, 0, ID, 0, 0);
                //if (dt != null && dt.Rows.Count > 0)
                //{

                //    return false;
                //}
               // clsReconciliation clsReconciliation = new clsReconciliation();
                //bool A = clsReconciliation.DeleteReconciliationByVoucherNumber(VoucherNumber, CompanyID);
                return false;
            }
            catch (Exception)
            {

                throw;
            }

        }
        
        [HttpPost]
        [Route("InsertReconciliation")]
        public int InsertReconciliation(string guid,int VoucherNumber,
            [FromBody] string DetailsList,

            int CompanyID, 
            int CreationUserId)
        {
            try
            {
                List<tbl_Reconciliation> details = JsonConvert.DeserializeObject<List<tbl_Reconciliation>>(DetailsList);
                clsReconciliation clsReconciliation = new clsReconciliation();
             







                SqlTransaction trn;
                clsSQL clsSQL = new clsSQL();
                SqlConnection con = new SqlConnection(clsSQL.conString);
                con.Open();
                trn = con.BeginTransaction();
                String A = "";
                bool IsSaved = true;
                try
                {
                    if (guid != "") {
                        clsReconciliation.DeleteReconciliationByVoucherNumber(guid, CompanyID, trn);
                    } else {
                        clsReconciliation.DeleteReconciliationByVoucherNumber(details[0].TransactionGuid, CompanyID, trn);
                    }
             
                    DataTable dtMaxNUmber = clsReconciliation.SelectReconciliationMaxNumber(CompanyID,trn);
                    if (VoucherNumber == 0) { 
                 
                    if (dtMaxNUmber != null && dtMaxNUmber.Rows.Count > 0) {
                        VoucherNumber = Simulate.Integer32(dtMaxNUmber.Rows[0][0]) +1;
                    }
                    }

                    for (int i = 0; i < details.Count; i++)
                    {
                         A = clsReconciliation.InsertReconciliation(VoucherNumber,
                        details[i].JVDetailsGuid,

                       details[i].Amount,

                        CompanyID,
                        CreationUserId,
                        details[i].TransactionGuid, 
                        trn
                              ); if (A == "")
                            IsSaved = false;

                    }

                    


                    if (IsSaved)
                        trn.Commit();
                    else
                        trn.Rollback();
                }
                catch (Exception)
                {
                    trn.Rollback();

                }
                finally { con.Close(); }






               
                
                return 1;
            }
            catch (Exception ex)
            {

                throw;
            }

        }

        #endregion
        #region MyGeneralReports
        [HttpGet]
        [Route("SelectAgingReports")]
        public string SelectAgingReports(DateTime date1, DateTime date2, DateTime date3, DateTime date4,
            DateTime date5, DateTime date6, string  Accounts,int UserID, int CompanyID)
        {
            try
            {
                clsReports clsReports = new clsReports();
              DataTable dt=  clsReports.SelectAgingReports(date1, date2, date3, date4, date5, date6, Accounts, CompanyID);
                if (dt != null)
                {

                    string JSONString = string.Empty;
                    JSONString = JsonConvert.SerializeObject(dt);
                    return JSONString;
                }
                else
                {

                    return "";
                }
            }
            catch (Exception)
            {

                throw;
            }




        }
        [HttpGet]
        [Route("SelectAgingReportsPDF")]
        public IActionResult SelectAgingReportsPDF(DateTime date1, DateTime date2, DateTime date3, DateTime date4,
            DateTime date5, DateTime date6, string Accounts, int UserID, int CompanyID)
        {
            try
            {
               

                FastReport.Utils.Config.WebMode = true;
                clsReports clsReports = new clsReports();
                DataTable dt = clsReports.SelectAgingReports(date1, date2, date3, date4, date5, date6, Accounts, CompanyID);




                dsAgingReports ds = new dsAgingReports();

                
                if (dt != null && dt.Rows.Count > 0)
                {
                    for (int i = 0; i < dt.Rows.Count; i++)
                    {
                        ds.AgingReports.Rows.Add();
                        ds.AgingReports.Rows[i]["Index"] = Simulate.Integer32(i+1);
                        ds.AgingReports.Rows[i]["ID"] = Simulate.String(dt.Rows[i]["ID"]);
                        ds.AgingReports.Rows[i]["BBAName"] = Simulate.String(dt.Rows[i]["AName"]);
                        ds.AgingReports.Rows[i]["Date1"] = Simulate.Currency_format(dt.Rows[i]["Date1"]);
                        ds.AgingReports.Rows[i]["Date2"] = Simulate.Currency_format(dt.Rows[i]["Date2"]);
                        ds.AgingReports.Rows[i]["Date3"] = Simulate.Currency_format(dt.Rows[i]["Date3"]);
                        ds.AgingReports.Rows[i]["Date4"] = Simulate.Currency_format(dt.Rows[i]["Date4"]);
                        ds.AgingReports.Rows[i]["Date5"] = Simulate.Currency_format(dt.Rows[i]["Date5"]);
                        ds.AgingReports.Rows[i]["Date6"] = Simulate.Currency_format(dt.Rows[i]["Date6"]);
                        ds.AgingReports.Rows[i]["Date7"] = Simulate.Currency_format(dt.Rows[i]["BalanceTodate"]);

                    }
                }
               


                FastReport.Report report = new FastReport.Report();


              
                string MyPath = getMyPath("rptAging", CompanyID);
                report.Load(MyPath);
                report.RegisterData(ds);
                report.SetParameterValue("report.Date", (date6).ToString("yyyy-MM-dd"));
                report.SetParameterValue("report.Date1",    (date6 - date1).TotalDays);
                report.SetParameterValue("report.Date2",   Simulate.String( (date6 - date2).TotalDays));
                report.SetParameterValue("report.Date3",   (date6 - date3).TotalDays);
                report.SetParameterValue("report.Date4",   (date6 - date4).TotalDays);
                report.SetParameterValue("report.Date5",   (date6-date5).TotalDays );
                report.SetParameterValue("report.Date6", "0");
                report.SetParameterValue("report.Factor", "يوم");
                //    report.SetParameterValue("report.TotalDueToWord", Simulate.String(clsConvertNumberToString.NoToTxt(Simulate.Val(Math.Abs(TotalDue)))));

                //   report.SetParameterValue("report.AmountWithOutDecimal", Simulate.String(AmountWithOutDecimal));
                //   report.SetParameterValue("report.AmountDecimal", Simulate.String(AmountDecimal));
                //    report.SetParameterValue("report.AmountToWord", Simulate.String(AmountToWord));
                //   report.SetParameterValue("report.DueDate", (Simulate.StringToDate(dtHeader.Rows[0]["VoucherDate"]).AddMonths(4)).ToString("yyyy-MM-dd"));

                //   report.SetParameterValue("report.TotalDue", Simulate.Currency_format(TotalDue));
                FastreportStanderdParameters(report, UserID, CompanyID);


                report.Prepare();

                return FastreporttoPDF(report);



            }
            catch (Exception ex)
            {

                return Json(ex);
            }

        }
        [HttpGet]
        [Route("SelectBusinessPartnerBalances")]
        public string SelectBusinessPartnerBalances(DateTime Date, string Accounts, int UserID, int CompanyID)
        {
            try
            {
                clsReports clsReports = new clsReports();
                DataTable dt = clsReports.SelectBusinessPartnerBalances(Date, Accounts, CompanyID);
                if (dt != null)
                {

                    string JSONString = string.Empty;
                    JSONString = JsonConvert.SerializeObject(dt);
                    return JSONString;
                }
                else
                {

                    return "";
                }
            }
            catch (Exception)
            {

                throw;
            }




        }
        [HttpGet]
        [Route("SelectBusinessPartnerBalancesPDF")]
        public IActionResult SelectBusinessPartnerBalancesPDF(DateTime Date , string Accounts, int UserID, int CompanyID)
        {
            try
            {


                FastReport.Utils.Config.WebMode = true;
                clsReports clsReports = new clsReports();
                DataTable dt = clsReports.SelectBusinessPartnerBalances(Date,  Accounts, CompanyID);




                dsBusinessPartnerReports ds = new dsBusinessPartnerReports();


                if (dt != null && dt.Rows.Count > 0)
                {
                    for (int i = 0; i < dt.Rows.Count; i++)
                    {
                        ds.BusinessPartnerReports.Rows.Add();
                        ds.BusinessPartnerReports.Rows[i]["Index"] = Simulate.Integer32(i + 1);
                        ds.BusinessPartnerReports.Rows[i]["ID"] = Simulate.String(dt.Rows[i]["ID"]);
                        ds.BusinessPartnerReports.Rows[i]["BBAName"] = Simulate.String(dt.Rows[i]["AName"]);
                        ds.BusinessPartnerReports.Rows[i]["AccountAName"] = Simulate.String(dt.Rows[i]["AccountAName"]);
                        ds.BusinessPartnerReports.Rows[i]["Total"] = Simulate.Currency_format(dt.Rows[i]["Total"]);
                        ds.BusinessPartnerReports.Rows[i]["Due"] = Simulate.Currency_format(dt.Rows[i]["Due"]);
                        

                    }
                }



                FastReport.Report report = new FastReport.Report();


                 string MyPath = getMyPath("rptBusinessPartnerReports", CompanyID);
                report.Load(MyPath);
                report.RegisterData(ds);
                //report.SetParameterValue("report.Date", (date6).ToString("yyyy-MM-dd"));
                //report.SetParameterValue("report.Date1", (date6 - date1).TotalDays);
                //report.SetParameterValue("report.Date2", Simulate.String((date6 - date2).TotalDays));
   
                report.SetParameterValue("report.Date", (DateTime.Now).ToString("yyyy-MM-dd"));
          
                //    report.SetParameterValue("report.TotalDueToWord", Simulate.String(clsConvertNumberToString.NoToTxt(Simulate.Val(Math.Abs(TotalDue)))));

                //   report.SetParameterValue("report.AmountWithOutDecimal", Simulate.String(AmountWithOutDecimal));
                //   report.SetParameterValue("report.AmountDecimal", Simulate.String(AmountDecimal));
                //    report.SetParameterValue("report.AmountToWord", Simulate.String(AmountToWord));
                //   report.SetParameterValue("report.DueDate", (Simulate.StringToDate(dtHeader.Rows[0]["VoucherDate"]).AddMonths(4)).ToString("yyyy-MM-dd"));

                //   report.SetParameterValue("report.TotalDue", Simulate.Currency_format(TotalDue));
                FastreportStanderdParameters(report, UserID, CompanyID);


                report.Prepare();

                return FastreporttoPDF(report);



            }
            catch (Exception ex)
            {

                return Json(ex);
            }

        }




        //        [HttpGet]
        //        [Route("SelectFinancingReportRoyalJordanian1XLS")]
        //        public ActionResult SelectFinancingReportRoyalJordanian1XLS(int BranchID, int CompanyID, string users, DateTime Date1, DateTime Date2)
        //        {
        //            try
        //            {
        //                string a = @"select tbl_BusinessPartner.EmpCode as employee_number,
        //'2001-08-03' as effective_start_date,
        //'TPT Deductions' as element_name
        //,'1' as cost_segment1
        //,'D010' as cost_segment2
        //,'116003' as cost_segment3
        //, '0' as cost_segment4
        //,'Actual Source Of Deduction' as input_name1
        //,'Jordan Islamic Bank' as input_value1
        //,'Source' as input_name2
        //,'Jordan Islamic Bank/Khrebet Alsouq-Ajwaa Alordon Ass. (Sales)' as input_value2
        //,'Source Amount In JOD' as input_name3
        //,0 as input_value3
        //,'Monthly Installment' as input_name4
        //,0 as input_value4
        //,'Comment' as input_name5
        //,'Mobile' as input_value5
        //,'' as conc


        //from tbl_FinancingHeader 
        //inner join tbl_LoanTypes 
        //on tbl_LoanTypes.ID =  tbl_FinancingHeader.LoanType
        //inner join tbl_BusinessPartner 
        //on tbl_BusinessPartner.ID =  tbl_FinancingHeader.BusinessPartnerID";







        //                clsCompany clsCompany = new clsCompany();
        //                DataTable dtCompany = clsCompany.SelectCompany(CompanyID, "", "", "");
        //                clsBranch clsBranch = new clsBranch();

        //                DataTable dtBranch = clsBranch.SelectBranch(BranchID, "", "", 0);

        //                FastReport.Utils.Config.WebMode = true;
        //                clsFinancingHeader clsFinancingHeader = new clsFinancingHeader();
        //                DataTable dt = clsFinancingHeader.SelectFinancingReport(Date1, Date2, Simulate.String(users), BranchID, CompanyID);

        //                dsFinancingReport ds = new dsFinancingReport();
        //                ds.DataTableH.Rows.Add();
        //                ds.DataTableH.Rows[0]["Date1"] = Date1;
        //                ds.DataTableH.Rows[0]["Date2"] = Date2;
        //                if (dtCompany != null && dtCompany.Rows.Count > 0)
        //                {

        //                    ds.DataTableH.Rows[0]["CompanyName"] = dtCompany.Rows[0]["AName"];

        //                }
        //                if (dtBranch != null && dtBranch.Rows.Count == 1)
        //                {

        //                    ds.DataTableH.Rows[0]["BranchName"] = dtBranch.Rows[0]["AName"];

        //                }
        //                else
        //                {
        //                    ds.DataTableH.Rows[0]["BranchName"] = "All";

        //                }
        //                if (dt != null && dt.Rows.Count > 0)
        //                {
        //                    for (int i = 0; i < dt.Rows.Count; i++)
        //                    {
        //                        ds.DataTableD.Rows.Add();

        //                        ds.DataTableD.Rows[i]["Index"] = i + 1;
        //                        ds.DataTableD.Rows[i]["Customer"] = dt.Rows[i]["businessPartnerAName"];

        //                        ds.DataTableD.Rows[i]["Total"] = dt.Rows[i]["FinancingAmount"];
        //                        ds.DataTableD.Rows[i]["Price"] = dt.Rows[i]["FinancingAmount"];
        //                        ds.DataTableD.Rows[i]["QTY"] = 1;
        //                        ds.DataTableD.Rows[i]["Descrption"] = Simulate.String(dt.Rows[i]["Description"]);

        //                    }
        //                }

        //                FastReport.Web.WebReport report = new FastReport.Web.WebReport();
        //                report.Report.RegisterData(ds);


        //                string MyPath = ($"{Environment.CurrentDirectory}" + @"\Reports\rptFinancingReport.frx");
  //      string MyPath = getMyPath("rptGift", CompanyID);
        //                report.Report.Load(MyPath);


        //                report.Report.Prepare();

        //                return Fastreporttoxls(ds.DataTableD);
        //            }
        //            catch (Exception)
        //            {

        //                throw;
        //            }
        //        }
        #endregion
        #region Subscriptions
        [HttpGet]
        [Route("SelectSubscriptions")]
        public string SelectSubscriptions(int Id, int BusinessPartnerID,
            int SubscriptionTypeID, int TransactionStatusID, int CompanyID)
        {
            try
            {
                clsSubscriptions clsSubscriptions = new clsSubscriptions();
                DataTable dt = clsSubscriptions.SelectSubscriptions( Id,  BusinessPartnerID,
             SubscriptionTypeID,  TransactionStatusID,  CompanyID);
                if (dt != null)
                {

                    string JSONString = string.Empty;
                    JSONString = JsonConvert.SerializeObject(dt);
                    return JSONString;
                }
                else
                {

                    return "";
                }
            }
            catch (Exception)
            {

                throw;
            }




        }

        [HttpGet]
        [Route("SelectSubscriptionsStatus")]
        public string SelectSubscriptionsStatus(int ID,   int CompanyID)
        {
            try
            {
                clsSubscriptionsStatus clsSubscriptionsStatus = new clsSubscriptionsStatus();

         
                DataTable dt = clsSubscriptionsStatus.SelectSubscriptionsStatus(ID,CompanyID);
                if (dt != null)
                {
                    string JSONString = string.Empty;
                    JSONString = JsonConvert.SerializeObject(dt);
                    return JSONString;
                }
                else
                {
                    return "";
                }
            }
            catch (Exception)
            {

                throw;
            }
        }
        [HttpGet]
        [Route("SelectSubscriptionsTypes")]
        public string SelectSubscriptionsTypes(int ID, int CompanyID)
        {
            try
            {
                clsSubscriptionsTypes clsSubscriptionsTypes = new clsSubscriptionsTypes();
                  DataTable dt = clsSubscriptionsTypes.SelectSubscriptionsTypes(ID,CompanyID);
                if (dt != null)
                {
                    string JSONString = string.Empty;
                    JSONString = JsonConvert.SerializeObject(dt);
                    return JSONString;
                }
                else
                {
                    return "";
                }
            }
            catch (Exception)
            {

                throw;
            }
        }
        [HttpGet]
        [Route("DeleteSubscriptionsByID")]
        public bool DeleteSubscriptionsByID(int ID)
        {
            try
            {

                clsSubscriptions clsSubscriptions = new clsSubscriptions();
                bool A = clsSubscriptions.DeleteSubscriptionsByID(ID);
                return A;
            }
            catch (Exception)
            {

                throw;
            }

        }
        [HttpGet]
        [Route("InsertSubscriptions")]
        public int InsertSubscriptions(int BusinessPartnerID,
            int SubscriptionTypeID,
            DateTime TransactionDate,
            int TransactionStatusID,
            double Amount,
            int CompanyID,
            int CreationUserId)
        {
            try
            {
                clsSubscriptions clsSubscriptions = new clsSubscriptions();
                int A = clsSubscriptions.InsertSubscriptions(
                    BusinessPartnerID,
              SubscriptionTypeID,
              TransactionDate,
              TransactionStatusID,
              Amount,
              CompanyID,
              CreationUserId);
                return A;
            }
            catch (Exception ex)
            {

                throw;
            }

        }
        [HttpPost]
        [Route("importTest")]
        public string importTest(int companyID, [FromBody] string Logo ) {
            try
            {
                byte[] myLogo = new Byte[64];
                if (Logo != null && Logo.Length > 0)
                {
                    myLogo = Convert.FromBase64String(Logo);
                }
                else
                {
                    myLogo = null;
                }


                DataTable dt;
                using (MemoryStream stream = new MemoryStream(myLogo))
                {
                    XmlDocument xmlDcoument = new XmlDocument();
                    xmlDcoument.Load(stream);
                    XmlNodeList? xmlNodeList = xmlDcoument.DocumentElement.ChildNodes;
                      dt = ConvertXmlNodeListToDataTable(xmlNodeList);
                   

                    //using (XLWorkbook workbook = new XLWorkbook(stream))
                    //{
                    //    // Access the Excel workbook, worksheets, cells, etc.
                    //    // For example, you can read data from the Excel file:
                    //    var worksheet = workbook.Worksheet(1);
                    //    string cellValue = worksheet.Cell("A1").Value.ToString();
                    //  //  Console.WriteLine("Cell A1 value: " + cellValue);
                    //}
                }
                clsBusinessPartner cls = new clsBusinessPartner();
                DataTable dtBP = cls.SelectBusinessPartner(0, 0, "", "", -1, companyID);
                clsLoanTypes clsLoanTypes = new clsLoanTypes();
                DataTable dtLoanType = clsLoanTypes.SelectLoanTypes(0, "-1,0,1,2,3", "", "", "", 0);
                dt.Columns.Add("ID");
                dt.Columns.Add("AName");
                dt.Columns.Add("TransactionTypeID");
                dt.Columns.Add("TransactionTypeName");
                dt.Columns.Add("IsActiveSubscription");
               
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    DataRow[] filteredRows = dtBP.Select("EmpCode LIKE '%" + dt.Rows[i]["EMP_NO"] + "%'");
                    if (filteredRows.Length > 0) {
                        dt.Rows[i]["ID"] = filteredRows[0]["ID"];
                        dt.Rows[i]["AName"] = filteredRows[0]["AName"];
                    }
                    DataRow[] filteredRowsLoan = dtLoanType.Select("Code LIKE '%" + dt.Rows[i]["SOURCE1"] + "%'");
                    if (filteredRowsLoan.Length > 0)
                    {
                        dt.Rows[i]["TransactionTypeID"] = filteredRowsLoan[0]["ID"];
                        dt.Rows[i]["TransactionTypeName"] = filteredRowsLoan[0]["AName"];
                    }
                }



                if (dt != null)
                {
                    string JSONString = string.Empty;
                    JSONString = JsonConvert.SerializeObject(dt);
                    return JSONString;
                }
                else
                {
                    return "";
                }
            }
            catch (Exception ex)
            {

                throw;
            }
        }
        public static DataTable ConvertXmlNodeListToDataTable(XmlNodeList xnl)
        {
            DataTable dt = new DataTable();
            int TempColumn = 0;
            

            for (int i = 0; i < xnl.Item(0).ChildNodes[0].ChildNodes.Count; i++)
            {
                
                TempColumn++;
                DataColumn dc = new DataColumn(xnl.Item(0).ChildNodes[0].ChildNodes[i].Name, System.Type.GetType("System.String"));
                if (dt.Columns.Contains(xnl.Item(0).ChildNodes[0].ChildNodes[i].Name))
                {
                    dt.Columns.Add(dc.ColumnName = dc.ColumnName + TempColumn.ToString());
                }
                else
                {
                    dt.Columns.Add(dc);
                    }
                }



            int ColumnsCount = dt.Columns.Count;
            for (int i = 0; i < xnl.Item(0).ChildNodes.Count; i++)
            {
                DataRow dr = dt.NewRow();
                for (int j = 0; j < ColumnsCount; j++)
                {
                    dr[j] = xnl.Item(0).ChildNodes[i].ChildNodes[j].InnerText;
                }
                dt.Rows.Add(dr);
            }
            return dt;
        }
        #endregion
        #region ReportingTypeNodes

        [HttpGet]
        [Route("SelectReportingTypeNodesByID")]
        public string SelectReportingTypeNodesByID(int ID, int ParentID,int ReportingTypeID, int CompanyID)
        {
            try
            {
                clsReportingTypeNodes clsReportingTypeNodes = new clsReportingTypeNodes();
                DataTable dt = clsReportingTypeNodes.SelectReportingTypeNodesByID(ID, ParentID, ReportingTypeID, CompanyID);
                if (dt != null)
                {

                    string JSONString = string.Empty;
                    JSONString = JsonConvert.SerializeObject(dt);
                    return JSONString;
                }
                else
                {

                    return "";
                }
            }
            catch (Exception)
            {

                throw;
            }




        }
        [HttpGet]
        [Route("DeleteReportingTypeNodesByID")]
        public bool DeleteReportingTypeNodesByID(int ID)
        {
            try
            {

                clsReportingTypeNodes clsReportingTypeNodes = new clsReportingTypeNodes();
                bool A = clsReportingTypeNodes.DeleteReportingTypeNodesByID(ID);
                return A;
            }
            catch (Exception)
            {

                throw;
            }

        }
        [HttpGet]
        [Route("InsertReportingTypeNodes")]
        public int InsertReportingTypeNodes(string AName, string EName, int ReportingTypeID, int ParentID, int CompanyID, int CreationUserId)
        {
            try
            {
                clsReportingTypeNodes clsReportingTypeNodes = new clsReportingTypeNodes();
                int A = clsReportingTypeNodes.InsertReportingTypeNodes(AName, EName, ReportingTypeID, ParentID, CompanyID, CreationUserId);
                return A;
            }
            catch (Exception ex)
            {

                throw;
            }

        }
        [HttpGet]
        [Route("UpdateReportingTypeNodes")]
        public int UpdateReportingTypeNodes(int ID, string AName, string EName, int ReportingTypeID, int ParentID, int ModificationUserId)
        {
            try
            {
                clsReportingTypeNodes clsReportingTypeNodes = new clsReportingTypeNodes();
                int A = clsReportingTypeNodes.UpdateReportingTypeNodes(ID, AName, EName, ReportingTypeID, ParentID, ModificationUserId);
                return A;
            }
            catch (Exception)
            {

                throw;
            }

        }

        #endregion
    }
}

 