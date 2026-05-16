using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using ScientificActivityBusinessLogics.BusinessLogics;
using ScientificActivityDatabaseImplement;
using ScientificActivityDatabaseImplement.Models;
using ScientificActivityDataModels.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScientificActivityTests.Integration
{
    public class RecommendationIntegrationTests
    {
        [Fact]
        public void GetRecommendations_WhenJournalConferenceAndGrantHaveMatchingTags_ShouldReturnAllRelevantResources()
        {
            // Arrange
            var options = CreateOptions();

            using (var context = new ScientificActivityDatabase(options))
            {
                SeedResearcherWithTags(context, 1,
                    "Информационные системы",
                    "Машинное обучение");

                var journal = CreateJournal(
                    id: 1,
                    title: "Журнал информационных систем",
                    issn: "1111-1111");

                var conference = CreateConference(
                    id: 1,
                    title: "Конференция по машинному обучению",
                    startDate: DateTime.Today.AddDays(10),
                    endDate: DateTime.Today.AddDays(12));

                var grant = CreateGrant(
                    id: 1,
                    contestNumber: "РНФ-2026-001",
                    title: "Грант по информационным системам",
                    status: GrantStatus.Открыт,
                    endDate: DateTime.Today.AddDays(30));

                context.Journals.Add(journal);
                context.Conferences.Add(conference);
                context.Grants.Add(grant);

                context.SaveChanges();

                AddJournalTag(context, journal.Id, 1);
                AddConferenceTag(context, conference.Id, 2);
                AddGrantTag(context, grant.Id, 1);

                context.SaveChanges();
            }

            using (var context = new ScientificActivityDatabase(options))
            {
                var logic = CreateLogic(context);

                // Act
                var result = logic.GetRecommendations(1);

                // Assert
                result.Should().NotBeNull();

                result.ResearcherTags.Should().BeEquivalentTo(new List<string>
                {
                    "Информационные системы",
                    "Машинное обучение"
                });

                result.Journals.Should().HaveCount(1);
                result.Journals[0].Title.Should().Be("Журнал информационных систем");
                result.Journals[0].MatchedTags.Should().Contain("Информационные системы");

                result.Conferences.Should().HaveCount(1);
                result.Conferences[0].Title.Should().Be("Конференция по машинному обучению");
                result.Conferences[0].MatchedTags.Should().Contain("Машинное обучение");

                result.Grants.Should().HaveCount(1);
                result.Grants[0].Title.Should().Be("Грант по информационным системам");
                result.Grants[0].MatchedTags.Should().Contain("Информационные системы");
            }
        }

        [Fact]
        public void GetRecommendations_WhenResourcesHaveNoMatchingTags_ShouldReturnEmptyRecommendationLists()
        {
            // Arrange
            var options = CreateOptions();

            using (var context = new ScientificActivityDatabase(options))
            {
                SeedResearcherWithTags(context, 1,
                    "Информационные системы",
                    "Машинное обучение");

                var journal = CreateJournal(
                    id: 1,
                    title: "Журнал по химии",
                    issn: "2222-2222");

                var conference = CreateConference(
                    id: 1,
                    title: "Конференция по биологии",
                    startDate: DateTime.Today.AddDays(10),
                    endDate: DateTime.Today.AddDays(12));

                var grant = CreateGrant(
                    id: 1,
                    contestNumber: "РНФ-2026-002",
                    title: "Грант по медицине",
                    status: GrantStatus.Открыт,
                    endDate: DateTime.Today.AddDays(30));

                context.Journals.Add(journal);
                context.Conferences.Add(conference);
                context.Grants.Add(grant);

                context.SaveChanges();

                AddJournalTag(context, journal.Id, 3);
                AddConferenceTag(context, conference.Id, 4);
                AddGrantTag(context, grant.Id, 5);

                context.SaveChanges();
            }

            using (var context = new ScientificActivityDatabase(options))
            {
                var logic = CreateLogic(context);

                // Act
                var result = logic.GetRecommendations(1);

                // Assert
                result.Should().NotBeNull();

                result.Journals.Should().BeEmpty();
                result.Conferences.Should().BeEmpty();
                result.Grants.Should().BeEmpty();
            }
        }

        [Fact]
        public void GetRecommendations_WhenResearcherHasNoTags_ShouldReturnEmptyRecommendationLists()
        {
            // Arrange
            var options = CreateOptions();

            using (var context = new ScientificActivityDatabase(options))
            {
                context.Researchers.Add(CreateResearcher(1));

                var journal = CreateJournal(
                    id: 1,
                    title: "Журнал информационных систем",
                    issn: "3333-3333");

                context.Journals.Add(journal);

                context.Tags.Add(new Tag
                {
                    Id = 1,
                    Name = "Информационные системы",
                    NormalizedName = "информационные системы",
                    IsActive = true,
                    IsSelectable = true
                });

                context.SaveChanges();

                AddJournalTag(context, journal.Id, 1);

                context.SaveChanges();
            }

            using (var context = new ScientificActivityDatabase(options))
            {
                var logic = CreateLogic(context);

                // Act
                var result = logic.GetRecommendations(1);

                // Assert
                result.Should().NotBeNull();

                result.ResearcherTags.Should().BeEmpty();
                result.Journals.Should().BeEmpty();
                result.Conferences.Should().BeEmpty();
                result.Grants.Should().BeEmpty();
            }
        }

        [Fact]
        public void GetRecommendations_WhenJournalHasMoreMatchedTags_ShouldPlaceItFirst()
        {
            // Arrange
            var options = CreateOptions();

            using (var context = new ScientificActivityDatabase(options))
            {
                SeedResearcherWithTags(context, 1,
                    "Информационные системы",
                    "Машинное обучение",
                    "Анализ данных");

                var strongJournal = CreateJournal(
                    id: 1,
                    title: "Журнал с высокой релевантностью",
                    issn: "4444-4444");

                var weakJournal = CreateJournal(
                    id: 2,
                    title: "Журнал с низкой релевантностью",
                    issn: "5555-5555");

                context.Journals.AddRange(strongJournal, weakJournal);

                context.SaveChanges();

                AddJournalTag(context, strongJournal.Id, 1);
                AddJournalTag(context, strongJournal.Id, 2);
                AddJournalTag(context, strongJournal.Id, 6);

                AddJournalTag(context, weakJournal.Id, 1);

                context.SaveChanges();
            }

            using (var context = new ScientificActivityDatabase(options))
            {
                var logic = CreateLogic(context);

                // Act
                var result = logic.GetRecommendations(1);

                // Assert
                result.Journals.Should().HaveCount(2);

                result.Journals[0].Title.Should().Be("Журнал с высокой релевантностью");
                result.Journals[0].MatchCount.Should().Be(3);

                result.Journals[1].Title.Should().Be("Журнал с низкой релевантностью");
                result.Journals[1].MatchCount.Should().Be(1);
            }
        }

        [Fact]
        public void GetRecommendations_WhenConferenceIsPast_ShouldNotReturnPastConference()
        {
            // Arrange
            var options = CreateOptions();

            using (var context = new ScientificActivityDatabase(options))
            {
                SeedResearcherWithTags(context, 1,
                    "Информационные системы");

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

                context.SaveChanges();

                AddConferenceTag(context, pastConference.Id, 1);
                AddConferenceTag(context, futureConference.Id, 1);

                context.SaveChanges();
            }

            using (var context = new ScientificActivityDatabase(options))
            {
                var logic = CreateLogic(context);

                // Act
                var result = logic.GetRecommendations(1);

                // Assert
                result.Conferences.Should().HaveCount(1);
                result.Conferences[0].Title.Should().Be("Актуальная конференция");
                result.Conferences.Should().NotContain(x => x.Title == "Прошедшая конференция");
            }
        }

        [Fact]
        public void GetRecommendations_WhenGrantIsClosed_ShouldNotReturnClosedGrant()
        {
            // Arrange
            var options = CreateOptions();

            using (var context = new ScientificActivityDatabase(options))
            {
                SeedResearcherWithTags(context, 1,
                    "Информационные системы");

                var closedGrant = CreateGrant(
                    id: 1,
                    contestNumber: "РНФ-2026-003",
                    title: "Закрытый грант",
                    status: GrantStatus.Закрыт,
                    endDate: DateTime.Today.AddDays(30));

                var openGrant = CreateGrant(
                    id: 2,
                    contestNumber: "РНФ-2026-004",
                    title: "Открытый грант",
                    status: GrantStatus.Открыт,
                    endDate: DateTime.Today.AddDays(30));

                context.Grants.AddRange(closedGrant, openGrant);

                context.SaveChanges();

                AddGrantTag(context, closedGrant.Id, 1);
                AddGrantTag(context, openGrant.Id, 1);

                context.SaveChanges();
            }

            using (var context = new ScientificActivityDatabase(options))
            {
                var logic = CreateLogic(context);

                // Act
                var result = logic.GetRecommendations(1);

                // Assert
                result.Grants.Should().HaveCount(1);
                result.Grants[0].Title.Should().Be("Открытый грант");
                result.Grants.Should().NotContain(x => x.Title == "Закрытый грант");
            }
        }

        [Fact]
        public void GetRecommendations_WhenResourceHasNoTags_ShouldNotReturnResource()
        {
            // Arrange
            var options = CreateOptions();

            using (var context = new ScientificActivityDatabase(options))
            {
                SeedResearcherWithTags(context, 1,
                    "Информационные системы");

                var journalWithoutTags = CreateJournal(
                    id: 1,
                    title: "Журнал без тегов",
                    issn: "6666-6666");

                context.Journals.Add(journalWithoutTags);

                context.SaveChanges();
            }

            using (var context = new ScientificActivityDatabase(options))
            {
                var logic = CreateLogic(context);

                // Act
                var result = logic.GetRecommendations(1);

                // Assert
                result.Journals.Should().BeEmpty();
            }
        }

        private static DbContextOptions<ScientificActivityDatabase> CreateOptions()
        {
            return new DbContextOptionsBuilder<ScientificActivityDatabase>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;
        }

        private static RecommendationLogic CreateLogic(ScientificActivityDatabase context)
        {
            return new RecommendationLogic(
                NullLogger<RecommendationLogic>.Instance,
                context);
        }

        private static void SeedResearcherWithTags(
            ScientificActivityDatabase context,
            int researcherId,
            params string[] researcherTagNames)
        {
            context.Researchers.Add(CreateResearcher(researcherId));

            var tags = new List<Tag>
            {
                new Tag
                {
                    Id = 1,
                    Name = "Информационные системы",
                    NormalizedName = "информационные системы",
                    IsActive = true,
                    IsSelectable = true
                },
                new Tag
                {
                    Id = 2,
                    Name = "Машинное обучение",
                    NormalizedName = "машинное обучение",
                    IsActive = true,
                    IsSelectable = true
                },
                new Tag
                {
                    Id = 3,
                    Name = "Химия",
                    NormalizedName = "химия",
                    IsActive = true,
                    IsSelectable = true
                },
                new Tag
                {
                    Id = 4,
                    Name = "Биология",
                    NormalizedName = "биология",
                    IsActive = true,
                    IsSelectable = true
                },
                new Tag
                {
                    Id = 5,
                    Name = "Медицина",
                    NormalizedName = "медицина",
                    IsActive = true,
                    IsSelectable = true
                },
                new Tag
                {
                    Id = 6,
                    Name = "Анализ данных",
                    NormalizedName = "анализ данных",
                    IsActive = true,
                    IsSelectable = true
                }
            };

            context.Tags.AddRange(tags);
            context.SaveChanges();

            foreach (var tagName in researcherTagNames)
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
                PasswordHash = "password-hash",
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
                EIssn = null,
                Publisher = "Тестовый издатель",
                SubjectArea = "Информационные системы",
                IsVak = true,
                IsWhiteList = true,
                WhiteListLevel2025 = 2,
                Country = "Россия",
                Url = $"https://example.com/journal-{id}",
                RcsiRecordSourceId = id + 1000
            };
        }

        private static Conference CreateConference(
            int id,
            string title,
            DateTime startDate,
            DateTime endDate)
        {
            return new Conference
            {
                Id = id,
                Title = title,
                Description = "Тестовое описание конференции",
                StartDate = startDate,
                EndDate = endDate,
                City = "Ульяновск",
                Country = "Россия",
                Organizer = "Тестовый организатор",
                SubjectArea = "Информационные системы",
                Format = ConferenceFormat.Смешанная,
                Level = ConferenceLevel.Международная,
                Url = $"https://example.com/conference-{id}"
            };
        }

        private static Grant CreateGrant(
            int id,
            string contestNumber,
            string title,
            GrantStatus status,
            DateTime endDate)
        {
            return new Grant
            {
                Id = id,
                ContestNumber = contestNumber,
                Title = title,
                Description = "Тестовое описание грантового конкурса",
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

        private static void AddJournalTag(
            ScientificActivityDatabase context,
            int journalId,
            int tagId)
        {
            context.JournalTags.Add(new JournalTag
            {
                JournalId = journalId,
                TagId = tagId
            });
        }

        private static void AddConferenceTag(
            ScientificActivityDatabase context,
            int conferenceId,
            int tagId)
        {
            context.ConferenceTags.Add(new ConferenceTag
            {
                ConferenceId = conferenceId,
                TagId = tagId
            });
        }

        private static void AddGrantTag(
            ScientificActivityDatabase context,
            int grantId,
            int tagId)
        {
            context.GrantTags.Add(new GrantTag
            {
                GrantId = grantId,
                TagId = tagId
            });
        }
    }
}
