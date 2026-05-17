using Microsoft.AspNetCore.Mvc;
using ScientificActivityBusinessLogics.BusinessLogics;
using ScientificActivityBusinessLogics.Services;
using ScientificActivityContracts.BindingModels;
using ScientificActivityContracts.BusinessLogicsContracts;
using ScientificActivityContracts.ViewModels;

namespace ScientificActivityRestApi.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class ELibraryController : ControllerBase
    {
        private readonly IELibraryLogic _logic;
        private readonly ImportProgressService _progressService;
        private readonly IServiceScopeFactory _scopeFactory;

        public ELibraryController(IELibraryLogic logic, ImportProgressService progressService,
            IServiceScopeFactory scopeFactory)
        {
            _logic = logic;
            _progressService = progressService;
            _scopeFactory = scopeFactory;
        }

        [HttpPost]
        public ActionResult<ImportProgressViewModel> StartImportAuthorPublications(ELibraryImportBindingModel model)
        {
            var job = _progressService.CreateJob("Импорт публикаций из eLibrary");

            Task.Run(() =>
            {
                using var scope = _scopeFactory.CreateScope();

                var scopedLogic = scope.ServiceProvider.GetRequiredService<IELibraryLogic>();
                var scopedProgress = scope.ServiceProvider.GetRequiredService<ImportProgressService>();

                try
                {
                    scopedProgress.Update(job.JobId, "Подготовка импорта публикаций", percent: 5);

                    var processedCount = scopedLogic.ImportAuthorPublications(model, job.JobId);

                    scopedProgress.Complete(
                        job.JobId,
                        $"Импорт публикаций завершен. Обработано записей: {processedCount}");
                }
                catch (Exception ex)
                {
                    scopedProgress.Fail(job.JobId, ex);
                }
            });

            return Ok(job);
        }

        [HttpPost]
        public IActionResult SearchAuthors([FromBody] ELibraryAuthorSearchBindingModel model)
        {
            try
            {
                return Ok(_logic.SearchAuthors(model));
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet]
        public IActionResult GetAuthorProfile(string authorId)
        {
            try
            {
                var result = _logic.GetAuthorProfile(authorId);
                if (result == null)
                {
                    return NotFound("Автор не найден");
                }

                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost]
        public IActionResult BindAuthorToResearcher([FromBody] ELibraryBindAuthorBindingModel model)
        {
            try
            {
                return Ok(_logic.BindAuthorToResearcher(model));
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost]
        public IActionResult ImportAuthorProfile([FromBody] ELibraryImportBindingModel model)
        {
            try
            {
                return Ok(_logic.ImportAuthorProfile(model));
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet]
        public IActionResult GetStoredAuthorProfile(int researcherId)
        {
            try
            {
                var result = _logic.GetStoredAuthorProfile(researcherId);
                if (result == null)
                {
                    return NotFound("Импортированный профиль eLibrary не найден");
                }

                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost]
        public IActionResult ImportAuthorPublications([FromBody] ELibraryImportBindingModel model)
        {
            try
            {
                return Ok(_logic.ImportAuthorPublications(model));
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost]
        public IActionResult OpenManualLogin()
        {
            try
            {
                _logic.OpenELibraryForManualLogin();
                return Ok("Окно eLibrary было открыто, авторизация сохранена в Selenium-профиле.");
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}
