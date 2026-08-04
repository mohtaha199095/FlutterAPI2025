using System;
using System.Data;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Newtonsoft.Json;
using WebApplication2.cls;

namespace WebApplication2.Controllers
{
    [Route("api/ctlShiftAssignment")]
    public class ctlShiftAssignment : Controller
    {
        // ==========================================================
        // GET WEEKLY
        // Returns a single weekly assignment row for an employee in the
        // shape Flutter's ShiftAssignmentModel1 expects.
        // ==========================================================
        [HttpGet]
        [Route("GetWeekly")]
        public string GetWeekly(int EmployeeID, int CompanyID)
        {
            try
            {
                clsEmployeeShiftAssignment cls = new clsEmployeeShiftAssignment();
                DataTable dt = cls.SelectAll(EmployeeID, CompanyID);

                // Aggregate weekday rows into a single weekly record
                // Weekday encoding (server / weekly screen): 1=Sat..7=Fri
                int shiftID = 0;
                string sat = "", sun = "", mon = "", tue = "", wed = "", thu = "", fri = "";

                if (dt != null)
                {
                    foreach (DataRow r in dt.Rows)
                    {
                        int wd = Simulate.Integer32(r["WeekDay"]);
                        int sId = Simulate.Integer32(r["ShiftID"]);
                        if (shiftID == 0) shiftID = sId;
                        string flag = sId > 0 ? "1" : "";
                        switch (wd)
                        {
                            case 1: sat = flag; break;
                            case 2: sun = flag; break;
                            case 3: mon = flag; break;
                            case 4: tue = flag; break;
                            case 5: wed = flag; break;
                            case 6: thu = flag; break;
                            case 7: fri = flag; break;
                        }
                    }
                }

                var result = new
                {
                    ID = 0,
                    EmployeeID = EmployeeID,
                    ShiftID = shiftID,
                    Sunday = sun,
                    Monday = mon,
                    Tuesday = tue,
                    Wednesday = wed,
                    Thursday = thu,
                    Friday = fri,
                    Saturday = sat,
                };

                // Wrap in an array so Flutter's list-decode path works
                return JsonConvert.SerializeObject(new[] { result });
            }
            catch
            {
                throw;
            }
        }

        // ==========================================================
        // SAVE WEEKLY
        // Body: ShiftAssignmentModel1 JSON
        // Replaces the employee's weekday rows in tbl_EmployeeShiftAssignment
        // atomically. A day's flag value (any non-empty string) means the
        // shift applies to that weekday.
        // ==========================================================
        [HttpPost]
        [Route("SaveWeekly")]
        public string SaveWeekly([FromBody] JsonElement data, int CompanyID = 0, int UserID = 0)
        {
            try
            {
                int employeeID = data.TryGetProperty("EmployeeID", out var eEl) && eEl.ValueKind == JsonValueKind.Number ? eEl.GetInt32() : 0;
                int shiftID = data.TryGetProperty("ShiftID", out var sEl) && sEl.ValueKind == JsonValueKind.Number ? sEl.GetInt32() : 0;

                if (CompanyID == 0 && data.TryGetProperty("CompanyID", out var cEl) && cEl.ValueKind == JsonValueKind.Number)
                    CompanyID = cEl.GetInt32();

                if (employeeID <= 0 || shiftID <= 0 || CompanyID <= 0)
                    return "{\"success\":false,\"message\":\"Missing EmployeeID, ShiftID or CompanyID\"}";

                bool sat = HasFlag(data, "Saturday");
                bool sun = HasFlag(data, "Sunday");
                bool mon = HasFlag(data, "Monday");
                bool tue = HasFlag(data, "Tuesday");
                bool wed = HasFlag(data, "Wednesday");
                bool thu = HasFlag(data, "Thursday");
                bool fri = HasFlag(data, "Friday");

                // No start/end on weekly payload - cover an indefinite range
                string startDate = DateTime.Today.ToString("yyyy-MM-dd");
                string endDate = DateTime.Today.AddYears(50).ToString("yyyy-MM-dd");

                clsSQL clsSQL = new clsSQL();
                using (SqlConnection con = new SqlConnection(clsSQL.CreateDataBaseConnectionString(CompanyID)))
                {
                    con.Open();
                    SqlTransaction trn = con.BeginTransaction();

                    try
                    {
                        // 1) Wipe existing weekly rows for this employee
                        SqlCommand del = new SqlCommand(
                            @"DELETE FROM tbl_EmployeeShiftAssignment
                              WHERE EmployeeID = @EmployeeID AND CompanyID = @CompanyID",
                            con, trn);
                        del.Parameters.AddWithValue("@EmployeeID", employeeID);
                        del.Parameters.AddWithValue("@CompanyID", CompanyID);
                        del.ExecuteNonQuery();

                        // 2) Insert one row per active weekday (1=Sat..7=Fri)
                        clsEmployeeShiftAssignment esa = new clsEmployeeShiftAssignment();
                        if (sat) esa.Insert(employeeID, shiftID, 1, startDate, endDate, true, CompanyID, UserID, trn);
                        if (sun) esa.Insert(employeeID, shiftID, 2, startDate, endDate, true, CompanyID, UserID, trn);
                        if (mon) esa.Insert(employeeID, shiftID, 3, startDate, endDate, true, CompanyID, UserID, trn);
                        if (tue) esa.Insert(employeeID, shiftID, 4, startDate, endDate, true, CompanyID, UserID, trn);
                        if (wed) esa.Insert(employeeID, shiftID, 5, startDate, endDate, true, CompanyID, UserID, trn);
                        if (thu) esa.Insert(employeeID, shiftID, 6, startDate, endDate, true, CompanyID, UserID, trn);
                        if (fri) esa.Insert(employeeID, shiftID, 7, startDate, endDate, true, CompanyID, UserID, trn);

                        trn.Commit();
                        return "{\"success\":true,\"message\":\"Weekly assignment saved\"}";
                    }
                    catch
                    {
                        try { trn.Rollback(); } catch { }
                        throw;
                    }
                }
            }
            catch (Exception ex)
            {
                return "{\"success\":false,\"message\":\"" + ex.Message.Replace("\"", "'") + "\"}";
            }
        }

        private static bool HasFlag(JsonElement data, string name)
        {
            if (!data.TryGetProperty(name, out var v)) return false;
            if (v.ValueKind == JsonValueKind.String) return !string.IsNullOrEmpty(v.GetString());
            if (v.ValueKind == JsonValueKind.True) return true;
            if (v.ValueKind == JsonValueKind.Number) return v.GetInt32() != 0;
            return false;
        }
    }
}
