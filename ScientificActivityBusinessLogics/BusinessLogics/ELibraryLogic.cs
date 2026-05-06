using Microsoft.Extensions.Logging;
using ScientificActivityContracts.BindingModels;
using ScientificActivityContracts.BusinessLogicsContracts;
using ScientificActivityContracts.SearchModels;
using ScientificActivityContracts.StoragesContracts;
using ScientificActivityContracts.ViewModels;
using ScientificActivityParsers.Interfaces;
using ScientificActivityParsers.Models;
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
        private readonly IELibraryAuthorProfileStorage _eLibraryAuthorProfileStorage;
        private readonly IJournalStorage _journalStorage;

        public ELibraryLogic(
            ILogger<ELibraryLogic> logger,
            IELibraryParser eLibraryParser,
            IResearcherStorage researcherStorage,
            IPublicationStorage publicationStorage,
            IELibraryAuthorProfileStorage eLibraryAuthorProfileStorage,
            IJournalStorage journalStorage)
        {
            _logger = logger;
            _eLibraryParser = eLibraryParser;
            _researcherStorage = researcherStorage;
            _publicationStorage = publicationStorage;
            _journalStorage = journalStorage;
            _eLibraryAuthorProfileStorage = eLibraryAuthorProfileStorage;
        }

        public ELibraryAuthorProfileViewModel? GetStoredAuthorProfile(int researcherId)
        {
            _logger.LogInformation("ELibrary.GetStoredAuthorProfile. ResearcherId:{ResearcherId}", researcherId);

            return _eLibraryAuthorProfileStorage.GetByResearcherId(researcherId);
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

        public int ImportAuthorPublications(ELibraryImportBindingModel model)
        {
            _logger.LogInformation("ELibrary.ImportAuthorPublications. ResearcherId:{ResearcherId}", model.ResearcherId);

            var researcher = GetResearcherForImport(model);

            if (researcher == null)
            {
                throw new Exception("Исследователь не найден");
            }

            if (string.IsNullOrWhiteSpace(researcher.ELibraryAuthorId))
            {
                throw new Exception("У исследователя не указан ELibraryAuthorId");
            }

            var publications = _eLibraryParser.GetAuthorPublications(researcher.ELibraryAuthorId);

            _logger.LogInformation(
                "ELibrary.ImportAuthorPublications. Parsed publications count:{Count}",
                publications.Count);

            if (publications.Count == 0)
            {
                return 0;
            }

            try
            {
                var categoryInfo = _eLibraryParser.GetAuthorPublicationCategoryInfo(researcher.ELibraryAuthorId);

                _logger.LogInformation(
                    "ELibrary.ImportAuthorPublications. Parsed category groups count:{Count}",
                    categoryInfo.PublicationIdsByCategory.Count);

                foreach (var category in categoryInfo.PublicationIdsByCategory)
                {
                    _logger.LogInformation(
                        "ELibrary category {Category}. Publications count:{Count}",
                        category.Key,
                        category.Value.Count);
                }

                ApplyCategoriesToPublications(publications, categoryInfo);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Не удалось получить категории публикаций eLibrary. Импорт продолжится без категорий.");
            }

            var existingPublications = _publicationStorage.GetFilteredList(new PublicationSearchModel
            {
                ResearcherId = researcher.Id
            });

            var existingByELibraryId = existingPublications
                .Where(x => !string.IsNullOrWhiteSpace(x.ELibraryId))
                .GroupBy(x => x.ELibraryId!)
                .ToDictionary(x => x.Key, x => x.First(), StringComparer.OrdinalIgnoreCase);

            var existingByTitleYear = existingPublications
                .Where(x => !string.IsNullOrWhiteSpace(x.Title))
                .GroupBy(x => BuildPublicationKey(x.Title, x.Year))
                .ToDictionary(x => x.Key, x => x.First(), StringComparer.OrdinalIgnoreCase);

            var processedCount = 0;
            var insertedCount = 0;
            var updatedCount = 0;
            var skippedCount = 0;

            foreach (var publication in publications)
            {
                if (string.IsNullOrWhiteSpace(publication.Title))
                {
                    skippedCount++;
                    continue;
                }

                var publicationYear = publication.Year ?? DateTime.Now.Year;

                PublicationViewModel? existing = null;

                if (!string.IsNullOrWhiteSpace(publication.ELibraryId) &&
                    existingByELibraryId.TryGetValue(publication.ELibraryId, out var existingById))
                {
                    existing = existingById;
                }
                else
                {
                    var key = BuildPublicationKey(publication.Title, publicationYear);
                    if (existingByTitleYear.TryGetValue(key, out var existingByKey))
                    {
                        existing = existingByKey;
                    }
                }

                var journalId = TryGetOrCreateJournal(publication);

                var bindingModel = new PublicationBindingModel
                {
                    Id = existing?.Id ?? 0,

                    Title = publication.Title,

                    Authors = string.IsNullOrWhiteSpace(publication.Authors)
                        ? $"{researcher.LastName} {researcher.FirstName} {researcher.MiddleName}".Trim()
                        : publication.Authors,

                    Year = publicationYear,
                    PublicationDate = null,

                    Type = MapPublicationType(publication),

                    Doi = string.IsNullOrWhiteSpace(publication.Doi) ? existing?.Doi : publication.Doi,
                    Url = string.IsNullOrWhiteSpace(publication.Url) ? existing?.Url : publication.Url,

                    JournalId = journalId ?? existing?.JournalId,
                    ConferenceId = existing?.ConferenceId,

                    ResearcherId = researcher.Id,

                    Keywords = string.IsNullOrWhiteSpace(publication.Keywords) ? existing?.Keywords : publication.Keywords,
                    Annotation = string.IsNullOrWhiteSpace(publication.Annotation) ? existing?.Annotation : publication.Annotation,

                    CitationsRincCount = publication.CitationsRincCount ?? existing?.CitationsRincCount ?? 0,
                    ELibraryId = string.IsNullOrWhiteSpace(publication.ELibraryId) ? existing?.ELibraryId : publication.ELibraryId,

                    IsInRinc = publication.IsInRinc || existing?.IsInRinc == true,
                    IsInCoreRinc = publication.IsInCoreRinc || existing?.IsInCoreRinc == true,

                    IsWhiteListLevel1 = publication.IsWhiteListLevel1 || existing?.IsWhiteListLevel1 == true,
                    IsWhiteListLevel2 = publication.IsWhiteListLevel2 || existing?.IsWhiteListLevel2 == true,
                    IsWhiteListLevel3 = publication.IsWhiteListLevel3 || existing?.IsWhiteListLevel3 == true,
                    IsWhiteListLevel4 = publication.IsWhiteListLevel4 || existing?.IsWhiteListLevel4 == true,

                    IsRsci = publication.IsRsci || existing?.IsRsci == true,

                    IsScopusQ1 = publication.IsScopusQ1 || existing?.IsScopusQ1 == true,
                    IsScopusQ2 = publication.IsScopusQ2 || existing?.IsScopusQ2 == true,
                    IsScopusQ3 = publication.IsScopusQ3 || existing?.IsScopusQ3 == true,
                    IsScopusQ4 = publication.IsScopusQ4 || existing?.IsScopusQ4 == true,

                    IsWebOfScienceQ1 = publication.IsWebOfScienceQ1 || existing?.IsWebOfScienceQ1 == true,
                    IsWebOfScienceQ2 = publication.IsWebOfScienceQ2 || existing?.IsWebOfScienceQ2 == true,
                    IsWebOfScienceQ3 = publication.IsWebOfScienceQ3 || existing?.IsWebOfScienceQ3 == true,
                    IsWebOfScienceQ4 = publication.IsWebOfScienceQ4 || existing?.IsWebOfScienceQ4 == true,
                    IsWebOfScienceNoQuartile = publication.IsWebOfScienceNoQuartile || existing?.IsWebOfScienceNoQuartile == true,

                    IsVak = publication.IsVak || existing?.IsVak == true,
                    IsVakCategory1 = publication.IsVakCategory1 || existing?.IsVakCategory1 == true,
                    IsVakCategory2 = publication.IsVakCategory2 || existing?.IsVakCategory2 == true,
                    IsVakCategory3 = publication.IsVakCategory3 || existing?.IsVakCategory3 == true,

                    RubricOecd = string.IsNullOrWhiteSpace(publication.RubricOecd) ? existing?.RubricOecd : publication.RubricOecd,
                    RubricAsjc = string.IsNullOrWhiteSpace(publication.RubricAsjc) ? existing?.RubricAsjc : publication.RubricAsjc,
                    RubricGrnti = string.IsNullOrWhiteSpace(publication.RubricGrnti) ? existing?.RubricGrnti : publication.RubricGrnti,
                    VakSpecialty = string.IsNullOrWhiteSpace(publication.VakSpecialty) ? existing?.VakSpecialty : publication.VakSpecialty
                };

                if (bindingModel.IsInCoreRinc)
                {
                    bindingModel.IsInRinc = true;
                }

                if (bindingModel.IsVakCategory1 || bindingModel.IsVakCategory2 || bindingModel.IsVakCategory3)
                {
                    bindingModel.IsVak = true;
                }

                if (existing == null)
                {
                    var inserted = _publicationStorage.Insert(bindingModel);

                    if (inserted != null)
                    {
                        insertedCount++;
                        processedCount++;

                        if (!string.IsNullOrWhiteSpace(inserted.ELibraryId))
                        {
                            existingByELibraryId[inserted.ELibraryId] = inserted;
                        }

                        existingByTitleYear[BuildPublicationKey(inserted.Title, inserted.Year)] = inserted;
                    }
                }
                else
                {
                    var updated = _publicationStorage.Update(bindingModel);

                    if (updated != null)
                    {
                        updatedCount++;
                        processedCount++;
                    }
                }
            }

            _logger.LogInformation(
                "ELibrary.ImportAuthorPublications finished. Parsed:{Parsed}, Inserted:{Inserted}, Updated:{Updated}, Skipped:{Skipped}, Processed:{Processed}",
                publications.Count,
                insertedCount,
                updatedCount,
                skippedCount,
                processedCount);

            return processedCount;
        }

        private int? TryGetOrCreateJournal(ScientificActivityParsers.Models.ELibraryPublicationImportModel publication)
        {
            if (string.IsNullOrWhiteSpace(publication.JournalTitle))
            {
                return null;
            }

            JournalViewModel? existingJournal = null;

            if (!string.IsNullOrWhiteSpace(publication.JournalIssn))
            {
                existingJournal = _journalStorage.GetElement(new JournalSearchModel
                {
                    Issn = publication.JournalIssn
                });
            }

            if (existingJournal == null)
            {
                existingJournal = _journalStorage.GetElement(new JournalSearchModel
                {
                    Title = publication.JournalTitle
                });
            }

            if (existingJournal != null)
            {
                return existingJournal.Id;
            }

            var created = _journalStorage.Insert(new JournalBindingModel
            {
                Title = publication.JournalTitle,
                Issn = publication.JournalIssn,
                EIssn = null,
                Publisher = null,
                SubjectArea = publication.RubricOecd ?? publication.RubricGrnti ?? publication.RubricAsjc,
                IsVak = publication.IsVak,
                IsWhiteList = publication.IsWhiteListLevel1 ||
                              publication.IsWhiteListLevel2 ||
                              publication.IsWhiteListLevel3 ||
                              publication.IsWhiteListLevel4,
                WhiteListLevel2023 = null,
                WhiteListLevel2025 = GetWhiteListLevel2025(publication),
                WhiteListState = null,
                WhiteListNotice = null,
                WhiteListAcceptedDate = null,
                WhiteListDiscontinuedDate = null,
                Country = null,
                Url = null,
                RcsiRecordSourceId = null
            });

            return created?.Id;
        }

        private static ScientificActivityDataModels.Enums.PublicationType MapPublicationType(
    ScientificActivityParsers.Models.ELibraryPublicationImportModel publication)
        {
            return !string.IsNullOrWhiteSpace(publication.JournalTitle)
                ? ScientificActivityDataModels.Enums.PublicationType.Статья_в_журнале
                : ScientificActivityDataModels.Enums.PublicationType.Статья_в_сборнике_конференции;
        }

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

            var profileModel = MapProfileToBindingModel(researcher.Id, authorProfile);
            var savedProfile = _eLibraryAuthorProfileStorage.InsertOrUpdate(profileModel);

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

            var updatedResearcher = _researcherStorage.Update(updateModel);

            return savedProfile != null && updatedResearcher != null;
        }

        private static ELibraryAuthorProfileBindingModel MapProfileToBindingModel(int researcherId, ELibraryAuthorProfileViewModel profile)
        {
            return new ELibraryAuthorProfileBindingModel
            {
                ResearcherId = researcherId,
                AuthorId = profile.AuthorId,
                FullName = profile.FullName,
                Organization = profile.Organization,
                Department = profile.Department,
                SpinCode = profile.SpinCode,

                PublicationsCountElibrary = profile.PublicationsCountElibrary,
                PublicationsCountRinc = profile.PublicationsCountRinc,
                PublicationsCoreRincCount = profile.PublicationsCoreRincCount,

                CitationsCountElibrary = profile.CitationsCountElibrary,
                CitationsCountRinc = profile.CitationsCountRinc,
                CitationsCoreRincCount = profile.CitationsCoreRincCount,

                HIndexElibrary = profile.HIndexElibrary,
                HIndexRinc = profile.HIndexRinc,
                HIndexCoreRinc = profile.HIndexCoreRinc,
                HIndexWithoutSelfCitations = profile.HIndexWithoutSelfCitations,

                PublicationsCitingAuthorCount = profile.PublicationsCitingAuthorCount,
                MostCitedPublicationCitationsCount = profile.MostCitedPublicationCitationsCount,
                CitedPublicationsCount = profile.CitedPublicationsCount,
                AverageCitationsPerPublication = profile.AverageCitationsPerPublication,

                FirstPublicationYear = profile.FirstPublicationYear,
                SelfCitationsCount = profile.SelfCitationsCount,
                CoauthorCitationsCount = profile.CoauthorCitationsCount,
                CoauthorsCount = profile.CoauthorsCount,

                ForeignArticlesCount = profile.ForeignArticlesCount,
                RussianArticlesCount = profile.RussianArticlesCount,
                VakArticlesCount = profile.VakArticlesCount,
                ImpactFactorArticlesCount = profile.ImpactFactorArticlesCount,

                ForeignJournalCitationsCount = profile.ForeignJournalCitationsCount,
                RussianJournalCitationsCount = profile.RussianJournalCitationsCount,
                VakJournalCitationsCount = profile.VakJournalCitationsCount,
                ImpactFactorJournalCitationsCount = profile.ImpactFactorJournalCitationsCount,

                AverageWeightedImpactFactorPublished = profile.AverageWeightedImpactFactorPublished,
                AverageWeightedImpactFactorCited = profile.AverageWeightedImpactFactorCited,

                PublicationsRincLast5YearsCount = profile.PublicationsRincLast5YearsCount,
                PublicationsCoreRincLast5YearsCount = profile.PublicationsCoreRincLast5YearsCount,
                CitationsRincLast5YearsCount = profile.CitationsRincLast5YearsCount,
                CitationsCoreRincLast5YearsCount = profile.CitationsCoreRincLast5YearsCount,
                CitationsAllLast5YearsCount = profile.CitationsAllLast5YearsCount,

                MainRubricGrnti = profile.MainRubricGrnti,
                MainRubricOecd = profile.MainRubricOecd,
                PercentileCoreRinc = profile.PercentileCoreRinc,

                PublicationsRincByYearJson = SerializeDictionary(profile.PublicationsRincByYear),
                PublicationsCoreRincByYearJson = SerializeDictionary(profile.PublicationsCoreRincByYear),
                CitationsRincByYearJson = SerializeDictionary(profile.CitationsRincByYear),
                CitationsCoreRincByYearJson = SerializeDictionary(profile.CitationsCoreRincByYear),
                HIndexRincByYearJson = SerializeDictionary(profile.HIndexRincByYear),
                HIndexCoreRincByYearJson = SerializeDictionary(profile.HIndexCoreRincByYear),
                PercentileCoreRincByYearJson = SerializeDictionary(profile.PercentileCoreRincByYear),
                PublicationsRinc5YearsByEndYearJson = SerializeDictionary(profile.PublicationsRinc5YearsByEndYear),
                PublicationsCoreRinc5YearsByEndYearJson = SerializeDictionary(profile.PublicationsCoreRinc5YearsByEndYear),
                CitationsRinc5YearsByEndYearJson = SerializeDictionary(profile.CitationsRinc5YearsByEndYear),
                CitationsCoreRinc5YearsByEndYearJson = SerializeDictionary(profile.CitationsCoreRinc5YearsByEndYear),

                ResearchTopics = profile.ResearchTopics,
                ImportedAt = DateTime.UtcNow
            };
        }

        private static string SerializeDictionary(Dictionary<int, int>? dictionary)
        {
            return System.Text.Json.JsonSerializer.Serialize(dictionary ?? new Dictionary<int, int>());
        }

        private static int? GetWhiteListLevel2025(ScientificActivityParsers.Models.ELibraryPublicationImportModel publication)
        {
            if (publication.IsWhiteListLevel1)
            {
                return 1;
            }

            if (publication.IsWhiteListLevel2)
            {
                return 2;
            }

            if (publication.IsWhiteListLevel3)
            {
                return 3;
            }

            if (publication.IsWhiteListLevel4)
            {
                return 4;
            }

            return null;
        }

        private static void ApplyCategoriesToPublications(
    List<ELibraryPublicationImportModel> publications,
    ELibraryPublicationCategoryInfoModel categoryInfo)
        {
            foreach (var publication in publications)
            {
                if (string.IsNullOrWhiteSpace(publication.ELibraryId))
                {
                    continue;
                }

                var isInRinc = IsPublicationInLoadedCategory(categoryInfo, "rinc", publication.ELibraryId);
                if (isInRinc.HasValue)
                {
                    publication.IsInRinc = isInRinc.Value;
                }

                var isInCoreRinc = IsPublicationInLoadedCategory(categoryInfo, "coreRinc", publication.ELibraryId);
                if (isInCoreRinc.HasValue)
                {
                    publication.IsInCoreRinc = isInCoreRinc.Value;
                }

                var isWhiteList1 = IsPublicationInLoadedCategory(categoryInfo, "whiteList1", publication.ELibraryId);
                if (isWhiteList1.HasValue)
                {
                    publication.IsWhiteListLevel1 = isWhiteList1.Value;
                }

                var isWhiteList2 = IsPublicationInLoadedCategory(categoryInfo, "whiteList2", publication.ELibraryId);
                if (isWhiteList2.HasValue)
                {
                    publication.IsWhiteListLevel2 = isWhiteList2.Value;
                }

                var isWhiteList3 = IsPublicationInLoadedCategory(categoryInfo, "whiteList3", publication.ELibraryId);
                if (isWhiteList3.HasValue)
                {
                    publication.IsWhiteListLevel3 = isWhiteList3.Value;
                }

                var isWhiteList4 = IsPublicationInLoadedCategory(categoryInfo, "whiteList4", publication.ELibraryId);
                if (isWhiteList4.HasValue)
                {
                    publication.IsWhiteListLevel4 = isWhiteList4.Value;
                }

                var isRsci = IsPublicationInLoadedCategory(categoryInfo, "rsci", publication.ELibraryId);
                if (isRsci.HasValue)
                {
                    publication.IsRsci = isRsci.Value;
                }

                var isScopusQ1 = IsPublicationInLoadedCategory(categoryInfo, "scopusQ1", publication.ELibraryId);
                if (isScopusQ1.HasValue)
                {
                    publication.IsScopusQ1 = isScopusQ1.Value;
                }

                var isScopusQ2 = IsPublicationInLoadedCategory(categoryInfo, "scopusQ2", publication.ELibraryId);
                if (isScopusQ2.HasValue)
                {
                    publication.IsScopusQ2 = isScopusQ2.Value;
                }

                var isScopusQ3 = IsPublicationInLoadedCategory(categoryInfo, "scopusQ3", publication.ELibraryId);
                if (isScopusQ3.HasValue)
                {
                    publication.IsScopusQ3 = isScopusQ3.Value;
                }

                var isScopusQ4 = IsPublicationInLoadedCategory(categoryInfo, "scopusQ4", publication.ELibraryId);
                if (isScopusQ4.HasValue)
                {
                    publication.IsScopusQ4 = isScopusQ4.Value;
                }

                var isWosQ1 = IsPublicationInLoadedCategory(categoryInfo, "wosQ1", publication.ELibraryId);
                if (isWosQ1.HasValue)
                {
                    publication.IsWebOfScienceQ1 = isWosQ1.Value;
                }

                var isWosQ2 = IsPublicationInLoadedCategory(categoryInfo, "wosQ2", publication.ELibraryId);
                if (isWosQ2.HasValue)
                {
                    publication.IsWebOfScienceQ2 = isWosQ2.Value;
                }

                var isWosQ3 = IsPublicationInLoadedCategory(categoryInfo, "wosQ3", publication.ELibraryId);
                if (isWosQ3.HasValue)
                {
                    publication.IsWebOfScienceQ3 = isWosQ3.Value;
                }

                var isWosQ4 = IsPublicationInLoadedCategory(categoryInfo, "wosQ4", publication.ELibraryId);
                if (isWosQ4.HasValue)
                {
                    publication.IsWebOfScienceQ4 = isWosQ4.Value;
                }

                var isWosNoQuartile = IsPublicationInLoadedCategory(categoryInfo, "wosNoQuartile", publication.ELibraryId);
                if (isWosNoQuartile.HasValue)
                {
                    publication.IsWebOfScienceNoQuartile = isWosNoQuartile.Value;
                }

                var isVak = IsPublicationInLoadedCategory(categoryInfo, "vak", publication.ELibraryId);
                if (isVak.HasValue)
                {
                    publication.IsVak = isVak.Value;
                }

                var isVak1 = IsPublicationInLoadedCategory(categoryInfo, "vak1", publication.ELibraryId);
                if (isVak1.HasValue)
                {
                    publication.IsVakCategory1 = isVak1.Value;
                }

                var isVak2 = IsPublicationInLoadedCategory(categoryInfo, "vak2", publication.ELibraryId);
                if (isVak2.HasValue)
                {
                    publication.IsVakCategory2 = isVak2.Value;
                }

                var isVak3 = IsPublicationInLoadedCategory(categoryInfo, "vak3", publication.ELibraryId);
                if (isVak3.HasValue)
                {
                    publication.IsVakCategory3 = isVak3.Value;
                }

                if (publication.IsInCoreRinc)
                {
                    publication.IsInRinc = true;
                }

                if (publication.IsVakCategory1 || publication.IsVakCategory2 || publication.IsVakCategory3)
                {
                    publication.IsVak = true;
                }
            }
        }

        private static bool IsPublicationInCategory(
            ELibraryPublicationCategoryInfoModel categoryInfo,
            string categoryName,
            string eLibraryId)
        {
            return categoryInfo.PublicationIdsByCategory.TryGetValue(categoryName, out var ids)
                   && ids.Contains(eLibraryId);
        }

        private static bool? IsPublicationInLoadedCategory(
    ELibraryPublicationCategoryInfoModel categoryInfo,
    string categoryKey,
    string? eLibraryId)
        {
            if (string.IsNullOrWhiteSpace(eLibraryId))
            {
                return null;
            }

            if (!categoryInfo.PublicationIdsByCategory.TryGetValue(categoryKey, out var ids))
            {
                return null;
            }

            return ids.Contains(eLibraryId);
        }
    }
}
