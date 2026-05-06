using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScientificActivityDataModels.Models
{
    public interface IELibraryAuthorProfileModel : IId
    {
        int ResearcherId { get; }

        string AuthorId { get; }
        string FullName { get; }
        string? Organization { get; }
        string? Department { get; }
        string? SpinCode { get; }

        int? PublicationsCountElibrary { get; }
        int? PublicationsCountRinc { get; }
        int? PublicationsCoreRincCount { get; }

        int? CitationsCountElibrary { get; }
        int? CitationsCountRinc { get; }
        int? CitationsCoreRincCount { get; }

        int? HIndexElibrary { get; }
        int? HIndexRinc { get; }
        int? HIndexCoreRinc { get; }
        int? HIndexWithoutSelfCitations { get; }

        int? PublicationsCitingAuthorCount { get; }
        int? MostCitedPublicationCitationsCount { get; }
        int? CitedPublicationsCount { get; }
        decimal? AverageCitationsPerPublication { get; }

        int? FirstPublicationYear { get; }
        int? SelfCitationsCount { get; }
        int? CoauthorCitationsCount { get; }
        int? CoauthorsCount { get; }

        int? ForeignArticlesCount { get; }
        int? RussianArticlesCount { get; }
        int? VakArticlesCount { get; }
        int? ImpactFactorArticlesCount { get; }

        int? ForeignJournalCitationsCount { get; }
        int? RussianJournalCitationsCount { get; }
        int? VakJournalCitationsCount { get; }
        int? ImpactFactorJournalCitationsCount { get; }

        decimal? AverageWeightedImpactFactorPublished { get; }
        decimal? AverageWeightedImpactFactorCited { get; }

        int? PublicationsRincLast5YearsCount { get; }
        int? PublicationsCoreRincLast5YearsCount { get; }
        int? CitationsRincLast5YearsCount { get; }
        int? CitationsCoreRincLast5YearsCount { get; }
        int? CitationsAllLast5YearsCount { get; }

        string? MainRubricGrnti { get; }
        string? MainRubricOecd { get; }
        int? PercentileCoreRinc { get; }

        string? PublicationsRincByYearJson { get; }
        string? PublicationsCoreRincByYearJson { get; }
        string? CitationsRincByYearJson { get; }
        string? CitationsCoreRincByYearJson { get; }
        string? HIndexRincByYearJson { get; }
        string? HIndexCoreRincByYearJson { get; }
        string? PercentileCoreRincByYearJson { get; }
        string? PublicationsRinc5YearsByEndYearJson { get; }
        string? PublicationsCoreRinc5YearsByEndYearJson { get; }
        string? CitationsRinc5YearsByEndYearJson { get; }
        string? CitationsCoreRinc5YearsByEndYearJson { get; }

        string? ResearchTopics { get; }

        DateTime ImportedAt { get; }
    }
}
