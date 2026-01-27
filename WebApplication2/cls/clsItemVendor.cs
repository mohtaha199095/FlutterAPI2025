using DocumentFormat.OpenXml.Wordprocessing;
using Microsoft.Data.SqlClient;
using Newtonsoft.Json.Linq;
using System;
using System.Data;

namespace WebApplication2.cls
{
    public class clsItemVendor
    {
        public int ReplaceForItem(string ItemGuid, JArray vendors, int CompanyID, int UserId, SqlTransaction trn = null)
        {
            // Always deactivate old rows first (your pattern)
            DeactivateByItemGuid(ItemGuid, CompanyID, trn);

            if (vendors == null || vendors.Count == 0) return 0;

            int inserted = 0;

            foreach (JObject v in vendors)
            {
                InsertItemVendor(
                    ItemGuid: ItemGuid,
                    VendorID: (int?)v["vendorId"] ?? 0,
                    VendorItemCode: (string)v["vendorItemCode"] ?? "",
                    LeadTimeDays: (int?)v["leadTimeDays"] ?? 0,
                    MinOrderQty: (decimal?)v["minOrderQty"] ?? 0,
                    OrderMultipleQty: (decimal?)v["orderMultipleQty"] ?? 0,
                    IsPreferred: (bool?)v["isPreferred"] ?? false,
                    IsActive: (bool?)v["isActive"] ?? true, // default true
                    CompanyID: CompanyID,
                    CreationUserId: UserId,
                    trn: trn
                );

                inserted++;
            }

            return inserted;
        }
        public DataTable SelectItemVendorByGuid(string Guid, string ItemGuid, int VendorID,int IsActive, int CompanyID)
        {
            try
            {
                SqlParameter[] prm =
                {
                    new SqlParameter("@Guid", SqlDbType.NVarChar,-1) { Value = Guid ?? "" },
                    new SqlParameter("@ItemGuid", SqlDbType.NVarChar,-1) { Value = ItemGuid ?? "" },
                    new SqlParameter("@VendorID", SqlDbType.Int) { Value = VendorID },
                    new SqlParameter("@CompanyID", SqlDbType.Int) { Value = CompanyID },   
                    new SqlParameter("@IsActive", SqlDbType.Int) { Value = IsActive },  
                };

                clsSQL clsSQL = new clsSQL();
                DataTable dt = clsSQL.ExecuteQueryStatement(@"
select * from tbl_ItemVendor
where  (ItemGuid=@ItemGuid or @ItemGuid='')
  and (VendorID=@VendorID or @VendorID=0)
  and (CompanyID=@CompanyID or @CompanyID=0)
  and (isactive=@isactive or @IsActive=-1)
order by IsPreferred desc, VendorID
", clsSQL.CreateDataBaseConnectionString(CompanyID), prm);

                return dt;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public bool DeleteItemVendorByGuid(string Guid, int CompanyID)
        {
            try
            {
                clsSQL clsSQL = new clsSQL();
                SqlParameter[] prm =
                {
                    new SqlParameter("@Guid", SqlDbType.NVarChar,-1) { Value = Guid ?? "" },
                };

                clsSQL.ExecuteNonQueryStatement(@"delete from tbl_ItemVendor where Guid=@Guid", clsSQL.CreateDataBaseConnectionString(CompanyID), prm);
                return true;
            }
            catch (Exception)
            {
                throw;
            }
        }

        // useful for "replaceForItem": deactivate all then insert
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

                //string sql = @"update tbl_ItemVendor set IsActive=0
                //               where CompanyID=@CompanyID and ItemGuid=@ItemGuid";
                string sql = @"delete from tbl_ItemVendor  
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

        public string InsertItemVendor(
            string ItemGuid,
            int VendorID,
            string VendorItemCode,
            int LeadTimeDays,
            decimal MinOrderQty,
            decimal OrderMultipleQty,
            bool IsPreferred,
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
                    new SqlParameter("@VendorID", SqlDbType.Int) { Value = VendorID },
                    new SqlParameter("@VendorItemCode", SqlDbType.NVarChar,-1) { Value = VendorItemCode ?? "" },
                    new SqlParameter("@LeadTimeDays", SqlDbType.Int) { Value = LeadTimeDays },
                    new SqlParameter("@MinOrderQty", SqlDbType.Decimal) { Value = MinOrderQty },
                    new SqlParameter("@OrderMultipleQty", SqlDbType.Decimal) { Value = OrderMultipleQty },
                    new SqlParameter("@IsPreferred", SqlDbType.Bit) { Value = IsPreferred },
                    new SqlParameter("@IsActive", SqlDbType.Bit) { Value = IsActive },
                    new SqlParameter("@CompanyID", SqlDbType.Int) { Value = CompanyID },
                    new SqlParameter("@CreationUserId", SqlDbType.Int) { Value = CreationUserId },
                    new SqlParameter("@CreationDate", SqlDbType.DateTime) { Value = DateTime.Now },
                };

                string sql = @"
insert into tbl_ItemVendor
( ItemGuid, VendorID, VendorItemCode, LeadTimeDays, MinOrderQty, OrderMultipleQty,
 IsPreferred, IsActive, CompanyID, CreationUserId, CreationDate)
OUTPUT INSERTED.ItemGuid
values
( @ItemGuid, @VendorID, @VendorItemCode, @LeadTimeDays, @MinOrderQty, @OrderMultipleQty,
 @IsPreferred, @IsActive, @CompanyID, @CreationUserId, @CreationDate)
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

        public int UpdateItemVendor(
            string Guid,
            int VendorID,
            string VendorItemCode,
            int LeadTimeDays,
            decimal MinOrderQty,
            decimal OrderMultipleQty,
            bool IsPreferred,
            bool IsActive,
            int CompanyID,
            int ModificationUserId)
        {
            try
            {
                clsSQL clsSQL = new clsSQL();

                SqlParameter[] prm =
                {
                    new SqlParameter("@Guid", SqlDbType.NVarChar,-1) { Value = Guid ?? "" },
                    new SqlParameter("@VendorID", SqlDbType.Int) { Value = VendorID },
                    new SqlParameter("@VendorItemCode", SqlDbType.NVarChar,-1) { Value = VendorItemCode ?? "" },
                    new SqlParameter("@LeadTimeDays", SqlDbType.Int) { Value = LeadTimeDays },
                    new SqlParameter("@MinOrderQty", SqlDbType.Decimal) { Value = MinOrderQty },
                    new SqlParameter("@OrderMultipleQty", SqlDbType.Decimal) { Value = OrderMultipleQty },
                    new SqlParameter("@IsPreferred", SqlDbType.Bit) { Value = IsPreferred },
                    new SqlParameter("@IsActive", SqlDbType.Bit) { Value = IsActive },
                    new SqlParameter("@CompanyID", SqlDbType.Int) { Value = CompanyID },
                    new SqlParameter("@ModificationUserId", SqlDbType.Int) { Value = ModificationUserId },
                    new SqlParameter("@ModificationDate", SqlDbType.DateTime) { Value = DateTime.Now },
                };

                int A = clsSQL.ExecuteNonQueryStatement(@"
update tbl_ItemVendor set
 VendorID=@VendorID,
 VendorItemCode=@VendorItemCode,
 LeadTimeDays=@LeadTimeDays,
 MinOrderQty=@MinOrderQty,
 OrderMultipleQty=@OrderMultipleQty,
 IsPreferred=@IsPreferred,
 IsActive=@IsActive,
 ModificationUserId=@ModificationUserId,
 ModificationDate=@ModificationDate
where Guid=@Guid and CompanyID=@CompanyID
", clsSQL.CreateDataBaseConnectionString(CompanyID), prm);

                return A;
            }
            catch (Exception)
            {
                throw;
            }
        }
    }
}
