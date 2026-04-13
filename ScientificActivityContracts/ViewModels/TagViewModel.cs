using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScientificActivityContracts.ViewModels
{
    public class TagViewModel
    {
        public int Id { get; set; }

        [DisplayName("Название")]
        public string Name { get; set; } = string.Empty;

        public string NormalizedName { get; set; } = string.Empty;

        public bool IsActive { get; set; }

        public bool IsSelectable { get; set; }
    }
}
