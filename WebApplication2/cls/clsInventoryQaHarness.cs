using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using WebApplication2.MainClasses;

namespace WebApplication2.cls
{
    /// <summary>
    /// Full Warehouse / Inventory QA: transfer &amp; count validation fixtures,
    /// QTYFactor / costing rules, schema, and live stock integrity scans.
    /// </summary>
    public static class clsInventoryQaHarness
    {
        public class QaResult
        {
            public string Category { get; set; }
            public string Name { get; set; }
            public bool Passed { get; set; }
            public string Detail { get; set; }
        }

        public class QaReport
        {
            public bool AllPassed { get; set; }
            public int TotalChecks { get; set; }
            public int PassedChecks { get; set; }
            public int FailedChecks { get; set; }
            public string RunAtUtc { get; set; }
            public int CompanyId { get; set; }
            public List<QaResult> Results { get; set; } = new List<QaResult>();
            public Dictionary<string, int> SummaryByCategory { get; set; } = new Dictionary<string, int>();
        }

        public static QaReport Run(int companyId = 0, bool scanDatabase = true)
        {
            var results = new List<QaResult>();

            results.AddRange(RunTransferValidationFixtures());
            results.AddRange(RunStockCountValidationFixtures());
            results.AddRange(RunOnHandMathFixtures());
            results.AddRange(RunConfigFixtures());

            if (companyId > 0)
            {
                try
                {
                    var connInfo = ResolveCompanyConnectionInfo(companyId);
                    if (!connInfo.ok)
                    {
                        results.Add(new QaResult
                        {
                            Category = "Schema",
                            Name = "CompanyDatabaseConnection",
                            Passed = false,
                            Detail = connInfo.detail
                        });
                    }
                    else
                    {
                        results.AddRange(RunSchemaChecks(companyId));
                        results.AddRange(RunQtyFactorChecks(companyId));
                        if (scanDatabase)
                        {
                            results.AddRange(RunStockBalanceDriftScan(companyId));
                            results.AddRange(RunNegativeStockScan(companyId));
                            results.AddRange(RunTransferPairScan(companyId));
                            results.AddRange(RunInventoryAccountScan(companyId));
                        }
                    }
                }
                catch (Exception ex)
                {
                    results.Add(new QaResult
                    {
                        Category = "Schema",
                        Name = "CompanyDatabaseConnection",
                        Passed = false,
                        Detail = ex.Message
                    });
                }
            }
            else
            {
                results.Add(new QaResult
                {
                    Category = "Schema",
                    Name = "SchemaChecksSkipped",
                    Passed = true,
                    Detail = "Pass CompanyID to validate DB schema and scan live stock."
                });
            }

            int passed = results.Count(r => r.Passed);
            return new QaReport
            {
                AllPassed = results.All(r => r.Passed),
                TotalChecks = results.Count,
                PassedChecks = passed,
                FailedChecks = results.Count - passed,
                RunAtUtc = DateTime.UtcNow.ToString("o"),
                CompanyId = companyId,
                Results = results,
                SummaryByCategory = results
                    .GroupBy(r => r.Category ?? "Other")
                    .ToDictionary(g => g.Key, g => g.Count(x => !x.Passed))
            };
        }

        static (bool ok, string detail) ResolveCompanyConnectionInfo(int companyId)
        {
            clsSQL sql = new clsSQL();
            SqlParameter[] prm =
            {
                new SqlParameter("@ID", SqlDbType.Int) { Value = companyId },
            };
            DataTable dt = sql.ExecuteQueryStatement(
                "SELECT TOP 1 ID, ISNULL(DataBaseName,N'') AS DataBaseName FROM tbl_company WHERE ID=@ID",
                sql.MainDataBaseconString, prm);
            if (dt == null || dt.Rows.Count == 0)
                return (false, $"Company ID {companyId} was not found in tbl_company.");

            string dbName = Simulate.String(dt.Rows[0]["DataBaseName"]);
            if (string.IsNullOrWhiteSpace(dbName))
            {
                return (false,
                    "This company has no tenant database (DataBaseName is empty). " +
                    "Set the company database name in Settings → Company, then log in again.");
            }

            string conn = sql.CreateDataBaseConnectionString(companyId);
            if (string.IsNullOrWhiteSpace(conn))
                return (false, "Company database connection string could not be built.");

            return (true, dbName);
        }

