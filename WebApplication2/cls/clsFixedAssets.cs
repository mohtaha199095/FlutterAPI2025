using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using WebApplication2.MainClasses;
using static WebApplication2.MainClasses.clsEnum;

namespace WebApplication2.cls
{
    /// <summary>
    /// Fixed Assets Phase 1: categories, register, capitalize from PI,
    /// straight-line + declining-balance depreciation runs, disposal.
    /// </summary>
    public class clsFixedAssets
    {
        public const int MethodStraightLine = 1;
        public const int MethodDeclining = 2;

        public const int StatusDraft = 0;
        public const int StatusActive = 1;
        public const int StatusFullyDepreciated = 2;
        public const int StatusDisposed = 3;

        // ---------- Categories ----------
        public DataTable SelectCategories(int id, int companyId, int activeOnly = 0)
        {
            clsSQL sql = new clsSQL();
            string q = @"
SELECT c.*,
       a1.AName AS AssetAccountName, a2.AName AS AccumDepAccountName, a3.AName AS DepExpenseAccountName
FROM tbl_FixedAssetCategory c
LEFT JOIN tbl_Accounts a1 ON a1.ID = c.AssetAccountID
LEFT JOIN tbl_Accounts a2 ON a2.ID = c.AccumDepAccountID
LEFT JOIN tbl_Accounts a3 ON a3.ID = c.DepExpenseAccountID
WHERE c.CompanyID = @CompanyID
  AND (@ID = 0 OR c.ID = @ID)
  AND (@ActiveOnly = 0 OR ISNULL(c.Active,1) = 1)
ORDER BY c.Code, c.Name";
            SqlParameter[] prm =
            {
                new SqlParameter("@ID", SqlDbType.Int) { Value = id },
                new SqlParameter("@CompanyID", SqlDbType.Int) { Value = companyId },
                new SqlParameter("@ActiveOnly", SqlDbType.Int) { Value = activeOnly },
            };
            return sql.ExecuteQueryStatement(q, sql.CreateDataBaseConnectionString(companyId), prm);
        }

        static void ValidateCategoryAccounts(int assetAccountId, int accumDepAccountId, int depExpenseAccountId)
        {
            if (assetAccountId <= 0)
                throw new Exception("Asset account is required on the category.");
            if (accumDepAccountId <= 0)
                throw new Exception("Accumulated depreciation account is required on the category.");
            if (depExpenseAccountId <= 0)
                throw new Exception("Depreciation expense account is required on the category.");
        }

        public int InsertCategory(string code, string name, int usefulLifeMonths, int method,
            decimal decliningRate, int assetAccountId, int accumDepAccountId, int depExpenseAccountId,
            bool active, int companyId, int userId)
        {
            if (string.IsNullOrWhiteSpace(code)) throw new Exception("Category code is required.");
            if (string.IsNullOrWhiteSpace(name)) throw new Exception("Category name is required.");
            ValidateCategoryAccounts(assetAccountId, accumDepAccountId, depExpenseAccountId);

            clsSQL sql = new clsSQL();
            string q = @"
INSERT INTO tbl_FixedAssetCategory
(Code, Name, DefaultUsefulLifeMonths, DefaultDepreciationMethod, DefaultDecliningRate,
 AssetAccountID, AccumDepAccountID, DepExpenseAccountID, Active, CompanyID, CreationUserID, CreationDate)
OUTPUT INSERTED.ID
VALUES
(@Code, @Name, @Life, @Method, @Rate, @AssetAcc, @AccumAcc, @ExpAcc, @Active, @CompanyID, @UserID, GETDATE())";
            SqlParameter[] prm =
            {
                new SqlParameter("@Code", SqlDbType.NVarChar, 50) { Value = Simulate.String(code) },
                new SqlParameter("@Name", SqlDbType.NVarChar, 200) { Value = Simulate.String(name) },
                new SqlParameter("@Life", SqlDbType.Int) { Value = usefulLifeMonths },
                new SqlParameter("@Method", SqlDbType.Int) { Value = method },
                new SqlParameter("@Rate", SqlDbType.Decimal) { Value = decliningRate },
                new SqlParameter("@AssetAcc", SqlDbType.Int) { Value = assetAccountId },
                new SqlParameter("@AccumAcc", SqlDbType.Int) { Value = accumDepAccountId },
                new SqlParameter("@ExpAcc", SqlDbType.Int) { Value = depExpenseAccountId },
                new SqlParameter("@Active", SqlDbType.Bit) { Value = active },
                new SqlParameter("@CompanyID", SqlDbType.Int) { Value = companyId },
                new SqlParameter("@UserID", SqlDbType.Int) { Value = userId },
            };
            return Simulate.Integer32(sql.ExecuteScalar(q, prm, sql.CreateDataBaseConnectionString(companyId)));
        }

        public int UpdateCategory(int id, string code, string name, int usefulLifeMonths, int method,
            decimal decliningRate, int assetAccountId, int accumDepAccountId, int depExpenseAccountId,
            bool active, int companyId, int userId)
        {
            if (string.IsNullOrWhiteSpace(code)) throw new Exception("Category code is required.");
            if (string.IsNullOrWhiteSpace(name)) throw new Exception("Category name is required.");
            ValidateCategoryAccounts(assetAccountId, accumDepAccountId, depExpenseAccountId);

            clsSQL sql = new clsSQL();
            string q = @"
UPDATE tbl_FixedAssetCategory SET
  Code=@Code, Name=@Name, DefaultUsefulLifeMonths=@Life, DefaultDepreciationMethod=@Method,
  DefaultDecliningRate=@Rate, AssetAccountID=@AssetAcc, AccumDepAccountID=@AccumAcc,
  DepExpenseAccountID=@ExpAcc, Active=@Active, ModificationUserID=@UserID, ModificationDate=GETDATE()
WHERE ID=@ID AND CompanyID=@CompanyID";
            SqlParameter[] prm =
            {
                new SqlParameter("@ID", SqlDbType.Int) { Value = id },
                new SqlParameter("@Code", SqlDbType.NVarChar, 50) { Value = Simulate.String(code) },
                new SqlParameter("@Name", SqlDbType.NVarChar, 200) { Value = Simulate.String(name) },
                new SqlParameter("@Life", SqlDbType.Int) { Value = usefulLifeMonths },
                new SqlParameter("@Method", SqlDbType.Int) { Value = method },
                new SqlParameter("@Rate", SqlDbType.Decimal) { Value = decliningRate },
                new SqlParameter("@AssetAcc", SqlDbType.Int) { Value = assetAccountId },
                new SqlParameter("@AccumAcc", SqlDbType.Int) { Value = accumDepAccountId },
                new SqlParameter("@ExpAcc", SqlDbType.Int) { Value = depExpenseAccountId },
                new SqlParameter("@Active", SqlDbType.Bit) { Value = active },
                new SqlParameter("@CompanyID", SqlDbType.Int) { Value = companyId },
                new SqlParameter("@UserID", SqlDbType.Int) { Value = userId },
            };
            return sql.ExecuteNonQueryStatement(q, sql.CreateDataBaseConnectionString(companyId), prm);
        }

        public bool DeleteCategory(int id, int companyId)
        {
            clsSQL sql = new clsSQL();
            object cnt = sql.ExecuteScalar(
                "SELECT COUNT(1) FROM tbl_FixedAsset WHERE CategoryID=@ID AND CompanyID=@CompanyID",
                new[]
                {
                    new SqlParameter("@ID", SqlDbType.Int) { Value = id },
                    new SqlParameter("@CompanyID", SqlDbType.Int) { Value = companyId },
                },
                sql.CreateDataBaseConnectionString(companyId));
            if (Simulate.Integer32(cnt) > 0)
                throw new Exception("Cannot delete category: assets are linked to it.");

            return sql.ExecuteNonQueryStatement(
                "DELETE FROM tbl_FixedAssetCategory WHERE ID=@ID AND CompanyID=@CompanyID",
                sql.CreateDataBaseConnectionString(companyId),
                new[]
                {
                    new SqlParameter("@ID", SqlDbType.Int) { Value = id },
                    new SqlParameter("@CompanyID", SqlDbType.Int) { Value = companyId },
                }) > 0;
        }

