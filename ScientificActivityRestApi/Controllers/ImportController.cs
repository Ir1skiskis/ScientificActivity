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
                _logger.LogWarning("Файл не передан или пустой");
                return BadRequest("Файл не выбран");
            }

            _logger.LogInformation("Получен файл {FileName}, размер {Length}", file.FileName, file.Length);

            var extension = Path.GetExtension(file.FileName);
            if (!string.Equals(extension, ".pdf", StringComparison.OrdinalIgnoreCase))
            {
                _logger.LogWarning("Передан не PDF-файл: {FileName}", file.FileName);
                return BadRequest("Можно загружать только PDF-файл");
            }

            string tempDirectory = Path.Combine(_environment.ContentRootPath, "TempImports");
            Directory.CreateDirectory(tempDirectory);

            string tempFileName = $"{Guid.NewGuid():N}{extension}";
            string tempFilePath = Path.Combine(tempDirectory, tempFileName);

            try
            {
                _logger.LogInformation("Сохраняем временный файл: {TempFilePath}", tempFilePath);

                await using (var stream = new FileStream(tempFilePath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }

                _logger.LogInformation("Файл сохранён, запускаем ImportVakJournalsAsync");

                var count = await _importService.ImportVakJournalsAsync(tempFilePath);

                _logger.LogInformation("Импорт журналов ВАК завершён. Count={Count}", count);

                return Ok($"Импорт журналов ВАК завершён. Обработано журналов: {count}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка импорта журналов ВАК");
                return BadRequest(ex.ToString());
            }
            finally
            {
                try
                {
                    if (System.IO.File.Exists(tempFilePath))
                    {
                        System.IO.File.Delete(tempFilePath);
                        _logger.LogInformation("Временный файл удалён: {TempFilePath}", tempFilePath);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Не удалось удалить временный PDF-файл {TempFilePath}", tempFilePath);
                }
            }
        }
    }
}
