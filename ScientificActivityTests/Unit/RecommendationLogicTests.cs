using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using ScientificActivityBusinessLogics.BusinessLogics;
using ScientificActivityBusinessLogics.Helpers;
using ScientificActivityDatabaseImplement;
using ScientificActivityDatabaseImplement.Models;
using ScientificActivityDataModels.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScientificActivityTests.Unit
{
    public class RecommendationLogicTests
    {
        [Fact]
        public void GetRecommendations_WhenJournalHasMatchingTags_ShouldReturnRelevantJournals()
        {
            // Arrange
            using var context = CreateContext();
            SeedResearcherWithTags(context, 10, "Информационные системы", "Машинное обучение");

            var journal = CreateJournal(1, "Журнал информационных систем", "1234-5678");
            var irrelevantJournal = CreateJournal(2, "Журнал по химии", "2222-3333");

            context.Journals.AddRange(journal, irrelevantJournal);

            AddJournalTag(context, journal.Id, 1);
            AddJournalTag(context, irrelevantJournal.Id, 3);

            context.SaveChanges();

            var logic = CreateLogic(context);

            // Act
            var result = logic.GetRecommendations(10);

            // Assert
            result.Journals.Should().HaveCount(1);
            result.Journals[0].Title.Should().Be("Журнал информационных систем");
            result.Journals[0].MatchedTags.Should().Contain("Информационные системы");
        }

        [Fact]
        public void GetRecommendations_WhenConferenceHasMatchingTags_ShouldReturnRelevantConferences()
        {
            // Arrange
            using var context = CreateContext();
            SeedResearcherWithTags(context, 10, "Информационные системы", "Машинное обучение");

            var conference = CreateConference(
                id: 1,
                title: "Конференция по машинному обучению",
                startDate: DateTime.Today.AddDays(10),
                endDate: DateTime.Today.AddDays(12));

            var irrelevantConference = CreateConference(
                id: 2,
                title: "Конференция по химии",
                startDate: DateTime.Today.AddDays(10),
                endDate: DateTime.Today.AddDays(12));

            context.Conferences.AddRange(conference, irrelevantConference);

            AddConferenceTag(context, conference.Id, 2);
            AddConferenceTag(context, irrelevantConference.Id, 3);

            context.SaveChanges();

            var logic = CreateLogic(context);

            // Act
            var result = logic.GetRecommendations(10);

            // Assert
            result.Conferences.Should().HaveCount(1);
            result.Conferences[0].Title.Should().Be("Конференция по машинному обучению");
            result.Conferences[0].MatchedTags.Should().Contain("Машинное обучение");
        }

        [Fact]
        public void GetRecommendations_WhenGrantHasMatchingTags_ShouldReturnRelevantGrants()
        {
            // Arrange
            using var context = CreateContext();
            SeedResearcherWithTags(context, 10, "Информационные системы", "Машинное обучение");

            var grant = CreateGrant(
                id: 1,
                contestNumber: "РНФ-2026-001",
                title: "Грант по информационным системам",
                status: GrantStatus.Открыт,
                endDate: DateTime.Today.AddDays(30));

            var irrelevantGrant = CreateGrant(
                id: 2,
                contestNumber: "РНФ-2026-002",
                title: "Грант по химии",
                status: GrantStatus.Открыт,
                endDate: DateTime.Today.AddDays(30));

            context.Grants.AddRange(grant, irrelevantGrant);

            AddGrantTag(context, grant.Id, 1);
            AddGrantTag(context, irrelevantGrant.Id, 3);

            context.SaveChanges();

            var logic = CreateLogic(context);

            // Act
            var result = logic.GetRecommendations(10);

            // Assert
            result.Grants.Should().HaveCount(1);
            result.Grants[0].Title.Should().Be("Грант по информационным системам");
            result.Grants[0].MatchedTags.Should().Contain("Информационные системы");
        }

        [Fact]
        public void GetRecommendations_WhenJournalHasNoMatchingTags_ShouldNotReturnJournal()
        {
            // Arrange
            using var context = CreateContext();
            SeedResearcherWithTags(context, 10, "Информационные системы");

            var irrelevantJournal = CreateJournal(1, "Журнал по химии", "2222-3333");

            context.Journals.Add(irrelevantJournal);
            AddJournalTag(context, irrelevantJournal.Id, 3);
            context.SaveChanges();

            var logic = CreateLogic(context);

            // Act
            var result = logic.GetRecommendations(10);

            // Assert
            result.Journals.Should().BeEmpty();
        }

        [Fact]
        public void GetRecommendations_WhenConferenceIsPast_ShouldNotReturnConference()
        {
            // Arrange
            using var context = CreateContext();
            SeedResearcherWithTags(context, 10, "Информационные системы");

            var pastConference = CreateConference(
                id: 1,
                title: "Прошедшая конференция",
                startDate: DateTime.Today.AddDays(-20),
                endDate: DateTime.Today.AddDays(-10));

            var futureConference = CreateConference(
                id: 2,
                title: "Актуальная конференция",
                startDate: DateTime.Today.AddDays(10),
                endDate: DateTime.Today.AddDays(12));

            context.Conferences.AddRange(pastConference, futureConference);

            AddConferenceTag(context, pastConference.Id, 1);
            AddConferenceTag(context, futureConference.Id, 1);

            context.SaveChanges();

            var logic = CreateLogic(context);

            // Act
            var result = logic.GetRecommendations(10);

            // Assert
            result.Conferences.Should().HaveCount(1);
            result.Conferences[0].Title.Should().Be("Актуальная конференция");
            result.Conferences.Should().NotContain(x => x.Title == "Прошедшая конференция");
        }

        [Fact]
        public void GetRecommendations_WhenGrantIsClosed_ShouldNotReturnGrant()
        {
            // Arrange
            using var context = CreateContext();
            SeedResearcherWithTags(context, 10, "Информационные системы");

            var closedGrant = CreateGrant(
                id: 1,
                contestNumber: "РНФ-2026-001",
                title: "Закрытый грант",
                status: GrantStatus.Закрыт,
                endDate: DateTime.Today.AddDays(30));

            var openGrant = CreateGrant(
                id: 2,
                contestNumber: "РНФ-2026-002",
                title: "Открытый грант",
                status: GrantStatus.Открыт,
                endDate: DateTime.Today.AddDays(30));

            context.Grants.AddRange(closedGrant, openGrant);

            AddGrantTag(context, closedGrant.Id, 1);
            AddGrantTag(context, openGrant.Id, 1);

            context.SaveChanges();

            var logic = CreateLogic(context);

            // Act
            var result = logic.GetRecommendations(10);

            // Assert
            result.Grants.Should().HaveCount(1);
            result.Grants[0].Title.Should().Be("Открытый грант");
            result.Grants.Should().NotContain(x => x.Title == "Закрытый грант");
        }

        [Fact]
        public void GetRecommendations_WhenItemsHaveDifferentMatchCount_ShouldSortByRelevance()
        {
            // Arrange
            using var context = CreateContext();
            SeedResearcherWithTags(context, 10, "Информационные системы", "Машинное обучение", "Анализ данных");

            var strongJournal = CreateJournal(1, "Самый релевантный журнал", "1111-1111");
            var weakJournal = CreateJournal(2, "Менее релевантный журнал", "2222-2222");

            context.Journals.AddRange(strongJournal, weakJournal);

            AddJournalTag(context, strongJournal.Id, 1);
            AddJournalTag(context, strongJournal.Id, 2);
            AddJournalTag(context, strongJournal.Id, 4);

            AddJournalTag(context, weakJournal.Id, 1);

            context.SaveChanges();

            var logic = CreateLogic(context);

            // Act
            var result = logic.GetRecommendations(10);

            // Assert
            result.Journals.Should().HaveCount(2);
            result.Journals[0].Title.Should().Be("Самый релевантный журнал");
            result.Journals[0].MatchCount.Should().Be(3);
            result.Journals[1].Title.Should().Be("Менее релевантный журнал");
            result.Journals[1].MatchCount.Should().Be(1);
        }

        [Fact]
        public void GetRecommendations_WhenResearcherHasNoTags_ShouldReturnEmptyResult()
        {
            // Arrange
            using var context = CreateContext();

            context.Researchers.Add(CreateResearcher(10));
            context.Journals.Add(CreateJournal(1, "Журнал информационных систем", "1234-5678"));
            AddJournalTag(context, 1, 1);
            context.SaveChanges();

            var logic = CreateLogic(context);

            // Act
            var result = logic.GetRecommendations(10);

            // Assert
            result.ResearcherTags.Should().BeEmpty();
            result.Journals.Should().BeEmpty();
            result.Conferences.Should().BeEmpty();
            result.Grants.Should().BeEmpty();
        }

        [Fact]
        public void GetRecommendations_WhenResourceHasNoTags_ShouldNotReturnResource()
        {
            // Arrange
            using var context = CreateContext();
            SeedResearcherWithTags(context, 10, "Информационные системы");

            context.Journals.Add(CreateJournal(1, "Журнал без тематик", "5555-5555"));
            context.SaveChanges();

            var logic = CreateLogic(context);

            // Act
            var result = logic.GetRecommendations(10);

            // Assert
            result.Journals.Should().BeEmpty();
        }

        [Fact]
        public void GetRecommendations_WhenTagsHaveDifferentCaseButSameName_ShouldFindMatch()
        {
            // Arrange
            using var context = CreateContext();

            context.Researchers.Add(CreateResearcher(10));

            var researcherTag = new Tag
            {
                Id = 100,
                Name = "информационные системы",
                NormalizedName = "информационные системы",
                IsActive = true,
                IsSelectable = true
            };

            var journalTag = new Tag
            {
                Id = 101,
                Name = "Информационные системы",
                NormalizedName = "информационные системы",
                IsActive = true,
                IsSelectable = true
            };

            var journal = CreateJournal(1, "Журнал информационных систем", "1234-5678");

            context.Tags.AddRange(researcherTag, journalTag);
            context.Journals.Add(journal);

            context.ResearcherTags.Add(new ResearcherTag
            {
                ResearcherId = 10,
                TagId = 100
            });

            context.JournalTags.Add(new JournalTag
            {
                JournalId = 1,
                TagId = 101
            });

            context.SaveChanges();

            var logic = CreateLogic(context);

            // Act
            var result = logic.GetRecommendations(10);

            // Assert
            result.Journals.Should().HaveCount(1);
            result.Journals[0].Title.Should().Be("Журнал информационных систем");
        }

        [Fact]
        public void NormalizeTags_WhenTagsContainDuplicates_ShouldReturnUniqueTags()
        {
            // Arrange
            var tags = new List<string>
            {
                "Информационные системы",
                "информационные системы",
                " Машинное обучение ",
                "машинное обучение",
                "",
                "   "
            };

            // Act
            var result = RecommendationMathHelper.NormalizeTags(tags);

            // Assert
            result.Should().BeEquivalentTo(new List<string>
            {
                "информационные системы",
                "машинное обучение"
            });
        }

        [Fact]
        public void CalculateCosineSimilarity_WhenTagsPartiallyMatch_ShouldReturnCorrectValue()
        {
            // Arrange
            var researcherTags = new List<string>
            {
                "информационные системы",
                "машинное обучение",
                "анализ данных"
            };

            var resourceTags = new List<string>
            {
                "информационные системы",
                "машинное обучение"
            };

            // Act
            var result = RecommendationMathHelper.CalculateCosineSimilarity(researcherTags, resourceTags);

            // Assert
            result.Should().BeApproximately(0.816, 0.001);
        }

        private static ScientificActivityDatabase CreateContext()
        {
            var options = new DbContextOptionsBuilder<ScientificActivityDatabase>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            return new ScientificActivityDatabase(options);
        }

        private static RecommendationLogic CreateLogic(ScientificActivityDatabase context)
        {
            return new RecommendationLogic(
                NullLogger<RecommendationLogic>.Instance,
                context);
        }

        private static void SeedResearcherWithTags(ScientificActivityDatabase context, int researcherId, params string[] tagNames)
        {
            context.Researchers.Add(CreateResearcher(researcherId));

            var tags = new List<Tag>
            {
                new Tag { Id = 1, Name = "Информационные системы", NormalizedName = "информационные системы", IsActive = true, IsSelectable = true },
                new Tag { Id = 2, Name = "Машинное обучение", NormalizedName = "машинное обучение", IsActive = true, IsSelectable = true },
                new Tag { Id = 3, Name = "Химия", NormalizedName = "химия", IsActive = true, IsSelectable = true },
                new Tag { Id = 4, Name = "Анализ данных", NormalizedName = "анализ данных", IsActive = true, IsSelectable = true }
            };

            context.Tags.AddRange(tags);

            foreach (var tagName in tagNames)
            {
                var tag = tags.First(x => x.Name.Equals(tagName, StringComparison.OrdinalIgnoreCase));
                context.ResearcherTags.Add(new ResearcherTag
                {
                    ResearcherId = researcherId,
                    TagId = tag.Id
                });
            }

            context.SaveChanges();
        }

        private static Researcher CreateResearcher(int id)
        {
            return new Researcher
            {
                Id = id,
                Email = $"researcher{id}@example.com",
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
                ResearchTopics = "информационные системы; машинное обучение"
            };
        }

        private static Journal CreateJournal(int id, string title, string issn)
        {
            return new Journal
            {
                Id = id,
                Title = title,
                Issn = issn,
                Publisher = "Тестовый издатель",
                SubjectArea = "Информационные системы",
                IsVak = true,
                IsWhiteList = true,
                WhiteListLevel2025 = 2,
                Country = "Россия",
                Url = $"https://example.com/journal-{id}"
            };
        }

        private static Conference CreateConference(int id, string title, DateTime startDate, DateTime endDate)
        {
            return new Conference
            {
                Id = id,
                Title = title,
                Description = "Описание конференции",
                StartDate = startDate,
                EndDate = endDate,
                City = "Ульяновск",
                Country = "Россия",
                Organizer = "УлГТУ",
                SubjectArea = "Информационные системы",
                Format = ConferenceFormat.Смешанная,
                Level = ConferenceLevel.Международная,
                Url = $"https://example.com/conference-{id}"
            };
        }

        private static Grant CreateGrant(int id, string contestNumber, string title, GrantStatus status, DateTime endDate)
        {
            return new Grant
            {
                Id = id,
                ContestNumber = contestNumber,
                Title = title,
                Description = "Описание гранта",
                Organization = "Российский научный фонд",
                StartDate = DateTime.Today.AddDays(-5),
                EndDate = endDate,
                Amount = 3000000,
                Currency = "руб.",
                SubjectArea = "Информационные системы",
                Status = status,
                Url = $"https://rscf.ru/contests/{contestNumber}"
            };
        }

        private static void AddJournalTag(ScientificActivityDatabase context, int journalId, int tagId)
        {
            context.JournalTags.Add(new JournalTag
            {
                JournalId = journalId,
                TagId = tagId
            });
        }

        private static void AddConferenceTag(ScientificActivityDatabase context, int conferenceId, int tagId)
        {
            context.ConferenceTags.Add(new ConferenceTag
            {
                ConferenceId = conferenceId,
                TagId = tagId
            });
        }

        private static void AddGrantTag(ScientificActivityDatabase context, int grantId, int tagId)
        {
            context.GrantTags.Add(new GrantTag
            {
                GrantId = grantId,
                TagId = tagId
            });
        }
    }
}
