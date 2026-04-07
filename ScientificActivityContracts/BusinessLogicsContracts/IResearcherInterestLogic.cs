using ScientificActivityContracts.BindingModels;
using ScientificActivityContracts.SearchModels;
using ScientificActivityContracts.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScientificActivityContracts.BusinessLogicsContracts
{
    public interface IResearcherInterestLogic
    {
        List<ResearcherInterestViewModel>? ReadList(ResearcherInterestSearchModel? model);
        ResearcherInterestViewModel? ReadElement(ResearcherInterestSearchModel? model);
        bool Create(ResearcherInterestBindingModel model);
        bool Update(ResearcherInterestBindingModel model);
        bool Delete(ResearcherInterestBindingModel model);
    }
}
