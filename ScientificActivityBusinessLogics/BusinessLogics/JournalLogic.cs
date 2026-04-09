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
    public class JournalLogic : IJournalLogic
    {
        private readonly ILogger _logger;
        private readonly IJournalStorage _journalStorage;

        public JournalLogic(ILogger<JournalLogic> logger, IJournalStorage journalStorage)
        {
            _logger = logger;
            _journalStorage = journalStorage;
        }

        public List<JournalViewModel>? ReadList(JournalSearchModel? model)
        {
            _logger.LogInformation("ReadList. Journal. Id:{Id}, Title:{Title}, ISSN:{ISSN}",
                model?.Id, model?.Title, model?.Issn);

            var list = model == null
                ? _journalStorage.GetFullList()
                : _journalStorage.GetFilteredList(model);

            if (list == null)
            {
                _logger.LogWarning("ReadList returned null list");
                return null;
            }

            _logger.LogInformation("ReadList. Count:{Count}", list.Count);
            return list;
        }

        public JournalViewModel? ReadElement(JournalSearchModel? model)
        {
            if (model == null)
            {
                throw new ArgumentNullException(nameof(model));
            }

            _logger.LogInformation("ReadElement. Journal. Id:{Id}, Title:{Title}, ISSN:{ISSN}",
                model.Id, model.Title, model.Issn);

            var element = _journalStorage.GetElement(model);
            if (element == null)
            {
                _logger.LogWarning("ReadElement. Journal not found");
                return null;
            }

            _logger.LogInformation("ReadElement. Found journal Id:{Id}", element.Id);
            return element;
        }

        public bool Create(JournalBindingModel model)
        {
            CheckModel(model);

            if (_journalStorage.Insert(model) == null)
            {
                _logger.LogWarning("Insert operation failed");
                return false;
            }

            return true;
        }

        public bool Update(JournalBindingModel model)
        {
            CheckModel(model);

            if (_journalStorage.Update(model) == null)
            {
                _logger.LogWarning("Update operation failed");
                return false;
            }

            return true;
        }

        public bool Delete(JournalBindingModel model)
        {
            CheckModel(model, false);

            if (_journalStorage.Delete(model) == null)
            {
                _logger.LogWarning("Delete operation failed");
                return false;
            }

            return true;
        }

        public JournalPagedListViewModel ReadPagedList(JournalSearchModel model)
        {
            if (model.Page <= 0)
            {
                model.Page = 1;
            }

            if (model.PageSize <= 0)
            {
                model.PageSize = 25;
            }

            var totalCount = _journalStorage.GetCount(model);
            var journals = _journalStorage.GetPagedList(model);

            return new JournalPagedListViewModel
            {
                Journals = journals,
                CurrentPage = model.Page,
                PageSize = model.PageSize,
                TotalCount = totalCount,
                TotalPages = (int)Math.Ceiling(totalCount / (double)model.PageSize),

                Title = model.Title,
                Issn = model.Issn,
                IsVak = model.IsVak,
                IsWhiteList = model.IsWhiteList
            };
        }

        private void CheckModel(JournalBindingModel model, bool withParams = true)
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
                throw new ArgumentNullException(nameof(model.Title), "Не указано название журнала");
            }

            model.Title = model.Title.Trim();
            model.Issn = string.IsNullOrWhiteSpace(model.Issn) ? null : model.Issn.Trim();
            model.EIssn = string.IsNullOrWhiteSpace(model.EIssn) ? null : model.EIssn.Trim();
            model.Publisher = string.IsNullOrWhiteSpace(model.Publisher) ? null : model.Publisher.Trim();
            model.SubjectArea = string.IsNullOrWhiteSpace(model.SubjectArea) ? null : model.SubjectArea.Trim();
            model.Country = string.IsNullOrWhiteSpace(model.Country) ? null : model.Country.Trim();
            model.Url = string.IsNullOrWhiteSpace(model.Url) ? null : model.Url.Trim();

            if (!string.IsNullOrWhiteSpace(model.Issn))
            {
                var existingByIssn = _journalStorage.GetElement(new JournalSearchModel
                {
                    Issn = model.Issn
                });

                if (existingByIssn != null && existingByIssn.Id != model.Id)
                {
                    throw new InvalidOperationException("Журнал с таким ISSN уже существует");
                }
            }

            _logger.LogInformation("Journal check. Title:{Title}, ISSN:{ISSN}, Id:{Id}",
                model.Title, model.Issn, model.Id);
        }
    }
}
