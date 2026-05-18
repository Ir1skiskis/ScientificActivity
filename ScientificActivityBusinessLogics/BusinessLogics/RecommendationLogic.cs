using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using ScientificActivityContracts.BusinessLogicsContracts;
using ScientificActivityContracts.ViewModels;
using ScientificActivityDatabaseImplement;
using ScientificActivityDatabaseImplement.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.RegularExpressions;

namespace ScientificActivityBusinessLogics.BusinessLogics
{
    public class RecommendationLogic : IRecommendationLogic
    {
        private readonly ILogger<RecommendationLogic> _logger;
        private readonly ScientificActivityDatabase _context;

        public RecommendationLogic(
            ILogger<RecommendationLogic> logger,
            ScientificActivityDatabase context)
        {
            _logger = logger;
            _context = context;
        }

        public RecommendationResultViewModel GetRecommendations(int researcherId)
        {
            _logger.LogInformation("GetRecommendations. ResearcherId:{ResearcherId}", researcherId);

            var researcherTags = _context.ResearcherTags
                .AsNoTracking()
                .Include(x => x.Tag)
                .Where(x => x.ResearcherId == researcherId && x.Tag.IsActive)
                .Select(x => x.Tag.Name)
                .Distinct()
                .OrderBy(x => x)
                .ToList();

            if (!researcherTags.Any())
            {
                return new RecommendationResultViewModel();
            }

            return new RecommendationResultViewModel
            {
                ResearcherTags = researcherTags,
                Grants = GetGrantRecommendations(researcherTags),
                Conferences = GetConferenceRecommendations(researcherTags),
                Journals = GetJournalRecommendations(researcherTags)
            };
        }

        private List<RecommendationItemViewModel> GetGrantRecommendations(List<string> researcherTags)
        {
            var today = DateTime.SpecifyKind(DateTime.UtcNow.Date, DateTimeKind.Utc);

            return _context.Grants
                .AsNoTracking()
                .Include(x => x.GrantTags)
                    .ThenInclude(x => x.Tag)
                .Where(x => x.Status == ScientificActivityDataModels.Enums.GrantStatus.Открыт &&
                            x.EndDate >= today)
                .ToList()
                .Select(grant =>
                {
                    var matchedTags = grant.GrantTags
                        .Where(x => x.Tag != null && researcherTags.Contains(x.Tag.Name, StringComparer.OrdinalIgnoreCase))
                        .Select(x => x.Tag.Name)
                        .Distinct()
                        .OrderBy(x => x)
                        .ToList();

                    return new RecommendationItemViewModel
                    {
                        Id = grant.Id,
                        Title = grant.Title,
                        Description = grant.Description,
                        Url = grant.Url,
                        Source = grant.Organization,
                        DateFrom = grant.StartDate,
                        DateTo = grant.EndDate,
                        MatchedTags = matchedTags
                    };
                })
                .Where(x => x.MatchedTags.Any())
                .OrderByDescending(x => x.MatchCount)
                .ThenBy(x => x.DateTo)
                .Take(20)
                .ToList();
        }

        private List<RecommendationItemViewModel> GetConferenceRecommendations(List<string> researcherTags)
        {
            var today = DateTime.SpecifyKind(DateTime.UtcNow.Date, DateTimeKind.Utc);

            return _context.Conferences
                .AsNoTracking()
                .Include(x => x.ConferenceTags)
                    .ThenInclude(x => x.Tag)
                .Where(x => x.EndDate >= today)
                .ToList()
                .Select(conference =>
                {
                    var matchedTags = conference.ConferenceTags
                        .Where(x => x.Tag != null && researcherTags.Contains(x.Tag.Name, StringComparer.OrdinalIgnoreCase))
                        .Select(x => x.Tag.Name)
                        .Distinct()
                        .OrderBy(x => x)
                        .ToList();

                    var sourceParts = new List<string>();

                    if (!string.IsNullOrWhiteSpace(conference.City))
                    {
                        sourceParts.Add(conference.City);
                    }

                    if (!string.IsNullOrWhiteSpace(conference.Country))
                    {
                        sourceParts.Add(conference.Country);
                    }

                    return new RecommendationItemViewModel
                    {
                        Id = conference.Id,
                        Title = conference.Title,
                        Description = conference.Description,
                        Url = conference.Url,
                        Source = string.Join(", ", sourceParts),
                        DateFrom = conference.StartDate,
                        DateTo = conference.EndDate,
                        MatchedTags = matchedTags
                    };
                })
                .Where(x => x.MatchedTags.Any())
                .OrderByDescending(x => x.MatchCount)
                .ThenBy(x => x.DateFrom)
                .Take(20)
                .ToList();
        }

