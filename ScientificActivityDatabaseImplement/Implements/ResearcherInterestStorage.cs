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
    public class ResearcherInterestStorage : IResearcherInterestStorage
    {
        public List<ResearcherInterestViewModel> GetFullList()
        {
            using var context = new ScientificActivityDatabase();

            return context.ResearcherInterests
                .Include(x => x.Researcher)
                .AsEnumerable()
                .Select(x => x.GetViewModel)
                .ToList();
        }

        public List<ResearcherInterestViewModel> GetFilteredList(ResearcherInterestSearchModel model)
        {
            using var context = new ScientificActivityDatabase();

            IQueryable<ResearcherInterest> query = context.ResearcherInterests
                .Include(x => x.Researcher);

            if (model.Id.HasValue)
            {
                query = query.Where(x => x.Id == model.Id.Value);
            }
            if (model.ResearcherId.HasValue)
            {
                query = query.Where(x => x.ResearcherId == model.ResearcherId.Value);
            }
            if (!string.IsNullOrWhiteSpace(model.Keyword))
            {
                query = query.Where(x => x.Keyword.Contains(model.Keyword));
            }

            return query
                .AsEnumerable()
                .Select(x => x.GetViewModel)
                .ToList();
        }

        public ResearcherInterestViewModel? GetElement(ResearcherInterestSearchModel model)
        {
            using var context = new ScientificActivityDatabase();

            ResearcherInterest? element = null;

            if (model.Id.HasValue)
            {
                element = context.ResearcherInterests
                    .Include(x => x.Researcher)
                    .FirstOrDefault(x => x.Id == model.Id.Value);
            }
            else if (model.ResearcherId.HasValue && !string.IsNullOrWhiteSpace(model.Keyword))
            {
                element = context.ResearcherInterests
                    .Include(x => x.Researcher)
                    .FirstOrDefault(x => x.ResearcherId == model.ResearcherId.Value && x.Keyword == model.Keyword);
            }

            return element?.GetViewModel;
        }

        public ResearcherInterestViewModel? Insert(ResearcherInterestBindingModel model)
        {
            using var context = new ScientificActivityDatabase();

            var newElement = ResearcherInterest.Create(model);
            if (newElement == null)
            {
                return null;
            }

            context.ResearcherInterests.Add(newElement);
            context.SaveChanges();

            return context.ResearcherInterests
                .Include(x => x.Researcher)
                .First(x => x.Id == newElement.Id)
                .GetViewModel;
        }

        public ResearcherInterestViewModel? Update(ResearcherInterestBindingModel model)
        {
            using var context = new ScientificActivityDatabase();

            var element = context.ResearcherInterests
                .Include(x => x.Researcher)
                .FirstOrDefault(x => x.Id == model.Id);

            if (element == null)
            {
                return null;
            }

            element.Update(model);
            context.SaveChanges();

            return context.ResearcherInterests
                .Include(x => x.Researcher)
                .First(x => x.Id == element.Id)
                .GetViewModel;
        }

        public ResearcherInterestViewModel? Delete(ResearcherInterestBindingModel model)
        {
            using var context = new ScientificActivityDatabase();

            var element = context.ResearcherInterests
                .Include(x => x.Researcher)
                .FirstOrDefault(x => x.Id == model.Id);

            if (element == null)
            {
                return null;
            }

            var viewModel = element.GetViewModel;

            context.ResearcherInterests.Remove(element);
            context.SaveChanges();

            return viewModel;
        }
    }
}
