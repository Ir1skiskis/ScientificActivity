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
        private readonly ILogger<TagLogic> _logger;
        private readonly ITagStorage _tagStorage;
        private readonly ScientificActivityDatabase _context;

        public TagLogic(
            ILogger<TagLogic> logger,
            ITagStorage tagStorage,
            ScientificActivityDatabase context)
        {
            _logger = logger;
            _tagStorage = tagStorage;
            _context = context;
        }

        public List<TagViewModel> ReadList(bool onlySelectable = true)
        {
            return _tagStorage.GetFilteredList(new TagSearchModel
            {
                OnlyActive = true,
                OnlySelectable = onlySelectable
            });
        }

        public List<TagViewModel> GetResearcherTags(int researcherId)
        {
            return _context.ResearcherTags
                .AsNoTracking()
                .Include(x => x.Tag)
                .Where(x => x.ResearcherId == researcherId && x.Tag != null)
                .OrderBy(x => x.Tag.Name)
                .Select(x => new TagViewModel
                {
                    Id = x.Tag.Id,
                    Name = x.Tag.Name,
                    NormalizedName = x.Tag.NormalizedName,
                    IsActive = x.Tag.IsActive,
                    IsSelectable = x.Tag.IsSelectable
                })
                .ToList();
        }

        public void SaveResearcherTags(ResearcherTagBindingModel model)
        {
            if (model == null)
            {
                throw new ArgumentNullException(nameof(model));
            }

            _logger.LogInformation("SaveResearcherTags. ResearcherId:{ResearcherId}", model.ResearcherId);

            var existing = _context.ResearcherTags
                .Where(x => x.ResearcherId == model.ResearcherId)
                .ToList();

            _context.ResearcherTags.RemoveRange(existing);

            var distinctTagIds = model.TagIds
                .Distinct()
                .ToList();

            foreach (var tagId in distinctTagIds)
            {
                _context.ResearcherTags.Add(new ScientificActivityDatabaseImplement.Models.ResearcherTag
                {
                    ResearcherId = model.ResearcherId,
                    TagId = tagId
                });
            }

            _context.SaveChanges();
        }
    }
}
