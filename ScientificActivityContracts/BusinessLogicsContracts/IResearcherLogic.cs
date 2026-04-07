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
    public interface IResearcherLogic
    {
        List<ResearcherViewModel>? ReadList(ResearcherSearchModel? model);
        ResearcherViewModel? ReadElement(ResearcherSearchModel? model);
        bool Create(ResearcherBindingModel model);
        bool Update(ResearcherBindingModel model);
        bool Delete(ResearcherBindingModel model);
    }
}
