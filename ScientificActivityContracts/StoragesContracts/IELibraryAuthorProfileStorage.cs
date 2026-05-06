using ScientificActivityContracts.BindingModels;
using ScientificActivityContracts.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScientificActivityContracts.StoragesContracts
{
    public interface IELibraryAuthorProfileStorage
    {
        ELibraryAuthorProfileViewModel? GetByResearcherId(int researcherId);
        ELibraryAuthorProfileViewModel? GetByAuthorId(string authorId);
        ELibraryAuthorProfileViewModel? Insert(ELibraryAuthorProfileBindingModel model);
        ELibraryAuthorProfileViewModel? Update(ELibraryAuthorProfileBindingModel model);
        ELibraryAuthorProfileViewModel? InsertOrUpdate(ELibraryAuthorProfileBindingModel model);
    }
}
