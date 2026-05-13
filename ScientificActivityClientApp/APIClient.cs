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
            var result = response.Content.ReadAsStringAsync().Result;

            if (response.IsSuccessStatusCode)
            {
                return JsonConvert.DeserializeObject<T>(result);
            }

            throw new Exception(result);
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
    }
}
