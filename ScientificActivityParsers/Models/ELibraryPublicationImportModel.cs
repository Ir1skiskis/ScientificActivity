using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScientificActivityParsers.Models
{
    public class ELibraryPublicationImportModel
    {
        public string ELibraryId { get; set; } = string.Empty;

        public string Title { get; set; } = string.Empty;

        public string Authors { get; set; } = string.Empty;

        public int? Year { get; set; }

        public string? Url { get; set; }

        public string? Doi { get; set; }

        public string? JournalTitle { get; set; }

        public string? JournalIssn { get; set; }

        public string? Keywords { get; set; }

        public string? Annotation { get; set; }

        public int? CitationsRincCount { get; set; }

        public bool IsInRinc { get; set; }

        public bool IsInCoreRinc { get; set; }

        public bool IsWhiteListLevel1 { get; set; }

        public bool IsWhiteListLevel2 { get; set; }

        public bool IsWhiteListLevel3 { get; set; }

        public bool IsWhiteListLevel4 { get; set; }

        public bool IsRsci { get; set; }

        public bool IsScopusQ1 { get; set; }

        public bool IsScopusQ2 { get; set; }

        public bool IsScopusQ3 { get; set; }

        public bool IsScopusQ4 { get; set; }

        public bool IsWebOfScienceQ1 { get; set; }

        public bool IsWebOfScienceQ2 { get; set; }

        public bool IsWebOfScienceQ3 { get; set; }

        public bool IsWebOfScienceQ4 { get; set; }

        public bool IsWebOfScienceNoQuartile { get; set; }

        public bool IsVak { get; set; }

        public bool IsVakCategory1 { get; set; }

        public bool IsVakCategory2 { get; set; }

        public bool IsVakCategory3 { get; set; }

        public string? RubricOecd { get; set; }

        public string? RubricAsjc { get; set; }

        public string? RubricGrnti { get; set; }

        public string? VakSpecialty { get; set; }
    }
}
