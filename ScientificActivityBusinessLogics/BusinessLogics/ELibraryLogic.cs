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
        private readonly IPublicationStorage _publicationStorage;

        public ELibraryLogic(
            ILogger<ELibraryLogic> logger,
            IELibraryParser eLibraryParser,
            IResearcherStorage researcherStorage,
            IPublicationStorage publicationStorage)
        {
            _logger = logger;
            _eLibraryParser = eLibraryParser;
            _researcherStorage = researcherStorage;
            _publicationStorage = publicationStorage;
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

        //public int ImportAuthorPublications(ELibraryImportBindingModel model)
        //{
        //    _logger.LogInformation("ELibrary.ImportAuthorPublications. ResearcherId:{ResearcherId}", model.ResearcherId);

        //    var researcher = GetResearcherForImport(model);

        //    if (researcher == null)
        //    {
        //        throw new Exception("Исследователь не найден");
        //    }

        //    if (string.IsNullOrWhiteSpace(researcher.ELibraryAuthorId))
        //    {
        //        throw new Exception("У исследователя не указан ELibraryAuthorId");
        //    }

        //    //var publications = _eLibraryParser.GetAuthorPublications(researcher.ELibraryAuthorId);
        //    //if (publications.Count == 0)
        //    //{
        //    //    return 0;
        //    //}

        //    var existingPublications = _publicationStorage.GetFilteredList(new PublicationSearchModel
        //    {
        //        ResearcherId = researcher.Id
        //    });

        //    var existingKeys = new HashSet<string>(
        //        existingPublications.Select(x => BuildPublicationKey(x.Title, x.Year)),
        //        StringComparer.OrdinalIgnoreCase);

        //    var importedCount = 0;
        //    foreach (var publication in publications)
        //    {
        //        if (string.IsNullOrWhiteSpace(publication.Title))
        //        {
        //            continue;
        //        }

        //        var publicationYear = publication.Year ?? DateTime.Now.Year;
        //        var key = BuildPublicationKey(publication.Title, publicationYear);
        //        if (existingKeys.Contains(key))
        //        {
        //            continue;
        //        }

        //        var insertModel = new PublicationBindingModel
        //        {
        //            Title = publication.Title,
        //            Authors = string.IsNullOrWhiteSpace(publication.Authors)
        //                ? $"{researcher.LastName} {researcher.FirstName} {researcher.MiddleName}".Trim()
        //                : publication.Authors,
        //            Year = publicationYear,
        //            PublicationDate = null,
        //            Type = ScientificActivityDataModels.Enums.PublicationType.Статья_в_журнале,
        //            Doi = null,
        //            Url = publication.Url,
        //            JournalId = null,
        //            ConferenceId = null,
        //            ResearcherId = researcher.Id,
        //            Keywords = publication.Keywords,
        //            Annotation = publication.Annotation
        //        };

        //        if (_publicationStorage.Insert(insertModel) != null)
        //        {
        //            existingKeys.Add(key);
        //            importedCount++;
        //        }
        //    }

        //    return importedCount;
        //}

        private static string BuildPublicationKey(string title, int year)
        {
            return $"{title.Trim().ToUpperInvariant()}|{year}";
        }

        private ResearcherViewModel? GetResearcherForImport(ELibraryImportBindingModel model)
        {
            if (model == null)
            {
                throw new ArgumentNullException(nameof(model));
            }

            ResearcherViewModel? researcher = null;

            if (model.ResearcherId > 0)
            {
                researcher = _researcherStorage.GetElement(new ResearcherSearchModel
                {
                    Id = model.ResearcherId
                });

                if (researcher == null)
                {
                    researcher = _researcherStorage.GetElement(new ResearcherSearchModel
                    {
                        ELibraryAuthorId = model.ResearcherId.ToString()
                    });
                }
            }

            if (researcher == null && !string.IsNullOrWhiteSpace(model.ELibraryAuthorId))
            {
                researcher = _researcherStorage.GetElement(new ResearcherSearchModel
                {
                    ELibraryAuthorId = model.ELibraryAuthorId.Trim()
                });
            }

            return researcher;
        }

        public bool ImportAuthorProfile(ELibraryImportBindingModel model)
        {
            _logger.LogInformation("ELibrary.ImportAuthorProfile. ResearcherId:{ResearcherId}", model.ResearcherId);

            var researcher = GetResearcherForImport(model);

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
