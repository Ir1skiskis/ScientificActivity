using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using ScientificActivityBusinessLogics.BusinessLogics;
using ScientificActivityContracts.BindingModels;
using ScientificActivityContracts.SearchModels;
using ScientificActivityContracts.StoragesContracts;
using ScientificActivityContracts.ViewModels;
using ScientificActivityDataModels.Enums;
using ScientificActivityParsers.Interfaces;
using ScientificActivityParsers.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ScientificActivityBusinessLogics.Services;
using ScientificActivityContracts.BusinessLogicsContracts;

namespace ScientificActivityTests.Unit
{
    public class ELibraryLogicTests
    {
        private readonly Mock<IELibraryParser> _eLibraryParserMock;
        private readonly Mock<IResearcherStorage> _researcherStorageMock;
        private readonly Mock<IPublicationStorage> _publicationStorageMock;
        private readonly Mock<IELibraryAuthorProfileStorage> _eLibraryAuthorProfileStorageMock;
        private readonly Mock<IJournalStorage> _journalStorageMock;
        private readonly ELibraryLogic _logic;
        private readonly Mock<IRecommendationLogic> _recommendationLogicMock;
        private readonly ImportProgressService _progressService;

        public ELibraryLogicTests()
        {
            _eLibraryParserMock = new Mock<IELibraryParser>();
            _researcherStorageMock = new Mock<IResearcherStorage>();
            _publicationStorageMock = new Mock<IPublicationStorage>();
            _eLibraryAuthorProfileStorageMock = new Mock<IELibraryAuthorProfileStorage>();
            _journalStorageMock = new Mock<IJournalStorage>();
            _recommendationLogicMock = new Mock<IRecommendationLogic>();
            _progressService = new ImportProgressService();

            _recommendationLogicMock
                .Setup(x => x.AutoAssignResearcherTagsFromPublications(
                    It.IsAny<int>(),
                    It.IsAny<int>(),
                    It.IsAny<bool>()))
                .Returns(new List<TagViewModel>());

            _logic = new ELibraryLogic(
                NullLogger<ELibraryLogic>.Instance,
                _eLibraryParserMock.Object,
                _researcherStorageMock.Object,
                _publicationStorageMock.Object,
                _eLibraryAuthorProfileStorageMock.Object,
                _journalStorageMock.Object,
                _progressService,
                _recommendationLogicMock.Object);
        }

        [Fact]
        public void ImportAuthorProfile_WhenProfileReceived_ShouldSaveProfile()
        {
            // Arrange
            var researcher = CreateResearcher();
            var profile = CreateAuthorProfile();

            _researcherStorageMock
                .Setup(x => x.GetElement(It.Is<ResearcherSearchModel>(m => m.Id == 1)))
                .Returns(researcher);

            _eLibraryParserMock
                .Setup(x => x.GetAuthorProfile("812005"))
                .Returns(profile);

            _eLibraryAuthorProfileStorageMock
                .Setup(x => x.InsertOrUpdate(It.IsAny<ELibraryAuthorProfileBindingModel>()))
                .Returns(CreateSavedAuthorProfile());

            _researcherStorageMock
                .Setup(x => x.Update(It.IsAny<ResearcherBindingModel>()))
                .Returns((ResearcherBindingModel m) => ToResearcherViewModel(m));

            // Act
            var result = _logic.ImportAuthorProfile(new ELibraryImportBindingModel
            {
                ResearcherId = 1
            });

            // Assert
            result.Should().BeTrue();

            _eLibraryParserMock.Verify(
                x => x.GetAuthorProfile("812005"),
                Times.Once);

            _eLibraryAuthorProfileStorageMock.Verify(
                x => x.InsertOrUpdate(It.Is<ELibraryAuthorProfileBindingModel>(m =>
                    m.ResearcherId == 1 &&
                    m.AuthorId == "812005" &&
                    m.FullName == "Гуськов Глеб Юрьевич" &&
                    m.Organization == "Ульяновский государственный технический университет" &&
                    m.Department == "Информационные системы" &&
                    m.PublicationsCountElibrary == 100 &&
                    m.PublicationsCountRinc == 80 &&
                    m.CitationsCountElibrary == 500 &&
                    m.CitationsCountRinc == 450 &&
                    m.HIndexElibrary == 12 &&
                    m.HIndexRinc == 10)),
                Times.Once);
        }

