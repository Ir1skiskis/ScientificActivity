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
    public interface ITagStorage
    {
        List<TagViewModel> GetFullList();
        List<TagViewModel> GetFilteredList(TagSearchModel model);
        TagViewModel? GetElement(TagSearchModel model);
        TagViewModel? Insert(TagBindingModel model);
        TagViewModel? Update(TagBindingModel model);
        TagViewModel? Delete(TagBindingModel model);
    }
}
