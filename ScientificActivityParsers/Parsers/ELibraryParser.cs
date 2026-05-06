using HtmlAgilityPack;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using Newtonsoft.Json;
using OpenQA.Selenium.Support.UI;
using ScientificActivityContracts.BindingModels;
using ScientificActivityContracts.ViewModels;
using ScientificActivityParsers.Interfaces;
using System.Globalization;
using System.Text.RegularExpressions;
using ScientificActivityParsers.Models;

namespace ScientificActivityParsers.Parsers
{
    public class ELibraryParser : IELibraryParser
    {
        private static List<ELibraryCookieModel> LoadCookiesFromFile()
        {
            var path = Path.Combine(AppContext.BaseDirectory, "elibrary-cookies.json");

            if (!File.Exists(path))
            {
                Console.WriteLine($"Файл cookies не найден: {path}");
                return new List<ELibraryCookieModel>();
            }

            var json = File.ReadAllText(path);

            if (string.IsNullOrWhiteSpace(json))
            {
                return new List<ELibraryCookieModel>();
            }

            var cookies = JsonConvert.DeserializeObject<List<ELibraryCookieModel>>(json);

            return cookies ?? new List<ELibraryCookieModel>();
        }

        private static void ApplyELibraryCookies(IWebDriver driver)
        {
            var cookies = LoadCookiesFromFile();

            if (cookies.Count == 0)
            {
                Console.WriteLine("Cookies eLibrary не загружены: список пуст.");
                return;
            }

            driver.Navigate().GoToUrl("https://www.elibrary.ru/");
            WaitForDocumentReady(driver, TimeSpan.FromSeconds(30));

            foreach (var cookieModel in cookies)
            {
                if (string.IsNullOrWhiteSpace(cookieModel.Name) ||
                    string.IsNullOrWhiteSpace(cookieModel.Value))
                {
                    continue;
                }

                try
                {
                    var cookie = new Cookie(
                        cookieModel.Name,
                        cookieModel.Value,
                        cookieModel.Path,
                        cookieModel.Expiry);

                    driver.Manage().Cookies.AddCookie(cookie);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Не удалось добавить cookie {cookieModel.Name}: {ex.Message}");
                }
            }

            driver.Navigate().Refresh();
            WaitForDocumentReady(driver, TimeSpan.FromSeconds(30));

            Console.WriteLine("Cookies после применения:");

            foreach (var cookie in driver.Manage().Cookies.AllCookies)
            {
                Console.WriteLine($"{cookie.Name} | {cookie.Domain} | {cookie.Path}");
            }
        }

        private static readonly Dictionary<string, string> ELibraryCategoryQueryNames = new()
        {
            ["rinc"] = "cats_risc",
            ["coreRinc"] = "cats_corerisc",

            ["whiteList1"] = "cats_whitelist1",
            ["whiteList2"] = "cats_whitelist2",
            ["whiteList3"] = "cats_whitelist3",
            ["whiteList4"] = "cats_whitelist4",

            ["rsci"] = "cats_rsci",

            ["scopusQ1"] = "cats_scopus1",
            ["scopusQ2"] = "cats_scopus2",
            ["scopusQ3"] = "cats_scopus3",
            ["scopusQ4"] = "cats_scopus4",

            ["wosQ1"] = "cats_wos1",
            ["wosQ2"] = "cats_wos2",
            ["wosQ3"] = "cats_wos3",
            ["wosQ4"] = "cats_wos4",
            ["wosNoQuartile"] = "cats_wos5",

            ["vak"] = "cats_vak",
            ["vak1"] = "cats_vak1",
            ["vak2"] = "cats_vak2",
            ["vak3"] = "cats_vak3"
        };