        [Fact]
        public void ImportAuthorProfile_WhenProfileHasResearchTopics_ShouldUpdateResearcherResearchTopics()
        {
            // Arrange
            var researcher = CreateResearcher();
            researcher.ResearchTopics = "старые темы";

            var profile = CreateAuthorProfile();
            profile.ResearchTopics = "информационные системы; машинное обучение";

            _researcherStorageMock
                .Setup(x => x.GetElement(It.Is<ResearcherSearchModel>(m => m.Id == 1)))
                .Returns(researcher);

            _eLibraryParserMock
                .Setup(x => x.GetAuthorProfile("812005"))
                .Returns(profile);

            _eLibraryAuthorProfileStorageMock
                .Setup(x => x.InsertOrUpdate(It.IsAny<ELibraryAuthorProfileBindingModel>()))
                .Returns(CreateSavedAuthorProfile());

            _researcherStorageMock
                .Setup(x => x.Update(It.IsAny<ResearcherBindingModel>()))
                .Returns((ResearcherBindingModel m) => ToResearcherViewModel(m));

            // Act
            var result = _logic.ImportAuthorProfile(new ELibraryImportBindingModel
            {
                ResearcherId = 1
            });

            // Assert
            result.Should().BeTrue();

            _researcherStorageMock.Verify(
                x => x.Update(It.Is<ResearcherBindingModel>(m =>
                    m.Id == 1 &&
                    m.ELibraryAuthorId == "812005" &&
                    m.ResearchTopics == "информационные системы; машинное обучение")),
                Times.Once);
        }

        [Fact]
        public void ImportAuthorProfile_WhenResearcherNotFound_ShouldThrowException()
        {
            // Arrange
            _researcherStorageMock
                .Setup(x => x.GetElement(It.IsAny<ResearcherSearchModel>()))
                .Returns((ResearcherViewModel?)null);

            // Act
            var action = () => _logic.ImportAuthorProfile(new ELibraryImportBindingModel
            {
                ResearcherId = 1
            });

            // Assert
            action.Should().Throw<Exception>()
                .WithMessage("Исследователь не найден");

            _eLibraryParserMock.Verify(
                x => x.GetAuthorProfile(It.IsAny<string>()),
                Times.Never);

            _eLibraryAuthorProfileStorageMock.Verify(
                x => x.InsertOrUpdate(It.IsAny<ELibraryAuthorProfileBindingModel>()),
                Times.Never);
        }

        [Fact]
        public void ImportAuthorProfile_WhenResearcherHasNoELibraryAuthorId_ShouldThrowException()
        {
            // Arrange
            var researcher = CreateResearcher();
            researcher.ELibraryAuthorId = "";

            _researcherStorageMock
                .Setup(x => x.GetElement(It.Is<ResearcherSearchModel>(m => m.Id == 1)))
                .Returns(researcher);

            // Act
            var action = () => _logic.ImportAuthorProfile(new ELibraryImportBindingModel
            {
                ResearcherId = 1
            });

            // Assert
            action.Should().Throw<Exception>()
                .WithMessage("У исследователя не указан ELibraryAuthorId");

            _eLibraryParserMock.Verify(
                x => x.GetAuthorProfile(It.IsAny<string>()),
                Times.Never);
        }

