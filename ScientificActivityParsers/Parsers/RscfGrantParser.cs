using HtmlAgilityPack;
using ScientificActivityParsers.Interfaces;
using ScientificActivityParsers.Models;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace ScientificActivityParsers.Parsers
{
    public class RscfGrantParser : IGrantParser
    {
        private readonly HttpClient _httpClient;

        public RscfGrantParser(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<List<GrantImportModel>> ParseAsync(CancellationToken cancellationToken = default)
        {
            var startUrl = "https://rscf.ru/contests/";
            var visited = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            var toVisit = new Queue<string>();
            var result = new List<GrantImportModel>();

            toVisit.Enqueue(startUrl);

            while (toVisit.Count > 0)
            {
                var currentUrl = toVisit.Dequeue();

                if (!visited.Add(currentUrl))
                {
                    continue;
                }

                Console.WriteLine($"START PAGE: {currentUrl}");

                var html = await _httpClient.GetStringAsync(currentUrl, cancellationToken);

                var doc = new HtmlDocument();
                doc.LoadHtml(html);

                var pageItems = ParseGrantPage(doc, currentUrl);
                Console.WriteLine($"PARSED ITEMS ON PAGE: {pageItems.Count}");

                result.AddRange(pageItems);

                var pageLinks = ExtractPaginationLinks(doc, currentUrl);
                Console.WriteLine($"FOUND PAGE LINKS: {string.Join(" | ", pageLinks)}");

                foreach (var link in pageLinks)
                {
                    if (!visited.Contains(link))
                    {
                        toVisit.Enqueue(link);
                    }
                }
            }

            return result
                .Where(x => !string.IsNullOrWhiteSpace(x.Title))
                .Where(x => !string.IsNullOrWhiteSpace(x.ContestNumber))
                .GroupBy(x => x.ContestNumber)
                .Select(x => x.First())
                .ToList();
        }

        private static List<GrantImportModel> ParseGrantPage(HtmlDocument doc, string pageUrl)
        {
            var result = new List<GrantImportModel>();

            var contestNodes = doc.DocumentNode.SelectNodes("//div[contains(@class,'contest-table-row')]");
            if (contestNodes == null)
            {
                return result;
            }

            foreach (var contestNode in contestNodes)
            {
                var numNode = contestNode.SelectSingleNode("./div[contains(@class,'contest-num')]");
                var nameNode = contestNode.SelectSingleNode("./div[contains(@class,'contest-name')]");
                var dateNodes = contestNode.SelectNodes("./div[contains(@class,'contest-date')]");
                var statusNode = contestNode.SelectSingleNode("./div[contains(@class,'contest-status')]");
                var docsNode = contestNode.SelectSingleNode("./div[contains(@class,'contest-docs')]");

                var contestNumber = CleanText(numNode?.InnerText);
                var title = CleanText(nameNode?.InnerText);

                if (string.IsNullOrWhiteSpace(contestNumber) || string.IsNullOrWhiteSpace(title))
                {
                    continue;
                }

                DateTime? applicationDeadline = null;
                DateTime? resultDate = null;

                if (dateNodes != null && dateNodes.Count > 0)
                {
                    applicationDeadline = ExtractDate(dateNodes[0].InnerText);
                }

                if (dateNodes != null && dateNodes.Count > 1)
                {
                    resultDate = ExtractDate(dateNodes[1].InnerText);
                }

                var statusText = ExtractMainStatus(statusNode);
                var noticeUrl = ExtractNoticeUrl(docsNode, pageUrl);

                result.Add(new GrantImportModel
                {
                    ContestNumber = contestNumber,
                    Title = title,
                    Organization = "Российский научный фонд",
                    Description = $"Конкурс №{contestNumber}. {title}",
                    ApplicationDeadline = applicationDeadline,
                    ResultDate = resultDate,
                    StatusText = statusText,
                    Url = string.IsNullOrWhiteSpace(noticeUrl) ? pageUrl : noticeUrl
                });
            }

            return result;
        }

        private static List<string> ExtractPaginationLinks(HtmlDocument doc, string baseUrl)
        {
            var result = new List<string>();

            // Сначала пытаемся найти именно блок пагинации
            var paginationLinks = doc.DocumentNode.SelectNodes(
                "//div[contains(@class,'pagination')]//a[@href] | " +
                "//nav[contains(@class,'pagination')]//a[@href] | " +
                "//ul[contains(@class,'pagination')]//a[@href]");

            // Если не нашли, берём только ссылки, похожие на пагинацию конкурсов
            if (paginationLinks == null)
            {
                paginationLinks = doc.DocumentNode.SelectNodes("//a[@href]");
            }

            if (paginationLinks == null)
            {
                return result;
            }

            foreach (var link in paginationLinks)
            {
                var href = link.GetAttributeValue("href", string.Empty).Trim();
                var text = CleanText(link.InnerText);

                if (string.IsNullOrWhiteSpace(href))
                {
                    continue;
                }

                if (!LooksLikeRscfContestPageLink(href, text))
                {
                    continue;
                }

                var absoluteUrl = BuildAbsoluteUrl(baseUrl, href);
                if (string.IsNullOrWhiteSpace(absoluteUrl))
                {
                    continue;
                }

                if (!absoluteUrl.Contains("rscf.ru", StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                if (!absoluteUrl.Contains("/contests", StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                if (absoluteUrl.EndsWith(".pdf", StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                result.Add(absoluteUrl);
            }

            return result
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();
        }

        private static bool LooksLikeRscfContestPageLink(string href, string text)
        {
            // Только относительные или rscf-ссылки
            if (href.StartsWith("http", StringComparison.OrdinalIgnoreCase) &&
                !href.Contains("rscf.ru", StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }

            // Исключаем явно внешние/служебные ссылки
            if (href.Contains("vk.com", StringComparison.OrdinalIgnoreCase) ||
                href.Contains("telegram", StringComparison.OrdinalIgnoreCase) ||
                href.Contains("arxiv.org", StringComparison.OrdinalIgnoreCase) ||
                href.Contains("away.php", StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }

            // Признаки пагинации
            if (href.Contains("PAGEN_", StringComparison.OrdinalIgnoreCase) ||
                href.Contains("page=", StringComparison.OrdinalIgnoreCase) ||
                href.Contains("PAGEN", StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }

            if (int.TryParse(text, out _))
            {
                return true;
            }

            if (text.Contains("вперед", StringComparison.OrdinalIgnoreCase) ||
                text.Contains("далее", StringComparison.OrdinalIgnoreCase) ||
                text.Contains("след", StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }

            return false;
        }

        private static string ExtractNoticeUrl(HtmlNode? docsNode, string baseUrl)
        {
            if (docsNode == null)
            {
                return string.Empty;
            }

            var links = docsNode.SelectNodes(".//a[@href]");
            if (links == null)
            {
                return string.Empty;
            }

            foreach (var link in links)
            {
                var text = CleanText(link.InnerText);
                var href = link.GetAttributeValue("href", string.Empty).Trim();

                if (string.IsNullOrWhiteSpace(href))
                {
                    continue;
                }

                if (text.Contains("Извещение", StringComparison.OrdinalIgnoreCase))
                {
                    return BuildAbsoluteUrl(baseUrl, href);
                }
            }

            var firstHref = links.FirstOrDefault()?.GetAttributeValue("href", string.Empty)?.Trim();
            return string.IsNullOrWhiteSpace(firstHref) ? string.Empty : BuildAbsoluteUrl(baseUrl, firstHref);
        }

        private static string BuildAbsoluteUrl(string baseUrl, string href)
        {
            if (string.IsNullOrWhiteSpace(href))
            {
                return string.Empty;
            }

            if (Uri.TryCreate(href, UriKind.Absolute, out var absoluteUri))
            {
                return absoluteUri.ToString();
            }

            return new Uri(new Uri(baseUrl), href).ToString();
        }

        private static string CleanText(string? text)
        {
            if (string.IsNullOrWhiteSpace(text))
            {
                return string.Empty;
            }

            return HtmlEntity.DeEntitize(text)
                .Replace("\r", " ")
                .Replace("\n", " ")
                .Replace("\t", " ")
                .Trim();
        }

        private static DateTime? ExtractDate(string? text)
        {
            if (string.IsNullOrWhiteSpace(text))
            {
                return null;
            }

            var cleaned = CleanText(text);
            var parts = cleaned.Split(' ', StringSplitOptions.RemoveEmptyEntries);

            foreach (var part in parts)
            {
                if (DateTime.TryParseExact(
                    part.Trim(),
                    "dd.MM.yyyy",
                    CultureInfo.InvariantCulture,
                    DateTimeStyles.None,
                    out var parsedDate))
                {
                    return DateTime.SpecifyKind(parsedDate, DateTimeKind.Utc);
                }
            }

            return null;
        }

        private static string ExtractMainStatus(HtmlNode? statusNode)
        {
            if (statusNode == null)
            {
                return "Открыт";
            }

            var finishedNode = statusNode.SelectSingleNode(".//span[contains(@class,'contest-danger')]");
            if (finishedNode != null)
            {
                var text = CleanText(finishedNode.InnerText);
                if (text.Contains("Конкурс завершен", StringComparison.OrdinalIgnoreCase) ||
                    text.Contains("Конкурс завершён", StringComparison.OrdinalIgnoreCase))
                {
                    return "Завершен";
                }
            }

            var expertNode = statusNode.SelectSingleNode(".//span[contains(@class,'text-warning')]");
            if (expertNode != null)
            {
                var text = CleanText(expertNode.InnerText);
                if (text.Contains("Экспертиза", StringComparison.OrdinalIgnoreCase))
                {
                    return "Экспертиза";
                }
            }

            var openedNode = statusNode.SelectSingleNode(".//span[contains(@class,'contest-success')]");
            if (openedNode != null)
            {
                var text = CleanText(openedNode.InnerText);
                if (text.Contains("Прием заявок", StringComparison.OrdinalIgnoreCase) ||
                    text.Contains("Приём заявок", StringComparison.OrdinalIgnoreCase))
                {
                    return "Открыт";
                }
            }

            var cleaned = CleanText(statusNode.InnerText);

            if (cleaned.Contains("Конкурс завершен", StringComparison.OrdinalIgnoreCase) ||
                cleaned.Contains("Конкурс завершён", StringComparison.OrdinalIgnoreCase))
            {
                return "Завершен";
            }

            if (cleaned.Contains("Экспертиза", StringComparison.OrdinalIgnoreCase))
            {
                return "Экспертиза";
            }

            if (cleaned.Contains("Прием заявок", StringComparison.OrdinalIgnoreCase) ||
                cleaned.Contains("Приём заявок", StringComparison.OrdinalIgnoreCase))
            {
                return "Открыт";
            }

            return "Открыт";
        }
    }
}
