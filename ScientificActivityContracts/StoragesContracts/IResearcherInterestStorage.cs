using ScientificActivityContracts.BindingModels;
using ScientificActivityContracts.SearchModels;
using ScientificActivityContracts.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScientificActivityContracts.StoragesContracts
{
    public interface IResearcherInterestStorage
    {
        List<ResearcherInterestViewModel> GetFullList();
        List<ResearcherInterestViewModel> GetFilteredList(ResearcherInterestSearchModel model);
        ResearcherInterestViewModel? GetElement(ResearcherInterestSearchModel model);
        ResearcherInterestViewModel? Insert(ResearcherInterestBindingModel model);
        ResearcherInterestViewModel? Update(ResearcherInterestBindingModel model);
        ResearcherInterestViewModel? Delete(ResearcherInterestBindingModel model);
    }
}
