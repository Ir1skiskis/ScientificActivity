using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using ScientificActivityParsers.Interfaces;
using ScientificActivityParsers.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace ScientificActivityParsers.Services
{
    public class RcsiApiClient : IRcsiApiClient
    {
        private readonly HttpClient _httpClient;

        private const string RecordSourcesJsonUrl =
            "https://journalrank.rcsi.science/ru/record-sources/download/?dataType=Json";

        public RcsiApiClient(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<List<RcsiRecordSourceModel>> GetAllRecordSourcesAsync(CancellationToken cancellationToken = default)
        {
            using var request = new HttpRequestMessage(HttpMethod.Get, RecordSourcesJsonUrl);
            request.Headers.Accept.Clear();
            request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("text/plain"));
            request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("*/*"));

            using var response = await _httpClient.SendAsync(request, cancellationToken);
            var json = await response.Content.ReadAsStringAsync(cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                throw new InvalidOperationException(
                    $"Не удалось получить JSON Белого списка РЦНИ. Код: {(int)response.StatusCode}. Ответ: {json}");
            }

            if (string.IsNullOrWhiteSpace(json))
            {
                throw new InvalidOperationException("РЦНИ вернул пустой JSON");
            }

            var items = JsonConvert.DeserializeObject<List<RcsiRecordSourceModel>>(json);
            return items ?? new List<RcsiRecordSourceModel>();
        }
    }
}
