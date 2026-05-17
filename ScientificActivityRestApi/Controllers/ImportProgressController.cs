using Microsoft.AspNetCore.Mvc;
using ScientificActivityBusinessLogics.Services;

namespace ScientificActivityRestApi.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class ImportProgressController : ControllerBase
    {
        private readonly ImportProgressService _progressService;

        public ImportProgressController(ImportProgressService progressService)
        {
            _progressService = progressService;
        }

        [HttpGet]
        public IActionResult GetProgress(string jobId)
        {
            var job = _progressService.GetJob(jobId);

            if (job == null)
            {
                return NotFound("Задача импорта не найдена");
            }

            return Ok(job);
        }
    }
}
