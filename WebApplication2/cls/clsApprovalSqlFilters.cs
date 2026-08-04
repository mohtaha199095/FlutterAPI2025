using System.Text.RegularExpressions;

namespace WebApplication2.cls
{
    /// <summary>
    /// Ensures financial reports only include posted journal vouchers.
    /// </summary>
    public static class clsApprovalSqlFilters
    {
        public const int PostedStatus = 2;

        public const string PostedJvHeaderPredicate =
            " ISNULL(tbl_JournalVoucherHeader.DocumentStatus, 2) = 2 ";

        private static readonly Regex JvHeaderJoinRegex = new Regex(
            @"(?<header>tbl_JournalVoucherHeader)\.(?<hguid>guid|Guid)\s*=\s*(?<alias>tbl_[Jj]ournal[Vv]oucher[Dd]etails|[aA])\.(?<pguid>Parentguid|ParentGuid|parentguid)",
            RegexOptions.Compiled);

        private static readonly Regex JvHeaderJoinRegexReversed = new Regex(
            @"(?<alias>tbl_[Jj]ournal[Vv]oucher[Dd]etails|[aA])\.(?<pguid>Parentguid|ParentGuid|parentguid)\s*=\s*(?<header>tbl_JournalVoucherHeader)\.(?<hguid>guid|Guid)",
            RegexOptions.Compiled);

        private static readonly Regex JvHeaderAliasJoinRegex = new Regex(
            @"(?<header>tbl_JournalVoucherHeader|h)\.(?<hguid>[Gg]uid)\s*=\s*(?<alias>tbl_JournalVoucherDetails|tbl_journalvoucherdetails|d|a)\.(?<pguid>[Pp]arent[Gg]uid|[Pp]arentguid)",
            RegexOptions.Compiled | RegexOptions.IgnoreCase);

        public static string ApplyPostedJvFilter(string sql)
        {
            if (string.IsNullOrWhiteSpace(sql) || sql.Contains("DocumentStatus"))
                return sql;

            sql = JvHeaderJoinRegex.Replace(sql, m =>
                m.Value + " AND ISNULL(" + m.Groups["header"].Value + ".DocumentStatus, 2) = 2 ");

            sql = JvHeaderJoinRegexReversed.Replace(sql, m =>
                m.Value + " AND ISNULL(" + m.Groups["header"].Value + ".DocumentStatus, 2) = 2 ");

            sql = JvHeaderAliasJoinRegex.Replace(sql, m =>
                m.Value + " AND ISNULL(" + m.Groups["header"].Value + ".DocumentStatus, 2) = 2 ");

            return sql;
        }
    }
}
