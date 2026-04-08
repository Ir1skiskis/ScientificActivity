using Microsoft.Extensions.Logging;
using ScientificActivityContracts.BindingModels;
using ScientificActivityContracts.BusinessLogicsContracts;
using ScientificActivityContracts.SearchModels;
using ScientificActivityContracts.StoragesContracts;
using ScientificActivityContracts.ViewModels;
using ScientificActivityParsers.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScientificActivityBusinessLogics.BusinessLogics
{
    public class ELibraryLogic : IELibraryLogic
    {
        private readonly ILogger<ELibraryLogic> _logger;
        private readonly IELibraryParser _eLibraryParser;
        private readonly IResearcherStorage _researcherStorage;

        public ELibraryLogic(
            ILogger<ELibraryLogic> logger,
            IELibraryParser eLibraryParser,
            IResearcherStorage researcherStorage)
        {
            _logger = logger;
            _eLibraryParser = eLibraryParser;
            _researcherStorage = researcherStorage;
        }

        public List<ELibraryAuthorSearchViewModel> SearchAuthors(ELibraryAuthorSearchBindingModel model)
        {
            _logger.LogInformation(
                "ELibrary.SearchAuthors. LastName:{LastName}, FirstName:{FirstName}, MiddleName:{MiddleName}",
                model.LastName,
                model.FirstName,
                model.MiddleName);

            return _eLibraryParser.SearchAuthors(model);
        }

        public ELibraryAuthorProfileViewModel? GetAuthorProfile(string authorId)
        {
            _logger.LogInformation("ELibrary.GetAuthorProfile. AuthorId:{AuthorId}", authorId);

            return _eLibraryParser.GetAuthorProfile(authorId);
        }

        public bool BindAuthorToResearcher(ELibraryBindAuthorBindingModel model)
        {
            _logger.LogInformation(
                "ELibrary.BindAuthorToResearcher. ResearcherId:{ResearcherId}, AuthorId:{AuthorId}",
                model.ResearcherId,
                model.AuthorId);

            var researcher = _researcherStorage.GetElement(new ResearcherSearchModel
            {
                Id = model.ResearcherId
            });

            if (researcher == null)
            {
                throw new Exception("Исследователь не найден");
            }

            var researcherWithSameAuthorId = _researcherStorage.GetElement(new ResearcherSearchModel
            {
                ELibraryAuthorId = model.AuthorId
            });

            if (researcherWithSameAuthorId != null && researcherWithSameAuthorId.Id != model.ResearcherId)
            {
                throw new Exception("Этот профиль eLibrary уже привязан к другому исследователю");
            }

            var updateModel = new ResearcherBindingModel
            {
                Id = researcher.Id,
                Email = researcher.Email,
                PasswordHash = researcher.PasswordHash,
                Role = researcher.Role,
                IsActive = researcher.IsActive,
                LastName = researcher.LastName,
                FirstName = researcher.FirstName,
                MiddleName = researcher.MiddleName,
                Phone = researcher.Phone,
                Department = researcher.Department,
                Position = researcher.Position,
                AcademicDegree = researcher.AcademicDegree,
                ELibraryAuthorId = model.AuthorId,
                ResearchTopics = researcher.ResearchTopics
            };

            return _researcherStorage.Update(updateModel) != null;
        }

        public bool ImportAuthorProfile(ELibraryImportBindingModel model)
        {
            _logger.LogInformation("ELibrary.ImportAuthorProfile. ResearcherId:{ResearcherId}", model.ResearcherId);

            var researcher = _researcherStorage.GetElement(new ResearcherSearchModel
            {
                Id = model.ResearcherId
            });

            if (researcher == null)
            {
                throw new Exception("Исследователь не найден");
            }

            if (string.IsNullOrWhiteSpace(researcher.ELibraryAuthorId))
            {
                throw new Exception("У исследователя не указан ELibraryAuthorId");
            }

            var authorProfile = _eLibraryParser.GetAuthorProfile(researcher.ELibraryAuthorId);
            if (authorProfile == null)
            {
                throw new Exception("Не удалось получить профиль автора из eLibrary");
            }

            var updateModel = new ResearcherBindingModel
            {
                Id = researcher.Id,
                Email = researcher.Email,
                PasswordHash = researcher.PasswordHash,
                Role = researcher.Role,
                IsActive = researcher.IsActive,
                LastName = researcher.LastName,
                FirstName = researcher.FirstName,
                MiddleName = researcher.MiddleName,
                Phone = researcher.Phone,
                Department = researcher.Department,
                Position = researcher.Position,
                AcademicDegree = researcher.AcademicDegree,
                ELibraryAuthorId = researcher.ELibraryAuthorId,
                ResearchTopics = string.IsNullOrWhiteSpace(authorProfile.ResearchTopics)
                    ? researcher.ResearchTopics
                    : authorProfile.ResearchTopics
            };

            return _researcherStorage.Update(updateModel) != null;
        }
    }
}
