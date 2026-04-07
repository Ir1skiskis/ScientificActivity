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
    public interface IConferenceStorage
    {
        List<ConferenceViewModel> GetFullList();
        List<ConferenceViewModel> GetFilteredList(ConferenceSearchModel model);
        ConferenceViewModel? GetElement(ConferenceSearchModel model);
        ConferenceViewModel? Insert(ConferenceBindingModel model);
        ConferenceViewModel? Update(ConferenceBindingModel model);
        ConferenceViewModel? Delete(ConferenceBindingModel model);
    }
}
