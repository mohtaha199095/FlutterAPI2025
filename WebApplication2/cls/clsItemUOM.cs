using Microsoft.Data.SqlClient;
using Newtonsoft.Json.Linq;
using System;
using System.Data;

namespace WebApplication2.cls
{
    public class clsItemUOM
    {
        public int ReplaceForItem(string ItemGuid, JArray uoms, int CompanyID, int UserId, SqlTransaction trn = null)
        {
            DeactivateByItemGuid(ItemGuid, CompanyID, trn);

            if (uoms == null || uoms.Count == 0) return 0;

            int inserted = 0;

            foreach (JObject u in uoms)
            {
                InsertItemUOM(
                    ItemGuid: ItemGuid,
                    FromUOMID: (int?)u["fromUOMID"] ?? (int?)u["fromUomId"] ?? 0,
                    ToUOMID: (int?)u["toUOMID"] ?? (int?)u["toUomId"] ?? 0,
                    Factor: (decimal?)u["factor"] ?? 1,
                    IsDefaultSales: (bool?)u["isDefaultSales"] ?? false,
                    IsDefaultPurchase: (bool?)u["isDefaultPurchase"] ?? false,
                    IsActive: (bool?)u["isActive"] ?? true,
                    CompanyID: CompanyID,
                    CreationUserId: UserId,
                    trn: trn
                );

                inserted++;
            }

            return inserted;
        }
        public DataTable SelectItemUOMByGuid(string Guid, string ItemGuid, int CompanyID)
        {
            try
            {
                SqlParameter[] prm =
                {
                    new SqlParameter("@Guid", SqlDbType.NVarChar,-1) { Value = Guid ?? "" },
                    new SqlParameter("@ItemGuid", SqlDbType.NVarChar,-1) { Value = ItemGuid ?? "" },
                    new SqlParameter("@CompanyID", SqlDbType.Int) { Value = CompanyID },
                };

                clsSQL clsSQL = new clsSQL();
                return clsSQL.ExecuteQueryStatement(@"
select * from tbl_ItemUOM
where  (ItemGuid=@ItemGuid or @ItemGuid='00000000-0000-0000-0000-000000000000')
  and (CompanyID=@CompanyID or @CompanyID=0)
order by IsDefaultSales desc, IsDefaultPurchase desc
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

                //string sql = @"update tbl_ItemUOM set IsActive=0
                //               where CompanyID=@CompanyID and ItemGuid=@ItemGuid";
                string sql = @"delete from  tbl_ItemUOM  
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

        public string InsertItemUOM(
            string ItemGuid,
            int FromUOMID,
            int ToUOMID,
            decimal Factor,
            bool IsDefaultSales,
            bool IsDefaultPurchase,
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
                    new SqlParameter("@FromUOMID", SqlDbType.Int) { Value = FromUOMID },
                    new SqlParameter("@ToUOMID", SqlDbType.Int) { Value = ToUOMID },
                    new SqlParameter("@Factor", SqlDbType.Decimal) { Value = Factor },
                    new SqlParameter("@IsDefaultSales", SqlDbType.Bit) { Value = IsDefaultSales },
                    new SqlParameter("@IsDefaultPurchase", SqlDbType.Bit) { Value = IsDefaultPurchase },
                    new SqlParameter("@IsActive", SqlDbType.Bit) { Value = IsActive },
                    new SqlParameter("@CompanyID", SqlDbType.Int) { Value = CompanyID },
                    new SqlParameter("@CreationUserId", SqlDbType.Int) { Value = CreationUserId },
                    new SqlParameter("@CreationDate", SqlDbType.DateTime) { Value = DateTime.Now },
                };

                string sql = @"
insert into tbl_ItemUOM
( ItemGuid, FromUOMID, ToUOMID, Factor, IsDefaultSales, IsDefaultPurchase,
 IsActive, CompanyID, CreationUserId, CreationDate)
OUTPUT INSERTED.ID
values
( @ItemGuid, @FromUOMID, @ToUOMID, @Factor, @IsDefaultSales, @IsDefaultPurchase,
 @IsActive, @CompanyID, @CreationUserId, @CreationDate)
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
