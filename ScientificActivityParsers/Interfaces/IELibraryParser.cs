using ScientificActivityContracts.BindingModels;
using ScientificActivityContracts.ViewModels;
using ScientificActivityParsers.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScientificActivityParsers.Interfaces
{
    public interface IELibraryParser
    {
        List<ELibraryAuthorSearchViewModel> SearchAuthors(ELibraryAuthorSearchBindingModel model);
        ELibraryAuthorProfileViewModel? GetAuthorProfile(string authorId);
        List<ELibraryPublicationImportModel> GetAuthorPublications(string authorId);
    }
}