        [Fact]
        public void ImportAuthorProfile_WhenParserReturnsNull_ShouldThrowException()
        {
            // Arrange
            var researcher = CreateResearcher();

            _researcherStorageMock
                .Setup(x => x.GetElement(It.Is<ResearcherSearchModel>(m => m.Id == 1)))
                .Returns(researcher);

            _eLibraryParserMock
                .Setup(x => x.GetAuthorProfile("812005"))
                .Returns((ELibraryAuthorProfileViewModel?)null);

            // Act
            var action = () => _logic.ImportAuthorProfile(new ELibraryImportBindingModel
            {
                ResearcherId = 1
            });

            // Assert
            action.Should().Throw<Exception>()
                .WithMessage("Не удалось получить профиль автора из eLibrary");

            _eLibraryAuthorProfileStorageMock.Verify(
                x => x.InsertOrUpdate(It.IsAny<ELibraryAuthorProfileBindingModel>()),
                Times.Never);

            _researcherStorageMock.Verify(
                x => x.Update(It.IsAny<ResearcherBindingModel>()),
                Times.Never);
        }

        [Fact]
        public void ImportAuthorPublications_WhenParserReturnsNewPublication_ShouldInsertPublication()
        {
            // Arrange
            SetupResearcher(CreateResearcher());
            SetupExistingPublications([]);

            var parsedPublications = new List<ELibraryPublicationImportModel>
            {
                new ELibraryPublicationImportModel
                {
                    Title = "Новая публикация eLibrary",
                    Authors = "Гуськов Г. Ю.",
                    Year = 2026,
                    ELibraryId = "ELIB-001",
                    Doi = "10.1000/new",
                    Url = "https://elibrary.ru/item.asp?id=1",
                    CitationsRincCount = 7
                }
            };

            _eLibraryParserMock
                .Setup(x => x.GetAuthorPublications(
                    "812005",
                    It.IsAny<Action<ParserProgressModel>?>()))
                .Returns(parsedPublications);

            _publicationStorageMock
                .Setup(x => x.Insert(It.IsAny<PublicationBindingModel>()))
                .Returns((PublicationBindingModel m) => ToPublicationViewModel(m, 1));

            // Act
            var result = _logic.ImportAuthorPublications(new ELibraryImportBindingModel
            {
                ResearcherId = 1
            });

            // Assert
            result.Should().Be(1);

            _publicationStorageMock.Verify(
                x => x.Insert(It.Is<PublicationBindingModel>(m =>
                    m.Title == "Новая публикация eLibrary" &&
                    m.Year == 2026 &&
                    m.ELibraryId == "ELIB-001" &&
                    m.Doi == "10.1000/new" &&
                    m.Url == "https://elibrary.ru/item.asp?id=1" &&
                    m.CitationsRincCount == 7 &&
                    m.ResearcherId == 1)),
                Times.Once);
        }

        [Fact]
        public void ImportAuthorPublications_WhenPublicationExistsByELibraryId_ShouldUpdatePublication()
        {
            // Arrange
            SetupResearcher(CreateResearcher());

            var existingPublications = new List<PublicationViewModel>
            {
                new PublicationViewModel
                {
                    Id = 10,
                    Title = "Старая публикация",
                    Year = 2024,
                    ResearcherId = 1,
                    ELibraryId = "ELIB-001",
                    Doi = "10.old",
                    Url = "https://old.example.com",
                    IsInRinc = true
                }
            };

            SetupExistingPublications(existingPublications);

            _eLibraryParserMock
                .Setup(x => x.GetAuthorPublications(
                    "812005",
                    It.IsAny<Action<ParserProgressModel>?>()))
                .Returns(new List<ELibraryPublicationImportModel>
                {
                    new ELibraryPublicationImportModel
                    {
                        Title = "Обновленная публикация",
                        Authors = "Гуськов Г. Ю.",
                        Year = 2024,
                        ELibraryId = "ELIB-001",
                        Doi = "10.new",
                        Url = "https://new.example.com",
                        CitationsRincCount = 15
                    }
                });

            _publicationStorageMock
                .Setup(x => x.Update(It.IsAny<PublicationBindingModel>()))
                .Returns((PublicationBindingModel m) => ToPublicationViewModel(m, m.Id));

            // Act
            var result = _logic.ImportAuthorPublications(new ELibraryImportBindingModel
            {
                ResearcherId = 1
            });

            // Assert
            result.Should().Be(1);

            _publicationStorageMock.Verify(
                x => x.Update(It.Is<PublicationBindingModel>(m =>
                    m.Id == 10 &&
                    m.Title == "Обновленная публикация" &&
                    m.ELibraryId == "ELIB-001" &&
                    m.Doi == "10.new" &&
                    m.Url == "https://new.example.com" &&
                    m.CitationsRincCount == 15)),
                Times.Once);

            _publicationStorageMock.Verify(
                x => x.Insert(It.IsAny<PublicationBindingModel>()),
                Times.Never);
        }

