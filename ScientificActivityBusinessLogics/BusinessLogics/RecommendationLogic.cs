using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using ScientificActivityContracts.BusinessLogicsContracts;
using ScientificActivityContracts.ViewModels;
using ScientificActivityDatabaseImplement;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
            return _context.Grants
                .AsNoTracking()
                .Include(x => x.GrantTags)
                    .ThenInclude(x => x.Tag)
                .ToList()
                .Select(grant =>
                {
                    var matchedTags = grant.GrantTags
                        .Where(x => x.Tag != null && researcherTags.Contains(x.Tag.Name))
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
            return _context.Conferences
                .AsNoTracking()
                .Include(x => x.ConferenceTags)
                    .ThenInclude(x => x.Tag)
                .ToList()
                .Select(conference =>
                {
                    var matchedTags = conference.ConferenceTags
                        .Where(x => x.Tag != null && researcherTags.Contains(x.Tag.Name))
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
                        .Where(x => x.Tag != null && researcherTags.Contains(x.Tag.Name))
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
    }
}
