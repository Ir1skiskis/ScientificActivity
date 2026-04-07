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
    public interface IPublicationStorage
    {
        List<PublicationViewModel> GetFullList();
        List<PublicationViewModel> GetFilteredList(PublicationSearchModel model);
        PublicationViewModel? GetElement(PublicationSearchModel model);
        PublicationViewModel? Insert(PublicationBindingModel model);
        PublicationViewModel? Update(PublicationBindingModel model);
        PublicationViewModel? Delete(PublicationBindingModel model);
    }
}