        [Fact]
        public void ImportAuthorPublications_WhenPublicationExistsByTitleAndYear_ShouldUpdatePublication()
        {
            // Arrange
            SetupResearcher(CreateResearcher());

            var existingPublications = new List<PublicationViewModel>
            {
                new PublicationViewModel
                {
                    Id = 20,
                    Title = "Публикация с совпадающим названием",
                    Year = 2025,
                    ResearcherId = 1,
                    ELibraryId = null,
                    Keywords = "старые ключевые слова"
                }
            };

            SetupExistingPublications(existingPublications);

            _eLibraryParserMock
                .Setup(x => x.GetAuthorPublications(
                    "812005",
                    It.IsAny<Action<ParserProgressModel>?>()))
                .Returns(new List<ELibraryPublicationImportModel>
                {
                    new ELibraryPublicationImportModel
                    {
                        Title = "Публикация с совпадающим названием",
                        Authors = "Гуськов Г. Ю.",
                        Year = 2025,
                        ELibraryId = "ELIB-002",
                        Keywords = "новые ключевые слова"
                    }
                });

            _publicationStorageMock
                .Setup(x => x.Update(It.IsAny<PublicationBindingModel>()))
                .Returns((PublicationBindingModel m) => ToPublicationViewModel(m, m.Id));

            // Act
            var result = _logic.ImportAuthorPublications(new ELibraryImportBindingModel
            {
                ResearcherId = 1
            });

            // Assert
            result.Should().Be(1);

            _publicationStorageMock.Verify(
                x => x.Update(It.Is<PublicationBindingModel>(m =>
                    m.Id == 20 &&
                    m.Title == "Публикация с совпадающим названием" &&
                    m.Year == 2025 &&
                    m.ELibraryId == "ELIB-002" &&
                    m.Keywords == "новые ключевые слова")),
                Times.Once);

            _publicationStorageMock.Verify(
                x => x.Insert(It.IsAny<PublicationBindingModel>()),
                Times.Never);
        }

        [Fact]
        public void ImportAuthorPublications_WhenPublicationTitleIsEmpty_ShouldSkipPublication()
        {
            // Arrange
            SetupResearcher(CreateResearcher());
            SetupExistingPublications([]);

            _eLibraryParserMock
                .Setup(x => x.GetAuthorPublications(
                    "812005",
                    It.IsAny<Action<ParserProgressModel>?>()))
                .Returns(new List<ELibraryPublicationImportModel>
                {
                    new ELibraryPublicationImportModel
                    {
                        Title = "",
                        Authors = "Гуськов Г. Ю.",
                        Year = 2026,
                        ELibraryId = "ELIB-EMPTY"
                    }
                });

            // Act
            var result = _logic.ImportAuthorPublications(new ELibraryImportBindingModel
            {
                ResearcherId = 1
            });

            // Assert
            result.Should().Be(0);

            _publicationStorageMock.Verify(
                x => x.Insert(It.IsAny<PublicationBindingModel>()),
                Times.Never);

            _publicationStorageMock.Verify(
                x => x.Update(It.IsAny<PublicationBindingModel>()),
                Times.Never);
        }

