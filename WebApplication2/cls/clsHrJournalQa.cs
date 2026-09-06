using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using WebApplication2.MainClasses;

namespace WebApplication2.cls
{
    /// <summary>
    /// Automatic journal QA: validates HR JV patterns in-memory and scans company DB for unbalanced HR journals.
    /// </summary>
    public static class clsHrJournalQa
    {
        public const int PayrollVoucherTypeId = (int)clsEnum.VoucherType.Payroll;

        public class JournalLine
        {
            public int AccountId { get; set; }
            public decimal Debit { get; set; }
            public decimal Credit { get; set; }
            public decimal Total { get; set; }
        }

        public class UnbalancedJournal
        {
            public string Guid { get; set; }
            public string Notes { get; set; }
            public int JvTypeId { get; set; }
            public decimal TotalDebit { get; set; }
            public decimal TotalCredit { get; set; }
            public decimal TotalLine { get; set; }
        }

        public static bool IsBalanced(IEnumerable<JournalLine> lines)
        {
            decimal totalDebit = 0;
            decimal totalCredit = 0;
            decimal totalLine = 0;
            foreach (JournalLine line in lines.Where(l => l.AccountId > 0))
            {
                totalDebit += line.Debit;
                totalCredit += line.Credit;
                totalLine += line.Total;
            }
            return totalDebit == totalCredit && totalLine == 0;
        }

        /// <summary>Pattern checks mirroring encashment, EOS, and payroll JV layouts (no DB).</summary>
        public static List<clsHrQaHarness.QaResult> RunFixtureChecks()
        {
            var results = new List<clsHrQaHarness.QaResult>();

            results.Add(CheckPattern(
                "JournalPattern_LeaveEncashment",
                BuildEncashmentLines(750m)));

            results.Add(CheckPattern(
                "JournalPattern_EndOfService",
                BuildEosLines(4200.500m)));

            results.Add(CheckPattern(
                "JournalPattern_PayrollNetPay",
                BuildPayrollNetPayLines(885m, 115m, 142.5m)));

            results.Add(CheckPattern(
                "JournalPattern_RejectUnbalanced",
                new List<JournalLine>
                {
                    new JournalLine { AccountId = 100, Debit = 100m, Credit = 0m, Total = 100m },
                    new JournalLine { AccountId = 200, Debit = 0m, Credit = 90m, Total = -90m },
                },
                expectBalanced: false));

            return results;
        }

        static clsHrQaHarness.QaResult CheckPattern(
            string name,
            List<JournalLine> lines,
            bool expectBalanced = true)
        {
            bool balanced = IsBalanced(lines);
            bool passed = balanced == expectBalanced;
            decimal td = lines.Where(l => l.AccountId > 0).Sum(l => l.Debit);
            decimal tc = lines.Where(l => l.AccountId > 0).Sum(l => l.Credit);
            return new clsHrQaHarness.QaResult
            {
                Category = "JournalQA",
                Name = name,
                Passed = passed,
                Detail = $"Debit={td} Credit={tc} Balanced={balanced}"
            };
        }

        static List<JournalLine> BuildEncashmentLines(decimal amount)
        {
            return new List<JournalLine>
            {
                new JournalLine { AccountId = 5100, Debit = amount, Credit = 0m, Total = amount },
                new JournalLine { AccountId = 2100, Debit = 0m, Credit = amount, Total = -amount },
            };
        }

        static List<JournalLine> BuildEosLines(decimal amount)
        {
            return new List<JournalLine>
            {
                new JournalLine { AccountId = 5200, Debit = amount, Credit = 0m, Total = amount },
                new JournalLine { AccountId = 2100, Debit = 0m, Credit = amount, Total = -amount },
            };
        }

        /// <summary>Expense DR, payable CR, employer contribution tracked separately if posted.</summary>
        static List<JournalLine> BuildPayrollNetPayLines(decimal netPay, decimal eeDeductions, decimal erContribution)
        {
            decimal gross = netPay + eeDeductions;
            return new List<JournalLine>
            {
                new JournalLine { AccountId = 5300, Debit = gross + erContribution, Credit = 0m, Total = gross + erContribution },
                new JournalLine { AccountId = 2100, Debit = 0m, Credit = netPay, Total = -netPay },
                new JournalLine { AccountId = 2200, Debit = 0m, Credit = eeDeductions, Total = -eeDeductions },
                new JournalLine { AccountId = 2300, Debit = 0m, Credit = erContribution, Total = -erContribution },
            };
        }

