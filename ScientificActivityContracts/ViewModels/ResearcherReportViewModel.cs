using ScientificActivityContracts.BindingModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScientificActivityContracts.ViewModels
{
    public class ResearcherReportViewModel
    {
        public ResearcherReportSettingsBindingModel Settings { get; set; } = new();

        public ELibraryAuthorProfileViewModel? Profile { get; set; }

        public List<PublicationViewModel> Publications { get; set; } = new();

        public List<PublicationViewModel> PeriodPublications { get; set; } = new();

        public ResearcherReportPeriodStatisticViewModel PeriodStatistic { get; set; } = new();

        public Dictionary<int, int> PublicationsRincByYear { get; set; } = new();
        public Dictionary<int, int> PublicationsCoreRincByYear { get; set; } = new();
        public Dictionary<int, int> CitationsRincByYear { get; set; } = new();
        public Dictionary<int, int> CitationsCoreRincByYear { get; set; } = new();
        public Dictionary<int, int> HIndexRincByYear { get; set; } = new();
        public Dictionary<int, int> HIndexCoreRincByYear { get; set; } = new();
        public Dictionary<int, int> PercentileCoreRincByYear { get; set; } = new();

        public Dictionary<int, int> PublicationsRinc5YearsByEndYear { get; set; } = new();
        public Dictionary<int, int> PublicationsCoreRinc5YearsByEndYear { get; set; } = new();
        public Dictionary<int, int> CitationsRinc5YearsByEndYear { get; set; } = new();
        public Dictionary<int, int> CitationsCoreRinc5YearsByEndYear { get; set; } = new();

        public DateTime GeneratedAt { get; set; } = DateTime.Now;
    }
}
