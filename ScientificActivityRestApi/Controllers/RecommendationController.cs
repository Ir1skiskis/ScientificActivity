using Microsoft.AspNetCore.Mvc;
using ScientificActivityBusinessLogics.BusinessLogics;
using ScientificActivityContracts.BindingModels;
using ScientificActivityContracts.BusinessLogicsContracts;
using ScientificActivityContracts.ViewModels;

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

        [HttpGet]
        public ActionResult<List<TagViewModel>> GetResearcherTags(int researcherId)
        {
            try
            {
                return Ok(_logic.GetResearcherTags(researcherId));
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost]
        public IActionResult SaveResearcherTags([FromBody] SaveResearcherTagsBindingModel model)
        {
            try
            {
                _logic.SaveResearcherTags(model.ResearcherId, model.TagIds);
                return Ok();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}
