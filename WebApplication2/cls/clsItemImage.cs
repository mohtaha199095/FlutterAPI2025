using Microsoft.Data.SqlClient;
using Newtonsoft.Json.Linq;
using System;
using System.Data;

namespace WebApplication2.cls
{
    public class clsItemImage
    {
        public int ReplaceForItem(string ItemGuid, JArray images, int CompanyID, int UserId, SqlTransaction trn = null)
        {
            DeactivateByItemGuid(ItemGuid, CompanyID, trn);

            if (images == null || images.Count == 0) return 0;

            int inserted = 0;
            int sort = 0;

            foreach (JObject im in images)
            {
                sort++;

                // Expect either:
                // 1) "imageBase64": "data:image/png;base64,...."
                // 2) "base64": "...."
                // 3) "imageData": "...."
                string base64 =
                    (string)im["imageBase64"] ??
                    (string)im["base64"] ??
                    (string)im["imageData"] ??
                    "";

                byte[] bytes = DecodeBase64ImageSafe(base64);
                if (bytes == null || bytes.Length == 0) continue; // skip bad image

                Insert(
                    ItemGuid: ItemGuid,
                    ImageData: bytes,
                    SortOrder: (int?)im["sortOrder"] ?? sort,
                    IsDefault: (bool?)im["isDefault"] ?? false,
                    IsActive: (bool?)im["isActive"] ?? true,
                    CompanyID: CompanyID,
                    CreationUserId: UserId,
                    trn: trn
                );

                inserted++;
            }

            return inserted;
        }

        private byte[] DecodeBase64ImageSafe(string base64)
        {
            if (string.IsNullOrWhiteSpace(base64)) return new byte[0];

            base64 = base64.Trim();

            // handle data URL prefix
            int commaIndex = base64.IndexOf(",");
            if (commaIndex >= 0 && base64.StartsWith("data:", StringComparison.OrdinalIgnoreCase))
                base64 = base64.Substring(commaIndex + 1);

            // handle quoted string
            if (base64.StartsWith("\"") && base64.EndsWith("\""))
                base64 = base64.Substring(1, base64.Length - 2);

            try
            {
                return Convert.FromBase64String(base64);
            }
            catch
            {
               return new byte[0]; }
        
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
select
    Id,
    ItemGuid,
    SortOrder,
    IsDefault,
    IsActive,
    CompanyID,
    CreationUserId,
    CreationDate,
    ImageBase64 = case 
        when ImageData is null then ''
        else CAST('' as xml).value('xs:base64Binary(sql:column(""ImageData""))', 'varchar(max)')
    end
from tbl_ItemImages
where (ItemGuid=@ItemGuid or @ItemGuid='')
  and (CompanyID=@CompanyID or @CompanyID=0)
order by SortOrder
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

                //string sql = @"update tbl_ItemImages set IsActive=0
                //               where CompanyID=@CompanyID and ItemGuid=@ItemGuid";
                string sql = @"delete from  tbl_ItemImages 
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
            byte[] ImageData,
            int SortOrder,
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
                    new SqlParameter("@ItemGuid", SqlDbType.UniqueIdentifier) { Value =Simulate.Guid( ItemGuid ) },
                    new SqlParameter("@ImageData", SqlDbType.VarBinary,-1) { Value = (object)ImageData ?? DBNull.Value },
                    new SqlParameter("@SortOrder", SqlDbType.Int) { Value = SortOrder },
                    new SqlParameter("@IsDefault", SqlDbType.Bit) { Value = IsDefault },
                    new SqlParameter("@IsActive", SqlDbType.Bit) { Value = IsActive },
                    new SqlParameter("@CompanyID", SqlDbType.Int) { Value = CompanyID },
                    new SqlParameter("@CreationUserId", SqlDbType.Int) { Value = CreationUserId },
                    new SqlParameter("@CreationDate", SqlDbType.DateTime) { Value = DateTime.Now },
                };

                string sql = @"
insert into tbl_ItemImages
( ItemGuid, ImageData, SortOrder, IsDefault, IsActive, CompanyID, CreationUserId, CreationDate)
OUTPUT INSERTED.id
values
( @ItemGuid, @ImageData, @SortOrder, @IsDefault, @IsActive, @CompanyID, @CreationUserId, @CreationDate)
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