        // ---------- Assets ----------
        public DataTable SelectAssets(int id, int companyId, int status = -1, int categoryId = 0, string guid = "")
        {
            clsSQL sql = new clsSQL();
            string q = @"
SELECT a.*,
       c.Code AS CategoryCode, c.Name AS CategoryName,
       b.AName AS BranchName
FROM tbl_FixedAsset a
LEFT JOIN tbl_FixedAssetCategory c ON c.ID = a.CategoryID
LEFT JOIN tbl_Branch b ON b.ID = a.BranchID
WHERE a.CompanyID = @CompanyID
  AND (@ID = 0 OR a.ID = @ID)
  AND (@Status < 0 OR a.Status = @Status)
  AND (@CategoryID = 0 OR a.CategoryID = @CategoryID)
  AND (@Guid = '' OR CONVERT(varchar(36), a.Guid) = @Guid)
  AND ISNULL(a.Active,1) = 1
ORDER BY a.AssetCode, a.Name";
            SqlParameter[] prm =
            {
                new SqlParameter("@ID", SqlDbType.Int) { Value = id },
                new SqlParameter("@CompanyID", SqlDbType.Int) { Value = companyId },
                new SqlParameter("@Status", SqlDbType.Int) { Value = status },
                new SqlParameter("@CategoryID", SqlDbType.Int) { Value = categoryId },
                new SqlParameter("@Guid", SqlDbType.NVarChar, 36) { Value = Simulate.String(guid) },
            };
            return sql.ExecuteQueryStatement(q, sql.CreateDataBaseConnectionString(companyId), prm);
        }

        public int InsertAsset(string assetCode, string name, int categoryId, int branchId, int costCenterId,
            decimal acquisitionCost, decimal salvageValue, int usefulLifeMonths, int method, decimal decliningRate,
            DateTime inServiceDate, int status, string notes, int companyId, int userId,
            int sourceInvoiceHeaderId = 0, int sourceInvoiceDetailsId = 0, string sourceInvoiceGuid = "",
            string sourceInvoiceDetailsGuid = "", SqlTransaction trn = null)
        {
            if (acquisitionCost < 0) throw new Exception("Acquisition cost cannot be negative.");
            if (salvageValue < 0) throw new Exception("Salvage value cannot be negative.");
            if (salvageValue > acquisitionCost) throw new Exception("Salvage value cannot exceed acquisition cost.");
            if (categoryId <= 0) throw new Exception("Asset category is required.");
            if (usefulLifeMonths <= 0) throw new Exception("Useful life (months) must be greater than zero.");
            if (method != MethodStraightLine && method != MethodDeclining)
                throw new Exception("Invalid depreciation method.");
            if (method == MethodDeclining && decliningRate <= 0)
                throw new Exception("Declining rate is required for declining-balance method.");

            decimal nbv = acquisitionCost;
            clsSQL sql = new clsSQL();
            string q = @"
INSERT INTO tbl_FixedAsset
(Guid, AssetCode, Name, CategoryID, BranchID, CostCenterID, AcquisitionCost, SalvageValue,
 UsefulLifeMonths, DepreciationMethod, DecliningRate, InServiceDate,
 SourceInvoiceHeaderID, SourceInvoiceDetailsID, SourceInvoiceGuid, SourceInvoiceDetailsGuid,
 AccumulatedDepreciation, NetBookValue, LastDepreciationPeriod, Status, Notes, Active,
 CompanyID, CreationUserID, CreationDate)
OUTPUT INSERTED.ID
VALUES
(NEWID(), @Code, @Name, @CategoryID, @BranchID, @CostCenterID, @Cost, @Salvage,
 @Life, @Method, @Rate, @InService,
 @SrcHdr, @SrcDet, @SrcGuid, @SrcDetGuid,
 0, @NBV, NULL, @Status, @Notes, 1,
 @CompanyID, @UserID, GETDATE())";
            SqlParameter[] prm =
            {
                new SqlParameter("@Code", SqlDbType.NVarChar, 50) { Value = Simulate.String(assetCode) },
                new SqlParameter("@Name", SqlDbType.NVarChar, 250) { Value = Simulate.String(name) },
                new SqlParameter("@CategoryID", SqlDbType.Int) { Value = categoryId },
                new SqlParameter("@BranchID", SqlDbType.Int) { Value = branchId },
                new SqlParameter("@CostCenterID", SqlDbType.Int) { Value = costCenterId },
                new SqlParameter("@Cost", SqlDbType.Decimal) { Value = acquisitionCost },
                new SqlParameter("@Salvage", SqlDbType.Decimal) { Value = salvageValue },
                new SqlParameter("@Life", SqlDbType.Int) { Value = usefulLifeMonths },
                new SqlParameter("@Method", SqlDbType.Int) { Value = method },
                new SqlParameter("@Rate", SqlDbType.Decimal) { Value = decliningRate },
                new SqlParameter("@InService", SqlDbType.DateTime) { Value = inServiceDate },
                new SqlParameter("@SrcHdr", SqlDbType.Int) { Value = sourceInvoiceHeaderId },
                new SqlParameter("@SrcDet", SqlDbType.Int) { Value = sourceInvoiceDetailsId },
                new SqlParameter("@SrcGuid", SqlDbType.UniqueIdentifier)
                {
                    Value = string.IsNullOrWhiteSpace(sourceInvoiceGuid)
                        ? (object)DBNull.Value
                        : Simulate.Guid(sourceInvoiceGuid)
                },
                new SqlParameter("@SrcDetGuid", SqlDbType.UniqueIdentifier)
                {
                    Value = string.IsNullOrWhiteSpace(sourceInvoiceDetailsGuid)
                        ? (object)DBNull.Value
                        : Simulate.Guid(sourceInvoiceDetailsGuid)
                },
                new SqlParameter("@NBV", SqlDbType.Decimal) { Value = nbv },
                new SqlParameter("@Status", SqlDbType.Int) { Value = status },
                new SqlParameter("@Notes", SqlDbType.NVarChar, -1) { Value = Simulate.String(notes) },
                new SqlParameter("@CompanyID", SqlDbType.Int) { Value = companyId },
                new SqlParameter("@UserID", SqlDbType.Int) { Value = userId },
            };
            return Simulate.Integer32(sql.ExecuteScalar(q, prm, sql.CreateDataBaseConnectionString(companyId), trn));
        }

        public int UpdateAsset(int id, string assetCode, string name, int categoryId, int branchId, int costCenterId,
            decimal acquisitionCost, decimal salvageValue, int usefulLifeMonths, int method, decimal decliningRate,
            DateTime inServiceDate, int status, string notes, int companyId, int userId)
        {
            DataTable existing = SelectAssets(id, companyId);
            if (existing == null || existing.Rows.Count == 0) throw new Exception("Asset not found.");
            DataRow row = existing.Rows[0];
            int curStatus = Simulate.Integer32(row["Status"]);
            if (curStatus == StatusDisposed) throw new Exception("Disposed assets cannot be edited.");
            if (status == StatusDisposed)
                throw new Exception("Use Dispose Asset to dispose. Status cannot be set to Disposed here.");

            decimal accum = Simulate.decimal_(row["AccumulatedDepreciation"]);
            if (accum > 0.0001m)
            {
                // Lock cost basis and in-service date once depreciation started
                acquisitionCost = Simulate.decimal_(row["AcquisitionCost"]);
                salvageValue = Simulate.decimal_(row["SalvageValue"]);
                usefulLifeMonths = Simulate.Integer32(row["UsefulLifeMonths"]);
                method = Simulate.Integer32(row["DepreciationMethod"]);
                decliningRate = Simulate.decimal_(row["DecliningRate"]);
                inServiceDate = Simulate.StringToDate(Simulate.String(row["InServiceDate"]));
                if (status == StatusDraft)
                    throw new Exception("Cannot set Draft after depreciation has been posted.");
            }
            else
            {
                if (acquisitionCost < 0) throw new Exception("Acquisition cost cannot be negative.");
                if (salvageValue < 0) throw new Exception("Salvage value cannot be negative.");
                if (salvageValue > acquisitionCost) throw new Exception("Salvage value cannot exceed acquisition cost.");
                if (usefulLifeMonths <= 0) throw new Exception("Useful life (months) must be greater than zero.");
            }
            if (categoryId <= 0) throw new Exception("Asset category is required.");

            decimal nbv = acquisitionCost - accum;
            if (nbv < salvageValue) nbv = salvageValue;

            clsSQL sql = new clsSQL();
            string q = @"
UPDATE tbl_FixedAsset SET
  AssetCode=@Code, Name=@Name, CategoryID=@CategoryID, BranchID=@BranchID, CostCenterID=@CostCenterID,
  AcquisitionCost=@Cost, SalvageValue=@Salvage, UsefulLifeMonths=@Life, DepreciationMethod=@Method,
  DecliningRate=@Rate, InServiceDate=@InService, NetBookValue=@NBV, Status=@Status, Notes=@Notes,
  ModificationUserID=@UserID, ModificationDate=GETDATE()
WHERE ID=@ID AND CompanyID=@CompanyID AND Status <> 3";
            SqlParameter[] prm =
            {
                new SqlParameter("@ID", SqlDbType.Int) { Value = id },
                new SqlParameter("@Code", SqlDbType.NVarChar, 50) { Value = Simulate.String(assetCode) },
                new SqlParameter("@Name", SqlDbType.NVarChar, 250) { Value = Simulate.String(name) },
                new SqlParameter("@CategoryID", SqlDbType.Int) { Value = categoryId },
                new SqlParameter("@BranchID", SqlDbType.Int) { Value = branchId },
                new SqlParameter("@CostCenterID", SqlDbType.Int) { Value = costCenterId },
                new SqlParameter("@Cost", SqlDbType.Decimal) { Value = acquisitionCost },
                new SqlParameter("@Salvage", SqlDbType.Decimal) { Value = salvageValue },
                new SqlParameter("@Life", SqlDbType.Int) { Value = usefulLifeMonths },
                new SqlParameter("@Method", SqlDbType.Int) { Value = method },
                new SqlParameter("@Rate", SqlDbType.Decimal) { Value = decliningRate },
                new SqlParameter("@InService", SqlDbType.DateTime) { Value = inServiceDate },
                new SqlParameter("@NBV", SqlDbType.Decimal) { Value = nbv },
                new SqlParameter("@Status", SqlDbType.Int) { Value = status },
                new SqlParameter("@Notes", SqlDbType.NVarChar, -1) { Value = Simulate.String(notes) },
                new SqlParameter("@CompanyID", SqlDbType.Int) { Value = companyId },
                new SqlParameter("@UserID", SqlDbType.Int) { Value = userId },
            };
            return sql.ExecuteNonQueryStatement(q, sql.CreateDataBaseConnectionString(companyId), prm);
        }

