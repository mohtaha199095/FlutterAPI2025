using Microsoft.AspNet.SignalR.Infrastructure;
using System;
using System.Data;
using System.Data.SqlClient;
using System.Net.NetworkInformation;
using Microsoft.AspNet.SignalR.Infrastructure;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Nancy;
using System.Data.SqlClient;
using System;
using System.Threading.Tasks;
using WebApplication2.Controllers;
namespace WebApplication2.cls
{
    public class TableHub : Hub
    {
        // You can define additional methods here if needed
    }
    public class clsBranchFloorsTables
    {
        private readonly ConnectionManager _connectionManager;
        private readonly IHubContext<TableHub> _hubContext;

        public class TableUpdateRequest
        {
            public string CompanyId { get; set; } // New field for dynamic connection string
            public int TableId { get; set; }
            public string Status { get; set; }
        }
        
        public string UpdateBranchFloorsTablesStatus(int CompanyID, int TableID, int NewColor, SqlTransaction trn=null)
        {
            try
            {
                clsSQL clsSQL = new clsSQL();

                SqlParameter[] prm =
                   {
                    new SqlParameter("@CompanyID", SqlDbType.Int) { Value = CompanyID },

                    new SqlParameter("@TableID", SqlDbType.Int) { Value = TableID },
                    new SqlParameter("@NewColor", SqlDbType.Int) { Value = NewColor },


                };
                string a = @"update tbl_BranchFloorsTables set  

 Color =@NewColor  
 where ID=@TableID";

                string A = Simulate.String(clsSQL.ExecuteNonQueryStatement(a, clsSQL.CreateDataBaseConnectionString(CompanyID), prm, trn));
              

                return A;


            }
            catch (Exception ex)
            {

                return "";
            }

        }
        public DataTable SelectBranchFloorsTables(int Id, int floorID, string AName, string EName, int CompanyID)
        {
            try
            {
                SqlParameter[] prm =
                {
                    new SqlParameter("@Id", SqlDbType.Int) { Value = Id }, 
                    new SqlParameter("@floorID", SqlDbType.Int) { Value = floorID },
                    new SqlParameter("@AName", SqlDbType.NVarChar, -1) { Value = AName },
                    new SqlParameter("@EName", SqlDbType.NVarChar, -1) { Value = EName },
                    new SqlParameter("@CompanyID", SqlDbType.Int) { Value = CompanyID },
                };

                clsSQL clsSQL = new clsSQL();
                DataTable dt = clsSQL.ExecuteQueryStatement(@"SELECT * FROM tbl_BranchFloorsTables WHERE 
                    (ID = @Id OR @Id = 0) AND
                    (AName = @AName OR @AName = '') AND
                    (EName = @EName OR @EName = '') AND
                    (CompanyID = @CompanyID OR @CompanyID = 0)",
                    clsSQL.CreateDataBaseConnectionString(CompanyID), prm);

                return dt;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public bool DeleteBranchFloorsTableByID(int Id, int CompanyID)
        {
            try
            {
                SqlParameter[] prm =
                {
                    new SqlParameter("@Id", SqlDbType.Int) { Value = Id }
                };

                clsSQL clsSQL = new clsSQL();
                clsSQL.ExecuteNonQueryStatement(@"DELETE FROM tbl_BranchFloorsTables WHERE ID = @Id",
                    clsSQL.CreateDataBaseConnectionString(CompanyID), prm);

                return true;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public int InsertBranchFloorsTable(string AName, string EName, string Shape, string Color, int ChairsCount, int PositionX, int PositionY, decimal Width, int FloorID, int CompanyID, int CreationUserID)
        {
            try
            {
                SqlParameter[] prm =
                {
                    new SqlParameter("@AName", SqlDbType.NVarChar, -1) { Value = AName },
                    new SqlParameter("@EName", SqlDbType.NVarChar, -1) { Value = EName },
                    new SqlParameter("@Shape", SqlDbType.NVarChar, -1) { Value = Shape },
                    new SqlParameter("@Color", SqlDbType.NVarChar, -1) { Value = Color },
                    new SqlParameter("@ChairsCount", SqlDbType.Int) { Value = ChairsCount },
                    new SqlParameter("@PositionX", SqlDbType.Int) { Value = PositionX },
                    new SqlParameter("@PositionY", SqlDbType.Int) { Value = PositionY },
                    new SqlParameter("@Width", SqlDbType.Decimal) { Value = Width },
                    new SqlParameter("@FloorID", SqlDbType.Int) { Value = FloorID },
                    new SqlParameter("@CompanyID", SqlDbType.Int) { Value = CompanyID },
                    new SqlParameter("@CreationUserID", SqlDbType.Int) { Value = CreationUserID },
                    new SqlParameter("@CreationDate", SqlDbType.DateTime) { Value = DateTime.Now },
                };

                string query = @"INSERT INTO tbl_BranchFloorsTables (AName, EName, Shape, Color, ChairsCount, PositionX, PositionY, Width, FloorID, CompanyID, CreationUserID, CreationDate)
                                  OUTPUT INSERTED.ID
                                  VALUES (@AName, @EName, @Shape, @Color, @ChairsCount, @PositionX, @PositionY, @Width, @FloorID, @CompanyID, @CreationUserID, @CreationDate)";

                clsSQL clsSQL = new clsSQL();
                return Convert.ToInt32(clsSQL.ExecuteScalar(query, prm, clsSQL.CreateDataBaseConnectionString(CompanyID)));
            }
            catch (Exception)
            {
                throw;
            }
        }

        public int UpdateBranchFloorsTable(int ID, string AName, string EName, string Shape, string Color, int ChairsCount, int PositionX, int PositionY, decimal Width, int ModificationUserID, int CompanyID)
        {
            try
            {
                SqlParameter[] prm =
                {
                    new SqlParameter("@ID", SqlDbType.Int) { Value = ID },
                    new SqlParameter("@AName", SqlDbType.NVarChar, -1) { Value = AName },
                    new SqlParameter("@EName", SqlDbType.NVarChar, -1) { Value = EName },
                    new SqlParameter("@Shape", SqlDbType.NVarChar, -1) { Value = Shape },
                    new SqlParameter("@Color", SqlDbType.NVarChar, -1) { Value = Color },
                    new SqlParameter("@ChairsCount", SqlDbType.Int) { Value = ChairsCount },
                    new SqlParameter("@PositionX", SqlDbType.Int) { Value = PositionX },
                    new SqlParameter("@PositionY", SqlDbType.Int) { Value = PositionY },
                    new SqlParameter("@Width", SqlDbType.Decimal) { Value = Width },
                    new SqlParameter("@ModificationUserID", SqlDbType.Int) { Value = ModificationUserID },
                    new SqlParameter("@ModificationDate", SqlDbType.DateTime) { Value = DateTime.Now },
                };

                string query = @"UPDATE tbl_BranchFloorsTables SET 
                                  AName = @AName, 
                                  EName = @EName, 
                                  Shape = @Shape, 
                                  Color = @Color, 
                                  ChairsCount = @ChairsCount, 
                                  PositionX = @PositionX, 
                                  PositionY = @PositionY, 
                                  Width = @Width, 
                                  ModificationDate = @ModificationDate, 
                                  ModificationUserID = @ModificationUserID
                                  WHERE ID = @ID";

                clsSQL clsSQL = new clsSQL();
                return clsSQL.ExecuteNonQueryStatement(query, clsSQL.CreateDataBaseConnectionString(CompanyID), prm);
            }
            catch (Exception)
            {
                throw;
            }
        }
    }
}
