using Microsoft.AspNetCore.Mvc;
using ScientificActivityContracts.BindingModels;
using ScientificActivityContracts.BusinessLogicsContracts;
using ScientificActivityContracts.SearchModels;

namespace ScientificActivityRestApi.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class ResearcherController : ControllerBase
    {
        private readonly IResearcherLogic _researcherLogic;
        private readonly ILogger<ResearcherController> _logger;

        public ResearcherController(IResearcherLogic researcherLogic, ILogger<ResearcherController> logger)
        {
            _researcherLogic = researcherLogic;
            _logger = logger;
        }

        [HttpGet]
        public IActionResult GetAllResearchers()
        {
            try
            {
                var result = _researcherLogic.ReadList(null);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка получения списка исследователей");
                return BadRequest(ex.Message);
            }
        }

        [HttpGet]
        public IActionResult GetResearchersByFilter(
            int? id,
            string? email,
            string? lastName,
            string? firstName,
            string? department,
            string? position,
            string? eLibraryAuthorId,
            bool? isActive)
        {
            try
            {
                var result = _researcherLogic.ReadList(new ResearcherSearchModel
                {
                    Id = id,
                    Email = email,
                    LastName = lastName,
                    FirstName = firstName,
                    Department = department,
                    Position = position,
                    ELibraryAuthorId = eLibraryAuthorId,
                    IsActive = isActive
                });

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка фильтрации исследователей");
                return BadRequest(ex.Message);
            }
        }

        [HttpGet]
        public IActionResult GetResearcherById(int id)
        {
            try
            {
                var result = _researcherLogic.ReadElement(new ResearcherSearchModel { Id = id });
                if (result == null)
                {
                    return NotFound("Исследователь не найден");
                }

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка получения исследователя по id={Id}", id);
                return BadRequest(ex.Message);
            }
        }

        [HttpPost]
        public IActionResult CreateResearcher([FromBody] ResearcherBindingModel model)
        {
            try
            {
                var success = _researcherLogic.Create(model);
                if (!success)
                {
                    return BadRequest("Не удалось создать исследователя");
                }

                return Ok(new { Message = "Исследователь успешно создан" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка создания исследователя");
                return BadRequest(ex.Message);
            }
        }

        [HttpPost]
        public IActionResult UpdateResearcher([FromBody] ResearcherBindingModel model)
        {
            try
            {
                var success = _researcherLogic.Update(model);
                if (!success)
                {
                    return BadRequest("Не удалось обновить исследователя");
                }

                return Ok(new { Message = "Исследователь успешно обновлён" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка обновления исследователя");
                return BadRequest(ex.Message);
            }
        }

        [HttpPost]
        public IActionResult DeleteResearcher([FromBody] ResearcherBindingModel model)
        {
            try
            {
                var success = _researcherLogic.Delete(model);
                if (!success)
                {
                    return BadRequest("Не удалось удалить исследователя");
                }

                return Ok(new { Message = "Исследователь успешно удалён" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка удаления исследователя");
                return BadRequest(ex.Message);
            }
        }
    }
}
