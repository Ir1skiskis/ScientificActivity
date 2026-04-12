using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScientificActivityParsers.Models
{
    public class ELibraryPublicationImportModel
    {
        public string Title { get; set; } = string.Empty;

        public string Authors { get; set; } = string.Empty;

        public int? Year { get; set; }

        public string? Url { get; set; }

        public string? JournalTitle { get; set; }

        public string? Keywords { get; set; }

        public string? Annotation { get; set; }
    }
}
