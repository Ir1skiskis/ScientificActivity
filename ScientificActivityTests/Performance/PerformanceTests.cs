using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using ScientificActivityBusinessLogics.BusinessLogics;
using ScientificActivityBusinessLogics.Helpers;
using ScientificActivityContracts.SearchModels;
using ScientificActivityContracts.ViewModels;
using ScientificActivityDatabaseImplement;
using ScientificActivityDatabaseImplement.Implements;
using ScientificActivityDatabaseImplement.Models;
using ScientificActivityDataModels.Enums;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScientificActivityTests.Performance
{
    public class PerformanceTests
    {
        private const int FastOperationLimitMilliseconds = 2000;
        private const int PaginationLimitMilliseconds = 1000;
        private const int RecommendationLimitMilliseconds = 3000;
        private const int ReportLimitMilliseconds = 3000;
        private const int ExportLimitMilliseconds = 5000;

        [Fact]
        public void JournalPageLoading_WhenDatabaseContainsManyJournals_ShouldCompleteWithinAllowedTime()
        {
            // Arrange
            var options = CreateOptions();
            SeedJournals(options, 30000);

            var storage = new JournalStorage(options);
            var logic = new JournalLogic(NullLogger<JournalLogic>.Instance, storage);

            var searchModel = new JournalSearchModel
            {
                Page = 1,
                PageSize = 25
            };

            // Act
            var stopwatch = Stopwatch.StartNew();

            var result = logic.ReadPagedList(searchModel);

            stopwatch.Stop();

            // Assert
            result.Should().NotBeNull();
            result.Journals.Should().HaveCount(25);
            result.TotalCount.Should().Be(30000);
            stopwatch.ElapsedMilliseconds.Should().BeLessThan(FastOperationLimitMilliseconds);
        }

        [Fact]
        public void JournalFilteringByTitle_WhenDatabaseContainsManyJournals_ShouldCompleteWithinAllowedTime()
        {
            // Arrange
            var options = CreateOptions();
            SeedJournals(options, 30000);

            var storage = new JournalStorage(options);
            var logic = new JournalLogic(NullLogger<JournalLogic>.Instance, storage);

            var searchModel = new JournalSearchModel
            {
                Title = "Информационные системы"
            };

            // Act
            var stopwatch = Stopwatch.StartNew();

            var result = logic.ReadList(searchModel);

            stopwatch.Stop();

            // Assert
            result.Should().NotBeNull();
            result.Should().NotBeEmpty();
            result.Should().OnlyContain(x => x.Title.Contains("Информационные системы"));
            stopwatch.ElapsedMilliseconds.Should().BeLessThan(FastOperationLimitMilliseconds);
        }

        [Fact]
        public void JournalFilteringByVak_WhenDatabaseContainsManyJournals_ShouldCompleteWithinAllowedTime()
        {
            // Arrange
            var options = CreateOptions();
            SeedJournals(options, 30000);

            var storage = new JournalStorage(options);
            var logic = new JournalLogic(NullLogger<JournalLogic>.Instance, storage);

            var searchModel = new JournalSearchModel
            {
                IsVak = true
            };

            // Act
            var stopwatch = Stopwatch.StartNew();

            var result = logic.ReadList(searchModel);

            stopwatch.Stop();

            // Assert
            result.Should().NotBeNull();
            result.Should().NotBeEmpty();
            result.Should().OnlyContain(x => x.IsVak);
            stopwatch.ElapsedMilliseconds.Should().BeLessThan(FastOperationLimitMilliseconds);
        }

        [Fact]
        public void JournalFilteringByWhiteList_WhenDatabaseContainsManyJournals_ShouldCompleteWithinAllowedTime()
        {
            // Arrange
            var options = CreateOptions();
            SeedJournals(options, 30000);

            var storage = new JournalStorage(options);
            var logic = new JournalLogic(NullLogger<JournalLogic>.Instance, storage);

            var searchModel = new JournalSearchModel
            {
                IsWhiteList = true
            };

            // Act
            var stopwatch = Stopwatch.StartNew();

            var result = logic.ReadList(searchModel);

            stopwatch.Stop();

            // Assert
            result.Should().NotBeNull();
            result.Should().NotBeEmpty();
            result.Should().OnlyContain(x => x.IsWhiteList);
            stopwatch.ElapsedMilliseconds.Should().BeLessThan(FastOperationLimitMilliseconds);
        }

        [Fact]
        public void JournalPagination_WhenDatabaseContainsManyJournals_ShouldCompleteWithinAllowedTime()
        {
            // Arrange
            var options = CreateOptions();
            SeedJournals(options, 30000);

            var storage = new JournalStorage(options);
            var logic = new JournalLogic(NullLogger<JournalLogic>.Instance, storage);

            var searchModel = new JournalSearchModel
            {
                Page = 10,
                PageSize = 25
            };

            // Act
            var stopwatch = Stopwatch.StartNew();

            var result = logic.ReadPagedList(searchModel);

            stopwatch.Stop();

            // Assert
            result.Should().NotBeNull();
            result.Journals.Should().HaveCount(25);
            result.CurrentPage.Should().Be(10);
            result.PageSize.Should().Be(25);
            stopwatch.ElapsedMilliseconds.Should().BeLessThan(PaginationLimitMilliseconds);
        }

        [Fact]
        public void ConferenceFiltering_WhenDatabaseContainsManyConferences_ShouldCompleteWithinAllowedTime()
        {
            // Arrange
            var options = CreateOptions();
            SeedConferences(options, 5000);

            var storage = new ConferenceStorage(options);
            var logic = new ConferenceLogic(NullLogger<ConferenceLogic>.Instance, storage);

            var searchModel = new ConferenceSearchModel
            {
                SubjectArea = "Информационные системы",
                DateFrom = DateTime.Today
            };

            // Act
            var stopwatch = Stopwatch.StartNew();

            var result = logic.ReadList(searchModel);

            stopwatch.Stop();

            // Assert
            result.Should().NotBeNull();
            result.Should().NotBeEmpty();
            result.Should().OnlyContain(x =>
                x.SubjectArea != null &&
                x.SubjectArea.Contains("Информационные системы") &&
                x.StartDate >= DateTime.Today);

            stopwatch.ElapsedMilliseconds.Should().BeLessThan(FastOperationLimitMilliseconds);
        }

        [Fact]
        public void GrantFilteringByStatus_WhenDatabaseContainsManyGrants_ShouldCompleteWithinAllowedTime()
        {
            // Arrange
            var options = CreateOptions();
            SeedGrants(options, 1000);

            var storage = new GrantStorage(options);
            var logic = new GrantLogic(NullLogger<GrantLogic>.Instance, storage);

            var searchModel = new GrantSearchModel
            {
                Status = GrantStatus.Открыт
            };

            // Act
            var stopwatch = Stopwatch.StartNew();

            var result = logic.ReadList(searchModel);

            stopwatch.Stop();

            // Assert
            result.Should().NotBeNull();
            result.Should().NotBeEmpty();
            result.Should().OnlyContain(x => x.Status == GrantStatus.Открыт);
            stopwatch.ElapsedMilliseconds.Should().BeLessThan(FastOperationLimitMilliseconds);
        }

        [Fact]
        public void RecommendationBuilding_WhenDatabaseContainsManyResources_ShouldCompleteWithinAllowedTime()
        {
            // Arrange
            var options = CreateOptions();
            SeedRecommendationData(options);

            using var context = new ScientificActivityDatabase(options);
            var logic = new RecommendationLogic(NullLogger<RecommendationLogic>.Instance, context);

            // Act
            var stopwatch = Stopwatch.StartNew();

            var result = logic.GetRecommendations(1);

            stopwatch.Stop();

            // Assert
            result.Should().NotBeNull();

            result.ResearcherTags.Should().NotBeEmpty();
            result.Journals.Should().NotBeEmpty();
            result.Conferences.Should().NotBeEmpty();
            result.Grants.Should().NotBeEmpty();

            stopwatch.ElapsedMilliseconds.Should().BeLessThan(RecommendationLimitMilliseconds);
        }

        [Fact]
        public void ReportTextBuilding_WhenReportContainsManyPublications_ShouldCompleteWithinAllowedTime()
        {
            // Arrange
            var builder = new ResearcherReportBuilder();

            var researcher = CreateResearcherViewModel();
            var publications = CreatePublicationViewModels(500);
            var eLibraryProfile = CreateELibraryProfileViewModel();

            var options = new ResearcherReportOptions
            {
                IncludeProfile = true,
                IncludePublications = true,
                IncludeELibraryProfile = true
            };

            // Act
            var stopwatch = Stopwatch.StartNew();

            var result = builder.BuildReportText(
                researcher,
                publications,
                eLibraryProfile,
                options);

            stopwatch.Stop();

            // Assert
            result.Should().NotBeNullOrWhiteSpace();
            result.Should().Contain("ОТЧЕТ О НАУЧНОЙ АКТИВНОСТИ");
            result.Should().Contain("ПРОФИЛЬ ИССЛЕДОВАТЕЛЯ");
            result.Should().Contain("ПУБЛИКАЦИИ");
            result.Should().Contain("ПОКАЗАТЕЛИ ELIBRARY");
            result.Should().Contain("Тестовая публикация 500");

            stopwatch.ElapsedMilliseconds.Should().BeLessThan(ReportLimitMilliseconds);
        }

        [Fact]
        public void PdfGeneration_WhenReportTextIsReady_ShouldCompleteWithinAllowedTime()
        {
            // Arrange
            var builder = new ResearcherReportBuilder();

            var reportText = builder.BuildReportText(
                CreateResearcherViewModel(),
                CreatePublicationViewModels(500),
                CreateELibraryProfileViewModel(),
                new ResearcherReportOptions
                {
                    IncludeProfile = true,
                    IncludePublications = true,
                    IncludeELibraryProfile = true
                });

            // Act
            var stopwatch = Stopwatch.StartNew();

            var result = builder.GeneratePdfBytes(reportText);

            stopwatch.Stop();

            // Assert
            result.Should().NotBeNull();
            result.Should().NotBeEmpty();
            stopwatch.ElapsedMilliseconds.Should().BeLessThan(ExportLimitMilliseconds);
        }

        [Fact]
        public void DocxGeneration_WhenReportTextIsReady_ShouldCompleteWithinAllowedTime()
        {
            // Arrange
            var builder = new ResearcherReportBuilder();

            var reportText = builder.BuildReportText(
                CreateResearcherViewModel(),
                CreatePublicationViewModels(500),
                CreateELibraryProfileViewModel(),
                new ResearcherReportOptions
                {
                    IncludeProfile = true,
                    IncludePublications = true,
                    IncludeELibraryProfile = true
                });

            // Act
            var stopwatch = Stopwatch.StartNew();

            var result = builder.GenerateDocxBytes(reportText);

            stopwatch.Stop();

            // Assert
            result.Should().NotBeNull();
            result.Should().NotBeEmpty();
            stopwatch.ElapsedMilliseconds.Should().BeLessThan(ExportLimitMilliseconds);
        }

        [Fact]
        public void RepeatedJournalFiltering_WhenTwentyRequestsAreExecuted_ShouldCompleteWithoutErrors()
        {
            // Arrange
            var options = CreateOptions();
            SeedJournals(options, 30000);

            var storage = new JournalStorage(options);
            var logic = new JournalLogic(NullLogger<JournalLogic>.Instance, storage);

            // Act
            var action = () =>
            {
                for (var i = 0; i < 20; i++)
                {
                    var result = logic.ReadList(new JournalSearchModel
                    {
                        IsVak = i % 2 == 0,
                        IsWhiteList = i % 3 == 0
                    });

                    result.Should().NotBeNull();
                }
            };

            // Assert
            action.Should().NotThrow();
        }

        private static DbContextOptions<ScientificActivityDatabase> CreateOptions()
        {
            return new DbContextOptionsBuilder<ScientificActivityDatabase>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;
        }

        private static void SeedJournals(DbContextOptions<ScientificActivityDatabase> options, int count)
        {
            using var context = new ScientificActivityDatabase(options);

            var journals = new List<Journal>();

            for (var i = 1; i <= count; i++)
            {
                var title = i % 5 == 0
                    ? $"Информационные системы и технологии {i}"
                    : $"Научный журнал {i}";

                journals.Add(new Journal
                {
                    Id = i,
                    Title = title,
                    Issn = $"{i % 10000:0000}-{(i * 3) % 10000:0000}",
                    EIssn = null,
                    Publisher = "Тестовый издатель",
                    SubjectArea = i % 5 == 0
                        ? "Информационные системы; Машинное обучение"
                        : "Общие научные направления",
                    IsVak = i % 2 == 0,
                    IsWhiteList = i % 3 == 0,
                    WhiteListLevel2023 = null,
                    WhiteListLevel2025 = i % 3 == 0 ? (i % 4) + 1 : null,
                    WhiteListState = i % 3 == 0 ? "active" : null,
                    WhiteListNotice = null,
                    WhiteListAcceptedDate = i % 3 == 0 ? new DateTime(2025, 1, 1) : null,
                    WhiteListDiscontinuedDate = null,
                    Country = "Россия",
                    Url = $"https://example.com/journal-{i}",
                    RcsiRecordSourceId = i
                });
            }

            context.Journals.AddRange(journals);
            context.SaveChanges();
        }

        private static void SeedConferences(DbContextOptions<ScientificActivityDatabase> options, int count)
        {
            using var context = new ScientificActivityDatabase(options);

            var conferences = new List<Conference>();

            for (var i = 1; i <= count; i++)
            {
                conferences.Add(new Conference
                {
                    Id = i,
                    Title = $"Научная конференция {i}",
                    Description = "Тестовое описание конференции",
                    StartDate = i % 2 == 0
                        ? DateTime.Today.AddDays(i % 365)
                        : DateTime.Today.AddDays(-i % 365),
                    EndDate = i % 2 == 0
                        ? DateTime.Today.AddDays((i % 365) + 2)
                        : DateTime.Today.AddDays((-i % 365) + 2),
                    City = "Ульяновск",
                    Country = "Россия",
                    Organizer = "Тестовый организатор",
                    SubjectArea = i % 4 == 0
                        ? "Информационные системы; Искусственный интеллект"
                        : "Общие научные направления",
                    Format = i % 3 == 0
                        ? ConferenceFormat.Онлайн
                        : i % 3 == 1
                            ? ConferenceFormat.Очная
                            : ConferenceFormat.Смешанная,
                    Level = i % 2 == 0
                        ? ConferenceLevel.Международная
                        : ConferenceLevel.Всероссийская,
                    Url = $"https://example.com/conference-{i}"
                });
            }

            context.Conferences.AddRange(conferences);
            context.SaveChanges();
        }

        private static void SeedGrants(DbContextOptions<ScientificActivityDatabase> options, int count)
        {
            using var context = new ScientificActivityDatabase(options);

            var grants = new List<Grant>();

            for (var i = 1; i <= count; i++)
            {
                grants.Add(new Grant
                {
                    Id = i,
                    ContestNumber = $"РНФ-2026-{i:0000}",
                    Title = $"Грантовый конкурс {i}",
                    Description = "Тестовое описание грантового конкурса",
                    Organization = "Российский научный фонд",
                    StartDate = DateTime.Today.AddDays(-10),
                    EndDate = DateTime.Today.AddDays(i % 120),
                    Amount = 3000000 + i,
                    Currency = "руб.",
                    SubjectArea = i % 4 == 0
                        ? "Информационные системы; Машинное обучение"
                        : "Общие научные направления",
                    Status = i % 2 == 0
                        ? GrantStatus.Открыт
                        : GrantStatus.Закрыт,
                    Url = $"https://rscf.ru/contests/РНФ-2026-{i:0000}"
                });
            }

            context.Grants.AddRange(grants);
            context.SaveChanges();
        }

        private static void SeedRecommendationData(DbContextOptions<ScientificActivityDatabase> options)
        {
            using var context = new ScientificActivityDatabase(options);

            context.Researchers.Add(new Researcher
            {
                Id = 1,
                Email = "researcher@example.com",
                PasswordHash = "password-hash",
                Role = UserRole.Исследователь,
                IsActive = true,
                LastName = "Гуськов",
                FirstName = "Глеб",
                MiddleName = "Юрьевич",
                Phone = "79001234567",
                Department = "Информационные системы",
                Position = "Доцент",
                AcademicDegree = AcademicDegree.Кандидат_наук,
                ELibraryAuthorId = "812005",
                ResearchTopics = "информационные системы; машинное обучение"
            });

            var tags = new List<Tag>
            {
                new Tag { Id = 1, Name = "Информационные системы", NormalizedName = "информационные системы", IsActive = true, IsSelectable = true },
                new Tag { Id = 2, Name = "Машинное обучение", NormalizedName = "машинное обучение", IsActive = true, IsSelectable = true },
                new Tag { Id = 3, Name = "Химия", NormalizedName = "химия", IsActive = true, IsSelectable = true }
            };

            context.Tags.AddRange(tags);

            context.ResearcherTags.AddRange(
                new ResearcherTag { ResearcherId = 1, TagId = 1 },
                new ResearcherTag { ResearcherId = 1, TagId = 2 });

            var journals = new List<Journal>();
            var conferences = new List<Conference>();
            var grants = new List<Grant>();

            for (var i = 1; i <= 1000; i++)
            {
                journals.Add(new Journal
                {
                    Id = i,
                    Title = $"Рекомендованный журнал {i}",
                    Issn = $"{i % 10000:0000}-{(i * 7) % 10000:0000}",
                    Publisher = "Тестовый издатель",
                    SubjectArea = "Информационные системы",
                    IsVak = true,
                    IsWhiteList = true,
                    WhiteListLevel2025 = 2,
                    Country = "Россия",
                    Url = $"https://example.com/recommended-journal-{i}",
                    RcsiRecordSourceId = i
                });

                conferences.Add(new Conference
                {
                    Id = i,
                    Title = $"Рекомендованная конференция {i}",
                    Description = "Тестовое описание конференции",
                    StartDate = DateTime.Today.AddDays(10 + i % 60),
                    EndDate = DateTime.Today.AddDays(12 + i % 60),
                    City = "Ульяновск",
                    Country = "Россия",
                    Organizer = "Тестовый организатор",
                    SubjectArea = "Машинное обучение",
                    Format = ConferenceFormat.Смешанная,
                    Level = ConferenceLevel.Международная,
                    Url = $"https://example.com/recommended-conference-{i}"
                });

                grants.Add(new Grant
                {
                    Id = i,
                    ContestNumber = $"РНФ-REC-{i:0000}",
                    Title = $"Рекомендованный грант {i}",
                    Description = "Тестовое описание гранта",
                    Organization = "Российский научный фонд",
                    StartDate = DateTime.Today.AddDays(-5),
                    EndDate = DateTime.Today.AddDays(30 + i % 60),
                    Amount = 3000000,
                    Currency = "руб.",
                    SubjectArea = "Информационные системы",
                    Status = GrantStatus.Открыт,
                    Url = $"https://rscf.ru/contests/РНФ-REC-{i:0000}"
                });
            }

            context.Journals.AddRange(journals);
            context.Conferences.AddRange(conferences);
            context.Grants.AddRange(grants);
            context.SaveChanges();

            var journalTags = new List<JournalTag>();
            var conferenceTags = new List<ConferenceTag>();
            var grantTags = new List<GrantTag>();

            for (var i = 1; i <= 1000; i++)
            {
                journalTags.Add(new JournalTag
                {
                    JournalId = i,
                    TagId = 1
                });

                conferenceTags.Add(new ConferenceTag
                {
                    ConferenceId = i,
                    TagId = 2
                });

                grantTags.Add(new GrantTag
                {
                    GrantId = i,
                    TagId = 1
                });
            }

            context.JournalTags.AddRange(journalTags);
            context.ConferenceTags.AddRange(conferenceTags);
            context.GrantTags.AddRange(grantTags);

            context.SaveChanges();
        }

        private static ResearcherViewModel CreateResearcherViewModel()
        {
            return new ResearcherViewModel
            {
                Id = 1,
                Email = "researcher@example.com",
                PasswordHash = "password-hash",
                Role = UserRole.Исследователь,
                IsActive = true,
                LastName = "Гуськов",
                FirstName = "Глеб",
                MiddleName = "Юрьевич",
                Phone = "79001234567",
                Department = "Информационные системы",
                Position = "Доцент",
                AcademicDegree = AcademicDegree.Кандидат_наук,
                ELibraryAuthorId = "812005",
                ResearchTopics = "информационные системы; машинное обучение",
                PublicationsCount = 500
            };
        }

        private static List<PublicationViewModel> CreatePublicationViewModels(int count)
        {
            var publications = new List<PublicationViewModel>();

            for (var i = 1; i <= count; i++)
            {
                publications.Add(new PublicationViewModel
                {
                    Id = i,
                    Title = $"Тестовая публикация {i}",
                    Authors = "Гуськов Г. Ю., Табеев А. П.",
                    Year = 2026 - i % 10,
                    Type = PublicationType.Статья_в_журнале,
                    Doi = $"10.1000/test-{i}",
                    Url = $"https://example.com/publication-{i}",
                    ResearcherId = 1,
                    Keywords = "информационные системы; машинное обучение",
                    Annotation = "Тестовая аннотация публикации.",
                    CitationsRincCount = i % 50,
                    ELibraryId = $"ELIB-{i}",
                    IsInRinc = true,
                    IsInCoreRinc = i % 3 == 0,
                    IsVak = i % 2 == 0
                });
            }

            return publications;
        }

        private static ELibraryAuthorProfileViewModel CreateELibraryProfileViewModel()
        {
            return new ELibraryAuthorProfileViewModel
            {
                AuthorId = "812005",
                FullName = "Гуськов Глеб Юрьевич",
                Organization = "Ульяновский государственный технический университет",
                Department = "Информационные системы",
                SpinCode = "2896-1945",
                PublicationsCountElibrary = 100,
                PublicationsCountRinc = 80,
                PublicationsCoreRincCount = 30,
                CitationsCountElibrary = 500,
                CitationsCountRinc = 450,
                CitationsCoreRincCount = 120,
                HIndexElibrary = 12,
                HIndexRinc = 10,
                HIndexCoreRinc = 6,
                HIndexWithoutSelfCitations = 9,
                PublicationsCitingAuthorCount = 40,
                MostCitedPublicationCitationsCount = 70,
                CitedPublicationsCount = 35,
                AverageCitationsPerPublication = (decimal?)5.5,
                FirstPublicationYear = 2012,
                SelfCitationsCount = 20,
                CoauthorCitationsCount = 100,
                CoauthorsCount = 15,
                ForeignArticlesCount = 3,
                RussianArticlesCount = 77,
                VakArticlesCount = 25,
                ImpactFactorArticlesCount = 10,
                ForeignJournalCitationsCount = 30,
                RussianJournalCitationsCount = 420,
                VakJournalCitationsCount = 200,
                ImpactFactorJournalCitationsCount = 80,
                AverageWeightedImpactFactorPublished = (decimal?)1.2,
                AverageWeightedImpactFactorCited = (decimal?)1.8,
                PublicationsRincLast5YearsCount = 31,
                PublicationsCoreRincLast5YearsCount = 12,
                CitationsRincLast5YearsCount = 210,
                CitationsCoreRincLast5YearsCount = 90,
                CitationsAllLast5YearsCount = 240,
                MainRubricGrnti = "Информатика",
                MainRubricOecd = "Computer and information sciences",
                PercentileCoreRinc = 25,
                ResearchTopics = "информационные системы; машинное обучение"
            };
        }
    }
}
