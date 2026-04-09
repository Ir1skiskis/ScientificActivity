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
    public class JournalVakSpecialtyLogic : IJournalVakSpecialtyLogic
    {
        private readonly ILogger<JournalVakSpecialtyLogic> _logger;
        private readonly IJournalVakSpecialtyStorage _storage;

        public JournalVakSpecialtyLogic(
            ILogger<JournalVakSpecialtyLogic> logger,
            IJournalVakSpecialtyStorage storage)
        {
            _logger = logger;
            _storage = storage;
        }

        public List<JournalVakSpecialtyViewModel>? ReadList(JournalVakSpecialtySearchModel? model)
        {
            _logger.LogInformation(
                "ReadList. JournalVakSpecialty. JournalId:{JournalId}, SpecialtyCode:{SpecialtyCode}",
                model?.JournalId, model?.SpecialtyCode);

            var list = model == null
                ? _storage.GetFullList()
                : _storage.GetFilteredList(model);

            _logger.LogInformation("ReadList. JournalVakSpecialty. Count:{Count}", list.Count);
            return list;
        }

        public JournalVakSpecialtyViewModel? ReadElement(JournalVakSpecialtySearchModel model)
        {
            if (model == null)
            {
                throw new ArgumentNullException(nameof(model));
            }

            _logger.LogInformation(
                "ReadElement. JournalVakSpecialty. JournalId:{JournalId}, SpecialtyCode:{SpecialtyCode}",
                model.JournalId, model.SpecialtyCode);

            return _storage.GetElement(model);
        }

        public bool Create(JournalVakSpecialtyBindingModel model)
        {
            CheckModel(model);

            if (_storage.Insert(model) == null)
            {
                _logger.LogWarning("Insert JournalVakSpecialty failed");
                return false;
            }

            return true;
        }

        public bool DeleteByJournal(int journalId)
        {
            var deleted = _storage.DeleteByJournal(journalId);
            _logger.LogInformation(
                "DeleteByJournal. JournalVakSpecialty. JournalId:{JournalId}, Deleted:{Deleted}",
                journalId, deleted);

            return true;
        }

        private void CheckModel(JournalVakSpecialtyBindingModel model)
        {
            if (model == null)
            {
                throw new ArgumentNullException(nameof(model));
            }

            if (model.JournalId <= 0)
            {
                throw new ArgumentException("Не указан JournalId", nameof(model.JournalId));
            }

            if (string.IsNullOrWhiteSpace(model.SpecialtyCode))
            {
                throw new ArgumentException("Не указан код специальности", nameof(model.SpecialtyCode));
            }

            if (string.IsNullOrWhiteSpace(model.SpecialtyName))
            {
                throw new ArgumentException("Не указано название специальности", nameof(model.SpecialtyName));
            }

            if (model.DateTo.HasValue && model.DateTo.Value < model.DateFrom)
            {
                throw new ArgumentException("Дата окончания не может быть меньше даты начала");
            }

            model.SpecialtyCode = model.SpecialtyCode.Trim();
            model.SpecialtyName = model.SpecialtyName.Trim();
            model.ScienceBranch = string.IsNullOrWhiteSpace(model.ScienceBranch)
                ? string.Empty
                : model.ScienceBranch.Trim();
        }
    }
}
