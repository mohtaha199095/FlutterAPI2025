using System;
using System.Data;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using WebApplication2.cls;

namespace WebApplication2.Controllers
{
    [Route("api/ctlAttendanceMachine")]
    public class ctlAttendanceMachine : Controller
    {
        // ==========================================================
        // SELECT ALL
        // ==========================================================
        [HttpGet]
        [Route("SelectAll")]
        public string SelectAll(int CompanyID)
        {
            try
            {
                clsAttendanceMachine obj = new clsAttendanceMachine();
                DataTable dt = obj.SelectAll(CompanyID);
                return dt != null ? JsonConvert.SerializeObject(dt) : "[]";
            }
            catch
            {
                throw;
            }
        }

        // ==========================================================
        // SELECT BY ID
        // ==========================================================
        [HttpGet]
        [Route("SelectByID")]
        public string SelectByID(int ID, int CompanyID)
        {
            try
            {
                clsAttendanceMachine obj = new clsAttendanceMachine();
                DataTable dt = obj.SelectByID(ID, CompanyID);
                return dt != null ? JsonConvert.SerializeObject(dt) : "[]";
            }
            catch
            {
                throw;
            }
        }

        // ==========================================================
        // SAVE (Insert or Update based on ID)
        // Body: AttendanceMachine JSON
        //  { ID, AName, Model, IPAddress, Port, Password, IsActive, CompanyID }
        // ==========================================================
        [HttpPost]
        [Route("Save")]
        public int Save([FromBody] JsonElement data, int CreationUserID = 0)
        {
            try
            {
                int ID = data.TryGetProperty("ID", out var idEl) ? idEl.GetInt32() : 0;
                string AName = data.TryGetProperty("AName", out var anEl) && anEl.ValueKind == JsonValueKind.String ? anEl.GetString() : "";
                string Model = data.TryGetProperty("Model", out var mEl) && mEl.ValueKind == JsonValueKind.String ? mEl.GetString() : "";
                string IPAddress = data.TryGetProperty("IPAddress", out var ipEl) && ipEl.ValueKind == JsonValueKind.String ? ipEl.GetString() : "";
                int Port = data.TryGetProperty("Port", out var pEl) && pEl.ValueKind == JsonValueKind.Number ? pEl.GetInt32() : 0;
                string Password = data.TryGetProperty("Password", out var pwEl) && pwEl.ValueKind == JsonValueKind.String ? pwEl.GetString() : "";
                bool IsActive = data.TryGetProperty("IsActive", out var aEl) && (aEl.ValueKind == JsonValueKind.True || aEl.ValueKind == JsonValueKind.False) ? aEl.GetBoolean() : true;
                int CompanyID = data.TryGetProperty("CompanyID", out var cEl) && cEl.ValueKind == JsonValueKind.Number ? cEl.GetInt32() : 0;

                clsAttendanceMachine obj = new clsAttendanceMachine();

                if (ID > 0)
                {
                    return obj.Update(ID, AName, Model, IPAddress, Port, Password, IsActive, CompanyID, CreationUserID);
                }
                else
                {
                    return obj.Insert(AName, Model, IPAddress, Port, Password, IsActive, CompanyID, CreationUserID);
                }
            }
            catch
            {
                throw;
            }
        }

        // ==========================================================
        // DELETE
        // ==========================================================
        [HttpPost]
        [Route("Delete")]
        public bool Delete(int ID, int CompanyID)
        {
            try
            {
                clsAttendanceMachine obj = new clsAttendanceMachine();
                return obj.Delete(ID, CompanyID);
            }
            catch
            {
                throw;
            }
        }

        // ==========================================================
        // TEST CONNECTION
        // ==========================================================
        [HttpGet]
        [Route("TestConnection")]
        public bool TestConnection(string IP, int Port, string Password)
        {
            try
            {
                clsAttendanceMachine obj = new clsAttendanceMachine();
                return obj.TestConnection(IP, Port, Password);
            }
            catch
            {
                return false;
            }
        }

        // ==========================================================
        // SYNC LOGS
        // ==========================================================
        [HttpPost]
        [Route("SyncLogs")]
        public int SyncLogs(int MachineID, int CompanyID, int UserID = 0)
        {
            try
            {
                clsAttendanceMachine obj = new clsAttendanceMachine();
                return obj.SyncLogs(MachineID, CompanyID, UserID);
            }
            catch
            {
                throw;
            }
        }
    }
}
