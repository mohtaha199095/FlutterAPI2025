using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System;
using System.Data;
using WebApplication2.cls;

namespace WebApplication2.Controllers
{
    [Route("api/ctlFixedAssets")]
    public class ctlFixedAssets : Controller
    {
        // ---------- Categories ----------
        [HttpGet]
        [Route("SelectCategories")]
        public string SelectCategories(int ID = 0, int CompanyID = 0, int ActiveOnly = 0)
        {
            DataTable dt = new clsFixedAssets().SelectCategories(ID, CompanyID, ActiveOnly);
            return dt == null ? "[]" : JsonConvert.SerializeObject(dt);
        }

        [HttpPost]
        [Route("InsertCategory")]
        public IActionResult InsertCategory(string Code, string Name, int DefaultUsefulLifeMonths = 60,
            int DefaultDepreciationMethod = 1, decimal DefaultDecliningRate = 0,
            int AssetAccountID = 0, int AccumDepAccountID = 0, int DepExpenseAccountID = 0,
            bool Active = true, int CompanyID = 0, int CreationUserID = 0)
        {
            try
            {
                int id = new clsFixedAssets().InsertCategory(
                    Code, Name, DefaultUsefulLifeMonths, DefaultDepreciationMethod, DefaultDecliningRate,
                    AssetAccountID, AccumDepAccountID, DepExpenseAccountID, Active, CompanyID, CreationUserID);
                return Ok(id);
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        [HttpPost]
        [Route("UpdateCategory")]
        public IActionResult UpdateCategory(int ID, string Code, string Name, int DefaultUsefulLifeMonths = 60,
            int DefaultDepreciationMethod = 1, decimal DefaultDecliningRate = 0,
            int AssetAccountID = 0, int AccumDepAccountID = 0, int DepExpenseAccountID = 0,
            bool Active = true, int CompanyID = 0, int ModificationUserID = 0)
        {
            try
            {
                int n = new clsFixedAssets().UpdateCategory(
                    ID, Code, Name, DefaultUsefulLifeMonths, DefaultDepreciationMethod, DefaultDecliningRate,
                    AssetAccountID, AccumDepAccountID, DepExpenseAccountID, Active, CompanyID, ModificationUserID);
                return Ok(n);
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        [HttpPost]
        [Route("DeleteCategory")]
        public IActionResult DeleteCategory(int ID, int CompanyID = 0)
        {
            try
            {
                bool ok = new clsFixedAssets().DeleteCategory(ID, CompanyID);
                return Ok(ok);
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        // ---------- Assets ----------
        [HttpGet]
        [Route("SelectAssets")]
        public string SelectAssets(int ID = 0, int CompanyID = 0, int Status = -1, int CategoryID = 0, string Guid = "")
        {
            DataTable dt = new clsFixedAssets().SelectAssets(ID, CompanyID, Status, CategoryID, Guid);
            return dt == null ? "[]" : JsonConvert.SerializeObject(dt);
        }

        [HttpPost]
        [Route("InsertAsset")]
        public IActionResult InsertAsset(string AssetCode, string Name, int CategoryID = 0, int BranchID = 0,
            int CostCenterID = 0, decimal AcquisitionCost = 0, decimal SalvageValue = 0,
            int UsefulLifeMonths = 60, int DepreciationMethod = 1, decimal DecliningRate = 0,
            DateTime? InServiceDate = null, int Status = 1, string Notes = "",
            int CompanyID = 0, int CreationUserID = 0)
        {
            try
            {
                int id = new clsFixedAssets().InsertAsset(
                    AssetCode, Name, CategoryID, BranchID, CostCenterID,
                    AcquisitionCost, SalvageValue, UsefulLifeMonths, DepreciationMethod, DecliningRate,
                    InServiceDate ?? DateTime.Today, Status, Notes, CompanyID, CreationUserID);
                return Ok(id);
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        [HttpPost]
        [Route("UpdateAsset")]
        public IActionResult UpdateAsset(int ID, string AssetCode, string Name, int CategoryID = 0, int BranchID = 0,
            int CostCenterID = 0, decimal AcquisitionCost = 0, decimal SalvageValue = 0,
            int UsefulLifeMonths = 60, int DepreciationMethod = 1, decimal DecliningRate = 0,
            DateTime? InServiceDate = null, int Status = 1, string Notes = "",
            int CompanyID = 0, int ModificationUserID = 0)
        {
            try
            {
                int n = new clsFixedAssets().UpdateAsset(
                    ID, AssetCode, Name, CategoryID, BranchID, CostCenterID,
                    AcquisitionCost, SalvageValue, UsefulLifeMonths, DepreciationMethod, DecliningRate,
                    InServiceDate ?? DateTime.Today, Status, Notes, CompanyID, ModificationUserID);
                return Ok(n);
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        [HttpPost]
        [Route("DeleteAsset")]
        public IActionResult DeleteAsset(int ID, int CompanyID = 0)
        {
            try
            {
                bool ok = new clsFixedAssets().DeleteAsset(ID, CompanyID);
                return Ok(ok);
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        // ---------- Capitalize ----------
        [HttpGet]
        [Route("SelectPostedPurchaseInvoiceLines")]
        public string SelectPostedPurchaseInvoiceLines(int CompanyID = 0, int HeaderID = 0)
        {
            DataTable dt = new clsFixedAssets().SelectPostedPurchaseInvoiceLines(CompanyID, HeaderID);
            return dt == null ? "[]" : JsonConvert.SerializeObject(dt);
        }

        [HttpPost]
        [Route("CapitalizeFromInvoice")]
        public IActionResult CapitalizeFromInvoice(string InvoiceDetailsGuid, string AssetCode = "", string Name = "",
            int CategoryID = 0, int BranchID = 0, int CostCenterID = 0, decimal SalvageValue = 0,
            int UsefulLifeMonths = 0, int DepreciationMethod = 0, decimal DecliningRate = 0,
            DateTime? InServiceDate = null, string Notes = "", int CompanyID = 0, int CreationUserID = 0)
        {
            try
            {
                int id = new clsFixedAssets().CapitalizeFromInvoice(
                    InvoiceDetailsGuid, AssetCode, Name, CategoryID, BranchID, CostCenterID,
                    SalvageValue, UsefulLifeMonths, DepreciationMethod, DecliningRate,
                    InServiceDate, Notes, CompanyID, CreationUserID);
                return Ok(id);
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        // ---------- Depreciation ----------
        [HttpGet]
        [Route("PreviewDepreciation")]
        public IActionResult PreviewDepreciation(string Period, int CompanyID = 0)
        {
            try
            {
                DataTable dt = new clsFixedAssets().PreviewDepreciation(Period, CompanyID);
                return Content(dt == null ? "[]" : JsonConvert.SerializeObject(dt), "application/json");
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        [HttpPost]
        [Route("PostDepreciationRun")]
        public IActionResult PostDepreciationRun(string Period, int CompanyID = 0, int UserID = 0, int BranchID = 0)
        {
            try
            {
                var result = new clsFixedAssets().PostDepreciationRun(Period, CompanyID, UserID, BranchID);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        [HttpGet]
        [Route("SelectDepreciationRuns")]
        public string SelectDepreciationRuns(int CompanyID = 0)
        {
            DataTable dt = new clsFixedAssets().SelectDepreciationRuns(CompanyID);
            return dt == null ? "[]" : JsonConvert.SerializeObject(dt);
        }

        [HttpPost]
        [Route("CancelDepreciationRun")]
        public IActionResult CancelDepreciationRun(int RunID, int CompanyID = 0, int UserID = 0)
        {
            try
            {
                var result = new clsFixedAssets().CancelDepreciationRun(RunID, CompanyID, UserID);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        [HttpGet]
        [Route("SelectDepreciationSchedule")]
        public string SelectDepreciationSchedule(int AssetID = 0, int CompanyID = 0)
        {
            DataTable dt = new clsFixedAssets().SelectDepreciationSchedule(AssetID, CompanyID);
            return dt == null ? "[]" : JsonConvert.SerializeObject(dt);
        }

        // ---------- Disposal ----------
        [HttpPost]
        [Route("DisposeAsset")]
        public IActionResult DisposeAsset(int AssetID, DateTime? DisposalDate = null, decimal Proceeds = 0,
            int ProceedsAccountID = 0, int GainLossAccountID = 0, int CompanyID = 0, int UserID = 0)
        {
            try
            {
                var result = new clsFixedAssets().DisposeAsset(
                    AssetID, DisposalDate ?? DateTime.Today, Proceeds, ProceedsAccountID, CompanyID, UserID,
                    GainLossAccountID);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        [HttpGet]
        [Route("SelectDisposedAssets")]
        public string SelectDisposedAssets(int CompanyID = 0)
        {
            DataTable dt = new clsFixedAssets().SelectDisposedAssets(CompanyID);
            return dt == null ? "[]" : JsonConvert.SerializeObject(dt);
        }

        [HttpPost]
        [Route("CancelDisposal")]
        public IActionResult CancelDisposal(int AssetID, int CompanyID = 0, int UserID = 0)
        {
            try
            {
                var result = new clsFixedAssets().CancelDisposal(AssetID, CompanyID, UserID);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        // ---------- Reports ----------
        [HttpGet]
        [Route("SelectRegisterReport")]
        public string SelectRegisterReport(int CompanyID = 0, int Status = -1)
        {
            DataTable dt = new clsFixedAssets().SelectRegisterReport(CompanyID, Status);
            return dt == null ? "[]" : JsonConvert.SerializeObject(dt);
        }
    }
}