        public bool DeleteAsset(int id, int companyId)
        {
            clsSQL sql = new clsSQL();
            object depCnt = sql.ExecuteScalar(
                "SELECT COUNT(1) FROM tbl_FixedAssetDepreciation WHERE AssetID=@ID AND CompanyID=@CompanyID",
                new[]
                {
                    new SqlParameter("@ID", SqlDbType.Int) { Value = id },
                    new SqlParameter("@CompanyID", SqlDbType.Int) { Value = companyId },
                },
                sql.CreateDataBaseConnectionString(companyId));
            if (Simulate.Integer32(depCnt) > 0)
                throw new Exception("Cannot delete asset: depreciation has been posted.");

            DataTable existing = SelectAssets(id, companyId);
            if (existing != null && existing.Rows.Count > 0)
            {
                int status = Simulate.Integer32(existing.Rows[0]["Status"]);
                if (status == StatusDisposed)
                    throw new Exception("Cannot delete a disposed asset.");
            }

            return sql.ExecuteNonQueryStatement(
                "DELETE FROM tbl_FixedAsset WHERE ID=@ID AND CompanyID=@CompanyID",
                sql.CreateDataBaseConnectionString(companyId),
                new[]
                {
                    new SqlParameter("@ID", SqlDbType.Int) { Value = id },
                    new SqlParameter("@CompanyID", SqlDbType.Int) { Value = companyId },
                }) > 0;
        }

        // ---------- Capitalize from Purchase Invoice ----------
        public int CapitalizeFromInvoice(string invoiceDetailsGuid, string assetCode, string name, int categoryId,
            int branchId, int costCenterId, decimal salvageValue, int usefulLifeMonths, int method,
            decimal decliningRate, DateTime? inServiceDate, string notes, int companyId, int userId)
        {
            if (string.IsNullOrWhiteSpace(invoiceDetailsGuid)) throw new Exception("Invoice detail is required.");
            if (categoryId <= 0) throw new Exception("Asset category is required.");

            clsSQL sql = new clsSQL();
            string conStr = sql.CreateDataBaseConnectionString(companyId);
            SqlParameter[] chk =
            {
                new SqlParameter("@DetGuid", SqlDbType.UniqueIdentifier) { Value = Simulate.Guid(invoiceDetailsGuid) },
                new SqlParameter("@CompanyID", SqlDbType.Int) { Value = companyId },
            };
            object already = sql.ExecuteScalar(@"
SELECT TOP 1 ID FROM tbl_FixedAsset
WHERE CompanyID=@CompanyID AND SourceInvoiceDetailsGuid=@DetGuid",
                chk, conStr);
            if (Simulate.Integer32(already) > 0)
                throw new Exception("This invoice line has already been capitalized.");

            string q = @"
SELECT CONVERT(varchar(36), d.Guid) AS DetailsGuid, d.HeaderGuid, d.ItemName, d.Qty, d.TotalQTY,
       d.PriceBeforeTax, d.DiscountBeforeTaxAmountPcs, d.DiscountBeforeTaxAmountAll,
       h.ID AS HeaderID, CONVERT(varchar(36), h.Guid) AS HeaderGuidStr, h.DocumentStatus,
       h.InvoiceTypeID AS VoucherTypeID, h.InvoiceDate AS VoucherDate, h.BranchID,
       ISNULL(h.InvoiceNo,0) AS InvoiceNo
FROM tbl_InvoiceDetails d
INNER JOIN tbl_InvoiceHeader h ON h.Guid = d.HeaderGuid
WHERE d.Guid = @DetGuid AND h.CompanyID = @CompanyID";
            DataTable dt = sql.ExecuteQueryStatement(q, conStr, chk);
            if (dt == null || dt.Rows.Count == 0) throw new Exception("Invoice line not found.");
            DataRow r = dt.Rows[0];

            int voucherType = Simulate.Integer32(r["VoucherTypeID"]);
            if (voucherType != (int)VoucherType.PurchaseInvoice &&
                voucherType != (int)VoucherType.PurchaseInvoiceFromFinancing)
                throw new Exception("Only purchase invoices can be capitalized.");

            int docStatus = Simulate.Integer32(r["DocumentStatus"]);
            if (docStatus != (int)DocumentStatus.Posted)
                throw new Exception("Purchase invoice must be posted before capitalization.");

            // Match picker LineExclTax: (PriceBeforeTax - DiscountPcs) * qty
            decimal qty = Simulate.decimal_(r["TotalQTY"]);
            if (qty <= 0) qty = Simulate.decimal_(r["Qty"]);
            decimal unitNet = Simulate.decimal_(r["PriceBeforeTax"]) - Simulate.decimal_(r["DiscountBeforeTaxAmountPcs"]);
            decimal lineExclTax = unitNet * qty;
            if (lineExclTax <= 0) throw new Exception("Invoice line amount (excl. tax) must be greater than zero.");

            if (string.IsNullOrWhiteSpace(name))
                name = Simulate.String(r["ItemName"]);
            if (string.IsNullOrWhiteSpace(assetCode))
                assetCode = "FA-" + Simulate.String(r["DetailsGuid"]).Substring(0, 8);

            DateTime serviceDate = inServiceDate ?? Simulate.StringToDate(Simulate.String(r["VoucherDate"]));
            if (branchId <= 0) branchId = Simulate.Integer32(r["BranchID"]);

            if (categoryId > 0)
            {
                DataTable cats = SelectCategories(categoryId, companyId);
                if (cats != null && cats.Rows.Count > 0)
                {
                    DataRow c = cats.Rows[0];
                    if (usefulLifeMonths <= 0) usefulLifeMonths = Simulate.Integer32(c["DefaultUsefulLifeMonths"]);
                    if (method <= 0) method = Simulate.Integer32(c["DefaultDepreciationMethod"]);
                    if (decliningRate <= 0) decliningRate = Simulate.decimal_(c["DefaultDecliningRate"]);
                }
            }
            if (usefulLifeMonths <= 0) usefulLifeMonths = 60;
            if (method <= 0) method = MethodStraightLine;

            DataTable accounts = new cls_AccountSetting().SelectAccountSetting(0, 0, companyId);
            int faCostAcc = 0;
            if (categoryId > 0)
            {
                DataTable cats = SelectCategories(categoryId, companyId);
                if (cats != null && cats.Rows.Count > 0)
                    faCostAcc = Simulate.Integer32(cats.Rows[0]["AssetAccountID"]);
            }

            int creditAcc = GetAccountId(accounts, (int)AccountMainSetting.PurchaseAccount);
            int inventoryAcc = GetAccountId(accounts, (int)AccountMainSetting.Inventory);
            if (clsInventoryConfig.UsePerpetualInventory && inventoryAcc > 0)
                creditAcc = inventoryAcc;
            if (creditAcc <= 0 && inventoryAcc > 0)
                creditAcc = inventoryAcc;

            if (faCostAcc <= 0)
                throw new Exception("Asset account is required on the asset category.");
            if (creditAcc <= 0)
                throw new Exception("Purchase/Inventory account is not configured for capitalization reclass.");
            if (faCostAcc == creditAcc)
                throw new Exception("Fixed asset cost account and purchase/inventory account must be different.");

            string noteText = Simulate.String(notes);
            if (string.IsNullOrWhiteSpace(noteText))
                noteText = "Capitalized from PI #" + Simulate.String(r["InvoiceNo"]) +
                           " line " + Simulate.String(r["DetailsGuid"]);

            new clsJournalVoucherTypes().Inserttbl_JournalVoucherTypes(
                (int)VoucherType.FixedAssetCapitalization,
                "رسملة أصل ثابت", "Fixed Asset Capitalization", 0, companyId);

            using (SqlConnection con = new SqlConnection(conStr))
            {
                con.Open();
                using (SqlTransaction trn = con.BeginTransaction())
                {
                    try
                    {
                        int assetId = InsertAsset(
                            assetCode, name, categoryId, branchId, costCenterId,
                            lineExclTax, salvageValue, usefulLifeMonths, method, decliningRate,
                            serviceDate, StatusActive, noteText, companyId, userId,
                            Simulate.Integer32(r["HeaderID"]), 0, Simulate.String(r["HeaderGuidStr"]),
                            Simulate.String(r["DetailsGuid"]), trn);

                        clsJournalVoucherHeader jvh = new clsJournalVoucherHeader();
                        DataTable dtMax = jvh.SelectMaxJVNo("", (int)VoucherType.FixedAssetCapitalization, companyId, trn);
                        int maxNo = 1;
                        if (dtMax != null && dtMax.Rows.Count > 0)
                            maxNo = Simulate.Integer32(dtMax.Rows[0][0]) + 1;

                        string jvGuid = jvh.InsertJournalVoucherHeader(
                            branchId, costCenterId,
                            "FA capitalize " + assetCode +
                                (string.IsNullOrWhiteSpace(name) ? "" : (" - " + name)) +
                                " from PI #" + Simulate.String(r["InvoiceNo"]),
                            Simulate.String(maxNo),
                            (int)VoucherType.FixedAssetCapitalization,
                            companyId, serviceDate, userId, "", 0, trn, 2);

                        clsJournalVoucherDetails jvd = new clsJournalVoucherDetails();
                        string assetLabel = string.IsNullOrWhiteSpace(name)
                            ? assetCode
                            : (assetCode + " - " + name);
                        // Dr Fixed Asset Cost
                        jvd.InsertJournalVoucherDetails(jvGuid, 1, faCostAcc, 0,
                            lineExclTax, 0, lineExclTax, 1, 1, lineExclTax,
                            branchId, costCenterId, serviceDate, "Capitalize to FA · " + assetLabel, companyId, userId, "", trn);
                        // Cr Inventory / Purchase (clear original PI debit)
                        jvd.InsertJournalVoucherDetails(jvGuid, 2, creditAcc, 0,
                            0, lineExclTax, -lineExclTax, 1, 1, -lineExclTax,
                            branchId, costCenterId, serviceDate, "Reclass from PI · " + assetLabel, companyId, userId, "", trn);

                        if (!jvh.CheckJVMatch(jvGuid, companyId, trn))
                            throw new Exception("Capitalization journal is not balanced.");

                        sql.ExecuteNonQueryStatement(@"
UPDATE tbl_FixedAsset
SET CapitalizationJVGuid = @JV,
    ModificationUserID = @User,
    ModificationDate = GETDATE()
WHERE ID = @ID AND CompanyID = @CompanyID",
                            conStr,
                            new[]
                            {
                                new SqlParameter("@JV", SqlDbType.UniqueIdentifier) { Value = Simulate.Guid(jvGuid) },
                                new SqlParameter("@User", SqlDbType.Int) { Value = userId },
                                new SqlParameter("@ID", SqlDbType.Int) { Value = assetId },
                                new SqlParameter("@CompanyID", SqlDbType.Int) { Value = companyId },
                            },
                            trn);

                        trn.Commit();
                        return assetId;
                    }
                    catch
                    {
                        trn.Rollback();
                        throw;
                    }
                }
            }
        }

