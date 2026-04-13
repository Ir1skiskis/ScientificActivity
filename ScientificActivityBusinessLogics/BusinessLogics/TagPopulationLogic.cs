using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using ScientificActivityContracts.BindingModels;
using ScientificActivityContracts.BusinessLogicsContracts;
using ScientificActivityContracts.SearchModels;
using ScientificActivityContracts.StoragesContracts;
using ScientificActivityDatabaseImplement;
using ScientificActivityDatabaseImplement.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScientificActivityBusinessLogics.BusinessLogics
{
    public class TagPopulationLogic : ITagPopulationLogic
    {
        private readonly ILogger<TagPopulationLogic> _logger;
        private readonly ScientificActivityDatabase _context;
        private readonly ITagStorage _tagStorage;

        public TagPopulationLogic(
            ILogger<TagPopulationLogic> logger,
            ScientificActivityDatabase context,
            ITagStorage tagStorage)
        {
            _logger = logger;
            _context = context;
            _tagStorage = tagStorage;
        }

        public void PopulateTags()
        {
            _logger.LogInformation("PopulateTags started");

            EnsureBaseTags();
            PopulateConferenceTags();
            PopulateJournalTags();
            PopulateGrantTags();

            _logger.LogInformation("PopulateTags finished");
        }

        private void EnsureBaseTags()
        {
            var baseTags = TagRules.Keys.ToList();

            foreach (var tagName in baseTags)
            {
                var normalized = NormalizeText(tagName);

                var existing = _tagStorage.GetElement(new TagSearchModel
                {
                    NormalizedName = normalized
                });

                if (existing == null)
                {
                    _tagStorage.Insert(new TagBindingModel
                    {
                        Name = tagName,
                        NormalizedName = normalized,
                        IsActive = true,
                        IsSelectable = true
                    });
                }
            }
        }

        private void PopulateConferenceTags()
        {
            var tags = _context.Tags.AsNoTracking().ToList();
            var conferences = _context.Conferences.AsNoTracking().ToList();

            foreach (var conference in conferences)
            {
                var tagNames = ExtractConferenceTagNames(conference.SubjectArea);

                foreach (var tagName in tagNames)
                {
                    var normalized = NormalizeText(tagName);
                    var tag = tags.FirstOrDefault(x => x.NormalizedName == normalized);
                    if (tag == null)
                    {
                        continue;
                    }

                    var exists = _context.ConferenceTags.Any(x => x.ConferenceId == conference.Id && x.TagId == tag.Id);
                    if (!exists)
                    {
                        _context.ConferenceTags.Add(new ConferenceTag
                        {
                            ConferenceId = conference.Id,
                            TagId = tag.Id
                        });
                    }
                }
            }

            _context.SaveChanges();
        }

        private void PopulateJournalTags()
        {
            var tags = _context.Tags.AsNoTracking().ToList();
            var journals = _context.Journals.AsNoTracking().ToList();

            foreach (var journal in journals)
            {
                var source = $"{journal.SubjectArea}";
                var tagNames = ExtractJournalTagNames(source);

                foreach (var tagName in tagNames)
                {
                    var normalized = NormalizeText(tagName);
                    var tag = tags.FirstOrDefault(x => x.NormalizedName == normalized);
                    if (tag == null)
                    {
                        continue;
                    }

                    var exists = _context.JournalTags.Any(x => x.JournalId == journal.Id && x.TagId == tag.Id);
                    if (!exists)
                    {
                        _context.JournalTags.Add(new JournalTag
                        {
                            JournalId = journal.Id,
                            TagId = tag.Id
                        });
                    }
                }
            }

            _context.SaveChanges();
        }

        private void PopulateGrantTags()
        {
            var tags = _context.Tags.AsNoTracking().ToList();
            var grants = _context.Grants.AsNoTracking().ToList();

            foreach (var grant in grants)
            {
                var tagNames = ExtractGrantTagNames(grant.Title, grant.Description, grant.SubjectArea);

                foreach (var tagName in tagNames)
                {
                    var normalized = NormalizeText(tagName);
                    var tag = tags.FirstOrDefault(x => x.NormalizedName == normalized);
                    if (tag == null)
                    {
                        continue;
                    }

                    var exists = _context.GrantTags.Any(x => x.GrantId == grant.Id && x.TagId == tag.Id);
                    if (!exists)
                    {
                        _context.GrantTags.Add(new GrantTag
                        {
                            GrantId = grant.Id,
                            TagId = tag.Id
                        });
                    }
                }
            }

            _context.SaveChanges();
        }

        private static readonly Dictionary<string, string[]> TagRules = new(StringComparer.OrdinalIgnoreCase)
        {
            ["Информационные технологии"] = new[] { "информационные технологии", "computer science", "computing", "information technology" },
            ["Искусственный интеллект"] = new[] { "искусственный интеллект", "artificial intelligence", "ai" },
            ["Математика"] = new[] { "математика", "mathematics", "applied mathematics", "numerical analysis" },
            ["Моделирование"] = new[] { "моделирование", "modeling", "modelling", "simulation" },
            ["Материаловедение"] = new[] { "материаловедение", "materials science", "mechanics of materials", "новые материалы" },
            ["Машиностроение"] = new[] { "машиностроение", "mechanical engineering", "mechatronics" },
            ["Микроэлектроника"] = new[] { "микроэлектроника", "integrated circuits", "semiconductor", "оптоэлектронных приборов", "фотонных интегральных схем", "гибкой и печатной электроники", "свч", "терагерцового диапазона" },
            ["Физика"] = new[] { "физика", "physics", "laser physics" },
            ["Химия"] = new[] { "химия", "chemistry" },
            ["Биология"] = new[] { "биология", "biology", "cell biology" },
            ["Биотехнологии"] = new[] { "биотехнологии", "biotechnology" },
            ["Медицина"] = new[] { "медицина", "medicine", "medical" },
            ["Фармакология"] = new[] { "фармакология", "pharmacology" },
            ["Фармация"] = new[] { "фармация", "pharmacy" },
            ["Психология"] = new[] { "психология", "psychology" },
            ["Социология"] = new[] { "социология", "sociology" },
            ["История"] = new[] { "история", "history" },
            ["Филология"] = new[] { "филология", "philology", "literature" },
            ["Образование"] = new[] { "образование", "education" },
            ["Педагогика"] = new[] { "педагогика" },
            ["Экономика"] = new[] { "экономика", "economics" },
            ["Менеджмент"] = new[] { "менеджмент", "management" },
            ["Финансы"] = new[] { "финансы", "finance" },
            ["Право"] = new[] { "право", "law", "юридические науки" },
            ["Экология"] = new[] { "экология", "environmental science", "природопользование" },
            ["Науки о Земле"] = new[] { "науки о земле", "earth sciences", "earth and planetary sciences", "геология", "география" },
            ["Энергетика"] = new[] { "энергетика", "energy" },
            ["Строительство"] = new[] { "строительство", "construction", "строительные" },
            ["Архитектура"] = new[] { "архитектура", "architecture" },
            ["Транспорт"] = new[] { "транспорт", "transport", "транспортные коммуникации" }
        };

        private static string NormalizeText(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
            {
                return string.Empty;
            }

            return text.Trim().ToLowerInvariant();
        }

        private List<string> ExtractConferenceTagNames(string? source)
        {
            if (string.IsNullOrWhiteSpace(source))
            {
                return new List<string>();
            }

            return source
                .Split(',', StringSplitOptions.RemoveEmptyEntries)
                .Select(x => x.Trim())
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .Where(x =>
                    !x.Equals("Широкая тематика", StringComparison.OrdinalIgnoreCase) &&
                    !x.Equals("Иное", StringComparison.OrdinalIgnoreCase) &&
                    !x.Contains("(Разное)", StringComparison.OrdinalIgnoreCase) &&
                    !x.Equals("Молодые учёные", StringComparison.OrdinalIgnoreCase) &&
                    !x.Equals("Проблемы науки", StringComparison.OrdinalIgnoreCase) &&
                    !x.Equals("Инновации", StringComparison.OrdinalIgnoreCase))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();
        }

        private List<string> ExtractJournalTagNames(string? source)
        {
            if (string.IsNullOrWhiteSpace(source))
            {
                return new List<string>();
            }

            var normalized = NormalizeText(source);
            var result = new List<string>();

            foreach (var rule in TagRules)
            {
                if (rule.Value.Any(keyword => normalized.Contains(NormalizeText(keyword))))
                {
                    result.Add(rule.Key);
                }
            }

            return result.Distinct(StringComparer.OrdinalIgnoreCase).ToList();
        }

        private List<string> ExtractGrantTagNames(string? title, string? description, string? subjectArea)
        {
            var source = $"{title} {description} {subjectArea}";
            if (string.IsNullOrWhiteSpace(source))
            {
                return new List<string>();
            }

            var normalized = NormalizeText(source);
            var result = new List<string>();

            foreach (var rule in TagRules)
            {
                if (rule.Value.Any(keyword => normalized.Contains(NormalizeText(keyword))))
                {
                    result.Add(rule.Key);
                }
            }

            return result.Distinct(StringComparer.OrdinalIgnoreCase).ToList();
        }
    }
}