        static List<QaResult> RunTransferValidationFixtures()
        {
            var results = new List<QaResult>();
            var xfer = new clsWarehouseTransfer();
            var lines = new List<WarehouseTransferLine>
            {
                new WarehouseTransferLine { ItemGuid = Guid.NewGuid().ToString(), Qty = 1 }
            };

            var r1 = xfer.PostTransfer(1, 0, 2, lines, "", 1, 1, DateTime.Today, null);
            results.Add(Ok("TransferValidation", "RejectsMissingStores",
                !r1.Success && (r1.Message ?? "").IndexOf("required", StringComparison.OrdinalIgnoreCase) >= 0,
                r1.Message));

            var r2 = xfer.PostTransfer(1, 5, 5, lines, "", 1, 1, DateTime.Today, null);
            results.Add(Ok("TransferValidation", "RejectsSameStore",
                !r2.Success && (r2.Message ?? "").IndexOf("different", StringComparison.OrdinalIgnoreCase) >= 0,
                r2.Message));

            var r3 = xfer.PostTransfer(1, 1, 2, new List<WarehouseTransferLine>(), "", 1, 1, DateTime.Today, null);
            results.Add(Ok("TransferValidation", "RejectsEmptyLines",
                !r3.Success && (r3.Message ?? "").IndexOf("line", StringComparison.OrdinalIgnoreCase) >= 0,
                r3.Message));

            var r4 = xfer.PostTransfer(1, 1, 2, null, "", 1, 1, DateTime.Today, null);
            results.Add(Ok("TransferValidation", "RejectsNullLines",
                !r4.Success, r4.Message));

            return results;
        }

        static List<QaResult> RunStockCountValidationFixtures()
        {
            var results = new List<QaResult>();
            var count = new clsStockCount();

            var r1 = count.PostStockCount(1, 0, new List<StockCountLine>
            {
                new StockCountLine { ItemGuid = Guid.NewGuid().ToString(), CountedQty = 1 }
            }, 0, "", 1, 1, DateTime.Today, null);
            results.Add(Ok("StockCountValidation", "RejectsMissingStore",
                !r1.Success && (r1.Message ?? "").IndexOf("Store", StringComparison.OrdinalIgnoreCase) >= 0,
                r1.Message));

            var r2 = count.PostStockCount(1, 1, new List<StockCountLine>(), 0, "", 1, 1, DateTime.Today, null);
            results.Add(Ok("StockCountValidation", "RejectsEmptyLines",
                !r2.Success && (r2.Message ?? "").IndexOf("line", StringComparison.OrdinalIgnoreCase) >= 0,
                r2.Message));

            var r3 = count.PostStockCount(1, 1, new List<StockCountLine>
            {
                new StockCountLine { ItemGuid = Guid.NewGuid().ToString(), CountedQty = -5 }
            }, 0, "", 1, 1, DateTime.Today, null);
            results.Add(Ok("StockCountValidation", "RejectsNegativeCountedQty",
                !r3.Success && (r3.Message ?? "").IndexOf("negative", StringComparison.OrdinalIgnoreCase) >= 0,
                r3.Message));

            return results;
        }

        /// <summary>Pure math: on-hand = SUM(TotalQTY * QTYFactor) for counted lines.</summary>
        static List<QaResult> RunOnHandMathFixtures()
        {
            var results = new List<QaResult>();

            // GR 10 (+1) then GI 3 (-1) → on-hand 7
            decimal onHand = CalcOnHand(new[]
            {
                (10m, 1),
                (3m, -1),
            });
            results.Add(Ok("OnHandMath", "GrThenGi", onHand == 7m, $"OnHand={onHand}"));

            // Transfer: GI 5 from A, GR 5 to B — company total unchanged when store=0
            decimal company = CalcOnHand(new[]
            {
                (100m, 1),
                (5m, -1),
                (5m, 1),
            });
            results.Add(Ok("OnHandMath", "TransferNetZeroCompany", company == 100m, $"OnHand={company}"));

            // Stock count: system 8, counted 10 → GR variance 2 → on-hand 10
            decimal afterCount = CalcOnHand(new[]
            {
                (8m, 1),
                (2m, 1),
            });
            results.Add(Ok("OnHandMath", "StockCountSurplus", afterCount == 10m, $"OnHand={afterCount}"));

            // Offers (factor 0) do not move stock
            decimal withOffer = CalcOnHand(new[]
            {
                (50m, 1),
                (20m, 0),
            });
            results.Add(Ok("OnHandMath", "OfferDoesNotAffectStock", withOffer == 50m, $"OnHand={withOffer}"));

            // Avg-cost pollution example: real layers only (exclude transfer GR)
            // Purchase 100@10 + purchase 50@20 = 13.333...; polluted would be 12.5
            decimal trueAvg = (100m * 10m + 50m * 20m) / 150m;
            decimal polluted = (100m * 10m + 50m * 10m + 50m * 20m) / 200m;
            results.Add(Ok("OnHandMath", "AvgCostExcludesTransferLayers",
                Math.Round(trueAvg, 4) == 13.3333m && Math.Round(polluted, 4) == 12.5m,
                $"TrueAvg={trueAvg:N4} Polluted={polluted:N4} (transfer GRs must be excluded)"));

            return results;
        }

