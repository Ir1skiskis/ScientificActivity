using HtmlAgilityPack;
using Microsoft.Extensions.Logging;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;
using ScientificActivityParsers.Interfaces;
using ScientificActivityParsers.Models;
using SeleniumUndetectedChromeDriver;
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

        public async Task<List<ConferenceImportModel>> ParseAsync(bool includePast = true, bool includeAnnouncements = true, CancellationToken cancellationToken = default)
        {
            var allConferences = new List<ConferenceImportModel>();

            if (includePast)
            {
                var past = await ParsePastEventsAsync(cancellationToken);
                allConferences.AddRange(past);
            }

            if (includeAnnouncements)
            {
                var announcements = await ParseAnnouncementsAsync(cancellationToken);
                allConferences.AddRange(announcements);
            }

            var finalResult = allConferences
                .GroupBy(x => NormalizeUrl(x.Url))
                .Select(g => g.First())
                .GroupBy(x => $"{x.Title.Trim().ToLowerInvariant()}|{x.StartDate:yyyy-MM-dd}")
                .Select(g => g.First())
                .ToList();

            return finalResult;
        }

        private ConferenceImportModel? ParseConferenceDetailsPage(string html, string pageUrl, Dictionary<string, (string? city, string? country)>? locations)
        {
            var doc = new HtmlDocument();
            doc.LoadHtml(html);
            var title = ExtractTitle(doc);
            if (string.IsNullOrWhiteSpace(title))
                return null;

            string? city = null, country = null;
            if (locations != null && locations.TryGetValue(pageUrl, out var loc))
            {
                city = loc.city;
                country = loc.country;
            }
            if (string.IsNullOrWhiteSpace(city) && string.IsNullOrWhiteSpace(country))
            {
                var locationNode = doc.DocumentNode.SelectSingleNode("//div[contains(@class,'notice-item-top-location')]//p");
                if (locationNode != null)
                {
                    string locText = CleanText(locationNode.InnerText);
                    (city, country) = ParseCityCountryFromLocationString(locText);
                }
            }

            var fullText = NormalizeText(HtmlEntity.DeEntitize(doc.DocumentNode.InnerText));
            var factsBlock = ExtractFactsBlock(fullText);
            var organizer = ExtractOrganizer(doc, factsBlock, fullText);
            var subjectArea = ExtractSubjectArea(doc, fullText);
            var status = ExtractStatus(doc, fullText);
            ParseDates(factsBlock, fullText, out var startDate, out var endDate, out var submissionStart, out var submissionEnd);
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

            var match = Regex.Match(source, @"Организаторы?\s*:\s*(?<value>.+?)(?=(?:\n|\.\s+)(?:Конференция приурочена|Для нас юбилей|Рабочие языки|Оргкомитет|Заявка должна включать|Крайний срок|$))",
            RegexOptions.IgnoreCase | RegexOptions.Singleline);

            if (!match.Success)
            {
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

            value = Regex.Replace(value, @"\s{2,}", " ").Trim();

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

            value = value.Trim().Trim(',', ';', '.', '-').Trim();

            return value;
        }

        private static readonly Dictionary<char, string> TranslitMap = new()
        {
            ['а'] = "a",
            ['б'] = "b",
            ['в'] = "v",
            ['г'] = "g",
            ['д'] = "d",
            ['е'] = "e",
            ['ё'] = "e",
            ['ж'] = "zh",
            ['з'] = "z",
            ['и'] = "i",
            ['й'] = "y",
            ['к'] = "k",
            ['л'] = "l",
            ['м'] = "m",
            ['н'] = "n",
            ['о'] = "o",
            ['п'] = "p",
            ['р'] = "r",
            ['с'] = "s",
            ['т'] = "t",
            ['у'] = "u",
            ['ф'] = "f",
            ['х'] = "kh",
            ['ц'] = "ts",
            ['ч'] = "ch",
            ['ш'] = "sh",
            ['щ'] = "shh",
            ['ъ'] = "",
            ['ы'] = "y",
            ['ь'] = "",
            ['э'] = "e",
            ['ю'] = "yu",
            ['я'] = "ya",
            [' '] = "-",
            [','] = "-",
            ['–'] = "-",
            ['·'] = "-",
            ['0'] = "-",
            ['('] = "",
            [')'] = "",
            ['/'] = "-",
            ['\\'] = "-",
            ['&'] = "",
            ['+'] = ""
        };

        private static (string? city, string? country) ParseCityCountryFromLocationString(string location)
        {
            if (string.IsNullOrWhiteSpace(location))
                return (null, null);

            var parts = location.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                                .Select(p => p.Trim())
                                .Where(p => !string.IsNullOrEmpty(p))
                                .ToList();

            if (parts.Count == 0)
                return (null, null);

            string city = parts[0];
            string? country = null;

            if (parts.Count > 1)
            {
                var last = parts.Last();
                var knownCountries = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
                {
                    "Россия", "РФ", "Российская Федерация",
                    "Узбекистан", "Казахстан", "Беларусь", "Республика Беларусь",
                    "Таджикистан", "Кыргызстан", "Армения", "Азербайджан",
                    "Грузия", "Австралия", "Венгрия", "Германия", "Франция",
                    "Италия", "США", "Китай", "Индия", "Турция", "ОАЭ", "Египет",
                    "Украина", "Молдова", "Литва", "Латвия", "Эстония", "Польша"
                };

                if (knownCountries.Contains(last))
                {
                    country = last;
                }
                else if (last.Equals("РФ", StringComparison.OrdinalIgnoreCase))
                {
                    country = "Россия";
                }
            }

            return (city, country);
        }

        private async Task<string> GetFilterHomeNonceAsync(
    HttpClient client,
    string pageUrl,
    bool isPastEvents,
    CancellationToken cancellationToken)
        {
            _logger.LogInformation("Получение nonce со страницы: {Url}", pageUrl);

            using var request = new HttpRequestMessage(HttpMethod.Get, pageUrl);

            request.Headers.UserAgent.ParseAdd(
                "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/148.0.0.0 Safari/537.36");

            request.Headers.Accept.ParseAdd("text/html,application/xhtml+xml,application/xml;q=0.9,*/*;q=0.8");
            request.Headers.AcceptLanguage.ParseAdd("ru-RU,ru;q=0.9,en-US;q=0.8,en;q=0.7");

            var response = await client.SendAsync(request, cancellationToken);
            response.EnsureSuccessStatusCode();

            var html = await response.Content.ReadAsStringAsync(cancellationToken);

            if (string.IsNullOrWhiteSpace(html))
            {
                throw new Exception($"Не удалось получить HTML страницы для nonce: {pageUrl}");
            }

            var candidates = ExtractNonceCandidates(html);

            if (candidates.Count == 0)
            {
                var debugPath = Path.Combine(
                    Path.GetTempPath(),
                    $"na-konferencii_nonce_debug_{DateTime.Now:yyyyMMddHHmmss}.html");

                await File.WriteAllTextAsync(debugPath, html, cancellationToken);

                throw new Exception($"Не удалось найти nonce на странице {pageUrl}. HTML сохранен: {debugPath}");
            }

            _logger.LogInformation(
                "Найдены nonce-кандидаты: {Nonces}",
                string.Join(", ", candidates));

            foreach (var candidate in candidates)
            {
                var isValid = await IsFilterHomeNonceValidAsync(
                    client,
                    candidate,
                    pageUrl,
                    isPastEvents,
                    cancellationToken);

                if (isValid)
                {
                    _logger.LogInformation("Подходящий nonce для filterhome найден: {Nonce}", candidate);
                    return candidate;
                }

                _logger.LogInformation("Nonce не подошел для filterhome: {Nonce}", candidate);
            }

            var debugAllPath = Path.Combine(
                Path.GetTempPath(),
                $"na-konferencii_nonce_all_failed_{DateTime.Now:yyyyMMddHHmmss}.html");

            await File.WriteAllTextAsync(debugAllPath, html, cancellationToken);

            throw new Exception(
                $"На странице найдены nonce, но ни один не подошел для filterhome. HTML сохранен: {debugAllPath}. " +
                $"Кандидаты: {string.Join(", ", candidates)}");
        }

        private static List<string> ExtractNonceCandidates(string html)
        {
            var result = new List<string>();

            if (string.IsNullOrWhiteSpace(html))
            {
                return result;
            }

            var patterns = new[]
            {
        @"[""']nonce[""']\s*[:=]\s*[""'](?<nonce>[a-f0-9]{10})[""']",
        @"nonce\s*[:=]\s*[""'](?<nonce>[a-f0-9]{10})[""']",
        @"[""']ajax_nonce[""']\s*[:=]\s*[""'](?<nonce>[a-f0-9]{10})[""']",
        @"ajax_nonce\s*[:=]\s*[""'](?<nonce>[a-f0-9]{10})[""']",
        @"[""']security[""']\s*[:=]\s*[""'](?<nonce>[a-f0-9]{10})[""']",
        @"security\s*[:=]\s*[""'](?<nonce>[a-f0-9]{10})[""']",
        @"data-nonce\s*=\s*[""'](?<nonce>[a-f0-9]{10})[""']",
        @"name\s*=\s*[""']nonce[""'][^>]*value\s*=\s*[""'](?<nonce>[a-f0-9]{10})[""']",
        @"value\s*=\s*[""'](?<nonce>[a-f0-9]{10})[""'][^>]*name\s*=\s*[""']nonce[""']"
    };

            foreach (var pattern in patterns)
            {
                var matches = Regex.Matches(
                    html,
                    pattern,
                    RegexOptions.IgnoreCase | RegexOptions.Singleline);

                foreach (Match match in matches)
                {
                    var nonce = match.Groups["nonce"].Value.Trim();

                    if (!string.IsNullOrWhiteSpace(nonce) &&
                        !result.Contains(nonce, StringComparer.OrdinalIgnoreCase))
                    {
                        result.Add(nonce);
                    }
                }
            }

            var filterHomeWindowMatches = Regex.Matches(
                html,
                @"filterhome[\s\S]{0,2000}?(?<nonce>[a-f0-9]{10})",
                RegexOptions.IgnoreCase);

            foreach (Match match in filterHomeWindowMatches)
            {
                var nonce = match.Groups["nonce"].Value.Trim();

                if (!string.IsNullOrWhiteSpace(nonce) &&
                    !result.Contains(nonce, StringComparer.OrdinalIgnoreCase))
                {
                    result.Insert(0, nonce);
                }
            }

            return result
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();
        }

        private async Task<bool> IsFilterHomeNonceValidAsync(
    HttpClient client,
    string nonce,
    string pageUrl,
    bool isPastEvents,
    CancellationToken cancellationToken)
        {
            var today = DateTime.UtcNow.ToString("dd/MM/yyyy", CultureInfo.InvariantCulture);

            var parameters = new List<KeyValuePair<string, string>>
    {
        new("action", "filterhome"),
        new("nonce", nonce),
        new("period_start", isPastEvents ? "" : today),
        new("period_end", isPastEvents ? today : ""),
        new("zayavka_start", ""),
        new("zayavka_end", ""),
        new("location", ""),
        new("search_type", ""),
        new("page", "1"),
        new("past_events", isPastEvents ? "1" : "0"),
        new("actual_events", "0"),
        new("autor_id", "0"),
        new("search_keyword", ""),
        new("application", "")
    };

            try
            {
                using var request = new HttpRequestMessage(
                    HttpMethod.Post,
                    "https://na-konferencii.ru/wp-admin/admin-ajax.php");

                request.Headers.Referrer = new Uri(pageUrl);
                request.Headers.TryAddWithoutValidation("Origin", "https://na-konferencii.ru");
                request.Headers.TryAddWithoutValidation("X-Requested-With", "XMLHttpRequest");
                request.Headers.TryAddWithoutValidation("Accept", "*/*");
                request.Headers.TryAddWithoutValidation(
                    "User-Agent",
                    "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/148.0.0.0 Safari/537.36");

                request.Content = new FormUrlEncodedContent(parameters);

                var response = await client.SendAsync(request, cancellationToken);
                var html = await response.Content.ReadAsStringAsync(cancellationToken);

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogInformation(
                        "Проверка nonce {Nonce}: статус {StatusCode}",
                        nonce,
                        (int)response.StatusCode);

                    return false;
                }

                if (string.IsNullOrWhiteSpace(html))
                {
                    return false;
                }

                var hasConferenceItems =
                    html.Contains("notice-item", StringComparison.OrdinalIgnoreCase) ||
                    html.Contains("notice-item-title", StringComparison.OrdinalIgnoreCase);

                var looksBlocked =
                    html.Contains("forbidden", StringComparison.OrdinalIgnoreCase) ||
                    html.Contains("access denied", StringComparison.OrdinalIgnoreCase) ||
                    html.Contains("403", StringComparison.OrdinalIgnoreCase);

                _logger.LogInformation(
                    "Проверка nonce {Nonce}: html length {Length}, has items {HasItems}, blocked {Blocked}",
                    nonce,
                    html.Length,
                    hasConferenceItems,
                    looksBlocked);

                return hasConferenceItems && !looksBlocked;
            }
            catch (Exception ex)
            {
                _logger.LogInformation(ex, "Ошибка проверки nonce {Nonce}", nonce);
                return false;
            }
        }

        private static string? ExtractFilterHomeNonce(string html)
        {
            if (string.IsNullOrWhiteSpace(html))
            {
                return null;
            }

            var patterns = new[]
            {
        @"[""']nonce[""']\s*:\s*[""'](?<nonce>[a-zA-Z0-9]{6,32})[""']",
        @"nonce\s*:\s*[""'](?<nonce>[a-zA-Z0-9]{6,32})[""']",
        @"[""']ajax_nonce[""']\s*:\s*[""'](?<nonce>[a-zA-Z0-9]{6,32})[""']",
        @"ajax_nonce\s*:\s*[""'](?<nonce>[a-zA-Z0-9]{6,32})[""']",
        @"[""']security[""']\s*:\s*[""'](?<nonce>[a-zA-Z0-9]{6,32})[""']",
        @"security\s*:\s*[""'](?<nonce>[a-zA-Z0-9]{6,32})[""']",
        @"data-nonce\s*=\s*[""'](?<nonce>[a-zA-Z0-9]{6,32})[""']",
        @"name\s*=\s*[""']nonce[""'][^>]*value\s*=\s*[""'](?<nonce>[a-zA-Z0-9]{6,32})[""']",
        @"value\s*=\s*[""'](?<nonce>[a-zA-Z0-9]{6,32})[""'][^>]*name\s*=\s*[""']nonce[""']"
    };

            foreach (var pattern in patterns)
            {
                var match = Regex.Match(html, pattern, RegexOptions.IgnoreCase | RegexOptions.Singleline);

                if (match.Success)
                {
                    return match.Groups["nonce"].Value.Trim();
                }
            }

            var filterHomeMatch = Regex.Match(
                html,
                @"filterhome[\s\S]{0,1000}?[""']nonce[""']\s*[:=]\s*[""'](?<nonce>[a-zA-Z0-9]{6,32})[""']",
                RegexOptions.IgnoreCase);

            if (filterHomeMatch.Success)
            {
                return filterHomeMatch.Groups["nonce"].Value.Trim();
            }

            return null;
        }

        private static void ConfigureNaKonferenciiAjaxClient(HttpClient client, string referrerUrl)
        {
            client.DefaultRequestHeaders.Clear();

            client.DefaultRequestHeaders.Add("X-Requested-With", "XMLHttpRequest");
            client.DefaultRequestHeaders.Referrer = new Uri(referrerUrl);

            client.DefaultRequestHeaders.UserAgent.ParseAdd(
                "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/148.0.0.0 Safari/537.36");

            client.DefaultRequestHeaders.Add(
                "Accept",
                "text/html,application/xhtml+xml,application/xml;q=0.9,*/*;q=0.8");

            client.DefaultRequestHeaders.Add("Accept-Language", "ru-RU,ru;q=0.9,en-US;q=0.8,en;q=0.7");
            client.DefaultRequestHeaders.Add("Cache-Control", "no-cache");
        }

        public async Task<List<ConferenceImportModel>> ParsePastEventsAsync(CancellationToken cancellationToken = default)
        {
            var result = new List<ConferenceImportModel>();
            var conferenceUrls = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            var conferenceLocations = new Dictionary<string, (string? city, string? country)>(StringComparer.OrdinalIgnoreCase);

            using var client = new HttpClient();

            var pageUrl = "https://na-konferencii.ru/proshedshie-meroprijatija";

            ConfigureNaKonferenciiAjaxClient(client, pageUrl);

            var nonce = await GetFilterHomeNonceAsync(
            client,
            pageUrl,
            isPastEvents: false,
            cancellationToken);
            var baseParams = new List<KeyValuePair<string, string>>
            {
                new("action", "filterhome"),
                new("nonce", nonce),
                new("past_events", "1"),
                new("actual_events", "0"),
                new("period_start", ""),
                new("period_end", DateTime.Now.ToString("dd/MM/yyyy")),
                new("location", ""),
                new("search_type", ""),
                new("autor_id", "0"),
                new("search_keyword", ""),
                new("application", "")
            };

            const int totalPages = 536;
            for (int page = 1; page <= totalPages; page++)
            {
                cancellationToken.ThrowIfCancellationRequested();
                _logger.LogInformation("Обработка страницы {Page} из {Total}", page, totalPages);

                var parameters = new List<KeyValuePair<string, string>>(baseParams)
{
    new("page", page.ToString())
};

                string? html = null;
                const int maxRetries = 3;

                for (int retry = 1; retry <= maxRetries; retry++)
                {
                    try
                    {
                        using var content = new FormUrlEncodedContent(parameters);

                        var response = await client.PostAsync(
                            "https://na-konferencii.ru/wp-admin/admin-ajax.php",
                            content,
                            cancellationToken);

                        response.EnsureSuccessStatusCode();

                        html = await response.Content.ReadAsStringAsync(cancellationToken);
                        break;
                    }
                    catch (Exception ex) when (retry < maxRetries)
                    {
                        _logger.LogWarning(
                            ex,
                            "Ошибка запроса для страницы {Page}, попытка {Retry} из {MaxRetries}, повтор через {Delay}с",
                            page,
                            retry,
                            maxRetries,
                            Math.Pow(2, retry));

                        await Task.Delay(TimeSpan.FromSeconds(Math.Pow(2, retry)), cancellationToken);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(
                            ex,
                            "Ошибка запроса для страницы {Page} после {MaxRetries} попыток, пропускаем",
                            page,
                            maxRetries);

                        break;
                    }
                }

                if (string.IsNullOrWhiteSpace(html) || html.Length < 100)
                {
                    _logger.LogInformation("Пустой ответ или ошибка на странице {Page}, завершаем", page);
                    break;
                }

                if (page == 1)
                {
                    _logger.LogInformation("HTML ответа (первые 500 символов): {Html}", html.Length > 500 ? html[..500] : html);
                    var debugPath = Path.Combine(Path.GetTempPath(), "na-konferencii_response_page1.html");
                    await File.WriteAllTextAsync(debugPath, html, cancellationToken);
                    _logger.LogInformation("Полный ответ сохранён в {Path}", debugPath);
                }

                var doc = new HtmlDocument();
                doc.LoadHtml(html);
                var items = doc.DocumentNode.SelectNodes("//div[contains(@class,'notice-item')]");
                if (items == null || items.Count == 0)
                {
                    _logger.LogInformation("Нет элементов конференций на странице {Page}, завершаем", page);
                    break;
                }

                foreach (var item in items)
                {
                    var linkNode = item.SelectSingleNode(".//div[contains(@class,'notice-item-title')]/a");
                    if (linkNode == null) continue;
                    string href = linkNode.GetAttributeValue("href", "");
                    if (string.IsNullOrEmpty(href)) continue;
                    string absoluteUrl = BuildAbsoluteUrl("https://na-konferencii.ru", href);
                    absoluteUrl = NormalizeUrl(absoluteUrl);
                    if (string.IsNullOrEmpty(absoluteUrl)) continue;
                    conferenceUrls.Add(absoluteUrl);

                    var locationNode = item.SelectSingleNode(".//div[contains(@class,'notice-item-top-location')]//p");
                    if (locationNode != null)
                    {
                        string locationText = CleanText(locationNode.InnerText);
                        var (city, country) = ParseCityCountryFromLocationString(locationText);
                        if (!conferenceLocations.ContainsKey(absoluteUrl))
                            conferenceLocations[absoluteUrl] = (city, country);
                    }
                }
                _logger.LogInformation("Собрано {Count} уникальных ссылок, всего {Total}", conferenceUrls.Count, page);
                await Task.Delay(200, cancellationToken);
            }

            _logger.LogInformation("Всего собрано ссылок: {Count}", conferenceUrls.Count);

            using var detailsClient = new HttpClient();
            detailsClient.DefaultRequestHeaders.UserAgent.ParseAdd("Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/148.0.0.0 Safari/537.36");

            foreach (var confUrl in conferenceUrls)
            {
                cancellationToken.ThrowIfCancellationRequested();
                try
                {
                    _logger.LogInformation("Парсинг детальной страницы: {Url}", confUrl);
                    var html = await detailsClient.GetStringAsync(confUrl, cancellationToken);
                    var conference = ParseConferenceDetailsPage(html, confUrl, conferenceLocations);
                    if (conference != null && !string.IsNullOrWhiteSpace(conference.Title))
                    {
                        result.Add(conference);
                    }
                    else
                    {
                        _logger.LogWarning("Не удалось распарсить конференцию: {Url}", confUrl);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Ошибка парсинга {Url}", confUrl);
                }
            }

            var finalResult = result
                .GroupBy(x => NormalizeUrl(x.Url))
                .Select(g => g.First())
                .GroupBy(x => $"{x.Title.Trim().ToLowerInvariant()}|{x.StartDate:yyyy-MM-dd}")
                .Select(g => g.First())
                .ToList();

            _logger.LogInformation("Итоговое количество уникальных конференций: {Count}", finalResult.Count);
            return finalResult;
        }

        public async Task<List<ConferenceImportModel>> ParseAnnouncementsAsync(CancellationToken cancellationToken = default)
        {
            var result = new List<ConferenceImportModel>();
            var conferenceUrls = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            var conferenceLocations = new Dictionary<string, (string? city, string? country)>(StringComparer.OrdinalIgnoreCase);

            using var client = new HttpClient();

            var pageUrl = "https://na-konferencii.ru/anonsy-nauchnyh-meroprijatij";

            ConfigureNaKonferenciiAjaxClient(client, pageUrl);

            var nonce = await GetFilterHomeNonceAsync(
            client,
            pageUrl,
            isPastEvents: true,
            cancellationToken);
            var baseParams = new List<KeyValuePair<string, string>>
            {
                new("action", "filterhome"),
                new("nonce", nonce),
                new("past_events", "0"),
                new("actual_events", "0"),
                new("period_start", DateTime.Now.ToString("dd/MM/yyyy")),
                new("period_end", ""),
                new("location", ""),
                new("search_type", ""),
                new("autor_id", "0"),
                new("search_keyword", ""),
                new("application", ""),
                new("zayavka_start", ""),
                new("zayavka_end", "")
            };

            const int totalPages = 9;
            for (int page = 1; page <= totalPages; page++)
            {
                cancellationToken.ThrowIfCancellationRequested();
                _logger.LogInformation("Обработка страницы анонсов {Page} из {Total}", page, totalPages);

                var parameters = new List<KeyValuePair<string, string>>(baseParams)
{
    new("page", page.ToString())
};

                string? html = null;
                const int maxRetries = 3;

                for (int retry = 1; retry <= maxRetries; retry++)
                {
                    try
                    {
                        using var content = new FormUrlEncodedContent(parameters);

                        var response = await client.PostAsync(
                            "https://na-konferencii.ru/wp-admin/admin-ajax.php",
                            content,
                            cancellationToken);

                        response.EnsureSuccessStatusCode();

                        html = await response.Content.ReadAsStringAsync(cancellationToken);
                        break;
                    }
                    catch (Exception ex) when (retry < maxRetries)
                    {
                        _logger.LogWarning(
                            ex,
                            "Ошибка запроса для страницы {Page}, попытка {Retry} из {MaxRetries}, повтор через {Delay}с",
                            page,
                            retry,
                            maxRetries,
                            Math.Pow(2, retry));

                        await Task.Delay(TimeSpan.FromSeconds(Math.Pow(2, retry)), cancellationToken);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(
                            ex,
                            "Ошибка запроса для страницы {Page} после {MaxRetries} попыток, пропускаем",
                            page,
                            maxRetries);

                        break;
                    }
                }
                if (page == 1 && !string.IsNullOrWhiteSpace(html))
                {
                    var debugPath = Path.Combine(Path.GetTempPath(), $"anonsy_page1_{DateTime.Now:yyyyMMddHHmmss}.html");
                    await File.WriteAllTextAsync(debugPath, html, cancellationToken);
                    _logger.LogInformation("Сохранён ответ анонсов страницы 1 в {Path}", debugPath);
                }
                if (string.IsNullOrWhiteSpace(html) || html.Length < 100) break;

                var doc = new HtmlDocument();
                doc.LoadHtml(html);
                var items = doc.DocumentNode.SelectNodes("//div[contains(@class,'notice-item')]");
                if (items == null || items.Count == 0) break;

                foreach (var item in items)
                {
                    var linkNode = item.SelectSingleNode(".//div[contains(@class,'notice-item-title')]/a");
                    if (linkNode == null) continue;
                    string href = linkNode.GetAttributeValue("href", "");
                    if (string.IsNullOrEmpty(href)) continue;
                    string absoluteUrl = BuildAbsoluteUrl("https://na-konferencii.ru", href);
                    absoluteUrl = NormalizeUrl(absoluteUrl);
                    if (string.IsNullOrEmpty(absoluteUrl)) continue;
                    conferenceUrls.Add(absoluteUrl);

                    var locationNode = item.SelectSingleNode(".//div[contains(@class,'notice-item-top-location')]//p");
                    if (locationNode != null)
                    {
                        string locationText = CleanText(locationNode.InnerText);
                        var (city, country) = ParseCityCountryFromLocationString(locationText);
                        if (!conferenceLocations.ContainsKey(absoluteUrl))
                            conferenceLocations[absoluteUrl] = (city, country);
                    }
                }
                _logger.LogInformation("Собрано {Count} уникальных ссылок из анонсов (страница {Page})", conferenceUrls.Count, page);
                await Task.Delay(200, cancellationToken);
            }

            using var detailsClient = new HttpClient();
            detailsClient.DefaultRequestHeaders.UserAgent.ParseAdd("Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/148.0.0.0 Safari/537.36");

            foreach (var confUrl in conferenceUrls)
            {
                cancellationToken.ThrowIfCancellationRequested();
                try
                {
                    _logger.LogInformation("Парсинг детальной страницы (анонс): {Url}", confUrl);
                    var html = await detailsClient.GetStringAsync(confUrl, cancellationToken);
                    var conference = ParseConferenceDetailsPage(html, confUrl, conferenceLocations);
                    if (conference != null && !string.IsNullOrWhiteSpace(conference.Title))
                        result.Add(conference);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Ошибка парсинга {Url}", confUrl);
                }
            }

            var finalResult = result
                .GroupBy(x => NormalizeUrl(x.Url))
                .Select(g => g.First())
                .GroupBy(x => $"{x.Title.Trim().ToLowerInvariant()}|{x.StartDate:yyyy-MM-dd}")
                .Select(g => g.First())
                .ToList();

            _logger.LogInformation("Итоговое количество уникальных конференций (анонсы): {Count}", finalResult.Count);
            return finalResult;
        }
    }
}
