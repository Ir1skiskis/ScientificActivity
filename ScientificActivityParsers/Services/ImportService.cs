using ScientificActivityContracts.BindingModels;
using ScientificActivityContracts.BusinessLogicsContracts;
using ScientificActivityContracts.SearchModels;
using ScientificActivityDataModels.Enums;
using ScientificActivityParsers.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScientificActivityParsers.Services
{
    public class ImportService : IImportService
    {
        private readonly IGrantParser _grantParser;
        private readonly IConferenceParser _conferenceParser;
        private readonly IGrantLogic _grantLogic;
        private readonly IConferenceLogic _conferenceLogic;

        public ImportService(
            IGrantParser grantParser,
            IConferenceParser conferenceParser,
            IGrantLogic grantLogic,
            IConferenceLogic conferenceLogic)
        {
            _grantParser = grantParser;
            _conferenceParser = conferenceParser;
            _grantLogic = grantLogic;
            _conferenceLogic = conferenceLogic;
        }

        public async Task<int> ImportGrantsAsync(CancellationToken cancellationToken = default)
        {
            var items = await _grantParser.ParseAsync(cancellationToken);
            var processedCount = 0;

            foreach (var item in items)
            {
                var parsedStart = EnsureUtc(item.ApplicationDeadline ?? DateTime.UtcNow.Date);
                var parsedEnd = EnsureUtc(item.ResultDate ?? parsedStart);

                var startDate = parsedStart <= parsedEnd ? parsedStart : parsedEnd;
                var endDate = parsedStart <= parsedEnd ? parsedEnd : parsedStart;

                var mappedStatus = MapGrantStatus(item.StatusText);

                var existing = _grantLogic.ReadElement(new GrantSearchModel
                {
                    ContestNumber = item.ContestNumber
                });

                if (existing != null)
                {
                    var updated = _grantLogic.Update(new GrantBindingModel
                    {
                        Id = existing.Id,
                        ContestNumber = item.ContestNumber,
                        Title = item.Title,
                        Description = item.Description,
                        Organization = item.Organization,
                        StartDate = startDate,
                        EndDate = endDate,
                        Amount = existing.Amount,
                        Currency = existing.Currency,
                        SubjectArea = item.SubjectArea,
                        Status = mappedStatus,
                        Url = item.Url
                    });

                    if (updated)
                    {
                        processedCount++;
                    }

                    continue;
                }

                var created = _grantLogic.Create(new GrantBindingModel
                {
                    ContestNumber = item.ContestNumber,
                    Title = item.Title,
                    Description = item.Description,
                    Organization = item.Organization,
                    StartDate = startDate,
                    EndDate = endDate,
                    SubjectArea = item.SubjectArea,
                    Status = mappedStatus,
                    Url = item.Url
                });

                if (created)
                {
                    processedCount++;
                }
            }

            return processedCount;
        }

        public async Task<int> ImportConferencesAsync(CancellationToken cancellationToken = default)
        {
            var items = await _conferenceParser.ParseAsync(cancellationToken);
            var processedCount = 0;

            foreach (var item in items)
            {
                var parsedStart = EnsureUtc(item.StartDate ?? DateTime.UtcNow.Date);
                var parsedEnd = EnsureUtc(item.EndDate ?? parsedStart);

                var startDate = parsedStart <= parsedEnd ? parsedStart : parsedEnd;
                var endDate = parsedStart <= parsedEnd ? parsedEnd : parsedStart;

                var existing = _conferenceLogic.ReadElement(new ConferenceSearchModel
                {
                    Title = item.Title
                });

                if (existing != null)
                {
                    var updated = _conferenceLogic.Update(new ConferenceBindingModel
                    {
                        Id = existing.Id,
                        Title = item.Title,
                        Description = item.Description,
                        StartDate = startDate,
                        EndDate = endDate,
                        City = item.City,
                        Country = item.Country,
                        Organizer = item.Organizer,
                        SubjectArea = item.SubjectArea,
                        Format = existing.Format,
                        Level = existing.Level,
                        Url = item.Url
                    });

                    if (updated)
                    {
                        processedCount++;
                    }

                    continue;
                }

                var created = _conferenceLogic.Create(new ConferenceBindingModel
                {
                    Title = item.Title,
                    Description = item.Description,
                    StartDate = startDate,
                    EndDate = endDate,
                    City = item.City,
                    Country = item.Country,
                    Organizer = item.Organizer,
                    SubjectArea = item.SubjectArea,
                    Format = ConferenceFormat.Не_указан,
                    Level = ConferenceLevel.Не_указан,
                    Url = item.Url
                });

                if (created)
                {
                    processedCount++;
                }
            }

            return processedCount;
        }

        private static GrantStatus MapGrantStatus(string? statusText)
        {
            if (string.IsNullOrWhiteSpace(statusText))
            {
                return GrantStatus.Открыт;
            }

            if (statusText.Contains("Экспертиза", StringComparison.OrdinalIgnoreCase))
            {
                return GrantStatus.В_экспертизе;
            }

            if (statusText.Contains("Заверш", StringComparison.OrdinalIgnoreCase))
            {
                return GrantStatus.Завершен;
            }

            if (statusText.Contains("Прием заявок", StringComparison.OrdinalIgnoreCase) ||
                statusText.Contains("Приём заявок", StringComparison.OrdinalIgnoreCase) ||
                statusText.Contains("Открыт", StringComparison.OrdinalIgnoreCase))
            {
                return GrantStatus.Открыт;
            }

            return GrantStatus.Открыт;
        }

        private static DateTime EnsureUtc(DateTime dateTime)
        {
            if (dateTime.Kind == DateTimeKind.Utc)
            {
                return dateTime;
            }

            if (dateTime.Kind == DateTimeKind.Unspecified)
            {
                return DateTime.SpecifyKind(dateTime, DateTimeKind.Utc);
            }

            return dateTime.ToUniversalTime();
        }
    }
}
