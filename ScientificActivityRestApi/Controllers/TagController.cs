using Microsoft.AspNetCore.Mvc;
using ScientificActivityContracts.BindingModels;
using ScientificActivityContracts.BusinessLogicsContracts;

namespace ScientificActivityRestApi.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class TagController : ControllerBase
    {
        private readonly ITagLogic _logic;

        public TagController(ITagLogic logic)
        {
            _logic = logic;
        }

        [HttpGet]
        public IActionResult GetSelectableTags()
        {
            try
            {
                return Ok(_logic.ReadList(true));
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet]
        public IActionResult GetResearcherTags(int researcherId)
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
        public IActionResult SaveResearcherTags([FromBody] ResearcherTagBindingModel model)
        {
            try
            {
                _logic.SaveResearcherTags(model);
                return Ok(true);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}
