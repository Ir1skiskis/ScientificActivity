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
    public interface IConferenceLogic
    {
        List<ConferenceViewModel>? ReadList(ConferenceSearchModel? model);
        ConferenceViewModel? ReadElement(ConferenceSearchModel? model);
        bool Create(ConferenceBindingModel model);
        bool Update(ConferenceBindingModel model);
        bool Delete(ConferenceBindingModel model);
    }
}
