namespace ScientificActivityClientApp.Models
{
    public static class TextHelper
    {
        public static string Truncate(string? value, int maxLength)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return string.Empty;
            }

            if (value.Length <= maxLength)
            {
                return value;
            }

            return value[..maxLength].TrimEnd() + "...";
        }
    }
}
