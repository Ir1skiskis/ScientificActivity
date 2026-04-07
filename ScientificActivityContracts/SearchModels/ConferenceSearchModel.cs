using ScientificActivityDataModels.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScientificActivityContracts.SearchModels
{
    public class ConferenceSearchModel
    {
        public int? Id { get; set; }

        public string? Title { get; set; }

        public string? City { get; set; }

        public string? Country { get; set; }

        public string? SubjectArea { get; set; }

        public ConferenceFormat? Format { get; set; }

        public ConferenceLevel? Level { get; set; }

        public DateTime? DateFrom { get; set; }

        public DateTime? DateTo { get; set; }
    }
}
