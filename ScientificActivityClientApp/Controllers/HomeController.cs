using Microsoft.AspNetCore.Mvc;
using ScientificActivityClientApp.Models;
using ScientificActivityContracts.BindingModels;
using ScientificActivityContracts.ViewModels;
using ScientificActivityDataModels.Enums;
using ScientificActivityParsers.Interfaces;
using System.Diagnostics;
using System.Net.Http.Headers;
using System.Threading;

namespace ScientificActivityClientApp.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        public IActionResult Index()
        {
            return View();
        }

        // -------------------- Авторизация --------------------

        public IActionResult Enter(string? message = null, string? error = null)
        {
            if (!string.IsNullOrWhiteSpace(message))
            {
                ViewBag.Message = message;
            }

            if (!string.IsNullOrWhiteSpace(error))
            {
                ViewBag.Error = error;
            }

            if (TempData["Error"] != null)
            {
                ViewBag.Error = TempData["Error"]?.ToString();
            }

            if (TempData["Message"] != null)
            {
                ViewBag.Message = TempData["Message"]?.ToString();
            }

            return View();
        }

        [HttpPost]
        [ActionName("Enter")]
        public IActionResult EnterPost(string email, string password)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
                {
                    TempData["Error"] = "Введите email и пароль";
                    TempData["Email"] = email;
                    return RedirectToAction("Enter");
                }

                email = email.Trim();

                APIClient.Researcher = APIClient.GetRequest<ResearcherViewModel>(
                    $"api/Auth/Login?email={email}&password={password}");

                if (APIClient.Researcher == null)
                {
                    TempData["Error"] = "Неверный email или пароль";
                    TempData["Email"] = email;
                    return RedirectToAction("Enter");
                }

                TempData["Message"] = "Вход выполнен успешно";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка входа");

                if (ex.Message.Contains("Неверный email или пароль") ||
                    ex.Message.Contains("NotFound") ||
                    ex.Message.Contains("404"))
                {
                    TempData["Error"] = "Неверный email или пароль";
                }
                else
                {
                    TempData["Error"] = $"Ошибка входа: {ex.Message}";
                }

                TempData["Email"] = email;
                return RedirectToAction("Enter");
            }
        }

        [HttpGet]
        public IActionResult Register()
        {
            ViewBag.LastName = "";
            ViewBag.FirstName = "";
            ViewBag.MiddleName = "";
            ViewBag.Email = "";
            ViewBag.Phone = "";
            ViewBag.Department = "";
            ViewBag.Position = "";
            ViewBag.ResearchTopics = "";
            ViewBag.ELibraryAuthorId = "";
            ViewBag.AcademicDegree = 0;

            return View();
        }

        [HttpPost]
        public IActionResult Register(
            string lastName,
            string firstName,
            string? middleName,
            string email,
            string phone,
            string password,
            string department,
            string position,
            int academicDegree,
            string? eLibraryAuthorId,
            string? researchTopics)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(lastName) ||
                    string.IsNullOrWhiteSpace(firstName) ||
                    string.IsNullOrWhiteSpace(email) ||
                    string.IsNullOrWhiteSpace(phone) ||
                    string.IsNullOrWhiteSpace(password) ||
                    string.IsNullOrWhiteSpace(department) ||
                    string.IsNullOrWhiteSpace(position))
                {
                    ViewBag.Error = "Заполните все обязательные поля";
                    FillRegisterViewBag(lastName, firstName, middleName, email, phone, department, position, researchTopics, eLibraryAuthorId, academicDegree);
                    return View();
                }

                var model = new ResearcherBindingModel
                {
                    LastName = lastName,
                    FirstName = firstName,
                    MiddleName = string.IsNullOrWhiteSpace(middleName) ? null : middleName,
                    Email = email,
                    Phone = phone,
                    PasswordHash = password,
                    Department = department,
                    Position = position,
                    AcademicDegree = (AcademicDegree)academicDegree,
                    ELibraryAuthorId = string.IsNullOrWhiteSpace(eLibraryAuthorId) ? null : eLibraryAuthorId,
                    ResearchTopics = string.IsNullOrWhiteSpace(researchTopics) ? null : researchTopics,
                    Role = UserRole.Исследователь,
                    IsActive = true
                };

                APIClient.PostRequest("api/Auth/Register", model);

                return RedirectToAction("Enter", new { message = "Регистрация прошла успешно. Теперь войдите в систему." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка регистрации");
                ViewBag.Error = ex.Message;
                FillRegisterViewBag(lastName, firstName, middleName, email, phone, department, position, researchTopics, eLibraryAuthorId, academicDegree);
                return View();
            }
        }

        public IActionResult Logout()
        {
            APIClient.Researcher = null;
            TempData["Message"] = "Вы вышли из системы";
            return RedirectToAction("Index");
        }

        // -------------------- Профиль --------------------

        private void FillELibraryProfileViewBag(string? authorId)
        {
            ViewBag.ELibraryProfile = null;

            if (string.IsNullOrWhiteSpace(authorId))
            {
                return;
            }

            try
            {
                var profile = APIClient.GetRequest<ELibraryAuthorProfileViewModel>(
                    $"api/ELibrary/GetAuthorProfile?authorId={authorId}");

                ViewBag.ELibraryProfile = profile;
            }
            catch
            {
                ViewBag.ELibraryProfile = null;
            }
        }

        public IActionResult Profile()
        {
            if (APIClient.Researcher == null)
            {
                return RedirectToAction("Enter", new { error = "Требуется авторизация" });
            }

            UpdateResearcherProfile();
            FillELibraryProfileViewBag(APIClient.Researcher.ELibraryAuthorId);

            return View(APIClient.Researcher);
        }

        [HttpPost]
        public IActionResult Profile(
            string lastName,
            string firstName,
            string? middleName,
            string email,
            string phone,
            string department,
            string position,
            int academicDegree,
            string? eLibraryAuthorId,
            string? researchTopics,
            string? newPassword)
        {
            try
            {
                if (APIClient.Researcher == null)
                {
                    return RedirectToAction("Enter");
                }

                if (string.IsNullOrWhiteSpace(lastName) ||
                    string.IsNullOrWhiteSpace(firstName) ||
                    string.IsNullOrWhiteSpace(email) ||
                    string.IsNullOrWhiteSpace(phone) ||
                    string.IsNullOrWhiteSpace(department) ||
                    string.IsNullOrWhiteSpace(position))
                {
                    ViewBag.Error = "Заполните обязательные поля";
                    return View(APIClient.Researcher);
                }

                var updateModel = new ResearcherBindingModel
                {
                    Id = APIClient.Researcher.Id,
                    LastName = lastName,
                    FirstName = firstName,
                    MiddleName = string.IsNullOrWhiteSpace(middleName) ? null : middleName,
                    Email = email,
                    Phone = phone,
                    PasswordHash = string.IsNullOrWhiteSpace(newPassword) ? string.Empty : newPassword,
                    Department = department,
                    Position = position,
                    AcademicDegree = (AcademicDegree)academicDegree,
                    ELibraryAuthorId = string.IsNullOrWhiteSpace(eLibraryAuthorId) ? null : eLibraryAuthorId,
                    ResearchTopics = string.IsNullOrWhiteSpace(researchTopics) ? null : researchTopics,
                    Role = APIClient.Researcher.Role,
                    IsActive = APIClient.Researcher.IsActive
                };

                APIClient.PostRequest("api/Researcher/UpdateResearcher", updateModel);

                APIClient.Researcher.LastName = lastName;
                APIClient.Researcher.FirstName = firstName;
                APIClient.Researcher.MiddleName = middleName;
                APIClient.Researcher.Email = email;
                APIClient.Researcher.Phone = phone;
                APIClient.Researcher.Department = department;
                APIClient.Researcher.Position = position;
                APIClient.Researcher.AcademicDegree = (AcademicDegree)academicDegree;
                APIClient.Researcher.ELibraryAuthorId = eLibraryAuthorId;
                APIClient.Researcher.ResearchTopics = researchTopics;

                ViewBag.Message = "Профиль успешно обновлён";
                FillELibraryProfileViewBag(APIClient.Researcher.ELibraryAuthorId);
                return View(APIClient.Researcher);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка обновления профиля");
                ViewBag.Error = ex.Message;
                FillELibraryProfileViewBag(APIClient.Researcher?.ELibraryAuthorId);
                return View(APIClient.Researcher);
            }
        }

        [HttpPost]
        public IActionResult BindELibraryAuthor(string authorId)
        {
            try
            {
                if (APIClient.Researcher == null)
                {
                    return RedirectToAction("Enter");
                }

                if (string.IsNullOrWhiteSpace(authorId))
                {
                    ViewBag.Error = "Укажите AuthorId eLibrary";
                    FillELibraryProfileViewBag(APIClient.Researcher.ELibraryAuthorId);
                    return View("Profile", APIClient.Researcher);
                }

                APIClient.PostRequest("api/ELibrary/BindAuthorToResearcher", new ELibraryBindAuthorBindingModel
                {
                    ResearcherId = APIClient.Researcher.Id,
                    AuthorId = authorId.Trim()
                });

                APIClient.Researcher.ELibraryAuthorId = authorId.Trim();

                FillELibraryProfileViewBag(APIClient.Researcher.ELibraryAuthorId);
                ViewBag.Message = "AuthorId eLibrary успешно привязан";

                return View("Profile", APIClient.Researcher);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка привязки AuthorId eLibrary");
                ViewBag.Error = ex.Message;
                FillELibraryProfileViewBag(APIClient.Researcher?.ELibraryAuthorId);
                return View("Profile", APIClient.Researcher);
            }
        }

        [HttpPost]
        public IActionResult LoadELibraryProfile(string authorId)
        {
            try
            {
                if (APIClient.Researcher == null)
                {
                    return RedirectToAction("Enter");
                }

                var actualAuthorId = string.IsNullOrWhiteSpace(authorId)
                    ? APIClient.Researcher.ELibraryAuthorId
                    : authorId.Trim();

                if (string.IsNullOrWhiteSpace(actualAuthorId))
                {
                    ViewBag.Error = "Укажите AuthorId eLibrary";
                    return View("Profile", APIClient.Researcher);
                }

                FillELibraryProfileViewBag(actualAuthorId);
                ViewBag.Message = "Профиль eLibrary загружен";

                return View("Profile", APIClient.Researcher);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка загрузки профиля eLibrary");
                ViewBag.Error = ex.Message;
                FillELibraryProfileViewBag(APIClient.Researcher?.ELibraryAuthorId);
                return View("Profile", APIClient.Researcher);
            }
        }

        [HttpPost]
        public IActionResult ImportELibraryProfile()
        {
            try
            {
                if (APIClient.Researcher == null)
                {
                    return RedirectToAction("Enter");
                }

                if (string.IsNullOrWhiteSpace(APIClient.Researcher.ELibraryAuthorId))
                {
                    ViewBag.Error = "Сначала укажите и привяжите AuthorId eLibrary";
                    return View("Profile", APIClient.Researcher);
                }

                APIClient.PostRequest("api/ELibrary/ImportAuthorProfile", new ELibraryImportBindingModel
                {
                    ResearcherId = APIClient.Researcher.Id
                });

                UpdateResearcherProfile();
                FillELibraryProfileViewBag(APIClient.Researcher?.ELibraryAuthorId);

                ViewBag.Message = "Данные из eLibrary импортированы в профиль";

                return View("Profile", APIClient.Researcher);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка импорта профиля eLibrary");
                ViewBag.Error = ex.Message;
                FillELibraryProfileViewBag(APIClient.Researcher?.ELibraryAuthorId);
                return View("Profile", APIClient.Researcher);
            }
        }

        // -------------------- Журналы -----------------------------------

        [HttpGet]
        public IActionResult Journals(string? title, string? issn, bool? isVak, bool? isWhiteList, int page = 1)
        {
            if (APIClient.Researcher == null)
            {
                return RedirectToAction("Enter");
            }

            try
            {
                const int pageSize = 25;

                var allJournals = APIClient.GetRequest<List<JournalViewModel>>("api/Journal/GetAllJournals")
                                 ?? new List<JournalViewModel>();

                IEnumerable<JournalViewModel> query = allJournals;

                if (!string.IsNullOrWhiteSpace(title))
                {
                    query = query.Where(x =>
                        !string.IsNullOrWhiteSpace(x.Title) &&
                        x.Title.Contains(title, StringComparison.OrdinalIgnoreCase));
                }

                if (!string.IsNullOrWhiteSpace(issn))
                {
                    query = query.Where(x =>
                        (!string.IsNullOrWhiteSpace(x.Issn) &&
                         x.Issn.Contains(issn, StringComparison.OrdinalIgnoreCase)) ||
                        (!string.IsNullOrWhiteSpace(x.EIssn) &&
                         x.EIssn.Contains(issn, StringComparison.OrdinalIgnoreCase)));
                }

                if (isVak.HasValue)
                {
                    query = query.Where(x => x.IsVak == isVak.Value);
                }

                if (isWhiteList.HasValue)
                {
                    query = query.Where(x => x.IsWhiteList == isWhiteList.Value);
                }

                var filteredJournals = query
                    .OrderBy(x => x.Title)
                    .ToList();

                var totalCount = filteredJournals.Count;
                var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

                if (totalPages == 0)
                {
                    totalPages = 1;
                }

                if (page < 1)
                {
                    page = 1;
                }

                if (page > totalPages)
                {
                    page = totalPages;
                }

                var journals = filteredJournals
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .ToList();

                ViewBag.CurrentPage = page;
                ViewBag.TotalPages = totalPages;
                ViewBag.TotalCount = totalCount;

                ViewBag.TitleFilter = title;
                ViewBag.IssnFilter = issn;
                ViewBag.IsVakFilter = isVak;
                ViewBag.IsWhiteListFilter = isWhiteList;

                return View(journals);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка получения журналов");
                TempData["Error"] = ex.Message;
                return View(new List<JournalViewModel>());
            }
        }


        [HttpGet]
        public IActionResult CreateJournal()
        {
            if (APIClient.Researcher == null || APIClient.Researcher.Role != UserRole.Администратор)
            {
                return RedirectToAction("Journals");
            }

            return View();
        }

        [HttpPost]
        public IActionResult CreateJournal(string title, string? issn, string? eIssn, string? publisher,
            string? subjectArea, bool isVak, bool isWhiteList, string? country, string? url)
        {
            try
            {
                if (APIClient.Researcher == null || APIClient.Researcher.Role != UserRole.Администратор)
                {
                    return RedirectToAction("Journals");
                }

                APIClient.PostRequest("api/Journal/CreateJournal", new JournalBindingModel
                {
                    Title = title,
                    Issn = issn,
                    EIssn = eIssn,
                    Publisher = publisher,
                    SubjectArea = subjectArea,
                    IsVak = isVak,
                    IsWhiteList = isWhiteList,
                    WhiteListLevel2023 = null,
                    WhiteListLevel2025 = null,
                    WhiteListState = null,
                    WhiteListNotice = null,
                    WhiteListAcceptedDate = null,
                    WhiteListDiscontinuedDate = null,
                    Country = country,
                    Url = url
                });

                TempData["Message"] = "Журнал добавлен";
                return RedirectToAction("Journals");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка создания журнала");
                ViewBag.Error = ex.Message;
                return View();
            }
        }

        [HttpGet]
        public IActionResult UpdateJournal(int id)
        {
            if (APIClient.Researcher == null || APIClient.Researcher.Role != UserRole.Администратор)
            {
                return RedirectToAction("Journals");
            }

            try
            {
                var journal = APIClient.GetRequest<JournalViewModel>($"api/Journal/GetJournalById?id={id}");
                if (journal == null)
                {
                    TempData["Error"] = "Журнал не найден";
                    return RedirectToAction("Journals");
                }

                ViewBag.JournalId = journal.Id;
                ViewBag.TitleValue = journal.Title;
                ViewBag.IssnValue = journal.Issn;
                ViewBag.EIssnValue = journal.EIssn;
                ViewBag.PublisherValue = journal.Publisher;
                ViewBag.SubjectAreaValue = journal.SubjectArea;
                ViewBag.IsVakValue = journal.IsVak;
                ViewBag.IsWhiteListValue = journal.IsWhiteList;
                ViewBag.WhiteListLevel2023Value = journal.WhiteListLevel2023;
                ViewBag.WhiteListLevel2025Value = journal.WhiteListLevel2025;
                ViewBag.WhiteListStateValue = journal.WhiteListState;
                ViewBag.WhiteListNoticeValue = journal.WhiteListNotice;
                ViewBag.WhiteListAcceptedDateValue = journal.WhiteListAcceptedDate;
                ViewBag.WhiteListDiscontinuedDateValue = journal.WhiteListDiscontinuedDate;
                ViewBag.CountryValue = journal.Country;
                ViewBag.UrlValue = journal.Url;

                return View();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка получения журнала для редактирования");
                TempData["Error"] = ex.Message;
                return RedirectToAction("Journals");
            }
        }

        [HttpPost]
        public IActionResult UpdateJournal(int id, string title, string? issn, string? eIssn, string? publisher,
            string? subjectArea, bool isVak, bool isWhiteList, int? whiteListLevel2023, int? whiteListLevel2025,
            string? whiteListState, string? whiteListNotice, DateTime? whiteListAcceptedDate, 
            DateTime? whiteListDiscontinuedDate, string? country, string? url)
        {
            try
            {
                if (APIClient.Researcher == null || APIClient.Researcher.Role != UserRole.Администратор)
                {
                    return RedirectToAction("Journals");
                }

                APIClient.PostRequest("api/Journal/UpdateJournal", new JournalBindingModel
                {
                    Id = id,
                    Title = title,
                    Issn = issn,
                    EIssn = eIssn,
                    Publisher = publisher,
                    SubjectArea = subjectArea,
                    IsVak = isVak,
                    IsWhiteList = isWhiteList,
                    WhiteListLevel2023 = whiteListLevel2023,
                    WhiteListLevel2025 = whiteListLevel2025,
                    WhiteListState = whiteListState,
                    WhiteListNotice = whiteListNotice,
                    WhiteListAcceptedDate = whiteListAcceptedDate,
                    WhiteListDiscontinuedDate = whiteListDiscontinuedDate,
                    Country = country,
                    Url = url
                });

                TempData["Message"] = "Журнал обновлён";
                return RedirectToAction("Journals");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка обновления журнала");
                ViewBag.Error = ex.Message;
                ViewBag.JournalId = id;
                ViewBag.TitleValue = title;
                ViewBag.IssnValue = issn;
                ViewBag.EIssnValue = eIssn;
                ViewBag.PublisherValue = publisher;
                ViewBag.SubjectAreaValue = subjectArea;
                ViewBag.IsVakValue = isVak;
                ViewBag.IsWhiteListValue = isWhiteList;
                ViewBag.WhiteListLevel2023Value = whiteListLevel2023;
                ViewBag.WhiteListLevel2025Value = whiteListLevel2025;
                ViewBag.WhiteListStateValue = whiteListState;
                ViewBag.WhiteListNoticeValue = whiteListNotice;
                ViewBag.WhiteListAcceptedDateValue = whiteListAcceptedDate;
                ViewBag.WhiteListDiscontinuedDateValue = whiteListDiscontinuedDate;
                ViewBag.CountryValue = country;
                ViewBag.UrlValue = url;
                return View();
            }
        }

        [HttpPost]
        public IActionResult DeleteJournal(int id)
        {
            try
            {
                if (APIClient.Researcher == null || APIClient.Researcher.Role != UserRole.Администратор)
                {
                    return RedirectToAction("Journals");
                }

                APIClient.PostRequest("api/Journal/DeleteJournal", new JournalBindingModel
                {
                    Id = id
                });

                TempData["Message"] = "Журнал удалён";
                return RedirectToAction("Journals");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка удаления журнала");
                TempData["Error"] = ex.Message;
                return RedirectToAction("Journals");
            }
        }

        [HttpGet]
        public IActionResult ImportAllJournals()
        {
            if (APIClient.Researcher == null || APIClient.Researcher.Role != UserRole.Администратор)
            {
                TempData["Error"] = "Импорт доступен только администратору";
                return RedirectToAction("Journals");
            }

            return View();
        }

        [HttpPost]
        public async Task<IActionResult> ImportAllJournals(IFormFile file)
        {
            if (APIClient.Researcher == null || APIClient.Researcher.Role != UserRole.Администратор)
            {
                TempData["Error"] = "Импорт доступен только администратору";
                return RedirectToAction("Journals");
            }

            if (file == null || file.Length == 0)
            {
                ViewBag.Error = "Выберите PDF-файл ВАК";
                return View();
            }

            if (!Path.GetExtension(file.FileName).Equals(".pdf", StringComparison.OrdinalIgnoreCase))
            {
                ViewBag.Error = "Можно загрузить только PDF-файл";
                return View();
            }

            try
            {
                using var form = new MultipartFormDataContent();
                await using var fileStream = file.OpenReadStream();
                using var streamContent = new StreamContent(fileStream);

                streamContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/pdf");
                form.Add(streamContent, "file", file.FileName);

                var url = $"{APIClient.ApiAddress}/api/Import/ImportAllJournals";
                using var httpClient = new HttpClient
                {
                    Timeout = Timeout.InfiniteTimeSpan
                };

                var response = await httpClient.PostAsync(url, form);
                var responseText = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    ViewBag.Error = $"Ошибка API ({(int)response.StatusCode}): {responseText}";
                    return View();
                }

                TempData["Message"] = string.IsNullOrWhiteSpace(responseText)
                    ? "Импорт журналов успешно завершён"
                    : responseText;

                return RedirectToAction("Journals");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка общего импорта журналов");
                ViewBag.Error = ex.ToString();
                return View();
            }
        }

        [HttpGet]
        public IActionResult ImportVakJournals()
        {
            if (APIClient.Researcher == null || APIClient.Researcher.Role != UserRole.Администратор)
            {
                TempData["Error"] = "Импорт доступен только администратору";
                return RedirectToAction("Journals");
            }

            return View();
        }

        [HttpPost]
        public async Task<IActionResult> ImportVakJournals(IFormFile file)
        {
            if (APIClient.Researcher == null || APIClient.Researcher.Role != UserRole.Администратор)
            {
                TempData["Error"] = "Импорт доступен только администратору";
                return RedirectToAction("Journals");
            }

            if (file == null || file.Length == 0)
            {
                ViewBag.Error = "Выберите PDF-файл ВАК";
                return View();
            }

            if (!Path.GetExtension(file.FileName).Equals(".pdf", StringComparison.OrdinalIgnoreCase))
            {
                ViewBag.Error = "Можно загрузить только PDF-файл";
                return View();
            }

            try
            {
                using var form = new MultipartFormDataContent();
                await using var fileStream = file.OpenReadStream();
                using var streamContent = new StreamContent(fileStream);

                streamContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/pdf");
                form.Add(streamContent, "file", file.FileName);

                var url = $"{APIClient.ApiAddress}/api/Import/ImportVakJournals";
                using var httpClient = new HttpClient
                {
                    Timeout = Timeout.InfiniteTimeSpan
                };

                var response = await httpClient.PostAsync(url, form);
                var responseText = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    ViewBag.Error = $"Ошибка API ({(int)response.StatusCode}): {responseText}";
                    return View();
                }

                TempData["Message"] = string.IsNullOrWhiteSpace(responseText)
                    ? "Импорт ВАК успешно завершён"
                    : responseText;

                return RedirectToAction("Journals");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка импорта ВАК");
                ViewBag.Error = ex.ToString();
                return View();
            }
        }

        [HttpPost]
        public async Task<IActionResult> ImportWhiteListJournals()
        {
            if (APIClient.Researcher == null || APIClient.Researcher.Role != UserRole.Администратор)
            {
                TempData["Error"] = "Операция доступна только администратору";
                return RedirectToAction("Journals");
            }

            try
            {
                var url = $"{APIClient.ApiAddress}/api/Import/ImportWhiteListJournals";
                using var httpClient = new HttpClient
                {
                    Timeout = Timeout.InfiniteTimeSpan
                };

                var response = await httpClient.PostAsync(url, null);
                var responseText = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    TempData["Error"] = $"Ошибка API ({(int)response.StatusCode}): {responseText}";
                    return RedirectToAction("Journals");
                }

                TempData["Message"] = string.IsNullOrWhiteSpace(responseText)
                    ? "Импорт Белого списка успешно завершён"
                    : responseText;

                return RedirectToAction("Journals");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка импорта Белого списка");
                TempData["Error"] = ex.Message;
                return RedirectToAction("Journals");
            }
        }

        [HttpPost]
        public async Task<IActionResult> EnrichWhiteListRcsiLinks()
        {
            if (APIClient.Researcher == null || APIClient.Researcher.Role != UserRole.Администратор)
            {
                TempData["Error"] = "Операция доступна только администратору";
                return RedirectToAction("Journals");
            }

            try
            {
                var url = $"{APIClient.ApiAddress}/api/Import/EnrichWhiteListRcsiLinks";
                using var httpClient = new HttpClient
                {
                    Timeout = Timeout.InfiniteTimeSpan
                };

                var response = await httpClient.PostAsync(url, null);
                var responseText = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    TempData["Error"] = $"Ошибка API ({(int)response.StatusCode}): {responseText}";
                    return RedirectToAction("Journals");
                }

                TempData["Message"] = string.IsNullOrWhiteSpace(responseText)
                    ? "RcsiRecordSourceId и Url успешно обновлены"
                    : responseText;

                return RedirectToAction("Journals");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка заполнения RcsiRecordSourceId и Url");
                TempData["Error"] = ex.Message;
                return RedirectToAction("Journals");
            }
        }

        [HttpPost]
        public async Task<IActionResult> EnrichWhiteListSubjectAreas()
        {
            if (APIClient.Researcher == null || APIClient.Researcher.Role != UserRole.Администратор)
            {
                TempData["Error"] = "Операция доступна только администратору";
                return RedirectToAction("Journals");
            }

            try
            {
                var url = $"{APIClient.ApiAddress}/api/Import/EnrichWhiteListSubjectAreas";
                using var httpClient = new HttpClient
                {
                    Timeout = Timeout.InfiniteTimeSpan
                };

                var response = await httpClient.PostAsync(url, null);
                var responseText = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    TempData["Error"] = $"Ошибка API ({(int)response.StatusCode}): {responseText}";
                    return RedirectToAction("Journals");
                }

                TempData["Message"] = string.IsNullOrWhiteSpace(responseText)
                    ? "Тематики успешно обновлены"
                    : responseText;

                return RedirectToAction("Journals");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка заполнения тематик из РЦНИ");
                TempData["Error"] = ex.Message;
                return RedirectToAction("Journals");
            }
        }



        // -------------------- Конференции -------------------------------

        public IActionResult Conferences(int page = 1)
        {
            if (APIClient.Researcher == null)
            {
                return RedirectToAction("Enter");
            }

            try
            {
                const int pageSize = 10;

                var allConferences = APIClient.GetRequest<List<ConferenceViewModel>>("api/Conference/GetAllConferences")
                                     ?? new List<ConferenceViewModel>();

                var totalCount = allConferences.Count;
                var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

                if (totalPages == 0)
                {
                    totalPages = 1;
                }

                if (page < 1)
                {
                    page = 1;
                }

                if (page > totalPages)
                {
                    page = totalPages;
                }

                var conferences = allConferences
                    .OrderByDescending(x => x.StartDate)
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .ToList();

                ViewBag.CurrentPage = page;
                ViewBag.TotalPages = totalPages;
                ViewBag.TotalCount = totalCount;

                return View(conferences);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка получения конференций");
                TempData["Error"] = ex.Message;
                return View(new List<ConferenceViewModel>());
            }
        }

        [HttpGet]
        public IActionResult CreateConference()
        {
            if (APIClient.Researcher == null || APIClient.Researcher.Role != UserRole.Администратор)
            {
                return RedirectToAction("Conferences");
            }

            return View();
        }

        [HttpPost]
        public IActionResult CreateConference(string title, string? description, DateTime startDate, DateTime endDate,
            string? city, string? country, string? organizer, string? subjectArea, int format, int level, string? url)
        {
            try
            {
                if (APIClient.Researcher == null || APIClient.Researcher.Role != UserRole.Администратор)
                {
                    return RedirectToAction("Conferences");
                }

                APIClient.PostRequest("api/Conference/CreateConference", new ConferenceBindingModel
                {
                    Title = title,
                    Description = description,
                    StartDate = startDate,
                    EndDate = endDate,
                    City = city,
                    Country = country,
                    Organizer = organizer,
                    SubjectArea = subjectArea,
                    Format = (ConferenceFormat)format,
                    Level = (ConferenceLevel)level,
                    Url = url
                });

                TempData["Message"] = "Конференция добавлена";
                return RedirectToAction("Conferences");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка создания конференции");
                ViewBag.Error = ex.Message;
                return View();
            }
        }

        [HttpGet]
        public IActionResult UpdateConference(int id)
        {
            if (APIClient.Researcher == null || APIClient.Researcher.Role != UserRole.Администратор)
            {
                return RedirectToAction("Conferences");
            }

            try
            {
                var conference = APIClient.GetRequest<ConferenceViewModel>($"api/Conference/GetConferenceById?id={id}");
                if (conference == null)
                {
                    TempData["Error"] = "Конференция не найдена";
                    return RedirectToAction("Conferences");
                }

                ViewBag.ConferenceId = conference.Id;
                ViewBag.TitleValue = conference.Title;
                ViewBag.DescriptionValue = conference.Description;
                ViewBag.StartDateValue = conference.StartDate.ToString("yyyy-MM-dd");
                ViewBag.EndDateValue = conference.EndDate.ToString("yyyy-MM-dd");
                ViewBag.CityValue = conference.City;
                ViewBag.CountryValue = conference.Country;
                ViewBag.OrganizerValue = conference.Organizer;
                ViewBag.SubjectAreaValue = conference.SubjectArea;
                ViewBag.FormatValue = (int)conference.Format;
                ViewBag.LevelValue = (int)conference.Level;
                ViewBag.UrlValue = conference.Url;

                return View();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка получения конференции для редактирования");
                TempData["Error"] = ex.Message;
                return RedirectToAction("Conferences");
            }
        }

        [HttpPost]
        public IActionResult UpdateConference(int id, string title, string? description, DateTime startDate, DateTime endDate,
            string? city, string? country, string? organizer, string? subjectArea, int format, int level, string? url)
        {
            try
            {
                if (APIClient.Researcher == null || APIClient.Researcher.Role != UserRole.Администратор)
                {
                    return RedirectToAction("Conferences");
                }

                APIClient.PostRequest("api/Conference/UpdateConference", new ConferenceBindingModel
                {
                    Id = id,
                    Title = title,
                    Description = description,
                    StartDate = startDate,
                    EndDate = endDate,
                    City = city,
                    Country = country,
                    Organizer = organizer,
                    SubjectArea = subjectArea,
                    Format = (ConferenceFormat)format,
                    Level = (ConferenceLevel)level,
                    Url = url
                });

                TempData["Message"] = "Конференция обновлена";
                return RedirectToAction("Conferences");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка обновления конференции");
                ViewBag.Error = ex.Message;
                ViewBag.ConferenceId = id;
                ViewBag.TitleValue = title;
                ViewBag.DescriptionValue = description;
                ViewBag.StartDateValue = startDate.ToString("yyyy-MM-dd");
                ViewBag.EndDateValue = endDate.ToString("yyyy-MM-dd");
                ViewBag.CityValue = city;
                ViewBag.CountryValue = country;
                ViewBag.OrganizerValue = organizer;
                ViewBag.SubjectAreaValue = subjectArea;
                ViewBag.FormatValue = format;
                ViewBag.LevelValue = level;
                ViewBag.UrlValue = url;
                return View();
            }
        }

        [HttpPost]
        public IActionResult DeleteConference(int id)
        {
            try
            {
                if (APIClient.Researcher == null || APIClient.Researcher.Role != UserRole.Администратор)
                {
                    return RedirectToAction("Conferences");
                }

                APIClient.PostRequest("api/Conference/DeleteConference", new ConferenceBindingModel
                {
                    Id = id
                });

                TempData["Message"] = "Конференция удалена";
                return RedirectToAction("Conferences");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка удаления конференции");
                TempData["Error"] = ex.Message;
                return RedirectToAction("Conferences");
            }
        }

        [HttpPost]
        public IActionResult ImportConferences()
        {
            try
            {
                if (APIClient.Researcher == null || APIClient.Researcher.Role != UserRole.Администратор)
                {
                    TempData["Error"] = "Импорт доступен только администратору";
                    return RedirectToAction("Conferences");
                }

                APIClient.PostRequest("api/Import/ImportConferences", new { });

                TempData["Message"] = "Импорт конференций успешно запущен";
                return RedirectToAction("Conferences");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка импорта конференций");
                TempData["Error"] = $"Ошибка импорта конференций: {ex.Message}";
                return RedirectToAction("Conferences");
            }
        }

        // -------------------- Гранты ------------------------------------

        public IActionResult Grants(int page = 1, int? status = null)
        {
            if (APIClient.Researcher == null)
            {
                return RedirectToAction("Enter");
            }

            try
            {
                const int pageSize = 10;

                var allGrants = APIClient.GetRequest<List<GrantViewModel>>("api/Grant/GetAllGrants")
                               ?? new List<GrantViewModel>();

                if (status.HasValue)
                {
                    allGrants = allGrants
                        .Where(x => (int)x.Status == status.Value)
                        .ToList();
                }

                var totalCount = allGrants.Count;
                var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

                if (totalPages == 0)
                {
                    totalPages = 1;
                }

                if (page < 1)
                {
                    page = 1;
                }

                if (page > totalPages)
                {
                    page = totalPages;
                }

                var grants = allGrants
                    .OrderByDescending(x => x.StartDate)
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .ToList();

                ViewBag.CurrentPage = page;
                ViewBag.TotalPages = totalPages;
                ViewBag.TotalCount = totalCount;
                ViewBag.SelectedStatus = status;

                return View(grants);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка получения грантов");
                TempData["Error"] = ex.Message;
                return View(new List<GrantViewModel>());
            }
        }

        [HttpGet]
        public IActionResult CreateGrant()
        {
            if (APIClient.Researcher == null || APIClient.Researcher.Role != UserRole.Администратор)
            {
                return RedirectToAction("Grants");
            }

            return View();
        }

        [HttpPost]
        public IActionResult CreateGrant(string title, string? description, string organization,
            DateTime startDate, DateTime endDate, decimal? amount, string? currency,
            string? subjectArea, int status, string? url)
        {
            try
            {
                if (APIClient.Researcher == null || APIClient.Researcher.Role != UserRole.Администратор)
                {
                    return RedirectToAction("Grants");
                }

                APIClient.PostRequest("api/Grant/CreateGrant", new GrantBindingModel
                {
                    Title = title,
                    Description = description,
                    Organization = organization,
                    StartDate = startDate,
                    EndDate = endDate,
                    Amount = amount,
                    Currency = currency,
                    SubjectArea = subjectArea,
                    Status = (GrantStatus)status,
                    Url = url
                });

                TempData["Message"] = "Грант добавлен";
                return RedirectToAction("Grants");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка создания гранта");
                ViewBag.Error = ex.Message;
                return View();
            }
        }

        [HttpGet]
        public IActionResult UpdateGrant(int id)
        {
            if (APIClient.Researcher == null || APIClient.Researcher.Role != UserRole.Администратор)
            {
                return RedirectToAction("Grants");
            }

            try
            {
                var grant = APIClient.GetRequest<GrantViewModel>($"api/Grant/GetGrantById?id={id}");
                if (grant == null)
                {
                    TempData["Error"] = "Грант не найден";
                    return RedirectToAction("Grants");
                }

                ViewBag.GrantId = grant.Id;
                ViewBag.TitleValue = grant.Title;
                ViewBag.DescriptionValue = grant.Description;
                ViewBag.OrganizationValue = grant.Organization;
                ViewBag.StartDateValue = grant.StartDate.ToString("yyyy-MM-dd");
                ViewBag.EndDateValue = grant.EndDate.ToString("yyyy-MM-dd");
                ViewBag.AmountValue = grant.Amount;
                ViewBag.CurrencyValue = grant.Currency;
                ViewBag.SubjectAreaValue = grant.SubjectArea;
                ViewBag.StatusValue = (int)grant.Status;
                ViewBag.UrlValue = grant.Url;

                return View();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка получения гранта для редактирования");
                TempData["Error"] = ex.Message;
                return RedirectToAction("Grants");
            }
        }

        [HttpPost]
        public IActionResult UpdateGrant(int id, string title, string? description, string organization,
            DateTime startDate, DateTime endDate, decimal? amount, string? currency,
            string? subjectArea, int status, string? url)
        {
            try
            {
                if (APIClient.Researcher == null || APIClient.Researcher.Role != UserRole.Администратор)
                {
                    return RedirectToAction("Grants");
                }

                APIClient.PostRequest("api/Grant/UpdateGrant", new GrantBindingModel
                {
                    Id = id,
                    Title = title,
                    Description = description,
                    Organization = organization,
                    StartDate = startDate,
                    EndDate = endDate,
                    Amount = amount,
                    Currency = currency,
                    SubjectArea = subjectArea,
                    Status = (GrantStatus)status,
                    Url = url
                });

                TempData["Message"] = "Грант обновлён";
                return RedirectToAction("Grants");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка обновления гранта");
                ViewBag.Error = ex.Message;
                ViewBag.GrantId = id;
                ViewBag.TitleValue = title;
                ViewBag.DescriptionValue = description;
                ViewBag.OrganizationValue = organization;
                ViewBag.StartDateValue = startDate.ToString("yyyy-MM-dd");
                ViewBag.EndDateValue = endDate.ToString("yyyy-MM-dd");
                ViewBag.AmountValue = amount;
                ViewBag.CurrencyValue = currency;
                ViewBag.SubjectAreaValue = subjectArea;
                ViewBag.StatusValue = status;
                ViewBag.UrlValue = url;
                return View();
            }
        }

        [HttpPost]
        public IActionResult DeleteGrant(int id)
        {
            try
            {
                if (APIClient.Researcher == null || APIClient.Researcher.Role != UserRole.Администратор)
                {
                    return RedirectToAction("Grants");
                }

                APIClient.PostRequest("api/Grant/DeleteGrant", new GrantBindingModel
                {
                    Id = id
                });

                TempData["Message"] = "Грант удалён";
                return RedirectToAction("Grants");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка удаления гранта");
                TempData["Error"] = ex.Message;
                return RedirectToAction("Grants");
            }
        }

        [HttpPost]
        public IActionResult ImportGrants()
        {
            try
            {
                if (APIClient.Researcher == null || APIClient.Researcher.Role != UserRole.Администратор)
                {
                    TempData["Error"] = "Импорт доступен только администратору";
                    return RedirectToAction("Grants");
                }

                var response = APIClient.PostRequestWithResponse<object, ApiMessageResponse>(
                    "api/Import/ImportGrants",
                    new { });

                TempData["Message"] = response?.Message ?? "Импорт грантов завершён";
                return RedirectToAction("Grants");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка импорта грантов");
                TempData["Error"] = $"Ошибка импорта грантов: {ex.Message}";
                return RedirectToAction("Grants");
            }
        }

        // -------------------- Интересы исследователя --------------------

        public IActionResult ResearcherInterests()
        {
            if (APIClient.Researcher == null)
            {
                return RedirectToAction("Enter");
            }

            try
            {
                var interests = APIClient.GetRequest<List<ResearcherInterestViewModel>>(
                    $"api/ResearcherInterest/GetByResearcher?researcherId={APIClient.Researcher.Id}");

                return View(interests ?? new List<ResearcherInterestViewModel>());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка получения интересов исследователя");
                TempData["Error"] = ex.Message;
                return View(new List<ResearcherInterestViewModel>());
            }
        }

        [HttpGet]
        public IActionResult CreateResearcherInterest()
        {
            if (APIClient.Researcher == null)
            {
                return RedirectToAction("Enter");
            }

            return View();
        }

        [HttpPost]
        public IActionResult CreateResearcherInterest(string keyword, decimal weight)
        {
            try
            {
                if (APIClient.Researcher == null)
                {
                    return RedirectToAction("Enter");
                }

                if (string.IsNullOrWhiteSpace(keyword))
                {
                    ViewBag.Error = "Введите ключевое слово";
                    ViewBag.Keyword = keyword;
                    ViewBag.Weight = weight;
                    return View();
                }

                var model = new ResearcherInterestBindingModel
                {
                    ResearcherId = APIClient.Researcher.Id,
                    Keyword = keyword,
                    Weight = weight
                };

                APIClient.PostRequest("api/ResearcherInterest/Create", model);

                TempData["Message"] = "Интерес успешно добавлен";
                return RedirectToAction("ResearcherInterests");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка добавления интереса");
                ViewBag.Error = ex.Message;
                ViewBag.Keyword = keyword;
                ViewBag.Weight = weight;
                return View();
            }
        }

        [HttpPost]
        public IActionResult DeleteResearcherInterest(int id)
        {
            try
            {
                if (APIClient.Researcher == null)
                {
                    return RedirectToAction("Enter");
                }

                APIClient.PostRequest("api/ResearcherInterest/Delete", new ResearcherInterestBindingModel
                {
                    Id = id
                });

                TempData["Message"] = "Интерес удалён";
                return RedirectToAction("ResearcherInterests");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка удаления интереса");
                TempData["Error"] = ex.Message;
                return RedirectToAction("ResearcherInterests");
            }
        }

        [HttpGet]
        public IActionResult UpdateResearcherInterest(int id)
        {
            if (APIClient.Researcher == null)
            {
                return RedirectToAction("Enter");
            }

            try
            {
                var interests = APIClient.GetRequest<List<ResearcherInterestViewModel>>(
                    $"api/ResearcherInterest/GetByResearcher?researcherId={APIClient.Researcher.Id}") ?? new List<ResearcherInterestViewModel>();

                var interest = interests.FirstOrDefault(x => x.Id == id);
                if (interest == null)
                {
                    TempData["Error"] = "Интерес не найден";
                    return RedirectToAction("ResearcherInterests");
                }

                ViewBag.InterestId = interest.Id;
                ViewBag.Keyword = interest.Keyword;
                ViewBag.Weight = interest.Weight;

                return View();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка получения интереса для редактирования");
                TempData["Error"] = ex.Message;
                return RedirectToAction("ResearcherInterests");
            }
        }

        [HttpPost]
        public IActionResult UpdateResearcherInterest(int id, string keyword, decimal weight)
        {
            try
            {
                if (APIClient.Researcher == null)
                {
                    return RedirectToAction("Enter");
                }

                if (string.IsNullOrWhiteSpace(keyword))
                {
                    ViewBag.Error = "Введите ключевое слово";
                    ViewBag.InterestId = id;
                    ViewBag.Keyword = keyword;
                    ViewBag.Weight = weight;
                    return View();
                }

                APIClient.PostRequest("api/ResearcherInterest/Update", new ResearcherInterestBindingModel
                {
                    Id = id,
                    ResearcherId = APIClient.Researcher.Id,
                    Keyword = keyword,
                    Weight = weight
                });

                TempData["Message"] = "Интерес обновлён";
                return RedirectToAction("ResearcherInterests");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка обновления интереса");
                ViewBag.Error = ex.Message;
                ViewBag.InterestId = id;
                ViewBag.Keyword = keyword;
                ViewBag.Weight = weight;
                return View();
            }
        }

        // -------------------- Публикации --------------------

        public IActionResult Publications()
        {
            if (APIClient.Researcher == null)
            {
                return RedirectToAction("Enter");
            }

            try
            {
                var publications = APIClient.GetRequest<List<PublicationViewModel>>(
                    $"api/Publication/GetPublicationsByFilter?researcherId={APIClient.Researcher.Id}");

                return View(publications ?? new List<PublicationViewModel>());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка получения публикаций");
                TempData["Error"] = ex.Message;
                return View(new List<PublicationViewModel>());
            }
        }

        [HttpGet]
        public IActionResult CreatePublication()
        {
            if (APIClient.Researcher == null)
            {
                return RedirectToAction("Enter");
            }

            LoadPublicationDictionaries();
            return View();
        }

        [HttpPost]
        public IActionResult CreatePublication(
            string title,
            string authors,
            int year,
            DateTime? publicationDate,
            int type,
            string? doi,
            string? url,
            int? journalId,
            int? conferenceId,
            string? keywords,
            string? annotation)
        {
            try
            {
                if (APIClient.Researcher == null)
                {
                    return RedirectToAction("Enter");
                }

                if (string.IsNullOrWhiteSpace(title) || string.IsNullOrWhiteSpace(authors))
                {
                    ViewBag.Error = "Заполните обязательные поля";
                    LoadPublicationDictionaries();
                    FillPublicationViewBag(title, authors, year, publicationDate, type, doi, url, journalId, conferenceId, keywords, annotation);
                    return View();
                }

                var model = new PublicationBindingModel
                {
                    Title = title,
                    Authors = authors,
                    Year = year,
                    PublicationDate = publicationDate,
                    Type = (PublicationType)type,
                    Doi = string.IsNullOrWhiteSpace(doi) ? null : doi,
                    Url = string.IsNullOrWhiteSpace(url) ? null : url,
                    JournalId = journalId,
                    ConferenceId = conferenceId,
                    ResearcherId = APIClient.Researcher.Id,
                    Keywords = string.IsNullOrWhiteSpace(keywords) ? null : keywords,
                    Annotation = string.IsNullOrWhiteSpace(annotation) ? null : annotation
                };

                APIClient.PostRequest("api/Publication/CreatePublication", model);

                TempData["Message"] = "Публикация успешно добавлена";
                return RedirectToAction("Publications");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка создания публикации");
                ViewBag.Error = ex.Message;
                LoadPublicationDictionaries();
                FillPublicationViewBag(title, authors, year, publicationDate, type, doi, url, journalId, conferenceId, keywords, annotation);
                return View();
            }
        }

        [HttpPost]
        public IActionResult DeletePublication(int id)
        {
            try
            {
                if (APIClient.Researcher == null)
                {
                    return RedirectToAction("Enter");
                }

                APIClient.PostRequest("api/Publication/DeletePublication", new PublicationBindingModel
                {
                    Id = id
                });

                TempData["Message"] = "Публикация удалена";
                return RedirectToAction("Publications");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка удаления публикации");
                TempData["Error"] = ex.Message;
                return RedirectToAction("Publications");
            }
        }

        [HttpGet]
        public IActionResult UpdatePublication(int id)
        {
            if (APIClient.Researcher == null)
            {
                return RedirectToAction("Enter");
            }

            try
            {
                var publication = APIClient.GetRequest<PublicationViewModel>($"api/Publication/GetPublicationById?id={id}");
                if (publication == null)
                {
                    TempData["Error"] = "Публикация не найдена";
                    return RedirectToAction("Publications");
                }

                LoadPublicationDictionaries();
                FillPublicationViewBag(
                    publication.Title,
                    publication.Authors,
                    publication.Year,
                    publication.PublicationDate,
                    (int)publication.Type,
                    publication.Doi,
                    publication.Url,
                    publication.JournalId,
                    publication.ConferenceId,
                    publication.Keywords,
                    publication.Annotation);

                ViewBag.PublicationId = publication.Id;
                return View();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка получения публикации для редактирования");
                TempData["Error"] = ex.Message;
                return RedirectToAction("Publications");
            }
        }

        [HttpPost]
        public IActionResult UpdatePublication(
            int id,
            string title,
            string authors,
            int year,
            DateTime? publicationDate,
            int type,
            string? doi,
            string? url,
            int? journalId,
            int? conferenceId,
            string? keywords,
            string? annotation)
        {
            try
            {
                if (APIClient.Researcher == null)
                {
                    return RedirectToAction("Enter");
                }

                if (string.IsNullOrWhiteSpace(title) || string.IsNullOrWhiteSpace(authors))
                {
                    ViewBag.Error = "Заполните обязательные поля";
                    LoadPublicationDictionaries();
                    FillPublicationViewBag(title, authors, year, publicationDate, type, doi, url, journalId, conferenceId, keywords, annotation);
                    ViewBag.PublicationId = id;
                    return View();
                }

                var model = new PublicationBindingModel
                {
                    Id = id,
                    Title = title,
                    Authors = authors,
                    Year = year,
                    PublicationDate = publicationDate,
                    Type = (PublicationType)type,
                    Doi = string.IsNullOrWhiteSpace(doi) ? null : doi,
                    Url = string.IsNullOrWhiteSpace(url) ? null : url,
                    JournalId = journalId,
                    ConferenceId = conferenceId,
                    ResearcherId = APIClient.Researcher.Id,
                    Keywords = string.IsNullOrWhiteSpace(keywords) ? null : keywords,
                    Annotation = string.IsNullOrWhiteSpace(annotation) ? null : annotation
                };

                APIClient.PostRequest("api/Publication/UpdatePublication", model);

                TempData["Message"] = "Публикация обновлена";
                return RedirectToAction("Publications");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка обновления публикации");
                ViewBag.Error = ex.Message;
                LoadPublicationDictionaries();
                FillPublicationViewBag(title, authors, year, publicationDate, type, doi, url, journalId, conferenceId, keywords, annotation);
                ViewBag.PublicationId = id;
                return View();
            }
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        private void UpdateResearcherProfile()
        {
            if (APIClient.Researcher == null)
            {
                return;
            }

            var updatedResearcher = APIClient.GetRequest<ResearcherViewModel>(
                $"api/Researcher/GetResearcherById?id={APIClient.Researcher.Id}");

            if (updatedResearcher != null)
            {
                APIClient.Researcher = updatedResearcher;
            }
        }

        private void FillRegisterViewBag(
            string lastName,
            string firstName,
            string? middleName,
            string email,
            string phone,
            string department,
            string position,
            string? researchTopics,
            string? eLibraryAuthorId,
            int academicDegree)
        {
            ViewBag.LastName = lastName;
            ViewBag.FirstName = firstName;
            ViewBag.MiddleName = middleName;
            ViewBag.Email = email;
            ViewBag.Phone = phone;
            ViewBag.Department = department;
            ViewBag.Position = position;
            ViewBag.ResearchTopics = researchTopics;
            ViewBag.ELibraryAuthorId = eLibraryAuthorId;
            ViewBag.AcademicDegree = academicDegree;
        }

        private void LoadPublicationDictionaries()
        {
            ViewBag.Journals = APIClient.GetRequest<List<JournalViewModel>>("api/Journal/GetAllJournals") ?? new List<JournalViewModel>();
            ViewBag.Conferences = APIClient.GetRequest<List<ConferenceViewModel>>("api/Conference/GetAllConferences") ?? new List<ConferenceViewModel>();
        }

        private void FillPublicationViewBag(
            string title,
            string authors,
            int year,
            DateTime? publicationDate,
            int type,
            string? doi,
            string? url,
            int? journalId,
            int? conferenceId,
            string? keywords,
            string? annotation)
        {
            ViewBag.TitleValue = title;
            ViewBag.AuthorsValue = authors;
            ViewBag.YearValue = year;
            ViewBag.PublicationDateValue = publicationDate?.ToString("yyyy-MM-dd");
            ViewBag.TypeValue = type;
            ViewBag.DoiValue = doi;
            ViewBag.UrlValue = url;
            ViewBag.JournalIdValue = journalId;
            ViewBag.ConferenceIdValue = conferenceId;
            ViewBag.KeywordsValue = keywords;
            ViewBag.AnnotationValue = annotation;
        }
    }
}
