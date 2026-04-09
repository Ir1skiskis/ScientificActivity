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
    public class JournalStorage : IJournalStorage
    {
        public List<JournalViewModel> GetFullList()
        {
            using var context = new ScientificActivityDatabase();

            return context.Journals
                .AsEnumerable()
                .Select(x => x.GetViewModel)
                .ToList();
        }

        public List<JournalViewModel> GetFilteredList(JournalSearchModel model)
        {
            using var context = new ScientificActivityDatabase();

            IQueryable<Journal> query = context.Journals;

            if (model.Id.HasValue)
            {
                query = query.Where(x => x.Id == model.Id.Value);
            }
            if (!string.IsNullOrWhiteSpace(model.Title))
            {
                query = query.Where(x => x.Title.Contains(model.Title));
            }
            if (!string.IsNullOrWhiteSpace(model.Issn))
            {
                query = query.Where(x => x.Issn != null && x.Issn.Contains(model.Issn));
            }
            if (!string.IsNullOrWhiteSpace(model.SubjectArea))
            {
                query = query.Where(x => x.SubjectArea != null && x.SubjectArea.Contains(model.SubjectArea));
            }
            if (model.Quartile.HasValue)
            {
                query = query.Where(x => x.Quartile == model.Quartile.Value);
            }
            if (model.IsVak.HasValue)
            {
                query = query.Where(x => x.IsVak == model.IsVak.Value);
            }
            if (model.IsWhiteList.HasValue)
            {
                query = query.Where(x => x.IsWhiteList == model.IsWhiteList.Value);
            }

            return query
                .AsEnumerable()
                .Select(x => x.GetViewModel)
                .ToList();
        }

        public JournalViewModel? GetElement(JournalSearchModel model)
        {
            using var context = new ScientificActivityDatabase();

            Journal? element = null;

            if (model.Id.HasValue)
            {
                element = context.Journals.FirstOrDefault(x => x.Id == model.Id.Value);
            }
            else if (!string.IsNullOrWhiteSpace(model.Issn))
            {
                element = context.Journals.FirstOrDefault(x => x.Issn == model.Issn);
            }
            else if (!string.IsNullOrWhiteSpace(model.Title))
            {
                element = context.Journals.FirstOrDefault(x => x.Title == model.Title);
            }

            return element?.GetViewModel;
        }

        public JournalViewModel? Insert(JournalBindingModel model)
        {
            using var context = new ScientificActivityDatabase();

            var newElement = Journal.Create(model);
            if (newElement == null)
            {
                return null;
            }

            context.Journals.Add(newElement);
            context.SaveChanges();

            return newElement.GetViewModel;
        }

        public JournalViewModel? Update(JournalBindingModel model)
        {
            using var context = new ScientificActivityDatabase();

            var element = context.Journals.FirstOrDefault(x => x.Id == model.Id);
            if (element == null)
            {
                return null;
            }

            element.Update(model);
            context.SaveChanges();

            return element.GetViewModel;
        }

        public JournalViewModel? Delete(JournalBindingModel model)
        {
            using var context = new ScientificActivityDatabase();

            var element = context.Journals.FirstOrDefault(x => x.Id == model.Id);
            if (element == null)
            {
                return null;
            }

            var viewModel = element.GetViewModel;

            context.Journals.Remove(element);
            context.SaveChanges();

            return viewModel;
        }

        public List<JournalViewModel> GetPagedList(JournalSearchModel model)
        {
            using var context = new ScientificActivityDatabase();

            var query = context.Journals.AsQueryable();

            if (!string.IsNullOrWhiteSpace(model.Title))
            {
                query = query.Where(x => x.Title.Contains(model.Title));
            }

            if (!string.IsNullOrWhiteSpace(model.Issn))
            {
                query = query.Where(x => x.Issn != null && x.Issn.Contains(model.Issn));
            }

            if (model.IsVak.HasValue)
            {
                query = query.Where(x => x.IsVak == model.IsVak.Value);
            }

            if (model.IsWhiteList.HasValue)
            {
                query = query.Where(x => x.IsWhiteList == model.IsWhiteList.Value);
            }

            var skip = (model.Page - 1) * model.PageSize;

            return query
                .OrderBy(x => x.Title)
                .Skip(skip)
                .Take(model.PageSize)
                .AsEnumerable()
                .Select(x => x.GetViewModel)
                .ToList();
        }

        public int GetCount(JournalSearchModel model)
        {
            using var context = new ScientificActivityDatabase();

            var query = context.Journals.AsQueryable();

            if (!string.IsNullOrWhiteSpace(model.Title))
            {
                query = query.Where(x => x.Title.Contains(model.Title));
            }

            if (!string.IsNullOrWhiteSpace(model.Issn))
            {
                query = query.Where(x => x.Issn != null && x.Issn.Contains(model.Issn));
            }

            if (model.IsVak.HasValue)
            {
                query = query.Where(x => x.IsVak == model.IsVak.Value);
            }

            if (model.IsWhiteList.HasValue)
            {
                query = query.Where(x => x.IsWhiteList == model.IsWhiteList.Value);
            }

            return query.Count();
        }
    }
}
