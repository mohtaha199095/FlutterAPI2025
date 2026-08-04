using System;
using Microsoft.AspNetCore.Mvc;
using WebApplication2.cls;

namespace WebApplication2.Controllers
{
    [Route("api/ctlStockBalance")]
    public class ctlStockBalance : Controller
    {
        // Rebuilds the whole snapshot for a company from transaction history.
        [HttpGet]
        [Route("RebuildStockBalance")]
        public IActionResult RebuildStockBalance(int companyId)
        {
            try
            {
                int affected = new clsStockBalance().RebuildAll(companyId);
                return Ok(ApiResponse<int>.Ok(affected, "Stock balance rebuilt."));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<string>.Fail("Server error: " + ex.Message));
            }
        }

        // Fast on-hand read for a single item (optionally per store).
        [HttpGet]
        [Route("GetOnHand")]
        public IActionResult GetOnHand(string itemGuid, int storeId, int companyId)
        {
            try
            {
                decimal onHand = new clsStockBalance().GetOnHand(Simulate.String(itemGuid), storeId, companyId);
                return Ok(ApiResponse<decimal>.Ok(onHand, "OK"));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<string>.Fail("Server error: " + ex.Message));
            }
        }
    }
}
