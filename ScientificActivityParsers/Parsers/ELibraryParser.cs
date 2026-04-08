using HtmlAgilityPack;
using ScientificActivityContracts.BindingModels;
using ScientificActivityContracts.ViewModels;
using ScientificActivityParsers.Interfaces;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace ScientificActivityParsers.Parsers
{
    public class ELibraryParser : IELibraryParser
    {
        private readonly HttpClient _httpClient;

        public ELibraryParser(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public List<ELibraryAuthorSearchViewModel> SearchAuthors(ELibraryAuthorSearchBindingModel model)
        {
            if (string.IsNullOrWhiteSpace(model.LastName))
            {
                throw new ArgumentException("Не указана фамилия автора");
            }

            return new List<ELibraryAuthorSearchViewModel>();
        }

        public ELibraryAuthorProfileViewModel? GetAuthorProfile(string authorId)
        {
            if (string.IsNullOrWhiteSpace(authorId))
            {
                throw new ArgumentException("Не указан AuthorId");
            }

            var url = $"https://elibrary.ru/author_profile.asp?id={authorId}";
            var html = _httpClient.GetStringAsync(url).Result;

            if (string.IsNullOrWhiteSpace(html))
            {
                return null;
            }

            EnsureNotBlocked(html);

            var doc = new HtmlDocument();
            doc.LoadHtml(html);

            var profile = new ELibraryAuthorProfileViewModel
            {
                AuthorId = authorId
            };

            ParseHeaderBlock(doc, profile);
            ParseGeneralIndicators(doc, profile);
            ParseYearIndicators(doc, profile);

            return profile;
        }

        private static void EnsureNotBlocked(string html)
        {
            var blockMarkers = new[]
            {
                "доступ к данной странице",
                "доступ к сайту",
                "ограничен",
                "незарегистрированных пользователей",
                "недостаточно прав для открытия страницы",
                "закончилась текущая сессия"
            };

            foreach (var marker in blockMarkers)
            {
                if (html.Contains(marker, StringComparison.OrdinalIgnoreCase))
                {
                    throw new Exception("eLibrary не вернул страницу автора. Доступ ограничен или требуется авторизация.");
                }
            }
        }

        private static void ParseHeaderBlock(HtmlDocument doc, ELibraryAuthorProfileViewModel profile)
        {
            var headerDiv = doc.DocumentNode.SelectSingleNode("//div[contains(@style,'width:540px') and contains(@style,'text-align: center')]");
            if (headerDiv == null)
            {
                return;
            }

            var fullNameNode = headerDiv.SelectSingleNode(".//b");
            if (fullNameNode != null)
            {
                profile.FullName = ToTitleCaseRu(CleanText(fullNameNode.InnerText));
            }

            var organizationLink = headerDiv.SelectSingleNode(".//a[contains(@href,'org_profile.asp')]");
            if (organizationLink != null)
            {
                profile.Organization = CleanText(organizationLink.InnerText);
            }

            var headerText = CleanText(headerDiv.InnerText);

            var spinMatch = Regex.Match(headerText, @"SPIN-код:\s*([\d\-]+)", RegexOptions.IgnoreCase);
            if (spinMatch.Success)
            {
                profile.SpinCode = spinMatch.Groups[1].Value.Trim();
            }

            var authorIdMatch = Regex.Match(headerText, @"AuthorID:\s*(\d+)", RegexOptions.IgnoreCase);
            if (authorIdMatch.Success)
            {
                profile.AuthorId = authorIdMatch.Groups[1].Value.Trim();
            }

            var department = ExtractDepartment(headerDiv);
            if (!string.IsNullOrWhiteSpace(department))
            {
                profile.Department = department;
            }
        }

        private static void ParseGeneralIndicators(HtmlDocument doc, ELibraryAuthorProfileViewModel profile)
        {
            var table = FindTableAfterHeader(doc, "ОБЩИЕ ПОКАЗАТЕЛИ");
            if (table == null)
            {
                return;
            }

            var rows = table.SelectNodes(".//tr");
            if (rows == null)
            {
                return;
            }

            foreach (var row in rows)
            {
                var tds = row.SelectNodes("./td");
                if (tds == null || tds.Count < 3)
                {
                    continue;
                }

                var indicatorName = CleanText(tds[1].InnerText);
                var indicatorValue = CleanText(tds[2].InnerText);

                if (string.IsNullOrWhiteSpace(indicatorName))
                {
                    continue;
                }

                switch (indicatorName)
                {
                    case "Число публикаций на elibrary.ru":
                        profile.PublicationsCountElibrary = ExtractFirstInt(indicatorValue);
                        break;
                    case "Число публикаций в РИНЦ":
                        profile.PublicationsCountRinc = ExtractFirstInt(indicatorValue);
                        break;
                    case "Число публикаций, входящих в ядро РИНЦ":
                        profile.PublicationsCoreRincCount = ExtractFirstInt(indicatorValue);
                        break;
                    case "Число цитирований из публикаций на elibrary.ru":
                        profile.CitationsCountElibrary = ExtractFirstInt(indicatorValue);
                        break;
                    case "Число цитирований из публикаций, входящих в РИНЦ":
                        profile.CitationsCountRinc = ExtractFirstInt(indicatorValue);
                        break;
                    case "Число цитирований из публикаций, входящих в ядро РИНЦ":
                        profile.CitationsCoreRincCount = ExtractFirstInt(indicatorValue);
                        break;
                    case "Индекс Хирша по всем публикациям на elibrary.ru":
                        profile.HIndexElibrary = ExtractFirstInt(indicatorValue);
                        break;
                    case "Индекс Хирша по публикациям в РИНЦ":
                        profile.HIndexRinc = ExtractFirstInt(indicatorValue);
                        break;
                    case "Индекс Хирша по ядру РИНЦ":
                        profile.HIndexCoreRinc = ExtractFirstInt(indicatorValue);
                        break;
                    case "Число публикаций, процитировавших работы автора":
                        profile.PublicationsCitingAuthorCount = ExtractFirstInt(indicatorValue);
                        break;
                    case "Число ссылок на самую цитируемую публикацию":
                        profile.MostCitedPublicationCitationsCount = ExtractFirstInt(indicatorValue);
                        break;
                    case "Число публикаций автора, процитированных хотя бы один раз":
                        profile.CitedPublicationsCount = ExtractFirstInt(indicatorValue);
                        break;
                    case "Среднее число цитирований в расчете на одну публикацию":
                        profile.AverageCitationsPerPublication = ExtractFirstDecimal(indicatorValue);
                        break;
                    case "Индекс Хирша без учета самоцитирований":
                        profile.HIndexWithoutSelfCitations = ExtractFirstInt(indicatorValue);
                        break;
                    case "Год первой публикации":
                        profile.FirstPublicationYear = ExtractFirstInt(indicatorValue);
                        break;
                    case "Число самоцитирований":
                        profile.SelfCitationsCount = ExtractFirstInt(indicatorValue);
                        break;
                    case "Число цитирований соавторами":
                        profile.CoauthorCitationsCount = ExtractFirstInt(indicatorValue);
                        break;
                    case "Число соавторов":
                        profile.CoauthorsCount = ExtractFirstInt(indicatorValue);
                        break;
                    case "Число статей в зарубежных журналах":
                        profile.ForeignArticlesCount = ExtractFirstInt(indicatorValue);
                        break;
                    case "Число статей в российских журналах":
                        profile.RussianArticlesCount = ExtractFirstInt(indicatorValue);
                        break;
                    case "Число статей в российских журналах из перечня ВАК":
                        profile.VakArticlesCount = ExtractFirstInt(indicatorValue);
                        break;
                    case "Число статей в журналах с ненулевым импакт-фактором":
                        profile.ImpactFactorArticlesCount = ExtractFirstInt(indicatorValue);
                        break;
                    case "Число цитирований из зарубежных журналов":
                        profile.ForeignJournalCitationsCount = ExtractFirstInt(indicatorValue);
                        break;
                    case "Число цитирований из российских журналов":
                        profile.RussianJournalCitationsCount = ExtractFirstInt(indicatorValue);
                        break;
                    case "Число цитирований из российских журналов из перечня ВАК":
                        profile.VakJournalCitationsCount = ExtractFirstInt(indicatorValue);
                        break;
                    case "Число цитирований из журналов с ненулевым импакт-фактором":
                        profile.ImpactFactorJournalCitationsCount = ExtractFirstInt(indicatorValue);
                        break;
                    case "Средневзвешенный импакт-фактор журналов, в которых были опубликованы статьи":
                        profile.AverageWeightedImpactFactorPublished = ExtractFirstDecimal(indicatorValue);
                        break;
                    case "Средневзвешенный импакт-фактор журналов, в которых были процитированы статьи":
                        profile.AverageWeightedImpactFactorCited = ExtractFirstDecimal(indicatorValue);
                        break;
                    case "Число публикаций в РИНЦ за последние 5 лет (2020-2024)":
                        profile.PublicationsRincLast5YearsCount = ExtractFirstInt(indicatorValue);
                        break;
                    case "Число публикаций в ядре РИНЦ за последние 5 лет":
                        profile.PublicationsCoreRincLast5YearsCount = ExtractFirstInt(indicatorValue);
                        break;
                    case "Число ссылок из РИНЦ на работы, опубликованные за последние 5 лет":
                        profile.CitationsRincLast5YearsCount = ExtractFirstInt(indicatorValue);
                        break;
                    case "Число ссылок из ядра РИНЦ на работы, опубликованные за последние 5 лет":
                        profile.CitationsCoreRincLast5YearsCount = ExtractFirstInt(indicatorValue);
                        break;
                    case "Число ссылок на работы автора из всех публикаций за последние 5 лет":
                        profile.CitationsAllLast5YearsCount = ExtractFirstInt(indicatorValue);
                        break;
                    case "Процентиль по ядру РИНЦ":
                        profile.PercentileCoreRinc = ExtractFirstInt(indicatorValue);
                        break;
                }
            }

            var innerRows = table.SelectNodes(".//table//tr");
            if (innerRows != null)
            {
                foreach (var row in innerRows)
                {
                    var tds = row.SelectNodes("./td");
                    if (tds == null || tds.Count < 3)
                    {
                        continue;
                    }

                    var name = CleanText(tds[1].InnerText);
                    var value = CleanText(tds[2].InnerText);

                    if (name.Contains("Основная рубрика (ГРНТИ)", StringComparison.OrdinalIgnoreCase))
                    {
                        profile.MainRubricGrnti = value;
                    }
                    else if (name.Contains("Основная рубрика (OECD)", StringComparison.OrdinalIgnoreCase))
                    {
                        profile.MainRubricOecd = value;
                    }
                }
            }
        }

        private static void ParseYearIndicators(HtmlDocument doc, ELibraryAuthorProfileViewModel profile)
        {
            var table = FindTableAfterHeader(doc, "ПОКАЗАТЕЛИ ПО ГОДАМ");
            if (table == null)
            {
                return;
            }

            var rows = table.SelectNodes(".//tr");
            if (rows == null || rows.Count == 0)
            {
                return;
            }

            List<int> years = new();

            foreach (var row in rows)
            {
                var cells = row.SelectNodes("./th|./td");
                if (cells == null || cells.Count == 0)
                {
                    continue;
                }

                var cellTexts = cells
                    .Select(c => CleanText(c.InnerText))
                    .Where(x => !string.IsNullOrWhiteSpace(x))
                    .ToList();

                if (cellTexts.Count == 0)
                {
                    continue;
                }

                if (years.Count == 0)
                {
                    years = cellTexts
                        .Select(x => int.TryParse(x, out var year) ? year : (int?)null)
                        .Where(x => x.HasValue)
                        .Select(x => x!.Value)
                        .ToList();

                    if (years.Count > 0)
                    {
                        continue;
                    }
                }

                var indicatorName = ExtractIndicatorNameFromRow(row);
                if (string.IsNullOrWhiteSpace(indicatorName))
                {
                    continue;
                }

                var values = ExtractRowValues(row);
                if (values.Count == 0 || years.Count == 0)
                {
                    continue;
                }

                var yearValuePairs = new Dictionary<int, int>();
                for (int i = 0; i < years.Count && i < values.Count; i++)
                {
                    yearValuePairs[years[i]] = values[i];
                }

                if (indicatorName.Contains("Число публикаций в РИНЦ", StringComparison.OrdinalIgnoreCase) &&
                    indicatorName.Contains("за 5 лет", StringComparison.OrdinalIgnoreCase))
                {
                    profile.PublicationsRinc5YearsByEndYear = yearValuePairs;
                }
                else if (indicatorName.Contains("Число публикаций в ядре", StringComparison.OrdinalIgnoreCase) &&
                         indicatorName.Contains("за 5 лет", StringComparison.OrdinalIgnoreCase))
                {
                    profile.PublicationsCoreRinc5YearsByEndYear = yearValuePairs;
                }
                else if (indicatorName.Contains("Число цитирований в РИНЦ", StringComparison.OrdinalIgnoreCase) &&
                         indicatorName.Contains("за 5 лет", StringComparison.OrdinalIgnoreCase))
                {
                    profile.CitationsRinc5YearsByEndYear = yearValuePairs;
                }
                else if (indicatorName.Contains("Число цитирований из ядра", StringComparison.OrdinalIgnoreCase) &&
                         indicatorName.Contains("за 5 лет", StringComparison.OrdinalIgnoreCase))
                {
                    profile.CitationsCoreRinc5YearsByEndYear = yearValuePairs;
                }
                else if (indicatorName.Contains("Число публикаций в РИНЦ", StringComparison.OrdinalIgnoreCase))
                {
                    profile.PublicationsRincByYear = yearValuePairs;
                }
                else if (indicatorName.Contains("Число публикаций в ядре", StringComparison.OrdinalIgnoreCase))
                {
                    profile.PublicationsCoreRincByYear = yearValuePairs;
                }
                else if (indicatorName.Contains("Число цитирований в РИНЦ", StringComparison.OrdinalIgnoreCase))
                {
                    profile.CitationsRincByYear = yearValuePairs;
                }
                else if (indicatorName.Contains("Число цитирований из ядра", StringComparison.OrdinalIgnoreCase))
                {
                    profile.CitationsCoreRincByYear = yearValuePairs;
                }
                else if (indicatorName.Contains("Индекс Хирша в РИНЦ", StringComparison.OrdinalIgnoreCase))
                {
                    profile.HIndexRincByYear = yearValuePairs;
                }
                else if (indicatorName.Contains("Индекс Хирша по ядру РИНЦ", StringComparison.OrdinalIgnoreCase))
                {
                    profile.HIndexCoreRincByYear = yearValuePairs;
                }
                else if (indicatorName.Contains("Процентиль по ядру РИНЦ", StringComparison.OrdinalIgnoreCase))
                {
                    profile.PercentileCoreRincByYear = yearValuePairs;
                }
            }
        }

        private static HtmlNode? FindTableAfterHeader(HtmlDocument doc, string headerText)
        {
            var headerNode = doc.DocumentNode.SelectSingleNode(
                $"//div[contains(@class,'midtext') and .//b[contains(normalize-space(.), '{headerText}')]]");

            if (headerNode == null)
            {
                return null;
            }

            var current = headerNode.NextSibling;
            while (current != null)
            {
                if (current.NodeType != HtmlNodeType.Element)
                {
                    current = current.NextSibling;
                    continue;
                }

                if (current.Name.Equals("table", StringComparison.OrdinalIgnoreCase))
                {
                    return current;
                }

                var nestedTable = current.SelectSingleNode(".//table");
                if (nestedTable != null)
                {
                    return nestedTable;
                }

                current = current.NextSibling;
            }

            return null;
        }

        private static List<int> ExtractYearsFromHeaderRow(HtmlNode headerRow)
        {
            var years = new List<int>();
            var cells = headerRow.SelectNodes("./td|./th");
            if (cells == null)
            {
                return years;
            }

            foreach (var cell in cells)
            {
                var text = CleanText(cell.InnerText);
                if (int.TryParse(text, out var year))
                {
                    years.Add(year);
                }
            }

            return years;
        }

        private static string CleanText(string? text)
        {
            if (string.IsNullOrWhiteSpace(text))
            {
                return string.Empty;
            }

            var decoded = HtmlEntity.DeEntitize(text);
            decoded = decoded.Replace("\r", " ").Replace("\n", " ").Replace("\t", " ");
            decoded = Regex.Replace(decoded, @"\s+", " ");
            return decoded.Trim();
        }

        private static int? ExtractFirstInt(string? text)
        {
            if (string.IsNullOrWhiteSpace(text))
            {
                return null;
            }

            var match = Regex.Match(text, @"\d+");
            if (!match.Success)
            {
                return null;
            }

            return int.TryParse(match.Value, out var value) ? value : null;
        }

        private static decimal? ExtractFirstDecimal(string? text)
        {
            if (string.IsNullOrWhiteSpace(text))
            {
                return null;
            }

            var match = Regex.Match(text, @"\d+([.,]\d+)?");
            if (!match.Success)
            {
                return null;
            }

            var normalized = match.Value.Replace(',', '.');
            return decimal.TryParse(normalized, NumberStyles.Any, CultureInfo.InvariantCulture, out var value)
                ? value
                : null;
        }

        private static string? ExtractDepartment(HtmlNode headerDiv)
        {
            var rawHtml = headerDiv.InnerHtml;

            var match = Regex.Match(
                rawHtml,
                @"кафедра\s+([^<(]+)",
                RegexOptions.IgnoreCase);

            if (!match.Success)
            {
                return null;
            }

            var department = HtmlEntity.DeEntitize(match.Groups[1].Value).Trim();

            if (string.IsNullOrWhiteSpace(department))
            {
                return null;
            }

            return ToTitleCaseRu(department);
        }

        private static string ToTitleCaseRu(string? text)
        {
            if (string.IsNullOrWhiteSpace(text))
            {
                return string.Empty;
            }

            var cleaned = CleanText(text).ToLowerInvariant();
            var words = cleaned.Split(' ', StringSplitOptions.RemoveEmptyEntries);

            for (int i = 0; i < words.Length; i++)
            {
                var word = words[i];
                if (word.Length == 1)
                {
                    words[i] = word.ToUpperInvariant();
                    continue;
                }

                words[i] = char.ToUpperInvariant(word[0]) + word[1..];
            }

            return string.Join(" ", words);
        }

        private static string ExtractIndicatorNameFromRow(HtmlNode row)
        {
            var firstCell = row.SelectSingleNode("./td[1]") ?? row.SelectSingleNode("./th[1]");
            if (firstCell == null)
            {
                return string.Empty;
            }

            var text = CleanText(firstCell.InnerText);

            if (string.IsNullOrWhiteSpace(text))
            {
                return string.Empty;
            }

            return text;
        }

        private static List<int> ExtractRowValues(HtmlNode row)
        {
            var result = new List<int>();

            var cells = row.SelectNodes("./td|./th");
            if (cells == null || cells.Count <= 1)
            {
                return result;
            }

            foreach (var cell in cells.Skip(1))
            {
                var text = CleanText(cell.InnerText);

                if (string.IsNullOrWhiteSpace(text))
                {
                    continue;
                }

                var value = ExtractFirstInt(text);
                if (value.HasValue)
                {
                    result.Add(value.Value);
                }
            }

            return result;
        }
    }
}
