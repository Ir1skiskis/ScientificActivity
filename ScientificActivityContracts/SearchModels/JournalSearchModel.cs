using ScientificActivityDataModels.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScientificActivityContracts.SearchModels
{
    public class JournalSearchModel
    {
        public int? Id { get; set; }

        public string? Title { get; set; }

        public string? Issn { get; set; }

        public string? SubjectArea { get; set; }

        public JournalQuartile? Quartile { get; set; }

        public bool? IsVak { get; set; }

        public bool? IsWhiteList { get; set; }
    }
}
