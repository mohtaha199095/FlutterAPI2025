using System;
using Microsoft.AspNetCore.Mvc;
using WebApplication2.cls;

namespace WebApplication2.Controllers
{
    [Route("api/ctlInventoryDemoData")]
    public class ctlInventoryDemoData : Controller
    {
        [HttpPost]
        [Route("SeedInventoryDemo")]
        public IActionResult SeedInventoryDemo(int CompanyID, int UserId, bool Force = false)
        {
            try
            {
                if (!TryGetCompanyConnection(CompanyID, out string error))
                    return BadRequest(MakeError(error));

                InventoryDemoSeedResult result =
                    new clsInventoryDemoData().Seed(CompanyID, UserId, Force);
                return result.Success ? Ok(result) : BadRequest(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, MakeError("Server error: " + ex.Message));
            }
        }

        [HttpPost]
        [Route("SeedMovementDemo")]
        public IActionResult SeedMovementDemo(int CompanyID, int UserId)
        {
            try
            {
                if (!TryGetCompanyConnection(CompanyID, out string error))
                    return BadRequest(MakeError(error));

                InventoryDemoSeedResult result =
                    new clsInventoryDemoData().SeedMovementTransactions(CompanyID, UserId);
                return result.Success ? Ok(result) : BadRequest(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, MakeError("Server error: " + ex.Message));
            }
        }

        static bool TryGetCompanyConnection(int companyId, out string error)
        {
            error = "";
            clsSQL sql = new clsSQL();
            string connStr = sql.CreateDataBaseConnectionString(companyId);
            if (string.IsNullOrWhiteSpace(connStr))
            {
                error = "Invalid company or database connection.";
                return false;
            }
            return true;
        }

        static InventoryDemoSeedResult MakeError(string message) =>
            new InventoryDemoSeedResult { Success = false, Message = message };
    }
}