        [Fact]
        public void ImportAuthorPublications_WhenJournalDoesNotExist_ShouldCreateJournalAndLinkPublication()
        {
            // Arrange
            SetupResearcher(CreateResearcher());
            SetupExistingPublications([]);

            _journalStorageMock
                .Setup(x => x.GetFullList())
                .Returns(new List<JournalViewModel>());

            _eLibraryParserMock
                .Setup(x => x.GetAuthorPublications(
                    "812005",
                    It.IsAny<Action<ParserProgressModel>?>()))
                .Returns(new List<ELibraryPublicationImportModel>
                {
                    new ELibraryPublicationImportModel
                    {
                        Title = "Публикация в новом журнале",
                        Authors = "Гуськов Г. Ю.",
                        Year = 2026,
                        ELibraryId = "ELIB-003",
                        JournalTitle = "Новый научный журнал",
                        JournalIssn = "2222-3333",
                        RubricOecd = "Computer and information sciences",
                        IsWhiteListLevel2 = true
                    }
                });

            _journalStorageMock
                .Setup(x => x.GetElement(It.Is<JournalSearchModel>(m => m.Issn == "2222-3333")))
                .Returns((JournalViewModel?)null);

            _journalStorageMock
                .Setup(x => x.GetElement(It.Is<JournalSearchModel>(m => m.Title == "Новый научный журнал")))
                .Returns((JournalViewModel?)null);

            _journalStorageMock
                .Setup(x => x.Insert(It.IsAny<JournalBindingModel>()))
                .Returns(new JournalViewModel
                {
                    Id = 200,
                    Title = "Новый научный журнал",
                    Issn = "2222-3333",
                    IsWhiteList = true,
                    WhiteListLevel2025 = 2
                });

            _publicationStorageMock
                .Setup(x => x.Insert(It.IsAny<PublicationBindingModel>()))
                .Returns((PublicationBindingModel m) => ToPublicationViewModel(m, 30));

            // Act
            var result = _logic.ImportAuthorPublications(new ELibraryImportBindingModel
            {
                ResearcherId = 1
            });

            // Assert
            result.Should().Be(1);

            _journalStorageMock.Verify(
                x => x.Insert(It.Is<JournalBindingModel>(m =>
                    m.Title == "Новый научный журнал" &&
                    m.Issn == "2222-3333" &&
                    m.SubjectArea == "Computer and information sciences" &&
                    m.IsWhiteList == true &&
                    m.WhiteListLevel2025 == 2)),
                Times.Once);

            _publicationStorageMock.Verify(
                x => x.Insert(It.Is<PublicationBindingModel>(m =>
                    m.Title == "Публикация в новом журнале" &&
                    m.JournalId == 200)),
                Times.Once);
        }

        [Fact]
        public void ImportAuthorPublications_WhenJournalExistsByIssn_ShouldLinkPublicationToExistingJournal()
        {
            // Arrange
            SetupResearcher(CreateResearcher());
            SetupExistingPublications([]);

            _journalStorageMock
                .Setup(x => x.GetFullList())
                .Returns(new List<JournalViewModel>
                {
                    new JournalViewModel
                    {
                        Id = 100,
                        Title = "Журнал информационных систем",
                        Issn = "1234-5678"
                    }
                });

            _eLibraryParserMock
                .Setup(x => x.GetAuthorPublications(
                    "812005",
                    It.IsAny<Action<ParserProgressModel>?>()))
                .Returns(new List<ELibraryPublicationImportModel>
                {
                    new ELibraryPublicationImportModel
                    {
                        Title = "Публикация в существующем журнале",
                        Authors = "Гуськов Г. Ю.",
                        Year = 2026,
                        ELibraryId = "ELIB-004",
                        JournalTitle = "Журнал информационных систем",
                        JournalIssn = "1234-5678"
                    }
                });

            _journalStorageMock
                .Setup(x => x.GetElement(It.Is<JournalSearchModel>(m => m.Issn == "1234-5678")))
                .Returns(new JournalViewModel
                {
                    Id = 100,
                    Title = "Журнал информационных систем",
                    Issn = "1234-5678"
                });

            _publicationStorageMock
                .Setup(x => x.Insert(It.IsAny<PublicationBindingModel>()))
                .Returns((PublicationBindingModel m) => ToPublicationViewModel(m, 40));

            // Act
            var result = _logic.ImportAuthorPublications(new ELibraryImportBindingModel
            {
                ResearcherId = 1
            });

            // Assert
            result.Should().Be(1);

            _publicationStorageMock.Verify(
                x => x.Insert(It.Is<PublicationBindingModel>(m =>
                    m.Title == "Публикация в существующем журнале" &&
                    m.JournalId == 100)),
                Times.Once);

            _journalStorageMock.Verify(
                x => x.Insert(It.IsAny<JournalBindingModel>()),
                Times.Never);
        }

