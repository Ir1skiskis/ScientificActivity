using Newtonsoft.Json;
using ScientificActivityContracts.BindingModels;
using ScientificActivityContracts.ViewModels;
using System.Net.Http.Headers;
using System.Text;

namespace ScientificActivityClientApp
{
    public static class APIClient
    {
        private static readonly HttpClient _httpClient = new();

        public static string ApiAddress { get; set; } = "https://localhost:7173";

        public static ResearcherViewModel? Researcher { get; set; }

        public static void Connect(IConfiguration configuration)
        {
            var apiUrl = configuration["ApiUrl"];
            if (string.IsNullOrWhiteSpace(apiUrl))
            {
                throw new InvalidOperationException("Не задан адрес Rest API в конфигурации");
            }

            _httpClient.BaseAddress = new Uri(apiUrl);

            _httpClient.Timeout = TimeSpan.FromHours(3);

            _httpClient.DefaultRequestHeaders.Accept.Clear();
            _httpClient.DefaultRequestHeaders.Accept.Add(
                new MediaTypeWithQualityHeaderValue("application/json"));
        }

        public static T? GetRequest<T>(string requestUrl)
        {
            var response = _httpClient.GetAsync(requestUrl).Result;
            var responseText = response.Content.ReadAsStringAsync().Result;

            if (response.IsSuccessStatusCode)
            {
                return response.Content.ReadFromJsonAsync<T>().Result;
            }

            throw new Exception($"GET {requestUrl} завершился ошибкой. StatusCode: {(int)response.StatusCode}. Ответ API: {responseText}");
        }

        public static void PostRequest<T>(string requestUrl, T model)
        {
            var json = JsonConvert.SerializeObject(model);
            var data = new StringContent(json, Encoding.UTF8, "application/json");

            var response = _httpClient.PostAsync(requestUrl, data).Result;
            var result = response.Content.ReadAsStringAsync().Result;

            if (!response.IsSuccessStatusCode)
            {
                throw new Exception(result);
            }
        }

        public static TResponse? PostRequestWithResponse<TRequest, TResponse>(string requestUrl, TRequest model)
        {
            var json = JsonConvert.SerializeObject(model);
            var data = new StringContent(json, Encoding.UTF8, "application/json");

            var response = _httpClient.PostAsync(requestUrl, data).Result;
            var result = response.Content.ReadAsStringAsync().Result;

            if (response.IsSuccessStatusCode)
            {
                return JsonConvert.DeserializeObject<TResponse>(result);
            }

            throw new Exception(result);
        }

        public static async Task<TResponse?> PostRequestAsync<TRequest, TResponse>(string requestUrl, TRequest model)
        {
            var json = JsonConvert.SerializeObject(model);
            var data = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync(requestUrl, data);
            var result = await response.Content.ReadAsStringAsync();

            if (response.IsSuccessStatusCode)
            {
                return JsonConvert.DeserializeObject<TResponse>(result);
            }

            throw new Exception(result);
        }

        public static async Task<byte[]?> PostFileRequestAsync<TRequest>(string requestUrl, TRequest model)
        {
            var json = JsonConvert.SerializeObject(model);
            var data = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync(requestUrl, data);
            var result = await response.Content.ReadAsStringAsync();

            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadAsByteArrayAsync();
            }

            throw new Exception(result);
        }

        public static async Task<ResearcherReportViewModel?> GetResearcherReportPreviewAsync(ResearcherReportSettingsBindingModel model)
        {
            return await PostRequestAsync<ResearcherReportSettingsBindingModel, ResearcherReportViewModel>(
                "api/ResearcherReport/Preview",
                model);
        }

        public static async Task<byte[]?> DownloadResearcherReportPdfAsync(ResearcherReportSettingsBindingModel model)
        {
            return await PostFileRequestAsync(
                "api/ResearcherReport/DownloadPdf",
                model);
        }

        public static async Task<byte[]?> DownloadResearcherReportDocxAsync(ResearcherReportSettingsBindingModel model)
        {
            return await PostFileRequestAsync(
                "api/ResearcherReport/DownloadDocx",
                model);
        }

        public static async Task<(bool Success, int Count, string Error)> RegenerateAllTagsAsync()
        {
            try
            {
                var response = await _httpClient.PostAsync("api/TagGeneration/RegenerateAllTags", null);
                var responseText = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    return (false, 0, responseText);
                }

                if (int.TryParse(responseText, out var count))
                {
                    return (true, count, string.Empty);
                }

                return (false, 0, $"RestApi вернул неожиданный ответ: {responseText}");
            }
            catch (Exception ex)
            {
                return (false, 0, ex.Message);
            }
        }

        public static async Task<int?> RegenerateConferenceTagsAsync()
        {
            var response = await _httpClient.PostAsync("api/TagGeneration/RegenerateConferenceTags", null);

            if (!response.IsSuccessStatusCode)
            {
                return null;
            }

            return await response.Content.ReadFromJsonAsync<int>();
        }

        public static async Task<int?> RegenerateGrantTagsAsync()
        {
            var response = await _httpClient.PostAsync("api/TagGeneration/RegenerateGrantTags", null);

            if (!response.IsSuccessStatusCode)
            {
                return null;
            }

            return await response.Content.ReadFromJsonAsync<int>();
        }

        public static async Task<int?> RegenerateJournalTagsAsync()
        {
            var response = await _httpClient.PostAsync("api/TagGeneration/RegenerateJournalTags", null);

            if (!response.IsSuccessStatusCode)
            {
                return null;
            }

            return await response.Content.ReadFromJsonAsync<int>();
        }

        public static List<TagViewModel>? GetSelectableTags()
        {
            return GetRequest<List<TagViewModel>>("api/Tag/GetSelectableTags");
        }

        public static List<TagViewModel>? GetResearcherTags(int researcherId)
        {
            return GetRequest<List<TagViewModel>>($"api/Recommendation/GetResearcherTags?researcherId={researcherId}");
        }

        public static bool SaveResearcherTags(int researcherId, List<int> tagIds)
        {
            var model = new SaveResearcherTagsBindingModel
            {
                ResearcherId = researcherId,
                TagIds = tagIds
            };

            var response = _httpClient.PostAsJsonAsync("api/Recommendation/SaveResearcherTags", model).Result;

            return response.IsSuccessStatusCode;
        }

        public static ELibraryAuthorProfileViewModel? GetStoredELibraryProfile(int researcherId)
        {
            var response = _httpClient.GetAsync($"api/ELibrary/GetStoredAuthorProfile?researcherId={researcherId}").Result;

            if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                return null;
            }

            var responseText = response.Content.ReadAsStringAsync().Result;

            if (!response.IsSuccessStatusCode)
            {
                throw new Exception($"GET api/ELibrary/GetStoredAuthorProfile?researcherId={researcherId} завершился ошибкой. StatusCode: {(int)response.StatusCode}. Ответ API: {responseText}");
            }

            return response.Content.ReadFromJsonAsync<ELibraryAuthorProfileViewModel>().Result;
        }

        public static bool ChangePassword(ChangePasswordBindingModel model)
        {
            try
            {
                var response = _httpClient.PostAsJsonAsync("api/Researcher/ChangePassword", model).Result;

                if (!response.IsSuccessStatusCode)
                {
                    var error = response.Content.ReadAsStringAsync().Result;
                    throw new Exception($"Смена пароля завершилась ошибкой. StatusCode: {(int)response.StatusCode}. Ответ API: {error}");
                }

                return response.Content.ReadFromJsonAsync<bool>().Result;
            }
            catch
            {
                throw;
            }
        }
    }
}
