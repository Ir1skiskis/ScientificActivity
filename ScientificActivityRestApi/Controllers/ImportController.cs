using Microsoft.AspNetCore.Mvc;
using ScientificActivityParsers.Interfaces;
using ScientificActivityRestApi.Models;

namespace ScientificActivityRestApi.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class ImportController : ControllerBase
    {
        private readonly IImportService _importService;
        private readonly ILogger<ImportController> _logger;
        private readonly IWebHostEnvironment _environment;

        public ImportController(
            IImportService importService,
            ILogger<ImportController> logger,
            IWebHostEnvironment environment)
        {
            _importService = importService;
            _logger = logger;
            _environment = environment;
        }

        [HttpPost]
        public async Task<IActionResult> ImportGrants()
        {
            try
            {
                var count = await _importService.ImportGrantsAsync();
                return Ok(new { Message = $"Импорт грантов завершён. Обработано записей: {count}" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка импорта грантов");
                return BadRequest(ex.Message);
            }
        }

        [HttpPost]
        public async Task<IActionResult> ImportConferences()
        {
            try
            {
                var count = await _importService.ImportConferencesAsync();
                return Ok(new { Message = $"Импорт конференций завершён. Обработано записей: {count}" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка импорта конференций");
                return BadRequest(ex.Message);
            }
        }

        [HttpPost]
        [RequestSizeLimit(200_000_000)]
        public async Task<IActionResult> ImportVakJournals(IFormFile file)
        {
            _logger.LogInformation("Начало ImportVakJournals");

            if (file == null || file.Length == 0)
            {
                return BadRequest("Файл не выбран");
            }

            var extension = Path.GetExtension(file.FileName);
            if (!string.Equals(extension, ".pdf", StringComparison.OrdinalIgnoreCase))
            {
                return BadRequest("Можно загружать только PDF-файл");
            }

            string tempDirectory = Path.Combine(_environment.ContentRootPath, "TempImports");
            Directory.CreateDirectory(tempDirectory);

            string tempFileName = $"{Guid.NewGuid():N}{extension}";
            string tempFilePath = Path.Combine(tempDirectory, tempFileName);

            try
            {
                await using (var stream = new FileStream(tempFilePath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }

                var count = await _importService.ImportVakJournalsAsync(tempFilePath);

                return Ok(new
                {
                    Message = $"Импорт ВАК завершён. Обработано журналов: {count}"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка импорта ВАК");
                return BadRequest(ex.ToString());
            }
            finally
            {
                try
                {
                    if (System.IO.File.Exists(tempFilePath))
                    {
                        System.IO.File.Delete(tempFilePath);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Не удалось удалить временный PDF-файл {Path}", tempFilePath);
                }
            }
        }

        [HttpPost]
        public async Task<IActionResult> ImportWhiteListJournals()
        {
            try
            {
                var count = await _importService.ImportWhiteListJournalsAsync();
                return Ok(new
                {
                    Message = $"Импорт Белого списка завершён. Обработано журналов: {count}"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка импорта белого списка");
                return BadRequest(ex.ToString());
            }
        }

        [HttpPost]
        public async Task<IActionResult> EnrichWhiteListRcsiLinks()
        {
            try
            {
                var count = await _importService.EnrichWhiteListRcsiLinksAsync();
                return Ok(new
                {
                    Message = $"Заполнение RcsiRecordSourceId и Url завершено. Обновлено журналов: {count}"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка заполнения RcsiRecordSourceId и Url");
                return BadRequest(ex.ToString());
            }
        }

        [HttpPost]
        public async Task<IActionResult> EnrichWhiteListSubjectAreas()
        {
            try
            {
                var count = await _importService.EnrichWhiteListSubjectAreasAsync();
                return Ok(new
                {
                    Message = $"Обогащение тематик завершено. Обновлено журналов: {count}"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка обогащения тематик из РЦНИ");
                return BadRequest(ex.ToString());
            }
        }

        [HttpPost]
        [RequestSizeLimit(200_000_000)]
        public async Task<IActionResult> ImportAllJournals(IFormFile file)
        {
            _logger.LogInformation("Начало ImportAllJournals");

            if (file == null || file.Length == 0)
            {
                return BadRequest("Файл не выбран");
            }

            var extension = Path.GetExtension(file.FileName);
            if (!string.Equals(extension, ".pdf", StringComparison.OrdinalIgnoreCase))
            {
                return BadRequest("Можно загружать только PDF-файл");
            }

            string tempDirectory = Path.Combine(_environment.ContentRootPath, "TempImports");
            Directory.CreateDirectory(tempDirectory);

            string tempFileName = $"{Guid.NewGuid():N}{extension}";
            string tempFilePath = Path.Combine(tempDirectory, tempFileName);

            try
            {
                await using (var stream = new FileStream(tempFilePath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }

                var result = await _importService.ImportAllJournalsAsync(tempFilePath);

                return Ok(new
                {
                    Message = $"Импорт завершён. ВАК: {result.VakCount}, Белый список: {result.WhiteListCount}"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка общего импорта журналов");
                return BadRequest(ex.ToString());
            }
            finally
            {
                try
                {
                    if (System.IO.File.Exists(tempFilePath))
                    {
                        System.IO.File.Delete(tempFilePath);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Не удалось удалить временный PDF-файл {Path}", tempFilePath);
                }
            }
        }
    }
}
