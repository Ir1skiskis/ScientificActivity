using Microsoft.AspNetCore.Mvc;
using ScientificActivityContracts.BusinessLogicsContracts;

namespace ScientificActivityRestApi.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class RecommendationController : ControllerBase
    {
        private readonly IRecommendationLogic _logic;

        public RecommendationController(IRecommendationLogic logic)
        {
            _logic = logic;
        }

        [HttpGet]
        public IActionResult GetRecommendations(int researcherId)
        {
            try
            {
                return Ok(_logic.GetRecommendations(researcherId));
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}
