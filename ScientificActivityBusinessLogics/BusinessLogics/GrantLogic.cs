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
    public class GrantLogic : IGrantLogic
    {
        private readonly ILogger _logger;
        private readonly IGrantStorage _grantStorage;

        public GrantLogic(ILogger<GrantLogic> logger, IGrantStorage grantStorage)
        {
            _logger = logger;
            _grantStorage = grantStorage;
        }

        public List<GrantViewModel>? ReadList(GrantSearchModel? model)
        {
            _logger.LogInformation("ReadList. Grant. Id:{Id}, Title:{Title}, Organization:{Organization}",
                model?.Id, model?.Title, model?.Organization);

            var list = model == null
                ? _grantStorage.GetFullList()
                : _grantStorage.GetFilteredList(model);

            if (list == null)
            {
                _logger.LogWarning("ReadList returned null list");
                return null;
            }

            _logger.LogInformation("ReadList. Count:{Count}", list.Count);
            return list;
        }

        public GrantViewModel? ReadElement(GrantSearchModel? model)
        {
            if (model == null)
            {
                throw new ArgumentNullException(nameof(model));
            }

            _logger.LogInformation(
                "ReadElement. Grant search. Id:{Id}, Title:{Title}, ContestNumber:{ContestNumber}, Organization:{Organization}",
                model.Id,
                model.Title,
                model.ContestNumber,
                model.Organization);

            var element = _grantStorage.GetElement(model);
            if (element == null)
            {
                _logger.LogWarning("ReadElement. Grant not found");
                return null;
            }

            _logger.LogInformation(
                "ReadElement result. Grant. Id:{Id}, Title:{Title}, ContestNumber:{ContestNumber}",
                element?.Id,
                element?.Title,
                element?.ContestNumber);
            return element;
        }

        public bool Create(GrantBindingModel model)
        {
            CheckModel(model);

            if (_grantStorage.Insert(model) == null)
            {
                _logger.LogWarning("Insert operation failed");
                return false;
            }

            return true;
        }

        public bool Update(GrantBindingModel model)
        {
            CheckModel(model);

            if (_grantStorage.Update(model) == null)
            {
                _logger.LogWarning("Update operation failed");
                return false;
            }

            return true;
        }

        public bool Delete(GrantBindingModel model)
        {
            CheckModel(model, false);

            if (_grantStorage.Delete(model) == null)
            {
                _logger.LogWarning("Delete operation failed");
                return false;
            }

            return true;
        }

        private void CheckModel(GrantBindingModel model, bool withParams = true)
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
                throw new ArgumentNullException(nameof(model.Title), "Не указано название гранта");
            }

            if (string.IsNullOrWhiteSpace(model.Organization))
            {
                throw new ArgumentNullException(nameof(model.Organization), "Не указана организация");
            }

            if (model.StartDate == default)
            {
                throw new ArgumentException("Не указана дата начала конкурса", nameof(model.StartDate));
            }

            if (model.EndDate == default)
            {
                throw new ArgumentException("Не указана дата окончания конкурса", nameof(model.EndDate));
            }

            if (model.EndDate < model.StartDate)
            {
                throw new ArgumentException("Дата окончания не может быть раньше даты начала");
            }

            if (model.Amount.HasValue && model.Amount.Value < 0)
            {
                throw new ArgumentException("Сумма гранта не может быть отрицательной", nameof(model.Amount));
            }

            if (string.IsNullOrWhiteSpace(model.ContestNumber))
            {
                throw new ArgumentNullException(nameof(model.ContestNumber), "Не указан номер конкурса");
            }

            var existingByContestNumber = _grantStorage.GetElement(new GrantSearchModel
            {
                ContestNumber = model.ContestNumber
            });

            if (existingByContestNumber != null && existingByContestNumber.Id != model.Id)
            {
                throw new InvalidOperationException("Грант с таким номером конкурса уже существует");
            }

            model.Title = model.Title.Trim();
            model.Description = string.IsNullOrWhiteSpace(model.Description) ? null : model.Description.Trim();
            model.Organization = model.Organization.Trim();
            model.Currency = string.IsNullOrWhiteSpace(model.Currency) ? null : model.Currency.Trim();
            model.SubjectArea = string.IsNullOrWhiteSpace(model.SubjectArea) ? null : model.SubjectArea.Trim();
            model.Url = string.IsNullOrWhiteSpace(model.Url) ? null : model.Url.Trim();

            _logger.LogInformation(
                "Grant check. Id:{Id}, ContestNumber:{ContestNumber}, Title:{Title}, Organization:{Organization}",
                model.Id,
                model.ContestNumber,
                model.Title,
                model.Organization);
        }
    }
}
