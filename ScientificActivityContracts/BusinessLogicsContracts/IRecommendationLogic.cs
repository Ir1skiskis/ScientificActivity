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
    }
}
