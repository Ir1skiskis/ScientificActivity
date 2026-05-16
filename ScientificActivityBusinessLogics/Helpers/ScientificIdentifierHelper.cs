using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace ScientificActivityBusinessLogics.Helpers
{
    public static class ScientificIdentifierHelper
    {
        public static string? NormalizeIssn(string? value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return null;
            }

            value = value.Trim();
            value = value.Replace("Х", "X");
            value = value.Replace("х", "X");
            value = value.ToUpperInvariant();

            value = Regex.Replace(value, @"[^0-9X]", "");

            if (value.Length != 8)
            {
                return value;
            }

            return $"{value[..4]}-{value[4..]}";
        }
    }
}
