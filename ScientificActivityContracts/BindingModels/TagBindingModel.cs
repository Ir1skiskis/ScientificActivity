using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScientificActivityContracts.BindingModels
{
    public class TagBindingModel
    {
        public int Id { get; set; }

        public string Name { get; set; } = string.Empty;

        public string NormalizedName { get; set; } = string.Empty;

        public bool IsActive { get; set; } = true;

        public bool IsSelectable { get; set; } = true;
    }   
}
