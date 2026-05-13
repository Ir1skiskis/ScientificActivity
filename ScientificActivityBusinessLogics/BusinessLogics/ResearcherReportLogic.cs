using Microsoft.Extensions.Logging;
using ScientificActivityBusinessLogics.Reports;
using ScientificActivityContracts.BindingModels;
using ScientificActivityContracts.BusinessLogicsContracts;
using ScientificActivityContracts.SearchModels;
using ScientificActivityContracts.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScientificActivityBusinessLogics.BusinessLogics
{
    public class ResearcherReportLogic : IResearcherReportLogic
    {
        private readonly IELibraryLogic _eLibraryLogic;
        private readonly IPublicationLogic _publicationLogic;
        private readonly ILogger<ResearcherReportLogic> _logger;

        public ResearcherReportLogic(
            IELibraryLogic eLibraryLogic,
            IPublicationLogic publicationLogic,
            ILogger<ResearcherReportLogic> logger)
        {
            _eLibraryLogic = eLibraryLogic;
            _publicationLogic = publicationLogic;
            _logger = logger;
        }

        public ResearcherReportViewModel BuildReport(ResearcherReportSettingsBindingModel model)
        {
            if (model == null)
            {
                throw new ArgumentNullException(nameof(model), "Настройки отчета не переданы");
            }

            if (model.ResearcherId <= 0)
            {
                throw new ArgumentException("Не указан корректный идентификатор исследователя", nameof(model.ResearcherId));
            }

            _logger.LogInformation("Формирование отчета исследователя. ResearcherId: {ResearcherId}", model.ResearcherId);

            var profile = _eLibraryLogic.GetStoredAuthorProfile(model.ResearcherId);

            if (profile == null)
            {
                throw new InvalidOperationException("Расширенный профиль исследователя не найден. Сначала импортируйте профиль из eLibrary.");
            }

            var report = new ResearcherReportViewModel
            {
                Settings = model,
                Profile = profile,
                GeneratedAt = DateTime.Now,

                PublicationsRincByYear = profile.PublicationsRincByYear ?? new Dictionary<int, int>(),
                PublicationsCoreRincByYear = profile.PublicationsCoreRincByYear ?? new Dictionary<int, int>(),
                CitationsRincByYear = profile.CitationsRincByYear ?? new Dictionary<int, int>(),
                CitationsCoreRincByYear = profile.CitationsCoreRincByYear ?? new Dictionary<int, int>(),
                HIndexRincByYear = profile.HIndexRincByYear ?? new Dictionary<int, int>(),
                HIndexCoreRincByYear = profile.HIndexCoreRincByYear ?? new Dictionary<int, int>(),
                PercentileCoreRincByYear = profile.PercentileCoreRincByYear ?? new Dictionary<int, int>(),

                PublicationsRinc5YearsByEndYear = profile.PublicationsRinc5YearsByEndYear ?? new Dictionary<int, int>(),
                PublicationsCoreRinc5YearsByEndYear = profile.PublicationsCoreRinc5YearsByEndYear ?? new Dictionary<int, int>(),
                CitationsRinc5YearsByEndYear = profile.CitationsRinc5YearsByEndYear ?? new Dictionary<int, int>(),
                CitationsCoreRinc5YearsByEndYear = profile.CitationsCoreRinc5YearsByEndYear ?? new Dictionary<int, int>()
            };

            if (model.IncludePublications)
            {
                var publications = _publicationLogic.ReadList(new PublicationSearchModel
                {
                    ResearcherId = model.ResearcherId
                }) ?? new List<PublicationViewModel>();

                report.Publications = FilterPublications(publications, model);
            }

            return report;
        }

        public byte[] GeneratePdf(ResearcherReportSettingsBindingModel model)
        {
            var report = BuildReport(model);

            return ResearcherReportPdfBuilder.Build(report);
        }

        public byte[] GenerateDocx(ResearcherReportSettingsBindingModel model)
        {
            var report = BuildReport(model);

            return ResearcherReportDocxBuilder.Build(report);
        }

        private static List<PublicationViewModel> FilterPublications(
            List<PublicationViewModel> publications,
            ResearcherReportSettingsBindingModel settings)
        {
            IEnumerable<PublicationViewModel> query = publications;

            if (settings.IncludeOnlyLastFiveYearsPublications)
            {
                var minYear = DateTime.Now.Year - 4;
                query = query.Where(x => x.Year >= minYear);
            }

            if (settings.IncludeOnlyVakPublications)
            {
                query = query.Where(x => x.IsVak);
            }

            if (settings.IncludeOnlyPublicationsWithDoi)
            {
                query = query.Where(x => !string.IsNullOrWhiteSpace(x.Doi));
            }

            if (settings.PublicationsDateFrom.HasValue)
            {
                query = query.Where(x =>
                    x.PublicationDate.HasValue &&
                    x.PublicationDate.Value.Date >= settings.PublicationsDateFrom.Value.Date);
            }

            if (settings.PublicationsDateTo.HasValue)
            {
                query = query.Where(x =>
                    x.PublicationDate.HasValue &&
                    x.PublicationDate.Value.Date <= settings.PublicationsDateTo.Value.Date);
            }

            return query
                .OrderByDescending(x => x.Year)
                .ThenBy(x => x.Title)
                .ToList();
        }

        private static string BuildTemporaryTextReport(ResearcherReportViewModel report)
        {
            var builder = new StringBuilder();

            builder.AppendLine(report.Settings.ReportTitle);
            builder.AppendLine();

            if (report.Profile != null)
            {
                builder.AppendLine($"ФИО: {report.Profile.FullName}");
                builder.AppendLine($"Организация: {report.Profile.Organization}");
                builder.AppendLine($"Кафедра: {report.Profile.Department}");
                builder.AppendLine($"AuthorID: {report.Profile.AuthorId}");
                builder.AppendLine($"SPIN-код: {report.Profile.SpinCode}");
                builder.AppendLine();

                builder.AppendLine("Основные показатели:");
                builder.AppendLine($"Публикации eLibrary: {report.Profile.PublicationsCountElibrary}");
                builder.AppendLine($"Публикации РИНЦ: {report.Profile.PublicationsCountRinc}");
                builder.AppendLine($"Публикации ядра РИНЦ: {report.Profile.PublicationsCoreRincCount}");
                builder.AppendLine($"Цитирования eLibrary: {report.Profile.CitationsCountElibrary}");
                builder.AppendLine($"Цитирования РИНЦ: {report.Profile.CitationsCountRinc}");
                builder.AppendLine($"Индекс Хирша РИНЦ: {report.Profile.HIndexRinc}");
                builder.AppendLine();
            }

            if (report.Publications.Any())
            {
                builder.AppendLine("Публикации:");
                foreach (var publication in report.Publications)
                {
                    builder.AppendLine($"{publication.Year}. {publication.Title}");
                    builder.AppendLine($"Авторы: {publication.Authors}");

                    if (!string.IsNullOrWhiteSpace(publication.JournalTitle))
                    {
                        builder.AppendLine($"Журнал: {publication.JournalTitle}");
                    }

                    if (!string.IsNullOrWhiteSpace(publication.Doi))
                    {
                        builder.AppendLine($"DOI: {publication.Doi}");
                    }

                    builder.AppendLine();
                }
            }

            builder.AppendLine($"Дата формирования: {report.GeneratedAt:dd.MM.yyyy HH:mm}");

            return builder.ToString();
        }
    }
}
