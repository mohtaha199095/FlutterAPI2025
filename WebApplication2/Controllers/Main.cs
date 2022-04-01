using FastReport.Export.PdfSimple;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using WebApplication2.cls;
using WebApplication2.cls.Reports;
using WebApplication2.DataBaseTable;
using WebApplication2.DataSet;

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
                DataTable dt = clsJournalVoucherDetails.SelectJournalVoucherDetailsByParentId(ParentGuid, AccountID, SubAccountID);
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

                    DataTable dt = clsJournalVoucherDetails.SelectJournalVoucherDetailsByParentId("", ID, 0);
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
                return msg;

            }
            catch (Exception)
            {

                throw;
            }

        }
        [HttpGet]
        [Route("InsertAccounts")]
        public int InsertAccounts(int ParentID, string AccountNumber, string AName, string EName, int ReportingTypeID, int AccountNatureID, int CompanyID, int CreationUserId)
        {
            try
            {
                clsAccounts clsAccounts = new clsAccounts();
                int A = clsAccounts.InsertAccounts(ParentID, AccountNumber, AName, EName, ReportingTypeID, AccountNatureID, CompanyID, CreationUserId);
                return A;
            }
            catch (Exception ex)
            {

                throw;
            }

        }
        [HttpGet]
        [Route("UpdateAccounts")]
        public int UpdateAccounts(int ID, int ParentID, string AccountNumber, string AName, string EName, int ReportingTypeID, int AccountNatureID, int ModificationUserId)
        {
            try
            {
                clsAccounts clsAccounts = new clsAccounts();
                int A = clsAccounts.UpdateAccounts(ID, ParentID, AccountNumber, AName, EName, ReportingTypeID, AccountNatureID, ModificationUserId);
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
        public string SelectBusinessPartner(int ID, int Type, int CompanyID)
        {
            try
            {
                clsBusinessPartner clsBusinessPartner = new clsBusinessPartner();
                DataTable dt = clsBusinessPartner.SelectBusinessPartner(ID, Type, "", "", CompanyID);
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
            string Email, int Type, int CompanyID, int CreationUserId)
        {
            try
            {
                clsBusinessPartner clsBusinessPartner = new clsBusinessPartner();
                int A = clsBusinessPartner.InsertBusinessPartner(Simulate.String(AName), Simulate.String(EName), Simulate.String(CommercialName)
                    , Simulate.String(Address), Simulate.String(Tel), Simulate.Bool(Active), Simulate.Val(Limit),
             Simulate.String(Email), Type, CompanyID, CreationUserId);
                return A;
            }
            catch (Exception ex)
            {

                throw;
            }

        }
        [HttpGet]
        [Route("UpdateBusinessPartner")]
        public int UpdateBusinessPartner(int ID, string AName, string EName, string CommercialName, string Address, string Tel, string Active, string Limit,
            string Email, int Type, int ModificationUserId)
        {
            try
            {
                clsBusinessPartner clsBusinessPartner = new clsBusinessPartner();
                int A = clsBusinessPartner.UpdateBusinessPartner(ID, Simulate.String(AName), Simulate.String(EName), Simulate.String(CommercialName)
                    , Simulate.String(Address), Simulate.String(Tel), Simulate.Bool(Active), Simulate.Val(Limit),
            Simulate.String(Email), Type, ModificationUserId);
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
        private void FastreportStanderdParameters(FastReport.Report Report, int UserID, int CompantID)
        {
            clsCompany clsCompany = new clsCompany();
            DataTable dt = clsCompany.SelectCompany(CompantID, "", "", "");
            if (dt != null)
            {

                Report.SetParameterValue("Standerd.CompanyName", Simulate.String(dt.Rows[0]["AName"]));
                Report.SetParameterValue("Standerd.Address", Simulate.String(dt.Rows[0]["Address"]));
                FastReport.PictureObject Logo = (FastReport.PictureObject)Report.FindObject("CompanyLogo");
                if (Simulate.String(dt.Rows[0]["Logo"]) != "")
                    Logo.Image = Simulate.StringToImg((byte[])dt.Rows[0]["Logo"]);
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


                string MyPath = ($"{Environment.CurrentDirectory}" + @"\Reports\rptTrialBalance.frx");

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
        public string SelectAccountStatement(DateTime Date1, DateTime Date2, int BranchID, int CostCenterID, int Accountid, int subAccountid, int CompanyID)
        {
            try
            {
                clsReports clsReports = new clsReports();
                DataTable dt = clsReports.SelectAccountStatement(Date1, Date2, BranchID, CostCenterID, Accountid, subAccountid, CompanyID);
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
        public IActionResult SelectAccountStatementPDF(DateTime Date1, DateTime Date2, int BranchID, int CostCenterID, int Accountid, int subAccountid, int UserID, int CompanyID)
        {
            try
            {

                FastReport.Utils.Config.WebMode = true;
                clsReports clsReports = new clsReports();
                DataTable dt = clsReports.SelectAccountStatement(Date1, Date2, BranchID, CostCenterID, Accountid, subAccountid, CompanyID);

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


                string MyPath = ($"{Environment.CurrentDirectory}" + @"\Reports\rptAccountStatement.frx");

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
                        DataTable dtSubAccount = clsBusinessPartner.SelectBusinessPartner(subAccountid, 0, "", "", CompanyID);
                        if (dtSubAccount != null && dtSubAccount.Rows.Count > 0)
                        {
                            SubAccountName = " / " + Simulate.String(dtSubAccount.Rows[0]["EName"]);
                        }

                    }
                    string subAccountIdString = "";
                    if (subAccountid > 0)
                    {
                        subAccountIdString = " / " + subAccountid;
                    }
                    report.SetParameterValue("report.AccountName", Simulate.String(dtAccount.Rows[0]["EName"]) + SubAccountName);
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


                string MyPath = ($"{Environment.CurrentDirectory}" + @"\Reports\rptAccountStatement.frx");

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


                string MyPath = ($"{Environment.CurrentDirectory}" + @"\Reports\rptAccountStatement.frx");

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


                string MyPath = ($"{Environment.CurrentDirectory}" + @"\Reports\rptAccountStatement.frx");

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
        public bool DeleteCashDrawerByID(int ID)
        {
            try
            {
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

        public string InsertCashVoucherHeader(DateTime voucherDate, int branchID, int costCenterID, int cashID
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
        public string UpdateCashVoucherHeader(DateTime voucherDate, int branchID, int costCenterID, int cashID
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
                int A = clsBanks.InsertBanks(AName, EName, AccountNumber, CompanyID, CreationUserId);
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
                int A = clsBanks.UpdateBanks(ID, AName, EName, AccountNumber, ModificationUserId);
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
    }
}
