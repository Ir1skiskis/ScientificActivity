using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using ScientificActivityBusinessLogics.BusinessLogics;
using ScientificActivityContracts.BindingModels;
using ScientificActivityContracts.SearchModels;
using ScientificActivityDatabaseImplement;
using ScientificActivityDatabaseImplement.Implements;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScientificActivityTests.Integration
{
    public class JournalIntegrationTests
    {
        [Fact]
        public void Create_WhenJournalIsValid_ShouldSaveJournalToDatabase()
        {
            // Arrange
            var options = CreateOptions();
            var storage = new JournalStorage(options);
            var logic = new JournalLogic(NullLogger<JournalLogic>.Instance, storage);

            var model = CreateJournalBindingModel(
                title: "Журнал интеграционного тестирования",
                issn: "1111-1111",
                isVak: true,
                isWhiteList: false);

            // Act
            var createResult = logic.Create(model);
            var savedJournal = logic.ReadElement(new JournalSearchModel
            {
                Issn = "1111-1111"
            });

            // Assert
            createResult.Should().BeTrue();

            savedJournal.Should().NotBeNull();
            savedJournal!.Title.Should().Be("Журнал интеграционного тестирования");
            savedJournal.Issn.Should().Be("1111-1111");
            savedJournal.IsVak.Should().BeTrue();
            savedJournal.IsWhiteList.Should().BeFalse();
        }

        [Fact]
        public void ReadList_WhenFilterByIsVak_ShouldReturnOnlyVakJournals()
        {
            // Arrange
            var options = CreateOptions();
            var storage = new JournalStorage(options);
            var logic = new JournalLogic(NullLogger<JournalLogic>.Instance, storage);

            logic.Create(CreateJournalBindingModel(
                title: "Журнал ВАК",
                issn: "2222-2222",
                isVak: true,
                isWhiteList: false));

            logic.Create(CreateJournalBindingModel(
                title: "Обычный журнал",
                issn: "3333-3333",
                isVak: false,
                isWhiteList: false));

            // Act
            var result = logic.ReadList(new JournalSearchModel
            {
                IsVak = true
            });

            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(1);
            result![0].Title.Should().Be("Журнал ВАК");
            result[0].IsVak.Should().BeTrue();
        }

        [Fact]
        public void ReadList_WhenFilterByIsWhiteList_ShouldReturnOnlyWhiteListJournals()
        {
            // Arrange
            var options = CreateOptions();
            var storage = new JournalStorage(options);
            var logic = new JournalLogic(NullLogger<JournalLogic>.Instance, storage);

            logic.Create(CreateJournalBindingModel(
                title: "Журнал Белого списка",
                issn: "4444-4444",
                isVak: false,
                isWhiteList: true,
                whiteListLevel2025: 2));

            logic.Create(CreateJournalBindingModel(
                title: "Журнал вне Белого списка",
                issn: "5555-5555",
                isVak: false,
                isWhiteList: false));

            // Act
            var result = logic.ReadList(new JournalSearchModel
            {
                IsWhiteList = true
            });

            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(1);
            result![0].Title.Should().Be("Журнал Белого списка");
            result[0].IsWhiteList.Should().BeTrue();
            result[0].WhiteListLevel2025.Should().Be(2);
        }

        [Fact]
        public void ReadList_WhenFilterByTitle_ShouldReturnMatchingJournal()
        {
            // Arrange
            var options = CreateOptions();
            var storage = new JournalStorage(options);
            var logic = new JournalLogic(NullLogger<JournalLogic>.Instance, storage);

            logic.Create(CreateJournalBindingModel(
                title: "Информационные системы и технологии",
                issn: "6666-6666",
                isVak: true,
                isWhiteList: true));

            logic.Create(CreateJournalBindingModel(
                title: "Химические науки",
                issn: "7777-7777",
                isVak: false,
                isWhiteList: false));

            // Act
            var result = logic.ReadList(new JournalSearchModel
            {
                Title = "Информационные системы"
            });

            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(1);
            result![0].Title.Should().Be("Информационные системы и технологии");
        }

        [Fact]
        public void ReadPagedList_WhenPageSizeIsTwo_ShouldReturnOnlyTwoJournalsAndCorrectTotalPages()
        {
            // Arrange
            var options = CreateOptions();
            var storage = new JournalStorage(options);
            var logic = new JournalLogic(NullLogger<JournalLogic>.Instance, storage);

            logic.Create(CreateJournalBindingModel("Журнал 1", "1000-0001", true, false));
            logic.Create(CreateJournalBindingModel("Журнал 2", "1000-0002", true, false));
            logic.Create(CreateJournalBindingModel("Журнал 3", "1000-0003", true, false));
            logic.Create(CreateJournalBindingModel("Журнал 4", "1000-0004", true, false));
            logic.Create(CreateJournalBindingModel("Журнал 5", "1000-0005", true, false));

            // Act
            var result = logic.ReadPagedList(new JournalSearchModel
            {
                Page = 2,
                PageSize = 2,
                IsVak = true
            });

            // Assert
            result.Journals.Should().HaveCount(2);
            result.CurrentPage.Should().Be(2);
            result.PageSize.Should().Be(2);
            result.TotalCount.Should().Be(5);
            result.TotalPages.Should().Be(3);
            result.IsVak.Should().BeTrue();
        }

        [Fact]
        public void Update_WhenJournalExists_ShouldChangeJournalDataInDatabase()
        {
            // Arrange
            var options = CreateOptions();
            var storage = new JournalStorage(options);
            var logic = new JournalLogic(NullLogger<JournalLogic>.Instance, storage);

            logic.Create(CreateJournalBindingModel(
                title: "Старое название журнала",
                issn: "8888-8888",
                isVak: false,
                isWhiteList: false));

            var savedJournal = logic.ReadElement(new JournalSearchModel
            {
                Issn = "8888-8888"
            });

            var updateModel = new JournalBindingModel
            {
                Id = savedJournal!.Id,
                Title = "Новое название журнала",
                Issn = "8888-8888",
                EIssn = null,
                Publisher = "Обновленный издатель",
                SubjectArea = "Информационные системы; Программная инженерия",
                IsVak = true,
                IsWhiteList = true,
                WhiteListLevel2023 = null,
                WhiteListLevel2025 = 1,
                WhiteListState = "active",
                WhiteListNotice = null,
                WhiteListAcceptedDate = new DateTime(2025, 1, 1),
                WhiteListDiscontinuedDate = null,
                Country = "Россия",
                Url = "https://example.com/updated-journal",
                RcsiRecordSourceId = 123
            };

            // Act
            var updateResult = logic.Update(updateModel);

            var updatedJournal = logic.ReadElement(new JournalSearchModel
            {
                Issn = "8888-8888"
            });

            // Assert
            updateResult.Should().BeTrue();

            updatedJournal.Should().NotBeNull();
            updatedJournal!.Title.Should().Be("Новое название журнала");
            updatedJournal.Publisher.Should().Be("Обновленный издатель");
            updatedJournal.SubjectArea.Should().Be("Информационные системы; Программная инженерия");
            updatedJournal.IsVak.Should().BeTrue();
            updatedJournal.IsWhiteList.Should().BeTrue();
            updatedJournal.WhiteListLevel2025.Should().Be(1);
            updatedJournal.RcsiRecordSourceId.Should().Be(123);
        }

        [Fact]
        public void Delete_WhenJournalExists_ShouldRemoveJournalFromDatabase()
        {
            // Arrange
            var options = CreateOptions();
            var storage = new JournalStorage(options);
            var logic = new JournalLogic(NullLogger<JournalLogic>.Instance, storage);

            logic.Create(CreateJournalBindingModel(
                title: "Журнал для удаления",
                issn: "9999-9999",
                isVak: false,
                isWhiteList: false));

            var savedJournal = logic.ReadElement(new JournalSearchModel
            {
                Issn = "9999-9999"
            });

            // Act
            var deleteResult = logic.Delete(new JournalBindingModel
            {
                Id = savedJournal!.Id
            });

            var deletedJournal = logic.ReadElement(new JournalSearchModel
            {
                Issn = "9999-9999"
            });

            // Assert
            deleteResult.Should().BeTrue();
            deletedJournal.Should().BeNull();
        }

        private static DbContextOptions<ScientificActivityDatabase> CreateOptions()
        {
            return new DbContextOptionsBuilder<ScientificActivityDatabase>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;
        }

        private static JournalBindingModel CreateJournalBindingModel(
            string title,
            string issn,
            bool isVak,
            bool isWhiteList,
            int? whiteListLevel2025 = null)
        {
            return new JournalBindingModel
            {
                Id = 0,
                Title = title,
                Issn = issn,
                EIssn = null,
                Publisher = "Тестовый издатель",
                SubjectArea = "Информационные системы",
                IsVak = isVak,
                IsWhiteList = isWhiteList,
                WhiteListLevel2023 = null,
                WhiteListLevel2025 = whiteListLevel2025,
                WhiteListState = isWhiteList ? "active" : null,
                WhiteListNotice = null,
                WhiteListAcceptedDate = isWhiteList ? new DateTime(2025, 1, 1) : null,
                WhiteListDiscontinuedDate = null,
                Country = "Россия",
                Url = $"https://example.com/{issn}",
                RcsiRecordSourceId = null
            };
        }
    }
}
