using Microsoft.AspNetCore.Mvc;
using ScientificActivityContracts.BindingModels;
using ScientificActivityContracts.BusinessLogicsContracts;
using ScientificActivityContracts.SearchModels;
using ScientificActivityDataModels.Enums;

namespace ScientificActivityRestApi.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class ConferenceController : ControllerBase
    {
        private readonly IConferenceLogic _conferenceLogic;
        private readonly ILogger<ConferenceController> _logger;

        public ConferenceController(IConferenceLogic conferenceLogic, ILogger<ConferenceController> logger)
        {
            _conferenceLogic = conferenceLogic;
            _logger = logger;
        }

        [HttpGet]
        public IActionResult GetAllConferences()
        {
            try
            {
                var result = _conferenceLogic.ReadList(null);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка получения списка конференций");
                return BadRequest(ex.Message);
            }
        }

        [HttpGet]
        public IActionResult GetConferencesByFilter(
            int? id,
            string? title,
            string? city,
            string? country,
            string? subjectArea,
            ConferenceFormat? format,
            ConferenceLevel? level,
            DateTime? dateFrom,
            DateTime? dateTo)
        {
            try
            {
                var result = _conferenceLogic.ReadList(new ConferenceSearchModel
                {
                    Id = id,
                    Title = title,
                    City = city,
                    Country = country,
                    SubjectArea = subjectArea,
                    Format = format,
                    Level = level,
                    DateFrom = dateFrom,
                    DateTo = dateTo
                });

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка фильтрации конференций");
                return BadRequest(ex.Message);
            }
        }

        [HttpGet]
        public IActionResult GetConferenceById(int id)
        {
            try
            {
                var result = _conferenceLogic.ReadElement(new ConferenceSearchModel { Id = id });
                if (result == null)
                {
                    return NotFound("Конференция не найдена");
                }

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка получения конференции по id={Id}", id);
                return BadRequest(ex.Message);
            }
        }

        [HttpPost]
        public IActionResult CreateConference([FromBody] ConferenceBindingModel model)
        {
            try
            {
                var success = _conferenceLogic.Create(model);
                if (!success)
                {
                    return BadRequest("Не удалось создать конференцию");
                }

                return Ok(new { Message = "Конференция успешно создана" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка создания конференции");
                return BadRequest(ex.Message);
            }
        }

        [HttpPost]
        public IActionResult UpdateConference([FromBody] ConferenceBindingModel model)
        {
            try
            {
                var success = _conferenceLogic.Update(model);
                if (!success)
                {
                    return BadRequest("Не удалось обновить конференцию");
                }

                return Ok(new { Message = "Конференция успешно обновлена" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка обновления конференции");
                return BadRequest(ex.Message);
            }
        }

        [HttpPost]
        public IActionResult DeleteConference([FromBody] ConferenceBindingModel model)
        {
            try
            {
                var success = _conferenceLogic.Delete(model);
                if (!success)
                {
                    return BadRequest("Не удалось удалить конференцию");
                }

                return Ok(new { Message = "Конференция успешно удалена" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка удаления конференции");
                return BadRequest(ex.Message);
            }
        }
    }
}
