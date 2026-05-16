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
    public class GrantIntegrationTests
    {
        [Fact]
        public void Create_WhenGrantIsValid_ShouldSaveGrantToDatabase()
        {
            // Arrange
            var options = CreateOptions();
            var storage = new GrantStorage(options);
            var logic = new GrantLogic(NullLogger<GrantLogic>.Instance, storage);

            var model = CreateGrantBindingModel(
                contestNumber: "РНФ-2026-001",
                title: "Грант интеграционного тестирования",
                subjectArea: "Информационные системы",
                status: GrantStatus.Открыт,
                startDate: new DateTime(2026, 1, 15),
                endDate: new DateTime(2026, 3, 31));

            // Act
            var createResult = logic.Create(model);

            var savedGrant = logic.ReadElement(new GrantSearchModel
            {
                ContestNumber = "РНФ-2026-001"
            });

            // Assert
            createResult.Should().BeTrue();

            savedGrant.Should().NotBeNull();
            savedGrant!.ContestNumber.Should().Be("РНФ-2026-001");
            savedGrant.Title.Should().Be("Грант интеграционного тестирования");
            savedGrant.Organization.Should().Be("Российский научный фонд");
            savedGrant.SubjectArea.Should().Be("Информационные системы");
            savedGrant.Status.Should().Be(GrantStatus.Открыт);
            savedGrant.Url.Should().Be("https://rscf.ru/contests/РНФ-2026-001");
        }

        [Fact]
        public void ReadList_WhenFilterByStatus_ShouldReturnOnlyGrantsWithSpecifiedStatus()
        {
            // Arrange
            var options = CreateOptions();
            var storage = new GrantStorage(options);
            var logic = new GrantLogic(NullLogger<GrantLogic>.Instance, storage);

            logic.Create(CreateGrantBindingModel(
                contestNumber: "РНФ-2026-001",
                title: "Открытый конкурс",
                subjectArea: "Информационные системы",
                status: GrantStatus.Открыт,
                startDate: DateTime.Today.AddDays(-5),
                endDate: DateTime.Today.AddDays(30)));

            logic.Create(CreateGrantBindingModel(
                contestNumber: "РНФ-2026-002",
                title: "Закрытый конкурс",
                subjectArea: "Информационные системы",
                status: GrantStatus.Закрыт,
                startDate: DateTime.Today.AddDays(-60),
                endDate: DateTime.Today.AddDays(-20)));

            // Act
            var result = logic.ReadList(new GrantSearchModel
            {
                Status = GrantStatus.Открыт
            });

            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(1);
            result![0].Title.Should().Be("Открытый конкурс");
            result[0].Status.Should().Be(GrantStatus.Открыт);
        }

        [Fact]
        public void ReadList_WhenFilterBySubjectArea_ShouldReturnMatchingGrants()
        {
            // Arrange
            var options = CreateOptions();
            var storage = new GrantStorage(options);
            var logic = new GrantLogic(NullLogger<GrantLogic>.Instance, storage);

            logic.Create(CreateGrantBindingModel(
                contestNumber: "РНФ-2026-003",
                title: "Грант по машинному обучению",
                subjectArea: "Машинное обучение; Искусственный интеллект",
                status: GrantStatus.Открыт,
                startDate: DateTime.Today.AddDays(-5),
                endDate: DateTime.Today.AddDays(30)));

            logic.Create(CreateGrantBindingModel(
                contestNumber: "РНФ-2026-004",
                title: "Грант по химии",
                subjectArea: "Химия",
                status: GrantStatus.Открыт,
                startDate: DateTime.Today.AddDays(-5),
                endDate: DateTime.Today.AddDays(30)));

            // Act
            var result = logic.ReadList(new GrantSearchModel
            {
                SubjectArea = "Машинное обучение"
            });

            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(1);
            result![0].Title.Should().Be("Грант по машинному обучению");
            result[0].SubjectArea.Should().Contain("Машинное обучение");
        }

        [Fact]
        public void ReadList_WhenFilterByContestNumber_ShouldReturnMatchingGrant()
        {
            // Arrange
            var options = CreateOptions();
            var storage = new GrantStorage(options);
            var logic = new GrantLogic(NullLogger<GrantLogic>.Instance, storage);

            logic.Create(CreateGrantBindingModel(
                contestNumber: "РНФ-2026-005",
                title: "Конкурс с уникальным номером",
                subjectArea: "Программная инженерия",
                status: GrantStatus.Открыт,
                startDate: DateTime.Today.AddDays(-5),
                endDate: DateTime.Today.AddDays(30)));

            logic.Create(CreateGrantBindingModel(
                contestNumber: "РНФ-2026-006",
                title: "Другой конкурс",
                subjectArea: "Программная инженерия",
                status: GrantStatus.Открыт,
                startDate: DateTime.Today.AddDays(-5),
                endDate: DateTime.Today.AddDays(30)));

            // Act
            var result = logic.ReadList(new GrantSearchModel
            {
                ContestNumber = "РНФ-2026-005"
            });

            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(1);
            result![0].ContestNumber.Should().Be("РНФ-2026-005");
            result[0].Title.Should().Be("Конкурс с уникальным номером");
        }

        [Fact]
        public void ReadList_WhenFilterByOrganization_ShouldReturnMatchingGrants()
        {
            // Arrange
            var options = CreateOptions();
            var storage = new GrantStorage(options);
            var logic = new GrantLogic(NullLogger<GrantLogic>.Instance, storage);

            logic.Create(CreateGrantBindingModel(
                contestNumber: "РНФ-2026-007",
                title: "Грант РНФ",
                subjectArea: "Информационные системы",
                status: GrantStatus.Открыт,
                startDate: DateTime.Today.AddDays(-5),
                endDate: DateTime.Today.AddDays(30),
                organization: "Российский научный фонд"));

            logic.Create(CreateGrantBindingModel(
                contestNumber: "ФОНД-2026-001",
                title: "Грант другого фонда",
                subjectArea: "Информационные системы",
                status: GrantStatus.Открыт,
                startDate: DateTime.Today.AddDays(-5),
                endDate: DateTime.Today.AddDays(30),
                organization: "Другой фонд"));

            // Act
            var result = logic.ReadList(new GrantSearchModel
            {
                Organization = "Российский научный фонд"
            });

            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(1);
            result![0].Title.Should().Be("Грант РНФ");
            result[0].Organization.Should().Be("Российский научный фонд");
        }

        [Fact]
        public void ReadList_WhenFilterByDateRange_ShouldReturnGrantsInsideDateRange()
        {
            // Arrange
            var options = CreateOptions();
            var storage = new GrantStorage(options);
            var logic = new GrantLogic(NullLogger<GrantLogic>.Instance, storage);

            logic.Create(CreateGrantBindingModel(
                contestNumber: "РНФ-2026-008",
                title: "Весенний грант",
                subjectArea: "Информационные системы",
                status: GrantStatus.Открыт,
                startDate: new DateTime(2026, 3, 1),
                endDate: new DateTime(2026, 4, 30)));

            logic.Create(CreateGrantBindingModel(
                contestNumber: "РНФ-2026-009",
                title: "Осенний грант",
                subjectArea: "Информационные системы",
                status: GrantStatus.Открыт,
                startDate: new DateTime(2026, 9, 1),
                endDate: new DateTime(2026, 10, 30)));

            // Act
            var result = logic.ReadList(new GrantSearchModel
            {
                DateFrom = new DateTime(2026, 3, 1),
                DateTo = new DateTime(2026, 5, 1)
            });

            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(1);
            result![0].Title.Should().Be("Весенний грант");
            result[0].StartDate.Should().BeOnOrAfter(new DateTime(2026, 3, 1));
            result[0].EndDate.Should().BeOnOrBefore(new DateTime(2026, 5, 1));
        }

        [Fact]
        public void Update_WhenGrantExists_ShouldChangeGrantDataInDatabase()
        {
            // Arrange
            var options = CreateOptions();
            var storage = new GrantStorage(options);
            var logic = new GrantLogic(NullLogger<GrantLogic>.Instance, storage);

            logic.Create(CreateGrantBindingModel(
                contestNumber: "РНФ-2026-010",
                title: "Старое название гранта",
                subjectArea: "Информационные системы",
                status: GrantStatus.Открыт,
                startDate: new DateTime(2026, 1, 1),
                endDate: new DateTime(2026, 3, 1)));

            var savedGrant = logic.ReadElement(new GrantSearchModel
            {
                ContestNumber = "РНФ-2026-010"
            });

            var updateModel = new GrantBindingModel
            {
                Id = savedGrant!.Id,
                ContestNumber = "РНФ-2026-010",
                Title = "Новое название гранта",
                Description = "Обновленное описание гранта",
                Organization = "Российский научный фонд",
                StartDate = new DateTime(2026, 2, 1),
                EndDate = new DateTime(2026, 4, 1),
                Amount = 5000000,
                Currency = "руб.",
                SubjectArea = "Искусственный интеллект; Анализ данных",
                Status = GrantStatus.Открыт,
                Url = "https://rscf.ru/contests/updated"
            };

            // Act
            var updateResult = logic.Update(updateModel);

            var updatedGrant = logic.ReadElement(new GrantSearchModel
            {
                ContestNumber = "РНФ-2026-010"
            });

            // Assert
            updateResult.Should().BeTrue();

            updatedGrant.Should().NotBeNull();
            updatedGrant!.Title.Should().Be("Новое название гранта");
            updatedGrant.Description.Should().Be("Обновленное описание гранта");
            updatedGrant.StartDate.Should().Be(new DateTime(2026, 2, 1));
            updatedGrant.EndDate.Should().Be(new DateTime(2026, 4, 1));
            updatedGrant.Amount.Should().Be(5000000);
            updatedGrant.SubjectArea.Should().Be("Искусственный интеллект; Анализ данных");
            updatedGrant.Url.Should().Be("https://rscf.ru/contests/updated");
        }

        [Fact]
        public void Delete_WhenGrantExists_ShouldRemoveGrantFromDatabase()
        {
            // Arrange
            var options = CreateOptions();
            var storage = new GrantStorage(options);
            var logic = new GrantLogic(NullLogger<GrantLogic>.Instance, storage);

            logic.Create(CreateGrantBindingModel(
                contestNumber: "РНФ-2026-011",
                title: "Грант для удаления",
                subjectArea: "Информационные системы",
                status: GrantStatus.Открыт,
                startDate: DateTime.Today.AddDays(-5),
                endDate: DateTime.Today.AddDays(30)));

            var savedGrant = logic.ReadElement(new GrantSearchModel
            {
                ContestNumber = "РНФ-2026-011"
            });

            // Act
            var deleteResult = logic.Delete(new GrantBindingModel
            {
                Id = savedGrant!.Id
            });

            var deletedGrant = logic.ReadElement(new GrantSearchModel
            {
                ContestNumber = "РНФ-2026-011"
            });

            // Assert
            deleteResult.Should().BeTrue();
            deletedGrant.Should().BeNull();
        }

        [Fact]
        public void ReadList_WhenDatabaseDoesNotContainGrants_ShouldReturnEmptyList()
        {
            // Arrange
            var options = CreateOptions();
            var storage = new GrantStorage(options);
            var logic = new GrantLogic(NullLogger<GrantLogic>.Instance, storage);

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

        private static GrantBindingModel CreateGrantBindingModel(
            string contestNumber,
            string title,
            string subjectArea,
            GrantStatus status,
            DateTime startDate,
            DateTime endDate,
            string organization = "Российский научный фонд")
        {
            return new GrantBindingModel
            {
                Id = 0,
                ContestNumber = contestNumber,
                Title = title,
                Description = "Тестовое описание грантового конкурса",
                Organization = organization,
                StartDate = startDate,
                EndDate = endDate,
                Amount = 3000000,
                Currency = "руб.",
                SubjectArea = subjectArea,
                Status = status,
                Url = $"https://rscf.ru/contests/{contestNumber}"
            };
        }
    }
}
