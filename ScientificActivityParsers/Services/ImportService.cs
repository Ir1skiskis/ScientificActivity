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
        private readonly IJournalParser _journalParser;

        private readonly IGrantLogic _grantLogic;
        private readonly IConferenceLogic _conferenceLogic;
        private readonly IJournalLogic _journalLogic;
        private readonly IJournalVakSpecialtyLogic _journalVakSpecialtyLogic;

        public ImportService(
            IGrantParser grantParser,
            IConferenceParser conferenceParser,
            IJournalParser journalParser,
            IGrantLogic grantLogic,
            IConferenceLogic conferenceLogic,
            IJournalLogic journalLogic,
            IJournalVakSpecialtyLogic journalVakSpecialtyLogic)
        {
            _grantParser = grantParser;
            _conferenceParser = conferenceParser;
            _journalParser = journalParser;

            _grantLogic = grantLogic;
            _conferenceLogic = conferenceLogic;
            _journalLogic = journalLogic;
            _journalVakSpecialtyLogic = journalVakSpecialtyLogic;
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

        public async Task<int> ImportVakJournalsAsync(string pdfPath, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(pdfPath))
            {
                throw new ArgumentNullException(nameof(pdfPath));
            }

            var items = await _journalParser.ParseVakPdfAsync(pdfPath, cancellationToken);
            var processedCount = 0;

            foreach (var item in items)
            {
                cancellationToken.ThrowIfCancellationRequested();

                try
                {
                    Console.WriteLine($"Импорт журнала: {item.Title} | ISSN: {item.Issn}");

                    var existing = FindExistingJournal(item);
                    int journalId;

                    if (existing != null)
                    {
                        var updated = _journalLogic.Update(new JournalBindingModel
                        {
                            Id = existing.Id,
                            Title = item.Title,
                            Issn = NormalizeIssn(item.Issn),
                            EIssn = NormalizeIssn(item.EIssn),
                            Publisher = existing.Publisher,
                            SubjectArea = BuildSubjectArea(item),
                            Quartile = existing.Quartile,
                            IsVak = true,
                            IsWhiteList = existing.IsWhiteList,
                            Country = existing.Country,
                            Url = existing.Url
                        });

                        if (!updated)
                        {
                            Console.WriteLine($"Не удалось обновить журнал: {item.Title}");
                            continue;
                        }

                        journalId = existing.Id;
                        _journalVakSpecialtyLogic.DeleteByJournal(journalId);
                    }
                    else
                    {
                        var createdOk = _journalLogic.Create(new JournalBindingModel
                        {
                            Title = item.Title,
                            Issn = NormalizeIssn(item.Issn),
                            EIssn = NormalizeIssn(item.EIssn),
                            Publisher = null,
                            SubjectArea = BuildSubjectArea(item),
                            Quartile = JournalQuartile.Не_указан,
                            IsVak = true,
                            IsWhiteList = false,
                            Country = null,
                            Url = null
                        });

                        if (!createdOk)
                        {
                            Console.WriteLine($"Не удалось создать журнал: {item.Title}");
                            continue;
                        }

                        var created = FindExistingJournal(item);
                        if (created == null)
                        {
                            throw new InvalidOperationException($"Не удалось повторно найти созданный журнал '{item.Title}'");
                        }

                        journalId = created.Id;
                    }

                    foreach (var specialty in item.VakSpecialties)
                    {
                        var createdSpecialty = _journalVakSpecialtyLogic.Create(new JournalVakSpecialtyBindingModel
                        {
                            JournalId = journalId,
                            SpecialtyCode = specialty.SpecialtyCode,
                            SpecialtyName = specialty.SpecialtyName,
                            ScienceBranch = specialty.ScienceBranch,
                            DateFrom = EnsureUtc(specialty.DateFrom),
                            DateTo = specialty.DateTo.HasValue ? EnsureUtc(specialty.DateTo.Value) : null
                        });

                        if (!createdSpecialty)
                        {
                            Console.WriteLine($"Не удалось создать специальность {specialty.SpecialtyCode} для журнала {item.Title}");
                        }
                    }

                    processedCount++;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Ошибка на журнале '{item.Title}': {ex}");
                    throw;
                }
            }

            return processedCount;
        }

        private ScientificActivityContracts.ViewModels.JournalViewModel? FindExistingJournal(
            ScientificActivityParsers.Models.JournalImportModel item)
        {
            var normalizedIssn = NormalizeIssn(item.Issn);

            if (!string.IsNullOrWhiteSpace(normalizedIssn))
            {
                var byIssn = _journalLogic.ReadElement(new JournalSearchModel
                {
                    Issn = normalizedIssn
                });

                if (byIssn != null)
                {
                    return byIssn;
                }
            }

            return _journalLogic.ReadElement(new JournalSearchModel
            {
                Title = item.Title
            });
        }

        private static string? NormalizeIssn(string? issn)
        {
            if (string.IsNullOrWhiteSpace(issn))
            {
                return null;
            }

            return issn.Trim().ToUpperInvariant();
        }

        private static string? BuildSubjectArea(ScientificActivityParsers.Models.JournalImportModel item)
        {
            if (item.VakSpecialties == null || item.VakSpecialties.Count == 0)
            {
                return item.SubjectArea;
            }

            return string.Join("; ",
                item.VakSpecialties
                    .Select(x => $"{x.SpecialtyCode} {x.SpecialtyName}".Trim())
                    .Distinct(StringComparer.OrdinalIgnoreCase));
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
