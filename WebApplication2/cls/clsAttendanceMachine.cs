using System;
using System.Data;
using Microsoft.Data.SqlClient;

namespace WebApplication2.cls
{
    public class clsAttendanceMachine
    {
        // ==========================================================
        // SELECT ALL MACHINES BY COMPANY
        // ==========================================================
        public DataTable SelectAll(int CompanyID)
        {
            try
            {
                SqlParameter[] prm =
                {
                    new SqlParameter("@CompanyID", SqlDbType.Int) { Value = CompanyID }
                };

                clsSQL cls = new clsSQL();

                string sql = @"
                    SELECT *
                    FROM tbl_AttendanceMachines
                    WHERE CompanyID = @CompanyID
                    ORDER BY AName
                ";

                return cls.ExecuteQueryStatement(sql, cls.CreateDataBaseConnectionString(CompanyID), prm);
            }
            catch
            {
                throw;
            }
        }

        // ==========================================================
        // SELECT BY ID
        // ==========================================================
        public DataTable SelectByID(int ID, int CompanyID)
        {
            try
            {
                SqlParameter[] prm =
                {
                    new SqlParameter("@ID", SqlDbType.Int) { Value = ID },
                    new SqlParameter("@CompanyID", SqlDbType.Int) { Value = CompanyID }
                };

                clsSQL cls = new clsSQL();

                string sql = @"
                    SELECT *
                    FROM tbl_AttendanceMachines
                    WHERE ID = @ID AND CompanyID = @CompanyID
                ";

                return cls.ExecuteQueryStatement(sql, cls.CreateDataBaseConnectionString(CompanyID), prm);
            }
            catch
            {
                throw;
            }
        }

        // ==========================================================
        // INSERT
        // ==========================================================
        public int Insert(
            string AName,
            string Model,
            string IPAddress,
            int Port,
            string Password,
            bool IsActive,
            int CompanyID,
            int CreationUserID)
        {
            try
            {
                SqlParameter[] prm =
                {
                    new SqlParameter("@AName", SqlDbType.NVarChar, -1) { Value = AName ?? "" },
                    new SqlParameter("@Model", SqlDbType.NVarChar, -1) { Value = Model ?? "" },
                    new SqlParameter("@IPAddress", SqlDbType.NVarChar, 200) { Value = IPAddress ?? "" },
                    new SqlParameter("@Port", SqlDbType.Int) { Value = Port },
                    new SqlParameter("@Password", SqlDbType.NVarChar, 200) { Value = Password ?? "" },
                    new SqlParameter("@IsActive", SqlDbType.Bit) { Value = IsActive },
                    new SqlParameter("@CompanyID", SqlDbType.Int) { Value = CompanyID },
                    new SqlParameter("@CreationUserID", SqlDbType.Int) { Value = CreationUserID },
                    new SqlParameter("@CreationDate", SqlDbType.DateTime) { Value = DateTime.Now }
                };

                string sql = @"
                    INSERT INTO tbl_AttendanceMachines
                    (AName, Model, IPAddress, Port, Password, IsActive,
                     CompanyID, CreationUserID, CreationDate)
                    OUTPUT INSERTED.ID
                    VALUES
                    (@AName, @Model, @IPAddress, @Port, @Password, @IsActive,
                     @CompanyID, @CreationUserID, @CreationDate)
                ";

                clsSQL cls = new clsSQL();

                return Simulate.Integer32(
                    cls.ExecuteScalar(sql, prm, cls.CreateDataBaseConnectionString(CompanyID))
                );
            }
            catch
            {
                throw;
            }
        }

