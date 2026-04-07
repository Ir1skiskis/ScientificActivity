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
    public class ConferenceStorage : IConferenceStorage
    {
        public List<ConferenceViewModel> GetFullList()
        {
            using var context = new ScientificActivityDatabase();

            return context.Conferences
                .AsEnumerable()
                .Select(x => x.GetViewModel)
                .ToList();
        }

        public List<ConferenceViewModel> GetFilteredList(ConferenceSearchModel model)
        {
            using var context = new ScientificActivityDatabase();

            IQueryable<Conference> query = context.Conferences;

            if (model.Id.HasValue)
            {
                query = query.Where(x => x.Id == model.Id.Value);
            }
            if (!string.IsNullOrWhiteSpace(model.Title))
            {
                query = query.Where(x => x.Title.Contains(model.Title));
            }
            if (!string.IsNullOrWhiteSpace(model.City))
            {
                query = query.Where(x => x.City != null && x.City.Contains(model.City));
            }
            if (!string.IsNullOrWhiteSpace(model.Country))
            {
                query = query.Where(x => x.Country != null && x.Country.Contains(model.Country));
            }
            if (!string.IsNullOrWhiteSpace(model.SubjectArea))
            {
                query = query.Where(x => x.SubjectArea != null && x.SubjectArea.Contains(model.SubjectArea));
            }
            if (model.Format.HasValue)
            {
                query = query.Where(x => x.Format == model.Format.Value);
            }
            if (model.Level.HasValue)
            {
                query = query.Where(x => x.Level == model.Level.Value);
            }
            if (model.DateFrom.HasValue)
            {
                query = query.Where(x => x.StartDate >= model.DateFrom.Value);
            }
            if (model.DateTo.HasValue)
            {
                query = query.Where(x => x.EndDate <= model.DateTo.Value);
            }

            return query
                .AsEnumerable()
                .Select(x => x.GetViewModel)
                .ToList();
        }

        public ConferenceViewModel? GetElement(ConferenceSearchModel model)
        {
            using var context = new ScientificActivityDatabase();

            Conference? element = null;

            if (model.Id.HasValue)
            {
                element = context.Conferences.FirstOrDefault(x => x.Id == model.Id.Value);
            }
            else if (!string.IsNullOrWhiteSpace(model.Title))
            {
                element = context.Conferences.FirstOrDefault(x => x.Title == model.Title);
            }

            return element?.GetViewModel;
        }

        public ConferenceViewModel? Insert(ConferenceBindingModel model)
        {
            using var context = new ScientificActivityDatabase();

            var newElement = Conference.Create(model);
            if (newElement == null)
            {
                return null;
            }

            context.Conferences.Add(newElement);
            context.SaveChanges();

            return newElement.GetViewModel;
        }

        public ConferenceViewModel? Update(ConferenceBindingModel model)
        {
            using var context = new ScientificActivityDatabase();

            var element = context.Conferences.FirstOrDefault(x => x.Id == model.Id);
            if (element == null)
            {
                return null;
            }

            element.Update(model);
            context.SaveChanges();

            return element.GetViewModel;
        }

        public ConferenceViewModel? Delete(ConferenceBindingModel model)
        {
            using var context = new ScientificActivityDatabase();

            var element = context.Conferences.FirstOrDefault(x => x.Id == model.Id);
            if (element == null)
            {
                return null;
            }

            var viewModel = element.GetViewModel;

            context.Conferences.Remove(element);
            context.SaveChanges();

            return viewModel;
        }
    }
}
