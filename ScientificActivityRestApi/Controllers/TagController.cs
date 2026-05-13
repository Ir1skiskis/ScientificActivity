using Microsoft.AspNetCore.Mvc;
using ScientificActivityContracts.BindingModels;
using ScientificActivityContracts.BusinessLogicsContracts;
using ScientificActivityContracts.ViewModels;

namespace ScientificActivityRestApi.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class TagController : ControllerBase
    {
        private readonly ITagLogic _tagLogic;

        public TagController(ITagLogic tagLogic)
        {
            _tagLogic = tagLogic;
        }

        [HttpGet]
        public ActionResult<List<TagViewModel>> GetSelectableTags()
        {
            try
            {
                return Ok(_tagLogic.GetSelectableTags());
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet]
        public ActionResult<List<TagViewModel>> GetConferenceTags()
        {
            try
            {
                return Ok(_tagLogic.GetConferenceTags());
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet]
        public ActionResult<List<TagViewModel>> GetGrantTags()
        {
            try
            {
                return Ok(_tagLogic.GetGrantTags());
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet]
        public ActionResult<List<TagViewModel>> GetJournalTags()
        {
            try
            {
                return Ok(_tagLogic.GetJournalTags());
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}
