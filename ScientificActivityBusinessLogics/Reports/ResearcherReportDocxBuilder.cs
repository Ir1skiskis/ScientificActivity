using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;
using ScientificActivityContracts.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScientificActivityBusinessLogics.Reports
{
    public static class ResearcherReportDocxBuilder
    {
        public static byte[] Build(ResearcherReportViewModel report)
        {
            if (report == null)
            {
                throw new ArgumentNullException(nameof(report));
            }

            using var memoryStream = new MemoryStream();

            using (var document = WordprocessingDocument.Create(
                       memoryStream,
                       WordprocessingDocumentType.Document,
                       true))
            {
                var mainPart = document.AddMainDocumentPart();
                mainPart.Document = new Document();

                var body = new Body();

                AddPageSettings(body);

                AddTitle(body, report.Settings.ReportTitle);
                AddParagraph(
                    body,
                    $"Дата формирования отчета: {report.GeneratedAt:dd.MM.yyyy HH:mm}",
                    justification: JustificationValues.Center,
                    fontSize: "22");

                AddEmptyParagraph(body);

                if (report.Profile == null)
                {
                    AddParagraph(body, "Данные профиля исследователя не найдены.", bold: true);
                    mainPart.Document.Append(body);
                    mainPart.Document.Save();
                    return memoryStream.ToArray();
                }

                var profile = report.Profile;
                var sectionNumber = 1;

                if (report.Settings.IncludeCommonInfo)
                {
                    AddSectionTitle(body, sectionNumber++, "Основные сведения об исследователе");

                    AddKeyValueTable(body, new List<(string Name, string? Value)>
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

                    AddEmptyParagraph(body);
                }

                if (report.Settings.IncludePublicationSummary)
                {
                    AddSectionTitle(body, sectionNumber++, "Общие показатели публикационной активности");

                    AddKeyValueTable(body, new List<(string Name, string? Value)>
                    {
                        ("Число публикаций на eLibrary", Format(profile.PublicationsCountElibrary)),
                        ("Число публикаций в РИНЦ", Format(profile.PublicationsCountRinc)),
                        ("Число публикаций в ядре РИНЦ", Format(profile.PublicationsCoreRincCount)),
                        ("Год первой публикации", Format(profile.FirstPublicationYear))
                    });

                    AddEmptyParagraph(body);
                }

                if (report.Settings.IncludeCitationIndexes)
                {
                    AddSectionTitle(body, sectionNumber++, "Показатели цитирования и индексы");

                    AddKeyValueTable(body, new List<(string Name, string? Value)>
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

                    AddEmptyParagraph(body);
                }

                if (report.Settings.IncludeAdditionalMetrics)
                {
                    AddSectionTitle(body, sectionNumber++, "Дополнительные наукометрические показатели");

                    AddKeyValueTable(body, new List<(string Name, string? Value)>
                    {
                        ("Публикаций, процитировавших работы автора", Format(profile.PublicationsCitingAuthorCount)),
                        ("Ссылок на самую цитируемую публикацию", Format(profile.MostCitedPublicationCitationsCount)),
                        ("Публикаций автора, процитированных хотя бы один раз", Format(profile.CitedPublicationsCount)),
                        ("Число самоцитирований", Format(profile.SelfCitationsCount)),
                        ("Число цитирований соавторами", Format(profile.CoauthorCitationsCount)),
                        ("Число соавторов", Format(profile.CoauthorsCount)),
                        ("Процентиль по ядру РИНЦ", Format(profile.PercentileCoreRinc))
                    });

                    AddEmptyParagraph(body);
                }

                if (report.Settings.IncludeJournalMetrics)
                {
                    AddSectionTitle(body, sectionNumber++, "Показатели журнальной активности");

                    AddKeyValueTable(body, new List<(string Name, string? Value)>
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

                    AddEmptyParagraph(body);
                }

                if (report.Settings.IncludeLastFiveYearsMetrics)
                {
                    AddSectionTitle(body, sectionNumber++, "Показатели за последние 5 лет");

                    AddKeyValueTable(body, new List<(string Name, string? Value)>
                    {
                        ("Публикации в РИНЦ за 5 лет", Format(profile.PublicationsRincLast5YearsCount)),
                        ("Публикации в ядре РИНЦ за 5 лет", Format(profile.PublicationsCoreRincLast5YearsCount)),
                        ("Цитирования в РИНЦ за 5 лет", Format(profile.CitationsRincLast5YearsCount)),
                        ("Цитирования в ядре РИНЦ за 5 лет", Format(profile.CitationsCoreRincLast5YearsCount)),
                        ("Все ссылки на работы автора за 5 лет", Format(profile.CitationsAllLast5YearsCount))
                    });

                    AddEmptyParagraph(body);
                }

                if (report.Settings.IncludeYearDynamics)
                {
                    AddSectionTitle(body, sectionNumber++, "Динамика показателей по годам");

                    AddYearDynamicsTable(body, report);

                    AddEmptyParagraph(body);

                    AddSectionTitle(body, sectionNumber++, "Пятилетние показатели по годам окончания периода");

                    AddFiveYearsDynamicsTable(body, report);

                    AddEmptyParagraph(body);
                }

                if (report.Settings.IncludePublications)
                {
                    AddSectionTitle(body, sectionNumber++, "Список публикаций");

                    AddParagraph(
                        body,
                        $"Всего публикаций в отчете: {report.Publications.Count}",
                        bold: true);

                    AddPublicationsTable(body, report.Publications);

                    AddEmptyParagraph(body);
                }

                mainPart.Document.Append(body);
                mainPart.Document.Save();
            }

            return memoryStream.ToArray();
        }

        private static void AddPageSettings(Body body)
        {
            var sectionProperties = new SectionProperties(
                new PageSize
                {
                    Width = 11906,
                    Height = 16838
                },
                new PageMargin
                {
                    Top = 1134,
                    Right = 850,
                    Bottom = 1134,
                    Left = 850,
                    Header = 708,
                    Footer = 708,
                    Gutter = 0
                });

            body.Append(sectionProperties);
        }

        private static void AddTitle(Body body, string? title)
        {
            AddParagraph(
                body,
                string.IsNullOrWhiteSpace(title)
                    ? "Отчет о научной активности исследователя"
                    : title,
                bold: true,
                justification: JustificationValues.Center,
                fontSize: "32");
        }

        private static void AddSectionTitle(Body body, int number, string title)
        {
            AddParagraph(
                body,
                $"{number}. {title}",
                bold: true,
                fontSize: "26",
                spacingBefore: "240",
                spacingAfter: "120");
        }

        private static void AddParagraph(
            Body body,
            string? text,
            bool bold = false,
            JustificationValues? justification = null,
            string fontSize = "22",
            string spacingBefore = "0",
            string spacingAfter = "80")
        {
            var paragraph = new Paragraph();

            var paragraphProperties = new ParagraphProperties
            {
                SpacingBetweenLines = new SpacingBetweenLines
                {
                    Before = spacingBefore,
                    After = spacingAfter
                }
            };

            if (justification.HasValue)
            {
                paragraphProperties.Justification = new Justification
                {
                    Val = justification.Value
                };
            }

            paragraph.Append(paragraphProperties);

            var runProperties = new RunProperties
            {
                FontSize = new FontSize { Val = fontSize },
                RunFonts = new RunFonts
                {
                    Ascii = "Times New Roman",
                    HighAnsi = "Times New Roman",
                    EastAsia = "Times New Roman",
                    ComplexScript = "Times New Roman"
                }
            };

            if (bold)
            {
                runProperties.Bold = new Bold();
            }

            var run = new Run();
            run.Append(runProperties);
            run.Append(new Text(text ?? string.Empty)
            {
                Space = SpaceProcessingModeValues.Preserve
            });

            paragraph.Append(run);
            body.Append(paragraph);
        }

        private static void AddEmptyParagraph(Body body)
        {
            body.Append(new Paragraph(new Run(new Text(string.Empty))));
        }

        private static void AddKeyValueTable(Body body, List<(string Name, string? Value)> rows)
        {
            var table = CreateBaseTable();

            table.Append(CreateTableRow(
                new List<string> { "Показатель", "Значение" },
                isHeader: true));

            foreach (var row in rows)
            {
                table.Append(CreateTableRow(new List<string>
                {
                    row.Name,
                    string.IsNullOrWhiteSpace(row.Value) ? "Не указано" : row.Value
                }));
            }

            body.Append(table);
        }

        private static void AddYearDynamicsTable(Body body, ResearcherReportViewModel report)
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
                AddParagraph(body, "Годовые показатели не найдены.");
                return;
            }

            var table = CreateBaseTable();

            table.Append(CreateTableRow(new List<string>
            {
                "Год",
                "Публикации РИНЦ",
                "Публикации ядра РИНЦ",
                "Цитирования РИНЦ",
                "Цитирования ядра РИНЦ",
                "h-index РИНЦ",
                "h-index ядра",
                "Процентиль ядра"
            }, isHeader: true, fontSize: "18"));

            foreach (var year in years)
            {
                table.Append(CreateTableRow(new List<string>
                {
                    year.ToString(),
                    GetValue(report.PublicationsRincByYear, year),
                    GetValue(report.PublicationsCoreRincByYear, year),
                    GetValue(report.CitationsRincByYear, year),
                    GetValue(report.CitationsCoreRincByYear, year),
                    GetValue(report.HIndexRincByYear, year),
                    GetValue(report.HIndexCoreRincByYear, year),
                    GetValue(report.PercentileCoreRincByYear, year)
                }, fontSize: "18"));
            }

            body.Append(table);
        }

        private static void AddFiveYearsDynamicsTable(Body body, ResearcherReportViewModel report)
        {
            var years = report.PublicationsRinc5YearsByEndYear.Keys
                .Union(report.PublicationsCoreRinc5YearsByEndYear.Keys)
                .Union(report.CitationsRinc5YearsByEndYear.Keys)
                .Union(report.CitationsCoreRinc5YearsByEndYear.Keys)
                .OrderBy(x => x)
                .ToList();

            if (!years.Any())
            {
                AddParagraph(body, "Пятилетние показатели не найдены.");
                return;
            }

            var table = CreateBaseTable();

            table.Append(CreateTableRow(new List<string>
            {
                "Период",
                "Публикации РИНЦ за 5 лет",
                "Публикации ядра РИНЦ за 5 лет",
                "Цитирования РИНЦ за 5 лет",
                "Цитирования ядра РИНЦ за 5 лет"
            }, isHeader: true, fontSize: "18"));

            foreach (var endYear in years)
            {
                var startYear = endYear - 4;

                table.Append(CreateTableRow(new List<string>
                {
                    $"{startYear}-{endYear}",
                    GetValue(report.PublicationsRinc5YearsByEndYear, endYear),
                    GetValue(report.PublicationsCoreRinc5YearsByEndYear, endYear),
                    GetValue(report.CitationsRinc5YearsByEndYear, endYear),
                    GetValue(report.CitationsCoreRinc5YearsByEndYear, endYear)
                }, fontSize: "18"));
            }

            body.Append(table);
        }

        private static void AddPublicationsTable(Body body, List<PublicationViewModel> publications)
        {
            if (publications == null || !publications.Any())
            {
                AddParagraph(body, "Публикации по выбранным параметрам не найдены.");
                return;
            }

            var table = CreateBaseTable();

            table.Append(CreateTableRow(new List<string>
            {
                "№",
                "Год",
                "Публикация",
                "Источник",
                "DOI",
                "Цитирования РИНЦ"
            }, isHeader: true, fontSize: "18"));

            var number = 1;

            foreach (var publication in publications
                         .OrderByDescending(x => x.Year)
                         .ThenBy(x => x.Title))
            {
                var source = GetPublicationSource(publication);

                table.Append(CreateTableRow(new List<string>
                {
                    number.ToString(),
                    publication.Year.ToString(),
                    BuildPublicationDescription(publication),
                    source,
                    string.IsNullOrWhiteSpace(publication.Doi) ? "—" : publication.Doi,
                    (publication.CitationsRincCount ?? 0).ToString()
                }, fontSize: "18"));

                number++;
            }

            body.Append(table);
        }

        private static Table CreateBaseTable()
        {
            var table = new Table();

            var tableProperties = new TableProperties(
                new TableWidth
                {
                    Width = "5000",
                    Type = TableWidthUnitValues.Pct
                },
                new TableBorders(
                    new TopBorder
                    {
                        Val = BorderValues.Single,
                        Size = 4
                    },
                    new BottomBorder
                    {
                        Val = BorderValues.Single,
                        Size = 4
                    },
                    new LeftBorder
                    {
                        Val = BorderValues.Single,
                        Size = 4
                    },
                    new RightBorder
                    {
                        Val = BorderValues.Single,
                        Size = 4
                    },
                    new InsideHorizontalBorder
                    {
                        Val = BorderValues.Single,
                        Size = 4
                    },
                    new InsideVerticalBorder
                    {
                        Val = BorderValues.Single,
                        Size = 4
                    }),
                new TableCellMarginDefault(
                    new TopMargin
                    {
                        Width = "80",
                        Type = TableWidthUnitValues.Dxa
                    },
                    new BottomMargin
                    {
                        Width = "80",
                        Type = TableWidthUnitValues.Dxa
                    },
                    new TableCellLeftMargin
                    {
                        Width = 80,
                        Type = TableWidthValues.Dxa
                    },
                    new TableCellRightMargin
                    {
                        Width = 80,
                        Type = TableWidthValues.Dxa
                    })
            );

            table.Append(tableProperties);

            return table;
        }

        private static TableRow CreateTableRow(
            List<string> values,
            bool isHeader = false,
            string fontSize = "20")
        {
            var row = new TableRow();

            if (isHeader)
            {
                row.Append(new TableRowProperties(new TableHeader()));
            }

            foreach (var value in values)
            {
                row.Append(CreateTableCell(value, isHeader, fontSize));
            }

            return row;
        }

        private static TableCell CreateTableCell(
            string? text,
            bool isHeader = false,
            string fontSize = "20")
        {
            var cell = new TableCell();

            var cellProperties = new TableCellProperties();

            if (isHeader)
            {
                cellProperties.Shading = new Shading
                {
                    Fill = "D9EAF7"
                };
            }

            cell.Append(cellProperties);

            var paragraph = new Paragraph();

            var paragraphProperties = new ParagraphProperties
            {
                SpacingBetweenLines = new SpacingBetweenLines
                {
                    Before = "0",
                    After = "0"
                }
            };

            paragraph.Append(paragraphProperties);

            var runProperties = new RunProperties
            {
                FontSize = new FontSize { Val = fontSize },
                RunFonts = new RunFonts
                {
                    Ascii = "Times New Roman",
                    HighAnsi = "Times New Roman",
                    EastAsia = "Times New Roman",
                    ComplexScript = "Times New Roman"
                }
            };

            if (isHeader)
            {
                runProperties.Bold = new Bold();
            }

            var run = new Run();
            run.Append(runProperties);
            run.Append(new Text(text ?? string.Empty)
            {
                Space = SpaceProcessingModeValues.Preserve
            });

            paragraph.Append(run);
            cell.Append(paragraph);

            return cell;
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

            return string.Join(Environment.NewLine, parts);
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
