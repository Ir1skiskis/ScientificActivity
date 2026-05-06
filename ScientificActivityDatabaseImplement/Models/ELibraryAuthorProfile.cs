using ScientificActivityContracts.BindingModels;
using ScientificActivityContracts.ViewModels;
using ScientificActivityDataModels.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace ScientificActivityDatabaseImplement.Models
{
    public class ELibraryAuthorProfile : IELibraryAuthorProfileModel
    {
        public int Id { get; set; }

        [Required]
        public int ResearcherId { get; set; }

        [Required]
        public string AuthorId { get; set; } = string.Empty;

        [Required]
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

        public virtual Researcher Researcher { get; set; } = null!;

        public static ELibraryAuthorProfile? Create(ELibraryAuthorProfileBindingModel model)
        {
            if (model == null)
            {
                return null;
            }

            return new ELibraryAuthorProfile
            {
                Id = model.Id,
                ResearcherId = model.ResearcherId,
                AuthorId = model.AuthorId,
                FullName = model.FullName,
                Organization = model.Organization,
                Department = model.Department,
                SpinCode = model.SpinCode,

                PublicationsCountElibrary = model.PublicationsCountElibrary,
                PublicationsCountRinc = model.PublicationsCountRinc,
                PublicationsCoreRincCount = model.PublicationsCoreRincCount,

                CitationsCountElibrary = model.CitationsCountElibrary,
                CitationsCountRinc = model.CitationsCountRinc,
                CitationsCoreRincCount = model.CitationsCoreRincCount,

                HIndexElibrary = model.HIndexElibrary,
                HIndexRinc = model.HIndexRinc,
                HIndexCoreRinc = model.HIndexCoreRinc,
                HIndexWithoutSelfCitations = model.HIndexWithoutSelfCitations,

                PublicationsCitingAuthorCount = model.PublicationsCitingAuthorCount,
                MostCitedPublicationCitationsCount = model.MostCitedPublicationCitationsCount,
                CitedPublicationsCount = model.CitedPublicationsCount,
                AverageCitationsPerPublication = model.AverageCitationsPerPublication,

                FirstPublicationYear = model.FirstPublicationYear,
                SelfCitationsCount = model.SelfCitationsCount,
                CoauthorCitationsCount = model.CoauthorCitationsCount,
                CoauthorsCount = model.CoauthorsCount,

                ForeignArticlesCount = model.ForeignArticlesCount,
                RussianArticlesCount = model.RussianArticlesCount,
                VakArticlesCount = model.VakArticlesCount,
                ImpactFactorArticlesCount = model.ImpactFactorArticlesCount,

                ForeignJournalCitationsCount = model.ForeignJournalCitationsCount,
                RussianJournalCitationsCount = model.RussianJournalCitationsCount,
                VakJournalCitationsCount = model.VakJournalCitationsCount,
                ImpactFactorJournalCitationsCount = model.ImpactFactorJournalCitationsCount,

                AverageWeightedImpactFactorPublished = model.AverageWeightedImpactFactorPublished,
                AverageWeightedImpactFactorCited = model.AverageWeightedImpactFactorCited,

                PublicationsRincLast5YearsCount = model.PublicationsRincLast5YearsCount,
                PublicationsCoreRincLast5YearsCount = model.PublicationsCoreRincLast5YearsCount,
                CitationsRincLast5YearsCount = model.CitationsRincLast5YearsCount,
                CitationsCoreRincLast5YearsCount = model.CitationsCoreRincLast5YearsCount,
                CitationsAllLast5YearsCount = model.CitationsAllLast5YearsCount,

                MainRubricGrnti = model.MainRubricGrnti,
                MainRubricOecd = model.MainRubricOecd,
                PercentileCoreRinc = model.PercentileCoreRinc,

                PublicationsRincByYearJson = model.PublicationsRincByYearJson,
                PublicationsCoreRincByYearJson = model.PublicationsCoreRincByYearJson,
                CitationsRincByYearJson = model.CitationsRincByYearJson,
                CitationsCoreRincByYearJson = model.CitationsCoreRincByYearJson,
                HIndexRincByYearJson = model.HIndexRincByYearJson,
                HIndexCoreRincByYearJson = model.HIndexCoreRincByYearJson,
                PercentileCoreRincByYearJson = model.PercentileCoreRincByYearJson,
                PublicationsRinc5YearsByEndYearJson = model.PublicationsRinc5YearsByEndYearJson,
                PublicationsCoreRinc5YearsByEndYearJson = model.PublicationsCoreRinc5YearsByEndYearJson,
                CitationsRinc5YearsByEndYearJson = model.CitationsRinc5YearsByEndYearJson,
                CitationsCoreRinc5YearsByEndYearJson = model.CitationsCoreRinc5YearsByEndYearJson,

                ResearchTopics = model.ResearchTopics,
                ImportedAt = model.ImportedAt
            };
        }

        public void Update(ELibraryAuthorProfileBindingModel model)
        {
            if (model == null)
            {
                return;
            }

            AuthorId = model.AuthorId;
            FullName = model.FullName;
            Organization = model.Organization;
            Department = model.Department;
            SpinCode = model.SpinCode;

            PublicationsCountElibrary = model.PublicationsCountElibrary;
            PublicationsCountRinc = model.PublicationsCountRinc;
            PublicationsCoreRincCount = model.PublicationsCoreRincCount;

            CitationsCountElibrary = model.CitationsCountElibrary;
            CitationsCountRinc = model.CitationsCountRinc;
            CitationsCoreRincCount = model.CitationsCoreRincCount;

            HIndexElibrary = model.HIndexElibrary;
            HIndexRinc = model.HIndexRinc;
            HIndexCoreRinc = model.HIndexCoreRinc;
            HIndexWithoutSelfCitations = model.HIndexWithoutSelfCitations;

            PublicationsCitingAuthorCount = model.PublicationsCitingAuthorCount;
            MostCitedPublicationCitationsCount = model.MostCitedPublicationCitationsCount;
            CitedPublicationsCount = model.CitedPublicationsCount;
            AverageCitationsPerPublication = model.AverageCitationsPerPublication;

            FirstPublicationYear = model.FirstPublicationYear;
            SelfCitationsCount = model.SelfCitationsCount;
            CoauthorCitationsCount = model.CoauthorCitationsCount;
            CoauthorsCount = model.CoauthorsCount;

            ForeignArticlesCount = model.ForeignArticlesCount;
            RussianArticlesCount = model.RussianArticlesCount;
            VakArticlesCount = model.VakArticlesCount;
            ImpactFactorArticlesCount = model.ImpactFactorArticlesCount;

            ForeignJournalCitationsCount = model.ForeignJournalCitationsCount;
            RussianJournalCitationsCount = model.RussianJournalCitationsCount;
            VakJournalCitationsCount = model.VakJournalCitationsCount;
            ImpactFactorJournalCitationsCount = model.ImpactFactorJournalCitationsCount;

            AverageWeightedImpactFactorPublished = model.AverageWeightedImpactFactorPublished;
            AverageWeightedImpactFactorCited = model.AverageWeightedImpactFactorCited;

            PublicationsRincLast5YearsCount = model.PublicationsRincLast5YearsCount;
            PublicationsCoreRincLast5YearsCount = model.PublicationsCoreRincLast5YearsCount;
            CitationsRincLast5YearsCount = model.CitationsRincLast5YearsCount;
            CitationsCoreRincLast5YearsCount = model.CitationsCoreRincLast5YearsCount;
            CitationsAllLast5YearsCount = model.CitationsAllLast5YearsCount;

            MainRubricGrnti = model.MainRubricGrnti;
            MainRubricOecd = model.MainRubricOecd;
            PercentileCoreRinc = model.PercentileCoreRinc;

            PublicationsRincByYearJson = model.PublicationsRincByYearJson;
            PublicationsCoreRincByYearJson = model.PublicationsCoreRincByYearJson;
            CitationsRincByYearJson = model.CitationsRincByYearJson;
            CitationsCoreRincByYearJson = model.CitationsCoreRincByYearJson;
            HIndexRincByYearJson = model.HIndexRincByYearJson;
            HIndexCoreRincByYearJson = model.HIndexCoreRincByYearJson;
            PercentileCoreRincByYearJson = model.PercentileCoreRincByYearJson;
            PublicationsRinc5YearsByEndYearJson = model.PublicationsRinc5YearsByEndYearJson;
            PublicationsCoreRinc5YearsByEndYearJson = model.PublicationsCoreRinc5YearsByEndYearJson;
            CitationsRinc5YearsByEndYearJson = model.CitationsRinc5YearsByEndYearJson;
            CitationsCoreRinc5YearsByEndYearJson = model.CitationsCoreRinc5YearsByEndYearJson;

            ResearchTopics = model.ResearchTopics;
            ImportedAt = model.ImportedAt;
        }

        public ELibraryAuthorProfileViewModel GetViewModel => new()
        {
            AuthorId = AuthorId,
            FullName = FullName,
            Organization = Organization,
            Department = Department,
            SpinCode = SpinCode,

            PublicationsCountElibrary = PublicationsCountElibrary,
            PublicationsCountRinc = PublicationsCountRinc,
            PublicationsCoreRincCount = PublicationsCoreRincCount,

            CitationsCountElibrary = CitationsCountElibrary,
            CitationsCountRinc = CitationsCountRinc,
            CitationsCoreRincCount = CitationsCoreRincCount,

            HIndexElibrary = HIndexElibrary,
            HIndexRinc = HIndexRinc,
            HIndexCoreRinc = HIndexCoreRinc,
            HIndexWithoutSelfCitations = HIndexWithoutSelfCitations,

            PublicationsCitingAuthorCount = PublicationsCitingAuthorCount,
            MostCitedPublicationCitationsCount = MostCitedPublicationCitationsCount,
            CitedPublicationsCount = CitedPublicationsCount,
            AverageCitationsPerPublication = AverageCitationsPerPublication,

            FirstPublicationYear = FirstPublicationYear,
            SelfCitationsCount = SelfCitationsCount,
            CoauthorCitationsCount = CoauthorCitationsCount,
            CoauthorsCount = CoauthorsCount,

            ForeignArticlesCount = ForeignArticlesCount,
            RussianArticlesCount = RussianArticlesCount,
            VakArticlesCount = VakArticlesCount,
            ImpactFactorArticlesCount = ImpactFactorArticlesCount,

            ForeignJournalCitationsCount = ForeignJournalCitationsCount,
            RussianJournalCitationsCount = RussianJournalCitationsCount,
            VakJournalCitationsCount = VakJournalCitationsCount,
            ImpactFactorJournalCitationsCount = ImpactFactorJournalCitationsCount,

            AverageWeightedImpactFactorPublished = AverageWeightedImpactFactorPublished,
            AverageWeightedImpactFactorCited = AverageWeightedImpactFactorCited,

            PublicationsRincLast5YearsCount = PublicationsRincLast5YearsCount,
            PublicationsCoreRincLast5YearsCount = PublicationsCoreRincLast5YearsCount,
            CitationsRincLast5YearsCount = CitationsRincLast5YearsCount,
            CitationsCoreRincLast5YearsCount = CitationsCoreRincLast5YearsCount,
            CitationsAllLast5YearsCount = CitationsAllLast5YearsCount,

            MainRubricGrnti = MainRubricGrnti,
            MainRubricOecd = MainRubricOecd,
            PercentileCoreRinc = PercentileCoreRinc,

            PublicationsRincByYear = DeserializeDictionary(PublicationsRincByYearJson),
            PublicationsCoreRincByYear = DeserializeDictionary(PublicationsCoreRincByYearJson),
            CitationsRincByYear = DeserializeDictionary(CitationsRincByYearJson),
            CitationsCoreRincByYear = DeserializeDictionary(CitationsCoreRincByYearJson),
            HIndexRincByYear = DeserializeDictionary(HIndexRincByYearJson),
            HIndexCoreRincByYear = DeserializeDictionary(HIndexCoreRincByYearJson),
            PercentileCoreRincByYear = DeserializeDictionary(PercentileCoreRincByYearJson),
            PublicationsRinc5YearsByEndYear = DeserializeDictionary(PublicationsRinc5YearsByEndYearJson),
            PublicationsCoreRinc5YearsByEndYear = DeserializeDictionary(PublicationsCoreRinc5YearsByEndYearJson),
            CitationsRinc5YearsByEndYear = DeserializeDictionary(CitationsRinc5YearsByEndYearJson),
            CitationsCoreRinc5YearsByEndYear = DeserializeDictionary(CitationsCoreRinc5YearsByEndYearJson),

            ResearchTopics = ResearchTopics
        };

        private static Dictionary<int, int> DeserializeDictionary(string? json)
        {
            if (string.IsNullOrWhiteSpace(json))
            {
                return new Dictionary<int, int>();
            }

            return JsonSerializer.Deserialize<Dictionary<int, int>>(json) ?? new Dictionary<int, int>();
        }
    }
}
