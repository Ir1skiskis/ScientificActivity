using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using ScientificActivityContracts.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScientificActivityBusinessLogics.Reports
{
    public static class ResearcherReportPdfBuilder
    {
        public static byte[] Build(ResearcherReportViewModel report)
        {
            if (report == null)
            {
                throw new ArgumentNullException(nameof(report));
            }

            var document = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(25);
                    page.DefaultTextStyle(x => x.FontSize(10).FontFamily("Arial"));

                    page.Header().Element(header =>
                    {
                        header.Column(column =>
                        {
                            column.Item()
                                .AlignCenter()
                                .Text(string.IsNullOrWhiteSpace(report.Settings.ReportTitle)
                                    ? "Отчет о научной активности исследователя"
                                    : report.Settings.ReportTitle)
                                .FontSize(16)
                                .Bold();

                            column.Item()
                                .PaddingTop(5)
                                .AlignCenter()
                                .Text($"Дата формирования: {report.GeneratedAt:dd.MM.yyyy HH:mm}")
                                .FontSize(9)
                                .FontColor(Colors.Grey.Darken1);
                        });
                    });

                    page.Content().PaddingVertical(15).Element(content =>
                    {
                        BuildContent(content, report);
                    });

                    page.Footer().AlignCenter().Text(text =>
                    {
                        text.Span("Страница ").FontSize(9);
                        text.CurrentPageNumber().FontSize(9);
                        text.Span(" из ").FontSize(9);
                        text.TotalPages().FontSize(9);
                    });
                });
            });

            return document.GeneratePdf();
        }

        private static void BuildContent(IContainer container, ResearcherReportViewModel report)
        {
            container.Column(column =>
            {
                column.Spacing(12);

                if (report.Profile == null)
                {
                    column.Item().Text("Данные профиля исследователя не найдены.").Bold();
                    return;
                }

                var profile = report.Profile;
                var sectionNumber = 1;

                if (report.Settings.IncludeCommonInfo)
                {
                    AddSectionTitle(column, sectionNumber++, "Основные сведения об исследователе");

                    AddKeyValueTable(column, new List<(string Name, string? Value)>
                    {
                        ("ФИО", profile.FullName),
                        ("Организация", profile.Organization),
                        ("Кафедра", profile.Department),
                        ("AuthorID eLibrary", profile.AuthorId),
                        ("SPIN-код", profile.SpinCode),
                        ("Основная рубрика ГРНТИ", profile.MainRubricGrnti),
                        ("Основная рубрика OECD", profile.MainRubricOecd),
                        ("Научные интересы", profile.ResearchTopics)
                    });
                }

                if (report.Settings.IncludePublicationSummary)
                {
                    AddSectionTitle(column, sectionNumber++, "Общие показатели публикационной активности");

                    AddKeyValueTable(column, new List<(string Name, string? Value)>
                    {
                        ("Число публикаций на eLibrary", Format(profile.PublicationsCountElibrary)),
                        ("Число публикаций в РИНЦ", Format(profile.PublicationsCountRinc)),
                        ("Число публикаций в ядре РИНЦ", Format(profile.PublicationsCoreRincCount)),
                        ("Год первой публикации", Format(profile.FirstPublicationYear))
                    });
                }

                if (report.Settings.IncludeCitationIndexes)
                {
                    AddSectionTitle(column, sectionNumber++, "Показатели цитирования и индексы");

                    AddKeyValueTable(column, new List<(string Name, string? Value)>
                    {
                        ("Число цитирований на eLibrary", Format(profile.CitationsCountElibrary)),
                        ("Число цитирований в РИНЦ", Format(profile.CitationsCountRinc)),
                        ("Число цитирований в ядре РИНЦ", Format(profile.CitationsCoreRincCount)),
                        ("Индекс Хирша по всем публикациям", Format(profile.HIndexElibrary)),
                        ("Индекс Хирша по РИНЦ", Format(profile.HIndexRinc)),
                        ("Индекс Хирша по ядру РИНЦ", Format(profile.HIndexCoreRinc)),
                        ("Индекс Хирша без самоцитирований", Format(profile.HIndexWithoutSelfCitations)),
                        ("Среднее число цитирований на одну публикацию", Format(profile.AverageCitationsPerPublication))
                    });
                }

                if (report.Settings.IncludeAdditionalMetrics)
                {
                    AddSectionTitle(column, sectionNumber++, "Дополнительные наукометрические показатели");

                    AddKeyValueTable(column, new List<(string Name, string? Value)>
                    {
                        ("Публикаций, процитировавших работы автора", Format(profile.PublicationsCitingAuthorCount)),
                        ("Ссылок на самую цитируемую публикацию", Format(profile.MostCitedPublicationCitationsCount)),
                        ("Публикаций автора, процитированных хотя бы один раз", Format(profile.CitedPublicationsCount)),
                        ("Число самоцитирований", Format(profile.SelfCitationsCount)),
                        ("Число цитирований соавторами", Format(profile.CoauthorCitationsCount)),
                        ("Число соавторов", Format(profile.CoauthorsCount)),
                        ("Процентиль по ядру РИНЦ", Format(profile.PercentileCoreRinc))
                    });
                }

                if (report.Settings.IncludeJournalMetrics)
                {
                    AddSectionTitle(column, sectionNumber++, "Показатели журнальной активности");

                    AddKeyValueTable(column, new List<(string Name, string? Value)>
                    {
                        ("Статьи в зарубежных журналах", Format(profile.ForeignArticlesCount)),
                        ("Статьи в российских журналах", Format(profile.RussianArticlesCount)),
                        ("Статьи в журналах ВАК", Format(profile.VakArticlesCount)),
                        ("Статьи в журналах с ненулевым импакт-фактором", Format(profile.ImpactFactorArticlesCount)),
                        ("Цитирования из зарубежных журналов", Format(profile.ForeignJournalCitationsCount)),
                        ("Цитирования из российских журналов", Format(profile.RussianJournalCitationsCount)),
                        ("Цитирования из журналов ВАК", Format(profile.VakJournalCitationsCount)),
                        ("Цитирования из журналов с ненулевым импакт-фактором", Format(profile.ImpactFactorJournalCitationsCount)),
                        ("Средневзвешенный импакт-фактор журналов, в которых опубликованы статьи", Format(profile.AverageWeightedImpactFactorPublished)),
                        ("Средневзвешенный импакт-фактор журналов, в которых процитированы статьи", Format(profile.AverageWeightedImpactFactorCited))
                    });
                }

                if (report.Settings.IncludeLastFiveYearsMetrics)
                {
                    AddSectionTitle(column, sectionNumber++, "Показатели за последние 5 лет");

                    AddKeyValueTable(column, new List<(string Name, string? Value)>
                    {
                        ("Публикации в РИНЦ за 5 лет", Format(profile.PublicationsRincLast5YearsCount)),
                        ("Публикации в ядре РИНЦ за 5 лет", Format(profile.PublicationsCoreRincLast5YearsCount)),
                        ("Цитирования в РИНЦ за 5 лет", Format(profile.CitationsRincLast5YearsCount)),
                        ("Цитирования в ядре РИНЦ за 5 лет", Format(profile.CitationsCoreRincLast5YearsCount)),
                        ("Все ссылки на работы автора за 5 лет", Format(profile.CitationsAllLast5YearsCount))
                    });
                }

                if (report.Settings.IncludeYearDynamics)
                {
                    AddSectionTitle(column, sectionNumber++, "Динамика показателей по годам");
                    AddYearDynamicsTable(column, report);

                    AddSectionTitle(column, sectionNumber++, "Пятилетние показатели по годам окончания периода");
                    AddFiveYearsDynamicsTable(column, report);
                }

                if (report.Settings.IncludePublications)
                {
                    AddSectionTitle(column, sectionNumber++, "Список публикаций");

                    column.Item()
                        .Text($"Всего публикаций в отчете: {report.Publications.Count}")
                        .Bold();

                    AddPublicationsTable(column, report.Publications);
                }
            });
        }

        private static void AddSectionTitle(ColumnDescriptor column, int number, string title)
        {
            column.Item()
                .PaddingTop(8)
                .PaddingBottom(4)
                .Text($"{number}. {title}")
                .FontSize(13)
                .Bold();
        }

        private static void AddKeyValueTable(ColumnDescriptor column, List<(string Name, string? Value)> rows)
        {
            column.Item().Table(table =>
            {
                table.ColumnsDefinition(columns =>
                {
                    columns.RelativeColumn(2);
                    columns.RelativeColumn(3);
                });

                AddHeaderCell(table, "Показатель");
                AddHeaderCell(table, "Значение");

                foreach (var row in rows)
                {
                    AddCell(table, row.Name);
                    AddCell(table, string.IsNullOrWhiteSpace(row.Value) ? "Не указано" : row.Value);
                }
            });
        }

        private static void AddYearDynamicsTable(ColumnDescriptor column, ResearcherReportViewModel report)
        {
            var years = report.PublicationsRincByYear.Keys
                .Union(report.PublicationsCoreRincByYear.Keys)
                .Union(report.CitationsRincByYear.Keys)
                .Union(report.CitationsCoreRincByYear.Keys)
                .Union(report.HIndexRincByYear.Keys)
                .Union(report.HIndexCoreRincByYear.Keys)
                .Union(report.PercentileCoreRincByYear.Keys)
                .OrderBy(x => x)
                .ToList();

            if (!years.Any())
            {
                column.Item().Text("Годовые показатели не найдены.");
                return;
            }

            column.Item().Table(table =>
            {
                table.ColumnsDefinition(columns =>
                {
                    columns.RelativeColumn(0.7f);
                    columns.RelativeColumn();
                    columns.RelativeColumn();
                    columns.RelativeColumn();
                    columns.RelativeColumn();
                    columns.RelativeColumn();
                    columns.RelativeColumn();
                    columns.RelativeColumn();
                });

                AddHeaderCell(table, "Год", 7);
                AddHeaderCell(table, "Публ. РИНЦ", 7);
                AddHeaderCell(table, "Публ. ядра", 7);
                AddHeaderCell(table, "Цит. РИНЦ", 7);
                AddHeaderCell(table, "Цит. ядра", 7);
                AddHeaderCell(table, "h РИНЦ", 7);
                AddHeaderCell(table, "h ядра", 7);
                AddHeaderCell(table, "Перцентиль", 7);

                foreach (var year in years)
                {
                    AddCell(table, year.ToString(), 7);
                    AddCell(table, GetValue(report.PublicationsRincByYear, year), 7);
                    AddCell(table, GetValue(report.PublicationsCoreRincByYear, year), 7);
                    AddCell(table, GetValue(report.CitationsRincByYear, year), 7);
                    AddCell(table, GetValue(report.CitationsCoreRincByYear, year), 7);
                    AddCell(table, GetValue(report.HIndexRincByYear, year), 7);
                    AddCell(table, GetValue(report.HIndexCoreRincByYear, year), 7);
                    AddCell(table, GetValue(report.PercentileCoreRincByYear, year), 7);
                }
            });
        }

        private static void AddFiveYearsDynamicsTable(ColumnDescriptor column, ResearcherReportViewModel report)
        {
            var years = report.PublicationsRinc5YearsByEndYear.Keys
                .Union(report.PublicationsCoreRinc5YearsByEndYear.Keys)
                .Union(report.CitationsRinc5YearsByEndYear.Keys)
                .Union(report.CitationsCoreRinc5YearsByEndYear.Keys)
                .OrderBy(x => x)
                .ToList();

            if (!years.Any())
            {
                column.Item().Text("Пятилетние показатели не найдены.");
                return;
            }

            column.Item().Table(table =>
            {
                table.ColumnsDefinition(columns =>
                {
                    columns.RelativeColumn();
                    columns.RelativeColumn();
                    columns.RelativeColumn();
                    columns.RelativeColumn();
                    columns.RelativeColumn();
                });

                AddHeaderCell(table, "Период", 8);
                AddHeaderCell(table, "Публ. РИНЦ", 8);
                AddHeaderCell(table, "Публ. ядра", 8);
                AddHeaderCell(table, "Цит. РИНЦ", 8);
                AddHeaderCell(table, "Цит. ядра", 8);

                foreach (var endYear in years)
                {
                    var startYear = endYear - 4;

                    AddCell(table, $"{startYear}-{endYear}", 8);
                    AddCell(table, GetValue(report.PublicationsRinc5YearsByEndYear, endYear), 8);
                    AddCell(table, GetValue(report.PublicationsCoreRinc5YearsByEndYear, endYear), 8);
                    AddCell(table, GetValue(report.CitationsRinc5YearsByEndYear, endYear), 8);
                    AddCell(table, GetValue(report.CitationsCoreRinc5YearsByEndYear, endYear), 8);
                }
            });
        }

        private static void AddPublicationsTable(ColumnDescriptor column, List<PublicationViewModel> publications)
        {
            if (publications == null || !publications.Any())
            {
                column.Item().Text("Публикации по выбранным параметрам не найдены.");
                return;
            }

            column.Item().Table(table =>
            {
                table.ColumnsDefinition(columns =>
                {
                    columns.ConstantColumn(22);
                    columns.ConstantColumn(35);
                    columns.RelativeColumn(4);
                    columns.RelativeColumn(2);
                    columns.RelativeColumn(1.5f);
                    columns.ConstantColumn(45);
                });

                AddHeaderCell(table, "№", 7);
                AddHeaderCell(table, "Год", 7);
                AddHeaderCell(table, "Публикация", 7);
                AddHeaderCell(table, "Источник", 7);
                AddHeaderCell(table, "DOI", 7);
                AddHeaderCell(table, "Цит.", 7);

                var number = 1;

                foreach (var publication in publications
                             .OrderByDescending(x => x.Year)
                             .ThenBy(x => x.Title))
                {
                    AddCell(table, number.ToString(), 7);
                    AddCell(table, publication.Year.ToString(), 7);
                    AddCell(table, BuildPublicationDescription(publication), 7);
                    AddCell(table, GetPublicationSource(publication), 7);
                    AddCell(table, string.IsNullOrWhiteSpace(publication.Doi) ? "—" : publication.Doi, 7);
                    AddCell(table, (publication.CitationsRincCount ?? 0).ToString(), 7);

                    number++;
                }
            });
        }

        private static void AddHeaderCell(TableDescriptor table, string text, int fontSize = 9)
        {
            table.Cell()
                .Border(0.5f)
                .BorderColor(Colors.Grey.Medium)
                .Background(Colors.Blue.Lighten4)
                .Padding(3)
                .Text(text)
                .FontSize(fontSize)
                .Bold();
        }

        private static void AddCell(TableDescriptor table, string? text, int fontSize = 9)
        {
            table.Cell()
                .Border(0.5f)
                .BorderColor(Colors.Grey.Lighten1)
                .Padding(3)
                .Text(string.IsNullOrWhiteSpace(text) ? "—" : text)
                .FontSize(fontSize);
        }

        private static string BuildPublicationDescription(PublicationViewModel publication)
        {
            var parts = new List<string>();

            if (!string.IsNullOrWhiteSpace(publication.Title))
            {
                parts.Add(publication.Title);
            }

            if (!string.IsNullOrWhiteSpace(publication.Authors))
            {
                parts.Add($"Авторы: {publication.Authors}");
            }

            var flags = new List<string>();

            if (publication.IsInRinc)
            {
                flags.Add("РИНЦ");
            }

            if (publication.IsInCoreRinc)
            {
                flags.Add("ядро РИНЦ");
            }

            if (publication.IsVak)
            {
                flags.Add("ВАК");
            }

            if (publication.IsRsci)
            {
                flags.Add("RSCI");
            }

            if (flags.Any())
            {
                parts.Add($"Признаки: {string.Join(", ", flags)}");
            }

            return string.Join("\n", parts);
        }

        private static string GetPublicationSource(PublicationViewModel publication)
        {
            if (!string.IsNullOrWhiteSpace(publication.JournalTitle))
            {
                return publication.JournalTitle;
            }

            if (!string.IsNullOrWhiteSpace(publication.ConferenceTitle))
            {
                return publication.ConferenceTitle;
            }

            if (!string.IsNullOrWhiteSpace(publication.Url))
            {
                return publication.Url;
            }

            return "—";
        }

        private static string GetValue(Dictionary<int, int>? dictionary, int key)
        {
            if (dictionary == null)
            {
                return "0";
            }

            return dictionary.TryGetValue(key, out var value)
                ? value.ToString()
                : "0";
        }

        private static string Format(int? value)
        {
            return value.HasValue ? value.Value.ToString() : "Не указано";
        }

        private static string Format(decimal? value)
        {
            return value.HasValue ? value.Value.ToString("0.##") : "Не указано";
        }
    }
}
