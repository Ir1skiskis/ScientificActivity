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
    public interface IJournalVakSpecialtyLogic
    {
        List<JournalVakSpecialtyViewModel>? ReadList(JournalVakSpecialtySearchModel? model);
        JournalVakSpecialtyViewModel? ReadElement(JournalVakSpecialtySearchModel model);
        bool Create(JournalVakSpecialtyBindingModel model);
        bool DeleteByJournal(int journalId);
    }
}
