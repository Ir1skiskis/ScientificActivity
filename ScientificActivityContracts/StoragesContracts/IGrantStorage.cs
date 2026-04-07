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
    public interface IGrantStorage
    {
        List<GrantViewModel> GetFullList();
        List<GrantViewModel> GetFilteredList(GrantSearchModel model);
        GrantViewModel? GetElement(GrantSearchModel model);
        GrantViewModel? Insert(GrantBindingModel model);
        GrantViewModel? Update(GrantBindingModel model);
        GrantViewModel? Delete(GrantBindingModel model);
    }
}