        public ELibraryPublicationCategoryInfoModel GetAuthorPublicationCategoryInfo(string authorId)
        {
            if (string.IsNullOrWhiteSpace(authorId))
            {
                throw new ArgumentException("Не указан AuthorId");
            }

            using var driver = CreateChromeDriver();
            ApplyELibraryCookies(driver);

            var result = new ELibraryPublicationCategoryInfoModel();

            var baseHtml = LoadPageHtml(
                driver,
                $"https://www.elibrary.ru/author_items.asp?authorid={authorId}&show_refs=1&hide_doubles=1");

            var baseDoc = new HtmlDocument();
            baseDoc.LoadHtml(baseHtml);

            result.CategoryCounts = ParseCategoryCounts(baseDoc);

            foreach (var category in ELibraryCategoryQueryNames)
            {
                try
                {
                    var publicationIds = LoadPublicationIdsByCategory(driver, authorId, category.Value);

                    result.PublicationIdsByCategory[category.Key] = publicationIds;

                    Console.WriteLine($"ELibrary category {category.Key}. Publications count:{publicationIds.Count}");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Категория eLibrary {category.Key} не загружена: {ex.Message}");
                }

                Thread.Sleep(2000);
            }

            return result;
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

            using var driver = CreateChromeDriver();
            ApplyELibraryCookies(driver);

            var url = $"https://elibrary.ru/author_profile.asp?id={authorId}";
            var html = LoadPageHtml(driver, url, true);

            if (string.IsNullOrWhiteSpace(html))
            {
                return null;
            }

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

        public List<ELibraryPublicationImportModel> GetAuthorPublications(string authorId)
        {
            if (string.IsNullOrWhiteSpace(authorId))
            {
                throw new ArgumentException("Не указан AuthorId");
            }

            using var driver = CreateChromeDriver();
            ApplyELibraryCookies(driver);

            var url = $"https://www.elibrary.ru/author_items.asp?authorid={authorId}&show_refs=1";
            var html = LoadPageHtml(driver, url, true);

            Console.WriteLine($"ELibrary author_items html length: {html.Length}");
            Console.WriteLine($"Contains restab: {html.Contains("id=\"restab\"", StringComparison.OrdinalIgnoreCase)}");
            Console.WriteLine($"Contains arw: {html.Contains("id=\"arw", StringComparison.OrdinalIgnoreCase)}");
            Console.WriteLine($"Contains brw: {html.Contains("id=\"brw", StringComparison.OrdinalIgnoreCase)}");
            Console.WriteLine($"Contains robot: {html.Contains("робот", StringComparison.OrdinalIgnoreCase)}");
            Console.WriteLine($"Contains captcha: {html.Contains("captcha", StringComparison.OrdinalIgnoreCase)}");
            Console.WriteLine($"Contains доступ ограничен: {html.Contains("доступ", StringComparison.OrdinalIgnoreCase) && html.Contains("огранич", StringComparison.OrdinalIgnoreCase)}");

            if (string.IsNullOrWhiteSpace(html))
            {
                return new List<ELibraryPublicationImportModel>();
            }

            var doc = new HtmlDocument();
            doc.LoadHtml(html);

            var result = ParsePublicationList(doc);
            Console.WriteLine($"ELibrary GetAuthorPublications parsed count: {result.Count}");

            foreach (var publication in result)
            {
                if (string.IsNullOrWhiteSpace(publication.ELibraryId))
                {
                    continue;
                }

                try
                {
                    var detailUrl = $"https://www.elibrary.ru/item.asp?id={publication.ELibraryId}";
                    var detailHtml = LoadPageHtml(driver, detailUrl, false);

                    if (string.IsNullOrWhiteSpace(detailHtml))
                    {
                        continue;
                    }

                    var detailDoc = new HtmlDocument();
                    detailDoc.LoadHtml(detailHtml);

                    EnrichPublicationFromDetails(detailDoc, publication);

                    if (string.IsNullOrWhiteSpace(publication.Url))
                    {
                        publication.Url = detailUrl;
                    }

                    Thread.Sleep(2000);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Ошибка загрузки деталки публикации {publication.ELibraryId}: {ex.Message}");

                    try
                    {
                        var debugDir = Path.Combine(AppContext.BaseDirectory, "elibrary-debug");
                        Directory.CreateDirectory(debugDir);

                        var debugPath = Path.Combine(debugDir, $"item_{publication.ELibraryId}.html");
                        File.WriteAllText(debugPath, driver.PageSource ?? string.Empty);

                        Console.WriteLine($"HTML проблемной деталки сохранён: {debugPath}");
                    }
                    catch (Exception saveEx)
                    {
                        Console.WriteLine($"Не удалось сохранить HTML деталки {publication.ELibraryId}: {saveEx.Message}");
                    }

                    continue;
                }
            }

