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
        List<TagViewModel> GetSelectableTags();
        List<TagViewModel> GetConferenceTags();
        List<TagViewModel> GetGrantTags();
        List<TagViewModel> GetJournalTags();
    }
}
