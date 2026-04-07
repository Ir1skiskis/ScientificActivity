using Microsoft.Extensions.Logging;
using ScientificActivityContracts.BindingModels;
using ScientificActivityContracts.BusinessLogicsContracts;
using ScientificActivityContracts.SearchModels;
using ScientificActivityContracts.StoragesContracts;
using ScientificActivityContracts.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScientificActivityBusinessLogics.BusinessLogics
{
    public class PublicationLogic : IPublicationLogic
    {
        private readonly ILogger _logger;
        private readonly IPublicationStorage _publicationStorage;
        private readonly IResearcherStorage _researcherStorage;
        private readonly IJournalStorage _journalStorage;
        private readonly IConferenceStorage _conferenceStorage;

        public PublicationLogic(
            ILogger<PublicationLogic> logger,
            IPublicationStorage publicationStorage,
            IResearcherStorage researcherStorage,
            IJournalStorage journalStorage,
            IConferenceStorage conferenceStorage)
        {
            _logger = logger;
            _publicationStorage = publicationStorage;
            _researcherStorage = researcherStorage;
            _journalStorage = journalStorage;
            _conferenceStorage = conferenceStorage;
        }

        public List<PublicationViewModel>? ReadList(PublicationSearchModel? model)
        {
            _logger.LogInformation("ReadList. Publication. Id:{Id}, ResearcherId:{ResearcherId}, Title:{Title}",
                model?.Id, model?.ResearcherId, model?.Title);

            var list = model == null
                ? _publicationStorage.GetFullList()
                : _publicationStorage.GetFilteredList(model);

            if (list == null)
            {
                _logger.LogWarning("ReadList returned null list");
                return null;
            }

            _logger.LogInformation("ReadList. Count:{Count}", list.Count);
            return list;
        }

        public PublicationViewModel? ReadElement(PublicationSearchModel? model)
        {
            if (model == null)
            {
                throw new ArgumentNullException(nameof(model));
            }

            _logger.LogInformation("ReadElement. Publication. Id:{Id}, Doi:{Doi}", model.Id, model.Doi);

            var element = _publicationStorage.GetElement(model);
            if (element == null)
            {
                _logger.LogWarning("ReadElement. Publication not found");
                return null;
            }

            _logger.LogInformation("ReadElement. Found publication Id:{Id}", element.Id);
            return element;
        }

        public bool Create(PublicationBindingModel model)
        {
            CheckModel(model);

            if (_publicationStorage.Insert(model) == null)
            {
                _logger.LogWarning("Insert operation failed");
                return false;
            }

            return true;
        }

        public bool Update(PublicationBindingModel model)
        {
            CheckModel(model);

            if (_publicationStorage.Update(model) == null)
            {
                _logger.LogWarning("Update operation failed");
                return false;
            }

            return true;
        }

        public bool Delete(PublicationBindingModel model)
        {
            CheckModel(model, false);

            if (_publicationStorage.Delete(model) == null)
            {
                _logger.LogWarning("Delete operation failed");
                return false;
            }

            return true;
        }

        private void CheckModel(PublicationBindingModel model, bool withParams = true)
        {
            if (model == null)
            {
                throw new ArgumentNullException(nameof(model));
            }

            if (!withParams)
            {
                return;
            }

            if (string.IsNullOrWhiteSpace(model.Title))
            {
                throw new ArgumentNullException(nameof(model.Title), "Не указано название публикации");
            }

            if (string.IsNullOrWhiteSpace(model.Authors))
            {
                throw new ArgumentNullException(nameof(model.Authors), "Не указаны авторы публикации");
            }

            if (model.Year < 1900 || model.Year > DateTime.Now.Year + 1)
            {
                throw new ArgumentException("Указан некорректный год публикации", nameof(model.Year));
            }

            if (model.ResearcherId <= 0)
            {
                throw new ArgumentException("Не указан исследователь", nameof(model.ResearcherId));
            }

            var researcher = _researcherStorage.GetElement(new ResearcherSearchModel
            {
                Id = model.ResearcherId
            });

            if (researcher == null)
            {
                throw new InvalidOperationException("Указанный исследователь не найден");
            }

            if (model.JournalId.HasValue && model.JournalId.Value > 0)
            {
                var journal = _journalStorage.GetElement(new JournalSearchModel
                {
                    Id = model.JournalId.Value
                });

                if (journal == null)
                {
                    throw new InvalidOperationException("Указанный журнал не найден");
                }
            }

            if (model.ConferenceId.HasValue && model.ConferenceId.Value > 0)
            {
                var conference = _conferenceStorage.GetElement(new ConferenceSearchModel
                {
                    Id = model.ConferenceId.Value
                });

                if (conference == null)
                {
                    throw new InvalidOperationException("Указанная конференция не найдена");
                }
            }

            if (model.JournalId.HasValue && model.ConferenceId.HasValue)
            {
                throw new InvalidOperationException("Публикация не может одновременно относиться и к журналу, и к конференции");
            }

            model.Title = model.Title.Trim();
            model.Authors = model.Authors.Trim();
            model.Doi = string.IsNullOrWhiteSpace(model.Doi) ? null : model.Doi.Trim();
            model.Url = string.IsNullOrWhiteSpace(model.Url) ? null : model.Url.Trim();
            model.Keywords = string.IsNullOrWhiteSpace(model.Keywords) ? null : model.Keywords.Trim();
            model.Annotation = string.IsNullOrWhiteSpace(model.Annotation) ? null : model.Annotation.Trim();

            if (!string.IsNullOrWhiteSpace(model.Doi))
            {
                var existingByDoi = _publicationStorage.GetElement(new PublicationSearchModel
                {
                    Doi = model.Doi
                });

                if (existingByDoi != null && existingByDoi.Id != model.Id)
                {
                    throw new InvalidOperationException("Публикация с таким DOI уже существует");
                }
            }

            _logger.LogInformation("Publication check. Title:{Title}, ResearcherId:{ResearcherId}, Id:{Id}",
                model.Title, model.ResearcherId, model.Id);
        }
    }
}
