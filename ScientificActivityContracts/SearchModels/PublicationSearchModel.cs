using ScientificActivityDataModels.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScientificActivityContracts.SearchModels
{
    public class PublicationSearchModel
    {
        public int? Id { get; set; }

        public int? ResearcherId { get; set; }

        public int? JournalId { get; set; }

        public int? ConferenceId { get; set; }

        public string? Title { get; set; }

        public int? Year { get; set; }

        public PublicationType? Type { get; set; }

        public string? Doi { get; set; }

        public string? Keywords { get; set; }
    }
}
