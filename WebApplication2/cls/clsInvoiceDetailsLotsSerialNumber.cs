using System;
using System.Data;
using Microsoft.Data.SqlClient;

namespace WebApplication2.cls
{
    public class clsInvoiceDetailsLotsSerialNumber
    {
        public DataTable SelectInvoiceDetailsLotSerialNumber(Guid Guid, Guid InvoiceDetailsGuid, Guid ItemGuid, int CompanyID)
        {
            try
            {
                SqlParameter[] prm =
                {
                    new SqlParameter("@Guid", SqlDbType.UniqueIdentifier) { Value = Guid },
                    new SqlParameter("@InvoiceDetailsGuid", SqlDbType.UniqueIdentifier) { Value = InvoiceDetailsGuid },
                    new SqlParameter("@ItemGuid", SqlDbType.UniqueIdentifier) { Value = ItemGuid },
                    new SqlParameter("@CompanyID", SqlDbType.Int) { Value = CompanyID }
                };

                clsSQL clsSQL = new clsSQL();
                DataTable dt = clsSQL.ExecuteQueryStatement(@"SELECT * FROM tbl_InvoiceDetailsLotsSerialNumber WHERE 
                    (Guid = @Guid OR @Guid = '00000000-0000-0000-0000-000000000000') AND
                    (InvoiceDetailsGuid = @InvoiceDetailsGuid OR @InvoiceDetailsGuid = '00000000-0000-0000-0000-000000000000') AND
                    (ItemGuid = @ItemGuid OR @ItemGuid = '00000000-0000-0000-0000-000000000000') AND
                    (CompanyID = @CompanyID OR @CompanyID = 0)",
                    clsSQL.CreateDataBaseConnectionString(CompanyID), prm);

                return dt;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public bool DeleteInvoiceDetailsLotSerialNumberByGuid(Guid InvoiceGuid, int CompanyID)
        {
            try
            {
                clsSQL clsSQL = new clsSQL();

                SqlParameter[] prm =
                {
                    new SqlParameter("@InvoiceGuid", SqlDbType.UniqueIdentifier) { Value = InvoiceGuid }
                };

                int rowsAffected = clsSQL.ExecuteNonQueryStatement(@"DELETE FROM tbl_InvoiceDetailsLotsSerialNumber WHERE InvoiceGuid = @InvoiceGuid",
                    clsSQL.CreateDataBaseConnectionString(CompanyID), prm);

                return rowsAffected > 0;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public int InsertInvoiceDetailsLotSerialNumber(Guid InvoiceDetailsGuid, Guid ItemGuid, 
            int InvoiceType, Guid InvoiceGuid, Guid LotGuid, string SerialNumber, bool Status, int CompanyID,
            int CreationUserID, SqlTransaction trn = null)
        {
            try
            {
                SqlParameter[] prm =
                {
                    new SqlParameter("@InvoiceDetailsGuid", SqlDbType.UniqueIdentifier) { Value = InvoiceDetailsGuid },
                    new SqlParameter("@ItemGuid", SqlDbType.UniqueIdentifier) { Value = ItemGuid },
                    new SqlParameter("@InvoiceType", SqlDbType.Int) { Value = InvoiceType },
                    new SqlParameter("@InvoiceGuid", SqlDbType.UniqueIdentifier) { Value = InvoiceGuid },
                    new SqlParameter("@LotGuid", SqlDbType.UniqueIdentifier) { Value = LotGuid },
                    new SqlParameter("@SerialNumber", SqlDbType.NVarChar, -1) { Value = SerialNumber },
                    new SqlParameter("@Status", SqlDbType.Bit) { Value = Status },
                    new SqlParameter("@CompanyID", SqlDbType.Int) { Value = CompanyID },
                    new SqlParameter("@CreationUserID", SqlDbType.Int) { Value = CreationUserID },
                    new SqlParameter("@CreationDate", SqlDbType.DateTime) { Value = DateTime.Now }
                };

                string query = @"INSERT INTO tbl_InvoiceDetailsLotsSerialNumber 
                    (Guid, InvoiceDetailsGuid, ItemGuid, InvoiceType, InvoiceGuid, LotGuid, SerialNumber, Status, CompanyID, CreationUserID, CreationDate)
                    OUTPUT INSERTED.Guid
                    VALUES (NEWID(), @InvoiceDetailsGuid, @ItemGuid, @InvoiceType, @InvoiceGuid, @LotGuid, @SerialNumber, @Status, @CompanyID, @CreationUserID, @CreationDate)";

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

        
    }
}
