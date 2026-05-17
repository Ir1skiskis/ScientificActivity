using Microsoft.AspNetCore.Mvc;
using ScientificActivityContracts.BindingModels;
using ScientificActivityContracts.ViewModels;

namespace ScientificActivityClientApp.Controllers
{
    public class ResearcherReportController : Controller
    {
        [HttpGet]
        public IActionResult Configure(int researcherId)
        {
            var currentYear = DateTime.Now.Year;
            if (researcherId <= 0)
            {
                TempData["Error"] = "Не указан исследователь для формирования отчета";
                return RedirectToAction("Index", "Researcher");
            }

            var model = new ResearcherReportSettingsBindingModel
            {
                ResearcherId = researcherId,
                StartYear = currentYear - 4,
                EndYear = currentYear,
                ReportTitle = "Отчет о научной активности исследователя",

                IncludeCommonInfo = true,
                IncludePublicationSummary = true,
                IncludeCitationIndexes = true,
                IncludeAdditionalMetrics = true,
                IncludeJournalMetrics = true,
                IncludeLastFiveYearsMetrics = true,
                IncludeYearDynamics = true,

                IncludePublications = false,
                IncludeOnlyLastFiveYearsPublications = false,
                IncludeOnlyVakPublications = false,
                IncludeOnlyPublicationsWithDoi = false
            };

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Preview(ResearcherReportSettingsBindingModel model)
        {
            if (model.ResearcherId <= 0)
            {
                TempData["Error"] = "Не указан исследователь для формирования отчета";
                return RedirectToAction("Index", "Researcher");
            }

            NormalizeReportPeriod(model);

            var report = await APIClient.GetResearcherReportPreviewAsync(model);

            if (report == null)
            {
                TempData["Error"] = "Не удалось сформировать предварительный просмотр отчета. Проверьте, что у исследователя импортирован профиль eLibrary.";
                return RedirectToAction("Configure", new { researcherId = model.ResearcherId });
            }

            var allPublications = APIClient.GetRequest<List<PublicationViewModel>>(
                $"api/Publication/GetFilteredPublications?researcherId={model.ResearcherId}")
                ?? new List<PublicationViewModel>();

            var periodPublications = allPublications
                .Where(x => x.Year >= model.StartYear && x.Year <= model.EndYear)
                .OrderByDescending(x => x.Year)
                .ThenBy(x => x.Title)
                .ToList();

            var periodStatistic = BuildPeriodStatistic(
                periodPublications,
                model.StartYear,
                model.EndYear);

            report.Settings = model;
            report.PeriodPublications = periodPublications;
            report.PeriodStatistic = periodStatistic;

            return View(report);
        }

        [HttpPost]
        public async Task<IActionResult> DownloadPdf(ResearcherReportSettingsBindingModel model)
        {
            var fileBytes = await APIClient.DownloadResearcherReportPdfAsync(model);

            if (fileBytes == null || fileBytes.Length == 0)
            {
                TempData["Error"] = "Не удалось скачать PDF-отчет";
                return RedirectToAction("Configure", new { researcherId = model.ResearcherId });
            }

            var fileName = BuildSafeFileName(model.ReportTitle, "pdf");

            return File(fileBytes, "application/pdf", fileName);
        }

        [HttpPost]
        public async Task<IActionResult> DownloadDocx(ResearcherReportSettingsBindingModel model)
        {
            var fileBytes = await APIClient.DownloadResearcherReportDocxAsync(model);

            if (fileBytes == null || fileBytes.Length == 0)
            {
                TempData["Error"] = "Не удалось скачать DOCX-отчет";
                return RedirectToAction("Configure", new { researcherId = model.ResearcherId });
            }

            var fileName = BuildSafeFileName(model.ReportTitle, "docx");

            return File(
                fileBytes,
                "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
                fileName);
        }

        private static string BuildSafeFileName(string? reportTitle, string extension)
        {
            var title = string.IsNullOrWhiteSpace(reportTitle)
                ? "Отчет о научной активности"
                : reportTitle.Trim();

            foreach (var invalidChar in Path.GetInvalidFileNameChars())
            {
                title = title.Replace(invalidChar, '_');
            }

            return $"{title}_{DateTime.Now:yyyyMMdd_HHmmss}.{extension}";
        }

        private static void NormalizeReportPeriod(ResearcherReportSettingsBindingModel model)
        {
            var currentYear = DateTime.Now.Year;

            if (model.StartYear <= 0)
            {
                model.StartYear = currentYear - 4;
            }

            if (model.EndYear <= 0)
            {
                model.EndYear = currentYear;
            }

            if (model.StartYear > model.EndYear)
            {
                var temp = model.StartYear;
                model.StartYear = model.EndYear;
                model.EndYear = temp;
            }

            if (model.EndYear > currentYear)
            {
                model.EndYear = currentYear;
            }

            if (model.StartYear < 1900)
            {
                model.StartYear = 1900;
            }
        }

        private static ResearcherReportPeriodStatisticViewModel BuildPeriodStatistic(
    List<PublicationViewModel> periodPublications,
    int startYear,
    int endYear)
        {
            var result = new ResearcherReportPeriodStatisticViewModel
            {
                StartYear = startYear,
                EndYear = endYear,
                PublicationsCount = periodPublications.Count,
                RincPublicationsCount = periodPublications.Count(x => x.IsInRinc),
                CoreRincPublicationsCount = periodPublications.Count(x => x.IsInCoreRinc),
                VakPublicationsCount = periodPublications.Count(x => x.IsVak),
                WhiteListPublicationsCount = periodPublications.Count(x =>
                    x.IsWhiteListLevel1 ||
                    x.IsWhiteListLevel2 ||
                    x.IsWhiteListLevel3 ||
                    x.IsWhiteListLevel4),
                CitationsRincCount = periodPublications.Sum(x => x.CitationsRincCount ?? 0)
            };

            result.YearStatistics = Enumerable.Range(startYear, endYear - startYear + 1)
                .Select(year =>
                {
                    var yearPublications = periodPublications
                        .Where(x => x.Year == year)
                        .ToList();

                    return new ResearcherReportYearStatisticViewModel
                    {
                        Year = year,
                        PublicationsCount = yearPublications.Count,
                        RincPublicationsCount = yearPublications.Count(x => x.IsInRinc),
                        CoreRincPublicationsCount = yearPublications.Count(x => x.IsInCoreRinc),
                        VakPublicationsCount = yearPublications.Count(x => x.IsVak),
                        WhiteListPublicationsCount = yearPublications.Count(x =>
                            x.IsWhiteListLevel1 ||
                            x.IsWhiteListLevel2 ||
                            x.IsWhiteListLevel3 ||
                            x.IsWhiteListLevel4),
                        CitationsRincCount = yearPublications.Sum(x => x.CitationsRincCount ?? 0)
                    };
                })
                .OrderBy(x => x.Year)
                .ToList();

            result.Keywords = periodPublications
                .Where(x => !string.IsNullOrWhiteSpace(x.Keywords))
                .SelectMany(x => x.Keywords!
                    .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .OrderBy(x => x)
                .ToList();

            result.JournalTitles = periodPublications
                .Where(x => !string.IsNullOrWhiteSpace(x.JournalTitle))
                .Select(x => x.JournalTitle!)
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .OrderBy(x => x)
                .ToList();

            return result;
        }
    }
}