        public DataTable SelectPostedPurchaseInvoiceLines(int companyId, int headerId = 0)
        {
            clsSQL sql = new clsSQL();
            string q = @"
SELECT h.ID AS HeaderID, CONVERT(varchar(36), h.Guid) AS HeaderGuid, h.InvoiceNo, h.InvoiceDate AS VoucherDate, h.InvoiceTypeID AS VoucherTypeID,
       CONVERT(varchar(36), d.Guid) AS DetailsGuid, d.ItemName, d.Qty, d.TotalQTY,
       d.PriceBeforeTax, d.DiscountBeforeTaxAmountPcs, d.DiscountBeforeTaxAmountAll,
       ((d.PriceBeforeTax - ISNULL(d.DiscountBeforeTaxAmountPcs,0)) * CASE WHEN ISNULL(d.TotalQTY,0)>0 THEN d.TotalQTY ELSE d.Qty END) AS LineExclTax,
       CASE WHEN EXISTS (
           SELECT 1 FROM tbl_FixedAsset fa
           WHERE fa.CompanyID = h.CompanyID AND fa.SourceInvoiceDetailsGuid = d.Guid
       ) THEN 1 ELSE 0 END AS AlreadyCapitalized
FROM tbl_InvoiceHeader h
INNER JOIN tbl_InvoiceDetails d ON d.HeaderGuid = h.Guid
WHERE h.CompanyID = @CompanyID
  AND h.DocumentStatus = 2
  AND h.InvoiceTypeID IN (2, 22)
  AND (@HeaderID = 0 OR h.ID = @HeaderID)
ORDER BY h.InvoiceDate DESC, h.ID DESC, d.RowIndex";
            SqlParameter[] prm =
            {
                new SqlParameter("@CompanyID", SqlDbType.Int) { Value = companyId },
                new SqlParameter("@HeaderID", SqlDbType.Int) { Value = headerId },
            };
            return sql.ExecuteQueryStatement(q, sql.CreateDataBaseConnectionString(companyId), prm);
        }

        // ---------- Depreciation ----------
        public DataTable PreviewDepreciation(string period, int companyId)
        {
            ValidatePeriod(period);
            DateTime periodEnd = PeriodEnd(period);

            clsSQL sql = new clsSQL();
            string q = @"
SELECT a.*,
       c.AssetAccountID AS CatAssetAccountID,
       c.AccumDepAccountID AS CatAccumDepAccountID,
       c.DepExpenseAccountID AS CatDepExpenseAccountID
FROM tbl_FixedAsset a
LEFT JOIN tbl_FixedAssetCategory c ON c.ID = a.CategoryID
WHERE a.CompanyID = @CompanyID
  AND ISNULL(a.Active,1) = 1
  AND a.Status IN (1, 2)
  AND a.InServiceDate <= @PeriodEnd
  AND NOT EXISTS (
      SELECT 1 FROM tbl_FixedAssetDepreciation d
      WHERE d.CompanyID = a.CompanyID AND d.AssetID = a.ID AND d.Period = @Period
  )
ORDER BY a.AssetCode";
            SqlParameter[] prm =
            {
                new SqlParameter("@CompanyID", SqlDbType.Int) { Value = companyId },
                new SqlParameter("@Period", SqlDbType.NVarChar, 10) { Value = period },
                new SqlParameter("@PeriodEnd", SqlDbType.DateTime) { Value = periodEnd },
            };
            DataTable assets = sql.ExecuteQueryStatement(q, sql.CreateDataBaseConnectionString(companyId), prm);
            DataTable preview = assets.Clone();
            if (!preview.Columns.Contains("DepreciationAmount"))
                preview.Columns.Add("DepreciationAmount", typeof(decimal));
            if (!preview.Columns.Contains("NewAccumulated"))
                preview.Columns.Add("NewAccumulated", typeof(decimal));
            if (!preview.Columns.Contains("NewNBV"))
                preview.Columns.Add("NewNBV", typeof(decimal));

            if (assets == null) return preview;

            foreach (DataRow row in assets.Rows)
            {
                decimal amount = CalculateMonthlyDepreciation(row, period);
                if (amount <= 0.0001m) continue;
                DataRow nr = preview.NewRow();
                nr.ItemArray = (object[])row.ItemArray.Clone();
                decimal accum = Simulate.decimal_(row["AccumulatedDepreciation"]);
                decimal cost = Simulate.decimal_(row["AcquisitionCost"]);
                decimal salvage = Simulate.decimal_(row["SalvageValue"]);
                decimal newAccum = accum + amount;
                decimal newNbv = cost - newAccum;
                if (newNbv < salvage) newNbv = salvage;
                nr["DepreciationAmount"] = Math.Round(amount, 3);
                nr["NewAccumulated"] = Math.Round(newAccum, 3);
                nr["NewNBV"] = Math.Round(newNbv, 3);
                preview.Rows.Add(nr);
            }
            return preview;
        }

