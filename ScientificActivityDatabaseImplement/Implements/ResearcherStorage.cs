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
    public class ResearcherStorage : IResearcherStorage
    {
        public List<ResearcherViewModel> GetFullList()
        {
            using var context = new ScientificActivityDatabase();

            return context.Researchers
                .Include(x => x.Publications)
                .AsEnumerable()
                .Select(x => x.GetViewModel)
                .ToList();
        }

        public List<ResearcherViewModel> GetFilteredList(ResearcherSearchModel model)
        {
            using var context = new ScientificActivityDatabase();

            IQueryable<Researcher> query = context.Researchers
                .Include(x => x.Publications);

            if (model.Id.HasValue)
            {
                query = query.Where(x => x.Id == model.Id.Value);
            }
            if (!string.IsNullOrWhiteSpace(model.Email))
            {
                query = query.Where(x => x.Email.Contains(model.Email));
            }
            if (!string.IsNullOrWhiteSpace(model.LastName))
            {
                query = query.Where(x => x.LastName.Contains(model.LastName));
            }
            if (!string.IsNullOrWhiteSpace(model.FirstName))
            {
                query = query.Where(x => x.FirstName.Contains(model.FirstName));
            }
            if (!string.IsNullOrWhiteSpace(model.Department))
            {
                query = query.Where(x => x.Department.Contains(model.Department));
            }
            if (!string.IsNullOrWhiteSpace(model.Position))
            {
                query = query.Where(x => x.Position.Contains(model.Position));
            }
            if (!string.IsNullOrWhiteSpace(model.ELibraryAuthorId))
            {
                query = query.Where(x => x.ELibraryAuthorId != null && x.ELibraryAuthorId.Contains(model.ELibraryAuthorId));
            }
            if (model.IsActive.HasValue)
            {
                query = query.Where(x => x.IsActive == model.IsActive.Value);
            }

            return query
                .AsEnumerable()
                .Select(x => x.GetViewModel)
                .ToList();
        }

        public ResearcherViewModel? GetElement(ResearcherSearchModel model)
        {
            using var context = new ScientificActivityDatabase();

            Researcher? element = null;

            if (model.Id.HasValue)
            {
                element = context.Researchers
                    .Include(x => x.Publications)
                    .FirstOrDefault(x => x.Id == model.Id.Value);
            }
            else if (!string.IsNullOrWhiteSpace(model.Email))
            {
                element = context.Researchers
                    .Include(x => x.Publications)
                    .FirstOrDefault(x => x.Email == model.Email);
            }
            else if (!string.IsNullOrWhiteSpace(model.ELibraryAuthorId))
            {
                element = context.Researchers
                    .Include(x => x.Publications)
                    .FirstOrDefault(x => x.ELibraryAuthorId == model.ELibraryAuthorId);
            }

            if (element == null)
            {
                return null;
            }

            if (!string.IsNullOrWhiteSpace(model.PasswordHash) && element.PasswordHash != model.PasswordHash)
            {
                return null;
            }

            return element.GetViewModel;
        }

        public ResearcherViewModel? Insert(ResearcherBindingModel model)
        {
            using var context = new ScientificActivityDatabase();

            var newElement = Researcher.Create(model);
            if (newElement == null)
            {
                return null;
            }

            context.Researchers.Add(newElement);
            context.SaveChanges();

            return context.Researchers
                .Include(x => x.Publications)
                .First(x => x.Id == newElement.Id)
                .GetViewModel;
        }

        public ResearcherViewModel? Update(ResearcherBindingModel model)
        {
            using var context = new ScientificActivityDatabase();

            var element = context.Researchers
                .Include(x => x.Publications)
                .FirstOrDefault(x => x.Id == model.Id);

            if (element == null)
            {
                return null;
            }

            element.Update(model);
            context.SaveChanges();

            return element.GetViewModel;
        }

        public ResearcherViewModel? Delete(ResearcherBindingModel model)
        {
            using var context = new ScientificActivityDatabase();

            var element = context.Researchers
                .Include(x => x.Publications)
                .FirstOrDefault(x => x.Id == model.Id);

            if (element == null)
            {
                return null;
            }

            var viewModel = element.GetViewModel;

            context.Researchers.Remove(element);
            context.SaveChanges();

            return viewModel;
        }
    }
}
