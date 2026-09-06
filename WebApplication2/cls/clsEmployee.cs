using DocumentFormat.OpenXml.Bibliography;
using DocumentFormat.OpenXml.Office2016.Drawing.ChartDrawing;
using FastReport;
using FastReport.Barcode;
using Microsoft.Data.SqlClient;
using System;
using System.Data;
using System.Security.Cryptography.Xml;
using static WebApplication2.MainClasses.clsEnum;

namespace WebApplication2.cls
{
    public class clsEmployee
    {
        public DataTable SelectEmployee(int Id, string AName, string EName, string UserName, string Password,string Email, string Tel1,int CompanyId, int IsSystemUser)
        {
            try
            {
                SqlParameter[] prm =
                 { new SqlParameter("@Id", SqlDbType.Int) { Value = Id },
      new SqlParameter("@AName", SqlDbType.NVarChar,-1) { Value = AName },
       new SqlParameter("@EName", SqlDbType.NVarChar,-1) { Value = EName },
           new SqlParameter("@UserName", SqlDbType.NVarChar,-1) { Value = UserName },
                new SqlParameter("@Password", SqlDbType.NVarChar,-1) { Value = Password },
                    new SqlParameter("@CompanyId", SqlDbType.Int) { Value = CompanyId },
                      new SqlParameter("@IsSystemUser", SqlDbType.Int) { Value = IsSystemUser },
                               new SqlParameter("@Email",SqlDbType.NVarChar,-1) { Value = Email },
                      new SqlParameter("@Tel1", SqlDbType.NVarChar,-1) { Value = Tel1 },

                }; clsSQL clsSQL = new clsSQL();
                DataTable dt = clsSQL.ExecuteQueryStatement(@"select * from tbl_employee where (id=@Id or @Id=0 ) and
                     (AName=@AName or @AName='' ) and (EName=@EName or @EName='' ) and (UserName=@UserName or @UserName='' )
                      and  (Password=@Password or @Password='' )
and (CompanyId=@CompanyId or @CompanyId=0 )
   and  (Email=@Email or @Email='' )
   and  (Tel1=@Tel1 or @Tel1='' )
and (IsSystemUser=@IsSystemUser or @IsSystemUser=-1 )


", clsSQL.CreateDataBaseConnectionString(CompanyId), prm);

                return dt;
            }
            catch (Exception)
            {

                throw;
            }


        }

        /// <summary>
        /// Lookup system user by keyboard-wedge access card UID (exact match, trimmed).
        /// </summary>
        public DataTable SelectEmployeeByAccessCard(string accessCardUid, int companyId)
        {
            try
            {
                string uid = Simulate.String(accessCardUid).Trim();
                if (string.IsNullOrEmpty(uid) || companyId <= 0)
                    return new DataTable();

                SqlParameter[] prm =
                {
                    new SqlParameter("@AccessCardUid", SqlDbType.NVarChar, -1) { Value = uid },
                    new SqlParameter("@CompanyId", SqlDbType.Int) { Value = companyId },
                };
                clsSQL clsSQL = new clsSQL();
                return clsSQL.ExecuteQueryStatement(@"
SELECT * FROM tbl_employee
WHERE CompanyId = @CompanyId
  AND ISNULL(IsSystemUser, 0) = 1
  AND LTRIM(RTRIM(ISNULL(AccessCardUid, N''))) = @AccessCardUid
", clsSQL.CreateDataBaseConnectionString(companyId), prm);
            }
            catch (Exception)
            {
                throw;
            }
        }

        /// <summary>
        /// True when the user may perform the POS action (TypeID=4).
        /// Empty POS rights list = allow all (matches Flutter client).
        /// </summary>
        public bool UserHasPOSAction(int userId, int actionId, int companyId)
        {
            try
            {
                if (userId <= 0 || actionId <= 0 || companyId <= 0) return false;

                SqlParameter[] adminPrm =
                {
                    new SqlParameter("@ID", SqlDbType.Int) { Value = userId },
                    new SqlParameter("@CompanyID", SqlDbType.Int) { Value = companyId },
                };
                clsSQL clsSQL = new clsSQL();
                object adminVal = clsSQL.ExecuteScalar(
                    "SELECT TOP 1 ISNULL(IsAdmin,0) FROM tbl_employee WHERE ID=@ID AND (CompanyID=@CompanyID OR @CompanyID=0)",
                    adminPrm,
                    clsSQL.CreateDataBaseConnectionString(companyId),
                    null);
                if (Simulate.Bool(adminVal))
                    return true;

                clsUserAuthorizationModels auth = new clsUserAuthorizationModels();
                DataTable allPos = auth.SelectUserAuthorizationModels(userId, 4, 0, companyId);
                if (allPos == null || allPos.Rows.Count == 0)
                    return true;

                DataTable forAction = auth.SelectUserAuthorizationModels(userId, 4, actionId, companyId);
                if (forAction == null || forAction.Rows.Count == 0)
                    return false;

                if (forAction.Columns.Contains("IsAccess"))
                    return Simulate.Bool(forAction.Rows[0]["IsAccess"]);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public bool UpdateEmployeePassword(int employeeId, string newPassword, int companyId)
        {
            try
            {
                SqlParameter[] prm =
                {
                    new SqlParameter("@ID", SqlDbType.Int) { Value = employeeId },
                    new SqlParameter("@Password", SqlDbType.NVarChar, -1) { Value = newPassword },
                    new SqlParameter("@CompanyId", SqlDbType.Int) { Value = companyId },
                    new SqlParameter("@ModificationDate", SqlDbType.DateTime) { Value = DateTime.Now },
                };
                clsSQL clsSQL = new clsSQL();
                int rows = clsSQL.ExecuteNonQueryStatement(
                    @"update tbl_employee set Password=@Password, ModificationDate=@ModificationDate
where ID=@ID and (CompanyId=@CompanyId or @CompanyId=0)",
                    clsSQL.CreateDataBaseConnectionString(companyId),
                    prm);
                return rows > 0;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public bool DeleteEmployeeByID(int Id,int CompanyID)
        {
            try
            {
                clsSQL clsSQL = new clsSQL();

                SqlParameter[] prm =
                 { new SqlParameter("@Id", SqlDbType.Int) { Value = Id },

                };
                int A = clsSQL.ExecuteNonQueryStatement(@"delete from tbl_employee where (id=@Id  )", clsSQL.CreateDataBaseConnectionString(CompanyID), prm);

                return true;
            }
            catch (Exception)
            {

                throw;
            }


        }
        public int InsertEmployee(string AName, string EName, string UserName, string Password, int CompanyID, int CreationUserId,
          bool IsSystemUser, string Email, string Tel1,
          string EmployeeCode
                                 , string Tel2
                , string Address
                , int CountryID
                , int CityID
                , int NationalityID
                , string NationalNumber
                , string IDNumber
                ,DateTime IDIssueDate
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
                ,int MedicalInsuranceProgramID
            ,int DepartmentID,
            bool IsPOSOnly,

          byte[] Signuture, SqlTransaction trn = null,
            bool IsActive = true,
            bool ShowOnTouchLogin = true,
            string AccessCardUid = "",
            bool IsAdmin = false)
        {
            try
            {
                if (!clsJordanHrValidators.IsValidNationalNumber(NationalNumber, out string natMsg))
                    throw new Exception(natMsg);
                if (!clsJordanHrValidators.IsValidSocialSecurityNumber(SocialSecurityNumber, out string sscMsg))
                    throw new Exception(sscMsg);

                SqlParameter[] prm =
                 { new SqlParameter("@AName", SqlDbType.NVarChar,-1) { Value = AName },
                  new SqlParameter("@EName", SqlDbType.NVarChar,-1) { Value = EName },
                    new SqlParameter("@UserName", SqlDbType.NVarChar,-1) { Value = UserName },
                    new SqlParameter("@Password", SqlDbType.NVarChar,-1) { Value = Password },
                     new SqlParameter("@CompanyID", SqlDbType.Int) { Value = CompanyID },
                       new SqlParameter("@CreationUserId", SqlDbType.Int) { Value = CreationUserId },
                     new SqlParameter("@CreationDate", SqlDbType.DateTime) { Value = DateTime.Now },
                          new SqlParameter("@IsSystemUser", SqlDbType.Bit) { Value = IsSystemUser },
                          new SqlParameter("@IsPOSOnly", SqlDbType.Bit) { Value = IsPOSOnly },
                          new SqlParameter("@IsAdmin", SqlDbType.Bit) { Value = IsAdmin },
                          new SqlParameter("@IsActive", SqlDbType.Bit) { Value = IsActive },
                          new SqlParameter("@ShowOnTouchLogin", SqlDbType.Bit) { Value = ShowOnTouchLogin },
                          new SqlParameter("@AccessCardUid", SqlDbType.NVarChar,-1) { Value = Simulate.String(AccessCardUid).Trim() },
                               new SqlParameter("@Signuture", SqlDbType.Image) { Value = Signuture },
                                    new SqlParameter("@Email", SqlDbType.NVarChar,-1) { Value = Email },
                                         new SqlParameter("@Tel1", SqlDbType.NVarChar,-1) { Value = Tel1 },
 new SqlParameter("@EmployeeCode", SqlDbType.NVarChar,-1) { Value = EmployeeCode },
  new SqlParameter("@Tel2", SqlDbType.NVarChar,-1) { Value = Tel2 },

    new SqlParameter("@Address", SqlDbType.NVarChar,-1) { Value = Address },

    new SqlParameter("@CountryID", SqlDbType.Int) { Value = CountryID },
        new SqlParameter("@CityID", SqlDbType.Int) { Value = CityID },
        new SqlParameter("@NationalityID", SqlDbType.Int) { Value = NationalityID },
            new SqlParameter("@NationalNumber", SqlDbType.NVarChar,-1) { Value = NationalNumber },
                        new SqlParameter("@IDNumber", SqlDbType.NVarChar,-1) { Value = IDNumber },
                        new SqlParameter("@IDIssueDate", SqlDbType.DateTime) { Value = IDIssueDate },
                        new SqlParameter("@IDExpireDate", SqlDbType.DateTime) { Value = IDExpireDate },
            new SqlParameter("@PassportNumber", SqlDbType.NVarChar,-1) { Value = PassportNumber },
               new SqlParameter("@PassportIssueDate", SqlDbType.DateTime) { Value = PassportIssueDate },
                        new SqlParameter("@PassportExpireDate", SqlDbType.DateTime) { Value = PassportExpireDate },
        new SqlParameter("@EducationalLevelID", SqlDbType.Int) { Value = EducationalLevelID },
                                new SqlParameter("@HireDate", SqlDbType.DateTime) { Value = HireDate },
            new SqlParameter("@BankName", SqlDbType.NVarChar,-1) { Value = BankName },

    new SqlParameter("@IBAN", SqlDbType.NVarChar,-1) { Value = IBAN },
    new SqlParameter("@SWIFTCode", SqlDbType.NVarChar,-1) { Value = SWIFTCode },
    new SqlParameter("@BankAccountNumber", SqlDbType.NVarChar,-1) { Value = BankAccountNumber },
     new SqlParameter("@SocialSecurityNumber", SqlDbType.NVarChar,-1) { Value = SocialSecurityNumber },
       new SqlParameter("@SocialSecurityProgramID", SqlDbType.Int) { Value = SocialSecurityProgramID },
         new SqlParameter("@MedicalInsuranceNumber", SqlDbType.NVarChar,-1) { Value = MedicalInsuranceNumber },
    new SqlParameter("@MedicalInsuranceProgramID", SqlDbType.Int) { Value = MedicalInsuranceProgramID },
        new SqlParameter("@DepartmentID", SqlDbType.Int) { Value = DepartmentID },


              







  


                };

                string a = @"insert into tbl_employee(AName,EName,UserName,Password,CompanyID,CreationUserId,CreationDate,
IsSystemUser,IsPOSOnly,IsAdmin,IsActive,ShowOnTouchLogin,AccessCardUid,Email,Tel1,Signuture
,EmployeeCode 
                ,Tel2
                ,Address
                ,CountryID 
                ,CityID 
                ,NationalityID 
                ,NationalNumber 
                ,IDNumber 
                ,IDIssueDate 
                ,IDExpireDate 
                ,PassportNumber 
                ,PassportIssueDate 
                ,PassportExpireDate 
                ,EducationalLevelID 
                ,HireDate 
                ,BankName 
                ,IBAN 
                ,SWIFTCode 
                ,BankAccountNumber 
                ,SocialSecurityNumber 
                ,SocialSecurityProgramID 
                ,MedicalInsuranceNumber 
                ,MedicalInsuranceProgramID 
,DepartmentID
) 
OUTPUT INSERTED.ID values(@AName,@EName,@UserName,@Password,@CompanyID,@CreationUserId,@CreationDate,@IsSystemUser,@IsPOSOnly,@IsAdmin,@IsActive,@ShowOnTouchLogin,@AccessCardUid,@Email,@Tel1,@Signuture
                ,@EmployeeCode 
                ,@Tel2
                ,@Address
                ,@CountryID 
                ,@CityID 
                ,@NationalityID 
                ,@NationalNumber 
                ,@IDNumber 
                ,@IDIssueDate 
                ,@IDExpireDate 
                ,@PassportNumber 
                ,@PassportIssueDate 
                ,@PassportExpireDate 
                ,@EducationalLevelID 
                ,@HireDate 
                ,@BankName 
                ,@IBAN 
                ,@SWIFTCode 
                ,@BankAccountNumber 
                ,@SocialSecurityNumber 
                ,@SocialSecurityProgramID 
                ,@MedicalInsuranceNumber 
                ,@MedicalInsuranceProgramID 
,@DepartmentID

)";







                
                clsSQL clsSQL = new clsSQL();
                if (trn == null)
                {
                    return Simulate.Integer32(clsSQL.ExecuteScalar(a, prm, clsSQL.CreateDataBaseConnectionString(CompanyID)));
                }
                else
                {
                    return Simulate.Integer32(clsSQL.ExecuteScalar(a, prm, clsSQL.CreateDataBaseConnectionString(CompanyID), trn));
                }

            }
            catch (Exception)
            {

                throw;
            }


        }
        public int UpdateEmployee(string AName, string EName, string UserName, string Password, int ID
            , int ModificationUserId,bool IsSystemUser,String Email,String Tel1, byte[] Signuture,int CompanyID, string EmployeeCode
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
                , int MedicalInsuranceProgramID,int DepartmentID, bool IsPOSOnly,
            bool IsActive = true, bool ShowOnTouchLogin = true, string AccessCardUid = "", bool IsAdmin = false)
        {
            try
            {
                if (!clsJordanHrValidators.IsValidNationalNumber(NationalNumber, out string natMsg))
                    throw new Exception(natMsg);
                if (!clsJordanHrValidators.IsValidSocialSecurityNumber(SocialSecurityNumber, out string sscMsg))
                    throw new Exception(sscMsg);

                clsSQL clsSQL = new clsSQL();

                SqlParameter[] prm =
                 { new SqlParameter("@AName", SqlDbType.NVarChar,-1) { Value = AName },
                  new SqlParameter("@EName", SqlDbType.NVarChar,-1) { Value = EName },
                    new SqlParameter("@UserName", SqlDbType.NVarChar,-1) { Value = UserName },
                    new SqlParameter("@Password", SqlDbType.NVarChar,-1) { Value = Password },
                     new SqlParameter("@ID", SqlDbType.Int) { Value = ID },
                           new SqlParameter("@ModificationUserId", SqlDbType.Int) { Value = ModificationUserId },
                     new SqlParameter("@ModificationDate", SqlDbType.DateTime) { Value = DateTime.Now },
                            new SqlParameter("@IsSystemUser", SqlDbType.Bit) { Value = IsSystemUser },
                            new SqlParameter("@IsPOSOnly", SqlDbType.Bit) { Value = IsPOSOnly },
                            new SqlParameter("@IsAdmin", SqlDbType.Bit) { Value = IsAdmin },
                            new SqlParameter("@IsActive", SqlDbType.Bit) { Value = IsActive },
                            new SqlParameter("@ShowOnTouchLogin", SqlDbType.Bit) { Value = ShowOnTouchLogin },
                            new SqlParameter("@AccessCardUid", SqlDbType.NVarChar,-1) { Value = Simulate.String(AccessCardUid).Trim() },
                               new SqlParameter("@Signuture", SqlDbType.Image) { Value = Signuture },
                                   new SqlParameter("@Email", SqlDbType.NVarChar,-1) { Value = Email },
                                       new SqlParameter("@Tel1", SqlDbType.NVarChar,-1) { Value = Tel1 },
                                        new SqlParameter("@EmployeeCode", SqlDbType.NVarChar,-1) { Value = EmployeeCode },
  new SqlParameter("@Tel2", SqlDbType.NVarChar,-1) { Value = Tel2 },

    new SqlParameter("@Address", SqlDbType.NVarChar,-1) { Value = Address },

    new SqlParameter("@CountryID", SqlDbType.Int) { Value = CountryID },
        new SqlParameter("@CityID", SqlDbType.Int) { Value = CityID },
        new SqlParameter("@NationalityID", SqlDbType.Int) { Value = NationalityID },
            new SqlParameter("@NationalNumber", SqlDbType.NVarChar,-1) { Value = NationalNumber },
                        new SqlParameter("@IDNumber", SqlDbType.NVarChar,-1) { Value = IDNumber },
                        new SqlParameter("@IDIssueDate", SqlDbType.DateTime) { Value = IDIssueDate },
                        new SqlParameter("@IDExpireDate", SqlDbType.DateTime) { Value = IDExpireDate },
            new SqlParameter("@PassportNumber", SqlDbType.NVarChar,-1) { Value = PassportNumber },
               new SqlParameter("@PassportIssueDate", SqlDbType.DateTime) { Value = PassportIssueDate },
                        new SqlParameter("@PassportExpireDate", SqlDbType.DateTime) { Value = PassportExpireDate },
        new SqlParameter("@EducationalLevelID", SqlDbType.Int) { Value = EducationalLevelID },
                                new SqlParameter("@HireDate", SqlDbType.DateTime) { Value = HireDate },
            new SqlParameter("@BankName", SqlDbType.NVarChar,-1) { Value = BankName },

    new SqlParameter("@IBAN", SqlDbType.NVarChar,-1) { Value = IBAN },
    new SqlParameter("@SWIFTCode", SqlDbType.NVarChar,-1) { Value = SWIFTCode },
    new SqlParameter("@BankAccountNumber", SqlDbType.NVarChar,-1) { Value = BankAccountNumber },
     new SqlParameter("@SocialSecurityNumber", SqlDbType.NVarChar,-1) { Value = SocialSecurityNumber },
       new SqlParameter("@SocialSecurityProgramID", SqlDbType.Int) { Value = SocialSecurityProgramID },
         new SqlParameter("@MedicalInsuranceNumber", SqlDbType.NVarChar,-1) { Value = MedicalInsuranceNumber },
    new SqlParameter("@MedicalInsuranceProgramID", SqlDbType.Int) { Value = MedicalInsuranceProgramID },
     new SqlParameter("@DepartmentID", SqlDbType.Int) { Value = DepartmentID },

    
                };
                int A = clsSQL.ExecuteNonQueryStatement(@"update tbl_employee set AName=@AName,EName=@EName,
UserName=@UserName,Password=@Password
,ModificationDate=@ModificationDate
,ModificationUserId=@ModificationUserId 
,Signuture=@Signuture
,IsSystemUser=@IsSystemUser
,IsPOSOnly=@IsPOSOnly
,IsAdmin=@IsAdmin
,IsActive=@IsActive
,ShowOnTouchLogin=@ShowOnTouchLogin
,AccessCardUid=@AccessCardUid
,Email=@Email 
,Tel1=@Tel1 

  ,EmployeeCode =@EmployeeCode 
  ,Tel2=@Tel2
  ,Address=@Address
  ,CountryID =@CountryID 
  ,CityID =@CityID 
  ,NationalityID =@NationalityID 
  ,NationalNumber =@NationalNumber 
  ,IDNumber =@IDNumber 
  ,IDIssueDate =@IDIssueDate 
  ,IDExpireDate =@IDExpireDate 
  ,PassportNumber =@PassportNumber 
  ,PassportIssueDate =@PassportIssueDate 
  ,PassportExpireDate =@PassportExpireDate 
  ,EducationalLevelID =@EducationalLevelID 
  ,HireDate =@HireDate 
  ,BankName =@BankName 
  ,IBAN =@IBAN 
  ,SWIFTCode =@SWIFTCode 
  ,BankAccountNumber =@BankAccountNumber 
  ,SocialSecurityNumber =@SocialSecurityNumber 
  ,SocialSecurityProgramID =@SocialSecurityProgramID 
  ,MedicalInsuranceNumber =@MedicalInsuranceNumber 
  ,MedicalInsuranceProgramID =@MedicalInsuranceProgramID 
,DepartmentID=@DepartmentID

where id =@id", clsSQL.CreateDataBaseConnectionString(CompanyID), prm);
                 
                return A;
            }
            catch (Exception)
            {

                throw;
            }


        }

        public int UpdateEmployeeReportsTo(int employeeId, int reportsToEmployeeId, int companyId, int userId)
        {
            if (employeeId <= 0) return 0;
            if (reportsToEmployeeId == employeeId) reportsToEmployeeId = 0;

            SqlParameter[] prm =
            {
                new SqlParameter("@ID", SqlDbType.Int) { Value = employeeId },
                new SqlParameter("@ReportsToEmployeeID", SqlDbType.Int) { Value = reportsToEmployeeId },
                new SqlParameter("@CompanyID", SqlDbType.Int) { Value = companyId },
                new SqlParameter("@UserID", SqlDbType.Int) { Value = userId },
            };
            clsSQL clsSQL = new clsSQL();
            return clsSQL.ExecuteNonQueryStatement(@"
UPDATE tbl_employee SET
  ReportsToEmployeeID = @ReportsToEmployeeID,
  ModificationUserId = @UserID,
  ModificationDate = GETDATE()
WHERE ID = @ID AND CompanyID = @CompanyID",
                clsSQL.CreateDataBaseConnectionString(companyId), prm);
        }
    }
}
