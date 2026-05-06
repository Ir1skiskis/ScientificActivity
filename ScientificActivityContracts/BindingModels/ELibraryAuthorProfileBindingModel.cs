using ScientificActivityDataModels.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScientificActivityContracts.BindingModels
{
    public class ELibraryAuthorProfileBindingModel : IELibraryAuthorProfileModel
    {
        public int Id { get; set; }

        public int ResearcherId { get; set; }

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

        public string? PublicationsRincByYearJson { get; set; }
        public string? PublicationsCoreRincByYearJson { get; set; }
        public string? CitationsRincByYearJson { get; set; }
        public string? CitationsCoreRincByYearJson { get; set; }
        public string? HIndexRincByYearJson { get; set; }
        public string? HIndexCoreRincByYearJson { get; set; }
        public string? PercentileCoreRincByYearJson { get; set; }
        public string? PublicationsRinc5YearsByEndYearJson { get; set; }
        public string? PublicationsCoreRinc5YearsByEndYearJson { get; set; }
        public string? CitationsRinc5YearsByEndYearJson { get; set; }
        public string? CitationsCoreRinc5YearsByEndYearJson { get; set; }

        public string? ResearchTopics { get; set; }

        public DateTime ImportedAt { get; set; } = DateTime.UtcNow;
    }
}
