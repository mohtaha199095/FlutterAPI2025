using ClosedXML.Excel;
using DocumentFormat.OpenXml.Bibliography;
using DocumentFormat.OpenXml.EMMA;
using DocumentFormat.OpenXml.ExtendedProperties;
using DocumentFormat.OpenXml.Office2010.Excel;
using DocumentFormat.OpenXml.Office2010.ExcelAc;
using DocumentFormat.OpenXml.Presentation;
using DocumentFormat.OpenXml.Spreadsheet;
using DocumentFormat.OpenXml.Wordprocessing;
using FastReport;
using FastReport.Barcode;
using FastReport.Editor;
using FastReport.Export;
using FastReport.Export.PdfSimple;
 
using FastReport.Export.PdfSimple.PdfCore;
using FastReport.Format;
using FastReport.Table;
using FastReport.Utils;
using FastReport.Web;
using J2N.Text;
using Microsoft.AspNetCore.Http;
 
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.CodeAnalysis.Operations;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
 
using Microsoft.VisualBasic;
using Nancy.ModelBinding.DefaultBodyDeserializers;
using Newtonsoft.Json;
using PuppeteerSharp;
using Swashbuckle.AspNetCore.SwaggerGen; 
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Data;
using System.Drawing;
using System.Dynamic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Reflection.PortableExecutable;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
using WebApplication2.cls;
using WebApplication2.cls.Reports;
using WebApplication2.DataBaseTable;
using WebApplication2.DataSet;
using WebApplication2.MainClasses;
using static WebApplication2.MainClasses.clsEnum;



namespace WebApplication2.Controllers
{
    // [EnableCors]
    [ApiController]
    [Route("[controller]")]
    public class Main : Controller
    {
       
        public IActionResult Index()
        {
            string a = "asckd";
            return Json(a);
        }

        #region Admin diagnostics (owner tools — requires AdminLogin:Enabled in appsettings)

        [HttpGet]
        [Route("AdminCheckLogin")]
        public IActionResult AdminCheckLogin(string UserName, string Password, string Email)
        {
            var configuration = HttpContext.RequestServices.GetService<IConfiguration>();
            if (!clsAdminLogin.IsEnabled(configuration))
                return StatusCode(403, new { ok = false, message = "Admin login is disabled." });

            return Json(new
            {
                ok = false,
                requiresOtp = true,
                message = "Admin sign-in requires password and email verification. Use the updated admin login screen."
            });
        }

        [HttpPost]
        [Route("AdminRequestLogin")]
        public IActionResult AdminRequestLogin(string UserName, string Password, string Email)
        {
            var configuration = HttpContext.RequestServices.GetService<IConfiguration>();
            var environment = HttpContext.RequestServices.GetService<IHostEnvironment>();
            var logger = HttpContext.RequestServices.GetService<ILogger<Main>>();
            string clientIp = clsAdminAuthHelper.ReadClientIp(Request);
            string guardKey = clsAdminLoginGuard.BuildKey(Simulate.String(UserName), clientIp);

            if (!clsAdminLogin.IsEnabled(configuration))
                return StatusCode(403, new { ok = false, message = "Admin login is disabled." });

            if (clsAdminLoginGuard.IsLocked(guardKey, out int retryAfterSeconds))
            {
                clsAdminAuditLog.Write(
                    "LoginLocked",
                    Simulate.String(UserName),
                    $"Retry after {retryAfterSeconds}s",
                    clientIp,
                    false);
                return Json(new
                {
                    ok = false,
                    message = $"Too many failed attempts. Try again in {retryAfterSeconds} seconds."
                });
            }

            if (!clsAdminLogin.CredentialsMatch(
                    configuration,
                    Simulate.String(UserName),
                    Simulate.String(Password),
                    Simulate.String(Email)))
            {
                clsAdminLoginGuard.RegisterFailure(guardKey);
                clsAdminAuditLog.Write(
                    "LoginFailed",
                    Simulate.String(UserName),
                    "Invalid admin password",
                    clientIp,
                    false);
                return Json(new { ok = false, message = "Invalid admin credentials." });
            }

            clsAdminLoginGuard.RegisterSuccess(guardKey);

            var adminEmail = (configuration["AdminLogin:Email"] ?? "").Trim();
            var adminUser = (configuration["AdminLogin:UserName"] ?? adminEmail).Trim();
            if (string.IsNullOrWhiteSpace(adminEmail))
            {
                return Json(new { ok = false, message = "AdminLogin:Email is not configured on the server." });
            }

            var created = clsAdminSession.CreateOtpChallenge(adminUser, adminEmail, out string otpCode);
            if (!created.ok)
            {
                return Json(new
                {
                    ok = false,
                    message = "Please wait a minute before requesting another verification code."
                });
            }

            bool emailSent = clsPasswordResetEmailSender.TrySendAdminLoginOtp(
                configuration,
                environment,
                logger,
                adminEmail,
                otpCode,
                clsAdminSession.OtpExpiryMinutes);

            bool exposeDevOtp = !emailSent
                && clsPasswordResetEmailSender.ShouldExposeOtpInApiResponse(configuration, environment);

            clsAdminAuditLog.Write(
                "LoginOtpSent",
                adminUser,
                emailSent ? $"OTP emailed to {clsAdminSession.MaskEmail(adminEmail)}" : "OTP generated (SMTP off)",
                clientIp,
                true);

            return Json(new
            {
                ok = true,
                challengeId = created.challengeId,
                emailSent,
                emailHint = clsAdminSession.MaskEmail(adminEmail),
                message = emailSent
                    ? $"Verification code sent to {clsAdminSession.MaskEmail(adminEmail)}."
                    : "Verification code created. Email is not configured — use the code shown below (development) or in the API console.",
                devOtp = exposeDevOtp ? otpCode : null
            });
        }

        [HttpPost]
        [Route("AdminVerifyLoginOtp")]
        public IActionResult AdminVerifyLoginOtp(string ChallengeId, string Otp)
        {
            var configuration = HttpContext.RequestServices.GetService<IConfiguration>();
            string clientIp = clsAdminAuthHelper.ReadClientIp(Request);
            if (!clsAdminLogin.IsEnabled(configuration))
                return StatusCode(403, new { ok = false, message = "Admin login is disabled." });

            string otp = (Otp ?? "").Trim();
            if (otp.Length != 6 || !otp.All(char.IsDigit))
            {
                return Json(new { ok = false, message = "Enter the 6-digit code from your email." });
            }

            if (!clsAdminSession.VerifyOtp(
                    Simulate.String(ChallengeId),
                    otp,
                    out string userName,
                    out string email))
            {
                clsAdminAuditLog.Write(
                    "LoginOtpFailed",
                    userName,
                    "Invalid or expired OTP",
                    clientIp,
                    false);
                return Json(new { ok = false, message = "Invalid or expired verification code." });
            }

            string token = clsAdminSession.CreateSession(userName, email, clientIp);
            clsAdminAuditLog.Write(
                "LoginSuccess",
                userName,
                "Admin portal session created",
                clientIp,
                true);

            return Json(new
            {
                ok = true,
                message = "Admin authenticated.",
                adminToken = token,
                userName,
                email,
                expiresInMinutes = clsAdminSession.SessionExpiryMinutes,
                scope = "MainDatabase"
            });
        }

        [HttpGet]
        [Route("AdminValidateSession")]
        public IActionResult AdminValidateSession(string AdminToken)
        {
            var configuration = HttpContext.RequestServices.GetService<IConfiguration>();
            if (!clsAdminLogin.IsEnabled(configuration))
                return StatusCode(403, new { ok = false, message = "Admin login is disabled." });

            if (!clsAdminSession.TryValidateToken(Simulate.String(AdminToken), out string userName, out string email))
            {
                return Json(new { ok = false, message = "Admin session expired or invalid." });
            }

            return Json(new { ok = true, userName, email });
        }

        [HttpPost]
        [Route("AdminLogout")]
        public IActionResult AdminLogout(string AdminToken)
        {
            string token = Simulate.String(AdminToken);
            if (clsAdminSession.TryValidateToken(token, out string userName, out _))
            {
                clsAdminAuditLog.Write(
                    "Logout",
                    userName,
                    "Admin portal session ended",
                    clsAdminAuthHelper.ReadClientIp(Request),
                    true);
            }

            clsAdminSession.RevokeToken(token);
            return Json(new { ok = true });
        }

        private IActionResult RequireAdminDiagnostics()
        {
            var configuration = HttpContext.RequestServices.GetService<IConfiguration>();
            if (!clsAdminAuthHelper.TryAuthorizeAdmin(
                    configuration,
                    Request,
                    out IActionResult errorResult,
                    out _,
                    out _))
            {
                return errorResult;
            }

            return null;
        }

        [HttpGet]
        [Route("AdminPing")]
        public IActionResult AdminPing()
        {
            var denied = RequireAdminDiagnostics();
            if (denied != null) return denied;

            var host = Request.Host.Value ?? "";
            return Json(clsAdminDiagnostics.Ping(host));
        }

        [HttpGet]
        [Route("AdminCheckMainDatabase")]
        public IActionResult AdminCheckMainDatabase()
        {
            var denied = RequireAdminDiagnostics();
            if (denied != null) return denied;

            var configuration = HttpContext.RequestServices.GetService<IConfiguration>();
            return Json(clsAdminDiagnostics.CheckMainDatabase(configuration));
        }

        [HttpGet]
        [Route("AdminCheckCompanyDatabase")]
        public IActionResult AdminCheckCompanyDatabase(int CompanyID)
        {
            var denied = RequireAdminDiagnostics();
            if (denied != null) return denied;

            var configuration = HttpContext.RequestServices.GetService<IConfiguration>();
            return Json(clsAdminDiagnostics.CheckCompanyDatabase(CompanyID, configuration));
        }

        [HttpGet]
        [Route("GetTechnicalInfo")]
        public IActionResult GetTechnicalInfo(int CompanyID, int UserID)
        {
            return Json(clsTechnicalInfo.GetTechnicalInfo(CompanyID, UserID));
        }

        #endregion

        #region Employee
        [HttpGet]
        [Route("CheckDatebaseVersion")]
        public void CheckDatebaseVersion(int CompanyId) {
            try
            {
                clsDataBaseVersion cls = new clsDataBaseVersion();
                clsCompany clsCompany =new clsCompany();
               DataTable dt=  cls.SelectDataBaseVersion(0, CompanyId);
                if (dt != null && dt.Rows.Count > 0) {
                    decimal versionNumber = Simulate.decimal_(dt.Rows[0]["VersionNumber"]);
                    clsSQL clssql = new clsSQL();
                    if (versionNumber < 1) { 
                    
                      


                         
                        cls.InsertDataBaseVersion(Simulate.decimal_( 1.1), CompanyId);

                    }

                    cls.checkDatabaseUpdates(versionNumber, CompanyId);

                    // Keep standard FastReport defaults present for every company database.
                    try
                    {
                        clsTransactionReportPrint.TryEnsureTransactionReportSchema(CompanyId);
                        clsTransactionReportDefaults.ApplyDefaultSeeds(CompanyId, 0);
                    }
                    catch
                    {
                    }

                    Random random = new Random();
                    int randomValue = random.Next(1000, 9999);
                    bool a =cls.CreateDataBase("a"+ Simulate.String( randomValue), CompanyId);
                    if (a) {

                        CheckDatebaseVersion(CompanyId);
                    }
                }
              
            }
            catch (Exception)
            {

                throw;
            }
        
        }

        [HttpGet]
        [Route("CheckLogin")]
        public string CheckLogin(string UserName, string Password,string Email, int CompanyID,
            string DeviceInfo = "", string AppVersion = "", string Platform = "")
        {
            try
            {
                string JSONString =JsonConvert.SerializeObject(string.Empty);
                var auditCtx = BuildAuditContext(DeviceInfo, AppVersion, Platform);
                if (CompanyID == 0) {
                    return JsonConvert.SerializeObject(new { Error = "Please select a company first." });
                }
                if (Simulate.String(Email) != "") {

                    Password = "";
                    UserName = "";


                } else { 
                if (Simulate.String(UserName) == "")
                {
                    return JSONString;

                }
                if (Simulate.String(Password) == "" || CompanyID==0)
                {
                    return JSONString;

                }
                }
                clsEmployee clsEmployee = new clsEmployee();

                clsSQL sql = new clsSQL();
                string companyConn = sql.CreateDataBaseConnectionString(CompanyID);
                if (string.IsNullOrWhiteSpace(companyConn))
                {
                    return JsonConvert.SerializeObject(new
                    {
                        Error = "Company not found or company database is not configured. Open company settings and search for your company again."
                    });
                }

                if (IsCompanyLoginBlocked(CompanyID))
                {
                    return JsonConvert.SerializeObject(new
                    {
                        Error = "This company account is inactive or suspended. Contact the system administrator."
                    });
                }

                CheckDatebaseVersion(CompanyID);

                var configuration = HttpContext.RequestServices.GetService<IConfiguration>();
                if (clsAdminLogin.CredentialsMatch(
                        configuration,
                        Simulate.String(UserName),
                        Simulate.String(Password),
                        Simulate.String(Email)))
                {
                    DataTable adminDt = clsAdminLogin.ResolveEmployeeForCompany(
                        clsEmployee, CompanyID, configuration);
                    if (adminDt != null && adminDt.Rows.Count > 0)
                    {
                        return SerializeEmployeeWithSession(adminDt, CompanyID, auditCtx);
                    }

                    return JsonConvert.SerializeObject(new
                    {
                        Error = "Admin login is enabled but no employee was found for this company. Add a system user or an employee with the admin email."
                    });
                }

                DataTable dt = clsEmployee.SelectEmployee(0, "", "", Simulate.String(UserName), Simulate.String(Password),Simulate.String( Email), "", CompanyID, 1);
                if (dt != null && dt.Rows.Count > 0)
                {
                    if (IsEmployeeLoginBlocked(dt))
                    {
                        clsAuditService.LogLoginFailed(auditCtx, CompanyID, Simulate.String(UserName));
                        return JsonConvert.SerializeObject(new
                        {
                            Error = "This user account is inactive. Contact the system administrator."
                        });
                    }
                    return SerializeEmployeeWithSession(dt, CompanyID, auditCtx);
                }
                else
                {
                    clsForgotPasswordRequest clsForgotPasswordRequest=new clsForgotPasswordRequest();
                    DataTable dtForgotPasswordRequest = clsForgotPasswordRequest.SelectForgotPasswordRequest(
                        Simulate.String(UserName),
                        Simulate.String(Password),
                        CompanyID);
                    if (dtForgotPasswordRequest != null && dtForgotPasswordRequest.Rows.Count > 0) {
                         dt = clsEmployee.SelectEmployee(Simulate.Integer32( dtForgotPasswordRequest.Rows[0]["EmployeeID"]), "", "", "","", "", "", CompanyID, 1);
                        if (dt != null && dt.Rows.Count > 0)
                        {
                            if (IsEmployeeLoginBlocked(dt))
                            {
                                clsAuditService.LogLoginFailed(auditCtx, CompanyID, Simulate.String(UserName));
                                return JsonConvert.SerializeObject(new
                                {
                                    Error = "This user account is inactive. Contact the system administrator."
                                });
                            }
                            clsForgotPasswordRequest.ConsumeForgotPasswordRequest(
                                Simulate.Integer32(dtForgotPasswordRequest.Rows[0]["ID"]),
                                CompanyID);
                            return SerializeEmployeeWithSession(dt, CompanyID, auditCtx);
                        }
                    }

                    clsAuditService.LogLoginFailed(auditCtx, CompanyID, Simulate.String(UserName));
                    return JSONString;
                }
            }
            catch (Exception ex)
            {

                return ex.Message;
            }




        }

        private AuditContext BuildAuditContext(string deviceInfo, string appVersion, string platform)
        {
            string ip = HttpContext?.Connection?.RemoteIpAddress?.ToString() ?? "";
            return new AuditContext
            {
                IPAddress = ip,
                DeviceInfo = deviceInfo ?? "",
                AppVersion = appVersion ?? "",
                Platform = platform ?? "",
            };
        }

        private string SerializeEmployeeWithSession(DataTable dt, int companyId, AuditContext ctx, string authMethod = "Password")
        {
            if (dt == null || dt.Rows.Count == 0)
                return JsonConvert.SerializeObject(string.Empty);

            int userId = Simulate.Integer32(dt.Rows[0]["ID"]);
            string userName = Simulate.String(dt.Rows[0]["UserName"]);
            var session = clsAuditService.StartSession(ctx, userId, userName, companyId, authMethod);

            if (!dt.Columns.Contains("SessionGuid"))
                dt.Columns.Add("SessionGuid", typeof(string));

            foreach (DataRow row in dt.Rows)
                row["SessionGuid"] = session.SessionGuid.ToString();

            return JsonConvert.SerializeObject(dt);
        }

        private bool IsEmployeeLoginBlocked(DataTable dt)
        {
            try
            {
                if (dt == null || dt.Rows.Count == 0) return true;
                if (!dt.Columns.Contains("IsActive")) return false;
                object raw = dt.Rows[0]["IsActive"];
                if (raw == null || raw == DBNull.Value) return false;
                return !Simulate.Bool(raw);
            }
            catch
            {
                return false;
            }
        }

