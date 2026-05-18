using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using ScientificActivityBusinessLogics.BusinessLogics;
using ScientificActivityBusinessLogics.Services;
using ScientificActivityContracts.BindingModels;
using ScientificActivityContracts.SearchModels;
using ScientificActivityDatabaseImplement;
using ScientificActivityDatabaseImplement.Implements;
using ScientificActivityDataModels.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScientificActivityTests.Integration
{
    public class ResearcherIntegrationTests
    {
        [Fact]
        public void Create_WhenResearcherIsValid_ShouldSaveResearcherToDatabase()
        {
            // Arrange
            var options = CreateOptions();
            var storage = new ResearcherStorage(options);
            var logic = CreateLogic(storage);

            var model = CreateResearcherBindingModel(
                email: "researcher@example.com",
                lastName: "Гуськов",
                firstName: "Глеб",
                eLibraryAuthorId: "812005",
                researchTopics: "информационные системы; машинное обучение");

            // Act
            var createResult = logic.Create(model);

            var savedResearcher = logic.ReadElement(new ResearcherSearchModel
            {
                Email = "researcher@example.com"
            });

            // Assert
            createResult.Should().BeTrue();

            savedResearcher.Should().NotBeNull();
            savedResearcher!.Email.Should().Be("researcher@example.com");
            savedResearcher.LastName.Should().Be("Гуськов");
            savedResearcher.FirstName.Should().Be("Глеб");
            savedResearcher.ELibraryAuthorId.Should().Be("812005");
            savedResearcher.ResearchTopics.Should().Be("информационные системы; машинное обучение");
        }

        [Fact]
        public void Login_WhenEmailAndPasswordAreCorrect_ShouldReturnResearcher()
        {
            // Arrange
            var options = CreateOptions();
            var storage = new ResearcherStorage(options);
            var logic = CreateLogic(storage);

            logic.Create(CreateResearcherBindingModel(
                email: "login@example.com",
                lastName: "Иванов",
                firstName: "Иван",
                eLibraryAuthorId: "100001",
                researchTopics: "программная инженерия"));

            // Act
            var result = logic.Login("login@example.com", "password-hash");

            // Assert
            result.Should().NotBeNull();
            result!.Email.Should().Be("login@example.com");
            result.LastName.Should().Be("Иванов");
        }

        [Fact]
        public void Login_WhenPasswordIsIncorrect_ShouldReturnNull()
        {
            // Arrange
            var options = CreateOptions();
            var storage = new ResearcherStorage(options);
            var logic = CreateLogic(storage);

            logic.Create(CreateResearcherBindingModel(
                email: "login-error@example.com",
                lastName: "Петров",
                firstName: "Петр",
                eLibraryAuthorId: "100002",
                researchTopics: "анализ данных"));

            // Act
            var result = logic.Login("login-error@example.com", "wrong-password");

            // Assert
            result.Should().BeNull();
        }

        [Fact]
        public void ReadElement_WhenSearchById_ShouldReturnResearcher()
        {
            // Arrange
            var options = CreateOptions();
            var storage = new ResearcherStorage(options);
            var logic = CreateLogic(storage);

            logic.Create(CreateResearcherBindingModel(
                email: "by-id@example.com",
                lastName: "Сидоров",
                firstName: "Сидор",
                eLibraryAuthorId: "100003",
                researchTopics: "искусственный интеллект"));

            var savedResearcher = logic.ReadElement(new ResearcherSearchModel
            {
                Email = "by-id@example.com"
            });

            // Act
            var result = logic.ReadElement(new ResearcherSearchModel
            {
                Id = savedResearcher!.Id
            });

            // Assert
            result.Should().NotBeNull();
            result!.Id.Should().Be(savedResearcher.Id);
            result.Email.Should().Be("by-id@example.com");
        }

        [Fact]
        public void ReadElement_WhenSearchByELibraryAuthorId_ShouldReturnResearcher()
        {
            // Arrange
            var options = CreateOptions();
            var storage = new ResearcherStorage(options);
            var logic = CreateLogic(storage);

            logic.Create(CreateResearcherBindingModel(
                email: "elibrary@example.com",
                lastName: "Смирнов",
                firstName: "Алексей",
                eLibraryAuthorId: "812005",
                researchTopics: "информационные системы"));

            // Act
            var result = logic.ReadElement(new ResearcherSearchModel
            {
                ELibraryAuthorId = "812005"
            });

            // Assert
            result.Should().NotBeNull();
            result!.Email.Should().Be("elibrary@example.com");
            result.ELibraryAuthorId.Should().Be("812005");
        }

        [Fact]
        public void ReadList_WhenFilterByDepartment_ShouldReturnMatchingResearchers()
        {
            // Arrange
            var options = CreateOptions();
            var storage = new ResearcherStorage(options);
            var logic = CreateLogic(storage);

            logic.Create(CreateResearcherBindingModel(
                email: "department-1@example.com",
                lastName: "Гуськов",
                firstName: "Глеб",
                eLibraryAuthorId: "200001",
                researchTopics: "информационные системы",
                department: "Информационные системы"));

            logic.Create(CreateResearcherBindingModel(
                email: "department-2@example.com",
                lastName: "Иванов",
                firstName: "Иван",
                eLibraryAuthorId: "200002",
                researchTopics: "математика",
                department: "Высшая математика"));

            // Act
            var result = logic.ReadList(new ResearcherSearchModel
            {
                Department = "Информационные системы"
            });

            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(1);
            result![0].Email.Should().Be("department-1@example.com");
            result[0].Department.Should().Be("Информационные системы");
        }

        [Fact]
        public void ReadList_WhenFilterByIsActive_ShouldReturnOnlyActiveResearchers()
        {
            // Arrange
            var options = CreateOptions();
            var storage = new ResearcherStorage(options);
            var logic = CreateLogic(storage);

            logic.Create(CreateResearcherBindingModel(
                email: "active@example.com",
                lastName: "Активный",
                firstName: "Пользователь",
                eLibraryAuthorId: "300001",
                researchTopics: "информационные системы",
                isActive: true));

            logic.Create(CreateResearcherBindingModel(
                email: "inactive@example.com",
                lastName: "Неактивный",
                firstName: "Пользователь",
                eLibraryAuthorId: "300002",
                researchTopics: "информационные системы",
                isActive: false));

            // Act
            var result = logic.ReadList(new ResearcherSearchModel
            {
                IsActive = true
            });

            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(1);
            result![0].Email.Should().Be("active@example.com");
            result[0].IsActive.Should().BeTrue();
        }

        [Fact]
        public void Update_WhenResearcherExists_ShouldChangeResearcherDataInDatabase()
        {
            // Arrange
            var options = CreateOptions();
            var storage = new ResearcherStorage(options);
            var logic = CreateLogic(storage);

            logic.Create(CreateResearcherBindingModel(
                email: "update@example.com",
                lastName: "Стариков",
                firstName: "СтароеИмя",
                eLibraryAuthorId: "400001",
                researchTopics: "старые темы"));

            var savedResearcher = logic.ReadElement(new ResearcherSearchModel
            {
                Email = "update@example.com"
            });

            var updateModel = new ResearcherBindingModel
            {
                Id = savedResearcher!.Id,
                Email = "update@example.com",
                PasswordHash = "password-hash",
                Role = UserRole.Исследователь,
                IsActive = true,
                LastName = "Новиков",
                FirstName = "НовоеИмя",
                MiddleName = "Павлович",
                Phone = "+7 (900) 765-43-21",
                Department = "Прикладная математика и информатика",
                Position = "Старший преподаватель",
                AcademicDegree = AcademicDegree.Кандидат_наук,
                ELibraryAuthorId = "400001",
                ResearchTopics = "анализ данных; рекомендательные системы"
            };

            // Act
            var updateResult = logic.Update(updateModel);

            var updatedResearcher = logic.ReadElement(new ResearcherSearchModel
            {
                Email = "update@example.com"
            });

            // Assert
            updateResult.Should().BeTrue();

            updatedResearcher.Should().NotBeNull();
            updatedResearcher!.LastName.Should().Be("Новиков");
            updatedResearcher.FirstName.Should().Be("НовоеИмя");
            updatedResearcher.Phone.Should().Be("79007654321");
            updatedResearcher.Department.Should().Be("Прикладная математика и информатика");
            updatedResearcher.Position.Should().Be("Старший преподаватель");
            updatedResearcher.ResearchTopics.Should().Be("анализ данных; рекомендательные системы");
        }

        [Fact]
        public void Delete_WhenResearcherExists_ShouldRemoveResearcherFromDatabase()
        {
            // Arrange
            var options = CreateOptions();
            var storage = new ResearcherStorage(options);
            var logic = CreateLogic(storage);

            logic.Create(CreateResearcherBindingModel(
                email: "delete@example.com",
                lastName: "Удаляемый",
                firstName: "Пользователь",
                eLibraryAuthorId: "500001",
                researchTopics: "информационные системы"));

            var savedResearcher = logic.ReadElement(new ResearcherSearchModel
            {
                Email = "delete@example.com"
            });

            // Act
            var deleteResult = logic.Delete(new ResearcherBindingModel
            {
                Id = savedResearcher!.Id
            });

            var deletedResearcher = logic.ReadElement(new ResearcherSearchModel
            {
                Email = "delete@example.com"
            });

            // Assert
            deleteResult.Should().BeTrue();
            deletedResearcher.Should().BeNull();
        }

        [Fact]
        public void ReadList_WhenDatabaseDoesNotContainResearchers_ShouldReturnEmptyList()
        {
            // Arrange
            var options = CreateOptions();
            var storage = new ResearcherStorage(options);
            var logic = CreateLogic(storage);

            // Act
            var result = logic.ReadList(null);

            // Assert
            result.Should().NotBeNull();
            result.Should().BeEmpty();
        }

        private static DbContextOptions<ScientificActivityDatabase> CreateOptions()
        {
            return new DbContextOptionsBuilder<ScientificActivityDatabase>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;
        }

        private static ResearcherLogic CreateLogic(ResearcherStorage storage)
        {
            return new ResearcherLogic(
                NullLogger<ResearcherLogic>.Instance,
                storage,
                new PasswordHashService());
        }

        private static ResearcherBindingModel CreateResearcherBindingModel(
            string email,
            string lastName,
            string firstName,
            string eLibraryAuthorId,
            string researchTopics,
            string department = "Информационные системы",
            bool isActive = true)
        {
            return new ResearcherBindingModel
            {
                Id = 0,
                Email = email,
                PasswordHash = "password-hash",
                Role = UserRole.Исследователь,
                IsActive = isActive,
                LastName = lastName,
                FirstName = firstName,
                MiddleName = "Павлович",
                Phone = "+7 (900) 123-45-67",
                Department = department,
                Position = "Доцент",
                AcademicDegree = AcademicDegree.Кандидат_наук,
                ELibraryAuthorId = eLibraryAuthorId,
                ResearchTopics = researchTopics
            };
        }
    }
}