        public static List<UnbalancedJournal> ScanUnbalancedHrJournals(int companyId)
        {
            if (companyId <= 0) return new List<UnbalancedJournal>();

            clsSQL sql = new clsSQL();
            string conn = sql.CreateDataBaseConnectionString(companyId);
            if (string.IsNullOrWhiteSpace(conn))
                return new List<UnbalancedJournal>();

            if (!TableExists(sql, conn, "tbl_JournalVoucherHeader")
                || !TableExists(sql, conn, "tbl_JournalVoucherDetails"))
            {
                return new List<UnbalancedJournal>();
            }

            SqlParameter[] prm =
            {
                new SqlParameter("@CompanyID", SqlDbType.Int) { Value = companyId },
                new SqlParameter("@PayrollType", SqlDbType.Int) { Value = PayrollVoucherTypeId },
            };

            DataTable dt = sql.ExecuteQueryStatement(@"
SELECT h.Guid, ISNULL(h.Notes,N'') AS Notes, ISNULL(h.JVTypeID,0) AS JVTypeID,
       SUM(ISNULL(d.Debit,0)) AS TotalDebit,
       SUM(ISNULL(d.Credit,0)) AS TotalCredit,
       SUM(ISNULL(d.Total,0)) AS TotalLine
FROM tbl_JournalVoucherHeader h
INNER JOIN tbl_JournalVoucherDetails d ON d.ParentGuid = h.Guid AND d.CompanyID = h.CompanyID
WHERE h.CompanyID = @CompanyID
  AND ISNULL(d.AccountID,0) > 0
  AND (
        h.JVTypeID = @PayrollType
        OR ISNULL(h.Notes,N'') LIKE N'%encashment%'
        OR ISNULL(h.Notes,N'') LIKE N'%EOS%'
        OR ISNULL(h.Notes,N'') LIKE N'%End-of-service%'
        OR ISNULL(h.Notes,N'') LIKE N'%Leave encashment%'
        OR ISNULL(h.Notes,N'') LIKE N'%payroll%'
      )
GROUP BY h.Guid, h.Notes, h.JVTypeID
HAVING SUM(ISNULL(d.Debit,0)) <> SUM(ISNULL(d.Credit,0))
    OR SUM(ISNULL(d.Total,0)) <> 0",
                sql.CreateDataBaseConnectionString(companyId), prm);

            var rows = new List<UnbalancedJournal>();
            if (dt == null) return rows;

            foreach (DataRow row in dt.Rows)
            {
                rows.Add(new UnbalancedJournal
                {
                    Guid = Simulate.String(row["Guid"]),
                    Notes = Simulate.String(row["Notes"]),
                    JvTypeId = Simulate.Integer32(row["JVTypeID"]),
                    TotalDebit = Simulate.Decimal(row["TotalDebit"]),
                    TotalCredit = Simulate.Decimal(row["TotalCredit"]),
                    TotalLine = Simulate.Decimal(row["TotalLine"]),
                });
            }
            return rows;
        }

        static bool TableExists(clsSQL sql, string conn, string tableName)
        {
            try
            {
                SqlParameter[] prm =
                {
                    new SqlParameter("@Table", SqlDbType.NVarChar, 128) { Value = tableName },
                };
                object val = sql.ExecuteScalar(
                    "SELECT COUNT(*) FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME=@Table",
                    prm, conn, null);
                return Simulate.Integer32(val) > 0;
            }
            catch
            {
                return false;
            }
        }

        public static List<clsHrQaHarness.QaResult> RunDatabaseScan(int companyId)
        {
            var results = new List<clsHrQaHarness.QaResult>();
            if (companyId <= 0)
            {
                results.Add(new clsHrQaHarness.QaResult
                {
                    Category = "JournalQA",
                    Name = "DatabaseScanSkipped",
                    Passed = true,
                    Detail = "CompanyID not supplied — in-memory journal patterns only."
                });
                return results;
            }

            try
            {
                clsSQL sql = new clsSQL();
                string conn = sql.CreateDataBaseConnectionString(companyId);
                if (!TableExists(sql, conn, "tbl_JournalVoucherHeader"))
                {
                    results.Add(new clsHrQaHarness.QaResult
                    {
                        Category = "JournalQA",
                        Name = "JournalTablesPresent",
                        Passed = false,
                        Detail = "Journal tables not found in company database. " +
                                 "Log out and log in once to run DB migrations, or use a company with GL enabled."
                    });
                    return results;
                }

                List<UnbalancedJournal> bad = ScanUnbalancedHrJournals(companyId);
                results.Add(new clsHrQaHarness.QaResult
                {
                    Category = "JournalQA",
                    Name = "NoUnbalancedHrJournalsInDatabase",
                    Passed = bad.Count == 0,
                    Detail = bad.Count == 0
                        ? "All scanned HR/payroll journals are balanced."
                        : $"{bad.Count} unbalanced journal(s): " +
                          string.Join("; ", bad.Take(5).Select(b =>
                              $"{b.Notes} (DR={b.TotalDebit}, CR={b.TotalCredit}, TL={b.TotalLine})"))
                });

                foreach (UnbalancedJournal row in bad.Take(10))
                {
                    results.Add(new clsHrQaHarness.QaResult
                    {
                        Category = "JournalQA",
                        Name = "UnbalancedJournal_" + row.Guid.Substring(0, 8),
                        Passed = false,
                        Detail = $"{row.Notes} | Type={row.JvTypeId} DR={row.TotalDebit} CR={row.TotalCredit} TL={row.TotalLine}"
                    });
                }
            }
            catch (Exception ex)
            {
                results.Add(new clsHrQaHarness.QaResult
                {
                    Category = "JournalQA",
                    Name = "DatabaseScanError",
                    Passed = false,
                    Detail = ex.Message
                });
            }

            return results;
        }
    }
}
