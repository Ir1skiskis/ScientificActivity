using ScientificActivityContracts.BindingModels;
using ScientificActivityContracts.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScientificActivityContracts.BusinessLogicsContracts
{
    public interface IELibraryLogic
    {
        List<ELibraryAuthorSearchViewModel> SearchAuthors(ELibraryAuthorSearchBindingModel model);
        ELibraryAuthorProfileViewModel? GetAuthorProfile(string authorId);
        bool BindAuthorToResearcher(ELibraryBindAuthorBindingModel model);
        bool ImportAuthorProfile(ELibraryImportBindingModel model);
        //int ImportAuthorPublications(ELibraryImportBindingModel model);
    }
}
