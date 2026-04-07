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
    public class ResearcherLogic : IResearcherLogic
    {
        private readonly ILogger _logger;
        private readonly IResearcherStorage _researcherStorage;

        public ResearcherLogic(ILogger<ResearcherLogic> logger, IResearcherStorage researcherStorage)
        {
            _logger = logger;
            _researcherStorage = researcherStorage;
        }

        public List<ResearcherViewModel>? ReadList(ResearcherSearchModel? model)
        {
            _logger.LogInformation("ReadList. Researcher. Id:{Id}, Email:{Email}, LastName:{LastName}",
                model?.Id, model?.Email, model?.LastName);

            var list = model == null
                ? _researcherStorage.GetFullList()
                : _researcherStorage.GetFilteredList(model);

            if (list == null)
            {
                _logger.LogWarning("ReadList returned null list");
                return null;
            }

            _logger.LogInformation("ReadList. Count:{Count}", list.Count);
            return list;
        }

        public ResearcherViewModel? ReadElement(ResearcherSearchModel? model)
        {
            if (model == null)
            {
                throw new ArgumentNullException(nameof(model));
            }

            _logger.LogInformation("ReadElement. Researcher. Id:{Id}, Email:{Email}", model.Id, model.Email);

            var element = _researcherStorage.GetElement(model);
            if (element == null)
            {
                _logger.LogWarning("ReadElement. Researcher not found");
                return null;
            }

            _logger.LogInformation("ReadElement. Found researcher Id:{Id}", element.Id);
            return element;
        }

        public bool Create(ResearcherBindingModel model)
        {
            CheckModel(model, isCreate: true);

            if (_researcherStorage.Insert(model) == null)
            {
                _logger.LogWarning("Insert operation failed");
                return false;
            }

            return true;
        }

        public bool Update(ResearcherBindingModel model)
        {
            CheckModel(model, isCreate: false);

            if (_researcherStorage.Update(model) == null)
            {
                _logger.LogWarning("Update operation failed");
                return false;
            }

            return true;
        }

        public bool Delete(ResearcherBindingModel model)
        {
            CheckModel(model, withParams: false);

            if (_researcherStorage.Delete(model) == null)
            {
                _logger.LogWarning("Delete operation failed");
                return false;
            }

            return true;
        }

        private void CheckModel(ResearcherBindingModel model, bool withParams = true, bool isCreate = false)
        {
            if (model == null)
            {
                throw new ArgumentNullException(nameof(model));
            }

            if (!withParams)
            {
                return;
            }

            if (string.IsNullOrWhiteSpace(model.Email))
            {
                throw new ArgumentNullException(nameof(model.Email), "Не указан email исследователя");
            }

            if (!IsValidEmail(model.Email))
            {
                throw new ArgumentException("Некорректный формат email", nameof(model.Email));
            }

            if (string.IsNullOrWhiteSpace(model.LastName))
            {
                throw new ArgumentNullException(nameof(model.LastName), "Не указана фамилия исследователя");
            }

            if (string.IsNullOrWhiteSpace(model.FirstName))
            {
                throw new ArgumentNullException(nameof(model.FirstName), "Не указано имя исследователя");
            }

            if (string.IsNullOrWhiteSpace(model.Phone))
            {
                throw new ArgumentNullException(nameof(model.Phone), "Не указан телефон исследователя");
            }

            if (NormalizePhone(model.Phone).Length < 10)
            {
                throw new ArgumentException("Телефон должен содержать не менее 10 цифр", nameof(model.Phone));
            }

            if (string.IsNullOrWhiteSpace(model.Department))
            {
                throw new ArgumentNullException(nameof(model.Department), "Не указана кафедра или подразделение");
            }

            if (string.IsNullOrWhiteSpace(model.Position))
            {
                throw new ArgumentNullException(nameof(model.Position), "Не указана должность исследователя");
            }

            if (isCreate && string.IsNullOrWhiteSpace(model.PasswordHash))
            {
                throw new ArgumentNullException(nameof(model.PasswordHash), "Не указан пароль");
            }

            model.Email = model.Email.Trim();
            model.Phone = NormalizePhone(model.Phone);
            model.LastName = model.LastName.Trim();
            model.FirstName = model.FirstName.Trim();
            model.MiddleName = string.IsNullOrWhiteSpace(model.MiddleName) ? null : model.MiddleName.Trim();
            model.Department = model.Department.Trim();
            model.Position = model.Position.Trim();
            model.ELibraryAuthorId = string.IsNullOrWhiteSpace(model.ELibraryAuthorId) ? null : model.ELibraryAuthorId.Trim();
            model.ResearchTopics = string.IsNullOrWhiteSpace(model.ResearchTopics) ? null : model.ResearchTopics.Trim();
            model.PasswordHash = string.IsNullOrWhiteSpace(model.PasswordHash) ? string.Empty : model.PasswordHash.Trim();

            var existingByEmail = _researcherStorage.GetElement(new ResearcherSearchModel
            {
                Email = model.Email
            });

            if (existingByEmail != null && existingByEmail.Id != model.Id)
            {
                throw new InvalidOperationException("Исследователь с таким email уже существует");
            }

            if (!string.IsNullOrWhiteSpace(model.ELibraryAuthorId))
            {
                var existingByELibrary = _researcherStorage.GetElement(new ResearcherSearchModel
                {
                    ELibraryAuthorId = model.ELibraryAuthorId
                });

                if (existingByELibrary != null && existingByELibrary.Id != model.Id)
                {
                    throw new InvalidOperationException("Исследователь с таким eLibrary ID уже существует");
                }
            }
        }

        private static bool IsValidEmail(string email)
        {
            try
            {
                var addr = new System.Net.Mail.MailAddress(email);
                return addr.Address == email;
            }
            catch
            {
                return false;
            }
        }

        private static string NormalizePhone(string phone)
        {
            return new string(phone.Where(char.IsDigit).ToArray());
        }
    }
}
