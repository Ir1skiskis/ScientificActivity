using ScientificActivityContracts.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScientificActivityContracts.BusinessLogicsContracts
{
    public interface IRecommendationLogic
    {
        RecommendationResultViewModel GetRecommendations(int researcherId);
        List<TagViewModel> GetResearcherTags(int researcherId);
        void SaveResearcherTags(int researcherId, List<int> tagIds);

        List<TagViewModel> AutoAssignResearcherTagsFromPublications(
            int researcherId,
            int maxTagsCount = 10,
            bool replaceExistingTags = false);
    }
}
