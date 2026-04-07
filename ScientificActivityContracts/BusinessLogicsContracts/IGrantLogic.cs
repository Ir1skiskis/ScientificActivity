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
    public interface IGrantLogic
    {
        List<GrantViewModel>? ReadList(GrantSearchModel? model);
        GrantViewModel? ReadElement(GrantSearchModel? model);
        bool Create(GrantBindingModel model);
        bool Update(GrantBindingModel model);
        bool Delete(GrantBindingModel model);
    }
}
