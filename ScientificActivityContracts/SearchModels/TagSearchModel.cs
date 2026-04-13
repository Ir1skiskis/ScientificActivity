using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScientificActivityContracts.SearchModels
{
    public class TagSearchModel
    {
        public int? Id { get; set; }

        public string? Name { get; set; }

        public string? NormalizedName { get; set; }

        public bool? OnlyActive { get; set; }

        public bool? OnlySelectable { get; set; }
    }
}
