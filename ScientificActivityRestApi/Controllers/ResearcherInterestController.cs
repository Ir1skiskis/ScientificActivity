using Microsoft.AspNetCore.Mvc;
using ScientificActivityContracts.BindingModels;
using ScientificActivityContracts.BusinessLogicsContracts;
using ScientificActivityContracts.SearchModels;

namespace ScientificActivityRestApi.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class ResearcherInterestController : ControllerBase
    {
        private readonly IResearcherInterestLogic _logic;
        private readonly ILogger<ResearcherInterestController> _logger;

        public ResearcherInterestController(
            IResearcherInterestLogic logic,
            ILogger<ResearcherInterestController> logger)
        {
            _logic = logic;
            _logger = logger;
        }

        // -------------------- Получить все --------------------

        [HttpGet]
        public IActionResult GetAll()
        {
            try
            {
                var result = _logic.ReadList(null);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка получения интересов");
                return BadRequest(ex.Message);
            }
        }

        // -------------------- По исследователю --------------------

        [HttpGet]
        public IActionResult GetByResearcher(int researcherId)
        {
            try
            {
                var result = _logic.ReadList(new ResearcherInterestSearchModel
                {
                    ResearcherId = researcherId
                });

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка получения интересов по исследователю");
                return BadRequest(ex.Message);
            }
        }

        // -------------------- Добавить --------------------

        [HttpPost]
        public IActionResult Create([FromBody] ResearcherInterestBindingModel model)
        {
            try
            {
                var success = _logic.Create(model);

                if (!success)
                {
                    return BadRequest("Не удалось добавить интерес");
                }

                return Ok(new { Message = "Интерес добавлен" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка создания интереса");
                return BadRequest(ex.Message);
            }
        }

        // -------------------- Обновить --------------------

        [HttpPost]
        public IActionResult Update([FromBody] ResearcherInterestBindingModel model)
        {
            try
            {
                var success = _logic.Update(model);

                if (!success)
                {
                    return BadRequest("Не удалось обновить интерес");
                }

                return Ok(new { Message = "Интерес обновлён" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка обновления интереса");
                return BadRequest(ex.Message);
            }
        }

        // -------------------- Удалить --------------------

        [HttpPost]
        public IActionResult Delete([FromBody] ResearcherInterestBindingModel model)
        {
            try
            {
                var success = _logic.Delete(model);

                if (!success)
                {
                    return BadRequest("Не удалось удалить интерес");
                }

                return Ok(new { Message = "Интерес удалён" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка удаления интереса");
                return BadRequest(ex.Message);
            }
        }
    }
}
