using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScientificActivityContracts.SearchModels
{
    public class JournalVakSpecialtySearchModel
    {
        public int? Id { get; set; }
        public int? JournalId { get; set; }
        public string? SpecialtyCode { get; set; }
        public DateTime? DateFrom { get; set; }
        public DateTime? DateTo { get; set; }
    }
}
