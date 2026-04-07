using ScientificActivityDataModels.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScientificActivityContracts.BindingModels
{
    public class ResearcherInterestBindingModel : IResearcherInterestModel
    {
        public int Id { get; set; }

        public int ResearcherId { get; set; }

        public string Keyword { get; set; } = string.Empty;

        public decimal Weight { get; set; }
    }
}
