using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScientificActivityContracts.BindingModels
{
    public class ResearcherTagBindingModel
    {
        public int ResearcherId { get; set; }

        public List<int> TagIds { get; set; } = new();
    }
}
