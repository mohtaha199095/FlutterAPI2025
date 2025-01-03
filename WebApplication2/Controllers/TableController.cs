using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using WebApplication2.cls;

namespace WebApplication2.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class TableController : ControllerBase
    {
        private readonly TableService _tableService;

        public TableController(TableService tableService)
        {
            _tableService = tableService;
        }

        [HttpPost("updateTableStatus")]
        public async Task<IActionResult> UpdateTableStatus(int CompanyID,int Status, int TableId)
        {
            

            await _tableService.tbl_BranchFloorsTablesColorUpdated(CompanyID, TableId, Status);
            return Ok("Table status updated.");
        }
    }

    public class TableUpdateRequest
    {
        public int CompanyId { get; set; } // New field for dynamic connection string
        public int TableId { get; set; }
        public int Status { get; set; }
    }

}
