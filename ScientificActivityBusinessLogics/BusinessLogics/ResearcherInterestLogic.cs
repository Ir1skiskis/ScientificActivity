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
    public class ResearcherInterestLogic : IResearcherInterestLogic
    {
        private readonly ILogger _logger;
        private readonly IResearcherInterestStorage _interestStorage;
        private readonly IResearcherStorage _researcherStorage;

        public ResearcherInterestLogic(
            ILogger<ResearcherInterestLogic> logger,
            IResearcherInterestStorage interestStorage,
            IResearcherStorage researcherStorage)
        {
            _logger = logger;
            _interestStorage = interestStorage;
            _researcherStorage = researcherStorage;
        }

        public List<ResearcherInterestViewModel>? ReadList(ResearcherInterestSearchModel? model)
        {
            _logger.LogInformation("ReadList. ResearcherInterest. Id:{Id}, ResearcherId:{ResearcherId}, Keyword:{Keyword}",
                model?.Id, model?.ResearcherId, model?.Keyword);

            var list = model == null
                ? _interestStorage.GetFullList()
                : _interestStorage.GetFilteredList(model);

            if (list == null)
            {
                _logger.LogWarning("ReadList returned null list");
                return null;
            }

            _logger.LogInformation("ReadList. Count:{Count}", list.Count);
            return list;
        }

        public ResearcherInterestViewModel? ReadElement(ResearcherInterestSearchModel? model)
        {
            if (model == null)
            {
                throw new ArgumentNullException(nameof(model));
            }

            _logger.LogInformation("ReadElement. ResearcherInterest. Id:{Id}", model.Id);

            var element = _interestStorage.GetElement(model);
            if (element == null)
            {
                _logger.LogWarning("ReadElement. Interest not found");
                return null;
            }

            _logger.LogInformation("ReadElement. Found interest Id:{Id}", element.Id);
            return element;
        }

        public bool Create(ResearcherInterestBindingModel model)
        {
            CheckModel(model);

            if (_interestStorage.Insert(model) == null)
            {
                _logger.LogWarning("Insert operation failed");
                return false;
            }

            return true;
        }

        public bool Update(ResearcherInterestBindingModel model)
        {
            CheckModel(model);

            if (_interestStorage.Update(model) == null)
            {
                _logger.LogWarning("Update operation failed");
                return false;
            }

            return true;
        }

        public bool Delete(ResearcherInterestBindingModel model)
        {
            CheckModel(model, false);

            if (_interestStorage.Delete(model) == null)
            {
                _logger.LogWarning("Delete operation failed");
                return false;
            }

            return true;
        }

        private void CheckModel(ResearcherInterestBindingModel model, bool withParams = true)
        {
            if (model == null)
            {
                throw new ArgumentNullException(nameof(model));
            }

            if (!withParams)
            {
                return;
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

            if (string.IsNullOrWhiteSpace(model.Keyword))
            {
                throw new ArgumentNullException(nameof(model.Keyword), "Не указано ключевое слово");
            }

            model.Keyword = model.Keyword.Trim();

            if (model.Weight <= 0)
            {
                throw new ArgumentException("Вес интереса должен быть больше 0", nameof(model.Weight));
            }

            var existing = _interestStorage.GetElement(new ResearcherInterestSearchModel
            {
                ResearcherId = model.ResearcherId,
                Keyword = model.Keyword
            });

            if (existing != null && existing.Id != model.Id)
            {
                throw new InvalidOperationException("У исследователя уже есть такой интерес");
            }

            _logger.LogInformation("ResearcherInterest check. ResearcherId:{ResearcherId}, Keyword:{Keyword}, Weight:{Weight}, Id:{Id}",
                model.ResearcherId, model.Keyword, model.Weight, model.Id);
        }
    }
}
