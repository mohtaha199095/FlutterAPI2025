using System;
using System.Data;
using Microsoft.Data.SqlClient;

namespace WebApplication2.cls
{
    public class clsPOSScaleConfiguration
    {
        public DataTable SelectScales(int ID, string ScaleName, string ScaleType, int CompanyID)
        {
            try
            {
                SqlParameter[] prm =
                {
                    new SqlParameter("@ID", SqlDbType.Int) { Value = ID },
                    new SqlParameter("@ScaleName", SqlDbType.NVarChar, -1) { Value = ScaleName },
                    new SqlParameter("@ScaleType", SqlDbType.NVarChar, -1) { Value = ScaleType },
                    new SqlParameter("@CompanyID", SqlDbType.Int) { Value = CompanyID },
                };

                clsSQL clsSQL = new clsSQL();
                DataTable dt = clsSQL.ExecuteQueryStatement(@"
                    SELECT * FROM tbl_POSScaleConfiguration 
                    WHERE (ID = @ID OR @ID = 0) 
                    AND (ScaleName = @ScaleName OR @ScaleName = '') 
                    AND (ScaleType = @ScaleType OR @ScaleType = '') 
                    AND (CompanyID = @CompanyID OR @CompanyID = 0)",
                    clsSQL.CreateDataBaseConnectionString(CompanyID), prm);

                return dt;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public bool DeleteScaleByID(int ID, int CompanyID)
        {
            try
            {
                SqlParameter[] prm =
                {
                    new SqlParameter("@ID", SqlDbType.Int) { Value = ID }
                };

                clsSQL clsSQL = new clsSQL();
                int rowsAffected = clsSQL.ExecuteNonQueryStatement(@"
                    DELETE FROM tbl_POSScaleConfiguration WHERE ID = @ID",
                    clsSQL.CreateDataBaseConnectionString(CompanyID), prm);

                return rowsAffected > 0;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public int InsertScale(
            string ScaleName, string ScaleType, string ConnectionType, string PortName,
            int BaudRate, int DataBits, string Parity, int StopBits, string BarcodePrefix,int SKULength ,
            bool AutoDetect, string DefaultPrintType,int Divisionfactor, bool Status, int CompanyID, int CreationUserID,
            SqlTransaction trn = null)
        {
            try
            {
                SqlParameter[] prm =
                {
                    new SqlParameter("@ScaleName", SqlDbType.NVarChar, -1) { Value = ScaleName },
                    new SqlParameter("@ScaleType", SqlDbType.NVarChar, -1) { Value = ScaleType },
                    new SqlParameter("@ConnectionType", SqlDbType.NVarChar, -1) { Value = ConnectionType },
                    new SqlParameter("@PortName", SqlDbType.NVarChar, -1) { Value = PortName },
                    new SqlParameter("@BaudRate", SqlDbType.Int) { Value = BaudRate },
                    new SqlParameter("@DataBits", SqlDbType.Int) { Value = DataBits },
                    new SqlParameter("@Parity", SqlDbType.NVarChar, -1) { Value = Parity },
                    new SqlParameter("@StopBits", SqlDbType.Int) { Value = StopBits },
                    new SqlParameter("@BarcodePrefix", SqlDbType.NVarChar, -1) { Value = BarcodePrefix },
                         new SqlParameter("@SKULength", SqlDbType.Int) { Value = SKULength },
                    new SqlParameter("@AutoDetect", SqlDbType.Bit) { Value = AutoDetect },
                    new SqlParameter("@DefaultPrintType", SqlDbType.NVarChar, -1) { Value = DefaultPrintType },
                    new SqlParameter("@Divisionfactor", SqlDbType.Int) { Value = Divisionfactor },
                   new SqlParameter("@Status", SqlDbType.Bit) { Value = Status },
                    new SqlParameter("@CompanyID", SqlDbType.Int) { Value = CompanyID },
                    new SqlParameter("@CreationUserID", SqlDbType.Int) { Value = CreationUserID },
                    new SqlParameter("@CreationDate", SqlDbType.DateTime) { Value = DateTime.Now },
                };

                string query = @"
                    INSERT INTO tbl_POSScaleConfiguration 
                    (ScaleName, ScaleType, ConnectionType, PortName, BaudRate, DataBits, Parity, StopBits, BarcodePrefix, SKULength,
                     AutoDetect, DefaultPrintType,Divisionfactor, Status, CompanyID, CreationUserID, CreationDate)
                    OUTPUT INSERTED.ID 
                    VALUES 
                    (@ScaleName, @ScaleType, @ConnectionType, @PortName, @BaudRate, @DataBits, @Parity, @StopBits, @BarcodePrefix, @SKULength,
                     @AutoDetect, @DefaultPrintType,@Divisionfactor, @Status, @CompanyID, @CreationUserID, @CreationDate)";

                clsSQL clsSQL = new clsSQL();
                if (trn == null)
                {
                    return Simulate.Integer32(clsSQL.ExecuteScalar(query, prm, clsSQL.CreateDataBaseConnectionString(CompanyID)));
                }
                else
                {
                    return Simulate.Integer32(clsSQL.ExecuteScalar(query, prm, clsSQL.CreateDataBaseConnectionString(CompanyID), trn));
                }
            }
            catch (Exception)
            {
                throw;
            }
        }

        public int UpdateScale(
            int ID, string ScaleName, string ScaleType, string ConnectionType, string PortName,
            int BaudRate, int DataBits, string Parity, int StopBits, string BarcodePrefix,int SKULength,
            bool AutoDetect, string DefaultPrintType,int Divisionfactor, bool Status, int ModificationUserID, int CompanyID,
            SqlTransaction trn = null)
        {
            try
            {
                SqlParameter[] prm =
                {
                    new SqlParameter("@ID", SqlDbType.Int) { Value = ID },
                    new SqlParameter("@ScaleName", SqlDbType.NVarChar, -1) { Value = ScaleName },
                    new SqlParameter("@ScaleType", SqlDbType.NVarChar, -1) { Value = ScaleType },
                    new SqlParameter("@ConnectionType", SqlDbType.NVarChar, -1) { Value = ConnectionType },
                    new SqlParameter("@PortName", SqlDbType.NVarChar, -1) { Value = PortName },
                    new SqlParameter("@BaudRate", SqlDbType.Int) { Value = BaudRate },
                    new SqlParameter("@DataBits", SqlDbType.Int) { Value = DataBits },
                       new SqlParameter("@SKULength", SqlDbType.Int) { Value = SKULength },
                    
                    new SqlParameter("@Parity", SqlDbType.NVarChar, -1) { Value = Parity },
                    new SqlParameter("@StopBits", SqlDbType.Int) { Value = StopBits },
                    new SqlParameter("@BarcodePrefix", SqlDbType.NVarChar, -1) { Value = BarcodePrefix },
                    new SqlParameter("@AutoDetect", SqlDbType.Bit) { Value = AutoDetect },
                    new SqlParameter("@DefaultPrintType", SqlDbType.NVarChar, -1) { Value = DefaultPrintType },
        new SqlParameter("@Divisionfactor", SqlDbType.Int) { Value = Divisionfactor },
                    new SqlParameter("@Status", SqlDbType.Bit) { Value = Status },
                    
                    new SqlParameter("@ModificationUserID", SqlDbType.Int) { Value = ModificationUserID },
                    new SqlParameter("@ModificationDate", SqlDbType.DateTime) { Value = DateTime.Now },
                };

                string query = @"
                    UPDATE tbl_POSScaleConfiguration SET 
                        ScaleName = @ScaleName,
                        ScaleType = @ScaleType,
                        ConnectionType = @ConnectionType,
                        PortName = @PortName,
                        BaudRate = @BaudRate,
                        DataBits = @DataBits,
                        Parity = @Parity,
                        StopBits = @StopBits,
                        BarcodePrefix = @BarcodePrefix,
                        SKULength=@SKULength,
                        AutoDetect = @AutoDetect,
                        DefaultPrintType = @DefaultPrintType,
                        Divisionfactor=@Divisionfactor,
                        Status = @Status,
                        ModificationUserID = @ModificationUserID,
                        ModificationDate = @ModificationDate
                    WHERE ID = @ID";

                clsSQL clsSQL = new clsSQL();
                if (trn == null)
                {
                    return clsSQL.ExecuteNonQueryStatement(query, clsSQL.CreateDataBaseConnectionString(CompanyID), prm);
                }
                else
                {
                    return clsSQL.ExecuteNonQueryStatement(query, clsSQL.CreateDataBaseConnectionString(CompanyID), prm, trn);
                }
            }
            catch (Exception)
            {
                throw;
            }
        }
    }
}
