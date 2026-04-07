using HtmlAgilityPack;
using Microsoft.Extensions.Logging;
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
    public class NaKonferenciiConferenceParser : IConferenceParser
    {
        private static readonly CultureInfo RussianCulture = new("ru-RU");

        private readonly HttpClient _httpClient;
        private readonly ILogger<NaKonferenciiConferenceParser> _logger;

        public NaKonferenciiConferenceParser(
            HttpClient httpClient,
            ILogger<NaKonferenciiConferenceParser> logger)
        {
            _httpClient = httpClient;
            _logger = logger;
        }

        public async Task<List<ConferenceImportModel>> ParseAsync(CancellationToken cancellationToken = default)
        {
            var result = new List<ConferenceImportModel>();

            var categoryUrls = GetStartCategoryUrls();

            var visitedCategoryPages = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            var categoryQueue = new Queue<string>();

            foreach (var url in categoryUrls
                         .Where(x => !string.IsNullOrWhiteSpace(x))
                         .Select(NormalizeUrl)
                         .Distinct(StringComparer.OrdinalIgnoreCase))
            {
                categoryQueue.Enqueue(url);
            }

            var conferenceUrls = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            while (categoryQueue.Count > 0)
            {
                cancellationToken.ThrowIfCancellationRequested();

                var currentCategoryUrl = categoryQueue.Dequeue();

                if (!visitedCategoryPages.Add(currentCategoryUrl))
                {
                    continue;
                }

                try
                {
                    _logger.LogInformation("Conference parser: open category page {Url}", currentCategoryUrl);

                    var html = await GetPageHtmlAsync(currentCategoryUrl, cancellationToken);

                    var doc = new HtmlDocument();
                    doc.LoadHtml(html);

                    var foundConferenceUrls = ExtractConferenceLinksFromCategoryPage(doc, currentCategoryUrl);
                    _logger.LogInformation(
                        "Conference parser: found {Count} conference links on category page {Url}",
                        foundConferenceUrls.Count,
                        currentCategoryUrl);

                    foreach (var conferenceUrl in foundConferenceUrls)
                    {
                        conferenceUrls.Add(conferenceUrl);
                    }

                    var paginationLinks = ExtractCategoryPaginationLinks(doc, currentCategoryUrl);
                    _logger.LogInformation(
                        "Conference parser: found {Count} pagination links on category page {Url}",
                        paginationLinks.Count,
                        currentCategoryUrl);

                    foreach (var pageLink in paginationLinks)
                    {
                        if (!visitedCategoryPages.Contains(pageLink))
                        {
                            categoryQueue.Enqueue(pageLink);
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Conference parser: failed to process category page {Url}", currentCategoryUrl);
                }
            }

            _logger.LogInformation(
                "Conference parser: total unique conference URLs collected {Count}",
                conferenceUrls.Count);

            foreach (var conferenceUrl in conferenceUrls)
            {
                cancellationToken.ThrowIfCancellationRequested();

                try
                {
                    _logger.LogInformation("Conference parser: open conference page {Url}", conferenceUrl);

                    var html = await GetPageHtmlAsync(conferenceUrl, cancellationToken);

                    var conference = ParseConferenceDetailsPage(html, conferenceUrl);
                    if (conference != null &&
                        !string.IsNullOrWhiteSpace(conference.Title) &&
                        !string.IsNullOrWhiteSpace(conference.Url))
                    {
                        result.Add(conference);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Conference parser: failed to parse conference page {Url}", conferenceUrl);
                }
            }

            var finalResult = result
                .Where(x => !string.IsNullOrWhiteSpace(x.Title))
                .Where(x => !string.IsNullOrWhiteSpace(x.Url))
                .GroupBy(x => NormalizeUrl(x.Url), StringComparer.OrdinalIgnoreCase)
                .Select(g => g.First())
                .GroupBy(x => $"{x.Title.Trim().ToLowerInvariant()}|{x.StartDate:yyyy-MM-dd}")
                .Select(g => g.First())
                .ToList();

            _logger.LogInformation(
                "Conference parser: total parsed {Count} unique conferences",
                finalResult.Count);

            return finalResult;
        }

        private static List<string> GetStartCategoryUrls()
        {
            return new List<string>
            {
                "https://na-konferencii.ru/conference-cat/tehnicheskie-nauki/informacionnye-tehnologii",
                "https://na-konferencii.ru/conference-cat/tehnicheskie-nauki/tehnologii",
                "https://na-konferencii.ru/conference-cat/tehnicheskie-nauki/jenergetika",
                "https://na-konferencii.ru/conference-cat/tehnicheskie-nauki/modelirovanie",
                "https://na-konferencii.ru/conference-cat/estestvennye-nauki/fizika",
                "https://na-konferencii.ru/conference-cat/estestvennye-nauki/himija",
                "https://na-konferencii.ru/conference-cat/estestvennye-nauki/geografija",
                "https://na-konferencii.ru/conference-cat/estestvennye-nauki/geologija",
                "https://na-konferencii.ru/conference-cat/estestvennye-nauki/nauki-o-zemle",
                "https://na-konferencii.ru/conference-cat/estestvennye-nauki/jekologija-prirodopolzovanie",
                "https://na-konferencii.ru/conference-cat/obshhestvennyie-gumanitarnyie-nauki/jekonomika-upravlenie-finansy",
                "https://na-konferencii.ru/conference-cat/obshhestvennyie-gumanitarnyie-nauki/gosudarstvennoe-upravlenie",
                "https://na-konferencii.ru/conference-cat/obshhestvennyie-gumanitarnyie-nauki/juridicheskie-nauki",
                "https://na-konferencii.ru/conference-cat/obshhestvennyie-gumanitarnyie-nauki/filologija",
                "https://na-konferencii.ru/conference-cat/obshhestvennyie-gumanitarnyie-nauki/sociologija",
                "https://na-konferencii.ru/conference-cat/obshhestvennyie-gumanitarnyie-nauki/filosofija",
                "https://na-konferencii.ru/conference-cat/obshhestvennyie-gumanitarnyie-nauki/zhurnalistika",
                "https://na-konferencii.ru/conference-cat/obshhestvennyie-gumanitarnyie-nauki/inostrannye-jazyki",
                "https://na-konferencii.ru/conference-cat/obshhestvennyie-gumanitarnyie-nauki/obrazovanie-attestacija",
                "https://na-konferencii.ru/conference-cat/raznoe/shirokaja-tematika"
            };
        }

        private async Task<string> GetPageHtmlAsync(string url, CancellationToken cancellationToken)
        {
            using var request = new HttpRequestMessage(HttpMethod.Get, url);
            request.Headers.UserAgent.ParseAdd("Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/131.0 Safari/537.36");
            request.Headers.AcceptLanguage.ParseAdd("ru-RU,ru;q=0.9,en;q=0.8");

            using var response = await _httpClient.SendAsync(request, cancellationToken);
            response.EnsureSuccessStatusCode();

            return await response.Content.ReadAsStringAsync(cancellationToken);
        }

        private List<string> ExtractConferenceLinksFromCategoryPage(HtmlDocument doc, string baseUrl)
        {
            var result = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            var links = doc.DocumentNode.SelectNodes("//a[@href]");
            if (links == null)
            {
                return result.ToList();
            }

            foreach (var link in links)
            {
                var href = CleanText(link.GetAttributeValue("href", string.Empty));
                if (string.IsNullOrWhiteSpace(href))
                {
                    continue;
                }

                var absoluteUrl = BuildAbsoluteUrl(baseUrl, href);
                if (string.IsNullOrWhiteSpace(absoluteUrl))
                {
                    continue;
                }

                absoluteUrl = NormalizeUrl(absoluteUrl);

                if (!absoluteUrl.Contains("na-konferencii.ru", StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                if (!absoluteUrl.Contains("/conference/", StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                if (absoluteUrl.Contains("/conference-cat/", StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                if (absoluteUrl.Contains("/page/", StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                result.Add(absoluteUrl);
            }

            return result.ToList();
        }

        private List<string> ExtractCategoryPaginationLinks(HtmlDocument doc, string baseUrl)
        {
            var result = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            var currentCategoryRoot = GetCategoryRoot(baseUrl);

            var links = doc.DocumentNode.SelectNodes("//a[@href]");
            if (links == null)
            {
                return result.ToList();
            }

            foreach (var link in links)
            {
                var href = CleanText(link.GetAttributeValue("href", string.Empty));
                if (string.IsNullOrWhiteSpace(href))
                {
                    continue;
                }

                var absoluteUrl = BuildAbsoluteUrl(baseUrl, href);
                if (string.IsNullOrWhiteSpace(absoluteUrl))
                {
                    continue;
                }

                absoluteUrl = NormalizeUrl(absoluteUrl);

                if (!absoluteUrl.Contains("na-konferencii.ru", StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                if (!absoluteUrl.Contains("/page/", StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                if (!absoluteUrl.StartsWith(currentCategoryRoot, StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                result.Add(absoluteUrl);
            }

            return result.ToList();
        }

        private ConferenceImportModel? ParseConferenceDetailsPage(string html, string pageUrl)
        {
            var doc = new HtmlDocument();
            doc.LoadHtml(html);

            var title = ExtractTitle(doc);
            if (string.IsNullOrWhiteSpace(title))
            {
                _logger.LogWarning("Conference parser: title not found on {Url}", pageUrl);
                return null;
            }

            var fullText = NormalizeText(HtmlEntity.DeEntitize(doc.DocumentNode.InnerText));
            var factsBlock = ExtractFactsBlock(fullText);

            ParseLocation(factsBlock, out var city, out var country);

            var organizer = ExtractOrganizer(doc, factsBlock, fullText);
            var subjectArea = ExtractSubjectArea(doc, fullText);
            var status = ExtractStatus(doc, fullText);

            ParseDates(
                factsBlock,
                fullText,
                out var startDate,
                out var endDate,
                out var submissionStart,
                out var submissionEnd);

            var description = BuildDescription(status, submissionStart, submissionEnd);

            return new ConferenceImportModel
            {
                Title = title,
                Description = description,
                StartDate = startDate,
                EndDate = endDate,
                City = city,
                Country = country,
                Organizer = organizer,
                SubjectArea = subjectArea,
                Url = NormalizeUrl(pageUrl)
            };
        }

        private static string ExtractTitle(HtmlDocument doc)
        {
            var h1 =
                doc.DocumentNode.SelectSingleNode("//h1") ??
                doc.DocumentNode.SelectSingleNode("//title");

            var title = CleanText(h1?.InnerText);

            title = Regex.Replace(title, @"\s*\|\s*Научные\-конференции\.РФ\s*$", string.Empty, RegexOptions.IgnoreCase).Trim();

            return title;
        }

        private static string ExtractFactsBlock(string fullText)
        {
            if (string.IsNullOrWhiteSpace(fullText))
            {
                return string.Empty;
            }

            var normalized = Regex.Replace(fullText, @"\s+", " ").Trim();

            var startIndex = -1;

            var markers = new[]
            {
                "Дата проведения:",
                "Прием заявок с",
                "Приём заявок с",
                "Организаторы:",
                "Форма проведения:"
            };

            foreach (var marker in markers)
            {
                var markerIndex = normalized.IndexOf(marker, StringComparison.OrdinalIgnoreCase);
                if (markerIndex >= 0)
                {
                    startIndex = markerIndex;
                    break;
                }
            }

            if (startIndex <= 0)
            {
                return normalized;
            }

            var prefix = normalized[..startIndex].Trim();
            var tail = normalized[startIndex..].Trim();

            var locationPart = ExtractLocationFromPrefix(prefix);

            return $"{locationPart} {tail}".Trim();
        }

        private static string ExtractLocationFromPrefix(string prefix)
        {
            if (string.IsNullOrWhiteSpace(prefix))
            {
                return string.Empty;
            }

            var candidates = new[]
            {
                @"(?<city>[А-ЯA-ZЁ][А-ЯA-ZЁа-яa-z\-\s\.]+)\s*,\s*(?<country>[А-ЯA-ZЁ][А-ЯA-ZЁа-яa-z\-\s\.]+)$",
                @"(?<city>[А-ЯA-ZЁ][А-ЯA-ZЁа-яa-z\-\s\.]+)\s+(?<country>Россия|Узбекистан|Казахстан|Беларусь|Республика Беларусь|Таджикистан|Кыргызстан|Армения|Азербайджан|Грузия|Новосибирская область|Ханты-Мансийский автономный округ\s*–\s*Югра)$"
            };

            foreach (var pattern in candidates)
            {
                var match = Regex.Match(prefix, pattern, RegexOptions.IgnoreCase);
                if (match.Success)
                {
                    return $"{match.Groups["city"].Value.Trim()}, {match.Groups["country"].Value.Trim()}";
                }
            }

            return prefix;
        }

        private static string? ExtractOrganizer(HtmlDocument doc, string factsBlock, string fullText)
        {
            var source = !string.IsNullOrWhiteSpace(factsBlock)
                ? factsBlock
                : fullText;

            source = NormalizeText(source);

            // 1. Сначала пытаемся взять текст после "Организаторы:"
            var match = Regex.Match(
                source,
                @"Организаторы?\s*:\s*(?<value>.+)",
                RegexOptions.IgnoreCase | RegexOptions.Singleline);

            if (!match.Success)
            {
                // fallback по параграфам
                var paragraphs = doc.DocumentNode.SelectNodes("//p|//div");
                if (paragraphs != null)
                {
                    foreach (var node in paragraphs)
                    {
                        var text = CleanInlineText(node.InnerText);
                        if (string.IsNullOrWhiteSpace(text))
                        {
                            continue;
                        }

                        if (text.StartsWith("Организаторы:", StringComparison.OrdinalIgnoreCase) ||
                            text.StartsWith("Организатор:", StringComparison.OrdinalIgnoreCase))
                        {
                            var value = text[(text.IndexOf(':') + 1)..].Trim();
                            value = TrimOrganizerTail(value);
                            return string.IsNullOrWhiteSpace(value) ? null : value;
                        }
                    }
                }

                return null;
            }

            var organizer = match.Groups["value"].Value.Trim();
            organizer = TrimOrganizerTail(organizer);

            return string.IsNullOrWhiteSpace(organizer) ? null : organizer;
        }

        private static string? ExtractSubjectArea(HtmlDocument doc, string fullText)
        {
            var categoryLinks = doc.DocumentNode.SelectNodes("//a[contains(@href,'conference-cat')]");
            if (categoryLinks != null && categoryLinks.Count > 0)
            {
                var ignored = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
                {
                    "DOI",
                    "eLibrary.ru",
                    "Scopus",
                    "Springer",
                    "Web of Science"
                };

                var values = categoryLinks
                    .Select(x => CleanText(x.InnerText))
                    .Where(x => !string.IsNullOrWhiteSpace(x))
                    .Where(x => !ignored.Contains(x))
                    .Distinct(StringComparer.OrdinalIgnoreCase)
                    .ToList();

                if (values.Count > 0)
                {
                    return string.Join(", ", values);
                }
            }

            var match = Regex.Match(
                fullText,
                @"Организаторы?\s*:.*?(?:DOI|eLibrary\.ru|Scopus|Springer|Web of Science)\s*,\s*(?<topics>.+?)(?=Международная|Всероссийская|64\-я|XI|VI|Контактная информация:|$)",
                RegexOptions.IgnoreCase | RegexOptions.Singleline);

            return match.Success ? CleanInlineText(match.Groups["topics"].Value) : null;
        }

        private static string? ExtractStatus(HtmlDocument doc, string fullText)
        {
            var statusNode = doc.DocumentNode.SelectSingleNode("//*[contains(@class,'opens') or contains(@class,'closed')]");
            var status = CleanInlineText(statusNode?.InnerText);

            if (!string.IsNullOrWhiteSpace(status))
            {
                return NormalizeStatus(status);
            }

            if (fullText.Contains("Идет прием заявок", StringComparison.OrdinalIgnoreCase) ||
                fullText.Contains("Идёт приём заявок", StringComparison.OrdinalIgnoreCase))
            {
                return "Идет прием заявок";
            }

            if (fullText.Contains("Прием заявок завершен", StringComparison.OrdinalIgnoreCase) ||
                fullText.Contains("Приём заявок завершен", StringComparison.OrdinalIgnoreCase) ||
                fullText.Contains("Приём заявок завершён", StringComparison.OrdinalIgnoreCase))
            {
                return "Прием заявок завершен";
            }

            return null;
        }

        private static void ParseDates(
            string factsBlock,
            string fullText,
            out DateTime? startDate,
            out DateTime? endDate,
            out DateTime? submissionStart,
            out DateTime? submissionEnd)
        {
            startDate = null;
            endDate = null;
            submissionStart = null;
            submissionEnd = null;

            var source = !string.IsNullOrWhiteSpace(factsBlock) ? factsBlock : fullText;
            var normalized = Regex.Replace(source, @"\s+", " ").Trim();

            var conferenceMatch = Regex.Match(
                normalized,
                @"Дата проведения:\s*(?<startDay>\d{1,2})\s+(?<startMonth>[А-Яа-яЁё]+)\s*-\s*(?<endDay>\d{1,2})\s+(?<endMonth>[А-Яа-яЁё]+)\s+(?<year>\d{4})\s*г",
                RegexOptions.IgnoreCase);

            if (conferenceMatch.Success)
            {
                var startRaw =
                    $"{conferenceMatch.Groups["startDay"].Value} {conferenceMatch.Groups["startMonth"].Value} {conferenceMatch.Groups["year"].Value}";
                var endRaw =
                    $"{conferenceMatch.Groups["endDay"].Value} {conferenceMatch.Groups["endMonth"].Value} {conferenceMatch.Groups["year"].Value}";

                startDate = ParseRussianDate(startRaw);
                endDate = ParseRussianDate(endRaw);
            }

            var submissionMatch = Regex.Match(
                normalized,
                @"При[её]м заявок\s+с\s+(?<startDay>\d{1,2})\s+(?<startMonth>[А-Яа-яЁё]+)\s+по\s+(?<endDay>\d{1,2})\s+(?<endMonth>[А-Яа-яЁё]+)\s+(?<year>\d{4})\s*г",
                RegexOptions.IgnoreCase);

            if (submissionMatch.Success)
            {
                var endYear = int.Parse(submissionMatch.Groups["year"].Value);

                var startMonthNumber = ParseRussianMonth(submissionMatch.Groups["startMonth"].Value);
                var endMonthNumber = ParseRussianMonth(submissionMatch.Groups["endMonth"].Value);

                var startYear = endYear;

                if (startMonthNumber > 0 && endMonthNumber > 0 && startMonthNumber > endMonthNumber)
                {
                    startYear = endYear - 1;
                }

                var submissionStartRaw =
                    $"{submissionMatch.Groups["startDay"].Value} {submissionMatch.Groups["startMonth"].Value} {startYear}";
                var submissionEndRaw =
                    $"{submissionMatch.Groups["endDay"].Value} {submissionMatch.Groups["endMonth"].Value} {endYear}";

                submissionStart = ParseRussianDate(submissionStartRaw);
                submissionEnd = ParseRussianDate(submissionEndRaw);
            }

            if (startDate.HasValue && !endDate.HasValue)
            {
                endDate = startDate;
            }

            if (!submissionStart.HasValue || !submissionEnd.HasValue)
            {
                var compactSubmissionMatch = Regex.Match(
                    normalized,
                    @"При[её]м заявок\s*:\s*(?<from>\d{2}\.\d{2}\.\d{4})\s*-\s*(?<to>\d{2}\.\d{2}\.\d{4})",
                    RegexOptions.IgnoreCase);

                if (compactSubmissionMatch.Success)
                {
                    submissionStart ??= ParseDotDate(compactSubmissionMatch.Groups["from"].Value);
                    submissionEnd ??= ParseDotDate(compactSubmissionMatch.Groups["to"].Value);
                }
            }
        }

        private static string BuildDescription(string? status, DateTime? submissionStart, DateTime? submissionEnd)
        {
            var parts = new List<string>();

            if (!string.IsNullOrWhiteSpace(status))
            {
                parts.Add($"Статус: {NormalizeStatus(status)}");
            }

            if (submissionStart.HasValue || submissionEnd.HasValue)
            {
                parts.Add(
                    $"Приём заявок: {(submissionStart.HasValue ? submissionStart.Value.ToString("dd.MM.yyyy") : "-")} - {(submissionEnd.HasValue ? submissionEnd.Value.ToString("dd.MM.yyyy") : "-")}");
            }

            return string.Join(". ", parts);
        }

        private static void ParseLocation(string sourceText, out string? city, out string? country)
        {
            city = null;
            country = null;

            if (string.IsNullOrWhiteSpace(sourceText))
            {
                return;
            }

            var normalized = Regex.Replace(sourceText, @"\s+", " ").Trim();

            var explicitMatch = Regex.Match(
                normalized,
                @"^(?<city>[А-ЯA-ZЁ][А-ЯA-ZЁа-яa-z\-\s\.]+?)\s*,\s*(?<country>[А-ЯA-ZЁ][А-ЯA-ZЁа-яa-z\-\s\.]+?)(?=\s+Дата проведения:|\s+При[её]м заявок|\s+Форма проведения:|\s+Организаторы:|$)",
                RegexOptions.IgnoreCase);

            if (explicitMatch.Success)
            {
                city = CleanInlineText(explicitMatch.Groups["city"].Value);
                country = CleanInlineText(explicitMatch.Groups["country"].Value);
                return;
            }

            var countryOnlyMatch = Regex.Match(
                normalized,
                @"^(?<country>Россия|Узбекистан|Казахстан|Беларусь|Республика Беларусь|Таджикистан|Кыргызстан|Армения|Азербайджан|Грузия|Новосибирская область|Ханты-Мансийский автономный округ\s*–\s*Югра)(?=\s+Дата проведения:|\s+При[её]м заявок|\s+Форма проведения:|\s+Организаторы:|$)",
                RegexOptions.IgnoreCase);

            if (countryOnlyMatch.Success)
            {
                country = CleanInlineText(countryOnlyMatch.Groups["country"].Value);
            }
        }

        private static int ParseRussianMonth(string monthName)
        {
            if (string.IsNullOrWhiteSpace(monthName))
            {
                return 0;
            }

            var monthMap = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase)
            {
                ["января"] = 1,
                ["февраля"] = 2,
                ["марта"] = 3,
                ["апреля"] = 4,
                ["мая"] = 5,
                ["июня"] = 6,
                ["июля"] = 7,
                ["августа"] = 8,
                ["сентября"] = 9,
                ["октября"] = 10,
                ["ноября"] = 11,
                ["декабря"] = 12
            };

            return monthMap.TryGetValue(monthName.Trim(), out var month) ? month : 0;
        }

        private static DateTime? ParseRussianDate(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
            {
                return null;
            }

            var normalized = Regex.Replace(text.Trim(), @"\s+", " ");

            if (DateTime.TryParseExact(
                normalized,
                new[] { "d MMMM yyyy", "dd MMMM yyyy" },
                RussianCulture,
                DateTimeStyles.None,
                out var date))
            {
                return DateTime.SpecifyKind(date.Date, DateTimeKind.Utc);
            }

            return null;
        }

        private static DateTime? ParseDotDate(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
            {
                return null;
            }

            if (DateTime.TryParseExact(
                text.Trim(),
                "dd.MM.yyyy",
                CultureInfo.InvariantCulture,
                DateTimeStyles.None,
                out var date))
            {
                return DateTime.SpecifyKind(date.Date, DateTimeKind.Utc);
            }

            return null;
        }

        private static string NormalizeStatus(string status)
        {
            if (string.IsNullOrWhiteSpace(status))
            {
                return string.Empty;
            }

            var normalized = CleanInlineText(status);

            normalized = Regex.Replace(normalized, @"^Статус:\s*", string.Empty, RegexOptions.IgnoreCase).Trim();

            if (normalized.Equals("Идёт приём заявок", StringComparison.OrdinalIgnoreCase))
            {
                return "Идет прием заявок";
            }

            if (normalized.Equals("Идет прием заявок", StringComparison.OrdinalIgnoreCase))
            {
                return "Идет прием заявок";
            }

            if (normalized.Equals("Приём заявок завершён", StringComparison.OrdinalIgnoreCase) ||
                normalized.Equals("Приём заявок завершен", StringComparison.OrdinalIgnoreCase) ||
                normalized.Equals("Прием заявок завершен", StringComparison.OrdinalIgnoreCase))
            {
                return "Прием заявок завершен";
            }

            return normalized;
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

            if (Uri.TryCreate(new Uri(baseUrl), href, out var combinedUri))
            {
                return combinedUri.ToString();
            }

            return string.Empty;
        }

        private static string NormalizeUrl(string url)
        {
            if (string.IsNullOrWhiteSpace(url))
            {
                return string.Empty;
            }

            if (!Uri.TryCreate(url, UriKind.Absolute, out var uri))
            {
                return url.Trim().TrimEnd('/');
            }

            var builder = new UriBuilder(uri)
            {
                Query = string.Empty,
                Fragment = string.Empty
            };

            var normalized = builder.Uri.ToString().TrimEnd('/');

            return normalized;
        }

        private static string GetCategoryRoot(string url)
        {
            var normalized = NormalizeUrl(url);

            var pageIndex = normalized.IndexOf("/page/", StringComparison.OrdinalIgnoreCase);
            if (pageIndex >= 0)
            {
                return normalized[..pageIndex];
            }

            return normalized;
        }

        private static string NormalizeText(string? text)
        {
            if (string.IsNullOrWhiteSpace(text))
            {
                return string.Empty;
            }

            return HtmlEntity.DeEntitize(text)
                .Replace("\r", " ")
                .Replace("\n", " ")
                .Replace("\t", " ")
                .Replace('\u00A0', ' ')
                .Trim();
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
                .Replace('\u00A0', ' ')
                .Trim();
        }

        private static string CleanInlineText(string? text)
        {
            if (string.IsNullOrWhiteSpace(text))
            {
                return string.Empty;
            }

            return HtmlEntity.DeEntitize(text)
                .Replace("\r", " ")
                .Replace("\n", " ")
                .Replace("\t", " ")
                .Replace('\u00A0', ' ')
                .Trim();
        }

        private static string TrimOrganizerTail(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
            {
                return string.Empty;
            }

            var value = CleanInlineText(text);

            // Убираем HTML/пробельный мусор
            value = Regex.Replace(value, @"\s{2,}", " ").Trim();

            // Отсекаем все, что явно не относится к организатору
            var stopPatterns = new[]
            {
        @"\bПеречень\s+ВАК\b",
        @"\bВАК\b",
        @"\bРИНЦ\b",
        @"\bScopus\b",
        @"\bWeb\s+of\s+Science\b",
        @"\beLibrary\b",
        @"\bDOI\b",
        @"\bИнформационное\s+сообщение\b",
        @"\bУважаемые\s+коллеги\b",
        @"\bЦелью\s+конференции\s+является\b",
        @"\bКонференция\s+ориентирована\b",
        @"\bКонференция\s+в\s+\d{4}\s+году\s+посвящена\b",
        @"\bНаучный\s+альманах\b",
        @"\bВ\s+программу\s+будут\s+включены\b",
        @"\bОсновные\s+тематики\s+докладов\b",
        @"\bРабочий\s+язык\s+конференции\b",
        @"\bОнлайн-трансляция\s+не\s+предусмотрена\b",
        @"\bВ\s+рамках\s+конференции\s+будут\s+обсуждаться\b",
        @"\bКонтактная\s+информация\b",
        @"\bФорма\s+проведения\b",
        @"\bДата\s+проведения\b",
        @"\bПри[её]м\s+заявок\b"
    };

            var cutIndex = -1;

            foreach (var pattern in stopPatterns)
            {
                var match = Regex.Match(value, pattern, RegexOptions.IgnoreCase);
                if (match.Success)
                {
                    if (cutIndex == -1 || match.Index < cutIndex)
                    {
                        cutIndex = match.Index;
                    }
                }
            }

            if (cutIndex >= 0)
            {
                value = value[..cutIndex].Trim();
            }

            // Если после организатора внезапно пошли тематики через запятую,
            // пробуем остановиться до первого тематического хвоста
            var subjectAreaStarters = new[]
            {
        "Информационные технологии",
        "Молодые учёные",
        "Молодые ученые",
        "Проблемы науки",
        "Инновации",
        "Экология",
        "Природопользование",
        "Естественные науки",
        "Технические науки",
        "Химия",
        "Биология",
        "Физика",
        "Математика",
        "Педагогика",
        "Филология",
        "Иностранные языки",
        "Экономика",
        "Юридические науки",
        "Социология",
        "Философия",
        "Журналистика",
        "Образование",
        "Науки о Земле",
        "Широкая тематика"
    };

            foreach (var starter in subjectAreaStarters)
            {
                var index = value.IndexOf(starter, StringComparison.OrdinalIgnoreCase);
                if (index > 0)
                {
                    value = value[..index].Trim().Trim(',', ';');
                }
            }

            // Чистим хвостовую пунктуацию
            value = value.Trim().Trim(',', ';', '.', '-').Trim();

            return value;
        }
    }
}
