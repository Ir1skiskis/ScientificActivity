using Microsoft.AspNetCore.Mvc;
using ScientificActivityContracts.BindingModels;
using ScientificActivityContracts.BusinessLogicsContracts;

namespace ScientificActivityRestApi.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class ResearcherReportController : ControllerBase
    {
        private readonly IResearcherReportLogic _researcherReportLogic;
        private readonly ILogger<ResearcherReportController> _logger;

        public ResearcherReportController(
            IResearcherReportLogic researcherReportLogic,
            ILogger<ResearcherReportController> logger)
        {
            _researcherReportLogic = researcherReportLogic;
            _logger = logger;
        }

        [HttpPost]
        public IActionResult Preview([FromBody] ResearcherReportSettingsBindingModel model)
        {
            try
            {
                var result = _researcherReportLogic.BuildReport(model);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Ошибка формирования предварительного просмотра отчета. ResearcherId: {ResearcherId}",
                    model?.ResearcherId);

                return BadRequest(ex.Message);
            }
        }

        [HttpPost]
        public IActionResult DownloadPdf([FromBody] ResearcherReportSettingsBindingModel model)
        {
            try
            {
                var fileBytes = _researcherReportLogic.GeneratePdf(model);

                var fileName = BuildSafeFileName(model.ReportTitle, "pdf");

                return File(fileBytes, "application/pdf", fileName);
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Ошибка формирования PDF-отчета. ResearcherId: {ResearcherId}",
                    model?.ResearcherId);

                return BadRequest(ex.Message);
            }
        }

        [HttpPost]
        public IActionResult DownloadDocx([FromBody] ResearcherReportSettingsBindingModel model)
        {
            try
            {
                var fileBytes = _researcherReportLogic.GenerateDocx(model);

                var fileName = BuildSafeFileName(model.ReportTitle, "docx");

                return File(
                    fileBytes,
                    "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
                    fileName);
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Ошибка формирования DOCX-отчета. ResearcherId: {ResearcherId}",
                    model?.ResearcherId);

                return BadRequest(ex.Message);
            }
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
