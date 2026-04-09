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
    public interface IJournalVakSpecialtyStorage
    {
        List<JournalVakSpecialtyViewModel> GetFullList();
        List<JournalVakSpecialtyViewModel> GetFilteredList(JournalVakSpecialtySearchModel model);
        JournalVakSpecialtyViewModel? GetElement(JournalVakSpecialtySearchModel model);
        JournalVakSpecialtyViewModel? Insert(JournalVakSpecialtyBindingModel model);
        int DeleteByJournal(int journalId);
    }
}