        // ==========================================================
        // UPDATE
        // ==========================================================
        public int Update(
            int ID,
            string AName,
            string Model,
            string IPAddress,
            int Port,
            string Password,
            bool IsActive,
            int CompanyID,
            int ModificationUserID)
        {
            try
            {
                SqlParameter[] prm =
                {
                    new SqlParameter("@ID", SqlDbType.Int) { Value = ID },
                    new SqlParameter("@AName", SqlDbType.NVarChar, -1) { Value = AName ?? "" },
                    new SqlParameter("@Model", SqlDbType.NVarChar, -1) { Value = Model ?? "" },
                    new SqlParameter("@IPAddress", SqlDbType.NVarChar, 200) { Value = IPAddress ?? "" },
                    new SqlParameter("@Port", SqlDbType.Int) { Value = Port },
                    new SqlParameter("@Password", SqlDbType.NVarChar, 200) { Value = Password ?? "" },
                    new SqlParameter("@IsActive", SqlDbType.Bit) { Value = IsActive },
                    new SqlParameter("@CompanyID", SqlDbType.Int) { Value = CompanyID },
                    new SqlParameter("@ModificationUserID", SqlDbType.Int) { Value = ModificationUserID },
                    new SqlParameter("@ModificationDate", SqlDbType.DateTime) { Value = DateTime.Now }
                };

                string sql = @"
                    UPDATE tbl_AttendanceMachines SET
                        AName = @AName,
                        Model = @Model,
                        IPAddress = @IPAddress,
                        Port = @Port,
                        Password = @Password,
                        IsActive = @IsActive,
                        ModificationUserID = @ModificationUserID,
                        ModificationDate = @ModificationDate
                    WHERE ID = @ID AND CompanyID = @CompanyID
                ";

                clsSQL cls = new clsSQL();

                return cls.ExecuteNonQueryStatement(sql, cls.CreateDataBaseConnectionString(CompanyID), prm);
            }
            catch
            {
                throw;
            }
        }

        // ==========================================================
        // DELETE
        // ==========================================================
        public bool Delete(int ID, int CompanyID)
        {
            try
            {
                SqlParameter[] prm =
                {
                    new SqlParameter("@ID", SqlDbType.Int) { Value = ID },
                    new SqlParameter("@CompanyID", SqlDbType.Int) { Value = CompanyID }
                };

                string sql = @"
                    DELETE FROM tbl_AttendanceMachines
                    WHERE ID = @ID AND CompanyID = @CompanyID
                ";

                clsSQL cls = new clsSQL();

                cls.ExecuteNonQueryStatement(sql, cls.CreateDataBaseConnectionString(CompanyID), prm);

                return true;
            }
            catch
            {
                throw;
            }
        }

        // ==========================================================
        // TEST CONNECTION
        // Smoke check: best-effort TCP connect to host:port.
        // ==========================================================
        public bool TestConnection(string IP, int Port, string Password)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(IP) || Port <= 0)
                    return false;

                using (var client = new System.Net.Sockets.TcpClient())
                {
                    var asyncResult = client.BeginConnect(IP, Port, null, null);
                    bool success = asyncResult.AsyncWaitHandle.WaitOne(TimeSpan.FromSeconds(3));

                    if (!success)
                    {
                        try { client.Close(); } catch { }
                        return false;
                    }

                    client.EndConnect(asyncResult);
                    return client.Connected;
                }
            }
            catch
            {
                return false;
            }
        }

        // ==========================================================
        // SYNC LOGS
        // Pull pending biometric punches into tbl_AttendanceRawPunch.
        // Vendor-agnostic skeleton: actual device pull is plugged
        // in by integrators. We commit a transaction so partial pulls
        // do not leave inconsistent rows.
        // ==========================================================
        public int SyncLogs(int MachineID, int CompanyID, int UserID)
        {
            try
            {
                clsSQL cls = new clsSQL();

                using (SqlConnection con = new SqlConnection(cls.CreateDataBaseConnectionString(CompanyID)))
                {
                    con.Open();
                    SqlTransaction trn = con.BeginTransaction();

                    try
                    {
                        // 1) Load machine config (used by vendor integrations)
                        SqlCommand getMachine = new SqlCommand(
                            @"SELECT TOP 1 ID, IPAddress, Port, Password
                              FROM tbl_AttendanceMachines
                              WHERE ID = @MachineID AND CompanyID = @CompanyID",
                            con, trn);

                        getMachine.Parameters.AddWithValue("@MachineID", MachineID);
                        getMachine.Parameters.AddWithValue("@CompanyID", CompanyID);

                        var rdr = getMachine.ExecuteReader();
                        bool found = rdr.Read();
                        rdr.Close();

                        if (!found)
                        {
                            trn.Rollback();
                            return 0;
                        }

                        // 2) Vendor pull goes here. We deliberately leave the
                        //    device-protocol layer to integrators; until then,
                        //    SyncLogs is a no-op that confirms transactional
                        //    plumbing and machine lookup are healthy.
                        int rowsPulled = 0;

                        trn.Commit();
                        return rowsPulled;
                    }
                    catch
                    {
                        try { trn.Rollback(); } catch { }
                        throw;
                    }
                }
            }
            catch
            {
                throw;
            }
        }
    }
}
