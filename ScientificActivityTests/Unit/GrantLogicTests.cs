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
    public class GrantLogicTests
    {
        private readonly Mock<IGrantStorage> _grantStorageMock;
        private readonly GrantLogic _logic;

        public GrantLogicTests()
        {
            _grantStorageMock = new Mock<IGrantStorage>();

            _logic = new GrantLogic(
                NullLogger<GrantLogic>.Instance,
                _grantStorageMock.Object);
        }

        [Fact]
        public void Create_WhenGrantModelIsCorrect_ShouldInsertGrantAndReturnTrue()
        {
            // Arrange
            var model = CreateValidGrantBindingModel();

            _grantStorageMock
                .Setup(x => x.GetElement(It.Is<GrantSearchModel>(m => m.ContestNumber == "РНФ-2026-001")))
                .Returns((GrantViewModel?)null);

            _grantStorageMock
                .Setup(x => x.Insert(It.IsAny<GrantBindingModel>()))
                .Returns((GrantBindingModel m) => ToGrantViewModel(m, 1));

            // Act
            var result = _logic.Create(model);

            // Assert
            result.Should().BeTrue();

            _grantStorageMock.Verify(
                x => x.Insert(It.Is<GrantBindingModel>(m =>
                    m.ContestNumber == "РНФ-2026-001" &&
                    m.Title == "Конкурс на проведение фундаментальных научных исследований" &&
                    m.Organization == "Российский научный фонд" &&
                    m.Status == GrantStatus.Открыт &&
                    m.Url == "https://rscf.ru/contests/rnf-2026-001")),
                Times.Once);
        }

        [Fact]
        public void Update_WhenGrantModelIsCorrect_ShouldUpdateGrantAndReturnTrue()
        {
            // Arrange
            var model = CreateValidGrantBindingModel();
            model.Id = 10;
            model.Title = "Обновленный конкурс Российского научного фонда";
            model.Description = "Обновленное описание конкурса";
            model.Amount = 5000000;
            model.SubjectArea = "Информационные системы; Искусственный интеллект";

            _grantStorageMock
                .Setup(x => x.GetElement(It.Is<GrantSearchModel>(m => m.ContestNumber == "РНФ-2026-001")))
                .Returns(new GrantViewModel
                {
                    Id = 10,
                    ContestNumber = "РНФ-2026-001",
                    Title = "Старое название конкурса",
                    Organization = "Российский научный фонд",
                    StartDate = new DateTime(2026, 1, 1),
                    EndDate = new DateTime(2026, 3, 1),
                    Status = GrantStatus.Открыт
                });

            _grantStorageMock
                .Setup(x => x.Update(It.IsAny<GrantBindingModel>()))
                .Returns((GrantBindingModel m) => ToGrantViewModel(m, m.Id));

            // Act
            var result = _logic.Update(model);

            // Assert
            result.Should().BeTrue();

            _grantStorageMock.Verify(
                x => x.Update(It.Is<GrantBindingModel>(m =>
                    m.Id == 10 &&
                    m.Title == "Обновленный конкурс Российского научного фонда" &&
                    m.Description == "Обновленное описание конкурса" &&
                    m.Amount == 5000000 &&
                    m.SubjectArea == "Информационные системы; Искусственный интеллект")),
                Times.Once);

            _grantStorageMock.Verify(
                x => x.Insert(It.IsAny<GrantBindingModel>()),
                Times.Never);
        }

        [Fact]
        public void Create_WhenTitleIsEmpty_ShouldThrowArgumentNullExceptionAndNotInsert()
        {
            // Arrange
            var model = CreateValidGrantBindingModel();
            model.Title = "";

            // Act
            var action = () => _logic.Create(model);

            // Assert
            action.Should().Throw<ArgumentNullException>()
                .WithMessage("*Не указано название гранта*");

            _grantStorageMock.Verify(
                x => x.Insert(It.IsAny<GrantBindingModel>()),
                Times.Never);
        }

        [Fact]
        public void Create_WhenAnotherGrantWithSameContestNumberExists_ShouldThrowInvalidOperationExceptionAndNotInsert()
        {
            // Arrange
            var model = CreateValidGrantBindingModel();
            model.Id = 0;
            model.ContestNumber = "РНФ-2026-001";

            _grantStorageMock
                .Setup(x => x.GetElement(It.Is<GrantSearchModel>(m => m.ContestNumber == "РНФ-2026-001")))
                .Returns(new GrantViewModel
                {
                    Id = 99,
                    ContestNumber = "РНФ-2026-001",
                    Title = "Уже существующий конкурс",
                    Organization = "Российский научный фонд",
                    StartDate = new DateTime(2026, 1, 1),
                    EndDate = new DateTime(2026, 3, 1),
                    Status = GrantStatus.Открыт
                });

            // Act
            var action = () => _logic.Create(model);

            // Assert
            action.Should().Throw<InvalidOperationException>()
                .WithMessage("Грант с таким номером конкурса уже существует");

            _grantStorageMock.Verify(
                x => x.Insert(It.IsAny<GrantBindingModel>()),
                Times.Never);
        }

        [Fact]
        public void ReadList_WhenSearchByOpenStatus_ShouldReturnOnlyOpenGrants()
        {
            // Arrange
            var grants = new List<GrantViewModel>
            {
                CreateGrantViewModel(
                    id: 1,
                    contestNumber: "РНФ-2026-001",
                    title: "Открытый конкурс 1",
                    subjectArea: "Информационные системы",
                    status: GrantStatus.Открыт,
                    startDate: DateTime.Today.AddDays(-5),
                    endDate: DateTime.Today.AddDays(30)),
                CreateGrantViewModel(
                    id: 2,
                    contestNumber: "РНФ-2026-002",
                    title: "Открытый конкурс 2",
                    subjectArea: "Машинное обучение",
                    status: GrantStatus.Открыт,
                    startDate: DateTime.Today.AddDays(-10),
                    endDate: DateTime.Today.AddDays(20))
            };

            _grantStorageMock
                .Setup(x => x.GetFilteredList(It.Is<GrantSearchModel>(m => m.Status == GrantStatus.Открыт)))
                .Returns(grants);

            // Act
            var result = _logic.ReadList(new GrantSearchModel
            {
                Status = GrantStatus.Открыт
            });

            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(2);
            result.Should().OnlyContain(x => x.Status == GrantStatus.Открыт);

            _grantStorageMock.Verify(
                x => x.GetFilteredList(It.Is<GrantSearchModel>(m => m.Status == GrantStatus.Открыт)),
                Times.Once);
        }

        [Fact]
        public void ReadList_WhenSearchActualOpenGrants_ShouldNotReturnClosedGrants()
        {
            // Arrange
            var today = DateTime.Today;

            var actualOpenGrants = new List<GrantViewModel>
            {
                CreateGrantViewModel(
                    id: 1,
                    contestNumber: "РНФ-2026-001",
                    title: "Актуальный открытый грант",
                    subjectArea: "Информационные системы",
                    status: GrantStatus.Открыт,
                    startDate: today.AddDays(-5),
                    endDate: today.AddDays(20))
            };

            _grantStorageMock
                .Setup(x => x.GetFilteredList(It.Is<GrantSearchModel>(m =>
                    m.Status == GrantStatus.Открыт &&
                    m.DateFrom.HasValue &&
                    m.DateFrom.Value.Date == today)))
                .Returns(actualOpenGrants);

            // Act
            var result = _logic.ReadList(new GrantSearchModel
            {
                Status = GrantStatus.Открыт,
                DateFrom = today
            });

            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(1);
            result.Should().OnlyContain(x =>
                x.Status == GrantStatus.Открыт &&
                x.EndDate.Date >= today);

            result.Should().NotContain(x => x.Status == GrantStatus.Закрыт);
        }

        [Fact]
        public void ReadList_WhenSearchByDeadline_ShouldReturnGrantsWithSuitableEndDate()
        {
            // Arrange
            var today = DateTime.Today;
            var dateTo = today.AddDays(30);

            var grants = new List<GrantViewModel>
            {
                CreateGrantViewModel(
                    id: 1,
                    contestNumber: "РНФ-2026-001",
                    title: "Грант со сроком в течение месяца",
                    subjectArea: "Искусственный интеллект",
                    status: GrantStatus.Открыт,
                    startDate: today.AddDays(-10),
                    endDate: today.AddDays(15)),
                CreateGrantViewModel(
                    id: 2,
                    contestNumber: "РНФ-2026-002",
                    title: "Грант со сроком через месяц",
                    subjectArea: "Программная инженерия",
                    status: GrantStatus.Открыт,
                    startDate: today.AddDays(-3),
                    endDate: today.AddDays(30))
            };

            _grantStorageMock
                .Setup(x => x.GetFilteredList(It.Is<GrantSearchModel>(m =>
                    m.DateFrom.HasValue &&
                    m.DateTo.HasValue &&
                    m.DateFrom.Value.Date == today &&
                    m.DateTo.Value.Date == dateTo)))
                .Returns(grants);

            // Act
            var result = _logic.ReadList(new GrantSearchModel
            {
                DateFrom = today,
                DateTo = dateTo
            });

            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(2);
            result.Should().OnlyContain(x =>
                x.EndDate.Date >= today &&
                x.EndDate.Date <= dateTo);

            _grantStorageMock.Verify(
                x => x.GetFilteredList(It.Is<GrantSearchModel>(m =>
                    m.DateFrom.HasValue &&
                    m.DateTo.HasValue)),
                Times.Once);
        }

        [Fact]
        public void Create_WhenEndDateEarlierThanStartDate_ShouldThrowArgumentExceptionAndNotInsert()
        {
            // Arrange
            var model = CreateValidGrantBindingModel();
            model.StartDate = new DateTime(2026, 5, 10);
            model.EndDate = new DateTime(2026, 5, 1);

            // Act
            var action = () => _logic.Create(model);

            // Assert
            action.Should().Throw<ArgumentException>()
                .WithMessage("Дата окончания не может быть раньше даты начала");

            _grantStorageMock.Verify(
                x => x.Insert(It.IsAny<GrantBindingModel>()),
                Times.Never);
        }

        [Fact]
        public void Create_WhenTitleContainsScientificKeywords_ShouldSaveSubjectAreaForRecommendations()
        {
            // Arrange
            var model = CreateValidGrantBindingModel();
            model.Title = "Конкурс проектов по искусственному интеллекту и машинному обучению";
            model.SubjectArea = "искусственный интеллект; машинное обучение; анализ данных";

            _grantStorageMock
                .Setup(x => x.GetElement(It.Is<GrantSearchModel>(m => m.ContestNumber == "РНФ-2026-001")))
                .Returns((GrantViewModel?)null);

            _grantStorageMock
                .Setup(x => x.Insert(It.IsAny<GrantBindingModel>()))
                .Returns((GrantBindingModel m) => ToGrantViewModel(m, 9));

            // Act
            var result = _logic.Create(model);

            // Assert
            result.Should().BeTrue();

            _grantStorageMock.Verify(
                x => x.Insert(It.Is<GrantBindingModel>(m =>
                    m.Title == "Конкурс проектов по искусственному интеллекту и машинному обучению" &&
                    m.SubjectArea == "искусственный интеллект; машинное обучение; анализ данных")),
                Times.Once);
        }

        [Fact]
        public void ReadList_WhenStorageReturnsEmptyList_ShouldReturnEmptyList()
        {
            // Arrange
            _grantStorageMock
                .Setup(x => x.GetFullList())
                .Returns([]);

            // Act
            var result = _logic.ReadList(null);

            // Assert
            result.Should().NotBeNull();
            result.Should().BeEmpty();

            _grantStorageMock.Verify(
                x => x.GetFullList(),
                Times.Once);
        }

        [Fact]
        public void Create_WhenOrganizationIsEmpty_ShouldThrowArgumentNullExceptionAndNotInsert()
        {
            // Arrange
            var model = CreateValidGrantBindingModel();
            model.Organization = "";

            // Act
            var action = () => _logic.Create(model);

            // Assert
            action.Should().Throw<ArgumentNullException>()
                .WithMessage("*Не указана организация*");

            _grantStorageMock.Verify(
                x => x.Insert(It.IsAny<GrantBindingModel>()),
                Times.Never);
        }

        [Fact]
        public void Create_WhenContestNumberIsEmpty_ShouldThrowArgumentNullExceptionAndNotInsert()
        {
            // Arrange
            var model = CreateValidGrantBindingModel();
            model.ContestNumber = "";

            // Act
            var action = () => _logic.Create(model);

            // Assert
            action.Should().Throw<ArgumentNullException>()
                .WithMessage("*Не указан номер конкурса*");

            _grantStorageMock.Verify(
                x => x.Insert(It.IsAny<GrantBindingModel>()),
                Times.Never);
        }

        [Fact]
        public void Create_WhenAmountIsNegative_ShouldThrowArgumentExceptionAndNotInsert()
        {
            // Arrange
            var model = CreateValidGrantBindingModel();
            model.Amount = -1000;

            // Act
            var action = () => _logic.Create(model);

            // Assert
            action.Should().Throw<ArgumentException>()
                .WithMessage("*Сумма гранта не может быть отрицательной*");

            _grantStorageMock.Verify(
                x => x.Insert(It.IsAny<GrantBindingModel>()),
                Times.Never);
        }

        [Fact]
        public void Create_WhenFieldsContainExtraSpaces_ShouldTrimTextFieldsBeforeInsert()
        {
            // Arrange
            var model = CreateValidGrantBindingModel();
            model.Title = "  Конкурс с лишними пробелами  ";
            model.Description = "  Описание конкурса  ";
            model.Organization = "  Российский научный фонд  ";
            model.Currency = "  руб.  ";
            model.SubjectArea = "  Информационные системы; Машинное обучение  ";
            model.Url = "  https://rscf.ru/contests/trimmed  ";

            _grantStorageMock
                .Setup(x => x.GetElement(It.Is<GrantSearchModel>(m => m.ContestNumber == "РНФ-2026-001")))
                .Returns((GrantViewModel?)null);

            _grantStorageMock
                .Setup(x => x.Insert(It.IsAny<GrantBindingModel>()))
                .Returns((GrantBindingModel m) => ToGrantViewModel(m, 11));

            // Act
            var result = _logic.Create(model);

            // Assert
            result.Should().BeTrue();

            _grantStorageMock.Verify(
                x => x.Insert(It.Is<GrantBindingModel>(m =>
                    m.Title == "Конкурс с лишними пробелами" &&
                    m.Description == "Описание конкурса" &&
                    m.Organization == "Российский научный фонд" &&
                    m.Currency == "руб." &&
                    m.SubjectArea == "Информационные системы; Машинное обучение" &&
                    m.Url == "https://rscf.ru/contests/trimmed")),
                Times.Once);
        }

        private static GrantBindingModel CreateValidGrantBindingModel()
        {
            return new GrantBindingModel
            {
                Id = 0,
                ContestNumber = "РНФ-2026-001",
                Title = "Конкурс на проведение фундаментальных научных исследований",
                Description = "Конкурс Российского научного фонда для поддержки научных исследований",
                Organization = "Российский научный фонд",
                StartDate = new DateTime(2026, 1, 15),
                EndDate = new DateTime(2026, 3, 31),
                Amount = 3000000,
                Currency = "руб.",
                SubjectArea = "Информационные системы; Машинное обучение",
                Status = GrantStatus.Открыт,
                Url = "https://rscf.ru/contests/rnf-2026-001"
            };
        }

        private static GrantViewModel CreateGrantViewModel(
            int id,
            string contestNumber,
            string title,
            string subjectArea,
            GrantStatus status,
            DateTime startDate,
            DateTime endDate)
        {
            return new GrantViewModel
            {
                Id = id,
                ContestNumber = contestNumber,
                Title = title,
                Description = "Тестовое описание грантового конкурса",
                Organization = "Российский научный фонд",
                StartDate = startDate,
                EndDate = endDate,
                Amount = 3000000,
                Currency = "руб.",
                SubjectArea = subjectArea,
                Status = status,
                Url = $"https://rscf.ru/contests/{contestNumber}"
            };
        }

        private static GrantViewModel ToGrantViewModel(GrantBindingModel model, int id)
        {
            return new GrantViewModel
            {
                Id = id,
                ContestNumber = model.ContestNumber,
                Title = model.Title,
                Description = model.Description,
                Organization = model.Organization,
                StartDate = model.StartDate,
                EndDate = model.EndDate,
                Amount = model.Amount,
                Currency = model.Currency,
                SubjectArea = model.SubjectArea,
                Status = model.Status,
                Url = model.Url
            };
        }
    }
}
