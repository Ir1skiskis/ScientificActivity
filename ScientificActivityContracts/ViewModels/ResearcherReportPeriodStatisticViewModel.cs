using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScientificActivityContracts.ViewModels
{
    public class ResearcherReportPeriodStatisticViewModel
    {
        public int StartYear { get; set; }

        public int EndYear { get; set; }

        public int PublicationsCount { get; set; }

        public int RincPublicationsCount { get; set; }

        public int CoreRincPublicationsCount { get; set; }

        public int VakPublicationsCount { get; set; }

        public int WhiteListPublicationsCount { get; set; }

        public int CitationsRincCount { get; set; }

        public List<ResearcherReportYearStatisticViewModel> YearStatistics { get; set; } = new();

        public List<string> Keywords { get; set; } = new();

        public List<string> JournalTitles { get; set; } = new();
    }
}