        public object PostDepreciationRun(string period, int companyId, int userId, int branchId = 0)
        {
            ValidatePeriod(period);
            DataTable preview = PreviewDepreciation(period, companyId);
            if (preview == null || preview.Rows.Count == 0)
                throw new Exception("No assets to depreciate for period " + period + ".");

            // Aggregate Dr Exp / Cr Accum by account pair — accounts come from the category only.
            var buckets = new Dictionary<string, (int Exp, int Accum, decimal Amount, int Branch, int CC)>();
            decimal total = 0;
            foreach (DataRow row in preview.Rows)
            {
                decimal amt = Simulate.decimal_(row["DepreciationAmount"]);
                if (amt <= 0) continue;
                int expAcc = Simulate.Integer32(row["CatDepExpenseAccountID"]);
                int accumAcc = Simulate.Integer32(row["CatAccumDepAccountID"]);
                if (expAcc <= 0 || accumAcc <= 0)
                {
                    string code = Simulate.String(row["AssetCode"]);
                    throw new Exception(
                        "Depreciation accounts missing for asset " + code +
                        ". Set Accum. Dep and Dep Expense accounts on the asset category.");
                }
                int br = Simulate.Integer32(row["BranchID"]);
                if (br <= 0) br = branchId;
                int cc = Simulate.Integer32(row["CostCenterID"]);
                string key = expAcc + "|" + accumAcc + "|" + br + "|" + cc;
                if (!buckets.ContainsKey(key))
                    buckets[key] = (expAcc, accumAcc, 0, br, cc);
                var b = buckets[key];
                buckets[key] = (b.Exp, b.Accum, b.Amount + amt, b.Branch, b.CC);
                total += amt;
            }
            if (total <= 0) throw new Exception("Depreciation total is zero.");

            clsSQL clsSQL = new clsSQL();
            using (SqlConnection con = new SqlConnection(clsSQL.CreateDataBaseConnectionString(companyId)))
            {
                con.Open();
                using (SqlTransaction trn = con.BeginTransaction())
                {
                    try
                    {
                        // Re-check uniqueness under lock of period
                        object existingRun = clsSQL.ExecuteScalar(
                            "SELECT TOP 1 ID FROM tbl_FixedAssetDepreciationRun WHERE CompanyID=@C AND Period=@P",
                            new[]
                            {
                                new SqlParameter("@C", SqlDbType.Int) { Value = companyId },
                                new SqlParameter("@P", SqlDbType.NVarChar, 10) { Value = period },
                            },
                            clsSQL.CreateDataBaseConnectionString(companyId), trn);
                        // Allow multiple runs in same period only if different assets — uniqueness is per asset/period

                        int jvBranch = branchId;
                        if (jvBranch <= 0 && preview.Rows.Count > 0)
                            jvBranch = Simulate.Integer32(preview.Rows[0]["BranchID"]);

                        clsJournalVoucherHeader jvh = new clsJournalVoucherHeader();
                        DataTable dtMax = jvh.SelectMaxJVNo("", (int)VoucherType.FixedAssetDepreciation, companyId, trn);
                        int maxNo = 1;
                        if (dtMax != null && dtMax.Rows.Count > 0)
                            maxNo = Simulate.Integer32(dtMax.Rows[0][0]) + 1;

                        string jvGuid = jvh.InsertJournalVoucherHeader(
                            jvBranch, 0,
                            "FA depreciation " + period,
                            Simulate.String(maxNo),
                            (int)VoucherType.FixedAssetDepreciation,
                            companyId, PeriodEnd(period), userId, "", 0, trn, 2);

                        clsJournalVoucherDetails jvd = new clsJournalVoucherDetails();
                        DateTime now = DateTime.Now;
                        int rowIndex = 0;
                        foreach (var kv in buckets)
                        {
                            var b = kv.Value;
                            rowIndex++;
                            jvd.InsertJournalVoucherDetails(jvGuid, rowIndex, b.Exp, 0, b.Amount, 0, b.Amount, 1, 1, b.Amount, b.Branch, b.CC, now, "Depreciation " + period, companyId, userId, "", trn);
                            rowIndex++;
                            jvd.InsertJournalVoucherDetails(jvGuid, rowIndex, b.Accum, 0, 0, b.Amount, -b.Amount, 1, 1, -b.Amount, b.Branch, b.CC, now, "Accum dep " + period, companyId, userId, "", trn);
                        }

                        if (!jvh.CheckJVMatch(jvGuid, companyId, trn))
                            throw new Exception("Depreciation journal is not balanced.");

                        int runId = Simulate.Integer32(clsSQL.ExecuteScalar(@"
INSERT INTO tbl_FixedAssetDepreciationRun
(Guid, Period, RunDate, JVHeaderGuid, AssetCount, TotalAmount, CreatedBy, CompanyID, CreationDate)
OUTPUT INSERTED.ID
VALUES (NEWID(), @Period, GETDATE(), @JV, @Cnt, @Total, @User, @CompanyID, GETDATE())",
                            new[]
                            {
                                new SqlParameter("@Period", SqlDbType.NVarChar, 10) { Value = period },
                                new SqlParameter("@JV", SqlDbType.UniqueIdentifier) { Value = Simulate.Guid(jvGuid) },
                                new SqlParameter("@Cnt", SqlDbType.Int) { Value = preview.Rows.Count },
                                new SqlParameter("@Total", SqlDbType.Decimal) { Value = Math.Round(total, 3) },
                                new SqlParameter("@User", SqlDbType.Int) { Value = userId },
                                new SqlParameter("@CompanyID", SqlDbType.Int) { Value = companyId },
                            },
                            clsSQL.CreateDataBaseConnectionString(companyId), trn));

                        foreach (DataRow row in preview.Rows)
                        {
                            int assetId = Simulate.Integer32(row["ID"]);
                            decimal amt = Simulate.decimal_(row["DepreciationAmount"]);
                            decimal newAccum = Simulate.decimal_(row["NewAccumulated"]);
                            decimal newNbv = Simulate.decimal_(row["NewNBV"]);
                            decimal salvage = Simulate.decimal_(row["SalvageValue"]);
                            int newStatus = (newNbv <= salvage + 0.0001m) ? StatusFullyDepreciated : StatusActive;

                            clsSQL.ExecuteNonQueryStatement(@"
INSERT INTO tbl_FixedAssetDepreciation
(AssetID, Period, Amount, JVHeaderGuid, RunID, CompanyID, CreatedAt)
VALUES (@AssetID, @Period, @Amount, @JV, @RunID, @CompanyID, GETDATE())",
                                clsSQL.CreateDataBaseConnectionString(companyId),
                                new[]
                                {
                                    new SqlParameter("@AssetID", SqlDbType.Int) { Value = assetId },
                                    new SqlParameter("@Period", SqlDbType.NVarChar, 10) { Value = period },
                                    new SqlParameter("@Amount", SqlDbType.Decimal) { Value = amt },
                                    new SqlParameter("@JV", SqlDbType.UniqueIdentifier) { Value = Simulate.Guid(jvGuid) },
                                    new SqlParameter("@RunID", SqlDbType.Int) { Value = runId },
                                    new SqlParameter("@CompanyID", SqlDbType.Int) { Value = companyId },
                                }, trn);

                            clsSQL.ExecuteNonQueryStatement(@"
UPDATE tbl_FixedAsset SET
  AccumulatedDepreciation=@Accum, NetBookValue=@NBV, LastDepreciationPeriod=@Period, Status=@Status,
  ModificationUserID=@User, ModificationDate=GETDATE()
WHERE ID=@ID AND CompanyID=@CompanyID",
                                clsSQL.CreateDataBaseConnectionString(companyId),
                                new[]
                                {
                                    new SqlParameter("@Accum", SqlDbType.Decimal) { Value = newAccum },
                                    new SqlParameter("@NBV", SqlDbType.Decimal) { Value = newNbv },
                                    new SqlParameter("@Period", SqlDbType.NVarChar, 10) { Value = period },
                                    new SqlParameter("@Status", SqlDbType.Int) { Value = newStatus },
                                    new SqlParameter("@User", SqlDbType.Int) { Value = userId },
                                    new SqlParameter("@ID", SqlDbType.Int) { Value = assetId },
                                    new SqlParameter("@CompanyID", SqlDbType.Int) { Value = companyId },
                                }, trn);
                        }

                        trn.Commit();
                        return new
                        {
                            success = true,
                            runId,
                            jvGuid,
                            period,
                            assetCount = preview.Rows.Count,
                            totalAmount = Math.Round(total, 3)
                        };
                    }
                    catch
                    {
                        trn.Rollback();
                        throw;
                    }
                }
            }
        }

