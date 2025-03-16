using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.SignalR;
using System;
using System.Threading.Tasks;

namespace WebApplication2.cls
{
    [EnableCors("AllowSpecificOrigin")] // Ensure CORS policy is applied
    public class TableService
    {
        private readonly IHubContext<TableHub> _hubContext;

        public TableService(IHubContext<TableHub> hubContext)
        {
            _hubContext = hubContext;
        }

        public async Task tbl_BranchFloorsTablesColorUpdated(int companyId, int tableId, int status)
        {
            try
            {
                // Dynamically update table status in the database
                clsBranchFloorsTables clsBranchFloors = new clsBranchFloorsTables();
                var result = clsBranchFloors.UpdateBranchFloorsTablesStatus(companyId, tableId, status, null);

                // Notify all clients about the status update via SignalR
                await _hubContext.Clients.All.SendAsync("TableStatusUpdated", tableId, status, companyId);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred: {ex.Message}");
                throw; // Re-throw the exception if needed
            }
        }
    }
}