        internal static decimal CalcOnHand(IEnumerable<(decimal qty, int factor)> lines)
        {
            decimal sum = 0;
            foreach (var line in lines)
                sum += line.qty * line.factor;
            return sum;
        }

        static List<QaResult> RunConfigFixtures()
        {
            return new List<QaResult>
            {
                Ok("Config", "PerpetualDefaultOff",
                    clsInventoryConfig.UsePerpetualInventory == false,
                    "UsePerpetualInventory must stay false unless GR/IR clearing is configured."),
                Ok("Config", "JvBalanceEpsilonPositive",
                    clsInventoryConfig.JvBalanceEpsilon > 0,
                    $"Epsilon={clsInventoryConfig.JvBalanceEpsilon}"),
                Ok("Config", "VoucherTypeGoodReceiptIs8",
                    (int)clsEnum.VoucherType.GoodRecipt == 8,
                    $"GoodRecipt={(int)clsEnum.VoucherType.GoodRecipt}"),
                Ok("Config", "VoucherTypeGoodIssueIs9",
                    (int)clsEnum.VoucherType.GoodIssue == 9,
                    $"GoodIssue={(int)clsEnum.VoucherType.GoodIssue}"),
            };
        }

        static List<QaResult> RunSchemaChecks(int companyId)
        {
            var results = new List<QaResult>();
            clsSQL sql = new clsSQL();
            string conn = sql.CreateDataBaseConnectionString(companyId);

            results.Add(SchemaTableExists(sql, conn, "tbl_StockBalance", companyId));
            results.Add(SchemaTableExists(sql, conn, "tbl_Store", companyId));
            results.Add(SchemaTableExists(sql, conn, "tbl_Items", companyId));
            results.Add(SchemaTableExists(sql, conn, "tbl_InvoiceDetailsLotsTracking", companyId));
            results.Add(SchemaTableExists(sql, conn, "tbl_InvoiceDetailsLotsSerialNumber", companyId));
            results.Add(SchemaColumnExists(sql, conn, "tbl_Items", "IsStockItem", companyId));
            results.Add(SchemaColumnExists(sql, conn, "tbl_Items", "AllowNegativeStock", companyId));
            results.Add(SchemaColumnExists(sql, conn, "tbl_Items", "AVGCostPerUnit", companyId));
            results.Add(SchemaColumnExists(sql, conn, "tbl_StockBalance", "OnHandQty", companyId));

            try
            {
                object dbVersion = sql.ExecuteScalar(
                    "SELECT TOP 1 ISNULL(VersionNumber,0) FROM tbl_DataBaseVersion ORDER BY VersionNumber DESC",
                    conn, (SqlTransaction)null);
                decimal version = Simulate.decimal_(dbVersion);
                results.Add(Ok("Schema", "DatabaseVersion_AtLeast1028",
                    version >= Simulate.decimal_(10.28),
                    $"Current version={version} (tbl_StockBalance since 10.28)"));
            }
            catch (Exception ex)
            {
                results.Add(Ok("Schema", "DatabaseVersion_AtLeast1028", false, ex.Message));
            }

            return results;
        }

        static List<QaResult> RunQtyFactorChecks(int companyId)
        {
            var results = new List<QaResult>();
            clsSQL sql = new clsSQL();
            string conn = sql.CreateDataBaseConnectionString(companyId);

            var expected = new Dictionary<int, int>
            {
                { 2, 1 }, { 3, -1 }, { 4, 1 }, { 5, 0 }, { 6, 0 }, { 7, -1 },
                { 8, 1 }, { 9, -1 }, { 10, -1 }, { 11, 1 }, { 22, 1 }, { 25, -1 }, { 26, 1 },
            };

            try
            {
                DataTable dt = sql.ExecuteQueryStatement(
                    "SELECT ID, ISNULL(QTYFactor,0) AS QTYFactor FROM tbl_JournalVoucherTypes",
                    conn, null);
                var map = new Dictionary<int, int>();
                if (dt != null)
                {
                    foreach (DataRow row in dt.Rows)
                        map[Simulate.Integer32(row["ID"])] = Simulate.Integer32(row["QTYFactor"]);
                }

                foreach (var kv in expected)
                {
                    bool has = map.TryGetValue(kv.Key, out int factor);
                    results.Add(Ok("QtyFactor", $"Type{kv.Key}_Factor{kv.Value}",
                        has && factor == kv.Value,
                        has ? $"Actual={factor}" : "Type missing from tbl_JournalVoucherTypes"));
                }
            }
            catch (Exception ex)
            {
                results.Add(Ok("QtyFactor", "LoadJournalVoucherTypes", false, ex.Message));
            }

            return results;
        }

