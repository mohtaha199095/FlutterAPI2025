using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Newtonsoft.Json;
using WebApplication2.cls;

namespace WebApplication2.Controllers
{
    [Route("api/ctlStockCount")]
    public class ctlStockCount : Controller
    {
        // POST body: JSON array of { "ItemGuid": "...", "CountedQty": 12 }
        [HttpPost]
        [Route("PostStockCount")]
        public IActionResult PostStockCount(
            int branchId, int storeId, int adjustmentAccountId,
            string note, int companyId, int userId, DateTime countDate,
            [FromBody] string linesJson)
        {
            clsSQL clsSQL = new clsSQL();
            using SqlConnection con = new SqlConnection(clsSQL.CreateDataBaseConnectionString(companyId));
            con.Open();
            using SqlTransaction trn = con.BeginTransaction();
            try
            {
                List<StockCountLine> lines;
                try
                {
                    lines = JsonConvert.DeserializeObject<List<StockCountLine>>(linesJson);
                }
                catch (Exception ex)
                {
                    return BadRequest(ApiResponse<string>.Fail("Invalid lines JSON: " + ex.Message));
                }

                StockCountResult result = new clsStockCount().PostStockCount(
                    branchId, storeId, lines, adjustmentAccountId, note,
                    companyId, userId, countDate, trn);

                if (!result.Success)
                {
                    trn.Rollback();
                    return BadRequest(ApiResponse<string>.Fail(result.Message));
                }

                trn.Commit();
                return Ok(result);
            }
            catch (Exception ex)
            {
                try { trn.Rollback(); } catch { }
                return StatusCode(500, ApiResponse<string>.Fail("Server error: " + ex.Message));
            }
            finally { con.Close(); }
        }
    }
}
