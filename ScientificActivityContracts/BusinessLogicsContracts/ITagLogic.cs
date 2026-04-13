using ScientificActivityContracts.BindingModels;
using ScientificActivityContracts.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScientificActivityContracts.BusinessLogicsContracts
{
    public interface ITagLogic
    {
        List<TagViewModel> ReadList(bool onlySelectable = true);
        void SaveResearcherTags(ResearcherTagBindingModel model);
        List<TagViewModel> GetResearcherTags(int researcherId);
    }
}