            return result;
        }

        private static IWebDriver CreateChromeDriver()
        {
            var options = new ChromeOptions();

            options.AddArgument("--headless=new");
            options.AddArgument("--window-size=1920,1080");
            options.AddArgument("--disable-gpu");
            options.AddArgument("--no-sandbox");
            options.AddArgument("--disable-dev-shm-usage");
            options.AddArgument("--lang=ru-RU");
            options.AddArgument("--user-agent=Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/135.0.0.0 Safari/537.36");

            options.PageLoadStrategy = PageLoadStrategy.Normal;

            var service = ChromeDriverService.CreateDefaultService();
            service.HideCommandPromptWindow = true;
            service.SuppressInitialDiagnosticInformation = true;

            var driver = new ChromeDriver(service, options, TimeSpan.FromSeconds(60));

            driver.Manage().Timeouts().PageLoad = TimeSpan.FromSeconds(60);
            driver.Manage().Timeouts().AsynchronousJavaScript = TimeSpan.FromSeconds(30);
            driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(2);

            return driver;
        }

        private static string LoadPageHtml(IWebDriver driver, string url, bool debug = false)
        {
            driver.Navigate().GoToUrl(url);

            WaitForDocumentReady(driver, TimeSpan.FromSeconds(30));

            Thread.Sleep(1500);

            var html = driver.PageSource ?? string.Empty;

            if (debug)
            {
                Console.WriteLine($"ELibrary loaded url: {url}");
                Console.WriteLine($"ELibrary loaded html length: {html.Length}");
                Console.WriteLine($"Current url: {driver.Url}");
                Console.WriteLine($"Title: {driver.Title}");
                Console.WriteLine($"Contains item bigtext: {html.Contains("bigtext", StringComparison.OrdinalIgnoreCase)}");
                Console.WriteLine($"Contains abstract1: {html.Contains("abstract1", StringComparison.OrdinalIgnoreCase)}");
                Console.WriteLine($"Contains abstract2: {html.Contains("abstract2", StringComparison.OrdinalIgnoreCase)}");
                Console.WriteLine($"Contains item.asp: {html.Contains("item.asp", StringComparison.OrdinalIgnoreCase)}");
                Console.WriteLine($"Contains доступ к данной странице: {html.Contains("доступ к данной странице", StringComparison.OrdinalIgnoreCase)}");
                Console.WriteLine($"Contains недостаточно прав: {html.Contains("недостаточно прав", StringComparison.OrdinalIgnoreCase)}");
                Console.WriteLine($"Contains закончилась текущая сессия: {html.Contains("закончилась текущая сессия", StringComparison.OrdinalIgnoreCase)}");
                Console.WriteLine($"Contains captcha: {html.Contains("captcha", StringComparison.OrdinalIgnoreCase)}");
                Console.WriteLine($"Contains робот: {html.Contains("робот", StringComparison.OrdinalIgnoreCase)}");
            }

            if (string.IsNullOrWhiteSpace(html))
            {
                return string.Empty;
            }

            EnsureNotBlocked(html, url);

            return html;
        }

        private static void WaitForDocumentReady(IWebDriver driver, TimeSpan timeout)
        {
            var wait = new WebDriverWait(driver, timeout);

            wait.Until(d =>
            {
                try
                {
                    var js = (IJavaScriptExecutor)d;
                    var readyState = js.ExecuteScript("return document.readyState")?.ToString();
                    return readyState == "complete" || readyState == "interactive";
                }
                catch
                {
                    return false;
                }
            });
        }

        private HashSet<string> LoadPublicationIdsByCategory(IWebDriver driver, string authorId, string categoryQueryName)
        {
            var result = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            var baseUrl =
                $"https://www.elibrary.ru/author_items.asp?authorid={authorId}" +
                $"&show_refs=1" +
                $"&hide_doubles=1";

            driver.Navigate().GoToUrl(baseUrl);
            WaitForDocumentReady(driver, TimeSpan.FromSeconds(30));
            Thread.Sleep(1500);

            EnsureNotBlocked(driver.PageSource ?? string.Empty, baseUrl);

            IWebElement? checkbox = null;

            try
            {
                checkbox = driver.FindElement(By.Name(categoryQueryName));
            }
            catch
            {
                Console.WriteLine($"Чекбокс категории {categoryQueryName} не найден на странице автора.");
                return result;
            }

            try
            {
                ((IJavaScriptExecutor)driver).ExecuteScript(
                    "arguments[0].scrollIntoView({block: 'center'});",
                    checkbox);

                Thread.Sleep(300);

                if (!checkbox.Selected)
                {
                    ((IJavaScriptExecutor)driver).ExecuteScript("arguments[0].click();", checkbox);
                }

                Thread.Sleep(500);

                var form = (IWebElement?)((IJavaScriptExecutor)driver).ExecuteScript(
                    "return arguments[0].closest('form');",
                    checkbox);

                if (form != null)
                {
                    ((IJavaScriptExecutor)driver).ExecuteScript("arguments[0].submit();", form);
                }
                else
                {
                    checkbox.SendKeys(Keys.Enter);
                }

                WaitForDocumentReady(driver, TimeSpan.FromSeconds(30));
                Thread.Sleep(2000);

                var html = driver.PageSource ?? string.Empty;

                Console.WriteLine($"ELibrary category by click {categoryQueryName}. Url: {driver.Url}");
                Console.WriteLine($"ELibrary category by click {categoryQueryName}. Html length: {html.Length}");
                Console.WriteLine($"ELibrary category by click {categoryQueryName}. Title: {driver.Title}");

                EnsureNotBlocked(html, driver.Url);

                var doc = new HtmlDocument();
                doc.LoadHtml(html);

                var publications = ParsePublicationList(doc);

                foreach (var publication in publications)
                {
                    if (!string.IsNullOrWhiteSpace(publication.ELibraryId))
                    {
                        result.Add(publication.ELibraryId);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Не удалось загрузить категорию {categoryQueryName} через Selenium-клик: {ex.Message}");
                throw;
            }

            return result;
        }

        private static Dictionary<string, int> ParseCategoryCounts(HtmlDocument doc)
        {
            var result = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);

            foreach (var category in ELibraryCategoryQueryNames)
            {
                var inputName = category.Value;

                var inputNode = doc.DocumentNode.SelectSingleNode($"//input[@name='{inputName}']");
                if (inputNode == null)
                {
                    result[category.Key] = 0;
                    continue;
                }

                var row = inputNode;
                while (row != null && !row.Name.Equals("tr", StringComparison.OrdinalIgnoreCase))
                {
                    row = row.ParentNode;
                }

                if (row == null)
                {
                    result[category.Key] = 0;
                    continue;
                }

                var rowText = CleanText(row.InnerText);
                var countMatch = Regex.Match(rowText, @"\((\d+)\)");

                result[category.Key] = countMatch.Success && int.TryParse(countMatch.Groups[1].Value, out var count)
                    ? count
                    : 0;
            }

            return result;
        }

        private static void EnsureNotBlocked(string html, string? url = null)
        {
            var normalized = NormalizeTextForSearch(html);

            var hardBlockMarkers = new[]
            {
                "403 - forbidden",
                "access is denied",
                "доступ к данной странице для незарегистрированных пользователей ограничен",
                "доступ к сайту временно ограничен",
                "недостаточно прав для открытия страницы",
                "закончилась текущая сессия",
                "для доступа к этой странице необходимо авторизоваться",
                "введите код с картинки",
                "подтвердите, что вы не робот",
                "проверка пользователя"
            };

            foreach (var marker in hardBlockMarkers)
            {
                if (normalized.Contains(marker, StringComparison.OrdinalIgnoreCase))
                {
                    throw new Exception($"eLibrary не вернул нужную страницу. Маркер блокировки: '{marker}'. Url: {url}");
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

        private static List<ELibraryPublicationImportModel> ParsePublicationList(HtmlDocument doc)
        {
            var result = new List<ELibraryPublicationImportModel>();

            var rows = doc.DocumentNode.SelectNodes("//table[@id='restab']//tr[starts-with(@id,'arw') or starts-with(@id,'brw')]");
            if (rows == null)
            {
                return result;
            }

            foreach (var row in rows)
            {
                var publication = ParsePublicationListRow(row);
                if (publication != null && !string.IsNullOrWhiteSpace(publication.Title))
                {
                    result.Add(publication);
                }
            }

            return result;
        }

        private static ELibraryPublicationImportModel? ParsePublicationListRow(HtmlNode row)
        {
            var idAttribute = row.GetAttributeValue("id", string.Empty);
            var idMatch = Regex.Match(idAttribute, @"\d+");
            if (!idMatch.Success)
            {
                return null;
            }

            var publication = new ELibraryPublicationImportModel
            {
                ELibraryId = idMatch.Value,
                Url = $"https://www.elibrary.ru/item.asp?id={idMatch.Value}"
            };

            var titleLink = row.SelectSingleNode(".//a[contains(@href,'item.asp?id=')]");
            if (titleLink != null)
            {
                publication.Title = CleanText(titleLink.InnerText);
            }
            else
            {
                var boldNode = row.SelectSingleNode(".//td[2]//b");
                publication.Title = CleanText(boldNode?.InnerText);
            }

            var authorsNode = row.SelectSingleNode(".//td[2]//i");
            publication.Authors = CleanText(authorsNode?.InnerText);

            var publicationTextNode = row.SelectSingleNode(".//td[2]");
            var publicationText = CleanText(publicationTextNode?.InnerText);

            publication.Year = ExtractPublicationYear(publicationText);

            var journalLink = row.SelectSingleNode(".//td[2]//a[contains(@href,'contents.asp')]");
            if (journalLink != null)
            {
                publication.JournalTitle = CleanText(journalLink.InnerText);
            }

            var citationCell = row.SelectSingleNode("./td[3]");
            publication.CitationsRincCount = ExtractFirstInt(CleanText(citationCell?.InnerText));

            return publication;
        }

        private static void EnrichPublicationFromDetails(HtmlDocument doc, ELibraryPublicationImportModel publication)
        {
            var wholeText = CleanText(doc.DocumentNode.InnerText);

            publication.Doi = ExtractByRegex(wholeText, @"DOI:\s*([^\s]+)");

            var titleNode = doc.DocumentNode.SelectSingleNode("//p[contains(@class,'bigtext')]");
            if (titleNode != null)
            {
                publication.Title = CleanText(titleNode.InnerText);
            }

            var yearMatch = Regex.Match(wholeText, @"Год:\s*(\d{4})", RegexOptions.IgnoreCase);
            if (yearMatch.Success && int.TryParse(yearMatch.Groups[1].Value, out var year))
            {
                publication.Year = year;
            }

            var journalHeader = FindSectionTable(doc, "ЖУРНАЛ:");
            if (journalHeader != null)
            {
                var journalLink = journalHeader.SelectSingleNode(".//a[contains(@href,'contents.asp')]");
                if (journalLink != null)
                {
                    publication.JournalTitle = ToTitleCaseRu(CleanText(journalLink.InnerText));
                }

                var journalText = CleanText(journalHeader.InnerText);
                var issnMatch = Regex.Match(journalText, @"ISSN:\s*([\dXХx\-]+)", RegexOptions.IgnoreCase);
                if (issnMatch.Success)
                {
                    publication.JournalIssn = NormalizeIssn(issnMatch.Groups[1].Value);
                }
            }

            var keywordsTable = FindSectionTable(doc, "КЛЮЧЕВЫЕ СЛОВА:");
            if (keywordsTable != null)
            {
                var keywords = keywordsTable
                    .SelectNodes(".//a[contains(@href,'keyword_items.asp')]")
                    ?.Select(x => CleanText(x.InnerText))
                    .Where(x => !string.IsNullOrWhiteSpace(x))
                    .Distinct(StringComparer.OrdinalIgnoreCase)
                    .ToList();

                if (keywords != null && keywords.Count > 0)
                {
                    publication.Keywords = string.Join(", ", keywords);
                }
            }

            var abstractFull = doc.DocumentNode.SelectSingleNode("//div[@id='abstract2']");
            var abstractShort = doc.DocumentNode.SelectSingleNode("//div[@id='abstract1']");
            publication.Annotation = CleanText(abstractFull?.InnerText);
            if (string.IsNullOrWhiteSpace(publication.Annotation))
            {
                publication.Annotation = CleanText(abstractShort?.InnerText);
            }

            publication.IsInRinc = ExtractBibliometricYesNo(doc, "Входит в РИНЦ");
            publication.IsInCoreRinc = ExtractBibliometricYesNo(doc, "Входит в ядро РИНЦ");

            var citationsMatch = Regex.Match(wholeText, @"Цитирований в РИНЦ:\s*(\d+)", RegexOptions.IgnoreCase);
            if (citationsMatch.Success && int.TryParse(citationsMatch.Groups[1].Value, out var citations))
            {
                publication.CitationsRincCount = citations;
            }

            publication.RubricOecd = ExtractSpanValue(doc, "rubric_oecd");
            publication.RubricAsjc = ExtractSpanValue(doc, "rubric_asjc");
            publication.RubricGrnti = ExtractSpanValue(doc, "rubric_grnti");
            publication.VakSpecialty = ExtractSpanValue(doc, "rubric_vak");

            publication.IsVak = !string.IsNullOrWhiteSpace(publication.VakSpecialty)
                                && !publication.VakSpecialty.Equals("нет", StringComparison.OrdinalIgnoreCase);
        }

        private static HtmlNode? FindSectionTable(HtmlDocument doc, string sectionTitle)
        {
            var titleNode = doc.DocumentNode.SelectSingleNode(
                $"//*[contains(normalize-space(.), '{sectionTitle}')]");

            if (titleNode == null)
            {
                return null;
            }

            var current = titleNode;
            while (current != null && !current.Name.Equals("table", StringComparison.OrdinalIgnoreCase))
            {
                current = current.ParentNode;
            }

            return current;
        }

        private static string? ExtractSpanValue(HtmlDocument doc, string spanId)
        {
            var node = doc.DocumentNode.SelectSingleNode($"//span[@id='{spanId}']");
            var value = CleanText(node?.InnerText);

            if (string.IsNullOrWhiteSpace(value) || value.Equals("нет", StringComparison.OrdinalIgnoreCase))
            {
                return null;
            }

            return value;
        }

        private static bool ExtractBibliometricYesNo(HtmlDocument doc, string label)
        {
            var rows = doc.DocumentNode.SelectNodes("//tr");
            if (rows == null)
            {
                return false;
            }

            foreach (var row in rows)
            {
                var rowText = NormalizeTextForSearch(row.InnerText);

                if (!rowText.Contains(label, StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                var pattern = Regex.Escape(label) + @"\s*:\s*(да|нет)";
                var match = Regex.Match(rowText, pattern, RegexOptions.IgnoreCase);

                if (match.Success)
                {
                    return match.Groups[1].Value.Equals("да", StringComparison.OrdinalIgnoreCase);
                }
            }

            return false;
        }

        private static string NormalizeTextForSearch(string? text)
        {
            if (string.IsNullOrWhiteSpace(text))
            {
                return string.Empty;
            }

            var result = HtmlEntity.DeEntitize(text);
            result = result.Replace('\u00A0', ' ');
            result = result.Replace("\r", " ");
            result = result.Replace("\n", " ");
            result = result.Replace("\t", " ");
            result = Regex.Replace(result, @"\s+", " ");
            return result.Trim();
        }

        private static string? ExtractByRegex(string text, string pattern)
        {
            var match = Regex.Match(text, pattern, RegexOptions.IgnoreCase);
            return match.Success ? match.Groups[1].Value.Trim() : null;
        }

        private static int? ExtractPublicationYear(string text)
        {
            var matches = Regex.Matches(text, @"\b(19|20)\d{2}\b");
            if (matches.Count == 0)
            {
                return null;
            }

            return matches
                .Select(x => int.TryParse(x.Value, out var year) ? year : 0)
                .Where(x => x > 0)
                .OrderByDescending(x => x)
                .FirstOrDefault();
        }

        private static string? NormalizeIssn(string? issn)
        {
            if (string.IsNullOrWhiteSpace(issn))
            {
                return null;
            }

            return issn.Trim().ToUpperInvariant().Replace('Х', 'X').Replace('х', 'X');
        }
    }
}