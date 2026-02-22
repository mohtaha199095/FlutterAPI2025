//using Microsoft.AspNetCore.Mvc;
//using System.Threading.Tasks;
//using WebApplication2.cls;

//namespace WebApplication2.Controllers
//{
//    [ApiController]
//    [Route("[controller]")]
//    public class TableController : ControllerBase
//    {
//        private readonly TableService _tableService;

//        public TableController(TableService tableService)
//        {
//            _tableService = tableService;
//        }

//        [HttpPost("updateTableStatus")]
//        public async Task<IActionResult> UpdateTableStatus(int CompanyID,int Status, int TableId)
//        {


//            await _tableService.tbl_BranchFloorsTablesColorUpdated(CompanyID, TableId, Status);
//            return Ok("Table status updated.");
//        }
//    }

//    public class TableUpdateRequest
//    {
//        public int CompanyId { get; set; } // New field for dynamic connection string
//        public int TableId { get; set; }
//        public int Status { get; set; }
//    }

//}
using Microsoft.AspNetCore.Cors;
 
using System;
using System.Text.Json;
using System.Threading.Tasks;

namespace WebApplication2.cls
{
   
    public class TableService
    {
      

        public async Task tbl_BranchFloorsTablesColorUpdated(int companyId, int tableId, int status)
        {
            try
            {
                // 1) Update DB
                

                // 2) ✅ Broadcast via Raw WebSocket (Flutter web_socket_channel)
                var payload = JsonSerializer.Serialize(new
                {
                    type = "TableStatusUpdated",
                    companyId = companyId,
                    tableId = tableId,
                    status = status
                });
                int branchId = 1;
                await TablesWsManager.BroadcastToBranch(branchId, payload);
                // 3) (اختياري) Keep SignalR too
                // await _hubContext.Clients.All.SendAsync("TableStatusUpdated", tableId, status, companyId);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred: {ex.Message}");
                throw;
            }
        }
    }
}
