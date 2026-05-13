using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using ScientificActivityContracts.BusinessLogicsContracts;
using ScientificActivityDatabaseImplement;
using ScientificActivityDatabaseImplement.Models;
using ScientificActivityDataModels.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace ScientificActivityBusinessLogics.BusinessLogics
{
    public class TagGenerationLogic : ITagGenerationLogic
    {
        private readonly ScientificActivityDatabase _context;
        private readonly ILogger<TagGenerationLogic> _logger;

        private static readonly Regex SpacesRegex = new(@"\s+", RegexOptions.Compiled);

        private static readonly Regex JournalSubjectSplitRegex = new(
            @"[;,]\s*(?=(?:\d{1,2}\.\d{1,2}\.\d{1,2}\.?\s*)|(?:[A-ZА-ЯЁ]))",
            RegexOptions.Compiled | RegexOptions.IgnoreCase);

        private static readonly HashSet<string> GrantStopWords = new(StringComparer.OrdinalIgnoreCase)
        {
            "конкурс", "конкурса", "грант", "гранты", "грантов", "поддержка", "поддержки",
            "проект", "проекта", "проектов", "научных", "научные", "научный", "научного",
            "исследований", "исследования", "исследование", "реализация", "реализации",
            "проведение", "проведения", "развитие", "развития", "создание", "создания",
            "российский", "российских", "российской", "российского", "федерации",
            "молодых", "ученых", "учёных", "коллективов", "организаций", "организации",
            "рамках", "области", "направлении", "направления", "программа", "программы",
            "приоритетных", "исследовательских", "лабораторий", "лаборатории",
            "для", "при", "над", "под", "без", "или", "как", "что", "это", "его", "её",
            "их", "по", "на", "в", "во", "с", "со", "из", "от", "до", "за", "и", "а", "о", "об"
        };

        public TagGenerationLogic(ScientificActivityDatabase context, ILogger<TagGenerationLogic> logger)
        {
            _context = context;
            _logger = logger;
        }

        public int RegenerateAllTags()
        {
            var conferenceCount = RegenerateConferenceTags();
            var grantCount = RegenerateGrantTags();
            var journalCount = RegenerateJournalTags();

            return conferenceCount + grantCount + journalCount;
        }

        public int RegenerateConferenceTags()
        {
            var today = DateTime.UtcNow.Date;

            var conferences = _context.Conferences
                .Include(x => x.ConferenceTags)
                .Where(x => x.EndDate.Date >= today)
                .Where(x => !string.IsNullOrWhiteSpace(x.SubjectArea))
                .ToList();

            var oldConferenceTags = _context.ConferenceTags.ToList();
            _context.ConferenceTags.RemoveRange(oldConferenceTags);
            _context.SaveChanges();

            var createdLinksCount = 0;

            foreach (var conference in conferences)
            {
                var tagNames = ExtractSimpleSubjectTags(conference.SubjectArea);

                foreach (var tagName in tagNames)
                {
                    var tag = GetOrCreateTag(tagName);

                    var exists = _context.ConferenceTags.Any(x =>
                        x.ConferenceId == conference.Id &&
                        x.TagId == tag.Id);

                    if (exists)
                    {
                        continue;
                    }

                    _context.ConferenceTags.Add(new ConferenceTag
                    {
                        ConferenceId = conference.Id,
                        TagId = tag.Id
                    });

                    createdLinksCount++;
                }
            }

            _context.SaveChanges();

            _logger.LogInformation(
                "Conference tags regenerated. Actual conferences: {ConferencesCount}, links created: {LinksCount}",
                conferences.Count,
                createdLinksCount);

            return createdLinksCount;
        }

        public int RegenerateGrantTags()
        {
            var today = DateTime.UtcNow.Date;

            var grants = _context.Grants
                .Include(x => x.GrantTags)
                .Where(x => x.EndDate.Date >= today)
                .Where(x => x.Status == GrantStatus.Открыт)
                .Where(x => !string.IsNullOrWhiteSpace(x.Title))
                .ToList();

            var oldGrantTags = _context.GrantTags.ToList();
            _context.GrantTags.RemoveRange(oldGrantTags);
            _context.SaveChanges();

            var createdLinksCount = 0;

            foreach (var grant in grants)
            {
                var tagNames = ExtractGrantTags(grant.Title);

                foreach (var tagName in tagNames)
                {
                    var tag = GetOrCreateTag(tagName);

                    var exists = _context.GrantTags.Any(x =>
                        x.GrantId == grant.Id &&
                        x.TagId == tag.Id);

                    if (exists)
                    {
                        continue;
                    }

                    _context.GrantTags.Add(new GrantTag
                    {
                        GrantId = grant.Id,
                        TagId = tag.Id
                    });

                    createdLinksCount++;
                }
            }

            _context.SaveChanges();

            _logger.LogInformation(
                "Grant tags regenerated. Actual grants: {GrantsCount}, links created: {LinksCount}",
                grants.Count,
                createdLinksCount);

            return createdLinksCount;
        }

        public int RegenerateJournalTags()
        {
            var journals = _context.Journals
                .Include(x => x.JournalTags)
                .Where(x => !string.IsNullOrWhiteSpace(x.SubjectArea))
                .ToList();

            var oldJournalTags = _context.JournalTags.ToList();
            _context.JournalTags.RemoveRange(oldJournalTags);
            _context.SaveChanges();

            var createdLinksCount = 0;

            foreach (var journal in journals)
            {
                var tagNames = ExtractJournalSubjectTags(journal.SubjectArea);

                foreach (var tagName in tagNames)
                {
                    var tag = GetOrCreateTag(tagName);

                    var exists = _context.JournalTags.Any(x =>
                        x.JournalId == journal.Id &&
                        x.TagId == tag.Id);

                    if (exists)
                    {
                        continue;
                    }

                    _context.JournalTags.Add(new JournalTag
                    {
                        JournalId = journal.Id,
                        TagId = tag.Id
                    });

                    createdLinksCount++;
                }
            }

            _context.SaveChanges();

            _logger.LogInformation(
                "Journal tags regenerated. Journals: {JournalsCount}, links created: {LinksCount}",
                journals.Count,
                createdLinksCount);

            return createdLinksCount;
        }

        private Tag GetOrCreateTag(string name)
        {
            name = CleanTagName(name);
            var normalizedName = NormalizeTagName(name);

            var tag = _context.Tags.FirstOrDefault(x => x.NormalizedName == normalizedName);

            if (tag != null)
            {
                if (tag.Name.Length < name.Length)
                {
                    tag.Name = name;
                    _context.Tags.Update(tag);
                    _context.SaveChanges();
                }

                return tag;
            }

            tag = new Tag
            {
                Name = name,
                NormalizedName = normalizedName,
                IsActive = true,
                IsSelectable = true
            };

            _context.Tags.Add(tag);
            _context.SaveChanges();

            return tag;
        }

        private static List<string> ExtractSimpleSubjectTags(string? subjectArea)
        {
            if (string.IsNullOrWhiteSpace(subjectArea))
            {
                return new List<string>();
            }

            return subjectArea
                .Split(new[] { ';', ',', '/', '|', '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries)
                .Select(CleanTagName)
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .Where(x => x.Length >= 3)
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .OrderBy(x => x)
                .ToList();
        }

        private static List<string> ExtractJournalSubjectTags(string? subjectArea)
        {
            if (string.IsNullOrWhiteSpace(subjectArea))
            {
                return new List<string>();
            }

            var prepared = subjectArea
                .Replace("\r", " ")
                .Replace("\n", " ")
                .Replace("  ", " ");

            var parts = JournalSubjectSplitRegex
                .Split(prepared)
                .Select(CleanTagName)
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .Where(x => x.Length >= 3)
                .Select(FixJournalCodeSpacing)
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .OrderBy(x => x)
                .ToList();

            return parts;
        }

        private static List<string> ExtractGrantTags(string? title)
        {
            if (string.IsNullOrWhiteSpace(title))
            {
                return new List<string>();
            }

            var cleanedTitle = title
                .Replace("«", " ")
                .Replace("»", " ")
                .Replace("\"", " ")
                .Replace("'", " ")
                .Replace("(", " ")
                .Replace(")", " ")
                .Replace("[", " ")
                .Replace("]", " ")
                .Replace("{", " ")
                .Replace("}", " ")
                .Replace(":", " ")
                .Replace(";", " ")
                .Replace(",", " ")
                .Replace(".", " ")
                .Replace("/", " ")
                .Replace("\\", " ")
                .Replace("№", " ")
                .Replace("-", " ");

            cleanedTitle = SpacesRegex.Replace(cleanedTitle, " ").Trim();

            var words = cleanedTitle
                .Split(' ', StringSplitOptions.RemoveEmptyEntries)
                .Select(x => x.Trim())
                .Where(x => x.Length >= 4)
                .Where(x => !GrantStopWords.Contains(x))
                .Where(x => !int.TryParse(x, out _))
                .ToList();

            var result = new List<string>();

            foreach (var word in words)
            {
                result.Add(ToNormalReadableWord(word));
            }

            for (var i = 0; i < words.Count - 1; i++)
            {
                var phrase = $"{ToNormalReadableWord(words[i])} {ToNormalReadableWord(words[i + 1])}";
                result.Add(phrase);
            }

            for (var i = 0; i < words.Count - 2; i++)
            {
                var phrase = $"{ToNormalReadableWord(words[i])} {ToNormalReadableWord(words[i + 1])} {ToNormalReadableWord(words[i + 2])}";
                result.Add(phrase);
            }

            return result
                .Select(CleanTagName)
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .Where(x => x.Length >= 4)
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .OrderBy(x => x)
                .ToList();
        }

        private static string CleanTagName(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return string.Empty;
            }

            var result = value.Trim();

            result = result
                .Trim(' ', '.', ',', ';', ':', '-', '–', '—', '|', '/', '\\')
                .Replace(" ,", ",")
                .Replace(" ;", ";")
                .Replace("( ", "(")
                .Replace(" )", ")");

            result = SpacesRegex.Replace(result, " ").Trim();

            return result;
        }

        private static string FixJournalCodeSpacing(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return string.Empty;
            }

            var result = Regex.Replace(
                value,
                @"^(\d{1,2}\.\d{1,2}\.\d{1,2})\.\s*",
                "$1. ");

            result = Regex.Replace(
                result,
                @"^(\d{1,2}\.\d{1,2}\.\d{1,2})\s+",
                "$1 ");

            return CleanTagName(result);
        }

        private static string NormalizeTagName(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return string.Empty;
            }

            var result = value
                .Trim()
                .ToUpperInvariant()
                .Replace("Ё", "Е");

            result = Regex.Replace(result, @"\s+", " ");
            result = Regex.Replace(result, @"\s*\.\s*", ".");
            result = Regex.Replace(result, @"\s*,\s*", ", ");
            result = Regex.Replace(result, @"\s*;\s*", "; ");

            return result;
        }

        private static string ToNormalReadableWord(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return string.Empty;
            }

            value = value.Trim();

            if (value.Length == 1)
            {
                return value.ToUpperInvariant();
            }

            return char.ToUpperInvariant(value[0]) + value[1..].ToLowerInvariant();
        }
    }
}
