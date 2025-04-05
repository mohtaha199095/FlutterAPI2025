using System;
using System.Data;
using Microsoft.Data.SqlClient;

namespace WebApplication2.cls
{
    public class clsInvoiceDetailsLotsTracking
    {
        public DataTable SelectInvoiceDetailsLotsTracking(  Guid InvoiceDetailsGuid, Guid ItemGuid,int InvoiceType
            ,string LotNumber, DateTime date1,DateTime date2,
            int CompanyID)
        {
            try
            {
                SqlParameter[] prm =
                {
                    new SqlParameter("@ItemGuid", SqlDbType.UniqueIdentifier) { Value = ItemGuid },
                    new SqlParameter("@InvoiceDetailsGuid", SqlDbType.UniqueIdentifier) { Value = InvoiceDetailsGuid },
        new SqlParameter("@InvoiceType", SqlDbType.Int) { Value = InvoiceType },
         new SqlParameter("@LotNumber", SqlDbType.VarChar) { Value = LotNumber },
          new SqlParameter("@date1", SqlDbType.DateTime) { Value = date1 },
           new SqlParameter("@date2", SqlDbType.DateTime) { Value = date2 },
                    new SqlParameter("@CompanyID", SqlDbType.Int) { Value = CompanyID }
                };

                clsSQL clsSQL = new clsSQL();
                DataTable dt = clsSQL.ExecuteQueryStatement(@"SELECT 

cast(tbl_InvoiceHeader.InvoiceDate as date) as InvoiceDate,
tbl_InvoiceHeader.InvoiceNo , 
tbl_InvoiceDetailsLotsTracking.LotNumber , tbl_InvoiceDetailsLotsTracking.ExpiryDate ,  tbl_InvoiceDetailsLotsTracking.QTY *tbl_JournalVoucherTypes.QTYFactor as QTY ,
tbl_JournalVoucherTypes.AName as JournalVoucherTypesAName,
tbl_Branch.AName as BranchAName,
tbl_Store.AName as StoreAName ,
tbl_BusinessPartner.AName as BusinessPartnerAName,
tbl_Items.AName as ItemsAName,
      (
        SELECT STUFF((
            SELECT '"",""' + s.SerialNumber
            FROM tbl_InvoiceDetailsLotsSerialNumber s
            WHERE s.lotguid = tbl_InvoiceDetailsLotsTracking.guid
            FOR XML PATH(''), TYPE
        ).value('.', 'NVARCHAR(MAX)'), 1, 2, '')
    )+'""' AS SerialNumbersList
 FROM tbl_InvoiceDetailsLotsTracking
left join tbl_InvoiceHeader on tbl_InvoiceHeader.Guid = tbl_InvoiceDetailsLotsTracking.InvoiceGuid
left join tbl_JournalVoucherTypes on tbl_JournalVoucherTypes.id = tbl_InvoiceHeader.InvoiceTypeID
left join tbl_Branch on tbl_Branch.id = tbl_InvoiceHeader.BranchID
left join tbl_Store on tbl_Store.id = tbl_InvoiceHeader.StoreID
left join tbl_BusinessPartner on tbl_BusinessPartner.id = tbl_InvoiceHeader.BusinessPartnerID
left join tbl_InvoiceDetails on tbl_InvoiceDetails.Guid = tbl_InvoiceDetailsLotsTracking.InvoiceDetailsGuid
left join tbl_Items on tbl_InvoiceDetails.ItemGuid = tbl_Items.Guid
where (tbl_InvoiceDetailsLotsTracking.ItemGuid =@ItemGuid or @ItemGuid='00000000-0000-0000-0000-000000000000')
and (InvoiceDetailsGuid =@InvoiceDetailsGuid or @InvoiceDetailsGuid='00000000-0000-0000-0000-000000000000')
and (InvoiceType=@InvoiceType or @InvoiceType=0 )
and (LotNumber =@LotNumber or @LotNumber ='')
and (tbl_InvoiceDetailsLotsTracking.CompanyID = @CompanyID OR @CompanyID = 0)
and tbl_InvoiceHeader.InvoiceDate between @date1 and @date2
 
 ",
                    clsSQL.CreateDataBaseConnectionString(CompanyID), prm);

                return dt;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public bool DeleteInvoiceDetailsLotsTrackingByGuid(Guid InvoiceGuid, int CompanyID)
        {
            try
            {
                clsSQL clsSQL = new clsSQL();

                SqlParameter[] prm =
                {
                    new SqlParameter("@InvoiceGuid", SqlDbType.UniqueIdentifier) { Value = InvoiceGuid }
                };

                int rowsAffected = clsSQL.ExecuteNonQueryStatement(@"DELETE FROM tbl_InvoiceDetailsLotsTracking WHERE InvoiceGuid = @InvoiceGuid",
                    clsSQL.CreateDataBaseConnectionString(CompanyID), prm);

                return rowsAffected > 0;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public string InsertInvoiceDetailsLotsTracking(Guid InvoiceDetailsGuid, Guid ItemGuid, int InvoiceType, Guid InvoiceGuid, string LotNumber, DateTime ExpiryDate, decimal QTY, int CompanyID, int CreationUserID, SqlTransaction trn = null)
        {
            try
            {
                SqlParameter[] prm =
                {
                    new SqlParameter("@InvoiceDetailsGuid", SqlDbType.UniqueIdentifier) { Value = InvoiceDetailsGuid },
                    new SqlParameter("@ItemGuid", SqlDbType.UniqueIdentifier) { Value = ItemGuid },
                    new SqlParameter("@InvoiceType", SqlDbType.Int) { Value = InvoiceType },
                    new SqlParameter("@InvoiceGuid", SqlDbType.UniqueIdentifier) { Value = InvoiceGuid },
                    new SqlParameter("@LotNumber", SqlDbType.NVarChar, -1) { Value = LotNumber },
                    new SqlParameter("@ExpiryDate", SqlDbType.DateTime) { Value = ExpiryDate },
                    new SqlParameter("@QTY", SqlDbType.Decimal) { Value = QTY },
                    new SqlParameter("@CompanyID", SqlDbType.Int) { Value = CompanyID },
                    new SqlParameter("@CreationUserID", SqlDbType.Int) { Value = CreationUserID },
                    new SqlParameter("@CreationDate", SqlDbType.DateTime) { Value = DateTime.Now }
                };

                string query = @"INSERT INTO tbl_InvoiceDetailsLotsTracking 
                    (Guid, InvoiceDetailsGuid, ItemGuid, InvoiceType, InvoiceGuid, LotNumber, ExpiryDate, QTY, CompanyID, CreationUserID, CreationDate)
                    OUTPUT INSERTED.Guid
                    VALUES (NEWID(), @InvoiceDetailsGuid, @ItemGuid, @InvoiceType, @InvoiceGuid, @LotNumber, @ExpiryDate, @QTY, @CompanyID, @CreationUserID, @CreationDate)";

                clsSQL clsSQL = new clsSQL();

                if (trn == null)
                {
                    return Simulate.String(clsSQL.ExecuteScalar(query, prm, clsSQL.CreateDataBaseConnectionString(CompanyID)));
                }
                else
                {
                    return Simulate.String(clsSQL.ExecuteScalar(query, prm, clsSQL.CreateDataBaseConnectionString(CompanyID), trn));
                }
            }
            catch (Exception)
            {
                throw;
            }
        }

        
    }
}