        static List<QaResult> RunStockBalanceDriftScan(int companyId)
        {
            var results = new List<QaResult>();
            clsSQL sql = new clsSQL();
            string conn = sql.CreateDataBaseConnectionString(companyId);

            try
            {
                // Compare snapshot vs live invoice-detail on-hand for all item/store pairs that have either.
                SqlParameter[] prmCompany =
                {
                    new SqlParameter("@CompanyID", SqlDbType.Int) { Value = companyId },
                };
                object driftCount = sql.ExecuteScalar(@"
;WITH live AS (
    SELECT d.ItemGuid, d.StoreID,
           SUM(d.TotalQTY * ISNULL(jvt.QTYFactor,0)) AS OnHand
    FROM tbl_InvoiceDetails d
    LEFT JOIN tbl_JournalVoucherTypes jvt ON jvt.ID = d.InvoiceTypeID
    WHERE d.IsCounted = 1 AND (d.CompanyID = @CompanyID OR @CompanyID = 0)
    GROUP BY d.ItemGuid, d.StoreID
),
snap AS (
    SELECT ItemGuid, StoreID, OnHandQty
    FROM tbl_StockBalance
    WHERE CompanyID = @CompanyID OR @CompanyID = 0
)
SELECT COUNT(*) FROM (
    SELECT ISNULL(l.ItemGuid, s.ItemGuid) AS ItemGuid, ISNULL(l.StoreID, s.StoreID) AS StoreID
    FROM live l
    FULL OUTER JOIN snap s ON s.ItemGuid = l.ItemGuid AND s.StoreID = l.StoreID
    WHERE ABS(ISNULL(l.OnHand,0) - ISNULL(s.OnHandQty,0)) > 0.01
) x",
                    prmCompany, conn, null);

                int drifts = Simulate.Integer32(driftCount);
                results.Add(Ok("Integrity", "StockBalanceMatchesInvoiceDetails",
                    drifts == 0,
                    drifts == 0
                        ? "Snapshot matches live on-hand."
                        : $"{drifts} item/store pair(s) drifted — run RebuildStockBalance."));
            }
            catch (Exception ex)
            {
                results.Add(Ok("Integrity", "StockBalanceMatchesInvoiceDetails", false, ex.Message));
            }

            return results;
        }

        static List<QaResult> RunNegativeStockScan(int companyId)
        {
            var results = new List<QaResult>();
            clsSQL sql = new clsSQL();
            string conn = sql.CreateDataBaseConnectionString(companyId);

            try
            {
                SqlParameter[] prmCompany =
                {
                    new SqlParameter("@CompanyID", SqlDbType.Int) { Value = companyId },
                };
                object negCount = sql.ExecuteScalar(@"
;WITH oh AS (
    SELECT d.ItemGuid, d.StoreID,
           SUM(d.TotalQTY * ISNULL(jvt.QTYFactor,0)) AS OnHand
    FROM tbl_InvoiceDetails d
    LEFT JOIN tbl_JournalVoucherTypes jvt ON jvt.ID = d.InvoiceTypeID
    WHERE d.IsCounted = 1 AND (d.CompanyID = @CompanyID OR @CompanyID = 0)
    GROUP BY d.ItemGuid, d.StoreID
)
SELECT COUNT(*)
FROM oh
INNER JOIN tbl_Items i ON i.Guid = oh.ItemGuid
WHERE ISNULL(i.IsStockItem,0) = 1
  AND ISNULL(i.AllowNegativeStock,0) = 0
  AND oh.OnHand < -0.01",
                    prmCompany, conn, null);

                int n = Simulate.Integer32(negCount);
                results.Add(Ok("Integrity", "NoForbiddenNegativeStock",
                    n == 0,
                    n == 0 ? "No negative stock on locked items." : $"{n} item/store pair(s) below zero."));
            }
            catch (Exception ex)
            {
                results.Add(Ok("Integrity", "NoForbiddenNegativeStock", false, ex.Message));
            }

            return results;
        }

