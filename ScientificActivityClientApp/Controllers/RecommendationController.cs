using Microsoft.AspNetCore.Mvc;
using ScientificActivityContracts.BindingModels;
using ScientificActivityContracts.ViewModels;
using ScientificActivityDataModels.Enums;

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
                return RedirectToAction("Login", "Researcher");
            }

            var result = APIClient.SaveResearcherTags(APIClient.Researcher.Id, tagIds ?? new List<int>());

            if (result)
            {
                TempData["Message"] = "Интересы для рекомендаций успешно сохранены";
            }
            else
            {
                TempData["Error"] = "Не удалось сохранить интересы для рекомендаций";
            }

            return RedirectToAction("Profile", "Home");
        }

        [HttpPost]
        public async Task<IActionResult> RegenerateTags()
        {
            if (APIClient.Researcher == null)
            {
                return RedirectToAction("Login", "Researcher");
            }

            if (APIClient.Researcher.Role != UserRole.Администратор)
            {
                TempData["Error"] = "Генерация тегов доступна только администратору";
                return RedirectToAction("Index");
            }

            var result = await APIClient.RegenerateAllTagsAsync();

            if (!result.Success)
            {
                TempData["Error"] = $"Не удалось сформировать теги. Ошибка: {result.Error}";
            }
            else
            {
                TempData["Message"] = $"Теги успешно сформированы. Создано связей: {result.Count}";
            }

            return RedirectToAction("Index");
        }
    }
}
