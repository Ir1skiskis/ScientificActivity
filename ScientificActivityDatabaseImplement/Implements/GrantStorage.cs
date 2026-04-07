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
    public class GrantStorage : IGrantStorage
    {
        public List<GrantViewModel> GetFullList()
        {
            using var context = new ScientificActivityDatabase();

            return context.Grants
                .AsEnumerable()
                .Select(x => x.GetViewModel)
                .ToList();
        }

        public List<GrantViewModel> GetFilteredList(GrantSearchModel model)
        {
            using var context = new ScientificActivityDatabase();

            IQueryable<Grant> query = context.Grants;

            if (model.Id.HasValue)
            {
                query = query.Where(x => x.Id == model.Id.Value);
            }
            if (!string.IsNullOrWhiteSpace(model.Title))
            {
                query = query.Where(x => x.Title.Contains(model.Title));
            }
            if (!string.IsNullOrWhiteSpace(model.Organization))
            {
                query = query.Where(x => x.Organization.Contains(model.Organization));
            }
            if (!string.IsNullOrWhiteSpace(model.SubjectArea))
            {
                query = query.Where(x => x.SubjectArea != null && x.SubjectArea.Contains(model.SubjectArea));
            }
            if (model.Status.HasValue)
            {
                query = query.Where(x => x.Status == model.Status.Value);
            }
            if (model.DateFrom.HasValue)
            {
                query = query.Where(x => x.StartDate >= model.DateFrom.Value);
            }
            if (model.DateTo.HasValue)
            {
                query = query.Where(x => x.EndDate <= model.DateTo.Value);
            }
            if (!string.IsNullOrEmpty(model.ContestNumber))
            {
                query = query.Where(x => x.ContestNumber.Contains(model.ContestNumber));
            }

            return query
                .AsEnumerable()
                .Select(x => x.GetViewModel)
                .ToList();
        }

        public GrantViewModel? GetElement(GrantSearchModel model)
        {
            using var context = new ScientificActivityDatabase();

            Grant? element = null;

            if (model.Id.HasValue)
            {
                element = context.Grants.FirstOrDefault(x => x.Id == model.Id.Value);
            }
            else if (!string.IsNullOrWhiteSpace(model.Title))
            {
                element = context.Grants.FirstOrDefault(x => x.Title == model.Title);
            }
            else if (!string.IsNullOrEmpty(model.ContestNumber))
            {
                element = context.Grants.FirstOrDefault(x => x.ContestNumber == model.ContestNumber);
            }

            return element?.GetViewModel;
        }

        public GrantViewModel? Insert(GrantBindingModel model)
        {
            using var context = new ScientificActivityDatabase();

            var newElement = Grant.Create(model);
            if (newElement == null)
            {
                return null;
            }

            context.Grants.Add(newElement);
            context.SaveChanges();

            return newElement.GetViewModel;
        }

        public GrantViewModel? Update(GrantBindingModel model)
        {
            using var context = new ScientificActivityDatabase();

            var element = context.Grants.FirstOrDefault(x => x.Id == model.Id);
            if (element == null)
            {
                return null;
            }

            element.Update(model);
            context.SaveChanges();

            return element.GetViewModel;
        }

        public GrantViewModel? Delete(GrantBindingModel model)
        {
            using var context = new ScientificActivityDatabase();

            var element = context.Grants.FirstOrDefault(x => x.Id == model.Id);
            if (element == null)
            {
                return null;
            }

            var viewModel = element.GetViewModel;

            context.Grants.Remove(element);
            context.SaveChanges();

            return viewModel;
        }
    }
}
