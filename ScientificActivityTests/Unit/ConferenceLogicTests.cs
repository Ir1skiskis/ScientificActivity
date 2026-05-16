using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using ScientificActivityBusinessLogics.BusinessLogics;
using ScientificActivityContracts.BindingModels;
using ScientificActivityContracts.SearchModels;
using ScientificActivityContracts.StoragesContracts;
using ScientificActivityContracts.ViewModels;
using ScientificActivityDataModels.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScientificActivityTests.Unit
{
    public class ConferenceLogicTests
    {
        private readonly Mock<IConferenceStorage> _conferenceStorageMock;
        private readonly ConferenceLogic _logic;

        public ConferenceLogicTests()
        {
            _conferenceStorageMock = new Mock<IConferenceStorage>();

            _logic = new ConferenceLogic(
                NullLogger<ConferenceLogic>.Instance,
                _conferenceStorageMock.Object);
        }

        [Fact]
        public void Create_WhenConferenceModelIsCorrect_ShouldInsertConferenceAndReturnTrue()
        {
            // Arrange
            var model = CreateValidConferenceBindingModel();

            _conferenceStorageMock
                .Setup(x => x.Insert(It.IsAny<ConferenceBindingModel>()))
                .Returns((ConferenceBindingModel m) => ToConferenceViewModel(m, 1));

            // Act
            var result = _logic.Create(model);

            // Assert
            result.Should().BeTrue();

            _conferenceStorageMock.Verify(
                x => x.Insert(It.Is<ConferenceBindingModel>(m =>
                    m.Title == "Международная конференция по информационным системам" &&
                    m.StartDate == new DateTime(2026, 6, 10) &&
                    m.EndDate == new DateTime(2026, 6, 12) &&
                    m.Url == "https://example.com/conference" &&
                    m.SubjectArea == "Информационные системы; Машинное обучение")),
                Times.Once);
        }

        [Fact]
        public void Update_WhenConferenceModelIsCorrect_ShouldUpdateConferenceAndReturnTrue()
        {
            // Arrange
            var model = CreateValidConferenceBindingModel();
            model.Id = 10;
            model.Title = "Обновленная конференция по искусственному интеллекту";
            model.City = "Москва";
            model.SubjectArea = "Искусственный интеллект; Анализ данных";
            model.Url = "https://example.com/updated-conference";

            _conferenceStorageMock
                .Setup(x => x.Update(It.IsAny<ConferenceBindingModel>()))
                .Returns((ConferenceBindingModel m) => ToConferenceViewModel(m, m.Id));

            // Act
            var result = _logic.Update(model);

            // Assert
            result.Should().BeTrue();

            _conferenceStorageMock.Verify(
                x => x.Update(It.Is<ConferenceBindingModel>(m =>
                    m.Id == 10 &&
                    m.Title == "Обновленная конференция по искусственному интеллекту" &&
                    m.City == "Москва" &&
                    m.SubjectArea == "Искусственный интеллект; Анализ данных" &&
                    m.Url == "https://example.com/updated-conference")),
                Times.Once);

            _conferenceStorageMock.Verify(
                x => x.Insert(It.IsAny<ConferenceBindingModel>()),
                Times.Never);
        }

        [Fact]
        public void Create_WhenTitleIsEmpty_ShouldThrowArgumentNullExceptionAndNotInsert()
        {
            // Arrange
            var model = CreateValidConferenceBindingModel();
            model.Title = "";

            // Act
            var action = () => _logic.Create(model);

            // Assert
            action.Should().Throw<ArgumentNullException>()
                .WithMessage("*Не указано название конференции*");

            _conferenceStorageMock.Verify(
                x => x.Insert(It.IsAny<ConferenceBindingModel>()),
                Times.Never);
        }

        [Fact]
        public void ReadList_WhenSearchByTitle_ShouldReturnMatchingConferences()
        {
            // Arrange
            var conferences = new List<ConferenceViewModel>
            {
                CreateConferenceViewModel(
                    id: 1,
                    title: "Конференция по информационным системам",
                    subjectArea: "Информационные системы",
                    startDate: DateTime.Today.AddDays(20),
                    endDate: DateTime.Today.AddDays(22),
                    format: ConferenceFormat.Очная,
                    level: ConferenceLevel.Всероссийская),
                CreateConferenceViewModel(
                    id: 2,
                    title: "Конференция по химии",
                    subjectArea: "Химия",
                    startDate: DateTime.Today.AddDays(30),
                    endDate: DateTime.Today.AddDays(31),
                    format: ConferenceFormat.Онлайн,
                    level: ConferenceLevel.Международная)
            };

            _conferenceStorageMock
                .Setup(x => x.GetFilteredList(It.Is<ConferenceSearchModel>(m => m.Title == "информационным системам")))
                .Returns(conferences.Where(x => x.Title.Contains("информационным системам", StringComparison.OrdinalIgnoreCase)).ToList());

            // Act
            var result = _logic.ReadList(new ConferenceSearchModel
            {
                Title = "информационным системам"
            });

            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(1);
            result![0].Title.Should().Be("Конференция по информационным системам");

            _conferenceStorageMock.Verify(
                x => x.GetFilteredList(It.Is<ConferenceSearchModel>(m => m.Title == "информационным системам")),
                Times.Once);
        }

        [Fact]
        public void ReadList_WhenSearchBySubjectArea_ShouldReturnMatchingConferences()
        {
            // Arrange
            var conferences = new List<ConferenceViewModel>
            {
                CreateConferenceViewModel(
                    id: 1,
                    title: "Конференция по машинному обучению",
                    subjectArea: "Машинное обучение; Искусственный интеллект",
                    startDate: DateTime.Today.AddDays(10),
                    endDate: DateTime.Today.AddDays(12),
                    format: ConferenceFormat.Смешанная,
                    level: ConferenceLevel.Международная)
            };

            _conferenceStorageMock
                .Setup(x => x.GetFilteredList(It.Is<ConferenceSearchModel>(m => m.SubjectArea == "Машинное обучение")))
                .Returns(conferences);

            // Act
            var result = _logic.ReadList(new ConferenceSearchModel
            {
                SubjectArea = "Машинное обучение"
            });

            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(1);
            result.Should().OnlyContain(x => x.SubjectArea != null && x.SubjectArea.Contains("Машинное обучение"));

            _conferenceStorageMock.Verify(
                x => x.GetFilteredList(It.Is<ConferenceSearchModel>(m => m.SubjectArea == "Машинное обучение")),
                Times.Once);
        }

        [Fact]
        public void ReadList_WhenSearchActualConferences_ShouldReturnOnlyFutureConferences()
        {
            // Arrange
            var today = DateTime.Today;

            var actualConferences = new List<ConferenceViewModel>
            {
                CreateConferenceViewModel(
                    id: 1,
                    title: "Актуальная конференция 1",
                    subjectArea: "Информационные системы",
                    startDate: today.AddDays(5),
                    endDate: today.AddDays(7),
                    format: ConferenceFormat.Очная,
                    level: ConferenceLevel.Всероссийская),
                CreateConferenceViewModel(
                    id: 2,
                    title: "Актуальная конференция 2",
                    subjectArea: "Машинное обучение",
                    startDate: today.AddDays(20),
                    endDate: today.AddDays(21),
                    format: ConferenceFormat.Онлайн,
                    level: ConferenceLevel.Международная)
            };

            _conferenceStorageMock
                .Setup(x => x.GetFilteredList(It.Is<ConferenceSearchModel>(m =>
                    m.DateFrom.HasValue &&
                    m.DateFrom.Value.Date == today)))
                .Returns(actualConferences);

            // Act
            var result = _logic.ReadList(new ConferenceSearchModel
            {
                DateFrom = today
            });

            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(2);
            result.Should().OnlyContain(x => x.EndDate.Date >= today);

            _conferenceStorageMock.Verify(
                x => x.GetFilteredList(It.Is<ConferenceSearchModel>(m =>
                    m.DateFrom.HasValue &&
                    m.DateFrom.Value.Date == today)),
                Times.Once);
        }

        [Fact]
        public void ReadList_WhenSearchActualConferences_ShouldNotReturnPastConferences()
        {
            // Arrange
            var today = DateTime.Today;

            var actualConferences = new List<ConferenceViewModel>
            {
                CreateConferenceViewModel(
                    id: 1,
                    title: "Будущая конференция",
                    subjectArea: "Информационные системы",
                    startDate: today.AddDays(15),
                    endDate: today.AddDays(17),
                    format: ConferenceFormat.Очная,
                    level: ConferenceLevel.Всероссийская)
            };

            _conferenceStorageMock
                .Setup(x => x.GetFilteredList(It.Is<ConferenceSearchModel>(m =>
                    m.DateFrom.HasValue &&
                    m.DateFrom.Value.Date == today)))
                .Returns(actualConferences);

            // Act
            var result = _logic.ReadList(new ConferenceSearchModel
            {
                DateFrom = today
            });

            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(1);
            result.Should().NotContain(x => x.EndDate.Date < today);
            result![0].Title.Should().Be("Будущая конференция");
        }

        [Fact]
        public void ReadList_WhenSearchByLevel_ShouldReturnConferencesWithSpecifiedLevel()
        {
            // Arrange
            var conferences = new List<ConferenceViewModel>
            {
                CreateConferenceViewModel(
                    id: 1,
                    title: "Международная конференция",
                    subjectArea: "Информационные системы",
                    startDate: DateTime.Today.AddDays(10),
                    endDate: DateTime.Today.AddDays(11),
                    format: ConferenceFormat.Смешанная,
                    level: ConferenceLevel.Международная)
            };

            _conferenceStorageMock
                .Setup(x => x.GetFilteredList(It.Is<ConferenceSearchModel>(m => m.Level == ConferenceLevel.Международная)))
                .Returns(conferences);

            // Act
            var result = _logic.ReadList(new ConferenceSearchModel
            {
                Level = ConferenceLevel.Международная
            });

            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(1);
            result.Should().OnlyContain(x => x.Level == ConferenceLevel.Международная);

            _conferenceStorageMock.Verify(
                x => x.GetFilteredList(It.Is<ConferenceSearchModel>(m => m.Level == ConferenceLevel.Международная)),
                Times.Once);
        }

        [Fact]
        public void ReadList_WhenSearchByFormat_ShouldReturnConferencesWithSpecifiedFormat()
        {
            // Arrange
            var conferences = new List<ConferenceViewModel>
            {
                CreateConferenceViewModel(
                    id: 1,
                    title: "Онлайн-конференция",
                    subjectArea: "Программная инженерия",
                    startDate: DateTime.Today.AddDays(8),
                    endDate: DateTime.Today.AddDays(9),
                    format: ConferenceFormat.Онлайн,
                    level: ConferenceLevel.Всероссийская)
            };

            _conferenceStorageMock
                .Setup(x => x.GetFilteredList(It.Is<ConferenceSearchModel>(m => m.Format == ConferenceFormat.Онлайн)))
                .Returns(conferences);

            // Act
            var result = _logic.ReadList(new ConferenceSearchModel
            {
                Format = ConferenceFormat.Онлайн
            });

            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(1);
            result.Should().OnlyContain(x => x.Format == ConferenceFormat.Онлайн);

            _conferenceStorageMock.Verify(
                x => x.GetFilteredList(It.Is<ConferenceSearchModel>(m => m.Format == ConferenceFormat.Онлайн)),
                Times.Once);
        }

        [Fact]
        public void Create_WhenEndDateEarlierThanStartDate_ShouldThrowArgumentExceptionAndNotInsert()
        {
            // Arrange
            var model = CreateValidConferenceBindingModel();
            model.StartDate = new DateTime(2026, 8, 10);
            model.EndDate = new DateTime(2026, 8, 5);

            // Act
            var action = () => _logic.Create(model);

            // Assert
            action.Should().Throw<ArgumentException>()
                .WithMessage("Дата окончания не может быть раньше даты начала");

            _conferenceStorageMock.Verify(
                x => x.Insert(It.IsAny<ConferenceBindingModel>()),
                Times.Never);
        }

        [Fact]
        public void Update_WhenExistingConferenceIsUpdated_ShouldCallUpdateNotInsert()
        {
            // Arrange
            var model = CreateValidConferenceBindingModel();
            model.Id = 25;
            model.Title = "Конференция без повторного сохранения";
            model.Url = "https://example.com/existing-conference";

            _conferenceStorageMock
                .Setup(x => x.Update(It.IsAny<ConferenceBindingModel>()))
                .Returns((ConferenceBindingModel m) => ToConferenceViewModel(m, m.Id));

            // Act
            var result = _logic.Update(model);

            // Assert
            result.Should().BeTrue();

            _conferenceStorageMock.Verify(
                x => x.Update(It.Is<ConferenceBindingModel>(m =>
                    m.Id == 25 &&
                    m.Title == "Конференция без повторного сохранения" &&
                    m.Url == "https://example.com/existing-conference")),
                Times.Once);

            _conferenceStorageMock.Verify(
                x => x.Insert(It.IsAny<ConferenceBindingModel>()),
                Times.Never);
        }

        [Fact]
        public void ReadList_WhenStorageReturnsEmptyList_ShouldReturnEmptyList()
        {
            // Arrange
            _conferenceStorageMock
                .Setup(x => x.GetFullList())
                .Returns([]);

            // Act
            var result = _logic.ReadList(null);

            // Assert
            result.Should().NotBeNull();
            result.Should().BeEmpty();

            _conferenceStorageMock.Verify(
                x => x.GetFullList(),
                Times.Once);
        }

        [Fact]
        public void Create_WhenStartDateIsDefault_ShouldThrowArgumentExceptionAndNotInsert()
        {
            // Arrange
            var model = CreateValidConferenceBindingModel();
            model.StartDate = default;

            // Act
            var action = () => _logic.Create(model);

            // Assert
            action.Should().Throw<ArgumentException>()
                .WithMessage("*Не указана дата начала конференции*");

            _conferenceStorageMock.Verify(
                x => x.Insert(It.IsAny<ConferenceBindingModel>()),
                Times.Never);
        }

        [Fact]
        public void Create_WhenEndDateIsDefault_ShouldThrowArgumentExceptionAndNotInsert()
        {
            // Arrange
            var model = CreateValidConferenceBindingModel();
            model.EndDate = default;

            // Act
            var action = () => _logic.Create(model);

            // Assert
            action.Should().Throw<ArgumentException>()
                .WithMessage("*Не указана дата окончания конференции*");

            _conferenceStorageMock.Verify(
                x => x.Insert(It.IsAny<ConferenceBindingModel>()),
                Times.Never);
        }

        [Fact]
        public void Create_WhenFieldsContainExtraSpaces_ShouldTrimTextFieldsBeforeInsert()
        {
            // Arrange
            var model = CreateValidConferenceBindingModel();
            model.Title = "  Конференция с лишними пробелами  ";
            model.Description = "  Описание конференции  ";
            model.City = "  Ульяновск  ";
            model.Country = "  Россия  ";
            model.Organizer = "  УлГТУ  ";
            model.SubjectArea = "  Информационные системы  ";
            model.Url = "  https://example.com/trimmed  ";

            _conferenceStorageMock
                .Setup(x => x.Insert(It.IsAny<ConferenceBindingModel>()))
                .Returns((ConferenceBindingModel m) => ToConferenceViewModel(m, 30));

            // Act
            var result = _logic.Create(model);

            // Assert
            result.Should().BeTrue();

            _conferenceStorageMock.Verify(
                x => x.Insert(It.Is<ConferenceBindingModel>(m =>
                    m.Title == "Конференция с лишними пробелами" &&
                    m.Description == "Описание конференции" &&
                    m.City == "Ульяновск" &&
                    m.Country == "Россия" &&
                    m.Organizer == "УлГТУ" &&
                    m.SubjectArea == "Информационные системы" &&
                    m.Url == "https://example.com/trimmed")),
                Times.Once);
        }

        private static ConferenceBindingModel CreateValidConferenceBindingModel()
        {
            return new ConferenceBindingModel
            {
                Id = 0,
                Title = "Международная конференция по информационным системам",
                Description = "Научная конференция по информационным системам, анализу данных и программной инженерии",
                StartDate = new DateTime(2026, 6, 10),
                EndDate = new DateTime(2026, 6, 12),
                City = "Ульяновск",
                Country = "Россия",
                Organizer = "Ульяновский государственный технический университет",
                SubjectArea = "Информационные системы; Машинное обучение",
                Format = ConferenceFormat.Смешанная,
                Level = ConferenceLevel.Международная,
                Url = "https://example.com/conference"
            };
        }

        private static ConferenceViewModel CreateConferenceViewModel(
            int id,
            string title,
            string subjectArea,
            DateTime startDate,
            DateTime endDate,
            ConferenceFormat format,
            ConferenceLevel level)
        {
            return new ConferenceViewModel
            {
                Id = id,
                Title = title,
                Description = "Тестовое описание конференции",
                StartDate = startDate,
                EndDate = endDate,
                City = "Ульяновск",
                Country = "Россия",
                Organizer = "Тестовый организатор",
                SubjectArea = subjectArea,
                Format = format,
                Level = level,
                Url = $"https://example.com/conference-{id}"
            };
        }

        private static ConferenceViewModel ToConferenceViewModel(ConferenceBindingModel model, int id)
        {
            return new ConferenceViewModel
            {
                Id = id,
                Title = model.Title,
                Description = model.Description,
                StartDate = model.StartDate,
                EndDate = model.EndDate,
                City = model.City,
                Country = model.Country,
                Organizer = model.Organizer,
                SubjectArea = model.SubjectArea,
                Format = model.Format,
                Level = model.Level,
                Url = model.Url
            };
        }
    }
}
