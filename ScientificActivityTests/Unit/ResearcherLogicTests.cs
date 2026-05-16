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
    public class ResearcherLogicTests
    {
        private readonly Mock<IResearcherStorage> _researcherStorageMock;
        private readonly ResearcherLogic _logic;

        public ResearcherLogicTests()
        {
            _researcherStorageMock = new Mock<IResearcherStorage>();

            _logic = new ResearcherLogic(
                NullLogger<ResearcherLogic>.Instance,
                _researcherStorageMock.Object);
        }

        [Fact]
        public void Create_WhenResearcherModelIsCorrect_ShouldInsertResearcherAndReturnTrue()
        {
            // Arrange
            var model = CreateValidResearcherBindingModel();

            _researcherStorageMock
                .Setup(x => x.GetElement(It.Is<ResearcherSearchModel>(m => m.Email == model.Email.Trim())))
                .Returns((ResearcherViewModel?)null);

            _researcherStorageMock
                .Setup(x => x.Insert(It.IsAny<ResearcherBindingModel>()))
                .Returns((ResearcherBindingModel m) => ToViewModel(m, 1));

            // Act
            var result = _logic.Create(model);

            // Assert
            result.Should().BeTrue();

            _researcherStorageMock.Verify(
                x => x.Insert(It.Is<ResearcherBindingModel>(m =>
                    m.Email == "researcher@example.com" &&
                    m.LastName == "Табеев" &&
                    m.FirstName == "Александр" &&
                    m.Phone == "79001234567" &&
                    m.Department == "Информационные системы" &&
                    m.Position == "Ассистент" &&
                    m.ResearchTopics == "информационные системы; машинное обучение")),
                Times.Once);
        }

        [Fact]
        public void Create_WhenEmailAlreadyExists_ShouldThrowInvalidOperationException()
        {
            // Arrange
            var model = CreateValidResearcherBindingModel();

            _researcherStorageMock
                .Setup(x => x.GetElement(It.Is<ResearcherSearchModel>(m => m.Email == model.Email)))
                .Returns(new ResearcherViewModel
                {
                    Id = 2,
                    Email = model.Email,
                    LastName = "Иванов",
                    FirstName = "Иван",
                    Phone = "79000000000",
                    Department = "Информационные системы",
                    Position = "Доцент",
                    IsActive = true
                });

            // Act
            var action = () => _logic.Create(model);

            // Assert
            action.Should().Throw<InvalidOperationException>()
                .WithMessage("Исследователь с таким email уже существует");

            _researcherStorageMock.Verify(
                x => x.Insert(It.IsAny<ResearcherBindingModel>()),
                Times.Never);
        }

        [Fact]
        public void Create_WhenLastNameIsEmpty_ShouldThrowArgumentNullException()
        {
            // Arrange
            var model = CreateValidResearcherBindingModel();
            model.LastName = "";

            // Act
            var action = () => _logic.Create(model);

            // Assert
            action.Should().Throw<ArgumentNullException>()
                .WithMessage("*Не указана фамилия исследователя*");

            _researcherStorageMock.Verify(
                x => x.Insert(It.IsAny<ResearcherBindingModel>()),
                Times.Never);
        }

        [Fact]
        public void Create_WhenEmailIsEmpty_ShouldThrowArgumentNullException()
        {
            // Arrange
            var model = CreateValidResearcherBindingModel();
            model.Email = "";

            // Act
            var action = () => _logic.Create(model);

            // Assert
            action.Should().Throw<ArgumentNullException>()
                .WithMessage("*Не указан email исследователя*");

            _researcherStorageMock.Verify(
                x => x.Insert(It.IsAny<ResearcherBindingModel>()),
                Times.Never);
        }

        [Fact]
        public void ReadElement_WhenResearcherExists_ShouldReturnResearcher()
        {
            // Arrange
            var expectedResearcher = new ResearcherViewModel
            {
                Id = 1,
                Email = "researcher@example.com",
                LastName = "Табеев",
                FirstName = "Александр",
                MiddleName = "Павлович",
                Phone = "79001234567",
                Department = "Информационные системы",
                Position = "Ассистент",
                AcademicDegree = AcademicDegree.Не_указано,
                IsActive = true,
                ELibraryAuthorId = "812005",
                ResearchTopics = "информационные системы; машинное обучение"
            };

            _researcherStorageMock
                .Setup(x => x.GetElement(It.Is<ResearcherSearchModel>(m => m.Id == 1)))
                .Returns(expectedResearcher);

            // Act
            var result = _logic.ReadElement(new ResearcherSearchModel
            {
                Id = 1
            });

            // Assert
            result.Should().NotBeNull();
            result!.Id.Should().Be(1);
            result.Email.Should().Be("researcher@example.com");
            result.FullName.Should().Be("Табеев Александр Павлович");
            result.ELibraryAuthorId.Should().Be("812005");

            _researcherStorageMock.Verify(
                x => x.GetElement(It.Is<ResearcherSearchModel>(m => m.Id == 1)),
                Times.Once);
        }

        [Fact]
        public void ReadElement_WhenResearcherDoesNotExist_ShouldReturnNull()
        {
            // Arrange
            _researcherStorageMock
                .Setup(x => x.GetElement(It.Is<ResearcherSearchModel>(m => m.Id == 999)))
                .Returns((ResearcherViewModel?)null);

            // Act
            var result = _logic.ReadElement(new ResearcherSearchModel
            {
                Id = 999
            });

            // Assert
            result.Should().BeNull();

            _researcherStorageMock.Verify(
                x => x.GetElement(It.Is<ResearcherSearchModel>(m => m.Id == 999)),
                Times.Once);
        }

        [Fact]
        public void Update_WhenResearcherModelIsCorrect_ShouldUpdateProfileAndReturnTrue()
        {
            // Arrange
            var model = CreateValidResearcherBindingModel();
            model.Id = 1;
            model.Department = "Прикладная математика и информатика";
            model.Position = "Старший преподаватель";
            model.Phone = "+7 (900) 765-43-21";
            model.ResearchTopics = "анализ данных; рекомендательные системы";

            _researcherStorageMock
                .Setup(x => x.GetElement(It.Is<ResearcherSearchModel>(m => m.Email == model.Email)))
                .Returns(new ResearcherViewModel
                {
                    Id = 1,
                    Email = model.Email,
                    LastName = "Табеев",
                    FirstName = "Александр",
                    Phone = "79001234567",
                    Department = "Информационные системы",
                    Position = "Ассистент",
                    IsActive = true
                });

            _researcherStorageMock
                .Setup(x => x.Update(It.IsAny<ResearcherBindingModel>()))
                .Returns((ResearcherBindingModel m) => ToViewModel(m, m.Id));

            // Act
            var result = _logic.Update(model);

            // Assert
            result.Should().BeTrue();

            _researcherStorageMock.Verify(
                x => x.Update(It.Is<ResearcherBindingModel>(m =>
                    m.Id == 1 &&
                    m.Department == "Прикладная математика и информатика" &&
                    m.Position == "Старший преподаватель" &&
                    m.Phone == "79007654321" &&
                    m.ResearchTopics == "анализ данных; рекомендательные системы")),
                Times.Once);
        }

        [Fact]
        public void Update_WhenResearchTopicsIsEmpty_ShouldSaveProfileWithNullResearchTopics()
        {
            // Arrange
            var model = CreateValidResearcherBindingModel();
            model.Id = 1;
            model.ResearchTopics = "   ";

            _researcherStorageMock
                .Setup(x => x.GetElement(It.Is<ResearcherSearchModel>(m => m.Email == model.Email)))
                .Returns(new ResearcherViewModel
                {
                    Id = 1,
                    Email = model.Email,
                    LastName = model.LastName,
                    FirstName = model.FirstName,
                    Phone = model.Phone,
                    Department = model.Department,
                    Position = model.Position,
                    IsActive = true
                });

            _researcherStorageMock
                .Setup(x => x.Update(It.IsAny<ResearcherBindingModel>()))
                .Returns((ResearcherBindingModel m) => ToViewModel(m, m.Id));

            // Act
            var result = _logic.Update(model);

            // Assert
            result.Should().BeTrue();

            _researcherStorageMock.Verify(
                x => x.Update(It.Is<ResearcherBindingModel>(m =>
                    m.Id == 1 &&
                    m.ResearchTopics == null)),
                Times.Once);
        }

        [Fact]
        public void Update_WhenELibraryAuthorIdIsSpecified_ShouldSaveELibraryAuthorId()
        {
            // Arrange
            var model = CreateValidResearcherBindingModel();
            model.Id = 1;
            model.ELibraryAuthorId = " 812005 ";

            _researcherStorageMock
                .Setup(x => x.GetElement(It.Is<ResearcherSearchModel>(m => m.Email == model.Email)))
                .Returns(new ResearcherViewModel
                {
                    Id = 1,
                    Email = model.Email,
                    LastName = model.LastName,
                    FirstName = model.FirstName,
                    Phone = model.Phone,
                    Department = model.Department,
                    Position = model.Position,
                    IsActive = true
                });

            _researcherStorageMock
                .Setup(x => x.GetElement(It.Is<ResearcherSearchModel>(m => m.ELibraryAuthorId == "812005")))
                .Returns((ResearcherViewModel?)null);

            _researcherStorageMock
                .Setup(x => x.Update(It.IsAny<ResearcherBindingModel>()))
                .Returns((ResearcherBindingModel m) => ToViewModel(m, m.Id));

            // Act
            var result = _logic.Update(model);

            // Assert
            result.Should().BeTrue();

            _researcherStorageMock.Verify(
                x => x.Update(It.Is<ResearcherBindingModel>(m =>
                    m.Id == 1 &&
                    m.ELibraryAuthorId == "812005")),
                Times.Once);
        }

        [Fact]
        public void ReadElement_WhenSearchByELibraryAuthorId_ShouldReturnResearcher()
        {
            // Arrange
            var expectedResearcher = new ResearcherViewModel
            {
                Id = 1,
                Email = "researcher@example.com",
                LastName = "Табеев",
                FirstName = "Александр",
                MiddleName = "Павлович",
                Phone = "79001234567",
                Department = "Информационные системы",
                Position = "Ассистент",
                AcademicDegree = AcademicDegree.Не_указано,
                IsActive = true,
                ELibraryAuthorId = "812005",
                ResearchTopics = "информационные системы; машинное обучение"
            };

            _researcherStorageMock
                .Setup(x => x.GetElement(It.Is<ResearcherSearchModel>(m => m.ELibraryAuthorId == "812005")))
                .Returns(expectedResearcher);

            // Act
            var result = _logic.ReadElement(new ResearcherSearchModel
            {
                ELibraryAuthorId = "812005"
            });

            // Assert
            result.Should().NotBeNull();
            result!.Id.Should().Be(1);
            result.ELibraryAuthorId.Should().Be("812005");
            result.FullName.Should().Be("Табеев Александр Павлович");

            _researcherStorageMock.Verify(
                x => x.GetElement(It.Is<ResearcherSearchModel>(m => m.ELibraryAuthorId == "812005")),
                Times.Once);
        }

        private static ResearcherBindingModel CreateValidResearcherBindingModel()
        {
            return new ResearcherBindingModel
            {
                Id = 0,
                Email = "researcher@example.com",
                PasswordHash = "password-hash",
                Role = UserRole.Исследователь,
                IsActive = true,
                LastName = "Табеев",
                FirstName = "Александр",
                MiddleName = "Павлович",
                Phone = "+7 (900) 123-45-67",
                Department = "Информационные системы",
                Position = "Ассистент",
                AcademicDegree = AcademicDegree.Не_указано,
                ELibraryAuthorId = null,
                ResearchTopics = "информационные системы; машинное обучение"
            };
        }

        private static ResearcherViewModel ToViewModel(ResearcherBindingModel model, int id)
        {
            return new ResearcherViewModel
            {
                Id = id,
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
    }
}
