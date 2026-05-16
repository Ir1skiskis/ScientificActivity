using FluentAssertions;
using ScientificActivityBusinessLogics.Helpers;
using ScientificActivityContracts.ViewModels;
using ScientificActivityDataModels.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScientificActivityTests.Unit
{
    public class ResearcherReportBuilderTests
    {
        private readonly ResearcherReportBuilder _builder;

        public ResearcherReportBuilderTests()
        {
            _builder = new ResearcherReportBuilder();
        }

        [Fact]
        public void BuildReportText_WhenProfileSectionSelected_ShouldContainResearcherData()
        {
            // Arrange
            var researcher = CreateResearcher();
            var publications = CreatePublications();
            var eLibraryProfile = CreateELibraryProfile();

            var options = new ResearcherReportOptions
            {
                IncludeProfile = true,
                IncludePublications = false,
                IncludeELibraryProfile = false
            };

            // Act
            var result = _builder.BuildReportText(researcher, publications, eLibraryProfile, options);

            // Assert
            result.Should().Contain("ПРОФИЛЬ ИССЛЕДОВАТЕЛЯ");
            result.Should().Contain("Гуськов Глеб Юрьевич");
            result.Should().Contain("Информационные системы");
            result.Should().Contain("Доцент");
            result.Should().Contain("812005");

            result.Should().NotContain("ПУБЛИКАЦИИ");
            result.Should().NotContain("ПОКАЗАТЕЛИ ELIBRARY");
        }

        [Fact]
        public void BuildReportText_WhenPublicationsIncluded_ShouldContainPublicationList()
        {
            // Arrange
            var researcher = CreateResearcher();
            var publications = CreatePublications();
            var eLibraryProfile = CreateELibraryProfile();

            var options = new ResearcherReportOptions
            {
                IncludeProfile = false,
                IncludePublications = true,
                IncludeELibraryProfile = false
            };

            // Act
            var result = _builder.BuildReportText(researcher, publications, eLibraryProfile, options);

            // Assert
            result.Should().Contain("ПУБЛИКАЦИИ");
            result.Should().Contain("Метод расширения словаря языковой модели");
            result.Should().Contain("Система анализа научной активности");
            result.Should().Contain("10.35752/1991-2927_2026_1_83_55");
            result.Should().Contain("Цитирования РИНЦ: 15");

            result.Should().NotContain("ПРОФИЛЬ ИССЛЕДОВАТЕЛЯ");
            result.Should().NotContain("ПОКАЗАТЕЛИ ELIBRARY");
        }

        [Fact]
        public void BuildReportText_WhenPublicationsExcluded_ShouldNotContainPublicationList()
        {
            // Arrange
            var researcher = CreateResearcher();
            var publications = CreatePublications();
            var eLibraryProfile = CreateELibraryProfile();

            var options = new ResearcherReportOptions
            {
                IncludeProfile = true,
                IncludePublications = false,
                IncludeELibraryProfile = true
            };

            // Act
            var result = _builder.BuildReportText(researcher, publications, eLibraryProfile, options);

            // Assert
            result.Should().Contain("ПРОФИЛЬ ИССЛЕДОВАТЕЛЯ");
            result.Should().Contain("ПОКАЗАТЕЛИ ELIBRARY");

            result.Should().NotContain("ПУБЛИКАЦИИ");
            result.Should().NotContain("Метод расширения словаря языковой модели");
            result.Should().NotContain("Система анализа научной активности");
        }

        [Fact]
        public void BuildReportText_WhenELibraryProfileIncluded_ShouldContainELibraryIndicators()
        {
            // Arrange
            var researcher = CreateResearcher();
            var publications = CreatePublications();
            var eLibraryProfile = CreateELibraryProfile();

            var options = new ResearcherReportOptions
            {
                IncludeProfile = false,
                IncludePublications = false,
                IncludeELibraryProfile = true
            };

            // Act
            var result = _builder.BuildReportText(researcher, publications, eLibraryProfile, options);

            // Assert
            result.Should().Contain("ПОКАЗАТЕЛИ ELIBRARY");
            result.Should().Contain("AuthorID: 812005");
            result.Should().Contain("SPIN-код: 2896-1945");
            result.Should().Contain("Публикации eLibrary: 100");
            result.Should().Contain("Публикации РИНЦ: 80");
            result.Should().Contain("Цитирования eLibrary: 500");
            result.Should().Contain("Индекс Хирша РИНЦ: 10");

            result.Should().NotContain("ПУБЛИКАЦИИ");
        }

        [Fact]
        public void BuildReportText_WhenELibraryProfileExcluded_ShouldNotContainELibraryIndicators()
        {
            // Arrange
            var researcher = CreateResearcher();
            var publications = CreatePublications();
            var eLibraryProfile = CreateELibraryProfile();

            var options = new ResearcherReportOptions
            {
                IncludeProfile = true,
                IncludePublications = true,
                IncludeELibraryProfile = false
            };

            // Act
            var result = _builder.BuildReportText(researcher, publications, eLibraryProfile, options);

            // Assert
            result.Should().Contain("ПРОФИЛЬ ИССЛЕДОВАТЕЛЯ");
            result.Should().Contain("ПУБЛИКАЦИИ");

            result.Should().NotContain("ПОКАЗАТЕЛИ ELIBRARY");
            result.Should().NotContain("Публикации eLibrary: 100");
            result.Should().NotContain("Индекс Хирша РИНЦ: 10");
        }

        [Fact]
        public void BuildReportText_WhenPublicationsListIsEmpty_ShouldBuildReportWithoutException()
        {
            // Arrange
            var researcher = CreateResearcher();
            var publications = new List<PublicationViewModel>();
            var eLibraryProfile = CreateELibraryProfile();

            var options = new ResearcherReportOptions
            {
                IncludeProfile = true,
                IncludePublications = true,
                IncludeELibraryProfile = false
            };

            // Act
            var action = () => _builder.BuildReportText(researcher, publications, eLibraryProfile, options);
            var result = action();

            // Assert
            action.Should().NotThrow();

            result.Should().Contain("ПРОФИЛЬ ИССЛЕДОВАТЕЛЯ");
            result.Should().Contain("ПУБЛИКАЦИИ");
            result.Should().Contain("Публикации не найдены.");
        }

        [Fact]
        public void BuildReportText_WhenELibraryProfileIsMissing_ShouldBuildReportWithAvailableData()
        {
            // Arrange
            var researcher = CreateResearcher();
            var publications = CreatePublications();
            ELibraryAuthorProfileViewModel? eLibraryProfile = null;

            var options = new ResearcherReportOptions
            {
                IncludeProfile = true,
                IncludePublications = false,
                IncludeELibraryProfile = true
            };

            // Act
            var result = _builder.BuildReportText(researcher, publications, eLibraryProfile, options);

            // Assert
            result.Should().Contain("ПРОФИЛЬ ИССЛЕДОВАТЕЛЯ");
            result.Should().Contain("Гуськов Глеб Юрьевич");
            result.Should().Contain("ПОКАЗАТЕЛИ ELIBRARY");
            result.Should().Contain("Профиль eLibrary не загружен.");
        }

        [Fact]
        public void GeneratePdfBytes_WhenReportTextIsCorrect_ShouldReturnPdfBytes()
        {
            // Arrange
            var researcher = CreateResearcher();

            var reportText = _builder.BuildReportText(
                researcher,
                CreatePublications(),
                CreateELibraryProfile(),
                new ResearcherReportOptions
                {
                    IncludeProfile = true,
                    IncludePublications = true,
                    IncludeELibraryProfile = true
                });

            // Act
            var result = _builder.GeneratePdfBytes(reportText);

            // Assert
            result.Should().NotBeNull();
            result.Should().NotBeEmpty();

            var header = System.Text.Encoding.UTF8.GetString(result.Take(8).ToArray());
            header.Should().StartWith("%PDF");
        }

        [Fact]
        public void GenerateDocxBytes_WhenReportTextIsCorrect_ShouldReturnDocxBytes()
        {
            // Arrange
            var researcher = CreateResearcher();

            var reportText = _builder.BuildReportText(
                researcher,
                CreatePublications(),
                CreateELibraryProfile(),
                new ResearcherReportOptions
                {
                    IncludeProfile = true,
                    IncludePublications = true,
                    IncludeELibraryProfile = true
                });

            // Act
            var result = _builder.GenerateDocxBytes(reportText);

            // Assert
            result.Should().NotBeNull();
            result.Should().NotBeEmpty();

            var header = System.Text.Encoding.UTF8.GetString(result.Take(2).ToArray());
            header.Should().Be("PK");
        }

        [Fact]
        public void BuildReportText_WhenNoSectionsSelected_ShouldThrowInvalidOperationException()
        {
            // Arrange
            var researcher = CreateResearcher();
            var publications = CreatePublications();
            var eLibraryProfile = CreateELibraryProfile();

            var options = new ResearcherReportOptions
            {
                IncludeProfile = false,
                IncludePublications = false,
                IncludeELibraryProfile = false
            };

            // Act
            var action = () => _builder.BuildReportText(researcher, publications, eLibraryProfile, options);

            // Assert
            action.Should().Throw<InvalidOperationException>()
                .WithMessage("Не выбран ни один раздел отчета");
        }

        private static ResearcherViewModel CreateResearcher()
        {
            return new ResearcherViewModel
            {
                Id = 1,
                Email = "researcher@example.com",
                PasswordHash = "hash",
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
                PublicationsCount = 2
            };
        }

        private static List<PublicationViewModel> CreatePublications()
        {
            return new List<PublicationViewModel>
            {
                new PublicationViewModel
                {
                    Id = 1,
                    Title = "Метод расширения словаря языковой модели",
                    Authors = "Гуськов Г. Ю., Грачева Д. А.",
                    Year = 2026,
                    Type = PublicationType.Статья_в_журнале,
                    Doi = "10.35752/1991-2927_2026_1_83_55",
                    Url = "https://www.elibrary.ru/item.asp?id=89112691",
                    ResearcherId = 1,
                    Keywords = "языковая модель; кластеризация; анализ данных",
                    Annotation = "Статья посвящена задаче обработки коротких текстов.",
                    CitationsRincCount = 15,
                    ELibraryId = "89112691",
                    IsInRinc = true,
                    IsInCoreRinc = true,
                    IsVak = true
                },
                new PublicationViewModel
                {
                    Id = 2,
                    Title = "Система анализа научной активности",
                    Authors = "Табеев А. П.",
                    Year = 2025,
                    Type = PublicationType.Статья_в_сборнике_конференции,
                    Doi = null,
                    Url = "https://example.com/publication",
                    ResearcherId = 1,
                    Keywords = "научная активность; рекомендации",
                    Annotation = "Работа посвящена разработке информационной системы.",
                    CitationsRincCount = 3,
                    ELibraryId = "TEST-002",
                    IsInRinc = true,
                    IsInCoreRinc = false,
                    IsVak = false
                }
            };
        }

        private static ELibraryAuthorProfileViewModel CreateELibraryProfile()
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
