using ScientificActivityDataModels.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScientificActivityContracts.ViewModels
{
    public class ResearcherInterestViewModel : IResearcherInterestModel
    {
        public int Id { get; set; }

        public int ResearcherId { get; set; }

        [DisplayName("Ключевое слово")]
        public string Keyword { get; set; } = string.Empty;

        [DisplayName("Вес")]
        public decimal Weight { get; set; }

        [DisplayName("Исследователь")]
        public string ResearcherFullName { get; set; } = string.Empty;
    }
}