        private bool IsCompanyLoginBlocked(int companyId)
        {
            try
            {
                clsSQL sql = new clsSQL();
                DataTable dt = sql.ExecuteQueryStatement(
                    "SELECT TOP 1 ISNULL(IsActive,1) AS IsActive, ISNULL(IsSuspended,0) AS IsSuspended FROM tbl_Company WHERE ID = " + Simulate.String(companyId),
                    sql.MainDataBaseconString, null);
                if (dt == null || dt.Rows.Count == 0) return false;
                bool isActive = Simulate.Bool(dt.Rows[0]["IsActive"]);
                bool isSuspended = Simulate.Bool(dt.Rows[0]["IsSuspended"]);
                return !isActive || isSuspended;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Touch / POS login by keyboard-wedge access card UID (no password).
        /// </summary>
        [HttpGet]
        [Route("CheckLoginByCard")]
        public string CheckLoginByCard(string CardUid, int CompanyID,
            string DeviceInfo = "", string AppVersion = "", string Platform = "")
        {
            try
            {
                var auditCtx = BuildAuditContext(DeviceInfo, AppVersion, Platform);
                string cardUid = Simulate.String(CardUid).Trim();
                if (CompanyID == 0)
                    return JsonConvert.SerializeObject(new { Error = "Please select a company first." });
                if (string.IsNullOrEmpty(cardUid))
                    return JsonConvert.SerializeObject(string.Empty);

                clsSQL sql = new clsSQL();
                string companyConn = sql.CreateDataBaseConnectionString(CompanyID);
                if (string.IsNullOrWhiteSpace(companyConn))
                {
                    return JsonConvert.SerializeObject(new
                    {
                        Error = "Company not found or company database is not configured. Open company settings and search for your company again."
                    });
                }

                if (IsCompanyLoginBlocked(CompanyID))
                {
                    return JsonConvert.SerializeObject(new
                    {
                        Error = "This company account is inactive or suspended. Contact the system administrator."
                    });
                }

                CheckDatebaseVersion(CompanyID);

                clsEmployee clsEmployee = new clsEmployee();
                DataTable dt = clsEmployee.SelectEmployeeByAccessCard(cardUid, CompanyID);
                if (dt == null || dt.Rows.Count == 0)
                {
                    clsAuditService.LogLoginFailed(auditCtx, CompanyID, "Card:" + cardUid);
                    return JsonConvert.SerializeObject(string.Empty);
                }

                if (IsEmployeeLoginBlocked(dt))
                {
                    clsAuditService.LogLoginFailed(auditCtx, CompanyID, "Card:" + cardUid);
                    return JsonConvert.SerializeObject(new
                    {
                        Error = "This user account is inactive. Contact the system administrator."
                    });
                }

                return SerializeEmployeeWithSession(dt, CompanyID, auditCtx, "Card");
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }

        /// <summary>
        /// Validate manager override for a single POS action via card and/or username+password.
        /// Does NOT start a new login session or swap the cashier.
        /// </summary>
        [HttpGet]
        [Route("ValidatePOSOverride")]
        public string ValidatePOSOverride(int CompanyID, int ActionId,
            string CardUid = "", string UserName = "", string Password = "")
        {
            try
            {
                if (CompanyID <= 0 || ActionId <= 0)
                {
                    return JsonConvert.SerializeObject(new
                    {
                        Granted = false,
                        Error = "Invalid company or action."
                    });
                }

                CheckDatebaseVersion(CompanyID);

                clsEmployee clsEmployee = new clsEmployee();
                DataTable dt = null;
                string authMethod = "";

                string cardUid = Simulate.String(CardUid).Trim();
                if (!string.IsNullOrEmpty(cardUid))
                {
                    dt = clsEmployee.SelectEmployeeByAccessCard(cardUid, CompanyID);
                    authMethod = "Card";
                }
                else if (!string.IsNullOrEmpty(Simulate.String(UserName)) &&
                         !string.IsNullOrEmpty(Simulate.String(Password)))
                {
                    dt = clsEmployee.SelectEmployee(0, "", "", Simulate.String(UserName), Simulate.String(Password), "", "", CompanyID, 1);
                    authMethod = "Password";
                }

                if (dt == null || dt.Rows.Count == 0)
                {
                    return JsonConvert.SerializeObject(new
                    {
                        Granted = false,
                        Error = "Credentials not matched"
                    });
                }

                if (IsEmployeeLoginBlocked(dt))
                {
                    return JsonConvert.SerializeObject(new
                    {
                        Granted = false,
                        Error = "This user account is inactive."
                    });
                }

                int approverId = Simulate.Integer32(dt.Rows[0]["ID"]);
                string approverName = Simulate.String(dt.Rows[0]["AName"]);
                if (string.IsNullOrWhiteSpace(approverName))
                    approverName = Simulate.String(dt.Rows[0]["UserName"]);

                bool hasAction = clsEmployee.UserHasPOSAction(approverId, ActionId, CompanyID);
                if (!hasAction)
                {
                    return JsonConvert.SerializeObject(new
                    {
                        Granted = false,
                        ApproverId = approverId,
                        ApproverName = approverName,
                        AuthMethod = authMethod,
                        Error = "Approver does not have permission for this POS action."
                    });
                }

                return JsonConvert.SerializeObject(new
                {
                    Granted = true,
                    ApproverId = approverId,
                    ApproverName = approverName,
                    AuthMethod = authMethod,
                    ActionId = ActionId
                });
            }
            catch (Exception ex)
            {
                return JsonConvert.SerializeObject(new { Granted = false, Error = ex.Message });
            }
        }

        [HttpGet]
        [Route("SelectEmployeesByID")]
        public string SelectEmployeesByID(int ID, string UserName, string Password, int CompanyId)
        {
            try
            {
                clsEmployee clsEmployee = new clsEmployee();
                DataTable dt = clsEmployee.SelectEmployee(ID, "", "", Simulate.String(UserName), Simulate.String(Password), "", "", CompanyId,-1);
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
        public int InsertEmployee([FromBody] JsonElement data, string AName, string EName, string UserName, 
            string Password, int CompanyID, int CreationUserId, string Email,string  Tel1, bool IsSystemUser
             ,string EmployeeCode
                                 , string Tel2
                , string Address
                , int CountryID
                , int CityID
                , int NationalityID
                , string NationalNumber
                , string IDNumber
                , DateTime IDIssueDate
                , DateTime IDExpireDate
                , string PassportNumber
                , DateTime PassportIssueDate
                , DateTime PassportExpireDate
                , int EducationalLevelID
                , DateTime HireDate
                , string BankName
                , string IBAN
                , string SWIFTCode
                , string BankAccountNumber
                , string SocialSecurityNumber
                , int SocialSecurityProgramID
                , string MedicalInsuranceNumber
                , int MedicalInsuranceProgramID
            ,int DepartmentID
            , bool IsPOSOnly = false
            , bool IsActive = true
            , bool ShowOnTouchLogin = true
            , string AccessCardUid = ""
            , bool IsAdmin = false

            )
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
                int A = clsEmployee.InsertEmployee(Simulate.String(AName), Simulate.String(EName), Simulate.String(UserName), Simulate.String(Password), Simulate.Integer32(CompanyID),
                    CreationUserId,  IsSystemUser, Simulate.String(Email) , Simulate.String(Tel1) 
                    , Simulate.String(EmployeeCode)
                , Simulate.String(Tel2)
                , Simulate.String(Address)
                , Simulate.Integer32(CountryID)
                , Simulate.Integer32(CityID)
                , Simulate.Integer32(NationalityID)
                , Simulate.String(NationalNumber)
                , Simulate.String(IDNumber)
                , Simulate.StringToDate(IDIssueDate)
                , Simulate.StringToDate(IDExpireDate)
                , Simulate.String(PassportNumber)
                , Simulate.StringToDate(PassportIssueDate)
                , Simulate.StringToDate(PassportExpireDate)
                , Simulate.Integer32(EducationalLevelID)
                , Simulate.StringToDate(HireDate)
                , Simulate.String(BankName)
                , Simulate.String(IBAN)
                , Simulate.String(SWIFTCode)
                , Simulate.String(BankAccountNumber)
                , Simulate.String(SocialSecurityNumber)
                , Simulate.Integer32(SocialSecurityProgramID)
                , Simulate.String(MedicalInsuranceNumber)
                , Simulate.Integer32(MedicalInsuranceProgramID) , DepartmentID, IsPOSOnly,Signuturea,
                    null, IsActive, ShowOnTouchLogin, Simulate.String(AccessCardUid), IsAdmin);
                return A;
            }
            catch (Exception ex)
            {

                throw;
            }

        }
        [HttpPost]
        [Route("UpdateEmployee")]
        public int UpdateEmployee([FromBody] JsonElement data, string AName, string EName, string UserName, string Password, int ID, int ModificationUserId, bool IsSystemUser, String  Email, String  Tel1 , int CompanyID
            , string EmployeeCode
                                 , string Tel2
                , string Address
                , int CountryID
                , int CityID
                , int NationalityID
                , string NationalNumber
                , string IDNumber
                , DateTime IDIssueDate
                , DateTime IDExpireDate
                , string PassportNumber
                , DateTime PassportIssueDate
                , DateTime PassportExpireDate
                , int EducationalLevelID
                , DateTime HireDate
                , string BankName
                , string IBAN
                , string SWIFTCode
                , string BankAccountNumber
                , string SocialSecurityNumber
                , int SocialSecurityProgramID
                , string MedicalInsuranceNumber
                , int MedicalInsuranceProgramID,int DepartmentID, bool IsPOSOnly = false,
            bool IsActive = true, bool ShowOnTouchLogin = true, string AccessCardUid = "", bool IsAdmin = false)
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
                int A = clsEmployee.UpdateEmployee(Simulate.String(AName), Simulate.String(EName), 
                    Simulate.String(UserName), Simulate.String(Password), ID,
                    ModificationUserId,  IsSystemUser, Simulate.String(Email),
                    Simulate.String(Tel1), Signuturea, CompanyID


                , Simulate.String( EmployeeCode)
                , Simulate.String(Tel2)
                , Simulate.String(Address)
                , Simulate.Integer32(CountryID)
                , Simulate.Integer32( CityID)
                , Simulate.Integer32(NationalityID)
                , Simulate.String(NationalNumber)
                , Simulate.String(IDNumber)
                , Simulate.StringToDate(IDIssueDate)
                , Simulate.StringToDate(IDExpireDate)
                , Simulate.String(PassportNumber)
                , Simulate.StringToDate(PassportIssueDate)
                , Simulate.StringToDate( PassportExpireDate)
                , Simulate.Integer32(EducationalLevelID)
                , Simulate.StringToDate(HireDate)
                , Simulate.String(BankName)
                , Simulate.String(IBAN)
                , Simulate.String(SWIFTCode)
                , Simulate.String(BankAccountNumber)
                , Simulate.String(SocialSecurityNumber)
                , Simulate.Integer32(SocialSecurityProgramID)
                , Simulate.String(MedicalInsuranceNumber)
                , Simulate.Integer32(MedicalInsuranceProgramID)
                 ,Simulate.Integer32(DepartmentID), IsPOSOnly, IsActive, ShowOnTouchLogin, Simulate.String(AccessCardUid), IsAdmin);
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
        public string SelectCompanyByID(int ID, string Phone, string PartOfTheName, bool fromMainDB)
        {
            try
            {
                clsCompany clsCompany = new clsCompany();
                DataTable dt = clsCompany.SelectCompany(ID, "", "", Phone ?? "", ID, PartOfTheName ?? "", fromMainDB);
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
                return ex.ToString();
              //  throw;
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


                    A = clsCompany.InsertCompany(Simulate.String(AName), Simulate.String(EName), Simulate.String(Email)
                  , Simulate.String(Address), Simulate.String(Tel1), Simulate.String(Tel2), Simulate.String(ContactPerson),
                    Simulate.String(ContactNumber), myLogo, Simulate.String(TradeName), Simulate.String(UserName) + Simulate.String(Tel1), clsSQL.MainDataBaseconString);


                    if (A > 0)
                    {






                
                        clsDataBaseVersion ClsDataBaseVersion = new clsDataBaseVersion();
                        ClsDataBaseVersion.CreateDataBase(UserName + Tel1, A);

                       var ff   = clsCompany.InsertCompanyWithID(A,Simulate.String(AName), Simulate.String(EName), Simulate.String(Email)
      , Simulate.String(Address), Simulate.String(Tel1), Simulate.String(Tel2), Simulate.String(ContactPerson),
        Simulate.String(ContactNumber), myLogo, Simulate.String(TradeName), Simulate.String(UserName) + Simulate.String(Tel1), clsSQL.CreateDataBaseConnectionString(A));
                        clsEmployee clsEmployee = new clsEmployee();
                        byte[] Signuture = new byte[0];
                        int b = clsEmployee.InsertEmployee(Simulate.String(AName), Simulate.String(EName), 
                            Simulate.String(UserName), Simulate.String(Password), A, 0,true, Simulate.String(Email),
                            Simulate.String(Tel1),
                            "","","",0,0,0,"","",DateTime.Now,DateTime.Now,"",DateTime.Now,DateTime.Now,0,DateTime.Now,"","","","","",0,"",0, 0, false,Signuture, null, true, true, "", true);
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
            string ContactNumber, [FromBody] string Logo, string TradeName, int ModificationUserId,
            bool EnableTouchScreenPosLogin = false,
            bool EnableEcommerce = false,
            string WebSlug = "")
        {
            try
            {
                // Ensure EnableTouchScreenPosLogin / ecommerce columns exist before update.
                try { CheckDatebaseVersion(ID); } catch { /* non-blocking */ }

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
             Simulate.String(ContactNumber), myLogo, Simulate.String(TradeName), ModificationUserId,ID,
             EnableTouchScreenPosLogin, EnableEcommerce, Simulate.String(WebSlug));
                return A;
            }
            catch (Exception)
            {

                throw;
            }

        }

        [HttpGet]
        [Route("CheckWebSlugAvailable")]
        public string CheckWebSlugAvailable(int CompanyID, string WebSlug)
        {
            try
            {
                clsCompany clsCompany = new clsCompany();
                string normalized = (WebSlug ?? "").Trim().ToLowerInvariant();
                var sb = new System.Text.StringBuilder();
                foreach (char c in normalized)
                {
                    if ((c >= 'a' && c <= 'z') || (c >= '0' && c <= '9') || c == '-')
                        sb.Append(c);
                }
                normalized = sb.ToString();
                if (string.IsNullOrEmpty(normalized))
                {
                    return JsonConvert.SerializeObject(new
                    {
                        Available = false,
                        Normalized = "",
                        Error = "Slug is required",
                    });
                }
                bool available = clsCompany.IsWebSlugAvailable(CompanyID, normalized);
                return JsonConvert.SerializeObject(new
                {
                    Available = available,
                    Normalized = normalized,
                    Error = available ? "" : "Web shop slug is already used by another company.",
                });
            }
            catch (Exception ex)
            {
                return JsonConvert.SerializeObject(new
                {
                    Available = false,
                    Normalized = "",
                    Error = ex.Message,
                });
            }
        }

        /// <summary>
        /// Touch-screen POS login helpers (landing page). Users list never includes passwords.
        /// </summary>
        [HttpGet]
        [Route("GetTouchScreenPosLoginEnabled")]
        public string GetTouchScreenPosLoginEnabled(int CompanyID)
        {
            try
            {
                if (CompanyID <= 0)
                {
                    return JsonConvert.SerializeObject(new { Enabled = false });
                }
                try { CheckDatebaseVersion(CompanyID); } catch { /* non-blocking */ }
                clsCompany clsCompany = new clsCompany();
                bool enabled = clsCompany.IsTouchScreenPosLoginEnabled(CompanyID);
                return JsonConvert.SerializeObject(new { Enabled = enabled });
            }
            catch
            {
                return JsonConvert.SerializeObject(new { Enabled = false });
            }
        }

        [HttpGet]
        [Route("SelectTouchPosUsers")]
        public string SelectTouchPosUsers(int CompanyID)
        {
            try
            {
                if (CompanyID <= 0)
                {
                    return JsonConvert.SerializeObject(new { Error = "Please select a company first.", Users = Array.Empty<object>() });
                }
                try { CheckDatebaseVersion(CompanyID); } catch { /* non-blocking */ }

                clsCompany clsCompany = new clsCompany();
                if (!clsCompany.IsTouchScreenPosLoginEnabled(CompanyID))
                {
                    return JsonConvert.SerializeObject(new
                    {
                        Error = "Touch screen POS login is disabled for this company.",
                        Users = Array.Empty<object>()
                    });
                }

                DataTable dt = clsCompany.SelectTouchPosUsers(CompanyID);
                return JsonConvert.SerializeObject(new
                {
                    Users = dt ?? new DataTable()
                });
            }
            catch (Exception ex)
            {
                return JsonConvert.SerializeObject(new { Error = ex.Message, Users = Array.Empty<object>() });
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

        public class ReorderKeysBody
        {
            public string OrderedGuids { get; set; }
            public string OrderedIds { get; set; }
        }

        [HttpPost]
        [Route("ReorderItemsCategories")]
        public bool ReorderItemsCategories(
            [FromQuery] int CompanyID,
            [FromQuery] int ModificationUserID,
            [FromQuery] int CashDrawerID = 0,
            [FromQuery] string OrderedIds = null,
            [FromBody] ReorderKeysBody body = null)
        {
            try
            {
                string ordered = OrderedIds;
                if (string.IsNullOrWhiteSpace(ordered) && body != null)
                    ordered = !string.IsNullOrWhiteSpace(body.OrderedIds) ? body.OrderedIds : body.OrderedGuids;

                // Cash-level: order follows this drawer on any PC (does not change master POSOrder).
                if (CashDrawerID > 0)
                {
                    try
                    {
                        clsPOSCashMenuOrder cashOrder = new clsPOSCashMenuOrder();
                        return cashOrder.ReplaceOrder(
                            CashDrawerID,
                            clsPOSCashMenuOrder.KindCategory,
                            Simulate.String(ordered),
                            CompanyID,
                            ModificationUserID);
                    }
                    catch
                    {
                        return false;
                    }
                }
                clsItemsCategory clsItemsCategory = new clsItemsCategory();
                return clsItemsCategory.ReorderItemsCategories(Simulate.String(ordered), CompanyID, ModificationUserID);
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
                DataTable dt= new DataTable();
                if (JVTypeID == 15) {
                    dt = clsJournalVoucherHeader.SelectJournalVoucherHeaderForScheduling(Simulate.String(Guid), BranchID, CostCenterID, Simulate.String(Notes), Simulate.String(JVNumber), JVTypeID, CompanyID, Simulate.StringToDate(Date1), Simulate.StringToDate(Date2));

                }
                else { 
                
                
                  dt = clsJournalVoucherHeader.SelectJournalVoucherHeader(Simulate.String(Guid), BranchID, CostCenterID, Simulate.String(Notes), Simulate.String(JVNumber), JVTypeID, CompanyID, Simulate.StringToDate(Date1), Simulate.StringToDate(Date2));
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
        [Route("SelectJournalVoucherDetailsByParentIdForPrint")]
        public string SelectJournalVoucherDetailsByParentIdForPrint(string ParentGuid,  int CompanyID)
        {
            try
            {
                clsJournalVoucherDetails clsJournalVoucherDetails = new clsJournalVoucherDetails();
             
                DataTable dt = clsJournalVoucherDetails.SelectJournalVoucherDetailsByParentIdForPrint(CompanyID, ParentGuid, 0, 0).Tables[0];
 
          
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

        public string InsertJournalVoucherHeader(int BranchID, int CostCenterID, string Notes, string JVNumber, int JVTypeID, [FromBody] string DetailsList, int CompanyID, DateTime VoucherDate, int CreationUserId,string RelatedFinancingHeaderGuid = "",int RelatedLoanTypeID=0, string BudgetOverrideReason = "")

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

                    decimal totalAmount = 0;
                    for (int i = 0; i < details.Count; i++)
                        totalAmount += details[i].Debit;

                    bool forceBudgetApproval = false;
                    BudgetCheckResult budgetCheck = null;
                    if (JVTypeID == (int)clsEnum.VoucherType.ManualJV)
                    {
                        var spend = clsBudgetControl.FromJournalDetails(details, BranchID, CostCenterID);
                        string blocked = new clsBudgetControl().ApplyGate(
                            CompanyID, JVTypeID, VoucherDate, BranchID, CostCenterID, spend,
                            BudgetOverrideReason, out forceBudgetApproval, out budgetCheck);
                        if (blocked != null)
                        {
                            trn.Rollback();
                            return blocked;
                        }
                    }

                    clsApprovalEngine approvalEngine = new clsApprovalEngine();
                    int documentStatus = approvalEngine.ResolveInitialDocumentStatus(
                        CompanyID, JVTypeID, BranchID, totalAmount);
                    if (forceBudgetApproval)
                        documentStatus = (int)clsEnum.DocumentStatus.Draft;
                     
                        A = clsJournalVoucherHeader.InsertJournalVoucherHeader(BranchID, CostCenterID, Simulate.String(Notes), JVNumber, JVTypeID, CompanyID, VoucherDate, CreationUserId, RelatedFinancingHeaderGuid, RelatedLoanTypeID, trn, documentStatus);
                    if (A == "") IsSaved = false;
                    for (int i = 0; i < details.Count; i++)
                    {
                        if (details[i].Debit == 0 && details[i].Credit == 0)
                        {

                        }
                        else { 
                            string c = clsDetails.InsertJournalVoucherDetails(A, i, details[i].AccountID, details[i].SubAccountID, details[i].Debit, details[i].Credit
                                  , details[i].Total, details[i].CurrencyID, details[i].CurrencyRate, details[i].CurrencyBaseAmount, details[i].BranchID, details[i].CostCenterID, details[i].DueDate, details[i].Note, details[i].CompanyID
                                  , details[i].CreationUserID, details[i].RelatedDetailsGuid, trn);
                        if (c == "")
                            IsSaved = false;
                        }
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
                    {
                        trn.Rollback();
                    A = "";
                     }

                    if (IsSaved && forceBudgetApproval && !string.IsNullOrEmpty(A))
                    {
                        string ovErr = new clsBudget().CompleteBudgetOverride(
                            "tbl_JournalVoucherHeader", CompanyID, CreationUserId, JVTypeID, A,
                            JVNumber, BudgetOverrideReason, budgetCheck?.Breaches);
                        if (ovErr != null) return ovErr;
                    }

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

                        double TransactionOpenDebitAmount = 0;
                        double TransactionOpenCreditAmount = 0;



                        if (Simulate.Val(dt.Rows[i]["Debit"]) > 0 && Simulate.Val(dt.Rows[i]["Debit"]) > Simulate.Val(dt.Rows[i]["Reconciled"])) {

                            TransactionOpenDebitAmount = Simulate.Val(dt.Rows[i]["Debit"]) - Simulate.Val(dt.Rows[i]["Reconciled"]);
                        }
                        if (Math.Abs(Simulate.Val(dt.Rows[i]["Credit"])) > 0 &&Math.Abs( Simulate.Val(dt.Rows[i]["Credit"])) > Simulate.Val(dt.Rows[i]["Reconciled"]))
                        {
                           

                            TransactionOpenCreditAmount = Simulate.Val(dt.Rows[i]["Credit"]) - Simulate.Val(dt.Rows[i]["Reconciled"]);
                        }

                        TotalDebit = TotalDebit + Simulate.Val(TransactionOpenDebitAmount);
                        TotalCredit = TotalCredit + Simulate.Val(TransactionOpenCreditAmount);
                    }

                    for (int i = 0; i < dt.Rows.Count; i++)
                    {
                        if (TotalDebit>0&&TotalDebit >= TotalCredit && Simulate.Val(dt.Rows[i]["Credit"]) > 0)
                        {
                          decimal  LoopOpenCreditAmount = Simulate.decimal_(dt.Rows[i]["Credit"]) - Simulate.decimal_(dt.Rows[i]["Reconciled"]);



                            if (Simulate.decimal_(dt.Rows[i]["Total"])!=0 && LoopOpenCreditAmount>0) { 
                            var a = clsReconciliation.InsertReconciliation(VoucherNumber, Simulate.String(dt.Rows[i]["Guid"]), LoopOpenCreditAmount*-1, CompanyID, CreationUserId, Simulate.String(dt.Rows[i]["Guid"]), trn);
                                if (a == "")
                                {
                                    isSaved = false;
                                }
                            }

                        }
                        else if (TotalCredit>0&&TotalCredit >= TotalDebit && Simulate.Val(dt.Rows[i]["Debit"]) > 0)
                        {
                            decimal LoopOpenDebitAmount = Simulate.decimal_(dt.Rows[i]["Debit"]) - Simulate.decimal_(dt.Rows[i]["Reconciled"]);
                            if (Simulate.decimal_(dt.Rows[i]["Total"]) != 0 && LoopOpenDebitAmount>0)
                            {
                                var a = clsReconciliation.InsertReconciliation(VoucherNumber, Simulate.String(dt.Rows[i]["Guid"]), LoopOpenDebitAmount, CompanyID, CreationUserId, Simulate.String(dt.Rows[i]["Guid"]), trn);
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

                            double LoopOpenCreditAmount = Simulate.Val(dt.Rows[i]["Credit"]) - Simulate.Val(dt.Rows[i]["Reconciled"]);

                            if (LoopOpenCreditAmount > 0)
                            {
                                if (RemainingAmount > LoopOpenCreditAmount)
                                {

                                    var a = clsReconciliation.InsertReconciliation(VoucherNumber, Simulate.String(dt.Rows[i]["Guid"]), Simulate.decimal_(LoopOpenCreditAmount*-1), CompanyID, CreationUserId, Simulate.String(dt.Rows[i]["Guid"]), trn);
                                    if (a == "")
                                    {
                                        isSaved = false;
                                    }

                                    RemainingAmount = RemainingAmount - Simulate.Val(LoopOpenCreditAmount);
                                }
                                else if (RemainingAmount >= 0)
                                {
                                    var a = clsReconciliation.InsertReconciliation(VoucherNumber, Simulate.String(dt.Rows[i]["Guid"]), Simulate.decimal_(RemainingAmount) * -1, CompanyID, CreationUserId, Simulate.String(dt.Rows[i]["Guid"]), trn);
                                    if (a == "")
                                    {
                                        isSaved = false;
                                    }
                                    RemainingAmount = RemainingAmount - Simulate.Val(LoopOpenCreditAmount);
                                    break;

                                }
                                else
                                {
                                    RemainingAmount = RemainingAmount - Simulate.Val(LoopOpenCreditAmount);
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
                            double LoopOpenDebitAmount = Simulate.Val(dt.Rows[i]["Debit"]) - Simulate.Val(dt.Rows[i]["Reconciled"]);



                            if (LoopOpenDebitAmount > 0) {
                            if (RemainingAmount >= Simulate.Val(LoopOpenDebitAmount))
                            {

                                var a = clsReconciliation.InsertReconciliation(VoucherNumber, Simulate.String(dt.Rows[i]["Guid"]), Simulate.decimal_(LoopOpenDebitAmount), CompanyID, CreationUserId, Simulate.String(dt.Rows[i]["Guid"]), trn);

                                    if (a == "")
                                    {
                                        isSaved = false;
                                    }
                                    RemainingAmount = RemainingAmount - Simulate.Val(LoopOpenDebitAmount);
                            }
                            else if(RemainingAmount>= 0)
                            {
                                var a = clsReconciliation.InsertReconciliation(VoucherNumber, Simulate.String(dt.Rows[i]["Guid"]), Simulate.decimal_(RemainingAmount) , CompanyID, CreationUserId, Simulate.String(dt.Rows[i]["Guid"]), trn);
                                    if (a == "")
                                    {
                                        isSaved = false;
                                    }
                                    RemainingAmount = RemainingAmount - Simulate.Val(LoopOpenDebitAmount);
                                break;

                            }
                            else { RemainingAmount = RemainingAmount - Simulate.Val(LoopOpenDebitAmount);  break; }
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
        public string UpdateJournalVoucherHeader(int BranchID, int CostCenterID, string Notes, string JVNumber, int JVTypeID, [FromBody] string DetailsList, int CompanyID, DateTime VoucherDate, string Guid, int ModificationUserId, string BudgetOverrideReason = "")
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
                    bool forceBudgetApproval = false;
                    BudgetCheckResult budgetCheck = null;
                    int documentStatus = (int)clsEnum.DocumentStatus.Posted;

                    DataTable dtExisting = clsJournalVoucherHeader.SelectJournalVoucherHeaderByGuid(Guid, CompanyID, trn);
                    if (dtExisting != null && dtExisting.Rows.Count > 0)
                    {
                        var row = dtExisting.Rows[0];
                        documentStatus = Simulate.Integer32(row["DocumentStatus"]);
                        int existingBranchId = Simulate.Integer32(row["BranchID"]);
                        int existingJvTypeId = Simulate.Integer32(row["JVTypeID"]);
                        decimal amount = 0;
                        if (details != null)
                        {
                            foreach (var line in details)
                                amount += Simulate.Decimal(line.Debit);
                        }

                        var approvalEngine = new clsApprovalEngine();
                        if (approvalEngine.DocumentStatusBlocksEdit(
                                CompanyID, existingJvTypeId, existingBranchId, amount, documentStatus))
                        {
                            trn.Rollback();
                            return "";
                        }
                    }

                    if (JVTypeID == (int)clsEnum.VoucherType.ManualJV)
                    {
                        var spend = clsBudgetControl.FromJournalDetails(details, BranchID, CostCenterID);
                        string blocked = new clsBudgetControl().ApplyGate(
                            CompanyID, JVTypeID, VoucherDate, BranchID, CostCenterID, spend,
                            BudgetOverrideReason, out forceBudgetApproval, out budgetCheck, Guid);
                        if (blocked != null)
                        {
                            trn.Rollback();
                            return blocked;
                        }
                        if (forceBudgetApproval)
                            documentStatus = (int)clsEnum.DocumentStatus.Draft;
                    }

                    A = clsJournalVoucherHeader.UpdateJournalVoucherHeader(BranchID, CostCenterID, Simulate.String(Notes), JVNumber, JVTypeID, VoucherDate, Guid, ModificationUserId, "", 0,CompanyID, trn);

                    if (JVTypeID != 15) { //this condition to not lose the reconcilation with the details 
                    clsDetails.DeleteJournalVoucherDetailsByParentId(Guid,CompanyID, trn);
                    for (int i = 0; i < details.Count; i++)
                    {
                        string c = clsDetails.InsertJournalVoucherDetails(Guid, i, details[i].AccountID, details[i].SubAccountID, details[i].Debit, details[i].Credit
                              , details[i].Total, details[i].CurrencyID, details[i].CurrencyRate, details[i].CurrencyBaseAmount, details[i].BranchID, details[i].CostCenterID, details[i].DueDate, details[i].Note, details[i].CompanyID
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

                    if (IsSaved && forceBudgetApproval)
                    {
                        // Force Draft so CompleteBudgetOverride can Submit into approval.
                        clsJournalVoucherHeader.UpdateDocumentStatus(
                            Guid, (int)clsEnum.DocumentStatus.Draft, ModificationUserId, CompanyID);
                        string ovErr = new clsBudget().CompleteBudgetOverride(
                            "tbl_JournalVoucherHeader", CompanyID, ModificationUserId, JVTypeID, Guid,
                            JVNumber, BudgetOverrideReason, budgetCheck?.Breaches);
                        if (ovErr != null) return ovErr;
                    }
                    else if (IsSaved)
                    {
                        A = string.IsNullOrEmpty(A) ? Guid : A;
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
        public int InsertAccounts(int ParentID, string AccountNumber, string AName, string EName, int ReportingTypeID,int ReportingTypeNodeID, int AccountNatureID, int CompanyID, int CreationUserId, bool IsSubLedger)
        {
            try
            {
                clsAccounts clsAccounts = new clsAccounts();
                int A = clsAccounts.InsertAccounts(ParentID, AccountNumber, AName, EName, ReportingTypeID, ReportingTypeNodeID, AccountNatureID, CompanyID, CreationUserId,   IsSubLedger);
                return A;
            }
            catch (Exception ex)
            {

                throw;
            }

        }
        [HttpGet]
        [Route("UpdateAccounts")]
        public int UpdateAccounts(int ID, int ParentID, string AccountNumber, string AName, string EName, int ReportingTypeID,int ReportingTypeNodeID, int AccountNatureID, int ModificationUserId, int CompanyID, bool IsSubLedger)
        {
            try
            {
                clsAccounts clsAccounts = new clsAccounts();
                int A = clsAccounts.UpdateAccounts(ID, ParentID, AccountNumber, AName, EName, ReportingTypeID, ReportingTypeNodeID,AccountNatureID, ModificationUserId, CompanyID,   IsSubLedger);
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
                DataTable dt = clsBusinessPartner.SelectBusinessPartner(ID,  Type, "", "", "", "", Active, CompanyID);
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
            string StreetName, string HouseNumber, string NationalNumber, string PassportNumber, int Nationality, string IDNumber,string TaxNumber, string Job, string BankName, string BankAccountNumber, string Note)
        {
            try
            {
                clsBusinessPartner clsBusinessPartner = new clsBusinessPartner();

                #region Validation 
                if (Simulate.String(EmpCode) != "") { 
                DataTable dtEmp11 = clsBusinessPartner.SelectBusinessPartner(0, 0, "", "", Simulate.String(EmpCode), "", - 1, CompanyID);
                    if (dtEmp11 != null && dtEmp11.Rows.Count > 0) {
                        return -1;// Emp Code found 
                    }
                }
                if (Simulate.String(IDNumber) != "")
                {
                    DataTable dtEmp12 = clsBusinessPartner.SelectBusinessPartner(0, 0, "", "", "", Simulate.String(NationalNumber), -1, CompanyID);
                    if (dtEmp12 != null && dtEmp12.Rows.Count > 0)
                    {
                        return -2;// National Number  found 
                    }
                }
                #endregion


                int A = clsBusinessPartner.InsertBusinessPartner(Simulate.String(AName), Simulate.String(EName), Simulate.String(CommercialName)
                    , Simulate.String(Address), Simulate.String(Tel), Simulate.Bool(Active), Simulate.Val(Limit),
             Simulate.String(Email), Type, CompanyID, CreationUserId
             , Simulate.String(EmpCode), Simulate.String(StreetName), Simulate.String(HouseNumber), Simulate.String(NationalNumber),
                Simulate.String(PassportNumber), Simulate.Integer32(Nationality),
                Simulate.String(IDNumber), Simulate.String(TaxNumber), Simulate.String(Job),  BankName,  BankAccountNumber, Simulate.String(Note));
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
            string PassportNumber, int Nationality, string IDNumber,string TaxNumber,string Job,int CompanyID,string BankName,string BankAccountNumber, string Note)
        {
            try
            {






                clsBusinessPartner clsBusinessPartner = new clsBusinessPartner();

                #region Validation 
                if (Simulate.String(EmpCode) != "")
                {
                    DataTable dtEmp11 = clsBusinessPartner.SelectBusinessPartner(0, 0, "", "", Simulate.String(EmpCode), "", -1, CompanyID);
                    if (dtEmp11 != null && dtEmp11.Rows.Count > 1)
                    {
                        return -1;// Emp Code found 
                    }
                }
                if (Simulate.String(NationalNumber) != "")
                {
                    DataTable dtEmp12 = clsBusinessPartner.SelectBusinessPartner(0, 0, "", "", "", Simulate.String(NationalNumber), -1, CompanyID);
                    if (dtEmp12 != null && dtEmp12.Rows.Count > 0)
                    {
                        for (int i = 0; i < dtEmp12.Rows.Count; i++)
                        {
                            if (Simulate.Integer32(dtEmp12.Rows[i]["ID"]) != ID) {
                                return -2;// National Number  found 
                            }
                        }

                 
                    }
                }
                #endregion


                int A = clsBusinessPartner.UpdateBusinessPartner(ID, Simulate.String(AName), Simulate.String(EName), Simulate.String(CommercialName)
                    , Simulate.String(Address), Simulate.String(Tel), Simulate.Bool(Active), Simulate.Val(Limit),
            Simulate.String(Email), Type, ModificationUserId
            , Simulate.String(EmpCode), Simulate.String(StreetName), Simulate.String(HouseNumber), Simulate.String(NationalNumber),
                Simulate.String(PassportNumber), Simulate.Integer32(Nationality), Simulate.String(IDNumber),Simulate.String(TaxNumber), Simulate.String(Job), CompanyID,  BankName,  BankAccountNumber, Simulate.String(Note));
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
        public FileContentResult Fastreporttoxlsx([FromBody] List<DataTable> ds, [FromQuery] List<String> SheetName, [FromQuery] List<String> ColumnType)
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
                    //    return File(stream.ToArray(), "text/csv", "Grid.csv");

                    //   return File(stream.ToArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "Grid.csv");


                }
            }



        }
        [HttpGet("{menuId}/menuitems")]
        public FileContentResult FastreporttoCSV(
    [FromBody] List<DataTable> ds,
    [FromQuery] List<string> SheetName,
    [FromQuery] List<string> ColumnType)
        {
            if (ds == null || ds.Count == 0)
                return  null;

            // Use only the first table for CSV (since CSV is single-sheet format)
            DataTable table = ds[0];
            var sb = new StringBuilder();

            // Write header
            for (int col = 0; col < table.Columns.Count; col++)
            {
                sb.Append($"\"{table.Columns[col].ColumnName}\"");
                if (col < table.Columns.Count - 1)
                    sb.Append(",");
            }
            sb.AppendLine();

            // Write rows
            for (int row = 0; row < table.Rows.Count; row++)
            {
                for (int col = 0; col < table.Columns.Count; col++)
                {
                    string value;

                    if (ColumnType.Count > col)
                    {
                        string type = ColumnType[col].ToLower();
                        if (type == "int")
                            value = Simulate.Integer32(table.Rows[row][col]).ToString();
                        else if (type == "double" || type == "decimal")
                            value = Simulate.Val(table.Rows[row][col]).ToString();
                        else
                            value = Simulate.String(table.Rows[row][col]);
                    }
                    else
                    {
                        value = Simulate.String(table.Rows[row][col]);
                    }

                    // Escape quotes and wrap in quotes
                    value = value.Replace("\"", "\"\"");
                    sb.Append($"\"{value}\"");

                    if (col < table.Columns.Count - 1)
                        sb.Append(",");
                }
                sb.AppendLine();
            }

            byte[] csvBytes = Encoding.UTF8.GetBytes(sb.ToString());

            return File(csvBytes, "text/csv", $"{SheetName[0]}.csv");
        }
        [HttpGet("{menuId}/menuitems")]
        private void FastreportStanderdParameters(FastReport.Report Report, int UserID, int CompantID)
        {
            clsCompany clsCompany = new clsCompany();
            DataTable dt = clsCompany.SelectCompany(CompantID, "", "", "", CompantID,"", false);
            if (dt != null && dt.Rows.Count>0)
            {
               
                Report.SetParameterValue("Standerd.CompanyName", Simulate.String(dt.Rows[0]["AName"]));
                Report.SetParameterValue("Standerd.Address", Simulate.String(dt.Rows[0]["Address"]));
                try { Report.SetParameterValue("Standerd.Tel1", Simulate.String(dt.Rows[0]["Tel1"])); } catch (Exception) { }
                try { Report.SetParameterValue("Standerd.Email", Simulate.String(dt.Rows[0]["Email"])); } catch (Exception) { }
                try { Report.SetParameterValue("Standerd.TradeName", Simulate.String(dt.Rows[0]["TradeName"])); } catch (Exception) { }
                try
                {
                    FastReport.PictureObject logoPic = (FastReport.PictureObject)Report.FindObject("CompanyLogo");
                    if (logoPic != null)
                    {
                        byte[] logoBytes = null;
                        if (dt.Rows[0]["Logo"] != DBNull.Value && dt.Rows[0]["Logo"] != null)
                            logoBytes = (byte[])dt.Rows[0]["Logo"];
                        if (logoBytes != null && logoBytes.Length > 0)
                        {
                            logoPic.Image = Simulate.StringToImg(logoBytes);
                            try { Report.SetParameterValue("Standerd.Logo", logoPic.Image); } catch (Exception) { }
                        }
                    }
                }
                catch (Exception)
                {
                }
        
               
            }
            clsEmployee clsEmployee = new clsEmployee();
            DataTable dtemp = clsEmployee.SelectEmployee(UserID, "", "", "", "", "", "",  CompantID,-1);
            if (dtemp != null && dtemp.Rows.Count > 0)
            {
                Report.SetParameterValue("Standerd.User", Simulate.String(dtemp.Rows[0]["AName"]));
            }
           
            Report.SetParameterValue("Standerd.PrintDate", DateTime.Now.ToString("yyyy-MM-dd"));
            Report.SetParameterValue("Standerd.PrintTime", Simulate.String(Simulate.TimeString(DateTime.Now)));

        }

        /// <summary>Computed display columns for FastReport rptEmployeeContract.</summary>
        private static void EnrichEmployeeContractDataForPrint(DataTable dt, int companyID)
        {
            if (dt == null || dt.Rows.Count == 0) return;
            void AddCol(string name, Type t)
            {
                if (!dt.Columns.Contains(name)) dt.Columns.Add(name, t);
            }
            AddCol("StartDateDisplay", typeof(string));
            AddCol("EndDateDisplay", typeof(string));
            AddCol("BasicSalaryDisplay", typeof(string));
            AddCol("ProbationMonthsDisplay", typeof(string));
            AddCol("WorkingHoursDisplay", typeof(string));
            AddCol("IsOpenEndedDisplay", typeof(string));
            AddCol("IsActiveDisplay", typeof(string));
            AddCol("AnnualLeaveDisplay", typeof(string));
            AddCol("SickLeaveDisplay", typeof(string));
            AddCol("SalaryElementsAgreementDisplay", typeof(string));
            foreach (DataRow r in dt.Rows)
            {
                int LeaveInt(DataRow row, string col, int defaultVal)
                {
                    if (!dt.Columns.Contains(col)) return defaultVal;
                    object o = row[col];
                    if (o == null || o == DBNull.Value) return defaultVal;
                    int v = Simulate.Integer32(o);
                    return v <= 0 ? defaultVal : v;
                }

                r["StartDateDisplay"] = Simulate.StringToDate(r["StartDate"]).ToString("yyyy-MM-dd");
                bool open = Simulate.Bool(r["IsOpenEnded"]);
                r["EndDateDisplay"] = open
                    ? "Open-ended / غير محدد المدة"
                    : Simulate.StringToDate(r["EndDate"]).ToString("yyyy-MM-dd");
                r["BasicSalaryDisplay"] = Simulate.Currency_format(Simulate.Val(r["BasicSalary"]));
                r["ProbationMonthsDisplay"] = Simulate.Integer32(r["ProbationMonths"]).ToString();
                r["WorkingHoursDisplay"] = Simulate.Val(r["WorkingHoursPerWeek"]).ToString("0.##");
                r["IsOpenEndedDisplay"] = open ? "Yes / نعم" : "No / لا";
                r["IsActiveDisplay"] = Simulate.Bool(r["IsActive"]) ? "Active / ساري" : "Inactive / غير ساري";

                int al = LeaveInt(r, "AnnualLeaveDaysPerYear", 14);
                int al5 = LeaveInt(r, "AnnualLeaveDaysAfter5Years", 21);
                int sk = LeaveInt(r, "SickLeaveFullPayDaysPerYear", 14);
                int skx = LeaveInt(r, "SickLeaveExtendedDaysPerYear", 14);
                r["AnnualLeaveDisplay"] =
                    al + " يوماً سنوياً بأجر كامل؛ وتصبح " + al5 +
                    " يوماً بعد إكمال خمس سنوات متتالية مع صاحب العمل (وفق قانون العمل الأردني). / " +
                    al + " days/year (full pay); " + al5 +
                    " days/year after five consecutive years with the same employer (Jordan Labour Law).";
                r["SickLeaveDisplay"] =
                    sk + " يوماً إجازة مرضية بأجر كامل سنوياً (تقرير طبي معتمد)؛ ويمكن تمديدها بحد أقصى " + skx +
                    " يوماً إضافياً وفق المادة (65) من القانون (حالة الرقود/لجنة طبية). / " +
                    sk + " days/year sick leave on full pay (approved medical report); up to " + skx +
                    " further days per Art. 65 (hospitalization / medical commission rules).";

                int empId = Simulate.Integer32(r["EmployeeID"]);
                int contractId = Simulate.Integer32(r["ID"]);
                r["SalaryElementsAgreementDisplay"] = BuildSalaryAgreementPrintText(empId, contractId, companyID);
            }
        }

        /// <summary>Text block for contract PDF: pay components flagged IncludeOnContractPrint.</summary>
        private static string BuildSalaryAgreementPrintText(int employeeId, int contractId, int companyID)
        {
            if (employeeId <= 0 || contractId <= 0 || companyID <= 0)
                return "—";
            try
            {
                clsEmployeeSalaryElements cls = new clsEmployeeSalaryElements();
                DataTable pay = cls.SelectEmployeeSalaryElementsForContractPrint(employeeId, contractId, companyID);
                if (pay == null || pay.Rows.Count == 0)
                    return "لا توجد عناصر أجر إضافية محددة للإدراج في طباعة العقد / No pay items flagged to print on this contract.";
                System.Text.StringBuilder sb = new System.Text.StringBuilder();
                foreach (DataRow z in pay.Rows)
                {
                    string an = Simulate.String(z["SalaryElementAName"]);
                    string en = Simulate.String(z["SalaryElementEName"]);
                    string amt = Simulate.Currency_format(Simulate.Val(z["AssignedValue"]));
                    string sd = Simulate.StringToDate(z["StartDate"]).ToString("yyyy-MM-dd");
                    string ed = Simulate.StringToDate(z["EndDate"]).ToString("yyyy-MM-dd");
                    sb.Append("• ");
                    sb.Append(string.IsNullOrWhiteSpace(an) ? ("#" + Simulate.String(z["SalaryElementID"])) : an);
                    if (!string.IsNullOrWhiteSpace(en))
                    {
                        sb.Append(" / ");
                        sb.Append(en);
                    }
                    sb.Append(": ");
                    sb.Append(amt);
                    sb.Append(" — ");
                    sb.Append(sd);
                    sb.Append(" → ");
                    sb.Append(ed);
                    sb.Append("\r\n");
                }
                return sb.ToString().TrimEnd();
            }
            catch
            {
                return "—";
            }
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
                

                clsReports.LoadCompanyFastReport(
                    report,
                    clsTransactionReportDefaults.PageTrialBalance,
                    "rptTrialBalance",
                    CompanyID,
                    UserId); if (BranchID == 0)
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


               

                clsReports.LoadCompanyFastReport(
                    report,
                    clsTransactionReportDefaults.PageBalanceSheet,
                    "rptBalanceSheet",
                    CompanyID,
                    UserId); if (BranchID == 0)
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


                 

                clsReports.LoadCompanyFastReport(
                    report,
                    clsTransactionReportDefaults.PageIncomeStatement,
                    "rptIncomeStatement",
                    CompanyID,
                    UserId); if (BranchID == 0)
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
        public string SelectAccountStatement(DateTime Date1, DateTime Date2, int BranchID, int CostCenterID, int Accountid,
            int subAccountid, int CompanyID,bool isDue,string JVTypeIDList,string multiAccounts)
        {
            try
            {
                clsReports clsReports = new clsReports();
                DataTable dt = clsReports.SelectAccountStatement(Date1, Date2, BranchID, CostCenterID, Accountid, subAccountid, CompanyID, isDue, JVTypeIDList, multiAccounts);
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
        public IActionResult SelectAccountStatementPDF(DateTime Date1, DateTime Date2, int BranchID, int CostCenterID, int Accountid, int subAccountid, int UserID, int CompanyID,bool isDue, string JVTypeIDList, string multiAccounts)
        {
            try
            {
                cls_AccountSetting cls_AccountSetting = new cls_AccountSetting();
                DataTable dtAccountSetting = cls_AccountSetting.SelectAccountSetting(0, 0, CompanyID);
                clsInvoiceHeader clsInvoiceHeader = new clsInvoiceHeader();
                int CashAccount = clsInvoiceHeader.GetValueFromDT(dtAccountSetting, "AccountRefID", Simulate.String((int)clsEnum.AccountMainSetting.CashAccount), 2);
                int BankAccount = clsInvoiceHeader.GetValueFromDT(dtAccountSetting, "AccountRefID", Simulate.String((int)clsEnum.AccountMainSetting.Banks), 2);
                int CustomerAccount = clsInvoiceHeader.GetValueFromDT(dtAccountSetting, "AccountRefID", Simulate.String((int)clsEnum.AccountMainSetting.CustomerAccount), 2);
                int VendorAccount = clsInvoiceHeader.GetValueFromDT(dtAccountSetting, "AccountRefID", Simulate.String((int)clsEnum.AccountMainSetting.VendorAccount), 2);


                FastReport.Utils.Config.WebMode = true;
                clsReports clsReports = new clsReports();
                DataTable dt = clsReports.SelectAccountStatement(Date1, Date2, BranchID, CostCenterID, Accountid, subAccountid, CompanyID, isDue, JVTypeIDList, multiAccounts);

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

                        if (Simulate.Integer32(dt.Rows[i]["RelatedLoanTypeID"]) > 0
                            && !string.IsNullOrWhiteSpace(Simulate.String(dt.Rows[i]["RelatedLoanTypeAName"])))
                        {
                            ds.AccountStatment.Rows[i]["VoucherType"] = dt.Rows[i]["RelatedLoanTypeAName"];
                        }
                        else {
                            ds.AccountStatment.Rows[i]["VoucherType"] = dt.Rows[i]["VoucherType"];

                        }

                        if (Simulate.Integer32(dt.Rows[i]["JVtypeid"]) == 1 || Simulate.Integer32(dt.Rows[i]["SourceTransactionNumber"])==0 ) {
                            ds.AccountStatment.Rows[i]["JVNumber"] = dt.Rows[i]["JVNumber"];
                        } else {

                            
                            ds.AccountStatment.Rows[i]["JVNumber"] = dt.Rows[i]["SourceTransactionNumber"];
                      
                        
                        }
                       
                        ds.AccountStatment.Rows[i]["AccountEname"] = dt.Rows[i]["AccountEname"];
                        ds.AccountStatment.Rows[i]["AccountNumber"] = dt.Rows[i]["AccountNumber"];
                    }
                }





                FastReport.Report report = new FastReport.Report();
                report.RegisterData(ds);


        
                clsReports.LoadCompanyFastReport(
                    report,
                    clsTransactionReportDefaults.PageAccountStatement,
                    "rptAccountStatement",
                    CompanyID,
                    UserID); if (BranchID == 0)
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
                string multiAccountsSafe = Simulate.String(multiAccounts);
                string SubAccountName = "";
                if (subAccountid > 0)
                {

                    if (Simulate.Integer32(Accountid) == VendorAccount || Simulate.Integer32(Accountid) == CustomerAccount
                        || AccountListContains(multiAccountsSafe, VendorAccount)
                        || AccountListContains(multiAccountsSafe, CustomerAccount))
                    {
                        clsBusinessPartner clsBusinessPartner = new clsBusinessPartner();
                        DataTable dtSubAccount = clsBusinessPartner.SelectBusinessPartner(subAccountid, 0, "", "", "", "", -1, CompanyID);
                        if (dtSubAccount != null && dtSubAccount.Rows.Count > 0)
                        {
                            SubAccountName = " / " + Simulate.String(dtSubAccount.Rows[0]["AName"])+' '+ Simulate.String(dtSubAccount.Rows[0]["EmpCode"]);
                        }

                    }
                    else if (Simulate.Integer32(Accountid) == BankAccount
                        || AccountListContains(multiAccountsSafe, BankAccount)) {

                        clsBanks clsBanks = new clsBanks();
                        DataTable dtSubAccount = clsBanks.SelectBanks(subAccountid,  "", "",  CompanyID);
                        if (dtSubAccount != null && dtSubAccount.Rows.Count > 0)
                        {
                            SubAccountName = " / " + Simulate.String(dtSubAccount.Rows[0]["AName"]);
                        }

                    }
                    else if (Simulate.Integer32(Accountid) == CashAccount)
                    {
                        clsCashDrawer clsCashDrawer = new clsCashDrawer();
                        DataTable dtSubAccount = clsCashDrawer.SelectCashDrawerByID(subAccountid,  "", "",   CompanyID);
                        if (dtSubAccount != null && dtSubAccount.Rows.Count > 0)
                        {
                            SubAccountName = " / " + Simulate.String(dtSubAccount.Rows[0]["AName"]);
                        }


                    }



                }
                string subAccountIdString = "";
                if (subAccountid > 0)
                {
                    subAccountIdString = " / " + subAccountid;
                }
                if (Accountid == 0) {
                List<String> aa = multiAccountsSafe.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries).ToList();
                    String AccountNameList = "";
                    String AccountNumberList = "";
                    for (int i = 0; i < aa.Count; i++)
                    {
                        string Comma = ", ";
                        if (i == 0) {
                            Comma = "";
                        
                        }


                        ////
               
                      


                        DataTable dtAccount = clsAccount.SelectAccountsByID( Simulate.Integer32( aa[i]), 0, "", "", "", CompanyID);
                        if (dtAccount != null && dtAccount.Rows.Count > 0) {
                            AccountNameList = AccountNameList + Comma + Simulate.String(dtAccount.Rows[0]["AName"]);
                            AccountNumberList = AccountNumberList + Comma + Simulate.String(dtAccount.Rows[0]["AccountNumber"]);
                        }

                    }
                    report.SetParameterValue("report.AccountName", Simulate.String(AccountNameList  + SubAccountName));
                    report.SetParameterValue("report.AccountNumber", Simulate.String(AccountNumberList + subAccountIdString));




                }
                else {

                    DataTable dtAccount = clsAccount.SelectAccountsByID(Accountid, 0, "", "", "", CompanyID);


                    if (dtAccount != null && dtAccount.Rows.Count > 0)
                {
                  
                    report.SetParameterValue("report.AccountName", Simulate.String(dtAccount.Rows[0]["AName"]) + SubAccountName);
                    report.SetParameterValue("report.AccountNumber", Simulate.String(dtAccount.Rows[0]["AccountNumber"]) + subAccountIdString);

                }
                }
                FastreportStanderdParameters(report, UserID, CompanyID);
                //    report.Prepare();

                report.Prepare();

                return FastreporttoPDF(report);
                //return Json(PrepareFrxReport(report), JsonRequestBehavior.AllowGet);


            }
            catch (Exception ex)
            {

                return BadRequest(ex.Message);
            }

        }

        /// <summary>
        /// Exact account-id match in a comma-separated list (avoids substring false positives and null).
        /// </summary>
        private static bool AccountListContains(string multiAccounts, int accountId)
        {
            if (accountId <= 0 || string.IsNullOrWhiteSpace(multiAccounts))
                return false;

            string target = Simulate.String(accountId);
            foreach (string part in multiAccounts.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
            {
                if (string.Equals(part, target, StringComparison.OrdinalIgnoreCase))
                    return true;
            }
            return false;
        }

        #endregion
        #region Invoices
        [HttpGet]
        [Route("SelectInvoicesByFilter")]
        public string SelectInvoicesByFilter(DateTime date1, DateTime date2, bool withDateFilter, int paymentMethodID, int branchID, int businessPartnerID, int storeid, int invoiceTypeid, int cashDrawerID, int isCounted, int companyID, int userID, string Time1 = "", string Time2 = "", int FilterUserID = 0)
        {
            try
            {
                clsReports clsReports = new clsReports();
                DataTable dt = clsReports.SelectInvoicesByFilter(date1, date2, withDateFilter, paymentMethodID, branchID, businessPartnerID, storeid, invoiceTypeid, cashDrawerID, isCounted, companyID, Time1, Time2, FilterUserID);

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
        public IActionResult SelectInvoicesByFilterPDFDateTime(DateTime date1, DateTime date2, bool withDateFilter, int paymentMethodID, int branchID, int businessPartnerID, int storeid, int invoiceTypeid, int cashDrawerID, int isCounted, int companyID, int userID, string Time1 = "", string Time2 = "", int FilterUserID = 0)
        {
            try
            {

                FastReport.Utils.Config.WebMode = true;
                clsReports clsReports = new clsReports();
                DataTable dt = clsReports.SelectInvoicesByFilter(date1, date2, withDateFilter, paymentMethodID, branchID, businessPartnerID, storeid, invoiceTypeid, cashDrawerID, isCounted, companyID, Time1, Time2, FilterUserID);

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



                

                clsReports.LoadCompanyFastReport(
                    report,
                    clsTransactionReportDefaults.PageInvoicesByFilter,
                    "rptAccountStatement",
                    companyID,
                    userID); if (branchID == 0)
                {
                    report.SetParameterValue("report.Branch", "All Branches");

                }
                else
                {
                    clsBranch clsBranch = new clsBranch();
                    DataTable dtBranch = clsBranch.SelectBranch(branchID, "", "", companyID);
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


                

                clsReports.LoadCompanyFastReport(
                    report,
                    clsTransactionReportDefaults.PageItemTransactions,
                    "rptAccountStatement",
                    companyID,
                    userID); if (branchID == 0)
                {
                    report.SetParameterValue("report.Branch", "All Branches");

                }
                else
                {
                    clsBranch clsBranch = new clsBranch();
                    DataTable dtBranch = clsBranch.SelectBranch(branchID, "", "", companyID);
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



               

                clsReports.LoadCompanyFastReport(
                    report,
                    clsTransactionReportDefaults.PageInventory,
                    "rptAccountStatement",
                    companyID,
                    userID); if (branchID == 0)
                {
                    report.SetParameterValue("report.Branch", "All Branches");

                }
                else
                {
                    clsBranch clsBranch = new clsBranch();
                    DataTable dtBranch = clsBranch.SelectBranch(branchID, "", "", companyID);
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
        public string SelectCashReport(bool IsPosDate, DateTime Date1, DateTime Date2, int BranchID, int CashID, int InvoiceTypeid, int UserID, int CompanyID, string Time1 = "", string Time2 = "", int FilterUserID = 0, bool GroupByUser = false, bool RemoveCents = false, bool SumAllDays = false)
        {
            try
            {
                clsReports clsReports = new clsReports();
                DataTable dt = clsReports.SelectCashReport(IsPosDate, Date1, Date2, BranchID, CashID, InvoiceTypeid, CompanyID, Time1, Time2, FilterUserID, GroupByUser, RemoveCents, SumAllDays);
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
        public IActionResult SelectCashReportPDF(bool IsPosDate, DateTime Date1, DateTime Date2, int BranchID, int CashID, int InvoiceTypeid, int UserId, int CompanyID, string Time1 = "", string Time2 = "", int FilterUserID = 0, bool GroupByUser = false, bool IsReceiptLayout = false, bool RemoveCents = false, bool SumAllDays = false)
        {
            try
            {

                FastReport.Utils.Config.WebMode = true;
                clsReports clsReports = new clsReports();
                DataTable dt = clsReports.SelectCashReport(IsPosDate, Date1, Date2, BranchID, CashID, InvoiceTypeid, CompanyID, Time1, Time2, FilterUserID, GroupByUser, RemoveCents, SumAllDays);

                dsCashReport ds = new dsCashReport();

                if (dt != null && dt.Rows.Count > 0)
                {
                    for (int i = 0; i < dt.Rows.Count; i++)
                    {
                        ds.CashReport.Rows.Add();

                        // Typed CashReport columns are strings — format dates/amounts
                        // invariantly so receipt Number format and labels stay clean.
                        ds.CashReport.Rows[i]["InvoiceDate"] = SumAllDays
                            ? Simulate.DateString(Date1) + " - " + Simulate.DateString(Date2)
                            : Simulate.DateString(
                                Simulate.StringToDate(dt.Rows[i]["InvoiceDate"]));
                        ds.CashReport.Rows[i]["PaymentMethodID"] = Simulate.String(dt.Rows[i]["PaymentMethodID"]);
                        ds.CashReport.Rows[i]["PaymentMethod"] = Simulate.String(dt.Rows[i]["PaymentMethod"]);
                        ds.CashReport.Rows[i]["BusinessPartnerID"] = Simulate.String(dt.Rows[i]["BusinessPartnerID"]);
                        ds.CashReport.Rows[i]["BusinessPartner"] = Simulate.String(dt.Rows[i]["BusinessPartner"]);
                        ds.CashReport.Rows[i]["CreationUser"] = GroupByUser
                            ? Simulate.String(dt.Rows[i]["CreationUser"])
                            : string.Empty;
                        ds.CashReport.Rows[i]["InvoiceCount"] = Simulate.String(dt.Rows[i]["InvoiceCount"]);
                        ds.CashReport.Rows[i]["TotalTax"] = Simulate.decimal_(dt.Rows[i]["TotalTax"])
                            .ToString(System.Globalization.CultureInfo.InvariantCulture);
                        ds.CashReport.Rows[i]["HeaderDiscount"] = Simulate.decimal_(dt.Rows[i]["HeaderDiscount"])
                            .ToString(System.Globalization.CultureInfo.InvariantCulture);
                        ds.CashReport.Rows[i]["TotalDiscount"] = Simulate.decimal_(dt.Rows[i]["TotalDiscount"])
                            .ToString(System.Globalization.CultureInfo.InvariantCulture);
                        ds.CashReport.Rows[i]["TotalInvoice"] = Simulate.decimal_(dt.Rows[i]["TotalInvoice"])
                            .ToString(System.Globalization.CultureInfo.InvariantCulture);
                    }
                }





                FastReport.Report report = new FastReport.Report();

                // Separate shipped templates avoid FastReport Visible toggles (both layouts were rendering).
                string fallbackFrx;
                if (IsReceiptLayout)
                    fallbackFrx = GroupByUser ? "rptCashReportPOSGrouped" : "rptCashReportPOS";
                else
                    fallbackFrx = GroupByUser ? "rptCashReportGrouped" : "rptCashReport";

                // Load first, then bind — RegisterData before Load is wiped by Load().
                // A4 and receipt templates both need the live table wired after Load
                // (same pattern as ctlPOSOpsReports.BuildPosOpsPdf).
                report.Load(clsReports.getStandardGlobalPath(fallbackFrx));
                BindPosStyleReportData(report, ds);
                if (BranchID == 0)
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
                    DataTable dtCash = clsCashDrawer.SelectCashDrawerByID(CashID, "", "", CompanyID);
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
                return BadRequest("Print error: " + ex.Message);
            }

        }

        /// <summary>
        /// POS / receipt .frx files use bare TableDataSource nodes. RegisterData alone
        /// does not wire the live table — set Enabled, ReferenceName, and Table explicitly
        /// (same approach as ctlPOSOpsReports.BuildPosOpsPdf).
        /// </summary>
        private static void BindPosStyleReportData(FastReport.Report report, System.Data.DataSet data)
        {
            if (report == null || data == null)
                return;

            System.Data.DataSet anon = new System.Data.DataSet();
            foreach (System.Data.DataTable table in data.Tables)
            {
                if (table == null || string.IsNullOrWhiteSpace(table.TableName))
                    continue;
                if (anon.Tables.Contains(table.TableName))
                    continue;
                System.Data.DataTable copy = table.Copy();
                copy.TableName = table.TableName;
                anon.Tables.Add(copy);
            }

            try { report.RegisterData(anon); } catch { }

            foreach (System.Data.DataTable table in anon.Tables)
            {
                try { report.RegisterData(table, table.TableName); } catch { }

                object src = null;
                try { src = report.GetDataSource(table.TableName); } catch { }
                if (src == null)
                    continue;

                try
                {
                    src.GetType().GetProperty("Enabled")?.SetValue(src, true);
                    var refProp = src.GetType().GetProperty("ReferenceName");
                    if (refProp != null && refProp.CanWrite)
                        refProp.SetValue(src, table.TableName);
                    var tableProp = src.GetType().GetProperty("Table");
                    if (tableProp != null && tableProp.CanWrite)
                        tableProp.SetValue(src, table);
                }
                catch { }
            }

            try
            {
                for (int i = 0; i < report.Dictionary.DataSources.Count; i++)
                    report.Dictionary.DataSources[i].Enabled = true;
            }
            catch { }
        }

        #endregion


        #region Print Invoice
        
        

        private void TrySubmitEInvoiceOnPrint(string guid, int companyId)
        {
            clsEInvoiceConfigurations clsEInvoiceConfigurations = new clsEInvoiceConfigurations();
            DataTable dtInvoiceConf = clsEInvoiceConfigurations.SelectEInvoiceConfigurations(
                0, "", "", companyId);

            int submitSalesInvoices = 0;
            int submitSalesReturnInvoices = 0;
            int submitPosSalesInvoices = 0;
            int submitPosSalesReturnInvoices = 0;

            if (dtInvoiceConf != null && dtInvoiceConf.Rows.Count > 0)
            {
                submitSalesInvoices = Simulate.Integer32(dtInvoiceConf.Rows[0]["SubmitSalesInvoices"]);
                submitSalesReturnInvoices = Simulate.Integer32(dtInvoiceConf.Rows[0]["SubmitSalesReturnInvoices"]);
                submitPosSalesInvoices = Simulate.Integer32(dtInvoiceConf.Rows[0]["SubmitPOSSalesInvoices"]);
                submitPosSalesReturnInvoices = Simulate.Integer32(dtInvoiceConf.Rows[0]["SubmitPOSSalesReturnInvoices"]);
            }

            clsInvoiceHeader clsInvoiceHeader = new clsInvoiceHeader();
            DataTable dtHeader = clsInvoiceHeader.SelectInvoiceHeaderByGuid(
                guid, DateTime.Now.AddYears(-100), DateTime.Now.AddYears(100), 0, 0, 0, companyId);

            if (dtHeader == null || dtHeader.Rows.Count == 0)
                return;

            clsEInvoiceService clsEInvoiceService = new clsEInvoiceService();
            int invoiceTypeId = Simulate.Integer32(dtHeader.Rows[0]["InvoiceTypeID"]);

            if (invoiceTypeId == (int)clsEnum.VoucherType.POSSalesInvoice
                && submitPosSalesInvoices == (int)clsEnum.InvoiceTaxSubmitTypes.SubmitOnlyOnPrint)
            {
                clsEInvoiceService.SubmitEInvoice(companyId, guid, "", "");
            }
            else if (invoiceTypeId == (int)clsEnum.VoucherType.POSSalesInvoicereturn
                && submitPosSalesReturnInvoices == (int)clsEnum.InvoiceTaxSubmitTypes.SubmitOnlyOnPrint)
            {
                clsEInvoiceService.SubmitEInvoice(companyId, guid, "", "");
            }
            else if (invoiceTypeId == (int)clsEnum.VoucherType.SalesInvoice
                && submitSalesInvoices == (int)clsEnum.InvoiceTaxSubmitTypes.SubmitOnlyOnPrint)
            {
                clsEInvoiceService.SubmitEInvoice(companyId, guid, "", "");
            }
            else if (invoiceTypeId == (int)clsEnum.VoucherType.SalesRefund
                && submitSalesReturnInvoices == (int)clsEnum.InvoiceTaxSubmitTypes.SubmitOnlyOnPrint)
            {
                clsEInvoiceService.SubmitEInvoice(companyId, guid, "", "");
            }
        }

        [HttpGet]
        [Route("SelectInvoicePDF")]
        public IActionResult SelectInvoicePDF(
            string guid, int UserId, int CompanyID, int TransactionReportID = 0)
        {
            try
            {
                TrySubmitEInvoiceOnPrint(guid, CompanyID);
                // Persist print flag whenever invoice PDF is generated for print/reprint.
                try
                {
                    new clsInvoiceHeader().MarkInvoiceAsPrinted(guid, UserId, CompanyID);
                }
                catch
                {
                    // Non-blocking: print should still proceed if flag update fails.
                }
                return PrintTransactionReportPdf(
                    clsTransactionReportPrint.PageInvoicePageAdd,
                    guid,
                    UserId,
                    CompanyID,
                    TransactionReportID);
            }
            catch (Exception ex)
            {
                return BadRequest("Print error: " + ex.Message);
            }
        }

        #endregion

        #region Print Invoice


        [HttpGet]
        [Route("SelectJVPDF")]
        public IActionResult SelectJVPDF(string guid, int UserId, int CompanyID, int TransactionReportID = 0)
        {
            return PrintTransactionReportPdf(
                clsTransactionReportPrint.PageJournalVoucherAdd,
                guid,
                UserId,
                CompanyID,
                TransactionReportID);
        }

        [HttpGet]
        [Route("PreviewTransactionReportPDF")]
        public IActionResult PreviewTransactionReportPDF(
            string PageName,
            int TransactionReportID,
            int UserId,
            int CompanyID,
            string HeaderGuid = "")
        {
            try
            {
                PageName = Simulate.String(PageName);
                if (string.IsNullOrWhiteSpace(PageName) && TransactionReportID <= 0)
                    return BadRequest("PageName or TransactionReportID is required.");

                clsTransactionReportPrint printer = new clsTransactionReportPrint();

                // Settings preview always uses sample data so every layout can be
                // opened without requiring a saved transaction in the company DB.
                if (TransactionReportID > 0 || string.IsNullOrWhiteSpace(HeaderGuid))
                {
                    if (string.IsNullOrWhiteSpace(PageName) && TransactionReportID > 0)
                    {
                        DataTable dt = new clsTransactionReport()
                            .SelectTransactionReportByID(TransactionReportID, CompanyID);
                        if (dt != null && dt.Rows.Count > 0)
                            PageName = Simulate.String(dt.Rows[0]["PageName"]);
                    }

                    byte[] samplePdf = printer.BuildSamplePreviewPdf(
                        PageName, UserId, CompanyID, TransactionReportID);
                    string sampleName = $"{PageName}_{TransactionReportID}_sample.pdf";
                    return File(samplePdf, "application/pdf", sampleName);
                }

                return PrintTransactionReportPdf(
                    PageName, HeaderGuid, UserId, CompanyID, TransactionReportID);
            }
            catch (Exception ex)
            {
                return BadRequest("Preview error: " + ex.Message);
            }
        }

        [HttpGet]
        [Route("PrintTransactionReportPDF")]
        public IActionResult PrintTransactionReportPDF(
            string PageName,
            string HeaderGuid,
            int UserId,
            int CompanyID,
            int TransactionReportID = 0)
        {
            return PrintTransactionReportPdf(PageName, HeaderGuid, UserId, CompanyID, TransactionReportID);
        }

        private IActionResult PrintTransactionReportPdf(
            string pageName,
            string headerGuid,
            int userId,
            int companyId,
            int transactionReportId = 0)
        {
            try
            {
                pageName = Simulate.String(pageName);
                if (string.IsNullOrWhiteSpace(pageName))
                    return BadRequest("PageName is required.");

                if (string.IsNullOrWhiteSpace(headerGuid))
                {
                    clsTransactionReport trCls = new clsTransactionReport();
                    headerGuid = trCls.SelectLatestHeaderGuidForPage(pageName, companyId);
                }

                if (string.IsNullOrWhiteSpace(headerGuid))
                    return BadRequest("No transaction found. Save one first to print or preview.");

                clsTransactionReportPrint printer = new clsTransactionReportPrint();
                byte[] pdfBytes = printer.BuildTransactionReportPdf(
                    headerGuid, pageName, userId, companyId, transactionReportId);

                string fileName = $"{pageName}_{transactionReportId}.pdf";
                return File(pdfBytes, "application/pdf", fileName);
            }
            catch (Exception ex)
            {
                return BadRequest("Print error: " + ex.Message);
            }
        }

        private IActionResult PrintJournalVoucherPdf(string guid, int UserId, int CompanyID, int transactionReportId = 0)
        {
            return PrintTransactionReportPdf(
                clsTransactionReportPrint.PageJournalVoucherAdd,
                guid,
                UserId,
                CompanyID,
                transactionReportId);
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
                return Fastreporttoxlsx(dtList, dtName, ColumnType);


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

                DataTable dt = cls.ExecuteQueryStatement(@"    
 select * ,   q.[المستحق]-q.[المدفوع] as  N'الفرق' from (
 select   tbl_LoanTypes.Code as N'النوع',
 tbl_BusinessPartner.EmpCode as N'الرقم',
  tbl_BusinessPartner.AName  as N'الاسم',
 --(select sum(Total )  from tbl_JournalVoucherDetails 
 --inner join tbl_JournalVoucherHeader on tbl_JournalVoucherHeader.Guid= tbl_JournalVoucherDetails.ParentGuid where
 -- SubAccountID =tbl_BusinessPartner.id and AccountID = 826
 --and tbl_JournalVoucherHeader.RelatedLoanTypeID = tbl_LoanTypes.ID   ) as N'رصيد الذمم',




 (select sum(Total ) 
 
 - (
 
    select sum(amount) from tbl_Reconciliation where JVDetailsGuid in (
   select tbl_JournalVoucherDetails.Guid from tbl_JournalVoucherHeader 
  left join tbl_JournalVoucherDetails on tbl_JournalVoucherDetails.ParentGuid = tbl_JournalVoucherHeader.Guid
  where   SubAccountID =tbl_BusinessPartner.id and AccountID = @accountid
 and tbl_JournalVoucherHeader.RelatedLoanTypeID = tbl_LoanTypes.ID 
 and tbl_JournalVoucherHeader.JVTypeID in (14,15))
 )
 from tbl_JournalVoucherDetails 
 inner join tbl_JournalVoucherHeader on tbl_JournalVoucherHeader.Guid= tbl_JournalVoucherDetails.ParentGuid where
  SubAccountID =tbl_BusinessPartner.id and AccountID = @accountid
 and tbl_JournalVoucherHeader.RelatedLoanTypeID = tbl_LoanTypes.ID 
 and tbl_JournalVoucherHeader.JVTypeID in (14,15)
 )
  
 as N'رصيد الذمم',
   --sum(debit)   as N'الشهري',








isnull((
 select * from  GetSumDueUnReconciledAmountByFinanceGuid (

@AccountID,
@duedate2 ,
tbl_BusinessPartner.ID ,
@CompanyID,'00000000-0000-0000-0000-000000000000',
tbl_LoanTypes.ID 
) 
)
+isnull((select sum(total*-1) from tbl_JournalVoucherDetails 
left join tbl_JournalVoucherHeader on tbl_JournalVoucherHeader.Guid = tbl_JournalVoucherDetails.ParentGuid
where
 JVTypeID = 16 and VoucherDate between @duedate1 and @duedate2
 and RelatedLoanTypeID =  tbl_LoanTypes.ID
 and tbl_JournalVoucherDetails.AccountID =  @accountid 
 and tbl_JournalVoucherDetails.SubAccountID = tbl_BusinessPartner.id 
 ),0),0) 
as N'المستحق', 
 isnull( (select sum(Credit )  from tbl_JournalVoucherDetails 
 inner join tbl_JournalVoucherHeader on tbl_JournalVoucherHeader.Guid= tbl_JournalVoucherDetails.ParentGuid where
 JVTypeID = 16 and SubAccountID =tbl_BusinessPartner.id 
 and tbl_JournalVoucherHeader.RelatedLoanTypeID = tbl_LoanTypes.ID and DueDate between  @DueDate1 and @DueDate2 ),0) as N'المدفوع' 


 -- sum(debit)  - isnull( (select sum(Credit )  from tbl_JournalVoucherDetails 
 --inner join tbl_JournalVoucherHeader on tbl_JournalVoucherHeader.Guid= tbl_JournalVoucherDetails.ParentGuid where
 --JVTypeID = 16 and SubAccountID =tbl_BusinessPartner.id 
 --and tbl_JournalVoucherHeader.RelatedLoanTypeID = tbl_LoanTypes.ID and DueDate between  @DueDate1 and @DueDate2 ),0)  as  N'الفرق' 
 
 
 from tbl_JournalVoucherDetails 
 left join tbl_BusinessPartner on tbl_BusinessPartner.id = tbl_JournalVoucherDetails.SubAccountID
 inner join tbl_JournalVoucherHeader on tbl_JournalVoucherHeader.Guid = tbl_JournalVoucherDetails.ParentGuid
 left join tbl_LoanTypes on tbl_JournalVoucherHeader.RelatedLoanTypeID = tbl_LoanTypes.ID
 where RelatedLoanTypeID > 0
 and DueDate between  @DueDate1 and @DueDate2
 and AccountID = @AccountID
 
and tbl_JournalVoucherHeader.CompanyID=@CompanyID
 group by tbl_BusinessPartner.EmpCode,tbl_BusinessPartner.AName,tbl_BusinessPartner.id ,tbl_LoanTypes.Code,tbl_LoanTypes.ID
 ) as q", cls.CreateDataBaseConnectionString(CompanyID), prm);


               
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
        //[HttpPost]
        //[Route("InsertItems")]
        //public string InsertItems(string AName, string EName, string Description, decimal SalesPriceBeforeTax, decimal SalesPriceAfterTax, int CategoryID, int SalesTaxID
        //    , int SpecialSalesTaxID, int PurchaseTaxID, int SpecialPurchaseTaxID, string Barcode, int ReadType, int OriginID, decimal MinimumLimit, [FromBody] string Picture
        //    , bool IsActive, bool IsPOS, int BoxTypeID, bool IsStockItem, int POSOrder, bool TrackLot, bool TrackSerial, bool TrackExpiryDate, int CompanyID, int CreationUserId)
        //{
        //    try
        //    {
        //        byte[] myPicture = new Byte[64];
        //        if (myPicture != null && myPicture.Length > 0)
        //        {
        //            myPicture = Convert.FromBase64String(Picture);
        //        }
        //        else
        //        {

        //            myPicture = null;
        //        }

        //        clsItems clsItems = new clsItems();
        //        String A = clsItems.InsertItems(Simulate.String(AName), Simulate.String(EName), Simulate.String(Description),
        //            Simulate.decimal_(SalesPriceBeforeTax), Simulate.decimal_(SalesPriceAfterTax), CategoryID, SalesTaxID
        //    , SpecialSalesTaxID, PurchaseTaxID, SpecialPurchaseTaxID, Simulate.String(Barcode), ReadType, OriginID, MinimumLimit, myPicture
        //    , IsActive, IsPOS, BoxTypeID, IsStockItem, POSOrder,  TrackLot,  TrackSerial,  TrackExpiryDate, CompanyID, CreationUserId);



        //        return A;
        //    }
        //    catch (Exception ex)
        //    {

        //        throw;
        //    }

        //}
        private byte[] DecodeBase64Image(string base64)
        {
            try
            {          byte[] myPicture = new Byte[64];
                if (myPicture != null && myPicture.Length > 0)
                {
                    myPicture = Convert.FromBase64String(base64);
                }
                else
                {

                    myPicture = null;
                }return myPicture;
                //if (string.IsNullOrWhiteSpace(base64))
                //    return null;

                //// support: data:image/png;base64,...
                //int commaIndex = base64.IndexOf(',');
                //if (commaIndex > -1 && base64.Substring(0, commaIndex).Contains("base64"))
                //    base64 = base64.Substring(commaIndex + 1);

                //return Convert.FromBase64String(base64);
            }
            catch
            {
                return null; // fail-safe
            }
        }
        [HttpPost]
        [Route("InsertItems")]
        public string InsertItems(
     string AName, string EName, string Description,
     decimal SalesPriceBeforeTax, decimal SalesPriceAfterTax,
     int CategoryID, int SalesTaxID, int SpecialSalesTaxID,
     int PurchaseTaxID, int SpecialPurchaseTaxID,
     string Barcode, int ReadType, int OriginID,
     decimal MinimumLimit, [FromBody] string Picture,
     bool IsActive, bool IsPOS, int BoxTypeID, bool IsStockItem, int POSOrder,
     bool TrackLot, bool TrackSerial, bool TrackExpiryDate,

     // 🔹 NEW PARAMETERS (added only)
     string ItemCode, int ItemTypeID,
     int BrandID, int ManufacturerID, string ModelNo,
     int BaseUOMID, int SalesUOMID, int PurchaseUOMID,
     decimal StandardCost, decimal LastPurchaseCost,
     bool IsWeightedItem, bool IsOpenPrice,
     bool AllowNegativeStock,
     int ShelfLifeDays, int ExpiryWarningDays,
      string ParentGuid,
decimal BaseFactor,
     int CompanyID, int CreationUserId
 )
        {
            try
            {
                byte[] myPicture = DecodeBase64Image(Picture);

                clsItems clsItems = new clsItems();
                string A = clsItems.InsertItems(
                    Simulate.String(AName),
                    Simulate.String(EName),
                    Simulate.String(Description),
                    Simulate.decimal_(SalesPriceBeforeTax),
                    Simulate.decimal_(SalesPriceAfterTax),
                    CategoryID,
                    SalesTaxID,
                    SpecialSalesTaxID,
                    PurchaseTaxID,
                    SpecialPurchaseTaxID,
                    Simulate.String(Barcode),
                    ReadType,
                    OriginID,
                    MinimumLimit,
                    myPicture,
                    IsActive,
                    IsPOS,
                    BoxTypeID,
                    IsStockItem,
                    POSOrder,
                    TrackLot,
                    TrackSerial,
                    TrackExpiryDate,

                    // NEW
                    Simulate.String(ItemCode),
                    ItemTypeID,
                    BrandID,
                    ManufacturerID,
                    Simulate.String(ModelNo),
                    BaseUOMID,
                    SalesUOMID,
                    PurchaseUOMID,
                    StandardCost,
                    LastPurchaseCost,
                    IsWeightedItem,
                    IsOpenPrice,
                    AllowNegativeStock,
                    ShelfLifeDays,
                    ExpiryWarningDays,
                       ParentGuid,
  BaseFactor,
                    CompanyID,
                    CreationUserId
                );

                return A;
            }
            catch
            {
                throw;
            }
        }
        //[HttpPost]
        //[Route("UpdateItems")]
        //public int UpdateItems(string Guid, string AName, string EName, string Description, decimal SalesPriceBeforeTax, decimal SalesPriceAfterTax, int CategoryID, int SalesTaxID
        //    , int SpecialSalesTaxID, int PurchaseTaxID, int SpecialPurchaseTaxID, string Barcode, int ReadType, int OriginID, decimal MinimumLimit, [FromBody] string Picture
        //    , bool IsActive, bool IsPOS, int BoxTypeID, bool IsStockItem, int POSOrder, bool TrackLot, bool TrackSerial, bool TrackExpiryDate, int ModificationUserId, int CompanyID)
        //{
        //    try
        //    {



        //        byte[] myPicture = new Byte[64];
        //        if (myPicture != null && myPicture.Length > 0)
        //        {
        //            myPicture = Convert.FromBase64String(Picture);
        //        }
        //        else
        //        {

        //            myPicture = null;
        //        }
        //        clsItems clsItems = new clsItems();
        //        int A = clsItems.UpdateItems(Guid, Simulate.String(AName), Simulate.String(EName), Simulate.String(Description),
        //            Simulate.decimal_(SalesPriceBeforeTax), Simulate.decimal_(SalesPriceAfterTax), CategoryID, SalesTaxID
        //    , SpecialSalesTaxID, PurchaseTaxID, SpecialPurchaseTaxID, Simulate.String(Barcode), ReadType, OriginID, MinimumLimit, myPicture
        //    , IsActive, IsPOS, BoxTypeID, IsStockItem, POSOrder,  TrackLot,  TrackSerial,  TrackExpiryDate, ModificationUserId, CompanyID);
        //        return A;
        //    }
        //    catch (Exception)
        //    {

        //        throw;
        //    }

        //}
        [HttpPost]
        [Route("UpdateItems")]
        public int UpdateItems(
    string Guid,
    string AName, string EName, string Description,
    decimal SalesPriceBeforeTax, decimal SalesPriceAfterTax,
    int CategoryID, int SalesTaxID, int SpecialSalesTaxID,
    int PurchaseTaxID, int SpecialPurchaseTaxID,
    string Barcode, int ReadType, int OriginID,
    decimal MinimumLimit, [FromBody] string Picture,
    bool IsActive, bool IsPOS, int BoxTypeID, bool IsStockItem, int POSOrder,
    bool TrackLot, bool TrackSerial, bool TrackExpiryDate,

    // 🔹 NEW PARAMETERS (added only)
    string ItemCode, int ItemTypeID,
    int BrandID, int ManufacturerID, string ModelNo,
    int BaseUOMID, int SalesUOMID, int PurchaseUOMID,
    decimal StandardCost, decimal LastPurchaseCost,
    bool IsWeightedItem, bool IsOpenPrice,
    bool AllowNegativeStock,
    int ShelfLifeDays, int ExpiryWarningDays,
     string ParentGuid,
decimal BaseFactor,
    int ModificationUserId, int CompanyID
)
        {
            try
            {
                byte[] myPicture = DecodeBase64Image(Picture);

                clsItems clsItems = new clsItems();
                int A = clsItems.UpdateItems(
                    Guid,
                    Simulate.String(AName),
                    Simulate.String(EName),
                    Simulate.String(Description),
                    Simulate.decimal_(SalesPriceBeforeTax),
                    Simulate.decimal_(SalesPriceAfterTax),
                    CategoryID,
                    SalesTaxID,
                    SpecialSalesTaxID,
                    PurchaseTaxID,
                    SpecialPurchaseTaxID,
                    Simulate.String(Barcode),
                    ReadType,
                    OriginID,
                    MinimumLimit,
                    myPicture,
                    IsActive,
                    IsPOS,
                    BoxTypeID,
                    IsStockItem,
                    POSOrder,
                    TrackLot,
                    TrackSerial,
                    TrackExpiryDate,

                    // NEW
                    Simulate.String(ItemCode),
                    ItemTypeID,
                    BrandID,
                    ManufacturerID,
                    Simulate.String(ModelNo),
                    BaseUOMID,
                    SalesUOMID,
                    PurchaseUOMID,
                    StandardCost,
                    LastPurchaseCost,
                    IsWeightedItem,
                    IsOpenPrice,
                    AllowNegativeStock,
                    ShelfLifeDays,
                    ExpiryWarningDays,
                       ParentGuid,
  BaseFactor,
                    ModificationUserId,
                    CompanyID
                );

                return A;
            }
            catch
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
        public int InsertCountries(string AName, string EName,string NationalityAName,string NationalityEName, int CompanyID, int CreationUserId)
        {
            try
            {
                clsCountries clsCountries = new clsCountries();
                int A = clsCountries.InsertCountries(Simulate.String(AName), Simulate.String(EName),
                    Simulate.String(NationalityAName), Simulate.String(NationalityEName), CompanyID, CreationUserId);
                return A;
            }
            catch (Exception ex)
            {

                throw;
            }

        }
        [HttpGet]
        [Route("UpdateCountries")]
        public int UpdateCountries(int ID, string AName, string EName, String NationalityAName , String NationalityEName , int ModificationUserId,int CompanyID)
        {
            try
            {
                clsCountries clsCountries = new clsCountries();
                int A = clsCountries.UpdateCountries(ID, Simulate.String(AName), Simulate.String(EName),
                    Simulate.String(NationalityAName), Simulate.String(NationalityEName), 
                    ModificationUserId, CompanyID);
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
        public string SelectInvoiceHeaderByGuid(string Guid, int BranchID, int InvoiceTypeID,int TableID, int CompanyID, DateTime Date1, DateTime Date2)
        {
            try
            {
                clsInvoiceHeader clsInvoiceHeader = new clsInvoiceHeader();
                DataTable dt = clsInvoiceHeader.SelectInvoiceHeaderByGuid(Simulate.String(Guid), Date1, Date2, InvoiceTypeID, BranchID, TableID, CompanyID);
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
                clsInvoiceHeader clsInvoiceHeader= new clsInvoiceHeader();
                    bool IsSaved = clsInvoiceHeader.DeleteInvoiceDetailsByHeaderGuid(Guid, CompanyID);
                    return IsSaved;
                //    clsInvoiceDetails clsInvoiceDetails = new clsInvoiceDetails();
                //    clsInvoiceHeader clsInvoiceHeader = new clsInvoiceHeader();

                //    clsJournalVoucherHeader clsJournalVoucherHeader = new clsJournalVoucherHeader();
                //    clsJournalVoucherDetails clsJournalVoucherDetails = new clsJournalVoucherDetails();
                //    SqlTransaction trn; clsSQL clsSQL = new clsSQL();
                //    SqlConnection con = new SqlConnection(clsSQL.CreateDataBaseConnectionString(CompanyID));
                //    con.Open();
                //    trn = con.BeginTransaction(); int A = 0;
                //    bool IsSaved = true;
                //    try
                //    {
                //        DataTable dt = clsInvoiceHeader.SelectInvoiceHeaderByGuid(Guid, Simulate.StringToDate("1900-01-01"), Simulate.StringToDate("2300-01-01"), 0, 0, 0, 0, trn);
                //        IsSaved = clsInvoiceHeader.DeleteInvoiceHeaderByGuid(Guid,CompanyID, trn);
                //        bool a = clsInvoiceDetails.DeleteInvoiceDetailsByHeaderGuid(Guid,CompanyID, trn);
                //        if (dt != null && dt.Rows.Count > 0)
                //        {
                //            string JVGuid = Simulate.String(dt.Rows[0]["JVGuid"]);
                //            bool aa = clsJournalVoucherHeader.DeleteJournalVoucherHeaderByID(JVGuid,CompanyID, trn);
                //            bool aaa = clsJournalVoucherDetails.DeleteJournalVoucherDetailsByParentId(JVGuid,CompanyID, trn);
                //        }
                //        if (!a)
                //            IsSaved = false;


                //        if (IsSaved)
                //            trn.Commit();
                //        else
                //            trn.Rollback();
                //    }
                //    catch (Exception)
                //    {
                //        trn.Rollback();

                //    }
                //    finally { con.Close(); }


              //  return IsSaved;
            }
            catch (Exception)
            {

                throw;
            }

        }
        [HttpPost]
        [Route("InsertInvoiceHeader")]

        public IActionResult InsertInvoiceHeader(int branchID, int storeID, int businessPartnerID
            , int cashID,  int bankid, string refNo, int invoiceNo, decimal headerDiscount
            , int invoiceTypeID, bool isCounted, string note, int companyID,
            decimal totalTax, string pOSDayGuid, string relatedInvoiceGuid,
            decimal totalDiscount, int paymentMethodID,
            string pOSSessionGuid, decimal totalInvoice,
            DateTime invoiceDate, int creationUserId, int accountID, int tableID, int status,
              int CurrencyID, decimal CurrencyBaseAmount, decimal CurrencyRate,
            [FromBody] string DetailsList, string clientRequestId = null, string BudgetOverrideReason = "", int costCenterID = 0)

        {

            try
            {
                clsSQL clsSQL = new clsSQL();
                clsInvoiceHeader clsInvoiceHeader = new clsInvoiceHeader();
                SqlTransaction trn;
                SqlConnection con = new SqlConnection(clsSQL.CreateDataBaseConnectionString(companyID));
                con.Open();
                trn = con.BeginTransaction(); try
                {
                    var result = clsInvoiceHeader.InsertInvoiceHeaderWithDetails(branchID, costCenterID, storeID, businessPartnerID
          , cashID, bankid, refNo, invoiceNo, headerDiscount
          , invoiceTypeID, isCounted, note, companyID,
           totalTax, pOSDayGuid, relatedInvoiceGuid,
           totalDiscount, paymentMethodID,
           pOSSessionGuid, totalInvoice,
           invoiceDate, creationUserId, accountID, tableID, status,
             CurrencyID, CurrencyBaseAmount, CurrencyRate,
           DetailsList, trn, false, clientRequestId, BudgetOverrideReason);
                    if (!result.Success)
                    {
                        trn.Rollback();
                        if (!string.IsNullOrEmpty(result.Message) && result.Message.StartsWith("BUDGET_OVER:"))
                            return BadRequest(result);
                        return BadRequest(result); // 400 with message
                    }


                    trn.Commit();

                    if (result.Success && !string.IsNullOrEmpty(result.Message) &&
                        result.Message.StartsWith("BUDGET_OVERRIDE_PENDING:") &&
                        !string.IsNullOrEmpty(result.Data))
                    {
                        List<BudgetBreach> invoiceBreaches = null;
                        int pipe = result.Message.IndexOf('|');
                        if (pipe > 0 && pipe < result.Message.Length - 1)
                        {
                            try
                            {
                                invoiceBreaches = JsonConvert.DeserializeObject<List<BudgetBreach>>(
                                    result.Message.Substring(pipe + 1));
                            }
                            catch { }
                        }
                        string ovErr = new clsBudget().CompleteBudgetOverride(
                            "tbl_InvoiceHeader", companyID, creationUserId, invoiceTypeID, result.Data,
                            Simulate.String(invoiceNo), BudgetOverrideReason, invoiceBreaches);
                        if (ovErr != null)
                            return BadRequest(ApiResponse<string>.Fail(ovErr));
                    }

                    return Ok(result); // 200 with invoice guid

               
                        
                        
                        
                        
            

            }
            catch (Exception ex)
            {
                    try { trn.Rollback(); } catch { }
                    return StatusCode(500, ApiResponse<string>.Fail($"Server error: {ex.Message}"));

                }
                finally { con.Close(); }
                

            }
            catch (Exception ex)
            {

                return StatusCode(500, ApiResponse<string>.Fail($"Server error: {ex.Message}"));

            }

        }
        [Route("UpdateInvoiceHeader")]
        public IActionResult UpdateInvoiceHeader(int branchID, int storeID, int businessPartnerID
            , int cashID,   int bankid, string refNo, int invoiceNo, decimal headerDiscount
            , int invoiceTypeID, bool isCounted, string note,
            decimal totalTax, string pOSDayGuid, string relatedInvoiceGuid,
            decimal totalDiscount, int paymentMethodID,
            string pOSSessionGuid, decimal totalInvoice,
            DateTime invoiceDate, int modificationUserID, string guid, int accountID,int compnayid,int tableID,int status,
              int CurrencyID, decimal CurrencyBaseAmount, decimal CurrencyRate,
            [FromBody] string DetailsList)
        {




 clsSQL clsSQL = new clsSQL();
            using var con = new SqlConnection(clsSQL.CreateDataBaseConnectionString(compnayid));
            con.Open();
            using var trn = con.BeginTransaction();

            try
            {
                if (string.IsNullOrWhiteSpace(guid))
                    return BadRequest(ApiResponse<string>.Fail("Invoice header guid is required."));


                DBInvoiceHeader dbInvoiceHeader = new DBInvoiceHeader
                {
                    CurrencyID = Simulate.Integer32(CurrencyID),
                    CurrencyBaseAmount = Simulate.decimal_(CurrencyBaseAmount),
                    CurrencyRate = Simulate.decimal_(CurrencyRate),
                    BranchID = branchID,
                    StoreID = storeID,
                    status = Simulate.Integer32(status),
                    tableID = Simulate.Integer32(tableID),
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
 

                List<DBInvoiceDetails> details;
                try
                {
                    details = JsonConvert.DeserializeObject<List<DBInvoiceDetails>>(DetailsList) ;
                }
                catch (Exception ex)
                {
                    trn.Rollback();
                    return BadRequest(ApiResponse<string>.Fail("Invalid DetailsList JSON: " + ex.Message));
                }

                if (details.Count == 0)
                {
                    trn.Rollback();
                    return BadRequest(ApiResponse<string>.Fail("Invoice details are empty."));
                }








                clsInvoiceHeader clsInvoiceHeader = new clsInvoiceHeader();
                clsInvoiceDetails clsInvoiceDetails = new clsInvoiceDetails();
 
            
                try
                {
                    DataTable dtExisting = clsInvoiceHeader.SelectInvoiceHeaderByGuid(
                        guid,
                        Simulate.StringToDate("1900-01-01"),
                        Simulate.StringToDate("2300-01-01"),
                        0, 0, 0, compnayid,
                        trn);
                    int documentStatus = (int)clsEnum.DocumentStatus.Posted;
                    if (dtExisting != null && dtExisting.Rows.Count > 0)
                    {
                        if (dtExisting.Columns.Contains("DocumentStatus"))
                            documentStatus = Simulate.Integer32(dtExisting.Rows[0]["DocumentStatus"]);
                        if (documentStatus == (int)clsEnum.DocumentStatus.PendingApproval)
                        {
                            trn.Rollback();
                            return BadRequest(ApiResponse<string>.Fail("Document is pending approval and cannot be edited."));
                        }
                    }

                    var headerUpdateResult = clsInvoiceHeader.UpdateInvoiceHeader(dbInvoiceHeader,compnayid, trn);
                    if (string.IsNullOrWhiteSpace(headerUpdateResult))
                    {
                        trn.Rollback();
                        return BadRequest(ApiResponse<string>.Fail("Failed to update invoice header."));
                    }



                    clsInvoiceDetails.DeleteInvoiceDetailsByHeaderGuid(guid,compnayid, trn);
                    var hasTrackedLines = details.Exists(d =>
                        d.TrackLot || d.TrackSerial || d.TrackExpiryDate);
                    if (hasTrackedLines)
                    {
                        clsInvoiceDetailsLotsTracking clsInvoiceDetailsLotsTrackingCleanup =
                            new clsInvoiceDetailsLotsTracking();
                        clsInvoiceDetailsLotsSerialNumber clsInvoiceDetailsLotsSerialNumberCleanup =
                            new clsInvoiceDetailsLotsSerialNumber();
                        clsInvoiceDetailsLotsTrackingCleanup.DeleteInvoiceDetailsLotsTrackingByGuid(
                            Simulate.Guid(guid), compnayid, trn);
                        clsInvoiceDetailsLotsSerialNumberCleanup.DeleteInvoiceDetailsLotSerialNumberByGuid(
                            Simulate.Guid(guid), compnayid, trn);
                    }
                    clsItems clsitems = new clsItems();
                    for (int i = 0; i < details.Count; i++)
                    {

                        string detailGuid = clsInvoiceDetails.InsertInvoiceDetails(details[i], guid, trn);
                        if (string.IsNullOrWhiteSpace(detailGuid))
                        {
                            trn.Rollback();
                            return BadRequest(ApiResponse<string>.Fail($"Failed to insert invoice line #{i + 1}."));
                        }
                        details[i].Guid = Simulate.Guid(detailGuid);
                        if (details[i].InvoiceTypeID == (int)clsEnum.VoucherType.PurchaseInvoice ||
                            details[i].InvoiceTypeID == (int)clsEnum.VoucherType.GoodRecipt ||
                            details[i].InvoiceTypeID == (int)clsEnum.VoucherType.PurchaseInvoiceFromFinancing ||
                            details[i].InvoiceTypeID == (int)clsEnum.VoucherType.manufacturingOrderOutput)
                        {
                            if (documentStatus == (int)clsEnum.DocumentStatus.Posted)
                            {
                                clsitems.UpdateItemCost(
                                    details[i].ItemGuid.ToString(),
                                    details[i].TotalQTY,
                                    details[i].PriceBeforeTax - details[i].DiscountBeforeTaxAmountPcs,
                                    compnayid,
                                    trn);
                            }
                        }
                        if (detailGuid != "" && (details[i].TrackLot || details[i].TrackSerial || details[i].TrackExpiryDate))
                        {
                            var lotSaveResult = clsInvoiceHeader.SaveInvoiceLineLotSerialDetails(
                                details[i], detailGuid, guid, compnayid, modificationUserID, trn);
                            if (!lotSaveResult.Success)
                            {
                                trn.Rollback();
                                return BadRequest(ApiResponse<string>.Fail(
                                    $"Line #{i + 1}: {lotSaveResult.Message}"));
                            }
                        }
                    }

                   
                    if (documentStatus == (int)clsEnum.DocumentStatus.Posted)
                    {
                        string stockError = clsInvoiceHeader.ValidateStockAvailability(details, compnayid, trn);
                        if (!string.IsNullOrEmpty(stockError))
                        {
                            trn.Rollback();
                            return BadRequest(ApiResponse<string>.Fail(stockError));
                        }

                        new clsManufacturingOps().AssertMoReceiptAllowedIfLinked(
                            Simulate.String(dbInvoiceHeader.RelatedInvoiceGuid),
                            Simulate.Integer32(dbInvoiceHeader.InvoiceTypeID),
                            compnayid,
                            trn);
                        clsInvoiceHeader.ApplyManufacturingIssueAvgCosts(details, compnayid, trn);

                        var jvOk = clsInvoiceHeader.InsertInvoiceJournalVoucher(details, accountID, paymentMethodID,
                            cashID, bankid, businessPartnerID, headerDiscount, Simulate.Integer32(branchID),
                            Simulate.Integer32(0),//CostCenter
                            Simulate.String(note),compnayid, Simulate.StringToDate(invoiceDate), modificationUserID,
                            invoiceTypeID, guid, CurrencyID, CurrencyRate, trn);
                        if (!jvOk)
                        {
                            trn.Rollback();
                            return BadRequest(ApiResponse<string>.Fail("Updated invoice, but failed to create Journal Voucher."));
                        }
                        clsInvoiceHeader.ApplyInvoiceTableStatusOnPost(tableID, invoiceTypeID, compnayid, trn);
                    }

                    trn.Commit();
                    return Ok(ApiResponse<string>.Ok(guid, "Invoice updated successfully."));
                }
                catch (Exception ex)
                {
                    try { trn.Rollback(); } catch { }
                    return StatusCode(500, ApiResponse<string>.Fail("Server error: " + ex.Message));
                }
             
               
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
            bool IsCash, bool IsBank, bool IsDebit, bool ShowOnPOS, int CompanyID, int CreationUserId)
        {
            try
            {
                clsPaymentMethod clsPaymentMethod = new clsPaymentMethod();
                int A = clsPaymentMethod.InsertPaymentMethod(Simulate.String(AName), Simulate.String(EName), BranchID,
                     GLAccountID,  GLSubAccountID,
             IsCash,  IsBank,  IsDebit, ShowOnPOS, CompanyID, CreationUserId);
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
            bool IsCash, bool IsBank, bool IsDebit, bool ShowOnPOS, int ModificationUserId, int CompanyID )
        {
            try
            {
                clsPaymentMethod clsPaymentMethod = new clsPaymentMethod();
                int A = clsPaymentMethod.UpdatePaymentMethod(ID, Simulate.String(AName), Simulate.String(EName), BranchID, 
                    GLAccountID,  GLSubAccountID,
             IsCash,  IsBank,  IsDebit, ShowOnPOS, ModificationUserId, CompanyID);
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
        public String InsertPOSDay(DateTime StartDate, DateTime EndDate, DateTime POSDate, int Status, int CompanyID,int CashDrawerID, int CreationUserId)
        {
            try
            {
                clsPOSDay clsPOSDay = new clsPOSDay();
                String A = clsPOSDay.InsertPOSDay(StartDate, EndDate, POSDate, Status, CompanyID, CashDrawerID, CreationUserId);
                return JsonConvert.SerializeObject( A);
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
        public string OpenNewPOSDay(string Guid, DateTime NewDate, DateTime StartDate, DateTime EndDate, int CompanyID,int CashDrawerID, int CreationUserId)
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
                    int A = clsPOSDay.ClosePOSDay(Guid, EndDate, CreationUserId, CashDrawerID, CompanyID, trn);


                    string NewDay = clsPOSDay.InsertPOSDay(DateTime.Now, DateTime.Now, NewDate, 1, CompanyID,  CashDrawerID, CreationUserId, trn);
                    if (NewDay == "")
                    { IsSaved = false; }

                    if (IsSaved)
                    { trn.Commit(); return   NewDay; }
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
                    string NewSession = "";
                    if (SessionTypeID > 0)
                    {

                          NewSession = clsPOSSessions.InsertPOSSessions(POSDayGuid, SessionTypeID, NewDate, DateTime.Now, CashDrawerID, 1, CompanyID, CreationUserId, trn);
                    }
                    else {
                        NewSession = "00000000-0000-0000-0000-000000000000";
                    }
                    
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
        [HttpPost]
        [Route("CopyDasBoardWidget")]
        public string CopyDasBoardWidget(int ID, int userId, int companyId, string Title, bool enable = true)
        {
            try
            {
                clsSQL cls = new clsSQL();

                SqlParameter[] deleteParams = new SqlParameter[]
                {
                    new SqlParameter("@Title", SqlDbType.NVarChar,-1) { Value = Title ?? "" },
                    new SqlParameter("@newUserID", SqlDbType.Int) { Value = userId },
                    new SqlParameter("@CompanyID", SqlDbType.Int) { Value = companyId }
                };

                cls.ExecuteNonQueryStatement(
                    "delete from tbl_DashboardWidgets where Title = @Title AND CompanyID = @CompanyID and userId=@newUserID",
                    cls.CreateDataBaseConnectionString(companyId),
                    deleteParams);

                if (!enable)
                    return "Success";

                string query = @"
            INSERT INTO [dbo].[tbl_DashboardWidgets] 
            (UserId, WidgetType, GroupName, Title, SQLQuery, ChartConfig, Icon, Color, SectionName, SectionIndex, CreationDate, CreationUserID, ModificationDate, ModificationUserID, CompanyID, IsActive)
            SELECT 
                @newUserID, WidgetType, GroupName, Title, SQLQuery, ChartConfig, Icon, Color, SectionName, SectionIndex, GETDATE(), @newUserID, GETDATE(), @newUserID, CompanyID, 1
            FROM [dbo].[tbl_DashboardWidgets]
            WHERE ID = @ID AND CompanyID = @CompanyID";

                SqlParameter[] insertParams = new SqlParameter[]
                {
                    new SqlParameter("@ID", SqlDbType.Int) { Value = ID },
                    new SqlParameter("@newUserID", SqlDbType.Int) { Value = userId },
                    new SqlParameter("@CompanyID", SqlDbType.Int) { Value = companyId }
                };

                int result = cls.ExecuteNonQueryStatement(
                    query,
                    cls.CreateDataBaseConnectionString(companyId),
                    insertParams);

                return result > 0 ? "Success" : "Failed";
            }
            catch (Exception ex)
            {
                return "Error: " + ex.Message;
            }
        }

        [HttpPost]
        [Route("updateWidgetStatue")]
        public string updateWidgetStatue( int ID,bool newStatus, int UserID, int CompanyID)
        {
            try
            {
                 


                 

                clsSQL cls = new clsSQL();
                string query = @"
            UPDATE [dbo].[tbl_DashboardWidgets]
            SET 
                IsActive = @newStatus,
            
                ModificationUserID=@UserID,
                ModificationDate = GETDATE()
            WHERE 
                ID = @ID AND 
                UserID=@UserID AND 
                CompanyID = @CompanyID";
                int result = 0;
                 
                    SqlParameter[] parameters = new SqlParameter[]
                    {
                new SqlParameter("@ID", SqlDbType.Int) { Value = ID },
                new SqlParameter("@newStatus", SqlDbType.Bit) { Value = newStatus },
                new SqlParameter("@CompanyID", SqlDbType.Int) { Value = CompanyID },
                new SqlParameter("@UserID", SqlDbType.Int) { Value = UserID }

                    };

                    result = cls.ExecuteNonQueryStatement(query, cls.CreateDataBaseConnectionString(CompanyID), parameters);


               
                return result.ToString();

            }
            catch (Exception ex)
            {
                return "";
            }
        }


        [HttpPost]
        [Route("saveWidgetOrder")]
        public string saveWidgetOrder([FromBody] List<Dictionary<string, object>> widgetDataString,int UserID, int CompanyID)
        {
            try
            {
                List<WidgetOrderModel> widgetData = new List<WidgetOrderModel> ();
 

                if (widgetDataString == null || widgetDataString.Count == 0)
                {
                    return "";
                }


                foreach (var widget in widgetDataString)
                {
                 
                    WidgetOrderModel a = new WidgetOrderModel();
                    if (widget.TryGetValue("widgetName", out var sectionName))
                    {
                        a.SectionName = sectionName?.ToString();
                    }

                    if (widget.TryGetValue("widgetIndex", out var sectionIndex))
                    {
                         
                            a.SectionIndex = sectionIndex?.ToString();


                    }
                    if (widget.TryGetValue("id", out var id))
                    {

                        a.ID = id?.ToString();


                    }
                    widgetData.Add(a);
                   
                }

                clsSQL cls = new clsSQL();
                string query = @"
            UPDATE [dbo].[tbl_DashboardWidgets]
            SET 
                SectionName = @SectionName,
                SectionIndex = @SectionIndex,
                ModificationUserID=@UserID,
                ModificationDate = GETDATE()
            WHERE 
                ID = @ID AND 
                UserID=@UserID AND 
                CompanyID = @CompanyID";
                int result = 0;
                foreach (var widget in widgetData)
                {
                    SqlParameter[] parameters = new SqlParameter[]
                    {
                new SqlParameter("@ID", SqlDbType.Int) { Value = widget.ID },
                new SqlParameter("@SectionName", SqlDbType.NVarChar, 50) { Value = widget.SectionName },
                new SqlParameter("@SectionIndex", SqlDbType.Int) { Value = widget.SectionIndex },
                new SqlParameter("@CompanyID", SqlDbType.Int) { Value = CompanyID },
                new SqlParameter("@UserID", SqlDbType.Int) { Value = UserID }
                
                    };

                      result = cls.ExecuteNonQueryStatement(query, cls.CreateDataBaseConnectionString(CompanyID), parameters);
                
                    
                }
                return result.ToString();
                
            }
            catch (Exception ex)
            {
                return "";
            }
        }

        public class WidgetOrderModel
        {
            public string ID { get; set; }
            public string SectionName { get; set; }
            public string SectionIndex { get; set; }
        }

        [HttpGet]
        [Route("GetDashboardWidgets")]
        public string GetDashboardWidgets(int userId, int companyId)
        {
            string tytt = "";
            try
            {
                clsSQL clssql = new clsSQL();

                SqlParameter[] prm =
                {
                new SqlParameter("@UserId", SqlDbType.Int) { Value = userId },
                new SqlParameter("@CompanyId", SqlDbType.Int) { Value = companyId }
            };

                string query = @"
                SELECT ID,Title, WidgetType,GroupName, SQLQuery, ChartConfig, Icon, Color ,SectionName,SectionIndex,isactive
                FROM tbl_DashboardWidgets 
                WHERE isactive = 1 and UserId = @UserId AND (CompanyID = @CompanyId OR @CompanyId = 0)";

                DataTable dt = clssql.ExecuteQueryStatement(query, clssql.CreateDataBaseConnectionString(companyId), prm);

                if (dt != null && dt.Rows.Count > 0)
                {
                    var widgetResults = new List<object>();

                    foreach (DataRow row in dt.Rows)
                    {
                        try
                        {
                            string widgetTitle = row["Title"].ToString();
                            string widgetType = row["WidgetType"].ToString();
                            tytt = row["Title"].ToString();
                            string groupName = row["GroupName"].ToString();
                            string sqlQuery = row["SQLQuery"].ToString();
                            string isactive = row["isactive"].ToString();
                            string chartConfig = row["ChartConfig"] == DBNull.Value ? "" : row["ChartConfig"].ToString();
                            string icon = row["Icon"].ToString();
                            string color = row["Color"].ToString();
                            string sectionName = row["SectionName"].ToString();
                            string sectionIndex = row["SectionIndex"].ToString();
                            string iD = row["ID"].ToString();
                            string conn = clssql.CreateDataBaseConnectionString(companyId);
                            sqlQuery = clsDashboardWidgets.ResolveWidgetSql(
                                clssql,
                                conn,
                                widgetTitle,
                                sqlQuery);

                            DataTable widgetData;
                            try
                            {
                                // Pass null for widget SQL params (same as before refactor).
                                // Do not add both @CompanyId and @CompanyID — SQL Server treats them as one name.
                                widgetData = clssql.ExecuteQueryStatement(
                                    sqlQuery,
                                    conn,
                                    null);

                                if (string.Equals(widgetType, "KPI", StringComparison.OrdinalIgnoreCase))
                                {
                                    clsDashboardWidgets.EnsureKpiTrendColumn(widgetData);
                                }
                            }
                            catch (Exception sqlEx)
                            {
                                System.Diagnostics.Debug.WriteLine(
                                    $"Dashboard widget SQL failed [{widgetTitle}]: {sqlEx.Message}");
                                widgetData = new DataTable();
                            }

                            var columnTypes = new List<object>();
                            if (widgetData != null)
                            {
                                foreach (DataColumn col in widgetData.Columns)
                                {
                                    columnTypes.Add(new
                                    {
                                        ColumnType = col.DataType.Name
                                    });
                                }
                            }

                            object configObj = null;
                            if (!string.IsNullOrWhiteSpace(chartConfig))
                            {
                                try { configObj = JsonConvert.DeserializeObject(chartConfig); }
                                catch { configObj = null; }
                            }

                            double? trendValue = null;
                            if (string.Equals(widgetType, "KPI", StringComparison.OrdinalIgnoreCase))
                            {
                                clsDashboardWidgets.EnsureKpiTrendColumn(widgetData);
                                trendValue = clsDashboardWidgets.ExtractTrendValue(widgetData);
                            }

                            var dataRows = clsDashboardWidgets.ToRowDictionaries(widgetData);

                            widgetResults.Add(new
                            {
                                Title = widgetTitle,
                                Type = widgetType,
                                Data = dataRows,
                                PercentageChange = trendValue,
                                IsActive = isactive,
                                GroupName = groupName,
                                ColumnTypes = columnTypes,
                                Config = configObj,
                                Icon = icon,
                                Color = color,
                                SectionIndex = sectionIndex,
                                SectionName = sectionName,
                                ID = iD,
                            });
                        }
                        catch (Exception widgetEx)
                        {
                            // Skip broken widget; do not fail the whole dashboard payload.
                            System.Diagnostics.Debug.WriteLine($"Dashboard widget skipped: {widgetEx.Message}");
                        }
                    }

                    string JSONString = JsonConvert.SerializeObject(widgetResults);
                    return JSONString;
                }
                else
                {
                    return JsonConvert.SerializeObject(new List<object>());
                }
            }
            catch (Exception ex)
            {
                // Always return a JSON array so Flutter can iterate safely on login.
                System.Diagnostics.Debug.WriteLine($"GetDashboardWidgets failed: {ex.Message}");
                return JsonConvert.SerializeObject(new List<object>());
            }
        }




        [HttpGet]
        [Route("GetUserMenuPreferences")]
        public string GetUserMenuPreferences(int userId, int companyId, string moduleNamespace)
        {
            try
            {
                clsUserMenuPreferences service = new clsUserMenuPreferences();
                var dto = service.SelectUserMenuPreferences(
                    userId,
                    companyId,
                    moduleNamespace ?? string.Empty);

                return JsonConvert.SerializeObject(new
                {
                    PinnedKeys = dto.PinnedKeys,
                    ExpandedGroups = dto.ExpandedGroups,
                });
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"GetUserMenuPreferences failed: {ex.Message}");
                return JsonConvert.SerializeObject(new
                {
                    PinnedKeys = new List<string>(),
                    ExpandedGroups = new List<string>(),
                });
            }
        }

        public sealed class SaveUserMenuPreferencesRequest
        {
            public List<string> PinnedKeys { get; set; } = new List<string>();
            public List<string> ExpandedGroups { get; set; } = new List<string>();
        }

        [HttpPost]
        [Route("SaveUserMenuPreferences")]
        public string SaveUserMenuPreferences(
            [FromBody] SaveUserMenuPreferencesRequest request,
            int userId,
            int companyId,
            string moduleNamespace,
            int modificationUserId)
        {
            try
            {
                clsUserMenuPreferences service = new clsUserMenuPreferences();
                bool ok = service.SaveUserMenuPreferences(
                    userId,
                    companyId,
                    moduleNamespace ?? string.Empty,
                    request?.PinnedKeys ?? new List<string>(),
                    request?.ExpandedGroups ?? new List<string>(),
                    modificationUserId);

                return ok ? "Success" : "Failed";
            }
            catch (Exception ex)
            {
                return "Error: " + ex.Message;
            }
        }

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

                // Sanitize the incoming comma-separated type list: accept integers only and
                // rebuild the IN(...) clause from validated values. This neutralizes SQL
                // injection that was previously possible by concatenating the raw string.
                List<string> safeTypeIds = new List<string>();
                if (!string.IsNullOrWhiteSpace(InvoiceTypeID))
                {
                    foreach (string part in InvoiceTypeID.Split(','))
                    {
                        if (int.TryParse(part.Trim(), out int parsedTypeId))
                            safeTypeIds.Add(parsedTypeId.ToString());
                    }
                }
                if (safeTypeIds.Count == 0)
                    return "";
                string invoiceTypeInClause = string.Join(",", safeTypeIds);

                SqlParameter[] prm =
                 {



                        new SqlParameter("@date1", SqlDbType.DateTime) { Value = date1 },
                    new SqlParameter("@date2", SqlDbType.DateTime) { Value = date2 },



                        new SqlParameter("@CompanyID", SqlDbType.Int) { Value =CompanyID },



                };
                string a = @"  
select q.Date,(select isnull( sum(TotalInvoice),0)from tbl_InvoiceHeader
 where (companyid=@companyID or @companyID=0)and (InvoiceTypeID in (" + invoiceTypeInClause + @"))
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
            [FromBody] string DetailsList, string BudgetOverrideReason = "")

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

                bool forceBudgetApproval = false;
                BudgetCheckResult budgetCheck = null;
                if (voucherType == (int)clsEnum.VoucherType.CashPayment)
                {
                    var spend = clsBudgetControl.FromCashDetails(details, branchID, costCenterID);
                    string blocked = new clsBudgetControl().ApplyGate(
                        companyID, voucherType, voucherDate, branchID, costCenterID, spend,
                        BudgetOverrideReason, out forceBudgetApproval, out budgetCheck);
                    if (blocked != null) return blocked;
                }

                clsCashVoucherHeader clsCashVoucherHeader = new clsCashVoucherHeader();
                clsCashVoucherDetails clsCashVoucherDetails = new clsCashVoucherDetails();
                SqlTransaction trn; clsSQL clsSQL = new clsSQL();
                SqlConnection con = new SqlConnection(clsSQL.CreateDataBaseConnectionString(companyID));
                con.Open();
                trn = con.BeginTransaction(); string A = "";
                try
                {
                    bool IsSaved = true;

                    DataTable dt = clsSQL.ExecuteQueryStatement("select isnull( max(voucherno),0)+1 as Max from tbl_cashvoucherheader where   VoucherType ="+ Simulate.String(voucherType) + "and RelatedFinancingGuid ='00000000-0000-0000-0000-000000000000' and companyid=" + companyID.ToString(), clsSQL.CreateDataBaseConnectionString(companyID), trn);
                    if (dt != null && dt.Rows.Count > 0) {

                        dbCashVoucherHeader.VoucherNo = Simulate.Integer32(dt.Rows[0][0]);
                    }
                    else {

                        dbCashVoucherHeader.VoucherNo = 1;
                    }

                    clsApprovalEngine approvalEngine = new clsApprovalEngine();
                    int documentStatus = approvalEngine.ResolveInitialDocumentStatus(
                        companyID, voucherType, branchID, amount);
                    if (forceBudgetApproval)
                        documentStatus = (int)clsEnum.DocumentStatus.Draft;

                    A = clsCashVoucherHeader.InsertCashVoucherHeader(dbCashVoucherHeader, trn, documentStatus);
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


                    if (IsSaved && documentStatus == (int)clsEnum.DocumentStatus.Posted)
                        IsSaved = clsCashVoucherHeader.InsertCashVoucherJournalVoucher(A, AccountID, branchID, costCenterID, cashID, amount, Simulate.String(note), voucherDate, DueDate,details, "", voucherType, companyID, creationUserID, trn);
                    if (IsSaved)
                    { trn.Commit(); }
                    else
                    { trn.Rollback(); return ""; }

                    if (forceBudgetApproval && !string.IsNullOrEmpty(A))
                    {
                        string ovErr = new clsBudget().CompleteBudgetOverride(
                            "tbl_CashVoucherHeader", companyID, creationUserID, voucherType, A,
                            Simulate.String(dbCashVoucherHeader.VoucherNo), BudgetOverrideReason,
                            budgetCheck?.Breaches);
                        if (ovErr != null) return ovErr;
                    }

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
        [Route("UpdateCashVoucherHeader")]
        public string UpdateCashVoucherHeader(DateTime voucherDate, int branchID, int costCenterID,int AccountID, int cashID
            , decimal amount, string jVGuid, string note
            , string manualNo, int voucherType, string relatedInvoiceGuid, int companyID,
             int modificationUserID, string guid,int PaymentMethodTypeID,string ChequeNote,DateTime DueDate,
             string ChequeName,
            [FromBody] string detailsList, string BudgetOverrideReason = "")
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

                bool forceBudgetApproval = false;
                BudgetCheckResult budgetCheck = null;
                if (voucherType == (int)clsEnum.VoucherType.CashPayment)
                {
                    var spend = clsBudgetControl.FromCashDetails(details, branchID, costCenterID);
                    string blocked = new clsBudgetControl().ApplyGate(
                        companyID, voucherType, voucherDate, branchID, costCenterID, spend,
                        BudgetOverrideReason, out forceBudgetApproval, out budgetCheck, guid);
                    if (blocked != null) return blocked;
                }

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

                    DataTable dtExisting = clsCashVoucherHeader.SelectCashVoucherHeaderByGuid(
                        guid,
                        Simulate.StringToDate("1900-01-01"),
                        Simulate.StringToDate("2300-01-01"),
                        0, 0, companyID,
                        "00000000-0000-0000-0000-000000000000",
                        trn);
                    int documentStatus = (int)clsEnum.DocumentStatus.Posted;
                    if (dtExisting != null && dtExisting.Rows.Count > 0)
                    {
                        var row = dtExisting.Rows[0];
                        documentStatus = Simulate.Integer32(row["DocumentStatus"]);
                        int branchId = Simulate.Integer32(row["BranchID"]);
                        int voucherTypeId = Simulate.Integer32(row["VoucherType"]);
                        decimal voucherAmount = Simulate.Decimal(row["Amount"]);

                        var approvalEngine = new clsApprovalEngine();
                        if (approvalEngine.DocumentStatusBlocksEdit(
                                companyID, voucherTypeId, branchId, voucherAmount, documentStatus))
                        {
                            trn.Rollback();
                            return "";
                        }
                    }

                    if (forceBudgetApproval)
                        documentStatus = (int)clsEnum.DocumentStatus.Draft;

                    A = clsCashVoucherHeader.UpdateCashVoucherHeader(dbCashVoucherHeader, companyID, trn);
                    clsCashVoucherDetails.DeleteCashVoucherDetailsByHeaderGuid(guid, companyID, trn);
                    for (int i = 0; i < details.Count; i++)
                    {

                        string c = clsCashVoucherDetails.InsertCashVoucherDetails(details[i], guid, trn);
                        if (c == "")
                            IsSaved = false;
                    }
                    if (IsSaved && documentStatus == (int)clsEnum.DocumentStatus.Posted)
                        IsSaved = clsCashVoucherHeader.InsertCashVoucherJournalVoucher(guid, AccountID, branchID, costCenterID, cashID, amount, Simulate.String(note), voucherDate, DueDate, details, Simulate.String(jVGuid), voucherType, companyID, modificationUserID, trn);
                    if (IsSaved)
                    { trn.Commit(); }
                    else
                    { trn.Rollback(); return ""; }

                    if (forceBudgetApproval)
                    {
                        clsCashVoucherHeader.UpdateDocumentStatus(
                            guid, (int)clsEnum.DocumentStatus.Draft, modificationUserID, companyID);
                        string ovErr = new clsBudget().CompleteBudgetOverride(
                            "tbl_CashVoucherHeader", companyID, modificationUserID, voucherType, guid,
                            "", BudgetOverrideReason, budgetCheck?.Breaches);
                        if (ovErr != null) return ovErr;
                    }

                    return string.IsNullOrEmpty(A) ? guid : A;
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
        public IActionResult PrintCashVoucherByHeaderGuid(
            string HeaderGuid, int UserId, int CompanyID, int TransactionReportID = 0)
        {
            try
            {
                string pageName = TransactionReportID > 0
                    ? clsTransactionReportPrint.PageCashVoucherAdd
                    : new clsTransactionReportPrint()
                        .ResolveCashVoucherLayoutPage(HeaderGuid, CompanyID);

                return PrintTransactionReportPdf(
                    pageName,
                    HeaderGuid,
                    UserId,
                    CompanyID,
                    TransactionReportID);
            }
            catch (Exception ex)
            {
                return BadRequest("Print error: " + ex.Message);
            }
        }

        /// <summary>FastReport PDF: bilingual employment contract for one contract row.</summary>
        [HttpGet]
        [Route("PrintEmployeeContract")]
        public IActionResult PrintEmployeeContract(int ContractID, int UserId, int CompanyID)
        {
            try
            {
                if (ContractID <= 0)
                    return BadRequest("ContractID is required");

                FastReport.Utils.Config.WebMode = true;
                clsEmployeeContract clsEc = new clsEmployeeContract();
                DataTable dt = clsEc.SelectEmployeeContractByID(ContractID, 0, false, CompanyID);
                if (dt == null || dt.Rows.Count == 0)
                    return NotFound();

                EnrichEmployeeContractDataForPrint(dt, CompanyID);
                dt.TableName = "EmployeeContract";
                System.Data.DataSet dsPrint = new System.Data.DataSet();
                dsPrint.Tables.Add(dt);

                FastReport.Report report = new FastReport.Report();
                report.RegisterData(dsPrint);

                clsReports repHelper = new clsReports();
                repHelper.LoadCompanyFastReport(
                    report,
                    clsTransactionReportDefaults.PageEmployeeContractAdd,
                    "rptEmployeeContract",
                    CompanyID,
                    UserId);

                DataRow r0 = dt.Rows[0];
                report.SetParameterValue("report.ContractNumber", Simulate.String(r0["ContractNumber"]));
                report.SetParameterValue("report.ContractID", Simulate.Integer32(r0["ID"]).ToString());

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



                clsReports clsReports = new clsReports();

                clsReports.LoadCompanyFastReport(
                    report,
                    clsTransactionReportDefaults.PageCashVoucherCheque,
                    "rptCheques",
                    CompanyID,
                    UserId);
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
        public bool DeleteBanksByID(int ID,int AccountID, int CompanyID)
        {
            try
            {
                clsJournalVoucherDetails clsJournalVoucherDetails = new clsJournalVoucherDetails();
                DataTable dt = clsJournalVoucherDetails.SelectJournalVoucherDetailsByParentId("", AccountID, 0,ID, 0, 0, CompanyID);
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
                DataTable dtCompany = clsCompany.SelectCompany(CompanyID, "", "", "", CompanyID, "", false);
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


                clsReports clsReports = new clsReports();

                clsReports.LoadCompanyFastReport(
                    report,
                    clsTransactionReportDefaults.PageEmployeeLoans,
                    "rptCutomerLoansReport",
                    CompanyID,
                    Simulate.Integer32(userID));
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
             DataTable   dtCompany = clsCompany.SelectCompany(CompanyID, "", "", "", CompanyID, "", false);
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
                clsReports clsReports = new clsReports();
                

                clsReports.LoadCompanyFastReport(
                    report,
                    clsTransactionReportDefaults.PageFinancingReport,
                    "rptFinancingReport",
                    CompanyID,
                    Simulate.Integer32(users));
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
               
                DataTable dtCompany = clsCompany.SelectCompany(CompanyID, "", "", "", CompanyID, "", false);
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
                        ds.DataTableD.Rows[i]["purchaseinvoicerefnumber"] = Simulate.String(dt.Rows[i]["purchaseinvoicerefnumber"]);
            
                        ds.DataTableD.Rows[i]["VendorAName"] = dt.Rows[i]["VendorAName"];
                        ds.DataTableD.Rows[i]["SalesPrice"] = dt.Rows[i]["TotalAmountWithInterest"];
                        ds.DataTableD.Rows[i]["VoucherDate"] = dt.Rows[i]["VoucherDate"];
                        ds.DataTableD.Rows[i]["VoucherNumber"] = dt.Rows[i]["VoucherNumber"];
                        ds.DataTableD.Rows[i]["TotalAfterTax"] = dt.Rows[i]["TotalAmountWithInterest"];
                        ds.DataTableD.Rows[i]["PriceBeforeTax"] = dt.Rows[i]["PriceBeforeTax"];
                        ds.DataTableD.Rows[i]["TaxAmount"] = dt.Rows[i]["TaxAmount"];
                  
                    }
                }
               







                FastReport.Web.WebReport report = new FastReport.Web.WebReport();
                report.Report.RegisterData(ds);



                clsReports clsReports = new clsReports();

                clsReports.LoadCompanyFastReport(
                    report.Report,
                    clsTransactionReportDefaults.PageFinancingReport,
                    "rptFinancingReport",
                    CompanyID,
                    Simulate.Integer32(users));
                

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
        public bool DeleteFinancingHeaderByGuid(string Guid,string InvoiceHeaderGuid, int CompanyID)
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
                trn = con.BeginTransaction();
                int A = 0;
                bool IsSaved = true;
                try
                {
                   
                    DataTable dt = clsFinancingHeader.SelectFinancingHeaderByGuid(Guid, Simulate.StringToDate("1900-01-01"), Simulate.StringToDate("2300-01-01"), 0, 0,  0, 0, "-1", 0,trn);
                    IsSaved = clsFinancingHeader.DeleteFinancingHeaderByGuid(Guid,CompanyID, trn);
                    bool a = clsFinancingDetails.DeleteFinancingDetailsByHeaderGuid(Guid,CompanyID, trn);
                    if (dt != null && dt.Rows.Count == 1)
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
                    {
                        trn.Commit();
                   
                        if (InvoiceHeaderGuid != "")
                        {
                            
                            DeleteInvoiceDetailsByHeaderGuid(InvoiceHeaderGuid, CompanyID);
                        }

                    }
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
            bool IsShowInMonthlyReports,string PurchaseInvoiceRefNumber,
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
                    PurchaseInvoiceRefNumber=Simulate.String(PurchaseInvoiceRefNumber)
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
                            List<DBFinancingDetails> details;
                            try
                            {
                                details = JsonConvert.DeserializeObject<List<DBFinancingDetails>>(DetailsList);
                            }
                            catch (Exception ex)
                            {
                                trn.Rollback();
                                return "ERR:Invalid financing details: " + ex.Message;
                            }
                            if (details == null || details.Count == 0)
                            {
                                IsSaved = false;
                            }
                            else
                            {
                            for (int i = 0; i < details.Count; i++)
                            {
                                string c = clsFinancingDetails.InsertFinancingDetails(dbFinancingHeader, details[i], A,companyID, trn);
                                if (c == "")
                                    IsSaved = false;
                            }



                           
                            if (IsSaved)

                            {
                                clsBusinessPartner clsBusinessPartner = new clsBusinessPartner();
                                DataTable dtBusinessPartner = clsBusinessPartner.SelectBusinessPartner(businessPartnerID,0,"","", "", "", -1,companyID,trn);
                                if (dtBusinessPartner != null && dtBusinessPartner.Rows.Count > 0) {
                                    IsSaved =   clsFinancingHeader.InsertPurchaseInvoiceHeader(
                                branchID, CostCenterID, 0, creationUserID,
                                voucherDate, VendorID, PurchaseInvoiceRefNumber,
                                Simulate.String( dtBusinessPartner.Rows[0]["AName"]) +" - "+ Simulate.String(dtBusinessPartner.Rows[0]["EmpCode"]) + note,
                                1, A, companyID, details, trn);

                                } else {

                                    IsSaved = false;
                                }

                        
                            }
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
                                          , details[i].Total, details[i].CurrencyID, details[i].CurrencyRate, details[i].CurrencyBaseAmount, branchID, details[i].CostCenterID, details[i].DueDate, details[i].Note, companyID
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
                                           ,  netAmount,1,1, netAmount, branchID,0, voucherDate,Simulate.String( note), companyID
                                           , creationUserID,"", trn);
                                if (c1 == "")
                                    IsSaved = false;


                                string insertCredit = clsJournalVoucherDetails.InsertJournalVoucherDetails(JVGuid, details.Count + 1,PaymentAccountID, PaymentSubAccountID, 0, totalAmount
                                        , -1 * totalAmount,1,1, -1 * totalAmount, branchID, 0, voucherDate, Simulate.String(note), companyID
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
                            ,-1*( netAmount - totalAmount),1,1,  -1 * (netAmount - totalAmount), branchID, 0, voucherDate, Simulate.String(note), companyID
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
                    { trn.Rollback(); return "ERR:Transaction could not be saved. Check vendor, payment method, account settings, and financing line details."; }

                }
                catch (Exception ex)
                {

                    trn.Rollback();
                    return "ERR:" + ex.Message;
                }
                finally { con.Close(); }

            }
            catch (Exception ex)
            {

                return "ERR:" + ex.Message;
            }

        }
        [Route("UpdateFinancingHeader")]
        public  string UpdateFinancingHeader(
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

        string InvoiceHeaderGuid, string PurchaseInvoiceRefNumber,
       
            [FromBody] string DetailsList
            
           )
        {




            clsInvoiceHeader clsInvoiceHeader = new clsInvoiceHeader();
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
                    InvoiceHeaderGuid=Simulate.Guid( InvoiceHeaderGuid),  
                    PurchaseInvoiceRefNumber=Simulate.String( PurchaseInvoiceRefNumber)
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
              
                    clsFinancingDetails.DeleteFinancingDetailsByHeaderGuid(guid,companyID, trn);
                    string invoiceGuide = "";
                     DataTable dtFinanceheader = clsFinancingHeader.SelectFinancingHeaderByGuid(guid, DateTime.Now.AddYears(-100), DateTime.Now.AddYears(100), 0, 0, companyID, 0, "-1", 0,trn);
                    if (dtFinanceheader != null && dtFinanceheader.Rows.Count > 0) {

                          invoiceGuide = Simulate.String(dtFinanceheader.Rows[0]["InvoiceHeaderGuid"]);
                        clsInvoiceHeader.DeleteInvoiceDetailsByHeaderGuid(
Simulate.String(invoiceGuide)
, companyID, trn);
                    }




                    A = clsFinancingHeader.UpdateFinancingHeader(dbFinancingHeader, companyID, trn);


                    clsLoanTypes clsLoanTypes = new clsLoanTypes();
                    DataTable DTLoanTypes = clsLoanTypes.SelectLoanTypes(loanType, "0,1,2,3", "", "", "", companyID);
                    if (DTLoanTypes != null && DTLoanTypes.Rows.Count > 0 && Simulate.Integer32(DTLoanTypes.Rows[0]["MainTypeID"]) == 1)
                    {
                        DetailsList = DetailsList.Replace("\\", "\\\\");
                        List<DBFinancingDetails> details = JsonConvert.DeserializeObject<List<DBFinancingDetails>>(DetailsList);
                        for (int i = 0; i < details.Count; i++)
                        {

                            string c = clsFinancingDetails.InsertFinancingDetails(dbFinancingHeader, details[i], guid, companyID,trn);
                            if (c == "")
                                IsSaved = false;
                        }

                    

                        if (IsSaved)
                        {

                            clsBusinessPartner clsBusinessPartner = new clsBusinessPartner();
                            DataTable dtBusinessPartner = clsBusinessPartner.SelectBusinessPartner(businessPartnerID, 0, "", "", "", "", -1, companyID, trn);
                            if (dtBusinessPartner != null && dtBusinessPartner.Rows.Count > 0) {

                                if (!string.IsNullOrWhiteSpace(InvoiceHeaderGuid) &&
                                    !string.Equals(InvoiceHeaderGuid, invoiceGuide, StringComparison.OrdinalIgnoreCase))
                                {
                                    IsSaved = clsInvoiceHeader.DeleteInvoiceDetailsByHeaderGuid(
                                        InvoiceHeaderGuid, companyID, trn);
                                }

                                IsSaved =   clsFinancingHeader.InsertPurchaseInvoiceHeader(
                                   branchID, CostCenterID, 0, modificationUserID,
                                   voucherDate, VendorID, PurchaseInvoiceRefNumber,
                                   Simulate.String(dtBusinessPartner.Rows[0]["AName"]) + " - " 
                                   + Simulate.String(dtBusinessPartner.Rows[0]["EmpCode"]) + note,

                                   1, guid, companyID, details, trn);

                            } else {

                                IsSaved = false;

                            }
                           
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
                                  , details[i].Total, details[i].CurrencyID, details[i].CurrencyRate, details[i].CurrencyBaseAmount, branchID, details[i].CostCenterID, details[i].DueDate, details[i].Note, companyID
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
                                       , netAmount,1, 1, netAmount, branchID, 0, voucherDate, Simulate.String(note), companyID
                                       , modificationUserID, "",trn);
                            if (Simulate.Integer32(DTLoanTypes.Rows[0]["ReceivableAccountID"]) == 0)
                            {
                                IsSaved = false;
                            }
                            if (c1 == "")
                                IsSaved = false;
                            string insertCredit = clsJournalVoucherDetails.InsertJournalVoucherDetails(dbFinancingHeader.JVGuid.ToString(), details.Count + 1, PaymentAccountID, PaymentSubAccountID, 0, totalAmount
                                     , -1 * totalAmount,1,1, -1 * totalAmount, branchID, 0, voucherDate, Simulate.String(note), companyID
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
                        ,-1*( netAmount - totalAmount),1, 1, -1 * (netAmount - totalAmount), branchID, 0, voucherDate, Simulate.String(note), companyID
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
                    { trn.Commit();

                  
                 


                        return A; 
                    
                    
                    }
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

                    clsReports clsReports = new clsReports();

                    clsReports.LoadCompanyFastReport(
                        report,
                        clsTransactionReportDefaults.PageFinancingDocument,
                        "rptFinancing",
                        CompanyID,
                        UserId);
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
                    dtSign = clsEmployee.SelectEmployee(Simulate.Integer32(dtHeader.Rows[0]["SalesmanID"]), "", "", "", "", "", "", CompanyID, -1);

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
                     
                    dtSign = clsEmployee.SelectEmployee(1111, "", "", "", "", "", "", CompanyID, -1);
                  
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



                    clsReports clsReports = new clsReports();

                    clsReports.LoadCompanyFastReport(
                        report,
                        clsTransactionReportDefaults.PageCashLoan,
                        "rptCashLoan",
                        CompanyID,
                        UserId);
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



                    clsReports clsReports = new clsReports();

                    clsReports.LoadCompanyFastReport(
                        report,
                        clsTransactionReportDefaults.PageGift,
                        "rptGift",
                        CompanyID,
                        UserId);
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
   
        [HttpGet]
        [Route("PrintFinancingGuarantee")]
        public IActionResult PrintFinancingGuarantee(string guid, int UserId, int CompanyID)
        {
            try
            {
                clsReports clsReports = new clsReports();
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
                    
                        cls_AccountSetting cls_AccountSetting = new cls_AccountSetting();
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


           

                clsReports.LoadCompanyFastReport(
                    report,
                    clsTransactionReportDefaults.PageFinancingGuarantee,
                    "rptFinancingGuarantee",
                    CompanyID,
                    UserId);
                report.RegisterData(ds);
                report.RegisterData(dsBusinessPartner);
                report.SetParameterValue("report.TotalDueToWord", Simulate.String(clsConvertNumberToString.NoToTxt(Simulate.Val(Math.Abs( TotalDue)))));

                report.SetParameterValue("report.AmountWithOutDecimal", Simulate.String(AmountWithOutDecimal));
                report.SetParameterValue("report.AmountDecimal", Simulate.String(AmountDecimal));
                report.SetParameterValue("report.AmountToWord", Simulate.String(AmountToWord));


                report.SetParameterValue("report.DueDate", (Simulate.StringToDate(dtHeader.Rows[0]["VoucherDate"]).AddMonths(4)).ToString("yyyy-MM-dd"));


                report.SetParameterValue("report.TotalDue", Simulate.Currency_format(TotalDue));





                clsSignuture cls = new clsSignuture();

              DataTable  dtSign = cls.SelectSignuture(Simulate.String(dtHeader.Rows[0]["SignutureGuid4"]),0, 0, CompanyID);
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
        [HttpGet]
        [Route("PrintFinancingSalesInvoice")]
        public IActionResult PrintFinancingSalesInvoice(string guid, int UserId, int CompanyID)
        {
            try
            {
                clsReports clsReports = new clsReports();
                FastReport.Utils.Config.WebMode = true;
                clsFinancingHeader clsFinancingHeader = new clsFinancingHeader();
                clsFinancingDetails clsFinancingDetails = new clsFinancingDetails();
                decimal AmountWithProfit = 0;
                DataTable dtHeader = clsFinancingHeader.SelectFinancingHeaderByGuid(guid, DateTime.Now.AddYears(-100), DateTime.Now.AddYears(100), 0, 0, CompanyID, 0, "-1", 0);
                DataTable dtDetails = clsFinancingDetails.SelectFinancingDetailsByHeaderGuid(guid, 0, CompanyID);
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


                        ds.Details.Rows[i]["PriceBeforeTax"] = Simulate.decimal_(dtDetails.Rows[i]["PriceBeforeTax"]);
                        ds.Details.Rows[i]["TaxAmount"] = Simulate.decimal_(dtDetails.Rows[i]["TaxAmount"]);

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

                        cls_AccountSetting cls_AccountSetting = new cls_AccountSetting();
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




                clsReports.LoadCompanyFastReport(
                    report,
                    clsTransactionReportDefaults.PageFinancingSalesInvoice,
                    "rptFinancingSalesInvoice",
                    CompanyID,
                    UserId);
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
               DataTable dtBusinessPartner= clsBusinessPartner.SelectBusinessPartner(BusnessPartnerID, 0, "", "", "", "", -1, CompanyID);
                DataTable dtGrantoID = clsBusinessPartner.SelectBusinessPartner(GrantoID, 0, "", "", "", "", -1, CompanyID);
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

                            if (dt.Rows.Count > 0 && dt.Columns.Contains("EName"))
                            {
                                dt.Columns.Remove("EName");
                            }
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
                    ColumnType.Add("string");



                }


                if (Type == "Sales" || Type == "Loans") {

                    return FastreporttoCSV(dtlist, dtName, ColumnType);

                } else {
                    return Fastreporttoxlsx(dtlist, dtName, ColumnType);

                }

                   
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
                if(details.Count>0)
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
        [Route("SelectJvReconciliationReport")]
        public string SelectJvReconciliationReport(string ParentGuid, int CompanyID)
        {
            try
            {
                clsReconciliation clsReconciliation = new clsReconciliation();
                DataTable dt = clsReconciliation.SelectJvReconciliationReport(ParentGuid, CompanyID);
                if (dt != null)
                {
                    return JsonConvert.SerializeObject(dt);
                }

                return "";
            }
            catch (Exception)
            {
                throw;
            }
        }

        [HttpGet]
        [Route("SelectCustomerPaymentInstallmentTree")]
        public string SelectCustomerPaymentInstallmentTree(
            int BusinessPartnerID,
            DateTime Date1,
            DateTime Date2,
            int CompanyID)
        {
            try
            {
                clsReconciliation clsReconciliation = new clsReconciliation();
                DataTable dt = clsReconciliation.SelectCustomerPaymentInstallmentTree(
                    BusinessPartnerID,
                    Date1,
                    Date2,
                    CompanyID);
                if (dt != null)
                {
                    return JsonConvert.SerializeObject(dt);
                }
                return "";
            }
            catch (Exception)
            {
                throw;
            }
        }

        [HttpGet]
        [Route("SelectCustomerPaymentInstallmentTreePDF")]
        public IActionResult SelectCustomerPaymentInstallmentTreePDF(
            int BusinessPartnerID,
            DateTime Date1,
            DateTime Date2,
            int CompanyID,
            int UserID)
        {
            try
            {
                FastReport.Utils.Config.WebMode = true;
                clsReconciliation clsReconciliation = new clsReconciliation();
                DataTable src = clsReconciliation.SelectCustomerPaymentInstallmentTree(
                    BusinessPartnerID,
                    Date1,
                    Date2,
                    CompanyID);

                DataTable print = new DataTable("DataTable1");
                print.Columns.Add("Index", typeof(string));
                print.Columns.Add("CustomerName", typeof(string));
                print.Columns.Add("EmpCode", typeof(string));
                print.Columns.Add("PaymentVoucherDate", typeof(string));
                print.Columns.Add("PaymentJVTypeName", typeof(string));
                print.Columns.Add("PaymentJVNumber", typeof(string));
                print.Columns.Add("PaymentAmount", typeof(decimal));
                print.Columns.Add("InstallmentDueDate", typeof(string));
                print.Columns.Add("InstallmentNote", typeof(string));
                print.Columns.Add("InstallmentLineTotal", typeof(decimal));
                print.Columns.Add("ReconciledAmount", typeof(decimal));
                print.Columns.Add("ReconciliationVoucherNumber", typeof(string));
                print.Columns.Add("FinancingVoucherNumber", typeof(string));
                print.Columns.Add("LoanTypeName", typeof(string));
                print.Columns.Add("Status", typeof(string));

                string customerName = "";
                string empCode = "";
                if (src != null)
                {
                    for (int i = 0; i < src.Rows.Count; i++)
                    {
                        if (string.IsNullOrWhiteSpace(customerName))
                        {
                            customerName = Simulate.String(src.Rows[i]["CustomerName"]);
                            empCode = Simulate.String(src.Rows[i]["EmpCode"]);
                        }

                        int linkKind = Simulate.Integer32(src.Rows[i]["LinkKind"]);
                        string status = linkKind == 1
                            ? "Unallocated payment"
                            : linkKind == 2
                                ? "Unallocated installment"
                                : "Linked";

                        object payDateObj = src.Rows[i]["PaymentVoucherDate"];
                        object dueDateObj = src.Rows[i]["InstallmentDueDate"];
                        string payDate = (payDateObj == null || payDateObj == DBNull.Value)
                            ? ""
                            : Simulate.StringToDate(payDateObj).ToString("yyyy-MM-dd");
                        string dueDate = (dueDateObj == null || dueDateObj == DBNull.Value)
                            ? ""
                            : Simulate.StringToDate(dueDateObj).ToString("yyyy-MM-dd");

                        print.Rows.Add(
                            (i + 1).ToString(),
                            Simulate.String(src.Rows[i]["CustomerName"]),
                            Simulate.String(src.Rows[i]["EmpCode"]),
                            payDate,
                            Simulate.String(src.Rows[i]["PaymentJVTypeName"]),
                            Simulate.Integer32(src.Rows[i]["PaymentJVNumber"]) > 0
                                ? Simulate.String(src.Rows[i]["PaymentJVNumber"])
                                : "",
                            Convert.ToDecimal(Simulate.Val(src.Rows[i]["PaymentAmount"])),
                            dueDate,
                            Simulate.String(src.Rows[i]["InstallmentNote"]),
                            Convert.ToDecimal(Simulate.Val(src.Rows[i]["InstallmentLineTotal"])),
                            Convert.ToDecimal(Simulate.Val(src.Rows[i]["ReconciledAmount"])),
                            Simulate.Integer32(src.Rows[i]["ReconciliationVoucherNumber"]) > 0
                                ? Simulate.String(src.Rows[i]["ReconciliationVoucherNumber"])
                                : "",
                            Simulate.Integer32(src.Rows[i]["FinancingVoucherNumber"]) > 0
                                ? Simulate.String(src.Rows[i]["FinancingVoucherNumber"])
                                : "",
                            Simulate.String(src.Rows[i]["LoanTypeName"]),
                            status);
                    }
                }

                System.Data.DataSet ds = new System.Data.DataSet();
                ds.Tables.Add(print);

                FastReport.Report report = new FastReport.Report();
                report.RegisterData(ds);
                try { report.RegisterData(print, "DataTable1"); } catch { }

                clsReports clsReports = new clsReports();
                clsReports.LoadCompanyFastReport(
                    report,
                    clsTransactionReportDefaults.PagePaymentInstallmentTree,
                    "rptPaymentInstallmentTree",
                    CompanyID,
                    UserID);

                BindPosStyleReportData(report, ds);
                try
                {
                    var dataSrc = report.GetDataSource("DataTable1");
                    if (dataSrc != null)
                    {
                        dataSrc.GetType().GetProperty("Enabled")?.SetValue(dataSrc, true);
                        var tableProp = dataSrc.GetType().GetProperty("Table");
                        if (tableProp != null && tableProp.CanWrite)
                            tableProp.SetValue(dataSrc, print);
                    }
                }
                catch { }

                report.SetParameterValue("report.FromDate", Date1.ToString("yyyy-MM-dd"));
                report.SetParameterValue("report.ToDate", Date2.ToString("yyyy-MM-dd"));
                report.SetParameterValue(
                    "report.Name",
                    string.IsNullOrWhiteSpace(customerName) ? BusinessPartnerID.ToString() : customerName);
                report.SetParameterValue("report.EMPCode", empCode);
                FastreportStanderdParameters(report, UserID, CompanyID);
                report.Prepare();
                return FastreporttoPDF(report);
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
        public string SelectLoanScheduling(int AccountID, int SubAccountID,   int CompanyID,string JVGuid)
        {
            try
            {
                clsReconciliation clsReconciliation = new clsReconciliation();
                DataTable dt = clsReconciliation.SelectLoanScheduling(AccountID, SubAccountID,   CompanyID, JVGuid);
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






                return Fastreporttoxlsx(dtlist, dtName, ColumnType);














            }
            catch (Exception)
            {

                throw;
            }




        }
        [HttpGet]
        [Route("selectLoanSummaryByCustomerReport")]
        public string selectLoanSummaryByCustomerReport(string accountid, string BusinessPartnerID
            , DateTime date1, DateTime date2
            , string CompanyID)
        {
            try
            {
                clsReports clsReports = new clsReports();
                DataTable dt = clsReports.selectLoanSummaryByCustomerReportExcel(
                    Simulate.Integer32(BusinessPartnerID), Simulate.Integer32(accountid),
                    Simulate.StringToDate(date1), Simulate.StringToDate(date2)
                    , Simulate.Integer32(CompanyID));
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
        [Route("selectLoanSummaryByCustomerReportExcel")]
        public ActionResult selectLoanSummaryByCustomerReportExcel( string accountid, string BusinessPartnerID
            ,DateTime date1,DateTime date2
            , string CompanyID)
        {
            try
            {
                clsReports clsReports = new clsReports();
                DataTable dt = clsReports.selectLoanSummaryByCustomerReportExcel( 
                    Simulate.Integer32(BusinessPartnerID), Simulate.Integer32(accountid),
                    Simulate.StringToDate(date1),Simulate.StringToDate(date2)
                    , Simulate.Integer32(CompanyID));

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
               
                
                //ColumnType.Add("string");
                //ColumnType.Add("int");
                //ColumnType.Add("int");
                //ColumnType.Add("int");
                //ColumnType.Add("int");
                //ColumnType.Add("string");
                //ColumnType.Add("string");
                //ColumnType.Add("int");
                //ColumnType.Add("int");
                //dt.Columns.RemoveAt(0);

                //dt.Columns.RemoveAt(1);
                //dt.Columns.RemoveAt(2);
                dt.Columns[0].ColumnName = "العميل";
                dt.Columns[1].ColumnName = "رقم الوظيفي";
                dt.Columns[2].ColumnName = "الشهر";
                dt.Columns[3].ColumnName = "كشف النظام";
                dt.Columns[4].ColumnName = "كشف الملكيه";
                dt.Columns[5].ColumnName = "الفرق";
                //dt.Columns[6].ColumnName = "الملاحظات";
                //dt.Columns[7].ColumnName = "إجمالي المبلغ";
                //dt.Columns[8].ColumnName = "القسط";
                //dt.Columns[9].ColumnName = "المدفوع";
                //dt.Columns[10].ColumnName = "المده";
                //dt.Columns[11].ColumnName = "تاريخ اول قسط";
                //dt.Columns[12].ColumnName = "تاريخ اخر قسط";
                //dt.Columns[13].ColumnName = "المستحق";
                //dt.Columns[14].ColumnName = "المجدول";
                dtlist.Add(dt);


                return Fastreporttoxlsx(dtlist, dtName, ColumnType);

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
                        if (details[i].Amount == 0)
                            continue;

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







                // Return the generated reconciliation transaction number (VoucherNumber)
                // so the client can display it immediately.
                if (IsSaved) { return VoucherNumber; } else { return 0; }
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
            DateTime date5, DateTime date6, string  Accounts,int UserID,int SubAccountID, int CompanyID, bool HideZeroBalances = false)
        {
            try
            {
                clsReports clsReports = new clsReports();
              DataTable dt=  clsReports.SelectAgingReports(date1, date2, date3, date4, date5, date6, Accounts, SubAccountID, CompanyID, HideZeroBalances);
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
            DateTime date5, DateTime date6, string Accounts, int UserID,int SubAccountID, int CompanyID, bool HideZeroBalances = false)
        {
            try
            {
               

                FastReport.Utils.Config.WebMode = true;
                clsReports clsReports = new clsReports();
                DataTable dt = clsReports.SelectAgingReports(date1, date2, date3, date4, date5, date6, Accounts, SubAccountID, CompanyID, HideZeroBalances);




                dsAgingReports ds = new dsAgingReports();

                
                if (dt != null && dt.Rows.Count > 0)
                {
                    for (int i = 0; i < dt.Rows.Count; i++)
                    {
                        ds.AgingReports.Rows.Add();
                        ds.AgingReports.Rows[i]["Index"] = Simulate.Integer32(i+1);
                        ds.AgingReports.Rows[i]["ID"] = Simulate.String(dt.Rows[i]["ID"]);
                        object empCodeVal = dt.Columns.Contains("EMPCode")
                            ? dt.Rows[i]["EMPCode"]
                            : (dt.Columns.Contains("EmpCode") ? dt.Rows[i]["EmpCode"] : "");
                        ds.AgingReports.Rows[i]["EMPCode"] = Simulate.String(empCodeVal);
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



            

                clsReports.LoadCompanyFastReport(
                    report,
                    clsTransactionReportDefaults.PageAging,
                    "rptAging",
                    CompanyID,
                    UserID);
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
        public string SelectBusinessPartnerBalances(DateTime Date, string Accounts,int BranchID,int CostCenterID, int UserID, int CompanyID,bool withZeroAmount, int SubAccountID = 0)
        {
            try
            {
                clsReports clsReports = new clsReports();
                DataTable dt = clsReports.SelectBusinessPartnerBalances(Date, Accounts,  BranchID,  CostCenterID, CompanyID, withZeroAmount, SubAccountID);
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
        public IActionResult SelectBusinessPartnerBalancesPDF(DateTime Date , string Accounts, int BranchID, int CostCenterID, int UserID, int CompanyID,bool withZeroAmount, int SubAccountID = 0)
        {
            try
            {


                FastReport.Utils.Config.WebMode = true;
                clsReports clsReports = new clsReports();
                DataTable dt = clsReports.SelectBusinessPartnerBalances(Date,  Accounts,  BranchID,  CostCenterID, CompanyID, withZeroAmount, SubAccountID);




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

 

                clsReports.LoadCompanyFastReport(
                    report,
                    clsTransactionReportDefaults.PageBusinessPartnerBalances,
                    "rptBusinessPartnerReports",
                    CompanyID,
                    UserID);
                report.RegisterData(ds);
                //report.SetParameterValue("report.Date", (date6).ToString("yyyy-MM-dd"));
                //report.SetParameterValue("report.Date1", (date6 - date1).TotalDays);
                //report.SetParameterValue("report.Date2", Simulate.String((date6 - date2).TotalDays));
   
                report.SetParameterValue("report.Date", (Date).ToString("yyyy-MM-dd"));
          
       
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
        public ActionResult SelectBusinessPartnerBalancesExcel(DateTime Date, string Accounts, int BranchID, int CostCenterID, int UserID, int CompanyID, bool withZeroAmount, int SubAccountID = 0)
        {
            try
            {
                clsReports clsReports = new clsReports();
                DataTable dt = clsReports.SelectBusinessPartnerBalances(Date, Accounts,  BranchID,  CostCenterID, CompanyID, withZeroAmount, SubAccountID);



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
                 
               




                return Fastreporttoxlsx(dtlist, dtName, ColumnType);
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

 

                // Detect file format
                string fileFormat = GetFileFormat(myLogo);
                DataTable dt=new DataTable();

                if (fileFormat == "") { 
                using (MemoryStream stream = new MemoryStream(myLogo))
                {
                        System.Xml.XmlDocument xmlDcoument = new System.Xml.XmlDocument();
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
                }
                else    if (fileFormat == ".xls")
                {
                    // Process .xls file using NPOI
                    //HSSFWorkbook workbook = new HSSFWorkbook(stream);
                    //var sheet = workbook.GetSheetAt(0);
                    //dt = ConvertSheetToDataTable(sheet);
                   // string connectionString = "Provider=Microsoft.Jet.OLEDB.4.0;Data Source=yourfile.xls;Extended Properties=\"Excel 8.0;HDR=YES;\"";

                    //using (OleDbConnection connection = new OleDbConnection(connectionString))
                    //{
                    //    connection.Open();
                    //    OleDbCommand command = new OleDbCommand("SELECT * FROM [Sheet1$]", connection);
                    //    OleDbDataAdapter adapter = new OleDbDataAdapter(command);
                    //    DataTable dataTable = new DataTable();
                    //    adapter.Fill(dataTable);
                    //    // Process dataTable as needed
                    //}
                }
                else if (fileFormat == ".xlsx")
                {
                    using (MemoryStream stream = new MemoryStream(myLogo))
                    {  // Process .xlsx file using ClosedXML
                        var workbook = new XLWorkbook(stream);
                    var worksheet = workbook.Worksheet(1);
                    dt = ConvertWorksheetToDataTable(worksheet);
                    }
                }
                clsBusinessPartner cls = new clsBusinessPartner();
                DataTable dtBP = cls.SelectBusinessPartner(0, 0, "", "", "", "", -1, companyID);
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
        [HttpPost]
        [Route("ConvertWorksheetToDataTable")]
        public DataTable ConvertWorksheetToDataTable(IXLWorksheet excelStream)
        {
            if (excelStream == null)
            {
                throw new ArgumentNullException(nameof(excelStream), "The Excel worksheet cannot be null.");
            }

            // Create a new DataTable
            DataTable dataTable = new DataTable();

            // Read the first row as the column headers
            var headerRow = excelStream.FirstRowUsed();
            if (headerRow == null)
            {
                throw new InvalidOperationException("The Excel worksheet does not contain any rows.");
            }

            foreach (var headerCell in headerRow.CellsUsed())
            {
                string columnName = headerCell.GetString();
                if (string.IsNullOrEmpty(columnName))
                {
                    columnName = $"Column{headerCell.Address.ColumnNumber}";
                }

                dataTable.Columns.Add(columnName);
            }

            // Read the remaining rows as data
            var rows = headerRow.RowBelow().Worksheet.RowsUsed();
            foreach (var row in rows)
            {
                if (row.RowNumber() <= headerRow.RowNumber()) continue; // Skip the header row

                DataRow dataRow = dataTable.NewRow();
                foreach (var cell in row.Cells(1, dataTable.Columns.Count))
                {
                    object cellValue = cell.Value;
                    dataRow[cell.Address.ColumnNumber - 1] = cellValue is null || string.IsNullOrWhiteSpace(cellValue.ToString())
                        ? DBNull.Value
                        : cellValue;
                }
                dataTable.Rows.Add(dataRow);
            }

            return dataTable;

        }
        [HttpPost]
        [Route("GetFileFormat")]
        private string GetFileFormat(byte[] fileBytes)
        {
            // Check for .xls magic number
            if (fileBytes.Length > 4 &&
                fileBytes[0] == 0xD0 && fileBytes[1] == 0xCF &&
                fileBytes[2] == 0x11 && fileBytes[3] == 0xE0)
            {
                return ".xls";
            }
            // Check for .xlsx magic number
            else if (fileBytes.Length > 4 &&
                     fileBytes[0] == 0x50 && fileBytes[1] == 0x4B &&
                     fileBytes[2] == 0x03 && fileBytes[3] == 0x04)
            {
                return ".xlsx";
            }
            else
            {
                return string.Empty;
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

        #region Excel & PDF export
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

                string columnName = ColumnAName[i];
                int count = 0;
              
                for (int b = 0; b < dt.Columns.Count; b++)
                {
                    if (dt.Columns[b].ColumnName== columnName) {
                        count++;
                    }
                }

                if (count > 0)
                {
                    columnName = ColumnAName[i] + "_" + count;

                    dt.Columns.Add(columnName);

                }
                else { 
                dt.Columns.Add(ColumnAName[i]);
                
                }
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
       
            return Fastreporttoxlsx(dtlist, dtSheetName, ColumnType);
        }

        private DataTable ConvertToDataTable(Dictionary<string, Dictionary<string, object>> dataItems)
        {
            try
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
            catch (Exception ex)
            {

                throw;
            }
        }


        [HttpPost]
        [Route("ExportListToPDF")]
        public IActionResult ExportListToPDF(bool isLandScape,string CompanyID,string UserID, [FromBody] JsonElement jsonData, [FromQuery] List<String> ColumnAName, [FromQuery] List<String> ColumnEName, [FromQuery] List<String> ColumnType, [FromQuery] List<String> HeaderParametersName, [FromQuery] List<String> HeaderParametersValue)
        {
            string jsonString = jsonData.GetRawText();
            //List<Dictionary<string, object>> dataItems = System.Text.Json.JsonSerializer.Deserialize<List<Dictionary<string, object>>>(jsonString);

            Dictionary<string, Dictionary<string, object>> dataItems = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, Dictionary<string, object>>>(jsonString);

            DataTable dataTable = ConvertToDataTable(dataItems);

            DataTable dt = new DataTable();
            for (int i = 0; i < ColumnAName.Count; i++)
            {
                string columnName = ColumnAName[i];
                int count = 0;

                for (int b = 0; b < dt.Columns.Count; b++)
                {
                    if (dt.Columns[b].ColumnName == columnName)
                    {
                        count++;
                    }
                }

                if (count > 0)
                {
                    columnName = ColumnAName[i] + "_" + count;

                    dt.Columns.Add(columnName);

                }
                else
                {
                    dt.Columns.Add(ColumnAName[i]);

                }
            }
            for (int i = 0; i < dataTable.Rows.Count; i++)
            {
                dt.Rows.Add();
                for (int ii = 0; ii < ColumnEName.Count; ii++)
                {
                    dt.Rows[i][ColumnAName[ii]] = dataTable.Rows[i][ColumnEName[ii]];
                }
            }

            List<DataTable> dtlist = new List<DataTable>();
            dtlist.Add(dt);
            List<String> dtSheetName = new List<String>();
            dtSheetName.Add("Sheet1");
            List<List<string>> stringPairs = new List<List<string>>()
        {
           
        };
            for (int i = 0; i < HeaderParametersName.Count; i++)
            {
                stringPairs.Add(new List<string> { HeaderParametersName[i], HeaderParametersValue[i] });
            }
            return FastreporttoPDF(CreateDynamicFastReport(isLandScape,"Ar",dt, stringPairs, Simulate.Integer32(UserID),Simulate.Integer32( CompanyID)));


        }
        [HttpPost]
        [Route("CreateDynamicFastReport")]
        Report CreateDynamicFastReport(bool isLandScape, string Lang,DataTable table, List<List<string>> parameters ,int UserID,int CompanyID)
        {


         


            // Create a new report instance
            Report report = new Report();
 
            report.RegisterData(table, "SampleTable");
            report.GetDataSource("SampleTable").Enabled = true;
           
            // Add a new report page
            var page = new ReportPage();
            report.Pages.Add(page);
            if (isLandScape) {
                page.PaperWidth = 297;   // A4 width for landscape
                page.PaperHeight = 210;

            } else {
                // Set page properties
                page.PaperWidth = 210;   // A4 width
                page.PaperHeight = 297;  // A4 height

            }
       
            
            // Set margins
            page.LeftMargin = Units.Millimeters * 1;
            page.RightMargin = Units.Millimeters * 1;
            page.TopMargin = Units.Millimeters * 2;
            page.BottomMargin = 1;

            //////////////////////////////
            // --- Title Band report
            var titleBand = new FastReport.ReportTitleBand
            {
                Visible=true,
            CanGrow=true,
                Name = "TitleBand",
                //FillColor = System.Drawing.Color.Red,
                Height = Units.Centimeters * 3,  // Height adjusted
               // Fill = new SolidFill(System.Drawing.Color.LightBlue),
                PrintOn = PrintOn.FirstPage
            };
            
            for (int i = 0; i < parameters.Count; i++)
            {
                titleBand.Objects.Add(addTextToFastReport(parameters[i][0] ,0, i,page));
                titleBand.Objects.Add(addTextToFastReport(parameters[i][1] ,1, i, page));
            }

            var txtAddresas = new TextObject
            {
                Name = "TitleText",
                Bounds = new RectangleF(0, (Units.Centimeters *3)+ Units.Centimeters * parameters.Count/2, Units.Centimeters * 20, Units.Centimeters * 1),
                Text = " ",  // Bind to the parameter
                Font = new System.Drawing.Font("Arial", 18, FontStyle.Bold),
                HorzAlign = HorzAlign.Center,
                VertAlign = VertAlign.Center,
                //Height = 60,  
            };
         //   txtAddresas.Border.Lines = BorderLines.All;
      //      txtAddresas.Border.Width = 1;
            titleBand.Objects.Add(txtAddresas);






           // --- Add Picture Object (Logo) ---
           var pictureObject = new PictureObject
            {
                Name = "CompanyLogo",
                Bounds = new RectangleF(Units.Centimeters * 0, Units.Centimeters * 0.5f, Units.Centimeters * 4, Units.Centimeters * 2.5f),
               // SizeMode = fast.StretchImage
            };

            // Add the picture to the title band
            titleBand.Objects.Add(pictureObject);

            // Add the title band to the page
           // page.Bands.Add(titleBand);
            page.AddChild(titleBand);

            //var titleObject = new TextObject
            //{
            //   // Bounds = new RectangleF(Units.Centimeters * 6, Units.Centimeters * 1, Units.Centimeters * 10, Units.Centimeters * 1),
            //    Text = "Sales Report",  // Use your dynamic title
            //    Font = new System.Drawing.Font("Arial", 14, FontStyle.Bold),
            //    HorzAlign = HorzAlign.Center,
            //    VertAlign = VertAlign.Center
            //};

            //titleBand.Objects.Add(titleObject);


            //                    

             


            ////////// Company Name
            float locationy = 0.01f;
             var txtCompany = new TextObject
            {
                Bounds = new RectangleF(0, locationy , Units.Centimeters *20, Units.Centimeters * 1),
                Text = "[Standerd.CompanyName]",  // Bind to the parameter
                Font = new System.Drawing.Font("Arial", 36, FontStyle.Bold),
                HorzAlign = HorzAlign.Center,
                VertAlign = VertAlign.Center,Height=60,
              
            };
            titleBand.Objects.Add(txtCompany);


            ////////// Address
            var txtAddress = new TextObject
            {
                Bounds = new RectangleF(0, locationy + Units.Centimeters * 1, Units.Centimeters * 20, Units.Centimeters * 1),
                Text = "[Standerd.Address]",  // Bind to the parameter
                Font = new System.Drawing.Font("Arial", 18, FontStyle.Bold),
                HorzAlign = HorzAlign.Center,
                VertAlign = VertAlign.Center,
                Height = 60,
            };
            titleBand.Objects.Add(txtAddress);
          
            PageFooterBand myPageFooterBand =new FastReport.PageFooterBand
            {
                Visible = true,
                CanGrow = true,
                Name = "TitleBand",
                //FillColor = System.Drawing.Color.Red,
                Height = 40,  // Height adjusted
                                                 // Fill = new SolidFill(System.Drawing.Color.LightBlue),
                PrintOn = PrintOn.FirstPage
            };
            page.AddChild(myPageFooterBand);

            //// PrintDate
            var txtPrintDate = new TextObject
            {
                Bounds = new RectangleF(300, 0, Units.Centimeters * 20, Units.Centimeters * 1),
                Text = "[Standerd.PrintDate]",  // Bind to the parameter
                Font = new System.Drawing.Font("Arial", 10, FontStyle.Regular),
                HorzAlign = HorzAlign.Center,
                VertAlign = VertAlign.Center,
                Height = 60,
            };
            myPageFooterBand.Objects.Add(txtPrintDate);
            ////////  User  
            var txtUser = new TextObject
            {
                Bounds = new RectangleF(0, 0, Units.Centimeters * 20, Units.Centimeters * 1),
                Text = "[Standerd.User]",  // Bind to the parameter
                Font = new System.Drawing.Font("Arial", 10, FontStyle.Regular),
                HorzAlign = HorzAlign.Center,
                VertAlign = VertAlign.Center,
                Height = 60,
            };
            myPageFooterBand.Objects.Add(txtUser);

            ////////// Standerd.PrintTime
            var txtPrintTime = new TextObject
            {
                Bounds = new RectangleF(0-300, 0, Units.Centimeters * 20, Units.Centimeters * 1),
                Text = "[Standerd.PrintTime]",  // Bind to the parameter
                Font = new System.Drawing.Font("Arial", 10, FontStyle.Regular),
                HorzAlign = HorzAlign.Center,
                VertAlign = VertAlign.Center,
                Height = 60,
            };
            myPageFooterBand.Objects.Add(txtPrintTime);


            // Calculate the width of each column to fit within the page width
            float columnWidth = Units.Centimeters * 20 / table.Columns.Count;
            if(isLandScape)
                columnWidth = Units.Centimeters * 29 / table.Columns.Count;

            // Create and add a data band to the page
            var dataBandHeader = new DataHeaderBand
            {
                Height = 1,

                Bounds = new RectangleF(0, 0, columnWidth * table.Columns.Count - 1, Units.Centimeters * 0.7f),
            };

            var dataBand = new DataBand
            {
                DataSource = report.GetDataSource("SampleTable"),
                Height = Units.Centimeters * 0.5f,
                //  Bounds = new RectangleF(0, 10, columnWidth, Units.Centimeters * 0.2f),
              GrowToBottom=true,
                CanGrow = true,
                CanShrink = true,
                KeepDetail = true,
                Header = dataBandHeader
            };

            page.Bands.Add(dataBand);
           
            // Create a header row with column names
            for (int i = 0; i < table.Columns.Count; i++)
            {

                var startlocation = columnWidth *   - i;
                if (Lang == "Ar") {
                    startlocation = columnWidth * (table.Columns.Count - 1 - i);
                }
                var headerCell = new TextObject
                {
                   
                    Bounds = new RectangleF(startlocation, 0, columnWidth, Units.Centimeters * 0.7f),
                    Text = table.Columns[i].ColumnName,
                    VertAlign = VertAlign.Center,
                    HorzAlign = HorzAlign.Center,
                    Font = new System.Drawing.Font("Arial", 10, FontStyle.Bold),
                    Fill = new SolidFill(System.Drawing.Color.LightGray),
                };
                headerCell.Border.Lines = BorderLines.All;  // Set border to all sides
                headerCell.Border.Width = 1;                // Set border width
                headerCell.Border.Color = System.Drawing.Color.Black;      // Set border color
                dataBandHeader.Objects.Add(headerCell);
            }

            // Create text objects for each row and column in the DataTable
            for (int columnIndex = 0; columnIndex < table.Columns.Count; columnIndex++)
            {
                var startlocation = columnWidth * -columnIndex;
                HorzAlign horzAlign = HorzAlign.Left;
                if (Lang == "Ar")
                {
                      horzAlign = HorzAlign.Right;
                    startlocation = columnWidth * (table.Columns.Count - 1 - columnIndex);
                }
                var textObject = new TextObject
                {
                    GrowToBottom = true,
                    CanGrow = true,
                    Bounds = new RectangleF(startlocation   , 0, columnWidth, Units.Centimeters * 0.7f),
                    Text = $"[SampleTable.{table.Columns[columnIndex].ColumnName}]",
                    HorzAlign = horzAlign,
                    VertAlign = VertAlign.Center,
                    Border = new FastReport.Border(),  // Add border
                   // Padding = new Padding(5, 2, 5, 2),  // Add padding for better spacing
                    Font = new System.Drawing.Font("Arial", 10, FontStyle.Regular),  // Use a readable font
                   
                    CanShrink = true,
                };// Apply borders
                textObject.Border.Lines = BorderLines.All;  // Set border to all sides
                textObject.Border.Width = 1;                // Set border width
                textObject.Border.Color = System.Drawing.Color.Black;      // Set border color
                dataBand.Objects.Add(textObject);
            }


            FastreportStanderdParameters(report,UserID,CompanyID);
            // Prepare the report and show it
            report.Prepare();
           return report ;
        }

        private TextObject addTextToFastReport(string text , int ColIndex, int RowIndex,ReportPage Page) {
         
            var font = new System.Drawing.Font("Arial", 10, FontStyle.Bold);
            var width = Units.Centimeters * 4;
            var startlocationx = Page.PaperWidth +390;
            if (ColIndex % 2 != 0) {
                font = new System.Drawing.Font("Arial", 10, FontStyle.Regular);
                width = Units.Centimeters * 6;
                startlocationx = startlocationx - width;
            }
            int aa = (int)Math.Floor(Simulate.decimal_( RowIndex) / 2);
            var startlocationxy =( aa * Units.Centimeters * 1)+120;
            if (RowIndex % 2 != 0)
            {
                startlocationx = startlocationx - Units.Centimeters * 10;


            }

            var titleText = new TextObject
            {
                Name = "TitleText",
                Bounds = new RectangleF(startlocationx, startlocationxy, width, Units.Centimeters  *1),
                //  Font = new Font("Arial", 18, FontStyle.Bold),
                Text = text,
                Font = font,
                HorzAlign = HorzAlign.Center,
                VertAlign = VertAlign.Center
            }; titleText.Border.Lines = BorderLines.All;
            titleText.Border.Width = 1;
            return titleText;
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

        #region Leads
        [Route("InsertLeads")]
        public int InsertLeads(string AName, string Tel1, string Email, string Country, string Note, int CompanyID, int CreationUserID = 1)
        {
            try
            {
                clsLeads clsLeads = new clsLeads();
                int A = clsLeads.InsertLead(Simulate.String(AName), Simulate.String(Tel1), Simulate.String(Email), Simulate.String(Country), Simulate.String(Note), CompanyID, CreationUserID);
                return A;
            }
            catch (Exception ex)
            {

                throw;
            }

        }

        [HttpGet]
        [Route("SelectLeads")]
        public string SelectLeads(int ID, int CompanyID, int StatusFilter = -1)
        {
            clsLeads cls = new clsLeads();
            DataTable dt = cls.SelectLeads(ID, CompanyID, StatusFilter);
            return dt != null ? Newtonsoft.Json.JsonConvert.SerializeObject(dt) : "";
        }

        [HttpPost]
        [Route("UpdateLeadStatus")]
        public bool UpdateLeadStatus(int ID, int Status, int CompanyID)
        {
            clsLeads cls = new clsLeads();
            return cls.UpdateLeadStatus(ID, Status, CompanyID);
        }

        [HttpPost]
        [Route("UpdateInvoiceHeaderOpportunityID")]
        public bool UpdateInvoiceHeaderOpportunityID(string Guid, int OpportunityID, int CompanyID)
        {
            if (string.IsNullOrWhiteSpace(Guid) || OpportunityID <= 0 || CompanyID <= 0)
                return false;
            clsSQL sql = new clsSQL();
            SqlParameter[] prm =
            {
                new SqlParameter("@Guid", SqlDbType.VarChar) { Value = Guid },
                new SqlParameter("@OpportunityID", SqlDbType.Int) { Value = OpportunityID },
                new SqlParameter("@CompanyID", SqlDbType.Int) { Value = CompanyID },
            };
            return sql.ExecuteNonQueryStatement(
                @"UPDATE tbl_InvoiceHeader SET OpportunityID = @OpportunityID
                  WHERE Guid = @Guid AND CompanyID = @CompanyID",
                sql.CreateDataBaseConnectionString(CompanyID), prm) > 0;
        }
        #endregion
        #region Forgot Password

        const string ForgotPasswordGenericMessage =
            "If your email and phone match our records, a one-time verification code was sent. Check your inbox and spam folder.";

        [HttpGet]
        [Route("ForgotPassword")]
        public int ForgotPassword(string email, string phoneNumber, string CompanyID)
        {
            var result = ProcessForgotPasswordOtpRequest(email, phoneNumber, Simulate.Integer32(CompanyID));
            return result.OtpQueued ? 1 : 0;
        }

        [HttpPost]
        [Route("RequestForgotPasswordOtp")]
        public string RequestForgotPasswordOtp(string email, string phoneNumber, int CompanyID)
        {
            var result = ProcessForgotPasswordOtpRequest(email, phoneNumber, CompanyID);
            string message = BuildForgotPasswordUserMessage(result);
            var configuration = HttpContext?.RequestServices?.GetService<IConfiguration>();
            var environment = HttpContext?.RequestServices?.GetService<IHostEnvironment>();
            bool exposeDevOtp = result.OtpQueued
                && !result.EmailSent
                && clsPasswordResetEmailSender.ShouldExposeOtpInApiResponse(configuration, environment);
            return JsonConvert.SerializeObject(new
            {
                success = result.OtpQueued,
                message,
                canVerify = result.OtpQueued,
                emailSent = result.EmailSent,
                reason = result.Reason,
                phoneHint = result.PhoneHint,
                devOtp = exposeDevOtp ? result.OtpCode : null
            });
        }

        static string BuildForgotPasswordUserMessage(ForgotPasswordOtpProcessResult result)
        {
            if (result.OtpQueued)
            {
                if (result.EmailSent) return ForgotPasswordGenericMessage;
                return "Verification code created. Email is not configured — use the code shown below (development) or in the API console.";
            }
            if (result.RateLimited) return "Please wait a minute before requesting another code.";
            switch (result.Reason)
            {
                case "no_company":
                    return "Select your company on the login screen before resetting your password.";
                case "phone_mismatch":
                    if (!string.IsNullOrEmpty(result.PhoneHint))
                    {
                        return $"Email was found, but the phone number does not match HR. The number on file ends with {result.PhoneHint}. Update Tel1 in employee settings or use that number.";
                    }
                    return "Email was found, but the phone number does not match the employee record. Check Tel1 in HR employee settings.";
                case "multiple_accounts":
                    return "More than one employee uses this email for the selected company. Contact your administrator.";
                case "no_employee":
                    return "No employee with this email or username was found for the selected company. Select the correct company or ask HR to add your email and phone.";
                default:
                    return "We could not verify your email and phone for the selected company. Check your employee profile in HR or contact your administrator.";
            }
        }

        [HttpPost]
        [Route("VerifyForgotPasswordOtp")]
        public string VerifyForgotPasswordOtp(string email, string otp, int CompanyID)
        {
            try
            {
                int companyId = Simulate.Integer32(CompanyID);
                if (companyId <= 0 || string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(otp))
                {
                    return JsonConvert.SerializeObject(new
                    {
                        success = false,
                        message = "Invalid or expired verification code."
                    });
                }

                string normalizedOtp = (otp ?? string.Empty).Trim();
                if (normalizedOtp.Length != 6 || !normalizedOtp.All(char.IsDigit))
                {
                    return JsonConvert.SerializeObject(new
                    {
                        success = false,
                        message = "Enter the 6-digit code from your email."
                    });
                }

                clsForgotPasswordRequest forgot = new clsForgotPasswordRequest();
                DataTable dtRequest = forgot.SelectForgotPasswordRequest(email, normalizedOtp, companyId);
                if (dtRequest == null || dtRequest.Rows.Count == 0)
                {
                    return JsonConvert.SerializeObject(new
                    {
                        success = false,
                        message = "Invalid or expired verification code."
                    });
                }

                int requestId = 0;
                if (dtRequest != null && dtRequest.Rows.Count > 0)
                {
                    requestId = Simulate.Integer32(dtRequest.Rows[0]["ID"]);
                }

                return JsonConvert.SerializeObject(new
                {
                    success = true,
                    message = "Verification code accepted. Choose your new password.",
                    email = clsForgotPasswordRequest.NormalizeEmail(email),
                    resetRequestId = requestId
                });
            }
            catch (Exception ex)
            {
                return JsonConvert.SerializeObject(new
                {
                    success = false,
                    message = "Could not verify code: " + ex.Message
                });
            }
        }

        [HttpPost]
        [Route("CompleteForgotPasswordReset")]
        [Consumes("application/json")]
        public string CompleteForgotPasswordReset([FromBody] JsonElement body)
        {
            try
            {
                if (body.ValueKind is JsonValueKind.Undefined or JsonValueKind.Null)
                {
                    return JsonConvert.SerializeObject(new
                    {
                        success = false,
                        message = "Invalid request. Send email, code, and new password as JSON."
                    });
                }

                string email = ReadForgotPasswordJsonString(body, "email");
                string otp = ReadForgotPasswordJsonString(body, "otp");
                string newPassword = ReadForgotPasswordJsonString(body, "newPassword");
                string confirmPassword = ReadForgotPasswordJsonString(body, "confirmPassword");
                int companyId = ReadForgotPasswordJsonInt(body, "companyID", "CompanyID");
                int resetRequestId = ReadForgotPasswordJsonInt(body, "resetRequestId");

                if (companyId <= 0)
                {
                    return JsonConvert.SerializeObject(new
                    {
                        success = false,
                        message = "Select your company on the login screen first."
                    });
                }

                string pwd = newPassword ?? string.Empty;
                string confirm = confirmPassword ?? string.Empty;
                string normalizedEmail = clsForgotPasswordRequest.NormalizeEmail(email);
                if (string.IsNullOrWhiteSpace(normalizedEmail))
                {
                    return JsonConvert.SerializeObject(new
                    {
                        success = false,
                        message = "Email is required."
                    });
                }
                if (pwd.Length < 8)
                {
                    return JsonConvert.SerializeObject(new
                    {
                        success = false,
                        message = "Password must be at least 8 characters."
                    });
                }
                if (!string.Equals(pwd, confirm, StringComparison.Ordinal))
                {
                    return JsonConvert.SerializeObject(new
                    {
                        success = false,
                        message = "Passwords do not match."
                    });
                }

                clsForgotPasswordRequest forgot = new clsForgotPasswordRequest();
                DataTable dtRequest = null;

                if (resetRequestId > 0)
                {
                    dtRequest = forgot.SelectForgotPasswordRequestById(resetRequestId, companyId);
                    if (dtRequest != null && dtRequest.Rows.Count > 0)
                    {
                        string rowEmail = clsForgotPasswordRequest.NormalizeEmail(
                            Simulate.String(dtRequest.Rows[0]["Email"]));
                        if (!string.Equals(rowEmail, normalizedEmail, StringComparison.Ordinal))
                        {
                            dtRequest = null;
                        }
                    }
                }

                if (dtRequest == null || dtRequest.Rows.Count == 0)
                {
                    string normalizedOtp = (otp ?? string.Empty).Trim();
                    if (normalizedOtp.Length != 6 || !normalizedOtp.All(char.IsDigit))
                    {
                        return JsonConvert.SerializeObject(new
                        {
                            success = false,
                            message = "Your verification session expired. Go back and request a new code."
                        });
                    }
                    dtRequest = forgot.SelectForgotPasswordRequest(email, normalizedOtp, companyId);
                }

                if (dtRequest == null || dtRequest.Rows.Count == 0)
                {
                    return JsonConvert.SerializeObject(new
                    {
                        success = false,
                        message = "Your verification session expired. Request a new code and verify again before saving the password."
                    });
                }

                int employeeId = clsForgotPasswordRequest.GetEmployeeIdFromRow(dtRequest.Rows[0]);
                int requestId = Simulate.Integer32(dtRequest.Rows[0]["ID"]);

                if (employeeId <= 0)
                {
                    return JsonConvert.SerializeObject(new
                    {
                        success = false,
                        message = "Invalid employee record for this reset. Contact your administrator."
                    });
                }

                clsEmployee clsEmployee = new clsEmployee();
                DataTable empCheck = clsEmployee.SelectEmployee(employeeId, "", "", "", "", "", "", companyId, -1);
                if (empCheck == null || empCheck.Rows.Count == 0)
                {
                    return JsonConvert.SerializeObject(new
                    {
                        success = false,
                        message = "Employee account not found for this company. Contact your administrator."
                    });
                }

                bool updated = clsEmployee.UpdateEmployeePassword(employeeId, pwd, companyId);
                if (!updated)
                {
                    return JsonConvert.SerializeObject(new
                    {
                        success = false,
                        message = "Could not save the new password. Contact your administrator."
                    });
                }

                forgot.ConsumeForgotPasswordRequest(requestId, companyId);

                string loginUserName = normalizedEmail;
                DataTable emp = clsEmployee.SelectEmployee(employeeId, "", "", "", "", "", "", companyId, -1);
                if (emp != null && emp.Rows.Count > 0)
                {
                    string userName = Simulate.String(emp.Rows[0]["UserName"]);
                    if (!string.IsNullOrWhiteSpace(userName))
                    {
                        loginUserName = userName.Trim();
                    }
                }

                return JsonConvert.SerializeObject(new
                {
                    success = true,
                    message = "Password updated successfully. You can sign in with your new password.",
                    email = normalizedEmail,
                    userName = loginUserName
                });
            }
            catch (Exception ex)
            {
                return JsonConvert.SerializeObject(new
                {
                    success = false,
                    message = "Could not update password: " + ex.Message
                });
            }
        }

        static string ReadForgotPasswordJsonString(JsonElement body, string propertyName, string defaultValue = "")
        {
            if (!body.TryGetProperty(propertyName, out JsonElement prop))
            {
                return defaultValue;
            }
            return prop.ValueKind switch
            {
                JsonValueKind.String => prop.GetString() ?? defaultValue,
                JsonValueKind.Number => prop.GetRawText(),
                JsonValueKind.True => "true",
                JsonValueKind.False => "false",
                _ => defaultValue,
            };
        }

        static int ReadForgotPasswordJsonInt(JsonElement body, params string[] propertyNames)
        {
            foreach (string propertyName in propertyNames)
            {
                if (!body.TryGetProperty(propertyName, out JsonElement prop))
                {
                    continue;
                }
                if (prop.ValueKind == JsonValueKind.Number && prop.TryGetInt32(out int number))
                {
                    return number;
                }
                if (prop.ValueKind == JsonValueKind.String
                    && int.TryParse(prop.GetString(), out int parsed))
                {
                    return parsed;
                }
            }
            return 0;
        }

        private sealed class ForgotPasswordOtpProcessResult
        {
            public bool OtpQueued { get; set; }
            public bool RateLimited { get; set; }
            public bool EmailSent { get; set; }
            public string OtpCode { get; set; } = "";
            public string Reason { get; set; } = "";
            public string PhoneHint { get; set; } = "";
        }

        private ForgotPasswordOtpProcessResult ProcessForgotPasswordOtpRequest(string email, string phoneNumber, int companyId)
        {
            var result = new ForgotPasswordOtpProcessResult();
            try
            {
                if (companyId <= 0
                    || string.IsNullOrWhiteSpace(email)
                    || string.IsNullOrWhiteSpace(phoneNumber))
                {
                    result.Reason = companyId <= 0 ? "no_company" : "invalid_input";
                    return result;
                }

                string normalizedEmail = clsForgotPasswordRequest.NormalizeEmail(email);
                if (!normalizedEmail.Contains('@') || normalizedEmail.Length < 5)
                {
                    result.Reason = "invalid_input";
                    return result;
                }

                clsForgotPasswordRequest forgot = new clsForgotPasswordRequest();
                var configuration = HttpContext?.RequestServices?.GetService<IConfiguration>();
                var environment = HttpContext?.RequestServices?.GetService<IHostEnvironment>();
                var logger = HttpContext?.RequestServices?.GetService<ILogger<Main>>();

                if (forgot.CountRecentRequests(normalizedEmail, companyId, 60) >= clsForgotPasswordRequest.MaxRequestsPerHour)
                {
                    result.RateLimited = true;
                    result.Reason = "rate_limited";
                    return result;
                }

                DateTime? lastRequest = forgot.GetLastRequestTime(normalizedEmail, companyId);
                if (lastRequest.HasValue
                    && (DateTime.Now - lastRequest.Value).TotalSeconds < clsForgotPasswordRequest.MinSecondsBetweenRequests)
                {
                    result.RateLimited = true;
                    result.Reason = "rate_limited";
                    return result;
                }

                DataTable dt = forgot.FindEmployeesByEmailOrLogin(companyId, email);

                if (dt == null || dt.Rows.Count == 0)
                {
                    if (configuration != null
                        && clsAdminLogin.IsEnabled(configuration)
                        && string.Equals(
                            normalizedEmail,
                            clsForgotPasswordRequest.NormalizeEmail(configuration["AdminLogin:Email"] ?? ""),
                            StringComparison.Ordinal))
                    {
                        clsEmployee clsEmployee = new clsEmployee();
                        dt = clsAdminLogin.ResolveEmployeeForCompany(clsEmployee, companyId, configuration);
                    }
                }

                if (dt == null || dt.Rows.Count == 0)
                {
                    result.Reason = "no_employee";
                    logger?.LogWarning(
                        "Password reset: no employee for company {CompanyId} email {Email}",
                        companyId,
                        normalizedEmail);
                    return result;
                }

                var matches = clsForgotPasswordRequest.FilterByPhone(dt, phoneNumber);

                if (matches.Count == 0 && dt.Rows.Count == 1)
                {
                    result.Reason = "phone_mismatch";
                    result.PhoneHint = clsForgotPasswordRequest.PhoneHintSuffix(dt.Rows[0]);
                    logger?.LogWarning(
                        "Password reset: phone mismatch company {CompanyId} email {Email} storedTel {Tel}",
                        companyId,
                        normalizedEmail,
                        Simulate.String(dt.Rows[0]["Tel1"]));
                    return result;
                }

                if (matches.Count != 1)
                {
                    result.Reason = matches.Count > 1 ? "multiple_accounts" : "no_employee";
                    return result;
                }

                int employeeId = Simulate.Integer32(matches[0]["ID"]);
                forgot.InvalidatePendingForEmployee(companyId, employeeId);
                string generatedPassword = clsForgotPasswordRequest.GenerateSecureOtp();
                result.OtpCode = generatedPassword;
                forgot.InsertForgotPasswordRequest(companyId, normalizedEmail, phoneNumber.Trim(), generatedPassword, employeeId);

                if (configuration != null)
                {
                    result.EmailSent = clsPasswordResetEmailSender.TrySend(
                        configuration,
                        environment,
                        logger,
                        normalizedEmail,
                        generatedPassword,
                        clsForgotPasswordRequest.OtpExpiryMinutes);
                }

                result.OtpQueued = true;
                return result;
            }
            catch (Exception)
            {
                throw;
            }
        }

        #endregion
        #region BranchFloors

        [HttpGet]
        [Route("SelectBranchFloors")]
        public string SelectBranchFloors(int ID, int CompanyID)
        {
            try
            {
                clsBranchFloors clsBranchFloors = new clsBranchFloors();
                DataTable dt = clsBranchFloors.SelectBranchFloors(ID, "", "", 0, CompanyID);
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
        [Route("DeleteBranchFloorByID")]
        public bool DeleteBranchFloorByID(int ID, int CompanyID)
        {
            try
            {
                 
                clsBranchFloors clsBranchFloors = new clsBranchFloors();
                bool A = clsBranchFloors.DeleteBranchFloorByID(ID, CompanyID);
                return A;
            }
            catch (Exception)
            {
                throw;
            }
        }

        [HttpGet]
        [Route("InsertBranchFloor")]
        public int InsertBranchFloor(string AName, string EName, int BranchID, int CompanyID, int CreationUserID)
        {
            try
            {
                clsBranchFloors clsBranchFloors = new clsBranchFloors();
                int A = clsBranchFloors.InsertBranchFloor(Simulate.String(AName), Simulate.String(EName), BranchID, CompanyID, CreationUserID);
                return A;
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        [HttpGet]
        [Route("UpdateBranchFloor")]
        public int UpdateBranchFloor(int ID, string AName, string EName, int BranchID, int ModificationUserID, int CompanyID)
        {
            try
            {
                clsBranchFloors clsBranchFloors = new clsBranchFloors();
                int A = clsBranchFloors.UpdateBranchFloor(ID, Simulate.String(AName), Simulate.String(EName), BranchID, ModificationUserID, CompanyID);
                return A;
            }
            catch (Exception)
            {
                throw;
            }
        }

        #endregion
        #region Branch Floors Tables

        [HttpGet]
        [Route("SelectBranchFloorsTables")]
        public string SelectBranchFloorsTables(int ID,int FloorID, int CompanyID)
        {
            try
            {
                clsBranchFloorsTables clsBranchFloors = new clsBranchFloorsTables();
                DataTable dt = clsBranchFloors.SelectBranchFloorsTables(ID, FloorID, "", "", CompanyID);
                if (dt != null)
                {
                    string JSONString = JsonConvert.SerializeObject(dt);
                    return JSONString;
                }
                else
                {
                    return string.Empty;
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Error selecting branch floors", ex);
            }
        }

        [HttpGet]
        [Route("DeleteBranchFloorsTableByID")]
        public bool DeleteBranchFloorsTableByID(int ID, int CompanyID)
        {
            try
            {
                clsBranchFloorsTables clsBranchFloors = new clsBranchFloorsTables();
                return clsBranchFloors.DeleteBranchFloorsTableByID(ID, CompanyID);
            }
            catch (Exception ex)
            {
                throw new Exception("Error deleting branch floor", ex);
            }
        }

        [HttpPost]
        [Route("InsertBranchFloorsTable")]
        public int InsertBranchFloorsTable(string AName, string EName, string Shape, string Color, int ChairsCount, int PositionX, int PositionY, decimal Width, int FloorID, int CompanyID, int CreationUserID)
        {
            try
            {
                clsBranchFloorsTables clsBranchFloors = new clsBranchFloorsTables();
                return clsBranchFloors.InsertBranchFloorsTable(AName, EName, Shape, Color, ChairsCount, PositionX, PositionY, Width, FloorID, CompanyID, CreationUserID);
            }
            catch (Exception ex)
            {
                throw new Exception("Error inserting branch floor", ex);
            }
        }

        [HttpPost]
        [Route("UpdateBranchFloorsTable")]
        public int UpdateBranchFloorsTable(int ID, string AName, string EName, string Shape, string Color, int ChairsCount, int PositionX, int PositionY, decimal Width, int ModificationUserID, int CompanyID)
        {
            try
            {
                clsBranchFloorsTables clsBranchFloors = new clsBranchFloorsTables();
                return clsBranchFloors.UpdateBranchFloorsTable(ID, AName, EName, Shape, Color, ChairsCount, PositionX, PositionY, Width, ModificationUserID, CompanyID);
            }
            catch (Exception ex)
            {
                throw new Exception("Error updating branch floor", ex);
            }
        }
        [HttpPost]
        [Route("UpdateBranchFloorsTablesStatus")]
        public string UpdateBranchFloorsTablesStatus(int CompanyID, int TableID, int NewColor)
        {
            try
            {
                return "";
            //    await _tableService.tbl_BranchFloorsTablesColorUpdated(CompanyID, TableID, NewColor);
            // return Ok("Branch floor table status updated.");
            }
            catch (Exception ex)
            {
                throw new Exception("Error inserting branch floor", ex);
            }
        }
        #endregion
        #region Currency

        [HttpGet]
        [Route("SelectCurrency")]
        public string SelectCurrency(int ID, int CompanyID)
        {
            try
            {
                clsCurrency clsCurrency = new clsCurrency();
                DataTable dt = clsCurrency.SelectCurrency(ID, "", "", CompanyID);
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
        [Route("DeleteCurrencyByID")]
        public bool DeleteCurrencyByID(int ID, int CompanyID)
        {
            try
            {
                clsCurrency clsCurrency = new clsCurrency();
                bool result = clsCurrency.DeleteCurrencyByID(ID, CompanyID);
                return result;
            }
            catch (Exception)
            {
                throw;
            }
        }

        [HttpGet]
        [Route("InsertCurrency")]
        public int InsertCurrency(string AName, string EName, string Code, string PartAName, string PartEName,
                                  int DecimalPlaces, string Symbol, decimal ExchangeRate, bool IsActive, bool IsBase,
                                  int CompanyID, int CreationUserId)
        {
            try
            {
                clsCurrency clsCurrency = new clsCurrency();
                int result = clsCurrency.InsertCurrency(AName, EName, Code, PartAName, PartEName, DecimalPlaces, Symbol, ExchangeRate, IsActive, IsBase, CreationUserId, CompanyID);
                return result;
            }
            catch (Exception)
            {
                throw;
            }
        }

        [HttpGet]
        [Route("UpdateCurrency")]
        public int UpdateCurrency(int ID, string AName, string EName, string Code, string PartAName, string PartEName,
                                  int DecimalPlaces, string Symbol, decimal ExchangeRate, bool IsActive, bool IsBase,
                                  int ModificationUserId, int CompanyID)
        {
            try
            {
                clsCurrency clsCurrency = new clsCurrency();
                int result = clsCurrency.UpdateCurrency(ID, AName, EName, Code, PartAName, PartEName, DecimalPlaces, Symbol, ExchangeRate, IsActive, IsBase, ModificationUserId, CompanyID);
                return result;
            }
            catch (Exception)
            {
                throw;
            }
        }

        #endregion
    }
}

 