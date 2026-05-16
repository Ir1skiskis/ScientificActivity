using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace ScientificActivityBusinessLogics.Helpers
{
    public static class DuplicateKeyHelper
    {
        public static string BuildPublicationKey(string? title, int year)
        {
            var normalizedTitle = NormalizeTextForKey(title);
            return $"{normalizedTitle}|{year}";
        }

        public static string BuildConferenceKey(string? title, DateTime startDate, string? city)
        {
            var normalizedTitle = NormalizeTextForKey(title);
            var normalizedCity = NormalizeTextForKey(city);
            return $"{normalizedTitle}|{startDate:yyyy-MM-dd}|{normalizedCity}";
        }

        private static string NormalizeTextForKey(string? value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return string.Empty;
            }

            value = value.Trim().ToUpperInvariant();
            value = Regex.Replace(value, @"\s+", " ");

            return value;
        }
    }
}