        private List<RecommendationItemViewModel> GetJournalRecommendations(List<string> researcherTags)
        {
            return _context.Journals
                .AsNoTracking()
                .Include(x => x.JournalTags)
                    .ThenInclude(x => x.Tag)
                .ToList()
                .Select(journal =>
                {
                    var matchedTags = journal.JournalTags
                        .Where(x => x.Tag != null && researcherTags.Contains(x.Tag.Name, StringComparer.OrdinalIgnoreCase))
                        .Select(x => x.Tag.Name)
                        .Distinct()
                        .OrderBy(x => x)
                        .ToList();

                    var descriptionParts = new List<string>();

                    if (!string.IsNullOrWhiteSpace(journal.SubjectArea))
                    {
                        descriptionParts.Add(journal.SubjectArea);
                    }

                    if (journal.IsVak)
                    {
                        descriptionParts.Add("ВАК");
                    }

                    if (journal.IsWhiteList)
                    {
                        descriptionParts.Add("Белый список");
                    }

                    if (journal.WhiteListLevel2025.HasValue)
                    {
                        descriptionParts.Add($"Уровень БС 2025: {journal.WhiteListLevel2025.Value}");
                    }
                    else if (journal.WhiteListLevel2023.HasValue)
                    {
                        descriptionParts.Add($"Уровень БС 2023: {journal.WhiteListLevel2023.Value}");
                    }

                    return new RecommendationItemViewModel
                    {
                        Id = journal.Id,
                        Title = journal.Title,
                        Description = string.Join(" | ", descriptionParts),
                        Url = journal.Url,
                        Source = journal.Issn,
                        MatchedTags = matchedTags
                    };
                })
                .Where(x => x.MatchedTags.Any())
                .OrderByDescending(x => x.MatchCount)
                .ThenBy(x => x.Title)
                .Take(30)
                .ToList();
        }

        public List<TagViewModel> GetResearcherTags(int researcherId)
        {
            return _context.ResearcherTags
                .Where(x => x.ResearcherId == researcherId)
                .Select(x => new TagViewModel
                {
                    Id = x.Tag.Id,
                    Name = x.Tag.Name,
                    NormalizedName = x.Tag.NormalizedName,
                    IsActive = x.Tag.IsActive,
                    IsSelectable = x.Tag.IsSelectable
                })
                .OrderBy(x => x.Name)
                .ToList();
        }

        public void SaveResearcherTags(int researcherId, List<int> tagIds)
        {
            var researcher = _context.Researchers.FirstOrDefault(x => x.Id == researcherId);

            if (researcher == null)
            {
                throw new InvalidOperationException("Исследователь не найден");
            }

            tagIds = tagIds
                .Distinct()
                .ToList();

            var existingTags = _context.Tags
                .Where(x => tagIds.Contains(x.Id))
                .Select(x => x.Id)
                .ToList();

            var oldLinks = _context.ResearcherTags
                .Where(x => x.ResearcherId == researcherId)
                .ToList();

            _context.ResearcherTags.RemoveRange(oldLinks);

            foreach (var tagId in existingTags)
            {
                _context.ResearcherTags.Add(new ResearcherTag
                {
                    ResearcherId = researcherId,
                    TagId = tagId
                });
            }

            _context.SaveChanges();
        }

