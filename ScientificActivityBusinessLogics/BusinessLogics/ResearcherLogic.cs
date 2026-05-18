using Microsoft.Extensions.Logging;
using ScientificActivityBusinessLogics.Services;
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
        private readonly PasswordHashService _passwordHashService;

        public ResearcherLogic(ILogger<ResearcherLogic> logger, IResearcherStorage researcherStorage, PasswordHashService passwordHashService)
        {
            _logger = logger;
            _researcherStorage = researcherStorage;
            _passwordHashService = passwordHashService;
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

            model.PasswordHash = _passwordHashService.HashPassword(model.PasswordHash);

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

        public ResearcherViewModel? Login(string email, string password)
        {
            if (string.IsNullOrWhiteSpace(email))
            {
                throw new ArgumentException("Введите email", nameof(email));
            }

            if (string.IsNullOrWhiteSpace(password))
            {
                throw new ArgumentException("Введите пароль", nameof(password));
            }

            email = email.Trim();

            var researcher = _researcherStorage.GetElement(new ResearcherSearchModel
            {
                Email = email
            });

            if (researcher == null)
            {
                return null;
            }

            var storedPasswordHash = _researcherStorage.GetPasswordHashByEmail(email);

            if (string.IsNullOrWhiteSpace(storedPasswordHash))
            {
                return null;
            }

            var isPasswordValid = _passwordHashService.VerifyPassword(password, storedPasswordHash);

            if (!isPasswordValid && storedPasswordHash == password)
            {
                var updateModel = new ResearcherBindingModel
                {
                    Id = researcher.Id,
                    Email = researcher.Email,
                    PasswordHash = _passwordHashService.HashPassword(password),
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
                    ResearchTopics = researcher.ResearchTopics
                };

                _researcherStorage.Update(updateModel);
                isPasswordValid = true;
            }

            if (!isPasswordValid)
            {
                return null;
            }

            return researcher;
        }

        public bool ChangePassword(ChangePasswordBindingModel model)
        {
            if (model == null)
            {
                throw new ArgumentNullException(nameof(model));
            }

            if (model.ResearcherId <= 0)
            {
                throw new Exception("Не указан исследователь");
            }

            if (string.IsNullOrWhiteSpace(model.OldPassword))
            {
                throw new Exception("Введите старый пароль");
            }

            if (string.IsNullOrWhiteSpace(model.NewPassword))
            {
                throw new Exception("Введите новый пароль");
            }

            if (model.NewPassword.Length < 6)
            {
                throw new Exception("Новый пароль должен содержать не менее 6 символов");
            }

            if (model.NewPassword != model.ConfirmNewPassword)
            {
                throw new Exception("Новый пароль и подтверждение пароля не совпадают");
            }

            var researcher = _researcherStorage.GetElement(new ResearcherSearchModel
            {
                Id = model.ResearcherId
            });

            if (researcher == null)
            {
                throw new Exception("Исследователь не найден");
            }

            var storedPasswordHash = _researcherStorage.GetPasswordHashByEmail(researcher.Email);

            if (string.IsNullOrWhiteSpace(storedPasswordHash))
            {
                throw new Exception("Не удалось получить данные пароля");
            }

            var isOldPasswordValid = _passwordHashService.VerifyPassword(model.OldPassword, storedPasswordHash);

            if (!isOldPasswordValid && storedPasswordHash == model.OldPassword)
            {
                isOldPasswordValid = true;
            }

            if (!isOldPasswordValid)
            {
                throw new Exception("Старый пароль указан неверно");
            }

            var updateModel = new ResearcherBindingModel
            {
                Id = researcher.Id,
                Email = researcher.Email,
                PasswordHash = _passwordHashService.HashPassword(model.NewPassword),
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
                ResearchTopics = researcher.ResearchTopics
            };

            return _researcherStorage.Update(updateModel) != null;
        }
    }
}
