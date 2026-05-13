using Microsoft.AspNetCore.Mvc;
using ScientificActivityContracts.BindingModels;

namespace ScientificActivityClientApp.Controllers
{
    public class ResearcherReportController : Controller
    {
        [HttpGet]
        public IActionResult Configure(int researcherId)
        {
            if (researcherId <= 0)
            {
                TempData["Error"] = "Не указан исследователь для формирования отчета";
                return RedirectToAction("Index", "Researcher");
            }

            var model = new ResearcherReportSettingsBindingModel
            {
                ResearcherId = researcherId,
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

            var report = await APIClient.GetResearcherReportPreviewAsync(model);

            if (report == null)
            {
                TempData["Error"] = "Не удалось сформировать предварительный просмотр отчета. Проверьте, что у исследователя импортирован профиль eLibrary.";
                return RedirectToAction("Configure", new { researcherId = model.ResearcherId });
            }

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
    }
}
