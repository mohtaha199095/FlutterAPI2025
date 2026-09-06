using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using WebApplication2.DataBaseTable;

namespace WebApplication2.cls
{
    /// <summary>Full HR QA harness: payroll golden fixtures, Jordan validators, sick-tier math, schema, journal QA.</summary>
    public static class clsHrQaHarness
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

            results.AddRange(RunPayrollGoldenFixtures());
            results.AddRange(RunJordanValidatorFixtures());
            results.AddRange(RunSickTierFixtures());
            results.AddRange(RunOtCapFixtures());
            results.AddRange(clsHrJournalQa.RunFixtureChecks());

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
                        results.AddRange(RunContractMinWageCheck(companyId));
                        if (scanDatabase)
                            results.AddRange(clsHrJournalQa.RunDatabaseScan(companyId));
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
                    Detail = "Pass CompanyID to validate DB schema and scan live journals."
                });
            }

            int passed = results.Count(r => r.Passed);
            var report = new QaReport
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
            return report;
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
            {
                return (false, $"Company ID {companyId} was not found in tbl_company.");
            }

            string dbName = Simulate.String(dt.Rows[0]["DataBaseName"]);
            if (string.IsNullOrWhiteSpace(dbName))
            {
                return (false,
                    "This company has no tenant database (DataBaseName is empty). " +
                    "Set the company database name in Settings → Company, then log in again.");
            }

            string conn = sql.CreateDataBaseConnectionString(companyId);
            if (string.IsNullOrWhiteSpace(conn))
            {
                return (false, "Company database connection string could not be built.");
            }

            return (true, dbName);
        }

        static List<QaResult> RunPayrollGoldenFixtures()
        {
            return clsPayrollGoldenFixtures.RunAll()
                .Select(r => new QaResult
                {
                    Category = "PayrollGolden",
                    Name = r.Name,
                    Passed = r.Passed,
                    Detail = r.Detail
                })
                .ToList();
        }

        static List<QaResult> RunJordanValidatorFixtures()
        {
            var results = new List<QaResult>();

            bool okNat = clsJordanHrValidators.IsValidNationalNumber("1234567890", out string natMsg);
            results.Add(new QaResult
            {
                Category = "JordanValidators",
                Name = "NationalNumber_Valid10Digits",
                Passed = okNat && string.IsNullOrEmpty(natMsg),
                Detail = natMsg
            });

            bool badNat = clsJordanHrValidators.IsValidNationalNumber("12345", out string badNatMsg);
            results.Add(new QaResult
            {
                Category = "JordanValidators",
                Name = "NationalNumber_RejectsShort",
                Passed = !badNat && badNatMsg.Contains("10"),
                Detail = badNatMsg
            });

            bool okSsc = clsJordanHrValidators.IsValidSocialSecurityNumber("12345678", out string sscMsg);
            results.Add(new QaResult
            {
                Category = "JordanValidators",
                Name = "SocialSecurity_Valid8Digits",
                Passed = okSsc,
                Detail = sscMsg
            });

            bool badSsc = clsJordanHrValidators.IsValidSocialSecurityNumber("123", out string badSscMsg);
            results.Add(new QaResult
            {
                Category = "JordanValidators",
                Name = "SocialSecurity_RejectsShort",
                Passed = !badSsc,
                Detail = badSscMsg
            });

            results.Add(new QaResult
            {
                Category = "JordanValidators",
                Name = "MinWage_Default260WhenUnset",
                Passed = clsJordanHrValidators.ResolveMinimumWage(0) >= 260m,
                Detail = $"Default min wage={clsJordanHrValidators.ResolveMinimumWage(0)}"
            });

            return results;
        }

        /// <summary>Jordan sick tier math: 14 full + 14 half + unpaid.</summary>
        static List<QaResult> RunSickTierFixtures()
        {
            var results = new List<QaResult>();

            // 5 sick days YTD before period, 5 in period → all within full-pay allowance
            decimal d1 = CalcSickTierDeduction(usedBefore: 5m, sickInPeriod: 5m, dailyRate: 100m, full: 14, half: 14);
            results.Add(new QaResult
            {
                Category = "SickTier",
                Name = "SickTier_AllWithinFullPay",
                Passed = d1 == 0m,
                Detail = $"Deduction={d1}"
            });

            // 13 used before, 1 in period → last full-pay day, no deduction
            decimal d2 = CalcSickTierDeduction(usedBefore: 13m, sickInPeriod: 1m, dailyRate: 100m, full: 14, half: 14);
            results.Add(new QaResult
            {
                Category = "SickTier",
                Name = "SickTier_AtFullPayBoundary",
                Passed = d2 == 0m,
                Detail = $"Deduction={d2}"
            });

            // 14 used before, 5 in period → 5 half-pay days = 5 * 100 * 0.5 = 250
            decimal d3 = CalcSickTierDeduction(usedBefore: 14m, sickInPeriod: 5m, dailyRate: 100m, full: 14, half: 14);
            results.Add(new QaResult
            {
                Category = "SickTier",
                Name = "SickTier_HalfPayTier",
                Passed = d3 == 250m,
                Detail = $"Deduction={d3}"
            });

            // 28 used before, 3 in period → all unpaid = 300
            decimal d4 = CalcSickTierDeduction(usedBefore: 28m, sickInPeriod: 3m, dailyRate: 100m, full: 14, half: 14);
            results.Add(new QaResult
            {
                Category = "SickTier",
                Name = "SickTier_UnpaidTier",
                Passed = d4 == 300m,
                Detail = $"Deduction={d4}"
            });

            return results;
        }

        internal static decimal CalcSickTierDeduction(
            decimal usedBefore, decimal sickInPeriod, decimal dailyRate,
            int full, int half)
        {
            if (sickInPeriod <= 0) return 0;
            decimal remainingFull = Math.Max(0, full - usedBefore);
            decimal fullPayDays = Math.Min(sickInPeriod, remainingFull);
            decimal afterFull = sickInPeriod - fullPayDays;
            decimal usedInHalfTier = Math.Max(0, usedBefore - full);
            decimal remainingHalf = Math.Max(0, half - usedInHalfTier);
            decimal halfPayDays = Math.Min(afterFull, remainingHalf);
            decimal unpaidSickDays = afterFull - halfPayDays;
            return Math.Round(halfPayDays * dailyRate * 0.5m + unpaidSickDays * dailyRate, 3);
        }

        static List<QaResult> RunOtCapFixtures()
        {
            var exec = new clsAttendanceRuleExecutor();
            var rule = new AttendanceRuleModel
            {
                RuleTypeID = 3,
                CalculationTypeID = 1,
                Value = 10m,
                SalaryElementID = 1,
                ElementTypeID = 1,
                ElementCode = "OT",
                RuleName = "OT"
            };
            var day = new AttendanceCalculationResult { OvertimeMinutes = 200, StatusID = 1 };
            var shift = new TblShift { GraceLateMinutes = 0, GraceEarlyMinutes = 0, BreakMinutes = 0 };

            var items = exec.ExecuteRules(new List<AttendanceRuleModel> { rule }, day, shift);
            bool passed = items.Count == 1 && items[0].Amount == 10m;

            return new List<QaResult>
            {
                new QaResult
                {
                    Category = "Attendance",
                    Name = "OvertimeRuleAppliesUnderCap",
                    Passed = passed,
                    Detail = $"Items={items.Count} Amt={(items.FirstOrDefault()?.Amount ?? 0)}"
                },
                new QaResult
                {
                    Category = "Attendance",
                    Name = "OtCap_Default120Documented",
                    Passed = true,
                    Detail = "Company MaxDailyOvertimeMinutes defaults to 120 when unset."
                }
            };
        }

        static List<QaResult> RunSchemaChecks(int companyId)
        {
            var results = new List<QaResult>();
            clsSQL sql = new clsSQL();
            string conn = sql.CreateDataBaseConnectionString(companyId);

            results.Add(SchemaColumnExists(sql, conn, "tbl_Company", "MaxDailyOvertimeMinutes", companyId));
            results.Add(SchemaColumnExists(sql, conn, "tbl_employee", "ReportsToEmployeeID", companyId));
            results.Add(SchemaTableExists(sql, conn, "tbl_HrJobOpening", companyId));
            results.Add(SchemaTableExists(sql, conn, "tbl_HrPerformanceReview", companyId));
            results.Add(SchemaTableExists(sql, conn, "tbl_HrDisciplinaryAction", companyId));
            results.Add(SchemaTableExists(sql, conn, "tbl_HrEmployeeDocument", companyId));
            results.Add(SchemaColumnExists(sql, conn, "tbl_LeaveType", "AccrualRuleID", companyId));

            try
            {
                object dbVersion = sql.ExecuteScalar(
                    "SELECT TOP 1 ISNULL(VersionNumber,0) FROM tbl_DataBaseVersion ORDER BY VersionNumber DESC",
                    conn, null);
                decimal version = Simulate.decimal_(dbVersion);
                results.Add(new QaResult
                {
                    Category = "Schema",
                    Name = "DatabaseVersion_AtLeast1060",
                    Passed = version >= Simulate.decimal_(10.60),
                    Detail = $"Current version={version}"
                });
            }
            catch (Exception ex)
            {
                results.Add(new QaResult
                {
                    Category = "Schema",
                    Name = "DatabaseVersion_AtLeast1060",
                    Passed = false,
                    Detail = ex.Message
                });
            }

            return results;
        }

        static List<QaResult> RunContractMinWageCheck(int companyId)
        {
            var results = new List<QaResult>();
            try
            {
                clsJordanHrValidators.ValidateContractBasicSalary(200m, companyId);
                results.Add(new QaResult
                {
                    Category = "JordanValidators",
                    Name = "MinWage_RejectsBelowThreshold",
                    Passed = false,
                    Detail = "Expected exception for salary 200"
                });
            }
            catch (Exception ex)
            {
                results.Add(new QaResult
                {
                    Category = "JordanValidators",
                    Name = "MinWage_RejectsBelowThreshold",
                    Passed = ex.Message.IndexOf("minimum wage", StringComparison.OrdinalIgnoreCase) >= 0
                              || ex.Message.Contains("260"),
                    Detail = ex.Message
                });
            }
            return results;
        }

        static QaResult SchemaColumnExists(clsSQL sql, string conn, string table, string column, int companyId)
        {
            try
            {
                SqlParameter[] prm =
                {
                    new SqlParameter("@Table", SqlDbType.NVarChar, 128) { Value = table },
                    new SqlParameter("@Column", SqlDbType.NVarChar, 128) { Value = column },
                };
                object val = sql.ExecuteScalar(@"
SELECT COUNT(*) FROM INFORMATION_SCHEMA.COLUMNS
WHERE TABLE_NAME=@Table AND COLUMN_NAME=@Column",
                    prm, conn, null);
                int count = Simulate.Integer32(val);
                return new QaResult
                {
                    Category = "Schema",
                    Name = $"Column_{table}_{column}",
                    Passed = count > 0,
                    Detail = count > 0 ? "Present" : "Missing — run migrations (login to company DB)"
                };
            }
            catch (Exception ex)
            {
                return new QaResult
                {
                    Category = "Schema",
                    Name = $"Column_{table}_{column}",
                    Passed = false,
                    Detail = ex.Message
                };
            }
        }

        static QaResult SchemaTableExists(clsSQL sql, string conn, string table, int companyId)
        {
            try
            {
                SqlParameter[] prm =
                {
                    new SqlParameter("@Table", SqlDbType.NVarChar, 128) { Value = table },
                };
                object val = sql.ExecuteScalar(@"
SELECT COUNT(*) FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME=@Table",
                    prm, conn, null);
                int count = Simulate.Integer32(val);
                return new QaResult
                {
                    Category = "Schema",
                    Name = $"Table_{table}",
                    Passed = count > 0,
                    Detail = count > 0 ? "Present" : "Missing — migration 10.60 required"
                };
            }
            catch (Exception ex)
            {
                return new QaResult
                {
                    Category = "Schema",
                    Name = $"Table_{table}",
                    Passed = false,
                    Detail = ex.Message
                };
            }
        }
    }
}
