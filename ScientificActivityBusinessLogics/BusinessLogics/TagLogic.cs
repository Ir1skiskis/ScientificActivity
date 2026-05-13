using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using ScientificActivityContracts.BindingModels;
using ScientificActivityContracts.BusinessLogicsContracts;
using ScientificActivityContracts.SearchModels;
using ScientificActivityContracts.StoragesContracts;
using ScientificActivityContracts.ViewModels;
using ScientificActivityDatabaseImplement;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScientificActivityBusinessLogics.BusinessLogics
{
    public class TagLogic : ITagLogic
    {
        private readonly ScientificActivityDatabase _context;
        private readonly ILogger<TagLogic> _logger;

        public TagLogic(ScientificActivityDatabase context, ILogger<TagLogic> logger)
        {
            _context = context;
            _logger = logger;
        }

        public List<TagViewModel> GetSelectableTags()
        {
            var today = DateTime.UtcNow.Date;

            var conferenceTagIds = _context.ConferenceTags
                .Where(x => x.Conference.EndDate.Date >= today)
                .Select(x => x.TagId);

            var grantTagIds = _context.GrantTags
                .Where(x => x.Grant.EndDate.Date >= today)
                .Select(x => x.TagId);

            var journalTagIds = _context.JournalTags
                .Select(x => x.TagId);

            var tagIds = conferenceTagIds
                .Union(grantTagIds)
                .Union(journalTagIds);

            return _context.Tags
                .Where(x => x.IsActive)
                .Where(x => x.IsSelectable)
                .Where(x => tagIds.Contains(x.Id))
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

        public List<TagViewModel> GetConferenceTags()
        {
            var today = DateTime.UtcNow.Date;

            var tagIds = _context.ConferenceTags
                .Where(x => x.Conference.EndDate.Date >= today)
                .Select(x => x.TagId)
                .Distinct();

            return _context.Tags
                .Where(x => x.IsActive)
                .Where(x => x.IsSelectable)
                .Where(x => tagIds.Contains(x.Id))
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

        public List<TagViewModel> GetGrantTags()
        {
            var today = DateTime.UtcNow.Date;

            var tagIds = _context.GrantTags
                .Where(x => x.Grant.EndDate.Date >= today)
                .Select(x => x.TagId)
                .Distinct();

            return _context.Tags
                .Where(x => x.IsActive)
                .Where(x => x.IsSelectable)
                .Where(x => tagIds.Contains(x.Id))
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

        public List<TagViewModel> GetJournalTags()
        {
            var tagIds = _context.JournalTags
                .Select(x => x.TagId)
                .Distinct();

            return _context.Tags
                .Where(x => x.IsActive)
                .Where(x => x.IsSelectable)
                .Where(x => tagIds.Contains(x.Id))
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
    }
}