        // ---------- Disposal ----------
        public object DisposeAsset(int assetId, DateTime disposalDate, decimal proceeds, int proceedsAccountId,
            int companyId, int userId, int gainLossAccountId = 0)
        {
            DataTable assets = SelectAssets(assetId, companyId);
            if (assets == null || assets.Rows.Count == 0) throw new Exception("Asset not found.");
            DataRow row = assets.Rows[0];
            int status = Simulate.Integer32(row["Status"]);
            if (status == StatusDisposed) throw new Exception("Asset is already disposed.");
            if (status == StatusDraft) throw new Exception("Draft assets cannot be disposed. Activate first.");

            decimal cost = Simulate.decimal_(row["AcquisitionCost"]);
            decimal accum = Simulate.decimal_(row["AccumulatedDepreciation"]);
            decimal nbv = Simulate.decimal_(row["NetBookValue"]);
            if (nbv <= 0) nbv = cost - accum;
            if (proceeds < 0) throw new Exception("Proceeds cannot be negative.");

            DataTable accounts = new cls_AccountSetting().SelectAccountSetting(0, 0, companyId);
            int assetAcc = ResolveAssetAccount(row, companyId);
            int accumAcc = ResolveAccumAccount(row, companyId);
            int gainLossAcc = gainLossAccountId > 0
                ? gainLossAccountId
                : GetAccountId(accounts, (int)AccountMainSetting.GainLossOnDisposal);
            if (proceedsAccountId <= 0)
                proceedsAccountId = GetAccountId(accounts, (int)AccountMainSetting.DisposalProceedsClearing);
            if (assetAcc <= 0)
                throw new Exception("Asset account is required on the asset category.");
            if (accumAcc <= 0)
                throw new Exception("Accumulated depreciation account is required on the asset category.");
            if (gainLossAcc <= 0)
                throw new Exception("Gain/loss on disposal account is required.");
            if (proceeds > 0.0001m && proceedsAccountId <= 0)
                throw new Exception("Proceeds account is required when disposal proceeds > 0.");

            int branchId = Simulate.Integer32(row["BranchID"]);
            int cc = Simulate.Integer32(row["CostCenterID"]);
            decimal gainLoss = proceeds - nbv; // positive = gain

            clsSQL clsSQL = new clsSQL();
            using (SqlConnection con = new SqlConnection(clsSQL.CreateDataBaseConnectionString(companyId)))
            {
                con.Open();
                using (SqlTransaction trn = con.BeginTransaction())
                {
                    try
                    {
                        clsJournalVoucherHeader jvh = new clsJournalVoucherHeader();
                        DataTable dtMax = jvh.SelectMaxJVNo("", (int)VoucherType.FixedAssetDisposal, companyId, trn);
                        int maxNo = 1;
                        if (dtMax != null && dtMax.Rows.Count > 0)
                            maxNo = Simulate.Integer32(dtMax.Rows[0][0]) + 1;

                        string code = Simulate.String(row["AssetCode"]);
                        string assetName = Simulate.String(row["Name"]);
                        string assetLabel = string.IsNullOrWhiteSpace(assetName)
                            ? code
                            : (code + " - " + assetName);
                        string jvGuid = jvh.InsertJournalVoucherHeader(
                            branchId, cc,
                            "FA disposal " + assetLabel,
                            Simulate.String(maxNo),
                            (int)VoucherType.FixedAssetDisposal,
                            companyId, disposalDate, userId, "", 0, trn, 2);

                        clsJournalVoucherDetails jvd = new clsJournalVoucherDetails();
                        int ri = 0;
                        // Dr Accum Dep
                        if (accum > 0.0001m)
                        {
                            ri++;
                            jvd.InsertJournalVoucherDetails(jvGuid, ri, accumAcc, 0, accum, 0, accum, 1, 1, accum, branchId, cc, disposalDate, "Clear accum dep · " + assetLabel, companyId, userId, "", trn);
                        }
                        // Dr Proceeds
                        if (proceeds > 0.0001m)
                        {
                            ri++;
                            jvd.InsertJournalVoucherDetails(jvGuid, ri, proceedsAccountId, 0, proceeds, 0, proceeds, 1, 1, proceeds, branchId, cc, disposalDate, "Disposal proceeds · " + assetLabel, companyId, userId, "", trn);
                        }
                        // Cr Asset Cost
                        ri++;
                        jvd.InsertJournalVoucherDetails(jvGuid, ri, assetAcc, 0, 0, cost, -cost, 1, 1, -cost, branchId, cc, disposalDate, "Remove asset cost · " + assetLabel, companyId, userId, "", trn);
                        // Gain/Loss plug
                        if (gainLoss > 0.0001m)
                        {
                            // Gain: credit gain/loss
                            ri++;
                            jvd.InsertJournalVoucherDetails(jvGuid, ri, gainLossAcc, 0, 0, gainLoss, -gainLoss, 1, 1, -gainLoss, branchId, cc, disposalDate, "Gain on disposal · " + assetLabel, companyId, userId, "", trn);
                        }
                        else if (gainLoss < -0.0001m)
                        {
                            decimal loss = -gainLoss;
                            ri++;
                            jvd.InsertJournalVoucherDetails(jvGuid, ri, gainLossAcc, 0, loss, 0, loss, 1, 1, loss, branchId, cc, disposalDate, "Loss on disposal · " + assetLabel, companyId, userId, "", trn);
                        }

                        if (!jvh.CheckJVMatch(jvGuid, companyId, trn))
                            throw new Exception("Disposal journal is not balanced.");

                        clsSQL.ExecuteNonQueryStatement(@"
UPDATE tbl_FixedAsset SET
  Status=3, DisposalDate=@D, DisposalProceeds=@P, DisposalJVGuid=@JV,
  NetBookValue=0, ModificationUserID=@User, ModificationDate=GETDATE()
WHERE ID=@ID AND CompanyID=@CompanyID",
                            clsSQL.CreateDataBaseConnectionString(companyId),
                            new[]
                            {
                                new SqlParameter("@D", SqlDbType.DateTime) { Value = disposalDate },
                                new SqlParameter("@P", SqlDbType.Decimal) { Value = proceeds },
                                new SqlParameter("@JV", SqlDbType.UniqueIdentifier) { Value = Simulate.Guid(jvGuid) },
                                new SqlParameter("@User", SqlDbType.Int) { Value = userId },
                                new SqlParameter("@ID", SqlDbType.Int) { Value = assetId },
                                new SqlParameter("@CompanyID", SqlDbType.Int) { Value = companyId },
                            }, trn);

                        trn.Commit();
                        return new
                        {
                            success = true,
                            jvGuid,
                            assetId,
                            proceeds,
                            netBookValue = nbv,
                            gainLoss = Math.Round(gainLoss, 3)
                        };
                    }
                    catch
                    {
                        trn.Rollback();
                        throw;
                    }
                }
            }
        }

        public DataTable SelectDisposedAssets(int companyId)
        {
            clsSQL sql = new clsSQL();
            return sql.ExecuteQueryStatement(@"
SELECT a.ID, a.AssetCode, a.Name, a.AcquisitionCost, a.AccumulatedDepreciation,
       a.DisposalDate, a.DisposalProceeds, CONVERT(varchar(36), a.DisposalJVGuid) AS DisposalJVGuid,
       ISNULL(c.Name,'') AS CategoryName,
       (ISNULL(a.DisposalProceeds,0) - (ISNULL(a.AcquisitionCost,0) - ISNULL(a.AccumulatedDepreciation,0))) AS GainLoss
FROM tbl_FixedAsset a
LEFT JOIN tbl_FixedAssetCategory c ON c.ID = a.CategoryID
WHERE a.CompanyID=@CompanyID AND a.Status=3
ORDER BY a.DisposalDate DESC, a.ID DESC",
                sql.CreateDataBaseConnectionString(companyId),
                new[] { new SqlParameter("@CompanyID", SqlDbType.Int) { Value = companyId } });
        }

