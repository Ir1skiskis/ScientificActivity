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
    public interface IJournalLogic
    {
        List<JournalViewModel>? ReadList(JournalSearchModel? model);
        JournalViewModel? ReadElement(JournalSearchModel? model);
        bool Create(JournalBindingModel model);
        bool Update(JournalBindingModel model);
        bool Delete(JournalBindingModel model);
        JournalPagedListViewModel ReadPagedList(JournalSearchModel model);
    }
}
