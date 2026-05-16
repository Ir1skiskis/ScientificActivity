using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScientificActivityBusinessLogics.Helpers
{
    public static class RecommendationMathHelper
    {
        public static double CalculateCosineSimilarity(IEnumerable<string> firstTags, IEnumerable<string> secondTags)
        {
            var first = NormalizeTags(firstTags);
            var second = NormalizeTags(secondTags);

            if (first.Count == 0 || second.Count == 0)
            {
                return 0;
            }

            var allTags = first
                .Union(second, StringComparer.OrdinalIgnoreCase)
                .ToList();

            var dotProduct = allTags.Sum(tag =>
                (first.Contains(tag, StringComparer.OrdinalIgnoreCase) ? 1 : 0) *
                (second.Contains(tag, StringComparer.OrdinalIgnoreCase) ? 1 : 0));

            var firstLength = Math.Sqrt(first.Count);
            var secondLength = Math.Sqrt(second.Count);

            if (firstLength == 0 || secondLength == 0)
            {
                return 0;
            }

            return dotProduct / (firstLength * secondLength);
        }

        public static List<string> NormalizeTags(IEnumerable<string> tags)
        {
            return tags
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .Select(x => x.Trim().ToLowerInvariant())
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .OrderBy(x => x)
                .ToList();
        }
    }
}
