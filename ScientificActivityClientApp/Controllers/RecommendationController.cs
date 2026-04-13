using Microsoft.AspNetCore.Mvc;
using ScientificActivityContracts.BindingModels;
using ScientificActivityContracts.ViewModels;

namespace ScientificActivityClientApp.Controllers
{
    public class RecommendationController : Controller
    {
        public IActionResult Index()
        {
            if (APIClient.Researcher == null)
            {
                return RedirectToAction("Enter", "Home");
            }

            var model = APIClient.GetRequest<RecommendationResultViewModel>(
                $"api/Recommendation/GetRecommendations?researcherId={APIClient.Researcher.Id}");

            return View("~/Views/Home/Recommendation.cshtml", model ?? new RecommendationResultViewModel());
        }

        [HttpPost]
        public IActionResult SaveResearcherTags(List<int> tagIds)
        {
            if (APIClient.Researcher == null)
            {
                return RedirectToAction("Enter", "Home");
            }

            APIClient.PostRequest("api/Tag/SaveResearcherTags", new ResearcherTagBindingModel
            {
                ResearcherId = APIClient.Researcher.Id,
                TagIds = tagIds ?? new List<int>()
            });

            TempData["Message"] = "Интересы для рекомендаций сохранены";
            return RedirectToAction("Profile", "Home");
        }
    }
}
