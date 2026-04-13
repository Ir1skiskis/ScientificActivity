using Microsoft.EntityFrameworkCore;
using ScientificActivityContracts.BindingModels;
using ScientificActivityContracts.SearchModels;
using ScientificActivityContracts.StoragesContracts;
using ScientificActivityContracts.ViewModels;
using ScientificActivityDatabaseImplement.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScientificActivityDatabaseImplement.Implements
{
    public class TagStorage : ITagStorage
    {
        private readonly ScientificActivityDatabase _context;

        public TagStorage(ScientificActivityDatabase context)
        {
            _context = context;
        }

        public List<TagViewModel> GetFullList()
        {
            return _context.Tags
                .AsNoTracking()
                .Select(x => x.GetViewModel)
                .ToList();
        }

        public List<TagViewModel> GetFilteredList(TagSearchModel model)
        {
            if (model == null)
            {
                return new();
            }

            var query = _context.Tags.AsNoTracking().AsQueryable();

            if (model.Id.HasValue)
            {
                query = query.Where(x => x.Id == model.Id.Value);
            }

            if (!string.IsNullOrWhiteSpace(model.Name))
            {
                query = query.Where(x => x.Name.Contains(model.Name));
            }

            if (!string.IsNullOrWhiteSpace(model.NormalizedName))
            {
                query = query.Where(x => x.NormalizedName == model.NormalizedName);
            }

            if (model.OnlyActive.HasValue && model.OnlyActive.Value)
            {
                query = query.Where(x => x.IsActive);
            }

            if (model.OnlySelectable.HasValue && model.OnlySelectable.Value)
            {
                query = query.Where(x => x.IsSelectable);
            }

            return query
                .OrderBy(x => x.Name)
                .Select(x => x.GetViewModel)
                .ToList();
        }

        public TagViewModel? GetElement(TagSearchModel model)
        {
            if (model == null)
            {
                return null;
            }

            return GetFilteredList(model).FirstOrDefault();
        }

        public TagViewModel? Insert(TagBindingModel model)
        {
            var entity = Tag.Create(model);
            if (entity == null)
            {
                return null;
            }

            _context.Tags.Add(entity);
            _context.SaveChanges();
            return entity.GetViewModel;
        }

        public TagViewModel? Update(TagBindingModel model)
        {
            var entity = _context.Tags.FirstOrDefault(x => x.Id == model.Id);
            if (entity == null)
            {
                return null;
            }

            entity.Update(model);
            _context.SaveChanges();
            return entity.GetViewModel;
        }

        public TagViewModel? Delete(TagBindingModel model)
        {
            var entity = _context.Tags.FirstOrDefault(x => x.Id == model.Id);
            if (entity == null)
            {
                return null;
            }

            _context.Tags.Remove(entity);
            _context.SaveChanges();
            return entity.GetViewModel;
        }
    }
}
