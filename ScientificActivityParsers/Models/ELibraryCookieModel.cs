namespace ScientificActivityParsers.Models
{
    public class ELibraryCookieModel
    {
        public string Name { get; set; } = string.Empty;

        public string Value { get; set; } = string.Empty;

        public string Domain { get; set; } = ".elibrary.ru";

        public string Path { get; set; } = "/";

        public DateTime? Expiry { get; set; }
    }
}
