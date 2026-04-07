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
    public interface IResearcherStorage
    {
        List<ResearcherViewModel> GetFullList();
        List<ResearcherViewModel> GetFilteredList(ResearcherSearchModel model);
        ResearcherViewModel? GetElement(ResearcherSearchModel model);
        ResearcherViewModel? Insert(ResearcherBindingModel model);
        ResearcherViewModel? Update(ResearcherBindingModel model);
        ResearcherViewModel? Delete(ResearcherBindingModel model);
    }
}