        [Fact]
        public void ImportAuthorPublications_WhenPublicationIsInCoreRinc_ShouldSetIsInRincTrue()
        {
            // Arrange
            SetupResearcher(CreateResearcher());
            SetupExistingPublications([]);

            _eLibraryParserMock
                .Setup(x => x.GetAuthorPublications(
                    "812005",
                    It.IsAny<Action<ParserProgressModel>?>()))
                .Returns(new List<ELibraryPublicationImportModel>
                {
                    new ELibraryPublicationImportModel
                    {
                        Title = "Публикация из ядра РИНЦ",
                        Authors = "Гуськов Г. Ю.",
                        Year = 2026,
                        ELibraryId = "ELIB-005",
                        IsInRinc = false,
                        IsInCoreRinc = true
                    }
                });

            _publicationStorageMock
                .Setup(x => x.Insert(It.IsAny<PublicationBindingModel>()))
                .Returns((PublicationBindingModel m) => ToPublicationViewModel(m, 50));

            // Act
            var result = _logic.ImportAuthorPublications(new ELibraryImportBindingModel
            {
                ResearcherId = 1
            });

            // Assert
            result.Should().Be(1);

            _publicationStorageMock.Verify(
                x => x.Insert(It.Is<PublicationBindingModel>(m =>
                    m.Title == "Публикация из ядра РИНЦ" &&
                    m.IsInCoreRinc == true &&
                    m.IsInRinc == true)),
                Times.Once);
        }

        [Fact]
        public void ImportAuthorPublications_WhenPublicationHasVakCategory_ShouldSetIsVakTrue()
        {
            // Arrange
            SetupResearcher(CreateResearcher());
            SetupExistingPublications([]);

            _eLibraryParserMock
                .Setup(x => x.GetAuthorPublications(
                    "812005",
                    It.IsAny<Action<ParserProgressModel>?>()))
                .Returns(new List<ELibraryPublicationImportModel>
                {
                    new ELibraryPublicationImportModel
                    {
                        Title = "Публикация ВАК",
                        Authors = "Гуськов Г. Ю.",
                        Year = 2026,
                        ELibraryId = "ELIB-006",
                        IsVak = false,
                        IsVakCategory2 = true
                    }
                });

            _publicationStorageMock
                .Setup(x => x.Insert(It.IsAny<PublicationBindingModel>()))
                .Returns((PublicationBindingModel m) => ToPublicationViewModel(m, 60));

            // Act
            var result = _logic.ImportAuthorPublications(new ELibraryImportBindingModel
            {
                ResearcherId = 1
            });

            // Assert
            result.Should().Be(1);

            _publicationStorageMock.Verify(
                x => x.Insert(It.Is<PublicationBindingModel>(m =>
                    m.Title == "Публикация ВАК" &&
                    m.IsVakCategory2 == true &&
                    m.IsVak == true)),
                Times.Once);
        }

