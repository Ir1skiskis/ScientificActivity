using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScientificActivityContracts.ViewModels
{
    public class ResearcherReportYearStatisticViewModel
    {
        public int Year { get; set; }

        public int PublicationsCount { get; set; }

        public int RincPublicationsCount { get; set; }

        public int CoreRincPublicationsCount { get; set; }

        public int VakPublicationsCount { get; set; }

        public int WhiteListPublicationsCount { get; set; }

        public int CitationsRincCount { get; set; }
    }
}
