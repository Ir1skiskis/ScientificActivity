using Microsoft.AspNetCore.Mvc;
using ScientificActivityContracts.BindingModels;
using ScientificActivityContracts.BusinessLogicsContracts;
using ScientificActivityContracts.SearchModels;
using ScientificActivityDataModels.Enums;

namespace ScientificActivityRestApi.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class PublicationController : ControllerBase
    {
        private readonly IPublicationLogic _publicationLogic;
        private readonly ILogger<PublicationController> _logger;

        public PublicationController(IPublicationLogic publicationLogic, ILogger<PublicationController> logger)
        {
            _publicationLogic = publicationLogic;
            _logger = logger;
        }

        [HttpGet]
        public IActionResult GetAllPublications()
        {
            try
            {
                var result = _publicationLogic.ReadList(null);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка получения списка публикаций");
                return BadRequest(ex.Message);
            }
        }

        [HttpGet]
        public IActionResult GetPublicationsByFilter(
            int? id,
            int? researcherId,
            int? journalId,
            int? conferenceId,
            string? title,
            int? year,
            PublicationType? type,
            string? doi,
            string? keywords)
        {
            try
            {
                var result = _publicationLogic.ReadList(new PublicationSearchModel
                {
                    Id = id,
                    ResearcherId = researcherId,
                    JournalId = journalId,
                    ConferenceId = conferenceId,
                    Title = title,
                    Year = year,
                    Type = type,
                    Doi = doi,
                    Keywords = keywords
                });

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка фильтрации публикаций");
                return BadRequest(ex.Message);
            }
        }

        [HttpGet]
        public IActionResult GetPublicationById(int id)
        {
            try
            {
                var result = _publicationLogic.ReadElement(new PublicationSearchModel { Id = id });
                if (result == null)
                {
                    return NotFound("Публикация не найдена");
                }

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка получения публикации по id={Id}", id);
                return BadRequest(ex.Message);
            }
        }

        [HttpPost]
        public IActionResult CreatePublication([FromBody] PublicationBindingModel model)
        {
            try
            {
                var success = _publicationLogic.Create(model);
                if (!success)
                {
                    return BadRequest("Не удалось создать публикацию");
                }

                return Ok(new { Message = "Публикация успешно создана" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка создания публикации");
                return BadRequest(ex.Message);
            }
        }

        [HttpPost]
        public IActionResult UpdatePublication([FromBody] PublicationBindingModel model)
        {
            try
            {
                var success = _publicationLogic.Update(model);
                if (!success)
                {
                    return BadRequest("Не удалось обновить публикацию");
                }

                return Ok(new { Message = "Публикация успешно обновлена" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка обновления публикации");
                return BadRequest(ex.Message);
            }
        }

        [HttpPost]
        public IActionResult DeletePublication([FromBody] PublicationBindingModel model)
        {
            try
            {
                var success = _publicationLogic.Delete(model);
                if (!success)
                {
                    return BadRequest("Не удалось удалить публикацию");
                }

                return Ok(new { Message = "Публикация успешно удалена" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка удаления публикации");
                return BadRequest(ex.Message);
            }
        }
    }
}