        public List<TagViewModel> AutoAssignResearcherTagsFromPublications(
    int researcherId,
    int maxTagsCount,
    bool replaceExistingTags)
        {
            _logger.LogInformation(
                "AutoAssignResearcherTagsFromPublications. ResearcherId:{ResearcherId}, MaxTagsCount:{MaxTagsCount}, ReplaceExistingTags:{ReplaceExistingTags}",
                researcherId,
                maxTagsCount,
                replaceExistingTags);

            var researcherExists = _context.Researchers.Any(x => x.Id == researcherId);

            if (!researcherExists)
            {
                throw new InvalidOperationException("Исследователь не найден");
            }

            var publicationSources = _context.Publications
    .AsNoTracking()
    .Where(x => x.ResearcherId == researcherId)
    .Select(x => new
    {
        x.Title,
        x.Keywords,
        x.RubricGrnti,
        x.RubricOecd,
        x.RubricAsjc,
        x.VakSpecialty
    })
    .ToList();

            var sourceTexts = publicationSources
                .SelectMany(x => new[]
                {
        x.Keywords,
        x.RubricGrnti,
        x.RubricOecd,
        x.RubricAsjc,
        x.VakSpecialty,
        x.Title
                })
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .Select(x => x!)
                .ToList();

            if (!sourceTexts.Any())
            {
                _logger.LogInformation(
                    "AutoAssignResearcherTagsFromPublications. Publication thematic fields not found. ResearcherId:{ResearcherId}",
                    researcherId);

                return GetResearcherTags(researcherId);
            }

            var keywordCounts = sourceTexts
    .SelectMany(SplitPublicationThematicText)
    .Select(x => new
    {
        Original = x,
        Normalized = NormalizeRecommendationText(x)
    })
    .Where(x => !string.IsNullOrWhiteSpace(x.Normalized))
    .Where(x => x.Normalized.Length >= 3)
    .GroupBy(x => x.Normalized)
    .Select(group => new KeywordFrequencyModel
    {
        NormalizedKeyword = group.Key,
        DisplayKeyword = group
            .GroupBy(x => x.Original, StringComparer.OrdinalIgnoreCase)
            .OrderByDescending(x => x.Count())
            .ThenBy(x => x.Key)
            .First()
            .Key,
        Count = group.Count()
    })
    .OrderByDescending(x => x.Count)
    .ThenBy(x => x.DisplayKeyword)
    .ToList();

            if (!keywordCounts.Any())
            {
                _logger.LogInformation(
                    "AutoAssignResearcherTagsFromPublications. Keywords were empty after normalization. ResearcherId:{ResearcherId}",
                    researcherId);

                return GetResearcherTags(researcherId);
            }

            var selectableTags = GetSelectableRecommendationTags();

            if (!selectableTags.Any())
            {
                _logger.LogWarning(
                    "AutoAssignResearcherTagsFromPublications. Selectable tags not found.");

                return GetResearcherTags(researcherId);
            }

            var matchedTags = keywordCounts
                .SelectMany(keyword => selectableTags
                    .Select(tag => new
                    {
                        Keyword = keyword,
                        Tag = tag,
                        Score = CalculateKeywordTagScore(keyword.NormalizedKeyword, NormalizeRecommendationText(tag.Name), keyword.Count)
                    }))
                .Where(x => x.Score > 0)
                .GroupBy(x => x.Tag.Id)
                .Select(group => group
                    .OrderByDescending(x => x.Score)
                    .ThenByDescending(x => x.Keyword.Count)
                    .First())
                .OrderByDescending(x => x.Score)
                .ThenBy(x => x.Tag.Name)
                .Take(Math.Max(maxTagsCount, 1))
                .Select(x => x.Tag)
                .ToList();

            if (!matchedTags.Any())
            {
                _logger.LogInformation(
                    "AutoAssignResearcherTagsFromPublications. No tags matched publication keywords. ResearcherId:{ResearcherId}",
                    researcherId);

                return GetResearcherTags(researcherId);
            }

            if (replaceExistingTags)
            {
                var oldLinks = _context.ResearcherTags
                    .Where(x => x.ResearcherId == researcherId)
                    .ToList();

                _context.ResearcherTags.RemoveRange(oldLinks);
            }

            var existingTagIds = _context.ResearcherTags
                .Where(x => x.ResearcherId == researcherId)
                .Select(x => x.TagId)
                .ToHashSet();

            var addedCount = 0;

            foreach (var tag in matchedTags)
            {
                if (existingTagIds.Contains(tag.Id))
                {
                    continue;
                }

                _context.ResearcherTags.Add(new ResearcherTag
                {
                    ResearcherId = researcherId,
                    TagId = tag.Id
                });

                existingTagIds.Add(tag.Id);
                addedCount++;
            }

            _context.SaveChanges();

            _logger.LogInformation(
                "AutoAssignResearcherTagsFromPublications finished. ResearcherId:{ResearcherId}, Matched:{MatchedCount}, Added:{AddedCount}, Tags:{Tags}",
                researcherId,
                matchedTags.Count,
                addedCount,
                string.Join(", ", matchedTags.Select(x => x.Name)));

            return GetResearcherTags(researcherId);
        }

