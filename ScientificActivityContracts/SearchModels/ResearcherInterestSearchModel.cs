using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScientificActivityContracts.SearchModels
{
    public class ResearcherInterestSearchModel
    {
        public int? Id { get; set; }

        public int? ResearcherId { get; set; }

        public string? Keyword { get; set; }
    }
}
