using Microsoft.AspNetCore.Mvc;
using ScientificActivityParsers.Interfaces;

namespace ScientificActivityRestApi.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class ImportController : ControllerBase
    {
        private readonly IImportService _importService;
        private readonly ILogger<ImportController> _logger;

        public ImportController(IImportService importService, ILogger<ImportController> logger)
        {
            _importService = importService;
            _logger = logger;
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
    }
}