        [Fact]
        public void ImportAuthorPublications_WhenParserReturnsEmptyList_ShouldReturnZero()
        {
            // Arrange
            SetupResearcher(CreateResearcher());

            _eLibraryParserMock
                .Setup(x => x.GetAuthorPublications(
                    "812005",
                    It.IsAny<Action<ParserProgressModel>?>()))
                .Returns(new List<ELibraryPublicationImportModel>());

            // Act
            var result = _logic.ImportAuthorPublications(new ELibraryImportBindingModel
            {
                ResearcherId = 1
            });

            // Assert
            result.Should().Be(0);

            _publicationStorageMock.Verify(
                x => x.GetFilteredList(It.IsAny<PublicationSearchModel>()),
                Times.Never);

            _publicationStorageMock.Verify(
                x => x.Insert(It.IsAny<PublicationBindingModel>()),
                Times.Never);

            _publicationStorageMock.Verify(
                x => x.Update(It.IsAny<PublicationBindingModel>()),
                Times.Never);
        }

        [Fact]
        public void ImportAuthorPublications_WhenPublicationHasDoiUrlAndCitations_ShouldMapFieldsToBindingModel()
        {
            // Arrange
            SetupResearcher(CreateResearcher());
            SetupExistingPublications([]);

            _eLibraryParserMock
                .Setup(x => x.GetAuthorPublications(
                    "812005",
                    It.IsAny<Action<ParserProgressModel>?>()))
                .Returns(new List<ELibraryPublicationImportModel>
                {
                    new ELibraryPublicationImportModel
                    {
                        Title = "Публикация с DOI URL и цитированиями",
                        Authors = "Гуськов Г. Ю.",
                        Year = 2026,
                        ELibraryId = "ELIB-007",
                        Doi = "10.35752/1991-2927_2026_1_83_55",
                        Url = "https://www.elibrary.ru/item.asp?id=89112691",
                        CitationsRincCount = 25
                    }
                });

            _publicationStorageMock
                .Setup(x => x.Insert(It.IsAny<PublicationBindingModel>()))
                .Returns((PublicationBindingModel m) => ToPublicationViewModel(m, 70));

            // Act
            var result = _logic.ImportAuthorPublications(new ELibraryImportBindingModel
            {
                ResearcherId = 1
            });

            // Assert
            result.Should().Be(1);

            _publicationStorageMock.Verify(
                x => x.Insert(It.Is<PublicationBindingModel>(m =>
                    m.Title == "Публикация с DOI URL и цитированиями" &&
                    m.Doi == "10.35752/1991-2927_2026_1_83_55" &&
                    m.Url == "https://www.elibrary.ru/item.asp?id=89112691" &&
                    m.CitationsRincCount == 25)),
                Times.Once);
        }

        private void SetupResearcher(ResearcherViewModel researcher)
        {
            _researcherStorageMock
                .Setup(x => x.GetElement(It.Is<ResearcherSearchModel>(m => m.Id == researcher.Id)))
                .Returns(researcher);
        }

        private void SetupExistingPublications(List<PublicationViewModel> publications)
        {
            _publicationStorageMock
                .Setup(x => x.GetFilteredList(It.Is<PublicationSearchModel>(m => m.ResearcherId == 1)))
                .Returns(publications);
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
                ResearchTopics = "старые темы"
            };
        }

        private static ELibraryAuthorProfileViewModel CreateAuthorProfile()
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

                PublicationsRincByYear = new Dictionary<int, int>
                {
                    { 2024, 10 },
                    { 2025, 12 }
                },
                PublicationsCoreRincByYear = new Dictionary<int, int>
                {
                    { 2024, 4 },
                    { 2025, 5 }
                },
                CitationsRincByYear = new Dictionary<int, int>
                {
                    { 2024, 50 },
                    { 2025, 70 }
                },
                CitationsCoreRincByYear = new Dictionary<int, int>
                {
                    { 2024, 20 },
                    { 2025, 30 }
                },
                HIndexRincByYear = new Dictionary<int, int>
                {
                    { 2024, 9 },
                    { 2025, 10 }
                },
                HIndexCoreRincByYear = new Dictionary<int, int>
                {
                    { 2024, 5 },
                    { 2025, 6 }
                },
                PercentileCoreRincByYear = new Dictionary<int, int>
                {
                    { 2024, 30 },
                    { 2025, 25 }
                },
                PublicationsRinc5YearsByEndYear = new Dictionary<int, int>
                {
                    { 2025, 31 }
                },
                PublicationsCoreRinc5YearsByEndYear = new Dictionary<int, int>
                {
                    { 2025, 12 }
                },
                CitationsRinc5YearsByEndYear = new Dictionary<int, int>
                {
                    { 2025, 210 }
                },
                CitationsCoreRinc5YearsByEndYear = new Dictionary<int, int>
                {
                    { 2025, 90 }
                },

