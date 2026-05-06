using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScientificActivityParsers.Models
{
    public class ELibraryPublicationCategoryInfoModel
    {
        public Dictionary<string, HashSet<string>> PublicationIdsByCategory { get; set; } = new();

        public Dictionary<string, int> CategoryCounts { get; set; } = new();
    }
}