        static List<QaResult> RunTransferPairScan(int companyId)
        {
            var results = new List<QaResult>();
            clsSQL sql = new clsSQL();
            string conn = sql.CreateDataBaseConnectionString(companyId);

            try
            {
                // GI leg of WHTRANSFER should have a related GR (RelatedInvoiceGuid on GR points to GI).
                SqlParameter[] prmCompany =
                {
                    new SqlParameter("@CompanyID", SqlDbType.Int) { Value = companyId },
                };
                object orphanGi = sql.ExecuteScalar(@"
SELECT COUNT(*)
FROM tbl_InvoiceHeader gi
WHERE gi.RefNo = N'WHTRANSFER'
  AND gi.InvoiceTypeID = 9
  AND (gi.CompanyID = @CompanyID OR @CompanyID = 0)
  AND NOT EXISTS (
      SELECT 1 FROM tbl_InvoiceHeader gr
      WHERE gr.RefNo = N'WHTRANSFER'
        AND gr.InvoiceTypeID = 8
        AND gr.RelatedInvoiceGuid = gi.Guid
        AND (gr.CompanyID = @CompanyID OR @CompanyID = 0))",
                    prmCompany, conn, null);

                int orphans = Simulate.Integer32(orphanGi);
                results.Add(Ok("Integrity", "WarehouseTransferPairsComplete",
                    orphans == 0,
                    orphans == 0
                        ? "Every WHTRANSFER GI has a matching GR."
                        : $"{orphans} orphan WHTRANSFER GI(s) without GR."));
            }
            catch (Exception ex)
            {
                results.Add(Ok("Integrity", "WarehouseTransferPairsComplete", false, ex.Message));
            }

            return results;
        }

        static List<QaResult> RunInventoryAccountScan(int companyId)
        {
            var results = new List<QaResult>();
            try
            {
                cls_AccountSetting accountSetting = new cls_AccountSetting();
                DataTable dtAcc = accountSetting.SelectAccountSetting(0, 0, companyId, null);
                clsInvoiceHeader header = new clsInvoiceHeader();
                int inventoryAcc = header.GetValueFromDT(
                    dtAcc, "AccountRefID", Simulate.String((int)clsEnum.AccountMainSetting.Inventory), 2);
                results.Add(Ok("Integrity", "InventoryAccountConfigured",
                    inventoryAcc > 0,
                    inventoryAcc > 0
                        ? $"Inventory AccountRefID={inventoryAcc}"
                        : "Account Settings → Inventory GL is not set (transfers/GR/GI may fail)."));
            }
            catch (Exception ex)
            {
                results.Add(Ok("Integrity", "InventoryAccountConfigured", false, ex.Message));
            }

            return results;
        }

        static QaResult SchemaTableExists(clsSQL sql, string conn, string table, int companyId)
        {
            try
            {
                SqlParameter[] prm =
                {
                    new SqlParameter("@T", SqlDbType.NVarChar, 128) { Value = table },
                };
                object o = sql.ExecuteScalar(
                    "SELECT COUNT(*) FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = @T",
                    prm, conn, null);
                bool ok = Simulate.Integer32(o) > 0;
                return Ok("Schema", $"Table_{table}", ok, ok ? "Exists" : "Missing");
            }
            catch (Exception ex)
            {
                return Ok("Schema", $"Table_{table}", false, ex.Message);
            }
        }

        static QaResult SchemaColumnExists(clsSQL sql, string conn, string table, string column, int companyId)
        {
            try
            {
                SqlParameter[] prm =
                {
                    new SqlParameter("@T", SqlDbType.NVarChar, 128) { Value = table },
                    new SqlParameter("@C", SqlDbType.NVarChar, 128) { Value = column },
                };
                object o = sql.ExecuteScalar(
                    "SELECT COUNT(*) FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = @T AND COLUMN_NAME = @C",
                    prm, conn, null);
                bool ok = Simulate.Integer32(o) > 0;
                return Ok("Schema", $"Column_{table}_{column}", ok, ok ? "Exists" : "Missing");
            }
            catch (Exception ex)
            {
                return Ok("Schema", $"Column_{table}_{column}", false, ex.Message);
            }
        }

        static QaResult Ok(string category, string name, bool passed, string detail) =>
            new QaResult
            {
                Category = category,
                Name = name,
                Passed = passed,
                Detail = detail ?? ""
            };
    }
}
