using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScientificActivityContracts.BindingModels
{
    public class ResearcherReportSettingsBindingModel
    {
        public int ResearcherId { get; set; }

        public int StartYear { get; set; }

        public int EndYear { get; set; }

        public bool IncludeCommonInfo { get; set; } = true;
        public bool IncludePublicationSummary { get; set; } = true;
        public bool IncludeCitationIndexes { get; set; } = true;
        public bool IncludeAdditionalMetrics { get; set; } = true;
        public bool IncludeJournalMetrics { get; set; } = true;
        public bool IncludeLastFiveYearsMetrics { get; set; } = true;
        public bool IncludeYearDynamics { get; set; } = true;

        public bool IncludePublications { get; set; } = false;
        public bool IncludeOnlyLastFiveYearsPublications { get; set; } = false;
        public bool IncludeOnlyVakPublications { get; set; } = false;
        public bool IncludeOnlyPublicationsWithDoi { get; set; } = false;

        public DateTime? PublicationsDateFrom { get; set; }
        public DateTime? PublicationsDateTo { get; set; }

        public string ReportTitle { get; set; } = "Отчет о научной активности исследователя";
    }
}
