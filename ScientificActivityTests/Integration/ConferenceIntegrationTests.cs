using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using ScientificActivityBusinessLogics.BusinessLogics;
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
    public class ConferenceIntegrationTests
    {
        [Fact]
        public void Create_WhenConferenceIsValid_ShouldSaveConferenceToDatabase()
        {
            // Arrange
            var options = CreateOptions();
            var storage = new ConferenceStorage(options);
            var logic = new ConferenceLogic(NullLogger<ConferenceLogic>.Instance, storage);

            var model = CreateConferenceBindingModel(
                title: "Конференция интеграционного тестирования",
                subjectArea: "Информационные системы",
                startDate: new DateTime(2026, 6, 10),
                endDate: new DateTime(2026, 6, 12),
                format: ConferenceFormat.Смешанная,
                level: ConferenceLevel.Международная);

            // Act
            var createResult = logic.Create(model);

            var savedConference = logic.ReadElement(new ConferenceSearchModel
            {
                Title = "Конференция интеграционного тестирования"
            });

            // Assert
            createResult.Should().BeTrue();

            savedConference.Should().NotBeNull();
            savedConference!.Title.Should().Be("Конференция интеграционного тестирования");
            savedConference.SubjectArea.Should().Be("Информационные системы");
            savedConference.StartDate.Should().Be(new DateTime(2026, 6, 10));
            savedConference.EndDate.Should().Be(new DateTime(2026, 6, 12));
            savedConference.Format.Should().Be(ConferenceFormat.Смешанная);
            savedConference.Level.Should().Be(ConferenceLevel.Международная);
        }

        [Fact]
        public void ReadList_WhenFilterByTitle_ShouldReturnMatchingConference()
        {
            // Arrange
            var options = CreateOptions();
            var storage = new ConferenceStorage(options);
            var logic = new ConferenceLogic(NullLogger<ConferenceLogic>.Instance, storage);

            logic.Create(CreateConferenceBindingModel(
                title: "Конференция по информационным системам",
                subjectArea: "Информационные системы",
                startDate: DateTime.Today.AddDays(10),
                endDate: DateTime.Today.AddDays(12),
                format: ConferenceFormat.Смешанная,
                level: ConferenceLevel.Международная));

            logic.Create(CreateConferenceBindingModel(
                title: "Конференция по химии",
                subjectArea: "Химия",
                startDate: DateTime.Today.AddDays(20),
                endDate: DateTime.Today.AddDays(21),
                format: ConferenceFormat.Очная,
                level: ConferenceLevel.Всероссийская));

            // Act
            var result = logic.ReadList(new ConferenceSearchModel
            {
                Title = "информационным системам"
            });

            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(1);
            result![0].Title.Should().Be("Конференция по информационным системам");
        }

        [Fact]
        public void ReadList_WhenFilterBySubjectArea_ShouldReturnMatchingConferences()
        {
            // Arrange
            var options = CreateOptions();
            var storage = new ConferenceStorage(options);
            var logic = new ConferenceLogic(NullLogger<ConferenceLogic>.Instance, storage);

            logic.Create(CreateConferenceBindingModel(
                title: "Конференция по машинному обучению",
                subjectArea: "Машинное обучение; Искусственный интеллект",
                startDate: DateTime.Today.AddDays(10),
                endDate: DateTime.Today.AddDays(12),
                format: ConferenceFormat.Онлайн,
                level: ConferenceLevel.Международная));

            logic.Create(CreateConferenceBindingModel(
                title: "Конференция по экономике",
                subjectArea: "Экономика",
                startDate: DateTime.Today.AddDays(15),
                endDate: DateTime.Today.AddDays(16),
                format: ConferenceFormat.Очная,
                level: ConferenceLevel.Всероссийская));

            // Act
            var result = logic.ReadList(new ConferenceSearchModel
            {
                SubjectArea = "Машинное обучение"
            });

            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(1);
            result![0].Title.Should().Be("Конференция по машинному обучению");
            result[0].SubjectArea.Should().Contain("Машинное обучение");
        }

        [Fact]
        public void ReadList_WhenFilterByFormat_ShouldReturnOnlyConferencesWithSpecifiedFormat()
        {
            // Arrange
            var options = CreateOptions();
            var storage = new ConferenceStorage(options);
            var logic = new ConferenceLogic(NullLogger<ConferenceLogic>.Instance, storage);

            logic.Create(CreateConferenceBindingModel(
                title: "Онлайн-конференция",
                subjectArea: "Информационные системы",
                startDate: DateTime.Today.AddDays(10),
                endDate: DateTime.Today.AddDays(11),
                format: ConferenceFormat.Онлайн,
                level: ConferenceLevel.Всероссийская));

            logic.Create(CreateConferenceBindingModel(
                title: "Очная конференция",
                subjectArea: "Информационные системы",
                startDate: DateTime.Today.AddDays(20),
                endDate: DateTime.Today.AddDays(21),
                format: ConferenceFormat.Очная,
                level: ConferenceLevel.Всероссийская));

            // Act
            var result = logic.ReadList(new ConferenceSearchModel
            {
                Format = ConferenceFormat.Онлайн
            });

            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(1);
            result![0].Title.Should().Be("Онлайн-конференция");
            result[0].Format.Should().Be(ConferenceFormat.Онлайн);
        }

        [Fact]
        public void ReadList_WhenFilterByLevel_ShouldReturnOnlyConferencesWithSpecifiedLevel()
        {
            // Arrange
            var options = CreateOptions();
            var storage = new ConferenceStorage(options);
            var logic = new ConferenceLogic(NullLogger<ConferenceLogic>.Instance, storage);

            logic.Create(CreateConferenceBindingModel(
                title: "Международная конференция",
                subjectArea: "Программная инженерия",
                startDate: DateTime.Today.AddDays(10),
                endDate: DateTime.Today.AddDays(11),
                format: ConferenceFormat.Смешанная,
                level: ConferenceLevel.Международная));

            logic.Create(CreateConferenceBindingModel(
                title: "Всероссийская конференция",
                subjectArea: "Программная инженерия",
                startDate: DateTime.Today.AddDays(20),
                endDate: DateTime.Today.AddDays(21),
                format: ConferenceFormat.Смешанная,
                level: ConferenceLevel.Всероссийская));

            // Act
            var result = logic.ReadList(new ConferenceSearchModel
            {
                Level = ConferenceLevel.Международная
            });

            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(1);
            result![0].Title.Should().Be("Международная конференция");
            result[0].Level.Should().Be(ConferenceLevel.Международная);
        }

        [Fact]
        public void ReadList_WhenFilterByDateRange_ShouldReturnConferencesInsideDateRange()
        {
            // Arrange
            var options = CreateOptions();
            var storage = new ConferenceStorage(options);
            var logic = new ConferenceLogic(NullLogger<ConferenceLogic>.Instance, storage);

            logic.Create(CreateConferenceBindingModel(
                title: "Конференция в июне",
                subjectArea: "Информационные системы",
                startDate: new DateTime(2026, 6, 10),
                endDate: new DateTime(2026, 6, 12),
                format: ConferenceFormat.Смешанная,
                level: ConferenceLevel.Международная));

            logic.Create(CreateConferenceBindingModel(
                title: "Конференция в августе",
                subjectArea: "Информационные системы",
                startDate: new DateTime(2026, 8, 10),
                endDate: new DateTime(2026, 8, 12),
                format: ConferenceFormat.Смешанная,
                level: ConferenceLevel.Международная));

            // Act
            var result = logic.ReadList(new ConferenceSearchModel
            {
                DateFrom = new DateTime(2026, 6, 1),
                DateTo = new DateTime(2026, 6, 30)
            });

            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(1);
            result![0].Title.Should().Be("Конференция в июне");
            result[0].StartDate.Should().BeOnOrAfter(new DateTime(2026, 6, 1));
            result[0].EndDate.Should().BeOnOrBefore(new DateTime(2026, 6, 30));
        }

        [Fact]
        public void Update_WhenConferenceExists_ShouldChangeConferenceDataInDatabase()
        {
            // Arrange
            var options = CreateOptions();
            var storage = new ConferenceStorage(options);
            var logic = new ConferenceLogic(NullLogger<ConferenceLogic>.Instance, storage);

            logic.Create(CreateConferenceBindingModel(
                title: "Старое название конференции",
                subjectArea: "Информационные системы",
                startDate: new DateTime(2026, 6, 10),
                endDate: new DateTime(2026, 6, 12),
                format: ConferenceFormat.Очная,
                level: ConferenceLevel.Всероссийская));

            var savedConference = logic.ReadElement(new ConferenceSearchModel
            {
                Title = "Старое название конференции"
            });

            var updateModel = new ConferenceBindingModel
            {
                Id = savedConference!.Id,
                Title = "Новое название конференции",
                Description = "Обновленное описание",
                StartDate = new DateTime(2026, 7, 10),
                EndDate = new DateTime(2026, 7, 12),
                City = "Москва",
                Country = "Россия",
                Organizer = "Обновленный организатор",
                SubjectArea = "Искусственный интеллект; Анализ данных",
                Format = ConferenceFormat.Онлайн,
                Level = ConferenceLevel.Международная,
                Url = "https://example.com/updated-conference"
            };

            // Act
            var updateResult = logic.Update(updateModel);

            var updatedConference = logic.ReadElement(new ConferenceSearchModel
            {
                Title = "Новое название конференции"
            });

            // Assert
            updateResult.Should().BeTrue();

            updatedConference.Should().NotBeNull();
            updatedConference!.Title.Should().Be("Новое название конференции");
            updatedConference.Description.Should().Be("Обновленное описание");
            updatedConference.StartDate.Should().Be(new DateTime(2026, 7, 10));
            updatedConference.EndDate.Should().Be(new DateTime(2026, 7, 12));
            updatedConference.City.Should().Be("Москва");
            updatedConference.Format.Should().Be(ConferenceFormat.Онлайн);
            updatedConference.Level.Should().Be(ConferenceLevel.Международная);
            updatedConference.SubjectArea.Should().Be("Искусственный интеллект; Анализ данных");
        }

        [Fact]
        public void Delete_WhenConferenceExists_ShouldRemoveConferenceFromDatabase()
        {
            // Arrange
            var options = CreateOptions();
            var storage = new ConferenceStorage(options);
            var logic = new ConferenceLogic(NullLogger<ConferenceLogic>.Instance, storage);

            logic.Create(CreateConferenceBindingModel(
                title: "Конференция для удаления",
                subjectArea: "Информационные системы",
                startDate: DateTime.Today.AddDays(10),
                endDate: DateTime.Today.AddDays(12),
                format: ConferenceFormat.Смешанная,
                level: ConferenceLevel.Всероссийская));

            var savedConference = logic.ReadElement(new ConferenceSearchModel
            {
                Title = "Конференция для удаления"
            });

            // Act
            var deleteResult = logic.Delete(new ConferenceBindingModel
            {
                Id = savedConference!.Id
            });

            var deletedConference = logic.ReadElement(new ConferenceSearchModel
            {
                Title = "Конференция для удаления"
            });

            // Assert
            deleteResult.Should().BeTrue();
            deletedConference.Should().BeNull();
        }

        [Fact]
        public void ReadList_WhenDatabaseDoesNotContainConferences_ShouldReturnEmptyList()
        {
            // Arrange
            var options = CreateOptions();
            var storage = new ConferenceStorage(options);
            var logic = new ConferenceLogic(NullLogger<ConferenceLogic>.Instance, storage);

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

        private static ConferenceBindingModel CreateConferenceBindingModel(
            string title,
            string subjectArea,
            DateTime startDate,
            DateTime endDate,
            ConferenceFormat format,
            ConferenceLevel level)
        {
            return new ConferenceBindingModel
            {
                Id = 0,
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
                Url = $"https://example.com/{title.Replace(" ", "-").ToLowerInvariant()}"
            };
        }
    }
}
