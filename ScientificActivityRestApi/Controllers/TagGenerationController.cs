using Microsoft.AspNetCore.Mvc;
using ScientificActivityContracts.BusinessLogicsContracts;

namespace ScientificActivityRestApi.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class TagGenerationController : ControllerBase
    {
        private readonly ITagGenerationLogic _tagGenerationLogic;

        public TagGenerationController(ITagGenerationLogic tagGenerationLogic)
        {
            _tagGenerationLogic = tagGenerationLogic;
        }

        [HttpPost]
        public ActionResult<int> RegenerateAllTags()
        {
            try
            {
                var count = _tagGenerationLogic.RegenerateAllTags();
                return Ok(count);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost]
        public ActionResult<int> RegenerateConferenceTags()
        {
            try
            {
                var count = _tagGenerationLogic.RegenerateConferenceTags();
                return Ok(count);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost]
        public ActionResult<int> RegenerateGrantTags()
        {
            try
            {
                var count = _tagGenerationLogic.RegenerateGrantTags();
                return Ok(count);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost]
        public ActionResult<int> RegenerateJournalTags()
        {
            try
            {
                var count = _tagGenerationLogic.RegenerateJournalTags();
                return Ok(count);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}
