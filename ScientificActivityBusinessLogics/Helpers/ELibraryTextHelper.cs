using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace ScientificActivityBusinessLogics.Helpers
{
    public static class ELibraryTextHelper
    {
        private static readonly CultureInfo RussianCulture = new("ru-RU");

        public static string NormalizeFullName(string? value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return string.Empty;
            }

            value = Regex.Replace(value.Trim(), @"\s+", " ");
            value = value.ToLower(RussianCulture);

            return RussianCulture.TextInfo.ToTitleCase(value);
        }

        public static string NormalizeDepartment(string? value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return string.Empty;
            }

            value = Regex.Replace(value, @"SPIN-код:.*", "", RegexOptions.IgnoreCase).Trim();
            value = Regex.Replace(value, @"AuthorID:.*", "", RegexOptions.IgnoreCase).Trim();
            value = Regex.Replace(value, @"\([^)]*\)", "").Trim();
            value = Regex.Replace(value, @"\s+", " ").Trim();

            if (string.IsNullOrWhiteSpace(value))
            {
                return string.Empty;
            }

            value = value.ToLower(RussianCulture);

            return char.ToUpper(value[0], RussianCulture) + value[1..];
        }
    }
}
