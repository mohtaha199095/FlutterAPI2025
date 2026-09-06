using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Newtonsoft.Json;
using System;
using System.Data;
using WebApplication2.cls;

namespace WebApplication2.Controllers
{
    [Route("api/ctlPayrollStatutory")]
    public class ctlPayrollStatutory : Controller
    {
        [HttpGet]
        [Route("GetCountryPack")]
        public IActionResult GetCountryPack(int CompanyID)
        {
            try
            {
                clsSQL sql = new clsSQL();
                SqlParameter[] prm = { new SqlParameter("@ID", SqlDbType.Int) { Value = CompanyID } };
                object val = sql.ExecuteScalar(
                    "SELECT TOP 1 ISNULL(PayrollCountryPack, N'JO') FROM tbl_Company WHERE ID=@ID",
                    prm, sql.MainDataBaseconString, null);
                string pack = Simulate.String(val);
                if (string.IsNullOrWhiteSpace(pack)) pack = "JO";
                return Ok(new { countryPack = pack });
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        [HttpPost]
        [Route("UpdateCountryPack")]
        public IActionResult UpdateCountryPack(int CompanyID, string CountryPack)
        {
            try
            {
                string pack = Simulate.String(CountryPack);
                if (string.IsNullOrWhiteSpace(pack)) pack = "JO";
                pack = pack.Trim().ToUpperInvariant();

                clsSQL sql = new clsSQL();
                SqlParameter[] prm =
                {
                    new SqlParameter("@ID", SqlDbType.Int) { Value = CompanyID },
                    new SqlParameter("@Pack", SqlDbType.NVarChar, 10) { Value = pack },
                };
                sql.ExecuteNonQueryStatement(
                    "UPDATE tbl_Company SET PayrollCountryPack=@Pack WHERE ID=@ID",
                    sql.MainDataBaseconString, prm);
                return Ok(new { success = true, countryPack = pack });
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        // ---------- Statutory rates ----------
        [HttpGet]
        [Route("SelectStatutoryRates")]
        public string SelectStatutoryRates(int ID = 0, string CountryPack = "", int CompanyID = 0)
        {
            clsSQL sql = new clsSQL();
            SqlParameter[] prm =
            {
                new SqlParameter("@ID", SqlDbType.Int) { Value = ID },
                new SqlParameter("@CountryPack", SqlDbType.NVarChar, 10) { Value = CountryPack ?? "" },
                new SqlParameter("@CompanyID", SqlDbType.Int) { Value = CompanyID },
            };
            DataTable dt = sql.ExecuteQueryStatement(@"
SELECT * FROM tbl_StatutoryRate
WHERE (ID=@ID OR @ID=0)
  AND (CountryPack=@CountryPack OR @CountryPack='')
  AND (CompanyID=@CompanyID OR @CompanyID=0)
ORDER BY CountryPack, EffectiveFrom DESC",
                sql.CreateDataBaseConnectionString(CompanyID), prm);
            return dt == null ? "[]" : JsonConvert.SerializeObject(dt);
        }

        [HttpPost]
        [Route("InsertStatutoryRate")]
        public int InsertStatutoryRate(string CountryPack, DateTime EffectiveFrom,
            decimal EmployeePercent, decimal EmployerPercent, decimal WageCeiling, decimal MinSubjectWage,
            bool IsActive, int CompanyID, int CreationUserID)
        {
            string pack = Simulate.String(CountryPack).Trim().ToUpperInvariant();
            if (MinSubjectWage <= 0 && pack == "JO") MinSubjectWage = 260m;

            clsSQL sql = new clsSQL();
            SqlParameter[] prm =
            {
                new SqlParameter("@CountryPack", SqlDbType.NVarChar, 10) { Value = Simulate.String(CountryPack) },
                new SqlParameter("@EffectiveFrom", SqlDbType.DateTime) { Value = EffectiveFrom },
                new SqlParameter("@EmployeePercent", SqlDbType.Decimal) { Value = EmployeePercent },
                new SqlParameter("@EmployerPercent", SqlDbType.Decimal) { Value = EmployerPercent },
                new SqlParameter("@WageCeiling", SqlDbType.Decimal) { Value = WageCeiling },
                new SqlParameter("@MinSubjectWage", SqlDbType.Decimal) { Value = MinSubjectWage },
                new SqlParameter("@IsActive", SqlDbType.Bit) { Value = IsActive },
                new SqlParameter("@CompanyID", SqlDbType.Int) { Value = CompanyID },
                new SqlParameter("@CreationUserID", SqlDbType.Int) { Value = CreationUserID },
            };
            return Simulate.Integer32(sql.ExecuteScalar(@"
INSERT INTO tbl_StatutoryRate
  (CountryPack, EffectiveFrom, EmployeePercent, EmployerPercent, WageCeiling, MinSubjectWage, IsActive, CompanyID, CreationUserID, CreationDate)
OUTPUT INSERTED.ID
VALUES (@CountryPack, @EffectiveFrom, @EmployeePercent, @EmployerPercent, @WageCeiling, @MinSubjectWage, @IsActive, @CompanyID, @CreationUserID, GETDATE())",
                prm, sql.CreateDataBaseConnectionString(CompanyID)));
        }

        [HttpPost]
        [Route("UpdateStatutoryRate")]
        public int UpdateStatutoryRate(int ID, string CountryPack, DateTime EffectiveFrom,
            decimal EmployeePercent, decimal EmployerPercent, decimal WageCeiling, decimal MinSubjectWage,
            bool IsActive, int CompanyID, int ModificationUserID)
        {
            string pack = Simulate.String(CountryPack).Trim().ToUpperInvariant();
            if (MinSubjectWage <= 0 && pack == "JO") MinSubjectWage = 260m;

            clsSQL sql = new clsSQL();
            SqlParameter[] prm =
            {
                new SqlParameter("@ID", SqlDbType.Int) { Value = ID },
                new SqlParameter("@CountryPack", SqlDbType.NVarChar, 10) { Value = Simulate.String(CountryPack) },
                new SqlParameter("@EffectiveFrom", SqlDbType.DateTime) { Value = EffectiveFrom },
                new SqlParameter("@EmployeePercent", SqlDbType.Decimal) { Value = EmployeePercent },
                new SqlParameter("@EmployerPercent", SqlDbType.Decimal) { Value = EmployerPercent },
                new SqlParameter("@WageCeiling", SqlDbType.Decimal) { Value = WageCeiling },
                new SqlParameter("@MinSubjectWage", SqlDbType.Decimal) { Value = MinSubjectWage },
                new SqlParameter("@IsActive", SqlDbType.Bit) { Value = IsActive },
                new SqlParameter("@CompanyID", SqlDbType.Int) { Value = CompanyID },
                new SqlParameter("@ModificationUserID", SqlDbType.Int) { Value = ModificationUserID },
            };
            return sql.ExecuteNonQueryStatement(@"
UPDATE tbl_StatutoryRate SET
  CountryPack=@CountryPack, EffectiveFrom=@EffectiveFrom,
  EmployeePercent=@EmployeePercent, EmployerPercent=@EmployerPercent,
  WageCeiling=@WageCeiling, MinSubjectWage=@MinSubjectWage, IsActive=@IsActive,
  ModificationUserID=@ModificationUserID, ModificationDate=GETDATE()
WHERE ID=@ID AND CompanyID=@CompanyID",
                sql.CreateDataBaseConnectionString(CompanyID), prm);
        }

        [HttpPost]
        [Route("DeleteStatutoryRate")]
        public bool DeleteStatutoryRate(int ID, int CompanyID)
        {
            clsSQL sql = new clsSQL();
            SqlParameter[] prm =
            {
                new SqlParameter("@ID", SqlDbType.Int) { Value = ID },
                new SqlParameter("@CompanyID", SqlDbType.Int) { Value = CompanyID },
            };
            sql.ExecuteNonQueryStatement(
                "DELETE FROM tbl_StatutoryRate WHERE ID=@ID AND CompanyID=@CompanyID",
                sql.CreateDataBaseConnectionString(CompanyID), prm);
            return true;
        }

        // ---------- Tax brackets ----------
        [HttpGet]
        [Route("SelectIncomeTaxBrackets")]
        public string SelectIncomeTaxBrackets(int ID = 0, string CountryPack = "", int CompanyID = 0)
        {
            clsSQL sql = new clsSQL();
            SqlParameter[] prm =
            {
                new SqlParameter("@ID", SqlDbType.Int) { Value = ID },
                new SqlParameter("@CountryPack", SqlDbType.NVarChar, 10) { Value = CountryPack ?? "" },
                new SqlParameter("@CompanyID", SqlDbType.Int) { Value = CompanyID },
            };
            DataTable dt = sql.ExecuteQueryStatement(@"
SELECT * FROM tbl_IncomeTaxBracket
WHERE (ID=@ID OR @ID=0)
  AND (CountryPack=@CountryPack OR @CountryPack='')
  AND (CompanyID=@CompanyID OR @CompanyID=0)
ORDER BY CountryPack, EffectiveFrom DESC, FromAmount",
                sql.CreateDataBaseConnectionString(CompanyID), prm);
            return dt == null ? "[]" : JsonConvert.SerializeObject(dt);
        }

        [HttpPost]
        [Route("InsertIncomeTaxBracket")]
        public int InsertIncomeTaxBracket(string CountryPack, DateTime EffectiveFrom,
            decimal FromAmount, decimal ToAmount, decimal RatePercent, decimal PersonalExemption,
            int CompanyID, int CreationUserID)
        {
            clsSQL sql = new clsSQL();
            SqlParameter[] prm =
            {
                new SqlParameter("@CountryPack", SqlDbType.NVarChar, 10) { Value = Simulate.String(CountryPack) },
                new SqlParameter("@EffectiveFrom", SqlDbType.DateTime) { Value = EffectiveFrom },
                new SqlParameter("@FromAmount", SqlDbType.Decimal) { Value = FromAmount },
                new SqlParameter("@ToAmount", SqlDbType.Decimal) { Value = ToAmount },
                new SqlParameter("@RatePercent", SqlDbType.Decimal) { Value = RatePercent },
                new SqlParameter("@PersonalExemption", SqlDbType.Decimal) { Value = PersonalExemption },
                new SqlParameter("@CompanyID", SqlDbType.Int) { Value = CompanyID },
                new SqlParameter("@CreationUserID", SqlDbType.Int) { Value = CreationUserID },
            };
            return Simulate.Integer32(sql.ExecuteScalar(@"
INSERT INTO tbl_IncomeTaxBracket
  (CountryPack, EffectiveFrom, FromAmount, ToAmount, RatePercent, PersonalExemption, CompanyID, CreationUserID, CreationDate)
OUTPUT INSERTED.ID
VALUES (@CountryPack, @EffectiveFrom, @FromAmount, @ToAmount, @RatePercent, @PersonalExemption, @CompanyID, @CreationUserID, GETDATE())",
                prm, sql.CreateDataBaseConnectionString(CompanyID)));
        }

        [HttpPost]
        [Route("UpdateIncomeTaxBracket")]
        public int UpdateIncomeTaxBracket(int ID, string CountryPack, DateTime EffectiveFrom,
            decimal FromAmount, decimal ToAmount, decimal RatePercent, decimal PersonalExemption,
            int CompanyID, int ModificationUserID)
        {
            clsSQL sql = new clsSQL();
            SqlParameter[] prm =
            {
                new SqlParameter("@ID", SqlDbType.Int) { Value = ID },
                new SqlParameter("@CountryPack", SqlDbType.NVarChar, 10) { Value = Simulate.String(CountryPack) },
                new SqlParameter("@EffectiveFrom", SqlDbType.DateTime) { Value = EffectiveFrom },
                new SqlParameter("@FromAmount", SqlDbType.Decimal) { Value = FromAmount },
                new SqlParameter("@ToAmount", SqlDbType.Decimal) { Value = ToAmount },
                new SqlParameter("@RatePercent", SqlDbType.Decimal) { Value = RatePercent },
                new SqlParameter("@PersonalExemption", SqlDbType.Decimal) { Value = PersonalExemption },
                new SqlParameter("@CompanyID", SqlDbType.Int) { Value = CompanyID },
                new SqlParameter("@ModificationUserID", SqlDbType.Int) { Value = ModificationUserID },
            };
            return sql.ExecuteNonQueryStatement(@"
UPDATE tbl_IncomeTaxBracket SET
  CountryPack=@CountryPack, EffectiveFrom=@EffectiveFrom,
  FromAmount=@FromAmount, ToAmount=@ToAmount, RatePercent=@RatePercent,
  PersonalExemption=@PersonalExemption,
  ModificationUserID=@ModificationUserID, ModificationDate=GETDATE()
WHERE ID=@ID AND CompanyID=@CompanyID",
                sql.CreateDataBaseConnectionString(CompanyID), prm);
        }

        [HttpPost]
        [Route("DeleteIncomeTaxBracket")]
        public bool DeleteIncomeTaxBracket(int ID, int CompanyID)
        {
            clsSQL sql = new clsSQL();
            SqlParameter[] prm =
            {
                new SqlParameter("@ID", SqlDbType.Int) { Value = ID },
                new SqlParameter("@CompanyID", SqlDbType.Int) { Value = CompanyID },
            };
            sql.ExecuteNonQueryStatement(
                "DELETE FROM tbl_IncomeTaxBracket WHERE ID=@ID AND CompanyID=@CompanyID",
                sql.CreateDataBaseConnectionString(CompanyID), prm);
            return true;
        }

        // ---------- Social security programs ----------
        [HttpGet]
        [Route("SelectSocialSecurityPrograms")]
        public string SelectSocialSecurityPrograms(int ID = 0, string Code = "", int CompanyID = 0)
        {
            clsSQL sql = new clsSQL();
            SqlParameter[] prm =
            {
                new SqlParameter("@ID", SqlDbType.Int) { Value = ID },
                new SqlParameter("@Code", SqlDbType.NVarChar, 50) { Value = Code ?? "" },
                new SqlParameter("@CompanyID", SqlDbType.Int) { Value = CompanyID },
            };
            DataTable dt = sql.ExecuteQueryStatement(@"
SELECT * FROM tbl_SocialSecurityProgram
WHERE (ID=@ID OR @ID=0)
  AND (Code=@Code OR @Code='')
  AND (CompanyID=@CompanyID OR @CompanyID=0)
ORDER BY Code",
                sql.CreateDataBaseConnectionString(CompanyID), prm);
            return dt == null ? "[]" : JsonConvert.SerializeObject(dt);
        }

        [HttpPost]
        [Route("InsertSocialSecurityProgram")]
        public int InsertSocialSecurityProgram(string AName, string EName, string Code, int CompanyID, int CreationUserID)
        {
            clsSQL sql = new clsSQL();
            SqlParameter[] prm =
            {
                new SqlParameter("@AName", SqlDbType.NVarChar, -1) { Value = Simulate.String(AName) },
                new SqlParameter("@EName", SqlDbType.NVarChar, -1) { Value = Simulate.String(EName) },
                new SqlParameter("@Code", SqlDbType.NVarChar, 50) { Value = Simulate.String(Code) },
                new SqlParameter("@CompanyID", SqlDbType.Int) { Value = CompanyID },
                new SqlParameter("@CreationUserID", SqlDbType.Int) { Value = CreationUserID },
            };
            return Simulate.Integer32(sql.ExecuteScalar(@"
INSERT INTO tbl_SocialSecurityProgram (AName, EName, Code, CompanyID, CreationUserID, CreationDate)
OUTPUT INSERTED.ID
VALUES (@AName, @EName, @Code, @CompanyID, @CreationUserID, GETDATE())",
                prm, sql.CreateDataBaseConnectionString(CompanyID)));
        }

        [HttpPost]
        [Route("UpdateSocialSecurityProgram")]
        public int UpdateSocialSecurityProgram(int ID, string AName, string EName, string Code, int CompanyID, int ModificationUserID)
        {
            clsSQL sql = new clsSQL();
            SqlParameter[] prm =
            {
                new SqlParameter("@ID", SqlDbType.Int) { Value = ID },
                new SqlParameter("@AName", SqlDbType.NVarChar, -1) { Value = Simulate.String(AName) },
                new SqlParameter("@EName", SqlDbType.NVarChar, -1) { Value = Simulate.String(EName) },
                new SqlParameter("@Code", SqlDbType.NVarChar, 50) { Value = Simulate.String(Code) },
                new SqlParameter("@CompanyID", SqlDbType.Int) { Value = CompanyID },
                new SqlParameter("@ModificationUserID", SqlDbType.Int) { Value = ModificationUserID },
            };
            return sql.ExecuteNonQueryStatement(@"
UPDATE tbl_SocialSecurityProgram SET
  AName=@AName, EName=@EName, Code=@Code,
  ModificationUserID=@ModificationUserID, ModificationDate=GETDATE()
WHERE ID=@ID AND CompanyID=@CompanyID",
                sql.CreateDataBaseConnectionString(CompanyID), prm);
        }

        [HttpPost]
        [Route("DeleteSocialSecurityProgram")]
        public bool DeleteSocialSecurityProgram(int ID, int CompanyID)
        {
            clsSQL sql = new clsSQL();
            SqlParameter[] prm =
            {
                new SqlParameter("@ID", SqlDbType.Int) { Value = ID },
                new SqlParameter("@CompanyID", SqlDbType.Int) { Value = CompanyID },
            };
            sql.ExecuteNonQueryStatement(
                "DELETE FROM tbl_SocialSecurityProgram WHERE ID=@ID AND CompanyID=@CompanyID",
                sql.CreateDataBaseConnectionString(CompanyID), prm);
            return true;
        }

        [HttpGet]
        [Route("GetMaxDailyOvertimeMinutes")]
        public IActionResult GetMaxDailyOvertimeMinutes(int CompanyID)
        {
            try
            {
                int minutes = new clsHrReports().GetMaxDailyOvertimeMinutes(CompanyID);
                return Ok(new { maxDailyOvertimeMinutes = minutes });
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        [HttpPost]
        [Route("UpdateMaxDailyOvertimeMinutes")]
        public IActionResult UpdateMaxDailyOvertimeMinutes(int CompanyID, int MaxDailyOvertimeMinutes)
        {
            try
            {
                new clsHrReports().UpdateMaxDailyOvertimeMinutes(CompanyID, MaxDailyOvertimeMinutes);
                return Ok(new { success = true });
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }
    }
}
