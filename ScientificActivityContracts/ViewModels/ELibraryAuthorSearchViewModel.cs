using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScientificActivityContracts.ViewModels
{
    public class ELibraryAuthorSearchViewModel
    {
        public string AuthorId { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string? Organization { get; set; }
        public string? SpinCode { get; set; }
    }
}
