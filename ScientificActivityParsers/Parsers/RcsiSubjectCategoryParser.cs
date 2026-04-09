using HtmlAgilityPack;
using ScientificActivityParsers.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace ScientificActivityParsers.Parsers
{
    public class RcsiSubjectCategoryParser : IRcsiSubjectCategoryParser
    {
        private readonly HttpClient _httpClient;

        private static readonly Regex RecordSourceIdRegex = new(
            @"/record-sources/(?:details|subject-categories|indicators|levels|quartiles|ratings|links|analysis)/(?<id>\d+)/?",
            RegexOptions.Compiled | RegexOptions.IgnoreCase);

        public RcsiSubjectCategoryParser(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<List<string>> ParseCategoriesAsync(
            int? recordSourceId,
            string? journalUrl,
            CancellationToken cancellationToken = default)
        {
            var resolvedId = recordSourceId ?? ExtractRecordSourceIdFromUrl(journalUrl);

            if (!resolvedId.HasValue || resolvedId.Value <= 0)
            {
                return new List<string>();
            }

            var result = new List<string>();
            var page = 1;

            while (true)
            {
                cancellationToken.ThrowIfCancellationRequested();

                var url = $"https://journalrank.rcsi.science/ru/record-sources/subject-categories/{resolvedId.Value}/?pagesize=100&page={page}";
                using var response = await _httpClient.GetAsync(url, cancellationToken);
                var html = await response.Content.ReadAsStringAsync(cancellationToken);

                if (!response.IsSuccessStatusCode || string.IsNullOrWhiteSpace(html))
                {
                    break;
                }

                var doc = new HtmlDocument();
                doc.LoadHtml(html);

                var rows = doc.DocumentNode.SelectNodes("//table[contains(@class,'table')]//tbody/tr");
                if (rows == null || rows.Count == 0)
                {
                    break;
                }

                var addedOnPage = 0;

                foreach (var row in rows)
                {
                    var cells = row.SelectNodes("./td");
                    if (cells == null || cells.Count < 3)
                    {
                        continue;
                    }

                    var database = NormalizeText(cells[0].InnerText);
                    var code = NormalizeText(cells[1].InnerText);
                    var title = NormalizeText(cells[2].InnerText);

                    if (string.IsNullOrWhiteSpace(title))
                    {
                        continue;
                    }

                    var value = string.IsNullOrWhiteSpace(database)
                        ? title
                        : $"{database}: {title}";

                    if (!string.IsNullOrWhiteSpace(code))
                    {
                        value = $"{value} [{code}]";
                    }

                    result.Add(value);
                    addedOnPage++;
                }

                if (addedOnPage == 0 || rows.Count < 100)
                {
                    break;
                }

                page++;
            }

            return result
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();
        }

        private static int? ExtractRecordSourceIdFromUrl(string? journalUrl)
        {
            if (string.IsNullOrWhiteSpace(journalUrl))
            {
                return null;
            }

            var match = RecordSourceIdRegex.Match(journalUrl);
            if (!match.Success)
            {
                return null;
            }

            if (int.TryParse(match.Groups["id"].Value, out var id) && id > 0)
            {
                return id;
            }

            return null;
        }

        private static string NormalizeText(string? value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return string.Empty;
            }

            return WebUtility.HtmlDecode(value).Trim();
        }
    }
}
