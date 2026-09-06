using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using WebApplication2.cls;

namespace WebApplication2.Controllers
{
    [Route("api/ctlBudget")]
    public class ctlBudget : Controller
    {
        [HttpGet]
        [Route("SelectHeaders")]
        public string SelectHeaders(int ID = 0, int CompanyID = 0, int FiscalYear = 0, int DocumentStatus = -1, string Guid = "")
        {
            DataTable dt = new clsBudget().SelectHeaders(ID, CompanyID, FiscalYear, DocumentStatus, Guid);
            return dt == null ? "[]" : JsonConvert.SerializeObject(dt);
        }

        [HttpGet]
        [Route("SelectLines")]
        public string SelectLines(int BudgetHeaderID = 0, int CompanyID = 0)
        {
            DataTable dt = new clsBudget().SelectLines(BudgetHeaderID, CompanyID);
            return dt == null ? "[]" : JsonConvert.SerializeObject(dt);
        }

        [HttpPost]
        [Route("InsertHeader")]
        public IActionResult InsertHeader(int FiscalYear, string AName, string EName, int BranchID = 0,
            string Notes = "", int CompanyID = 0, int CreationUserID = 0)
        {
            try
            {
                int status = new clsApprovalEngine().ResolveInitialDocumentStatus(
                    CompanyID, clsBudget.TypeBudget, BranchID, 0);
                // Budget should never auto-post without approval when policy exists;
                // Resolve returns Posted when no policy — keep as Draft for explicit submit.
                if (status == (int)MainClasses.clsEnum.DocumentStatus.Posted)
                    status = (int)MainClasses.clsEnum.DocumentStatus.Draft;

                int id = new clsBudget().InsertHeader(
                    FiscalYear, AName, EName, BranchID, Notes, CompanyID, CreationUserID, status);
                string guid = new clsBudget().GetGuidById(id, CompanyID);
                return Ok(new { ID = id, Guid = guid, DocumentStatus = status });
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        [HttpPost]
        [Route("UpdateHeader")]
        public IActionResult UpdateHeader(int ID, int FiscalYear, string AName, string EName, int BranchID = 0,
            string Notes = "", int CompanyID = 0, int ModificationUserID = 0)
        {
            try
            {
                bool ok = new clsBudget().UpdateHeader(
                    ID, FiscalYear, AName, EName, BranchID, Notes, CompanyID, ModificationUserID);
                return Ok(ok);
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        [HttpPost]
        [Route("DeleteHeader")]
        public IActionResult DeleteHeader(int ID, int CompanyID = 0)
        {
            try
            {
                return Ok(new clsBudget().DeleteHeader(ID, CompanyID));
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        [HttpPost]
        [Route("ReplaceLines")]
        public IActionResult ReplaceLines(int BudgetHeaderID, int CompanyID, int UserID, [FromBody] string LinesJson)
        {
            try
            {
                var lines = string.IsNullOrWhiteSpace(LinesJson)
                    ? new List<BudgetLineDto>()
                    : JsonConvert.DeserializeObject<List<BudgetLineDto>>(LinesJson);
                bool ok = new clsBudget().ReplaceLines(BudgetHeaderID, CompanyID, UserID, lines);
                return Ok(ok);
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        [HttpPost]
        [Route("Evaluate")]
        public IActionResult Evaluate(int CompanyID, DateTime VoucherDate, [FromBody] string LinesJson,
            int BranchID = 0, int CostCenterID = 0, string ExcludeDocumentGuid = "")
        {
            try
            {
                var lines = string.IsNullOrWhiteSpace(LinesJson)
                    ? new List<BudgetSpendLine>()
                    : JsonConvert.DeserializeObject<List<BudgetSpendLine>>(LinesJson);
                var result = new clsBudgetControl().Evaluate(
                    CompanyID, VoucherDate, BranchID, CostCenterID, lines, ExcludeDocumentGuid);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        [HttpGet]
        [Route("GetBudgetVsActual")]
        public string GetBudgetVsActual(int CompanyID, int FiscalYear, int AccountID = 0, int CostCenterID = -1, int BranchID = -1)
        {
            DataTable dt = new clsBudgetControl().GetBudgetVsActual(CompanyID, FiscalYear, AccountID, CostCenterID, BranchID);
            return dt == null ? "[]" : JsonConvert.SerializeObject(dt);
        }

        [HttpGet]
        [Route("SelectOverrideLog")]
        public string SelectOverrideLog(int CompanyID, int Year = 0, int DocumentTypeId = 0)
        {
            DataTable dt = new clsBudget().SelectOverrideLog(CompanyID, Year, DocumentTypeId);
            return dt == null ? "[]" : JsonConvert.SerializeObject(dt);
        }

        [HttpGet]
        [Route("GetSettings")]
        public string GetSettings(int CompanyID)
        {
            DataTable dt = new clsBudget().GetSettings(CompanyID);
            return dt == null ? "[]" : JsonConvert.SerializeObject(dt);
        }

        [HttpPost]
        [Route("SaveSettings")]
        public IActionResult SaveSettings(int CompanyID, bool IsEnabled, int UserID)
        {
            try
            {
                return Ok(new clsBudget().SaveSettings(CompanyID, IsEnabled, UserID));
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }
    }
}
