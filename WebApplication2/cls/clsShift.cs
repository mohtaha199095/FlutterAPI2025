using Microsoft.Data.SqlClient;
using Newtonsoft.Json;
using System;
using System.Data;

namespace WebApplication2.cls
{
    public class clsShift
    {
        // ==========================================================
        // SELECT SHIFT
        // ==========================================================
        public DataTable SelectShiftByID(int ID, int CompanyID)
        {
            try
            {
                SqlParameter[] prm =
                {
                    new SqlParameter("@ID", SqlDbType.Int){ Value = ID },
                    new SqlParameter("@CompanyID", SqlDbType.Int){ Value = CompanyID }
                };

                clsSQL cls = new clsSQL();

                return cls.ExecuteQueryStatement(@"
                    SELECT *
                    FROM tbl_Shifts
                    WHERE (ID = @ID OR @ID = 0)
                      AND CompanyID = @CompanyID
                ", cls.CreateDataBaseConnectionString(CompanyID), prm);
            }
            catch { throw; }
        }

        // ==========================================================
        // SELECT SHIFT DETAILS
        // ==========================================================
        public DataTable SelectShiftDetails(int ShiftID, int CompanyID)
        {
            try
            {
                SqlParameter[] prm =
                {
                    new SqlParameter("@ShiftID", SqlDbType.Int){ Value = ShiftID },
                    new SqlParameter("@CompanyID", SqlDbType.Int){ Value = CompanyID }
                };

                clsSQL cls = new clsSQL();

                return cls.ExecuteQueryStatement(@"
                    SELECT *
                    FROM tbl_ShiftDetails
                    WHERE ShiftID = @ShiftID
                      AND CompanyID = @CompanyID
                    ORDER BY SegmentNo
                ", cls.CreateDataBaseConnectionString(CompanyID), prm);
            }
            catch { throw; }
        }

        // ==========================================================
        // INSERT SHIFT + DETAILS
        // ==========================================================
        public int InsertShift(
            string AName,
            string EName,
            string StartTime,
            string EndTime,
            int GraceEarly,
            int GraceLate,
            int BreakMinutes,
            bool IsOvernight,
            bool IsUseDetails,
            string jsonDetails,      // JSON string from Flutter
            int CompanyID,
            int CreationUserID,
            SqlTransaction trn = null
        )
        {
            try
            {
                SqlParameter[] prm =
                {
                    new SqlParameter("@ShiftAName", AName),
                    new SqlParameter("@ShiftEName", EName),
                    new SqlParameter("@StartTime", StartTime),
                    new SqlParameter("@EndTime", EndTime),
                    new SqlParameter("@GraceEarly", GraceEarly),
                    new SqlParameter("@GraceLate", GraceLate),
                    new SqlParameter("@BreakMinutes", BreakMinutes),
                    new SqlParameter("@IsOvernight", IsOvernight),
                    new SqlParameter("@IsUseDetails", IsUseDetails),
                    new SqlParameter("@CompanyID", CompanyID),
                    new SqlParameter("@CreationUserID", CreationUserID),
                    new SqlParameter("@CreationDate", DateTime.Now)
                };
                    
                string sql = @"
                    INSERT INTO tbl_Shifts
                    (AName, EName, StartTime, EndTime,
                     GraceEarlyMinutes, GraceLateMinutes, BreakMinutes,
                     IsOvernight, IsUseDetails, IsActive,
                     CompanyID, CreationUserID, CreationDate)
                    OUTPUT INSERTED.ID
                    VALUES
                    (@ShiftAName, @ShiftEName, @StartTime, @EndTime,
                     @GraceEarly, @GraceLate, @BreakMinutes,
                     @IsOvernight, @IsUseDetails, 1,
                     @CompanyID, @CreationUserID, @CreationDate)
                ";

                clsSQL cls = new clsSQL();

                int newID = Simulate.Integer32(
                    cls.ExecuteScalar(sql, prm, cls.CreateDataBaseConnectionString(CompanyID))
                );

                // Insert Details
                InsertShiftDetails(newID, jsonDetails, CompanyID, CreationUserID);

                return newID;
            }
            catch { throw; }
        }

        // ==========================================================
        // UPDATE SHIFT
        // ==========================================================
        public int UpdateShift(
            int ID,
            string AName,
            string EName,
            string StartTime,
            string EndTime,
            int GraceEarly,
            int GraceLate,
            int BreakMinutes,
            bool IsOvernight,
            bool IsUseDetails,
            string jsonDetails,
            int ModificationUserID,
            int CompanyID
        )
        {
            try
            {
                SqlParameter[] prm =
                {
                    new SqlParameter("@ID", ID),
                    new SqlParameter("@ShiftAName", AName),
                    new SqlParameter("@ShiftEName", EName),
                    new SqlParameter("@StartTime", StartTime),
                    new SqlParameter("@EndTime", EndTime),
                    new SqlParameter("@GraceEarly", GraceEarly),
                    new SqlParameter("@GraceLate", GraceLate),
                    new SqlParameter("@BreakMinutes", BreakMinutes),
                    new SqlParameter("@IsOvernight", IsOvernight),
                    new SqlParameter("@IsUseDetails", IsUseDetails),
                    new SqlParameter("@ModificationUserID", ModificationUserID),
                    new SqlParameter("@ModificationDate", DateTime.Now)
                };

                string sql = @"
                    UPDATE tbl_Shifts SET
                         AName = @ShiftAName,
                         EName = @ShiftEName,
                        StartTime = @StartTime,
                        EndTime = @EndTime,
                        GraceEarlyMinutes = @GraceEarly,
                        GraceLateMinutes = @GraceLate,
                        BreakMinutes = @BreakMinutes,
                        IsOvernight = @IsOvernight,
                        IsUseDetails = @IsUseDetails,
                        ModificationUserID = @ModificationUserID,
                        ModificationDate = @ModificationDate
                    WHERE ID = @ID
                ";

                clsSQL cls = new clsSQL();

                int a = cls.ExecuteNonQueryStatement(
                    sql, cls.CreateDataBaseConnectionString(CompanyID), prm
                );

                // Remove old details
                DeleteShiftDetails(ID, CompanyID);

                // Insert new details
                InsertShiftDetails(ID, jsonDetails, CompanyID, ModificationUserID);

                return a;
            }
            catch { throw; }
        }

        // ==========================================================
        // DELETE SHIFT + DETAILS
        // ==========================================================
        public int DeleteShift(int ID, int CompanyID)
        {
            try
            {
                DeleteShiftDetails(ID, CompanyID);

                SqlParameter[] prm =
                {
                    new SqlParameter("@ID", ID)
                };

                clsSQL cls = new clsSQL();

                return cls.ExecuteNonQueryStatement(@"
                    DELETE FROM tbl_Shifts 
                    WHERE ID = @ID
                ", cls.CreateDataBaseConnectionString(CompanyID), prm);
            }
            catch { throw; }
        }

        // ==========================================================
        // INSERT SHIFT DETAILS (INTERNAL)
        // ==========================================================
        private void InsertShiftDetails(int ShiftID, string json, int CompanyID, int UserID)
        {
            if (string.IsNullOrWhiteSpace(json)) return;

            DataTable dt = JsonConvert.DeserializeObject<DataTable>(json);

            clsSQL cls = new clsSQL();

            foreach (DataRow dr in dt.Rows)
            {
                SqlParameter[] prm =
                {
                    new SqlParameter("@ShiftID", ShiftID),
                    new SqlParameter("@SegmentNo", dr["SegmentNo"]),
                    new SqlParameter("@StartTime", dr["StartTime"]),
                    new SqlParameter("@EndTime", dr["EndTime"]),
                    new SqlParameter("@BreakMinutes", dr["BreakMinutes"]),
                    new SqlParameter("@IsOvernight", dr["IsOvernight"]),
                    new SqlParameter("@CompanyID", CompanyID),
                    new SqlParameter("@CreationUserID", UserID),
                    new SqlParameter("@CreationDate", DateTime.Now)
                };

                cls.ExecuteNonQueryStatement(@"
                    INSERT INTO tbl_ShiftDetails
                    (ShiftID, SegmentNo, StartTime, EndTime, BreakMinutes,
                     IsOvernight, CompanyID, CreationUserID, CreationDate)
                    VALUES
                    (@ShiftID, @SegmentNo, @StartTime, @EndTime, @BreakMinutes,
                     @IsOvernight, @CompanyID, @CreationUserID, @CreationDate)
                ", cls.CreateDataBaseConnectionString(CompanyID), prm);
            }
        }

        // ==========================================================
        // DELETE SHIFT DETAILS
        // ==========================================================
        private void DeleteShiftDetails(int ShiftID, int CompanyID)
        {
            SqlParameter[] prm =
            {
                new SqlParameter("@ShiftID", ShiftID)
            };

            clsSQL cls = new clsSQL();

            cls.ExecuteNonQueryStatement(@"
                DELETE FROM tbl_ShiftDetails
                WHERE ShiftID = @ShiftID
            ", cls.CreateDataBaseConnectionString(CompanyID), prm);
        }
    }
}