        public object CancelDisposal(int assetId, int companyId, int userId)
        {
            if (assetId <= 0) throw new Exception("Asset is required.");

            clsSQL clsSQL = new clsSQL();
            string conStr = clsSQL.CreateDataBaseConnectionString(companyId);
            DataTable assets = SelectAssets(assetId, companyId);
            if (assets == null || assets.Rows.Count == 0) throw new Exception("Asset not found.");
            DataRow row = assets.Rows[0];
            if (Simulate.Integer32(row["Status"]) != StatusDisposed)
                throw new Exception("Asset is not disposed.");

            string jvGuid = Simulate.String(row["DisposalJVGuid"]);
            decimal cost = Simulate.decimal_(row["AcquisitionCost"]);
            decimal accum = Simulate.decimal_(row["AccumulatedDepreciation"]);
            decimal salvage = Simulate.decimal_(row["SalvageValue"]);
            decimal nbv = cost - accum;
            if (nbv < salvage) nbv = salvage;
            int newStatus = (nbv <= salvage + 0.0001m) ? StatusFullyDepreciated : StatusActive;

            using (SqlConnection con = new SqlConnection(conStr))
            {
                con.Open();
                using (SqlTransaction trn = con.BeginTransaction())
                {
                    try
                    {
                        if (!string.IsNullOrWhiteSpace(jvGuid) &&
                            jvGuid != "00000000-0000-0000-0000-000000000000")
                        {
                            new clsJournalVoucherDetails()
                                .DeleteJournalVoucherDetailsByParentId(jvGuid, companyId, trn);
                            new clsJournalVoucherHeader()
                                .DeleteJournalVoucherHeaderByID(jvGuid, companyId, trn);
                        }

                        clsSQL.ExecuteNonQueryStatement(@"
UPDATE tbl_FixedAsset SET
  Status=@Status,
  NetBookValue=@NBV,
  DisposalDate=NULL,
  DisposalProceeds=0,
  DisposalJVGuid=NULL,
  ModificationUserID=@User,
  ModificationDate=GETDATE()
WHERE ID=@ID AND CompanyID=@CompanyID",
                            conStr,
                            new[]
                            {
                                new SqlParameter("@Status", SqlDbType.Int) { Value = newStatus },
                                new SqlParameter("@NBV", SqlDbType.Decimal) { Value = Math.Round(nbv, 3) },
                                new SqlParameter("@User", SqlDbType.Int) { Value = userId },
                                new SqlParameter("@ID", SqlDbType.Int) { Value = assetId },
                                new SqlParameter("@CompanyID", SqlDbType.Int) { Value = companyId },
                            }, trn);

                        trn.Commit();
                        return new
                        {
                            success = true,
                            assetId,
                            status = newStatus,
                            netBookValue = Math.Round(nbv, 3),
                            jvGuid,
                        };
                    }
                    catch
                    {
                        trn.Rollback();
                        throw;
                    }
                }
            }
        }

        // ---------- Reports ----------
        public DataTable SelectRegisterReport(int companyId, int status = -1)
        {
            return SelectAssets(0, companyId, status);
        }

        public DataTable SelectDepreciationSchedule(int assetId, int companyId)
        {
            clsSQL sql = new clsSQL();
            string q = @"
SELECT d.*, a.AssetCode, a.Name AS AssetName, r.Guid AS RunGuid
FROM tbl_FixedAssetDepreciation d
INNER JOIN tbl_FixedAsset a ON a.ID = d.AssetID
LEFT JOIN tbl_FixedAssetDepreciationRun r ON r.ID = d.RunID
WHERE d.CompanyID = @CompanyID
  AND (@AssetID = 0 OR d.AssetID = @AssetID)
ORDER BY d.Period, a.AssetCode";
            return sql.ExecuteQueryStatement(q, sql.CreateDataBaseConnectionString(companyId),
                new[]
                {
                    new SqlParameter("@CompanyID", SqlDbType.Int) { Value = companyId },
                    new SqlParameter("@AssetID", SqlDbType.Int) { Value = assetId },
                });
        }

        public DataTable SelectDepreciationRuns(int companyId)
        {
            clsSQL sql = new clsSQL();
            return sql.ExecuteQueryStatement(@"
SELECT * FROM tbl_FixedAssetDepreciationRun
WHERE CompanyID=@CompanyID
ORDER BY Period DESC, ID DESC",
                sql.CreateDataBaseConnectionString(companyId),
                new[] { new SqlParameter("@CompanyID", SqlDbType.Int) { Value = companyId } });
        }

        public object CancelDepreciationRun(int runId, int companyId, int userId)
        {
            if (runId <= 0) throw new Exception("Depreciation run is required.");

            clsSQL clsSQL = new clsSQL();
            string conStr = clsSQL.CreateDataBaseConnectionString(companyId);
            DataTable runs = clsSQL.ExecuteQueryStatement(@"
SELECT * FROM tbl_FixedAssetDepreciationRun
WHERE ID=@ID AND CompanyID=@CompanyID",
                conStr,
                new[]
                {
                    new SqlParameter("@ID", SqlDbType.Int) { Value = runId },
                    new SqlParameter("@CompanyID", SqlDbType.Int) { Value = companyId },
                });
            if (runs == null || runs.Rows.Count == 0)
                throw new Exception("Depreciation run not found.");

            DataRow run = runs.Rows[0];
            string period = Simulate.String(run["Period"]);
            string jvGuid = Simulate.String(run["JVHeaderGuid"]);

            DataTable lines = clsSQL.ExecuteQueryStatement(@"
SELECT d.*, a.AssetCode, a.AccumulatedDepreciation, a.NetBookValue, a.AcquisitionCost,
       a.SalvageValue, a.Status
FROM tbl_FixedAssetDepreciation d
INNER JOIN tbl_FixedAsset a ON a.ID = d.AssetID AND a.CompanyID = d.CompanyID
WHERE d.RunID=@RunID AND d.CompanyID=@CompanyID",
                conStr,
                new[]
                {
                    new SqlParameter("@RunID", SqlDbType.Int) { Value = runId },
                    new SqlParameter("@CompanyID", SqlDbType.Int) { Value = companyId },
                });
            if (lines == null || lines.Rows.Count == 0)
                throw new Exception("No depreciation lines found for this run.");

            // Only allow cancel when this period is the latest posted period for every asset in the run.
            foreach (DataRow line in lines.Rows)
            {
                int assetId = Simulate.Integer32(line["AssetID"]);
                object later = clsSQL.ExecuteScalar(@"
SELECT TOP 1 Period FROM tbl_FixedAssetDepreciation
WHERE CompanyID=@CompanyID AND AssetID=@AssetID AND Period > @Period
ORDER BY Period DESC",
                    new[]
                    {
                        new SqlParameter("@CompanyID", SqlDbType.Int) { Value = companyId },
                        new SqlParameter("@AssetID", SqlDbType.Int) { Value = assetId },
                        new SqlParameter("@Period", SqlDbType.NVarChar, 10) { Value = period },
                    },
                    conStr);
                if (later != null && !string.IsNullOrWhiteSpace(Simulate.String(later)))
                {
                    throw new Exception(
                        "Cannot cancel period " + period + " for asset " + Simulate.String(line["AssetCode"]) +
                        ". Cancel later periods first (latest: " + Simulate.String(later) + ").");
                }
                int status = Simulate.Integer32(line["Status"]);
                if (status == StatusDisposed)
                {
                    throw new Exception(
                        "Cannot cancel depreciation for disposed asset " + Simulate.String(line["AssetCode"]) + ".");
                }
            }

