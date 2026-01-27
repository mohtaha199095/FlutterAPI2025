using Microsoft.Data.SqlClient;
using Newtonsoft.Json.Linq;
using System;
using System.Data;

namespace WebApplication2.cls
{
    public class clsItemAdditionalBarcode
    {
        public int ReplaceForItem(string ItemGuid, JArray barcodes, int CompanyID, int UserId, SqlTransaction trn = null)
        {
            DeactivateByItemGuid(ItemGuid, CompanyID, trn);

            if (barcodes == null || barcodes.Count == 0) return 0;

            int inserted = 0;

            foreach (JObject b in barcodes)
            {
                Insert(
                    ItemGuid: ItemGuid,
                    Barcode: (string)b["barcode"] ?? "",
                    UOMID: (int?)b["uomId"] ?? (int?)b["UOMID"] ?? 0,
                    IsDefault: (bool?)b["isDefault"] ?? false,
                    IsActive: (bool?)b["isActive"] ?? true,
                    CompanyID: CompanyID,
                    CreationUserId: UserId,
                    trn: trn
                );

                inserted++;
            }

            return inserted;
        }
        public DataTable SelectByItemGuid(string ItemGuid, int CompanyID)
        {
            try
            {
                SqlParameter[] prm =
                {
                    new SqlParameter("@ItemGuid", SqlDbType.NVarChar,-1) { Value = ItemGuid ?? "" },
                    new SqlParameter("@CompanyID", SqlDbType.Int) { Value = CompanyID },
                };

                clsSQL clsSQL = new clsSQL();
                return clsSQL.ExecuteQueryStatement(@"
select * from tbl_ItemBarcodes
where (ItemGuid=@ItemGuid or @ItemGuid='')
  and (CompanyID=@CompanyID or @CompanyID=0)
order by IsDefault desc, Barcode
", clsSQL.CreateDataBaseConnectionString(CompanyID), prm);
            }
            catch (Exception)
            {
                throw;
            }
        }

        public int DeactivateByItemGuid(string ItemGuid, int CompanyID, SqlTransaction trn = null)
        {
            try
            {
                clsSQL clsSQL = new clsSQL();
                SqlParameter[] prm =
                {
                    new SqlParameter("@ItemGuid", SqlDbType.NVarChar,-1) { Value = ItemGuid ?? "" },
                    new SqlParameter("@CompanyID", SqlDbType.Int) { Value = CompanyID },
                };

                //string sql = @"update tbl_ItemBarcodes set IsActive=0
                //               where CompanyID=@CompanyID and ItemGuid=@ItemGuid";
                string sql = @"delete from  tbl_ItemBarcodes 
                               where CompanyID=@CompanyID and ItemGuid=@ItemGuid";
                if (trn == null)
                    return clsSQL.ExecuteNonQueryStatement(sql, clsSQL.CreateDataBaseConnectionString(CompanyID), prm);
                else
                    return clsSQL.ExecuteNonQueryStatement(sql, clsSQL.CreateDataBaseConnectionString(CompanyID), prm, trn);
            }
            catch (Exception)
            {
                throw;
            }
        }

        public string Insert(
            string ItemGuid,
            string Barcode,
            int UOMID,
            bool IsDefault,
            bool IsActive,
            int CompanyID,
            int CreationUserId,
            SqlTransaction trn = null)
        {
            try
            {
                SqlParameter[] prm =
                {
                    new SqlParameter("@ItemGuid", SqlDbType.NVarChar,-1) { Value = ItemGuid ?? "" },
                    new SqlParameter("@Barcode", SqlDbType.NVarChar,-1) { Value = Barcode ?? "" },
                    new SqlParameter("@UOMID", SqlDbType.Int) { Value = UOMID },
                    new SqlParameter("@IsDefault", SqlDbType.Bit) { Value = IsDefault },
                    new SqlParameter("@IsActive", SqlDbType.Bit) { Value = IsActive },
                    new SqlParameter("@CompanyID", SqlDbType.Int) { Value = CompanyID },
                    new SqlParameter("@CreationUserId", SqlDbType.Int) { Value = CreationUserId },
                    new SqlParameter("@CreationDate", SqlDbType.DateTime) { Value = DateTime.Now },
                };

                string sql = @"
insert into tbl_ItemBarcodes
( ItemGuid, Barcode, UOMID, IsDefault, IsActive, CompanyID, CreationUserId, CreationDate)
OUTPUT INSERTED.id
values
( @ItemGuid, @Barcode, @UOMID, @IsDefault, @IsActive, @CompanyID, @CreationUserId, @CreationDate)
";

                clsSQL clsSQL = new clsSQL();
                if (trn == null)
                    return Convert.ToString(clsSQL.ExecuteScalar(sql, prm, clsSQL.CreateDataBaseConnectionString(CompanyID)));
                else
                    return Convert.ToString(clsSQL.ExecuteScalar(sql, prm, clsSQL.CreateDataBaseConnectionString(CompanyID), trn));
            }
            catch (Exception)
            {
                throw;
            }
        }
    }
}
