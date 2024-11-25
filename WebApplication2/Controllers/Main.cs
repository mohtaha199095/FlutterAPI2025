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
using System.Linq;
using System.Text;
using SixLabors.ImageSharp.ColorSpaces;
using Microsoft.VisualBasic;
using DocumentFormat.OpenXml.Wordprocessing;
using DocumentFormat.OpenXml.ExtendedProperties;
using System.Dynamic;
using System.ComponentModel.Design;
using DocumentFormat.OpenXml.Office2010.ExcelAc;
using DocumentFormat.OpenXml.Presentation;
using System.Reflection.Emit;
using System.Text.Json;
using FastReport.Utils.Json;
using FastReport.Export.PdfSimple.PdfCore;
using System.Collections;
using Nancy.ModelBinding.DefaultBodyDeserializers;
using Microsoft.CodeAnalysis.Operations;

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
        public string CheckLogin(string UserName, string Password, int CompanyID)
        {
            try
            {
                string JSONString = string.Empty;
                if (Simulate.String(UserName) == "")
                {
                    return JSONString;

                }
                if (Simulate.String(Password) == "" || CompanyID==0)
                {
                    return JSONString;

                }
                clsEmployee clsEmployee = new clsEmployee();
                DataTable dt = clsEmployee.SelectEmployee(0, "", "", Simulate.String(UserName), Simulate.String(Password), CompanyID, 1);
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
                DataTable dt = clsEmployee.SelectEmployee(ID, "", "", Simulate.String(UserName), Simulate.String(Password), CompanyId,-1);
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
        [Route("SelectEmployeesByBranchAccess")]
        public string SelectEmployeesByBranchAccess(int UserID, int CompanyId)
        {
            try
            {

                SqlParameter[] prm =
                 { 
                    new SqlParameter("@UserID", SqlDbType.Int) { Value = UserID },
                    new SqlParameter("@CompanyID", SqlDbType.Int) { Value = CompanyId },
                };
                string a = @"select * from tbl_employee where id in (
select distinct UserID from tbl_UserAuthorizationModels where  CompanyID = @CompanyID and IsAccess=1 and TypeID=1 and
ModelID in (select ModelID from tbl_UserAuthorizationModels where CompanyID = @CompanyID and IsAccess=1 and TypeID=1 and UserID=@UserID )

) ";

                clsSQL cls = new  clsSQL();

                DataTable dt = cls.ExecuteQueryStatement(a, cls.CreateDataBaseConnectionString(CompanyId), prm);


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
        public bool DeleteEmployeeByID(int ID,int CompanyID)
        {
            try
            {
                clsJournalVoucherDetails clsJournalVoucherDetails = new clsJournalVoucherDetails();
                DataTable dt = clsJournalVoucherDetails.SelectJournalVoucherDetailsByParentId("", 0, 0, 0, 0, ID, CompanyID);
                if (dt != null && dt.Rows.Count > 0)
                {

                    return false;
                }
                clsEmployee clsEmployee = new clsEmployee();
                bool A = clsEmployee.DeleteEmployeeByID(ID, CompanyID);
                return A;
            }
            catch (Exception)
            {

                throw;
            }

        }
        [HttpPost]
        [Route("InsertEmployee")]
        public int InsertEmployee([FromBody] JsonElement data, string AName, string EName, string UserName, string Password, int CompanyID, int CreationUserId, bool IsSystemUser)
        {
            try
            {
               
                  var SignutureText = data.GetProperty("Signuture").GetString();
                byte[] Signuturea = new Byte[64];
                if (SignutureText != null && SignutureText.Length > 0)
                {
                    Signuturea = Convert.FromBase64String(SignutureText);
                }



                clsEmployee clsEmployee = new clsEmployee();
                int A = clsEmployee.InsertEmployee(Simulate.String(AName), Simulate.String(EName), Simulate.String(UserName), Simulate.String(Password), Simulate.Integer32(CompanyID), CreationUserId,  IsSystemUser, Signuturea);
                return A;
            }
            catch (Exception ex)
            {

                throw;
            }

        }
        [HttpPost]
        [Route("UpdateEmployee")]
        public int UpdateEmployee([FromBody] JsonElement data, string AName, string EName, string UserName, string Password, int ID, int ModificationUserId, bool IsSystemUser,int CompanyID)
        {
            try
            {
                
                  var SignutureText = data.GetProperty("Signuture").GetString();
                byte[] Signuturea = new Byte[64];
                if (SignutureText != null && SignutureText.Length > 0)
                {
                    Signuturea = Convert.FromBase64String(SignutureText);
                }

                clsEmployee clsEmployee = new clsEmployee();
                int A = clsEmployee.UpdateEmployee(Simulate.String(AName), Simulate.String(EName), Simulate.String(UserName), Simulate.String(Password), ID, ModificationUserId,  IsSystemUser, Signuturea, CompanyID);
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
        public string SelectCompanyByID(int ID,string Phone,string PartOfTheName)
        {
            try
            {
                clsCompany clsCompany = new clsCompany();
                DataTable dt = clsCompany.SelectCompany(ID, "", "", Phone, ID, PartOfTheName);
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
        public bool DeleteCompanyByID(int ID,int CompanyID)
        {
            try
            {
                clsCompany clsCompany = new clsCompany();
                bool A = clsCompany.DeleteCompanyByID(ID, CompanyID);
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
                SqlConnection con = new SqlConnection(clsSQL.MainDataBaseconString);
                con.Open();
                trn = con.BeginTransaction();
                int A = 0;
                bool IsSaved = true;
                try
                {

                    clsDataBaseVersion ClsDataBaseVersion = new clsDataBaseVersion();
                    ClsDataBaseVersion.CreateDataBase(UserName+ Tel1);

                    A = clsCompany.InsertCompany(Simulate.String(AName), Simulate.String(EName), Simulate.String(Email)
            , Simulate.String(Address), Simulate.String(Tel1), Simulate.String(Tel2), Simulate.String(ContactPerson),
              Simulate.String(ContactNumber), myLogo, Simulate.String(TradeName),Simulate.String( UserName )+ Simulate.String( Tel1));
                    if (A > 0)
                    {
                        clsEmployee clsEmployee = new clsEmployee();
                        byte[] Signuture = new byte[0];
                        int b = clsEmployee.InsertEmployee(Simulate.String(AName), Simulate.String(EName), Simulate.String(UserName), Simulate.String(Password), A, 0,true, Signuture);
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
             Simulate.String(ContactNumber), myLogo, Simulate.String(TradeName), ModificationUserId,ID);
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
        public bool DeleteBranchByID(int ID, int CompanyID)
        {
            try
            {
                clsJournalVoucherDetails clsJournalVoucherDetails = new clsJournalVoucherDetails();
                DataTable dt = clsJournalVoucherDetails.SelectJournalVoucherDetailsByParentId("", 0,0, ID, 0, 0,  CompanyID); 
                if (dt != null && dt.Rows.Count > 0)
                {

                    return false;
                }
                clsBranch clsBranch = new clsBranch();
                bool A = clsBranch.DeleteBranchByID(ID, CompanyID);
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
        public int UpdateBranch(int ID, string AName, string EName, int ModificationUserId, int CompanyID)
        {
            try
            {
                clsBranch clsBranch = new clsBranch();
                int A = clsBranch.UpdateBranch(ID, AName, EName, ModificationUserId, CompanyID);
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
        public bool DeleteCostCenterByID(int ID, int CompanyID)
        {
            try
            {
                clsJournalVoucherDetails clsJournalVoucherDetails = new clsJournalVoucherDetails();
                DataTable dt = clsJournalVoucherDetails.SelectJournalVoucherDetailsByParentId("", 0, 0, 0,  ID, 0, CompanyID);
                if (dt != null && dt.Rows.Count > 0)
                {

                    return false;
                }
                clsCostCenter clsCostCenter = new clsCostCenter();
                bool A = clsCostCenter.DeleteCostCenterByID(ID, CompanyID);
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
        public int UpdateCostCenter(int ID, string AName, string EName, int ModificationUserId,int CompanyID)
        {
            try
            {
                clsCostCenter clsCostCenter = new clsCostCenter();
                int A = clsCostCenter.UpdateCostCenter(ID, AName, EName, ModificationUserId,CompanyID);
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
        public bool DeleteItemsCategoryByID(int ID,int CompanyID)
        {
            try
            {
                clsItemsCategory clsItemsCategory = new clsItemsCategory();
                bool A = clsItemsCategory.DeleteItemsCategoryByID(ID, CompanyID);
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
        public int UpdateItemsCategory(int ID, string AName, string EName, int ModificationUserId, int CompanyID)
        {
            try
            {
                clsItemsCategory clsItemsCategory = new clsItemsCategory();
                int A = clsItemsCategory.UpdateItemsCategory(ID, AName, EName, ModificationUserId,CompanyID);
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
        public bool DeleteJournalVoucherHeadersByID(string Guid,int CompanyID)
        {
            try
            {
                clsJournalVoucherDetails clsJournalVoucherDetails = new clsJournalVoucherDetails();
                clsJournalVoucherHeader clsJournalVoucherHeader = new clsJournalVoucherHeader();

                SqlTransaction trn; clsSQL clsSQL = new clsSQL();
                SqlConnection con = new SqlConnection(clsSQL.CreateDataBaseConnectionString(CompanyID));
                con.Open();
                trn = con.BeginTransaction(); int A = 0;
                bool IsSaved = true;
                try
                {
                    IsSaved = clsJournalVoucherHeader.DeleteJournalVoucherHeaderByID(Guid, CompanyID, trn);
                    bool a = clsJournalVoucherDetails.DeleteJournalVoucherDetailsByParentId(Guid,CompanyID, trn);
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

        public string InsertJournalVoucherHeader(int BranchID, int CostCenterID, string Notes, string JVNumber, int JVTypeID, [FromBody] string DetailsList, int CompanyID, DateTime VoucherDate, int CreationUserId,string RelatedFinancingHeaderGuid = "",int RelatedLoanTypeID=0)

        {
            try
            {

                List<tbl_JournalVoucherDetails> details = JsonConvert.DeserializeObject<List<tbl_JournalVoucherDetails>>(DetailsList);
                clsJournalVoucherHeader clsJournalVoucherHeader = new clsJournalVoucherHeader();
                clsJournalVoucherDetails clsDetails = new clsJournalVoucherDetails();
                SqlTransaction trn; clsSQL clsSQL = new clsSQL();
                SqlConnection con = new SqlConnection(clsSQL.CreateDataBaseConnectionString(CompanyID));
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
                     
                        A = clsJournalVoucherHeader.InsertJournalVoucherHeader(BranchID, CostCenterID, Simulate.String(Notes), JVNumber, JVTypeID, CompanyID, VoucherDate, CreationUserId, RelatedFinancingHeaderGuid, RelatedLoanTypeID, trn);
                    if (A == "") IsSaved = false;
                    for (int i = 0; i < details.Count; i++)
                    {
                        string c = clsDetails.InsertJournalVoucherDetails(A, i, details[i].AccountID, details[i].SubAccountID, details[i].Debit, details[i].Credit
                              , details[i].Total, details[i].BranchID, details[i].CostCenterID, details[i].DueDate, details[i].Note, details[i].CompanyID
                              , details[i].CreationUserID, details[i].RelatedDetailsGuid, trn);
                        if (c == "")
                            IsSaved = false;
                    }

                    if (!clsJournalVoucherHeader.CheckJVMatch(A, CompanyID,trn))
                    {
                        IsSaved = false;
                        A = "";
                    }



                   
                        if (JVTypeID== 16) { 
                        for (int i = 0; i < details.Count; i++)
                        {
                            if (details[i].SubAccountID > 0) {
                             var aaaa=    ReconcileByType(details[i].AccountID, details[i].SubAccountID,RelatedLoanTypeID,CompanyID,CreationUserId,trn);
                                    if (!aaaa) {
                                    IsSaved = false;
                                    }
                                
                                }
                        }
                        }
                    if (IsSaved)
                    {
                        trn.Commit();
                    }
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
        bool ReconcileByType(int AccountID, int SubAccountID,int RelatedLoanTypeID, int CompanyID, int  CreationUserId, SqlTransaction trn) {
            try
            {
   

                int VoucherNumber = 0;
                clsReconciliation clsReconciliation = new clsReconciliation();
                DataTable maxDT = clsReconciliation.SelectReconciliationMaxNumber(CompanyID, trn);

                if (maxDT != null && maxDT.Rows.Count > 0)
                {
                    VoucherNumber = 1 + Simulate.Integer32(maxDT.Rows[0][0]);
                }
                else
                {
                    VoucherNumber = 1;
                }
             
                 DataTable dt = clsReconciliation.SelectAccountsForAutoReconciliation(AccountID, SubAccountID, CompanyID,RelatedLoanTypeID,trn);
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
                    if (SubAccountID == 3159)
                    {
                        var aaaa = "h";

                    }
                    for (int i = 0; i < dt.Rows.Count; i++)
                    {
                        if (TotalDebit>0&&TotalDebit >= TotalCredit && Simulate.Val(dt.Rows[i]["Credit"]) > 0)
                        {
                            if (Simulate.decimal_(dt.Rows[i]["Total"])!=0 ) { 
                            var a = clsReconciliation.InsertReconciliation(VoucherNumber, Simulate.String(dt.Rows[i]["Guid"]), Simulate.decimal_(dt.Rows[i]["Total"]), CompanyID, CreationUserId, Simulate.String(dt.Rows[i]["Guid"]), trn);
                                if (a == "")
                                {
                                    isSaved = false;
                                }
                            }

                        }
                        else if (TotalCredit>0&&TotalCredit >= TotalDebit && Simulate.Val(dt.Rows[i]["Debit"]) > 0)
                        {
                            if (Simulate.decimal_(dt.Rows[i]["Total"]) != 0)
                            {
                                var a = clsReconciliation.InsertReconciliation(VoucherNumber, Simulate.String(dt.Rows[i]["Guid"]), Simulate.decimal_(dt.Rows[i]["Total"]), CompanyID, CreationUserId, Simulate.String(dt.Rows[i]["Guid"]), trn);
                                if (a == "")
                                {
                                    isSaved = false;
                                }
                            }
                        }

                    }
                    if (TotalCredit > TotalDebit)
                    {
                        double RemainingAmount = TotalDebit;
                        for (int i = 0; i < dt.Rows.Count; i++)
                        {
                            if (Simulate.Val(dt.Rows[i]["Credit"]) > 0)
                            {
                                if (RemainingAmount > Simulate.Val(dt.Rows[i]["Credit"]))
                                {

                                    var a = clsReconciliation.InsertReconciliation(VoucherNumber, Simulate.String(dt.Rows[i]["Guid"]), Simulate.decimal_(dt.Rows[i]["Total"]), CompanyID, CreationUserId, Simulate.String(dt.Rows[i]["Guid"]), trn);
                                    if (a == "")
                                    {
                                        isSaved = false;
                                    }

                                    RemainingAmount = RemainingAmount - Simulate.Val(dt.Rows[i]["Credit"]);
                                }
                                else if (RemainingAmount >= 0)
                                {
                                    var a = clsReconciliation.InsertReconciliation(VoucherNumber, Simulate.String(dt.Rows[i]["Guid"]), Simulate.decimal_(RemainingAmount) * -1, CompanyID, CreationUserId, Simulate.String(dt.Rows[i]["Guid"]), trn);
                                    if (a == "")
                                    {
                                        isSaved = false;
                                    }
                                    RemainingAmount = RemainingAmount - Simulate.Val(dt.Rows[i]["Credit"]);
                                    break;

                                }
                                else
                                {
                                    RemainingAmount = RemainingAmount - Simulate.Val(dt.Rows[i]["Credit"]);
                                    break;
                                }
                            }
                       
                         }

                    }
                    else if (TotalDebit > TotalCredit)
                    {
                       
                        double RemainingAmount = TotalCredit;
                        for (int i = 0; i < dt.Rows.Count; i++)
                        {



                            if (Simulate.Val(dt.Rows[i]["Debit"]) > 0) {
                            if (RemainingAmount >= Simulate.Val(dt.Rows[i]["Debit"]))
                            {

                                var a = clsReconciliation.InsertReconciliation(VoucherNumber, Simulate.String(dt.Rows[i]["Guid"]), Simulate.decimal_(dt.Rows[i]["Total"]), CompanyID, CreationUserId, Simulate.String(dt.Rows[i]["Guid"]), trn);

                                    if (a == "")
                                    {
                                        isSaved = false;
                                    }
                                    RemainingAmount = RemainingAmount - Simulate.Val(dt.Rows[i]["Debit"]);
                            }
                            else if(RemainingAmount>= 0)
                            {
                                var a = clsReconciliation.InsertReconciliation(VoucherNumber, Simulate.String(dt.Rows[i]["Guid"]), Simulate.decimal_(RemainingAmount) , CompanyID, CreationUserId, Simulate.String(dt.Rows[i]["Guid"]), trn);
                                    if (a == "")
                                    {
                                        isSaved = false;
                                    }
                                    RemainingAmount = RemainingAmount - Simulate.Val(dt.Rows[i]["Debit"]);
                                break;

                            }
                            else { RemainingAmount = RemainingAmount - Simulate.Val(dt.Rows[i]["Debit"]);  break; }
                            }
                        }


                    }

                }
                DataTable dt1 = clsReconciliation.SelectReconciliationByJVDetailsGuid(VoucherNumber, "", 0, "00000000-0000-0000-0000-000000000000", trn);
                string sum = dt1.Compute("Sum(Amount)", "").ToString();
                if (Simulate.Val(sum) == 0) { 
                    return true; 
                } else { 
                    return false;
                }
                 
            }
            catch (Exception)
            {
                return false;
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
                SqlConnection con = new SqlConnection(clsSQL.CreateDataBaseConnectionString(CompanyID));
                con.Open();
                trn = con.BeginTransaction();
                string A = "";
                try
                {
                    bool IsSaved = true;

                    A = clsJournalVoucherHeader.UpdateJournalVoucherHeader(BranchID, CostCenterID, Simulate.String(Notes), JVNumber, JVTypeID, VoucherDate, Guid, ModificationUserId, "", 0,CompanyID, trn);

                    if (JVTypeID != 15) { //this condition to not lose the reconcilation with the details 
                    clsDetails.DeleteJournalVoucherDetailsByParentId(Guid,CompanyID, trn);
                    for (int i = 0; i < details.Count; i++)
                    {
                        string c = clsDetails.InsertJournalVoucherDetails(Guid, i, details[i].AccountID, details[i].SubAccountID, details[i].Debit, details[i].Credit
                              , details[i].Total, details[i].BranchID, details[i].CostCenterID, details[i].DueDate, details[i].Note, details[i].CompanyID
                              , details[i].CreationUserID, details[i].RelatedDetailsGuid, trn);
                        if (c == "")
                            IsSaved = false;
                    }
                    }
                    if (!clsJournalVoucherHeader.CheckJVMatch(Guid,CompanyID, trn))
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
        public string SelectJournalVoucherDetailsByParentId(string ParentGuid, int AccountID, int SubAccountID, int CompanyID)
        {
            try
            {
                clsJournalVoucherDetails clsJournalVoucherDetails = new clsJournalVoucherDetails();
                DataTable dt = clsJournalVoucherDetails.SelectJournalVoucherDetailsByParentId(ParentGuid, AccountID, SubAccountID,0, 0, 0, CompanyID);
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
        public string DeleteAccountsByID(int ID,int CompanyID)
        {
            try
            {
                string msg = "Failed to delete this record";
                bool A = false;
                clsJournalVoucherDetails clsJournalVoucherDetails = new clsJournalVoucherDetails();
                clsAccounts clsAccounts = new clsAccounts();
                SqlTransaction trn; clsSQL clsSQL = new clsSQL();
                SqlConnection con = new SqlConnection(clsSQL.CreateDataBaseConnectionString(CompanyID));
                con.Open();
                trn = con.BeginTransaction();
                try
                {

                    DataTable dt = clsJournalVoucherDetails.SelectJournalVoucherDetailsByParentId("", ID, 0,0,0,0, CompanyID);
                    if (dt != null && dt.Rows.Count > 0)
                    {
                        msg = "this account is used , cant delete used account";
                    }
                    else
                    {
                        A = clsAccounts.DeleteAccountsByID(ID, CompanyID); msg = "account deleted successfully";

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
        public int UpdateAccounts(int ID, int ParentID, string AccountNumber, string AName, string EName, int ReportingTypeID,int ReportingTypeNodeID, int AccountNatureID, int ModificationUserId, int CompanyID)
        {
            try
            {
                clsAccounts clsAccounts = new clsAccounts();
                int A = clsAccounts.UpdateAccounts(ID, ParentID, AccountNumber, AName, EName, ReportingTypeID, ReportingTypeNodeID,AccountNatureID, ModificationUserId, CompanyID);
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
        public bool DeleteBusinessPartnerByID(int ID, int CompanyID)
        {
            try
            {
                clsBusinessPartner clsBusinessPartner = new clsBusinessPartner();

                clsJournalVoucherDetails clsJournalVoucherDetails = new clsJournalVoucherDetails();
                DataTable dt= clsJournalVoucherDetails.SelectJournalVoucherDetailsByParentId( "" ,0,ID,0,0,0, CompanyID);
                if (dt != null && dt.Rows.Count > 0) {

                    return false;
                }
                bool A = clsBusinessPartner.DeleteBusinessPartnerByID(ID, CompanyID);
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
            string StreetName, string HouseNumber, string NationalNumber, string PassportNumber, int Nationality, string IDNumber,string TaxNumber, string Job)
        {
            try
            {
                clsBusinessPartner clsBusinessPartner = new clsBusinessPartner();
                int A = clsBusinessPartner.InsertBusinessPartner(Simulate.String(AName), Simulate.String(EName), Simulate.String(CommercialName)
                    , Simulate.String(Address), Simulate.String(Tel), Simulate.Bool(Active), Simulate.Val(Limit),
             Simulate.String(Email), Type, CompanyID, CreationUserId
             , Simulate.String(EmpCode), Simulate.String(StreetName), Simulate.String(HouseNumber), Simulate.String(NationalNumber),
                Simulate.String(PassportNumber), Simulate.Integer32(Nationality),
                Simulate.String(IDNumber), Simulate.String(TaxNumber), Simulate.String(Job) );
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
            string PassportNumber, int Nationality, string IDNumber,string TaxNumber,string Job,int CompanyID)
        {
            try
            {
                clsBusinessPartner clsBusinessPartner = new clsBusinessPartner();
                int A = clsBusinessPartner.UpdateBusinessPartner(ID, Simulate.String(AName), Simulate.String(EName), Simulate.String(CommercialName)
                    , Simulate.String(Address), Simulate.String(Tel), Simulate.Bool(Active), Simulate.Val(Limit),
            Simulate.String(Email), Type, ModificationUserId
            , Simulate.String(EmpCode), Simulate.String(StreetName), Simulate.String(HouseNumber), Simulate.String(NationalNumber),
                Simulate.String(PassportNumber), Simulate.Integer32(Nationality), Simulate.String(IDNumber),Simulate.String(TaxNumber), Simulate.String(Job), CompanyID);
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
        public ActionResult Fastreporttoxls( DataTable ds,bool IsRightToLeft)
        {

            //var grdReport = new System.Web.UI.WebControls.GridView();

            //grdReport.DataSource = ds;

            //grdReport.DataBind();

            using (XLWorkbook wb = new XLWorkbook())
            {
                ds.TableName = "s";
                wb.RightToLeft = IsRightToLeft;
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
                            if (ColumnType[ii].ToLower() == "int")
                            {
                                for (int i = 0; i < ds[iii].Rows.Count; i++)
                                {
                                    wb.Worksheet(SheetName[iii]).Cell(i + 2, ii + 1).Value = Simulate.Integer32(ds[iii].Rows[i][ii]);
                                }
                            }
                            else if (ColumnType[ii].ToLower() == "double" || ColumnType[ii].ToLower() == "decimal")
                            {
                                for (int i = 0; i < ds[iii].Rows.Count; i++)
                                {
                                    wb.Worksheet(SheetName[iii]).Cell(i + 2, ii + 1).Value = Simulate.Val(ds[iii].Rows[i][ii]);
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
                    return File(stream.ToArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "Grid.xlsx");


                 //   return File(stream.ToArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "Grid.csv");
               

                }
            }

            //StringBuilder sb = new StringBuilder();
            //for (int j = 0; j < 10; j++)
            //{
            //    //Append data with separator.
            //    sb.Append(j + ',');
            //}


            //return File(Encoding.UTF8.GetBytes(sb.ToString()), "text/csv", "Grid.csv");




        }
        [HttpGet("{menuId}/menuitems")]
        private void FastreportStanderdParameters(FastReport.Report Report, int UserID, int CompantID)
        {
            clsCompany clsCompany = new clsCompany();
            DataTable dt = clsCompany.SelectCompany(CompantID, "", "", "", CompantID,"");
            if (dt != null && dt.Rows.Count>0)
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
            DataTable dtemp = clsEmployee.SelectEmployee(UserID, "", "", "", "", CompantID,-1);
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
        public string SelectTrialBalance(DateTime Date1, DateTime Date2, int BranchID, int CostCenterID, int CompanyID, int Level)
        {
            try
            {
                clsReports clsReports = new clsReports();
                DataTable dt = clsReports.SelectTrialBalance(Date1, Date2, BranchID, CostCenterID, CompanyID, Level);
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
        public IActionResult SelectTrialBalancePDF(DateTime Date1, DateTime Date2, int BranchID, int CostCenterID, int UserId, int CompanyID, int Level)
        {
            try
            {

                FastReport.Utils.Config.WebMode = true;
                clsReports clsReports = new clsReports();
                DataTable dt = clsReports.SelectTrialBalance(Date1, Date2, BranchID, CostCenterID, CompanyID, Level);

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
                        ds.TrialBalance.Rows[i]["ChildCount"] = Simulate.Integer32( dt.Rows[i]["ChildCount"]);

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
        public string SelectAccountStatement(DateTime Date1, DateTime Date2, int BranchID, int CostCenterID, int Accountid, int subAccountid, int CompanyID,bool isDue,string JVTypeIDList)
        {
            try
            {
                clsReports clsReports = new clsReports();
                DataTable dt = clsReports.SelectAccountStatement(Date1, Date2, BranchID, CostCenterID, Accountid, subAccountid, CompanyID, isDue, JVTypeIDList);
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
        public IActionResult SelectAccountStatementPDF(DateTime Date1, DateTime Date2, int BranchID, int CostCenterID, int Accountid, int subAccountid, int UserID, int CompanyID,bool isDue, string JVTypeIDList)
        {
            try
            {

                FastReport.Utils.Config.WebMode = true;
                clsReports clsReports = new clsReports();
                DataTable dt = clsReports.SelectAccountStatement(Date1, Date2, BranchID, CostCenterID, Accountid, subAccountid, CompanyID, isDue, JVTypeIDList);

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
                    DataTable dtBranch = clsBranch.SelectBranch(BranchID, "", "", CompanyID);
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
                    DataTable dtCostCenter = clsCostCenter.SelectCostCentersByID(CostCenterID, "", "", CompanyID);
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
                    DataTable dtJournalVoucherTypes = clsJournalVoucherTypes.SelectJournalVoucherTypes(InvoiceTypeid,CompanyID);
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
        
        DataTable CreateSampleData()
{
    var table = new DataTable("SampleTable");
    table.Columns.Add("ID", typeof(int));
    table.Columns.Add("Name", typeof(string));
    table.Columns.Add("Age", typeof(int));

    table.Rows.Add(1, "John Doe", 29);
    table.Rows.Add(2, "Jane Smith", 34);
    table.Rows.Add(3, "Sam Brown", 42);

    return table;
}
        Report GenerateDynamicReport()
        {
            // Create a new report instance
             Report report = new Report();

            // Create and set up data source
            DataTable table = CreateSampleData();
            report.RegisterData(table, "SampleTable");

            // Add a report page
            var page = new ReportPage();
            report.Pages.Add(page);

            // Set page settings (e.g., orientation, margins)
            page.PaperWidth = 210;   // A4 width
            page.PaperHeight = 297;  // A4 height

            // Add title band
            var titleBand = new ReportTitleBand();
            titleBand.Height = FastReport.Utils.Units.Centimeters * 2;
            page.Bands.Add(titleBand);

            // Add title text
            var titleText = new TextObject();
            titleText.Bounds = new System.Drawing.RectangleF(0, 0, FastReport.Utils.Units.Centimeters * 10, FastReport.Utils.Units.Centimeters * 1);
            titleText.Text = "Dynamic Fast Report";
            titleText.HorzAlign = HorzAlign.Center;
            titleBand.Objects.Add(titleText);

            // Add data band
            var dataBand = new DataBand();
            dataBand.DataSource = report.GetDataSource("SampleTable");
            dataBand.Height = FastReport.Utils.Units.Centimeters * 0.7f;
            page.Bands.Add(dataBand);

            // Create text objects for each column in DataTable
            for (int i = 0; i < table.Columns.Count; i++)
            {
                var textObject = new TextObject();
                textObject.Bounds = new System.Drawing.RectangleF(FastReport.Utils.Units.Centimeters * i * 3, 0, FastReport.Utils.Units.Centimeters * 3, FastReport.Utils.Units.Centimeters * 0.7f);
                textObject.Text = $"[SampleTable.{table.Columns[i].ColumnName}]";
                dataBand.Objects.Add(textObject);
            }

            // Show or save report
            report.Prepare();
            report.Prepare();
            
            return    report;  // To preview report
                            // report.Save("Report.frx");  // To save the report definition if needed
        }

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
                        ds.InvoiceDetails.Rows[i]["DiscountBeforeTaxAmount"] = Simulate.decimal_(dtDetails.Rows[i]["DiscountBeforeTaxAmountAll"]);
                        ds.InvoiceDetails.Rows[i]["TaxID"] = Simulate.String(dtDetails.Rows[i]["TaxID"]);
                        ds.InvoiceDetails.Rows[i]["TaxPercentage"] = Simulate.String(dtDetails.Rows[i]["TaxPercentage"]);
                        ds.InvoiceDetails.Rows[i]["TaxAmount"] = Simulate.decimal_(dtDetails.Rows[i]["TaxAmount"]);
                        ds.InvoiceDetails.Rows[i]["SpecialTaxID"] = Simulate.String(dtDetails.Rows[i]["SpecialTaxID"]);
                        ds.InvoiceDetails.Rows[i]["SpecialTaxPercentage"] = Simulate.String(dtDetails.Rows[i]["SpecialTaxPercentage"]);
                        ds.InvoiceDetails.Rows[i]["SpecialTaxAmount"] = Simulate.decimal_(dtDetails.Rows[i]["SpecialTaxAmount"]);
                        ds.InvoiceDetails.Rows[i]["DiscountAfterTaxAmount"] = Simulate.decimal_(dtDetails.Rows[i]["DiscountAfterTaxAmountAll"]);
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
                report.Load(MyPath); 
                if (Simulate.Integer32(dtHeader.Rows[0]["BranchID"]) == 0)
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
                    DataTable dtJournalVoucherTypes = clsJournalVoucherTypes.SelectJournalVoucherTypes(Simulate.Integer32(dtHeader.Rows[0]["InvoiceTypeid"]),CompanyID);
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
                dsJVDetails ds = clsJournalVoucherDetails.SelectJournalVoucherDetailsByParentIdForPrint(CompanyID,guid, 0, 0);






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
                    DataTable dtBranch = clsBranch.SelectBranch(Simulate.Integer32(dtHeader.Rows[0]["BranchID"]), "", "", CompanyID);
                    if (dtBranch != null && dtBranch.Rows.Count > 0)
                    {
                        report.SetParameterValue("report.Branch", Simulate.String(dtBranch.Rows[0]["AName"]));

                    }
                }
                report.SetParameterValue("report.JVNo", Simulate.String(dtHeader.Rows[0]["JVNumber"]));
                report.SetParameterValue("report.CreationUser", Simulate.String(dtHeader.Rows[0]["EmployeeAName"]));
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

        #region Export to excel
      
        [Route("ExportDynamicsList")]
        public ActionResult ExportDynamicsList(int Name,[FromBody] string list=null)
        {
            try
            {
                  string [] aa= list.Split("//");
                DataTable dt = new DataTable();
                for (int i = 0; i < aa.Length; i++)
                {
                    string[] row = aa[i].Split("**");
                    for (int ii = 0; ii < row.Length; ii++)
                    {
                        if (i == 0)
                        {
                            dt.Columns.Add(row[ii]);
                    }
                        else {
                            if (dt.Rows.Count < i) {
                                dt.Rows.Add();
                            }
                            dt.Rows[i-1][ii]=row[ii];
                        }

                    }
                   

                }
               List<string> ColumnType = new List<String>();
                ColumnType.Add("int");
                ColumnType.Add("int");
                ColumnType.Add("string");
                ColumnType.Add("string");
                ColumnType.Add("string");
                ColumnType.Add("int");
                ColumnType.Add("string");
                ColumnType.Add("string");
                List<String> dtName = new List<String>();
                dtName.Add("Sheet");
                List<DataTable> dtList = new List<DataTable>();
                dtList.Add(dt); 
                return FastreporttoCSV(dtList, dtName, ColumnType);


                //clsCompany clsCompany = new clsCompany();
                //DataTable dtCompany = clsCompany.SelectCompany(CompanyID, "", "", "");
                //clsBranch clsBranch = new clsBranch();

                //DataTable dtBranch = clsBranch.SelectBranch(BranchID, "", "", 0);

                //FastReport.Utils.Config.WebMode = true;
                //clsFinancingHeader clsFinancingHeader = new clsFinancingHeader();
                //DataTable dt = clsFinancingHeader.SelectFinancingReport(Date1, Date2, Simulate.String(users), BranchID, CompanyID);

                //dsFinancingReport ds = new dsFinancingReport();
                //ds.DataTableH.Rows.Add();
                //ds.DataTableH.Rows[0]["Date1"] = Date1;
                //ds.DataTableH.Rows[0]["Date2"] = Date2;
                //if (dtCompany != null && dtCompany.Rows.Count > 0)
                //{

                //    ds.DataTableH.Rows[0]["CompanyName"] = dtCompany.Rows[0]["AName"];

                //}
                //if (dtBranch != null && dtBranch.Rows.Count == 1)
                //{

                //    ds.DataTableH.Rows[0]["BranchName"] = dtBranch.Rows[0]["AName"];

                //}
                //else
                //{
                //    ds.DataTableH.Rows[0]["BranchName"] = "All";

                //}
                //if (dt != null && dt.Rows.Count > 0)
                //{
                //    for (int i = 0; i < dt.Rows.Count; i++)
                //    {
                //        ds.DataTableD.Rows.Add();

                //        ds.DataTableD.Rows[i]["Index"] = i + 1;
                //        ds.DataTableD.Rows[i]["Customer"] = dt.Rows[i]["businessPartnerAName"];

                //        ds.DataTableD.Rows[i]["Total"] = dt.Rows[i]["FinancingAmount"];
                //        ds.DataTableD.Rows[i]["Price"] = dt.Rows[i]["FinancingAmount"];
                //        ds.DataTableD.Rows[i]["QTY"] = 1;
                //        ds.DataTableD.Rows[i]["Descrption"] = Simulate.String(dt.Rows[i]["Description"]);

                //    }
                //}





                //FastReport.Web.WebReport report = new FastReport.Web.WebReport();
                //report.Report.RegisterData(ds);



                //string MyPath = getMyPath("rptFinancingReport", CompanyID);
                //report.Report.Load(MyPath);


                //report.Report.Prepare();

            }
            catch (Exception)
            {

                throw;
            }
        }
        [Route("ExportLoansAmountReport")]
        public ActionResult ExportLoansAmountReport(DateTime DueDate1, DateTime DueDate2,int ARAccountID,int CompanyID)
        {
            try
            {
                clsSQL cls = new clsSQL();
                SqlParameter[] prm =
                {
                       new SqlParameter("@DueDate1", SqlDbType.DateTime) { Value = DueDate1 },
                         new SqlParameter("@DueDate2", SqlDbType.DateTime) { Value = DueDate2 },

                           new SqlParameter("@AccountID", SqlDbType.Int) { Value =ARAccountID },
                                      new SqlParameter("@CompanyID", SqlDbType.Int) { Value =CompanyID },
                };

                DataTable dt = cls.ExecuteQueryStatement(@"   select   tbl_LoanTypes.Code as N'النوع',
 tbl_BusinessPartner.EmpCode as N'الرقم',
  tbl_BusinessPartner.AName  as N'الاسم',
 (select sum(Total )  from tbl_JournalVoucherDetails 
 inner join tbl_JournalVoucherHeader on tbl_JournalVoucherHeader.Guid= tbl_JournalVoucherDetails.ParentGuid where
  SubAccountID =tbl_BusinessPartner.id and AccountID = 826
 and tbl_JournalVoucherHeader.RelatedLoanTypeID = tbl_LoanTypes.ID   ) as N'رصيد الذمم',
   sum(debit)   as N'الشهري',

isnull( (select sum(Credit )  from tbl_JournalVoucherDetails 
 inner join tbl_JournalVoucherHeader on tbl_JournalVoucherHeader.Guid= tbl_JournalVoucherDetails.ParentGuid where
 JVTypeID = 16 and SubAccountID =tbl_BusinessPartner.id 
 and tbl_JournalVoucherHeader.RelatedLoanTypeID = tbl_LoanTypes.ID and DueDate between  @DueDate1 and @DueDate2 ),0) as N'المدفوع' ,


  sum(debit)  - isnull( (select sum(Credit )  from tbl_JournalVoucherDetails 
 inner join tbl_JournalVoucherHeader on tbl_JournalVoucherHeader.Guid= tbl_JournalVoucherDetails.ParentGuid where
 JVTypeID = 16 and SubAccountID =tbl_BusinessPartner.id 
 and tbl_JournalVoucherHeader.RelatedLoanTypeID = tbl_LoanTypes.ID and DueDate between  @DueDate1 and @DueDate2 ),0)  as  N'الفرق'  
 from tbl_JournalVoucherDetails 
 left join tbl_BusinessPartner on tbl_BusinessPartner.id = tbl_JournalVoucherDetails.SubAccountID
 inner join tbl_JournalVoucherHeader on tbl_JournalVoucherHeader.Guid = tbl_JournalVoucherDetails.ParentGuid
 left join tbl_LoanTypes on tbl_JournalVoucherHeader.RelatedLoanTypeID = tbl_LoanTypes.ID
 where RelatedLoanTypeID > 0
 and DueDate between  @DueDate1 and @DueDate2
 and AccountID = @AccountID
and tbl_JournalVoucherHeader.CompanyID=@CompanyID
 group by tbl_BusinessPartner.EmpCode,tbl_BusinessPartner.AName,tbl_BusinessPartner.id ,tbl_LoanTypes.Code,tbl_LoanTypes.ID", cls.CreateDataBaseConnectionString(CompanyID), prm);


               
                return Fastreporttoxls(dt, false);


               

            }
            catch (Exception)
            {

                throw;
            }
        }
        DataTable ConvertListToDataTable(string[] list)
        {
            // New table.
            DataTable table = new DataTable();
            
            // Get max columns.
            int columns = 0;
            //foreach (var array in list)
            //{
            //    if (array.Length > columns)
            //    {
            //        columns = array.Length;
            //    }
            //}

            // Add columns.
            for (int i = 0; i < columns; i++)
            {
                table.Columns.Add();
            }

            // Add rows.
            foreach (var array in list)
            {
                table.Rows.Add(array);
            }

            return table;
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
        public bool DeleteItemsByGuid(string Guid,int CompanyID)
        {
            try
            {
                clsItems clsItems = new clsItems();
                bool A = clsItems.DeleteItemsByGuid(Guid, CompanyID);
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
            , bool IsActive, bool IsPOS, int BoxTypeID, bool IsStockItem, int POSOrder, int ModificationUserId, int CompanyID)
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
            , IsActive, IsPOS, BoxTypeID, IsStockItem, POSOrder, ModificationUserId, CompanyID);
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
        public string SelectTaxByID(int ID, int CompanyID,
            int IsSalesSpecialTax, int IsSalesTax, int IsPurchaseTax, int IsSpecialPurchaseTax)
        {
            try
            {

                clsTax clsTax = new clsTax();
                DataTable dt = clsTax.SelectTaxByID(ID, "", "", CompanyID,
             IsSalesSpecialTax,  IsSalesTax,  IsPurchaseTax,  IsSpecialPurchaseTax);
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
        public bool DeleteTaxByID(int ID,int CompanyID)
        {
            try
            {
                clsTax clsTax = new clsTax();
                bool A = clsTax.DeleteTaxByID(ID, CompanyID);
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
        public int UpdateTax(int ID, string AName, string EName, decimal Value, bool IsSalesTax, bool IsPurchaseTax, bool IsSalesSpecialTax, bool IsSpecialPurchaseTax, int ModificationUserId,int CompanyID)
        {
            try
            {
                clsTax clsTax = new clsTax();
                int A = clsTax.UpdateTax(ID, Simulate.String(AName), Simulate.String(EName), Value, IsSalesTax, IsPurchaseTax, IsSalesSpecialTax, IsSpecialPurchaseTax, ModificationUserId, CompanyID);
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
        public bool DeleteItemReadTypeByID(int ID,int CompanyID)
        {
            try
            {
                clsItemReadType ItemReadType = new clsItemReadType();
                bool A = ItemReadType.DeleteItemReadTypeByID(ID, CompanyID);
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
        public int UpdateItemReadType(int ID, string AName, string EName, int ModificationUserId,int CompanyID)
        {
            try
            {
                clsItemReadType clsItemReadType = new clsItemReadType();
                int A = clsItemReadType.UpdateItemReadType(ID, Simulate.String(AName), Simulate.String(EName), ModificationUserId, CompanyID);
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
        public bool DeleteCountriesByID(int ID,int CompanyID)
        {
            try
            {
                clsCountries Countries = new clsCountries();
                bool A = Countries.DeleteCountriesByID(ID, CompanyID);
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
        public int UpdateCountries(int ID, string AName, string EName, int ModificationUserId,int CompanyID)
        {
            try
            {
                clsCountries clsCountries = new clsCountries();
                int A = clsCountries.UpdateCountries(ID, Simulate.String(AName), Simulate.String(EName), ModificationUserId, CompanyID);
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
        public bool DeleteItemsBoxTypeByID(int ID,int CompanyID)
        {
            try
            {
                clsItemsBoxType clsItemsBoxType = new clsItemsBoxType();
                bool A = clsItemsBoxType.DeleteItemsBoxTypeByID(ID, CompanyID);
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
        public int UpdateItemsBoxType(int ID, string AName, string EName, decimal Qty, int ModificationUserId,int CompanyID)
        {
            try
            {
                clsItemsBoxType clsItemsBoxType = new clsItemsBoxType();
                int A = clsItemsBoxType.UpdateItemsBoxType(ID, Simulate.String(AName), Simulate.String(EName), Simulate.decimal_(Qty), ModificationUserId, CompanyID);
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
        public bool DeleteInvoiceDetailsByHeaderGuid(string Guid,int CompanyID)
        {
            try
            {
                clsInvoiceDetails clsInvoiceDetails = new clsInvoiceDetails();
                clsInvoiceHeader clsInvoiceHeader = new clsInvoiceHeader();

                clsJournalVoucherHeader clsJournalVoucherHeader = new clsJournalVoucherHeader();
                clsJournalVoucherDetails clsJournalVoucherDetails = new clsJournalVoucherDetails();
                SqlTransaction trn; clsSQL clsSQL = new clsSQL();
                SqlConnection con = new SqlConnection(clsSQL.CreateDataBaseConnectionString(CompanyID));
                con.Open();
                trn = con.BeginTransaction(); int A = 0;
                bool IsSaved = true;
                try
                {
                    DataTable dt = clsInvoiceHeader.SelectInvoiceHeaderByGuid(Guid, Simulate.StringToDate("1900-01-01"), Simulate.StringToDate("2300-01-01"), 0, 0, 0, trn);
                    IsSaved = clsInvoiceHeader.DeleteInvoiceHeaderByGuid(Guid,CompanyID, trn);
                    bool a = clsInvoiceDetails.DeleteInvoiceDetailsByHeaderGuid(Guid,CompanyID, trn);
                    if (dt != null && dt.Rows.Count > 0)
                    {
                        string JVGuid = Simulate.String(dt.Rows[0]["JVGuid"]);
                        bool aa = clsJournalVoucherHeader.DeleteJournalVoucherHeaderByID(JVGuid,CompanyID, trn);
                        bool aaa = clsJournalVoucherDetails.DeleteJournalVoucherDetailsByParentId(JVGuid,CompanyID, trn);
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
            , int cashID,  int bankid, string refNo, int invoiceNo, decimal headerDiscount
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
                    BankID = Simulate.Integer32(bankid),
                    
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
                SqlConnection con = new SqlConnection(clsSQL.CreateDataBaseConnectionString(companyID));
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
                        IsSaved = clsInvoiceHeader.InsertInvoiceJournalVoucher(details, accountID, paymentMethodID, cashID, bankid, businessPartnerID, headerDiscount, Simulate.Integer32(branchID), Simulate.String(note), Simulate.Integer32(companyID), Simulate.StringToDate(invoiceDate), creationUserId, invoiceTypeID, A, trn);
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
            , int cashID,   int bankid, string refNo, int invoiceNo, decimal headerDiscount
            , int invoiceTypeID, bool isCounted, string note,
            decimal totalTax, string pOSDayGuid, string relatedInvoiceGuid,
            decimal totalDiscount, int paymentMethodID,
            string pOSSessionGuid, decimal totalInvoice,
            DateTime invoiceDate, int modificationUserID, string guid, int accountID,int compnayid,
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
                    BankID = Simulate.Integer32(bankid),
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
                SqlConnection con = new SqlConnection(clsSQL.CreateDataBaseConnectionString(compnayid));
                con.Open();
                trn = con.BeginTransaction();
                string A = "";
                try
                {
                    bool IsSaved = true;

                    A = clsInvoiceHeader.UpdateInvoiceHeader(dbInvoiceHeader,compnayid, trn);
                    clsInvoiceDetails.DeleteInvoiceDetailsByHeaderGuid(guid,compnayid, trn);
                    for (int i = 0; i < details.Count; i++)
                    {

                        string c = clsInvoiceDetails.InsertInvoiceDetails(details[i], guid, trn);
                        if (c == "")
                            IsSaved = false;
                    }
                    if (IsSaved)
                        IsSaved = clsInvoiceHeader.InsertInvoiceJournalVoucher(details, accountID, paymentMethodID, cashID, bankid, businessPartnerID, headerDiscount, Simulate.Integer32(branchID), Simulate.String(note),compnayid, Simulate.StringToDate(invoiceDate), modificationUserID, invoiceTypeID, guid, trn);
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
        public bool DeleteStoreByID(int ID,int CompanyID)
        {
            try
            {
                clsStore clsStore = new clsStore();
                bool A = clsStore.DeleteStoreByID(ID, CompanyID);
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
        public int UpdateStore(int ID, string AName, string EName, int BranchID, int ModificationUserId,int CompanyID)
        {
            try
            {
                clsStore clsStore = new clsStore();
                int A = clsStore.UpdateStore(ID, Simulate.String(AName), Simulate.String(EName), BranchID, ModificationUserId, CompanyID);
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
        public bool DeleteCashDrawerByID(int ID,int CashAccountID, int CompanyID)
        {
            try
            {
                clsJournalVoucherDetails clsJournalVoucherDetails = new clsJournalVoucherDetails();
                DataTable dt = clsJournalVoucherDetails.SelectJournalVoucherDetailsByParentId("", CashAccountID, ID, 0, 0, 0, CompanyID);
                if (dt != null && dt.Rows.Count > 0)
                {

                    return false;
                }
                clsCashDrawer clsCashDrawer = new clsCashDrawer();
                bool A = clsCashDrawer.DeleteCashDrawerByID(ID, CompanyID);
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
        public int UpdateCashDrawer(int ID, string AName, string EName, int BranchID, int ModificationUserId,int CompanyID)
        {
            try
            {
                clsCashDrawer clsCashDrawer = new clsCashDrawer();
                int A = clsCashDrawer.UpdateCashDrawer(ID, Simulate.String(AName), Simulate.String(EName), BranchID, ModificationUserId, CompanyID);
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
        public bool DeletePaymentMethodByID(int ID, int CompanyID)
        {
            try
            {
                clsPaymentMethod clsPaymentMethod = new clsPaymentMethod();
                bool A = clsPaymentMethod.DeletePaymentMethodByID(ID, CompanyID);
                return A;
            }
            catch (Exception)
            {

                throw;
            }

        }
        [HttpGet]
        [Route("InsertPaymentMethod")]
        public int InsertPaymentMethod(string AName, string EName, int BranchID, int GLAccountID, int GLSubAccountID,
            bool IsCash, bool IsBank, bool IsDebit, int CompanyID, int CreationUserId)
        {
            try
            {
                clsPaymentMethod clsPaymentMethod = new clsPaymentMethod();
                int A = clsPaymentMethod.InsertPaymentMethod(Simulate.String(AName), Simulate.String(EName), BranchID,
                     GLAccountID,  GLSubAccountID,
             IsCash,  IsBank,  IsDebit, CompanyID, CreationUserId);
                return A;
            }
            catch (Exception ex)
            {

                throw;
            }

        }
        [HttpGet]
        [Route("UpdatePaymentMethod")]
        public int UpdatePaymentMethod(int ID, string AName, string EName, int BranchID, int GLAccountID, int GLSubAccountID,
            bool IsCash, bool IsBank, bool IsDebit, int ModificationUserId, int CompanyID )
        {
            try
            {
                clsPaymentMethod clsPaymentMethod = new clsPaymentMethod();
                int A = clsPaymentMethod.UpdatePaymentMethod(ID, Simulate.String(AName), Simulate.String(EName), BranchID, 
                    GLAccountID,  GLSubAccountID,
             IsCash,  IsBank,  IsDebit, ModificationUserId, CompanyID);
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
        public int UpdatePOSSetting(int ID, int CashDrawerID, int POSSettingID, int Value, int ModificationUserId,int CompanyID)
        {
            try
            {
                clsPOSSetting clsPOSSetting = new clsPOSSetting();
                int A = clsPOSSetting.UpdatePOSSetting(ID, CashDrawerID, POSSettingID, Simulate.String(Value), ModificationUserId, CompanyID);
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
        public bool DeletePOSDayByGuid(string Guid,int CompanyID)
        {
            try
            {

                clsPOSDay clsPOSDay = new clsPOSDay();
                bool A = clsPOSDay.DeletePOSDayByGuid(Guid, CompanyID);
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
        public int UpdatePOSDay(string Guid, DateTime StartDate, DateTime EndDate, DateTime POSDate, int Status, int ModificationUserId,int CompanyID)
        {
            try
            {
                clsPOSDay clsPOSDay = new clsPOSDay();
                int A = clsPOSDay.UpdatePOSDay(Guid, StartDate, EndDate, POSDate, Status, ModificationUserId, CompanyID);
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
                SqlConnection con = new SqlConnection(clsSQL.CreateDataBaseConnectionString(CompanyID));
                con.Open();
                trn = con.BeginTransaction();
                try
                {
                    bool IsSaved = true;
                    int A = clsPOSDay.ClosePOSDay(Guid, EndDate, CreationUserId,CompanyID, trn);


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
        public bool DeletePOSSessionsByGuid(string Guid,int CompanyID)
        {
            try
            {

                clsPOSSessions clsPOSSessions = new clsPOSSessions();
                bool A = clsPOSSessions.DeletePOSSessionsByGuid(Guid,CompanyID);
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
        public int UpdatePOSSessions(string Guid, int SessionTypeID, string POSDayGuid, DateTime StartDate, DateTime EndDate, int CashDrawerID, int Status, int ModificationUserId,int CompanyID)
        {
            try
            {
                clsPOSSessions clsPOSSessions = new clsPOSSessions();
                int A = clsPOSSessions.UpdatePOSSessions(Guid, SessionTypeID, POSDayGuid, StartDate, EndDate, CashDrawerID, Status, ModificationUserId, CompanyID);
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
                SqlConnection con = new SqlConnection(clsSQL.CreateDataBaseConnectionString(CompanyID));
                con.Open();
                trn = con.BeginTransaction();
                try
                {
                    bool IsSaved = true;
                    int A = clsPOSSessions.ClosePOSSessions(Guid, EndDate, CreationUserId,CompanyID, trn);


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
                DataTable dt = clsJournalVoucherTypes.SelectJournalVoucherTypes(type, CompanyID);
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
                DataTable dt = clssql.ExecuteQueryStatement(a, clssql.CreateDataBaseConnectionString(CompanyID), prm);


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
                DataTable dt = clssql.ExecuteQueryStatement(a, clssql.CreateDataBaseConnectionString(CompanyID), prm);


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
        [Route("SelectMonthlyGLAccountBalance")]
        public string SelectMonthlyGLAccountBalance(bool IsBalanceToDate,DateTime StartDate, DateTime EndDate, string AccountType,int CompanyID)
        {
            try
            {
                clsSQL clssql = new clsSQL();
                SqlParameter[] prm =
                 {
                      new SqlParameter("@StartDate", SqlDbType.DateTime) { Value =StartDate },
                       new SqlParameter("@EndDate", SqlDbType.DateTime) { Value =EndDate },
                        new SqlParameter("@CompanyID", SqlDbType.Int) { Value =CompanyID },
                };
                string TransactionsByMonths = @"  
SELECT 
    SUM(Total) *-1  AS NetSales, 
    YEAR(VoucherDate) AS Year, 
    MONTH(VoucherDate) AS Month
FROM 
    tbl_JournalVoucherDetails
INNER JOIN 
    tbl_JournalVoucherHeader ON tbl_JournalVoucherHeader.Guid = tbl_JournalVoucherDetails.ParentGuid
WHERE 
    AccountID IN (
        SELECT 
            AccountID 
        FROM 
            tbl_AccountSetting 
        WHERE 
            AccountRefID IN ("+ AccountType + @") 
            AND CompanyID = @companyid 
            AND Active = 1
    )
  and voucherdate between @startdate and @enddate
GROUP BY 
    YEAR(VoucherDate), 
    MONTH(VoucherDate)
ORDER BY 
    Year, 
    Month;
SELECT 
    SUM(Total) *-1  AS NetSales, 
    YEAR(VoucherDate) AS Year, 
    MONTH(VoucherDate) AS Month
FROM 
    tbl_JournalVoucherDetails
INNER JOIN 
    tbl_JournalVoucherHeader ON tbl_JournalVoucherHeader.Guid = tbl_JournalVoucherDetails.ParentGuid
WHERE 
    AccountID IN (
        SELECT 
            AccountID 
        FROM 
            tbl_AccountSetting 
        WHERE 
            AccountRefID IN (" + AccountType + @") 
            AND CompanyID = @companyid 
            AND Active = 1
    )
 and voucherdate between @startdate and @enddate
GROUP BY 
    YEAR(VoucherDate), 
    MONTH(VoucherDate)
ORDER BY 
    Year, 
    Month;";


                string BalanceTodate = @" 

    DECLARE @CurrentDate DATE= @StartDate

CREATE TABLE #MonthlyTotals (
    Year int,    Month int,
    NetSales DECIMAL(18, 2)
)
WHILE @CurrentDate <= @EndDate
BEGIN
      INSERT INTO #MonthlyTotals (Year,Month, NetSales)
    SELECT 
	    YEAR(@CurrentDate) AS Year, 
    MONTH(@CurrentDate) AS Month,
	
	
	 SUM(Total) AS NetSales
    FROM tbl_JournalVoucherDetails
    INNER JOIN tbl_JournalVoucherHeader ON tbl_JournalVoucherHeader.Guid = tbl_JournalVoucherDetails.ParentGuid
    WHERE VoucherDate <= @CurrentDate
      AND tbl_JournalVoucherHeader.companyid = @companyid
	  and  AccountID IN (
            SELECT 
                AccountID 
            FROM 
                tbl_AccountSetting 
            WHERE 
                AccountRefID IN  ("+ AccountType + @") 
                AND CompanyID =@CompanyID
                AND Active = 1
        )
    SET @CurrentDate = DATEADD(MONTH, 1, @CurrentDate)
END
SELECT * FROM #MonthlyTotals
DROP TABLE #MonthlyTotals";
                string a;
                if (IsBalanceToDate) {
                    a = BalanceTodate;
                } else { a = TransactionsByMonths; }
                DataTable dt = clssql.ExecuteQueryStatement(a, clssql.CreateDataBaseConnectionString(CompanyID), prm);


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
        public string SelectCashVoucherHeaderByGuid(string Guid, int BranchID, int VoucherTypeID, int CompanyID, DateTime Date1, DateTime Date2,string RelatedFinancingGuid)
        {
            try
            {
                clsCashVoucherHeader clsCashVoucherHeader = new clsCashVoucherHeader();
                DataTable dt = clsCashVoucherHeader.SelectCashVoucherHeaderByGuid(Simulate.String(Guid), Date1, Date2, VoucherTypeID, BranchID, CompanyID, RelatedFinancingGuid);
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
        public bool DeleteCashVoucherHeaderByGuid(string Guid, int CompanyID)
        {
            try
            {
        
                clsCashVoucherDetails clsCashVoucherDetails = new clsCashVoucherDetails();
                clsCashVoucherHeader clsCashVoucherHeader = new clsCashVoucherHeader();
                clsJournalVoucherHeader clsJournalVoucherHeader = new clsJournalVoucherHeader();
                clsJournalVoucherDetails clsJournalVoucherDetails = new clsJournalVoucherDetails();
                SqlTransaction trn; clsSQL clsSQL = new clsSQL();
                SqlConnection con = new SqlConnection(clsSQL.CreateDataBaseConnectionString(CompanyID));
                con.Open();
                trn = con.BeginTransaction(); int A = 0;
                bool IsSaved = true;
                try
                {
                    DataTable dt = clsCashVoucherHeader.SelectCashVoucherHeaderByGuid(Guid, Simulate.StringToDate("1900-01-01"), Simulate.StringToDate("2300-01-01"), 0, 0, 0,  "", trn);
                    IsSaved = clsCashVoucherHeader.DeleteCashVoucherHeaderByGuid(Guid,CompanyID, trn);
                    bool a = clsCashVoucherDetails.DeleteCashVoucherDetailsByHeaderGuid(Guid,CompanyID, trn);
                    if (dt != null && dt.Rows.Count > 0)
                    {
                        string JVGuid = Simulate.String(dt.Rows[0]["JVGuid"]);
                        bool aa = clsJournalVoucherHeader.DeleteJournalVoucherHeaderByID(JVGuid,CompanyID, trn);
                        bool aaa = clsJournalVoucherDetails.DeleteJournalVoucherDetailsByParentId(JVGuid,CompanyID, trn);
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

        public string InsertCashVoucherHeader(DateTime voucherDate, int branchID, int costCenterID,
            int AccountID, int cashID
            , decimal amount, string note, int voucherNumber
            , string manualNo, int voucherType, string relatedInvoiceGuid, int companyID, int creationUserID
            , int PaymentMethodTypeID, string ChequeNote, DateTime DueDate,
             string ChequeName,
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
                    ChequeName = Simulate.String(ChequeName),
                    DueDate = DueDate,
                    ChequeNote = Simulate.String(ChequeNote),
                    PaymentMethodTypeID = Simulate.Integer32(PaymentMethodTypeID),
                };


                List<DBCashVoucherDetails> details = JsonConvert.DeserializeObject<List<DBCashVoucherDetails>>(DetailsList);
                clsCashVoucherHeader clsCashVoucherHeader = new clsCashVoucherHeader();
                clsCashVoucherDetails clsCashVoucherDetails = new clsCashVoucherDetails();
                SqlTransaction trn; clsSQL clsSQL = new clsSQL();
                SqlConnection con = new SqlConnection(clsSQL.CreateDataBaseConnectionString(companyID));
                con.Open();
                trn = con.BeginTransaction(); string A = "";
                try
                {
                    bool IsSaved = true;

                    DataTable dt = clsSQL.ExecuteQueryStatement("select isnull( max(voucherno),0)+1 as Max from tbl_cashvoucherheader where  VoucherType ="+ Simulate.String(voucherType) +" and companyid=" + companyID.ToString(), clsSQL.CreateDataBaseConnectionString(companyID), trn);
                    if (dt != null && dt.Rows.Count > 0) {

                        dbCashVoucherHeader.VoucherNo = Simulate.Integer32(dt.Rows[0][0]);
                    }
                    else {

                        dbCashVoucherHeader.VoucherNo = 1;
                    }

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
                        IsSaved = clsCashVoucherHeader.InsertCashVoucherJournalVoucher(A, AccountID, branchID, costCenterID, cashID, amount, Simulate.String(note), voucherDate, details, "", voucherType, companyID, creationUserID, trn);
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
             int modificationUserID, string guid,int PaymentMethodTypeID,string ChequeNote,DateTime DueDate,
             string ChequeName,
            [FromBody] string detailsList)
        {





            try
            {

                DBCashVoucherHeader dbCashVoucherHeader = new DBCashVoucherHeader
                {
                    VoucherDate = voucherDate,
                    BranchID = branchID,
                    CostCenterID = costCenterID,
                    AccountID = AccountID,
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
                    ChequeName = Simulate.String(ChequeName),
                    DueDate = DueDate,
                    ChequeNote = Simulate.String(ChequeNote),
                    PaymentMethodTypeID = Simulate.Integer32(PaymentMethodTypeID),
                };

                List<DBCashVoucherDetails> details = JsonConvert.DeserializeObject<List<DBCashVoucherDetails>>(detailsList);
                clsCashVoucherHeader clsCashVoucherHeader = new clsCashVoucherHeader();
                clsCashVoucherDetails clsCashVoucherDetails = new clsCashVoucherDetails();
                SqlTransaction trn; clsSQL clsSQL = new clsSQL();
                SqlConnection con = new SqlConnection(clsSQL.CreateDataBaseConnectionString(companyID));
                con.Open();
                trn = con.BeginTransaction();
                string A = "";
                try
                {
                    bool IsSaved = true;

                    A = clsCashVoucherHeader.UpdateCashVoucherHeader(dbCashVoucherHeader, companyID, trn);
                    clsCashVoucherDetails.DeleteCashVoucherDetailsByHeaderGuid(guid, companyID, trn);
                    for (int i = 0; i < details.Count; i++)
                    {

                        string c = clsCashVoucherDetails.InsertCashVoucherDetails(details[i], guid, trn);
                        if (c == "")
                            IsSaved = false;
                    }
                    if (IsSaved)
                        IsSaved = clsCashVoucherHeader.InsertCashVoucherJournalVoucher(guid, AccountID, branchID, costCenterID, cashID, amount, Simulate.String(note), voucherDate, details, Simulate.String(jVGuid), voucherType, companyID, modificationUserID, trn);
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
           
                DataTable dtHeader = clsCashVoucherHeader.SelectCashVoucherHeaderByGuid(HeaderGuid, DateTime.Now.AddYears(-100), DateTime.Now.AddYears(100), 0, 0, CompanyID, "");
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

                        ds.Header.Rows[i]["PaymentMethodAName"] = Simulate.String   (dtHeader.Rows[i]["PaymentMethodAName"]);


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

        [HttpGet]
        [Route("PrintCashVoucherCheque")]
        public IActionResult PrintCashVoucherCheque(string HeaderGuid, int UserId, int CompanyID)
        {
            try
            {

                FastReport.Utils.Config.WebMode = true;
                clsCashVoucherHeader clsCashVoucherHeader = new clsCashVoucherHeader();
                clsCashVoucherDetails clsCashVoucherDetails = new clsCashVoucherDetails();

                DataTable dtHeader = clsCashVoucherHeader.SelectCashVoucherHeaderByGuid(HeaderGuid, DateTime.Now.AddYears(-100), DateTime.Now.AddYears(100), 0, 0, CompanyID, "");
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



                string MyPath = getMyPath("rptCheques", CompanyID);
                report.Load(MyPath);
                report.RegisterData(ds);


                report.SetParameterValue("VoucherDate", Simulate.StringToDate(dtHeader.Rows[0]["DueDate"]).ToString("yyyy-MM-dd"));
                report.SetParameterValue("Name", Simulate.String(dtHeader.Rows[0]["ChequeName"]));
                report.SetParameterValue("amountfils", Simulate.String(AmountDecimal));
                report.SetParameterValue("Amount", Simulate.String(AmountWithOutDecimal));
                report.SetParameterValue("AmountTafkeet", Simulate.String(AmountToWord));
                report.SetParameterValue("Notes", Simulate.String(dtHeader.Rows[0]["ChequeNote"]));



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
        public bool DeleteBanksByID(int ID,int CompanyID)
        {
            try
            {
                clsJournalVoucherDetails clsJournalVoucherDetails = new clsJournalVoucherDetails();
                DataTable dt = clsJournalVoucherDetails.SelectJournalVoucherDetailsByParentId("", 0, 0,ID, 0, 0, CompanyID);
                if (dt != null && dt.Rows.Count > 0)
                {

                    return false;
                }
                clsBanks clsBanks = new clsBanks();
                bool A = clsBanks.DeleteBanksByID(ID, CompanyID);
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
        public int UpdateBanks(int ID, string AName, string EName, string AccountNumber, int ModificationUserId,int CompanyID)
        {
            try
            {
                clsBanks clsBanks = new clsBanks();
                int A = clsBanks.UpdateBanks(ID, Simulate.String(AName), Simulate.String(EName), Simulate.String(AccountNumber), ModificationUserId, CompanyID);
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
        public bool DeletePOSSessionsTypeByID(int ID,int CompanyID)
        {
            try
            {
                clsPosSessionsType clsPosSessionsType = new clsPosSessionsType();
                bool A = clsPosSessionsType.DeletePOSSessionsTypeByID(ID, CompanyID);
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
        public int UpdatePOSSessionsType(int ID, string AName, string EName, int ModificationUserId, int CompanyID)
        {
            try
            {
                clsPosSessionsType clsPosSessionsType = new clsPosSessionsType();
                int A = clsPosSessionsType.UpdatePOSSessionsType(ID, AName, EName, ModificationUserId, CompanyID);
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
        [Route("SelectEmployeesLoans")]
        public string SelectEmployeesLoans(DateTime Date1, DateTime Date2, int accountid, int BusinessPartnerID, int CompanyID,bool HideZeroBalances)
        {
            try
            {
                clsFinancingHeader clsFinancingHeader = new clsFinancingHeader();
                DataTable dt = clsFinancingHeader.SelectEmployeesLoans( Date1,  Date2,  accountid,  BusinessPartnerID,  CompanyID, HideZeroBalances);
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
        [Route("SelectEmployeesLoansPDF")]
        public IActionResult SelectEmployeesLoansPDF( DateTime Date1, DateTime Date2 ,int accountid, int BusinessPartnerID, int CompanyID, string userID,bool HideZeroBalances)
        {
            try
            {
                clsCompany clsCompany = new clsCompany();
                DataTable dtCompany = clsCompany.SelectCompany(CompanyID, "", "", "", CompanyID, "");
                clsBranch clsBranch = new clsBranch();

 
                FastReport.Utils.Config.WebMode = true;
                clsFinancingHeader clsFinancingHeader = new clsFinancingHeader();
                DataTable dt = clsFinancingHeader.SelectEmployeesLoans(Date1, Date2, accountid, BusinessPartnerID, CompanyID  , HideZeroBalances);

                dsEmployeeLoans ds = new dsEmployeeLoans();
             
                
                if (dt != null && dt.Rows.Count > 0)
                {
                    for (int i = 0; i < dt.Rows.Count; i++)
                    {
                        ds.DataTable1.Rows.Add();
                        ds.DataTable1.Rows[i]["Index"] = i + 1;
                        ds.DataTable1.Rows[i]["VoucherNumber"] = dt.Rows[i]["VoucherNumber"];

                        
                        ds.DataTable1.Rows[i]["BusinessPartnerID"] = dt.Rows[i]["BusinessPartnerID"];
                        ds.DataTable1.Rows[i]["BusinessPartnerAName"] = dt.Rows[i]["BusinessPartnerAName"];
                        ds.DataTable1.Rows[i]["EmpCode"] = dt.Rows[i]["EmpCode"];
                        ds.DataTable1.Rows[i]["Code"] =  dt.Rows[i]["Code"];
                        ds.DataTable1.Rows[i]["VoucherDate"] =Simulate.StringToDate( dt.Rows[i]["VoucherDate"]).ToString("yyyy-MM-dd");
                        ds.DataTable1.Rows[i]["Description"] = dt.Rows[i]["Description"];
                        ds.DataTable1.Rows[i]["TotalAmount"] = Simulate.Val( dt.Rows[i]["TotalAmount"]);
                        ds.DataTable1.Rows[i]["InstallmentAmount"] = Simulate.Val(dt.Rows[i]["InstallmentAmount"]);
                        ds.DataTable1.Rows[i]["Paid"] = Simulate.Val(dt.Rows[i]["Paid"]);
                        ds.DataTable1.Rows[i]["RemainingAmount"] =Simulate.Val( Simulate.Val(dt.Rows[i]["TotalAmount"])-Simulate.Val( dt.Rows[i]["Paid"]));

                        ds.DataTable1.Rows[i]["PeriodInMonths"] = dt.Rows[i]["PeriodInMonths"];
                        ds.DataTable1.Rows[i]["FirstInstallmentDate"] = dt.Rows[i]["FirstInstallmentDate"];
                        ds.DataTable1.Rows[i]["LastInstallmentDate"] = dt.Rows[i]["LastInstallmentDate"];
                        ds.DataTable1.Rows[i]["DueAmount"] = dt.Rows[i]["DueAmount"];

                        
                    }
                }



                string Name = "All";
                if (BusinessPartnerID > 0 && dt.Rows.Count>0) {
                    Name = Simulate.String(dt.Rows[0]["BusinessPartnerAName"]);
                }
                string EMPCode = "All";
                if (BusinessPartnerID > 0 && dt.Rows.Count > 0)
                {
                    EMPCode = Simulate.String(dt.Rows[0]["EmpCode"]);
                }
                FastReport.Report report = new FastReport.Report();
                report.RegisterData(ds);
             

                string MyPath = getMyPath("rptCutomerLoansReport", CompanyID);
                report.Load(MyPath);
                report.SetParameterValue("report.FromDate", Date1.ToString("yyyy-MM-dd"));
                report.SetParameterValue("report.ToDate", Date2.ToString("yyyy-MM-dd"));
                report.SetParameterValue("report.Name", Name);
                report.SetParameterValue("report.EMPCode", EMPCode);

                
                FastreportStanderdParameters(report, 0, CompanyID);
 
                report.Prepare();

                return FastreporttoPDF(report);
             }
            catch (Exception)
            {

                throw;
            }
        }

        [HttpGet]
        [Route("SelectFinancingHeaderByGuid")]
        public string SelectFinancingHeaderByGuid(string Guid, int BranchID, int CreationUserID,  int CompanyID, DateTime Date1, DateTime Date2,int CurrentUserId,string LoanType,int BusinessPartnerID)
        {
            try
            {
                clsFinancingHeader clsFinancingHeader = new clsFinancingHeader();
                DataTable dt = clsFinancingHeader.SelectFinancingHeaderByGuid(Simulate.String(Guid), Date1, Date2,  BranchID, CreationUserID, CompanyID, CurrentUserId, LoanType, BusinessPartnerID);
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
             DataTable   dtCompany = clsCompany.SelectCompany(CompanyID, "", "", "", CompanyID, "");
                clsBranch clsBranch = new clsBranch();

                DataTable dtBranch = clsBranch.SelectBranch(BranchID, "", "" , CompanyID);

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

                        ds.DataTableD.Rows[i]["Total"] = dt.Rows[i]["TotalAmount"];
                        ds.DataTableD.Rows[i]["Price"] = dt.Rows[i]["TotalAmount"];
                        ds.DataTableD.Rows[i]["QTY"] = 1;
                        ds.DataTableD.Rows[i]["Descrption"] = Simulate.String(dt.Rows[i]["Description"]);
                      
                    }
                }





                FastReport.Report report = new FastReport.Report();
                 report.RegisterData(ds);

               // FastreportStanderdParameters(report,0,0);
                 
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
                FastreportStanderdParameters(report, 0, CompanyID);
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
               
                DataTable dtCompany = clsCompany.SelectCompany(CompanyID, "", "", "", CompanyID, "");
                clsBranch clsBranch = new clsBranch();

                DataTable dtBranch = clsBranch.SelectBranch(BranchID, "", "", CompanyID);

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

                        ds.DataTableD.Rows[i]["Total"] = dt.Rows[i]["TotalAmount"];
                        ds.DataTableD.Rows[i]["Price"] = dt.Rows[i]["TotalAmount"];
                        ds.DataTableD.Rows[i]["QTY"] = 1;
                        ds.DataTableD.Rows[i]["Descrption"] = Simulate.String(dt.Rows[i]["Description"]);

                    }
                }





                FastReport.Web.WebReport report = new FastReport.Web.WebReport();
                report.Report.RegisterData(ds);


               
                string MyPath = getMyPath("rptFinancingReport", CompanyID);
                report.Report.Load(MyPath);
                

                report.Report.Prepare();

                return Fastreporttoxls(ds.DataTableD, false);
             }
            catch (Exception)
            {

                throw;
            }
        }
        [HttpGet]
        [Route("DeleteFinancingHeaderByGuid")]
        public bool DeleteFinancingHeaderByGuid(string Guid,int CompanyID)
        {
            try
            {
                clsFinancingDetails clsFinancingDetails = new clsFinancingDetails();
                clsFinancingHeader clsFinancingHeader = new clsFinancingHeader();
                clsJournalVoucherHeader clsJournalVoucherHeader = new clsJournalVoucherHeader();
                clsJournalVoucherDetails clsJournalVoucherDetails = new clsJournalVoucherDetails();
                SqlTransaction trn; clsSQL clsSQL = new clsSQL();
                SqlConnection con = new SqlConnection(clsSQL.CreateDataBaseConnectionString(CompanyID));
                con.Open();
                trn = con.BeginTransaction(); int A = 0;
                bool IsSaved = true;
                try
                {
                    DataTable dt = clsFinancingHeader.SelectFinancingHeaderByGuid(Guid, Simulate.StringToDate("1900-01-01"), Simulate.StringToDate("2300-01-01"), 0, 0,  0, 0, "-1", 0,trn);
                    IsSaved = clsFinancingHeader.DeleteFinancingHeaderByGuid(Guid,CompanyID, trn);
                    bool a = clsFinancingDetails.DeleteFinancingDetailsByHeaderGuid(Guid,CompanyID, trn);
                    if (dt != null && dt.Rows.Count > 0)
                    {
                        string JVGuid = Simulate.String(dt.Rows[0]["JVGuid"]);


                        bool aa = clsJournalVoucherHeader.DeleteJournalVoucherHeaderByID(JVGuid,CompanyID, trn);
                        bool aaa = clsJournalVoucherDetails.DeleteJournalVoucherDetailsByParentId(JVGuid, CompanyID, trn);
                        clsCashVoucherDetails clsCashVoucherDetails = new clsCashVoucherDetails();

                        clsCashVoucherHeader clsCashVoucherHeader = new clsCashVoucherHeader();
                        DataTable dtcash = clsCashVoucherHeader.SelectCashVoucherHeaderByGuid("",DateTime.Now.AddYears(-100), DateTime.Now.AddYears(100),0,0,0, Guid,trn);

                        if (dtcash != null && dtcash.Rows.Count > 0) {

                            clsCashVoucherHeader.DeleteCashVoucherHeaderByGuid(Simulate.String( dtcash.Rows[0]["Guid"]),CompanyID, trn);
                            clsCashVoucherDetails.DeleteCashVoucherDetailsByHeaderGuid(Simulate.String(dtcash.Rows[0]["Guid"]),CompanyID, trn);

                        }
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

        [Route("UpdateFinancingHeaderIsShowInMonthlyReports")]
        public string UpdateFinancingHeaderIsShowInMonthlyReports(
         
          string Guid, bool IsShowInMonthlyReports,
          int CompanyID
           

          )
        { 
            try
            {
                clsFinancingHeader cls = new clsFinancingHeader();
               string a=  cls.UpdateFinancingHeaderIsShowInMonthlyReports(Guid, IsShowInMonthlyReports, CompanyID,null);
                return a;
            }
            catch (Exception ex)
            {

                return "";
            }




           
         }

                [HttpPost]
        [Route("InsertFinancingHeader")]

        public string InsertFinancingHeader(DateTime voucherDate, int branchID, int CostCenterID, int BankCostCenterID,   int voucherNumber, int businessPartnerID
            , string note, decimal totalAmount, decimal downPayment, decimal netAmount
            ,  int grantor,int loanType,  int creationUserID, int companyID, decimal IntrestRate,
            bool isAmountReturned,
            int MonthsCount,int PaymentAccountID,int PaymentSubAccountID, int VendorID
            ,string ChequeName,string ChequeNumber,string ChequeNote,int PaymentMethodTypeID,int SalesManID,
            bool IsShowInMonthlyReports,
            [FromBody] string DetailsList)

        {
            try
            {

                
                
                DBFinancingHeader dbFinancingHeader = new DBFinancingHeader
                {
                    VoucherDate = voucherDate,
                    BranchID = branchID,
                    CostCenterID = CostCenterID,
                    BankCostCenterID = BankCostCenterID,
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
                    SalesManID= SalesManID,
                    IsShowInMonthlyReports = IsShowInMonthlyReports,
                };


                clsFinancingHeader clsFinancingHeader = new clsFinancingHeader();
                clsFinancingDetails clsFinancingDetails = new clsFinancingDetails();
                SqlTransaction trn; clsSQL clsSQL = new clsSQL();
                SqlConnection con = new SqlConnection(clsSQL.CreateDataBaseConnectionString(companyID));
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
                                string c = clsFinancingDetails.InsertFinancingDetails(dbFinancingHeader, details[i], A,companyID, trn);
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
                        string    JVGuid = clsJournalVoucherHeader.InsertJournalVoucherHeader(branchID, 0, Simulate.String(note),Simulate.String( maxJv), (int)clsEnum.VoucherType.Finance, companyID, voucherDate, creationUserID, "", Simulate.Integer32( DTLoanTypes.Rows[0]["id"]), trn);
                            //Insert cash Voucher 

                            DBCashVoucherHeader dbCashVoucherHeader = new DBCashVoucherHeader
                            {
                                VoucherDate = voucherDate,
                                BranchID = branchID,
                                CostCenterID = 0,
                                CashID = PaymentSubAccountID,
                                AccountID = PaymentAccountID ,
                                VoucherNo = voucherNumber,
                                Amount = netAmount,
                                JVGuid = Simulate.Guid(JVGuid),
                                Note = Simulate.String(note),
                                ManualNo = Simulate.String(ChequeNumber),
                                VoucherType = 12,//payments 
                                RelatedInvoiceGuid = Simulate.Guid(""),
                                CompanyID = companyID,
                                CreationUserID = creationUserID,
                                CreationDate = DateTime.Now,
                                ChequeName = Simulate.String(ChequeName),
                                DueDate = voucherDate,
                                ChequeNote = Simulate.String(ChequeNote),
                                PaymentMethodTypeID = Simulate.Integer32(PaymentMethodTypeID),
                                RelatedFinancingGuid = Simulate.Guid(A),
                            };
                            clsCashVoucherHeader clsCashVoucherHeader = new clsCashVoucherHeader();
                            clsCashVoucherDetails clsCashVoucherDetails = new clsCashVoucherDetails();
                            string CashVoucherHeaderGid = clsCashVoucherHeader.InsertCashVoucherHeader(dbCashVoucherHeader, trn);
                            DBCashVoucherDetails dbCashVoucherDetails = new DBCashVoucherDetails
                            {
                                AccountID= Simulate.Integer32(DTLoanTypes.Rows[0]["ReceivableAccountID"]),
                                SubAccountID= businessPartnerID,    
                                BranchID=branchID,
                                CompanyID=companyID,
                                CostCenterID=0,
                                Debit=netAmount,
                                Credit=0,
                                Total= netAmount,
                                HeaderGuid=Simulate.Guid( CashVoucherHeaderGid),
                                Note=Simulate.String( note),
                                IsUpper=true,
                                RowIndex=1,
                                VoucherType=12,

                            };
                            string c = clsCashVoucherDetails.InsertCashVoucherDetails(dbCashVoucherDetails, CashVoucherHeaderGid, trn);
                            if (c == "") {
                                IsSaved = false;
                            }
                            
                            // End of Cash Voucher


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
                                          , creationUserID, details[i].RelatedDetailsGuid, trn);
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
                                           , creationUserID,"", trn);
                                if (c1 == "")
                                    IsSaved = false;


                                string insertCredit = clsJournalVoucherDetails.InsertJournalVoucherDetails(JVGuid, details.Count + 1,PaymentAccountID, PaymentSubAccountID, 0, totalAmount
                                        , -1 * totalAmount, branchID, 0, voucherDate, Simulate.String(note), companyID
                                        , creationUserID, "",trn);
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
                            , creationUserID,"", trn);
                                if (insertProfit == "")
                                    IsSaved = false;

                                if (Simulate.Integer32(DTLoanTypes.Rows[0]["ProfitAccount"]) == 0)
                                {
                                    IsSaved = false;
                                }
                            }
                            clsFinancingHeader.UpdateFinancingHeaderJVGuid(A, JVGuid,companyID, trn);     
                                
                            if (!clsJournalVoucherHeader.CheckJVMatch(JVGuid, companyID,trn))
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
            int branchID, int CostCenterID, int BankCostCenterID,
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
            int MonthsCount, int PaymentAccountID, int PaymentSubAccountID,int VendorID
               , string ChequeName, string ChequeNumber, string ChequeNote, int PaymentMethodTypeID,
            bool IsShowInMonthlyReports,int SalesManID,
            [FromBody] string DetailsList
            
           )
        {





            try
            {
                 
                DBFinancingHeader dbFinancingHeader = new DBFinancingHeader
                {
                    VoucherDate = voucherDate,
                    BranchID = branchID,
                    CostCenterID = CostCenterID,
                    BankCostCenterID = BankCostCenterID,
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
                    IsShowInMonthlyReports = IsShowInMonthlyReports,
                    SalesManID= SalesManID,
                };


                clsFinancingHeader clsFinancingHeader = new clsFinancingHeader();
                clsFinancingDetails clsFinancingDetails = new clsFinancingDetails();
                SqlTransaction trn; clsSQL clsSQL = new clsSQL();
                SqlConnection con = new SqlConnection(clsSQL.CreateDataBaseConnectionString(companyID));
                con.Open();
                trn = con.BeginTransaction();
                string A = "";
                try
                {
                    bool IsSaved = true;

                    A = clsFinancingHeader.UpdateFinancingHeader(dbFinancingHeader, companyID,trn);
                    clsFinancingDetails.DeleteFinancingDetailsByHeaderGuid(guid,companyID, trn);

                    clsLoanTypes clsLoanTypes = new clsLoanTypes();
                    DataTable DTLoanTypes = clsLoanTypes.SelectLoanTypes(loanType, "0,1,2,3", "", "", "", companyID);
                    if (DTLoanTypes != null && DTLoanTypes.Rows.Count > 0 && Simulate.Integer32(DTLoanTypes.Rows[0]["MainTypeID"]) == 1)
                    {
                        List<DBFinancingDetails> details = JsonConvert.DeserializeObject<List<DBFinancingDetails>>(DetailsList);
                        for (int i = 0; i < details.Count; i++)
                        {

                            string c = clsFinancingDetails.InsertFinancingDetails(dbFinancingHeader, details[i], guid, companyID,trn);
                            if (c == "")
                                IsSaved = false;
                        }

                    }
                    else {
                      clsJournalVoucherHeader clsJournalVoucherHeader = new clsJournalVoucherHeader();
                        DataTable dtMaxJV = clsJournalVoucherHeader.SelectMaxJVNo("", (int)clsEnum.VoucherType.Finance, companyID, trn);
                        int maxJv = 1;
                        if (dtMaxJV != null && dtMaxJV.Rows.Count > 0)
                        {
                            maxJv = Simulate.Integer32(dtMaxJV.Rows[0][0]) + 1;
                        }
                        clsJournalVoucherHeader.UpdateJournalVoucherHeader(branchID, 0, Simulate.String(note),
                            Simulate.String(maxJv), (int)clsEnum.VoucherType.Finance, 
                            voucherDate, dbFinancingHeader.JVGuid.ToString(), modificationUserID, "", Simulate.Integer32(DTLoanTypes.Rows[0]["id"]), companyID,trn);
                        clsJournalVoucherDetails clsJournalVoucherDetails = new clsJournalVoucherDetails();
                         clsJournalVoucherDetails.DeleteJournalVoucherDetailsByParentId(dbFinancingHeader.JVGuid.ToString(),companyID, trn);



                        List<tbl_JournalVoucherDetails> details = JsonConvert.DeserializeObject<List<tbl_JournalVoucherDetails>>(DetailsList);

                        //Insert cash Voucher 

                        clsCashVoucherDetails clsCashVoucherDetails = new clsCashVoucherDetails();

                        clsCashVoucherHeader clsCashVoucherHeader = new clsCashVoucherHeader();
                        DataTable dtcash = clsCashVoucherHeader.SelectCashVoucherHeaderByGuid("", DateTime.Now.AddYears(-100), DateTime.Now.AddYears(100), 0, 0, 0, guid, trn);

                        if (dtcash != null && dtcash.Rows.Count > 0)
                        {

                            clsCashVoucherHeader.DeleteCashVoucherHeaderByGuid(Simulate.String(dtcash.Rows[0]["Guid"]),companyID, trn);
                            clsCashVoucherDetails.DeleteCashVoucherDetailsByHeaderGuid(Simulate.String(dtcash.Rows[0]["Guid"]),companyID, trn);

                        }
                        DBCashVoucherHeader dbCashVoucherHeader = new DBCashVoucherHeader
                        {
                            VoucherDate = voucherDate,
                            BranchID = branchID,
                            CostCenterID = 0,
                            CashID = PaymentSubAccountID,
                            AccountID = PaymentAccountID,
                            VoucherNo = voucherNumber,
                            Amount = netAmount,
                            JVGuid = Simulate.Guid(jVGuid),
                            Note = Simulate.String(note),
                            ManualNo = Simulate.String(ChequeNumber),
                            VoucherType = 12,//payments 
                            RelatedInvoiceGuid = Simulate.Guid(""),
                            CompanyID = companyID,
                            CreationUserID = modificationUserID,
                            CreationDate = DateTime.Now,
                            ChequeName = Simulate.String(ChequeName),
                            DueDate = voucherDate,
                            ChequeNote = Simulate.String(ChequeNote),
                            PaymentMethodTypeID = Simulate.Integer32(PaymentMethodTypeID),
                            RelatedFinancingGuid = Simulate.Guid(guid),
                        };
                       
                        string CashVoucherHeaderGid = clsCashVoucherHeader.InsertCashVoucherHeader(dbCashVoucherHeader, trn);
                        DBCashVoucherDetails dbCashVoucherDetails = new DBCashVoucherDetails
                        {
                            AccountID = Simulate.Integer32(DTLoanTypes.Rows[0]["ReceivableAccountID"]),
                            SubAccountID = businessPartnerID,
                            BranchID = branchID,
                            CompanyID = companyID,
                            CostCenterID = 0,
                            Debit = netAmount,
                            Credit = 0,
                            Total = netAmount,
                            HeaderGuid = Simulate.Guid(CashVoucherHeaderGid),
                            Note = Simulate.String(note),
                            IsUpper = true,
                            RowIndex = 1,
                            VoucherType = 12,

                        };
                        string c = clsCashVoucherDetails.InsertCashVoucherDetails(dbCashVoucherDetails, CashVoucherHeaderGid, trn);
                        if (c == "")
                        {
                            IsSaved = false;
                        }

                        // End of Cash Voucher

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
                                  , modificationUserID, details[i].RelatedDetailsGuid, trn);
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
                                       , modificationUserID, "",trn);
                            if (Simulate.Integer32(DTLoanTypes.Rows[0]["ReceivableAccountID"]) == 0)
                            {
                                IsSaved = false;
                            }
                            if (c1 == "")
                                IsSaved = false;
                            string insertCredit = clsJournalVoucherDetails.InsertJournalVoucherDetails(dbFinancingHeader.JVGuid.ToString(), details.Count + 1, PaymentAccountID, PaymentSubAccountID, 0, totalAmount
                                     , -1 * totalAmount, branchID, 0, voucherDate, Simulate.String(note), companyID
                                     , modificationUserID, "", trn);
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
                        , modificationUserID, "",trn);

                            if (Simulate.Integer32(DTLoanTypes.Rows[0]["ProfitAccount"]) == 0)
                            {
                                IsSaved = false;
                            }
                            if (insertProfit == "")
                            IsSaved = false;
                        }



                        if (!clsJournalVoucherHeader.CheckJVMatch(dbFinancingHeader.JVGuid.ToString(), companyID,trn))
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

                DataTable dtHeader = clsFinancingHeader.SelectFinancingHeaderByGuid(guid, DateTime.Now.AddYears(-100), DateTime.Now.AddYears(100), 0,0,  CompanyID,0,"-1",0);
                DataTable dtDetails = clsFinancingDetails.SelectFinancingDetailsByHeaderGuid(guid ,0, CompanyID);

                clsLoanTypes clsLoanTypes = new clsLoanTypes();
                DataTable dtLoanType = clsLoanTypes.SelectLoanTypes(Simulate.Integer32(dtHeader.Rows[0]["LoanType"]), "-1,0,1,2,3", "","","",CompanyID);
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
                        dsBusinessPartner = FillDsBusnessPartner(Simulate.Integer32(dtHeader.Rows[i]["BusinessPartnerID"]), Simulate.Integer32(dtHeader.Rows[i]["Grantor"]), CompanyID);
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

                        ds.Header.Rows[i]["SalesManAName"] = Simulate.String(dtHeader.Rows[i]["SalesManName"]);



                        clsReports clsReports = new clsReports();

                        cls_AccountSetting cls_AccountSetting = new cls_AccountSetting();
                        DataTable dtAccountSetting = cls_AccountSetting.SelectAccountSetting(0, 0, Simulate.Integer32(dtHeader.Rows[i]["CompanyID"]));
                        clsInvoiceHeader clsInvoiceHeader = new clsInvoiceHeader();
                        int CustomerAccount = clsInvoiceHeader.GetValueFromDT(dtAccountSetting, "AccountRefID", Simulate.String((int)clsEnum.AccountMainSetting.CustomerAccount), 2);


                        DataTable dtStatment = clsReports.SelectCustomerBalanceBeforeTransaction(guid,Simulate.StringToDate(dtHeader.Rows[i]["VoucherDate"]),  
                             CustomerAccount, Simulate.Integer32(dtHeader.Rows[i]["BusinessPartnerID"]), CompanyID);
                        if (dtStatment != null && dtStatment.Rows.Count > 0)
                        {

                            TotalDue = Simulate.decimal_(dtStatment.Rows[0][0]);
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



                    //
                    //if (Logo != null && Simulate.String(dtHeader.Rows[0]["SignutureGuid1"]) != "")
                    //{

                    //    Logo.Image = Simulate.StringToImg((byte[])dtHeader.Rows[0]["SignutureGuid1"]);
                    //    Report.SetParameterValue("Standerd.Logo", Simulate.StringToImg((byte[])dtHeader.Rows[0]["SignutureGuid1"]));
                    //}
                    DataTable dtSign = new DataTable();
                    clsSignuture cls = new clsSignuture();
                    if (Simulate.String(dtHeader.Rows[0]["SignutureGuid1"]) != "") { 
                      dtSign=  cls.SelectSignuture(Simulate.String( dtHeader.Rows[0]["SignutureGuid1"]),0, 0,CompanyID);
                    FastReport.PictureObject SignutureGuid1 = (FastReport.PictureObject)report.FindObject("SignutureGuid1");
                    if (dtSign !=null && dtSign.Rows.Count>0&& SignutureGuid1 != null && Simulate.String(dtSign.Rows[0]["Signuture"]) != "") {
                        try
                        {

                        SignutureGuid1.Image = Simulate.StringToImg((byte[])dtSign.Rows[0]["Signuture"]);
                        }
                        catch (Exception)
                        {

                            
                        }
                        
                    }

                    }
                    if (Simulate.String(dtHeader.Rows[0]["SignutureGuid2"]) != "")
                    {
                        dtSign = cls.SelectSignuture(Simulate.String(dtHeader.Rows[0]["SignutureGuid2"]), 0, 0, CompanyID);
                    FastReport.PictureObject SignutureGuid2 = (FastReport.PictureObject)report.FindObject("SignutureGuid2");
                    if (dtSign != null && dtSign.Rows.Count > 0 && SignutureGuid2 != null && Simulate.String(dtSign.Rows[0]["Signuture"]) != "")
                    {
                        try
                        {
                        SignutureGuid2.Image = Simulate.StringToImg((byte[])dtSign.Rows[0]["Signuture"]);

                        }
                        catch (Exception)
                        {


                        }
                    }
                    }
                    if (Simulate.String(dtHeader.Rows[0]["SignutureGuid3"]) != "")
                    {
                        dtSign = cls.SelectSignuture(Simulate.String(dtHeader.Rows[0]["SignutureGuid3"]),0, 0, CompanyID);
                    FastReport.PictureObject SignutureGuid3 = (FastReport.PictureObject)report.FindObject("SignutureGuid3");
                    if (dtSign != null && dtSign.Rows.Count > 0 && SignutureGuid3 != null && Simulate.String(dtSign.Rows[0]["Signuture"]) != "")
                    {
                        try
                        {
                        SignutureGuid3.Image = Simulate.StringToImg((byte[])dtSign.Rows[0]["Signuture"]);

                        }
                        catch (Exception)
                        {


                        }
                    }
                    }
                    if (Simulate.String(dtHeader.Rows[0]["SignutureGuid4"]) != "")
                    {
                        dtSign = cls.SelectSignuture(Simulate.String(dtHeader.Rows[0]["SignutureGuid4"]),0, 0, CompanyID);
                    FastReport.PictureObject SignutureGuid4 = (FastReport.PictureObject)report.FindObject("SignutureGuid4");
                    if (dtSign != null && dtSign.Rows.Count > 0 && SignutureGuid4 != null && Simulate.String(dtSign.Rows[0]["Signuture"]) != "")
                    {
                        try
                        {
                        SignutureGuid4.Image = Simulate.StringToImg((byte[])dtSign.Rows[0]["Signuture"]);

                        }
                        catch (Exception)
                        {


                        }
                    }
                    }

                    clsEmployee clsEmployee= new clsEmployee();
                    dtSign = clsEmployee.SelectEmployee(Simulate.Integer32(dtHeader.Rows[0]["SalesmanID"]), "", "", "", "", CompanyID, -1);

                    //if (CompanyID == 1022)
                    //{

                    //   
                    //}
                    //else {

                    // dtSign = clsEmployee.SelectEmployee(Simulate.Integer32(dtHeader.Rows[0]["SalesmanID"]), "", "", "", "", 0, -1);

                    //}
                  
                        FastReport.PictureObject SignutureGuid5 = (FastReport.PictureObject)report.FindObject("SignutureGuid5");
                    
                    if (dtSign != null && dtSign.Rows.Count > 0 && SignutureGuid5 != null &&   Simulate.String(dtSign.Rows[0]["Signuture"]) != "")
                    {
                        try
                        {
                            SignutureGuid5.Image = Simulate.StringToImg((byte[])dtSign.Rows[0]["Signuture"]);
                 
                        }
                        catch (Exception)
                        {

                          
                        }


                        }
                     
                    dtSign = clsEmployee.SelectEmployee(1111, "", "", "", "", CompanyID, -1);
                  
                        FastReport.PictureObject SignutureGuid6 = (FastReport.PictureObject)report.FindObject("SignutureGuid6");
                    if (dtSign != null && dtSign.Rows.Count > 0 && SignutureGuid6 != null && Simulate.String(dtSign.Rows[0]["Signuture"]) != "")
                    {
                        try
                        {
                  
                            SignutureGuid6.Image = Simulate.StringToImg((byte[])dtSign.Rows[0]["Signuture"]);
                        }
                        catch (Exception)
                        {


                        }


                        }
                     
                    FastreportStanderdParameters(report, UserId, CompanyID);



                    report.Prepare();

                    return FastreporttoPDF(report);

                } else if (Simulate.Integer32(dtLoanType.Rows[0]["MainTypeID"])==2) {//Loan



                    clsJournalVoucherDetails clsJVDetails = new clsJournalVoucherDetails();
                    dsJVDetails dsJVDetails = clsJVDetails.SelectJournalVoucherDetailsByParentIdForPrint(CompanyID,Simulate.String(dtHeader.Rows[0]["JVGuid"]), 0, 0);



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
                    dsJVDetails dsJVDetails = clsJVDetails.SelectJournalVoucherDetailsByParentIdForPrint(CompanyID,Simulate.String(dtHeader.Rows[0]["JVGuid"]), 0, 0);



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
                DataTable dtHeader = clsFinancingHeader.SelectFinancingHeaderByGuid(guid, DateTime.Now.AddYears(-100), DateTime.Now.AddYears(100), 0,0,CompanyID, 0, "-1",0);
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
                        dsBusinessPartner = FillDsBusnessPartner(Simulate.Integer32(dtHeader.Rows[i]["BusinessPartnerID"]), Simulate.Integer32(dtHeader.Rows[i]["Grantor"]), CompanyID);
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


                        DataTable dtStatment = clsReports.SelectCustomerBalanceBeforeTransaction(guid, Simulate.StringToDate(dtHeader.Rows[i]["VoucherDate"]),
                            CustomerAccount, Simulate.Integer32(dtHeader.Rows[i]["BusinessPartnerID"]), CompanyID);
                        if (dtStatment != null && dtStatment.Rows.Count > 0)
                        {

                            TotalDue = Simulate.decimal_(dtStatment.Rows[0][0]);
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





                clsSignuture cls = new clsSignuture();

              DataTable  dtSign = cls.SelectSignuture(Simulate.String(dtHeader.Rows[0]["SignutureGuid4"]),0, 0, 0);
                FastReport.PictureObject SignutureGuid4 = (FastReport.PictureObject)report.FindObject("SignutureGuid4");
                if (dtSign != null && dtSign.Rows.Count > 0 && SignutureGuid4 != null && Simulate.String(dtSign.Rows[0]["Signuture"]) != "")
                {
                    try
                    {
                        SignutureGuid4.Image = Simulate.StringToImg((byte[])dtSign.Rows[0]["Signuture"]);

                    }
                    catch (Exception)
                    {


                    }
                }
                FastreportStanderdParameters(report, UserId, CompanyID);


                report.Prepare();

                return FastreporttoPDF(report);



            }
            catch (Exception ex)
            {

                return Json(ex);
            }

        }
        dsBusinessPartner FillDsBusnessPartner(int BusnessPartnerID, int GrantoID,int CompanyID)
        {
            try
            {
                dsBusinessPartner ds =new dsBusinessPartner();
                clsBusinessPartner clsBusinessPartner = new clsBusinessPartner();
               DataTable dtBusinessPartner= clsBusinessPartner.SelectBusinessPartner(BusnessPartnerID, 0, "", "", -1, CompanyID);
                DataTable dtGrantoID = clsBusinessPartner.SelectBusinessPartner(GrantoID, 0, "", "", -1, CompanyID);
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


                if (GrantoID>0&&dtGrantoID != null && dtGrantoID.Rows.Count > 0)
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
                 DataTable dt = cls.ExecuteQueryStatement(a, cls.CreateDataBaseConnectionString(CompanyID));
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
                    dt = cls.SelectLoanReportRJ(Date1, Date2, ARAccountID, UserId, CompanyID);
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
             DateTime d2=   Simulate.StringToDate(Date2);
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
                    ColumnType.Add("double");
                    ColumnType.Add("string");
                    ColumnType.Add("double");


                    ColumnType.Add("string");
                    
                    ColumnType.Add("string");
                    ColumnType.Add("string");
                    ColumnType.Add("string");
                    dtlist.Add(dt);
                    dtName.Add("ts_" + d2.Month.ToString() + d2.Year.ToString());
                }
                else if (Type == "Loans")
                {
                    DataTable dt = cls.SelectLoanReportRJ(Date1, Date2,ARAccountID, UserId, CompanyID); dt.Columns.RemoveAt(0);
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
                    ColumnType.Add("double"); 
                    ColumnType.Add("string");
                    ColumnType.Add("double");

                   
                    dtlist.Add(dt);
                    dtName.Add("tl_" + d2.Month.ToString() + d2.Year.ToString());
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
                            if (dt.Rows.Count > 0) { 
                            

                          
                            dtlist.Add(dt);
                            dtName.Add(Simulate.String( dtStatus.Rows[ii]["Code"])+ " "+ Simulate.String(dttype.Rows[i]["Code"]));
                            }
                        }

                    }
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
                    ColumnType.Add("int");




                }
           
               


                return FastreporttoCSV(dtlist, dtName, ColumnType);
            }
            catch (Exception ex)
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
        public bool DeleteUserAuthorizationByUserID(int UserId,int CompanyID)
        {
            try
            {
                clsUserAuthorization clsUserAuthorization = new clsUserAuthorization();
               
                
                
                bool A = clsUserAuthorization.DeleteUserAuthorizationByUserID(UserId, CompanyID);
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
                SqlConnection con = new SqlConnection(clsSQL.CreateDataBaseConnectionString(CompanyID));
                con.Open();
                trn = con.BeginTransaction();
                List<DBUserAuthrization> details = JsonConvert.DeserializeObject<List<DBUserAuthrization>>(DetailsList);

                DBUserAuthrization DBUserAuthrization;
                clsUserAuthorization clsUserAuthorization = new clsUserAuthorization();
                clsUserAuthorization.DeleteUserAuthorizationByUserID(details[0].UserID, CompanyID);
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
        public string SelectForms(int FormID, int CompanyID)
        {
            try
            {
                clsSQL clsSQL=new clsSQL();

                DataTable dt = clsSQL.ExecuteQueryStatement("select * from tbl_Forms", clsSQL.CreateDataBaseConnectionString(CompanyID));
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
        public bool DeleteUserAuthorizationModelsByUserID(int UserId,int CompanyID)
        {
            try
            {
                clsUserAuthorizationModels clsUserAuthorizationModels = new clsUserAuthorizationModels();



                bool A = clsUserAuthorizationModels.DeleteUserAuthorizationModelsByUserID(UserId, CompanyID);
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
                SqlConnection con = new SqlConnection( clsSQL.CreateDataBaseConnectionString(CompanyID));
                con.Open();
                trn = con.BeginTransaction();
                List<DBUserAuthrizationModels> details = JsonConvert.DeserializeObject<List<DBUserAuthrizationModels>>(DetailsList);

                DBUserAuthrizationModels DBUserAuthrizationModels;
                clsUserAuthorizationModels clsUserAuthorizationModels = new clsUserAuthorizationModels();
                clsUserAuthorizationModels.DeleteUserAuthorizationModelsByUserID(details[0].UserID, CompanyID);
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
        public bool DeleteLoanTypesByID(int ID,int CompanyID)
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
                bool A = clsLoanTypes.DeleteLoanTypesByID(ID, CompanyID);
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
            int CreationUserId,bool IsShowInMonthlyReports)
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
                    CreationUserId, IsShowInMonthlyReports
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
            int ModificationUserId,bool IsShowInMonthlyReports,int CompanyID)
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
             ModificationUserId, IsShowInMonthlyReports, CompanyID);
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
                DataTable dt = clsReconciliation.SelectReconciliationByJVDetailsGuid(VoucherNumber, JVDetailsGuid, CompanyID, "00000000-0000-0000-0000-000000000000");
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
        [Route("SelectReconciliationPaymentDetails")]
        public string SelectReconciliationPaymentDetails( string FGuid,int CompanyID)
        {
            try
            {
                SqlParameter[] prm =
                 {
                    new SqlParameter("@FGuid", SqlDbType.UniqueIdentifier) { Value = Simulate.Guid( FGuid ) },
              };
                string a = @"select * from (select   tbl_JournalVoucherDetails.ParentGuid JVGuid,
tbl_Reconciliation.VoucherNumber as ReconciliationVoucherNumber,
 
(select top 1 tt.AName from tbl_Reconciliation mm 
left join tbl_JournalVoucherDetails aa on aa.Guid = mm.JVDetailsGuid
left join tbl_JournalVoucherHeader hh on hh.Guid = aa.ParentGuid
left join tbl_JournalVoucherTypes tt on tt.id = hh.JVTypeID
where mm.VoucherNumber=tbl_Reconciliation.VoucherNumber and mm.Amount<0) JournalVoucherTypesName ,
(select top 1 hh.JVNumber from tbl_Reconciliation mm 
left join tbl_JournalVoucherDetails aa on aa.Guid = mm.JVDetailsGuid
left join tbl_JournalVoucherHeader hh on hh.Guid = aa.ParentGuid
left join tbl_JournalVoucherTypes tt on tt.id = hh.JVTypeID
where mm.VoucherNumber=tbl_Reconciliation.VoucherNumber and mm.Amount<0) JVNumber ,
(select top 1 hh.VoucherDate from tbl_Reconciliation mm 
left join tbl_JournalVoucherDetails aa on aa.Guid = mm.JVDetailsGuid
left join tbl_JournalVoucherHeader hh on hh.Guid = aa.ParentGuid
left join tbl_JournalVoucherTypes tt on tt.id = hh.JVTypeID
where mm.VoucherNumber=tbl_Reconciliation.VoucherNumber and mm.Amount<0) VoucherDate ,
--tbl_JournalVoucherHeader.JVNumber,
--tbl_JournalVoucherHeader.VoucherDate ,
(select sum(Debit) from tbl_JournalVoucherDetails where ParentGuid=tbl_JournalVoucherHeader.Guid) as TotalJV,
tbl_JournalVoucherDetails.Total LineTotal,
   tbl_Reconciliation.Amount ReconciledAmount
,tbl_Reconciliation.JVDetailsGuid 
 from tbl_JournalVoucherDetails 
 left join tbl_Reconciliation on tbl_Reconciliation.JVDetailsGuid= tbl_JournalVoucherDetails.Guid 
 left join tbl_JournalVoucherHeader on tbl_JournalVoucherDetails.ParentGuid=tbl_JournalVoucherHeader.Guid
 where tbl_Reconciliation.Amount<>0and   ParentGuid in (
select distinct JVGuid from (
select JVGuid from tbl_FinancingDetails where HeaderGuid = 
@FGuid
union all 
select JVGuid  from tbl_FinancingHeader where Guid = 
@FGuid) as q)
and tbl_JournalVoucherDetails.guid in (select JVDetailsGuid from tbl_Reconciliation )
) as qaaa
order by qaaa.voucherdate asc ";

                clsSQL clsSQL = new clsSQL();
                DataTable dt = clsSQL.ExecuteQueryStatement(a, clsSQL.CreateDataBaseConnectionString(CompanyID), prm);
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
        public string SelectReconciliationDetails(int AccountID,int SubAccountID,int VoucherNumber,  int CompanyID,String TransactionGuid)
        {
            try
            {
                clsSQL clsSQL = new clsSQL();
                clsReconciliation clsReconciliation = new clsReconciliation();
                if (VoucherNumber > 0 && AccountID == 0) { 
                     
                DataTable dt1 = clsReconciliation.SelectReconciliationByJVDetailsGuid(VoucherNumber, TransactionGuid, CompanyID, TransactionGuid);
                    if (dt1 != null && dt1.Rows.Count > 0) {
                        DataTable dt2 = clsSQL.ExecuteQueryStatement("select * from tbl_journalvoucherdetails where companyid ='"+ CompanyID + "'  and guid ='"+ dt1.Rows[0]["jvdetailsguid"] + "'", clsSQL.CreateDataBaseConnectionString(CompanyID));
                        AccountID =Simulate.Integer32(dt2.Rows[0]["Accountid"]) ;
                        SubAccountID = Simulate.Integer32(dt2.Rows[0]["SubAccountid"]);
                    }
                }
            
                DataTable dt = clsReconciliation.SelectReconciliationDetails(AccountID, SubAccountID , VoucherNumber,CompanyID, TransactionGuid);
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
        [Route("SelectUnReconciledAmount")]
        public string SelectUnReconciledAmount(int CompanyID,int AccountID)
        {
            try
            {

                SqlParameter[] prm =
               {
                         new SqlParameter("@AccountID", SqlDbType.Int) { Value = AccountID },
                     new SqlParameter("@CompanyID", SqlDbType.Int) { Value = CompanyID },
                 };

                string a = @"  select * from (
select AccountID,ID as BusinessPartnerID,EmpCode,AName,Total 
 from tbl_JournalVoucherDetails
 left join tbl_BusinessPartner on tbl_BusinessPartner.ID = SubAccountID
 where 
 Total<0 and
 
  AccountID =@Accountid 
 and tbl_JournalVoucherDetails.CompanyID =@CompanyID
 and SubAccountID >0
 and (isnull((select sum(Amount) from tbl_Reconciliation where JVDetailsGuid = tbl_JournalVoucherDetails.Guid),0)<>Total)
 
 ) as q  order by q.AName";
                clsSQL clssql = new clsSQL();
                DataTable dt = clssql.ExecuteQueryStatement(a, clssql.CreateDataBaseConnectionString(CompanyID), prm);
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
        [Route("SelectAllReconciliations")]
        public string SelectAllReconciliations(int CompanyID)
        {
            try
            {
                clsReconciliation clsReconciliation = new clsReconciliation();
                DataTable dt = clsReconciliation.SelectAllReconciliations(CompanyID);
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
        [Route("DeleteReconciliationByVoucherNumber")]
        public bool DeleteReconciliationByVoucherNumber(int VoucherNumber, int CompanyID)
        {
            try
            {
                clsReconciliation clsReconciliation = new clsReconciliation();

                bool A = clsReconciliation.DeleteReconciliationByVoucherNumber(VoucherNumber, CompanyID);
                return A;
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
                SqlConnection con = new SqlConnection(clsSQL.CreateDataBaseConnectionString(CompanyID));
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
                DataTable dt = clsReconciliation.SelectAccountsForAutoReconciliation(AccountID,  SubAccountID,  CompanyID,0,trn);
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
                                if (a == "")
                                {
                                    isSaved = false;
                                }
                            }
                           else if (TotalCredit>TotalDebit && Simulate.Val(dt.Rows[i]["Debit"]) > 0)
                            {
                                var a = clsReconciliation.InsertReconciliation(VoucherNumber, Simulate.String(dt.Rows[i]["Guid"]), Simulate.decimal_(dt.Rows[i]["Total"]), CompanyID, CreationUserId, Simulate.String(dt.Rows[i]["Guid"]), trn);
                                if (a == "")
                                {
                                    isSaved = false;
                                }
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
                                    if (a == "")
                                    {
                                        isSaved = false;
                                    }

                                    break;
                                }
                                else {
                                    var a = clsReconciliation.InsertReconciliation(VoucherNumber, Simulate.String(dt.Rows[i]["Guid"]), Simulate.decimal_(dt.Rows[i]["Total"]) - Simulate.decimal_(dt.Rows[i]["reconciled"]), CompanyID, CreationUserId, Simulate.String(dt.Rows[i]["Guid"]), trn);
                                    if (a == "")
                                    {
                                        isSaved = false;
                                    }

                                }

                            }
                        
                        }else if (TotalDebit > TotalCredit)
                        {
                            double RemainingAmount = TotalCredit;
                            for (int i = 0; i < dt.Rows.Count; i++)
                            {
                                RemainingAmount = RemainingAmount - Simulate.Val(dt.Rows[i]["Debit"]) + Simulate.Val(dt.Rows[i]["reconciled"]);
                                if (RemainingAmount <= 0 && Simulate.Val(dt.Rows[i]["Debit"]) > 0)
                                {
                                    var a = clsReconciliation.InsertReconciliation(VoucherNumber, Simulate.String(dt.Rows[i]["Guid"]), Simulate.decimal_(RemainingAmount + Simulate.Val(dt.Rows[i]["Debit"])) - Simulate.decimal_(dt.Rows[i]["reconciled"]), CompanyID, CreationUserId, Simulate.String(dt.Rows[i]["Guid"]), trn);
                                    if (a == "") {
                                        isSaved = false;
                                    }

                                    break;
                                }
                                else if (Simulate.Val(dt.Rows[i]["Debit"]) > 0)
                                {
                                    var a = clsReconciliation.InsertReconciliation(VoucherNumber, Simulate.String(dt.Rows[i]["Guid"]), Simulate.decimal_(dt.Rows[i]["Total"]) - Simulate.decimal_(dt.Rows[i]["reconciled"]), CompanyID, CreationUserId, Simulate.String(dt.Rows[i]["Guid"]), trn);
                                    if (a == "")
                                    {
                                        isSaved = false;
                                    }

                                }

                            }



                        }
                     

                }
               DataTable dt1=     clsReconciliation.SelectReconciliationByJVDetailsGuid(VoucherNumber, "", 0, "00000000-0000-0000-0000-000000000000", trn);
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
        public string SelectLoanScheduling(int AccountID, int SubAccountID,   int CompanyID,string financingHeaderGuid)
        {
            try
            {
                clsReconciliation clsReconciliation = new clsReconciliation();
                DataTable dt = clsReconciliation.SelectLoanScheduling(AccountID, SubAccountID,   CompanyID, financingHeaderGuid);
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
        [Route("SelectEmployeesLoansExcel")]
        public ActionResult SelectEmployeesLoansExcel(DateTime Date1, DateTime Date2, int accountid, int BusinessPartnerID, int CompanyID,bool HideZeroBalances)
        {
            try
            {
                clsFinancingHeader clsFinancingHeader = new clsFinancingHeader();
                DataTable dt = clsFinancingHeader.SelectEmployeesLoans(Date1, Date2, accountid, BusinessPartnerID, CompanyID, HideZeroBalances);

                List<String> ColumnType = new List<String>();
                List<DataTable> dtlist = new List<DataTable>();
                List<String> dtName = new List<String>();
                clsFinancingHeader cls = new clsFinancingHeader();
                dtName.Add("Report");


                ColumnType.Add("string");
                ColumnType.Add("string");
                ColumnType.Add("string");
                ColumnType.Add("string");
                ColumnType.Add("string");
                ColumnType.Add("string");
                ColumnType.Add("string");
                ColumnType.Add("int");
                ColumnType.Add("int");
                ColumnType.Add("int");
                ColumnType.Add("int");
                ColumnType.Add("string");
                ColumnType.Add("string");
                ColumnType.Add("int");
                ColumnType.Add("int");
                dt.Columns.RemoveAt(0);

                dt.Columns.RemoveAt(1);
                dt.Columns.RemoveAt(2);
                dt.Columns[0].ColumnName = "نوع القرض";
                dt.Columns[1].ColumnName = "رقم السند";
                dt.Columns[2].ColumnName = "العميل";
                dt.Columns[3].ColumnName = "الرقم الوظيفي";
                dt.Columns[4].ColumnName = "كود القرض";
                dt.Columns[5].ColumnName = "التاريخ";
                dt.Columns[6].ColumnName = "الملاحظات";
                dt.Columns[7].ColumnName = "إجمالي المبلغ";
                dt.Columns[8].ColumnName = "القسط";
                dt.Columns[9].ColumnName = "المدفوع";
                dt.Columns[10].ColumnName = "المده";
                dt.Columns[11].ColumnName = "تاريخ اول قسط";
                dt.Columns[12].ColumnName = "تاريخ اخر قسط";
                dt.Columns[13].ColumnName = "المستحق";
                dt.Columns[14].ColumnName = "المجدول";
                dtlist.Add(dt);






                return FastreporttoCSV(dtlist, dtName, ColumnType);














            }
            catch (Exception)
            {

                throw;
            }




        }
        [HttpPost]
        [Route("InsertLoanScheduling")]

        public string InsertLoanScheduling(int BranchID, int CostCenterID, string Notes, string JVNumber, int JVTypeID, [FromBody] string DetailsList, int CompanyID, DateTime VoucherDate, int CreationUserId, string financingHeaderGuid,int RelatedLoanTypeID)

        {
            try
            {
                clsSQL clsSQL = new clsSQL();
              


            var JVGuid=    InsertJournalVoucherHeader(BranchID,   CostCenterID,   Notes,   JVNumber,   JVTypeID,   DetailsList,  CompanyID,  VoucherDate,  CreationUserId, financingHeaderGuid, RelatedLoanTypeID);
                 List<tbl_JournalVoucherDetails> details = JsonConvert.DeserializeObject<List<tbl_JournalVoucherDetails>>(DetailsList);
                clsJournalVoucherDetails clsJournalVoucherDetails = new clsJournalVoucherDetails();
               DataTable dt = clsJournalVoucherDetails.SelectJournalVoucherDetailsByParentId(JVGuid,0,0,0,0,0, CompanyID);
                SqlTransaction trn;
                SqlConnection con = new SqlConnection( clsSQL.CreateDataBaseConnectionString(CompanyID));
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
                            a.Amount = details[i].Total*-1;
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
                cls.ExecuteNonQueryStatement(A, cls.CreateDataBaseConnectionString(CompanyID), prm);
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
                SqlConnection con = new SqlConnection(clsSQL.CreateDataBaseConnectionString(CompanyID));
                con.Open();
                trn = con.BeginTransaction();
                String A = "";
                bool IsSaved = true;
                try
                {
                       if (guid != ""&&guid != "00000000-0000-0000-0000-000000000000") {
                        clsReconciliation.DeleteReconciliationByTransactionGuid(guid, CompanyID, trn);
                    }
                    if (VoucherNumber > 0)
                    {
                        clsReconciliation.DeleteReconciliationByVoucherNumber(VoucherNumber, CompanyID, trn);
                    }

                    if (VoucherNumber == 0) {
                     //string NewGuid =  Simulate.String( Guid.NewGuid());
                    DataTable dtMaxNUmber = clsReconciliation.SelectReconciliationMaxNumber(CompanyID,trn);
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
                              );
                        if (A == "")
                            IsSaved = false;

                    }
                    //test total = 0 
                    DataTable dt1 = clsReconciliation.SelectReconciliationByJVDetailsGuid(VoucherNumber, "", 0, "00000000-0000-0000-0000-000000000000", trn);
                    string sum = dt1.Compute("Sum(Amount)", "").ToString();
                    if (Simulate.Val(sum) == 0) {   } else { IsSaved = false; }


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
        public string SelectBusinessPartnerBalances(DateTime Date, string Accounts, int UserID, int CompanyID,bool withZeroAmount)
        {
            try
            {
                clsReports clsReports = new clsReports();
                DataTable dt = clsReports.SelectBusinessPartnerBalances(Date, Accounts, CompanyID, withZeroAmount);
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
        public IActionResult SelectBusinessPartnerBalancesPDF(DateTime Date , string Accounts, int UserID, int CompanyID,bool withZeroAmount)
        {
            try
            {


                FastReport.Utils.Config.WebMode = true;
                clsReports clsReports = new clsReports();
                DataTable dt = clsReports.SelectBusinessPartnerBalances(Date,  Accounts, CompanyID, withZeroAmount);




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
                        ds.BusinessPartnerReports.Rows[i]["EMPCode"] = Simulate.String(dt.Rows[i]["EMPCode"]); 

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
        [Route("SelectBusinessPartnerBalancesExcel")]
        public ActionResult SelectBusinessPartnerBalancesExcel(DateTime Date, string Accounts, int UserID, int CompanyID, bool withZeroAmount)
        {
            try
            {
                clsReports clsReports = new clsReports();
                DataTable dt = clsReports.SelectBusinessPartnerBalances(Date, Accounts, CompanyID, withZeroAmount);



                List<String> ColumnType = new List<String>();
                List<DataTable> dtlist = new List<DataTable>();
                List<String> dtName = new List<String>();
                clsFinancingHeader cls = new clsFinancingHeader();
                dtName.Add("Report");

                

                    ColumnType.Add("int");
                    ColumnType.Add("string");
                    ColumnType.Add("string");
                    ColumnType.Add("int");
                    ColumnType.Add("int");
                    ColumnType.Add("int");
                    ColumnType.Add("int");



                dt.Columns[0].ColumnName = "الرقم";
                dt.Columns[1].ColumnName = "الإسم";
                dt.Columns[2].ColumnName = "الحساب";
                dt.Columns[3].ColumnName = "الرقم الوظيفي";
                dt.Columns[4].ColumnName = "المجموع";
                dt.Columns[5].ColumnName = "المستحق";
                dtlist.Add(dt);
                 
               




                return FastreporttoCSV(dtlist, dtName, ColumnType);
            }
            catch (Exception ex)
            {

                throw;
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
        public bool DeleteSubscriptionsByID(int ID,int CompanyID)
        {
            try
            {

                clsSubscriptions clsSubscriptions = new clsSubscriptions();
                bool A = clsSubscriptions.DeleteSubscriptionsByID(ID, CompanyID);
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
                DataTable dtLoanType = clsLoanTypes.SelectLoanTypes(0, "-1,0,1,2,3", "", "", "", companyID);
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
        public bool DeleteReportingTypeNodesByID(int ID,int CompanyID)
        {
            try
            {

                clsReportingTypeNodes clsReportingTypeNodes = new clsReportingTypeNodes();
                bool A = clsReportingTypeNodes.DeleteReportingTypeNodesByID(ID, CompanyID);
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
        public int UpdateReportingTypeNodes(int ID, string AName, string EName, int ReportingTypeID, int ParentID, int ModificationUserId,int CompanyID)
        {
            try
            {
                clsReportingTypeNodes clsReportingTypeNodes = new clsReportingTypeNodes();
                int A = clsReportingTypeNodes.UpdateReportingTypeNodes(ID, AName, EName, ReportingTypeID, ParentID, ModificationUserId, CompanyID);
                return A;
            }
            catch (Exception)
            {

                throw;
            }

        }

        #endregion

        #region MyRegion
        [HttpPost]
        [Route("ExportListToExcel")]
        public ActionResult ExportListToExcel( string CompanyID, [FromBody] JsonElement jsonData, [FromQuery] List<String> ColumnAName, [FromQuery] List<String> ColumnEName, [FromQuery] List<String> ColumnType)
        {
            string jsonString = jsonData.GetRawText();
            //List<Dictionary<string, object>> dataItems = System.Text.Json.JsonSerializer.Deserialize<List<Dictionary<string, object>>>(jsonString);

            Dictionary<string, Dictionary<string, object>> dataItems = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, Dictionary<string, object>>>(jsonString);

            DataTable dataTable = ConvertToDataTable(dataItems);

            DataTable dt = new DataTable();
            for (int i = 0; i < ColumnAName.Count; i++)
            {
                dt.Columns.Add(ColumnAName[i]);
            }
            for (int i = 0; i < dataTable.Rows.Count; i++)
            {
                dt.Rows.Add();
                for (int ii = 0; ii < ColumnEName.Count; ii++)
                {
                    dt.Rows[i][ColumnAName[ii]] = dataTable.Rows[i][ColumnEName[ii]];
                }
            }

            List<DataTable> dtlist =new List<DataTable>();
            dtlist.Add(dt);
            List<String> dtSheetName = new List<String>();
            dtSheetName.Add("Sheet1");
       
            return FastreporttoCSV(dtlist, dtSheetName, ColumnType);
        }

        private DataTable ConvertToDataTable(Dictionary<string, Dictionary<string, object>> dataItems)
        {
            DataTable dataTable = new DataTable();

            // Assuming first dictionary determines the columns structure
            if (dataItems.Count > 0)
            {
                foreach (var column in dataItems["row_0"])
                {
                    dataTable.Columns.Add(column.Key, typeof(object)); // Use typeof according to your data type needs
                }

                foreach (var item in dataItems)
                {
                    DataRow row = dataTable.NewRow();
                    foreach (var column in item.Value)
                    {
                        row[column.Key] = column.Value;
                    }
                    dataTable.Rows.Add(row);
                }
            }

            return dataTable;
        }
        #endregion


        #region Signuture


        [HttpGet]
        [Route("SelectSignuture")]
        public string SelectSignuture(string Guid, int IsOpen, int CreationUserID, int CompanyID)
        {
            try
            {
                clsSignuture clsSignuture = new clsSignuture();
                DataTable dt = clsSignuture.SelectSignuture( Guid,  IsOpen, CreationUserID, CompanyID);
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
        [Route("DeleteSignutureByGuid")]
        public bool DeleteSignutureByGuid(string Guid,int CompanyID)
        {
            try
            {
                clsSignuture clsSignuture = new clsSignuture();
              
                bool A = clsSignuture.DeleteSignutureByGuid(Guid, CompanyID);
                return A;
            }
            catch (Exception  ) 
            {

                return false;
            }

        }
        [HttpPost]
        [Route("InsertSignuture")]
        public string InsertSignuture([FromBody] JsonElement data, string SourceGuid, int VoucherType, bool IsOpen, int CompanyID, int CreationUserId,string TableName,string ColumnName)
        {
            try
            {
                 
                var Terms = data.GetProperty("Terms").GetString();
                var SignutureText = data.GetProperty("Signuture").GetString();

                 
              
              

                byte[] Signuturea = new Byte[64];
                if (SignutureText != null && SignutureText.Length > 0)
                {
                      Signuturea = Convert.FromBase64String(SignutureText);
                }
                clsSignuture clsSignuture = new clsSignuture();
                string A = clsSignuture.InsertSignuture(Signuturea, Simulate.String(SourceGuid), Simulate.Integer32(VoucherType), IsOpen, CompanyID, CreationUserId, Terms);
               
                if (A != null )
                {

                    clsSQL cls = new clsSQL();
                    cls.ExecuteNonQueryStatement("update "+ TableName+" set "+ColumnName+" = '"+A +"' where guid = '"+ SourceGuid + "'", cls.CreateDataBaseConnectionString(CompanyID));

                }

                return A;
            }
            catch (Exception ex)
            {

                throw;
            }

        }
        [HttpPost]
        [Route("UpdateSignuture")]
        public bool UpdateSignuture([FromBody] JsonElement data, string Guid, bool IsOpen,  int ModificationUserId,int CompanyID)
        {
            try
            {
                var SignutureText = data.GetProperty("Signuture").GetString();
                byte[] Signuturea = new Byte[64];
                if (SignutureText != null && SignutureText.Length > 0)
                {
                    Signuturea = Convert.FromBase64String(SignutureText);
                }
                clsSignuture clsSignuture = new clsSignuture();
                int A = clsSignuture.UpdateSignuture(Simulate.String(Guid), Simulate.Bool(IsOpen), Signuturea,   ModificationUserId, CompanyID);
                if (A == 0) {
                    return false;
                } else {

                    return true;
                }
              
            }
            catch (Exception)
            {

                return false;
            }

        }
        #endregion
    }
}

 