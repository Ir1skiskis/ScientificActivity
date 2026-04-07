using Microsoft.AspNetCore.Mvc;
using ScientificActivityContracts.BindingModels;
using ScientificActivityContracts.BusinessLogicsContracts;
using ScientificActivityContracts.SearchModels;
using ScientificActivityDataModels.Enums;

namespace ScientificActivityRestApi.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IResearcherLogic _researcherLogic;
        private readonly ILogger<AuthController> _logger;

        public AuthController(IResearcherLogic researcherLogic, ILogger<AuthController> logger)
        {
            _researcherLogic = researcherLogic;
            _logger = logger;
        }

        [HttpPost]
        public IActionResult Register([FromBody] ResearcherBindingModel model)
        {
            try
            {
                model.Role = UserRole.Исследователь;
                model.IsActive = true;

                var success = _researcherLogic.Create(model);

                if (!success)
                {
                    return BadRequest("Не удалось зарегистрировать пользователя");
                }

                return Ok(new { Message = "Регистрация успешна" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка регистрации");
                return BadRequest(ex.Message);
            }
        }

        [HttpGet]
        public IActionResult Login(string email, string password)
        {
            try
            {
                var researcher = _researcherLogic.ReadElement(new ResearcherSearchModel
                {
                    Email = email.Trim(),
                    PasswordHash = password
                });

                if (researcher == null)
                {
                    return NotFound("Неверный email или пароль");
                }

                return Ok(researcher);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка входа");
                return BadRequest(ex.Message);
            }
        }
    }
}
