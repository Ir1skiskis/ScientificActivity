using Newtonsoft.Json;
using ScientificActivityParsers.Interfaces;
using ScientificActivityParsers.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace ScientificActivityParsers.Services
{
    public class RcsiLevelApiClient : IRcsiLevelApiClient
    {
        private readonly HttpClient _httpClient;

        public RcsiLevelApiClient(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<RcsiLevelApiResponseModel?> GetByIssnAsync(string issn, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(issn))
            {
                return null;
            }

            var normalizedIssn = issn.Trim().ToUpperInvariant().Replace('Х', 'X').Replace('х', 'X');
            var url = $"https://journalrank.rcsi.science/api/record-sources/{WebUtility.UrlEncode(normalizedIssn)}/level";

            using var response = await _httpClient.GetAsync(url, cancellationToken);
            if (response.StatusCode == HttpStatusCode.BadRequest || response.StatusCode == HttpStatusCode.NotFound)
            {
                return null;
            }

            var json = await response.Content.ReadAsStringAsync(cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                throw new InvalidOperationException(
                    $"Ошибка запроса к API РЦНИ по ISSN '{normalizedIssn}'. Код: {(int)response.StatusCode}. Ответ: {json}");
            }

            if (string.IsNullOrWhiteSpace(json))
            {
                return null;
            }

            return JsonConvert.DeserializeObject<RcsiLevelApiResponseModel>(json);
        }
    }
}
