using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScientificActivityContracts.BindingModels
{
    public class ELibraryBindAuthorBindingModel
    {
        public int ResearcherId { get; set; }
        public string AuthorId { get; set; } = string.Empty;
    }
}