        private List<TagViewModel> GetSelectableRecommendationTags()
        {
            var today = DateTime.SpecifyKind(DateTime.UtcNow.Date, DateTimeKind.Utc);

            var conferenceTagIds = _context.ConferenceTags
                .Where(x => x.Conference.EndDate >= today)
                .Select(x => x.TagId);

            var grantTagIds = _context.GrantTags
                .Where(x => x.Grant.Status == ScientificActivityDataModels.Enums.GrantStatus.Открыт)
                .Where(x => x.Grant.EndDate >= today)
                .Select(x => x.TagId);

            var journalTagIds = _context.JournalTags
                .Select(x => x.TagId);

            var selectableTagIds = conferenceTagIds
                .Union(grantTagIds)
                .Union(journalTagIds);

            return _context.Tags
                .AsNoTracking()
                .Where(x => x.IsActive)
                .Where(x => x.IsSelectable)
                .Where(x => selectableTagIds.Contains(x.Id))
                .OrderBy(x => x.Name)
                .Select(x => new TagViewModel
                {
                    Id = x.Id,
                    Name = x.Name,
                    NormalizedName = x.NormalizedName,
                    IsActive = x.IsActive,
                    IsSelectable = x.IsSelectable
                })
                .ToList();
        }

        private static List<string> SplitPublicationThematicText(string? text)
        {
            if (string.IsNullOrWhiteSpace(text))
            {
                return new List<string>();
            }

            var result = new List<string>();

            var parts = text
                .Split(new[] { '/', ',', ';', '|', '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries)
                .Select(x => x.Trim())
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .Where(x => x.Length >= 3)
                .ToList();

            foreach (var part in parts)
            {
                result.Add(part);

                var normalized = NormalizeRecommendationText(part);

                if (normalized.Contains("ИСКУССТВЕННЫЙ ИНТЕЛЛЕКТ"))
                {
                    result.Add("Искусственный интеллект");
                }

                if (normalized.Contains("МАТЕМАТИЧЕСКОЕ МОДЕЛИРОВАНИЕ") ||
                    normalized.Contains("ТЕОРИЯ МОДЕЛИРОВАНИЯ"))
                {
                    result.Add("Моделирование");
                    result.Add("Математика");
                }

                if (normalized.Contains("ИНФОРМАЦИОННЫЕ СИСТЕМЫ") ||
                    normalized.Contains("БАЗЫ ДАННЫХ") ||
                    normalized.Contains("ИНФОРМАЦИОННЫЙ ПОИСК") ||
                    normalized.Contains("ПРОГРАММНОЕ ОБЕСПЕЧЕНИЕ") ||
                    normalized.Contains("ВЫЧИСЛИТЕЛЬНАЯ ТЕХНИКА") ||
                    normalized.Contains("КОМПЬЮТЕРНЫЕ СРЕДСТВА"))
                {
                    result.Add("Информационные технологии");
                }

                if (normalized.Contains("КИБЕРНЕТИКА"))
                {
                    result.Add("Информационные технологии");
                    result.Add("Моделирование");
                }

                if (normalized.Contains("ПЕДАГОГИКА") ||
                    normalized.Contains("ОБРАЗОВАНИЕ"))
                {
                    result.Add("Образование");
                    result.Add("Педагогика");
                }

                if (normalized.Contains("ЭЛЕКТРОНИКА") ||
                    normalized.Contains("РАДИОТЕХНИКА"))
                {
                    result.Add("Микроэлектроника");
                }

                if (normalized.Contains("АВТОМАТИЗАЦИЯ") ||
                    normalized.Contains("ЦИФРОВИЗАЦИЯ"))
                {
                    result.Add("Информационные технологии");
                }
            }

            return result
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();
        }

        private static int CalculateKeywordTagScore(string normalizedKeyword, string normalizedTag, int keywordCount)
        {
            if (string.IsNullOrWhiteSpace(normalizedKeyword) ||
                string.IsNullOrWhiteSpace(normalizedTag))
            {
                return 0;
            }

            if (normalizedKeyword == normalizedTag)
            {
                return keywordCount * 100;
            }

            if (normalizedKeyword.Contains(normalizedTag, StringComparison.OrdinalIgnoreCase))
            {
                return keywordCount * 80;
            }

            if (normalizedTag.Contains(normalizedKeyword, StringComparison.OrdinalIgnoreCase))
            {
                return keywordCount * 70;
            }

            if (normalizedTag == "ИСКУССТВЕННЫЙ ИНТЕЛЛЕКТ" &&
                normalizedKeyword.Contains("ИСКУССТВЕННЫЙ ИНТЕЛЛЕКТ", StringComparison.OrdinalIgnoreCase))
            {
                return keywordCount * 100;
            }

            if (normalizedTag == "ИНФОРМАЦИОННЫЕ ТЕХНОЛОГИИ" &&
                (normalizedKeyword.Contains("ИНФОРМАТИКА", StringComparison.OrdinalIgnoreCase) ||
                 normalizedKeyword.Contains("ИНФОРМАЦИОНН", StringComparison.OrdinalIgnoreCase) ||
                 normalizedKeyword.Contains("ВЫЧИСЛИТЕЛЬНАЯ ТЕХНИКА", StringComparison.OrdinalIgnoreCase) ||
                 normalizedKeyword.Contains("ПРОГРАММНОЕ ОБЕСПЕЧЕНИЕ", StringComparison.OrdinalIgnoreCase) ||
                 normalizedKeyword.Contains("БАЗЫ ДАННЫХ", StringComparison.OrdinalIgnoreCase)))
            {
                return keywordCount * 90;
            }

            if (normalizedTag == "МОДЕЛИРОВАНИЕ" &&
                (normalizedKeyword.Contains("МОДЕЛИРОВАНИЕ", StringComparison.OrdinalIgnoreCase) ||
                 normalizedKeyword.Contains("МОДЕЛИРОВАНИЯ", StringComparison.OrdinalIgnoreCase)))
            {
                return keywordCount * 90;
            }

            if (normalizedTag == "МАТЕМАТИКА" &&
                normalizedKeyword.Contains("МАТЕМАТИЧЕСК", StringComparison.OrdinalIgnoreCase))
            {
                return keywordCount * 80;
            }

            if (normalizedTag == "ОБРАЗОВАНИЕ" &&
                normalizedKeyword.Contains("ОБРАЗОВАН", StringComparison.OrdinalIgnoreCase))
            {
                return keywordCount * 90;
            }

            if (normalizedTag == "ПЕДАГОГИКА" &&
                normalizedKeyword.Contains("ПЕДАГОГ", StringComparison.OrdinalIgnoreCase))
            {
                return keywordCount * 90;
            }

            if (normalizedTag == "МИКРОЭЛЕКТРОНИКА" &&
                (normalizedKeyword.Contains("ЭЛЕКТРОНИКА", StringComparison.OrdinalIgnoreCase) ||
                 normalizedKeyword.Contains("РАДИОТЕХНИКА", StringComparison.OrdinalIgnoreCase)))
            {
                return keywordCount * 70;
            }

            return 0;
        }

        private static string NormalizeRecommendationText(string? value)
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

            return result.Trim();
        }

        private class KeywordFrequencyModel
        {
            public string DisplayKeyword { get; set; } = string.Empty;

            public string NormalizedKeyword { get; set; } = string.Empty;

            public int Count { get; set; }
        }
    }
}
