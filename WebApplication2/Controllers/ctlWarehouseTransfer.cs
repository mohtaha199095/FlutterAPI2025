using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Newtonsoft.Json;
using WebApplication2.cls;

namespace WebApplication2.Controllers
{
    [Route("api/ctlWarehouseTransfer")]
    public class ctlWarehouseTransfer : Controller
    {
        // POST body: JSON array of { "ItemGuid": "...", "Qty": 5 }
        [HttpPost]
        [Route("PostTransfer")]
        public IActionResult PostTransfer(
            int branchId, int sourceStoreId, int destStoreId,
            string note, int companyId, int userId, DateTime transferDate,
            [FromBody] string linesJson)
        {
            clsSQL clsSQL = new clsSQL();
            using SqlConnection con = new SqlConnection(clsSQL.CreateDataBaseConnectionString(companyId));
            con.Open();
            using SqlTransaction trn = con.BeginTransaction();
            try
            {
                List<WarehouseTransferLine> lines;
                try
                {
                    lines = JsonConvert.DeserializeObject<List<WarehouseTransferLine>>(linesJson);
                }
                catch (Exception ex)
                {
                    return BadRequest(ApiResponse<string>.Fail("Invalid lines JSON: " + ex.Message));
                }

                WarehouseTransferResult result = new clsWarehouseTransfer().PostTransfer(
                    branchId, sourceStoreId, destStoreId, lines, note,
                    companyId, userId, transferDate, trn);

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
