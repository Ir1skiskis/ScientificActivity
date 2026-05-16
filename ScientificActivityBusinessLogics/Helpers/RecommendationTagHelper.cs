using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace ScientificActivityBusinessLogics.Helpers
{
    public static class RecommendationTagHelper
    {
        public static List<string> ExtractTags(string? value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return new List<string>();
            }

            return value
                .Split(new[] { ';', ',', '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries)
                .Select(NormalizeTag)
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .OrderBy(x => x)
                .ToList();
        }

        public static string NormalizeTag(string? value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return string.Empty;
            }

            value = value.Trim().ToLowerInvariant();
            value = Regex.Replace(value, @"\s+", " ");

            return value;
        }

        public static bool HasIntersection(IEnumerable<string> firstTags, IEnumerable<string> secondTags)
        {
            var first = firstTags
                .Select(NormalizeTag)
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .ToHashSet(StringComparer.OrdinalIgnoreCase);

            return secondTags
                .Select(NormalizeTag)
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .Any(first.Contains);
        }
    }
}
