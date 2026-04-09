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
    public class JournalVakSpecialtyStorage : IJournalVakSpecialtyStorage
    {
        public List<JournalVakSpecialtyViewModel> GetFullList()
        {
            using var context = new ScientificActivityDatabase();

            return context.JournalVakSpecialties
                .AsEnumerable()
                .Select(x => x.GetViewModel)
                .ToList();
        }

        public List<JournalVakSpecialtyViewModel> GetFilteredList(JournalVakSpecialtySearchModel model)
        {
            using var context = new ScientificActivityDatabase();

            IQueryable<JournalVakSpecialty> query = context.JournalVakSpecialties;

            if (model.Id.HasValue)
            {
                query = query.Where(x => x.Id == model.Id.Value);
            }

            if (model.JournalId.HasValue)
            {
                query = query.Where(x => x.JournalId == model.JournalId.Value);
            }

            if (!string.IsNullOrWhiteSpace(model.SpecialtyCode))
            {
                query = query.Where(x => x.SpecialtyCode == model.SpecialtyCode);
            }

            if (model.DateFrom.HasValue)
            {
                query = query.Where(x => x.DateFrom == model.DateFrom.Value);
            }

            if (model.DateTo.HasValue)
            {
                query = query.Where(x => x.DateTo == model.DateTo.Value);
            }

            return query
                .AsEnumerable()
                .Select(x => x.GetViewModel)
                .ToList();
        }

        public JournalVakSpecialtyViewModel? GetElement(JournalVakSpecialtySearchModel model)
        {
            using var context = new ScientificActivityDatabase();

            JournalVakSpecialty? element = null;

            if (model.Id.HasValue)
            {
                element = context.JournalVakSpecialties
                    .FirstOrDefault(x => x.Id == model.Id.Value);
            }
            else if (model.JournalId.HasValue &&
                     !string.IsNullOrWhiteSpace(model.SpecialtyCode) &&
                     model.DateFrom.HasValue)
            {
                element = context.JournalVakSpecialties.FirstOrDefault(x =>
                    x.JournalId == model.JournalId.Value &&
                    x.SpecialtyCode == model.SpecialtyCode &&
                    x.DateFrom == model.DateFrom.Value);
            }

            return element?.GetViewModel;
        }

        public JournalVakSpecialtyViewModel? Insert(JournalVakSpecialtyBindingModel model)
        {
            using var context = new ScientificActivityDatabase();

            var newElement = JournalVakSpecialty.Create(model);
            if (newElement == null)
            {
                return null;
            }

            context.JournalVakSpecialties.Add(newElement);
            context.SaveChanges();

            return newElement.GetViewModel;
        }

        public int DeleteByJournal(int journalId)
        {
            using var context = new ScientificActivityDatabase();

            var elements = context.JournalVakSpecialties
                .Where(x => x.JournalId == journalId)
                .ToList();

            if (elements.Count == 0)
            {
                return 0;
            }

            context.JournalVakSpecialties.RemoveRange(elements);
            context.SaveChanges();

            return elements.Count;
        }
    }
}
