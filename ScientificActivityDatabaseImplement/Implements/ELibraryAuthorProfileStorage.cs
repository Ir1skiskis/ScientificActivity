using Microsoft.EntityFrameworkCore;
using ScientificActivityContracts.BindingModels;
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
    public class ELibraryAuthorProfileStorage : IELibraryAuthorProfileStorage
    {
        public ELibraryAuthorProfileViewModel? GetByResearcherId(int researcherId)
        {
            using var context = new ScientificActivityDatabase();

            return context.ELibraryAuthorProfiles
                .Include(x => x.Researcher)
                .FirstOrDefault(x => x.ResearcherId == researcherId)
                ?.GetViewModel;
        }

        public ELibraryAuthorProfileViewModel? GetByAuthorId(string authorId)
        {
            using var context = new ScientificActivityDatabase();

            return context.ELibraryAuthorProfiles
                .Include(x => x.Researcher)
                .FirstOrDefault(x => x.AuthorId == authorId)
                ?.GetViewModel;
        }

        public ELibraryAuthorProfileViewModel? Insert(ELibraryAuthorProfileBindingModel model)
        {
            using var context = new ScientificActivityDatabase();

            var newElement = ELibraryAuthorProfile.Create(model);
            if (newElement == null)
            {
                return null;
            }

            context.ELibraryAuthorProfiles.Add(newElement);
            context.SaveChanges();

            return context.ELibraryAuthorProfiles
                .Include(x => x.Researcher)
                .First(x => x.Id == newElement.Id)
                .GetViewModel;
        }

        public ELibraryAuthorProfileViewModel? Update(ELibraryAuthorProfileBindingModel model)
        {
            using var context = new ScientificActivityDatabase();

            var element = context.ELibraryAuthorProfiles
                .Include(x => x.Researcher)
                .FirstOrDefault(x => x.Id == model.Id);

            if (element == null)
            {
                return null;
            }

            element.Update(model);
            context.SaveChanges();

            return context.ELibraryAuthorProfiles
                .Include(x => x.Researcher)
                .First(x => x.Id == element.Id)
                .GetViewModel;
        }

        public ELibraryAuthorProfileViewModel? InsertOrUpdate(ELibraryAuthorProfileBindingModel model)
        {
            using var context = new ScientificActivityDatabase();

            var element = context.ELibraryAuthorProfiles
                .Include(x => x.Researcher)
                .FirstOrDefault(x => x.ResearcherId == model.ResearcherId);

            if (element == null)
            {
                var newElement = ELibraryAuthorProfile.Create(model);
                if (newElement == null)
                {
                    return null;
                }

                context.ELibraryAuthorProfiles.Add(newElement);
                context.SaveChanges();

                return context.ELibraryAuthorProfiles
                    .Include(x => x.Researcher)
                    .First(x => x.Id == newElement.Id)
                    .GetViewModel;
            }

            model.Id = element.Id;
            element.Update(model);
            context.SaveChanges();

            return context.ELibraryAuthorProfiles
                .Include(x => x.Researcher)
                .First(x => x.Id == element.Id)
                .GetViewModel;
        }
    }
}