            using (SqlConnection con = new SqlConnection(conStr))
            {
                con.Open();
                using (SqlTransaction trn = con.BeginTransaction())
                {
                    try
                    {
                        foreach (DataRow line in lines.Rows)
                        {
                            int assetId = Simulate.Integer32(line["AssetID"]);
                            decimal amt = Simulate.decimal_(line["Amount"]);
                            decimal accum = Simulate.decimal_(line["AccumulatedDepreciation"]) - amt;
                            if (accum < 0) accum = 0;
                            decimal cost = Simulate.decimal_(line["AcquisitionCost"]);
                            decimal salvage = Simulate.decimal_(line["SalvageValue"]);
                            decimal nbv = cost - accum;
                            if (nbv < salvage) nbv = salvage;

                            object prevPeriodObj = clsSQL.ExecuteScalar(@"
SELECT MAX(Period) FROM tbl_FixedAssetDepreciation
WHERE CompanyID=@CompanyID AND AssetID=@AssetID AND Period < @Period",
                                new[]
                                {
                                    new SqlParameter("@CompanyID", SqlDbType.Int) { Value = companyId },
                                    new SqlParameter("@AssetID", SqlDbType.Int) { Value = assetId },
                                    new SqlParameter("@Period", SqlDbType.NVarChar, 10) { Value = period },
                                },
                                conStr, trn);
                            string prevPeriod = Simulate.String(prevPeriodObj);
                            object lastPeriodVal = string.IsNullOrWhiteSpace(prevPeriod)
                                ? (object)DBNull.Value
                                : prevPeriod;

                            int newStatus = StatusActive;
                            if (nbv <= salvage + 0.0001m) newStatus = StatusFullyDepreciated;

                            clsSQL.ExecuteNonQueryStatement(@"
UPDATE tbl_FixedAsset SET
  AccumulatedDepreciation=@Accum, NetBookValue=@NBV, LastDepreciationPeriod=@LastPeriod, Status=@Status,
  ModificationUserID=@User, ModificationDate=GETDATE()
WHERE ID=@ID AND CompanyID=@CompanyID",
                                conStr,
                                new[]
                                {
                                    new SqlParameter("@Accum", SqlDbType.Decimal) { Value = Math.Round(accum, 3) },
                                    new SqlParameter("@NBV", SqlDbType.Decimal) { Value = Math.Round(nbv, 3) },
                                    new SqlParameter("@LastPeriod", SqlDbType.NVarChar, 10) { Value = lastPeriodVal },
                                    new SqlParameter("@Status", SqlDbType.Int) { Value = newStatus },
                                    new SqlParameter("@User", SqlDbType.Int) { Value = userId },
                                    new SqlParameter("@ID", SqlDbType.Int) { Value = assetId },
                                    new SqlParameter("@CompanyID", SqlDbType.Int) { Value = companyId },
                                }, trn);
                        }

                        clsSQL.ExecuteNonQueryStatement(@"
DELETE FROM tbl_FixedAssetDepreciation
WHERE RunID=@RunID AND CompanyID=@CompanyID",
                            conStr,
                            new[]
                            {
                                new SqlParameter("@RunID", SqlDbType.Int) { Value = runId },
                                new SqlParameter("@CompanyID", SqlDbType.Int) { Value = companyId },
                            }, trn);

                        if (!string.IsNullOrWhiteSpace(jvGuid) &&
                            jvGuid != "00000000-0000-0000-0000-000000000000")
                        {
                            new clsJournalVoucherDetails()
                                .DeleteJournalVoucherDetailsByParentId(jvGuid, companyId, trn);
                            new clsJournalVoucherHeader()
                                .DeleteJournalVoucherHeaderByID(jvGuid, companyId, trn);
                        }

                        clsSQL.ExecuteNonQueryStatement(@"
DELETE FROM tbl_FixedAssetDepreciationRun
WHERE ID=@ID AND CompanyID=@CompanyID",
                            conStr,
                            new[]
                            {
                                new SqlParameter("@ID", SqlDbType.Int) { Value = runId },
                                new SqlParameter("@CompanyID", SqlDbType.Int) { Value = companyId },
                            }, trn);

                        trn.Commit();
                        return new
                        {
                            success = true,
                            runId,
                            period,
                            assetCount = lines.Rows.Count,
                            jvGuid,
                        };
                    }
                    catch
                    {
                        trn.Rollback();
                        throw;
                    }
                }
            }
        }

        // ---------- Helpers ----------
        public static decimal CalculateMonthlyDepreciation(DataRow row, string period)
        {
            decimal cost = Simulate.decimal_(row["AcquisitionCost"]);
            decimal salvage = Simulate.decimal_(row["SalvageValue"]);
            decimal accum = Simulate.decimal_(row["AccumulatedDepreciation"]);
            decimal nbv = cost - accum;
            if (nbv <= salvage + 0.0001m) return 0;

            int life = Simulate.Integer32(row["UsefulLifeMonths"]);
            if (life <= 0) return 0;

            int method = Simulate.Integer32(row["DepreciationMethod"]);
            decimal rate = Simulate.decimal_(row["DecliningRate"]);

            int monthsDone = CountDepreciatedMonths(row, period);
            int remaining = life - monthsDone;
            if (remaining <= 0) remaining = 1;

            decimal depreciable = cost - salvage - accum;
            if (depreciable <= 0) return 0;

            if (method == MethodDeclining)
            {
                decimal monthly = nbv * (rate / 100m) / 12m;
                // Switch to SL residual when declining would undershoot salvage over remaining life
                decimal slResidual = depreciable / remaining;
                if (monthly < slResidual - 0.0001m || (nbv - monthly) < salvage)
                    monthly = slResidual;
                if (monthly > depreciable) monthly = depreciable;
                if (nbv - monthly < salvage) monthly = nbv - salvage;
                return monthly < 0 ? 0 : Math.Round(monthly, 3);
            }

            // Straight-line
            decimal sl = depreciable / remaining;
            if (nbv - sl < salvage) sl = nbv - salvage;
            return sl < 0 ? 0 : Math.Round(sl, 3);
        }

        static int CountDepreciatedMonths(DataRow row, string period)
        {
            // Prefer counting from InServiceDate to period (inclusive months already posted ≈ LastPeriod)
            string last = Simulate.String(row["LastDepreciationPeriod"]);
            DateTime inService = Simulate.StringToDate(Simulate.String(row["InServiceDate"]));
            DateTime periodStart = PeriodStart(period);

            if (!string.IsNullOrWhiteSpace(last) && last.Length >= 7)
            {
                try
                {
                    DateTime lastStart = PeriodStart(last);
                    int months = ((lastStart.Year - inService.Year) * 12) + (lastStart.Month - inService.Month) + 1;
                    if (months < 0) months = 0;
                    // Cap by useful life
                    int life = Simulate.Integer32(row["UsefulLifeMonths"]);
                    if (life > 0 && months > life) months = life;
                    return months;
                }
                catch { /* fall through */ }
            }

            // No prior depreciation: months already done = 0 if period is first eligible
            if (periodStart < new DateTime(inService.Year, inService.Month, 1))
                return 0;
            return 0;
        }

        static void ValidatePeriod(string period)
        {
            if (string.IsNullOrWhiteSpace(period) || period.Length < 7)
                throw new Exception("Period must be yyyy-MM.");
            PeriodStart(period); // throws if invalid
        }

        static DateTime PeriodStart(string period)
        {
            int y = Simulate.Integer32(period.Substring(0, 4));
            int m = Simulate.Integer32(period.Substring(5, 2));
            if (y < 2000 || m < 1 || m > 12) throw new Exception("Invalid period " + period);
            return new DateTime(y, m, 1);
        }

        static DateTime PeriodEnd(string period)
        {
            DateTime start = PeriodStart(period);
            return start.AddMonths(1).AddDays(-1);
        }

        static int GetAccountId(DataTable dtAcc, int accountRefId)
        {
            if (dtAcc == null) return 0;
            foreach (DataRow r in dtAcc.Rows)
            {
                if (Simulate.Integer32(r["AccountRefID"]) == accountRefId)
                    return Simulate.Integer32(r["AccountID"]);
            }
            return 0;
        }

        int ResolveAssetAccount(DataRow assetRow, int companyId)
        {
            int catId = Simulate.Integer32(assetRow["CategoryID"]);
            if (catId > 0)
            {
                DataTable cats = SelectCategories(catId, companyId);
                if (cats != null && cats.Rows.Count > 0)
                    return Simulate.Integer32(cats.Rows[0]["AssetAccountID"]);
            }
            return 0;
        }

        int ResolveAccumAccount(DataRow assetRow, int companyId)
        {
            int catId = Simulate.Integer32(assetRow["CategoryID"]);
            if (catId > 0)
            {
                DataTable cats = SelectCategories(catId, companyId);
                if (cats != null && cats.Rows.Count > 0)
                    return Simulate.Integer32(cats.Rows[0]["AccumDepAccountID"]);
            }
            return 0;
        }
    }
}
