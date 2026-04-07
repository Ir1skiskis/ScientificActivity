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
    public interface IPublicationLogic
    {
        List<PublicationViewModel>? ReadList(PublicationSearchModel? model);
        PublicationViewModel? ReadElement(PublicationSearchModel? model);
        bool Create(PublicationBindingModel model);
        bool Update(PublicationBindingModel model);
        bool Delete(PublicationBindingModel model);
    }
}
