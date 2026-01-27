using Microsoft.Data.SqlClient;
using Newtonsoft.Json.Linq;
using System;
using System.Data;

namespace WebApplication2.cls
{
    public class clsItemReorder
    {
        public int ReplaceForItem(string ItemGuid, JArray policies, int CompanyID, int UserId, SqlTransaction trn = null)
        {
            DeactivateByItemGuid(ItemGuid, CompanyID, trn);

            if (policies == null || policies.Count == 0) return 0;

            int inserted = 0;

            foreach (JObject p in policies)
            {
                Insert(
                    ItemGuid: ItemGuid,
                    WarehouseID: (int?)p["warehouseId"] ?? (int?)p["WarehouseID"] ?? 0,
                    PolicyType: (int?)p["policyType"] ?? (int?)p["PolicyType"] ?? 0,
                    ReorderPointQty: (decimal?)p["reorderPointQty"] ?? 0,
                    ReorderQty: (decimal?)p["reorderQty"] ?? 0,
                    MinQty: (decimal?)p["minQty"] ?? 0,
                    MaxQty: (decimal?)p["maxQty"] ?? 0,
                    SafetyStockQty: (decimal?)p["safetyStockQty"] ?? 0,
                    IsActive: (bool?)p["isActive"] ?? true,
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
select * from tbl_ItemReorder
where (ItemGuid=@ItemGuid or @ItemGuid='')
  and (CompanyID=@CompanyID or @CompanyID=0)
order by WarehouseID
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

                //string sql = @"update tbl_ItemReorder set IsActive=0
                //               where CompanyID=@CompanyID and ItemGuid=@ItemGuid";
                string sql = @"delete from  tbl_ItemReorder  
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
            int WarehouseID,
            int PolicyType,
            decimal ReorderPointQty,
            decimal ReorderQty,
            decimal MinQty,
            decimal MaxQty,
            decimal SafetyStockQty,
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
                    new SqlParameter("@WarehouseID", SqlDbType.Int) { Value = WarehouseID },
                    new SqlParameter("@PolicyType", SqlDbType.Int) { Value = PolicyType },
                    new SqlParameter("@ReorderPointQty", SqlDbType.Decimal) { Value = ReorderPointQty },
                    new SqlParameter("@ReorderQty", SqlDbType.Decimal) { Value = ReorderQty },
                    new SqlParameter("@MinQty", SqlDbType.Decimal) { Value = MinQty },
                    new SqlParameter("@MaxQty", SqlDbType.Decimal) { Value = MaxQty },
                    new SqlParameter("@SafetyStockQty", SqlDbType.Decimal) { Value = SafetyStockQty },
                    new SqlParameter("@IsActive", SqlDbType.Bit) { Value = IsActive },
                    new SqlParameter("@CompanyID", SqlDbType.Int) { Value = CompanyID },
                    new SqlParameter("@CreationUserId", SqlDbType.Int) { Value = CreationUserId },
                    new SqlParameter("@CreationDate", SqlDbType.DateTime) { Value = DateTime.Now },
                };

                string sql = @"
insert into tbl_ItemReorder
( ItemGuid, WarehouseID, PolicyType, ReorderPointQty, ReorderQty, MinQty, MaxQty, SafetyStockQty,
 IsActive, CompanyID, CreationUserId, CreationDate)
OUTPUT INSERTED.id
values
( @ItemGuid, @WarehouseID, @PolicyType, @ReorderPointQty, @ReorderQty, @MinQty, @MaxQty, @SafetyStockQty,
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
