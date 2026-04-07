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
    public class PublicationStorage : IPublicationStorage
    {
        public List<PublicationViewModel> GetFullList()
        {
            using var context = new ScientificActivityDatabase();

            return context.Publications
                .Include(x => x.Researcher)
                .Include(x => x.Journal)
                .Include(x => x.Conference)
                .AsEnumerable()
                .Select(x => x.GetViewModel)
                .ToList();
        }

        public List<PublicationViewModel> GetFilteredList(PublicationSearchModel model)
        {
            using var context = new ScientificActivityDatabase();

            IQueryable<Publication> query = context.Publications
                .Include(x => x.Researcher)
                .Include(x => x.Journal)
                .Include(x => x.Conference);

            if (model.Id.HasValue)
            {
                query = query.Where(x => x.Id == model.Id.Value);
            }
            if (model.ResearcherId.HasValue)
            {
                query = query.Where(x => x.ResearcherId == model.ResearcherId.Value);
            }
            if (model.JournalId.HasValue)
            {
                query = query.Where(x => x.JournalId == model.JournalId.Value);
            }
            if (model.ConferenceId.HasValue)
            {
                query = query.Where(x => x.ConferenceId == model.ConferenceId.Value);
            }
            if (!string.IsNullOrWhiteSpace(model.Title))
            {
                query = query.Where(x => x.Title.Contains(model.Title));
            }
            if (model.Year.HasValue)
            {
                query = query.Where(x => x.Year == model.Year.Value);
            }
            if (model.Type.HasValue)
            {
                query = query.Where(x => x.Type == model.Type.Value);
            }
            if (!string.IsNullOrWhiteSpace(model.Doi))
            {
                query = query.Where(x => x.Doi != null && x.Doi.Contains(model.Doi));
            }
            if (!string.IsNullOrWhiteSpace(model.Keywords))
            {
                query = query.Where(x => x.Keywords != null && x.Keywords.Contains(model.Keywords));
            }

            return query
                .AsEnumerable()
                .Select(x => x.GetViewModel)
                .ToList();
        }

        public PublicationViewModel? GetElement(PublicationSearchModel model)
        {
            using var context = new ScientificActivityDatabase();

            Publication? element = null;

            if (model.Id.HasValue)
            {
                element = context.Publications
                    .Include(x => x.Researcher)
                    .Include(x => x.Journal)
                    .Include(x => x.Conference)
                    .FirstOrDefault(x => x.Id == model.Id.Value);
            }
            else if (!string.IsNullOrWhiteSpace(model.Doi))
            {
                element = context.Publications
                    .Include(x => x.Researcher)
                    .Include(x => x.Journal)
                    .Include(x => x.Conference)
                    .FirstOrDefault(x => x.Doi == model.Doi);
            }

            return element?.GetViewModel;
        }

        public PublicationViewModel? Insert(PublicationBindingModel model)
        {
            using var context = new ScientificActivityDatabase();

            var newElement = Publication.Create(model);
            if (newElement == null)
            {
                return null;
            }

            context.Publications.Add(newElement);
            context.SaveChanges();

            return context.Publications
                .Include(x => x.Researcher)
                .Include(x => x.Journal)
                .Include(x => x.Conference)
                .First(x => x.Id == newElement.Id)
                .GetViewModel;
        }

        public PublicationViewModel? Update(PublicationBindingModel model)
        {
            using var context = new ScientificActivityDatabase();

            var element = context.Publications
                .Include(x => x.Researcher)
                .Include(x => x.Journal)
                .Include(x => x.Conference)
                .FirstOrDefault(x => x.Id == model.Id);

            if (element == null)
            {
                return null;
            }

            element.Update(model);
            context.SaveChanges();

            return context.Publications
                .Include(x => x.Researcher)
                .Include(x => x.Journal)
                .Include(x => x.Conference)
                .First(x => x.Id == element.Id)
                .GetViewModel;
        }

        public PublicationViewModel? Delete(PublicationBindingModel model)
        {
            using var context = new ScientificActivityDatabase();

            var element = context.Publications
                .Include(x => x.Researcher)
                .Include(x => x.Journal)
                .Include(x => x.Conference)
                .FirstOrDefault(x => x.Id == model.Id);

            if (element == null)
            {
                return null;
            }

            var viewModel = element.GetViewModel;

            context.Publications.Remove(element);
            context.SaveChanges();

            return viewModel;
        }
    }
}
