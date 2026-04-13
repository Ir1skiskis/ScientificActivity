using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScientificActivityContracts.ViewModels
{
    public class RecommendationResultViewModel
    {
        public List<string> ResearcherTags { get; set; } = new();

        public List<RecommendationItemViewModel> Grants { get; set; } = new();

        public List<RecommendationItemViewModel> Conferences { get; set; } = new();

        public List<RecommendationItemViewModel> Journals { get; set; } = new();
    }
}
