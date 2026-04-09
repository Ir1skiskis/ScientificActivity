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
    public interface IJournalStorage
    {
        List<JournalViewModel> GetFullList();
        List<JournalViewModel> GetFilteredList(JournalSearchModel model);
        JournalViewModel? GetElement(JournalSearchModel model);
        JournalViewModel? Insert(JournalBindingModel model);
        JournalViewModel? Update(JournalBindingModel model);
        JournalViewModel? Delete(JournalBindingModel model);
        List<JournalViewModel> GetPagedList(JournalSearchModel model);
        int GetCount(JournalSearchModel model);
    }
}
