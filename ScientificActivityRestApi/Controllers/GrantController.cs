using Microsoft.AspNetCore.Mvc;
using ScientificActivityContracts.BindingModels;
using ScientificActivityContracts.BusinessLogicsContracts;
using ScientificActivityContracts.SearchModels;
using ScientificActivityDataModels.Enums;

namespace ScientificActivityRestApi.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class GrantController : ControllerBase
    {
        private readonly IGrantLogic _grantLogic;
        private readonly ILogger<GrantController> _logger;

        public GrantController(IGrantLogic grantLogic, ILogger<GrantController> logger)
        {
            _grantLogic = grantLogic;
            _logger = logger;
        }

        [HttpGet]
        public IActionResult GetAllGrants()
        {
            try
            {
                var result = _grantLogic.ReadList(null);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка получения списка грантов");
                return BadRequest(ex.Message);
            }
        }

        [HttpGet]
        public IActionResult GetGrantsByFilter(
            int? id,
            string? title,
            string? organization,
            string? subjectArea,
            GrantStatus? status,
            DateTime? dateFrom,
            DateTime? dateTo)
        {
            try
            {
                var result = _grantLogic.ReadList(new GrantSearchModel
                {
                    Id = id,
                    Title = title,
                    Organization = organization,
                    SubjectArea = subjectArea,
                    Status = status,
                    DateFrom = dateFrom,
                    DateTo = dateTo
                });

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка фильтрации грантов");
                return BadRequest(ex.Message);
            }
        }

        [HttpGet]
        public IActionResult GetGrantById(int id)
        {
            try
            {
                var result = _grantLogic.ReadElement(new GrantSearchModel { Id = id });
                if (result == null)
                {
                    return NotFound("Грант не найден");
                }

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка получения гранта по id={Id}", id);
                return BadRequest(ex.Message);
            }
        }

        [HttpPost]
        public IActionResult CreateGrant([FromBody] GrantBindingModel model)
        {
            try
            {
                var success = _grantLogic.Create(model);
                if (!success)
                {
                    return BadRequest("Не удалось создать грант");
                }

                return Ok(new { Message = "Грант успешно создан" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка создания гранта");
                return BadRequest(ex.Message);
            }
        }

        [HttpPost]
        public IActionResult UpdateGrant([FromBody] GrantBindingModel model)
        {
            try
            {
                var success = _grantLogic.Update(model);
                if (!success)
                {
                    return BadRequest("Не удалось обновить грант");
                }

                return Ok(new { Message = "Грант успешно обновлён" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка обновления гранта");
                return BadRequest(ex.Message);
            }
        }

        [HttpPost]
        public IActionResult DeleteGrant([FromBody] GrantBindingModel model)
        {
            try
            {
                var success = _grantLogic.Delete(model);
                if (!success)
                {
                    return BadRequest("Не удалось удалить грант");
                }

                return Ok(new { Message = "Грант успешно удалён" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка удаления гранта");
                return BadRequest(ex.Message);
            }
        }
    }
}
