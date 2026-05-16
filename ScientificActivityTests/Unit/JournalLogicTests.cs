using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using ScientificActivityBusinessLogics.BusinessLogics;
using ScientificActivityContracts.BindingModels;
using ScientificActivityContracts.SearchModels;
using ScientificActivityContracts.StoragesContracts;
using ScientificActivityContracts.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScientificActivityTests.Unit
{
    public class JournalLogicTests
    {
        private readonly Mock<IJournalStorage> _journalStorageMock;
        private readonly JournalLogic _logic;

        public JournalLogicTests()
        {
            _journalStorageMock = new Mock<IJournalStorage>();

            _logic = new JournalLogic(
                NullLogger<JournalLogic>.Instance,
                _journalStorageMock.Object);
        }

        [Fact]
        public void Create_WhenJournalModelIsCorrect_ShouldInsertJournalAndReturnTrue()
        {
            // Arrange
            var model = CreateValidJournalBindingModel();

            _journalStorageMock
                .Setup(x => x.GetElement(It.Is<JournalSearchModel>(m => m.Issn == "2413-1709")))
                .Returns((JournalViewModel?)null);

            _journalStorageMock
                .Setup(x => x.Insert(It.IsAny<JournalBindingModel>()))
                .Returns((JournalBindingModel m) => ToJournalViewModel(m, 1));

            // Act
            var result = _logic.Create(model);

            // Assert
            result.Should().BeTrue();

            _journalStorageMock.Verify(
                x => x.Insert(It.Is<JournalBindingModel>(m =>
                    m.Title == "Ученые записки Крымского федерального университета имени В.И. Вернадского. Социология. Педагогика. Психология" &&
                    m.Issn == "2413-1709" &&
                    m.Publisher == "Крымский федеральный университет" &&
                    m.SubjectArea == "Социология; Педагогика; Психология")),
                Times.Once);
        }

        [Fact]
        public void Update_WhenJournalExistsBySameIssnAndSameId_ShouldUpdateJournalAndReturnTrue()
        {
            // Arrange
            var model = CreateValidJournalBindingModel();
            model.Id = 10;
            model.Title = "Обновленное название журнала";
            model.Issn = "2413-1709";
            model.Publisher = "Обновленный издатель";

            _journalStorageMock
                .Setup(x => x.GetElement(It.Is<JournalSearchModel>(m => m.Issn == "2413-1709")))
                .Returns(new JournalViewModel
                {
                    Id = 10,
                    Title = "Старое название журнала",
                    Issn = "2413-1709",
                    Publisher = "Старый издатель"
                });

            _journalStorageMock
                .Setup(x => x.Update(It.IsAny<JournalBindingModel>()))
                .Returns((JournalBindingModel m) => ToJournalViewModel(m, m.Id));

            // Act
            var result = _logic.Update(model);

            // Assert
            result.Should().BeTrue();

            _journalStorageMock.Verify(
                x => x.Update(It.Is<JournalBindingModel>(m =>
                    m.Id == 10 &&
                    m.Title == "Обновленное название журнала" &&
                    m.Issn == "2413-1709" &&
                    m.Publisher == "Обновленный издатель")),
                Times.Once);
        }

        [Fact]
        public void Update_WhenJournalHasNoIssnButSameTitle_ShouldUpdateJournalByTitle()
        {
            // Arrange
            var model = CreateValidJournalBindingModel();
            model.Id = 15;
            model.Issn = null;
            model.Title = "Журнал без ISSN";
            model.SubjectArea = "Информационные системы; Программная инженерия";

            _journalStorageMock
                .Setup(x => x.Update(It.IsAny<JournalBindingModel>()))
                .Returns((JournalBindingModel m) => ToJournalViewModel(m, m.Id));

            // Act
            var result = _logic.Update(model);

            // Assert
            result.Should().BeTrue();

            _journalStorageMock.Verify(
                x => x.Update(It.Is<JournalBindingModel>(m =>
                    m.Id == 15 &&
                    m.Title == "Журнал без ISSN" &&
                    m.Issn == null &&
                    m.SubjectArea == "Информационные системы; Программная инженерия")),
                Times.Once);

            _journalStorageMock.Verify(
                x => x.GetElement(It.Is<JournalSearchModel>(m => m.Issn != null)),
                Times.Never);
        }

        [Fact]
        public void Create_WhenJournalImportedFromVakList_ShouldSaveIsVakTrue()
        {
            // Arrange
            var model = CreateValidJournalBindingModel();
            model.IsVak = true;
            model.IsWhiteList = false;
            model.WhiteListLevel2025 = null;

            _journalStorageMock
                .Setup(x => x.GetElement(It.Is<JournalSearchModel>(m => m.Issn == model.Issn)))
                .Returns((JournalViewModel?)null);

            _journalStorageMock
                .Setup(x => x.Insert(It.IsAny<JournalBindingModel>()))
                .Returns((JournalBindingModel m) => ToJournalViewModel(m, 2));

            // Act
            var result = _logic.Create(model);

            // Assert
            result.Should().BeTrue();

            _journalStorageMock.Verify(
                x => x.Insert(It.Is<JournalBindingModel>(m =>
                    m.IsVak == true &&
                    m.IsWhiteList == false &&
                    m.Issn == "2413-1709")),
                Times.Once);
        }

        [Fact]
        public void Create_WhenJournalImportedFromWhiteList_ShouldSaveIsWhiteListTrue()
        {
            // Arrange
            var model = CreateValidJournalBindingModel();
            model.IsVak = false;
            model.IsWhiteList = true;
            model.WhiteListLevel2025 = 2;
            model.WhiteListState = "active";
            model.RcsiRecordSourceId = 12345;

            _journalStorageMock
                .Setup(x => x.GetElement(It.Is<JournalSearchModel>(m => m.Issn == model.Issn)))
                .Returns((JournalViewModel?)null);

            _journalStorageMock
                .Setup(x => x.Insert(It.IsAny<JournalBindingModel>()))
                .Returns((JournalBindingModel m) => ToJournalViewModel(m, 3));

            // Act
            var result = _logic.Create(model);

            // Assert
            result.Should().BeTrue();

            _journalStorageMock.Verify(
                x => x.Insert(It.Is<JournalBindingModel>(m =>
                    m.IsWhiteList == true &&
                    m.WhiteListLevel2025 == 2 &&
                    m.WhiteListState == "active" &&
                    m.RcsiRecordSourceId == 12345)),
                Times.Once);
        }

        [Theory]
        [InlineData(1)]
        [InlineData(2)]
        [InlineData(3)]
        [InlineData(4)]
        public void Create_WhenWhiteListLevelIsSpecified_ShouldSaveWhiteListLevel(int whiteListLevel)
        {
            // Arrange
            var model = CreateValidJournalBindingModel();
            model.IsWhiteList = true;
            model.WhiteListLevel2025 = whiteListLevel;

            _journalStorageMock
                .Setup(x => x.GetElement(It.Is<JournalSearchModel>(m => m.Issn == model.Issn)))
                .Returns((JournalViewModel?)null);

            _journalStorageMock
                .Setup(x => x.Insert(It.IsAny<JournalBindingModel>()))
                .Returns((JournalBindingModel m) => ToJournalViewModel(m, 4));

            // Act
            var result = _logic.Create(model);

            // Assert
            result.Should().BeTrue();

            _journalStorageMock.Verify(
                x => x.Insert(It.Is<JournalBindingModel>(m =>
                    m.IsWhiteList == true &&
                    m.WhiteListLevel2025 == whiteListLevel)),
                Times.Once);
        }

        [Fact]
        public void ReadList_WhenSearchByTitle_ShouldReturnMatchingJournals()
        {
            // Arrange
            var journals = new List<JournalViewModel>
            {
                CreateJournalViewModel(1, "Информационные системы и технологии", "1111-1111", isVak: true, isWhiteList: true),
                CreateJournalViewModel(2, "Вестник технического университета", "2222-2222", isVak: false, isWhiteList: false)
            };

            _journalStorageMock
                .Setup(x => x.GetFilteredList(It.Is<JournalSearchModel>(m => m.Title == "Информационные системы")))
                .Returns(journals.Where(x => x.Title.Contains("Информационные системы")).ToList());

            // Act
            var result = _logic.ReadList(new JournalSearchModel
            {
                Title = "Информационные системы"
            });

            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(1);
            result![0].Title.Should().Be("Информационные системы и технологии");

            _journalStorageMock.Verify(
                x => x.GetFilteredList(It.Is<JournalSearchModel>(m => m.Title == "Информационные системы")),
                Times.Once);
        }

        [Fact]
        public void ReadElement_WhenSearchByIssn_ShouldReturnJournalWithSpecifiedIssn()
        {
            // Arrange
            var expectedJournal = CreateJournalViewModel(
                id: 5,
                title: "Журнал с точным ISSN",
                issn: "1234-5678",
                isVak: true,
                isWhiteList: false);

            _journalStorageMock
                .Setup(x => x.GetElement(It.Is<JournalSearchModel>(m => m.Issn == "1234-5678")))
                .Returns(expectedJournal);

            // Act
            var result = _logic.ReadElement(new JournalSearchModel
            {
                Issn = "1234-5678"
            });

            // Assert
            result.Should().NotBeNull();
            result!.Id.Should().Be(5);
            result.Issn.Should().Be("1234-5678");
            result.Title.Should().Be("Журнал с точным ISSN");

            _journalStorageMock.Verify(
                x => x.GetElement(It.Is<JournalSearchModel>(m => m.Issn == "1234-5678")),
                Times.Once);
        }

        [Fact]
        public void ReadList_WhenSearchByIsVak_ShouldReturnOnlyVakJournals()
        {
            // Arrange
            var journals = new List<JournalViewModel>
            {
                CreateJournalViewModel(1, "Журнал ВАК 1", "1111-1111", isVak: true, isWhiteList: false),
                CreateJournalViewModel(2, "Журнал ВАК 2", "2222-2222", isVak: true, isWhiteList: true)
            };

            _journalStorageMock
                .Setup(x => x.GetFilteredList(It.Is<JournalSearchModel>(m => m.IsVak == true)))
                .Returns(journals);

            // Act
            var result = _logic.ReadList(new JournalSearchModel
            {
                IsVak = true
            });

            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(2);
            result.Should().OnlyContain(x => x.IsVak);

            _journalStorageMock.Verify(
                x => x.GetFilteredList(It.Is<JournalSearchModel>(m => m.IsVak == true)),
                Times.Once);
        }

        [Fact]
        public void ReadList_WhenSearchByIsWhiteList_ShouldReturnOnlyWhiteListJournals()
        {
            // Arrange
            var journals = new List<JournalViewModel>
            {
                CreateJournalViewModel(1, "Журнал Белого списка 1", "1111-1111", isVak: false, isWhiteList: true),
                CreateJournalViewModel(2, "Журнал Белого списка 2", "2222-2222", isVak: true, isWhiteList: true)
            };

            _journalStorageMock
                .Setup(x => x.GetFilteredList(It.Is<JournalSearchModel>(m => m.IsWhiteList == true)))
                .Returns(journals);

            // Act
            var result = _logic.ReadList(new JournalSearchModel
            {
                IsWhiteList = true
            });

            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(2);
            result.Should().OnlyContain(x => x.IsWhiteList);

            _journalStorageMock.Verify(
                x => x.GetFilteredList(It.Is<JournalSearchModel>(m => m.IsWhiteList == true)),
                Times.Once);
        }

        [Fact]
        public void ReadPagedList_WhenPageAndPageSizeSpecified_ShouldReturnPagedResult()
        {
            // Arrange
            var model = new JournalSearchModel
            {
                Page = 2,
                PageSize = 3,
                Title = "журнал",
                IsVak = true,
                IsWhiteList = true
            };

            var pageItems = new List<JournalViewModel>
            {
                CreateJournalViewModel(4, "Журнал 4", "0000-0004", isVak: true, isWhiteList: true),
                CreateJournalViewModel(5, "Журнал 5", "0000-0005", isVak: true, isWhiteList: true),
                CreateJournalViewModel(6, "Журнал 6", "0000-0006", isVak: true, isWhiteList: true)
            };

            _journalStorageMock
                .Setup(x => x.GetCount(It.Is<JournalSearchModel>(m =>
                    m.Page == 2 &&
                    m.PageSize == 3 &&
                    m.Title == "журнал" &&
                    m.IsVak == true &&
                    m.IsWhiteList == true)))
                .Returns(10);

            _journalStorageMock
                .Setup(x => x.GetPagedList(It.Is<JournalSearchModel>(m =>
                    m.Page == 2 &&
                    m.PageSize == 3 &&
                    m.Title == "журнал" &&
                    m.IsVak == true &&
                    m.IsWhiteList == true)))
                .Returns(pageItems);

            // Act
            var result = _logic.ReadPagedList(model);

            // Assert
            result.Journals.Should().HaveCount(3);
            result.CurrentPage.Should().Be(2);
            result.PageSize.Should().Be(3);
            result.TotalCount.Should().Be(10);
            result.TotalPages.Should().Be(4);
            result.Title.Should().Be("журнал");
            result.IsVak.Should().BeTrue();
            result.IsWhiteList.Should().BeTrue();

            _journalStorageMock.Verify(x => x.GetCount(It.IsAny<JournalSearchModel>()), Times.Once);
            _journalStorageMock.Verify(x => x.GetPagedList(It.IsAny<JournalSearchModel>()), Times.Once);
        }

        [Fact]
        public void Create_WhenTitleIsEmpty_ShouldThrowArgumentNullException()
        {
            // Arrange
            var model = CreateValidJournalBindingModel();
            model.Title = "";

            // Act
            var action = () => _logic.Create(model);

            // Assert
            action.Should().Throw<ArgumentNullException>()
                .WithMessage("*Не указано название журнала*");

            _journalStorageMock.Verify(
                x => x.Insert(It.IsAny<JournalBindingModel>()),
                Times.Never);
        }

        [Fact]
        public void Create_WhenIssnContainsExtraSpaces_ShouldTrimIssnBeforeInsert()
        {
            // Arrange
            var model = CreateValidJournalBindingModel();
            model.Issn = " 2413-1709 ";

            _journalStorageMock
                .Setup(x => x.GetElement(It.Is<JournalSearchModel>(m => m.Issn == "2413-1709")))
                .Returns((JournalViewModel?)null);

            _journalStorageMock
                .Setup(x => x.Insert(It.IsAny<JournalBindingModel>()))
                .Returns((JournalBindingModel m) => ToJournalViewModel(m, 6));

            // Act
            var result = _logic.Create(model);

            // Assert
            result.Should().BeTrue();

            _journalStorageMock.Verify(
                x => x.Insert(It.Is<JournalBindingModel>(m =>
                    m.Issn == "2413-1709")),
                Times.Once);
        }

        [Fact]
        public void Create_WhenSubjectAreaSpecified_ShouldSaveSubjectArea()
        {
            // Arrange
            var model = CreateValidJournalBindingModel();
            model.SubjectArea = "  Информационные системы; Искусственный интеллект; Машинное обучение  ";

            _journalStorageMock
                .Setup(x => x.GetElement(It.Is<JournalSearchModel>(m => m.Issn == model.Issn)))
                .Returns((JournalViewModel?)null);

            _journalStorageMock
                .Setup(x => x.Insert(It.IsAny<JournalBindingModel>()))
                .Returns((JournalBindingModel m) => ToJournalViewModel(m, 7));

            // Act
            var result = _logic.Create(model);

            // Assert
            result.Should().BeTrue();

            _journalStorageMock.Verify(
                x => x.Insert(It.Is<JournalBindingModel>(m =>
                    m.SubjectArea == "Информационные системы; Искусственный интеллект; Машинное обучение")),
                Times.Once);
        }

        [Fact]
        public void ReadList_WhenStorageReturnsEmptyList_ShouldReturnEmptyList()
        {
            // Arrange
            _journalStorageMock
                .Setup(x => x.GetFullList())
                .Returns([]);

            // Act
            var result = _logic.ReadList(null);

            // Assert
            result.Should().NotBeNull();
            result.Should().BeEmpty();

            _journalStorageMock.Verify(
                x => x.GetFullList(),
                Times.Once);
        }

        [Fact]
        public void Create_WhenAnotherJournalWithSameIssnExists_ShouldThrowInvalidOperationException()
        {
            // Arrange
            var model = CreateValidJournalBindingModel();
            model.Id = 1;
            model.Issn = "2413-1709";

            _journalStorageMock
                .Setup(x => x.GetElement(It.Is<JournalSearchModel>(m => m.Issn == "2413-1709")))
                .Returns(new JournalViewModel
                {
                    Id = 99,
                    Title = "Другой журнал с таким ISSN",
                    Issn = "2413-1709"
                });

            // Act
            var action = () => _logic.Create(model);

            // Assert
            action.Should().Throw<InvalidOperationException>()
                .WithMessage("Журнал с таким ISSN уже существует");

            _journalStorageMock.Verify(
                x => x.Insert(It.IsAny<JournalBindingModel>()),
                Times.Never);
        }

        private static JournalBindingModel CreateValidJournalBindingModel()
        {
            return new JournalBindingModel
            {
                Id = 0,
                Title = "Ученые записки Крымского федерального университета имени В.И. Вернадского. Социология. Педагогика. Психология",
                Issn = "2413-1709",
                EIssn = null,
                Publisher = "Крымский федеральный университет",
                SubjectArea = "Социология; Педагогика; Психология",
                IsVak = false,
                IsWhiteList = false,
                WhiteListLevel2023 = null,
                WhiteListLevel2025 = null,
                WhiteListState = null,
                WhiteListNotice = null,
                WhiteListAcceptedDate = null,
                WhiteListDiscontinuedDate = null,
                Country = "Россия",
                Url = "https://example.com/journal",
                RcsiRecordSourceId = null
            };
        }

        private static JournalViewModel CreateJournalViewModel(
            int id,
            string title,
            string issn,
            bool isVak,
            bool isWhiteList)
        {
            return new JournalViewModel
            {
                Id = id,
                Title = title,
                Issn = issn,
                EIssn = null,
                Publisher = "Тестовый издатель",
                SubjectArea = "Информационные системы; Программная инженерия",
                IsVak = isVak,
                IsWhiteList = isWhiteList,
                WhiteListLevel2023 = isWhiteList ? 2 : null,
                WhiteListLevel2025 = isWhiteList ? 2 : null,
                WhiteListState = isWhiteList ? "active" : null,
                WhiteListNotice = null,
                WhiteListAcceptedDate = isWhiteList ? new DateTime(2025, 1, 1) : null,
                WhiteListDiscontinuedDate = null,
                Country = "Россия",
                Url = "https://example.com/journal",
                RcsiRecordSourceId = isWhiteList ? id + 1000 : null
            };
        }

        private static JournalViewModel ToJournalViewModel(JournalBindingModel model, int id)
        {
            return new JournalViewModel
            {
                Id = id,
                Title = model.Title,
                Issn = model.Issn,
                EIssn = model.EIssn,
                Publisher = model.Publisher,
                SubjectArea = model.SubjectArea,
                IsVak = model.IsVak,
                IsWhiteList = model.IsWhiteList,
                WhiteListLevel2023 = model.WhiteListLevel2023,
                WhiteListLevel2025 = model.WhiteListLevel2025,
                WhiteListState = model.WhiteListState,
                WhiteListNotice = model.WhiteListNotice,
                WhiteListAcceptedDate = model.WhiteListAcceptedDate,
                WhiteListDiscontinuedDate = model.WhiteListDiscontinuedDate,
                Country = model.Country,
                Url = model.Url,
                RcsiRecordSourceId = model.RcsiRecordSourceId
            };
        }
    }
}
