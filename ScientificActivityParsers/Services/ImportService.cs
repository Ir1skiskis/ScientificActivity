using ScientificActivityContracts.BindingModels;
using ScientificActivityContracts.BusinessLogicsContracts;
using ScientificActivityContracts.SearchModels;
using ScientificActivityContracts.ViewModels;
using ScientificActivityDataModels.Enums;
using ScientificActivityParsers.Interfaces;
using ScientificActivityParsers.Models;
using ScientificActivityParsers.Parsers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;

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

        private readonly IWhiteListJournalParser _whiteListJournalParser;
        private readonly IRcsiSubjectCategoryParser _rcsiSubjectCategoryParser;
        private readonly IRcsiLevelApiClient _rcsiLevelApiClient;

        public ImportService(
            IGrantParser grantParser,
            IConferenceParser conferenceParser,
            IJournalParser journalParser,
            IGrantLogic grantLogic,
            IConferenceLogic conferenceLogic,
            IJournalLogic journalLogic,
            IJournalVakSpecialtyLogic journalVakSpecialtyLogic,
            IWhiteListJournalParser whiteListJournalParser,
            IRcsiSubjectCategoryParser rcsiSubjectCategoryParser,
            IRcsiLevelApiClient rcsiLevelApiClient)
        {
            _grantParser = grantParser;
            _conferenceParser = conferenceParser;
            _journalParser = journalParser;

            _grantLogic = grantLogic;
            _conferenceLogic = conferenceLogic;
            _journalLogic = journalLogic;
            _journalVakSpecialtyLogic = journalVakSpecialtyLogic;

            _whiteListJournalParser = whiteListJournalParser;
            _rcsiSubjectCategoryParser = rcsiSubjectCategoryParser;
            _rcsiLevelApiClient = rcsiLevelApiClient;
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
                    Console.WriteLine($"Импорт журнала ВАК: {item.Title} | ISSN: {item.Issn}");

                    var existing = FindExistingJournal(item);
                    int journalId;

                    if (existing != null)
                    {
                        var updated = _journalLogic.Update(new JournalBindingModel
                        {
                            Id = existing.Id,
                            Title = item.Title,
                            Issn = !string.IsNullOrWhiteSpace(item.Issn) ? NormalizeIssn(item.Issn) : existing.Issn,
                            EIssn = !string.IsNullOrWhiteSpace(item.EIssn) ? NormalizeIssn(item.EIssn) : existing.EIssn,
                            Publisher = existing.Publisher,
                            SubjectArea = BuildSubjectArea(item),
                            IsVak = true,
                            IsWhiteList = existing.IsWhiteList,
                            WhiteListLevel2023 = existing.WhiteListLevel2023,
                            WhiteListLevel2025 = existing.WhiteListLevel2025,
                            WhiteListState = existing.WhiteListState,
                            WhiteListNotice = existing.WhiteListNotice,
                            WhiteListAcceptedDate = existing.WhiteListAcceptedDate,
                            WhiteListDiscontinuedDate = existing.WhiteListDiscontinuedDate,
                            Country = existing.Country,
                            Url = existing.Url,
                            RcsiRecordSourceId = existing.RcsiRecordSourceId
                        });

                        if (!updated)
                        {
                            Console.WriteLine($"Не удалось обновить журнал ВАК: {item.Title}");
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
                            WhiteListLevel2023 = null,
                            WhiteListLevel2025 = null,
                            WhiteListState = null,
                            WhiteListNotice = null,
                            WhiteListAcceptedDate = null,
                            WhiteListDiscontinuedDate = null,
                            IsVak = true,
                            IsWhiteList = false,
                            Country = null,
                            Url = null,
                            RcsiRecordSourceId = null
                        });

                        if (!createdOk)
                        {
                            Console.WriteLine($"Не удалось создать журнал ВАК: {item.Title}");
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
                    Console.WriteLine($"Ошибка на журнале ВАК '{item.Title}': {ex}");
                    throw;
                }
            }

            return processedCount;
        }

        public async Task<int> ImportWhiteListJournalsAsync(CancellationToken cancellationToken = default)
        {
            var items = await _whiteListJournalParser.ParseAsync(cancellationToken);
            Console.WriteLine($"White list parsed count: {items.Count}");
            var processedCount = 0;

            foreach (var item in items)
            {
                cancellationToken.ThrowIfCancellationRequested();

                var existing = FindExistingJournalByAnyIssn(item);

                if (existing != null)
                {
                    var updated = _journalLogic.Update(new JournalBindingModel
                    {
                        Id = existing.Id,
                        Title = existing.Title,
                        Issn = !string.IsNullOrWhiteSpace(existing.Issn) ? existing.Issn : item.Issn,
                        EIssn = !string.IsNullOrWhiteSpace(existing.EIssn) ? existing.EIssn : item.EIssn,
                        Publisher = existing.Publisher,
                        SubjectArea = !string.IsNullOrWhiteSpace(existing.SubjectArea)
                            ? existing.SubjectArea
                            : item.SubjectArea,
                        IsVak = existing.IsVak,
                        IsWhiteList = true,
                        WhiteListLevel2023 = item.WhiteListLevel2023,
                        WhiteListLevel2025 = item.WhiteListLevel2025,
                        WhiteListState = item.WhiteListState,
                        WhiteListNotice = item.WhiteListNotice,
                        WhiteListAcceptedDate = item.WhiteListAcceptedDate,
                        WhiteListDiscontinuedDate = item.WhiteListDiscontinuedDate,
                        Country = existing.Country,
                        Url = !string.IsNullOrWhiteSpace(existing.Url) ? existing.Url : item.Url,
                        RcsiRecordSourceId = existing.RcsiRecordSourceId ?? item.RcsiRecordSourceId
                    });

                    if (updated)
                    {
                        processedCount++;
                    }

                    continue;
                }

                var created = _journalLogic.Create(new JournalBindingModel
                {
                    Title = item.Title,
                    Issn = NormalizeIssn(item.Issn),
                    EIssn = NormalizeIssn(item.EIssn),
                    Publisher = null,
                    SubjectArea = item.SubjectArea,
                    IsVak = false,
                    IsWhiteList = true,
                    WhiteListLevel2023 = item.WhiteListLevel2023,
                    WhiteListLevel2025 = item.WhiteListLevel2025,
                    WhiteListState = item.WhiteListState,
                    WhiteListNotice = item.WhiteListNotice,
                    WhiteListAcceptedDate = item.WhiteListAcceptedDate,
                    WhiteListDiscontinuedDate = item.WhiteListDiscontinuedDate,
                    Country = null,
                    Url = item.Url,
                    RcsiRecordSourceId = item.RcsiRecordSourceId
                });

                if (created)
                {
                    processedCount++;
                }
            }

            return processedCount;
        }

        public async Task<int> EnrichWhiteListRcsiLinksAsync(CancellationToken cancellationToken = default)
        {
            var journals = _journalLogic.ReadList(new JournalSearchModel
            {
                IsWhiteList = true
            }) ?? new List<JournalViewModel>();

            var journalsToProcess = journals
                .Where(j => !j.RcsiRecordSourceId.HasValue || string.IsNullOrWhiteSpace(j.Url))
                .ToList();

            var processedCount = 0;
            var failedCount = 0;

            const int maxDegreeOfParallelism = 6;
            using var semaphore = new SemaphoreSlim(maxDegreeOfParallelism);

            var tasks = journalsToProcess.Select(async journal =>
            {
                await semaphore.WaitAsync(cancellationToken);
                try
                {
                    cancellationToken.ThrowIfCancellationRequested();

                    var issnsToTry = new List<string>();

                    if (!string.IsNullOrWhiteSpace(journal.Issn))
                    {
                        issnsToTry.Add(NormalizeIssn(journal.Issn)!);
                    }

                    if (!string.IsNullOrWhiteSpace(journal.EIssn))
                    {
                        issnsToTry.Add(NormalizeIssn(journal.EIssn)!);
                    }

                    issnsToTry = issnsToTry
                        .Where(x => !string.IsNullOrWhiteSpace(x))
                        .Distinct(StringComparer.OrdinalIgnoreCase)
                        .ToList();

                    if (issnsToTry.Count == 0)
                    {
                        return;
                    }

                    RcsiLevelApiResponseModel? apiModel = null;

                    foreach (var issn in issnsToTry)
                    {
                        try
                        {
                            apiModel = await _rcsiLevelApiClient.GetByIssnAsync(issn, cancellationToken);
                            if (apiModel != null)
                            {
                                break;
                            }
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"RCSI level API error for ISSN {issn}: {ex.Message}");
                        }
                    }

                    if (apiModel == null)
                    {
                        return;
                    }

                    var fullUrl = string.IsNullOrWhiteSpace(apiModel.Url)
                        ? null
                        : apiModel.Url.StartsWith("http", StringComparison.OrdinalIgnoreCase)
                            ? apiModel.Url
                            : $"https://journalrank.rcsi.science{apiModel.Url}";

                    var updated = _journalLogic.Update(new JournalBindingModel
                    {
                        Id = journal.Id,
                        Title = journal.Title,
                        Issn = journal.Issn,
                        EIssn = journal.EIssn,
                        Publisher = journal.Publisher,
                        SubjectArea = journal.SubjectArea,
                        IsVak = journal.IsVak,
                        IsWhiteList = journal.IsWhiteList,
                        WhiteListLevel2023 = journal.WhiteListLevel2023,
                        WhiteListLevel2025 = journal.WhiteListLevel2025,
                        WhiteListState = journal.WhiteListState,
                        WhiteListNotice = journal.WhiteListNotice,
                        WhiteListAcceptedDate = journal.WhiteListAcceptedDate,
                        WhiteListDiscontinuedDate = journal.WhiteListDiscontinuedDate,
                        Country = journal.Country,
                        Url = !string.IsNullOrWhiteSpace(journal.Url) ? journal.Url : fullUrl,
                        RcsiRecordSourceId = journal.RcsiRecordSourceId ?? (apiModel.Id > 0 ? apiModel.Id : null)
                    });

                    if (updated)
                    {
                        Interlocked.Increment(ref processedCount);
                        Console.WriteLine($"RCSI link enriched: journalId={journal.Id}, title={journal.Title}, rcsiId={apiModel.Id}, url={fullUrl}");
                    }

                    await Task.Delay(50, cancellationToken);
                }
                catch (OperationCanceledException)
                {
                    throw;
                }
                catch (Exception ex)
                {
                    Interlocked.Increment(ref failedCount);
                    Console.WriteLine($"RCSI link enrich failed: journalId={journal.Id}, title={journal.Title}, error={ex}");
                }
                finally
                {
                    semaphore.Release();
                }
            });

            await Task.WhenAll(tasks);

            Console.WriteLine($"RCSI LINK ENRICH SUMMARY: total={journalsToProcess.Count}, updated={processedCount}, failed={failedCount}");

            return processedCount;
        }

        public async Task<int> EnrichWhiteListSubjectAreasAsync(CancellationToken cancellationToken = default)
        {
            var journals = _journalLogic.ReadList(new JournalSearchModel
            {
                IsWhiteList = true
            }) ?? new List<JournalViewModel>();

            var journalsToProcess = journals
                .Where(j =>
                    string.IsNullOrWhiteSpace(j.SubjectArea) &&
                    (j.RcsiRecordSourceId.HasValue || !string.IsNullOrWhiteSpace(j.Url)))
                .ToList();

            var processedCount = 0;
            var failedCount = 0;
            var noCategoriesCount = 0;

            const int maxDegreeOfParallelism = 6;
            using var semaphore = new SemaphoreSlim(maxDegreeOfParallelism);

            var tasks = journalsToProcess.Select(async journal =>
            {
                await semaphore.WaitAsync(cancellationToken);
                try
                {
                    cancellationToken.ThrowIfCancellationRequested();

                    var categories = await _rcsiSubjectCategoryParser.ParseCategoriesAsync(
                        journal.RcsiRecordSourceId,
                        journal.Url,
                        cancellationToken);

                    if (categories.Count == 0)
                    {
                        Interlocked.Increment(ref noCategoriesCount);
                        Console.WriteLine($"ENRICH SKIP NO CATEGORIES: Id={journal.Id}, Title={journal.Title}, Url={journal.Url}, RcsiId={journal.RcsiRecordSourceId}");
                        return;
                    }

                    var updated = _journalLogic.Update(new JournalBindingModel
                    {
                        Id = journal.Id,
                        Title = journal.Title,
                        Issn = journal.Issn,
                        EIssn = journal.EIssn,
                        Publisher = journal.Publisher,
                        SubjectArea = string.Join("; ", categories),
                        IsVak = journal.IsVak,
                        IsWhiteList = journal.IsWhiteList,
                        WhiteListLevel2023 = journal.WhiteListLevel2023,
                        WhiteListLevel2025 = journal.WhiteListLevel2025,
                        WhiteListState = journal.WhiteListState,
                        WhiteListNotice = journal.WhiteListNotice,
                        WhiteListAcceptedDate = journal.WhiteListAcceptedDate,
                        WhiteListDiscontinuedDate = journal.WhiteListDiscontinuedDate,
                        Country = journal.Country,
                        Url = journal.Url,
                        RcsiRecordSourceId = journal.RcsiRecordSourceId
                    });

                    if (updated)
                    {
                        Interlocked.Increment(ref processedCount);
                        Console.WriteLine($"ENRICH UPDATED: Id={journal.Id}, Title={journal.Title}, Categories={categories.Count}");
                    }

                    await Task.Delay(50, cancellationToken);
                }
                catch (OperationCanceledException)
                {
                    throw;
                }
                catch (Exception ex)
                {
                    Interlocked.Increment(ref failedCount);
                    Console.WriteLine($"ENRICH SUBJECT FAILED: Id={journal.Id}, Title={journal.Title}, Error={ex}");
                }
                finally
                {
                    semaphore.Release();
                }
            });

            await Task.WhenAll(tasks);

            Console.WriteLine($"ENRICH SUBJECT SUMMARY: total={journalsToProcess.Count}, updated={processedCount}, noCategories={noCategoriesCount}, failed={failedCount}");

            return processedCount;
        }

        private JournalViewModel? FindExistingJournal(JournalImportModel item)
        {
            if (item.RcsiRecordSourceId.HasValue)
            {
                var byRcsiId = _journalLogic.ReadElement(new JournalSearchModel
                {
                    RcsiRecordSourceId = item.RcsiRecordSourceId.Value
                });

                if (byRcsiId != null)
                {
                    return byRcsiId;
                }
            }

            var issnsToCheck = item.AllIssns
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .Select(NormalizeIssn)
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();

            if (!string.IsNullOrWhiteSpace(item.Issn))
            {
                issnsToCheck.Insert(0, NormalizeIssn(item.Issn)!);
            }

            if (!string.IsNullOrWhiteSpace(item.EIssn))
            {
                issnsToCheck.Insert(0, NormalizeIssn(item.EIssn)!);
            }

            foreach (var issn in issnsToCheck.Distinct(StringComparer.OrdinalIgnoreCase))
            {
                var byIssn = _journalLogic.ReadElement(new JournalSearchModel
                {
                    Issn = issn
                });

                if (byIssn != null)
                {
                    return byIssn;
                }
            }

            if (!string.IsNullOrWhiteSpace(item.Title))
            {
                return _journalLogic.ReadElement(new JournalSearchModel
                {
                    Title = item.Title
                });
            }

            return null;
        }

        private JournalViewModel? FindExistingJournalByAnyIssn(JournalImportModel item)
        {
            if (item.RcsiRecordSourceId.HasValue)
            {
                var byRcsiId = _journalLogic.ReadElement(new JournalSearchModel
                {
                    RcsiRecordSourceId = item.RcsiRecordSourceId.Value
                });

                if (byRcsiId != null)
                {
                    return byRcsiId;
                }
            }

            var issnsToCheck = item.AllIssns
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .Select(NormalizeIssn)
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();

            if (!string.IsNullOrWhiteSpace(item.Issn))
            {
                issnsToCheck.Insert(0, NormalizeIssn(item.Issn)!);
            }

            if (!string.IsNullOrWhiteSpace(item.EIssn))
            {
                issnsToCheck.Insert(0, NormalizeIssn(item.EIssn)!);
            }

            foreach (var issn in issnsToCheck.Distinct(StringComparer.OrdinalIgnoreCase))
            {
                var byIssn = _journalLogic.ReadElement(new JournalSearchModel
                {
                    Issn = issn
                });

                if (byIssn != null)
                {
                    return byIssn;
                }
            }

            if (!string.IsNullOrWhiteSpace(item.Title))
            {
                return _journalLogic.ReadElement(new JournalSearchModel
                {
                    Title = item.Title
                });
            }

            return null;
        }

        private static string? NormalizeIssn(string? issn)
        {
            if (string.IsNullOrWhiteSpace(issn))
            {
                return null;
            }

            return issn.Trim().ToUpperInvariant().Replace('Х', 'X').Replace('х', 'X');
        }

        private static string? BuildSubjectArea(JournalImportModel item)
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

        public async Task<ImportAllJournalsResult> ImportAllJournalsAsync(string pdfPath, CancellationToken cancellationToken = default)
        {
            Console.WriteLine("Старт импорта ВАК");
            var vakCount = await ImportVakJournalsAsync(pdfPath, cancellationToken);
            Console.WriteLine($"Импорт ВАК завершён. Count={vakCount}");

            Console.WriteLine("Старт импорта Белого списка");
            var whiteListCount = await ImportWhiteListJournalsAsync(cancellationToken);
            Console.WriteLine($"Импорт Белого списка завершён. Count={whiteListCount}");

            Console.WriteLine("Старт заполнения RcsiRecordSourceId и Url");
            var rcsiLinkCount = await EnrichWhiteListRcsiLinksAsync(cancellationToken);
            Console.WriteLine($"Заполнение RcsiRecordSourceId и Url завершено. Count={rcsiLinkCount}");

            Console.WriteLine("Старт заполнения тематик РЦНИ");
            var subjectCount = await EnrichWhiteListSubjectAreasAsync(cancellationToken);
            Console.WriteLine($"Заполнение тематик РЦНИ завершено. Count={subjectCount}");

            return new ImportAllJournalsResult
            {
                VakCount = vakCount,
                WhiteListCount = whiteListCount
            };
        }
    }
}
