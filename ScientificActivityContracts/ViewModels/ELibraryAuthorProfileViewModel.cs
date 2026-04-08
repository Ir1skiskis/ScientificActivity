using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScientificActivityContracts.ViewModels
{
    public class ELibraryAuthorProfileViewModel
    {
        public string AuthorId { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string? Organization { get; set; }
        public string? Department { get; set; }
        public string? SpinCode { get; set; }

        public int? PublicationsCountElibrary { get; set; }
        public int? PublicationsCountRinc { get; set; }
        public int? PublicationsCoreRincCount { get; set; }

        public int? CitationsCountElibrary { get; set; }
        public int? CitationsCountRinc { get; set; }
        public int? CitationsCoreRincCount { get; set; }

        public int? HIndexElibrary { get; set; }
        public int? HIndexRinc { get; set; }
        public int? HIndexCoreRinc { get; set; }
        public int? HIndexWithoutSelfCitations { get; set; }

        public int? PublicationsCitingAuthorCount { get; set; }
        public int? MostCitedPublicationCitationsCount { get; set; }
        public int? CitedPublicationsCount { get; set; }
        public decimal? AverageCitationsPerPublication { get; set; }

        public int? FirstPublicationYear { get; set; }
        public int? SelfCitationsCount { get; set; }
        public int? CoauthorCitationsCount { get; set; }
        public int? CoauthorsCount { get; set; }

        public int? ForeignArticlesCount { get; set; }
        public int? RussianArticlesCount { get; set; }
        public int? VakArticlesCount { get; set; }
        public int? ImpactFactorArticlesCount { get; set; }

        public int? ForeignJournalCitationsCount { get; set; }
        public int? RussianJournalCitationsCount { get; set; }
        public int? VakJournalCitationsCount { get; set; }
        public int? ImpactFactorJournalCitationsCount { get; set; }

        public decimal? AverageWeightedImpactFactorPublished { get; set; }
        public decimal? AverageWeightedImpactFactorCited { get; set; }

        public int? PublicationsRincLast5YearsCount { get; set; }
        public int? PublicationsCoreRincLast5YearsCount { get; set; }
        public int? CitationsRincLast5YearsCount { get; set; }
        public int? CitationsCoreRincLast5YearsCount { get; set; }
        public int? CitationsAllLast5YearsCount { get; set; }

        public string? MainRubricGrnti { get; set; }
        public string? MainRubricOecd { get; set; }
        public int? PercentileCoreRinc { get; set; }

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

        public string? ResearchTopics { get; set; }
    }
}
