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
    public class ConferenceLogic : IConferenceLogic
    {
        private readonly ILogger _logger;
        private readonly IConferenceStorage _conferenceStorage;

        public ConferenceLogic(ILogger<ConferenceLogic> logger, IConferenceStorage conferenceStorage)
        {
            _logger = logger;
            _conferenceStorage = conferenceStorage;
        }

        public List<ConferenceViewModel>? ReadList(ConferenceSearchModel? model)
        {
            _logger.LogInformation("ReadList. Conference. Id:{Id}, Title:{Title}",
                model?.Id, model?.Title);

            var list = model == null
                ? _conferenceStorage.GetFullList()
                : _conferenceStorage.GetFilteredList(model);

            if (list == null)
            {
                _logger.LogWarning("ReadList returned null list");
                return null;
            }

            _logger.LogInformation("ReadList. Count:{Count}", list.Count);
            return list;
        }

        public ConferenceViewModel? ReadElement(ConferenceSearchModel? model)
        {
            if (model == null)
            {
                throw new ArgumentNullException(nameof(model));
            }

            _logger.LogInformation("ReadElement. Conference. Id:{Id}, Title:{Title}",
                model.Id, model.Title);

            var element = _conferenceStorage.GetElement(model);
            if (element == null)
            {
                _logger.LogWarning("ReadElement. Conference not found");
                return null;
            }

            _logger.LogInformation("ReadElement. Found conference Id:{Id}", element.Id);
            return element;
        }

        public bool Create(ConferenceBindingModel model)
        {
            CheckModel(model);

            if (_conferenceStorage.Insert(model) == null)
            {
                _logger.LogWarning("Insert operation failed");
                return false;
            }

            return true;
        }

        public bool Update(ConferenceBindingModel model)
        {
            CheckModel(model);

            if (_conferenceStorage.Update(model) == null)
            {
                _logger.LogWarning("Update operation failed");
                return false;
            }

            return true;
        }

        public bool Delete(ConferenceBindingModel model)
        {
            CheckModel(model, false);

            if (_conferenceStorage.Delete(model) == null)
            {
                _logger.LogWarning("Delete operation failed");
                return false;
            }

            return true;
        }

        private void CheckModel(ConferenceBindingModel model, bool withParams = true)
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
                throw new ArgumentNullException(nameof(model.Title), "Не указано название конференции");
            }

            if (model.StartDate == default)
            {
                throw new ArgumentException("Не указана дата начала конференции", nameof(model.StartDate));
            }

            if (model.EndDate == default)
            {
                throw new ArgumentException("Не указана дата окончания конференции", nameof(model.EndDate));
            }

            if (model.EndDate < model.StartDate)
            {
                throw new ArgumentException("Дата окончания не может быть раньше даты начала");
            }

            model.Title = model.Title.Trim();
            model.Description = string.IsNullOrWhiteSpace(model.Description) ? null : model.Description.Trim();
            model.City = string.IsNullOrWhiteSpace(model.City) ? null : model.City.Trim();
            model.Country = string.IsNullOrWhiteSpace(model.Country) ? null : model.Country.Trim();
            model.Organizer = string.IsNullOrWhiteSpace(model.Organizer) ? null : model.Organizer.Trim();
            model.SubjectArea = string.IsNullOrWhiteSpace(model.SubjectArea) ? null : model.SubjectArea.Trim();
            model.Url = string.IsNullOrWhiteSpace(model.Url) ? null : model.Url.Trim();

            _logger.LogInformation("Conference check. Title:{Title}, StartDate:{StartDate}, EndDate:{EndDate}, Id:{Id}",
                model.Title, model.StartDate, model.EndDate, model.Id);
        }
    }
}
