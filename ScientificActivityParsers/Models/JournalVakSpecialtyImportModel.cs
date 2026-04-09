using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScientificActivityParsers.Models
{
    public class JournalVakSpecialtyImportModel
    {
        public string SpecialtyCode { get; set; } = string.Empty;
        public string SpecialtyName { get; set; } = string.Empty;
        public string ScienceBranch { get; set; } = string.Empty;

        public DateTime DateFrom { get; set; }
        public DateTime? DateTo { get; set; }
    }
}