                ResearchTopics = "информационные системы; машинное обучение"
            };
        }

        private static ELibraryAuthorProfileViewModel CreateSavedAuthorProfile()
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
                CitationsCountElibrary = 500,
                CitationsCountRinc = 450,
                HIndexElibrary = 12,
                HIndexRinc = 10,
                ResearchTopics = "информационные системы; машинное обучение"
            };
        }

        private static ResearcherViewModel ToResearcherViewModel(ResearcherBindingModel model)
        {
            return new ResearcherViewModel
            {
                Id = model.Id,
                Email = model.Email,
                PasswordHash = model.PasswordHash,
                Role = model.Role,
                IsActive = model.IsActive,
                LastName = model.LastName,
                FirstName = model.FirstName,
                MiddleName = model.MiddleName,
                Phone = model.Phone,
                Department = model.Department,
                Position = model.Position,
                AcademicDegree = model.AcademicDegree,
                ELibraryAuthorId = model.ELibraryAuthorId,
                ResearchTopics = model.ResearchTopics
            };
        }

        private static PublicationViewModel ToPublicationViewModel(PublicationBindingModel model, int id)
        {
            return new PublicationViewModel
            {
                Id = id,
                Title = model.Title,
                Authors = model.Authors,
                Year = model.Year,
                PublicationDate = model.PublicationDate,
                Type = model.Type,
                Doi = model.Doi,
                Url = model.Url,
                JournalId = model.JournalId,
                ConferenceId = model.ConferenceId,
                ResearcherId = model.ResearcherId,
                Keywords = model.Keywords,
                Annotation = model.Annotation,
                CitationsRincCount = model.CitationsRincCount,
                ELibraryId = model.ELibraryId,

                IsInRinc = model.IsInRinc,
                IsInCoreRinc = model.IsInCoreRinc,

                IsWhiteListLevel1 = model.IsWhiteListLevel1,
                IsWhiteListLevel2 = model.IsWhiteListLevel2,
                IsWhiteListLevel3 = model.IsWhiteListLevel3,
                IsWhiteListLevel4 = model.IsWhiteListLevel4,

                IsRsci = model.IsRsci,

                IsScopusQ1 = model.IsScopusQ1,
                IsScopusQ2 = model.IsScopusQ2,
                IsScopusQ3 = model.IsScopusQ3,
                IsScopusQ4 = model.IsScopusQ4,

                IsWebOfScienceQ1 = model.IsWebOfScienceQ1,
                IsWebOfScienceQ2 = model.IsWebOfScienceQ2,
                IsWebOfScienceQ3 = model.IsWebOfScienceQ3,
                IsWebOfScienceQ4 = model.IsWebOfScienceQ4,
                IsWebOfScienceNoQuartile = model.IsWebOfScienceNoQuartile,

                IsVak = model.IsVak,
                IsVakCategory1 = model.IsVakCategory1,
                IsVakCategory2 = model.IsVakCategory2,
                IsVakCategory3 = model.IsVakCategory3,

                RubricOecd = model.RubricOecd,
                RubricAsjc = model.RubricAsjc,
                RubricGrnti = model.RubricGrnti,
                VakSpecialty = model.VakSpecialty
            };
        }
    }
}
