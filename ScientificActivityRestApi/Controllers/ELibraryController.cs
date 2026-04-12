using Microsoft.AspNetCore.Mvc;
using ScientificActivityContracts.BindingModels;
using ScientificActivityContracts.BusinessLogicsContracts;

namespace ScientificActivityRestApi.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class ELibraryController : ControllerBase
    {
        private readonly IELibraryLogic _logic;

        public ELibraryController(IELibraryLogic logic)
        {
            _logic = logic;
        }

        [HttpPost]
        public IActionResult SearchAuthors([FromBody] ELibraryAuthorSearchBindingModel model)
        {
            try
            {
                return Ok(_logic.SearchAuthors(model));
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet]
        public IActionResult GetAuthorProfile(string authorId)
        {
            try
            {
                var result = _logic.GetAuthorProfile(authorId);
                if (result == null)
                {
                    return NotFound("Автор не найден");
                }

                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost]
        public IActionResult BindAuthorToResearcher([FromBody] ELibraryBindAuthorBindingModel model)
        {
            try
            {
                return Ok(_logic.BindAuthorToResearcher(model));
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost]
        public IActionResult ImportAuthorProfile([FromBody] ELibraryImportBindingModel model)
        {
            try
            {
                return Ok(_logic.ImportAuthorProfile(model));
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost]
        public IActionResult ImportAuthorPublications([FromBody] ELibraryImportBindingModel model)
        {
            try
            {
                return Ok(_logic.ImportAuthorPublications(model));
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}
