using Microsoft.AspNetCore.Mvc;
using ScientificActivityContracts.BindingModels;
using ScientificActivityContracts.BusinessLogicsContracts;
using ScientificActivityContracts.SearchModels;
using ScientificActivityDataModels.Enums;

namespace ScientificActivityRestApi.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class JournalController : ControllerBase
    {
        private readonly IJournalLogic _journalLogic;
        private readonly ILogger<JournalController> _logger;

        public JournalController(IJournalLogic journalLogic, ILogger<JournalController> logger)
        {
            _journalLogic = journalLogic;
            _logger = logger;
        }

        [HttpGet]
        public IActionResult GetAllJournals()
        {
            try
            {
                var result = _journalLogic.ReadList(null);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка получения списка журналов");
                return BadRequest(ex.Message);
            }
        }

        [HttpGet]
        public IActionResult GetJournalsByFilter(
            int? id,
            string? title,
            string? issn,
            string? subjectArea,
            JournalQuartile? quartile,
            bool? isVak,
            bool? isWhiteList)
        {
            try
            {
                var result = _journalLogic.ReadList(new JournalSearchModel
                {
                    Id = id,
                    Title = title,
                    Issn = issn,
                    SubjectArea = subjectArea,
                    Quartile = quartile,
                    IsVak = isVak,
                    IsWhiteList = isWhiteList
                });

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка фильтрации журналов");
                return BadRequest(ex.Message);
            }
        }

        [HttpGet]
        public IActionResult GetJournalById(int id)
        {
            try
            {
                var result = _journalLogic.ReadElement(new JournalSearchModel { Id = id });
                if (result == null)
                {
                    return NotFound("Журнал не найден");
                }

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка получения журнала по id={Id}", id);
                return BadRequest(ex.Message);
            }
        }

        [HttpPost]
        public IActionResult CreateJournal([FromBody] JournalBindingModel model)
        {
            try
            {
                var success = _journalLogic.Create(model);
                if (!success)
                {
                    return BadRequest("Не удалось создать журнал");
                }

                return Ok(new { Message = "Журнал успешно создан" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка создания журнала");
                return BadRequest(ex.Message);
            }
        }

        [HttpPost]
        public IActionResult UpdateJournal([FromBody] JournalBindingModel model)
        {
            try
            {
                var success = _journalLogic.Update(model);
                if (!success)
                {
                    return BadRequest("Не удалось обновить журнал");
                }

                return Ok(new { Message = "Журнал успешно обновлён" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка обновления журнала");
                return BadRequest(ex.Message);
            }
        }

        [HttpPost]
        public IActionResult DeleteJournal([FromBody] JournalBindingModel model)
        {
            try
            {
                var success = _journalLogic.Delete(model);
                if (!success)
                {
                    return BadRequest("Не удалось удалить журнал");
                }

                return Ok(new { Message = "Журнал успешно удалён" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка удаления журнала");
                return BadRequest(ex.Message);
            }
        }
    }
}
