using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace WebApplication2.cls
{
    public static class clsAiChatFuzzyMatch
    {
        private const double ExactContainsScore = 0.96;
        private const double MinPickScore = 0.42;
        private const double ConfirmScore = 0.62;
        private const double AutoAcceptScore = 0.92;

        public static double AutoAcceptThreshold => AutoAcceptScore;
        public static double ConfirmThreshold => ConfirmScore;
        public static double MinMatchThreshold => MinPickScore;

        /// <summary>Normalize text for fuzzy comparison — handles Arabic letter variants.</summary>
        public static string NormalizeForMatch(string text)
        {
            if (string.IsNullOrWhiteSpace(text)) return "";

            var sb = new StringBuilder(text.Length);
            foreach (char c in text.Trim().ToLowerInvariant())
            {
                switch (c)
                {
                    case 'أ': case 'إ': case 'آ': case 'ٱ': case 'ء':
                        sb.Append('ا'); break;
                    case 'ة':
                        sb.Append('ه'); break;
                    case 'ى': case 'ئ':
                        sb.Append('ي'); break;
                    case 'ؤ':
                        sb.Append('و'); break;
                    case 'ـ': // tatweel
                        break;
                    default:
                        if (c >= '\u064B' && c <= '\u065F') break; // tashkeel
                        if (char.IsLetterOrDigit(c) || c == ' ') sb.Append(c);
                        break;
                }
            }

            string result = Regex.Replace(sb.ToString(), @"\s+", " ").Trim();

            // Strip leading definite article for matching
            if (result.StartsWith("ال") && result.Length > 3)
                result = result[2..];

            return result;
        }

        public static double ScoreMatch(string query, params string[] candidates)
        {
            if (string.IsNullOrWhiteSpace(query)) return 0;

            string q = NormalizeForMatch(query);
            if (q.Length == 0) return 0;

            double best = 0;
            foreach (string raw in candidates)
            {
                if (string.IsNullOrWhiteSpace(raw)) continue;
                best = Math.Max(best, ScorePair(q, NormalizeForMatch(raw)));
            }
            return best;
        }

        private static double ScorePair(string q, string c)
        {
            if (string.IsNullOrWhiteSpace(c)) return 0;
            if (q == c) return 1.0;
            if (c.Contains(q, StringComparison.Ordinal) || q.Contains(c, StringComparison.Ordinal))
                return ExactContainsScore;

            double levenshtein = LevenshteinRatio(q, c);
            double token = TokenOverlapScore(q, c);
            double prefix = PrefixScore(q, c);

            return Math.Max(levenshtein, Math.Max(token * 0.95, prefix * 0.88));
        }

        private static double TokenOverlapScore(string q, string c)
        {
            var qTokens = q.Split(' ', StringSplitOptions.RemoveEmptyEntries)
                .Where(t => t.Length >= 2).ToHashSet();
            var cTokens = c.Split(' ', StringSplitOptions.RemoveEmptyEntries)
                .Where(t => t.Length >= 2).ToList();

            if (qTokens.Count == 0 || cTokens.Count == 0) return 0;

            int hits = cTokens.Count(t => qTokens.Any(qt =>
                t.Contains(qt, StringComparison.Ordinal) ||
                qt.Contains(t, StringComparison.Ordinal) ||
                LevenshteinRatio(qt, t) >= 0.8));

            return (double)hits / Math.Max(qTokens.Count, cTokens.Count);
        }

        private static double PrefixScore(string q, string c)
        {
            int min = Math.Min(q.Length, c.Length);
            if (min < 2) return 0;

            int match = 0;
            for (int i = 0; i < min; i++)
            {
                if (q[i] == c[i]) match++;
                else break;
            }
            return (double)match / min;
        }

        private static double LevenshteinRatio(string a, string b)
        {
            if (a == b) return 1.0;
            int dist = LevenshteinDistance(a, b);
            int maxLen = Math.Max(a.Length, b.Length);
            if (maxLen == 0) return 1.0;
            return 1.0 - (double)dist / maxLen;
        }

        private static int LevenshteinDistance(string a, string b)
        {
            int n = a.Length, m = b.Length;
            if (n == 0) return m;
            if (m == 0) return n;

            int[] prev = new int[m + 1];
            int[] curr = new int[m + 1];

            for (int j = 0; j <= m; j++) prev[j] = j;

            for (int i = 1; i <= n; i++)
            {
                curr[0] = i;
                for (int j = 1; j <= m; j++)
                {
                    int cost = a[i - 1] == b[j - 1] ? 0 : 1;
                    curr[j] = Math.Min(
                        Math.Min(curr[j - 1] + 1, prev[j] + 1),
                        prev[j - 1] + cost);
                }
                (prev, curr) = (curr, prev);
            }
            return prev[m];
        }

        /// <summary>Build SQL LIKE patterns from normalized term and Arabic variants.</summary>
        public static IEnumerable<string> BuildLikePatterns(string term)
        {
            var patterns = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            string clean = term.Trim();
            if (clean.Length == 0) yield break;

            patterns.Add(clean);

            string norm = NormalizeForMatch(clean);
            if (!string.IsNullOrWhiteSpace(norm)) patterns.Add(norm);

            // Common Arabic search variants
            if (norm.Contains('ا'))
            {
                patterns.Add(norm.Replace('ا', 'أ'));
                patterns.Add(norm.Replace('ا', 'إ'));
            }
            if (norm.Contains('ه'))
                patterns.Add(norm.Replace('ه', 'ة'));
            if (norm.Contains('ي'))
                patterns.Add(norm.Replace('ي', 'ى'));

            foreach (string p in patterns)
            {
                if (p.Length >= 2)
                    yield return "%" + p + "%";
            }

            foreach (string word in norm.Split(' ', StringSplitOptions.RemoveEmptyEntries))
            {
                if (word.Length >= 2)
                    yield return "%" + word + "%";
            }
        }
    }
}
